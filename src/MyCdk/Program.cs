using Amazon.CDK;

namespace MyCdk
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();
            new MyCdkStack(app, "MyCdkStack", new StackProps
            {
                StackName = $"deploy-codebuild-stack",
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
