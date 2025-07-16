using ApplicationLayer.Services.UserManagement;
using ApplicationLayer.Services.MovieManagement;
using ApplicationLayer.Services.ShowtimeManagement;
using InfrastructureLayer.Data;
using InfrastructureLayer.Repository;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ApplicationLayer.Services.PromotionManagement;
using ApplicationLayer.Services.CinemaRoomManagement;
using ApplicationLayer.Services.BookingTicketManagement;
using ApplicationLayer.Services.EmployeeManagement;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ApplicationLayer.Services.JWT;
using InfrastructureLayer.Core.JWT;
using InfrastructureLayer.Core.Crypto;
using Microsoft.OpenApi.Models;
using InfrastructureLayer.Core.Mail;
using InfrastructureLayer.Core.Cache;
using StackExchange.Redis;
using ApplicationLayer.Mappings;
using ApplicationLayer.Services.Payment;
using ApplicationLayer.Services.TicketSellingManagement;
using ApplicationLayer.Services.Helper;

namespace ControllerLayer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Đăng ký các service và cấu hình ứng dụng

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
                    policy.WithOrigins("https://localhost:7069",
                        "http://localhost:7069",
                        "http://localhost:5073",
                        "https://localhost:5073",
                        "http://localhost:5000",
                        "https://localhost:5001",
                        "http://localhost:5274",
                        "https://localhost:5274")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials(); // Cho phép chia sẻ cookie giữa UI và API
                });
                
                // Chính sách cho API công khai - không yêu cầu xác thực
                options.AddPolicy("PublicAPI", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            // JWT Token
            var jwtSecret = builder.Configuration["Jwt:Secret"]
                ?? "ea8cf10696dc45a8b7b5f15758ae3ef238b440cfa1f84b449af315d515de6f95";
            var key = Encoding.ASCII.GetBytes(jwtSecret);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };
            })
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


            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Movie Cinema", Version = "v1" });

                // Add a bearer token to Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer"
                });

                // Require the bearer token for all API operations
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                      new string[] {}
                  }
                });

                //File upload support
                c.SupportNonNullableReferenceTypes();
            });

            // Cấu hình DbContext
            builder.Services.AddDbContext<MovieContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Đăng ký AutoMapper
            builder.Services.AddAutoMapper(typeof(ApplicationLayer.Mappings.MovieMappingProfile));
            builder.Services.AddAutoMapper(typeof(SeatMappingProfile));
            builder.Services.AddAutoMapper(typeof(ApplicationLayer.Mapper.MappingProfile));

            // Đăng ký Generic Repository Pattern
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            // Đăng ký Redis
            builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("RedisConnection") ?? throw new ArgumentNullException("RedisConnection")));


            // Đăng ký của phần CORE {Cache, Crypto, Jwt, Mail}
            builder.Services.AddSingleton<IJwtService, JwtService>();

            builder.Services.AddSingleton<ICryptoService, CryptoService>();

            builder.Services.AddScoped<ICacheService, CacheService>();

            var smtpUsername = builder.Configuration.GetValue<string>("SMTPEmail") ?? "smtp_email";
            var smtpPassword = builder.Configuration.GetValue<string>("SMTPPassword") ?? "smtp_password";
            builder.Services.AddSingleton<IMailService>(new MailService("smtp.gmail.com", 587, smtpUsername, smtpPassword));

            // Đăng ký Background Job
            builder.Services.AddHostedService<ExpirePendingSeatsJob>();


            // Đăng ký Repository và Services
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IUserService, UserService>();

            builder.Services.AddScoped<IMovieService, MovieService>();
            builder.Services.AddScoped<IMovieRepository, MovieRepository>();

            builder.Services.AddScoped<IPromotionService, PromotionService>();

            builder.Services.AddScoped<ICinemaRoomService, CinemaRoomService>();

            builder.Services.AddScoped<IShowtimeService, ShowtimeService>();

            builder.Services.AddScoped<IPointHistoryService, PointHistoryService>();

            builder.Services.AddScoped<IBookingRepository, BookingRepository>();
            builder.Services.AddScoped<IBookingTicketService, BookingTicketService>();

            builder.Services.AddScoped<ISeatRepository, SeatRepository>();
            builder.Services.AddScoped<ISeatService, SeatService>();

            builder.Services.AddScoped<IAuthService, AuthService>();

            builder.Services.AddScoped<IPaymentService, PaymentService>();

            builder.Services.AddScoped<ITicketService, TicketService>();

            // Thêm vào Program.cs
            builder.Services.AddAutoMapper(typeof(BookingProfile));

            builder.Services.AddScoped<IEmployeeService, EmployeeService>();

            // Actor & Director services
            builder.Services.AddScoped<IActorService, ActorService>();
            builder.Services.AddScoped<IDirectorService, DirectorService>();

            // Tạm comment AuthService vì cần mail service
            // builder.Services.AddScoped<IAuthService, AuthService>();

            // Cấu hình Authentication với Cookie - ĐÃ HỢP NHẤT Ở TRÊN
            // builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            //     .AddCookie(options =>
            //     {
            //         options.LoginPath = "/api/user/login";
            //         options.LogoutPath = "/api/user/logout"; 
            //         options.AccessDeniedPath = "/api/user/access-denied";
            //         options.ExpireTimeSpan = TimeSpan.FromHours(2);
            //         options.SlidingExpiration = true;
            //         options.Cookie.HttpOnly = true;
            //         options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            //     });

            builder.Services.AddAuthorization();

            //
            builder.Services.AddHttpContextAccessor();

            // Thêm vào phần đăng ký services
            builder.Services.AddScoped<ICustomerSearchService, CustomerSearchService>();

            //===================================================================================================================================================

            var app = builder.Build();

            // Đã đăng ký Mail service ở trên rồi

            // Tự động tạo database và khởi tạo dữ liệu mẫu
            using (var scope = app.Services.CreateScope())
            {
                Console.WriteLine("Đang kiểm tra database...");
                var context = scope.ServiceProvider.GetRequiredService<MovieContext>();

                // Áp dụng migration tự động
                Console.WriteLine("Applying pending migrations (if any)...");
                //context.Database.Migrate();
                Console.WriteLine("Database is up-to-date!");

                // Seed dữ liệu admin mặc định
                await DataSeeder.SeedAdminUser(context);
                
                // Seed dữ liệu người dùng mẫu
                await DataSeeder.SeedSampleUsers(context);
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
