using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Microsoft.Extensions.Configuration;

namespace FunctionOne;

public class SqsHandler
{
    [LambdaFunction]
    public string Handler(SQSEvent sqsEvent, [FromServices] IConfiguration configuration, ILambdaContext context)
    {
        foreach (var message in sqsEvent.Records)
        {
            context.Logger.LogInformation($"Processed message {message.Body}");
        }
        context.Logger.LogInformation($"Processed message {configuration["HelloKey"]}");
        return "Hello from sqs";
    }
}