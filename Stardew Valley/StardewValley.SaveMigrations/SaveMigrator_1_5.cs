using System;
using Microsoft.Xna.Framework;
using StardewValley.Buildings;
using StardewValley.Enchantments;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.Quests;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;

namespace StardewValley.SaveMigrations;

/// <summary>Migrates existing save files for compatibility with Stardew Valley 1.5.</summary>
public class SaveMigrator_1_5 : ISaveMigrator
{
	/// <inheritdoc />
	public Version GameVersion { get; } = new Version(1, 5);


	/// <inheritdoc />
	public bool ApplySaveFix(SaveFixes saveFix)
	{
		switch (saveFix)
		{
		case SaveFixes.BedsToFurniture:
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				if (location is FarmHouse { HasOwner: var hasOwner } farmHouse)
				{
					for (int j = 0; j < farmHouse.map.Layers[0].TileWidth; j++)
					{
						for (int k = 0; k < farmHouse.map.Layers[0].TileHeight; k++)
						{
							if (farmHouse.doesTileHaveProperty(j, k, "DefaultBedPosition", "Back") != null)
							{
								if (farmHouse.upgradeLevel == 0)
								{
									farmHouse.furniture.Add(new BedFurniture(BedFurniture.DEFAULT_BED_INDEX, new Vector2(j, k)));
								}
								else
								{
									string itemId = BedFurniture.DOUBLE_BED_INDEX;
									if (hasOwner && !farmHouse.owner.activeDialogueEvents.ContainsKey("pennyRedecorating"))
									{
										if (farmHouse.owner.mailReceived.Contains("pennyQuilt0"))
										{
											itemId = "2058";
										}
										if (farmHouse.owner.mailReceived.Contains("pennyQuilt1"))
										{
											itemId = "2064";
										}
										if (farmHouse.owner.mailReceived.Contains("pennyQuilt2"))
										{
											itemId = "2070";
										}
									}
									farmHouse.furniture.Add(new BedFurniture(itemId, new Vector2(j, k)));
								}
							}
						}
					}
				}
				return true;
			});
			return true;
		case SaveFixes.ChildBedsToFurniture:
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				if (location is FarmHouse farmHouse2)
				{
					for (int l = 0; l < farmHouse2.map.Layers[0].TileWidth; l++)
					{
						for (int m = 0; m < farmHouse2.map.Layers[0].TileHeight; m++)
						{
							if (farmHouse2.doesTileHaveProperty(l, m, "DefaultChildBedPosition", "Back") != null)
							{
								farmHouse2.furniture.Add(new BedFurniture(BedFurniture.CHILD_BED_INDEX, new Vector2(l, m)));
							}
						}
					}
				}
				return true;
			});
			return true;
		case SaveFixes.ModularizeFarmStructures:
			Game1.getFarm().AddDefaultBuildings();
			return true;
		case SaveFixes.FixFlooringFlags:
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				foreach (TerrainFeature value in location.terrainFeatures.Values)
				{
					if (value is Flooring flooring)
					{
						flooring.ApplyFlooringFlags();
					}
				}
				return true;
			});
			return true;
		case SaveFixes.FixStableOwnership:
			Utility.ForEachBuilding(delegate(Stable stable)
			{
				if (stable.owner.Value == -6666666 && Game1.getFarmerMaybeOffline(-6666666L) == null)
				{
					stable.owner.Value = Game1.player.UniqueMultiplayerID;
				}
				return true;
			});
			return true;
		case SaveFixes.ResetForges:
			ResetForges();
			return true;
		case SaveFixes.MakeDarkSwordVampiric:
			Utility.ForEachItem(delegate(Item item)
			{
				if (item is MeleeWeapon { QualifiedItemId: "(W)2" } meleeWeapon)
				{
					meleeWeapon.AddEnchantment(new VampiricEnchantment());
				}
				return true;
			});
			return true;
		case SaveFixes.FixBeachFarmBushes:
			if (Game1.whichFarm == 6)
			{
				Farm farm = Game1.getFarm();
				Vector2[] array = new Vector2[4]
				{
					new Vector2(77f, 4f),
					new Vector2(78f, 3f),
					new Vector2(83f, 4f),
					new Vector2(83f, 3f)
				};
				foreach (Vector2 bushLocation in array)
				{
					foreach (LargeTerrainFeature feature in farm.largeTerrainFeatures)
					{
						if (feature.Tile == bushLocation)
						{
							if (feature is Bush bush)
							{
								bush.Tile = new Vector2(bush.Tile.X, bush.Tile.Y + 1f);
							}
							break;
						}
					}
				}
			}
			return true;
		case SaveFixes.OstrichIncubatorFragility:
			Utility.ForEachItem(delegate(Item item)
			{
				if (item is Object { Fragility: 2, Name: "Ostrich Incubator" } @object)
				{
					@object.Fragility = 0;
				}
				return true;
			});
			return true;
		case SaveFixes.LeoChildrenFix:
			Utility.FixChildNameCollisions();
			return true;
		case SaveFixes.Leo6HeartGermanFix:
			if (Utility.HasAnyPlayerSeenEvent("6497428") && !Game1.MasterPlayer.hasOrWillReceiveMail("leoMoved"))
			{
				Game1.addMailForTomorrow("leoMoved", noLetter: true, sendToEveryone: true);
				Game1.player.team.requestLeoMove.Fire();
			}
			return true;
		case SaveFixes.BirdieQuestRemovedFix:
			foreach (Farmer who in Game1.getAllFarmers())
			{
				if (who.hasQuest("130"))
				{
					foreach (Quest quest in who.questLog)
					{
						if (quest.id == "130")
						{
							quest.canBeCancelled.Value = true;
						}
					}
				}
				if (who.hasOrWillReceiveMail("birdieQuestBegun") && !who.hasOrWillReceiveMail("birdieQuestFinished"))
				{
					who.addQuest("130");
				}
			}
			return true;
		case SaveFixes.SkippedSummit:
			if (Game1.MasterPlayer.mailReceived.Contains("Farm_Eternal"))
			{
				foreach (Farmer who in Game1.getAllFarmers())
				{
					if (!who.songsHeard.Contains("end_credits"))
					{
						who.mailReceived.Remove("Summit_event");
					}
				}
			}
			return true;
		default:
			return false;
		}
	}

	/// <summary>Reset all weapon stats to reflect any changes in buffs.</summary>
	public static void ResetForges()
	{
		Utility.ForEachItem(delegate(Item item)
		{
			if (item is MeleeWeapon meleeWeapon)
			{
				meleeWeapon.RecalculateAppliedForges();
			}
			return true;
		});
	}
}
