﻿using Amazon.CDK;
using Amazon.CDK.AWS.SecretsManager;
using Constructs;
using Amazon.CDK.AWS.CodeBuild;
using System;
using System.Collections.Generic;
using Amazon.CDK.AWS.S3;

namespace MyCdk
{
    internal class MyCdkStack : Stack
    {
        internal MyCdkStack(Construct scope, string id, IStackProps? props = null) : base(scope, id, props)
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
            this.Tags.SetTag("epmcode", epmCode.ValueAsString);

            var bucket = new Bucket(this, "my-bucket", new BucketProps
            {
                BucketName = "my-bucket"
            });

            var buildImage = new Project(this, "BuildContainerImage", new ProjectProps
            {
                BuildSpec = BuildSpec.FromSourceFilename("../image-buildspec.yml"),
                ProjectName = "build-container-image",
                Environment = new BuildEnvironment
                {
                    ComputeType = ComputeType.MEDIUM,
                    BuildImage = LinuxBuildImage.AMAZON_LINUX_2_5
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
                    Repo = "dotnet-hello-world"
                }),
                Artifacts = Artifacts.S3(new S3ArtifactsProps
                {
                    Bucket = bucket,
                    Path = "/image"
                })
            });

            var deployTemplate = new Project(this, "BuildDeployTemplate", new ProjectProps
            {
                ProjectName = "build-deploy-template",
                BuildSpec = BuildSpec.FromSourceFilename("../template-buildspec.yml"),
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

            new CfnOutput(this, "build container image", new CfnOutputProps
            {
                Value = $"{buildImage.ProjectName} {buildImage.Stack.StackId} {bucket.BucketArn} done."
            });

            new CfnOutput(this, "build deploy template", new CfnOutputProps
            {
                Value = $"{deployTemplate.ProjectName} {deployTemplate.Stack.StackId} {bucket.BucketArn} done."
            });
        }
    }
}