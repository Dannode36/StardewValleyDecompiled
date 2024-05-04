using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Extensions;
using StardewValley.Tools;

namespace StardewValley.TerrainFeatures;

public class CosmeticPlant : Grass
{
	[XmlElement("flipped")]
	public readonly NetBool flipped = new NetBool();

	[XmlElement("xOffset")]
	private readonly NetInt xOffset = new NetInt();

	[XmlElement("yOffset")]
	private readonly NetInt yOffset = new NetInt();

	public CosmeticPlant()
	{
	}

	public CosmeticPlant(int which)
		: base(which, 1)
	{
		flipped.Value = Game1.random.NextBool();
	}

	public override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(flipped, "flipped").AddField(xOffset, "xOffset").AddField(yOffset, "yOffset");
	}

	public override Rectangle getBoundingBox()
	{
		Vector2 tileLocation = Tile;
		return new Rectangle((int)(tileLocation.X * 64f + 16f), (int)((tileLocation.Y + 1f) * 64f - 8f - 4f), 8, 8);
	}

	public override string textureName()
	{
		return "TerrainFeatures\\upperCavePlants";
	}

	public override void loadSprite()
	{
		xOffset.Value = Game1.random.Next(-2, 3) * 4;
		yOffset.Value = Game1.random.Next(-2, 1) * 4;
	}

	public override bool performToolAction(Tool t, int explosion, Vector2 tileLocation)
	{
		GameLocation location = Location;
		if ((t is MeleeWeapon weapon && (int)weapon.type != 2) || explosion > 0)
		{
			shake((float)Math.PI * 3f / 32f, (float)Math.PI / 40f, Game1.random.NextBool());
			int numberOfWeedsToDestroy = ((explosion > 0) ? Math.Max(1, explosion + 2 - Game1.random.Next(2)) : (((int)t.upgradeLevel == 3) ? 3 : ((int)t.upgradeLevel + 1)));
			Game1.createRadialDebris(location, textureName(), new Rectangle(grassType.Value * 16, 6, 7, 6), (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(6, 14));
			numberOfWeeds.Value = (int)numberOfWeeds - numberOfWeedsToDestroy;
			if ((int)numberOfWeeds <= 0)
			{
				Random grassRandom = Utility.CreateRandom(Game1.uniqueIDForThisGame, (double)tileLocation.X * 7.0, (double)tileLocation.Y * 11.0, Game1.CurrentMineLevel, Game1.player.timesReachedMineBottom);
				if (grassRandom.NextDouble() < 0.005)
				{
					Game1.createObjectDebris("(O)114", (int)tileLocation.X, (int)tileLocation.Y, -1, 0, 1f, location);
				}
				else if (grassRandom.NextDouble() < 0.01)
				{
					Game1.createDebris(4, (int)tileLocation.X, (int)tileLocation.Y, grassRandom.Next(1, 2), location);
				}
				else if (grassRandom.NextDouble() < 0.02)
				{
					Game1.createDebris(92, (int)tileLocation.X, (int)tileLocation.Y, grassRandom.Next(2, 4), location);
				}
				return true;
			}
		}
		return false;
	}

	public override void draw(SpriteBatch spriteBatch)
	{
		Vector2 tileLocation = Tile;
		spriteBatch.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f) + new Vector2(32 + (int)xOffset, 60 + (int)yOffset)), new Rectangle(grassType.Value * 16, 0, 16, 24), Color.White, shakeRotation, new Vector2(8f, 23f), 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, ((float)(getBoundingBox().Y - 4) + tileLocation.X / 900f + 0.01f) / 10000f);
	}
}
