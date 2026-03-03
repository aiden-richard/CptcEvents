using CptcEvents.Authorization;
using CptcEvents.Authorization.EventAuthorizationService;
using CptcEvents.Authorization.GroupAuthorizationService;
using CptcEvents.Models;
using CptcEvents.Services;
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
        private readonly IEventAuthorizationService _eventAuthorization;
        private readonly IImageStorageService? _imageStorageService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImagesController"/> class.
        /// </summary>
        public ImagesController(
            IEventService eventService,
            IEventAuthorizationService eventAuthorization,
            IImageStorageService? imageStorageService = null)
        {
            _eventService = eventService;
            _eventAuthorization = eventAuthorization;
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

            // Check access using event authorization service
            ServicesAuthorizationResult viewCheck = await _eventAuthorization.CanViewEventAsync(eventItem, User);
            if (!viewCheck.Succeeded)
            {
                return viewCheck.ToActionResult(this);
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
