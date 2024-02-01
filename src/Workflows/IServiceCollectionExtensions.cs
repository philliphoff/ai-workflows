using Dapr.Workflow;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Dapr.AI;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddDaprAIWorkflows(this IServiceCollection services)
    {
        services.TryAddSingleton<DaprAIManager>();

        services.AddDaprClient();

        return services.AddDaprWorkflow(
            options =>
            {
                options.RegisterAIWorkflows();
            });
    }
}
