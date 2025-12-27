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
	}

	public override void _Input(InputEvent @Event)
	{
		if (@Event.IsActionPressed("ToggleSettings"))
		{
			GetViewport().SetInputAsHandled();
			if (!CloseTopOverlay())
			{
				AddOverlay(SettingsScene.Instantiate<UILayer>());
			}
		}
		else if (@Event.IsActionPressed("ToggleDebugOverlay"))
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
		if (CurrentDebug != null && !Router.Config.FetchConfig<bool>("Debug", "DebugEnabled")) { ToggleDebug(); }
	}

	#region Overlays
	[ExportGroup("Overlays")]
	[Export] private PackedScene SettingsScene;
	[Export] private PackedScene DebugScene;

	[Export] private Control OverlayHolder;

	private Dictionary<int, List<UILayer>> ActiveOverlays = [];
	private UILayer CurrentDebug = null;

	public void AddOverlay(UILayer Overlay, int Layer = 0)
	{
		Layer = Math.Clamp(Layer, 0, OverlayHolder.GetChildCount() - 1);
		OverlayHolder.GetChild(Layer).AddChild(Overlay);

		if (!ActiveOverlays.TryGetValue(Layer, out List<UILayer> Value))
		{
			Value = [];
			ActiveOverlays[Layer] = Value;
		}
		Value.Add(Overlay);

		SetTopOverlay();
	}

	public void CloseOverlay(UILayer Overlay)
	{
		if (!Overlay.RequestOverlayExit()) { return; }

		foreach (int Key in ActiveOverlays.Keys)
		{
			if (ActiveOverlays[Key].Remove(Overlay))
			{
				Overlay.QueueFree();
				SetTopOverlay();
				return;
			}
		}
	}

	private UILayer GetTopOverlay()
	{
		List<int> SortedKeys = [.. ActiveOverlays.Keys];
		SortedKeys.Sort();
		SortedKeys.Reverse();

		foreach (int Layer in SortedKeys)
		{
			if (ActiveOverlays[Layer].Count > 0)
			{
				return ActiveOverlays[Layer][^1];
			}
		}
		return null;
	}

	private bool CloseTopOverlay()
	{
		List<int> SortedKeys = [.. ActiveOverlays.Keys];
		SortedKeys.Sort();
		SortedKeys.Reverse();

		foreach (int Layer in SortedKeys)
		{
			if (ActiveOverlays[Layer].Count > 0)
			{
				if (ActiveOverlays[Layer][^1].RequestOverlayExit())
				{
					ActiveOverlays[Layer][^1].QueueFree();
					ActiveOverlays[Layer].RemoveAt(ActiveOverlays[Layer].Count - 1);

					// FIXME. Bad, bad, bad.
					CurrentDebug = null;
					SetTopOverlay();
				}
				return true;
			}
		}
		return false;
	}

	private void SetTopOverlay()
	{
		GetTopOverlay()?.CallDeferred(UILayer.MethodName.GrabDefaultFocus);
	}

	private void ToggleDebug()
	{
		if (CurrentDebug != null)
		{
			CloseOverlay(CurrentDebug);
			CurrentDebug = null;
			return;
		}
		
		AddOverlay(CurrentDebug = DebugScene.Instantiate<UILayer>(), 1);
	}
	#endregion
}
