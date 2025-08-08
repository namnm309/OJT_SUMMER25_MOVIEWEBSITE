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


