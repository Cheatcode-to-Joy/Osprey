using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public partial class DebugManager : Node
{
	public void OnTreeEntered()
	{
		Router.Debug = this;
	}

	public override void _Ready()
	{
		LoadCommands();
		Print($"{ProjectSettings.GetSetting("application/config/name")}");
		Print($"Version {ProjectSettings.GetSetting("application/config/version")}");
	}

	private List<string> Messages = [];
	[Signal] public delegate void MessageSentEventHandler(string Message);
	[Signal] public delegate void LogClearedEventHandler();

	public void Print(string NewMessage)
	{
		if (NewMessage == null || NewMessage == "")
		{
			NewMessage = "WARNING: Attempted printing an empty message.";
		}

		if (NewMessage.StartsWith("ERROR:"))
		{
			NewMessage = Colourise(NewMessage, "FF365E");
		}
		else if (NewMessage.StartsWith("WARNING:"))
		{
			NewMessage = Colourise(NewMessage, "FFD95C");
		}
		Messages.Add(NewMessage);
		EmitSignal(SignalName.MessageSent, NewMessage);
	}

	public void FillLog()
	{
		foreach (string Message in Messages)
		{
			EmitSignal(SignalName.MessageSent, Message);
		}
	}

	public void ClearLog()
	{
		Messages = [];
		EmitSignal(SignalName.LogCleared);
	}

	private static string Colourise(string Message, string Colour)
	{
		return $"[color=#{Colour}]{Message}[/color]";
	}

	#region Commands
	[Export] private CommandManager Commander;
	private void LoadCommands()
	{
		// TODO.
	}

	public void OnCommandSubmitted(string Message)
	{
		Print($"> {Message}");

		Commander.SubmitCommand(Message);
	}
	#endregion
}
