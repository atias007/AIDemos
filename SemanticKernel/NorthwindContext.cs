using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace SemanticKernel;

internal class NorthwindContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Supplier> Suppliers { get; set; }
}

internal class Supplier
{
    [Key]
    public int SupplierID { get; set; }

    public string CompanyName { get; set; } = null!;

    public string? Country { get; set; }

    public bool Active { get; set; }
}