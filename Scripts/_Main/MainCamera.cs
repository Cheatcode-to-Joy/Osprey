using Godot;
using System;

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

    public async void ShakeCamera(float ShakeStrength = DefaultShakeStrength, float ShakeDuration = DefaultShakeDuration, float ShakeFade = DefaultShakeFade)
	{
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
