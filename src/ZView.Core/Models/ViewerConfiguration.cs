namespace ZView.Core.Models
{
    /// <summary>
    /// Configuration options for ZView engine.
    /// </summary>
    public class ViewerConfiguration
    {
        public SortBy SortBy { get; set; } = SortBy.Name;
        public SortDirection SortDirection { get; set; } = SortDirection.Ascending;
        public LoopMode LoopMode { get; set; } = LoopMode.Loop;
        
        public int PreloadCount { get; set; } = 2; // Preload 2 ahead and 2 behind
        public int MaxMemoryCacheCount { get; set; } = 16;
        public bool AutoRotateExif { get; set; } = true;
        
        // Viewer HUD & Overlays
        public bool EnableColorProbe { get; set; } = true;
        public bool EnableMiniMap { get; set; } = true;
        public bool EnablePixelGrid { get; set; } = true;
        public bool ShowFilmstrip { get; set; } = true;
        public bool DarkTheme { get; set; } = true;
    }
}
