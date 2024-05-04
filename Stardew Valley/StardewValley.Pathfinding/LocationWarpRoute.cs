namespace StardewValley.Pathfinding;

/// <summary>A possible path through location names that NPCs can take while pathfinding.</summary>
public class LocationWarpRoute
{
	/// <summary>The sequential location names that an NPC can pathfind through.</summary>
	public readonly string[] LocationNames;

	/// <summary>If set, this route can only be used by NPCs of the given gender.</summary>
	public readonly Gender? OnlyGender;

	/// <summary>Construct an instance.</summary>
	/// <param name="locationNames">The sequential location names that an NPC can pathfind through.</param>
	/// <param name="onlyGender">If set, this route can only be used by NPCs of the given gender.</param>
	public LocationWarpRoute(string[] locationNames, Gender? onlyGender)
	{
		LocationNames = locationNames;
		OnlyGender = onlyGender;
	}
}
