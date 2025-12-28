using Godot;
using System;

public partial class TitleButton : Button
{
	[Export] private Label SelectorL;
	[Export] private Label SelectorR;

	private void ShowDecorations()
	{
		SelectorL.Text = ">";
		SelectorR.Text = "<";
	}

	private void HideDecorations()
	{
		SelectorL.Text = " ";
		SelectorR.Text = " ";
	}

	private void OnGuiInput(InputEvent Event)
	{
		if (Event.IsActionPressed("FocusInputYes"))
		{
			GetViewport().SetInputAsHandled();
			EmitSignal(BaseButton.SignalName.Pressed);
		}
	}

	private void OnFocusEntered()
	{
		ShowDecorations();
	}

	private void OnFocusExited()
	{
		if (!IsHovered()) { HideDecorations(); }
	}

	private void OnMouseEntered()
	{
		ShowDecorations();
	}

	private void OnMouseExited()
	{
		if (!HasFocus()) { HideDecorations(); }
	}
}
