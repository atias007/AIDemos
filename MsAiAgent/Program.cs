using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using ModelContextProtocol.Client;
using MsAiAgent;
using OpenAI;
using OpenAI.Responses;
using System.Net.Mime;
using System.Net.NetworkInformation;
using System.Text.Json;

string key = Environment.GetEnvironmentVariable("OPEN_AI_API_KEY") ?? throw new InvalidDataException("api key is null");
string model = "gpt-4o";

//await PromptDemo(key, model);
//await ChatDemo(key, model);
//await ImageDemo(key, model);
//await ThreadDemo(key, model);
//await ToolsDemo(key, model);
//await AdvanceToolsDemo(key, model);
//await ToolsWithLoggerDemo(key, model);
//await ToolsWithHumenInTheLoop(key, model);
//await StructuredResponseDemo1(key, model);
//await StructuredResponseDemo2(key);

static async Task PromptDemo(string key, string model)
{
    AIAgent agent = new OpenAIClient(key)
        .GetChatClient(model)
        .CreateAIAgent("you are proffesional joke teller");

    var response = await agent.RunAsync("Tell me joke about pirate");
    Console.WriteLine(response);
}

static async Task ChatDemo(string key, string model)
{
    var systemMessage = new ChatMessage(ChatRole.System,
        """
        If the user ask you to tell a joke, refuse to do so, explain that you are not a clown.
        Offer the user as intresting fact about astronomy instead.
        """);

    var userMessage = new ChatMessage(ChatRole.User, "Tell me joke about pirate");
    var messages = new List<ChatMessage> { systemMessage, userMessage };

    AIAgent agent = new OpenAIClient(key)
        .GetChatClient(model)
        .CreateAIAgent("you are proffesional joke teller");

    var responseStream = agent.RunStreamingAsync(messages);
    await foreach (var item in responseStream)
    {
        Console.Write(item);
    }
}

static async Task ImageDemo(string key, string model)
{
    AIAgent agent = new OpenAIClient(key)
        .GetChatClient(model)
        .CreateAIAgent("you are helpful agent that can analyze images");

    var message = new ChatMessage(ChatRole.User,
        [
            new TextContent("What do you see in this picture?"),
            new UriContent("https://www.dogster.com/wp-content/uploads/2024/04/german-shepherd-dog-standing-at-the-park_Bildagentur-Zoonar-GmbH_Shutterstock.jpg", MediaTypeNames.Image.Jpeg)
        ]);

    Console.WriteLine(await agent.RunAsync(message));
}

static async Task ThreadDemo(string key, string model)
{
    AIAgent agent = new OpenAIClient(key)
       .GetChatClient(model)
       .CreateAIAgent("you are proffesional joke teller");

    var thread1 = agent.GetNewThread();
    Console.WriteLine(await agent.RunAsync("Tell me a joke about c# developer", thread1));
    Console.WriteLine("----------------");
    Console.WriteLine(await agent.RunAsync("now add some emojis to the joke and tell it in the voice of robot", thread1));
}

static async Task ToolsDemo(string key, string model)
{
    Console.WriteLine("thinking...");
    AIAgent agent = new OpenAIClient(key)
       .GetChatClient(model)
       .CreateAIAgent("you are helful assistant", tools: Tools.AiFunctions);

    Console.WriteLine(await agent.RunAsync("What is the weather like in New York"));
}

static async Task ToolsWithLoggerDemo(string key, string model)
{
    // create console logger
    using var loggerFactory = LoggerFactory.Create(builder =>
    {
        builder
            .AddSimpleConsole(options =>
            {
                options.ColorBehavior = LoggerColorBehavior.Enabled;
            })
            .SetMinimumLevel(LogLevel.Trace);
    });

    Console.WriteLine("thinking...");
    AIAgent agent = new OpenAIClient(key)
       .GetChatClient(model)
       .CreateAIAgent("you are helful assistant", tools: Tools.AiFunctions, loggerFactory: loggerFactory);

    Console.WriteLine(await agent.RunAsync("What is the weather like in New York"));
}

static async Task ToolsWithHumenInTheLoop(string key, string model)
{
#pragma warning disable MEAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    var tool = new ApprovalRequiredAIFunction(AIFunctionFactory.Create(Tools.GetWeather));

    Console.WriteLine("thinking...");
    AIAgent agent = new OpenAIClient(key)
       .GetChatClient(model)
       .CreateAIAgent("you are helful assistant", tools: [tool]);

    var thread = agent.GetNewThread();
    var response = await agent.RunAsync("What is the weather like in Tel Aviv", thread);

    // -------- //

    var approveRequest = response.Messages
        .SelectMany(m => m.Contents)
        .OfType<FunctionApprovalRequestContent>()
        .ToList();

    foreach (var request in approveRequest)
    {
        Console.WriteLine($"We required approval to execute {request.FunctionCall.Name} with arguments {string.Join(", ", request.FunctionCall.Arguments!)}");
        Console.WriteLine("To execute press [Y], to cancel press [N]");
        var k = Console.ReadKey(true);
        Console.WriteLine("thinking...");
        var approve = k.Key == ConsoleKey.Y;
        var approvedMessage = new ChatMessage(ChatRole.User, [request.CreateResponse(approve)]);
        Console.WriteLine(await agent.RunAsync(approvedMessage, thread));
    }

#pragma warning restore MEAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
}

static async Task AdvanceToolsDemo(string key, string model)
{
    Console.WriteLine("load mcp server...");

    #region 1. Add MCP Server

    await using McpClient mcpClient1 = await McpClient.CreateAsync(
            new StdioClientTransport(new()
            {
                Name = "GitMCP",
                Command = "uvx",
                Arguments = ["mcp-server-git", "--repository", @"C:\Planar"]
            }));

    var tools = await mcpClient1.ListToolsAsync();

    #endregion 1. Add MCP Server

    #region 2. Print tools from MCP Server

    Console.WriteLine("--------------------------");
    Console.WriteLine("List of tools");
    Console.WriteLine("--------------------------");

    foreach (var tool in tools)
    {
        Console.WriteLine($"Tool from MCP Server: {tool.Name} - {tool.Description}");
    }

    Console.WriteLine("--------------------------");
    Console.WriteLine("Press [Enter] to continue");
    Console.WriteLine("--------------------------");
    Console.ReadLine();

    #endregion 2. Print tools from MCP Server

    var agentTools = tools.Cast<AITool>();

    #region EXTRA: Agent as tools

    // AIAgent tripAdvisorAgent = null;
    // var tripAdvisorTools = tripAdvisorAgent.AsAIFunction();

    #endregion EXTRA: Agent as tools

    Console.WriteLine("thinking...");
    AIAgent agent = new OpenAIClient(key)
       .GetChatClient(model)
       .CreateAIAgent("you are git assistance", tools: [.. agentTools]);

    Console.WriteLine(await agent.RunAsync(@"Summarize the last 2 commits as local folder c:\planar"));
}

static async Task StructuredResponseDemo1(string key, string model)
{
    AIAgent agent = new OpenAIClient(key)
        .GetChatClient(model)
        .CreateAIAgent("you are helpful agent that can analyze images and extract text from image");

    var image = File.ReadAllBytes(@"c:\temp\image2.jpg");
    var message = new ChatMessage(ChatRole.User,
        [
            new TextContent(
                """
                is this picture is a valid israeli identifier?
                return json response with the following fields:
                - isValid: boolean that represent a valid israeli identifier
                - idNumber: text that represent the identifier number from the picture, null if the picture is not valid israeli identifier
                - quality: integer number between 1 to 100 represent a score for the quality and sharpness of the picture
                """),
            new DataContent(image, MediaTypeNames.Image.Jpeg)
        ]);

    Console.WriteLine(await agent.RunAsync(message));
}

static async Task StructuredResponseDemo2(string key)
{
    var options = new ChatOptions
    {
        MaxOutputTokens = 1000,
        ResponseFormat = ChatResponseFormat.ForJsonSchema(
            schema: IdInfo.Schema,
            schemaName: nameof(IdInfo),
            schemaDescription: "Information about israeli identifier image include: id number, image quality, is valid"),
        //Temperature = 0.5f
    };

    var chatOptions = new ChatClientAgentOptions
    {
        Name = "helpful assistant",
        Instructions = "you are helpful agent that can analyze images and extract text from image.",
        ChatOptions = options
    };

    AIAgent agent = new OpenAIClient(key)
        .GetChatClient("gpt-5")
        .CreateAIAgent(chatOptions);

    var image = File.ReadAllBytes(@"c:\temp\image2.jpg");

    var message = new ChatMessage(ChatRole.User,
        [
            new TextContent(
                """
                is this picture is a valid israeli identifier?
                include in the response:
                - identifier number from the picture if it was valid.
                - integer number between 1 to 100 represent a score for the quality and sharpness of the picture.
                """),
            new DataContent(image, MediaTypeNames.Image.Jpeg)
        ]);

    var response = await agent.RunAsync(message);

    var idInfo = response.Deserialize<IdInfo>(JsonSerializerOptions.Web);
    Console.WriteLine(idInfo);
}