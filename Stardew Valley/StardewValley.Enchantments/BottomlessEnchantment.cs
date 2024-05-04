using StardewValley.Tools;

namespace StardewValley.Enchantments;

public class BottomlessEnchantment : WateringCanEnchantment
{
	public override string GetName()
	{
		return "Bottomless";
	}

	protected override void _ApplyTo(Item item)
	{
		base._ApplyTo(item);
		if (item is WateringCan tool)
		{
			tool.IsBottomless = true;
			tool.WaterLeft = tool.waterCanMax;
		}
	}

	protected override void _UnapplyTo(Item item)
	{
		base._UnapplyTo(item);
		if (item is WateringCan tool)
		{
			tool.IsBottomless = false;
		}
	}
}
