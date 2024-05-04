using System;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Buffs;
using StardewValley.ItemTypeDefinitions;

namespace StardewValley.Objects;

public class Boots : Item
{
	[XmlElement("defenseBonus")]
	public readonly NetInt defenseBonus = new NetInt();

	[XmlElement("immunityBonus")]
	public readonly NetInt immunityBonus = new NetInt();

	[XmlElement("indexInTileSheet")]
	public readonly NetInt indexInTileSheet = new NetInt();

	[XmlElement("price")]
	public readonly NetInt price = new NetInt();

	[XmlElement("indexInColorSheet")]
	public readonly NetInt indexInColorSheet = new NetInt();

	[XmlElement("appliedBootSheetIndex")]
	public readonly NetString appliedBootSheetIndex = new NetString();

	/// <summary>The cached value for <see cref="P:StardewValley.Objects.Boots.DisplayName" />.</summary>
	[XmlIgnore]
	public string displayName;

	[XmlIgnore]
	public string description;

	/// <inheritdoc />
	public override string TypeDefinitionId { get; } = "(B)";


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

	public Boots()
	{
		base.Category = -97;
	}

	public Boots(string itemId)
		: this()
	{
		itemId = ValidateUnqualifiedItemId(itemId);
		base.ItemId = itemId;
		reloadData();
		base.Category = -97;
	}

	/// <inheritdoc />
	protected override void MigrateLegacyItemId()
	{
		base.ItemId = indexInTileSheet.Value.ToString();
	}

	/// <inheritdoc />
	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(defenseBonus, "defenseBonus").AddField(immunityBonus, "immunityBonus").AddField(indexInTileSheet, "indexInTileSheet")
			.AddField(price, "price")
			.AddField(indexInColorSheet, "indexInColorSheet")
			.AddField(appliedBootSheetIndex, "appliedBootSheetIndex");
	}

	public virtual void reloadData()
	{
		ParsedItemData parsedData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
		string[] data = DataLoader.Boots(Game1.content)[base.ItemId].Split('/');
		Name = ArgUtility.Get(data, 0, null, allowBlank: false) ?? parsedData.InternalName;
		price.Value = Convert.ToInt32(data[2]);
		defenseBonus.Value = Convert.ToInt32(data[3]);
		immunityBonus.Value = Convert.ToInt32(data[4]);
		indexInColorSheet.Value = Convert.ToInt32(data[5]);
		indexInTileSheet.Value = parsedData.SpriteIndex;
	}

	public void applyStats(Boots applied_boots)
	{
		reloadData();
		if (defenseBonus.Value == (int)applied_boots.defenseBonus && immunityBonus.Value == (int)applied_boots.immunityBonus)
		{
			appliedBootSheetIndex.Value = null;
		}
		else
		{
			appliedBootSheetIndex.Value = applied_boots.getStatsIndex();
		}
		defenseBonus.Value = applied_boots.defenseBonus.Value;
		immunityBonus.Value = applied_boots.immunityBonus.Value;
		price.Value = applied_boots.price.Value;
		loadDisplayFields();
	}

	public virtual string getStatsIndex()
	{
		return appliedBootSheetIndex.Value ?? base.ItemId;
	}

	/// <inheritdoc />
	public override int salePrice(bool ignoreProfitMargins = false)
	{
		return (int)defenseBonus * 100 + (int)immunityBonus * 100;
	}

	/// <inheritdoc />
	public override void onEquip(Farmer who)
	{
		base.onEquip(who);
		who.changeShoeColor(GetBootsColorString());
	}

	/// <inheritdoc />
	public override void onUnequip(Farmer who)
	{
		base.onUnequip(who);
		who.changeShoeColor("12");
	}

	public override void AddEquipmentEffects(BuffEffects effects)
	{
		base.AddEquipmentEffects(effects);
		effects.Defense.Value += (int)defenseBonus;
		effects.Immunity.Value += (int)immunityBonus;
	}

	public string GetBootsColorString()
	{
		if (DataLoader.Boots(Game1.content).TryGetValue(base.ItemId, out var rawData))
		{
			string[] split = rawData.Split('/');
			if (split.Length > 7 && split[7] != "")
			{
				return split[7] + ":" + indexInColorSheet.Value;
			}
		}
		return indexInColorSheet.Value.ToString();
	}

	public int getNumberOfDescriptionCategories()
	{
		if ((int)immunityBonus > 0 && (int)defenseBonus > 0)
		{
			return 2;
		}
		return 1;
	}

	public override void drawTooltip(SpriteBatch spriteBatch, ref int x, ref int y, SpriteFont font, float alpha, StringBuilder overrideText)
	{
		Utility.drawTextWithShadow(spriteBatch, Game1.parseText(description, Game1.smallFont, getDescriptionWidth()), font, new Vector2(x + 16, y + 16 + 4), Game1.textColor);
		y += (int)font.MeasureString(Game1.parseText(description, Game1.smallFont, getDescriptionWidth())).Y;
		if ((int)defenseBonus > 0)
		{
			Utility.drawWithShadow(spriteBatch, Game1.mouseCursors, new Vector2(x + 16 + 4, y + 16 + 4), new Rectangle(110, 428, 10, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
			Utility.drawTextWithShadow(spriteBatch, Game1.content.LoadString("Strings\\UI:ItemHover_DefenseBonus", defenseBonus), font, new Vector2(x + 16 + 52, y + 16 + 12), Game1.textColor * 0.9f * alpha);
			y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
		}
		if ((int)immunityBonus > 0)
		{
			Utility.drawWithShadow(spriteBatch, Game1.mouseCursors, new Vector2(x + 16 + 4, y + 16 + 4), new Rectangle(150, 428, 10, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
			Utility.drawTextWithShadow(spriteBatch, Game1.content.LoadString("Strings\\UI:ItemHover_ImmunityBonus", immunityBonus), font, new Vector2(x + 16 + 52, y + 16 + 12), Game1.textColor * 0.9f * alpha);
			y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
		}
	}

	public override Point getExtraSpaceNeededForTooltipSpecialIcons(SpriteFont font, int minWidth, int horizontalBuffer, int startingHeight, StringBuilder descriptionText, string boldTitleText, int moneyAmountToDisplayAtBottom)
	{
		int maxStat = 9999;
		Point dimensions = new Point(0, startingHeight);
		dimensions.Y -= (int)font.MeasureString(descriptionText).Y;
		dimensions.Y += (int)((float)(getNumberOfDescriptionCategories() * 4 * 12) + font.MeasureString(Game1.parseText(description, Game1.smallFont, getDescriptionWidth())).Y);
		dimensions.X = (int)Math.Max(minWidth, Math.Max(font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_DefenseBonus", maxStat)).X + (float)horizontalBuffer, font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_ImmunityBonus", maxStat)).X + (float)horizontalBuffer));
		return dimensions;
	}

	public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
	{
		AdjustMenuDrawForRecipes(ref transparency, ref scaleSize);
		ParsedItemData data = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
		spriteBatch.Draw(data.GetTexture(), location + new Vector2(32f, 32f) * scaleSize, data.GetSourceRect(), color * transparency, 0f, new Vector2(8f, 8f) * scaleSize, scaleSize * 4f, SpriteEffects.None, layerDepth);
		DrawMenuIcons(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color);
	}

	public override int maximumStackSize()
	{
		return 1;
	}

	/// <inheritdoc />
	public override string getCategoryName()
	{
		return Object.GetCategoryDisplayName(-97);
	}

	public override string getDescription()
	{
		if (description == null)
		{
			loadDisplayFields();
		}
		return Game1.parseText(description + Environment.NewLine + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:Boots.cs.12500", (int)immunityBonus + (int)defenseBonus), Game1.smallFont, getDescriptionWidth());
	}

	public override bool isPlaceable()
	{
		return false;
	}

	/// <inheritdoc />
	protected override Item GetOneNew()
	{
		return new Boots(base.ItemId);
	}

	/// <inheritdoc />
	protected override void GetOneCopyFrom(Item source)
	{
		base.GetOneCopyFrom(source);
		if (source is Boots fromBoots)
		{
			appliedBootSheetIndex.Value = fromBoots.appliedBootSheetIndex.Value;
			indexInColorSheet.Value = fromBoots.indexInColorSheet.Value;
			defenseBonus.Value = fromBoots.defenseBonus.Value;
			immunityBonus.Value = fromBoots.immunityBonus.Value;
			loadDisplayFields();
		}
	}

	protected virtual bool loadDisplayFields()
	{
		if (DataLoader.Boots(Game1.content).TryGetValue(base.ItemId, out var rawData))
		{
			string[] data = rawData.Split('/');
			displayName = Name;
			if (data.Length > 6)
			{
				displayName = data[6];
			}
			if (appliedBootSheetIndex.Value != null)
			{
				displayName = Game1.content.LoadString("Strings\\StringsFromCSFiles:CustomizedBootItemName", DisplayName);
			}
			description = data[1];
			return true;
		}
		return false;
	}
}
