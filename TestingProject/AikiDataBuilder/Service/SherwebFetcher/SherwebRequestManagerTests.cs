using AikiDataBuilder.Database;
using AikiDataBuilder.Model.SystemResponse;
using AikiDataBuilder.Services.SherwebFetcher;
using Azure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TestingProject.AikiDataBuilder.Service.SherwebFetcher;

[TestFixture]
public class SherwebRequestManagerTests
{
    
    private SherwebRequestManager _sherwebRequestManager;
    private IHttpClientFactory _httpClientFactory;
    private IConfiguration _configuration;
    private IServiceProvider _serviceProvider;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        //Fill in the credidentials, but DO NOT KEEP THEM IN THERE
        /*var inMemorySettings = new Dictionary<string, string>
        {
            { "SherwebCredentials:BaseUrl", "https://api.sherweb.com" },
            { "SherwebCredentials:SubscriptionKey", "" },
            { "SherwebCredentials:ClientId", "" },
            { "SherwebCredentials:ClientSecret", "" }
        };*/


        var basePath = Path.GetFullPath("../../../../AikiDataBuilder");
        _configuration = new ConfigurationBuilder()
            .SetBasePath(basePath) // Ensures correct base directory
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            //.AddInMemoryCollection(inMemorySettings)
            .Build();


        
        var serviceCollection = new ServiceCollection();

        // Register IHttpClientFactory
        serviceCollection.AddHttpClient();

        // Build the service provider
        _serviceProvider = serviceCollection.BuildServiceProvider();

        // Get IHttpClientFactory from DI
        _httpClientFactory = _serviceProvider.GetRequiredService<IHttpClientFactory>();
    }
    
    [SetUp]
    public void Start()
    {
        _sherwebRequestManager = new SherwebRequestManager(
            _configuration,
            _httpClientFactory
        );
        
        
    }
    [Test]
    public async Task GetCredentials_SeeAchievesCredentials_ReturnsCredentials()
    {
        var response =  _sherwebRequestManager.GetCredentials();
        Assert.IsNotNull(response);
        Assert.That(response.Status, Is.EqualTo(OperationResultStatus.Success));
        
        foreach (var kvp in response.Result)
        {
            TestContext.Out.WriteLine($"{kvp.Key}: {kvp.Value}");
        }

        Assert.That(response.Exception, Is.Null);
    }
    
    
    [Test]
    public async Task ResetAuthorizationToken_GetsAuthorizationToken_ReturnAuthorizationToken()
    {
        var response = await _sherwebRequestManager.ResetAuthorizationToken();
        Assert.That(response, Is.Not.Null);
        TestContext.Out.WriteLine(response.Status);
        Assert.That(response.Status, Is.EqualTo(OperationResultStatus.Success));
        Assert.That(response.Exception, Is.Null);
        
        TestContext.Out.WriteLine(response.Result.Item1);
        
        Assert.That(response.Result.Item2, Is.True);
    }


    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
    
}