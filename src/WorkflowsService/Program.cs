using System.Collections.Concurrent;
using System.Text.Json.Serialization;
using Dapr;
using Dapr.AI;
using Dapr.AI.Activities;
using Dapr.AI.Workflows;
using Dapr.Workflow;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDaprClient();

builder.Services.AddDaprWorkflow(
    options =>
    {
        options.RegisterAIWorkflows();
    });

var app = builder.Build();

app.UseCloudEvents();
app.MapSubscribeHandler();

var waitMap = new ConcurrentDictionary<string, TaskCompletionSource<string>>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/start-session", async (StartSessionRequest request, DaprWorkflowClient workflowClient) =>
{
    await workflowClient.ScheduleNewWorkflowAsync(
        name: nameof(ChatWorkflow),
        instanceId: request.InstanceId,
        input: new ChatHistory());
})
.WithName("StartSession")
.WithOpenApi();

app.MapPost("/continue-session", async (ContinueSessionRequest request, DaprWorkflowClient workflowClient) =>
{
    var tcs = waitMap.GetOrAdd(request.InstanceId, new TaskCompletionSource<string>());

    await workflowClient.RaiseEventAsync(
        instanceId: request.InstanceId,
        eventName: "Prompt",
        eventPayload: request.Prompt);

    var response = await tcs.Task;

    waitMap.TryRemove(request.InstanceId, out _);

    return response;
})
.WithName("ContinueSession")
.WithOpenApi();

app.MapPost("/end-session", async (EndSessionRequest request, DaprWorkflowClient workflowClient) =>
{
    await workflowClient.TerminateWorkflowAsync(
        instanceId: request.InstanceId);
})
.WithName("EndSession")
.WithOpenApi();

app.MapPost("/subscriptions/session-response", [Topic("pubsub", "session-response")] (PublishedResponse response) =>
{
    if (waitMap.TryGetValue(response.InstanceId, out var tcs))
    {
        tcs.SetResult(response.Message);
    }
});

app.Run();

sealed record StartSessionRequest([property: JsonPropertyName("instanceId")] string InstanceId);

sealed record ContinueSessionRequest(
    [property: JsonPropertyName("instanceId")] string InstanceId,
    [property: JsonPropertyName("prompt")] string Prompt);

sealed record EndSessionRequest([property: JsonPropertyName("instanceId")] string InstanceId);
