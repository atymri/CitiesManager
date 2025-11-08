using System.Text;
using Asp.Versioning;
using CitiesManager.Core.Identity;
using CitiesManager.Core.ServiceContracts;
using CitiesManager.Core.Services;
using CitiesManager.Infrastructure.DatabaseContext;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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

            builder.Services.AddTransient<IJwtService, JwtService>();


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

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                };
                options.IncludeErrorDetails = true;
            });

            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Cities Manager API", Version = "v1" });
                options.SwaggerDoc("v2", new OpenApiInfo { Title = "Cities Manager API", Version = "v2" });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter JWT Token in this format: {your token}"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
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
