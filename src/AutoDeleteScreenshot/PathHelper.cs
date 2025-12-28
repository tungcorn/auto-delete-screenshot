using System.Runtime.InteropServices;

namespace AutoDeleteScreenshot;

public static class PathHelper
{
    private static readonly Guid KnownFolderScreenshots = new("b7bede81-dfdb-4f24-936b-7058bbc606ea");
    private static SettingsManager? _settingsManager;

    [DllImport("shell32.dll")]
    private static extern int SHGetKnownFolderPath(
        [MarshalAs(UnmanagedType.LPStruct)] Guid rfid, 
        uint dwFlags, 
        IntPtr hToken, 
        out IntPtr ppszPath);

    /// <summary>
    /// Khởi tạo PathHelper với SettingsManager
    /// </summary>
    public static void Initialize(SettingsManager settingsManager)
    {
        _settingsManager = settingsManager;
    }

    /// <summary>
    /// Lấy đường dẫn thư mục Screenshots (từ settings hoặc mặc định)
    /// </summary>
    public static string GetScreenshotsPath()
    {
        // Ưu tiên path từ settings
        if (_settingsManager != null && _settingsManager.HasScreenshotsPath)
        {
            return _settingsManager.ScreenshotsPath!;
        }

        // Thử lấy từ Known Folder
        try
        {
            if (SHGetKnownFolderPath(KnownFolderScreenshots, 0, IntPtr.Zero, out IntPtr pPath) == 0)
            {
                string? path = Marshal.PtrToStringUni(pPath);
                Marshal.FreeCoTaskMem(pPath);
                
                if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                {
                    return path;
                }
            }
        }
        catch
        {
            // Ignore error
        }

        // Fallback cuối cùng
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Screenshots");
    }

    /// <summary>
    /// Yêu cầu người dùng chọn folder Screenshots
    /// </summary>
    public static string? PromptForFolder()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Select Screenshots folder to monitor",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = false
        };

        // Thử set initial folder
        string currentPath = GetScreenshotsPath();
        if (Directory.Exists(currentPath))
        {
            dialog.InitialDirectory = currentPath;
        }

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            return dialog.SelectedPath;
        }

        return null;
    }

    /// <summary>
    /// Lưu đường dẫn mới vào settings
    /// </summary>
    public static void SetScreenshotsPath(string path)
    {
        if (_settingsManager != null)
        {
            _settingsManager.ScreenshotsPath = path;
        }
    }
}
