using System;
using System.IO;
using StardewValley.Extensions;

namespace StardewValley.Network.NetEvents;

/// <summary>A request to add or remove mail for a group of players.</summary>
public sealed class SetMailRequest : BaseSetFlagRequest
{
	/// <summary>When the mail should be received by the player.</summary>
	public MailType MailType { get; private set; } = MailType.Tomorrow;


	/// <inheritdoc />
	public SetMailRequest()
	{
	}

	/// <summary>Constructs an instance.</summary>
	/// <param name="target">The players for which to perform the action.</param>
	/// <param name="mailId">The mail ID to add.</param>
	/// <param name="mailType">When the mail should be received by the player.</param>
	/// <param name="state">Whether to add the mail; else it'll be removed.</param>
	/// <param name="onlyPlayerId">This allows targeting individual players in specialized cases; most code should set <see cref="P:StardewValley.Network.NetEvents.BasePlayerActionRequest.Target" /> instead.</param>
	public SetMailRequest(PlayerActionTarget target, string mailId, MailType mailType, bool state, long? onlyPlayerId = null)
		: base(target, mailId, state, onlyPlayerId)
	{
		MailType = mailType;
	}

	/// <inheritdoc />
	public override void PerformAction(Farmer farmer)
	{
		switch (MailType)
		{
		case MailType.Now:
			ToggleMailbox(farmer, base.FlagId, base.FlagState);
			break;
		case MailType.Tomorrow:
			farmer.mailForTomorrow.Toggle(base.FlagId, base.FlagState);
			break;
		case MailType.Received:
			farmer.mailReceived.Toggle(base.FlagId, base.FlagState);
			break;
		case MailType.All:
			ToggleMailbox(farmer, base.FlagId, base.FlagState);
			farmer.mailForTomorrow.Toggle(base.FlagId, base.FlagState);
			farmer.mailReceived.Toggle(base.FlagId, base.FlagState);
			break;
		default:
			Game1.log.Warn($"Received request to add mail ID '{base.FlagId}' with unknown mail type '{MailType}'");
			break;
		}
	}

	/// <inheritdoc />
	public override void Read(BinaryReader reader)
	{
		base.Read(reader);
		MailType = (MailType)Enum.ToObject(typeof(MailType), reader.ReadByte());
	}

	/// <inheritdoc />
	public override void Write(BinaryWriter writer)
	{
		base.Write(writer);
		writer.Write((byte)MailType);
	}

	/// <summary>Toggle a mail flag in the player's mailbox.</summary>
	/// <param name="farmer">The farmer to update.</param>
	/// <param name="mailId">The mail ID to add or remove.</param>
	/// <param name="add">Whether to add the mail flag; else remove it.</param>
	private void ToggleMailbox(Farmer farmer, string mailId, bool add)
	{
		if (add)
		{
			farmer.mailbox.Add(mailId);
			return;
		}
		farmer.mailbox.RemoveWhere((string p) => p == mailId);
	}
}
