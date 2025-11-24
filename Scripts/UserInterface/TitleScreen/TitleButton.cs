using Godot;
using System;

public partial class TitleButton : Button
{
	[Export] private Label SelectorL;
	[Export] private Label SelectorR;

	public void OnMouseEntered()
	{
		SelectorL.Show();
		SelectorR.Show();
	}

	public void OnMouseExited()
	{
		SelectorL.Hide();
		SelectorR.Hide();
	}
}
