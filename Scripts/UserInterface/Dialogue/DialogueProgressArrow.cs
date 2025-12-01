using Godot;
using System;

public partial class DialogueProgressArrow : Control
{
	[Export] private TextureRect ArrowSprite;
	[Export] private AtlasTexture Atlas;
	private const float ArrowAnimationTime = 0.5f;
	private const int PixelSize = 8;

	public override void _Ready()
	{
		Tween ArrowAnimator = CreateTween();
		ArrowAnimator.SetLoops();
		ArrowAnimator.TweenProperty(ArrowSprite, "position:x", 1, ArrowAnimationTime);
		ArrowAnimator.TweenProperty(ArrowSprite, "position:x", 0, ArrowAnimationTime);
	}

	public void SetSprite(int Index)
	{
		Vector2 SpriteSize = Atlas.Atlas.GetSize();
		int IndexInPixels = Math.Max(Index, 0) * PixelSize;
		Vector2 StartPosition = new(IndexInPixels % SpriteSize.X, (float)Math.Floor(IndexInPixels / SpriteSize.X));

		if (StartPosition.X > SpriteSize.X - PixelSize || StartPosition.Y > SpriteSize.Y - PixelSize)
		{
			Router.Debug.Print("ERROR: Dialogue arrow index out of bounds.");
			return;
		}

		Atlas.Region = new Rect2(StartPosition, new Vector2(PixelSize, PixelSize));
	}
}
