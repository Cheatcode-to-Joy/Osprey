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
				KeyButton.FocusEntered += () => OnKeyButtonFocusEnter(KeyButton);
				KeyButton.FocusExited += () => OnKeyButtonFocusExit(KeyButton);
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

	private bool Enabled = true;

	public void OnEnable()
	{
		if (Enabled) { return; }
		Enabled = true;

		if (HoveredKey != null) { HoveredKey.ThemeTypeVariation = "KeyButtonActive"; }
		Modulate = new(1, 1, 1, 1);
		FocusBehaviorRecursive = FocusBehaviorRecursiveEnum.Inherited;
	}

	public void OnDisable()
	{
		if (!Enabled) { return; }
		Enabled = false;

		if (HoveredKey != null) { HoveredKey.ThemeTypeVariation = "KeyButtonInactive"; }
		Modulate = new(1, 1, 1, 0.5f);
		FocusBehaviorRecursive = FocusBehaviorRecursiveEnum.Disabled;
	}

	private Label HoveredKey = null;

	private void MakeButtonActive(Label KeyButton)
	{
		if (Enabled) { KeyButton.ThemeTypeVariation = "KeyButtonActive"; }
	}

	private void MakeButtonInactive(Label KeyButton)
	{
		if (HoveredKey != null && HoveredKey == KeyButton) { return; }
		if (Enabled) { KeyButton.ThemeTypeVariation = "KeyButtonInactive"; }
	}

	public void OnKeyButtonMouseEnter(Label KeyButton)
	{
		HoveredKey = KeyButton;
		MakeButtonActive(KeyButton);
	}

	public void OnKeyButtonMouseExit(Label KeyButton)
	{
		if (HoveredKey == KeyButton) { HoveredKey = null; }
		MakeButtonInactive(KeyButton);
	}

	public void OnKeyButtonFocusEnter(Label KeyButton)
	{
		MakeButtonActive(KeyButton);
	}

	public void OnKeyButtonFocusExit(Label KeyButton)
	{
		MakeButtonInactive(KeyButton);
	}
	#endregion
}
