using Godot;
using System;

public partial class CoreHUD : Control
{
	#region Dialogue
	[ExportGroup("Dialogue")]
	[Export] private PackedScene DialogueScene;
	private DialogueOverlay CurrentDialogue = null;
	private DialogueOverlay QueuedDialogue = null;
	public void SpawnDialogue(string DialogueName)
	{
		QueuedDialogue?.QueueFree();
		QueuedDialogue = DialogueScene.Instantiate<DialogueOverlay>();
		QueuedDialogue.Connect(DialogueOverlay.SignalName.DialogueFinished, new Callable(this, MethodName.OnDialogueEnded));
		QueuedDialogue.LoadDialogue(DialogueName);

		if (CurrentDialogue != null) { CurrentDialogue?.EndDialogue(); }
		else { PlayQueuedDialogue(); }
	}

	private void PlayQueuedDialogue()
	{
		if (QueuedDialogue == null) { return; }

		CurrentDialogue = QueuedDialogue;
		QueuedDialogue = null;
		AddChild(CurrentDialogue);
		CurrentDialogue.Play();
	}

	public void OnDialogueEnded()
	{
		CurrentDialogue = null;
		PlayQueuedDialogue();
	}
	#endregion
}
