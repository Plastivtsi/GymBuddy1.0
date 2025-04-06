using Microsoft.AspNetCore.Mvc;
using PL.Models;
using System.Diagnostics;
using BLL.Service; // ������ ��� IUserService
using DAL.Models;
using DAL.Interfaces; // ������ ��� User
using Microsoft.AspNetCore.Identity;


namespace PL.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<User> _userManager;

        public HomeController(ILogger<HomeController> logger, UserManager<User> userManager)
        {
            _userManager = userManager;
            _logger = logger;

            //_userService = userService; // ����������� ����� DI
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Profile()
        {
            // �������� ID ����������� � ���
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId==0)
            {
                // ���� ���������� �� �������������, ��������������� �� ������� �����
                return RedirectToAction("Login", "Account");
            }

            // �������� ����������� ����� �����
            var user =  _userManager.GetUserAsync(User);
            //userId = user1.Id;
            //var user = _userService.GetUserById(userId.ToString());

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