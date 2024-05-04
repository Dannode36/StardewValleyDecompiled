using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Projectiles;

namespace StardewValley.Companions;

public class HungryFrogCompanion : HoppingCompanion
{
	private const int RANGE = 300;

	private const int FULLNESS_TIME = 12000;

	public float fullnessTime;

	private float monsterEatCheckTimer;

	private float tongueOutTimer;

	private readonly NetBool tongueOut = new NetBool(value: false);

	private readonly NetBool tongueReturn = new NetBool(value: false);

	private readonly NetPosition tonguePosition = new NetPosition();

	private readonly NetVector2 tongueVelocity = new NetVector2();

	private readonly NetNPCRef attachedMonsterField = new NetNPCRef();

	private readonly NetEvent0 fullnessTrigger = new NetEvent0();

	private float initialEquipDelay = 12000f;

	private float lastHopTimer;

	private Monster attachedMonster
	{
		get
		{
			if (base.Owner != null)
			{
				return attachedMonsterField.Get(base.Owner.currentLocation) as Monster;
			}
			return null;
		}
		set
		{
			attachedMonsterField.Set(base.Owner.currentLocation, value);
		}
	}

	public HungryFrogCompanion()
	{
	}

	public HungryFrogCompanion(int variant)
	{
		whichVariant.Value = variant;
	}

	public override void InitNetFields()
	{
		base.InitNetFields();
		base.NetFields.AddField(tongueOut, "tongueOut").AddField(tongueReturn, "tongueReturn").AddField(tonguePosition.NetFields, "tonguePosition.NetFields")
			.AddField(tongueVelocity, "tongueVelocity")
			.AddField(attachedMonsterField.NetFields, "attachedMonsterField.NetFields")
			.AddField(fullnessTrigger, "fullnessTrigger");
		fullnessTrigger.onEvent += triggerFullnessTimer;
	}

	public override void Update(GameTime time, GameLocation location)
	{
		if (!tongueOut.Value)
		{
			base.Update(time, location);
		}
		if (!Game1.shouldTimePass())
		{
			return;
		}
		if (fullnessTime > 0f)
		{
			fullnessTime -= (float)time.ElapsedGameTime.TotalMilliseconds;
		}
		lastHopTimer += (float)time.ElapsedGameTime.TotalMilliseconds;
		if (initialEquipDelay > 0f)
		{
			initialEquipDelay -= (float)time.ElapsedGameTime.TotalMilliseconds;
			return;
		}
		if (base.IsLocal)
		{
			monsterEatCheckTimer += (float)time.ElapsedGameTime.TotalMilliseconds;
			if (monsterEatCheckTimer >= 2000f && fullnessTime <= 0f && !tongueOut.Value)
			{
				monsterEatCheckTimer = 0f;
				if (!(location is SlimeHutch))
				{
					Monster closest_monster = Utility.findClosestMonsterWithinRange(location, base.Position, 300);
					if (closest_monster != null)
					{
						if (closest_monster is Bat && closest_monster.Age == 789)
						{
							monsterEatCheckTimer = 0f;
							return;
						}
						if (closest_monster.Name.Equals("Truffle Crab"))
						{
							monsterEatCheckTimer = 0f;
							return;
						}
						if (closest_monster is GreenSlime slime && slime.prismatic.Value)
						{
							monsterEatCheckTimer = 0f;
							return;
						}
						height = 0f;
						Vector2 motion = Utility.getVelocityTowardPoint(base.Position, closest_monster.getStandingPosition(), 12f);
						tongueOut.Value = true;
						tongueReturn.Value = false;
						tonguePosition.Value = base.Position + new Vector2(-32f, -32f) + new Vector2((direction.Value != 3) ? 28 : 0, -20f);
						tongueVelocity.Value = motion;
						location.playSound("croak");
						direction.Value = ((!(closest_monster.Position.X < base.Position.X)) ? 1 : 3);
					}
				}
				tongueOutTimer = 0f;
			}
			if (tongueOut.Value)
			{
				tongueOutTimer += (float)time.ElapsedGameTime.TotalMilliseconds * (float)((!tongueReturn) ? 1 : (-1));
				tonguePosition.Value += tongueVelocity.Value;
				if (attachedMonster == null)
				{
					if (Vector2.Distance(base.Position, tonguePosition.Value) >= 300f)
					{
						tongueReachedMonster(null);
					}
					else
					{
						int damageSize = 40;
						Rectangle boundingBox = new Rectangle((int)tonguePosition.X + 32 - damageSize / 2, (int)tonguePosition.Y + 32 - damageSize / 2, damageSize, damageSize);
						if (base.Owner.currentLocation.doesPositionCollideWithCharacter(boundingBox) is Monster monster)
						{
							tongueReachedMonster(monster);
						}
					}
				}
				if (attachedMonster != null)
				{
					attachedMonster.Position = tonguePosition.Value;
					attachedMonster.xVelocity = 0f;
					attachedMonster.yVelocity = 0f;
				}
				if (tongueReturn.Value)
				{
					Vector2 homingVector = Vector2.Subtract(base.Position + new Vector2(-32f, -32f) + new Vector2((direction.Value != 3) ? 28 : 0, -20f), tonguePosition.Value);
					homingVector.Normalize();
					homingVector *= 12f;
					tongueVelocity.Value = homingVector;
				}
				if ((tongueReturn.Value && Vector2.Distance(base.Position, tonguePosition.Value) <= 48f) || tongueOutTimer <= 0f)
				{
					if (attachedMonster != null)
					{
						if (attachedMonster is HotHead hothead && hothead.timeUntilExplode.Value > 0f)
						{
							hothead.currentLocation?.netAudio.StopPlaying("fuse");
						}
						if (attachedMonster.currentLocation != null)
						{
							attachedMonster.currentLocation.characters.Remove(attachedMonster);
						}
						else
						{
							location.characters.Remove(attachedMonster);
						}
						fullnessTrigger.Fire();
						attachedMonster = null;
					}
					Vector2.Distance(base.Position, tonguePosition.Value);
					tongueOut.Value = false;
					tongueReturn.Value = false;
				}
			}
		}
		else if (tongueOut.Value && attachedMonster != null)
		{
			attachedMonster.Position = tonguePosition.Value;
			attachedMonster.position.Paused = true;
			attachedMonster.xVelocity = 0f;
			attachedMonster.yVelocity = 0f;
		}
		fullnessTrigger.Poll();
	}

	public override void OnOwnerWarp()
	{
		attachedMonster = null;
		tongueOut.Value = false;
		tongueReturn.Value = false;
		base.OnOwnerWarp();
	}

	public override void Hop(float amount)
	{
		base.Hop(amount);
		if (fullnessTime > 0f)
		{
			base.Owner?.currentLocation.localSound("frog_slap");
		}
		lastHopTimer = 0f;
	}

	private void triggerFullnessTimer()
	{
		fullnessTime = 12000f;
	}

	public void tongueReachedMonster(Monster m)
	{
		tongueReturn.Value = true;
		tongueVelocity.Value *= -1f;
		attachedMonster = m;
		if (m != null)
		{
			m.DamageToFarmer = 0;
			m.farmerPassesThrough = true;
			base.Owner?.currentLocation.localSound("fishSlap");
		}
	}

	public override void Draw(SpriteBatch b)
	{
		if (base.Owner == null || base.Owner.currentLocation == null || (base.Owner.currentLocation.DisplayName == "Temp" && !Game1.isFestival()))
		{
			return;
		}
		Texture2D texture = Game1.content.Load<Texture2D>("TileSheets\\companions");
		SpriteEffects effect = SpriteEffects.None;
		Rectangle startingSourceRect = new Rectangle((fullnessTime > 0f) ? 128 : 0, 16 + whichVariant.Value * 16, 16, 16);
		Color c = ((whichVariant.Value == 7) ? Utility.GetPrismaticColor() : Color.White);
		if (direction.Value == 3)
		{
			effect = SpriteEffects.FlipHorizontally;
		}
		if (tongueOut.Value)
		{
			b.Draw(texture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset + new Vector2(0f, (0f - height) * 4f)), Utility.translateRect(startingSourceRect, 112), c, 0f, new Vector2(8f, 16f), 4f, effect, (_position.Y - 12f) / 10000f);
		}
		else if (height > 0f)
		{
			if (gravity > 0f)
			{
				b.Draw(texture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset + new Vector2(0f, (0f - height) * 4f)), Utility.translateRect(startingSourceRect, 16), c, 0f, new Vector2(8f, 16f), 4f, effect, (_position.Y - 12f) / 10000f);
			}
			else if (gravity > -0.15f)
			{
				b.Draw(texture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset + new Vector2(0f, (0f - height) * 4f)), Utility.translateRect(startingSourceRect, 32), c, 0f, new Vector2(8f, 16f), 4f, effect, (_position.Y - 12f) / 10000f);
			}
			else
			{
				b.Draw(texture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset + new Vector2(0f, (0f - height) * 4f)), Utility.translateRect(startingSourceRect, 48), c, 0f, new Vector2(8f, 16f), 4f, effect, (_position.Y - 12f) / 10000f);
			}
		}
		else if (lastHopTimer > 5000f && !tongueOut.Value)
		{
			b.Draw(texture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset + new Vector2(0f, (0f - height) * 4f)), Utility.translateRect(startingSourceRect, 80 + ((Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 400.0 >= 200.0) ? 16 : 0)), c, 0f, new Vector2(8f, 16f), 4f, effect, (_position.Y - 12f) / 10000f);
		}
		else
		{
			b.Draw(texture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset + new Vector2(0f, (0f - height) * 4f)), startingSourceRect, c, 0f, new Vector2(8f, 16f), 4f, effect, (_position.Y - 12f) / 10000f);
		}
		b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f * Utility.Lerp(1f, 0.8f, Math.Min(height, 1f)), SpriteEffects.None, 0f);
		if (tongueOut.Value)
		{
			Vector2 v = Game1.GlobalToLocal(tonguePosition.Value + new Vector2(32f));
			Vector2 v2 = Game1.GlobalToLocal(base.Position + new Vector2(-32f, -32f) + new Vector2((direction.Value != 3) ? 44 : 24, 16f));
			Utility.drawLineWithScreenCoordinates((int)v2.X, (int)v2.Y, (int)v.X, (int)v.Y, b, Color.Red, 1f, 4);
			Texture2D projTex = Projectile.projectileSheet;
			Rectangle sourceRect = Game1.getSourceRectForStandardTileSheet(Projectile.projectileSheet, 19, 16, 16);
			b.Draw(projTex, Game1.GlobalToLocal(tonguePosition.Value + new Vector2(32f, 32f)) + base.Owner.drawOffset, sourceRect, Color.White, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, 1f);
		}
	}
}
