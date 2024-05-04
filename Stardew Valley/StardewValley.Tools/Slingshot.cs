using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Projectiles;

namespace StardewValley.Tools;

public class Slingshot : Tool
{
	public const int basicDamage = 5;

	public const string basicSlingshotId = "32";

	public const string masterSlingshotId = "33";

	public const string galaxySlingshotId = "34";

	public const int drawBackSoundThreshold = 8;

	[XmlIgnore]
	public int lastClickX;

	[XmlIgnore]
	public int lastClickY;

	[XmlIgnore]
	public int mouseDragAmount;

	[XmlIgnore]
	public double pullStartTime = -1.0;

	[XmlIgnore]
	public float nextAutoFire = -1f;

	[XmlIgnore]
	public bool canPlaySound;

	[XmlIgnore]
	private readonly NetEvent0 finishEvent = new NetEvent0();

	[XmlIgnore]
	public readonly NetPoint aimPos = new NetPoint().Interpolated(interpolate: true, wait: true);

	/// <inheritdoc />
	public override string TypeDefinitionId { get; } = "(W)";


	public Slingshot()
		: this("32")
	{
	}

	/// <inheritdoc />
	protected override void MigrateLegacyItemId()
	{
		base.ItemId = base.InitialParentTileIndex.ToString();
	}

	/// <inheritdoc />
	protected override Item GetOneNew()
	{
		return new Slingshot(base.ItemId);
	}

	protected override string loadDisplayName()
	{
		return ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId).DisplayName;
	}

	protected override string loadDescription()
	{
		return ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId).Description;
	}

	public override bool doesShowTileLocationMarker()
	{
		return false;
	}

	public Slingshot(string itemId = "32")
	{
		itemId = ValidateUnqualifiedItemId(itemId);
		ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem("(W)" + itemId);
		base.ItemId = itemId;
		base.BaseName = itemData.InternalName;
		base.InitialParentTileIndex = itemData.SpriteIndex;
		base.CurrentParentTileIndex = itemData.SpriteIndex;
		base.IndexOfMenuItemView = itemData.SpriteIndex;
		numAttachmentSlots.Value = 1;
		attachments.SetCount(1);
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(finishEvent, "finishEvent").AddField(aimPos, "aimPos");
		finishEvent.onEvent += doFinish;
	}

	public int GetBackArmDistance(Farmer who)
	{
		if (CanAutoFire() && nextAutoFire > 0f)
		{
			return (int)Utility.Lerp(20f, 0f, nextAutoFire / GetAutoFireRate());
		}
		if (!Game1.options.useLegacySlingshotFiring)
		{
			return (int)(20f * GetSlingshotChargeTime());
		}
		return Math.Min(20, (int)Vector2.Distance(who.getStandingPosition(), new Vector2(aimPos.X, aimPos.Y)) / 20);
	}

	public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
	{
		base.IndexOfMenuItemView = base.InitialParentTileIndex;
		if (!CanAutoFire())
		{
			PerformFire(location, who);
		}
		finish();
	}

	public virtual void PerformFire(GameLocation location, Farmer who)
	{
		if (attachments[0] != null)
		{
			updateAimPos();
			int mouseX = aimPos.X;
			int mouseY = aimPos.Y;
			int backArmDistance = GetBackArmDistance(who);
			Vector2 shoot_origin = GetShootOrigin(who);
			Vector2 v = Utility.getVelocityTowardPoint(GetShootOrigin(who), AdjustForHeight(new Vector2(mouseX, mouseY)), (float)(15 + Game1.random.Next(4, 6)) * (1f + who.buffs.WeaponSpeedMultiplier));
			if (backArmDistance > 4 && !canPlaySound)
			{
				Object ammunition = (Object)attachments[0].getOne();
				attachments[0].Stack--;
				if (attachments[0].Stack <= 0)
				{
					attachments[0] = null;
				}
				string text = base.ItemId;
				float damageMod = ((text == "33") ? 2f : ((!(text == "34")) ? 1f : 4f));
				int damage = GetAmmoDamage(ammunition);
				string collisionSound = GetAmmoCollisionSound(ammunition);
				BasicProjectile.onCollisionBehavior collisionBehavior = GetAmmoCollisionBehavior(ammunition);
				if (!Game1.options.useLegacySlingshotFiring)
				{
					v.X *= -1f;
					v.Y *= -1f;
				}
				location.projectiles.Add(new BasicProjectile((int)(damageMod * (float)(damage + Game1.random.Next(-(damage / 2), damage + 2)) * (1f + who.buffs.AttackMultiplier)), -1, 0, 0, (float)(Math.PI / (double)(64f + (float)Game1.random.Next(-63, 64))), 0f - v.X, 0f - v.Y, shoot_origin - new Vector2(32f, 32f), collisionSound, null, null, explode: false, damagesMonsters: true, location, who, collisionBehavior, ammunition.ItemId)
				{
					IgnoreLocationCollision = (Game1.currentLocation.currentEvent != null || Game1.currentMinigame != null)
				});
			}
		}
		else
		{
			Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Slingshot.cs.14254"));
		}
		canPlaySound = true;
	}

	/// <summary>Get the damage inflicted by shooting an ammunition item, excluding the slingshot upgrade level multiplier.</summary>
	/// <param name="ammunition">The item that was shot.</param>
	public virtual int GetAmmoDamage(Object ammunition)
	{
		return ammunition?.QualifiedItemId switch
		{
			"(O)388" => 2, 
			"(O)390" => 5, 
			"(O)378" => 10, 
			"(O)380" => 20, 
			"(O)384" => 30, 
			"(O)382" => 15, 
			"(O)386" => 50, 
			"(O)441" => 20, 
			_ => 1, 
		};
	}

	/// <summary>Get the sound to play when the ammunition item hits.</summary>
	/// <param name="ammunition">The item that was shot.</param>
	public virtual string GetAmmoCollisionSound(Object ammunition)
	{
		if (ammunition?.QualifiedItemId == "(O)441")
		{
			return "explosion";
		}
		if (ammunition != null && ammunition.Category == -5)
		{
			return "slimedead";
		}
		return "hammer";
	}

	/// <summary>Get the logic to apply when the ammunition item hits.</summary>
	/// <param name="ammunition">The item that was shot.</param>
	public virtual BasicProjectile.onCollisionBehavior GetAmmoCollisionBehavior(Object ammunition)
	{
		if (ammunition.QualifiedItemId == "(O)441")
		{
			return BasicProjectile.explodeOnImpact;
		}
		return null;
	}

	public Vector2 GetShootOrigin(Farmer who)
	{
		return AdjustForHeight(who.getStandingPosition(), for_cursor: false);
	}

	public Vector2 AdjustForHeight(Vector2 position, bool for_cursor = true)
	{
		if (!Game1.options.useLegacySlingshotFiring && for_cursor)
		{
			return new Vector2(position.X, position.Y);
		}
		return new Vector2(position.X, position.Y - 32f - 8f);
	}

	public void finish()
	{
		finishEvent.Fire();
	}

	private void doFinish()
	{
		if (lastUser != null)
		{
			lastUser.usingSlingshot = false;
			lastUser.canReleaseTool = true;
			lastUser.UsingTool = false;
			lastUser.canMove = true;
			lastUser.Halt();
			if (lastUser == Game1.player && Game1.options.gamepadControls)
			{
				Game1.game1.controllerSlingshotSafeTime = 0.2f;
			}
		}
	}

	/// <inheritdoc />
	protected override bool canThisBeAttached(Object o, int slot)
	{
		switch (o.QualifiedItemId)
		{
		case "(O)378":
		case "(O)380":
		case "(O)382":
		case "(O)384":
		case "(O)386":
		case "(O)388":
		case "(O)390":
		case "(O)441":
			return true;
		default:
			if (!o.bigCraftable.Value)
			{
				if (o.Category != -5 && o.Category != -79)
				{
					return o.Category == -75;
				}
				return true;
			}
			return false;
		}
	}

	public override string getHoverBoxText(Item hoveredItem)
	{
		if (hoveredItem is Object obj && canThisBeAttached(obj))
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Slingshot.cs.14256", DisplayName, obj.DisplayName);
		}
		if (hoveredItem == null && attachments?[0] != null)
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Slingshot.cs.14258", attachments[0].DisplayName);
		}
		return null;
	}

	public override bool onRelease(GameLocation location, int x, int y, Farmer who)
	{
		DoFunction(location, x, y, 1, who);
		return true;
	}

	public override bool beginUsing(GameLocation location, int x, int y, Farmer who)
	{
		who.usingSlingshot = true;
		who.canReleaseTool = false;
		mouseDragAmount = 0;
		int offset = ((who.FacingDirection == 3 || who.FacingDirection == 1) ? 1 : ((who.FacingDirection == 0) ? 2 : 0));
		who.FarmerSprite.setCurrentFrame(42 + offset);
		if (!who.IsLocalPlayer)
		{
			return true;
		}
		Game1.oldMouseState = Game1.input.GetMouseState();
		Game1.lastMousePositionBeforeFade = Game1.getMousePosition();
		lastClickX = Game1.getOldMouseX() + Game1.viewport.X;
		lastClickY = Game1.getOldMouseY() + Game1.viewport.Y;
		pullStartTime = Game1.currentGameTime.TotalGameTime.TotalSeconds;
		if (CanAutoFire())
		{
			nextAutoFire = -1f;
		}
		updateAimPos();
		return true;
	}

	public virtual float GetAutoFireRate()
	{
		return 0.3f;
	}

	public virtual bool CanAutoFire()
	{
		return false;
	}

	private void updateAimPos()
	{
		if (lastUser == null || !lastUser.IsLocalPlayer)
		{
			return;
		}
		Point mousePos = Game1.getMousePosition();
		if (Game1.options.gamepadControls && !Game1.lastCursorMotionWasMouse)
		{
			Vector2 stick = Game1.oldPadState.ThumbSticks.Left;
			if (stick.Length() < 0.25f)
			{
				stick.X = 0f;
				stick.Y = 0f;
				if (Game1.oldPadState.DPad.Down == ButtonState.Pressed)
				{
					stick.Y = -1f;
				}
				else if (Game1.oldPadState.DPad.Up == ButtonState.Pressed)
				{
					stick.Y = 1f;
				}
				if (Game1.oldPadState.DPad.Left == ButtonState.Pressed)
				{
					stick.X = -1f;
				}
				if (Game1.oldPadState.DPad.Right == ButtonState.Pressed)
				{
					stick.X = 1f;
				}
				if (stick.X != 0f && stick.Y != 0f)
				{
					stick.Normalize();
					stick *= 1f;
				}
			}
			Vector2 shoot_origin = GetShootOrigin(lastUser);
			if (!Game1.options.useLegacySlingshotFiring && stick.Length() < 0.25f)
			{
				switch (lastUser.FacingDirection)
				{
				case 3:
					stick = new Vector2(-1f, 0f);
					break;
				case 1:
					stick = new Vector2(1f, 0f);
					break;
				case 0:
					stick = new Vector2(0f, 1f);
					break;
				case 2:
					stick = new Vector2(0f, -1f);
					break;
				}
			}
			mousePos = Utility.Vector2ToPoint(shoot_origin + new Vector2(stick.X, 0f - stick.Y) * 600f);
			mousePos.X -= Game1.viewport.X;
			mousePos.Y -= Game1.viewport.Y;
		}
		int mouseX = mousePos.X + Game1.viewport.X;
		int mouseY = mousePos.Y + Game1.viewport.Y;
		aimPos.X = mouseX;
		aimPos.Y = mouseY;
	}

	public override void tickUpdate(GameTime time, Farmer who)
	{
		lastUser = who;
		finishEvent.Poll();
		if (!who.usingSlingshot)
		{
			return;
		}
		if (who.IsLocalPlayer)
		{
			updateAimPos();
			int mouseX = aimPos.X;
			int mouseY = aimPos.Y;
			mouseDragAmount++;
			if (!Game1.options.useLegacySlingshotFiring)
			{
				Vector2 shoot_origin = GetShootOrigin(who);
				Vector2 aim_offset = AdjustForHeight(new Vector2(mouseX, mouseY)) - shoot_origin;
				if (Math.Abs(aim_offset.X) > Math.Abs(aim_offset.Y))
				{
					if (aim_offset.X < 0f)
					{
						who.faceDirection(3);
					}
					if (aim_offset.X > 0f)
					{
						who.faceDirection(1);
					}
				}
				else
				{
					if (aim_offset.Y < 0f)
					{
						who.faceDirection(0);
					}
					if (aim_offset.Y > 0f)
					{
						who.faceDirection(2);
					}
				}
			}
			else
			{
				who.faceGeneralDirection(new Vector2(mouseX, mouseY), 0, opposite: true);
			}
			if (!Game1.options.useLegacySlingshotFiring)
			{
				if (canPlaySound && GetSlingshotChargeTime() >= 1f)
				{
					who.playNearbySoundAll("slingshot");
					canPlaySound = false;
				}
			}
			else if (canPlaySound && (Math.Abs(mouseX - lastClickX) > 8 || Math.Abs(mouseY - lastClickY) > 8) && mouseDragAmount > 4)
			{
				who.playNearbySoundAll("slingshot");
				canPlaySound = false;
			}
			if (!CanAutoFire())
			{
				lastClickX = mouseX;
				lastClickY = mouseY;
			}
			if (Game1.options.useLegacySlingshotFiring)
			{
				Game1.mouseCursor = Game1.cursor_none;
			}
			if (CanAutoFire())
			{
				bool first_fire = false;
				if (GetBackArmDistance(who) >= 20 && nextAutoFire < 0f)
				{
					nextAutoFire = 0f;
					first_fire = true;
				}
				if (nextAutoFire > 0f || first_fire)
				{
					nextAutoFire -= (float)time.ElapsedGameTime.TotalSeconds;
					if (nextAutoFire <= 0f)
					{
						PerformFire(who.currentLocation, who);
						nextAutoFire = GetAutoFireRate();
					}
				}
			}
		}
		int offset = ((who.FacingDirection == 3 || who.FacingDirection == 1) ? 1 : ((who.FacingDirection == 0) ? 2 : 0));
		who.FarmerSprite.setCurrentFrame(42 + offset);
	}

	/// <inheritdoc />
	protected override void GetAttachmentSlotSprite(int slot, out Texture2D texture, out Rectangle sourceRect)
	{
		base.GetAttachmentSlotSprite(slot, out texture, out sourceRect);
		if (attachments[0] == null)
		{
			sourceRect = Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 43);
		}
	}

	public float GetSlingshotChargeTime()
	{
		if (pullStartTime < 0.0)
		{
			return 0f;
		}
		return Utility.Clamp((float)((Game1.currentGameTime.TotalGameTime.TotalSeconds - pullStartTime) / (double)GetRequiredChargeTime()), 0f, 1f);
	}

	public float GetRequiredChargeTime()
	{
		return 0.3f;
	}

	public override void draw(SpriteBatch b)
	{
		if (lastUser.usingSlingshot && lastUser.IsLocalPlayer)
		{
			int mouseX = aimPos.X;
			int mouseY = aimPos.Y;
			Vector2 shoot_origin = GetShootOrigin(lastUser);
			Vector2 v = Utility.getVelocityTowardPoint(shoot_origin, AdjustForHeight(new Vector2(mouseX, mouseY)), 256f);
			double distanceBetweenRadiusAndSquare = Math.Sqrt(v.X * v.X + v.Y * v.Y) - 181.0;
			double xPercent = v.X / 256f;
			double yPercent = v.Y / 256f;
			int x = (int)((double)v.X - distanceBetweenRadiusAndSquare * xPercent);
			int y = (int)((double)v.Y - distanceBetweenRadiusAndSquare * yPercent);
			if (!Game1.options.useLegacySlingshotFiring)
			{
				x *= -1;
				y *= -1;
			}
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(shoot_origin.X - (float)x, shoot_origin.Y - (float)y)), Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 43), Color.White, 0f, new Vector2(32f, 32f), 1f, SpriteEffects.None, 0.999999f);
		}
	}

	public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
	{
		AdjustMenuDrawForRecipes(ref transparency, ref scaleSize);
		if (base.IndexOfMenuItemView == 0 || base.IndexOfMenuItemView == 21 || base.ItemId == "47")
		{
			switch (base.BaseName)
			{
			case "Slingshot":
				base.CurrentParentTileIndex = int.Parse("32");
				break;
			case "Master Slingshot":
				base.CurrentParentTileIndex = int.Parse("33");
				break;
			case "Galaxy Slingshot":
				base.CurrentParentTileIndex = int.Parse("34");
				break;
			}
			base.IndexOfMenuItemView = base.CurrentParentTileIndex;
		}
		spriteBatch.Draw(Tool.weaponsTexture, location + new Vector2(32f, 29f), Game1.getSourceRectForStandardTileSheet(Tool.weaponsTexture, base.IndexOfMenuItemView, 16, 16), color * transparency, 0f, new Vector2(8f, 8f), scaleSize * 4f, SpriteEffects.None, layerDepth);
		if (drawStackNumber != 0 && attachments?[0] != null)
		{
			Utility.drawTinyDigits(attachments[0].Stack, spriteBatch, location + new Vector2((float)(64 - Utility.getWidthOfTinyDigitString(attachments[0].Stack, 3f * scaleSize)) + 3f * scaleSize, 64f - 18f * scaleSize + 2f), 3f * scaleSize, 1f, Color.White);
		}
		DrawMenuIcons(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color);
	}
}
