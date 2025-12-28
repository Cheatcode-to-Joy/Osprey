using Godot;
using System;

public partial class CoreHUD : Control
{
	// FIXME. Delete. Yeah all of the stuff above dialogue.
	[Export] private PackedScene CreateFileScene;
	public override void _Ready()
	{
		// CallDeferred(MethodName.OpenCreateFile);
	}
	public void OpenCreateFile()
	{
		Router.Main.AddOverlay(CreateFileScene.Instantiate<UILayer>());
	}

	#region Dialogue
	[ExportGroup("Dialogue")]
	[Export] private PackedScene DialogueScene;
	private OverlayDialogue CurrentDialogue = null;
	private OverlayDialogue QueuedDialogue = null;
	public void SpawnDialogue(string DialogueName)
	{
		QueuedDialogue?.QueueFree();
		QueuedDialogue = DialogueScene.Instantiate<OverlayDialogue>();
		QueuedDialogue.Connect(OverlayDialogue.SignalName.DialogueFinished, new Callable(this, MethodName.OnDialogueEnded));
		QueuedDialogue.LoadDialogue(DialogueName);

		if (CurrentDialogue != null) { CurrentDialogue?.EndDialogue(); }
		else { PlayQueuedDialogue(); }
	}

	private void PlayQueuedDialogue()
	{
		if (QueuedDialogue == null) { return; }

		CurrentDialogue = QueuedDialogue;
		QueuedDialogue = null;
		Router.Main.AddOverlay(CurrentDialogue);
		CurrentDialogue.Play();
	}

	public void OnDialogueEnded()
	{
		Router.Main.CloseOverlay(CurrentDialogue);
		CurrentDialogue = null;
		CallDeferred(MethodName.PlayQueuedDialogue);
	}
	#endregion
}
