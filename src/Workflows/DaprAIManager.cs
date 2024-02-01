using System.Collections.Concurrent;
using Dapr.AI.Workflows;
using Dapr.Workflow;
using Microsoft.Extensions.Logging;

namespace Dapr.AI;

public sealed class DaprAIManager
{
    private readonly DaprWorkflowClient workflowClient;

    private readonly ILogger<DaprAIManager> logger;

    private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> waitMap = new();

    public DaprAIManager(DaprWorkflowClient workflowClient, ILogger<DaprAIManager> logger)
    {
        this.workflowClient = workflowClient;
        this.logger = logger;
    }

    public async Task StartChatSessionAsync(string sessionId)
    {
        await this.workflowClient.ScheduleNewWorkflowAsync(
            name: nameof(ChatWorkflow),
            instanceId: sessionId,
            input: new ChatHistory());
    }

    public async Task<string> ContinueChatSessionAsync(string sessionId, string prompt)
    {
        var tcs = waitMap.GetOrAdd(sessionId, new TaskCompletionSource<string>());

        await workflowClient.RaiseEventAsync(
            instanceId: sessionId,
            eventName: "Prompt",
            eventPayload: prompt);

        var response = await tcs.Task;

        waitMap.TryRemove(sessionId, out _);

        return response;
    }

    public Task NotifyPromptResponseAsync(string sessionId, string response)
    {
        if (waitMap.TryGetValue(sessionId, out var tcs))
        {
            tcs.SetResult(response);
        }

        return Task.CompletedTask;
    }

    public async Task EndChatSessionAsync(string sessionId)
    {
        await this.workflowClient.TerminateWorkflowAsync(
            instanceId: sessionId);
    }
}
