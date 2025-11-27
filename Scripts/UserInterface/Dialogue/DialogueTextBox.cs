using Godot;
using System;

public partial class DialogueTextBox : NinePatchRect, IConfigReliant
{
	[Export] private RichTextLabel TextLabel;

	private bool TextActive = false;
	private float TextSpeed;
	private const float MinTextSpeed = 0.02f;
	private const float MaxTextSpeed = 1.0f;

	private float TextSpeedMod = 1;

	private bool ProgressingTextButton = false;
	private const float TextSpeedProgress = 0.05f;

	[Signal] public delegate void TextFinishedEventHandler();
	private bool SignalEmitted = false;

	public override void _Ready()
	{
		OnConfigUpdate();
	}

	private double TimeSince = 0;
	public override void _Process(double Delta)
	{
		if (!TextActive) { return; }

		TimeSince += Delta;
		if (TimeSince > TextSpeed * TextSpeedMod * (ProgressingTextButton ? TextSpeedProgress : 1))
		{
			TextLabel.VisibleCharacters++;
			TimeSince = 0;
		}

		if (TextLabel.VisibleRatio >= 1) { TextActive = false; }
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
		if (TextActive)
		{
			ProgressingTextButton = true;
		}
		else if (!SignalEmitted)
		{
			EmitSignal(SignalName.TextFinished);
			SignalEmitted = true;
		}
	}
	
	public void OnStopProgressDialogue()
	{
		ProgressingTextButton = false;
	}

	public void SetText(string NewText)
	{
		TimeSince = 0;
		TextSpeedMod = 1;
		SignalEmitted = false;

		TextLabel.Text = NewText;
		TextLabel.VisibleCharacters = 0;
		TextActive = true;
	}
}
