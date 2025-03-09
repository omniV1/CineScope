using Microsoft.AspNetCore.Mvc;

namespace CineScope.Server.Controllers
{
    public class BannedWordController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
