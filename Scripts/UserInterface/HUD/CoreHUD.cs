using Godot;
using System;

public partial class CoreHUD : Control
{
	// FIXME. Delete.
	public override void _Input(InputEvent @Event)
	{
		if (@Event is InputEventKey EventKey && EventKey.Keycode == Key.A && @Event.IsPressed() && !EventKey.Echo) { SpawnDialogue("_DEFAULT_DIALOGUE"); }
	}

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
