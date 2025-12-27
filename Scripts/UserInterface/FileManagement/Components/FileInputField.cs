using Godot;
using System;
using System.Text.RegularExpressions;

public partial class FileInputField : LineEdit
{
	private string CachedInput = "";

	private void OnTextChanged(string NewText)
	{
		Match TextMatch = CreateFileOverlay.PermittedInputRegex().Match(NewText);
		if (TextMatch.Success) { CachedInput = NewText; }
		else
		{
			Text = CachedInput;
			CaretColumn = CachedInput.Length;
		}
	}

	[Signal] public delegate void InputStartRequestedEventHandler();
	private void OnGuiInput(InputEvent Event)
	{
		if (Event.IsActionPressed("FocusInputYes"))
		{
			GetViewport().SetInputAsHandled();
			EmitSignal(SignalName.InputStartRequested);
		}
	}

	[Export] private NinePatchRect FakeFocus;
	public void ToggleFakeFocus(bool Active)
	{
		FakeFocus.Visible = Active;
	}
}
