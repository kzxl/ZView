using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using ZView.Core.Models;

namespace ZView.Core.Services
{
    public class RecentFilesService : IRecentFilesService
    {
        private const int MaxItems = 8;
        private readonly string _storagePath;
        private readonly List<RecentFileEntry> _items = new();
        private readonly object _lock = new();

        public RecentFilesService(string? storagePath = null)
        {
            if (!string.IsNullOrWhiteSpace(storagePath))
            {
                _storagePath = storagePath;
            }
            else
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string dir = Path.Combine(appData, "ZeroUniverse", "ZView");
                Directory.CreateDirectory(dir);
                _storagePath = Path.Combine(dir, "recent_files.json");
            }

            Load();
        }

        public IReadOnlyList<RecentFileEntry> GetRecentItems()
        {
            lock (_lock)
            {
                return _items.ToList();
            }
        }

        public void AddRecent(string path, bool isDirectory)
        {
            if (string.IsNullOrWhiteSpace(path)) return;

            lock (_lock)
            {
                _items.RemoveAll(x => string.Equals(x.Path, path, StringComparison.OrdinalIgnoreCase));

                string displayName = Path.GetFileName(path);
                if (string.IsNullOrWhiteSpace(displayName)) displayName = path;

                string sizeFormatted = string.Empty;
                if (!isDirectory && File.Exists(path))
                {
                    long bytes = new FileInfo(path).Length;
                    if (bytes < 1024) sizeFormatted = $"{bytes} B";
                    else if (bytes < 1024 * 1024) sizeFormatted = $"{bytes / 1024.0:F1} KB";
                    else sizeFormatted = $"{bytes / (1024.0 * 1024.0):F2} MB";
                }

                _items.Insert(0, new RecentFileEntry
                {
                    Path = path,
                    DisplayName = displayName,
                    IsDirectory = isDirectory,
                    LastOpened = DateTime.UtcNow,
                    FormattedSize = sizeFormatted
                });

                if (_items.Count > MaxItems)
                {
                    _items.RemoveRange(MaxItems, _items.Count - MaxItems);
                }

                Save();
            }
        }

        public void RemoveRecent(string path)
        {
            lock (_lock)
            {
                int removed = _items.RemoveAll(x => string.Equals(x.Path, path, StringComparison.OrdinalIgnoreCase));
                if (removed > 0) Save();
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _items.Clear();
                Save();
            }
        }

        private void Load()
        {
            try
            {
                if (File.Exists(_storagePath))
                {
                    string json = File.ReadAllText(_storagePath);
                    var loaded = JsonSerializer.Deserialize<List<RecentFileEntry>>(json);
                    if (loaded != null)
                    {
                        _items.Clear();
                        _items.AddRange(loaded.Where(x => File.Exists(x.Path) || Directory.Exists(x.Path)).Take(MaxItems));
                    }
                }
            }
            catch { }
        }

        private void Save()
        {
            try
            {
                string json = JsonSerializer.Serialize(_items, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_storagePath, json);
            }
            catch { }
        }
    }
}
