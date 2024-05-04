using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.GameData.Crops;
using StardewValley.GameData.GiantCrops;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Mods;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

namespace StardewValley;

public class Crop : INetObject<NetFields>, IHaveModData
{
	public const string mixedSeedsId = "770";

	public const string mixedSeedsQId = "(O)770";

	public const int seedPhase = 0;

	public const int rowOfWildSeeds = 23;

	public const int finalPhaseLength = 99999;

	public const int forageCrop_springOnion = 1;

	public const string forageCrop_springOnionID = "1";

	public const int forageCrop_ginger = 2;

	public const string forageCrop_gingerID = "2";

	/// <summary>The backing field for <see cref="P:StardewValley.Crop.currentLocation" />.</summary>
	private GameLocation currentLocationImpl;

	/// <summary>The number of days in each visual step of growth before the crop is harvestable. The last entry in this list is <see cref="F:StardewValley.Crop.finalPhaseLength" />.</summary>
	public readonly NetIntList phaseDays = new NetIntList();

	/// <summary>The index of this crop in the spritesheet texture (one crop per row).</summary>
	[XmlElement("rowInSpriteSheet")]
	public readonly NetInt rowInSpriteSheet = new NetInt();

	[XmlElement("phaseToShow")]
	public readonly NetInt phaseToShow = new NetInt(-1);

	[XmlElement("currentPhase")]
	public readonly NetInt currentPhase = new NetInt();

	/// <summary>The unqualified item ID produced when this crop is harvested.</summary>
	[XmlElement("indexOfHarvest")]
	public readonly NetString indexOfHarvest = new NetString();

	[XmlElement("dayOfCurrentPhase")]
	public readonly NetInt dayOfCurrentPhase = new NetInt();

	/// <summary>The seed ID, if this is a forage or wild seed crop.</summary>
	[XmlElement("whichForageCrop")]
	public readonly NetString whichForageCrop = new NetString();

	/// <summary>The tint colors that can be applied to the crop sprite, if any. If multiple colors are listed, one is chosen at random for each crop.</summary>
	[XmlElement("tintColor")]
	public readonly NetColor tintColor = new NetColor();

	[XmlElement("flip")]
	public readonly NetBool flip = new NetBool();

	[XmlElement("fullGrown")]
	public readonly NetBool fullyGrown = new NetBool();

	/// <summary>Whether this is a raised crop on a trellis that can't be walked through.</summary>
	[XmlElement("raisedSeeds")]
	public readonly NetBool raisedSeeds = new NetBool();

	/// <summary>Whether to apply the <see cref="F:StardewValley.Crop.tintColor" />.</summary>
	[XmlElement("programColored")]
	public readonly NetBool programColored = new NetBool();

	[XmlElement("dead")]
	public readonly NetBool dead = new NetBool();

	[XmlElement("forageCrop")]
	public readonly NetBool forageCrop = new NetBool();

	/// <summary>The unqualified seed ID, if this is a regular crop.</summary>
	[XmlElement("seedIndex")]
	public readonly NetString netSeedIndex = new NetString();

	/// <summary>The asset name for the crop texture under the game's <c>Content</c> folder, or null to use <see cref="F:StardewValley.Game1.cropSpriteSheetName" />.</summary>
	[XmlElement("overrideTexturePath")]
	public readonly NetString overrideTexturePath = new NetString();

	protected Texture2D _drawnTexture;

	protected bool? _isErrorCrop;

	[XmlIgnore]
	public Vector2 drawPosition;

	[XmlIgnore]
	public Vector2 tilePosition;

	[XmlIgnore]
	public float layerDepth;

	[XmlIgnore]
	public float coloredLayerDepth;

	[XmlIgnore]
	public Rectangle sourceRect;

	[XmlIgnore]
	public Rectangle coloredSourceRect;

	private static Vector2 origin = new Vector2(8f, 24f);

	private static Vector2 smallestTileSizeOrigin = new Vector2(8f, 8f);

	/// <summary>The location containing the crop.</summary>
	[XmlIgnore]
	public GameLocation currentLocation
	{
		get
		{
			return currentLocationImpl;
		}
		set
		{
			if (value != currentLocationImpl)
			{
				currentLocationImpl = value;
				updateDrawMath(tilePosition);
			}
		}
	}

	/// <summary>The dirt which contains this crop.</summary>
	[XmlIgnore]
	public HoeDirt Dirt { get; set; }

	[XmlIgnore]
	public Texture2D DrawnCropTexture
	{
		get
		{
			if (dead.Value)
			{
				return Game1.cropSpriteSheet;
			}
			if (_drawnTexture == null)
			{
				if (overrideTexturePath.Value == null)
				{
					overrideTexturePath.Value = GetData()?.GetCustomTextureName("TileSheets\\crops");
				}
				_drawnTexture = null;
				if (overrideTexturePath.Value != null)
				{
					try
					{
						_drawnTexture = Game1.content.Load<Texture2D>(overrideTexturePath);
					}
					catch (Exception)
					{
						_drawnTexture = null;
					}
				}
				if (_drawnTexture == null)
				{
					_drawnTexture = Game1.cropSpriteSheet;
				}
			}
			return _drawnTexture;
		}
	}

	/// <inheritdoc />
	[XmlIgnore]
	public ModDataDictionary modData { get; } = new ModDataDictionary();


	/// <inheritdoc />
	[XmlElement("modData")]
	public ModDataDictionary modDataForSerialization
	{
		get
		{
			return modData.GetForSerialization();
		}
		set
		{
			modData.SetFromSerialization(value);
		}
	}

	public NetFields NetFields { get; } = new NetFields("Crop");


	public Crop()
	{
		NetFields.SetOwner(this).AddField(phaseDays, "phaseDays").AddField(rowInSpriteSheet, "rowInSpriteSheet")
			.AddField(phaseToShow, "phaseToShow")
			.AddField(currentPhase, "currentPhase")
			.AddField(indexOfHarvest, "indexOfHarvest")
			.AddField(dayOfCurrentPhase, "dayOfCurrentPhase")
			.AddField(whichForageCrop, "whichForageCrop")
			.AddField(tintColor, "tintColor")
			.AddField(flip, "flip")
			.AddField(fullyGrown, "fullyGrown")
			.AddField(raisedSeeds, "raisedSeeds")
			.AddField(programColored, "programColored")
			.AddField(dead, "dead")
			.AddField(forageCrop, "forageCrop")
			.AddField(netSeedIndex, "netSeedIndex")
			.AddField(overrideTexturePath, "overrideTexturePath")
			.AddField(modData, "modData");
		dayOfCurrentPhase.fieldChangeVisibleEvent += delegate
		{
			updateDrawMath(tilePosition);
		};
		fullyGrown.fieldChangeVisibleEvent += delegate
		{
			updateDrawMath(tilePosition);
		};
		currentLocation = Game1.currentLocation;
	}

	public Crop(bool forageCrop, string which, int tileX, int tileY, GameLocation location)
		: this()
	{
		currentLocation = location;
		this.forageCrop.Value = forageCrop;
		whichForageCrop.Value = which;
		fullyGrown.Value = true;
		currentPhase.Value = 5;
		updateDrawMath(new Vector2(tileX, tileY));
	}

	public Crop(string seedId, int tileX, int tileY, GameLocation location)
		: this()
	{
		currentLocation = location;
		seedId = ResolveSeedId(seedId, location);
		if (TryGetData(seedId, out var data))
		{
			ParsedItemData harvestItemData = ItemRegistry.GetDataOrErrorItem(data.HarvestItemId);
			if (!harvestItemData.HasTypeObject())
			{
				Game1.log.Warn($"Crop seed {seedId} produces non-object item {harvestItemData.QualifiedItemId}, which isn't valid.");
			}
			phaseDays.AddRange(data.DaysInPhase);
			phaseDays.Add(99999);
			rowInSpriteSheet.Value = data.SpriteIndex;
			indexOfHarvest.Value = harvestItemData.ItemId;
			overrideTexturePath.Value = data.GetCustomTextureName("TileSheets\\crops");
			if (isWildSeedCrop())
			{
				whichForageCrop.Value = seedId;
			}
			else
			{
				netSeedIndex.Value = seedId;
			}
			raisedSeeds.Value = data.IsRaised;
			List<string> tintColors = data.TintColors;
			if (tintColors != null && tintColors.Count > 0)
			{
				Color? color = Utility.StringToColor(Utility.CreateRandom((double)tileX * 1000.0, tileY, Game1.dayOfMonth).ChooseFrom(data.TintColors));
				if (color.HasValue)
				{
					tintColor.Value = color.Value;
					programColored.Value = true;
				}
			}
		}
		else
		{
			netSeedIndex.Value = seedId ?? "0";
			indexOfHarvest.Value = seedId ?? "0";
		}
		flip.Value = Game1.random.NextBool();
		updateDrawMath(new Vector2(tileX, tileY));
	}

	/// <summary>Choose a random seed from a bag of mixed seeds, if applicable.</summary>
	/// <param name="itemId">The unqualified item ID for the seed item.</param>
	/// <param name="location">The location for which to resolve the crop.</param>
	/// <returns>Returns the unqualified seed ID to use.</returns>
	public static string ResolveSeedId(string itemId, GameLocation location)
	{
		if (!(itemId == "MixedFlowerSeeds"))
		{
			if (itemId == "770")
			{
				string seedId = getRandomLowGradeCropForThisSeason(location.GetSeason());
				if (seedId == "473")
				{
					seedId = "472";
				}
				if (location is IslandLocation)
				{
					seedId = Game1.random.Next(4) switch
					{
						0 => "479", 
						1 => "833", 
						2 => "481", 
						_ => "478", 
					};
				}
				return seedId;
			}
			return itemId;
		}
		return getRandomFlowerSeedForThisSeason(location.GetSeason());
	}

	/// <summary>Get the crop's data from <see cref="F:StardewValley.Game1.cropData" />, if found.</summary>
	public CropData GetData()
	{
		if (!TryGetData(isWildSeedCrop() ? whichForageCrop.Value : netSeedIndex.Value, out var data))
		{
			return null;
		}
		return data;
	}

	/// <summary>Try to get a crop's data from <see cref="F:StardewValley.Game1.cropData" />.</summary>
	/// <param name="seedId">The unqualified item ID for the crop's seed (i.e. the key in <see cref="F:StardewValley.Game1.cropData" />).</param>
	/// <param name="data">The crop data, if found.</param>
	/// <returns>Returns whether the crop data was found.</returns>
	public static bool TryGetData(string seedId, out CropData data)
	{
		if (seedId == null)
		{
			data = null;
			return false;
		}
		return Game1.cropData.TryGetValue(seedId, out data);
	}

	/// <summary>Get whether this crop is in season for the given location.</summary>
	/// <param name="location">The location to check.</param>
	public bool IsInSeason(GameLocation location)
	{
		if (location.SeedsIgnoreSeasonsHere())
		{
			return true;
		}
		return GetData()?.Seasons?.Contains(location.GetSeason()) ?? false;
	}

	/// <summary>Get whether a crop is in season for the given location.</summary>
	/// <param name="location">The location to check.</param>
	/// <param name="seedId">The unqualified item ID for the crop's seed.</param>
	public static bool IsInSeason(GameLocation location, string seedId)
	{
		if (location.SeedsIgnoreSeasonsHere())
		{
			return true;
		}
		if (TryGetData(seedId, out var data))
		{
			return data.Seasons?.Contains(location.GetSeason()) ?? false;
		}
		return false;
	}

	/// <summary>Get the method by which the crop can be harvested.</summary>
	public HarvestMethod GetHarvestMethod()
	{
		return GetData()?.HarvestMethod ?? HarvestMethod.Grab;
	}

	/// <summary>Get whether this crop regrows after it's harvested.</summary>
	public bool RegrowsAfterHarvest()
	{
		CropData data = GetData();
		if (data == null)
		{
			return false;
		}
		return data.RegrowDays > 0;
	}

	public virtual bool IsErrorCrop()
	{
		if (forageCrop.Value)
		{
			return false;
		}
		if (!_isErrorCrop.HasValue)
		{
			_isErrorCrop = GetData() == null;
		}
		return _isErrorCrop.Value;
	}

	public virtual void ResetPhaseDays()
	{
		CropData data = GetData();
		if (data != null)
		{
			phaseDays.Clear();
			phaseDays.AddRange(data.DaysInPhase);
			phaseDays.Add(99999);
		}
	}

	public static string getRandomLowGradeCropForThisSeason(Season season)
	{
		if (season == Season.Winter)
		{
			season = Game1.random.Choose(Season.Spring, Season.Summer, Season.Fall);
		}
		return season switch
		{
			Season.Spring => Game1.random.Next(472, 476).ToString(), 
			Season.Summer => Game1.random.Next(4) switch
			{
				0 => "487", 
				1 => "483", 
				2 => "482", 
				_ => "484", 
			}, 
			Season.Fall => Game1.random.Next(487, 491).ToString(), 
			_ => null, 
		};
	}

	public static string getRandomFlowerSeedForThisSeason(Season season)
	{
		if (season == Season.Winter)
		{
			season = Game1.random.Choose(Season.Spring, Season.Summer, Season.Fall);
		}
		return season switch
		{
			Season.Spring => Game1.random.Choose("427", "429"), 
			Season.Summer => Game1.random.Choose("455", "453", "431"), 
			Season.Fall => Game1.random.Choose("431", "425"), 
			_ => "-1", 
		};
	}

	public virtual void growCompletely()
	{
		currentPhase.Value = phaseDays.Count - 1;
		dayOfCurrentPhase.Value = 0;
		if (RegrowsAfterHarvest())
		{
			fullyGrown.Value = true;
		}
		updateDrawMath(tilePosition);
	}

	public virtual bool hitWithHoe(int xTile, int yTile, GameLocation location, HoeDirt dirt)
	{
		if ((bool)forageCrop && whichForageCrop == "2")
		{
			dirt.state.Value = (location.IsRainingHere() ? 1 : 0);
			Object harvestedItem = ItemRegistry.Create<Object>("(O)829");
			Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(12, new Vector2(xTile * 64, yTile * 64), Color.White, 8, Game1.random.NextBool(), 50f));
			location.playSound("dirtyHit");
			Game1.createItemDebris(harvestedItem.getOne(), new Vector2(xTile * 64 + 32, yTile * 64 + 32), -1);
			return true;
		}
		return false;
	}

	public virtual bool harvest(int xTile, int yTile, HoeDirt soil, JunimoHarvester junimoHarvester = null, bool isForcedScytheHarvest = false)
	{
		if ((bool)dead)
		{
			if (junimoHarvester != null)
			{
				return true;
			}
			return false;
		}
		bool success = false;
		if ((bool)forageCrop)
		{
			Object o = null;
			int experience = 3;
			Random r = Utility.CreateDaySaveRandom(xTile * 1000, yTile * 2000);
			if (whichForageCrop == "1")
			{
				o = ItemRegistry.Create<Object>("(O)399");
			}
			else if (whichForageCrop == "2")
			{
				soil.shake((float)Math.PI / 48f, (float)Math.PI / 40f, (float)(xTile * 64) < Game1.player.Position.X);
				return false;
			}
			if (Game1.player.professions.Contains(16))
			{
				o.Quality = 4;
			}
			else if (r.NextDouble() < (double)((float)Game1.player.ForagingLevel / 30f))
			{
				o.Quality = 2;
			}
			else if (r.NextDouble() < (double)((float)Game1.player.ForagingLevel / 15f))
			{
				o.Quality = 1;
			}
			Game1.stats.ItemsForaged += (uint)o.Stack;
			if (junimoHarvester != null)
			{
				junimoHarvester.tryToAddItemToHut(o);
				return true;
			}
			if (isForcedScytheHarvest)
			{
				Vector2 initialTile = new Vector2(xTile, yTile);
				Game1.createItemDebris(o, new Vector2(initialTile.X * 64f + 32f, initialTile.Y * 64f + 32f), -1);
				Game1.player.gainExperience(2, experience);
				Game1.player.currentLocation.playSound("moss_cut");
				return true;
			}
			if (Game1.player.addItemToInventoryBool(o))
			{
				Vector2 initialTile = new Vector2(xTile, yTile);
				Game1.player.animateOnce(279 + Game1.player.FacingDirection);
				Game1.player.canMove = false;
				Game1.player.currentLocation.playSound("harvest");
				DelayedAction.playSoundAfterDelay("coin", 260);
				if (!RegrowsAfterHarvest())
				{
					Game1.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite(17, new Vector2(initialTile.X * 64f, initialTile.Y * 64f), Color.White, 7, r.NextBool(), 125f));
					Game1.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite(14, new Vector2(initialTile.X * 64f, initialTile.Y * 64f), Color.White, 7, r.NextBool(), 50f));
				}
				Game1.player.gainExperience(2, experience);
				return true;
			}
			Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
		}
		else if ((int)currentPhase >= phaseDays.Count - 1 && (!fullyGrown || (int)dayOfCurrentPhase <= 0))
		{
			if (indexOfHarvest == null)
			{
				return true;
			}
			CropData data = GetData();
			Random r = Utility.CreateRandom((double)xTile * 7.0, (double)yTile * 11.0, Game1.stats.DaysPlayed, Game1.uniqueIDForThisGame);
			int fertilizerQualityLevel = soil.GetFertilizerQualityBoostLevel();
			double chanceForGoldQuality = 0.2 * ((double)Game1.player.FarmingLevel / 10.0) + 0.2 * (double)fertilizerQualityLevel * (((double)Game1.player.FarmingLevel + 2.0) / 12.0) + 0.01;
			double chanceForSilverQuality = Math.Min(0.75, chanceForGoldQuality * 2.0);
			int cropQuality = 0;
			if (fertilizerQualityLevel >= 3 && r.NextDouble() < chanceForGoldQuality / 2.0)
			{
				cropQuality = 4;
			}
			else if (r.NextDouble() < chanceForGoldQuality)
			{
				cropQuality = 2;
			}
			else if (r.NextDouble() < chanceForSilverQuality || fertilizerQualityLevel >= 3)
			{
				cropQuality = 1;
			}
			cropQuality = MathHelper.Clamp(cropQuality, data?.HarvestMinQuality ?? 0, data?.HarvestMaxQuality ?? cropQuality);
			int numToHarvest = 1;
			if (data != null)
			{
				int minStack = data.HarvestMinStack;
				int maxStack = Math.Max(minStack, data.HarvestMaxStack);
				if (data.HarvestMaxIncreasePerFarmingLevel > 0f)
				{
					maxStack += (int)((float)Game1.player.FarmingLevel * data.HarvestMaxIncreasePerFarmingLevel);
				}
				if (minStack > 1 || maxStack > 1)
				{
					numToHarvest = r.Next(minStack, maxStack + 1);
				}
			}
			if (data != null && data.ExtraHarvestChance > 0.0)
			{
				while (r.NextDouble() < Math.Min(0.9, data.ExtraHarvestChance))
				{
					numToHarvest++;
				}
			}
			Item harvestedItem = (programColored ? new ColoredObject(indexOfHarvest, 1, tintColor.Value)
			{
				Quality = cropQuality
			} : ItemRegistry.Create(indexOfHarvest, 1, cropQuality));
			HarvestMethod harvestMethod = data?.HarvestMethod ?? HarvestMethod.Grab;
			if (harvestMethod == HarvestMethod.Scythe || isForcedScytheHarvest)
			{
				if (junimoHarvester != null)
				{
					DelayedAction.playSoundAfterDelay("daggerswipe", 150, junimoHarvester.currentLocation);
					if (Utility.isOnScreen(junimoHarvester.TilePoint, 64, junimoHarvester.currentLocation))
					{
						junimoHarvester.currentLocation.playSound("harvest");
						DelayedAction.playSoundAfterDelay("coin", 260, junimoHarvester.currentLocation);
					}
					junimoHarvester.tryToAddItemToHut(harvestedItem.getOne());
				}
				else
				{
					Game1.createItemDebris(harvestedItem.getOne(), new Vector2(xTile * 64 + 32, yTile * 64 + 32), -1);
				}
				success = true;
			}
			else if (junimoHarvester != null || (harvestedItem != null && Game1.player.addItemToInventoryBool(harvestedItem.getOne())))
			{
				Vector2 initialTile = new Vector2(xTile, yTile);
				if (junimoHarvester == null)
				{
					Game1.player.animateOnce(279 + Game1.player.FacingDirection);
					Game1.player.canMove = false;
				}
				else
				{
					junimoHarvester.tryToAddItemToHut(harvestedItem.getOne());
				}
				if (r.NextDouble() < Game1.player.team.AverageLuckLevel() / 1500.0 + Game1.player.team.AverageDailyLuck() / 1200.0 + 9.999999747378752E-05)
				{
					numToHarvest *= 2;
					if (junimoHarvester == null)
					{
						Game1.player.currentLocation.playSound("dwoop");
					}
					else if (Utility.isOnScreen(junimoHarvester.TilePoint, 64, junimoHarvester.currentLocation))
					{
						junimoHarvester.currentLocation.playSound("dwoop");
					}
				}
				else if (harvestMethod == HarvestMethod.Grab)
				{
					if (junimoHarvester == null)
					{
						Game1.player.currentLocation.playSound("harvest");
					}
					else if (Utility.isOnScreen(junimoHarvester.TilePoint, 64, junimoHarvester.currentLocation))
					{
						junimoHarvester.currentLocation.playSound("harvest");
					}
					if (junimoHarvester == null)
					{
						DelayedAction.playSoundAfterDelay("coin", 260, Game1.player.currentLocation);
					}
					else if (Utility.isOnScreen(junimoHarvester.TilePoint, 64, junimoHarvester.currentLocation))
					{
						DelayedAction.playSoundAfterDelay("coin", 260, junimoHarvester.currentLocation);
					}
					if (!RegrowsAfterHarvest() && (junimoHarvester == null || junimoHarvester.currentLocation.Equals(Game1.currentLocation)))
					{
						Game1.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite(17, new Vector2(initialTile.X * 64f, initialTile.Y * 64f), Color.White, 7, Game1.random.NextBool(), 125f));
						Game1.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite(14, new Vector2(initialTile.X * 64f, initialTile.Y * 64f), Color.White, 7, Game1.random.NextBool(), 50f));
					}
				}
				success = true;
			}
			else
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
			}
			if (success)
			{
				if (indexOfHarvest == "421")
				{
					indexOfHarvest.Value = "431";
					numToHarvest = r.Next(1, 4);
				}
				harvestedItem = (programColored ? new ColoredObject(indexOfHarvest, 1, tintColor.Value) : ItemRegistry.Create(indexOfHarvest));
				int price = 0;
				if (harvestedItem is Object obj)
				{
					price = obj.Price;
				}
				float experience = (float)(16.0 * Math.Log(0.018 * (double)price + 1.0, Math.E));
				if (junimoHarvester == null)
				{
					Game1.player.gainExperience(0, (int)Math.Round(experience));
				}
				for (int i = 0; i < numToHarvest - 1; i++)
				{
					if (junimoHarvester == null)
					{
						Game1.createItemDebris(harvestedItem.getOne(), new Vector2(xTile * 64 + 32, yTile * 64 + 32), -1);
					}
					else
					{
						junimoHarvester.tryToAddItemToHut(harvestedItem.getOne());
					}
				}
				if (indexOfHarvest == "262" && r.NextDouble() < 0.4)
				{
					Item hay_item = ItemRegistry.Create("(O)178");
					if (junimoHarvester == null)
					{
						Game1.createItemDebris(hay_item.getOne(), new Vector2(xTile * 64 + 32, yTile * 64 + 32), -1);
					}
					else
					{
						junimoHarvester.tryToAddItemToHut(hay_item.getOne());
					}
				}
				else if (indexOfHarvest == "771")
				{
					soil?.Location?.playSound("cut");
					if (r.NextDouble() < 0.1)
					{
						Item mixedSeeds = ItemRegistry.Create("(O)770");
						if (junimoHarvester == null)
						{
							Game1.createItemDebris(mixedSeeds.getOne(), new Vector2(xTile * 64 + 32, yTile * 64 + 32), -1);
						}
						else
						{
							junimoHarvester.tryToAddItemToHut(mixedSeeds.getOne());
						}
					}
				}
				int regrowDays = data?.RegrowDays ?? (-1);
				if (regrowDays <= 0)
				{
					return true;
				}
				fullyGrown.Value = true;
				if (dayOfCurrentPhase.Value == regrowDays)
				{
					updateDrawMath(tilePosition);
				}
				dayOfCurrentPhase.Value = regrowDays;
			}
		}
		return false;
	}

	public virtual string getRandomWildCropForSeason(Season season)
	{
		return season switch
		{
			Season.Spring => Game1.random.Choose("(O)16", "(O)18", "(O)20", "(O)22"), 
			Season.Summer => Game1.random.Choose("(O)396", "(O)398", "(O)402"), 
			Season.Fall => Game1.random.Choose("(O)404", "(O)406", "(O)408", "(O)410"), 
			Season.Winter => Game1.random.Choose("(O)412", "(O)414", "(O)416", "(O)418"), 
			_ => "(O)22", 
		};
	}

	public virtual Rectangle getSourceRect(int number)
	{
		if ((bool)dead)
		{
			return new Rectangle(192 + number % 4 * 16, 384, 16, 32);
		}
		int effectiveRow = rowInSpriteSheet;
		Season localSeason = Game1.GetSeasonForLocation(currentLocation);
		if (indexOfHarvest == "771")
		{
			switch (localSeason)
			{
			case Season.Fall:
				effectiveRow = (int)rowInSpriteSheet + 1;
				break;
			case Season.Winter:
				effectiveRow = (int)rowInSpriteSheet + 2;
				break;
			}
		}
		return new Rectangle(Math.Min(240, ((!fullyGrown) ? ((int)(((int)phaseToShow != -1) ? phaseToShow : currentPhase) + (((int)(((int)phaseToShow != -1) ? phaseToShow : currentPhase) == 0 && number % 2 == 0) ? (-1) : 0) + 1) : (((int)dayOfCurrentPhase <= 0) ? 6 : 7)) * 16 + ((effectiveRow % 2 != 0) ? 128 : 0)), effectiveRow / 2 * 16 * 2, 16, 32);
	}

	/// <summary>Get the giant crops which can grow from this crop, if any.</summary>
	/// <param name="giantCrops">The giant crops which can grow from this crop.</param>
	/// <returns>Returns whether <paramref name="giantCrops" /> is non-empty.</returns>
	public bool TryGetGiantCrops(out IReadOnlyList<KeyValuePair<string, GiantCropData>> giantCrops)
	{
		giantCrops = GiantCrop.GetGiantCropsFor("(O)" + indexOfHarvest.Value);
		return giantCrops.Count > 0;
	}

	public void Kill()
	{
		dead.Value = true;
		raisedSeeds.Value = false;
	}

	public virtual void newDay(int state)
	{
		GameLocation environment = currentLocation;
		Vector2 tileVector = tilePosition;
		Point tile = Utility.Vector2ToPoint(tileVector);
		if ((bool)environment.isOutdoors && ((bool)dead || !IsInSeason(environment)))
		{
			Kill();
			return;
		}
		if (state != 1)
		{
			CropData data = GetData();
			if (data == null || data.NeedsWatering)
			{
				goto IL_0405;
			}
		}
		if (!fullyGrown)
		{
			dayOfCurrentPhase.Value = Math.Min((int)dayOfCurrentPhase + 1, (phaseDays.Count > 0) ? phaseDays[Math.Min(phaseDays.Count - 1, currentPhase)] : 0);
		}
		else
		{
			dayOfCurrentPhase.Value--;
		}
		if ((int)dayOfCurrentPhase >= ((phaseDays.Count > 0) ? phaseDays[Math.Min(phaseDays.Count - 1, currentPhase)] : 0) && (int)currentPhase < phaseDays.Count - 1)
		{
			currentPhase.Value++;
			dayOfCurrentPhase.Value = 0;
		}
		while ((int)currentPhase < phaseDays.Count - 1 && phaseDays.Count > 0 && phaseDays[currentPhase] <= 0)
		{
			currentPhase.Value++;
		}
		if (isWildSeedCrop() && (int)phaseToShow == -1 && (int)currentPhase > 0)
		{
			phaseToShow.Value = Game1.random.Next(1, 7);
		}
		if ((environment is Farm || environment.HasMapPropertyWithValue("AllowGiantCrops")) && (int)currentPhase == phaseDays.Count - 1 && TryGetGiantCrops(out var possibleGiantCrops))
		{
			foreach (KeyValuePair<string, GiantCropData> pair in possibleGiantCrops)
			{
				string giantCropId = pair.Key;
				GiantCropData giantCrop = pair.Value;
				if ((giantCrop.Chance < 1f && !Utility.CreateDaySaveRandom(tile.X, tile.Y, Game1.hash.GetDeterministicHashCode(giantCropId)).NextBool(giantCrop.Chance)) || !GameStateQuery.CheckConditions(giantCrop.Condition, environment))
				{
					continue;
				}
				bool valid = true;
				for (int y = tile.Y; y < tile.Y + giantCrop.TileSize.Y; y++)
				{
					for (int x = tile.X; x < tile.X + giantCrop.TileSize.X; x++)
					{
						Vector2 v = new Vector2(x, y);
						if (!environment.terrainFeatures.TryGetValue(v, out var terrainFeature) || !(terrainFeature is HoeDirt dirt) || dirt.crop?.indexOfHarvest != indexOfHarvest)
						{
							valid = false;
							break;
						}
					}
					if (!valid)
					{
						break;
					}
				}
				if (!valid)
				{
					continue;
				}
				for (int y = tile.Y; y < tile.Y + giantCrop.TileSize.Y; y++)
				{
					for (int x = tile.X; x < tile.X + giantCrop.TileSize.X; x++)
					{
						Vector2 v = new Vector2(x, y);
						((HoeDirt)environment.terrainFeatures[v]).crop = null;
					}
				}
				environment.resourceClumps.Add(new GiantCrop(giantCropId, tileVector));
				break;
			}
		}
		goto IL_0405;
		IL_0405:
		if ((!fullyGrown || (int)dayOfCurrentPhase <= 0) && (int)currentPhase >= phaseDays.Count - 1)
		{
			if (isWildSeedCrop())
			{
				Season seedSeason = (string)whichForageCrop switch
				{
					"495" => Season.Spring, 
					"496" => Season.Summer, 
					"497" => Season.Fall, 
					"498" => Season.Winter, 
					_ => currentLocation.GetSeason(), 
				};
				if (environment.objects.TryGetValue(tileVector, out var obj))
				{
					if (obj is IndoorPot pot)
					{
						pot.heldObject.Value = ItemRegistry.Create<Object>(getRandomWildCropForSeason(seedSeason));
						pot.hoeDirt.Value.crop = null;
					}
					else
					{
						environment.objects.Remove(tileVector);
					}
				}
				if (!environment.objects.ContainsKey(tileVector))
				{
					Object spawned = ItemRegistry.Create<Object>(getRandomWildCropForSeason(seedSeason));
					spawned.IsSpawnedObject = true;
					spawned.CanBeGrabbed = true;
					spawned.SpecialVariable = 724519;
					environment.objects.Add(tileVector, spawned);
				}
				if (environment.terrainFeatures.TryGetValue(tileVector, out var terrainFeature) && terrainFeature is HoeDirt dirt)
				{
					dirt.crop = null;
				}
			}
			if (indexOfHarvest != null && indexOfHarvest.Value != null && indexOfHarvest.Value.Length > 0 && environment.IsFarm)
			{
				foreach (Farmer allFarmer in Game1.getAllFarmers())
				{
					allFarmer.autoGenerateActiveDialogueEvent("cropMatured_" + indexOfHarvest);
				}
			}
		}
		if ((bool)fullyGrown && indexOfHarvest != null && indexOfHarvest.Value != null && indexOfHarvest.Value == "595")
		{
			Game1.getFarm().hasMatureFairyRoseTonight = true;
		}
		updateDrawMath(tileVector);
	}

	public virtual bool isPaddyCrop()
	{
		return GetData()?.IsPaddyCrop ?? false;
	}

	public virtual bool shouldDrawDarkWhenWatered()
	{
		if (isPaddyCrop())
		{
			return false;
		}
		return !raisedSeeds.Value;
	}

	/// <summary>Get whether this is a vanilla wild seed crop.</summary>
	public virtual bool isWildSeedCrop()
	{
		if (overrideTexturePath.Value == null || overrideTexturePath.Value == Game1.cropSpriteSheet.Name)
		{
			return rowInSpriteSheet.Value == 23;
		}
		return false;
	}

	public virtual void updateDrawMath(Vector2 tileLocation)
	{
		if (tileLocation.Equals(Vector2.Zero))
		{
			return;
		}
		if ((bool)forageCrop)
		{
			if (!int.TryParse(whichForageCrop.Value, out var which_forage_crop))
			{
				which_forage_crop = 1;
			}
			drawPosition = new Vector2(tileLocation.X * 64f + ((tileLocation.X * 11f + tileLocation.Y * 7f) % 10f - 5f) + 32f, tileLocation.Y * 64f + ((tileLocation.Y * 11f + tileLocation.X * 7f) % 10f - 5f) + 32f);
			layerDepth = (tileLocation.Y * 64f + 32f + ((tileLocation.Y * 11f + tileLocation.X * 7f) % 10f - 5f)) / 10000f;
			sourceRect = new Rectangle((int)(tileLocation.X * 51f + tileLocation.Y * 77f) % 3 * 16, 128 + which_forage_crop * 16, 16, 16);
		}
		else
		{
			drawPosition = new Vector2(tileLocation.X * 64f + ((!shouldDrawDarkWhenWatered() || (int)currentPhase >= phaseDays.Count - 1) ? 0f : ((tileLocation.X * 11f + tileLocation.Y * 7f) % 10f - 5f)) + 32f, tileLocation.Y * 64f + (((bool)raisedSeeds || (int)currentPhase >= phaseDays.Count - 1) ? 0f : ((tileLocation.Y * 11f + tileLocation.X * 7f) % 10f - 5f)) + 32f);
			layerDepth = (tileLocation.Y * 64f + 32f + ((!shouldDrawDarkWhenWatered() || (int)currentPhase >= phaseDays.Count - 1) ? 0f : ((tileLocation.Y * 11f + tileLocation.X * 7f) % 10f - 5f))) / 10000f / (((int)currentPhase == 0 && shouldDrawDarkWhenWatered()) ? 2f : 1f);
			sourceRect = getSourceRect((int)tileLocation.X * 7 + (int)tileLocation.Y * 11);
			coloredSourceRect = new Rectangle(((!fullyGrown) ? ((int)currentPhase + 1 + 1) : (((int)dayOfCurrentPhase <= 0) ? 6 : 7)) * 16 + (((int)rowInSpriteSheet % 2 != 0) ? 128 : 0), (int)rowInSpriteSheet / 2 * 16 * 2, 16, 32);
			coloredLayerDepth = (tileLocation.Y * 64f + 32f + ((tileLocation.Y * 11f + tileLocation.X * 7f) % 10f - 5f)) / 10000f / (float)(((int)currentPhase != 0 || !shouldDrawDarkWhenWatered()) ? 1 : 2);
		}
		tilePosition = tileLocation;
	}

	public virtual void draw(SpriteBatch b, Vector2 tileLocation, Color toTint, float rotation)
	{
		Vector2 position = Game1.GlobalToLocal(Game1.viewport, drawPosition);
		if ((bool)forageCrop)
		{
			if (whichForageCrop == "2")
			{
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + ((tileLocation.X * 11f + tileLocation.Y * 7f) % 10f - 5f) + 32f, tileLocation.Y * 64f + ((tileLocation.Y * 11f + tileLocation.X * 7f) % 10f - 5f) + 64f)), new Rectangle(128 + (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(tileLocation.X * 111f + tileLocation.Y * 77f)) % 800.0 / 200.0) * 16, 128, 16, 16), Color.White, rotation, new Vector2(8f, 16f), 4f, SpriteEffects.None, (tileLocation.Y * 64f + 32f + ((tileLocation.Y * 11f + tileLocation.X * 7f) % 10f - 5f)) / 10000f);
			}
			else
			{
				b.Draw(Game1.mouseCursors, position, sourceRect, Color.White, 0f, smallestTileSizeOrigin, 4f, SpriteEffects.None, layerDepth);
			}
			return;
		}
		if (IsErrorCrop())
		{
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem("(O)" + indexOfHarvest);
			b.Draw(itemData.GetTexture(), position, itemData.GetSourceRect(), toTint, rotation, new Vector2(8f, 8f), 4f, SpriteEffects.None, layerDepth);
			return;
		}
		SpriteEffects effect = (flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
		b.Draw(DrawnCropTexture, position, sourceRect, toTint, rotation, origin, 4f, effect, layerDepth);
		Color tintColor = this.tintColor.Value;
		if (!tintColor.Equals(Color.White) && (int)currentPhase == phaseDays.Count - 1 && !dead)
		{
			b.Draw(DrawnCropTexture, position, coloredSourceRect, tintColor, rotation, origin, 4f, effect, coloredLayerDepth);
		}
	}

	public virtual void drawInMenu(SpriteBatch b, Vector2 screenPosition, Color toTint, float rotation, float scale, float layerDepth)
	{
		if (IsErrorCrop())
		{
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem("(O)" + indexOfHarvest);
			b.Draw(itemData.GetTexture(), screenPosition, itemData.GetSourceRect(), toTint, rotation, new Vector2(32f, 32f), scale, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
		}
		else
		{
			b.Draw(DrawnCropTexture, screenPosition, getSourceRect(0), toTint, rotation, new Vector2(32f, 96f), scale, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
		}
	}

	public virtual void drawWithOffset(SpriteBatch b, Vector2 tileLocation, Color toTint, float rotation, Vector2 offset)
	{
		if (IsErrorCrop())
		{
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem("(O)" + indexOfHarvest);
			b.Draw(itemData.GetTexture(), Game1.GlobalToLocal(Game1.viewport, offset + new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f)), itemData.GetSourceRect(), toTint, rotation, new Vector2(8f, 8f), 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (tileLocation.Y + 0.66f) * 64f / 10000f + tileLocation.X * 1E-05f);
			return;
		}
		if ((bool)forageCrop)
		{
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f)), sourceRect, Color.White, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, (tileLocation.Y + 0.66f) * 64f / 10000f + tileLocation.X * 1E-05f);
			return;
		}
		b.Draw(DrawnCropTexture, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f)), sourceRect, toTint, rotation, new Vector2(8f, 24f), 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (tileLocation.Y + 0.66f) * 64f / 10000f + tileLocation.X * 1E-05f);
		if (!tintColor.Equals(Color.White) && (int)currentPhase == phaseDays.Count - 1 && !dead)
		{
			b.Draw(DrawnCropTexture, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f)), coloredSourceRect, tintColor.Value, rotation, new Vector2(8f, 24f), 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (tileLocation.Y + 0.67f) * 64f / 10000f + tileLocation.X * 1E-05f);
		}
	}
}
