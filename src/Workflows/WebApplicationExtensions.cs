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

        return app;
    }
}
