using System;
using Microsoft.Xna.Framework;
using StardewValley.Companions;
using StardewValley.Monsters;
using StardewValley.TokenizableStrings;

namespace StardewValley.Objects;

public class TrinketEffect
{
	protected Trinket _trinket;

	public int general_stat_1;

	protected Companion _companion;

	public TrinketEffect(Trinket trinket)
	{
		_trinket = trinket;
	}

	public virtual void OnUse(Farmer farmer)
	{
	}

	public virtual void Apply(Farmer farmer)
	{
		if (_trinket.ItemId == "ParrotEgg")
		{
			_companion = new FlyingCompanion(1);
			if (Game1.gameMode == 3)
			{
				farmer.AddCompanion(_companion);
			}
		}
	}

	public virtual void Unapply(Farmer farmer)
	{
		farmer.RemoveCompanion(_companion);
	}

	public virtual void OnFootstep(Farmer farmer)
	{
	}

	public virtual void OnReceiveDamage(Farmer farmer, int damageAmount)
	{
	}

	public virtual void OnDamageMonster(Farmer farmer, Monster monster, int damageAmount)
	{
		if (_trinket.ItemId == "ParrotEgg" && monster != null && monster.Health <= 0)
		{
			double chance = (double)(general_stat_1 + 1) * 0.1;
			while (Game1.random.NextDouble() <= chance)
			{
				monster.objectsToDrop.Add("GoldCoin");
			}
		}
	}

	public virtual void GenerateRandomStats(Trinket trinket)
	{
		Random r = Utility.CreateRandom((int)trinket.generationSeed);
		if (trinket.ItemId == "IridiumSpur")
		{
			general_stat_1 = r.Next(5, 11);
			trinket.descriptionSubstitutionTemplates.Add(general_stat_1.ToString());
		}
		else if (trinket.ItemId == "ParrotEgg")
		{
			int maxLevel = Math.Min(4, (int)(1 + Game1.player.totalMoneyEarned / 750000));
			general_stat_1 = r.Next(0, maxLevel);
			trinket.descriptionSubstitutionTemplates.Add((general_stat_1 + 1).ToString());
			trinket.descriptionSubstitutionTemplates.Add(TokenStringBuilder.LocalizedText("Strings\\1_6_Strings:ParrotEgg_Chance_" + general_stat_1));
		}
	}

	public virtual void Update(Farmer farmer, GameTime time, GameLocation location)
	{
	}
}
