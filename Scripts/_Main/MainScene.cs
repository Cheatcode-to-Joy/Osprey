using Godot;
using System;

public partial class MainScene : Node2D
{
	[Export] private DebugOverlay DOverlay;
	[Export] private PackedScene Settings;
	[Export] private Control SettingsHolder;
	private SettingsMenu CurrentSettings = null;

	public override void _Input(InputEvent @Event)
	{
		if (@Event.IsActionPressed("ToggleDebugOverlay"))
		{
			DOverlay.Visible = !DOverlay.Visible;
		}
		else if (@Event.IsActionPressed("ToggleSettings"))
		{
			ToggleSettings();
		}
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
