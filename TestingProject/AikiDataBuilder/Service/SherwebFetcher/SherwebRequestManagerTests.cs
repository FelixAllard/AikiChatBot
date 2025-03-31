using AikiDataBuilder.Database;
using AikiDataBuilder.Model.SystemResponse;
using AikiDataBuilder.Services.SherwebFetcher;
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
        var inMemorySettings = new Dictionary<string, string>
        {
            { "SherwebCredentials:BaseUrl", "https://api.sherweb.com" },
            { "SherwebCredentials:SubscriptionKey", "test-subscription-key" },
            { "SherwebCredentials:ClientId", "test-client-id" },
            { "SherwebCredentials:ClientSecret", "test-client-secret" }
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
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
            new SherwebDbContext(new DbContextOptions<SherwebDbContext>()),
            _httpClientFactory
        );
        
        
    }

    [Test]
    public async Task ResetAuthorizationToken_GetsAuthorizationToken_ReturnAuthorizationToken()
    {
        _sherwebRequestManager.GetCredentials();
        var response = await _sherwebRequestManager.ResetAuthorizationToken();
        Assert.That(response, Is.Not.Null);
        
        Assert.That(response.Status, Is.EqualTo(OperationResultStatus.Success));
        Assert.That(response.Exception, Is.Null);
        
        Console.WriteLine(response.Result.Item1);
        
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