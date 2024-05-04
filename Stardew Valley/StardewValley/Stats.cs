using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using StardewValley.Extensions;
using StardewValley.GameData.Crops;
using StardewValley.GameData.Objects;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.TokenizableStrings;

namespace StardewValley;

public class Stats
{
	/// <summary>The number of each monster type killed, prefixed by the monster's internal name.</summary>
	public SerializableDictionaryWithCaseInsensitiveKeys<int> specificMonstersKilled = new SerializableDictionaryWithCaseInsensitiveKeys<int>();

	/// <summary>The numeric metrics tracked by the game.</summary>
	/// <remarks>Most code should use methods like <see cref="M:StardewValley.Stats.Get(System.String)" /> or <see cref="M:StardewValley.Stats.Set(System.String,System.UInt32)" /> instead of calling this directly.</remarks>
	public SerializableDictionaryWithCaseInsensitiveKeys<uint> Values = new SerializableDictionaryWithCaseInsensitiveKeys<uint>();

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="F:StardewValley.Stats.Values" /> instead.</summary>
	[XmlElement("stat_dictionary")]
	public SerializableDictionary<string, uint> obsolete_stat_dictionary;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.AverageBedtime" /> instead.</summary>
	[XmlElement("averageBedtime")]
	public uint? obsolete_averageBedtime;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.BeveragesMade" /> instead.</summary>
	[XmlElement("beveragesMade")]
	public uint? obsolete_beveragesMade;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.CaveCarrotsFound" /> instead.</summary>
	[XmlElement("caveCarrotsFound")]
	public uint? obsolete_caveCarrotsFound;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.CheeseMade" /> instead.</summary>
	[XmlElement("cheeseMade")]
	public uint? obsolete_cheeseMade;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.ChickenEggsLayed" /> instead.</summary>
	[XmlElement("chickenEggsLayed")]
	public uint? obsolete_chickenEggsLayed;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.CopperFound" /> instead.</summary>
	[XmlElement("copperFound")]
	public uint? obsolete_copperFound;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.CowMilkProduced" /> instead.</summary>
	[XmlElement("cowMilkProduced")]
	public uint? obsolete_cowMilkProduced;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.CropsShipped" /> instead.</summary>
	[XmlElement("cropsShipped")]
	public uint? obsolete_cropsShipped;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.DaysPlayed" /> instead.</summary>
	[XmlElement("daysPlayed")]
	public uint? obsolete_daysPlayed;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.DiamondsFound" /> instead.</summary>
	[XmlElement("diamondsFound")]
	public uint? obsolete_diamondsFound;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.DirtHoed" /> instead.</summary>
	[XmlElement("dirtHoed")]
	public uint? obsolete_dirtHoed;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.DuckEggsLayed" /> instead.</summary>
	[XmlElement("duckEggsLayed")]
	public uint? obsolete_duckEggsLayed;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.FishCaught" /> instead.</summary>
	[XmlElement("fishCaught")]
	public uint? obsolete_fishCaught;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.GeodesCracked" /> instead.</summary>
	[XmlElement("geodesCracked")]
	public uint? obsolete_geodesCracked;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.GiftsGiven" /> instead.</summary>
	[XmlElement("giftsGiven")]
	public uint? obsolete_giftsGiven;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.GoatCheeseMade" /> instead.</summary>
	[XmlElement("goatCheeseMade")]
	public uint? obsolete_goatCheeseMade;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.GoatMilkProduced" /> instead.</summary>
	[XmlElement("goatMilkProduced")]
	public uint? obsolete_goatMilkProduced;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.GoldFound" /> instead.</summary>
	[XmlElement("goldFound")]
	public uint? obsolete_goldFound;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.GoodFriends" /> instead.</summary>
	[XmlElement("goodFriends")]
	public uint? obsolete_goodFriends;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.IndividualMoneyEarned" /> instead.</summary>
	[XmlElement("individualMoneyEarned")]
	public uint? obsolete_individualMoneyEarned;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.IridiumFound" /> instead.</summary>
	[XmlElement("iridiumFound")]
	public uint? obsolete_iridiumFound;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.IronFound" /> instead.</summary>
	[XmlElement("ironFound")]
	public uint? obsolete_ironFound;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.ItemsCooked" /> instead.</summary>
	[XmlElement("itemsCooked")]
	public uint? obsolete_itemsCooked;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.ItemsCrafted" /> instead.</summary>
	[XmlElement("itemsCrafted")]
	public uint? obsolete_itemsCrafted;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.ItemsForaged" /> instead.</summary>
	[XmlElement("itemsForaged")]
	public uint? obsolete_itemsForaged;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.ItemsShipped" /> instead.</summary>
	[XmlElement("itemsShipped")]
	public uint? obsolete_itemsShipped;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.MonstersKilled" /> instead.</summary>
	[XmlElement("monstersKilled")]
	public uint? obsolete_monstersKilled;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.MysticStonesCrushed" /> instead.</summary>
	[XmlElement("mysticStonesCrushed")]
	public uint? obsolete_mysticStonesCrushed;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.NotesFound" /> instead.</summary>
	[XmlElement("notesFound")]
	public uint? obsolete_notesFound;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.OtherPreciousGemsFound" /> instead.</summary>
	[XmlElement("otherPreciousGemsFound")]
	public uint? obsolete_otherPreciousGemsFound;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.PiecesOfTrashRecycled" /> instead.</summary>
	[XmlElement("piecesOfTrashRecycled")]
	public uint? obsolete_piecesOfTrashRecycled;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.PreservesMade" /> instead.</summary>
	[XmlElement("preservesMade")]
	public uint? obsolete_preservesMade;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.PrismaticShardsFound" /> instead.</summary>
	[XmlElement("prismaticShardsFound")]
	public uint? obsolete_prismaticShardsFound;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.QuestsCompleted" /> instead.</summary>
	[XmlElement("questsCompleted")]
	public uint? obsolete_questsCompleted;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.RabbitWoolProduced" /> instead.</summary>
	[XmlElement("rabbitWoolProduced")]
	public uint? obsolete_rabbitWoolProduced;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.RocksCrushed" /> instead.</summary>
	[XmlElement("rocksCrushed")]
	public uint? obsolete_rocksCrushed;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.SheepWoolProduced" /> instead.</summary>
	[XmlElement("sheepWoolProduced")]
	public uint? obsolete_sheepWoolProduced;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.SlimesKilled" /> instead.</summary>
	[XmlElement("slimesKilled")]
	public uint? obsolete_slimesKilled;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.StepsTaken" /> instead.</summary>
	[XmlElement("stepsTaken")]
	public uint? obsolete_stepsTaken;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.StoneGathered" /> instead.</summary>
	[XmlElement("stoneGathered")]
	public uint? obsolete_stoneGathered;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.StumpsChopped" /> instead.</summary>
	[XmlElement("stumpsChopped")]
	public uint? obsolete_stumpsChopped;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.TimesFished" /> instead.</summary>
	[XmlElement("timesFished")]
	public uint? obsolete_timesFished;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.TimesUnconscious" /> instead.</summary>
	[XmlElement("timesUnconscious")]
	public uint? obsolete_timesUnconscious;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="F:StardewValley.Constants.StatKeys.TotalMoneyGifted" /> instead.</summary>
	[XmlElement("totalMoneyGifted")]
	public uint? obsolete_totalMoneyGifted;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.TrufflesFound" /> instead.</summary>
	[XmlElement("trufflesFound")]
	public uint? obsolete_trufflesFound;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.WeedsEliminated" /> instead.</summary>
	[XmlElement("weedsEliminated")]
	public uint? obsolete_weedsEliminated;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.SeedsSown" /> instead.</summary>
	[XmlElement("seedsSown")]
	public uint? obsolete_seedsSown;

	[XmlIgnore]
	public uint AverageBedtime
	{
		get
		{
			return Get("averageBedtime");
		}
		set
		{
			uint prevAverage = Get("averageBedtime");
			uint daysPlayed = Get("daysPlayed");
			Set("averageBedtime", (prevAverage * (daysPlayed - 1) + value) / Math.Max(1u, daysPlayed));
		}
	}

	[XmlIgnore]
	public uint DaysPlayed
	{
		get
		{
			return Get("daysPlayed");
		}
		set
		{
			Set("daysPlayed", value);
		}
	}

	[XmlIgnore]
	public uint IndividualMoneyEarned
	{
		get
		{
			return Get("individualMoneyEarned");
		}
		set
		{
			uint previousEarned = Get("individualMoneyEarned");
			Set("individualMoneyEarned", value);
			if (previousEarned < 1000000 && value >= 1000000)
			{
				Game1.multiplayer.globalChatInfoMessage("SoloEarned1mil_" + (Game1.player.IsMale ? "Male" : "Female"), Game1.player.Name);
			}
			else if (previousEarned < 100000 && value >= 100000)
			{
				Game1.multiplayer.globalChatInfoMessage("SoloEarned100k_" + (Game1.player.IsMale ? "Male" : "Female"), Game1.player.Name);
			}
			else if (previousEarned < 10000 && value >= 10000)
			{
				Game1.multiplayer.globalChatInfoMessage("SoloEarned10k_" + (Game1.player.IsMale ? "Male" : "Female"), Game1.player.Name);
			}
			else if (previousEarned < 1000 && value >= 1000)
			{
				Game1.multiplayer.globalChatInfoMessage("SoloEarned1k_" + (Game1.player.IsMale ? "Male" : "Female"), Game1.player.Name);
			}
		}
	}

	[XmlIgnore]
	public uint ItemsCooked
	{
		get
		{
			return Get("itemsCooked");
		}
		set
		{
			Set("itemsCooked", value);
		}
	}

	[XmlIgnore]
	public uint ItemsCrafted
	{
		get
		{
			return Get("itemsCrafted");
		}
		set
		{
			Set("itemsCrafted", value);
			checkForCraftingAchievements();
		}
	}

	[XmlIgnore]
	public uint ItemsForaged
	{
		get
		{
			return Get("itemsForaged");
		}
		set
		{
			Set("itemsForaged", value);
		}
	}

	[XmlIgnore]
	public uint ItemsShipped
	{
		get
		{
			return Get("itemsShipped");
		}
		set
		{
			Set("itemsShipped", value);
		}
	}

	[XmlIgnore]
	public uint NotesFound
	{
		get
		{
			return Get("notesFound");
		}
		set
		{
			Set("notesFound", value);
		}
	}

	[XmlIgnore]
	public uint StepsTaken
	{
		get
		{
			return Get("stepsTaken");
		}
		set
		{
			Set("stepsTaken", value);
		}
	}

	[XmlIgnore]
	public uint StumpsChopped
	{
		get
		{
			return Get("stumpsChopped");
		}
		set
		{
			Set("stumpsChopped", value);
		}
	}

	[XmlIgnore]
	public uint TimesUnconscious
	{
		get
		{
			return Get("timesUnconscious");
		}
		set
		{
			Set("timesUnconscious", value);
		}
	}

	[XmlIgnore]
	public uint BeveragesMade
	{
		get
		{
			return Get("beveragesMade");
		}
		set
		{
			Set("beveragesMade", value);
		}
	}

	[XmlIgnore]
	public uint CheeseMade
	{
		get
		{
			return Get("cheeseMade");
		}
		set
		{
			Set("cheeseMade", value);
		}
	}

	[XmlIgnore]
	public uint ChickenEggsLayed
	{
		get
		{
			return Get("chickenEggsLayed");
		}
		set
		{
			Set("chickenEggsLayed", value);
		}
	}

	[XmlIgnore]
	public uint CowMilkProduced
	{
		get
		{
			return Get("cowMilkProduced");
		}
		set
		{
			Set("cowMilkProduced", value);
		}
	}

	[XmlIgnore]
	public uint CropsShipped
	{
		get
		{
			return Get("cropsShipped");
		}
		set
		{
			Set("cropsShipped", value);
		}
	}

	[XmlIgnore]
	public uint DirtHoed
	{
		get
		{
			return Get("dirtHoed");
		}
		set
		{
			Set("dirtHoed", value);
		}
	}

	[XmlIgnore]
	public uint DuckEggsLayed
	{
		get
		{
			return Get("duckEggsLayed");
		}
		set
		{
			Set("duckEggsLayed", value);
		}
	}

	[XmlIgnore]
	public uint GoatCheeseMade
	{
		get
		{
			return Get("goatCheeseMade");
		}
		set
		{
			Set("goatCheeseMade", value);
		}
	}

	[XmlIgnore]
	public uint GoatMilkProduced
	{
		get
		{
			return Get("goatMilkProduced");
		}
		set
		{
			Set("goatMilkProduced", value);
		}
	}

	[XmlIgnore]
	public uint PiecesOfTrashRecycled
	{
		get
		{
			return Get("piecesOfTrashRecycled");
		}
		set
		{
			Set("piecesOfTrashRecycled", value);
		}
	}

	[XmlIgnore]
	public uint PreservesMade
	{
		get
		{
			return Get("preservesMade");
		}
		set
		{
			Set("preservesMade", value);
		}
	}

	[XmlIgnore]
	public uint RabbitWoolProduced
	{
		get
		{
			return Get("rabbitWoolProduced");
		}
		set
		{
			Set("rabbitWoolProduced", value);
		}
	}

	[XmlIgnore]
	public uint SeedsSown
	{
		get
		{
			return Get("seedsSown");
		}
		set
		{
			Set("seedsSown", value);
		}
	}

	[XmlIgnore]
	public uint SheepWoolProduced
	{
		get
		{
			return Get("sheepWoolProduced");
		}
		set
		{
			Set("sheepWoolProduced", value);
		}
	}

	[XmlIgnore]
	public uint TrufflesFound
	{
		get
		{
			return Get("trufflesFound");
		}
		set
		{
			Set("trufflesFound", value);
		}
	}

	[XmlIgnore]
	public uint WeedsEliminated
	{
		get
		{
			return Get("weedsEliminated");
		}
		set
		{
			Set("weedsEliminated", value);
		}
	}

	[XmlIgnore]
	public uint MonstersKilled
	{
		get
		{
			return Get("monstersKilled");
		}
		set
		{
			Set("monstersKilled", value);
		}
	}

	[XmlIgnore]
	public uint SlimesKilled
	{
		get
		{
			return Get("slimesKilled");
		}
		set
		{
			Set("slimesKilled", value);
		}
	}

	[XmlIgnore]
	public uint FishCaught
	{
		get
		{
			return Get("fishCaught");
		}
		set
		{
			Set("fishCaught", value);
		}
	}

	[XmlIgnore]
	public uint TimesFished
	{
		get
		{
			return Get("timesFished");
		}
		set
		{
			Set("timesFished", value);
		}
	}

	[XmlIgnore]
	public uint CaveCarrotsFound
	{
		get
		{
			return Get("caveCarrotsFound");
		}
		set
		{
			Set("caveCarrotsFound", value);
		}
	}

	[XmlIgnore]
	public uint CopperFound
	{
		get
		{
			return Get("copperFound");
		}
		set
		{
			Set("copperFound", value);
		}
	}

	[XmlIgnore]
	public uint DiamondsFound
	{
		get
		{
			return Get("diamondsFound");
		}
		set
		{
			Set("diamondsFound", value);
		}
	}

	[XmlIgnore]
	public uint GeodesCracked
	{
		get
		{
			return Get("geodesCracked");
		}
		set
		{
			Set("geodesCracked", value);
		}
	}

	[XmlIgnore]
	public uint GoldFound
	{
		get
		{
			return Get("goldFound");
		}
		set
		{
			Set("goldFound", value);
		}
	}

	[XmlIgnore]
	public uint IridiumFound
	{
		get
		{
			return Get("iridiumFound");
		}
		set
		{
			Set("iridiumFound", value);
		}
	}

	[XmlIgnore]
	public uint IronFound
	{
		get
		{
			return Get("ironFound");
		}
		set
		{
			Set("ironFound", value);
		}
	}

	[XmlIgnore]
	public uint MysticStonesCrushed
	{
		get
		{
			return Get("mysticStonesCrushed");
		}
		set
		{
			Set("mysticStonesCrushed", value);
		}
	}

	[XmlIgnore]
	public uint OtherPreciousGemsFound
	{
		get
		{
			return Get("otherPreciousGemsFound");
		}
		set
		{
			Set("otherPreciousGemsFound", value);
		}
	}

	[XmlIgnore]
	public uint PrismaticShardsFound
	{
		get
		{
			return Get("prismaticShardsFound");
		}
		set
		{
			Set("prismaticShardsFound", value);
		}
	}

	[XmlIgnore]
	public uint RocksCrushed
	{
		get
		{
			return Get("rocksCrushed");
		}
		set
		{
			Set("rocksCrushed", value);
		}
	}

	[XmlIgnore]
	public uint StoneGathered
	{
		get
		{
			return Get("stoneGathered");
		}
		set
		{
			Set("stoneGathered", value);
		}
	}

	[XmlIgnore]
	public uint GiftsGiven
	{
		get
		{
			return Get("giftsGiven");
		}
		set
		{
			Set("giftsGiven", value);
		}
	}

	[XmlIgnore]
	public uint GoodFriends
	{
		get
		{
			return Get("goodFriends");
		}
		set
		{
			Set("goodFriends", value);
		}
	}

	[XmlIgnore]
	public uint QuestsCompleted
	{
		get
		{
			return Get("questsCompleted");
		}
		set
		{
			Set("questsCompleted", value);
			checkForQuestAchievements();
		}
	}

	/// <summary>Get the value of a tracked stat.</summary>
	/// <param name="key">The unique stat key, usually matching a <see cref="T:StardewValley.Constants.StatKeys" /> field.</param>
	public uint Get(string key)
	{
		if (!Values.TryGetValue(key, out var value))
		{
			return 0u;
		}
		return value;
	}

	/// <summary>Set the value of a tracked stat.</summary>
	/// <param name="key">The unique stat key, usually matching a <see cref="T:StardewValley.Constants.StatKeys" /> field.</param>
	/// <param name="value">The new value to set.</param>
	public void Set(string key, uint value)
	{
		if (value != 0)
		{
			Values[key] = value;
		}
		else
		{
			Values.Remove(key);
		}
	}

	/// <summary>Set the value of a tracked stat.</summary>
	/// <param name="key">The unique stat key, usually matching a <see cref="T:StardewValley.Constants.StatKeys" /> field.</param>
	/// <param name="value">The new value to set.</param>
	/// <remarks>The minimum stat value is zero. Setting a negative value is equivalent to setting zero.</remarks>
	public void Set(string key, int value)
	{
		if (value <= 0)
		{
			Set(key, 0u);
		}
		else
		{
			Set(key, (uint)value);
		}
	}

	/// <summary>Decrease the value of a tracked stat.</summary>
	/// <param name="key">The unique stat key, usually matching a <see cref="T:StardewValley.Constants.StatKeys" /> field.</param>
	/// <param name="amount">The amount by which to decrease the stat.</param>
	/// <remarks>The minimum stat value is zero. Decrementing past zero is equivalent to setting zero.</remarks>
	public uint Decrement(string key, uint amount = 1u)
	{
		uint newValue = Get(key);
		newValue = ((amount < newValue) ? (newValue - amount) : 0u);
		Set(key, newValue);
		return newValue;
	}

	/// <summary>Increase the value of a tracked stat.</summary>
	/// <param name="key">The unique stat key, usually matching a <see cref="T:StardewValley.Constants.StatKeys" /> field.</param>
	/// <param name="amount">The amount by which to increase the stat.</param>
	/// <returns>Returns the new stat value.</returns>
	public uint Increment(string key, uint amount = 1u)
	{
		uint newValue = Get(key) + amount;
		Set(key, newValue);
		return newValue;
	}

	/// <summary>Increase the value of a tracked stat.</summary>
	/// <param name="key">The unique stat key, usually matching a <see cref="T:StardewValley.Constants.StatKeys" /> field.</param>
	/// <param name="amount">The amount by which to increase the stat. If this is set to a negative value, the stat will be decremented instead (up to a minimum of zero).</param>
	/// <returns>Returns the new stat value.</returns>
	public uint Increment(string key, int amount)
	{
		if (amount >= 0)
		{
			return Increment(key, (uint)amount);
		}
		return Decrement(key, (uint)(-amount));
	}

	/// <summary>Update the stats when a monster is killed.</summary>
	/// <param name="name">The monster's internal name.</param>
	public void monsterKilled(string name)
	{
		int killedCount = getMonstersKilled(name) + 1;
		if (AdventureGuild.willThisKillCompleteAMonsterSlayerQuest(name))
		{
			specificMonstersKilled[name] = killedCount;
			bool hasCompletedAllGoals = AdventureGuild.areAllMonsterSlayerQuestsComplete();
			Game1.player.hasCompletedAllMonsterSlayerQuests.Value = hasCompletedAllGoals;
			Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Stats.cs.5129"));
			Game1.multiplayer.globalChatInfoMessage("MonsterSlayer" + Game1.random.Next(4), Game1.player.Name, TokenStringBuilder.MonsterName(name));
			if (hasCompletedAllGoals)
			{
				Game1.getSteamAchievement("Achievement_KeeperOfTheMysticRings");
			}
		}
		else
		{
			specificMonstersKilled[name] = killedCount;
		}
	}

	/// <summary>Get the number of a given monster type that the player has killed.</summary>
	/// <param name="name">The monster's internal name.</param>
	public int getMonstersKilled(string name)
	{
		if (!specificMonstersKilled.TryGetValue(name, out var count))
		{
			return 0;
		}
		return count;
	}

	public void onMoneyGifted(uint amount)
	{
		uint previousMoney = Get("totalMoneyGifted");
		uint totalMoneyGifted = Increment("totalMoneyGifted", amount);
		if (previousMoney <= 1000000 && totalMoneyGifted > 1000000)
		{
			Game1.multiplayer.globalChatInfoMessage("Gifted1mil", Game1.player.Name);
		}
		else if (previousMoney <= 100000 && totalMoneyGifted > 100000)
		{
			Game1.multiplayer.globalChatInfoMessage("Gifted100k", Game1.player.Name);
		}
		else if (previousMoney <= 10000 && totalMoneyGifted > 10000)
		{
			Game1.multiplayer.globalChatInfoMessage("Gifted10k", Game1.player.Name);
		}
		else if (previousMoney <= 1000 && totalMoneyGifted > 1000)
		{
			Game1.multiplayer.globalChatInfoMessage("Gifted1k", Game1.player.Name);
		}
	}

	public void takeStep()
	{
		switch (Increment("stepsTaken"))
		{
		case 10000u:
			Game1.multiplayer.globalChatInfoMessage("Walked10k", Game1.player.Name);
			break;
		case 100000u:
			Game1.multiplayer.globalChatInfoMessage("Walked100k", Game1.player.Name);
			break;
		case 1000000u:
			Game1.multiplayer.globalChatInfoMessage("Walked1m", Game1.player.Name);
			break;
		case 10000000u:
			Game1.multiplayer.globalChatInfoMessage("Walked10m", Game1.player.Name);
			break;
		}
	}

	public void checkForCookingAchievements()
	{
		Dictionary<string, string> recipes = DataLoader.CookingRecipes(Game1.content);
		int numberOfRecipesCooked = 0;
		int numberOfMealsMade = 0;
		foreach (KeyValuePair<string, string> v in recipes)
		{
			if (Game1.player.cookingRecipes.ContainsKey(v.Key))
			{
				string recipe = ArgUtility.SplitBySpaceAndGet(v.Value.Split('/')[2], 0);
				if (Game1.player.recipesCooked.TryGetValue(recipe, out var timesCooked))
				{
					numberOfMealsMade += timesCooked;
					numberOfRecipesCooked++;
				}
			}
		}
		Set("itemsCooked", numberOfMealsMade);
		if (numberOfRecipesCooked == recipes.Count)
		{
			Game1.getAchievement(17);
		}
		if (numberOfRecipesCooked >= 25)
		{
			Game1.getAchievement(16);
		}
		if (numberOfRecipesCooked >= 10)
		{
			Game1.getAchievement(15);
		}
	}

	public void checkForCraftingAchievements()
	{
		Dictionary<string, string> recipes = DataLoader.CraftingRecipes(Game1.content);
		int numberOfRecipesMade = 0;
		int numberOfItemsCrafted = 0;
		foreach (string s in recipes.Keys)
		{
			if (!(s == "Wedding Ring") && Game1.player.craftingRecipes.TryGetValue(s, out var timesCrafted))
			{
				numberOfItemsCrafted += timesCrafted;
				if (Game1.player.craftingRecipes[s] > 0)
				{
					numberOfRecipesMade++;
				}
			}
		}
		Set("itemsCrafted", numberOfItemsCrafted);
		if (numberOfRecipesMade >= recipes.Count - 1)
		{
			Game1.getAchievement(22);
		}
		if (numberOfRecipesMade >= 30)
		{
			Game1.getAchievement(21);
		}
		if (numberOfRecipesMade >= 15)
		{
			Game1.getAchievement(20);
		}
	}

	public void checkForShippingAchievements()
	{
		bool allPolycultureCropsShipped = true;
		bool anyMonocultureCropShipped = false;
		foreach (CropData data in Game1.cropData.Values)
		{
			if (data.CountForPolyculture)
			{
				allPolycultureCropsShipped = allPolycultureCropsShipped && farmerShipped(data.HarvestItemId, 15);
			}
			if (data.CountForMonoculture)
			{
				anyMonocultureCropShipped = anyMonocultureCropShipped || farmerShipped(data.HarvestItemId, 300);
			}
		}
		if (allPolycultureCropsShipped)
		{
			Game1.getAchievement(31);
		}
		if (anyMonocultureCropShipped)
		{
			Game1.getAchievement(32);
		}
	}

	private bool farmerShipped(string itemId, int number)
	{
		if (Game1.player.basicShipped.TryGetValue(itemId, out var timesShipped))
		{
			return timesShipped >= number;
		}
		return false;
	}

	public void checkForFishingAchievements()
	{
		int numberOfFishCaught = 0;
		int differentKindsOfFishCaught = 0;
		int totalKindsOfFish = 0;
		foreach (ParsedItemData itemData in ItemRegistry.GetObjectTypeDefinition().GetAllData())
		{
			if (itemData.ObjectType == "Fish" && !(itemData.RawData is ObjectData { ExcludeFromFishingCollection: not false }))
			{
				totalKindsOfFish++;
				if (Game1.player.fishCaught.TryGetValue(itemData.QualifiedItemId, out var data))
				{
					numberOfFishCaught += data[0];
					differentKindsOfFishCaught++;
				}
			}
		}
		Set("fishCaught", numberOfFishCaught);
		if (numberOfFishCaught >= 100)
		{
			Game1.getAchievement(27);
		}
		if (differentKindsOfFishCaught == totalKindsOfFish)
		{
			Game1.getAchievement(26);
			if (!Game1.player.hasOrWillReceiveMail("CF_Fish"))
			{
				Game1.addMailForTomorrow("CF_Fish");
			}
		}
		if (differentKindsOfFishCaught >= 24)
		{
			Game1.getAchievement(25);
		}
		if (differentKindsOfFishCaught >= 10)
		{
			Game1.getAchievement(24);
		}
	}

	public void checkForArchaeologyAchievements()
	{
		int length = Game1.netWorldState.Value.MuseumPieces.Length;
		if (length >= LibraryMuseum.totalArtifacts)
		{
			Game1.getAchievement(5);
		}
		if (length >= 40)
		{
			Game1.getAchievement(28);
		}
	}

	public void checkForMoneyAchievements()
	{
		if (Game1.player.totalMoneyEarned >= 10000000)
		{
			Game1.getAchievement(4);
		}
		if (Game1.player.totalMoneyEarned >= 1000000)
		{
			Game1.getAchievement(3);
		}
		if (Game1.player.totalMoneyEarned >= 250000)
		{
			Game1.getAchievement(2);
		}
		if (Game1.player.totalMoneyEarned >= 50000)
		{
			Game1.getAchievement(1);
		}
		if (Game1.player.totalMoneyEarned >= 15000)
		{
			Game1.getAchievement(0);
		}
	}

	public void checkForBuildingUpgradeAchievements()
	{
		if (Game1.player.HouseUpgradeLevel == 2)
		{
			Game1.getAchievement(19);
		}
		if (Game1.player.HouseUpgradeLevel == 1)
		{
			Game1.getAchievement(18);
		}
	}

	public void checkForQuestAchievements()
	{
		if (QuestsCompleted >= 40)
		{
			Game1.getAchievement(30);
			Game1.addMailForTomorrow("quest35");
		}
		if (QuestsCompleted >= 10)
		{
			Game1.getAchievement(29);
			Game1.addMailForTomorrow("quest10");
		}
	}

	public void checkForFriendshipAchievements()
	{
		uint numberOf5Level = 0u;
		uint numberOf8Level = 0u;
		uint numberOf10Level = 0u;
		foreach (Friendship value in Game1.player.friendshipData.Values)
		{
			if (value.Points >= 2500)
			{
				numberOf10Level++;
			}
			if (value.Points >= 2000)
			{
				numberOf8Level++;
			}
			if (value.Points >= 1250)
			{
				numberOf5Level++;
			}
		}
		GoodFriends = numberOf8Level;
		if (numberOf5Level >= 20)
		{
			Game1.getAchievement(13);
		}
		if (numberOf5Level >= 10)
		{
			Game1.getAchievement(12);
		}
		if (numberOf5Level >= 4)
		{
			Game1.getAchievement(11);
		}
		if (numberOf5Level >= 1)
		{
			Game1.getAchievement(6);
		}
		if (numberOf10Level >= 8)
		{
			Game1.getAchievement(9);
		}
		if (numberOf10Level >= 1)
		{
			Game1.getAchievement(7);
		}
		foreach (KeyValuePair<string, string> pair in DataLoader.CookingRecipes(Game1.content))
		{
			string recipeKey = pair.Key;
			string[] getConditions = ArgUtility.SplitBySpace(ArgUtility.Get(pair.Value.Split('/'), 3));
			if (!(ArgUtility.Get(getConditions, 0) != "f"))
			{
				string npcName = ArgUtility.Get(getConditions, 1);
				int minHearts = ArgUtility.GetInt(getConditions, 2);
				if (npcName != null && Game1.player.friendshipData.TryGetValue(npcName, out var friendship) && friendship.Points >= minHearts * 250 && !Game1.player.cookingRecipes.ContainsKey(recipeKey) && !Game1.player.hasOrWillReceiveMail(npcName + "Cooking"))
				{
					Game1.addMailForTomorrow(npcName + "Cooking");
				}
			}
		}
		foreach (KeyValuePair<string, string> pair in DataLoader.CraftingRecipes(Game1.content))
		{
			string recipeKey = pair.Key;
			string[] getConditions = ArgUtility.SplitBySpace(ArgUtility.Get(pair.Value.Split('/'), 4));
			if (!(ArgUtility.Get(getConditions, 0) != "f"))
			{
				string npcName = ArgUtility.Get(getConditions, 1);
				int minHearts = ArgUtility.GetInt(getConditions, 2);
				if (npcName != null && Game1.player.friendshipData.TryGetValue(npcName, out var friendship) && friendship.Points >= minHearts * 250 && !Game1.player.craftingRecipes.ContainsKey(recipeKey) && !Game1.player.hasOrWillReceiveMail(npcName + "Crafting"))
				{
					Game1.addMailForTomorrow(npcName + "Crafting");
				}
			}
		}
	}

	public bool isSharedAchievement(int which)
	{
		if ((uint)which <= 5u || which == 28)
		{
			return true;
		}
		return false;
	}

	public void checkForAchievements()
	{
		checkForCookingAchievements();
		checkForCraftingAchievements();
		checkForShippingAchievements();
		checkForFishingAchievements();
		checkForArchaeologyAchievements();
		checkForMoneyAchievements();
		checkForBuildingUpgradeAchievements();
		checkForQuestAchievements();
		checkForFriendshipAchievements();
		Game1.player.hasCompletedAllMonsterSlayerQuests.Value = AdventureGuild.areAllMonsterSlayerQuestsComplete();
	}
}
