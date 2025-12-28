using Godot;
using System;

public partial class TitleOverlay : UILayer
{
	[Export] private Control PressStart;
	[Export] private MenuButtons Buttons;

	[Export] private Label VersionNumber;

	public override void _Ready()
	{
		Buttons.Connect(MenuButtons.SignalName.ButtonPressed, new Callable(this, MethodName.OnButtonPressed));
		VersionNumber.Text = ProjectSettings.GetSetting("application/config/version").ToString();
	}

	#region UI Layer
	public override void GrabDefaultFocus()
	{
		Buttons.GetChild<Control>(0)?.GrabFocus();
	}

	public override bool RequestOverlayExit() { return false; }
	#endregion

	public void OnKeyPressed()
	{
		Buttons.Show();
		CallDeferred(MethodName.GrabDefaultFocus);
	}

	[Export] private PackedScene FileCarouselScene;
	[Export] private PackedScene SettingsScene;

	private StringName[] ButtonMethods = [MethodName.OnSavesPressed, MethodName.OnSettingsPressed];
	public void OnButtonPressed(int ButtonNumber)
	{
		Router.Debug.Print($"Title button {ButtonNumber} pressed.");
		Call(ButtonMethods[Math.Clamp(ButtonNumber, 0, ButtonMethods.Length - 1)]);
	}

	private void OnSavesPressed()
	{
		Router.Debug.Print("Displaying saves.");
		Router.Main.AddOverlay(FileCarouselScene.Instantiate<UILayer>());
	}

	private void OnSettingsPressed()
	{
		Router.Debug.Print("Opening settings.");
		Router.Main.AddOverlay(SettingsScene.Instantiate<UILayer>());
	}
}
