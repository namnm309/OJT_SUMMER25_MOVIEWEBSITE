## OJT Summer 2025 – Movie Website (.NET 8, ASP.NET Core)

Dự án website đặt vé xem phim gồm API (Web API) và UI (ASP.NET Core MVC/Razor), kiến trúc nhiều tầng rõ ràng, hỗ trợ đồng bộ ghế thời gian thực bằng SignalR, thanh toán VNPay (sandbox), Redis cache, Email SMTP, và Cloudinary để lưu ảnh.

### 1) Kiến trúc dự án
- `DomainLayer`: Domain models (Entities), `Enum`, `Exceptions`, `Constants` – thuần business domain.
- `InfrastructureLayer`: Truy cập dữ liệu (EF Core `MovieContext`), Migrations, Repository pattern, Core services (JWT, Crypto, Mail, Cache/Redis).
- `ApplicationLayer`: DTOs, Services (User/Movie/Showtime/Booking/Promotion/CinemaRoom/Concession/Payment/Ticket), AutoMapper Profiles, Background job cleanup, SignalR Hub.
- `ControllerLayer`: Dự án API (.NET 8, Swagger, JWT auth, CORS, DI), đăng ký services và seed dữ liệu mẫu khi khởi động.
- `UI`: Dự án Web MVC/Razor (.NET 8), gọi API qua `HttpClient`, lưu JWT trong Session, phân quyền UI theo role, upload ảnh với Cloudinary.
- Solution: `MovieWebApp.sln` (root), ngoài ra có `UI/UI.sln` phục vụ riêng UI khi cần.

### 2) Công nghệ chính
- Run-time: **.NET 8**
- Web: ASP.NET Core Web API, MVC/Razor
- Data: **EF Core (PostgreSQL Npgsql)**, Migrations nằm ở `InfrastructureLayer`
- Cache/Realtime: **Redis** (StackExchange.Redis), **SignalR** (đồng bộ ghế)
- Auth: **JWT** (+ cookie ở một số chỗ), Role-based
- Mapping: **AutoMapper**
- Mail: **MailKit/MimeKit** (SMTP)
- Media: **Cloudinary** (ảnh)
- Payment: **VNPay** (sandbox)

### 3) Yêu cầu hệ thống
- .NET SDK 8.0+
- PostgreSQL 13+ (khuyến nghị 15+)
- Redis (bắt buộc vì API khởi tạo `IConnectionMultiplexer`)
- Docker Desktop (tùy chọn, để chạy Postgres/Redis/containers)

### 4) Cấu hình & Biến môi trường
Các cấu hình chính (đặt trong `appsettings.json` hoặc biến môi trường/User Secrets):

- API (`ControllerLayer/appsettings.json`)
  - `ConnectionStrings:DefaultConnection` – PostgreSQL
  - `ConnectionStrings:RedisConnection` – Redis
  - `SMTPEmail`, `SMTPPassword` – tài khoản gửi mail
  - `Jwt:Secret` – secret ký JWT (có default, nên override)
  - `Vnpay:{TmnCode, HashSecret, BaseUrl, returnUrl}` – cấu hình VNPay

- UI (`UI/appsettings.json`)
  - `ApiSettings:BaseUrl` – URL API (mặc định `https://localhost:7049`)
  - `Jwt:Secret` – secret để UI validate token
  - `Cloudinary:{CloudName, ApiKey, ApiSecret}`

Khuyến nghị KHÔNG commit secrets thực. Dùng User Secrets hoặc ENV.

Thiết lập nhanh bằng User Secrets (Windows PowerShell):

```powershell
# API (ControllerLayer)
cd ControllerLayer
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=movieapp;Username=postgres;Password=postgres"
dotnet user-secrets set "ConnectionStrings:RedisConnection" "localhost:6379"
dotnet user-secrets set "Jwt:Secret" "CHANGE_ME_TO_A_LONG_RANDOM_SECRET"
dotnet user-secrets set "SMTPEmail" "your_gmail@gmail.com"
dotnet user-secrets set "SMTPPassword" "your_app_password"
dotnet user-secrets set "Vnpay:TmnCode" "..." 
dotnet user-secrets set "Vnpay:HashSecret" "..."
dotnet user-secrets set "Vnpay:BaseUrl" "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html"
dotnet user-secrets set "Vnpay:returnUrl" "https://localhost:7049/api/v1/payment/vnpay-return"

# UI
cd ../UI
dotnet user-secrets init
dotnet user-secrets set "ApiSettings:BaseUrl" "https://localhost:7049"
dotnet user-secrets set "Jwt:Secret" "SAME_OR_COMPATIBLE_SECRET_WITH_API"
dotnet user-secrets set "Cloudinary:CloudName" "..."
dotnet user-secrets set "Cloudinary:ApiKey" "..."
dotnet user-secrets set "Cloudinary:ApiSecret" "..."
```

Thiết lập bằng biến môi trường (ví dụ khi chạy Docker):
- `ConnectionStrings__DefaultConnection`
- `ConnectionStrings__RedisConnection`
- `Jwt__Secret`
- `SMTPEmail`, `SMTPPassword`
- `Vnpay__TmnCode`, `Vnpay__HashSecret`, `Vnpay__BaseUrl`, `Vnpay__returnUrl`
- UI: `ApiSettings__BaseUrl`, `Cloudinary__CloudName`, `Cloudinary__ApiKey`, `Cloudinary__ApiSecret`

### 5) Cơ sở dữ liệu (EF Core)
- DbContext: `InfrastructureLayer/Data/MovieContext.cs`
- Migrations: `InfrastructureLayer/Migrations`
- Factory: `InfrastructureLayer/Data/MovieContextFactory.cs` (đọc config từ `ControllerLayer/appsettings.json` khi design-time)

Lệnh EF Core (chạy từ root hoặc repo):

```powershell
# Cài đặt công cụ nếu chưa có
dotnet tool install --global dotnet-ef

# Áp dụng migrations để tạo DB
dotnet ef database update --project InfrastructureLayer --startup-project ControllerLayer

# Tạo migration mới (nếu cần)
dotnet ef migrations add <MigrationName> --project InfrastructureLayer --startup-project ControllerLayer
```

Seeder khởi tạo:
- Tài khoản Admin mặc định: `admin` / `admin123`
- Người dùng mẫu (staff/member) + dữ liệu phòng/ghế/phim/lịch chiếu (xem `InfrastructureLayer/Data/DataSeeder.cs`)

### 6) Chạy dịch vụ phụ (Docker – tùy chọn)
Chạy PostgreSQL và Redis nhanh bằng Docker (Windows):

```powershell
# PostgreSQL
docker run -d --name pg-movie -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=movieapp -p 5432:5432 postgres:15

# Redis
docker run -d --name redis-movie -p 6379:6379 redis:7
```

### 7) Chạy dự án (Local)
Ports mặc định (từ `launchSettings.json`):
- API: `http://localhost:5274`, `https://localhost:7049` (Swagger: `/swagger`)
- UI:  `http://localhost:5073`, `https://localhost:7069`

Chạy bằng CLI:

```powershell
# 1) API (yêu cầu Postgres + Redis đã chạy)
dotnet run --project ControllerLayer

# 2) UI
dotnet run --project UI
```

Hoặc mở solution `MovieWebApp.sln` trong Visual Studio và Start cả 2 projects.

First run checklist:
- Đảm bảo PostgreSQL và Redis đã chạy, chuỗi kết nối đúng
- `dotnet ef database update` để tạo DB/tables
- Start API trước, kiểm tra `https://localhost:7049/swagger`
- Start UI, truy cập `https://localhost:7069`
- Đăng nhập `admin/admin123` để vào dashboard

### 8) API endpoints (chính)
Xem chi tiết và test trực tiếp tại Swagger: `https://localhost:7049/swagger`

- Auth (`/api/v1/Auth`)
  - POST `Register-User`, `Register-Admin` – đăng ký
  - POST `Login` – đăng nhập lấy token
  - GET `View` – xem thông tin user (yêu cầu JWT)
  - PATCH `Edit` – sửa hồ sơ (yêu cầu JWT)
  - POST `Logout` – đăng xuất
  - POST `Register-Email`, `Forgot-Password`, `Verify-ChangePassword`

- User (`/api/user`)
  - POST `login`, `register`, `logout`
  - GET `profile` (auth), PUT `profile` (auth)
  - GET `members`, `count`, `growth`
  - Admin: POST `/`, PATCH `/{id}`, DELETE `/{id}`, PATCH `/{id}/status`, GET `/{id}`

- Movie (`/api/v1/movie`)
  - GET `View`, `ViewPagination`, `GetById`, `Search`, `count`, `growth`
  - GET `GetRecommended`, `GetComingSoon`, `GetNowShowing`
  - PATCH `SetFeatured`, `SetRecommended`, `UpdateRating` (auth)
  - Genre: GET `ViewGenre`, POST `CreateGenre`, PATCH `ChangeStatusGenre` (auth)
  - CRUD: POST `Create`, PATCH `Update`, DELETE `Delete` (auth)

- Showtime (`/api/v1/showtime`)
  - GET all, GET `{id}`, GET `GetByMonth`
  - POST `create-new`, POST (create), PUT `{id}` (update), DELETE `{id}`
  - GET dropdown: `movies-dropdown`, `cinema-rooms-dropdown`

- Booking (`/api/v1/booking-ticket`)
  - Dropdown: GET `dropdown/movies`, `dropdown/movies/{movieId}/dates`, `dropdown/movies/{movieId}/times`
  - Seats: GET `available` (auth), POST `validate` (auth), GET `showtime/{showTimeId}/details`
  - Confirm: POST `confirm-user-booking`, `confirm-user-booking-v2`, `confirm-booking-with-score`
  - Member: POST `check-member`, `create-member-account`
  - Admin: POST `confirm-Admin-booking`
  - Booking info: GET `booking/{bookingCode}`, `booking-id/{bookingCode}`
  - List/Status: GET `bookings`, PUT `booking/{bookingId}/status`, POST `booking/{bookingId}/cancel`
  - My bookings: GET `user-bookings-count`, `user-bookings` (auth)
  - Debug: `debug/showtimes`, `debug/test-showtimes/{movieId}`

- Payment (`/api/v1/payment`)
  - POST `vnpay` (auth) – tạo thanh toán VNPay
  - GET `vnpay-return` – callback/redirect từ VNPay

- Ticket (`/api/v1/ticket`)
  - GET `booking?bookingId=...`, GET `User`, GET `{ticketId}` (auth)

- Promotions (`/api/v1/promotions`)
  - GET all, GET `{id}`
  - POST (create), PUT (update), DELETE `{id}`
  - POST `save-user-promotion` (auth), GET `my` (auth), POST `redeem/{userPromotionId}` (auth)

- Cinema Room (`/api/v1/cinemaroom`)
  - GET `ViewRoom`, `ViewRoomPagination`, `ViewSeat?Id=...`, `search`
  - POST `Add`, PATCH `Update/{id}`, DELETE `Delete/{id}`
  - Admin seats views: `rooms-True/{roomId}/seats`, `rooms-False/{roomId}/seats`
  - Seats CRUD: GET `{roomId}/seats`, PUT `{roomId}/seats/{seatId}`
  - POST `{roomId}/update-all-prices`
  - POST `migration/add-layout-columns` (thao tác DB trực tiếp – cân nhắc quyền hạn)

- Concession Items (`/api/ConcessionItems`)
  - GET all, GET `paginated`, GET `{id}`, GET `active`
  - POST (create), PUT `{id}` (update), DELETE `{id}`

- Seat Signal (`/api/seatsignal`)
  - POST `hold` (auth), POST `summary` (auth), DELETE `release/{seatLogId}` (auth)

### 9) Realtime đặt ghế (SignalR)
- Hub endpoint: `/seatHub`
- Kết nối kèm query `showTimeId` để join group theo suất chiếu
- JWT có thể truyền qua query `access_token`

### 10) Docker hoá (tùy chọn)
Build & run containers rời (yêu cầu DB/Redis ngoài container):

```powershell
# Build
docker build -t movie-api ./ControllerLayer
docker build -t movie-ui ./UI

# Run API (container lắng nghe :8080/:8081)
docker run -d --name movie-api -p 5274:8080 -p 7049:8081 ^
  -e ConnectionStrings__DefaultConnection="Host=host.docker.internal;Port=5432;Database=movieapp;Username=postgres;Password=postgres" ^
  -e ConnectionStrings__RedisConnection="host.docker.internal:6379" ^
  -e Jwt__Secret="CHANGE_ME" ^
  movie-api

# Run UI
docker run -d --name movie-ui -p 5073:8080 -p 7069:8081 ^
  -e ApiSettings__BaseUrl="https://host.docker.internal:7049" ^
  movie-ui
```

Gợi ý `docker-compose.yml` (tham khảo, tự tạo file nếu muốn):

```yaml
version: "3.8"
services:
  postgres:
    image: postgres:15
    environment:
      POSTGRES_PASSWORD: postgres
      POSTGRES_DB: movieapp
    ports: ["5432:5432"]
  redis:
    image: redis:7
    ports: ["6379:6379"]
  api:
    build: ./ControllerLayer
    depends_on: [postgres, redis]
    environment:
      ConnectionStrings__DefaultConnection: Host=postgres;Port=5432;Database=movieapp;Username=postgres;Password=postgres
      ConnectionStrings__RedisConnection: redis:6379
      Jwt__Secret: CHANGE_ME
    ports: ["5274:8080", "7049:8081"]
  ui:
    build: ./UI
    depends_on: [api]
    environment:
      ApiSettings__BaseUrl: https://api:8081
    ports: ["5073:8080", "7069:8081"]
```

### 11) Test
Chạy toàn bộ test:

```powershell
dotnet test
```

### 12) Lưu ý bảo mật & cấu hình
- Không commit secrets thật trong `appsettings.json`
- Dùng User Secrets/ENV khi dev và production
- Đổi `Jwt:Secret` trước khi deploy
- Xem `PaymentController` để cấu hình redirect UI sau VNPay: hiện có base URL UI tạm thời `https://localhost:7069` – nên chuyển thành cấu hình (ví dụ `UIBaseUrl`) khi triển khai

### 13) Tài khoản mẫu (Seeder)
- Admin: `admin` / `admin123`
- Staff: `staff001` / `password123`
- Member: `member001` / `password123`
- Member: `member002` / `password123`

### 14) Đóng góp
- Giữ code rõ ràng, tuân quy tắc đặt tên và conventions hiện tại
- Bổ sung/điều chỉnh test khi thay đổi logic
- PR luôn mô tả gọn, đính kèm ảnh (nếu là UI), và kế hoạch rollout

## OJT Summer 2025 – Movie Website (ASP.NET Core)

Dự án website đặt vé xem phim gồm API và UI, xây dựng theo kiến trúc nhiều tầng trên .NET 8.

### Kiến trúc & Thư mục chính
- `DomainLayer`: Entities, Enums, Exceptions, hằng số domain
- `InfrastructureLayer`: EF Core `MovieContext`, Migrations, Repositories, Core services (Cache/Redis, Crypto, JWT, Mail)
- `ApplicationLayer`: DTOs, Services, AutoMapper Profiles, Background jobs, SignalR Hub
- `ControllerLayer` (API): ASP.NET Core Web API, Swagger, DI, CORS, Auth (JWT)
- `UI` (Web): ASP.NET Core MVC/Razor, gọi API, Auth (JWT trong Session), Cloudinary

### Yêu cầu hệ thống
- .NET SDK 8.0+
- PostgreSQL 13+ (khuyến nghị 15+)
- Redis (bắt buộc vì API khởi tạo `IConnectionMultiplexer`)
- Docker (tùy chọn)

### Cấu hình
- API: `ControllerLayer/appsettings.json`
  - `ConnectionStrings:DefaultConnection`: chuỗi kết nối PostgreSQL
  - `ConnectionStrings:RedisConnection`: chuỗi kết nối Redis
  - `SMTPEmail`, `SMTPPassword`: gửi mail
  - `Vnpay:{TmnCode, HashSecret, BaseUrl, returnUrl}`: cổng thanh toán VNPay (sandbox)
  - JWT secret đang đọc từ `Jwt:Secret` (có giá trị mặc định – nên override bằng User Secrets/ENV)
- UI: `UI/appsettings.json`
  - `ApiSettings:BaseUrl`: URL API (mặc định `https://localhost:7049`)
  - `Jwt:Secret`: khóa xác thực (UI dùng để validate token)
  - `Cloudinary:{CloudName, ApiKey, ApiSecret}`: upload ảnh

Khuyến nghị không commit secret thật. Dùng User Secrets (local) hoặc biến môi trường khi triển khai.

### Cơ sở dữ liệu (EF Core)
Migrations nằm ở `InfrastructureLayer/Migrations`, DbContext ở `InfrastructureLayer/Data/MovieContext.cs`.

1) Cài công cụ EF (nếu chưa có):
```bash
dotnet tool install --global dotnet-ef
```

2) Tạo DB và áp dụng migrations:
```bash
dotnet ef database update --project InfrastructureLayer --startup-project ControllerLayer
```

3) (Tùy chọn) Tạo migration mới:
```bash
dotnet ef migrations add <MigrationName> --project InfrastructureLayer --startup-project ControllerLayer
```

Seeder mặc định tạo tài khoản quản trị và dữ liệu mẫu người dùng/phòng/ghế/phim/lịch chiếu khi API khởi động lần đầu.

### Chạy dự án (Local)
Mặc định cổng theo `launchSettings.json`:
- API: HTTP `http://localhost:5274`, HTTPS `https://localhost:7049` (Swagger tại `/swagger`)
- UI: HTTP `http://localhost:5073`, HTTPS `https://localhost:7069`

1) Chạy API (cần PostgreSQL + Redis sẵn sàng):
```bash
dotnet run --project ControllerLayer
```

2) Chạy UI:
```bash
dotnet run --project UI
```

3) Mở trình duyệt:
- API Swagger: `https://localhost:7049/swagger`
- UI: `https://localhost:7069` (hoặc `http://localhost:5073`)

SignalR hub dùng đường dẫn `/seatHub` để đồng bộ ghế theo suất chiếu.

### Tài khoản mẫu (được seed)
- Admin: `admin` / `admin123`
- Staff: `staff001` / `password123`
- Member: `member001` / `password123`
- Member: `member002` / `password123`

Lưu ý: Bạn có thể chỉnh lại hoặc vô hiệu seeding theo nhu cầu.

### Chạy bằng Docker (tùy chọn)
Đã có `Dockerfile` cho cả API và UI. Ví dụ build & chạy nhanh (yêu cầu DB/Redis bên ngoài):

1) Build images:
```bash
docker build -t movie-api ./ControllerLayer
docker build -t movie-ui ./UI
```

2) Run containers (map cổng tương ứng):
```bash
# API lắng nghe 8080/8081 trong container
docker run -d --name movie-api -p 5274:8080 -p 7049:8081 \
  -e ConnectionStrings__DefaultConnection="Host=<host>;Port=5432;Database=<db>;Username=<user>;Password=<pass>" \
  -e ConnectionStrings__RedisConnection="<redis-connection-string>" \
  movie-api

# UI trỏ về API
docker run -d --name movie-ui -p 5073:8080 -p 7069:8081 
  -e ApiSettings__BaseUrl="https://host.docker.internal:7049" \
  movie-ui
```

Bạn có thể dùng `.env` hoặc secret store để truyền cấu hình an toàn hơn.

### Kiểm thử
Có project test tại `ApplicationLayer.Tests`:
```bash
dotnet test
```

### Tính năng chính (tóm tắt)
- Quản lý người dùng, phân quyền (Admin/Staff/Member), JWT Auth
- Danh mục phim, thể loại, đạo diễn, diễn viên, hình ảnh
- Phòng chiếu, ghế, suất chiếu; đặt vé với đồng bộ ghế theo thời gian thực (SignalR)
- Khuyến mãi/voucher, điểm thưởng, bắp nước (concession)
- Thanh toán VNPay (sandbox)
- Giao diện quản trị và người dùng bằng Razor MVC

### Góp ý & Phát triển
Mọi góp ý/PR đều được hoan nghênh. Vui lòng đảm bảo:
- Viết code rõ ràng, tuân theo convention hiện có
- Thêm/ cập nhật test khi thay đổi logic
- Không commit secrets thực vào repo



