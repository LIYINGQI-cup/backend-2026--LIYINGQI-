using FinanceTracker.Api.Controllers;
using FinanceTracker.Api.Dtos;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using FinanceTracker.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FinanceTracker.Tests;

public class OperationsTests
{
    private AppDbContext GetInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task CreateOperation_FutureDate_Returns400()
    {
        var db = GetInMemoryDb();
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        db.Accounts.Add(new Account { Id = accountId, UserId = userId, Currency = "RUB", IsArchived = false });
        db.Categories.Add(new Category { Id = categoryId, UserId = userId, Type = OperationType.Expense });
        await db.SaveChangesAsync();

        var controller = new OperationsController(db);
        
        var user = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(new[]
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, userId.ToString())
        }));
        controller.ControllerContext = new ControllerContext { HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = user } };

        var request = new OperationRequest
        {
            AccountId = accountId,
            CategoryId = categoryId,
            Type = "Expense",
            Amount = 100,
            Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)) // Future date (R4)
        };

        var result = await controller.CreateOperation(request);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var problem = Assert.IsType<ProblemDetails>(badRequest.Value);
        Assert.Equal(400, problem.Status);
    }

    [Fact]
    public async Task CreateOperation_ArchivedAccount_Returns409()
    {
        var db = GetInMemoryDb();
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        db.Accounts.Add(new Account { Id = accountId, UserId = userId, Currency = "RUB", IsArchived = true }); // Archived (R5)
        db.Categories.Add(new Category { Id = categoryId, UserId = userId, Type = OperationType.Expense });
        await db.SaveChangesAsync();

        var controller = new OperationsController(db);
        var user = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(new[]
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, userId.ToString())
        }));
        controller.ControllerContext = new ControllerContext { HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = user } };

        var request = new OperationRequest
        {
            AccountId = accountId,
            CategoryId = categoryId,
            Type = "Expense",
            Amount = 100,
            Date = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        var result = await controller.CreateOperation(request);

        var conflict = Assert.IsType<ConflictObjectResult>(result);
        var problem = Assert.IsType<ProblemDetails>(conflict.Value);
        Assert.Equal(409, problem.Status);
    }
}