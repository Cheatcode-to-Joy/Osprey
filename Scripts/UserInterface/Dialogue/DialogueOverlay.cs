using Godot;
using System.Collections.Generic;

public partial class DialogueOverlay : Control
{
	[Export] private DialogueSpeaker SpeakerLeft;
	[Export] private DialogueSpeaker SpeakerRight;
	[Export] private DialogueTextBox TextBox;

	private Dialogue CDialogue;

	private bool LeftSpeaking = true;

	// FIXME. Delete.
	public override void _Input(InputEvent @Event)
	{
		if (@Event is InputEventKey EventKey && EventKey.Keycode == Key.A && @Event.IsPressed() && !EventKey.Echo) { LoadDialogue("_DEFAULT_DIALOGUE"); }
	}
	
	public void LoadDialogue(string JSONPath)
	{
		string DialogueName = JSONPath;
		JSONPath = $"res://Assets/Text/Dialogue/{JSONPath}.json";

		CDialogue = JSONReader.ReadJSONFile<Dialogue>(JSONPath, out bool Success);
		if (!Success)
		{
			Router.Debug.Print($"ERROR: Dialogue not found: {DialogueName}.");
			CDialogue = JSONReader.ReadJSONFile<Dialogue>($"res://Assets/Text/Dialogue/_DEFAULT_DIALOGUE.json", out _);
		}

		SpeakerLeft.SetSpeaker(CDialogue.SLeftName);
		SpeakerRight.SetSpeaker(CDialogue.SRightName);

		LeftSpeaking = CDialogue.StartOnLeft;

		SpeakerLeft.SetMain(LeftSpeaking);
		SpeakerRight.SetMain(!LeftSpeaking);

		ConnectSpeaker();

		Play();
	}

	private void Play()
	{
		string Locale = Router.Config.FetchConfig<string>("Text", "Language");
		if (!CDialogue.Content.TryGetValue(Locale, out string Value))
		{
			Router.Debug.Print($"ERROR: Current dialogue is not available in locale {Locale}.");
			return;
		}

		TextBox.SetText(Value);
	}

	public void ChangeSpeaker()
	{
		LeftSpeaking = !LeftSpeaking;
		SpeakerLeft.SetMain(LeftSpeaking);
		SpeakerRight.SetMain(!LeftSpeaking);

		DisconnectListener();
		ConnectSpeaker();
	}

	private void DisconnectListener()
	{
		TextBox.Disconnect(DialogueTextBox.SignalName.CharacterTyped, new Callable(GetListener(), DialogueSpeaker.MethodName.PlayTypeSFX));
	}

	private void ConnectSpeaker()
	{
		TextBox.Connect(DialogueTextBox.SignalName.CharacterTyped, new Callable(GetSpeaker(), DialogueSpeaker.MethodName.PlayTypeSFX));
	}

	public DialogueSpeaker GetSpeaker()
	{
		return LeftSpeaking ? SpeakerLeft : SpeakerRight;
	}

	public DialogueSpeaker GetListener()
	{
		return LeftSpeaking ? SpeakerRight : SpeakerLeft;
	}

	public void OnTextFinished()
	{
		// TODO. Expand.
		QueueFree();
	}

	#region Dialogue
	public class Dialogue
	{
		public string SLeftName { get; set; } = "_TestSpeaker";
		public string SRightName { get; set; } = "_TestSpeaker";
		public bool StartOnLeft { get; set; } = true;

		public Dictionary<string, string> Content { get; set; } = [];

		public override string ToString()
		{
			string Result = $"Left Speaker: {SLeftName}\nRight Speaker: {SRightName}\nConversation begins with the {(StartOnLeft ? "left" : "right")}.";
			foreach (string Locale in Content.Keys) { Result += $"\n{Locale}: {Content[Locale]}"; }
			return Result;
		}
	}
	#endregion
}
