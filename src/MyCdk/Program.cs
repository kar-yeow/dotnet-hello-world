using Amazon.CDK;

namespace MyCdk
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();
            new MyCodeBuildStack(app, "MyCodeBuildStack", new StackProps
            {
                StackName = $"build-image-stack",
                Env = new Amazon.CDK.Environment
                {
                    Account = System.Environment.GetEnvironmentVariable("AWS_ACCOUNT"),
                    Region = System.Environment.GetEnvironmentVariable("AWS_REGION")
                }
            });

            new MyAppRunnerStack(app, "MyAppRunnerStack", new StackProps
            {
                StackName = $"build-template-stack",
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
