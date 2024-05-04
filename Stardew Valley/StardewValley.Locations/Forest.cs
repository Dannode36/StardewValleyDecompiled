using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.Network;
using StardewValley.Network.NetEvents;
using StardewValley.TerrainFeatures;
using xTile;
using xTile.Dimensions;

namespace StardewValley.Locations;

public class Forest : GameLocation
{
	public const string raccoonStumpCheckFlag = "checkedRaccoonStump";

	public const string raccoontreeFlag = "raccoonTreeFallen";

	[XmlIgnore]
	public readonly NetObjectList<FarmAnimal> marniesLivestock = new NetObjectList<FarmAnimal>();

	[XmlIgnore]
	public readonly NetList<Microsoft.Xna.Framework.Rectangle, NetRectangle> travelingMerchantBounds = new NetList<Microsoft.Xna.Framework.Rectangle, NetRectangle>();

	[XmlIgnore]
	public readonly NetBool netTravelingMerchantDay = new NetBool(value: false);

	/// <summary>Obsolete. This is only kept to preserve data from old save files. The log blocking access to the Secret Woods is now in <see cref="F:StardewValley.GameLocation.resourceClumps" />.</summary>
	[XmlElement("log")]
	public ResourceClump obsolete_log;

	[XmlElement("stumpFixed")]
	public readonly NetBool stumpFixed = new NetBool();

	private int numRaccoonBabies = -1;

	private int chimneyTimer = 500;

	private bool hasShownCCUpgrade;

	private Microsoft.Xna.Framework.Rectangle hatterSource = new Microsoft.Xna.Framework.Rectangle(600, 1957, 64, 32);

	private Vector2 hatterPos = new Vector2(2056f, 6016f);

	[XmlIgnore]
	public bool travelingMerchantDay
	{
		get
		{
			return netTravelingMerchantDay.Value;
		}
		set
		{
			netTravelingMerchantDay.Value = value;
		}
	}

	public Forest()
	{
	}

	public Forest(string map, string name)
		: base(map, name)
	{
		marniesLivestock.Add(new FarmAnimal("Dairy Cow", Game1.multiplayer.getNewID(), -1L));
		marniesLivestock.Add(new FarmAnimal("Dairy Cow", Game1.multiplayer.getNewID(), -1L));
		marniesLivestock[0].Position = new Vector2(6272f, 1280f);
		marniesLivestock[1].Position = new Vector2(6464f, 1280f);
		resourceClumps.Add(new ResourceClump(602, 2, 2, new Vector2(1f, 6f)));
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(marniesLivestock, "marniesLivestock").AddField(travelingMerchantBounds, "travelingMerchantBounds").AddField(netTravelingMerchantDay, "netTravelingMerchantDay")
			.AddField(stumpFixed, "stumpFixed");
		stumpFixed.fieldChangeEvent += delegate(NetBool f, bool oldValue, bool newValue)
		{
			if (newValue && mapPath.Value != null)
			{
				fixStump(this);
			}
		};
	}

	public static void fixStump(GameLocation location)
	{
		if (!NetWorldState.checkAnywhereForWorldStateID("forestStumpFixed"))
		{
			NetWorldState.addWorldStateIDEverywhere("forestStumpFixed");
		}
		location.updateMap();
		for (int x = 52; x < 60; x++)
		{
			for (int y = 0; y < 2; y++)
			{
				location.removeTile(x, y, "AlwaysFront");
			}
		}
		location.ApplyMapOverride("Forest_RaccoonHouse", (Microsoft.Xna.Framework.Rectangle?)null, (Microsoft.Xna.Framework.Rectangle?)new Microsoft.Xna.Framework.Rectangle(53, 2, 7, 6));
		location.largeTerrainFeatures.Remove(location.getLargeTerrainFeatureAt(55, 10));
		location.largeTerrainFeatures.Remove(location.getLargeTerrainFeatureAt(56, 13));
		location.largeTerrainFeatures.Remove(location.getLargeTerrainFeatureAt(61, 10));
		Game1.currentLightSources.Add(new LightSource(4, new Vector2(3540f, 357f), 0.75f, Color.Black * 0.6f, LightSource.LightContext.None, 0L));
	}

	public void removeSewerTrash()
	{
		ApplyMapOverride("Forest-SewerClean", (Microsoft.Xna.Framework.Rectangle?)null, (Microsoft.Xna.Framework.Rectangle?)new Microsoft.Xna.Framework.Rectangle(83, 97, 24, 12));
		setMapTileIndex(43, 106, -1, "Buildings");
		setMapTileIndex(17, 106, -1, "Buildings");
		setMapTileIndex(13, 105, -1, "Buildings");
		setMapTileIndex(4, 85, -1, "Buildings");
		setMapTileIndex(2, 85, -1, "Buildings");
	}

	protected override void resetLocalState()
	{
		base.resetLocalState();
		addFrog();
		if (Game1.year > 2 && getCharacterFromName("TrashBear") != null && NetWorldState.checkAnywhereForWorldStateID("trashBearDone"))
		{
			characters.Remove(getCharacterFromName("TrashBear"));
		}
		if (numRaccoonBabies == -1)
		{
			numRaccoonBabies = Game1.netWorldState.Value.TimesFedRaccoons - 1;
			if (Game1.netWorldState.Value.Date.TotalDays - Game1.netWorldState.Value.DaysPlayedWhenLastRaccoonBundleWasFinished < 7)
			{
				numRaccoonBabies--;
			}
			if (numRaccoonBabies < 0)
			{
				numRaccoonBabies = 0;
			}
			if (numRaccoonBabies >= 8)
			{
				Game1.getAchievement(39);
			}
		}
		if (!Game1.eventUp && !Game1.player.mailReceived.Contains("seenRaccoonFinishEvent") && numRaccoonBabies >= 8 && !Game1.isRaining && !Game1.isSnowing && Game1.timeOfDay < Game1.getStartingToGetDarkTime(this))
		{
			Game1.player.mailReceived.Add("seenRaccoonFinishEvent");
			string raccoon_event = "none/-10000 -1000/farmer 56 15 0/skippable/specificTemporarySprite raccoonCircle/viewport 56 6 true/pause 3000/specificTemporarySprite raccoonSong/playSound raccoonSong/precisePause 9505/specificTemporarySprite raccoonCircle2/precisePause 9405/specificTemporarySprite raccoonbutterflies/precisePause 9505/specificTemporarySprite raccoondance1/precisePause 9505/specificTemporarySprite raccoondance2/pause 6000/globalfade .003 false/viewport -10000 -1000/spriteText 6 \"" + Game1.content.LoadString("Strings\\1_6_Strings:RaccoonFinal") + "\"/pause 500/end";
			startEvent(new Event(raccoon_event));
		}
	}

	public override void MakeMapModifications(bool force = false)
	{
		base.MakeMapModifications(force);
		if (force)
		{
			hasShownCCUpgrade = false;
		}
		if ((bool)stumpFixed)
		{
			fixStump(this);
		}
		else if (Game1.MasterPlayer.mailReceived.Contains("raccoonTreeFallen"))
		{
			for (int x = 52; x < 60; x++)
			{
				for (int y = 0; y < 2; y++)
				{
					removeTile(x, y, "AlwaysFront");
				}
			}
			ApplyMapOverride("Forest_RaccoonStump", (Microsoft.Xna.Framework.Rectangle?)null, (Microsoft.Xna.Framework.Rectangle?)new Microsoft.Xna.Framework.Rectangle(53, 2, 7, 6));
		}
		if (NetWorldState.checkAnywhereForWorldStateID("trashBearDone"))
		{
			removeSewerTrash();
		}
		if (Game1.MasterPlayer.mailReceived.Contains("communityUpgradeShortcuts"))
		{
			showCommunityUpgradeShortcuts();
		}
		if (Game1.IsSummer && Game1.dayOfMonth >= 17 && Game1.dayOfMonth <= 19)
		{
			ApplyMapOverride(Game1.game1.xTileContent.Load<Map>("Maps\\Forest_FishingDerbySign"), "Forest_FishingDerbySign", null, new Microsoft.Xna.Framework.Rectangle(69, 44, 2, 3), base.cleanUpTileForMapOverride);
		}
		else if (_appliedMapOverrides.Contains("Forest_FishingDerbySign"))
		{
			ApplyMapOverride("Forest_FishingDerbySign_Revert", (Microsoft.Xna.Framework.Rectangle?)null, (Microsoft.Xna.Framework.Rectangle?)new Microsoft.Xna.Framework.Rectangle(69, 44, 2, 3));
			_appliedMapOverrides.Remove("Forest_FishingDerbySign");
			_appliedMapOverrides.Remove("Forest_FishingDerbySign_Revert");
		}
		if (Game1.IsSummer && Game1.dayOfMonth >= 20 && Game1.dayOfMonth <= 21)
		{
			if (getCharacterFromName("derby_contestent0") == null && (Game1.IsMasterGame || !Game1.player.sleptInTemporaryBed))
			{
				if (checkForTerrainFeaturesAndObjectsButDestroyNonPlayerItems(66, 50))
				{
					characters.Add(new NPC(new AnimatedSprite("Characters\\Assorted_Fishermen", 0, 16, 64), new Vector2(66f, 50f) * 64f, -1, "derby_contestent0")
					{
						Breather = false,
						HideShadow = true,
						drawOffset = new Vector2(0f, 96f),
						shouldShadowBeOffset = true,
						SimpleNonVillagerNPC = true
					});
				}
				if (checkForTerrainFeaturesAndObjectsButDestroyNonPlayerItems(69, 50))
				{
					characters.Add(new NPC(new AnimatedSprite("Characters\\Assorted_Fishermen", 2, 16, 64), new Vector2(69f, 50f) * 64f, -1, "derby_contestent1")
					{
						Breather = false,
						HideShadow = true,
						drawOffset = new Vector2(0f, 96f),
						shouldShadowBeOffset = true,
						SimpleNonVillagerNPC = true
					});
				}
				if (checkForTerrainFeaturesAndObjectsButDestroyNonPlayerItems(74, 50))
				{
					characters.Add(new NPC(new AnimatedSprite("Characters\\Assorted_Fishermen", 3, 16, 64), new Vector2(74f, 50f) * 64f, -1, "derby_contestent2")
					{
						Breather = false,
						HideShadow = true,
						drawOffset = new Vector2(0f, 96f),
						shouldShadowBeOffset = true,
						SimpleNonVillagerNPC = true
					});
				}
				if (checkForTerrainFeaturesAndObjectsButDestroyNonPlayerItems(43, 59))
				{
					characters.Add(new NPC(new AnimatedSprite("Characters\\Assorted_Fishermen", 1, 16, 64), new Vector2(43f, 59f) * 64f, -1, "derby_contestent3")
					{
						Breather = false,
						HideShadow = true,
						drawOffset = new Vector2(0f, 96f),
						shouldShadowBeOffset = true,
						SimpleNonVillagerNPC = true
					});
				}
				if (checkForTerrainFeaturesAndObjectsButDestroyNonPlayerItems(84, 40) && checkForTerrainFeaturesAndObjectsButDestroyNonPlayerItems(85, 40))
				{
					characters.Add(new NPC(new AnimatedSprite("Characters\\Assorted_Fishermen", 2, 32, 64), new Vector2(84f, 40f) * 64f, -1, "derby_contestent4")
					{
						Breather = false,
						HideShadow = true,
						drawOffset = new Vector2(0f, 96f),
						shouldShadowBeOffset = true,
						SimpleNonVillagerNPC = true
					});
				}
				if (checkForTerrainFeaturesAndObjectsButDestroyNonPlayerItems(88, 49))
				{
					characters.Add(new NPC(new AnimatedSprite("Characters\\Assorted_Fishermen", 8, 32, 32), new Vector2(88f, 49f) * 64f, -1, "derby_contestent5")
					{
						Breather = false,
						HideShadow = true,
						drawOffset = new Vector2(0f, 0f),
						shouldShadowBeOffset = true,
						SimpleNonVillagerNPC = true
					});
				}
				if (checkForTerrainFeaturesAndObjectsButDestroyNonPlayerItems(92, 54))
				{
					characters.Add(new NPC(new AnimatedSprite("Characters\\Assorted_Fishermen", 9, 32, 32), new Vector2(91f, 54f) * 64f, -1, "derby_contestent6")
					{
						Breather = false,
						HideShadow = true,
						drawOffset = new Vector2(0f, 0f),
						shouldShadowBeOffset = true,
						SimpleNonVillagerNPC = true
					});
				}
				if (checkForTerrainFeaturesAndObjectsButDestroyNonPlayerItems(20, 73))
				{
					characters.Add(new NPC(new AnimatedSprite("Characters\\Assorted_Fishermen", 10, 32, 32), new Vector2(20f, 73f) * 64f, -1, "derby_contestent7")
					{
						Breather = false,
						HideShadow = true,
						drawOffset = new Vector2(0f, 0f),
						shouldShadowBeOffset = true,
						SimpleNonVillagerNPC = true
					});
				}
				if (checkForTerrainFeaturesAndObjectsButDestroyNonPlayerItems(77, 48))
				{
					characters.Add(new NPC(new AnimatedSprite("Characters\\Assorted_Fishermen", 11, 32, 32), new Vector2(76f, 48f) * 64f, -1, "derby_contestent8")
					{
						Breather = false,
						HideShadow = true,
						drawOffset = new Vector2(0f, 0f),
						shouldShadowBeOffset = true,
						SimpleNonVillagerNPC = true
					});
				}
				if (checkForTerrainFeaturesAndObjectsButDestroyNonPlayerItems(83, 51))
				{
					characters.Add(new NPC(new AnimatedSprite("Characters\\Assorted_Fishermen", 12, 32, 32), new Vector2(82f, 51f) * 64f, -1, "derby_contestent9")
					{
						Breather = false,
						HideShadow = true,
						drawOffset = new Vector2(0f, 0f),
						shouldShadowBeOffset = true,
						SimpleNonVillagerNPC = true
					});
				}
			}
			if (getCharacterFromName("derby_contestent0") != null)
			{
				NPC npc = getCharacterFromName("derby_contestent0");
				npc.drawOffset = new Vector2(0f, 96f);
				npc.shouldShadowBeOffset = true;
				if (npc.Sprite == null || npc.Sprite.Texture == null)
				{
					npc.Sprite = new AnimatedSprite("Characters\\Assorted_Fishermen", 0, 16, 64);
				}
				npc.SimpleNonVillagerNPC = true;
				npc.HideShadow = true;
				npc.Breather = false;
			}
			if (getCharacterFromName("derby_contestent1") != null)
			{
				NPC npc = getCharacterFromName("derby_contestent1");
				npc.drawOffset = new Vector2(0f, 96f);
				npc.shouldShadowBeOffset = true;
				if (npc.Sprite == null || npc.Sprite.Texture == null)
				{
					npc.Sprite = new AnimatedSprite("Characters\\Assorted_Fishermen", 2, 16, 64);
				}
				npc.Sprite.CurrentFrame = 2;
				npc.SimpleNonVillagerNPC = true;
				npc.HideShadow = true;
				npc.Breather = false;
			}
			if (getCharacterFromName("derby_contestent2") != null)
			{
				NPC npc = getCharacterFromName("derby_contestent2");
				if (npc.Sprite == null || npc.Sprite.Texture == null)
				{
					npc.Sprite = new AnimatedSprite("Characters\\Assorted_Fishermen", 3, 16, 64);
				}
				npc.Sprite.CurrentFrame = 3;
				npc.drawOffset = new Vector2(0f, 96f);
				npc.shouldShadowBeOffset = true;
				npc.SimpleNonVillagerNPC = true;
				npc.HideShadow = true;
				npc.Breather = false;
			}
			if (getCharacterFromName("derby_contestent3") != null)
			{
				NPC npc = getCharacterFromName("derby_contestent3");
				if (npc.Sprite == null || npc.Sprite.Texture == null)
				{
					npc.Sprite = new AnimatedSprite("Characters\\Assorted_Fishermen", 1, 16, 64);
				}
				npc.Sprite.CurrentFrame = 1;
				npc.drawOffset = new Vector2(0f, 96f);
				npc.shouldShadowBeOffset = true;
				npc.SimpleNonVillagerNPC = true;
				npc.HideShadow = true;
				npc.Breather = false;
			}
			if (getCharacterFromName("derby_contestent4") != null)
			{
				NPC npc = getCharacterFromName("derby_contestent4");
				if (npc.Sprite == null || npc.Sprite.Texture == null)
				{
					npc.Sprite = new AnimatedSprite("Characters\\Assorted_Fishermen", 2, 32, 64);
				}
				npc.Sprite.CurrentFrame = 2;
				npc.drawOffset = new Vector2(0f, 96f);
				npc.shouldShadowBeOffset = true;
				npc.SimpleNonVillagerNPC = true;
				npc.HideShadow = true;
				npc.Breather = false;
			}
			if (getCharacterFromName("derby_contestent5") != null)
			{
				NPC npc = getCharacterFromName("derby_contestent5");
				if (npc.Sprite == null || npc.Sprite.Texture == null)
				{
					npc.Sprite = new AnimatedSprite("Characters\\Assorted_Fishermen", 8, 32, 32);
				}
				npc.Sprite.CurrentFrame = 8;
				npc.shouldShadowBeOffset = true;
				npc.SimpleNonVillagerNPC = true;
				npc.HideShadow = true;
				npc.Breather = false;
			}
			if (getCharacterFromName("derby_contestent6") != null)
			{
				NPC npc = getCharacterFromName("derby_contestent6");
				if (npc.Sprite == null || npc.Sprite.Texture == null)
				{
					npc.Sprite = new AnimatedSprite("Characters\\Assorted_Fishermen", 9, 32, 32);
				}
				npc.Sprite.CurrentFrame = 9;
				npc.shouldShadowBeOffset = true;
				npc.SimpleNonVillagerNPC = true;
				npc.HideShadow = true;
				npc.Breather = false;
			}
			if (getCharacterFromName("derby_contestent7") != null)
			{
				NPC npc = getCharacterFromName("derby_contestent7");
				if (npc.Sprite == null || npc.Sprite.Texture == null)
				{
					npc.Sprite = new AnimatedSprite("Characters\\Assorted_Fishermen", 10, 32, 32);
				}
				npc.Sprite.CurrentFrame = 10;
				npc.shouldShadowBeOffset = true;
				npc.SimpleNonVillagerNPC = true;
				npc.HideShadow = true;
				npc.Breather = false;
			}
			if (getCharacterFromName("derby_contestent8") != null)
			{
				NPC npc = getCharacterFromName("derby_contestent8");
				if (npc.Sprite == null || npc.Sprite.Texture == null)
				{
					npc.Sprite = new AnimatedSprite("Characters\\Assorted_Fishermen", 11, 32, 32);
				}
				npc.Sprite.CurrentFrame = 11;
				npc.shouldShadowBeOffset = true;
				npc.SimpleNonVillagerNPC = true;
				npc.HideShadow = true;
				npc.Breather = false;
			}
			if (getCharacterFromName("derby_contestent9") != null)
			{
				NPC npc = getCharacterFromName("derby_contestent9");
				if (npc.Sprite == null || npc.Sprite.Texture == null)
				{
					npc.Sprite = new AnimatedSprite("Characters\\Assorted_Fishermen", 12, 32, 32);
				}
				npc.Sprite.CurrentFrame = 12;
				npc.shouldShadowBeOffset = true;
				npc.SimpleNonVillagerNPC = true;
				npc.HideShadow = true;
				npc.Breather = false;
			}
			ApplyMapOverride(Game1.game1.xTileContent.Load<Map>("Maps\\Forest_FishingDerby"), "Forest_FishingDerby", null, new Microsoft.Xna.Framework.Rectangle(63, 43, 11, 5), base.cleanUpTileForMapOverride);
			Game1.currentLightSources.Add(new LightSource(1, new Vector2(4596f, 2968f), 3f, LightSource.LightContext.None, 0L));
			Game1.currentLightSources.Add(new LightSource(1, new Vector2(4324f, 3044f), 3f, LightSource.LightContext.None, 0L));
		}
		else
		{
			if (!_appliedMapOverrides.Contains("Forest_FishingDerby") && getTileIndexAt(63, 47, "Buildings") == -1)
			{
				return;
			}
			ApplyMapOverride("Forest_FishingDerby_Revert", (Microsoft.Xna.Framework.Rectangle?)null, (Microsoft.Xna.Framework.Rectangle?)new Microsoft.Xna.Framework.Rectangle(63, 43, 11, 5));
			_appliedMapOverrides.Remove("Forest_FishingDerby");
			_appliedMapOverrides.Remove("Forest_FishingDerby_Revert");
			for (int i = characters.Count - 1; i >= 0; i--)
			{
				if (characters[i].Name.StartsWith("derby_contestent"))
				{
					characters.RemoveAt(i);
				}
			}
		}
	}

	private void showCommunityUpgradeShortcuts()
	{
		if (hasShownCCUpgrade)
		{
			return;
		}
		removeTile(119, 36, "Buildings");
		LargeTerrainFeature blockingBush = null;
		foreach (LargeTerrainFeature t in largeTerrainFeatures)
		{
			if (t.Tile == new Vector2(119f, 35f))
			{
				blockingBush = t;
				break;
			}
		}
		if (blockingBush != null)
		{
			largeTerrainFeatures.Remove(blockingBush);
		}
		hasShownCCUpgrade = true;
		warps.Add(new Warp(120, 35, "Beach", 0, 6, flipFarmer: false));
		warps.Add(new Warp(120, 36, "Beach", 0, 6, flipFarmer: false));
	}

	protected override void resetSharedState()
	{
		base.resetSharedState();
		if (ShouldTravelingMerchantVisitToday())
		{
			if (!travelingMerchantDay)
			{
				travelingMerchantDay = true;
				Point merchantOrigin = GetTravelingMerchantCartTile();
				travelingMerchantBounds.Clear();
				travelingMerchantBounds.Add(new Microsoft.Xna.Framework.Rectangle(merchantOrigin.X * 64, merchantOrigin.Y * 64, 492, 116));
				travelingMerchantBounds.Add(new Microsoft.Xna.Framework.Rectangle(merchantOrigin.X * 64 + 180, merchantOrigin.Y * 64 + 104, 76, 48));
				travelingMerchantBounds.Add(new Microsoft.Xna.Framework.Rectangle(merchantOrigin.X * 64 + 340, merchantOrigin.Y * 64 + 104, 104, 48));
				foreach (Microsoft.Xna.Framework.Rectangle travelingMerchantBound in travelingMerchantBounds)
				{
					Utility.clearObjectsInArea(travelingMerchantBound, this);
				}
			}
		}
		else
		{
			travelingMerchantDay = false;
			travelingMerchantBounds.Clear();
		}
		if (Game1.year > 2 && !IsRainingHere() && !Utility.isFestivalDay() && getCharacterFromName("TrashBear") == null && !NetWorldState.checkAnywhereForWorldStateID("trashBearDone"))
		{
			characters.Add(new TrashBear());
		}
		if (Game1.MasterPlayer.mailReceived.Contains("raccoonMovedIn"))
		{
			if (getCharacterFromName("Raccoon") == null)
			{
				characters.Add(new Raccoon(mrs_racooon: false));
			}
			if (getCharacterFromName("MrsRaccoon") == null && (Game1.netWorldState.Value.TimesFedRaccoons > 1 || (Game1.netWorldState.Value.DaysPlayedWhenLastRaccoonBundleWasFinished != 0 && Game1.netWorldState.Value.Date.TotalDays - Game1.netWorldState.Value.DaysPlayedWhenLastRaccoonBundleWasFinished >= 7)))
			{
				characters.Add(new Raccoon(mrs_racooon: true));
			}
		}
	}

	public static bool isWizardHouseUnlocked()
	{
		if (Game1.player.mailReceived.Contains("wizardJunimoNote"))
		{
			return true;
		}
		if (Game1.MasterPlayer.mailReceived.Contains("JojaMember"))
		{
			return true;
		}
		bool num = Game1.MasterPlayer.mailReceived.Contains("ccFishTank");
		bool ccBulletin = Game1.MasterPlayer.mailReceived.Contains("ccBulletin");
		bool ccPantry = Game1.MasterPlayer.mailReceived.Contains("ccPantry");
		bool ccVault = Game1.MasterPlayer.mailReceived.Contains("ccVault");
		bool ccBoilerRoom = Game1.MasterPlayer.mailReceived.Contains("ccBoilerRoom");
		bool ccCraftsRoom = Game1.MasterPlayer.mailReceived.Contains("ccCraftsRoom");
		return num && ccBulletin && ccPantry && ccVault && ccBoilerRoom && ccCraftsRoom;
	}

	/// <summary>Get whether the traveling cart should visit the forest today.</summary>
	public bool ShouldTravelingMerchantVisitToday()
	{
		return Game1.dayOfMonth % 7 % 5 == 0;
	}

	/// <summary>Get the tile coordinates for the top-left corner of the traveling cart's bounding area.</summary>
	public Point GetTravelingMerchantCartTile()
	{
		if (!TryGetMapPropertyAs("TravelingCartPosition", out Point tile, required: false))
		{
			return new Point(23, 10);
		}
		return tile;
	}

	public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
	{
		int tileIndexOfCheckLocation = getTileIndexAt(tileLocation, "Buildings");
		if (tileIndexOfCheckLocation == 901 && !isWizardHouseUnlocked())
		{
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Forest_WizardTower_Locked"));
			return false;
		}
		if (base.checkAction(tileLocation, viewport, who))
		{
			return true;
		}
		switch (tileIndexOfCheckLocation)
		{
		case 1394:
			if (who.mailReceived.Contains("OpenedSewer"))
			{
				Game1.warpFarmer("Sewer", 3, 48, 0);
				playSound("openChest");
			}
			else if (who.hasRustyKey)
			{
				playSound("openBox");
				Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:Forest_OpenedSewer")));
				who.mailReceived.Add("OpenedSewer");
			}
			else
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:LockedDoor"));
			}
			break;
		case 1972:
			if (who.achievements.Count > 0)
			{
				Utility.TryOpenShopMenu("HatMouse", "HatMouse");
			}
			else
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Forest_HatMouseStore_Abandoned"));
			}
			break;
		}
		if (travelingMerchantDay && Game1.timeOfDay < 2000)
		{
			Point cartOrigin = GetTravelingMerchantCartTile();
			if (tileLocation.X == cartOrigin.X + 4 && tileLocation.Y == cartOrigin.Y + 1)
			{
				Utility.TryOpenShopMenu("Traveler", null, playOpenSound: true);
				return true;
			}
			if (tileLocation.X == cartOrigin.X && tileLocation.Y == cartOrigin.Y + 1)
			{
				playSound("pig");
				return true;
			}
		}
		return false;
	}

	public override bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character, bool pathfinding, bool projectile = false, bool ignoreCharacterRequirement = false, bool skipCollisionEffects = false)
	{
		if (travelingMerchantBounds != null)
		{
			foreach (Microsoft.Xna.Framework.Rectangle r in travelingMerchantBounds)
			{
				if (position.Intersects(r))
				{
					return true;
				}
			}
		}
		return base.isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character, pathfinding, projectile, ignoreCharacterRequirement);
	}

	public override bool isTilePlaceable(Vector2 v, bool itemIsPassable = false)
	{
		if (travelingMerchantBounds != null)
		{
			Microsoft.Xna.Framework.Rectangle tileRect = new Microsoft.Xna.Framework.Rectangle((int)v.X * 64, (int)v.Y * 64, 64, 64);
			foreach (Microsoft.Xna.Framework.Rectangle r in travelingMerchantBounds)
			{
				if (tileRect.Intersects(r))
				{
					return false;
				}
			}
		}
		return base.isTilePlaceable(v, itemIsPassable);
	}

	public override void DayUpdate(int dayOfMonth)
	{
		base.DayUpdate(dayOfMonth);
		numRaccoonBabies = -1;
		if (Game1.IsMasterGame && ShouldTravelingMerchantVisitToday() && Game1.netWorldState.Value.VisitsUntilY1Guarantee >= 0)
		{
			Game1.netWorldState.Value.VisitsUntilY1Guarantee--;
		}
		if (IsSpringHere())
		{
			for (int i = 0; i < 7; i++)
			{
				Vector2 origin = new Vector2(Game1.random.Next(70, map.Layers[0].LayerWidth - 10), Game1.random.Next(68, map.Layers[0].LayerHeight - 15));
				if (!(origin.Y > 30f))
				{
					continue;
				}
				foreach (Vector2 v in Utility.recursiveFindOpenTiles(this, origin, 16))
				{
					string s = doesTileHaveProperty((int)v.X, (int)v.Y, "Diggable", "Back");
					if (!terrainFeatures.ContainsKey(v) && s != null && Game1.random.NextDouble() < (double)(1f - Vector2.Distance(origin, v) * 0.15f))
					{
						terrainFeatures.Add(v, new HoeDirt(0, new Crop(forageCrop: true, "1", (int)v.X, (int)v.Y, this)));
					}
				}
			}
		}
		if (Game1.year > 2 && getCharacterFromName("TrashBear") != null)
		{
			characters.Remove(getCharacterFromName("TrashBear"));
		}
		if (Game1.IsSummer)
		{
			for (int i = characters.Count - 1; i >= 0; i--)
			{
				if (characters[i].Name.StartsWith("derby_contestent"))
				{
					characters.RemoveAt(i);
				}
			}
		}
		if (Game1.IsSpring && Game1.dayOfMonth == 17)
		{
			objects.TryAdd(new Vector2(52f, 98f), ItemRegistry.Create<Object>("(O)PotOfGold"));
		}
		if (Game1.IsSpring && Game1.dayOfMonth == 18 && objects.ContainsKey(new Vector2(52f, 98f)) && objects[new Vector2(52f, 98f)].QualifiedItemId == "(O)PotOfGold")
		{
			objects.Remove(new Vector2(52f, 98f));
		}
	}

	public override void UpdateWhenCurrentLocation(GameTime time)
	{
		base.UpdateWhenCurrentLocation(time);
		foreach (FarmAnimal item in marniesLivestock)
		{
			item.updateWhenCurrentLocation(time, this);
		}
		if (Game1.timeOfDay >= 2000)
		{
			return;
		}
		Point cartOrigin = GetTravelingMerchantCartTile();
		if (travelingMerchantDay)
		{
			if (Game1.random.NextDouble() < 0.001)
			{
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(99, 1423, 13, 19), new Vector2(cartOrigin.X * 64, cartOrigin.Y * 64 + 32 - 4), flipped: false, 0f, Color.White)
				{
					interval = Game1.random.Next(500, 1500),
					layerDepth = 0.07682f,
					scale = 4f
				});
			}
			if (Game1.random.NextDouble() < 0.001)
			{
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(51, 1444, 5, 5), new Vector2(cartOrigin.X * 64 + 32 - 4, (cartOrigin.Y + 1) * 64 + 32 + 8), flipped: false, 0f, Color.White)
				{
					interval = 500f,
					animationLength = 1,
					layerDepth = 0.07682f,
					scale = 4f
				});
			}
			if (Game1.random.NextDouble() < 0.003)
			{
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(89, 1445, 6, 3), new Vector2((cartOrigin.X + 4) * 64 + 32 + 4, cartOrigin.Y * 64 + 24), flipped: false, 0f, Color.White)
				{
					interval = 50f,
					animationLength = 3,
					pingPong = true,
					totalNumberOfLoops = 1,
					layerDepth = 0.07682f,
					scale = 4f
				});
			}
		}
		chimneyTimer -= time.ElapsedGameTime.Milliseconds;
		if (chimneyTimer <= 0)
		{
			chimneyTimer = (travelingMerchantDay ? 500 : Game1.random.Next(200, 2000));
			Vector2 smokeSpot = (travelingMerchantDay ? new Vector2((cartOrigin.X + 6) * 64 + 12, (cartOrigin.Y - 2) * 64 + 12) : new Vector2(5592f, 608f));
			temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), smokeSpot, flipped: false, 0.002f, Color.Gray)
			{
				alpha = 0.75f,
				motion = new Vector2(0f, -0.5f),
				acceleration = new Vector2(0.002f, 0f),
				interval = 99999f,
				layerDepth = 1f,
				scale = 3f,
				scaleChange = 0.01f,
				rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f
			});
			if ((bool)stumpFixed && Game1.MasterPlayer.mailReceived.Contains("raccoonMovedIn"))
			{
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), new Vector2(57.33f, 1.75f) * 64f, flipped: false, 0.002f, Color.Gray)
				{
					alpha = 0.75f,
					motion = new Vector2(0f, -0.5f),
					acceleration = new Vector2(0.002f, 0f),
					interval = 99999f,
					drawAboveAlwaysFront = true,
					layerDepth = 1f,
					scale = 3f,
					scaleChange = 0.01f,
					rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f
				});
			}
			if (travelingMerchantDay)
			{
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(225, 1388, 7, 5), new Vector2((cartOrigin.X + 6) * 64 + 12, (cartOrigin.Y - 2) * 64 + 24), flipped: false, 0f, Color.White)
				{
					interval = chimneyTimer - chimneyTimer / 5,
					animationLength = 1,
					layerDepth = 0.99f,
					scale = 4.3f,
					scaleChange = -0.015f
				});
			}
		}
	}

	public override bool performAction(string[] action, Farmer who, Location tileLocation)
	{
		if (Game1.MasterPlayer.mailReceived.Contains("raccoonTreeFallen") && action.Length != 0 && action[0] == "FixRaccoonStump")
		{
			if (who.Items.ContainsId("(O)709", 100))
			{
				createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:FixRaccoonStump_Question"), createYesNoResponses(), "ForestStump");
			}
			else
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:FixRaccoonStump_Hint"));
				if (!who.mailReceived.Contains("checkedRaccoonStump"))
				{
					who.addQuest("134");
					who.mailReceived.Add("checkedRaccoonStump");
				}
			}
		}
		return base.performAction(action, who, tileLocation);
	}

	public override bool answerDialogueAction(string questionAndAnswer, string[] questionParams)
	{
		if (questionAndAnswer == "ForestStump_Yes")
		{
			Game1.globalFadeToBlack(fadedForStumpFix);
			Game1.player.Items.ReduceId("(O)709", 100);
			Game1.player.team.RequestSetSimpleFlag(SimpleFlagType.HasQuest, PlayerActionTarget.All, "134", flagState: false);
			return true;
		}
		return base.answerDialogueAction(questionAndAnswer, questionParams);
	}

	public void fadedForStumpFix()
	{
		Game1.freezeControls = true;
		DelayedAction.playSoundAfterDelay("crafting", 1000);
		DelayedAction.playSoundAfterDelay("crafting", 1500);
		DelayedAction.playSoundAfterDelay("crafting", 2000);
		DelayedAction.playSoundAfterDelay("crafting", 2500);
		DelayedAction.playSoundAfterDelay("axchop", 3000);
		DelayedAction.playSoundAfterDelay("discoverMineral", 3200);
		Game1.viewportFreeze = true;
		Game1.viewport.X = -10000;
		stumpFixed.Value = true;
		Game1.pauseThenDoFunction(4000, doneWithStumpFix);
		fixStump(this);
		Game1.addMailForTomorrow("raccoonMovedIn", noLetter: true, sendToEveryone: true);
	}

	public void doneWithStumpFix()
	{
		Game1.globalFadeToClear(delegate
		{
			if (!Game1.fadeToBlack)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:FixRaccoonStump_Done"));
			}
		});
		Game1.viewportFreeze = false;
		Game1.freezeControls = false;
	}

	public override void performTenMinuteUpdate(int timeOfDay)
	{
		base.performTenMinuteUpdate(timeOfDay);
		if (travelingMerchantDay && Game1.random.NextDouble() < 0.4)
		{
			temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(57, 1430, 4, 12), new Vector2(1792f, 656f), flipped: false, 0f, Color.White)
			{
				interval = 50f,
				animationLength = 10,
				pingPong = true,
				totalNumberOfLoops = 1,
				layerDepth = 0.07682f,
				scale = 4f
			});
			if (Game1.random.NextDouble() < 0.66)
			{
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(89, 1445, 6, 3), new Vector2(1764f, 664f), flipped: false, 0f, Color.White)
				{
					interval = 50f,
					animationLength = 3,
					pingPong = true,
					totalNumberOfLoops = 1,
					layerDepth = 0.07683001f,
					scale = 4f
				});
			}
		}
		if (!Game1.IsSummer || Game1.dayOfMonth < 20 || Game1.dayOfMonth > 21)
		{
			return;
		}
		Random r = Utility.CreateDaySaveRandom(Game1.timeOfDay * 20);
		NPC n = getCharacterFromName("derby_contestent" + r.Next(10));
		if (n == null)
		{
			return;
		}
		n.shake(600);
		if (r.NextBool(0.25))
		{
			int whichSaying = r.Next(7);
			n.showTextAboveHead(Game1.content.LoadString("Strings\\1_6_Strings:FishingDerby_Exclamation" + whichSaying));
			if (whichSaying == 0 || whichSaying == 6)
			{
				temporarySprites.Add(new TemporaryAnimatedSprite(138, 1500f, 1, 1, n.Position, flicker: false, flipped: false, verticalFlipped: false, 0f)
				{
					motion = new Vector2((float)Game1.random.Next(-10, 10) / 10f, -7f),
					acceleration = new Vector2(0f, 0.1f),
					alphaFade = 0.001f,
					drawAboveAlwaysFront = true
				});
			}
			n.jump(4f);
		}
	}

	public override void draw(SpriteBatch spriteBatch)
	{
		base.draw(spriteBatch);
		foreach (FarmAnimal item in marniesLivestock)
		{
			item.draw(spriteBatch);
		}
		if (travelingMerchantDay)
		{
			Point cartOrigin = GetTravelingMerchantCartTile();
			spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(new Vector2((cartOrigin.X + 1) * 64, (cartOrigin.Y - 2) * 64)), new Microsoft.Xna.Framework.Rectangle(142, 1382, 109, 70), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0768f);
			spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(new Vector2(cartOrigin.X * 64, cartOrigin.Y * 64 + 32)), new Microsoft.Xna.Framework.Rectangle(112, 1424, 30, 24), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.07681f);
			spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(new Vector2((cartOrigin.X + 1) * 64, (cartOrigin.Y + 1) * 64 + 32 - 8)), new Microsoft.Xna.Framework.Rectangle(142, 1424, 16, 3), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.07682f);
			spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(new Vector2((cartOrigin.X + 1) * 64 + 8, cartOrigin.Y * 64 - 32 - 8)), new Microsoft.Xna.Framework.Rectangle(71, 1966, 18, 18), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.07678001f);
			spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(new Vector2(cartOrigin.X * 64, cartOrigin.Y * 64 - 32)), new Microsoft.Xna.Framework.Rectangle(167, 1966, 18, 18), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.07678001f);
			if (Game1.timeOfDay >= 2000)
			{
				spriteBatch.Draw(Game1.staminaRect, Game1.GlobalToLocal(Game1.viewport, new Microsoft.Xna.Framework.Rectangle((cartOrigin.X + 4) * 64 + 16, cartOrigin.Y * 64, 64, 64)), Game1.staminaRect.Bounds, Color.Black, 0f, Vector2.Zero, SpriteEffects.None, 0.076840006f);
			}
		}
		if (Game1.player.achievements.Count > 0)
		{
			spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(hatterPos), hatterSource, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.6016f);
		}
		if (!stumpFixed && Game1.MasterPlayer.mailReceived.Contains("raccoonTreeFallen") && !Game1.player.mailReceived.Contains("checkedRaccoonStump"))
		{
			float yOffset = -8f + 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
			spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(3576f, 272f + yOffset)), new Microsoft.Xna.Framework.Rectangle(141, 465, 20, 24), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.050400995f);
			spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(3616f, 312f + yOffset)), new Microsoft.Xna.Framework.Rectangle(175, 425, 12, 12), Color.White * 0.75f, 0f, new Vector2(6f, 6f), 4f, SpriteEffects.None, 0.050409995f);
		}
		else if (numRaccoonBabies > 0)
		{
			for (int i = 0; i < Math.Min(numRaccoonBabies, 8); i++)
			{
				switch (i)
				{
				case 0:
					spriteBatch.Draw(Game1.mouseCursors_1_6, Game1.GlobalToLocal(new Vector2(3706f, 340f)), new Microsoft.Xna.Framework.Rectangle(213 + ((Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 5000.0 < 200.0) ? 10 : 0), 472, 10, 9), Color.White, 0f, new Vector2(5.5f, 9f), 4f, SpriteEffects.None, 0.0448f);
					break;
				case 1:
					spriteBatch.Draw(Game1.mouseCursors_1_6, Game1.GlobalToLocal(new Vector2(54f, 4f) * 64f + new Vector2(8f, -12f)), new Microsoft.Xna.Framework.Rectangle(235 + ((Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 4500.0 < 200.0) ? 9 : 0), 472, 9, 12), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, 0.0448f);
					break;
				case 2:
					spriteBatch.Draw(Game1.mouseCursors_1_6, Game1.GlobalToLocal(new Vector2(3462f, 433f)), new Microsoft.Xna.Framework.Rectangle(213 + ((Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 6000.0 < 200.0) ? 10 : 0), 472, 10, 9), Color.White, 0f, new Vector2(5.5f, 9f), 4f, SpriteEffects.None, 0.0448f);
					break;
				case 3:
					spriteBatch.Draw(Game1.mouseCursors_1_6, Game1.GlobalToLocal(new Vector2(58f, 4f) * 64f + new Vector2(4f, -20f)), new Microsoft.Xna.Framework.Rectangle(235 + ((Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 4800.0 < 200.0) ? 9 : 0), 472, 9, 12), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0448f);
					break;
				case 4:
					spriteBatch.Draw(Game1.mouseCursors_1_6, Game1.GlobalToLocal(new Vector2(3770f, 408f)), new Microsoft.Xna.Framework.Rectangle(213 + ((Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 5000.0 < 200.0) ? 10 : 0), 472, 10, 9), Color.White, 0f, new Vector2(5.5f, 9f), 4f, SpriteEffects.None, 0.0448f);
					break;
				case 5:
					spriteBatch.Draw(Game1.mouseCursors_1_6, Game1.GlobalToLocal(new Vector2(55f, 3f) * 64f + new Vector2(12f, 4f)), new Microsoft.Xna.Framework.Rectangle(213 + ((Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 5000.0 < 200.0) ? 10 : 0), 472, 10, 9), Color.White, 0f, new Vector2(5.5f, 9f), 4f, SpriteEffects.None, 0.0064f);
					break;
				case 6:
					spriteBatch.Draw(Game1.mouseCursors_1_6, Game1.GlobalToLocal(new Vector2(56f, 3f) * 64f + new Vector2(40f, -8f)), new Microsoft.Xna.Framework.Rectangle(213 + ((Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 5200.0 < 200.0) ? 10 : 0), 472, 10, 9), Color.White, 0f, new Vector2(5.5f, 9f), 4f, SpriteEffects.None, 0.0064f);
					break;
				case 7:
					spriteBatch.Draw(Game1.mouseCursors_1_6, Game1.GlobalToLocal(new Vector2(58f, 3f) * 64f + new Vector2(-20f, -48f)), new Microsoft.Xna.Framework.Rectangle(235 + ((Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 4600.0 < 200.0) ? 9 : 0), 472, 9, 12), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0448f);
					break;
				}
			}
		}
		if (Game1.IsSpring && Game1.dayOfMonth == 17)
		{
			spriteBatch.Draw(Game1.mouseCursors_1_6, Game1.GlobalToLocal(new Vector2(52f, 97f) * 64f), new Microsoft.Xna.Framework.Rectangle(257, 108, 136, 116), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
		}
	}
}
