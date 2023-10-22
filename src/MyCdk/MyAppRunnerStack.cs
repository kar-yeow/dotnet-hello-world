using Amazon.CDK;
using Constructs;
using Amazon.CDK.AWS.AppRunner;
using static Amazon.CDK.AWS.AppRunner.CfnService;
using Amazon.CDK.AWS.IAM;

namespace MyCdk
{
    public class MyAppRunnerStack : Stack
    {
        public MyAppRunnerStack(Construct scope, string id, IStackProps? props = null) : base(scope, id, props)
        {
            var epmCode = new CfnParameter(this, "empcode", new CfnParameterProps
            {
                Type = "String",
                Description = "ATO project cost code",
                AllowedValues = new string[] { "ABC123", "EFG456" },
                Default = "ABC123"
            });
            var appVersion = new CfnParameter(this, "version", new CfnParameterProps
            {
                Type = "String",
                Description = "Application software version to deploy",
                AllowedValues = new string[] { "0.0.1", "0.0.2", "0.0.3" },
                Default = "0.0.1"
            });
            Tags.SetTag("epmcode", epmCode.ValueAsString);

            var autoScalingConfiguration = new CfnAutoScalingConfiguration(this, "MyHelloAppRunnerAutoScaling", new CfnAutoScalingConfigurationProps
            {
                AutoScalingConfigurationName = "dass-hello-auto-scaling",
                MaxConcurrency = 100,
                MaxSize = 25,
                MinSize = 1
            });
            var instanceRole = Role.FromRoleArn(this, 
                    "AppRunnerInstanceRole",
                    $"arn:aws:iam::{Account}:role/ato-role-dass-instance-apprunner",
                    new FromRoleArnOptions
            {
                Mutable = false,
                AddGrantsToResources = false
            });

            var accessRole = Role.FromRoleArn(this, 
                    "AppRunnerAccessRole",
                    $"arn:aws:iam::{Account}:role/ato-role-dass-ecr",
                    new FromRoleArnOptions
            {
                Mutable = false,
                AddGrantsToResources = false
            });

            var appRunner = new CfnService(this, "MyAppRunnerService", new CfnServiceProps{
                ServiceName = "dass-hello-apprunner-service",
                AutoScalingConfigurationArn = autoScalingConfiguration.AttrAutoScalingConfigurationArn,
                InstanceConfiguration = new InstanceConfigurationProperty
                {
                    InstanceRoleArn = instanceRole.RoleArn,
                    Cpu = "2 vCPU",
                    Memory = "4 GB",
                },
                SourceConfiguration = new SourceConfigurationProperty
                {
                    AuthenticationConfiguration = new AuthenticationConfigurationProperty 
                    {
                        AccessRoleArn = accessRole.RoleArn
                    },
                    ImageRepository = new ImageRepositoryProperty
                    {
                        ImageIdentifier = $"{this.Account}.dkr.ecr.{this.Region}.amazonaws.com/dotnet-hello-world:{appVersion.ValueAsString}",
                        ImageRepositoryType = "ECR",
                        ImageConfiguration = new ImageConfigurationProperty
                        {
                            Port = "5050"
                        }
                    }
                },
                HealthCheckConfiguration = new HealthCheckConfigurationProperty
                {
                    Protocol = "HTTP",
                    Path = "/",
                    HealthyThreshold = 2,
                    UnhealthyThreshold = 5,
                    Timeout = 5,
                    Interval = 20,
                }
            });

            new CfnOutput(this, "MyAppRunnerStackOutput", new CfnOutputProps
            {
                Value = $"{appRunner.ServiceName} {appRunner.Stack.StackId} {appRunner.AttrServiceUrl} done."
            });
        }
    }
}
