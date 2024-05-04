using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.GameData.Buildings;
using StardewValley.TokenizableStrings;

namespace StardewValley.Menus;

public class BuildingSkinMenu : IClickableMenu
{
	/// <summary>Metadata for a skin shown in the menu.</summary>
	public class SkinEntry
	{
		/// <summary>The index of the skin in the menu's list.</summary>
		public int Index;

		/// <summary>The skin ID in <c>Data/Buildings</c>.</summary>
		public readonly string Id;

		/// <summary>The translated display name.</summary>
		public readonly string DisplayName;

		/// <summary>The translated description.</summary>
		public readonly string Description;

		/// <summary>The skin data from <c>Data/Buildings</c>.</summary>
		public readonly BuildingSkin Data;

		/// <summary>Construct an instance.</summary>
		/// <param name="index">The index of the skin in the menu's list.</param>
		/// <param name="skin">The skin ID in <c>Data/Buildings</c>.</param>
		public SkinEntry(int index, BuildingSkin skin)
			: this(index, skin, TokenParser.ParseText(skin.Name), TokenParser.ParseText(skin.Description))
		{
		}

		/// <summary>Construct an instance.</summary>
		/// <param name="index">The index of the skin in the menu's list.</param>
		/// <param name="skin">The skin data from <c>Data/Buildings</c>.</param>
		/// <param name="displayName">The translated display name.</param>
		/// <param name="description">The translated description.</param>
		public SkinEntry(int index, BuildingSkin skin, string displayName, string description)
		{
			Index = index;
			Id = skin?.Id;
			Data = skin;
			DisplayName = displayName;
			Description = description;
		}
	}

	public const int region_okButton = 101;

	public const int region_nextSkin = 102;

	public const int region_prevSkin = 103;

	public static int WindowWidth = 576;

	public static int WindowHeight = 576;

	public Rectangle PreviewPane;

	public ClickableTextureComponent OkButton;

	/// <summary>The building whose skin to change.</summary>
	public Building Building;

	public ClickableTextureComponent NextSkinButton;

	public ClickableTextureComponent PreviousSkinButton;

	public string BuildingDisplayName;

	public string BuildingDescription;

	/// <summary>The building skins available in the menu.</summary>
	public List<SkinEntry> Skins = new List<SkinEntry>();

	/// <summary>The current building skin shown in the menu.</summary>
	public SkinEntry Skin;

	/// <summary>Construct an instance.</summary>
	/// <param name="targetBuilding">The building whose skin to change.</param>
	/// <param name="ignoreSeparateConstructionEntries">Whether to ignore skins with <see cref="F:StardewValley.GameData.Buildings.BuildingSkin.ShowAsSeparateConstructionEntry" /> set to true.</param>
	public BuildingSkinMenu(Building targetBuilding, bool ignoreSeparateConstructionEntries = false)
		: base(Game1.uiViewport.Width / 2 - WindowWidth / 2, Game1.uiViewport.Height / 2 - WindowHeight / 2, WindowWidth, WindowHeight)
	{
		Game1.player.Halt();
		Building = targetBuilding;
		BuildingData buildingData = targetBuilding.GetData();
		BuildingDisplayName = TokenParser.ParseText(buildingData.Name);
		BuildingDescription = TokenParser.ParseText(buildingData.Description);
		int index = 0;
		Skins.Add(new SkinEntry(index++, null, BuildingDisplayName, BuildingDescription));
		if (buildingData.Skins != null)
		{
			foreach (BuildingSkin skin in buildingData.Skins)
			{
				if (!(skin.Id != Building.skinId.Value) || ((!ignoreSeparateConstructionEntries || !skin.ShowAsSeparateConstructionEntry) && GameStateQuery.CheckConditions(skin.Condition, Building.GetParentLocation())))
				{
					Skins.Add(new SkinEntry(index++, skin));
				}
			}
		}
		RepositionElements();
		SetSkin(Math.Max(Skins.FindIndex((SkinEntry skin) => skin.Id == Building.skinId.Value), 0));
		populateClickableComponentList();
		if (Game1.options.SnappyMenus)
		{
			snapToDefaultClickableComponent();
		}
	}

	public override void snapToDefaultClickableComponent()
	{
		currentlySnappedComponent = getComponentWithID(101);
		snapCursorToCurrentSnappedComponent();
	}

	public override void receiveGamePadButton(Buttons b)
	{
		switch (b)
		{
		case Buttons.RightTrigger:
			Game1.playSound("shwip");
			SetSkin(Skin.Index + 1);
			break;
		case Buttons.LeftTrigger:
			Game1.playSound("shwip");
			SetSkin(Skin.Index - 1);
			break;
		}
		base.receiveGamePadButton(b);
	}

	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (OkButton.containsPoint(x, y))
		{
			exitThisMenu(playSound);
		}
		else if (PreviousSkinButton.containsPoint(x, y))
		{
			Game1.playSound("shwip");
			SetSkin(Skin.Index - 1);
		}
		else if (NextSkinButton.containsPoint(x, y))
		{
			SetSkin(Skin.Index + 1);
			Game1.playSound("shwip");
		}
		else
		{
			base.receiveLeftClick(x, y, playSound);
		}
	}

	public void SetSkin(int index)
	{
		if (Skins.Count == 0)
		{
			SetSkin(null);
			return;
		}
		index %= Skins.Count;
		if (index < 0)
		{
			index = Skins.Count + index;
		}
		SetSkin(Skins[index]);
	}

	public virtual void SetSkin(SkinEntry skin)
	{
		Skin = skin;
		if (Building.skinId.Value != skin.Id)
		{
			Building.skinId.Value = skin.Id;
			Building.netBuildingPaintColor.Value.Color1Default.Value = true;
			Building.netBuildingPaintColor.Value.Color2Default.Value = true;
			Building.netBuildingPaintColor.Value.Color3Default.Value = true;
			BuildingData buildingData = Building.GetData();
			if (buildingData != null && Building.daysOfConstructionLeft.Value == buildingData.BuildDays)
			{
				Building.daysOfConstructionLeft.Value = skin.Data?.BuildDays ?? buildingData.BuildDays;
			}
		}
	}

	public override bool overrideSnappyMenuCursorMovementBan()
	{
		return false;
	}

	public override bool readyToClose()
	{
		return true;
	}

	public override void performHoverAction(int x, int y)
	{
		OkButton.tryHover(x, y);
		PreviousSkinButton.tryHover(x, y);
		NextSkinButton.tryHover(x, y);
	}

	public virtual void RepositionElements()
	{
		PreviewPane.Y = yPositionOnScreen + 48;
		PreviewPane.Width = 576;
		PreviewPane.Height = 576;
		PreviewPane.X = xPositionOnScreen + width / 2 - PreviewPane.Width / 2;
		Rectangle panelRectangle = PreviewPane;
		panelRectangle.Inflate(-16, -16);
		PreviousSkinButton = new ClickableTextureComponent(new Rectangle(panelRectangle.Left, panelRectangle.Center.Y - 32, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
		{
			myID = 103,
			leftNeighborID = -99998,
			rightNeighborID = -99998,
			downNeighborID = 101,
			upNeighborID = -99998,
			fullyImmutable = true
		};
		NextSkinButton = new ClickableTextureComponent(new Rectangle(panelRectangle.Right - 64, panelRectangle.Center.Y - 32, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
		{
			myID = 102,
			leftNeighborID = -99998,
			rightNeighborID = -99998,
			downNeighborID = 101,
			upNeighborID = -99998,
			fullyImmutable = true
		};
		panelRectangle.Y += 64;
		panelRectangle.Height = 0;
		panelRectangle.Y += 80;
		panelRectangle.Y += 64;
		OkButton = new ClickableTextureComponent(new Rectangle(PreviewPane.Right - 64 - 16, PreviewPane.Bottom - 64 - 16, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
		{
			myID = 101,
			upNeighborID = 102
		};
		if (Skins.Count == 0)
		{
			NextSkinButton.visible = false;
			PreviousSkinButton.visible = false;
		}
		populateClickableComponentList();
	}

	public virtual bool SaveColor()
	{
		return true;
	}

	public virtual void SetRegion(int newRegion)
	{
		RepositionElements();
	}

	public override void draw(SpriteBatch b)
	{
		if (!Game1.options.showClearBackgrounds)
		{
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
		}
		Game1.DrawBox(PreviewPane.X, PreviewPane.Y, PreviewPane.Width, PreviewPane.Height);
		Rectangle rectangle = PreviewPane;
		rectangle.Inflate(0, 0);
		b.End();
		b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, Utility.ScissorEnabled);
		b.GraphicsDevice.ScissorRectangle = rectangle;
		Vector2 buildingDrawCenter = new Vector2(PreviewPane.X + PreviewPane.Width / 2, PreviewPane.Y + PreviewPane.Height / 2 - 16);
		Rectangle sourceRect = Building.getSourceRectForMenu() ?? Building.getSourceRect();
		Building?.drawInMenu(b, (int)buildingDrawCenter.X - (int)((float)(int)Building.tilesWide / 2f * 64f), (int)buildingDrawCenter.Y - sourceRect.Height * 4 / 2);
		b.End();
		b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
		SpriteText.drawStringWithScrollCenteredAt(b, Game1.content.LoadString("Strings\\Buildings:BuildingSkinMenu_ChooseAppearance", BuildingDisplayName), xPositionOnScreen + width / 2, PreviewPane.Top - 96);
		OkButton.draw(b);
		NextSkinButton.draw(b);
		PreviousSkinButton.draw(b);
		drawMouse(b);
	}
}
