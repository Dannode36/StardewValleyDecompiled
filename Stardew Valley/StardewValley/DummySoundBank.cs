using System;
using Microsoft.Xna.Framework.Audio;

namespace StardewValley;

/// <summary>A sound bank implementation which does nothing, used when the game can't play audio.</summary>
public class DummySoundBank : ISoundBank, IDisposable
{
	/// <summary>An empty cue instance which does nothing.</summary>
	internal static readonly ICue DummyCue = new DummyCue();

	/// <inheritdoc />
	public bool IsInUse => false;

	/// <inheritdoc />
	public bool IsDisposed => true;

	/// <inheritdoc />
	public bool Exists(string name)
	{
		return true;
	}

	/// <inheritdoc />
	public ICue GetCue(string name)
	{
		return DummyCue;
	}

	/// <inheritdoc />
	public void PlayCue(string name)
	{
	}

	/// <inheritdoc />
	public void PlayCue(string name, AudioListener listener, AudioEmitter emitter)
	{
	}

	/// <inheritdoc />
	public void AddCue(CueDefinition definition)
	{
	}

	/// <inheritdoc />
	public CueDefinition GetCueDefinition(string name)
	{
		return null;
	}

	/// <summary>An empty cue instance which does nothing.</summary>
	public void Dispose()
	{
	}
}
