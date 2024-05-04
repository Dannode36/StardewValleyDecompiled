using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

namespace StardewValley.SaveMigrations;

/// <summary>Migrates existing save files for compatibility with Stardew Valley 1.4.</summary>
public class SaveMigrator_1_4 : ISaveMigrator
{
	/// <inheritdoc />
	public Version GameVersion { get; } = new Version(1, 4);


	/// <inheritdoc />
	public bool ApplySaveFix(SaveFixes saveFix)
	{
		switch (saveFix)
		{
		case SaveFixes.AddTownBush:
			if (Game1.getLocationFromName("Town") is Town town)
			{
				Vector2 tile = new Vector2(61f, 93f);
				if (town.getLargeTerrainFeatureAt((int)tile.X, (int)tile.Y) == null)
				{
					town.largeTerrainFeatures.Add(new Bush(tile, 2, town));
				}
			}
			return true;
		case SaveFixes.StoredBigCraftablesStackFix:
			Utility.ForEachItem(delegate(Item item)
			{
				if (item is Object @object && (bool)@object.bigCraftable && @object.Stack == 0)
				{
					@object.Stack = 1;
				}
				return true;
			});
			return true;
		case SaveFixes.PorchedCabinBushesFix:
			Utility.ForEachBuilding(delegate(Building building)
			{
				if ((int)building.daysOfConstructionLeft <= 0 && building.GetIndoors() is Cabin)
				{
					building.removeOverlappingBushes(Game1.getFarm());
				}
				return true;
			});
			return true;
		case SaveFixes.ChangeObeliskFootprintHeight:
			Utility.ForEachBuilding(delegate(Building building)
			{
				if (building.buildingType.Value.Contains("Obelisk"))
				{
					building.tilesHigh.Value = 2;
					building.tileY.Value++;
				}
				return true;
			});
			return true;
		case SaveFixes.CreateStorageDressers:
			Utility.ForEachItem(delegate(Item item)
			{
				if (item is Clothing)
				{
					item.Category = -100;
				}
				return true;
			});
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				if (location is DecoratableLocation)
				{
					List<Furniture> list = new List<Furniture>();
					for (int l = 0; l < location.furniture.Count; l++)
					{
						Furniture furniture = location.furniture[l];
						if (furniture.ItemId == "704" || furniture.ItemId == "709" || furniture.ItemId == "714" || furniture.ItemId == "719")
						{
							StorageFurniture item2 = new StorageFurniture(furniture.ItemId, furniture.TileLocation, furniture.currentRotation);
							list.Add(item2);
							location.furniture.RemoveAt(l);
							l--;
						}
					}
					foreach (Furniture current3 in list)
					{
						location.furniture.Add(current3);
					}
				}
				return true;
			});
			return true;
		case SaveFixes.InferPreserves:
		{
			string[] preserveItemIndices = new string[4] { "(O)350", "(O)348", "(O)344", "(O)342" };
			string[] suffixes = new string[3] { " Juice", " Wine", " Jelly" };
			Object.PreserveType[] suffixPreserveTypes = new Object.PreserveType[3]
			{
				Object.PreserveType.Juice,
				Object.PreserveType.Wine,
				Object.PreserveType.Jelly
			};
			string[] prefixes = new string[1] { "Pickled " };
			Object.PreserveType[] prefixPreserveTypes = new Object.PreserveType[1] { Object.PreserveType.Pickle };
			Utility.ForEachItem(delegate(Item item)
			{
				if (!(item is Object object2))
				{
					return true;
				}
				if (!Utility.IsNormalObjectAtParentSheetIndex(object2, object2.ItemId))
				{
					return true;
				}
				if (!preserveItemIndices.Contains(object2.QualifiedItemId))
				{
					return true;
				}
				if (!object2.preserve.Value.HasValue)
				{
					bool flag = false;
					for (int j = 0; j < suffixes.Length; j++)
					{
						string text = suffixes[j];
						if (object2.Name.EndsWith(text))
						{
							string text2 = object2.Name.Substring(0, object2.Name.Length - text.Length);
							string text3 = null;
							foreach (ParsedItemData current in ItemRegistry.GetObjectTypeDefinition().GetAllData())
							{
								if (current.InternalName == text2)
								{
									text3 = current.ItemId;
									break;
								}
							}
							if (text3 != null)
							{
								object2.preservedParentSheetIndex.Value = text3;
								object2.preserve.Value = suffixPreserveTypes[j];
								flag = true;
								break;
							}
						}
					}
					if (flag)
					{
						return true;
					}
					for (int k = 0; k < prefixes.Length; k++)
					{
						string text4 = prefixes[k];
						if (object2.Name.StartsWith(text4))
						{
							string text5 = object2.Name.Substring(text4.Length);
							string text6 = null;
							foreach (ParsedItemData current2 in ItemRegistry.GetObjectTypeDefinition().GetAllData())
							{
								if (current2.InternalName == text5)
								{
									text6 = current2.ItemId;
									break;
								}
							}
							if (text6 != null)
							{
								object2.preservedParentSheetIndex.Value = text6;
								object2.preserve.Value = prefixPreserveTypes[k];
								break;
							}
						}
					}
				}
				return true;
			});
			return true;
		}
		case SaveFixes.TransferHatSkipHairFlag:
			Utility.ForEachItem(delegate(Item item)
			{
				if (item is Hat { skipHairDraw: not false } hat)
				{
					hat.hairDrawType.Set(0);
					hat.skipHairDraw = false;
				}
				return true;
			});
			return true;
		case SaveFixes.RevealSecretNoteItemTastes:
		{
			Dictionary<int, string> notesData = DataLoader.SecretNotes(Game1.content);
			for (int i = 0; i < 21; i++)
			{
				if (notesData.TryGetValue(i, out var note) && Game1.player.secretNotesSeen.Contains(i))
				{
					Utility.ParseGiftReveals(note);
				}
			}
			return true;
		}
		case SaveFixes.TransferHoneyTypeToPreserves:
			return true;
		case SaveFixes.TransferNoteBlockScale:
			Utility.ForEachItem(delegate(Item item)
			{
				if (item is Object object3 && (object3.QualifiedItemId == "(O)363" || object3.QualifiedItemId == "(O)464"))
				{
					object3.preservedParentSheetIndex.Value = ((int)object3.scale.X).ToString();
				}
				return true;
			});
			return true;
		case SaveFixes.FixCropHarvestAmountsAndInferSeedIndex:
			return true;
		case SaveFixes.quarryMineBushes:
		{
			GameLocation mountain = Game1.RequireLocation("Mountain");
			mountain.largeTerrainFeatures.Add(new Bush(new Vector2(101f, 18f), 1, mountain));
			mountain.largeTerrainFeatures.Add(new Bush(new Vector2(104f, 21f), 0, mountain));
			mountain.largeTerrainFeatures.Add(new Bush(new Vector2(105f, 18f), 0, mountain));
			return true;
		}
		case SaveFixes.MissingQisChallenge:
			foreach (Farmer farmer in Game1.getAllFarmers())
			{
				if (farmer.mailReceived.Contains("skullCave") && !farmer.hasQuest("20") && !farmer.hasOrWillReceiveMail("QiChallengeComplete"))
				{
					farmer.addQuest("20");
				}
			}
			return true;
		default:
			return false;
		}
	}

	/// <summary>Apply one-time save migrations which predate <see cref="T:StardewValley.SaveMigrations.SaveFixes" />.</summary>
	public static void ApplyLegacyChanges()
	{
		foreach (Farmer farmer in Game1.getAllFarmers())
		{
			foreach (string npcName in farmer.friendshipData.Keys)
			{
				farmer.friendshipData[npcName].Points = Math.Min(farmer.friendshipData[npcName].Points, 3125);
			}
		}
		foreach (KeyValuePair<string, string> pair in Game1.netWorldState.Value.BundleData)
		{
			int key = Convert.ToInt32(pair.Key.Split('/')[1]);
			if (!Game1.netWorldState.Value.Bundles.ContainsKey(key))
			{
				Game1.netWorldState.Value.Bundles.Add(key, new NetArray<bool, NetBool>(ArgUtility.SplitBySpace(pair.Value.Split('/')[2]).Length));
			}
			if (!Game1.netWorldState.Value.BundleRewards.ContainsKey(key))
			{
				Game1.netWorldState.Value.BundleRewards.Add(key, new NetBool(value: false));
			}
		}
		foreach (Farmer allFarmer in Game1.getAllFarmers())
		{
			foreach (Item item in allFarmer.Items)
			{
				if (item != null)
				{
					item.HasBeenInInventory = true;
				}
			}
		}
		RecalculateLostBookCount();
		Utility.iterateChestsAndStorage(delegate(Item item)
		{
			item.HasBeenInInventory = true;
		});
		Game1.hasApplied1_4_UpdateChanges = true;
	}

	/// <summary>Recalculate the number of lost books found.</summary>
	public static void RecalculateLostBookCount()
	{
		int highestLostBookCount = 0;
		foreach (Farmer player in Game1.getAllFarmers())
		{
			if (player.archaeologyFound.TryGetValue("102", out var data) && data[0] > 0)
			{
				highestLostBookCount = Math.Max(highestLostBookCount, data[0]);
				player.mailForTomorrow.Add("lostBookFound%&NL&%");
			}
		}
		Game1.netWorldState.Value.LostBooksFound = highestLostBookCount;
	}
}
