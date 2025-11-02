using Microsoft.Extensions.AI;
using System.Text.Json;

namespace MsAiAgent;

internal class IdInfo
{
    public bool IsValid { get; set; }
    public string? IdNumber { get; set; }
    public int Quality { get; set; }

    public static JsonElement Schema => AIJsonUtilities.CreateJsonSchema(typeof(IdInfo));

    public override string ToString()
    {
        return $"Valid: {IsValid}, Number: {IdNumber}, Quality: {Quality}";
    }
}