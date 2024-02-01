using System.Text.Json.Serialization;
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
        options.RegisterWorkflow<ChatWorkflow>();

        options.RegisterActivity<NotifyActivity>();
    });

var app = builder.Build();

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
    await workflowClient.RaiseEventAsync(
        instanceId: request.InstanceId,
        eventName: "Prompt",
        eventPayload: request.Prompt);
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

app.Run();

sealed record StartSessionRequest([property: JsonPropertyName("instanceId")] string InstanceId);

sealed record ContinueSessionRequest(
    [property: JsonPropertyName("instanceId")] string InstanceId,
    [property: JsonPropertyName("prompt")] string Prompt);

sealed record EndSessionRequest([property: JsonPropertyName("instanceId")] string InstanceId);
