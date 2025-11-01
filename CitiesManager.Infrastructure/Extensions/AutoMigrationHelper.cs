using System;
using System.Threading.Tasks;
using CitiesManager.Infrastructure.DatabaseContext;
using EFCore.AutomaticMigrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CitiesManager.Infrastructure.Extensions
{
    public static class AutoMigrationHelper
    {
        public static async Task ApplyPendingMigrationsAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var options = new DbMigrationsOptions
            {
                AutomaticMigrationsEnabled = true,
                AutomaticMigrationDataLossAllowed = true,
                ResetDatabaseSchema = false
            };

            await context.MigrateToLatestVersionAsync(options);
        }
    }
}