using Godot;
using System;

public partial class FileSlab : PanelContainer, IConfigReliant
{
	public override void _Ready()
	{
		Router.Config.Connect(ConfigManager.SignalName.ConfigChanged, new Callable(this, MethodName.OnConfigUpdate));
		SetButtonText();
	}

	public void OnConfigUpdate()
	{
		SetButtonText();
	}

	private bool HasSave = false;
	private int SaveIndex = 0;

	[Export] private Label SaveNumber;
	public void SetSaveNumber(int Number)
	{
		SaveIndex = Number - 1;
		SaveNumber.Text = Number.ToString("00");
	}

	public void InsertSaveData(FileData Data)
	{
		HasSave = true;
		// TODO.
	}

	[Export] private Button MenuButton;
	public void GrabDefaultFocus()
	{
		MenuButton.CallDeferred(Control.MethodName.GrabFocus);
	}

	private void SetButtonText()
	{
		MenuButton.Text = Tr(HasSave ? "LOAD" : "CREATE");
	}

	[Signal] public delegate void SaveLaunchRequestedEventHandler(int SaveNumber);
	[Signal] public delegate void SaveCreationRequestedEventHandler(int SaveNumber);
	public void OnButtonPressed()
	{
		EmitSignal(HasSave ? SignalName.SaveLaunchRequested : SignalName.SaveCreationRequested, SaveIndex);
	}
}
