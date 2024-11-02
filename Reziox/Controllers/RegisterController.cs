
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
        [HttpPost("SignUp")]
        public async Task<IActionResult> SignUp([FromBody] SignUpVM signUpRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //checked if the email already exists
            var existemail = await _db.Users.FirstOrDefaultAsync(u => u.Email == signUpRequest.Email);
            if (existemail != null)
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
        [HttpPost("LogIn")]
        public async Task<IActionResult> LogIn([FromBody] LoginVM loginRequest)
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
        [HttpPut("UpdateUser")]
        public async Task<IActionResult> UpdateUser(int userId, [FromBody] SignUpVM updateUserRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //find the user by id
            var user = await _db.Users.FirstOrDefaultAsync (u=>u.UserId==userId);
            if (user == null)
            {
                return NotFound($"User {userId} not found.");
            }

            //update user
            user.UserName = updateUserRequest.UserName;
            user.Email = updateUserRequest.Email;
            user.PhoneNumber = updateUserRequest.PhoneNumber;
            user.City = updateUserRequest.City;

            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
