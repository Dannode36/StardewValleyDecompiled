using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.ObjectModel;

namespace StardewValley.Extensions;

/// <summary>Provides utility extension methods on MonoGame and xTile types.</summary>
public static class FrameworkExtensions
{
	/// <summary>Get a subset of the screen viewport that's guaranteed to be visible on a lower-quality display.</summary>
	/// <param name="viewport">The viewport pixel area.</param>
	public static Microsoft.Xna.Framework.Rectangle GetTitleSafeArea(this Viewport viewport)
	{
		return viewport.TitleSafeArea;
	}

	/// <summary>Get the point positions within the rectangle.</summary>
	/// <param name="rect">The rectangle area.</param>
	public static IEnumerable<Point> GetPoints(this Microsoft.Xna.Framework.Rectangle rect)
	{
		int right = rect.Right;
		int bottom = rect.Bottom;
		for (int y = rect.Y; y < bottom; y++)
		{
			for (int x = rect.X; x < right; x++)
			{
				yield return new Point(x, y);
			}
		}
	}

	/// <summary>Get the integer <see cref="T:Microsoft.Xna.Framework.Vector2" /> positions within the rectangle.</summary>
	/// <param name="rect">The rectangle area.</param>
	public static IEnumerable<Vector2> GetVectors(this Microsoft.Xna.Framework.Rectangle rect)
	{
		int right = rect.Right;
		int bottom = rect.Bottom;
		for (int y = rect.Y; y < bottom; y++)
		{
			for (int x = rect.X; x < right; x++)
			{
				yield return new Vector2(x, y);
			}
		}
	}

	/// <summary>Get a new rectangle with the same values as this instance.</summary>
	/// <param name="rect">The rectangle to clone.</param>
	public static Microsoft.Xna.Framework.Rectangle Clone(this Microsoft.Xna.Framework.Rectangle rect)
	{
		return new Microsoft.Xna.Framework.Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
	}

	/// <summary>Get a property value as a string, if it exists.</summary>
	/// <param name="properties">The property collection to search.</param>
	/// <param name="key">The property key to fetch.</param>
	/// <param name="value">The property value, if found.</param>
	/// <returns>Returns whether the property value was found.</returns>
	public static bool TryGetValue(this IPropertyCollection properties, string key, out string value)
	{
		if (!properties.TryGetValue(key, out var propertyValue))
		{
			value = null;
			return false;
		}
		value = propertyValue;
		return true;
	}

	/// <summary>Add a property if the key isn't already present.</summary>
	/// <param name="properties">The properties to modify.</param>
	/// <param name="key">The key of the property to add.</param>
	/// <param name="value">The value of the property to add.</param>
	/// <returns>Returns whether the value was successfully added.</returns>
	public static bool TryAdd(this IPropertyCollection properties, string key, string value)
	{
		if (properties.ContainsKey(key))
		{
			return false;
		}
		properties.Add(key, new PropertyValue(value));
		return true;
	}

	/// <summary>Get a map layer by ID, or throw an exception if it's not found.</summary>
	/// <param name="map">The map whose layer to get.</param>
	/// <param name="layerId">The layer ID.</param>
	/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">The <paramref name="layerId" /> doesn't match a layer in the map.</exception>
	public static Layer RequireLayer(this Map map, string layerId)
	{
		return map.GetLayer(layerId) ?? throw new KeyNotFoundException($"The '{map.assetPath}' map doesn't have required layer {layerId}.");
	}

	/// <summary>Get the tile index at the given layer coordinate.</summary>
	/// <param name="map">The map whose tiles to check.</param>
	/// <param name="x">The tile X coordinate.</param>
	/// <param name="y">The tile Y coordinate.</param>
	/// <param name="layerId">The layer whose tiles to check.</param>
	/// <returns>Returns the matching tile's index, or <c>-1</c> if no tile was found.</returns>
	public static int GetTileIndexAt(this Map map, int x, int y, string layerId)
	{
		return map?.GetLayer(layerId)?.Tiles[x, y]?.TileIndex ?? (-1);
	}

	/// <summary>Get the tile index at the given layer coordinate.</summary>
	/// <param name="map">The map whose tiles to check.</param>
	/// <param name="tile">The tile coordinates.</param>
	/// <param name="layerId">The layer whose tiles to check.</param>
	/// <returns>Returns the matching tile's index, or <c>-1</c> if no tile was found.</returns>
	public static int GetTileIndexAt(this Map map, Location tile, string layerId)
	{
		return map?.GetLayer(layerId)?.Tiles[tile.X, tile.Y]?.TileIndex ?? (-1);
	}

	/// <summary>Get the tile index at the given layer coordinate.</summary>
	/// <param name="layer">The layer whose tiles to check.</param>
	/// <param name="tile">The tile coordinates.</param>
	/// <returns>Returns the matching tile's index, or <c>-1</c> if no tile was found.</returns>
	public static int GetTileIndexAt(this Layer layer, Location tile)
	{
		return layer?.Tiles[tile.X, tile.Y]?.TileIndex ?? (-1);
	}

	/// <summary>Get the tile index at the given layer coordinate.</summary>
	/// <param name="layer">The layer whose tiles to check.</param>
	/// <param name="x">The tile X coordinate.</param>
	/// <param name="y">The tile Y coordinate.</param>
	/// <returns>Returns the matching tile's index, or <c>-1</c> if no tile was found.</returns>
	public static int GetTileIndexAt(this Layer layer, int x, int y)
	{
		return layer?.Tiles[x, y]?.TileIndex ?? (-1);
	}
}
