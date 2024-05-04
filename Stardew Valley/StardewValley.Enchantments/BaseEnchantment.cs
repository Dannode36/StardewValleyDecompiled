using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Buffs;
using StardewValley.Extensions;
using StardewValley.Monsters;
using StardewValley.Tools;

namespace StardewValley.Enchantments;

[XmlInclude(typeof(BaseWeaponEnchantment))]
[XmlInclude(typeof(ArtfulEnchantment))]
[XmlInclude(typeof(BugKillerEnchantment))]
[XmlInclude(typeof(CrusaderEnchantment))]
[XmlInclude(typeof(HaymakerEnchantment))]
[XmlInclude(typeof(MagicEnchantment))]
[XmlInclude(typeof(VampiricEnchantment))]
[XmlInclude(typeof(AxeEnchantment))]
[XmlInclude(typeof(HoeEnchantment))]
[XmlInclude(typeof(MilkPailEnchantment))]
[XmlInclude(typeof(PanEnchantment))]
[XmlInclude(typeof(PickaxeEnchantment))]
[XmlInclude(typeof(ShearsEnchantment))]
[XmlInclude(typeof(WateringCanEnchantment))]
[XmlInclude(typeof(ArchaeologistEnchantment))]
[XmlInclude(typeof(AutoHookEnchantment))]
[XmlInclude(typeof(BottomlessEnchantment))]
[XmlInclude(typeof(EfficientToolEnchantment))]
[XmlInclude(typeof(GenerousEnchantment))]
[XmlInclude(typeof(MasterEnchantment))]
[XmlInclude(typeof(PowerfulEnchantment))]
[XmlInclude(typeof(PreservingEnchantment))]
[XmlInclude(typeof(ReachingToolEnchantment))]
[XmlInclude(typeof(ShavingEnchantment))]
[XmlInclude(typeof(SwiftToolEnchantment))]
[XmlInclude(typeof(FisherEnchantment))]
[XmlInclude(typeof(AmethystEnchantment))]
[XmlInclude(typeof(AquamarineEnchantment))]
[XmlInclude(typeof(DiamondEnchantment))]
[XmlInclude(typeof(EmeraldEnchantment))]
[XmlInclude(typeof(JadeEnchantment))]
[XmlInclude(typeof(RubyEnchantment))]
[XmlInclude(typeof(TopazEnchantment))]
[XmlInclude(typeof(AttackEnchantment))]
[XmlInclude(typeof(DefenseEnchantment))]
[XmlInclude(typeof(SlimeSlayerEnchantment))]
[XmlInclude(typeof(CritEnchantment))]
[XmlInclude(typeof(WeaponSpeedEnchantment))]
[XmlInclude(typeof(CritPowerEnchantment))]
[XmlInclude(typeof(LightweightEnchantment))]
[XmlInclude(typeof(SlimeGathererEnchantment))]
[XmlInclude(typeof(GalaxySoulEnchantment))]
public class BaseEnchantment : INetObject<NetFields>
{
	[XmlIgnore]
	protected string _displayName;

	[XmlIgnore]
	protected bool _applied;

	[XmlIgnore]
	[InstancedStatic]
	public static bool hideEnchantmentName;

	[XmlIgnore]
	[InstancedStatic]
	public static bool hideSecondaryEnchantName;

	protected static List<BaseEnchantment> _enchantments;

	protected readonly NetInt level = new NetInt(1);

	[XmlIgnore]
	public NetFields NetFields { get; } = new NetFields("BaseEnchantment");


	[XmlElement("level")]
	public int Level
	{
		get
		{
			return level.Value;
		}
		set
		{
			level.Value = value;
		}
	}

	public BaseEnchantment()
	{
		InitializeNetFields();
	}

	public static BaseEnchantment GetEnchantmentFromItem(Item base_item, Item item)
	{
		if (base_item == null || (base_item is MeleeWeapon w && !w.isScythe()))
		{
			switch (item?.QualifiedItemId)
			{
			case "(O)896":
				if ((base_item as MeleeWeapon)?.isGalaxyWeapon() ?? false)
				{
					return new GalaxySoulEnchantment();
				}
				break;
			case "(O)60":
				return new EmeraldEnchantment();
			case "(O)62":
				return new AquamarineEnchantment();
			case "(O)64":
				return new RubyEnchantment();
			case "(O)66":
				return new AmethystEnchantment();
			case "(O)68":
				return new TopazEnchantment();
			case "(O)70":
				return new JadeEnchantment();
			case "(O)72":
				return new DiamondEnchantment();
			}
		}
		if (item?.QualifiedItemId == "(O)74")
		{
			return Utility.CreateRandom(Game1.stats.Get("timesEnchanted"), Game1.uniqueIDForThisGame, Game1.player.UniqueMultiplayerID).ChooseFrom(GetAvailableEnchantmentsForItem(base_item as Tool));
		}
		return null;
	}

	public static List<BaseEnchantment> GetAvailableEnchantmentsForItem(Tool item)
	{
		List<BaseEnchantment> item_enchantments = new List<BaseEnchantment>();
		if (item == null)
		{
			return GetAvailableEnchantments();
		}
		List<BaseEnchantment> enchantments = GetAvailableEnchantments();
		HashSet<Type> applied_enchantments = new HashSet<Type>();
		foreach (BaseEnchantment enchantment in item.enchantments)
		{
			applied_enchantments.Add(enchantment.GetType());
		}
		foreach (BaseEnchantment enchantment in enchantments)
		{
			if (enchantment.CanApplyTo(item) && !applied_enchantments.Contains(enchantment.GetType()))
			{
				item_enchantments.Add(enchantment);
			}
		}
		foreach (string previous_enchantment in item.previousEnchantments)
		{
			if (item_enchantments.Count <= 1)
			{
				break;
			}
			for (int i = 0; i < item_enchantments.Count; i++)
			{
				if (item_enchantments[i].GetName() == previous_enchantment)
				{
					item_enchantments.RemoveAt(i);
					break;
				}
			}
		}
		return item_enchantments;
	}

	public static List<BaseEnchantment> GetAvailableEnchantments()
	{
		if (_enchantments == null)
		{
			_enchantments = new List<BaseEnchantment>();
			_enchantments.Add(new ArtfulEnchantment());
			_enchantments.Add(new BugKillerEnchantment());
			_enchantments.Add(new VampiricEnchantment());
			_enchantments.Add(new CrusaderEnchantment());
			_enchantments.Add(new HaymakerEnchantment());
			_enchantments.Add(new PowerfulEnchantment());
			_enchantments.Add(new ReachingToolEnchantment());
			_enchantments.Add(new ShavingEnchantment());
			_enchantments.Add(new BottomlessEnchantment());
			_enchantments.Add(new GenerousEnchantment());
			_enchantments.Add(new ArchaeologistEnchantment());
			_enchantments.Add(new MasterEnchantment());
			_enchantments.Add(new AutoHookEnchantment());
			_enchantments.Add(new PreservingEnchantment());
			_enchantments.Add(new EfficientToolEnchantment());
			_enchantments.Add(new SwiftToolEnchantment());
			_enchantments.Add(new FisherEnchantment());
		}
		return _enchantments;
	}

	/// <summary>Reset cached enchantment data.</summary>
	public static void ResetEnchantments()
	{
		_enchantments = null;
	}

	public virtual bool IsForge()
	{
		return false;
	}

	public virtual bool IsSecondaryEnchantment()
	{
		return false;
	}

	public virtual void InitializeNetFields()
	{
		NetFields.SetOwner(this).AddField(level, "level");
	}

	public void OnEquip(Farmer farmer)
	{
		if (!_applied)
		{
			farmer.enchantments.Add(this);
			_applied = true;
			_OnEquip(farmer);
		}
	}

	public void OnUnequip(Farmer farmer)
	{
		if (_applied)
		{
			farmer.enchantments.Remove(this);
			_applied = false;
			_OnUnequip(farmer);
		}
	}

	protected virtual void _OnEquip(Farmer who)
	{
	}

	protected virtual void _OnUnequip(Farmer who)
	{
	}

	public void OnCalculateDamage(Monster monster, GameLocation location, Farmer who, ref int amount)
	{
		_OnDealDamage(monster, location, who, ref amount);
	}

	public void OnDealDamage(Monster monster, GameLocation location, Farmer who, ref int amount)
	{
		_OnDealDamage(monster, location, who, ref amount);
	}

	protected virtual void _OnDealDamage(Monster monster, GameLocation location, Farmer who, ref int amount)
	{
	}

	public void OnMonsterSlay(Monster m, GameLocation location, Farmer who)
	{
		_OnMonsterSlay(m, location, who);
	}

	protected virtual void _OnMonsterSlay(Monster m, GameLocation location, Farmer who)
	{
	}

	public virtual void AddEquipmentEffects(BuffEffects effects)
	{
	}

	public void OnCutWeed(Vector2 tile_location, GameLocation location, Farmer who)
	{
		_OnCutWeed(tile_location, location, who);
	}

	protected virtual void _OnCutWeed(Vector2 tile_location, GameLocation location, Farmer who)
	{
	}

	public virtual BaseEnchantment GetOne()
	{
		BaseEnchantment obj = Activator.CreateInstance(GetType()) as BaseEnchantment;
		obj.level.Value = level.Value;
		return obj;
	}

	public int GetLevel()
	{
		return level.Value;
	}

	public void SetLevel(Item item, int new_level)
	{
		if (new_level < 1)
		{
			new_level = 1;
		}
		else if (GetMaximumLevel() >= 0 && new_level > GetMaximumLevel())
		{
			new_level = GetMaximumLevel();
		}
		if (level.Value != new_level)
		{
			UnapplyTo(item);
			level.Value = new_level;
			ApplyTo(item);
		}
	}

	public virtual int GetMaximumLevel()
	{
		return -1;
	}

	public void ApplyTo(Item item, Farmer farmer = null)
	{
		_ApplyTo(item);
		if (IsItemCurrentlyEquipped(item, farmer))
		{
			OnEquip(farmer);
		}
	}

	protected virtual void _ApplyTo(Item item)
	{
	}

	public bool IsItemCurrentlyEquipped(Item item, Farmer farmer)
	{
		if (farmer == null)
		{
			return false;
		}
		return _IsCurrentlyEquipped(item, farmer);
	}

	protected virtual bool _IsCurrentlyEquipped(Item item, Farmer farmer)
	{
		return farmer.CurrentTool == item;
	}

	public void UnapplyTo(Item item, Farmer farmer = null)
	{
		_UnapplyTo(item);
		if (IsItemCurrentlyEquipped(item, farmer))
		{
			OnUnequip(farmer);
		}
	}

	protected virtual void _UnapplyTo(Item item)
	{
	}

	public virtual bool CanApplyTo(Item item)
	{
		return true;
	}

	public string GetDisplayName()
	{
		if (_displayName == null)
		{
			_displayName = Game1.content.LoadStringReturnNullIfNotFound("Strings\\EnchantmentNames:" + GetName());
			if (_displayName == null)
			{
				_displayName = GetName();
			}
		}
		return _displayName;
	}

	public virtual string GetName()
	{
		return "Unknown Enchantment";
	}

	public virtual bool ShouldBeDisplayed()
	{
		return true;
	}
}
