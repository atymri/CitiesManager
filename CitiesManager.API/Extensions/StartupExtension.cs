using Asp.Versioning;
using CitiesManager.Core.Identity;
using CitiesManager.Infrastructure.DatabaseContext;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace CitiesManager.API.Extensions
{
    public static class StartupExtension
    {
        public static void ConfigureServices(this WebApplicationBuilder builder)
        {

            builder.Services.AddControllers(options =>
            {
                // Filters for content negotiation
                options.Filters.Add(new ProducesAttribute("application/json"));
                options.Filters.Add(new ConsumesAttribute("application/json"));

            });

            builder.Services.AddApiVersioning(config =>
            {
                // Read API version from URL segment (e.g., /api/v1/cities)
                config.ApiVersionReader = new UrlSegmentApiVersionReader();
                // config.ApiVersionReader = new HeaderApiVersionReader("ver");
                // config.ApiVersionReader = new QueryStringApiVersionReader("v");

                config.DefaultApiVersion = new ApiVersion(1, 0);
                config.AssumeDefaultVersionWhenUnspecified = true;
            }).AddApiExplorer(setup =>
            {
                setup.GroupNameFormat = "'v'VVV"; // e.g., v1, v2
                setup.SubstituteApiVersionInUrl = true;
            });

            // Swagger setup
            builder.Services.AddSwaggerGen(c =>
            {
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "api.xml"));
                c.SwaggerDoc("v1", new OpenApiInfo() { Title = "Cities API v1", Version = "1.0" });
                c.SwaggerDoc("v2", new OpenApiInfo() { Title = "Cities API v2", Version = "2.0" });
            });

            var connectionString = builder.Configuration.GetConnectionString("Default") ??
                                   throw new ArgumentNullException("CONNECTION STRING IS NULL!");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            builder.Services.AddEndpointsApiExplorer();

            /*
                CORS: Which requests are allowed?
                CORS indicates that which domains can send request to our domain, like
                our API is hosted on api.cities.com, which domains are allowed to send requests.
                Here, we are allowing all domains to send requests to our API:

                builder.Services.AddCors(options =>
               {
                   options.AddDefaultPolicy(builder =>
                   {
                       builder.WithOrigins("*");
                   });
               });


             */

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy
                        .WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()!)
                        .WithHeaders("Authorization", "origin", "accept", "content-type")
                        .WithMethods("GET", "POST", "PUT", "DELETE");
                    // simply saying that only requests sent from localhost:4200 are allowed.

                });

                options.AddPolicy("4100Client", policy =>
                {
                    policy
                        .WithOrigins("http://localhost:4100")
                        .WithHeaders("Authorization", "origin", "accept")
                        .WithMethods("GET");
                });
            });

            builder.Services.AddIdentity<User, Role>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 4;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders()
                .AddUserStore<UserStore<User, Role, ApplicationDbContext, Guid>>()
                .AddRoleStore<RoleStore<Role, ApplicationDbContext, Guid>>();

        }
    }
}
