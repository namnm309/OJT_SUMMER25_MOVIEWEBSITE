using ApplicationLayer.Mapper;
using ApplicationLayer.Services;
using DomainLayer.Entities;
using InfrastructureLayer.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MovieWebApplication.Data;

namespace ControllerLayer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Thêm Dbcontext 
            builder.Services.AddDbContext<MovieContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Đăng ký AutoMapper
            builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

            // Đăng ký repository generic
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            //Register Service
            builder.Services.AddScoped<IPasswordHasher<Users>, PasswordHasher<Users>>();
            builder.Services.AddScoped<IAuthService, AuthService>();

            var app = builder.Build();

            // Tự động tạo database nếu chưa có
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<MovieContext>();
                context.Database.EnsureCreated();
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }


            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
