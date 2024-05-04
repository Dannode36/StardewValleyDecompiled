using StardewValley.Buffs;

namespace StardewValley.Enchantments;

public class CritEnchantment : BaseWeaponEnchantment
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
		effects.CriticalChanceMultiplier.Value += 0.02f * (float)(int)level;
	}

	public override int GetMaximumLevel()
	{
		return 3;
	}

	public override string GetName()
	{
		return Game1.content.LoadString("Strings\\1_6_Strings:CritEnchantment", base.Level);
	}
}
