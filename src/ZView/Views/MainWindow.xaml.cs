using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using ZeroUI.Core.Media;
using ZView.Core.Models;
using ZView.ViewModels;

namespace ZView.Views
{
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
                    // Center and fit new image
                    ImageViewer.FitToWindow();
                    if (_vm.CurrentItem != null)
                    {
                        FilmstripList.ScrollIntoView(_vm.CurrentItem);
                    }
                }
            };
        }

        #region Window Chrome Buttons

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

        #region Keyboard Ergonomics

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Fullscreen toggle
            if (e.Key == Key.F11)
            {
                _vm.ToggleFullScreenCommand.Execute(null);
                e.Handled = true;
                return;
            }

            // Escape closes fullscreen, modes or side drawer
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
                if (_vm.IsAnnotationMode) { _vm.IsAnnotationMode = false; e.Handled = true; return; }
                if (_vm.IsMeasurementMode) { _vm.IsMeasurementMode = false; e.Handled = true; return; }
                if (_vm.IsDeskewMode) { _vm.IsDeskewMode = false; e.Handled = true; return; }
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
                    case Key.S:
                        SaveCurrentActiveExport();
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
                }
            }

            // Single key shortcuts
            switch (e.Key)
            {
                case Key.Right:
                case Key.Space:
                    if (_vm.NextCommand.CanExecute(null)) _vm.NextCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Key.Left:
                case Key.Back:
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
                case Key.A:
                    _vm.ToggleAnnotationCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Key.M:
                    _vm.ToggleMeasurementCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Key.W:
                    _vm.ToggleWatermarkCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Key.D:
                    _vm.ToggleDeskewCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Key.I:
                    _vm.ToggleMetadataCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Key.T:
                    _vm.ToggleFilmstripCommand.Execute(null);
                    e.Handled = true;
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
            }
        }

        #endregion

        #region Toolbar Action Handlers

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

        #region Contextual Sub-Bar Handlers

        // Annotation Tools
        private void BtnSetToolBox_Click(object sender, RoutedEventArgs e) => _vm.AnnotationTool = AnnotationShapeType.BoundingBox;
        private void BtnSetToolArrow_Click(object sender, RoutedEventArgs e) => _vm.AnnotationTool = AnnotationShapeType.Arrow;
        private void BtnSetToolEllipse_Click(object sender, RoutedEventArgs e) => _vm.AnnotationTool = AnnotationShapeType.Ellipse;
        private void BtnSetToolCallout_Click(object sender, RoutedEventArgs e) => _vm.AnnotationTool = AnnotationShapeType.TextCallout;
        private void BtnSetToolBlur_Click(object sender, RoutedEventArgs e) => _vm.AnnotationTool = AnnotationShapeType.BlurPixelate;

        private void BtnSetSeverityOk_Click(object sender, RoutedEventArgs e) => _vm.AnnotationSeverity = AnnotationSeverity.Ok;
        private void BtnSetSeverityWarning_Click(object sender, RoutedEventArgs e) => _vm.AnnotationSeverity = AnnotationSeverity.Warning;
        private void BtnSetSeverityDefect_Click(object sender, RoutedEventArgs e) => _vm.AnnotationSeverity = AnnotationSeverity.Defect;
        private void BtnSetSeverityCritical_Click(object sender, RoutedEventArgs e) => _vm.AnnotationSeverity = AnnotationSeverity.Critical;

        private void BtnSaveAnnotatedImage_Click(object sender, RoutedEventArgs e)
        {
            if (_vm.CurrentImageSource == null) return;
            var burned = AnnotationLayer.BurnAnnotationsToBitmap(_vm.CurrentImageSource);
            SaveBitmapToFile(burned, "annotated");
        }

        // Measurement Modes & Units
        private void BtnMeasureLinear_Click(object sender, RoutedEventArgs e) => _vm.MeasurementMode = MeasurementMode.LinearDistance;
        private void BtnMeasureAngle_Click(object sender, RoutedEventArgs e) => _vm.MeasurementMode = MeasurementMode.ThreePointAngle;
        private void BtnUnitMm_Click(object sender, RoutedEventArgs e) => _vm.MeasurementUnit = MeasurementUnit.Millimeter;
        private void BtnUnitUm_Click(object sender, RoutedEventArgs e) => _vm.MeasurementUnit = MeasurementUnit.Micrometer;
        private void BtnUnitPx_Click(object sender, RoutedEventArgs e) => _vm.MeasurementUnit = MeasurementUnit.Pixel;

        // Watermark Export
        private void BtnSaveWatermarkedImage_Click(object sender, RoutedEventArgs e)
        {
            if (_vm.CurrentImageSource == null) return;
            var burned = WatermarkLayer.BurnWatermarkToBitmap(_vm.CurrentImageSource);
            SaveBitmapToFile(burned, "watermarked");
        }

        // Deskew Export
        private void BtnResetDeskew_Click(object sender, RoutedEventArgs e) => _vm.DeskewAngle = 0.0;

        private void BtnExportDeskewedImage_Click(object sender, RoutedEventArgs e)
        {
            var processed = DeskewLayer.GetProcessedBitmap();
            if (processed != null) SaveBitmapToFile(processed, "deskewed");
        }

        private void SaveCurrentActiveExport()
        {
            if (_vm.IsAnnotationMode) BtnSaveAnnotatedImage_Click(this, new RoutedEventArgs());
            else if (_vm.IsWatermarkMode) BtnSaveWatermarkedImage_Click(this, new RoutedEventArgs());
            else if (_vm.IsDeskewMode) BtnExportDeskewedImage_Click(this, new RoutedEventArgs());
            else if (_vm.CurrentImageSource != null) SaveBitmapToFile(_vm.CurrentImageSource, "export");
        }

        private void SaveBitmapToFile(BitmapSource bitmap, string suffix)
        {
            string origName = _vm.CurrentItem?.FileName ?? "image.png";
            string baseName = Path.GetFileNameWithoutExtension(origName);

            var sfd = new SaveFileDialog
            {
                Title = "Export Image — ZView",
                FileName = $"{baseName}_{suffix}.png",
                Filter = "PNG Image (*.png)|*.png|JPEG Image (*.jpg)|*.jpg|Bitmap Image (*.bmp)|*.bmp"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    using var stream = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write);
                    BitmapEncoder encoder = Path.GetExtension(sfd.FileName).ToLowerInvariant() switch
                    {
                        ".jpg" or ".jpeg" => new JpegBitmapEncoder { QualityLevel = 95 },
                        ".bmp" => new BmpBitmapEncoder(),
                        _ => new PngBitmapEncoder()
                    };

                    encoder.Frames.Add(BitmapFrame.Create(bitmap));
                    encoder.Save(stream);
                    _vm.ShowOsd($"💾 Exported: {Path.GetFileName(sfd.FileName)}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to save image: {ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #endregion
    }
}
