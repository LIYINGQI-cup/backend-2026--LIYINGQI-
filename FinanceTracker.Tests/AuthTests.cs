using FinanceTracker.Api.Controllers;
using FinanceTracker.Api.Dtos;
using FinanceTracker.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;

namespace FinanceTracker.Tests;

public class AuthTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly AuthController _controller;

    public AuthTests()
    {
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        var jwtSettings = Options.Create(new JwtSettings
        {
            Key = "test-secret-key-at-least-32-characters-long",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenMinutes = 60
        });
        var jwtService = new JwtService(jwtSettings);

        _controller = new AuthController(_userManagerMock.Object, jwtService);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409Conflict()
    {
        var request = new RegisterRequest { Email = "test@example.com", Password = "Password123", Name = "Test" };
        
        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(new ApplicationUser());

        var result = await _controller.Register(request);

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task Register_WeakPassword_Returns400ValidationProblem()
    {
        var request = new RegisterRequest { Email = "weak@example.com", Password = "123", Name = "Test" };
        
        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync((ApplicationUser?)null);
            
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), request.Password))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password too weak" }));

        var result = await _controller.Register(request);

        var objectResult = (ObjectResult)result;
        Assert.Equal(400, objectResult.StatusCode);
    }

    [Fact]
    public async Task Login_InvalidCredentials_Returns401Unauthorized()
    {
        var request = new LoginRequest { Email = "notfound@example.com", Password = "WrongPassword1" };
        
        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync((ApplicationUser?)null);

        var result = await _controller.Login(request);

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Login_EmptyEmail_Returns401Unauthorized()
    {
        var request = new LoginRequest { Email = "", Password = "Password123" };
        
        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync((ApplicationUser?)null);

        var result = await _controller.Login(request);

        Assert.IsType<UnauthorizedObjectResult>(result);
    }
}