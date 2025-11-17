using Godot;
using System;

public partial class MainScene : Node2D
{
	[Export] private DebugOverlay DOverlay;

	public override void _Input(InputEvent @Event)
	{
		if (@Event.IsActionPressed("ToggleDebugOverlay"))
		{
			DOverlay.Visible = !DOverlay.Visible;
		}
	}
}
