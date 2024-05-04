using System.IO;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Audio;

namespace StardewValley.Network;

public class NetAudio : INetObject<NetFields>
{
	private readonly NetEventBinary audioEvent = new NetEventBinary();

	/// <summary>The backing field for <see cref="P:StardewValley.Network.NetAudio.ActiveCues" />.</summary>
	private readonly NetStringDictionary<bool, NetBool> activeCues = new NetStringDictionary<bool, NetBool>();

	/// <summary>The location whose audio this instance manages.</summary>
	private GameLocation location;

	public NetFields NetFields { get; } = new NetFields("NetAudio");


	/// <summary>The sound IDs to play continuously until they're removed from the list.</summary>
	public NetDictionary<string, bool, NetBool, SerializableDictionary<string, bool>, NetStringDictionary<bool, NetBool>>.KeysCollection ActiveCues => activeCues.Keys;

	/// <summary>Construct an instance.</summary>
	/// <param name="location">The location whose audio this instance manages.</param>
	public NetAudio(GameLocation location)
	{
		this.location = location;
		NetFields.SetOwner(this).AddField(audioEvent, "audioEvent").AddField(activeCues, "activeCues");
		audioEvent.AddReaderHandler(handleAudioEvent);
	}

	private void handleAudioEvent(BinaryReader reader)
	{
		Read(reader, out var audioName, out var position, out var pitch, out var context);
		Game1.sounds.PlayLocal(audioName, location, position, pitch, context, out var _);
	}

	public void Update()
	{
		audioEvent.Poll();
	}

	/// <summary>Send an audio cue to all players, including the current one.</summary>
	/// <param name="audioName">The sound ID to play.</param>
	/// <param name="position">The tile position from which the sound is playing, or <c>null</c> if it's playing throughout the location.</param>
	/// <param name="pitch">The pitch modifier to apply, or <c>null</c> for the default pitch.</param>
	/// <param name="context">The source which triggered a game sound.</param>
	public void Fire(string audioName, Vector2? position, int? pitch, SoundContext context)
	{
		audioEvent.Fire(delegate(BinaryWriter writer)
		{
			writer.Write(audioName);
			writer.WriteVector2(position ?? new Vector2(-2.1474836E+09f));
			writer.Write(pitch ?? int.MinValue);
			writer.Write((int)context);
		});
		audioEvent.Poll();
	}

	/// <summary>Read an audio cue from the network that was sent via <see cref="M:StardewValley.Network.NetAudio.Fire(System.String,System.Nullable{Microsoft.Xna.Framework.Vector2},System.Nullable{System.Int32},StardewValley.Audio.SoundContext)" />.</summary>
	/// <param name="reader">The network input reader.</param>
	/// <param name="audioName">The sound ID to play.</param>
	/// <param name="position">The tile position from which the sound is playing, or <c>null</c> if it's playing throughout the location.</param>
	/// <param name="pitch">The pitch modifier to apply, or <c>null</c> for the default pitch.</param>
	/// <param name="context">The source which triggered a game sound.</param>
	public void Read(BinaryReader reader, out string audioName, out Vector2? position, out int? pitch, out SoundContext context)
	{
		audioName = reader.ReadString();
		position = reader.ReadVector2();
		pitch = reader.ReadInt32();
		context = (SoundContext)reader.ReadInt32();
		if ((int)position.Value.X == int.MinValue && (int)position.Value.Y == int.MinValue)
		{
			position = null;
		}
		if (pitch == int.MinValue)
		{
			pitch = null;
		}
	}

	/// <summary>Play a sound continuously until it's stopped via <see cref="M:StardewValley.Network.NetAudio.StopPlaying(System.String)" />.</summary>
	/// <param name="cueName">The sound ID to play.</param>
	public void StartPlaying(string cueName)
	{
		activeCues[cueName] = false;
	}

	/// <summary>Stop a sound that is playing continuously after <see cref="M:StardewValley.Network.NetAudio.StartPlaying(System.String)" />.</summary>
	/// <param name="cueName">The sound ID to stop.</param>
	public void StopPlaying(string cueName)
	{
		activeCues.Remove(cueName);
	}
}
