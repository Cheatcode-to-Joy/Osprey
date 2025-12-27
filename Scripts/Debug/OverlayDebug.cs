using Godot;
using System;

public partial class OverlayDebug : UILayer
{
	[Export] public DebugConsole Console;

	#region UI Layer
	public override void GrabDefaultFocus()
	{
		Console.GrabDefaultFocus();
	}
	#endregion
}
