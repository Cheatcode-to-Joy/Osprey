using Godot;
using System;
using System.Collections.Generic;

public partial class FileDataInput : VBoxContainer
{
	private string[] PermittedCharacters =
	{
		"ABCDEFGHIJKLMNOPQRSTUVWXYZ",
		"abcdefghijklmnopqrstuvwxyz"
	};

	private List<List<Label>> KeyButtons = [];

	public override void _Ready()
	{
		InitialiseKeyButtons();
	}

	public void SetFocus()
	{
		if (KeyButtons.Count > 0 && KeyButtons[0].Count > 0) { KeyButtons[0][0].CallDeferred(Control.MethodName.GrabFocus); }
	}

	#region Keys
	[Export] private PackedScene KeySectionScene;
	[Export] private PackedScene KeyButtonScene;
	private void InitialiseKeyButtons()
	{
		foreach (string SectionData in PermittedCharacters)
		{
			HFlowContainer KeySection = KeySectionScene.Instantiate<HFlowContainer>();
			AddChild(KeySection);

			List<Label> SectionButtons = [];

			foreach (char LetterData in SectionData)
			{
				Label KeyButton = KeyButtonScene.Instantiate<Label>();
				KeyButton.Text = LetterData.ToString();

				KeySection.AddChild(KeyButton);

				SectionButtons.Add(KeyButton);

				KeyButton.GuiInput += Event => OnKeyButtonInput(Event, LetterData);
				KeyButton.MouseEntered += () => OnKeyButtonMouseEnter(KeyButton);
				KeyButton.MouseExited += () => OnKeyButtonMouseExit(KeyButton);
			}

			KeyButtons.Add(SectionButtons);
		}
	}

	[Signal] public delegate void CharacterInputEventHandler(char InputCharacter);
	[Signal] public delegate void InputDeclinedEventHandler();
	public void OnKeyButtonInput(InputEvent Event, char InputCharacter)
	{
		if (Event.IsActionPressed("FocusInputYes"))
		{
			GetViewport().SetInputAsHandled();
			EmitSignal(SignalName.CharacterInput, InputCharacter);
			return;
		}

		if (Event.IsActionPressed("FocusInputReturn"))
		{
			GetViewport().SetInputAsHandled();
			EmitSignal(SignalName.InputDeclined);
			return;
		}
	}

	public static void OnKeyButtonMouseEnter(Label KeyButton)
	{
		// BUG: For some reason the text is brighter on hover as intended, but not on focus enter.
		KeyButton.ThemeTypeVariation = "KeyButtonActive";
	}

	public static void OnKeyButtonMouseExit(Label KeyButton)
	{
		KeyButton.ThemeTypeVariation = "KeyButtonInactive";
	}
	#endregion
}
