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

	private List<string> Messages = new();
	[Signal] public delegate void MessageSentEventHandler(string Message);

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

	private string Colourise(string Message, string Colour)
	{
		return $"[color=#{Colour}]{Message}[/color]";
	}
}
