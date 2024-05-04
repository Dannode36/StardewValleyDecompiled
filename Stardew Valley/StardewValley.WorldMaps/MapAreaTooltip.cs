using Microsoft.Xna.Framework;
using StardewValley.GameData.WorldMaps;

namespace StardewValley.WorldMaps;

/// <summary>A tooltip shown when hovering over parts of a larger <see cref="T:StardewValley.WorldMaps.MapArea" /> on the world map.</summary>
public class MapAreaTooltip
{
	/// <summary>The cached value for <see cref="M:StardewValley.WorldMaps.MapAreaTooltip.GetPixelArea" />.</summary>
	protected Rectangle? CachedPixelArea;

	/// <summary>The map area which contains this position.</summary>
	public MapArea Area { get; }

	/// <summary>The underlying tooltip data.</summary>
	public WorldMapTooltipData Data { get; }

	/// <summary>The tooltip text to display.</summary>
	public string Text { get; }

	/// <summary>A unique ID for this tooltip within the map region.</summary>
	public string NamespacedId { get; }

	/// <summary>Construct an instance.</summary>
	/// <param name="mapArea">The map area which contains this position.</param>
	/// <param name="data">The underlying map position data.</param>
	/// <param name="text">The tooltip text to display.</param>
	public MapAreaTooltip(MapArea mapArea, WorldMapTooltipData data, string text)
	{
		Area = mapArea;
		Data = data;
		Text = text;
		NamespacedId = mapArea.Id + "/" + data.Id;
	}

	/// <summary>Get the pixel area within the map which can be hovered to show this tooltip, adjusted for pixel zoom.</summary>
	public Rectangle GetPixelArea()
	{
		Rectangle? cachedPixelArea = CachedPixelArea;
		if (!cachedPixelArea.HasValue)
		{
			Rectangle area = Data.PixelArea;
			if (area.IsEmpty)
			{
				area = Area.Data.PixelArea;
			}
			CachedPixelArea = new Rectangle(area.X * 4, area.Y * 4, area.Width * 4, area.Height * 4);
		}
		return CachedPixelArea.Value;
	}
}
