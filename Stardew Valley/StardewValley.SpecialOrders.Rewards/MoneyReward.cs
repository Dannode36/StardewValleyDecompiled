using System.Collections.Generic;
using Netcode;

namespace StardewValley.SpecialOrders.Rewards;

public class MoneyReward : OrderReward
{
	public NetInt amount = new NetInt(0);

	public NetFloat multiplier = new NetFloat(1f);

	public override void InitializeNetFields()
	{
		base.InitializeNetFields();
		base.NetFields.AddField(amount, "amount").AddField(multiplier, "multiplier");
	}

	public virtual int GetRewardMoneyAmount()
	{
		return (int)((float)amount.Value * multiplier.Value);
	}

	public override void Load(SpecialOrder order, Dictionary<string, string> data)
	{
		amount.Value = int.Parse(order.Parse(data["Amount"]));
		if (data.TryGetValue("Multiplier", out var rawValue))
		{
			multiplier.Value = float.Parse(order.Parse(rawValue));
		}
	}
}
