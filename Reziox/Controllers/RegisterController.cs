
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model.DTO;
using Reziox.DataAccess;
using Reziox.Model;
using Reziox.Model.TheUsers;

namespace Reziox.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegisterController : ControllerBase
    {
        private readonly AppDbContext _db;
        
        public RegisterController(AppDbContext db )
        {
            _db = db;
        }

        [HttpPost("SignUp")]
        public async Task<IActionResult> SignUp([FromBody]dtoSignUp signUpRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //checked if the email already exists
            var existemail = await _db.Users.FirstOrDefaultAsync(u => u.Email == signUpRequest.Email.ToLower());
            if (existemail != null)
            {
                return BadRequest("email is already in use , change it.");
            }
            //convert password to hash for more scurity 
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(signUpRequest.Password);
            //convert string to enum value
            if (!Enum.TryParse(signUpRequest.City.ToLower(), out MyCitys cityEnum))
            {
                return BadRequest($"City :{signUpRequest.City}");
            }
            var user = new User
            {
                UserName = signUpRequest.UserName,
                Email = signUpRequest.Email.ToLower(),
                Password = hashedPassword,
                PhoneNumber = signUpRequest.PhoneNumber,
                City = cityEnum
            };
            //add user
            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();
            var profileuser = new dtoProfile
            {
                UserId = user.UserId,
                UserImage = user.UserImage,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                City = user.City.ToString(),
            };
            return Ok(profileuser);
            return Ok($"Your Account Created Successfuly !");
        }
        [HttpPost("LogIn")]
        public async Task<IActionResult> LogIn([FromBody] dtoLogin loginRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //find the user by email
            var existUser = await _db.Users.FirstOrDefaultAsync(u => u.Email == loginRequest.Email.ToLower());
            if (existUser == null)
            {
                return Unauthorized("invalid email");
            }
            //verify the password 
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginRequest.Password.ToLower(), existUser.Password);
            if (!isPasswordValid)
            {
                return Unauthorized("invalid password.");
            }
            //return user information

            var profileuser = new dtoProfile
            {
                UserId = existUser.UserId,
                UserImage = existUser.UserImage,
                UserName = existUser.UserName,
                Email = existUser.Email,
                PhoneNumber = existUser.PhoneNumber,
                City = existUser.City.ToString(),
            };
            return Ok(profileuser);
        }

    }
}
