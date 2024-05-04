using StardewValley.Tools;

namespace StardewValley.Enchantments;

public class EmeraldEnchantment : BaseWeaponEnchantment
{
	protected override void _ApplyTo(Item item)
	{
		base._ApplyTo(item);
		if (item is MeleeWeapon weapon)
		{
			weapon.speed.Value += 5 * GetLevel();
		}
	}

	protected override void _UnapplyTo(Item item)
	{
		base._UnapplyTo(item);
		if (item is MeleeWeapon weapon)
		{
			weapon.speed.Value -= 5 * GetLevel();
		}
	}

	public override bool ShouldBeDisplayed()
	{
		return false;
	}

	public override bool IsForge()
	{
		return true;
	}
}
