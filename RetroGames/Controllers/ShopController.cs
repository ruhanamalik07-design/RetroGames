using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetroGames.Data;
using RetroGames.Models;

namespace RetroGames.Controllers;

public class ShopController(ApplicationDbContext context) : Controller
{
    private const string CartSessionKey = "CartKey";

    public async Task<IActionResult> Index()
    {
        return View(await context.Categories.AsNoTracking().OrderBy(category => category.Name).ToListAsync());
    }

    public async Task<IActionResult> Browse(int id)
    {
        var category = await context.Categories.AsNoTracking()
            .Include(item => item.Products)
            .SingleOrDefaultAsync(item => item.Id == id);
        if (category is null)
        {
            return NotFound();
        }

        await GetCartKeyAsync();
        return View(category);
    }

    public async Task<IActionResult> Cart()
    {
        var key = await GetCartKeyAsync();
        var lines = await context.Carts
            .Where(row => row.SessionKey == key)
            .GroupBy(row => new { row.ProductId, row.Product.Name, row.Product.Price })
            .OrderBy(group => group.Key.Name)
            .Select(group => new CartLine(group.Key.ProductId, group.Key.Name, group.Key.Price, group.Count()))
            .ToListAsync();
        return View(lines);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToCart(int productId)
    {
        if (!await context.Products.AnyAsync(product => product.Id == productId))
        {
            return NotFound();
        }

        context.Carts.Add(new Cart { ProductId = productId, SessionKey = await GetCartKeyAsync() });
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Cart));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveFromCart(int productId)
    {
        var key = await GetCartKeyAsync();
        await context.Carts.Where(row => row.SessionKey == key && row.ProductId == productId)
            .ExecuteDeleteAsync();
        return RedirectToAction(nameof(Cart));
    }

    private async Task<Guid> GetCartKeyAsync()
    {
        await HttpContext.Session.LoadAsync();
        if (Guid.TryParse(HttpContext.Session.GetString(CartSessionKey), out var key))
        {
            return key;
        }

        key = Guid.NewGuid();
        HttpContext.Session.SetString(CartSessionKey, key.ToString());
        return key;
    }
}