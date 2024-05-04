using System;
using Microsoft.Xna.Framework;
using StardewValley.Companions;
using StardewValley.Monsters;

namespace StardewValley.Objects;

public class FairyBoxTrinketEffect : TrinketEffect
{
	private float healTimer;

	private float healDelay = 4000f;

	private float power = 0.25f;

	private int damageTakenOrReceivedSinceLastHeal;

	public FairyBoxTrinketEffect(Trinket trinket)
		: base(trinket)
	{
	}

	public override void GenerateRandomStats(Trinket trinket)
	{
		Random r = Utility.CreateRandom(trinket.generationSeed.Value);
		int level = 1;
		if (r.NextDouble() < 0.45)
		{
			level = 2;
		}
		else if (r.NextDouble() < 0.25)
		{
			level = 3;
		}
		else if (r.NextDouble() < 0.125)
		{
			level = 4;
		}
		else if (r.NextDouble() < 0.0675)
		{
			level = 5;
		}
		healDelay = 5000 - level * 300;
		power = 0.7f + (float)level * 0.1f;
		trinket.descriptionSubstitutionTemplates.Add(level.ToString() ?? "");
	}

	public override void OnDamageMonster(Farmer farmer, Monster monster, int damageAmount)
	{
		damageTakenOrReceivedSinceLastHeal += damageAmount;
		base.OnDamageMonster(farmer, monster, damageAmount);
	}

	public override void OnReceiveDamage(Farmer farmer, int damageAmount)
	{
		damageTakenOrReceivedSinceLastHeal += damageAmount;
		base.OnReceiveDamage(farmer, damageAmount);
	}

	public override void Update(Farmer farmer, GameTime time, GameLocation location)
	{
		healTimer += (float)time.ElapsedGameTime.TotalMilliseconds;
		if (healTimer >= healDelay)
		{
			if (farmer.health < farmer.maxHealth && damageTakenOrReceivedSinceLastHeal >= 0)
			{
				int healAmount = (int)Math.Min(Math.Pow(damageTakenOrReceivedSinceLastHeal, 0.33000001311302185), (float)farmer.maxHealth / 10f);
				healAmount = (int)((float)healAmount * power);
				healAmount += Game1.random.Next((int)((float)(-healAmount) * 0.25f), (int)((float)healAmount * 0.25f) + 1);
				if (healAmount > 0)
				{
					farmer.health = Math.Min(farmer.maxHealth, farmer.health + healAmount);
					location.debris.Add(new Debris(healAmount, farmer.getStandingPosition(), Color.Lime, 1f, farmer));
					Game1.playSound("fairy_heal");
					damageTakenOrReceivedSinceLastHeal = 0;
				}
			}
			healTimer = 0f;
		}
		base.Update(farmer, time, location);
	}

	public override void Apply(Farmer farmer)
	{
		healTimer = 0f;
		damageTakenOrReceivedSinceLastHeal = 0;
		_companion = new FlyingCompanion(0);
		if (Game1.gameMode == 3)
		{
			farmer.AddCompanion(_companion);
		}
		base.Apply(farmer);
	}

	public override void Unapply(Farmer farmer)
	{
		farmer.RemoveCompanion(_companion);
	}
}
