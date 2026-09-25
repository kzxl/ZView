using System;
using System.IO;
using Xunit;
using ZView.Core.Services;

namespace ZView.Tests
{
    public class ShellContextMenuTests
    {
        [Fact]
        public void SupportedExtensions_ContainsCoreImageFormats()
        {
            var exts = ShellContextMenuService.SupportedExtensions;
            Assert.NotNull(exts);
            Assert.Contains(".png", exts);
            Assert.Contains(".jpg", exts);
            Assert.Contains(".webp", exts);
            Assert.Contains(".avif", exts);
            Assert.Contains(".heic", exts);
            Assert.Contains(".svg", exts);
            Assert.Contains(".psd", exts);
            Assert.Contains(".dng", exts);
        }

        [Fact]
        public void ResolveExecutablePath_ReturnsNonEmptyPath()
        {
            string path = ShellContextMenuService.ResolveExecutablePath();
            Assert.False(string.IsNullOrWhiteSpace(path));
        }

        [Fact]
        public void RegisterAndUnregister_CyclesCorrectly()
        {
            // Create a temporary dummy executable file for verification
            string tempDir = Path.Combine(Path.GetTempPath(), "ZViewTest_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);
            string dummyExe = Path.Combine(tempDir, "ZView.exe");
            File.WriteAllText(dummyExe, "Dummy executable binary payload");

            try
            {
                // Register
                bool regResult = ShellContextMenuService.Register(dummyExe, "Test Xem ZView", "Test Xem Thư Mục ZView");
                Assert.True(regResult);
                Assert.True(ShellContextMenuService.IsRegistered());

                // Unregister
                bool unregResult = ShellContextMenuService.Unregister();
                Assert.True(unregResult);
                Assert.False(ShellContextMenuService.IsRegistered());
            }
            finally
            {
                // Ensure clean state even if assertions fail
                ShellContextMenuService.Unregister();
                if (Directory.Exists(tempDir))
                {
                    try { Directory.Delete(tempDir, true); } catch { }
                }
            }
        }
    }
}
