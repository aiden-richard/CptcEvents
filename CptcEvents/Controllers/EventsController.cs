using CptcEvents.Models;
using CptcEvents.Services;
using Microsoft.AspNetCore.Mvc;
using CptcEvents.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace CptcEvents.Controllers
{
    [Authorize]
    public class EventsController : Controller
    {
        private readonly IEventService _eventsService;
        private readonly IGroupService _groupService;
        private readonly UserManager<ApplicationUser> _userManager;

        public EventsController(IEventService eventsService, IGroupService groupService, UserManager<ApplicationUser> userManager)
        {
            _eventsService = eventsService;
            _groupService = groupService;
            _userManager = userManager;
        }

        #region Event CRUD Operations

        // GET: Events or Events/{eventId}
        [HttpGet("Events/{eventId?}")]
        public async Task<IActionResult> Index(int? eventId)
        {
            string? userId = User?.Identity?.IsAuthenticated == true ? _userManager.GetUserId(User) : null;
            if (userId == null)
            {
                return Challenge();
            }

            IEnumerable<Event> events = await _eventsService.GetEventsForUserAsync(userId);

            return View(events);
        }

        // GET: Events/Details/5
        [HttpGet("Events/Details/{eventId}")]
        public async Task<IActionResult> Details(int eventId)
        {
            string? userId = User?.Identity?.IsAuthenticated == true ? _userManager.GetUserId(User) : null;

            Event? eventItem = await _eventsService.GetEventByIdAsync(eventId);
            if (eventItem == null)
            {
                return NotFound();
            }

            // If user isn't authenticated, only allow viewing public events
            if (userId == null && !eventItem.IsPublic)
            {
                return Challenge();
            }

            // If authenticated, check membership for private events
            if (userId != null && !eventItem.IsPublic)
            {
                bool isMember = await _groupService.IsUserMemberAsync(eventItem.GroupId, userId);
                if (!isMember)
                {
                    return Forbid();
                }
            }

            return View(eventItem);
        }

        // GET: Events/Create
        [HttpGet("Events/Create")]
        public async Task<IActionResult> Create()
        {
            // Ensure user is authenticated
            string? userId = User?.Identity?.IsAuthenticated == true ? _userManager.GetUserId(User) : null;
            if (userId == null)
            {
                return Challenge();
            }

            // Load groups for the current user
            await PopulateGroupsSelectListAsync();
            return View();
        }

        // POST: Events/Create
        [HttpPost("Events/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventViewModel model)
        {
            string? userId = User?.Identity?.IsAuthenticated == true ? _userManager.GetUserId(User) : null;
            if (userId == null)
            {
                return Challenge();
            }

            if (!ModelState.IsValid)
            {
                await PopulateGroupsSelectListAsync();
                return View(model);
            }

            // Verify user is at least a moderator of the group to create events
            bool isModerator = await _groupService.IsUserModeratorAsync(model.GroupId, userId);
            if (!isModerator)
            {
                ModelState.AddModelError(string.Empty, "You must be a moderator of the group to create events.");
                await PopulateGroupsSelectListAsync();
                return View(model);
            }

            Event newEvent = new Event
            {
                Title = model.Title,
                Description = model.Description,
                GroupId = model.GroupId,
                IsPublic = model.IsPublic,
                IsAllDay = model.IsAllDay,
                DateOfEvent = model.DateOfEvent,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                Url = model.Url
            };

            await _eventsService.CreateEventAsync(newEvent);

            return RedirectToAction(nameof(Index));
        }

        // GET: Events/Edit/5
        [HttpGet("Events/Edit/{eventId}")]
        public async Task<IActionResult> Edit(int eventId)
        {
            string? userId = User?.Identity?.IsAuthenticated == true ? _userManager.GetUserId(User) : null;
            if (userId == null)
            {
                return Challenge();
            }

            Event? eventItem = await _eventsService.GetEventByIdAsync(eventId);
            if (eventItem == null)
            {
                return NotFound();
            }

            // Check if user is at least a moderator of the event's group
            bool isModerator = await _groupService.IsUserModeratorAsync(eventItem.GroupId, userId);
            if (!isModerator)
            {
                return Forbid();
            }

            var viewModel = new EventEditViewModel
            {
                Id = eventItem.Id,
                Title = eventItem.Title,
                Description = eventItem.Description,
                GroupId = eventItem.GroupId,
                GroupName = eventItem.Group?.Name,
                IsPublic = eventItem.IsPublic,
                IsAllDay = eventItem.IsAllDay,
                DateOfEvent = eventItem.DateOfEvent,
                StartTime = eventItem.StartTime,
                EndTime = eventItem.EndTime,
                Url = eventItem.Url,
                IsModerator = isModerator
            };

            return View(viewModel);
        }

        // POST: Events/Edit/5
        [HttpPost("Events/Edit/{eventId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int eventId, EventEditViewModel model)
        {
            string? userId = User?.Identity?.IsAuthenticated == true ? _userManager.GetUserId(User) : null;
            if (userId == null)
            {
                return Challenge();
            }

            if (eventId != model.Id)
            {
                return BadRequest();
            }

            // Retrieve existing event
            Event? existingEvent = await _eventsService.GetEventByIdAsync(eventId);
            if (existingEvent == null)
            {
                return NotFound();
            }

            // Check if user is at least a moderator of the event's group
            bool isModerator = await _groupService.IsUserModeratorAsync(existingEvent.GroupId, userId);
            if (!isModerator)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                model.IsModerator = isModerator;
                model.GroupName = existingEvent.Group?.Name;
                return View(model);
            }

            Event? result = await _eventsService.UpdateEventAsync(existingEvent.Id, new Event
            {
                Title = model.Title,
                Description = model.Description,
                IsPublic = model.IsPublic,
                IsAllDay = model.IsAllDay,
                DateOfEvent = model.DateOfEvent,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                Url = model.Url
            });
            if (result == null)
            {
                ModelState.AddModelError(string.Empty, "Failed to update event.");
                model.IsModerator = isModerator;
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Events/Delete/5
        [HttpGet("Events/Delete/{eventId}")]
        public async Task<IActionResult> Delete(int eventId)
        {
            string? userId = User?.Identity?.IsAuthenticated == true ? _userManager.GetUserId(User) : null;
            if (userId == null)
            {
                return Challenge();
            }

            Event? eventItem = await _eventsService.GetEventByIdAsync(eventId);
            if (eventItem == null)
            {
                return NotFound();
            }

            // Check if user is at least a moderator of the event's group
            bool isModerator = await _groupService.IsUserModeratorAsync(eventItem.GroupId, userId);
            if (!isModerator)
            {
                return Forbid();
            }

            return View(eventItem);
        }

        // POST: Events/Delete/5
        [HttpPost("Events/Delete/{eventId}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int eventId)
        {
            string? userId = User?.Identity?.IsAuthenticated == true ? _userManager.GetUserId(User) : null;
            if (userId == null)
            {
                return Challenge();
            }

            Event? eventItem = await _eventsService.GetEventByIdAsync(eventId);
            if (eventItem == null)
            {
                return NotFound();
            }

            // Check if user is at least a moderator of the event's group
            bool isModerator = await _groupService.IsUserModeratorAsync(eventItem.GroupId, userId);
            if (!isModerator)
            {
                return Forbid();
            }

            await _eventsService.DeleteEventAsync(eventId);

            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Helper Methods

        // Helper that populates ViewData["Groups"] with groups available to the current user.
        private async Task PopulateGroupsSelectListAsync()
        {
            var groups = new List<Group>();
            if (User?.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    // Only get groups where user is at least a moderator (can create events)
                    var allGroups = await _groupService.GetGroupsForUserAsync(user.Id);
                    foreach (var group in allGroups)
                    {
                        bool isModerator = await _groupService.IsUserModeratorAsync(group.Id, user.Id);
                        if (isModerator)
                        {
                            groups.Add(group);
                        }
                    }
                }
            }

            ViewData["Groups"] = new SelectList(groups, "Id", "Name");
        }

        #endregion

        #region API Endpoints

        /// <summary>
        /// Retrieves all public calendar events and returns their data formatted for FullCalendar.
        /// </summary>
        /// <remarks>This method is intended for use by pages that require event data
        /// formatted for the FullCalendar JavaScript library. The returned list will be empty if no events are
        /// found.</remarks>
        /// <returns>A JSON result containing a list of event objects formatted for FullCalendar.</returns>
        [AllowAnonymous]
        public async Task<IActionResult> GetEvents()
        {
            // Get events from the database
            IEnumerable<Event> events = await _eventsService.GetPublicEventsAsync();

            // Create a list to hold FullCalendar-compatible events
            List<object> fullCalendarEvents = new List<object>();

            // Loop through the events and add them to the list
            foreach (Event e in events)
            {
                fullCalendarEvents.Add(EventMapper.ToFullCalendarEvent(e));
            }

            return Json(fullCalendarEvents);
        }

        /// <summary>
        /// Returns events within an inclusive date range formatted for FullCalendar.
        /// Query parameters: start=yyyy-MM-dd, end=yyyy-MM-dd
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetEventsInRange([FromQuery] DateOnly start, [FromQuery] DateOnly end)
        {
            if (end < start)
            {
                return BadRequest("End date must be on or after start date.");
            }

            IEnumerable<Event> events = await _eventsService.GetEventsInRangeAsync(start, end);
            var fullCalendarEvents = events.Select(e => EventMapper.ToFullCalendarEvent(e)).ToList();
            return Json(fullCalendarEvents);
        }

        #endregion
    }
}
