using System.ComponentModel.DataAnnotations;

namespace Rezioxgithub.Model.ThePlace
{
    public class PartTimeVM
    {
        [Required]
        public TimeSpan Start { get; set; }
        [Required]
        public TimeSpan End { get; set; }
    }
}
