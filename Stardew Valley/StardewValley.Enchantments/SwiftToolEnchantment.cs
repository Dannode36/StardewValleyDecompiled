using StardewValley.Tools;

namespace StardewValley.Enchantments;

public class SwiftToolEnchantment : BaseEnchantment
{
	public override string GetName()
	{
		return "Swift";
	}

	public override bool CanApplyTo(Item item)
	{
		if (item is Tool && !(item is MilkPail) && !(item is MeleeWeapon) && !(item is Shears) && !(item is FishingRod) && !(item is Pan) && !(item is WateringCan) && !(item is Wand))
		{
			return !(item is Slingshot);
		}
		return false;
	}

	protected override void _ApplyTo(Item item)
	{
		base._ApplyTo(item);
		if (item is Tool tool)
		{
			tool.AnimationSpeedModifier = 0.66f;
		}
	}

	protected override void _UnapplyTo(Item item)
	{
		base._UnapplyTo(item);
		if (item is Tool tool)
		{
			tool.AnimationSpeedModifier = 1f;
		}
	}
}
