using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Powers;

/// <summary>The content data for powers in the powers sub menu.</summary>
public class PowersData
{
	/// <summary>A tokenizable string for the power's display name.</summary>
	public string DisplayName;

	/// <summary>A tokenizable string for the power's description.</summary>
	[ContentSerializer(Optional = true)]
	public string Description = "";

	/// <summary>The asset name for the power's icon texture.</summary>
	public string TexturePath;

	/// <summary>The top-left pixel coordinate of the 16x16 sprite icon to show in the powers menu.</summary>
	public Point TexturePosition;

	/// <summary>If set, a game state query which indicates whether the power has been unlocked. Defaults to always unlocked.</summary>
	public string UnlockedCondition;

	/// <summary>Custom fields ignored by the base game, for use by mods.</summary>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, object> CustomFields;
}
