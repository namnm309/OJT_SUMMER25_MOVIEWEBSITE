using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainLayer.Entities;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureLayer.Data
{
    public class MovieContext : DbContext
    {
        // Constructor cho DI (Dependency Injection)
        public MovieContext(DbContextOptions<MovieContext> options) : base(options)
        {
        }

        // Constructor mặc định cho testing/migration
        public MovieContext()
        {
        }

        // Khai báo các DbSet Entity - mỗi DbSet đại diện cho một bảng trong cơ sở dữ liệu 
        public DbSet<Users> Users { get; set; }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<MovieImage> MovieImages { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<MovieGenre> MovieGenres { get; set; }
        public DbSet<CinemaRoom> CinemaRooms { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<ShowTime> ShowTimes { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingDetail> BookingDetails { get; set; }
        public DbSet<PointHistory> PointHistories { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<Employee> Employees { get; set; }


        // Cấu hình chi tiết Entity - sử dụng khi cần cấu hình phức tạp ngoài Data Annotations 
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình chi tiết cho Entity Users - override các thuộc tính đã có trong Data Annotations
            modelBuilder.Entity<Users>(entity =>
            {
                entity.HasIndex(u => u.Username)
                    .IsUnique()
                    .HasDatabaseName("IX_Users_Username");

                entity.HasIndex(u => u.Email)
                    .IsUnique()
                    .HasDatabaseName("IX_Users_Email");

                entity.Property(u => u.Username)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(u => u.Email)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(u => u.Password)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(u => u.FullName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(u => u.Address)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(u => u.IdentityCard)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(u => u.Score)
                    .HasColumnType("double precision")
                    .HasDefaultValue(0.0);

                entity.Property(u => u.IsActive)
                    .HasDefaultValue(true);

                entity.Property(u => u.CreatedAt)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .IsRequired();

                entity.Property(u => u.UpdatedAt)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .IsRequired();
            });
        }

    }
}
