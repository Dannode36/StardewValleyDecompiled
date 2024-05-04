using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Tools;

/// <summary>As part of <see cref="T:StardewValley.GameData.Tools.ToolData" />, the requirements to upgrade items into a tool.</summary>
public class ToolUpgradeData
{
	/// <summary>A game state query which indicates whether this upgrade is available. Default always enabled.</summary>
	[ContentSerializer(Optional = true)]
	public string Condition;

	/// <summary>The gold price to upgrade the tool, or <c>-1</c> to use <see cref="F:StardewValley.GameData.Tools.ToolData.SalePrice" />.</summary>
	[ContentSerializer(Optional = true)]
	public int Price = -1;

	/// <summary>If set, the item ID for the tool that must be in the player's inventory for the upgrade to appear. The tool will be destroyed when the upgrade is accepted.</summary>
	[ContentSerializer(Optional = true)]
	public string RequireToolId;

	/// <summary>If set, the item ID for an extra item that must be traded to upgrade the tool (for example, copper bars for many copper tools).</summary>
	[ContentSerializer(Optional = true)]
	public string TradeItemId;

	/// <summary>The number of <see cref="F:StardewValley.GameData.Tools.ToolUpgradeData.TradeItemId" /> required.</summary>
	[ContentSerializer(Optional = true)]
	public int TradeItemAmount = 1;
}
