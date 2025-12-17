using ModelContextProtocol.Server;
using System.ComponentModel;

namespace MCPServer;

[McpServerToolType]
internal class SupplierTools
{
    [McpServerTool, Description("Returns a greeting message.")]
    public static string GetGreeting([Description("bla..")] string name)
    {
        return $"Hello, {name}!";
    }
}