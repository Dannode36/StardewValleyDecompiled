using StardewValley.Buffs;

namespace StardewValley.Enchantments;

public class DefenseEnchantment : BaseWeaponEnchantment
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
		effects.Defense.Value += (int)level;
	}

	public override int GetMaximumLevel()
	{
		return 3;
	}

	public override string GetName()
	{
		return Game1.content.LoadString("Strings\\1_6_Strings:DefenseEnchantment", base.Level);
	}
}
