using System.Collections.Generic;
using Netcode;
using Netcode.Validation;

namespace StardewValley.SpecialOrders.Rewards;

public class ObjectReward : OrderReward
{
	public NetString itemKey = new NetString("");

	public NetInt amount = new NetInt(0);

	[NotNetField]
	public Object objectInstance;

	public override void InitializeNetFields()
	{
		base.InitializeNetFields();
		base.NetFields.AddField(itemKey, "itemKey").AddField(amount, "amount");
	}

	public override void Load(SpecialOrder order, Dictionary<string, string> data)
	{
		itemKey.Value = order.Parse(data["Item"]);
		amount.Value = int.Parse(order.Parse(data["Amount"]));
		objectInstance = new Object(itemKey, amount);
	}

	public override void Grant()
	{
		Object i = new Object(itemKey.Value, amount.Value);
		Game1.player.addItemByMenuIfNecessary(i);
	}
}
