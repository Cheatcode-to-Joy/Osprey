using Godot;
using System;
using System.Collections.Generic;

public partial class FileCarouselOverlay : UILayer
{
	public override void _Ready()
	{
		InstantiateBlocks([]);
	}

	#region UI Layer
	public override void GrabDefaultFocus()
	{
		FileBlocks[0].FileSlabs[0].GrabDefaultFocus();
	}
	#endregion

	[Export] private Control BlockHolder;
	[Export] private PackedScene BlockScene;
	private const int BlockNumber = 4;
	private FileBlock[] FileBlocks = new FileBlock[BlockNumber];

	private void InstantiateBlocks(Dictionary<int, FileData> Data)
	{
		int CurrentSave = 0;
		for (int BlockIndex = 0; BlockIndex < BlockNumber; BlockIndex++)
		{
			FileBlocks[BlockIndex] = BlockScene.Instantiate<FileBlock>();
			foreach (FileSlab Slab in FileBlocks[BlockIndex].FileSlabs)
			{
				if (Data.TryGetValue(CurrentSave, out FileData DataPoint)) { Slab.InsertSaveData(DataPoint); }
				CurrentSave++;
				Slab.SetSaveNumber(CurrentSave);
			}
		}

		SwitchToBlock(0);
	}

	private int CurrentBlock = 0;
	private void SwitchToBlock(int NewBlock)
	{
		foreach (Node Block in BlockHolder.GetChildren()) { BlockHolder.RemoveChild(Block); }

		CurrentBlock = Math.Clamp(NewBlock, 0, BlockNumber - 1);
		BlockHolder.AddChild(FileBlocks[CurrentBlock]);

		CheckButtons();
	}

	[Export] private Button ButtonBack;
	[Export] private Button ButtonForward;
	private void CheckButtons()
	{
		ButtonBack.Disabled = CurrentBlock == 0;
		ButtonForward.Disabled = CurrentBlock == BlockNumber - 1;
	}

	public void OnButtonPressed(int Offset)
	{
		SwitchToBlock(CurrentBlock + Offset);
	}
}
