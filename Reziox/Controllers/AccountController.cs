using BCrypt.Net;
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
    public class AccountController : ControllerBase
    {
        private readonly AppDbContext _db;
        
        public AccountController(AppDbContext db )
        {
            _db = db;
        }

        [HttpPost("SignUp")]
        public async Task<IActionResult> SignUp([FromBody] dtoSignUp dtosignUpRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                //checked if the email already exists
                var existemail = await _db.Users.Where(u => u.Email == dtosignUpRequest.Email.ToLower()).FirstOrDefaultAsync();
                if (existemail != null)
                {
                    return BadRequest("email is already in use , change it.");
                }
                //convert password to hash for more scurity 
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(dtosignUpRequest.Password);
                //convert string to enum value
                if (!Enum.TryParse(dtosignUpRequest.City.ToLower(), out MyCitys cityEnum))
                {
                    return BadRequest($"City :{dtosignUpRequest.City}");
                }
                var user = new User
                {
                    UserName = dtosignUpRequest.UserName,
                    Email = dtosignUpRequest.Email.ToLower(),
                    Password = hashedPassword,
                    PhoneNumber = dtosignUpRequest.PhoneNumber,
                    City = cityEnum,
                    DiviceToken= dtosignUpRequest.DiviceToken
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
                    City = user.City.ToString()
                };
                return Ok(profileuser);
                
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("LogIn")]
        public async Task<IActionResult> LogIn([FromBody] dtoLogin dtologinRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                //find the user by email
                var existUser = await _db.Users.Where(u => u.Email == dtologinRequest.Email.ToLower()).FirstOrDefaultAsync();
                if (existUser == null)
                {
                    return Unauthorized("invalid email");
                }
                //verify the password 
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dtologinRequest.Password.ToLower(), existUser.Password);
                if (!isPasswordValid)
                {
                    return Unauthorized("invalid password.");
                }
                existUser.DiviceToken = dtologinRequest.DiviceToken;
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
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        
    }
}
