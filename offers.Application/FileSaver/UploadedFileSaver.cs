using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using offers.Application.Models.ViewModel;
using System;
using System.Linq;

namespace offers.Application.FileSaver
{
    public static class UploadedFileSaver
    {
        private readonly static string _basePath = @"C:\Users\misho\Desktop\Project\offers.itacademy.ge\UploadedFiles";

        public static async Task<string> SaveUploadedFileAsync(IFormFile file, CancellationToken cancellationToken = default)
        {
            var folder = "uploads";
            var savePath = Path.Combine(_basePath, folder);
            Directory.CreateDirectory(savePath);

            var extension = Path.GetExtension(file.FileName);
            var uniqueFileName = Guid.NewGuid().ToString() + extension;
            var fullPath = Path.Combine(savePath, uniqueFileName);

            using var streamOut = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(streamOut, cancellationToken);

            return "/" + folder + "/" + uniqueFileName; 
        }
    }
}
