using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;

namespace StardewValley.Buildings;

public class ShippingBin : Building
{
	private TemporaryAnimatedSprite shippingBinLid;

	private Farm farm;

	private Rectangle shippingBinLidOpenArea;

	protected Vector2 _lidGenerationPosition;

	public ShippingBin(Vector2 tileLocation)
		: base("Shipping Bin", tileLocation)
	{
		initLid();
	}

	public ShippingBin()
		: this(Vector2.Zero)
	{
	}

	public void initLid()
	{
		shippingBinLid = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(134, 226, 30, 25), new Vector2((int)tileX, (int)tileY - 1) * 64f + new Vector2(1f, -7f) * 4f, flipped: false, 0f, Color.White)
		{
			holdLastFrame = true,
			destroyable = false,
			interval = 20f,
			animationLength = 13,
			paused = true,
			scale = 4f,
			layerDepth = (float)(((int)tileY + 1) * 64) / 10000f + 0.0001f,
			pingPong = true,
			pingPongMotion = 0
		};
		shippingBinLidOpenArea = new Rectangle(((int)tileX - 1) * 64, ((int)tileY - 1) * 64, 256, 192);
		_lidGenerationPosition = new Vector2((int)tileX, (int)tileY);
	}

	public override Rectangle? getSourceRectForMenu()
	{
		return new Rectangle(0, 0, texture.Value.Bounds.Width, texture.Value.Bounds.Height);
	}

	public override void resetLocalState()
	{
		base.resetLocalState();
		if (shippingBinLid != null)
		{
			_ = shippingBinLidOpenArea;
		}
		else
		{
			initLid();
		}
	}

	public override void Update(GameTime time)
	{
		base.Update(time);
		if (farm == null)
		{
			farm = Game1.getFarm();
		}
		if (shippingBinLid != null)
		{
			_ = shippingBinLidOpenArea;
			if (_lidGenerationPosition.X == (float)(int)tileX && _lidGenerationPosition.Y == (float)(int)tileY)
			{
				bool opening = false;
				foreach (Farmer farmer in GetParentLocation().farmers)
				{
					if (farmer.GetBoundingBox().Intersects(shippingBinLidOpenArea))
					{
						openShippingBinLid();
						opening = true;
					}
				}
				if (!opening)
				{
					closeShippingBinLid();
				}
				updateShippingBinLid(time);
				return;
			}
		}
		initLid();
	}

	/// <inheritdoc />
	public override void performActionOnBuildingPlacement()
	{
		base.performActionOnBuildingPlacement();
		initLid();
	}

	private void openShippingBinLid()
	{
		if (shippingBinLid != null)
		{
			if (shippingBinLid.pingPongMotion != 1 && IsInCurrentLocation())
			{
				Game1.currentLocation.localSound("doorCreak");
			}
			shippingBinLid.pingPongMotion = 1;
			shippingBinLid.paused = false;
		}
	}

	private void closeShippingBinLid()
	{
		TemporaryAnimatedSprite temporaryAnimatedSprite = shippingBinLid;
		if (temporaryAnimatedSprite != null && temporaryAnimatedSprite.currentParentTileIndex > 0)
		{
			if (shippingBinLid.pingPongMotion != -1 && IsInCurrentLocation())
			{
				Game1.currentLocation.localSound("doorCreakReverse");
			}
			shippingBinLid.pingPongMotion = -1;
			shippingBinLid.paused = false;
		}
	}

	private void updateShippingBinLid(GameTime time)
	{
		if (isShippingBinLidOpen(requiredToBeFullyOpen: true) && shippingBinLid.pingPongMotion == 1)
		{
			shippingBinLid.paused = true;
		}
		else if (shippingBinLid.currentParentTileIndex == 0 && shippingBinLid.pingPongMotion == -1)
		{
			if (!shippingBinLid.paused && IsInCurrentLocation())
			{
				Game1.currentLocation.localSound("woodyStep");
			}
			shippingBinLid.paused = true;
		}
		shippingBinLid.update(time);
	}

	private bool isShippingBinLidOpen(bool requiredToBeFullyOpen = false)
	{
		if (shippingBinLid != null && shippingBinLid.currentParentTileIndex >= ((!requiredToBeFullyOpen) ? 1 : (shippingBinLid.animationLength - 1)))
		{
			return true;
		}
		return false;
	}

	private void shipItem(Item i, Farmer who)
	{
		if (i != null)
		{
			who.removeItemFromInventory(i);
			farm?.getShippingBin(who).Add(i);
			if (i is Object obj && farm != null)
			{
				showShipment(obj, playThrowSound: false);
			}
			farm.lastItemShipped = i;
			if (Game1.player.ActiveObject == null)
			{
				Game1.player.showNotCarrying();
				Game1.player.Halt();
			}
		}
	}

	public override bool CanLeftClick(int x, int y)
	{
		Rectangle hit_rect = new Rectangle((int)tileX * 64, (int)tileY * 64, (int)tilesWide * 64, (int)tilesHigh * 64);
		hit_rect.Y -= 64;
		hit_rect.Height += 64;
		return hit_rect.Contains(x, y);
	}

	public override bool leftClicked()
	{
		if (farm != null && Game1.player.ActiveObject != null && Game1.player.ActiveObject.canBeShipped() && Vector2.Distance(Game1.player.Tile, new Vector2((float)(int)tileX + 0.5f, (int)tileY)) <= 2f)
		{
			farm.getShippingBin(Game1.player).Add(Game1.player.ActiveObject);
			farm.lastItemShipped = Game1.player.ActiveObject;
			Game1.player.showNotCarrying();
			showShipment(Game1.player.ActiveObject);
			Game1.player.ActiveObject = null;
			return true;
		}
		return base.leftClicked();
	}

	public void showShipment(Object o, bool playThrowSound = true)
	{
		if (farm != null)
		{
			GameLocation parentLocation = GetParentLocation();
			if (playThrowSound)
			{
				parentLocation.localSound("backpackIN");
			}
			DelayedAction.playSoundAfterDelay("Ship", playThrowSound ? 250 : 0);
			int temp = Game1.random.Next();
			parentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(524, 218, 34, 22), new Vector2((int)tileX, (int)tileY - 1) * 64f + new Vector2(-1f, 5f) * 4f, flipped: false, 0f, Color.White)
			{
				interval = 100f,
				totalNumberOfLoops = 1,
				animationLength = 3,
				pingPong = true,
				alpha = alpha,
				scale = 4f,
				layerDepth = (float)(((int)tileY + 1) * 64) / 10000f + 0.0002f,
				id = temp,
				extraInfoForEndBehavior = temp,
				endFunction = parentLocation.removeTemporarySpritesWithID
			});
			parentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(524, 230, 34, 10), new Vector2((int)tileX, (int)tileY - 1) * 64f + new Vector2(-1f, 17f) * 4f, flipped: false, 0f, Color.White)
			{
				interval = 100f,
				totalNumberOfLoops = 1,
				animationLength = 3,
				pingPong = true,
				alpha = alpha,
				scale = 4f,
				layerDepth = (float)(((int)tileY + 1) * 64) / 10000f + 0.0003f,
				id = temp,
				extraInfoForEndBehavior = temp
			});
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(o.QualifiedItemId);
			parentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(itemData.TextureName, itemData.GetSourceRect(), new Vector2((int)tileX, (int)tileY - 1) * 64f + new Vector2(7 + Game1.random.Next(6), 2f) * 4f, flipped: false, 0f, Color.White)
			{
				interval = 9999f,
				scale = 4f,
				alphaFade = 0.045f,
				layerDepth = (float)(((int)tileY + 1) * 64) / 10000f + 0.000225f,
				motion = new Vector2(0f, 0.3f),
				acceleration = new Vector2(0f, 0.2f),
				scaleChange = -0.05f
			});
		}
	}

	public override bool doAction(Vector2 tileLocation, Farmer who)
	{
		if ((int)daysOfConstructionLeft <= 0 && tileLocation.X >= (float)(int)tileX && tileLocation.X <= (float)((int)tileX + 1) && tileLocation.Y == (float)(int)tileY)
		{
			if (!Game1.didPlayerJustRightClick(ignoreNonMouseHeldInput: true))
			{
				return false;
			}
			ItemGrabMenu itemGrabMenu = new ItemGrabMenu(null, reverseGrab: true, showReceivingMenu: false, Utility.highlightShippableObjects, shipItem, "", null, snapToBottom: true, canBeExitedWithKey: true, playRightClickSound: false, allowRightClick: true, showOrganizeButton: false, 0, null, -1, this);
			itemGrabMenu.initializeUpperRightCloseButton();
			itemGrabMenu.setBackgroundTransparency(b: false);
			itemGrabMenu.setDestroyItemOnClick(b: true);
			itemGrabMenu.initializeShippingBin();
			Game1.activeClickableMenu = itemGrabMenu;
			if (who.IsLocalPlayer)
			{
				Game1.playSound("shwip");
			}
			if (Game1.player.FacingDirection == 1)
			{
				Game1.player.Halt();
			}
			Game1.player.showCarrying();
			return true;
		}
		return base.doAction(tileLocation, who);
	}

	public override void drawInMenu(SpriteBatch b, int x, int y)
	{
		base.drawInMenu(b, x, y);
		b.Draw(Game1.mouseCursors, new Vector2(x + 4, y - 20), new Rectangle(134, 226, 30, 25), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
	}

	public override void draw(SpriteBatch b)
	{
		if (!base.isMoving)
		{
			base.draw(b);
			if (shippingBinLid != null && (int)daysOfConstructionLeft <= 0)
			{
				shippingBinLid.color = color;
				shippingBinLid.draw(b, localPosition: false, 0, 0, alpha * (((int)newConstructionTimer > 0) ? ((1000f - (float)(int)newConstructionTimer) / 1000f) : 1f));
			}
		}
	}
}
