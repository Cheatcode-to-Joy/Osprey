using Godot;
using System;

public partial class TitleScreen : Control
{
	[Export] private Control PressStart;
	[Export] private MenuButtons Buttons;

	public override void _Ready()
	{
		Buttons.Connect(MenuButtons.SignalName.ButtonPressed, new Callable(this, MethodName.OnButtonPressed));
	}

	public override void _Input(InputEvent @Event)
	{
		if (!PressStart.Visible) { return; }
		if (@Event is InputEventKey || @Event is InputEventMouseButton || @Event is InputEventJoypadButton)
		{
			OnKeyPressed();
		}
	}

	private void OnKeyPressed()
	{
		PressStart.Hide();
		Buttons.Show();
	}

	public void OnButtonPressed(int ButtonNumber)
	{
		// TODO.
	}
}
