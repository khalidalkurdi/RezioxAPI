using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.ExternalcCloud
{
    public class CloudImage : ICloudImag
    {
        private readonly Cloudinary _cloudinary;
        public CloudImage(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }
        public async Task<string> SaveImageAsync(IFormFile image)
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

    }
}
