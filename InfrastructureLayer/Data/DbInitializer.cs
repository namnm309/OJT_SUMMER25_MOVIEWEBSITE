using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfrastructureLayer.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;


namespace InfrastructureLayer.Database
{
    public static class DbInitializer
    {
        public static IApplicationBuilder UseInitializeDatabase(this IApplicationBuilder application)
        {
            using var serviceScope = application.ApplicationServices.CreateScope();
            var dbContext = serviceScope.ServiceProvider.GetService<MovieContext>();

            if (dbContext != null)
            {
                // Apply pending migrations
                if (dbContext.Database.GetPendingMigrations().Any())
                {
                    Console.WriteLine("Applying Migrations...");
                    dbContext.Database.Migrate();
                }

                // Seed data
                try
                {
                    DataSeeder.SeedAdminUser(dbContext).Wait();
                    DataSeeder.SeedSampleData(dbContext).Wait();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error seeding data: {ex.Message}");
                }
            }

            return application;
        }
    }
}
