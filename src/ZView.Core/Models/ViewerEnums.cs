namespace ZView.Core.Models
{
    public enum SortBy
    {
        Name,
        DateModified,
        FileSize,
        Extension
    }

    public enum SortDirection
    {
        Ascending,
        Descending
    }

    public enum ViewerZoomMode
    {
        FitToWindow,
        FitWidth,
        FitHeight,
        ActualSize,
        Custom
    }

    public enum LoopMode
    {
        Loop,
        StopAtEnds
    }
}
