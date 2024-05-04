using System;

namespace StardewValley.Network;

/// <summary>A net sync server which allows intercepting received messages.</summary>
public interface IHookableServer
{
	/// <summary>A callback to raise when receiving a message. This receives the incoming message, a method to send a message, and a callback to run the default logic.</summary>
	Action<IncomingMessage, Action<OutgoingMessage>, Action> OnProcessingMessage { get; set; }
}
