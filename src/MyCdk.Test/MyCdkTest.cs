using Amazon.CDK;
using Amazon.CDK.Assertions;

namespace MyCdk.Test
{
    public class MyCdkTest
    {
        [Fact]
        public void ShouldHaveCodeBuildProject()
        {
            var app = new App();
            var stack = new MyCodeBuildStack(app, "MyCodeBuildStack");
            var template = Template.FromStack(stack);
            template.HasResourceProperties("AWS::CodeBuild::Project", new Dictionary<string, object>
            {
                ["Environment"] = new Dictionary<string, object>
                {
<<<<<<< HEAD
                    ["ComputeType"] = "BUILD_GENERAL1_SMALL",
=======
                    ["ComputeType"] = "BUILD_GENERAL1_MEDIUM",
>>>>>>> master
                    ["EnvironmentVariables"] = new Dictionary<string, object>[]
                    {
                        new Dictionary<string, object>
                        {
<<<<<<< HEAD
                            ["Name"] = "AWS_DEFAULT_REGION",
                            ["Type"] = "PLAINTEXT"
                        },
                        new Dictionary<string, object>
                        {
                            ["Name"] = "AWS_ACCOUNT_ID",
                            ["Type"] = "PLAINTEXT"
                        },
                        new Dictionary<string, object>
                        {
=======
>>>>>>> master
                            ["Name"] = "EPM_CODE",
                            ["Type"] = "PLAINTEXT"
                        },
                        new Dictionary<string, object>
                        {
<<<<<<< HEAD
                            ["Name"] = "APP_VERSION",
=======
                            ["Name"] = "SOFTWARE_VERSION",
>>>>>>> master
                            ["Type"] = "PLAINTEXT"
                        },
                        new Dictionary<string, object>
                        {
                            ["Name"] = "SAVE_IMAGE",
                            ["Type"] = "PLAINTEXT"
                        }
                    },
                    ["Image"] = "aws/codebuild/amazonlinux2-x86_64-standard:5.0"
                },
                ["Source"] = new Dictionary<string, object>
                {
                    ["Location"] = "https://github.com/kar-yeow/dotnet-hello-world.git",
                    ["Type"] = "GITHUB"
                }
            });
        }

        [Fact]
        public void ShouldHaveAppRunnerService()
        {
            var app = new App();
            var stack = new MyAppRunnerStack(app, "MyAppRunnerStack");
            var template = Template.FromStack(stack);
            template.HasResourceProperties("AWS::AppRunner::Service", new Dictionary<string, object>
            {
<<<<<<< HEAD
                ["ServiceName"] = "dass-hello-apprunner-service",
=======
                ["ServiceName"] = "hello-world-app-runner-service",
>>>>>>> master
                ["SourceConfiguration"] = new Dictionary<string, object>
                {
                    ["ImageRepository"] = new Dictionary<string, object>
                    {
                        ["ImageConfiguration"] = new Dictionary<string, object>
                        {
                            ["Port"] = "5050"
                        },
                        ["ImageRepositoryType"] = "ECR"
                    }
                }
            });
        }
    }
}