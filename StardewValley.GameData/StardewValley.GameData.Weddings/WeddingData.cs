using System.Collections.Generic;

namespace StardewValley.GameData.Weddings;

public class WeddingData
{
	/// <summary>A tokenizable string for the event script which plays the wedding.</summary>
	/// <remarks>The key is the internal name of the NPC or unique ID of the player being married, else <c>default</c> for the default script which automatically handles marrying either an NPC or player.</remarks>
	public Dictionary<string, string> EventScript;

	/// <summary>The other NPCs which should attend wedding events (unless they're the spouse), indexed by <see cref="F:StardewValley.GameData.Weddings.WeddingAttendeeData.Id" />.</summary>
	public Dictionary<string, WeddingAttendeeData> Attendees;
}
