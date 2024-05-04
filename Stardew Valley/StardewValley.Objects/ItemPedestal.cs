using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Network;

namespace StardewValley.Objects;

public class ItemPedestal : Object
{
	[XmlIgnore]
	public NetMutex itemModifyMutex = new NetMutex();

	[XmlElement("requiredItem")]
	public NetRef<Object> requiredItem = new NetRef<Object>();

	[XmlElement("successColor")]
	public NetColor successColor = new NetColor();

	[XmlElement("lockOnSuccess")]
	public NetBool lockOnSuccess = new NetBool();

	[XmlElement("locked")]
	public NetBool locked = new NetBool();

	[XmlElement("match")]
	public NetBool match = new NetBool();

	/// <summary>Whether this is a pedestal at the Ginger Island shrine, which can't be destroyed or picked up.</summary>
	[XmlElement("isIslandShrinePedestal")]
	public readonly NetBool isIslandShrinePedestal = new NetBool();

	[XmlIgnore]
	public Texture2D texture;

	/// <inheritdoc />
	public override string TypeDefinitionId => "(BC)";

	/// <inheritdoc />
	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(itemModifyMutex.NetFields, "itemModifyMutex.NetFields").AddField(requiredItem, "requiredItem").AddField(successColor, "successColor")
			.AddField(lockOnSuccess, "lockOnSuccess")
			.AddField(locked, "locked")
			.AddField(match, "match")
			.AddField(isIslandShrinePedestal, "isIslandShrinePedestal");
		heldObject.InterpolationWait = false;
	}

	public ItemPedestal()
	{
	}

	public ItemPedestal(Vector2 tile, Object required_item, bool lock_on_success, Color success_color, string itemId = "221")
		: base(tile, itemId)
	{
		requiredItem.Value = required_item;
		lockOnSuccess.Value = lock_on_success;
		successColor.Value = success_color;
	}

	/// <inheritdoc />
	protected override Item GetOneNew()
	{
		return new ItemPedestal(TileLocation, (Object)(requiredItem.Value?.getOne()), lockOnSuccess.Value, successColor.Value, base.ItemId);
	}

	/// <inheritdoc />
	protected override void GetOneCopyFrom(Item source)
	{
		base.GetOneCopyFrom(source);
		if (source is ItemPedestal fromPedestal)
		{
			isIslandShrinePedestal.Value = fromPedestal.isIslandShrinePedestal.Value;
		}
	}

	/// <inheritdoc />
	public override bool performObjectDropInAction(Item dropInItem, bool probe, Farmer who, bool returnFalseIfItemConsumed = false)
	{
		GameLocation location = Location;
		if (location == null)
		{
			return false;
		}
		if (locked.Value)
		{
			return false;
		}
		if (!dropInItem.canBeTrashed())
		{
			return false;
		}
		if (heldObject.Value != null && !probe)
		{
			DropObject(who);
			return false;
		}
		if (dropInItem.GetType() == typeof(Object))
		{
			if (!probe)
			{
				Object placed_object = dropInItem.getOne() as Object;
				itemModifyMutex.RequestLock(delegate
				{
					location.playSound("woodyStep");
					heldObject.Value = placed_object;
					UpdateItemMatch();
					itemModifyMutex.ReleaseLock();
				}, delegate
				{
					if (placed_object != heldObject.Value)
					{
						Game1.createItemDebris(placed_object, (TileLocation + new Vector2(0.5f, 0.5f)) * 64f, -1, location);
					}
				});
			}
			return true;
		}
		return false;
	}

	public virtual void UpdateItemMatch()
	{
		bool success = false;
		if (heldObject.Value != null && requiredItem.Value != null && Utility.getStandardDescriptionFromItem(heldObject.Value, 1) == Utility.getStandardDescriptionFromItem(requiredItem.Value, 1))
		{
			success = true;
		}
		if (success != match.Value)
		{
			match.Value = success;
			if (match.Value && lockOnSuccess.Value)
			{
				locked.Value = true;
			}
		}
	}

	/// <inheritdoc />
	public override bool checkForAction(Farmer who, bool checking_for_activity = false)
	{
		if (locked.Value)
		{
			return false;
		}
		if (checking_for_activity)
		{
			return true;
		}
		if (DropObject(who))
		{
			return true;
		}
		return false;
	}

	public bool DropObject(Farmer who)
	{
		if (heldObject.Value != null)
		{
			itemModifyMutex.RequestLock(delegate
			{
				Object value = heldObject.Value;
				heldObject.Value = null;
				if (who.addItemToInventoryBool(value))
				{
					value.performRemoveAction();
					Game1.playSound("coin");
				}
				else
				{
					heldObject.Value = value;
				}
				UpdateItemMatch();
				itemModifyMutex.ReleaseLock();
			});
			return true;
		}
		return false;
	}

	public override bool performToolAction(Tool t)
	{
		if (isIslandShrinePedestal.Value)
		{
			return false;
		}
		return base.performToolAction(t);
	}

	public override void updateWhenCurrentLocation(GameTime time)
	{
		GameLocation location = Location;
		if (location != null)
		{
			itemModifyMutex.Update(location);
		}
	}

	public override bool onExplosion(Farmer who)
	{
		if (isIslandShrinePedestal.Value)
		{
			return false;
		}
		return base.onExplosion(who);
	}

	public override void DayUpdate()
	{
		base.DayUpdate();
		itemModifyMutex.ReleaseLock();
	}

	public override void draw(SpriteBatch b, int x, int y, float alpha = 1f)
	{
		Vector2 position = new Vector2(x * 64, y * 64);
		ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
		b.Draw(Game1.bigCraftableSpriteSheet, Game1.GlobalToLocal(Game1.viewport, position), itemData.GetSourceRect(), Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, Math.Max(0f, (position.Y - 2f) / 10000f));
		if (match.Value)
		{
			b.Draw(Game1.bigCraftableSpriteSheet, Game1.GlobalToLocal(Game1.viewport, position), itemData.GetSourceRect(1), successColor.Value, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, Math.Max(0f, (position.Y - 1f) / 10000f));
		}
		if (heldObject.Value != null)
		{
			Vector2 draw_position = new Vector2(x, y);
			if (heldObject.Value.bigCraftable.Value)
			{
				draw_position.Y -= 1f;
			}
			heldObject.Value.draw(b, (int)draw_position.X * 64, (int)((draw_position.Y - 0.2f) * 64f) - 64, position.Y / 10000f, 1f);
		}
	}
}
