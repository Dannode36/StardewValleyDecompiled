using Netcode;
using StardewValley.GameData.Buffs;

namespace StardewValley.Buffs;

/// <summary>The combined buff attribute values applied to a player.</summary>
public class BuffEffects : INetObject<NetFields>
{
	/// <summary>The attributes which are added to the player's stats.</summary>
	private readonly NetFloat[] AdditiveFields;

	/// <summary>The attributes which are multiplied by the player's stats.</summary>
	private readonly NetFloat[] MultiplicativeFields;

	/// <summary>The buff to the player's combat skill level.</summary>
	public readonly NetFloat CombatLevel = new NetFloat(0f);

	/// <summary>The buff to the player's farming skill level.</summary>
	public readonly NetFloat FarmingLevel = new NetFloat(0f);

	/// <summary>The buff to the player's fishing skill level.</summary>
	public readonly NetFloat FishingLevel = new NetFloat(0f);

	/// <summary>The buff to the player's mining skill level.</summary>
	public readonly NetFloat MiningLevel = new NetFloat(0f);

	/// <summary>The buff to the player's luck skill level.</summary>
	public readonly NetFloat LuckLevel = new NetFloat(0f);

	/// <summary>The buff to the player's foraging skill level.</summary>
	public readonly NetFloat ForagingLevel = new NetFloat(0f);

	/// <summary>The buff to the player's max stamina.</summary>
	public readonly NetFloat MaxStamina = new NetFloat(0f);

	/// <summary>The buff to the player's magnetic radius.</summary>
	public readonly NetFloat MagneticRadius = new NetFloat(0f);

	/// <summary>The buff to the player's walk speed.</summary>
	public readonly NetFloat Speed = new NetFloat(0f);

	/// <summary>The buff to the player's defense.</summary>
	public readonly NetFloat Defense = new NetFloat(0f);

	/// <summary>The buff to the player's attack power.</summary>
	public readonly NetFloat Attack = new NetFloat(0f);

	/// <summary>The combined buff to the player's resistance to negative effects.</summary>
	public readonly NetFloat Immunity = new NetFloat(0f);

	/// <summary>The combined multiplier applied to the player's attack power.</summary>
	public readonly NetFloat AttackMultiplier = new NetFloat(0f);

	/// <summary>The combined multiplier applied to monster knockback when hit by the player's weapon.</summary>
	public readonly NetFloat KnockbackMultiplier = new NetFloat(0f);

	/// <summary>The combined multiplier applied to the player's weapon swing speed.</summary>
	public readonly NetFloat WeaponSpeedMultiplier = new NetFloat(0f);

	/// <summary>The combined multiplier applied to the player's critical hit chance.</summary>
	public readonly NetFloat CriticalChanceMultiplier = new NetFloat(0f);

	/// <summary>The combined multiplier applied to the player's critical hit damage.</summary>
	public readonly NetFloat CriticalPowerMultiplier = new NetFloat(0f);

	/// <summary>The combined multiplier applied to the player's weapon accuracy.</summary>
	public readonly NetFloat WeaponPrecisionMultiplier = new NetFloat(0f);

	public NetFields NetFields { get; } = new NetFields("BuffEffects");


	/// <summary>Construct an instance.</summary>
	public BuffEffects()
	{
		AdditiveFields = new NetFloat[12]
		{
			FarmingLevel, FishingLevel, MiningLevel, LuckLevel, ForagingLevel, MaxStamina, MagneticRadius, Speed, Defense, Attack,
			CombatLevel, Immunity
		};
		MultiplicativeFields = new NetFloat[6] { AttackMultiplier, KnockbackMultiplier, WeaponSpeedMultiplier, CriticalChanceMultiplier, CriticalPowerMultiplier, WeaponPrecisionMultiplier };
		NetFields.SetOwner(this).AddField(FarmingLevel, "FarmingLevel").AddField(FishingLevel, "FishingLevel")
			.AddField(MiningLevel, "MiningLevel")
			.AddField(LuckLevel, "LuckLevel")
			.AddField(ForagingLevel, "ForagingLevel")
			.AddField(MaxStamina, "MaxStamina")
			.AddField(MagneticRadius, "MagneticRadius")
			.AddField(Speed, "Speed")
			.AddField(Defense, "Defense")
			.AddField(Attack, "Attack")
			.AddField(CombatLevel, "CombatLevel")
			.AddField(Immunity, "Immunity")
			.AddField(AttackMultiplier, "AttackMultiplier")
			.AddField(KnockbackMultiplier, "KnockbackMultiplier")
			.AddField(WeaponSpeedMultiplier, "WeaponSpeedMultiplier")
			.AddField(CriticalChanceMultiplier, "CriticalChanceMultiplier")
			.AddField(CriticalPowerMultiplier, "CriticalPowerMultiplier")
			.AddField(WeaponPrecisionMultiplier, "WeaponPrecisionMultiplier");
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="data">The initial attributes to copy from raw object data.</param>
	public BuffEffects(BuffAttributesData data)
		: this()
	{
		Add(data);
	}

	/// <summary>Add another buff's effects to the stats.</summary>
	/// <param name="other">The buff effects to add.</param>
	public void Add(BuffEffects other)
	{
		if (other != null)
		{
			for (int i = 0; i < AdditiveFields.Length; i++)
			{
				AdditiveFields[i].Value += other.AdditiveFields[i].Value;
			}
			for (int i = 0; i < MultiplicativeFields.Length; i++)
			{
				MultiplicativeFields[i].Value += other.MultiplicativeFields[i].Value;
			}
		}
	}

	/// <summary>Add buff effect data to the stats.</summary>
	/// <param name="data">The buff effect data to add.</param>
	public void Add(BuffAttributesData data)
	{
		if (data != null)
		{
			FarmingLevel.Value = data.FarmingLevel;
			FishingLevel.Value = data.FishingLevel;
			MiningLevel.Value = data.MiningLevel;
			LuckLevel.Value = data.LuckLevel;
			ForagingLevel.Value = data.ForagingLevel;
			MaxStamina.Value = data.MaxStamina;
			MagneticRadius.Value = data.MagneticRadius;
			Speed.Value = data.Speed;
			Defense.Value = data.Defense;
			Attack.Value = data.Attack;
		}
	}

	/// <summary>Get whether any stat has a non-zero value.</summary>
	public bool HasAnyValue()
	{
		NetFloat[] additiveFields = AdditiveFields;
		for (int i = 0; i < additiveFields.Length; i++)
		{
			if (additiveFields[i].Value != 0f)
			{
				return true;
			}
		}
		additiveFields = MultiplicativeFields;
		for (int i = 0; i < additiveFields.Length; i++)
		{
			if (additiveFields[i].Value != 0f)
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Remove all effects from the stats.</summary>
	public void Clear()
	{
		NetFloat[] additiveFields = AdditiveFields;
		for (int i = 0; i < additiveFields.Length; i++)
		{
			additiveFields[i].Value = 0f;
		}
		additiveFields = MultiplicativeFields;
		for (int i = 0; i < additiveFields.Length; i++)
		{
			additiveFields[i].Value = 0f;
		}
	}

	/// <summary>Get the main effects in the pre-1.6 <c>Data/ObjectInformation</c> format.</summary>
	/// <remarks>This is a specialized method and shouldn't be used by most code.</remarks>
	public string[] ToLegacyAttributeFormat()
	{
		return new string[13]
		{
			((int)FarmingLevel.Value).ToString(),
			((int)FishingLevel.Value).ToString(),
			((int)MiningLevel.Value).ToString(),
			"0",
			((int)LuckLevel.Value).ToString(),
			((int)ForagingLevel.Value).ToString(),
			"0",
			((int)MaxStamina.Value).ToString(),
			((int)MagneticRadius.Value).ToString(),
			Speed.Value.ToString(),
			((int)Defense.Value).ToString(),
			((int)Attack.Value).ToString(),
			""
		};
	}
}
