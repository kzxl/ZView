using System;

namespace ZView.Core.Models
{
    public class RecentFileEntry
    {
        public string Path { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool IsDirectory { get; set; }
        public DateTime LastOpened { get; set; } = DateTime.UtcNow;
        public string FormattedSize { get; set; } = string.Empty;
        public string IconGlyph => IsDirectory ? "📁" : "📄";
    }
}
