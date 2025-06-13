# 🛠️ Repository & Mapping Documentation

## 📖 Tổng quan

Chúng ta đã implement 2 patterns quan trọng:

1. **🗺️ AutoMapper** - Tự động convert giữa các objects
2. **🗃️ Generic Repository** - Pattern chung cho database operations

---

## 🗺️ AutoMapper Usage

### ✅ Trước khi có AutoMapper:
```csharp
// Manual mapping - phải viết tay từng field
private UserResponseDto MapToUserResponseDto(Users user)
{
    return new UserResponseDto
    {
        UserId = user.UserId,
        Username = user.Username,
        Email = user.Email,
        FullName = user.FullName,
        // ... 15 fields khác
    };
}
```

### 🚀 Sau khi có AutoMapper:
```csharp
// Chỉ 1 dòng!
var userResponse = _mapper.Map<UserResponseDto>(user);

// Convert List cũng đơn giản
var userList = _mapper.Map<List<UserResponseDto>>(users);
```

### 🔧 Cách sử dụng trong Service:

```csharp
public class SomeService
{
    private readonly IMapper _mapper;

    public SomeService(IMapper mapper)
    {
        _mapper = mapper;
    }

    public async Task<UserResponseDto> GetUserAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        // Magic! 🪄
        return _mapper.Map<UserResponseDto>(user);
    }
}
```

---

## 🗃️ Generic Repository Usage

### ✅ Trước khi có Generic Repository:
```csharp
// Mỗi entity phải viết riêng all methods
public interface IMovieRepository
{
    Task<Movie> CreateAsync(Movie movie);
    Task<List<Movie>> GetAllAsync();
    Task<Movie?> GetByIdAsync(Guid id);
    Task<Movie> UpdateAsync(Movie movie);
    Task<bool> DeleteAsync(Guid id);
    // ... repeat cho từng entity
}
```

### 🚀 Sau khi có Generic Repository:
```csharp
// Mọi entity đều có sẵn methods cơ bản!
public interface IMovieRepository : IGenericRepository<Movie>
{
    // Chỉ cần thêm methods đặc biệt cho Movie
    Task<List<Movie>> GetMoviesByGenreAsync(string genre);
    Task<List<Movie>> GetNowShowingAsync();
}
```

### 📚 Available Methods từ Generic Repository:

#### **CRUD Operations:**
```csharp
// Tạo mới
var newUser = await _userRepository.CreateAsync(user);

// Lấy tất cả  
var allUsers = await _userRepository.GetAllAsync();

// Lấy theo ID
var user = await _userRepository.GetByIdAsync(userId);

// Cập nhật
var updatedUser = await _userRepository.UpdateAsync(user);

// Xóa
var deleted = await _userRepository.DeleteAsync(userId);
```

#### **Query Operations:**
```csharp
// Tìm theo điều kiện
var activeUsers = await _userRepository.FindAsync(u => u.IsActive == true);

// Tìm 1 user đầu tiên
var admin = await _userRepository.FirstOrDefaultAsync(u => u.Role == UserRole.Admin);

// Đếm
var userCount = await _userRepository.CountAsync();
var activeCount = await _userRepository.CountAsync(u => u.IsActive);

// Check tồn tại
var exists = await _userRepository.ExistsAsync(u => u.Email == "test@email.com");
```

#### **Pagination:**
```csharp
// Lấy trang đầu tiên, 10 users mỗi trang
var users = await _userRepository.GetPagedAsync(page: 1, pageSize: 10);

// Tìm + phân trang
var activeUsers = await _userRepository.FindPagedAsync(
    u => u.IsActive == true, 
    page: 2, 
    pageSize: 20
);
```

---

## 🎯 Cách tạo Repository mới

### 1. Tạo Interface (kế thừa Generic):
```csharp
public interface IMovieRepository : IGenericRepository<Movie>
{
    // Chỉ methods đặc biệt cho Movie
    Task<List<Movie>> GetByGenreAsync(string genre);
    Task<List<Movie>> GetNowShowingAsync();
}
```

### 2. Tạo Implementation:
```csharp
public class MovieRepository : GenericRepository<Movie>, IMovieRepository
{
    public MovieRepository(MovieContext context) : base(context)
    {
    }

    public async Task<List<Movie>> GetByGenreAsync(string genre)
    {
        // Sử dụng _dbSet từ GenericRepository
        return await _dbSet
            .Where(m => m.MovieGenres.Any(mg => mg.Genre.Name == genre))
            .ToListAsync();
    }

    public async Task<List<Movie>> GetNowShowingAsync()
    {
        return await FindAsync(m => m.Status == MovieStatus.DangChieu);
    }

    // Các methods cơ bản đã có sẵn từ GenericRepository!
    // CreateAsync, UpdateAsync, GetByIdAsync, etc.
}
```

### 3. Đăng ký DI:
```csharp
// Generic Repository đã được đăng ký rồi
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// Chỉ cần đăng ký specific repository
builder.Services.AddScoped<IMovieRepository, MovieRepository>();
```

---

## 🤝 Best Practices

### ✅ DOs:
- **Luôn inject IMapper** vào service constructor
- **Sử dụng lambda expressions** cho FindAsync: `u => u.IsActive`
- **Kế thừa GenericRepository** cho specific repositories  
- **Chỉ implement methods đặc biệt** trong specific repositories

### ❌ DON'Ts:
- **Không manual mapping** nữa (dùng AutoMapper)
- **Không implement lại CRUD** methods trong specific repositories
- **Không viết raw SQL** trừ khi thực sự cần

---

## 📝 Examples cho Team

### Movie Service Example:
```csharp
public class MovieService
{
    private readonly IMovieRepository _movieRepository;
    private readonly IMapper _mapper;

    public MovieService(IMovieRepository movieRepository, IMapper mapper)
    {
        _movieRepository = movieRepository;
        _mapper = mapper;
    }

    public async Task<List<MovieDto>> GetAllMoviesAsync()
    {
        var movies = await _movieRepository.GetAllAsync();
        return _mapper.Map<List<MovieDto>>(movies);
    }

    public async Task<MovieDto> CreateMovieAsync(CreateMovieDto request)
    {
        var movie = _mapper.Map<Movie>(request);
        var created = await _movieRepository.CreateAsync(movie);
        return _mapper.Map<MovieDto>(created);
    }

    public async Task<List<MovieDto>> SearchMoviesAsync(string title)
    {
        var movies = await _movieRepository.FindAsync(m => m.Title.Contains(title));
        return _mapper.Map<List<MovieDto>>(movies);
    }
}
```

**🎉 Code giảm từ 50+ dòng xuống còn 15 dòng!**

---

## 🔍 Lambda Expressions Cheat Sheet

```csharp
// Basic comparisons
u => u.IsActive == true
u => u.Role == UserRole.Admin
u => u.Score > 100

// String operations
u => u.Username.Contains("admin")
u => u.Email.StartsWith("test")
u => u.FullName.ToLower() == "john doe"

// Date operations
u => u.CreatedAt > DateTime.Now.AddDays(-7)
u => u.BirthDate.HasValue && u.BirthDate.Value.Year > 1990

// Complex conditions
u => u.IsActive && u.Role == UserRole.Member && u.Score > 50
u => u.Bookings.Any(b => b.Status == BookingStatus.Confirmed)
```

**💡 Tip:** Lambda expressions = cách viết ngắn gọn cho điều kiện WHERE trong SQL! 