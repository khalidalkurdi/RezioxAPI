using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reziox.DataAccess;
using Reziox.Model.TheUsers;

namespace Reziox.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegisterController : ControllerBase
    {
        private readonly AppDbContext _db;
        public RegisterController(AppDbContext db)
        {

            _db = db;

        }
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var user = await _db.Users.ToListAsync();
            return Ok(user);
        }
        [HttpPost]
        public async Task<IActionResult> AddUser([FromBody] UserVM u)
        {
            var user = new User()
            {
                UserName = u.UserName,
                Email = u.Email,
                Password = u.Password,
                PhoneNumber = u.PhoneNumber,
                City = u.City,
            };
             await _db.Users.AddAsync(user);
            _db.SaveChanges();
            return Ok( user.Email+ "--add successfuly");
        }
    }
}
