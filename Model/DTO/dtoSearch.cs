using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.DTO
{
    public class dtoSearch
    {
       public DateOnly choicdate { get; set; }
       public int? minPrice { get; set; }
       public int? maxPrice { get; set; }
       public int? gusts { get; set; }
       public string typeshift { get; set; }
       public string? city { get; set; }
       public ICollection<string>? features { get; set; }
    }
}
