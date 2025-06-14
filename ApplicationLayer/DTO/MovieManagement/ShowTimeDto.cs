using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.DTO.MovieManagement
{
    public class ShowTimeDto
    {
        public DateTime ShowDate { get; set; }

        [Required]
        public Guid RoomId { get; set; }
    }

}
