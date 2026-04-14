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
        // Colorful (vibrant but tasteful)
        new("Sunset",     Color.FromArgb(255, 255, 154, 0),    Color.FromArgb(255, 208, 0, 108)),
        new("Ocean",      Color.FromArgb(255, 0, 180, 219),    Color.FromArgb(255, 0, 119, 182)),
        new("Aurora",     Color.FromArgb(255, 67, 206, 162),   Color.FromArgb(255, 24, 90, 157)),
        new("Candy",      Color.FromArgb(255, 224, 0, 254),    Color.FromArgb(255, 142, 45, 226)),
        new("Dawn",       Color.FromArgb(255, 255, 175, 64),   Color.FromArgb(255, 255, 95, 109)),
        new("Horizon",    Color.FromArgb(255, 60, 165, 220),   Color.FromArgb(255, 160, 100, 200)),

        // Pastels (wider color range)
        new("Rose",       Color.FromArgb(255, 240, 180, 190),  Color.FromArgb(255, 195, 140, 165)),
        new("Lavender",   Color.FromArgb(255, 210, 185, 235),  Color.FromArgb(255, 160, 140, 210)),
        new("Powder",     Color.FromArgb(255, 180, 210, 240),  Color.FromArgb(255, 135, 170, 215)),
        new("Mint",       Color.FromArgb(255, 170, 230, 200),  Color.FromArgb(255, 130, 190, 165)),
        new("Peach",      Color.FromArgb(255, 245, 200, 170),  Color.FromArgb(255, 215, 160, 135)),
        new("Blush",      Color.FromArgb(255, 235, 180, 210),  Color.FromArgb(255, 190, 145, 180)),

        // Muted & earthy
        new("Clay",       Color.FromArgb(255, 160, 110, 85),   Color.FromArgb(255, 100, 65, 50)),
        new("Moss",       Color.FromArgb(255, 55, 90, 60),     Color.FromArgb(255, 100, 140, 90)),
        new("Fog",        Color.FromArgb(255, 120, 125, 140),  Color.FromArgb(255, 180, 185, 200)),

        // Dark with color accent
        new("Storm",      Color.FromArgb(255, 20, 25, 45),     Color.FromArgb(255, 65, 85, 135)),
        new("Dusk",       Color.FromArgb(255, 35, 20, 55),     Color.FromArgb(255, 100, 55, 120)),
        new("Merlot",     Color.FromArgb(255, 45, 15, 25),     Color.FromArgb(255, 120, 45, 65)),
        new("Twilight",   Color.FromArgb(255, 30, 25, 60),     Color.FromArgb(255, 130, 80, 140)),

        // Dark & moody (more contrast between stops)
        new("Obsidian",   Color.FromArgb(255, 15, 15, 20),     Color.FromArgb(255, 55, 55, 75)),
        new("Midnight",   Color.FromArgb(255, 10, 15, 30),     Color.FromArgb(255, 40, 60, 110)),
        new("Eclipse",    Color.FromArgb(255, 20, 10, 40),     Color.FromArgb(255, 70, 35, 100)),
        new("Deep Sea",   Color.FromArgb(255, 10, 20, 40),     Color.FromArgb(255, 25, 80, 120)),
        new("Ember",      Color.FromArgb(255, 35, 15, 10),     Color.FromArgb(255, 110, 40, 25)),
    ];
}
