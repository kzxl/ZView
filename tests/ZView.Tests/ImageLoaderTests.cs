using Xunit;
using ZView.Core.Services;

namespace ZView.Tests
{
    public class ImageLoaderTests
    {
        [Theory]
        [InlineData(".jpg", true)]
        [InlineData("jpg", true)]
        [InlineData(".png", true)]
        [InlineData(".webp", true)]
        [InlineData(".tiff", true)]
        [InlineData(".bmp", true)]
        [InlineData(".gif", true)]
        [InlineData(".ico", true)]
        [InlineData(".exe", false)]
        [InlineData(".txt", false)]
        [InlineData(".dll", false)]
        public void IsSupportedExtension_ShouldValidateCorrectly(string extension, bool expected)
        {
            var loader = new ImageLoaderService();
            bool actual = loader.IsSupportedExtension(extension);
            Assert.Equal(expected, actual);
        }
    }
}
