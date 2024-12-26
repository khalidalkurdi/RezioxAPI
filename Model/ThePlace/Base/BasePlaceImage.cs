using Reziox.Model.ThePlace;
using Reziox.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.ThePlace.Base
{
    public class BasePlaceImage
    {
        [Key]
        public int ImageId { get; set; }
        [Required]
        public string ImageUrl { get; set; }
    }
}
