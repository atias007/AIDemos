using System.ComponentModel.DataAnnotations.Schema;

namespace AIVectorSearchWithDuckDB;

[Table("Employees")]
public class Employee
{
    [Column("EmployeeID")]
    public int Id { get; set; }

    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public string? Title { get; set; }

    public string? Notes { get; set; }

    public override string ToString()
    {
        return $"""
            Employee
              Id: {Id}
              First Name: {FirstName}
              Last Name: {LastName}
              Title: {Title}
              Notes: {Notes}
            """;
    }

    public IEnumerable<float[]> Vectors { get; set; } = [];
}