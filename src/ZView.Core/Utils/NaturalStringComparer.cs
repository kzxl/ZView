using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ZView.Core.Utils
{
    /// <summary>
    /// Natural order comparer (e.g., photo1.jpg, photo2.jpg, photo10.jpg) using Windows StrCmpLogicalW.
    /// </summary>
    public sealed class NaturalStringComparer : IComparer<string>
    {
        public static NaturalStringComparer Default { get; } = new NaturalStringComparer();

        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern int StrCmpLogicalW(string psz1, string psz2);

        public int Compare(string? x, string? y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return StrCmpLogicalW(x, y);
                }
            }
            catch
            {
                // Fallback to ordinal ignore case
            }

            return string.Compare(x, y, StringComparison.OrdinalIgnoreCase);
        }
    }
}
