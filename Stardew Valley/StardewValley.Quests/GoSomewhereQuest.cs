using System.Xml.Serialization;
using Netcode;

namespace StardewValley.Quests;

public class GoSomewhereQuest : Quest
{
	[XmlElement("whereToGo")]
	public readonly NetString whereToGo = new NetString();

	public GoSomewhereQuest()
	{
	}

	public GoSomewhereQuest(string where)
	{
		whereToGo.Value = where;
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(whereToGo, "whereToGo");
	}

	public override void adjustGameLocation(GameLocation location)
	{
		checkIfComplete(null, -1, -2, null, location.name);
	}

	public override bool checkIfComplete(NPC n = null, int number1 = -1, int number2 = -2, Item item = null, string str = null, bool probe = false)
	{
		if (str != null && str.Equals(whereToGo.Value))
		{
			if (!probe)
			{
				questComplete();
			}
			return true;
		}
		return false;
	}
}
