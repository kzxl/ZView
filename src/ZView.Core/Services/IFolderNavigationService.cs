using System;
using System.Collections.Generic;
using ZView.Core.Models;

namespace ZView.Core.Services
{
    public interface IFolderNavigationService : IDisposable
    {
        string? CurrentDirectory { get; }
        IReadOnlyList<ImageFileItem> Items { get; }
        int CurrentIndex { get; set; }
        ImageFileItem? CurrentItem { get; }
        int TotalCount { get; }
        
        bool HasNext { get; }
        bool HasPrevious { get; }
        
        SortBy CurrentSortBy { get; }
        SortDirection CurrentSortDirection { get; }
        LoopMode LoopMode { get; set; }

        event EventHandler? CurrentItemChanged;
        event EventHandler? ItemsListChanged;

        void LoadDirectory(string directoryPath, string? selectedFilePath = null);
        void LoadFiles(IEnumerable<string> filePaths, string? selectedFilePath = null);
        
        ImageFileItem? MoveNext();
        ImageFileItem? MovePrevious();
        ImageFileItem? MoveFirst();
        ImageFileItem? MoveLast();
        
        bool SelectByPath(string filePath);
        void SetSorting(SortBy sortBy, SortDirection direction);
        void Refresh();
        
        List<string> GetNeighborPaths(int forwardCount = 2, int backwardCount = 2);
    }
}
