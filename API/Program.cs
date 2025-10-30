using Application.Services;
using Application.ServicesImp;
using Domain.Interfaces;
using Infrastucture.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<IGeolocationService, IPGeolocationService>();

builder.Services.AddSingleton<IBlockedCountryRepository, InMemoryBlockedCountryRepository>();
builder.Services.AddSingleton<ITemporalBlockRepository, InMemoryTemporalBlockRepository>();
builder.Services.AddSingleton<IBlockedAttemptRepository, InMemoryBlockedAttemptRepository>();

builder.Services.AddScoped<ICountryBlockService, CountryBlockService>();
builder.Services.AddHostedService<TemporalBlockCleanupService>();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
