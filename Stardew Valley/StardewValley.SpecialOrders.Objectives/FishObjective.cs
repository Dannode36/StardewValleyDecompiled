using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Netcode;

namespace StardewValley.SpecialOrders.Objectives;

public class FishObjective : OrderObjective
{
	[XmlElement("acceptableContextTagSets")]
	public NetStringList acceptableContextTagSets = new NetStringList();

	public override void InitializeNetFields()
	{
		base.InitializeNetFields();
		base.NetFields.AddField(acceptableContextTagSets, "acceptableContextTagSets");
	}

	public override void Load(SpecialOrder order, Dictionary<string, string> data)
	{
		if (data.TryGetValue("AcceptedContextTags", out var rawValue))
		{
			acceptableContextTagSets.Add(order.Parse(rawValue));
		}
	}

	protected override void _Register()
	{
		base._Register();
		SpecialOrder order = _order;
		order.onFishCaught = (Action<Farmer, Item>)Delegate.Combine(order.onFishCaught, new Action<Farmer, Item>(OnFishCaught));
	}

	protected override void _Unregister()
	{
		base._Unregister();
		SpecialOrder order = _order;
		order.onFishCaught = (Action<Farmer, Item>)Delegate.Remove(order.onFishCaught, new Action<Farmer, Item>(OnFishCaught));
	}

	public virtual void OnFishCaught(Farmer farmer, Item fish_item)
	{
		foreach (string acceptableContextTagSet in acceptableContextTagSets)
		{
			bool fail = false;
			string[] array = acceptableContextTagSet.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				if (!ItemContextTagManager.DoAnyTagsMatch(array[i].Split('/'), fish_item.GetContextTags()))
				{
					fail = true;
					break;
				}
			}
			if (!fail)
			{
				IncrementCount(fish_item.Stack);
				break;
			}
		}
	}
}
