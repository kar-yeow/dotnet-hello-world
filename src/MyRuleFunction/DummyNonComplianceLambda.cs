using Amazon.ConfigService;
using Amazon.Lambda.ConfigEvents;
using Amazon.Lambda.Core;
using System.Text.Json;

namespace MyRuleFunction
{
    public class DummyNonComplianceLambda : AbstractCustomRuleFunction
    {
        protected override bool IsCompliance(ConfigEvent e, InvokeEvent ie, ILambdaContext c)
        {
            return false;
        }
    }
}