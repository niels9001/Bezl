using Windows.UI;

namespace Vignette.Models;

public record GradientDefinition(
    string Name,
    Color StartColor,
    Color EndColor,
    double AngleDegrees = 135)
{
    public static readonly GradientDefinition[] Presets =
    [
        new("Sunset",   Color.FromArgb(255, 255, 154, 0),   Color.FromArgb(255, 208, 0, 108)),
        new("Ocean",    Color.FromArgb(255, 0, 180, 219),    Color.FromArgb(255, 0, 131, 176)),
        new("Forest",   Color.FromArgb(255, 17, 153, 82),    Color.FromArgb(255, 56, 239, 125)),
        new("Candy",    Color.FromArgb(255, 224, 0, 254),    Color.FromArgb(255, 142, 45, 226)),
        new("Midnight", Color.FromArgb(255, 15, 32, 39),     Color.FromArgb(255, 44, 83, 100)),
        new("Dawn",     Color.FromArgb(255, 255, 175, 64),   Color.FromArgb(255, 255, 95, 109)),
        new("Aurora",   Color.FromArgb(255, 67, 206, 162),   Color.FromArgb(255, 24, 90, 157)),
        new("Lavender", Color.FromArgb(255, 189, 147, 249),  Color.FromArgb(255, 129, 140, 248)),
    ];
}
