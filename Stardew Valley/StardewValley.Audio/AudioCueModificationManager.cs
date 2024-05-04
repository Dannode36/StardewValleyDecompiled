using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using StardewValley.GameData;

namespace StardewValley.Audio;

public class AudioCueModificationManager
{
	public Dictionary<string, AudioCueData> cueModificationData;

	public void OnStartup()
	{
		cueModificationData = DataLoader.AudioChanges(Game1.content);
		ApplyAllCueModifications();
	}

	public virtual void ApplyAllCueModifications()
	{
		foreach (string key in cueModificationData.Keys)
		{
			ApplyCueModification(key);
		}
	}

	public virtual string GetFilePath(string file_path)
	{
		return Path.Combine(Game1.content.RootDirectory, file_path);
	}

	public virtual void ApplyCueModification(string key)
	{
		if (!cueModificationData.TryGetValue(key, out var modification_data))
		{
			return;
		}
		bool is_modification = false;
		int category_index = Game1.audioEngine.GetCategoryIndex("Default");
		CueDefinition cue_definition;
		if (Game1.soundBank.Exists(modification_data.Id))
		{
			cue_definition = Game1.soundBank.GetCueDefinition(modification_data.Id);
			is_modification = true;
		}
		else
		{
			cue_definition = new CueDefinition();
			cue_definition.name = modification_data.Id;
		}
		if (modification_data.Category != null)
		{
			category_index = Game1.audioEngine.GetCategoryIndex(modification_data.Category);
		}
		if (modification_data.FilePaths != null)
		{
			SoundEffect[] effects = new SoundEffect[modification_data.FilePaths.Count];
			for (int i = 0; i < modification_data.FilePaths.Count; i++)
			{
				string file_path = GetFilePath(modification_data.FilePaths[i]);
				bool vorbis = Path.GetExtension(file_path).ToLowerInvariant() == ".ogg";
				int invalid_sounds = 0;
				try
				{
					SoundEffect sound_effect;
					if (vorbis && modification_data.StreamedVorbis)
					{
						sound_effect = new OggStreamSoundEffect(file_path);
					}
					else
					{
						using FileStream stream = new FileStream(file_path, FileMode.Open);
						sound_effect = SoundEffect.FromStream(stream, vorbis);
					}
					effects[i - invalid_sounds] = sound_effect;
				}
				catch (Exception e)
				{
					Game1.log.Error("Error loading sound: " + file_path, e);
					invalid_sounds++;
				}
				if (invalid_sounds > 0)
				{
					Array.Resize(ref effects, effects.Length - invalid_sounds);
				}
			}
			cue_definition.SetSound(effects, category_index, modification_data.Looped, modification_data.UseReverb);
			if (is_modification)
			{
				cue_definition.OnModified?.Invoke();
			}
		}
		Game1.soundBank.AddCue(cue_definition);
	}
}
