using System.Collections;
using System.Collections.Generic;
using Netcode;

namespace StardewValley.Network;

public class NetFarmerRef : INetObject<NetFields>, IEnumerable<long?>, IEnumerable
{
	public readonly NetBool defined = new NetBool();

	public readonly NetLong uid = new NetLong();

	public NetFields NetFields { get; } = new NetFields("NetFarmerRef");


	public long UID
	{
		get
		{
			if (!defined)
			{
				return 0L;
			}
			return uid.Value;
		}
		set
		{
			uid.Value = value;
			defined.Value = true;
		}
	}

	public Farmer Value
	{
		get
		{
			if (!defined)
			{
				return null;
			}
			return getFarmer(uid.Value);
		}
		set
		{
			defined.Value = value != null;
			uid.Value = value?.UniqueMultiplayerID ?? 0;
		}
	}

	public NetFarmerRef()
	{
		NetFields.SetOwner(this).AddField(defined, "defined").AddField(uid, "uid");
	}

	private Farmer getFarmer(long uid)
	{
		foreach (Farmer farmer in Game1.getAllFarmers())
		{
			if (farmer.UniqueMultiplayerID == uid)
			{
				return farmer;
			}
		}
		return null;
	}

	public NetFarmerRef Delayed(bool interpolationWait)
	{
		defined.Interpolated(interpolate: false, interpolationWait);
		uid.Interpolated(interpolate: false, interpolationWait);
		return this;
	}

	public void Set(NetFarmerRef other)
	{
		uid.Value = other.uid.Value;
		defined.Value = other.defined.Value;
	}

	public IEnumerator<long?> GetEnumerator()
	{
		yield return defined ? new long?(uid.Value) : null;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Add(long? value)
	{
		if (!value.HasValue)
		{
			defined.Value = false;
			uid.Value = 0L;
		}
		else
		{
			defined.Value = true;
			uid.Value = value.Value;
		}
	}
}
