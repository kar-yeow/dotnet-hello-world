using Amazon.CDK;
using Amazon.CDK.AWS.Config;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.S3;
using Constructs;

namespace MyCdk
{
    public class MyCheckingStack : Stack
    {
        public MyCheckingStack(Construct scope, Amazon.CDK.AWS.EC2.ISubnet[] subnetToMonitor, string id, IStackProps? stackProps) : base(scope, id, stackProps)
        {
            if (!subnetToMonitor.Any())
            {
                throw new Exception("subnetToMonitor is zero");
            }

            var bucket = Bucket.FromBucketName(this, "MyBucket", "dass-hello-bucket");

            //var func = Function.FromFunctionArn(this, "MyFunction", "arn:aws:lambda:ap-southeast-2:037690295447:function:dass-hello-test");

            var func = new Function(this, "MyVerifyIsolatedSubnetFunction", new FunctionProps
            {
                FunctionName = "dass-rule-function-test",
                Runtime = Runtime.DOTNET_6,
                Handler = "MyRuleFunction::MyRuleFunction.DummyNonComplianceLambda::HandleRequest",
                Code = Code.FromBucket(bucket, "my-rule-function.zip"),
                //LogRetention = RetentionDays.THREE_DAYS,
                Timeout = Duration.Seconds(30),
                Role = Role.FromRoleName(this, "MyFunctionRole", "ato-role-baseline-lambda-exec")
            });

            for (int i = 0; i < subnetToMonitor.Length; i++)
            {
                string ruleName = $"dass-verify-isolated-subnet-compliance-rule-{i+1}"; // Add 1 so we aren't using a zero based index for naming.
                _ = new CustomRule(this, ruleName, new CustomRuleProps
                {
                    ConfigRuleName = ruleName,
                    LambdaFunction = func,
                    RuleScope = RuleScope.FromResource(ResourceType.EC2_SUBNET, subnetToMonitor[i].SubnetId),
                    ConfigurationChanges = true,
                    Periodic = false,
                    Description = $"{ruleName} to test custom rule"
                });
            }
            Tags.SetTag("compliance-checked", "true");
        }
    }
}
