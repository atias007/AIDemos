using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Polly;

#region 1. Prepare

var _retry = Policy
    .Handle<Exception>()
    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(1));

var handler = new HttpClientHandler
{
    CheckCertificateRevocationList = false
};
using var httpClient = new HttpClient(handler);

#endregion 1. Prepare

#region 2. OpenAI Configuration

var apiKey = Environment.GetEnvironmentVariable("OPEN_AI_API_KEY") ?? throw new ArgumentNullException();
const string modelId = "gpt-4.1";

#endregion 2. OpenAI Configuration

#region 3. Create Agent

// OPENAI:
var agent = Kernel
    .CreateBuilder()
    .AddOpenAIChatCompletion(modelId, apiKey, httpClient: httpClient)
    .Build();

#endregion 3. Create Agent

#region 4. Add MCP Server

await using IMcpClient mcpClient1 = await McpClientFactory.CreateAsync(
        new StdioClientTransport(new()
        {
            Name = "GitMCP",
            Command = "uvx",
            Arguments = ["mcp-server-git", "--repository", @"C:\Planar"]
        }));

var tools1 = await mcpClient1.ListToolsAsync();

#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
agent.Plugins.AddFromFunctions("Git", tools1.Select(aiFunction => aiFunction.AsKernelFunction()));
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

#endregion 4. Add MCP Server

#region 4.1 Add Another MCP server

await using IMcpClient mcpClient2 = await McpClientFactory.CreateAsync(
        new StdioClientTransport(new()
        {
            Name = "FileSystemMCP",
            Command = "npx",
            Arguments = ["-y", "@modelcontextprotocol/server-filesystem", @"C:\Planar"]
        }));

var tools2 = await mcpClient2.ListToolsAsync();

#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
agent.Plugins.AddFromFunctions("FS", tools2.Select(aiFunction => aiFunction.AsKernelFunction()));
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

#endregion 4.1 Add Another MCP server

#region 5. DEMO: list MCP tools

////foreach (var tool in tools1)
////{
////    Console.WriteLine($"Tool: {tool.Name} - {tool.Description}");
////}

#endregion 5. DEMO: list MCP tools

#region 6. DEMO: Chat with LLM & MCP Server

string? userInput;
var history = new ChatHistory();
var chatCompletionService = agent.GetRequiredService<IChatCompletionService>();

var openAIPromptExecutionSettings = new OpenAIPromptExecutionSettings
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Required()
};

do
{
    Console.Write("User > ");
    userInput = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(userInput)) { break; }

    // Add user input
    history.AddUserMessage(userInput);

    // Get the response from the AI
    var result = await _retry.ExecuteAsync(() => chatCompletionService.GetChatMessageContentAsync(
        history,
        executionSettings: openAIPromptExecutionSettings,
        kernel: agent));

    // Print the results
    Console.WriteLine("Assistant > " + result);

    // Add the message from the agent to the chat history
    history.AddMessage(result.Role, result.Content ?? string.Empty);
} while (userInput is not null);

#endregion 6. DEMO: Chat with LLM & MCP Server

Console.WriteLine("--- THE END ---");
Console.ReadLine();