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

	private const string JSONPathStart = "res://Assets/Text/Dialogue/Speakers/";

	private Dictionary<string, string> DefaultValues = new()
	{
		{ "ID", "0" },
		{ "Name", "_Placeholder" },
		{ "Portrait", "_Placeholder" }
	};

	public void SetSpeaker(string JSONPath)
	{
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

		InitialisePortrait(JSONExtractor.ReadData<string>(NodeName, SpeakerData, DefaultValues, "Portrait"));
	}

	private const int PortraitWidth = 144;
	private const int PortraitHeight = 144;
	private const string PortraitPathStart = "res://Assets/Visual/UserInterface/Dialogue/Portraits/";
	private const string PortraitExtension = ".png";

	[Export] private AtlasTexture Atlas;
	private void InitialisePortrait(string FilePath)
	{
		Atlas.Region = new Rect2(0, 0, PortraitWidth, PortraitHeight);

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
	}
}
