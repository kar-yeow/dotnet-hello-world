using Amazon.CDK;
using Amazon.CDK.AWS.CodeBuild;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.S3;
using Constructs;

namespace MyCdk
{
    public class MyFunctionStack : Stack
    {
        public MyFunctionStack(Construct scope, string id, IStackProps? stackProps) : base(scope, id, stackProps)
        {
            var bucket = Bucket.FromBucketAttributes(this, "MyBucket", new BucketAttributes
                    {
                        Account = this.Account,
                        Region = this.Region,
                        BucketName = "ato-dass-hello-bucket"
                    })
                    ?? new Bucket(this, "MyBucket", new BucketProps
                    {
                        BucketName = "ato-dass-hello-bucket"
                    });
            var logGroup = LogGroup.FromLogGroupName(this, "MyLogGroup", "dass-function-codebuild") 
                    ?? new LogGroup(this, "MyLogGroup", new LogGroupProps
                    {
                        LogGroupName = "dass-function-codebuild",
                        Retention = RetentionDays.ONE_MONTH
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
                        LogGroup = logGroup,
                        Prefix = "dass-function-codebuild"
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
                    Name = "my-codebuild-rule-function.zip",
                    IncludeBuildId = false
                })
            });

            _ = new CfnOutput(this, "MyRuleStackOutput", new CfnOutputProps
            {
                Value = $"project={buildFunction.ProjectName}"
            });
        }
    }
}
