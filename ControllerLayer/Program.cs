using ApplicationLayer.Services.UserManagement;
using ApplicationLayer.Services.MovieManagement;
using InfrastructureLayer.Data;
using InfrastructureLayer.Repository;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ApplicationLayer.Services.PromotionManagement;
using ApplicationLayer.Services.CinemaRoomManagement;

namespace ControllerLayer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Đăng kí service tại đây 

            //===================================================================================================================================================

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                });
            
            // Cấu hình CORS để cho phép UI share credentials
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowUI", policy =>
                {
                    policy.WithOrigins("https://localhost:7069", "http://localhost:7069", "http://localhost:5073", "https://localhost:5073", "http://localhost:5000", "https://localhost:5001")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials(); // Quan trọng: cho phép share cookies
                });
                
                // Policy cho public APIs - không cần credentials
                options.AddPolicy("PublicAPI", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });
            
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Cấu hình DbContext
            builder.Services.AddDbContext<MovieContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Đăng ký AutoMapper
            builder.Services.AddAutoMapper(typeof(ApplicationLayer.Mappings.MovieMappingProfile));

            // Đăng ký Generic Repository Pattern
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            
            // Đăng ký Repository và Services
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IUserService, UserService>();

            builder.Services.AddScoped<IMovieService, MovieService>();
            builder.Services.AddScoped<IMovieRepository, MovieRepository>();

            builder.Services.AddScoped<IPromotionService, PromotionService>();

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

            //Tự động migrate database và seed dữ liệu
            using (var scope = app.Services.CreateScope())
            {
                Console.WriteLine("Đang kiểm tra database...");
                var context = scope.ServiceProvider.GetRequiredService<MovieContext>();

                // Temporary: Use EnsureCreated to bootstrap database
                if (context.Database.EnsureCreated())
                {
                    Console.WriteLine("Database created successfully with new schema!");
                }
                else
                {
                    Console.WriteLine("Database already exists!");
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
