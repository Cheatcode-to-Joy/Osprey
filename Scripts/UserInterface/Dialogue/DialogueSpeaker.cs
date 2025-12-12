using Godot;
using System;
using System.Collections.Generic;

public partial class DialogueSpeaker : Control
{
	[Export] private RichTextLabel NameLabel;
	[Export] private AtlasTexture Atlas;

	private string CodeName = "NO_NAME";
	public Speaker CSpeaker;

	private const int PortraitWidth = 144;
	private const int PortraitHeight = 144;

	private MultiSFX TypePlayer;

	[Signal] public delegate void ExpressionChangedEventHandler();

	public void SetSpeaker(string JSONPath)
	{
		CodeName = JSONPath;
		JSONPath = $"res://Assets/Text/Dialogue/Speakers/{JSONPath}.json";

		CSpeaker = JSONReader.ReadJSONFile<Speaker>(JSONPath, out bool Success);
		if (!Success)
		{
			Router.Debug.Print($"ERROR: Speaker not found: {CodeName}.");
			CSpeaker = JSONReader.ReadJSONFile<Speaker>($"res://Assets/Text/Dialogue/Speakers/_DEFAULT_SPEAKER.json", out _);
		}

		InitialiseName();
		InitialisePortrait();
		InitialiseTypeSFX();
	}

	private void InitialiseName()
	{
		string Locale = Router.Config.FetchConfig<string>("Text", "Language");
		if (!CSpeaker.Name.TryGetValue(Locale, out string Value))
		{
			Router.Debug.Print($"ERROR: Speaker name is not available in locale {Locale}.");
			Value = "NONAME";
		}
		NameLabel.Text = Value[..Math.Min(Value.Length, (int)(1 + NameLabel.Size.X / 8))];
	}

	private void InitialisePortrait()
	{
		Atlas.Region = new Rect2(0, 0, PortraitWidth, PortraitHeight);

		ChangeExpression("DEFAULT");
	}

	private void InitialiseTypeSFX()
	{
		AudioStream Stream = Router.Audio.SFXMaker.CreateStream(CSpeaker.TypeSFX);
		if (Stream != null) { TypePlayer = Router.Audio.SFXMaker.CreateMultiAudioStream(Stream); }
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

		Atlas.Region = new Rect2(0, 0, PortraitWidth, PortraitHeight);

		// Animation
		Tween AnimationTween = CreateTween();
		Connect(SignalName.ExpressionChanged, new Callable(AnimationTween, Tween.MethodName.Kill));
		foreach (FrameData Data in CSpeaker.ExpressionData[ExpressionName.ToUpper()].Timing)
		{
			AnimationTween.TweenCallback(Callable.From(() => AnimateExpression(Data.Frame)));
			AnimationTween.TweenInterval(Data.Time);
		}

		string TransitionType = CSpeaker.ExpressionData[ExpressionName.ToUpper()].Transition;
		switch (TransitionType)
		{
			case "LOOP":
			AnimationTween.SetLoops();
			break;
			case "STAY":
			break;
			default:
			AnimationTween.TweenCallback(Callable.From(() => ChangeExpression(TransitionType)));
			break;
		}
	}

	private void AnimateExpression(int Frame)
	{
		Frame = ((Frame + 1) * PortraitWidth) <= Atlas.Atlas.GetSize().X ? Frame : 0;
		Atlas.Region = new Rect2(Frame * PortraitWidth, 0, PortraitWidth, PortraitHeight);
	}

	public void PlayTypeSFX()
	{
		TypePlayer?.Replay();
	}

	public void SetMain(bool IsMain)
	{
		// TODO. Replace with animations.
		Modulate = new Color(1, 1, 1, IsMain ? 1 : 0.5f);
	}

	public void OnTreeExiting()
	{
		if (TypePlayer == null) { return; }

		if (TypePlayer.Playing)
		{
			TypePlayer.Connect(AudioStreamPlayer.SignalName.Finished, new Callable(TypePlayer, Node.MethodName.QueueFree));
		}
		else
		{
			TypePlayer.QueueFree();
		}
	}

	#region Speaker
	public class Speaker
	{
		public int ID { get; set; } = 0;
		public Dictionary<string, string> Name { get; set; } = [];
		public string TypeSFX { get; set; }
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
		public int Frame { get; set; } = 0;
		public float Time { get; set; } = 0.5f;

		public override string ToString()
		{
			return $"{Frame}: {Time}";
		}
	}
	#endregion
}
