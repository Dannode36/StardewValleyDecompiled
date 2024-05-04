using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Objects;

namespace StardewValley.SaveMigrations;

/// <summary>Migrates existing save files for compatibility with Stardew Valley 1.3.</summary>
public class SaveMigrator_1_3 : ISaveMigrator
{
	/// <inheritdoc />
	public Version GameVersion { get; } = new Version(1, 3);


	/// <inheritdoc />
	public bool ApplySaveFix(SaveFixes saveFix)
	{
		return false;
	}

	/// <summary>Apply one-time save migrations which predate <see cref="T:StardewValley.SaveMigrations.SaveFixes" />.</summary>
	public static void ApplyLegacyChanges()
	{
		if (!Game1.IsMasterGame)
		{
			return;
		}
		FarmHouse farmHouse = Game1.RequireLocation<FarmHouse>("FarmHouse");
		farmHouse.furniture.Add(new Furniture("1792", Utility.PointToVector2(farmHouse.getFireplacePoint())));
		GameLocation town = Game1.RequireLocation("Town");
		if (!Game1.MasterPlayer.mailReceived.Contains("JojaMember") && town.CanItemBePlacedHere(new Vector2(57f, 16f)))
		{
			town.objects.Add(new Vector2(57f, 16f), ItemRegistry.Create<Object>("(BC)55"));
		}
		MarkFloorChestAsCollectedIfNecessary(10);
		MarkFloorChestAsCollectedIfNecessary(20);
		MarkFloorChestAsCollectedIfNecessary(40);
		MarkFloorChestAsCollectedIfNecessary(50);
		MarkFloorChestAsCollectedIfNecessary(60);
		MarkFloorChestAsCollectedIfNecessary(70);
		MarkFloorChestAsCollectedIfNecessary(80);
		MarkFloorChestAsCollectedIfNecessary(90);
		MarkFloorChestAsCollectedIfNecessary(100);
		Utility.ForEachVillager(delegate(NPC villager)
		{
			if (villager.datingFarmer == true)
			{
				if (Game1.player.friendshipData.TryGetValue(villager.Name, out var value) && !value.IsDating())
				{
					value.Status = FriendshipStatus.Dating;
				}
				villager.datingFarmer = null;
			}
			if (villager.divorcedFromFarmer == true)
			{
				if (Game1.player.friendshipData.TryGetValue(villager.Name, out var value2) && !value2.IsDating() && !value2.IsDivorced())
				{
					value2.Status = FriendshipStatus.Divorced;
				}
				villager.divorcedFromFarmer = null;
			}
			return true;
		});
		MigrateHorseIds();
		Game1.hasApplied1_3_UpdateChanges = true;
	}

	/// <summary>Mark a mine floor chest as collected if needed.</summary>
	/// <param name="floorNumber">The mine level.</param>
	/// <remarks>This should only be used on pre-1.3 saves, because the addition of multiplayer means it's not safe to assume that the local player is the one who opened the chest.</remarks>
	public static void MarkFloorChestAsCollectedIfNecessary(int floorNumber)
	{
		if (MineShaft.permanentMineChanges != null && MineShaft.permanentMineChanges.TryGetValue(floorNumber, out var changes) && changes.chestsLeft <= 0)
		{
			Game1.player.chestConsumedMineLevels[floorNumber] = true;
		}
	}

	/// <summary>Migrate the obsolete <see cref="F:StardewValley.Farmer.obsolete_friendships" /> into the new <see cref="F:StardewValley.Farmer.friendshipData" /> field, if applicable.</summary>
	/// <param name="player">The player whose data to migrate.</param>
	public static void MigrateFriendshipData(Farmer player)
	{
		if (player.obsolete_friendships != null && player.friendshipData.Length == 0)
		{
			foreach (KeyValuePair<string, int[]> friend in player.obsolete_friendships)
			{
				player.friendshipData[friend.Key] = new Friendship(friend.Value[0])
				{
					GiftsThisWeek = friend.Value[1],
					TalkedToToday = (friend.Value[2] != 0),
					GiftsToday = friend.Value[3],
					ProposalRejected = (friend.Value[4] != 0)
				};
			}
			player.obsolete_friendships = null;
		}
		if (string.IsNullOrEmpty(player.spouse))
		{
			return;
		}
		bool engaged = player.spouse.Contains("engaged");
		string spouseName = player.spouse.Replace("engaged", "");
		Friendship friendship = player.friendshipData[spouseName];
		if (friendship.Status == FriendshipStatus.Friendly || friendship.Status == FriendshipStatus.Dating || engaged)
		{
			friendship.Status = (engaged ? FriendshipStatus.Engaged : FriendshipStatus.Married);
			player.spouse = spouseName;
			if (!engaged)
			{
				friendship.WeddingDate = WorldDate.Now();
				friendship.WeddingDate.TotalDays -= player.obsolete_daysMarried.GetValueOrDefault();
				player.obsolete_daysMarried = null;
			}
		}
	}

	/// <summary>Fix the <see cref="P:StardewValley.Characters.Horse.HorseId" /> value for pre-1.3 horses.</summary>
	private static void MigrateHorseIds()
	{
		List<Stable> stablesMissingHorses = new List<Stable>();
		Utility.ForEachBuilding(delegate(Stable stable)
		{
			if (stable.getStableHorse() == null && stable.GetParentLocation() != null)
			{
				stablesMissingHorses.Add(stable);
			}
			return true;
		});
		for (int i = stablesMissingHorses.Count - 1; i >= 0; i--)
		{
			Stable stable = stablesMissingHorses[i];
			GameLocation parentLocation = stable.GetParentLocation();
			Rectangle boundingBox = stable.GetBoundingBox();
			foreach (NPC character in parentLocation.characters)
			{
				if (character is Horse horse && horse.HorseId == Guid.Empty && boundingBox.Intersects(horse.GetBoundingBox()))
				{
					horse.HorseId = stable.HorseId;
					stablesMissingHorses.RemoveAt(i);
					break;
				}
			}
		}
		for (int i = stablesMissingHorses.Count - 1; i >= 0; i--)
		{
			Stable stable = stablesMissingHorses[i];
			foreach (NPC character2 in stable.GetParentLocation().characters)
			{
				if (character2 is Horse horse && horse.HorseId == Guid.Empty)
				{
					horse.HorseId = stable.HorseId;
					stablesMissingHorses.RemoveAt(i);
					break;
				}
			}
		}
		foreach (Stable item in stablesMissingHorses)
		{
			item.grabHorse();
		}
	}
}
