using Reziox.Model.ThePlace;
using Reziox.Model.TheUsers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.DTO
{
    public class dtoReview
    {
        [Required]     
        public int UserId { get; set; }
        [Required]        
        public int PlaceId { get; set; }
        [Range(0.0, 5.0)]
        [Required]
        public double Rating { get; set; }
        [Required]
        public string? Comment { get; set; }
    }
}
