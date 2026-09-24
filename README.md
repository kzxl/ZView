# ZView — Enterprise High-Performance Image Workstation

[![Platform](https://img.shields.io/badge/Platform-.NET%208.0%20WPF-blue.svg)](https://dotnet.microsoft.com/)
[![Architecture](https://img.shields.io/badge/Architecture-Universe%20v4.0-purple.svg)]()
[![Status](https://img.shields.io/badge/Build-Passing-brightgreen.svg)]()

**ZView** is a next-generation, ultra-fast, and lightweight image workstation and viewer for Windows, built on **ZeroUniverse Architecture v4.0** and empowered by the **ZeroUI Media Subsystem** (`ZeroUI.Wpf.Media`).

Drawing foundational insights and ergonomics from renowned image viewers like [ImageGlass](https://github.com/d2phap/ImageGlass), ZView takes performance, memory management, and forensic inspection to industrial standards.

---

## ⚡ Key Architectural Features

### 1. Multi-Tiered Zero-Lock Decoding Engine
- **Hardware-Accelerated WIC Pipeline**: Uses Windows Imaging Component (WIC) native decoders for instant GPU-assisted decoding.
- **Zero-Lock File Streaming**: Reads files through non-blocking memory buffers (`FileShare.ReadWrite`). Never locks images on disk, allowing external modifications, renames, and deletions without OS file collisions.
- **Broad Format Support**:
  - Standard Web & Print: `JPEG`, `PNG`, `BMP`, `GIF`, `ICO`, `TIFF`, `WDP`, `HDP`, `JXR`.
  - Modern / Next-Gen: `WEBP`, `TGA`, `CUR`, `DDS`, `HEIC`, `AVIF`.
- **Automatic EXIF Orientation**: Detects EXIF Orientation tags (1–8) and applies lossless matrix transformations so photos appear upright automatically.

### 2. Smart LRU Memory Caching & Pre-fetching Engine
- **Zero-Latency Navigation (0ms)**: When viewing image $K$, an asynchronous background prefetch worker automatically decodes images $K+1, K+2, K-1, K-2$ into a thread-safe LRU (Least Recently Used) cache.
- **Adaptive Memory Cap**: Ensures low RAM footprint (default 16 high-res frames) with automatic cache trimming to avoid memory pressure during prolonged viewing sessions.

### 3. Industrial Telemetry & Forensic Tools
- **Forensic Pixel Grid**: Automatically exposes sub-pixel boundaries when zooming beyond 800%, essential for computer vision inspection and digital asset quality control.
- **Live Color Probe HUD**: Hover over any pixel to inspect real-time coordinates $(X, Y)$, RGB levels, and Hex color codes without switching to third-party tools.
- **Interactive MiniMap Navigator**: Viewport radar in the corner of the view for effortless spatial orientation when working with high-gigapixel images.
- **Slide-In EXIF Drawer**: View detailed camera metadata (Camera Model, Lens, ISO, F-Number, Shutter Speed, DPI, Aspect Ratio, Megapixels).

### 4. Interactive Bottom Filmstrip
- Horizontal thumbnail strip featuring lazy background loading.
- Instant jumping to any image within the current working directory.
- Toggle visibility seamlessly with hotkey `T`.

---

## ⌨️ Ergonomic Keyboard Shortcuts

| Shortcut | Action Description |
| :--- | :--- |
| **`Right` / `Space`** | Next image in folder |
| **`Left` / `Backspace`** | Previous image in folder |
| **`Home` / `End`** | Jump to first / last image |
| **`Ctrl + MouseWheel`** | Smooth zoom centered on cursor |
| **`Ctrl + 0`** | Fit image to window bounds |
| **`Ctrl + 1`** | Actual size (100% 1:1 scale) |
| **`Ctrl + R`** | Rotate 90° Clockwise |
| **`Ctrl + Shift + R`** | Rotate 90° Counter-Clockwise |
| **`Ctrl + H`** | Flip Horizontally |
| **`Ctrl + V`** | Flip Vertically |
| **`F11`** | Toggle Borderless Fullscreen |
| **`I` / `Alt + Enter`** | Toggle EXIF & Metadata Telemetry Drawer |
| **`T`** | Toggle Bottom Thumbnail Filmstrip |
| **`Ctrl + C`** | Copy Image to Clipboard |
| **`Ctrl + Shift + C`** | Copy Full File Path to Clipboard |
| **`Delete`** | Safely move file to Windows Recycle Bin |
| **`Escape`** | Exit Fullscreen or close active side drawer |

---

## 📂 Project Structure

```
ZView/
├── .project-rule.md                 # Universe Architecture v4.0 metadata
├── Directory.Build.props            # Solution-wide build configurations
├── ZView.slnx                       # Next-gen solution file
├── publish.ps1                      # Optimized Lite publish script
├── src/
│   ├── ZView.Core/                  # Pure domain logic & decoding engines
│   │   ├── Models/                  # File items, metadata, enums, settings
│   │   ├── Services/                # Loader, Folder Navigation, LRU Cache
│   │   └── Utils/                   # NaturalStringComparer, ExifMetadataReader
│   └── ZView/                       # WPF Desktop Presentation
│       ├── ViewModels/              # MainViewModel, RelayCommand
│       ├── Views/                   # MainWindow with Extended TitleBar
│       ├── Converters/              # WPF Value Converters
│       └── Resources/               # Icons & Theming Tokens
└── tests/
    └── ZView.Tests/                 # Unit tests (xUnit) verifying LRU & Sorting
```

---

## 🚀 Building & Publishing

### Build
```bash
dotnet build ZView.slnx
```

### Run Tests
```bash
dotnet test tests/ZView.Tests/ZView.Tests.csproj
```

### Publish Lite (Framework-Dependent Single File)
```powershell
.\publish.ps1
```
The output executable will be created in `publish/zview-lite/ZView.exe`.
