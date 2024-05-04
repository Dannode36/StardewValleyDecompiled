using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;
using StardewValley.TerrainFeatures;

namespace StardewValley.Objects;

public class IndoorPot : Object
{
	[XmlElement("hoeDirt")]
	public readonly NetRef<HoeDirt> hoeDirt = new NetRef<HoeDirt>();

	[XmlElement("bush")]
	public readonly NetRef<Bush> bush = new NetRef<Bush>();

	[XmlIgnore]
	public readonly NetBool bushLoadDirty = new NetBool(value: true);

	/// <inheritdoc />
	public override string TypeDefinitionId => "(BC)";

	/// <inheritdoc />
	[XmlIgnore]
	public override GameLocation Location
	{
		get
		{
			return base.Location;
		}
		set
		{
			if (hoeDirt.Value != null)
			{
				hoeDirt.Value.Location = value;
			}
			if (bush.Value != null)
			{
				bush.Value.Location = value;
			}
			base.Location = value;
		}
	}

	/// <inheritdoc />
	public override Vector2 TileLocation
	{
		get
		{
			return base.TileLocation;
		}
		set
		{
			if (hoeDirt.Value != null)
			{
				hoeDirt.Value.Tile = value;
			}
			if (bush.Value != null)
			{
				bush.Value.Tile = value;
			}
			base.TileLocation = value;
		}
	}

	/// <inheritdoc />
	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(hoeDirt, "hoeDirt").AddField(bush, "bush").AddField(bushLoadDirty, "bushLoadDirty");
		bush.fieldChangeEvent += delegate(NetRef<Bush> field, Bush value, Bush newValue)
		{
			if (newValue != null)
			{
				newValue.Location = Location;
				newValue.inPot.Value = true;
			}
		};
	}

	public IndoorPot()
	{
	}

	public IndoorPot(Vector2 tileLocation)
		: base(tileLocation, "62")
	{
		GameLocation location = (Location = Game1.currentLocation);
		hoeDirt.Value = new HoeDirt(0, location);
		if (location.IsRainingHere() && (bool)location.isOutdoors)
		{
			Water();
		}
	}

	public override void DayUpdate()
	{
		base.DayUpdate();
		hoeDirt.Value.dayUpdate();
		showNextIndex.Value = hoeDirt.Value.isWatered();
		GameLocation location = Location;
		if ((bool)location.isOutdoors && location.IsRainingHere())
		{
			Water();
		}
		if (heldObject.Value != null)
		{
			readyForHarvest.Value = true;
		}
		bush.Value?.dayUpdate();
	}

	/// <summary>Water the dirt in this garden pot.</summary>
	public void Water()
	{
		hoeDirt.Value.state.Value = 1;
		showNextIndex.Value = true;
	}

	/// <summary>Get whether an item type can be planted in indoor pots, regardless of whether the pot has room currently.</summary>
	/// <param name="item">The item to check.</param>
	public bool IsPlantableItem(Item item)
	{
		if (item.HasTypeObject())
		{
			string qualifiedItemId = item.QualifiedItemId;
			if (qualifiedItemId == "(O)499" || qualifiedItemId == "(O)805")
			{
				return false;
			}
			if (item.Category == -19)
			{
				return true;
			}
			string cropItemId = Crop.ResolveSeedId(item.ItemId, Location);
			if (Game1.cropData.ContainsKey(cropItemId))
			{
				return true;
			}
			if (item is Object obj && obj.IsTeaSapling())
			{
				return true;
			}
		}
		return false;
	}

	/// <inheritdoc />
	public override bool performObjectDropInAction(Item dropInItem, bool probe, Farmer who, bool returnFalseIfItemConsumed = false)
	{
		if (who != null && dropInItem != null && bush.Value == null)
		{
			if (hoeDirt.Value.canPlantThisSeedHere(dropInItem.ItemId, dropInItem.Category == -19))
			{
				if (dropInItem.QualifiedItemId == "(O)805")
				{
					if (!probe)
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"));
					}
					return false;
				}
				if (!probe)
				{
					return hoeDirt.Value.plant(dropInItem.ItemId, who, dropInItem.Category == -19);
				}
				return true;
			}
			if (hoeDirt.Value.crop == null && dropInItem.QualifiedItemId == "(O)251")
			{
				if (!probe)
				{
					NetRef<Bush> netRef = bush;
					Bush obj = new Bush(tileLocation.Value, 3, Location);
					obj.inPot.Value = true;
					netRef.Value = obj;
					if (!Location.IsOutdoors)
					{
						bush.Value.loadSprite();
						Game1.playSound("coin");
					}
				}
				return true;
			}
		}
		return false;
	}

	public override bool performToolAction(Tool t)
	{
		if (t != null)
		{
			hoeDirt.Value.performToolAction(t, -1, tileLocation.Value);
			if (bush.Value != null)
			{
				if (bush.Value.performToolAction(t, -1, tileLocation.Value))
				{
					bush.Value = null;
				}
				return false;
			}
		}
		if (hoeDirt.Value.isWatered())
		{
			Water();
		}
		return base.performToolAction(t);
	}

	/// <inheritdoc />
	public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
	{
		if (who != null)
		{
			if (justCheckingForActivity)
			{
				if (!hoeDirt.Value.readyForHarvest() && heldObject.Value == null)
				{
					if (bush.Value != null)
					{
						return bush.Value.inBloom();
					}
					return false;
				}
				return true;
			}
			if (who.isMoving())
			{
				Game1.haltAfterCheck = false;
			}
			if (heldObject.Value != null)
			{
				bool num = who.addItemToInventoryBool(heldObject.Value);
				if (num)
				{
					heldObject.Value = null;
					readyForHarvest.Value = false;
					Game1.playSound("coin");
				}
				return num;
			}
			bool b = hoeDirt.Value.performUseAction(tileLocation.Value);
			if (b)
			{
				return b;
			}
			if ((int)hoeDirt.Value.crop?.currentPhase > 0 && hoeDirt.Value.getMaxShake() == 0f)
			{
				hoeDirt.Value.shake((float)Math.PI / 32f, (float)Math.PI / 50f, Game1.random.NextBool());
				DelayedAction.playSoundAfterDelay("leafrustle", Game1.random.Next(100));
			}
			bush.Value?.performUseAction(tileLocation.Value);
		}
		return false;
	}

	/// <inheritdoc />
	public override void actionOnPlayerEntry()
	{
		base.actionOnPlayerEntry();
		hoeDirt.Value?.performPlayerEntryAction();
	}

	public override void updateWhenCurrentLocation(GameTime time)
	{
		base.updateWhenCurrentLocation(time);
		if (Location != null)
		{
			hoeDirt.Value.tickUpdate(time);
			bush.Value?.tickUpdate(time);
			if ((bool)bushLoadDirty)
			{
				bush.Value?.loadSprite();
				bushLoadDirty.Value = false;
			}
		}
	}

	public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
	{
		Vector2 scaleFactor = getScale();
		scaleFactor *= 4f;
		Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64));
		Rectangle destination = new Rectangle((int)(position.X - scaleFactor.X / 2f) + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scaleFactor.Y / 2f) + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
		ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
		spriteBatch.Draw(itemData.GetTexture(), destination, itemData.GetSourceRect(showNextIndex ? 1 : 0), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f);
		if (hoeDirt.Value.HasFertilizer())
		{
			Rectangle fertilizer_rect = hoeDirt.Value.GetFertilizerSourceRect();
			fertilizer_rect.Width = 13;
			fertilizer_rect.Height = 13;
			spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + 4f, tileLocation.Y * 64f - 12f)), fertilizer_rect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (tileLocation.Y + 0.65f) * 64f / 10000f + (float)x * 1E-05f);
		}
		hoeDirt.Value.crop?.drawWithOffset(spriteBatch, tileLocation.Value, (hoeDirt.Value.isWatered() && (int)hoeDirt.Value.crop.currentPhase == 0 && !hoeDirt.Value.crop.raisedSeeds) ? (new Color(180, 100, 200) * 1f) : Color.White, hoeDirt.Value.getShakeRotation(), new Vector2(32f, 8f));
		heldObject.Value?.draw(spriteBatch, x * 64, y * 64 - 48, (tileLocation.Y + 0.66f) * 64f / 10000f + (float)x * 1E-05f, 1f);
		bush.Value?.draw(spriteBatch, -24f);
	}
}
