using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Netcode;

namespace StardewValley.SpecialOrders.Objectives;

public class ShipObjective : OrderObjective
{
	[XmlElement("acceptableContextTagSets")]
	public NetStringList acceptableContextTagSets = new NetStringList();

	[XmlElement("useShipmentValue")]
	public NetBool useShipmentValue = new NetBool();

	public override void Load(SpecialOrder order, Dictionary<string, string> data)
	{
		if (data.TryGetValue("AcceptedContextTags", out var rawValue))
		{
			acceptableContextTagSets.Add(order.Parse(rawValue));
		}
		if (data.TryGetValue("UseShipmentValue", out rawValue) && rawValue.ToLowerInvariant().Trim() == "true")
		{
			useShipmentValue.Value = true;
		}
	}

	public override void InitializeNetFields()
	{
		base.InitializeNetFields();
		base.NetFields.AddField(acceptableContextTagSets, "acceptableContextTagSets").AddField(useShipmentValue, "useShipmentValue");
	}

	protected override void _Register()
	{
		base._Register();
		SpecialOrder order = _order;
		order.onItemShipped = (Action<Farmer, Item, int>)Delegate.Combine(order.onItemShipped, new Action<Farmer, Item, int>(OnItemShipped));
	}

	protected override void _Unregister()
	{
		base._Unregister();
		SpecialOrder order = _order;
		order.onItemShipped = (Action<Farmer, Item, int>)Delegate.Remove(order.onItemShipped, new Action<Farmer, Item, int>(OnItemShipped));
	}

	public virtual void OnItemShipped(Farmer farmer, Item item, int shipped_price)
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
				}
			}
			if (!fail)
			{
				if (useShipmentValue.Value)
				{
					IncrementCount(shipped_price);
				}
				else
				{
					IncrementCount(item.Stack);
				}
				break;
			}
		}
	}
}
