namespace StardewValley.Constants;

/// <summary>The stat keys which can be used with methods like <see cref="M:StardewValley.Stats.Get(System.String)" /> and <see cref="M:StardewValley.Stats.Set(System.String,System.UInt32)" />.</summary>
public class StatKeys
{
	public const string AverageBedtime = "averageBedtime";

	public const string BeachFarmSpawns = "beachFarmSpawns";

	public const string BeveragesMade = "beveragesMade";

	public const string BillboardQuestsDone = "BillboardQuestsDone";

	public const string BlessingOfWaters = "blessingOfWaters";

	public const string BoatRidesToIsland = "boatRidesToIsland";

	public const string Book_Bomb = "Book_Bombs";

	public const string Book_Crabbing = "Book_Crabbing";

	public const string Book_Defense = "Book_Defense";

	public const string Book_Friendship = "Book_Friendship";

	public const string Book_Marlon = "Book_Marlon";

	public const string Book_PriceCatalogue = "Book_PriceCatalogue";

	public const string Book_Roe = "Book_Roe";

	public const string Book_Speed = "Book_Speed";

	public const string Book_Speed2 = "Book_Speed2";

	public const string Book_Trash = "Book_Trash";

	public const string Book_Void = "Book_Void";

	public const string Book_WildSeeds = "Book_WildSeeds";

	public const string Book_Woodcutting = "Book_Woodcutting";

	public const string Book_Diamonds = "Book_Diamonds";

	public const string Book_Mystery = "Book_Mystery";

	public const string Book_AnimalCatalogue = "Book_AnimalCatalogue";

	public const string Book_Horse = "Book_Horse";

	public const string Book_Artifact = "Book_Artifact";

	public const string Book_Grass = "Book_Grass";

	public const string CaveCarrotsFound = "caveCarrotsFound";

	public const string CheeseMade = "cheeseMade";

	public const string ChickenEggsLayed = "chickenEggsLayed";

	public const string ChildrenTurnedToDoves = "childrenTurnedToDoves";

	public const string CopperFound = "copperFound";

	public const string CowMilkProduced = "cowMilkProduced";

	public const string CropsShipped = "cropsShipped";

	public const string DaysPlayed = "daysPlayed";

	public const string DiamondsFound = "diamondsFound";

	public const string DirtHoed = "dirtHoed";

	public const string DuckEggsLayed = "duckEggsLayed";

	public const string ExMemoriesWiped = "exMemoriesWiped";

	public const string FishCaught = "fishCaught";

	public const string GeodesCracked = "geodesCracked";

	public const string GiftsGiven = "giftsGiven";

	public const string GoatCheeseMade = "goatCheeseMade";

	public const string GoatMilkProduced = "goatMilkProduced";

	public const string GoldenTagsTurnedIn = "GoldenTagsTurnedIn";

	public const string GoldFound = "goldFound";

	public const string GoodFriends = "goodFriends";

	public const string HardModeMonstersKilled = "hardModeMonstersKilled";

	public const string IndividualMoneyEarned = "individualMoneyEarned";

	public const string IridiumFound = "iridiumFound";

	public const string IronFound = "ironFound";

	public const string ItemsCooked = "itemsCooked";

	public const string ItemsCrafted = "itemsCrafted";

	public const string ItemsForaged = "itemsForaged";

	public const string ItemsShipped = "itemsShipped";

	public const string MasteryExp = "MasteryExp";

	public const string MasteryLevelsSpent = "masteryLevelsSpent";

	public const string MonstersKilled = "monstersKilled";

	public const string MossHarvested = "mossHarvested";

	public const string MysteryBoxesOpened = "MysteryBoxesOpened";

	public const string MysticStonesCrushed = "mysticStonesCrushed";

	public const string NotesFound = "notesFound";

	public const string OtherPreciousGemsFound = "otherPreciousGemsFound";

	public const string PiecesOfTrashRecycled = "piecesOfTrashRecycled";

	public const string PreservesMade = "preservesMade";

	public const string PrismaticShardsFound = "prismaticShardsFound";

	public const string QuestsCompleted = "questsCompleted";

	public const string RabbitWoolProduced = "rabbitWoolProduced";

	public const string RocksCrushed = "rocksCrushed";

	public const string SheepWoolProduced = "sheepWoolProduced";

	public const string SlimesKilled = "slimesKilled";

	public const string SpecialOrderPrizeTickets = "specialOrderPrizeTickets";

	public const string StepsTaken = "stepsTaken";

	public const string StoneGathered = "stoneGathered";

	public const string StumpsChopped = "stumpsChopped";

	public const string TicketPrizesClaimed = "ticketPrizesClaimed";

	public const string TimesEnchanted = "timesEnchanted";

	public const string TimesFished = "timesFished";

	public const string TimesUnconscious = "timesUnconscious";

	public const string TotalMoneyGifted = "totalMoneyGifted";

	public const string TrashCansChecked = "trashCansChecked";

	public const string TrinketSlots = "trinketSlots";

	public const string TrufflesFound = "trufflesFound";

	public const string WeedsEliminated = "weedsEliminated";

	public const string WildTreesPlanted = "wildtreesplanted";

	public const string SeedsSown = "seedsSown";

	public static string Mastery(int skill)
	{
		return $"mastery_{skill}";
	}

	public static string SquidFestScore(int dayOfMonth, int year)
	{
		return $"SquidFestScore_{dayOfMonth}_{year}";
	}
}
