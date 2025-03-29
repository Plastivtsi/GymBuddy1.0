using Microsoft.AspNetCore.Mvc;
using DAL.Models;
using BLL.Models;
using BLL.Service;
using DAL.Interfaces;

namespace YourProject.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IUserService _userService;

        public ProfileController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: /Profile/
        public IActionResult Index()
        {
            var userId = Autorization.CurrentUserId;
            var user = _userService.GetUserById(userId.ToString()); 

            if (user == null)
            {
                return RedirectToAction("Login", "Account"); 
            }

            return View(user);
        }

        // GET: /Profile/Edit/
        public IActionResult Edit()
        {
            var userId = Autorization.CurrentUserId;
            var user = _userService.GetUserById(userId.ToString());

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(user);
        }

        // POST: /Profile/Edit/
        [HttpPost]
        public IActionResult Edit(User model)
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