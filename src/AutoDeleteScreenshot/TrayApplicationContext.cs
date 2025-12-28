using System.Drawing;

namespace AutoDeleteScreenshot;

/// <summary>
/// ApplicationContext để quản lý System Tray icon và menu
/// </summary>
public class TrayApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _trayIcon;
    private readonly ContextMenuStrip _contextMenu;
    private readonly ScreenshotWatcher _screenshotWatcher;
    private readonly FileCleanupService _fileCleanupService;
    private readonly SettingsManager _settingsManager;
    
    // Menu items cho thời gian xóa
    private readonly ToolStripMenuItem _menuNoDelete;
    private readonly ToolStripMenuItem _menu15Min;
    private readonly ToolStripMenuItem _menu30Min;
    private readonly ToolStripMenuItem _menu1Hour;
    private readonly ToolStripMenuItem _menu24Hours;
    private readonly ToolStripMenuItem _menuShowToast;
    
    // Thời gian xóa hiện tại (phút), 0 = không xóa
    private int _deleteAfterMinutes = 30;
    private bool _showToast = false;

    public TrayApplicationContext()
    {
        // Load settings từ file
        _settingsManager = new SettingsManager();
        _deleteAfterMinutes = _settingsManager.DeleteAfterMinutes;
        _showToast = _settingsManager.ShowToast;
        
        // Khởi tạo PathHelper với settings
        PathHelper.Initialize(_settingsManager);
        
        // Kiểm tra xem đã có folder chưa, nếu chưa thì yêu cầu chọn
        if (!_settingsManager.HasScreenshotsPath)
        {
            PromptForScreenshotsFolder();
        }
        
        // Tạo context menu
        _contextMenu = new ContextMenuStrip();
        
        // Header
        var header = new ToolStripLabel("⏱️ Auto Delete Screenshot")
        {
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };
        _contextMenu.Items.Add(header);
        _contextMenu.Items.Add(new ToolStripSeparator());
        
        // Time options
        _menuNoDelete = new ToolStripMenuItem("No auto-delete", null, OnDeleteTimeChanged) { Tag = 0 };
        _menu15Min = new ToolStripMenuItem("15 minutes", null, OnDeleteTimeChanged) { Tag = 15 };
        _menu30Min = new ToolStripMenuItem("30 minutes", null, OnDeleteTimeChanged) { Tag = 30, Checked = true };
        _menu1Hour = new ToolStripMenuItem("1 hour", null, OnDeleteTimeChanged) { Tag = 60 };
        _menu24Hours = new ToolStripMenuItem("24 hours", null, OnDeleteTimeChanged) { Tag = 1440 };
        
        _contextMenu.Items.Add(_menuNoDelete);
        _contextMenu.Items.Add(_menu15Min);
        _contextMenu.Items.Add(_menu30Min);
        _contextMenu.Items.Add(_menu1Hour);
        _contextMenu.Items.Add(_menu24Hours);
        
        _contextMenu.Items.Add(new ToolStripSeparator());
        
        // Toast option
        _menuShowToast = new ToolStripMenuItem("Show notification on capture", null, OnShowToastChanged)
        {
            CheckOnClick = true,
            Checked = _showToast
        };
        _contextMenu.Items.Add(_menuShowToast);
        
        _contextMenu.Items.Add(new ToolStripSeparator());
        
        // Select Screenshots folder
        var folderItem = new ToolStripMenuItem("📂 Select Screenshots folder...", null, OnSelectFolder);
        _contextMenu.Items.Add(folderItem);
        
        // Run at startup
        var startupItem = new ToolStripMenuItem("🚀 Run at Windows startup", null, OnStartupChanged)
        {
            CheckOnClick = true,
            Checked = StartupManager.IsEnabled()
        };
        _contextMenu.Items.Add(startupItem);
        
        _contextMenu.Items.Add(new ToolStripSeparator());
        
        // Exit button
        var exitItem = new ToolStripMenuItem("❌ Exit", null, OnExit);
        _contextMenu.Items.Add(exitItem);
        
        // Tạo tray icon
        _trayIcon = new NotifyIcon
        {
            Icon = LoadIcon(),
            Text = "Auto Delete Screenshot - 30 min",
            Visible = true,
            ContextMenuStrip = _contextMenu
        };
        
        // Double click để mở menu
        _trayIcon.MouseClick += (s, e) =>
        {
            if (e.Button == MouseButtons.Left)
            {
                // Hiện menu khi click trái
                var mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", 
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                mi?.Invoke(_trayIcon, null);
            }
        };
        
        UpdateMenuCheckmarks();
        
        // Khởi tạo ScreenshotWatcher
        _screenshotWatcher = new ScreenshotWatcher(
            () => _deleteAfterMinutes,
            OnNewScreenshot
        );
        
        // Khởi tạo FileCleanupService - quét mỗi 60 giây
        _fileCleanupService = new FileCleanupService(60);
    }
    
    /// <summary>
    /// Xử lý khi có ảnh chụp mới
    /// </summary>
    private void OnNewScreenshot(string fileName)
    {
        if (_showToast)
        {
            string timeText = _deleteAfterMinutes switch
            {
                15 => "15 minutes",
                30 => "30 minutes",
                60 => "1 hour",
                1440 => "24 hours",
                _ => $"{_deleteAfterMinutes} minutes"
            };
            
            // Show balloon tip
            _trayIcon.ShowBalloonTip(
                3000,
                "📷 Auto Delete Screenshot",
                $"Screenshot will be deleted in {timeText}",
                ToolTipIcon.Info
            );
        }
    }

    /// <summary>
    /// Load icon từ file hoặc tạo icon mặc định
    /// </summary>
    private Icon LoadIcon()
    {
        try
        {
            string iconPath = Path.Combine(AppContext.BaseDirectory, "Resources", "icon.png");
            if (File.Exists(iconPath))
            {
                using var bitmap = new Bitmap(iconPath);
                return Icon.FromHandle(bitmap.GetHicon());
            }
        }
        catch { }
        
        // Tạo icon mặc định nếu không load được
        return CreateDefaultIcon();
    }

    /// <summary>
    /// Tạo icon mặc định màu xanh
    /// </summary>
    private Icon CreateDefaultIcon()
    {
        var bitmap = new Bitmap(16, 16);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.Clear(Color.Transparent);
            using var brush = new SolidBrush(Color.FromArgb(0, 120, 215)); // Windows blue
            g.FillEllipse(brush, 1, 1, 14, 14);
            using var whiteBrush = new SolidBrush(Color.White);
            g.FillRectangle(whiteBrush, 6, 4, 4, 5); // Clock hand
            g.FillRectangle(whiteBrush, 6, 6, 5, 2); // Clock hand horizontal
        }
        return Icon.FromHandle(bitmap.GetHicon());
    }

    /// <summary>
    /// Xử lý khi thay đổi thời gian xóa
    /// </summary>
    private void OnDeleteTimeChanged(object? sender, EventArgs e)
    {
        if (sender is ToolStripMenuItem item && item.Tag is int minutes)
        {
            _deleteAfterMinutes = minutes;
            UpdateMenuCheckmarks();
            UpdateTooltip();
            
            // Lưu setting
            _settingsManager.DeleteAfterMinutes = minutes;
        }
    }

    /// <summary>
    /// Xử lý khi bật/tắt toast
    /// </summary>
    private void OnShowToastChanged(object? sender, EventArgs e)
    {
        _showToast = _menuShowToast.Checked;
        // Lưu setting
        _settingsManager.ShowToast = _showToast;
    }

    /// <summary>
    /// Xử lý khi click menu chọn folder
    /// </summary>
    private void OnSelectFolder(object? sender, EventArgs e)
    {
        PromptForScreenshotsFolder();
    }

    /// <summary>
    /// Xử lý khi thay đổi cài đặt khởi động cùng Windows
    /// </summary>
    private void OnStartupChanged(object? sender, EventArgs e)
    {
        if (sender is ToolStripMenuItem item)
        {
            bool success;
            if (item.Checked)
            {
                success = StartupManager.Enable();
                if (success)
                {
                    _trayIcon.ShowBalloonTip(
                        2000,
                        "🚀 Enabled",
                        "App will start with Windows",
                        ToolTipIcon.Info
                    );
                }
            }
            else
            {
                success = StartupManager.Disable();
                if (success)
                {
                    _trayIcon.ShowBalloonTip(
                        2000,
                        "🚀 Disabled",
                        "App will not start with Windows",
                        ToolTipIcon.Info
                    );
                }
            }

            if (!success)
            {
                item.Checked = !item.Checked; // Revert checkbox on error
                MessageBox.Show(
                    "Cannot change startup settings.\nTry running the app as Administrator.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }

    /// <summary>
    /// Show folder selection dialog
    /// </summary>
    private void PromptForScreenshotsFolder()
    {
        string? selectedPath = PathHelper.PromptForFolder();
        
        if (!string.IsNullOrEmpty(selectedPath))
        {
            PathHelper.SetScreenshotsPath(selectedPath);
            
            _trayIcon.ShowBalloonTip(
                3000,
                "📂 Folder Selected",
                $"Watching: {selectedPath}",
                ToolTipIcon.Info
            );
            
            // Restart services to apply new path
            RestartServices();
        }
        else if (!_settingsManager.HasScreenshotsPath)
        {
            // Show warning if no path selected
            MessageBox.Show(
                "You need to select a Screenshots folder for the app to work.\n\nRight-click the icon and select 'Select Screenshots folder...'",
                "Auto Delete Screenshot",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
        }
    }

    /// <summary>
    /// Restart services after folder change
    /// </summary>
    private void RestartServices()
    {
        // Dispose old services
        _screenshotWatcher?.Dispose();
        _fileCleanupService?.Dispose();
        
        // Show restart required message
        MessageBox.Show(
            "Please restart the application to apply the new folder.",
            "Restart Required",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
        );
    }

    /// <summary>
    /// Update checkmarks for menu items
    /// </summary>
    private void UpdateMenuCheckmarks()
    {
        _menuNoDelete.Checked = _deleteAfterMinutes == 0;
        _menu15Min.Checked = _deleteAfterMinutes == 15;
        _menu30Min.Checked = _deleteAfterMinutes == 30;
        _menu1Hour.Checked = _deleteAfterMinutes == 60;
        _menu24Hours.Checked = _deleteAfterMinutes == 1440;
    }

    /// <summary>
    /// Update tray icon tooltip
    /// </summary>
    private void UpdateTooltip()
    {
        string timeText = _deleteAfterMinutes switch
        {
            0 => "No auto-delete",
            15 => "15 min",
            30 => "30 min",
            60 => "1 hour",
            1440 => "24 hours",
            _ => $"{_deleteAfterMinutes} min"
        };
        _trayIcon.Text = $"Auto Delete Screenshot - {timeText}";
    }

    /// <summary>
    /// Handle Exit button click
    /// </summary>
    private void OnExit(object? sender, EventArgs e)
    {
        _trayIcon.Visible = false;
        _trayIcon.Dispose();
        Application.Exit();
    }

    /// <summary>
    /// Lấy thời gian xóa hiện tại
    /// </summary>
    public int DeleteAfterMinutes => _deleteAfterMinutes;

    /// <summary>
    /// Có hiện toast không
    /// </summary>
    public bool ShowToast => _showToast;

    /// <summary>
    /// Cleanup khi đóng ứng dụng
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _fileCleanupService?.Dispose();
            _screenshotWatcher?.Dispose();
            _trayIcon?.Dispose();
            _contextMenu?.Dispose();
        }
        base.Dispose(disposing);
    }
}
