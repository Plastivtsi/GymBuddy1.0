using Microsoft.AspNetCore.Mvc;
using DAL.Models;
using BLL.Models;
using BLL.Service;
using DAL.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace YourProject.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IUserService _userService;
        private readonly UserManager<User> _userManager;

        public ProfileController(UserManager<User> userManager,IUserService userService)
        {
            _userManager = userManager;
            _userService = userService;
        }

        // GET: /Profile/
        public async Task<IActionResult> Index()
        {
            var user1 = await _userManager.GetUserAsync(User);
            var userId = user1.Id;
            var user = await _userService.GetUserById(userId.ToString());
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(user);
        }

        // GET: /Profile/Edit/
        public async Task<IActionResult> Edit()
        {
            var user1 = await _userManager.GetUserAsync(User);
            var userId = user1.Id;
            var user = await _userService.GetUserById(userId.ToString());

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(user);
        }

        // POST: /Profile/Edit/
        [HttpPost]
        public async Task<IActionResult> Edit(User model)
        {
            if (ModelState.IsValid)
            {
                _userService.UpdateUser(model);
                return RedirectToAction("Index");
            }

            return View(model);
        }
    }
}