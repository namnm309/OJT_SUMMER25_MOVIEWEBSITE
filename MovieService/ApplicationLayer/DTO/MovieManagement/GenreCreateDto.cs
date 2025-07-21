using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.DTO.MovieManagement
{
    public class GenreCreateDto
    {
        [Required]
        [MaxLength(50)]
        public string GenreName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}
