namespace FinanceTracker.Api.Dtos;

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; 
    public bool IsSystem { get; set; }
}