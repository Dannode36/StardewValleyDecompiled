using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.WorldMaps;

namespace StardewValley.Menus;

/// <summary>The in-game world map view.</summary>
public class MapPage : IClickableMenu
{
	/// <summary>The world map debug lines to draw.</summary>
	[Flags]
	public enum WorldMapDebugLineType
	{
		/// <summary>Don't show debug lines on the map.</summary>
		None = 0,
		/// <summary>Highlight map areas.</summary>
		Areas = 1,
		/// <summary>Highlight map position rectangles.</summary>
		Positions = 2,
		/// <summary>Highlight tooltip rectangles.</summary>
		Tooltips = 4,
		/// <summary>Highlight all types.</summary>
		All = -1
	}

	/// <summary>The world map debug lines to draw, if any.</summary>
	public static WorldMapDebugLineType EnableDebugLines;

	/// <summary>The map position containing the current player.</summary>
	public readonly MapAreaPosition mapPosition;

	/// <summary>The map region containing the <see cref="F:StardewValley.Menus.MapPage.mapPosition" />.</summary>
	public readonly MapRegion mapRegion;

	/// <summary>The smaller sections of the map linked to one or more in-game locations. Each map area might be edited/swapped depending on the context, have its own tooltip(s), or have its own player marker positions.</summary>
	public readonly MapArea[] mapAreas;

	/// <summary>The translated scroll text to show at the bottom of the map, if any.</summary>
	public readonly string scrollText;

	/// <summary>The default component ID in <see cref="F:StardewValley.Menus.MapPage.points" /> to which to snap the controller cursor by default.</summary>
	public readonly int defaultComponentID;

	/// <summary>The pixel area on screen containing all the map areas being drawn.</summary>
	public Rectangle mapBounds;

	/// <summary>The tooltips to render, indexed by <see cref="P:StardewValley.WorldMaps.MapAreaTooltip.NamespacedId" />.</summary>
	public readonly Dictionary<string, ClickableComponent> points = new Dictionary<string, ClickableComponent>(StringComparer.OrdinalIgnoreCase);

	/// <summary>The tooltip text being drawn.</summary>
	public string hoverText = "";

	public MapPage(int x, int y, int width, int height)
		: base(x, y, width, height)
	{
		WorldMapManager.ReloadData();
		Point playerTile = GetNormalizedPlayerTile(Game1.player);
		mapPosition = WorldMapManager.GetPositionData(Game1.player.currentLocation, playerTile) ?? WorldMapManager.GetPositionData(Game1.getFarm(), Point.Zero);
		mapRegion = mapPosition.Region;
		mapAreas = mapRegion.GetAreas();
		scrollText = mapPosition.GetScrollText(playerTile);
		mapBounds = mapRegion.GetMapPixelBounds();
		int id = (defaultComponentID = 1000);
		MapArea[] array = mapAreas;
		for (int i = 0; i < array.Length; i++)
		{
			MapAreaTooltip[] tooltips = array[i].GetTooltips();
			foreach (MapAreaTooltip tooltip in tooltips)
			{
				Rectangle pixelArea = tooltip.GetPixelArea();
				pixelArea = new Rectangle(mapBounds.X + pixelArea.X, mapBounds.Y + pixelArea.Y, pixelArea.Width, pixelArea.Height);
				id++;
				ClickableComponent component = new ClickableComponent(pixelArea, tooltip.NamespacedId)
				{
					myID = id,
					label = tooltip.Text
				};
				points[tooltip.NamespacedId] = component;
				if (tooltip.NamespacedId == "Farm/Default")
				{
					defaultComponentID = id;
				}
			}
		}
		array = mapAreas;
		for (int i = 0; i < array.Length; i++)
		{
			MapAreaTooltip[] tooltips = array[i].GetTooltips();
			foreach (MapAreaTooltip tooltip in tooltips)
			{
				if (points.TryGetValue(tooltip.NamespacedId, out var component))
				{
					SetNeighborId(component, "left", tooltip.Data.LeftNeighbor);
					SetNeighborId(component, "right", tooltip.Data.RightNeighbor);
					SetNeighborId(component, "up", tooltip.Data.UpNeighbor);
					SetNeighborId(component, "down", tooltip.Data.DownNeighbor);
				}
			}
		}
	}

	public override void populateClickableComponentList()
	{
		base.populateClickableComponentList();
		allClickableComponents.AddRange(points.Values);
	}

	/// <summary>Set a controller navigation ID for a tooltip component.</summary>
	/// <param name="component">The tooltip component whose neighbor ID to set.</param>
	/// <param name="direction">The direction to set.</param>
	/// <param name="neighborKeys">The tooltip neighbor keys to match. See remarks on <see cref="F:StardewValley.GameData.WorldMaps.WorldMapTooltipData.LeftNeighbor" /> for details on the format.</param>
	/// <returns>Returns whether the <paramref name="neighborKeys" /> matched an existing tooltip neighbor ID.</returns>
	public void SetNeighborId(ClickableComponent component, string direction, string neighborKeys)
	{
		if (string.IsNullOrWhiteSpace(neighborKeys))
		{
			return;
		}
		if (!TryGetNeighborId(neighborKeys, out var neighborId, out var foundIgnore))
		{
			if (!foundIgnore)
			{
				Game1.log.Warn($"World map tooltip '{component.name}' has {direction} neighbor keys '{neighborKeys}' which don't match a tooltip namespaced ID or alias.");
			}
			return;
		}
		switch (direction)
		{
		case "left":
			component.leftNeighborID = neighborId;
			break;
		case "right":
			component.rightNeighborID = neighborId;
			break;
		case "up":
			component.upNeighborID = neighborId;
			break;
		case "down":
			component.downNeighborID = neighborId;
			break;
		default:
			Game1.log.Warn("Can't set neighbor ID for unknown direction '" + direction + "'.");
			break;
		}
	}

	/// <summary>Get the controller navigation ID for a tooltip neighbor field value.</summary>
	/// <param name="keys">The tooltip neighbor keys to match. See remarks on <see cref="F:StardewValley.GameData.WorldMaps.WorldMapTooltipData.LeftNeighbor" /> for details on the format.</param>
	/// <param name="id">The matching controller navigation ID, if found.</param>
	/// <param name="foundIgnore">Whether the neighbor IDs contains <c>ignore</c>, which indicates it should be skipped silently if none match.</param>
	/// <param name="isAlias">Whether the <paramref name="keys" /> are from an alias in <see cref="F:StardewValley.GameData.WorldMaps.WorldMapRegionData.MapNeighborIdAliases" />.</param>
	/// <returns>Returns <c>true</c> if the neighbor ID was found, else <c>false</c>.</returns>
	public bool TryGetNeighborId(string keys, out int id, out bool foundIgnore, bool isAlias = false)
	{
		foundIgnore = false;
		if (!string.IsNullOrWhiteSpace(keys))
		{
			string[] array = keys.Split(',', StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < array.Length; i++)
			{
				string key = array[i].Trim();
				if (string.Equals(key, "ignore", StringComparison.OrdinalIgnoreCase))
				{
					foundIgnore = true;
					continue;
				}
				if (points.TryGetValue(key, out var neighbor))
				{
					id = neighbor.myID;
					return true;
				}
				if (!isAlias && mapRegion.Data.MapNeighborIdAliases.TryGetValue(key, out var alias))
				{
					if (TryGetNeighborId(alias, out id, out var localFoundIgnore, isAlias: true))
					{
						foundIgnore |= localFoundIgnore;
						return true;
					}
					foundIgnore |= localFoundIgnore;
				}
			}
		}
		id = -1;
		return false;
	}

	public override void snapToDefaultClickableComponent()
	{
		currentlySnappedComponent = getComponentWithID(defaultComponentID);
		snapCursorToCurrentSnappedComponent();
	}

	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		foreach (ClickableComponent c in points.Values)
		{
			if (c.containsPoint(x, y))
			{
				if (c.name == "Beach/LonelyStone")
				{
					Game1.playSound("stoneCrack");
				}
				else if (c.name == "Forest/SewerPipe")
				{
					Game1.playSound("shadowpeep");
				}
				return;
			}
		}
		if (Game1.activeClickableMenu is GameMenu gameMenu)
		{
			gameMenu.changeTab(gameMenu.lastOpenedNonMapTab);
		}
	}

	public override void performHoverAction(int x, int y)
	{
		hoverText = "";
		foreach (ClickableComponent c in points.Values)
		{
			if (c.containsPoint(x, y))
			{
				hoverText = c.label;
				break;
			}
		}
	}

	public override void draw(SpriteBatch b)
	{
		drawMap(b);
		drawMiniPortraits(b);
		drawScroll(b);
		drawTooltip(b);
	}

	public override void receiveKeyPress(Keys key)
	{
		if (Game1.options.doesInputListContain(Game1.options.mapButton, key) && readyToClose())
		{
			exitThisMenu();
		}
		base.receiveKeyPress(key);
	}

	public virtual void drawMiniPortraits(SpriteBatch b)
	{
		Dictionary<Vector2, int> usedPositions = new Dictionary<Vector2, int>();
		foreach (Farmer player in Game1.getOnlineFarmers())
		{
			Point tile = GetNormalizedPlayerTile(player);
			MapAreaPosition positionData = (player.IsLocalPlayer ? mapPosition : WorldMapManager.GetPositionData(player.currentLocation, tile));
			if (positionData != null && !(positionData.Region.Id != mapRegion.Id))
			{
				Vector2 pos = positionData.GetMapPixelPosition(player.currentLocation, tile);
				pos = new Vector2(pos.X + (float)mapBounds.X - 32f, pos.Y + (float)mapBounds.Y - 32f);
				usedPositions.TryGetValue(pos, out var count);
				usedPositions[pos] = count + 1;
				if (count > 0)
				{
					pos += new Vector2(48 * (count % 2), 48 * (count / 2));
				}
				player.FarmerRenderer.drawMiniPortrat(b, pos, 0.00011f, 4f, 2, player);
			}
		}
	}

	public virtual void drawScroll(SpriteBatch b)
	{
		if (scrollText != null)
		{
			float scrollDrawY = yPositionOnScreen + height + 32 + 4;
			float scrollDrawBottom = scrollDrawY + 80f;
			if (scrollDrawBottom > (float)Game1.uiViewport.Height)
			{
				scrollDrawY -= scrollDrawBottom - (float)Game1.uiViewport.Height;
			}
			SpriteText.drawStringWithScrollCenteredAt(b, scrollText, xPositionOnScreen + width / 2, (int)scrollDrawY);
		}
	}

	public virtual void drawMap(SpriteBatch b, bool drawBorders = true)
	{
		if (drawBorders)
		{
			int boxY = mapBounds.Y - 96;
			Game1.drawDialogueBox(mapBounds.X - 32, boxY, (mapBounds.Width + 16) * 4, (mapBounds.Height + 32) * 4, speaker: false, drawOnlyBox: true);
		}
		float sortLayer = 0.86f;
		MapAreaTexture baseTexture = mapRegion.GetBaseTexture();
		if (baseTexture != null)
		{
			Rectangle destRect = baseTexture.GetOffsetMapPixelArea(mapBounds.X, mapBounds.Y);
			b.Draw(baseTexture.Texture, destRect, baseTexture.SourceRect, Color.White, 0f, Vector2.Zero, SpriteEffects.None, sortLayer);
			sortLayer += 0.001f;
		}
		MapArea[] array = mapAreas;
		for (int i = 0; i < array.Length; i++)
		{
			MapAreaTexture[] textures = array[i].GetTextures();
			foreach (MapAreaTexture overlay in textures)
			{
				Rectangle destRect = overlay.GetOffsetMapPixelArea(mapBounds.X, mapBounds.Y);
				b.Draw(overlay.Texture, destRect, overlay.SourceRect, Color.White, 0f, Vector2.Zero, SpriteEffects.None, sortLayer);
				sortLayer += 0.001f;
			}
		}
		if (EnableDebugLines == WorldMapDebugLineType.None)
		{
			return;
		}
		array = mapAreas;
		foreach (MapArea area in array)
		{
			if (EnableDebugLines.HasFlag(WorldMapDebugLineType.Tooltips))
			{
				MapAreaTooltip[] tooltips = area.GetTooltips();
				for (int j = 0; j < tooltips.Length; j++)
				{
					Rectangle pixelArea = tooltips[j].GetPixelArea();
					pixelArea = new Rectangle(mapBounds.X + pixelArea.X, mapBounds.Y + pixelArea.Y, pixelArea.Width, pixelArea.Height);
					Utility.DrawSquare(b, pixelArea, 2, Color.Blue);
				}
			}
			if (EnableDebugLines.HasFlag(WorldMapDebugLineType.Areas))
			{
				Rectangle pixelArea = area.Data.PixelArea;
				if (pixelArea.Width > 0 || pixelArea.Height > 0)
				{
					pixelArea = new Rectangle(mapBounds.X + pixelArea.X * 4, mapBounds.Y + pixelArea.Y * 4, pixelArea.Width * 4, pixelArea.Height * 4);
					Utility.DrawSquare(b, pixelArea, 4, Color.Black);
				}
			}
			if (!EnableDebugLines.HasFlag(WorldMapDebugLineType.Positions))
			{
				continue;
			}
			foreach (MapAreaPosition worldPosition in area.GetWorldPositions())
			{
				Rectangle pixelArea = worldPosition.GetPixelArea();
				pixelArea = new Rectangle(mapBounds.X + pixelArea.X, mapBounds.Y + pixelArea.Y, pixelArea.Width, pixelArea.Height);
				Utility.DrawSquare(b, pixelArea, 2, Color.Red);
			}
		}
	}

	public virtual void drawTooltip(SpriteBatch b)
	{
		if (!string.IsNullOrEmpty(hoverText))
		{
			IClickableMenu.drawHoverText(b, hoverText, Game1.smallFont);
		}
	}

	/// <summary>Get the tile coordinate for a player, with negative values snapped to zero.</summary>
	/// <param name="player">The player instance.</param>
	public Point GetNormalizedPlayerTile(Farmer player)
	{
		Point tile = player.TilePoint;
		if (tile.X < 0 || tile.Y < 0)
		{
			tile = new Point(Math.Max(0, tile.X), Math.Max(0, tile.Y));
		}
		return tile;
	}
}
