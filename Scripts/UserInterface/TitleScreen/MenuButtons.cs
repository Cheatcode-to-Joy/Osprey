using Godot;
using System;

public partial class MenuButtons : VBoxContainer
{
	[Signal] public delegate void ButtonPressedEventHandler(int ButtonNumber);
}
