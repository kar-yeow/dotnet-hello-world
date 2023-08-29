using Amazon.CDK;
using Constructs;
using Amazon.CDK.AWS.AppRunner;
using static Amazon.CDK.AWS.AppRunner.CfnService;

namespace MyCdk
{
    internal class MyAppRunnerStack : Stack
    {
        internal MyAppRunnerStack(Construct scope, string id, IStackProps? props = null) : base(scope, id, props)
        {
            var epmCode = new CfnParameter(this, "empcode", new CfnParameterProps
            {
                Type = "String",
                Description = "ATO project cost code"
            });
            var appVersion = new CfnParameter(this, "version", new CfnParameterProps
            {
                Type = "String",
                Description = "Application software version to deploy"
            });
            this.Tags.SetTag("epmcode", epmCode.ValueAsString);

            var autoScalingConfiguration = new CfnAutoScalingConfiguration(this, "app-runner-auto-scaling", new CfnAutoScalingConfigurationProps
            {
                AutoScalingConfigurationName = "my-auto-scaling",
                MaxConcurrency = 100,
                MaxSize = 25,
                MinSize = 1
            });

            var appRunner = new CfnService(this, "app-runner-template", new CfnServiceProps{
                ServiceName = "hello-world-app-runner-service",
                AutoScalingConfigurationArn = autoScalingConfiguration.AttrAutoScalingConfigurationArn,
                SourceConfiguration = new SourceConfigurationProperty
                {
                    ImageRepository = new ImageRepositoryProperty
                    {
                        ImageIdentifier = $"{this.Account}.dkr.ecr.{this.Region}.amazonaws.com/dotnet-hello-world:{appVersion.ValueAsString}",
                        ImageRepositoryType = "ECR",
                        ImageConfiguration = new ImageConfigurationProperty
                        {
                            Port = "8080"
                        }
                    }
                }
            });

            new CfnOutput(this, "app runner template", new CfnOutputProps
            {
                Value = $"{appRunner.ServiceName} {appRunner.Stack.StackId} {appRunner.AttrServiceUrl} done."
            });
        }
    }
}
