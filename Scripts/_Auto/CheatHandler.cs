using System.Collections.Generic;
using Godot;

public partial class CheatHandler : Node
{
	public override void _Ready()
	{
		Router.Input.Connect(InputMethodHandler.SignalName.InputMethodChanged, new Callable(this, MethodName.OnInputMethodChanged));
		LoadCheats();
	}

	private Dictionary<string, string> KeyboardCheats = [];
	private Dictionary<string, string> JoypadCheats = [];

	private void LoadCheats()
	{
		Dictionary<string, Cheat> RawCheats = JSONReader.ReadJSONFile<Dictionary<string, Cheat>>("res://Assets/Text/Cheats.json", out bool _);
		foreach (string CheatName in RawCheats.Keys)
		{
			Cheat NewCheat = RawCheats[CheatName];
			if (!HasMethod($"Cheat{NewCheat.Function}"))
			{
				Router.Debug.Print($"WARNING: Cheat {CheatName} does not have a valid function: {NewCheat.Function}.");
				continue;
			}

			if (NewCheat.Inputs.TryGetValue("Keyboard", out string[] Keyboard))
			{
				foreach (string Entry in Keyboard) { KeyboardCheats[Entry] = NewCheat.Function; }
			}

			if (NewCheat.Inputs.TryGetValue("Joypad", out string[] Joypad))
			{
				foreach (string Entry in Joypad) { JoypadCheats[Entry] = NewCheat.Function; }
			}
		}
	}

	#region PARSING
    [Export] private Timer InputExpirationTimer;

	private List<string> CurrentKeys = [];

    public override void _UnhandledInput(InputEvent @Event)
    {
        if (!@Event.IsPressed() || @Event.IsEcho()) { return; }

		if (@Event is InputEventKey ThisKey)
		{
			CurrentKeys.Add(ThisKey.Keycode.ToString());
		}
		else if (@Event is InputEventJoypadButton ThisButton)
		{
			CurrentKeys.Add(ThisButton.ButtonIndex.ToString());
		}
		else { return; }

        if (CheckCheatcodes())
        {
            CurrentKeys = [];
            InputExpirationTimer.Stop();
        }
        else
        {
            InputExpirationTimer.Start();
        }
    }

	private bool CheckCheatcodes()
    {
		Dictionary<string, string> Cheats = Router.Input.InputMethod == InputMethodHandler.INPUT_METHODS.Keyboard ? KeyboardCheats : JoypadCheats;

        foreach (string Cheat in Cheats.Keys)
        {
            string[] Sequence = Cheat.Split("-");

            int CheckKeyFromEnd = 1;
            while (Sequence.Length >= CheckKeyFromEnd && CurrentKeys.Count >= CheckKeyFromEnd)
            {
                if (Sequence[^CheckKeyFromEnd] != CurrentKeys[^CheckKeyFromEnd]) { break; }
                
                if (Sequence.Length == CheckKeyFromEnd)
                {
                    ExecuteCheatcode(Cheats[Cheat]);
                    return true;
                }
                CheckKeyFromEnd++;
            }
        }

        return false;
    }

    private void ExecuteCheatcode(string Cheat)
    {
		if (!HasMethod($"Cheat{Cheat}"))
		{
			Router.Debug.Print($"ERROR: Cheat method {Cheat} not found.");
			return;
		}
        Callable NewCallable = new(this, $"Cheat{Cheat}");
        NewCallable.Call();
    }

    private void ResetKeys()
    {
        CurrentKeys = [];
    }

    public void OnInputExpirationTimerTimeout()
    {
        ResetKeys();
    }

	public void OnInputMethodChanged(int NewInputMethod)
	{
		ResetKeys();
	}
    #endregion

	#region Cheats
	public void CheatFunMode()
	{
		Router.Config.SetConfig("Debug", "FunEnabled", true);
		Router.Debug.Print("Fun mode enabled.");
	}
	#endregion
}

#region Cheat
public class Cheat
{
	public Dictionary<string, string[]> Inputs { get; set; } = [];

	public string Function { get; set; } = "";
}
#endregion