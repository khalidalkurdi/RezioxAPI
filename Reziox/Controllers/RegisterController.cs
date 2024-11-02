using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
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
        [HttpPost]
        public async Task<IActionResult> SignUp([FromBody] SignUpVM signUpRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //checked if the email already exists
            var existemail = await _db.Users.FirstOrDefaultAsync(u => u.Email == signUpRequest.Email);
            if (existemail!=null)
            {
                return BadRequest("Email is already in use , change it.");
            }

            //create a new user
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(signUpRequest.Password);
            var user = new User
            {
                UserName = signUpRequest.UserName,
                Email = signUpRequest.Email,
                Password = hashedPassword, 
                PhoneNumber = signUpRequest.PhoneNumber,
                City = signUpRequest.City
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return Ok();
        }
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginVM loginRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //find the user by email
            var existuser = await _db.Users.FirstOrDefaultAsync(u => u.Email == loginRequest.Email);
            if (existuser == null)
            {
                return Unauthorized("Invalid email");
            }

            //verify the password 
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginRequest.Password, existuser.Password);
            if (!isPasswordValid)
            {
                return Unauthorized("Invalid password.");
            }

            //return user information
            
            return Ok(existuser);
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var user = await _db.Users.ToListAsync();
            return Ok(user);
        }
       
    }
}
