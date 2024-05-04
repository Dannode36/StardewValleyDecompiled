using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Objects;

namespace StardewValley.SpecialOrders.Objectives;

public class DonateObjective : OrderObjective
{
	[XmlElement("dropBox")]
	public NetString dropBox = new NetString();

	[XmlElement("dropBoxGameLocation")]
	public NetString dropBoxGameLocation = new NetString();

	[XmlElement("dropBoxTileLocation")]
	public NetVector2 dropBoxTileLocation = new NetVector2();

	[XmlElement("acceptableContextTagSets")]
	public NetStringList acceptableContextTagSets = new NetStringList();

	[XmlElement("minimumCapacity")]
	public NetInt minimumCapacity = new NetInt(-1);

	[XmlElement("confirmed")]
	public NetBool confirmed = new NetBool(value: false);

	public virtual string GetDropboxLocationName()
	{
		if (dropBoxGameLocation.Value == "Trailer" && Game1.MasterPlayer.hasOrWillReceiveMail("pamHouseUpgrade"))
		{
			return "Trailer_Big";
		}
		return dropBoxGameLocation.Value;
	}

	public override void Load(SpecialOrder order, Dictionary<string, string> data)
	{
		if (data.TryGetValue("AcceptedContextTags", out var rawValue))
		{
			acceptableContextTagSets.Add(order.Parse(rawValue.Trim()));
		}
		if (data.TryGetValue("DropBox", out rawValue))
		{
			dropBox.Value = order.Parse(rawValue.Trim());
		}
		if (data.TryGetValue("DropBoxGameLocation", out rawValue))
		{
			dropBoxGameLocation.Value = order.Parse(rawValue.Trim());
		}
		if (data.TryGetValue("DropBoxIndicatorLocation", out rawValue))
		{
			string[] coordinates = ArgUtility.SplitBySpace(order.Parse(rawValue));
			dropBoxTileLocation.Value = new Vector2((float)Convert.ToDouble(coordinates[0]), (float)Convert.ToDouble(coordinates[1]));
		}
		if (data.TryGetValue("MinimumCapacity", out rawValue))
		{
			minimumCapacity.Value = int.Parse(order.Parse(rawValue));
		}
	}

	public int GetAcceptCount(Item item, int stack_count)
	{
		if (IsValidItem(item))
		{
			return Math.Min(GetMaxCount() - GetCount(), stack_count);
		}
		return 0;
	}

	public override void OnCompletion()
	{
		base.OnCompletion();
		if (dropBoxGameLocation != null)
		{
			GameLocation l = Game1.getLocationFromName(GetDropboxLocationName());
			if (l != null)
			{
				l.showDropboxIndicator = false;
			}
		}
	}

	public override bool CanComplete()
	{
		return confirmed.Value;
	}

	public virtual void Confirm()
	{
		if (GetCount() >= GetMaxCount())
		{
			confirmed.Value = true;
		}
		else
		{
			confirmed.Value = false;
		}
	}

	public override bool CanUncomplete()
	{
		return true;
	}

	public override void InitializeNetFields()
	{
		base.InitializeNetFields();
		base.NetFields.AddField(acceptableContextTagSets, "acceptableContextTagSets").AddField(dropBox, "dropBox").AddField(dropBoxGameLocation, "dropBoxGameLocation")
			.AddField(dropBoxTileLocation, "dropBoxTileLocation")
			.AddField(minimumCapacity, "minimumCapacity")
			.AddField(confirmed, "confirmed");
		confirmed.fieldChangeVisibleEvent += OnConfirmed;
	}

	protected void OnConfirmed(NetBool field, bool oldValue, bool newValue)
	{
		if (!Utility.ShouldIgnoreValueChangeCallback())
		{
			CheckCompletion();
		}
	}

	public virtual bool IsValidItem(Item item)
	{
		if (item == null)
		{
			return false;
		}
		foreach (string acceptableContextTagSet in acceptableContextTagSets)
		{
			bool fail = false;
			string[] array = acceptableContextTagSet.Split(',');
			foreach (string acceptable_tags in array)
			{
				if (acceptable_tags.StartsWith("color") && item is ColoredObject colorObject && colorObject.preservedParentSheetIndex.Value != null)
				{
					if (ItemContextTagManager.DoAnyTagsMatch(acceptable_tags.Split('/'), ItemContextTagManager.GetBaseContextTags(colorObject.preservedParentSheetIndex.Value)))
					{
						return true;
					}
					fail = true;
					break;
				}
				if (!ItemContextTagManager.DoAnyTagsMatch(acceptable_tags.Split('/'), item.GetContextTags()))
				{
					fail = true;
					break;
				}
			}
			if (!fail)
			{
				return true;
			}
		}
		return false;
	}
}
