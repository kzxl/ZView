using System;
using System.IO;
using System.Windows.Media.Imaging;
using ZView.Core.Models;

namespace ZView.Core.Utils
{
    /// <summary>
    /// Utility to safely extract EXIF camera and technical metadata from WIC BitmapMetadata.
    /// </summary>
    public static class ExifMetadataReader
    {
        public static ImageMetadataInfo ReadMetadata(string filePath, BitmapFrame? frame = null)
        {
            var info = new ImageMetadataInfo
            {
                FilePath = filePath,
                FileName = Path.GetFileName(filePath),
                DirectoryPath = Path.GetDirectoryName(filePath) ?? string.Empty
            };

            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Exists)
            {
                info.FileSizeBytes = fileInfo.Length;
                info.FileSizeFormatted = FormatBytes(fileInfo.Length);
                info.DateModified = fileInfo.LastWriteTime;
            }

            if (frame != null)
            {
                info.PixelWidth = frame.PixelWidth;
                info.PixelHeight = frame.PixelHeight;
                info.DpiX = Math.Round(frame.DpiX, 1);
                info.DpiY = Math.Round(frame.DpiY, 1);
                info.ColorFormat = frame.Format.ToString();
                info.BitsPerPixel = frame.Format.BitsPerPixel;

                double megapixels = (frame.PixelWidth * (double)frame.PixelHeight) / 1_000_000.0;
                info.Megapixels = $"{megapixels:F2} MP";

                if (frame.PixelHeight > 0)
                {
                    int gcd = GreatestCommonDivisor(frame.PixelWidth, frame.PixelHeight);
                    int aspectW = frame.PixelWidth / gcd;
                    int aspectH = frame.PixelHeight / gcd;
                    if (aspectW <= 16 && aspectH <= 16)
                    {
                        info.AspectRatio = $"{aspectW}:{aspectH}";
                    }
                    else
                    {
                        info.AspectRatio = $"{((double)frame.PixelWidth / frame.PixelHeight):F2}:1";
                    }
                }

                if (frame.Metadata is BitmapMetadata metadata)
                {
                    ExtractExif(metadata, info);
                }
            }

            return info;
        }

        private static void ExtractExif(BitmapMetadata meta, ImageMetadataInfo info)
        {
            try
            {
                info.CameraMake = GetQueryString(meta, "/app1/ifd/{ushort=271}") ?? string.Empty;
                info.CameraModel = GetQueryString(meta, "/app1/ifd/{ushort=272}") ?? string.Empty;
                info.Software = GetQueryString(meta, "/app1/ifd/{ushort=305}") ?? string.Empty;
                info.DateTaken = GetQueryString(meta, "/app1/ifd/exif/{ushort=36867}") ??
                                 GetQueryString(meta, "/app1/ifd/{ushort=306}") ?? string.Empty;

                var orientObj = meta.GetQuery("/app1/ifd/{ushort=274}");
                if (orientObj is ushort ushortVal) info.Orientation = ushortVal;
                else if (orientObj is int intVal) info.Orientation = intVal;

                // Lens Model
                info.LensModel = GetQueryString(meta, "/app1/ifd/exif/{ushort=42036}") ?? string.Empty;

                // ISO
                var isoObj = meta.GetQuery("/app1/ifd/exif/{ushort=34855}");
                if (isoObj != null) info.Iso = $"ISO {isoObj}";

                // F-Number
                var fObj = meta.GetQuery("/app1/ifd/exif/{ushort=33437}");
                if (fObj != null) info.FNumber = FormatFNumber(fObj);

                // Exposure Time (Shutter Speed)
                var expObj = meta.GetQuery("/app1/ifd/exif/{ushort=33434}");
                if (expObj != null) info.ExposureTime = FormatExposureTime(expObj);

                // Focal length
                var focalObj = meta.GetQuery("/app1/ifd/exif/{ushort=37386}");
                if (focalObj != null) info.FocalLength = FormatFocalLength(focalObj);

                info.HasExif = !string.IsNullOrWhiteSpace(info.CameraMake) ||
                               !string.IsNullOrWhiteSpace(info.CameraModel) ||
                               !string.IsNullOrWhiteSpace(info.Iso);
            }
            catch
            {
                // Ignore metadata query failures
            }
        }

        private static string? GetQueryString(BitmapMetadata meta, string query)
        {
            try
            {
                if (meta.ContainsQuery(query))
                {
                    var val = meta.GetQuery(query);
                    return val?.ToString()?.Trim();
                }
            }
            catch { }
            return null;
        }

        private static string FormatFNumber(object fObj)
        {
            if (fObj is double d) return $"f/{d:F1}";
            if (double.TryParse(fObj.ToString(), out double parsed)) return $"f/{parsed:F1}";
            return $"f/{fObj}";
        }

        private static string FormatExposureTime(object expObj)
        {
            if (double.TryParse(expObj.ToString(), out double sec))
            {
                if (sec >= 1.0) return $"{sec:F1}s";
                if (sec > 0)
                {
                    double denom = Math.Round(1.0 / sec);
                    return $"1/{denom}s";
                }
            }
            return $"{expObj}s";
        }

        private static string FormatFocalLength(object focalObj)
        {
            if (double.TryParse(focalObj.ToString(), out double mm))
            {
                return $"{mm:F0} mm";
            }
            return $"{focalObj} mm";
        }

        private static int GreatestCommonDivisor(int a, int b)
        {
            while (b != 0)
            {
                int temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
            if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024.0):F2} MB";
            return $"{bytes / (1024.0 * 1024.0 * 1024.0):F2} GB";
        }
    }
}
