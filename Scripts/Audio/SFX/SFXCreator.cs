using Godot;
using System;
using System.Collections.Generic;

public partial class SFXCreator : Node
{
	[Export] private PackedScene OmniSFX;
	[Export] private PackedScene PosiSFX;
	[Export] private PackedScene MultiSFX;

	private const string AudioFolder = "res://Assets/Audio/SFX/";

	public AudioStream CreateStream(string JSONPath)
	{
		JSONPath = $"{AudioFolder}{JSONPath}.json";

		SoundEffect CSound = JSONReader.ReadJSONFile<SoundEffect>(JSONPath);
		if (CSound == null) { return null; }

		try
		{
			if (CSound.Streams.Count == 0)
			{
				Router.Debug.Print($"ERROR: There are no audio streams in {JSONPath}.");
				return null;
			}

			if (CSound.Streams.Count == 1)
			{
				return GD.Load<AudioStream>($"{AudioFolder}{CSound.Streams[0].File}.mp3");
			}

			AudioStreamRandomizer CStream = new();
			CStream.RandomPitch = CSound.PitchVariation;
			CStream.RandomVolumeOffsetDb = CSound.VolumeVariation;
			foreach (StreamFile Stream in CSound.Streams)
			{
				CStream.AddStream(-1, GD.Load<AudioStream>($"{AudioFolder}{Stream.File}.mp3"), Stream.Weight);
			}
			return CStream;
		}
		catch (Exception)
		{
			Router.Debug.Print($"ERROR: Failed to read audio streams from {JSONPath}.");
			return null;
		}
	}

	public void CreateOmniAudioStream(AudioStream CStream)
	{
		OmniSFX SoundPlayer = OmniSFX.Instantiate<OmniSFX>();
		SoundPlayer.Stream = CStream;

		AddChild(SoundPlayer);
	}

	public PosiSFX CreatePosiAudioStream(AudioStream CStream)
	{
		PosiSFX SoundPlayer = PosiSFX.Instantiate<PosiSFX>();
		SoundPlayer.Stream = CStream;

		return SoundPlayer;
	}

	public MultiSFX CreateMultiAudioStream(AudioStream CStream)
	{
		MultiSFX SoundPlayer = MultiSFX.Instantiate<MultiSFX>();
		SoundPlayer.Stream = CStream;

		AddChild(SoundPlayer);
		return SoundPlayer;
	}

	#region SoundEffect
	public class SoundEffect
	{
		public List<StreamFile> Streams { get; set; } = [];
		public float PitchVariation { get; set; } = 0.0f;
		public float VolumeVariation { get; set; } = 0.0f;

		public override string ToString()
		{
			string Result = $"Pitch variation: {PitchVariation}\nVolume variation: {VolumeVariation}";
			foreach (StreamFile Stream in Streams) { Result += $"\n{Stream}"; }
			return Result;
		}
	}

	public class StreamFile
	{
		public string File { get; set; } = "";
		public float Weight { get; set; } = 1.0f;

		public override string ToString()
		{
			return $"file {File}, weight {Weight}";
		}
	}
	#endregion
}