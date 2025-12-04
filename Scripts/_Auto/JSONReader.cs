using Godot;
using System;
using System.Text.Json;

public static class JSONReader
{
	public static T ReadJSONFile<T>(string TextFile)
	{
		try
		{
			using FileAccess Reader = FileAccess.Open(TextFile, FileAccess.ModeFlags.Read);
			string Contents = Reader.GetAsText();

			return JsonSerializer.Deserialize<T>(Contents);
		}
		catch (Exception)
		{
			Router.Debug.Print($"ERROR: JSON file read unsuccessful: {TextFile}.");
			return default;
		}
	}
}