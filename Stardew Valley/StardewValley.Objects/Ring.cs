using System;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Buffs;
using StardewValley.Extensions;
using StardewValley.GameData.Objects;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Monsters;

namespace StardewValley.Objects;

[XmlInclude(typeof(CombinedRing))]
public class Ring : Item
{
	public const string SmallGlowRingId = "516";

	public const string GlowRingId = "517";

	public const string SmallMagnetRingId = "518";

	public const string MagnetRingId = "519";

	public const string SlimeCharmerRingId = "520";

	public const string WarriorRingId = "521";

	public const string VampireRingId = "522";

	public const string SavageRingId = "523";

	public const string YobaRingId = "524";

	public const string SturdyRingId = "525";

	public const string BurglarsRingId = "526";

	public const string IridiumBandId = "527";

	public const string AmethystRingId = "529";

	public const string TopazRingId = "530";

	public const string AquamarineRingId = "531";

	public const string JadeRingId = "532";

	public const string EmeraldRingId = "533";

	public const string RubyRingId = "534";

	public const string WeddingRingId = "801";

	public const string CrabshellRingId = "810";

	public const string NapalmRingId = "811";

	public const string ThornsRingId = "839";

	public const string LuckyRingId = "859";

	public const string HotJavaRingId = "860";

	public const string ProtectiveRingId = "861";

	public const string SoulSapperRingId = "862";

	public const string PhoenixRingId = "863";

	public const string CombinedRingId = "880";

	public const string ImmunityBandId = "887";

	public const string GlowstoneRingId = "888";

	[XmlElement("price")]
	public readonly NetInt price = new NetInt();

	/// <summary>Obsolete. This is only kept to preserve data from old save files, and isn't synchronized in multiplayer. Use <see cref="P:StardewValley.Item.ItemId" /> instead.</summary>
	[XmlElement("indexInTileSheet")]
	public int? obsolete_indexInTileSheet;

	[XmlElement("uniqueID")]
	public readonly NetInt uniqueID = new NetInt();

	[XmlIgnore]
	public string description;

	[XmlIgnore]
	public string displayName;

	[XmlIgnore]
	protected int? _lightSourceID;

	/// <inheritdoc />
	public override string TypeDefinitionId { get; } = "(O)";


	/// <inheritdoc />
	[XmlIgnore]
	public override string DisplayName
	{
		get
		{
			if (displayName == null)
			{
				loadDisplayFields();
			}
			return displayName;
		}
	}

	/// <inheritdoc />
	protected override void MigrateLegacyItemId()
	{
		itemId.Value = obsolete_indexInTileSheet?.ToString() ?? base.ParentSheetIndex.ToString();
		obsolete_indexInTileSheet = null;
	}

	public Ring()
	{
	}

	public Ring(string itemId)
		: this()
	{
		itemId = ValidateUnqualifiedItemId(itemId);
		ObjectData data = Game1.objectData[itemId];
		base.ItemId = itemId;
		base.Category = -96;
		Name = data.Name ?? ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId).InternalName;
		price.Value = data.Price;
		ResetParentSheetIndex();
		if (!int.TryParse(itemId, out var seedOffset))
		{
			seedOffset = 0;
		}
		uniqueID.Value = Game1.year + Game1.dayOfMonth + Game1.timeOfDay + seedOffset + Game1.player.TilePoint.X + (int)Game1.stats.MonstersKilled + (int)Game1.stats.ItemsCrafted;
		loadDisplayFields();
	}

	/// <inheritdoc />
	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(price, "price").AddField(uniqueID, "uniqueID");
	}

	/// <inheritdoc />
	public override bool CanBeLostOnDeath()
	{
		return false;
	}

	/// <inheritdoc />
	public override void onEquip(Farmer who)
	{
		base.onEquip(who);
		GameLocation location = who.currentLocation;
		if (_lightSourceID.HasValue)
		{
			location.removeLightSource(_lightSourceID.Value);
			_lightSourceID = null;
		}
		switch (base.ItemId)
		{
		case "516":
			_lightSourceID = (int)uniqueID + (int)who.UniqueMultiplayerID;
			while (location.sharedLights.ContainsKey(_lightSourceID.Value))
			{
				_lightSourceID = _lightSourceID.Value + 1;
			}
			location.sharedLights[_lightSourceID.Value] = new LightSource(1, new Vector2(who.Position.X + 21f, who.Position.Y + 64f), 5f, new Color(0, 50, 170), (int)uniqueID + (int)who.UniqueMultiplayerID, LightSource.LightContext.None, who.UniqueMultiplayerID);
			break;
		case "517":
			_lightSourceID = (int)uniqueID + (int)who.UniqueMultiplayerID;
			while (location.sharedLights.ContainsKey(_lightSourceID.Value))
			{
				_lightSourceID = _lightSourceID.Value + 1;
			}
			location.sharedLights[_lightSourceID.Value] = new LightSource(1, new Vector2(who.Position.X + 21f, who.Position.Y + 64f), 10f, new Color(0, 30, 150), (int)uniqueID + (int)who.UniqueMultiplayerID, LightSource.LightContext.None, who.UniqueMultiplayerID);
			break;
		case "888":
			_lightSourceID = (int)uniqueID + (int)who.UniqueMultiplayerID;
			while (location.sharedLights.ContainsKey(_lightSourceID.Value))
			{
				_lightSourceID = _lightSourceID.Value + 1;
			}
			location.sharedLights[_lightSourceID.Value] = new LightSource(1, new Vector2(who.Position.X + 21f, who.Position.Y + 64f), 10f, new Color(0, 80, 0), (int)uniqueID + (int)who.UniqueMultiplayerID, LightSource.LightContext.None, who.UniqueMultiplayerID);
			break;
		case "527":
			_lightSourceID = (int)uniqueID + (int)who.UniqueMultiplayerID;
			while (location.sharedLights.ContainsKey(_lightSourceID.Value))
			{
				_lightSourceID = _lightSourceID.Value + 1;
			}
			location.sharedLights[_lightSourceID.Value] = new LightSource(1, new Vector2(who.Position.X + 21f, who.Position.Y + 64f), 10f, new Color(0, 80, 0), (int)uniqueID + (int)who.UniqueMultiplayerID, LightSource.LightContext.None, who.UniqueMultiplayerID);
			break;
		}
	}

	/// <inheritdoc />
	public override void onUnequip(Farmer who)
	{
		base.onUnequip(who);
		switch (base.ItemId)
		{
		case "516":
		case "517":
		case "888":
		case "527":
			if (_lightSourceID.HasValue)
			{
				who.currentLocation.removeLightSource(_lightSourceID.Value);
				_lightSourceID = null;
			}
			break;
		}
	}

	public override void AddEquipmentEffects(BuffEffects effects)
	{
		base.AddEquipmentEffects(effects);
		switch (base.ItemId)
		{
		case "518":
			effects.MagneticRadius.Value += 64f;
			break;
		case "519":
			effects.MagneticRadius.Value += 128f;
			break;
		case "888":
			effects.MagneticRadius.Value += 128f;
			break;
		case "527":
			effects.MagneticRadius.Value += 128f;
			effects.AttackMultiplier.Value += 0.1f;
			break;
		case "529":
			effects.KnockbackMultiplier.Value += 0.1f;
			break;
		case "530":
			effects.Defense.Value += 1f;
			break;
		case "531":
			effects.CriticalChanceMultiplier.Value += 0.1f;
			break;
		case "532":
			effects.CriticalPowerMultiplier.Value += 0.1f;
			break;
		case "533":
			effects.WeaponSpeedMultiplier.Value += 0.1f;
			break;
		case "534":
			effects.AttackMultiplier.Value += 0.1f;
			break;
		case "810":
			effects.Defense.Value += 5f;
			break;
		case "859":
			effects.LuckLevel.Value += 1f;
			break;
		case "887":
			effects.Immunity.Value += 4f;
			break;
		}
	}

	/// <inheritdoc />
	public override string getCategoryName()
	{
		return Object.GetCategoryDisplayName(-96);
	}

	public virtual void onNewLocation(Farmer who, GameLocation environment)
	{
		if (_lightSourceID.HasValue)
		{
			environment.removeLightSource(_lightSourceID.Value);
			_lightSourceID = null;
		}
		switch (base.ItemId)
		{
		case "516":
		case "517":
		{
			GameLocation oldLocation = who.currentLocation;
			who.currentLocation = environment;
			onEquip(who);
			who.currentLocation = oldLocation;
			break;
		}
		case "888":
		case "527":
			_lightSourceID = (int)uniqueID + (int)who.UniqueMultiplayerID;
			while (environment.sharedLights.ContainsKey(_lightSourceID.Value))
			{
				_lightSourceID = _lightSourceID.Value + 1;
			}
			environment.sharedLights[_lightSourceID.Value] = new LightSource(1, new Vector2(who.Position.X + 21f, who.Position.Y + 64f), 10f, new Color(0, 30, 150), LightSource.LightContext.None, who.UniqueMultiplayerID);
			break;
		}
	}

	public virtual void onLeaveLocation(Farmer who, GameLocation environment)
	{
		switch (base.ItemId)
		{
		case "516":
		case "517":
		case "527":
		case "888":
			if (!_lightSourceID.HasValue && who != null)
			{
				_lightSourceID = (int)uniqueID + (int)who.UniqueMultiplayerID;
				bool found = false;
				while (environment.sharedLights.ContainsKey(_lightSourceID.Value))
				{
					_lightSourceID = _lightSourceID.Value + 1;
					found = true;
				}
				if (found)
				{
					_lightSourceID--;
				}
			}
			if (_lightSourceID.HasValue)
			{
				environment.removeLightSource(_lightSourceID.Value);
				_lightSourceID = null;
			}
			break;
		}
	}

	/// <inheritdoc />
	public override int salePrice(bool ignoreProfitMargins = false)
	{
		return price;
	}

	/// <summary>Apply ring effects when a monster is slain.</summary>
	/// <param name="monster">The monster slain.</param>
	/// <param name="location">The location where the monster was slain.</param>
	/// <param name="who">The player receiving ring effects.</param>
	public virtual void onMonsterSlay(Monster monster, GameLocation location, Farmer who)
	{
		string text = base.ItemId;
		if (!(text == "811"))
		{
			if (text == "860")
			{
				if (Game1.random.NextBool(0.25))
				{
					monster?.objectsToDrop.Add("395");
				}
				else if (Game1.random.NextBool(0.1))
				{
					monster?.objectsToDrop.Add("253");
				}
			}
		}
		else if (monster != null)
		{
			location?.explode(monster.Tile, 2, who, damageFarmers: false, -1, !(location is Farm) && !(location is SlimeHutch));
		}
		if (!who.IsLocalPlayer)
		{
			return;
		}
		switch (base.ItemId)
		{
		case "521":
			if (Game1.random.NextBool(0.1 + (double)((float)who.LuckLevel / 100f)))
			{
				who.applyBuff("20");
				Game1.playSound("warrior");
			}
			break;
		case "522":
			who.health = Math.Min(who.maxHealth, who.health + 2);
			break;
		case "523":
			who.applyBuff("22");
			break;
		case "862":
			who.Stamina = Math.Min(who.MaxStamina, who.Stamina + 4f);
			break;
		}
	}

	public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
	{
		AdjustMenuDrawForRecipes(ref transparency, ref scaleSize);
		ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
		spriteBatch.Draw(itemData.GetTexture(), location + new Vector2(32f, 32f) * scaleSize, itemData.GetSourceRect(), color * transparency, 0f, new Vector2(8f, 8f) * scaleSize, scaleSize * 4f, SpriteEffects.None, layerDepth);
		DrawMenuIcons(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color);
	}

	public virtual void update(GameTime time, GameLocation environment, Farmer who)
	{
		if (!_lightSourceID.HasValue)
		{
			return;
		}
		Vector2 offset = Vector2.Zero;
		if (who.shouldShadowBeOffset)
		{
			offset += who.drawOffset;
		}
		environment.repositionLightSource(_lightSourceID.Value, new Vector2(who.Position.X + 21f, who.Position.Y) + offset);
		if (!environment.isOutdoors && !(environment is MineShaft) && !(environment is VolcanoDungeon))
		{
			LightSource l = environment.getLightSource(_lightSourceID.Value);
			if (l != null)
			{
				l.radius.Value = 3f;
			}
		}
	}

	public override int maximumStackSize()
	{
		return 1;
	}

	public override Point getExtraSpaceNeededForTooltipSpecialIcons(SpriteFont font, int minWidth, int horizontalBuffer, int startingHeight, StringBuilder descriptionText, string boldTitleText, int moneyAmountToDisplayAtBottom)
	{
		Point dimensions = new Point(0, startingHeight);
		int extra_rows_needed = 0;
		if (GetsEffectOfRing("810"))
		{
			extra_rows_needed++;
		}
		if (GetsEffectOfRing("887") || GetsEffectOfRing("530"))
		{
			extra_rows_needed++;
		}
		if (GetsEffectOfRing("859"))
		{
			extra_rows_needed++;
		}
		dimensions.X = (int)Math.Max(minWidth, font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_DefenseBonus", 9999)).X + (float)horizontalBuffer);
		dimensions.Y += extra_rows_needed * Math.Max((int)font.MeasureString("TT").Y, 48);
		return dimensions;
	}

	public virtual bool GetsEffectOfRing(string ringId)
	{
		return base.ItemId == ringId;
	}

	public virtual int GetEffectsOfRingMultiplier(string ringId)
	{
		if (GetsEffectOfRing(ringId))
		{
			return 1;
		}
		return 0;
	}

	public override void drawTooltip(SpriteBatch spriteBatch, ref int x, ref int y, SpriteFont font, float alpha, StringBuilder overrideText)
	{
		if (description == null)
		{
			loadDisplayFields();
		}
		Utility.drawTextWithShadow(spriteBatch, Game1.parseText(description, Game1.smallFont, getDescriptionWidth()), font, new Vector2(x + 16, y + 16 + 4), Game1.textColor);
		y += (int)font.MeasureString(Game1.parseText(description, Game1.smallFont, getDescriptionWidth())).Y;
		if (GetsEffectOfRing("810") || GetsEffectOfRing("530"))
		{
			Utility.drawWithShadow(spriteBatch, Game1.mouseCursors, new Vector2(x + 16 + 4, y + 16 + 4), new Rectangle(110, 428, 10, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
			Utility.drawTextWithShadow(spriteBatch, Game1.content.LoadString("Strings\\UI:ItemHover_DefenseBonus", GetsEffectOfRing("810") ? (5 * GetEffectsOfRingMultiplier("810")) : GetEffectsOfRingMultiplier("530")), font, new Vector2(x + 16 + 52, y + 16 + 12), Game1.textColor * 0.9f * alpha);
			y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
		}
		if (GetsEffectOfRing("887"))
		{
			Utility.drawWithShadow(spriteBatch, Game1.mouseCursors, new Vector2(x + 16 + 4, y + 16 + 4), new Rectangle(150, 428, 10, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
			Utility.drawTextWithShadow(spriteBatch, Game1.content.LoadString("Strings\\UI:ItemHover_ImmunityBonus", 4 * GetEffectsOfRingMultiplier("887")), font, new Vector2(x + 16 + 52, y + 16 + 12), Game1.textColor * 0.9f * alpha);
			y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
		}
		if (GetsEffectOfRing("859"))
		{
			Utility.drawWithShadow(spriteBatch, Game1.mouseCursors, new Vector2(x + 16 + 4, y + 16 + 4), new Rectangle(50, 428, 10, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
			Utility.drawTextWithShadow(spriteBatch, "+" + Game1.content.LoadString("Strings\\UI:ItemHover_Buff4", GetEffectsOfRingMultiplier("859")), font, new Vector2(x + 16 + 52, y + 16 + 12), Game1.textColor * 0.9f * alpha);
			y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
		}
	}

	public override string getDescription()
	{
		if (description == null)
		{
			loadDisplayFields();
		}
		return Game1.parseText(description, Game1.smallFont, getDescriptionWidth());
	}

	public override bool isPlaceable()
	{
		return false;
	}

	/// <inheritdoc />
	protected override Item GetOneNew()
	{
		return new Ring(base.ItemId);
	}

	protected virtual bool loadDisplayFields()
	{
		ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
		displayName = itemData.DisplayName;
		description = itemData.Description;
		return true;
	}

	public virtual bool CanCombine(Ring ring)
	{
		if (ring is CombinedRing || this is CombinedRing)
		{
			return false;
		}
		if (base.QualifiedItemId == ring.QualifiedItemId)
		{
			return false;
		}
		return true;
	}

	public Ring Combine(Ring ring)
	{
		CombinedRing combinedRing = new CombinedRing();
		combinedRing.combinedRings.Add(getOne() as Ring);
		combinedRing.combinedRings.Add(ring.getOne() as Ring);
		combinedRing.uniqueID.Value = uniqueID.Value;
		return combinedRing;
	}
}
