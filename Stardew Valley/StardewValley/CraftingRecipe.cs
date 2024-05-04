using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Extensions;
using StardewValley.Inventories;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects;
using StardewValley.TokenizableStrings;

namespace StardewValley;

public class CraftingRecipe
{
	public const int wild_seed_special_category = -777;

	/// <summary>The index in <c>Data/CookingRecipes</c> or <c>Data/CraftingRecipes</c> for the ingredient list.</summary>
	public const int index_ingredients = 0;

	/// <summary>The index in <c>Data/CookingRecipes</c> or <c>Data/CraftingRecipes</c> for the produced items.</summary>
	public const int index_output = 2;

	/// <summary>The index in <c>Data/CookingRecipes</c> for the unlock conditions.</summary>
	public const int index_cookingUnlockConditions = 3;

	/// <summary>The index in <c>Data/CookingRecipes</c> for the optional translated recipe name. If omitted, the display name for the first output item is used.</summary>
	public const int index_cookingDisplayName = 4;

	/// <summary>The index in <c>Data/CraftingRecipes</c> for whether it produces a <see cref="F:StardewValley.ItemRegistry.type_bigCraftable" /> item.</summary>
	public const int index_craftingBigCraftable = 3;

	/// <summary>The index in <c>Data/CraftingRecipes</c> for the unlock conditions.</summary>
	public const int index_craftingUnlockConditions = 4;

	/// <summary>The index in <c>Data/CraftingRecipes</c> for the optional translated recipe name. If omitted, the display name for the first output item is used.</summary>
	public const int index_craftingDisplayName = 5;

	/// <summary>The recipe key in <c>Data/CookingRecipes</c> or <c>Data/CraftingRecipes</c>.</summary>
	public string name;

	/// <summary>The translated display name for this recipe.</summary>
	public string DisplayName;

	/// <summary>The translated description for the item produced by recipe.</summary>
	public string description;

	/// <summary>The cached crafting recipe data loaded from <c>Data/CraftingRecipes</c>.</summary>
	public static Dictionary<string, string> craftingRecipes;

	/// <summary>The cached cooking recipe data loaded from <c>Data/CookingRecipes</c>.</summary>
	public static Dictionary<string, string> cookingRecipes;

	/// <summary>The ingredients needed by this recipe, indexed by unqualified item ID or category number.</summary>
	public Dictionary<string, int> recipeList = new Dictionary<string, int>();

	/// <summary>The unqualified item IDs produced by this recipe. If there are multiple items, one is chosen at random each time.</summary>
	public List<string> itemToProduce = new List<string>();

	/// <summary>Whether this recipe produces a <see cref="F:StardewValley.ItemRegistry.type_bigCraftable" /> item, instead of an <see cref="F:StardewValley.ItemRegistry.type_object" /> item.</summary>
	public bool bigCraftable;

	/// <summary>Whether this is a recipe in <c>Data/CookingRecipes</c> (true) or <c>Data/CraftingRecipes</c> (false).</summary>
	public bool isCookingRecipe;

	/// <summary>The number of times this recipe has been crafted by the player.</summary>
	public int timesCrafted;

	/// <summary>The number of the selected item in <see cref="F:StardewValley.CraftingRecipe.itemToProduce" /> to produce.</summary>
	public int numberProducedPerCraft;

	public static void InitShared()
	{
		craftingRecipes = DataLoader.CraftingRecipes(Game1.content);
		cookingRecipes = DataLoader.CookingRecipes(Game1.content);
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="name">The recipe key in <c>Data/CookingRecipes</c> or <c>Data/CraftingRecipes</c>.</param>
	public CraftingRecipe(string name)
		: this(name, cookingRecipes.ContainsKey(name))
	{
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="name">The recipe key in <c>Data/CookingRecipes</c> or <c>Data/CraftingRecipes</c>.</param>
	/// <param name="isCookingRecipe">Whether this is a recipe in <c>Data/CookingRecipes</c> (true) or <c>Data/CraftingRecipes</c> (false).</param>
	public CraftingRecipe(string name, bool isCookingRecipe)
	{
		this.isCookingRecipe = isCookingRecipe;
		this.name = name;
		string info;
		if (isCookingRecipe && cookingRecipes.TryGetValue(name, out var recipe))
		{
			info = recipe;
		}
		else if (craftingRecipes.TryGetValue(name, out recipe))
		{
			info = recipe;
		}
		else
		{
			this.name = (name = "Torch");
			info = craftingRecipes[name];
		}
		string[] fields = info.Split('/');
		if (!ArgUtility.TryGet(fields, 0, out var rawIngredients, out var error, allowBlank: false))
		{
			rawIngredients = "";
			LogParseError(info, error);
		}
		if (!ArgUtility.TryGet(fields, 2, out var rawOutputItems, out error, allowBlank: false))
		{
			rawOutputItems = "";
			LogParseError(info, error);
		}
		if (!ArgUtility.TryGetOptional(fields, isCookingRecipe ? 4 : 5, out var tokenizableDisplayName, out error))
		{
			LogParseError(info, error);
		}
		bigCraftable = !isCookingRecipe && ArgUtility.GetBool(fields, 3);
		string[] ingredients = ArgUtility.SplitBySpace(rawIngredients);
		for (int i = 0; i < ingredients.Length; i += 2)
		{
			recipeList.Add(ingredients[i], ArgUtility.GetInt(ingredients, i + 1, 1));
		}
		string[] outputItems = ArgUtility.SplitBySpace(rawOutputItems);
		for (int i = 0; i < outputItems.Length; i += 2)
		{
			itemToProduce.Add(outputItems[i]);
			numberProducedPerCraft = ArgUtility.GetInt(outputItems, i + 1, 1);
		}
		ParsedItemData itemData = GetItemData(useFirst: true);
		DisplayName = ((!string.IsNullOrWhiteSpace(tokenizableDisplayName)) ? TokenParser.ParseText(tokenizableDisplayName) : (itemData?.DisplayName ?? rawOutputItems));
		description = itemData?.Description ?? "";
		if (!Game1.player.craftingRecipes.TryGetValue(name, out timesCrafted))
		{
			timesCrafted = 0;
		}
		if (name.Equals("Crab Pot") && Game1.player.professions.Contains(7))
		{
			recipeList = new Dictionary<string, int>
			{
				["388"] = 25,
				["334"] = 2
			};
		}
	}

	public virtual string getIndexOfMenuView()
	{
		if (itemToProduce.Count <= 0)
		{
			return "-1";
		}
		return itemToProduce[0];
	}

	public virtual bool doesFarmerHaveIngredientsInInventory(IList<Item> extraToCheck = null)
	{
		foreach (KeyValuePair<string, int> kvp in recipeList)
		{
			int required_count = kvp.Value;
			required_count -= Game1.player.getItemCount(kvp.Key);
			if (required_count <= 0)
			{
				continue;
			}
			if (extraToCheck != null)
			{
				required_count -= Game1.player.getItemCountInList(extraToCheck, kvp.Key);
				if (required_count <= 0)
				{
					continue;
				}
			}
			return false;
		}
		return true;
	}

	public virtual void drawMenuView(SpriteBatch b, int x, int y, float layerDepth = 0.88f, bool shadow = true)
	{
		ParsedItemData dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(bigCraftable ? ("(BC)" + getIndexOfMenuView()) : getIndexOfMenuView());
		Texture2D texture = dataOrErrorItem.GetTexture();
		Rectangle sourceRect = dataOrErrorItem.GetSourceRect();
		Utility.drawWithShadow(b, texture, new Vector2(x, y), sourceRect, Color.White, 0f, Vector2.Zero, 4f, flipped: false, layerDepth);
	}

	public virtual ParsedItemData GetItemData(bool useFirst = false)
	{
		string id = (useFirst ? itemToProduce.FirstOrDefault() : Game1.random.ChooseFrom(itemToProduce));
		return ItemRegistry.GetDataOrErrorItem(bigCraftable ? ("(BC)" + id) : ("(O)" + id));
	}

	public virtual Item createItem()
	{
		Item item = ItemRegistry.Create(GetItemData().QualifiedItemId, numberProducedPerCraft);
		if (isCookingRecipe && item is Object obj && Game1.player.team.SpecialOrderRuleActive("QI_COOKING"))
		{
			obj.orderData.Value = "QI_COOKING";
			obj.MarkContextTagsDirty();
		}
		return item;
	}

	public static bool isThereSpecialIngredientRule(Item potentialIngredient, string requiredIngredient)
	{
		if (requiredIngredient == (-777).ToString() && (potentialIngredient.QualifiedItemId == "(O)495" || potentialIngredient.QualifiedItemId == "(O)496" || potentialIngredient.QualifiedItemId == "(O)497" || potentialIngredient.QualifiedItemId == "(O)498"))
		{
			return true;
		}
		return false;
	}

	public virtual void consumeIngredients(List<IInventory> additionalMaterials)
	{
		foreach (KeyValuePair<string, int> pair in recipeList)
		{
			string itemId = pair.Key;
			int required_count = pair.Value;
			bool foundInBackpack = false;
			for (int i = Game1.player.Items.Count - 1; i >= 0; i--)
			{
				if (ItemMatchesForCrafting(Game1.player.Items[i], itemId))
				{
					int toRemove = required_count;
					required_count -= Game1.player.Items[i].Stack;
					Game1.player.Items[i] = Game1.player.Items[i].ConsumeStack(toRemove);
					if (required_count <= 0)
					{
						foundInBackpack = true;
						break;
					}
				}
			}
			if (additionalMaterials == null || foundInBackpack)
			{
				continue;
			}
			for (int c = 0; c < additionalMaterials.Count; c++)
			{
				IInventory items = additionalMaterials[c];
				if (items == null)
				{
					continue;
				}
				bool removedItem = false;
				for (int i = items.Count - 1; i >= 0; i--)
				{
					if (ItemMatchesForCrafting(items[i], itemId))
					{
						int removed_count = Math.Min(required_count, items[i].Stack);
						required_count -= removed_count;
						items[i] = items[i].ConsumeStack(removed_count);
						if (items[i] == null)
						{
							removedItem = true;
						}
						if (required_count <= 0)
						{
							break;
						}
					}
				}
				if (removedItem)
				{
					items.RemoveEmptySlots();
				}
				if (required_count <= 0)
				{
					break;
				}
			}
		}
	}

	public static bool DoesFarmerHaveAdditionalIngredientsInInventory(List<KeyValuePair<string, int>> additional_recipe_items, IList<Item> extraToCheck = null)
	{
		foreach (KeyValuePair<string, int> kvp in additional_recipe_items)
		{
			int required_count = kvp.Value;
			required_count -= Game1.player.getItemCount(kvp.Key);
			if (required_count <= 0)
			{
				continue;
			}
			if (extraToCheck != null)
			{
				required_count -= Game1.player.getItemCountInList(extraToCheck, kvp.Key);
				if (required_count <= 0)
				{
					continue;
				}
			}
			return false;
		}
		return true;
	}

	public static bool ItemMatchesForCrafting(Item item, string item_id)
	{
		if (item == null)
		{
			return false;
		}
		if (item.Category.ToString() == item_id)
		{
			return true;
		}
		if (isThereSpecialIngredientRule(item, item_id))
		{
			return true;
		}
		ParsedItemData item_data = ItemRegistry.GetDataOrErrorItem(item_id);
		if (item.QualifiedItemId == item_data.QualifiedItemId)
		{
			return true;
		}
		return false;
	}

	public static void ConsumeAdditionalIngredients(List<KeyValuePair<string, int>> additionalRecipeItems, List<IInventory> additionalMaterials)
	{
		for (int j = additionalRecipeItems.Count - 1; j >= 0; j--)
		{
			string item_index = additionalRecipeItems[j].Key;
			int required_count = additionalRecipeItems[j].Value;
			bool foundInBackpack = false;
			for (int i = Game1.player.Items.Count - 1; i >= 0; i--)
			{
				if (ItemMatchesForCrafting(Game1.player.Items[i], item_index))
				{
					int toRemove = required_count;
					required_count -= Game1.player.Items[i].Stack;
					Game1.player.Items[i].Stack -= toRemove;
					if (Game1.player.Items[i].Stack <= 0)
					{
						Game1.player.Items[i] = null;
					}
					if (required_count <= 0)
					{
						foundInBackpack = true;
						break;
					}
				}
			}
			if (additionalMaterials != null && !foundInBackpack)
			{
				for (int c = 0; c < additionalMaterials.Count; c++)
				{
					IInventory items = additionalMaterials[c];
					if (items == null)
					{
						continue;
					}
					bool removedItem = false;
					for (int i = items.Count - 1; i >= 0; i--)
					{
						if (ItemMatchesForCrafting(items[i], item_index))
						{
							int removed_count = Math.Min(required_count, items[i].Stack);
							required_count -= removed_count;
							items[i].Stack -= removed_count;
							if (items[i].Stack <= 0)
							{
								items[i] = null;
								removedItem = true;
							}
							if (required_count <= 0)
							{
								break;
							}
						}
					}
					if (removedItem)
					{
						items.RemoveEmptySlots();
					}
					if (required_count <= 0)
					{
						break;
					}
				}
			}
		}
	}

	public virtual int getCraftableCount(IList<Chest> additional_material_chests)
	{
		List<Item> additional_items = new List<Item>();
		if (additional_material_chests != null)
		{
			for (int c = 0; c < additional_material_chests.Count; c++)
			{
				additional_items.AddRange(additional_material_chests[c].Items);
			}
		}
		return getCraftableCount(additional_items);
	}

	public virtual int getCraftableCount(IList<Item> additional_materials)
	{
		int craftable_count = -1;
		foreach (KeyValuePair<string, int> pair in recipeList)
		{
			int ingredient_count = 0;
			string itemId = pair.Key;
			int required_count = pair.Value;
			if (!itemId.StartsWith("(") && !itemId.StartsWith("-"))
			{
				itemId = "(O)" + itemId;
			}
			for (int i = Game1.player.Items.Count - 1; i >= 0; i--)
			{
				if (Game1.player.Items[i] is Object obj && (obj.QualifiedItemId == itemId || obj.Category.ToString() == itemId || isThereSpecialIngredientRule(obj, itemId)))
				{
					ingredient_count += obj.Stack;
				}
			}
			if (additional_materials != null)
			{
				for (int c = 0; c < additional_materials.Count; c++)
				{
					if (additional_materials[c] is Object obj && (obj.QualifiedItemId == itemId || obj.Category.ToString() == itemId || isThereSpecialIngredientRule(obj, itemId)))
					{
						ingredient_count += obj.Stack;
					}
				}
			}
			int current_craftable_count = ingredient_count / required_count;
			if (current_craftable_count < craftable_count || craftable_count == -1)
			{
				craftable_count = current_craftable_count;
			}
		}
		return craftable_count;
	}

	public virtual string getCraftCountText()
	{
		int timesCrafted;
		if (isCookingRecipe)
		{
			if (Game1.player.recipesCooked.TryGetValue(getIndexOfMenuView(), out var timesCooked) && timesCooked > 0)
			{
				return Game1.content.LoadString("Strings\\UI:Collections_Description_RecipesCooked", timesCooked);
			}
		}
		else if (Game1.player.craftingRecipes.TryGetValue(name, out timesCrafted) && timesCrafted > 0)
		{
			return Game1.content.LoadString("Strings\\UI:Crafting_NumberCrafted", timesCrafted);
		}
		return null;
	}

	public virtual int getDescriptionHeight(int width)
	{
		return (int)(Game1.smallFont.MeasureString(Game1.parseText(description, Game1.smallFont, width)).Y + (float)(getNumberOfIngredients() * 36) + (float)(int)Game1.smallFont.MeasureString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.567")).Y + 21f);
	}

	public virtual void drawRecipeDescription(SpriteBatch b, Vector2 position, int width, IList<Item> additional_crafting_items)
	{
		int lineExpansion = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? 8 : 0);
		b.Draw(Game1.staminaRect, new Rectangle((int)(position.X + 8f), (int)(position.Y + 32f + Game1.smallFont.MeasureString("Ing!").Y) - 4 - 2 - (int)((float)lineExpansion * 1.5f), width - 32, 2), Game1.textColor * 0.35f);
		Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.567"), Game1.smallFont, position + new Vector2(8f, 28f), Game1.textColor * 0.75f);
		int i = -1;
		foreach (KeyValuePair<string, int> pair in recipeList)
		{
			i++;
			int required_count = pair.Value;
			string required_item = pair.Key;
			int bag_count = Game1.player.getItemCount(required_item);
			int containers_count = 0;
			int countLeft = required_count - bag_count;
			if (additional_crafting_items != null)
			{
				containers_count = Game1.player.getItemCountInList(additional_crafting_items, required_item);
				if (countLeft > 0)
				{
					countLeft -= containers_count;
				}
			}
			string ingredient_name_text = getNameFromIndex(required_item);
			Color drawColor = ((countLeft <= 0) ? Game1.textColor : Color.Red);
			ParsedItemData dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(getSpriteIndexFromRawIndex(required_item));
			Texture2D texture = dataOrErrorItem.GetTexture();
			Rectangle sourceRect = dataOrErrorItem.GetSourceRect();
			float scale = 2f;
			if (sourceRect.Width > 0 || sourceRect.Height > 0)
			{
				scale *= 16f / (float)Math.Max(sourceRect.Width, sourceRect.Height);
			}
			b.Draw(texture, new Vector2(position.X + 16f, position.Y + 64f + (float)(i * 64 / 2) + (float)(i * 4) + 16f), sourceRect, Color.White, 0f, new Vector2(sourceRect.Width / 2, sourceRect.Height / 2), scale, SpriteEffects.None, 0.86f);
			Utility.drawTinyDigits(required_count, b, new Vector2(position.X + 32f - Game1.tinyFont.MeasureString(required_count.ToString() ?? "").X, position.Y + 64f + (float)(i * 64 / 2) + (float)(i * 4) + 21f), 2f, 0.87f, Color.AntiqueWhite);
			Vector2 text_draw_position = new Vector2(position.X + 32f + 8f, position.Y + 64f + (float)(i * 64 / 2) + (float)(i * 4) + 4f);
			Utility.drawTextWithShadow(b, ingredient_name_text, Game1.smallFont, text_draw_position, drawColor);
			if (Game1.options.showAdvancedCraftingInformation)
			{
				text_draw_position.X = position.X + (float)width - 40f;
				b.Draw(Game1.mouseCursors, new Rectangle((int)text_draw_position.X, (int)text_draw_position.Y + 2, 22, 26), new Rectangle(268, 1436, 11, 13), Color.White);
				Utility.drawTextWithShadow(b, (bag_count + containers_count).ToString() ?? "", Game1.smallFont, text_draw_position - new Vector2(Game1.smallFont.MeasureString(bag_count + containers_count + " ").X, 0f), drawColor);
			}
		}
		b.Draw(Game1.staminaRect, new Rectangle((int)position.X + 8, (int)position.Y + lineExpansion + 64 + 4 + recipeList.Count * 36, width - 32, 2), Game1.textColor * 0.35f);
		Utility.drawTextWithShadow(b, Game1.parseText(description, Game1.smallFont, width - 8), Game1.smallFont, position + new Vector2(0f, 76 + recipeList.Count * 36 + lineExpansion), Game1.textColor * 0.75f);
	}

	public virtual int getNumberOfIngredients()
	{
		return recipeList.Count;
	}

	public virtual string getSpriteIndexFromRawIndex(string item_id)
	{
		switch (item_id)
		{
		case "-1":
			return "(O)20";
		case "-2":
			return "(O)80";
		case "-3":
			return "(O)24";
		case "-4":
			return "(O)145";
		case "-5":
			return "(O)176";
		case "-6":
			return "(O)184";
		default:
			if (item_id == (-777).ToString())
			{
				return "(O)495";
			}
			return item_id;
		}
	}

	public virtual string getNameFromIndex(string item_id)
	{
		if (item_id != null && item_id.StartsWith('-'))
		{
			switch (item_id)
			{
			case "-1":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.568");
			case "-2":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.569");
			case "-3":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.570");
			case "-4":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.571");
			case "-5":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.572");
			case "-6":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.573");
			default:
				if (item_id == (-777).ToString())
				{
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.574");
				}
				return "???";
			}
		}
		ParsedItemData item_data = ItemRegistry.GetDataOrErrorItem(item_id);
		if (item_data != null)
		{
			return item_data.DisplayName;
		}
		return ItemRegistry.GetErrorItemName();
	}

	/// <summary>Log a message indicating the underlying crafting data is invalid.</summary>
	/// <param name="rawData">The raw data being parsed.</param>
	/// <param name="message">The error message indicating why parsing failed.</param>
	private void LogParseError(string rawData, string message)
	{
		Game1.log.Error($"Failed parsing raw recipe data '{rawData}' for {(isCookingRecipe ? "cooking" : "crafting")} recipe '{name}': {message}");
	}
}
