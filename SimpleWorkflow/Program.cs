using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using OpenAI.Chat;

var openAiApiKey = Environment.GetEnvironmentVariable("OPEN_AI_API_KEY") ?? throw new ArgumentNullException();
var ollamaUri = new Uri("http://127.0.0.1:11434");
var model1 = "gpt-oss:20b-cloud";
var model2 = "gpt-4o-mini";

var chatClient1 = new OllamaSharp.OllamaApiClient(ollamaUri, model1) as IChatClient;
AIAgent writer = new ChatClientAgent(
    chatClient1,
    new ChatClientAgentOptions
    {
        Name = "Writer",
        Instructions = "Write stories that are engaging and creative."
    });

//var response = await writer.RunAsync("Write a short story about a haunted house.");
//Console.WriteLine(response.Text);

var chatClient2 = new OpenAI.Chat.ChatClient(model2, openAiApiKey).AsIChatClient();
// Create a specialized editor agent
AIAgent editor = new ChatClientAgent(
    chatClient2,
    new ChatClientAgentOptions
    {
        Name = "Editor",
        Instructions = "Make the story more engaging, fix grammar, and enhance the plot."
    });

// Create a workflow that connects writer to editor
var workflow = AgentWorkflowBuilder.BuildSequential(writer, editor);
var workflowAgent = workflow.AsAgent();
var workflowResponse = await workflowAgent.RunAsync("Write a short story about a haunted house.");

Console.WriteLine(workflowResponse.Text);

Console.ReadLine();