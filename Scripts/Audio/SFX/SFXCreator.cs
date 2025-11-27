using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

public partial class SFXCreator : Node
{
	private const string NodeName = "SFXCreator";

	[Export] private PackedScene OmniSFX;
	[Export] private PackedScene PosiSFX;

	private const string FilePathStart = "res://Assets/Audio/SFX/";
	private const string FileExtension = ".mp3";

	private Dictionary<string, string> DefaultValues = new()
	{
		{ "IsAudioPositional", "false" }
	};

	public Variant CreateSFX(string JSONPath)
	{
		JSONPath = FilePathStart + JSONPath + ".json";
		if (!JSONPath.IsAbsolutePath())
		{
			Router.Debug.Print($"ERROR: SFX JSON file path not in correct format: {JSONPath}.");
			return false;
		}

		Dictionary<string, JsonElement> SFXData = JSONReader.ReadJSONFile<Dictionary<string, JsonElement>>(JSONPath, false);
		if (SFXData == null)
		{
			return false; // Error is already called in JSONReader.
		}

		bool IsAudioPositional = JSONExtractor.ReadData<bool>(NodeName, SFXData, DefaultValues, "IsAudioPositional");

		return IsAudioPositional ? CreatePosiSFX(SFXData) : CreateOmniSFX(SFXData);
	}

	private Variant CreateOmniSFX(Dictionary<string, JsonElement> SFXData)
	{
		AudioStreamPlayer SoundEffect = OmniSFX.Instantiate<AudioStreamPlayer>();
		string[][] StreamData = JSONExtractor.ReadData<string[][]>(NodeName, SFXData, DefaultValues, "StreamData");
		SoundEffect.Stream = CreateStream(StreamData);
		if (SoundEffect.Stream == null) { return false; }
		// TODO. More will be added later.

		AddChild(SoundEffect);
		return false;
	}

	private Variant CreatePosiSFX(Dictionary<string, JsonElement> SFXData)
	{
		AudioStreamPlayer2D SoundEffect = PosiSFX.Instantiate<AudioStreamPlayer2D>();
		string[][] StreamData = JSONExtractor.ReadData<string[][]>(NodeName, SFXData, DefaultValues, "StreamData");
		SoundEffect.Stream = CreateStream(StreamData);
		if (SoundEffect.Stream == null) { return false; }
		// TODO. More will be added later.

		return SoundEffect;
	}

	private AudioStream CreateStream(string[][] StreamData)
	{
		if (StreamData == null) { return null; }
		if (StreamData.Length < 1 || (StreamData.Length == 1 && StreamData[0].Length > 1))
		{
			JSONExtractor.OnMissingData(NodeName, "StreamData");
			return null;
		}

		AudioStream NewStream;
		if (StreamData[0].Length == 1)
		{
			try
			{
				NewStream = (AudioStream)ResourceLoader.Load(FilePathStart + StreamData[0][0] + FileExtension);
			}
			catch (Exception)
			{
				Router.Debug.Print($"ERROR: Audio Stream unable to be imported: {FilePathStart + StreamData[0][0] + FileExtension}.");
				return null;
			}
		}
		else
		{
			AudioStreamRandomizer NewRandomizer = new AudioStreamRandomizer();
			try
			{
				if (StreamData[0].Length != StreamData[1].Length) { return null; }
				for (int Index = 0; Index < StreamData[0].Length; Index++)
				{
					NewRandomizer.AddStream(Index, (AudioStream)ResourceLoader.Load(FilePathStart + StreamData[0][Index] + FileExtension), StreamData[1][Index].ToFloat());
				}

				if (StreamData.Length > 2 && StreamData[2].Length >= 2)
				{
					NewRandomizer.RandomPitch = StreamData[2][0].ToFloat();
					NewRandomizer.RandomVolumeOffsetDb = StreamData[2][1].ToFloat();
				}

				NewStream = NewRandomizer;
			}
			catch (Exception)
			{
				Router.Debug.Print($"ERROR: Audio Stream unable to be imported: {StreamData[0][0]} and following...");
				return null;
			}
		}

		return NewStream;
	}
}