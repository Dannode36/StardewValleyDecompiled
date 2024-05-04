using System.Collections.Generic;
using StardewValley.Locations;

namespace StardewValley.Pathfinding;

/// <summary>Handles pathfinding between locations.</summary>
public static class WarpPathfindingCache
{
	/// <summary>Every possible path through location names that NPCs can take while pathfinding, indexed by the start location.</summary>
	/// <remarks>For example, <c>"BusStop": [ "BusStop", "Town", "Mountain" ]</c> means that an NPC in the bus stop can warp to town and then to the mountain.</remarks>
	private static readonly Dictionary<string, List<LocationWarpRoute>> Routes = new Dictionary<string, List<LocationWarpRoute>>();

	/// <summary>The location names which NPCs aren't allowed to warp through.</summary>
	/// <remarks>The farmhand cellars are added automatically.</remarks>
	public static readonly HashSet<string> IgnoreLocationNames = new HashSet<string> { "Backwoods", "Cellar", "Farm" };

	/// <summary>A map of warp targets to the actual location name NPCs should warp to.</summary>
	public static readonly Dictionary<string, string> OverrideTargetNames = new Dictionary<string, string> { ["BoatTunnel"] = "IslandSouth" };

	/// <summary>The locations which can only be accessed by NPCs of one gender.</summary>
	public static readonly Dictionary<string, Gender> GenderRestrictions = new Dictionary<string, Gender>
	{
		["BathHouse_MensLocker"] = Gender.Male,
		["BathHouse_WomensLocker"] = Gender.Female
	};

	/// <summary>Cache the possible pathfinding routes between game locations.</summary>
	public static void PopulateCache()
	{
		for (int i = 1; i <= Game1.netWorldState.Value.HighestPlayerLimit; i++)
		{
			IgnoreLocationNames.Add("Cellar" + i);
		}
		Routes.Clear();
		foreach (GameLocation l in Game1.locations)
		{
			if (!IgnoreLocationNames.Contains(l.NameOrUniqueName))
			{
				ExploreWarpPoints(l, new List<string>(), null);
			}
		}
	}

	/// <summary>Get a valid pathfinding route between a start and destination location.</summary>
	/// <param name="startingLocation">The name of the location the NPC is starting from.</param>
	/// <param name="endingLocation">The name of the destination location.</param>
	/// <param name="gender">The NPC's gender, used to choose gender-specific routes like the pool locker rooms.</param>
	/// <returns>If a valid route was found, returns a list of location names to transit through including the start and destination locations. For example, <c>[ "BusStop", "Town", "Mountain" ]</c> means that an NPC in the bus stop can warp to town and then to the mountain. If no valid route was found, returns null.</returns>
	public static string[] GetLocationRoute(string startingLocation, string endingLocation, Gender gender)
	{
		if (Routes.TryGetValue(startingLocation, out var routes))
		{
			foreach (LocationWarpRoute route in routes)
			{
				if (route.LocationNames[route.LocationNames.Length - 1] == endingLocation)
				{
					Gender? onlyGender = route.OnlyGender;
					if (!onlyGender.HasValue || route.OnlyGender == gender || gender == Gender.Undefined)
					{
						return route.LocationNames;
					}
				}
			}
		}
		return null;
	}

	/// <summary>Recursively populate the cache based on every location reachable through warps starting from this location.</summary>
	/// <param name="location">The location to start from.</param>
	/// <param name="route">The location names explored up to this point for the current route, excluding the <paramref name="location" />.</param>
	/// <param name="genderRestriction">The gender restriction for the route up to this point, if any. For example, a route which passes through the men's locker room is restricted to male NPCs.</param>
	private static void ExploreWarpPoints(GameLocation location, List<string> route, Gender? genderRestriction)
	{
		string locationName = location?.name.Value;
		if (locationName == null || location.ShouldExcludeFromNpcPathfinding() || route.Contains(locationName))
		{
			return;
		}
		if (GenderRestrictions.TryGetValue(locationName, out var newGenderRestriction))
		{
			if (genderRestriction.HasValue && genderRestriction.Value != newGenderRestriction)
			{
				return;
			}
			genderRestriction = newGenderRestriction;
		}
		route.Add(locationName);
		if (route.Count > 1)
		{
			AddRoute(route, genderRestriction);
		}
		bool hasWarps = location.warps.Count > 0;
		bool hasDoors = location.doors.Length > 0;
		if (hasWarps || hasDoors)
		{
			HashSet<string> exploredTargets = new HashSet<string> { locationName };
			if (route.Count > 1)
			{
				exploredTargets.Add(route[route.Count - 2]);
			}
			if (hasWarps)
			{
				foreach (Warp warp in location.warps)
				{
					ExploreWarpPoints(warp.TargetName, route, genderRestriction, exploredTargets);
				}
			}
			if (hasDoors)
			{
				foreach (string value in location.doors.Values)
				{
					ExploreWarpPoints(value, route, genderRestriction, exploredTargets);
				}
			}
		}
		if (route.Count > 0)
		{
			route.RemoveAt(route.Count - 1);
		}
	}

	/// <summary>Recursively populate the cache based on every location reachable through warps starting from this location.</summary>
	/// <param name="locationName">The location name to start from.</param>
	/// <param name="route">The location names explored up to this point for the current route, excluding the <paramref name="locationName" />.</param>
	/// <param name="genderRestriction">The gender restriction for the route up to this point, if any. For example, a route which passes through the men's locker room is restricted to male NPCs.</param>
	/// <param name="seenTargets">The warp target names which have already been explored from this location.</param>
	/// <returns>Returns whether any routes were added.</returns>
	private static void ExploreWarpPoints(string locationName, List<string> route, Gender? genderRestriction, HashSet<string> seenTargets)
	{
		if (OverrideTargetNames.TryGetValue(locationName, out var newLocationName))
		{
			locationName = newLocationName;
		}
		if (seenTargets.Add(locationName) && !IgnoreLocationNames.Contains(locationName) && !MineShaft.IsGeneratedLevel(locationName, out var level) && !VolcanoDungeon.IsGeneratedLevel(locationName, out level))
		{
			ExploreWarpPoints(Game1.getLocationFromName(locationName), route, genderRestriction);
		}
	}

	/// <summary>Add a route to the <see cref="F:StardewValley.Pathfinding.WarpPathfindingCache.Routes" /> cache.</summary>
	/// <param name="route">The location names in the route.</param>
	/// <param name="onlyGender">If set, this route can only be used by NPCs of the given gender.</param>
	private static void AddRoute(List<string> route, Gender? onlyGender)
	{
		if (!Routes.TryGetValue(route[0], out var routes))
		{
			routes = (Routes[route[0]] = new List<LocationWarpRoute>());
		}
		routes.Add(new LocationWarpRoute(route.ToArray(), onlyGender));
	}
}
