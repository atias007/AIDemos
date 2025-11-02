using Microsoft.Data.SqlClient;
using RepoDb;

namespace AIVectorSearchWithDuckDB;

internal static class SqlDb
{
    public static async Task<IEnumerable<Employee>> GetEmployeesAsync()
    {
        var connectionString = "Server=localhost;Database=Northwind;Trusted_Connection=True;Encrypt=False";
        using var conn = new SqlConnection(connectionString);
        await conn.EnsureOpenAsync();
        var customers = await conn.QueryAllAsync<Employee>();
        return customers;
    }
}