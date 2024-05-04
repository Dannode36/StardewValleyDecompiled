using System;
using Microsoft.Xna.Framework.Audio;

namespace StardewValley;

public interface ICue : IDisposable
{
	bool IsStopped { get; }

	bool IsStopping { get; }

	bool IsPlaying { get; }

	bool IsPaused { get; }

	string Name { get; }

	float Pitch { get; set; }

	float Volume { get; set; }

	bool IsPitchBeingControlledByRPC { get; }

	void Play();

	void Pause();

	void Resume();

	void Stop(AudioStopOptions options);

	void SetVariable(string var, int val);

	void SetVariable(string var, float val);

	float GetVariable(string var);
}
