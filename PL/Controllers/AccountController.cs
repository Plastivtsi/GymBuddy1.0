using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
//using BLL.Models.Interfaces;
using DAL.Models;
using BLL.Models;
using BLL.Models.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace PL.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<AccountController> _logger;

        
        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string nickname, string email, string password)
        {
            var user = new User { UserName = nickname, Email = email };

            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                _logger.LogInformation("User {Nickname} created a new account.", nickname);
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View();
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string loginUsername, string loginPassword)
        {
            var result = await _signInManager.PasswordSignInAsync(loginUsername, loginPassword, isPersistent: false, lockoutOnFailure: false);
            var user = await _userManager.FindByNameAsync(loginUsername);
            if (user != null)
            {
                if (await _userManager.IsInRoleAsync(user, "Blocked"))
                {
                    ViewBag.Error = $"Ваш акаунт було заблоковано адміністратором. Причина: {user.BlockedReason}";
                    return View();
                }
            }
            if (result.Succeeded)
            {
                _logger.LogInformation("User {Username} logged in.", loginUsername);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View();
            }
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
        [HttpPost]
        public async Task<IActionResult> AssignRole(int userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user != null && !await _userManager.IsInRoleAsync(user, role))
            {
                await _userManager.AddToRoleAsync(user, role);
                return Ok($"Role '{role}' assigned to user {user.UserName}");
            }
            return BadRequest("User not found or already in role");
        }

    }
}
