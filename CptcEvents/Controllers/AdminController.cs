using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CptcEvents.Services;
using Microsoft.AspNetCore.Identity;
using CptcEvents.Models;
using CptcEvents.Models.ViewModels;

namespace CptcEvents.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IInstructorCodeService _instructorCodeService;
        private readonly IEventService _eventService;
        private readonly UserManager<Models.ApplicationUser> _userManager;

        public AdminController(IInstructorCodeService instructorCodeService, IEventService eventService, UserManager<Models.ApplicationUser> userManager)
        {
            _instructorCodeService = instructorCodeService;
            _eventService = eventService;
            _userManager = userManager;
        }

        /// <summary>
        /// Displays the main admin dashboard.
        /// GET /Admin
        /// </summary>
        /// <returns>Admin dashboard view.</returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Dashboard for managing instructor codes.
        /// GET /Admin/ManageInstructorCodes
        /// </summary>
        /// <returns>View listing all instructor codes.</returns>
        public async Task<IActionResult> ManageInstructorCodes()
        {
            var codes = await _instructorCodeService.GetAllCodesAsync();
            return View(codes);
        }

        /// <summary>
        /// Show the form to create a new instructor code.
        /// GET /Admin/CreateInstructorCode
        /// </summary>
        /// <returns>Instructor code creation form view.</returns>
        public IActionResult CreateInstructorCode()
        {
            return View(new CreateInstructorCodeViewModel());
        }

        /// <summary>
        /// Processes the creation of a new instructor code.
        /// POST /Admin/CreateInstructorCode
        /// </summary>
        /// <param name="model">The instructor code creation form data.</param>
        /// <returns>Redirects to ManageInstructorCodes on success, or form view with validation errors on failure.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateInstructorCode(CreateInstructorCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Generate a random code
            var code = await _instructorCodeService.GenerateUniqueInstructorCodeAsync(8);

            DateTime? expiresAt = model.Expires ? model.ExpiresAt?.ToUniversalTime() : null;

            var currentUser = await _userManager.GetUserAsync(User);
            await _instructorCodeService.CreateCodeAsync(code, model.Email, expiresAt, currentUser?.UserName ?? currentUser?.Email);

            TempData["Success"] = $"Instructor code '{code}' created successfully.";
            return RedirectToAction(nameof(ManageInstructorCodes));
        }

        /// <summary>
        /// Processes the deletion of an instructor code.
        /// POST /Admin/DeleteInstructorCode/{id}
        /// </summary>
        /// <param name="id">The ID of the instructor code to delete.</param>
        /// <returns>Redirects to ManageInstructorCodes.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteInstructorCode(int id)
        {
            var success = await _instructorCodeService.DeleteCodeAsync(id);
            if (success)
            {
                TempData["Success"] = "Instructor code deleted successfully.";
            }
            else
            {
                TempData["Error"] = "Instructor code not found.";
            }
            return RedirectToAction(nameof(ManageInstructorCodes));
        }

        /// <summary>
        /// Displays all public events pending approval, approved events, and denied events.
        /// GET /Admin/ApprovePublicEvents
        /// </summary>
        /// <returns>View listing pending, approved, and denied public events.</returns>
        public async Task<IActionResult> ApprovePublicEvents()
        {
            var publicEvents = await _eventService.GetPublicEventsAsync();
            var pendingEvents = publicEvents.Where(e => !e.IsApprovedPublic && !e.IsDeniedPublic).OrderByDescending(e => e.DateOfEvent).ThenBy(e => e.StartTime).ToList();
            var approvedEvents = publicEvents.Where(e => e.IsApprovedPublic).OrderByDescending(e => e.DateOfEvent).ThenBy(e => e.StartTime).ToList();
            var deniedEvents = publicEvents.Where(e => e.IsDeniedPublic).OrderByDescending(e => e.DateOfEvent).ThenBy(e => e.StartTime).ToList();
            
            var viewModel = new ApprovePublicEventsViewModel
            {
                PendingEvents = pendingEvents,
                ApprovedEvents = approvedEvents,
                DeniedEvents = deniedEvents
            };
            
            return View(viewModel);
        }

        /// <summary>
        /// Approves a public event for display on the homepage.
        /// POST /Admin/ApproveEvent/{eventId}
        /// </summary>
        /// <param name="eventId">The ID of the event to approve.</param>
        /// <returns>Redirects to ApprovePublicEvents.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveEvent(int eventId)
        {
            var eventItem = await _eventService.GetEventByIdAsync(eventId);
            if (eventItem == null)
            {
                return RedirectToAction(nameof(ApprovePublicEvents));
            }

            // Update the event to mark it as approved
            Event? existingEvent = await _eventService.GetEventByIdAsync(eventId);
            if (existingEvent == null)
            {
                return RedirectToAction(nameof(ApprovePublicEvents));
            }
            existingEvent.IsApprovedPublic = true;
            await _eventService.UpdateEventAsync(eventId, existingEvent);

            TempData["Success"] = $"Event '{eventItem.Title}' has been approved for display on the homepage.";
            return RedirectToAction(nameof(ApprovePublicEvents));
        }

        /// <summary>
        /// Revokes approval for a public event.
        /// POST /Admin/RevokeApproval/{eventId}
        /// </summary>
        /// <param name="eventId">The ID of the event to revoke approval for.</param>
        /// <returns>Redirects to ApprovePublicEvents.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RevokeApproval(int eventId)
        {
            var eventItem = await _eventService.GetEventByIdAsync(eventId);
            if (eventItem == null)
            {
                return RedirectToAction(nameof(ApprovePublicEvents));
            }

            // Update the event to revoke approval
            Event? existingEvent = await _eventService.GetEventByIdAsync(eventId);
            if (existingEvent == null)
            {
                return RedirectToAction(nameof(ApprovePublicEvents));
            }
            existingEvent.IsApprovedPublic = false;
            await _eventService.UpdateEventAsync(eventId, existingEvent);

            TempData["Success"] = $"Approval revoked for event '{eventItem.Title}'.";
            return RedirectToAction(nameof(ApprovePublicEvents));
        }

        /// <summary>
        /// Denies a public event from being displayed on the homepage.
        /// POST /Admin/DenyEvent/{eventId}
        /// </summary>
        /// <param name="eventId">The ID of the event to deny.</param>
        /// <returns>Redirects to ApprovePublicEvents.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DenyEvent(int eventId)
        {
            var eventItem = await _eventService.GetEventByIdAsync(eventId);
            if (eventItem == null)
            {
                return RedirectToAction(nameof(ApprovePublicEvents));
            }

            // Update the event to mark it as denied
            var existingEvent = await _eventService.GetEventByIdAsync(eventId);
            if (existingEvent == null)
            {
                return RedirectToAction(nameof(ApprovePublicEvents));
            }
            existingEvent.IsDeniedPublic = true;
            existingEvent.IsApprovedPublic = false;
            await _eventService.UpdateEventAsync(eventId, existingEvent);

            TempData["Success"] = $"Event '{eventItem.Title}' has been denied.";
            return RedirectToAction(nameof(ApprovePublicEvents));
        }

        /// <summary>
        /// Restores a denied event back to pending approval.
        /// POST /Admin/RestoreEvent/{eventId}
        /// </summary>
        /// <param name="eventId">The ID of the event to restore.</param>
        /// <returns>Redirects to ApprovePublicEvents.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreEvent(int eventId)
        {
            var eventItem = await _eventService.GetEventByIdAsync(eventId);
            if (eventItem == null)
            {
                return RedirectToAction(nameof(ApprovePublicEvents));
            }

            // Update the event to restore it to pending
            var existingEvent = await _eventService.GetEventByIdAsync(eventId);
            if (existingEvent == null)
            {
                return RedirectToAction(nameof(ApprovePublicEvents));
            }
            existingEvent.IsDeniedPublic = false;
            await _eventService.UpdateEventAsync(eventId, existingEvent);

            TempData["Success"] = $"Event '{eventItem.Title}' has been restored to pending.";
            return RedirectToAction(nameof(ApprovePublicEvents));
        }
    }
}