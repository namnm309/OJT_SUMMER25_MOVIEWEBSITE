# 🎬 Cinema City Movie Management System

## 📋 Tổng Quan Dự Án

Cinema City Movie Management System là hệ thống quản lý rạp chiếu phim được xây dựng bằng ASP.NET Core với kiến trúc multi-layer, cung cấp các tính năng quản lý phim, đặt vé, quản lý khuyến mãi và tương tác người dùng.

### 🏗️ Kiến Trúc Hệ Thống

```
📁 ojt_summer25_group2_movie/
├── 🔧 ControllerLayer/          # Web API Controllers
├── 📊 ApplicationLayer/         # Business Logic & Services
├── 🏛️ DomainLayer/             # Entities & Domain Logic
├── 💾 InfrastructureLayer/     # Data Access & Repositories
├── 🎨 UI/                      # Frontend Web Application
└── 🧪 UI2/                     # Alternative UI Implementation
```

### 🛠️ Công Nghệ Sử Dụng

- **Backend**: ASP.NET Core 8.0
- **Database**: PostgreSQL với Entity Framework Core
- **Frontend**: Razor Pages, JavaScript, Bootstrap
- **CSS Framework**: Custom CSS với Bootstrap integration
- **Authentication**: JWT Tokens
- **File Upload**: Cloudinary Image Service
- **Architecture Pattern**: Clean Architecture + Repository Pattern

---

## 🚀 Các Tính Năng Chính

### 1. 🎭 Quản Lý Phim (Movie Management)

#### 📁 Controllers & APIs
- **Backend API**: `/api/v1/movie/*`
- **UI Controllers**: `MoviesController`, `MovieManagement/MoviesController`

#### 🔧 Tính Năng
- ✅ **CRUD Operations**: Tạo, đọc, cập nhật, xóa phim
- ✅ **Advanced Filtering**: Lọc theo thể loại, trạng thái, từ khóa
- ✅ **Pagination**: Phân trang cho danh sách phim
- ✅ **Image Management**: Upload và quản lý hình ảnh phim
- ✅ **Genre Management**: Quản lý thể loại phim
- ✅ **Status Management**: Quản lý trạng thái phim (Chưa có, Sắp chiếu, Đang chiếu, Ngừng chiếu)

#### 🎯 API Endpoints
```http
GET    /api/v1/movie/View                 # Lấy tất cả phim
GET    /api/v1/movie/ViewPagination       # Phân trang
GET    /api/v1/movie/GetById              # Chi tiết phim
GET    /api/v1/movie/Search               # Tìm kiếm phim
GET    /api/v1/movie/GetRecommended       # Phim đề xuất
GET    /api/v1/movie/GetComingSoon        # Phim sắp chiếu
GET    /api/v1/movie/GetNowShowing        # Phim đang chiếu
POST   /api/v1/movie/Create               # Tạo phim mới
PATCH  /api/v1/movie/Update               # Cập nhật phim
PATCH  /api/v1/movie/SetFeatured          # Đánh dấu nổi bật
PATCH  /api/v1/movie/SetRecommended       # Đánh dấu đề xuất
PATCH  /api/v1/movie/UpdateRating         # Cập nhật rating
DELETE /api/v1/movie/Delete               # Xóa phim
```

### 2. 🏠 Trang Chủ (Homepage)

#### 🎨 Thiết Kế UI/UX
- **Hero Section**: Phim nổi bật với background động
- **Recommended Movies**: Carousel phim đề xuất (6 phim)
- **Coming Soon Movies**: Grid phim sắp chiếu (4 phim)
- **Promotions**: Section khuyến mãi với images từ CDN

#### 🔄 Logic Hiển Thị
```csharp
// Phim nổi bật (Hero): IsFeatured = true (5 phim)
var featuredMovies = movies.Where(m => m.IsFeatured).Take(5);

// Phim đề xuất: IsRecommended = true (6 phim)
var recommendedMovies = movies.Where(m => m.IsRecommended).Take(6);

// Phim sắp chiếu: Status = ComingSoon (4 phim)
var comingSoonMovies = movies.Where(m => m.Status == 1).Take(4);
```

### 3. 🎬 Trang Phim (Movies Page)

#### 📱 Tính Năng AJAX Loading
- **Tab "Tất cả phim"**: Pagination với `/api/v1/movie/ViewPagination`
- **Tab "Phim đề xuất"**: Load tất cả với `/api/v1/movie/GetRecommended`
- **Tab "Phim sắp chiếu"**: Load tất cả với `/api/v1/movie/GetComingSoon`
- **Tab "Phim đang chiếu"**: Load tất cả với `/api/v1/movie/GetNowShowing`

#### 🎨 Modern UI Design
- Glass morphism effects
- Dark theme với gradient background
- Movie cards với hover animations
- Search integration
- Loading states và error handling

### 4. 🔍 Tìm Kiếm (Search)

#### 🌐 Multi-Page Integration
- **Header Search**: Có ở tất cả trang
- **Homepage Search**: Hero section
- **Movies Page Search**: Integrated search
- **Dedicated Search Page**: `/Movies/Search`

#### 🔧 Backend Implementation
```csharp
public async Task<IActionResult> SearchMovie(string? keyword)
{
    var movies = string.IsNullOrWhiteSpace(keyword)
        ? await _movieRepo.ListAsync(navigationProperties)
        : await _movieRepo.WhereAsync(
            filter: m => m.Title.Contains(keyword),
            navigationProperties: new[]
            {
                nameof(Movie.MovieImages),
                nameof(Movie.MovieGenres) + "." + nameof(MovieGenre.Genre)
            });

    var result = _mapper.Map<List<MovieResponseDto>>(movies);
    return SuccessResp.Ok(result);
}
```

### 5. 👨‍💼 Quản Lý Dashboard

#### 🎛️ Movie Management Dashboard
- **Header Navigation**: Đồng bộ với trang chính
- **Toggle Controls**: Phim nổi bật, Phim đề xuất
- **Status Management**: Dropdown với enum mapping
- **Rating System**: Update rating cho phim
- **CRUD Operations**: Đầy đủ thao tác quản lý

#### 📊 Admin Dashboard
- **Statistics Overview**: Tổng số phim, trạng thái
- **Quick Actions**: Navigation nhanh
- **Clean Layout**: 2-column responsive design

---

## 🗄️ Cấu Trúc Database

### 📋 Entities Chính

#### 🎬 Movie Entity
```csharp
public class Movie : BaseEntity
{
    public string Title { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? ProductionCompany { get; set; }
    public int RunningTime { get; set; }
    public MovieVersion Version { get; set; }
    public string? Director { get; set; }
    public string? Actors { get; set; }
    public string? Content { get; set; }
    public string? TrailerUrl { get; set; }
    public MovieStatus Status { get; set; }
    
    // New Properties Added
    public bool IsFeatured { get; set; } = false;
    public bool IsRecommended { get; set; } = false;
    public double Rating { get; set; } = 0.0;
    
    // Navigation Properties
    public virtual ICollection<MovieGenre> MovieGenres { get; set; }
    public virtual ICollection<MovieImage> MovieImages { get; set; }
    public virtual ICollection<ShowTime> ShowTimes { get; set; }
}
```

#### 🏷️ Enums
```csharp
public enum MovieStatus
{
    NotAvailable = 0,    // Chưa có
    ComingSoon = 1,      // Sắp chiếu  
    NowShowing = 2,      // Đang chiếu
    Stopped = 3          // Ngừng chiếu
}

public enum MovieVersion
{
    TwoD = 1,
    ThreeD = 2,
    FourDX = 3
}
```

### 🔄 Migrations Đã Thêm
```bash
20250623143747_AddMovieRecommendedFeaturedRating.cs
20250624163558_AddMovieDisplayColumns.cs
20250624170516_UpdateMovieWithNewAttributes.cs
20250624181942_AddMovieNewColumns.cs
20250624185705_AddMovieFeaturedRecommendedRating.cs
```

---

## 🎨 Frontend Architecture

### 📱 Responsive Design System

#### 🎯 CSS Structure
```
📁 wwwroot/css/
├── 🏠 HomePage/
│   ├── base.css              # Core styles & variables
│   ├── header.css            # Navigation header
│   ├── hero.css              # Hero section styles
│   ├── moviecard.css         # Movie card components
│   ├── button.css            # Button styles
│   ├── layout.css            # Layout utilities
│   ├── responsive.css        # Media queries
│   └── custom-sections.css   # Custom components
├── 🎬 movie-management.css   # Admin dashboard styles
└── 📱 modern-nav.css         # Modern navigation
```

#### 🌈 Design System
- **Colors**: Dark theme với Cinema red accent (#e50914)
- **Typography**: Inter font family
- **Effects**: Glass morphism, gradients, smooth animations
- **Layout**: CSS Grid + Flexbox
- **Responsive**: Mobile-first approach

### 🔧 JavaScript Features

#### ⚡ AJAX Implementation
```javascript
// Movie filtering với AJAX
async function loadMoviesByFilter(filter, page = 1) {
    try {
        const response = await fetch(`/Movies/GetMoviesByFilter?filter=${filter}&page=${page}`);
        const result = await response.json();
        
        if (result.success) {
            renderMovies(result.data);
            if (result.pagination) {
                renderPagination(result.pagination);
            }
        }
    } catch (error) {
        console.error('Error loading movies:', error);
    }
}
```

#### 🔍 Search Integration
```javascript
// Universal search function
function searchMovies() {
    const keyword = document.getElementById('searchInput').value.trim();
    const searchUrl = '/Movies/Search' + (keyword ? `?keyword=${encodeURIComponent(keyword)}` : '');
    window.location.href = searchUrl;
}
```

---

## 🔧 Chi Tiết Implementation

### 1. 🎯 Movie Homepage Logic Enhancement

#### ❌ Logic Cũ (Sai)
```csharp
// Phim đề xuất: movies.Take(6) - Random
// Phim sắp chiếu: Status = 1 || ReleaseDate > DateTime.Now - Sai logic
```

#### ✅ Logic Mới (Đúng)
```csharp
// Phim đề xuất: IsRecommended = true (6 phim)
viewModel.RecommendedMovies = movies
    .Where(m => m.IsRecommended)
    .Take(6)
    .ToList();

// Phim sắp chiếu: Status = ComingSoon (4 phim)
viewModel.ComingSoonMovies = movies
    .Where(m => m.Status == 1) // ComingSoon enum
    .Take(4)
    .ToList();

// Phim nổi bật: IsFeatured = true (5 phim cho hero)
var featuredMovies = movies.Where(m => m.IsFeatured).ToList();
viewModel.HeroMovies = featuredMovies.Any() ? featuredMovies.Take(5).ToList() : movies.Take(5).ToList();
```

### 2. 🔄 Genre Mapping Transformation

#### ❌ Cấu Trúc Cũ
```csharp
public List<string> Genres { get; set; } // Simple string list
```

#### ✅ Cấu Trúc Mới
```csharp
public List<GenreViewModel> Genres { get; set; } // Full object with ID, Name, Description

public class GenreViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
```

#### 🔧 Mapping Implementation
```csharp
// Backend: GenreDto -> Frontend: GenreViewModel
private List<GenreViewModel> MapGenres(JsonElement movieJson)
{
    var genres = new List<GenreViewModel>();
    
    if (movieJson.TryGetProperty("genres", out var genresProp))
    {
        foreach (var genreElement in genresProp.EnumerateArray())
        {
            var genre = new GenreViewModel
            {
                Id = genreElement.GetProperty("id").GetGuid().ToString(),
                Name = genreElement.GetProperty("name").GetString() ?? string.Empty,
                Description = genreElement.GetProperty("description").GetString()
            };
            genres.Add(genre);
        }
    }
    
    return genres;
}
```

### 3. 🎨 Header Navigation Unification

#### 🔧 Vấn đề Đã Giải Quyết
- **Inconsistent URLs**: Các trang có navigation links khác nhau
- **Missing Features**: Search form không hoạt động
- **Layout Issues**: Header spacing và design

#### ✅ Giải Pháp
```html
<!-- Unified Header Navigation -->
<nav class="navbar">
    <div class="nav-brand">
        <a href="/">Cinema City</a>
    </div>
    <div class="nav-links">
        <a href="/">Trang chủ</a>
        <a href="/Events">Sự kiện</a>
        <a href="/Products">Sản phẩm</a>
        <a href="/Movies">Phim</a>
    </div>
    <div class="nav-search">
        <input type="text" placeholder="Tìm kiếm phim...">
        <button onclick="searchMovies()">Search</button>
    </div>
</nav>
```

### 4. 📱 Movies Page AJAX Redesign

#### 🔄 Từ Server-Side Sang Client-Side
```javascript
// Old: Server-side rendering với full page reload
// New: AJAX loading với smooth transitions

document.addEventListener('DOMContentLoaded', function() {
    loadMoviesByFilter('all', 1); // Load initial data
});

function switchTab(filter) {
    // Update active tab
    document.querySelectorAll('.filter-tab').forEach(tab => {
        tab.classList.remove('active');
    });
    event.target.classList.add('active');
    
    // Load movies for filter
    loadMoviesByFilter(filter);
}
```

### 5. 🎛️ Admin Dashboard Cleanup

#### ❌ Vấn Đề Cũ
- Layout 3-column không cân đối
- Button thừa "Quản lý khuyến mãi"
- Inconsistent styling

#### ✅ Cải Thiện
```html
<!-- Cleaned Layout -->
<div class="row">
    <div class="col-md-6">
        <div class="quick-action-card">
            <h5>Quản lý phim</h5>
            <a href="/MovieManagement/Movies" class="btn btn-primary">
                <i class="fas fa-film"></i> Quản lý phim
            </a>
        </div>
    </div>
    <div class="col-md-6">
        <div class="quick-action-card">
            <h5>Quản lý người dùng</h5>
            <a href="/UserManagement/Members" class="btn btn-success">
                <i class="fas fa-users"></i> Quản lý thành viên
            </a>
        </div>
    </div>
</div>
```

---

## 🛠️ Setup & Installation

### 📋 Prerequisites
- .NET 8.0 SDK
- PostgreSQL 13+
- Node.js (cho frontend build tools - optional)
- Visual Studio 2022 hoặc VS Code

### 🚀 Installation Steps

#### 1. Clone Repository
```bash
git clone <repository-url>
cd ojt_summer25_group2_movie
```

#### 2. Database Setup
```bash
# Cập nhật connection string trong appsettings.json
# ControllerLayer/appsettings.json
# UI/appsettings.json

# Run migrations
cd InfrastructureLayer
dotnet ef database update --startup-project ../ControllerLayer
```

#### 3. Build & Run Backend API
```bash
cd ControllerLayer
dotnet restore
dotnet build
dotnet run  # Runs on https://localhost:5273
```

#### 4. Build & Run Frontend
```bash
cd UI
dotnet restore
dotnet build
dotnet run  # Runs on https://localhost:7069
```

### 🔧 Configuration

#### 📝 ControllerLayer/appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=cinema_city;Username=postgres;Password=your_password"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

#### 📝 UI/appsettings.json
```json
{
  "ApiSettings": {
    "BaseUrl": "https://localhost:5273"
  },
  "CloudinarySettings": {
    "CloudName": "your_cloud_name",
    "ApiKey": "your_api_key",
    "ApiSecret": "your_api_secret"
  }
}
```

---

## 🧪 Testing & Debug

### 🔍 API Testing
```bash
# Test movie endpoints
GET https://localhost:5273/api/v1/movie/View
GET https://localhost:5273/api/v1/movie/GetRecommended
GET https://localhost:5273/api/v1/movie/Search?keyword=avengers
```

### 🎬 Frontend Testing
- **Homepage**: Check hero movies, recommended, coming soon
- **Movies Page**: Test all filter tabs và pagination
- **Search**: Test search từ header và dedicated page
- **Admin**: Test toggle buttons cho featured/recommended

### 📊 Common Issues & Solutions

#### ❌ Build Errors
```bash
# Nullable reference warnings - Safe to ignore
# Missing navigation properties - Check Include statements
# JSON parsing errors - Verify API response structure
```

#### 🔧 Runtime Issues
```bash
# API connection failed - Check ControllerLayer is running
# Database connection - Verify PostgreSQL và connection string
# Image upload failed - Check Cloudinary credentials
```

---

## 📝 Chi Tiết Thay Đổi Gần Đây

### 🔧 Session Development Log

#### 1. 🏗️ Build Check & Header Issues (Fixed)
**Vấn đề**: Build warnings và header navigation không đúng
**Giải pháp**:
- ✅ Build thành công với 28 warnings (chỉ nullable warnings)
- ✅ Sửa keyframes animations trong JavaScript
- ✅ Cập nhật tất cả navigation links trong header

#### 2. 🧭 Navigation Links Standardization
**Thay đổi**:
```html
<!-- Before: Inconsistent URLs -->
<a href="/Home">Trang chủ</a>
<a href="/Event">Sự kiện</a>

<!-- After: Standardized URLs -->
<a href="/">Trang chủ</a>
<a href="/Events">Sự kiện</a>
<a href="/Products">Sản phẩm</a>
<a href="/Movies">Phim</a>
```

#### 3. 🎬 Homepage Movie Logic Correction
**Vấn đề**: Logic hiển thị phim không đúng business requirement
**Trước**:
```csharp
// Sai logic
var recommendedMovies = movies.Take(6); // Random
var comingSoonMovies = movies.Where(m => m.Status == 1 || m.ReleaseDate > DateTime.Now); // Sai
```

**Sau**:
```csharp
// Đúng logic theo yêu cầu
var recommendedMovies = movies.Where(m => m.IsRecommended == true).Take(6);
var comingSoonMovies = movies.Where(m => m.Status == 1).Take(4); // ComingSoon enum
var featuredMovies = movies.Where(m => m.IsFeatured == true).Take(5);
```

#### 4. 🎛️ Movie Management Dashboard Enhancement
**Cải tiến**:
- ✅ Header đồng bộ với trang chính (thay thế header phức tạp)
- ✅ Bỏ button "Quản lý khuyến mãi" thừa
- ✅ Layout từ 3-column → 2-column (col-md-6)
- ✅ Breadcrumb navigation chuẩn
- ✅ Spacing issues resolved (padding-top: 100px → 0)

#### 5. 📱 Movies Page AJAX Implementation
**Transformation từ server-side sang client-side**:

**API Mapping**:
```javascript
// Tab logic mới
const apiEndpoints = {
    'all': '/api/v1/movie/ViewPagination?page={page}&pageSize=12', // Có pagination
    'recommended': '/api/v1/movie/GetRecommended',                 // Tất cả phim recommended
    'coming-soon': '/api/v1/movie/GetComingSoon',                  // Tất cả phim sắp chiếu
    'now-showing': '/api/v1/movie/GetNowShowing'                   // Tất cả phim đang chiếu
};
```

**AJAX Loading**:
```javascript
async function loadMoviesByFilter(filter, page = 1) {
    showLoading();
    try {
        const response = await fetch(getApiUrl(filter, page));
        const result = await response.json();
        
        if (result.success) {
            renderMovies(result.data);
            updateStats(result);
            if (filter === 'all' && result.pagination) {
                renderPagination(result.pagination);
            }
        }
    } catch (error) {
        showError('Lỗi khi tải phim');
    } finally {
        hideLoading();
    }
}
```

#### 6. 🔍 Search Functionality Enhancement
**Backend API Improvement**:
```csharp
// Before: Thiếu navigation properties
public async Task<IActionResult> SearchMovie(string? keyword)
{
    var movies = await _movieRepo.WhereAsync(m => m.Title.Contains(keyword));
    var result = _mapper.Map<List<MovieListDto>>(movies); // Thiếu genres, images
    return SuccessResp.Ok(result);
}

// After: Đầy đủ navigation properties
public async Task<IActionResult> SearchMovie(string? keyword)
{
    var movies = string.IsNullOrWhiteSpace(keyword)
        ? await _movieRepo.ListAsync(
            nameof(Movie.MovieImages),
            nameof(Movie.MovieGenres) + "." + nameof(MovieGenre.Genre)
        )
        : await _movieRepo.WhereAsync(
            filter: m => m.Title.Contains(keyword),
            navigationProperties: new[]
            {
                nameof(Movie.MovieImages),
                nameof(Movie.MovieGenres) + "." + nameof(MovieGenre.Genre)
            });

    var result = _mapper.Map<List<MovieResponseDto>>(movies); // Đầy đủ thông tin
    return SuccessResp.Ok(result);
}
```

**Frontend Integration**:
- ✅ Header search hoạt động trên tất cả trang
- ✅ SearchResults.cshtml có header navigation đầy đủ
- ✅ Modern UI với glass morphism effects
- ✅ Responsive design và error handling

#### 7. 🎨 Genre Mapping System Overhaul
**Transformation**: `List<string>` → `List<GenreViewModel>`

**Backend Changes**:
```csharp
// ApplicationLayer/DTO/MovieManagement/MovieResponseDto.cs
public List<GenreDto> Genres { get; set; } = new(); // Backend structure

// UI/Models/MovieViewModel.cs  
public List<GenreViewModel> Genres { get; set; } = new(); // Frontend structure

public class GenreViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
```

**Mapping Logic**:
```csharp
// HomeController.cs - MapGenres method
private List<GenreViewModel> MapGenres(JsonElement movieJson)
{
    var genres = new List<GenreViewModel>();
    
    if (movieJson.TryGetProperty("genres", out var genresProp))
    {
        foreach (var genreElement in genresProp.EnumerateArray())
        {
            var genre = new GenreViewModel
            {
                Id = genreElement.TryGetProperty("id", out var idProp) 
                    ? idProp.GetGuid().ToString() 
                    : Guid.NewGuid().ToString(),
                Name = genreElement.TryGetProperty("name", out var nameProp) 
                    ? nameProp.GetString() ?? string.Empty 
                    : string.Empty,
                Description = genreElement.TryGetProperty("description", out var descProp) 
                    ? descProp.GetString() 
                    : null
            };
            
            if (!string.IsNullOrEmpty(genre.Name))
            {
                genres.Add(genre);
            }
        }
    }
    
    return genres;
}
```

**View Updates**:
```html
<!-- Before -->
@(movie.Genres.First())

<!-- After -->
@(movie.Genres.First().Name)

<!-- JavaScript -->
// Before
movie.genres.join(', ')

// After  
movie.genres.map(g => g.name).join(', ')
```

#### 8. 🌐 Backend API Extensions
**New Endpoints Added**:
```csharp
// MovieController.cs
[HttpGet("GetRecommended")]
public async Task<IActionResult> GetRecommended()
{
    return await _movieService.GetRecommended();
}

[HttpGet("GetComingSoon")]
public async Task<IActionResult> GetComingSoon()
{
    return await _movieService.GetComingSoon();
}

[HttpGet("GetNowShowing")]
public async Task<IActionResult> GetNowShowing()
{
    return await _movieService.GetNowShowing();
}
```

**MovieService Implementation**:
```csharp
public async Task<IActionResult> GetRecommended()
{
    var movies = await _movieRepo.WhereAsync(
        filter: m => m.IsRecommended == true,
        orderBy: q => q.OrderByDescending(m => m.CreatedAt),
        navigationProperties: new[]
        {
            nameof(Movie.MovieImages),
            nameof(Movie.MovieGenres) + "." + nameof(MovieGenre.Genre)
        });

    var result = _mapper.Map<List<MovieResponseDto>>(movies);
    return SuccessResp.Ok(result);
}
```

#### 9. 🎭 Status Mapping Correction
**Fixed Enum Mapping**:
```csharp
public enum MovieStatus
{
    NotAvailable = 0,  // Chưa có (trước đây map sai)
    ComingSoon = 1,    // Sắp chiếu  
    NowShowing = 2,    // Đang chiếu
    Stopped = 3        // Ngừng chiếu
}
```

**Dropdown Options**:
```html
<option value="0">Chưa có</option>
<option value="1">Sắp chiếu</option>
<option value="2">Đang chiếu</option>
<option value="3">Ngừng chiếu</option>
```

#### 10. 🎨 UI/UX Enhancements
**Movies Page Redesign**:
- ✅ Header integration từ `_Header.cshtml`
- ✅ Dark theme với gradient background
- ✅ Glass morphism effects và modern cards
- ✅ Hero statistics section
- ✅ Smooth filter transitions
- ✅ Loading states và error handling
- ✅ Responsive design cho mobile

**SearchResults Page Enhancement**:
- ✅ Complete layout overhaul với header/footer
- ✅ Modern search interface
- ✅ Movie grid với hover effects
- ✅ Empty state handling
- ✅ Consistent styling với homepage

#### 11. 🚀 Performance & Error Handling
**AJAX Error Handling**:
```javascript
async function loadMoviesByFilter(filter, page = 1) {
    try {
        showLoadingState();
        const response = await fetch(apiUrl);
        
        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }
        
        const result = await response.json();
        
        if (result.success) {
            renderMovies(result.data);
        } else {
            throw new Error(result.message || 'API returned error');
        }
    } catch (error) {
        console.error('Error loading movies:', error);
        showErrorState(error.message);
    } finally {
        hideLoadingState();
    }
}
```

**Backend Logging**:
```csharp
_logger.LogInformation("✅ USING API DATA - Loaded {Count} movies", movies.Count);
_logger.LogInformation("🎬 Featured movies: {Titles}", string.Join(", ", featuredMovies.Select(m => m.Title)));
_logger.LogError("❌ API Error: {Message}", result.Message);
```

### 📈 Impact & Results

#### ✅ Achievements
1. **Consistent Navigation**: Tất cả trang có header navigation đồng nhất
2. **Proper Business Logic**: Homepage hiển thị đúng phim theo yêu cầu
3. **Modern UI/UX**: Glass morphism, dark theme, responsive design
4. **Complete Search**: Search hoạt động trên tất cả trang với header đầy đủ
5. **AJAX Performance**: Movies page load nhanh không reload trang
6. **Data Integrity**: Genre mapping đúng cấu trúc object
7. **API Completeness**: Đầy đủ endpoints cho tất cả filter types

#### 📊 Statistics
- **Build Status**: ✅ Success với 27-40 warnings (chỉ nullable)
- **API Endpoints**: 4 endpoints mới cho movie filtering
- **Frontend Pages**: 6 trang được cập nhật UI/UX
- **Database**: 5 migrations cho movie properties mới
- **CSS Files**: 10+ files được organize theo component structure

---

## 📈 Future Enhancements

### 🎯 Planned Features
- [ ] **Booking System**: Đặt vé online
- [ ] **Payment Integration**: VNPay, MoMo
- [ ] **Email Notifications**: Xác nhận đặt vé
- [ ] **Mobile App**: React Native
- [ ] **Analytics Dashboard**: Reports và statistics
- [ ] **Social Features**: Reviews, ratings, sharing

### 🚀 Technical Improvements
- [ ] **Performance**: Redis caching
- [ ] **Security**: Enhanced authentication
- [ ] **Testing**: Unit và integration tests
- [ ] **CI/CD**: GitHub Actions pipeline
- [ ] **Documentation**: Swagger API docs
- [ ] **Monitoring**: Application insights

---

## 👥 Team & Contributors

### 🏗️ Project Structure
- **Backend Team**: API development, database design
- **Frontend Team**: UI/UX design, responsive layouts
- **DevOps Team**: Deployment, server management
- **QA Team**: Testing, bug reports

### 📚 Development Guidelines
- **Coding Standards**: C# conventions, clean code
- **Git Workflow**: Feature branches, pull requests
- **Code Review**: Peer review before merge
- **Documentation**: README updates, code comments

---

## 📄 License & Contact

### 📞 Support
- **Email**: support@cinemacity.com
- **GitHub Issues**: [Create Issue](link-to-issues)
- **Documentation**: [Wiki](link-to-wiki)

### 🏢 Company
**Cinema City Development Team**  
OJT Summer 2025 - Group 2  
Movie Management System Project

---

*⭐ Nếu project này hữu ích, hãy star repository và chia sẻ với team khác!*

**🎬 Happy Coding! 🍿**
GET https://localhost:5273/api/v1/movie/Search?keyword=avengers
```

### 🎬 Frontend Testing
- **Homepage**: Check hero movies, recommended, coming soon
- **Movies Page**: Test all filter tabs và pagination
- **Search**: Test search từ header và dedicated page
- **Admin**: Test toggle buttons cho featured/recommended

### 📊 Common Issues & Solutions

#### ❌ Build Errors
```bash
# Nullable reference warnings - Safe to ignore
# Missing navigation properties - Check Include statements
# JSON parsing errors - Verify API response structure
```

#### 🔧 Runtime Issues
```bash
# API connection failed - Check ControllerLayer is running
# Database connection - Verify PostgreSQL và connection string
# Image upload failed - Check Cloudinary credentials
```

---

## 📈 Future Enhancements

### 🎯 Planned Features
- [ ] **Booking System**: Đặt vé online
- [ ] **Payment Integration**: VNPay, MoMo
- [ ] **Email Notifications**: Xác nhận đặt vé
- [ ] **Mobile App**: React Native
- [ ] **Analytics Dashboard**: Reports và statistics
- [ ] **Social Features**: Reviews, ratings, sharing

### 🚀 Technical Improvements
- [ ] **Performance**: Redis caching
- [ ] **Security**: Enhanced authentication
- [ ] **Testing**: Unit và integration tests
- [ ] **CI/CD**: GitHub Actions pipeline
- [ ] **Documentation**: Swagger API docs
- [ ] **Monitoring**: Application insights

---

## 👥 Team & Contributors

### 🏗️ Project Structure
- **Backend Team**: API development, database design
- **Frontend Team**: UI/UX design, responsive layouts
- **DevOps Team**: Deployment, server management
- **QA Team**: Testing, bug reports

### 📚 Development Guidelines
- **Coding Standards**: C# conventions, clean code
- **Git Workflow**: Feature branches, pull requests
- **Code Review**: Peer review before merge
- **Documentation**: README updates, code comments

---

## 📄 License & Contact

### 📞 Support
- **Email**: support@cinemacity.com
- **GitHub Issues**: [Create Issue](link-to-issues)
- **Documentation**: [Wiki](link-to-wiki)

### 🏢 Company
**Cinema City Development Team**  
OJT Summer 2025 - Group 2  
Movie Management System Project

---

*⭐ Nếu project này hữu ích, hãy star repository và chia sẻ với team khác!*

**🎬 Happy Coding! 🍿**
