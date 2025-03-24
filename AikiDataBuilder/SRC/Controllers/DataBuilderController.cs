using AikiDataBuilder.Services.DataFetcher;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AikiDataBuilder.Controllers;


[ApiController]
[Route("api/data-builder")]

public class DataBuilderController : ControllerBase
{
    private readonly DataFetcher _dataFetcher;
    private readonly ILogger<DataBuilderController> _logger;
    
    public DataBuilderController(DataFetcher dataFetcher,
         ILogger<DataBuilderController> logger)
    {
        _dataFetcher = dataFetcher;
        _logger = logger;
        _logger.LogInformation("BUILT THE DATABUILDERCONTROLLER");
    }
    [HttpGet]
    public async Task<IActionResult> GetEverything()
    {
        _logger.LogInformation("GET EVERYTHING");
        _dataFetcher.GetApiFetchers(); // Ensure this does not need to be awaited
        await _dataFetcher.ExecuteAllFetchersAsync(); // Await the async method
        return Ok("Did things ong");
    }

}