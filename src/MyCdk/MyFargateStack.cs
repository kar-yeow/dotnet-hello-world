using Amazon.CDK;
using Constructs;
using Amazon.CDK.AWS.AppRunner;
using static Amazon.CDK.AWS.AppRunner.CfnService;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.CloudWatch;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ECS.Patterns;
using Amazon.CDK.AWS.ElasticLoadBalancingV2;
using ApplicationLoadBalancerProps = Amazon.CDK.AWS.ElasticLoadBalancingV2.ApplicationLoadBalancerProps;

namespace MyCdk
{
    public class MyFargateStack : Stack
    {
        public MyFargateStack(Construct scope, string id, IStackProps? props = null) : base(scope, id, props)
        {
            var role = Role.FromRoleName(this, "MyTaskRole", "ato-role-dass-ecs");
            var vpc = Vpc.FromLookup(this, "MyVpc", new VpcLookupOptions
            {
                VpcName = "ato-dass-dev",
                Region = this.Region,
                OwnerAccountId = this.Account,
                SubnetGroupNameTag = "Public subnet"
            });
            var availabilityZones = new string[] { "ap-southeast-2" };
            //var vpc = Vpc.FromVpcAttributes(this, "MyVpc", new VpcAttributes
            //{
            //    VpcId = "vpc-01fe61cb1984e4911",
            //    AvailabilityZones = availabilityZones,
            //    IsolatedSubnetIds = new string[] { "subnet-049369136ecb2bd54" }
            //});
            //var subnets = vpc.SelectSubnets(new SubnetSelection
            //{
            //    AvailabilityZones = availabilityZones,
            //    SubnetType = SubnetType.PRIVATE_ISOLATED
            //}).Subnets;
            var cluster = new Amazon.CDK.AWS.ECS.Cluster(this, "MyCluster", new Amazon.CDK.AWS.ECS.ClusterProps
            {
                ClusterName = "dass-hello-ctst-cdk-cluster"
            });
            //var cluster = Amazon.CDK.AWS.ECS.Cluster.FromClusterAttributes(this, "MyCluster", new Amazon.CDK.AWS.ECS.ClusterAttributes
            //{
            //    ClusterName = "dass-hello-cfst-cluster"//,
            //    //Vpc = vpc
            //});
            var securityGroup = SecurityGroup.FromSecurityGroupId(this, "MySg", "sg-0c5054efb308b6f24");
            var alb = new ApplicationLoadBalancer(this, "MyAlb", new ApplicationLoadBalancerProps
            {
                InternetFacing = false,
                IpAddressType = IpAddressType.IPV4,
                LoadBalancerName = "dass-hello-cdk-alb",
                Vpc = vpc,
                //SecurityGroup = securityGroup,
            });
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
                    Image = ContainerImage.FromRegistry($"{Account}.dkr.ecr.{Region}.amazonaws.com/hellotest/dotnet-hello-world:0.0.1"),
                    ContainerName = "dotnet-hello-world",
                    ContainerPort = 5050
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
                ListenerPort = 5050,
                Protocol = ApplicationProtocol.HTTP,
                LoadBalancer = alb,
                ServiceName = "dass-fargate-service"
            });

            _ = new CfnOutput(this, "MyFargateStack", new CfnOutputProps 
            { 
                Value = $"my fargate stack {service.LoadBalancer}"
            });
        }
    }
}
