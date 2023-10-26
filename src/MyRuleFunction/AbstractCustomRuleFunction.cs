using Amazon.ConfigService.Model;
using Amazon.ConfigService;
using Amazon.Lambda.Core;
using Amazon.Lambda.ConfigEvents;
using Newtonsoft.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace MyRuleFunction
{
    public class InvokeEvent
    {
        public ConfigurationItem? ConfigurationItem { get; set; }
        public string? MessageType { get; set; }
    }

    public abstract class AbstractCustomRuleFunction
    {
        private AmazonConfigServiceClient _config = new AmazonConfigServiceClient();

	    protected AbstractCustomRuleFunction()
        {
        }

        public async Task HandleRequest(ConfigEvent e, ILambdaContext c)
        {
            c.Logger.LogInformation($"Handle request called for {e.ConfigRuleName}");
            InvokeEvent ie = GetInvokeEvent(e);
            c.Logger.LogInformation($"my lambda function called {ie.MessageType} {ie.ConfigurationItem?.ResourceName} ");
            if (ie.ConfigurationItem == null ||  ie.MessageType != MessageType.ConfigurationItemChangeNotification.Value) 
            {
                throw new Exception($"Events with the message type {ie.MessageType} are not evaluated for this Config rule.");
            }
            ComplianceType result = EvaluateCompliance(e, ie, c);

            await RegisterEvaluationResult(e, ie.ConfigurationItem, result);
        }

        protected InvokeEvent GetInvokeEvent(ConfigEvent e)
        {
            var ie = JsonConvert.DeserializeObject<InvokeEvent>(e.InvokingEvent);
            if (ie == null)
            {
                throw new Exception($"Deserialize the InvokingEvent returned a failed. {e.InvokingEvent}");
            }
            return ie;
        }

        protected async Task RegisterEvaluationResult(ConfigEvent e, ConfigurationItem ci, ComplianceType result)
        {
            Evaluation evaluation = new Evaluation
            {
                ComplianceResourceId = ci.ResourceId,
                ComplianceResourceType = ci.ResourceType,
                OrderingTimestamp = ci.ConfigurationItemCaptureTime,
                ComplianceType = result,
                Annotation = e.ConfigRuleName
            };

            PutEvaluationsRequest request = new PutEvaluationsRequest
            {
                Evaluations = { evaluation },
                ResultToken = e.ResultToken
            };

            PutEvaluationsResponse response = await _config.PutEvaluationsAsync(request);

            // Ends the function execution if any evaluation results are not successfully reported.
            if (response.FailedEvaluations.Any())
            {
                // todo: This needs to log as much information as possible. The exception should probably be converted to a checked exception to be handled at the top of the stack, since the lambda failed due to external dependencies.
                throw new Exception($"The following evaluations were not successfully reported to AWS Config: {response.FailedEvaluations}");
            }
        }

        protected bool IsNotApplicable(ConfigEvent e, ConfigurationItem? ci)
        {
            var status = ci?.ConfigurationItemStatus;
            return e.EventLeftScope || status == ConfigurationItemStatus.ResourceDeleted 
                    || status == ConfigurationItemStatus.ResourceNotRecorded 
                    || status == ConfigurationItemStatus.ResourceDeletedNotRecorded;
        }

        protected abstract bool IsCompliance(ConfigEvent e, InvokeEvent ie, ILambdaContext c);

        protected ComplianceType EvaluateCompliance(ConfigEvent e, InvokeEvent ie, ILambdaContext c)
	    {
            var result = ComplianceType.NON_COMPLIANT;
            c.Logger.Log($"msg={ie.MessageType} resource id={ie.ConfigurationItem?.ResourceId} resource name={ie.ConfigurationItem?.ResourceName}");
            if (IsNotApplicable(e, ie.ConfigurationItem))
		    {
			    result = ComplianceType.NOT_APPLICABLE;
		    }
		    else if (IsCompliance(e, ie, c))
		    {
			    result = ComplianceType.COMPLIANT;
		    }
            c.Logger.Log($"{e.ConfigRuleName} compliance result={result}");
            return result;
	    }
    }
}
