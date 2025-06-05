using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DomainLayer.Enum;

namespace DomainLayer.Entities
{    
    [Table("tbl_users")]
    public class Users
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid UserId { get; set; }

        // Auth
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
        public int Phone { get; set; }

        //Enum
        [Required]
        public UserRole Role { get; set; } = UserRole.Member;

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Address { get; set; } = string.Empty;

        // Score
        [Column(TypeName = "double precision")]
        public double Score { get; set; } = 0.0;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Additional Info
        [Column(TypeName = "date")]
        public DateTime? BirthDate { get; set; }

        [MaxLength(10)]
        public string? Gender { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        // Avatar - lưu URL string thay vì image binary
        [MaxLength(500)]
        public string? Avatar { get; set; }
    }
}
