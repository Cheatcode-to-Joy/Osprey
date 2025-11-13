using System;
using System.Text.Json;
using Godot;

public static class JSONReader
{
	private const string TextFolder = "res://Assets/Text";

	public static T ReadJSONFile<T>(string TextFile, bool UseFolder = true)
	{
		try
		{
			using FileAccess Reader = FileAccess.Open(UseFolder ? $"{TextFolder}/{TextFile}" : TextFile, FileAccess.ModeFlags.Read);
			string Contents = Reader.GetAsText();

			return JsonSerializer.Deserialize<T>(Contents);
		}
		catch (Exception)
		{
			Router.Debug.Print($"ERROR: JSON file read unsuccessful: {TextFile}.");
			return default;
		}
	}

	public static T DecodeJSONElement<T>(JsonElement Value)
	{
		return JsonSerializer.Deserialize<T>(Value);
	}
}