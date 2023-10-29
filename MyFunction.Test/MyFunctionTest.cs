using Amazon.ConfigService.Model;
using Amazon.Lambda.Core;
using MyRuleFunction;

namespace MyFunction.Test
{

    public class MyLambdaLogger : ILambdaLogger
    {
        public void Log(string message)
        {
            Console.WriteLine(message);
        }

        public void LogLine(string message)
        {
            Console.WriteLine(message);
        }
    }

    public class MyLambdaContext : ILambdaContext
    {
        public string AwsRequestId => throw new NotImplementedException();

        public IClientContext ClientContext => throw new NotImplementedException();

        public string FunctionName => throw new NotImplementedException();

        public string FunctionVersion => throw new NotImplementedException();

        public ICognitoIdentity Identity => throw new NotImplementedException();

        public string InvokedFunctionArn => throw new NotImplementedException();

        public ILambdaLogger Logger => new MyLambdaLogger();

        public string LogGroupName => throw new NotImplementedException();

        public string LogStreamName => throw new NotImplementedException();

        public int MemoryLimitInMB => throw new NotImplementedException();

        public TimeSpan RemainingTime => throw new NotImplementedException();
    }

    public class MyDummyNonComplianceLambda : DummyNonComplianceLambda
    {
        public PutEvaluationsRequest? EvaluationsRequest {get;set;}
        protected override async Task<PutEvaluationsResponse> PutEvaluationsAsync(PutEvaluationsRequest request)
        {
            EvaluationsRequest = request;
            return new PutEvaluationsResponse
            {
                HttpStatusCode = System.Net.HttpStatusCode.OK,
            };
        }
    }

    public class MyFunctionTest
    {
        [Fact]
        public async Task ShouldHandleRequest()
        {
            var lambda = new MyDummyNonComplianceLambda();
            await lambda.HandleRequest(new Amazon.Lambda.ConfigEvents.ConfigEvent
            {
                ResultToken = "ResultToken",
                AccountId = "012345678901",
                ConfigRuleId = "1",
                ConfigRuleArn = "arn:rule",
                EventLeftScope = false,
                ConfigRuleName = "test-rule",
                ExecutionRoleArn = "arn:exec",
                RuleParameters = "",
                InvokingEvent = @"
{
    ""configurationItemDiff"": null,
    ""configurationItem"": {
        ""relatedEvents"": [],
        ""relationships"": [],
        ""configuration"": {
            ""path"": ""/"",
            ""roleName"": ""CloudWatch-CrossAccountSharingRole"",
            ""roleId"": ""AROAQVOAJSGCSYD636VOZ"",
            ""arn"": ""arn:aws:iam::046037307781:role/CloudWatch-CrossAccountSharingRole"",
            ""createDate"": ""2023-09-21T06:35:38.000Z"",
            ""assumeRolePolicyDocument"": ""%7B%22Version%22%3A%222012-10-17%22%2C%22Statement%22%3A%5B%7B%22Effect%22%3A%22Allow%22%2C%22Principal%22%3A%7B%22AWS%22%3A%22%2A%22%7D%2C%22Action%22%3A%22sts%3AAssumeRole%22%2C%22Condition%22%3A%7B%22StringEquals%22%3A%7B%22aws%3APrincipalOrgID%22%3A%22o-e6jhzh5669%22%7D%7D%7D%5D%7D"",
            ""instanceProfileList"": [],
            ""rolePolicyList"": [
                {
                    ""policyName"": ""ato-policy-cloudwatch-automatic-dashboards"",
                    ""policyDocument"": ""%7B%22Version%22%3A%222012-10-17%22%2C%22Statement%22%3A%5B%7B%22Action%22%3A%5B%22autoscaling%3ADescribeAutoScalingGroups%22%2C%22cloudfront%3AGetDistribution%22%2C%22cloudfront%3AListDistributions%22%2C%22dynamodb%3ADescribeTable%22%2C%22dynamodb%3AListTables%22%2C%22ec2%3ADescribeInstances%22%2C%22ec2%3ADescribeVolumes%22%2C%22ecs%3ADescribeClusters%22%2C%22ecs%3ADescribeContainerInstances%22%2C%22ecs%3AListClusters%22%2C%22ecs%3AListContainerInstances%22%2C%22ecs%3AListServices%22%2C%22elasticache%3ADescribeCacheClusters%22%2C%22elasticbeanstalk%3ADescribeEnvironments%22%2C%22elasticfilesystem%3ADescribeFileSystems%22%2C%22elasticloadbalancing%3ADescribeLoadBalancers%22%2C%22kinesis%3ADescribeStream%22%2C%22kinesis%3AListStreams%22%2C%22lambda%3AGetFunction%22%2C%22lambda%3AListFunctions%22%2C%22rds%3ADescribeDBClusters%22%2C%22rds%3ADescribeDBInstances%22%2C%22resource-groups%3AListGroupResources%22%2C%22resource-groups%3AListGroups%22%2C%22route53%3AGetHealthCheck%22%2C%22route53%3AListHealthChecks%22%2C%22s3%3AListAllMyBuckets%22%2C%22s3%3AListBucket%22%2C%22sns%3AListTopics%22%2C%22sqs%3AGetQueueAttributes%22%2C%22sqs%3AGetQueueUrl%22%2C%22sqs%3AListQueues%22%2C%22synthetics%3ADescribeCanariesLastRun%22%2C%22tag%3AGetResources%22%5D%2C%22Effect%22%3A%22Allow%22%2C%22Resource%22%3A%22%2A%22%2C%22Sid%22%3A%22CloudWatchDashboard%22%7D%2C%7B%22Action%22%3A%5B%22apigateway%3AGET%22%5D%2C%22Effect%22%3A%22Allow%22%2C%22Resource%22%3A%5B%22arn%3Aaws%3Aapigateway%3A%2A%3A%3A%2Frestapis%2A%22%5D%7D%5D%7D""
                },
                {
                    ""policyName"": ""ato-policy-cloudwatch-crossaccountsharing"",
                    ""policyDocument"": ""%7B%22Version%22%3A%222012-10-17%22%2C%22Statement%22%3A%5B%7B%22Action%22%3A%5B%22autoscaling%3ADescribe%2A%22%2C%22cloudwatch%3ADescribe%2A%22%2C%22cloudwatch%3AGet%2A%22%2C%22cloudwatch%3AList%2A%22%2C%22logs%3AGet%2A%22%2C%22logs%3AList%2A%22%2C%22logs%3AStartQuery%22%2C%22logs%3AStopQuery%22%2C%22logs%3ADescribe%2A%22%2C%22logs%3ATestMetricFilter%22%2C%22logs%3AFilterLogEvents%22%2C%22sns%3AGet%2A%22%2C%22sns%3AList%2A%22%5D%2C%22Effect%22%3A%22Allow%22%2C%22Resource%22%3A%22%2A%22%2C%22Sid%22%3A%22CloudWatchShareLogs%22%7D%5D%7D""
                }
            ],
            ""attachedManagedPolicies"": [],
            ""permissionsBoundary"": null,
            ""tags"": [],
            ""roleLastUsed"": null
        },
        ""supplementaryConfiguration"": {},
        ""tags"": {},
        ""configurationItemVersion"": ""1.3"",
        ""configurationItemCaptureTime"": ""2023-09-22T06:43:55.603Z"",
        ""configurationStateId"": 1695365035603,
        ""awsAccountId"": ""046037307781"",
        ""configurationItemStatus"": ""ResourceDiscovered"",
        ""resourceType"": ""AWS::IAM::Role"",
        ""resourceId"": ""AROAQVOAJSGCSYD636VOZ"",
        ""resourceName"": ""CloudWatch-CrossAccountSharingRole"",
        ""ARN"": ""arn:aws:iam::046037307781:role/CloudWatch-CrossAccountSharingRole"",
        ""awsRegion"": ""global"",
        ""availabilityZone"": ""Not Applicable"",
        ""configurationStateMd5Hash"": """",
        ""resourceCreationTime"": ""2023-09-21T06:35:38.000Z"",
        ""configurationItemDeliveryTime"": null,
        ""recordingFrequency"": null
    },
    ""notificationCreationTime"": ""2023-10-26T23:59:02.325Z"",
    ""messageType"": ""ConfigurationItemChangeNotification"",
    ""recordVersion"": ""1.3""
}
                "

            }, new MyLambdaContext
            {

            });
            Assert.Equal("ResultToken", lambda.EvaluationsRequest?.ResultToken);
        }
    }
}