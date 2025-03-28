using Microsoft.AspNetCore.Mvc;
using PL.Models;
using System.Diagnostics;
using BLL.Service; // ������ ��� IUserService
using DAL.Models;
using DAL.Interfaces; // ������ ��� User

namespace PL.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        public readonly IUserService _userService;
        public HomeController(ILogger<HomeController> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService; // ���������� ����� DI
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Profile()
        {
            // �������� ID ����������� � ���
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                // ���� ���������� �� �������������, ��������������� �� ������� �����
                return RedirectToAction("Login", "Account");
            }

            // �������� ����������� ����� �����
            var user = _userService.GetUserById(userId);

            if (user == null)
            {
                // ���� ����������� �� ��������, �������� ������� ��� ���������������
                return RedirectToAction("Error");
            }

            // �������� ������ � �������������
            return View(user);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}