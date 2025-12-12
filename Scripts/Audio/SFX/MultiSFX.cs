using Godot;
using System;

public partial class MultiSFX : AudioStreamPlayer
{
	public void Replay()
	{
		if (Stream != null) { Play(); }
	}

	public void OnSourceExit()
	{
		if (Playing)
		{
			Connect(AudioStreamPlayer.SignalName.Finished, new Callable(this, Node.MethodName.QueueFree), (int)ConnectFlags.OneShot);
			return;
		}

		QueueFree();
	}
}
