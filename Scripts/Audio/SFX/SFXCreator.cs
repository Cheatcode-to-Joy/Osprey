using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

public partial class SFXCreator : Node
{
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

		bool IsAudioPositional = ReadData<bool>(SFXData, "IsAudioPositional");

		return IsAudioPositional ? CreatePosiSFX(SFXData) : CreateOmniSFX(SFXData);
	}

	private Variant CreateOmniSFX(Dictionary<string, JsonElement> SFXData)
	{
		AudioStreamPlayer SoundEffect = OmniSFX.Instantiate<AudioStreamPlayer>();
		string[][] StreamData = ReadData<string[][]>(SFXData, "StreamData");
		SoundEffect.Stream = CreateStream(StreamData);
		if (SoundEffect.Stream == null) { return false; }
		// TODO. More will be added later.

		AddChild(SoundEffect);
		return false;
	}

	private Variant CreatePosiSFX(Dictionary<string, JsonElement> SFXData)
	{
		AudioStreamPlayer2D SoundEffect = PosiSFX.Instantiate<AudioStreamPlayer2D>();
		string[][] StreamData = ReadData<string[][]>(SFXData, "StreamData");
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
			OnMissingData("StreamData");
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

	private T ReadData<T>(Dictionary<string, JsonElement> SFXData, string Property)
	{
		T Value = default;
		try
		{
			if (SFXData.ContainsKey(Property))
			{
				Value = JSONReader.DecodeJSONElement<T>(SFXData[Property]);
			}
			else
			{
				if (DefaultValues.ContainsKey(Property))
				{
					Value = (T)Convert.ChangeType(DefaultValues[Property], typeof(T));
					OnDefaultData(Property, DefaultValues[Property]);
				}
				else
				{
					OnMissingData(Property);
				}
			}

			return Value;
		}
		catch (InvalidCastException)
		{
			Router.Debug.Print($"ERROR: Invalid data cast for {Property} in SFX JSON read. Returning default.");
			return Value;
		}
	}

	private void OnMissingData(string Property)
	{
		Router.Debug.Print($"ERROR: SFX JSON file not in correct format. {Property} missing.");
	}

	private void OnWrongDataRead(string Property, string Value)
	{
		Router.Debug.Print($"ERROR: SFX JSON file not in correct format. {Property}: {Value}.");
	}

	private void OnDefaultData(string Property, string Value)
	{
		Router.Debug.Print($"WARNING: Used default value for SFX creation. Specifying a value is encouraged. {Property}: {Value}.");
	}
}