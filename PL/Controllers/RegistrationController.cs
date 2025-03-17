using Microsoft.AspNetCore.Mvc;

namespace PL.Controllers
{
    public class RegistrationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
