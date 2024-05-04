using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Shops;

/// <summary>As part of <see cref="T:StardewValley.GameData.Shops.ShopOwnerData" />, a possible dialogue that can be shown in the shop UI.</summary>
public class ShopDialogueData
{
	/// <summary>An ID for this dialogue. This only needs to be unique within the current dialogue list. For a custom entry, you should use a globally unique ID which includes your mod ID like <c>ExampleMod.Id_DialogueName</c>.</summary>
	public string Id;

	/// <summary>A game state query which indicates whether the dialogue should be available. Defaults to always available.</summary>
	[ContentSerializer(Optional = true)]
	public string Condition;

	/// <summary>A tokenizable string for the dialogue text to show. The resulting text is parsed using the dialogue format.</summary>
	[ContentSerializer(Optional = true)]
	public string Dialogue;

	/// <summary>A list of random dialogues to choose from, using the same format as <see cref="F:StardewValley.GameData.Shops.ShopDialogueData.Dialogue" />. If set, <see cref="F:StardewValley.GameData.Shops.ShopDialogueData.Dialogue" /> is ignored.</summary>
	[ContentSerializer(Optional = true)]
	public List<string> RandomDialogue;
}
