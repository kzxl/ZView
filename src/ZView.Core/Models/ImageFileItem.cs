using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Media.Imaging;

namespace ZView.Core.Models
{
    /// <summary>
    /// Represents an image file item discovered in the active directory.
    /// Supports asynchronous thumbnail binding with INotifyPropertyChanged.
    /// </summary>
    public class ImageFileItem : INotifyPropertyChanged
    {
        private BitmapSource? _thumbnail;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string FilePath { get; }
        public string FileName { get; }
        public string DirectoryPath { get; }
        public long FileSizeBytes { get; }
        public DateTime DateModified { get; }
        public string Extension { get; }

        public BitmapSource? Thumbnail
        {
            get => _thumbnail;
            set
            {
                if (!ReferenceEquals(_thumbnail, value))
                {
                    if (value != null && !value.IsFrozen && value.CanFreeze)
                    {
                        value.Freeze();
                    }
                    _thumbnail = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Thumbnail)));
                }
            }
        }
        public int PixelWidth { get; set; }
        public int PixelHeight { get; set; }

        public string FormattedSize
        {
            get
            {
                if (FileSizeBytes < 1024) return $"{FileSizeBytes} B";
                if (FileSizeBytes < 1024 * 1024) return $"{FileSizeBytes / 1024.0:F1} KB";
                if (FileSizeBytes < 1024 * 1024 * 1024) return $"{FileSizeBytes / (1024.0 * 1024.0):F2} MB";
                return $"{FileSizeBytes / (1024.0 * 1024.0 * 1024.0):F2} GB";
            }
        }

        public ImageFileItem(string filePath)
        {
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            var fileInfo = new FileInfo(filePath);
            FileName = fileInfo.Name;
            DirectoryPath = fileInfo.DirectoryName ?? string.Empty;
            FileSizeBytes = fileInfo.Exists ? fileInfo.Length : 0;
            DateModified = fileInfo.Exists ? fileInfo.LastWriteTime : DateTime.MinValue;
            Extension = fileInfo.Extension.ToLowerInvariant();
        }

        public override string ToString() => FileName;
    }
}
