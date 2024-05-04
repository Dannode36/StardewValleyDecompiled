using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.GameData.Objects;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TokenizableStrings;

namespace StardewValley.ItemTypeDefinitions;

/// <summary>Manages the data for object items.</summary>
public class ObjectDataDefinition : BaseItemDataDefinition
{
	/// <inheritdoc />
	public override string Identifier => "(O)";

	/// <inheritdoc />
	public override string StandardDescriptor => "O";

	/// <inheritdoc />
	public override IEnumerable<string> GetAllIds()
	{
		return Game1.objectData.Keys;
	}

	/// <inheritdoc />
	public override bool Exists(string itemId)
	{
		if (itemId != null)
		{
			return Game1.objectData.ContainsKey(itemId);
		}
		return false;
	}

	/// <inheritdoc />
	public override ParsedItemData GetData(string itemId)
	{
		ObjectData data = GetRawData(itemId);
		if (data == null)
		{
			return null;
		}
		int category = data.Category;
		if (category == 0 && data.Type == "Ring")
		{
			category = -96;
		}
		return new ParsedItemData(this, itemId, data.SpriteIndex, data.Texture ?? "Maps\\springobjects", data.Name, TokenParser.ParseText(data.DisplayName), TokenParser.ParseText(data.Description), category, data.Type, data, isErrorItem: false, data.ExcludeFromRandomSale);
	}

	/// <inheritdoc />
	public override Rectangle GetSourceRect(ParsedItemData data, Texture2D texture, int spriteIndex)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (texture == null)
		{
			throw new ArgumentNullException("texture");
		}
		return Game1.getSourceRectForStandardTileSheet(texture, spriteIndex, 16, 16);
	}

	/// <inheritdoc />
	public override Item CreateItem(ParsedItemData data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		string itemId = data.ItemId;
		HashSet<string> contextTags = ItemContextTagManager.GetBaseContextTags(itemId);
		if (contextTags.Contains("torch_item"))
		{
			return new Torch(1, itemId);
		}
		if (itemId == "812")
		{
			return new ColoredObject(itemId, 1, Color.Orange);
		}
		if (contextTags.Contains("item_type_ring") || itemId == "801")
		{
			if (!(itemId == "880"))
			{
				return new Ring(itemId);
			}
			return new CombinedRing();
		}
		return new Object(itemId, 1);
	}

	/// <summary>Get whether an object has an explicit category set in <c>Data/Objects</c>, regardless of whether a category is dynamically assigned after it's loaded.</summary>
	/// <param name="data">The parsed item data to check.</param>
	public static bool HasExplicitCategory(ParsedItemData data)
	{
		if (data.HasTypeObject() && data.RawData is ObjectData objectData)
		{
			return objectData.Category < 0;
		}
		return false;
	}

	/// <summary>Get the raw price field set in <c>Data/Objects</c>.</summary>
	/// <param name="data">The parsed item data to check.</param>
	public static int GetRawPrice(ParsedItemData data)
	{
		if (!data.HasTypeObject() || !(data.RawData is ObjectData objectData))
		{
			return 0;
		}
		return objectData.Price;
	}

	/// <summary>Create a flavored Aged Roe item (like 'Aged Tuna Roe').</summary>
	/// <param name="ingredient">The roe to age, or the fish whose aged roe to create.</param>
	public virtual ColoredObject CreateFlavoredAgedRoe(Object ingredient)
	{
		if (ingredient == null)
		{
			throw new ArgumentNullException("ingredient");
		}
		if (ingredient.QualifiedItemId != "(O)812")
		{
			ingredient = CreateFlavoredRoe(ingredient);
		}
		Color color = TailoringMenu.GetDyeColor(ingredient) ?? Color.Orange;
		ColoredObject coloredObject = new ColoredObject("447", 1, color);
		coloredObject.Name = "Aged " + ingredient.Name;
		coloredObject.preserve.Value = Object.PreserveType.AgedRoe;
		coloredObject.preservedParentSheetIndex.Value = ingredient.preservedParentSheetIndex.Value;
		coloredObject.Price = ingredient.Price * 2;
		return coloredObject;
	}

	/// <summary>Create a flavored honey item (like 'Poppy Honey').</summary>
	/// <param name="ingredient">The item for which to create a honey, or <c>null</c> for a Wild Honey.</param>
	public virtual Object CreateFlavoredHoney(Object ingredient)
	{
		Object honey = new Object("340", 1);
		if (ingredient == null || ingredient.Name == null || ingredient.Name == "Error Item" || ingredient.ItemId == "-1")
		{
			honey.Name = "Wild Honey";
		}
		else
		{
			honey.Name = ingredient.Name + " Honey";
			honey.Price += ingredient.Price * 2;
		}
		honey.preserve.Value = Object.PreserveType.Honey;
		honey.preservedParentSheetIndex.Value = ingredient?.ItemId ?? "-1";
		return honey;
	}

	/// <summary>Create a flavored jelly item (like 'Apple Jelly').</summary>
	/// <param name="ingredient">The item to jelly.</param>
	public virtual Object CreateFlavoredJelly(Object ingredient)
	{
		if (ingredient == null)
		{
			throw new ArgumentNullException("ingredient");
		}
		Color color = TailoringMenu.GetDyeColor(ingredient) ?? Color.Red;
		Object jelly = new ColoredObject("344", 1, color);
		jelly.Name = ingredient.Name + " Jelly";
		jelly.preserve.Value = Object.PreserveType.Jelly;
		jelly.preservedParentSheetIndex.Value = ingredient.ItemId;
		jelly.Price = ingredient.Price * 2 + 50;
		if (ingredient.Edibility > 0)
		{
			jelly.Edibility = (int)((float)ingredient.Edibility * 2f);
		}
		else if (ingredient.Edibility == -300)
		{
			jelly.Edibility = (int)((float)ingredient.Price * 0.2f);
		}
		else
		{
			jelly.Edibility = ingredient.Edibility;
		}
		return jelly;
	}

	/// <summary>Create a flavored juice item (like 'Apple Juice').</summary>
	/// <param name="ingredient">The item for which to create a juice.</param>
	public virtual Object CreateFlavoredJuice(Object ingredient)
	{
		if (ingredient == null)
		{
			throw new ArgumentNullException("ingredient");
		}
		Color color = TailoringMenu.GetDyeColor(ingredient) ?? Color.Green;
		Object juice = new ColoredObject("350", 1, color);
		juice.Name = ingredient.Name + " Juice";
		juice.preserve.Value = Object.PreserveType.Juice;
		juice.preservedParentSheetIndex.Value = ingredient.ItemId;
		juice.Price = (int)((double)ingredient.Price * 2.25);
		if (ingredient.Edibility > 0)
		{
			juice.Edibility = (int)((float)ingredient.Edibility * 2f);
		}
		else if (ingredient.Edibility == -300)
		{
			juice.Edibility = (int)((float)ingredient.Price * 0.4f);
		}
		else
		{
			juice.Edibility = ingredient.Edibility;
		}
		return juice;
	}

	/// <summary>Create a pickled item (like 'Pickled Beet').</summary>
	/// <param name="ingredient">The item to pickle.</param>
	public virtual Object CreateFlavoredPickle(Object ingredient)
	{
		if (ingredient == null)
		{
			throw new ArgumentNullException("ingredient");
		}
		Color color = TailoringMenu.GetDyeColor(ingredient) ?? Color.Green;
		Object pickled = new ColoredObject("342", 1, color);
		pickled.Name = "Pickled " + ingredient.Name;
		pickled.preserve.Value = Object.PreserveType.Pickle;
		pickled.preservedParentSheetIndex.Value = ingredient.ItemId;
		pickled.Price = ingredient.Price * 2 + 50;
		if (ingredient.Edibility > 0)
		{
			pickled.Edibility = (int)((float)ingredient.Edibility * 1.75f);
		}
		else if (ingredient.Edibility == -300)
		{
			pickled.Edibility = (int)((float)ingredient.Price * 0.25f);
		}
		else
		{
			pickled.Edibility = ingredient.Edibility;
		}
		return pickled;
	}

	/// <summary>Create a flavored Roe item (like 'Tuna Roe').</summary>
	/// <param name="ingredient">The fish whose roe to create.</param>
	public virtual ColoredObject CreateFlavoredRoe(Object ingredient)
	{
		if (ingredient == null)
		{
			throw new ArgumentNullException("ingredient");
		}
		Color color = ((ingredient.QualifiedItemId == "(O)698") ? new Color(61, 55, 42) : (TailoringMenu.GetDyeColor(ingredient) ?? Color.Orange));
		ColoredObject coloredObject = new ColoredObject("812", 1, color);
		coloredObject.Name = ingredient.Name + " Roe";
		coloredObject.preserve.Value = Object.PreserveType.Roe;
		coloredObject.preservedParentSheetIndex.Value = ingredient.ItemId;
		coloredObject.Price += ingredient.Price / 2;
		return coloredObject;
	}

	/// <summary>Create a flavored wine item (like 'Apple Wine').</summary>
	/// <param name="ingredient">The item for which to create a wine.</param>
	public virtual Object CreateFlavoredWine(Object ingredient)
	{
		if (ingredient == null)
		{
			throw new ArgumentNullException("ingredient");
		}
		Color color = TailoringMenu.GetDyeColor(ingredient) ?? Color.Purple;
		ColoredObject wine = new ColoredObject("348", 1, color);
		wine.Name = ingredient.Name + " Wine";
		wine.Price = ingredient.Price * 3;
		wine.preserve.Value = Object.PreserveType.Wine;
		wine.preservedParentSheetIndex.Value = ingredient.ItemId;
		if (ingredient.Edibility > 0)
		{
			wine.Edibility = (int)((float)ingredient.Edibility * 1.75f);
		}
		else if (ingredient.Edibility == -300)
		{
			wine.Edibility = (int)((float)ingredient.Price * 0.1f);
		}
		else
		{
			wine.Edibility = ingredient.Edibility;
		}
		return wine;
	}

	/// <summary>Create a flavored bait item (like 'Squid Bait').</summary>
	/// <param name="ingredient">The item for which to create a bait.</param>
	public virtual Object CreateFlavoredBait(Object ingredient)
	{
		if (ingredient == null)
		{
			throw new ArgumentNullException("ingredient");
		}
		Color color = TailoringMenu.GetDyeColor(ingredient) ?? Color.Orange;
		ColoredObject coloredObject = new ColoredObject("SpecificBait", 1, color);
		coloredObject.Name = ingredient.Name + " Bait";
		coloredObject.Price = Math.Max(1, (int)((float)ingredient.Price * 0.1f));
		coloredObject.preserve.Value = Object.PreserveType.Bait;
		coloredObject.preservedParentSheetIndex.Value = ingredient.ItemId;
		return coloredObject;
	}

	/// <summary>Create a flavored dried fruit item (like 'Dried Apple').</summary>
	/// <param name="ingredient">The item for which to create a wine.</param>
	public virtual Object CreateFlavoredDriedFruit(Object ingredient)
	{
		if (ingredient == null)
		{
			throw new ArgumentNullException("ingredient");
		}
		Color color = TailoringMenu.GetDyeColor(ingredient) ?? Color.Orange;
		Object driedFruit = new ColoredObject("DriedFruit", 1, color);
		driedFruit.Name = Lexicon.makePlural("Dried " + ingredient.Name);
		driedFruit.Price = (int)((float)(ingredient.Price * 5) * 1.5f) + 25;
		driedFruit.Quality = ingredient.Quality;
		driedFruit.preserve.Value = Object.PreserveType.DriedFruit;
		driedFruit.preservedParentSheetIndex.Value = ingredient.ItemId;
		driedFruit.Edibility = ingredient.Edibility * 3;
		if (ingredient.Edibility > 0)
		{
			driedFruit.Edibility = (int)((float)ingredient.Edibility * 3f);
		}
		else if (ingredient.Edibility == -300)
		{
			driedFruit.Edibility = (int)((float)ingredient.Price * 0.5f);
		}
		else
		{
			driedFruit.Edibility = ingredient.Edibility;
		}
		return driedFruit;
	}

	/// <summary>Create a flavored dried fruit item (like 'Dried Apple').</summary>
	/// <param name="ingredient">The item for which to create a wine.</param>
	public virtual Object CreateFlavoredDriedMushroom(Object ingredient)
	{
		if (ingredient == null)
		{
			throw new ArgumentNullException("ingredient");
		}
		Color color = TailoringMenu.GetDyeColor(ingredient) ?? Color.Orange;
		ColoredObject coloredObject = new ColoredObject("DriedMushrooms", 1, color);
		coloredObject.Name = Lexicon.makePlural("Dried " + ingredient.Name);
		coloredObject.Price = (int)((float)(ingredient.Price * 5) * 1.5f) + 25;
		coloredObject.Quality = ingredient.Quality;
		coloredObject.preserve.Value = Object.PreserveType.DriedMushroom;
		coloredObject.preservedParentSheetIndex.Value = ingredient.ItemId;
		coloredObject.Edibility = ingredient.Edibility * 3;
		return coloredObject;
	}

	/// <summary>Create a flavored dried fruit item (like 'Dried Apple').</summary>
	/// <param name="ingredient">The item for which to create a wine.</param>
	public virtual Object CreateFlavoredSmokedFish(Object ingredient)
	{
		if (ingredient == null)
		{
			throw new ArgumentNullException("ingredient");
		}
		Color color = TailoringMenu.GetDyeColor(ingredient) ?? Color.Orange;
		Object driedFruit = new ColoredObject("SmokedFish", 1, color);
		driedFruit.Name = "Smoked " + ingredient.Name;
		driedFruit.Price = ingredient.Price * 2;
		driedFruit.Quality = ingredient.Quality;
		driedFruit.preserve.Value = Object.PreserveType.SmokedFish;
		driedFruit.preservedParentSheetIndex.Value = ingredient.ItemId;
		if (ingredient.Edibility > 0)
		{
			driedFruit.Edibility = (int)((float)ingredient.Edibility * 1.5f);
		}
		else if (ingredient.Edibility == -300)
		{
			driedFruit.Edibility = (int)((float)ingredient.Price * 0.3f);
		}
		else
		{
			driedFruit.Edibility = ingredient.Edibility;
		}
		return driedFruit;
	}

	/// <summary>Create a flavored item (like 'Apple Juice').</summary>
	/// <param name="preserveType">The flavored item type to create.</param>
	/// <param name="ingredient">The ingredient to apply to the flavored item (like apple for Apple Juice).</param>
	public virtual Object CreateFlavoredItem(Object.PreserveType preserveType, Object ingredient)
	{
		return preserveType switch
		{
			Object.PreserveType.AgedRoe => CreateFlavoredAgedRoe(ingredient), 
			Object.PreserveType.Honey => CreateFlavoredHoney(ingredient), 
			Object.PreserveType.Jelly => CreateFlavoredJelly(ingredient), 
			Object.PreserveType.Juice => CreateFlavoredJuice(ingredient), 
			Object.PreserveType.Pickle => CreateFlavoredPickle(ingredient), 
			Object.PreserveType.Roe => CreateFlavoredRoe(ingredient), 
			Object.PreserveType.Wine => CreateFlavoredWine(ingredient), 
			Object.PreserveType.Bait => CreateFlavoredBait(ingredient), 
			Object.PreserveType.DriedFruit => CreateFlavoredDriedFruit(ingredient), 
			Object.PreserveType.DriedMushroom => CreateFlavoredDriedMushroom(ingredient), 
			Object.PreserveType.SmokedFish => CreateFlavoredSmokedFish(ingredient), 
			_ => null, 
		};
	}

	/// <summary>Get the raw data fields for an item.</summary>
	/// <param name="itemId">The unqualified item ID.</param>
	protected ObjectData GetRawData(string itemId)
	{
		if (itemId == null || !Game1.objectData.TryGetValue(itemId, out var data))
		{
			return null;
		}
		return data;
	}
}
