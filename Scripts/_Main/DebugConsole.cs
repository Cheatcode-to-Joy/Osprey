using Godot;
using System;

public partial class DebugConsole : Control
{
	public override void _Ready()
	{
		Router.Debug.Connect(DebugManager.SignalName.MessageSent, new Callable(this, MethodName.AddMessageToLog));
		Connect(SignalName.RequestDebugFill, new Callable(Router.Debug, DebugManager.MethodName.FillLog));

		DebugLabel.GetVScrollBar().Scale = Vector2.Zero;

		InitialiseLog();
	}

	[Export] private RichTextLabel DebugLabel;
	[Signal] public delegate void RequestDebugFillEventHandler();
	private void InitialiseLog()
	{
		DebugLabel.Text = "";
		EmitSignal(SignalName.RequestDebugFill);
	}

	public void AddMessageToLog(string Message)
	{
		if (DebugLabel.Text != "") { DebugLabel.Text = $"{DebugLabel.Text}\n{Message}"; }
		else { DebugLabel.Text = Message; }
	}
}
