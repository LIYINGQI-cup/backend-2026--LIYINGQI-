using System.Security.Claims;
using FinanceTracker.Api.Dtos;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using FinanceTracker.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OperationsController : ControllerBase
{
    private readonly AppDbContext _db;

    public OperationsController(AppDbContext db)
    {
        _db = db;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    [HttpGet]
    public async Task<IActionResult> GetOperations(
        [FromQuery] Guid? accountId,
        [FromQuery] string? type,
        [FromQuery] List<Guid>? categoryIds,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] string sortBy = "dateDesc",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (pageSize > 100) return BadRequest(new ProblemDetails { Status = 400, Detail = "pageSize cannot exceed 100." });
        if (from.HasValue && to.HasValue && from > to) return BadRequest(new ProblemDetails { Status = 400, Detail = "from must be less than or equal to to." });

        var userId = GetUserId();
        var query = _db.Operations
            .Include(o => o.Account)
            .Include(o => o.Category)
            .Where(o => o.Account.UserId == userId);

        if (accountId.HasValue) query = query.Where(o => o.AccountId == accountId);
        if (!string.IsNullOrEmpty(type) && Enum.TryParse<OperationType>(type, true, out var opType)) 
            query = query.Where(o => o.Type == opType);
        if (categoryIds != null && categoryIds.Any()) 
            query = query.Where(o => categoryIds.Contains(o.CategoryId));
        if (from.HasValue) query = query.Where(o => o.Date >= from);
        if (to.HasValue) query = query.Where(o => o.Date <= to);

        query = sortBy switch
        {
            "dateAsc" => query.OrderBy(o => o.Date).ThenByDescending(o => o.CreatedAt),
            "amountDesc" => query.OrderByDescending(o => o.Amount).ThenByDescending(o => o.CreatedAt),
            "amountAsc" => query.OrderBy(o => o.Amount).ThenByDescending(o => o.CreatedAt),
            _ => query.OrderByDescending(o => o.Date).ThenByDescending(o => o.CreatedAt)
        };

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new OperationDto
            {
                Id = o.Id,
                AccountId = o.AccountId,
                AccountName = o.Account.Name,
                Currency = o.Account.Currency,
                CategoryId = o.CategoryId,
                CategoryName = o.Category.Name,
                Type = o.Type.ToString(),
                Amount = o.Amount,
                Date = o.Date,
                Comment = o.Comment,
                CreatedAt = o.CreatedAt
            })
            .ToListAsync();

        return Ok(new { items, page, pageSize, totalCount, totalPages });
    }

    [HttpPost]
    public async Task<IActionResult> CreateOperation(OperationRequest request)
    {
        if (!Enum.TryParse<OperationType>(request.Type, true, out var opType))
            return BadRequest(new ProblemDetails { Status = 400, Detail = "Invalid operation type." });

        if (request.Date > DateOnly.FromDateTime(DateTime.UtcNow))
            return BadRequest(new ProblemDetails { Status = 400, Detail = "Date cannot be in the future." });

        var userId = GetUserId();
        var account = await _db.Accounts.FirstOrDefaultAsync(a => a.Id == request.AccountId);
        if (account == null) return NotFound(new ProblemDetails { Status = 404, Detail = "Account not found." });
        if (account.UserId != userId) return Forbid();
        if (account.IsArchived) return Conflict(new ProblemDetails { Status = 409, Detail = "Account is archived." }); // R5

        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == request.CategoryId);
        if (category == null) return NotFound(new ProblemDetails { Status = 404, Detail = "Category not found." });
        if (category.UserId != null && category.UserId != userId) return Forbid();

        if (category.Type != opType)
            return BadRequest(new ProblemDetails { Status = 400, Detail = "Category type does not match operation type." });

        var operation = new Operation
        {
            Id = Guid.NewGuid(),
            AccountId = request.AccountId,
            CategoryId = request.CategoryId,
            Type = opType,
            Amount = request.Amount,
            Date = request.Date,
            Comment = request.Comment,
            CreatedAt = DateTime.UtcNow
        };

        _db.Operations.Add(operation);
        await _db.SaveChangesAsync();

        var dto = new OperationDto
        {
            Id = operation.Id,
            AccountId = operation.AccountId,
            AccountName = account.Name,
            Currency = account.Currency,
            CategoryId = operation.CategoryId,
            CategoryName = category.Name,
            Type = operation.Type.ToString(),
            Amount = operation.Amount,
            Date = operation.Date,
            Comment = operation.Comment,
            CreatedAt = operation.CreatedAt
        };

        return CreatedAtAction(nameof(GetOperations), new { id = operation.Id }, dto);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOperation(Guid id)
    {
        var userId = GetUserId();
        var op = await _db.Operations
            .Include(o => o.Account)
            .Include(o => o.Category)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (op == null) return NotFound();
        if (op.Account.UserId != userId) return Forbid();

        return Ok(new OperationDto
        {
            Id = op.Id,
            AccountId = op.AccountId,
            AccountName = op.Account.Name,
            Currency = op.Account.Currency,
            CategoryId = op.CategoryId,
            CategoryName = op.Category.Name,
            Type = op.Type.ToString(),
            Amount = op.Amount,
            Date = op.Date,
            Comment = op.Comment,
            CreatedAt = op.CreatedAt
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOperation(Guid id, OperationRequest request)
    {
        var userId = GetUserId();
        var operation = await _db.Operations.Include(o => o.Account).FirstOrDefaultAsync(o => o.Id == id);
        if (operation == null) return NotFound();
        if (operation.Account.UserId != userId) return Forbid();

        if (!Enum.TryParse<OperationType>(request.Type, true, out var opType))
            return BadRequest(new ProblemDetails { Status = 400, Detail = "Invalid operation type." });

        if (request.Date > DateOnly.FromDateTime(DateTime.UtcNow))
            return BadRequest(new ProblemDetails { Status = 400, Detail = "Date cannot be in the future." });

        var account = await _db.Accounts.FirstOrDefaultAsync(a => a.Id == request.AccountId);
        if (account == null) return NotFound();
        if (account.UserId != userId) return Forbid();
        if (account.IsArchived) return Conflict(new ProblemDetails { Status = 409, Detail = "Account is archived." });

        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == request.CategoryId);
        if (category == null) return NotFound();
        if (category.UserId != null && category.UserId != userId) return Forbid();
        if (category.Type != opType) return BadRequest(new ProblemDetails { Status = 400, Detail = "Category type does not match operation type." });

        operation.AccountId = request.AccountId;
        operation.CategoryId = request.CategoryId;
        operation.Type = opType;
        operation.Amount = request.Amount;
        operation.Date = request.Date;
        operation.Comment = request.Comment;

        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOperation(Guid id)
    {
        var userId = GetUserId();
        var operation = await _db.Operations.Include(o => o.Account).FirstOrDefaultAsync(o => o.Id == id);
        if (operation == null) return NotFound();
        if (operation.Account.UserId != userId) return Forbid();

        _db.Operations.Remove(operation);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}