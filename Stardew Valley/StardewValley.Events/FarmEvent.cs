using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;

namespace StardewValley.Events;

/// <summary>A cutscene that plays overnight before the day ends.</summary>
public interface FarmEvent : INetObject<NetFields>
{
	/// <summary>Set up the event before it runs.</summary>
	/// <returns>Returns true if the event should be skipped (e.g. setup failed), else false to continue.</returns>
	bool setUp();

	/// <summary>Update the event for a game tick while it's running.</summary>
	/// <param name="time">The elapsed game time.</param>
	/// <returns>Returns true if the event is done and can be ended, else false to continue.</returns>
	bool tickUpdate(GameTime time);

	/// <summary>Draw the event to the screen.</summary>
	/// <param name="b">The sprite batch being drawn.</param>
	void draw(SpriteBatch b);

	/// <summary>Draw anything the event needs to show above everything else.</summary>
	/// <param name="b">The sprite batch being drawn.</param>
	void drawAboveEverything(SpriteBatch b);

	/// <summary>Make any location changes needed when the event ends.</summary>
	void makeChangesToLocation();
}
