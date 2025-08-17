using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
namespace DataAccess.ExternalcCloud
{
    public class CloudImage : ICloudImag
    {
        private readonly Cloudinary _cloudinary;
        public CloudImage(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        /// <summary>
        /// take old url then remove it
        /// </summary>
        /// <param name="Url"></param>
        /// <returns> bool value</returns>
        public bool RemoveImage(string Url)
        {
            string publicId = Path.GetFileNameWithoutExtension(new Uri(Url).Segments.Last());
            var deletionParams = new DeletionParams(publicId)
            {
                ResourceType = ResourceType.Image
            };

            var result = _cloudinary.Destroy(deletionParams);
            if (result.Result == "ok")
                return true;
            return false;
        }
        /// <summary>
        /// take image then add it to cloud
        /// </summary>
        /// <param name="image"></param>
        /// <returns> url of image at cloud</returns>
        /// <exception cref="Exception"></exception>
        public async Task<string> SaveImageAsync(IFormFile image)
        {
            if (image == null || image.Length == 0)
                throw new Exception();
            //requst
            using var stream = image.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(image.FileName, stream)
            };
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            if (uploadResult.Error != null)
                throw new Exception();
            return uploadResult.SecureUrl.ToString();
        }

        
    }
}
