using StardewValley.Tools;

namespace StardewValley.Enchantments;

public class WateringCanEnchantment : BaseEnchantment
{
	public override bool CanApplyTo(Item item)
	{
		if (item is WateringCan)
		{
			return true;
		}
		return false;
	}
}
