using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CptcEvents.Services;
using Microsoft.AspNetCore.Identity;
using CptcEvents.Models;

namespace CptcEvents.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IInstructorCodeService _instructorCodeService;
        private readonly UserManager<Models.ApplicationUser> _userManager;

        public AdminController(IInstructorCodeService instructorCodeService, UserManager<Models.ApplicationUser> userManager)
        {
            _instructorCodeService = instructorCodeService;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Dashboard for managing instructor codes.
        /// </summary>
        public async Task<IActionResult> ManageInstructorCodes()
        {
            var codes = await _instructorCodeService.GetAllCodesAsync();
            return View(codes);
        }

        /// <summary>
        /// Show the form to create a new instructor code.
        /// </summary>
        public IActionResult CreateInstructorCode()
        {
            return View(new CreateInstructorCodeViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateInstructorCode(CreateInstructorCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Generate a random code
            var code = GenerateRandomCode();

            DateTime? expiresAt = model.Expires ? model.ExpiresAt?.ToUniversalTime() : null;

            var currentUser = await _userManager.GetUserAsync(User);
            await _instructorCodeService.CreateCodeAsync(code, model.Email, expiresAt, currentUser?.UserName ?? currentUser?.Email);

            TempData["Success"] = $"Instructor code '{code}' created successfully.";
            return RedirectToAction(nameof(ManageInstructorCodes));
        }

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

        private string GenerateRandomCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public IActionResult Users()
        {
            return View();
        }

        public IActionResult Groups()
        {
            return View();
        }

        public IActionResult Events()
        {
            return View();
        }

        public IActionResult Invites()
        {
            return View();
        }
    }
}