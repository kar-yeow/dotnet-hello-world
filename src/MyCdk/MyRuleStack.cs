using Amazon.CDK;
using Amazon.CDK.AWS.CodeBuild;
using Amazon.CDK.AWS.Config;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.S3;
using Constructs;

namespace MyCdk
{
    public class MyRuleStack : Stack
    {
        public MyRuleStack(Construct scope, SecurityGroup sg, string id, IStackProps? stackProps) : base(scope, id, stackProps)
        {
            //if (!subnetToMonitor.Any())
            //{
            //    throw new Exception("subnetToMonitor is zero");
            //}

            var bucket = Bucket.FromBucketName(this, "MyBucket", "ato-dass-hello-bucket")
                    ?? new Bucket(this, "MyBucket", new BucketProps
                    {
                        BucketName = "ato-dass-hello-bucket"
                    });

            var buildFunction = new Project(this, "MyRuleFunctionZip", new ProjectProps
            {
                Role = Role.FromRoleArn(this, "MyCodeBuildRole", $"arn:aws:iam::{Account}:role/ato-role-dass-codebuild-service", new FromRoleArnOptions
                {
                    Mutable = false,
                    AddGrantsToResources = false
                }),
                BuildSpec = BuildSpec.FromSourceFilename("src/lambda-buildspec.yml"),
                ProjectName = "dass-build-rule-lambda-function",
                Environment = new BuildEnvironment
                {
                    ComputeType = ComputeType.SMALL,
                    BuildImage = LinuxBuildImage.AMAZON_LINUX_2_5,
                    Privileged = false
                },
                Logging = new LoggingOptions
                {
                    CloudWatch = new CloudWatchLoggingOptions
                    {
                        Enabled = true,
                        LogGroup = new LogGroup(this, "MyLogGroup", new LogGroupProps
                        {
                            LogGroupName = "dass-hello-codebuild"
                        }),
                        Prefix = "dass-hello-codebuild"
                    }
                },
                Source = Source.GitHub(new GitHubSourceProps
                {
                    Owner = "kar-yeow",
                    Repo = "dotnet-hello-world",
                    BranchOrRef = "add-cdk-test",
                    Webhook = false
                }),
                Artifacts = Artifacts.S3(new S3ArtifactsProps
                {
                    Bucket = bucket,
                    PackageZip = true,
                    Name = "my-codebuild-rule-function.zip"
                })
            });

            var lambdaFunction = new Function(this, "MyCodeBuildRuleFunction", new FunctionProps
            {
                FunctionName = "dass-codebuild-rule-function",
                Description = "Dummy rule function to return non compliance status",
                Runtime = Runtime.DOTNET_6,
                Handler = "MyRuleFunction::MyRuleFunction.DummyNonComplianceLambda::HandleRequest",
                Code = Code.FromBucket(bucket, "my-codebuild-rule-function.zip"),
                //LogRetention = RetentionDays.THREE_DAYS,
                Timeout = Duration.Seconds(30),
                Role = Role.FromRoleName(this, "MyFunctionRole", "ato-role-dass-lambda-rule-exec", new FromRoleNameOptions
                {
                    Mutable = false,
                    AddGrantsToResources = false
                })
            });

            _ = new CfnOutput(this, "MyRuleStackOutput", new CfnOutputProps
            {
                Value = $"project={buildFunction.ProjectName} function={lambdaFunction.FunctionName}"
            });

            //var rule = new CustomRule(this, "MyCustomRule", new CustomRuleProps
            //{
            //    ConfigRuleName = "dass-security-group-rule",
            //    LambdaFunction = lambdaFunction,
            //    RuleScope = Amazon.CDK.AWS.Config.RuleScope.FromResource(ResourceType.EC2_SECURITY_GROUP, sg.SecurityGroupId),
            //    ConfigurationChanges = true,
            //    Periodic = false,
            //    Description = "DASS dummy rule to test custom security group"
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

            //_ = new CfnOutput(this, "MyRuleStackOutput", new CfnOutputProps
            //{
            //    Value = $"rule={rule.ConfigRuleId} {rule.ConfigRuleArn}, function= {lambdaFunction.FunctionArn} {lambdaFunction.FunctionName}"
            //});
        }
    }
}
