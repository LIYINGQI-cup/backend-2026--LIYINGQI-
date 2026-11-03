namespace FinanceTracker.Api.Dtos;

public class StatisticsDto
{
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
    public string Currency { get; set; } = string.Empty;
    public Guid? AccountId { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal Net { get; set; }
    public List<CategoryStatisticsDto> ByCategory { get; set; } = new();
    public List<TimelinePointDto> Timeline { get; set; } = new();
}

public class CategoryStatisticsDto
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public double Share { get; set; }
}

public class TimelinePointDto
{
    public string Period { get; set; } = string.Empty;
    public decimal Income { get; set; }
    public decimal Expense { get; set; }
}