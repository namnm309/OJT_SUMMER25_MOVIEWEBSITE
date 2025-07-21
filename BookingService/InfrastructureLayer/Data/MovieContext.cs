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
            // Database migration sẽ được gọi ở Program.cs để tránh lỗi lúc design-time
        }

        // Constructor mặc định cho testing/migration
        public MovieContext()
        {
        }

        // Khai báo các DbSet Entity - mỗi DbSet đại diện cho một bảng trong cơ sở dữ liệu 
      
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingDetail> BookingDetails { get; set; }
       
        public DbSet<Promotion> Promotions { get; set; }
       
        public DbSet<Transaction> Transaction { get; set; }
       
        public DbSet<Ticket> Ticket { get; set; }
       
        public DbSet<UserPromotion> UserPromotions { get; set; } // New table linking users & promotions

        public DbSet<ConcessionItem> ConcessionItems { get; set; }

        // Cấu hình chi tiết Entity - sử dụng khi cần cấu hình phức tạp ngoài Data Annotations 
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình chi tiết cho Entity Users - override các thuộc tính đã có trong Data Annotations

        }
    }
}
