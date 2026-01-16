using Godot;
using System;
using System.Linq;
using System.Text.RegularExpressions;

public partial class CreateFileOverlay : UILayer
{
	[GeneratedRegex("^[A-Za-z]*$")]
	public static partial Regex PermittedInputRegex();

	public override void _Ready()
	{
		DataInput.OnDisable();
		DataInput.Connect(FileDataInput.SignalName.CharacterInput, new Callable(this, MethodName.AppendKeyInput));

		foreach (FileInputField InputField in InputFields)
		{
			InputField.Connect(FileInputField.SignalName.InputStartRequested, new Callable(DataInput, FileDataInput.MethodName.SetFocus));
		}
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

	#region UI Layer
	public override void GrabDefaultFocus()
	{
		InputFields[0].CallDeferred(Control.MethodName.GrabFocus);
	}
	#endregion

	#region UI Behaviour
	[Export] private FileDataInput DataInput;
	private FileInputField ActiveInput = null;
	private Callable ActiveFocusCallable;
	[Export] private FileInputField[] InputFields;

	public void GrabActiveFocus()
	{
		ActiveInput?.CallDeferred(Control.MethodName.GrabFocus);
	}

	public void AppendKeyInput(char InputKey)
	{
		ActiveInput?.InsertTextAtCaret(InputKey.ToString());
	}

	public void OnTextInputFocusEntered(FileInputField TextInput)
	{
		if (ActiveInput != null && ActiveInput != TextInput) { ActiveInput.ToggleFakeFocus(false); }

		DataInput.OnEnable();
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

		DataInput.OnDisable();

		ActiveInput = null;
	}
	#endregion
}
