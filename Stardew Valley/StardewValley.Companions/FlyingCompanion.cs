using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;

namespace StardewValley.Companions;

public class FlyingCompanion : Companion
{
	public const int VARIANT_FAIRY = 0;

	public const int VARIANT_PARROT = 1;

	private float flitTimer;

	private Vector2 extraPosition;

	private Vector2 extraPositionMotion;

	private Vector2 extraPositionAcceleration;

	private bool floatup;

	private int flapAnimationLength = 4;

	private int currentSidewaysFlap;

	private bool hasLight = true;

	private int lightID = 301579;

	private NetInt whichSubVariant = new NetInt(-1);

	private NetInt startingYForVariant = new NetInt(0);

	private bool perching;

	private float timeSinceLastZeroLerp;

	private float parrot_squawkTimer;

	private float parrot_squatTimer;

	public FlyingCompanion()
	{
	}

	public FlyingCompanion(int whichVariant, int whichSubVariant = -1)
	{
		base.whichVariant.Value = whichVariant;
		this.whichSubVariant.Value = whichSubVariant;
		if (whichVariant == 1)
		{
			startingYForVariant.Value = 160;
			hasLight = false;
		}
	}

	public override void InitNetFields()
	{
		base.InitNetFields();
		base.NetFields.AddField(whichSubVariant, "whichSubVariant").AddField(startingYForVariant, "startingYForVariant");
	}

	public override void Draw(SpriteBatch b)
	{
		if (base.Owner == null || base.Owner.currentLocation == null || (base.Owner.currentLocation.DisplayName == "Temp" && !Game1.isFestival()))
		{
			return;
		}
		Texture2D texture = Game1.content.Load<Texture2D>("TileSheets\\companions");
		SpriteEffects effect = SpriteEffects.None;
		if (direction.Value == 1)
		{
			effect = SpriteEffects.FlipHorizontally;
		}
		if (perching)
		{
			if (parrot_squatTimer > 0f)
			{
				b.Draw(texture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset + new Vector2(0f, (0f - height) * 4f) + extraPosition), new Rectangle((int)(parrot_squatTimer % 1000f) / 500 * 16 + 128, startingYForVariant, 16, 16), Color.White, 0f, new Vector2(8f, 8f), 4f, effect, _position.Y / 10000f);
			}
			else if (parrot_squawkTimer > 0f)
			{
				b.Draw(texture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset + new Vector2(0f, (0f - height) * 4f) + extraPosition), new Rectangle(160, startingYForVariant, 16, 16), Color.White, 0f, new Vector2(8f, 8f), 4f, effect, _position.Y / 10000f);
			}
			else
			{
				b.Draw(texture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset + new Vector2(0f, (0f - height) * 4f) + extraPosition), new Rectangle(128, startingYForVariant, 16, 16), Color.White, 0f, new Vector2(8f, 8f), 4f, effect, _position.Y / 10000f);
			}
		}
		else
		{
			b.Draw(texture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset + new Vector2(0f, (0f - height) * 4f) + extraPosition), new Rectangle((int)whichSubVariant * 64 + (int)(flitTimer / (float)(500 / flapAnimationLength)) * 16, startingYForVariant, 16, 16), Color.White, 0f, new Vector2(8f, 8f), 4f, effect, _position.Y / 10000f);
			b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset + new Vector2(extraPosition.X, 0f)), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f * Utility.Lerp(1f, 0.8f, Math.Min(height, 1f)), SpriteEffects.None, (_position.Y - 8f) / 10000f - 2E-06f);
		}
	}

	public override void Update(GameTime time, GameLocation location)
	{
		base.Update(time, location);
		height = 32f;
		flitTimer += (float)time.ElapsedGameTime.TotalMilliseconds;
		if (flitTimer > (float)(flapAnimationLength * 125))
		{
			flitTimer = 0f;
			extraPositionMotion = new Vector2((Game1.random.NextDouble() < 0.5) ? 0.1f : (-0.1f), -2f);
			if (extraPositionMotion.X < 0f)
			{
				currentSidewaysFlap--;
			}
			else
			{
				currentSidewaysFlap++;
			}
			if (currentSidewaysFlap < -4 || currentSidewaysFlap > 4)
			{
				extraPositionMotion.X *= -1f;
			}
			extraPositionAcceleration = new Vector2(0f, floatup ? 0.13f : 0.14f);
			if (extraPosition.Y > 8f)
			{
				floatup = true;
			}
			else if (extraPosition.Y < -8f)
			{
				floatup = false;
			}
		}
		if (!perching)
		{
			extraPosition += extraPositionMotion;
			extraPositionMotion += extraPositionAcceleration;
		}
		if (hasLight && location.Equals(Game1.currentLocation))
		{
			Utility.repositionLightSource(lightID, base.Position - new Vector2(0f, height * 4f) + extraPosition);
		}
		if (whichVariant.Value != 1)
		{
			return;
		}
		if (lerp <= 0f)
		{
			timeSinceLastZeroLerp += (float)time.ElapsedGameTime.TotalMilliseconds;
		}
		else
		{
			timeSinceLastZeroLerp = 0f;
		}
		whichSubVariant.Value = ((!(timeSinceLastZeroLerp < 100f)) ? 1 : 0);
		if (timeSinceLastZeroLerp > 2000f)
		{
			if (!perching && (!(Math.Abs(base.OwnerPosition.X - (base.Position.X + extraPosition.X)) < 8f) || !(Math.Abs(base.OwnerPosition.Y - (base.Position.Y + extraPosition.Y)) < 8f)))
			{
				return;
			}
			if (perching && !(base.Owner.Position + new Vector2(32f, 20f)).Equals(base.Position))
			{
				perching = false;
				timeSinceLastZeroLerp = 0f;
				parrot_squatTimer = 0f;
				parrot_squawkTimer = 0f;
				return;
			}
			if (parrot_squawkTimer > 0f)
			{
				parrot_squawkTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
			}
			if (parrot_squatTimer > 0f)
			{
				parrot_squatTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
			}
			perching = true;
			base.Position = base.Owner.Position + new Vector2(32f, 20f);
			extraPosition = Vector2.Zero;
			endPosition = base.Position;
			if (Game1.random.NextDouble() < 0.0005 && parrot_squawkTimer <= 0f)
			{
				parrot_squawkTimer = 500f;
				location.localSound("parrot_squawk");
			}
			else if (Game1.random.NextDouble() < 0.0015 && parrot_squatTimer <= 0f)
			{
				parrot_squatTimer = Game1.random.Next(2, 6) * 1000;
			}
		}
		else
		{
			perching = false;
		}
	}

	public override void InitializeCompanion(Farmer farmer)
	{
		base.InitializeCompanion(farmer);
		if (hasLight)
		{
			lightID = Game1.random.Next();
			Game1.currentLightSources.Add(new LightSource(1, base.Position, 2f, Color.Black, lightID, LightSource.LightContext.None, 0L));
		}
		if ((int)whichSubVariant == -1)
		{
			Random r = Utility.CreateRandom(farmer.uniqueMultiplayerID.Value);
			whichSubVariant.Value = r.Next(4);
			if ((int)whichVariant == 0 && r.NextDouble() < 0.5)
			{
				startingYForVariant.Value += 176;
			}
		}
	}

	public override void CleanupCompanion()
	{
		base.CleanupCompanion();
		if (hasLight)
		{
			Utility.removeLightSource(lightID);
		}
	}

	public override void OnOwnerWarp()
	{
		base.OnOwnerWarp();
		extraPosition = Vector2.Zero;
		extraPositionMotion = Vector2.Zero;
		extraPositionAcceleration = Vector2.Zero;
		if (hasLight)
		{
			lightID = Game1.random.Next();
			Game1.currentLightSources.Add(new LightSource(1, base.Position, 2f, Color.Black, lightID, LightSource.LightContext.None, 0L));
		}
	}

	public override void Hop(float amount)
	{
	}
}
