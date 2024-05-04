using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Minecarts;

/// <summary>As part of <see cref="T:StardewValley.GameData.Minecarts.MinecartNetworkData" />, a minecart destination which can be used by players.</summary>
public class MinecartDestinationData
{
	/// <summary>A unique string ID for this destination within the network.</summary>
	public string Id;

	/// <summary>A tokenizable string for the destination name shown in the minecart menu. You can use the location's display name with the <c>LocationName</c> token (like <c>[LocationName Desert]</c> for the desert).</summary>
	public string DisplayName;

	/// <summary>A game state query which indicates whether this minecart destination is available. Defaults to always available.</summary>
	[ContentSerializer(Optional = true)]
	public string Condition;

	/// <summary>The gold price that must be paid to go to this destination, if any.</summary>
	[ContentSerializer(Optional = true)]
	public int Price;

	/// <summary>A localizable string for the message to show when purchasing a ticket, if applicable. Defaults to <see cref="F:StardewValley.GameData.Minecarts.MinecartNetworkData.BuyTicketMessage" />.</summary>
	[ContentSerializer(Optional = true)]
	public string BuyTicketMessage;

	/// <summary>The unique name for the location to warp to.</summary>
	public string TargetLocation;

	/// <summary>The destination tile position within the location.</summary>
	public Point TargetTile;

	/// <summary>The direction the player should face after arrival (one of <c>down</c>, <c>left</c>, <c>right</c>, or <c>up</c>).</summary>
	[ContentSerializer(Optional = true)]
	public string TargetDirection;

	/// <summary>Custom fields ignored by the base game, for use by mods.</summary>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> CustomFields;
}
