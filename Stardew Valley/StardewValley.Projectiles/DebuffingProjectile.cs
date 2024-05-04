using System;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;

namespace StardewValley.Projectiles;

public class DebuffingProjectile : Projectile
{
	/// <summary>The buff ID to apply to players hit by this projectile.</summary>
	public readonly NetString debuff = new NetString();

	public NetBool wavyMotion = new NetBool(value: true);

	public NetInt debuffIntensity = new NetInt(-1);

	private float periodicEffectTimer;

	/// <summary>Construct an empty instance.</summary>
	public DebuffingProjectile()
	{
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="debuff">The debuff ID to apply to players hit by this projectile.</param>
	/// <param name="spriteIndex">The index of the sprite to draw in <see cref="F:StardewValley.Projectiles.Projectile.projectileSheetName" />.</param>
	/// <param name="bouncesTillDestruct">The number of times the projectile can bounce off walls before being destroyed.</param>
	/// <param name="tailLength">The length of the tail which trails behind the main projectile.</param>
	/// <param name="rotationVelocity">The rotation velocity.</param>
	/// <param name="xVelocity">The speed at which the projectile moves along the X axis.</param>
	/// <param name="yVelocity">The speed at which the projectile moves along the Y axis.</param>
	/// <param name="startingPosition">The pixel world position at which the projectile will start moving.</param>
	/// <param name="location">The location containing the projectile.</param>
	/// <param name="owner">The character who fired the projectile.</param>
	public DebuffingProjectile(string debuff, int spriteIndex, int bouncesTillDestruct, int tailLength, float rotationVelocity, float xVelocity, float yVelocity, Vector2 startingPosition, GameLocation location = null, Character owner = null, bool hitsMonsters = false, bool playDefaultSoundOnFire = true)
		: this()
	{
		theOneWhoFiredMe.Set(location, owner);
		this.debuff.Value = debuff;
		currentTileSheetIndex.Value = spriteIndex;
		bouncesLeft.Value = bouncesTillDestruct;
		base.tailLength.Value = tailLength;
		base.rotationVelocity.Value = rotationVelocity;
		base.xVelocity.Value = xVelocity;
		base.yVelocity.Value = yVelocity;
		position.Value = startingPosition;
		damagesMonsters.Value = hitsMonsters;
		if (playDefaultSoundOnFire)
		{
			if (location == null)
			{
				Game1.playSound("debuffSpell");
			}
			else
			{
				location.playSound("debuffSpell");
			}
		}
	}

	/// <inheritdoc />
	protected override void InitNetFields()
	{
		base.InitNetFields();
		base.NetFields.AddField(debuff, "debuff").AddField(wavyMotion, "wavyMotion").AddField(debuffIntensity, "debuffIntensity");
	}

	public override void updatePosition(GameTime time)
	{
		xVelocity.Value += acceleration.X;
		yVelocity.Value += acceleration.Y;
		position.X += xVelocity.Value;
		position.Y += yVelocity.Value;
		if ((bool)wavyMotion)
		{
			position.X += (float)Math.Sin((double)time.TotalGameTime.Milliseconds * Math.PI / 128.0) * 8f;
			position.Y += (float)Math.Cos((double)time.TotalGameTime.Milliseconds * Math.PI / 128.0) * 8f;
		}
	}

	public override bool update(GameTime time, GameLocation location)
	{
		if (debuff == "frozen")
		{
			periodicEffectTimer += (float)time.ElapsedGameTime.TotalMilliseconds;
			if (periodicEffectTimer > 50f)
			{
				periodicEffectTimer = 0f;
				location.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\Projectiles", new Rectangle(32, 32, 16, 16), 9999f, 1, 1, position.Value, flicker: false, flipped: false, 1f, 0.01f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = Utility.getRandom360degreeVector(1f) + new Vector2(xVelocity.Value, yVelocity.Value),
					drawAboveAlwaysFront = true
				});
			}
		}
		return base.update(time, location);
	}

	public override void behaviorOnCollisionWithPlayer(GameLocation location, Farmer player)
	{
		if (!damagesMonsters && Game1.random.Next(11) >= player.Immunity && !player.hasBuff("28") && !player.hasTrinketWithID("BasiliskPaw"))
		{
			piercesLeft.Value--;
			if (Game1.player == player)
			{
				player.applyBuff(debuff);
			}
			explosionAnimation(location);
			if (debuff == "19")
			{
				location.playSound("frozen");
			}
			else
			{
				location.playSound("debuffHit");
			}
		}
	}

	public override void behaviorOnCollisionWithTerrainFeature(TerrainFeature t, Vector2 tileLocation, GameLocation location)
	{
		explosionAnimation(location);
		piercesLeft.Value--;
	}

	public override void behaviorOnCollisionWithOther(GameLocation location)
	{
		explosionAnimation(location);
		piercesLeft.Value--;
	}

	protected virtual void explosionAnimation(GameLocation location)
	{
		if (!(debuff == "frozen"))
		{
			Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(352, Game1.random.Next(100, 150), 2, 1, position.Value, flicker: false, flipped: false));
		}
	}

	public override void behaviorOnCollisionWithMonster(NPC n, GameLocation location)
	{
		if ((bool)damagesMonsters && n is Monster && debuff == "frozen" && (!(n is Leaper leaper) || !leaper.leaping.Value))
		{
			if ((int)(n as Monster).stunTime < 51)
			{
				piercesLeft.Value--;
			}
			if ((int)(n as Monster).stunTime < debuffIntensity.Value - 1000)
			{
				location.playSound("frozen");
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(118, 227, 16, 13), new Vector2(0f, 0f), flipped: false, 0f, Color.White)
				{
					layerDepth = (float)(n.StandingPixel.Y + 2) / 10000f,
					animationLength = 1,
					interval = debuffIntensity.Value,
					scale = 4f,
					id = (int)(n.position.X * 777f + n.position.Y * 77777f),
					positionFollowsAttachedCharacter = true,
					attachedCharacter = n
				});
			}
			(n as Monster).stunTime.Value = debuffIntensity.Value;
		}
	}
}
