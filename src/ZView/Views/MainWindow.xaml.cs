using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ZView.Core.Models;
using ZView.ViewModels;

namespace ZView.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// Pure Ultra-High Performance Image Viewer & Telemetry Inspector.
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _vm;
        private WindowState _previousWindowState = WindowState.Normal;
        private WindowStyle _previousWindowStyle = WindowStyle.SingleBorderWindow;

        public MainWindow(MainViewModel vm)
        {
            InitializeComponent();
            _vm = vm ?? throw new ArgumentNullException(nameof(vm));
            DataContext = _vm;

            _vm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MainViewModel.IsFullScreen))
                {
                    ApplyFullScreenMode(_vm.IsFullScreen);
                }
                else if (e.PropertyName == nameof(MainViewModel.CurrentImageSource))
                {
                    // Center and fit new image on viewport
                    ImageViewer.FitToWindow();
                    if (_vm.CurrentItem != null)
                    {
                        FilmstripList.ScrollIntoView(_vm.CurrentItem);
                    }
                }
            };
        }

        #region Window Caption Buttons

        private void BtnMinimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void BtnMaximize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();

        #endregion

        #region Fullscreen & Drag/Drop

        private void ApplyFullScreenMode(bool isFullScreen)
        {
            if (isFullScreen)
            {
                _previousWindowState = WindowState;
                _previousWindowStyle = WindowStyle;

                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
            }
            else
            {
                WindowStyle = _previousWindowStyle;
                WindowState = _previousWindowState;
            }
        }

        private void Window_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                {
                    _ = _vm.OpenFileOrDirectoryAsync(files[0]);
                }
            }
        }

        #endregion

        #region Keyboard Navigation & Shortcuts

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Escape closes fullscreen or open drawers
            if (e.Key == Key.Escape)
            {
                if (_vm.IsFullScreen)
                {
                    _vm.IsFullScreen = false;
                    e.Handled = true;
                    return;
                }
                if (_vm.IsMetadataDrawerOpen)
                {
                    _vm.IsMetadataDrawerOpen = false;
                    e.Handled = true;
                    return;
                }
                if (_vm.IsSettingsDrawerOpen)
                {
                    _vm.IsSettingsDrawerOpen = false;
                    e.Handled = true;
                    return;
                }
            }

            // Ctrl combinations
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.O:
                        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                            _vm.OpenFolderCommand.Execute(null);
                        else
                            _vm.OpenFileCommand.Execute(null);
                        e.Handled = true;
                        return;
                    case Key.D0:
                    case Key.NumPad0:
                        ImageViewer.FitToWindow();
                        e.Handled = true;
                        return;
                    case Key.D1:
                    case Key.NumPad1:
                        ImageViewer.ActualSize();
                        e.Handled = true;
                        return;
                    case Key.R:
                        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                            ImageViewer.RotateCounterClockwise();
                        else
                            ImageViewer.RotateClockwise();
                        e.Handled = true;
                        return;
                    case Key.H:
                        ImageViewer.ToggleFlipHorizontal();
                        e.Handled = true;
                        return;
                    case Key.V:
                        ImageViewer.ToggleFlipVertical();
                        e.Handled = true;
                        return;
                    case Key.C:
                        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                            _vm.CopyPathCommand.Execute(null);
                        else
                            _vm.CopyImageCommand.Execute(null);
                        e.Handled = true;
                        return;
                    case Key.P:
                        if (_vm.PrintImageCommand.CanExecute(null))
                            _vm.PrintImageCommand.Execute(null);
                        e.Handled = true;
                        return;
                }
            }


            // Single key shortcuts for pure image navigation & view modes
            switch (e.Key)
            {
                case Key.Right:
                case Key.Space:
                case Key.PageDown:
                    if (_vm.NextCommand.CanExecute(null)) _vm.NextCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Key.Left:
                case Key.Back:
                case Key.PageUp:
                    if (_vm.PreviousCommand.CanExecute(null)) _vm.PreviousCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Key.Home:
                    if (_vm.FirstCommand.CanExecute(null)) _vm.FirstCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Key.End:
                    if (_vm.LastCommand.CanExecute(null)) _vm.LastCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Key.F:
                    if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                    {
                        _vm.ToggleFilterCommand.Execute(null);
                        e.Handled = true;
                    }
                    else
                    {
                        ImageViewer.FitToWindow();
                        e.Handled = true;
                    }
                    break;
                case Key.D1:
                    ImageViewer.ActualSize();
                    e.Handled = true;
                    break;
                case Key.R:
                    ImageViewer.RotateClockwise();
                    e.Handled = true;
                    break;
                case Key.H:
                    ImageViewer.ToggleFlipHorizontal();
                    e.Handled = true;
                    break;
                case Key.Z:
                    _vm.ToggleLoupeCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Key.I:
                    _vm.ToggleMetadataCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Key.S:
                    _vm.ToggleSettingsCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Key.B:
                    _vm.ToggleFilmstripCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Key.T:
                    _vm.ToggleThemeCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Key.OemQuestion:
                case Key.F1:
                    _vm.ToggleShortcutDialogCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Key.Escape:
                    if (_vm.IsUpdateDialogOpen) { _vm.IsUpdateDialogOpen = false; e.Handled = true; }
                    else if (_vm.IsShortcutDialogOpen) { _vm.IsShortcutDialogOpen = false; e.Handled = true; }
                    else if (_vm.IsMetadataDrawerOpen) { _vm.IsMetadataDrawerOpen = false; e.Handled = true; }
                    else if (_vm.IsSettingsDrawerOpen) { _vm.IsSettingsDrawerOpen = false; e.Handled = true; }
                    else if (_vm.IsFullScreen) { _vm.IsFullScreen = false; e.Handled = true; }
                    break;
                case Key.Delete:
                    if (_vm.DeleteFileCommand.CanExecute(null)) _vm.DeleteFileCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Key.Add:
                case Key.OemPlus:
                    ImageViewer.ZoomIn();
                    e.Handled = true;
                    break;
                case Key.Subtract:
                case Key.OemMinus:
                    ImageViewer.ZoomOut();
                    e.Handled = true;
                    break;
                case Key.F11:
                    _vm.ToggleFullScreenCommand.Execute(null);
                    e.Handled = true;
                    break;
            }
        }

        private void BtnDownloadUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (_vm.AvailableUpdate != null)
            {
                try
                {
                    string url = !string.IsNullOrEmpty(_vm.AvailableUpdate.AssetDownloadUrl)
                        ? _vm.AvailableUpdate.AssetDownloadUrl
                        : _vm.AvailableUpdate.HtmlUrl;

                    if (!string.IsNullOrEmpty(url))
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
                    }
                }
                catch { }
            }
            _vm.IsUpdateDialogOpen = false;
        }

        #endregion

        #region Toolbar Quick Handlers

        private void BtnFitToWindow_Click(object sender, RoutedEventArgs e) => ImageViewer.FitToWindow();
        private void BtnActualSize_Click(object sender, RoutedEventArgs e) => ImageViewer.ActualSize();
        private void BtnZoomIn_Click(object sender, RoutedEventArgs e) => ImageViewer.ZoomIn();
        private void BtnZoomOut_Click(object sender, RoutedEventArgs e) => ImageViewer.ZoomOut();
        private void BtnRotateCW_Click(object sender, RoutedEventArgs e) => ImageViewer.RotateClockwise();
        private void BtnFlipH_Click(object sender, RoutedEventArgs e) => ImageViewer.ToggleFlipHorizontal();

        private void FilmstripList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FilmstripList.SelectedItem is ImageFileItem item && item != _vm.CurrentItem)
            {
                _vm.SelectItemCommand.Execute(item);
            }
        }

        #endregion
    }
}
