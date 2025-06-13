using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainLayer.Enum;

namespace DomainLayer.Entities
{
    [Table("tbl_users")]
    public class Users : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string IdentityCard { get; set; } = string.Empty;

        //Enum
        [Required]
        public UserRole Role { get; set; } = UserRole.Member;

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Address { get; set; } = string.Empty;

        [Column(TypeName = "double precision")]
        public double Score { get; set; } = 0.0;

        [Column(TypeName = "date")]
        public DateTime? BirthDate { get; set; }

        [MaxLength(10)]
        public UserGender Gender { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        // Avatar - lưu URL string thay vì image binary
        [MaxLength(500)]
        public string? Avatar { get; set; }

        // Quan hệ với bảng bookings và pointHistories
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public virtual ICollection<PointHistory> PointHistories { get; set; } = new List<PointHistory>();


    }
}
