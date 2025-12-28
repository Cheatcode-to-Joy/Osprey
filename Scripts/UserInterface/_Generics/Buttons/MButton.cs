using Godot;
using System;

public partial class MButton : Button
{
	private void OnGuiInput(InputEvent Event)
	{
		if (Event.IsActionPressed("FocusInputYes"))
		{
			GetViewport().SetInputAsHandled();
			EmitSignal(BaseButton.SignalName.Pressed);
		}
	}
}
