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
	private const string PortraitPathStart = "res://Assets/Visual/UserInterface/Dialogue/Portraits/";
	private const string PortraitExtension = ".png";

	private Dictionary<string, string> DialogueText;

	private Dictionary<string, string> DefaultValues = new()
	{
		{ "LeftPortraitPath", "_Placeholder" },
		{ "RightPortraitPath", "_Placeholder" }
	};

	// FIXME. Delete.
	public override void _Input(InputEvent @Event)
	{
		if (@Event is InputEventKey EventKey && EventKey.Keycode == Key.A && @Event.IsPressed() && !EventKey.Echo) { LoadDialogue("_Test"); }
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

		InitialisePortrait(LeftPortrait, JSONExtractor.ReadData<string>(NodeName, DialogueData, DefaultValues, "LeftPortraitPath"));
		InitialisePortrait(RightPortrait, JSONExtractor.ReadData<string>(NodeName, DialogueData, DefaultValues, "RightPortraitPath"));

		DialogueText = JSONExtractor.ReadData<Dictionary<string, string>>(NodeName, DialogueData, DefaultValues, "DialogueText");

		Play();
	}

	private void InitialisePortrait(TextureRect Portrait, string FilePath)
	{
		AtlasTexture Atlas = new()
		{
			Region = new Rect2(0, 0, PortraitWidth, PortraitHeight)
		};

		if (FilePath == "NONE")
		{
			Atlas.Atlas = null;
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
			Atlas.Atlas = NewTexture;
		}
		Portrait.Texture = Atlas;
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
