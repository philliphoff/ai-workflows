using Dapr.AI.Activities;
using Dapr.Workflow;

namespace Dapr.AI.Workflows;

public sealed record ChatHistoryItem(string Role, string Message);

public sealed record ChatHistory
{
    public ChatHistoryItem[] Items { get; init; } = Array.Empty<ChatHistoryItem>();
}

public sealed class ChatWorkflow : Workflow<ChatHistory, Unit>
{
    public override async Task<Unit> RunAsync(WorkflowContext context, ChatHistory input)
    {
        await context.CallActivityAsync(
            name: nameof(NotifyActivity),
            input: new ActivityInput("Workflow started!"));

        var latestHistory = input;

        var prompt = await context.WaitForExternalEventAsync<string>(
            eventName: "Prompt");

        await context.CallActivityAsync(
            name: nameof(NotifyActivity),
            input: new ActivityInput($"Prompt received: {prompt}"));

        latestHistory = latestHistory with
        {
            Items =
                latestHistory
                    .Items
                    .Append(new ChatHistoryItem("User", prompt))
                    .ToArray()
        };

        var request = new ChatRequest(prompt)
        {
            History = latestHistory
        };

        var response = await context.CallActivityAsync<string>(
            name: nameof(ChatActivity),
            input: request);

        latestHistory = latestHistory with
        {
            Items =
                latestHistory
                    .Items
                    .Append(new ChatHistoryItem("AI", response))
                    .ToArray()
        };

        await context.CallActivityAsync(
            name: nameof(PublishResponseActivity),
            input: new PublishResponseRequest(response));

        context.ContinueAsNew(latestHistory);

        return Unit.Default;
    }
}
