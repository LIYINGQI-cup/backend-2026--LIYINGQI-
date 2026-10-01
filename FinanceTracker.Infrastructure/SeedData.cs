using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Infrastructure;

public static class SeedData
{
    // Demo user
    public static readonly Guid DemoUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    // System categories
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

    // Accounts
    public static readonly Guid CashId = Guid.Parse("c0000000-0000-0000-0000-000000000001");
    public static readonly Guid DebitCardId = Guid.Parse("c0000000-0000-0000-0000-000000000002");
    public static readonly Guid SavingsId = Guid.Parse("c0000000-0000-0000-0000-000000000003");

    public static async Task InitializeAsync(AppDbContext db)
    {
        // Only seed on empty database
        if (await db.Users.AnyAsync())
        {
            return;
        }

        var now = DateTime.UtcNow;

        // Demo user
        var user = new User
        {
            Id = DemoUserId,
            Email = "demo@fintracker.local",
            Name = "Demo User",
            PasswordHash = "PLACEHOLDER_HASH", // Will be replaced by Identity in LI-11
            CreatedAt = now
        };
        db.Users.Add(user);

        // System categories (UserId = null)
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

        // Operations (30)
        var operations = new List<Operation>
        {
            // August
            new() { Id = Guid.Parse("d0000000-0000-0000-0000-000000000001"), AccountId = DebitCardId, CategoryId = SalaryId, Type = OperationType.Income, Amount = 60000.00m, Date = new DateOnly(2026, 8, 5), Comment = "August salary", CreatedAt = now },
            new() { Id = Guid.Parse("d0000000-0000-0000-0000-000000000002"), AccountId = DebitCardId, CategoryId = RentId, Type = OperationType.Expense, Amount = 25000.00m, Date = new DateOnly(2026, 8, 5), Comment = "Rent, August", CreatedAt = now },
            new() { Id = Guid.Parse("d0000000-0000-0000-0000-000000000003"), AccountId = DebitCardId, CategoryId = FoodId, Type = OperationType.Expense, Amount = 3200.50m, Date = new DateOnly(2026, 8, 6), Comment = "Groceries", CreatedAt = now },
            new() { Id = Guid.Parse("d0000000-0000-0000-0000-000000000004"), AccountId = DebitCardId, CategoryId = RestaurantsId, Type = OperationType.Expense, Amount = 1800.00m, Date = new DateOnly(2026, 8, 9), Comment = "Dinner", CreatedAt = now },
            new() { Id = Guid.Parse("d0000000-0000-0000-0000-000000000005"), AccountId = DebitCardId, CategoryId = FoodId, Type = OperationType.Expense, Amount = 2750.00m, Date = new DateOnly(2026, 8, 12), Comment = "Groceries", CreatedAt = now },
            new() { Id = Guid.Parse("d0000000-0000-0000-0000-000000000006"), AccountId = DebitCardId, CategoryId = ClothesId, Type = OperationType.Expense, Amount = 4990.00m, Date = new DateOnly(2026, 8, 14), Comment = "Sneakers", CreatedAt = now },
            new() { Id = Guid.Parse("d0000000-0000-0000-0000-000000000007"), AccountId = DebitCardId, CategoryId = FoodId, Type = OperationType.Expense, Amount = 3100.25m, Date = new DateOnly(2026, 8, 18), Comment = "Groceries", CreatedAt = now },
            new() { Id = Guid.Parse("d0000000-0000-0000-0000-000000000008"), AccountId = DebitCardId, CategoryId = MedicineId, Type = OperationType.Expense, Amount = 1200.00m, Date = new DateOnly(2026, 8, 20), Comment = "Pharmacy", CreatedAt = now },
            new() { Id = Guid.Parse("d0000000-0000-0000-0000-000000000009"), AccountId = DebitCardId, CategoryId = InvestmentsId, Type = OperationType.Expense, Amount = 10000.00m, Date = new DateOnly(2026, 8, 25), Comment = "Index fund", CreatedAt = now },
            // September
            new() { Id = Guid.Parse("d0000000-0000-0000-0000-000000000010"), AccountId = DebitCardId, CategoryId = SalaryId, Type = OperationType.Income, Amount = 60000.00m, Date = new DateOnly(2026, 9, 5), Comment = "September salary", CreatedAt = now },
            new() { Id = Guid.Parse("d0000000-0000-0000-0000-000000000011"), AccountId = DebitCardId, CategoryId = RentId, Type = OperationType.Expense, Amount = 25000.00m, Date = new DateOnly(2026, 9, 5), Comment = "Rent, September", CreatedAt = now },
            new() { Id = Guid.Parse("d0000000-0000-0000-0000-000000000012"), AccountId = DebitCardId, CategoryId = FoodId, Type = OperationType.Expense, Amount = 2980.00m, Date = new DateOnly(2026, 9, 7), Comment = "Groceries", CreatedAt = now },
            new() { Id = Guid.Parse("d0000000-0000-0000-0000-000000000013"), AccountId = DebitCardId, CategoryId = SportId, Type = OperationType.Expense, Amount = 3500.00m, Date = new DateOnly(2026, 9, 10), Comment = "Gym membership", CreatedAt = now },
            new() { Id = Guid.Parse("d0000000-0000-0000-0000-000000000014"), AccountId = DebitCardId, CategoryId = RestaurantsId, Type = OperationType.Expense, Amount = 2400.00m, Date = new DateOnly(2026, 9, 13), Comment = "Lunch with friends", CreatedAt = now },
            new() { Id = Guid.Parse("d0000000-0000-0000-0000-000000000015"), AccountId = DebitCardId, CategoryId = FoodId, Type = OperationType.Expense, Amount = 3050.75m, Date = new DateOnly(2026, 9, 17), Comment = "Groceries", CreatedAt = now },
            new() { Id = Guid.Parse("d0000000-0000-0000-0000-000000000016"), AccountId = DebitCardId, CategoryId = FunId, Type = OperationType.Expense, Amount = 1500.00m, Date = new DateOnly(2026, 9, 21), Comment = "Cinema", CreatedAt = now },
            new() { Id = Guid.Parse("d0000000-0000-0000-0000-000000000017"), AccountId = DebitCardId, CategoryId = FoodId, Type = OperationType.Expense, Amount = 2600.00m, Date = new DateOnly(2026, 9, 26), Comment = "Groceries", CreatedAt = now },
            new() { Id = Guid.Parse("d0000000-0000-0000-0000-000000000018"), AccountId = DebitCardId, CategoryId = InvestmentsId, Type = OperationType.Expense, Amount = 10000.00m, Date = new DateOnly(2026, 9, 28), Comment = "Index fund", CreatedAt = now },
            // Cash
            new() { Id = Guid.Parse("d0000000-0000-0000-0000-000000000019"), AccountId = CashId, CategoryId = TaxiId, Type = OperationType.Expense, Amount = 450.00m, Date = new DateOnly(2026, 8, 2), Comment = null, CreatedAt = now },
            new() { Id = Guid.Parse("d0000000-0000-0000-0000-000000000020"), AccountId = CashId, CategoryId = FoodId, Type = OperationType.Expense, Amount = 780.00m, Date = new DateOnly(2026, 8, 10), Comment = "Market", CreatedAt = now },
            new() { Id = Guid.Parse("d0000000-0000-0000-0000-000000000021"), AccountId = CashId, CategoryId = OtherIncomeId, Type = OperationType.Income, Amount = 3000.00m, Date = new DateOnly(2026, 8, 15), Comment = "Birthday gift", CreatedAt = now },
            new() { Id = Guid.Parse("d0000000-0000-0000-0000-000000000022"), AccountId = CashId, CategoryId = TaxiId, Type = OperationType.Expense, Amount = 520.00m, Date = new DateOnly(2026, 8, 22), Comment = null, CreatedAt = now },
            new() { Id = Guid.Parse("d0000000-0000-0000-0000-000000000023"), AccountId = CashId, CategoryId = FunId, Type = OperationType.Expense, Amount = 1200.00m, Date = new DateOnly(2026, 8, 29), Comment = "Concert", CreatedAt = now },
            new() { Id = Guid.Parse("d0000000-0000-0000-0000-000000000024"), AccountId = CashId, CategoryId = TaxiId, Type = OperationType.Expense, Amount = 610.00m, Date = new DateOnly(2026, 9, 3), Comment = null, CreatedAt = now },
            new() { Id = Guid.Parse("d0000000-0000-0000-0000-000000000025"), AccountId = CashId, CategoryId = FoodId, Type = OperationType.Expense, Amount = 900.00m, Date = new DateOnly(2026, 9, 11), Comment = "Market", CreatedAt = now },
            new() { Id = Guid.Parse("d0000000-0000-0000-0000-000000000026"), AccountId = CashId, CategoryId = OtherExpenseId, Type = OperationType.Expense, Amount = 350.00m, Date = new DateOnly(2026, 9, 19), Comment = "Haircut", CreatedAt = now },
            new() { Id = Guid.Parse("d0000000-0000-0000-0000-000000000027"), AccountId = CashId, CategoryId = TaxiId, Type = OperationType.Expense, Amount = 480.00m, Date = new DateOnly(2026, 9, 24), Comment = null, CreatedAt = now },
            // Savings
            new() { Id = Guid.Parse("d0000000-0000-0000-0000-000000000028"), AccountId = SavingsId, CategoryId = ScholarshipId, Type = OperationType.Income, Amount = 300.00m, Date = new DateOnly(2026, 8, 1), Comment = "Scholarship, August", CreatedAt = now },
            new() { Id = Guid.Parse("d0000000-0000-0000-0000-000000000029"), AccountId = SavingsId, CategoryId = ScholarshipId, Type = OperationType.Income, Amount = 300.00m, Date = new DateOnly(2026, 9, 1), Comment = "Scholarship, September", CreatedAt = now },
            new() { Id = Guid.Parse("d0000000-0000-0000-0000-000000000030"), AccountId = SavingsId, CategoryId = OtherExpenseId, Type = OperationType.Expense, Amount = 45.00m, Date = new DateOnly(2026, 9, 15), Comment = "Bank fee", CreatedAt = now }
        };
        db.Operations.AddRange(operations);

        // Transfers (2)
        var transfers = new List<Transfer>
        {
            new() { Id = Guid.Parse("e0000000-0000-0000-0000-000000000001"), FromAccountId = DebitCardId, ToAccountId = CashId, Amount = 5000.00m, Date = new DateOnly(2026, 8, 7), Comment = "Cash withdrawal", CreatedAt = now },
            new() { Id = Guid.Parse("e0000000-0000-0000-0000-000000000002"), FromAccountId = DebitCardId, ToAccountId = CashId, Amount = 5000.00m, Date = new DateOnly(2026, 9, 8), Comment = "Cash withdrawal", CreatedAt = now }
        };
        db.Transfers.AddRange(transfers);

        await db.SaveChangesAsync();
    }
}
