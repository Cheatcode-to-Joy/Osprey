using Godot;
using System;

public partial class PosiSFX : AudioStreamPlayer2D, ISoundEffect
{
	#region ISoundEffect
	public void PlaySound(AudioStream Sound)
	{
		Stream = Sound;
		Play();
	}

	public void OnFinished()
	{
		QueueFree();
	}
	#endregion
}
