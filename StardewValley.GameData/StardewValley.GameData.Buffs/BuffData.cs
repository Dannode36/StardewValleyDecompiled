using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Buffs;

/// <summary>A predefined buff which can be applied in-game.</summary>
public class BuffData
{
	/// <summary>A tokenizable string for the translated buff name.</summary>
	public string DisplayName;

	/// <summary>A tokenizable string for the translated buff description.</summary>
	[ContentSerializer(Optional = true)]
	public string Description;

	/// <summary>Whether this buff counts as a debuff, so its duration should be halved when wearing a Sturdy Ring.</summary>
	[ContentSerializer(Optional = true)]
	public bool IsDebuff;

	/// <summary>The glow color to apply to the player, if any.</summary>
	[ContentSerializer(Optional = true)]
	public string GlowColor;

	/// <summary>The buff duration in milliseconds, or <c>-2</c> for a buff that should last all day.</summary>
	public int Duration;

	/// <summary>The maximum buff duration in milliseconds. If specified and larger than <see cref="F:StardewValley.GameData.Buffs.BuffData.Duration" />, a random value between <see cref="F:StardewValley.GameData.Buffs.BuffData.Duration" /> and <see cref="F:StardewValley.GameData.Buffs.BuffData.MaxDuration" /> will be selected for each buff.</summary>
	[ContentSerializer(Optional = true)]
	public int MaxDuration = -1;

	/// <summary>The texture to load for the buff icon.</summary>
	public string IconTexture;

	/// <summary>The sprite index for the buff icon within the <see cref="F:StardewValley.GameData.Buffs.BuffData.IconTexture" />.</summary>
	public int IconSpriteIndex;

	/// <summary>The custom buff attributes to apply, if any.</summary>
	[ContentSerializer(Optional = true)]
	public BuffAttributesData Effects;

	/// <summary>The trigger actions to run when the buff is applied to the player.</summary>
	[ContentSerializer(Optional = true)]
	public List<string> ActionsOnApply;

	/// <summary>Custom fields ignored by the base game, for use by mods.</summary>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> CustomFields;
}
