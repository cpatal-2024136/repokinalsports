using System;
using System.Security.Cryptography.X509Certificates;
using AuthService.Application.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;

namespace AuthService.Application.Services;

public class CloudinaryService(IConfiguration configuration) : ICloudinaryService
{
    private readonly Cloudinary _cloudinary = new(new Account(
        configuration["CloudinarySettings:Cloudname"],
        configuration["CloudinarySettings:ApiKey"],
        configuration["CloudinarySettings:ApiSecret"]
    ));

    public async Task<bool> DeleteImageAsync(string publicId)
    {
        try
        {
            var deleteParams = new DelResParams
            {
                PublicIds = [publicId]
            };
            var result = await _cloudinary.DeleteResourcesAsync(deleteParams);
            return result.Deleted?.ContainsKey(publicId) == true;
        }
        catch
        {
            return false;
        }
        
    }

    public string GetDefaultAvatarUrl()
    {
        var defaultPath = configuration["CloudinarySettings:DefaultAvatarPath"] ?? "avatarDefault-1749508519496.png";
        if(defaultPath.Contains('/')) return defaultPath.Split('/').Last();
        return defaultPath;
    }

    public string GetFullImageUrl(string imagePath)
    {
        var baseUrl = configuration["CloudinarySettings:BaseUrl"] ?? "https://res.cloudinary.com/dlhtat4nn/image/upload/v1769726008";
        var folder = configuration["CloudinarySetting:Folder"] ?? "auth_ks_in6av/profiles";
        var defaultPath = configuration ["CloudinarySetting:DefaultAvatarpath"] ?? "avatarDefault-1749508519496.png";

        var pathToUse = string.IsNullOrEmpty(imagePath) ? defaultPath : imagePath;
        if(!pathToUse.Contains('/')) pathToUse = $"{folder}/{pathToUse}";

        return $"{baseUrl}{pathToUse}";
    }

    public async Task<string> UploadImageAsync(IFileData imageFile, string fileName)
    {
        try
        {
            using var stream = new MemoryStream(imageFile.Data);
            var folder = configuration["CloudinarySettings:Folder"] ?? "auth_ks_in6av/profile";

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(imageFile.FileName, stream),
                PublicId = $"{folder}/{fileName}",
                Folder = folder,
                Transformation = new Transformation()
                    .Width(400)
                    .Height(400)
                    .Crop("fill")
                    .Gravity("face")
                    .Quality("auto")
                    .FetchFormat("auto")
            };
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if(uploadResult.Error != null)
            {
                throw new InvalidOperationException($"Error al subir la imagen: {uploadResult.Error.Message}");
            }

            return fileName;
        }
        catch(Exception ex)
        {
            throw new InvalidOperationException($"Error al subir la imagen a cloudinary: {ex.Message}", ex);
        }
    }
}
