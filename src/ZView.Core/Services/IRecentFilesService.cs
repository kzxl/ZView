using System.Collections.Generic;
using ZView.Core.Models;

namespace ZView.Core.Services
{
    public interface IRecentFilesService
    {
        IReadOnlyList<RecentFileEntry> GetRecentItems();
        void AddRecent(string path, bool isDirectory);
        void RemoveRecent(string path);
        void Clear();
    }
}
