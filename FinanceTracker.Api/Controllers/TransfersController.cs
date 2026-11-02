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
public class TransfersController : ControllerBase
{
    private readonly AppDbContext _db;

    public TransfersController(AppDbContext db)
    {
        _db = db;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    [HttpGet]
    public async Task<IActionResult> GetTransfers(
        [FromQuery] Guid? accountId,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (pageSize > 100) return BadRequest(new ProblemDetails { Status = 400, Detail = "pageSize cannot exceed 100." });
        if (from.HasValue && to.HasValue && from > to) return BadRequest(new ProblemDetails { Status = 400, Detail = "from must be <= to." });

        var userId = GetUserId();
        
        var query = _db.Transfers
            .Include(t => t.FromAccount)
            .Include(t => t.ToAccount)
            .Where(t => t.FromAccount.UserId == userId);

        if (accountId.HasValue)
        {
            query = query.Where(t => t.FromAccountId == accountId || t.ToAccountId == accountId);
        }

        if (from.HasValue) query = query.Where(t => t.Date >= from);
        if (to.HasValue) query = query.Where(t => t.Date <= to);

        query = query.OrderByDescending(t => t.Date).ThenByDescending(t => t.CreatedAt);

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TransferDto
            {
                Id = t.Id,
                FromAccountId = t.FromAccountId,
                FromAccountName = t.FromAccount.Name,
                ToAccountId = t.ToAccountId,
                ToAccountName = t.ToAccount.Name,
                Currency = t.FromAccount.Currency, // 转账币种跟随转出账户
                Amount = t.Amount,
                Date = t.Date,
                Comment = t.Comment,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();

        return Ok(new { items, page, pageSize, totalCount, totalPages });
    }

    [HttpPost]
    public async Task<IActionResult> CreateTransfer(TransferRequest request)
    {
        var userId = GetUserId();

        var fromAccount = await _db.Accounts.FirstOrDefaultAsync(a => a.Id == request.FromAccountId);
        var toAccount = await _db.Accounts.FirstOrDefaultAsync(a => a.Id == request.ToAccountId);

        if (fromAccount == null || toAccount == null) return NotFound();

        if (fromAccount.UserId != userId || toAccount.UserId != userId) return Forbid();

        if (fromAccount.Id == toAccount.Id)
        {
            return BadRequest(new ProblemDetails { Status = 400, Detail = "Cannot transfer to the same account." });
        }

        if (fromAccount.Currency != toAccount.Currency)
        {
            return BadRequest(new ProblemDetails { Status = 400, Detail = "Accounts must have the same currency." });
        }

        if (fromAccount.IsArchived || toAccount.IsArchived)
        {
            return Conflict(new ProblemDetails { Status = 409, Detail = "Account is archived." });
        }

        if (request.Date > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            return BadRequest(new ProblemDetails { Status = 400, Detail = "Date cannot be in the future." });
        }

        var transfer = new Transfer
        {
            Id = Guid.NewGuid(),
            FromAccountId = request.FromAccountId,
            ToAccountId = request.ToAccountId,
            Amount = request.Amount,
            Date = request.Date,
            Comment = request.Comment,
            CreatedAt = DateTime.UtcNow
        };

        _db.Transfers.Add(transfer);
        await _db.SaveChangesAsync();

        var dto = new TransferDto
        {
            Id = transfer.Id,
            FromAccountId = transfer.FromAccountId,
            FromAccountName = fromAccount.Name,
            ToAccountId = transfer.ToAccountId,
            ToAccountName = toAccount.Name,
            Currency = fromAccount.Currency,
            Amount = transfer.Amount,
            Date = transfer.Date,
            Comment = transfer.Comment,
            CreatedAt = transfer.CreatedAt
        };

        return CreatedAtAction(nameof(GetTransfer), new { id = transfer.Id }, dto);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTransfer(Guid id)
    {
        var userId = GetUserId();
        var transfer = await _db.Transfers
            .Include(t => t.FromAccount)
            .Include(t => t.ToAccount)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (transfer == null) return NotFound();
        
        if (transfer.FromAccount.UserId != userId) return Forbid();

        var dto = new TransferDto
        {
            Id = transfer.Id,
            FromAccountId = transfer.FromAccountId,
            FromAccountName = transfer.FromAccount.Name,
            ToAccountId = transfer.ToAccountId,
            ToAccountName = transfer.ToAccount.Name,
            Currency = transfer.FromAccount.Currency,
            Amount = transfer.Amount,
            Date = transfer.Date,
            Comment = transfer.Comment,
            CreatedAt = transfer.CreatedAt
        };

        return Ok(dto);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTransfer(Guid id)
    {
        var userId = GetUserId();
        var transfer = await _db.Transfers.Include(t => t.FromAccount).FirstOrDefaultAsync(t => t.Id == id);

        if (transfer == null) return NotFound();
        
        if (transfer.FromAccount.UserId != userId) return Forbid();

        _db.Transfers.Remove(transfer);
        await _db.SaveChangesAsync();
        
        return NoContent(); 
    }
}