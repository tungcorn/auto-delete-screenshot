# Auto Delete Screenshot

A **super lightweight** (~150KB) and portable Windows utility that keeps your desktop clean by automatically deleting temporary screenshots. Designed for **zero impact** on system performance.

![.NET](https://img.shields.io/badge/.NET-8.0-blue)
![Platform](https://img.shields.io/badge/Platform-Windows-lightgrey)
![License](https://img.shields.io/badge/License-MIT-green)

## Features

- ⚡ **Super Lightweight** - Only ~150KB. Minimal RAM usage & 0% CPU idle.
- 🕐 **Auto-delete** screenshots after 15 min, 30 min, 1 hour, or 24 hours
- 📂 **Custom folder** selection for monitoring
- 🚀 **Run at startup** option
- 🔔 **Optional notifications** when screenshots are captured
- 💾 **Portable** - no installation required

## Installation

### Step 1: Check .NET Runtime
Open **Command Prompt** or **PowerShell** and run:
```
dotnet --list-runtimes
```

✅ **If you see** `Microsoft.WindowsDesktop.App 8.x.x` or higher → Skip to Step 3

❌ **If you get an error** or don't see version 8.x or higher → Continue to Step 2

### Step 2: Install .NET Runtime
Download and install **.NET 8.0 Desktop Runtime** (or newer):

👉 [Download .NET 8.0 Desktop Runtime (x64)](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-8.0.11-windows-x64-installer)

> **Note:** .NET 9.x or higher will also work.

### Step 3: Download & Run
1. Download `AutoDeleteScreenshot.exe` from [Latest Release](https://github.com/TungCorn/auto-delete-screenshot/releases/latest)
2. Double-click to run
3. **Important:** On first launch, right-click the tray icon and choose **"📂 Select Screenshots folder..."** to tell the app where your screenshots are saved.
4. The app will appear in your system tray (bottom-right corner)

### Step 4: Configure (Optional)
Right-click the tray icon to:
- Set deletion time (15 min / 30 min / 1 hour / 24 hours)
- Enable notifications
- Enable "Run at Windows startup"

## How It Works

```
📸 You take a screenshot
        ↓
🏷️ App renames it: Screenshot_AUTODEL_1234567890.png
        ↓
⏰ Timer runs every 60 seconds
        ↓
🗑️ Expired files get deleted automatically
```

Only files with `_AUTODEL_` tag are deleted. Your other files are **100% safe**.

## Troubleshooting

### Browser Warning (Download Blocked)
When downloading, your browser (Chrome/Edge) might block the file because it is not commonly downloaded.
*   **Chrome:** Click the download icon ➔ "Keep" ➔ "Show more" ➔ "Keep anyway".
*   **Edge:** Click "..." (More actions) ➔ "Keep" ➔ "Show more" ➔ "Keep anyway".

### Windows SmartScreen Warning
When you run the app for the first time, you might see a blue window saying **"Windows protected your PC"**.
This happens because the app is open-source and not digitally signed.

To run it:
1. Click **"More info"**
2. Click **"Run anyway"**

## Build from Source

```bash
git clone https://github.com/TungCorn/auto-delete-screenshot.git
cd auto-delete-screenshot/src/AutoDeleteScreenshot
dotnet publish -c Release -r win-x64 --self-contained false /p:PublishSingleFile=true
```

Output: `bin/Release/net8.0-windows/win-x64/publish/AutoDeleteScreenshot.exe`

## Uninstall

1. Right-click tray icon → Exit
2. Delete `AutoDeleteScreenshot.exe`
3. (Optional) Delete settings: `%APPDATA%\AutoDeleteScreenshot\`

## Safety

✅ Only deletes files with `_AUTODEL_` tag  
✅ No admin privileges required  
✅ Open source - verify the code yourself  

## License

[MIT License](LICENSE)

## Author

**TungCorn** - [GitHub](https://github.com/TungCorn)
