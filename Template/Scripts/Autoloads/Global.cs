using Godot;
using System.Threading.Tasks;
using System;

namespace Template.Valky;

[Service]
public partial class Global : Node
{
    /// <summary>
    /// If no await calls are needed, add "return await Task.FromResult(1);"
    /// </summary>
    public event Func<Task> OnQuit;

    [Export] private OptionsManager optionsManager;

    public Logger Logger { get; private set; } = new();

	public override void _Ready()
	{
        Logger.MessageLogged += Game.Console.AddMessage;

        new ModLoader().LoadMods(this);
    }

	public override void _PhysicsProcess(double delta)
	{
        if (Input.IsActionJustPressed(InputActions.Fullscreen))
        {
            optionsManager.ToggleFullscreen();
        }

        Logger.Update();
	}

    public override async void _Notification(int what)
	{
		if (what == NotificationWMCloseRequest)
		{
			await QuitAndCleanup();
		}
	}

    public async Task QuitAndCleanup()
	{
        GetTree().AutoAcceptQuit = false;

        // Handle cleanup here
        optionsManager.SaveOptions();
        optionsManager.SaveHotkeys();

        if (OnQuit != null)
        {
            await OnQuit?.Invoke();
        }

        // This must be here because buttons call Global::Quit()
        GetTree().Quit();
	}
}

