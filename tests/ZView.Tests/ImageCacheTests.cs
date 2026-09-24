using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;
using ZView.Core.Services;

namespace ZView.Tests
{
    public class ImageCacheTests
    {
        private static BitmapSource CreateTestBitmap(int width = 10, int height = 10)
        {
            var wb = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr32, null);
            wb.Freeze();
            return wb;
        }

        [Fact]
        public void LruCache_ShouldRespectCapacityAndEvictOldest()
        {
            var cache = new ImageCacheService(capacity: 3);

            var b1 = CreateTestBitmap();
            var b2 = CreateTestBitmap();
            var b3 = CreateTestBitmap();
            var b4 = CreateTestBitmap();

            cache.Put("p1", b1);
            cache.Put("p2", b2);
            cache.Put("p3", b3);

            Assert.Equal(3, cache.Count);
            Assert.True(cache.TryGet("p1", out _));

            // Now p2 is LRU because p1 was accessed recently
            cache.Put("p4", b4);

            Assert.Equal(3, cache.Count);
            Assert.True(cache.TryGet("p1", out _));
            Assert.True(cache.TryGet("p4", out _));
            Assert.True(cache.TryGet("p3", out _));
            Assert.False(cache.TryGet("p2", out _)); // p2 was evicted
        }

        [Fact]
        public void MemoryBudget_ShouldEvictWhenExceedingBytes()
        {
            // 10x10 BGRA32 bitmap is 400 bytes.
            // With budget of 900 bytes, it can hold 2 bitmaps (800 bytes). A 3rd bitmap (total 1200 > 900) will evict the oldest.
            var cache = new ImageCacheService(capacity: 10, maxMemoryBytes: 900);

            var b1 = CreateTestBitmap(10, 10);
            var b2 = CreateTestBitmap(10, 10);
            var b3 = CreateTestBitmap(10, 10);

            cache.Put("p1", b1);
            cache.Put("p2", b2);
            cache.Put("p3", b3);

            Assert.True(cache.Count <= 2);
            Assert.False(cache.TryGet("p1", out _)); // p1 evicted due to memory limit
            Assert.True(cache.TryGet("p3", out _));
        }
    }
}

