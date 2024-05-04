using System;
using System.Linq;
using System.Xml.Serialization;
using Netcode;

namespace StardewValley.Network;

public class NetMutex : INetObject<NetFields>
{
	public const long NoOwner = -1L;

	private long prevOwner = -1L;

	private readonly NetLong owner = new NetLong(-1L)
	{
		InterpolationWait = false
	};

	private readonly NetEvent1Field<long, NetLong> lockRequest = new NetEvent1Field<long, NetLong>
	{
		InterpolationWait = false
	};

	private Action onLockAcquired;

	private Action onLockFailed;

	[XmlIgnore]
	public NetFields NetFields { get; } = new NetFields("NetMutex");


	public NetMutex()
	{
		NetFields.SetOwner(this).AddField(owner, "owner").AddField(lockRequest, "lockRequest");
		lockRequest.onEvent += delegate(long playerId)
		{
			if (Game1.IsMasterGame && (owner.Value == -1 || owner.Value == playerId))
			{
				owner.Value = playerId;
				owner.MarkDirty();
			}
		};
	}

	public void RequestLock(Action acquired = null, Action failed = null)
	{
		if (owner.Value == Game1.player.UniqueMultiplayerID)
		{
			acquired?.Invoke();
			return;
		}
		if (owner.Value != -1)
		{
			failed?.Invoke();
			return;
		}
		lockRequest.Fire(Game1.player.UniqueMultiplayerID);
		onLockAcquired = acquired;
		onLockFailed = failed;
	}

	public void ReleaseLock()
	{
		owner.Value = -1L;
		onLockFailed = null;
		onLockAcquired = null;
	}

	public bool IsLocked()
	{
		return owner.Value != -1;
	}

	public bool IsLockHeld()
	{
		return owner.Value == Game1.player.UniqueMultiplayerID;
	}

	public void Update(GameLocation location)
	{
		Update(location.farmers);
	}

	public void Update(FarmerCollection farmers)
	{
		lockRequest.Poll();
		if (owner.Value != prevOwner)
		{
			if (owner.Value == Game1.player.UniqueMultiplayerID && onLockAcquired != null)
			{
				onLockAcquired();
			}
			if (owner.Value != Game1.player.UniqueMultiplayerID && onLockFailed != null)
			{
				onLockFailed();
			}
			onLockAcquired = null;
			onLockFailed = null;
			prevOwner = owner.Value;
		}
		if (Game1.IsMasterGame && owner.Value != -1 && farmers.FirstOrDefault((Farmer f) => f.UniqueMultiplayerID == owner.Value && f.locationBeforeForcedEvent.Value == null) == null)
		{
			ReleaseLock();
		}
	}
}
