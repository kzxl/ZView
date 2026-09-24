using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ZView.Core.Services
{
    /// <summary>
    /// Thread-safe LRU memory cache with background pre-fetching engine.
    /// Eliminates IO bottlenecks and frame drops during fast image flipping.
    /// </summary>
    public class ImageCacheService : IImageCacheService
    {
        private readonly object _syncLock = new();
        private readonly Dictionary<string, LinkedListNode<CacheItem>> _map = new(StringComparer.OrdinalIgnoreCase);
        private readonly LinkedList<CacheItem> _lruList = new();
        private int _capacity;

        private class CacheItem
        {
            public string FilePath { get; set; } = string.Empty;
            public BitmapSource Bitmap { get; set; } = null!;
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
                    TrimToCapacity();
                }
            }
        }

        public int Count
        {
            get
            {
                lock (_syncLock) return _map.Count;
            }
        }

        public ImageCacheService(int capacity = 16)
        {
            _capacity = Math.Max(2, capacity);
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

            lock (_syncLock)
            {
                if (_map.TryGetValue(filePath, out var existingNode))
                {
                    existingNode.Value.Bitmap = bitmap;
                    _lruList.Remove(existingNode);
                    _lruList.AddFirst(existingNode);
                }
                else
                {
                    var item = new CacheItem { FilePath = filePath, Bitmap = bitmap };
                    var newNode = new LinkedListNode<CacheItem>(item);
                    _lruList.AddFirst(newNode);
                    _map[filePath] = newNode;

                    TrimToCapacity();
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

        private void TrimToCapacity()
        {
            while (_map.Count > _capacity && _lruList.Last != null)
            {
                var lruNode = _lruList.Last;
                _lruList.RemoveLast();
                _map.Remove(lruNode.Value.FilePath);
            }
        }
    }
}
