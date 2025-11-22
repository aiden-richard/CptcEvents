using Microsoft.AspNetCore.Mvc;
using CptcEvents.Models;
using CptcEvents.Services;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace CptcEvents.Controllers
{
    public class GroupsController : Controller
    {
        private readonly IGroupService _groupService;
        private readonly UserManager<ApplicationUser> _userManager;

        public GroupsController(IGroupService groupService, UserManager<ApplicationUser> userManager)
        {
            _groupService = groupService;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // GET: Groups/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Groups/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GroupViewModel model)
        {
            string? userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Challenge();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Group newGroup = new Group
            {
                Name = model.Name,
                Description = model.Description,
                PrivacyLevel = model.PrivacyLevel,
                OwnerId = userId
            };

            // Persist the new group to the database
            var created = await _groupService.CreateGroupAsync(newGroup);

            return RedirectToAction(nameof(Index));
        }

        // POST: Groups/Join/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Join(int id)
        {
            // TODO: Add membership record for the current user

            return RedirectToAction(nameof(Index));
        }

        // POST: Groups/Leave/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Leave(int id)
        {
            // TODO: Remove membership record for the current user

            return RedirectToAction(nameof(Index));
        }

        // GET: Groups/Delete/5
        [HttpGet]
        public IActionResult Delete(int id)
        {
            // TODO: Load the group and show confirmation view
            return View();
        }

        // POST: Groups/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            // TODO: Delete the group (or mark as deleted)

            return RedirectToAction(nameof(Index));
        }
    }
}
