using StardewValley.Buffs;

namespace StardewValley.Enchantments;

public class LightweightEnchantment : BaseWeaponEnchantment
{
	public override bool IsSecondaryEnchantment()
	{
		return true;
	}

	public override bool IsForge()
	{
		return false;
	}

	public override void AddEquipmentEffects(BuffEffects effects)
	{
		base.AddEquipmentEffects(effects);
		effects.KnockbackMultiplier.Value -= (float)(int)level * 0.1f;
	}

	public override int GetMaximumLevel()
	{
		return 5;
	}

	public override string GetName()
	{
		return Game1.content.LoadString("Strings\\1_6_Strings:LightweightEnchantment", base.Level);
	}
}
