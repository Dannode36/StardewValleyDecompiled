using System;
using System.Collections.Generic;

namespace StardewValley.Network.NetReady.Internal;

/// <summary>A cancelable ready-check.</summary>
internal abstract class BaseReadyCheck
{
	/// <summary>The unique ID for this ready check.</summary>
	public string Id { get; }

	/// <summary>The ID of the active lock.</summary>
	public int ActiveLockId { get; protected set; }

	/// <summary>The current local ready state of the check.</summary>
	public ReadyState State { get; protected set; }

	/// <summary>The number of farmers that are ready to proceed.</summary>
	public int NumberReady { get; protected set; }

	/// <summary>The number of farmers that are required to proceed.</summary>
	public int NumberRequired { get; protected set; }

	/// <summary>Whether all required farmers are ready to proceed.</summary>
	public bool IsReady { get; protected set; }

	/// <summary>Whether we can still cancel our ready state.</summary>
	public bool IsCancelable => State != ReadyState.Locked;

	/// <summary>Construct an instance.</summary>
	/// <param name="id">The unique ID for this ready check.</param>
	protected BaseReadyCheck(string id)
	{
		Id = id;
		State = ReadyState.NotReady;
		NumberReady = 0;
		NumberRequired = Game1.getOnlineFarmers().Count;
		IsReady = false;
	}

	/// <summary>Set the players that are needed for this ready check to pass.</summary>
	/// <param name="farmerIds">The required player IDs.</param>
	public abstract void SetRequiredFarmers(List<long> farmerIds);

	/// <summary>Set whether the local player is ready to proceed.</summary>
	/// <param name="ready">Whether the local player is ready.</param>
	/// <returns>Returns <c>true</c> if we successfully updated the local state, or <c>false</c> if we can no longer update the state.</returns>
	public virtual bool SetLocalReady(bool ready)
	{
		if (!IsCancelable)
		{
			return false;
		}
		ReadyState state = State;
		State = (ready ? ReadyState.Ready : ReadyState.NotReady);
		return state != State;
	}

	/// <summary>Update this ready check.</summary>
	public abstract void Update();

	/// <summary>Process an incoming ready check sync message.</summary>
	/// <param name="messageType">The ready check sync type.</param>
	/// <param name="message">The incoming sync message.</param>
	public abstract void ProcessMessage(ReadyCheckMessageType messageType, IncomingMessage message);

	/// <summary>Send a message to other players.</summary>
	/// <param name="messageType">The ready check sync type.</param>
	/// <param name="data">The message data to send.</param>
	protected abstract void SendMessage(ReadyCheckMessageType messageType, params object[] data);

	/// <summary>Create a ready check sync message that can be sent to other players.</summary>
	/// <param name="messageType">The ready check sync type.</param>
	/// <param name="data">The message data to send.</param>
	protected OutgoingMessage CreateSyncMessage(ReadyCheckMessageType messageType, params object[] data)
	{
		object[] messageData = new object[data.Length + 2];
		messageData[0] = Id;
		messageData[1] = (byte)messageType;
		Array.Copy(data, 0, messageData, 2, data.Length);
		return new OutgoingMessage(31, Game1.player, messageData);
	}
}
