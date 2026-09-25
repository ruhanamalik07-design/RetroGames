using Microsoft.AspNetCore.Mvc;
using RetroGames.Models;

namespace RetroGames.Controllers
{
    public class CategoriesController : Controller
    {
        public IActionResult Index()
        {
            var categories = new List<Category>();
            for(int i = 1; i <= 10; i++)
            {
                categories.Add(new Category { Id = i, Name = "Category " + i });
            }
            return View(categories);
        }
        public IActionResult Browse(string categoryName) 
        {
            ViewBag.CategoryName = categoryName;
            return View();
        }
    }
}
