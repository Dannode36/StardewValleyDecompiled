using StardewValley.Tools;

namespace StardewValley.Enchantments;

public class ArchaeologistEnchantment : HoeEnchantment
{
	public override string GetName()
	{
		return "Archaeologist";
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
