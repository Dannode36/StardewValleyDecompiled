using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Extensions;

namespace StardewValley.Quests;

public class LostItemQuest : Quest
{
	/// <summary>The internal name for the NPC who gave the quest.</summary>
	[XmlElement("npcName")]
	public readonly NetString npcName = new NetString();

	/// <summary>The internal name for the location where the item can be found.</summary>
	[XmlElement("locationOfItem")]
	public readonly NetString locationOfItem = new NetString();

	/// <summary>The qualified item ID for the item to find.</summary>
	[XmlElement("itemIndex")]
	public readonly NetString ItemId = new NetString();

	/// <summary>The X tile position within the location where the item can be found.</summary>
	[XmlElement("tileX")]
	public readonly NetInt tileX = new NetInt();

	/// <summary>The Y tile position within the location where the item can be found.</summary>
	[XmlElement("tileY")]
	public readonly NetInt tileY = new NetInt();

	/// <summary>Whether the player has found the item yet.</summary>
	[XmlElement("itemFound")]
	public readonly NetBool itemFound = new NetBool();

	/// <summary>The translatable text segments for the objective shown in the quest log.</summary>
	[XmlElement("objective")]
	public readonly NetDescriptionElementRef objective = new NetDescriptionElementRef();

	/// <summary>Construct an instance.</summary>
	public LostItemQuest()
	{
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="npcName">The internal name for the NPC who gave the quest.</param>
	/// <param name="locationOfItem">The internal name for the location where the item can be found.</param>
	/// <param name="itemId">The qualified or unqualified item ID for the item to find.</param>
	/// <param name="tileX">The X tile position within the location where the item can be found.</param>
	/// <param name="tileY">The Y tile position within the location where the item can be found.</param>
	/// <exception cref="T:System.InvalidOperationException">The <paramref name="itemId" /> matches a non-object-type item, which can't be placed in the world.</exception>
	public LostItemQuest(string npcName, string locationOfItem, string itemId, int tileX, int tileY)
	{
		this.npcName.Value = npcName;
		this.locationOfItem.Value = locationOfItem;
		ItemId.Value = ItemRegistry.QualifyItemId(itemId) ?? itemId;
		this.tileX.Value = tileX;
		this.tileY.Value = tileY;
		questType.Value = 9;
		if (!ItemRegistry.GetDataOrErrorItem(ItemId.Value).HasTypeObject())
		{
			throw new InvalidOperationException($"Can't create {GetType().Name} #{id.Value} because the lost item ({ItemId.Value}) isn't an object-type item.");
		}
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(objective, "objective").AddField(npcName, "npcName").AddField(locationOfItem, "locationOfItem")
			.AddField(ItemId, "ItemId")
			.AddField(tileX, "tileX")
			.AddField(tileY, "tileY")
			.AddField(itemFound, "itemFound");
	}

	public override void adjustGameLocation(GameLocation location)
	{
		if (!itemFound && location.name.Equals(locationOfItem.Value))
		{
			Vector2 position = new Vector2((int)tileX, (int)tileY);
			location.overlayObjects.Remove(position);
			Object o = ItemRegistry.Create<Object>(ItemId);
			o.TileLocation = position;
			o.questItem.Value = true;
			o.questId.Value = id.Value;
			o.IsSpawnedObject = true;
			location.overlayObjects.Add(position, o);
		}
	}

	public new void reloadObjective()
	{
		if (objective.Value != null)
		{
			base.currentObjective = objective.Value.loadDescriptionElement();
		}
	}

	public override bool checkIfComplete(NPC n = null, int number1 = -1, int number2 = -2, Item item = null, string str = null, bool probe = false)
	{
		if ((bool)completed)
		{
			return false;
		}
		if (!itemFound && item != null && item.QualifiedItemId == ItemId.Value)
		{
			if (!probe)
			{
				itemFound.Value = true;
				string npcDisplayName = npcName;
				NPC namedNpc = Game1.getCharacterFromName(npcName);
				if (namedNpc != null)
				{
					npcDisplayName = namedNpc.displayName;
				}
				Game1.player.completelyStopAnimatingOrDoingAction();
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Quests:MessageFoundLostItem", item.DisplayName, npcDisplayName));
				objective.Value = new DescriptionElement("Strings\\Quests:ObjectiveReturnToNPC", namedNpc);
				Game1.playSound("jingle1");
			}
		}
		else if ((bool)itemFound && n != null && n.Name.Equals(npcName.Value) && n.IsVillager && Game1.player.Items.ContainsId(ItemId))
		{
			if (!probe)
			{
				questComplete();
				string[] fields = Quest.GetRawQuestFields(id.Value);
				Dialogue thankYou = new Dialogue(n, null, ArgUtility.Get(fields, 9, "Data\\ExtraDialogue:LostItemQuest_DefaultThankYou", allowBlank: false));
				n.setNewDialogue(thankYou);
				Game1.drawDialogue(n);
				Game1.player.changeFriendship(250, n);
				Game1.player.removeFirstOfThisItemFromInventory(ItemId);
			}
			return true;
		}
		return false;
	}
}
