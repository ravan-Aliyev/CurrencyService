using Mapster;
using CurrencyService.Application;
using CurrencyService.Infrasturucture;
using CurrencyService.Api;
using CurrencyService.Api.Startup;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices();
builder.Services.AddApiServices(builder.Configuration);

//Logging
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Seq("http://seq:5341") // Seq URL
    .CreateLogger();

builder.Host.UseSerilog();

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.AddApiMiddlewares();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
