using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;

namespace UI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddControllersWithViews();


            builder.Services.AddHttpContextAccessor();

            // Add HttpClient for API calls với credential sharing
            builder.Services.AddHttpClient("ApiClient", client =>
            {
                var baseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5274";
                client.BaseAddress = new Uri(baseUrl);
            }).ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler()
                {
                    UseCookies = true,
                    CookieContainer = new System.Net.CookieContainer()
                };
            });


            // ----- CookieAuthentication đã vô hiệu hoá, chuyển sang JwtBearer sử dụng token lưu ở Session -----
            /*
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    options.AccessDeniedPath = "/Home/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromHours(2);
                    options.SlidingExpiration = true;
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                });
            */

            var jwtSecret = builder.Configuration["Jwt:Secret"] ?? Environment.GetEnvironmentVariable("JWT_SECRET") ?? "THIS IS DEFAULT SECRET, CHANGE IT";

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecret)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    RoleClaimType = ClaimTypes.Role,
                    NameClaimType = ClaimTypes.Name,
                    ClockSkew = TimeSpan.Zero
                };

                // Lấy token từ Session thay vì header (cho các request render Razor)
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = ctx =>
                    {
                        var token = ctx.HttpContext.Session.GetString("JWToken");
                        if (!string.IsNullOrEmpty(token))
                        {
                            ctx.Token = token;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.AddAuthorization();

            // Thêm Session để lưu trữ JWT
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Đăng ký ApiService
            builder.Services.AddScoped<UI.Services.IApiService, UI.Services.ApiService>();

            // Đăng ký AuthUIService
            builder.Services.AddScoped<UI.Services.IAuthUIService, UI.Services.AuthUIService>();

            // Đăng ký ImageService (for movie/promotion management)
            builder.Services.AddScoped<UI.Services.IImageService, UI.Services.CloudinaryImageService>();

            // Đăng ký BookingManagementUIService
            builder.Services.AddScoped<UI.Areas.BookingManagement.Services.IBookingManagementUIService,
                          UI.Areas.BookingManagement.Services.BookingManagementUIService>();

            // Đăng ký ShowtimeService
            builder.Services.AddScoped<UI.Areas.ShowtimeManagement.Services.IShowtimeService,
                          UI.Areas.ShowtimeManagement.Services.ShowtimeService>();

            // Register PromotionManagementUIService
            builder.Services.AddScoped<UI.Areas.PromotionManagement.Services.IPromotionManagementUIService,
                          UI.Areas.PromotionManagement.Services.PromotionManagementUIService>();


            // Đăng ký CinemaManagementUIService
            builder.Services.AddScoped<UI.Areas.CinemaManagement.Services.ICinemaManagementUIService,
                          UI.Areas.CinemaManagement.Services.CinemaManagementUIService>();

            var app = builder.Build();


            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");

                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            // Thêm routing cho Areas
            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
