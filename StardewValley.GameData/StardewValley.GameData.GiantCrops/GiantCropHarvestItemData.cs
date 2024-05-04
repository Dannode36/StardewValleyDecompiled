using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.GiantCrops;

/// <summary>As part of <see cref="T:StardewValley.GameData.GiantCrops.GiantCropData" />, a possible item to produce when it's harvested.</summary>
public class GiantCropHarvestItemData : GenericSpawnItemDataWithCondition
{
	/// <summary>The probability that the item will be produced, as a value between 0 (never) and 1 (always).</summary>
	[ContentSerializer(Optional = true)]
	public float Chance { get; set; } = 1f;


	/// <summary>Whether to drop this item only for the Shaving enchantment (true), only when the giant crop is broken (false), or both (null).</summary>
	[ContentSerializer(Optional = true)]
	public bool? ForShavingEnchantment { get; set; }

	/// <summary>If set, the minimum stack size when this item is dropped due to the Shaving enchantment, scaled to the tool's power level.</summary>
	/// <remarks>
	///   <para>This value is multiplied by the health deducted by the tool hit which triggered the enchantment. For example, an iridium tool that reduced the giant crop's health by 3 points will produce three times this value per hit.</para>
	///
	///   <para>If the scaled min and max are both set, the stack size is randomized between them. If only one is set, it's applied as a limit after the generic fields. If neither is set, the generic fields are applied as usual without scaling.</para>
	/// </remarks>
	[ContentSerializer(Optional = true)]
	public int? ScaledMinStackWhenShaving { get; set; } = 2;


	/// <summary>If set, the maximum stack size when this item is dropped due to the Shaving enchantment, scaled to the tool's power level.</summary>
	/// <inheritdoc cref="P:StardewValley.GameData.GiantCrops.GiantCropHarvestItemData.ScaledMinStackWhenShaving" path="/remarks" />
	[ContentSerializer(Optional = true)]
	public int? ScaledMaxStackWhenShaving { get; set; } = 2;

}
