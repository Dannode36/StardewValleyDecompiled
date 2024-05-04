namespace StardewValley.SaveMigrations;

/// <summary>The sequential save migration IDs that can be applied to a save.</summary>
public enum SaveFixes
{
	/// <summary>Do nothing.</summary>
	NONE = 0,
	/// <summary>For Stardew Valley 1.4, fix pre-1.4 big craftables that are already stored in chests potentially having a stack count of 0 (due to maximumStackSize being set to -1 which was then Math.Max-ed to 0).</summary>
	StoredBigCraftablesStackFix = 1,
	/// <summary>For Stardew Valley 1.4, remove bushes underneath cabins (this was a problem in the wilderness farm).</summary>
	PorchedCabinBushesFix = 2,
	/// <summary>For Stardew Valley 1.4, update Obelisk buildings' size from 3x3 tiles to 3x2.</summary>
	ChangeObeliskFootprintHeight = 3,
	/// <summary>For Stardew Valley 1.4, convert dressers to StorageFurniture and fix the category for clothing items.</summary>
	CreateStorageDressers = 4,
	/// <summary>For Stardew Valley 1.4, fix pre-1.2 preserves items not knowing what item they were created with, causing them to change their names incorrectly.</summary>
	InferPreserves = 5,
	/// <summary>For Stardew Valley 1.4, reload data for hats from the data sheet.</summary>
	TransferHatSkipHairFlag = 6,
	/// <summary>For Stardew Valley 1.4, reveal any gift preferences that would've been revealed via secret notes.</summary>
	RevealSecretNoteItemTastes = 7,
	/// <summary>For Stardew Valley 1.4, transfer the "honeyType" value to "preservedParentSheetIndex" so that honey can be spawned from any flower.</summary>
	TransferHoneyTypeToPreserves = 8,
	/// <summary>For Stardew Valley 1.4, transfer the note block pitch/sound effect from "scale.X" (which isn't synchronized in multiplayer) to preservedParentSheetIndex (which is synchronized and otherwise unused).</summary>
	TransferNoteBlockScale = 9,
	/// <summary>For Stardew Valley 1.4, update pre-1.4 crops to avoid yielding extra produce due to a fix in the crop yield calculation. This also infers the seed index, since the netSeedIndex value wasn't previously serialized to the save data.</summary>
	FixCropHarvestAmountsAndInferSeedIndex = 10,
	/// <summary>For Stardew Valley 1.4, add the bushes near the quarry mine entrance.</summary>
	quarryMineBushes = 13,
	/// <summary>For Stardew Valley 1.4.4, add the Qi's Challenge quest for players who accidentally dismissed the letter without starting the quest, causing Secret Note 10 to be unobtainable.</summary>
	MissingQisChallenge = 14,
	/// <summary>For Stardew Valley 1.5, convert player bed map tiles into furniture.</summary>
	BedsToFurniture = 15,
	/// <summary>For Stardew Valley 1.5, convert child bed map tiles into furniture.</summary>
	ChildBedsToFurniture = 16,
	/// <summary>For Stardew Valley 1.5, convert the shipping bin into a building.</summary>
	ModularizeFarmStructures = 17,
	/// <summary>For Stardew Valley 1.5, recalculate flooring attributes (like isSteppingStone) that weren't properly serialized/deserialized due to being private.</summary>
	FixFlooringFlags = 18,
	/// <summary>For Stardew Valley 1.5, fix stable owner values.</summary>
	FixStableOwnership = 19,
	/// <summary>For Stardew Valley 1.5, add the bush where the bulletin board appears.</summary>
	AddTownBush = 21,
	/// <summary>For Stardew Valley 1.5, reset all weapon stats to reflect any changes in buffs.</summary>
	ResetForges = 23,
	/// <summary>For Stardew Valley 1.5, add the Vampiric enchantment to the Vampiric Sword.</summary>
	MakeDarkSwordVampiric = 25,
	/// <summary>For Stardew Valley 1.5, fix beach farm bushes in the spouse area for beta players.</summary>
	FixBeachFarmBushes = 27,
	/// <summary>For Stardew Valley 1.5, fix beta Ostrich Incubators being marked as indestructible.</summary>
	OstrichIncubatorFragility = 30,
	/// <summary>For Stardew Valley 1.5, rename any kids named Leo.</summary>
	LeoChildrenFix = 32,
	/// <summary>For Stardew Valley 1.5.2, fix an issue where a previous typo in the German localization prevented Leo from moving to the mainland after the 6-Heart event. This automatically adds the mail if any player has seen the event but the host doesn't have the mail.</summary>
	Leo6HeartGermanFix = 33,
	/// <summary>For Stardew Valley 1.5.2, fix Birdie's quest being cancelable and re-add it if needed.</summary>
	BirdieQuestRemovedFix = 34,
	/// <summary>For Stardew Valley 1.5.2, if the farm is eternal, iterate through all players who are flagged as having seen the Summit event and remove the mail from anyone who hasn't heard the ending song. This will cause anyone who's playing the game with music muted to be able to re-visit the event (as well as anyone who skipped the event), but will at least ensure they get the song in their jukebox.</summary>
	SkippedSummit = 35,
	/// <summary>For Stardew Valley 1.6, convert existing buildings to data.</summary>
	MigrateBuildingsToData = 37,
	/// <summary>For Stardew Valley 1.6, change the farmhouse into a building.</summary>
	ModularizeFarmhouse = 38,
	/// <summary>For Stardew Valley 1.6, migrate pets to the new data and change the pet bowl into a building.</summary>
	ModularizePets = 39,
	/// <summary>For Stardew Valley 1.6, add mail flags for NPCs that are removed conditionally via <c>Data/Characters</c>.</summary>
	AddNpcRemovalFlags = 42,
	/// <summary>For Stardew Valley 1.6, remove farmhands from cabins.</summary>
	MigrateFarmhands = 44,
	/// <summary>For Stardew Valley 1.6, apply the new litter item category/type.</summary>
	MigrateLitterItemData = 45,
	/// <summary>For Stardew Valley 1.6, add honey preserve info and fix pre-1.4 data to remove hacks in the Object display name logic.</summary>
	MigrateHoneyItems = 47,
	/// <summary>For Stardew Valley 1.6, pre-populate <see cref="F:StardewValley.Object.lastInputItem" /> and <see cref="F:StardewValley.Object.lastOutputRuleId" /> for machines so they keep the recalculate-on-collect behavior from 1.5.6.</summary>
	MigrateMachineLastOutputRule = 48,
	/// <summary>For Stardew Valley 1.6, standardize the number of fields in <see cref="F:StardewValley.SaveGame.bundleData" />.</summary>
	StandardizeBundleFields = 49,
	/// <summary>For Stardew Valley 1.6, migrate mail flags used to track Adventurer's Guild monster slayer goals.</summary>
	MigrateAdventurerGoalFlags = 51,
	/// <summary>For Stardew Valley 1.6, set the <see cref="F:StardewValley.Crop.netSeedIndex" /> value for crops created before Stardew Valley 1.4.</summary>
	SetCropSeedId = 53,
	/// <summary>For Stardew Valley 1.6, set the tile position for the mine boulder to fix collisions.</summary>
	FixMineBoulderCollisions = 54,
	/// <summary>For Stardew Valley 1.6, assign the main player's pet to the new Pet Bowl building.</summary>
	MigratePetAndPetBowlIds = 56,
	/// <summary>For Stardew Valley 1.6, migrate house paint from <see cref="F:StardewValley.Farm.housePaintColor" /> to the new building.</summary>
	MigrateHousePaint = 58,
	/// <summary>For Stardew Valley 1.6, update sheds for the new floor/wall IDs.</summary>
	MigrateShedFloorWallIds = 61,
	/// <summary>For Stardew Valley 1.6, update all items for the new item IDs.</summary>
	MigrateItemIds = 62,
	/// <summary>For Stardew Valley 1.6, remove the unimplemented meat items from the animal bundle. (Invalid items would previously be silently hidden, but they now appear as Error Items instead.)</summary>
	RemoveMeatFromAnimalBundle = 63,
	/// <summary>For Stardew Valley 1.6, move the legacy stat fields like <see cref="F:StardewValley.Stats.obsolete_averageBedtime" /> into <see cref="F:StardewValley.Stats.Values" />.</summary>
	MigrateStatFields = 64,
	/// <summary>For Stardew Valley 1.6, remove some bushes in front of the mastery room.</summary>
	RemoveMasteryRoomFoliage = 65,
	/// <summary>For Stardew Valley 1.6, convert town static map trees into real trees.</summary>
	AddTownTrees = 66,
	/// <summary>For Stardew Valley 1.6, update coordinates for new bus stop, and do lost and found work for buildings layer changes</summary>
	MapAdjustments_1_6 = 67,
	/// <summary>For Stardew Valley 1.6, move the legacy wallet item fields like <see cref="F:StardewValley.Farmer.obsolete_hasRustyKey" /> into <see cref="F:StardewValley.Farmer.mailReceived" />.</summary>
	MigrateWalletItems = 68,
	/// <summary>For Stardew Valley 1.6, move <see cref="F:StardewValley.Locations.Forest.obsolete_log" /> and <see cref="T:StardewValley.Locations.Woods" />' <c>stumps</c> into <see cref="F:StardewValley.GameLocation.resourceClumps" />.</summary>
	MigrateResourceClumps = 69,
	/// <summary>For Stardew Valley 1.6, correct <see cref="P:StardewValley.Tool.AttachmentSlotsCount" /> for pre-existing fishing rods. Previously they'd always be constructed with two slots, and then the fishing rod wouldn't use them if they didn't apply. In 1.6, attachment slot are a generic tool feature so the count needs to be correct.</summary>
	MigrateFishingRodAttachmentSlots = 70,
	/// <summary> slime hutches are smaller now, so move them +2, +2 so that they are still "centered" in the spots people ahve placed them </summary>
	MoveSlimeHutches = 72,
	/// <summary>For Stardew Valley 1.6, retroactively populate the <see cref="F:StardewValley.Farmer.locationsVisited" /> field.</summary>
	AddLocationsVisited = 74,
	/// <summary>For Stardew Valley 1.6, set <see cref="F:StardewValley.Objects.Chest.giftboxIsStarterGift" /> for an current farmhouse gift boxes.</summary>
	MarkStarterGiftBoxes = 75,
	/// <summary>For Stardew Valley 1.6, migrate the former send-mail events in <see cref="F:StardewValley.Farmer.eventsSeen" /> to <see cref="F:StardewValley.Farmer.triggerActionsRun" />.</summary>
	MigrateMailEventsToTriggerActions = 76,
	/// <summary>For Stardew Valley 1.6, shift furniture and items in upgraded farmhouses to account for the expansion of the farmhouse map up and to the left.</summary>
	ShiftFarmHouseFurnitureForExpansion = 77,
	/// <summary>For Stardew Valley 1.6, migrate preserve items to fix their preserved index value and convert to <see cref="T:StardewValley.Objects.ColoredObject" />.</summary>
	MigratePreservesTo16 = 78,
	/// <summary>For Stardew Valley 1.6, migrate saved quest data to the new format.</summary>
	MigrateQuestDataTo16 = 79,
	/// <summary>For Stardew Valley 1.6, set the <see cref="F:StardewValley.TerrainFeatures.Bush.inPot" /> field.</summary>
	SetBushesInPots = 80,
	/// <summary>For Stardew Valley 1.6, fix some pre-1.6 items not marked as being in the player's inventory.</summary>
	FixItemsNotMarkedAsInInventory = 81,
	/// <summary>For Stardew Valley 1.6, fix issues which only affect beta saves.</summary>
	BetaFixesFor16 = 82,
	/// <summary>For Stardew Valley 1.6, fix pre-1.6 basic wines .</summary>
	FixBasicWines = 83,
	/// <summary>For Stardew Valley 1.6, reset forges again.</summary>
	ResetForges_1_6 = 84,
	/// <summary>For Stardew Valley 1.6, grant ancient seed recipe if it was missed.</summary>
	RestoreAncientSeedRecipe_1_6 = 85,
	/// <summary>For Stardew Valley 1.6, fix any instanced indoor locations that are missing a unique name.</summary>
	FixInstancedInterior = 86,
	/// <summary>For Stardew Valley 1.6 Beta, fix any indoor locations affected by the beta-only FixAnimalHouses save fix.</summary>
	FixNonInstancedInterior = 87,
	/// <summary>For Stardew Valley 1.6, populate the new <see cref="F:StardewValley.FarmerTeam.constructedBuildings" /> field retroactively.</summary>
	PopulateConstructedBuildings = 88,
	/// <summary>For Stardew Valley 1.6, remove racoon quest from every player's quest log if it has already been completed.</summary>
	FixRacoonQuestCompletion = 89,
	MAX = 90
}
