using System;
using Microsoft.Xna.Framework.Audio;

namespace StardewValley.Audio;

internal class DummyAudioEngine : IAudioEngine, IDisposable
{
	private IAudioCategory category = new DummyAudioCategory();

	public AudioEngine Engine { get; }

	public bool IsDisposed { get; } = true;


	public void Update()
	{
	}

	public IAudioCategory GetCategory(string name)
	{
		return category;
	}

	public int GetCategoryIndex(string name)
	{
		return -1;
	}

	public void Dispose()
	{
	}
}
