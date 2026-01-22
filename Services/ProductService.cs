using Microsoft.EntityFrameworkCore;
using TestableWebApp.Data;
using TestableWebApp.Models;

namespace TestableWebApp.Services;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ProductService> _logger;

    public ProductService(ApplicationDbContext context, ILogger<ProductService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        return await _context.Products
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetActiveProductsAsync()
    {
        return await _context.Products
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        return await _context.Products.FindAsync(id);
    }

    public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category)
    {
        return await _context.Products
            .Where(p => p.Category.ToLower() == category.ToLower() && p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetActiveProductsAsync();

        searchTerm = searchTerm.ToLower();
        
        return await _context.Products
            .Where(p => p.IsActive && 
                (p.Name.ToLower().Contains(searchTerm) || 
                 (p.Description != null && p.Description.ToLower().Contains(searchTerm)) ||
                 p.Category.ToLower().Contains(searchTerm)))
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<Product> CreateProductAsync(Product product)
    {
        product.CreatedAt = DateTime.UtcNow;
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Product created: {ProductId} - {ProductName}", product.Id, product.Name);
        return product;
    }

    public async Task<Product?> UpdateProductAsync(int id, Product updatedProduct)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return null;

        product.Name = updatedProduct.Name;
        product.Description = updatedProduct.Description;
        product.Price = updatedProduct.Price;
        product.Category = updatedProduct.Category;
        product.StockQuantity = updatedProduct.StockQuantity;
        product.IsActive = updatedProduct.IsActive;
        product.ImageUrl = updatedProduct.ImageUrl;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Product updated: {ProductId} - {ProductName}", product.Id, product.Name);
        return product;
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return false;

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Product deleted: {ProductId}", id);
        return true;
    }

    public async Task<bool> UpdateStockAsync(int id, int quantity)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return false;

        if (product.StockQuantity + quantity < 0)
            return false;

        product.StockQuantity += quantity;
        product.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Stock updated for product {ProductId}: {NewQuantity}", id, product.StockQuantity);
        return true;
    }
}
