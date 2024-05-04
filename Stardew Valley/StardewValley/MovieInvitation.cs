using Netcode;
using StardewValley.Network;

namespace StardewValley;

public class MovieInvitation : INetObject<NetFields>
{
	private NetFarmerRef _farmer = new NetFarmerRef();

	protected NetString _invitedNPCName = new NetString();

	protected NetBool _fulfilled = new NetBool(value: false);

	public NetFields NetFields { get; } = new NetFields("MovieInvitation");


	public Farmer farmer
	{
		get
		{
			return _farmer.Value;
		}
		set
		{
			_farmer.Value = value;
		}
	}

	public NPC invitedNPC
	{
		get
		{
			return Game1.getCharacterFromName(_invitedNPCName.Value);
		}
		set
		{
			if (value == null)
			{
				_invitedNPCName.Set(null);
			}
			else
			{
				_invitedNPCName.Set(value.name);
			}
		}
	}

	public bool fulfilled
	{
		get
		{
			return _fulfilled.Value;
		}
		set
		{
			_fulfilled.Set(value);
		}
	}

	public MovieInvitation()
	{
		NetFields.SetOwner(this).AddField(_farmer.NetFields, "_farmer.NetFields").AddField(_invitedNPCName, "_invitedNPCName")
			.AddField(_fulfilled, "_fulfilled");
	}
}
