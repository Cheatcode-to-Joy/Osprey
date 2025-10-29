using Godot;
using System;
using System.Collections.Generic;

public partial class CheatHandler : Node
{
	#region Controls
	private enum CONTROLS
	{
		CanInput, // Received inputs get registered in Cheat Handler.
		CanExecute, // Valid commands execute methods instead of getting queued.
		CompareKeys // Key position is compared against cheat instead of raw input.
	};

	private Dictionary<CONTROLS, bool> Controls = new()
	{
		{ CONTROLS.CanInput, false },
		{ CONTROLS.CanExecute, false },
		{ CONTROLS.CompareKeys, true }
	};
	#endregion
}
