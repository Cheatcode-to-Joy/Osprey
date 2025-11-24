using Godot;
using System;

public partial class TitleButton : Button
{
	[Export] private Label SelectorL;
	[Export] private Label SelectorR;

	public void OnMouseEntered()
	{
		SelectorL.Text = ">";
		SelectorR.Text = "<";
	}

	public void OnMouseExited()
	{
		SelectorL.Text = " ";
		SelectorR.Text = " ";
	}
}
