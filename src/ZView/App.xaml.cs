using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ZView.Core.Services;
using ZView.ViewModels;
using ZView.Views;

namespace ZView
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize Localization and Theme
            ZView.Services.ZViewLocalization.Initialize();
            ZeroUI.Core.Localization.LocalizationManager.SetLanguage("vi-VN");
            ZeroUI.Wpf.Theme.ZeroWpfTheme.SetTheme(false); // Default to Clean Light Theme

            // Composition Root
            var imageLoader = new ImageLoaderService();
            var navigation = new FolderNavigationService(imageLoader);
            var cache = ImageCacheService.CreateAdaptive(capacity: 24);
            var vm = new MainViewModel(imageLoader, navigation, cache);

            // Handle --capture CLI argument for automated UI auditing
            for (int i = 0; i < e.Args.Length; i++)
            {
                if (e.Args[i] == "--capture" && i + 1 < e.Args.Length)
                {
                    string targetDir = e.Args[i + 1];
                    RunUiCapture(vm, targetDir);
                    return;
                }
            }

            var mainWindow = new MainWindow(vm);
            mainWindow.Show();

            // Handle command line file arguments
            if (e.Args.Length > 0 && !e.Args[0].StartsWith("--"))
            {
                string path = e.Args[0].Trim('"');
                if (File.Exists(path) || Directory.Exists(path))
                {
                    _ = vm.OpenFileOrDirectoryAsync(path);
                }
            }
        }

        private void RunUiCapture(MainViewModel vm, string outputDir)
        {
            Directory.CreateDirectory(outputDir);

            // 1. Capture Empty State (Light Theme)
            ZeroUI.Wpf.Theme.ZeroWpfTheme.SetTheme(false);
            var win = new MainWindow(vm)
            {
                Width = 1320,
                Height = 840,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            win.Show();
            DoEvents();

            CaptureWindow(win, Path.Combine(outputDir, "zview_empty_light.png"));

            // 2. Load Sample Image and Capture Light Theme
            string sampleImg = @"E:\15. Other\ZeroUniverse\ZeroApps\ZStack\data\macro_flower_result.png";
            if (File.Exists(sampleImg))
            {
                vm.OpenFileOrDirectoryAsync(sampleImg).GetAwaiter().GetResult();
                DoEvents();
                CaptureWindow(win, Path.Combine(outputDir, "zview_image_light.png"));

                // Open Telemetry / EXIF drawer for inspection capture
                vm.IsMetadataDrawerOpen = true;
                DoEvents();
                CaptureWindow(win, Path.Combine(outputDir, "zview_telemetry_light.png"));
                vm.IsMetadataDrawerOpen = false;
            }

            // 3. Dark Theme Capture
            ZeroUI.Wpf.Theme.ZeroWpfTheme.SetTheme(true);
            DoEvents();
            CaptureWindow(win, Path.Combine(outputDir, "zview_image_dark.png"));

            win.Close();
            Shutdown(0);
        }

        private static void DoEvents()
        {
            var frame = new System.Windows.Threading.DispatcherFrame();
            System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Background,
                new System.Action(() => frame.Continue = false));
            System.Windows.Threading.Dispatcher.PushFrame(frame);
            System.Threading.Thread.Sleep(150);
        }

        private static void CaptureWindow(FrameworkElement element, string filePath)
        {
            int w = (int)element.ActualWidth;
            int h = (int)element.ActualHeight;
            if (w <= 0) w = 1320;
            if (h <= 0) h = 840;

            var rtb = new RenderTargetBitmap(w, h, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(element);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            using var fs = File.Create(filePath);
            encoder.Save(fs);
        }
    }
}
