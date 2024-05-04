using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using StardewValley.Monsters;
using StardewValley.Projectiles;
using StardewValley.TokenizableStrings;

namespace StardewValley.Objects;

public class IceOrbTrinketEffect : TrinketEffect
{
	public const float RANGE = 600f;

	private float projectileTimer;

	private float projectileDelayMS = 4000f;

	private int freezeTime = 4000;

	public IceOrbTrinketEffect(Trinket trinket)
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
		projectileDelayMS = r.Next(3000, 5001);
		freezeTime = r.Next(2000, 4001);
		if (r.NextDouble() < 0.05)
		{
			trinket.displayNameOverrideTemplate.Value = TokenStringBuilder.LocalizedText("Strings\\1_6_Strings:PerfectIceRod");
			projectileDelayMS = 3000f;
			freezeTime = 4000;
		}
		trinket.descriptionSubstitutionTemplates.Add(Math.Round(projectileDelayMS / 1000f, 1).ToString(CultureInfo.InvariantCulture));
		trinket.descriptionSubstitutionTemplates.Add(Math.Round((float)freezeTime / 1000f, 1).ToString(CultureInfo.InvariantCulture));
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
			Monster closest_monster = Utility.findClosestMonsterWithinRange(location, farmer.getStandingPosition(), 600);
			if (closest_monster != null)
			{
				Vector2 motion = Utility.getVelocityTowardPoint(farmer.getStandingPosition(), closest_monster.getStandingPosition(), 5f);
				Math.Atan2(motion.Y, motion.X);
				DebuffingProjectile p = new DebuffingProjectile("frozen", 17, 0, 0, 0f, motion.X, motion.Y, farmer.getStandingPosition() - new Vector2(32f, 48f), location, farmer, hitsMonsters: true, playDefaultSoundOnFire: false);
				p.wavyMotion.Value = false;
				p.piercesLeft.Value = 99999;
				p.maxTravelDistance.Value = 3000;
				p.IgnoreLocationCollision = true;
				p.ignoreObjectCollisions.Value = true;
				p.maxVelocity.Value = 12f;
				p.projectileID.Value = 15;
				p.alpha.Value = 0.001f;
				p.alphaChange.Value = 0.05f;
				p.light.Value = true;
				p.debuffIntensity.Value = freezeTime;
				p.boundingBoxWidth.Value = 32;
				location.projectiles.Add(p);
				location.playSound("fireball");
			}
			projectileTimer = 0f;
		}
		base.Update(farmer, time, location);
	}
}
