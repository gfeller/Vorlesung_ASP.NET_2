using System;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Swagger;
using ValueApp.Exceptions;
using ValueApp.Services;

namespace ValueApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IValueService, ValueService>();

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            services.AddSwaggerGen(options =>
            {
                options.IncludeXmlComments(Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "ValueApp.xml"));
                options.SwaggerDoc("v1", new Info()
                {
                    Version = "v1",
                    Contact = new Contact() {Email = "mgfeller@hsr.ch", Name = "Michael Gfeller", Url = "https://github.com/gfeller"},
                    Description = "Das ist eine Demo",
                    Title = "Value Service"
                });
            });

            services.AddMvc(options => { options.Filters.Add(new ValidateModelAttribute()); }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();


            app.UseExceptionHandler(ConfigureErrorHandler);

            app.UseSwagger();
            app.UseSwaggerUI(options => { options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"); });
            app.UseMvc();
        }

        private void ConfigureErrorHandler(IApplicationBuilder errorApp)
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
                context.Response.StatusCode = exception != null ? (int) exception.Type : (int) HttpStatusCode.InternalServerError;
                await context.Response.WriteAsync(JsonConvert.SerializeObject(metadata));
            });
        }
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
}
