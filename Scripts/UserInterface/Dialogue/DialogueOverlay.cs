using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

public partial class DialogueOverlay : Control
{
	private const string NodeName = "DialogueOverlay";

	[Export] private DialogueSpeaker SpeakerLeft;
	[Export] private DialogueSpeaker SpeakerRight;
	[Export] private DialogueTextBox TextBox;

	private const string JSONPathStart = "res://Assets/Text/Dialogue/";

	private Dictionary<string, string> DialogueText;

	private Dictionary<string, string> DefaultValues = new()
	{
		{ "SpeakerLeft", "_Placeholder" },
		{ "SpeakerRight", "_Placeholder" }
	};

	// FIXME. Delete.
	public override void _Input(InputEvent @Event)
	{
		if (@Event is InputEventKey EventKey && EventKey.Keycode == Key.A && @Event.IsPressed() && !EventKey.Echo) { LoadDialogue("_TestDialogue"); }
	}
	
	public void LoadDialogue(string JSONPath)
	{
		JSONPath = JSONPathStart + JSONPath + ".json";
		if (!JSONPath.IsAbsolutePath())
		{
			Router.Debug.Print($"ERROR: Dialogue JSON file path not in correct format: {JSONPath}.");
			return;
		}

		Dictionary<string, JsonElement> DialogueData = JSONReader.ReadJSONFile<Dictionary<string, JsonElement>>(JSONPath, false);
		if (DialogueData == null)
		{
			return; // Error is already called in JSONReader.
		}

		SpeakerLeft.SetSpeaker(JSONExtractor.ReadData<string>(NodeName, DialogueData, DefaultValues, "SpeakerLeft"));
		SpeakerRight.SetSpeaker(JSONExtractor.ReadData<string>(NodeName, DialogueData, DefaultValues, "SpeakerRight"));

		DialogueText = JSONExtractor.ReadData<Dictionary<string, string>>(NodeName, DialogueData, DefaultValues, "DialogueText");

		Play();
	}

	private void Play()
	{
		TextBox.SetText(DialogueText[Router.Config.FetchConfig<string>("Text", "Language")]);
	}

	public void OnTextFinished()
	{
		// TODO. Expand.
		QueueFree();
	}
}
