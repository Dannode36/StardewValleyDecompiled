using StardewValley.Tools;

namespace StardewValley.Enchantments;

public class PickaxeEnchantment : BaseEnchantment
{
	public override bool CanApplyTo(Item item)
	{
		if (item is Pickaxe)
		{
			return true;
		}
		return false;
	}
}
