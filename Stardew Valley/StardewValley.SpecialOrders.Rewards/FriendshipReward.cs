using System.Collections.Generic;
using System.Xml.Serialization;
using Netcode;

namespace StardewValley.SpecialOrders.Rewards;

public class FriendshipReward : OrderReward
{
	[XmlElement("targetName")]
	public NetString targetName = new NetString();

	[XmlElement("amount")]
	public NetInt amount = new NetInt();

	public override void InitializeNetFields()
	{
		base.InitializeNetFields();
		base.NetFields.AddField(targetName, "targetName").AddField(amount, "amount");
	}

	public override void Load(SpecialOrder order, Dictionary<string, string> data)
	{
		if (!data.TryGetValue("TargetName", out var target_name))
		{
			target_name = order.requester;
		}
		target_name = order.Parse(target_name);
		targetName.Value = target_name;
		if (!data.TryGetValue("Amount", out var amount_string))
		{
			amount_string = "250";
		}
		amount_string = order.Parse(amount_string);
		amount.Value = int.Parse(amount_string);
	}

	public override void Grant()
	{
		NPC n = Game1.getCharacterFromName(targetName.Value);
		if (n != null)
		{
			Game1.player.changeFriendship(amount.Value, n);
		}
	}
}
