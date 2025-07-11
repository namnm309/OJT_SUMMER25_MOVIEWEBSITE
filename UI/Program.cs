using Microsoft.AspNetCore.Authentication.Cookies;

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

            builder.Services.AddAuthorization();

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
