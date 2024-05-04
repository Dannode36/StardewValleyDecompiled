using StardewValley.Tools;

namespace StardewValley.Enchantments;

public class ShearsEnchantment : BaseEnchantment
{
	public override bool CanApplyTo(Item item)
	{
		if (item is Shears)
		{
			return true;
		}
		return false;
	}
}
