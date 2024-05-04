using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.ItemTypeDefinitions;

namespace StardewValley;

public interface ISalable : IHaveItemTypeId
{
	/// <summary>A unique identifier for the base item type, like <c>(H)</c> for a hat.</summary>
	/// <remarks>For vanilla items, this matches one of the <see cref="T:StardewValley.ItemRegistry" />'s <c>type_*</c> fields.</remarks>
	string TypeDefinitionId { get; }

	/// <summary>A globally unique item ID.</summary>
	string QualifiedItemId { get; }

	string DisplayName { get; }

	string Name { get; }

	bool IsRecipe { get; set; }

	int Stack { get; set; }

	int Quality { get; set; }

	bool ShouldDrawIcon();

	void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow);

	string getDescription();

	int maximumStackSize();

	int addToStack(Item stack);

	/// <summary>Get the default price when selling this item to a shop.</summary>
	/// <param name="specificPlayerID">The player whose professions to take into account, or -1 to apply the bonuses from all applicable players.</param>
	int sellToStorePrice(long specificPlayerID = -1L);

	/// <summary>Get the default price when buying this item from a shop.</summary>
	/// <param name="ignoreProfitMargins">Whether to ignore the configured profit margins, even if they'd normally be applicable to this item.</param>
	int salePrice(bool ignoreProfitMargins = false);

	/// <summary>Get whether this item should apply profit margins when bought or sold in shops.</summary>
	bool appliesProfitMargins();

	/// <summary>The action to perform when this item is purchased from a shop.</summary>
	/// <param name="shopId">The unique ID for the shop it was purchased from.</param>
	/// <returns>Returns <c>true</c> if the item should be discarded (e.g. because it's a learn-type item like a recipe), or <c>false</c> if it should be added to the player's inventory.</returns>
	bool actionWhenPurchased(string shopId);

	bool canStackWith(ISalable other);

	bool CanBuyItem(Farmer farmer);

	bool IsInfiniteStock();

	ISalable GetSalableInstance();

	/// <summary>Ensure the <see cref="P:StardewValley.ISalable.Stack" /> is set to a valid value.</summary>
	void FixStackSize();

	/// <summary>Ensure the <see cref="P:StardewValley.ISalable.Quality" /> is set to a valid value.</summary>
	void FixQuality();
}
