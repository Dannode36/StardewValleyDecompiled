using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Shops;

/// <summary>As part of <see cref="T:StardewValley.GameData.Shops.ShopData" />, an NPC who can run the shop.</summary>
public class ShopOwnerData
{
	/// <summary>The backing field for <see cref="P:StardewValley.GameData.Shops.ShopOwnerData.Id" />.</summary>
	private string IdImpl;

	/// <summary>The backing field for <see cref="P:StardewValley.GameData.Shops.ShopOwnerData.Name" />.</summary>
	private string NameImpl;

	/// <summary>A game state query which indicates whether this owner entry is available. Defaults to always available.</summary>
	[ContentSerializer(Optional = true)]
	public string Condition;

	/// <summary>The internal name of the NPC to show in the shop menu portrait, or the asset name of the portrait spritesheet to display, or an empty string to disable the portrait. Omit to use the NPC matched via <see cref="P:StardewValley.GameData.Shops.ShopOwnerData.Name" /> if any.</summary>
	[ContentSerializer(Optional = true)]
	public string Portrait;

	/// <summary>The dialogues to show if this entry is selected. Each day one dialogue will be randomly chosen to show in the shop UI. Defaults to a generic dialogue (if this is <c>null</c>) or hides the dialogue (if this is set but none matched).</summary>
	[ContentSerializer(Optional = true)]
	public List<ShopDialogueData> Dialogues;

	/// <summary>If <see cref="F:StardewValley.GameData.Shops.ShopOwnerData.Dialogues" /> has multiple matching entries, whether to re-randomize which one is selected each time the shop is opened (instead of once per day).</summary>
	[ContentSerializer(Optional = true)]
	public bool RandomizeDialogueOnOpen = true;

	/// <summary>If set, a 'shop is closed'-style message to show instead of opening the shop.</summary>
	[ContentSerializer(Optional = true)]
	public string ClosedMessage;

	/// <summary>An ID for this entry within the shop. This only needs to be unique within the current shop's owner list. Defaults to <see cref="P:StardewValley.GameData.Shops.ShopOwnerData.Name" />.</summary>
	[ContentSerializer(Optional = true)]
	public string Id
	{
		get
		{
			return IdImpl ?? Name;
		}
		set
		{
			IdImpl = value;
		}
	}

	/// <summary>
	///   One of...
	///   <list type="bullet">
	///     <item><description>the internal name for the NPC who must be in range to use this entry;</description></item>
	///     <item><description><see cref="F:StardewValley.GameData.Shops.ShopOwnerType.AnyOrNone" /> to use this entry regardless of whether an NPC is in range;</description></item>
	///     <item><description><see cref="F:StardewValley.GameData.Shops.ShopOwnerType.Any" /> to use this entry if any NPC is in range;</description></item>
	///     <item><description><see cref="F:StardewValley.GameData.Shops.ShopOwnerType.None" /> to use this entry if no NPC is in range.</description></item>
	///   </list>
	///   This field is case-sensitive.
	/// </summary>
	public string Name
	{
		get
		{
			return NameImpl;
		}
		set
		{
			if (Enum.TryParse<ShopOwnerType>(value, ignoreCase: true, out var type) && Enum.IsDefined(typeof(ShopOwnerType), type))
			{
				NameImpl = type.ToString();
				Type = type;
			}
			else
			{
				NameImpl = value;
				Type = ShopOwnerType.NamedNpc;
			}
		}
	}

	/// <summary>How this entry matches NPCs.</summary>
	[ContentSerializerIgnore]
	public ShopOwnerType Type { get; private set; }

	/// <summary>Get whether an NPC name matches this entry.</summary>
	/// <param name="npcName">The NPC name to check.</param>
	public bool IsValid(string npcName)
	{
		return Type switch
		{
			ShopOwnerType.AnyOrNone => true, 
			ShopOwnerType.Any => !string.IsNullOrWhiteSpace(npcName), 
			ShopOwnerType.None => string.IsNullOrWhiteSpace(npcName), 
			_ => Name == npcName, 
		};
	}
}
