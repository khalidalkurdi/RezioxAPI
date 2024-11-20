
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly Cloudinary _cloudinary;
        public RegisterController(AppDbContext db , Cloudinary cloudinary)
        {
            _db = db;
            _cloudinary = cloudinary;
        }
        private async Task<string> SaveImageAsync(IFormFile image)
        {
            if (image == null || image.Length == 0)
                return null;
            //requst
            using var stream = image.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(image.FileName, stream)
            };
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            if (uploadResult.Error != null)
                return null;
            return uploadResult.SecureUrl.ToString();
        }

        [HttpPost("SignUp")]
        public async Task<IActionResult> SignUp([FromBody]SignUpDto signUpRequest)
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
            return Ok($"Your Account Created Successfuly !");
        }
        [HttpPost("LogIn")]
        public async Task<IActionResult> LogIn([FromBody] LoginDto loginRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //find the user by email
            var existuser = await _db.Users.FirstOrDefaultAsync(u => u.Email == loginRequest.Email.ToLower());
            if (existuser == null)
            {
                return Unauthorized("invalid email");
            }
            //verify the password 
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginRequest.Password.ToLower(), existuser.Password);
            if (!isPasswordValid)
            {
                return Unauthorized("invalid password.");
            }
            //return user information
            return Ok(existuser);
        }
        [HttpPut("Edit")] // try create class for update
        public async Task<IActionResult> UpdateUser(int userId ,[FromBody] SignUpDto updateUserRequest,IFormFile? edituserimage)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (userId == 0)
            {
                return BadRequest("0 id is not correct !");
            }
            //find the user by id
            var user = await _db.Users.FirstOrDefaultAsync(u=>u.UserId==userId);
            if (user == null)
            {
                return NotFound($"user {userId} not found.");
            }
            //update user info
            var userimage = await SaveImageAsync(edituserimage);
            if (userimage != null)
            {
                user.UserImage = userimage;
            }
            if (!Enum.TryParse(updateUserRequest.City.ToLower(), out MyCitys cityEnum))
            {
                return BadRequest($"City :{updateUserRequest.City}");
            }
            user.UserName = updateUserRequest.UserName;
            user.Email = updateUserRequest.Email;
            user.PhoneNumber = updateUserRequest.PhoneNumber;
            user.City = cityEnum;         
            await _db.SaveChangesAsync();
            return Ok(user);
        }
        [HttpDelete("DeleteUser/{userid}")]
        public async Task<IActionResult> DeleteUser(int userid)
        {
            var user = await _db.Users.FindAsync(userid);
            if (user == null)
            {
                return NotFound($" user {userid} not found.");
            }
            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
            return Ok("user deleted successfuly ...");
        }

    }
}
