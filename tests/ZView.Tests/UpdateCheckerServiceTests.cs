using System;
using System.Reflection;
using Xunit;
using ZView.Core.Services;

namespace ZView.Tests
{
    public class UpdateCheckerServiceTests
    {
        [Theory]
        [InlineData("1.1.0", "1.0.0", true)]
        [InlineData("2.0.0", "1.9.9", true)]
        [InlineData("1.0.0", "1.0.0", false)]
        [InlineData("1.0.0", "1.1.0", false)]
        [InlineData("1.10.0", "1.9.0", true)]
        public void IsNewerVersion_CorrectlyComparesVersions(string latest, string current, bool expected)
        {
            var method = typeof(GitHubUpdateCheckerService).GetMethod("IsNewerVersion", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(method);

            var result = (bool)method.Invoke(null, new object[] { latest, current })!;
            Assert.Equal(expected, result);
        }
    }
}
