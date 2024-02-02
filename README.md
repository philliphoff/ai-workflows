# AI Workflow Samples

A collection of samples demonstrating how (Dapr) workflows can be utilized for AI.

## Prerequisites

  - Dapr 1.12 or later
  - .NET 8 or later
  - Azure subscription and Azure Open AI resource

## Running the (Chat) Workflow

  1. In a terminal, export the following environment variables:
     - *`AZURE_OPEN_AI_ENDPOINT`:* The endpoint for the Azure Open AI resource
     - *`AZURE_OPEN_AI_DEPLOYMENT`:* The name of a deployed Azure Open AI model
     - *`AZURE_OPEN_AI_KEY`:* A key for the Azure Open AI resource

  1. Start the Aspire host:

     ```bash
     cd src/AspireHost
     dotnet run
     ```

  1. Open the Aspire dashboard (listed in the output) to identify the workflow service Swagger endpoint
  1. Execute the `start-session` endpoint (using an arbitrary `instanceId`)
  1. Execute the `continue-session` endpoint (using the same `instanceId` and an arbitrary `prompt`)
     - Notice the response body is the AI response to the prompt
  1. Repeat the prior step as often as desired
     - Notice that the AI retains context from prior prompts (e.g. your name, if provided)
  1. Execute the `end-session` endpoint (using the same `instanceId`)