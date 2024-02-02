using Dapr.AI.Activities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Dapr.AI;

public static class WebApplicationExtensions
{
    public static T UseDaprAIWorkflows<T>(this T app)
        where T : IApplicationBuilder, IEndpointRouteBuilder
    {
        app.UseCloudEvents();
        app.MapSubscribeHandler();

        app.MapPost("/subscriptions/session-response", [Topic("pubsub", "session-response")] async (PublishedResponse response, DaprAIManager aiManager) =>
        {
            await aiManager.NotifyPromptResponseAsync(response.InstanceId, response.Message);
        });

        return app;
    }
}
