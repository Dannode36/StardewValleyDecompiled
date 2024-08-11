using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.Crops;
using StardewValley.GameData.Tools;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Quests;
using StardewValley.SpecialOrders;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using StardewValley.Util;
using xTile.Layers;
using xTile.Tiles;

namespace StardewValley.SaveMigrations;

/// <summary>Migrates existing save files for compatibility with Stardew Valley 1.6.</summary>
public class SaveMigrator_1_6 : ISaveMigrator
{
	/// <summary>The pre-1.6 structure of <see cref="T:StardewValley.Quests.DescriptionElement" />.</summary>
	public class LegacyDescriptionElement
	{
		/// <summary>The translation key for the text to render.</summary>
		public string xmlKey;

		/// <summary>The values to substitute for placeholders like <c>{0}</c> in the translation text.</summary>
		public List<object> param;
	}

	/// <inheritdoc />
	public Version GameVersion { get; } = new Version(1, 5);


	/// <inheritdoc />
	public bool ApplySaveFix(SaveFixes saveFix)
	{
		switch (saveFix)
		{
		case SaveFixes.MigrateBuildingsToData:
			Utility.ForEachBuilding(delegate(Building building)
			{
				if (building is JunimoHut { obsolete_output: not null } junimoHut)
				{
					junimoHut.GetOutputChest().Items.AddRange(junimoHut.obsolete_output.Items);
					junimoHut.obsolete_output = null;
				}
				if (building.isUnderConstruction(ignoreUpgrades: false))
				{
					Game1.netWorldState.Value.MarkUnderConstruction("Robin", building);
					if (building.daysUntilUpgrade.Value > 0 && string.IsNullOrWhiteSpace(building.upgradeName.Value))
					{
						building.upgradeName.Value = InferBuildingUpgradingTo(building.buildingType.Value);
					}
				}
				return true;
			});
			return true;
		case SaveFixes.ModularizeFarmhouse:
			Game1.getFarm().AddDefaultBuildings();
			return true;
		case SaveFixes.ModularizePets:
		{
			foreach (Farmer allFarmer in Game1.getAllFarmers())
			{
				allFarmer.whichPetType = ((allFarmer.obsolete_catPerson ?? false) ? "Cat" : "Dog");
				allFarmer.obsolete_catPerson = null;
			}
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				for (int num = location.characters.Count - 1; num >= 0; num--)
				{
					if (location.characters[num] is Pet pet2)
					{
						string text = null;
						if (pet2.GetType() == typeof(Cat))
						{
							text = "Cat";
						}
						else if (pet2.GetType() == typeof(Dog))
						{
							text = "Dog";
						}
						if (text != null)
						{
							Pet pet3 = new Pet((int)(pet2.Position.X / 64f), (int)(pet2.Position.X / 64f), pet2.whichBreed, text)
							{
								Name = pet2.Name,
								displayName = pet2.displayName
							};
							if (pet2.currentLocation != null)
							{
								pet3.currentLocation = pet2.currentLocation;
							}
							pet3.friendshipTowardFarmer.Value = pet2.friendshipTowardFarmer;
							pet3.grantedFriendshipForPet.Value = pet2.grantedFriendshipForPet;
							pet3.lastPetDay.Clear();
							pet3.lastPetDay.CopyFrom(pet2.lastPetDay.Pairs);
							pet3.isSleepingOnFarmerBed.Value = pet2.isSleepingOnFarmerBed.Value;
							pet3.modData.CopyFrom(pet2.modData);
							location.characters[num] = pet3;
						}
					}
				}
				return true;
			});
			Farm farm = Game1.getFarm();
			farm.AddDefaultBuilding("Pet Bowl", farm.GetStarterPetBowlLocation());
			PetBowl bowl = farm.getBuildingByType("Pet Bowl") as PetBowl;
			Pet pet = Game1.player.getPet();
			if (bowl != null && pet != null)
			{
				bowl.AssignPet(pet);
				pet.setAtFarmPosition();
			}
			return true;
		}
		case SaveFixes.AddNpcRemovalFlags:
		{
			GameLocation location = Game1.getLocationFromName("WitchSwamp");
			if (location != null && location.getCharacterFromName("Henchman") == null)
			{
				Game1.addMail("henchmanGone", noLetter: true, sendToEveryone: true);
			}
			location = Game1.getLocationFromName("SandyHouse");
			if (location != null && location.getCharacterFromName("Bouncer") == null)
			{
				Game1.addMail("bouncerGone", noLetter: true, sendToEveryone: true);
			}
			return true;
		}
		case SaveFixes.MigrateFarmhands:
			return true;
		case SaveFixes.MigrateLitterItemData:
			Utility.ForEachItem(delegate(Item item)
			{
				switch (item.QualifiedItemId)
				{
				case "(O)2":
				case "(O)4":
				case "(O)6":
				case "(O)8":
				case "(O)10":
				case "(O)12":
				case "(O)14":
				case "(O)25":
				case "(O)75":
				case "(O)76":
				case "(O)77":
				case "(O)95":
				case "(O)290":
				case "(O)751":
				case "(O)764":
				case "(O)765":
				case "(O)816":
				case "(O)817":
				case "(O)818":
				case "(O)819":
				case "(O)843":
				case "(O)844":
				case "(O)849":
				case "(O)850":
				case "(O)32":
				case "(O)34":
				case "(O)36":
				case "(O)38":
				case "(O)40":
				case "(O)42":
				case "(O)44":
				case "(O)46":
				case "(O)48":
				case "(O)50":
				case "(O)52":
				case "(O)54":
				case "(O)56":
				case "(O)58":
				case "(O)343":
				case "(O)450":
				case "(O)668":
				case "(O)670":
				case "(O)760":
				case "(O)762":
				case "(O)845":
				case "(O)846":
				case "(O)847":
				case "(O)294":
				case "(O)295":
				case "(O)0":
				case "(O)313":
				case "(O)314":
				case "(O)315":
				case "(O)316":
				case "(O)317":
				case "(O)318":
				case "(O)319":
				case "(O)320":
				case "(O)321":
				case "(O)452":
				case "(O)674":
				case "(O)675":
				case "(O)676":
				case "(O)677":
				case "(O)678":
				case "(O)679":
				case "(O)750":
				case "(O)784":
				case "(O)785":
				case "(O)786":
				case "(O)792":
				case "(O)793":
				case "(O)794":
				case "(O)882":
				case "(O)883":
				case "(O)884":
					item.Category = -999;
					if (item is Object object4)
					{
						object4.Type = "Litter";
					}
					break;
				case "(O)372":
					item.Category = -4;
					if (item is Object object3)
					{
						object3.Type = "Fish";
					}
					break;
				}
				return true;
			});
			return true;
		case SaveFixes.MigrateHoneyItems:
			Utility.ForEachItem(delegate(Item item)
			{
				if (!(item is Object object5) || object5.QualifiedItemId != "(O)340")
				{
					return true;
				}
				object5.preserve.Value = Object.PreserveType.Honey;
				if (object5.preservedParentSheetIndex.Value == null || object5.preservedParentSheetIndex.Value == "0")
				{
					string text2 = object5.obsolete_honeyType;
					if (string.IsNullOrWhiteSpace(text2) && object5.name != null && object5.name.EndsWith(" Honey"))
					{
						text2 = object5.name.Substring(0, object5.name.Length - " Honey".Length).Replace(" ", "");
					}
					switch (text2)
					{
					case "Poppy":
						object5.preservedParentSheetIndex.Value = "376";
						break;
					case "Tulip":
						object5.preservedParentSheetIndex.Value = "591";
						break;
					case "SummerSpangle":
						object5.preservedParentSheetIndex.Value = "593";
						break;
					case "FairyRose":
						object5.preservedParentSheetIndex.Value = "595";
						break;
					case "BlueJazz":
						object5.preservedParentSheetIndex.Value = "597";
						break;
					default:
						object5.Name = "Wild Honey";
						object5.preservedParentSheetIndex.Value = null;
						break;
					}
				}
				if (object5.Name == "Honey" && object5.preservedParentSheetIndex.Value == "-1")
				{
					object5.Name = "Wild Honey";
				}
				object5.obsolete_honeyType = null;
				return true;
			});
			return true;
		case SaveFixes.MigrateMachineLastOutputRule:
			Utility.ForEachItem(delegate(Item item)
			{
				if (item is Object machine)
				{
					InferMachineInputOutputFields(machine);
				}
				return true;
			});
			return true;
		case SaveFixes.StandardizeBundleFields:
			return true;
		case SaveFixes.MigrateAdventurerGoalFlags:
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary["Gil_Slime Charmer Ring"] = "Gil_Slimes";
			dictionary["Gil_Slime Charmer Ring"] = "Gil_Slimes";
			dictionary["Gil_Savage Ring"] = "Gil_Shadows";
			dictionary["Gil_Vampire Ring"] = "Gil_Bats";
			dictionary["Gil_Skeleton Mask"] = "Gil_Skeletons";
			dictionary["Gil_Insect Head"] = "Gil_Insects";
			dictionary["Gil_Hard Hat"] = "Gil_Duggy";
			dictionary["Gil_Burglar's Ring"] = "Gil_DustSpirits";
			dictionary["Gil_Crabshell Ring"] = "Gil_Crabs";
			dictionary["Gil_Arcane Hat"] = "Gil_Mummies";
			dictionary["Gil_Knight's Helmet"] = "Gil_Dinos";
			dictionary["Gil_Napalm Ring"] = "Gil_Serpents";
			dictionary["Gil_Telephone"] = "Gil_FlameSpirits";
			Dictionary<string, string> map = dictionary;
			foreach (Farmer player in Game1.getAllFarmers())
			{
				NetStringHashSet[] array4 = new NetStringHashSet[2] { player.mailReceived, player.mailForTomorrow };
				foreach (NetStringHashSet mail in array4)
				{
					foreach (KeyValuePair<string, string> pair in map)
					{
						if (mail.Remove(pair.Key))
						{
							mail.Add(pair.Value);
						}
					}
				}
				IList<string> mailbox = Game1.mailbox;
				for (int i = 0; i < mailbox.Count; i++)
				{
					if (map.TryGetValue(mailbox[i], out var newFlag))
					{
						mailbox[i] = newFlag;
					}
				}
			}
			return true;
		}
		case SaveFixes.SetCropSeedId:
		{
			Dictionary<string, string> seedsByHarvestId = new Dictionary<string, string>();
			foreach (KeyValuePair<string, CropData> pair in Game1.cropData)
			{
				string seedId = pair.Key;
				string harvestId = pair.Value.HarvestItemId;
				if (harvestId != null)
				{
					seedsByHarvestId.TryAdd(harvestId, seedId);
				}
			}
			Utility.ForEachCrop(delegate(Crop crop)
			{
				if (crop.netSeedIndex.Value == "-1")
				{
					crop.netSeedIndex.Value = null;
				}
				if (!string.IsNullOrWhiteSpace(crop.netSeedIndex.Value))
				{
					return true;
				}
				if (crop.isWildSeedCrop() || crop.forageCrop.Value)
				{
					return true;
				}
				if (crop.indexOfHarvest.Value != null && seedsByHarvestId.TryGetValue(crop.indexOfHarvest.Value, out var value4))
				{
					crop.netSeedIndex.Value = value4;
				}
				return true;
			});
			return true;
		}
		case SaveFixes.FixMineBoulderCollisions:
		{
			Mine mine = Game1.RequireLocation<Mine>("Mine");
			Vector2 tile = mine.GetBoulderPosition();
			if (mine.objects.TryGetValue(tile, out var boulder) && boulder.QualifiedItemId == "(BC)78" && boulder.TileLocation == Vector2.Zero)
			{
				boulder.TileLocation = tile;
			}
			return true;
		}
		case SaveFixes.MigratePetAndPetBowlIds:
		{
			Pet pet = Game1.player.getPet();
			if (pet != null)
			{
				pet.petId.Value = Guid.NewGuid();
				PetBowl bowl = (PetBowl)Game1.getFarm().getBuildingByType("Pet Bowl");
				if (bowl != null)
				{
					bowl.AssignPet(pet);
					pet.setAtFarmPosition();
				}
			}
			return true;
		}
		case SaveFixes.MigrateHousePaint:
		{
			Farm farm = Game1.getFarm();
			if (farm.housePaintColor.Value != null)
			{
				farm.GetMainFarmHouse().netBuildingPaintColor.Value.CopyFrom(farm.housePaintColor.Value);
				farm.housePaintColor.Value = null;
			}
			return true;
		}
		case SaveFixes.MigrateItemIds:
			Utility.ForEachItem(delegate(Item item)
			{
				if (!(item is Boots boots))
				{
					if (!(item is MeleeWeapon meleeWeapon))
					{
						if (!(item is Fence fence2))
						{
							if (!(item is Slingshot slingshot))
							{
								if (item is Torch && item.itemId.Value != item.ParentSheetIndex.ToString())
								{
									item.itemId.Value = null;
								}
							}
							else
							{
								slingshot.ItemId = null;
							}
						}
						else if (fence2.obsolete_whichType.HasValue)
						{
							item.itemId.Value = null;
						}
					}
					else
					{
						meleeWeapon.appearance.Value = ((!string.IsNullOrWhiteSpace(meleeWeapon.appearance.Value) && meleeWeapon.appearance.Value != "-1") ? ItemRegistry.ManuallyQualifyItemId(meleeWeapon.appearance.Value, "(W)") : null);
					}
				}
				else if (boots.appliedBootSheetIndex.Value == "-1")
				{
					boots.appliedBootSheetIndex.Value = null;
				}
				_ = item.ItemId;
				return true;
			});
			foreach (Farmer player in Game1.getAllFarmers())
			{
				NetStringIntArrayDictionary fishCaught = player.fishCaught;
				if (fishCaught != null)
				{
					KeyValuePair<string, int[]>[] array = fishCaught.Pairs.ToArray();
					for (int j = 0; j < array.Length; j++)
					{
						KeyValuePair<string, int[]> pair = array[j];
						fishCaught.Remove(pair.Key);
						fishCaught[ItemRegistry.ManuallyQualifyItemId(pair.Key, "(O)")] = pair.Value;
					}
				}
				if (player.toolBeingUpgraded.Value != null)
				{
					switch (player.toolBeingUpgraded.Value.InitialParentTileIndex)
					{
					case 13:
						player.toolBeingUpgraded.Value = ItemRegistry.Create<Tool>("(T)CopperTrashCan");
						break;
					case 14:
						player.toolBeingUpgraded.Value = ItemRegistry.Create<Tool>("(T)SteelTrashCan");
						break;
					case 15:
						player.toolBeingUpgraded.Value = ItemRegistry.Create<Tool>("(T)GoldTrashCan");
						break;
					case 16:
						player.toolBeingUpgraded.Value = ItemRegistry.Create<Tool>("(T)IridiumTrashCan");
						break;
					}
				}
				if (!(player.obsolete_isMale ?? player.IsMale))
				{
					NetRef<Clothing>[] array2 = new NetRef<Clothing>[2] { player.shirtItem, player.pantsItem };
					foreach (NetRef<Clothing> field in array2)
					{
						Clothing clothing = field.Value;
						if (clothing == null)
						{
							continue;
						}
						if (clothing.obsolete_indexInTileSheetFemale > -1)
						{
							int variantId = clothing.obsolete_indexInTileSheetFemale.Value;
							if (clothing.HasTypeId("(S)"))
							{
								variantId += 1000;
							}
							ItemMetadata variantData = ItemRegistry.GetMetadata(clothing.TypeDefinitionId + variantId);
							if (variantData.Exists())
							{
								Clothing newClothing = (Clothing)variantData.CreateItemOrErrorItem();
								newClothing.clothesColor.Value = clothing.clothesColor.Value;
								newClothing.modData.CopyFrom(clothing.modData);
								field.Value = newClothing;
							}
						}
						clothing.obsolete_indexInTileSheetFemale = null;
					}
				}
				foreach (Quest rawQuest in player.questLog)
				{
					if (rawQuest is not CraftingQuest craftQuest)
					{
						if (rawQuest is not FishingQuest fishQuest)
						{
							if (rawQuest is not ItemDeliveryQuest itemDeliverQuest)
							{
								if (rawQuest is not ItemHarvestQuest itemHarvestQuest)
								{
									if (rawQuest is not LostItemQuest lostItemQuest)
									{
										if (rawQuest is not ResourceCollectionQuest resourceCollectQuest)
										{
											if (rawQuest is SecretLostItemQuest secretLostItemQuest)
											{
												secretLostItemQuest.ItemId.Value = ItemRegistry.ManuallyQualifyItemId(secretLostItemQuest.ItemId.Value, "(O)");
											}
										}
										else
										{
											resourceCollectQuest.ItemId.Value = ItemRegistry.ManuallyQualifyItemId(resourceCollectQuest.ItemId.Value, "(O)");
										}
									}
									else
									{
										lostItemQuest.ItemId.Value = ItemRegistry.ManuallyQualifyItemId(lostItemQuest.ItemId.Value, "(O)");
									}
								}
								else
								{
									itemHarvestQuest.ItemId.Value = ItemRegistry.ManuallyQualifyItemId(itemHarvestQuest.ItemId.Value, "(O)");
								}
							}
							else
							{
								itemDeliverQuest.ItemId.Value = ItemRegistry.ManuallyQualifyItemId(itemDeliverQuest.ItemId.Value, "(O)");
								if (itemDeliverQuest.dailyQuest.Value)
								{
									itemDeliverQuest.moneyReward.Value = itemDeliverQuest.GetGoldRewardPerItem(ItemRegistry.Create(itemDeliverQuest.ItemId.Value));
								}
							}
						}
						else
						{
							fishQuest.ItemId.Value = ItemRegistry.ManuallyQualifyItemId(fishQuest.ItemId.Value, "(O)");
						}
					}
					else
					{
						craftQuest.ItemId.Value = ItemRegistry.ManuallyQualifyItemId(craftQuest.ItemId.Value, craftQuest.obsolete_isBigCraftable.GetValueOrDefault() ? "(BC)" : "(O)");
						craftQuest.obsolete_isBigCraftable = null;
					}
				}
			}
			foreach (SpecialOrder order in Game1.player.team.specialOrders)
			{
				if (order.itemToRemoveOnEnd.Value == "-1")
				{
					order.itemToRemoveOnEnd.Value = null;
				}
			}
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				if (location is IslandShrine islandShrine)
				{
					islandShrine.AddMissingPedestals();
				}
				foreach (KeyValuePair<Vector2, Object> pair2 in location.objects.Pairs)
				{
					if (pair2.Value is Fence fence && fence.obsolete_whichType.HasValue)
					{
						fence.ItemId = null;
					}
				}
				foreach (TerrainFeature value5 in location.terrainFeatures.Values)
				{
					if (value5 is FruitTree fruitTree)
					{
						if (fruitTree.obsolete_treeType != null)
						{
							switch (fruitTree.obsolete_treeType)
							{
							case "0":
								fruitTree.treeId.Value = "628";
								break;
							case "1":
								fruitTree.treeId.Value = "629";
								break;
							case "2":
								fruitTree.treeId.Value = "630";
								break;
							case "3":
								fruitTree.treeId.Value = "631";
								break;
							case "4":
								fruitTree.treeId.Value = "632";
								break;
							case "5":
								fruitTree.treeId.Value = "633";
								break;
							case "7":
								fruitTree.treeId.Value = "69";
								break;
							case "8":
								fruitTree.treeId.Value = "835";
								break;
							default:
								fruitTree.treeId.Value = fruitTree.obsolete_treeType;
								break;
							}
							fruitTree.obsolete_treeType = null;
						}
						if (fruitTree.obsolete_fruitsOnTree.HasValue)
						{
							bool isGreenhouse = fruitTree.Location.IsGreenhouse;
							try
							{
								fruitTree.Location.IsGreenhouse = true;
								for (int k = 0; k < fruitTree.obsolete_fruitsOnTree; k++)
								{
									fruitTree.TryAddFruit();
								}
							}
							finally
							{
								fruitTree.Location.IsGreenhouse = isGreenhouse;
							}
							fruitTree.obsolete_fruitsOnTree = null;
						}
					}
				}
				foreach (Building building in location.buildings)
				{
					if (building is FishPond fishPond && fishPond.fishType.Value == "-1")
					{
						fishPond.fishType.Value = null;
					}
				}
				foreach (FarmAnimal current5 in location.animals.Values)
				{
					if (current5.currentProduce.Value == "-1")
					{
						current5.currentProduce.Value = null;
						current5.ReloadTextureIfNeeded();
					}
				}
				return true;
			});
			return true;
		case SaveFixes.MigrateShedFloorWallIds:
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				if (location is Shed shed)
				{
					if (shed.appliedFloor.TryGetValue("Floor_0", out var value2))
					{
						shed.appliedFloor.Remove("Floor_0");
						shed.appliedFloor["Floor"] = value2;
					}
					if (shed.appliedWallpaper.TryGetValue("Wall_0", out var value3))
					{
						shed.appliedWallpaper.Remove("Wall_0");
						shed.appliedWallpaper["Wall"] = value3;
					}
				}
				return true;
			});
			return true;
		case SaveFixes.RemoveMeatFromAnimalBundle:
		{
			if (Game1.netWorldState.Value.BundleData.TryGetValue("Pantry/4", out var rawData) && rawData.StartsWith("Animal/"))
			{
				string[] fields = rawData.Split('/');
				List<string> ingredients = ArgUtility.SplitBySpace(ArgUtility.Get(rawData.Split('/'), 2)).ToList();
				for (int i = 0; i < ingredients.Count; i += 3)
				{
					string id = ingredients[i];
					switch (id)
					{
					case "639":
					case "640":
					case "641":
					case "642":
					case "643":
						if (ItemRegistry.ResolveMetadata("(O)" + id) == null)
						{
							ingredients.RemoveRange(i, Math.Min(3, ingredients.Count - 1));
							i -= 3;
						}
						break;
					}
				}
				fields[2] = string.Join(" ", ingredients);
				Game1.netWorldState.Value.BundleData["Pantry/4"] = string.Join("/", fields);
				if (Game1.netWorldState.Value.Bundles.TryGetValue(4, out var values) && values.Length > ingredients.Count)
				{
					Array.Resize(ref values, ingredients.Count);
					Game1.netWorldState.Value.Bundles.Remove(4);
					Game1.netWorldState.Value.Bundles.Add(4, values);
				}
			}
			return true;
		}
		case SaveFixes.MigrateStatFields:
		{
			Stats stats = Game1.stats;
			SerializableDictionary<string, uint> obsolete_stat_dictionary = stats.obsolete_stat_dictionary;
			if (obsolete_stat_dictionary != null && obsolete_stat_dictionary.Count > 0)
			{
				foreach (KeyValuePair<string, uint> pair in stats.obsolete_stat_dictionary)
				{
					stats.Values[pair.Key] = (stats.Values.TryGetValue(pair.Key, out var prevValue) ? (prevValue + pair.Value) : pair.Value);
				}
				stats.obsolete_stat_dictionary = null;
			}
			if (stats.Values.TryGetValue("walnutsFound", out var walnutsFound))
			{
				Game1.netWorldState.Value.GoldenWalnutsFound += (int)walnutsFound;
				stats.Values.Remove("walnutsFound");
			}
			KeyValuePair<string, uint>[] array3 = stats.Values.ToArray();
			for (int j = 0; j < array3.Length; j++)
			{
				KeyValuePair<string, uint> pair = array3[j];
				if (pair.Value == 0)
				{
					stats.Values.Remove(pair.Key);
				}
			}
			stats.Set("averageBedtime", stats.obsolete_averageBedtime.GetValueOrDefault());
			stats.obsolete_averageBedtime = null;
			stats.Set("beveragesMade", stats.obsolete_beveragesMade.GetValueOrDefault());
			stats.obsolete_beveragesMade = null;
			stats.Set("caveCarrotsFound", stats.obsolete_caveCarrotsFound.GetValueOrDefault());
			stats.obsolete_caveCarrotsFound = null;
			stats.Set("cheeseMade", stats.obsolete_cheeseMade.GetValueOrDefault());
			stats.obsolete_cheeseMade = null;
			stats.Set("chickenEggsLayed", stats.obsolete_chickenEggsLayed.GetValueOrDefault());
			stats.obsolete_chickenEggsLayed = null;
			stats.Set("copperFound", stats.obsolete_copperFound.GetValueOrDefault());
			stats.obsolete_copperFound = null;
			stats.Set("cowMilkProduced", stats.obsolete_cowMilkProduced.GetValueOrDefault());
			stats.obsolete_cowMilkProduced = null;
			stats.Set("cropsShipped", stats.obsolete_cropsShipped.GetValueOrDefault());
			stats.obsolete_cropsShipped = null;
			stats.Set("daysPlayed", stats.obsolete_daysPlayed.GetValueOrDefault());
			stats.obsolete_daysPlayed = null;
			stats.Set("diamondsFound", stats.obsolete_diamondsFound.GetValueOrDefault());
			stats.obsolete_diamondsFound = null;
			stats.Set("dirtHoed", stats.obsolete_dirtHoed.GetValueOrDefault());
			stats.obsolete_dirtHoed = null;
			stats.Set("duckEggsLayed", stats.obsolete_duckEggsLayed.GetValueOrDefault());
			stats.obsolete_duckEggsLayed = null;
			stats.Set("fishCaught", stats.obsolete_fishCaught.GetValueOrDefault());
			stats.obsolete_fishCaught = null;
			stats.Set("geodesCracked", stats.obsolete_geodesCracked.GetValueOrDefault());
			stats.obsolete_geodesCracked = null;
			stats.Set("giftsGiven", stats.obsolete_giftsGiven.GetValueOrDefault());
			stats.obsolete_giftsGiven = null;
			stats.Set("goatCheeseMade", stats.obsolete_goatCheeseMade.GetValueOrDefault());
			stats.obsolete_goatCheeseMade = null;
			stats.Set("goatMilkProduced", stats.obsolete_goatMilkProduced.GetValueOrDefault());
			stats.obsolete_goatMilkProduced = null;
			stats.Set("goldFound", stats.obsolete_goldFound.GetValueOrDefault());
			stats.obsolete_goldFound = null;
			stats.Set("goodFriends", stats.obsolete_goodFriends.GetValueOrDefault());
			stats.obsolete_goodFriends = null;
			stats.Set("individualMoneyEarned", stats.obsolete_individualMoneyEarned.GetValueOrDefault());
			stats.obsolete_individualMoneyEarned = null;
			stats.Set("iridiumFound", stats.obsolete_iridiumFound.GetValueOrDefault());
			stats.obsolete_iridiumFound = null;
			stats.Set("ironFound", stats.obsolete_ironFound.GetValueOrDefault());
			stats.obsolete_ironFound = null;
			stats.Set("itemsCooked", stats.obsolete_itemsCooked.GetValueOrDefault());
			stats.obsolete_itemsCooked = null;
			stats.Set("itemsCrafted", stats.obsolete_itemsCrafted.GetValueOrDefault());
			stats.obsolete_itemsCrafted = null;
			stats.Set("itemsForaged", stats.obsolete_itemsForaged.GetValueOrDefault());
			stats.obsolete_itemsForaged = null;
			stats.Set("itemsShipped", stats.obsolete_itemsShipped.GetValueOrDefault());
			stats.obsolete_itemsShipped = null;
			stats.Set("monstersKilled", stats.obsolete_monstersKilled.GetValueOrDefault());
			stats.obsolete_monstersKilled = null;
			stats.Set("mysticStonesCrushed", stats.obsolete_mysticStonesCrushed.GetValueOrDefault());
			stats.obsolete_mysticStonesCrushed = null;
			stats.Set("notesFound", stats.obsolete_notesFound.GetValueOrDefault());
			stats.obsolete_notesFound = null;
			stats.Set("otherPreciousGemsFound", stats.obsolete_otherPreciousGemsFound.GetValueOrDefault());
			stats.obsolete_otherPreciousGemsFound = null;
			stats.Set("piecesOfTrashRecycled", stats.obsolete_piecesOfTrashRecycled.GetValueOrDefault());
			stats.obsolete_piecesOfTrashRecycled = null;
			stats.Set("preservesMade", stats.obsolete_preservesMade.GetValueOrDefault());
			stats.obsolete_preservesMade = null;
			stats.Set("prismaticShardsFound", stats.obsolete_prismaticShardsFound.GetValueOrDefault());
			stats.obsolete_prismaticShardsFound = null;
			stats.Set("questsCompleted", stats.obsolete_questsCompleted.GetValueOrDefault());
			stats.obsolete_questsCompleted = null;
			stats.Set("rabbitWoolProduced", stats.obsolete_rabbitWoolProduced.GetValueOrDefault());
			stats.obsolete_rabbitWoolProduced = null;
			stats.Set("rocksCrushed", stats.obsolete_rocksCrushed.GetValueOrDefault());
			stats.obsolete_rocksCrushed = null;
			stats.Set("sheepWoolProduced", stats.obsolete_sheepWoolProduced.GetValueOrDefault());
			stats.obsolete_sheepWoolProduced = null;
			stats.Set("slimesKilled", stats.obsolete_slimesKilled.GetValueOrDefault());
			stats.obsolete_slimesKilled = null;
			stats.Set("stepsTaken", stats.obsolete_stepsTaken.GetValueOrDefault());
			stats.obsolete_stepsTaken = null;
			stats.Set("stoneGathered", stats.obsolete_stoneGathered.GetValueOrDefault());
			stats.obsolete_stoneGathered = null;
			stats.Set("stumpsChopped", stats.obsolete_stumpsChopped.GetValueOrDefault());
			stats.obsolete_stumpsChopped = null;
			stats.Set("timesFished", stats.obsolete_timesFished.GetValueOrDefault());
			stats.obsolete_timesFished = null;
			stats.Set("timesUnconscious", stats.obsolete_timesUnconscious.GetValueOrDefault());
			stats.obsolete_timesUnconscious = null;
			stats.Set("totalMoneyGifted", stats.obsolete_totalMoneyGifted.GetValueOrDefault());
			stats.obsolete_totalMoneyGifted = null;
			stats.Set("trufflesFound", stats.obsolete_trufflesFound.GetValueOrDefault());
			stats.obsolete_trufflesFound = null;
			stats.Set("weedsEliminated", stats.obsolete_weedsEliminated.GetValueOrDefault());
			stats.obsolete_weedsEliminated = null;
			stats.Set("seedsSown", stats.obsolete_seedsSown.GetValueOrDefault());
			stats.obsolete_seedsSown = null;
			return true;
		}
		case SaveFixes.RemoveMasteryRoomFoliage:
		{
			GameLocation forest = Game1.getLocationFromName("Forest");
			if (forest != null)
			{
				for (int i = forest.largeTerrainFeatures.Count - 1; i >= 0; i--)
				{
					if (forest.largeTerrainFeatures[i].Tile == new Vector2(100f, 74f) || forest.largeTerrainFeatures[i].Tile == new Vector2(101f, 76f))
					{
						forest.largeTerrainFeatures.RemoveAt(i);
					}
				}
				if (forest.terrainFeatures.ContainsKey(new Vector2(98f, 75f)) && forest.terrainFeatures[new Vector2(98f, 75f)] is Tree t && (bool)t.tapped && forest.objects.ContainsKey(new Vector2(98f, 75f)))
				{
					Object o = forest.objects[new Vector2(98f, 75f)];
					if ((bool)o.readyForHarvest && o.heldObject != null)
					{
						Game1.player.team.returnedDonations.Add(o.heldObject.Value);
					}
					Game1.player.team.returnedDonations.Add(o);
					Game1.player.team.newLostAndFoundItems.Value = true;
				}
				forest.terrainFeatures.Remove(new Vector2(98f, 75f));
			}
			return true;
		}
		case SaveFixes.AddTownTrees:
		{
			GameLocation town = Game1.getLocationFromName("Town");
			Layer pathsLayer = town.map?.GetLayer("Paths");
			if (pathsLayer == null)
			{
				return false;
			}
			for (int x = 0; x < town.map.Layers[0].LayerWidth; x++)
			{
				for (int y = 0; y < town.map.Layers[0].LayerHeight; y++)
				{
					Tile t = pathsLayer.Tiles[x, y];
					if (t == null)
					{
						continue;
					}
					Vector2 tile = new Vector2(x, y);
					if (town.TryGetTreeIdForTile(t, out var treeId, out var growthStageOnLoad, out var _, out var isFruitTree) && town.GetFurnitureAt(tile) == null && !town.terrainFeatures.ContainsKey(tile) && !town.objects.ContainsKey(tile))
					{
						if (isFruitTree)
						{
							town.terrainFeatures.Add(tile, new FruitTree(treeId, growthStageOnLoad ?? 4));
						}
						else
						{
							town.terrainFeatures.Add(tile, new Tree(treeId, growthStageOnLoad ?? 5));
						}
					}
				}
			}
			return true;
		}
		case SaveFixes.MapAdjustments_1_6:
		{
			Game1.getLocationFromName("BusStop").shiftContents(10, 0);
			List<Point> obj = new List<Point>
			{
				new Point(78, 17),
				new Point(79, 17),
				new Point(79, 18),
				new Point(80, 17),
				new Point(80, 18),
				new Point(80, 19),
				new Point(81, 16),
				new Point(81, 17),
				new Point(81, 18),
				new Point(81, 19),
				new Point(82, 15),
				new Point(82, 16),
				new Point(82, 17),
				new Point(82, 18),
				new Point(83, 13),
				new Point(83, 14),
				new Point(83, 15),
				new Point(83, 16),
				new Point(83, 17),
				new Point(84, 13),
				new Point(84, 14),
				new Point(84, 15),
				new Point(84, 16),
				new Point(84, 17),
				new Point(84, 18),
				new Point(85, 13),
				new Point(85, 14),
				new Point(85, 15),
				new Point(85, 16),
				new Point(85, 17),
				new Point(85, 18),
				new Point(86, 14),
				new Point(86, 15),
				new Point(86, 16),
				new Point(86, 17),
				new Point(86, 18),
				new Point(87, 14),
				new Point(87, 15),
				new Point(87, 16),
				new Point(87, 17),
				new Point(87, 18),
				new Point(87, 19),
				new Point(88, 13),
				new Point(88, 14),
				new Point(88, 15),
				new Point(88, 16),
				new Point(88, 17),
				new Point(88, 18),
				new Point(88, 19),
				new Point(89, 13),
				new Point(89, 14),
				new Point(89, 15),
				new Point(89, 16),
				new Point(89, 17),
				new Point(79, 21),
				new Point(79, 22),
				new Point(79, 23),
				new Point(79, 24),
				new Point(79, 25),
				new Point(76, 16),
				new Point(75, 16),
				new Point(74, 16)
			};
			GameLocation mountain2 = Game1.getLocationFromName("Mountain");
			foreach (Point p in obj)
			{
				mountain2.cleanUpTileForMapOverride(p);
			}
			mountain2.terrainFeatures.Remove(new Vector2(79f, 20f));
			mountain2.terrainFeatures.Remove(new Vector2(79f, 19f));
			mountain2.terrainFeatures.Remove(new Vector2(79f, 16f));
			mountain2.terrainFeatures.Remove(new Vector2(80f, 20f));
			mountain2.largeTerrainFeatures.Remove(mountain2.getLargeTerrainFeatureAt(82, 11));
			mountain2.largeTerrainFeatures.Remove(mountain2.getLargeTerrainFeatureAt(86, 13));
			mountain2.largeTerrainFeatures.Remove(mountain2.getLargeTerrainFeatureAt(85, 16));
			mountain2.largeTerrainFeatures.Add(new Bush(new Vector2(81f, 9f), 1, mountain2));
			mountain2.largeTerrainFeatures.Add(new Bush(new Vector2(84f, 18f), 2, mountain2));
			mountain2.largeTerrainFeatures.Add(new Bush(new Vector2(87f, 19f), 1, mountain2));
			List<Point> obj2 = new List<Point>
			{
				new Point(92, 10),
				new Point(93, 10),
				new Point(94, 10),
				new Point(93, 13),
				new Point(95, 13),
				new Point(92, 5),
				new Point(92, 6),
				new Point(97, 9),
				new Point(91, 10),
				new Point(91, 9),
				new Point(91, 8),
				new Point(93, 11),
				new Point(94, 11),
				new Point(95, 11)
			};
			GameLocation town2 = Game1.getLocationFromName("Town");
			foreach (Point p in obj2)
			{
				town2.cleanUpTileForMapOverride(p);
			}
			town2.loadPathsLayerObjectsInArea(103, 16, 16, 27);
			town2.loadPathsLayerObjectsInArea(120, 57, 7, 12);
			town2.largeTerrainFeatures.Remove(town2.getLargeTerrainFeatureAt(105, 42));
			town2.largeTerrainFeatures.Remove(town2.getLargeTerrainFeatureAt(108, 42));
			List<Point> obj3 = new List<Point>
			{
				new Point(63, 77),
				new Point(63, 78),
				new Point(63, 79),
				new Point(63, 80),
				new Point(46, 26),
				new Point(46, 27),
				new Point(46, 28),
				new Point(46, 29)
			};
			GameLocation forest2 = Game1.getLocationFromName("Forest");
			foreach (Point p in obj3)
			{
				forest2.cleanUpTileForMapOverride(p);
			}
			forest2.largeTerrainFeatures.Add(new Bush(new Vector2(54f, 8f), 0, forest2));
			forest2.largeTerrainFeatures.Add(new Bush(new Vector2(58f, 8f), 0, forest2));
			return true;
		}
		case SaveFixes.MigrateWalletItems:
		{
			Farmer player = Game1.MasterPlayer;
			player.hasRustyKey = player.hasRustyKey || (player.obsolete_hasRustyKey ?? false);
			player.hasSkullKey = player.hasSkullKey || (player.obsolete_hasSkullKey ?? false);
			player.canUnderstandDwarves = player.canUnderstandDwarves || (player.obsolete_hasSkullKey ?? false);
			player.obsolete_hasRustyKey = null;
			player.obsolete_hasSkullKey = null;
			player.obsolete_canUnderstandDwarves = null;
			foreach (Farmer otherPlayer in Game1.getAllFarmers())
			{
				otherPlayer.hasClubCard = otherPlayer.hasClubCard || (otherPlayer.obsolete_hasClubCard ?? false);
				otherPlayer.hasDarkTalisman = otherPlayer.hasDarkTalisman || (otherPlayer.obsolete_hasDarkTalisman ?? false);
				otherPlayer.hasMagicInk = otherPlayer.hasMagicInk || (otherPlayer.obsolete_hasMagicInk ?? false);
				otherPlayer.hasMagnifyingGlass = otherPlayer.hasMagnifyingGlass || (otherPlayer.obsolete_hasMagnifyingGlass ?? false);
				otherPlayer.hasSpecialCharm = otherPlayer.hasSpecialCharm || (otherPlayer.obsolete_hasSpecialCharm ?? false);
				otherPlayer.HasTownKey = otherPlayer.HasTownKey || (otherPlayer.obsolete_hasTownKey ?? false);
				otherPlayer.hasUnlockedSkullDoor = otherPlayer.hasUnlockedSkullDoor || (otherPlayer.obsolete_hasUnlockedSkullDoor ?? false);
				otherPlayer.obsolete_hasClubCard = null;
				otherPlayer.obsolete_hasDarkTalisman = null;
				otherPlayer.obsolete_hasMagicInk = null;
				otherPlayer.obsolete_hasMagnifyingGlass = null;
				otherPlayer.obsolete_hasSpecialCharm = null;
				otherPlayer.obsolete_hasTownKey = null;
				otherPlayer.obsolete_hasUnlockedSkullDoor = null;
                otherPlayer.obsolete_daysMarried = null;
			}
			return true;
		}
		case SaveFixes.MigrateResourceClumps:
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				if (!(location is Forest forest3))
				{
					if (location is Woods woods)
					{
						woods.DayUpdate(Game1.dayOfMonth);
					}
				}
				else if (forest3.obsolete_log != null)
				{
					forest3.resourceClumps.Add(forest3.obsolete_log);
					forest3.obsolete_log = null;
				}
				return true;
			}, includeInteriors: false);
			return true;
		case SaveFixes.MigrateFishingRodAttachmentSlots:
			Utility.ForEachItem(delegate(Item item)
			{
				if (item is FishingRod fishingRod)
				{
					ToolData toolData = fishingRod.GetToolData();
					if (toolData == null || toolData.AttachmentSlots < 0 || fishingRod.AttachmentSlotsCount <= toolData.AttachmentSlots)
					{
						return true;
					}
					INetSerializable parent = fishingRod.attachments.Parent;
					fishingRod.attachments.Parent = null;
					try
					{
						int num2 = fishingRod.AttachmentSlotsCount - 1;
						while (fishingRod.AttachmentSlotsCount > toolData.AttachmentSlots && num2 >= 0)
						{
							if (fishingRod.attachments.Count <= num2)
							{
								fishingRod.AttachmentSlotsCount--;
							}
							else if (fishingRod.attachments[num2] == null)
							{
								fishingRod.AttachmentSlotsCount--;
							}
							num2--;
						}
					}
					finally
					{
						fishingRod.attachments.Parent = parent;
					}
				}
				return true;
			});
			return true;
		case SaveFixes.MoveSlimeHutches:
		{
			Farm farm = Game1.getFarm();
			for (int i = farm.buildings.Count - 1; i >= 0; i--)
			{
				if (farm.buildings[i].buildingType == "Slime Hutch")
				{
					farm.buildings[i].tileX.Value += 2;
					farm.buildings[i].tileY.Value += 2;
					farm.buildings[i].ReloadBuildingData();
					farm.buildings[i].updateInteriorWarps();
				}
			}
			return true;
		}
		case SaveFixes.AddLocationsVisited:
			foreach (Farmer who in Game1.getAllFarmers())
			{
				NetStringHashSet visited = who.locationsVisited;
				Farmer mainPlayer = Game1.MasterPlayer;
				visited.AddRange(new string[30]
				{
					"Farm", "FarmHouse", "FarmCave", "Cellar", "Town", "JoshHouse", "HaleyHouse", "SamHouse", "Blacksmith", "ManorHouse",
					"SeedShop", "Saloon", "Trailer", "Hospital", "HarveyRoom", "ArchaeologyHouse", "JojaMart", "Beach", "ElliottHouse", "FishShop",
					"Mountain", "ScienceHouse", "SebastianRoom", "Tent", "Forest", "AnimalShop", "LeahHouse", "Backwoods", "BusStop", "Tunnel"
				});
				if (mainPlayer.mailReceived.Contains("ccPantry"))
				{
					visited.Add("Greenhouse");
				}
				if (Game1.isLocationAccessible("CommunityCenter"))
				{
					visited.Add("CommunityCenter");
				}
				if (who.eventsSeen.Contains("100162"))
				{
					visited.Add("Mine");
				}
				if (mainPlayer.mailReceived.Contains("ccVault"))
				{
					visited.AddRange(new string[2] { "Desert", "SkullCave" });
				}
				if (who.eventsSeen.Contains("67"))
				{
					visited.Add("SandyHouse");
				}
				if (mainPlayer.mailReceived.Contains("bouncerGone"))
				{
					visited.Add("Club");
				}
				if (Game1.isLocationAccessible("Railroad"))
				{
					visited.AddRange(new string[4]
					{
						"Railroad",
						"BathHouse_Entry",
						who.IsMale ? "BathHouse_MensLocker" : "BathHouse_WomensLocker",
						"BathHouse_Pool"
					});
				}
				if (mainPlayer.mailReceived.Contains("Farm_Eternal"))
				{
					visited.Add("Summit");
				}
				if (mainPlayer.mailReceived.Contains("witchStatueGone"))
				{
					visited.AddRange(new string[2] { "WitchSwamp", "WitchWarpCave" });
				}
				if (mainPlayer.mailReceived.Contains("henchmanGone"))
				{
					visited.Add("WitchHut");
				}
				if (who.mailReceived.Contains("beenToWoods"))
				{
					visited.Add("Woods");
				}
				if (Forest.isWizardHouseUnlocked())
				{
					visited.Add("WizardHouse");
					if (who.getFriendshipHeartLevelForNPC("Wizard") >= 4)
					{
						visited.Add("WizardHouseBasement");
					}
				}
				if (who.mailReceived.Add("guildMember"))
				{
					visited.Add("AdventureGuild");
				}
				if (who.mailReceived.Contains("OpenedSewer"))
				{
					visited.Add("Sewer");
				}
				if (who.mailReceived.Contains("krobusUnseal"))
				{
					visited.Add("BugLand");
				}
				if (mainPlayer.mailReceived.Contains("abandonedJojaMartAccessible"))
				{
					visited.Add("AbandonedJojaMart");
				}
				if (mainPlayer.mailReceived.Contains("ccMovieTheater"))
				{
					visited.Add("MovieTheater");
				}
				if (mainPlayer.mailReceived.Contains("pamHouseUpgrade"))
				{
					visited.Add("Trailer_Big");
				}
				if (who.getFriendshipHeartLevelForNPC("Caroline") >= 2)
				{
					visited.Add("Sunroom");
				}
				if (Game1.year > 1 || (Game1.season == Season.Winter && Game1.dayOfMonth >= 15))
				{
					visited.AddRange(new string[3] { "BeachNightMarket", "MermaidHouse", "Submarine" });
				}
				if (who.mailReceived.Contains("willyBackRoomInvitation"))
				{
					visited.Add("BoatTunnel");
				}
				if (who.mailReceived.Contains("Visited_Island"))
				{
					visited.AddRange(new string[4] { "IslandSouth", "IslandEast", "IslandHut", "IslandShrine" });
					if (mainPlayer.mailReceived.Contains("Island_FirstParrot"))
					{
						visited.AddRange(new string[2] { "IslandNorth", "IslandFieldOffice" });
					}
					if (mainPlayer.mailReceived.Contains("islandNorthCaveOpened"))
					{
						visited.Add("IslandNorthCave1");
					}
					if (mainPlayer.mailReceived.Contains("reachedCaldera"))
					{
						visited.Add("Caldera");
					}
					if (mainPlayer.mailReceived.Contains("Island_Turtle"))
					{
						visited.AddRange(new string[2] { "IslandWest", "IslandWestCave1" });
					}
					if (mainPlayer.mailReceived.Contains("Island_UpgradeHouse"))
					{
						visited.AddRange(new string[2] { "IslandFarmHouse", "IslandFarmCave" });
					}
					if (mainPlayer.team.collectedNutTracker.Contains("Bush_CaptainRoom_2_4"))
					{
						visited.Add("CaptainRoom");
					}
					if (IslandWest.IsQiWalnutRoomDoorUnlocked(out var _))
					{
						visited.Add("QiNutRoom");
					}
					if (mainPlayer.mailReceived.Contains("Island_Resort"))
					{
						visited.AddRange(new string[2] { "IslandSouthEast", "IslandSouthEastCave" });
					}
				}
				if (mainPlayer.mailReceived.Contains("leoMoved"))
				{
					visited.Add("LeoTreeHouse");
				}
			}
			return true;
		case SaveFixes.MarkStarterGiftBoxes:
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				if (location is FarmHouse)
				{
					foreach (Object value6 in location.objects.Values)
					{
						if (value6 is Chest chest && chest.giftbox.Value && !chest.playerChest.Value)
						{
							chest.giftboxIsStarterGift.Value = true;
						}
					}
				}
				return true;
			});
			return true;
		case SaveFixes.MigrateMailEventsToTriggerActions:
		{
			Dictionary<string, string> migrateFromEvents = new Dictionary<string, string>
			{
				["2346097"] = "Mail_Abigail_8heart",
				["2346096"] = "Mail_Penny_10heart",
				["2346095"] = "Mail_Elliott_8heart",
				["2346094"] = "Mail_Elliott_10heart",
				["3333094"] = "Mail_Pierre_ExtendedHours",
				["2346093"] = "Mail_Harvey_10heart",
				["2346092"] = "Mail_Sam_10heart",
				["2346091"] = "Mail_Alex_10heart",
				["68"] = "Mail_Mom_5K",
				["69"] = "Mail_Mom_15K",
				["70"] = "Mail_Mom_32K",
				["71"] = "Mail_Mom_120K",
				["72"] = "Mail_Dad_5K",
				["73"] = "Mail_Dad_15K",
				["74"] = "Mail_Dad_32K",
				["75"] = "Mail_Dad_120K",
				["76"] = "Mail_Tribune_UpAndComing",
				["706"] = "Mail_Pierre_Fertilizers",
				["707"] = "Mail_Pierre_FertilizersHighQuality",
				["909"] = "Mail_Robin_Woodchipper",
				["3872126"] = "Mail_Willy_BackRoomUnlocked"
			};
			Dictionary<string, string> duplicateFromEvents = new Dictionary<string, string>
			{
				["2111194"] = "Mail_Emily_8heart",
				["2111294"] = "Mail_Emily_10heart",
				["3912126"] = "Mail_Elliott_Tour1",
				["3912127"] = "Mail_Elliott_Tour2",
				["3912128"] = "Mail_Elliott_Tour3",
				["3912129"] = "Mail_Elliott_Tour4",
				["3912130"] = "Mail_Elliott_Tour5",
				["3912131"] = "Mail_Elliott_Tour6"
			};
			foreach (Farmer allFarmer2 in Game1.getAllFarmers())
			{
				NetStringHashSet events = allFarmer2.eventsSeen;
				NetStringHashSet actions = allFarmer2.triggerActionsRun;
				foreach (KeyValuePair<string, string> pair in migrateFromEvents)
				{
					if (events.Remove(pair.Key))
					{
						actions.Add(pair.Value);
					}
				}
				foreach (KeyValuePair<string, string> pair in duplicateFromEvents)
				{
					if (events.Contains(pair.Key))
					{
						actions.Add(pair.Value);
					}
				}
			}
			return true;
		}
		case SaveFixes.ShiftFarmHouseFurnitureForExpansion:
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				FarmHouse house = location as FarmHouse;
				if (house != null && house.upgradeLevel >= 2)
				{
					house.shiftContents(15, 10, delegate(Vector2 tile, object entity)
					{
						if (entity is BedFurniture)
						{
							int xTile = (int)tile.X;
							int yTile = (int)tile.Y;
							if (house.doesTileHaveProperty(xTile, yTile, "DefaultBedPosition", "Back") == null)
							{
								return house.doesTileHaveProperty(xTile, yTile, "DefaultChildBedPosition", "Back") == null;
							}
							return false;
						}
						if (entity is Furniture { QualifiedItemId: "(F)1792" })
						{
							Vector2 vector2 = tile - Utility.PointToVector2(house.getFireplacePoint());
							if (!(Math.Abs(vector2.X) > 1E-05f))
							{
								return Math.Abs(vector2.Y) > 1E-05f;
							}
							return true;
						}
						return true;
					});
					foreach (NPC current3 in house.characters)
					{
						if (!current3.TilePoint.Equals(house.getKitchenStandingSpot()))
						{
							current3.Position += new Vector2(15f, 10f) * 64f;
						}
						if (house.getTileIndexAt(current3.TilePoint, "Buildings") != -1 || house.getTileIndexAt(current3.TilePoint, "Back") == -1)
						{
							Vector2 vector = Utility.recursiveFindOpenTileForCharacter(current3, house, Utility.PointToVector2(house.getKitchenStandingSpot()), 99, allowOffMap: false);
							if (vector != Vector2.Zero)
							{
								current3.setTileLocation(vector);
							}
							else
							{
								current3.setTileLocation(Utility.PointToVector2(house.getKitchenStandingSpot()));
							}
						}
					}
				}
				return true;
			});
			foreach (Farmer f in Game1.getAllFarmers())
			{
				if (f.currentLocation is FarmHouse { upgradeLevel: >=2 })
				{
					f.Position += new Vector2(15f, 10f) * 64f;
				}
			}
			return true;
		case SaveFixes.MigratePreservesTo16:
		{
			ObjectDataDefinition objTypeDefinition = ItemRegistry.GetObjectTypeDefinition();
			Utility.ForEachItem(delegate(Item item, Action remove, Action<Item> replaceWith)
			{
				if (!(item is Object @object))
				{
					return true;
				}
				string value = @object.preservedParentSheetIndex.Value;
				if (value == null || value == "0")
				{
					@object.preservedParentSheetIndex.Value = null;
					return true;
				}
				if (!@object.isRecipe && !(@object is ColoredObject))
				{
					Object object2 = null;
					switch (item.QualifiedItemId)
					{
					case "(O)344":
					{
						Object ingredient4 = ItemRegistry.Create<Object>("(O)" + value);
						object2 = objTypeDefinition.CreateFlavoredJelly(ingredient4);
						break;
					}
					case "(O)350":
					{
						Object ingredient3 = ItemRegistry.Create<Object>("(O)" + value);
						object2 = objTypeDefinition.CreateFlavoredJuice(ingredient3);
						break;
					}
					case "(O)342":
					{
						Object ingredient2 = ItemRegistry.Create<Object>("(O)" + value);
						object2 = objTypeDefinition.CreateFlavoredPickle(ingredient2);
						break;
					}
					case "(O)348":
					{
						Object ingredient = ItemRegistry.Create<Object>("(O)" + value);
						object2 = objTypeDefinition.CreateFlavoredWine(ingredient);
						break;
					}
					}
					if (object2 != null)
					{
						object2.Name = @object.Name;
						object2.Price = @object.Price;
						object2.Stack = @object.Stack;
						object2.Quality = @object.Quality;
						object2.CanBeGrabbed = @object.CanBeGrabbed;
						object2.CanBeSetDown = @object.CanBeSetDown;
						object2.Edibility = @object.Edibility;
						object2.Fragility = @object.Fragility;
						object2.HasBeenInInventory = @object.HasBeenInInventory;
						object2.questId.Value = @object.questId.Value;
						object2.questItem.Value = @object.questItem.Value;
						foreach (KeyValuePair<string, string> current4 in @object.modData.Pairs)
						{
							object2.modData[current4.Key] = current4.Value;
						}
						replaceWith(object2);
					}
				}
				return true;
			});
			return true;
		}
		case SaveFixes.MigrateQuestDataTo16:
		{
			Lazy<XmlSerializer> serializer = new Lazy<XmlSerializer>(() => new XmlSerializer(typeof(LegacyDescriptionElement), new Type[3]
			{
				typeof(DescriptionElement),
				typeof(Character),
				typeof(Item)
			}));
			foreach (Farmer allFarmer3 in Game1.getAllFarmers())
			{
				foreach (Quest quest in allFarmer3.questLog)
				{
					FieldInfo[] fields2 = quest.GetType().GetFields();
					foreach (FieldInfo field in fields2)
					{
						if (field.FieldType == typeof(NetDescriptionElementList))
						{
							NetDescriptionElementList fieldValue = (NetDescriptionElementList)field.GetValue(quest);
							if (fieldValue == null)
							{
								continue;
							}
							foreach (DescriptionElement entry in fieldValue)
							{
								MigrateLegacyDescriptionElement(serializer, entry);
							}
						}
						else if (field.FieldType == typeof(NetDescriptionElementRef))
						{
							MigrateLegacyDescriptionElement(serializer, ((NetDescriptionElementRef)field.GetValue(quest))?.Value);
						}
					}
				}
			}
			return true;
		}
		case SaveFixes.SetBushesInPots:
			Utility.ForEachItem(delegate(Item item)
			{
				if (item is IndoorPot indoorPot && indoorPot.bush.Value != null)
				{
					indoorPot.bush.Value.inPot.Value = true;
				}
				return true;
			});
			return true;
		case SaveFixes.FixItemsNotMarkedAsInInventory:
			foreach (Farmer farmer in Game1.getAllFarmers())
			{
				foreach (Item equippedItem in farmer.GetEquippedItems())
				{
					equippedItem.HasBeenInInventory = true;
				}
				foreach (Item item in farmer.Items)
				{
					if (item != null)
					{
						item.HasBeenInInventory = true;
					}
				}
			}
			return true;
		case SaveFixes.BetaFixesFor16:
			Utility.ForEachItem(delegate(Item item)
			{
				if (item is Boots || item is Clothing || item is Hat)
				{
					item.FixStackSize();
				}
				return true;
			});
			return true;
		case SaveFixes.FixBasicWines:
			Utility.ForEachItem(delegate(Item item)
			{
				if (item.ParentSheetIndex == 348 && item.QualifiedItemId.Equals("(O)348"))
				{
					item.ParentSheetIndex = 123;
				}
				return true;
			});
			return true;
		case SaveFixes.ResetForges_1_6:
			SaveMigrator_1_5.ResetForges();
			return true;
		case SaveFixes.RestoreAncientSeedRecipe_1_6:
			foreach (Farmer farmer in Game1.getAllFarmers())
			{
				if (farmer.mailReceived.Contains("museumCollectedRewardO_499_1") && !farmer.craftingRecipes.ContainsKey("Ancient Seeds"))
				{
					farmer.craftingRecipes.Add("Ancient Seeds", 0);
				}
			}
			return true;
		case SaveFixes.FixInstancedInterior:
			Utility.ForEachBuilding(delegate(Building building)
			{
				if (building.GetIndoorsType() == IndoorsType.Instanced)
				{
					GameLocation indoors = building.GetIndoors();
					if (indoors.uniqueName.Value == null)
					{
						indoors.uniqueName.Value = (building.GetData()?.IndoorMap ?? indoors.Name) + GuidHelper.NewGuid();
					}
					if (indoors is AnimalHouse animalHouse)
					{
						animalHouse.animalsThatLiveHere.RemoveWhere((long id) => Utility.getAnimal(id)?.home != building);
					}
				}
				return true;
			});
			return true;
		case SaveFixes.FixNonInstancedInterior:
			Utility.ForEachBuilding(delegate(Building building)
			{
				if (building.GetIndoorsType() == IndoorsType.Global)
				{
					building.GetIndoors().uniqueName.Value = null;
				}
				return true;
			});
			return true;
		case SaveFixes.PopulateConstructedBuildings:
			Utility.ForEachBuilding(delegate(Building building)
			{
				if (!string.IsNullOrWhiteSpace(building.buildingType.Value))
				{
					if (!building.isUnderConstruction(ignoreUpgrades: false))
					{
						Game1.player.team.constructedBuildings.Add(building.buildingType.Value);
					}
					BuildingData data = building.GetData();
					while (!string.IsNullOrWhiteSpace(data?.BuildingToUpgrade))
					{
						Game1.player.team.constructedBuildings.Add(data.BuildingToUpgrade);
						Building.TryGetData(data.BuildingToUpgrade, out data);
					}
				}
				return true;
			}, ignoreUnderConstruction: false);
			return true;
		case SaveFixes.FixRacoonQuestCompletion:
			if (NetWorldState.checkAnywhereForWorldStateID("forestStumpFixed"))
			{
				Game1.player.removeQuest("134");
				foreach (Farmer offlineFarmhand in Game1.getOfflineFarmhands())
				{
					offlineFarmhand.removeQuest("134");
				}
			}
			return true;
		default:
			return false;
		}
	}

	/// <summary>Convert individually implemented buildings that were saved before Stardew Valley 1.6 to the new Data/BuildingsData format.</summary>
	/// <param name="location">The location whose buildings to convert.</param>
	public static void ConvertBuildingsToData(GameLocation location)
	{
		for (int i = location.buildings.Count - 1; i >= 0; i--)
		{
			Building building = location.buildings[i];
			GameLocation indoors = building.GetIndoors();
			if (indoors != null)
			{
				ConvertBuildingsToData(indoors);
			}
			switch (building.buildingType.Value)
			{
			case "Log Cabin":
			case "Plank Cabin":
			case "Stone Cabin":
				building.skinId.Value = building.buildingType.Value;
				building.buildingType.Value = "Cabin";
				building.ReloadBuildingData();
				building.updateInteriorWarps();
				break;
			}
			string expectedType = building.GetData()?.BuildingType;
			if (expectedType != null && expectedType != building.GetType().FullName)
			{
				Building newBuilding = Building.CreateInstanceFromId(building.buildingType.Value, new Vector2((int)building.tileX, (int)building.tileY));
				if (newBuilding != null)
				{
					newBuilding.indoors.Value = building.indoors.Value;
					newBuilding.buildingType.Value = building.buildingType.Value;
					newBuilding.tileX.Value = building.tileX.Value;
					newBuilding.tileY.Value = building.tileY.Value;
					location.buildings.RemoveAt(i);
					location.buildings.Add(newBuilding);
					TransferValuesToDataBuilding(building, newBuilding);
				}
			}
		}
	}

	/// <summary>Copy values from an older pre-1.6 building to a new data-driven <see cref="T:StardewValley.Buildings.Building" /> instance.</summary>
	/// <param name="oldBuilding">The pre-1.6 building instance.</param>
	/// <param name="newBuilding">The new data-driven building instance that will replace <paramref name="oldBuilding" />.</param>
	public static void TransferValuesToDataBuilding(Building oldBuilding, Building newBuilding)
	{
		newBuilding.animalDoorOpen.Value = oldBuilding.animalDoorOpen.Value;
		newBuilding.animalDoorOpenAmount.Value = oldBuilding.animalDoorOpenAmount.Value;
		newBuilding.netBuildingPaintColor.Value.CopyFrom(oldBuilding.netBuildingPaintColor.Value);
		newBuilding.modData.CopyFrom(oldBuilding.modData.Pairs);
		if (oldBuilding is Mill oldMill)
		{
			oldMill.TransferValuesToNewBuilding(newBuilding);
		}
	}

	/// <summary>Migrate all farmhands from Cabin.deprecatedFarmhand into NetWorldState.</summary>
	/// <param name="locations">The locations to scan for cabins.</param>
	public static void MigrateFarmhands(List<GameLocation> locations)
	{
		foreach (GameLocation location in locations)
		{
			foreach (Building building in location.buildings)
			{
				if (building.GetIndoors() is Cabin { obsolete_farmhand: var farmhand } cabin)
				{
					cabin.obsolete_farmhand = null;
					Game1.netWorldState.Value.farmhandData[farmhand.UniqueMultiplayerID] = farmhand;
					cabin.farmhandReference.Value = farmhand;
				}
			}
		}
	}

	/// <summary>Migrate saved bundle data from Stardew Valley 1.5.6 or earlier to the new format.</summary>
	/// <param name="bundleData">The raw bundle data to standardize.</param>
	public static void StandardizeBundleFields(Dictionary<string, string> bundleData)
	{
		string[] array = bundleData.Keys.ToArray();
		foreach (string key in array)
		{
			string[] fields = bundleData[key].Split('/');
			if (fields.Length < 7)
			{
				Array.Resize(ref fields, 7);
				fields[6] = fields[0];
				bundleData[key] = string.Join("/", fields);
			}
		}
	}

	/// <summary>For a building with an upgrade started before 1.6, get the building type it should be upgraded to if possible.</summary>
	/// <param name="fromBuildingType">The building type before the upgrade finishes.</param>
	public static string InferBuildingUpgradingTo(string fromBuildingType)
	{
		switch (fromBuildingType)
		{
		case "Coop":
			return "Big Coop";
		case "Big Coop":
			return "Deluxe Coop";
		case "Barn":
			return "Big Barn";
		case "Big Barn":
			return "Deluxe Barn";
		case "Shed":
			return "Big Shed";
		default:
			foreach (KeyValuePair<string, BuildingData> pair in Game1.buildingData)
			{
				if (pair.Value.BuildingToUpgrade == fromBuildingType)
				{
					return pair.Key;
				}
			}
			return null;
		}
	}

	/// <summary>For a machine which contains output produced before 1.6, set the <see cref="F:StardewValley.Object.lastInputItem" /> and <see cref="F:StardewValley.Object.lastOutputRuleId" /> values when possible. This ensures that some machine logic works as expected (e.g. crystalariums resuming on collect).</summary>
	/// <param name="machine">The machine which produced output.</param>
	/// <remarks>This is heuristic, and some fields may not be set if it's not possible to retroactively infer them.</remarks>
	public static void InferMachineInputOutputFields(Object machine)
	{
		Object output = machine.heldObject.Value;
		string outputItemId = output?.QualifiedItemId;
		if (outputItemId == null)
		{
			return;
		}
		NetRef<Item> inputItem = machine.lastInputItem;
		NetString outputRule = machine.lastOutputRuleId;
		switch (machine.QualifiedItemId)
		{
		case "(BC)211":
			break;
		case "(BC)105":
			break;
		case "(BC)264":
			break;
		case "(BC)90":
			switch (outputItemId)
			{
			case "(O)466":
			case "(O)465":
			case "(O)369":
			case "(O)805":
				outputRule.Value = "Default";
				break;
			}
			break;
		case "(BC)163":
			switch (outputItemId)
			{
			case "(O)424":
				outputRule.Value = "Cheese";
				break;
			case "(O)426":
				outputRule.Value = "GoatCheese";
				break;
			case "(O)348":
				outputRule.Value = "Wine";
				break;
			case "(O)459":
				outputRule.Value = "Mead";
				break;
			case "(O)303":
				outputRule.Value = "PaleAle";
				break;
			case "(O)346":
				outputRule.Value = "Beer";
				break;
			}
			if (outputRule.Value != null)
			{
				inputItem.Value = output.getOne();
				inputItem.Value.Quality = 0;
			}
			break;
		case "(BC)114":
			if (outputItemId == "(O)382")
			{
				outputRule.Value = "Default";
				inputItem.Value = ItemRegistry.Create("(O)388", 10);
			}
			break;
		case "(BC)17":
			if (outputItemId == "(O)428")
			{
				outputRule.Value = "Default";
				inputItem.Value = ItemRegistry.Create("(O)440");
			}
			break;
		case "(BC)13":
			switch (outputItemId)
			{
			case "(O)334":
				outputRule.Value = "Default_CopperOre";
				inputItem.Value = ItemRegistry.Create("(O)378", 5);
				break;
			case "(O)335":
				outputRule.Value = "Default_IronOre";
				inputItem.Value = ItemRegistry.Create("(O)380", 5);
				break;
			case "(O)336":
				outputRule.Value = "Default_GoldOre";
				inputItem.Value = ItemRegistry.Create("(O)384", 5);
				break;
			case "(O)337":
				outputRule.Value = "Default_IridiumOre";
				inputItem.Value = ItemRegistry.Create("(O)386", 5);
				break;
			case "(O)338":
				if (output.Stack > 1)
				{
					outputRule.Value = "Default_FireQuartz";
					inputItem.Value = ItemRegistry.Create("(O)82");
				}
				else
				{
					outputRule.Value = "Default_Quartz";
					inputItem.Value = ItemRegistry.Create("(O)80");
				}
				break;
			case "(O)277":
				outputRule.Value = "Default_Bouquet";
				inputItem.Value = ItemRegistry.Create("(O)458");
				break;
			case "(O)910":
				outputRule.Value = "Default_RadioactiveOre";
				inputItem.Value = ItemRegistry.Create("(O)909", 5);
				break;
			}
			break;
		case "(BC)265":
			outputRule.Value = "Default";
			break;
		case "(BC)12":
			switch (outputItemId)
			{
			case "(O)346":
				outputRule.Value = "Default_Wheat";
				inputItem.Value = ItemRegistry.Create("(O)262");
				break;
			case "(O)303":
				outputRule.Value = "Default_Hops";
				inputItem.Value = ItemRegistry.Create("(O)304");
				break;
			case "(O)614":
				outputRule.Value = "Default_TeaLeaves";
				inputItem.Value = ItemRegistry.Create("(O)815");
				break;
			case "(O)395":
				outputRule.Value = "Default_CoffeeBeans";
				inputItem.Value = ItemRegistry.Create("(O)433", 5);
				break;
			case "(O)340":
				outputRule.Value = "Default_Honey";
				inputItem.Value = ItemRegistry.Create("(O)459", 5);
				break;
			default:
			{
				Object.PreserveType? value = output.preserve.Value;
				if (value.HasValue)
				{
					switch (value.GetValueOrDefault())
					{
					case Object.PreserveType.Juice:
						outputRule.Value = "Default_Juice";
						inputItem.Value = ItemRegistry.Create(output.preservedParentSheetIndex.Value, 1, 0, allowNull: true);
						break;
					case Object.PreserveType.Wine:
						outputRule.Value = "Default_Wine";
						inputItem.Value = ItemRegistry.Create(output.preservedParentSheetIndex.Value, 1, 0, allowNull: true);
						break;
					}
				}
				break;
			}
			}
			break;
		case "(BC)15":
			switch (outputItemId)
			{
			case "(O)445":
				outputRule.Value = "Default_SturgeonRoe";
				inputItem.Value = ItemRegistry.GetObjectTypeDefinition().CreateFlavoredRoe(ItemRegistry.Create<Object>("(O)698"));
				break;
			case "(O)447":
				outputRule.Value = "Default_Roe";
				inputItem.Value = ItemRegistry.GetObjectTypeDefinition().CreateFlavoredRoe(ItemRegistry.Create<Object>(output.preservedParentSheetIndex.Value));
				break;
			case "(O)342":
				outputRule.Value = "Default_Pickled";
				inputItem.Value = ItemRegistry.Create(output.preservedParentSheetIndex.Value, 1, 0, allowNull: true);
				break;
			case "(O)344":
				outputRule.Value = "Default_Jelly";
				inputItem.Value = ItemRegistry.Create(output.preservedParentSheetIndex.Value, 1, 0, allowNull: true);
				break;
			}
			break;
		case "(BC)16":
			if (!(outputItemId == "(O)426"))
			{
				if (outputItemId == "(O)424")
				{
					if (output.Quality == 0)
					{
						outputRule.Value = "Default_Milk";
						inputItem.Value = ItemRegistry.Create("(O)184");
					}
					else
					{
						outputRule.Value = "Default_LargeMilk";
						inputItem.Value = ItemRegistry.Create("(O)186");
					}
				}
			}
			else if (output.Quality == 0)
			{
				outputRule.Value = "Default_GoatMilk";
				inputItem.Value = ItemRegistry.Create("(O)436");
			}
			else
			{
				outputRule.Value = "Default_LargeGoatMilk";
				inputItem.Value = ItemRegistry.Create("(O)438");
			}
			break;
		case "(BC)20":
			switch (outputItemId)
			{
			case "(O)338":
				break;
			case "(O)382":
			case "(O)380":
			case "(O)390":
				outputRule.Value = "Default_Trash";
				inputItem.Value = ItemRegistry.Create("(O)168");
				break;
			case "(O)388":
				outputRule.Value = "Default_Driftwood";
				inputItem.Value = ItemRegistry.Create("(O)169");
				break;
			case "(O)428":
			case "(O)93":
				outputRule.Value = "Default_SoggyNewspaper";
				inputItem.Value = ItemRegistry.Create("(O)172");
				break;
			}
			break;
		case "(BC)24":
			switch (outputItemId)
			{
			case "(O)306":
				switch (output.Stack)
				{
				case 10:
					outputRule.Value = "Default_OstrichEgg";
					inputItem.Value = ItemRegistry.Create("(O)289", 1, output.Quality);
					break;
				case 3:
					outputRule.Value = "Default_GoldenEgg";
					inputItem.Value = ItemRegistry.Create("(O)928");
					break;
				default:
					if (output.Quality == 2)
					{
						outputRule.Value = "Default_LargeEgg";
						inputItem.Value = ItemRegistry.Create("(O)174");
					}
					else
					{
						outputRule.Value = "Default_Egg";
						inputItem.Value = ItemRegistry.Create("(O)176");
					}
					break;
				}
				break;
			case "(O)307":
				outputRule.Value = "Default_DuckEgg";
				inputItem.Value = ItemRegistry.Create("(O)442");
				break;
			case "(O)308":
				outputRule.Value = "Default_VoidEgg";
				inputItem.Value = ItemRegistry.Create("(O)305");
				break;
			case "(O)807":
				outputRule.Value = "Default_DinosaurEgg";
				inputItem.Value = ItemRegistry.Create("(O)107");
				break;
			}
			break;
		case "(BC)19":
			if (!(outputItemId == "(O)247") && outputItemId == "(O)432")
			{
				outputRule.Value = "Default_Truffle";
				inputItem.Value = ItemRegistry.Create("(O)430");
			}
			break;
		case "(BC)101":
		case "(BC)254":
		case "(BC)156":
			outputRule.Value = "Default";
			inputItem.Value = output.getOne();
			break;
		case "(BC)158":
			outputRule.Value = "Default";
			inputItem.Value = ItemRegistry.Create("(O)766", 100);
			break;
		case "(BC)21":
			outputRule.Value = "Default";
			inputItem.Value = output.getOne();
			break;
		case "(BC)25":
		{
			outputRule.Value = "Default";
			if (outputItemId != "(O)499" && output.HasTypeObject() && Game1.cropData.TryGetValue(output.ItemId, out var cropData) && cropData.HarvestItemId != null)
			{
				inputItem.Value = ItemRegistry.Create(cropData.HarvestItemId, 1, 0, allowNull: true);
			}
			break;
		}
		case "(BC)182":
			outputRule.Value = "Default";
			break;
		case "(BC)10":
		case "(BC)154":
		case "(BC)117":
		case "(BC)246":
		case "(BC)231":
		case "(BC)9":
		case "(BC)280":
		case "(BC)127":
		case "(BC)160":
		case "(BC)128":
			outputRule.Value = "Default";
			break;
		}
	}

	/// <summary>Migrate a pre-1.6 quest to the new format.</summary>
	/// <param name="serializer">The XML serializer with which to serialize/deserialize <see cref="T:StardewValley.Quests.DescriptionElement" /> and <see cref="T:StardewValley.SaveMigrations.SaveMigrator_1_6.LegacyDescriptionElement" /> values.</param>
	/// <param name="element">The description element to migrate.</param>
	/// <remarks>
	///   This updates quest data for two changes in 1.6:
	///
	///   <list type="bullet">
	///     <item><description>
	///       The way <see cref="F:StardewValley.Quests.DescriptionElement.substitutions" /> values are stored in the save XML changed from this:
	///
	///       <code>
	///         &lt;objective&gt;
	///           &lt;xmlKey&gt;Strings\StringsFromCSFiles:SocializeQuest.cs.13802&lt;/xmlKey&gt;
	///           &lt;param&gt;
	///             &lt;anyType xsi:type="xsd:int"&gt;4&lt;/anyType&gt;
	///             &lt;anyType xsi:type="xsd:int"&gt;28&lt;/anyType&gt;
	///           &lt;/param&gt;
	///         &lt;/objective&gt;
	///       </code>
	///
	///      To this:
	///
	///       <code>
	///         &lt;objective&gt;
	///           &lt;xmlKey&gt;Strings\StringsFromCSFiles:SocializeQuest.cs.13802&lt;/xmlKey&gt;
	///           &lt;param xsi:type="xsd:int"&gt;4&lt;/param&gt;
	///           &lt;param xsi:type="xsd:int"&gt;28&lt;/param&gt;
	///         &lt;/objective&gt;
	///       </code>
	///
	///       If the given description element is affected, this method re-deserializes the data into the correct format.
	///   </description></item>
	///
	///   <item><description>Some translation keys were merged to fix gender issues.</description></item>
	///   </list>
	/// </remarks>
	public static void MigrateLegacyDescriptionElement(Lazy<XmlSerializer> serializer, DescriptionElement element)
	{
		if (element == null)
		{
			return;
		}
		List<object> substitutions = element.substitutions;
		if (substitutions != null && substitutions.Count == 1 && element.substitutions[0] is XmlNode[] nodes)
		{
			StringBuilder xml = new StringBuilder("<?xml version=\"1.0\" encoding=\"utf-8\"?><LegacyDescriptionElement xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><param>");
			XmlNode[] array = nodes;
			foreach (XmlNode node in array)
			{
				xml.Append(node.OuterXml);
			}
			xml.Append("</param></LegacyDescriptionElement>");
			LegacyDescriptionElement data;
			using (StringReader stringReader = new StringReader(xml.ToString()))
			{
				using XmlReader xmlReader = new XmlTextReader(stringReader);
				data = (LegacyDescriptionElement)serializer.Value.Deserialize(xmlReader);
			}
			if (data != null)
			{
				element.substitutions = data.param;
			}
		}
		switch (element.translationKey)
		{
		case "Strings\\StringsFromCSFiles:FishingQuest.cs.13251":
			element.translationKey = "Strings\\StringsFromCSFiles:FishingQuest.cs.13248";
			break;
		case "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13563":
			element.translationKey = "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13560";
			break;
		case "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13574":
			element.translationKey = "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13571";
			break;
		}
		List<object> substitutions2 = element.substitutions;
		if (substitutions2 == null || substitutions2.Count <= 0)
		{
			return;
		}
		foreach (object substitution in element.substitutions)
		{
			if (substitution is DescriptionElement childElement)
			{
				MigrateLegacyDescriptionElement(serializer, childElement);
			}
		}
	}
}
