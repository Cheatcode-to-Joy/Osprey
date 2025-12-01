using Godot;
using System;

public partial class MainScene : Node2D, IConfigReliant
{
	[Export] private DebugOverlay DOverlay;
	[Export] private PackedScene Settings;
	[Export] private Control SettingsHolder;
	private SettingsMenu CurrentSettings = null;

	[Export] public MainCamera Camera;

	public void OnTreeEntered()
	{
		Router.Main = this;
	}

	public override void _Ready()
	{
		Router.Config.Connect(ConfigManager.SignalName.ConfigChanged, new Callable(this, MethodName.OnConfigUpdate));
	}

	public override void _Input(InputEvent @Event)
	{
		if (@Event.IsActionPressed("ToggleDebugOverlay"))
		{
			if (Router.Config.FetchConfig<bool>("Debug", "DebugEnabled")) { DOverlay.Visible = !DOverlay.Visible; }
		}
		else if (@Event.IsActionPressed("ToggleSettings"))
		{
			ToggleSettings();
		}
	}

	public void OnConfigUpdate()
	{
		if (DOverlay.Visible && !Router.Config.FetchConfig<bool>("Debug", "DebugEnabled")) { DOverlay.Visible = false; }
	}

	private void ToggleSettings()
	{
		if (CurrentSettings != null)
		{
			CurrentSettings.QueueFree();
			CurrentSettings = null;
			return;
		}

		CurrentSettings = Settings.Instantiate<SettingsMenu>();
		SettingsHolder.AddChild(CurrentSettings);
	}
}
