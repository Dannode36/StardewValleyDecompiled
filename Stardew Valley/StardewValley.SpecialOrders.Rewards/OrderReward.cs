using System.Collections.Generic;
using System.Xml.Serialization;
using Netcode;

namespace StardewValley.SpecialOrders.Rewards;

[XmlInclude(typeof(FriendshipReward))]
[XmlInclude(typeof(GemsReward))]
[XmlInclude(typeof(MailReward))]
[XmlInclude(typeof(MoneyReward))]
[XmlInclude(typeof(ObjectReward))]
[XmlInclude(typeof(ResetEventReward))]
public class OrderReward : INetObject<NetFields>
{
	[XmlIgnore]
	public NetFields NetFields { get; } = new NetFields("OrderReward");


	public OrderReward()
	{
		InitializeNetFields();
	}

	public virtual void InitializeNetFields()
	{
		NetFields.SetOwner(this);
	}

	public virtual void Grant()
	{
	}

	public virtual void Load(SpecialOrder order, Dictionary<string, string> data)
	{
	}
}
