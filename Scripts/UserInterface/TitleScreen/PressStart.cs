using Godot;
using System;

public partial class PressStart : Control
{
	[Export] private AnimationPlayer APlayer;
	[Export] private Timer DisappearTimer;

	private const float BlinkMultiplier = 8.0f;
	private const float DisappearTime = 0.7f;

	private enum STATE { AWAITING, ACTING }
	private STATE CurrentState = STATE.AWAITING;

	public override void _Input(InputEvent @Event)
	{
		if (CurrentState != STATE.AWAITING) { return; }
		if (@Event is InputEventKey || @Event is InputEventMouseButton || @Event is InputEventJoypadButton)
		{
			OnKeyPressed();
		}
	}

	[Signal] public delegate void KeyPressedEventHandler();
	private void OnKeyPressed()
	{
		CurrentState = STATE.ACTING;
		APlayer.SpeedScale = BlinkMultiplier;
		DisappearTimer.Start(DisappearTime);
	}

	public void OnDisappearTimerTimeout()
	{
		EmitSignal(SignalName.KeyPressed);
		QueueFree();
	}
}
