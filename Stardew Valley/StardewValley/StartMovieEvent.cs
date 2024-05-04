using System.Collections.Generic;
using System.IO;
using Netcode;

namespace StardewValley;

public class StartMovieEvent : NetEventArg
{
	public long uid;

	public List<List<Character>> playerGroups;

	public List<List<Character>> npcGroups;

	public StartMovieEvent()
	{
	}

	public StartMovieEvent(long farmer_uid, List<List<Character>> player_groups, List<List<Character>> npc_groups)
	{
		uid = farmer_uid;
		playerGroups = player_groups;
		npcGroups = npc_groups;
	}

	public void Read(BinaryReader reader)
	{
		uid = reader.ReadInt64();
		playerGroups = ReadCharacterList(reader);
		npcGroups = ReadCharacterList(reader);
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(uid);
		WriteCharacterList(writer, playerGroups);
		WriteCharacterList(writer, npcGroups);
	}

	public List<List<Character>> ReadCharacterList(BinaryReader reader)
	{
		List<List<Character>> group_list = new List<List<Character>>();
		int group_list_count = reader.ReadInt32();
		for (int i = 0; i < group_list_count; i++)
		{
			List<Character> group = new List<Character>();
			int group_count = reader.ReadInt32();
			for (int j = 0; j < group_count; j++)
			{
				Character character = ((reader.ReadInt32() == 1) ? ((Character)Game1.getFarmer(reader.ReadInt64())) : ((Character)Game1.getCharacterFromName(reader.ReadString())));
				group.Add(character);
			}
			group_list.Add(group);
		}
		return group_list;
	}

	public void WriteCharacterList(BinaryWriter writer, List<List<Character>> group_list)
	{
		writer.Write(group_list.Count);
		foreach (List<Character> group in group_list)
		{
			writer.Write(group.Count);
			foreach (Character character in group)
			{
				if (character is Farmer player)
				{
					writer.Write(1);
					writer.Write(player.UniqueMultiplayerID);
				}
				else
				{
					writer.Write(0);
					writer.Write(character.Name);
				}
			}
		}
	}
}
