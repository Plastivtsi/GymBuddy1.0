using Microsoft.AspNetCore.Mvc;
using System;
using BLL.Models;

namespace PL.Controllers
{
    public class AccountController : Controller
    {
        private Autorization auth = new Autorization();

       
        public ActionResult Login()
        {
            return View();
        }

        
        [HttpPost]
        public ActionResult Login(string loginUsername, string loginPassword)
        {
            try
            {
                if (auth.Login(loginUsername, loginPassword))
                {
                    HttpContext.Session.SetInt32("UserID", Autorization.CurrentUserId);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewBag.Error = "Невірні дані для входу";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }

            return View();
        }

        // Відображення сторінки реєстрації
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(string nickname, string email, string password)
        {
            try
            {
                if (auth.Register(nickname, email, password))
                {
                    return RedirectToAction("Login");
                }
                else
                {
                    ViewBag.Error = "Помилка при реєстрації.";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }

            return View();
        }

        // Вихід з системи
        public ActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
