using AIVectorSearch;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.InMemory;
using OpenAI;
using System.ClientModel;
using System.Diagnostics;

// 00 - Configuration
string model = "text-embedding-ada-002";
string key = Environment.GetEnvironmentVariable("OPEN_AI_API_KEY") ?? throw new InvalidDataException("api key is null");

// 01 - Create the embedding generator.
var generator =
    new OpenAIClient(new ApiKeyCredential(key))
      .GetEmbeddingClient(model: model)
      .AsIEmbeddingGenerator();

// 02
//   (A) - Create and populate the vector store.
var vectorStore = new InMemoryVectorStore();
var cloudServicesStore = vectorStore.GetCollection<int, CloudService>("CloudServices");
await cloudServicesStore.EnsureCollectionExistsAsync();

//   (B) - Generate embeddings using LLM and store them with original text.
Console.Write("Start embedding services... ");
var sw = Stopwatch.StartNew();
var embbeded = await generator.GenerateAsync(Data.CloudServices.Select(c => c.Description));
Console.WriteLine($" Done (duration {sw.Elapsed.TotalSeconds:N3})");
var vectors = embbeded.ToList();
foreach (var (i, service) in Data.CloudServices.Index())
{
    service.Vector = vectors[i].Vector;
    await cloudServicesStore.UpsertAsync(service);
}

// 03 - Get search query from user.
//      ex: Which Azure service should I use to store my Word documents?
//      ex: What is the best cloud service for hosting web applications?
//      ex: I need a cloud service that provides virtual machines.
//      ex: where can i store secrets?

Console.Write("Enter your search query:");
var query = Console.ReadLine() ?? string.Empty;

// 04 - Perform a similarity search.
Console.Write("Start embedding query... ");
sw = Stopwatch.StartNew();
var queryVector = await generator.GenerateVectorAsync(query);
Console.WriteLine($" Done (duration {sw.Elapsed.TotalSeconds:N3})");

var results = cloudServicesStore.SearchAsync(queryVector, top: 1);

// 05 - Display results.
Console.WriteLine("---------------------------------------------------");
await foreach (var result in results)
{
    Console.WriteLine($"Match score: {result.Score}");
    Console.WriteLine($"Name:        {result.Record.Name}");
    Console.WriteLine($"Description:\r\n{result.Record.Description}");
}
Console.WriteLine("---------------------------------------------------");

Console.ReadLine();