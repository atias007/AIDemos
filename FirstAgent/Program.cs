using OpenAI;
using OpenAI.Chat;

var openAiApiKey = Environment.GetEnvironmentVariable("OPEN_AI_API_KEY") ?? string.Empty;
var agent = new OpenAIClient(openAiApiKey);
var chat = agent.GetChatClient("gpt-4o");

var messages = new ChatMessage[]
{
    new SystemChatMessage("You are a helpful assistant who is extremely competent as a Computer Scientist! Your name is Rob."),
    new UserChatMessage("who was the very first computer scientist?")
};

await Console.Out.WriteLineAsync("thinking...");
//var response = await chat.CompleteChatAsync(messages);
//await Console.Out.WriteLineAsync("🧠 OpenAI Response:\r\n" + response.Value.Content.First().Text);

var response = chat.CompleteChatStreamingAsync(messages);
await Console.Out.WriteLineAsync("🧠 OpenAI Response:");
await foreach (var message in response)
{
    await Console.Out.WriteAsync(message.ContentUpdate.FirstOrDefault()?.Text);
}