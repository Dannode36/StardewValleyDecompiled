using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Constants;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Tools;

namespace StardewValley.Objects;

public class BreakableContainer : Object
{
	public const string barrelId = "118";

	public const string frostBarrelId = "120";

	public const string darkBarrelId = "122";

	public const string desertBarrelId = "124";

	public const string volcanoBarrelId = "174";

	public const string waterBarrelId = "262";

	[XmlElement("debris")]
	private readonly NetInt debris = new NetInt();

	private new int shakeTimer;

	[XmlElement("health")]
	private new readonly NetInt health = new NetInt();

	[XmlElement("hitSound")]
	private readonly NetString hitSound = new NetString();

	[XmlElement("breakSound")]
	private readonly NetString breakSound = new NetString();

	[XmlElement("breakDebrisSource")]
	private readonly NetRectangle breakDebrisSource = new NetRectangle();

	[XmlElement("breakDebrisSource2")]
	private readonly NetRectangle breakDebrisSource2 = new NetRectangle();

	/// <inheritdoc />
	public override string TypeDefinitionId => "(BC)";

	/// <inheritdoc />
	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(debris, "debris").AddField(health, "health").AddField(hitSound, "hitSound")
			.AddField(breakSound, "breakSound")
			.AddField(breakDebrisSource, "breakDebrisSource")
			.AddField(breakDebrisSource2, "breakDebrisSource2");
	}

	public BreakableContainer()
	{
	}

	public BreakableContainer(Vector2 tile, string itemId, int health = 3, int debrisType = 12, string hitSound = "woodWhack", string breakSound = "barrelBreak")
		: base(tile, itemId)
	{
		this.health.Value = health;
		debris.Value = debrisType;
		this.hitSound.Value = hitSound;
		this.breakSound.Value = breakSound;
		breakDebrisSource.Value = new Rectangle(598, 1275, 13, 4);
		breakDebrisSource2.Value = new Rectangle(611, 1275, 10, 4);
	}

	/// <summary>Get a barrel to place in the mines or Skull Cavern.</summary>
	/// <param name="tile">The tile position at which it'll be placed.</param>
	/// <param name="mine">The mine level.</param>
	public static BreakableContainer GetBarrelForMines(Vector2 tile, MineShaft mine)
	{
		int mineArea = mine.getMineArea();
		string itemId = ((mine.GetAdditionalDifficulty() > 0) ? (((mineArea == 0 || mineArea == 10) && !mine.isDarkArea()) ? "262" : "118") : (mineArea switch
		{
			40 => "120", 
			80 => "122", 
			121 => "124", 
			_ => "118", 
		}));
		BreakableContainer barrel = new BreakableContainer(tile, itemId);
		if (Game1.random.NextBool())
		{
			barrel.showNextIndex.Value = true;
		}
		return barrel;
	}

	/// <summary>Get a barrel to place in the Volcano Dungeon.</summary>
	/// <param name="tile">The tile position at which it'll be placed.</param>
	public static BreakableContainer GetBarrelForVolcanoDungeon(Vector2 tile)
	{
		BreakableContainer barrel = new BreakableContainer(tile, "174", 4, 14, "clank", "boulderBreak");
		if (Game1.random.NextBool())
		{
			barrel.showNextIndex.Value = true;
		}
		return barrel;
	}

	public override bool performToolAction(Tool t)
	{
		GameLocation location = Location;
		if (location == null)
		{
			return false;
		}
		if (t != null && t.isHeavyHitter())
		{
			health.Value--;
			if (t is MeleeWeapon weapon && (int)weapon.type == 2)
			{
				health.Value--;
			}
			if ((int)health <= 0)
			{
				if (breakSound != null)
				{
					playNearbySoundAll(breakSound);
				}
				releaseContents(t.getLastFarmerToUse());
				location.objects.Remove(tileLocation.Value);
				int numDebris = Game1.random.Next(4, 12);
				Color chipColor = GetChipColor();
				for (int i = 0; i < numDebris; i++)
				{
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", Game1.random.NextBool() ? breakDebrisSource.Value : breakDebrisSource2.Value, 999f, 1, 0, tileLocation.Value * 64f + new Vector2(32f, 32f), flicker: false, Game1.random.NextBool(), (tileLocation.Y * 64f + 32f) / 10000f, 0.01f, chipColor, 4f, 0f, (float)Game1.random.Next(-5, 6) * (float)Math.PI / 8f, (float)Game1.random.Next(-5, 6) * (float)Math.PI / 64f)
					{
						motion = new Vector2((float)Game1.random.Next(-30, 31) / 10f, Game1.random.Next(-10, -7)),
						acceleration = new Vector2(0f, 0.3f)
					});
				}
			}
			else if (hitSound != null)
			{
				shakeTimer = 300;
				playNearbySoundAll(hitSound);
				Color? debrisColor = ((base.ItemId == "120") ? new Color?(Color.White) : null);
				Game1.createRadialDebris(location, debris.Value, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(4, 7), resource: false, -1, item: false, debrisColor);
			}
		}
		return false;
	}

	public override bool onExplosion(Farmer who)
	{
		if (who == null)
		{
			who = Game1.player;
		}
		GameLocation location = Location;
		if (location == null)
		{
			return true;
		}
		releaseContents(who);
		int numDebris = Game1.random.Next(4, 12);
		Color chipColor = GetChipColor();
		for (int i = 0; i < numDebris; i++)
		{
			Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", Game1.random.NextBool() ? breakDebrisSource.Value : breakDebrisSource2.Value, 999f, 1, 0, tileLocation.Value * 64f + new Vector2(32f, 32f), flicker: false, Game1.random.NextBool(), (tileLocation.Y * 64f + 32f) / 10000f, 0.01f, chipColor, 4f, 0f, (float)Game1.random.Next(-5, 6) * (float)Math.PI / 8f, (float)Game1.random.Next(-5, 6) * (float)Math.PI / 64f)
			{
				motion = new Vector2((float)Game1.random.Next(-30, 31) / 10f, Game1.random.Next(-10, -7)),
				acceleration = new Vector2(0f, 0.3f)
			});
		}
		return true;
	}

	/// <summary>Get the color of cosmetic chip debris when breaking this container.</summary>
	public Color GetChipColor()
	{
		return base.ItemId switch
		{
			"120" => Color.White, 
			"122" => new Color(109, 122, 80), 
			"174" => new Color(107, 76, 83), 
			_ => new Color(130, 80, 30), 
		};
	}

	public void releaseContents(Farmer who)
	{
		GameLocation location = Location;
		if (location == null)
		{
			return;
		}
		Random r = Utility.CreateRandom(tileLocation.X, (double)tileLocation.Y * 10000.0, Game1.stats.DaysPlayed, (location as MineShaft)?.mineLevel ?? 0);
		int x = (int)tileLocation.X;
		int y = (int)tileLocation.Y;
		int mineLevel = -1;
		int difficultyLevel = 0;
		if (location is MineShaft mine)
		{
			mineLevel = mine.mineLevel;
			if (mine.isContainerPlatform(x, y))
			{
				mine.updateMineLevelData(0, -1);
			}
			difficultyLevel = mine.GetAdditionalDifficulty();
		}
		if (r.NextDouble() < 0.2)
		{
			if (r.NextDouble() < 0.1)
			{
				Game1.createMultipleItemDebris(Utility.getRaccoonSeedForCurrentTimeOfYear(who, r), new Vector2(x, y) * 64f + new Vector2(32f), -1, location);
			}
			return;
		}
		if (location is MineShaft mineShaft)
		{
			if (mineShaft.mineLevel > 120 && !mineShaft.isSideBranch())
			{
				int floor = mineShaft.mineLevel - 121;
				if (Utility.GetDayOfPassiveFestival("DesertFestival") > 0)
				{
					float chance = (float)(floor + (int)Game1.player.team.calicoEggSkullCavernRating * 2) * 0.003f;
					if (chance > 0.33f)
					{
						chance = 0.33f;
					}
					if (r.NextBool(chance))
					{
						Game1.createMultipleObjectDebris("CalicoEgg", x, y, r.Next(1, 4), who.UniqueMultiplayerID, location);
					}
				}
			}
			int effectiveMineLevel = mineShaft.mineLevel;
			if (mineShaft.mineLevel == 77377)
			{
				effectiveMineLevel = 5000;
			}
			Trinket.TrySpawnTrinket(location, null, new Vector2(x, y) * 64f + new Vector2(32f), 1.0 + (double)effectiveMineLevel * 0.001);
		}
		if (r.NextDouble() <= 0.05 && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS"))
		{
			Game1.createMultipleObjectDebris("(O)890", x, y, r.Next(1, 3), who.UniqueMultiplayerID, location);
		}
		if (Utility.tryRollMysteryBox(0.0081 + Game1.player.team.AverageDailyLuck() / 15.0, r))
		{
			Game1.createItemDebris(ItemRegistry.Create((Game1.player.stats.Get(StatKeys.Mastery(2)) != 0) ? "(O)GoldenMysteryBox" : "(O)MysteryBox"), new Vector2(x, y) * 64f + new Vector2(32f), -1, location);
		}
		Utility.trySpawnRareObject(who, new Vector2(x, y) * 64f, location, 1.5, 1.0, -1, r);
		if (difficultyLevel > 0)
		{
			if (!(r.NextDouble() < 0.15))
			{
				if (r.NextDouble() < 0.008)
				{
					Game1.createMultipleObjectDebris("(O)858", x, y, 1, location);
				}
				if (r.NextDouble() < 0.01)
				{
					Game1.createItemDebris(ItemRegistry.Create("(BC)71"), new Vector2(x, y) * 64f + new Vector2(32f), 0);
				}
				if (r.NextDouble() < 0.01)
				{
					Game1.createMultipleObjectDebris(r.Choose("(O)918", "(O)919", "(O)920"), x, y, 1, location);
				}
				if (r.NextDouble() < 0.01)
				{
					Game1.createMultipleObjectDebris("(O)386", x, y, r.Next(1, 4), location);
				}
				switch (r.Next(17))
				{
				case 0:
					Game1.createMultipleObjectDebris("(O)382", x, y, r.Next(1, 3), location);
					break;
				case 1:
					Game1.createMultipleObjectDebris("(O)380", x, y, r.Next(1, 4), location);
					break;
				case 2:
					Game1.createMultipleObjectDebris("(O)62", x, y, 1, location);
					break;
				case 3:
					Game1.createMultipleObjectDebris("(O)390", x, y, r.Next(2, 6), location);
					break;
				case 4:
					Game1.createMultipleObjectDebris("(O)80", x, y, r.Next(2, 3), location);
					break;
				case 5:
					Game1.createMultipleObjectDebris((who.timesReachedMineBottom > 0) ? "(O)84" : r.Choose("(O)92", "(O)370"), x, y, r.Choose(2, 3), location);
					break;
				case 6:
					Game1.createMultipleObjectDebris("(O)70", x, y, 1, location);
					break;
				case 7:
					Game1.createMultipleObjectDebris("(O)390", x, y, r.Next(2, 6), location);
					break;
				case 8:
					Game1.createMultipleObjectDebris("(O)" + r.Next(218, 245), x, y, 1, location);
					break;
				case 9:
					Game1.createMultipleObjectDebris((Game1.whichFarm == 6) ? "(O)920" : "(O)749", x, y, 1, location);
					break;
				case 10:
					Game1.createMultipleObjectDebris("(O)286", x, y, 1, location);
					break;
				case 11:
					Game1.createMultipleObjectDebris("(O)378", x, y, r.Next(1, 4), location);
					break;
				case 12:
					Game1.createMultipleObjectDebris("(O)384", x, y, r.Next(1, 4), location);
					break;
				case 13:
					Game1.createMultipleObjectDebris("(O)287", x, y, 1, location);
					break;
				}
			}
			return;
		}
		switch (base.ItemId)
		{
		case "118":
			if (r.NextDouble() < 0.65)
			{
				if (r.NextDouble() < 0.8)
				{
					switch (r.Next(9))
					{
					case 0:
						Game1.createMultipleObjectDebris("(O)382", x, y, r.Next(1, 3), location);
						break;
					case 1:
						Game1.createMultipleObjectDebris("(O)378", x, y, r.Next(1, 4), location);
						break;
					case 3:
						Game1.createMultipleObjectDebris("(O)390", x, y, r.Next(2, 6), location);
						break;
					case 4:
						Game1.createMultipleObjectDebris("(O)388", x, y, r.Next(2, 3), location);
						break;
					case 5:
						Game1.createMultipleObjectDebris((who.timesReachedMineBottom > 0) ? "(O)80" : r.Choose("(O)92", "(O)370"), x, y, r.Choose(2, 3), location);
						break;
					case 6:
						Game1.createMultipleObjectDebris("(O)388", x, y, r.Next(2, 6), location);
						break;
					case 7:
						Game1.createMultipleObjectDebris("(O)390", x, y, r.Next(2, 6), location);
						break;
					case 8:
						Game1.createMultipleObjectDebris("(O)770", x, y, 1, location);
						break;
					case 2:
						break;
					}
				}
				else
				{
					switch (r.Next(4))
					{
					case 0:
						Game1.createMultipleObjectDebris("(O)78", x, y, r.Next(1, 3), location);
						break;
					case 1:
						Game1.createMultipleObjectDebris("(O)78", x, y, r.Next(1, 3), location);
						break;
					case 2:
						Game1.createMultipleObjectDebris("(O)78", x, y, r.Next(1, 3), location);
						break;
					case 3:
						Game1.createMultipleObjectDebris("(O)535", x, y, r.Next(1, 3), location);
						break;
					}
				}
			}
			else if (r.NextDouble() < 0.4)
			{
				switch (r.Next(5))
				{
				case 0:
					Game1.createMultipleObjectDebris("(O)66", x, y, 1, location);
					break;
				case 1:
					Game1.createMultipleObjectDebris("(O)68", x, y, 1, location);
					break;
				case 2:
					Game1.createMultipleObjectDebris("(O)709", x, y, 1, location);
					break;
				case 3:
					Game1.createMultipleObjectDebris("(O)535", x, y, 1, location);
					break;
				case 4:
					Game1.createItemDebris(MineShaft.getSpecialItemForThisMineLevel(mineLevel, x, y), new Vector2(x, y) * 64f + new Vector2(32f, 32f), r.Next(4), location);
					break;
				}
			}
			break;
		case "120":
			if (r.NextDouble() < 0.65)
			{
				if (r.NextDouble() < 0.8)
				{
					switch (r.Next(9))
					{
					case 0:
						Game1.createMultipleObjectDebris("(O)382", x, y, r.Next(1, 3), location);
						break;
					case 1:
						Game1.createMultipleObjectDebris("(O)380", x, y, r.Next(1, 4), location);
						break;
					case 3:
						Game1.createMultipleObjectDebris("(O)378", x, y, r.Next(2, 6), location);
						break;
					case 4:
						Game1.createMultipleObjectDebris("(O)388", x, y, r.Next(2, 6), location);
						break;
					case 5:
						Game1.createMultipleObjectDebris((who.timesReachedMineBottom > 0) ? "(O)84" : r.Choose("(O)92", "(O)371"), x, y, r.Choose(2, 3), location);
						break;
					case 6:
						Game1.createMultipleObjectDebris("(O)390", x, y, r.Next(2, 4), location);
						break;
					case 7:
						Game1.createMultipleObjectDebris("(O)390", x, y, r.Next(2, 6), location);
						break;
					case 8:
						Game1.createMultipleObjectDebris("(O)770", x, y, 1, location);
						break;
					case 2:
						break;
					}
				}
				else
				{
					switch (r.Next(4))
					{
					case 0:
						Game1.createMultipleObjectDebris("(O)78", x, y, r.Next(1, 3), location);
						break;
					case 1:
						Game1.createMultipleObjectDebris("(O)536", x, y, r.Next(1, 3), location);
						break;
					case 2:
						Game1.createMultipleObjectDebris("(O)78", x, y, r.Next(1, 3), location);
						break;
					case 3:
						Game1.createMultipleObjectDebris("(O)78", x, y, r.Next(1, 3), location);
						break;
					}
				}
			}
			else if (r.NextDouble() < 0.4)
			{
				switch (r.Next(5))
				{
				case 0:
					Game1.createMultipleObjectDebris("(O)62", x, y, 1, location);
					break;
				case 1:
					Game1.createMultipleObjectDebris("(O)70", x, y, 1, location);
					break;
				case 2:
					Game1.createMultipleObjectDebris("(O)709", x, y, r.Next(1, 4), location);
					break;
				case 3:
					Game1.createMultipleObjectDebris("(O)536", x, y, 1, location);
					break;
				case 4:
					Game1.createItemDebris(MineShaft.getSpecialItemForThisMineLevel(mineLevel, x, y), new Vector2(x, y) * 64f + new Vector2(32f, 32f), r.Next(4), location);
					break;
				}
			}
			break;
		case "124":
		case "122":
			if (r.NextDouble() < 0.65)
			{
				if (r.NextDouble() < 0.8)
				{
					switch (r.Next(8))
					{
					case 0:
						Game1.createMultipleObjectDebris("(O)382", x, y, r.Next(1, 3), location);
						break;
					case 1:
						Game1.createMultipleObjectDebris("(O)384", x, y, r.Next(1, 4), location);
						break;
					case 3:
						Game1.createMultipleObjectDebris("(O)380", x, y, r.Next(2, 6), location);
						break;
					case 4:
						Game1.createMultipleObjectDebris("(O)378", x, y, r.Next(2, 6), location);
						break;
					case 5:
						Game1.createMultipleObjectDebris("(O)390", x, y, r.Next(2, 6), location);
						break;
					case 6:
						Game1.createMultipleObjectDebris("(O)388", x, y, r.Next(2, 6), location);
						break;
					case 7:
						Game1.createMultipleObjectDebris("(O)881", x, y, r.Next(2, 6), location);
						break;
					case 2:
						break;
					}
				}
				else
				{
					switch (r.Next(4))
					{
					case 0:
						Game1.createMultipleObjectDebris("(O)78", x, y, r.Next(1, 3), location);
						break;
					case 1:
						Game1.createMultipleObjectDebris("(O)537", x, y, r.Next(1, 3), location);
						break;
					case 2:
						Game1.createMultipleObjectDebris((who.timesReachedMineBottom > 0) ? "(O)82" : "(O)78", x, y, r.Next(1, 3), location);
						break;
					case 3:
						Game1.createMultipleObjectDebris("(O)78", x, y, r.Next(1, 3), location);
						break;
					}
				}
			}
			else if (r.NextDouble() < 0.4)
			{
				switch (r.Next(6))
				{
				case 0:
					Game1.createMultipleObjectDebris("(O)60", x, y, 1, location);
					break;
				case 1:
					Game1.createMultipleObjectDebris("(O)64", x, y, 1, location);
					break;
				case 2:
					Game1.createMultipleObjectDebris("(O)709", x, y, r.Next(1, 4), location);
					break;
				case 3:
					Game1.createMultipleObjectDebris("(O)749", x, y, 1, location);
					break;
				case 4:
					Game1.createItemDebris(MineShaft.getSpecialItemForThisMineLevel(mineLevel, x, y), new Vector2(x, y) * 64f + new Vector2(32f, 32f), r.Next(4), location);
					break;
				case 5:
					Game1.createMultipleObjectDebris("(O)688", x, y, 1, location);
					break;
				}
			}
			break;
		case "174":
			if (r.NextDouble() < 0.1)
			{
				Game1.player.team.RequestLimitedNutDrops("VolcanoBarrel", location, x * 64, y * 64, 5);
			}
			if (location is VolcanoDungeon dungeon && (int)dungeon.level == 5 && x == 34)
			{
				Item item = ItemRegistry.Create("(O)851");
				item.Quality = 2;
				Game1.createItemDebris(item, new Vector2(x, y) * 64f, 1);
			}
			else if (r.NextDouble() < 0.75)
			{
				if (r.NextDouble() < 0.8)
				{
					switch (r.Next(7))
					{
					case 0:
						Game1.createMultipleObjectDebris("(O)382", x, y, r.Next(1, 3), location);
						break;
					case 1:
						Game1.createMultipleObjectDebris("(O)384", x, y, r.Next(1, 4), location);
						break;
					case 2:
						location.characters.Add(new DwarvishSentry(new Vector2(x, y) * 64f));
						break;
					case 3:
						Game1.createMultipleObjectDebris("(O)380", x, y, r.Next(2, 6), location);
						break;
					case 4:
						Game1.createMultipleObjectDebris("(O)378", x, y, r.Next(2, 6), location);
						break;
					case 5:
						Game1.createMultipleObjectDebris("66", x, y, 1, location);
						break;
					case 6:
						Game1.createMultipleObjectDebris("(O)709", x, y, r.Next(2, 6), location);
						break;
					}
				}
				else
				{
					switch (r.Next(5))
					{
					case 0:
						Game1.createMultipleObjectDebris("(O)78", x, y, r.Next(1, 3), location);
						break;
					case 1:
						Game1.createMultipleObjectDebris("(O)749", x, y, r.Next(1, 3), location);
						break;
					case 2:
						Game1.createMultipleObjectDebris("(O)60", x, y, 1, location);
						break;
					case 3:
						Game1.createMultipleObjectDebris("(O)64", x, y, 1, location);
						break;
					case 4:
						Game1.createMultipleObjectDebris("(O)68", x, y, 1, location);
						break;
					}
				}
			}
			else if (r.NextDouble() < 0.4)
			{
				switch (r.Next(9))
				{
				case 0:
					Game1.createMultipleObjectDebris("(O)72", x, y, 1, location);
					break;
				case 1:
					Game1.createMultipleObjectDebris("(O)831", x, y, r.Next(1, 4), location);
					break;
				case 2:
					Game1.createMultipleObjectDebris("(O)833", x, y, r.Next(1, 3), location);
					break;
				case 3:
					Game1.createMultipleObjectDebris("(O)749", x, y, 1, location);
					break;
				case 4:
					Game1.createMultipleObjectDebris("(O)386", x, y, 1, location);
					break;
				case 5:
					Game1.createMultipleObjectDebris("(O)848", x, y, 1, location);
					break;
				case 6:
					Game1.createMultipleObjectDebris("(O)856", x, y, 1, location);
					break;
				case 7:
					Game1.createMultipleObjectDebris("(O)886", x, y, 1, location);
					break;
				case 8:
					Game1.createMultipleObjectDebris("(O)688", x, y, 1, location);
					break;
				}
			}
			else
			{
				location.characters.Add(new DwarvishSentry(new Vector2(x, y) * 64f));
			}
			break;
		}
	}

	public override void updateWhenCurrentLocation(GameTime time)
	{
		if (shakeTimer > 0)
		{
			shakeTimer -= time.ElapsedGameTime.Milliseconds;
		}
	}

	public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
	{
		Vector2 scaleFactor = getScale();
		scaleFactor *= 4f;
		Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64));
		Rectangle destination = new Rectangle((int)(position.X - scaleFactor.X / 2f), (int)(position.Y - scaleFactor.Y / 2f), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
		if (shakeTimer > 0)
		{
			int intensity = shakeTimer / 100 + 1;
			destination.X += Game1.random.Next(-intensity, intensity + 1);
			destination.Y += Game1.random.Next(-intensity, intensity + 1);
		}
		ParsedItemData data = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
		spriteBatch.Draw(data.GetTexture(), destination, data.GetSourceRect(showNextIndex ? 1 : 0), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, Math.Max(0f, (float)((y + 1) * 64 - 1) / 10000f));
	}
}
