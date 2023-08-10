using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Pizza.Services;
using Pizza.Utilities;


namespace Pizza
{
    public class Startup
    {
        private RsaSecurityKey _key;
        const string TokenAudience = "self";
        const string TokenIssuer = "pizza";


        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureDatabase();
            services.ConfigureSwagger();
            services.ConfigureIdentityService();
            services.ConfigureAuthorizationService();
            services.ConfigureTokenAuth();


            services.AddOptions();
            services.AddSession();


            // Add application services.
            services.AddTransient<OrderService>();
            services.AddTransient<SecurityService>();
            services.AddTransient<DataService>();


            services
                .AddMvc(options => { options.Filters.Add(new ValidateModelAttribute()); })
                .AddApplicationPart(typeof(Program).Assembly);

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            app.ConfigureDev(env);

            app.UseExceptionHandler(StartupExtension.ConfigureExceptionHandler);

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseSwagger();
            app.UseSwaggerUI(options => { options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"); });


            app.UseSession(new SessionOptions() { IdleTimeout = TimeSpan.FromMinutes(30) });
            app.UseRouting();


            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }
    }
}