using System.Collections.Generic;
using Netcode;

namespace StardewValley.Quests;

public class NetDescriptionElementList : NetList<DescriptionElement, NetDescriptionElementRef>
{
	public NetDescriptionElementList()
	{
	}

	public NetDescriptionElementList(IEnumerable<DescriptionElement> values)
		: base(values)
	{
	}

	public NetDescriptionElementList(int capacity)
		: base(capacity)
	{
	}

	public void Add(string key)
	{
		Add(new DescriptionElement(key));
	}
}
