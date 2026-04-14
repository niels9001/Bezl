<h1 align="center">Bezl</h1>
<p align="center">Beautiful screenshot borders made easy</p>

A WinUI 3 desktop app for capturing window screenshots with customizable gradient borders and rounded corners. Pick any window, frame it with a gradient, round the corners, and export — all in a few clicks.

## ⭐ Features

- **Click-to-pick window capture** — Select any window to screenshot using the system window picker
- **Gradient borders** — Add a gradient background around your screenshot with configurable padding
- **Corner radius** — Round the corners of your screenshots for a polished look
- **Gradient presets** — Choose from 24 built-in presets or create your own
- **Set as wallpaper** — Apply your gradient as the desktop wallpaper for cohesive screenshots
- **Export options** — Copy to clipboard or save as PNG

## 🚀 Getting started

### 1. Set up the environment

> [!NOTE]
> Bezl requires [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) or later to build and Windows 10 (19041+) or later to execute.

**Required [Visual Studio components](https://learn.microsoft.com/windows/apps/get-started/start-here?tabs=vs-2022-17-10#required-workloads-and-components):**
- .NET Desktop Development
- Windows application development

Or, if building from the command line:
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Windows App SDK 1.8+](https://learn.microsoft.com/windows/apps/windows-app-sdk/downloads)

### 2. Clone the repository

```powershell
git clone https://github.com/niels9001/Bezl.git
```

### 3. Build and run

**Visual Studio:** Open `Bezl.sln`, set `Bezl` as the startup project, select `x64`, and run.

**Command line:**
```powershell
dotnet build src\Bezl\Bezl.csproj -p:Platform=x64
dotnet run --project src\Bezl\Bezl.csproj
```

## 🖼️ How It Works

1. **Choose a gradient** — Pick a preset or create your own
2. **(Optional) Set as wallpaper** — Apply the gradient to your desktop
3. **Capture a window** — Click "Capture" and select your target
4. **Adjust** — Drag corner handles for radius, use the slider for padding
5. **Export** — Copy to clipboard or save as PNG

## ➡️ Related

- [Get started with WinUI](https://learn.microsoft.com/windows/apps/get-started/start-here)
- [Windows App SDK](https://github.com/microsoft/WindowsAppSDK)

## License

[MIT](LICENSE)
