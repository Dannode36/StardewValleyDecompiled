using StardewValley.Tools;

namespace StardewValley.Enchantments;

public class ReachingToolEnchantment : BaseEnchantment
{
	public override string GetName()
	{
		return "Expansive";
	}

	public override bool CanApplyTo(Item item)
	{
		if (item is Tool tool && (tool is WateringCan || tool is Hoe || tool is Pan))
		{
			return tool.UpgradeLevel == 4;
		}
		return false;
	}
}
