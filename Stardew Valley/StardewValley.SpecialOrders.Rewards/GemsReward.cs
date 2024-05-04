using System.Collections.Generic;
using Netcode;

namespace StardewValley.SpecialOrders.Rewards;

public class GemsReward : OrderReward
{
	public NetInt amount = new NetInt(0);

	public override void InitializeNetFields()
	{
		base.InitializeNetFields();
		base.NetFields.AddField(amount, "amount");
	}

	public override void Load(SpecialOrder order, Dictionary<string, string> data)
	{
		amount.Value = int.Parse(order.Parse(data["Amount"]));
	}

	public override void Grant()
	{
		Game1.player.QiGems += amount.Value;
	}
}
