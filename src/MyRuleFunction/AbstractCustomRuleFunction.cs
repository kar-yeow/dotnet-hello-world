using Amazon.ConfigService.Model;
using Amazon.ConfigService;
using Amazon.Lambda.Core;
using Amazon.Lambda.ConfigEvents;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace MyRuleFunction
{
    public class InvokeEvent
    {
        public JObject? ConfigurationItem { get; set; }
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
            Console.WriteLine($"Handle request {e.ConfigRuleName} {e.InvokingEvent}");
            InvokeEvent ie = GetInvokeEvent(e);
            if (ie.ConfigurationItem != null && ie.MessageType == MessageType.ConfigurationItemChangeNotification.Value)
            {
                ComplianceType result = EvaluateCompliance(e, ie, c);

                await RegisterEvaluationResult(e, ie.ConfigurationItem, result);
            }
        }

        protected InvokeEvent GetInvokeEvent(ConfigEvent e)
        {
            InvokeEvent? ie = JsonConvert.DeserializeObject<InvokeEvent>(e.InvokingEvent);
            if (ie == null)
            {
                throw new Exception($"Deserialize the InvokingEvent returned a null. {e.InvokingEvent}");
            }
            return ie;
        }

        protected virtual async Task<PutEvaluationsResponse> PutEvaluationsAsync(PutEvaluationsRequest request)
        {
            return await _config.PutEvaluationsAsync(request);
        }

        protected async Task RegisterEvaluationResult(ConfigEvent e, JObject ci, ComplianceType result)
        {
            Evaluation evaluation = new Evaluation
            {
                ComplianceResourceId = ci["resourceId"]?.Value<string>(),
                ComplianceResourceType = ci["resourceType"]?.Value<string>(),
                OrderingTimestamp = ci["configurationItemCaptureTime"]?.Value<DateTime>() ?? DateTime.Now,
                ComplianceType = result,
                Annotation = e.ConfigRuleName
            };

            PutEvaluationsResponse response = await PutEvaluationsAsync(new PutEvaluationsRequest
            {
                Evaluations = { evaluation },
                ResultToken = e.ResultToken
            });

            // Ends the function execution if any evaluation results are not successfully reported.
            if (response.FailedEvaluations.Any())
            {
                // todo: This needs to log as much information as possible. The exception should probably be converted to a checked exception to be handled at the top of the stack, since the lambda failed due to external dependencies.
                throw new Exception($"The following evaluations were not successfully reported to AWS Config: {response.FailedEvaluations}");
            }
        }

        protected bool IsNotApplicable(ConfigEvent e, JObject ci)
        {
            var token = ci["configurationItemStatus"];
            var status = token == null ? null : ConfigurationItemStatus.FindValue(token.Value<string>());
            return e.EventLeftScope || status == ConfigurationItemStatus.ResourceDeleted
                    || status == ConfigurationItemStatus.ResourceNotRecorded
                    || status == ConfigurationItemStatus.ResourceDeletedNotRecorded;
        }

        protected abstract bool IsCompliance(ConfigEvent e, InvokeEvent ie, ILambdaContext c);

        protected ComplianceType EvaluateCompliance(ConfigEvent e, InvokeEvent ie, ILambdaContext c)
	    {
            var result = ComplianceType.NON_COMPLIANT;
            if (ie.ConfigurationItem == null || IsNotApplicable(e, ie.ConfigurationItem))
		    {
			    result = ComplianceType.NOT_APPLICABLE;
		    }
		    else if (IsCompliance(e, ie, c))
		    {
			    result = ComplianceType.COMPLIANT;
		    }
            Console.WriteLine($"{e.ConfigRuleName} compliance result={result}");
            return result;
	    }
    }
}
