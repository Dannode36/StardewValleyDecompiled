using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.GameData.Movies;
using StardewValley.ItemTypeDefinitions;
using StardewValley.TokenizableStrings;

namespace StardewValley;

public class MovieConcession : ISalable, IHaveItemTypeId
{
	/// <summary>The underlying movie concession data.</summary>
	private readonly ConcessionItemData Data;

	/// <inheritdoc />
	public string TypeDefinitionId => "(Salable)";

	/// <inheritdoc />
	public string QualifiedItemId => TypeDefinitionId + "MovieConcession." + Id;

	public string Id => Data.Id;

	public string Name => Data.Name;

	public string DisplayName => TokenParser.ParseText(Data.DisplayName);

	public bool IsRecipe
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public int Stack
	{
		get
		{
			return 1;
		}
		set
		{
		}
	}

	public int Quality
	{
		get
		{
			return 0;
		}
		set
		{
		}
	}

	public List<string> Tags { get; }

	public MovieConcession(ConcessionItemData data)
	{
		Data = data;
		Tags = data.ItemTags?.ToList();
	}

	public void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
	{
		if (drawShadow)
		{
			spriteBatch.Draw(Game1.shadowTexture, location + new Vector2(32f, 48f), Game1.shadowTexture.Bounds, color * 0.5f, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f, SpriteEffects.None, layerDepth - 0.0001f);
		}
		spriteBatch.Draw(GetTexture(), location + new Vector2((int)(32f * scaleSize), (int)(32f * scaleSize)), Game1.getSourceRectForStandardTileSheet(GetTexture(), GetSpriteIndex(), 16, 16), color * transparency, 0f, new Vector2(8f, 8f) * scaleSize, 4f * scaleSize, SpriteEffects.None, layerDepth);
	}

	public Texture2D GetTexture()
	{
		if (!(Data.Texture == "LooseSprites\\Concessions"))
		{
			return Game1.content.Load<Texture2D>(Data.Texture);
		}
		return Game1.concessionsSpriteSheet;
	}

	public int GetSpriteIndex()
	{
		return Data.SpriteIndex;
	}

	public bool ShouldDrawIcon()
	{
		return true;
	}

	public string getDescription()
	{
		return TokenParser.ParseText(Data.Description);
	}

	public int maximumStackSize()
	{
		return 1;
	}

	public int addToStack(Item stack)
	{
		return 1;
	}

	public bool canStackWith(ISalable other)
	{
		return false;
	}

	/// <inheritdoc />
	public int sellToStorePrice(long specificPlayerID = -1L)
	{
		return -1;
	}

	/// <inheritdoc />
	public int salePrice(bool ignoreProfitMargins = false)
	{
		return Data.Price;
	}

	/// <inheritdoc />
	public bool appliesProfitMargins()
	{
		return false;
	}

	/// <inheritdoc />
	public bool actionWhenPurchased(string shopId)
	{
		return true;
	}

	public bool CanBuyItem(Farmer farmer)
	{
		return true;
	}

	public bool IsInfiniteStock()
	{
		return true;
	}

	public ISalable GetSalableInstance()
	{
		return this;
	}

	/// <inheritdoc />
	public void FixStackSize()
	{
	}

	/// <inheritdoc />
	public void FixQuality()
	{
	}

	/// <inheritdoc />
	public string GetItemTypeId()
	{
		return TypeDefinitionId;
	}
}
