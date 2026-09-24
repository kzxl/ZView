using System.Collections.Generic;
using Xunit;
using ZView.Core.Utils;

namespace ZView.Tests
{
    public class NaturalStringComparerTests
    {
        [Fact]
        public void Compare_ShouldSortNumbersNaturally()
        {
            var list = new List<string>
            {
                "photo100.jpg",
                "photo2.jpg",
                "photo1.jpg",
                "photo20.jpg",
                "photo10.jpg"
            };

            list.Sort(NaturalStringComparer.Default);

            var expected = new List<string>
            {
                "photo1.jpg",
                "photo2.jpg",
                "photo10.jpg",
                "photo20.jpg",
                "photo100.jpg"
            };

            Assert.Equal(expected, list);
        }

        [Fact]
        public void Compare_ShouldHandleNullsAndEquivalence()
        {
            var comparer = NaturalStringComparer.Default;

            Assert.Equal(0, comparer.Compare(null, null));
            Assert.True(comparer.Compare(null, "a") < 0);
            Assert.True(comparer.Compare("a", null) > 0);
            Assert.Equal(0, comparer.Compare("image.png", "IMAGE.PNG"));
        }
    }
}
