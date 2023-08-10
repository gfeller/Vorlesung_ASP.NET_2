using System;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Pizza.Data;
using Pizza.Services;
using Pizza_Demo.Exceptions;
using Swashbuckle.AspNetCore.Filters;

namespace Pizza;

public static class StartupExtension
{
    private static RsaSecurityKey _key;
    private static string TokenAudience = "self";
    private static string TokenIssuer = "pizza";

    public static void ConfigureSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            // need a fix for the new swagger version
            options.AddSecurityDefinition("JWT Token", new OpenApiSecurityScheme()
            {
                Description = "Please enter into field the word 'Bearer' following by space and JWT",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey
            });

            options.OperationFilter<SecurityRequirementsOperationFilter>();

            options.SwaggerDoc("v1", new OpenApiInfo()
            {
                Version = "v1",
                Contact = new OpenApiContact()
                {
                    Email = "mgfeller@hsr.ch",
                    Name = "Michael Gfeller",
                    Url = new Uri("https://github.com/gfeller")
                },
                Description = "Pizza Service API",
                Title = "Pizza Service",
            });
        });
    }

    public static void ConfigureDatabase(this IServiceCollection services)
    {
        services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase("Pizza"));


    }

    public static void ConfigureIdentityService(this IServiceCollection services)
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


    public static void ConfigureTokenAuth(this IServiceCollection services)
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

    public static void ConfigureAuthorizationService(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("Founders", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim(ClaimTypes.Name, "michael.gfeller@ost.ch", "markus.stolze@ost.ch");
            });

            options.AddPolicy("ElevatedRights", policy =>
                policy.RequireRole("Administrator", "PowerUser", "BackupAdministrator")
            );
        });
    }


    private static TokenValidationParameters GetTokenValidationParameters()
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

    public static void ConfigureExceptionHandler(this IApplicationBuilder errorApp)
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


    public static void ConfigureDev(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();

            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                //  serviceScope.ServiceProvider.GetService<ApplicationDbContext>().Database.Migrate();
            }

            app.ApplicationServices.GetRequiredService<DataService>().EnsureData("123456");
        }

        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }
    }

    public class TokenAuthOptions
    {
        public string Audience { get; set; }
        public string Issuer { get; set; }
        public SigningCredentials SigningCredentials { get; set; }
    }

}