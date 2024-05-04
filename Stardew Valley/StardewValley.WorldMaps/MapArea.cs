using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.GameData.WorldMaps;
using StardewValley.TokenizableStrings;

namespace StardewValley.WorldMaps;

/// <summary>A smaller section of the map which is linked to one or more in-game locations. The map area might be edited/swapped depending on the context, have its own tooltip(s), or have its own player marker positions.</summary>
public class MapArea
{
	/// <summary>The cached value for <see cref="M:StardewValley.WorldMaps.MapArea.GetTextures" />.</summary>
	protected MapAreaTexture[] CachedTextures;

	/// <summary>The cached value for <see cref="M:StardewValley.WorldMaps.MapArea.GetTooltips" />.</summary>
	protected MapAreaTooltip[] CachedTooltips;

	/// <summary>The cached value for <see cref="M:StardewValley.WorldMaps.MapArea.GetWorldPositions" />.</summary>
	protected MapAreaPosition[] CachedWorldPositions;

	/// <summary>The cached value for <see cref="M:StardewValley.WorldMaps.MapArea.GetScrollText" />.</summary>
	protected string CachedScrollText;

	/// <summary>The unique identifier for the area.</summary>
	public string Id { get; }

	/// <summary>The large-scale part of the world (like the Valley) which contains this area.</summary>
	public MapRegion Region { get; }

	/// <summary>The underlying data.</summary>
	public WorldMapAreaData Data { get; }

	/// <summary>Construct an instance.</summary>
	/// <param name="region">The large-scale part of the world (like the Valley) which contains this area.</param>
	/// <param name="data">The underlying data.</param>
	public MapArea(MapRegion region, WorldMapAreaData data)
	{
		Data = data;
		Id = data.Id;
		Region = region;
	}

	/// <summary>Get the textures to draw onto the map (adjusted for pixel zoom), if any.</summary>
	public MapAreaTexture[] GetTextures()
	{
		if (CachedTextures == null)
		{
			if (Data.Textures.Count > 0)
			{
				List<MapAreaTexture> textures = new List<MapAreaTexture>();
				foreach (WorldMapTextureData entry in Data.Textures)
				{
					if (!GameStateQuery.CheckConditions(entry.Condition))
					{
						continue;
					}
					Texture2D texture = null;
					if (entry.Condition == "IS_CUSTOM_FARM_TYPE")
					{
						string textureName = Game1.whichModFarm?.WorldMapTexture;
						if (textureName == null)
						{
							continue;
						}
						texture = GetTexture(textureName);
						if (texture.Width <= 200)
						{
							entry.SourceRect = texture.Bounds;
						}
					}
					else
					{
						texture = GetTexture(entry.Texture);
					}
					Rectangle sourceRect = entry.SourceRect;
					if (sourceRect.IsEmpty)
					{
						sourceRect = new Rectangle(0, 0, texture.Width, texture.Height);
					}
					Rectangle mapPixelArea = entry.MapPixelArea;
					if (mapPixelArea.IsEmpty)
					{
						mapPixelArea = Data.PixelArea;
					}
					textures.Add(new MapAreaTexture(mapPixelArea: new Rectangle(mapPixelArea.X * 4, mapPixelArea.Y * 4, mapPixelArea.Width * 4, mapPixelArea.Height * 4), texture: texture, sourceRect: sourceRect));
				}
				CachedTextures = textures.ToArray();
			}
			else
			{
				CachedTextures = LegacyShims.EmptyArray<MapAreaTexture>();
			}
		}
		return CachedTextures;
	}

	/// <summary>Get the tooltips to draw onto the map, if any.</summary>
	public MapAreaTooltip[] GetTooltips()
	{
		if (CachedTooltips == null)
		{
			List<WorldMapTooltipData> tooltips2 = Data.Tooltips;
			if (tooltips2 != null && tooltips2.Count > 0)
			{
				List<MapAreaTooltip> tooltips = new List<MapAreaTooltip>();
				foreach (WorldMapTooltipData entry in Data.Tooltips)
				{
					if (GameStateQuery.CheckConditions(entry.Condition))
					{
						string text = (GameStateQuery.CheckConditions(entry.KnownCondition) ? TokenParser.ParseText(Utility.TrimLines(entry.Text)) : "???");
						if (!string.IsNullOrWhiteSpace(text))
						{
							tooltips.Add(new MapAreaTooltip(this, entry, text));
						}
					}
				}
				CachedTooltips = tooltips.ToArray();
			}
			else
			{
				CachedTooltips = LegacyShims.EmptyArray<MapAreaTooltip>();
			}
		}
		return CachedTooltips;
	}

	/// <summary>Get all valid world positions in this area.</summary>
	public IEnumerable<MapAreaPosition> GetWorldPositions()
	{
		if (CachedWorldPositions == null)
		{
			List<MapAreaPosition> positions = new List<MapAreaPosition>();
			foreach (WorldMapAreaPositionData entry in Data.WorldPositions)
			{
				if (GameStateQuery.CheckConditions(entry.Condition))
				{
					positions.Add(new MapAreaPosition(this, entry));
				}
			}
			CachedWorldPositions = positions.ToArray();
		}
		return CachedWorldPositions;
	}

	/// <summary>Get a valid world position matching the given values, if any.</summary>
	/// <param name="locationName">The location name containing the tile.</param>
	/// <param name="contextName">The location's context name.</param>
	/// <param name="tile">The tile coordinate to match.</param>
	public MapAreaPosition GetWorldPosition(string locationName, string contextName, Point tile)
	{
		foreach (MapAreaPosition position in GetWorldPositions())
		{
			if (position.Matches(locationName, contextName, tile))
			{
				return position;
			}
		}
		return null;
	}

	/// <summary>Get the translated tooltip text to display when hovering the cursor over the map area.</summary>
	public virtual string GetScrollText()
	{
		if (CachedScrollText == null)
		{
			CachedScrollText = TokenParser.ParseText(Utility.TrimLines(Data.ScrollText));
		}
		return CachedScrollText;
	}

	/// <summary>Get the texture to load for an asset name.</summary>
	/// <param name="assetName">The asset name to load.</param>
	private Texture2D GetTexture(string assetName)
	{
		if (Game1.season != 0)
		{
			string seasonalName = assetName + "_" + Game1.currentSeason.ToLower();
			if (Game1.content.DoesAssetExist<Texture2D>(seasonalName))
			{
				return Game1.content.Load<Texture2D>(seasonalName);
			}
		}
		return Game1.content.Load<Texture2D>(assetName);
	}
}
