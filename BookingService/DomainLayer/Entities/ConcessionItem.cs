using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DomainLayer.Entities
{
    public class ConcessionItem : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } // Tên sản phẩm: Bắp rang, Nước ngọt, Combo v.v.

        [MaxLength(255)]
        public string Description { get; set; } // Mô tả sản phẩm

        [Required]
        public double Price { get; set; } // Giá sản phẩm

        [MaxLength(255)]
        public string ImageUrl { get; set; } // Hình ảnh minh họa (nếu có)

        public bool IsActive { get; set; } = true; // Trạng thái hoạt động
    }
}
