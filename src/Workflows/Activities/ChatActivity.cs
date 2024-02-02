using Azure;
using Azure.AI.OpenAI;
using Dapr.AI.Workflows;
using Dapr.Workflow;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Dapr.AI.Activities;

public sealed record ChatRequest(string Prompt)
{
    public ChatHistory History { get; init; } = new();
}

public sealed class ChatActivity : WorkflowActivity<ChatRequest, string>
{
    private readonly IConfiguration configuration;
    private readonly ILogger<ChatActivity> logger;

    public ChatActivity(IConfiguration configuration, ILogger<ChatActivity> logger)
    {
        this.configuration = configuration;
        this.logger = logger;
    }

    public override async Task<string> RunAsync(WorkflowActivityContext context, ChatRequest input)
    {
        try
        {
            string endpoint = this.configuration.GetValue<string>("AZURE_OPEN_AI_ENDPOINT") ?? throw new InvalidOperationException("Azure OpenAI endpoint not found.");
            string deployment = this.configuration.GetValue<string>("AZURE_OPEN_AI_DEPLOYMENT") ?? throw new InvalidOperationException("Azure OpenAI deployment not found.");
            string key = this.configuration.GetValue<string>("AZURE_OPEN_AI_KEY") ?? throw new InvalidOperationException("Azure OpenAI key not found.");

            var client = new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));

            var options = new ChatCompletionsOptions
            {
                DeploymentName = deployment,
            };

            foreach (var message in input.History.Items.Select(ToChatRequestMessage))
            {
                options.Messages.Add(message);
            }

            this.logger.LogInformation("Sending chat request to OpenAI with options {Options}...", options);

            var response = await client.GetChatCompletionsAsync(options);

            this.logger.LogInformation("Received response.");

            return response.Value.Choices.First().Message.Content;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error occurred while processing chat request.");

            throw;
        }
    }

    private static ChatRequestMessage ToChatRequestMessage(ChatHistoryItem item)
    {
        return item.Role switch
        {
            "User" => new ChatRequestUserMessage(item.Message),
            "AI" => new ChatRequestAssistantMessage(item.Message),
            _ => throw new InvalidOperationException($"Unknown role: {item.Role}")
        };
    }
}
