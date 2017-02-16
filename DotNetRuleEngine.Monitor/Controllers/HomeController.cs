using Microsoft.AspNetCore.Mvc;

namespace DotNetRuleEngine.Monitor.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
