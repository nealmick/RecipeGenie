using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeGenerator.Data;
using RecipeGenerator.Models;
using System.Linq;
using System.Threading.Tasks;

namespace RecipeGenerator.Controllers
{
    public class ListController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ListController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        [Authorize]  // Ensure this controller is only accessible to logged-in users
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);  // Get the current user's ID
            var recipes = await _context.Recipes
                                        .Where(r => r.UserId == userId)
                                        .OrderByDescending(r => r.Timestamp)
                                        .Take(100)
                                        .ToListAsync();
            return View(recipes);
        }
        public async Task<IActionResult> All()
        {
            var recipes = await _context.Recipes
                                        .OrderByDescending(r => r.Timestamp)
                                        .Take(100)
                                        .ToListAsync();
            return View("All", recipes); // This will render the All.cshtml view with the recipes data
        }
    }
}
