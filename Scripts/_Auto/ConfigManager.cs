using Godot;
using System;
using System.Linq;

public partial class ConfigManager : Node
{
	public void OnTreeEntered()
	{
		Router.Config = this;
		LoadDefaultConfig();
		LoadUserConfig();
	}

	public override void _Ready()
	{
		TranslationServer.SetLocale(FetchConfig<string>("Text", "Language"));
	}

	private const string UserConfigPath = "user://Config.cfg";
	private ConfigFile UserConfig = new();

	private const string DefaultConfigPath = "res://Assets/Text/DefaultConfig.cfg";
	private ConfigFile DefaultConfig = new();

	#region LOADING
	private void LoadUserConfig()
	{
		Error LoadError = UserConfig.Load(UserConfigPath);

		if (LoadError != Error.Ok)
		{
			CreateUserConfig();
			return;
		}
	}

	private void CreateUserConfig()
	{
		SaveConfig(DefaultConfig);
	}

	private void LoadDefaultConfig()
	{
		Error LoadError = DefaultConfig.Load(DefaultConfigPath);

		if (LoadError != Error.Ok)
		{
			Router.Debug.Print("ERROR: Failed to load default config.");
			return;
		}
	}
	#endregion

	#region SAVING
	[Signal] public delegate void ConfigChangedEventHandler();
	private void SaveConfig(ConfigFile ThisConfig)
	{
		ThisConfig.Save(UserConfigPath);
		EmitSignal(SignalName.ConfigChanged);
	}

	public void ResetConfig()
	{
		CreateUserConfig();
	}
	#endregion

	#region MODIFYING
	private void ModifyConfig(string SectionName, string ConfigName, Variant NewValue)
	{
		if (FetchConfig<Variant>(SectionName, ConfigName).Equals(NewValue))
		{
			Router.Debug.Print($"Attempted to rewrite value of {ConfigName} with equal value: {NewValue}");
			return;
		}

		if (FetchSpecificConfig<Variant>(SectionName, ConfigName, DefaultConfig).Equals(NewValue))
		{
			UserConfig.EraseSectionKey(SectionName, ConfigName);
		}
		else
		{
			UserConfig.SetValue(SectionName, ConfigName, NewValue);
		}

		SaveConfig(UserConfig);
	}

	private string[] ValidLanguages = { "en" };
	public void ChangeLanguage(string Language)
	{
		Language = Language.Trim().ToLower();

		if (!ValidLanguages.Contains(Language))
		{
			Router.Debug.Print("ERROR: Invalid language to switch to.");
			return;
		}

		if (FetchConfig<string>("Text", "Language") == Language)
		{
			Router.Debug.Print("WARNING: Language already active.");
			return;
		}

		TranslationServer.SetLocale(Language);
	}

	public void SetConfig(string SectionName, string ConfigName, Variant NewValue)
	{
		if (!DefaultConfig.HasSectionKey(SectionName, ConfigName))
		{
			Router.Debug.Print($"ERROR: Config value not found: Section {SectionName}, Config {ConfigName}.");
			return;
		}

		switch (ConfigName)
		{
			case "Language":
				ChangeLanguage(NewValue.ToString());
				break;
		}

		ModifyConfig(SectionName, ConfigName, NewValue);
	}
	#endregion

	#region FETCHING
	private const string UnsetValueCode = "UNSET_VALUE";
	public T FetchConfig<T>(string SectionName, string ConfigName)
	{
		string SUserValue = FetchConfigAsString(SectionName, ConfigName, UserConfig);
		if (SUserValue != UnsetValueCode) { return ConvertValue<T>(SUserValue); }

		string SDefaultValue = FetchConfigAsString(SectionName, ConfigName, DefaultConfig);
		if (SDefaultValue != UnsetValueCode) { return ConvertValue<T>(SDefaultValue); }

		return default;
	}

	private T FetchSpecificConfig<T>(string SectionName, string ConfigName, ConfigFile Config)
	{
		string SValue = FetchConfigAsString(SectionName, ConfigName, Config);
		if (SValue != UnsetValueCode) { return ConvertValue<T>(SValue); }

		return default;
	}

	private string FetchConfigAsString(string SectionName, string ConfigName, ConfigFile Config)
	{
		return Config.GetValue(SectionName, ConfigName, UnsetValueCode).ToString();
	}

	private T ConvertValue<T>(string StringValue)
	{
		try
		{
			T TValue = (T)Convert.ChangeType(StringValue, typeof(T));
			return TValue;
		}
		catch (InvalidCastException)
		{
			Router.Debug.Print($"ERROR: Invalid cast of {StringValue} into {typeof(T)}.");
			return default;
		}
	}
    #endregion
}
