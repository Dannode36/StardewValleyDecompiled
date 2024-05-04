using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Netcode;

namespace StardewValley.Audio;

public class LoopingCueManager
{
	private Dictionary<string, ICue> playingCues = new Dictionary<string, ICue>();

	private List<string> cuesToStop = new List<string>();

	public virtual void Update(GameLocation currentLocation)
	{
		NetDictionary<string, bool, NetBool, SerializableDictionary<string, bool>, StardewValley.Network.NetStringDictionary<bool, NetBool>>.KeysCollection activeCues = currentLocation.netAudio.ActiveCues;
		foreach (string cue in activeCues)
		{
			if (!playingCues.ContainsKey(cue))
			{
				Game1.playSound(cue, out var instance);
				playingCues[cue] = instance;
			}
		}
		foreach (KeyValuePair<string, ICue> playingCue in playingCues)
		{
			string cue = playingCue.Key;
			if (!activeCues.Contains(cue))
			{
				cuesToStop.Add(cue);
			}
		}
		foreach (string cue in cuesToStop)
		{
			playingCues[cue].Stop(AudioStopOptions.AsAuthored);
			playingCues.Remove(cue);
		}
		cuesToStop.Clear();
	}

	public void StopAll()
	{
		foreach (ICue value in playingCues.Values)
		{
			value.Stop(AudioStopOptions.Immediate);
		}
		playingCues.Clear();
	}
}
