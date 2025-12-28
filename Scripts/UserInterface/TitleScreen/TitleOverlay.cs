using Godot;
using System;

public partial class TitleOverlay : UILayer
{
	[Export] private Control PressStart;
	[Export] private MenuButtons Buttons;

	public override void _Ready()
	{
		Buttons.Connect(MenuButtons.SignalName.ButtonPressed, new Callable(this, MethodName.OnButtonPressed));
	}

	#region UI Layer
	public override void GrabDefaultFocus()
	{
		Buttons.GetChild<Control>(0).GrabFocus();
	}

	public override bool RequestOverlayExit() { return false; }
	#endregion

	public void OnKeyPressed()
	{
		Buttons.Show();
	}

	private StringName[] ButtonMethods = [MethodName.OnSavesPressed, MethodName.OnSettingsPressed];
	public void OnButtonPressed(int ButtonNumber)
	{
		Router.Debug.Print($"Title button {ButtonNumber} pressed.");
		Call(ButtonMethods[Math.Clamp(ButtonNumber, 0, ButtonMethods.Length - 1)]);
	}

	private void OnSavesPressed()
	{
		// TODO.
		Router.Debug.Print("Displaying saves.");
	}

	private void OnSettingsPressed()
	{
		// TODO.
		Router.Debug.Print("Opening settings.");
	}
}
