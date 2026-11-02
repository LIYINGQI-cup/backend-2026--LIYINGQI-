namespace FinanceTracker.Api.Dtos;

public class TransferDto
{
    public Guid Id { get; set; }
    public Guid FromAccountId { get; set; }
    public string FromAccountName { get; set; } = string.Empty;
    public Guid ToAccountId { get; set; }
    public string ToAccountName { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateOnly Date { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}