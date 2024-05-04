using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Buffs;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using StardewValley.Monsters;

namespace StardewValley.Objects;

public class CombinedRing : Ring
{
	public NetList<Ring, NetRef<Ring>> combinedRings = new NetList<Ring, NetRef<Ring>>();

	public CombinedRing()
		: base("880")
	{
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(combinedRings, "combinedRings");
		combinedRings.OnElementChanged += delegate
		{
			OnCombinedRingsChanged();
		};
		combinedRings.OnArrayReplaced += delegate
		{
			OnCombinedRingsChanged();
		};
	}

	protected override bool loadDisplayFields()
	{
		base.loadDisplayFields();
		description = "";
		foreach (Ring ring in combinedRings)
		{
			ring.getDescription();
			description = description + ring.description + "\n\n";
		}
		description = description.Trim();
		return true;
	}

	public override bool GetsEffectOfRing(string ringId)
	{
		foreach (Ring combinedRing in combinedRings)
		{
			if (combinedRing.GetsEffectOfRing(ringId))
			{
				return true;
			}
		}
		return base.GetsEffectOfRing(ringId);
	}

	/// <inheritdoc />
	protected override Item GetOneNew()
	{
		return new CombinedRing();
	}

	/// <inheritdoc />
	protected override void GetOneCopyFrom(Item source)
	{
		base.GetOneCopyFrom(source);
		if (!(source is CombinedRing fromRing))
		{
			return;
		}
		combinedRings.Clear();
		foreach (Ring combinedRing in fromRing.combinedRings)
		{
			Ring ring = (Ring)combinedRing.getOne();
			combinedRings.Add(ring);
		}
	}

	public override int GetEffectsOfRingMultiplier(string ringId)
	{
		int count = 0;
		foreach (Ring ring in combinedRings)
		{
			count += ring.GetEffectsOfRingMultiplier(ringId);
		}
		return count;
	}

	/// <inheritdoc />
	public override void onEquip(Farmer who)
	{
		foreach (Ring combinedRing in combinedRings)
		{
			combinedRing.onEquip(who);
		}
		base.onEquip(who);
	}

	/// <inheritdoc />
	public override void onUnequip(Farmer who)
	{
		foreach (Ring combinedRing in combinedRings)
		{
			combinedRing.onUnequip(who);
		}
		base.onUnequip(who);
	}

	public override void AddEquipmentEffects(BuffEffects effects)
	{
		base.AddEquipmentEffects(effects);
		foreach (Ring combinedRing in combinedRings)
		{
			combinedRing.AddEquipmentEffects(effects);
		}
	}

	public override void onLeaveLocation(Farmer who, GameLocation environment)
	{
		foreach (Ring combinedRing in combinedRings)
		{
			combinedRing.onLeaveLocation(who, environment);
		}
		base.onLeaveLocation(who, environment);
	}

	/// <inheritdoc />
	public override void onMonsterSlay(Monster m, GameLocation location, Farmer who)
	{
		foreach (Ring combinedRing in combinedRings)
		{
			combinedRing.onMonsterSlay(m, location, who);
		}
		base.onMonsterSlay(m, location, who);
	}

	public override void onNewLocation(Farmer who, GameLocation environment)
	{
		foreach (Ring combinedRing in combinedRings)
		{
			combinedRing.onNewLocation(who, environment);
		}
		base.onNewLocation(who, environment);
	}

	public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
	{
		if (combinedRings.Count >= 2)
		{
			AdjustMenuDrawForRecipes(ref transparency, ref scaleSize);
			float oldScaleSize = scaleSize;
			scaleSize = 1f;
			location.Y -= (oldScaleSize - 1f) * 32f;
			ParsedItemData dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(combinedRings[0].QualifiedItemId);
			Texture2D texture = dataOrErrorItem.GetTexture();
			Rectangle src = dataOrErrorItem.GetSourceRect().Clone();
			src.X += 5;
			src.Y += 7;
			src.Width = 4;
			src.Height = 6;
			spriteBatch.Draw(texture, location + new Vector2(51f, 51f) * scaleSize + new Vector2(-12f, 8f) * scaleSize, src, color * transparency, 0f, new Vector2(1.5f, 2f) * 4f * scaleSize, scaleSize * 4f, SpriteEffects.None, layerDepth);
			src.X++;
			src.Y += 4;
			src.Width = 3;
			src.Height = 1;
			spriteBatch.Draw(texture, location + new Vector2(51f, 51f) * scaleSize + new Vector2(-8f, 4f) * scaleSize, src, color * transparency, 0f, new Vector2(1.5f, 2f) * 4f * scaleSize, scaleSize * 4f, SpriteEffects.None, layerDepth);
			ParsedItemData dataOrErrorItem2 = ItemRegistry.GetDataOrErrorItem(combinedRings[1].QualifiedItemId);
			texture = dataOrErrorItem2.GetTexture();
			src = dataOrErrorItem2.GetSourceRect().Clone();
			src.X += 9;
			src.Y += 7;
			src.Width = 4;
			src.Height = 6;
			spriteBatch.Draw(texture, location + new Vector2(51f, 51f) * scaleSize + new Vector2(4f, 8f) * scaleSize, src, color * transparency, 0f, new Vector2(1.5f, 2f) * 4f * scaleSize, scaleSize * 4f, SpriteEffects.None, layerDepth);
			src.Y += 4;
			src.Width = 3;
			src.Height = 1;
			spriteBatch.Draw(texture, location + new Vector2(51f, 51f) * scaleSize + new Vector2(4f, 4f) * scaleSize, src, color * transparency, 0f, new Vector2(1.5f, 2f) * 4f * scaleSize, scaleSize * 4f, SpriteEffects.None, layerDepth);
			Color? color1 = TailoringMenu.GetDyeColor(combinedRings[0]);
			Color? color2 = TailoringMenu.GetDyeColor(combinedRings[1]);
			Color color1noNull = Color.Red;
			Color color2noNull = Color.Blue;
			if (color1.HasValue)
			{
				color1noNull = color1.Value;
			}
			if (color2.HasValue)
			{
				color2noNull = color2.Value;
			}
			base.drawInMenu(spriteBatch, location + new Vector2(-5f, -1f), scaleSize, transparency, layerDepth, drawStackNumber, Utility.Get2PhaseColor(color1noNull, color2noNull), drawShadow);
			spriteBatch.Draw(Game1.objectSpriteSheet, location + new Vector2(13f, 35f) * scaleSize, new Rectangle(263, 579, 4, 2), Utility.Get2PhaseColor(color1noNull, color2noNull, 0, 1f, 1125f) * transparency, -(float)Math.PI / 2f, new Vector2(2f, 1.5f) * scaleSize, scaleSize * 4f, SpriteEffects.None, layerDepth);
			spriteBatch.Draw(Game1.objectSpriteSheet, location + new Vector2(49f, 35f) * scaleSize, new Rectangle(263, 579, 4, 2), Utility.Get2PhaseColor(color1noNull, color2noNull, 0, 1f, 375f) * transparency, (float)Math.PI / 2f, new Vector2(2f, 1.5f) * scaleSize, scaleSize * 4f, SpriteEffects.None, layerDepth);
			spriteBatch.Draw(Game1.objectSpriteSheet, location + new Vector2(31f, 53f) * scaleSize, new Rectangle(263, 579, 4, 2), Utility.Get2PhaseColor(color1noNull, color2noNull, 0, 1f, 750f) * transparency, (float)Math.PI, new Vector2(2f, 1.5f) * scaleSize, scaleSize * 4f, SpriteEffects.None, layerDepth);
			DrawMenuIcons(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color);
		}
		else
		{
			base.drawInMenu(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color, drawShadow);
		}
	}

	public override void update(GameTime time, GameLocation environment, Farmer who)
	{
		foreach (Ring combinedRing in combinedRings)
		{
			combinedRing.update(time, environment, who);
		}
		base.update(time, environment, who);
	}

	/// <summary>Update data when the <see cref="F:StardewValley.Objects.CombinedRing.combinedRings" /> list changes.</summary>
	protected virtual void OnCombinedRingsChanged()
	{
		description = null;
	}
}
