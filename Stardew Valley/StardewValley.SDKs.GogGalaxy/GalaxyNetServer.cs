using System;
using System.Collections.Generic;
using System.IO;
using Galaxy.Api;
using StardewValley.Network;
using StardewValley.SDKs.GogGalaxy.Listeners;

namespace StardewValley.SDKs.GogGalaxy;

public class GalaxyNetServer : HookableServer
{
	private GalaxyID host;

	protected GalaxySocket server;

	private GalaxySpecificUserDataListener galaxySpecificUserDataListener;

	protected Bimap<long, ulong> peers = new Bimap<long, ulong>();

	/// <summary>A mapping of raw GalaxyIDs to custom display names.</summary>
	protected Dictionary<ulong, string> displayNames = new Dictionary<ulong, string>();

	public override int connectionsCount
	{
		get
		{
			if (server == null)
			{
				return 0;
			}
			return server.ConnectionCount;
		}
	}

	public GalaxyNetServer(IGameServer gameServer)
		: base(gameServer)
	{
	}

	public override string getUserId(long farmerId)
	{
		if (!peers.ContainsLeft(farmerId))
		{
			return null;
		}
		return peers[farmerId].ToString();
	}

	public override bool hasUserId(string userId)
	{
		foreach (ulong rightValue in peers.RightValues)
		{
			if (rightValue.ToString().Equals(userId))
			{
				return true;
			}
		}
		return false;
	}

	public override bool isConnectionActive(string connection_id)
	{
		foreach (GalaxyID connection in server.Connections)
		{
			if (getConnectionId(connection) == connection_id && connection.IsValid())
			{
				return true;
			}
		}
		return false;
	}

	public override string getUserName(long farmerId)
	{
		if (!peers.ContainsLeft(farmerId))
		{
			return null;
		}
		ulong peerId = peers[farmerId];
		if (displayNames.TryGetValue(peerId, out var displayName))
		{
			return displayName;
		}
		GalaxyID user = new GalaxyID(peerId);
		return GalaxyInstance.Friends().GetFriendPersonaName(user);
	}

	public override float getPingToClient(long farmerId)
	{
		if (!peers.ContainsLeft(farmerId))
		{
			return -1f;
		}
		GalaxyID user = new GalaxyID(peers[farmerId]);
		return server.GetPingWith(user);
	}

	public override void setPrivacy(ServerPrivacy privacy)
	{
		server.SetPrivacy(privacy);
	}

	public override bool connected()
	{
		return server.Connected;
	}

	public override string getInviteCode()
	{
		return server.GetInviteCode();
	}

	public override void initialize()
	{
		Game1.log.Verbose("Starting Galaxy server");
		host = GalaxyInstance.User().GetGalaxyID();
		galaxySpecificUserDataListener = new GalaxySpecificUserDataListener(onProfileDataReady);
		server = new GalaxySocket(Multiplayer.protocolVersion);
		server.CreateLobby(Game1.options.serverPrivacy, (uint)(Game1.multiplayer.playerLimit * 2));
	}

	public override void stopServer()
	{
		Game1.log.Verbose("Stopping Galaxy server");
		server.Close();
		galaxySpecificUserDataListener?.Dispose();
		galaxySpecificUserDataListener = null;
	}

	private void onProfileDataReady(GalaxyID userID)
	{
		if (!(userID == host) && !displayNames.ContainsKey(userID.ToUint64()))
		{
			string displayName = null;
			try
			{
				displayName = GalaxyInstance.User().GetUserData("StardewDisplayName", userID);
			}
			catch (Exception)
			{
			}
			if (!string.IsNullOrEmpty(displayName))
			{
				displayNames[userID.ToUint64()] = displayName;
				Game1.log.Verbose($"{userID} ({displayName}) connected");
			}
			else
			{
				Game1.log.Verbose(userID?.ToString() + " connected");
			}
			onConnect(getConnectionId(userID));
			gameServer.sendAvailableFarmhands(createUserID(userID), getConnectionId(userID), delegate(OutgoingMessage msg)
			{
				sendMessage(userID, msg);
			});
		}
	}

	public override void receiveMessages()
	{
		if (server == null)
		{
			return;
		}
		server.Receive(onReceiveConnection, onReceiveMessage, onReceiveDisconnect, onReceiveError);
		server.Heartbeat(server.LobbyMembers());
		foreach (GalaxyID client in server.Connections)
		{
			if (server.GetPingWith(client) > 30000)
			{
				server.Kick(client);
			}
		}
		bandwidthLogger?.Update();
	}

	public override void kick(long disconnectee)
	{
		base.kick(disconnectee);
		if (peers.ContainsLeft(disconnectee))
		{
			GalaxyID user = new GalaxyID(peers[disconnectee]);
			server.Kick(user);
			sendMessage(user, new OutgoingMessage(23, Game1.player));
		}
	}

	public string getConnectionId(GalaxyID peer)
	{
		return "GN_" + Convert.ToString(peer.ToUint64());
	}

	private string createUserID(GalaxyID peer)
	{
		return Convert.ToString(peer.ToUint64());
	}

	protected virtual void onReceiveConnection(GalaxyID peer)
	{
		if (!gameServer.isUserBanned(peer.ToString()))
		{
			if (GalaxyInstance.User().IsUserDataAvailable(peer))
			{
				onProfileDataReady(peer);
			}
			else
			{
				GalaxyInstance.User().RequestUserData(peer);
			}
		}
	}

	protected virtual void onReceiveMessage(GalaxyID peer, Stream messageStream)
	{
		bandwidthLogger?.RecordBytesDown(messageStream.Length);
		IncomingMessage message = new IncomingMessage();
		try
		{
			using BinaryReader reader = new BinaryReader(messageStream);
			message.Read(reader);
			base.OnProcessingMessage(message, delegate(OutgoingMessage outgoing)
			{
				sendMessage(peer, outgoing);
			}, delegate
			{
				if (peers.ContainsLeft(message.FarmerID) && peers[message.FarmerID] == peer.ToUint64())
				{
					gameServer.processIncomingMessage(message);
				}
				else if (message.MessageType == 2)
				{
					NetFarmerRoot farmer = Game1.multiplayer.readFarmer(message.Reader);
					GalaxyID capturedPeer = new GalaxyID(peer.ToUint64());
					gameServer.checkFarmhandRequest(createUserID(peer), getConnectionId(peer), farmer, delegate(OutgoingMessage msg)
					{
						sendMessage(capturedPeer, msg);
					}, delegate
					{
						peers[farmer.Value.UniqueMultiplayerID] = capturedPeer.ToUint64();
					});
				}
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

	public virtual void onReceiveDisconnect(GalaxyID peer)
	{
		Game1.log.Verbose(peer?.ToString() + " disconnected");
		onDisconnect(getConnectionId(peer));
		if (peers.ContainsRight(peer.ToUint64()))
		{
			playerDisconnected(peers[peer.ToUint64()]);
		}
		if (displayNames.ContainsKey(peer.ToUint64()))
		{
			displayNames.Remove(peer.ToUint64());
		}
	}

	protected virtual void onReceiveError(string messageKey)
	{
		Game1.log.Error("Server error: " + Game1.content.LoadString(messageKey));
	}

	public override void playerDisconnected(long disconnectee)
	{
		base.playerDisconnected(disconnectee);
		peers.RemoveLeft(disconnectee);
	}

	public override void sendMessage(long peerId, OutgoingMessage message)
	{
		if (peers.ContainsLeft(peerId))
		{
			sendMessage(new GalaxyID(peers[peerId]), message);
		}
	}

	protected virtual void sendMessage(GalaxyID peer, OutgoingMessage message)
	{
		if (bandwidthLogger != null)
		{
			using (MemoryStream stream = new MemoryStream())
			{
				using BinaryWriter writer = new BinaryWriter(stream);
				message.Write(writer);
				stream.Seek(0L, SeekOrigin.Begin);
				byte[] bytes = stream.ToArray();
				server.Send(peer, bytes);
				bandwidthLogger.RecordBytesUp(bytes.Length);
				return;
			}
		}
		server.Send(peer, message);
	}

	public override void setLobbyData(string key, string value)
	{
		server.SetLobbyData(key, value);
	}
}
