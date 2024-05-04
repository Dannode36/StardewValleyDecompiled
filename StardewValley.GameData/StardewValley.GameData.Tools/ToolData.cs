using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Tools;

/// <summary>The behavior and metadata for a tool that can be equipped by players.</summary>
public class ToolData
{
	/// <summary>The name for the C# class to construct within the <c>StardewValley.Tools</c> namespace. This must be a subclass of <c>StardewValley.Tool</c>.</summary>
	public string ClassName;

	/// <summary>The tool's internal name.</summary>
	public string Name;

	/// <summary>The number of attachment slots to set, or <c>-1</c> to keep the default value.</summary>
	[ContentSerializer(Optional = true)]
	public int AttachmentSlots = -1;

	/// <summary>The sale price for the tool in shops.</summary>
	[ContentSerializer(Optional = true)]
	public int SalePrice = -1;

	/// <summary>A tokenizable string for the tool's display name.</summary>
	public string DisplayName;

	/// <summary>A tokenizable string for the tool's description.</summary>
	public string Description;

	/// <summary>The asset name for the texture containing the tool's sprite.</summary>
	public string Texture;

	/// <summary>The index within the <see cref="F:StardewValley.GameData.Tools.ToolData.Texture" /> for the animation sprites, where 0 is the top icon.</summary>
	public int SpriteIndex;

	/// <summary>The index within the <see cref="F:StardewValley.GameData.Tools.ToolData.Texture" /> for the item icon, or <c>-1</c> to use the <see cref="F:StardewValley.GameData.Tools.ToolData.SpriteIndex" />.</summary>
	[ContentSerializer(Optional = true)]
	public int MenuSpriteIndex = -1;

	/// <summary>The tool's initial upgrade level, or <c>-1</c> to keep the default value.</summary>
	[ContentSerializer(Optional = true)]
	public int UpgradeLevel = -1;

	/// <summary>Whether to adjust the display name based on the conventional tool upgrade levels (e.g. upgrade level 1 would be "Copper {display name}").</summary>
	[ContentSerializer(Optional = true)]
	public bool ApplyUpgradeLevelToDisplayName;

	/// <summary>If set, the item ID for a tool which can be upgraded into this one using the default upgrade rules based on <see cref="F:StardewValley.GameData.Tools.ToolData.UpgradeLevel" />. This is prepended to <see cref="F:StardewValley.GameData.Tools.ToolData.UpgradeFrom" />.</summary>
	[ContentSerializer(Optional = true)]
	public string ConventionalUpgradeFrom;

	/// <summary>A list of items which the player can upgrade into this at Clint's shop.</summary>
	[ContentSerializer(Optional = true)]
	public List<ToolUpgradeData> UpgradeFrom;

	/// <summary>Whether the player can lose this tool when they die.</summary>
	[ContentSerializer(Optional = true)]
	public bool CanBeLostOnDeath;

	/// <summary>The class properties to set when creating the tool.</summary>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> SetProperties;

	/// <summary>The <c>modData</c> values to set when the tool is created.</summary>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> ModData;

	/// <summary>Custom fields ignored by the base game, for use by mods.</summary>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> CustomFields;
}
