using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace TestableWebApp.Controllers;

/// <summary>
/// Controller demonstrating challenging automation scenarios for Selenium/Playwright testing
/// </summary>
public class AutomationChallengesController : Controller
{
    private readonly ILogger<AutomationChallengesController> _logger;
    private readonly IWebHostEnvironment _environment;

    public AutomationChallengesController(
        ILogger<AutomationChallengesController> logger,
        IWebHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Index page showing all available challenge scenarios
    /// </summary>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Challenge 1: Dynamic Element Locators
    /// Demonstrates elements with IDs/classes that change on each page load
    /// </summary>
    public IActionResult DynamicElements()
    {
        // Generate random suffixes for element IDs to simulate dynamic frameworks like React
        ViewBag.RandomId1 = Guid.NewGuid().ToString("N").Substring(0, 8);
        ViewBag.RandomId2 = Guid.NewGuid().ToString("N").Substring(0, 8);
        ViewBag.RandomId3 = Guid.NewGuid().ToString("N").Substring(0, 8);
        ViewBag.Timestamp = DateTime.Now.Ticks;
        
        return View();
    }

    /// <summary>
    /// Challenge 2: Shadow DOM & iFrames
    /// Demonstrates nested Shadow DOM elements and multiple iFrames
    /// </summary>
    public IActionResult ShadowDomAndFrames()
    {
        return View();
    }

    /// <summary>
    /// Embedded content for iFrame demonstration
    /// </summary>
    public IActionResult FrameContent(int frameNumber = 1)
    {
        ViewBag.FrameNumber = frameNumber;
        return View();
    }

    /// <summary>
    /// Nested frame content for multi-level iFrame testing
    /// </summary>
    public IActionResult NestedFrameContent()
    {
        return View();
    }

    /// <summary>
    /// Challenge 3: Asynchronous Loading (AJAX)
    /// Demonstrates delayed content loading and stale element scenarios
    /// </summary>
    public IActionResult AsyncLoading()
    {
        return View();
    }

    /// <summary>
    /// API endpoint for AJAX content loading with configurable delay
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAsyncContent(int delayMs = 2000, string contentType = "products")
    {
        // Simulate network delay
        await Task.Delay(delayMs);

        object content = contentType switch
        {
            "products" => new
            {
                success = true,
                data = new[]
                {
                    new { id = 1, name = "Laptop Pro", price = 1299.99, stock = 15 },
                    new { id = 2, name = "Wireless Mouse", price = 29.99, stock = 150 },
                    new { id = 3, name = "Mechanical Keyboard", price = 89.99, stock = 45 }
                },
                timestamp = DateTime.Now
            },
            "users" => new
            {
                success = true,
                data = new[]
                {
                    new { id = 1, name = "John Doe", email = "john@example.com", role = "Admin" },
                    new { id = 2, name = "Jane Smith", email = "jane@example.com", role = "User" }
                },
                timestamp = DateTime.Now
            },
            "notifications" => new
            {
                success = true,
                data = new[]
                {
                    new { id = 1, message = "New order received", priority = "high", timestamp = DateTime.Now.AddMinutes(-5) },
                    new { id = 2, message = "System update completed", priority = "low", timestamp = DateTime.Now.AddMinutes(-15) }
                },
                timestamp = DateTime.Now
            },
            _ => new
            {
                success = false,
                error = "Unknown content type",
                timestamp = DateTime.Now
            }
        };

        return Json(content);
    }

    /// <summary>
    /// Challenge 4: Multi-Window and Tab Management
    /// Demonstrates scenarios that open new tabs/windows
    /// </summary>
    public IActionResult MultiWindow()
    {
        return View();
    }

    /// <summary>
    /// Content for new tab/window demonstration
    /// </summary>
    public IActionResult NewTabContent(string source = "unknown")
    {
        ViewBag.Source = source;
        ViewBag.OpenedAt = DateTime.Now;
        return View();
    }

    /// <summary>
    /// Popup window content
    /// </summary>
    public IActionResult PopupWindow()
    {
        return View();
    }

    /// <summary>
    /// Challenge 5: SVG Elements and Canvas
    /// Demonstrates SVG charts and Canvas-based visualizations
    /// </summary>
    public IActionResult SvgAndCanvas()
    {
        return View();
    }

    /// <summary>
    /// API endpoint to get chart data
    /// </summary>
    [HttpGet]
    public IActionResult GetChartData()
    {
        var data = new
        {
            labels = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun" },
            sales = new[] { 12500, 15300, 18200, 14800, 21000, 19500 },
            expenses = new[] { 8200, 9100, 10500, 9800, 11200, 10800 },
            profit = new[] { 4300, 6200, 7700, 5000, 9800, 8700 }
        };

        return Json(data);
    }

    /// <summary>
    /// Challenge 6: File Uploads and Downloads
    /// Demonstrates file upload/download operations
    /// </summary>
    public IActionResult FileOperations()
    {
        return View();
    }

    /// <summary>
    /// Handle file upload
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, error = "No file selected" });
        }

        try
        {
            // Create uploads directory if it doesn't exist
            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsPath);

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsPath, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Json(new
            {
                success = true,
                fileName = file.FileName,
                fileSize = file.Length,
                savedAs = fileName,
                uploadedAt = DateTime.Now
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Generate and download a sample file
    /// </summary>
    [HttpGet]
    public IActionResult DownloadSampleFile(string fileType = "txt")
    {
        try
        {
            byte[] fileContent;
            string contentType;
            string fileName;

            switch (fileType.ToLower())
            {
                case "txt":
                    fileContent = System.Text.Encoding.UTF8.GetBytes(
                        $"Sample Text File\nGenerated at: {DateTime.Now}\n\nThis is a test file for automation testing.\nYou can verify this content in your automated tests.");
                    contentType = "text/plain";
                    fileName = $"sample_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                    break;

                case "csv":
                    fileContent = System.Text.Encoding.UTF8.GetBytes(
                        "ID,Product,Price,Quantity\n1,Laptop,1299.99,5\n2,Mouse,29.99,50\n3,Keyboard,89.99,25");
                    contentType = "text/csv";
                    fileName = $"data_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    break;

                case "json":
                    var jsonData = new
                    {
                        generatedAt = DateTime.Now,
                        testData = new[]
                        {
                            new { id = 1, name = "Test Item 1", value = 100 },
                            new { id = 2, name = "Test Item 2", value = 200 }
                        }
                    };
                    fileContent = System.Text.Encoding.UTF8.GetBytes(
                        System.Text.Json.JsonSerializer.Serialize(jsonData, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
                    contentType = "application/json";
                    fileName = $"test_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                    break;

                default:
                    return BadRequest("Invalid file type");
            }

            return File(fileContent, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating download file");
            return StatusCode(500, "Error generating file");
        }
    }

    /// <summary>
    /// Combined Challenge: Complex Scenario
    /// Demonstrates multiple challenges in a single workflow
    /// </summary>
    public IActionResult ComplexScenario()
    {
        ViewBag.SessionId = Guid.NewGuid().ToString("N").Substring(0, 12);
        return View();
    }

    /// <summary>
    /// API endpoint for complex scenario that combines multiple challenges
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ProcessComplexWorkflow([FromBody] ComplexWorkflowRequest request)
    {
        // Simulate processing time
        await Task.Delay(request.ProcessingDelayMs);

        var response = new
        {
            success = true,
            requestId = Guid.NewGuid(),
            processedAt = DateTime.Now,
            steps = new[]
            {
                new { step = 1, status = "completed", message = "Validated input data" },
                new { step = 2, status = "completed", message = "Processed async operation" },
                new { step = 3, status = "completed", message = "Updated database" },
                new { step = 4, status = "completed", message = "Generated report" }
            },
            result = new
            {
                dataProcessed = request.DataItems?.Length ?? 0,
                calculatedValue = (request.DataItems?.Sum() ?? 0) * 1.5,
                statusCode = "SUCCESS"
            }
        };

        return Json(response);
    }
}

/// <summary>
/// Request model for complex workflow
/// </summary>
public class ComplexWorkflowRequest
{
    public string? WorkflowType { get; set; }
    public int[]? DataItems { get; set; }
    public int ProcessingDelayMs { get; set; } = 1000;
}
