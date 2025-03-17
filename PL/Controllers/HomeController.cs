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
            UserInfo = GetUserInfo();
        }

        public IActionResult Index()
        {
            ViewBag.UserInfo = GetUserInfo();
            return View();
            
        }
        private string GetUserInfo()
        {

            User proba = new User();
            UserService service = new UserService();
            service.UpdateUser(1, "Andriy", "plastivtsi", "222", 60, 174);
            proba = service.GetUserById(1);
            if (proba == null){
                return "повернуло null ";
            }
            else
            {
                return proba.UserName + " " + proba.Email +" "+ proba.Role+" " + proba.Height +" "+ proba.Weight;

            }
           
            //User proba = _userService.GetUserById(1);
            //return $"{proba.UserName} {proba.Email} {proba.Role} {proba.Height} {proba.Weight}";
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
