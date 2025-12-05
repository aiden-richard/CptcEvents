using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using CptcEvents.Services;
using CptcEvents.Data;
using CptcEvents.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace CptcEventsTests
{
    /// <summary>
    /// Unit tests for the <see cref="EventService"/> class.
    /// Tests cover CRUD operations and date range filtering using an in-memory database.
    /// </summary>
    [TestClass]
    public class EventServiceTests
    {
        /// <summary>
        /// Creates <see cref="DbContextOptions{TContext}"/> configured to use an in-memory database.
        /// Each test should use a unique database name to ensure test isolation.
        /// </summary>
        /// <param name="dbName">A unique name for the in-memory database.</param>
        /// <returns>Options configured for in-memory database usage.</returns>
        private static DbContextOptions<ApplicationDbContext> CreateOptions(string dbName)
        {
            return new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
        }

        /// <summary>
        /// Verifies that <see cref="EventService.GetEventsInRangeAsync"/> returns only events
        /// within the inclusive date range (start and end dates are included).
        /// </summary>
        [TestMethod]
        public async Task GetEventsInRangeAsync_ReturnsOnlyEventsWithinInclusiveRange()
        {
            // Arrange
            var options = CreateOptions("range_test_db");
            using (var context = new ApplicationDbContext(options))
            {
                context.Events.AddRange(new List<Event>
                {
                    new Event { Id = 1, Title = "Before", DateOfEvent = new DateOnly(2025, 9, 30) },
                    new Event { Id = 2, Title = "Start", DateOfEvent = new DateOnly(2025, 10, 1) },
                    new Event { Id = 3, Title = "Middle", DateOfEvent = new DateOnly(2025, 10, 3) },
                    new Event { Id = 4, Title = "End", DateOfEvent = new DateOnly(2025, 10, 5) },
                    new Event { Id = 5, Title = "After", DateOfEvent = new DateOnly(2025, 10, 6) }
                });
                await context.SaveChangesAsync();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var svc = new EventService(context);

                // Act
                var results = (await svc.GetEventsInRangeAsync(new DateOnly(2025, 10, 1), new DateOnly(2025, 10, 5))).ToList();

                // Assert
                var ids = results.Select(r => r.Id).OrderBy(x => x).ToList();
                CollectionAssert.AreEqual(new List<int> { 2, 3, 4 }, ids);
            }
        }

        /// <summary>
        /// Verifies that <see cref="EventService.GetEventsInRangeAsync"/> returns an empty
        /// collection when no events fall within the specified date range.
        /// </summary>
        [TestMethod]
        public async Task GetEventsInRangeAsync_EmptyWhenNoMatches()
        {
            // Arrange
            var options = CreateOptions("range_empty_db");
            using (var context = new ApplicationDbContext(options))
            {
                context.Events.AddRange(new List<Event>
                {
                    new Event { Id = 1, Title = "Other", DateOfEvent = new DateOnly(2025, 11, 1) }
                });
                await context.SaveChangesAsync();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var svc = new EventService(context);

                // Act
                var results = (await svc.GetEventsInRangeAsync(new DateOnly(2025, 10, 1), new DateOnly(2025, 10, 5))).ToList();

                // Assert
                Assert.AreEqual(0, results.Count);
            }
        }

        /// <summary>
        /// Verifies that <see cref="EventService.GetPublicEventsAsync"/> returns only
        /// events where <see cref="Event.IsPublic"/> is true.
        /// </summary>
        [TestMethod]
        public async Task GetPublicEventsAsync_ReturnsOnlyPublicEvents()
        {
            // Arrange
            var options = CreateOptions("public_test_db");
            using (var context = new ApplicationDbContext(options))
            {
                context.Events.AddRange(new List<Event>
                {
                    new Event { Id = 1, Title = "Pub1", IsPublic = true, DateOfEvent = new DateOnly(2025, 10, 1) },
                    new Event { Id = 2, Title = "Priv", IsPublic = false, DateOfEvent = new DateOnly(2025, 10, 2) },
                    new Event { Id = 3, Title = "Pub2", IsPublic = true, DateOfEvent = new DateOnly(2025, 10, 3) }
                });
                await context.SaveChangesAsync();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var svc = new EventService(context);

                // Act
                var results = (await svc.GetPublicEventsAsync()).ToList();

                // Assert
                var ids = results.Select(r => r.Id).OrderBy(x => x).ToList();
                CollectionAssert.AreEqual(new List<int> { 1, 3 }, ids);
            }
        }

        /// <summary>
        /// Verifies that <see cref="EventService.AddEventAsync"/> correctly persists
        /// a new event to the database.
        /// </summary>
        [TestMethod]
        public async Task AddEventAsync_PersistsEvent()
        {
            // Arrange
            var options = CreateOptions("add_test_db");
            using (var context = new ApplicationDbContext(options))
            {
                // Create a group first since events require a GroupId
                context.Groups.Add(new Group { Id = 1, Name = "Test Group", OwnerId = "test-user" });
                await context.SaveChangesAsync();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var svc = new EventService(context);
                var e = new Event { Title = "New", DateOfEvent = new DateOnly(2025, 12, 1), GroupId = 1 };

                // Act
                await svc.AddEventAsync(e);
            }

            using (var context = new ApplicationDbContext(options))
            {
                // Assert
                var all = await context.Events.ToListAsync();
                Assert.AreEqual(1, all.Count);
                Assert.AreEqual("New", all[0].Title);
            }
        }

        /// <summary>
        /// Verifies that <see cref="EventService.UpdateEventAsync"/> correctly updates
        /// an existing event's properties in the database.
        /// </summary>
        [TestMethod]
        public async Task UpdateEventAsync_UpdatesExistingEvent()
        {
            // Arrange
            var options = CreateOptions("update_test_db");
            using (var context = new ApplicationDbContext(options))
            {
                context.Events.Add(new Event { Id = 100, Title = "Old", DateOfEvent = new DateOnly(2025, 12, 2) });
                await context.SaveChangesAsync();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var svc = new EventService(context);
                var updated = new Event { Id = 100, Title = "Updated", DateOfEvent = new DateOnly(2025, 12, 2) };

                // Act
                await svc.UpdateEventAsync(100, updated);
            }

            using (var context = new ApplicationDbContext(options))
            {
                var got = await context.Events.FindAsync(100);
                Assert.IsNotNull(got);
                Assert.AreEqual("Updated", got!.Title);
            }
        }

        /// <summary>
        /// Verifies that <see cref="EventService.DeleteEventAsync"/> correctly removes
        /// an event from the database.
        /// </summary>
        [TestMethod]
        public async Task DeleteEventAsync_RemovesEvent()
        {
            // Arrange
            var options = CreateOptions("delete_test_db");
            using (var context = new ApplicationDbContext(options))
            {
                context.Events.Add(new Event { Id = 200, Title = "ToDelete", DateOfEvent = new DateOnly(2025, 12, 3) });
                await context.SaveChangesAsync();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var svc = new EventService(context);

                // Act
                await svc.DeleteEventAsync(200);
            }

            using (var context = new ApplicationDbContext(options))
            {
                var got = await context.Events.FindAsync(200);
                Assert.IsNull(got);
            }
        }

        /// <summary>
        /// Verifies that <see cref="EventService.GetEventsInRangeAsync"/> returns an empty
        /// collection when the start date is after the end date (invalid range).
        /// </summary>
        [TestMethod]
        public async Task GetEventsInRangeAsync_StartAfterEnd_ReturnsEmpty()
        {
            // Arrange
            var options = CreateOptions("range_start_after_end_db");
            using (var context = new ApplicationDbContext(options))
            {
                context.Events.AddRange(new List<Event>
                {
                    new Event { Id = 1, Title = "Only", DateOfEvent = new DateOnly(2025, 10, 3) }
                });
                await context.SaveChangesAsync();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var svc = new EventService(context);

                // Act
                var results = (await svc.GetEventsInRangeAsync(new DateOnly(2025, 10, 5), new DateOnly(2025, 10, 1))).ToList();

                // Assert
                Assert.AreEqual(0, results.Count);
            }
        }
    }
}
