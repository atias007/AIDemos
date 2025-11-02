namespace AIVectorSearchWithDuckDB;

public class EmployeeSearchResult
{
    public int EmployeeID { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public double Distance { get; set; }
}