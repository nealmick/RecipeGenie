


using Microsoft.AspNetCore.Mvc;

namespace RecipeGenerator.Controllers
{
    public class HelloController : Controller
    {
        public IActionResult Index()
        {
			var data = new Dictionary<string, string>
            {
                { "message", "hello" },
                { "status", "success" }
            };

            return Json(data);

		}
    }
}
