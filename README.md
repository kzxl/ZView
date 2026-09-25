# ZView — Enterprise High-Performance Image Workstation & Telemetry Inspector

[![Platform](https://img.shields.io/badge/Platform-.NET%208.0%20WPF-blue.svg)](https://dotnet.microsoft.com/)
[![Architecture](https://img.shields.io/badge/Architecture-Universe%20v4.0-purple.svg)]()
[![License](https://img.shields.io/badge/License-MIT-green.svg)]()
[![Status](https://img.shields.io/badge/Build-Passing-brightgreen.svg)]()

**ZView** is a standalone, ultra-fast, GPU-accelerated desktop image viewing and forensic telemetry workstation for Windows. Built for photography professionals, computer vision engineers, and digital artists, ZView delivers zero-latency folder navigation, sub-pixel forensic inspection, real-time telemetry extraction, and seamless multi-format decoding.

---

## ⚡ Key Highlights & Architecture

### 1. Multi-Tiered Zero-Lock Decoding Engine
- **Hardware-Accelerated WIC Pipeline**: Leverages native Windows Imaging Component (WIC) decoders for direct GPU-accelerated rasterization.
- **Zero Disk File Locking**: Accesses image streams using non-blocking memory buffers (`FileShare.ReadWrite`). Images are never locked on disk, allowing continuous external editing, renaming, or deletion without Win32 sharing violations.
- **30+ Supported Formats**:
  - *Standard Web & Print*: PNG, JPEG, BMP, GIF, ICO, TIFF, WDP, HDP, JXR, SVG.
  - *Next-Gen Formats*: WebP, AVIF, HEIC/HEIF, DDS, TGA, CUR, PSD, RAW/DNG.
- **Lossless EXIF Matrix Orientation**: Automatically parses EXIF Orientation flags (1–8) and applies lossless matrix transforms for upright rendering.

### 2. Multi-Directional LRU Cache & Background Prefetching
- **Instant Folder Traversal (0ms)**: When inspecting image $K$, an asynchronous worker thread pre-decodes adjacent frames $K+1, K+2, K-1, K-2$ directly into a thread-safe Least-Recently-Used (LRU) memory cache.
- **Adaptive Memory Management**: Automatically monitors system RAM thresholds to maintain a steady memory footprint during prolonged viewing sessions with tens of thousands of assets.

### 3. Forensic Telemetry & Inspection Suite
- **Floating Loupe HUD (Z)**: Real-time high-magnification floating loupe following the cursor for fine-grained inspection.
- **Forensic Sub-Pixel Grid**: Automatically overlays crisp pixel boundary grids when zooming above 800%.
- **Live RGB & Hex Color Probe**: Instantaneous pixel color sampling with coordinates $(X, Y)$, 8-bit RGBA channel levels, and Hex codes.
- **MiniMap Radar Navigator**: Viewport orientation radar for smooth navigation across multi-gigapixel captures.
- **Slide-In EXIF Drawer (I)**: Full telemetry readout including camera model, lens metadata, ISO, shutter speed, aperture, DPI, aspect ratio, and color depth.

### 4. Workflow Productivity & Windows Shell Integration
- **Explorer Context Menu Integration**: Seamlessly registers into Windows Explorer context menus for all 30+ image formats and folders without requiring UAC/Administrator rights (`HKEY_CURRENT_USER`).
- **Quick Filmstrip & Filter Bar (B / Ctrl+F)**: Bottom carousel with real-time substring filtering and instant directory jump.
- **Native Direct Printing (Ctrl+P)**: Built-in document scaling and printer dialog integration.
- **Internationalization (i18n)**: Multi-language support (English, Vietnamese, Japanese) via modular JSON locale definitions.
- **Automated Update Notification**: Built-in GitHub Releases checker with notification pill badge.
- **CLI Automation Support**:
  - `ZView.exe <path>`: Directly inspect image file or directory.
  - `ZView.exe --register-context`: Headless registration of Explorer right-click context menu.
  - `ZView.exe --unregister-context`: Headless removal of Explorer right-click context menu.

---

## ⌨️ Ergonomic Keyboard Shortcuts

| Shortcut | Action Description |
| :--- | :--- |
| **`Right` / `Space` / `PageDown`** | Next image in current directory |
| **`Left` / `Backspace` / `PageUp`** | Previous image in current directory |
| **`Home` / `End`** | Jump to first / last image |
| **`Ctrl + MouseWheel`** | Smooth zoom centered on cursor |
| **`F` / `Ctrl + 0`** | Fit image to viewport bounds |
| **`1` / `Ctrl + 1`** | Actual size 100% (1:1 scale) |
| **`R` / `Ctrl + R`** | Rotate 90° Clockwise |
| **`L` / `Ctrl + Shift + R`** | Rotate 90° Counter-Clockwise |
| **`H` / `Ctrl + H`** | Flip Horizontally |
| **`V` / `Ctrl + V`** | Flip Vertically |
| **`Z`** | Toggle Floating Loupe HUD |
| **`Ctrl + F` / `/`** | Toggle Filmstrip Search & Quick Filter |
| **`B`** | Toggle Bottom Filmstrip Carousel |
| **`I` / `Alt + Enter`** | Toggle EXIF & Telemetry Drawer |
| **`S`** | Toggle Settings & Display Options Drawer |
| **`Ctrl + P`** | Open Direct Print Dialog |
| **`Ctrl + C`** | Copy Image to Clipboard |
| **`Ctrl + Shift + C`** | Copy Full Image File Path |
| **`Delete`** | Safely send current file to Recycle Bin |
| **`F11`** | Toggle Borderless Fullscreen Mode |
| **`?` / `F1`** | Open Keyboard Shortcuts Cheat Sheet |
| **`Escape`** | Close active drawers, dialogs, or exit fullscreen |

---

## 📂 Project Architecture

```
ZView/
├── .project-rule.md                 # Architecture rules & metadata
├── Directory.Build.props            # Build configuration properties
├── ZView.slnx                       # Solution file
├── publish.ps1                      # Automated Lite single-file publish script
├── src/
│   ├── ZView.Core/                  # Domain contracts, models & services
│   │   ├── Models/                  # ImageFileItem, ImageMetadataInfo, UpdateInfo
│   │   ├── Services/                # ImageLoader, Caching, Navigation, UpdateChecker
│   │   └── Utils/                   # NaturalStringComparer, ExifMetadataReader
│   └── ZView/                       # WPF Presentation Layer
│       ├── Languages/               # Localization bundles (en-US, vi-VN, ja-JP)
│       ├── ViewModels/              # MainViewModel & RelayCommand
│       ├── Views/                   # MainWindow with Extended TitleBar & Drawers
│       └── Converters/              # WPF Value Converters
└── tests/
    └── ZView.Tests/                 # Unit test suite (xUnit)
```

---

## 🚀 Building & Publishing

### Build Solution
```bash
dotnet build ZView.slnx
```

### Run Unit Tests
```bash
dotnet test tests/ZView.Tests/ZView.Tests.csproj
```

### Publish Lite (Framework-Dependent Single-File Executable)
```powershell
.\publish.ps1
```
The optimized single-file binary will be generated at `publish/zview-lite/ZView.exe`.

---

## 📄 License
Released under the [MIT License](LICENSE).
