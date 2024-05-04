using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Constants;
using StardewValley.Enchantments;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Objects;

namespace StardewValley.Tools;

public class Pan : Tool
{
	[XmlIgnore]
	private readonly NetEvent0 finishEvent = new NetEvent0();

	public Pan()
		: base("Copper Pan", 1, 12, 12, stackable: false)
	{
	}

	public Pan(int upgradeLevel)
		: base("Copper Pan", upgradeLevel, 12, 12, stackable: false)
	{
	}

	/// <inheritdoc />
	protected override Item GetOneNew()
	{
		if (upgradeLevel.Value == -1)
		{
			base.UpgradeLevel = 1;
		}
		return new Pan(base.UpgradeLevel);
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(finishEvent, "finishEvent");
		finishEvent.onEvent += doFinish;
	}

	public override bool beginUsing(GameLocation location, int x, int y, Farmer who)
	{
		if (upgradeLevel.Value <= 0)
		{
			base.UpgradeLevel = 1;
		}
		base.CurrentParentTileIndex = 12;
		base.IndexOfMenuItemView = 12;
		int reach = 4;
		if (hasEnchantmentOfType<ReachingToolEnchantment>())
		{
			reach++;
		}
		bool overrideCheck = false;
		Rectangle orePanRect = new Rectangle(location.orePanPoint.X * 64 - (int)(64f * ((float)reach / 2f)), location.orePanPoint.Y * 64 - (int)(64f * ((float)reach / 2f)), 64 * reach, 64 * reach);
		Point playerPixel = who.StandingPixel;
		if (orePanRect.Contains(x, y) && Utility.distance(playerPixel.X, orePanRect.Center.X, playerPixel.Y, orePanRect.Center.Y) <= (float)(reach * 64))
		{
			overrideCheck = true;
		}
		who.lastClick = Vector2.Zero;
		x = (int)who.GetToolLocation().X;
		y = (int)who.GetToolLocation().Y;
		who.lastClick = new Vector2(x, y);
		if (location.orePanPoint != null && !location.orePanPoint.Equals(Point.Zero))
		{
			Rectangle panRect = who.GetBoundingBox();
			if (overrideCheck || panRect.Intersects(orePanRect))
			{
				who.faceDirection(2);
				who.FarmerSprite.animateOnce(303, 50f, 4);
				return true;
			}
		}
		who.forceCanMove();
		return true;
	}

	public static void playSlosh(Farmer who)
	{
		who.playNearbySoundLocal("slosh");
	}

	public override void tickUpdate(GameTime time, Farmer who)
	{
		lastUser = who;
		base.tickUpdate(time, who);
		finishEvent.Poll();
	}

	public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
	{
		base.DoFunction(location, x, y, power, who);
		Vector2 toolLocation = who.GetToolLocation();
		x = (int)toolLocation.X;
		y = (int)toolLocation.Y;
		base.CurrentParentTileIndex = 12;
		base.IndexOfMenuItemView = 12;
		location.localSound("coin", toolLocation / 64f);
		who.addItemsByMenuIfNecessary(getPanItems(location, who));
		location.orePanPoint.Value = Point.Zero;
		for (int i = 0; i < (int)upgradeLevel - 1; i++)
		{
			if (location.performOrePanTenMinuteUpdate(Game1.random))
			{
				break;
			}
			if (Game1.random.NextDouble() < 0.5 && location.performOrePanTenMinuteUpdate(Game1.random) && !(location is IslandNorth))
			{
				break;
			}
		}
		finish();
	}

	private void finish()
	{
		finishEvent.Fire();
	}

	private void doFinish()
	{
		lastUser.CanMove = true;
		lastUser.UsingTool = false;
		lastUser.canReleaseTool = true;
	}

	public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
	{
		base.IndexOfMenuItemView = 12;
		base.drawInMenu(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color, drawShadow);
	}

	public List<Item> getPanItems(GameLocation location, Farmer who)
	{
		List<Item> items = new List<Item>();
		string whichOre = "378";
		string whichExtra = null;
		who.stats.Increment("TimesPanned", 1);
		Random r = Utility.CreateRandom(location.orePanPoint.X, (double)location.orePanPoint.Y * 1000.0, Game1.stats.DaysPlayed, who.stats.Get("TimesPanned") * 77);
		double roll = r.NextDouble() - (double)(int)who.luckLevel * 0.001 - who.DailyLuck;
		roll -= (double)((int)upgradeLevel - 1) * 0.05;
		if (roll < 0.01)
		{
			whichOre = "386";
		}
		else if (roll < 0.241)
		{
			whichOre = "384";
		}
		else if (roll < 0.6)
		{
			whichOre = "380";
		}
		if (whichOre != "386" && r.NextDouble() < 0.1 + (hasEnchantmentOfType<ArchaeologistEnchantment>() ? 0.1 : 0.0))
		{
			whichOre = "881";
		}
		int orePieces = r.Next(2, 7) + 1 + (int)((r.NextDouble() + 0.1 + (double)((float)(int)who.luckLevel / 10f) + who.DailyLuck) * 2.0);
		int extraPieces = r.Next(5) + 1 + (int)((r.NextDouble() + 0.1 + (double)((float)(int)who.luckLevel / 10f)) * 2.0);
		orePieces += (int)upgradeLevel - 1;
		roll = r.NextDouble() - who.DailyLuck;
		int numRolls = upgradeLevel;
		bool gotRing = false;
		double extraChance = (double)((int)upgradeLevel - 1) * 0.04;
		if (enchantments.Count > 0)
		{
			extraChance *= 1.25;
		}
		if (hasEnchantmentOfType<GenerousEnchantment>())
		{
			numRolls += 2;
		}
		while (r.NextDouble() - who.DailyLuck < 0.4 + (double)who.LuckLevel * 0.04 + extraChance && numRolls > 0)
		{
			roll = r.NextDouble() - who.DailyLuck;
			roll -= (double)((int)upgradeLevel - 1) * 0.005;
			whichExtra = "382";
			if (roll < 0.02 + (double)who.LuckLevel * 0.002 && r.NextDouble() < 0.75)
			{
				whichExtra = "72";
				extraPieces = 1;
			}
			else if (roll < 0.1 && r.NextDouble() < 0.75)
			{
				whichExtra = (60 + r.Next(5) * 2).ToString();
				extraPieces = 1;
			}
			else if (roll < 0.36)
			{
				whichExtra = "749";
				extraPieces = Math.Max(1, extraPieces / 2);
			}
			else if (roll < 0.5)
			{
				whichExtra = r.Choose("82", "84", "86");
				extraPieces = 1;
			}
			if (roll < (double)who.LuckLevel * 0.002 && !gotRing && r.NextDouble() < 0.33)
			{
				items.Add(new Ring("859"));
				gotRing = true;
			}
			if (roll < 0.01 && r.NextDouble() < 0.5)
			{
				items.Add(Utility.getRandomCosmeticItem(r));
			}
			if (r.NextDouble() < 0.1 && hasEnchantmentOfType<FisherEnchantment>())
			{
				Item f = location.getFish(1f, null, r.Next(1, 6), who, 0.0, who.Tile);
				if (f != null && f.Category == -4)
				{
					items.Add(f);
				}
			}
			if (r.NextDouble() < 0.02 + (hasEnchantmentOfType<ArchaeologistEnchantment>() ? 0.05 : 0.0))
			{
				Item artifact = location.tryGetRandomArtifactFromThisLocation(who, r);
				if (artifact != null)
				{
					items.Add(artifact);
				}
			}
			if (Utility.tryRollMysteryBox(0.05, r))
			{
				items.Add(ItemRegistry.Create((Game1.player.stats.Get(StatKeys.Mastery(2)) != 0) ? "(O)GoldenMysteryBox" : "(O)MysteryBox"));
			}
			if (whichExtra != null)
			{
				items.Add(new Object(whichExtra, extraPieces));
			}
			numRolls--;
		}
		int amount = 0;
		while (r.NextDouble() < 0.05 + (hasEnchantmentOfType<ArchaeologistEnchantment>() ? 0.15 : 0.0))
		{
			amount++;
		}
		if (amount > 0)
		{
			items.Add(ItemRegistry.Create("(O)275", amount));
		}
		items.Add(new Object(whichOre, orePieces));
		if (location is IslandNorth islandNorth && (bool)islandNorth.bridgeFixed && r.NextDouble() < 0.2)
		{
			items.Add(ItemRegistry.Create("(O)822"));
		}
		else if (location is IslandLocation && r.NextDouble() < 0.2)
		{
			items.Add(ItemRegistry.Create("(O)831", r.Next(2, 6)));
		}
		return items;
	}
}
