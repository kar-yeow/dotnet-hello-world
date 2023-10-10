using Amazon.CDK;
using System.Diagnostics;

namespace MyCdk
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();
            var env = new Amazon.CDK.Environment
            {
                Account = System.Environment.GetEnvironmentVariable("AWS_ACCOUNT"),
                Region = System.Environment.GetEnvironmentVariable("AWS_DEFAULT_REGION")
            };

            _ = new MyCodeBuildStack(app, "MyCodeBuildStack", new StackProps
            {
                StackName = $"dass-build-image-stack",
                Env = env
            });

            _ = new MyAppRunnerStack(app, "MyAppRunnerStack", new StackProps
            {
                StackName = $"dass-build-template-stack",
                Env = env
            });

            _ = new MyFargateStack(app, "MyFargateStack", new StackProps
            {
                StackName = $"dass-build-fargate-template-stack",
                Env = env
            });

            _ = new MyFunctionStack(app, "MyFunctionStack", new StackProps
            {
                StackName = $"dass-hello-function-stack",
                Env = env
            });

            _ = new MyDummyStack(app, "MyDummyStack", new StackProps
            {
                StackName = $"dass-hello-dummy-stack",
                Env = env
            });

            app.Synth();
        }
    }
}
