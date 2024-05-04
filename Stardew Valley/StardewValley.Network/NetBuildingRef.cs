using System.Collections;
using System.Collections.Generic;
using Netcode;
using StardewValley.Buildings;

namespace StardewValley.Network;

public class NetBuildingRef : INetObject<NetFields>, IEnumerable<Building>, IEnumerable
{
	private readonly NetString nameOfIndoors = new NetString();

	private readonly NetLocationRef location = new NetLocationRef();

	public NetFields NetFields { get; } = new NetFields("NetBuildingRef");


	public Building Value
	{
		get
		{
			string nameOfIndoors = this.nameOfIndoors.Get();
			if (nameOfIndoors == null)
			{
				return null;
			}
			if (location.Value == null)
			{
				return Game1.getFarm().getBuildingByName(nameOfIndoors);
			}
			return location.Value.getBuildingByName(nameOfIndoors);
		}
		set
		{
			if (value == null)
			{
				nameOfIndoors.Value = null;
				location.Value = null;
			}
			else
			{
				nameOfIndoors.Value = value.GetIndoorsName();
				location.Value = value.GetParentLocation();
			}
		}
	}

	public NetBuildingRef()
	{
		NetFields.SetOwner(this).AddField(nameOfIndoors, "nameOfIndoors").AddField(location.NetFields, "location.NetFields");
	}

	public IEnumerator<Building> GetEnumerator()
	{
		yield return Value;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
