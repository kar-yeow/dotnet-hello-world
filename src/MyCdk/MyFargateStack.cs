using Amazon.CDK;
using Constructs;
using Amazon.CDK.AWS.AppRunner;
using static Amazon.CDK.AWS.AppRunner.CfnService;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.CloudWatch;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ECS.Patterns;

namespace MyCdk
{
    public class MyFargateStack : Stack
    {
        public MyFargateStack(Construct scope, string id, IStackProps? props = null) : base(scope, id, props)
        {
            var role = Role.FromRoleName(this, "MyTaskRole", "my-task-role", new FromRoleNameOptions
            {
                DefaultPolicyName = "ato-role-dass-ecs"
            });
            //var vpc = Vpc.FromLookup(this, "MyVpc", new VpcLookupOptions
            //{
            //    VpcName = "",
            //    Region = this.Region,
            //    OwnerAccountId = this.Account
            //});
            var cluster = new Cluster(this, "MyCluster", new ClusterProps
            {
                ClusterName = "dass-hello-ctst-cluster"
            });
            var service = new ApplicationLoadBalancedFargateService(this, "MyFargateService", new ApplicationLoadBalancedFargateServiceProps 
            {
                Cluster = cluster,
                //Vpc = vpc,
                TaskImageOptions = new ApplicationLoadBalancedTaskImageOptions
                {
                    Image = ContainerImage.FromRegistry($"{Account}.dkr.ecr.{Region}.amazonaws.com/hellotest/dotnet-hello-world:0.0.1")
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
                ServiceName = "dass-fargate-service",
                
            });

            _ = new CfnOutput(this, "MyFargateStack", new CfnOutputProps 
            { 
                Value = $"my fargate stack {service.LoadBalancer}"
            });
        }
    }
}
