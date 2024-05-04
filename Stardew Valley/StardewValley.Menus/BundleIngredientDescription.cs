namespace StardewValley.Menus;

public struct BundleIngredientDescription
{
	/// <summary>The qualified or unqualified item ID to match, unless <see cref="F:StardewValley.Menus.BundleIngredientDescription.category" /> is set.</summary>
	public readonly string id;

	/// <summary>The item ID for the preserved item to match.</summary>
	public string preservesId;

	/// <summary>The object category to match, unless <see cref="F:StardewValley.Menus.BundleIngredientDescription.id" /> is set.</summary>
	public readonly int? category;

	/// <summary>The stack size required.</summary>
	public readonly int stack;

	/// <summary>The minimum quality required.</summary>
	public readonly int quality;

	/// <summary>Whether this bundle has been completed.</summary>
	public bool completed;

	/// <summary>Construct an instance.</summary>
	/// <param name="idOrCategory">The item ID or category to match.</param>
	/// <param name="stack">The stack size required.</param>
	/// <param name="quality">The minimum quality required.</param>
	/// <param name="completed">Whether this bundle has been completed.</param>
	/// <param name="preservesId">The item ID for the preserved item to match.</param>
	public BundleIngredientDescription(string idOrCategory, int stack, int quality, bool completed, string preservesId = null)
	{
		this.stack = stack;
		this.quality = quality;
		this.completed = completed;
		this.preservesId = preservesId;
		if (int.TryParse(idOrCategory, out var categoryValue) && categoryValue < 0)
		{
			id = null;
			category = categoryValue;
		}
		else
		{
			id = idOrCategory;
			category = null;
		}
	}

	public BundleIngredientDescription(BundleIngredientDescription other, bool completed)
	{
		id = other.id;
		category = other.category;
		stack = other.stack;
		quality = other.quality;
		preservesId = other.preservesId;
		this.completed = completed;
	}
}
