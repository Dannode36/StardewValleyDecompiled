using System;
using Netcode;

namespace StardewValley.Network;

public class NetNPCRef : INetObject<NetFields>
{
	private readonly NetGuid guid = new NetGuid();

	public NetFields NetFields { get; } = new NetFields("NetNPCRef");


	public NetNPCRef()
	{
		NetFields.SetOwner(this).AddField(guid, "guid");
	}

	public NPC Get(GameLocation location)
	{
		if (!(guid.Value != Guid.Empty) || location == null || !location.characters.TryGetValue(guid.Value, out var npc))
		{
			return null;
		}
		return npc;
	}

	public void Set(GameLocation location, NPC npc)
	{
		if (npc == null)
		{
			guid.Value = Guid.Empty;
			return;
		}
		Guid newGuid = location.characters.GuidOf(npc);
		if (newGuid == Guid.Empty)
		{
			throw new ArgumentException();
		}
		guid.Value = newGuid;
	}

	public void Clear()
	{
		guid.Value = Guid.Empty;
	}
}
