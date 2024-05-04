using System.Collections.Generic;
using StardewValley.Network.NetReady.Internal;

namespace StardewValley.Network.NetReady;

/// <summary>Manages and synchronizes ready checks, which ensure all players are ready before proceeding (e.g. before sleeping).</summary>
public class ReadySynchronizer
{
	/// <summary>The active ready checks by ID.</summary>
	private readonly Dictionary<string, BaseReadyCheck> ReadyChecks = new Dictionary<string, BaseReadyCheck>();

	/// <summary>Set the players that are needed for this ready check to pass.</summary>
	/// <param name="id">The ready check ID.</param>
	/// <param name="requiredFarmers">The required player IDs.</param>
	public void SetLocalRequiredFarmers(string id, List<Farmer> requiredFarmers)
	{
		List<long> farmerIds = new List<long>();
		foreach (Farmer player in requiredFarmers)
		{
			farmerIds.Add(player.UniqueMultiplayerID);
		}
		GetOrCreate(id).SetRequiredFarmers(farmerIds);
	}

	/// <summary>Set whether the local player is ready to proceed.</summary>
	/// <param name="id">The ready check ID.</param>
	/// <param name="ready">Whether the local player is ready.</param>
	public void SetLocalReady(string id, bool ready)
	{
		GetOrCreate(id).SetLocalReady(ready);
	}

	/// <summary>Get whether all required players are ready to proceed.</summary>
	/// <param name="id">The ready check ID.</param>
	public bool IsReady(string id)
	{
		return GetIfExists(id)?.IsReady ?? false;
	}

	/// <summary>Get whether we can still cancel our acceptance of a ready check.</summary>
	/// <param name="id">The ready check ID.</param>
	public bool IsReadyCheckCancelable(string id)
	{
		return GetIfExists(id)?.IsCancelable ?? false;
	}

	/// <summary>Get the number of players that are ready to proceed.</summary>
	/// <param name="id">The ready check ID.</param>
	public int GetNumberReady(string id)
	{
		return GetIfExists(id)?.NumberReady ?? 0;
	}

	/// <summary>Get the number of players that are required to proceed.</summary>
	/// <param name="id">The ready check ID.</param>
	public int GetNumberRequired(string id)
	{
		return GetIfExists(id)?.NumberRequired ?? 0;
	}

	/// <summary>Update all ready checks.</summary>
	public void Update()
	{
		foreach (BaseReadyCheck value in ReadyChecks.Values)
		{
			value.Update();
		}
	}

	/// <summary>Clear all ready checks.</summary>
	public void Reset()
	{
		ReadyChecks.Clear();
	}

	/// <summary>Process an incoming ready check sync message.</summary>
	/// <param name="message">The incoming sync message.</param>
	public void ProcessMessage(IncomingMessage message)
	{
		string id = message.Reader.ReadString();
		ReadyCheckMessageType messageType = (ReadyCheckMessageType)message.Reader.ReadByte();
		GetOrCreate(id).ProcessMessage(messageType, message);
	}

	/// <summary>Get a ready check by ID, or <c>null</c> if it doesn't exist.</summary>
	/// <param name="id">The ready check ID.</param>
	private BaseReadyCheck GetIfExists(string id)
	{
		if (id == null || !ReadyChecks.TryGetValue(id, out var check))
		{
			return null;
		}
		return check;
	}

	/// <summary>Get a ready check by ID, creating it if needed.</summary>
	/// <param name="id">The ready check ID.</param>
	private BaseReadyCheck GetOrCreate(string id)
	{
		if (ReadyChecks.TryGetValue(id, out var check))
		{
			return check;
		}
		check = (Game1.IsMasterGame ? ((BaseReadyCheck)new ServerReadyCheck(id)) : ((BaseReadyCheck)new ClientReadyCheck(id)));
		ReadyChecks.Add(id, check);
		return check;
	}
}
