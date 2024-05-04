using StardewValley.Tools;

namespace StardewValley.Enchantments;

public class BaseWeaponEnchantment : BaseEnchantment
{
	public override bool CanApplyTo(Item item)
	{
		if (item is MeleeWeapon weapon)
		{
			return !weapon.isScythe();
		}
		return false;
	}

	public void OnSwing(MeleeWeapon weapon, Farmer farmer)
	{
		_OnSwing(weapon, farmer);
	}

	protected virtual void _OnSwing(MeleeWeapon weapon, Farmer farmer)
	{
	}
}
