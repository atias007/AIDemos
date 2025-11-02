// Import packages
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ModelContextProtocol.Client;
using ModelContextProtocol.SemanticKernel;
using ModelContextProtocol.SemanticKernel.Extensions;
using Polly;
using SemanticKernel;

#region Prepare

var _retry = Policy
    .Handle<Exception>()
    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(1));

var services = new ServiceCollection();
services
    .AddTransient<SupplierPlugin>()
    .AddDbContext<NorthwindContext>(option => option.UseSqlServer("Password=CustomsDev123!;Persist Security Info=True;User ID=sa;Initial Catalog=Northwind;Data Source=localhost;Encrypt=false"), ServiceLifetime.Transient);

var handler = new HttpClientHandler
{
    CheckCertificateRevocationList = false
};
using var httpClient = new HttpClient(handler);

#endregion Prepare

#region OpenAI Configuration

var apiKey = Environment.GetEnvironmentVariable("OPEN_AI_API_KEY") ?? throw new ArgumentNullException();
const string modelId = "gpt-4.1";

#endregion OpenAI Configuration

#region Ollama Configuration

//const string modelId = "llama3.2";
//const string endpoint = "http://localhost:11434";

#endregion Ollama Configuration

#region Create Agent Builder

// OPENAI:
var builder = Kernel
    .CreateBuilder()
    .AddOpenAIChatCompletion(modelId, apiKey, httpClient: httpClient);

// OLLAMA:
//var builder = Kernel
//     .CreateBuilder()
//     .AddOllamaChatCompletion(modelId, new Uri(endpoint));

#endregion Create Agent Builder

#region Add Logging

//builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));

#endregion Add Logging

var agent = builder.Build();

#region Simple Chat1

//var chatResult = await agent.InvokePromptAsync("give me 5 leg exercises to do in gym");
//Console.WriteLine(chatResult.ToString());
//Console.ReadLine();

#endregion Simple Chat1

#region Simple Chat2

//var chatResult2 = agent.InvokePromptStreamingAsync("give me 5 leg exercises to do in gym");
//await foreach (var result in chatResult2)
//{
//    Console.Write(result);
//}

#endregion Simple Chat2

#region Add Plugins

var chatCompletionService = agent.GetRequiredService<IChatCompletionService>();

//var plugin = services.BuildServiceProvider().GetRequiredService<SupplierPlugin>();
//agent.Plugins.AddFromObject(plugin, "Suppliers");

// Enable planning
var openAIPromptExecutionSettings = new OpenAIPromptExecutionSettings
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};

#endregion Add Plugins

#region Add MCP Server

await using IMcpClient mcpClient = await McpClientFactory.CreateAsync(
        new StdioClientTransport(new()
        {
            WorkingDirectory = @"R:\CodeQualityDemo\MCPServer",
            Name = "SupplieMCPServer",
            Command = "dotnet", // or other executable
            Arguments = ["run", "--project", @"MCPServer.csproj"]
        }));

var tools = await mcpClient.ListToolsAsync();
foreach (var tool in tools)
{
    // Console.WriteLine($"Tool: {tool.Name} - {tool.Description}");
}

#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
agent.Plugins.AddFromFunctions("SupplieMCPServer", tools.Select(aiFunction => aiFunction.AsKernelFunction()));
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

Console.ReadLine();

#endregion Add MCP Server

// Create a history store the conversation
var history = new ChatHistory();

// Initiate a back-and-forth chat
string? userInput;
do
{
    // Collect user input
    Console.Write("User > ");
    userInput = Console.ReadLine();

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