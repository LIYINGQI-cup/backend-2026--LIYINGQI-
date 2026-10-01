using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Infrastructure;

public static class SeedData
{
    public static readonly Guid DemoUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public static readonly Guid SalaryId = Guid.Parse("a0000000-0000-0000-0000-000000000001");
    public static readonly Guid ScholarshipId = Guid.Parse("a0000000-0000-0000-0000-000000000002");
    public static readonly Guid OtherIncomeId = Guid.Parse("a0000000-0000-0000-0000-000000000003");
    public static readonly Guid FoodId = Guid.Parse("b0000000-0000-0000-0000-000000000001");
    public static readonly Guid RestaurantsId = Guid.Parse("b0000000-0000-0000-0000-000000000002");
    public static readonly Guid MedicineId = Guid.Parse("b0000000-0000-0000-0000-000000000003");
    public static readonly Guid SportId = Guid.Parse("b0000000-0000-0000-0000-000000000004");
    public static readonly Guid TaxiId = Guid.Parse("b0000000-0000-0000-0000-000000000005");
    public static readonly Guid RentId = Guid.Parse("b0000000-0000-0000-0000-000000000006");
    public static readonly Guid InvestmentsId = Guid.Parse("b0000000-0000-0000-0000-000000000007");
    public static readonly Guid ClothesId = Guid.Parse("b0000000-0000-0000-0000-000000000008");
    public static readonly Guid FunId = Guid.Parse("b0000000-0000-0000-0000-000000000009");
    public static readonly Guid OtherExpenseId = Guid.Parse("b0000000-0000-0000-0000-000000000010");

    public static readonly Guid CashId = Guid.Parse("c0000000-0000-0000-0000-000000000001");
    public static readonly Guid DebitCardId = Guid.Parse("c0000000-0000-0000-0000-000000000002");
    public static readonly Guid SavingsId = Guid.Parse("c0000000-0000-0000-0000-000000000003");

    public static async Task InitializeAsync(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        if (await db.Users.AnyAsync())
        {
            return;
        }

        var now = DateTime.UtcNow;

        // Demo user via Identity
        var user = new ApplicationUser
        {
            Id = DemoUserId,
            Email = "demo@fintracker.local",
            UserName = "demo@fintracker.local",
            Name = "Demo User",
            CreatedAt = now
        };
        await userManager.CreateAsync(user, "Demo12345");

        // System categories
        var categories = new List<Category>
        {
            new() { Id = SalaryId, Name = "Salary", Type = OperationType.Income, IsSystem = true, UserId = null },
            new() { Id = ScholarshipId, Name = "Scholarship", Type = OperationType.Income, IsSystem = true, UserId = null },
            new() { Id = OtherIncomeId, Name = "Other", Type = OperationType.Income, IsSystem = true, UserId = null },
            new() { Id = FoodId, Name = "Food", Type = OperationType.Expense, IsSystem = true, UserId = null },
            new() { Id = RestaurantsId, Name = "Restaurants", Type = OperationType.Expense, IsSystem = true, UserId = null },
            new() { Id = MedicineId, Name = "Medicine", Type = OperationType.Expense, IsSystem = true, UserId = null },
            new() { Id = SportId, Name = "Sport", Type = OperationType.Expense, IsSystem = true, UserId = null },
            new() { Id = TaxiId, Name = "Taxi", Type = OperationType.Expense, IsSystem = true, UserId = null },
            new() { Id = RentId, Name = "Rent", Type = OperationType.Expense, IsSystem = true, UserId = null },
            new() { Id = InvestmentsId, Name = "Investments", Type = OperationType.Expense, IsSystem = true, UserId = null },
            new() { Id = ClothesId, Name = "Clothes", Type = OperationType.Expense, IsSystem = true, UserId = null },
            new() { Id = FunId, Name = "Fun", Type = OperationType.Expense, IsSystem = true, UserId = null },
            new() { Id = OtherExpenseId, Name = "Other", Type = OperationType.Expense, IsSystem = true, UserId = null }
        };
        db.Categories.AddRange(categories);

        // Accounts
        var accounts = new List<Account>
        {
            new() { Id = CashId, UserId = DemoUserId, Name = "Cash", Currency = "RUB", StartAmount = 5000.00m, IsArchived = false, CreatedAt = now },
            new() { Id = DebitCardId, UserId = DemoUserId, Name = "Debit card", Currency = "RUB", StartAmount = 42000.00m, IsArchived = false, CreatedAt = now },
            new() { Id = SavingsId, UserId = DemoUserId, Name = "Savings", Currency = "USD", StartAmount = 1500.00m, IsArchived = false, CreatedAt = now }
        };
        db.Accounts.AddRange(accounts);

        // Operations and Transfers: 与之前 LI-9 的内容完全一致，保持不变
        // ...（粘贴 LI-9 里 30 个 operations 和 2 个 transfers 的代码）

        await db.SaveChangesAsync();
    }
}
