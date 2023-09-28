using Amazon.CDK;
using Amazon.CDK.AWS.Config;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.S3;
using Constructs;

namespace MyCdk
{
    public class MyCheckingStack : Stack
    {
        public MyCheckingStack(Construct scope, SecurityGroup sg, string id, IStackProps? stackProps) : base(scope, id, stackProps)
        {
            //if (!subnetToMonitor.Any())
            //{
            //    throw new Exception("subnetToMonitor is zero");
            //}

            var bucket = Bucket.FromBucketName(this, "MyBucket", "ato-dass-hello-bucket");

            //var func = Function.FromFunctionArn(this, "MyFunction", "arn:aws:lambda:ap-southeast-2:037690295447:function:dass-hello-test");

            var func = new Function(this, "MyVerifyIsolatedSubnetFunction", new FunctionProps
            {
                FunctionName = "dass-rule-function-test",
                Runtime = Runtime.DOTNET_6,
                Handler = "MyRuleFunction::MyRuleFunction.DummyNonComplianceLambda::HandleRequest",
                Code = Code.FromBucket(bucket, "my-rule-function.zip"),
                //LogRetention = RetentionDays.THREE_DAYS,
                Timeout = Duration.Seconds(30),
                Role = Role.FromRoleName(this, "MyFunctionRole", "ato-role-baseline-lambda-exec", new FromRoleNameOptions
                {
                    Mutable = false,
                    AddGrantsToResources = false
                })
            });

            //var rule = new CustomRule(this, "MyCustomRule", new CustomRuleProps
            //{
            //    ConfigRuleName = "dass-sg-rule",
            //    LambdaFunction = func,
            //    RuleScope = Amazon.CDK.AWS.Config.RuleScope.FromResource(ResourceType.EC2_SECURITY_GROUP, sg.SecurityGroupId),
            //    ConfigurationChanges = true,
            //    Periodic = false,
            //    Description = $"dass dummy rule to test custom security group"
            //});

            //for (int i = 0; i < subnetToMonitor.Length; i++)
            //{
            //    string ruleName = $"dass-verify-isolated-subnet-compliance-rule-{i+1}"; // Add 1 so we aren't using a zero based index for naming.
            //    _ = new CustomRule(this, ruleName, new CustomRuleProps
            //    {
            //        ConfigRuleName = ruleName,
            //        LambdaFunction = func,
            //        RuleScope = RuleScope.FromResource(ResourceType.EC2_SUBNET, subnetToMonitor[i].SubnetId),
            //        ConfigurationChanges = true,
            //        Periodic = false,
            //        Description = $"{ruleName} to test custom rule"
            //    });
            //}
            Tags.SetTag("compliance-checked", "true");

            _ = new CfnOutput(this, "MyRuleOutput", new CfnOutputProps
            {
                //Value = $"rule={rule.ConfigRuleId} {rule.ConfigRuleArn}"
                Value = $"function= {func.FunctionArn} {func.FunctionName}"
            });
        }
    }
}
