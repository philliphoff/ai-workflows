using Dapr.Workflow;

namespace Dapr.AI.Activities;

public sealed record Unit()
{
    public static Unit Default { get; } = new Unit();
}

public sealed record ActivityInput(string Message);

public sealed class NotifyActivity : WorkflowActivity<ActivityInput, Unit>
{
    public override Task<Unit> RunAsync(WorkflowActivityContext context, ActivityInput input)
    {
        Console.WriteLine(input.Message);

        return Task.FromResult(Unit.Default);
    }
}
