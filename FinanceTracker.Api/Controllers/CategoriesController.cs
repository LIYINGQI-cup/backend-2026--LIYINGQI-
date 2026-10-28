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
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _db;

    public CategoriesController(AppDbContext db)
    {
        _db = db;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    [HttpGet]
    public async Task<IActionResult> GetCategories([FromQuery] string? type)
    {
        var userId = GetUserId();

        var query = _db.Categories
            .Where(c => c.IsSystem || c.UserId == userId);

        if (!string.IsNullOrEmpty(type) && Enum.TryParse<OperationType>(type, true, out var opType))
        {
            query = query.Where(c => c.Type == opType);
        }

        var categories = await query
            .OrderByDescending(c => c.IsSystem)
            .ThenBy(c => c.Name)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Type = c.Type.ToString(),
                IsSystem = c.IsSystem
            })
            .ToListAsync();

        return Ok(categories);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory(CreateCategoryRequest request)
    {
        if (!Enum.TryParse<OperationType>(request.Type, true, out var opType))
        {
            return BadRequest(new ProblemDetails { Status = 400, Detail = "Invalid category type." });
        }

        var userId = GetUserId();

        var exists = await _db.Categories.AnyAsync(c =>
            (c.IsSystem || c.UserId == userId) &&
            c.Name.ToLower() == request.Name.ToLower() &&
            c.Type == opType);

        if (exists)
        {
            return Conflict(new ProblemDetails { Status = 409, Detail = "Category with the same name and type already exists." });
        }

        var category = new Category
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = request.Name,
            Type = opType,
            IsSystem = false
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCategories), new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Type = category.Type.ToString(),
            IsSystem = false
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(Guid id, UpdateCategoryRequest request)
    {
        var userId = GetUserId();
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id);

        if (category == null) return NotFound();

        if (category.IsSystem || category.UserId != userId)
        {
            return Forbid();
        }

        var exists = await _db.Categories.AnyAsync(c =>
            c.Id != id &&
            (c.IsSystem || c.UserId == userId) &&
            c.Name.ToLower() == request.Name.ToLower() &&
            c.Type == category.Type);

        if (exists)
        {
            return Conflict(new ProblemDetails { Status = 409, Detail = "Category with the same name and type already exists." });
        }

        category.Name = request.Name;
        await _db.SaveChangesAsync();

        return Ok(new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Type = category.Type.ToString(),
            IsSystem = false
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        var userId = GetUserId();
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id);

        if (category == null) return NotFound();

        if (category.IsSystem || category.UserId != userId)
        {
            return Forbid();
        }

        var isUsed = await _db.Operations.AnyAsync(o => o.CategoryId == id);
        if (isUsed)
        {
            return Conflict(new ProblemDetails { Status = 409, Detail = "Category is used by operations." });
        }

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}