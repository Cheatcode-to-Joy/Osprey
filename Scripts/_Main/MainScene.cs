using Godot;
using System;

public partial class MainScene : Node2D
{
	[Export] private DebugConsole Console;

	public override void _Input(InputEvent @Event)
	{
		if (@Event.IsActionPressed("ToggleDebugConsole"))
		{
			Console.Visible = !Console.Visible;
		}
	}
}
