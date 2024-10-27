using Godot;
using GodotUtils;
using System.IO;

namespace Template.Setup;

public partial class SetupUI : Node
{
    [Export] private LineEdit lineEditGameName;
    [Export] private OptionButton genreOptionBtn;
    [Export] private PopupPanel popupPanel;
    [Export] private RichTextLabel gameNamePreview;
    [Export] private RichTextLabel genreSelectedInfo;
    [Export] private CheckButton checkButtonDeleteSetupScene;
    [Export] private CheckButton checkButtonDeleteOtherGenres;
    [Export] private CheckButton checkButtonMoveProjectFiles;

    private Genre _genre;
    private string _prevGameName = string.Empty;

    public override void _Ready()
    {
        _genre = (Genre)genreOptionBtn.Selected;
        SetupUtils.SetGenreSelectedInfo(genreSelectedInfo, _genre);
        SetupUtils.DisplayGameNamePreview("Undefined", gameNamePreview);
    }

    private void _on_yes_pressed()
    {
        string gameName = SetupUtils.FormatGameName(lineEditGameName.Text);
        string path = ProjectSettings.GlobalizePath("res://");

        // The IO functions ran below will break if empty folders exist
        DirectoryUtils.DeleteEmptyDirectories(path);

        SetupManager.RenameProjectFiles(path, gameName);
        SetupManager.RenameAllNamespaces(path, gameName);

        if (checkButtonMoveProjectFiles.ButtonPressed)
        {
            SetupManager.MoveProjectFiles(_genre,
                pathFrom: Path.Combine(path, "Genres"), 
                pathTo: path, 
                deleteOtherGenres: checkButtonDeleteOtherGenres.ButtonPressed);

            SceneFileUtils.FixBrokenDependencies();
        }

        if (checkButtonDeleteSetupScene.ButtonPressed)
        {
            // Delete the "0 Setup" directory
            Directory.Delete(Path.Combine(path, "Genres", "0 Setup"), true);
        }

        // Ensure all empty folders are deleted when finished
        DirectoryUtils.DeleteEmptyDirectories(path);

        GetTree().Quit();
        SetupEditor.Restart();
    }

    private void _on_genre_item_selected(int index)
    {
        _genre = (Genre)index;
        SetupUtils.SetGenreSelectedInfo(genreSelectedInfo, _genre);
    }

    private void _on_game_name_text_changed(string newText)
    {
        if (string.IsNullOrWhiteSpace(newText))
        {
            return;
        }

        // Since this name is being used for the namespace its first character must not be
        // a number and every other character must be alphanumeric
        if (!SetupUtils.IsAlphaNumericAndAllowSpaces(newText) || char.IsNumber(newText.Trim()[0]))
        {
            SetupUtils.DisplayGameNamePreview(_prevGameName, gameNamePreview);
            lineEditGameName.Text = _prevGameName;
            lineEditGameName.CaretColumn = _prevGameName.Length;
            return;
        }

        SetupUtils.DisplayGameNamePreview(newText, gameNamePreview);
        _prevGameName = newText;
    }

    private void _on_no_pressed()
    {
        popupPanel.Hide();
    }

    private void _on_apply_changes_pressed() 
    {
        string gameName = SetupUtils.FormatGameName(lineEditGameName.Text);

        if (string.IsNullOrWhiteSpace(gameName))
        {
            GD.Print("Please type a game name first!");
            return;
        }

        popupPanel.PopupCentered();
    }
}
