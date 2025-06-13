using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.DTO.MovieManagement
{
    public class MovieImageDto
    {
        [Required, MaxLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }

        public int DisplayOrder { get; set; } = 1;

        public bool IsPrimary { get; set; } = false;
    }

}
