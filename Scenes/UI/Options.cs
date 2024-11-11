using Godot;

namespace Template.UI;

[SceneTree]
public partial class Options : PanelContainer
{
    [OnInstantiate]
    private void Init()
    {

    }

    public override void _Ready()
    {
        if (Services.Get<SceneManager>().CurrentScene.Name != "Options")
        {
            _.BackgroundArt.Hide();
        }
    }
}