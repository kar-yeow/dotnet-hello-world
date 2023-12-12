using Amazon.CDK;
using Amazon.CDK.AWS.Config;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.SSM;
using Amazon.CDK.AWS.Synthetics;
using Constructs;

namespace MyCdk
{
    public class MyDummyStack : Stack
    {
        public MyDummyStack(Construct scope, string id, IStackProps? props = null) : base(scope, id, props)
        {
            //var vpc = new Vpc(this, "MyVpc", new VpcProps
            //{
            //    RestrictDefaultSecurityGroup = false,
            //    EnableDnsSupport = true,
            //    EnableDnsHostnames = true,
            //    NatGateways = 0,
            //    IpAddresses = IpAddresses.Cidr("10.0.0.0/20"),
            //    SubnetConfiguration = new SubnetConfiguration[]
            //    {
            //        new SubnetConfiguration
            //        {
            //            SubnetType = SubnetType.PRIVATE_ISOLATED,
            //            CidrMask = 22,
            //            Name = "my-isolated-subnet"
            //        }
            //    },
            //    MaxAzs = 2
            //});
            var vpc = Vpc.FromLookup(this, "MyVpc", new VpcLookupOptions
            {
                Region = this.Region,
                OwnerAccountId = this.Account
            });
            var sg = new SecurityGroup(this, "MySecurityGroup", new SecurityGroupProps
            {
                SecurityGroupName = "dass-rule-test-security-group",
                //Vpc = Vpc.FromVpcAttributes(this, "MyVpc", new VpcAttributes
                //{
                //    VpcId = "vpc-01fe61cb1984e4911",
                //    AvailabilityZones = new string[] { "ap-southeast-2" }
                //})
                Vpc = vpc
                //DisableInlineRules = true
            });

            //var subnets = new ISubnet[]
            //{
            //    Subnet.FromSubnetAttributes(this, "MySubnet1", new SubnetAttributes
            //    {
            //        SubnetId = "subnet-049369136ecb2bd54",
            //        RouteTableId = "rtb-049c4bf4db5b9b8ce"
            //    }),
            //    Subnet.FromSubnetAttributes(this, "MySubnet2", new SubnetAttributes
            //    {
            //        SubnetId = "subnet-06e6594cb233f514b",
            //        RouteTableId = "rtb-049c4bf4db5b9b8ce"
            //    })
            //};

            var bucket = Bucket.FromBucketAttributes(this, "MyBucket", new BucketAttributes
                    {
                        Account = this.Account,
                        Region = this.Region,
                        BucketName = "ato-dass-bucket"
                    })
                    ?? new Bucket(this, "MyBucket", new BucketProps
                    {
                        BucketName = "ato-dass-bucket"
                    });

            var lambdaFunction = new Function(this, "MyCodeBuildRuleFunction", new FunctionProps
            {
                FunctionName = "dass-codebuild-rule-function",
                Description = "Dummy rule function to return non compliance status",
                Runtime = Amazon.CDK.AWS.Lambda.Runtime.DOTNET_6,
                Handler = "MyRuleFunction::MyRuleFunction.DummyNonComplianceLambda::HandleRequest",
                Code = Amazon.CDK.AWS.Lambda.Code.FromBucket(bucket, "my-codebuild-rule-function.zip"),
                //LogRetention = RetentionDays.THREE_DAYS,
                Timeout = Duration.Seconds(30),
                Role = Role.FromRoleName(this, "MyFunctionRole", "ato-role-dass-lambda-rule-exec", new FromRoleNameOptions
                {
                    Mutable = false,
                    AddGrantsToResources = false
                })
            });

            var rule = new CustomRule(this, "MyCustomRule", new CustomRuleProps
            {
                ConfigRuleName = $"dass-security-group-rule-{sg.SecurityGroupId}",
                LambdaFunction = lambdaFunction,
                RuleScope = Amazon.CDK.AWS.Config.RuleScope.FromResource(ResourceType.EC2_SECURITY_GROUP, sg.SecurityGroupId),
                ConfigurationChanges = true,
                Periodic = false,
                Description = "DASS dummy rule to test custom security group"
            });

            var role = Role.FromRoleName(this, "MySsmDocumentRole", "ato-role-dass-automation-test");
            var ssmDocument = new CfnDocument(this, "MySsmDocument", new CfnDocumentProps
            {
                Content = new Dictionary<string, object>
                {
                    ["schemaVersion"] = "0.3",
                    ["assumeRole"] = "{{AutomationAssumeRole}}",
                    ["description"] = "#Test doc \nTest automation doc \n#Input Parameters \n- Id: (Required) \n#Output Parameters \n- None",
                    ["parameters"] = new Dictionary<string, object>
                    {
                        ["SecurityGroupId"] = new Dictionary<string, object>
                        {
                            ["type"] = "String",
                            ["description"] = "(Required) The Security Group ID",
                            ["allowedPattern"] = "^(sg-)([0-9a-f]){1,}$",
                            ["default"] = sg.SecurityGroupId
                        },
                        ["AutomationAssumeRole"] = new Dictionary<string, object>
                        {
                            ["type"] = "String",
                            ["description"] = "(Optional) The role ARN to assume during automation execution",
                            ["allowedPattern"] = "^arn:aws(-cn|-us-gov)?:iam::\\d{12}:role\\/[\\w+=,.@_\\/-]+|^$",
                            ["default"] = role.RoleArn
                        }
                    },
                    ["mainSteps"] = new Dictionary<string, object>[]
                    {
                        new Dictionary<string, object>
                        {
                            ["name"] = "RunScript",
                            ["action"] = "aws:executeScript",
                            ["description"] = "##Run hello Python\nEcho security group id",
                            ["inputs"] = new Dictionary<string, object>
                            {
                                ["Runtime"] = "python3.8",
                                ["Handler"] = "script_handler",
                                ["InputPayload"] = new Dictionary<string, object>
                                {
                                    ["securityGroupId"] = "{{SecurityGroupId}}"
                                },
                                ["Script"] = "def script_handler(event, context):\n  id = event.get('securityGroupId')\n  if id is None:\n    raise ValueError(\"Security Group ID is invalid\")\n  return { \"message\": \"Hello \" + id }"
                            },
                            ["outputs"] = new Dictionary<string, object>[]
                            {
                                new Dictionary<string, object> 
                                {
                                    ["Name"] = "Output",
                                    ["Selector"] = "$.Payload.message",
                                    ["Type"] = "String",
                                }
                            }
                        },
                        new Dictionary<string, object>
                        {
                            ["name"] = "RunAwsApi",
                            ["action"] = "aws:executeAwsApi",
                            ["description"] = "Make AWS API call",
                            ["isEnd"] = "true",
                            ["inputs"] = new Dictionary<string, object>
                            {
                                ["Service"] = "cloudformation",
                                ["Api"] = "DescribeStacks",
                                ["StackName"] = "dass-hello-dummy-stack"
                            },
                            ["outputs"] = new Dictionary<string, object>[]
                            {
                                new Dictionary<string, object>
                                {
                                    ["Name"] = "Output",
                                    ["Selector"] = "$.Payload",
                                    ["Type"] = "String",
                                }
                            }
                        }
                    }
                },
                Name = "dass-my-ssm-document",
                DocumentFormat = "JSON",
                DocumentType = "Automation",
                //TargetType = "AWS::EC2::SecurityGroup"
            });

            if (ssmDocument != null && ssmDocument.Name != null)
            {
                _ = new CfnRemediationConfiguration(this, "MyRuleRemediation", new CfnRemediationConfigurationProps
                {
                    ConfigRuleName = rule.ConfigRuleName,
                    TargetId = ssmDocument.Name,
                    TargetType = "SSM_DOCUMENT",
                    Automatic = false,
                    ExecutionControls = new CfnRemediationConfiguration.ExecutionControlsProperty
                    {
                        SsmControls = new CfnRemediationConfiguration.SsmControlsProperty
                        {
                            ConcurrentExecutionRatePercentage = 100,
                            ErrorPercentage = 100
                        }
                    },
                    //Parameters = new Dictionary<string, CfnRemediationConfiguration.RemediationParameterValueProperty>
                    //{
                    //    ["id"] = new CfnRemediationConfiguration.RemediationParameterValueProperty
                    //    {
                    //        //        ResourceValue = new CfnRemediationConfiguration.ResourceValueProperty
                    //        //        {
                    //        //            Value = sg.SecurityGroupId
                    //        //        }
                    //        StaticValue = new CfnRemediationConfiguration.StaticValueProperty
                    //        {
                    //            Value = new string[] { sg.SecurityGroupId }
                    //        }
                    //    },
                    //    //["AutomationAssumeRole"] = new CfnRemediationConfiguration.RemediationParameterValueProperty
                    //    //{
                    //    //    StaticValue = new CfnRemediationConfiguration.StaticValueProperty
                    //    //    {
                    //    //        Value = new string[] { "arn:aws:iam::046037307781:role/ato-role-dass-automation-test" }
                    //    //    }
                    //    //}
                    //}
                });
            }

            //for (int i = 0; i < subnetToMonitor.Length; i++)
            //{
            //    string ruleName = $"dass-verify-isolated-subnet-compliance-rule-{i+1}"; // Add 1 so we aren't using a zero based index for naming.
            //    _ = new CustomRule(this, ruleName, new CustomRuleProps
            //    {
            //        ConfigRuleName = ruleName,
            //        LambdaFunction = func,
            //        RuleScope = RuleScope.FromResource(ResourceType.EC2_SUBNET, subnetToMonitor[i].SubnetId),
            //        ConfigurationChanges = true,
            //        Periodic = false,
            //        Description = $"{ruleName} to test custom rule"
            //    });
            //}
            Tags.SetTag("compliance-checked", "true");

            _ = new CfnOutput(this, "MyConfigRuleAndLambdaFunctionOutput", new CfnOutputProps
            {
                Value = $"rule={rule.ConfigRuleName}, function= {lambdaFunction.FunctionArn} {lambdaFunction.FunctionName}"
            });

            _ = new CfnOutput(this, "MyDummyStackOutput", new CfnOutputProps
            {
                Value = $"Stack name= {this.StackName}, sg={sg.SecurityGroupId}"
            });
        }
    }
}
