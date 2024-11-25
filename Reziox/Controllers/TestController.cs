using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Reziox.DataAccess;
using System.Text.Json;

namespace Rezioxgithub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly Cloudinary _cloudinary;
        private readonly string _imgbbApiKey = "3b5f46e363fabef53ca5d88fcd71578a";
        public TestController(AppDbContext db ,Cloudinary Cloud)
        {
            _db = db;
            _cloudinary = Cloud;
        }
        [HttpPost("ActionName")]
        public async Task<IActionResult> SaveImageCloudinaryAsync(IFormFile image)
        {
            if (image == null || image.Length == 0)
                return BadRequest("File is empty");
            //requst
            using var stream = image.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(image.FileName, stream)
            };
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            if (uploadResult.Error != null)
                return BadRequest(uploadResult.Error.Message);
            return Ok(uploadResult.SecureUrl.ToString());
        }

        [HttpPost]
        public async Task<IActionResult> SaveImageAsync(IFormFile image)
        {
            //encode image
            string base64Image;
            using (var ms = new MemoryStream())
            {
                await image.CopyToAsync(ms);
                var imageBytes = ms.ToArray();
                base64Image = Convert.ToBase64String(imageBytes);
            }
            //requst
            using (var client = new HttpClient())
            {
                var requestContent = new MultipartFormDataContent
                {
                    { new StringContent(_imgbbApiKey), "key" },
                    { new StringContent(base64Image), "image" }
                };
                //post
                var response = await client.PostAsync("https://api.imgbb.com/1/upload" , requestContent);
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }
                    

                //get url from response 
                var responseContent = await response.Content.ReadAsStringAsync();
                using (var jsonDoc = JsonDocument.Parse(responseContent))
                {
                    var imageUrl = jsonDoc.RootElement
                                           .GetProperty("data")
                                           .GetProperty("url")
                                           .ToString();
                    return Ok(imageUrl);
                }
            }
        }
    }
}
