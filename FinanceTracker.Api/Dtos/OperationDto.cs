namespace FinanceTracker.Api.Dtos;

public class OperationDto
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateOnly Date { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}