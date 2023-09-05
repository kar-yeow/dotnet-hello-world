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
                    ["ComputeType"] = "BUILD_GENERAL1_MEDIUM",
                    ["EnvironmentVariables"] = new Dictionary<string, object>[]
                    {
                        new Dictionary<string, object>
                        {
                            ["Name"] = "EPM_CODE",
                            ["Type"] = "PLAINTEXT"
                        },
                        new Dictionary<string, object>
                        {
                            ["Name"] = "SOFTWARE_VERSION",
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
                ["ServiceName"] = "hello-world-app-runner-service",
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