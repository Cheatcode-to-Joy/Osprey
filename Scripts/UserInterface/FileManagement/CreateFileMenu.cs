using Godot;
using System;
using System.Linq;
using System.Text.RegularExpressions;

public partial class CreateFileMenu : Control
{
	[GeneratedRegex("^[A-Za-z]*$")]
	public static partial Regex PermittedInputRegex();

	public override void _Ready()
	{
		SetInputInactive();
		DataInput.Connect(FileDataInput.SignalName.CharacterInput, new Callable(this, MethodName.AppendKeyInput));

		foreach (FileInputField InputField in InputFields)
		{
			InputField.Connect(FileInputField.SignalName.InputStartRequested, new Callable(DataInput, FileDataInput.MethodName.SetFocus));
		}

		// FIXME. A bigger thing, automatically grabbing focus when the current menu becomes active (after exiting console stuff is bad).
		GrabDefaultFocus();
	}

	public override void _Input(InputEvent @Event)
	{
		if (@Event.IsActionPressed("FocusInputNo"))
		{
			GetViewport().SetInputAsHandled();
			ActiveInput?.DeleteCharAtCaret();
			return;
		}
	}


	#region UI Behaviour
	[Export] private FileDataInput DataInput;
	private FileInputField ActiveInput = null;
	private Callable ActiveFocusCallable;
	[Export] private FileInputField[] InputFields;

	public void GrabDefaultFocus()
	{
		InputFields[0].CallDeferred(Control.MethodName.GrabFocus);
	}

	public void GrabActiveFocus()
	{
		ActiveInput?.CallDeferred(Control.MethodName.GrabFocus);
	}

	public void AppendKeyInput(char InputKey)
	{
		ActiveInput?.InsertTextAtCaret(InputKey.ToString());
	}

	private void SetInputActive()
	{
		DataInput.Modulate = new(1, 1, 1, 1);
		DataInput.FocusBehaviorRecursive = FocusBehaviorRecursiveEnum.Inherited;
	}

	private void SetInputInactive()
	{
		DataInput.Modulate = new(1, 1, 1, 0.5f);
		DataInput.FocusBehaviorRecursive = FocusBehaviorRecursiveEnum.Disabled;
		
		ActiveInput = null;
	}

	public void OnTextInputFocusEntered(FileInputField TextInput)
	{
		if (ActiveInput != null && ActiveInput != TextInput) { ActiveInput.ToggleFakeFocus(false); }

		SetInputActive();
		TextInput.ToggleFakeFocus(true);

		ActiveFocusCallable = new Callable(this, MethodName.GrabActiveFocus);

		if (!DataInput.IsConnected(FileDataInput.SignalName.InputDeclined, ActiveFocusCallable))
		{
			DataInput.Connect(FileDataInput.SignalName.InputDeclined, ActiveFocusCallable);
		}

		ActiveInput = TextInput;
	}

	public void OnOtherFocusEntered()
	{
		ActiveInput?.ToggleFakeFocus(false);

		if (DataInput.IsConnected(FileDataInput.SignalName.InputDeclined, ActiveFocusCallable))
		{
			DataInput.Disconnect(FileDataInput.SignalName.InputDeclined, ActiveFocusCallable);
		}

		SetInputInactive();
	}
	#endregion
}
