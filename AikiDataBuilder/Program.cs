using System.Runtime.InteropServices.JavaScript;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using AikiDataBuilder.Database;
using AikiDataBuilder.Model.SystemResponse;
using AikiDataBuilder.Services.DataFetcher;
using AikiDataBuilder.Services.SherwebFetcher;
using AikiDataBuilder.Services.Workers;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Logging.ClearProviders();  // Clear default providers
builder.Logging.AddConsole();      // Add Console logging explicitly
builder.Services.AddSingleton<IConfiguration>(configuration);

var test = new OperationResult<string>()
{
    Status = OperationResultStatus.Success,
    Message = "WELL WELL WELL",
    Result = "Hello World!"
};
// Add DbContext configuration
builder.Services.AddDbContext<SherwebDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
            sqlOptions => sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null)
        )
        .EnableSensitiveDataLogging()
        .LogTo(Console.WriteLine, LogLevel.Information)
);

//Finds all the implementation of IApiFetcher and register them for DI
var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic).ToArray();
builder.Services.Scan(scan => scan
    .FromAssemblies(assemblies)
    .AddClasses(classes => classes.AssignableTo<IApiFetcher>())
    .AsImplementedInterfaces()
    .WithScopedLifetime()
);
builder.Services.Scan(scan => scan
    .FromAssemblyOf<Request>()
    .AddClasses(classes => classes.AssignableTo<Request>())
    .AsSelf()
    .WithTransientLifetime());


builder.Services.Scan(scan => scan
    .FromAssemblies(AppDomain.CurrentDomain.GetAssemblies())
    .AddClasses(classes => classes.AssignableTo<IRequestManager>())
    .AsImplementedInterfaces()
    .WithScopedLifetime()
);
//builder.Services.AddScoped<IApiFetcher, SherwebFetcher>();




//Http client that will be used in requests
builder.Services.AddHttpClient();
// Data Fetcher that contains all other classes of the data builder
builder.Services.AddScoped<DataFetcher>();
builder.Services.AddScoped<SherwebRequestManager>();


builder.Services.AddControllers();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.MapControllers();

app.Run();