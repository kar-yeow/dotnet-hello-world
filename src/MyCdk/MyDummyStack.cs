using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Constructs;

namespace MyCdk
{
    public class MyDummyStack : Stack
    {
        public MyDummyStack(Construct scope, string id, IStackProps? props = null) : base(scope,id, props)
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

            var subnets = new ISubnet[]
            {
                Subnet.FromSubnetAttributes(this, "MySubnet", new SubnetAttributes
                {
                    AvailabilityZone = "ap-southeast-2a"
                }),
                Subnet.FromSubnetAttributes(this, "MySubnet", new SubnetAttributes
                {
                    AvailabilityZone = "ap-southeast-2b"
                })
            };

            _ = new MyCheckingStack(scope, subnets, "MyCheckingStack", props);
        }
    }
}
