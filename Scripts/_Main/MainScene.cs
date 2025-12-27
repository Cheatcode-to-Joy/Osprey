using Godot;
using System;

public partial class MainScene : Node2D, IConfigReliant
{
	[Export] private PackedScene Settings;
	[Export] private Control SettingsHolder;
	private SettingsMenu CurrentSettings = null;

	[Export] private PackedScene Debug;
	[Export] private Control DebugHolder;
	private DebugOverlay CurrentDebug = null;

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
		if (@Event.IsActionPressed("ToggleSettings"))
		{
			GetViewport().SetInputAsHandled();
			ToggleSettings();
		}
		else if (@Event.IsActionPressed("ToggleDebugOverlay"))
		{
			if (Router.Config.FetchConfig<bool>("Debug", "DebugEnabled"))
			{
				GetViewport().SetInputAsHandled();
				ToggleDebug();
			}
		}
	}

	public void OnConfigUpdate()
	{
		if (CurrentDebug != null && !Router.Config.FetchConfig<bool>("Debug", "DebugEnabled")) { ToggleDebug(); }
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

	private void ToggleDebug()
	{
		if (CurrentDebug != null)
		{
			CurrentDebug.QueueFree();
			CurrentDebug = null;
			return;
		}

		CurrentDebug = Debug.Instantiate<DebugOverlay>();
		DebugHolder.AddChild(CurrentDebug);
	}
}
