using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IO;

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
                var baseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://cinemacity-backend-hhasbzggfafpgbgw.eastasia-01.azurewebsites.net";
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
                    },
                    // Nếu request không phải API (hoặc không yêu cầu JSON) và chưa xác thực, chuyển hướng tới trang đăng nhập
                    OnChallenge = context =>
                    {
                        // Chỉ can thiệp nếu chưa bắt đầu gửi response
                        if (!context.HttpContext.Response.HasStarted)
                        {
                            var request = context.HttpContext.Request;

                            // Xác định request API hay Razor page dựa trên đường dẫn hoặc header Accept
                            var isApiRequest = request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase) ||
                                               request.Headers["Accept"].Any(h => h.Contains("application/json", StringComparison.OrdinalIgnoreCase));

                            if (!isApiRequest)
                            {
                                context.HandleResponse(); // Ngăn JWT middleware trả về 401 mặc định
                                var returnUrl = Uri.EscapeDataString(request.Path + request.QueryString);
                                context.Response.Redirect($"/Account/Login?returnUrl={returnUrl}");
                            }
                        }
                        return Task.CompletedTask;
                    },
                    OnForbidden = context =>
                    {
                        // Tương tự OnChallenge nhưng cho trường hợp đã xác thực nhưng không có quyền
                        if (!context.Response.HasStarted)
                        {
                            var request = context.Request;
                            var isApiRequest = request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase) ||
                                               request.Headers["Accept"].Any(h => h.Contains("application/json", StringComparison.OrdinalIgnoreCase));
                            if (!isApiRequest)
                            {
                                context.Response.Redirect("/Home/AccessDenied");
                            }
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

            // Register ConcessionManagementUIService
            builder.Services.AddScoped<UI.Areas.ConcessionManagement.Services.IConcessionManagementUIService,
                          UI.Areas.ConcessionManagement.Services.ConcessionManagementUIService>();

            var app = builder.Build();


            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");

                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Proxy API requests to API server
            app.Map("/api/{**catch-all}", async context =>
            {
                var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"];
                var targetUrl = $"{apiBaseUrl}{context.Request.Path}{context.Request.QueryString}";
                
                using var httpClient = new HttpClient();
                var request = new HttpRequestMessage(new HttpMethod(context.Request.Method), targetUrl);
                
                // Copy headers
                foreach (var header in context.Request.Headers)
                {
                    if (!header.Key.StartsWith("Host", StringComparison.OrdinalIgnoreCase))
                    {
                        request.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                    }
                }
                
                // Copy body for POST/PUT requests
                if (context.Request.Method == "POST" || context.Request.Method == "PUT")
                {
                    context.Request.EnableBuffering();
                    var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
                    context.Request.Body.Position = 0;
                    request.Content = new StringContent(body, Encoding.UTF8, "application/json");
                }
                
                var response = await httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                context.Response.StatusCode = (int)response.StatusCode;
                context.Response.ContentType = response.Content.Headers.ContentType?.ToString() ?? "application/json";
                await context.Response.WriteAsync(responseContent);
            });

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
