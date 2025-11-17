using Godot;
using System;

public partial class DebugConsole : Control
{
	[Export] private DebugOverlay DOverlay;

	public override void _Ready()
	{
		Router.Debug.Connect(DebugManager.SignalName.MessageSent, new Callable(this, MethodName.AddMessageToLog));
		Connect(SignalName.RequestDebugFill, new Callable(Router.Debug, DebugManager.MethodName.FillLog));

		InitialiseLog();
	}

	#region Scrolling
	private const int MaxLines = 29;
	private int LineNumber = 0; // The total number of lines.
	private int TopLine = 0; // The topmost visible line.

	public override void _Input(InputEvent @Event)
	{
		if (!DOverlay.Visible) { return; }
		if (@Event.IsActionPressed("ScrollDown")) { ScrollDown(); }
		else if (@Event.IsActionPressed("ScrollUp")) { ScrollUp(); }
	}

	private void ScrollDown()
	{
		if ((LineNumber > MaxLines) && (TopLine < LineNumber - MaxLines))
		{
			TopLine++;
			DebugLabel.ScrollToLine(TopLine);
		}
	}

	private void ScrollUp()
	{
		if (TopLine > 0)
		{
			TopLine--;
			DebugLabel.ScrollToLine(TopLine);
		}
	}

	private void ScrollToBottom()
	{
		TopLine = Math.Max(0, LineNumber - MaxLines);
		DebugLabel.ScrollToLine(TopLine);
	}
	#endregion

	[Export] private RichTextLabel DebugLabel;
	[Signal] public delegate void RequestDebugFillEventHandler();
	private void InitialiseLog()
	{
		DebugLabel.Text = "";
		LineNumber = 0;

		EmitSignal(SignalName.RequestDebugFill);
	}

	public void AddMessageToLog(string Message)
	{
		if (DebugLabel.Text != "") { DebugLabel.Text = $"{DebugLabel.Text}\n{Message}"; }
		else { DebugLabel.Text = Message; }

		LineNumber++;
		ScrollToBottom();
	}
}
