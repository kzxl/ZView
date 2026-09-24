using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ZView.Core.Models;

namespace ZView.Core.Services
{
    public interface IImageLoaderService
    {
        IReadOnlySet<string> SupportedExtensions { get; }
        
        bool IsSupportedExtension(string extension);
        
        Task<BitmapSource?> LoadImageAsync(string filePath, bool autoRotate = true, CancellationToken ct = default);
        
        Task<BitmapSource?> LoadThumbnailAsync(string filePath, int targetSize = 160, CancellationToken ct = default);
        
        ImageMetadataInfo ExtractMetadata(string filePath);
    }
}
