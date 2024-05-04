namespace StardewValley.Network.NetEvents;

/// <summary>The mail lists for a player.</summary>
public enum MailType : byte
{
	/// <summary>Mail in the mailbox now.</summary>
	Now,
	/// <summary>Mail queued to add to the mailbox tomorrow.</summary>
	Tomorrow,
	/// <summary>Mail that has already been received.</summary>
	Received,
	/// <summary>All mail types.</summary>
	All
}
