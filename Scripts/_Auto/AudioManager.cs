using Godot;
using System;

public partial class AudioManager : Node
{
	[Export] public SFXCreator SFXMaker;

	public void OnTreeEntered()
	{
		Router.Audio = this;
	}
}
