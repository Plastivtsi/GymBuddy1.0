using Microsoft.AspNetCore.Mvc;
using PL.Models;
using System.Diagnostics;
using BLL;
using BLL.Models;

namespace PL.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        //private readonly UserService _userService;
        public string UserInfo { get; set; }

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            //_userService = userService;
            
        }

        public IActionResult Index()
        {
            
            return View();
            
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
