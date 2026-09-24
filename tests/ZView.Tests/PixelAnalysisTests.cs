using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;
using ZView.Core.Utils;

namespace ZView.Tests
{
    public class PixelAnalysisTests
    {
        [Fact]
        public void GetPixelColor_ShouldExtractCorrectRgbAndHex()
        {
            // Create a 2x2 bitmap with known pixels: Red, Green, Blue, White
            var wb = new WriteableBitmap(2, 2, 96, 96, PixelFormats.Bgra32, null);
            byte[] rawPixels = new byte[]
            {
                // Top-Left: Red (B=0, G=0, R=255, A=255)
                0, 0, 255, 255,
                // Top-Right: Green (B=0, G=255, R=0, A=255)
                0, 255, 0, 255,
                // Bottom-Left: Blue (B=255, G=0, R=0, A=255)
                255, 0, 0, 255,
                // Bottom-Right: White (B=255, G=255, R=255, A=255)
                255, 255, 255, 255
            };
            wb.WritePixels(new System.Windows.Int32Rect(0, 0, 2, 2), rawPixels, 2 * 4, 0);
            wb.Freeze();

            var redPixel = PixelAnalysis.GetPixelColor(wb, 0, 0);
            Assert.Equal(255, redPixel.R);
            Assert.Equal(0, redPixel.G);
            Assert.Equal(0, redPixel.B);
            Assert.Equal("#FF0000", redPixel.Hex);
            Assert.Equal(0.0, redPixel.Hue);
            Assert.Equal(100.0, redPixel.Saturation);

            var greenPixel = PixelAnalysis.GetPixelColor(wb, 1, 0);
            Assert.Equal(0, greenPixel.R);
            Assert.Equal(255, greenPixel.G);
            Assert.Equal(0, greenPixel.B);
            Assert.Equal("#00FF00", greenPixel.Hex);
            Assert.Equal(120.0, greenPixel.Hue);

            var bluePixel = PixelAnalysis.GetPixelColor(wb, 0, 1);
            Assert.Equal(0, bluePixel.R);
            Assert.Equal(0, bluePixel.G);
            Assert.Equal(255, bluePixel.B);
            Assert.Equal("#0000FF", bluePixel.Hex);
            Assert.Equal(240.0, bluePixel.Hue);
        }

        [Fact]
        public void CalculateHistogram_ShouldDistributeBinsCorrectly()
        {
            var wb = new WriteableBitmap(4, 4, 96, 96, PixelFormats.Bgra32, null);
            byte[] pixels = new byte[4 * 4 * 4];
            for (int i = 0; i < pixels.Length; i += 4)
            {
                pixels[i] = 50;     // B
                pixels[i + 1] = 100; // G
                pixels[i + 2] = 200; // R
                pixels[i + 3] = 255; // A
            }
            wb.WritePixels(new System.Windows.Int32Rect(0, 0, 4, 4), pixels, 4 * 4, 0);
            wb.Freeze();

            var hist = PixelAnalysis.CalculateHistogram(wb);
            Assert.Equal(16, hist.TotalSampledPixels);
            Assert.Equal(16, hist.RedBins[200]);
            Assert.Equal(16, hist.GreenBins[100]);
            Assert.Equal(16, hist.BlueBins[50]);
            Assert.True(hist.AverageLuminance > 0);
        }
    }
}
