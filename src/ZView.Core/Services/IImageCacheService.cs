using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ZView.Core.Services
{
    public interface IImageCacheService
    {
        int Capacity { get; set; }
        long MaxMemoryBytes { get; set; }
        long MemoryUsageBytes { get; }
        int Count { get; }
        
        bool TryGet(string filePath, out BitmapSource? bitmap);
        void Put(string filePath, BitmapSource bitmap);
        void Remove(string filePath);
        void Clear();
        
        Task PrefetchAsync(IEnumerable<string> filePaths, IImageLoaderService loader, CancellationToken ct = default);
    }
}

