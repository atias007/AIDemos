using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenSearchDemo;

[Table("Customers")]
public class Customer
{
    [Key]
    [StringLength(5)]
    [Column("CustomerID", TypeName = "nchar(5)")]
    public string CustomerID { get; set; } = string.Empty;

    [Required]
    [StringLength(40)]
    [Column("CompanyName", TypeName = "nvarchar(40)")]
    public string CompanyName { get; set; } = string.Empty;

    [StringLength(30)]
    [Column("ContactName", TypeName = "nvarchar(30)")]
    public string? ContactName { get; set; }

    [StringLength(30)]
    [Column("ContactTitle", TypeName = "nvarchar(30)")]
    public string? ContactTitle { get; set; }

    [StringLength(60)]
    [Column("Address", TypeName = "nvarchar(60)")]
    public string? Address { get; set; }

    [StringLength(15)]
    [Column("City", TypeName = "nvarchar(15)")]
    public string? City { get; set; }

    [StringLength(15)]
    [Column("Region", TypeName = "nvarchar(15)")]
    public string? Region { get; set; }

    [StringLength(10)]
    [Column("PostalCode", TypeName = "nvarchar(10)")]
    public string? PostalCode { get; set; }

    [StringLength(15)]
    [Column("Country", TypeName = "nvarchar(15)")]
    public string? Country { get; set; }

    [StringLength(24)]
    [Column("Phone", TypeName = "nvarchar(24)")]
    public string? Phone { get; set; }

    [StringLength(24)]
    [Column("Fax", TypeName = "nvarchar(24)")]
    public string? Fax { get; set; }
}