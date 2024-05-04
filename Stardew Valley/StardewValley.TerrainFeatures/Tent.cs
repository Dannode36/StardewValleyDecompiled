using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;

namespace StardewValley.TerrainFeatures;

public class Tent : LargeTerrainFeature
{
	public readonly NetInt health = new NetInt(5);

	private int invincTimer;

	private Vector2 shakeOffset;

	private bool goingToSleep;

	public static Vector2 lastTentTouchedByPlayer = Vector2.Zero;

	public Tent()
		: base(needsTick: true)
	{
		isDestroyedByNPCTrample = true;
	}

	public override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(health, "health");
	}

	public Tent(Vector2 tileLocation)
		: base(needsTick: true)
	{
		Tile = tileLocation;
		isDestroyedByNPCTrample = true;
	}

	public override Rectangle getBoundingBox()
	{
		Vector2 tileLocation = Tile;
		return new Rectangle((int)(tileLocation.X - 1f) * 64, (int)(tileLocation.Y - 1f) * 64, 192, 128);
	}

	public override bool performToolAction(Tool t, int damage, Vector2 tileLocation)
	{
		if (invincTimer <= 0)
		{
			health.Value--;
			invincTimer = 400;
			Game1.playSound("weed_cut");
		}
		return base.performToolAction(t, damage, tileLocation);
	}

	public override void dayUpdate()
	{
		health.Value = 0;
		Game1.displayFarmer = true;
		base.dayUpdate();
	}

	public override bool performUseAction(Vector2 tileLocation)
	{
		Vector2 tilePosition = Tile;
		Vector2 playerGrab = Game1.player.GetGrabTile();
		if ((playerGrab == tilePosition || (playerGrab.X == tilePosition.X && playerGrab.Y >= tilePosition.Y)) && !Game1.newDay && Game1.shouldTimePass() && Game1.player.hasMoved && !Game1.player.passedOut)
		{
			lastTentTouchedByPlayer = tilePosition;
			Location.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:FarmHouse_Bed_GoToSleep"), Location.createYesNoResponses(), "SleepTent", null);
		}
		return base.performUseAction(tileLocation);
	}

	public override void onDestroy()
	{
		GameLocation location = Location;
		Vector2 tilePosition = Tile;
		Game1.playSound("cut");
		Utility.addDirtPuffs(location, (int)tilePosition.X - 1, (int)tilePosition.Y - 1, 3, 2, 3);
		for (int i = 0; i < 16; i++)
		{
			location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Rectangle(112 + Game1.random.Next(4) * 8, 248, 8, 8), 9999f, 1, 1, Utility.getRandomPositionInThisRectangle(getBoundingBox(), Game1.random), flicker: false, flipped: false, tilePosition.Y * 64f / 10000f, 0.02f, Color.White, 4f, 0f, 0f, 0f)
			{
				motion = new Vector2(Game1.random.Next(-1, 2), -5f),
				acceleration = new Vector2(0f, 0.16f)
			});
		}
	}

	public override bool tickUpdate(GameTime time)
	{
		if (invincTimer > 0)
		{
			invincTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
			shakeOffset = new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2));
			if (invincTimer <= 0)
			{
				shakeOffset = Vector2.Zero;
			}
		}
		if ((int)health <= 0 && !goingToSleep)
		{
			onDestroy();
			return true;
		}
		return base.tickUpdate(time);
	}

	public override void draw(SpriteBatch spriteBatch)
	{
		Vector2 tileLocation = Tile;
		spriteBatch.Draw(Game1.mouseCursors_1_6, Game1.GlobalToLocal(tileLocation * 64f + new Vector2(-2f, -1f) * 64f), new Rectangle(48, 208, 64, 48), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0001f);
		spriteBatch.Draw(Game1.mouseCursors_1_6, Game1.GlobalToLocal(tileLocation * 64f + new Vector2(-1f, -3f) * 64f) + shakeOffset, new Rectangle(0, 192, 48, 64), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, tileLocation.Y * 64f / 10000f);
	}
}
