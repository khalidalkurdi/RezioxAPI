using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.DTO
{
    public class dtoSearch
    {
       public DateOnly ChoicDate { get; set; }
       public int? MinPrice { get; set; }
       public int? MaxPrice { get; set; }
       public int? Rating { get; set; }
       public int? Gusts { get; set; }
       public string? TypeShift { get; set; }
       public string? City { get; set; }
       public ICollection<string>? Features { get; set; }
    }
}
