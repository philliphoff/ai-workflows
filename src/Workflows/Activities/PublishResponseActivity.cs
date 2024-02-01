using System.Text.Json.Serialization;
using Dapr.Client;
using Dapr.Workflow;

namespace Dapr.AI.Activities;

public sealed record PublishResponseRequest([property: JsonPropertyName("message")] string Message);

public sealed record PublishedResponse(
    [property: JsonPropertyName("instanceId")] string InstanceId,
    [property: JsonPropertyName("message")] string Message);

public sealed class PublishResponseActivity : WorkflowActivity<PublishResponseRequest, Unit>
{
    private readonly DaprClient daprClient;

    public PublishResponseActivity(DaprClient daprClient)
    {
        this.daprClient = daprClient;
    }

    public override async Task<Unit> RunAsync(WorkflowActivityContext context, PublishResponseRequest input)
    {
        await this.daprClient.PublishEventAsync("pubsub", "session-response", new PublishedResponse(context.InstanceId, input.Message));

        return Unit.Default;
    }
}
