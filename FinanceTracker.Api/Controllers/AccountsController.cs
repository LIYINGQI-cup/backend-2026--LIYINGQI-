using System.Security.Claims;
using FinanceTracker.Api.Dtos;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly string[] _allowedCurrencies = { "RUB", "USD", "EUR", "CNY", "KZT", "GBP" };

    public AccountsController(AppDbContext db)
    {
        _db = db;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    [HttpGet]
    public async Task<IActionResult> GetAccounts([FromQuery] bool includeArchived = false)
    {
        var userId = GetUserId();
        var query = _db.Accounts.Where(a => a.UserId == userId);
        
        if (!includeArchived) query = query.Where(a => !a.IsArchived);

        var accounts = await query.OrderBy(a => a.CreatedAt).ToListAsync();
        var result = new List<AccountDto>();

        foreach (var acc in accounts)
        {
            result.Add(await MapToDto(acc));
        }

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAccount(Guid id)
    {
        var userId = GetUserId();
        var account = await _db.Accounts.FirstOrDefaultAsync(a => a.Id == id);

        if (account == null) return NotFound();
        if (account.UserId != userId) return Forbid();

        return Ok(await MapToDto(account));
    }

    [HttpPost]
    public async Task<IActionResult> CreateAccount(CreateAccountRequest request)
    {
        if (!_allowedCurrencies.Contains(request.Currency.ToUpper()))
        {
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                { "currency", new[] { "Currency is not allowed." } }
            }));
        }

        var account = new Account
        {
            Id = Guid.NewGuid(),
            UserId = GetUserId(),
            Name = request.Name,
            Currency = request.Currency.ToUpper(),
            StartAmount = request.StartAmount,
            IsArchived = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.Accounts.Add(account);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAccount), new { id = account.Id }, await MapToDto(account));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAccount(Guid id, UpdateAccountRequest request)
    {
        var userId = GetUserId();
        var account = await _db.Accounts.FirstOrDefaultAsync(a => a.Id == id);

        if (account == null) return NotFound();
        if (account.UserId != userId) return Forbid();

        if (!_allowedCurrencies.Contains(request.Currency.ToUpper()))
        {
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                { "currency", new[] { "Currency is not allowed." } }
            }));
        }

        if (account.Currency != request.Currency.ToUpper())
        {
            var hasOperations = await _db.Operations.AnyAsync(o => o.AccountId == id);
            var hasTransfers = await _db.Transfers.AnyAsync(t => t.FromAccountId == id || t.ToAccountId == id);
            
            if (hasOperations || hasTransfers)
            {
                return Conflict(new ProblemDetails { Status = 409, Title = "Conflict", Detail = "Cannot change currency of an account with operations or transfers." });
            }
        }

        account.Name = request.Name;
        account.Currency = request.Currency.ToUpper();
        account.StartAmount = request.StartAmount;

        await _db.SaveChangesAsync();
        return Ok(await MapToDto(account));
    }

    [HttpPatch("{id}/archive")]
    public async Task<IActionResult> ArchiveAccount(Guid id, ArchiveAccountRequest request)
    {
        var userId = GetUserId();
        var account = await _db.Accounts.FirstOrDefaultAsync(a => a.Id == id);

        if (account == null) return NotFound();
        if (account.UserId != userId) return Forbid();

        account.IsArchived = request.IsArchived;
        await _db.SaveChangesAsync();

        return Ok(await MapToDto(account));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAccount(Guid id)
    {
        var userId = GetUserId();
        var account = await _db.Accounts.FirstOrDefaultAsync(a => a.Id == id);

        if (account == null) return NotFound();
        if (account.UserId != userId) return Forbid();

        // R6: Cannot delete if there are operations or transfers
        var hasOperations = await _db.Operations.AnyAsync(o => o.AccountId == id);
        var hasTransfers = await _db.Transfers.AnyAsync(t => t.FromAccountId == id || t.ToAccountId == id);
        
        if (hasOperations || hasTransfers)
        {
            return Conflict(new ProblemDetails { Status = 409, Title = "Conflict", Detail = "Account has operations or transfers." });
        }

        _db.Accounts.Remove(account);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private async Task<AccountDto> MapToDto(Account account)
    {
        // Calculate R1: StartAmount + Income - Expense + Transfers In - Transfers Out
        var operations = await _db.Operations.Where(o => o.AccountId == account.Id).ToListAsync();
        var transfersOut = await _db.Transfers.Where(t => t.FromAccountId == account.Id).SumAsync(t => t.Amount);
        var transfersIn = await _db.Transfers.Where(t => t.ToAccountId == account.Id).SumAsync(t => t.Amount);

        decimal balance = account.StartAmount;
        balance += operations.Where(o => o.Type == FinanceTracker.Domain.Enums.OperationType.Income).Sum(o => o.Amount);
        balance -= operations.Where(o => o.Type == FinanceTracker.Domain.Enums.OperationType.Expense).Sum(o => o.Amount);
        balance += transfersIn;
        balance -= transfersOut;

        return new AccountDto
        {
            Id = account.Id,
            Name = account.Name,
            Currency = account.Currency,
            StartAmount = account.StartAmount,
            Balance = balance,
            IsArchived = account.IsArchived,
            CreatedAt = account.CreatedAt
        };
    }
}