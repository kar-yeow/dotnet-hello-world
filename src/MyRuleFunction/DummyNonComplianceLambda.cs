using Amazon.ConfigService;
using Amazon.Lambda.ConfigEvents;
using Amazon.Lambda.Core;
using System.Text.Json;

namespace MyRuleFunction
{
    public class DummyNonComplianceLambda : AbstractBaseLambda
    {
        protected override ComplianceType EvaluateCompliance(ConfigEvent e, InvokeEvent ie, ILambdaContext c)
        {
            c.Logger.Log($"msg {ie.MessageType} {ie.ConfigurationItem?.ResourceId} {ie.ConfigurationItem?.ResourceName}");
            return ComplianceType.NON_COMPLIANT;
        }
    }
}