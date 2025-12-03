using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

public partial class DialogueSpeaker : Control
{
	[Export] private RichTextLabel NameLabel;
	[Export] private AtlasTexture Atlas;

	private string CodeName = "NO_NAME";
	public Speaker CSpeaker;

	private const int PortraitWidth = 144;
	private const int PortraitHeight = 144;

	[Signal] public delegate void ExpressionChangedEventHandler();

	public void SetSpeaker(string JSONPath)
	{
		CodeName = JSONPath;
		JSONPath = $"Dialogue/Speakers/{JSONPath}.json";

		CSpeaker = JSONReader.ReadJSONFile<Speaker>(JSONPath);

		InitialiseName();
		InitialisePortrait();
	}

	private void InitialiseName()
	{
		// FIXME. Redo, probably?
		NameLabel.Text = CSpeaker.Name[..Math.Min(CSpeaker.Name.Length, (int)(1 + NameLabel.Size.X / 8))];
	}

	private void InitialisePortrait()
	{
		Atlas.Region = new Rect2(0, 0, PortraitWidth, PortraitHeight);

		ChangeExpression("DEFAULT");
	}

	public void ChangeExpression(string ExpressionName)
	{
		if (!CSpeaker.ExpressionData.ContainsKey(ExpressionName.ToUpper()))
		{
			Router.Debug.Print($"ERROR: Cannot find expression data for {ExpressionName}.");
			return;
		}

		try
		{
			Atlas.Atlas = GD.Load<Texture2D>($"res://Assets/Visual/UserInterface/Dialogue/Portraits/{CodeName}/{ExpressionName}.png");
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
		foreach (FrameData Data in CSpeaker.ExpressionData[ExpressionName.ToUpper()].Timing)
		{
			AnimationTween.TweenCallback(new Callable(this, MethodName.AnimateExpression));
			AnimationTween.TweenInterval(Data.Time);
		}
		switch (CSpeaker.ExpressionData[ExpressionName.ToUpper()].Transition)
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

	// FIXME. Figure out a better way. Why no bind in C#????
	private string CurrentExpression = "DEFAULT";
	private int CurrentFrame = 0;

	private void AnimateExpression()
	{
		int Frame = CSpeaker.ExpressionData[CurrentExpression].Timing[CurrentFrame].Frame;
		Frame = ((Frame + 1) * PortraitWidth) <= Atlas.Atlas.GetSize().X ? Frame : 0;
		Atlas.Region = new Rect2(Frame * PortraitWidth, 0, PortraitWidth, PortraitHeight);
		CurrentFrame = (CurrentFrame + 1) % CSpeaker.ExpressionData[CurrentExpression].Timing.Count;
	}

	private void PlayDefaultExpression()
	{
		ChangeExpression("DEFAULT");
	}

	public void SetMain(bool IsMain)
	{
		Modulate = new Color(1, 1, 1, IsMain ? 1 : 0.5f);
	}

	#region Speaker
	public class Speaker
	{
		public int ID { get; set; }
		public string Name { get; set; }
		public Dictionary<string, Expression> ExpressionData { get; set; } = [];

		public override string ToString()
		{
			string Result = $"ID: {ID}\nName: {Name}";
			foreach (string Expression in ExpressionData.Keys) { Result += $"\n{Expression}: {ExpressionData[Expression]}"; }
			return Result;
		}
	}

	public class Expression
	{
		public List<FrameData> Timing { get; set; }
		public string Transition { get; set; } = "DEFAULT";

		public override string ToString()
		{
			return $"[TimingData: {{{string.Join(", ", Timing)}}}, Transition: {Transition}]";
		}
	}

	public class FrameData
	{
		public int Frame { get; set; }
		public float Time { get; set; }

		public override string ToString()
		{
			return $"{Frame}: {Time}";
		}
	}
	#endregion
}
