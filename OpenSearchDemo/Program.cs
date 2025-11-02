using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearchDemo;
using RepoDb;

// Initialize repodb
GlobalConfiguration.Setup().UseSqlServer();

// load data
// var customers = await DataLayer.GetAllCustomers();

// Connect to OpenSearch
var nodeAddress = new Uri("http://localhost:9200");
var client = new OpenSearchClient(nodeAddress);

// var response = await client.IndexAsync(customers.First(), i => i.Index("customers"));

// save data to OpenSearch
// var manyResponse = client.IndexMany(customers, "customers");

// Console.ReadLine();

await FindCityLondon(client);
await FindTextEntry(client);

Console.ReadLine();

static void PrintResponse(ISearchResponse<Customer> response)
{
    Console.WriteLine($"Total: {response.Total}");
    foreach (var hit in response.Hits)
    {
        Console.WriteLine($"Id: {hit.Source.CustomerID}, Name: {hit.Source.CompanyName}, City: {hit.Source.City}");
    }
    Console.WriteLine("---------");
}

static async Task FindCityLondon(OpenSearchClient client)
{
    var searchResponse = await client.SearchAsync<Customer>(s => s
                                .Index("customers")
                                .Query(q => q
                                    .Match(m => m
                                        .Field(fld => fld.City)
                                        .Query("London"))));

    PrintResponse(searchResponse);
}

static async Task FindTextEntry(OpenSearchClient client)
{
    var searchResponse = await client.SearchAsync<Customer>(s => s
                                .Index("customers")
                                    .Query(q => q
                                        .Match(m => m.Query("London"))));
    PrintResponse(searchResponse);
}