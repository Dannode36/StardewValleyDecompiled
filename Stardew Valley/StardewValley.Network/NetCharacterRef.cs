using System;
using Netcode;

namespace StardewValley.Network;

public class NetCharacterRef : INetObject<NetFields>
{
	private readonly NetNPCRef npc = new NetNPCRef();

	private readonly NetFarmerRef farmer = new NetFarmerRef();

	public NetFields NetFields { get; } = new NetFields("NetCharacterRef");


	public NetCharacterRef()
	{
		NetFields.SetOwner(this).AddField(npc.NetFields, "npc.NetFields").AddField(farmer.NetFields, "farmer.NetFields");
	}

	public Character Get(GameLocation location)
	{
		NPC npcValue = npc.Get(location);
		if (npcValue != null)
		{
			return npcValue;
		}
		return farmer.Value;
	}

	public void Set(GameLocation location, Character character)
	{
		if (!(character is NPC curNpc))
		{
			if (!(character is Farmer curFarmer))
			{
				throw new ArgumentException();
			}
			npc.Clear();
			farmer.Value = curFarmer;
		}
		else
		{
			npc.Set(location, curNpc);
			farmer.Value = null;
		}
	}

	public void Clear()
	{
		npc.Clear();
		farmer.Value = null;
	}
}
