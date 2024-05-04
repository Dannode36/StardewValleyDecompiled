using System;

namespace StardewValley.Network;

/// <summary>A net sync clients which allows intercepting received messages.</summary>
public interface IHookableClient
{
	/// <summary>A callback to raise when receiving a message. This receives the incoming message, a method to send an arbitrary message, and a callback to run the default logic.</summary>
	Action<IncomingMessage, Action<OutgoingMessage>, Action> OnProcessingMessage { get; set; }

	/// <summary>A callback to raise when sending a message. This receives the outgoing message, a method to send an arbitrary message, and a callback to resume the default logic.</summary>
	Action<OutgoingMessage, Action<OutgoingMessage>, Action> OnSendingMessage { get; set; }
}
