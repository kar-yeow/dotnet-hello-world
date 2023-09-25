using Amazon.CDK;
using Amazon.CDK.AWS.Config;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.S3;
using Constructs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCdk
{
    public class MyCheckingStack : Stack
    {
        public MyCheckingStack(Construct scope, Amazon.CDK.AWS.EC2.ISubnet[] subnetToMonitor) : base(scope, "MyCheckingStack", null)
        {
            if (!subnetToMonitor.Any())
            {
                throw new Exception("subnetToMonitor is zero");
            }

            var bucket = Bucket.FromBucketName(this, "MyBucket", "dass-hello-bucket");

            FunctionProps props = new FunctionProps
            {
                Runtime = Runtime.DOTNET_6,
                Handler = "MyRuleFunction.DummyNonComplianceLambda",
                Code = Code.FromBucket(bucket, "my-rule-function.zip"),
                LogRetention = RetentionDays.TWO_WEEKS,
                Timeout = Duration.Seconds(30)
            };

            for (int i = 0; i < subnetToMonitor.Length; i++)
            {
                IFunction fn = new Function(this, $"verify-isolated-subnet-function-{i+1}", props);
                string ruleName = $"verify-isolated-subnet-compliance-rule-{i+1}"; // Add 1 so we aren't using a zero based index for naming.
                RuleScope ruleScope = RuleScope.FromResource(ResourceType.EC2_SUBNET, subnetToMonitor[i].SubnetId);
                _ = new CustomRule(this, ruleName, new CustomRuleProps
                {
                    ConfigRuleName = ruleName,
                    LambdaFunction = fn,
                    RuleScope = ruleScope,
                    ConfigurationChanges = true
                });
                Tags.SetTag("compliance-checked", "true");
            }
        }
    }
}
