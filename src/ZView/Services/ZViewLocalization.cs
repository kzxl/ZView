using System;
using System.Collections.Generic;
using ZeroUI.Core.Localization;

namespace ZView.Services
{
    /// <summary>
    /// Centralized localization dictionary registration for ZView Image Workstation.
    /// Provides 100% complete bilingual (Vietnamese / English) dictionary for UI strings.
    /// </summary>
    public static class ZViewLocalization
    {
        private static bool _isInitialized;

        public static void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            var viDict = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                // General & Window
                ["ZView.Title"] = "ZView — Trạm Quan Sát & Phân Tích Ảnh Chuyên Sâu",
                ["ZView.Brand"] = "ZVIEW WORKSTATION",
                ["ZView.NoImage"] = "Chưa nạp ảnh",
                ["ZView.Empty.DragDrop"] = "Kéo thả ảnh hoặc thư mục vào đây để duyệt",
                ["ZView.Empty.SupportedFormats"] = "Hỗ trợ 30+ định dạng ảnh: PNG, JPG, WEBP, AVIF, HEIC, TIFF, SVG, BMP, ICO, TGA, PSD, DNG...",
                ["ZView.Empty.BrowseButton"] = "Chọn tệp ảnh...",
                ["ZView.Empty.RecentFiles"] = "TỆP & THƯ MỤC MỞ GẦN ĐÂY",

                // Navigation & Primary Actions
                ["ZView.Nav.Open"] = "Mở tệp ảnh (Ctrl+O)",
                ["ZView.Nav.OpenBtn"] = "Mở ảnh",
                ["ZView.Nav.OpenFolder"] = "Mở thư mục ảnh (Ctrl+Shift+O)",
                ["ZView.Nav.OpenFolderBtn"] = "Thư mục",
                ["ZView.Nav.Prev"] = "Ảnh trước (← / PageUp)",
                ["ZView.Nav.Next"] = "Ảnh sau (→ / PageDown)",
                ["ZView.Nav.Save"] = "Lưu / Xuất ảnh nướng lớp (Ctrl+S)",
                ["ZView.Nav.Info"] = "Thông số & Siêu dữ liệu EXIF (I)",
                ["ZView.Nav.Fit"] = "Vừa khung (F)",
                ["ZView.Nav.ActualSize"] = "Kích thước gốc 100% (1)",
                ["ZView.Nav.RotateLeft"] = "Xoay trái 90° (L)",
                ["ZView.Nav.FlipH"] = "Lật ngang (H)",
                ["ZView.Nav.FlipV"] = "Lật dọc (V)",
                ["ZView.Nav.Filmstrip"] = "Bật/Tắt dải phim thumbnail (B)",
                ["ZView.Nav.Print"] = "In ảnh trực tiếp (Ctrl+P)",
                ["ZView.Nav.Fullscreen"] = "Toàn màn hình (F11)",

                ["ZView.Nav.Theme"] = "Chuyển giao diện (Sáng / Tối)",
                ["ZView.Nav.SkinStudio"] = "Bảng màu Skin Studio",
                ["ZView.Nav.Language"] = "Ngôn ngữ / Language (VI / EN)",
                ["ZView.Nav.Settings"] = "Cài đặt & Tùy chọn hiển thị (S)",
                ["ZView.Nav.Help"] = "Phím tắt & Hướng dẫn (?)",

                // Settings & Options Drawer
                ["ZView.Settings.Title"] = "CÀI ĐẶT & TÙY CHỌN HIỂN THỊ",
                ["ZView.Settings.FilmstripGroup"] = "DẢI PHIM THUMBNAIL PHÍA DƯỚI",
                ["ZView.Settings.ShowFilmstrip"] = "Hiển thị thanh dải phim (B)",
                ["ZView.Settings.ShowFileName"] = "Hiển thị tên tệp dưới thẻ thumbnail",
                ["ZView.Settings.ThumbnailSize"] = "Kích thước thẻ thumbnail:",
                ["ZView.Settings.HudGroup"] = "CÔNG CỤ HỖ TRỢ TRỰC QUAN (HUD)",
                ["ZView.Settings.MiniMap"] = "Bản đồ điều hướng radar (MiniMap)",
                ["ZView.Settings.PixelGrid"] = "Lưới điểm ảnh pháp y (Pixel Grid)",
                ["ZView.Settings.SystemGroup"] = "TÍCH HỢP HỆ THỐNG (WINDOWS SHELL)",
                ["ZView.Settings.ContextMenu"] = "Menu chuột phải Windows Explorer",
                ["ZView.Settings.ContextMenuDesc"] = "Tích hợp tùy chọn 'Xem bằng ZView' vào menu ngữ cảnh chuột phải của tệp ảnh và thư mục",
                ["ZView.Settings.ContextMenuRegister"] = "Đăng ký Menu",
                ["ZView.Settings.ContextMenuUnregister"] = "Hủy đăng ký",
                ["ZView.ContextMenu.FileVerb"] = "Xem bằng ZView",
                ["ZView.ContextMenu.DirVerb"] = "Xem ảnh bằng ZView",
                ["ZView.ContextMenu.RegisteredSuccess"] = "Đã đăng ký menu chuột phải Windows Explorer thành công!",
                ["ZView.ContextMenu.UnregisteredSuccess"] = "Đã hủy đăng ký menu chuột phải thành công!",

                // Interactive Modes
                ["ZView.Mode.Annotate"] = "Ghi chú & Đo đạc lỗi (A)",
                ["ZView.Mode.Measure"] = "Thước Caliper quang học (M)",
                ["ZView.Mode.Watermark"] = "Thủy ấn bảo mật (W)",
                ["ZView.Mode.Deskew"] = "Cân góc phẳng & OCR (D)",

                // Sub-bar: Annotation
                ["ZView.Annotate.Title"] = "CHẾ ĐỘ GHI CHÚ:",
                ["ZView.Annotate.Box"] = "Khung chữ nhật",
                ["ZView.Annotate.Arrow"] = "Mũi tên chỉ thị",
                ["ZView.Annotate.Ellipse"] = "Hình Elip",
                ["ZView.Annotate.Callout"] = "Ghi chú chú giải",
                ["ZView.Annotate.Mask"] = "Mặt nạ che mờ",
                ["ZView.Annotate.SeverityLabel"] = "Mức độ:",
                ["ZView.Annotate.Severity.Info"] = "Thông tin",
                ["ZView.Annotate.Severity.Minor"] = "Nhẹ",
                ["ZView.Annotate.Severity.Major"] = "Nặng",
                ["ZView.Annotate.Severity.Critical"] = "Nghiêm trọng",
                ["ZView.Annotate.Export"] = "Xuất ảnh ghi chú...",

                // Sub-bar: Measurement
                ["ZView.Measure.Title"] = "THƯỚC ĐO QUANG HỌC:",
                ["ZView.Measure.Linear"] = "Khoảng cách (2 điểm)",
                ["ZView.Measure.Angle"] = "Đo góc (3 điểm)",
                ["ZView.Measure.Unit"] = "Đơn vị:",
                ["ZView.Measure.Calib"] = "Hệ số Calib (px/unit):",

                // Sub-bar: Watermark
                ["ZView.Watermark.Title"] = "THỦY ẤN BẢO MẬT:",
                ["ZView.Watermark.TextLabel"] = "Nội dung:",
                ["ZView.Watermark.Placement"] = "Vị trí:",
                ["ZView.Watermark.Opacity"] = "Độ mờ:",
                ["ZView.Watermark.Export"] = "Xuất ảnh đóng dấu...",

                // Sub-bar: Deskew
                ["ZView.Deskew.Title"] = "CÂN GÓC & XỬ LÝ QUÉT:",
                ["ZView.Deskew.Angle"] = "Góc xoay:",
                ["ZView.Deskew.Reset"] = "Đặt lại 0°",
                ["ZView.Deskew.Binarize"] = "Xem trước nhị phân OCR",
                ["ZView.Deskew.Threshold"] = "Ngưỡng:",
                ["ZView.Deskew.Export"] = "Xuất ảnh thẳng góc...",

                // Telemetry & Inspector Drawer
                ["ZView.Drawer.Title"] = "SIÊU DỮ LIỆU & THÔNG SỐ KỸ THUẬT",
                ["ZView.Drawer.BasicInfo"] = "THÔNG TIN TỆP CƠ BẢN",
                ["ZView.Drawer.FileName"] = "Tên tệp:",
                ["ZView.Drawer.Path"] = "Đường dẫn:",
                ["ZView.Drawer.Resolution"] = "Độ phân giải:",
                ["ZView.Drawer.FileSize"] = "Dung lượng:",
                ["ZView.Drawer.Format"] = "Định dạng:",
                ["ZView.Drawer.ColorDepth"] = "Độ sâu màu:",
                ["ZView.Drawer.ExifData"] = "THÔNG SỐ MÁY ẢNH (EXIF)",
                ["ZView.Drawer.Camera"] = "Thiết bị chụp:",
                ["ZView.Drawer.Lens"] = "Ống kính / Tiêu cự:",
                ["ZView.Drawer.Exposure"] = "Thời gian phơi sáng:",
                ["ZView.Drawer.Aperture"] = "Khẩu độ:",
                ["ZView.Drawer.Iso"] = "Độ nhạy sáng ISO:",
                ["ZView.Drawer.DateTaken"] = "Thời gian ghi hình:",
                ["ZView.Drawer.Dpi"] = "Mật độ điểm ảnh:",
                ["ZView.Drawer.Histogram"] = "PHÂN BỐ BIỂU ĐỒ SẮC ĐỘ",

                // Status bar
                ["ZView.Status.Ready"] = "Sẵn sàng",
                ["ZView.Status.Loading"] = "Đang tải ảnh...",
                ["ZView.Status.Zoom"] = "Tỉ lệ:",
                ["ZView.Status.Dimensions"] = "Kích thước:",
                ["ZView.Status.Size"] = "Dung lượng:",
                ["ZView.Status.FileCount"] = "Số lượng:"
            };

            var enDict = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                // General & Window
                ["ZView.Title"] = "ZView — Enterprise Image Workstation & Inspection Studio",
                ["ZView.Brand"] = "ZVIEW WORKSTATION",
                ["ZView.NoImage"] = "No Image Loaded",
                ["ZView.Empty.DragDrop"] = "Drag & Drop images or folder here to view",
                ["ZView.Empty.SupportedFormats"] = "Supported 30+ formats: PNG, JPG, WEBP, AVIF, HEIC, TIFF, SVG, BMP, ICO, TGA, PSD, DNG...",
                ["ZView.Empty.BrowseButton"] = "Browse Image Files...",
                ["ZView.Empty.RecentFiles"] = "RECENTLY OPENED IMAGES & FOLDERS",

                // Navigation & Primary Actions
                ["ZView.Nav.Open"] = "Open Image File (Ctrl+O)",
                ["ZView.Nav.OpenBtn"] = "Open Image",
                ["ZView.Nav.OpenFolder"] = "Open Folder (Ctrl+Shift+O)",
                ["ZView.Nav.OpenFolderBtn"] = "Folder",
                ["ZView.Nav.Prev"] = "Previous Image (← / PageUp)",
                ["ZView.Nav.Next"] = "Next Image (→ / PageDown)",
                ["ZView.Nav.Save"] = "Save / Burn Export (Ctrl+S)",
                ["ZView.Nav.Info"] = "Image Telemetry & EXIF (I)",
                ["ZView.Nav.Fit"] = "Fit Viewport (F)",
                ["ZView.Nav.ActualSize"] = "Actual Size 100% (1)",
                ["ZView.Nav.RotateLeft"] = "Rotate 90° Left (L)",
                ["ZView.Nav.RotateRight"] = "Rotate 90° Right (R)",
                ["ZView.Nav.FlipH"] = "Flip Horizontal (H)",
                ["ZView.Nav.FlipV"] = "Flip Vertical (V)",
                ["ZView.Nav.Filmstrip"] = "Toggle Filmstrip (B)",
                ["ZView.Nav.Print"] = "Print Image (Ctrl+P)",
                ["ZView.Nav.Fullscreen"] = "Toggle Fullscreen (F11)",

                ["ZView.Nav.Theme"] = "Toggle Theme (Light / Dark)",
                ["ZView.Nav.SkinStudio"] = "Skin Studio Palette",
                ["ZView.Nav.Language"] = "Language / Ngôn ngữ (EN / VI)",
                ["ZView.Nav.Settings"] = "Settings & Display Options (S)",
                ["ZView.Nav.Help"] = "Shortcuts & Guide (?)",

                // Settings & Options Drawer
                ["ZView.Settings.Title"] = "VIEWPORT & DISPLAY SETTINGS",
                ["ZView.Settings.FilmstripGroup"] = "BOTTOM FILMSTRIP CAROUSEL",
                ["ZView.Settings.ShowFilmstrip"] = "Show Filmstrip Bar (B)",
                ["ZView.Settings.ShowFileName"] = "Show file names below thumbnails",
                ["ZView.Settings.ThumbnailSize"] = "Thumbnail card size:",
                ["ZView.Settings.HudGroup"] = "ON-SCREEN DISPLAY (HUD)",
                ["ZView.Settings.MiniMap"] = "MiniMap Navigator Radar",
                ["ZView.Settings.PixelGrid"] = "Forensic Pixel Grid",
                ["ZView.Settings.SystemGroup"] = "SYSTEM INTEGRATION (WINDOWS SHELL)",
                ["ZView.Settings.ContextMenu"] = "Windows Explorer Context Menu",
                ["ZView.Settings.ContextMenuDesc"] = "Add 'Open with ZView' option to right-click context menu for images and folders",
                ["ZView.Settings.ContextMenuRegister"] = "Register Menu",
                ["ZView.Settings.ContextMenuUnregister"] = "Unregister",
                ["ZView.ContextMenu.FileVerb"] = "Open with ZView",
                ["ZView.ContextMenu.DirVerb"] = "Open folder in ZView",
                ["ZView.ContextMenu.RegisteredSuccess"] = "Successfully registered into Windows Explorer context menu!",
                ["ZView.ContextMenu.UnregisteredSuccess"] = "Successfully removed from Windows Explorer context menu!",

                // Interactive Modes
                ["ZView.Mode.Annotate"] = "Visual Annotation (A)",
                ["ZView.Mode.Measure"] = "Optical Caliper (M)",
                ["ZView.Mode.Watermark"] = "Security Watermark (W)",
                ["ZView.Mode.Deskew"] = "Deskew & OCR Preview (D)",

                // Sub-bar: Annotation
                ["ZView.Annotate.Title"] = "ANNOTATION MODE:",
                ["ZView.Annotate.Box"] = "Bounding Box",
                ["ZView.Annotate.Arrow"] = "Indicator Arrow",
                ["ZView.Annotate.Ellipse"] = "Ellipse",
                ["ZView.Annotate.Callout"] = "Text Callout",
                ["ZView.Annotate.Mask"] = "Redaction Mask",
                ["ZView.Annotate.SeverityLabel"] = "Severity:",
                ["ZView.Annotate.Severity.Info"] = "Info",
                ["ZView.Annotate.Severity.Minor"] = "Minor",
                ["ZView.Annotate.Severity.Major"] = "Major",
                ["ZView.Annotate.Severity.Critical"] = "Critical",
                ["ZView.Annotate.Export"] = "Export Annotated...",

                // Sub-bar: Measurement
                ["ZView.Measure.Title"] = "OPTICAL CALIPER:",
                ["ZView.Measure.Linear"] = "Distance (2-Point)",
                ["ZView.Measure.Angle"] = "Angle (3-Point)",
                ["ZView.Measure.Unit"] = "Unit:",
                ["ZView.Measure.Calib"] = "Calib (px/unit):",

                // Sub-bar: Watermark
                ["ZView.Watermark.Title"] = "SECURITY WATERMARK:",
                ["ZView.Watermark.TextLabel"] = "Text:",
                ["ZView.Watermark.Placement"] = "Placement:",
                ["ZView.Watermark.Opacity"] = "Opacity:",
                ["ZView.Watermark.Export"] = "Export Watermarked...",

                // Sub-bar: Deskew
                ["ZView.Deskew.Title"] = "DESKEW & SCAN OCR:",
                ["ZView.Deskew.Angle"] = "Angle:",
                ["ZView.Deskew.Reset"] = "Reset 0°",
                ["ZView.Deskew.Binarize"] = "Binarize OCR Preview",
                ["ZView.Deskew.Threshold"] = "Threshold:",
                ["ZView.Deskew.Export"] = "Export Deskewed...",

                // Telemetry & Inspector Drawer
                ["ZView.Drawer.Title"] = "IMAGE METADATA & TELEMETRY",
                ["ZView.Drawer.BasicInfo"] = "BASIC FILE METRICS",
                ["ZView.Drawer.FileName"] = "File Name:",
                ["ZView.Drawer.Path"] = "File Path:",
                ["ZView.Drawer.Resolution"] = "Resolution:",
                ["ZView.Drawer.FileSize"] = "File Size:",
                ["ZView.Drawer.Format"] = "Image Format:",
                ["ZView.Drawer.ColorDepth"] = "Color Depth:",
                ["ZView.Drawer.ExifData"] = "CAMERA & SENSOR METRICS (EXIF)",
                ["ZView.Drawer.Camera"] = "Camera Model:",
                ["ZView.Drawer.Lens"] = "Lens / Focal:",
                ["ZView.Drawer.Exposure"] = "Exposure Time:",
                ["ZView.Drawer.Aperture"] = "Aperture:",
                ["ZView.Drawer.Iso"] = "ISO Speed:",
                ["ZView.Drawer.DateTaken"] = "Date Taken:",
                ["ZView.Drawer.Dpi"] = "Pixel Density (DPI):",
                ["ZView.Drawer.Histogram"] = "HISTOGRAM DISTRIBUTION",

                // Status bar
                ["ZView.Status.Ready"] = "Ready",
                ["ZView.Status.Loading"] = "Loading image...",
                ["ZView.Status.Zoom"] = "Zoom:",
                ["ZView.Status.Dimensions"] = "Dimensions:",
                ["ZView.Status.Size"] = "Size:",
                ["ZView.Status.FileCount"] = "Items:"
            };

            LocalizationManager.RegisterTable("vi-VN", viDict);
            LocalizationManager.RegisterTable("vi", viDict);
            LocalizationManager.RegisterTable("en-US", enDict);
            LocalizationManager.RegisterTable("en", enDict);

            // Dynamically load / overwrite with external JSON language files if present in Languages/ directory
            try
            {
                string appDir = AppDomain.CurrentDomain.BaseDirectory;
                string langDir = System.IO.Path.Combine(appDir, "Languages");
                if (System.IO.Directory.Exists(langDir))
                {
                    LocalizationManager.LoadFromDirectory(langDir, "*.json");
                }
            }
            catch { }
        }
    }
}

