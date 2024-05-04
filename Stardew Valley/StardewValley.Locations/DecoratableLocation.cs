using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;

namespace StardewValley.Locations;

public class DecoratableLocation : GameLocation
{
	/// <summary>Obsolete.</summary>
	public readonly DecorationFacade wallPaper = new DecorationFacade();

	[XmlIgnore]
	public readonly NetStringList wallpaperIDs = new NetStringList();

	public readonly NetStringDictionary<string, NetString> appliedWallpaper = new NetStringDictionary<string, NetString>
	{
		InterpolationWait = false
	};

	[XmlIgnore]
	public readonly Dictionary<string, List<Vector3>> wallpaperTiles = new Dictionary<string, List<Vector3>>();

	/// <summary>Obsolete.</summary>
	public readonly DecorationFacade floor = new DecorationFacade();

	[XmlIgnore]
	public readonly NetStringList floorIDs = new NetStringList();

	public readonly NetStringDictionary<string, NetString> appliedFloor = new NetStringDictionary<string, NetString>
	{
		InterpolationWait = false
	};

	[XmlIgnore]
	public readonly Dictionary<string, List<Vector3>> floorTiles = new Dictionary<string, List<Vector3>>();

	protected Dictionary<string, TileSheet> _wallAndFloorTileSheets = new Dictionary<string, TileSheet>();

	protected Map _wallAndFloorTileSheetMap;

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(appliedWallpaper, "appliedWallpaper").AddField(appliedFloor, "appliedFloor").AddField(floorIDs, "floorIDs")
			.AddField(wallpaperIDs, "wallpaperIDs");
		appliedWallpaper.OnValueAdded += delegate(string key, string value)
		{
			UpdateWallpaper(key);
		};
		appliedWallpaper.OnConflictResolve += delegate(string key, NetString rejected, NetString accepted)
		{
			UpdateWallpaper(key);
		};
		appliedWallpaper.OnValueTargetUpdated += delegate(string key, string old_value, string new_value)
		{
			if (appliedWallpaper.FieldDict.TryGetValue(key, out var value3))
			{
				value3.CancelInterpolation();
			}
			UpdateWallpaper(key);
		};
		appliedFloor.OnValueAdded += delegate(string key, string value)
		{
			UpdateFloor(key);
		};
		appliedFloor.OnConflictResolve += delegate(string key, NetString rejected, NetString accepted)
		{
			UpdateFloor(key);
		};
		appliedFloor.OnValueTargetUpdated += delegate(string key, string old_value, string new_value)
		{
			if (appliedFloor.FieldDict.TryGetValue(key, out var value2))
			{
				value2.CancelInterpolation();
			}
			UpdateFloor(key);
		};
	}

	public DecoratableLocation()
	{
	}

	public DecoratableLocation(string mapPath, string name)
		: base(mapPath, name)
	{
	}

	public override void updateLayout()
	{
		base.updateLayout();
		if (Game1.IsMasterGame)
		{
			setWallpapers();
			setFloors();
		}
	}

	public virtual void ReadWallpaperAndFloorTileData()
	{
		updateMap();
		wallpaperTiles.Clear();
		floorTiles.Clear();
		wallpaperIDs.Clear();
		floorIDs.Clear();
		Dictionary<string, string> initial_values = new Dictionary<string, string>();
		if (TryGetMapProperty("WallIDs", out var wallProperty))
		{
			string[] array = wallProperty.Split(',');
			for (int j = 0; j < array.Length; j++)
			{
				string[] data_split = ArgUtility.SplitBySpace(array[j]);
				if (data_split.Length >= 1)
				{
					wallpaperIDs.Add(data_split[0]);
				}
				if (data_split.Length >= 2)
				{
					initial_values[data_split[0]] = data_split[1];
				}
			}
		}
		if (wallpaperIDs.Count == 0)
		{
			List<Microsoft.Xna.Framework.Rectangle> walls = getWalls();
			for (int i = 0; i < walls.Count; i++)
			{
				string id = "Wall_" + i;
				wallpaperIDs.Add(id);
				Microsoft.Xna.Framework.Rectangle rect = walls[i];
				if (!wallpaperTiles.ContainsKey(i.ToString()))
				{
					wallpaperTiles[id] = new List<Vector3>();
				}
				foreach (Point tile in rect.GetPoints())
				{
					wallpaperTiles[id].Add(new Vector3(tile.X, tile.Y, tile.Y - rect.Top));
				}
			}
		}
		else
		{
			for (int x = 0; x < map.Layers[0].LayerWidth; x++)
			{
				for (int y = 0; y < map.Layers[0].LayerHeight; y++)
				{
					string tile_property = doesTileHaveProperty(x, y, "WallID", "Back");
					getTileIndexAt(new Point(x, y), "Back");
					if (tile_property == null)
					{
						continue;
					}
					if (!wallpaperIDs.Contains(tile_property))
					{
						wallpaperIDs.Add(tile_property);
					}
					if (appliedWallpaper.TryAdd(tile_property, "0") && initial_values.TryGetValue(tile_property, out var initial_value))
					{
						if (appliedWallpaper.TryGetValue(initial_value, out var newValue))
						{
							appliedWallpaper[tile_property] = newValue;
						}
						else if (GetWallpaperSource(initial_value).Value >= 0)
						{
							appliedWallpaper[tile_property] = initial_value;
						}
					}
					if (!wallpaperTiles.TryGetValue(tile_property, out var areas))
					{
						areas = (wallpaperTiles[tile_property] = new List<Vector3>());
					}
					areas.Add(new Vector3(x, y, 0f));
					string tilesheet_id = getTileSheetIDAt(x, y, "Back");
					map.GetTileSheet(tilesheet_id);
					if (IsFloorableOrWallpaperableTile(x, y + 1, "Back"))
					{
						areas.Add(new Vector3(x, y + 1, 1f));
					}
					if (IsFloorableOrWallpaperableTile(x, y + 2, "Buildings"))
					{
						areas.Add(new Vector3(x, y + 2, 2f));
					}
					else if (IsFloorableOrWallpaperableTile(x, y + 2, "Back") && !IsFloorableTile(x, y + 2, "Back"))
					{
						areas.Add(new Vector3(x, y + 2, 2f));
					}
				}
			}
		}
		initial_values.Clear();
		if (TryGetMapProperty("FloorIDs", out var floorProperty))
		{
			string[] array = floorProperty.Split(',');
			for (int j = 0; j < array.Length; j++)
			{
				string[] data_split = ArgUtility.SplitBySpace(array[j]);
				if (data_split.Length >= 1)
				{
					floorIDs.Add(data_split[0]);
				}
				if (data_split.Length >= 2)
				{
					initial_values[data_split[0]] = data_split[1];
				}
			}
		}
		if (floorIDs.Count == 0)
		{
			List<Microsoft.Xna.Framework.Rectangle> floors = getFloors();
			for (int i = 0; i < floors.Count; i++)
			{
				string id = "Floor_" + i;
				floorIDs.Add(id);
				Microsoft.Xna.Framework.Rectangle rect2 = floors[i];
				if (!floorTiles.ContainsKey(i.ToString()))
				{
					floorTiles[id] = new List<Vector3>();
				}
				foreach (Point tile in rect2.GetPoints())
				{
					floorTiles[id].Add(new Vector3(tile.X, tile.Y, 0f));
				}
			}
		}
		else
		{
			for (int x = 0; x < map.Layers[0].LayerWidth; x++)
			{
				for (int y = 0; y < map.Layers[0].LayerHeight; y++)
				{
					string tile_property = doesTileHaveProperty(x, y, "FloorID", "Back");
					if (tile_property == null)
					{
						continue;
					}
					if (!floorIDs.Contains(tile_property))
					{
						floorIDs.Add(tile_property);
					}
					if (appliedFloor.TryAdd(tile_property, "0") && initial_values.TryGetValue(tile_property, out var initial_value))
					{
						if (appliedFloor.TryGetValue(initial_value, out var newValue))
						{
							appliedFloor[tile_property] = newValue;
						}
						else if (GetFloorSource(initial_value).Value >= 0)
						{
							appliedFloor[tile_property] = initial_value;
						}
					}
					if (!floorTiles.TryGetValue(tile_property, out var areas))
					{
						areas = (floorTiles[tile_property] = new List<Vector3>());
					}
					areas.Add(new Vector3(x, y, 0f));
				}
			}
		}
		setFloors();
		setWallpapers();
	}

	public virtual TileSheet GetWallAndFloorTilesheet(string id)
	{
		if (map != _wallAndFloorTileSheetMap)
		{
			_wallAndFloorTileSheets.Clear();
			_wallAndFloorTileSheetMap = map;
		}
		if (_wallAndFloorTileSheets.TryGetValue(id, out var wallAndFloorTilesheet))
		{
			return wallAndFloorTilesheet;
		}
		try
		{
			foreach (ModWallpaperOrFlooring entry in DataLoader.AdditionalWallpaperFlooring(Game1.content))
			{
				if (!(entry.Id != id))
				{
					Texture2D texture = Game1.content.Load<Texture2D>(entry.Texture);
					if (texture.Width != 256)
					{
						Game1.log.Warn($"The tilesheet for wallpaper/floor '{entry.Id}' is {texture.Width} pixels wide, but it must be exactly {256} pixels wide.");
					}
					TileSheet tilesheet = new TileSheet("x_WallsAndFloors_" + id, map, entry.Texture, new Size(texture.Width / 16, texture.Height / 16), new Size(16, 16));
					map.AddTileSheet(tilesheet);
					map.LoadTileSheets(Game1.mapDisplayDevice);
					_wallAndFloorTileSheets[id] = tilesheet;
					return tilesheet;
				}
			}
			Game1.log.Error("The tilesheet for wallpaper/floor '" + id + "' could not be loaded: no such ID found in Data/AdditionalWallpaperFlooring.");
			_wallAndFloorTileSheets[id] = null;
			return null;
		}
		catch (Exception ex)
		{
			Game1.log.Error("The tilesheet for wallpaper/floor '" + id + "' could not be loaded.", ex);
			_wallAndFloorTileSheets[id] = null;
			return null;
		}
	}

	public virtual KeyValuePair<int, int> GetFloorSource(string pattern_id)
	{
		int pattern_index;
		if (pattern_id.Contains(':'))
		{
			string[] pattern_split = pattern_id.Split(':');
			TileSheet tilesheet = GetWallAndFloorTilesheet(pattern_split[0]);
			if (int.TryParse(pattern_split[1], out pattern_index) && tilesheet != null)
			{
				return new KeyValuePair<int, int>(map.TileSheets.IndexOf(tilesheet), pattern_index);
			}
		}
		if (int.TryParse(pattern_id, out pattern_index))
		{
			TileSheet tilesheet = map.GetTileSheet("walls_and_floors");
			return new KeyValuePair<int, int>(map.TileSheets.IndexOf(tilesheet), pattern_index);
		}
		return new KeyValuePair<int, int>(-1, -1);
	}

	public virtual KeyValuePair<int, int> GetWallpaperSource(string pattern_id)
	{
		int pattern_index;
		if (pattern_id.Contains(':'))
		{
			string[] pattern_split = pattern_id.Split(':');
			TileSheet tilesheet = GetWallAndFloorTilesheet(pattern_split[0]);
			if (int.TryParse(pattern_split[1], out pattern_index) && tilesheet != null)
			{
				return new KeyValuePair<int, int>(map.TileSheets.IndexOf(tilesheet), pattern_index);
			}
		}
		if (int.TryParse(pattern_id, out pattern_index))
		{
			TileSheet tilesheet = map.GetTileSheet("walls_and_floors");
			return new KeyValuePair<int, int>(map.TileSheets.IndexOf(tilesheet), pattern_index);
		}
		return new KeyValuePair<int, int>(-1, -1);
	}

	public virtual void UpdateFloor(string floor_id)
	{
		updateMap();
		if (!appliedFloor.TryGetValue(floor_id, out var pattern_id) || !floorTiles.TryGetValue(floor_id, out var tiles))
		{
			return;
		}
		foreach (Vector3 item in tiles)
		{
			int x = (int)item.X;
			int y = (int)item.Y;
			KeyValuePair<int, int> source = GetFloorSource(pattern_id);
			if (source.Value < 0)
			{
				continue;
			}
			int tilesheet_index = source.Key;
			int floor_pattern_id = source.Value;
			int tiles_wide = map.TileSheets[tilesheet_index].SheetWidth;
			string id = map.TileSheets[tilesheet_index].Id;
			string layer = "Back";
			floor_pattern_id = floor_pattern_id * 2 + floor_pattern_id / (tiles_wide / 2) * tiles_wide;
			if (id == "walls_and_floors")
			{
				floor_pattern_id += GetFirstFlooringTile();
			}
			if (!IsFloorableOrWallpaperableTile(x, y, layer))
			{
				continue;
			}
			Tile old_tile = map.RequireLayer(layer).Tiles[x, y];
			setMapTile(x, y, GetFlooringIndex(floor_pattern_id, x, y), layer, null, tilesheet_index);
			Tile new_tile = map.RequireLayer(layer).Tiles[x, y];
			if (old_tile == null)
			{
				continue;
			}
			foreach (KeyValuePair<string, PropertyValue> property in old_tile.Properties)
			{
				new_tile.Properties[property.Key] = property.Value;
			}
		}
	}

	public virtual void UpdateWallpaper(string wallpaper_id)
	{
		updateMap();
		if (!appliedWallpaper.ContainsKey(wallpaper_id) || !wallpaperTiles.ContainsKey(wallpaper_id))
		{
			return;
		}
		string pattern_id = appliedWallpaper[wallpaper_id];
		foreach (Vector3 item in wallpaperTiles[wallpaper_id])
		{
			int x = (int)item.X;
			int y = (int)item.Y;
			int type = (int)item.Z;
			KeyValuePair<int, int> source = GetWallpaperSource(pattern_id);
			if (source.Value < 0)
			{
				continue;
			}
			int tile_sheet_index = source.Key;
			int tile_id = source.Value;
			int tiles_wide = map.TileSheets[tile_sheet_index].SheetWidth;
			string layer = "Back";
			if (type == 2)
			{
				layer = "Buildings";
				if (!IsFloorableOrWallpaperableTile(x, y, "Buildings"))
				{
					layer = "Back";
				}
			}
			if (!IsFloorableOrWallpaperableTile(x, y, layer))
			{
				continue;
			}
			Tile old_tile = map.RequireLayer(layer).Tiles[x, y];
			setMapTile(x, y, tile_id / tiles_wide * tiles_wide * 3 + tile_id % tiles_wide + type * tiles_wide, layer, null, tile_sheet_index);
			Tile new_tile = map.RequireLayer(layer).Tiles[x, y];
			if (old_tile == null)
			{
				continue;
			}
			foreach (KeyValuePair<string, PropertyValue> property in old_tile.Properties)
			{
				new_tile.Properties[property.Key] = property.Value;
			}
		}
	}

	public override void UpdateWhenCurrentLocation(GameTime time)
	{
		if (!wasUpdated)
		{
			base.UpdateWhenCurrentLocation(time);
		}
	}

	public override void MakeMapModifications(bool force = false)
	{
		base.MakeMapModifications(force);
		if (!(this is FarmHouse))
		{
			ReadWallpaperAndFloorTileData();
			setWallpapers();
			setFloors();
		}
		if (getTileIndexAt(Game1.player.TilePoint.X, Game1.player.TilePoint.Y, "Buildings") != -1)
		{
			Game1.player.position.Y += 64f;
		}
	}

	protected override void resetLocalState()
	{
		base.resetLocalState();
		if (Game1.player.mailReceived.Add("button_tut_1"))
		{
			Game1.onScreenMenus.Add(new ButtonTutorialMenu(0));
		}
	}

	public override bool CanFreePlaceFurniture()
	{
		return true;
	}

	public virtual bool isTileOnWall(int x, int y)
	{
		foreach (string id in wallpaperTiles.Keys)
		{
			foreach (Vector3 tile_data in wallpaperTiles[id])
			{
				if ((int)tile_data.X == x && (int)tile_data.Y == y)
				{
					return true;
				}
			}
		}
		return false;
	}

	public int GetWallTopY(int x, int y)
	{
		foreach (string id in wallpaperTiles.Keys)
		{
			foreach (Vector3 tile_data in wallpaperTiles[id])
			{
				if ((int)tile_data.X == x && (int)tile_data.Y == y)
				{
					return y - (int)tile_data.Z;
				}
			}
		}
		return -1;
	}

	public virtual void setFloors()
	{
		foreach (KeyValuePair<string, string> pair in appliedFloor.Pairs)
		{
			UpdateFloor(pair.Key);
		}
	}

	public virtual void setWallpapers()
	{
		foreach (KeyValuePair<string, string> pair in appliedWallpaper.Pairs)
		{
			UpdateWallpaper(pair.Key);
		}
	}

	public void SetFloor(string which, string which_room)
	{
		if (which_room == null)
		{
			foreach (string key in floorIDs)
			{
				appliedFloor[key] = which;
			}
			return;
		}
		appliedFloor[which_room] = which;
	}

	public void SetWallpaper(string which, string which_room)
	{
		if (which_room == null)
		{
			foreach (string key in wallpaperIDs)
			{
				appliedWallpaper[key] = which;
			}
			return;
		}
		appliedWallpaper[which_room] = which;
	}

	public void OverrideSpecificWallpaper(string which, string which_room, string wallpaperStyleToOverride)
	{
		if (which_room == null)
		{
			foreach (string key in wallpaperIDs)
			{
				if (appliedWallpaper.ContainsKey(key) && appliedWallpaper[key] == wallpaperStyleToOverride)
				{
					appliedWallpaper[key] = which;
				}
			}
			return;
		}
		if (appliedWallpaper[which_room] == wallpaperStyleToOverride)
		{
			appliedWallpaper[which_room] = which;
		}
	}

	public void OverrideSpecificFlooring(string which, string which_room, string flooringStyleToOverride)
	{
		if (which_room == null)
		{
			foreach (string key in floorIDs)
			{
				if (appliedFloor.ContainsKey(key) && appliedFloor[key] == flooringStyleToOverride)
				{
					appliedFloor[key] = which;
				}
			}
			return;
		}
		if (appliedFloor[which_room] == flooringStyleToOverride)
		{
			appliedFloor[which_room] = which;
		}
	}

	public string GetFloorID(int x, int y)
	{
		foreach (string id in floorTiles.Keys)
		{
			foreach (Vector3 tile_data in floorTiles[id])
			{
				if ((int)tile_data.X == x && (int)tile_data.Y == y)
				{
					return id;
				}
			}
		}
		return null;
	}

	public string GetWallpaperID(int x, int y)
	{
		foreach (string id in wallpaperTiles.Keys)
		{
			foreach (Vector3 tile_data in wallpaperTiles[id])
			{
				if ((int)tile_data.X == x && (int)tile_data.Y == y)
				{
					return id;
				}
			}
		}
		return null;
	}

	protected bool IsFloorableTile(int x, int y, string layer_name)
	{
		int tile_index = getTileIndexAt(x, y, "Buildings");
		if (tile_index >= 197 && tile_index <= 199 && getTileSheetIDAt(x, y, "Buildings") == "untitled tile sheet")
		{
			return false;
		}
		return IsFloorableOrWallpaperableTile(x, y, layer_name);
	}

	public bool IsWallAndFloorTilesheet(string tilesheet_id)
	{
		if (tilesheet_id.StartsWith("x_WallsAndFloors_"))
		{
			return true;
		}
		return tilesheet_id == "walls_and_floors";
	}

	protected bool IsFloorableOrWallpaperableTile(int x, int y, string layer_name)
	{
		Layer layer = map.GetLayer(layer_name);
		if (layer != null && x < layer.LayerWidth && y < layer.LayerHeight && layer.Tiles[x, y] != null && layer.Tiles[x, y].TileSheet != null && IsWallAndFloorTilesheet(layer.Tiles[x, y].TileSheet.Id))
		{
			return true;
		}
		return false;
	}

	public override void TransferDataFromSavedLocation(GameLocation l)
	{
		if (l is DecoratableLocation decoratable_location)
		{
			if (!decoratable_location.appliedWallpaper.Keys.Any() && !decoratable_location.appliedFloor.Keys.Any())
			{
				ReadWallpaperAndFloorTileData();
				for (int i = 0; i < decoratable_location.wallPaper.Count; i++)
				{
					try
					{
						string key = wallpaperIDs[i];
						string value = decoratable_location.wallPaper[i].ToString();
						appliedWallpaper[key] = value;
					}
					catch (Exception)
					{
					}
				}
				for (int i = 0; i < decoratable_location.floor.Count; i++)
				{
					try
					{
						string key = floorIDs[i];
						string value = decoratable_location.floor[i].ToString();
						appliedFloor[key] = value;
					}
					catch (Exception)
					{
					}
				}
			}
			else
			{
				foreach (string key in decoratable_location.appliedWallpaper.Keys)
				{
					appliedWallpaper[key] = decoratable_location.appliedWallpaper[key];
				}
				foreach (string key in decoratable_location.appliedFloor.Keys)
				{
					appliedFloor[key] = decoratable_location.appliedFloor[key];
				}
			}
		}
		setWallpapers();
		setFloors();
		base.TransferDataFromSavedLocation(l);
	}

	public Furniture getRandomFurniture(Random r)
	{
		return r.ChooseFrom(furniture);
	}

	public virtual string getFloorRoomIdAt(Point p)
	{
		foreach (string key in floorTiles.Keys)
		{
			foreach (Vector3 tile_data in floorTiles[key])
			{
				if ((int)tile_data.X == p.X && (int)tile_data.Y == p.Y)
				{
					return key;
				}
			}
		}
		return null;
	}

	public virtual int GetFirstFlooringTile()
	{
		return 336;
	}

	public virtual int GetFlooringIndex(int base_tile_sheet, int tile_x, int tile_y)
	{
		int replaced_tile_index = getTileIndexAt(tile_x, tile_y, "Back");
		if (replaced_tile_index < 0)
		{
			return 0;
		}
		string tilesheet_name = getTileSheetIDAt(tile_x, tile_y, "Back");
		TileSheet tilesheet = map.GetTileSheet(tilesheet_name);
		int tiles_wide = 16;
		if (tilesheet != null)
		{
			tiles_wide = tilesheet.SheetWidth;
		}
		if (tilesheet_name == "walls_and_floors")
		{
			replaced_tile_index -= GetFirstFlooringTile();
		}
		int x_offset = tile_x % 2;
		int y_offset = tile_y % 2;
		return base_tile_sheet + x_offset + tiles_wide * y_offset;
	}

	public virtual List<Microsoft.Xna.Framework.Rectangle> getFloors()
	{
		return new List<Microsoft.Xna.Framework.Rectangle>();
	}
}
