using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Weapons;

/// <summary>The metadata for a weapon that can be used by players.</summary>
public class WeaponData
{
	/// <summary>The internal weapon name.</summary>
	public string Name;

	/// <summary>A tokenizable string for the weapon's translated display name.</summary>
	public string DisplayName;

	/// <summary>A tokenizable string for the weapon's translated description.</summary>
	public string Description;

	/// <summary>The minimum base damage caused by hitting a monster with this weapon.</summary>
	public int MinDamage;

	/// <summary>The maximum base damage caused by hitting a monster with this weapon.</summary>
	public int MaxDamage;

	/// <summary>How far the target is pushed when hit, as a multiplier relative to a base weapon like the Rusty Sword (e.g. 1.5 for 150% of Rusty Sword's weight).</summary>
	[ContentSerializer(Optional = true)]
	public float Knockback = 1f;

	/// <summary>How fast the player can swing the weapon. Each point of speed is worth 40ms of swing time relative to 0. This stacks with the player's weapon speed.</summary>
	[ContentSerializer(Optional = true)]
	public int Speed;

	/// <summary>Reduces the chance that a strike will miss.</summary>
	[ContentSerializer(Optional = true)]
	public int Precision;

	/// <summary>Reduces damage received by the player.</summary>
	[ContentSerializer(Optional = true)]
	public int Defense;

	/// <summary>The weapon type. One of <c>0</c> (stabbing sword), <c>1</c> (dagger), <c>2</c> (club or hammer), or <c>3</c> (slashing sword).</summary>
	public int Type;

	/// <summary>The base mine level used to determine when this weapon appears in mine containers.</summary>
	[ContentSerializer(Optional = true)]
	public int MineBaseLevel = -1;

	/// <summary>The min mine level used to determine when this weapon appears in mine containers.</summary>
	[ContentSerializer(Optional = true)]
	public int MineMinLevel = -1;

	/// <summary>Slightly increases the area of effect.</summary>
	[ContentSerializer(Optional = true)]
	public int AreaOfEffect;

	/// <summary>The chance of a critical hit, as a decimal value between 0 (never) and 1 (always).</summary>
	[ContentSerializer(Optional = true)]
	public float CritChance = 0.02f;

	/// <summary>A multiplier applied to the base damage for a critical hit.</summary>
	[ContentSerializer(Optional = true)]
	public float CritMultiplier = 3f;

	/// <summary>Whether the player can lose this weapon when they die.</summary>
	[ContentSerializer(Optional = true)]
	public bool CanBeLostOnDeath = true;

	/// <summary>The asset name for the texture containing the weapon's sprite.</summary>
	public string Texture;

	/// <summary>The index within the <see cref="F:StardewValley.GameData.Weapons.WeaponData.Texture" /> for the weapon sprite, where 0 is the top-left icon.</summary>
	public int SpriteIndex;

	/// <summary>The projectiles fired when the weapon is used, if any. The continue along their path until they hit a monster and cause damage. One projectile will fire for each entry in the list. This doesn't apply for slingshots, which have hardcoded projectile logic.</summary>
	[ContentSerializer(Optional = true)]
	public List<WeaponProjectile> Projectiles;

	/// <summary>Custom fields ignored by the base game, for use by mods.</summary>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> CustomFields;
}
