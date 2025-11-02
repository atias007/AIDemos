using LangChain.Providers;
using LangChain.Providers.OpenAI;
using LangChain.Providers.OpenAI.Predefined;

var openAiApiKey = Environment.GetEnvironmentVariable("OPEN_AI_API_KEY") ?? throw new ArgumentNullException();

var provider = new OpenAiProvider(openAiApiKey);
var llm = new OpenAiLatestFastChatModel(provider);

////var request = new ChatRequest
////{
////    Messages =
////    [
////        new Message { Content = "You are a helpful assistant who is extremely competent as a Computer Scientist! Your name is Rob.", Role = MessageRole.System },
////        new Message { Content = "who was the very first computer scientist?", Role = MessageRole.Human }
////    ]
////};

////var response = await llm.GenerateAsync(request);
////await Console.Out.WriteLineAsync("🧠 OpenAI Response: " + response.Messages[response.Messages.Count - 1].Content);

var messages = new List<Message>
{
    new() { Content = "You are a helpful assistant who is extremely competent as a Computer Scientist! Your name is Rob.", Role = MessageRole.System },
};

var input = string.Empty;
ChatResponse? response = null;
while (!string.Equals(input, "exit", StringComparison.OrdinalIgnoreCase))
{
    Console.Write("Enter a message (type 'exit' to quit): ");
    input = Console.ReadLine() ?? string.Empty;
    if (string.Equals(input, "exit", StringComparison.OrdinalIgnoreCase)) { break; }

    if (response != null) { messages = response.Messages.ToList(); }

    var message = new Message { Content = input, Role = MessageRole.Human };
    messages.Add(message);
    var request = new ChatRequest { Messages = messages };

    response = await llm.GenerateAsync(request);
    await Console.Out.WriteLineAsync("🧠 OpenAI Response: " + response.Messages[response.Messages.Count - 1].Content);
}