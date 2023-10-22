using Amazon.CDK;
using System.Diagnostics;

namespace MyCdk
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();
            var synthesizer = new BootstraplessSynthesizer();
            var env = new Amazon.CDK.Environment
            {
                //Account = System.Environment.GetEnvironmentVariable("AWS_ACCOUNT"),
                //Region = System.Environment.GetEnvironmentVariable("AWS_DEFAULT_REGION")
                Account = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT"),
                Region = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_REGION")
            };


            _ = new MyCodeBuildStack(app, "MyCodeBuildStack", new StackProps
            {
                StackName = $"dass-build-image-stack",
                Env = env,
                Synthesizer = synthesizer
            });

            _ = new MyAppRunnerStack(app, "MyAppRunnerStack", new StackProps
            {
                StackName = $"dass-build-template-stack",
                Env = env,
                Synthesizer = synthesizer
            });

            _ = new MyFargateStack(app, "MyFargateStack", new StackProps
            {
                StackName = $"dass-build-fargate-template-stack",
                Env = env,
                Synthesizer = synthesizer
            });

            _ = new MyFunctionStack(app, "MyFunctionStack", new StackProps
            {
                StackName = $"dass-hello-function-stack",
                Env = env,
                Synthesizer = synthesizer
            });

            _ = new MyDummyStack(app, "MyDummyStack", new StackProps
            {
                StackName = $"dass-hello-dummy-stack",
                Env = env,
                Synthesizer = synthesizer
            });

            app.Synth();
        }
    }
}
