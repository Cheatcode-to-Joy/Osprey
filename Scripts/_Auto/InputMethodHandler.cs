using Godot;
using System;
using System.Collections.Generic;

public partial class InputMethodHandler : Node
{
	public void OnTreeEntered()
	{
		Router.Input = this;
	}

	public enum INPUT_METHODS { Keyboard, Joypad }
	public INPUT_METHODS InputMethod = INPUT_METHODS.Keyboard; // TODO. Replace default.

	public enum CONTROLLER_BUTTONS { BAXY, ABYX, CXST } // clockwise, starting from the right
	public CONTROLLER_BUTTONS ControllerButtons = CONTROLLER_BUTTONS.BAXY; // TODO. Replace default.
	public Dictionary<CONTROLLER_BUTTONS, InputButton[]> InputButtons = new()
	{
		{CONTROLLER_BUTTONS.BAXY, [
			new InputButton(new Vector2(26, 18), new Color(208, 66, 66)),
			new InputButton(new Vector2(18, 18), new Color(60, 219, 78)),
			new InputButton(new Vector2(18, 24), new Color(64, 204, 208)),
			new InputButton(new Vector2(26, 24), new Color(236, 219, 51))]},
		{CONTROLLER_BUTTONS.ABYX, [
			new InputButton(new Vector2(18, 18), new Color(191, 91, 91)),
			new InputButton(new Vector2(26, 18), new Color(198, 185, 85)),
			new InputButton(new Vector2(26, 24), new Color(134, 180, 96)),
			new InputButton(new Vector2(18, 24), new Color(60, 141, 136))]},
		{CONTROLLER_BUTTONS.CXST, [
			new InputButton(new Vector2(42, 24), new Color(255, 102, 102)),
			new InputButton(new Vector2(42, 18), new Color(124, 178, 232)),
			new InputButton(new Vector2(34, 24), new Color(255, 105, 248)),
			new InputButton(new Vector2(34, 18), new Color(64, 226, 160))]}
	};

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

public class InputButton(Vector2 Pos, Color Col)
{
	public Vector2 IconPosition = Pos;
	public Color ButtonColour = Col;
}