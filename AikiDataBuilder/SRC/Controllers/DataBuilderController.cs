using AikiDataBuilder.Services.DataFetcher;
using Microsoft.AspNetCore.Mvc;

namespace AikiDataBuilder.Controllers;


[ApiController]
[Route("api/data-builder")]

public class DataBuilderController : ControllerBase
{
    private readonly DataFetcher _dataFetcher;
    
    public DataBuilderController(DataFetcher dataFetcher)
    {
        _dataFetcher = dataFetcher;
    }
    [HttpGet]
    public async Task<IActionResult> GetEverything()
    {
        _dataFetcher.GetApiFetchers();
        return Ok(_dataFetcher.ExecuteAllFetchersAsync());
    }
}