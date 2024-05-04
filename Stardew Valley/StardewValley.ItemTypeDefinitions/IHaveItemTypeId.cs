namespace StardewValley.ItemTypeDefinitions;

/// <summary>An item or item data which has an item data definition ID.</summary>
public interface IHaveItemTypeId
{
	/// <summary>Get the unique ID of the item data definition which specifies this item, like <c>(H)</c> for a hat.</summary>
	/// <remarks>For vanilla items, this matches one of the <see cref="T:StardewValley.ItemRegistry" />'s <c>type_*</c> fields.</remarks>
	string GetItemTypeId();
}
