using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Enchantments;
using StardewValley.Extensions;
using StardewValley.GameData.GiantCrops;
using StardewValley.Internal;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Tools;

namespace StardewValley.TerrainFeatures;

public class GiantCrop : ResourceClump
{
	/// <summary>A cache of giant crops by small-crop-ID for <see cref="M:StardewValley.TerrainFeatures.GiantCrop.GetGiantCropsFor(System.String)" />.</summary>
	private static readonly Dictionary<string, List<KeyValuePair<string, GiantCropData>>> CacheByCropId = new Dictionary<string, List<KeyValuePair<string, GiantCropData>>>();

	/// <summary>The <see cref="F:StardewValley.Game1.ticks" /> value when the <see cref="F:StardewValley.TerrainFeatures.GiantCrop.CacheByCropId" /> was last reset.</summary>
	private static int CacheTick;

	/// <summary>The backing field for <see cref="P:StardewValley.TerrainFeatures.GiantCrop.Id" />.</summary>
	[XmlElement("id")]
	public readonly NetString netId = new NetString();

	/// <summary>A unique ID for this giant crop matching its entry in <c>Data/GiantCrops</c>.</summary>
	[XmlIgnore]
	public string Id
	{
		get
		{
			if (netId.Value == null)
			{
				netId.Value = GetIdFromLegacySpriteIndex(parentSheetIndex.Value);
			}
			return netId.Value;
		}
		set
		{
			netId.Value = value;
		}
	}

	/// <summary>Construct an empty instance.</summary>
	public GiantCrop()
	{
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="id">A unique ID for this giant crop matching its entry in <c>Data/GiantCrops</c>.</param>
	/// <param name="tile">The top-left tile position for the giant crop.</param>
	public GiantCrop(string id, Vector2 tile)
		: this()
	{
		Tile = tile;
		Id = id;
		GiantCropData data = GetData();
		width.Value = data?.TileSize.X ?? 3;
		height.Value = data?.TileSize.Y ?? 3;
		health.Value = data?.Health ?? 3;
	}

	public override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(netId, "netId");
	}

	public override void draw(SpriteBatch spriteBatch)
	{
		Vector2 tileLocation = Tile;
		GiantCropData data = GetData();
		if (data != null)
		{
			Texture2D texture = Game1.content.Load<Texture2D>(data.Texture);
			spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, tileLocation * 64f - new Vector2((shakeTimer > 0f) ? ((float)Math.Sin(Math.PI * 2.0 / (double)shakeTimer) * 2f) : 0f, 64f)), new Rectangle(data.TexturePosition.X, data.TexturePosition.Y, 16 * data.TileSize.X, 16 * (data.TileSize.Y + 1)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (tileLocation.Y + (float)data.TileSize.Y) * 64f / 10000f);
		}
		else
		{
			IItemDataDefinition itemType = ItemRegistry.RequireTypeDefinition("(O)");
			spriteBatch.Draw(itemType.GetErrorTexture(), Game1.GlobalToLocal(Game1.viewport, tileLocation * 64f - new Vector2((shakeTimer > 0f) ? ((float)Math.Sin(Math.PI * 2.0 / (double)shakeTimer) * 2f) : 0f, 64f)), itemType.GetErrorSourceRect(), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (tileLocation.Y + 2f) * 64f / 10000f);
		}
	}

	public override bool performToolAction(Tool t, int damage, Vector2 tileLocation)
	{
		if (!(t is Axe))
		{
			return false;
		}
		GameLocation location = Location;
		Farmer player = t.getLastFarmerToUse() ?? Game1.player;
		int power = (int)t.upgradeLevel / 2 + 1;
		float healthDeducted = Math.Min(health.Value, power);
		GiantCropData data = GetData();
		Random r = ((!Game1.IsMultiplayer) ? Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed, (double)tileLocation.X * 7.0, (double)tileLocation.Y * 11.0) : (Game1.recentMultiplayerRandom = Utility.CreateRandom((double)tileLocation.X * 1000.0, tileLocation.Y)));
		location.playSound("axchop", tileLocation);
		Game1.createRadialDebris(Game1.currentLocation, 12, (int)tileLocation.X + (int)width / 2, (int)tileLocation.Y + (int)height / 2, r.Next(4, 9), resource: false);
		if (shakeTimer <= 0f)
		{
			shakeTimer = 100f;
			base.NeedsUpdate = true;
		}
		if (t.hasEnchantmentOfType<ShavingEnchantment>() && r.NextBool((float)power / 5f) && data?.HarvestItems != null)
		{
			foreach (GiantCropHarvestItemData drop in data.HarvestItems)
			{
				Item item = TryGetDrop(drop, r, player, isShaving: true, healthDeducted);
				if (item != null)
				{
					if (Id.Equals("QiFruit") && !Game1.player.team.SpecialOrderActive("QiChallenge2"))
					{
						break;
					}
					Debris d = new Debris(item, new Vector2((tileLocation.X + (float)((int)width / 2)) * 64f, (tileLocation.Y + (float)((int)height / 2)) * 64f), Game1.player.getStandingPosition());
					d.Chunks[0].xVelocity.Value += (float)r.Next(-10, 11) / 10f;
					d.chunkFinalYLevel = (int)(tileLocation.Y * 64f + 128f);
					location.debris.Add(d);
				}
			}
		}
		health.Value -= power;
		if (health.Value <= 0f)
		{
			t.getLastFarmerToUse().gainExperience(5, 50 * (((int)t.getLastFarmerToUse().luckLevel + 1) / 2));
			if (location.HasUnlockedAreaSecretNotes(t.getLastFarmerToUse()))
			{
				Object o = location.tryToCreateUnseenSecretNote(t.getLastFarmerToUse());
				if (o != null)
				{
					Game1.createItemDebris(o, tileLocation * 64f, -1, location);
				}
			}
			if (data?.HarvestItems != null)
			{
				foreach (GiantCropHarvestItemData drop in data.HarvestItems)
				{
					Item item = TryGetDrop(drop, r, player, isShaving: false, healthDeducted);
					if (item == null)
					{
						continue;
					}
					if (Id.Equals("QiFruit") && !Game1.player.team.SpecialOrderActive("QiChallenge2"))
					{
						if (!Game1.player.mailReceived.Contains("GiantQiFruitMessage"))
						{
							Game1.player.mailReceived.Add("GiantQiFruitMessage");
							Game1.chatBox.addMessage(Game1.content.LoadString("Strings\\1_6_Strings:GiantQiFruitMessage"), new Color(100, 50, 255));
						}
						Game1.createMultipleItemDebris(ItemRegistry.Create("(O)MysteryBox"), new Vector2((int)tileLocation.X + (int)width / 2, (int)tileLocation.Y + (int)width / 2) * 64f, -2, location);
					}
					else
					{
						Game1.createMultipleItemDebris(item, new Vector2((int)tileLocation.X + (int)width / 2, (int)tileLocation.Y + (int)width / 2) * 64f, -2, location);
						Game1.setRichPresence("giantcrop", item.Name);
					}
				}
			}
			Game1.createRadialDebris(Game1.currentLocation, 12, (int)tileLocation.X + (int)width / 2, (int)tileLocation.Y + (int)width / 2, r.Next(4, 9), resource: false);
			location.playSound("stumpCrack", tileLocation);
			for (int x = 0; x < (int)width; x++)
			{
				for (int y = 0; y < (int)height; y++)
				{
					float animationInterval = Utility.RandomFloat(80f, 110f);
					if ((int)width >= 2 && (int)height >= 2 && (x == 0 || x == (int)width - 2) && (y == 0 || y == (int)height - 2))
					{
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(5, (tileLocation + new Vector2((float)x + 0.5f, (float)y + 0.5f)) * 64f, Color.White, 8, flipped: false, 70f));
					}
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(5, (tileLocation + new Vector2(x, y)) * 64f, Color.White, 8, flipped: false, animationInterval));
				}
			}
			return true;
		}
		return false;
	}

	/// <summary>Get the giant crop's data from <c>Data/GiantCrops</c>, if found.</summary>
	public GiantCropData GetData()
	{
		if (!TryGetData(Id, out var data))
		{
			return null;
		}
		return data;
	}

	/// <summary>Try to get a giant crop's data from <c>Data/GiantCrops</c>.</summary>
	/// <param name="id">The giant crop ID (i.e. the key in <c>Data/GiantCrops</c>).</param>
	/// <param name="data">The giant crop data, if found.</param>
	/// <returns>Returns whether the giant crop data was found.</returns>
	public static bool TryGetData(string id, out GiantCropData data)
	{
		if (id == null)
		{
			data = null;
			return false;
		}
		return DataLoader.GiantCrops(Game1.content).TryGetValue(id, out data);
	}

	/// <summary>Get the giant crops that can grow from a given crop ID.</summary>
	/// <param name="cropId">The qualified or unqualified item ID for the crop's harvest item.</param>
	public static IReadOnlyList<KeyValuePair<string, GiantCropData>> GetGiantCropsFor(string cropId)
	{
		cropId = ItemRegistry.QualifyItemId(cropId);
		if (cropId != null)
		{
			RebuildCropIdCacheIfNeeded();
			if (CacheByCropId.TryGetValue(cropId, out var giantCrops))
			{
				return giantCrops;
			}
		}
		return LegacyShims.EmptyArray<KeyValuePair<string, GiantCropData>>();
	}

	/// <summary>Rebuild the <see cref="F:StardewValley.TerrainFeatures.GiantCrop.CacheByCropId" /> cache, if it was generated before the current tick.</summary>
	/// <param name="forceRebuild">Whether to force rebuilding the cache even if it was generated in the current tick.</param>
	/// <returns>Returns whether the cache was rebuilt.</returns>
	public static bool RebuildCropIdCacheIfNeeded(bool forceRebuild = false)
	{
		if (!forceRebuild && CacheTick == Game1.ticks)
		{
			return false;
		}
		CacheTick = Game1.ticks;
		CacheByCropId.Clear();
		foreach (KeyValuePair<string, GiantCropData> pair in DataLoader.GiantCrops(Game1.content))
		{
			string fromItemId = ItemRegistry.QualifyItemId(pair.Value.FromItemId);
			if (fromItemId != null)
			{
				if (!CacheByCropId.TryGetValue(fromItemId, out var list))
				{
					list = (CacheByCropId[fromItemId] = new List<KeyValuePair<string, GiantCropData>>());
				}
				list.Add(pair);
			}
		}
		return true;
	}

	/// <summary>Get a dropped item if its fields match.</summary>
	/// <param name="drop">The drop data.</param>
	/// <param name="r">The RNG to use for random checks.</param>
	/// <param name="targetFarmer">The player interacting with the giant crop.</param>
	/// <param name="isShaving">Whether the item is being dropped for the Shaving enchantment (true), instead of because the giant crop was broken (false).</param>
	/// <param name="healthDeducted">The health points deducted by the tool hit.</param>
	/// <returns>Returns the produced item (if any), else <c>null</c>.</returns>
	private Item TryGetDrop(GiantCropHarvestItemData drop, Random r, Farmer targetFarmer, bool isShaving, float healthDeducted)
	{
		if (!r.NextBool(drop.Chance))
		{
			return null;
		}
		if (drop.Condition != null && !GameStateQuery.CheckConditions(drop.Condition, Location, targetFarmer, null, null, r))
		{
			return null;
		}
		if (drop.ForShavingEnchantment.HasValue && drop.ForShavingEnchantment != isShaving)
		{
			return null;
		}
		Item item = ItemQueryResolver.TryResolveRandomItem(drop, new ItemQueryContext(Location, targetFarmer, r), avoidRepeat: false, null, null, null, delegate(string query, string error)
		{
			Game1.log.Error($"Giant crop '{Id}' failed parsing item query '{query}' for harvest item '{drop.Id}': {error}");
		});
		if (isShaving)
		{
			AdjustStackSizeWhenShaving(item, drop.ScaledMinStackWhenShaving, drop.ScaledMaxStackWhenShaving, healthDeducted, r);
		}
		return item;
	}

	/// <summary>Adjust the item's stack size for the scaled min/max values, if set.</summary>
	/// <param name="item">The item whose stack size to adjust.</param>
	/// <param name="min">The minimum stack size to apply, scaled to the <paramref name="healthDeducted" />.</param>
	/// <param name="max">The maximum stack size to apply, scaled to the <paramref name="healthDeducted" />.</param>
	/// <param name="healthDeducted">The health points deducted by the tool hit.</param>
	/// <param name="random">The RNG to use when randomizing the stack size.</param>
	private void AdjustStackSizeWhenShaving(Item item, int? min, int? max, float healthDeducted, Random random)
	{
		if (item != null && (min.HasValue || max.HasValue))
		{
			if (min.HasValue)
			{
				min = (int)((float?)min * healthDeducted).Value;
			}
			if (max.HasValue)
			{
				max = (int)((float?)max * healthDeducted).Value;
			}
			if (min.HasValue && max.HasValue)
			{
				item.Stack = random.Next(min.Value, max.Value + 1);
			}
			else if (item.Stack < min)
			{
				item.Stack = min.Value;
			}
			else if (item.Stack > max)
			{
				item.Stack = max.Value;
			}
		}
	}

	/// <summary>Get the giant crop ID which matches a pre-1.6 parent sheet index.</summary>
	/// <param name="spriteIndex">The parent sheet index.</param>
	private string GetIdFromLegacySpriteIndex(int spriteIndex)
	{
		return spriteIndex switch
		{
			190 => "Cauliflower", 
			254 => "Melon", 
			276 => "Pumpkin", 
			_ => spriteIndex.ToString(), 
		};
	}
}
