using Microsoft.Data.SqlClient;
using Microsoft.Extensions.AI;
using RepoDb;
using System.Data;
using System.Data.Common;

namespace AIVectorSearchWithDuckDB;

public class VectorDb
{
    private const string connectionString = "Server=localhost,1600;Database=Northwind;User Id=sa;Password=CustomsDev123!;Encrypt=False;TrustServerCertificate=True";

    public static async Task InsertVector(Employee record)
    {
        using var conn = new SqlConnection(connectionString);
        await conn.EnsureOpenAsync();

        foreach (var vector in record.Vectors)
        {
            var vectorString = $"[{string.Join(",", vector)}]";
            using var command = conn.CreateCommand();
            command.CommandText =
                @"INSERT INTO EmployeesVectors (EmployeeID, FirstName, LastName, Embedding)
                VALUES (@id, @first_name, @last_name, @vector)";

            command.Parameters.AddWithValue("id", record.Id);
            command.Parameters.AddWithValue("first_name", record.FirstName);
            command.Parameters.AddWithValue("last_name", record.LastName);
            command.Parameters.AddWithValue("vector", vectorString);

            var count = await command.ExecuteNonQueryAsync();
        }
    }

    public static async IAsyncEnumerable<EmployeeSearchResult> SearchVectors(float[] queryVector, int topk)
    {
        using var conn = new SqlConnection(connectionString);
        await conn.EnsureOpenAsync();
        var vectorString = $"[{string.Join(",", queryVector)}]";

        using var command = conn.CreateCommand();
        command.CommandText =
            """
            SELECT TOP (@topk) EmployeeID, FirstName, LastName, VECTOR_DISTANCE('cosine', [Embedding], CAST(@vector AS VECTOR(1536))) AS Distance
            FROM EmployeesVectors
            ORDER BY Distance ASC
            """;

        command.Parameters.Add("topk", SqlDbType.Int).Value = topk;
        command.Parameters.Add("vector", SqlDbType.NVarChar).Value = vectorString;

        using var reader = await command.ExecuteReaderAsync();
        while (reader.Read())
        {
            yield return new EmployeeSearchResult
            {
                EmployeeID = reader.GetInt32(0),
                FirstName = reader.GetString(1),
                LastName = reader.GetString(2),
                Distance = reader.GetDouble(3)
            };
        }
    }

    private float[] ParseVectorFromReader(IDataReader reader, int columnIndex)
    {
        var vectorString = reader.GetString(columnIndex);
        // Remove brackets and parse
        vectorString = vectorString.Trim('[', ']');
        return vectorString.Split(',')
            .Select(s => float.Parse(s.Trim()))
            .ToArray();
    }
}