using BCrypt.Net;
using DataAccess.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model;
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
        private readonly IEmailService _email;
        
        public AccountController(AppDbContext db,IEmailService email )
        {
            _db = db;
            _email = email;
        }
        /// <summary>
        /// take an email, password,username, phone, city and token of device 
        /// </summary>
        /// <param name="dtosignUp"></param>
        /// <returns> return profile of the user</returns>
        [HttpPost("SignUp")]
        public async Task<IActionResult> SignUp([FromBody] dtoSignUp dtosignUp)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                //checked if the email already exists
                var existemail = await _db.Users.Where(u => u.Email == dtosignUp.Email.ToLower()).FirstOrDefaultAsync();
                if (existemail != null)
                {
                    return BadRequest("email is already in use , change it.");
                }
                //convert password to hash for more scurity 
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(dtosignUp.Password);
                //convert string to enum value
                if (!Enum.TryParse(dtosignUp.City.ToLower(), out MyCitys cityEnum))
                {
                    return BadRequest($"City :{dtosignUp.City}");
                }
                var user = new User
                {
                    UserName = dtosignUp.UserName,
                    Email = dtosignUp.Email.ToLower(),
                    Password = hashedPassword,
                    PhoneNumber = dtosignUp.PhoneNumber,
                    City = cityEnum,
                    DiviceToken= dtosignUp.DiviceToken
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
        /// <summary>
        /// take an email and password
        /// </summary>
        /// <param name="dtologin"></param>
        /// <returns> return profile of the user</returns>
        [HttpPost("LogIn")]
        public async Task<IActionResult> LogIn([FromBody] dtoLogin dtologin)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                //find the user by email
                var existUser = await _db.Users.Where(u => u.Email == dtologin.Email.ToLower()).FirstOrDefaultAsync();
                if (existUser == null)
                {
                    return Unauthorized("invalid email");
                }
                //verify the password 
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dtologin.Password.ToLower(), existUser.Password);
                if (!isPasswordValid)
                {
                    return Unauthorized("invalid password.");
                }
                existUser.DiviceToken = dtologin.DiviceToken;
                await _db.SaveChangesAsync();
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
        /// <summary>
        /// take email then send verification code  to the email
        /// </summary>
        /// <param name="email"></param>
        /// <returns> status code </returns>
        [HttpPost("SendCode")]
        public async Task<IActionResult> SendVerificationCode([FromBody] string email)
        {
            try
            {
                //checked if the email exists
                var existemail = await _db.Users.Where(u => u.Email == email.ToLower()).FirstOrDefaultAsync();
                if (existemail == null)
                {
                    return NotFound("email is not found");
                }
                var oldCodes = await _db.Verifications.Where(v => v.Email == existemail.Email).Where(v => v.ExDate > DateTime.UtcNow).ToListAsync();
                if (oldCodes.Count > 4)
                {
                    return BadRequest("You sent much requset , please waite and try agin after 3 minuts.. ");
                }
                var random = new Random();
                var code = random.Next(100000, 999999).ToString();
                var vrification = new Verification
                {
                    Email = email,
                    Code = code
                };
                await _db.Verifications.AddAsync(vrification);
                await _db.SaveChangesAsync();
                await _email.SendVerificationCodeAsync(email,code);
                return Ok(" sent code successfuly !");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        /// <summary>
        /// take email and verfication code for verify the code is valid
        /// </summary>
        /// <param name="dtoVrification"></param>
        /// <returns> status code</returns>
        [HttpPost("Verification")]
        public async Task<IActionResult> VerifiyCode([FromBody] dtoVrification dtoVrification)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                //checked if the email exists
                var existUmail = await _db.Users.Where(u => u.Email.ToLower() == dtoVrification.Email.ToLower()).FirstOrDefaultAsync();
                if (existUmail == null)
                {
                    return NotFound(" Email not found");
                }
                var enabelCode= _db.Verifications.Where(v=>v.Email==dtoVrification.Email.ToLower()).Where(v=>v.Code==dtoVrification.Code).Where(v=>v.ExDate>DateTime.UtcNow).FirstOrDefault();
                if (enabelCode == null)
                {
                    return BadRequest("The code is not correct or is old code.. !");
                }
                return Ok("is correct");

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        /// <summary>
        /// take an email and new password  for update the password of user
        /// </summary>
        /// <param name="dtoReset"></param>
        /// <returns>status code</returns>
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] dtoResetPassword dtoReset)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                //checked if the email exists
                var existUser = await _db.Users.Where(u => u.Email == dtoReset.Email.ToLower()).FirstOrDefaultAsync();
                if (existUser == null)
                {
                    return NotFound("email is not found");
                }
                //convert password to hash for more scurity 
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(dtoReset.Password);
                existUser.Password = hashedPassword;               
                var listCode =await _db.Verifications.Where(v => v.Email == dtoReset.Email).ToListAsync();
                _db.Verifications.RemoveRange(listCode);
                await _db.SaveChangesAsync();
                return Ok("reset successfuly");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }
}
