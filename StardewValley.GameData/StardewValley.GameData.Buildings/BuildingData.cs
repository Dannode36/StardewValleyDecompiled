using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Buildings;

/// <summary>The data for a building which can be constructed by players.</summary>
public class BuildingData
{
	/// <summary>A tokenizable string for the display name (e.g. shown in the construction menu).</summary>
	public string Name;

	/// <summary>A tokenizable string for the description (e.g. shown in the construction menu).</summary>
	public string Description;

	/// <summary>The asset name for the texture under the game's <c>Content</c> folder.</summary>
	public string Texture;

	/// <summary>The appearances which can be selected from the construction menu (like stone vs plank cabins), if any, in addition to the default appearance based on <see cref="F:StardewValley.GameData.Buildings.BuildingData.Texture" />.</summary>
	[ContentSerializer(Optional = true)]
	public List<BuildingSkin> Skins = new List<BuildingSkin>();

	/// <summary>Whether to draw an automatic shadow along the bottom edge of the building's sprite.</summary>
	[ContentSerializer(Optional = true)]
	public bool DrawShadow = true;

	/// <summary>The tile position relative to the top-left corner of the building where the upgrade sign will be placed when Robin is building an upgrade. Defaults to approximately (5, 1) if the building interior type is a shed, else (0, 0).</summary>
	[ContentSerializer(Optional = true)]
	public Vector2 UpgradeSignTile = new Vector2(-1f, -1f);

	/// <summary>The pixel height of the upgrade sign when Robin is building an upgrade.</summary>
	[ContentSerializer(Optional = true)]
	public float UpgradeSignHeight;

	/// <summary>The building's width and height when constructed, measured in tiles.</summary>
	[ContentSerializer(Optional = true)]
	public Point Size = new Point(1, 1);

	/// <summary>Whether the building should become semi-transparent when the player is behind it.</summary>
	[ContentSerializer(Optional = true)]
	public bool FadeWhenBehind = true;

	/// <summary>If set, the building's pixel area within the <see cref="F:StardewValley.GameData.Buildings.BuildingData.Texture" />. Defaults to the entire texture.</summary>
	[ContentSerializer(Optional = true)]
	public Rectangle SourceRect = Rectangle.Empty;

	/// <summary>A pixel offset to apply each season. This is applied to the <see cref="F:StardewValley.GameData.Buildings.BuildingData.SourceRect" /> position by multiplying the offset by 0 (spring), 1 (summer), 2 (fall), or 3 (winter). Default 0, so all seasons use the same source rect.</summary>
	[ContentSerializer(Optional = true)]
	public Point SeasonOffset = Point.Zero;

	/// <summary>A pixel offset applied to the building sprite's placement in the world.</summary>
	[ContentSerializer(Optional = true)]
	public Vector2 DrawOffset = Vector2.Zero;

	/// <summary>A Y tile offset applied when figuring out render layering. For example, a value of 2.5 will treat the building as if it was 2.5 tiles further up the screen for the purposes of layering.</summary>
	[ContentSerializer(Optional = true)]
	public float SortTileOffset;

	/// <summary>
	///   If set, an ASCII text block which indicates which of the building's tiles the players can walk onto, where each character can be <c>X</c> (blocked) or <c>O</c> (passable). Defaults to all tiles blocked. For example, a stable covers a 4x2 tile area with the front two tiles passable:
	///   <code>
	///     XXXX
	///     XOOX
	///   </code>
	/// </summary>
	[ContentSerializer(Optional = true)]
	public string CollisionMap;

	/// <summary>The extra tiles to treat as part of the building when placing it through a construction menu, if any. For example, the farmhouse uses this to make sure the stairs are clear.</summary>
	[ContentSerializer(Optional = true)]
	public List<BuildingPlacementTile> AdditionalPlacementTiles;

	/// <summary>If set, the full name of the C# type to instantiate for the building instance. Defaults to a generic <c>StardewValley.Building</c> instance. Note that using a non-vanilla building type will cause a crash when trying to write the building to the save file.</summary>
	[ContentSerializer(Optional = true)]
	public string BuildingType;

	/// <summary>The NPC from whom you can request construction. The vanilla values are <c>Robin</c> and <c>Wizard</c>, but you can specify a different name if a mod opens a construction menu for them. Defaults to <c>Robin</c>. If omitted, it won't appear in any menu.</summary>
	[ContentSerializer(Optional = true)]
	public string Builder = "Robin";

	/// <summary>If set, a game state query which indicates whether the building should be available in the construction menu. Defaults to always available.</summary>
	[ContentSerializer(Optional = true)]
	public string BuildCondition;

	/// <summary>The number of days needed to complete construction (e.g. 1 for a building completed the next day). If set to 0, construction finishes instantly.</summary>
	[ContentSerializer(Optional = true)]
	public int BuildDays;

	/// <summary>The gold cost to construct the building.</summary>
	[ContentSerializer(Optional = true)]
	public int BuildCost;

	/// <summary>The materials you must provide to start construction.</summary>
	[ContentSerializer(Optional = true)]
	public List<BuildingMaterial> BuildMaterials;

	/// <summary>The ID of the building for which this is an upgrade, or omit to allow constructing it as a new building. For example, the Big Coop sets this to "Coop". Any numbers of buildings can be an upgrade for the same building, in which case the player can choose one upgrade path.</summary>
	[ContentSerializer(Optional = true)]
	public string BuildingToUpgrade;

	/// <summary>Whether the building is magical. This changes the carpenter menu to a mystic theme while this building's blueprint is selected, and completes the construction instantly when placed.</summary>
	[ContentSerializer(Optional = true)]
	public bool MagicalConstruction;

	/// <summary>A pixel offset to apply to the building sprite when drawn in the construction menu.</summary>
	[ContentSerializer(Optional = true)]
	public Point BuildMenuDrawOffset = Point.Zero;

	/// <summary>The position of the door that can be clicked to warp into the building interior. This is measured in tiles relative to the top-left corner tile. Defaults to disabled.</summary>
	[ContentSerializer(Optional = true)]
	public Point HumanDoor = new Point(-1, -1);

	/// <summary>If set, the position and size of the door that animals use to enter/exit the building, if the building interior is an animal location. This is measured in tiles relative to the top-left corner tile. Defaults to disabled.</summary>
	[ContentSerializer(Optional = true)]
	public Rectangle AnimalDoor = new Rectangle(-1, -1, 0, 0);

	/// <summary>The duration of the open animation for the <see cref="F:StardewValley.GameData.Buildings.BuildingData.AnimalDoor" />, measured in milliseconds. If omitted, the door switches to the open state instantly.</summary>
	[ContentSerializer(Optional = true)]
	public float AnimalDoorOpenDuration;

	/// <summary>If set, the sound which is played once each time the animal door is opened. Disabled by default.</summary>
	[ContentSerializer(Optional = true)]
	public string AnimalDoorOpenSound;

	/// <summary>The duration of the close animation for the <see cref="F:StardewValley.GameData.Buildings.BuildingData.AnimalDoor" />, measured in milliseconds. If omitted, the door switches to the closed state instantly.</summary>
	[ContentSerializer(Optional = true)]
	public float AnimalDoorCloseDuration;

	/// <summary>If set, the sound which is played once each time the animal door is closed. Disabled by default.</summary>
	[ContentSerializer(Optional = true)]
	public string AnimalDoorCloseSound;

	/// <summary>If set, the name of the existing global location to treat as the building's interior, like <c>FarmHouse</c> and <c>Greenhouse</c> for their respective buildings. If omitted, each building will have its own location instance.</summary>
	/// <remarks>
	///   <para>Each location can only be used by one building. If the location is already in use (e.g. because the player has two of this building), each subsequent building will use the <see cref="F:StardewValley.GameData.Buildings.BuildingData.IndoorMap" /> and <see cref="F:StardewValley.GameData.Buildings.BuildingData.IndoorMapType" /> instead. For example, the first greenhouse will use the global <c>Greenhouse</c> location, and any subsequent greenhouse will use a separate instanced location.</para>
	///
	///   <para>The non-instanced location must already be in <c>Game1.locations</c>.</para>
	/// </remarks>
	[ContentSerializer(Optional = true)]
	public string NonInstancedIndoorLocation;

	/// <summary>The name of the map asset under <c>Content/Maps</c> to load for the building interior (like <c>"Shed"</c> for the <c>Content/Maps/Shed</c> map).</summary>
	[ContentSerializer(Optional = true)]
	public string IndoorMap;

	/// <summary>If set, the full name of the C# <c>GameLocation</c> subclass which will manage the building's interior location. This must be one of the vanilla types to avoid a crash when saving. Defaults to a generic <c>GameLocation</c>.</summary>
	[ContentSerializer(Optional = true)]
	public string IndoorMapType;

	/// <summary>The maximum number of animals who can live in this building.</summary>
	[ContentSerializer(Optional = true)]
	public int MaxOccupants = 20;

	/// <summary>A list of building IDs whose animals to allow in this building too. For example, <c>[ "Barn", "Coop" ]</c> will allow barn and coop animals in this building. Default none.</summary>
	[ContentSerializer(Optional = true)]
	public List<string> ValidOccupantTypes = new List<string>();

	/// <summary>Whether animals can get pregnant and produce offspring in this building.</summary>
	[ContentSerializer(Optional = true)]
	public bool AllowAnimalPregnancy;

	/// <summary>When applied as an upgrade to an existing building, the placed items in its interior to move when transitioning to the new map.</summary>
	[ContentSerializer(Optional = true)]
	public List<IndoorItemMove> IndoorItemMoves;

	/// <summary>The items to place in the building interior when it's constructed or upgraded.</summary>
	[ContentSerializer(Optional = true)]
	public List<IndoorItemAdd> IndoorItems;

	/// <summary>A list of mail IDs to send to all players when the building is constructed for the first time.</summary>
	[ContentSerializer(Optional = true)]
	public List<string> AddMailOnBuild;

	/// <summary>A list of custom properties applied to the building, which can optionally be overridden per-skin in the <see cref="F:StardewValley.GameData.Buildings.BuildingData.Skins" /> field.</summary>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> Metadata = new Dictionary<string, string>();

	/// <summary>A lookup of arbitrary <c>modData</c> values to attach to the building when it's constructed.</summary>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> ModData = new Dictionary<string, string>();

	/// <summary>The amount of hay that can be stored in this building. If built on the farm, this works just like silos and contributes to the farm's available hay.</summary>
	[ContentSerializer(Optional = true)]
	public int HayCapacity;

	/// <summary>The input/output inventories that can be accessed from a tile on the building exterior. The allowed items are defined by the <see cref="F:StardewValley.GameData.Buildings.BuildingData.ItemConversions" /> field.</summary>
	[ContentSerializer(Optional = true)]
	public List<BuildingChest> Chests;

	/// <summary>The default tile action if the clicked tile isn't in <see cref="F:StardewValley.GameData.Buildings.BuildingData.ActionTiles" />.</summary>
	[ContentSerializer(Optional = true)]
	public string DefaultAction;

	/// <summary>The number of extra tiles around the building for which it may add tile properties via <see cref="F:StardewValley.GameData.Buildings.BuildingData.TileProperties" />, but without hiding tile properties from the underlying ground that aren't overwritten by the building data.</summary>
	[ContentSerializer(Optional = true)]
	public int AdditionalTilePropertyRadius;

	/// <summary>If true, terrain feature flooring can be placed underneath, and when the building is placed, it will not destroy flooring beneath it.</summary>
	[ContentSerializer(Optional = true)]
	public bool AllowsFlooringUnderneath = true;

	/// <summary>A list of tiles which the player can click to trigger an <c>Action</c> map tile property.</summary>
	[ContentSerializer(Optional = true)]
	public List<BuildingActionTile> ActionTiles = new List<BuildingActionTile>();

	/// <summary>The map tile properties to set.</summary>
	[ContentSerializer(Optional = true)]
	public List<BuildingTileProperty> TileProperties = new List<BuildingTileProperty>();

	/// <summary>The output items produced when an input item is converted.</summary>
	[ContentSerializer(Optional = true)]
	public List<BuildingItemConversion> ItemConversions;

	/// <summary>A list of textures to draw over or behind the building, with support for conditions and animations.</summary>
	[ContentSerializer(Optional = true)]
	public List<BuildingDrawLayer> DrawLayers;

	/// <summary>Custom fields ignored by the base game, for use by mods.</summary>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> CustomFields;

	/// <summary>A cached representation of <see cref="F:StardewValley.GameData.Buildings.BuildingData.ActionTiles" />.</summary>
	protected Dictionary<Point, string> _actionTiles;

	/// <summary>A cached representation of <see cref="F:StardewValley.GameData.Buildings.BuildingData.CollisionMap" />.</summary>
	protected Dictionary<Point, bool> _collisionMap;

	/// <summary>A cached representation of <see cref="F:StardewValley.GameData.Buildings.BuildingData.TileProperties" />.</summary>
	protected Dictionary<string, Dictionary<Point, Dictionary<string, string>>> _tileProperties;

	/// <summary>Get whether a tile is passable based on the <see cref="F:StardewValley.GameData.Buildings.BuildingData.CollisionMap" />.</summary>
	/// <param name="relativeX">The tile X position relative to the top-left corner of the building.</param>
	/// <param name="relativeY">The tile Y position relative to the top-left corner of the building.</param>
	public bool IsTilePassable(int relativeX, int relativeY)
	{
		if (CollisionMap == null)
		{
			if (relativeX >= 0 && relativeX < Size.X && relativeY >= 0 && relativeY < Size.Y)
			{
				return false;
			}
			return true;
		}
		Point tile = new Point(relativeX, relativeY);
		if (_collisionMap == null)
		{
			_collisionMap = new Dictionary<Point, bool>();
			if (CollisionMap != null)
			{
				string[] collisionLines = CollisionMap.Trim().Split('\n');
				for (int y = 0; y < collisionLines.Length; y++)
				{
					string collisionLine = collisionLines[y].Trim();
					for (int x = 0; x < collisionLine.Length; x++)
					{
						_collisionMap[new Point(x, y)] = collisionLine[x] == 'X';
					}
				}
			}
		}
		if (_collisionMap.TryGetValue(tile, out var collision))
		{
			return !collision;
		}
		return true;
	}

	/// <summary>Get the action to add at a given position based on <see cref="F:StardewValley.GameData.Buildings.BuildingData.ActionTiles" />.</summary>
	/// <param name="relativeX">The tile X position relative to the top-left corner of the building.</param>
	/// <param name="relativeY">The tile Y position relative to the top-left corner of the building.</param>
	public string GetActionAtTile(int relativeX, int relativeY)
	{
		Point tilePoint = new Point(relativeX, relativeY);
		if (_actionTiles == null)
		{
			_actionTiles = new Dictionary<Point, string>();
			foreach (BuildingActionTile buildingAction in ActionTiles)
			{
				_actionTiles[buildingAction.Tile] = buildingAction.Action;
			}
		}
		if (!_actionTiles.TryGetValue(tilePoint, out var action))
		{
			if (relativeX < 0 || relativeX >= Size.X || relativeY < 0 || relativeY >= Size.Y)
			{
				return null;
			}
			return DefaultAction;
		}
		return action;
	}

	/// <summary>Get whether a tile property should be added based on <see cref="F:StardewValley.GameData.Buildings.BuildingData.TileProperties" />.</summary>
	/// <param name="relativeX">The tile X position relative to the top-left corner of the building's bounding box.</param>
	/// <param name="relativeY">The tile Y position relative to the top-left corner of the building's bounding box.</param>
	/// <param name="propertyName">The property name to check.</param>
	/// <param name="layerName">The layer name to check.</param>
	/// <param name="propertyValue">The property value that should be set.</param>
	public bool HasPropertyAtTile(int relativeX, int relativeY, string propertyName, string layerName, ref string propertyValue)
	{
		if (_tileProperties == null)
		{
			_tileProperties = new Dictionary<string, Dictionary<Point, Dictionary<string, string>>>();
			foreach (BuildingTileProperty property in TileProperties)
			{
				if (!_tileProperties.TryGetValue(property.Layer, out var propsByTile))
				{
					propsByTile = (_tileProperties[property.Layer] = new Dictionary<Point, Dictionary<string, string>>());
				}
				for (int y = property.TileArea.Y; y < property.TileArea.Bottom; y++)
				{
					for (int x = property.TileArea.X; x < property.TileArea.Right; x++)
					{
						Point tile = new Point(x, y);
						if (!propsByTile.TryGetValue(tile, out var tileProps))
						{
							tileProps = (propsByTile[tile] = new Dictionary<string, string>());
						}
						tileProps[property.Name] = property.Value;
					}
				}
			}
		}
		if (_tileProperties.TryGetValue(layerName, out var propertiesByTile) && propertiesByTile.TryGetValue(new Point(relativeX, relativeY), out var tileProperties) && tileProperties.TryGetValue(propertyName, out var value))
		{
			propertyValue = value;
			return true;
		}
		return false;
	}
}
