using Godot;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public partial class DialogueTextBox : NinePatchRect, IConfigReliant
{
	[Export] private RichTextLabel TextLabel;

	private float TextSpeed;
	private const float MinTextSpeed = 0.02f;
	private const float MaxTextSpeed = 1.0f;

	private float TextSpeedMod = 1;

	private bool ProgressingTextButton = false;
	private const float TextSpeedProgress = 0.05f;

	[Signal] public delegate void InputRegisteredEventHandler();
	[Signal] public delegate void TextFinishedEventHandler();
	private bool SignalEmitted = false;

	public override void _Ready()
	{
		OnConfigUpdate();
	}
	
	public override void _Input(InputEvent @Event)
	{
		if (Input.IsActionJustPressed("ProgressDialogue")) { OnStartProgressDialogue(); }
		if (Input.IsActionJustReleased("ProgressDialogue")) { OnStopProgressDialogue(); }
	}

	public void OnConfigUpdate()
	{
		TextSpeed = Math.Clamp(Router.Config.FetchConfig<float>("Text", "TextSpeed"), MinTextSpeed, MaxTextSpeed);
	}

	public void OnStartProgressDialogue()
	{
		EmitSignal(SignalName.InputRegistered);

		ProgressingTextButton = true;

		if (!SignalEmitted && !PrintingText)
		{
			SignalEmitted = true;
			EmitSignal(SignalName.TextFinished);
			return;
		}
	}
	
	public void OnStopProgressDialogue()
	{
		ProgressingTextButton = false;
	}

	public void SetText(string NewText)
	{
		TextLabel.Text = "";
		TextLabel.VisibleCharacters = 0;

		CurrentText = NewText;
		ScanText();
		PrintText();
	}

	string CurrentText = "";
	string[] TextSections;

	private void ScanText()
	{
		TextSections = new Regex("({.+?})").Split(CurrentText);
	}

	[Export] private Timer PrintTimer;
	private bool PrintingText = false;
	private int ProcessedSections = 0;
	private async void PrintText()
	{
		ProcessedSections = 0;

		PrintingText = true;

		foreach (string Entry in TextSections)
		{
			if (Entry.Length < 1) { continue; }

			if (Entry.StartsWith("{")) { await ExecuteCommand(Entry); }
			else
			{
				TextLabel.AppendText(Entry);
				int TextLength = TextLabel.GetParsedText().Length;
				while (TextLabel.VisibleCharacters < TextLength)
				{
					TextLabel.VisibleCharacters++;
					PrintTimer.Start(TextSpeed * TextSpeedMod * (ProgressingTextButton ? TextSpeedProgress : 1));
					await ToSignal(PrintTimer, Timer.SignalName.Timeout);
				}
			}

			ProcessedSections++;
		}

		PrintingText = false;
	}

	private async Task ExecuteCommand(string RawCommand)
	{
		RawCommand = Regex.Replace(RawCommand, "[{}]", "");
		string[] Parametres = RawCommand.Split(" ");

		string[] Arguments = Parametres.Length > 1 ? Parametres.Skip(1).ToArray() : [];

		Router.Debug.Print($"Executing in-line command {RawCommand}.");

		switch (Parametres[0].ToLower())
		{
			case "split":
			await OnInlineSplit(Arguments);
			break;
			case "hold":
			await OnInlineHold(Arguments);
			break;
			case "pause":
			await OnInlinePause(Arguments);
			break;
			case "speed":
			await OnInlineSpeed(Arguments);
			break;
			default:
			Router.Debug.Print($"WARNING: Invalid in-line command {Parametres[0].ToLower()}.");
			return;
		}
	}

	// TODO. Add indicator of waiting for input.

	private async Task OnInlineSplit(string[] Parametres)
	{
		await ToSignal(this, SignalName.InputRegistered);

		TextLabel.Text = "";
		TextLabel.VisibleCharacters = 0;
	}

	private async Task OnInlineHold(string[] Parametres)
	{
		await ToSignal(this, SignalName.InputRegistered);
	}

	private async Task OnInlinePause(string[] Parametres)
	{
		float Duration = FetchParametreValue<float>("Pause", Parametres, 0, out bool Success);
		if (Success) { await ToSignal(GetTree().CreateTimer(Duration), SceneTreeTimer.SignalName.Timeout); }
	}

	private async Task OnInlineSpeed(string[] Parametres)
	{
		float NewSpeedMod = FetchParametreValue<float>("Speed", Parametres, 0, out bool Success);
		if (Success) { TextSpeedMod = NewSpeedMod; }
	}

	private T FetchParametreValue<T>(string Name, string[] Parametres, int Position, out bool Success)
	{
		if (Parametres.Length <= Position)
		{
			Router.Debug.Print($"ERROR: In-line command parametre missing: {Name}, position {Position}.");
			Success = false;
			return default;
		}

		try
		{
			T Value = default;
			Value = (T)Convert.ChangeType(Parametres[Position].Split("=")[1], typeof(T));
			Success = true;
			return Value;
		}
		catch (Exception)
		{
			Router.Debug.Print($"ERROR: In-line command parametre not in correct format: {Name}, position {Position}.");
			Success = false;
			return default;
		}
	}
}
