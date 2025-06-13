using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DomainLayer.Entities
{
    [Table("tbl_employees")]
    public class Employee
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid EmployeeId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [Phone]
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(200)]
        public string Address { get; set; } = string.Empty;
        
        [Column(TypeName = "date")]
        public DateTime DateOfBirth { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Position { get; set; } = string.Empty;
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Salary { get; set; }
        
        [Required]
        [Column(TypeName = "date")]
        public DateTime HireDate { get; set; }
        
        [MaxLength(500)]
        public string? ProfileImageUrl { get; set; }
        
        [Required]
        public bool IsActive { get; set; } = true;
        
        [MaxLength(50)]
        public string? UserId { get; set; }
        
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
} 