using System.Xml.Serialization;
using Netcode;

namespace StardewValley.Quests;

public class CraftingQuest : Quest
{
	/// <summary>Obsolete. This is only kept to preserve data from old save files, and isn't synced in multiplayer. Use <see cref="F:StardewValley.Quests.CraftingQuest.ItemId" /> instead.</summary>
	[XmlElement("isBigCraftable")]
	public bool? obsolete_isBigCraftable;

	/// <summary>The qualified item ID to craft.</summary>
	[XmlElement("indexToCraft")]
	public readonly NetString ItemId = new NetString();

	/// <summary>Construct an instance.</summary>
	public CraftingQuest()
	{
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="itemId">The qualified or unqualified item ID to craft.</param>
	public CraftingQuest(string itemId)
	{
		ItemId.Value = ItemRegistry.QualifyItemId(itemId) ?? itemId;
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(ItemId, "ItemId");
	}

	public override bool checkIfComplete(NPC n = null, int number1 = -1, int number2 = -2, Item item = null, string str = null, bool probe = false)
	{
		if (item?.QualifiedItemId == ItemId.Value)
		{
			if (!probe)
			{
				questComplete();
			}
			return true;
		}
		return false;
	}
}
