using System;
using System.IO;
using Xunit;
using ZView.Core.Services;

namespace ZView.Tests
{
    public class RecentFilesServiceTests : IDisposable
    {
        private readonly string _tempFile;

        public RecentFilesServiceTests()
        {
            _tempFile = Path.Combine(Path.GetTempPath(), $"zview_test_recent_{Guid.NewGuid():N}.json");
        }

        public void Dispose()
        {
            if (File.Exists(_tempFile)) File.Delete(_tempFile);
        }

        [Fact]
        public void AddRecent_AddsItemsInLruOrder()
        {
            var service = new RecentFilesService(_tempFile);

            // Create temporary test files so they validate
            string f1 = Path.Combine(Path.GetTempPath(), $"t1_{Guid.NewGuid():N}.png");
            string f2 = Path.Combine(Path.GetTempPath(), $"t2_{Guid.NewGuid():N}.png");
            File.WriteAllText(f1, "dummy");
            File.WriteAllText(f2, "dummy");

            try
            {
                service.AddRecent(f1, isDirectory: false);
                service.AddRecent(f2, isDirectory: false);

                var list = service.GetRecentItems();
                Assert.Equal(2, list.Count);
                Assert.Equal(f2, list[0].Path);
                Assert.Equal(f1, list[1].Path);

                // Reload from disk to verify persistence
                var service2 = new RecentFilesService(_tempFile);
                var list2 = service2.GetRecentItems();
                Assert.Equal(2, list2.Count);
                Assert.Equal(f2, list2[0].Path);
            }
            finally
            {
                if (File.Exists(f1)) File.Delete(f1);
                if (File.Exists(f2)) File.Delete(f2);
            }
        }

        [Fact]
        public void Clear_RemovesAllItems()
        {
            var service = new RecentFilesService(_tempFile);
            string f1 = Path.Combine(Path.GetTempPath(), $"t1_{Guid.NewGuid():N}.png");
            File.WriteAllText(f1, "dummy");

            try
            {
                service.AddRecent(f1, isDirectory: false);
                Assert.Single(service.GetRecentItems());

                service.Clear();
                Assert.Empty(service.GetRecentItems());
            }
            finally
            {
                if (File.Exists(f1)) File.Delete(f1);
            }
        }
    }
}
