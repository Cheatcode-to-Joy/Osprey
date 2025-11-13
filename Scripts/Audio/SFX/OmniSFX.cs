using Godot;
using System;

public partial class OmniSFX : AudioStreamPlayer, ISoundEffect
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
