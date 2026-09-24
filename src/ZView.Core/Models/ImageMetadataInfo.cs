using System;

namespace ZView.Core.Models
{
    /// <summary>
    /// Detailed technical and EXIF metadata information for an image.
    /// </summary>
    public class ImageMetadataInfo
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string DirectoryPath { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public string FileSizeFormatted { get; set; } = string.Empty;
        public DateTime DateModified { get; set; }

        public int PixelWidth { get; set; }
        public int PixelHeight { get; set; }
        public double DpiX { get; set; }
        public double DpiY { get; set; }
        public string ColorFormat { get; set; } = string.Empty;
        public int BitsPerPixel { get; set; }
        public string AspectRatio { get; set; } = string.Empty;
        public string Megapixels { get; set; } = string.Empty;

        // EXIF Camera Telemetry
        public bool HasExif { get; set; }
        public string CameraMake { get; set; } = string.Empty;
        public string CameraModel { get; set; } = string.Empty;
        public string LensModel { get; set; } = string.Empty;
        public string Software { get; set; } = string.Empty;
        public string DateTaken { get; set; } = string.Empty;
        public string ExposureTime { get; set; } = string.Empty;
        public string FNumber { get; set; } = string.Empty;
        public string Iso { get; set; } = string.Empty;
        public string FocalLength { get; set; } = string.Empty;
        public string ExposureBias { get; set; } = string.Empty;
        public int Orientation { get; set; } = 1;
        // Telemetry & Hardware Engine Info
        public double DecodeLatencyMs { get; set; }
        public string DecodeEngineInfo { get; set; } = "ZeroSystem · Permissive Kernel I/O";
    }
}


