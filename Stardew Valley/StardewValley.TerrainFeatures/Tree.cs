using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Constants;
using StardewValley.Enchantments;
using StardewValley.Extensions;
using StardewValley.GameData.WildTrees;
using StardewValley.Internal;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Tools;
using xTile.Dimensions;

namespace StardewValley.TerrainFeatures;

public class Tree : TerrainFeature
{
	/// <remarks>The backing field for <see cref="M:StardewValley.TerrainFeatures.Tree.GetWildTreeDataDictionary" />.</remarks>
	protected static Dictionary<string, WildTreeData> _WildTreeData;

	/// <summary>The backing field for <see cref="M:StardewValley.TerrainFeatures.Tree.GetWildTreeSeedLookup" />.</summary>
	protected static Dictionary<string, List<string>> _WildTreeSeedLookup;

	public const float chanceForDailySeed = 0.05f;

	public const float shakeRate = (float)Math.PI / 200f;

	public const float shakeDecayRate = 0.0030679617f;

	public const int minWoodDebrisForFallenTree = 12;

	public const int minWoodDebrisForStump = 5;

	public const int startingHealth = 10;

	public const int leafFallRate = 3;

	public const int stageForMossGrowth = 14;

	/// <summary>The oak tree type ID in <c>Data/WildTrees</c>.</summary>
	public const string bushyTree = "1";

	/// <summary>The maple tree type ID in <c>Data/WildTrees</c>.</summary>
	public const string leafyTree = "2";

	/// <summary>The pine tree type ID in <c>Data/WildTrees</c>.</summary>
	public const string pineTree = "3";

	public const string winterTree1 = "4";

	public const string winterTree2 = "5";

	/// <summary>The palm tree type ID (valley variant) in <c>Data/WildTrees</c>.</summary>
	public const string palmTree = "6";

	/// <summary>The mushroom tree type ID in <c>Data/WildTrees</c>.</summary>
	public const string mushroomTree = "7";

	/// <summary>The mahogany tree type ID in <c>Data/WildTrees</c>.</summary>
	public const string mahoganyTree = "8";

	/// <summary>The palm tree type ID (Ginger Island variant) in <c>Data/WildTrees</c>.</summary>
	public const string palmTree2 = "9";

	public const string greenRainTreeBushy = "10";

	public const string greenRainTreeLeafy = "11";

	public const string greenRainTreeFern = "12";

	public const string mysticTree = "13";

	public const int seedStage = 0;

	public const int sproutStage = 1;

	public const int saplingStage = 2;

	public const int bushStage = 3;

	public const int treeStage = 5;

	/// <summary>The texture for the displayed tree sprites.</summary>
	[XmlIgnore]
	public Lazy<Texture2D> texture;

	/// <summary>The current season for the location containing the tree.</summary>
	protected Season? localSeason;

	[XmlElement("growthStage")]
	public readonly NetInt growthStage = new NetInt();

	[XmlElement("treeType")]
	public readonly NetString treeType = new NetString();

	[XmlElement("health")]
	public readonly NetFloat health = new NetFloat();

	[XmlElement("flipped")]
	public readonly NetBool flipped = new NetBool();

	[XmlElement("stump")]
	public readonly NetBool stump = new NetBool();

	[XmlElement("tapped")]
	public readonly NetBool tapped = new NetBool();

	[XmlElement("hasSeed")]
	public readonly NetBool hasSeed = new NetBool();

	[XmlElement("hasMoss")]
	public readonly NetBool hasMoss = new NetBool();

	[XmlElement("isTemporaryGreenRainTree")]
	public readonly NetBool isTemporaryGreenRainTree = new NetBool();

	[XmlIgnore]
	public readonly NetBool wasShakenToday = new NetBool();

	[XmlElement("fertilized")]
	public readonly NetBool fertilized = new NetBool();

	[XmlIgnore]
	public readonly NetBool shakeLeft = new NetBool().Interpolated(interpolate: false, wait: false);

	[XmlIgnore]
	public readonly NetBool falling = new NetBool();

	[XmlIgnore]
	public readonly NetBool destroy = new NetBool();

	[XmlIgnore]
	public float shakeRotation;

	[XmlIgnore]
	public float maxShake;

	[XmlIgnore]
	public float alpha = 1f;

	private List<Leaf> leaves = new List<Leaf>();

	[XmlIgnore]
	public readonly NetLong lastPlayerToHit = new NetLong();

	[XmlIgnore]
	public float shakeTimer;

	[XmlElement("stopGrowingMoss")]
	public readonly NetBool stopGrowingMoss = new NetBool();

	public static Microsoft.Xna.Framework.Rectangle treeTopSourceRect = new Microsoft.Xna.Framework.Rectangle(0, 0, 48, 96);

	public static Microsoft.Xna.Framework.Rectangle stumpSourceRect = new Microsoft.Xna.Framework.Rectangle(32, 96, 16, 32);

	public static Microsoft.Xna.Framework.Rectangle shadowSourceRect = new Microsoft.Xna.Framework.Rectangle(663, 1011, 41, 30);

	/// <summary>The asset name for the texture loaded by <see cref="F:StardewValley.TerrainFeatures.Tree.texture" />, if applicable.</summary>
	[XmlIgnore]
	public string TextureName { get; private set; }

	public Tree()
		: base(needsTick: true)
	{
		resetTexture();
	}

	public Tree(string id, int growthStage, bool isGreenRainTemporaryTree = false)
		: this()
	{
		this.growthStage.Value = growthStage;
		isTemporaryGreenRainTree.Value = isGreenRainTemporaryTree;
		treeType.Value = id;
		if (treeType == "4")
		{
			treeType.Value = "1";
		}
		if (treeType == "5")
		{
			treeType.Value = "2";
		}
		flipped.Value = Game1.random.NextBool();
		health.Value = 10f;
	}

	public Tree(string id)
		: this()
	{
		treeType.Value = id;
		if (treeType == "4")
		{
			treeType.Value = "1";
		}
		if (treeType == "5")
		{
			treeType.Value = "2";
		}
		flipped.Value = Game1.random.NextBool();
		health.Value = 10f;
	}

	public override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(growthStage, "growthStage").AddField(treeType, "treeType").AddField(health, "health")
			.AddField(flipped, "flipped")
			.AddField(stump, "stump")
			.AddField(tapped, "tapped")
			.AddField(hasSeed, "hasSeed")
			.AddField(fertilized, "fertilized")
			.AddField(shakeLeft, "shakeLeft")
			.AddField(falling, "falling")
			.AddField(destroy, "destroy")
			.AddField(lastPlayerToHit, "lastPlayerToHit")
			.AddField(wasShakenToday, "wasShakenToday")
			.AddField(hasMoss, "hasMoss")
			.AddField(isTemporaryGreenRainTree, "isTemporaryGreenRainTree")
			.AddField(stopGrowingMoss, "stopGrowingMoss");
		treeType.fieldChangeVisibleEvent += delegate
		{
			CheckForNewTexture();
		};
	}

	/// <summary>Get the wild tree data from <c>Data/WildTrees</c>.</summary>
	/// <remarks>This is a specialized method; most code should use <see cref="M:StardewValley.TerrainFeatures.Tree.GetData" /> or <see cref="M:StardewValley.TerrainFeatures.Tree.TryGetData(System.String,StardewValley.GameData.WildTrees.WildTreeData@)" /> instead.</remarks>
	public static Dictionary<string, WildTreeData> GetWildTreeDataDictionary()
	{
		if (_WildTreeData == null)
		{
			_LoadWildTreeData();
		}
		return _WildTreeData;
	}

	/// <summary>Get tree types indexed by their qualified and unqualified seed item IDs.</summary>
	public static Dictionary<string, List<string>> GetWildTreeSeedLookup()
	{
		if (_WildTreeSeedLookup == null)
		{
			_LoadWildTreeData();
		}
		return _WildTreeSeedLookup;
	}

	/// <summary>Load the raw wild tree data from <c>Data/WildTrees</c>.</summary>
	/// <remarks>This generally shouldn't be called directly; most code should use <see cref="M:StardewValley.TerrainFeatures.Tree.GetWildTreeDataDictionary" /> or <see cref="M:StardewValley.TerrainFeatures.Tree.GetWildTreeSeedLookup" /> instead.</remarks>
	protected static void _LoadWildTreeData()
	{
		_WildTreeData = DataLoader.WildTrees(Game1.content);
		_WildTreeSeedLookup = new Dictionary<string, List<string>>();
		foreach (KeyValuePair<string, WildTreeData> pair in _WildTreeData)
		{
			string treeId = pair.Key;
			WildTreeData treeData = pair.Value;
			if (!treeData.SeedPlantable || string.IsNullOrWhiteSpace(treeData.SeedItemId))
			{
				continue;
			}
			ItemMetadata seedData = ItemRegistry.ResolveMetadata(treeData.SeedItemId);
			if (seedData != null)
			{
				if (!_WildTreeSeedLookup.TryGetValue(seedData.QualifiedItemId, out var itemIds))
				{
					itemIds = (_WildTreeSeedLookup[seedData.QualifiedItemId] = new List<string>());
				}
				itemIds.Add(treeId);
				if (!_WildTreeSeedLookup.TryGetValue(seedData.LocalItemId, out itemIds))
				{
					itemIds = (_WildTreeSeedLookup[seedData.LocalItemId] = new List<string>());
				}
				itemIds.Add(treeId);
			}
		}
	}

	/// <summary>Get the next tree that will sprout when planting a seed item.</summary>
	/// <param name="itemId">The seed's qualified or unqualified item ID.</param>
	public static string ResolveTreeTypeFromSeed(string itemId)
	{
		ItemMetadata metadata = ItemRegistry.GetMetadata(itemId);
		if (metadata?.TypeIdentifier == "(O)" && GetWildTreeSeedLookup().TryGetValue(metadata.LocalItemId, out var possibles))
		{
			return Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.Get("wildtreesplanted") + 1).ChooseFrom(possibles);
		}
		return null;
	}

	/// <summary>Reset the cached wild tree data, so it's reloaded on the next request.</summary>
	internal static void ClearCache()
	{
		_WildTreeData = null;
		_WildTreeSeedLookup = null;
	}

	/// <summary>Reload the tree texture based on <see cref="F:StardewValley.GameData.WildTrees.WildTreeData.Textures" /> if a different texture would be selected now.</summary>
	public void CheckForNewTexture()
	{
		if (texture.IsValueCreated)
		{
			string textureName = ChooseTexture();
			if (textureName != null && textureName != TextureName)
			{
				resetTexture();
			}
		}
	}

	/// <summary>Reset the tree texture, so it'll be reselected and reloaded next time it's accessed.</summary>
	public void resetTexture()
	{
		texture = new Lazy<Texture2D>(LoadTexture);
		Texture2D LoadTexture()
		{
			TextureName = ChooseTexture();
			if (TextureName == null)
			{
				return null;
			}
			return Game1.content.Load<Texture2D>(TextureName);
		}
	}

	/// <summary>Get the tree's data from <c>Data/WildTrees</c>, if found.</summary>
	public WildTreeData GetData()
	{
		if (!TryGetData(treeType.Value, out var data))
		{
			return null;
		}
		return data;
	}

	/// <summary>Try to get a tree's data from <c>Data/WildTrees</c>.</summary>
	/// <param name="id">The tree type ID (i.e. the key in <c>Data/WildTrees</c>).</param>
	/// <param name="data">The tree data, if found.</param>
	/// <returns>Returns whether the tree data was found.</returns>
	public static bool TryGetData(string id, out WildTreeData data)
	{
		if (id == null)
		{
			data = null;
			return false;
		}
		return GetWildTreeDataDictionary().TryGetValue(id, out data);
	}

	/// <summary>Choose an applicable texture from <see cref="F:StardewValley.GameData.WildTrees.WildTreeData.Textures" />.</summary>
	protected string ChooseTexture()
	{
		WildTreeData data = GetData();
		if (data != null && data.Textures?.Count > 0)
		{
			foreach (WildTreeTextureData entry in data.Textures)
			{
				if (Location != null && Location.IsGreenhouse && entry.Season.HasValue)
				{
					if (entry.Season == Season.Spring)
					{
						return entry.Texture;
					}
				}
				else if ((!entry.Season.HasValue || entry.Season == localSeason) && (entry.Condition == null || GameStateQuery.CheckConditions(entry.Condition, Location)))
				{
					return entry.Texture;
				}
			}
			return data.Textures[0].Texture;
		}
		return null;
	}

	public override Microsoft.Xna.Framework.Rectangle getBoundingBox()
	{
		Vector2 tileLocation = Tile;
		return new Microsoft.Xna.Framework.Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64);
	}

	public override Microsoft.Xna.Framework.Rectangle getRenderBounds()
	{
		Vector2 tileLocation = Tile;
		if ((bool)stump || (int)growthStage < 5)
		{
			return new Microsoft.Xna.Framework.Rectangle((int)(tileLocation.X - 0f) * 64, (int)(tileLocation.Y - 1f) * 64, 64, 128);
		}
		return new Microsoft.Xna.Framework.Rectangle((int)(tileLocation.X - 1f) * 64, (int)(tileLocation.Y - 5f) * 64, 192, 448);
	}

	public override bool performUseAction(Vector2 tileLocation)
	{
		GameLocation location = Location;
		if (!tapped)
		{
			if (maxShake == 0f && !stump && (int)growthStage >= 3 && IsLeafy())
			{
				location.localSound("leafrustle");
			}
			shake(tileLocation, doEvenIfStillShaking: false);
		}
		if (Game1.player.ActiveObject != null && Game1.player.ActiveObject.canBePlacedHere(location, tileLocation))
		{
			return false;
		}
		return true;
	}

	private int extraWoodCalculator(Vector2 tileLocation)
	{
		Random random = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed, (double)tileLocation.X * 7.0, (double)tileLocation.Y * 11.0);
		int extraWood = 0;
		if (random.NextDouble() < Game1.player.DailyLuck)
		{
			extraWood++;
		}
		if (random.NextDouble() < (double)Game1.player.ForagingLevel / 12.5)
		{
			extraWood++;
		}
		if (random.NextDouble() < (double)Game1.player.ForagingLevel / 12.5)
		{
			extraWood++;
		}
		if (random.NextDouble() < (double)Game1.player.LuckLevel / 25.0)
		{
			extraWood++;
		}
		return extraWood;
	}

	public override bool tickUpdate(GameTime time)
	{
		GameLocation location = Location;
		Season? season = localSeason;
		if (!season.HasValue)
		{
			setSeason();
			CheckForNewTexture();
		}
		if (shakeTimer > 0f)
		{
			shakeTimer -= time.ElapsedGameTime.Milliseconds;
		}
		if ((bool)destroy)
		{
			return true;
		}
		alpha = Math.Min(1f, alpha + 0.05f);
		Vector2 tileLocation = Tile;
		if ((int)growthStage >= 5 && !falling && !stump && Game1.player.GetBoundingBox().Intersects(new Microsoft.Xna.Framework.Rectangle(64 * ((int)tileLocation.X - 1), 64 * ((int)tileLocation.Y - 5), 192, 288)))
		{
			alpha = Math.Max(0.4f, alpha - 0.09f);
		}
		if (!falling)
		{
			if ((double)Math.Abs(shakeRotation) > Math.PI / 2.0 && leaves.Count <= 0 && health.Value <= 0f)
			{
				return true;
			}
			if (maxShake > 0f)
			{
				if ((bool)shakeLeft)
				{
					shakeRotation -= (((int)growthStage >= 5) ? 0.005235988f : ((float)Math.PI / 200f));
					if (shakeRotation <= 0f - maxShake)
					{
						shakeLeft.Value = false;
					}
				}
				else
				{
					shakeRotation += (((int)growthStage >= 5) ? 0.005235988f : ((float)Math.PI / 200f));
					if (shakeRotation >= maxShake)
					{
						shakeLeft.Value = true;
					}
				}
			}
			if (maxShake > 0f)
			{
				maxShake = Math.Max(0f, maxShake - (((int)growthStage >= 5) ? 0.0010226539f : 0.0030679617f));
			}
		}
		else
		{
			shakeRotation += (shakeLeft ? (0f - maxShake * maxShake) : (maxShake * maxShake));
			maxShake += 0.0015339808f;
			WildTreeData data = GetData();
			if (data != null && Game1.random.NextDouble() < 0.01 && IsLeafy())
			{
				location.localSound("leafrustle");
			}
			if ((double)Math.Abs(shakeRotation) > Math.PI / 2.0)
			{
				falling.Value = false;
				maxShake = 0f;
				if (data != null)
				{
					location.localSound("treethud");
					if (IsLeafy())
					{
						int leavesToAdd = Game1.random.Next(90, 120);
						for (int i = 0; i < leavesToAdd; i++)
						{
							leaves.Add(new Leaf(new Vector2(Game1.random.Next((int)(tileLocation.X * 64f), (int)(tileLocation.X * 64f + 192f)) + (shakeLeft ? (-320) : 256), tileLocation.Y * 64f - 64f), (float)Game1.random.Next(-10, 10) / 100f, Game1.random.Next(4), (float)Game1.random.Next(10, 40) / 10f));
						}
					}
					Random r;
					if (Game1.IsMultiplayer)
					{
						Game1.recentMultiplayerRandom = Utility.CreateRandom((double)tileLocation.X * 1000.0, tileLocation.Y);
						r = Game1.recentMultiplayerRandom;
					}
					else
					{
						r = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed, (double)tileLocation.X * 7.0, (double)tileLocation.Y * 11.0);
					}
					if (data.DropWoodOnChop)
					{
						int numToDrop = (int)((Game1.getFarmer(lastPlayerToHit.Value).professions.Contains(12) ? 1.25 : 1.0) * (double)(12 + extraWoodCalculator(tileLocation)));
						if (Game1.getFarmer(lastPlayerToHit.Value).stats.Get("Book_Woodcutting") != 0 && r.NextDouble() < 0.05)
						{
							numToDrop *= 2;
						}
						Game1.createRadialDebris(location, 12, (int)tileLocation.X + (shakeLeft ? (-4) : 4), (int)tileLocation.Y, numToDrop, resource: true);
						Game1.createRadialDebris(location, 12, (int)tileLocation.X + (shakeLeft ? (-4) : 4), (int)tileLocation.Y, (int)((Game1.getFarmer(lastPlayerToHit.Value).professions.Contains(12) ? 1.25 : 1.0) * (double)(12 + extraWoodCalculator(tileLocation))), resource: false);
					}
					Farmer targetFarmer = Game1.getFarmer(lastPlayerToHit.Value);
					if (data.DropWoodOnChop)
					{
						Game1.createMultipleObjectDebris("(O)92", (int)tileLocation.X + (shakeLeft ? (-4) : 4), (int)tileLocation.Y, 5, lastPlayerToHit.Value, location);
					}
					int numHardwood = 0;
					if (data.DropHardwoodOnLumberChop && targetFarmer != null)
					{
						while (targetFarmer.professions.Contains(14) && r.NextBool())
						{
							numHardwood++;
						}
					}
					List<WildTreeChopItemData> chopItems = data.ChopItems;
					if (chopItems != null && chopItems.Count > 0)
					{
						bool addedAdditionalHardwood = false;
						foreach (WildTreeChopItemData drop in data.ChopItems)
						{
							Item item = TryGetDrop(drop, r, targetFarmer, "ChopItems", null, false);
							if (item != null)
							{
								if (drop.ItemId == "709")
								{
									numHardwood += item.Stack;
									addedAdditionalHardwood = true;
								}
								else
								{
									Game1.createMultipleItemDebris(item, new Vector2(tileLocation.X + (float)(shakeLeft ? (-4) : 4), tileLocation.Y) * 64f, -2, location);
								}
							}
						}
						if (addedAdditionalHardwood && targetFarmer != null && targetFarmer.professions.Contains(14))
						{
							numHardwood += (int)((float)numHardwood * 0.25f + 0.9f);
						}
					}
					if (numHardwood > 0)
					{
						Game1.createMultipleObjectDebris("(O)709", (int)tileLocation.X + (shakeLeft ? (-4) : 4), (int)tileLocation.Y, numHardwood, lastPlayerToHit.Value, location);
					}
					float seedOnChopChance = data.SeedOnChopChance;
					if (Game1.getFarmer(lastPlayerToHit.Value).getEffectiveSkillLevel(2) >= 1 && data != null && data.SeedItemId != null && r.NextDouble() < (double)seedOnChopChance)
					{
						Game1.createMultipleObjectDebris(data.SeedItemId, (int)tileLocation.X + (shakeLeft ? (-4) : 4), (int)tileLocation.Y, r.Next(1, 3), lastPlayerToHit.Value, location);
					}
				}
				if (health.Value == -100f)
				{
					return true;
				}
				if (health.Value <= 0f)
				{
					health.Value = -100f;
				}
			}
		}
		for (int i = leaves.Count - 1; i >= 0; i--)
		{
			Leaf leaf = leaves[i];
			leaf.position.Y -= leaf.yVelocity - 3f;
			leaf.yVelocity = Math.Max(0f, leaf.yVelocity - 0.01f);
			leaf.rotation += leaf.rotationRate;
			if (leaf.position.Y >= tileLocation.Y * 64f + 64f)
			{
				leaves.RemoveAt(i);
			}
		}
		return false;
	}

	/// <summary>Get a dropped item if its fields match.</summary>
	/// <param name="drop">The drop data.</param>
	/// <param name="r">The RNG to use for random checks.</param>
	/// <param name="targetFarmer">The player interacting with the tree.</param>
	/// <param name="fieldName">The field name to show in error messages if the drop is invalid.</param>
	/// <param name="formatItemId">Format the selected item ID before it's resolved.</param>
	/// <param name="isStump">Whether the tree is a stump, or <c>null</c> to use <see cref="F:StardewValley.TerrainFeatures.Tree.stump" />.</param>
	/// <returns>Returns the produced item (if any), else <c>null</c>.</returns>
	protected Item TryGetDrop(WildTreeItemData drop, Random r, Farmer targetFarmer, string fieldName, Func<string, string> formatItemId = null, bool? isStump = null)
	{
		if (!r.NextBool(drop.Chance))
		{
			return null;
		}
		if (drop.Season.HasValue && drop.Season != Location.GetSeason())
		{
			return null;
		}
		if (drop.Condition != null && !GameStateQuery.CheckConditions(drop.Condition, Location, targetFarmer, null, null, r))
		{
			return null;
		}
		if (drop is WildTreeChopItemData chopItemData && !chopItemData.IsValidForGrowthStage(growthStage.Value, isStump ?? stump.Value))
		{
			return null;
		}
		return ItemQueryResolver.TryResolveRandomItem(drop, new ItemQueryContext(Location, targetFarmer, r), avoidRepeat: false, null, formatItemId, null, delegate(string query, string error)
		{
			Game1.log.Error($"Wild tree '{treeType.Value}' failed parsing item query '{query}' for {fieldName} entry '{drop.Id}': {error}");
		});
	}

	public void shake(Vector2 tileLocation, bool doEvenIfStillShaking)
	{
		GameLocation location = Location;
		WildTreeData data = GetData();
		if ((maxShake == 0f || doEvenIfStillShaking) && (int)growthStage >= 3 && !stump)
		{
			shakeLeft.Value = (float)Game1.player.StandingPixel.X > (tileLocation.X + 0.5f) * 64f || (Game1.player.Tile.X == tileLocation.X && Game1.random.NextBool());
			maxShake = (float)(((int)growthStage >= 5) ? (Math.PI / 128.0) : (Math.PI / 64.0));
			if ((int)growthStage >= 5)
			{
				if (IsLeafy())
				{
					if (Game1.random.NextDouble() < 0.66)
					{
						int numberOfLeaves = Game1.random.Next(1, 6);
						for (int i = 0; i < numberOfLeaves; i++)
						{
							leaves.Add(new Leaf(new Vector2(Game1.random.Next((int)(tileLocation.X * 64f - 64f), (int)(tileLocation.X * 64f + 128f)), Game1.random.Next((int)(tileLocation.Y * 64f - 256f), (int)(tileLocation.Y * 64f - 192f))), (float)Game1.random.Next(-10, 10) / 100f, Game1.random.Next(4), (float)Game1.random.Next(5) / 10f));
						}
					}
					if (Game1.random.NextDouble() < 0.01 && (localSeason == Season.Spring || localSeason == Season.Summer))
					{
						bool isIslandButterfly = Location.InIslandContext();
						while (Game1.random.NextDouble() < 0.8)
						{
							location.addCritter(new Butterfly(location, new Vector2(tileLocation.X + (float)Game1.random.Next(1, 3), tileLocation.Y - 2f + (float)Game1.random.Next(-1, 2)), isIslandButterfly));
						}
					}
				}
				if ((bool)hasSeed && (Game1.IsMultiplayer || Game1.player.ForagingLevel >= 1))
				{
					bool dropDefaultSeed = true;
					if (data != null && data.SeedDropItems?.Count > 0)
					{
						foreach (WildTreeSeedDropItemData drop in data.SeedDropItems)
						{
							Item seed = TryGetDrop(drop, Game1.random, Game1.player, "SeedDropItems");
							if (seed != null)
							{
								if (Game1.player.professions.Contains(16) && seed.HasContextTag("forage_item"))
								{
									seed.Quality = 4;
								}
								Game1.createItemDebris(seed, new Vector2(tileLocation.X * 64f, (tileLocation.Y - 3f) * 64f), -1, location, Game1.player.StandingPixel.Y);
								if (!drop.ContinueOnDrop)
								{
									dropDefaultSeed = false;
									break;
								}
							}
						}
					}
					if (dropDefaultSeed && data != null)
					{
						Item seed = ItemRegistry.Create(data.SeedItemId);
						if (Game1.player.professions.Contains(16) && seed.HasContextTag("forage_item"))
						{
							seed.Quality = 4;
						}
						Game1.createItemDebris(seed, new Vector2(tileLocation.X * 64f, (tileLocation.Y - 3f) * 64f), -1, location, Game1.player.StandingPixel.Y);
					}
					if (Utility.tryRollMysteryBox(0.03))
					{
						Game1.createItemDebris(ItemRegistry.Create((Game1.player.stats.Get(StatKeys.Mastery(2)) != 0) ? "(O)GoldenMysteryBox" : "(O)MysteryBox"), new Vector2(tileLocation.X, tileLocation.Y - 3f) * 64f, -1, location, Game1.player.StandingPixel.Y - 32);
					}
					Utility.trySpawnRareObject(Game1.player, new Vector2(tileLocation.X, tileLocation.Y - 3f) * 64f, Location, 2.0, 1.0, Game1.player.StandingPixel.Y - 32);
					if (Game1.random.NextBool() && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS"))
					{
						Game1.createObjectDebris("(O)890", (int)tileLocation.X, (int)tileLocation.Y - 3, ((int)tileLocation.Y + 1) * 64, 0, 1f, location);
					}
					hasSeed.Value = false;
				}
				if (wasShakenToday.Value)
				{
					return;
				}
				wasShakenToday.Value = true;
				if (data?.ShakeItems == null)
				{
					return;
				}
				{
					foreach (WildTreeItemData entry in data.ShakeItems)
					{
						Item item = TryGetDrop(entry, Game1.random, Game1.player, "ShakeItems");
						if (item != null)
						{
							Game1.createItemDebris(item, tileLocation * 64f, -2, Location);
						}
					}
					return;
				}
			}
			if (Game1.random.NextDouble() < 0.66)
			{
				int numberOfLeaves = Game1.random.Next(1, 3);
				for (int i = 0; i < numberOfLeaves; i++)
				{
					leaves.Add(new Leaf(new Vector2(Game1.random.Next((int)(tileLocation.X * 64f), (int)(tileLocation.X * 64f + 48f)), tileLocation.Y * 64f - 32f), (float)Game1.random.Next(-10, 10) / 100f, Game1.random.Next(4), (float)Game1.random.Next(30) / 10f));
				}
			}
		}
		else if ((bool)stump)
		{
			shakeTimer = 100f;
		}
	}

	public override bool isPassable(Character c = null)
	{
		if (health.Value <= -99f || (int)growthStage == 0)
		{
			return true;
		}
		return false;
	}

	/// <summary>Get the maximum size the tree can grow in its current position.</summary>
	/// <param name="ignoreSeason">Whether to assume the tree is in-season.</param>
	public virtual int GetMaxSizeHere(bool ignoreSeason = false)
	{
		GameLocation location = Location;
		Vector2 tile = Tile;
		if (GetData() == null)
		{
			return growthStage.Value;
		}
		if (location.IsNoSpawnTile(tile, "Tree") && !location.doesEitherTileOrTileIndexPropertyEqual((int)tile.X, (int)tile.Y, "CanPlantTrees", "Back", "T"))
		{
			return growthStage.Value;
		}
		if (!ignoreSeason && !IsInSeason())
		{
			return growthStage.Value;
		}
		if (growthStage.Value == 0 && location.objects.ContainsKey(tile))
		{
			return 0;
		}
		if (IsGrowthBlockedByNearbyTree())
		{
			return 4;
		}
		return 15;
	}

	/// <summary>Get whether this tree is in-season for its current location, so it can grow if applicable.</summary>
	public bool IsInSeason()
	{
		if (localSeason == Season.Winter && !fertilized.Value && !Location.SeedsIgnoreSeasonsHere())
		{
			return GetData()?.GrowsInWinter ?? false;
		}
		return true;
	}

	/// <summary>Get whether growth is blocked because it's too close to another fully-grown tree.</summary>
	public bool IsGrowthBlockedByNearbyTree()
	{
		GameLocation location = Location;
		Vector2 tile = Tile;
		Microsoft.Xna.Framework.Rectangle growthRect = new Microsoft.Xna.Framework.Rectangle((int)((tile.X - 1f) * 64f), (int)((tile.Y - 1f) * 64f), 192, 192);
		foreach (KeyValuePair<Vector2, TerrainFeature> other in location.terrainFeatures.Pairs)
		{
			if (other.Key != tile && other.Value is Tree otherTree && (int)otherTree.growthStage >= 5 && otherTree.getBoundingBox().Intersects(growthRect))
			{
				return true;
			}
		}
		return false;
	}

	public void onGreenRainDay(bool undo = false)
	{
		if (undo)
		{
			if ((bool)isTemporaryGreenRainTree)
			{
				isTemporaryGreenRainTree.Value = false;
				if (treeType == "10")
				{
					treeType.Value = "1";
				}
				else
				{
					treeType.Value = "2";
				}
				resetTexture();
			}
		}
		else
		{
			if (Location == null || !Location.IsOutdoors)
			{
				return;
			}
			if ((int)growthStage < 5)
			{
				if ((int)growthStage == 0 && (Game1.random.NextDouble() < 0.5 || Location == null || Location.objects.ContainsKey(Tile)))
				{
					return;
				}
				growthStage.Value = 4;
				for (int i = 0; i < 3; i++)
				{
					dayUpdate();
				}
			}
			bool? flag = GetData()?.GrowsMoss;
			if (flag.HasValue && flag.GetValueOrDefault() && Game1.random.NextBool())
			{
				hasMoss.Value = true;
			}
			if ((treeType == "1" || treeType == "2") && (int)growthStage >= 5 && Game1.random.NextBool(0.75))
			{
				isTemporaryGreenRainTree.Value = true;
				if (treeType == "1")
				{
					treeType.Value = "10";
				}
				else
				{
					treeType.Value = "11";
				}
				resetTexture();
			}
		}
	}

	public override void dayUpdate()
	{
		GameLocation environment = Location;
		if (!Game1.IsFall && !Game1.IsWinter)
		{
			GameLocation location2 = Location;
			if ((location2 == null || !location2.IsGreenRainingHere()) && isTemporaryGreenRainTree.Value)
			{
				isTemporaryGreenRainTree.Value = false;
				if (treeType == "10")
				{
					treeType.Value = "1";
				}
				else
				{
					treeType.Value = "2";
				}
				resetTexture();
			}
		}
		wasShakenToday.Value = false;
		setSeason();
		CheckForNewTexture();
		WildTreeData data = GetData();
		Vector2 tile = Tile;
		if (health.Value <= -100f)
		{
			destroy.Value = true;
		}
		if (tapped.Value)
		{
			Object tile_object = environment.getObjectAtTile((int)tile.X, (int)tile.Y);
			if (tile_object == null || !tile_object.IsTapper())
			{
				tapped.Value = false;
			}
			else if (tile_object.IsTapper() && tile_object.heldObject.Value == null)
			{
				UpdateTapperProduct(tile_object);
			}
		}
		if (GetMaxSizeHere() > growthStage.Value)
		{
			float chance = data?.GrowthChance ?? 0.2f;
			float fertilizedGrowthChance = data?.FertilizedGrowthChance ?? 1f;
			if (Game1.random.NextBool(chance) || (fertilized.Value && Game1.random.NextBool(fertilizedGrowthChance)))
			{
				growthStage.Value++;
			}
		}
		if (localSeason == Season.Winter && data != null && data.IsStumpDuringWinter && !Location.SeedsIgnoreSeasonsHere())
		{
			stump.Value = true;
		}
		else if (data != null && data.IsStumpDuringWinter && Game1.dayOfMonth <= 1 && Game1.IsSpring)
		{
			stump.Value = false;
			health.Value = 10f;
			shakeRotation = 0f;
		}
		if ((int)growthStage >= 5 && !stump.Value && environment is Farm && Game1.random.NextBool(data?.SeedSpreadChance ?? 0.15f))
		{
			int xCoord = Game1.random.Next(-3, 4) + (int)tile.X;
			int yCoord = Game1.random.Next(-3, 4) + (int)tile.Y;
			Vector2 location = new Vector2(xCoord, yCoord);
			if (!environment.IsNoSpawnTile(location, "Tree") && environment.isTileLocationOpen(new Location(xCoord, yCoord)) && !environment.IsTileOccupiedBy(location) && !environment.isWaterTile(xCoord, yCoord) && environment.isTileOnMap(location))
			{
				environment.terrainFeatures.Add(location, new Tree(treeType, 0));
			}
		}
		if ((bool)isTemporaryGreenRainTree && environment.IsGreenhouse && (localSeason == Season.Winter || localSeason == Season.Fall))
		{
			hasSeed.Value = false;
		}
		else
		{
			hasSeed.Value = data != null && data.SeedItemId != null && (int)growthStage >= 5 && Game1.random.NextBool(data.SeedOnShakeChance);
		}
		bool accelerateMoss = (int)growthStage >= 5 && !Game1.IsWinter && (treeType.Value == "10" || treeType.Value == "11") && !isTemporaryGreenRainTree.Value;
		if ((int)growthStage >= 5 && !Game1.IsWinter && !accelerateMoss)
		{
			for (int x = (int)tile.X - 2; (float)x <= tile.X + 2f; x++)
			{
				for (int y = (int)tile.Y - 2; (float)y <= tile.Y + 2f; y++)
				{
					Vector2 v = new Vector2(x, y);
					if (Location.terrainFeatures.ContainsKey(v) && Location.terrainFeatures[v] is Tree tree && tree.growthStage.Value >= 5 && (tree.treeType.Value == "10" || tree.treeType.Value == "11") && !tree.isTemporaryGreenRainTree.Value && (bool)tree.hasMoss)
					{
						accelerateMoss = true;
						break;
					}
				}
				if (accelerateMoss)
				{
					break;
				}
			}
		}
		float mossChance = (Game1.isRaining ? 0.2f : 0.1f);
		if (accelerateMoss && Game1.random.NextDouble() < 0.5)
		{
			growthStage.Value++;
		}
		if (Game1.IsSummer && !Game1.isGreenRain && !Game1.isRaining)
		{
			mossChance = 0.033f;
		}
		if (accelerateMoss && Game1.random.NextDouble() < 0.5)
		{
			mossChance += 0.1f;
		}
		if (stopGrowingMoss.Value)
		{
			hasMoss.Value = false;
			return;
		}
		if (!environment.IsGreenhouse && (localSeason == Season.Winter || stump.Value))
		{
			hasMoss.Value = false;
			return;
		}
		bool? flag = data?.GrowsMoss;
		if (flag.HasValue && flag.GetValueOrDefault() && (int)growthStage >= 14 && !stump.Value && Game1.random.NextBool(mossChance))
		{
			hasMoss.Value = true;
		}
	}

	public override void performPlayerEntryAction()
	{
		base.performPlayerEntryAction();
		setSeason();
		CheckForNewTexture();
	}

	/// <inheritdoc />
	public override bool seasonUpdate(bool onLoad)
	{
		if (Game1.IsFall && Game1.random.NextDouble() < 0.05 && !tapped && (treeType.Value == "1" || treeType.Value == "2") && growthStage.Value >= 5 && Location != null && !(Location is Town) && !Location.IsGreenhouse)
		{
			if (treeType == "1")
			{
				treeType.Value = "10";
			}
			else
			{
				treeType.Value = "11";
			}
			isTemporaryGreenRainTree.Value = true;
			resetTexture();
		}
		if (tapped.Value && Location != null)
		{
			Object tile_object = Location.getObjectAtTile((int)Tile.X, (int)Tile.Y);
			if (tile_object != null && tile_object.IsTapper())
			{
				UpdateTapperProduct(tile_object, null, onlyPerformRemovals: true);
			}
		}
		loadSprite();
		return false;
	}

	public override bool isActionable()
	{
		if (!tapped)
		{
			return (int)growthStage >= 3;
		}
		return false;
	}

	public virtual bool IsLeafy()
	{
		WildTreeData data = GetData();
		if (data != null && data.IsLeafy)
		{
			if (data.IsLeafyInWinter || !Location.IsWinterHere())
			{
				if (!data.IsLeafyInFall)
				{
					return !Location.IsFallHere();
				}
				return true;
			}
			return false;
		}
		return false;
	}

	/// <summary>Get the color of the cosmetic wood chips when chopping the tree.</summary>
	public Color? GetChopDebrisColor()
	{
		return GetChopDebrisColor(GetData());
	}

	/// <summary>Get the color of the cosmetic wood chips when chopping the tree.</summary>
	/// <param name="data">The wild tree data to read.</param>
	public Color? GetChopDebrisColor(WildTreeData data)
	{
		string rawColor = data?.DebrisColor;
		if (rawColor == null)
		{
			return null;
		}
		if (!int.TryParse(rawColor, out var debrisType))
		{
			return Utility.StringToColor(rawColor);
		}
		return Debris.getColorForDebris(debrisType);
	}

	public override bool performToolAction(Tool t, int explosion, Vector2 tileLocation)
	{
		GameLocation location = Location ?? Game1.currentLocation;
		if (explosion > 0)
		{
			tapped.Value = false;
		}
		if (health.Value <= -99f)
		{
			return false;
		}
		if ((int)growthStage >= 5)
		{
			if ((bool)hasMoss)
			{
				Item moss = CreateMossItem();
				if (t != null && t.getLastFarmerToUse() != null)
				{
					t.getLastFarmerToUse().gainExperience(2, moss.Stack);
				}
				hasMoss.Value = false;
				Game1.createMultipleItemDebris(moss, new Vector2(tileLocation.X, tileLocation.Y - 1f) * 64f, -1, location, Game1.player.StandingPixel.Y - 32);
				Game1.stats.Increment("mossHarvested");
				shake(tileLocation, doEvenIfStillShaking: true);
				growthStage.Value = 12 - moss.Stack;
				Game1.playSound("moss_cut");
				for (int i = 0; i < 6; i++)
				{
					location.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\debris", new Microsoft.Xna.Framework.Rectangle(Game1.random.Choose(16, 0), 96, 16, 16), new Vector2(tileLocation.X + (float)Game1.random.NextDouble() - 0.15f, tileLocation.Y - 1f + (float)Game1.random.NextDouble()) * 64f, flipped: false, 0.025f, Color.Green)
					{
						drawAboveAlwaysFront = true,
						motion = new Vector2((float)Game1.random.Next(-10, 11) / 10f, -4f),
						acceleration = new Vector2(0f, 0.3f + (float)Game1.random.Next(-10, 11) / 200f),
						animationLength = 1,
						interval = 1000f,
						sourceRectStartingPos = new Vector2(0f, 96f),
						alpha = 1f,
						layerDepth = 1f,
						scale = 4f
					});
				}
			}
			if ((bool)tapped)
			{
				return false;
			}
			if (t is Axe)
			{
				location.playSound("axchop", tileLocation);
				lastPlayerToHit.Value = t.getLastFarmerToUse().UniqueMultiplayerID;
				location.debris.Add(new Debris(12, Game1.random.Next(1, 3), t.getLastFarmerToUse().GetToolLocation() + new Vector2(16f, 0f), t.getLastFarmerToUse().Position, 0, GetChopDebrisColor()));
				if (location is Town && tileLocation.X < 100f && !isTemporaryGreenRainTree)
				{
					int pathsIndex = location.getTileIndexAt((int)tileLocation.X, (int)tileLocation.Y, "Paths");
					if (pathsIndex == 9 || pathsIndex == 10 || pathsIndex == 11)
					{
						shake(tileLocation, doEvenIfStillShaking: true);
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:TownTreeWarning"));
						return false;
					}
				}
				if (!stump && t.getLastFarmerToUse() != null && location.HasUnlockedAreaSecretNotes(t.getLastFarmerToUse()) && Game1.random.NextDouble() < 0.005)
				{
					Object o = location.tryToCreateUnseenSecretNote(t.getLastFarmerToUse());
					if (o != null)
					{
						Game1.createItemDebris(o, new Vector2(tileLocation.X, tileLocation.Y - 3f) * 64f, -1, location, Game1.player.StandingPixel.Y - 32);
					}
				}
				else if (!stump && t.getLastFarmerToUse() != null && Utility.tryRollMysteryBox(0.005))
				{
					Game1.createItemDebris(ItemRegistry.Create((t.getLastFarmerToUse().stats.Get(StatKeys.Mastery(2)) != 0) ? "(O)GoldenMysteryBox" : "(O)MysteryBox"), new Vector2(tileLocation.X, tileLocation.Y - 3f) * 64f, -1, location, Game1.player.StandingPixel.Y - 32);
				}
				else if (!stump && t.getLastFarmerToUse() != null && t.getLastFarmerToUse().stats.Get("TreesChopped") > 20 && Game1.random.NextDouble() < 0.0003 + (t.getLastFarmerToUse().mailReceived.Contains("GotWoodcuttingBook") ? 0.0007 : ((double)t.getLastFarmerToUse().stats.Get("TreesChopped") * 1E-05)))
				{
					Game1.createItemDebris(ItemRegistry.Create("(O)Book_Woodcutting"), new Vector2(tileLocation.X, tileLocation.Y - 3f) * 64f, -1, location, Game1.player.StandingPixel.Y - 32);
					t.getLastFarmerToUse().mailReceived.Add("GotWoodcuttingBook");
				}
				else if (!stump)
				{
					Utility.trySpawnRareObject(Game1.player, new Vector2(tileLocation.X, tileLocation.Y - 3f) * 64f, Location, 0.33, 1.0, Game1.player.StandingPixel.Y - 32);
				}
			}
			else if (explosion <= 0)
			{
				return false;
			}
			shake(tileLocation, doEvenIfStillShaking: true);
			float damage;
			if (explosion > 0)
			{
				damage = explosion;
				if (location is Town && tileLocation.X < 100f)
				{
					return false;
				}
			}
			else
			{
				if (t == null)
				{
					return false;
				}
				damage = t.upgradeLevel switch
				{
					0L => 1f, 
					1L => 1.25f, 
					2L => 1.67f, 
					3L => 2.5f, 
					4L => 5f, 
					_ => (int)t.upgradeLevel + 1, 
				};
			}
			if (t is Axe && t.hasEnchantmentOfType<ShavingEnchantment>() && Game1.random.NextDouble() <= (double)(damage / 5f))
			{
				Debris d = ((treeType == "12") ? new Debris("(O)259", new Vector2(tileLocation.X * 64f + 32f, (tileLocation.Y - 0.5f) * 64f + 32f), Game1.player.getStandingPosition()) : ((treeType == "7") ? new Debris("(O)420", new Vector2(tileLocation.X * 64f + 32f, (tileLocation.Y - 0.5f) * 64f + 32f), Game1.player.getStandingPosition()) : ((!(treeType == "8")) ? new Debris("388", new Vector2(tileLocation.X * 64f + 32f, (tileLocation.Y - 0.5f) * 64f + 32f), Game1.player.getStandingPosition()) : new Debris("(O)709", new Vector2(tileLocation.X * 64f + 32f, (tileLocation.Y - 0.5f) * 64f + 32f), Game1.player.getStandingPosition()))));
				d.Chunks[0].xVelocity.Value += (float)Game1.random.Next(-10, 11) / 10f;
				d.chunkFinalYLevel = (int)(tileLocation.Y * 64f + 64f);
				location.debris.Add(d);
			}
			health.Value -= damage;
			if (health.Value <= 0f && performTreeFall(t, explosion, tileLocation))
			{
				return true;
			}
		}
		else if ((int)growthStage >= 3)
		{
			if (t != null && t.BaseName.Contains("Ax"))
			{
				location.playSound("axchop", tileLocation);
				if (IsLeafy())
				{
					location.playSound("leafrustle");
				}
				location.debris.Add(new Debris(12, Game1.random.Next((int)t.upgradeLevel * 2, (int)t.upgradeLevel * 4), t.getLastFarmerToUse().GetToolLocation() + new Vector2(16f, 0f), Utility.PointToVector2(t.getLastFarmerToUse().StandingPixel), 0));
			}
			else if (explosion <= 0)
			{
				return false;
			}
			shake(tileLocation, doEvenIfStillShaking: true);
			float damage = 1f;
			damage = ((explosion > 0) ? ((float)explosion) : (t.upgradeLevel switch
			{
				0L => 2f, 
				1L => 2.5f, 
				2L => 3.34f, 
				3L => 5f, 
				4L => 10f, 
				_ => 10 + ((int)t.upgradeLevel - 4), 
			}));
			health.Value -= damage;
			if (health.Value <= 0f)
			{
				performBushDestroy(tileLocation);
				return true;
			}
		}
		else if ((int)growthStage >= 1)
		{
			if (explosion > 0)
			{
				location.playSound("cut");
				return true;
			}
			if (t != null && t.BaseName.Contains("Axe"))
			{
				location.playSound("axchop", tileLocation);
				Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(10, 20), resource: false);
			}
			if (t is Axe || t is Pickaxe || t is Hoe || t is MeleeWeapon)
			{
				location.playSound("cut");
				performSproutDestroy(t, tileLocation);
				return true;
			}
		}
		else
		{
			if (explosion > 0)
			{
				return true;
			}
			if (t.BaseName.Contains("Axe") || t.BaseName.Contains("Pick") || t.BaseName.Contains("Hoe"))
			{
				location.playSound("woodyHit", tileLocation);
				location.playSound("axchop", tileLocation);
				performSeedDestroy(t, tileLocation);
				return true;
			}
		}
		return false;
	}

	public static Item CreateMossItem()
	{
		Random rand = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.Get("mossHarvested") * 50);
		return ItemRegistry.Create("(O)Moss", rand.Next(1, 3));
	}

	public bool fertilize()
	{
		GameLocation location = Location;
		if ((int)growthStage >= 5)
		{
			Game1.showRedMessageUsingLoadString("Strings\\StringsFromCSFiles:TreeFertilizer1");
			location.playSound("cancel");
			return false;
		}
		if (fertilized.Value)
		{
			Game1.showRedMessageUsingLoadString("Strings\\StringsFromCSFiles:TreeFertilizer2");
			location.playSound("cancel");
			return false;
		}
		fertilized.Value = true;
		location.playSound("dirtyHit");
		return true;
	}

	public bool instantDestroy(Vector2 tileLocation)
	{
		if ((int)growthStage >= 5)
		{
			return performTreeFall(null, 0, tileLocation);
		}
		if ((int)growthStage >= 3)
		{
			performBushDestroy(tileLocation);
			return true;
		}
		if ((int)growthStage >= 1)
		{
			performSproutDestroy(null, tileLocation);
			return true;
		}
		performSeedDestroy(null, tileLocation);
		return true;
	}

	protected void performSeedDestroy(Tool t, Vector2 tileLocation)
	{
		GameLocation location = Location;
		Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(17, tileLocation * 64f, Color.White));
		WildTreeData data = GetData();
		if (data != null && data.SeedItemId != null)
		{
			if (lastPlayerToHit.Value != 0L && Game1.getFarmer(lastPlayerToHit.Value).getEffectiveSkillLevel(2) >= 1)
			{
				Game1.createMultipleObjectDebris(data.SeedItemId, (int)tileLocation.X, (int)tileLocation.Y, 1, t.getLastFarmerToUse().UniqueMultiplayerID, location);
			}
			else if (Game1.player.getEffectiveSkillLevel(2) >= 1)
			{
				Game1.createMultipleObjectDebris(data.SeedItemId, (int)tileLocation.X, (int)tileLocation.Y, 1, t?.getLastFarmerToUse().UniqueMultiplayerID ?? Game1.player.UniqueMultiplayerID, location);
			}
		}
	}

	/// <summary>Update the attached tapper's held output.</summary>
	/// <param name="tapper">The attached tapper instance.</param>
	/// <param name="previousOutput">The previous item produced by the tapper, if any.</param>
	public void UpdateTapperProduct(Object tapper, Object previousOutput = null, bool onlyPerformRemovals = false)
	{
		if (tapper == null)
		{
			return;
		}
		WildTreeData data = GetData();
		if (data == null)
		{
			return;
		}
		float timeMultiplier = 1f;
		foreach (string contextTag in tapper.GetContextTags())
		{
			if (contextTag.StartsWith("tapper_multiplier_") && float.TryParse(contextTag.Substring("tapper_multiplier_".Length), out var multiplier))
			{
				timeMultiplier = 1f / multiplier;
				break;
			}
		}
		Random random = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed, 73137.0, (double)Tile.X * 9.0, (double)Tile.Y * 13.0);
		if (TryGetTapperOutput(data.TapItems, previousOutput?.ItemId, random, timeMultiplier, out var output, out var minutesUntilReady) && (!onlyPerformRemovals || output == null))
		{
			tapper.heldObject.Value = output;
			tapper.minutesUntilReady.Value = minutesUntilReady;
		}
	}

	/// <summary>Get a valid item that can be produced by the tree's current tapper.</summary>
	/// <param name="tapItems">The tap item data to choose from.</param>
	/// <param name="previousItemId">The previous item ID that was produced.</param>
	/// <param name="r">The RNG with which to randomize.</param>
	/// <param name="timeMultiplier">A multiplier to apply to the minutes until ready.</param>
	/// <param name="output">The possible tapper output.</param>
	/// <param name="minutesUntilReady">The number of minutes until the tapper would produce the output.</param>
	protected bool TryGetTapperOutput(List<WildTreeTapItemData> tapItems, string previousItemId, Random r, float timeMultiplier, out Object output, out int minutesUntilReady)
	{
		if (tapItems != null)
		{
			previousItemId = ((previousItemId != null) ? ItemRegistry.QualifyItemId(previousItemId) : null);
			foreach (WildTreeTapItemData tapData in tapItems)
			{
				if (!GameStateQuery.CheckConditions(tapData.Condition, Location))
				{
					continue;
				}
				if (tapData.PreviousItemId != null)
				{
					bool found = false;
					foreach (string expectedPrevId in tapData.PreviousItemId)
					{
						found = (string.IsNullOrEmpty(expectedPrevId) ? (previousItemId == null) : string.Equals(previousItemId, ItemRegistry.QualifyItemId(expectedPrevId), StringComparison.OrdinalIgnoreCase));
						if (found)
						{
							break;
						}
					}
					if (!found)
					{
						continue;
					}
				}
				if (tapData.Season.HasValue && tapData.Season != localSeason)
				{
					continue;
				}
				Farmer targetFarmer = Game1.getFarmer(lastPlayerToHit.Value);
				Item item = TryGetDrop(tapData, r, targetFarmer, "TapItems", (string id) => id.Replace("PREVIOUS_OUTPUT_ID", previousItemId));
				if (item != null)
				{
					if (item is Object obj)
					{
						int daysUntilReady = (int)Utility.ApplyQuantityModifiers(tapData.DaysUntilReady, tapData.DaysUntilReadyModifiers, tapData.DaysUntilReadyModifierMode, Location, Game1.player);
						output = obj;
						minutesUntilReady = Utility.CalculateMinutesUntilMorning(Game1.timeOfDay, (int)Math.Max(1.0, Math.Floor((float)daysUntilReady * timeMultiplier)));
						return true;
					}
					Game1.log.Warn($"Wild tree '{treeType.Value}' can't produce item '{item.ItemId}': must be an object-type item.");
				}
			}
			if (previousItemId != null)
			{
				return TryGetTapperOutput(tapItems, null, r, timeMultiplier, out output, out minutesUntilReady);
			}
		}
		output = null;
		minutesUntilReady = 0;
		return false;
	}

	protected void performSproutDestroy(Tool t, Vector2 tileLocation)
	{
		GameLocation location = Location;
		Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(10, 20), resource: false);
		if (t != null && t.BaseName.Contains("Axe") && Game1.recentMultiplayerRandom.NextDouble() < (double)((float)t.getLastFarmerToUse().ForagingLevel / 10f))
		{
			Game1.createDebris(12, (int)tileLocation.X, (int)tileLocation.Y, 1);
		}
		Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(17, tileLocation * 64f, Color.White));
	}

	protected void performBushDestroy(Vector2 tileLocation)
	{
		GameLocation location = Location;
		WildTreeData data = GetData();
		if (data == null)
		{
			return;
		}
		Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(20, 30), resource: false, -1, item: false, GetChopDebrisColor(data));
		if (data.DropWoodOnChop || data.DropHardwoodOnLumberChop)
		{
			Game1.createDebris(12, (int)tileLocation.X, (int)tileLocation.Y, (int)((Game1.getFarmer(lastPlayerToHit.Value).professions.Contains(12) ? 1.25 : 1.0) * 4.0), location);
		}
		List<WildTreeChopItemData> chopItems = data.ChopItems;
		if (chopItems == null || chopItems.Count <= 0)
		{
			return;
		}
		Random r;
		if (Game1.IsMultiplayer)
		{
			Game1.recentMultiplayerRandom = Utility.CreateRandom((double)tileLocation.X * 1000.0, tileLocation.Y);
			r = Game1.recentMultiplayerRandom;
		}
		else
		{
			r = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed, (double)tileLocation.X * 7.0, (double)tileLocation.Y * 11.0);
		}
		Farmer targetFarmer = Game1.getFarmer(lastPlayerToHit.Value);
		foreach (WildTreeChopItemData drop in data.ChopItems)
		{
			Item item = TryGetDrop(drop, r, targetFarmer, "ChopItems");
			if (item != null)
			{
				Game1.createMultipleItemDebris(item, tileLocation * 64f, -2, location);
			}
		}
	}

	protected bool performTreeFall(Tool t, int explosion, Vector2 tileLocation)
	{
		GameLocation location = Location;
		WildTreeData data = GetData();
		Location.objects.Remove(Tile);
		tapped.Value = false;
		if (!stump)
		{
			if (t != null || explosion > 0)
			{
				location.playSound("treecrack");
			}
			stump.Value = true;
			health.Value = 5f;
			falling.Value = true;
			if (t != null && t.getLastFarmerToUse().IsLocalPlayer)
			{
				t?.getLastFarmerToUse().gainExperience(2, 12);
				if (t?.getLastFarmerToUse() == null)
				{
					shakeLeft.Value = true;
				}
				else
				{
					shakeLeft.Value = (float)t.getLastFarmerToUse().StandingPixel.X > (tileLocation.X + 0.5f) * 64f;
				}
				t.getLastFarmerToUse().stats.Increment("TreesChopped", 1);
			}
		}
		else
		{
			if (t != null && health.Value != -100f && t.getLastFarmerToUse().IsLocalPlayer)
			{
				t?.getLastFarmerToUse().gainExperience(2, 1);
			}
			health.Value = -100f;
			if (data != null)
			{
				Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(30, 40), resource: false, -1, item: false, GetChopDebrisColor(data));
				Random r;
				if (Game1.IsMultiplayer)
				{
					Game1.recentMultiplayerRandom = Utility.CreateRandom((double)tileLocation.X * 2000.0, tileLocation.Y);
					r = Game1.recentMultiplayerRandom;
				}
				else
				{
					r = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed, (double)tileLocation.X * 7.0, (double)tileLocation.Y * 11.0);
				}
				if (t?.getLastFarmerToUse() == null)
				{
					if (location.Equals(Game1.currentLocation))
					{
						Game1.createMultipleObjectDebris("(O)92", (int)tileLocation.X, (int)tileLocation.Y, 2, location);
					}
					else
					{
						for (int i = 0; i < 2; i++)
						{
							Game1.createItemDebris(ItemRegistry.Create("(O)92"), tileLocation * 64f, 2, location);
						}
					}
				}
				else if (Game1.IsMultiplayer)
				{
					if (data.DropWoodOnChop)
					{
						Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, (int)((Game1.getFarmer(lastPlayerToHit.Value).professions.Contains(12) ? 1.25 : 1.0) * 4.0), resource: true);
					}
					List<WildTreeChopItemData> chopItems = data.ChopItems;
					if (chopItems != null && chopItems.Count > 0)
					{
						Farmer targetFarmer = Game1.getFarmer(lastPlayerToHit.Value);
						foreach (WildTreeChopItemData drop in data.ChopItems)
						{
							Item item = TryGetDrop(drop, r, targetFarmer, "ChopItems");
							if (item != null)
							{
								if (item.QualifiedItemId == "(O)420" && tileLocation.X % 7f == 0f)
								{
									item = ItemRegistry.Create("(O)422", item.Stack, item.Quality);
								}
								Game1.createMultipleItemDebris(item, tileLocation * 64f, -2, location);
							}
						}
					}
				}
				else
				{
					if (data.DropWoodOnChop)
					{
						Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, (int)((Game1.getFarmer(lastPlayerToHit.Value).professions.Contains(12) ? 1.25 : 1.0) * (double)(5 + extraWoodCalculator(tileLocation))), resource: true);
					}
					List<WildTreeChopItemData> chopItems2 = data.ChopItems;
					if (chopItems2 != null && chopItems2.Count > 0)
					{
						Farmer targetFarmer = Game1.getFarmer(lastPlayerToHit.Value);
						foreach (WildTreeChopItemData drop in data.ChopItems)
						{
							Item item = TryGetDrop(drop, r, targetFarmer, "ChopItems");
							if (item != null)
							{
								if (item.QualifiedItemId == "(O)420" && tileLocation.X % 7f == 0f)
								{
									item = ItemRegistry.Create("(O)422", item.Stack, item.Quality);
								}
								Game1.createMultipleItemDebris(item, tileLocation * 64f, -2, location);
							}
						}
					}
				}
				if (Game1.random.NextDouble() <= 0.25 && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS"))
				{
					Game1.createObjectDebris("(O)890", (int)tileLocation.X, (int)tileLocation.Y - 3, ((int)tileLocation.Y + 1) * 64, 0, 1f, location);
				}
				location.playSound("treethud");
			}
			if (!falling)
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Update the tree's season for the location it's planted in.</summary>
	protected void setSeason()
	{
		GameLocation location = Location;
		localSeason = ((!(location is Desert) && !(location is MineShaft)) ? Game1.GetSeasonForLocation(location) : Season.Spring);
	}

	public override void drawInMenu(SpriteBatch spriteBatch, Vector2 positionOnScreen, Vector2 tileLocation, float scale, float layerDepth)
	{
		layerDepth += positionOnScreen.X / 100000f;
		if ((int)growthStage < 5)
		{
			Microsoft.Xna.Framework.Rectangle sourceRect = growthStage switch
			{
				0L => new Microsoft.Xna.Framework.Rectangle(32, 128, 16, 16), 
				1L => new Microsoft.Xna.Framework.Rectangle(0, 128, 16, 16), 
				2L => new Microsoft.Xna.Framework.Rectangle(16, 128, 16, 16), 
				_ => new Microsoft.Xna.Framework.Rectangle(0, 96, 16, 32), 
			};
			spriteBatch.Draw(texture.Value, positionOnScreen - new Vector2(0f, (float)sourceRect.Height * scale), sourceRect, Color.White, 0f, Vector2.Zero, scale, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + (positionOnScreen.Y + (float)sourceRect.Height * scale) / 20000f);
			return;
		}
		if (!falling)
		{
			spriteBatch.Draw(texture.Value, positionOnScreen + new Vector2(0f, -64f * scale), new Microsoft.Xna.Framework.Rectangle(32, 96, 16, 32), Color.White, 0f, Vector2.Zero, scale, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + (positionOnScreen.Y + 448f * scale - 1f) / 20000f);
		}
		if (!stump || (bool)falling)
		{
			spriteBatch.Draw(texture.Value, positionOnScreen + new Vector2(-64f * scale, -320f * scale), new Microsoft.Xna.Framework.Rectangle(0, 0, 48, 96), Color.White, shakeRotation, Vector2.Zero, scale, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + (positionOnScreen.Y + 448f * scale) / 20000f);
		}
	}

	public override void draw(SpriteBatch spriteBatch)
	{
		if (isTemporarilyInvisible)
		{
			return;
		}
		Vector2 tileLocation = Tile;
		float baseSortPosition = getBoundingBox().Bottom;
		if (texture.Value == null || !TryGetData(treeType.Value, out var data))
		{
			IItemDataDefinition itemType = ItemRegistry.RequireTypeDefinition("(O)");
			spriteBatch.Draw(itemType.GetErrorTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + ((shakeTimer > 0f) ? ((float)Math.Sin(Math.PI * 2.0 / (double)shakeTimer) * 3f) : 0f), tileLocation.Y * 64f)), itemType.GetErrorSourceRect(), Color.White * alpha, 0f, Vector2.Zero, 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (baseSortPosition + 1f) / 10000f);
			return;
		}
		if ((int)growthStage < 5)
		{
			Microsoft.Xna.Framework.Rectangle sourceRect = growthStage switch
			{
				0L => new Microsoft.Xna.Framework.Rectangle(32, 128, 16, 16), 
				1L => new Microsoft.Xna.Framework.Rectangle(0, 128, 16, 16), 
				2L => new Microsoft.Xna.Framework.Rectangle(16, 128, 16, 16), 
				_ => new Microsoft.Xna.Framework.Rectangle(0, 96, 16, 32), 
			};
			spriteBatch.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f - (float)(sourceRect.Height * 4 - 64) + (float)(((int)growthStage >= 3) ? 128 : 64))), sourceRect, fertilized ? Color.HotPink : Color.White, shakeRotation, new Vector2(8f, ((int)growthStage >= 3) ? 32 : 16), 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, ((int)growthStage == 0) ? 0.0001f : (baseSortPosition / 10000f));
		}
		else
		{
			if (!stump || (bool)falling)
			{
				if (IsLeafy())
				{
					spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f - 51f, tileLocation.Y * 64f - 16f)), shadowSourceRect, Color.White * ((float)Math.PI / 2f - Math.Abs(shakeRotation)), 0f, Vector2.Zero, 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1E-06f);
				}
				else
				{
					spriteBatch.Draw(Game1.mouseCursors_1_6, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f - 51f, tileLocation.Y * 64f - 16f)), new Microsoft.Xna.Framework.Rectangle(469, 298, 42, 31), Color.White * ((float)Math.PI / 2f - Math.Abs(shakeRotation)), 0f, Vector2.Zero, 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1E-06f);
				}
				Microsoft.Xna.Framework.Rectangle source_rect = treeTopSourceRect;
				if ((data.UseAlternateSpriteWhenSeedReady && hasSeed.Value) || (data.UseAlternateSpriteWhenNotShaken && !wasShakenToday.Value))
				{
					source_rect.X = 48;
				}
				else
				{
					source_rect.X = 0;
				}
				if ((bool)hasMoss)
				{
					source_rect.X = 96;
				}
				spriteBatch.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f + 64f)), source_rect, Color.White * alpha, shakeRotation, new Vector2(24f, 96f), 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (baseSortPosition + 2f) / 10000f - tileLocation.X / 1000000f);
			}
			Microsoft.Xna.Framework.Rectangle stumpSource = stumpSourceRect;
			if ((bool)hasMoss)
			{
				stumpSource.X += 96;
			}
			if (health.Value >= 1f || (!falling && health.Value > -99f))
			{
				spriteBatch.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + ((shakeTimer > 0f) ? ((float)Math.Sin(Math.PI * 2.0 / (double)shakeTimer) * 3f) : 0f), tileLocation.Y * 64f - 64f)), stumpSource, Color.White * alpha, 0f, Vector2.Zero, 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, baseSortPosition / 10000f);
			}
			if ((bool)stump && health.Value < 4f && health.Value > -99f)
			{
				spriteBatch.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + ((shakeTimer > 0f) ? ((float)Math.Sin(Math.PI * 2.0 / (double)shakeTimer) * 3f) : 0f), tileLocation.Y * 64f)), new Microsoft.Xna.Framework.Rectangle(Math.Min(2, (int)(3f - health.Value)) * 16, 144, 16, 16), Color.White * alpha, 0f, Vector2.Zero, 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (baseSortPosition + 1f) / 10000f);
			}
		}
		foreach (Leaf l in leaves)
		{
			spriteBatch.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, l.position), new Microsoft.Xna.Framework.Rectangle(16 + l.type % 2 * 8, 112 + l.type / 2 * 8, 8, 8), Color.White, l.rotation, Vector2.Zero, 4f, SpriteEffects.None, baseSortPosition / 10000f + 0.01f);
		}
	}
}
