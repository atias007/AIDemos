using ModelContextProtocol.Server;
using System.ComponentModel;

namespace MCPServer;

[McpServerToolType]
internal class SupplierTools
{
    [McpServerTool, Description("Returns a greeting message.")]
    public static string GetGreeting(string name)
    {
        return $"Hello, {name}!";
    }
}