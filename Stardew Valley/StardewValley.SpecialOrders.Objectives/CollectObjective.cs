using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Netcode;

namespace StardewValley.SpecialOrders.Objectives;

public class CollectObjective : OrderObjective
{
	[XmlElement("acceptableContextTagSets")]
	public NetStringList acceptableContextTagSets = new NetStringList();

	public override void Load(SpecialOrder order, Dictionary<string, string> data)
	{
		if (data.TryGetValue("AcceptedContextTags", out var rawValue))
		{
			acceptableContextTagSets.Add(order.Parse(rawValue));
		}
	}

	public override void InitializeNetFields()
	{
		base.InitializeNetFields();
		base.NetFields.AddField(acceptableContextTagSets, "acceptableContextTagSets");
	}

	protected override void _Register()
	{
		base._Register();
		SpecialOrder order = _order;
		order.onItemCollected = (Action<Farmer, Item>)Delegate.Combine(order.onItemCollected, new Action<Farmer, Item>(OnItemShipped));
	}

	protected override void _Unregister()
	{
		base._Unregister();
		SpecialOrder order = _order;
		order.onItemCollected = (Action<Farmer, Item>)Delegate.Remove(order.onItemCollected, new Action<Farmer, Item>(OnItemShipped));
	}

	public virtual void OnItemShipped(Farmer farmer, Item item)
	{
		foreach (string acceptableContextTagSet in acceptableContextTagSets)
		{
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
				IncrementCount(item.Stack);
				break;
			}
		}
	}
}
