using Amazon.CDK;
using Constructs;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ECS.Patterns;
using Amazon.CDK.AWS.ElasticLoadBalancingV2;
using ApplicationLoadBalancerProps = Amazon.CDK.AWS.ElasticLoadBalancingV2.ApplicationLoadBalancerProps;
using Amazon.CDK.AWS.ECR;
using System.Text.RegularExpressions;

namespace MyCdk
{
    public class MyFargateStack : Stack
    {
        public MyFargateStack(Construct scope, string id, IStackProps? props = null) : base(scope, id, props)
        {
            var vpcId = new CfnParameter(this, "vpc", new CfnParameterProps
            {
                Type = "String",
                Description = "VPC ID to create to stack",
                AllowedPattern = "vpc-0[0-9|a-f]{16}"
            });
            var subnetId1 = new CfnParameter(this, "subnet1", new CfnParameterProps
            {
                Type = "String",
                Description = "Subnet ID 1 for the VPC",
                AllowedPattern = "subnet-0[0-9|a-f]{16}"
            });
            var subnetId2 = new CfnParameter(this, "subnet2", new CfnParameterProps
            {
                Type = "String",
                Description = "Subnet ID 2 for the VPC",
                AllowedPattern = "subnet-0[0-9|a-f]{16}"
            });
            var subnetId3 = new CfnParameter(this, "subnet3", new CfnParameterProps
            {
                Type = "String",
                Description = "Subnet ID 3 for the VPC",
                AllowedPattern = "subnet-0[0-9|a-f]{16}"
            });

            var routerId1 = new CfnParameter(this, "router1", new CfnParameterProps
            {
                Type = "String",
                Description = "Subnet router table IDs",
                AllowedPattern = "rtb-0[0-9|a-f]{16}"
            });
            var routerId2 = new CfnParameter(this, "router2", new CfnParameterProps
            {
                Type = "String",
                Description = "Subnet router table IDs",
                AllowedPattern = "rtb-0[0-9|a-f]{16}"
            });
            var routerId3 = new CfnParameter(this, "router3", new CfnParameterProps
            {
                Type = "String",
                Description = "Subnet router table IDs",
                AllowedPattern = "rtb-0[0-9|a-f]{16}"
            });
            var role = Role.FromRoleName(this, "MyTaskRole", "ato-role-dass-ecs", new FromRoleNameOptions
            {
                Mutable = false,
                AddGrantsToResources = false
            });
            //var vpc = Vpc.FromLookup(this, "MyVpc", new VpcLookupOptions
            //{
            //    Region = this.Region,
            //    OwnerAccountId = this.Account
            //});
            var availabilityZones = new string[] { "ap-southeast-2a", "ap-southeast-2b", "ap-southeast-2c" };
            var vpc = Vpc.FromVpcAttributes(this, "MyVpc", new VpcAttributes
            {
                VpcId = vpcId.ValueAsString,
                AvailabilityZones = availabilityZones,
                IsolatedSubnetIds = new string[] { subnetId1.ValueAsString, subnetId2.ValueAsString, subnetId3.ValueAsString },
                IsolatedSubnetRouteTableIds = new string[] { routerId1.ValueAsString, routerId2.ValueAsString, routerId3.ValueAsString }

                //    IsolatedSubnetIds = new string[] { "subnet-049369136ecb2bd54", "subnet-07783132d0a53c5e7", "subnet-0d9bbac6f4cd2ff6f" },
                //    IsolatedSubnetRouteTableIds = new string[] { "rtb-049c4bf4db5b9b8ce", "rtb-049c4bf4db5b9b8ce", "rtb-049c4bf4db5b9b8ce" }
            });
            var subnets = vpc.SelectSubnets(new SubnetSelection
            {
                AvailabilityZones = availabilityZones
            });
            //var asg = new AutoScalingGroup(this, "MyAsg", new AutoScalingGroupProps
            //{
            //    Vpc = vpc,

            //    InstanceType = InstanceType.Of(InstanceClass.BURSTABLE3, InstanceSize.MICRO),
            //    MachineImage = new AmazonLinuxImage()
            //});
            //var securityGroup = SecurityGroup.FromLookupByName(this, "MySg", "Fargate", vpc);
            var securityGroup = new SecurityGroup(this, "MySg", new SecurityGroupProps
            {
                Description = "My hello security group",
                SecurityGroupName = "dass-hello-sg",
                Vpc = vpc,
                AllowAllOutbound = true
            });
            var alb = new ApplicationLoadBalancer(this, "MyAlb", new ApplicationLoadBalancerProps
            {
                InternetFacing = false,
                IpAddressType = IpAddressType.IPV4,
                LoadBalancerName = "dass-hello-fargate-alb",
                Vpc = vpc,
                VpcSubnets = new SubnetSelection
                {
                    Subnets = subnets.Subnets,//.Where(x => Regex.IsMatch(x.Ipv4CidrBlock, "172\\.16\\.[0-2]\\.")).ToList().ToArray(),
                    AvailabilityZones = subnets.AvailabilityZones
                },
                SecurityGroup = securityGroup,
            });
            //var listener = alb.AddListener("MyListener", new BaseApplicationListenerProps
            //{
            //    Port = 80
            //});
            //var targetGroup = listener.AddTargets("MyTargate", new AddApplicationTargetsProps
            //{
            //    Port = 80,
            //    TargetGroupName = "dass-hello-cdk-tg",
            //    Targets = new IApplicationLoadBalancerTarget[] { new IPAddress() }
            //});
            var service = new ApplicationLoadBalancedFargateService(this, "MyFargateService", new ApplicationLoadBalancedFargateServiceProps 
            {
                Cluster = new Cluster(this, "MyCluster", new ClusterProps
                {
                    ClusterName = "dass-hello-fargate-cluster",
                    Vpc = vpc
                }),
                AssignPublicIp = false,
                //TaskSubnets = new SubnetSelection
                //{
                //    Subnets = subnets
                //},
                TaskImageOptions = new ApplicationLoadBalancedTaskImageOptions
                {
                    Image = ContainerImage.FromEcrRepository(Repository.FromRepositoryName(this, "MyRepo", "dotnet-hello-world"), "0.0.1"),
                    ContainerName = "dotnet-hello-world",
                    ContainerPort = 5050,
                    ExecutionRole = role,
                    TaskRole = role,                    
                },
                //TaskDefinition = new FargateTaskDefinition(this, "MyTaskDefinition", new FargateTaskDefinitionProps
                //{
                //    TaskRole = role,
                //    Cpu = 1024,
                //    ExecutionRole = role,
                //    RuntimePlatform = new RuntimePlatform
                //    {
                //        CpuArchitecture = CpuArchitecture.X86_64,
                //        OperatingSystemFamily = OperatingSystemFamily.LINUX
                //    }
                //}),
                ListenerPort = 80,
                Protocol = ApplicationProtocol.HTTP,
                LoadBalancer = alb,
                ServiceName = "dass-hello-fargate-service"
            });

            _ = new CfnOutput(this, "MyFargateStack", new CfnOutputProps 
            { 
                Value = $"my fargate stack {service.LoadBalancer}"
            });
        }
    }
}
