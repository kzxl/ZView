using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using ZView.Core.Models;
using ZView.Core.Services;

namespace ZView.Tests
{
    public class FolderNavigationTests : IDisposable
    {
        private readonly string _testDir;
        private readonly FolderNavigationService _nav;
        private readonly ImageLoaderService _loader;

        public FolderNavigationTests()
        {
            _testDir = Path.Combine(Path.GetTempPath(), "ZView_Test_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_testDir);

            // Create sample dummy image files
            File.WriteAllBytes(Path.Combine(_testDir, "a1.jpg"), new byte[] { 1, 2, 3 });
            File.WriteAllBytes(Path.Combine(_testDir, "a2.png"), new byte[] { 1, 2, 3, 4 });
            File.WriteAllBytes(Path.Combine(_testDir, "a10.bmp"), new byte[] { 1 });
            File.WriteAllText(Path.Combine(_testDir, "ignored.txt"), "hello");

            _loader = new ImageLoaderService();
            _nav = new FolderNavigationService(_loader);
        }

        [Fact]
        public void LoadDirectory_ShouldFilterSupportedImagesAndSortNaturally()
        {
            _nav.LoadDirectory(_testDir);

            Assert.Equal(3, _nav.TotalCount);
            // Natural sort order: a1.jpg, a2.png, a10.bmp
            Assert.Equal("a1.jpg", _nav.Items[0].FileName);
            Assert.Equal("a2.png", _nav.Items[1].FileName);
            Assert.Equal("a10.bmp", _nav.Items[2].FileName);
            Assert.Equal(0, _nav.CurrentIndex);
            Assert.Equal("a1.jpg", _nav.CurrentItem?.FileName);
        }

        [Fact]
        public void MoveNext_WithLoopMode_ShouldWrapAround()
        {
            _nav.LoadDirectory(_testDir);
            _nav.LoopMode = LoopMode.Loop;

            Assert.Equal("a1.jpg", _nav.CurrentItem?.FileName);

            _nav.MoveNext();
            Assert.Equal("a2.png", _nav.CurrentItem?.FileName);

            _nav.MoveNext();
            Assert.Equal("a10.bmp", _nav.CurrentItem?.FileName);

            // Wrap around to start
            _nav.MoveNext();
            Assert.Equal("a1.jpg", _nav.CurrentItem?.FileName);
        }

        [Fact]
        public void MovePrevious_WithLoopMode_ShouldWrapToLast()
        {
            _nav.LoadDirectory(_testDir);
            _nav.LoopMode = LoopMode.Loop;

            Assert.Equal(0, _nav.CurrentIndex);
            _nav.MovePrevious();
            Assert.Equal(2, _nav.CurrentIndex);
            Assert.Equal("a10.bmp", _nav.CurrentItem?.FileName);
        }

        [Fact]
        public void StopAtEnds_ShouldNotWrapAround()
        {
            _nav.LoadDirectory(_testDir);
            _nav.LoopMode = LoopMode.StopAtEnds;

            _nav.MovePrevious();
            Assert.Equal(0, _nav.CurrentIndex);

            _nav.MoveLast();
            Assert.Equal(2, _nav.CurrentIndex);
            _nav.MoveNext();
            Assert.Equal(2, _nav.CurrentIndex);
        }

        [Fact]
        public void GetNeighborPaths_ShouldProvideAccurateNeighbors()
        {
            _nav.LoadDirectory(_testDir);
            _nav.CurrentIndex = 0; // a1.jpg
            _nav.LoopMode = LoopMode.Loop;

            var neighbors = _nav.GetNeighborPaths(forwardCount: 1, backwardCount: 1);

            Assert.Contains(Path.Combine(_testDir, "a2.png"), neighbors);
            Assert.Contains(Path.Combine(_testDir, "a10.bmp"), neighbors);
        }

        public void Dispose()
        {
            _nav.Dispose();
            if (Directory.Exists(_testDir))
            {
                try { Directory.Delete(_testDir, true); } catch { }
            }
        }
    }
}
