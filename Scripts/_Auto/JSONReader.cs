using Godot;
using System;
using System.Text.Json;

public static class JSONReader
{
	public static T ReadJSONFile<T>(string TextFile, out bool Success)
	{
		try
		{
			using FileAccess Reader = FileAccess.Open(TextFile, FileAccess.ModeFlags.Read);
			string Contents = Reader.GetAsText();

			T Result = JsonSerializer.Deserialize<T>(Contents);
			Success = true;
			return Result;
		}
		catch (Exception)
		{
			Router.Debug.Print($"ERROR: JSON file read unsuccessful: {TextFile}.");
			Success = false;
			return default;
		}
	}
}