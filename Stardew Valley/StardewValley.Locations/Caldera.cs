using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Tools;
using xTile.Dimensions;

namespace StardewValley.Locations;

public class Caldera : IslandLocation
{
	[XmlIgnore]
	public Texture2D mapBaseTilesheet;

	[XmlElement("visited")]
	public NetBool visited = new NetBool();

	public Caldera()
	{
	}

	public Caldera(string filename, string locationName)
		: base(filename, locationName)
	{
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(visited, "visited");
	}

	protected override void resetLocalState()
	{
		base.resetLocalState();
		if (!visited.Value)
		{
			visited.Value = true;
		}
		if (!Game1.MasterPlayer.hasOrWillReceiveMail("reachedCaldera"))
		{
			Game1.addMailForTomorrow("reachedCaldera", noLetter: true, sendToEveryone: true);
		}
		mapBaseTilesheet = Game1.temporaryContent.Load<Texture2D>(map.TileSheets[0].ImageSource);
		waterColor.Value = Color.White;
		if (Game1.player.mailReceived.Contains("CalderaTreasure"))
		{
			overlayObjects.Remove(new Vector2(25f, 28f));
		}
		else if (!objects.ContainsKey(new Vector2(25f, 28f)))
		{
			Chest chest = new Chest(playerChest: false, "227");
			chest.addItem(ItemRegistry.Create("(O)74"));
			chest.synchronized.Value = false;
			chest.type.Value = "interactive";
			chest.Fragility = 2;
			chest.SetBigCraftableSpriteIndex(227);
			overlayObjects.Add(new Vector2(25f, 28f), chest);
		}
		if (Game1.MasterPlayer.mailReceived.Contains("Farm_Eternal") && !Game1.player.mailReceived.Contains("gotCAMask"))
		{
			temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\critters", new Microsoft.Xna.Framework.Rectangle(15, 333, 13, 12), new Vector2(908.8f, 1792f), flipped: false, 0f, Color.White)
			{
				scale = 4f,
				interval = 99999f,
				totalNumberOfLoops = 99999,
				yPeriodic = true,
				yPeriodicRange = 2f,
				yPeriodicLoopTime = 2500f
			});
		}
	}

	protected override void resetSharedState()
	{
		base.resetSharedState();
		critters = new List<Critter>();
		if (Game1.random.NextDouble() < 0.17)
		{
			addCritter(new CalderaMonkey(new Vector2(12f, 21.3f) * 64f));
		}
		if (Game1.random.NextDouble() < 0.17)
		{
			addCritter(new CalderaMonkey(new Vector2(33f, 21.3f) * 64f));
		}
		if (Game1.random.NextDouble() < 0.17)
		{
			addCritter(new CalderaMonkey(new Vector2(18f, 17.3f) * 64f));
		}
	}

	public override bool CanRefillWateringCanOnTile(int tileX, int tileY)
	{
		return false;
	}

	public override void DayUpdate(int dayOfMonth)
	{
		base.DayUpdate(dayOfMonth);
		if (visited.Value && !Game1.player.hasOrWillReceiveMail("volcanoShortcutUnlocked"))
		{
			Game1.addMailForTomorrow("volcanoShortcutUnlocked", noLetter: true);
		}
	}

	public override void drawWaterTile(SpriteBatch b, int x, int y)
	{
		bool num = y == map.Layers[0].LayerHeight - 1 || !waterTiles[x, y + 1];
		bool topY = y == 0 || !waterTiles[x, y - 1];
		int water_tile_upper_left_x = 0;
		int water_tile_upper_left_y = 320;
		b.Draw(mapBaseTilesheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - (int)((!topY) ? waterPosition : 0f))), new Microsoft.Xna.Framework.Rectangle(water_tile_upper_left_x + waterAnimationIndex * 16, water_tile_upper_left_y + (((x + y) % 2 != 0) ? ((!waterTileFlip) ? 32 : 0) : (waterTileFlip ? 32 : 0)) + (topY ? ((int)waterPosition / 4) : 0), 16, 16 + (topY ? ((int)(0f - waterPosition) / 4) : 0)), waterColor.Value, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.56f);
		if (num)
		{
			b.Draw(mapBaseTilesheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, (y + 1) * 64 - (int)waterPosition)), new Microsoft.Xna.Framework.Rectangle(water_tile_upper_left_x + waterAnimationIndex * 16, water_tile_upper_left_y + (((x + (y + 1)) % 2 != 0) ? ((!waterTileFlip) ? 32 : 0) : (waterTileFlip ? 32 : 0)), 16, 16 - (int)(16f - waterPosition / 4f) - 1), waterColor.Value, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.56f);
		}
	}

	public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
	{
		if (Game1.MasterPlayer.mailReceived.Contains("Farm_Eternal") && !Game1.player.mailReceived.Contains("gotCAMask") && tileLocation.X == 14 && tileLocation.Y == 28)
		{
			Game1.playSound("monkey1");
			who.addItemByMenuIfNecessaryElseHoldUp(ItemRegistry.Create("(H)92"));
			Game1.player.mailReceived.Add("gotCAMask");
		}
		int tileIndexAt = getTileIndexAt(tileLocation, "Buildings");
		if ((uint)(tileIndexAt - 123) <= 1u || (uint)(tileIndexAt - 133) <= 1u || (uint)(tileIndexAt - 156) <= 1u)
		{
			Game1.activeClickableMenu = new ForgeMenu();
			return true;
		}
		return base.checkAction(tileLocation, viewport, who);
	}

	public override bool isActionableTile(int xTile, int yTile, Farmer who)
	{
		if (yTile == 21 && (xTile == 22 || xTile == 23))
		{
			return true;
		}
		if (Game1.MasterPlayer.mailReceived.Contains("Farm_Eternal") && !Game1.player.mailReceived.Contains("gotCAMask") && xTile == 14 && yTile == 28)
		{
			return true;
		}
		return base.isActionableTile(xTile, yTile, who);
	}

	public override void drawBackground(SpriteBatch b)
	{
		base.drawBackground(b);
		DrawParallaxHorizon(b, horizontal_parallax: false);
	}

	public override bool performToolAction(Tool t, int tileX, int tileY)
	{
		if (t is WateringCan && isTileOnMap(new Vector2(tileX, tileY)) && waterTiles[tileX, tileY])
		{
			for (int i = 0; i < 10; i++)
			{
				TemporaryAnimatedSprite s = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1965, 8, 8), new Vector2((float)tileX + 0.5f, (float)tileY + 0.5f) * 64f + new Vector2(Game1.random.Next(-16, 16), Game1.random.Next(-16, 16)), flipped: false, 0.02f, Color.White)
				{
					scale = 3f,
					animationLength = 7,
					totalNumberOfLoops = 10,
					interval = 90f,
					motion = new Vector2((float)Game1.random.Next(-10, 11) / 8f, -3f),
					acceleration = new Vector2(0f, 0.08f),
					delayBeforeAnimationStart = i * 50
				};
				temporarySprites.Add(s);
			}
			for (int i = 0; i < 5; i++)
			{
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), new Vector2(tileX, (float)tileY - 0.5f) * 64f + new Vector2(Game1.random.Next(64), Game1.random.Next(64)), flipped: false, 0.007f, Color.White)
				{
					alpha = 0.75f,
					motion = new Vector2(0f, -1f),
					acceleration = new Vector2(0.002f, 0f),
					interval = 99999f,
					layerDepth = 1f,
					scale = 4f,
					scaleChange = 0.02f,
					rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f,
					delayBeforeAnimationStart = i * 35
				});
			}
			DelayedAction.playSoundAfterDelay("fireball", 200);
			Game1.playSound("steam");
		}
		return base.performToolAction(t, tileX, tileY);
	}
}
