using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ZView.Core.Services
{
    /// <summary>
    /// Thread-safe LRU memory cache with memory-budget aware eviction and background pre-fetching engine.
    /// Eliminates IO bottlenecks and frame drops during high-speed image flipping while strictly bounding RAM usage.
    /// </summary>
    public class ImageCacheService : IImageCacheService
    {
        private readonly object _syncLock = new();
        private readonly Dictionary<string, LinkedListNode<CacheItem>> _map = new(StringComparer.OrdinalIgnoreCase);
        private readonly LinkedList<CacheItem> _lruList = new();
        private int _capacity;
        private long _maxMemoryBytes;
        private long _currentMemoryBytes;

        private class CacheItem
        {
            public string FilePath { get; set; } = string.Empty;
            public BitmapSource Bitmap { get; set; } = null!;
            public long SizeBytes { get; set; }
        }

        public int Capacity
        {
            get
            {
                lock (_syncLock) return _capacity;
            }
            set
            {
                lock (_syncLock)
                {
                    _capacity = Math.Max(2, value);
                    TrimToLimits();
                }
            }
        }

        public long MaxMemoryBytes
        {
            get
            {
                lock (_syncLock) return _maxMemoryBytes;
            }
            set
            {
                lock (_syncLock)
                {
                    _maxMemoryBytes = Math.Max(1024, value);
                    TrimToLimits();
                }
            }
        }

        public long MemoryUsageBytes
        {
            get
            {
                lock (_syncLock) return _currentMemoryBytes;
            }
        }

        public int Count
        {
            get
            {
                lock (_syncLock) return _map.Count;
            }
        }

        public ImageCacheService(int capacity = 16, long maxMemoryBytes = 512 * 1024 * 1024) // Default 512MB RAM budget
        {
            _capacity = Math.Max(2, capacity);
            _maxMemoryBytes = Math.Max(1024, maxMemoryBytes);
        }


        public bool TryGet(string filePath, out BitmapSource? bitmap)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                bitmap = null;
                return false;
            }

            lock (_syncLock)
            {
                if (_map.TryGetValue(filePath, out var node))
                {
                    // Move to head of LRU list (most recently used)
                    _lruList.Remove(node);
                    _lruList.AddFirst(node);
                    bitmap = node.Value.Bitmap;
                    return true;
                }
            }

            bitmap = null;
            return false;
        }

        public void Put(string filePath, BitmapSource bitmap)
        {
            if (string.IsNullOrWhiteSpace(filePath) || bitmap == null) return;

            long sizeBytes = EstimateBitmapSize(bitmap);

            lock (_syncLock)
            {
                if (_map.TryGetValue(filePath, out var existingNode))
                {
                    _currentMemoryBytes -= existingNode.Value.SizeBytes;
                    existingNode.Value.Bitmap = bitmap;
                    existingNode.Value.SizeBytes = sizeBytes;
                    _currentMemoryBytes += sizeBytes;

                    _lruList.Remove(existingNode);
                    _lruList.AddFirst(existingNode);
                }
                else
                {
                    var item = new CacheItem
                    {
                        FilePath = filePath,
                        Bitmap = bitmap,
                        SizeBytes = sizeBytes
                    };
                    var newNode = new LinkedListNode<CacheItem>(item);
                    _lruList.AddFirst(newNode);
                    _map[filePath] = newNode;
                    _currentMemoryBytes += sizeBytes;

                    TrimToLimits();
                }
            }
        }

        public void Remove(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return;

            lock (_syncLock)
            {
                if (_map.TryGetValue(filePath, out var node))
                {
                    _currentMemoryBytes -= node.Value.SizeBytes;
                    _lruList.Remove(node);
                    _map.Remove(filePath);
                }
            }
        }

        public void Clear()
        {
            lock (_syncLock)
            {
                _lruList.Clear();
                _map.Clear();
                _currentMemoryBytes = 0;
            }
        }

        public async Task PrefetchAsync(IEnumerable<string> filePaths, IImageLoaderService loader, CancellationToken ct = default)
        {
            if (filePaths == null || loader == null) return;

            foreach (var path in filePaths)
            {
                if (ct.IsCancellationRequested) break;
                if (string.IsNullOrWhiteSpace(path)) continue;

                bool alreadyCached;
                lock (_syncLock)
                {
                    alreadyCached = _map.ContainsKey(path);
                }

                if (!alreadyCached)
                {
                    var bitmap = await loader.LoadImageAsync(path, autoRotate: true, ct).ConfigureAwait(false);
                    if (bitmap != null && !ct.IsCancellationRequested)
                    {
                        Put(path, bitmap);
                    }
                }
            }
        }

        private void TrimToLimits()
        {
            // Evict while exceeding item capacity OR exceeding RAM budget (preserving minimum 1 item)
            while ((_map.Count > _capacity || _currentMemoryBytes > _maxMemoryBytes) && _lruList.Count > 1 && _lruList.Last != null)
            {
                var lruNode = _lruList.Last;
                _currentMemoryBytes -= lruNode.Value.SizeBytes;
                _lruList.RemoveLast();
                _map.Remove(lruNode.Value.FilePath);
            }
        }

        private static long EstimateBitmapSize(BitmapSource bitmap)
        {
            try
            {
                int bpp = bitmap.Format.BitsPerPixel;
                if (bpp <= 0) bpp = 32;
                return (long)bitmap.PixelWidth * bitmap.PixelHeight * bpp / 8;
            }
            catch
            {
                return 4 * 1024 * 1024; // 4MB default estimation
            }
        }
    }
}

