using Godot;
using GodotUtils;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Template.Setup;

public static partial class SetupUtils
{
    public static Dictionary<Genre, string> FolderNames { get; } = new()
    {
        { Genre.None, "No Genre" },
        { Genre.Platformer2D, "2D Platformer" },
        { Genre.TopDown2D, "2D Top Down" },
        { Genre.FPS3D, "3D FPS" }
    };

    public static string Highlight(string text)
    {
        return $"[wave amp=20.0 freq=2.0 connected=1][color=white]{text}[/color][/wave]";
    }

    public static string FormatGameName(string name)
    {
        return name.Trim().FirstCharToUpper().Replace(" ", "");
    }

    public static bool IsAlphaNumericAndAllowSpaces(string str)
    {
        return AlphaNumericAndSpacesRegex().IsMatch(str);
    }

    public static void SetGenreSelectedInfo(RichTextLabel genreSelectedInfo, Genre genre)
    {
        string text = $"The {Highlight(FolderNames[genre])} genre has been selected. " +
              $"All other assets not specific to {Highlight(FolderNames[genre])} " +
              $"will be deleted.";

        genreSelectedInfo.Text = text;
    }

    public static void DisplayGameNamePreview(string inputName, RichTextLabel gameNamePreview)
    {
        string name = FormatGameName(inputName);

        string text = $"The name of the project will be {Highlight(name)}. " +
              $"The root namespace for all scripts will be {Highlight(name)}. " +
              $"Please ensure the name is in PascalFormat.";

        gameNamePreview.Text = text;
    }

    [GeneratedRegex(@"^[a-zA-Z0-9\s,]*$")]
    private static partial Regex AlphaNumericAndSpacesRegex();
}
