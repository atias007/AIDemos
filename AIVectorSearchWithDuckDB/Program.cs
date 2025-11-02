using AIVectorSearchWithDuckDB;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Embeddings;
using RepoDb;

// Initialize repodb
GlobalConfiguration.Setup().UseSqlServer();

Console.WriteLine("1 - Index Data");
Console.WriteLine("2 - Search");
Console.WriteLine("---------------------");
Console.Write("Select option: ");
var option = Console.ReadLine();
Console.WriteLine();

var embeddClient = InitializeEmbeddingClient();
if (option == "1")
{
    await IndexDataAsync(embeddClient);
}
else if (option == "2")
{
    await SearchAsync(embeddClient);
}
else
{
    Console.WriteLine("Invalid option");
}

Console.WriteLine("Done");
Console.ReadLine();

// EX1: Employee that has BA education in psychology
// EX2: Degree in spanish or english
async Task SearchAsync(EmbeddingClient embeddClient)
{
    Console.Write("Ask question: ");
    var question = Console.ReadLine() ?? string.Empty;

    // Get the embedding for the question
    var questionVector = await CreateSingleEmbeddings(question, embeddClient);

    // Search for similar vectors in the database
    var result = VectorDb.SearchVectors(questionVector, 3);

    // Display the results
    await foreach (var r in result)
    {
        Console.WriteLine($"Found. {r.EmployeeID}: {r.FirstName} {r.LastName} ({r.Distance})");
    }
}

async Task IndexDataAsync(EmbeddingClient embeddClient)
{
    var employees = await SqlDb.GetEmployeesAsync();
    foreach (var e in employees)
    {
        var chunks = ChunksUtil.SplitIntoChunks(e.ToString(), 1536, 300);
        var embeddings = await CreateEmbeddings(chunks, embeddClient);
        e.Vectors = embeddings;

        await VectorDb.InsertVector(e);
    }
}

static EmbeddingClient InitializeEmbeddingClient()
{
    // Initialize OpenAI client
    const string model = "text-embedding-3-small";
    var openAIKey = Environment.GetEnvironmentVariable("OPEN_AI_API_KEY") ?? throw new InvalidDataException("api key is null");
    var openAIClient = new OpenAIClient(openAIKey);
    var embeddingClient = openAIClient.GetEmbeddingClient(model);
    Console.WriteLine("✓ Initialized OpenAI client\n");
    return embeddingClient;
}

static async Task<IEnumerable<float[]>> CreateEmbeddings(IEnumerable<string> texts, EmbeddingClient client)
{
    var response = await client.AsIEmbeddingGenerator().GenerateAsync(texts);
    Console.WriteLine($"✓ Create OpenAI embeddings for {texts.Count()} items\n");
    return response.Select(r => r.Vector.ToArray());
}

static async Task<float[]> CreateSingleEmbeddings(string texts, EmbeddingClient client)
{
    var result = await CreateEmbeddings([texts], client);
    return result.First();
}