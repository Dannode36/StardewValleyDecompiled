using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Minecarts;

/// <summary>The data for a network of minecarts, which are enabled together.</summary>
public class MinecartNetworkData
{
	/// <summary>A game state query which indicates whether this minecart network is unlocked.</summary>
	[ContentSerializer(Optional = true)]
	public string UnlockCondition;

	/// <summary>A localizable string for the message to show if the network is locked.</summary>
	[ContentSerializer(Optional = true)]
	public string LockedMessage;

	/// <summary>A localizable string for the message to show when selecting a destination.</summary>
	[ContentSerializer(Optional = true)]
	public string ChooseDestinationMessage;

	/// <summary>A localizable string for the message to show when purchasing a ticket, if applicable.</summary>
	[ContentSerializer(Optional = true)]
	public string BuyTicketMessage;

	/// <summary>The destinations which the player can travel to from any minecart in this network.</summary>
	public List<MinecartDestinationData> Destinations;
}
