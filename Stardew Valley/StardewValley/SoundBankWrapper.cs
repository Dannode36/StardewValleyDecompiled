using System;
using Microsoft.Xna.Framework.Audio;

namespace StardewValley;

/// <summary>The default sound bank implementation which defers to MonoGame audio.</summary>
public class SoundBankWrapper : ISoundBank, IDisposable
{
	/// <summary>The audio cue name used when a non-existent audio cue is requested to avoid a game crash.</summary>
	private string DefaultCueName = "shiny4";

	/// <summary>The underlying MonoGame sound bank.</summary>
	private SoundBank soundBank;

	/// <inheritdoc />
	public bool IsInUse => soundBank.IsInUse;

	/// <inheritdoc />
	public bool IsDisposed => soundBank.IsDisposed;

	/// <summary>Construct an instance.</summary>
	/// <param name="soundBank">The underlying MonoGame sound bank.</param>
	public SoundBankWrapper(SoundBank soundBank)
	{
		this.soundBank = soundBank;
	}

	/// <inheritdoc />
	public ICue GetCue(string name)
	{
		if (!Exists(name))
		{
			Game1.log.Error("Can't get audio ID '" + name + "' because it doesn't exist.");
			name = DefaultCueName;
		}
		return new CueWrapper(soundBank.GetCue(name));
	}

	/// <inheritdoc />
	public void PlayCue(string name)
	{
		if (!Exists(name))
		{
			Game1.log.Error("Can't play audio ID '" + name + "' because it doesn't exist.");
			name = DefaultCueName;
		}
		soundBank.PlayCue(name);
	}

	/// <inheritdoc />
	public void PlayCue(string name, AudioListener listener, AudioEmitter emitter)
	{
		soundBank.PlayCue(name, listener, emitter);
	}

	/// <inheritdoc />
	public void Dispose()
	{
		soundBank.Dispose();
	}

	/// <inheritdoc />
	public void AddCue(CueDefinition definition)
	{
		soundBank.AddCue(definition);
	}

	/// <inheritdoc />
	public bool Exists(string name)
	{
		return soundBank.Exists(name);
	}

	/// <inheritdoc />
	public CueDefinition GetCueDefinition(string name)
	{
		return soundBank.GetCueDefinition(name);
	}
}
