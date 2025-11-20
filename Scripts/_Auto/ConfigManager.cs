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
        TranslationServer.SetLocale(FetchConfig("Text", "Language"));
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
        if (FetchConfig(SectionName, ConfigName) == NewValue.ToString()) { return; }

        if (FetchSpecificConfig(SectionName, ConfigName, DefaultConfig) == NewValue.ToString())
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

        if (FetchConfig("Text", "Language") == Language)
        {
            Router.Debug.Print("WARNING: Language already active.");
            return;
        }

        TranslationServer.SetLocale(Language);
    }

    public void SetConfig(string SectionName, string ConfigName, Variant NewValue)
    {
        if (FetchConfig(SectionName, ConfigName) == "")
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
    public string FetchConfig(string SectionName, string ConfigName)
    {
        string UserValue = FetchSpecificConfig(SectionName, ConfigName, UserConfig);
        if (UserValue != "") { return UserValue; }

        string DefaultValue = FetchSpecificConfig(SectionName, ConfigName, DefaultConfig);
        if (DefaultValue != "") { return DefaultValue; }

        return "";
    }

    private string FetchSpecificConfig(string SectionName, string ConfigName, ConfigFile Config)
    {
        return Config.GetValue(SectionName, ConfigName, "").ToString();
    }
    #endregion
}
