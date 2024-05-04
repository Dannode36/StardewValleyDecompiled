using System;
using System.IO;
using System.Linq;
using Galaxy.Api;
using StardewValley.Network;
using StardewValley.SDKs.GogGalaxy.Listeners;

namespace StardewValley.SDKs.GogGalaxy;

public class GalaxyNetClient : HookableClient
{
	public GalaxyID lobbyId;

	protected GalaxySocket client;

	private GalaxyID serverId;

	/// <summary>The custom display name for the host player, or null if no custom name was found.</summary>
	private string hostDisplayName;

	private GalaxySpecificUserDataListener galaxySpecificUserDataListener;

	private float lastPingMs;

	public GalaxyNetClient(GalaxyID lobbyId)
	{
		this.lobbyId = lobbyId;
		hostDisplayName = null;
	}

	~GalaxyNetClient()
	{
		galaxySpecificUserDataListener?.Dispose();
		galaxySpecificUserDataListener = null;
	}

	private void onProfileDataReady(GalaxyID userID)
	{
		if (!(userID != serverId))
		{
			hostDisplayName = null;
			try
			{
				hostDisplayName = GalaxyInstance.User().GetUserData("StardewDisplayName", userID);
			}
			catch (Exception)
			{
			}
			galaxySpecificUserDataListener?.Dispose();
			galaxySpecificUserDataListener = null;
		}
	}

	public override string getUserID()
	{
		return Convert.ToString(GalaxyInstance.User().GetGalaxyID().ToUint64());
	}

	protected override string getHostUserName()
	{
		if (!string.IsNullOrEmpty(hostDisplayName))
		{
			return hostDisplayName;
		}
		return GalaxyInstance.Friends().GetFriendPersonaName(serverId);
	}

	public override float GetPingToHost()
	{
		return lastPingMs;
	}

	protected override void connectImpl()
	{
		client = new GalaxySocket(Multiplayer.protocolVersion);
		GalaxyInstance.User().GetGalaxyID();
		client.JoinLobby(lobbyId, onReceiveError);
	}

	public override void disconnect(bool neatly = true)
	{
		if (client != null)
		{
			Game1.log.Verbose("Disconnecting from server " + lobbyId);
			client.Close();
			client = null;
			connectionMessage = null;
		}
	}

	protected override void receiveMessagesImpl()
	{
		if (client == null || !client.Connected)
		{
			return;
		}
		if (client.Connected && serverId == null)
		{
			Game1.log.Verbose("Connected to server " + lobbyId);
			serverId = client.LobbyOwner;
			if (GalaxyInstance.User().IsUserDataAvailable(serverId))
			{
				onProfileDataReady(serverId);
			}
			else
			{
				hostDisplayName = GalaxyNetHelper.TryGetHostSteamDisplayName(lobbyId);
				galaxySpecificUserDataListener = new GalaxySpecificUserDataListener(onProfileDataReady);
				GalaxyInstance.User().RequestUserData(serverId);
			}
		}
		client.Receive(onReceiveConnection, onReceiveMessage, onReceiveDisconnect, onReceiveError);
		if (client != null)
		{
			client.Heartbeat(Enumerable.Repeat(serverId, 1));
			lastPingMs = client.GetPingWith(serverId);
			if (lastPingMs > 30000f)
			{
				timedOut = true;
				pendingDisconnect = Multiplayer.DisconnectType.GalaxyTimeout;
				disconnect();
			}
		}
	}

	protected virtual void onReceiveConnection(GalaxyID peer)
	{
	}

	protected virtual void onReceiveMessage(GalaxyID peer, Stream messageStream)
	{
		if (peer != serverId)
		{
			return;
		}
		bandwidthLogger?.RecordBytesDown(messageStream.Length);
		IncomingMessage message = new IncomingMessage();
		try
		{
			using BinaryReader reader = new BinaryReader(messageStream);
			message.Read(reader);
			base.OnProcessingMessage(message, sendMessageImpl, delegate
			{
				processIncomingMessage(message);
			});
		}
		finally
		{
			if (message != null)
			{
				((IDisposable)message).Dispose();
			}
		}
	}

	protected virtual void onReceiveDisconnect(GalaxyID peer)
	{
		if (peer != serverId)
		{
			Game1.multiplayer.playerDisconnected((long)peer.ToUint64());
			return;
		}
		timedOut = true;
		pendingDisconnect = Multiplayer.DisconnectType.HostLeft;
	}

	protected virtual void onReceiveError(string message)
	{
		connectionMessage = message;
	}

	protected virtual void sendMessageImpl(OutgoingMessage message)
	{
		if (client == null || !client.Connected || serverId == null)
		{
			return;
		}
		if (bandwidthLogger != null)
		{
			using (MemoryStream stream = new MemoryStream())
			{
				using BinaryWriter writer = new BinaryWriter(stream);
				message.Write(writer);
				stream.Seek(0L, SeekOrigin.Begin);
				byte[] bytes = stream.ToArray();
				client.Send(serverId, bytes);
				bandwidthLogger.RecordBytesUp(bytes.Length);
				return;
			}
		}
		client.Send(serverId, message);
	}

	public override void sendMessage(OutgoingMessage message)
	{
		base.OnSendingMessage(message, sendMessageImpl, delegate
		{
			sendMessageImpl(message);
		});
	}
}
