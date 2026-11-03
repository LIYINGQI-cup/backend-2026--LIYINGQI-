using System.Security.Claims;
using FinanceTracker.Api.Dtos;
using FinanceTracker.Domain.Enums;
using FinanceTracker.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StatisticsController : ControllerBase
{
    private readonly AppDbContext _db;

    public StatisticsController(AppDbContext db)
    {
        _db = db;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    [HttpGet]
    public async Task<IActionResult> GetStatistics(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        [FromQuery] Guid? accountId,
        [FromQuery] string? currency,
        [FromQuery] string groupBy = "day")
    {
        if ((!accountId.HasValue && string.IsNullOrEmpty(currency)) || (accountId.HasValue && !string.IsNullOrEmpty(currency)))
        {
            return BadRequest(new ProblemDetails { Status = 400, Detail = "Exactly one of accountId or currency must be provided." });
        }

        if (from > to)
        {
            return BadRequest(new ProblemDetails { Status = 400, Detail = "from must be <= to." });
        }

        if (to.DayNumber - from.DayNumber > 366)
        {
            return BadRequest(new ProblemDetails { Status = 400, Detail = "Period cannot exceed 366 days." });
        }

        var userId = GetUserId();

        var query = _db.Operations
            .Include(o => o.Account)
            .Include(o => o.Category)
            .Where(o => o.Account.UserId == userId && o.Date >= from && o.Date <= to);

        string targetCurrency = currency ?? "";

        if (accountId.HasValue)
        {
            var account = await _db.Accounts.FirstOrDefaultAsync(a => a.Id == accountId);
            if (account == null) return NotFound();
            if (account.UserId != userId) return Forbid();
            
            targetCurrency = account.Currency;
            query = query.Where(o => o.AccountId == accountId);
        }
        else
        {
            query = query.Where(o => o.Account.Currency == currency && !o.Account.IsArchived);
        }

        var operations = await query.ToListAsync();

        var totalIncome = operations.Where(o => o.Type == OperationType.Income).Sum(o => o.Amount);
        var totalExpense = operations.Where(o => o.Type == OperationType.Expense).Sum(o => o.Amount);
        var net = totalIncome - totalExpense;

        var byCategory = operations
            .GroupBy(o => new { o.CategoryId, o.Category.Name, o.Type })
            .Select(g => 
            {
                var amount = g.Sum(o => o.Amount);
                var share = 0.0;
                
                if (g.Key.Type == OperationType.Income && totalIncome > 0)
                {
                    share = Math.Round((double)(amount / totalIncome), 4);
                }
                else if (g.Key.Type == OperationType.Expense && totalExpense > 0)
                {
                    share = Math.Round((double)(amount / totalExpense), 4);
                }

                return new CategoryStatisticsDto
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.Name,
                    Type = g.Key.Type.ToString(),
                    Amount = amount,
                    Share = share
                };
            })
            .OrderByDescending(c => c.Amount)
            .ToList();

        var timeline = new List<TimelinePointDto>();
        if (groupBy == "month")
        {
            timeline = operations
                .GroupBy(o => new { o.Date.Year, o.Date.Month })
                .Select(g => new TimelinePointDto
                {
                    Period = $"{g.Key.Year:D4}-{g.Key.Month:D2}",
                    Income = g.Where(o => o.Type == OperationType.Income).Sum(o => o.Amount),
                    Expense = g.Where(o => o.Type == OperationType.Expense).Sum(o => o.Amount)
                })
                .OrderBy(t => t.Period)
                .ToList();
        }
        else // day
        {
            timeline = operations
                .GroupBy(o => o.Date)
                .Select(g => new TimelinePointDto
                {
                    Period = g.Key.ToString("yyyy-MM-dd"),
                    Income = g.Where(o => o.Type == OperationType.Income).Sum(o => o.Amount),
                    Expense = g.Where(o => o.Type == OperationType.Expense).Sum(o => o.Amount)
                })
                .OrderBy(t => t.Period)
                .ToList();
        }

        return Ok(new StatisticsDto
        {
            From = from,
            To = to,
            Currency = targetCurrency,
            AccountId = accountId,
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            Net = net,
            ByCategory = byCategory,
            Timeline = timeline
        });
    }
}