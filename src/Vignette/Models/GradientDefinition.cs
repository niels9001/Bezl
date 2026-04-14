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
        // Dark & moody
        new("Obsidian",   Color.FromArgb(255, 15, 15, 20),     Color.FromArgb(255, 45, 45, 55)),
        new("Charcoal",   Color.FromArgb(255, 30, 30, 35),     Color.FromArgb(255, 60, 60, 70)),
        new("Midnight",   Color.FromArgb(255, 10, 15, 30),     Color.FromArgb(255, 35, 50, 85)),
        new("Eclipse",    Color.FromArgb(255, 20, 10, 35),     Color.FromArgb(255, 55, 30, 75)),
        new("Deep Sea",   Color.FromArgb(255, 10, 20, 35),     Color.FromArgb(255, 20, 60, 80)),
        new("Ember",      Color.FromArgb(255, 35, 15, 10),     Color.FromArgb(255, 80, 30, 20)),

        // Dark with subtle color
        new("Storm",      Color.FromArgb(255, 25, 30, 45),     Color.FromArgb(255, 55, 70, 100)),
        new("Dusk",       Color.FromArgb(255, 40, 25, 50),     Color.FromArgb(255, 80, 50, 90)),
        new("Slate",      Color.FromArgb(255, 35, 40, 50),     Color.FromArgb(255, 70, 80, 95)),
        new("Merlot",     Color.FromArgb(255, 50, 20, 30),     Color.FromArgb(255, 90, 40, 55)),

        // Muted & earthy
        new("Clay",       Color.FromArgb(255, 140, 100, 80),   Color.FromArgb(255, 100, 70, 60)),
        new("Moss",       Color.FromArgb(255, 60, 80, 65),     Color.FromArgb(255, 90, 115, 85)),
        new("Fog",        Color.FromArgb(255, 130, 135, 145),  Color.FromArgb(255, 175, 180, 190)),

        // Pastels
        new("Rose",       Color.FromArgb(255, 230, 180, 190),  Color.FromArgb(255, 200, 150, 170)),
        new("Lavender",   Color.FromArgb(255, 195, 180, 220),  Color.FromArgb(255, 170, 155, 210)),
        new("Powder",     Color.FromArgb(255, 175, 200, 225),  Color.FromArgb(255, 150, 180, 215)),
        new("Mint",       Color.FromArgb(255, 175, 220, 200),  Color.FromArgb(255, 150, 200, 180)),
        new("Peach",      Color.FromArgb(255, 235, 195, 170),  Color.FromArgb(255, 220, 175, 155)),
        new("Blush",      Color.FromArgb(255, 225, 185, 200),  Color.FromArgb(255, 195, 160, 185)),
        new("Lilac",      Color.FromArgb(255, 200, 185, 225),  Color.FromArgb(255, 180, 165, 215)),

        // Subtle warm/cool
        new("Twilight",   Color.FromArgb(255, 45, 35, 65),     Color.FromArgb(255, 140, 90, 120)),
        new("Horizon",    Color.FromArgb(255, 45, 55, 75),     Color.FromArgb(255, 140, 155, 175)),
        new("Aurora",     Color.FromArgb(255, 40, 60, 70),     Color.FromArgb(255, 100, 160, 140)),
    ];
}
