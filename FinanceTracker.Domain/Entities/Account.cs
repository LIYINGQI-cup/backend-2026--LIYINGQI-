namespace FinanceTracker.Domain.Entities;

public class Account
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal StartAmount { get; set; }
    public bool IsArchived { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<Operation> Operations { get; set; } = new List<Operation>();
    public ICollection<Transfer> OutgoingTransfers { get; set; } = new List<Transfer>();
    public ICollection<Transfer> IncomingTransfers { get; set; } = new List<Transfer>();
}
