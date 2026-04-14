using System.Text.Json;
using System.Text.Json.Serialization;
using Vignette.Models;
using Windows.UI;

namespace Vignette.Services;

public static class GradientStore
{
    // Use real user profile path (not MSIX-virtualized) so data persists across app updates
    private static readonly string StorePath = Path.Combine(
        Environment.GetEnvironmentVariable("USERPROFILE")
            ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".vignette", "gradients.json");

    public static List<GradientDefinition> Load()
    {
        if (!File.Exists(StorePath))
            return new List<GradientDefinition>(GradientDefinition.Presets);

        try
        {
            var json = File.ReadAllText(StorePath);
            var dtos = JsonSerializer.Deserialize<List<GradientDto>>(json) ?? [];
            var gradients = dtos.Select(d => d.ToDefinition()).ToList();
            return gradients.Count > 0 ? gradients : new List<GradientDefinition>(GradientDefinition.Presets);
        }
        catch
        {
            return new List<GradientDefinition>(GradientDefinition.Presets);
        }
    }

    public static void Save(IEnumerable<GradientDefinition> gradients)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(StorePath)!);
        var dtos = gradients.Select(g => GradientDto.FromDefinition(g)).ToList();
        var json = JsonSerializer.Serialize(dtos, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(StorePath, json);
    }

    private record GradientDto(
        string Name,
        string StartColor,
        string EndColor,
        double AngleDegrees)
    {
        public GradientDefinition ToDefinition() =>
            new(Name, ParseColor(StartColor), ParseColor(EndColor), AngleDegrees);

        public static GradientDto FromDefinition(GradientDefinition g) =>
            new(g.Name, ColorToHex(g.StartColor), ColorToHex(g.EndColor), g.AngleDegrees);

        private static Color ParseColor(string hex)
        {
            hex = hex.TrimStart('#');
            if (hex.Length == 6)
                hex = "FF" + hex;
            var val = uint.Parse(hex, System.Globalization.NumberStyles.HexNumber);
            return Color.FromArgb((byte)(val >> 24), (byte)(val >> 16), (byte)(val >> 8), (byte)val);
        }

        private static string ColorToHex(Color c) => $"#{c.A:X2}{c.R:X2}{c.G:X2}{c.B:X2}";
    }
}
