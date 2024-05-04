using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.WildTrees;

/// <summary>Metadata for a non-fruit tree type.</summary>
public class WildTreeData
{
	/// <summary>The tree textures to show in game. The first matching texture will be used.</summary>
	public List<WildTreeTextureData> Textures;

	/// <summary>The qualified or unqualified item ID for the seed item.</summary>
	public string SeedItemId;

	/// <summary>Whether the seed can be planted by the player. If false, it can only be spawned automatically via map properties.</summary>
	[ContentSerializer(Optional = true)]
	public bool SeedPlantable = true;

	/// <summary>The percentage chance each day that the tree will grow to the next stage without tree fertilizer, as a value from 0 (will never grow) to 1 (will grow every day).</summary>
	[ContentSerializer(Optional = true)]
	public float GrowthChance = 0.2f;

	/// <summary>Overrides <see cref="F:StardewValley.GameData.WildTrees.WildTreeData.GrowthChance" /> when tree fertilizer is applied.</summary>
	[ContentSerializer(Optional = true)]
	public float FertilizedGrowthChance = 1f;

	/// <summary>The percentage chance each day that the tree will plant a seed on a nearby tile, as a value from 0 (never) to 1 (always). This only applied in locations where trees drop seeds (e.g. farms in vanilla).</summary>
	[ContentSerializer(Optional = true)]
	public float SeedSpreadChance = 0.15f;

	/// <summary>The percentage chance each day that the tree will produce a seed that will drop when the tree is shaken, as a value from 0 (never) to 1 (always).</summary>
	[ContentSerializer(Optional = true)]
	public float SeedOnShakeChance = 0.05f;

	/// <summary>The percentage chance that a seed will drop when the player chops down the tree, as a value from 0 (never) to 1 (always).</summary>
	[ContentSerializer(Optional = true)]
	public float SeedOnChopChance = 0.75f;

	/// <summary>Whether to drop wood when the player chops down the tree.</summary>
	[ContentSerializer(Optional = true)]
	public bool DropWoodOnChop = true;

	/// <summary>Whether to drop hardwood when the player chops down the tree, if they have the Lumberjack profession.</summary>
	[ContentSerializer(Optional = true)]
	public bool DropHardwoodOnLumberChop = true;

	/// <summary>Whether shaking or chopping the tree causes cosmetic leaves to drop from tree and produces a leaf rustle sound. When a leaf drops, the game will use one of the four leaf sprites in the tree's spritesheet in the slot left of the stump sprite.</summary>
	[ContentSerializer(Optional = true)]
	public bool IsLeafy = true;

	/// <summary>Whether <see cref="F:StardewValley.GameData.WildTrees.WildTreeData.IsLeafy" /> also applies in winter.</summary>
	[ContentSerializer(Optional = true)]
	public bool IsLeafyInWinter;

	/// <summary>Whether <see cref="F:StardewValley.GameData.WildTrees.WildTreeData.IsLeafy" /> also applies in fall.</summary>
	[ContentSerializer(Optional = true)]
	public bool IsLeafyInFall = true;

	/// <summary>The rules which override which locations the tree can be planted in, if applicable. These don't override more specific checks (e.g. not being plantable on stone).</summary>
	[ContentSerializer(Optional = true)]
	public List<PlantableRule> PlantableLocationRules;

	/// <summary>Whether the tree can grow in winter (subject to <see cref="F:StardewValley.GameData.WildTrees.WildTreeData.GrowthChance" /> or <see cref="F:StardewValley.GameData.WildTrees.WildTreeData.FertilizedGrowthChance" />).</summary>
	[ContentSerializer(Optional = true)]
	public bool GrowsInWinter;

	/// <summary>Whether the tree is reduced to a stump in winter and regrows in spring, like the vanilla mushroom tree.</summary>
	[ContentSerializer(Optional = true)]
	public bool IsStumpDuringWinter;

	/// <summary>Whether woodpeckers can spawn on the tree.</summary>
	[ContentSerializer(Optional = true)]
	public bool AllowWoodpeckers = true;

	/// <summary>Whether to render a different tree sprite when the tree hasn't been shaken that day.</summary>
	/// <inheritdoc cref="F:StardewValley.GameData.WildTrees.WildTreeData.UseAlternateSpriteWhenSeedReady" path="/remarks" />
	[ContentSerializer(Optional = true)]
	public bool UseAlternateSpriteWhenNotShaken;

	/// <summary>Whether to render a different tree sprite when it has a seed ready. If true, the tree spritesheet should be double-width with the alternate textures on the right.</summary>
	/// <remarks>If <see cref="F:StardewValley.GameData.WildTrees.WildTreeData.UseAlternateSpriteWhenNotShaken" /> or <see cref="F:StardewValley.GameData.WildTrees.WildTreeData.UseAlternateSpriteWhenSeedReady" /> is true, the tree spritesheet should be double-width with the alternate textures on the right. If both are true, the same alternate texture is used for both.</remarks>
	[ContentSerializer(Optional = true)]
	public bool UseAlternateSpriteWhenSeedReady;

	/// <summary>
	///   The color of the cosmetic wood chips when chopping the tree. This can be...
	///   <list type="bullet">
	///     <item><description>a MonoGame color field name (like <c>ForestGreen</c>);</description></item>
	///     <item><description>an RGB hex code (like <c>#AABBCC</c>) or RGBA hex code (like <c>#AABBCCDD</c>);</description></item>
	///     <item><description>or a debris type code: <c>12</c> (brown/woody), <c>10000</c> (white), <c>100001</c> (light green), <c>100002</c> (light blue), <c>100003</c> (red), <c>100004</c> (yellow), <c>100005</c> (black), <c>100006</c> (gray), <c>100007</c> (charcoal / dim gray).</description></item>
	///    </list>
	///    Defaults to brown/woody.
	/// </summary>
	[ContentSerializer(Optional = true)]
	public string DebrisColor;

	/// <summary>When a seed is dropped subject to <see cref="F:StardewValley.GameData.WildTrees.WildTreeData.SeedOnShakeChance" />, the item to drop instead of <see cref="F:StardewValley.GameData.WildTrees.WildTreeData.SeedItemId" />. If this is empty or none match, the <see cref="F:StardewValley.GameData.WildTrees.WildTreeData.SeedItemId" /> will be dropped instead.</summary>
	[ContentSerializer(Optional = true)]
	public List<WildTreeSeedDropItemData> SeedDropItems;

	/// <summary>The additional items to drop when the tree is chopped.</summary>
	[ContentSerializer(Optional = true)]
	public List<WildTreeChopItemData> ChopItems;

	/// <summary>The items produced by tapping the tree when it's fully grown. If multiple items can be produced, the first available one is selected.</summary>
	[ContentSerializer(Optional = true)]
	public List<WildTreeTapItemData> TapItems;

	/// <summary>The items produced by shaking the tree when it's fully grown.</summary>
	[ContentSerializer(Optional = true)]
	public List<WildTreeItemData> ShakeItems;

	/// <summary>Custom fields ignored by the base game, for use by mods.</summary>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> CustomFields;

	/// <summary>Whether this tree grows moss or not</summary>
	[ContentSerializer(Optional = true)]
	public bool GrowsMoss;

	/// <summary>Get whether trees of this type can be tapped in any season.</summary>
	public bool CanBeTapped()
	{
		List<WildTreeTapItemData> tapItems = TapItems;
		if (tapItems == null)
		{
			return false;
		}
		return tapItems.Count > 0;
	}
}
