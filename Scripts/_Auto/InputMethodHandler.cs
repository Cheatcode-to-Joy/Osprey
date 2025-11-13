using Godot;
using System;

public partial class InputMethodHandler : Node
{
	public enum INPUT_METHODS
	{
		Keyboard,
		Controller
	}
	private INPUT_METHODS InputMethod = INPUT_METHODS.Keyboard; // TODO. Replace default.
	[Signal] public delegate void InputMethodChangedEventHandler(INPUT_METHODS NewMethod);
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
			if (Math.Abs(@JoypadMotionEvent.AxisValue) > AxisOffsetMin) { CheckInput(INPUT_METHODS.Controller); }
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
			CheckInput(INPUT_METHODS.Controller);
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
