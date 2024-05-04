using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Netcode;

namespace StardewValley.SpecialOrders.Objectives;

public class DeliverObjective : OrderObjective
{
	[XmlElement("acceptableContextTagSets")]
	public NetStringList acceptableContextTagSets = new NetStringList();

	[XmlElement("targetName")]
	public NetString targetName = new NetString();

	[XmlElement("message")]
	public NetString message = new NetString();

	public override void Load(SpecialOrder order, Dictionary<string, string> data)
	{
		if (data.TryGetValue("AcceptedContextTags", out var rawValue))
		{
			acceptableContextTagSets.Add(order.Parse(rawValue));
		}
		if (data.TryGetValue("TargetName", out rawValue))
		{
			targetName.Value = order.Parse(rawValue);
		}
		else
		{
			targetName.Value = _order.requester.Value;
		}
		if (data.TryGetValue("Message", out rawValue))
		{
			message.Value = order.Parse(rawValue);
		}
		else
		{
			message.Value = "";
		}
	}

	public override void InitializeNetFields()
	{
		base.InitializeNetFields();
		base.NetFields.AddField(acceptableContextTagSets, "acceptableContextTagSets").AddField(targetName, "targetName").AddField(message, "message");
	}

	public override bool ShouldShowProgress()
	{
		return false;
	}

	protected override void _Register()
	{
		base._Register();
		SpecialOrder order = _order;
		order.onItemDelivered = (Func<Farmer, NPC, Item, bool, int>)Delegate.Combine(order.onItemDelivered, new Func<Farmer, NPC, Item, bool, int>(OnItemDelivered));
	}

	protected override void _Unregister()
	{
		base._Unregister();
		SpecialOrder order = _order;
		order.onItemDelivered = (Func<Farmer, NPC, Item, bool, int>)Delegate.Remove(order.onItemDelivered, new Func<Farmer, NPC, Item, bool, int>(OnItemDelivered));
	}

	public virtual int OnItemDelivered(Farmer farmer, NPC npc, Item item, bool probe)
	{
		if (IsComplete())
		{
			return 0;
		}
		if (npc.Name != targetName.Value)
		{
			return 0;
		}
		bool is_valid_delivery = true;
		foreach (string acceptableContextTagSet in acceptableContextTagSets)
		{
			is_valid_delivery = false;
			bool fail = false;
			string[] array = acceptableContextTagSet.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				if (!ItemContextTagManager.DoAnyTagsMatch(array[i].Split('/'), item.GetContextTags()))
				{
					fail = true;
					break;
				}
			}
			if (!fail)
			{
				is_valid_delivery = true;
				break;
			}
		}
		if (!is_valid_delivery)
		{
			return 0;
		}
		int required_amount = GetMaxCount() - GetCount();
		int donated_amount = Math.Min(item.Stack, required_amount);
		if (donated_amount < required_amount)
		{
			return 0;
		}
		if (!probe)
		{
			Item donated_item = item.getOne();
			donated_item.Stack = donated_amount;
			_order.donatedItems.Add(donated_item);
			item.Stack -= donated_amount;
			IncrementCount(donated_amount);
			if (!string.IsNullOrEmpty(message.Value))
			{
				npc.CurrentDialogue.Push(new Dialogue(npc, null, message.Value));
				Game1.drawDialogue(npc);
			}
		}
		return donated_amount;
	}
}
