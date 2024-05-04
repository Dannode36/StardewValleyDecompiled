using System.Collections.Generic;

namespace StardewValley.Network.NetReady.Internal;

/// <summary>A cancelable ready-check for a farmhand player.</summary>
internal sealed class ClientReadyCheck : BaseReadyCheck
{
	/// <inheritdoc />
	public ClientReadyCheck(string id)
		: base(id)
	{
	}

	/// <inheritdoc />
	public override void SetRequiredFarmers(List<long> farmerIds)
	{
		if (farmerIds == null)
		{
			base.NumberRequired = Game1.getOnlineFarmers().Count;
			SendMessage(ReadyCheckMessageType.RequireFarmers, -1);
			return;
		}
		base.NumberRequired = farmerIds.Count;
		object[] data = new object[farmerIds.Count + 1];
		data[0] = farmerIds.Count;
		for (int i = 0; i < farmerIds.Count; i++)
		{
			data[i + 1] = farmerIds[i];
		}
		SendMessage(ReadyCheckMessageType.RequireFarmers, data);
	}

	/// <inheritdoc />
	public override bool SetLocalReady(bool ready)
	{
		if (!base.SetLocalReady(ready))
		{
			return false;
		}
		base.NumberReady++;
		SendMessage((!ready) ? ReadyCheckMessageType.Cancel : ReadyCheckMessageType.Ready);
		return true;
	}

	/// <inheritdoc />
	public override void Update()
	{
	}

	/// <inheritdoc />
	public override void ProcessMessage(ReadyCheckMessageType messageType, IncomingMessage message)
	{
		switch (messageType)
		{
		case ReadyCheckMessageType.Lock:
			ProcessLock(message);
			return;
		case ReadyCheckMessageType.Release:
			ProcessRelease(message);
			return;
		case ReadyCheckMessageType.UpdateAmounts:
			ProcessUpdateAmounts(message);
			return;
		case ReadyCheckMessageType.Finish:
			ProcessFinish(message);
			return;
		}
		Game1.log.Warn($"{"ClientReadyCheck"} '{base.Id}' received invalid message type '{messageType}'.");
	}

	/// <inheritdoc />
	protected override void SendMessage(ReadyCheckMessageType messageType, params object[] data)
	{
		Game1.client?.sendMessage(CreateSyncMessage(messageType, data));
	}

	/// <summary>Handle a request to mark this check as non-cancelable.</summary>
	/// <param name="message">The incoming <see cref="F:StardewValley.Network.NetReady.Internal.ReadyCheckMessageType.Lock" /> message.</param>
	private void ProcessLock(IncomingMessage message)
	{
		base.ActiveLockId = message.Reader.ReadInt32();
		if (base.State == ReadyState.NotReady)
		{
			SendMessage(ReadyCheckMessageType.RejectLock, base.ActiveLockId);
		}
		else
		{
			base.State = ReadyState.Locked;
			SendMessage(ReadyCheckMessageType.AcceptLock, base.ActiveLockId);
		}
	}

	/// <summary>Handle a request to mark this check as cancelable.</summary>
	/// <param name="message">The incoming <see cref="F:StardewValley.Network.NetReady.Internal.ReadyCheckMessageType.Release" /> message.</param>
	private void ProcessRelease(IncomingMessage message)
	{
		int lockId = message.Reader.ReadInt32();
		if (base.State == ReadyState.Locked && lockId == base.ActiveLockId)
		{
			base.State = ReadyState.Ready;
		}
	}

	/// <summary>Handle a request to update the displayed ready and required farmer counts.</summary>
	/// <param name="message">The incoming <see cref="F:StardewValley.Network.NetReady.Internal.ReadyCheckMessageType.UpdateAmounts" /> message.</param>
	private void ProcessUpdateAmounts(IncomingMessage message)
	{
		base.NumberReady = message.Reader.ReadInt32();
		base.NumberRequired = message.Reader.ReadInt32();
	}

	/// <summary>Handle a request to flag this check as ready to proceed.</summary>
	/// <param name="message">The incoming <see cref="F:StardewValley.Network.NetReady.Internal.ReadyCheckMessageType.Finish" /> message.</param>
	private void ProcessFinish(IncomingMessage message)
	{
		base.IsReady = true;
	}
}
