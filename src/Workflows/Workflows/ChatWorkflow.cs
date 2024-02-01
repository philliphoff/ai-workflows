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

        var prompt = await context.WaitForExternalEventAsync<string>(
            eventName: "Prompt");

        var newHistory = input with
        {
            Items = input.Items.Append(new ChatHistoryItem("User", prompt)).ToArray()
        };

        await context.CallActivityAsync(
            name: nameof(NotifyActivity),
            input: new ActivityInput($"Prompt received: {prompt}"));

        await context.CallActivityAsync(
            name: nameof(PublishResponseActivity),
            input: new PublishResponseRequest("Response received!"));

        context.ContinueAsNew(newHistory);

        return Unit.Default;
    }
}
