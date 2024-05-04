using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Fences;

/// <summary>The metadata for a placeable fence item.</summary>
public class FenceData
{
	/// <summary>The initial health points for a fence when it's first placed, which affects how quickly it degrades. A fence loses 1/1440 points per in-game minute (roughly 0.04 points per hour or 0.5 points for a 12-hour day).</summary>
	public int Health;

	/// <summary>The minimum amount added to the health when a fence is repaired by a player.</summary>
	/// <remarks>Repairing a fence sets its health to <c>2 × (<see cref="F:StardewValley.GameData.Fences.FenceData.Health" /> + Random(<see cref="F:StardewValley.GameData.Fences.FenceData.RepairHealthAdjustmentMinimum" />, <see cref="F:StardewValley.GameData.Fences.FenceData.RepairHealthAdjustmentMaximum" />))</c>.</remarks>
	[ContentSerializer(Optional = true)]
	public float RepairHealthAdjustmentMinimum;

	/// <summary>The maximum amount added to the health when a fence is repaired by a player.</summary>
	/// <remarks>See remarks on <see cref="F:StardewValley.GameData.Fences.FenceData.RepairHealthAdjustmentMinimum" />.</remarks>
	[ContentSerializer(Optional = true)]
	public float RepairHealthAdjustmentMaximum;

	/// <summary>The asset name for the texture when the fence is placed. For example, the vanilla fences use individual tilesheets like <c>LooseSprites\Fence1</c> (wood fence).</summary>
	public string Texture;

	/// <summary>The audio cue ID played when the fence is placed or repairs (e.g. axe used by Wood Fence).</summary>
	public string PlacementSound;

	/// <summary>The audio cue ID played when the fence is broken or picked up by the player. Defaults to <see cref="F:StardewValley.GameData.Fences.FenceData.PlacementSound" />.</summary>
	[ContentSerializer(Optional = true)]
	public string RemovalSound;

	/// <summary>A list of tool IDs which can be used to break the fence, matching the keys in the <c>Data\Tools</c> asset.</summary>
	/// <remarks>A tool must match <see cref="F:StardewValley.GameData.Fences.FenceData.RemovalToolIds" /> <strong>or</strong> <see cref="F:StardewValley.GameData.Fences.FenceData.RemovalToolTypes" /> to be a valid removal tool. If both lists are null or empty, all tools can remove the fence.</remarks>
	[ContentSerializer(Optional = true)]
	public List<string> RemovalToolIds = new List<string>();

	/// <summary>A list of tool class full names which can be used to break the fence, like <c>StardewValley.Tools.Axe</c>.</summary>
	/// <inheritdoc cref="F:StardewValley.GameData.Fences.FenceData.RemovalToolIds" path="/remarks" />
	[ContentSerializer(Optional = true)]
	public List<string> RemovalToolTypes = new List<string>();

	/// <summary>The type of cosmetic debris particles to 'splash' from the tile when the fence is broken. The defined values are <c>0</c> (copper), <c>2</c> (iron), <c>4</c> (coal), <c>6</c> (gold), <c>8</c> (coins), <c>10</c> (iridium), <c>12</c> (wood), <c>14</c> (stone), <c>32</c> (big stone), and <c>34</c> (big wood). Default <c>14</c> (stone).</summary>
	[ContentSerializer(Optional = true)]
	public int RemovalDebrisType = 14;

	/// <summary>When an item like a torch is placed on the fence, the pixel offset to apply to its draw position.</summary>
	[ContentSerializer(Optional = true)]
	public Vector2 HeldObjectDrawOffset = new Vector2(0f, -20f);

	/// <summary>The X pixel offset to apply when the fence is oriented horizontally, with only one connected fence on the right. This fully replaces the X value specified by <see cref="F:StardewValley.GameData.Fences.FenceData.HeldObjectDrawOffset" /> when it's applied.</summary>
	[ContentSerializer(Optional = true)]
	public float LeftEndHeldObjectDrawX = -1f;

	/// <summary>Equivalent to <see cref="F:StardewValley.GameData.Fences.FenceData.LeftEndHeldObjectDrawX" />, but when there's only one connected fence on the left.</summary>
	[ContentSerializer(Optional = true)]
	public float RightEndHeldObjectDrawX;
}
