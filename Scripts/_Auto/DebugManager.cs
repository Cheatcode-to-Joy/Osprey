using Godot;
using System;
using System.Collections.Generic;

public partial class DebugManager : Node
{
	public void OnTreeEntered()
	{
		Router.Debug = this;
	}

	public override void _Ready()
	{
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
			NewMessage = Colourise(NewMessage, Colours.RED);
		}
		else if (NewMessage.StartsWith("WARNING:"))
		{
			NewMessage = Colourise(NewMessage, Colours.YELLOW);
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

	public enum Colours { RED, YELLOW }
	public static string Colourise(string Message, Colours Colour)
	{
		string ColourValue = "#FFFFFF";
		switch (Colour)
		{
			case Colours.RED:
			ColourValue = "#FF365E";
			break;
			case Colours.YELLOW:
			ColourValue = "#FFD95C";
			break;
		}

		return $"[color={ColourValue}]{Message}[/color]";
	}

	#region Commands
	[Export] private CommandManager Commander;
	public void OnCommandSubmitted(string Message)
	{
		Print($"> {Message}");

		Commander.SubmitCommand(Message);
	}
	#endregion
}
