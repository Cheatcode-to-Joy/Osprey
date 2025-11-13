using Godot;

public interface ISoundEffect
{
	void PlaySound(AudioStream Sound);
	void OnFinished();
}
