using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Domain.Entities;

public class Category
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public OperationType Type { get; set; }
    public bool IsSystem { get; set; }

    public User? User { get; set; }
    public ICollection<Operation> Operations { get; set; } = new List<Operation>();
}
