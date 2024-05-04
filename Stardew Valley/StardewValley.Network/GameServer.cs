using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.SDKs.Steam;

namespace StardewValley.Network;

public class GameServer : IGameServer, IBandwidthMonitor
{
	internal List<Server> servers = new List<Server>();

	private Dictionary<Action, Func<bool>> pendingGameAvailableActions = new Dictionary<Action, Func<bool>>();

	/// <summary>A set of connections that are waiting to receive the list of available farmhands.</summary>
	private readonly HashSet<string> pendingAvailableFarmhands = new HashSet<string>();

	private List<Action> completedPendingActions = new List<Action>();

	private List<string> bannedUsers = new List<string>();

	protected bool _wasConnected;

	protected bool _isLocalMultiplayerInitiatedServer;

	public int connectionsCount => servers.Sum((Server s) => s.connectionsCount);

	public BandwidthLogger BandwidthLogger
	{
		get
		{
			foreach (Server server in servers)
			{
				if (server.connectionsCount > 0)
				{
					return server.BandwidthLogger;
				}
			}
			return null;
		}
	}

	public bool LogBandwidth
	{
		get
		{
			foreach (Server server in servers)
			{
				if (server.connectionsCount > 0)
				{
					return server.LogBandwidth;
				}
			}
			return false;
		}
		set
		{
			foreach (Server server in servers)
			{
				if (server.connectionsCount > 0)
				{
					server.LogBandwidth = value;
					break;
				}
			}
		}
	}

	public GameServer(bool local_multiplayer = false)
	{
		servers.Add(Game1.multiplayer.InitServer(new LidgrenServer(this)));
		_isLocalMultiplayerInitiatedServer = local_multiplayer;
		if (!_isLocalMultiplayerInitiatedServer && Program.sdk.Networking != null)
		{
			if (Program.sdk.Networking is SteamNetHelper steamNetworking)
			{
				servers.Add(steamNetworking.CreateSteamServer(this));
			}
			Server sdkServer = Program.sdk.Networking.CreateServer(this);
			if (sdkServer != null)
			{
				servers.Add(sdkServer);
			}
		}
	}

	public bool isConnectionActive(string connectionId)
	{
		foreach (Server server in servers)
		{
			if (server.isConnectionActive(connectionId))
			{
				return true;
			}
		}
		return false;
	}

	public virtual void onConnect(string connectionID)
	{
		UpdateLocalOnlyFlag();
	}

	public virtual void onDisconnect(string connectionID)
	{
		UpdateLocalOnlyFlag();
	}

	public bool IsLocalMultiplayerInitiatedServer()
	{
		return _isLocalMultiplayerInitiatedServer;
	}

	public virtual void UpdateLocalOnlyFlag()
	{
		if (!Game1.game1.IsMainInstance)
		{
			return;
		}
		bool local_only = true;
		HashSet<long> local_clients = new HashSet<long>();
		GameRunner.instance.ExecuteForInstances(delegate
		{
			Client client = Game1.client;
			if (client == null && Game1.activeClickableMenu is FarmhandMenu farmhandMenu)
			{
				client = farmhandMenu.client;
			}
			if (client is LidgrenClient lidgrenClient)
			{
				local_clients.Add(lidgrenClient.client.UniqueIdentifier);
			}
		});
		foreach (Server server in servers)
		{
			if (server is LidgrenServer lidgren_server)
			{
				foreach (NetConnection connection in lidgren_server.server.Connections)
				{
					if (!local_clients.Contains(connection.RemoteUniqueIdentifier))
					{
						local_only = false;
						break;
					}
				}
			}
			else if (server.connectionsCount > 0)
			{
				local_only = false;
				break;
			}
			if (!local_only)
			{
				break;
			}
		}
		if (Game1.hasLocalClientsOnly != local_only)
		{
			Game1.hasLocalClientsOnly = local_only;
			if (Game1.hasLocalClientsOnly)
			{
				Game1.log.Verbose("Game has only local clients.");
			}
			else
			{
				Game1.log.Verbose("Game has remote clients.");
			}
		}
	}

	public string getInviteCode()
	{
		foreach (Server server in servers)
		{
			string code = server.getInviteCode();
			if (code != null)
			{
				return code;
			}
		}
		return null;
	}

	public string getUserName(long farmerId)
	{
		foreach (Server server in servers)
		{
			string name = server.getUserName(farmerId);
			if (name != null)
			{
				return name;
			}
		}
		return null;
	}

	public float getPingToClient(long farmerId)
	{
		foreach (Server server in servers)
		{
			if (server.getPingToClient(farmerId) != -1f)
			{
				return server.getPingToClient(farmerId);
			}
		}
		return -1f;
	}

	protected void initialize()
	{
		foreach (Server server in servers)
		{
			server.initialize();
		}
		whenGameAvailable(updateLobbyData);
	}

	public void setPrivacy(ServerPrivacy privacy)
	{
		foreach (Server server in servers)
		{
			server.setPrivacy(privacy);
		}
		if (Game1.netWorldState != null && Game1.netWorldState.Value != null)
		{
			Game1.netWorldState.Value.ServerPrivacy = privacy;
		}
	}

	public void stopServer()
	{
		if (Game1.chatBox != null)
		{
			Game1.chatBox.addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_DisablingServer"));
		}
		foreach (Server server in servers)
		{
			server.stopServer();
		}
	}

	public void receiveMessages()
	{
		foreach (Server server in servers)
		{
			server.receiveMessages();
		}
		completedPendingActions.Clear();
		foreach (Action action in pendingGameAvailableActions.Keys)
		{
			if (pendingGameAvailableActions[action]())
			{
				action();
				completedPendingActions.Add(action);
			}
		}
		foreach (Action action in completedPendingActions)
		{
			pendingGameAvailableActions.Remove(action);
		}
		completedPendingActions.Clear();
		if (Game1.chatBox == null)
		{
			return;
		}
		bool any_server_connected = anyServerConnected();
		if (_wasConnected != any_server_connected)
		{
			_wasConnected = any_server_connected;
			if (_wasConnected)
			{
				Game1.chatBox.addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_StartingServer"));
			}
		}
	}

	public void sendMessage(long peerId, OutgoingMessage message)
	{
		foreach (Server server in servers)
		{
			server.sendMessage(peerId, message);
		}
	}

	public bool canAcceptIPConnections()
	{
		return servers.Select((Server s) => s.canAcceptIPConnections()).Aggregate(seed: false, (bool a, bool b) => a || b);
	}

	public bool canOfferInvite()
	{
		return servers.Select((Server s) => s.canOfferInvite()).Aggregate(seed: false, (bool a, bool b) => a || b);
	}

	public void offerInvite()
	{
		foreach (Server s in servers)
		{
			if (s.canOfferInvite())
			{
				s.offerInvite();
			}
		}
	}

	public bool anyServerConnected()
	{
		foreach (Server server in servers)
		{
			if (server.connected())
			{
				return true;
			}
		}
		return false;
	}

	public bool connected()
	{
		foreach (Server server in servers)
		{
			if (!server.connected())
			{
				return false;
			}
		}
		return true;
	}

	public void sendMessage(long peerId, byte messageType, Farmer sourceFarmer, params object[] data)
	{
		sendMessage(peerId, new OutgoingMessage(messageType, sourceFarmer, data));
	}

	public void sendMessages()
	{
		foreach (Farmer farmer in Game1.otherFarmers.Values)
		{
			foreach (OutgoingMessage message in farmer.messageQueue)
			{
				sendMessage(farmer.UniqueMultiplayerID, message);
			}
			farmer.messageQueue.Clear();
		}
	}

	public void startServer()
	{
		_wasConnected = false;
		Game1.log.Verbose("Starting server. Protocol version: " + Multiplayer.protocolVersion);
		initialize();
		if (Game1.netWorldState == null)
		{
			Game1.netWorldState = new NetRoot<NetWorldState>(new NetWorldState());
		}
		Game1.netWorldState.Clock.InterpolationTicks = 0;
		Game1.netWorldState.Value.UpdateFromGame1();
	}

	public void initializeHost()
	{
		if (Game1.serverHost == null)
		{
			Game1.serverHost = new NetFarmerRoot();
		}
		Game1.serverHost.Value = Game1.player;
		using (List<Server>.Enumerator enumerator = servers.GetEnumerator())
		{
			while (enumerator.MoveNext() && !enumerator.Current.PopulatePlatformData(Game1.player))
			{
			}
		}
		Game1.serverHost.MarkClean();
		Game1.serverHost.Clock.InterpolationTicks = Game1.multiplayer.defaultInterpolationTicks;
	}

	public void sendServerIntroduction(long peer)
	{
		sendMessage(peer, new OutgoingMessage(1, Game1.serverHost.Value, Game1.multiplayer.writeObjectFullBytes(Game1.serverHost, peer), Game1.multiplayer.writeObjectFullBytes(Game1.player.teamRoot, peer), Game1.multiplayer.writeObjectFullBytes(Game1.netWorldState, peer)));
		foreach (KeyValuePair<long, NetRoot<Farmer>> r in Game1.otherFarmers.Roots)
		{
			if (r.Key != Game1.player.UniqueMultiplayerID && r.Key != peer)
			{
				sendMessage(peer, new OutgoingMessage(2, r.Value.Value, getUserName(r.Value.Value.UniqueMultiplayerID), Game1.multiplayer.writeObjectFullBytes(r.Value, peer)));
			}
		}
	}

	public void kick(long disconnectee)
	{
		foreach (Server server in servers)
		{
			server.kick(disconnectee);
		}
	}

	public string ban(long farmerId)
	{
		string userId = null;
		foreach (Server server in servers)
		{
			userId = server.getUserId(farmerId);
			if (userId != null)
			{
				break;
			}
		}
		if (userId != null && !Game1.bannedUsers.ContainsKey(userId))
		{
			string userName = Game1.multiplayer.getUserName(farmerId);
			if (userName == "" || userName == userId)
			{
				userName = null;
			}
			Game1.bannedUsers.Add(userId, userName);
			kick(farmerId);
			return userId;
		}
		return null;
	}

	public void playerDisconnected(long disconnectee)
	{
		Game1.otherFarmers.TryGetValue(disconnectee, out var disconnectedFarmer);
		Game1.multiplayer.playerDisconnected(disconnectee);
		if (disconnectedFarmer == null)
		{
			return;
		}
		OutgoingMessage message = new OutgoingMessage(19, disconnectedFarmer);
		foreach (long peer in Game1.otherFarmers.Keys)
		{
			if (peer != disconnectee)
			{
				sendMessage(peer, message);
			}
		}
	}

	public bool isGameAvailable()
	{
		bool inIntro = Game1.currentMinigame is Intro || Game1.Date.DayOfMonth == 0;
		bool isWedding = Game1.CurrentEvent != null && Game1.CurrentEvent.isWedding;
		bool isSleeping = Game1.newDaySync.hasInstance() && !Game1.newDaySync.hasFinished();
		bool isDemolishing = Game1.player.team.demolishLock.IsLocked();
		if (!Game1.isFestival() && !isWedding && !inIntro && !isSleeping && !isDemolishing && Game1.weddingsToday.Count == 0)
		{
			return Game1.gameMode != 6;
		}
		return false;
	}

	public bool whenGameAvailable(Action action, Func<bool> customAvailabilityCheck = null)
	{
		Func<bool> availabilityCheck = ((customAvailabilityCheck != null) ? customAvailabilityCheck : new Func<bool>(isGameAvailable));
		if (availabilityCheck())
		{
			action();
			return true;
		}
		pendingGameAvailableActions.Add(action, availabilityCheck);
		return false;
	}

	private void rejectFarmhandRequest(string userId, string connectionId, NetFarmerRoot farmer, Action<OutgoingMessage> sendMessage)
	{
		sendAvailableFarmhands(userId, connectionId, sendMessage);
		Game1.log.Verbose("Rejected request for farmhand " + ((farmer.Value != null) ? farmer.Value.UniqueMultiplayerID.ToString() : "???"));
	}

	public bool isUserBanned(string userID)
	{
		return Game1.bannedUsers.ContainsKey(userID);
	}

	private bool authCheck(string userID, Farmer farmhand)
	{
		if (!Game1.options.enableFarmhandCreation && !IsLocalMultiplayerInitiatedServer() && !farmhand.isCustomized)
		{
			return false;
		}
		if (!(userID == "") && !(farmhand.userID.Value == ""))
		{
			return farmhand.userID.Value == userID;
		}
		return true;
	}

	public bool IsFarmhandAvailable(Farmer farmhand)
	{
		if (!Game1.netWorldState.Value.TryAssignFarmhandHome(farmhand))
		{
			return false;
		}
		Cabin obj = Utility.getHomeOfFarmer(farmhand) as Cabin;
		if (obj != null && obj.isInventoryOpen())
		{
			return false;
		}
		return true;
	}

	public void checkFarmhandRequest(string userId, string connectionId, NetFarmerRoot farmer, Action<OutgoingMessage> sendMessage, Action approve)
	{
		if (farmer.Value == null)
		{
			rejectFarmhandRequest(userId, connectionId, farmer, sendMessage);
			return;
		}
		long id = farmer.Value.UniqueMultiplayerID;
		if (isGameAvailable())
		{
			Check();
		}
		else
		{
			sendAvailableFarmhands(userId, connectionId, sendMessage);
		}
		void Check()
		{
			Farmer originalFarmhand = Game1.netWorldState.Value.farmhandData[farmer.Value.UniqueMultiplayerID];
			if (!isConnectionActive(connectionId))
			{
				Game1.log.Verbose("Rejected request for connection ID " + connectionId + ": Connection not active.");
			}
			else if (originalFarmhand == null)
			{
				Game1.log.Verbose("Rejected request for farmhand " + id + ": doesn't exist");
				rejectFarmhandRequest(userId, connectionId, farmer, sendMessage);
			}
			else if (!authCheck(userId, originalFarmhand))
			{
				Game1.log.Verbose("Rejected request for farmhand " + id + ": authorization failure " + userId + " " + originalFarmhand.userID.Value);
				rejectFarmhandRequest(userId, connectionId, farmer, sendMessage);
			}
			else if ((Game1.otherFarmers.ContainsKey(id) && !Game1.multiplayer.isDisconnecting(id)) || Game1.serverHost.Value.UniqueMultiplayerID == id)
			{
				Game1.log.Verbose("Rejected request for farmhand " + id + ": already in use");
				rejectFarmhandRequest(userId, connectionId, farmer, sendMessage);
			}
			else if (!IsFarmhandAvailable(farmer.Value))
			{
				Game1.log.Verbose("Rejected request for farmhand " + id + ": farmhand availability failed");
				rejectFarmhandRequest(userId, connectionId, farmer, sendMessage);
			}
			else if (!Game1.netWorldState.Value.TryAssignFarmhandHome(farmer.Value))
			{
				Game1.log.Verbose("Rejected request for farmhand " + id + ": farmhand has no assigned cabin, and none is available to assign.");
				rejectFarmhandRequest(userId, connectionId, farmer, sendMessage);
			}
			else
			{
				Game1.log.Verbose("Approved request for farmhand " + id);
				approve();
				Game1.updateCellarAssignments();
				Game1.multiplayer.addPlayer(farmer);
				Game1.multiplayer.broadcastPlayerIntroduction(farmer);
				foreach (GameLocation location in Game1.locations)
				{
					if (Game1.multiplayer.isAlwaysActiveLocation(location))
					{
						sendLocation(id, location);
					}
				}
				if ((int)farmer.Value.disconnectDay == Game1.MasterPlayer.stats.DaysPlayed)
				{
					GameLocation disconnectLoc = Game1.getLocationFromName(farmer.Value.disconnectLocation);
					if (disconnectLoc != null && !Game1.multiplayer.isAlwaysActiveLocation(disconnectLoc))
					{
						sendLocation(id, disconnectLoc, force_current: true);
					}
				}
				else if (farmer.Value.lastSleepLocation != null)
				{
					GameLocation last_sleep_location = Game1.getLocationFromName(farmer.Value.lastSleepLocation);
					if (last_sleep_location != null && Game1.isLocationAccessible(last_sleep_location.Name) && !Game1.multiplayer.isAlwaysActiveLocation(last_sleep_location))
					{
						sendLocation(id, last_sleep_location, force_current: true);
					}
				}
				sendServerIntroduction(id);
				updateLobbyData();
			}
		}
	}

	public void sendAvailableFarmhands(string userId, string connectionId, Action<OutgoingMessage> sendMessage)
	{
		if (!isGameAvailable())
		{
			sendMessage(new OutgoingMessage(11, Game1.player, "Strings\\UI:Client_WaitForHostAvailability"));
			if (pendingAvailableFarmhands.Contains(connectionId))
			{
				Game1.log.Verbose("Connection " + connectionId + " is already waiting to receive available farmhands");
				return;
			}
			Game1.log.Verbose("Postponing sending available farmhands to connection ID " + connectionId);
			pendingAvailableFarmhands.Add(connectionId);
			whenGameAvailable(delegate
			{
				pendingAvailableFarmhands.Remove(connectionId);
				if (isConnectionActive(connectionId))
				{
					sendAvailableFarmhands(userId, connectionId, sendMessage);
				}
				else
				{
					Game1.log.Verbose("Failed to send available farmhands to connection ID " + connectionId + ": Connection not active.");
				}
			});
			return;
		}
		Game1.log.Verbose("Sending available farmhands to connection ID " + connectionId);
		List<NetRef<Farmer>> availableFarmhands = new List<NetRef<Farmer>>();
		foreach (NetRef<Farmer> farmhand in Game1.netWorldState.Value.farmhandData.FieldDict.Values)
		{
			if ((!farmhand.Value.isActive() || Game1.multiplayer.isDisconnecting(farmhand.Value.UniqueMultiplayerID)) && IsFarmhandAvailable(farmhand.Value))
			{
				availableFarmhands.Add(farmhand);
			}
		}
		using MemoryStream stream = new MemoryStream();
		using BinaryWriter writer = new BinaryWriter(stream);
		writer.Write(Game1.year);
		writer.Write(Game1.seasonIndex);
		writer.Write(Game1.dayOfMonth);
		writer.Write((byte)availableFarmhands.Count);
		foreach (NetRef<Farmer> farmhand in availableFarmhands)
		{
			try
			{
				farmhand.Serializer = SaveGame.farmerSerializer;
				farmhand.WriteFull(writer);
			}
			finally
			{
				farmhand.Serializer = null;
			}
		}
		stream.Seek(0L, SeekOrigin.Begin);
		sendMessage(new OutgoingMessage(9, Game1.player, stream.ToArray()));
	}

	public T GetServer<T>() where T : Server
	{
		foreach (Server server in servers)
		{
			if (server is T match)
			{
				return match;
			}
		}
		return null;
	}

	private void sendLocation(long peer, GameLocation location, bool force_current = false)
	{
		sendMessage(peer, 3, Game1.serverHost.Value, force_current, Game1.multiplayer.writeObjectFullBytes(Game1.multiplayer.locationRoot(location), peer));
	}

	private void warpFarmer(Farmer farmer, short x, short y, string name, bool isStructure)
	{
		GameLocation location = Game1.RequireLocation(name, isStructure);
		if (Game1.IsMasterGame)
		{
			location.hostSetup();
		}
		farmer.currentLocation = location;
		farmer.Position = new Vector2(x * 64, y * 64 - (farmer.Sprite.getHeight() - 32) + 16);
		sendLocation(farmer.UniqueMultiplayerID, location);
	}

	public void processIncomingMessage(IncomingMessage message)
	{
		switch (message.MessageType)
		{
		case 5:
		{
			short x = message.Reader.ReadInt16();
			short y = message.Reader.ReadInt16();
			string name = message.Reader.ReadString();
			bool isStructure = message.Reader.ReadByte() == 1;
			warpFarmer(message.SourceFarmer, x, y, name, isStructure);
			break;
		}
		case 2:
			message.Reader.ReadString();
			Game1.multiplayer.processIncomingMessage(message);
			break;
		case 10:
		{
			long recipient = message.Reader.ReadInt64();
			message.Reader.BaseStream.Position -= 8L;
			if (recipient == Multiplayer.AllPlayers || recipient == Game1.player.UniqueMultiplayerID)
			{
				Game1.multiplayer.processIncomingMessage(message);
			}
			rebroadcastClientMessage(message, recipient);
			break;
		}
		default:
			Game1.multiplayer.processIncomingMessage(message);
			break;
		}
		if (Game1.multiplayer.isClientBroadcastType(message.MessageType))
		{
			rebroadcastClientMessage(message, Multiplayer.AllPlayers);
		}
	}

	private void rebroadcastClientMessage(IncomingMessage message, long peerID)
	{
		OutgoingMessage outMessage = new OutgoingMessage(message);
		foreach (long peer in Game1.otherFarmers.Keys)
		{
			if (peer != message.FarmerID && (peerID == Multiplayer.AllPlayers || peer == peerID))
			{
				sendMessage(peer, outMessage);
			}
		}
	}

	private void setLobbyData(string key, string value)
	{
		foreach (Server server in servers)
		{
			server.setLobbyData(key, value);
		}
	}

	private bool unclaimedFarmhandsExist()
	{
		foreach (Farmer value in Game1.netWorldState.Value.farmhandData.Values)
		{
			if (value.userID.Value == "")
			{
				return true;
			}
		}
		return false;
	}

	public void updateLobbyData()
	{
		setLobbyData("farmName", Game1.player.farmName.Value);
		setLobbyData("farmType", Convert.ToString(Game1.whichFarm));
		if (Game1.whichFarm == 7)
		{
			setLobbyData("modFarmType", Game1.GetFarmTypeID());
		}
		else
		{
			setLobbyData("modFarmType", "");
		}
		WorldDate date = WorldDate.Now();
		setLobbyData("date", Convert.ToString(date.TotalDays));
		IEnumerable<string> farmhandUserIds = from farmhand in Game1.getAllFarmhands()
			select farmhand.userID.Value;
		setLobbyData("farmhands", string.Join(",", farmhandUserIds.Where((string user) => user != "")));
		setLobbyData("newFarmhands", Convert.ToString(Game1.options.enableFarmhandCreation && unclaimedFarmhandsExist()));
	}
}
