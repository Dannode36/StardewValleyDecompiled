using System;
using StardewValley.Monsters;

namespace StardewValley.Enchantments;

public class SlimeGathererEnchantment : BaseWeaponEnchantment
{
	public override bool IsSecondaryEnchantment()
	{
		return true;
	}

	public override bool IsForge()
	{
		return false;
	}

	protected override void _OnMonsterSlay(Monster m, GameLocation location, Farmer who)
	{
		base._OnMonsterSlay(m, location, who);
		if (m is GreenSlime || m is BigSlime)
		{
			int toDrop = 1 + Game1.random.Next((int)Math.Ceiling(Math.Sqrt(m.MaxHealth) / 3.0));
			Game1.createMultipleItemDebris(ItemRegistry.Create("(O)766", toDrop), m.getStandingPosition(), -1);
		}
	}

	public override int GetMaximumLevel()
	{
		return 5;
	}

	public override string GetName()
	{
		return Game1.content.LoadString("Strings\\1_6_Strings:SlimeGathererEnchantment");
	}
}
