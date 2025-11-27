using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

public partial class DialogueOverlay : Control
{
	private const string NodeName = "DialogueOverlay";

	[Export] private TextureRect LeftPortrait;
	[Export] private TextureRect RightPortrait;
	[Export] private DialogueTextBox TextBox;

	// Both or neither can speak as well.
	private bool IsLeftSpeaking = false;
	private bool IsRightSpeaking = false;

	private const int PortraitWidth = 144;
	private const int PortraitHeight = 144;
	private const string JSONPathStart = "res://Assets/Text/Dialogue/";
	private const string PortraitPathStart = "res://Assets/Visual/UserInterface/Dialogue/Portraits";
	private const string PortraitExtension = ".png";

	private string DialogueText = "";
	private string[] DialogueSections;

	private Dictionary<string, string> DefaultValues = new()
	{
		{ "LeftPortraitPath", "_Placeholder" },
		{ "RightPortraitPath", "_Placeholder" },
		{ "DialogueText", "" }
	};
	
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

		InitialisePortrait(LeftPortrait, JSONExtractor.ReadData<string>(NodeName, DialogueData, DefaultValues, "LeftPortraitPath"));
		InitialisePortrait(RightPortrait, JSONExtractor.ReadData<string>(NodeName, DialogueData, DefaultValues, "RightPortraitPath"));

		DialogueText = JSONExtractor.ReadData<string>(NodeName, DialogueData, DefaultValues, "DialogueText");
		DialogueSections = DialogueText.Split("{split}");
	}

	private void InitialisePortrait(TextureRect Portrait, string FilePath)
	{
		AtlasTexture Atlas = (AtlasTexture)Portrait.Texture;
		Atlas.Region = new Rect2(0, 0, PortraitWidth, PortraitHeight);

		if (FilePath == "NONE")
		{
			Portrait.Texture = null;
		}
		else
		{
			Texture2D NewTexture;
			try
			{
				NewTexture = GD.Load<Texture2D>($"{PortraitPathStart}{FilePath}{PortraitExtension}");
			}
			catch
			{
				Router.Debug.Print($"ERROR: Dialogue portrait {FilePath} not found.");
				NewTexture = GD.Load<Texture2D>($"{PortraitPathStart}_Placeholder{PortraitExtension}");
			}
			Portrait.Texture = NewTexture;
		}
	}
}
