﻿using Amazon.CDK;
using Constructs;
using Amazon.CDK.AWS.CodeBuild;
using Amazon.CDK.AWS.S3;
<<<<<<< HEAD
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.ECR;
using Amazon.CDK.AWS.Logs;
=======
using Amazon.CDK.AWS.CodeStarNotifications;
>>>>>>> master

namespace MyCdk
{
    public class MyCodeBuildStack : Stack
    {
        public MyCodeBuildStack(Construct scope, string id, IStackProps? props = null) : base(scope, id, props)
        {
            var epmCode = new CfnParameter(this, "epmcode", new CfnParameterProps
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
            var saveImage = new CfnParameter(this, "save", new CfnParameterProps
            {
                Type = "String",
                Description = "Save build in ECR and Artifactory",
                AllowedValues = new string[] { "true", "false" },
                Default = "true"
            });
            Tags.SetTag("epmcode", epmCode.ValueAsString);

            // Create bucket if not exists
            var bucket = Bucket.FromBucketAttributes(this, "MyBucket", new BucketAttributes
                    {
                        Account = this.Account,
                        Region = this.Region,
                        BucketName = "ato-dass-bucket"
                    })
                    ?? new Bucket(this, "MyBucket", new BucketProps
                    {
                        BucketName = "ato-dass-bucket",
                        
                    });

            var role = Role.FromRoleArn(this, "MyCodeBuildRole", $"arn:aws:iam::{Account}:role/ato-role-dass-codebuild-service", new FromRoleArnOptions
            {
                Mutable = false,
                AddGrantsToResources = false
            });

            // Create repo if not exists
            var repo = Repository.FromRepositoryName(this, "MyHelloRepo", "dotnet-hello-world")
                    ?? new Repository(this, "MyHelloRepo", new RepositoryProps
                    {
                        RepositoryName = "dotnet-hello-world",

                    });
            // Create log group if not exists
            var logGroup = LogGroup.FromLogGroupName(this, "MyLogGroup", "dass-hello-codebuild")
                    ?? new LogGroup(this, "MyLogGroup", new LogGroupProps
                    {
                        LogGroupName = "dass-hello-codebuild",
                        Retention = RetentionDays.ONE_MONTH
                    });

            var buildImage = new Project(this, "MyBuildContainerImage", new ProjectProps
            {
                Role = role,
                BuildSpec = BuildSpec.FromSourceFilename("src/image-buildspec.yml"),
                ProjectName = "dass-build-hello-container-image",
                Environment = new BuildEnvironment
                {
                    ComputeType = ComputeType.SMALL,
                    BuildImage = LinuxBuildImage.AMAZON_LINUX_2_5,
                    Privileged = true
                },
                Logging = new LoggingOptions
                {
                    CloudWatch = new CloudWatchLoggingOptions
                    {
                        Enabled = true,
                        LogGroup = logGroup,
                        Prefix = "dass-hello-codebuild"
                    }
                },
                EnvironmentVariables = new Dictionary<string, IBuildEnvironmentVariable>
                {
                    ["AWS_DEFAULT_REGION"] = new BuildEnvironmentVariable
                    {
                        Value = Region,
                        Type = BuildEnvironmentVariableType.PLAINTEXT
                    },
                    ["AWS_ACCOUNT_ID"] = new BuildEnvironmentVariable
                    {
                        Value = Account
                    },
                    ["EPM_CODE"] = new BuildEnvironmentVariable
                    {
                        Value = epmCode.ValueAsString
                    },
                    ["APP_VERSION"] = new BuildEnvironmentVariable
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
                    BranchOrRef = "add-cdk-test",
                    Webhook = false
                }),
                Artifacts = Artifacts.S3(new S3ArtifactsProps
                {
                    Bucket = bucket,
                    IncludeBuildId = false
                })
            });

            var deployTemplate = new Project(this, "MyBuildDeployTemplate", new ProjectProps
            {
                Role = role,
                ProjectName = "dass-build-hello-deploy-template",
                BuildSpec = BuildSpec.FromSourceFilename("src/template-buildspec.yml"),
                Environment = new BuildEnvironment
                {
                    ComputeType = ComputeType.SMALL,
                    BuildImage = LinuxBuildImage.AMAZON_LINUX_2_5
                },
                Source = Source.GitHub(new GitHubSourceProps
                {
                    Owner = "kar-yeow",
                    Repo = "dotnet-hello-world",
                    BranchOrRef = "add-cdk-test",
                    Webhook = false
                }),
                Artifacts = Artifacts.S3(new S3ArtifactsProps
                {
                    Bucket = bucket,
                    IncludeBuildId = false,
                    PackageZip = false
                })
            });

            _ = new CfnOutput(this, "MyBuildContainerImageOutput", new CfnOutputProps
            {
                Value = $"{buildImage.ProjectName} {buildImage.Stack.StackId} {bucket.BucketArn} done."
            });

            _ = new CfnOutput(this, "MyBuildDeployTemplateOutput", new CfnOutputProps
            {
                Value = $"{deployTemplate.ProjectName} {deployTemplate.Stack.StackId} {bucket.BucketArn} {repo.RepositoryArn}."
            });
        }
    }
}
