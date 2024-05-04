namespace StardewValley.GameData.Shops;

/// <summary>How a shop stock limit is applied in multiplayer.</summary>
public enum LimitedStockMode
{
	/// <summary>The limit applies to every player in the world. For example, if limited to one and a player bought it, no other players can buy one.</summary>
	Global,
	/// <summary>Each player has a separate limit. For example, if limited to one, each player could buy one.</summary>
	Player,
	/// <summary>Ignore the limit. This is used for items that adjust their own stock via code (e.g. by checking mail).</summary>
	None
}
