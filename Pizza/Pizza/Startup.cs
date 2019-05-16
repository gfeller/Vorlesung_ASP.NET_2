using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pizza.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Pizza.Services;
using Pizza.Utilities;
using Pizza_Demo.Exceptions;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

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
            ConfigureDatabase(services);
            ConfigureIdentityService(services);
            ConfigureAuthorizationService(services);
            ConfigureTokenAuth(services);
            ConfigureSwagger(services);

            services.AddOptions();
            services.AddSession();


            // Add application services.
            services.AddTransient<OrderService>();
            services.AddTransient<SecurityService>();
            services.AddTransient<DataService>();

            services.AddMvc(options => { options.Filters.Add(new ValidateModelAttribute()); })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            ConfigureDev(app, env);
            app.UseExceptionHandler(ConfigureExceptionHandler);

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAuthentication();

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });


            app.UseSession(new SessionOptions() { IdleTimeout = TimeSpan.FromMinutes(30) });
            app.UseMvc();
        
        }


        private static void ConfigureSwagger(IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                // need a fix for the new swagger version
                options.AddSecurityDefinition("JWT Token", new ApiKeyScheme
                {
                    Description = "Please enter into field the word 'Bearer' following by space and JWT",
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey"
                });

                options.OperationFilter<SecurityRequirementsOperationFilter>();
 
                options.SwaggerDoc("v1", new Info()
                {
                    Version = "v1",
                    Contact = new Contact()
                    {
                        Email = "mgfeller@hsr.ch",
                        Name = "Michael Gfeller",
                        Url = "https://github.com/gfeller"
                    },
                    Description = "Pizza Service API",
                    Title = "Pizza Service",
                });
            });
        }

        public virtual void ConfigureDatabase(IServiceCollection services)
        {
            //  services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase("Pizza"));
      
        }

        private void ConfigureIdentityService(IServiceCollection services)
        {
            services.AddDefaultIdentity<IdentityUser>(options =>
                {
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequiredLength = 4;
                    options.Password.RequireUppercase = false;
                    options.Lockout.MaxFailedAccessAttempts = 3;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(20);
                })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddAuthentication().AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options => { options.TokenValidationParameters = GetTokenValidationParameters(); });
        }

        private void ConfigureTokenAuth(IServiceCollection services)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048);
            RSAParameters rsaKeyInfo = rsa.ExportParameters(true);

            _key = new RsaSecurityKey(rsaKeyInfo);

            var tokenOptions = new TokenAuthOptions()
            {
                Audience = TokenAudience,
                Issuer = TokenIssuer,
                SigningCredentials = new SigningCredentials(_key, SecurityAlgorithms.RsaSha256Signature)
            };
            services.AddSingleton(tokenOptions);
        }

        private static void ConfigureAuthorizationService(IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("Founders", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim(ClaimTypes.Name, "mgfeller@hsr.ch", "sgehrig@hsr.ch", "mstolze@hsr.ch");
                });

                options.AddPolicy("ElevatedRights", policy =>
                    policy.RequireRole("Administrator", "PowerUser", "BackupAdministrator")
                );
            });
        }

        private void ConfigureDev(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();

                using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    //  serviceScope.ServiceProvider.GetService<ApplicationDbContext>().Database.Migrate();
                }

                app.ApplicationServices.GetService<DataService>().EnsureData("123456");
            }

            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
        }

        private TokenValidationParameters GetTokenValidationParameters()
        {
            return new TokenValidationParameters
            {
                // The signing key must match!
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _key,

                // Validate the JWT Issuer (iss) claim
                ValidateIssuer = true,
                ValidIssuer = TokenIssuer,

                // Validate the JWT Audience (aud) claim
                ValidateAudience = true,
                ValidAudience = TokenAudience,

                // Validate the token expiry
                ValidateLifetime = true,

                // If you want to allow a certain amount of clock drift, set that here:
                ClockSkew = TimeSpan.Zero
            };
        }

        private void ConfigureExceptionHandler(IApplicationBuilder errorApp)
        {
            errorApp.Run(async context =>
            {
                var errorFeature = context.Features.Get<IExceptionHandlerFeature>();
                var exception = errorFeature.Error as ServiceException;

                var metadata = new
                {
                    Message = "An unexpected error occurred! The error ID will be helpful to debug the problem",
                    DateTime = DateTimeOffset.Now,
                    RequestUri = new Uri(context.Request.Host.ToString() + context.Request.Path.ToString() + context.Request.QueryString, UriKind.RelativeOrAbsolute),
                    ErrorId = exception == null ? "Unkown" : exception.Code,
                    Type = exception?.Type ?? ServiceExceptionType.Unkown,
                    ExceptionMessage = exception?.Message,
                    ExceptionStackTrace = exception?.StackTrace
                };
                context.Response.ContentType = "application/json";
                if (exception != null)
                {
                    context.Response.StatusCode = (int)exception.Type;
                }

                await context.Response.WriteAsync(JsonConvert.SerializeObject(metadata));
            });
        }
    }


    public class TokenAuthOptions
    {
        public string Audience { get; set; }
        public string Issuer { get; set; }
        public SigningCredentials SigningCredentials { get; set; }
    }

     
}