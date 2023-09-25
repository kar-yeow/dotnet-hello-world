using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.RDS;
using Amazon.CDK.AWS.S3;
using Constructs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.CDK.AWS.Config;
using Amazon.CDK.AWS.Lambda;

namespace MyCdk
{
    public class MyDummyStack : Stack
    {
        public MyDummyStack(Construct scope, string id, IStackProps? props = null) : base(scope,id, props)
        {
            var  subnetConfigs = new SubnetConfiguration[]
            {
                new SubnetConfiguration
                {
                    SubnetType = SubnetType.PRIVATE_ISOLATED,
                    CidrMask = 22,
                    Name = "isolated - subnet"
                }
            };

            var vpc = new Vpc(this, "developer-vpc", new VpcProps
            {
                EnableDnsSupport = true,
                EnableDnsHostnames = true,
                NatGateways = 0,
                IpAddresses = IpAddresses.Cidr("10.0.0.0/20"),
                SubnetConfiguration = subnetConfigs,
                MaxAzs = 2
            });

            _ = new MyCheckingStack(scope, vpc.IsolatedSubnets, "MyCheckingStack", props);
        }
    }
}
