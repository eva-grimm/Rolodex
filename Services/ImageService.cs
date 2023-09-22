using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Rolodex.Models;
using Rolodex.Services.Interfaces;

namespace Rolodex.Services
{
    public class ImageService : IImageService
    {
        //private readonly string? _defaultImage = "/img/DefaultContactImage.png";
        private readonly string? _defaultImage = "https://placehold.co/500/ec4132/FDEEEC?font=roboto&text=Your+Image+Here";
        public string? ConvertByteArrayToFile(byte[]? fileData, string extension)
        {
            try
            {
                if (fileData is null) return _defaultImage;
                
                string? imageBase64Data = Convert.ToBase64String(fileData);
                imageBase64Data = string.Format($"data:{extension};base64,{imageBase64Data}");
                return imageBase64Data;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<byte[]> ConvertFileToByteArrayAsync(IFormFile? file)
        {
            try
            {
                using MemoryStream memoryStream = new();
                await file!.CopyToAsync(memoryStream);
                byte[] byteFile = memoryStream.ToArray();
                memoryStream.Close();
                return byteFile;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
