using CptcEvents.Data;
using Microsoft.EntityFrameworkCore;

namespace CptcEvents.Tests.Helpers;

/// <summary>
/// Creates a fresh in-memory <see cref="ApplicationDbContext"/> for each test.
/// Using a unique database name per call guarantees test isolation.
/// </summary>
public static class TestDbContextFactory
{
    public static ApplicationDbContext Create(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
