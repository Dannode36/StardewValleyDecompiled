namespace StardewValley.Buildings;

/// <summary>The type of indoors location a building has.</summary>
public enum IndoorsType
{
	/// <summary>The building doesn't have an indoors location.</summary>
	None,
	/// <summary>The building has a unique interior location that was created for this building, which isn't in <see cref="P:StardewValley.Game1.locations" /> separately.</summary>
	Instanced,
	/// <summary>The building links to a global location like <c>FarmHouse</c> for its interior, which is in <see cref="P:StardewValley.Game1.locations" /> separately.</summary>
	Global
}
