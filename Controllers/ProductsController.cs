using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestableWebApp.Models;
using TestableWebApp.Models.ViewModels;
using TestableWebApp.Services;

namespace TestableWebApp.Controllers;

public class ProductsController : Controller
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    // GET: Products
    public async Task<IActionResult> Index(string? category, string? search)
    {
        IEnumerable<Product> products;

        if (!string.IsNullOrEmpty(search))
        {
            products = await _productService.SearchProductsAsync(search);
            ViewData["SearchTerm"] = search;
        }
        else if (!string.IsNullOrEmpty(category))
        {
            products = await _productService.GetProductsByCategoryAsync(category);
            ViewData["Category"] = category;
        }
        else
        {
            products = await _productService.GetActiveProductsAsync();
        }

        ViewData["Categories"] = ProductCategory.Categories;
        return View(products); // Passes data to View
    }

    // GET: Products/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
            return NotFound();

        return View(product);
    }

    // GET: Products/Create
    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        ViewData["Categories"] = ProductCategory.Categories;
        return View();
    }

    // POST: Products/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(ProductViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Categories"] = ProductCategory.Categories;
            return View(model);
        }

        var product = new Product
        {
            Name = model.Name,
            Description = model.Description,
            Price = model.Price,
            Category = model.Category,
            StockQuantity = model.StockQuantity,
            IsActive = model.IsActive,
            ImageUrl = model.ImageUrl
        };

        await _productService.CreateProductAsync(product);
        
        TempData["SuccessMessage"] = "Product created successfully!";
        return RedirectToAction(nameof(Index));
    }

    // GET: Products/Edit/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
            return NotFound();

        var model = new ProductViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Category = product.Category,
            StockQuantity = product.StockQuantity,
            IsActive = product.IsActive,
            ImageUrl = product.ImageUrl
        };

        ViewData["Categories"] = ProductCategory.Categories;
        return View(model);
    }

    // POST: Products/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, ProductViewModel model)
    {
        if (id != model.Id)
            return NotFound();

        if (!ModelState.IsValid)
        {
            ViewData["Categories"] = ProductCategory.Categories;
            return View(model);
        }

        var product = new Product
        {
            Name = model.Name,
            Description = model.Description,
            Price = model.Price,
            Category = model.Category,
            StockQuantity = model.StockQuantity,
            IsActive = model.IsActive,
            ImageUrl = model.ImageUrl
        };

        var result = await _productService.UpdateProductAsync(id, product);
        if (result == null)
            return NotFound();

        TempData["SuccessMessage"] = "Product updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    // GET: Products/Delete/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
            return NotFound();

        return View(product);
    }

    // POST: Products/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _productService.DeleteProductAsync(id);
        if (!result)
            return NotFound();

        TempData["SuccessMessage"] = "Product deleted successfully!";
        return RedirectToAction(nameof(Index));
    }
}
