using System.IO;
using Netcode;

namespace StardewValley.Network.NetEvents;

/// <summary>A net-synced request to perform an action for the target players.</summary>
public abstract class BasePlayerActionRequest : NetEventArg
{
	/// <summary>The players for which to perform an action.</summary>
	public PlayerActionTarget Target { get; private set; }

	/// <summary>The specific player ID to apply this event to, or <c>null</c> to apply it to all players matching <see cref="P:StardewValley.Network.NetEvents.BasePlayerActionRequest.Target" />.</summary>
	public long? OnlyPlayerId { get; private set; }

	/// <summary>Reads the request data from a net-sync stream.</summary>
	/// <param name="reader">The binary stream to read.</param>
	public virtual void Read(BinaryReader reader)
	{
		Target = (PlayerActionTarget)reader.ReadByte();
		OnlyPlayerId = (reader.ReadBoolean() ? new long?(reader.ReadInt64()) : null);
	}

	/// <summary>Writes the request data to a net-sync stream.</summary>
	/// <param name="writer">The binary stream to write to.</param>
	public virtual void Write(BinaryWriter writer)
	{
		writer.Write((byte)Target);
		writer.Write(OnlyPlayerId.HasValue);
		if (OnlyPlayerId.HasValue)
		{
			writer.Write(OnlyPlayerId.Value);
		}
	}

	/// <summary>Get whether this event should be applied to a given player.</summary>
	/// <param name="player">The player to check.</param>
	public bool MatchesPlayer(Farmer player)
	{
		if (OnlyPlayerId.HasValue && player.UniqueMultiplayerID != OnlyPlayerId.Value)
		{
			return false;
		}
		switch (Target)
		{
		case PlayerActionTarget.Current:
			return true;
		case PlayerActionTarget.Host:
			return Game1.IsMasterGame;
		case PlayerActionTarget.All:
			return true;
		default:
			Game1.log.Warn($"Can't process net request {GetType().AssemblyQualifiedName}: Invalid target '{Target}'");
			return false;
		}
	}

	/// <summary>Get whether this request should only be applied to the local player.</summary>
	public bool OnlyForLocalPlayer()
	{
		if (OnlyPlayerId.HasValue)
		{
			return MatchesPlayer(Game1.player);
		}
		switch (Target)
		{
		case PlayerActionTarget.Current:
			return true;
		case PlayerActionTarget.Host:
			return Game1.IsMasterGame;
		case PlayerActionTarget.All:
			if (Game1.IsMasterGame)
			{
				return Game1.netWorldState.Value.farmhandData.Length == 0;
			}
			return false;
		default:
			Game1.log.Warn($"Can't process net request {GetType().AssemblyQualifiedName}: Invalid target '{Target}'");
			return false;
		}
	}

	/// <summary>Applies the request to the current player.</summary>
	/// <param name="farmer">The players to change.</param>
	public abstract void PerformAction(Farmer farmer);

	/// <summary>Constructs an instance.</summary>
	protected BasePlayerActionRequest()
		: this(PlayerActionTarget.Current, null)
	{
	}

	/// <summary>Constructs an instance.</summary>
	/// <param name="target">The players for which to perform the action.</param>
	/// <param name="onlyPlayerId">The specific player ID to apply this event to, or <c>null</c> to apply it to all players matching <paramref name="target" />.</param>
	protected BasePlayerActionRequest(PlayerActionTarget target, long? onlyPlayerId)
	{
		Target = target;
		OnlyPlayerId = onlyPlayerId;
	}
}
