using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.GameData;
using StardewValley.Objects;

namespace StardewValley.ItemTypeDefinitions;

/// <summary>Manages the data for 'floorpaper' flooring items.</summary>
public class FlooringDataDefinition : BaseItemDataDefinition
{
	/// <summary>The number of older floorings in <c>Maps\walls_and_floors</c> that aren't defined in a data asset.</summary>
	private const int LegacyFlooringCount = 88;

	/// <inheritdoc />
	public override string Identifier => "(FL)";

	/// <inheritdoc />
	public override IEnumerable<string> GetAllIds()
	{
		for (int i = 0; i < 88; i++)
		{
			yield return i.ToString();
		}
		List<ModWallpaperOrFlooring> data = DataLoader.AdditionalWallpaperFlooring(Game1.content);
		foreach (ModWallpaperOrFlooring set in data)
		{
			if (set.IsFlooring)
			{
				for (int i = 0; i < set.Count; i++)
				{
					yield return set.Id + ":" + i;
				}
			}
		}
	}

	/// <inheritdoc />
	public override bool Exists(string itemId)
	{
		if (itemId == null)
		{
			return false;
		}
		if (TryParseLegacyId(itemId, out var _))
		{
			return true;
		}
		ParseStandardId(itemId, out var id, out var index);
		ModWallpaperOrFlooring flooringSet = GetFlooringSet(id);
		return index < flooringSet?.Count;
	}

	/// <inheritdoc />
	public override ParsedItemData GetData(string itemId)
	{
		if (itemId != null)
		{
			if (TryParseLegacyId(itemId, out var legacyId))
			{
				return GetData(itemId, legacyId, "Maps\\walls_and_floors", null);
			}
			ParseStandardId(itemId, out var id, out var index);
			ModWallpaperOrFlooring data = GetFlooringSet(id);
			if (data != null)
			{
				return GetData(itemId, index, data.Texture, data);
			}
		}
		return null;
	}

	/// <inheritdoc />
	public override Item CreateItem(ParsedItemData data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (TryParseLegacyId(data.ItemId, out var legacyId))
		{
			return new Wallpaper(legacyId, isFloor: true);
		}
		ParseStandardId(data.ItemId, out var id, out var index);
		return new Wallpaper(id, index);
	}

	/// <inheritdoc />
	public override Rectangle GetSourceRect(ParsedItemData data, Texture2D texture, int spriteIndex)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (texture == null)
		{
			throw new ArgumentNullException("texture");
		}
		return Game1.getSourceRectForStandardTileSheet(texture, spriteIndex, 16, 48);
	}

	/// <summary>Try to parse the ID as a vanilla flooring ID that's not defined in <c>Data/AdditionalWallpaperFlooring</c>.</summary>
	/// <param name="raw">The item ID to parse.</param>
	/// <param name="legacyId">The parsed legacy ID, if applicable.</param>
	/// <returns>Returns whether the ID is a legacy vanilla ID.</returns>
	protected bool TryParseLegacyId(string raw, out int legacyId)
	{
		if (int.TryParse(raw, out legacyId) && legacyId >= 0)
		{
			return legacyId < 88;
		}
		return false;
	}

	/// <summary>Parse a standard flooring ID that should be defined in <c>Data/AdditionalWallpaperFlooring</c>. This may include a sprite index within the texture. For example, <c>ExampleMod.CustomFlooring:5</c> is a flooring at index 5 in the spritesheet texture defined under <c>ExampleMod.CustomFlooring</c> in <c>Data/AdditionalWallpaperFlooring</c>.</summary>
	/// <param name="raw">The item ID to parse.</param>
	/// <param name="id">The item ID without the index.</param>
	/// <param name="index">The sprite index, if any.</param>
	protected void ParseStandardId(string raw, out string id, out int index)
	{
		id = raw;
		index = 0;
		string[] parts = raw.Split(':', 2);
		if (parts.Length == 2 && int.TryParse(parts[1], out var parsedIndex))
		{
			id = parts[0];
			index = parsedIndex;
		}
	}

	/// <summary>Get a set of flooring items from the data asset.</summary>
	/// <param name="setId">The unqualified item ID.</param>
	protected ModWallpaperOrFlooring GetFlooringSet(string setId)
	{
		foreach (ModWallpaperOrFlooring set in DataLoader.AdditionalWallpaperFlooring(Game1.content))
		{
			if (set.Id == setId)
			{
				if (!set.IsFlooring)
				{
					return null;
				}
				return set;
			}
		}
		return null;
	}

	/// <summary>Get the base data for a flooring item.</summary>
	/// <param name="itemId">The unqualified item ID.</param>
	/// <param name="spriteIndex">The item's index within the sprite sheet.</param>
	/// <param name="textureName">The asset name for the sprite sheet to use when drawing the item to the screen.</param>
	/// <param name="rawData">The raw data fields from the underlying data asset if applicable, else <c>null</c>.</param>
	protected ParsedItemData GetData(string itemId, int spriteIndex, string textureName, object rawData)
	{
		return new ParsedItemData(this, itemId, spriteIndex, textureName, "Flooring", Game1.content.LoadString("Strings\\StringsFromCSFiles:Wallpaper.cs.13203"), Game1.content.LoadString("Strings\\StringsFromCSFiles:Wallpaper.cs.13205"), 0, null, rawData);
	}
}
