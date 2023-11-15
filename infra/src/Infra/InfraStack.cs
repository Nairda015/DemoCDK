using Amazon.CDK;
using Constructs;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.GreengrassV2;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Lambda.EventSources;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.SQS;

namespace Infra
{
    public class InfraStack : Stack
    {
        internal InfraStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {

            var queue = new Queue(this, "lambdatestqueue");
            
            
            var buildOption = new BundlingOptions()
            {
                Image = Runtime.DOTNET_6.BundlingImage,
                User = "root",
                OutputType = BundlingOutput.ARCHIVED,
                Command = new string[]{
               "/bin/sh",
                "-c",
                " dotnet tool install -g Amazon.Lambda.Tools"+
                " && dotnet build"+
                " && dotnet lambda package --output-package /asset-output/function.zip"
                }
            };

            
            //HTTP Example
            var lambdaFunctionOne = new Function(this, "FunctionsDefault", new FunctionProps
            {
                Runtime = Runtime.DOTNET_6,
                MemorySize = 512,
                LogRetention = RetentionDays.ONE_DAY,
                Handler = "FunctionOne::FunctionOne.Functions_Default_Generated::Default",
                Code = Code.FromAsset("../apps/src/FunctionOne/src/FunctionOne/", new Amazon.CDK.AWS.S3.Assets.AssetOptions
                {
                    Bundling = buildOption
                }),
            });

            //Proxy all request from the root path "/" to Lambda Function One
            var restAPI = new LambdaRestApi(this, "Endpoint", new LambdaRestApiProps
            {
                Handler = lambdaFunctionOne,
                Proxy = true,
            });
            restAPI.Root.AddMethod("GET");
            
            //SQS Example 
            var lambdaSqsFunction = new Function(this, "SqsHandler", new FunctionProps
            {
                Runtime = Runtime.DOTNET_6,
                MemorySize = 1024,
                LogRetention = RetentionDays.ONE_DAY,
                Handler = "FunctionOne::FunctionOne.SqsHandler_Handler_Generated::Handler",
                Code = Code.FromAsset("../apps/src/FunctionOne/src/FunctionOne/", new Amazon.CDK.AWS.S3.Assets.AssetOptions
                {
                    Bundling = buildOption
                }),
            });

            var eventSource = new SqsEventSource(queue);
            lambdaSqsFunction.AddEventSource(eventSource);


            new CfnOutput(this, "apigwtarn", new CfnOutputProps { Value = restAPI.ArnForExecuteApi() });
        }
    }
}
