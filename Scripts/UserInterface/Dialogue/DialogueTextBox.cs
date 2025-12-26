using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public partial class DialogueTextBox : NinePatchRect, IConfigReliant
{
	[Export] private RichTextLabel TextLabel;

	[Export] private DialogueOverlay Overlay;

	private float TextSpeed;
	private const float MinTextSpeed = 0.02f;
	private const float MaxTextSpeed = 0.5f;

	private float TextSpeedMod = 1;

	private bool ProgressingTextButton = false;
	private const float TextSpeedProgress = 0.05f;

	[Signal] public delegate void CharacterTypedEventHandler();
	[Signal] public delegate void InputRegisteredEventHandler();
	[Signal] public delegate void TextFinishedEventHandler();

	public override void _Ready()
	{
		OnConfigUpdate();
	}

	public void OnConfigUpdate()
	{
		TextSpeed = Math.Clamp(Router.Config.FetchConfig<float>("Text", "TextSpeed"), MinTextSpeed, MaxTextSpeed);
	}
	
	#region Input
	public override void _Input(InputEvent @Event)
	{
		if (Input.IsActionJustPressed("ProgressDialogue")) { OnStartProgressDialogue(); }
		if (Input.IsActionJustReleased("ProgressDialogue")) { OnStopProgressDialogue(); }
	}

	public void OnStartProgressDialogue()
	{
		EmitSignal(SignalName.InputRegistered);

		ProgressingTextButton = true;

		if (!PrintingText)
		{
			EmitSignal(SignalName.TextFinished);
			return;
		}
	}
	
	public void OnStopProgressDialogue()
	{
		ProgressingTextButton = false;
	}
	#endregion

	#region Print
	string[] TextSections;

	public void SetText(string NewText)
	{
		TextLabel.Text = "";
		TextLabel.VisibleCharacters = 0;

		PreloadAudio(NewText);

		ScanText(NewText);
		PrintText();
	}

	private Dictionary<string, AudioStream> Audio = [];

	[GeneratedRegex("{sound name=(.+?)}")]
	private static partial Regex AudioRegex();

	private void PreloadAudio(string Text)
	{
		MatchCollection AudioNames = AudioRegex().Matches(Text);
		foreach (Match AudioName in AudioNames)
		{
			string ConvertedName = AudioName.Groups[1].Value.ToString();
			if (!Audio.ContainsKey(ConvertedName))
			{
				AudioStream Stream = SFXCreator.CreateStream(ConvertedName);
				if (Stream != null) { Audio[ConvertedName] = Stream; }
			}
		}
	}

	[GeneratedRegex("({.+?})")]
	private static partial Regex CommandFindRegex();

	private void ScanText(string Text)
	{
		TextSections = CommandFindRegex().Split(Text);
	}

	[Export] private Timer PrintTimer;
	private bool PrintingText = false;

	private async void PrintText()
	{
		PrintingText = true;

		foreach (string Entry in TextSections)
		{
			if (Entry.Length < 1) { continue; }

			if (Entry.StartsWith(char.Parse("{"))) { await ExecuteCommand(Entry); }
			else
			{
				TextLabel.AppendText(Entry);
				int TextLength = TextLabel.GetParsedText().Length;
				while (TextLabel.VisibleCharacters < TextLength)
				{
					TextLabel.VisibleCharacters++;
					if (TextLabel.GetParsedText()[TextLabel.VisibleCharacters - 1] != char.Parse(" ")) { EmitSignal(SignalName.CharacterTyped); }
					PrintTimer.Start(TextSpeed * TextSpeedMod * (ProgressingTextButton ? TextSpeedProgress : 1));
					await ToSignal(PrintTimer, Timer.SignalName.Timeout);
				}
			}
		}

		PrintingText = false;
	}
	#endregion

	#region Commands
	[GeneratedRegex("[{}]")]
	private static partial Regex CommandTrimRegex();

	private async Task ExecuteCommand(string RawCommand)
	{
		RawCommand = CommandTrimRegex().Replace(RawCommand, "");
		string[] Sections = RawCommand.Split(" ");

		string Command = Sections[0].Trim().ToLower();
		Dictionary<string, string> Arguments = Sections.Length > 1 ? ParseParametres([.. Sections.Skip(1)]) : [];

		Router.Debug.Print($"Executing in-line command {RawCommand}.");

		switch (Command)
		{
			case "split":
			await OnInlineSplit();
			break;
			case "swapsplit":
			await OnInlineSwapSplit();
			break;
			case "hold":
			await OnInlineHold();
			break;
			case "pause":
			await OnInlinePause(Arguments);
			break;
			case "end":
			OnInlineEnd();
			break;
			case "speed":
			OnInlineSpeed(Arguments);
			break;
			case "expression":
			OnInlineExpression(Arguments);
			break;
			case "shake":
			OnInlineShake(Arguments);
			break;
			case "sound":
			OnInlineSound(Arguments);
			break;
			default:
			Router.Debug.Print($"WARNING: Invalid in-line command {Command}.");
			return;
		}
	}

	// TODO. Add indicator of waiting for input.

	private async Task OnInlineSplit()
	{
		InstantiateProgressArrow();
		await ToSignal(this, SignalName.InputRegistered);
		RemoveProgressArrow();

		TextLabel.Text = "";
		TextLabel.VisibleCharacters = 0;
	}

	private async Task OnInlineSwapSplit()
	{
		await OnInlineSplit();
		Overlay.ChangeSpeaker();
	}

	private async Task OnInlineHold()
	{
		InstantiateProgressArrow();
		await ToSignal(this, SignalName.InputRegistered);
		RemoveProgressArrow();
	}

	private async Task OnInlinePause(Dictionary<string, string> Parametres)
	{
		float Duration = FetchParametre<float>("Pause", Parametres, "duration", out bool Success, false);
		await ToSignal(GetTree().CreateTimer(Success ? Duration : 1.0f), SceneTreeTimer.SignalName.Timeout);
	}

	private void OnInlineEnd()
	{
		EmitSignal(SignalName.TextFinished);
	}

	private void OnInlineSpeed(Dictionary<string, string> Parametres)
	{
		float NewSpeedMod = FetchParametre<float>("Speed", Parametres, "multiplier", out bool Success, false);
		TextSpeedMod = Success ? NewSpeedMod : 1.0f;
	}

	private void OnInlineExpression(Dictionary<string, string> Parametres)
	{
		string ExpressionName = FetchParametre<string>("Expression", Parametres, "name", out bool ExpressionOK, false);
		bool Speaker = FetchParametre<bool>("Expression", Parametres, "speaker", out bool SpeakerOK, false);

		DialogueSpeaker Participant = (SpeakerOK ? Speaker : true) ? Overlay.GetSpeaker() : Overlay.GetListener();
		Participant.ChangeExpression(ExpressionOK ? ExpressionName : "DEFAULT");
	}

	private static void OnInlineShake(Dictionary<string, string> Parametres)
	{
		Dictionary<string, float> CameraParametres = [];

		string[] ParametreNames = ["Strength", "Duration", "Fade"];
		foreach (string Parametre in ParametreNames)
		{
			float Value = FetchParametre<float>("Shake", Parametres, Parametre.ToLower(), out bool Success, false);
			if (Success) { CameraParametres[Parametre] = Value; }
		}

		Router.Main.Camera.ShakeCamera(CameraParametres);
	}

	private void OnInlineSound(Dictionary<string, string> Parametres)
	{
		string SFXName = FetchParametre<string>("Expression", Parametres, "name", out bool Success, true);
		if (!Success) { return; }

		if (Audio.TryGetValue(SFXName, out AudioStream Value))
		{
			Router.Audio.SFXMaker.CreateOmniAudioStream(Value);
		}
	}

	private static Dictionary<string, string> ParseParametres(string[] RawParametres)
	{
		Dictionary<string, string> Parametres = [];
		foreach (string RawParametre in RawParametres)
		{
			string[] ParametreParts = RawParametre.Trim().Split("=");
			if (ParametreParts.Length != 2) { continue; }
			Parametres[ParametreParts[0].Trim().ToLower()] = ParametreParts[1].Trim();
		}

		return Parametres;
	}

	private static T FetchParametre<T>(string Command, Dictionary<string, string> Parametres, string Key, out bool Success, bool Mandatory)
	{
		if (!Parametres.TryGetValue(Key, out string Parametre))
		{
			if (Mandatory) { Router.Debug.Print($"ERROR: In-line command parametre missing: {Command} : {Key}."); }
			Success = false;
			return default;
		}

		try
		{
			T Value = default;
			Value = (T)Convert.ChangeType(Parametre, typeof(T));
			Success = true;
			return Value;
		}
		catch (Exception)
		{
			Router.Debug.Print($"ERROR: In-line command parametre not in correct format: {Command} : {Key}.");
			Success = false;
			return default;
		}
	}
	#endregion

	#region Indicator
	[Export] private PackedScene DialogueIndicator;
	private DialogueProgressArrow CurrentIndicator;
	private void InstantiateProgressArrow()
	{
		/*
		CurrentIndicator = DialogueIndicator.Instantiate<DialogueProgressArrow>();
		CurrentIndicator.SetSprite(Overlay.GetSpeaker().CSpeaker.ID);
		AddChild(CurrentIndicator);

		// FIXME. Set position. There's a PR for GetCharacterBounds, so hopefully soon?
		*/
	}

	private void RemoveProgressArrow()
	{
		/*
		CurrentIndicator?.QueueFree();
		CurrentIndicator = null;
		*/
	}
	#endregion
}
