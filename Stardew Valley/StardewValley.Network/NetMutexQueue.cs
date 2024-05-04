using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Netcode;

namespace StardewValley.Network;

public class NetMutexQueue<T> : INetObject<NetFields>
{
	private readonly NetLongDictionary<bool, NetBool> requests = new NetLongDictionary<bool, NetBool>
	{
		InterpolationWait = false
	};

	private readonly NetLong currentOwner = new NetLong
	{
		InterpolationWait = false
	};

	private readonly List<T> localJobs = new List<T>();

	[XmlIgnore]
	public Action<T> Processor = delegate
	{
	};

	[XmlIgnore]
	public NetFields NetFields { get; } = new NetFields("NetMutexQueue");


	public NetMutexQueue()
	{
		NetFields.SetOwner(this).AddField(requests, "requests").AddField(currentOwner, "currentOwner");
	}

	public void Add(T job)
	{
		localJobs.Add(job);
	}

	public bool Contains(T job)
	{
		return localJobs.Contains(job);
	}

	public void Clear()
	{
		localJobs.Clear();
	}

	public void Update(GameLocation location)
	{
		FarmerCollection farmers = location.farmers;
		if (farmers.Contains(Game1.player) && localJobs.Count > 0)
		{
			requests[Game1.player.UniqueMultiplayerID] = true;
		}
		else
		{
			requests.Remove(Game1.player.UniqueMultiplayerID);
		}
		if (Game1.IsMasterGame)
		{
			requests.RemoveWhere((KeyValuePair<long, bool> pair) => farmers.FirstOrDefault((Farmer f) => f.UniqueMultiplayerID == pair.Key) == null);
			if (!requests.ContainsKey(currentOwner.Value))
			{
				currentOwner.Value = -1L;
			}
		}
		if (currentOwner.Value == Game1.player.UniqueMultiplayerID)
		{
			foreach (T job in localJobs)
			{
				Processor(job);
			}
			localJobs.Clear();
			requests.Remove(Game1.player.UniqueMultiplayerID);
			currentOwner.Value = -1L;
		}
		if (Game1.IsMasterGame && currentOwner.Value == -1 && Utility.TryGetRandom(requests, out var ownerId, out var _))
		{
			currentOwner.Value = ownerId;
		}
	}
}
