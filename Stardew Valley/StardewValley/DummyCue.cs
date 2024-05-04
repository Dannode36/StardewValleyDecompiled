using System;
using Microsoft.Xna.Framework.Audio;

namespace StardewValley;

public class DummyCue : ICue, IDisposable
{
	public bool IsStopped => true;

	public bool IsStopping => false;

	public bool IsPlaying => false;

	public bool IsPaused => false;

	public string Name => "";

	public float Volume
	{
		get
		{
			return 1f;
		}
		set
		{
		}
	}

	public float Pitch
	{
		get
		{
			return 0f;
		}
		set
		{
		}
	}

	public bool IsPitchBeingControlledByRPC => true;

	public void Play()
	{
	}

	public void Pause()
	{
	}

	public void Resume()
	{
	}

	public void SetVariable(string var, int val)
	{
	}

	public void SetVariable(string var, float val)
	{
	}

	public float GetVariable(string var)
	{
		return 0f;
	}

	public void Stop(AudioStopOptions options)
	{
	}

	public void Dispose()
	{
	}
}
