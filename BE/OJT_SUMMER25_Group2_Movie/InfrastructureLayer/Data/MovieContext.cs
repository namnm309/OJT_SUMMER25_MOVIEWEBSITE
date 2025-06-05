using DomainLayer.Entities;
using Microsoft.EntityFrameworkCore;

namespace MovieWebApplication.Data
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

        //Khai báo entity 
        //Dbset biểu diễn 1 bảng của csdl 
        public DbSet<Users> Users { get; set; }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<MovieImage> MovieImages { get; set; }

        //Cấu hình các unique , quan hệ để tạo db ( nếu chưa có )
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User 
            modelBuilder.Entity<Users>(entity =>
            {
                entity.HasKey(x => x.Id);

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

            // Configure Movie 
            modelBuilder.Entity<Movie>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(m => m.Title)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(m => m.ProductionCompany)
                    .HasMaxLength(100);

                entity.Property(m => m.RunningTime)
                    .HasMaxLength(50);

                entity.Property(m => m.Actors)
                    .HasMaxLength(500);

                entity.Property(m => m.Director)
                    .HasMaxLength(100);

                entity.Property(m => m.TrailerUrl)
                    .HasMaxLength(500);

                entity.Property(m => m.Content)
                    .HasMaxLength(2000);
            });

            // Configure MovieImage và relationships
            modelBuilder.Entity<MovieImage>(entity =>
            {
                entity.Property(mi => mi.ImageUrl)
                    .HasMaxLength(500)
                    .IsRequired();  

                entity.Property(mi => mi.Description)
                    .HasMaxLength(200);

                entity.HasOne(mi => mi.Movie)
                    .WithMany(m => m.MovieImages)
                    .HasForeignKey(mi => mi.MovieId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasIndex(mi => new { mi.MovieId, mi.IsPrimary })
                    .HasFilter("\"IsPrimary\" = true")
                    .IsUnique()
                    .HasDatabaseName("IX_MovieImages_MovieId_IsPrimary");
            });
        }
    }
}
