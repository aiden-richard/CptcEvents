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
        public async Task<IActionResult> Create(Group group)
        {
            if (!ModelState.IsValid)
            {
                return View(group);
            }

            // Persist the new group to the database
            var created = await _groupService.AddGroupAsync(group);

            // If user is authenticated, automatically add them as a member
            if (User?.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    await _groupService.AddMemberToGroupAsync(created.Id, user.Id);
                }
            }

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
