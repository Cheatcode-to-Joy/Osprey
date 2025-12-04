using Godot;
using System;

public partial class MultiSFX : AudioStreamPlayer
{
	public void Replay()
	{
		if (Stream != null) { Play(); }
	}
}
