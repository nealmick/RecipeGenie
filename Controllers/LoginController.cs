using Microsoft.AspNetCore.Mvc;

namespace RecipeGenerator.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }
    }
}
