using Godot;
using System;
using System.Collections.Generic;

public partial class MainCamera : Camera2D
{
	private const float DefaultShakeStrength = 10.0f;
	private const float DefaultShakeDuration = 0.0f;
	private const float DefaultShakeFade = 2.0f;

	private Vector2 BaseOffset;

	private Random ShakeRandom = new();

    public override void _Ready()
    {
        BaseOffset = Offset;
    }

    public async void ShakeCamera(Dictionary<string, float> Arguments)
	{
		float ShakeStrength = Arguments.TryGetValue("Strength", out float Value) ? Value : DefaultShakeStrength;
		float ShakeDuration = Arguments.TryGetValue("Duration", out Value) ? Value : DefaultShakeDuration;
		float ShakeFade = Arguments.TryGetValue("Fade", out Value) ? Value : DefaultShakeFade;

		while (ShakeDuration > 0.0f)
		{
			Vector2 ShakeAmount = new Vector2(
				(float)((ShakeRandom.NextDouble() * ShakeStrength * 2) - ShakeStrength),
				(float)((ShakeRandom.NextDouble() * ShakeStrength * 2) - ShakeStrength));
			Offset = BaseOffset + ShakeAmount;
			ShakeDuration = Mathf.MoveToward(ShakeDuration, 0.0f, 0.05f);
			await ToSignal(GetTree().CreateTimer(0.05), SceneTreeTimer.SignalName.Timeout);
		}

		while (ShakeStrength > 0.0f)
		{
			Vector2 ShakeAmount = new Vector2(
				(float)((ShakeRandom.NextDouble() * ShakeStrength * 2) - ShakeStrength),
				(float)((ShakeRandom.NextDouble() * ShakeStrength * 2) - ShakeStrength));
			
			Offset = BaseOffset + ShakeAmount;
			ShakeStrength = Mathf.MoveToward(ShakeStrength, 0.0f, ShakeFade);
			await ToSignal(GetTree().CreateTimer(0.05), SceneTreeTimer.SignalName.Timeout);
		}

		Offset = BaseOffset;
	}
}
