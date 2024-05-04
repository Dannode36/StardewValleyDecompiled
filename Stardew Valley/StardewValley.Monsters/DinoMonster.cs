using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Extensions;
using StardewValley.Projectiles;

namespace StardewValley.Monsters;

[XmlInclude(typeof(BreathProjectile))]
public class DinoMonster : Monster
{
	public enum AttackState
	{
		None,
		Fireball,
		Charge
	}

	/// <summary>Lightweight version of projectile for pooling.</summary>
	public class BreathProjectile : INetObject<NetFields>
	{
		public readonly NetBool active = new NetBool();

		public readonly NetVector2 position = new NetVector2();

		public readonly NetVector2 startPosition = new NetVector2();

		public readonly NetVector2 velocity = new NetVector2();

		public float rotation;

		public float alpha;

		public NetFields NetFields { get; } = new NetFields("BreathProjectile");


		public BreathProjectile()
		{
			NetFields.SetOwner(this).AddField(active, "active").AddField(position, "position")
				.AddField(startPosition, "startPosition")
				.AddField(velocity, "velocity");
			active.InterpolationEnabled = (active.InterpolationWait = false);
			position.InterpolationEnabled = (position.InterpolationWait = false);
			startPosition.InterpolationEnabled = (startPosition.InterpolationWait = false);
			velocity.InterpolationEnabled = (velocity.InterpolationWait = false);
		}

		public Rectangle GetBoundingBox()
		{
			Vector2 pos = position.Value;
			int damageSize = 29;
			float currentScale = 1f;
			damageSize = (int)((float)damageSize * currentScale);
			return new Rectangle((int)pos.X + 32 - damageSize / 2, (int)pos.Y + 32 - damageSize / 2, damageSize, damageSize);
		}

		public Rectangle GetSourceRect()
		{
			return Game1.getSourceRectForStandardTileSheet(Projectile.projectileSheet, 10, 16, 16);
		}

		public void ExplosionAnimation(GameLocation location)
		{
			Rectangle sourceRect = GetSourceRect();
			sourceRect.X += 4;
			sourceRect.Y += 4;
			sourceRect.Width = 8;
			sourceRect.Height = 8;
			Game1.createRadialDebris_MoreNatural(location, "TileSheets\\Projectiles", sourceRect, 1, (int)position.X + 32, (int)position.Y + 32, 6, (int)(position.Y / 64f) + 1);
		}

		public void Update(GameTime time, GameLocation location, DinoMonster parent)
		{
			if (!active.Value)
			{
				return;
			}
			position.Value += velocity.Value;
			if (!Game1.IsMasterGame)
			{
				position.MarkClean();
				position.ResetNewestReceivedChangeVersion();
			}
			float dist = Vector2.Distance(position.Value, startPosition.Value);
			if (dist > 128f)
			{
				alpha = (256f - dist) / 128f;
			}
			else
			{
				alpha = 1f;
			}
			if (dist > 256f)
			{
				active.Value = false;
				return;
			}
			Rectangle boundingBox = GetBoundingBox();
			if (Game1.player.currentLocation == location && Game1.player.CanBeDamaged() && boundingBox.Intersects(Game1.player.GetBoundingBox()))
			{
				Game1.player.takeDamage(25, overrideParry: false, null);
				ExplosionAnimation(location);
				active.Value = false;
				return;
			}
			foreach (Vector2 tile in Utility.getListOfTileLocationsForBordersOfNonTileRectangle(boundingBox))
			{
				if (location.terrainFeatures.TryGetValue(tile, out var feature) && !feature.isPassable())
				{
					ExplosionAnimation(location);
					active.Value = false;
					return;
				}
			}
			if (!location.isTileOnMap(position.Value / 64f) || location.isCollidingPosition(boundingBox, Game1.viewport, isFarmer: false, 0, glider: true, parent, pathfinding: false, projectile: true))
			{
				ExplosionAnimation(location);
				active.Value = false;
			}
		}

		public void Draw(SpriteBatch b)
		{
			if (active.Value)
			{
				float currentScale = 4f;
				Texture2D texture = Projectile.projectileSheet;
				Rectangle sourceRect = GetSourceRect();
				Vector2 pixelPosition = position.Value;
				b.Draw(texture, Game1.GlobalToLocal(Game1.viewport, pixelPosition + new Vector2(32f, 32f)), sourceRect, Color.White * alpha, rotation, new Vector2(8f, 8f), currentScale, SpriteEffects.None, (pixelPosition.Y + 96f) / 10000f);
			}
		}
	}

	public int timeUntilNextAttack;

	public readonly NetBool firing = new NetBool(value: false);

	public NetInt attackState = new NetInt();

	public int nextFireTime;

	public int totalFireTime;

	public int nextChangeDirectionTime;

	public int nextWanderTime;

	public bool wanderState;

	public readonly NetObjectArray<BreathProjectile> projectiles = new NetObjectArray<BreathProjectile>(15);

	public int lastProjectileSlot;

	public DinoMonster()
	{
	}

	public DinoMonster(Vector2 position)
		: base("Pepper Rex", position)
	{
		Sprite.SpriteWidth = 32;
		Sprite.SpriteHeight = 32;
		Sprite.UpdateSourceRect();
		timeUntilNextAttack = 2000;
		nextChangeDirectionTime = Game1.random.Next(1000, 3000);
		nextWanderTime = Game1.random.Next(1000, 2000);
		for (int i = 0; i < projectiles.Count; i++)
		{
			projectiles[i] = new BreathProjectile();
		}
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(attackState, "attackState").AddField(firing, "firing").AddField(projectiles, "projectiles");
	}

	/// <inheritdoc />
	public override void reloadSprite(bool onlyAppearance = false)
	{
		base.reloadSprite(onlyAppearance);
		Sprite.SpriteWidth = 32;
		Sprite.SpriteHeight = 32;
		Sprite.UpdateSourceRect();
	}

	public override void draw(SpriteBatch b)
	{
		if (base.Health > 0 && !base.IsInvisible && Utility.isOnScreen(base.Position, 128))
		{
			int standingY = base.StandingPixel.Y;
			b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(56f, 16 + yJumpOffset), Sprite.SourceRect, Color.White, rotation, new Vector2(16f, 16f), Math.Max(0.2f, scale.Value) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : ((float)standingY / 10000f)));
			if (isGlowing)
			{
				b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(56f, 16 + yJumpOffset), Sprite.SourceRect, glowingColor * glowingTransparency, 0f, new Vector2(16f, 16f), 4f * Math.Max(0.2f, scale.Value), flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : ((float)standingY / 10000f + 0.001f)));
			}
		}
		foreach (BreathProjectile projectile in projectiles)
		{
			if (Utility.isOnScreen(projectile.position.Value, 64))
			{
				projectile.Draw(b);
			}
		}
	}

	public override Rectangle GetBoundingBox()
	{
		if (base.Health <= 0)
		{
			return new Rectangle(-100, -100, 0, 0);
		}
		Vector2 position = base.Position;
		return new Rectangle((int)position.X + 8, (int)position.Y, Sprite.SpriteWidth * 4 * 3 / 4, 64);
	}

	public override List<Item> getExtraDropItems()
	{
		List<Item> extra_items = new List<Item>();
		if (Game1.random.NextDouble() < 0.10000000149011612)
		{
			extra_items.Add(ItemRegistry.Create("(O)107"));
		}
		else
		{
			List<Item> non_egg_items = new List<Item>();
			non_egg_items.Add(ItemRegistry.Create("(O)580"));
			non_egg_items.Add(ItemRegistry.Create("(O)583"));
			non_egg_items.Add(ItemRegistry.Create("(O)584"));
			extra_items.Add(Game1.random.ChooseFrom(non_egg_items));
		}
		return extra_items;
	}

	public override bool ShouldMonsterBeRemoved()
	{
		foreach (BreathProjectile projectile in projectiles)
		{
			if ((bool)projectile.active)
			{
				return false;
			}
		}
		return base.ShouldMonsterBeRemoved();
	}

	protected override void sharedDeathAnimation()
	{
		base.currentLocation.playSound("skeletonDie");
		base.currentLocation.playSound("grunt");
		Rectangle bounds = GetBoundingBox();
		for (int i = 0; i < 16; i++)
		{
			Game1.createRadialDebris(base.currentLocation, Sprite.textureName, new Rectangle(64, 128, 16, 16), 16, (int)Utility.Lerp(bounds.Left, bounds.Right, (float)Game1.random.NextDouble()), (int)Utility.Lerp(bounds.Bottom, bounds.Top, (float)Game1.random.NextDouble()), 1, base.TilePoint.Y, Color.White, 4f);
		}
	}

	protected override void localDeathAnimation()
	{
		Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(44, base.Position, Color.HotPink, 10)
		{
			holdLastFrame = true,
			alphaFade = 0.01f,
			interval = 70f
		}, base.currentLocation, 8, 96);
	}

	public override void update(GameTime time, GameLocation location)
	{
		if (base.Health > 0)
		{
			base.update(time, location);
		}
		foreach (BreathProjectile projectile in projectiles)
		{
			projectile.Update(time, location, this);
		}
	}

	public override void behaviorAtGameTick(GameTime time)
	{
		if (attackState.Value == 1)
		{
			base.IsWalkingTowardPlayer = false;
			Halt();
		}
		else if (withinPlayerThreshold())
		{
			base.IsWalkingTowardPlayer = true;
		}
		else
		{
			base.IsWalkingTowardPlayer = false;
			nextChangeDirectionTime -= time.ElapsedGameTime.Milliseconds;
			nextWanderTime -= time.ElapsedGameTime.Milliseconds;
			if (nextChangeDirectionTime < 0)
			{
				nextChangeDirectionTime = Game1.random.Next(500, 1000);
				facingDirection.Value = (facingDirection.Value + (Game1.random.Next(0, 3) - 1) + 4) % 4;
			}
			if (nextWanderTime < 0)
			{
				if (wanderState)
				{
					nextWanderTime = Game1.random.Next(1000, 2000);
				}
				else
				{
					nextWanderTime = Game1.random.Next(1000, 3000);
				}
				wanderState = !wanderState;
			}
			if (wanderState)
			{
				moveLeft = (moveUp = (moveRight = (moveDown = false)));
				tryToMoveInDirection(facingDirection.Value, isFarmer: false, base.DamageToFarmer, isGlider);
			}
		}
		timeUntilNextAttack -= time.ElapsedGameTime.Milliseconds;
		if (attackState.Value == 0 && withinPlayerThreshold(2))
		{
			firing.Set(newValue: false);
			if (timeUntilNextAttack < 0)
			{
				timeUntilNextAttack = 0;
				attackState.Set(1);
				nextFireTime = 500;
				totalFireTime = 3000;
				base.currentLocation.playSound("croak");
			}
		}
		else
		{
			if (totalFireTime <= 0)
			{
				return;
			}
			if (!firing)
			{
				Farmer player = base.Player;
				if (player != null)
				{
					faceGeneralDirection(player.Position);
				}
			}
			totalFireTime -= time.ElapsedGameTime.Milliseconds;
			if (nextFireTime > 0)
			{
				nextFireTime -= time.ElapsedGameTime.Milliseconds;
				if (nextFireTime <= 0)
				{
					if (!firing.Value)
					{
						firing.Set(newValue: true);
						base.currentLocation.playSound("furnace");
					}
					float fire_angle = 0f;
					Point standingPixel = base.StandingPixel;
					Vector2 shot_origin = new Vector2((float)standingPixel.X - 32f, (float)standingPixel.Y - 32f);
					switch (facingDirection.Value)
					{
					case 0:
						yVelocity = -1f;
						shot_origin.Y -= 64f;
						fire_angle = 90f;
						break;
					case 1:
						xVelocity = -1f;
						shot_origin.X += 64f;
						fire_angle = 0f;
						break;
					case 3:
						xVelocity = 1f;
						shot_origin.X -= 64f;
						fire_angle = 180f;
						break;
					case 2:
						yVelocity = 1f;
						fire_angle = 270f;
						break;
					}
					fire_angle += (float)Math.Sin((double)((float)totalFireTime / 1000f * 180f) * Math.PI / 180.0) * 25f;
					Vector2 shot_velocity = new Vector2((float)Math.Cos((double)fire_angle * Math.PI / 180.0), 0f - (float)Math.Sin((double)fire_angle * Math.PI / 180.0));
					shot_velocity *= 10f;
					BreathProjectile projectile = projectiles[lastProjectileSlot];
					projectile.active.Value = true;
					NetVector2 netVector = projectile.position;
					Vector2 value = (projectile.startPosition.Value = shot_origin);
					netVector.Value = value;
					projectile.velocity.Value = shot_velocity;
					lastProjectileSlot = (lastProjectileSlot + 1) % projectiles.Count;
					nextFireTime = 70;
				}
			}
			if (totalFireTime <= 0)
			{
				totalFireTime = 0;
				nextFireTime = 0;
				attackState.Set(0);
				timeUntilNextAttack = Game1.random.Next(1000, 2000);
			}
		}
	}

	protected override void updateAnimation(GameTime time)
	{
		int direction_offset = 0;
		switch (FacingDirection)
		{
		case 2:
			direction_offset = 0;
			break;
		case 1:
			direction_offset = 4;
			break;
		case 0:
			direction_offset = 8;
			break;
		case 3:
			direction_offset = 12;
			break;
		}
		if (attackState.Value == 1)
		{
			if (firing.Value)
			{
				Sprite.CurrentFrame = 16 + direction_offset;
			}
			else
			{
				Sprite.CurrentFrame = 17 + direction_offset;
			}
			return;
		}
		if (isMoving() || wanderState)
		{
			switch (FacingDirection)
			{
			case 0:
				Sprite.AnimateUp(time);
				break;
			case 3:
				Sprite.AnimateLeft(time);
				break;
			case 1:
				Sprite.AnimateRight(time);
				break;
			case 2:
				Sprite.AnimateDown(time);
				break;
			}
			return;
		}
		switch (FacingDirection)
		{
		case 0:
			Sprite.AnimateUp(time);
			break;
		case 3:
			Sprite.AnimateLeft(time);
			break;
		case 1:
			Sprite.AnimateRight(time);
			break;
		case 2:
			Sprite.AnimateDown(time);
			break;
		}
		Sprite.StopAnimation();
	}

	protected override void updateMonsterSlaveAnimation(GameTime time)
	{
		int direction_offset = 0;
		switch (FacingDirection)
		{
		case 2:
			direction_offset = 0;
			break;
		case 1:
			direction_offset = 4;
			break;
		case 0:
			direction_offset = 8;
			break;
		case 3:
			direction_offset = 12;
			break;
		}
		if (attackState.Value == 1)
		{
			if (firing.Value)
			{
				Sprite.CurrentFrame = 16 + direction_offset;
			}
			else
			{
				Sprite.CurrentFrame = 17 + direction_offset;
			}
		}
		else if (isMoving())
		{
			switch (FacingDirection)
			{
			case 0:
				Sprite.AnimateUp(time);
				break;
			case 3:
				Sprite.AnimateLeft(time);
				break;
			case 1:
				Sprite.AnimateRight(time);
				break;
			case 2:
				Sprite.AnimateDown(time);
				break;
			}
		}
		else
		{
			Sprite.StopAnimation();
		}
	}
}
