namespace FinanceTracker.Api.Dtos;

public class AccountDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal StartAmount { get; set; }
    public decimal Balance { get; set; }
    public bool IsArchived { get; set; }
    public DateTime CreatedAt { get; set; }
}