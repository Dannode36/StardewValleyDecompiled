using System.Threading;
using Netcode;
using StardewValley.Network;

namespace StardewValley;

public class NewDaySynchronizer : NetSynchronizer
{
	/// <summary>A flag that clients use during <see cref="M:StardewValley.NewDaySynchronizer.start" /> to determine if they need to wait for the server.</summary>
	private bool ServerReady;

	/// <summary>A flag that used by <see cref="M:StardewValley.NewDaySynchronizer.hasInstance" /> that determines if the <see cref="T:StardewValley.NewDaySynchronizer" /> has a useable signaling context.</summary>
	private bool Instantiated;

	public NewDaySynchronizer()
	{
		ServerReady = false;
		Instantiated = false;
	}

	/// <summary>Determines if the <see cref="T:StardewValley.NewDaySynchronizer" /> object has a context that can actively be used for synchronization.</summary>
	/// <returns><see langword="true" /> if <see cref="T:StardewValley.NewDaySynchronizer" /> object has a signaling context, <see langword="false" /> otherwise.</returns>
	public bool hasInstance()
	{
		return Instantiated;
	}

	/// <summary>Creates a synchronizer context that can be used for signaling. <see cref="M:StardewValley.NewDaySynchronizer.hasInstance" /> will return <see langword="true" /> after this call.</summary>
	public void create()
	{
		Instantiated = true;
	}

	/// <summary>Destroys the synchronizer context, such that it can no longer be used for signaling. <see cref="M:StardewValley.NewDaySynchronizer.hasInstance" /> will return <see langword="false" /> after this call.</summary>
	public void destroy()
	{
		Instantiated = false;
		ServerReady = false;
		reset();
	}

	/// <summary>Notifies a client that the server has reached the <see cref="M:StardewValley.NewDaySynchronizer.start" />. <see cref="M:StardewValley.NewDaySynchronizer.start" /> will unblock after this call.</summary>
	public void flagServerReady()
	{
		if (!Game1.IsMasterGame)
		{
			ServerReady = true;
		}
	}

	public void start()
	{
		Game1.multiplayer.UpdateEarly();
		if (Game1.IsMasterGame)
		{
			ServerReady = true;
			{
				foreach (Farmer f in Game1.otherFarmers.Values)
				{
					Game1.server.sendMessage(f.UniqueMultiplayerID, new OutgoingMessage(30, Game1.player));
				}
				return;
			}
		}
		while (!ServerReady)
		{
			processMessages();
			if (shouldAbort())
			{
				ServerReady = false;
				throw new AbortNetSynchronizerException();
			}
			if (LocalMultiplayer.IsLocalMultiplayer())
			{
				break;
			}
		}
	}

	/// <summary>Check if the server has started the synchronization context, so the calling task can <see langword="yield" /> otherwise.</summary>
	public bool hasStarted()
	{
		if (ServerReady)
		{
			return true;
		}
		processMessages();
		return false;
	}

	public bool readyForFinish()
	{
		Game1.netReady.SetLocalReady("wakeup", ready: true);
		Game1.player.team.Update();
		Game1.multiplayer.UpdateLate();
		Game1.multiplayer.UpdateEarly();
		return Game1.netReady.IsReady("wakeup");
	}

	public bool readyForSave()
	{
		Game1.netReady.SetLocalReady("ready_for_save", ready: true);
		Game1.player.team.Update();
		Game1.multiplayer.UpdateLate();
		Game1.multiplayer.UpdateEarly();
		return Game1.netReady.IsReady("ready_for_save");
	}

	public int numReadyForSave()
	{
		return Game1.netReady.GetNumberReady("ready_for_save");
	}

	public void finish()
	{
		if (Game1.IsServer)
		{
			sendVar<NetBool, bool>("finished", value: true);
		}
		Game1.multiplayer.UpdateLate();
	}

	public bool hasFinished()
	{
		return hasVar("finished");
	}

	public void flagSaved()
	{
		if (Game1.IsServer)
		{
			sendVar<NetBool, bool>("saved", value: true);
		}
		Game1.multiplayer.UpdateLate();
	}

	public bool hasSaved()
	{
		return hasVar("saved");
	}

	public override void processMessages()
	{
		Game1.multiplayer.UpdateLate();
		Thread.Sleep(16);
		Program.sdk.Update();
		Game1.multiplayer.UpdateEarly();
	}

	protected override void sendMessage(params object[] data)
	{
		OutgoingMessage msg = new OutgoingMessage(14, Game1.player, data);
		if (Game1.IsServer)
		{
			foreach (Farmer f in Game1.otherFarmers.Values)
			{
				Game1.server.sendMessage(f.UniqueMultiplayerID, msg);
			}
			return;
		}
		if (Game1.IsClient)
		{
			Game1.client.sendMessage(msg);
		}
	}
}
