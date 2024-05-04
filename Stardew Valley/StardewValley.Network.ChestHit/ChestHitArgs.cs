using Microsoft.Xna.Framework;

namespace StardewValley.Network.ChestHit;

/// <summary>Arguments to pass to <see cref="M:StardewValley.Objects.Chest.HandleChestHit(StardewValley.Network.ChestHit.ChestHitArgs)" />.</summary>
public sealed class ChestHitArgs
{
	/// <summary>The parent location of the chest being hit.</summary>
	public GameLocation Location;

	/// <summary>The tile location of the chest being hit.</summary>
	public Point ChestTile;

	/// <summary>The target position of the tool used to hit the chest.</summary>
	public Vector2 ToolPosition;

	/// <summary>The position of the player who hit the chest.</summary>
	public Point StandingPixel;

	/// <summary>The facing direction of the player who hit the chest.</summary>
	public int Direction;

	/// <summary>Whether the chest was hit using hold-down-click.</summary>
	public bool HoldDownClick;

	/// <summary>Whether the tool the player is using can move the chest.</summary>
	public bool ToolCanHit;

	/// <summary>Whether the player hit the chest recently enough to move the chest.</summary>
	public bool RecentlyHit;
}
