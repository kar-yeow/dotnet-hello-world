using Amazon.ConfigService.Model;
using Amazon.ConfigService;
using Amazon.Lambda.Core;
using Amazon.Lambda.ConfigEvents;
using System.Text.Json;

namespace MyRuleFunction
{
    public class InvokeEvent
    {
        public ConfigurationItem? ConfigurationItem { get; set; }
        public string? MessageType { get; set; }
    }

    public abstract class AbstractBaseLambda
    {
        private AmazonConfigServiceClient _config = new AmazonConfigServiceClient();

	    protected AbstractBaseLambda()
        {
        }

        public async Task HandleRequest(ConfigEvent e, ILambdaContext c)
        { 
            InvokeEvent ie = GetInvokeEvent(e);
            if (ie.ConfigurationItem == null ||  ie.MessageType != MessageType.ConfigurationItemChangeNotification.Value) 
            {
                throw new Exception($"Events with the message type {ie.MessageType} are not evaluated for this Config rule.");
            }
            ComplianceType result = EvaluateCompliance(e, ie, c);
            Evaluation evaluation = CreateEvaluation(e, result);
            await RegisterEvaluationResult(evaluation, e.ResultToken);
        }

        protected InvokeEvent GetInvokeEvent(ConfigEvent e)
        {
            var ie = JsonSerializer.Deserialize<InvokeEvent>(e.InvokingEvent);
            if (ie == null)
            {
                throw new Exception($"Deserialising the InvokingEvent returned a failed. {e.InvokingEvent}");
            }

            return ie;
        }

        protected Evaluation CreateEvaluation(ConfigEvent e, ComplianceType result)
        {
            var ie = GetInvokeEvent(e);
            return new Evaluation
            {
                ComplianceResourceId = ie.ConfigurationItem?.ResourceId,
                ComplianceResourceType = ie.ConfigurationItem?.ResourceType,
                OrderingTimestamp = ie.ConfigurationItem?.ConfigurationItemCaptureTime ?? DateTime.Now,
                ComplianceType = result,
                Annotation = e.ConfigRuleName
            };
        }

        protected async Task RegisterEvaluationResult(Evaluation evaluation, string resultToken)
        {

            PutEvaluationsRequest request = new PutEvaluationsRequest
            {
                Evaluations = { evaluation },
                ResultToken = resultToken
            };

            PutEvaluationsResponse response = await _config.PutEvaluationsAsync(request);

            // Ends the function execution if any evaluation results are not successfully reported.
            if (response.FailedEvaluations.Any())
            {
                // todo: This needs to log as much information as possible. The exception should probably be converted to a checked exception to be handled at the top of the stack, since the lambda failed due to external dependencies.
                throw new Exception($"The following evaluations were not successfully reported to AWS Config: {response.FailedEvaluations}");
            }
        }

        protected abstract ComplianceType EvaluateCompliance(ConfigEvent e, InvokeEvent ie, ILambdaContext c);
	    /* Example code:
	    {
		    if (isEventNotApplicable())
		    {
			    return ComplianceType.NOT_APPLICABLE;
		    }
		    else if (isCompliant())
		    {
			    return ComplianceType.COMPLIANT;
		    }
		    else
		    {
			    return ComplianceType.NON_COMPLIANT;
		    }
	    }*/

    }
}
