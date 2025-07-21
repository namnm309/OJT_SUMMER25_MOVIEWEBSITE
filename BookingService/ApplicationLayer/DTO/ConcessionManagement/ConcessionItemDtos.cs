using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ApplicationLayer.DTO.ConcessionManagement
{
    // DTO để trả về thông tin đầy đủ của ConcessionItem
    public class ConcessionItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    // DTO để tạo mới ConcessionItem
    public class CreateConcessionItemDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public double Price { get; set; }

        [MaxLength(255)]
        public string ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;
    }

    // DTO để cập nhật ConcessionItem
    public class UpdateConcessionItemDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public double Price { get; set; }

        [MaxLength(255)]
        public string ImageUrl { get; set; }

        public bool IsActive { get; set; }
    }

    // Request DTO cho phân trang, kế thừa từ PaginationReq
    public class GetConcessionItemsRequest : PaginationReq
    {
        public bool? IsActive { get; set; }
        public string SearchTerm { get; set; }
    }

    // Response DTO cho phân trang, kế thừa từ PaginationResp
    public class GetConcessionItemsResponse : PaginationResp
    {
        public List<ConcessionItemDto> Items { get; set; }
    }
}
