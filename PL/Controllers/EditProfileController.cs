using Microsoft.AspNetCore.Mvc;
using DAL.Interfaces;
using DAL.Models;
using DAL.Interfaces;

namespace PL.Controllers
{
    public class EditProfileController : Controller
    {
        private readonly IUserService userService;

        public EditProfileController(IUserService userService)
        {
            this.userService = userService;
        }

        [HttpGet]
        public IActionResult Edit()
        {
            var userId = this.User?.Identity?.Name;

            if (string.IsNullOrEmpty(userId))
            {
                return this.Unauthorized();
            }

            var user = this.userService.GetUserById(userId);
            if (user == null)
            {
                return this.NotFound();
            }

            return this.View(user);
        }

        [HttpPost]
        public IActionResult Edit(User model)
        {
            if (this.ModelState.IsValid)
            {
                this.userService.UpdateUser(model);
                return this.RedirectToAction("Profile", new { id = model.Id });
            }
            
            return this.View(model);
        }
    }
}
