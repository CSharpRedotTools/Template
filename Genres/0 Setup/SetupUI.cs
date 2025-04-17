using Godot;
using GodotUtils;
using System.IO;

namespace Template.Setup;

public partial class SetupUI : Node
{
    [Export] private LineEdit _lineEditGameName;
    [Export] private LineEdit _lineEditRedotExe;
    [Export] private OptionButton _genreOptionBtn;
    [Export] private PopupPanel _popupPanel;
    [Export] private RichTextLabel _gameNamePreview;
    [Export] private RichTextLabel _genreSelectedInfo;
    [Export] private CheckButton _checkButtonDeleteSetupScene;
    [Export] private CheckButton _checkButtonDeleteOtherGenres;
    [Export] private CheckButton _checkButtonMoveProjectFiles;

    private Genre _genre;
    private string _prevGameName = string.Empty;

    public override void _Ready()
    {
        _genre = (Genre)_genreOptionBtn.Selected;
        SetupUtils.SetGenreSelectedInfo(_genreSelectedInfo, _genre);
        SetupUtils.DisplayGameNamePreview("Undefined", _gameNamePreview);

        
    }

    private void _on_yes_pressed()
    {
        string gameName = SetupUtils.FormatGameName(_lineEditGameName.Text);
        string path = ProjectSettings.GlobalizePath("res://");

        // The IO functions ran below will break if empty folders exist
        DirectoryUtils.DeleteEmptyDirectories(path);

        SetupManager.RenameProjectFiles(path, gameName);
        SetupManager.RenameAllNamespaces(path, gameName);
        SetupManager.SetupVSCodeTemplates(_lineEditRedotExe.Text, gameName);

        if (_checkButtonMoveProjectFiles.ButtonPressed)
        {
            SetupManager.MoveProjectFiles(_genre,
                pathFrom: Path.Combine(path, "Genres"), 
                pathTo: path, 
                deleteOtherGenres: _checkButtonDeleteOtherGenres.ButtonPressed);

            SceneFileUtils.FixBrokenDependencies();
        }

        if (_checkButtonDeleteSetupScene.ButtonPressed)
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
        SetupUtils.SetGenreSelectedInfo(_genreSelectedInfo, _genre);
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
            SetupUtils.DisplayGameNamePreview(_prevGameName, _gameNamePreview);
            _lineEditGameName.Text = _prevGameName;
            _lineEditGameName.CaretColumn = _prevGameName.Length;
            return;
        }

        SetupUtils.DisplayGameNamePreview(newText, _gameNamePreview);
        _prevGameName = newText;
    }

    private void _on_no_pressed()
    {
        _popupPanel.Hide();
    }

    private void _on_apply_changes_pressed() 
    {
        string gameName = SetupUtils.FormatGameName(_lineEditGameName.Text);

        if (string.IsNullOrWhiteSpace(gameName))
        {
            GD.Print("Please type a game name first!");
            return;
        }

        _popupPanel.PopupCentered();
    }
}
