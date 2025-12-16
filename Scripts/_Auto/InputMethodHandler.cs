using Godot;
using System;

public partial class InputMethodHandler : Node
{
	public void OnTreeEntered()
	{
		Router.Input = this;
	}

	public enum INPUT_METHODS
	{
		Keyboard,
		Joypad
	}
	public INPUT_METHODS InputMethod = INPUT_METHODS.Keyboard; // TODO. Replace default.
	[Signal] public delegate void InputMethodChangedEventHandler(int NewMethod);
	private const float AxisOffsetMin = 0.1f;

	public override void _Input(InputEvent @Event)
	{
		if (@Event.IsEcho()) { return; }

		if (@Event is InputEventMouseMotion @MouseMotionEvent)
		{
			if (!@MouseMotionEvent.Relative.IsZeroApprox()) { CheckInput(INPUT_METHODS.Keyboard); }
			return;
		}
		if (@Event is InputEventJoypadMotion @JoypadMotionEvent)
		{
			if (Math.Abs(@JoypadMotionEvent.AxisValue) > AxisOffsetMin) { CheckInput(INPUT_METHODS.Joypad); }
			return;
		}

		if (!@Event.IsPressed()) { return; }

		if (@Event is InputEventKey || @Event is InputEventMouseButton)
		{
			CheckInput(INPUT_METHODS.Keyboard);
			return;
		}

		if (@Event is InputEventJoypadButton)
		{
			CheckInput(INPUT_METHODS.Joypad);
			return;
		}
	}

	private void CheckInput(INPUT_METHODS NewMethod)
	{
		if (NewMethod != InputMethod) { ChangeInputMethod(NewMethod); }
	}

	private void ChangeInputMethod(INPUT_METHODS NewMethod)
	{
		InputMethod = NewMethod;
		EmitSignal(SignalName.InputMethodChanged, (int)InputMethod);
	}
}
