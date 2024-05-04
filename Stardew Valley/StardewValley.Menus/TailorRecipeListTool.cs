using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.GameData.Crafting;
using StardewValley.Objects;

namespace StardewValley.Menus;

public class TailorRecipeListTool : IClickableMenu
{
	public Rectangle scrollView;

	public List<ClickableTextureComponent> recipeComponents = new List<ClickableTextureComponent>();

	public ClickableTextureComponent okButton;

	public float scrollY;

	public Dictionary<string, KeyValuePair<Item, Item>> _recipeLookup = new Dictionary<string, KeyValuePair<Item, Item>>();

	public Item hoveredItem;

	public string hoverText = "";

	public Dictionary<string, string> _recipeHoverTexts = new Dictionary<string, string>();

	public Dictionary<string, string> _recipeOutputIds = new Dictionary<string, string>();

	public Dictionary<string, Color> _recipeColors = new Dictionary<string, Color>();

	public TailorRecipeListTool()
		: base(Game1.uiViewport.Width / 2 - (632 + IClickableMenu.borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2 - 64, 632 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2 + 64)
	{
		TailoringMenu tailoring_menu = new TailoringMenu();
		Game1.player.faceDirection(2);
		Game1.player.FarmerSprite.StopAnimation();
		Item cloth = ItemRegistry.Create<Object>("(O)428");
		foreach (string allId in ItemRegistry.GetObjectTypeDefinition().GetAllIds())
		{
			Object key = new Object(allId, 1);
			if (key.Name.Contains("Seeds") || key.Name.Contains("Floor") || key.Name.Equals("Lumber") || key.Name.Contains("Fence") || key.Name.Equals("Gate") || key.Name.Contains("Starter") || key.Name.Equals("Secret Note") || key.Name.Contains("Guide") || key.Name.Contains("Path") || key.Name.Contains("Ring") || (int)key.category == -22 || key.Category == -999 || key.isSapling())
			{
				continue;
			}
			Item value = tailoring_menu.CraftItem(cloth, key);
			TailorItemRecipe recipe = tailoring_menu.GetRecipeForItems(cloth, key);
			KeyValuePair<Item, Item> kvp = new KeyValuePair<Item, Item>(key, value);
			_recipeLookup[Utility.getStandardDescriptionFromItem(key, 1)] = kvp;
			string metadata = "";
			Color? dye_color = TailoringMenu.GetDyeColor(key);
			if (dye_color.HasValue)
			{
				_recipeColors[Utility.getStandardDescriptionFromItem(key, 1)] = dye_color.Value;
			}
			if (recipe != null)
			{
				metadata = "clothes id: " + recipe.CraftedItemId + " from ";
				foreach (string context_tag in recipe.SecondItemTags)
				{
					metadata = metadata + context_tag + " ";
				}
				metadata.Trim();
			}
			_recipeOutputIds[Utility.getStandardDescriptionFromItem(key, 1)] = TailoringMenu.ConvertLegacyItemId(recipe?.CraftedItemId) ?? value.QualifiedItemId;
			_recipeHoverTexts[Utility.getStandardDescriptionFromItem(key, 1)] = metadata;
			ClickableTextureComponent component = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), null, default(Rectangle), 1f)
			{
				myID = 0,
				name = Utility.getStandardDescriptionFromItem(key, 1),
				label = key.DisplayName
			};
			recipeComponents.Add(component);
		}
		okButton = new ClickableTextureComponent("OK", new Rectangle(xPositionOnScreen + width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 64, yPositionOnScreen + height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 16, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
		{
			upNeighborID = -99998,
			leftNeighborID = -99998,
			rightNeighborID = -99998,
			downNeighborID = -99998
		};
		RepositionElements();
	}

	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
	{
		base.gameWindowSizeChanged(oldBounds, newBounds);
		xPositionOnScreen = Game1.uiViewport.Width / 2 - (632 + IClickableMenu.borderWidth * 2) / 2;
		yPositionOnScreen = Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2 - 64;
		RepositionElements();
	}

	private void RepositionElements()
	{
		scrollView = new Rectangle(xPositionOnScreen + IClickableMenu.borderWidth, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder, width - IClickableMenu.borderWidth, 500);
		if (scrollView.Left < Game1.graphics.GraphicsDevice.ScissorRectangle.Left)
		{
			int size_difference = Game1.graphics.GraphicsDevice.ScissorRectangle.Left - scrollView.Left;
			scrollView.X += size_difference;
			scrollView.Width -= size_difference;
		}
		if (scrollView.Right > Game1.graphics.GraphicsDevice.ScissorRectangle.Right)
		{
			int size_difference = scrollView.Right - Game1.graphics.GraphicsDevice.ScissorRectangle.Right;
			scrollView.X -= size_difference;
			scrollView.Width -= size_difference;
		}
		if (scrollView.Top < Game1.graphics.GraphicsDevice.ScissorRectangle.Top)
		{
			int size_difference = Game1.graphics.GraphicsDevice.ScissorRectangle.Top - scrollView.Top;
			scrollView.Y += size_difference;
			scrollView.Width -= size_difference;
		}
		if (scrollView.Bottom > Game1.graphics.GraphicsDevice.ScissorRectangle.Bottom)
		{
			int size_difference = scrollView.Bottom - Game1.graphics.GraphicsDevice.ScissorRectangle.Bottom;
			scrollView.Y -= size_difference;
			scrollView.Width -= size_difference;
		}
		RepositionScrollElements();
	}

	public void RepositionScrollElements()
	{
		int y_offset = (int)scrollY;
		if (scrollY > 0f)
		{
			scrollY = 0f;
		}
		foreach (ClickableTextureComponent component in recipeComponents)
		{
			component.bounds.X = scrollView.X;
			component.bounds.Y = scrollView.Y + y_offset;
			y_offset += component.bounds.Height;
			if (scrollView.Intersects(component.bounds))
			{
				component.visible = true;
			}
			else
			{
				component.visible = false;
			}
		}
	}

	public override void snapToDefaultClickableComponent()
	{
		snapCursorToCurrentSnappedComponent();
	}

	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		foreach (ClickableTextureComponent component in recipeComponents)
		{
			if (!component.bounds.Contains(x, y) || !scrollView.Contains(x, y))
			{
				continue;
			}
			try
			{
				Item item = ItemRegistry.Create(_recipeOutputIds[component.name]);
				if (item is Clothing clothing && _recipeColors.TryGetValue(component.name, out var color))
				{
					clothing.Dye(color, 1f);
				}
				Game1.player.addItemToInventoryBool(item);
			}
			catch (Exception)
			{
			}
		}
		if (okButton.containsPoint(x, y))
		{
			exitThisMenu();
		}
	}

	public override void leftClickHeld(int x, int y)
	{
	}

	public override void releaseLeftClick(int x, int y)
	{
	}

	public override void receiveRightClick(int x, int y, bool playSound = true)
	{
	}

	public override void receiveKeyPress(Keys key)
	{
	}

	public override void receiveScrollWheelAction(int direction)
	{
		scrollY += direction;
		RepositionScrollElements();
		base.receiveScrollWheelAction(direction);
	}

	public override void performHoverAction(int x, int y)
	{
		hoveredItem = null;
		hoverText = "";
		foreach (ClickableTextureComponent component in recipeComponents)
		{
			if (component.containsPoint(x, y))
			{
				hoveredItem = _recipeLookup[component.name].Value;
				hoverText = _recipeHoverTexts[component.name];
			}
		}
	}

	public bool canLeaveMenu()
	{
		return true;
	}

	public override void draw(SpriteBatch b)
	{
		Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, speaker: false, drawOnlyBox: true);
		b.End();
		Rectangle cached_scissor_rect = b.GraphicsDevice.ScissorRectangle;
		b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, Utility.ScissorEnabled);
		b.GraphicsDevice.ScissorRectangle = scrollView;
		foreach (ClickableTextureComponent component in recipeComponents)
		{
			if (component.visible)
			{
				drawHorizontalPartition(b, component.bounds.Bottom - 32, small: true);
				KeyValuePair<Item, Item> kvp = _recipeLookup[component.name];
				component.draw(b);
				kvp.Key.drawInMenu(b, new Vector2(component.bounds.X, component.bounds.Y), 1f);
				if (_recipeColors.TryGetValue(component.name, out var color))
				{
					int size = 24;
					b.Draw(Game1.staminaRect, new Rectangle(scrollView.Left + scrollView.Width / 2 - size / 2, component.bounds.Center.Y - size / 2, size, size), color);
				}
				kvp.Value?.drawInMenu(b, new Vector2(scrollView.Left + scrollView.Width - 128, component.bounds.Y), 1f);
			}
		}
		b.End();
		b.GraphicsDevice.ScissorRectangle = cached_scissor_rect;
		b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
		okButton.draw(b);
		drawMouse(b);
		if (hoveredItem != null)
		{
			Utility.drawTextWithShadow(b, hoverText, Game1.smallFont, new Vector2(xPositionOnScreen + IClickableMenu.borderWidth, yPositionOnScreen + height - 64), Color.Black);
			if (!Game1.oldKBState.IsKeyDown(Keys.LeftShift))
			{
				IClickableMenu.drawToolTip(b, hoveredItem.getDescription(), hoveredItem.DisplayName, hoveredItem);
			}
		}
	}

	public override void update(GameTime time)
	{
	}
}
