using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNetCore.Mvc;
using CptcEvents.Controllers;
using CptcEvents.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace CptcEventsTests
{
    [TestClass]
    public class EventsControllerTests
    {
        /*
           1) DummyEventService:
              - Minimal implementation that tracks whether AddEventAsync was called.
              - Returns empty collections or null for read methods.
              - Used in tests that only need to verify side-effects (e.g., Create POST).
           2) DummyEventServiceWithEvents:
              - Accepts a list of Event objects in its constructor and returns them from GetAll/GetPublic.
              - Used in tests that need the controller to produce shaped JSON from a known events list.
         - Add an explanatory comment above the second fake explaining why two exist instead of one combined fake.
        */

        // Helper to access ToFullCalendarEvent without needing real IEventService
        private class DummyEventService : CptcEvents.Services.IEventService
        {
            public bool AddCalled { get; private set; }

            public Task AddEventAsync(Event newEvent)
            {
                AddCalled = true;
                return Task.CompletedTask;
            }
            public Task DeleteEventAsync(int id) => Task.CompletedTask;
            public Task<IEnumerable<Event>> GetAllEventsAsync() => Task.FromResult<IEnumerable<Event>>(new List<Event>());
            public Task<Event?> GetEventByIdAsync(int id) => Task.FromResult<Event?>(null);
            public Task<IEnumerable<Event>> GetPublicEventsAsync() => Task.FromResult<IEnumerable<Event>>(new List<Event>());
            public Task UpdateEventAsync(Event updatedEvent) => Task.CompletedTask;
            public Task<IEnumerable<Event>> GetEventsInRangeAsync(DateOnly start, DateOnly end) => Task.FromResult<IEnumerable<Event>>(new List<Event>());
        }

        private class DummyEventServiceWithEvents : CptcEvents.Services.IEventService
        {
            private readonly IEnumerable<Event> _events;
            public DummyEventServiceWithEvents(IEnumerable<Event> events)
            {
                _events = events;
            }

            public Task AddEventAsync(Event newEvent) => Task.CompletedTask;
            public Task DeleteEventAsync(int id) => Task.CompletedTask;
            public Task<IEnumerable<Event>> GetAllEventsAsync() => Task.FromResult(_events);
            public Task<Event?> GetEventByIdAsync(int id) => Task.FromResult<Event?>(null);
            public Task<IEnumerable<Event>> GetPublicEventsAsync() => Task.FromResult(_events);
            public Task UpdateEventAsync(Event updatedEvent) => Task.CompletedTask;
            public Task<IEnumerable<Event>> GetEventsInRangeAsync(DateOnly start, DateOnly end) => Task.FromResult(_events.Where(e => e.DateOfEvent >= start && e.DateOfEvent <= end));
        }

        [TestMethod]
        public void ToFullCalendarEvent_AllDay_SetsStartAndEndAsDateOnly()
        {
            // Arrange
            var controller = new EventsController(new DummyEventService());
            var e = new Event
            {
                Id = 42,
                Title = "All Day Event",
                IsAllDay = true,
                DateOfEvent = new DateOnly(2025, 10, 31)
            };

            // Act
            var result = controller.ToFullCalendarEvent(e) as Dictionary<string, object?>;

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
            var controller = new EventsController(new DummyEventService());
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
            var result = controller.ToFullCalendarEvent(e) as Dictionary<string, object?>;

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
                new Event { Id = 1, Title = "AllDay", IsAllDay = true, DateOfEvent = new DateOnly(2025, 10, 5) },
                new Event { Id = 2, Title = "Timed", IsAllDay = false, DateOfEvent = new DateOnly(2025, 10, 5), StartTime = new TimeOnly(8,0), EndTime = new TimeOnly(9,0) }
            };
            var svc = new DummyEventServiceWithEvents(events);
            var controller = new EventsController(svc);

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
            var controller = new EventsController(svc);
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
            var controller = new EventsController(svc);
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
            var svc = new DummyEventServiceWithEvents(events);
            var controller = new EventsController(svc);

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
            var svc = new DummyEventServiceWithEvents(new List<Event>());
            var controller = new EventsController(svc);

            // Act
            var actionResult = await controller.GetEventsInRange(new DateOnly(2025, 10, 5), new DateOnly(2025, 10, 1));

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(BadRequestObjectResult));
        }
    }
}
