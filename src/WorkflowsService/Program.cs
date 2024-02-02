using System.Collections.Concurrent;
using System.Text.Json.Serialization;
using Dapr.AI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDaprAIWorkflows();

var app = builder.Build();

app.UseDaprAIWorkflows();

var waitMap = new ConcurrentDictionary<string, TaskCompletionSource<string>>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/start-session", async (StartSessionRequest request, DaprAIManager aiManager) =>
{
    await aiManager.StartChatSessionAsync(request.InstanceId);
})
.WithName("StartSession")
.WithOpenApi();

app.MapPost("/continue-session", async (ContinueSessionRequest request, DaprAIManager aIManager) =>
{
    var response = await aIManager.ContinueChatSessionAsync(
        sessionId: request.InstanceId,
        prompt: request.Prompt);

    return response;
})
.WithName("ContinueSession")
.WithOpenApi();

app.MapPost("/end-session", async (EndSessionRequest request, DaprAIManager aIManager) =>
{
    await aIManager.EndChatSessionAsync(request.InstanceId);
})
.WithName("EndSession")
.WithOpenApi();

app.Run();

sealed record StartSessionRequest([property: JsonPropertyName("instanceId")] string InstanceId);

sealed record ContinueSessionRequest(
    [property: JsonPropertyName("instanceId")] string InstanceId,
    [property: JsonPropertyName("prompt")] string Prompt);

sealed record EndSessionRequest([property: JsonPropertyName("instanceId")] string InstanceId);
