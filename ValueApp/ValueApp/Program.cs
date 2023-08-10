using ValueApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<IValueService, ValueService>();


var app = builder.Build();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();