using Amazon.CDK;
using Constructs;
using Amazon.CDK.AWS.CodeBuild;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.IAM;

namespace MyCdk
{
    public class MyCodeBuildStack : Stack
    {
        public MyCodeBuildStack(Construct scope, string id, IStackProps? props = null) : base(scope, id, props)
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
            var saveImage = new CfnParameter(this, "save", new CfnParameterProps
            {
                Type = "String",
                Description = "Save build in ECR and Artifactory"
            });
            Tags.SetTag("epmcode", epmCode.ValueAsString);

            var bucket = new Bucket(this, "DassHelloBucket", new BucketProps
            {
                BucketName = "ato-dass-hello-bucket"
            });
            var role = Role.FromRoleArn(this, "BuildContainerRole", "arn:aws:iam::037690295447:role/ato-role-dass-codebuild-service", new FromRoleArnOptions
            {
                Mutable = false,
                AddGrantsToResources = false
            });

            var buildImage = new Project(this, "BuildContainerImage", new ProjectProps
            {
                Role = role,
                BuildSpec = BuildSpec.FromSourceFilename("src/image-buildspec.yml"),
                ProjectName = "dass-build-container-hello-cfst-image",
                Environment = new BuildEnvironment
                {
                    ComputeType = ComputeType.MEDIUM,
                    BuildImage = LinuxBuildImage.AMAZON_LINUX_2_5,
                    Privileged = true
                },
                EnvironmentVariables = new Dictionary<string, IBuildEnvironmentVariable>
                {
                    ["EPM_CODE"] = new BuildEnvironmentVariable
                    {
                        Value = epmCode.ValueAsString
                    },
                    ["SOFTWARE_VERSION"] = new BuildEnvironmentVariable
                    {
                        Value = appVersion.ValueAsString
                    },
                    ["SAVE_IMAGE"] = new BuildEnvironmentVariable
                    {
                        Value = saveImage.ValueAsString
                    }
                },
                Source = Source.GitHub(new GitHubSourceProps
                {
                    Owner = "kar-yeow",
                    Repo = "dotnet-hello-world",
                    Webhook = true
                }),
                Artifacts = Artifacts.S3(new S3ArtifactsProps
                {
                    Bucket = bucket,
                    Path = "/image"
                })
            });

            var deployTemplate = new Project(this, "BuildDeployTemplate", new ProjectProps
            {
                Role = role,
                ProjectName = "dass-build-deploy-hello-cfst-template",
                BuildSpec = BuildSpec.FromSourceFilename("src/template-buildspec.yml"),
                Environment = new BuildEnvironment
                {
                    ComputeType = ComputeType.MEDIUM,
                    BuildImage = LinuxBuildImage.AMAZON_LINUX_2_5
                },
                Source = Source.GitHub(new GitHubSourceProps
                {
                    Owner = "kar-yeow",
                    Repo = "dotnet-hello-world"
                }),
                Artifacts = Artifacts.S3(new S3ArtifactsProps
                {
                    Bucket = bucket,
                    Path = "/template"
                })
            });

            _ = new CfnOutput(this, "build container image", new CfnOutputProps
            {
                Value = $"{buildImage.ProjectName} {buildImage.Stack.StackId} {bucket.BucketArn} done."
            });

            _ = new CfnOutput(this, "build deploy template", new CfnOutputProps
            {
                Value = $"{deployTemplate.ProjectName} {deployTemplate.Stack.StackId} {bucket.BucketArn} done."
            });
        }
    }
}
