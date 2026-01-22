using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestableWebApp.Data;
using TestableWebApp.Models;
using TestableWebApp.Models.ViewModels;
using TestableWebApp.Services;

namespace TestableWebApp.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class ProductsApiController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsApiController> _logger;

    public ProductsApiController(IProductService productService, ILogger<ProductsApiController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Get all active products
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
    {
        var products = await _productService.GetActiveProductsAsync();
        return Ok(products);
    }

    /// <summary>
    /// Get all products including inactive (Admin only)
    /// </summary>
    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<Product>>> GetAllProducts()
    {
        var products = await _productService.GetAllProductsAsync();
        return Ok(products);
    }

    /// <summary>
    /// Get a specific product by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
            return NotFound(new { message = $"Product with ID {id} not found" });

        return Ok(product);
    }

    /// <summary>
    /// Search products by term
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<Product>>> SearchProducts([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new { message = "Search term is required" });

        var products = await _productService.SearchProductsAsync(q);
        return Ok(products);
    }

    /// <summary>
    /// Get products by category
    /// </summary>
    [HttpGet("category/{category}")]
    public async Task<ActionResult<IEnumerable<Product>>> GetByCategory(string category)
    {
        var products = await _productService.GetProductsByCategoryAsync(category);
        return Ok(products);
    }

    /// <summary>
    /// Create a new product (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Product>> CreateProduct([FromBody] ProductViewModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

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

        var created = await _productService.CreateProductAsync(product);
        return CreatedAtAction(nameof(GetProduct), new { id = created.Id }, created);
    }

    /// <summary>
    /// Update an existing product (Admin only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Product>> UpdateProduct(int id, [FromBody] ProductViewModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

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

        var updated = await _productService.UpdateProductAsync(id, product);
        if (updated == null)
            return NotFound(new { message = $"Product with ID {id} not found" });

        return Ok(updated);
    }

    /// <summary>
    /// Delete a product (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var result = await _productService.DeleteProductAsync(id);
        if (!result)
            return NotFound(new { message = $"Product with ID {id} not found" });

        return NoContent();
    }

    /// <summary>
    /// Update product stock
    /// </summary>
    [HttpPatch("{id}/stock")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStock(int id, [FromBody] StockUpdateModel model)
    {
        var result = await _productService.UpdateStockAsync(id, model.Quantity);
        if (!result)
            return BadRequest(new { message = "Unable to update stock. Product not found or insufficient stock." });

        return Ok(new { message = "Stock updated successfully" });
    }

    /// <summary>
    /// Get available categories
    /// </summary>
    [HttpGet("categories")]
    public ActionResult<string[]> GetCategories()
    {
        return Ok(ProductCategory.Categories);
    }
}

public class StockUpdateModel
{
    public int Quantity { get; set; }
}

[Route("api/[controller]")]
[ApiController]
public class OrdersApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<OrdersApiController> _logger;

    public OrdersApiController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<OrdersApiController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Get orders for current user
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<OrderViewModel>>> GetOrders()
    {
        var userId = _userManager.GetUserId(User);
        
        var orders = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .Select(o => new OrderViewModel
            {
                Id = o.Id,
                OrderDate = o.OrderDate,
                Status = o.Status.ToString(),
                TotalAmount = o.TotalAmount,
                ShippingAddress = o.ShippingAddress,
                Items = o.OrderItems.Select(oi => new OrderItemViewModel
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product!.Name,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.Quantity * oi.UnitPrice
                }).ToList()
            })
            .ToListAsync();

        return Ok(orders);
    }

    /// <summary>
    /// Get a specific order
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<OrderViewModel>> GetOrder(int id)
    {
        var userId = _userManager.GetUserId(User);
        
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .Where(o => o.Id == id && o.UserId == userId)
            .Select(o => new OrderViewModel
            {
                Id = o.Id,
                OrderDate = o.OrderDate,
                Status = o.Status.ToString(),
                TotalAmount = o.TotalAmount,
                ShippingAddress = o.ShippingAddress,
                Items = o.OrderItems.Select(oi => new OrderItemViewModel
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product!.Name,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.Quantity * oi.UnitPrice
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (order == null)
            return NotFound(new { message = $"Order with ID {id} not found" });

        return Ok(order);
    }

    /// <summary>
    /// Create a new order
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<OrderViewModel>> CreateOrder([FromBody] CreateOrderViewModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        // Validate products and calculate total
        decimal totalAmount = 0;
        var orderItems = new List<OrderItem>();

        foreach (var item in model.Items)
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product == null)
                return BadRequest(new { message = $"Product with ID {item.ProductId} not found" });

            if (!product.IsActive)
                return BadRequest(new { message = $"Product '{product.Name}' is not available" });

            if (product.StockQuantity < item.Quantity)
                return BadRequest(new { message = $"Insufficient stock for '{product.Name}'" });

            orderItems.Add(new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = product.Price
            });

            totalAmount += product.Price * item.Quantity;

            // Update stock
            product.StockQuantity -= item.Quantity;
        }

        var order = new Order
        {
            UserId = userId,
            TotalAmount = totalAmount,
            ShippingAddress = model.ShippingAddress,
            Notes = model.Notes,
            Status = OrderStatus.Pending,
            OrderItems = orderItems
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Order {OrderId} created by user {UserId}", order.Id, userId);

        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, new OrderViewModel
        {
            Id = order.Id,
            OrderDate = order.OrderDate,
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            ShippingAddress = order.ShippingAddress
        });
    }

    /// <summary>
    /// Cancel an order
    /// </summary>
    [HttpPost("{id}/cancel")]
    [Authorize]
    public async Task<IActionResult> CancelOrder(int id)
    {
        var userId = _userManager.GetUserId(User);
        
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

        if (order == null)
            return NotFound(new { message = $"Order with ID {id} not found" });

        if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Processing)
            return BadRequest(new { message = "Only pending or processing orders can be cancelled" });

        // Restore stock
        foreach (var item in order.OrderItems)
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product != null)
                product.StockQuantity += item.Quantity;
        }

        order.Status = OrderStatus.Cancelled;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Order {OrderId} cancelled by user {UserId}", id, userId);

        return Ok(new { message = "Order cancelled successfully" });
    }
}

[Route("api/[controller]")]
[ApiController]
public class AuthApiController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<AuthApiController> _logger;

    public AuthApiController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<AuthApiController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    /// <summary>
    /// Login via API
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                user.LastLoginAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
            }

            _logger.LogInformation("User {Email} logged in via API.", model.Email);
            return Ok(new { message = "Login successful", email = model.Email });
        }

        if (result.IsLockedOut)
        {
            return BadRequest(new { message = "Account locked out" });
        }

        return BadRequest(new { message = "Invalid email or password" });
    }

    /// <summary>
    /// Register via API
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "User");
            _logger.LogInformation("User {Email} registered via API.", model.Email);
            
            return Ok(new { message = "Registration successful", email = model.Email });
        }

        return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
    }

    /// <summary>
    /// Logout via API
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok(new { message = "Logged out successfully" });
    }

    /// <summary>
    /// Get current user info
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new
        {
            id = user.Id,
            email = user.Email,
            firstName = user.FirstName,
            lastName = user.LastName,
            roles = roles,
            createdAt = user.CreatedAt,
            lastLoginAt = user.LastLoginAt
        });
    }
}

/// <summary>
/// Health check endpoint
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        });
    }
}
