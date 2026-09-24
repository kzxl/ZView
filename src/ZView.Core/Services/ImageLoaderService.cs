using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ZeroPrimitives.Diagnostics;
using ZeroSystem;
using ZView.Core.Models;
using ZView.Core.Utils;

namespace ZView.Core.Services
{
    /// <summary>
    /// Enterprise multi-format image loader using ZeroSystem sovereign permissive streams and WIC hardware acceleration.
    /// Provides zero-lock file access, EXIF auto-rotation, and thread-safe frozen bitmaps.
    /// </summary>
    public class ImageLoaderService : IImageLoaderService
    {
        private static readonly HashSet<string> DefaultExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".ico", ".tiff", ".tif",
            ".wdp", ".hdp", ".jxr", ".webp", ".tga", ".cur", ".dds", ".heic", ".avif"
        };

        public IReadOnlySet<string> SupportedExtensions => DefaultExtensions;

        public bool IsSupportedExtension(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension)) return false;
            if (!extension.StartsWith(".")) extension = "." + extension;
            return DefaultExtensions.Contains(extension);
        }

        public async Task<BitmapSource?> LoadImageAsync(string filePath, bool autoRotate = true, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return null;

            return await Task.Run(() =>
            {
                try
                {
                    ct.ThrowIfCancellationRequested();

                    // Zero-lock sovereign stream via ZeroSystem UnsafeFileStreamFactory
                    using var fileStream = UnsafeFileStreamFactory.OpenPermissiveReadStream(filePath, bufferSize: 64 * 1024);
                    using var memoryStream = new MemoryStream((int)fileStream.Length);
                    fileStream.CopyTo(memoryStream);
                    memoryStream.Position = 0;

                    ct.ThrowIfCancellationRequested();

                    var decoder = BitmapDecoder.Create(
                        memoryStream,
                        BitmapCreateOptions.PreservePixelFormat | BitmapCreateOptions.IgnoreColorProfile,
                        BitmapCacheOption.OnLoad);

                    if (decoder.Frames.Count == 0) return null;

                    BitmapFrame frame = decoder.Frames[0];
                    BitmapSource result = frame;

                    // Handle EXIF orientation
                    if (autoRotate && frame.Metadata is BitmapMetadata meta)
                    {
                        var orientObj = meta.GetQuery("/app1/ifd/{ushort=274}");
                        int orientation = 1;
                        if (orientObj is ushort u) orientation = u;
                        else if (orientObj is int i) orientation = i;

                        result = ApplyOrientation(result, orientation);
                    }

                    if (result.CanFreeze && !result.IsFrozen)
                    {
                        result.Freeze();
                    }

                    return result;
                }
                catch (OperationCanceledException)
                {
                    return null;
                }
                catch
                {
                    // Fallback to simple BitmapImage if decoder fails
                    try
                    {
                        var bi = new BitmapImage();
                        bi.BeginInit();
                        bi.CacheOption = BitmapCacheOption.OnLoad;
                        bi.UriSource = new Uri(filePath, UriKind.Absolute);
                        bi.EndInit();
                        if (bi.CanFreeze) bi.Freeze();
                        return bi;
                    }
                    catch
                    {
                        return null;
                    }
                }
            }, ct).ConfigureAwait(false);
        }

        public async Task<BitmapSource?> LoadThumbnailAsync(string filePath, int targetSize = 160, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return null;

            return await Task.Run(() =>
            {
                try
                {
                    ct.ThrowIfCancellationRequested();

                    using var fileStream = UnsafeFileStreamFactory.OpenPermissiveReadStream(filePath, bufferSize: 16 * 1024);
                    using var memoryStream = new MemoryStream();
                    fileStream.CopyTo(memoryStream);
                    memoryStream.Position = 0;

                    var bi = new BitmapImage();
                    bi.BeginInit();
                    bi.StreamSource = memoryStream;
                    bi.DecodePixelWidth = targetSize;
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.EndInit();

                    if (bi.CanFreeze) bi.Freeze();
                    return (BitmapSource)bi;
                }
                catch
                {
                    return null;
                }
            }, ct).ConfigureAwait(false);
        }

        public ImageMetadataInfo ExtractMetadata(string filePath)
        {
            var sw = ValueStopwatch.StartNew();
            try
            {
                using var stream = UnsafeFileStreamFactory.OpenPermissiveReadStream(filePath, bufferSize: 16 * 1024);
                var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.None);
                var frame = decoder.Frames.Count > 0 ? decoder.Frames[0] : null;
                var info = ExifMetadataReader.ReadMetadata(filePath, frame);
                info.DecodeLatencyMs = Math.Round(sw.GetElapsedTime().TotalMilliseconds, 2);
                return info;
            }
            catch
            {
                var info = ExifMetadataReader.ReadMetadata(filePath, null);
                info.DecodeLatencyMs = Math.Round(sw.GetElapsedTime().TotalMilliseconds, 2);
                return info;
            }

        }

        private static BitmapSource ApplyOrientation(BitmapSource source, int orientation)
        {
            Transform? transform = orientation switch
            {
                2 => new ScaleTransform(-1, 1), // Flip Horizontal
                3 => new RotateTransform(180),  // Rotate 180
                4 => new ScaleTransform(1, -1), // Flip Vertical
                5 => new TransformGroup { Children = { new RotateTransform(90), new ScaleTransform(-1, 1) } },
                6 => new RotateTransform(90),   // Rotate 90 CW
                7 => new TransformGroup { Children = { new RotateTransform(270), new ScaleTransform(-1, 1) } },
                8 => new RotateTransform(270),  // Rotate 270 CW
                _ => null
            };

            if (transform == null) return source;

            var transformed = new TransformedBitmap(source, transform);
            if (transformed.CanFreeze) transformed.Freeze();
            return transformed;
        }
    }
}
