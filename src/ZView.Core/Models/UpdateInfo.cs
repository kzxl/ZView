using System;

namespace ZView.Core.Models
{
    public class UpdateInfo
    {
        public bool HasUpdate { get; set; }
        public string LatestVersion { get; set; } = string.Empty;
        public string CurrentVersion { get; set; } = string.Empty;
        public string ReleaseTitle { get; set; } = string.Empty;
        public string ReleaseNotes { get; set; } = string.Empty;
        public string HtmlUrl { get; set; } = string.Empty;
        public string? AssetDownloadUrl { get; set; }
        public string? AssetFileName { get; set; }
        public long AssetSizeBytes { get; set; }
        public DateTime PublishedAt { get; set; }
    }
}
