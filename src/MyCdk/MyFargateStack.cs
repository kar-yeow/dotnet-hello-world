using Amazon.CDK;
using Constructs;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ECS.Patterns;
using Amazon.CDK.AWS.ElasticLoadBalancingV2;
using ApplicationLoadBalancerProps = Amazon.CDK.AWS.ElasticLoadBalancingV2.ApplicationLoadBalancerProps;
using Amazon.CDK.AWS.ECR;

namespace MyCdk
{
    public class MyFargateStack : Stack
    {
        public MyFargateStack(Construct scope, string id, IStackProps? props = null) : base(scope, id, props)
        {
            var role = Role.FromRoleName(this, "MyTaskRole", "ato-role-dass-ecs", new FromRoleNameOptions
            {
                Mutable = false,
                AddGrantsToResources = false
            });
            var vpc = Vpc.FromLookup(this, "MyVpc", new VpcLookupOptions
            {
                Region = this.Region,
                OwnerAccountId = this.Account
            });
            var availabilityZones = new string[] { "ap-southeast-2a", "ap-southeast-2b", "ap-southeast-2c" };
            //var vpc = Vpc.FromVpcAttributes(this, "MyVpc", new VpcAttributes
            //{
            //    VpcId = "vpc-01fe61cb1984e4911",
            //    AvailabilityZones = availabilityZones,
            //    IsolatedSubnetIds = new string[] { "subnet-049369136ecb2bd54", "subnet-07783132d0a53c5e7", "subnet-0d9bbac6f4cd2ff6f" },
            //    IsolatedSubnetRouteTableIds = new string[] { "rtb-049c4bf4db5b9b8ce", "rtb-049c4bf4db5b9b8ce", "rtb-049c4bf4db5b9b8ce" }
            //});
            var subnets = vpc.SelectSubnets(new SubnetSelection
            {
                AvailabilityZones = availabilityZones
            });
            var cluster = new Cluster(this, "MyCluster", new ClusterProps
            {
                ClusterName = "dass-hello-fargate-cluster",
                Vpc = vpc
            });
            //var cluster = Amazon.CDK.AWS.ECS.Cluster.FromClusterAttributes(this, "MyCluster", new Amazon.CDK.AWS.ECS.ClusterAttributes
            //{
            //    ClusterName = "dass-hello-cfst-cluster"//,
            //    //Vpc = vpc
            //});
            //var asg = new AutoScalingGroup(this, "MyAsg", new AutoScalingGroupProps
            //{
            //    Vpc = vpc,

            //    InstanceType = InstanceType.Of(InstanceClass.BURSTABLE3, InstanceSize.MICRO),
            //    MachineImage = new AmazonLinuxImage()
            //});
            var securityGroup = SecurityGroup.FromLookupByName(this, "MySg", "Fargate", vpc);
            var alb = new ApplicationLoadBalancer(this, "MyAlb", new ApplicationLoadBalancerProps
            {
                InternetFacing = false,
                IpAddressType = IpAddressType.IPV4,
                LoadBalancerName = "dass-hello-fargate-alb",
                Vpc = vpc,
                VpcSubnets = new SubnetSelection
                {
                    Subnets = subnets.Subnets.Where(x => x.Ipv4CidrBlock.StartsWith("172.16.0.")).ToList().ToArray(),
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
                Cluster = cluster,
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
