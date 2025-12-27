using Godot;
using System;

public partial class UILayer : Control
{
	public virtual void GrabDefaultFocus() {}
	public virtual bool RequestOverlayExit() { return true; }
}
