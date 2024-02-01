using Dapr.AI.Activities;
using Dapr.AI.Workflows;
using Dapr.Workflow;

namespace Dapr.AI;

public static class WorkflowRuntimeOptionsExtensions
{
    public static void RegisterAIWorkflows(this WorkflowRuntimeOptions options)
    {
        options.RegisterWorkflow<ChatWorkflow>();

        options.RegisterActivity<ChatActivity>();
        options.RegisterActivity<NotifyActivity>();
        options.RegisterActivity<PublishResponseActivity>();
    }
}
