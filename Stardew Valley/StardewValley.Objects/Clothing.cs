using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.GameData.Shirts;
using StardewValley.ItemTypeDefinitions;
using StardewValley.TokenizableStrings;

namespace StardewValley.Objects;

public class Clothing : Item
{
	public enum ClothesType
	{
		SHIRT,
		PANTS
	}

	public const int SHIRT_SHEET_WIDTH = 128;

	public const string DefaultShirtSheetName = "Characters\\Farmer\\shirts";

	public const string DefaultPantsSheetName = "Characters\\Farmer\\pants";

	public const int MinShirtId = 1000;

	[XmlElement("price")]
	public readonly NetInt price = new NetInt();

	[XmlElement("indexInTileSheet")]
	public readonly NetInt indexInTileSheet = new NetInt();

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="F:StardewValley.Objects.Clothing.indexInTileSheet" /> instead.</summary>
	[XmlElement("indexInTileSheetFemale")]
	public int? obsolete_indexInTileSheetFemale;

	[XmlIgnore]
	public string description;

	[XmlIgnore]
	public string displayName;

	[XmlElement("clothesType")]
	public readonly NetEnum<ClothesType> clothesType = new NetEnum<ClothesType>();

	[XmlElement("dyeable")]
	public readonly NetBool dyeable = new NetBool(value: false);

	[XmlElement("clothesColor")]
	public readonly NetColor clothesColor = new NetColor(new Color(255, 255, 255));

	[XmlElement("isPrismatic")]
	public readonly NetBool isPrismatic = new NetBool(value: false);

	[XmlIgnore]
	protected bool _loadedData;

	/// <inheritdoc />
	public override string TypeDefinitionId
	{
		get
		{
			if (clothesType.Value != ClothesType.PANTS)
			{
				return "(S)";
			}
			return "(P)";
		}
	}

	public int Price
	{
		get
		{
			return price.Value;
		}
		set
		{
			price.Value = value;
		}
	}

	/// <inheritdoc />
	[XmlIgnore]
	public override string DisplayName
	{
		get
		{
			if (!_loadedData)
			{
				LoadData();
			}
			return displayName;
		}
	}

	public Clothing()
	{
		base.Category = -100;
	}

	/// <inheritdoc />
	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(price, "price").AddField(indexInTileSheet, "indexInTileSheet").AddField(clothesType, "clothesType")
			.AddField(dyeable, "dyeable")
			.AddField(clothesColor, "clothesColor")
			.AddField(isPrismatic, "isPrismatic");
	}

	public Clothing(string itemId)
		: this()
	{
		itemId = ValidateUnqualifiedItemId(itemId);
		Name = "Clothing";
		base.Category = -100;
		base.ItemId = itemId;
		LoadData(applyColor: true);
	}

	/// <summary>Apply the data from <see cref="F:StardewValley.Game1.pantsData" /> or <see cref="F:StardewValley.Game1.shirtData" /> to this item instance.</summary>
	/// <param name="applyColor">Whether to parse the tint color in field 6; else the tint is set to neutral white.</param>
	/// <param name="forceReload">Whether to reapply the latest data, even if this item was previously initialized.</param>
	public virtual void LoadData(bool applyColor = false, bool forceReload = false)
	{
		if (_loadedData && !forceReload)
		{
			return;
		}
		base.Category = -100;
		ShirtData shirtData;
		if (Game1.pantsData.TryGetValue(base.ItemId, out var pantsData))
		{
			Name = pantsData.Name;
			price.Value = pantsData.Price;
			indexInTileSheet.Value = pantsData.SpriteIndex;
			dyeable.Value = pantsData.CanBeDyed;
			if (applyColor)
			{
				clothesColor.Value = Utility.StringToColor(pantsData.DefaultColor) ?? Color.White;
			}
			else if (forceReload)
			{
				clothesColor.Value = Color.White;
			}
			displayName = TokenParser.ParseText(pantsData.DisplayName);
			description = TokenParser.ParseText(pantsData.Description);
			clothesType.Value = ClothesType.PANTS;
			isPrismatic.Value = pantsData.IsPrismatic;
		}
		else if (Game1.shirtData.TryGetValue(base.ItemId, out shirtData))
		{
			Name = shirtData.Name;
			price.Value = shirtData.Price;
			indexInTileSheet.Value = shirtData.SpriteIndex;
			dyeable.Value = shirtData.CanBeDyed;
			if (applyColor)
			{
				clothesColor.Value = Utility.StringToColor(shirtData.DefaultColor) ?? Color.White;
			}
			else if (forceReload)
			{
				clothesColor.Value = Color.White;
			}
			displayName = TokenParser.ParseText(shirtData.DisplayName);
			description = TokenParser.ParseText(shirtData.Description);
			clothesType.Value = ClothesType.SHIRT;
			isPrismatic.Value = shirtData.IsPrismatic;
		}
		else
		{
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
			displayName = itemData.DisplayName;
			description = itemData.Description;
		}
		if (dyeable.Value)
		{
			description = description + Environment.NewLine + Environment.NewLine + Game1.content.LoadString("Strings\\UI:Clothes_Dyeable");
		}
		_loadedData = true;
	}

	/// <inheritdoc />
	public override string getCategoryName()
	{
		return Object.GetCategoryDisplayName(-100);
	}

	/// <inheritdoc />
	public override int salePrice(bool ignoreProfitMargins = false)
	{
		return price;
	}

	public virtual void Dye(Color color, float strength = 0.5f)
	{
		if (dyeable.Value)
		{
			Color current_color = clothesColor.Value;
			clothesColor.Value = new Color(Utility.MoveTowards((float)(int)current_color.R / 255f, (float)(int)color.R / 255f, strength), Utility.MoveTowards((float)(int)current_color.G / 255f, (float)(int)color.G / 255f, strength), Utility.MoveTowards((float)(int)current_color.B / 255f, (float)(int)color.B / 255f, strength), Utility.MoveTowards((float)(int)current_color.A / 255f, (float)(int)color.A / 255f, strength));
		}
	}

	public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
	{
		AdjustMenuDrawForRecipes(ref transparency, ref scaleSize);
		Color clothes_color = clothesColor.Value;
		ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
		Texture2D texture = itemData.GetTexture();
		Rectangle spriteSourceRect = itemData.GetSourceRect();
		Rectangle dyeMaskSourceRect = Rectangle.Empty;
		if (!itemData.IsErrorItem)
		{
			if (clothesType.Value == ClothesType.SHIRT)
			{
				dyeMaskSourceRect = new Rectangle(spriteSourceRect.X + texture.Width / 2, spriteSourceRect.Y, spriteSourceRect.Width, spriteSourceRect.Height);
			}
			if (isPrismatic.Value)
			{
				clothes_color = Utility.GetPrismaticColor();
			}
		}
		switch (clothesType.Value)
		{
		case ClothesType.SHIRT:
		{
			float dye_portion_layer_offset = 1E-07f;
			if (layerDepth >= 1f - dye_portion_layer_offset)
			{
				layerDepth = 1f - dye_portion_layer_offset;
			}
			Vector2 origin = new Vector2(4f, 4f);
			if (itemData.IsErrorItem)
			{
				origin.X = spriteSourceRect.Width / 2;
				origin.Y = spriteSourceRect.Height / 2;
			}
			spriteBatch.Draw(texture, location + new Vector2(32f, 32f), spriteSourceRect, color * transparency, 0f, origin, scaleSize * 4f, SpriteEffects.None, layerDepth);
			spriteBatch.Draw(texture, location + new Vector2(32f, 32f), dyeMaskSourceRect, Utility.MultiplyColor(clothes_color, color) * transparency, 0f, origin, scaleSize * 4f, SpriteEffects.None, layerDepth + dye_portion_layer_offset);
			break;
		}
		case ClothesType.PANTS:
			spriteBatch.Draw(texture, location + new Vector2(32f, 32f), spriteSourceRect, Utility.MultiplyColor(clothes_color, color) * transparency, 0f, new Vector2(8f, 8f), scaleSize * 4f, SpriteEffects.None, layerDepth);
			break;
		}
		DrawMenuIcons(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color);
	}

	public override int maximumStackSize()
	{
		return 1;
	}

	public override string getDescription()
	{
		if (!_loadedData)
		{
			LoadData();
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
		return new Clothing(base.ItemId);
	}

	/// <inheritdoc />
	protected override void GetOneCopyFrom(Item source)
	{
		base.GetOneCopyFrom(source);
		if (source is Clothing fromClothing)
		{
			clothesColor.Value = fromClothing.clothesColor.Value;
		}
	}
}
