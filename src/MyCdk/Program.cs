using Amazon.CDK;
using System.Diagnostics;

namespace MyCdk
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();
            _ = new MyCodeBuildStack(app, "MyCodeBuildStack", new StackProps
            {
                StackName = $"dass-build-image-stack",
                Env = new Amazon.CDK.Environment
                {
                    Account = System.Environment.GetEnvironmentVariable("AWS_ACCOUNT"),
                    Region = System.Environment.GetEnvironmentVariable("AWS_REGION")
                }
            });

            _ = new MyAppRunnerStack(app, "MyAppRunnerStack", new StackProps
            {
                StackName = $"dass-build-template-stack",
                Env = new Amazon.CDK.Environment
                {
                    Account = System.Environment.GetEnvironmentVariable("AWS_ACCOUNT"),
                    Region = System.Environment.GetEnvironmentVariable("AWS_REGION")
                }
            });

            _ = new MyFargateStack(app, "MyFargateStack", new StackProps
            {
                StackName = $"dass-build-fargate-template-stack",
                Env = new Amazon.CDK.Environment
                {
                    Account = System.Environment.GetEnvironmentVariable("AWS_ACCOUNT"),
                    Region = System.Environment.GetEnvironmentVariable("AWS_REGION")
                }
            });

            app.Synth();
        }
    }
}
