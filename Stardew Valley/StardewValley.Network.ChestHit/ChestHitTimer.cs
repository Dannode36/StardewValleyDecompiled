using Microsoft.Xna.Framework;

namespace StardewValley.Network.ChestHit;

public sealed class ChestHitTimer
{
	/// <summary>The amount of milliseconds remaining until a chest must be hit twice to move it.</summary>
	public int Milliseconds;

	/// <summary>The time when this timer was saved in <see cref="F:StardewValley.Network.ChestHit.ChestHitSynchronizer.SavedTimers" />.</summary>
	public int SavedTime = -1;

	/// <summary>Ticks down the timer.</summary>
	/// <param name="gameTime">Provides a snapshot of timing values.</param>
	public void Update(GameTime time)
	{
		if (Milliseconds > 0)
		{
			Milliseconds -= (int)time.ElapsedGameTime.TotalMilliseconds;
		}
	}
}
