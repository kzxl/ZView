using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Microsoft.Win32;

namespace ZView.Core.Services
{
    /// <summary>
    /// Manages Windows Explorer context menu integration for ZView.
    /// Operates purely in HKEY_CURRENT_USER without requiring administrator/UAC elevation.
    /// Supports opening both single image files and directories.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public static class ShellContextMenuService
    {
        public const string SystemImageAssocKey = @"Software\Classes\SystemFileAssociations\image\shell\ZView";
        public const string DirShellKey = @"Software\Classes\Directory\shell\ZView";
        public const string DirBgShellKey = @"Software\Classes\Directory\Background\shell\ZView";

        public static readonly string[] SupportedExtensions =
        {
            ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".ico", ".tiff", ".tif",
            ".wdp", ".hdp", ".jxr", ".webp", ".tga", ".cur", ".dds", ".heic", ".avif",
            ".svg", ".psd", ".dng", ".cr2", ".nef", ".arw", ".raw"
        };

        [DllImport("shell32.dll")]
        private static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

        public static void NotifyShell()
        {
            try
            {
                // SHCNE_ASSOCCHANGED = 0x08000000, SHCNF_IDLIST = 0x0000
                SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
            }
            catch
            {
                // Non-critical notification failure
            }
        }

        public static bool IsRegistered()
        {
            try
            {
                using var imageKey = Registry.CurrentUser.OpenSubKey(SystemImageAssocKey);
                using var dirKey = Registry.CurrentUser.OpenSubKey(DirShellKey);
                return imageKey != null || dirKey != null;
            }
            catch
            {
                return false;
            }
        }

        public static string ResolveExecutablePath(string? customPath = null)
        {
            if (!string.IsNullOrEmpty(customPath) && File.Exists(customPath))
                return Path.GetFullPath(customPath);

            var current = Environment.ProcessPath ?? Process.GetCurrentProcess().MainModule?.FileName;
            if (!string.IsNullOrEmpty(current) && File.Exists(current) && !current.EndsWith("dotnet.exe", StringComparison.OrdinalIgnoreCase))
            {
                return Path.GetFullPath(current);
            }

            string baseDir = AppContext.BaseDirectory;
            string[] probePaths =
            {
                Path.Combine(baseDir, "ZView.exe"),
                Path.Combine(baseDir, "..", "..", "..", "..", "publish", "zview-lite", "ZView.exe"),
                Path.Combine(baseDir, "..", "Release", "net8.0-windows", "win-x64", "ZView.exe")
            };

            foreach (var p in probePaths)
            {
                try
                {
                    string full = Path.GetFullPath(p);
                    if (File.Exists(full)) return full;
                }
                catch { }
            }

            return current ?? "";
        }

        public static bool Register(string? executablePath = null, string? fileVerb = null, string? dirVerb = null)
        {
            try
            {
                var exe = ResolveExecutablePath(executablePath);
                if (string.IsNullOrEmpty(exe) || !File.Exists(exe))
                {
                    return false;
                }

                // Clean existing registrations first
                Unregister();

                string fVerb = fileVerb ?? "Xem bằng ZView";
                string dVerb = dirVerb ?? "Xem ảnh bằng ZView";
                string iconValue = $"\"{exe}\",0";

                // 1. Generic SystemFileAssociations for images
                using (var root = Registry.CurrentUser.CreateSubKey(SystemImageAssocKey))
                {
                    if (root != null)
                    {
                        root.SetValue("", fVerb);
                        root.SetValue("MUIVerb", fVerb);
                        root.SetValue("Icon", iconValue);
                        using var cmd = root.CreateSubKey("command");
                        cmd?.SetValue("", $"\"{exe}\" \"%1\"");
                    }
                }

                // 2. Specific SystemFileAssociations per supported extension
                foreach (var ext in SupportedExtensions)
                {
                    string extKeyPath = $@"Software\Classes\SystemFileAssociations\{ext}\shell\ZView";
                    try
                    {
                        using var extKey = Registry.CurrentUser.CreateSubKey(extKeyPath);
                        if (extKey != null)
                        {
                            extKey.SetValue("", fVerb);
                            extKey.SetValue("MUIVerb", fVerb);
                            extKey.SetValue("Icon", iconValue);
                            using var cmd = extKey.CreateSubKey("command");
                            cmd?.SetValue("", $"\"{exe}\" \"%1\"");
                        }
                    }
                    catch
                    {
                        // Ignore individual extension permission errors
                    }
                }

                // 3. Directory Context Menu
                using (var dirRoot = Registry.CurrentUser.CreateSubKey(DirShellKey))
                {
                    if (dirRoot != null)
                    {
                        dirRoot.SetValue("", dVerb);
                        dirRoot.SetValue("MUIVerb", dVerb);
                        dirRoot.SetValue("Icon", iconValue);
                        using var cmd = dirRoot.CreateSubKey("command");
                        cmd?.SetValue("", $"\"{exe}\" \"%1\"");
                    }
                }

                // 4. Directory Background Context Menu
                using (var dirBgRoot = Registry.CurrentUser.CreateSubKey(DirBgShellKey))
                {
                    if (dirBgRoot != null)
                    {
                        dirBgRoot.SetValue("", dVerb);
                        dirBgRoot.SetValue("MUIVerb", dVerb);
                        dirBgRoot.SetValue("Icon", iconValue);
                        using var cmd = dirBgRoot.CreateSubKey("command");
                        cmd?.SetValue("", $"\"{exe}\" \"%V\"");
                    }
                }

                NotifyShell();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool Unregister()
        {
            try
            {
                // Delete image association
                TryDeleteSubKeyTree(Registry.CurrentUser, @"Software\Classes\SystemFileAssociations\image\shell\ZView");

                // Delete extension associations
                foreach (var ext in SupportedExtensions)
                {
                    TryDeleteSubKeyTree(Registry.CurrentUser, $@"Software\Classes\SystemFileAssociations\{ext}\shell\ZView");
                }

                // Delete directory associations
                TryDeleteSubKeyTree(Registry.CurrentUser, DirShellKey);
                TryDeleteSubKeyTree(Registry.CurrentUser, DirBgShellKey);

                NotifyShell();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void TryDeleteSubKeyTree(RegistryKey root, string subKey)
        {
            try
            {
                root.DeleteSubKeyTree(subKey, throwOnMissingSubKey: false);
            }
            catch
            {
                // Best effort cleanup
            }
        }
    }
}
