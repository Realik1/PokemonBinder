using Microsoft.AspNetCore.Mvc;

namespace PokemonBinder.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
