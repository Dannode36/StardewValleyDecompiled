using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Objects;

/// <summary>The data for an object-type item.</summary>
public class ObjectData
{
	/// <summary>The internal item name.</summary>
	public string Name;

	/// <summary>A tokenizable string for the item's translated display name.</summary>
	public string DisplayName;

	/// <summary>A tokenizable string for the item's translated description.</summary>
	public string Description;

	/// <summary>The item's general type, like <c>Arch</c> (artifact) or <c>Minerals</c>.</summary>
	public string Type;

	/// <summary>The item category, usually matching a constant like <c>Object.flowersCategory</c>.</summary>
	public int Category;

	/// <summary>The price when sold by the player. This is not the price when bought from a shop.</summary>
	[ContentSerializer(Optional = true)]
	public int Price;

	/// <summary>The asset name for the texture containing the item's sprite, or <c>null</c> for <c>Maps/springobjects</c>.</summary>
	[ContentSerializer(Optional = true)]
	public string Texture;

	/// <summary>The sprite's index in the spritesheet.</summary>
	public int SpriteIndex;

	/// <summary>A numeric value that determines how much energy (edibility × 2.5) and health (edibility × 1.125) is restored when this item is eaten. An item with an edibility of -300 can't be eaten, values from -299 to -1 reduce health and energy, and zero can be eaten but doesn't change health/energy.</summary>
	/// <remarks>This is ignored for rings.</remarks>
	[ContentSerializer(Optional = true)]
	public int Edibility = -300;

	/// <summary>Whether to drink the item instead of eating it.</summary>
	/// <remarks>Ignored if the item isn't edible per <see cref="F:StardewValley.GameData.Objects.ObjectData.Edibility" />.</remarks>
	[ContentSerializer(Optional = true)]
	public bool IsDrink;

	/// <summary>The buffs to apply to the player when this item is eaten, if any.</summary>
	/// <remarks>Ignored if the item isn't edible per <see cref="F:StardewValley.GameData.Objects.ObjectData.Edibility" />.</remarks>
	[ContentSerializer(Optional = true)]
	public List<ObjectBuffData> Buffs;

	/// <summary>If set, the item will drop a default item when broken as a geode. If <see cref="F:StardewValley.GameData.Objects.ObjectData.GeodeDrops" /> is set too, there's a 50% chance of choosing a value from that list instead.</summary>
	[ContentSerializer(Optional = true)]
	public bool GeodeDropsDefaultItems;

	/// <summary>The items that can be dropped when this item is broken open as a geode.</summary>
	[ContentSerializer(Optional = true)]
	public List<ObjectGeodeDropData> GeodeDrops;

	/// <summary>If this is an artifact (i.e. <see cref="F:StardewValley.GameData.Objects.ObjectData.Type" /> is <c>Arch</c>), the chance that it can be found by digging artifact spots in each location.</summary>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, float> ArtifactSpotChances;

	/// <summary>Whether this item can be given to NPCs as a gift by default.</summary>
	/// <remarks>This doesn't override non-gift behavior (e.g. receiving quest items) or specific exclusions (e.g. only Pierre will accept Pierre's Missing Stocklist).</remarks>
	[ContentSerializer(Optional = true)]
	public bool CanBeGivenAsGift = true;

	/// <summary>Whether this item can be trashed by players by default.</summary>
	/// <remarks>This doesn't override specific exclusions (e.g. quest items can't be trashed).</remarks>
	[ContentSerializer(Optional = true)]
	public bool CanBeTrashed = true;

	/// <summary>Whether to exclude this item from the fishing collection and perfection score.</summary>
	[ContentSerializer(Optional = true)]
	public bool ExcludeFromFishingCollection;

	/// <summary>Whether to exclude this item from the shipping collection and perfection score.</summary>
	[ContentSerializer(Optional = true)]
	public bool ExcludeFromShippingCollection;

	/// <summary>Whether to exclude this item from shops when selecting random items to sell.</summary>
	[ContentSerializer(Optional = true)]
	public bool ExcludeFromRandomSale;

	/// <summary>The custom context tags to add for this item (in addition to the tags added automatically based on the other object data).</summary>
	[ContentSerializer(Optional = true)]
	public List<string> ContextTags;

	/// <summary>Custom fields ignored by the base game, for use by mods.</summary>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> CustomFields;
}
