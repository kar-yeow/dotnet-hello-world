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
                Subnet.FromSubnetAttributes(this, "MySubnet1", new SubnetAttributes
                {
                    SubnetId = "subnet-049369136ecb2bd54",
                    RouteTableId = "rtb-049c4bf4db5b9b8ce"
                }),
                Subnet.FromSubnetAttributes(this, "MySubnet2", new SubnetAttributes
                {
                    SubnetId = "subnet-06e6594cb233f514b",
                    RouteTableId = "rtb-049c4bf4db5b9b8ce"
                })
            };

            _ = new MyCheckingStack(scope, subnets, "MyCheckingStack", props);
        }
    }
}
