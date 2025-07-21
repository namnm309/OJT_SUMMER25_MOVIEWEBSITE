using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.DTO.PromotionManagement
{
    public class PromotionCreateDto
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        private DateTime _startDate;
        private DateTime _endDate;

        [Required]
        public DateTime StartDate
        {
            get => _startDate;
            set => _startDate = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        [Required]
        public DateTime EndDate
        {
            get => _endDate;
            set => _endDate = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        [Required]
        [Range(0, 100)]
        public int DiscountPercent { get; set; }

        [Required]
        public string Description { get; set; }

        public string ImageUrl { get; set; }
    }
}
