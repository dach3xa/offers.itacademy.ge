using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using offers.Application.Models.ViewModel;

namespace offers.Web.Controllers.FileSaver
{
    public static class UploadedFileSaver
    {
        public static async Task<string> SaveUploadedFileAsync(IFormFile file, IWebHostEnvironment webHostEnvironment)
        {

            var uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                throw new Exception("only image files!");
            }
            var uniqueFileName = Guid.NewGuid().ToString() + extension;

            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return "/uploads/" + uniqueFileName;
        }
    }
}
