using StardewValley.Tools;

namespace StardewValley.Enchantments;

public class FisherEnchantment : BaseEnchantment
{
	public override string GetName()
	{
		return "Fisher";
	}

	public override bool CanApplyTo(Item item)
	{
		if (item is Tool)
		{
			return item is Pan;
		}
		return false;
	}
}
