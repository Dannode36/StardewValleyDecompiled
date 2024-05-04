using System;
using System.Collections.Generic;

namespace StardewValley.Objects;

/// <summary>Handles incoming and outgoing phone calls.</summary>
public interface IPhoneHandler
{
	/// <summary>Check if the phone should start ringing for an incoming call now.</summary>
	/// <param name="random">The RNG with which to decide whether the phone rings and which call ID is selected.</param>
	string CheckForIncomingCall(Random random);

	/// <summary>Try to handle an incoming phone call.</summary>
	/// <param name="callId">The unique ID for the incoming call.</param>
	/// <param name="showDialogue">Show the dialogue box when the player answers the phone.</param>
	/// <returns>Returns whether the incoming call is handled and <paramref name="showDialogue" /> is set.</returns>
	bool TryHandleIncomingCall(string callId, out Action showDialogue);

	/// <summary>Get the phone numbers which the player can call.</summary>
	/// <returns>Returns pairs for the call ID (key) and display text (value).</returns>
	IEnumerable<KeyValuePair<string, string>> GetOutgoingNumbers();

	/// <summary>Try to handle the player selecting an outgoing phone number.</summary>
	/// <param name="callId">The unique ID for the outgoing call.</param>
	/// <returns>Returns whether the outgoing call was handled.</returns>
	bool TryHandleOutgoingCall(string callId);
}
