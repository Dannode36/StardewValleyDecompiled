using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData;

/// <summary>The data for an item to create, used in data assets like <see cref="T:StardewValley.GameData.Machines.MachineData" /> or <see cref="T:StardewValley.GameData.Shops.ShopData" />.</summary>
public class GenericSpawnItemData : ISpawnItemData
{
	/// <summary>The backing field for <see cref="P:StardewValley.GameData.GenericSpawnItemData.Id" />.</summary>
	private string IdImpl;

	/// <summary>An ID for this entry within the current list (not the item itself, which is <see cref="P:StardewValley.GameData.GenericSpawnItemData.ItemId" />). This only needs to be unique within the current list. For a custom entry, you should use a globally unique ID which includes your mod ID like <c>ExampleMod.Id_ItemName</c>.</summary>
	[ContentSerializer(Optional = true)]
	public string Id
	{
		get
		{
			if (IdImpl != null)
			{
				return IdImpl;
			}
			if (ItemId != null)
			{
				if (!IsRecipe)
				{
					return ItemId;
				}
				return ItemId + " (Recipe)";
			}
			List<string> randomItemId = RandomItemId;
			if (randomItemId != null && randomItemId.Count > 0)
			{
				if (!IsRecipe)
				{
					return string.Join("|", RandomItemId);
				}
				return string.Join("|", RandomItemId) + " (Recipe)";
			}
			return "???";
		}
		set
		{
			IdImpl = value;
		}
	}

	/// <inheritdoc />
	[ContentSerializer(Optional = true)]
	public string ItemId { get; set; }

	/// <inheritdoc />
	[ContentSerializer(Optional = true)]
	public List<string> RandomItemId { get; set; }

	/// <inheritdoc />
	[ContentSerializer(Optional = true)]
	public int? MaxItems { get; set; }

	/// <inheritdoc />
	[ContentSerializer(Optional = true)]
	public int MinStack { get; set; } = -1;


	/// <inheritdoc />
	[ContentSerializer(Optional = true)]
	public int MaxStack { get; set; } = -1;


	/// <inheritdoc />
	[ContentSerializer(Optional = true)]
	public int Quality { get; set; } = -1;


	/// <inheritdoc />
	[ContentSerializer(Optional = true)]
	public string ObjectInternalName { get; set; }

	/// <inheritdoc />
	[ContentSerializer(Optional = true)]
	public string ObjectDisplayName { get; set; }

	/// <inheritdoc />
	[ContentSerializer(Optional = true)]
	public int ToolUpgradeLevel { get; set; } = -1;


	/// <inheritdoc />
	[ContentSerializer(Optional = true)]
	public bool IsRecipe { get; set; }

	/// <inheritdoc />
	[ContentSerializer(Optional = true)]
	public List<QuantityModifier> StackModifiers { get; set; }

	/// <inheritdoc />
	[ContentSerializer(Optional = true)]
	public QuantityModifier.QuantityModifierMode StackModifierMode { get; set; }

	/// <inheritdoc />
	[ContentSerializer(Optional = true)]
	public List<QuantityModifier> QualityModifiers { get; set; }

	/// <inheritdoc />
	[ContentSerializer(Optional = true)]
	public QuantityModifier.QuantityModifierMode QualityModifierMode { get; set; }

	/// <inheritdoc />
	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> ModData { get; set; }

	/// <inheritdoc />
	[ContentSerializer(Optional = true)]
	public string PerItemCondition { get; set; }
}
