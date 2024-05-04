using StardewValley.Tools;

namespace StardewValley.Enchantments;

public class HoeEnchantment : BaseEnchantment
{
	public override bool CanApplyTo(Item item)
	{
		if (item is Hoe)
		{
			return true;
		}
		return false;
	}
}
