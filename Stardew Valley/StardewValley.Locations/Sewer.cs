using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using xTile.Dimensions;

namespace StardewValley.Locations;

public class Sewer : GameLocation
{
	public const float steamZoom = 4f;

	public const float steamYMotionPerMillisecond = 0.1f;

	private Texture2D steamAnimation;

	private Vector2 steamPosition;

	private Color steamColor = new Color(200, 255, 200);

	public Sewer()
	{
	}

	public Sewer(string map, string name)
		: base(map, name)
	{
		waterColor.Value = Color.LimeGreen;
	}

	public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
	{
		base.drawAboveAlwaysFrontLayer(b);
		for (float x = -1000f * Game1.options.zoomLevel + steamPosition.X; x < (float)Game1.graphics.GraphicsDevice.Viewport.Width + 256f; x += 256f)
		{
			for (float y = -256f + steamPosition.Y; y < (float)(Game1.graphics.GraphicsDevice.Viewport.Height + 128); y += 256f)
			{
				b.Draw(steamAnimation, new Vector2(x, y), new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), steamColor * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			}
		}
	}

	public override void UpdateWhenCurrentLocation(GameTime time)
	{
		base.UpdateWhenCurrentLocation(time);
		steamPosition.Y -= (float)time.ElapsedGameTime.Milliseconds * 0.1f;
		steamPosition.Y %= -256f;
		steamPosition -= Game1.getMostRecentViewportMotion();
		if (Game1.random.NextDouble() < 0.001)
		{
			localSound("cavedrip");
		}
	}

	public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
	{
		switch (getTileIndexAt(tileLocation, "Buildings"))
		{
		case 21:
			Game1.warpFarmer("Town", 35, 97, 2);
			DelayedAction.playSoundAfterDelay("stairsdown", 250);
			return true;
		case 84:
			Utility.TryOpenShopMenu("ShadowShop", null, playOpenSound: true);
			return true;
		default:
			return base.checkAction(tileLocation, viewport, who);
		}
	}

	protected override void resetSharedState()
	{
		base.resetSharedState();
		waterColor.Value = Color.LimeGreen * 0.75f;
	}

	protected override void resetLocalState()
	{
		base.resetLocalState();
		steamPosition = new Vector2(0f, 0f);
		steamAnimation = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\steamAnimation");
		Game1.ambientLight = new Color(250, 140, 160);
	}

	public override void MakeMapModifications(bool force = false)
	{
		base.MakeMapModifications(force);
		if (Game1.getCharacterFromName("Krobus").isMarried())
		{
			setMapTileIndex(31, 17, 84, "Buildings", 1);
			setMapTileIndex(31, 16, 1, "Front", 1);
		}
		else
		{
			setMapTileIndex(31, 17, -1, "Buildings");
			setMapTileIndex(31, 16, -1, "Front");
		}
	}
}
