using System;
using Netcode;

namespace StardewValley.Network;

public class NetDancePartner : INetObject<NetFields>
{
	private readonly NetFarmerRef farmer = new NetFarmerRef();

	private readonly NetString villager = new NetString();

	public Character Value
	{
		get
		{
			return GetCharacter();
		}
		set
		{
			SetCharacter(value);
		}
	}

	public NetFields NetFields { get; } = new NetFields("NetDancePartner");


	public NetDancePartner()
	{
		NetFields.SetOwner(this).AddField(farmer.NetFields, "farmer.NetFields").AddField(villager, "villager");
	}

	public NetDancePartner(Farmer farmer)
	{
		this.farmer.Value = farmer;
	}

	public NetDancePartner(string villagerName)
	{
		villager.Value = villagerName;
	}

	public Character GetCharacter()
	{
		if (farmer.Value != null)
		{
			return farmer.Value;
		}
		if (Game1.CurrentEvent != null && villager.Value != null)
		{
			return Game1.CurrentEvent.getActorByName(villager.Value);
		}
		return null;
	}

	public void SetCharacter(Character value)
	{
		if (value != null)
		{
			if (!(value is Farmer curFarmer))
			{
				if (!(value is NPC npc))
				{
					throw new ArgumentException(value.ToString());
				}
				if (!npc.IsVillager)
				{
					throw new ArgumentException(value.ToString());
				}
				farmer.Value = null;
				villager.Value = npc.Name;
			}
			else
			{
				farmer.Value = curFarmer;
				villager.Value = null;
			}
		}
		else
		{
			farmer.Value = null;
			villager.Value = null;
		}
	}

	public NPC TryGetVillager()
	{
		if (farmer.Value != null)
		{
			return null;
		}
		if (Game1.CurrentEvent != null && villager.Value != null)
		{
			return Game1.CurrentEvent.getActorByName(villager.Value);
		}
		return null;
	}

	public Farmer TryGetFarmer()
	{
		return farmer.Value;
	}

	public bool IsFarmer()
	{
		return TryGetFarmer() != null;
	}

	public bool IsVillager()
	{
		return TryGetVillager() != null;
	}

	public Gender GetGender()
	{
		if (IsFarmer())
		{
			return TryGetFarmer().Gender;
		}
		if (IsVillager())
		{
			return TryGetVillager().Gender;
		}
		return Gender.Undefined;
	}
}
