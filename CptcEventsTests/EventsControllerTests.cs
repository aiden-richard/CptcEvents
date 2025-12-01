using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNetCore.Mvc;
using CptcEvents.Controllers;
using CptcEvents.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using CptcEvents.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Threading;

namespace CptcEventsTests
{
    [TestClass]
    public class EventsControllerTests
    {
        // Helper to access ToFullCalendarEvent without needing real IEventService
        private class DummyEventService : CptcEvents.Services.IEventService
        {
            private readonly List<Event> _events;
            public bool AddCalled { get; private set; }

            // Default constructor -> no events
            public DummyEventService() : this(null)
            {
            }

            // Overloaded constructor -> supply events to be returned by read methods
            public DummyEventService(IEnumerable<Event>? events)
            {
                _events = events?.ToList() ?? new List<Event>();
            }

            public Task AddEventAsync(Event newEvent)
            {
                AddCalled = true;
                return Task.CompletedTask;
            }

            public Task DeleteEventAsync(int id) => Task.CompletedTask;

            public Task<IEnumerable<Event>> GetAllEventsAsync() => Task.FromResult<IEnumerable<Event>>(_events);

            public Task<Event?> GetEventByIdAsync(int id) => Task.FromResult<Event?>(_events.FirstOrDefault(e => e.Id == id));

            public Task<IEnumerable<Event>> GetPublicEventsAsync() => Task.FromResult<IEnumerable<Event>>(_events.Where(e => e.IsPublic));

            public Task UpdateEventAsync(Event updatedEvent) => Task.CompletedTask;

            public Task<IEnumerable<Event>> GetEventsInRangeAsync(DateOnly start, DateOnly end) => Task.FromResult<IEnumerable<Event>>(_events.Where(e => e.DateOfEvent >= start && e.DateOfEvent <= end));
        }

        // Minimal dummy group service for constructor compatibility
        private class DummyGroupService : IGroupService
        {
            public Task<Group> CreateGroupAsync(Group group) => Task.FromResult(group);
            public Task<IEnumerable<Group>> GetGroupsForUserAsync(string userId) => Task.FromResult<IEnumerable<Group>>(new List<Group>());
            public Task<Group?> GetGroupAsync(int id) => Task.FromResult<Group?>(null);
            public Task<bool> IsUserModeratorAsync(int groupId, string userId) => Task.FromResult(false);
        }

        // Minimal IUserStore implementation for constructing a UserManager in tests
        private class NoopUserStore : IUserStore<ApplicationUser>
        {
            public Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken)
                => Task.FromResult(IdentityResult.Success);
            public Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken)
                => Task.FromResult(IdentityResult.Success);
            public void Dispose() { }
            public Task<ApplicationUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
                => Task.FromResult<ApplicationUser?>(null);
            public Task<ApplicationUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
                => Task.FromResult<ApplicationUser?>(null);
            public Task<string?> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
                => Task.FromResult<string?>(user?.UserName?.ToUpperInvariant());
            public Task<string?> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken)
                => Task.FromResult<string?>(user?.Id);
            public Task<string?> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
                => Task.FromResult<string?>(user?.UserName);
            public Task SetNormalizedUserNameAsync(ApplicationUser user, string? normalizedName, CancellationToken cancellationToken)
            {
                // noop
                return Task.CompletedTask;
            }
            public Task SetUserNameAsync(ApplicationUser user, string? userName, CancellationToken cancellationToken)
            {
                if (user != null) user.UserName = userName;
                return Task.CompletedTask;
            }
            public Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken)
                => Task.FromResult(IdentityResult.Success);
        }

        // Simple helper to create a UserManager suitable for tests
        private static UserManager<ApplicationUser> CreateTestUserManager()
        {
            var store = new NoopUserStore();
            var options = Options.Create(new IdentityOptions());
            var passwordHasher = new PasswordHasher<ApplicationUser>();
            var userValidators = new List<IUserValidator<ApplicationUser>>();
            var passwordValidators = new List<IPasswordValidator<ApplicationUser>>();
            var keyNormalizer = new UpperInvariantLookupNormalizer();
            var errors = new IdentityErrorDescriber();
            var services = null as System.IServiceProvider;
            var logger = NullLogger<UserManager<ApplicationUser>>.Instance;

            return new UserManager<ApplicationUser>(store, options, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger);
        }

        [TestMethod]
        public void ToFullCalendarEvent_AllDay_SetsStartAndEndAsDateOnly()
        {
            // Arrange
            var e = new Event
            {
                Id = 42,
                Title = "All Day Event",
                IsAllDay = true,
                DateOfEvent = new DateOnly(2025, 10, 31)
            };

            // Act
            var result = EventMapper.ToFullCalendarEvent(e) as Dictionary<string, object?>;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(42, result!["id"]);
            Assert.AreEqual("All Day Event", result["title"]);

            Assert.IsInstanceOfType(result["start"], typeof(DateOnly));
            Assert.IsInstanceOfType(result["end"], typeof(DateOnly));

            var start = (DateOnly)result["start"]!;
            var end = (DateOnly)result["end"]!;
            Assert.AreEqual(new DateOnly(2025, 10, 31), start);
            Assert.AreEqual(new DateOnly(2025, 11, 1), end);
        }

        [TestMethod]
        public void ToFullCalendarEvent_TimedEvent_FormatsIsoStrings()
        {
            // Arrange
            var e = new Event
            {
                Id = 7,
                Title = "Timed Event",
                IsAllDay = false,
                DateOfEvent = new DateOnly(2025, 12, 1),
                StartTime = new TimeOnly(9, 30, 0),
                EndTime = new TimeOnly(11, 0, 0)
            };

            // Act
            var result = EventMapper.ToFullCalendarEvent(e) as Dictionary<string, object?>;

            // Assert
            Assert.IsNotNull(result);

            Assert.AreEqual(7, result!["id"]);
            Assert.AreEqual("Timed Event", result["title"]);

            Assert.IsInstanceOfType(result["start"], typeof(string));
            Assert.IsInstanceOfType(result["end"], typeof(string));

            var startStr = (string)result["start"]!;
            var endStr = (string)result["end"]!;

            // ISO 8601 's' format: yyyy-MM-ddTHH:mm:ss
            Assert.AreEqual("2025-12-01T09:30:00", startStr);
            Assert.AreEqual("2025-12-01T11:00:00", endStr);
        }

        [TestMethod]
        public async Task GetEvents_ReturnsJsonList_ShapedForFullCalendar()
        {
            // Arrange
            var events = new List<Event>
            {
                new Event { Id = 1, Title = "AllDay", IsAllDay = true, IsPublic = true, DateOfEvent = new DateOnly(2025, 10, 5) },
                new Event { Id = 2, Title = "Timed", IsAllDay = false, IsPublic = true, DateOfEvent = new DateOnly(2025, 10, 5), StartTime = new TimeOnly(8,0), EndTime = new TimeOnly(9,0) }
            };
            var svc = new DummyEventService(events);
            var controller = new EventsController(svc, new DummyGroupService(), CreateTestUserManager());

            // Act
            var actionResult = await controller.GetEvents();

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(JsonResult));
            var json = (JsonResult)actionResult;
            var list = json.Value as List<object>;
            Assert.IsNotNull(list);
            Assert.AreEqual(2, list!.Count);

            var first = list[0] as Dictionary<string, object?>;
            var second = list[1] as Dictionary<string, object?>;
            Assert.IsNotNull(first);
            Assert.IsNotNull(second);

            Assert.AreEqual(1, first!["id"]);
            Assert.AreEqual("AllDay", first["title"]);
            Assert.IsInstanceOfType(first["start"], typeof(DateOnly));

            Assert.AreEqual(2, second!["id"]);
            Assert.AreEqual("Timed", second["title"]);
            Assert.IsInstanceOfType(second["start"], typeof(string));
        }

        [TestMethod]
        public async Task Create_Post_Valid_RedirectsAndCallsAddEvent()
        {
            // Arrange
            var svc = new DummyEventService();
            var controller = new EventsController(svc, new DummyGroupService(), CreateTestUserManager());
            var newEvent = new Event { Id = 10, Title = "New Event", DateOfEvent = new DateOnly(2025, 9, 1), IsAllDay = true };

            // Act
            var result = await controller.Create(newEvent);

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirect = (RedirectToActionResult)result;
            Assert.AreEqual("Index", redirect.ActionName);
            Assert.IsTrue(svc.AddCalled, "AddEventAsync should have been called for valid model");
        }

        [TestMethod]
        public async Task Create_Post_Invalid_ReturnsViewWithModel()
        {
            // Arrange
            var svc = new DummyEventService();
            var controller = new EventsController(svc, new DummyGroupService(), CreateTestUserManager());
            var newEvent = new Event { Id = 11, Title = "Invalid Event", DateOfEvent = new DateOnly(2025, 9, 2) };
            controller.ModelState.AddModelError("Title", "Required");

            // Act
            var result = await controller.Create(newEvent);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var view = (ViewResult)result;
            Assert.AreSame(newEvent, view.Model);
            Assert.IsFalse(svc.AddCalled, "AddEventAsync should not be called when model is invalid");
        }

        [TestMethod]
        public async Task GetEventsInRange_ReturnsEventsWithinRange()
        {
            // Arrange
            var events = new List<Event>
            {
                new Event { Id = 1, Title = "A", IsAllDay = true, DateOfEvent = new DateOnly(2025, 10, 1) },
                new Event { Id = 2, Title = "B", IsAllDay = true, DateOfEvent = new DateOnly(2025, 10, 5) },
                new Event { Id = 3, Title = "C", IsAllDay = true, DateOfEvent = new DateOnly(2025, 10, 10) }
            };
            var svc = new DummyEventService(events);
            var controller = new EventsController(svc, new DummyGroupService(), CreateTestUserManager());

            // Act
            var actionResult = await controller.GetEventsInRange(new DateOnly(2025, 10, 1), new DateOnly(2025, 10, 5));

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(JsonResult));
            var json = (JsonResult)actionResult;
            var list = json.Value as List<object>;
            Assert.IsNotNull(list);
            Assert.AreEqual(2, list!.Count);

            var ids = list.Select(x => ((Dictionary<string, object?>)x)["id"]).ToList();
            CollectionAssert.AreEquivalent(new List<object> { 1, 2 }, ids);
        }

        [TestMethod]
        public async Task GetEventsInRange_EndBeforeStart_ReturnsBadRequest()
        {
            // Arrange
            var svc = new DummyEventService(new List<Event>());
            var controller = new EventsController(svc, new DummyGroupService(), CreateTestUserManager());

            // Act
            var actionResult = await controller.GetEventsInRange(new DateOnly(2025, 10, 5), new DateOnly(2025, 10, 1));

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(BadRequestObjectResult));
        }
    }
}
