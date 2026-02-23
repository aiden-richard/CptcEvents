using CptcEvents.Authorization;
using CptcEvents.Models;
using CptcEvents.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CptcEvents.Controllers
{
    /// <summary>
    /// Controller for accessing images with access control enforcement.
    /// Serves banner images for events, enforcing authorization rules based on event visibility and group membership.
    /// </summary>
    public class ImagesController : Controller
    {
        private readonly IEventService _eventService;
        private readonly IGroupAuthorizationService _groupAuthorization;
        private readonly IGroupService _groupService;
        private readonly IImageStorageService? _imageStorageService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImagesController"/> class.
        /// </summary>
        public ImagesController(
            IEventService eventService,
            IGroupAuthorizationService groupAuthorization,
            IGroupService groupService,
            UserManager<ApplicationUser> userManager,
            IImageStorageService? imageStorageService = null)
        {
            _eventService = eventService;
            _groupAuthorization = groupAuthorization;
            _groupService = groupService;
            _imageStorageService = imageStorageService;
        }

        /// <summary>
        /// Returns the banner image for an event, enforcing authorization based on event visibility and group membership.
        /// Public approved events are accessible to anyone (including anonymous users).
        /// Private/unapproved events are accessible only to authenticated group members and admins.
        /// GET /Images/Event/{eventId}
        /// </summary>
        /// <param name="eventId">The ID of the event whose banner image to retrieve.</param>
        /// <returns>File result with the image stream, or 404/403/Challenge as appropriate.</returns>
        [HttpGet("Images/Event/{eventId:int}")]
        public async Task<IActionResult> EventBanner(int eventId)
        {
            if (_imageStorageService == null)
            {
                return NotFound();
            }

            // Look up the event
            Event? eventItem = await _eventService.GetEventByIdAsync(eventId);
            if (eventItem == null || string.IsNullOrEmpty(eventItem.BannerImageUrl))
            {
                return NotFound();
            }

            // Check visibility: public approved events are accessible to anyone
            bool isPublicApproved = eventItem.IsPublic && eventItem.IsApprovedPublic;

            if (!isPublicApproved)
            {
                // For non-public events, user must be authenticated and be a member of the group (or an admin)
                string? userId = await _groupAuthorization.GetUserIdAsync(User);
                if (userId == null)
                {
                    return Challenge();
                }

                bool isAdmin = User.IsInRole("Admin");
                if (!isAdmin)
                {
                    bool isMember = await _groupService.IsUserMemberAsync(eventItem.GroupId, userId);
                    if (!isMember)
                    {
                        return Forbid();
                    }
                }
            }

            // Download the image from storage
            var result = await _imageStorageService.GetImageStreamAsync(eventItem.BannerImageUrl);
            if (result == null)
            {
                return NotFound();
            }

            return File(result.Value.Content, result.Value.ContentType);
        }
    }
}
