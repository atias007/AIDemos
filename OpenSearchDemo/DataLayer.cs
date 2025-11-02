using Microsoft.Data.SqlClient;
using RepoDb;

namespace OpenSearchDemo;

internal static class DataLayer
{
    public static async Task<IEnumerable<Customer>> GetAllCustomers()
    {
        var connectionString = "Server=localhost;Database=Northwind;Trusted_Connection=True;Encrypt=False";
        using var conn = new SqlConnection(connectionString);
        await conn.EnsureOpenAsync();
        var customers = await conn.QueryAllAsync<Customer>();
        return customers;
    }
}