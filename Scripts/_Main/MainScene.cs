using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class MainScene : Node2D, IConfigReliant
{
	[Export] public MainCamera Camera;

	public void OnTreeEntered()
	{
		Router.Main = this;
	}

	public override void _Ready()
	{
		Router.Config.Connect(ConfigManager.SignalName.ConfigChanged, new Callable(this, MethodName.OnConfigUpdate));
		AddOverlay(TitleScene.Instantiate<UILayer>());
	}

	public override void _Input(InputEvent @Event)
	{
		if (@Event.IsActionPressed("ToggleSettings"))
		{
			GetViewport().SetInputAsHandled();
			if (!CloseOverlay(GetTopOverlay()))
			{
				AddOverlay(SettingsScene.Instantiate<UILayer>());
			}
		}
		else if (@Event.IsActionPressed("ToggleDebug"))
		{
			if (Router.Config.FetchConfig<bool>("Debug", "DebugEnabled"))
			{
				GetViewport().SetInputAsHandled();
				ToggleDebug();
			}
		}
	}

	public void OnConfigUpdate()
	{
		if (CurrentDebug != null && !Router.Config.FetchConfig<bool>("Debug", "DebugEnabled")) { CloseOverlay(CurrentDebug); }
	}

	#region Overlays
	[ExportGroup("Overlays")]
	[Export] private PackedScene TitleScene;
	[Export] private PackedScene SettingsScene;
	[Export] private PackedScene DebugScene;

	[Export] private Control OverlayHolder;

	private List<UILayer> ActiveOverlays = [];
	private UILayer CurrentDebug = null;

	public void AddOverlay(UILayer Overlay, bool Debug = false)
	{
		OverlayHolder.GetChild(Debug ? 1 : 0).AddChild(Overlay);
		if (!Debug) { ActiveOverlays.Add(Overlay); }
		else { CurrentDebug = Overlay; }

		SetTopOverlay(Debug);
	}

	private UILayer GetTopOverlay(bool IncludeDebug = false)
	{
		if (IncludeDebug && CurrentDebug != null) { return CurrentDebug; }
		if (ActiveOverlays.Count > 0) { return ActiveOverlays[^1]; }
		return null;
	}

	public void SetTopOverlay(bool IncludeDebug = false)
	{
		GetTopOverlay(IncludeDebug)?.CallDeferred(UILayer.MethodName.GrabDefaultFocus);
	}

	public bool CloseOverlay(UILayer Overlay)
	{
		if (!Overlay.RequestOverlayExit()) { return false; }

		if (Overlay == CurrentDebug) { CurrentDebug = null; }
		else if (!ActiveOverlays.Remove(Overlay)) { return false; }

		Overlay.QueueFree();
		SetTopOverlay();

		return true;
	}

	private void ToggleDebug()
	{
		if (CurrentDebug != null)
		{
			CloseOverlay(CurrentDebug);
			return;
		}
		
		AddOverlay(CurrentDebug = DebugScene.Instantiate<UILayer>(), true);
	}
	#endregion
}
