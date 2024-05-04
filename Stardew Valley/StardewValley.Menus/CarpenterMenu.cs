using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.GameData.Buildings;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.TokenizableStrings;
using xTile.Dimensions;
using xTile.Layers;

namespace StardewValley.Menus;

public class CarpenterMenu : IClickableMenu
{
	/// <summary>Metadata for a building shown in the construction menu.</summary>
	public class BlueprintEntry
	{
		/// <summary>The index of the blueprint in the construction menu's list.</summary>
		public int Index { get; }

		/// <summary>The building type ID in <c>Data/Buildings</c>.</summary>
		public string Id { get; }

		/// <summary>The building data from <c>Data/Buildings</c>.</summary>
		public BuildingData Data { get; }

		/// <summary>The building skin to apply from <c>Data/Buildings</c>, if applicable.</summary>
		public BuildingSkin Skin { get; private set; }

		/// <summary>The translated display name.</summary>
		public string DisplayName { get; private set; }

		/// <summary>The translated display name as a <see cref="T:StardewValley.TokenizableStrings.TokenParser">tokenizable string</see>.</summary>
		public string TokenizedDisplayName { get; private set; }

		/// <summary>The translated description.</summary>
		public string Description { get; private set; }

		/// <summary>The number of tiles horizontally for the constructed building's collision box.</summary>
		public int TilesWide { get; }

		/// <summary>The number of tiles vertically for the constructed building's collision box.</summary>
		public int TilesHigh { get; }

		/// <inheritdoc cref="F:StardewValley.GameData.Buildings.BuildingData.BuildingToUpgrade" />
		public bool IsUpgrade
		{
			get
			{
				string buildingToUpgrade = Data.BuildingToUpgrade;
				if (buildingToUpgrade == null)
				{
					return false;
				}
				return buildingToUpgrade.Length > 0;
			}
		}

		/// <inheritdoc cref="F:StardewValley.GameData.Buildings.BuildingData.BuildDays" />
		public int BuildDays => Skin?.BuildDays ?? Data.BuildDays;

		/// <inheritdoc cref="F:StardewValley.GameData.Buildings.BuildingData.BuildCost" />
		public int BuildCost => Skin?.BuildCost ?? Data.BuildCost;

		/// <inheritdoc cref="F:StardewValley.GameData.Buildings.BuildingData.BuildMaterials" />
		public List<BuildingMaterial> BuildMaterials => Skin?.BuildMaterials ?? Data.BuildMaterials;

		/// <inheritdoc cref="F:StardewValley.GameData.Buildings.BuildingData.BuildingToUpgrade" />
		public string UpgradeFrom => Data.BuildingToUpgrade;

		/// <inheritdoc cref="F:StardewValley.GameData.Buildings.BuildingData.MagicalConstruction" />
		public bool MagicalConstruction => Data.MagicalConstruction;

		/// <summary>Construct an instance.</summary>
		/// <param name="index">The index of the blueprint in the construction menu's list.</param>
		/// <param name="id">The building type ID in <c>Data/Buildings</c>.</param>
		/// <param name="data">The building data from <c>Data/Buildings</c>.</param>
		/// <param name="skinId">The building skin ID, if applicable.</param>
		public BlueprintEntry(int index, string id, BuildingData data, string skinId)
		{
			Index = index;
			Id = id;
			Data = data;
			TilesWide = data.Size.X;
			TilesHigh = data.Size.Y;
			SetSkin(skinId);
		}

		/// <summary>Set the selected building skin.</summary>
		/// <param name="id">The skin ID.</param>
		public void SetSkin(string id)
		{
			if (Data.Skins != null)
			{
				foreach (BuildingSkin skin in Data.Skins)
				{
					if (skin.Id == id)
					{
						Skin = skin;
						TokenizedDisplayName = skin.Name ?? Data.Name;
						DisplayName = TokenParser.ParseText(TokenizedDisplayName);
						Description = TokenParser.ParseText(skin.Description) ?? TokenParser.ParseText(Data.Description);
						return;
					}
				}
			}
			Skin = null;
			TokenizedDisplayName = Data.Name;
			DisplayName = TokenParser.ParseText(Data.Name);
			Description = TokenParser.ParseText(Data.Description);
		}

		/// <summary>Get the display name for the building this upgrades from, if applicable.</summary>
		public string GetDisplayNameForBuildingToUpgrade()
		{
			if (!IsUpgrade || !Game1.buildingData.TryGetValue(Data.BuildingToUpgrade, out var otherData))
			{
				return null;
			}
			return TokenParser.ParseText(otherData.Name);
		}
	}

	public const int region_backButton = 101;

	public const int region_forwardButton = 102;

	public const int region_upgradeIcon = 103;

	public const int region_demolishButton = 104;

	public const int region_moveBuitton = 105;

	public const int region_okButton = 106;

	public const int region_cancelButton = 107;

	public const int region_paintButton = 108;

	public const int region_appearanceButton = 109;

	public int maxWidthOfBuildingViewer = 448;

	public int maxHeightOfBuildingViewer = 512;

	public int maxWidthOfDescription = 416;

	/// <summary>The name of the NPC whose building menu is being shown (the vanilla values are <see cref="F:StardewValley.Game1.builder_robin" /> and <see cref="F:StardewValley.Game1.builder_wizard" />). This affects which buildings are available to build based on the <see cref="F:StardewValley.GameData.Buildings.BuildingData.Builder" /> value.</summary>
	public readonly string Builder;

	/// <summary>The name of the location to return to after exiting the farm view.</summary>
	public readonly string BuilderLocationName;

	/// <summary>The viewport position to return to after exiting the farm view.</summary>
	public readonly Location BuilderViewport;

	/// <summary>The location in which to construct or manage buildings.</summary>
	public GameLocation TargetLocation;

	/// <summary>The tile to center on when switching to the <see cref="F:StardewValley.Menus.CarpenterMenu.TargetLocation" />, or <c>null</c> to apply the default behavior.</summary>
	public Vector2? TargetViewportCenterOnTile;

	/// <summary>The blueprints available in the menu.</summary>
	public readonly List<BlueprintEntry> Blueprints = new List<BlueprintEntry>();

	/// <summary>The current blueprint shown in the menu.</summary>
	public BlueprintEntry Blueprint;

	public ClickableTextureComponent okButton;

	public ClickableTextureComponent cancelButton;

	public ClickableTextureComponent backButton;

	public ClickableTextureComponent forwardButton;

	public ClickableTextureComponent upgradeIcon;

	public ClickableTextureComponent demolishButton;

	public ClickableTextureComponent moveButton;

	public ClickableTextureComponent paintButton;

	public ClickableTextureComponent appearanceButton;

	public Building currentBuilding;

	public Building buildingToMove;

	/// <summary>The materials needed to build the <see cref="F:StardewValley.Menus.CarpenterMenu.currentBuilding" />, if any. The stack size for each item is the number required.</summary>
	public readonly List<Item> ingredients = new List<Item>();

	/// <summary>Whether the menu is currently showing the target location (regardless of whether it's the farm), so the player can choose a building or position.</summary>
	public bool onFarm;

	public bool drawBG = true;

	public bool freeze;

	public bool upgrading;

	public bool demolishing;

	public bool moving;

	public bool painting;

	private string hoverText = "";

	public bool readOnly
	{
		set
		{
			if (value)
			{
				upgradeIcon.visible = false;
				demolishButton.visible = false;
				moveButton.visible = false;
				okButton.visible = false;
				paintButton.visible = false;
				cancelButton.leftNeighborID = 102;
			}
		}
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="builder">The name of the NPC whose building menu is being shown (the vanilla values are <see cref="F:StardewValley.Game1.builder_robin" /> and <see cref="F:StardewValley.Game1.builder_wizard" />). This affects which buildings are available to build based on the <see cref="F:StardewValley.GameData.Buildings.BuildingData.Builder" /> value.</param>
	/// <param name="targetLocation">The location in which to construct the building, or <c>null</c> for the farm.</param>
	public CarpenterMenu(string builder, GameLocation targetLocation = null)
	{
		Builder = builder;
		BuilderLocationName = Game1.currentLocation.NameOrUniqueName;
		BuilderViewport = Game1.viewport.Location;
		TargetLocation = targetLocation ?? Game1.getFarm();
		Game1.player.forceCanMove();
		resetBounds();
		int index = 0;
		foreach (KeyValuePair<string, BuildingData> data in Game1.buildingData)
		{
			if (data.Value.Builder != builder || !GameStateQuery.CheckConditions(data.Value.BuildCondition, targetLocation) || (data.Value.BuildingToUpgrade != null && TargetLocation.getNumberBuildingsConstructed(data.Value.BuildingToUpgrade) == 0) || !IsValidBuildingForLocation(data.Key, data.Value, TargetLocation))
			{
				continue;
			}
			Blueprints.Add(new BlueprintEntry(index++, data.Key, data.Value, null));
			if (data.Value.Skins == null)
			{
				continue;
			}
			foreach (BuildingSkin skin in data.Value.Skins)
			{
				if (skin.ShowAsSeparateConstructionEntry && GameStateQuery.CheckConditions(skin.Condition, TargetLocation))
				{
					Blueprints.Add(new BlueprintEntry(index++, data.Key, data.Value, skin.Id));
				}
			}
		}
		SetNewActiveBlueprint(0);
		if (Game1.options.SnappyMenus)
		{
			populateClickableComponentList();
			snapToDefaultClickableComponent();
		}
	}

	public override bool shouldClampGamePadCursor()
	{
		return onFarm;
	}

	public override void snapToDefaultClickableComponent()
	{
		currentlySnappedComponent = getComponentWithID(107);
		snapCursorToCurrentSnappedComponent();
	}

	private void resetBounds()
	{
		xPositionOnScreen = Game1.uiViewport.Width / 2 - maxWidthOfBuildingViewer - IClickableMenu.spaceToClearSideBorder;
		yPositionOnScreen = Game1.uiViewport.Height / 2 - maxHeightOfBuildingViewer / 2 - IClickableMenu.spaceToClearTopBorder + 32;
		width = maxWidthOfBuildingViewer + maxWidthOfDescription + IClickableMenu.spaceToClearSideBorder * 2 + 64;
		height = maxHeightOfBuildingViewer + IClickableMenu.spaceToClearTopBorder;
		initialize(xPositionOnScreen, yPositionOnScreen, width, height, showUpperRightCloseButton: true);
		okButton = new ClickableTextureComponent("OK", new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 192 - 12, yPositionOnScreen + maxHeightOfBuildingViewer + 64, 64, 64), null, null, Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(366, 373, 16, 16), 4f)
		{
			myID = 106,
			rightNeighborID = 104,
			leftNeighborID = 105,
			upNeighborID = 109
		};
		cancelButton = new ClickableTextureComponent("OK", new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 64, yPositionOnScreen + maxHeightOfBuildingViewer + 64, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 47), 1f)
		{
			myID = 107,
			leftNeighborID = 104,
			upNeighborID = 109
		};
		backButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + 64, yPositionOnScreen + maxHeightOfBuildingViewer + 64, 48, 44), Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(352, 495, 12, 11), 4f)
		{
			myID = 101,
			rightNeighborID = 102,
			upNeighborID = 109
		};
		forwardButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + maxWidthOfBuildingViewer - 256 + 16, yPositionOnScreen + maxHeightOfBuildingViewer + 64, 48, 44), Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(365, 495, 12, 11), 4f)
		{
			myID = 102,
			leftNeighborID = 101,
			rightNeighborID = -99998,
			upNeighborID = 109
		};
		demolishButton = new ClickableTextureComponent(Game1.content.LoadString("Strings\\UI:Carpenter_Demolish"), new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 128 - 8, yPositionOnScreen + maxHeightOfBuildingViewer + 64 - 4, 64, 64), null, null, Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(348, 372, 17, 17), 4f)
		{
			myID = 104,
			rightNeighborID = 107,
			leftNeighborID = 106,
			upNeighborID = 109
		};
		upgradeIcon = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + maxWidthOfBuildingViewer - 128 + 32, yPositionOnScreen + 8, 36, 52), Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(402, 328, 9, 13), 4f)
		{
			myID = 103,
			rightNeighborID = 104,
			leftNeighborID = 105,
			upNeighborID = 109
		};
		moveButton = new ClickableTextureComponent(Game1.content.LoadString("Strings\\UI:Carpenter_MoveBuildings"), new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 256 - 20, yPositionOnScreen + maxHeightOfBuildingViewer + 64, 64, 64), null, null, Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(257, 284, 16, 16), 4f)
		{
			myID = 105,
			rightNeighborID = 106,
			leftNeighborID = -99998,
			upNeighborID = 109
		};
		paintButton = new ClickableTextureComponent(Game1.content.LoadString("Strings\\UI:Carpenter_PaintBuildings"), new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 320 - 20, yPositionOnScreen + maxHeightOfBuildingViewer + 64, 64, 64), null, null, Game1.mouseCursors2, new Microsoft.Xna.Framework.Rectangle(80, 208, 16, 16), 4f)
		{
			myID = 105,
			rightNeighborID = -99998,
			leftNeighborID = -99998,
			upNeighborID = 109
		};
		appearanceButton = new ClickableTextureComponent(Game1.content.LoadString("Strings\\UI:Carpenter_ChangeAppearance"), new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + maxWidthOfBuildingViewer - 128 + 16, yPositionOnScreen + maxHeightOfBuildingViewer - 64 + 32, 64, 64), null, null, Game1.mouseCursors2, new Microsoft.Xna.Framework.Rectangle(96, 208, 16, 16), 4f)
		{
			myID = 109,
			downNeighborID = -99998
		};
		bool has_owned_buildings = false;
		bool has_paintable_buildings = false;
		foreach (Building building in TargetLocation.buildings)
		{
			if (building.hasCarpenterPermissions())
			{
				has_owned_buildings = true;
			}
			if ((building.CanBePainted() || building.CanBeReskinned(ignoreSeparateConstructionEntries: true)) && HasPermissionsToPaint(building))
			{
				has_paintable_buildings = true;
			}
		}
		demolishButton.visible = Game1.IsMasterGame;
		moveButton.visible = Game1.IsMasterGame || Game1.player.team.farmhandsCanMoveBuildings.Value == FarmerTeam.RemoteBuildingPermissions.On || (Game1.player.team.farmhandsCanMoveBuildings.Value == FarmerTeam.RemoteBuildingPermissions.OwnedBuildings && has_owned_buildings);
		paintButton.visible = has_paintable_buildings;
		if (!demolishButton.visible)
		{
			upgradeIcon.rightNeighborID = demolishButton.rightNeighborID;
			okButton.rightNeighborID = demolishButton.rightNeighborID;
			cancelButton.leftNeighborID = demolishButton.leftNeighborID;
		}
		if (!moveButton.visible)
		{
			upgradeIcon.leftNeighborID = moveButton.leftNeighborID;
			forwardButton.rightNeighborID = -99998;
			okButton.leftNeighborID = moveButton.leftNeighborID;
		}
		UpdateAppearanceButtonVisibility();
	}

	public void SetNewActiveBlueprint(int index)
	{
		index %= Blueprints.Count;
		if (index < 0)
		{
			index = Blueprints.Count + index;
		}
		SetNewActiveBlueprint(Blueprints[index]);
	}

	public void SetNewActiveBlueprint(BlueprintEntry blueprint)
	{
		Blueprint = blueprint;
		currentBuilding = Building.CreateInstanceFromId(blueprint.Id, Vector2.Zero);
		currentBuilding.skinId.Value = blueprint.Skin?.Id;
		ingredients.Clear();
		if (blueprint.BuildMaterials != null)
		{
			foreach (BuildingMaterial material in blueprint.BuildMaterials)
			{
				ingredients.Add(ItemRegistry.Create(material.ItemId, material.Amount));
			}
		}
		UpdateAppearanceButtonVisibility();
		if (Game1.options.SnappyMenus && currentlySnappedComponent != null && currentlySnappedComponent == appearanceButton && !appearanceButton.visible)
		{
			setCurrentlySnappedComponentTo(102);
			snapToDefaultClickableComponent();
		}
	}

	public virtual void UpdateAppearanceButtonVisibility()
	{
		if (appearanceButton != null && currentBuilding != null)
		{
			appearanceButton.visible = currentBuilding.CanBeReskinned(ignoreSeparateConstructionEntries: true);
		}
	}

	public override void performHoverAction(int x, int y)
	{
		cancelButton.tryHover(x, y);
		base.performHoverAction(x, y);
		if (!onFarm)
		{
			backButton.tryHover(x, y, 1f);
			forwardButton.tryHover(x, y, 1f);
			okButton.tryHover(x, y);
			demolishButton.tryHover(x, y);
			moveButton.tryHover(x, y);
			paintButton.tryHover(x, y);
			appearanceButton.tryHover(x, y);
			if (Blueprint.IsUpgrade && upgradeIcon.containsPoint(x, y))
			{
				hoverText = Game1.content.LoadString("Strings\\UI:Carpenter_Upgrade", Blueprint.GetDisplayNameForBuildingToUpgrade());
			}
			else if (demolishButton.containsPoint(x, y) && CanDemolishThis())
			{
				hoverText = Game1.content.LoadString("Strings\\UI:Carpenter_Demolish");
			}
			else if (moveButton.containsPoint(x, y))
			{
				hoverText = Game1.content.LoadString("Strings\\UI:Carpenter_MoveBuildings");
			}
			else if (okButton.containsPoint(x, y) && CanBuildCurrentBlueprint())
			{
				hoverText = Game1.content.LoadString("Strings\\UI:Carpenter_Build");
			}
			else if (paintButton.containsPoint(x, y))
			{
				hoverText = paintButton.name;
			}
			else if (appearanceButton.containsPoint(x, y))
			{
				hoverText = appearanceButton.name;
			}
			else
			{
				hoverText = "";
			}
		}
		else
		{
			if ((!upgrading && !demolishing && !moving && !painting) || freeze)
			{
				return;
			}
			foreach (Building building in TargetLocation.buildings)
			{
				building.color = Color.White;
			}
			Vector2 tile = new Vector2((Game1.viewport.X + Game1.getOldMouseX(ui_scale: false)) / 64, (Game1.viewport.Y + Game1.getOldMouseY(ui_scale: false)) / 64);
			Building b = TargetLocation.getBuildingAt(tile) ?? TargetLocation.getBuildingAt(new Vector2(tile.X, tile.Y + 1f)) ?? TargetLocation.getBuildingAt(new Vector2(tile.X, tile.Y + 2f)) ?? TargetLocation.getBuildingAt(new Vector2(tile.X, tile.Y + 3f));
			BuildingData data = b?.GetData();
			if (data != null)
			{
				int stickOutTilesHigh = (data.SourceRect.IsEmpty ? b.texture.Value.Height : b.GetData().SourceRect.Height) * 4 / 64 - (int)b.tilesHigh;
				if ((float)((int)b.tileY - stickOutTilesHigh) > tile.Y)
				{
					b = null;
				}
			}
			if (upgrading)
			{
				if (b != null)
				{
					b.color = ((b.buildingType == Blueprint.UpgradeFrom) ? (Color.Lime * 0.8f) : (Color.Red * 0.8f));
				}
			}
			else if (demolishing)
			{
				if (b != null && hasPermissionsToDemolish(b) && CanDemolishThis(b))
				{
					b.color = Color.Red * 0.8f;
				}
			}
			else if (moving)
			{
				if (b != null && hasPermissionsToMove(b))
				{
					b.color = Color.Lime * 0.8f;
				}
			}
			else if (painting && b != null && (b.CanBePainted() || b.CanBeReskinned(ignoreSeparateConstructionEntries: true)) && HasPermissionsToPaint(b))
			{
				b.color = Color.Lime * 0.8f;
			}
		}
	}

	public bool hasPermissionsToDemolish(Building b)
	{
		if (Game1.IsMasterGame)
		{
			return CanDemolishThis(b);
		}
		return false;
	}

	public bool HasPermissionsToPaint(Building b)
	{
		if ((b.isCabin || b.HasIndoorsName("Farmhouse")) && b.GetIndoors() is FarmHouse house)
		{
			if (house.IsOwnedByCurrentPlayer)
			{
				return true;
			}
			if (house.OwnerId.ToString() == Game1.player.spouse)
			{
				return true;
			}
			return false;
		}
		return true;
	}

	public bool hasPermissionsToMove(Building b)
	{
		if (!Game1.getFarm().greenhouseUnlocked.Value && b is GreenhouseBuilding)
		{
			return false;
		}
		if (Game1.IsMasterGame)
		{
			return true;
		}
		switch (Game1.player.team.farmhandsCanMoveBuildings.Value)
		{
		case FarmerTeam.RemoteBuildingPermissions.On:
			return true;
		case FarmerTeam.RemoteBuildingPermissions.OwnedBuildings:
			if (b.hasCarpenterPermissions())
			{
				return true;
			}
			break;
		}
		return false;
	}

	public override void receiveGamePadButton(Buttons b)
	{
		base.receiveGamePadButton(b);
		if (!onFarm)
		{
			switch (b)
			{
			case Buttons.LeftTrigger:
				SetNewActiveBlueprint(Blueprint.Index - 1);
				Game1.playSound("shwip");
				break;
			case Buttons.RightTrigger:
				SetNewActiveBlueprint(Blueprint.Index + 1);
				Game1.playSound("shwip");
				break;
			}
		}
	}

	public override void receiveKeyPress(Keys key)
	{
		if (freeze)
		{
			return;
		}
		if (!onFarm)
		{
			base.receiveKeyPress(key);
		}
		if (Game1.IsFading() || !onFarm)
		{
			return;
		}
		if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && readyToClose() && Game1.locationRequest == null)
		{
			returnToCarpentryMenu();
		}
		else if (!Game1.options.SnappyMenus)
		{
			if (Game1.options.doesInputListContain(Game1.options.moveDownButton, key))
			{
				Game1.panScreen(0, 4);
			}
			else if (Game1.options.doesInputListContain(Game1.options.moveRightButton, key))
			{
				Game1.panScreen(4, 0);
			}
			else if (Game1.options.doesInputListContain(Game1.options.moveUpButton, key))
			{
				Game1.panScreen(0, -4);
			}
			else if (Game1.options.doesInputListContain(Game1.options.moveLeftButton, key))
			{
				Game1.panScreen(-4, 0);
			}
		}
	}

	public override void update(GameTime time)
	{
		base.update(time);
		if (!onFarm || Game1.IsFading())
		{
			return;
		}
		int mouseX = Game1.getOldMouseX(ui_scale: false) + Game1.viewport.X;
		int mouseY = Game1.getOldMouseY(ui_scale: false) + Game1.viewport.Y;
		if (mouseX - Game1.viewport.X < 64)
		{
			Game1.panScreen(-8, 0);
		}
		else if (mouseX - (Game1.viewport.X + Game1.viewport.Width) >= -128)
		{
			Game1.panScreen(8, 0);
		}
		if (mouseY - Game1.viewport.Y < 64)
		{
			Game1.panScreen(0, -8);
		}
		else if (mouseY - (Game1.viewport.Y + Game1.viewport.Height) >= -64)
		{
			Game1.panScreen(0, 8);
		}
		Keys[] pressedKeys = Game1.oldKBState.GetPressedKeys();
		foreach (Keys key in pressedKeys)
		{
			receiveKeyPress(key);
		}
		if (Game1.IsMultiplayer)
		{
			return;
		}
		GameLocation target = TargetLocation;
		foreach (FarmAnimal value in target.animals.Values)
		{
			value.MovePosition(Game1.currentGameTime, Game1.viewport, target);
		}
	}

	protected bool VerifyTileAccessibility(int tileX, int tileY, Vector2 buildingPosition)
	{
		if (!TargetLocation.isTilePassable(new Location(tileX, tileY), Game1.viewport))
		{
			return false;
		}
		int relativeX = tileX - (int)buildingPosition.X;
		int relativeY = tileY - (int)buildingPosition.Y;
		if (!buildingToMove.isTilePassable(new Vector2((int)buildingToMove.tileX + relativeX, (int)buildingToMove.tileY + relativeY)))
		{
			return false;
		}
		Building tileBuilding = TargetLocation.getBuildingAt(new Vector2(tileX, tileY));
		if (tileBuilding != null && !tileBuilding.isMoving && !tileBuilding.isTilePassable(new Vector2(tileX, tileY)))
		{
			return false;
		}
		Microsoft.Xna.Framework.Rectangle tileRect = new Microsoft.Xna.Framework.Rectangle(tileX * 64, tileY * 64, 64, 64);
		tileRect.Inflate(-1, -1);
		foreach (ResourceClump resourceClump in TargetLocation.resourceClumps)
		{
			if (resourceClump.getBoundingBox().Intersects(tileRect))
			{
				return false;
			}
		}
		foreach (LargeTerrainFeature largeTerrainFeature in TargetLocation.largeTerrainFeatures)
		{
			if (largeTerrainFeature.getBoundingBox().Intersects(tileRect))
			{
				return false;
			}
		}
		return true;
	}

	public virtual bool ConfirmBuildingAccessibility(Vector2 buildingPosition)
	{
		if (buildingToMove == null)
		{
			return false;
		}
		if (buildingToMove.buildingType != "Farmhouse")
		{
			return true;
		}
		Point startPoint = buildingToMove.humanDoor.Value;
		startPoint.X += (int)buildingPosition.X;
		startPoint.Y += (int)buildingPosition.Y;
		startPoint.Y++;
		HashSet<Point> closedTiles = new HashSet<Point>();
		Stack<Point> openTiles = new Stack<Point>();
		openTiles.Push(startPoint);
		closedTiles.Add(startPoint);
		HashSet<Point> validWarpTiles = new HashSet<Point>();
		foreach (Warp w in TargetLocation.warps)
		{
			if (!(w.TargetName == "FarmCave"))
			{
				validWarpTiles.Add(new Point(w.X, w.Y));
			}
		}
		bool success = false;
		while (openTiles.Count > 0)
		{
			Point tile = openTiles.Pop();
			if (validWarpTiles.Contains(tile))
			{
				success = true;
				break;
			}
			if (TargetLocation.isTileOnMap(tile.X, tile.Y) && VerifyTileAccessibility(tile.X, tile.Y, buildingPosition))
			{
				Point newPoint = tile;
				newPoint.X++;
				if (closedTiles.Add(newPoint))
				{
					openTiles.Push(newPoint);
				}
				newPoint = tile;
				newPoint.X--;
				if (closedTiles.Add(newPoint))
				{
					openTiles.Push(newPoint);
				}
				newPoint = tile;
				newPoint.Y--;
				if (closedTiles.Add(newPoint))
				{
					openTiles.Push(newPoint);
				}
				newPoint = tile;
				newPoint.Y++;
				if (closedTiles.Add(newPoint))
				{
					openTiles.Push(newPoint);
				}
			}
		}
		return success;
	}

	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (freeze)
		{
			return;
		}
		if (!onFarm)
		{
			base.receiveLeftClick(x, y, playSound);
		}
		if (cancelButton.containsPoint(x, y))
		{
			if (onFarm)
			{
				if (moving && buildingToMove != null)
				{
					Game1.playSound("cancel");
					return;
				}
				returnToCarpentryMenu();
				Game1.playSound("smallSelect");
				return;
			}
			exitThisMenu();
			Game1.player.forceCanMove();
			Game1.playSound("bigDeSelect");
		}
		if (!onFarm && backButton.containsPoint(x, y))
		{
			SetNewActiveBlueprint(Blueprint.Index - 1);
			Game1.playSound("shwip");
			backButton.scale = backButton.baseScale;
		}
		if (!onFarm && forwardButton.containsPoint(x, y))
		{
			SetNewActiveBlueprint(Blueprint.Index + 1);
			forwardButton.scale = forwardButton.baseScale;
			Game1.playSound("shwip");
		}
		if (!onFarm)
		{
			if (demolishButton.containsPoint(x, y) && demolishButton.visible && CanDemolishThis())
			{
				Game1.globalFadeToBlack(setUpForBuildingPlacement);
				Game1.playSound("smallSelect");
				onFarm = true;
				demolishing = true;
			}
			if (moveButton.containsPoint(x, y) && moveButton.visible)
			{
				Game1.globalFadeToBlack(setUpForBuildingPlacement);
				Game1.playSound("smallSelect");
				onFarm = true;
				moving = true;
			}
			if (paintButton.containsPoint(x, y) && paintButton.visible)
			{
				Game1.globalFadeToBlack(setUpForBuildingPlacement);
				Game1.playSound("smallSelect");
				onFarm = true;
				painting = true;
			}
			if (appearanceButton.containsPoint(x, y) && appearanceButton.visible && currentBuilding.CanBeReskinned(ignoreSeparateConstructionEntries: true))
			{
				BuildingSkinMenu skinMenu = new BuildingSkinMenu(currentBuilding, ignoreSeparateConstructionEntries: true);
				Game1.playSound("smallSelect");
				BuildingSkinMenu buildingSkinMenu = skinMenu;
				buildingSkinMenu.behaviorBeforeCleanup = (Action<IClickableMenu>)Delegate.Combine(buildingSkinMenu.behaviorBeforeCleanup, (Action<IClickableMenu>)delegate
				{
					if (Game1.options.SnappyMenus)
					{
						setCurrentlySnappedComponentTo(109);
						snapCursorToCurrentSnappedComponent();
					}
					Blueprint.SetSkin(skinMenu.Skin?.Id);
				});
				SetChildMenu(skinMenu);
			}
			if (okButton.containsPoint(x, y) && !onFarm && CanBuildCurrentBlueprint())
			{
				Game1.globalFadeToBlack(setUpForBuildingPlacement);
				Game1.playSound("smallSelect");
				onFarm = true;
			}
		}
		if (!onFarm || freeze || Game1.IsFading())
		{
			return;
		}
		GameLocation farm;
		Building destroyed;
		GameLocation interior;
		Cabin cabin;
		if (demolishing)
		{
			farm = TargetLocation;
			destroyed = farm.getBuildingAt(new Vector2((Game1.viewport.X + Game1.getOldMouseX(ui_scale: false)) / 64, (Game1.viewport.Y + Game1.getOldMouseY(ui_scale: false)) / 64));
			if (destroyed == null)
			{
				return;
			}
			interior = destroyed.GetIndoors();
			cabin = interior as Cabin;
			if (destroyed != null)
			{
				if (cabin != null && !Game1.IsMasterGame)
				{
					Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_LockFailed"), 3));
					destroyed = null;
					return;
				}
				if (!CanDemolishThis(destroyed))
				{
					destroyed = null;
					return;
				}
				if (!Game1.IsMasterGame && !hasPermissionsToDemolish(destroyed))
				{
					destroyed = null;
					return;
				}
			}
			Cabin cabin2 = cabin;
			if (cabin2 != null && cabin2.HasOwner && cabin.owner.isCustomized.Value)
			{
				Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\UI:Carpenter_DemolishCabinConfirm", cabin.owner.Name), Game1.currentLocation.createYesNoResponses(), delegate(Farmer f, string answer)
				{
					if (answer == "Yes")
					{
						Game1.activeClickableMenu = this;
						Game1.player.team.demolishLock.RequestLock(ContinueDemolish, BuildingLockFailed);
					}
					else
					{
						DelayedAction.functionAfterDelay(returnToCarpentryMenu, 500);
					}
				});
			}
			else if (destroyed != null)
			{
				Game1.player.team.demolishLock.RequestLock(ContinueDemolish, BuildingLockFailed);
			}
			return;
		}
		if (upgrading)
		{
			Building toUpgrade = TargetLocation.getBuildingAt(new Vector2((Game1.viewport.X + Game1.getOldMouseX(ui_scale: false)) / 64, (Game1.viewport.Y + Game1.getOldMouseY(ui_scale: false)) / 64));
			if (toUpgrade != null && toUpgrade.buildingType == Blueprint.UpgradeFrom)
			{
				ConsumeResources();
				toUpgrade.upgradeName.Value = Blueprint.Id;
				toUpgrade.daysUntilUpgrade.Value = Math.Max(Blueprint.BuildDays, 1);
				toUpgrade.showUpgradeAnimation(TargetLocation);
				Game1.playSound("axe");
				DelayedAction.functionAfterDelay(returnToCarpentryMenuAfterSuccessfulBuild, 1500);
				freeze = true;
				Game1.multiplayer.globalChatInfoMessage("BuildingBuild", Game1.player.Name, "aOrAn:" + Blueprint.TokenizedDisplayName, Blueprint.TokenizedDisplayName, Game1.player.farmName);
				if (Blueprint.BuildDays < 1)
				{
					toUpgrade.FinishConstruction();
				}
				else
				{
					Game1.netWorldState.Value.MarkUnderConstruction(Builder, toUpgrade);
				}
			}
			else if (toUpgrade != null)
			{
				Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantUpgrade_BuildingType"), 3));
			}
			return;
		}
		if (painting)
		{
			Vector2 tile_position = new Vector2((Game1.viewport.X + Game1.getMouseX(ui_scale: false)) / 64, (Game1.viewport.Y + Game1.getMouseY(ui_scale: false)) / 64);
			Building paint_building = TargetLocation.getBuildingAt(tile_position);
			if (paint_building != null)
			{
				if (!paint_building.CanBePainted() && !paint_building.CanBeReskinned(ignoreSeparateConstructionEntries: true))
				{
					Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CannotPaint"), 3));
					return;
				}
				if (!HasPermissionsToPaint(paint_building))
				{
					Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CannotPaint_Permission"), 3));
					return;
				}
				paint_building.color = Color.White;
				SetChildMenu(paint_building.CanBePainted() ? ((IClickableMenu)new BuildingPaintMenu(paint_building)) : ((IClickableMenu)new BuildingSkinMenu(paint_building, ignoreSeparateConstructionEntries: true)));
			}
			return;
		}
		if (moving)
		{
			if (buildingToMove == null)
			{
				buildingToMove = TargetLocation.getBuildingAt(new Vector2((Game1.viewport.X + Game1.getMouseX(ui_scale: false)) / 64, (Game1.viewport.Y + Game1.getMouseY(ui_scale: false)) / 64));
				if (buildingToMove != null)
				{
					if ((int)buildingToMove.daysOfConstructionLeft > 0)
					{
						buildingToMove = null;
						return;
					}
					if (!hasPermissionsToMove(buildingToMove))
					{
						buildingToMove = null;
						return;
					}
					buildingToMove.isMoving = true;
					Game1.playSound("axchop");
				}
				return;
			}
			Vector2 buildingPosition = new Vector2((Game1.viewport.X + Game1.getMouseX(ui_scale: false)) / 64, (Game1.viewport.Y + Game1.getMouseY(ui_scale: false)) / 64);
			if (ConfirmBuildingAccessibility(buildingPosition))
			{
				if (TargetLocation.buildStructure(buildingToMove, buildingPosition, Game1.player))
				{
					buildingToMove.isMoving = false;
					buildingToMove = null;
					Game1.playSound("axchop");
					DelayedAction.playSoundAfterDelay("dirtyHit", 50);
					DelayedAction.playSoundAfterDelay("dirtyHit", 150);
				}
				else
				{
					Game1.playSound("cancel");
				}
			}
			else
			{
				Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantBuild"), 3));
				Game1.playSound("cancel");
			}
			return;
		}
		Game1.player.team.buildLock.RequestLock(delegate
		{
			if (onFarm && Game1.locationRequest == null)
			{
				if (tryToBuild())
				{
					ConsumeResources();
					DelayedAction.functionAfterDelay(returnToCarpentryMenuAfterSuccessfulBuild, 2000);
					freeze = true;
				}
				else
				{
					Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantBuild"), 3));
				}
			}
			Game1.player.team.buildLock.ReleaseLock();
		});
		void BuildingLockFailed()
		{
			if (demolishing)
			{
				Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_LockFailed"), 3));
			}
		}
		void ContinueDemolish()
		{
			if (demolishing && destroyed != null && farm.buildings.Contains(destroyed))
			{
				if ((int)destroyed.daysOfConstructionLeft > 0 || (int)destroyed.daysUntilUpgrade > 0)
				{
					Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_DuringConstruction"), 3));
				}
				else if (interior is AnimalHouse animalHouse && animalHouse.animalsThatLiveHere.Count > 0)
				{
					Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_AnimalsHere"), 3));
				}
				else if (interior != null && interior.farmers.Any())
				{
					Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_PlayerHere"), 3));
				}
				else
				{
					if (cabin != null)
					{
						foreach (Farmer farmer in Game1.getAllFarmers())
						{
							if (farmer.currentLocation != null && farmer.currentLocation.Name == cabin.GetCellarName())
							{
								Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_PlayerHere"), 3));
								return;
							}
						}
						if (cabin.IsOwnerActivated)
						{
							Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_FarmhandOnline"), 3));
							return;
						}
					}
					destroyed.BeforeDemolish();
					Chest chest = null;
					if (cabin != null)
					{
						List<Item> items = cabin.demolish();
						if (items.Count > 0)
						{
							chest = new Chest(playerChest: true);
							chest.fixLidFrame();
							chest.Items.OverwriteWith(items);
						}
					}
					if (farm.destroyStructure(destroyed))
					{
						Game1.flashAlpha = 1f;
						destroyed.showDestroyedAnimation(TargetLocation);
						Game1.playSound("explosion");
						Utility.spreadAnimalsAround(destroyed, farm);
						DelayedAction.functionAfterDelay(returnToCarpentryMenu, 1500);
						freeze = true;
						if (chest != null)
						{
							farm.objects[new Vector2((int)destroyed.tileX + (int)destroyed.tilesWide / 2, (int)destroyed.tileY + (int)destroyed.tilesHigh / 2)] = chest;
						}
					}
				}
			}
		}
	}

	public bool tryToBuild()
	{
		NetString skinId = currentBuilding.skinId;
		Vector2 tileLocation = new Vector2((Game1.viewport.X + Game1.getOldMouseX(ui_scale: false)) / 64, (Game1.viewport.Y + Game1.getOldMouseY(ui_scale: false)) / 64);
		if (TargetLocation.buildStructure(currentBuilding.buildingType.Value, tileLocation, Game1.player, out var building, Blueprint.MagicalConstruction))
		{
			building.skinId.Value = skinId;
			if (building.isUnderConstruction())
			{
				Game1.netWorldState.Value.MarkUnderConstruction(Builder, building);
			}
			return true;
		}
		return false;
	}

	public virtual void returnToCarpentryMenu()
	{
		LocationRequest locationRequest = Game1.getLocationRequest(BuilderLocationName);
		locationRequest.OnWarp += delegate
		{
			onFarm = false;
			Game1.player.viewingLocation.Value = null;
			resetBounds();
			upgrading = false;
			moving = false;
			painting = false;
			buildingToMove = null;
			freeze = false;
			Game1.displayHUD = true;
			Game1.viewportFreeze = false;
			Game1.viewport.Location = BuilderViewport;
			drawBG = true;
			demolishing = false;
			Game1.displayFarmer = true;
			if (Game1.options.SnappyMenus)
			{
				populateClickableComponentList();
				snapToDefaultClickableComponent();
			}
		};
		Game1.warpFarmer(locationRequest, Game1.player.TilePoint.X, Game1.player.TilePoint.Y, Game1.player.FacingDirection);
	}

	public void returnToCarpentryMenuAfterSuccessfulBuild()
	{
		LocationRequest locationRequest = Game1.getLocationRequest(BuilderLocationName);
		locationRequest.OnWarp += delegate
		{
			Game1.displayHUD = true;
			Game1.player.viewingLocation.Value = null;
			Game1.viewportFreeze = false;
			Game1.viewport.Location = BuilderViewport;
			freeze = true;
			Game1.displayFarmer = true;
			robinConstructionMessage();
		};
		Game1.warpFarmer(locationRequest, Game1.player.TilePoint.X, Game1.player.TilePoint.Y, Game1.player.FacingDirection);
	}

	public void robinConstructionMessage()
	{
		exitThisMenu();
		Game1.player.forceCanMove();
		if (Blueprint.MagicalConstruction)
		{
			return;
		}
		string dialoguePath = "Data\\ExtraDialogue:Robin_" + (upgrading ? "Upgrade" : "New") + "Construction";
		if (Utility.isFestivalDay(Game1.dayOfMonth + 1, Game1.season))
		{
			dialoguePath += "_Festival";
		}
		string displayName = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.de) ? Blueprint.DisplayName : Blueprint.DisplayName.ToLower());
		if (Blueprint.BuildDays <= 0)
		{
			Game1.DrawDialogue(Game1.getCharacterFromName("Robin"), "Data\\ExtraDialogue:Robin_Instant", displayName);
			return;
		}
		string[] displayNameParts = ArgUtility.SplitBySpace(Blueprint.DisplayName);
		string namePart;
		switch (LocalizedContentManager.CurrentLanguageCode)
		{
		case LocalizedContentManager.LanguageCode.de:
			namePart = displayNameParts.Last().Split('-').Last();
			break;
		case LocalizedContentManager.LanguageCode.pt:
		case LocalizedContentManager.LanguageCode.es:
		case LocalizedContentManager.LanguageCode.it:
			namePart = displayNameParts[0].ToLower();
			break;
		default:
			namePart = displayNameParts.Last().ToLower();
			break;
		}
		Game1.DrawDialogue(Game1.getCharacterFromName("Robin"), dialoguePath, displayName, namePart);
	}

	public override bool overrideSnappyMenuCursorMovementBan()
	{
		return onFarm;
	}

	public void setUpForBuildingPlacement()
	{
		Game1.currentLocation.cleanupBeforePlayerExit();
		hoverText = "";
		Game1.currentLocation = TargetLocation;
		Game1.player.viewingLocation.Value = TargetLocation.NameOrUniqueName;
		Game1.currentLocation.resetForPlayerEntry();
		Game1.globalFadeToClear();
		onFarm = true;
		cancelButton.bounds.X = Game1.uiViewport.Width - 128;
		cancelButton.bounds.Y = Game1.uiViewport.Height - 128;
		Game1.displayHUD = false;
		Game1.viewportFreeze = true;
		Game1.viewport.Location = GetInitialBuildingPlacementViewport(TargetLocation);
		Game1.clampViewportToGameMap();
		Game1.panScreen(0, 0);
		drawBG = false;
		freeze = false;
		Game1.displayFarmer = false;
		if (!demolishing && Blueprint.IsUpgrade && !moving && !painting)
		{
			upgrading = true;
		}
	}

	/// <summary>Get the viewport to set when we start building placement.</summary>
	/// <param name="location">The location for which to get a viewport.</param>
	public Location GetInitialBuildingPlacementViewport(GameLocation location)
	{
		if (TargetViewportCenterOnTile.HasValue)
		{
			Vector2 tile = TargetViewportCenterOnTile.Value;
			return CenterOnTile((int)tile.X, (int)tile.Y);
		}
		Building building = location.getBuildingByName("FarmHouse") ?? location.buildings.FirstOrDefault();
		if (building != null)
		{
			return CenterOnTile(building.tileX.Value + building.tilesWide.Value / 2, building.tileY.Value + building.tilesHigh.Value / 2);
		}
		Layer layer = location.Map.Layers[0];
		return CenterOnTile(layer.LayerWidth / 2, layer.LayerHeight / 2);
		static Location CenterOnTile(int x, int y)
		{
			x = (int)((float)(x * 64) - (float)Game1.viewport.Width / 2f);
			y = (int)((float)(y * 64) - (float)Game1.viewport.Height / 2f);
			return new Location(x, y);
		}
	}

	public override void gameWindowSizeChanged(Microsoft.Xna.Framework.Rectangle oldBounds, Microsoft.Xna.Framework.Rectangle newBounds)
	{
		resetBounds();
	}

	/// <summary>Get whether a building can ever be built in the target location.</summary>
	/// <param name="typeId">The building type ID in <c>Data/Buildings</c>.</param>
	/// <param name="data">The building data from <c>Data/Buildings</c>.</param>
	/// <param name="targetLocation">The location it would be built in.</param>
	public virtual bool IsValidBuildingForLocation(string typeId, BuildingData data, GameLocation targetLocation)
	{
		if (typeId == "Cabin" && TargetLocation.Name != "Farm")
		{
			return false;
		}
		return true;
	}

	/// <summary>Get whether the player can build the current blueprint now.</summary>
	public virtual bool CanBuildCurrentBlueprint()
	{
		BlueprintEntry blueprint = Blueprint;
		if (!IsValidBuildingForLocation(blueprint.Id, blueprint.Data, TargetLocation))
		{
			return false;
		}
		if (!DoesFarmerHaveEnoughResourcesToBuild())
		{
			return false;
		}
		if (blueprint.BuildCost > 0 && Game1.player.Money < blueprint.BuildCost)
		{
			return false;
		}
		return true;
	}

	/// <summary>Get whether it's safe to demolish the current building.</summary>
	public bool CanDemolishThis()
	{
		return CanDemolishThis(currentBuilding);
	}

	/// <summary>Get whether it's safe to demolish a given building.</summary>
	/// <param name="building">The building to check.</param>
	public virtual bool CanDemolishThis(Building building)
	{
		string type = building?.buildingType.Value;
		switch (type)
		{
		case "Farmhouse":
			if (building.HasIndoorsName("FarmHouse"))
			{
				return false;
			}
			break;
		case "Greenhouse":
			if (building.HasIndoorsName("Greenhouse"))
			{
				return false;
			}
			break;
		case "Pet Bowl":
		case "Shipping Bin":
			if (TargetLocation == Game1.getFarm() && !TargetLocation.HasMinBuildings(type, 2))
			{
				return false;
			}
			break;
		}
		return building != null;
	}

	public override void draw(SpriteBatch b)
	{
		BlueprintEntry blueprint = Blueprint;
		if (drawBG && !Game1.options.showClearBackgrounds)
		{
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.6f);
		}
		if (Game1.IsFading() || freeze)
		{
			return;
		}
		if (!onFarm)
		{
			base.draw(b);
			Microsoft.Xna.Framework.Rectangle rectangle = new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen - 96, yPositionOnScreen - 16, maxWidthOfBuildingViewer + 64, maxHeightOfBuildingViewer + 64);
			IClickableMenu.drawTextureBox(b, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, blueprint.MagicalConstruction ? Color.RoyalBlue : Color.White);
			rectangle.Inflate(-12, -12);
			b.End();
			b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, Utility.ScissorEnabled);
			b.GraphicsDevice.ScissorRectangle = rectangle;
			Microsoft.Xna.Framework.Rectangle sourceRect = currentBuilding.getSourceRectForMenu() ?? currentBuilding.getSourceRect();
			Point offset = blueprint.Data.BuildMenuDrawOffset;
			currentBuilding.drawInMenu(b, xPositionOnScreen + maxWidthOfBuildingViewer / 2 - (int)currentBuilding.tilesWide * 64 / 2 - 64 + offset.X, yPositionOnScreen + maxHeightOfBuildingViewer / 2 - sourceRect.Height * 4 / 2 + offset.Y);
			b.End();
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			if (blueprint.IsUpgrade)
			{
				upgradeIcon.draw(b);
			}
			string placeholder = " Deluxe  Barn   ";
			if (SpriteText.getWidthOfString(blueprint.DisplayName) >= SpriteText.getWidthOfString(placeholder))
			{
				placeholder = blueprint.DisplayName + " ";
			}
			SpriteText.drawStringWithScrollCenteredAt(b, blueprint.DisplayName, xPositionOnScreen + maxWidthOfBuildingViewer - IClickableMenu.spaceToClearSideBorder - 16 + 64 + (width - (maxWidthOfBuildingViewer + 128)) / 2, yPositionOnScreen, SpriteText.getWidthOfString(placeholder));
			int descriptionWidth = LocalizedContentManager.CurrentLanguageCode switch
			{
				LocalizedContentManager.LanguageCode.es => maxWidthOfDescription + 64 + ((blueprint.Id == "Deluxe Barn") ? 96 : 0), 
				LocalizedContentManager.LanguageCode.it => maxWidthOfDescription + 96, 
				LocalizedContentManager.LanguageCode.fr => maxWidthOfDescription + 96 + ((blueprint.Id == "Slime Hutch" || blueprint.Id == "Deluxe Coop" || blueprint.Id == "Deluxe Barn") ? 72 : 0), 
				LocalizedContentManager.LanguageCode.ko => maxWidthOfDescription + 96 + ((blueprint.Id == "Slime Hutch") ? 64 : ((blueprint.Id == "Deluxe Coop") ? 96 : ((blueprint.Id == "Deluxe Barn") ? 112 : ((blueprint.Id == "Big Barn") ? 64 : 0)))), 
				_ => maxWidthOfDescription + 64, 
			};
			IClickableMenu.drawTextureBox(b, xPositionOnScreen + maxWidthOfBuildingViewer - 16, yPositionOnScreen + 80, descriptionWidth, maxHeightOfBuildingViewer - 32, blueprint.MagicalConstruction ? Color.RoyalBlue : Color.White);
			if (blueprint.MagicalConstruction)
			{
				Utility.drawTextWithShadow(b, Game1.parseText(blueprint.Description, Game1.dialogueFont, descriptionWidth - 32), Game1.dialogueFont, new Vector2(xPositionOnScreen + maxWidthOfBuildingViewer - 4, yPositionOnScreen + 80 + 16 + 4), Game1.textColor * 0.25f, 1f, -1f, -1, -1, 0f);
				Utility.drawTextWithShadow(b, Game1.parseText(blueprint.Description, Game1.dialogueFont, descriptionWidth - 32), Game1.dialogueFont, new Vector2(xPositionOnScreen + maxWidthOfBuildingViewer - 1, yPositionOnScreen + 80 + 16 + 4), Game1.textColor * 0.25f, 1f, -1f, -1, -1, 0f);
			}
			Utility.drawTextWithShadow(b, Game1.parseText(blueprint.Description, Game1.dialogueFont, descriptionWidth - 32), Game1.dialogueFont, new Vector2(xPositionOnScreen + maxWidthOfBuildingViewer, yPositionOnScreen + 80 + 16), blueprint.MagicalConstruction ? Color.PaleGoldenrod : Game1.textColor, 1f, -1f, -1, -1, blueprint.MagicalConstruction ? 0f : 0.75f);
			Vector2 ingredientsPosition = new Vector2(xPositionOnScreen + maxWidthOfBuildingViewer + 16, yPositionOnScreen + 256 + 32);
			if (ingredients.Count < 3 && (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.fr || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.pt))
			{
				ingredientsPosition.Y += 64f;
			}
			if (blueprint.BuildCost >= 0)
			{
				b.Draw(Game1.mouseCursors_1_6, ingredientsPosition + new Vector2(-8f, -4f), new Microsoft.Xna.Framework.Rectangle(241, 303, 14, 13), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.89f);
				string price_string = Utility.getNumberWithCommas(blueprint.BuildCost);
				if (blueprint.MagicalConstruction)
				{
					Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", price_string), Game1.dialogueFont, new Vector2(ingredientsPosition.X + 64f, ingredientsPosition.Y + 8f), Game1.textColor * 0.5f, 1f, -1f, -1, -1, 0f);
					Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", price_string), Game1.dialogueFont, new Vector2(ingredientsPosition.X + 64f + 4f - 1f, ingredientsPosition.Y + 8f), Game1.textColor * 0.25f, 1f, -1f, -1, -1, 0f);
				}
				Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", price_string), Game1.dialogueFont, new Vector2(ingredientsPosition.X + 64f + 4f, ingredientsPosition.Y + 4f), (Game1.player.Money < blueprint.BuildCost) ? Color.Red : (blueprint.MagicalConstruction ? Color.PaleGoldenrod : Game1.textColor), 1f, -1f, -1, -1, blueprint.MagicalConstruction ? 0f : 0.25f);
			}
			if (!blueprint.MagicalConstruction)
			{
				int daysToBuild = blueprint.BuildDays;
				string timeString = ((daysToBuild > 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:QuestLog.cs.11374", daysToBuild) : ((daysToBuild == 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:QuestLog.cs.11375", daysToBuild) : Game1.content.LoadString("Strings\\1_6_Strings:Instant")));
				rectangle = new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen - 96 + width + 64, yPositionOnScreen + 80, 72 + (int)Game1.smallFont.MeasureString(timeString).X, 68);
				IClickableMenu.drawTextureBox(b, rectangle.X - 8, rectangle.Y, rectangle.Width + 16, rectangle.Height, Color.White);
				b.Draw(Game1.mouseCursors, new Vector2(rectangle.X + 8, rectangle.Y + 16), new Microsoft.Xna.Framework.Rectangle(410, 501, 9, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.89f);
				Utility.drawTextWithShadow(b, timeString, Game1.smallFont, new Vector2(rectangle.X + 8 + 44, rectangle.Y + 20), Game1.textColor);
			}
			ingredientsPosition.X -= 16f;
			ingredientsPosition.Y -= 21f;
			foreach (Item i in ingredients)
			{
				ingredientsPosition.Y += 68f;
				i.drawInMenu(b, ingredientsPosition, 1f);
				bool hasItem = Game1.player.Items.ContainsId(i.QualifiedItemId, i.Stack);
				if (blueprint.MagicalConstruction)
				{
					Utility.drawTextWithShadow(b, i.DisplayName, Game1.dialogueFont, new Vector2(ingredientsPosition.X + 64f + 12f, ingredientsPosition.Y + 24f), Game1.textColor * 0.25f, 1f, -1f, -1, -1, 0f);
					Utility.drawTextWithShadow(b, i.DisplayName, Game1.dialogueFont, new Vector2(ingredientsPosition.X + 64f + 16f - 1f, ingredientsPosition.Y + 24f), Game1.textColor * 0.25f, 1f, -1f, -1, -1, 0f);
				}
				Utility.drawTextWithShadow(b, i.DisplayName, Game1.dialogueFont, new Vector2(ingredientsPosition.X + 64f + 16f, ingredientsPosition.Y + 20f), hasItem ? (blueprint.MagicalConstruction ? Color.PaleGoldenrod : Game1.textColor) : Color.Red, 1f, -1f, -1, -1, blueprint.MagicalConstruction ? 0f : 0.25f);
			}
			backButton.draw(b);
			forwardButton.draw(b);
			okButton.draw(b, CanBuildCurrentBlueprint() ? Color.White : (Color.Gray * 0.8f), 0.88f);
			demolishButton.draw(b, CanDemolishThis() ? Color.White : (Color.Gray * 0.8f), 0.88f);
			moveButton.draw(b);
			paintButton.draw(b);
			appearanceButton.draw(b);
		}
		else
		{
			string message = (upgrading ? Game1.content.LoadString("Strings\\UI:Carpenter_SelectBuilding_Upgrade", blueprint.GetDisplayNameForBuildingToUpgrade()) : (demolishing ? Game1.content.LoadString("Strings\\UI:Carpenter_SelectBuilding_Demolish") : ((!painting) ? Game1.content.LoadString("Strings\\UI:Carpenter_ChooseLocation") : Game1.content.LoadString("Strings\\UI:Carpenter_SelectBuilding_Paint"))));
			SpriteText.drawStringWithScrollBackground(b, message, Game1.uiViewport.Width / 2 - SpriteText.getWidthOfString(message) / 2, 16);
			Game1.StartWorldDrawInUI(b);
			if (!upgrading && !demolishing && !moving && !painting)
			{
				Vector2 mousePositionTile = new Vector2((Game1.viewport.X + Game1.getOldMouseX(ui_scale: false)) / 64, (Game1.viewport.Y + Game1.getOldMouseY(ui_scale: false)) / 64);
				for (int y = 0; y < currentBuilding.tilesHigh.Value; y++)
				{
					for (int x = 0; x < currentBuilding.tilesWide.Value; x++)
					{
						int sheetIndex = currentBuilding.getTileSheetIndexForStructurePlacementTile(x, y);
						Vector2 currentGlobalTilePosition = new Vector2(mousePositionTile.X + (float)x, mousePositionTile.Y + (float)y);
						if (!Game1.currentLocation.isBuildable(currentGlobalTilePosition))
						{
							sheetIndex++;
						}
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, currentGlobalTilePosition * 64f), new Microsoft.Xna.Framework.Rectangle(194 + sheetIndex * 16, 388, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.999f);
					}
				}
				foreach (BuildingPlacementTile additionalPlacementTile in currentBuilding.GetAdditionalPlacementTiles())
				{
					bool onlyNeedsToBePassable = additionalPlacementTile.OnlyNeedsToBePassable;
					foreach (Point point in additionalPlacementTile.TileArea.GetPoints())
					{
						int x = point.X;
						int y = point.Y;
						int sheetIndex = currentBuilding.getTileSheetIndexForStructurePlacementTile(x, y);
						Vector2 currentGlobalTilePosition = new Vector2(mousePositionTile.X + (float)x, mousePositionTile.Y + (float)y);
						if (!Game1.currentLocation.isBuildable(currentGlobalTilePosition, onlyNeedsToBePassable))
						{
							sheetIndex++;
						}
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, currentGlobalTilePosition * 64f), new Microsoft.Xna.Framework.Rectangle(194 + sheetIndex * 16, 388, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.999f);
					}
				}
			}
			else if (!painting && moving && buildingToMove != null)
			{
				Vector2 mousePositionTile = new Vector2((Game1.viewport.X + Game1.getOldMouseX(ui_scale: false)) / 64, (Game1.viewport.Y + Game1.getOldMouseY(ui_scale: false)) / 64);
				for (int y = 0; y < (int)buildingToMove.tilesHigh; y++)
				{
					for (int x = 0; x < (int)buildingToMove.tilesWide; x++)
					{
						int sheetIndex = buildingToMove.getTileSheetIndexForStructurePlacementTile(x, y);
						Vector2 currentGlobalTilePosition = new Vector2(mousePositionTile.X + (float)x, mousePositionTile.Y + (float)y);
						if (!Game1.currentLocation.isBuildable(currentGlobalTilePosition))
						{
							sheetIndex++;
						}
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, currentGlobalTilePosition * 64f), new Microsoft.Xna.Framework.Rectangle(194 + sheetIndex * 16, 388, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.999f);
					}
				}
				foreach (BuildingPlacementTile additionalPlacementTile2 in buildingToMove.GetAdditionalPlacementTiles())
				{
					bool onlyNeedsToBePassable = additionalPlacementTile2.OnlyNeedsToBePassable;
					foreach (Point point2 in additionalPlacementTile2.TileArea.GetPoints())
					{
						int x = point2.X;
						int y = point2.Y;
						int sheetIndex = buildingToMove.getTileSheetIndexForStructurePlacementTile(x, y);
						Vector2 currentGlobalTilePosition = new Vector2(mousePositionTile.X + (float)x, mousePositionTile.Y + (float)y);
						if (!Game1.currentLocation.isBuildable(currentGlobalTilePosition, onlyNeedsToBePassable))
						{
							sheetIndex++;
						}
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, currentGlobalTilePosition * 64f), new Microsoft.Xna.Framework.Rectangle(194 + sheetIndex * 16, 388, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.999f);
					}
				}
			}
			Game1.EndWorldDrawInUI(b);
		}
		cancelButton.draw(b);
		if (GetChildMenu() == null)
		{
			drawMouse(b);
			if (hoverText.Length > 0)
			{
				IClickableMenu.drawHoverText(b, hoverText, Game1.dialogueFont);
			}
		}
	}

	/// <summary>Deduct the money and materials from the player's inventory to construct the current blueprint.</summary>
	public void ConsumeResources()
	{
		BlueprintEntry blueprint = Blueprint;
		foreach (Item ingredient in ingredients)
		{
			Game1.player.Items.ReduceId(ingredient.QualifiedItemId, ingredient.Stack);
		}
		Game1.player.Money -= blueprint.BuildCost;
	}

	/// <summary>Get whether the player has the money and materials needed to construct the current blueprint.</summary>
	public bool DoesFarmerHaveEnoughResourcesToBuild()
	{
		BlueprintEntry blueprint = Blueprint;
		if (blueprint.BuildCost < 0)
		{
			return false;
		}
		foreach (Item item in ingredients)
		{
			if (!Game1.player.Items.ContainsId(item.QualifiedItemId, item.Stack))
			{
				return false;
			}
		}
		return Game1.player.Money >= blueprint.BuildCost;
	}
}
