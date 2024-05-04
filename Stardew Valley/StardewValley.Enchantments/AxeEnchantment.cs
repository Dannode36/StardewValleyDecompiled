using StardewValley.Tools;

namespace StardewValley.Enchantments;

public class AxeEnchantment : BaseEnchantment
{
	public override bool CanApplyTo(Item item)
	{
		if (item is Axe)
		{
			return true;
		}
		return false;
	}
}
