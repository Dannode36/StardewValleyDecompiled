using StardewValley.Buffs;

namespace StardewValley.Enchantments;

public class AttackEnchantment : BaseWeaponEnchantment
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
		effects.Attack.Value += (int)level;
	}

	public override int GetMaximumLevel()
	{
		return 5;
	}

	public override string GetName()
	{
		return Game1.content.LoadString("Strings\\1_6_Strings:AttackEnchantment", base.Level);
	}
}
