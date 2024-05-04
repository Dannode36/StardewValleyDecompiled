using System.Collections.Generic;
using StardewValley.Network.ChestHit.Internal;
using StardewValley.Objects;

namespace StardewValley.Network.ChestHit;

/// <summary>Prevents race conditions when multiplayer players hit a chest.</summary>
public sealed class ChestHitSynchronizer
{
	/// <summary>The queue of chest hit events to process during the next <see cref="M:StardewValley.Network.ChestHit.ChestHitSynchronizer.Update" />, maintained by the host.</summary>
	private readonly Queue<ChestHitArgs> EventQueue = new Queue<ChestHitArgs>();

	/// <summary>A map of chests and their first tool hit timers, maintained by each farmhand.</summary>
	internal readonly Dictionary<string, Dictionary<ulong, ChestHitTimer>> SavedTimers = new Dictionary<string, Dictionary<ulong, ChestHitTimer>>();

	/// <summary>Resets the chest hit synchronizer, used in <see cref="M:StardewValley.FarmerTeam.NewDay" />.</summary>
	public void Reset()
	{
		EventQueue.Clear();
		SavedTimers.Clear();
	}

	/// <summary>Processes all of the chest hit events in <see cref="F:StardewValley.Network.ChestHit.ChestHitSynchronizer.EventQueue" />, used in <see cref="M:StardewValley.FarmerTeam.Update" />.</summary>
	public void Update()
	{
		if (!Game1.IsMasterGame)
		{
			return;
		}
		while (EventQueue.Count > 0)
		{
			ChestHitArgs args = EventQueue.Dequeue();
			if (args != null)
			{
				if (args.Location?.getObjectAtTile(args.ChestTile.X, args.ChestTile.Y, ignorePassables: true) is Chest chest)
				{
					chest.HandleChestHit(args);
				}
				continue;
			}
			break;
		}
	}

	/// <summary>Synchronizes a player hitting a chest.</summary>
	/// <param name="args">The arguments for the chest hit event.</param>
	public void Sync(ChestHitArgs args)
	{
		if (!(args.Location?.getObjectAtTile(args.ChestTile.X, args.ChestTile.Y, ignorePassables: true) is Chest chest))
		{
			return;
		}
		if (Game1.IsMasterGame)
		{
			EventQueue.Enqueue(args);
			return;
		}
		if (chest.hitTimerInstance != null)
		{
			chest.hitTimerInstance.SavedTime = (int)(Game1.currentGameTime?.TotalGameTime.TotalMilliseconds ?? (-999.0));
			if (!SavedTimers.TryGetValue(args.Location.NameOrUniqueName, out var localTimers))
			{
				localTimers = new Dictionary<ulong, ChestHitTimer>();
				SavedTimers.Add(args.Location.NameOrUniqueName, localTimers);
			}
			localTimers[HashPosition(args.ChestTile.X, args.ChestTile.Y)] = chest.hitTimerInstance;
		}
		Game1.client?.sendMessage(new OutgoingMessage(32, Game1.player, (byte)0, args.Location.isStructure.Value, args.Location.NameOrUniqueName, args.ChestTile.X, args.ChestTile.Y, args.ToolPosition, args.StandingPixel.X, args.StandingPixel.Y, args.Direction, args.HoldDownClick, args.ToolCanHit, args.RecentlyHit));
	}

	/// <summary>Signals that a chest has been moved.</summary>
	/// <param name="location">The parent location of the moved chest.</param>
	/// <param name="sourceTileX">The old, pre-move x-coordinate of the chest.</param>
	/// <param name="sourceTileY">The old, pre-move y-coordinate of the chest.</param>
	/// <param name="destTileX">The new, post-move x-coordinate of the chest.</param>
	/// <param name="destTileY">The new, post-move y-coordinate of the chest.</param>
	public void SignalMove(GameLocation location, int sourceTileX, int sourceTileY, int destTileX, int destTileY)
	{
		if (Game1.server == null || location == null)
		{
			return;
		}
		foreach (Farmer farmer in Game1.otherFarmers.Values)
		{
			Game1.server.sendMessage(farmer.UniqueMultiplayerID, new OutgoingMessage(32, Game1.player, (byte)1, location.NameOrUniqueName, sourceTileX, sourceTileY, destTileX, destTileY));
		}
	}

	/// <summary>Signals that a chest has been deleted.</summary>
	/// <param name="location">The parent location of the deleted chest.</param>
	/// <param name="tileX">The x-coordinate of the deleted chest.</param>
	/// <param name="tileY">The y-coordinate of the deleted chest.</param>
	public void SignalDelete(GameLocation location, int tileX, int tileY)
	{
		if (Game1.server == null || location == null)
		{
			return;
		}
		foreach (Farmer farmer in Game1.otherFarmers.Values)
		{
			Game1.server.sendMessage(farmer.UniqueMultiplayerID, new OutgoingMessage(32, Game1.player, (byte)2, location.NameOrUniqueName, tileX, tileY));
		}
	}

	/// <summary>Process an incoming chest hit sync message.</summary>
	/// <param name="message">The incoming sync message.</param>
	public void ProcessMessage(IncomingMessage message)
	{
		switch ((ChestHitMessageType)message.Reader.ReadByte())
		{
		case ChestHitMessageType.Sync:
			ProcessSync(message);
			break;
		case ChestHitMessageType.Move:
			ProcessMove(message);
			break;
		case ChestHitMessageType.Delete:
			ProcessDelete(message);
			break;
		}
	}

	/// <summary>Packs the two integer components of the tile coordinate into an unsigned long.</summary>
	/// <param name="x">The x-component of the tile coordinate.</param>
	/// <param name="y">The y-component of the tile coordinate.</param>
	internal static ulong HashPosition(int x, int y)
	{
		return ((ulong)(uint)x << 32) | (uint)y;
	}

	private static GameLocation ReadLocation(IncomingMessage message)
	{
		bool isStructure = message.Reader.ReadBoolean();
		GameLocation location = Game1.getLocationFromName(message.Reader.ReadString(), isStructure);
		if (location == null || (object)Game1.multiplayer.locationRoot(location) == null)
		{
			return null;
		}
		return location;
	}

	/// <summary>Handle a request to synchronize a player hitting a chest.</summary>
	/// <param name="message">The incoming <see cref="F:StardewValley.Network.ChestHit.Internal.ChestHitMessageType.Sync" /> message.</param>
	private void ProcessSync(IncomingMessage message)
	{
		if (!Game1.IsMasterGame)
		{
			Game1.log.Warn("Unexpectedly received a chest hit sync message as a farmhand.");
			return;
		}
		ChestHitArgs args = new ChestHitArgs();
		bool isStructure = message.Reader.ReadBoolean();
		string locationName = message.Reader.ReadString();
		args.Location = Game1.getLocationFromName(locationName, isStructure);
		if (args.Location != null && (object)Game1.multiplayer.locationRoot(args.Location) != null)
		{
			args.ChestTile.X = message.Reader.ReadInt32();
			args.ChestTile.Y = message.Reader.ReadInt32();
			args.ToolPosition.X = message.Reader.ReadSingle();
			args.ToolPosition.Y = message.Reader.ReadSingle();
			args.StandingPixel.X = message.Reader.ReadInt32();
			args.StandingPixel.Y = message.Reader.ReadInt32();
			args.Direction = message.Reader.ReadInt32();
			args.HoldDownClick = message.Reader.ReadBoolean();
			args.ToolCanHit = message.Reader.ReadBoolean();
			args.RecentlyHit = message.Reader.ReadBoolean();
			EventQueue.Enqueue(args);
		}
	}

	/// <summary>Handle an incoming signal that a chest has moved.</summary>
	/// <param name="message">The incoming <see cref="F:StardewValley.Network.ChestHit.Internal.ChestHitMessageType.Move" /> message.</param>
	private void ProcessMove(IncomingMessage message)
	{
		if (Game1.IsMasterGame)
		{
			Game1.log.Warn("Unexpectedly received a chest move message as the host.");
			return;
		}
		string locationName = message.Reader.ReadString();
		if (locationName == null)
		{
			return;
		}
		int sourceTileX = message.Reader.ReadInt32();
		int sourceTileY = message.Reader.ReadInt32();
		int destTileX = message.Reader.ReadInt32();
		int destTileY = message.Reader.ReadInt32();
		if (SavedTimers.TryGetValue(locationName, out var localTimers))
		{
			ulong sourceTile = HashPosition(sourceTileX, sourceTileY);
			if (localTimers.TryGetValue(sourceTile, out var timer))
			{
				localTimers.Remove(sourceTile);
				localTimers.TryAdd(HashPosition(destTileX, destTileY), timer);
			}
		}
	}

	/// <summary>Handle an incoming signal that a chest has been deleted.</summary>
	/// <param name="message">The incoming <see cref="F:StardewValley.Network.ChestHit.Internal.ChestHitMessageType.Delete" /> message.</param>
	private void ProcessDelete(IncomingMessage message)
	{
		if (Game1.IsMasterGame)
		{
			Game1.log.Warn("Unexpectedly received a chest delete message as the host.");
			return;
		}
		string locationName = message.Reader.ReadString();
		if (locationName != null)
		{
			int deleteTileX = message.Reader.ReadInt32();
			int deleteTileY = message.Reader.ReadInt32();
			if (SavedTimers.TryGetValue(locationName, out var localTimers))
			{
				localTimers.Remove(HashPosition(deleteTileX, deleteTileY));
			}
		}
	}
}
