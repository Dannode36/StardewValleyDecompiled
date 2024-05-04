using System;
using Lidgren.Network;

namespace StardewValley.Network;

public class LidgrenClient : HookableClient
{
	public string address;

	public NetClient client;

	private bool serverDiscovered;

	private int maxRetryAttempts;

	private int retryMs = 10000;

	private double lastAttemptMs;

	private int retryAttempts;

	private float lastLatencyMs;

	public LidgrenClient(string address)
	{
		this.address = address;
	}

	public override string getUserID()
	{
		return "";
	}

	public override float GetPingToHost()
	{
		return lastLatencyMs / 2f;
	}

	protected override string getHostUserName()
	{
		return client.ServerConnection.RemoteEndPoint.Address.ToString();
	}

	protected override void connectImpl()
	{
		NetPeerConfiguration config = new NetPeerConfiguration("StardewValley");
		config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
		config.EnableMessageType(NetIncomingMessageType.ConnectionLatencyUpdated);
		config.ConnectionTimeout = 30f;
		config.PingInterval = 5f;
		config.MaximumTransmissionUnit = 1200;
		client = new NetClient(config);
		client.Start();
		attemptConnection();
	}

	private void attemptConnection()
	{
		int port = 24642;
		if (address.Contains(':'))
		{
			string[] split = address.Split(':');
			address = split[0];
			port = Convert.ToInt32(split[1]);
		}
		client.DiscoverKnownPeer(address, port);
		lastAttemptMs = DateTime.UtcNow.TimeOfDay.TotalMilliseconds;
	}

	public override void disconnect(bool neatly = true)
	{
		if (client == null)
		{
			return;
		}
		if (client.ConnectionStatus != NetConnectionStatus.Disconnected && client.ConnectionStatus != NetConnectionStatus.Disconnecting)
		{
			if (neatly)
			{
				sendMessage(new OutgoingMessage(19, Game1.player));
			}
			client.FlushSendQueue();
			client.Disconnect("");
			client.FlushSendQueue();
		}
		connectionMessage = null;
	}

	protected virtual bool validateProtocol(string version)
	{
		return version == Multiplayer.protocolVersion;
	}

	protected override void receiveMessagesImpl()
	{
		if (client != null && !serverDiscovered && DateTime.UtcNow.TimeOfDay.TotalMilliseconds >= lastAttemptMs + (double)retryMs && retryAttempts < maxRetryAttempts)
		{
			attemptConnection();
			retryAttempts++;
		}
		NetIncomingMessage inc;
		while ((inc = client.ReadMessage()) != null)
		{
			switch (inc.MessageType)
			{
			case NetIncomingMessageType.ConnectionLatencyUpdated:
				readLatency(inc);
				break;
			case NetIncomingMessageType.DiscoveryResponse:
				if (!serverDiscovered)
				{
					Game1.log.Verbose("Found server at " + inc.SenderEndPoint);
					string protocolVersion = inc.ReadString();
					if (validateProtocol(protocolVersion))
					{
						serverName = inc.ReadString();
						receiveHandshake(inc);
						serverDiscovered = true;
						break;
					}
					Game1.log.Warn($"Failed to connect. The server's protocol ({protocolVersion}) does not match our own ({Multiplayer.protocolVersion}).");
					connectionMessage = Game1.content.LoadString("Strings\\UI:CoopMenu_FailedProtocolVersion");
					client.Disconnect("");
				}
				break;
			case NetIncomingMessageType.Data:
				parseDataMessageFromServer(inc);
				break;
			case NetIncomingMessageType.DebugMessage:
			case NetIncomingMessageType.WarningMessage:
			case NetIncomingMessageType.ErrorMessage:
			{
				string message = inc.ReadString();
				Game1.log.Verbose(inc.MessageType.ToString() + ": " + message);
				Game1.debugOutput = message;
				break;
			}
			case NetIncomingMessageType.StatusChanged:
				statusChanged(inc);
				break;
			}
		}
	}

	private void readLatency(NetIncomingMessage msg)
	{
		lastLatencyMs = msg.ReadFloat() * 1000f;
	}

	private void receiveHandshake(NetIncomingMessage msg)
	{
		client.Connect(msg.SenderEndPoint.Address.ToString(), msg.SenderEndPoint.Port);
	}

	private void statusChanged(NetIncomingMessage message)
	{
		NetConnectionStatus status = (NetConnectionStatus)message.ReadByte();
		if (status == NetConnectionStatus.Disconnected || status == NetConnectionStatus.Disconnecting)
		{
			string byeMessage = message.ReadString();
			clientRemotelyDisconnected(status, byeMessage);
		}
	}

	private void clientRemotelyDisconnected(NetConnectionStatus status, string message)
	{
		timedOut = true;
		if (status == NetConnectionStatus.Disconnected)
		{
			if (message == Multiplayer.kicked)
			{
				pendingDisconnect = Multiplayer.DisconnectType.Kicked;
			}
			else
			{
				pendingDisconnect = Multiplayer.DisconnectType.LidgrenTimeout;
			}
		}
		else
		{
			pendingDisconnect = Multiplayer.DisconnectType.LidgrenDisconnect_Unknown;
		}
	}

	protected virtual void sendMessageImpl(OutgoingMessage message)
	{
		NetOutgoingMessage sendMsg = client.CreateMessage();
		LidgrenMessageUtils.WriteMessage(message, sendMsg);
		client.SendMessage(sendMsg, NetDeliveryMethod.ReliableOrdered);
		bandwidthLogger?.RecordBytesUp(sendMsg.LengthBytes);
	}

	public override void sendMessage(OutgoingMessage message)
	{
		base.OnSendingMessage(message, sendMessageImpl, delegate
		{
			sendMessageImpl(message);
		});
	}

	private void parseDataMessageFromServer(NetIncomingMessage dataMsg)
	{
		bandwidthLogger?.RecordBytesDown(dataMsg.LengthBytes);
		IncomingMessage message = new IncomingMessage();
		try
		{
			using NetBufferReadStream stream = new NetBufferReadStream(dataMsg);
			while (dataMsg.LengthBits - dataMsg.Position >= 8)
			{
				LidgrenMessageUtils.ReadStreamToMessage(stream, message);
				base.OnProcessingMessage(message, sendMessageImpl, delegate
				{
					processIncomingMessage(message);
				});
			}
		}
		finally
		{
			if (message != null)
			{
				((IDisposable)message).Dispose();
			}
		}
	}
}
