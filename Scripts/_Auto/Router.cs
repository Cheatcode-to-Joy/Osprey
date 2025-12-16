public static class Router
{
	private static MainScene MainInstance;
	public static MainScene Main { get => MainInstance; set => MainInstance = value; }

	private static AudioManager AudioInstance;
	public static AudioManager Audio { get => AudioInstance; set => AudioInstance = value; }

	private static DebugManager DebugInstance;
	public static DebugManager Debug { get => DebugInstance; set => DebugInstance = value; }

	private static ConfigManager ConfigInstance;
	public static ConfigManager Config { get => ConfigInstance; set => ConfigInstance = value; }

	private static CheatHandler CheatInstance;
	public static CheatHandler Cheat { get => CheatInstance; set => CheatInstance = value; }

	private static InputMethodHandler InputInstance;
	public static InputMethodHandler Input { get => InputInstance; set => InputInstance = value; }
}