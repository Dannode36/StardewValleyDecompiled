using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Weapons;

/// <summary>As part of <see cref="T:StardewValley.GameData.Weapons.WeaponData" />, a projectile fired when the weapon is used.</summary>
public class WeaponProjectile
{
	/// <summary>A key which uniquely identifies the projectile within its weapon's data. The ID should only contain alphanumeric/underscore/dot characters. For custom projectiles, this should be prefixed with your mod ID like <c>Example.ModId_ProjectileId.</c></summary>
	public string Id;

	/// <summary>The amount of damage caused when they hit a monster.</summary>
	[ContentSerializer(Optional = true)]
	public int Damage = 10;

	/// <summary>Whether the projectile explodes when it collides with something.</summary>
	[ContentSerializer(Optional = true)]
	public bool Explodes;

	/// <summary>The number of times the projectile can bounce off walls before being destroyed.</summary>
	[ContentSerializer(Optional = true)]
	public int Bounces;

	/// <summary>The maximum tile distance the projectile can travel.</summary>
	[ContentSerializer(Optional = true)]
	public int MaxDistance = 4;

	/// <summary>The speed at which the projectile moves.</summary>
	[ContentSerializer(Optional = true)]
	public int Velocity = 10;

	/// <summary>The rotation velocity.</summary>
	[ContentSerializer(Optional = true)]
	public int RotationVelocity = 32;

	/// <summary>The length of the tail which trails behind the main projectile.</summary>
	[ContentSerializer(Optional = true)]
	public int TailLength = 1;

	/// <summary>The sound played when the projectile is fired.</summary>
	[ContentSerializer(Optional = true)]
	public string FireSound = "";

	/// <summary>The sound played when the projectile bounces off a wall.</summary>
	[ContentSerializer(Optional = true)]
	public string BounceSound = "";

	/// <summary>The sound played when the projectile collides with something.</summary>
	[ContentSerializer(Optional = true)]
	public string CollisionSound = "";

	/// <summary>The minimum value for a random offset applied to the direction of the project each time it's fired. If both fields are zero, it's always shot at the 90° angle matching the player's facing direction.</summary>
	[ContentSerializer(Optional = true)]
	public float MinAngleOffset;

	/// <summary>The maximum value for <see cref="F:StardewValley.GameData.Weapons.WeaponProjectile.MinAngleOffset" />.</summary>
	[ContentSerializer(Optional = true)]
	public float MaxAngleOffset;

	/// <summary>The sprite index in <c>TileSheets/Projectiles</c> to draw for this projectile.</summary>
	[ContentSerializer(Optional = true)]
	public int SpriteIndex = 11;

	/// <summary>The item to shoot. If set, this overrides <see cref="F:StardewValley.GameData.Weapons.WeaponProjectile.SpriteIndex" />.</summary>
	[ContentSerializer(Optional = true)]
	public GenericSpawnItemData Item;
}
