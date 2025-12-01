using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

public partial class DialogueSpeaker : Control
{
	private const string NodeName = "DialogueSpeaker";

	[Export] private TextureRect SpeakerPortraitTexture;
	[Export] private RichTextLabel SpeakerNameLabel;

	public int ID;
	private string CodeName = "NO_NAME";

	private const string JSONPathStart = "res://Assets/Text/Dialogue/Speakers/";

	private Dictionary<string, string> DefaultValues = new()
	{
		{ "ID", "0" },
		{ "Name", "_Placeholder" }
	};

	public void SetSpeaker(string JSONPath)
	{
		CodeName = JSONPath;
		JSONPath = JSONPathStart + JSONPath + ".json";
		if (!JSONPath.IsAbsolutePath())
		{
			Router.Debug.Print($"ERROR: Speaker JSON file path not in correct format: {JSONPath}.");
			return;
		}

		Dictionary<string, JsonElement> SpeakerData = JSONReader.ReadJSONFile<Dictionary<string, JsonElement>>(JSONPath, false);
		if (SpeakerData == null)
		{
			return; // Error is already called in JSONReader.
		}

		ID = JSONExtractor.ReadData<int>(NodeName, SpeakerData, DefaultValues, "ID");

		string RawName = JSONExtractor.ReadData<string>(NodeName, SpeakerData, DefaultValues, "Name");
		SpeakerNameLabel.Text = RawName.Substring(0, Math.Min(RawName.Length, (int)(1 + SpeakerNameLabel.Size.X / 8)));

		ExpressionData = JSONExtractor.ReadData<Dictionary<string,(List<(int, float)>, string)>>(NodeName, SpeakerData, DefaultValues, "ExpressionData", true);

		InitialisePortrait();
	}

	private const int PortraitWidth = 144;
	private const int PortraitHeight = 144;
	private const string PortraitPathStart = "res://Assets/Visual/UserInterface/Dialogue/Portraits/";
	private const string PortraitExtension = ".png";

	[Export] private AtlasTexture Atlas;
	private Dictionary<string,(List<(int, float)>, string)> ExpressionData;
	private void InitialisePortrait()
	{
		Atlas.Region = new Rect2(0, 0, PortraitWidth, PortraitHeight);

		ChangeExpression("DEFAULT");
	}

	[Signal] public delegate void ExpressionChangedEventHandler();
	public void ChangeExpression(string ExpressionName)
	{
		if (!ExpressionData.ContainsKey(ExpressionName.ToUpper()))
		{
			Router.Debug.Print($"ERROR: Cannot find expression data for {ExpressionName}.");
			return;
		}

		try
		{
			Atlas.Atlas = GD.Load<Texture2D>($"{PortraitPathStart}{CodeName}/{ExpressionName}{PortraitExtension}");
		}
		catch
		{
			Router.Debug.Print($"ERROR: Dialogue portrait {ExpressionName} not found.");
			return;
		}

		EmitSignal(SignalName.ExpressionChanged);

		CurrentExpression = ExpressionName.ToUpper();
		CurrentFrame = 0;

		Atlas.Region = new Rect2(0, 0, PortraitWidth, PortraitHeight);

		// Animation
		Tween AnimationTween = CreateTween();
		Connect(SignalName.ExpressionChanged, new Callable(AnimationTween, Tween.MethodName.Kill));
		foreach ((int, float) Entry in ExpressionData[ExpressionName.ToUpper()].Item1)
		{
			AnimationTween.TweenCallback(new Callable(this, MethodName.AnimateExpression));
			AnimationTween.TweenInterval(Entry.Item2);
		}
		switch (ExpressionData[ExpressionName.ToUpper()].Item2)
		{
			case "LOOP":
			AnimationTween.SetLoops();
			break;
			case "STAY":
			break;
			default:
			AnimationTween.TweenCallback(new Callable(this, MethodName.PlayDefaultExpression));
			break;
		}

	}

	private string CurrentExpression = "DEFAULT";
	private int CurrentFrame = 0;

	private void AnimateExpression()
	{
		int Frame = ExpressionData[CurrentExpression].Item1[CurrentFrame].Item1;
		Frame = ((Frame + 1) * PortraitWidth) <= Atlas.Atlas.GetSize().X ? Frame : 0;
		Atlas.Region = new Rect2(Frame * PortraitWidth, 0, PortraitWidth, PortraitHeight);
		CurrentFrame = (CurrentFrame + 1) % ExpressionData[CurrentExpression].Item1.Count;
	}

	private void PlayDefaultExpression()
	{
		ChangeExpression("DEFAULT");
	}

	/*
	private class Speaker
	{
		private int ID = 0;
		private string Name = "_Placeholder";
		private string Portrait = "_Placeholder";
		private ExpData ExpressionData;

		private class ExpData
		{
			private Dictionary<string, Expression> Expressions;

			private class Expression
			{
				private List<(int, float)> Timing;
				private string Transition = "DEFAULT";
			}
		}
	}
	*/
}
