using System.IO;

namespace StardewValley.Network.NetEvents;

/// <summary>A request to set a flag for a group of players.</summary>
public abstract class BaseSetFlagRequest : BasePlayerActionRequest
{
	/// <summary>The flag ID to update.</summary>
	public string FlagId { get; private set; }

	/// <summary>The flag state to set.</summary>
	public bool FlagState { get; private set; }

	/// <inheritdoc />
	public override void Read(BinaryReader reader)
	{
		base.Read(reader);
		FlagId = reader.ReadString();
		FlagState = reader.ReadBoolean();
	}

	/// <inheritdoc />
	public override void Write(BinaryWriter writer)
	{
		base.Write(writer);
		writer.Write(FlagId);
		writer.Write(FlagState);
	}

	/// <inheritdoc />
	protected BaseSetFlagRequest()
	{
	}

	/// <summary>Constructs an instance.</summary>
	/// <param name="target">The players for which to perform the action.</param>
	/// <param name="flagId">The flag ID to update.</param>
	/// <param name="flagState">The flag state to set.</param>
	/// <param name="onlyPlayerId">The specific player ID to apply this event to, or <c>null</c> to apply it to all players matching <paramref name="target" />.</param>
	protected BaseSetFlagRequest(PlayerActionTarget target, string flagId, bool flagState, long? onlyPlayerId)
		: base(target, onlyPlayerId)
	{
		FlagId = flagId;
		FlagState = flagState;
	}
}
