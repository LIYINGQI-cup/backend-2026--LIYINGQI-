using Microsoft.AspNetCore.Identity;

namespace FinanceTracker.Infrastructure;

public class ApplicationUser : IdentityUser<Guid>
{
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
