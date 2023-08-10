using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.OpenApi.Models;
using System.Net;
using System.Text.Json;
using ValueApp.Exceptions;
using ValueApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options => { options.Filters.Add(new ValidateModelAttribute()); });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // options.IncludeXmlComments(Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "ValueApp.xml"));
    options.SwaggerDoc("v1", new OpenApiInfo()
    {
        Version = "v1",
        Contact = new OpenApiContact() { Email = "michael.gfeller@ost.ch", Name = "Michael Gfeller", Url = new Uri("https://github.com/gfeller") },
        Description = "Das ist eine Demo",
        Title = "Value Service"
    });
});


builder.Services.AddSingleton<IValueService, ValueService>();
builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseExceptionHandler(ConfigureErrorHandler);

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();



void ConfigureErrorHandler(IApplicationBuilder errorApp)
{
    errorApp.Run(async context =>
    {
        var errorFeature = context.Features.Get<IExceptionHandlerFeature>();
        var exception = errorFeature.Error as ServiceException;


        var metadata = new
        {
            Message = "An unexpected error occurred! The error ID will be helpful to debug the problem",
            DateTime = DateTimeOffset.Now,
            RequestUri = new Uri(context.Request.Host.ToString() + context.Request.Path.ToString() + context.Request.QueryString),
            Type = exception?.Type ?? ServiceExceptionType.Unkown,
            ExceptionMessage = exception?.Message,
            ExceptionStackTrace = exception?.StackTrace,
        };
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = exception != null ? (int)exception.Type : (int)HttpStatusCode.InternalServerError;
        await context.Response.WriteAsync(JsonSerializer.Serialize(metadata));
    });
}

public class ValidateModelAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            throw new ServiceException(ServiceExceptionType.ForbiddenByRule);
        }
    }
}