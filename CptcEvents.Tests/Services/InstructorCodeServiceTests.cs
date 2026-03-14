using CptcEvents.Models;
using CptcEvents.Services;
using CptcEvents.Tests.Helpers;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Moq;

namespace CptcEvents.Tests.Services;

/// <summary>
/// Unit tests for <see cref="InstructorCodeService"/>.
/// Covers UC1 (Register — instructor code validation) and UC11 (Manage instructor codes).
/// </summary>
public class InstructorCodeServiceTests
{
    private static InstructorCodeService CreateService(Data.ApplicationDbContext ctx)
    {
        var emailSender = new Mock<IEmailSender>();
        emailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var config = new Mock<IConfiguration>();
        config.Setup(c => c["AppSettings:BaseUrl"]).Returns("https://localhost:7134");

        return new InstructorCodeService(ctx, emailSender.Object, config.Object);
    }

    // UC1: Register — instructor code validation

    [Fact]
    public async Task ValidateCode_ValidCode_ReturnsTrue()
    {
        // Arrange
        using var ctx = TestDbContextFactory.Create();
        ctx.InstructorCodes.Add(new InstructorCode
        {
            Code = "VALID123",
            Email = "instructor@example.com",
            IsActive = true
        });
        await ctx.SaveChangesAsync();
        var service = CreateService(ctx);

        // Act
        bool result = await service.ValidateCodeAsync("VALID123", "instructor@example.com");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ValidateCode_InvalidCode_ReturnsFalse()
    {
        // Arrange — UC1 A3: code does not exist
        using var ctx = TestDbContextFactory.Create();
        var service = CreateService(ctx);

        // Act
        bool result = await service.ValidateCodeAsync("NOSUCHCODE", "instructor@example.com");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ValidateCode_InactiveCode_ReturnsFalse()
    {
        // Arrange — UC1 A3: code exists but is inactive (already used)
        using var ctx = TestDbContextFactory.Create();
        ctx.InstructorCodes.Add(new InstructorCode
        {
            Code = "USED1234",
            Email = "instructor@example.com",
            IsActive = false
        });
        await ctx.SaveChangesAsync();
        var service = CreateService(ctx);

        // Act
        bool result = await service.ValidateCodeAsync("USED1234", "instructor@example.com");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ValidateCode_WrongEmail_ReturnsFalse()
    {
        // Arrange — UC1 A3: code exists but email does not match
        using var ctx = TestDbContextFactory.Create();
        ctx.InstructorCodes.Add(new InstructorCode
        {
            Code = "VALID123",
            Email = "instructor@example.com",
            IsActive = true
        });
        await ctx.SaveChangesAsync();
        var service = CreateService(ctx);

        // Act
        bool result = await service.ValidateCodeAsync("VALID123", "different@example.com");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ValidateCode_ExpiredCode_ReturnsFalse()
    {
        // Arrange — UC1 A3: code exists but is expired
        using var ctx = TestDbContextFactory.Create();
        ctx.InstructorCodes.Add(new InstructorCode
        {
            Code = "EXPIRED1",
            Email = "instructor@example.com",
            IsActive = true,
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1)
        });
        await ctx.SaveChangesAsync();
        var service = CreateService(ctx);

        // Act
        bool result = await service.ValidateCodeAsync("EXPIRED1", "instructor@example.com");

        // Assert
        Assert.False(result);
    }

    // UC11: Manage instructor registration codes

    [Fact]
    public async Task CreateCode_PersistsCode()
    {
        // Arrange
        using var ctx = TestDbContextFactory.Create();
        var service = CreateService(ctx);

        // Act
        var code = await service.CreateCodeAsync("NEWCODE1", "new@example.com", null, "admin-1");

        // Assert
        Assert.NotNull(code);
        Assert.Equal("NEWCODE1", code.Code);
        Assert.True(ctx.InstructorCodes.Any(c => c.Code == "NEWCODE1"));
    }

    [Fact]
    public async Task DeleteCode_RemovesCode()
    {
        // Arrange
        using var ctx = TestDbContextFactory.Create();
        var service = CreateService(ctx);
        var code = await service.CreateCodeAsync("DEL00001", "del@example.com", null, "admin-1");

        // Act
        bool deleted = await service.DeleteCodeAsync(code.Id);

        // Assert
        Assert.True(deleted);
        Assert.False(ctx.InstructorCodes.Any(c => c.Id == code.Id));
    }

    [Fact]
    public async Task DeleteCode_NonExistentCode_ReturnsFalse()
    {
        // Arrange — UC11 A2: code does not exist
        using var ctx = TestDbContextFactory.Create();
        var service = CreateService(ctx);

        // Act
        bool result = await service.DeleteCodeAsync(9999);

        // Assert
        Assert.False(result);
    }
}
