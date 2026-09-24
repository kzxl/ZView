using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using ZeroUI.Core.Media;
using ZView.Core.Models;
using ZView.Core.Services;

namespace ZView.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IImageLoaderService _imageLoader;
        private readonly IFolderNavigationService _navigation;
        private readonly IImageCacheService _cache;

        private BitmapSource? _currentImageSource;
        private ImageMetadataInfo? _currentMetadata;
        private bool _isLoading;
        private bool _isFullScreen;
        private bool _isMetadataDrawerOpen;
        private bool _isFilmstripVisible = true;
        private bool _isColorProbeEnabled = true;
        private bool _isMiniMapEnabled = true;
        private bool _isPixelGridEnabled = true;
        private bool _isCheckerboardVisible = true;
        private double _currentZoom = 1.0;
        private string _statusText = "Ready";
        private string _osdText = string.Empty;
        private bool _isOsdVisible;
        private CancellationTokenSource? _loadCts;
        private CancellationTokenSource? _prefetchCts;

        // Interactive Specialized Modes
        private bool _isAnnotationMode;
        private AnnotationShapeType _annotationTool = AnnotationShapeType.BoundingBox;
        private AnnotationSeverity _annotationSeverity = AnnotationSeverity.Defect;

        private bool _isMeasurementMode;
        private MeasurementMode _measurementMode = MeasurementMode.LinearDistance;
        private MeasurementUnit _measurementUnit = MeasurementUnit.Millimeter;
        private double _calibrationFactor = 1.0;

        private bool _isWatermarkMode;
        private string _watermarkText = "CONFIDENTIAL · {User} · {Date}";
        private WatermarkPlacement _watermarkPlacement = WatermarkPlacement.DiagonalTiled;
        private double _watermarkOpacity = 0.25;

        private bool _isDeskewMode;
        private double _deskewAngle = 0.0;
        private bool _isBinarizationEnabled;
        private byte _binarizationThreshold = 128;

        // Theme and Localization
        private bool _isDarkMode = false;
        private bool _isVietnamese = true;

        public ObservableCollection<ImageFileItem> Items { get; } = new();

        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                if (SetProperty(ref _isDarkMode, value))
                {
                    ZeroUI.Wpf.Theme.ZeroWpfTheme.SetTheme(value);
                    ShowOsd(value ? "🌙 Obsidian Dark Theme" : "☀️ Clean Light Theme");
                }
            }
        }

        public bool IsVietnamese
        {
            get => _isVietnamese;
            set
            {
                if (SetProperty(ref _isVietnamese, value))
                {
                    ZeroUI.Core.Localization.LocalizationManager.SetLanguage(value ? "vi-VN" : "en-US");
                    ShowOsd(value ? "🇻🇳 Tiếng Việt" : "🇺🇸 English");
                    OnPropertyChanged(nameof(TitleText));
                }
            }
        }

        public BitmapSource? CurrentImageSource
        {
            get => _currentImageSource;
            set => SetProperty(ref _currentImageSource, value);
        }

        public ImageMetadataInfo? CurrentMetadata
        {
            get => _currentMetadata;
            set => SetProperty(ref _currentMetadata, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool IsFullScreen
        {
            get => _isFullScreen;
            set => SetProperty(ref _isFullScreen, value);
        }

        public bool IsMetadataDrawerOpen
        {
            get => _isMetadataDrawerOpen;
            set => SetProperty(ref _isMetadataDrawerOpen, value);
        }

        public bool IsFilmstripVisible
        {
            get => _isFilmstripVisible;
            set => SetProperty(ref _isFilmstripVisible, value);
        }

        public bool IsColorProbeEnabled
        {
            get => _isColorProbeEnabled;
            set => SetProperty(ref _isColorProbeEnabled, value);
        }

        public bool IsMiniMapEnabled
        {
            get => _isMiniMapEnabled;
            set => SetProperty(ref _isMiniMapEnabled, value);
        }

        public bool IsPixelGridEnabled
        {
            get => _isPixelGridEnabled;
            set => SetProperty(ref _isPixelGridEnabled, value);
        }

        public bool IsCheckerboardVisible
        {
            get => _isCheckerboardVisible;
            set => SetProperty(ref _isCheckerboardVisible, value);
        }

        public double CurrentZoom
        {
            get => _currentZoom;
            set
            {
                if (SetProperty(ref _currentZoom, value))
                {
                    OnPropertyChanged(nameof(ZoomPercentageText));
                }
            }
        }

        public string ZoomPercentageText => $"{Math.Round(_currentZoom * 100)}%";

        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        public string OsdText
        {
            get => _osdText;
            set => SetProperty(ref _osdText, value);
        }

        public bool IsOsdVisible
        {
            get => _isOsdVisible;
            set => SetProperty(ref _isOsdVisible, value);
        }

        #region Interactive Modes Properties

        public bool IsAnnotationMode
        {
            get => _isAnnotationMode;
            set
            {
                if (SetProperty(ref _isAnnotationMode, value) && value)
                {
                    IsMeasurementMode = false;
                    IsDeskewMode = false;
                    ShowOsd("✏️ Annotation Mode Activated");
                }
            }
        }

        public AnnotationShapeType AnnotationTool
        {
            get => _annotationTool;
            set => SetProperty(ref _annotationTool, value);
        }

        public AnnotationSeverity AnnotationSeverity
        {
            get => _annotationSeverity;
            set => SetProperty(ref _annotationSeverity, value);
        }

        public bool IsMeasurementMode
        {
            get => _isMeasurementMode;
            set
            {
                if (SetProperty(ref _isMeasurementMode, value) && value)
                {
                    IsAnnotationMode = false;
                    IsDeskewMode = false;
                    ShowOsd("📐 Optical Caliper & Measurement Mode Activated");
                }
            }
        }

        public MeasurementMode MeasurementMode
        {
            get => _measurementMode;
            set => SetProperty(ref _measurementMode, value);
        }

        public MeasurementUnit MeasurementUnit
        {
            get => _measurementUnit;
            set => SetProperty(ref _measurementUnit, value);
        }

        public double CalibrationFactor
        {
            get => _calibrationFactor;
            set => SetProperty(ref _calibrationFactor, Math.Max(0.000001, value));
        }

        public bool IsWatermarkMode
        {
            get => _isWatermarkMode;
            set
            {
                if (SetProperty(ref _isWatermarkMode, value))
                {
                    ShowOsd(value ? "🏷️ Watermark Overlay Enabled" : "Watermark Disabled");
                }
            }
        }

        public string WatermarkText
        {
            get => _watermarkText;
            set => SetProperty(ref _watermarkText, value);
        }

        public WatermarkPlacement WatermarkPlacement
        {
            get => _watermarkPlacement;
            set => SetProperty(ref _watermarkPlacement, value);
        }

        public double WatermarkOpacity
        {
            get => _watermarkOpacity;
            set => SetProperty(ref _watermarkOpacity, Math.Max(0.01, Math.Min(1.0, value)));
        }

        public bool IsDeskewMode
        {
            get => _isDeskewMode;
            set
            {
                if (SetProperty(ref _isDeskewMode, value) && value)
                {
                    IsAnnotationMode = false;
                    IsMeasurementMode = false;
                    ShowOsd("📄 Document Deskew & OCR Preprocessing Activated");
                }
            }
        }

        public double DeskewAngle
        {
            get => _deskewAngle;
            set => SetProperty(ref _deskewAngle, Math.Max(-45.0, Math.Min(45.0, value)));
        }

        public bool IsBinarizationEnabled
        {
            get => _isBinarizationEnabled;
            set => SetProperty(ref _isBinarizationEnabled, value);
        }

        public byte BinarizationThreshold
        {
            get => _binarizationThreshold;
            set => SetProperty(ref _binarizationThreshold, value);
        }

        #endregion

        public ImageFileItem? CurrentItem => _navigation.CurrentItem;
        public int CurrentIndex => _navigation.CurrentIndex + 1;
        public int TotalCount => _navigation.TotalCount;
        public string TitleText => CurrentItem != null
            ? $"{CurrentItem.FileName} ({CurrentIndex}/{TotalCount}) — ZView"
            : "ZView — Enterprise Image Workstation";

        // Navigation & Interaction Commands
        public ICommand OpenFileCommand { get; }
        public ICommand OpenFolderCommand { get; }
        public ICommand NextCommand { get; }
        public ICommand PreviousCommand { get; }
        public ICommand FirstCommand { get; }
        public ICommand LastCommand { get; }
        public ICommand SelectItemCommand { get; }
        public ICommand ToggleFullScreenCommand { get; }
        public ICommand ToggleMetadataCommand { get; }
        public ICommand ToggleFilmstripCommand { get; }
        public ICommand ToggleAnnotationCommand { get; }
        public ICommand ToggleMeasurementCommand { get; }
        public ICommand ToggleWatermarkCommand { get; }
        public ICommand ToggleDeskewCommand { get; }
        public ICommand ToggleThemeCommand { get; }
        public ICommand ToggleLanguageCommand { get; }
        public ICommand OpenSkinStudioCommand { get; }
        public ICommand CopyImageCommand { get; }
        public ICommand CopyPathCommand { get; }
        public ICommand DeleteFileCommand { get; }
        public ICommand RefreshCommand { get; }

        public MainViewModel(IImageLoaderService imageLoader, IFolderNavigationService navigation, IImageCacheService cache)
        {
            _imageLoader = imageLoader ?? throw new ArgumentNullException(nameof(imageLoader));
            _navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));

            _navigation.CurrentItemChanged += OnNavigationCurrentItemChanged;
            _navigation.ItemsListChanged += OnNavigationItemsListChanged;

            OpenFileCommand = new RelayCommand(OpenFile);
            OpenFolderCommand = new RelayCommand(OpenFolder);
            NextCommand = new RelayCommand(() => _navigation.MoveNext(), () => _navigation.HasNext);
            PreviousCommand = new RelayCommand(() => _navigation.MovePrevious(), () => _navigation.HasPrevious);
            FirstCommand = new RelayCommand(() => _navigation.MoveFirst(), () => _navigation.TotalCount > 0);
            LastCommand = new RelayCommand(() => _navigation.MoveLast(), () => _navigation.TotalCount > 0);
            SelectItemCommand = new RelayCommand(param =>
            {
                if (param is ImageFileItem item)
                {
                    _navigation.SelectByPath(item.FilePath);
                }
            });

            ToggleFullScreenCommand = new RelayCommand(() => IsFullScreen = !IsFullScreen);
            ToggleMetadataCommand = new RelayCommand(() => IsMetadataDrawerOpen = !IsMetadataDrawerOpen);
            ToggleFilmstripCommand = new RelayCommand(() => IsFilmstripVisible = !IsFilmstripVisible);
            ToggleAnnotationCommand = new RelayCommand(() => IsAnnotationMode = !IsAnnotationMode);
            ToggleMeasurementCommand = new RelayCommand(() => IsMeasurementMode = !IsMeasurementMode);
            ToggleWatermarkCommand = new RelayCommand(() => IsWatermarkMode = !IsWatermarkMode);
            ToggleDeskewCommand = new RelayCommand(() => IsDeskewMode = !IsDeskewMode);

            ToggleThemeCommand = new RelayCommand(() => IsDarkMode = !IsDarkMode);
            ToggleLanguageCommand = new RelayCommand(() => IsVietnamese = !IsVietnamese);
            OpenSkinStudioCommand = new RelayCommand(() =>
            {
                try
                {
                    var dlg = new ZeroUI.Wpf.Theme.SkinStudioDialog();
                    dlg.Owner = Application.Current?.MainWindow;
                    dlg.ShowDialog();
                }
                catch (Exception ex)
                {
                    ShowOsd($"Skin Studio Error: {ex.Message}");
                }
            });

            CopyImageCommand = new RelayCommand(CopyImageToClipboard, () => CurrentImageSource != null);
            CopyPathCommand = new RelayCommand(CopyPathToClipboard, () => CurrentItem != null);
            DeleteFileCommand = new RelayCommand(DeleteCurrentFile, () => CurrentItem != null);
            RefreshCommand = new RelayCommand(() => _navigation.Refresh());
        }

        public async Task OpenFileOrDirectoryAsync(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;

            if (File.Exists(path))
            {
                string? dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir))
                {
                    _navigation.LoadDirectory(dir, path);
                }
                else
                {
                    _navigation.LoadFiles(new[] { path }, path);
                }
            }
            else if (Directory.Exists(path))
            {
                _navigation.LoadDirectory(path);
            }
        }

        private void OpenFile()
        {
            var dlg = new OpenFileDialog
            {
                Title = "Open Image — ZView",
                Filter = "All Supported Images|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.webp;*.ico;*.tiff;*.tif;*.wdp;*.hdp;*.jxr;*.svg;*.tga|All Files (*.*)|*.*",
                CheckFileExists = true
            };

            if (dlg.ShowDialog() == true)
            {
                _ = OpenFileOrDirectoryAsync(dlg.FileName);
            }
        }

        private void OpenFolder()
        {
            var dlg = new OpenFolderDialog
            {
                Title = "Select Image Folder — ZView"
            };

            if (dlg.ShowDialog() == true)
            {
                _ = OpenFileOrDirectoryAsync(dlg.FolderName);
            }
        }

        private void OnNavigationItemsListChanged(object? sender, EventArgs e)
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                Items.Clear();
                foreach (var item in _navigation.Items)
                {
                    Items.Add(item);
                }
                OnPropertyChanged(nameof(TotalCount));
                OnPropertyChanged(nameof(TitleText));
            });

            // Lazy load thumbnails in background
            _ = Task.Run(async () =>
            {
                foreach (var item in _navigation.Items)
                {
                    if (item.Thumbnail == null)
                    {
                        var thumb = await _imageLoader.LoadThumbnailAsync(item.FilePath).ConfigureAwait(false);
                        if (thumb != null)
                        {
                            item.Thumbnail = thumb;
                            Application.Current?.Dispatcher.Invoke(() => item.Thumbnail = thumb);
                        }
                    }
                }
            });
        }

        private async void OnNavigationCurrentItemChanged(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(CurrentItem));
            OnPropertyChanged(nameof(CurrentIndex));
            OnPropertyChanged(nameof(TotalCount));
            OnPropertyChanged(nameof(TitleText));

            var item = CurrentItem;
            if (item == null)
            {
                CurrentImageSource = null;
                CurrentMetadata = null;
                StatusText = "No images found";
                return;
            }

            ShowOsd($"{item.FileName}  [{CurrentIndex} / {TotalCount}]");

            // Cancel any pending load
            _loadCts?.Cancel();
            _loadCts = new CancellationTokenSource();
            var ct = _loadCts.Token;

            // Check cache first for 0ms response
            if (_cache.TryGet(item.FilePath, out var cachedBitmap) && cachedBitmap != null)
            {
                CurrentImageSource = cachedBitmap;
                IsLoading = false;
                UpdateMetadataAndStatus(item, cachedBitmap);
            }
            else
            {
                IsLoading = true;
                try
                {
                    var loadedBitmap = await _imageLoader.LoadImageAsync(item.FilePath, autoRotate: true, ct).ConfigureAwait(true);
                    if (loadedBitmap != null && !ct.IsCancellationRequested)
                    {
                        CurrentImageSource = loadedBitmap;
                        _cache.Put(item.FilePath, loadedBitmap);
                        UpdateMetadataAndStatus(item, loadedBitmap);
                    }
                }
                catch (OperationCanceledException) { }
                finally
                {
                    if (!ct.IsCancellationRequested) IsLoading = false;
                }
            }

            // Trigger background pre-fetching for neighbors
            PrefetchNeighbors();
        }

        private void PrefetchNeighbors()
        {
            _prefetchCts?.Cancel();
            _prefetchCts = new CancellationTokenSource();
            var ct = _prefetchCts.Token;

            var neighbors = _navigation.GetNeighborPaths(forwardCount: 2, backwardCount: 2);
            _ = Task.Run(() => _cache.PrefetchAsync(neighbors, _imageLoader, ct), ct);
        }

        private void UpdateMetadataAndStatus(ImageFileItem item, BitmapSource bitmap)
        {
            _ = Task.Run(() =>
            {
                var meta = _imageLoader.ExtractMetadata(item.FilePath);
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    CurrentMetadata = meta;
                    StatusText = $"{CurrentIndex} / {TotalCount}  |  {bitmap.PixelWidth} × {bitmap.PixelHeight} ({meta.AspectRatio})  |  {meta.Megapixels}  |  {item.FormattedSize}  |  {item.Extension.ToUpperInvariant().TrimStart('.')}";
                });
            });
        }

        private void CopyImageToClipboard()
        {
            if (CurrentImageSource != null)
            {
                try
                {
                    Clipboard.SetImage(CurrentImageSource);
                    ShowOsd("📋 Image copied to clipboard");
                }
                catch { }
            }
        }

        private void CopyPathToClipboard()
        {
            if (CurrentItem != null)
            {
                try
                {
                    Clipboard.SetText(CurrentItem.FilePath);
                    ShowOsd("📁 File path copied to clipboard");
                }
                catch { }
            }
        }

        private void DeleteCurrentFile()
        {
            if (CurrentItem == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to move this file to Recycle Bin?\n\n{CurrentItem.FileName}",
                "Confirm Delete — ZView",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    string pathToDelete = CurrentItem.FilePath;
                    // Safe delete to Recycle Bin
                    Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(
                        pathToDelete,
                        Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
                        Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);

                    _cache.Remove(pathToDelete);
                    _navigation.Refresh();
                    ShowOsd("🗑 Moved to Recycle Bin");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to delete file: {ex.Message}", "Delete Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void ShowOsd(string text)
        {
            OsdText = text;
            IsOsdVisible = true;

            _ = Task.Run(async () =>
            {
                await Task.Delay(2000).ConfigureAwait(false);
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    if (OsdText == text) IsOsdVisible = false;
                });
            });
        }
    }
}
