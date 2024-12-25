using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.DTO
{
    public class dtoSelectBooking
    {
       public int placeId { get; set; }
       public int userId { get; set; }
       public DateOnly datebooking { get; set; }
       public string bookinshift { get; set; }
    }
}
