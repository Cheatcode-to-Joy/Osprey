using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class CommandManager : Node
{
	public override void _Ready()
	{
		LoadCommands();
	}

	private Dictionary<string, Command> Commands = [];

	private void LoadCommands()
	{
		Commands = JSONReader.ReadJSONFile<Dictionary<string, Command>>("res://Assets/Text/Commands.json", out bool _);
	}

	public void SubmitCommand(string Message)
	{
		string[] Segments = Message.Trim().Split(" ");
		bool Success = Commands.TryGetValue(Segments[0], out Command CurrentCommand);
		if (!Success)
		{
			Router.Debug.Print($"WARNING: Command {Segments[0]} not found.");
			return;
		}

		GD.Print(HasMethod(CurrentCommand.Function));
		Success = (bool)Call(CurrentCommand.Function, (Segments.Length > 1) ? [..Segments.Skip(1)] : System.Array.Empty<string>());
		Router.Debug.Print(Success ? $"Command {Segments[0]} executed." : $"Command {Segments[0]} failed.");
	}

	#region Functions
	private bool CommandClearLog(string[] Arguments)
	{
		Router.Debug.ClearLog();
		return true;
	}

	private bool CommandStartDialogue(string[] Arguments)
	{
		if (Arguments.Length == 0) { return false; }

		CoreHUD HUD = (CoreHUD)Router.Main.FindChild("CoreHUD", false);
		if (HUD == null) { return false; }

		HUD.SpawnDialogue(Arguments[0]);
		return true;
	}

	private bool CommandHelp(string[] Arguments)
	{
		foreach (string CommandName in Commands.Keys)
		{
			Command CommandValue = Commands[CommandName];
			Router.Debug.Print($"{DebugManager.Colourise(CommandName, DebugManager.Colours.YELLOW)}: {CommandValue.CommandDescription}\n");
		}
		return true;
	}
	#endregion
}

#region Command
public class Command
{
	public string CommandName { get; set; } = "NONAME";
	public string CommandDescription { get; set; } = "NODESC";

	public string Function { get; set; } = "";
}
#endregion