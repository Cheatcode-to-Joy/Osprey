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

	private StringName[] ButtonMethods = { MethodName.OnNewGamePressed, MethodName.OnLoadSavesPressed, MethodName.OnSettingsPressed };
	public void OnButtonPressed(int ButtonNumber)
	{
		Router.Debug.Print($"Title button {ButtonNumber.ToString()} pressed.");
		Call(ButtonMethods[Math.Clamp(ButtonNumber, 0, ButtonMethods.Length - 1)]);
	}

	private void OnNewGamePressed()
	{
		// TODO.
		Router.Debug.Print("Starting new game.");
	}

	private void OnLoadSavesPressed()
	{
		// TODO.
		Router.Debug.Print("Loading saves.");
	}

	private void OnSettingsPressed()
	{
		// TODO.
		Router.Debug.Print("Opening settings.");
	}
}
