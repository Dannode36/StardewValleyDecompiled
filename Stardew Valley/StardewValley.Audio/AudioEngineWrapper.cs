using System;
using Microsoft.Xna.Framework.Audio;

namespace StardewValley.Audio;

internal class AudioEngineWrapper : IAudioEngine, IDisposable
{
	private AudioEngine audioEngine;

	public AudioEngine Engine => audioEngine;

	public bool IsDisposed => audioEngine.IsDisposed;

	public AudioEngineWrapper(AudioEngine engine)
	{
		audioEngine = engine;
	}

	public void Dispose()
	{
		audioEngine.Dispose();
	}

	public IAudioCategory GetCategory(string name)
	{
		return new AudioCategoryWrapper(audioEngine.GetCategory(name));
	}

	public int GetCategoryIndex(string name)
	{
		return audioEngine.GetCategoryIndex(name);
	}

	public void Update()
	{
		audioEngine.Update();
	}
}
