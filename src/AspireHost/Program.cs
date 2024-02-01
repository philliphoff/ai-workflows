var builder = DistributedApplication.CreateBuilder(args);

builder
    .AddProject<Projects.WorkflowsService>("workflows")
    .WithDaprSidecar();

using var app = builder.Build();

await app.RunAsync();
