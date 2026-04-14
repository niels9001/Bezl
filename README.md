# Bezl

A beautiful WinUI 3 desktop app for capturing window screenshots with customizable gradient borders and rounded corners.

## Features

- **Click-to-pick window capture** — Select any window to screenshot using the system window picker
- **Gradient borders** — Add a beautiful gradient background around your screenshot with configurable padding
- **Corner radius** — Round the corners of your screenshots for a polished look
- **Gradient generator** — Create custom gradients or choose from built-in presets (Sunset, Ocean, Forest, Candy, Midnight, Dawn)
- **Set as wallpaper** — Apply your gradient as the desktop wallpaper for cohesive screenshots
- **Export options** — Copy to clipboard or save as PNG

## Requirements

- Windows 10 version 1903 (19041) or later
- .NET 9
- Windows App SDK 1.7+

## Building

```bash
cd src/Bezl
dotnet build
```

## Running

```bash
dotnet run --project src/Bezl
```

## How It Works

1. **Generate a gradient** — Pick colors and angle, or choose a preset
2. **(Optional) Set as wallpaper** — Apply the gradient to your desktop
3. **Capture a window** — Click "Capture Window" and select your target
4. **Adjust settings** — Use sliders to fine-tune border padding and corner radius
5. **Export** — Copy to clipboard or save as PNG

## License

[MIT](LICENSE)
