
using ApplicationLayer.Services.CinemaRoomManagement;
using ApplicationLayer.Services.UserManagement;
using InfrastructureLayer.Data;
using InfrastructureLayer.Repository;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace ControllerLayer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Đăng kí service tại đây 

            //===================================================================================================================================================

            builder.Services.AddControllers();

            
            // Cấu hình CORS để cho phép UI share credentials
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowUI", policy =>
                {
                    policy.WithOrigins("https://localhost:7069", "http://localhost:7069")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials(); // Quan trọng: cho phép share cookies
                });
            });
            
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Cấu hình DbContext
            builder.Services.AddDbContext<MovieContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Đăng ký Repository và Services
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<ICinemaRoomRepository, CinemaRoomRepository>();
            builder.Services.AddScoped<ICinemaRoomService, CinemaRoomService>();

            // Cấu hình Authentication với Cookie
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/api/user/login";
                    options.LogoutPath = "/api/user/logout"; 
                    options.AccessDeniedPath = "/api/user/access-denied";
                    options.ExpireTimeSpan = TimeSpan.FromHours(2);
                    options.SlidingExpiration = true;
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                });

            builder.Services.AddAuthorization();

            //===================================================================================================================================================

            var app = builder.Build();

            //Tự động tạo database nếu chưa có và seed dữ liệu
            using (var scope = app.Services.CreateScope())
            {
                Console.WriteLine("Đang kiểm tra database...");
                var context = scope.ServiceProvider.GetRequiredService<MovieContext>();

                //Lưu ý nếu sử dụng EnsureCreated thì sẽ ko dùng đc Migrations 
                //Migrations vẫn sẽ auto tạo đc db và vip hơn nhiều 
                if (context.Database.EnsureCreated())
                {
                    Console.WriteLine("Database đã được tạo thành công!");
                }
                else
                {
                    Console.WriteLine("Database đã tồn tại!");
                }

                // Seed dữ liệu admin mặc định
                await DataSeeder.SeedAdminUser(context);
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            
            // Enable CORS
            app.UseCors("AllowUI");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            await app.RunAsync();
        }
    }
}
