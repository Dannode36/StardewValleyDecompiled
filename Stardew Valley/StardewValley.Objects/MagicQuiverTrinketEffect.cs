using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using StardewValley.Monsters;
using StardewValley.Projectiles;
using StardewValley.TokenizableStrings;

namespace StardewValley.Objects;

public class MagicQuiverTrinketEffect : TrinketEffect
{
	public const float RANGE = 500f;

	private float projectileTimer;

	private float projectileDelayMS = 1000f;

	private int mindamage = 10;

	private int maxdamage = 10;

	public MagicQuiverTrinketEffect(Trinket trinket)
		: base(trinket)
	{
	}

	public override void Apply(Farmer farmer)
	{
		projectileTimer = 0f;
		base.Apply(farmer);
	}

	public override void GenerateRandomStats(Trinket trinket)
	{
		Random r = Utility.CreateRandom((int)trinket.generationSeed);
		if (r.NextDouble() < 0.04)
		{
			trinket.displayNameOverrideTemplate.Value = TokenStringBuilder.LocalizedText("Strings\\1_6_Strings:PerfectMagicQuiver");
			mindamage = 30;
			maxdamage = 35;
			projectileDelayMS = 900f;
		}
		else if (r.NextDouble() < 0.1)
		{
			if (r.NextDouble() < 0.5)
			{
				trinket.displayNameOverrideTemplate.Value = TokenStringBuilder.LocalizedText("Strings\\1_6_Strings:RapidMagicQuiver");
				mindamage = r.Next(10, 15);
				mindamage -= 2;
				maxdamage = mindamage + 5;
				projectileDelayMS = 600 + r.Next(11) * 10;
			}
			else
			{
				trinket.displayNameOverrideTemplate.Value = TokenStringBuilder.LocalizedText("Strings\\1_6_Strings:HeavyMagicQuiver");
				mindamage = r.Next(25, 41);
				mindamage -= 2;
				maxdamage = mindamage + 5;
				projectileDelayMS = 1500 + r.Next(6) * 100;
			}
		}
		else
		{
			mindamage = r.Next(15, 31);
			mindamage -= 2;
			maxdamage = mindamage + 5;
			projectileDelayMS = 1100 + r.Next(11) * 100;
		}
		trinket.descriptionSubstitutionTemplates.Add(Math.Round((double)projectileDelayMS / 1000.0, 2).ToString(CultureInfo.InvariantCulture));
		trinket.descriptionSubstitutionTemplates.Add(mindamage.ToString());
		trinket.descriptionSubstitutionTemplates.Add(maxdamage.ToString());
	}

	public override void Update(Farmer farmer, GameTime time, GameLocation location)
	{
		if (!Game1.shouldTimePass())
		{
			return;
		}
		projectileTimer += (float)time.ElapsedGameTime.TotalMilliseconds;
		if (projectileTimer >= projectileDelayMS)
		{
			projectileTimer = 0f;
			if (location is SlimeHutch)
			{
				return;
			}
			Monster closest_monster = Utility.findClosestMonsterWithinRange(location, farmer.getStandingPosition(), 500, ignoreUntargetables: true);
			if (closest_monster != null && !closest_monster.Name.Equals("Truffle Crab"))
			{
				Vector2 motion = Utility.getVelocityTowardPoint(farmer.getStandingPosition(), closest_monster.getStandingPosition(), 2f);
				float projectile_rotation = (float)Math.Atan2(motion.Y, motion.X) + (float)Math.PI / 2f;
				BasicProjectile p = new BasicProjectile(Game1.random.Next(mindamage, maxdamage + 1), 16, 0, 0, 0f, motion.X, motion.Y, farmer.getStandingPosition() - new Vector2(32f, 48f), null, null, null, explode: false, damagesMonsters: true, location, farmer);
				p.IgnoreLocationCollision = true;
				p.ignoreObjectCollisions.Value = true;
				p.acceleration.Value = motion;
				p.maxVelocity.Value = 24f;
				p.projectileID.Value = 14;
				p.startingRotation.Value = projectile_rotation;
				p.alpha.Value = 0.001f;
				p.alphaChange.Value = 0.05f;
				p.light.Value = true;
				p.collisionSound.Value = "magic_arrow_hit";
				location.projectiles.Add(p);
				location.playSound("magic_arrow");
			}
		}
		base.Update(farmer, time, location);
	}
}
