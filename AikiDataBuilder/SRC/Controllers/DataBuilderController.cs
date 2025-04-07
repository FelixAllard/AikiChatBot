using System.Text.Json;
using System.Text.Json.Serialization;
using AikiDataBuilder.Database;
using AikiDataBuilder.Services.DataFetcher;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace AikiDataBuilder.Controllers;


[ApiController]
[Route("api/data-builder")]

public class DataBuilderController : ControllerBase
{
    private readonly DataFetcher _dataFetcher;
    private readonly ILogger<DataBuilderController> _logger;
    private readonly SherwebDbContext _sherwebDbContext;
    
    public DataBuilderController(DataFetcher dataFetcher,
         ILogger<DataBuilderController> logger,
         SherwebDbContext dbContext)
    {
        _dataFetcher = dataFetcher;
        _logger = logger;
        _sherwebDbContext = dbContext;
    }
    [HttpGet]
    public async Task<IActionResult> GetEverything()
    {
        _dataFetcher.GetApiFetchers(); // Ensure this does not need to be awaited
        await _dataFetcher.ExecuteAllFetchersAsync(); // Await the async method
        return Ok("Successfully Got everything!");
    }

    [HttpGet("/get-json")]
    public async Task<IActionResult> GetJson()
    {
        var allData = new 
        {
            Customers = await _sherwebDbContext.Customers
                .Include(c => c.Subscriptions) // include foreign key relationships
                    .ThenInclude(s => s.CommitmentTerm)
                        .ThenInclude(x => x.CommittedMinimalQuantities)
                .Include(c => c.Subscriptions) // include foreign key relationships
                    .ThenInclude(s => s.CommitmentTerm)
                        .ThenInclude(x=> x.RenewalConfiguration)
                .Include(c => c.Subscriptions)
                    .ThenInclude(s => s.Fees) //This one is weird
                .Include(c => c.ReceivableCharges)
                    .ThenInclude(s=> s.Charges)
                .Include(c=>c.Platform)
                    .ThenInclude(s => s.PlatformDetails)
                .Include(c=>c.Platform)
                    .ThenInclude(s => s.MeterUsages)
                .ToListAsync(),

            PayableCharges = await _sherwebDbContext.PayableCharges
                .Include(c => c.Charges)
                    .ThenInclude(s => s.Deductions)
                .Include(c => c.Charges)
                    .ThenInclude(s => s.Tags)
                .Include(c => c.Charges)
                    .ThenInclude(s => s.Invoice)
                .Include(c => c.Charges)
                    .ThenInclude(s=> s.Fees)

                .Include(c => c.Charges)
                    .ThenInclude(s => s.Taxes)
                .ToListAsync()

        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles // prevent infinite loops
        };

        string json = JsonSerializer.Serialize(allData, options);
        _logger.LogInformation("Finished all Querries");
        return Ok(json);
    }

}