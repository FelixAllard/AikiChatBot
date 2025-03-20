using System.Runtime.InteropServices.JavaScript;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using AikiDataBuilder.Database;
using AikiDataBuilder.Services.DataFetcher;
using AikiDataBuilder.Services.SherwebFetcher;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DbContext configuration
builder.Services.AddDbContext<SherwebDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null)
    )
);

//Http client that will be used in requests
builder.Services.AddHttpClient();
// Data Fetcher that contains all other classes of the data builder
//builder.Services.AddSingleton<DataFetcher>();


var app = builder.Build();

SherwebFetcher programTest = new SherwebFetcher();
programTest.Init();
programTest.CreateWorkers();
await programTest.GetInformationFromApi(DateTime.Now);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();