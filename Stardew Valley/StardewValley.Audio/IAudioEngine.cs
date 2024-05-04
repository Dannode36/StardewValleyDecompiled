using System;
using Microsoft.Xna.Framework.Audio;

namespace StardewValley.Audio;

public interface IAudioEngine : IDisposable
{
	bool IsDisposed { get; }

	AudioEngine Engine { get; }

	void Update();

	IAudioCategory GetCategory(string name);

	int GetCategoryIndex(string name);
}
