using System;
using System.Collections.Generic;
using System.Text.Json;

public static class JSONExtractor
{
	private static JsonSerializerOptions Options;

	private static void CreateSettings()
	{
		if (Options == null) { Options = new() { IncludeFields = true }; }
	}

	public static T ReadData<T>(string NodeName, Dictionary<string, JsonElement> Data, Dictionary<string, string> DefaultValues, string Property, bool Extra = false)
	{
		T Value = default;
		try
		{
			if (Data.ContainsKey(Property))
			{
				if (Extra)
				{
					CreateSettings();
					Value = JSONReader.DecodeJSONElementSettings<T>(Data[Property], Options);
				}
				else
				{
					Value = JSONReader.DecodeJSONElement<T>(Data[Property]);
				}
			}
			else
			{
				if (DefaultValues.ContainsKey(Property))
				{
					Value = (T)Convert.ChangeType(DefaultValues[Property], typeof(T));
					OnDefaultData(NodeName, Property, DefaultValues[Property]);
				}
				else
				{
					OnMissingData(NodeName, Property);
				}
			}

			return Value;
		}
		catch (InvalidCastException)
		{
			Router.Debug.Print($"ERROR: Invalid data cast for {Property} in {NodeName} JSON read. Returning default.");
			return Value;
		}
	}

	public static void OnMissingData(string NodeName, string Property)
	{
		Router.Debug.Print($"ERROR: {NodeName} JSON file not in correct format. {Property} missing.");
	}

	public static void OnWrongDataRead(string NodeName, string Property, string Value)
	{
		Router.Debug.Print($"ERROR: {NodeName} JSON file not in correct format. {Property}: {Value}.");
	}

	public static void OnDefaultData(string NodeName, string Property, string Value)
	{
		Router.Debug.Print($"WARNING: Used default value for {NodeName} creation. Specifying a value is encouraged. {Property}: {Value}.");
	}
}
