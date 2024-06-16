using Microsoft.AspNetCore.Mvc;
using RecipeGenerator.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore; // If you're not using lazy loading
using System;

namespace RecipeGenerator.Controllers
{
    public class DetailController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DetailController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Detail/{id}
        public async Task<IActionResult> Index(int id)
        {
            var recipe = await _context.Recipes
                                       .FirstOrDefaultAsync(r => r.Id == id); // Use async method with FirstOrDefaultAsync
            if (recipe == null)
            {
                return NotFound();
            }
            return View(recipe);
        }
    }
}
