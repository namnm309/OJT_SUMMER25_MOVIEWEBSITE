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

            // only call this method when there are pending migrations
            if (dbContext != null && dbContext.Database.GetPendingMigrations().Any())
            {
                Console.WriteLine("Applying  Migrations...");
                dbContext.Database.Migrate();
            }

            // Seed data regardless of migrations
            if (dbContext != null)
            {
                Console.WriteLine("Seeding database data...");
                DataSeeder.SeedData(dbContext).Wait();
                Console.WriteLine("Database seeding completed.");
            }

            return application;
        }
    }
}
