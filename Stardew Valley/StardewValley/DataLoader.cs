using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using StardewValley.GameData;
using StardewValley.GameData.BigCraftables;
using StardewValley.GameData.Buffs;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.Bundles;
using StardewValley.GameData.Characters;
using StardewValley.GameData.Crafting;
using StardewValley.GameData.Crops;
using StardewValley.GameData.FarmAnimals;
using StardewValley.GameData.Fences;
using StardewValley.GameData.FishPonds;
using StardewValley.GameData.FloorsAndPaths;
using StardewValley.GameData.FruitTrees;
using StardewValley.GameData.GarbageCans;
using StardewValley.GameData.GiantCrops;
using StardewValley.GameData.HomeRenovations;
using StardewValley.GameData.LocationContexts;
using StardewValley.GameData.Locations;
using StardewValley.GameData.Machines;
using StardewValley.GameData.MakeoverOutfits;
using StardewValley.GameData.Minecarts;
using StardewValley.GameData.Movies;
using StardewValley.GameData.Museum;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Pants;
using StardewValley.GameData.Pets;
using StardewValley.GameData.Powers;
using StardewValley.GameData.Shirts;
using StardewValley.GameData.Shops;
using StardewValley.GameData.SpecialOrders;
using StardewValley.GameData.Tools;
using StardewValley.GameData.Weapons;
using StardewValley.GameData.Weddings;
using StardewValley.GameData.WildTrees;
using StardewValley.GameData.WorldMaps;

namespace StardewValley;

/// <summary>Loads vanilla data assets with the right name and type.</summary>
public static class DataLoader
{
	/// <summary>Load the <c>Data/Achievements</c> asset, which adds the data for unlockable achievements.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	/// <remarks>Most code should use <see cref="F:StardewValley.Game1.achievements" /> instead.</remarks>
	public static Dictionary<int, string> Achievements(LocalizedContentManager content)
	{
		return Load<Dictionary<int, string>>(content, "Data\\Achievements");
	}

	/// <summary>Load the <c>Data/AdditionalFarms</c> asset, which defines farm type data beyond the original types.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	/// <remarks>See also <see cref="F:StardewValley.Game1.whichModFarm" />.</remarks>
	public static List<ModFarmType> AdditionalFarms(LocalizedContentManager content)
	{
		return Load<List<ModFarmType>>(content, "Data\\AdditionalFarms");
	}

	/// <summary>Load the <c>Data/AdditionalLanguages</c> asset, which defines custom display languages.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	public static List<ModLanguage> AdditionalLanguages(LocalizedContentManager content)
	{
		return Load<List<ModLanguage>>(content, "Data\\AdditionalLanguages");
	}

	/// <summary>Load the <c>Data/AdditionalWallpaperFlooring</c> asset, which defines custom indoor wallpaper and floor styles.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	public static List<ModWallpaperOrFlooring> AdditionalWallpaperFlooring(LocalizedContentManager content)
	{
		return Load<List<ModWallpaperOrFlooring>>(content, "Data\\AdditionalWallpaperFlooring");
	}

	/// <summary>Load the <c>Data/animationDescriptions</c> asset, which defines animations used in NPC scheduling.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	public static Dictionary<string, string> AnimationDescriptions(LocalizedContentManager content)
	{
		return Load<Dictionary<string, string>>(content, "Data\\animationDescriptions");
	}

	/// <summary>Load the <c>Data/AquariumFish</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	public static Dictionary<string, string> AquariumFish(LocalizedContentManager content)
	{
		return Load<Dictionary<string, string>>(content, "Data\\AquariumFish");
	}

	/// <summary>Load the <c>Data/AudioChanges</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	public static Dictionary<string, AudioCueData> AudioChanges(LocalizedContentManager content)
	{
		return Load<Dictionary<string, AudioCueData>>(content, "Data\\AudioChanges");
	}

	/// <summary>Load the <c>Data/BigCraftables</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	/// <remarks>Most code should use <see cref="F:StardewValley.Game1.bigCraftableData" /> instead.</remarks>
	public static Dictionary<string, BigCraftableData> BigCraftables(LocalizedContentManager content)
	{
		return Load<Dictionary<string, BigCraftableData>>(content, "Data\\BigCraftables");
	}

	/// <summary>Load the <c>Data/Boots</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	public static Dictionary<string, string> Boots(LocalizedContentManager content)
	{
		return Load<Dictionary<string, string>>(content, "Data\\Boots");
	}

	/// <summary>Load the <c>Data/Buffs</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	public static Dictionary<string, BuffData> Buffs(LocalizedContentManager content)
	{
		return Load<Dictionary<string, BuffData>>(content, "Data\\Buffs");
	}

	/// <summary>Load the <c>Data/Buildings</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	/// <remarks>Most code should use <see cref="F:StardewValley.Game1.buildingData" /> instead.</remarks>
	public static Dictionary<string, BuildingData> Buildings(LocalizedContentManager content)
	{
		return Load<Dictionary<string, BuildingData>>(content, "Data\\Buildings");
	}

	/// <summary>Load the <c>Data/Bundles</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	public static Dictionary<string, string> Bundles(LocalizedContentManager content)
	{
		return Load<Dictionary<string, string>>(content, "Data\\Bundles");
	}

	/// <summary>Load the <c>Data/ChairTiles</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	public static Dictionary<string, string> ChairTiles(LocalizedContentManager content)
	{
		return Load<Dictionary<string, string>>(content, "Data\\ChairTiles");
	}

	/// <summary>Load the <c>Data/Characters</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	/// <remarks>Most code should use <see cref="F:StardewValley.Game1.characterData" /> instead.</remarks>
	public static Dictionary<string, CharacterData> Characters(LocalizedContentManager content)
	{
		return Load<Dictionary<string, CharacterData>>(content, "Data\\Characters");
	}

	/// <summary>Load the <c>Data/Concessions</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	public static List<ConcessionItemData> Concessions(LocalizedContentManager content)
	{
		return Load<List<ConcessionItemData>>(content, "Data\\Concessions");
	}

	/// <summary>Load the <c>Data/ConcessionTastes</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	public static List<ConcessionTaste> ConcessionTastes(LocalizedContentManager content)
	{
		return Load<List<ConcessionTaste>>(content, "Data\\ConcessionTastes");
	}

	/// <summary>Load the <c>Data/CookingRecipes</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	/// <remarks>Most code should use <see cref="F:StardewValley.CraftingRecipe.cookingRecipes" /> instead.</remarks>
	public static Dictionary<string, string> CookingRecipes(LocalizedContentManager content)
	{
		return Load<Dictionary<string, string>>(content, "Data\\CookingRecipes");
	}

	/// <summary>Load the <c>Data/CraftingRecipes</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	/// <remarks>Most code should use <see cref="F:StardewValley.CraftingRecipe.craftingRecipes" /> instead.</remarks>
	public static Dictionary<string, string> CraftingRecipes(LocalizedContentManager content)
	{
		return Load<Dictionary<string, string>>(content, "Data\\CraftingRecipes");
	}

	/// <summary>Load the <c>Data/Crops</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	/// <remarks>Most code should use <see cref="F:StardewValley.Game1.cropData" /> instead.</remarks>
	public static Dictionary<string, CropData> Crops(LocalizedContentManager content)
	{
		return Load<Dictionary<string, CropData>>(content, "Data\\Crops");
	}

	/// <summary>Load the <c>Data/EngagementDialogue</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	public static Dictionary<string, string> EngagementDialogue(LocalizedContentManager content)
	{
		return Load<Dictionary<string, string>>(content, "Data\\EngagementDialogue");
	}

	/// <summary>Load the <c>Data/FarmAnimals</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	/// <remarks>Most code should use <see cref="F:StardewValley.Game1.farmAnimalData" /> instead.</remarks>
	public static Dictionary<string, FarmAnimalData> FarmAnimals(LocalizedContentManager content)
	{
		return Load<Dictionary<string, FarmAnimalData>>(content, "Data\\FarmAnimals");
	}

	/// <summary>Load the <c>Data/Fences</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	/// <remarks>Most code should use <see cref="M:StardewValley.Fence.GetFenceLookup" /> instead.</remarks>
	public static Dictionary<string, FenceData> Fences(LocalizedContentManager content)
	{
		return Load<Dictionary<string, FenceData>>(content, "Data\\Fences");
	}

	/// <summary>Load the <c>Data/Festivals/FestivalDates</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	public static Dictionary<string, string> Festivals_FestivalDates(LocalizedContentManager content)
	{
		return Load<Dictionary<string, string>>(content, "Data\\Festivals\\FestivalDates");
	}

	/// <summary>Load the <c>Data/Fish</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	public static Dictionary<string, string> Fish(LocalizedContentManager content)
	{
		return Load<Dictionary<string, string>>(content, "Data\\Fish");
	}

	/// <summary>Load the <c>Data/FishPondData</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	public static List<FishPondData> FishPondData(LocalizedContentManager content)
	{
		return Load<List<FishPondData>>(content, "Data\\FishPondData");
	}

	/// <summary>Load the <c>Data/FloorsAndPaths</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	/// <remarks>Most code should use <see cref="F:StardewValley.Game1.floorPathData" /> instead.</remarks>
	public static Dictionary<string, FloorPathData> FloorsAndPaths(LocalizedContentManager content)
	{
		return Load<Dictionary<string, FloorPathData>>(content, "Data\\FloorsAndPaths");
	}

	/// <summary>Load the <c>Data/FruitTrees</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	/// <remarks>Most code should use <see cref="F:StardewValley.Game1.fruitTreeData" /> instead.</remarks>
	public static Dictionary<string, FruitTreeData> FruitTrees(LocalizedContentManager content)
	{
		return Load<Dictionary<string, FruitTreeData>>(content, "Data\\FruitTrees");
	}

	/// <summary>Load the <c>Data/Furniture</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	public static Dictionary<string, string> Furniture(LocalizedContentManager content)
	{
		return Load<Dictionary<string, string>>(content, "Data\\Furniture");
	}

	/// <summary>Load the <c>Data/GarbageCans</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	public static GarbageCanData GarbageCans(LocalizedContentManager content)
	{
		return Load<GarbageCanData>(content, "Data\\GarbageCans");
	}

	/// <summary>Load the <c>Data/GiantCrops</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	public static Dictionary<string, GiantCropData> GiantCrops(LocalizedContentManager content)
	{
		return Load<Dictionary<string, GiantCropData>>(content, "Data\\GiantCrops");
	}

	/// <summary>Load the <c>Data/HairData</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	public static Dictionary<int, string> HairData(LocalizedContentManager content)
	{
		return Load<Dictionary<int, string>>(content, "Data\\HairData");
	}

	/// <summary>Load the <c>Data/hats</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	public static Dictionary<string, string> Hats(LocalizedContentManager content)
	{
		return Load<Dictionary<string, string>>(content, "Data\\hats");
	}

	/// <summary>Load the <c>Data/HomeRenovations</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	public static Dictionary<string, HomeRenovation> HomeRenovations(LocalizedContentManager content)
	{
		return Load<Dictionary<string, HomeRenovation>>(content, "Data\\HomeRenovations");
	}

	/// <summary>Load the <c>Data/IncomingPhoneCalls</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	public static Dictionary<string, IncomingPhoneCallData> IncomingPhoneCalls(LocalizedContentManager content)
	{
		return Load<Dictionary<string, IncomingPhoneCallData>>(content, "Data\\IncomingPhoneCalls");
	}

	/// <summary>Load the <c>Data/JukeboxTracks</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	/// <remarks>Most code should use <see cref="F:StardewValley.Game1.jukeboxTrackData" /> instead.</remarks>
	public static Dictionary<string, JukeboxTrackData> JukeboxTracks(LocalizedContentManager content)
	{
		return Load<Dictionary<string, JukeboxTrackData>>(content, "Data\\JukeboxTracks");
	}

	/// <summary>Load the <c>Data/LocationContexts</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	/// <remarks>Most code should use <see cref="F:StardewValley.Game1.locationContextData" /> instead.</remarks>
	public static Dictionary<string, LocationContextData> LocationContexts(LocalizedContentManager content)
	{
		return Load<Dictionary<string, LocationContextData>>(content, "Data\\LocationContexts");
	}

	/// <summary>Load the <c>Data/Locations</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	public static Dictionary<string, LocationData> Locations(LocalizedContentManager content)
	{
		return Load<Dictionary<string, LocationData>>(content, "Data\\Locations");
	}

	/// <summary>Load the <c>Data/Machines</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	public static Dictionary<string, MachineData> Machines(LocalizedContentManager content)
	{
		return Load<Dictionary<string, MachineData>>(content, "Data\\Machines");
	}

	/// <summary>Load the <c>Data/mail</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	public static Dictionary<string, string> Mail(LocalizedContentManager content)
	{
		return Load<Dictionary<string, string>>(content, "Data\\mail");
	}

	/// <summary>Load the <c>Data/MakeoverOutfits</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	public static List<MakeoverOutfit> MakeoverOutfits(LocalizedContentManager content)
	{
		return content.Load<List<MakeoverOutfit>>("Data\\MakeoverOutfits");
	}

	/// <summary>Load the <c>Data/Mannequins</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	public static Dictionary<string, MannequinData> Mannequins(LocalizedContentManager content)
	{
		return content.Load<Dictionary<string, MannequinData>>("Data\\Mannequins");
	}

	/// <summary>Load the <c>Data/Minecarts</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	public static Dictionary<string, MinecartNetworkData> Minecarts(LocalizedContentManager content)
	{
		return Load<Dictionary<string, MinecartNetworkData>>(content, "Data\\Minecarts");
	}

	/// <summary>Load the <c>Data/Monsters</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	public static Dictionary<string, string> Monsters(LocalizedContentManager content)
	{
		return Load<Dictionary<string, string>>(content, "Data\\Monsters");
	}

	/// <summary>Load the <c>Data/MonsterSlayerQuests</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	public static Dictionary<string, MonsterSlayerQuestData> MonsterSlayerQuests(LocalizedContentManager content)
	{
		return Load<Dictionary<string, MonsterSlayerQuestData>>(content, "Data\\MonsterSlayerQuests");
	}

	/// <summary>Load the <c>Data/Movies</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	/// <remarks>Most code should use <see cref="M:StardewValley.Locations.MovieTheater.GetMovieData" /> instead.</remarks>
	public static List<MovieData> Movies(LocalizedContentManager content)
	{
		return Load<List<MovieData>>(content, "Data\\Movies");
	}

	/// <summary>Load the <c>Data/MoviesReactions</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	/// <remarks>Most code should use <see cref="M:StardewValley.Locations.MovieTheater.GetMovieReactions" /> instead.</remarks>
	public static List<MovieCharacterReaction> MoviesReactions(LocalizedContentManager content)
	{
		return Load<List<MovieCharacterReaction>>(content, "Data\\MoviesReactions");
	}

	/// <summary>Load the <c>Data/MuseumRewards</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	public static Dictionary<string, MuseumRewards> MuseumRewards(LocalizedContentManager content)
	{
		return Load<Dictionary<string, MuseumRewards>>(content, "Data\\MuseumRewards");
	}

	/// <summary>Load the <c>Data/NPCGiftTastes</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	/// <remarks>Most code should use <see cref="F:StardewValley.Game1.NPCGiftTastes" /> instead.</remarks>
	public static Dictionary<string, string> NpcGiftTastes(LocalizedContentManager content)
	{
		return Load<Dictionary<string, string>>(content, "Data\\NPCGiftTastes");
	}

	/// <summary>Load the <c>Data/Objects</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	/// <remarks>Most code should use <see cref="F:StardewValley.Game1.objectData" /> instead.</remarks>
	public static Dictionary<string, ObjectData> Objects(LocalizedContentManager content)
	{
		return Load<Dictionary<string, ObjectData>>(content, "Data\\Objects");
	}

	/// <summary>Load the <c>Data/PaintData</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	public static Dictionary<string, string> PaintData(LocalizedContentManager content)
	{
		return Load<Dictionary<string, string>>(content, "Data\\PaintData");
	}

	/// <summary>Load the <c>Data/Pants</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	/// <remarks>Most code should use <see cref="F:StardewValley.Game1.pantsData" /> instead.</remarks>
	public static Dictionary<string, PantsData> Pants(LocalizedContentManager content)
	{
		return Load<Dictionary<string, PantsData>>(content, "Data\\Pants");
	}

	/// <summary>Load the <c>Data/PassiveFestivals</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	public static Dictionary<string, PassiveFestivalData> PassiveFestivals(LocalizedContentManager content)
	{
		return Load<Dictionary<string, PassiveFestivalData>>(content, "Data\\PassiveFestivals");
	}

	/// <summary>Load the <c>Data/Pets</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	/// <remarks>Most code should use <see cref="F:StardewValley.Game1.petData" /> instead.</remarks>
	public static Dictionary<string, PetData> Pets(LocalizedContentManager content)
	{
		return Load<Dictionary<string, PetData>>(content, "Data\\Pets");
	}

	/// <summary>Load the <c>Data/Powers</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	public static Dictionary<string, PowersData> Powers(LocalizedContentManager content)
	{
		return content.Load<Dictionary<string, PowersData>>("Data\\Powers");
	}

	/// <summary>Load the <c>Data/Quests</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	public static Dictionary<string, string> Quests(LocalizedContentManager content)
	{
		return Load<Dictionary<string, string>>(content, "Data\\Quests");
	}

	/// <summary>Load the <c>Data/RandomBundles</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	public static List<RandomBundleData> RandomBundles(LocalizedContentManager content)
	{
		return Load<List<RandomBundleData>>(content, "Data\\RandomBundles");
	}

	/// <summary>Load the <c>Data/SecretNotes</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	public static Dictionary<int, string> SecretNotes(LocalizedContentManager content)
	{
		return Load<Dictionary<int, string>>(content, "Data\\SecretNotes");
	}

	/// <summary>Load the <c>Data/Shirts</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	/// <remarks>Most code should use <see cref="F:StardewValley.Game1.shirtData" /> instead.</remarks>
	public static Dictionary<string, ShirtData> Shirts(LocalizedContentManager content)
	{
		return Load<Dictionary<string, ShirtData>>(content, "Data\\Shirts");
	}

	/// <summary>Load the <c>Data/Shops</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	public static Dictionary<string, ShopData> Shops(LocalizedContentManager content)
	{
		return Load<Dictionary<string, ShopData>>(content, "Data\\Shops");
	}

	/// <summary>Load the <c>Data/SpecialOrders</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	public static Dictionary<string, SpecialOrderData> SpecialOrders(LocalizedContentManager content)
	{
		return Load<Dictionary<string, SpecialOrderData>>(content, "Data\\SpecialOrders");
	}

	/// <summary>Load the <c>Data/TailoringRecipes</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	public static List<TailorItemRecipe> TailoringRecipes(LocalizedContentManager content)
	{
		return Load<List<TailorItemRecipe>>(content, "Data\\TailoringRecipes");
	}

	/// <summary>Load the <c>Data/Tools</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	/// <remarks>Most code should use <see cref="F:StardewValley.Game1.toolData" /> instead.</remarks>
	public static Dictionary<string, ToolData> Tools(LocalizedContentManager content)
	{
		return Load<Dictionary<string, ToolData>>(content, "Data\\Tools");
	}

	/// <summary>Load the <c>Data/TriggerActions</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	public static List<TriggerActionData> TriggerActions(LocalizedContentManager content)
	{
		return Load<List<TriggerActionData>>(content, "Data\\TriggerActions");
	}

	/// <summary>Load the <c>Data/Trinkets</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	public static Dictionary<string, TrinketData> Trinkets(LocalizedContentManager content)
	{
		return content.Load<Dictionary<string, TrinketData>>("Data\\Trinkets");
	}

	/// <summary>Load the <c>Data/Weapons</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	/// <remarks>Most code should use <see cref="F:StardewValley.Game1.weaponData" /> instead.</remarks>
	public static Dictionary<string, WeaponData> Weapons(LocalizedContentManager content)
	{
		return Load<Dictionary<string, WeaponData>>(content, "Data\\Weapons");
	}

	/// <summary>Load the <c>Data/Weddings</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	public static WeddingData Weddings(LocalizedContentManager content)
	{
		return Load<WeddingData>(content, "Data\\Weddings");
	}

	/// <summary>Load the <c>Data/WildTrees</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	/// <remarks>Most code should use <see cref="M:StardewValley.TerrainFeatures.Tree.GetWildTreeDataDictionary" /> or <see cref="M:StardewValley.TerrainFeatures.Tree.GetWildTreeSeedLookup" /> instead.</remarks>
	public static Dictionary<string, WildTreeData> WildTrees(LocalizedContentManager content)
	{
		return Load<Dictionary<string, WildTreeData>>(content, "Data\\WildTrees");
	}

	/// <summary>Load the <c>Data/WorldMap</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	/// <remarks>Most code should use <see cref="T:StardewValley.WorldMaps.WorldMapManager" /> instead.</remarks>
	public static Dictionary<string, WorldMapRegionData> WorldMap(LocalizedContentManager content)
	{
		return Load<Dictionary<string, WorldMapRegionData>>(content, "Data\\WorldMap");
	}

	/// <summary>Load the <c>Data/TV/CookingChannel</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	public static Dictionary<string, string> Tv_CookingChannel(LocalizedContentManager content)
	{
		return Load<Dictionary<string, string>>(content, "Data\\TV\\CookingChannel");
	}

	/// <summary>Load the <c>Data/TV/TipChannel</c> asset.</summary>
	/// <param name="content">The content manager through which to load data.</param>
	public static Dictionary<string, string> Tv_TipChannel(LocalizedContentManager content)
	{
		return Load<Dictionary<string, string>>(content, "Data\\TV\\TipChannel");
	}

	/// <summary>Load an asset.</summary>
	/// <typeparam name="TAsset">The asset type.</typeparam>
	/// <param name="content">The content manager through which to load data.</param>
	/// <param name="assetName">The asset name to load.</param>
	/// <exception cref="T:Microsoft.Xna.Framework.Content.ContentLoadException">The asset could not be loaded.</exception>
	private static TAsset Load<TAsset>(LocalizedContentManager content, string assetName)
	{
		try
		{
			return content.Load<TAsset>(assetName);
		}
		catch (Exception ex)
		{
			throw new ContentLoadException("Failed loading asset '" + assetName + "'.", ex);
		}
	}
}
