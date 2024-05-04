using System;
using System.Collections.Generic;

namespace StardewValley.SaveMigrations;

/// <summary>Manages and applies migrations to save files.</summary>
public static class SaveMigrator
{
	/// <summary>The highest save fix that can be applied.</summary>
	public static readonly SaveFixes LatestSaveFix = SaveFixes.FixRacoonQuestCompletion;

	/// <summary>Apply all applicable save fixes to the currently loaded save file.</summary>
	public static void ApplySaveFixes()
	{
		if (!Game1.hasApplied1_3_UpdateChanges)
		{
			SaveMigrator_1_3.ApplyLegacyChanges();
		}
		if (!Game1.hasApplied1_4_UpdateChanges)
		{
			SaveMigrator_1_4.ApplyLegacyChanges();
		}
		if (Game1.lastAppliedSaveFix >= LatestSaveFix)
		{
			return;
		}
		List<ISaveMigrator> migrations = GetAllMigrators(reverse: true);
		for (SaveFixes saveFix = Game1.lastAppliedSaveFix + 1; saveFix < SaveFixes.MAX; saveFix++)
		{
			if (Enum.IsDefined(typeof(SaveFixes), saveFix))
			{
				Game1.log.Debug("Applying save fix: " + saveFix);
				using List<ISaveMigrator>.Enumerator enumerator = migrations.GetEnumerator();
				while (enumerator.MoveNext() && !enumerator.Current.ApplySaveFix(saveFix))
				{
				}
			}
			Game1.lastAppliedSaveFix = saveFix;
		}
	}

	/// <summary>Apply a single save fix to the currently loaded save file.</summary>
	/// <param name="fix">The save fix to apply.</param>
	/// <param name="loadedItems">A list of all items loaded from the save.</param>
	public static void ApplySingleSaveFix(SaveFixes fix, List<Item> loadedItems)
	{
		using List<ISaveMigrator>.Enumerator enumerator = GetAllMigrators().GetEnumerator();
		while (enumerator.MoveNext() && !enumerator.Current.ApplySaveFix(fix))
		{
		}
	}

	/// <summary>Get all save migrators that can be applied.</summary>
	/// <param name="reverse">Whether to get migrations in reverse order (from newer to older). This is used when applying all migrations, since most fixes applied will be in a newer version.</param>
	public static List<ISaveMigrator> GetAllMigrators(bool reverse = false)
	{
		List<ISaveMigrator> migrations = new List<ISaveMigrator>();
		Type[] types = typeof(ISaveMigrator).Assembly.GetTypes();
		foreach (Type type in types)
		{
			if (type.IsClass && !type.IsAbstract && typeof(ISaveMigrator).IsAssignableFrom(type))
			{
				ISaveMigrator migration = ((ISaveMigrator)Activator.CreateInstance(type)) ?? throw new InvalidOperationException("Failed to create instance of save migration '" + type.FullName + "'.");
				migrations.Add(migration);
			}
		}
		if (reverse)
		{
			migrations.Sort((ISaveMigrator a, ISaveMigrator b) => -a.GameVersion.CompareTo(b.GameVersion));
		}
		else
		{
			migrations.Sort((ISaveMigrator a, ISaveMigrator b) => a.GameVersion.CompareTo(b.GameVersion));
		}
		return migrations;
	}
}
