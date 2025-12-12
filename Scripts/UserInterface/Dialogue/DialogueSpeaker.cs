using Godot;
using System;
using System.Collections.Generic;

public partial class DialogueSpeaker : Control
{
	[ExportGroup("References")]
	[ExportSubgroup("Internal")]
	[Export] private RichTextLabel NameLabel;
	[Export] private TextureRect Portrait;

	[ExportSubgroup("External")]
	[Export] private DialogueTextBox TextBox;

	[ExportGroup("Controls")]
	[Export] private bool Flipped = false;

	private string CodeName = "NO_NAME";
	public Speaker CSpeaker;

	private const int PortraitWidth = 144;
	private const int PortraitHeight = 144;
	private AtlasTexture Atlas;

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
		// TODO. Trim if necessary.
		NameLabel.Text = (Value.Length % 2 == 0) ? Value : $"{Value} ";
	}

	private void InitialisePortrait()
	{
		Atlas = new AtlasTexture { Region = new Rect2(0, 0, PortraitWidth, PortraitHeight) };
		Portrait.Texture = Atlas;
		Portrait.FlipH = Flipped;

		ChangeExpression("DEFAULT");
	}

	private void InitialiseTypeSFX()
	{
		AudioStream Stream = SFXCreator.CreateStream(CSpeaker.TypeSFX);
		if (Stream != null)
		{
			TypePlayer = Router.Audio.SFXMaker.CreateMultiAudioStream(Stream);
			TextBox.Connect(DialogueTextBox.SignalName.TextFinished, new Callable(TypePlayer, MultiSFX.MethodName.OnSourceExit));
		}
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
