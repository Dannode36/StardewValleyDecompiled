using StardewValley.Tools;

namespace StardewValley.Enchantments;

public class GenerousEnchantment : HoeEnchantment
{
	public override string GetName()
	{
		return "Generous";
	}

	public override bool CanApplyTo(Item item)
	{
		if (item is Tool)
		{
			if (!(item is Hoe))
			{
				return item is Pan;
			}
			return true;
		}
		return false;
	}
}
