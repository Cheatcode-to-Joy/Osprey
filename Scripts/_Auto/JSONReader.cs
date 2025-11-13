using System;
using System.Text.Json;
using Godot;

public static class JSONReader
{
	private const string TextFolder = "res://Assets/Text";

	public static T ReadJSONFile<T>(string TextFile, bool TextFolder = true)
	{
		try
		{
			using FileAccess Reader = FileAccess.Open(TextFolder ? $"{TextFolder}/{TextFile}" : TextFile, FileAccess.ModeFlags.Read);
			string Contents = Reader.GetAsText();

			return JsonSerializer.Deserialize<T>(Contents);
		}
		catch (Exception)
		{
			// TODO. Show error: JSON file read unsuccessful (<show>).
			GD.Print("JSON file read unsuccessful.");
			return default;
		}
	}

	public static T DecodeJSONElement<T>(JsonElement Value)
	{
		return JsonSerializer.Deserialize<T>(Value);
	}
}