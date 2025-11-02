using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateEmptyApplicationBuilder(null);

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport() // Configures standard I/O for communication
    .WithToolsFromAssembly(); // Automatically discovers and registers tools from attributes

var app = builder.Build();
await app.RunAsync();