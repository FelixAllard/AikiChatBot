using System.Text.Json;
using System.Text.Json.Serialization;
using AikiDataBuilder.Database;
using AikiDataBuilder.Services.DataFetcher;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonSerializer = System.Text.Json.JsonSerializer;


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
                .Include(c => c.Subscriptions)
                    .ThenInclude(s => s.CommitmentTerm)
                        .ThenInclude(x => x.CommittedMinimalQuantities)
                .Include(c => c.Subscriptions) 
                    .ThenInclude(s => s.CommitmentTerm)
                        .ThenInclude(x=> x.RenewalConfiguration)
                .Include(c => c.Subscriptions)
                    .ThenInclude(s => s.Fees)
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
    

    [HttpGet("/customers")]
    public async Task<IActionResult> GetAllCustomer()
    {
        var customers = await _sherwebDbContext.Customers
            .Include(c => c.Subscriptions)
            .ThenInclude(s => s.CommitmentTerm)
            .ThenInclude(x => x.CommittedMinimalQuantities)
            .Include(c => c.Subscriptions)
            .ThenInclude(s => s.CommitmentTerm)
            .ThenInclude(x => x.RenewalConfiguration)
            .Include(c => c.Subscriptions)
            .ThenInclude(s => s.Fees)
            .Include(c => c.ReceivableCharges)
            .ThenInclude(s => s.Charges)
            .Include(c => c.Platform)
            .ThenInclude(s => s.PlatformDetails)
            .Include(c => c.Platform)
            .ThenInclude(s => s.MeterUsages)
            .ToListAsync();

        var serializerSettings = new JsonSerializerSettings
        {
            
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        var flattenedList = new List<Dictionary<string, object>>();

        foreach (var customer in customers)
        {
            var json = JsonConvert.SerializeObject(customer, serializerSettings); // <-- 🔥 ignore loops
            var jObject = JObject.Parse(json);

            var flat = new Dictionary<string, object>();
            Flatten(jObject, flat, null);

            flattenedList.Add(flat);
        }

        _logger.LogInformation("Finished all Queries");

        var compactJson = JsonConvert.SerializeObject(flattenedList, Formatting.None); // <-- FORCE minified

        return Ok(compactJson);
    }


    [HttpGet("/payable-charges")]
    public async Task<IActionResult> GetAllPayableCharges()
    {
        var PayableCharges = await _sherwebDbContext.PayableCharge
            .Include(c => c.Deductions)
            .Include(c => c.Tags)
            .Include(c => c.Invoice)
            .Include(c => c.Fees)
            .Include(c => c.Taxes)
            .ToListAsync();
        var serializerSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            
        };

        var flattenedList = new List<Dictionary<string, object>>();

        foreach (var customer in PayableCharges)
        {
            var json = JsonConvert.SerializeObject(customer, serializerSettings); // <-- 🔥 ignore loops
            var jObject = JObject.Parse(json);

            var flat = new Dictionary<string, object>();
            Flatten(jObject, flat, null);

            flattenedList.Add(flat);
        }

        _logger.LogInformation("Finished all Queries");

        var compactJson = JsonConvert.SerializeObject(flattenedList, Formatting.None); // <-- FORCE minified
        return Ok(compactJson);
    }
    
    
// --- Helper to flatten nested JSON ---
    private void Flatten(JToken token, Dictionary<string, object> flat, string prefix)
    {
        if (token is JObject jObject)
        {
            foreach (var property in jObject.Properties())
            {
                var key = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}_{property.Name}";

                Flatten(property.Value, flat, key);
            }
        }
        else if (token is JArray jArray)
        {
            for (int i = 0; i < jArray.Count; i++)
            {
                Flatten(jArray[i], flat, $"{prefix}_{i}"); // array index becomes part of the key
            }
        }
        else if (token is JValue jValue)
        {
            flat[prefix] = jValue.Value; // finally safe to assign
        }
    }



    private string Combine(string prefix, string name)
    {
        if (string.IsNullOrEmpty(prefix))
            return name;
        return $"{prefix}.{name}";
    }
}