using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ZView.Core.Models;
using ZView.Core.Utils;

namespace ZView.Core.Services
{
    public class FolderNavigationService : IFolderNavigationService
    {
        private readonly IImageLoaderService _imageLoader;
        private readonly List<ImageFileItem> _items = new();
        private FileSystemWatcher? _watcher;
        private int _currentIndex = -1;
        private bool _isDisposed;

        public string? CurrentDirectory { get; private set; }
        public IReadOnlyList<ImageFileItem> Items => _items;
        public int TotalCount => _items.Count;
        public SortBy CurrentSortBy { get; private set; } = SortBy.Name;
        public SortDirection CurrentSortDirection { get; private set; } = SortDirection.Ascending;
        public LoopMode LoopMode { get; set; } = LoopMode.Loop;

        public event EventHandler? CurrentItemChanged;
        public event EventHandler? ItemsListChanged;

        public int CurrentIndex
        {
            get => _currentIndex;
            set
            {
                if (_items.Count == 0)
                {
                    _currentIndex = -1;
                    CurrentItemChanged?.Invoke(this, EventArgs.Empty);
                    return;
                }

                int clamped = Math.Clamp(value, 0, _items.Count - 1);
                if (_currentIndex != clamped)
                {
                    _currentIndex = clamped;
                    CurrentItemChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public ImageFileItem? CurrentItem =>
            _currentIndex >= 0 && _currentIndex < _items.Count ? _items[_currentIndex] : null;

        public bool HasNext =>
            _items.Count > 1 && (LoopMode == LoopMode.Loop || _currentIndex < _items.Count - 1);

        public bool HasPrevious =>
            _items.Count > 1 && (LoopMode == LoopMode.Loop || _currentIndex > 0);

        public FolderNavigationService(IImageLoaderService imageLoader)
        {
            _imageLoader = imageLoader ?? throw new ArgumentNullException(nameof(imageLoader));
        }

        public void LoadDirectory(string directoryPath, string? selectedFilePath = null)
        {
            if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
            {
                _items.Clear();
                CurrentDirectory = null;
                CurrentIndex = -1;
                StopWatcher();
                ItemsListChanged?.Invoke(this, EventArgs.Empty);
                return;
            }

            CurrentDirectory = directoryPath;
            SetupWatcher(directoryPath);

            var dir = new DirectoryInfo(directoryPath);
            var files = dir.EnumerateFiles()
                .Where(f => _imageLoader.IsSupportedExtension(f.Extension))
                .Select(f => new ImageFileItem(f.FullName))
                .ToList();

            ApplySorting(files);

            _items.Clear();
            _items.AddRange(files);

            if (!string.IsNullOrWhiteSpace(selectedFilePath))
            {
                SelectByPath(selectedFilePath);
            }
            else
            {
                CurrentIndex = _items.Count > 0 ? 0 : -1;
            }

            ItemsListChanged?.Invoke(this, EventArgs.Empty);
        }

        public void LoadFiles(IEnumerable<string> filePaths, string? selectedFilePath = null)
        {
            StopWatcher();
            CurrentDirectory = null;

            var items = filePaths
                .Where(File.Exists)
                .Where(p => _imageLoader.IsSupportedExtension(Path.GetExtension(p)))
                .Select(p => new ImageFileItem(p))
                .ToList();

            ApplySorting(items);

            _items.Clear();
            _items.AddRange(items);

            if (!string.IsNullOrWhiteSpace(selectedFilePath))
            {
                SelectByPath(selectedFilePath);
            }
            else
            {
                CurrentIndex = _items.Count > 0 ? 0 : -1;
            }

            ItemsListChanged?.Invoke(this, EventArgs.Empty);
        }

        public ImageFileItem? MoveNext()
        {
            if (_items.Count == 0) return null;

            if (_currentIndex < _items.Count - 1)
            {
                CurrentIndex++;
            }
            else if (LoopMode == LoopMode.Loop)
            {
                CurrentIndex = 0;
            }

            return CurrentItem;
        }

        public ImageFileItem? MovePrevious()
        {
            if (_items.Count == 0) return null;

            if (_currentIndex > 0)
            {
                CurrentIndex--;
            }
            else if (LoopMode == LoopMode.Loop)
            {
                CurrentIndex = _items.Count - 1;
            }

            return CurrentItem;
        }

        public ImageFileItem? MoveFirst()
        {
            if (_items.Count == 0) return null;
            CurrentIndex = 0;
            return CurrentItem;
        }

        public ImageFileItem? MoveLast()
        {
            if (_items.Count == 0) return null;
            CurrentIndex = _items.Count - 1;
            return CurrentItem;
        }

        public bool SelectByPath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return false;

            int index = _items.FindIndex(i => string.Equals(i.FilePath, filePath, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                CurrentIndex = index;
                return true;
            }

            return false;
        }

        public void SetSorting(SortBy sortBy, SortDirection direction)
        {
            CurrentSortBy = sortBy;
            CurrentSortDirection = direction;
            string? currentSelectedPath = CurrentItem?.FilePath;

            ApplySorting(_items);

            if (!string.IsNullOrWhiteSpace(currentSelectedPath))
            {
                SelectByPath(currentSelectedPath);
            }

            ItemsListChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Refresh()
        {
            if (!string.IsNullOrWhiteSpace(CurrentDirectory) && Directory.Exists(CurrentDirectory))
            {
                string? currentSelected = CurrentItem?.FilePath;
                LoadDirectory(CurrentDirectory, currentSelected);
            }
        }

        public List<string> GetNeighborPaths(int forwardCount = 2, int backwardCount = 2)
        {
            var neighbors = new List<string>();
            if (_items.Count <= 1 || _currentIndex < 0) return neighbors;

            // Forward
            for (int i = 1; i <= forwardCount; i++)
            {
                int nextIndex = _currentIndex + i;
                if (nextIndex < _items.Count)
                {
                    neighbors.Add(_items[nextIndex].FilePath);
                }
                else if (LoopMode == LoopMode.Loop)
                {
                    int wrapped = nextIndex % _items.Count;
                    if (wrapped != _currentIndex) neighbors.Add(_items[wrapped].FilePath);
                }
            }

            // Backward
            for (int i = 1; i <= backwardCount; i++)
            {
                int prevIndex = _currentIndex - i;
                if (prevIndex >= 0)
                {
                    neighbors.Add(_items[prevIndex].FilePath);
                }
                else if (LoopMode == LoopMode.Loop)
                {
                    int wrapped = (_items.Count + (prevIndex % _items.Count)) % _items.Count;
                    if (wrapped != _currentIndex && !neighbors.Contains(_items[wrapped].FilePath))
                    {
                        neighbors.Add(_items[wrapped].FilePath);
                    }
                }
            }

            return neighbors;
        }

        private void ApplySorting(List<ImageFileItem> list)
        {
            switch (CurrentSortBy)
            {
                case SortBy.Name:
                    list.Sort((a, b) => NaturalStringComparer.Default.Compare(a.FileName, b.FileName));
                    break;
                case SortBy.DateModified:
                    list.Sort((a, b) => a.DateModified.CompareTo(b.DateModified));
                    break;
                case SortBy.FileSize:
                    list.Sort((a, b) => a.FileSizeBytes.CompareTo(b.FileSizeBytes));
                    break;
                case SortBy.Extension:
                    list.Sort((a, b) =>
                    {
                        int extCmp = string.Compare(a.Extension, b.Extension, StringComparison.OrdinalIgnoreCase);
                        return extCmp != 0 ? extCmp : NaturalStringComparer.Default.Compare(a.FileName, b.FileName);
                    });
                    break;
            }

            if (CurrentSortDirection == SortDirection.Descending)
            {
                list.Reverse();
            }
        }

        private void SetupWatcher(string directoryPath)
        {
            StopWatcher();
            try
            {
                _watcher = new FileSystemWatcher(directoryPath)
                {
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size,
                    EnableRaisingEvents = true
                };

                _watcher.Created += (s, e) => OnFileCreatedOrDeleted(e.FullPath, isCreated: true);
                _watcher.Deleted += (s, e) => OnFileCreatedOrDeleted(e.FullPath, isCreated: false);
                _watcher.Renamed += (s, e) => OnFileRenamed(e.OldFullPath, e.FullPath);
            }
            catch
            {
                // FileSystemWatcher might fail on some network/virtual drives
            }
        }

        private void StopWatcher()
        {
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Dispose();
                _watcher = null;
            }
        }

        private void OnFileCreatedOrDeleted(string fullPath, bool isCreated)
        {
            if (!_imageLoader.IsSupportedExtension(Path.GetExtension(fullPath))) return;

            lock (_items)
            {
                if (isCreated)
                {
                    if (!_items.Any(i => string.Equals(i.FilePath, fullPath, StringComparison.OrdinalIgnoreCase)))
                    {
                        _items.Add(new ImageFileItem(fullPath));
                        ApplySorting(_items);
                    }
                }
                else
                {
                    int index = _items.FindIndex(i => string.Equals(i.FilePath, fullPath, StringComparison.OrdinalIgnoreCase));
                    if (index >= 0)
                    {
                        _items.RemoveAt(index);
                        if (_currentIndex >= _items.Count)
                        {
                            _currentIndex = Math.Max(0, _items.Count - 1);
                        }
                    }
                }
            }

            ItemsListChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnFileRenamed(string oldFullPath, string newFullPath)
        {
            lock (_items)
            {
                int index = _items.FindIndex(i => string.Equals(i.FilePath, oldFullPath, StringComparison.OrdinalIgnoreCase));
                if (index >= 0)
                {
                    if (_imageLoader.IsSupportedExtension(Path.GetExtension(newFullPath)))
                    {
                        _items[index] = new ImageFileItem(newFullPath);
                        ApplySorting(_items);
                    }
                    else
                    {
                        _items.RemoveAt(index);
                    }
                }
                else if (_imageLoader.IsSupportedExtension(Path.GetExtension(newFullPath)))
                {
                    _items.Add(new ImageFileItem(newFullPath));
                    ApplySorting(_items);
                }
            }

            ItemsListChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                StopWatcher();
                _isDisposed = true;
            }
        }
    }
}
