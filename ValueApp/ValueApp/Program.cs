using Microsoft.AspNetCore.Mvc.Infrastructure;
using ValueApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddSingleton<IValueService, ValueService>();


var app = builder.Build();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();