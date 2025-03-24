using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
//using BLL.Models.Interfaces;
using DAL.Models;
using BLL.Models;
using BLL.Models.Interfaces;

namespace PL.Controllers
{
    public class AccountController : Controller
    {
        private readonly ICreateUser _createUser;

       // private readonly Autorization _authorization;
        private readonly ILogger<AccountController> _logger;

        public AccountController(ICreateUser createUser, ILogger<AccountController> logger)
        {
            _createUser = createUser ?? throw new ArgumentNullException(nameof(createUser));
            _logger = logger;
        }

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(User user)
        {
            try
            {
                await _createUser.CreateNewUser(user.Name, user.Email, user.Password);
                _logger.LogInformation("Користувач {Nickname} успішно зареєстрований.", user.Name);
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Помилка реєстрації користувача {Nickname}.", user.Name);
                ViewBag.Error = ex.Message;
                return View("Register");
            }          
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string loginUsername, string loginPassword)
        {
            try
            {
                if (((Autorization)_createUser).Login(loginUsername, loginPassword))
                {
                    HttpContext.Session.SetInt32("UserID", Autorization.CurrentUserId);
                    _logger.LogInformation("Користувач {Username} успішно увійшов у систему.", loginUsername);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    _logger.LogWarning("Невдала спроба входу для {Username}.", loginUsername);
                    ViewBag.Error = "Невірні дані для входу";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка входу користувача {Username}.", loginUsername);
                ViewBag.Error = ex.Message;
            }

            return View();
        }

        public ActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
