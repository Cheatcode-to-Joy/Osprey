using Godot;
using System;

public partial class DialogueTextBox : NinePatchRect, IConfigReliant
{
	[Export] private RichTextLabel TextLabel;

	private bool TextActive = false;
	private float TextSpeed;
	private const float MinTextSpeed = 0.1f;
	private const float MaxTextSpeed = 5.0f;

	public override void _Ready()
	{
		OnConfigUpdate();
	}

	public override void _Process(double Delta)
	{

	}
	
	public void OnConfigUpdate()
	{
		TextSpeed = Math.Clamp(Router.Config.FetchConfig<float>("Text", "TextSpeed"), MinTextSpeed, MaxTextSpeed);
	}

	public void SetText(string NewText)
	{
		TextLabel.Text = NewText;
		TextLabel.VisibleCharacters = 0;
		TextActive = true;
	}
}
