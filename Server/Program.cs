using Microsoft.EntityFrameworkCore;
using Server.Hub;
using Server.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddDbContext<ApplicationContextDb>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSignalR();
builder.Services.AddScoped<ListContactHub>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

var connectionString = app.Configuration.GetConnectionString("DefaultConnection");

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseHsts();

Log.Information("Сервер запущен");

app.MapHub<ListContactHub>("/ListContactHub");

app.MapGet("/", () => "Server is running");

try
{
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Приложение завершилось с критической ошибкой");
}
finally
{
    Log.CloseAndFlush();
}