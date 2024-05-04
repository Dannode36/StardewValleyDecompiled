using System;
using System.IO;
using System.Runtime.InteropServices;
using StardewValley.Network;
using Steamworks;

namespace StardewValley.SDKs.Steam.Internal;

/// <summary>Simplifies interacting with Steam Networking Sockets for the Steam SDK client.</summary>
internal static class SteamSocketUtils
{
	/// <summary>Gets an array of configuration values to use when creating a Steam connection.</summary>
	internal static SteamNetworkingConfigValue_t[] GetNetworkingOptions()
	{
		return new SteamNetworkingConfigValue_t[1]
		{
			new SteamNetworkingConfigValue_t
			{
				m_eValue = ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_SendBufferSize,
				m_eDataType = ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32,
				m_val = 
				{
					m_int32 = 1048576
				}
			}
		};
	}

	/// <summary>Converts a <see cref="T:Steamworks.SteamNetworkingMessage_t" /> into an <see cref="T:StardewValley.Network.IncomingMessage" /> to be used internally, decompressing the data if needed.</summary>
	/// <param name="messagePtr">A pointer to the <see cref="T:Steamworks.SteamNetworkingMessage_t" /> that we will process.</param>
	/// <param name="message">A reference to message to write the data into.</param>
	/// <param name="messageConnection">The connection that sent the <see cref="T:Steamworks.SteamNetworkingMessage_t" />.</param>
	/// <param name="bandwidthLogger">A bandwidth logger with which to log the number of bytes received.</param>
	internal static void ProcessSteamMessage(IntPtr messagePtr, IncomingMessage message, out HSteamNetConnection messageConnection, BandwidthLogger bandwidthLogger)
	{
		SteamNetworkingMessage_t messageSteam = (SteamNetworkingMessage_t)Marshal.PtrToStructure(messagePtr, typeof(SteamNetworkingMessage_t));
		messageConnection = messageSteam.m_conn;
		byte[] rawData = new byte[messageSteam.m_cbSize];
		Marshal.Copy(messageSteam.m_pData, rawData, 0, rawData.Length);
		using (MemoryStream messageStream = new MemoryStream(Program.netCompression.DecompressBytes(rawData)))
		{
			messageStream.Position = 0L;
			using BinaryReader messageReader = new BinaryReader(messageStream);
			message.Read(messageReader);
		}
		SteamNetworkingMessage_t.Release(messagePtr);
		bandwidthLogger?.RecordBytesDown(rawData.Length);
	}

	/// <summary>Converts and sends an <see cref="T:StardewValley.Network.OutgoingMessage" /> over Steam's sockets, compressing the data if needed.</summary>
	/// <param name="messageConnection">The connection through which to send the message.</param>
	/// <param name="message">The message to be sent using Steam's sockets.</param>
	/// <param name="bandwidthLogger">A bandwidth logger with which to log the number of bytes sent.</param>
	/// <param name="onDisconnected">Cleans up all bookkeeping data about the connection if a message fails to send.</param>
	internal unsafe static void SendMessage(HSteamNetConnection messageConnection, OutgoingMessage message, BandwidthLogger bandwidthLogger, Action<HSteamNetConnection> onDisconnected = null)
	{
		byte[] messageBytes = null;
		using (MemoryStream messageStream = new MemoryStream())
		{
			using BinaryWriter messageWriter = new BinaryWriter(messageStream);
			message.Write(messageWriter);
			messageStream.Seek(0L, SeekOrigin.Begin);
			messageBytes = messageStream.ToArray();
		}
		byte[] data = Program.netCompression.CompressAbove(messageBytes, 1024);
		EResult result;
		fixed (byte* ptr = data)
		{
			result = SteamNetworkingSockets.SendMessageToConnection(messageConnection, (IntPtr)ptr, Convert.ToUInt32(data.Length), 8, out var _);
		}
		if (result != EResult.k_EResultOK)
		{
			Game1.log.Warn("Failed to send message (" + result.ToString() + "). Closing connection.");
			CloseConnection(messageConnection, onDisconnected);
		}
		else
		{
			bandwidthLogger?.RecordBytesUp(data.Length);
		}
	}

	/// <summary>Closes a Steam connection if it's valid.</summary>
	/// <param name="connection">The connection to close.</param>
	/// <param name="onDisconnected">The callback invoked immediately before the connection is closed to perform any cleanup needed.</param>
	internal static void CloseConnection(HSteamNetConnection connection, Action<HSteamNetConnection> onDisconnected = null)
	{
		if (!(connection == HSteamNetConnection.Invalid))
		{
			SteamNetworkingSockets.SetConnectionPollGroup(connection, HSteamNetPollGroup.Invalid);
			onDisconnected?.Invoke(connection);
			SteamNetworkingSockets.CloseConnection(connection, 1000, null, bEnableLinger: true);
		}
	}
}
