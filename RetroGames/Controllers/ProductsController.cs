using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RetroGames.Data;
using RetroGames.Models;
[Authorize]
public class ProductsController : Controller
{
    private readonly ApplicationDbContext _context;

    private readonly IWebHostEnvironment _environment;
    private const int MaxPhotoBytes = 5 * 1024 * 1024;

    public ProductsController(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    // GET: PRODUCTS
    public async Task<IActionResult> Index()
    {
        return View(await _context.Products.Include(p => p.Category).OrderBy(p => p.Name).ToListAsync());
    }

    // GET: PRODUCTS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (product == null)
        {
            return NotFound();
        }

        return View(product);
    }

    // GET: PRODUCTS/Create
    public IActionResult Create()
    {
        PopulateCategories();
        return View();
    }

    // POST: PRODUCTS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Name,Price,CategoryId,Description")] Product product, IFormFile? photo)
    {
        await ValidateProductAsync(product.CategoryId, photo);
        if (!ModelState.IsValid)
        {
            PopulateCategories(product.CategoryId);
            return View(product);
        }

        var uploadedPhoto = await SavePhotoAsync(photo);
        product.Photo = uploadedPhoto;
        _context.Add(product);
        await SaveChangesAsync(uploadedPhoto);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        PopulateCategories(product.CategoryId);
        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id,
        [Bind("Id,Name,Price,CategoryId,Description")] Product input, IFormFile? photo)
    {
        if (id != input.Id)
        {
            return NotFound();
        }

        var product = await _context.Products.FindAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        await ValidateProductAsync(input.CategoryId, photo);
        if (!ModelState.IsValid)
        {
            input.Photo = product.Photo;
            PopulateCategories(input.CategoryId);
            return View(input);
        }

        var uploadedPhoto = await SavePhotoAsync(photo);
        product.Name = input.Name;
        product.Price = input.Price;
        product.CategoryId = input.CategoryId;
        product.Description = input.Description;
        if (uploadedPhoto is not null)
        {
            product.Photo = uploadedPhoto;
        }

        try
        {
            await SaveChangesAsync(uploadedPhoto);
        }
        catch (DbUpdateConcurrencyException) when (!_context.Products.Any(p => p.Id == id))
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: PRODUCTS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (product == null)
        {
            return NotFound();
        }

        return View(product);
    }

    // POST: PRODUCTS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            _context.Products.Remove(product);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private void PopulateCategories(int? selectedId = null)
    {
        ViewData["CategoryId"] = new SelectList(
            _context.Categories.OrderBy(category => category.Name), "Id", "Name", selectedId);
    }

    private async Task ValidateProductAsync(int categoryId, IFormFile? photo)
    {
        if (!await _context.Categories.AnyAsync(category => category.Id == categoryId))
        {
            ModelState.AddModelError("CategoryId", "Select an existing category.");
        }

        if (photo is null)
        {
            return;
        }

        if (photo.Length < 8 || photo.Length > MaxPhotoBytes)
        {
            ModelState.AddModelError("Photo", "Choose a JPG or PNG image up to 5 MB.");
            return;
        }

        var signature = new byte[8];
        await using var stream = photo.OpenReadStream();
        await stream.ReadExactlyAsync(signature);
        var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();
        var isJpeg = (extension is ".jpg" or ".jpeg")
            && signature.AsSpan(0, 3).SequenceEqual(new byte[] { 255, 216, 255 });
        var isPng = extension == ".png"
            && signature.AsSpan().SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
        if (!isJpeg && !isPng)
        {
            ModelState.AddModelError("Photo", "Choose a JPG or PNG image.");
        }
    }

    private string UploadDirectory => Path.Combine(_environment.WebRootPath, "img", "product-uploads");

    private async Task<string?> SavePhotoAsync(IFormFile? photo)
    {
        if (photo is null)
        {
            return null;
        }

        Directory.CreateDirectory(UploadDirectory);
        var name = Guid.NewGuid().ToString("N") + Path.GetExtension(photo.FileName).ToLowerInvariant();
        var path = Path.Combine(UploadDirectory, name);
        try
        {
            await using var stream = new FileStream(path, FileMode.CreateNew);
            await photo.CopyToAsync(stream);
            return name;
        }
        catch (IOException)
        {
            System.IO.File.Delete(path);
            throw;
        }
    }

    private async Task SaveChangesAsync(string? uploadedPhoto)
    {
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            if (uploadedPhoto is not null)
            {
                System.IO.File.Delete(Path.Combine(UploadDirectory, uploadedPhoto));
            }

            throw;
        }
    }
}