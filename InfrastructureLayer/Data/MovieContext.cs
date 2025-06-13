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

        //========================================================================================================================
        //Khai báo entity 
        //Dbset biểu diễn 1 bảng của csdl 
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



        //Nếu muốn cấu hình chi tiết thêm thì overrive OnModelCreating
        //Nếu đã sử dụng [] trc các attribute thì có thể ko cần method này 
        //Nếu có 1 số cái phức tạp mà [] ko thể triển khai hết thì nên dùng method này 
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Setup lại các thuộc tính cho entity user , đã có [] vẫn có thể settup đc nó sẽ override
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
