using System;

namespace StardewValley.SaveMigrations;

/// <summary>Migrates existing save files for compatibility with a newer game version.</summary>
public interface ISaveMigrator
{
	/// <summary>The game version to which the migration applies.</summary>
	Version GameVersion { get; }

	/// <summary>Apply a migration to the currently loaded save file.</summary>
	/// <param name="saveFix">The save migration to apply.</param>
	/// <returns>Returns whether the migration was applied.</returns>
	bool ApplySaveFix(SaveFixes saveFix);
}
