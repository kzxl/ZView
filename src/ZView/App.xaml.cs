using System;
using System.IO;
using System.Windows;
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

            var mainWindow = new MainWindow(vm);
            mainWindow.Show();

            // Handle command line file arguments
            if (e.Args.Length > 0)
            {
                string path = e.Args[0].Trim('"');
                if (File.Exists(path) || Directory.Exists(path))
                {
                    _ = vm.OpenFileOrDirectoryAsync(path);
                }
            }
        }
    }
}
