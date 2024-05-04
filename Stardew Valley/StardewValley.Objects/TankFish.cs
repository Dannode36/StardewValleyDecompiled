using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.ItemTypeDefinitions;

namespace StardewValley.Objects;

public class TankFish
{
	public enum FishType
	{
		Normal,
		Eel,
		Cephalopod,
		Float,
		Ground,
		Crawl,
		Hop,
		Static
	}

	/// <summary>The field index in <c>Data/AquariumFish</c> for the sprite index.</summary>
	public const int field_spriteIndex = 0;

	/// <summary>The field index in <c>Data/AquariumFish</c> for the type.</summary>
	public const int field_type = 1;

	/// <summary>The field index in <c>Data/AquariumFish</c> for the idle animations.</summary>
	public const int field_idleAnimations = 2;

	/// <summary>The field index in <c>Data/AquariumFish</c> for the dart start animation frames.</summary>
	public const int field_dartStartFrames = 3;

	/// <summary>The field index in <c>Data/AquariumFish</c> for the dart hold animation frames.</summary>
	public const int field_dartHoldFrames = 4;

	/// <summary>The field index in <c>Data/AquariumFish</c> for the dart end animation frames.</summary>
	public const int field_dartEndFrames = 5;

	/// <summary>The field index in <c>Data/AquariumFish</c> for the texture, if set.</summary>
	public const int field_texture = 6;

	/// <summary>The field index in <c>Data/AquariumFish</c> for the pixel offset from the upper-left corner of sprite that the hat sits on, if set.</summary>
	public const int field_hatOffset = 7;

	protected FishTankFurniture _tank;

	public Vector2 position;

	public float zPosition;

	public bool facingLeft;

	public Vector2 velocity = Vector2.Zero;

	protected Texture2D _texture;

	public float nextSwim;

	public string fishItemId = "";

	public int fishIndex;

	public int currentFrame;

	public Point? hatPosition;

	public int frogVariant;

	public int numberOfDarts;

	public FishType fishType;

	public float minimumVelocity;

	public float fishScale = 1f;

	public List<int> currentAnimation;

	public List<int> idleAnimation;

	public List<int> dartStartAnimation;

	public List<int> dartHoldAnimation;

	public List<int> dartEndAnimation;

	public int currentAnimationFrame;

	public float currentFrameTime;

	public float nextBubble;

	public bool isErrorFish;

	public TankFish(FishTankFurniture tank, Item item)
	{
		_tank = tank;
		fishItemId = item.ItemId;
		if (!_tank.GetAquariumData().TryGetValue(item.ItemId, out var rawAquariumData))
		{
			rawAquariumData = "0/float";
			isErrorFish = true;
		}
		string[] aquarium_fish_split = rawAquariumData.Split('/');
		string rawTexture = ArgUtility.Get(aquarium_fish_split, 6, null, allowBlank: false);
		if (rawTexture != null)
		{
			try
			{
				_texture = Game1.content.Load<Texture2D>(rawTexture);
			}
			catch (Exception)
			{
				isErrorFish = true;
			}
		}
		if (_texture == null)
		{
			_texture = _tank.GetAquariumTexture();
		}
		string rawHatOffset = ArgUtility.Get(aquarium_fish_split, 7, null, allowBlank: false);
		if (rawHatOffset != null)
		{
			try
			{
				string[] point_split = ArgUtility.SplitBySpace(rawHatOffset);
				hatPosition = new Point(int.Parse(point_split[0]), int.Parse(point_split[1]));
			}
			catch (Exception)
			{
				hatPosition = null;
			}
		}
		fishIndex = int.Parse(aquarium_fish_split[0]);
		currentFrame = fishIndex;
		zPosition = Utility.RandomFloat(4f, 10f);
		fishScale = 0.75f;
		if (DataLoader.Fish(Game1.content).TryGetValue(item.ItemId, out var fish_data))
		{
			string[] fish_split = fish_data.Split('/');
			if (!(fish_split[1] == "trap"))
			{
				minimumVelocity = Utility.RandomFloat(0.25f, 0.35f);
				if (fish_split[2] == "smooth")
				{
					minimumVelocity = Utility.RandomFloat(0.5f, 0.6f);
				}
				if (fish_split[2] == "dart")
				{
					minimumVelocity = 0f;
				}
			}
		}
		switch (ArgUtility.Get(aquarium_fish_split, 1))
		{
		case "eel":
			fishType = FishType.Eel;
			minimumVelocity = Utility.Clamp(fishScale, 0.3f, 0.4f);
			break;
		case "cephalopod":
			fishType = FishType.Cephalopod;
			minimumVelocity = 0f;
			break;
		case "ground":
			fishType = FishType.Ground;
			zPosition = 4f;
			minimumVelocity = 0f;
			break;
		case "static":
			fishType = FishType.Static;
			break;
		case "crawl":
			fishType = FishType.Crawl;
			minimumVelocity = 0f;
			break;
		case "front_crawl":
			fishType = FishType.Crawl;
			zPosition = 3f;
			minimumVelocity = 0f;
			break;
		case "float":
			fishType = FishType.Float;
			break;
		}
		string rawIdleAnimation = ArgUtility.Get(aquarium_fish_split, 2, null, allowBlank: false);
		if (rawIdleAnimation != null)
		{
			string[] array = ArgUtility.SplitBySpace(rawIdleAnimation);
			idleAnimation = new List<int>();
			string[] array2 = array;
			foreach (string frame in array2)
			{
				idleAnimation.Add(int.Parse(frame));
			}
			SetAnimation(idleAnimation);
		}
		string rawDartStartFrames = ArgUtility.Get(aquarium_fish_split, 3, null, allowBlank: false);
		if (rawDartStartFrames != null)
		{
			string[] array3 = ArgUtility.SplitBySpace(rawDartStartFrames);
			dartStartAnimation = new List<int>();
			string[] array2 = array3;
			foreach (string frame in array2)
			{
				dartStartAnimation.Add(int.Parse(frame));
			}
		}
		string rawDartHoldFrames = ArgUtility.Get(aquarium_fish_split, 4, null, allowBlank: false);
		if (rawDartHoldFrames != null)
		{
			string[] array4 = ArgUtility.SplitBySpace(rawDartHoldFrames);
			dartHoldAnimation = new List<int>();
			string[] array2 = array4;
			foreach (string frame in array2)
			{
				dartHoldAnimation.Add(int.Parse(frame));
			}
		}
		string rawDartEndFrames = ArgUtility.Get(aquarium_fish_split, 5, null, allowBlank: false);
		if (rawDartEndFrames != null)
		{
			string[] array5 = ArgUtility.SplitBySpace(rawDartEndFrames);
			dartEndAnimation = new List<int>();
			string[] array2 = array5;
			foreach (string frame in array2)
			{
				dartEndAnimation.Add(int.Parse(frame));
			}
		}
		Rectangle tank_bounds_local = _tank.GetTankBounds();
		tank_bounds_local.X = 0;
		tank_bounds_local.Y = 0;
		position = Vector2.Zero;
		position = Utility.getRandomPositionInThisRectangle(tank_bounds_local, Game1.random);
		nextSwim = Utility.RandomFloat(0.1f, 10f);
		nextBubble = Utility.RandomFloat(0.1f, 10f);
		facingLeft = Game1.random.Next(2) == 1;
		if (facingLeft)
		{
			velocity = new Vector2(-1f, 0f);
		}
		else
		{
			velocity = new Vector2(1f, 0f);
		}
		velocity *= minimumVelocity;
		if (item.QualifiedItemId == "(TR)FrogEgg")
		{
			fishType = FishType.Hop;
			_texture = Game1.content.Load<Texture2D>("TileSheets\\companions");
			frogVariant = ((item as Trinket).GetEffect() as CompanionTrinketEffect).variant;
			isErrorFish = false;
		}
		if (fishType == FishType.Ground || fishType == FishType.Crawl || fishType == FishType.Hop || fishType == FishType.Static)
		{
			position.Y = 0f;
		}
		ConstrainToTank();
	}

	public void SetAnimation(List<int> frames)
	{
		if (fishType != FishType.Hop && currentAnimation != frames)
		{
			currentAnimation = frames;
			currentAnimationFrame = 0;
			currentFrameTime = 0f;
			List<int> list = currentAnimation;
			if (list != null && list.Count > 0)
			{
				currentFrame = frames[0];
			}
		}
	}

	public virtual void Draw(SpriteBatch b, float alpha, float draw_layer)
	{
		SpriteEffects sprite_effects = SpriteEffects.None;
		int draw_offset = -12;
		int slice_size = 8;
		if (fishType == FishType.Eel)
		{
			slice_size = 4;
		}
		int slice_offset = slice_size;
		if (facingLeft)
		{
			sprite_effects = SpriteEffects.FlipHorizontally;
			slice_offset *= -1;
			draw_offset = -draw_offset - slice_size;
		}
		float bob = (float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalSeconds * 1.25 + (double)(position.X / 32f)) * 2f;
		if (fishType == FishType.Crawl || fishType == FishType.Ground || fishType == FishType.Static)
		{
			bob = 0f;
		}
		float scale = GetScale();
		int cols = _texture.Width / 24;
		int sprite_sheet_x = currentFrame % cols * 24;
		int sprite_sheet_y = currentFrame / cols * 48;
		int wiggle_start_pixels = 10;
		float wiggle_amount = 1f;
		if (fishType == FishType.Eel)
		{
			wiggle_start_pixels = 20;
			bob *= 0f;
		}
		float hatOffsetY = -12f;
		float angle = 0f;
		if (isErrorFish)
		{
			angle = 0f;
			IItemDataDefinition itemType = ItemRegistry.RequireTypeDefinition("(F)");
			b.Draw(itemType.GetErrorTexture(), Game1.GlobalToLocal(GetWorldPosition() + new Vector2(0f, bob) * 4f * scale), itemType.GetErrorSourceRect(), Color.White * alpha, angle, new Vector2(12f, 12f), 4f * scale, sprite_effects, draw_layer);
		}
		else
		{
			switch (fishType)
			{
			case FishType.Ground:
			case FishType.Crawl:
			case FishType.Static:
				angle = 0f;
				b.Draw(_texture, Game1.GlobalToLocal(GetWorldPosition() + new Vector2(0f, bob) * 4f * scale), new Rectangle(sprite_sheet_x, sprite_sheet_y, 24, 24), Color.White * alpha, angle, new Vector2(12f, 12f), 4f * scale, sprite_effects, draw_layer);
				break;
			case FishType.Hop:
			{
				int frame = 0;
				if (position.Y > 0f)
				{
					frame = ((!((double)velocity.Y > 0.2)) ? 3 : (((double)velocity.Y > 0.3) ? 1 : 2));
				}
				else if (nextSwim <= 3f)
				{
					frame = ((Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 400.0 >= 200.0) ? 5 : 6);
				}
				Rectangle rect = new Rectangle(frame * 16, 16 + frogVariant * 16, 16, 16);
				Color c = Color.White;
				if (frogVariant == 7)
				{
					c = Utility.GetPrismaticColor();
				}
				b.Draw(_texture, Game1.GlobalToLocal(GetWorldPosition() + new Vector2(16f, -8f)), rect, c * alpha, angle, new Vector2(12f, 12f), 4f * scale, sprite_effects, draw_layer);
				break;
			}
			case FishType.Cephalopod:
			case FishType.Float:
				angle = Utility.Clamp(velocity.X, -0.5f, 0.5f);
				b.Draw(_texture, Game1.GlobalToLocal(GetWorldPosition() + new Vector2(0f, bob) * 4f * scale), new Rectangle(sprite_sheet_x, sprite_sheet_y, 24, 24), Color.White * alpha, angle, new Vector2(12f, 12f), 4f * scale, sprite_effects, draw_layer);
				break;
			default:
			{
				for (int slice = 0; slice < 24 / slice_size; slice++)
				{
					float multiplier = (float)(slice * slice_size) / (float)wiggle_start_pixels;
					multiplier = 1f - multiplier;
					float velocity_multiplier = velocity.Length() / 1f;
					float time_multiplier = 1f;
					float position_multiplier = 0f;
					velocity_multiplier = Utility.Clamp(velocity_multiplier, 0.2f, 1f);
					multiplier = Utility.Clamp(multiplier, 0f, 1f);
					if (fishType == FishType.Eel)
					{
						multiplier = 1f;
						velocity_multiplier = 1f;
						time_multiplier = 0.1f;
						position_multiplier = 4f;
					}
					if (facingLeft)
					{
						position_multiplier *= -1f;
					}
					float yOffset = (float)(Math.Sin((double)(slice * 20) + Game1.currentGameTime.TotalGameTime.TotalSeconds * 25.0 * (double)time_multiplier + (double)(position_multiplier * position.X / 16f)) * (double)wiggle_amount * (double)multiplier * (double)velocity_multiplier);
					if (slice == 24 / slice_size - 1)
					{
						hatOffsetY = -12f + yOffset;
					}
					b.Draw(_texture, Game1.GlobalToLocal(GetWorldPosition() + new Vector2(draw_offset + slice * slice_offset, bob + yOffset) * 4f * scale), new Rectangle(sprite_sheet_x + slice * slice_size, sprite_sheet_y, slice_size, 24), Color.White * alpha, 0f, new Vector2(0f, 12f), 4f * scale, sprite_effects, draw_layer);
				}
				break;
			}
			}
		}
		float hatOffsetX = (facingLeft ? 12 : (-12));
		b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(new Vector2(GetWorldPosition().X, (float)_tank.GetTankBounds().Bottom - zPosition * 4f)), null, Color.White * alpha * 0.75f, 0f, new Vector2(Game1.shadowTexture.Width / 2, Game1.shadowTexture.Height / 2), new Vector2(4f * scale, 1f), SpriteEffects.None, _tank.GetFishSortRegion().X - 1E-07f);
		int hatsDrawn = 0;
		foreach (TankFish fish in _tank.tankFish)
		{
			if (fish == this)
			{
				break;
			}
			if (fish.CanWearHat())
			{
				hatsDrawn++;
			}
		}
		if (!CanWearHat())
		{
			return;
		}
		int hatsSoFar = 0;
		foreach (Item heldItem in _tank.heldItems)
		{
			if (!(heldItem is Hat hat))
			{
				continue;
			}
			if (hatsSoFar == hatsDrawn)
			{
				Vector2 hatPlacementOffset = new Vector2(hatPosition.Value.X, hatPosition.Value.Y);
				if (facingLeft)
				{
					hatPlacementOffset.X *= -1f;
				}
				Vector2 hatOffset = new Vector2(hatOffsetX, hatOffsetY) + hatPlacementOffset;
				if (angle != 0f)
				{
					float cos = (float)Math.Cos(angle);
					float sin = (float)Math.Sin(angle);
					hatOffset.X = hatOffset.X * cos - hatOffset.Y * sin;
					hatOffset.Y = hatOffset.X * sin + hatOffset.Y * cos;
				}
				hatOffset *= 4f * scale;
				Vector2 pos = Game1.GlobalToLocal(GetWorldPosition() + hatOffset);
				pos.Y += bob;
				int direction = ((fishType == FishType.Cephalopod || fishType == FishType.Static) ? 2 : ((!facingLeft) ? 1 : 3));
				pos -= new Vector2(10f, 10f);
				pos += new Vector2(3f, 3f) * scale * 3f;
				pos -= new Vector2(10f, 10f) * scale * 3f;
				hat.draw(b, pos, scale, 1f, draw_layer + 1E-08f, direction);
				hatsDrawn++;
				break;
			}
			hatsSoFar++;
		}
	}

	[MemberNotNullWhen(true, "hatPosition")]
	public bool CanWearHat()
	{
		return hatPosition.HasValue;
	}

	public Vector2 GetWorldPosition()
	{
		return new Vector2((float)_tank.GetTankBounds().X + position.X, (float)_tank.GetTankBounds().Bottom - position.Y - zPosition * 4f);
	}

	public void ConstrainToTank()
	{
		Rectangle tank_bounds = _tank.GetTankBounds();
		Rectangle bounds = GetBounds();
		tank_bounds.X = 0;
		tank_bounds.Y = 0;
		if (bounds.X < tank_bounds.X)
		{
			position.X += tank_bounds.X - bounds.X;
			bounds = GetBounds();
		}
		if (bounds.Y < tank_bounds.Y)
		{
			position.Y -= tank_bounds.Y - bounds.Y;
			bounds = GetBounds();
		}
		if (bounds.Right > tank_bounds.Right)
		{
			position.X += tank_bounds.Right - bounds.Right;
			bounds = GetBounds();
		}
		if (fishType == FishType.Crawl || fishType == FishType.Ground || fishType == FishType.Static || fishType == FishType.Hop)
		{
			if (position.Y > (float)tank_bounds.Bottom)
			{
				position.Y -= (float)tank_bounds.Bottom - position.Y;
			}
		}
		else if (bounds.Bottom > tank_bounds.Bottom)
		{
			position.Y -= tank_bounds.Bottom - bounds.Bottom;
		}
	}

	public virtual float GetScale()
	{
		return fishScale;
	}

	public Rectangle GetBounds()
	{
		Vector2 dimensions = new Vector2(24f, 18f);
		dimensions *= 4f * GetScale();
		if (fishType == FishType.Crawl || fishType == FishType.Ground || fishType == FishType.Static || fishType == FishType.Hop)
		{
			return new Rectangle((int)(position.X - dimensions.X / 2f), (int)((float)_tank.GetTankBounds().Height - position.Y - dimensions.Y), (int)dimensions.X, (int)dimensions.Y);
		}
		return new Rectangle((int)(position.X - dimensions.X / 2f), (int)((float)_tank.GetTankBounds().Height - position.Y - dimensions.Y / 2f), (int)dimensions.X, (int)dimensions.Y);
	}

	public virtual void Update(GameTime time)
	{
		List<int> list = currentAnimation;
		if (list != null && list.Count > 0)
		{
			currentFrameTime += (float)time.ElapsedGameTime.TotalSeconds;
			float seconds_per_frame = 0.125f;
			if (currentFrameTime > seconds_per_frame)
			{
				currentAnimationFrame += (int)(currentFrameTime / seconds_per_frame);
				currentFrameTime %= seconds_per_frame;
				if (currentAnimationFrame >= currentAnimation.Count)
				{
					if (currentAnimation == idleAnimation)
					{
						currentAnimationFrame %= currentAnimation.Count;
						currentFrame = currentAnimation[currentAnimationFrame];
					}
					else if (currentAnimation == dartStartAnimation)
					{
						if (dartHoldAnimation != null)
						{
							SetAnimation(dartHoldAnimation);
						}
						else
						{
							SetAnimation(idleAnimation);
						}
					}
					else if (currentAnimation == dartHoldAnimation)
					{
						currentAnimationFrame %= currentAnimation.Count;
						currentFrame = currentAnimation[currentAnimationFrame];
					}
					else if (currentAnimation == dartEndAnimation)
					{
						SetAnimation(idleAnimation);
					}
				}
				else
				{
					currentFrame = currentAnimation[currentAnimationFrame];
				}
			}
		}
		if (fishType != FishType.Static)
		{
			Rectangle local_tank_bounds = _tank.GetTankBounds();
			local_tank_bounds.X = 0;
			local_tank_bounds.Y = 0;
			float velocity_x = velocity.X;
			if (fishType == FishType.Crawl)
			{
				velocity_x = Utility.Clamp(velocity_x, -0.5f, 0.5f);
			}
			position.X += velocity_x;
			Rectangle bounds = GetBounds();
			if (bounds.Left < local_tank_bounds.Left || bounds.Right > local_tank_bounds.Right)
			{
				ConstrainToTank();
				bounds = GetBounds();
				velocity.X *= -1f;
				facingLeft = !facingLeft;
			}
			position.Y += velocity.Y;
			bounds = GetBounds();
			if (bounds.Top < local_tank_bounds.Top || bounds.Bottom > local_tank_bounds.Bottom)
			{
				ConstrainToTank();
				velocity.Y *= 0f;
			}
			float move_magnitude = velocity.Length();
			if (move_magnitude > minimumVelocity)
			{
				float deceleration = 0.015f;
				if (fishType == FishType.Crawl || fishType == FishType.Ground || fishType == FishType.Hop)
				{
					deceleration = 0.03f;
				}
				move_magnitude = Utility.Lerp(move_magnitude, minimumVelocity, deceleration);
				if (move_magnitude < 0.0001f)
				{
					move_magnitude = 0f;
				}
				velocity.Normalize();
				velocity *= move_magnitude;
				if (currentAnimation == dartHoldAnimation && move_magnitude <= minimumVelocity + 0.5f)
				{
					List<int> list2 = dartEndAnimation;
					if (list2 != null && list2.Count > 0)
					{
						SetAnimation(dartEndAnimation);
					}
					else
					{
						List<int> list3 = idleAnimation;
						if (list3 != null && list3.Count > 0)
						{
							SetAnimation(idleAnimation);
						}
					}
				}
			}
			nextSwim -= (float)time.ElapsedGameTime.TotalSeconds;
			if (nextSwim <= 0f)
			{
				if (numberOfDarts == 0)
				{
					numberOfDarts = Game1.random.Next(1, 4);
					nextSwim = Utility.RandomFloat(6f, 12f);
					if (fishType == FishType.Cephalopod)
					{
						nextSwim = Utility.RandomFloat(2f, 5f);
					}
					if (fishType == FishType.Hop)
					{
						numberOfDarts = 0;
					}
					if (Game1.random.NextDouble() < 0.30000001192092896)
					{
						facingLeft = !facingLeft;
					}
				}
				else
				{
					nextSwim = Utility.RandomFloat(0.1f, 0.5f);
					numberOfDarts--;
					if (Game1.random.NextDouble() < 0.05000000074505806)
					{
						facingLeft = !facingLeft;
					}
				}
				List<int> list4 = dartStartAnimation;
				if (list4 != null && list4.Count > 0)
				{
					SetAnimation(dartStartAnimation);
				}
				else
				{
					List<int> list5 = dartHoldAnimation;
					if (list5 != null && list5.Count > 0)
					{
						SetAnimation(dartHoldAnimation);
					}
				}
				velocity.X = 1.5f;
				if (_tank.getTilesWide() <= 2)
				{
					velocity.X *= 0.5f;
				}
				if (facingLeft)
				{
					velocity.X *= -1f;
				}
				switch (fishType)
				{
				case FishType.Cephalopod:
					velocity.Y = Utility.RandomFloat(0.5f, 0.75f);
					break;
				case FishType.Ground:
					velocity.X *= 0.5f;
					velocity.Y = Utility.RandomFloat(0.5f, 0.25f);
					break;
				case FishType.Hop:
					velocity.Y = Utility.RandomFloat(0.35f, 0.65f);
					break;
				default:
					velocity.Y = Utility.RandomFloat(-0.5f, 0.5f);
					break;
				}
				if (fishType == FishType.Crawl)
				{
					velocity.Y = 0f;
				}
			}
		}
		if (fishType == FishType.Cephalopod || fishType == FishType.Ground || fishType == FishType.Crawl || fishType == FishType.Static || fishType == FishType.Hop)
		{
			float fall_speed = 0.2f;
			if (fishType == FishType.Static)
			{
				fall_speed = 0.6f;
			}
			if (position.Y > 0f)
			{
				position.Y -= fall_speed;
			}
		}
		nextBubble -= (float)time.ElapsedGameTime.TotalSeconds;
		if (nextBubble <= 0f)
		{
			nextBubble = Utility.RandomFloat(1f, 10f);
			float x_offset = 0f;
			if (fishType == FishType.Ground || fishType == FishType.Normal || fishType == FishType.Eel)
			{
				x_offset = 32f;
			}
			if (facingLeft)
			{
				x_offset *= -1f;
			}
			x_offset *= fishScale;
			_tank.bubbles.Add(new Vector4(position.X + x_offset, position.Y + zPosition, zPosition, 0.25f));
		}
		ConstrainToTank();
	}
}
