using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.BellsAndWhistles;
using StardewValley.Buffs;
using StardewValley.Characters;
using StardewValley.Constants;
using StardewValley.Delegates;
using StardewValley.Enchantments;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.Crops;
using StardewValley.GameData.FarmAnimals;
using StardewValley.GameData.Fences;
using StardewValley.GameData.LocationContexts;
using StardewValley.GameData.Machines;
using StardewValley.GameData.Objects;
using StardewValley.GameData.WildTrees;
using StardewValley.Internal;
using StardewValley.Inventories;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Network.NetEvents;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.TokenizableStrings;
using StardewValley.Tools;
using xTile.Dimensions;

namespace StardewValley;

[XmlInclude(typeof(BreakableContainer))]
[XmlInclude(typeof(Cask))]
[XmlInclude(typeof(Chest))]
[XmlInclude(typeof(ColoredObject))]
[XmlInclude(typeof(CrabPot))]
[XmlInclude(typeof(Fence))]
[XmlInclude(typeof(Furniture))]
[XmlInclude(typeof(IndoorPot))]
[XmlInclude(typeof(ItemPedestal))]
[XmlInclude(typeof(Mannequin))]
[XmlInclude(typeof(MiniJukebox))]
[XmlInclude(typeof(Phone))]
[XmlInclude(typeof(Sign))]
[XmlInclude(typeof(Torch))]
[XmlInclude(typeof(Trinket))]
[XmlInclude(typeof(Wallpaper))]
[XmlInclude(typeof(WoodChipper))]
[XmlInclude(typeof(Workbench))]
public class Object : Item
{
	public enum PreserveType
	{
		Wine,
		Jelly,
		Pickle,
		Juice,
		Roe,
		AgedRoe,
		Honey,
		Bait,
		DriedFruit,
		DriedMushroom,
		SmokedFish
	}

	public const int wood = 388;

	public const int stone = 390;

	public const int copper = 378;

	public const int iron = 380;

	public const int coal = 382;

	public const int gold = 384;

	public const int iridium = 386;

	public const string artifactSpotID = "590";

	public const string hayID = "178";

	public const string iridiumBarID = "337";

	public const string woodID = "388";

	public const string stoneID = "390";

	public const string copperID = "378";

	public const string ironID = "380";

	public const string coalID = "382";

	public const string goldID = "384";

	public const string iridiumID = "386";

	public const string amethystClusterID = "66";

	public const string aquamarineID = "62";

	public const string bobberID = "133";

	public const string caveCarrotID = "78";

	public const string diamondID = "72";

	public const string emeraldID = "60";

	public const string prismaticShardID = "74";

	public const string quartzID = "80";

	public const string rubyID = "64";

	public const string sapphireID = "70";

	public const string stardropID = "434";

	public const string topazID = "68";

	public const string artifactSpotQID = "(O)590";

	public const string hayQID = "(O)178";

	public const string copperBarQID = "(O)334";

	public const string ironBarQID = "(O)335";

	public const string goldBarQID = "(O)336";

	public const string iridiumBarQID = "(O)337";

	public const string woodQID = "(O)388";

	public const string stoneQID = "(O)390";

	public const string copperQID = "(O)378";

	public const string ironQID = "(O)380";

	public const string coalQID = "(O)382";

	public const string goldQID = "(O)384";

	public const string iridiumQID = "(O)386";

	public const string amethystClusterQID = "(O)66";

	public const string aquamarineQID = "(O)62";

	public const string caveCarrotQID = "(O)78";

	public const string diamondQID = "(O)72";

	public const string emeraldQID = "(O)60";

	public const string prismaticShardQID = "(O)74";

	public const string rubyQID = "(O)64";

	public const string sapphireQID = "(O)70";

	public const string stardropQID = "(O)434";

	public const string topazQID = "(O)68";

	public const int inedible = -300;

	public const int GreensCategory = -81;

	public const int GemCategory = -2;

	public const int VegetableCategory = -75;

	public const int FishCategory = -4;

	public const int EggCategory = -5;

	public const int MilkCategory = -6;

	public const int CookingCategory = -7;

	public const int CraftingCategory = -8;

	public const int BigCraftableCategory = -9;

	public const int FruitsCategory = -79;

	public const int SeedsCategory = -74;

	public const int mineralsCategory = -12;

	public const int flowersCategory = -80;

	public const int meatCategory = -14;

	public const int metalResources = -15;

	public const int buildingResources = -16;

	public const int sellAtPierres = -17;

	public const int sellAtPierresAndMarnies = -18;

	public const int fertilizerCategory = -19;

	public const int junkCategory = -20;

	public const int baitCategory = -21;

	public const int tackleCategory = -22;

	public const int sellAtFishShopCategory = -23;

	public const int furnitureCategory = -24;

	public const int ingredientsCategory = -25;

	public const int artisanGoodsCategory = -26;

	public const int syrupCategory = -27;

	public const int monsterLootCategory = -28;

	public const int equipmentCategory = -29;

	public const int clothingCategorySortValue = -94;

	public const int hatCategory = -95;

	public const int ringCategory = -96;

	public const int weaponCategory = -98;

	public const int bootsCategory = -97;

	public const int toolCategory = -99;

	public const int clothingCategory = -100;

	public const int trinketCategory = -101;

	public const int booksCategory = -102;

	public const int skillBooksCategory = -103;

	/// <summary>The category for spawned twigs, weeds, and stones which spawn randomly in a location.</summary>
	public const int litterCategory = -999;

	public const int WildHorseradishIndex = 16;

	public const int LeekIndex = 20;

	public const int DandelionIndex = 22;

	public const int HandCursorIndex = 26;

	public const int WaterAnimationIndex = 28;

	public const int LumberIndex = 30;

	public const int mineStoneGrey1Index = 32;

	public const int mineStoneBlue1Index = 34;

	public const int mineStoneBlue2Index = 36;

	public const int mineStoneGrey2Index = 38;

	public const int mineStoneBrown1Index = 40;

	public const int mineStoneBrown2Index = 42;

	public const int mineStonePurpleIndex = 44;

	public const int mineStoneMysticIndex = 46;

	public const int mineStoneSnow1 = 48;

	public const int mineStoneSnow3 = 52;

	public const int mineStoneRed1Index = 56;

	public const int mineStoneRed2Index = 58;

	public const int emeraldIndex = 60;

	public const int aquamarineIndex = 62;

	public const int rubyIndex = 64;

	public const int amethystClusterIndex = 66;

	public const int topazIndex = 68;

	public const int sapphireIndex = 70;

	public const int diamondIndex = 72;

	public const int prismaticShardIndex = 74;

	public const int stardrop = 434;

	/// <summary>The <see cref="F:StardewValley.Object.preservedParentSheetIndex" /> value for Wild Honey.</summary>
	public const string WildHoneyPreservedId = "-1";

	public const int lowQuality = 0;

	public const int medQuality = 1;

	public const int highQuality = 2;

	public const int bestQuality = 4;

	public const int fragility_Removable = 0;

	public const int fragility_Delicate = 1;

	public const int fragility_Indestructable = 2;

	public const int spriteSheetTileSize = 16;

	public const float wobbleAmountWhenWorking = 10f;

	/// <summary>The backing field for <see cref="P:StardewValley.Object.TileLocation" />.</summary>
	/// <remarks>When changing this value, most code should use <see cref="P:StardewValley.Object.TileLocation" /> instead so the bounding box is recalculated.</remarks>
	[XmlElement("tileLocation")]
	public readonly NetVector2 tileLocation = new NetVector2();

	[XmlElement("owner")]
	public readonly NetLong owner = new NetLong();

	[XmlElement("type")]
	public readonly NetString type = new NetString();

	[XmlElement("canBeSetDown")]
	public readonly NetBool canBeSetDown = new NetBool(value: false);

	[XmlElement("canBeGrabbed")]
	public readonly NetBool canBeGrabbed = new NetBool(value: true);

	[XmlElement("isSpawnedObject")]
	public readonly NetBool isSpawnedObject = new NetBool(value: false);

	[XmlElement("questItem")]
	public readonly NetBool questItem = new NetBool(value: false);

	[XmlElement("questId")]
	public readonly NetString questId = new NetString();

	[XmlElement("isOn")]
	public readonly NetBool isOn = new NetBool(value: true);

	[XmlElement("fragility")]
	public readonly NetInt fragility = new NetInt(0);

	[XmlElement("price")]
	public readonly NetInt price = new NetInt();

	[XmlElement("edibility")]
	public readonly NetInt edibility = new NetInt(-300);

	[XmlElement("bigCraftable")]
	public readonly NetBool bigCraftable = new NetBool();

	[XmlElement("setOutdoors")]
	public readonly NetBool setOutdoors = new NetBool();

	[XmlElement("setIndoors")]
	public readonly NetBool setIndoors = new NetBool();

	[XmlElement("readyForHarvest")]
	public readonly NetBool readyForHarvest = new NetBool();

	[XmlElement("showNextIndex")]
	public readonly NetBool showNextIndex = new NetBool();

	[XmlElement("flipped")]
	public readonly NetBool flipped = new NetBool();

	[XmlElement("isLamp")]
	public readonly NetBool isLamp = new NetBool();

	[XmlElement("heldObject")]
	public readonly NetRef<Object> heldObject = new NetRef<Object>();

	/// <summary>If this is a machine, the <see cref="F:StardewValley.GameData.Machines.MachineOutputRule.Id" /> value for the last rule which set the <see cref="F:StardewValley.Object.heldObject" /> value.</summary>
	[XmlElement("lastOutputRuleId")]
	public readonly NetString lastOutputRuleId = new NetString();

	/// <summary>If this is a machine, the last input item for which output was produced.</summary>
	[XmlElement("lastInputItem")]
	public readonly NetRef<Item> lastInputItem = new NetRef<Item>();

	[XmlElement("minutesUntilReady")]
	public readonly NetIntDelta minutesUntilReady = new NetIntDelta();

	[XmlElement("boundingBox")]
	public readonly NetRectangle boundingBox = new NetRectangle();

	public Vector2 scale;

	[XmlElement("uses")]
	public readonly NetInt uses = new NetInt();

	[XmlIgnore]
	private readonly NetRef<LightSource> netLightSource = new NetRef<LightSource>();

	[XmlIgnore]
	public bool isTemporarilyInvisible;

	[XmlIgnore]
	protected NetBool _destroyOvernight = new NetBool(value: false);

	[XmlIgnore]
	public bool shouldShowSign;

	/// <summary>If set, a custom buff to apply when the item is consumed, in addition to any buffs normally applied by the item.</summary>
	[XmlIgnore]
	public Func<Buff> customBuff;

	[XmlElement("signText")]
	public readonly NetString signText = new NetString();

	protected MachineEffects _machineAnimation;

	protected bool _machineAnimationLoop;

	protected int _machineAnimationIndex;

	protected int _machineAnimationFrame = -1;

	protected int _machineAnimationInterval;

	[XmlElement("orderData")]
	public readonly NetString orderData = new NetString();

	/// <summary>The inventory from which items are being auto-loaded, if any.</summary>
	/// <remarks>This is set during auto-loading, and unset immediately after the auto-load succeeds or fails.</remarks>
	[XmlIgnore]
	public static IInventory autoLoadFrom;

	[XmlIgnore]
	public int shakeTimer;

	[XmlIgnore]
	public int lastNoteBlockSoundTime;

	[XmlIgnore]
	public ICue internalSound;

	[XmlElement("preserve")]
	public readonly NetNullableEnum<PreserveType> preserve = new NetNullableEnum<PreserveType>();

	[XmlElement("preservedParentSheetIndex")]
	public readonly NetString preservedParentSheetIndex = new NetString();

	/// <summary>Obsolete. This is only kept to preserve data from old save files, and isn't synchronized in multiplayer. Use <see cref="F:StardewValley.Object.preservedParentSheetIndex" /> instead.</summary>
	[XmlElement("honeyType")]
	public string obsolete_honeyType;

	/// <summary>If set, a tokenizable string for the display name to use instead of the item data.</summary>
	/// <remarks>This can contain <c>%DISPLAY_NAME</c> for the display name from data, and <c>%PRESERVED_DISPLAY_NAME</c> for the display name of the preserved item (if applicable).</remarks>
	[XmlElement("displayNameFormat")]
	public string displayNameFormat;

	/// <summary>The cached value for <see cref="P:StardewValley.Object.DisplayName" />.</summary>
	[XmlIgnore]
	public string displayName;

	protected bool _hasHeldObject;

	protected bool _hasLightSource;

	public static int CurrentParsedItemCount;

	protected int health = 10;

	[XmlIgnore]
	public bool hovering;

	public bool destroyOvernight
	{
		get
		{
			return _destroyOvernight.Value;
		}
		set
		{
			_destroyOvernight.Value = value;
		}
	}

	[XmlIgnore]
	public LightSource lightSource
	{
		get
		{
			return netLightSource.Value;
		}
		set
		{
			netLightSource.Value = value;
		}
	}

	/// <summary>The location containing this object, if it's placed in the world.</summary>
	[XmlIgnore]
	public virtual GameLocation Location { get; set; }

	/// <summary>The item's tile location in the world.</summary>
	[XmlIgnore]
	public virtual Vector2 TileLocation
	{
		get
		{
			return tileLocation.Value;
		}
		set
		{
			if (tileLocation.Value != value)
			{
				tileLocation.Value = value;
				RecalculateBoundingBox();
			}
		}
	}

	[XmlIgnore]
	public string name
	{
		get
		{
			return netName.Value;
		}
		set
		{
			netName.Value = value;
		}
	}

	/// <inheritdoc />
	public override string TypeDefinitionId
	{
		get
		{
			if (!bigCraftable.Value)
			{
				return "(O)";
			}
			return "(BC)";
		}
	}

	/// <inheritdoc />
	[XmlIgnore]
	public override string DisplayName
	{
		get
		{
			displayName = loadDisplayName();
			if (orderData.Value == "QI_COOKING")
			{
				displayName = Game1.content.LoadString("Strings\\StringsFromCSFiles:Fresh_Prefix", displayName);
			}
			if ((bool)isRecipe)
			{
				string label = displayName;
				if (CraftingRecipe.craftingRecipes.TryGetValue(displayName, out var rawCraftingData))
				{
					string count = ArgUtility.SplitBySpaceAndGet(ArgUtility.Get(rawCraftingData.Split('/'), 2), 1);
					if (count != null)
					{
						label = label + " x" + count;
					}
				}
				return label + Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12657");
			}
			return displayName;
		}
	}

	/// <inheritdoc />
	[XmlIgnore]
	public override string Name
	{
		get
		{
			return name + (isRecipe ? " Recipe" : "");
		}
		set
		{
			name = value;
		}
	}

	[XmlIgnore]
	public string Type
	{
		get
		{
			return type;
		}
		set
		{
			type.Value = value;
		}
	}

	[XmlIgnore]
	public bool CanBeSetDown
	{
		get
		{
			return canBeSetDown;
		}
		set
		{
			canBeSetDown.Value = value;
		}
	}

	[XmlIgnore]
	public bool CanBeGrabbed
	{
		get
		{
			return canBeGrabbed;
		}
		set
		{
			canBeGrabbed.Value = value;
		}
	}

	[XmlIgnore]
	public bool IsOn
	{
		get
		{
			return isOn;
		}
		set
		{
			isOn.Value = value;
		}
	}

	[XmlIgnore]
	public bool IsSpawnedObject
	{
		get
		{
			return isSpawnedObject;
		}
		set
		{
			isSpawnedObject.Value = value;
		}
	}

	[XmlIgnore]
	public bool Flipped
	{
		get
		{
			return flipped;
		}
		set
		{
			flipped.Value = value;
		}
	}

	[XmlIgnore]
	public int Price
	{
		get
		{
			return price;
		}
		set
		{
			price.Value = value;
		}
	}

	[XmlIgnore]
	public int Edibility
	{
		get
		{
			return edibility;
		}
		set
		{
			edibility.Value = value;
		}
	}

	[XmlIgnore]
	public int Fragility
	{
		get
		{
			return fragility;
		}
		set
		{
			fragility.Value = value;
		}
	}

	[XmlIgnore]
	public Vector2 Scale
	{
		get
		{
			return scale;
		}
		set
		{
			scale = value;
		}
	}

	[XmlIgnore]
	public int MinutesUntilReady
	{
		get
		{
			return minutesUntilReady.Value;
		}
		set
		{
			minutesUntilReady.Value = value;
		}
	}

	/// <inheritdoc />
	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(tileLocation, "tileLocation").AddField(owner, "owner").AddField(type, "type")
			.AddField(canBeSetDown, "canBeSetDown")
			.AddField(canBeGrabbed, "canBeGrabbed")
			.AddField(isSpawnedObject, "isSpawnedObject")
			.AddField(questItem, "questItem")
			.AddField(questId, "questId")
			.AddField(isOn, "isOn")
			.AddField(fragility, "fragility")
			.AddField(price, "price")
			.AddField(edibility, "edibility")
			.AddField(uses, "uses")
			.AddField(bigCraftable, "bigCraftable")
			.AddField(setOutdoors, "setOutdoors")
			.AddField(setIndoors, "setIndoors")
			.AddField(readyForHarvest, "readyForHarvest")
			.AddField(showNextIndex, "showNextIndex")
			.AddField(flipped, "flipped")
			.AddField(isLamp, "isLamp")
			.AddField(heldObject, "heldObject")
			.AddField(lastInputItem, "lastInputItem")
			.AddField(lastOutputRuleId, "lastOutputRuleId")
			.AddField(minutesUntilReady, "minutesUntilReady")
			.AddField(boundingBox, "boundingBox")
			.AddField(preserve, "preserve")
			.AddField(preservedParentSheetIndex, "preservedParentSheetIndex")
			.AddField(netLightSource, "netLightSource")
			.AddField(orderData, "orderData")
			.AddField(_destroyOvernight, "_destroyOvernight")
			.AddField(signText, "signText");
		heldObject.fieldChangeVisibleEvent += delegate
		{
			_hasHeldObject = heldObject.Value != null;
		};
		netLightSource.fieldChangeVisibleEvent += delegate
		{
			_hasLightSource = netLightSource.Value != null;
		};
		bigCraftable.fieldChangeVisibleEvent += delegate
		{
			_qualifiedItemId = null;
		};
		signText.FilterStringEvent += Utility.FilterDirtyWords;
	}

	/// <summary>Construct an item with default data.</summary>
	public Object()
	{
	}

	/// <summary>Construct a <see cref="F:StardewValley.ItemRegistry.type_bigCraftable" />-type item.</summary>
	public Object(Vector2 tileLocation, string itemId, bool isRecipe = false)
		: this()
	{
		itemId = ValidateUnqualifiedItemId(itemId);
		base.isRecipe.Value = isRecipe;
		base.ItemId = itemId;
		canBeSetDown.Value = true;
		bigCraftable.Value = true;
		if (Game1.bigCraftableData.TryGetValue(itemId, out var data))
		{
			name = data.Name ?? ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId).InternalName;
			price.Value = data.Price;
			type.Value = "Crafting";
			base.Category = -9;
			setOutdoors.Value = data.CanBePlacedOutdoors;
			setIndoors.Value = data.CanBePlacedIndoors;
			fragility.Value = data.Fragility;
			isLamp.Value = data.IsLamp;
		}
		ResetParentSheetIndex();
		TileLocation = tileLocation;
		initializeLightSource(this.tileLocation.Value);
	}

	/// <summary>Construct a <see cref="F:StardewValley.ItemRegistry.type_object" />-type item.</summary>
	public Object(string itemId, int initialStack, bool isRecipe = false, int price = -1, int quality = 0)
		: this()
	{
		itemId = ValidateUnqualifiedItemId(itemId);
		stack.Value = initialStack;
		base.isRecipe.Value = isRecipe;
		base.quality.Value = quality;
		base.ItemId = itemId;
		ResetParentSheetIndex();
		if (Game1.objectData.TryGetValue(itemId, out var data))
		{
			name = data.Name ?? ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId).InternalName;
			this.price.Value = data.Price;
			edibility.Value = data.Edibility;
			type.Value = data.Type;
			base.Category = data.Category;
		}
		if (price != -1)
		{
			this.price.Value = price;
		}
		if (name == null)
		{
			name = "Error Item";
		}
		canBeSetDown.Value = true;
		canBeGrabbed.Value = true;
		isSpawnedObject.Value = false;
		if (Game1.random.NextBool() && Utility.IsLegacyIdAbove(itemId, 52) && !Utility.IsLegacyIdBetween(itemId, 8, 15) && !Utility.IsLegacyIdBetween(itemId, 384, 391))
		{
			flipped.Value = true;
		}
		if (base.QualifiedItemId == "(O)463" || base.QualifiedItemId == "(O)464")
		{
			scale = new Vector2(1f, 1f);
		}
		if (itemId == "449" || IsWeeds() || IsTwig())
		{
			fragility.Value = 2;
		}
		else if (name.Contains("Fence"))
		{
			scale = new Vector2(10f, 0f);
		}
		else if (IsBreakableStone())
		{
			switch (itemId)
			{
			case "8":
				minutesUntilReady.Value = 4;
				break;
			case "10":
				minutesUntilReady.Value = 8;
				break;
			case "12":
				minutesUntilReady.Value = 16;
				break;
			case "14":
				minutesUntilReady.Value = 12;
				break;
			case "25":
				minutesUntilReady.Value = 8;
				break;
			default:
				minutesUntilReady.Value = 1;
				break;
			}
		}
		if (base.Category == -22)
		{
			scale.Y = 1f;
		}
	}

	/// <summary>Change the item's ID and parent sheet index without changing any other item data.</summary>
	/// <param name="spriteIndex">The new parent sheet index and item ID to set.</param>
	[Obsolete("This is used for specialized game behavior and only supports vanilla objects. New code should place a new object instance instead.")]
	public virtual void SetIdAndSprite(int spriteIndex)
	{
		base.ParentSheetIndex = spriteIndex;
		base.ItemId = spriteIndex.ToString();
	}

	/// <summary>Recalculate the item's bounding box based on its current position.</summary>
	/// <remarks>This is only needed in cases where the position was moved manually instead of using the <see cref="P:StardewValley.Object.TileLocation" /> property.</remarks>
	public virtual void RecalculateBoundingBox()
	{
		Vector2 tile = TileLocation;
		boundingBox.Value = new Microsoft.Xna.Framework.Rectangle((int)tile.X * 64, (int)tile.Y * 64, 64, 64);
	}

	public virtual bool IsHeldOverHead()
	{
		return true;
	}

	protected override void _PopulateContextTags(HashSet<string> tags)
	{
		base._PopulateContextTags(tags);
		if (orderData.Value == "QI_COOKING")
		{
			tags.Add("quality_qi");
		}
		if (preserve != null && preserve.Value.HasValue)
		{
			switch (preserve.Value)
			{
			case PreserveType.Honey:
				tags.Add("honey_item");
				break;
			case PreserveType.Jelly:
				tags.Add("jelly_item");
				break;
			case PreserveType.Juice:
				tags.Add("juice_item");
				break;
			case PreserveType.Wine:
				tags.Add("wine_item");
				break;
			case PreserveType.Pickle:
				tags.Add("pickle_item");
				break;
			}
		}
		if (preservedParentSheetIndex.Value != null)
		{
			tags.Add("preserve_sheet_index_" + ItemContextTagManager.SanitizeContextTag(preservedParentSheetIndex.Value));
		}
	}

	/// <summary>Get the translated display name for the item, excluding metadata like "(Recipe)".</summary>
	/// <remarks>Most code should use <see cref="P:StardewValley.Object.DisplayName" /> instead, which caches the value.</remarks>
	protected virtual string loadDisplayName()
	{
		string baseName = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId).DisplayName;
		string preservedName = ((preservedParentSheetIndex.Value != null) ? ItemRegistry.GetDataOrErrorItem("(O)" + preservedParentSheetIndex).DisplayName : null);
		if (displayNameFormat != null)
		{
			string result = TokenParser.ParseText(displayNameFormat);
			if (result.Contains('%'))
			{
				result = result.Replace("%DISPLAY_NAME", baseName).Replace("%PRESERVED_DISPLAY_NAME", preservedName);
			}
			return result;
		}
		switch (preserve.Value)
		{
		case PreserveType.Honey:
			if (preservedName == null)
			{
				return baseName;
			}
			if (preservedParentSheetIndex.Value == "-1")
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12750");
			}
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12760", preservedName);
		case PreserveType.Wine:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12730", preservedName);
		case PreserveType.Jelly:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12739", preservedName);
		case PreserveType.Pickle:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12735", preservedName);
		case PreserveType.Juice:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12726", preservedName);
		case PreserveType.Roe:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Roe_DisplayName", preservedName?.TrimEnd('鱼'));
		case PreserveType.AgedRoe:
			if (preservedParentSheetIndex.Value != null)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:AgedRoe_DisplayName", preservedName?.TrimEnd('鱼'));
			}
			break;
		case PreserveType.Bait:
			return Game1.content.LoadString("Strings\\1_6_Strings:SpecificBait_DisplayName", preservedName);
		case PreserveType.DriedFruit:
		case PreserveType.DriedMushroom:
			return Lexicon.makePlural(Game1.content.LoadString("Strings\\1_6_Strings:DriedFruit_DisplayName", preservedName));
		case PreserveType.SmokedFish:
			return Game1.content.LoadString("Strings\\1_6_Strings:SmokedFish_DisplayName", preservedName);
		}
		return baseName;
	}

	public Vector2 getLocalPosition(xTile.Dimensions.Rectangle viewport)
	{
		return new Vector2(tileLocation.X * 64f - (float)viewport.X, tileLocation.Y * 64f - (float)viewport.Y);
	}

	public static Microsoft.Xna.Framework.Rectangle getSourceRectForBigCraftable(int index)
	{
		return getSourceRectForBigCraftable(Game1.bigCraftableSpriteSheet, index);
	}

	public static Microsoft.Xna.Framework.Rectangle getSourceRectForBigCraftable(Texture2D texture, int index)
	{
		return new Microsoft.Xna.Framework.Rectangle(index % (texture.Width / 16) * 16, index * 16 / texture.Width * 16 * 2, 16, 32);
	}

	public virtual bool performToolAction(Tool t)
	{
		GameLocation location = Location;
		if (isTemporarilyInvisible)
		{
			return false;
		}
		if (base.QualifiedItemId == "(BC)165" && heldObject.Value is Chest chest && !chest.isEmpty())
		{
			chest.clearNulls();
			if (t != null && t.isHeavyHitter() && !(t is MeleeWeapon))
			{
				playNearbySoundAll("hammer");
				shakeTimer = 100;
			}
			return false;
		}
		if (t == null)
		{
			if (location.objects.TryGetValue(tileLocation.Value, out var tileObj) && tileObj.Equals(this))
			{
				if (location.farmers.Count > 0)
				{
					Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(4, 10), resource: false);
				}
				location.objects.Remove(tileLocation.Value);
			}
			return false;
		}
		if (IsBreakableStone() && t is Pickaxe)
		{
			int damage = t.upgradeLevel.Value + 1;
			if ((base.QualifiedItemId == "(O)12" && (int)t.upgradeLevel == 1) || ((base.QualifiedItemId == "(O)12" || base.QualifiedItemId == "(O)14") && (int)t.upgradeLevel == 0))
			{
				damage = 0;
				playNearbySoundAll("crafting");
			}
			MinutesUntilReady -= damage;
			if (MinutesUntilReady <= 0)
			{
				return true;
			}
			playNearbySoundAll("hammer");
			shakeTimer = 100;
			return false;
		}
		if (IsBreakableStone() && t is Pickaxe)
		{
			return false;
		}
		if (name.Equals("Boulder") && (t.upgradeLevel.Value < 4 || !(t is Pickaxe)))
		{
			if (t.isHeavyHitter())
			{
				playNearbySoundAll("hammer");
			}
			return false;
		}
		if (IsWeeds() && t.isHeavyHitter())
		{
			int damage = 1;
			if (t is MeleeWeapon && t.isScythe() && t.QualifiedItemId != "(W)47")
			{
				damage = 2;
			}
			if (shakeTimer <= 0)
			{
				minutesUntilReady.Value -= damage;
			}
			if (minutesUntilReady.Value <= 0)
			{
				if (!(base.QualifiedItemId == "(O)319") && !(base.QualifiedItemId == "(O)320") && !(base.QualifiedItemId == "(O)321") && t.getLastFarmerToUse() != null)
				{
					foreach (BaseEnchantment enchantment in t.getLastFarmerToUse().enchantments)
					{
						enchantment.OnCutWeed(tileLocation.Value, location, t.getLastFarmerToUse());
					}
				}
				cutWeed(t.getLastFarmerToUse());
				return true;
			}
			if (shakeTimer <= 0)
			{
				Game1.playSound("weed_cut");
				shakeTimer = 200;
				return false;
			}
		}
		else
		{
			if (IsTwig() && t is Axe)
			{
				fragility.Value = 2;
				playNearbySoundAll("axchop");
				location.debris.Add(new Debris(ItemRegistry.Create("(O)388"), tileLocation.Value * 64f));
				Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(4, 10), resource: false);
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(12, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f), Color.White, 8, Game1.random.NextBool(), 50f));
				return true;
			}
			if (name.Contains("SupplyCrate") && t.isHeavyHitter())
			{
				MinutesUntilReady -= (int)t.upgradeLevel + 1;
				if (MinutesUntilReady <= 0)
				{
					fragility.Value = 2;
					playNearbySoundAll("barrelBreak");
					Random r = Utility.CreateRandom(Game1.uniqueIDForThisGame, (double)tileLocation.X * 777.0, (double)tileLocation.Y * 7.0);
					int houseLevel = t.getLastFarmerToUse().HouseUpgradeLevel;
					int x = (int)tileLocation.X;
					int y = (int)tileLocation.Y;
					switch (houseLevel)
					{
					case 0:
						switch (r.Next(7))
						{
						case 0:
							Game1.createMultipleObjectDebris("(O)770", x, y, r.Next(3, 6), location);
							break;
						case 1:
							Game1.createMultipleObjectDebris("(O)371", x, y, r.Next(5, 8), location);
							break;
						case 2:
							Game1.createMultipleObjectDebris("(O)535", x, y, r.Next(2, 5), location);
							break;
						case 3:
							Game1.createMultipleObjectDebris("(O)241", x, y, r.Next(1, 3), location);
							break;
						case 4:
							Game1.createMultipleObjectDebris("(O)395", x, y, r.Next(1, 3), location);
							break;
						case 5:
							Game1.createMultipleObjectDebris("(O)286", x, y, r.Next(3, 6), location);
							break;
						default:
							Game1.createMultipleObjectDebris("(O)286", x, y, r.Next(3, 6), location);
							break;
						}
						break;
					case 1:
						switch (r.Next(10))
						{
						case 0:
							Game1.createMultipleObjectDebris("(O)770", x, y, r.Next(3, 6), location);
							break;
						case 1:
							Game1.createMultipleObjectDebris("(O)371", x, y, r.Next(5, 8), location);
							break;
						case 2:
							Game1.createMultipleObjectDebris("(O)749", x, y, r.Next(2, 5), location);
							break;
						case 3:
							Game1.createMultipleObjectDebris("(O)253", x, y, r.Next(1, 3), location);
							break;
						case 4:
							Game1.createMultipleObjectDebris("(O)237", x, y, r.Next(1, 3), location);
							break;
						case 5:
							Game1.createMultipleObjectDebris("(O)246", x, y, r.Next(4, 8), location);
							break;
						case 6:
							Game1.createMultipleObjectDebris("(O)247", x, y, r.Next(2, 5), location);
							break;
						case 7:
							Game1.createMultipleObjectDebris("(O)245", x, y, r.Next(4, 8), location);
							break;
						case 8:
							Game1.createMultipleObjectDebris("(O)287", x, y, r.Next(3, 6), location);
							break;
						default:
							Game1.createMultipleObjectDebris("MixedFlowerSeeds", x, y, r.Next(4, 6), location);
							break;
						}
						break;
					default:
						switch (r.Next(9))
						{
						case 0:
							Game1.createMultipleObjectDebris("(O)770", x, y, r.Next(3, 6), location);
							break;
						case 1:
							Game1.createMultipleObjectDebris("(O)920", x, y, r.Next(5, 8), location);
							break;
						case 2:
							Game1.createMultipleObjectDebris("(O)749", x, y, r.Next(2, 5), location);
							break;
						case 3:
							Game1.createMultipleObjectDebris("(O)253", x, y, r.Next(2, 4), location);
							break;
						case 4:
							Game1.createMultipleObjectDebris(r.Choose("(O)904", "(O)905"), x, y, r.Next(1, 3), location);
							break;
						case 5:
							Game1.createMultipleObjectDebris("(O)246", x, y, r.Next(4, 8), location);
							Game1.createMultipleObjectDebris("(O)247", x, y, r.Next(2, 5), location);
							Game1.createMultipleObjectDebris("(O)245", x, y, r.Next(4, 8), location);
							break;
						case 6:
							Game1.createMultipleObjectDebris("(O)275", x, y, 2, location);
							break;
						case 7:
							Game1.createMultipleObjectDebris("(O)288", x, y, r.Next(3, 6), location);
							break;
						default:
							Game1.createMultipleObjectDebris("MixedFlowerSeeds", x, y, r.Next(5, 6), location);
							break;
						}
						break;
					}
					Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(4, 10), resource: false);
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(12, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f), Color.White, 8, Game1.random.NextBool(), 50f));
					return true;
				}
				shakeTimer = 200;
				playNearbySoundAll("woodWhack");
				return false;
			}
		}
		if (base.QualifiedItemId == "(O)590" || base.QualifiedItemId == "(O)SeedSpot")
		{
			if (t is Hoe)
			{
				Random r = Utility.CreateDaySaveRandom((0f - tileLocation.X) * 7f, tileLocation.Y * 777f, Game1.netWorldState.Value.TreasureTotemsUsed * 777);
				t.getLastFarmerToUse().stats.Increment("ArtifactSpotsDug", 1);
				if (t.getLastFarmerToUse().stats.Get("ArtifactSpotsDug") > 2 && r.NextDouble() < 0.008 + ((!t.getLastFarmerToUse().mailReceived.Contains("DefenseBookDropped")) ? ((double)t.getLastFarmerToUse().stats.Get("ArtifactSpotsDug") * 0.002) : 0.005))
				{
					t.getLastFarmerToUse().mailReceived.Add("DefenseBookDropped");
					Vector2 position = TileLocation * 64f;
					Game1.createMultipleItemDebris(ItemRegistry.Create("(O)Book_Defense"), position, Utility.GetOppositeFacingDirection(t.getLastFarmerToUse().FacingDirection), location);
				}
				if (base.QualifiedItemId == "(O)SeedSpot")
				{
					Item raccoonSeedForCurrentTimeOfYear = Utility.getRaccoonSeedForCurrentTimeOfYear(t.getLastFarmerToUse(), r);
					Vector2 position = TileLocation * 64f;
					Game1.createMultipleItemDebris(raccoonSeedForCurrentTimeOfYear, position, Utility.GetOppositeFacingDirection(t.getLastFarmerToUse().FacingDirection), location);
				}
				else
				{
					location.digUpArtifactSpot((int)tileLocation.X, (int)tileLocation.Y, t.getLastFarmerToUse());
				}
				if (!location.terrainFeatures.ContainsKey(tileLocation.Value))
				{
					location.makeHoeDirt(tileLocation.Value, ignoreChecks: true);
				}
				playNearbySoundAll("hoeHit");
				location.objects.Remove(tileLocation.Value);
			}
			return false;
		}
		if ((bool)bigCraftable && !(t is MeleeWeapon) && t.isHeavyHitter() && ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId).IsErrorItem)
		{
			playNearbySoundAll("hammer");
			performRemoveAction();
			location.objects.Remove(tileLocation.Value);
			return false;
		}
		if ((int)fragility == 2)
		{
			return false;
		}
		if (Type == "Crafting" && !(t is MeleeWeapon) && t.isHeavyHitter())
		{
			if (t is Hoe && IsSprinkler())
			{
				return false;
			}
			playNearbySoundAll("hammer");
			if ((int)fragility == 1)
			{
				Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(3, 6), resource: false);
				Game1.createRadialDebris(location, 14, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(3, 6), resource: false);
				DelayedAction.functionAfterDelay(delegate
				{
					Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(2, 5), resource: false);
					Game1.createRadialDebris(location, 14, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(2, 5), resource: false);
				}, 80);
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(12, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f), Color.White, 8, Game1.random.NextBool(), 50f));
				performRemoveAction();
				location.objects.Remove(tileLocation.Value);
				return false;
			}
			if (IsTapper() && location.terrainFeatures.TryGetValue(tileLocation.Value, out var terrainFeature) && terrainFeature is Tree tree)
			{
				tree.tapped.Value = false;
			}
			if (base.QualifiedItemId == "(BC)254")
			{
				if (heldObject.Value != null)
				{
					ResetParentSheetIndex();
					location.debris.Add(new Debris(heldObject.Value, tileLocation.Value * 64f + new Vector2(32f, 32f)));
					heldObject.Value = null;
					return true;
				}
				return true;
			}
			if (base.QualifiedItemId == "(BC)21" && heldObject.Value != null)
			{
				location.debris.Add(new Debris(heldObject.Value, tileLocation.Value * 64f + new Vector2(32f, 32f)));
				heldObject.Value = null;
			}
			if (IsSprinkler() && heldObject.Value != null)
			{
				if (heldObject.Value.heldObject.Value != null)
				{
					Object value = heldObject.Value.heldObject.Value;
					Chest chest = value as Chest;
					if (chest != null)
					{
						chest.GetMutex().RequestLock(delegate
						{
							List<Item> list = new List<Item>(chest.Items);
							chest.Items.Clear();
							foreach (Item current in list)
							{
								if (current != null)
								{
									location.debris.Add(new Debris(current, tileLocation.Value * 64f + new Vector2(32f, 32f)));
								}
							}
							Object value2 = heldObject.Value;
							heldObject.Value = null;
							location.debris.Add(new Debris(value2, tileLocation.Value * 64f + new Vector2(32f, 32f)));
							chest.GetMutex().ReleaseLock();
						});
					}
					return false;
				}
				location.debris.Add(new Debris(heldObject.Value, tileLocation.Value * 64f + new Vector2(32f, 32f)));
				heldObject.Value = null;
				return false;
			}
			if (IsSprinkler() && base.SpecialVariable == 999999)
			{
				location.debris.Add(new Debris(ItemRegistry.Create("(O)93"), tileLocation.Value * 64f + new Vector2(32f, 32f)));
			}
			if (heldObject.Value != null && (bool)readyForHarvest)
			{
				location.debris.Add(new Debris(heldObject.Value, tileLocation.Value * 64f + new Vector2(32f, 32f)));
			}
			if (base.QualifiedItemId == "(BC)156")
			{
				ResetParentSheetIndex();
				heldObject.Value = null;
				minutesUntilReady.Value = -1;
			}
			if (name.Contains("Seasonal"))
			{
				base.ParentSheetIndex -= base.ParentSheetIndex % 4;
			}
			return true;
		}
		return false;
	}

	public virtual void cutWeed(Farmer who)
	{
		GameLocation location = Location;
		Color c = Color.Green;
		string sound = "cut";
		int animation = 50;
		fragility.Value = 2;
		string toDrop = null;
		if (Game1.random.NextBool())
		{
			toDrop = "771";
		}
		else if (Game1.random.NextDouble() < 0.05 + ((who.stats.Get("Book_WildSeeds") != 0) ? 0.04 : 0.0))
		{
			toDrop = "770";
		}
		else if (Game1.currentSeason == "summer" && Game1.random.NextDouble() < 0.05 + ((who.stats.Get("Book_WildSeeds") != 0) ? 0.04 : 0.0))
		{
			toDrop = "MixedFlowerSeeds";
		}
		if (name.Contains("GreenRainWeeds") && Game1.random.NextDouble() < 0.1)
		{
			toDrop = "Moss";
		}
		switch (base.QualifiedItemId)
		{
		case "(O)678":
			c = new Color(228, 109, 159);
			break;
		case "(O)679":
			c = new Color(253, 191, 46);
			break;
		case "(O)313":
		case "(O)314":
		case "(O)315":
			c = new Color(84, 101, 27);
			break;
		case "(O)316":
		case "(O)317":
		case "(O)318":
			c = new Color(109, 49, 196);
			break;
		case "(O)319":
			c = new Color(30, 216, 255);
			sound = "breakingGlass";
			animation = 47;
			playNearbySoundAll("drumkit2");
			toDrop = null;
			break;
		case "(O)320":
			c = new Color(175, 143, 255);
			sound = "breakingGlass";
			animation = 47;
			playNearbySoundAll("drumkit2");
			toDrop = null;
			break;
		case "(O)321":
			c = new Color(73, 255, 158);
			sound = "breakingGlass";
			animation = 47;
			playNearbySoundAll("drumkit2");
			toDrop = null;
			break;
		case "(O)792":
		case "(O)793":
		case "(O)794":
			toDrop = "770";
			break;
		case "(O)882":
		case "(O)883":
		case "(O)884":
			c = new Color(30, 97, 68);
			if (Game1.MasterPlayer.hasOrWillReceiveMail("islandNorthCaveOpened") && Game1.random.NextDouble() < 0.1 && !Game1.MasterPlayer.hasOrWillReceiveMail("gotMummifiedFrog"))
			{
				Game1.addMailForTomorrow("gotMummifiedFrog", noLetter: true, sendToEveryone: true);
				toDrop = "828";
			}
			else if (Game1.random.NextDouble() < 0.01)
			{
				toDrop = "828";
			}
			else if (Game1.random.NextDouble() < 0.08)
			{
				toDrop = "831";
			}
			break;
		case "GreenRainWeeds0":
		case "GreenRainWeeds1":
		case "GreenRainWeeds4":
			sound = "weed_cut";
			break;
		}
		if (sound.Equals("breakingGlass") && Game1.random.NextDouble() < 0.0025)
		{
			toDrop = "338";
		}
		playNearbySoundAll(sound);
		Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(animation, tileLocation.Value * 64f, c));
		Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(animation, tileLocation.Value * 64f + new Vector2(Game1.random.Next(-16, 16), Game1.random.Next(-48, 48)), c * 0.75f)
		{
			scale = 0.75f,
			flipped = true
		});
		Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(animation, tileLocation.Value * 64f + new Vector2(Game1.random.Next(-16, 16), Game1.random.Next(-48, 48)), c * 0.75f)
		{
			scale = 0.75f,
			delayBeforeAnimationStart = 50
		});
		Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(animation, tileLocation.Value * 64f + new Vector2(Game1.random.Next(-16, 16), Game1.random.Next(-48, 48)), c * 0.75f)
		{
			scale = 0.75f,
			flipped = true,
			delayBeforeAnimationStart = 100
		});
		if (!sound.Equals("breakingGlass"))
		{
			if (Game1.random.NextDouble() < 1E-05)
			{
				location.debris.Add(new Debris(ItemRegistry.Create("(H)40"), tileLocation.Value * 64f + new Vector2(32f, 32f)));
			}
			if (Game1.random.NextDouble() <= 0.01 && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS"))
			{
				location.debris.Add(new Debris(ItemRegistry.Create("(O)890"), tileLocation.Value * 64f + new Vector2(32f, 32f)));
			}
		}
		if (toDrop != null)
		{
			location.debris.Add(new Debris(new Object(toDrop, 1), tileLocation.Value * 64f + new Vector2(32f, 32f)));
		}
		if (Game1.random.NextDouble() < 0.02)
		{
			location.addJumperFrog(tileLocation.Value);
		}
		if (location.HasUnlockedAreaSecretNotes(who) && Game1.random.NextDouble() < 0.009)
		{
			Object o = location.tryToCreateUnseenSecretNote(who);
			if (o != null)
			{
				Game1.createItemDebris(o, new Vector2(tileLocation.X + 0.5f, tileLocation.Y + 0.75f) * 64f, Game1.player.FacingDirection, location);
			}
		}
	}

	public virtual bool isAnimalProduct()
	{
		if (base.Category != -18 && base.Category != -5 && base.Category != -6)
		{
			return base.QualifiedItemId == "(O)430";
		}
		return true;
	}

	public virtual bool onExplosion(Farmer who)
	{
		if (who == null)
		{
			return false;
		}
		GameLocation location = Location;
		if (IsWeeds())
		{
			fragility.Value = 0;
			cutWeed(who);
			location.removeObject(tileLocation.Value, showDestroyedObject: false);
		}
		if (IsTwig())
		{
			fragility.Value = 0;
			Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(4, 10), resource: false);
			location.debris.Add(new Debris(ItemRegistry.Create("(O)388"), tileLocation.Value * 64f));
		}
		if (IsBreakableStone())
		{
			fragility.Value = 0;
		}
		performRemoveAction();
		return true;
	}

	public virtual bool canBeShipped()
	{
		if (!bigCraftable && type != null && Type != "Quest" && canBeTrashed() && !(this is Furniture))
		{
			return !(this is Wallpaper);
		}
		return false;
	}

	public virtual void ApplySprinkler(Vector2 tile)
	{
		GameLocation location = Location;
		if (!(location.doesTileHavePropertyNoNull((int)tile.X, (int)tile.Y, "NoSprinklers", "Back") == "T") && location.terrainFeatures.TryGetValue(tile, out var terrainFeature) && terrainFeature is HoeDirt dirt && (int)dirt.state != 2)
		{
			dirt.state.Value = 1;
		}
	}

	public virtual void ApplySprinklerAnimation()
	{
		GameLocation location = Location;
		int radius = GetModifiedRadiusForSprinkler();
		int tileX = (int)tileLocation.X;
		int tileY = (int)tileLocation.Y;
		if (radius != 0)
		{
			if (radius == 1)
			{
				location.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 1984, 192, 192), 60f, 3, 100, tileLocation.Value * 64f + new Vector2(-64f, -64f), flicker: false, flipped: false)
				{
					color = Color.White * 0.4f,
					delayBeforeAnimationStart = Game1.random.Next(1000),
					id = tileX * 4000 + tileY
				});
			}
			else if (radius > 0)
			{
				float scale = (float)radius / 2f;
				location.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 2176, 320, 320), 60f, 4, 100, tileLocation.Value * 64f + new Vector2(32f, 32f) + new Vector2(-160f, -160f) * scale, flicker: false, flipped: false)
				{
					color = Color.White * 0.4f,
					delayBeforeAnimationStart = Game1.random.Next(1000),
					id = tileX * 4000 + tileY,
					scale = scale
				});
			}
		}
		else
		{
			int delay = Game1.random.Next(1000);
			location.temporarySprites.Add(new TemporaryAnimatedSprite(29, tileLocation.Value * 64f + new Vector2(0f, -48f), Color.White * 0.5f, 4, flipped: false, 60f, 100)
			{
				delayBeforeAnimationStart = delay,
				id = tileX * 4000 + tileY
			});
			location.temporarySprites.Add(new TemporaryAnimatedSprite(29, tileLocation.Value * 64f + new Vector2(48f, 0f), Color.White * 0.5f, 4, flipped: false, 60f, 100)
			{
				rotation = (float)Math.PI / 2f,
				delayBeforeAnimationStart = delay,
				id = tileX * 4000 + tileY
			});
			location.temporarySprites.Add(new TemporaryAnimatedSprite(29, tileLocation.Value * 64f + new Vector2(0f, 48f), Color.White * 0.5f, 4, flipped: false, 60f, 100)
			{
				rotation = (float)Math.PI,
				delayBeforeAnimationStart = delay,
				id = tileX * 4000 + tileY
			});
			location.temporarySprites.Add(new TemporaryAnimatedSprite(29, tileLocation.Value * 64f + new Vector2(-48f, 0f), Color.White * 0.5f, 4, flipped: false, 60f, 100)
			{
				rotation = 4.712389f,
				delayBeforeAnimationStart = delay,
				id = tileX * 4000 + tileY
			});
		}
	}

	public virtual List<Vector2> GetSprinklerTiles()
	{
		int radius = GetModifiedRadiusForSprinkler();
		if (radius == 0)
		{
			return Utility.getAdjacentTileLocations(tileLocation.Value);
		}
		if (radius > 0)
		{
			List<Vector2> tiles = new List<Vector2>();
			for (int x = (int)tileLocation.X - radius; (float)x <= tileLocation.X + (float)radius; x++)
			{
				for (int y = (int)tileLocation.Y - radius; (float)y <= tileLocation.Y + (float)radius; y++)
				{
					tiles.Add(new Vector2(x, y));
				}
			}
			return tiles;
		}
		return new List<Vector2>();
	}

	public virtual bool IsInSprinklerRangeBroadphase(Vector2 target)
	{
		int radius = GetModifiedRadiusForSprinkler();
		if (radius == 0)
		{
			radius = 1;
		}
		if (Math.Abs(target.X - TileLocation.X) <= (float)radius)
		{
			return Math.Abs(target.Y - TileLocation.Y) <= (float)radius;
		}
		return false;
	}

	public virtual void DayUpdate()
	{
		GameLocation location = Location;
		health = 10;
		if (IsSprinkler() && (!location.isOutdoors || !location.IsRainingHere()) && GetModifiedRadiusForSprinkler() >= 0)
		{
			location.postFarmEventOvernightActions.Add(delegate
			{
				if (!Game1.player.team.SpecialOrderRuleActive("NO_SPRINKLER"))
				{
					foreach (Vector2 current in GetSprinklerTiles())
					{
						ApplySprinkler(current);
					}
					ApplySprinklerAnimation();
				}
			});
		}
		MachineData machineData = GetMachineData();
		if (machineData != null)
		{
			if (machineData.ClearContentsOvernightCondition != null && GameStateQuery.CheckConditions(machineData.ClearContentsOvernightCondition, location, null, inputItem: lastInputItem.Value, targetItem: heldObject.Value))
			{
				ResetParentSheetIndex();
				heldObject.Value = null;
				readyForHarvest.Value = false;
				showNextIndex.Value = false;
				minutesUntilReady.Value = -1;
			}
			if (heldObject.Value == null && MachineDataUtility.TryGetMachineOutputRule(this, machineData, MachineOutputTrigger.DayUpdate, null, null, location, out var outputRule, out var _, out var _, out var _))
			{
				OutputMachine(machineData, outputRule, null, null, location, probe: false);
			}
		}
		switch (base.QualifiedItemId)
		{
		case "(BC)MushroomLog":
			if (Game1.IsRainingHere(location))
			{
				minutesUntilReady.Value -= Utility.CalculateMinutesUntilMorning(Game1.timeOfDay);
			}
			break;
		case "(BC)272":
			if (!(location is AnimalHouse ah))
			{
				break;
			}
			foreach (KeyValuePair<long, FarmAnimal> pair in ah.animals.Pairs)
			{
				pair.Value.pet(Game1.player, is_auto_pet: true);
			}
			break;
		case "(BC)StatueOfBlessings":
			showNextIndex.Value = false;
			break;
		case "(BC)165":
			if (!(location is AnimalHouse animalHouse))
			{
				break;
			}
			foreach (KeyValuePair<long, FarmAnimal> kvp in animalHouse.animals.Pairs)
			{
				if (kvp.Value.GetHarvestType() == FarmAnimalHarvestType.HarvestWithTool && kvp.Value.currentProduce.Value != null && heldObject.Value is Chest chest)
				{
					Object produce = ItemRegistry.Create<Object>("(O)" + kvp.Value.currentProduce.Value);
					produce.CanBeSetDown = false;
					produce.Quality = kvp.Value.produceQuality;
					if (kvp.Value.hasEatenAnimalCracker.Value)
					{
						produce.Stack = 2;
					}
					if (chest.addItem(produce) == null)
					{
						Utility.RecordAnimalProduce(kvp.Value, kvp.Value.currentProduce);
						kvp.Value.currentProduce.Value = null;
						kvp.Value.ReloadTextureIfNeeded();
						showNextIndex.Value = true;
					}
				}
			}
			break;
		case "(BC)156":
			if (MinutesUntilReady > 0 || heldObject.Value == null)
			{
				break;
			}
			if (location.canSlimeHatchHere())
			{
				GreenSlime slime = null;
				Vector2 v = new Vector2((int)tileLocation.X, (int)tileLocation.Y + 1) * 64f;
				switch (heldObject.Value.QualifiedItemId)
				{
				case "(O)680":
					slime = new GreenSlime(v, 0);
					break;
				case "(O)413":
					slime = new GreenSlime(v, 40);
					break;
				case "(O)437":
					slime = new GreenSlime(v, 80);
					break;
				case "(O)439":
					slime = new GreenSlime(v, 121);
					break;
				case "(O)857":
					slime = new GreenSlime(v, 121);
					slime.makeTigerSlime();
					break;
				}
				if (slime != null)
				{
					Game1.showGlobalMessage(slime.cute ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12689") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12691"));
					Vector2 openSpot = Utility.recursiveFindOpenTileForCharacter(slime, location, tileLocation.Value + new Vector2(0f, 1f), 10, allowOffMap: false);
					slime.setTilePosition((int)openSpot.X, (int)openSpot.Y);
					location.characters.Add(slime);
					ResetParentSheetIndex();
					heldObject.Value = null;
					minutesUntilReady.Value = -1;
				}
			}
			else
			{
				minutesUntilReady.Value = Utility.CalculateMinutesUntilMorning(Game1.timeOfDay);
				readyForHarvest.Value = false;
			}
			break;
		case "(BC)108":
		case "(BC)109":
		{
			ResetParentSheetIndex();
			Season season = location.GetSeason();
			if (Location.IsOutdoors && (season == Season.Winter || season == Season.Fall))
			{
				base.ParentSheetIndex = 109;
			}
			break;
		}
		case "(BC)104":
			minutesUntilReady.Value = (location.IsWinterHere() ? 9999 : (-1));
			break;
		case "(BC)164":
		{
			if (!(location is Town))
			{
				break;
			}
			if (Game1.random.NextDouble() < 0.9)
			{
				GameLocation manorHouse = Game1.RequireLocation("ManorHouse");
				if (manorHouse.CanItemBePlacedHere(new Vector2(22f, 6f)))
				{
					if (!Game1.player.hasOrWillReceiveMail("lewisStatue"))
					{
						Game1.mailbox.Add("lewisStatue");
					}
					rot();
					manorHouse.objects.Add(new Vector2(22f, 6f), ItemRegistry.Create<Object>("(BC)164"));
				}
				break;
			}
			GameLocation animalShop = Game1.RequireLocation("AnimalShop");
			if (animalShop.CanItemBePlacedHere(new Vector2(11f, 6f)))
			{
				if (!Game1.player.hasOrWillReceiveMail("lewisStatue"))
				{
					Game1.mailbox.Add("lewisStatue");
				}
				rot();
				animalShop.objects.Add(new Vector2(11f, 6f), ItemRegistry.Create<Object>("(BC)164"));
			}
			break;
		}
		case "(O)747":
		case "(O)748":
			destroyOvernight = true;
			break;
		case "(O)746":
			if (location.IsWinterHere())
			{
				rot();
			}
			break;
		case "(O)784":
		case "(O)785":
			if (Game1.dayOfMonth == 1 && !location.IsSpringHere() && (bool)location.isOutdoors)
			{
				base.ParentSheetIndex++;
			}
			break;
		case "(O)674":
		case "(O)675":
			if (Game1.dayOfMonth == 1 && location.IsSummerHere() && (bool)location.isOutdoors)
			{
				base.ParentSheetIndex += 2;
			}
			break;
		case "(O)676":
		case "(O)677":
			if (Game1.dayOfMonth == 1 && location.IsFallHere() && (bool)location.isOutdoors)
			{
				base.ParentSheetIndex += 2;
			}
			break;
		}
		if ((bool)bigCraftable && name.Contains("Seasonal"))
		{
			int baseIndex = base.ParentSheetIndex - base.ParentSheetIndex % 4;
			base.ParentSheetIndex = baseIndex + location.GetSeasonIndex();
		}
	}

	public virtual void rot()
	{
		Random r = Utility.CreateRandom((double)Game1.year * 999.0, Game1.dayOfMonth, Game1.seasonIndex);
		SetIdAndSprite(r.Choose(747, 748));
		price.Value = 0;
		quality.Value = 0;
		name = "Rotten Plant";
		displayName = null;
		lightSource = null;
		bigCraftable.Value = false;
	}

	public override void actionWhenBeingHeld(Farmer who)
	{
		GameLocation location = who.currentLocation;
		if (location != null)
		{
			if (Game1.eventUp && Game1.CurrentEvent != null && Game1.CurrentEvent.isFestival)
			{
				if (lightSource != null && location.hasLightSource((int)who.UniqueMultiplayerID))
				{
					location.removeLightSource((int)who.UniqueMultiplayerID);
				}
				base.actionWhenBeingHeld(who);
				return;
			}
			if (lightSource != null && (!bigCraftable || (bool)isLamp))
			{
				if (!location.hasLightSource((int)who.UniqueMultiplayerID))
				{
					location.sharedLights[(int)who.UniqueMultiplayerID] = new LightSource(lightSource.textureIndex, lightSource.position.Value, lightSource.radius.Value, lightSource.color.Value, (int)who.UniqueMultiplayerID, LightSource.LightContext.None, who.UniqueMultiplayerID);
				}
				location.repositionLightSource((int)who.UniqueMultiplayerID, who.Position + new Vector2(32f, -64f));
			}
		}
		base.actionWhenBeingHeld(who);
	}

	public override void actionWhenStopBeingHeld(Farmer who)
	{
		GameLocation location = who.currentLocation;
		if (lightSource != null && location != null && location.hasLightSource((int)who.UniqueMultiplayerID))
		{
			location.removeLightSource((int)who.UniqueMultiplayerID);
		}
		base.actionWhenStopBeingHeld(who);
	}

	public static void ConsumeInventoryItem(Farmer who, Item drop_in, int amount)
	{
		if (drop_in.ConsumeStack(amount) == null)
		{
			(autoLoadFrom ?? who.Items).RemoveButKeepEmptySlot(drop_in);
			autoLoadFrom?.RemoveEmptySlots();
		}
	}

	/// <summary>Try to add an item to the object (e.g. input for a machine, placed on a table, etc).</summary>
	/// <param name="dropInItem">The item to add.</param>
	/// <param name="probe">Whether to return whether the item would be accepted without making any changes.</param>
	/// <param name="who">The player adding the item.</param>
	/// <param name="returnFalseIfItemConsumed">Whether to return false if the item was accepted, but it was already deducted from the inventory.</param>
	/// <returns>Usually returns whether the item was accepted by the machine.</returns>
	public virtual bool performObjectDropInAction(Item dropInItem, bool probe, Farmer who, bool returnFalseIfItemConsumed = false)
	{
		if (isTemporarilyInvisible)
		{
			return false;
		}
		if (!(dropInItem is Object dropIn))
		{
			return false;
		}
		GameLocation location = Location;
		if (IsSprinkler())
		{
			if (heldObject.Value == null && (dropIn.QualifiedItemId == "(O)915" || dropIn.QualifiedItemId == "(O)913"))
			{
				if (probe)
				{
					return true;
				}
				if (location is MineShaft || (location is VolcanoDungeon && dropIn.QualifiedItemId == "(O)913"))
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"));
					return false;
				}
				Object attached_object = dropIn.getOne() as Object;
				if (attached_object?.QualifiedItemId == "(O)913" && attached_object.heldObject.Value == null)
				{
					Chest chest = new Chest();
					chest.SpecialChestType = Chest.SpecialChestTypes.Enricher;
					attached_object.heldObject.Value = chest;
				}
				location.playSound("axe");
				heldObject.Value = attached_object;
				minutesUntilReady.Value = -1;
				return true;
			}
			if (dropIn.QualifiedItemId == "(O)93" && base.SpecialVariable != 999999)
			{
				if (probe)
				{
					return true;
				}
				base.SpecialVariable = 999999;
				Game1.playSound("woodyStep");
				lightSource = new LightSource(4, new Vector2(tileLocation.X * 64f + 16f, tileLocation.Y * 64f + 16f), 1.25f, new Color(1, 1, 1) * 0.9f, (int)(tileLocation.X * 2000f + tileLocation.Y), LightSource.LightContext.None, 0L);
				return true;
			}
		}
		if (dropIn.QualifiedItemId == "(O)872" && autoLoadFrom == null && TryApplyFairyDust(probe))
		{
			return true;
		}
		MachineData machineData = GetMachineData();
		if (machineData != null)
		{
			if (heldObject.Value != null && !machineData.AllowLoadWhenFull)
			{
				return false;
			}
			if (probe && MinutesUntilReady > 0)
			{
				return false;
			}
			if (PlaceInMachine(machineData, dropInItem, probe, who))
			{
				if (returnFalseIfItemConsumed && !probe)
				{
					return false;
				}
				return true;
			}
			return false;
		}
		if (base.QualifiedItemId == "(BC)99" && dropIn.QualifiedItemId == "(O)178")
		{
			GameLocation rootLocation = location.GetRootLocation();
			if (rootLocation.GetHayCapacity() <= 0)
			{
				if (autoLoadFrom == null && !probe)
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\Buildings:NeedSilo"));
				}
				return false;
			}
			if (probe)
			{
				return true;
			}
			location.playSound("Ship");
			DelayedAction.playSoundAfterDelay("grassyStep", 100);
			if (dropIn.Stack == 0)
			{
				dropIn.Stack = 1;
			}
			int old = rootLocation.piecesOfHay;
			int numLeft = rootLocation.tryToAddHay(dropIn.Stack);
			int now = rootLocation.piecesOfHay;
			if (old <= 0 && now > 0)
			{
				showNextIndex.Value = true;
			}
			else if (now <= 0)
			{
				showNextIndex.Value = false;
			}
			dropIn.Stack = numLeft;
			if (numLeft <= 0)
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Update the machine for the effects of fairy dust, if applicable.</summary>
	/// <param name="probe">Whether the game is only checking whether fairy dust would be accepted.</param>
	/// <returns>Returns whether the machine was updated (or if <paramref name="probe" /> is true, whether it would have been updated).</returns>
	public virtual bool TryApplyFairyDust(bool probe = false)
	{
		if (MinutesUntilReady > 0 && (GetMachineData()?.AllowFairyDust ?? false))
		{
			if (!probe)
			{
				Utility.addSprinklesToLocation(Location, (int)tileLocation.X, (int)tileLocation.Y, 1, 2, 400, 40, Color.White);
				Game1.playSound("yoba");
				MinutesUntilReady = 10;
				DelayedAction.functionAfterDelay(delegate
				{
					minutesElapsed(10);
				}, 50);
			}
			return true;
		}
		return false;
	}

	/// <summary>Get the output item to produce for a Solar Panel.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.MachineOutputDelegate" />
	public static Item OutputSolarPanel(Object machine, Item inputItem, bool probe, MachineItemOutput outputData, out int? overrideMinutesUntilReady)
	{
		int minutes = machine.MinutesUntilReady;
		GameLocation location = machine.Location;
		Object held_object = machine.heldObject.Value;
		if (held_object == null)
		{
			held_object = ItemRegistry.Create<Object>("(O)787");
			held_object.CanBeSetDown = false;
			minutes = Utility.CalculateMinutesUntilMorning(Game1.timeOfDay, 7);
		}
		if (minutes > 0 && location.IsOutdoors && !location.IsRainingHere())
		{
			minutes = Math.Max(0, minutes - 2400);
		}
		overrideMinutesUntilReady = ((minutes != machine.MinutesUntilReady) ? new int?(minutes) : null);
		return held_object;
	}

	/// <summary>Get the output item to produce for a Statue of Endless Fortune.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.MachineOutputDelegate" />
	public static Item OutputStatueOfEndlessFortune(Object machine, Item inputItem, bool probe, MachineItemOutput outputData, out int? overrideMinutesUntilReady)
	{
		overrideMinutesUntilReady = null;
		Item item = Utility.getTodaysBirthdayNPC()?.getFavoriteItem();
		if (item != null)
		{
			return item;
		}
		string index = "80";
		switch (Game1.random.Next(4))
		{
		case 0:
			index = "72";
			break;
		case 1:
			index = "337";
			break;
		case 2:
			index = "749";
			break;
		case 3:
			index = "336";
			break;
		}
		return new Object(index, 1);
	}

	/// <summary>Get the output item to produce for a Deconstructor.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.MachineOutputDelegate" />
	public static Item OutputDeconstructor(Object machine, Item inputItem, bool probe, MachineItemOutput outputData, out int? overrideMinutesUntilReady)
	{
		overrideMinutesUntilReady = null;
		if (!inputItem.HasTypeObject() && !inputItem.HasTypeBigCraftable())
		{
			return null;
		}
		if (!CraftingRecipe.craftingRecipes.TryGetValue(inputItem.Name, out var recipe))
		{
			return null;
		}
		string[] fields = recipe.Split('/');
		if (ArgUtility.SplitBySpace(ArgUtility.Get(fields, 2)).Length > 1)
		{
			return null;
		}
		if (inputItem.QualifiedItemId == "(O)710")
		{
			return ItemRegistry.Create("(O)334", 2);
		}
		Object bestIngredient = null;
		string[] ingredients = ArgUtility.SplitBySpace(ArgUtility.Get(fields, 0));
		for (int i = 0; i < ingredients.Length; i += 2)
		{
			string text = ArgUtility.Get(ingredients, i);
			int count = ArgUtility.GetInt(ingredients, i + 1, 1);
			Object ingredient = new Object(text, count);
			if (bestIngredient == null || ingredient.sellToStorePrice(-1L) * ingredient.Stack > bestIngredient.sellToStorePrice(-1L) * bestIngredient.Stack)
			{
				bestIngredient = ingredient;
			}
		}
		return bestIngredient;
	}

	/// <summary>Get the output item to produce for an Anvil.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.MachineOutputDelegate" />
	public static Item OutputAnvil(Object machine, Item inputItem, bool probe, MachineItemOutput outputData, out int? overrideMinutesUntilReady)
	{
		overrideMinutesUntilReady = 10;
		if (inputItem != null && inputItem is Trinket t)
		{
			if (!t.GetTrinketData().CanBeReforged)
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\1_6_Strings:Anvil_wrongtrinket"));
				return null;
			}
			Game1.currentLocation.playSound("metal_tap");
			DelayedAction.playSoundAfterDelay("metal_tap", 250);
			DelayedAction.playSoundAfterDelay("metal_tap", 500);
			return new Trinket(inputItem.ItemId, Game1.random.Next(9999999));
		}
		return null;
	}

	/// <summary>Get the output item to produce for a Geode Crusher.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.MachineOutputDelegate" />
	public static Item OutputGeodeCrusher(Object machine, Item inputItem, bool probe, MachineItemOutput outputData, out int? overrideMinutesUntilReady)
	{
		overrideMinutesUntilReady = null;
		if (!Utility.IsGeode(inputItem, disallow_special_geodes: true))
		{
			return null;
		}
		Game1.stats.GeodesCracked++;
		Object result = (Object)Utility.getTreasureFromGeode(inputItem);
		if (!probe)
		{
			GameLocation location = machine.Location;
			Vector2 pixelPos = machine.tileLocation.Value * 64f;
			Utility.addSmokePuff(location, pixelPos + new Vector2(4f, -48f), 200);
			Utility.addSmokePuff(location, pixelPos + new Vector2(-16f, -56f), 300);
			Utility.addSmokePuff(location, pixelPos + new Vector2(16f, -52f), 400);
			Utility.addSmokePuff(location, pixelPos + new Vector2(32f, -56f), 200);
			Utility.addSmokePuff(location, pixelPos + new Vector2(40f, -44f), 500);
		}
		return result;
	}

	/// <summary>Get the output item to produce for an Incubator or Ostrich Incubator.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.MachineOutputDelegate" />
	public static Item OutputIncubator(Object machine, Item inputItem, bool probe, MachineItemOutput outputData, out int? overrideMinutesUntilReady)
	{
		BuildingData buildingData = machine.Location.GetContainingBuilding()?.GetData();
		if (buildingData == null)
		{
			overrideMinutesUntilReady = null;
			return null;
		}
		FarmAnimalData animalData = FarmAnimal.GetAnimalDataFromEgg(inputItem, machine.Location);
		if (animalData == null || !buildingData.ValidOccupantTypes.Contains(animalData.House))
		{
			overrideMinutesUntilReady = null;
			return null;
		}
		overrideMinutesUntilReady = ((animalData.IncubationTime > 0) ? animalData.IncubationTime : 9000);
		return inputItem.getOne();
	}

	/// <summary>Get the output item to produce for a Seed Maker.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.MachineOutputDelegate" />
	public static Item OutputSeedMaker(Object machine, Item inputItem, bool probe, MachineItemOutput outputData, out int? overrideMinutesUntilReady)
	{
		overrideMinutesUntilReady = null;
		if (!inputItem.HasTypeObject())
		{
			return null;
		}
		string seed = null;
		foreach (KeyValuePair<string, CropData> v in Game1.cropData)
		{
			if (ItemRegistry.HasItemId(inputItem, v.Value.HarvestItemId))
			{
				seed = v.Key;
				break;
			}
		}
		if (seed == null)
		{
			return null;
		}
		Vector2 tile = machine.tileLocation.Value;
		Random r = Utility.CreateDaySaveRandom(tile.X, tile.Y * 77f, Game1.timeOfDay);
		if (r.NextDouble() < 0.005)
		{
			return new Object("499", 1);
		}
		if (r.NextDouble() < 0.02)
		{
			return new Object("770", r.Next(1, 5));
		}
		return new Object(seed, r.Next(1, 4));
	}

	public static Item OutputMushroomLog(Object machine, Item inputItem, bool probe, MachineItemOutput outputData, out int? overrideMinutesUntilReady)
	{
		overrideMinutesUntilReady = null;
		List<Tree> nearbyTrees = new List<Tree>();
		for (int x = (int)machine.TileLocation.X - 3; x < (int)machine.TileLocation.X + 4; x++)
		{
			for (int y = (int)machine.TileLocation.Y - 3; y < (int)machine.TileLocation.Y + 4; y++)
			{
				Vector2 v = new Vector2(x, y);
				if (machine.Location.terrainFeatures.ContainsKey(v) && machine.Location.terrainFeatures[v] is Tree)
				{
					nearbyTrees.Add(machine.Location.terrainFeatures[v] as Tree);
				}
			}
		}
		int treeCount = nearbyTrees.Count;
		List<string> mushroomPossibilities = new List<string>();
		int mossyCount = 0;
		foreach (Tree t in nearbyTrees)
		{
			if ((int)t.growthStage >= 5)
			{
				string mushroomType = (Game1.random.NextBool(0.05) ? "(O)422" : (Game1.random.NextBool(0.15) ? "(O)420" : "(O)404"));
				switch ((string)t.treeType)
				{
				case "2":
					mushroomType = (Game1.random.NextBool(0.1) ? "(O)422" : "(O)420");
					break;
				case "1":
					mushroomType = "(O)257";
					break;
				case "3":
					mushroomType = "(O)281";
					break;
				case "13":
					mushroomType = "(O)422";
					break;
				}
				mushroomPossibilities.Add(mushroomType);
				if ((bool)t.hasMoss)
				{
					mossyCount++;
				}
			}
		}
		for (int i = 0; i < Math.Max(1, (int)((float)nearbyTrees.Count * 0.75f)); i++)
		{
			mushroomPossibilities.Add(Game1.random.NextBool(0.05) ? "(O)422" : (Game1.random.NextBool(0.15) ? "(O)420" : "(O)404"));
		}
		int amount = Math.Max(1, Math.Min(5, Game1.random.Next(1, 3) * (nearbyTrees.Count / 2)));
		int quality = 0;
		float qualityBoostChance = (float)mossyCount * 0.025f + (float)treeCount * 0.025f;
		while (Game1.random.NextDouble() < (double)qualityBoostChance)
		{
			quality++;
			if (quality == 3)
			{
				quality = 4;
				break;
			}
		}
		return ItemRegistry.Create(Game1.random.ChooseFrom(mushroomPossibilities), amount, quality);
	}

	public bool ParseItemCount(string[] query, out string replacement, Random random, Farmer player)
	{
		if (query[0] == "ItemCount")
		{
			replacement = CurrentParsedItemCount.ToString();
			return true;
		}
		replacement = null;
		return false;
	}

	/// <summary>Place an item in this machine.</summary>
	/// <param name="machineData">The machine data to apply.</param>
	/// <param name="inputItem">The item to place in the machine.</param>
	/// <param name="probe">Whether to return whether the item would be placed successfully without making any changes.</param>
	/// <param name="who">The player placing an item in the machine.</param>
	/// <param name="showMessages">Whether to show UI messages for the player.</param>
	/// <param name="playSounds">Whether to play sounds when the item is placed.</param>
	public bool PlaceInMachine(MachineData machineData, Item inputItem, bool probe, Farmer who, bool showMessages = true, bool playSounds = true)
	{
		if (machineData == null || inputItem == null)
		{
			return false;
		}
		if (heldObject.Value != null)
		{
			if (!machineData.AllowLoadWhenFull)
			{
				return false;
			}
			if (inputItem.QualifiedItemId == lastInputItem.Value?.QualifiedItemId)
			{
				return false;
			}
		}
		if (!MachineDataUtility.HasAdditionalRequirements(autoLoadFrom ?? who.Items, machineData.AdditionalConsumedItems, out var failedRequirement))
		{
			if (showMessages && failedRequirement.InvalidCountMessage != null && !probe && autoLoadFrom == null)
			{
				CurrentParsedItemCount = failedRequirement.RequiredCount;
				Game1.showRedMessage(TokenParser.ParseText(failedRequirement.InvalidCountMessage, null, ParseItemCount));
				who.ignoreItemConsumptionThisFrame = true;
			}
			return false;
		}
		GameLocation location = Location;
		if (!MachineDataUtility.TryGetMachineOutputRule(this, machineData, MachineOutputTrigger.ItemPlacedInMachine, inputItem, who, location, out var outputRule, out var triggerRule, out var outputRuleIgnoringCount, out var triggerIgnoringCount))
		{
			if (showMessages && !probe && autoLoadFrom == null)
			{
				if (outputRuleIgnoringCount != null)
				{
					string invalidCountMessage = outputRuleIgnoringCount.InvalidCountMessage ?? machineData.InvalidCountMessage;
					if (!string.IsNullOrWhiteSpace(invalidCountMessage))
					{
						CurrentParsedItemCount = triggerIgnoringCount.RequiredCount;
						Game1.showRedMessage(TokenParser.ParseText(invalidCountMessage, null, ParseItemCount));
						who.ignoreItemConsumptionThisFrame = true;
					}
				}
				else if (machineData.InvalidItemMessage != null && GameStateQuery.CheckConditions(machineData.InvalidItemMessageCondition, location, who, null, who.ActiveObject))
				{
					Game1.showRedMessage(TokenParser.ParseText(machineData.InvalidItemMessage));
					who.ignoreItemConsumptionThisFrame = true;
				}
			}
			return false;
		}
		if (probe)
		{
			return true;
		}
		if (!OutputMachine(machineData, outputRule, inputItem, who, location, probe))
		{
			return false;
		}
		if (machineData.AdditionalConsumedItems != null)
		{
			IInventory inventory = autoLoadFrom ?? who.Items;
			foreach (MachineItemAdditionalConsumedItems additionalRequirement in machineData.AdditionalConsumedItems)
			{
				inventory.ReduceId(additionalRequirement.ItemId, additionalRequirement.RequiredCount);
			}
		}
		if (triggerRule.RequiredCount > 0)
		{
			ConsumeInventoryItem(who, inputItem, triggerRule.RequiredCount);
		}
		if (machineData.LoadEffects != null)
		{
			foreach (MachineEffects effect in machineData.LoadEffects)
			{
				if (PlayMachineEffect(effect, playSounds))
				{
					_machineAnimation = effect;
					_machineAnimationLoop = false;
					_machineAnimationIndex = 0;
					_machineAnimationFrame = -1;
					_machineAnimationInterval = 0;
					break;
				}
			}
		}
		playCustomMachineLoadEffects();
		MachineDataUtility.UpdateStats(machineData.StatsToIncrementWhenLoaded, inputItem, 1);
		return true;
	}

	private void playCustomMachineLoadEffects()
	{
		if (base.ItemId == "FishSmoker")
		{
			for (int i = 0; i < 12; i++)
			{
				Location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), 9999f, 1, 1, new Vector2((float)((int)TileLocation.X * 64) + 18f, ((float)(int)TileLocation.Y - 1.15f) * 64f), flicker: false, flipped: false)
				{
					color = new Color(60, 60, 60),
					alphaFade = -0.02f,
					alpha = 0.01f,
					alphaFadeFade = -0.0003f,
					motion = new Vector2(0.25f, -0.1f),
					acceleration = new Vector2(0f, -0.01f),
					rotationChange = (float)Game1.random.Next(-10, 10) / 500f,
					scale = 1.5f,
					scaleChange = 0.024f,
					layerDepth = Math.Max(0f, ((tileLocation.Y + 1f) * 64f - 24f + (float)i) / 10000f) + tileLocation.X * 1E-05f,
					delayBeforeAnimationStart = i * 550
				});
			}
		}
	}

	/// <summary>Cause the machine to produce output, if applicable.</summary>
	/// <param name="machine">The machine data to apply.</param>
	/// <param name="outputRule">The output rule to apply, or <c>null</c> to get a matching rule from the machine data.</param>
	/// <param name="inputItem">The input item for which to produce an item, if applicable.</param>
	/// <param name="who">The player for which to start producing output, or <c>null</c> for the main player.</param>
	/// <param name="location">The location containing the machine.</param>
	/// <param name="probe">Whether to return whether the machine would produce output without making any changes.</param>
	public virtual bool OutputMachine(MachineData machine, MachineOutputRule outputRule, Item inputItem, Farmer who, GameLocation location, bool probe)
	{
		if (machine == null)
		{
			return false;
		}
		if (heldObject.Value != null && !machine.AllowLoadWhenFull)
		{
			return false;
		}
		who = who ?? Game1.MasterPlayer;
		if (outputRule == null && !MachineDataUtility.TryGetMachineOutputRule(this, machine, MachineOutputTrigger.ItemPlacedInMachine, inputItem, who, location, out outputRule, out var _, out var _, out var _))
		{
			return false;
		}
		MachineItemOutput outputData = MachineDataUtility.GetOutputData(this, machine, outputRule, inputItem, who, location);
		int? overrideMinutesUntilReady;
		Item newHeldItem = MachineDataUtility.GetOutputItem(this, outputData, inputItem, who, probe, out overrideMinutesUntilReady);
		if (newHeldItem == null)
		{
			return false;
		}
		if (probe)
		{
			return true;
		}
		int minutesUntilReady = 0;
		if (overrideMinutesUntilReady >= 0)
		{
			minutesUntilReady = overrideMinutesUntilReady.Value;
		}
		else if (outputRule.MinutesUntilReady >= 0 || outputRule.DaysUntilReady >= 0)
		{
			minutesUntilReady = ((outputRule.DaysUntilReady >= 0) ? Utility.CalculateMinutesUntilMorning(Game1.timeOfDay, outputRule.DaysUntilReady) : outputRule.MinutesUntilReady);
		}
		minutesUntilReady = (int)Utility.ApplyQuantityModifiers(minutesUntilReady, machine.ReadyTimeModifiers, machine.ReadyTimeModifierMode, location, who, newHeldItem, inputItem);
		newHeldItem.FixQuality();
		newHeldItem.FixStackSize();
		heldObject.Value = (Object)newHeldItem;
		MinutesUntilReady = minutesUntilReady;
		if (MinutesUntilReady == 0)
		{
			readyForHarvest.Value = true;
		}
		lastOutputRuleId.Value = outputRule.Id;
		if (inputItem != null)
		{
			lastInputItem.Value = inputItem.getOne();
			lastInputItem.Value.Stack = inputItem.Stack;
		}
		else
		{
			lastInputItem.Value = null;
		}
		if (machine.IsIncubator && location is AnimalHouse animalHouse)
		{
			animalHouse.hasShownIncubatorBuildingFullMessage = false;
		}
		ResetParentSheetIndex();
		base.ParentSheetIndex += outputData.IncrementMachineParentSheetIndex;
		if (machine.LightWhileWorking != null)
		{
			initializeLightSource(tileLocation.Value);
		}
		if (machine.ShowNextIndexWhileWorking)
		{
			showNextIndex.Value = true;
		}
		if (machine.WobbleWhileWorking)
		{
			scale.X = 5f;
		}
		return true;
	}

	/// <summary>Apply a machine effect, if it's valid and its fields match.</summary>
	/// <param name="effect">The machine effect to apply.</param>
	/// <param name="playSounds">Whether to play sounds when the item is placed.</param>
	public virtual bool PlayMachineEffect(MachineEffects effect, bool playSounds = true)
	{
		return MachineDataUtility.PlayEffects(this, effect, playSounds);
	}

	public virtual void updateWhenCurrentLocation(GameTime time)
	{
		GameLocation environment = Location;
		if (environment == null)
		{
			return;
		}
		if ((bool)readyForHarvest && !_hasHeldObject)
		{
			readyForHarvest.Value = false;
		}
		if (_hasLightSource)
		{
			LightSource light = netLightSource.Get();
			if (light != null && (bool)isOn && !environment.hasLightSource(light.Identifier))
			{
				environment.sharedLights[light.identifier] = light.Clone();
			}
		}
		if (_machineAnimation != null)
		{
			List<int> frames = _machineAnimation.Frames;
			if (frames != null && frames.Count > 0)
			{
				_machineAnimationInterval += (int)time.ElapsedGameTime.TotalMilliseconds;
				if (_machineAnimation.Interval > 0 && _machineAnimationInterval >= _machineAnimation.Interval)
				{
					_machineAnimationIndex += _machineAnimationInterval / _machineAnimation.Interval;
					_machineAnimationInterval %= _machineAnimation.Interval;
					if (_machineAnimationIndex >= _machineAnimation.Frames.Count)
					{
						if (_machineAnimationLoop)
						{
							_machineAnimationIndex %= _machineAnimation.Frames.Count;
						}
						else
						{
							_machineAnimation = null;
							_machineAnimationFrame = -1;
						}
					}
				}
				if (_machineAnimation != null)
				{
					_machineAnimationFrame = _machineAnimation.Frames[_machineAnimationIndex];
				}
			}
			else
			{
				_machineAnimationFrame = -1;
			}
		}
		if (_hasHeldObject)
		{
			Object heldObject = this.heldObject.Get();
			if (heldObject.QualifiedItemId == "(O)913" && IsSprinkler() && heldObject.heldObject.Value is Chest chest)
			{
				chest.mutex.Update(environment);
				if (Game1.activeClickableMenu == null && chest.GetMutex().IsLockHeld())
				{
					chest.GetMutex().ReleaseLock();
				}
			}
			if (heldObject._hasLightSource)
			{
				lightSource = heldObject.netLightSource.Get();
				if (lightSource != null && !environment.hasLightSource(lightSource.Identifier))
				{
					environment.sharedLights[lightSource.identifier] = lightSource.Clone();
				}
			}
			if (!readyForHarvest.Value)
			{
				if (_machineAnimation == null)
				{
					MachineData data = GetMachineData();
					if (data?.WorkingEffects != null)
					{
						foreach (MachineEffects effect in data.WorkingEffects)
						{
							if (effect != null)
							{
								string condition = effect.Condition;
								GameLocation location = Location;
								Item value = lastInputItem.Value;
								if (GameStateQuery.CheckConditions(condition, location, null, heldObject, value))
								{
									_machineAnimation = effect;
									_machineAnimationLoop = true;
									_machineAnimationIndex = 0;
									_machineAnimationFrame = -1;
									MachineEffects machineAnimation = _machineAnimation;
									_machineAnimationInterval = ((machineAnimation != null && machineAnimation.Frames?.Count > 0 && _machineAnimation.Interval > 0) ? ((int)(((double)(long)(tileLocation.X * (float)(_machineAnimation.Interval / 2) + tileLocation.Y * (float)(_machineAnimation.Interval / 2 * 10)) + time.TotalGameTime.TotalMilliseconds) % (double)(_machineAnimation.Interval * _machineAnimation.Frames.Count))) : 0);
									break;
								}
							}
						}
					}
				}
			}
			else if (_machineAnimation != null && _machineAnimationLoop)
			{
				_machineAnimation = null;
			}
		}
		else if (_machineAnimation != null && _machineAnimationLoop)
		{
			_machineAnimation = null;
		}
		if (shakeTimer > 0)
		{
			shakeTimer -= time.ElapsedGameTime.Milliseconds;
			if (shakeTimer <= 0)
			{
				health = 10;
			}
		}
		if ((base.QualifiedItemId == "(O)590" || base.QualifiedItemId == "(O)SeedSpot") && Game1.random.NextDouble() < 0.01)
		{
			shakeTimer = 100;
		}
		if (base.QualifiedItemId == "(BC)56")
		{
			ResetParentSheetIndex();
			base.ParentSheetIndex += (int)(time.TotalGameTime.TotalMilliseconds % 600.0 / 100.0);
		}
		if (!(base.QualifiedItemId == "(BC)TextSign"))
		{
			return;
		}
		if (shouldShowSign)
		{
			shouldShowSign = false;
			lastNoteBlockSoundTime += (int)time.ElapsedGameTime.TotalMilliseconds;
			if (lastNoteBlockSoundTime > 125)
			{
				lastNoteBlockSoundTime = 125;
			}
		}
		else if (lastNoteBlockSoundTime > 0)
		{
			lastNoteBlockSoundTime -= (int)time.ElapsedGameTime.TotalMilliseconds;
			if (lastNoteBlockSoundTime < 0)
			{
				lastNoteBlockSoundTime = 0;
			}
		}
	}

	/// <summary>Handle the player entering the location containing the object.</summary>
	public virtual void actionOnPlayerEntry()
	{
		isTemporarilyInvisible = false;
		health = 10;
		if (base.QualifiedItemId == "(BC)99")
		{
			showNextIndex.Value = (int)Location.GetRootLocation().piecesOfHay > 0;
		}
	}

	/// <inheritdoc />
	public override bool canBeTrashed()
	{
		if (!questItem.Value && base.canBeTrashed())
		{
			if (Game1.objectData.TryGetValue(base.ItemId, out var data))
			{
				return data.CanBeTrashed;
			}
			return true;
		}
		return false;
	}

	public virtual bool isForage()
	{
		if (base.Category != -79 && base.Category != -81 && base.Category != -80 && base.Category != -75 && base.Category != -23 && !HasContextTag("forage_item"))
		{
			return base.QualifiedItemId == "(O)430";
		}
		return true;
	}

	/// <summary>Initialize and register a light source for this instance, if applicable for its data and state.</summary>
	/// <param name="tileLocation">The object's tile position.</param>
	/// <param name="mineShaft">Whether the object is in the mines.</param>
	public virtual void initializeLightSource(Vector2 tileLocation, bool mineShaft = false)
	{
		if (name == null)
		{
			return;
		}
		int identifier = (int)(tileLocation.X * 2000f + tileLocation.Y);
		Furniture furniture = this as Furniture;
		if ((int)furniture?.furniture_type == 14 && (bool)furniture.isOn)
		{
			lightSource = new LightSource(4, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f - 64f), 2.5f, new Color(0, 80, 160), identifier, LightSource.LightContext.None, 0L);
			return;
		}
		if ((int)furniture?.furniture_type == 16 && (bool)furniture.isOn)
		{
			lightSource = new LightSource(4, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f - 64f), 1.5f, new Color(0, 80, 160), identifier, LightSource.LightContext.None, 0L);
			return;
		}
		if ((bool)bigCraftable)
		{
			if (this is Torch && (bool)isOn)
			{
				float y_offset = -64f;
				if (ItemContextTagManager.HasBaseTag(base.QualifiedItemId, "campfire_item"))
				{
					y_offset = 32f;
				}
				lightSource = new LightSource(4, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f + y_offset), 2.5f, new Color(0, 80, 160), identifier, LightSource.LightContext.None, 0L);
				return;
			}
			if ((bool)isLamp)
			{
				lightSource = new LightSource(4, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f - 64f), 3f, new Color(0, 40, 80), identifier, LightSource.LightContext.None, 0L);
				return;
			}
			string qualifiedItemId = base.QualifiedItemId;
			if (qualifiedItemId == "(BC)74")
			{
				lightSource = new LightSource(4, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f), 1.5f, Color.DarkCyan, identifier, LightSource.LightContext.None, 0L);
				return;
			}
			if (qualifiedItemId == "(BC)96")
			{
				lightSource = new LightSource(4, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f), 1f, Color.HotPink * 0.75f, identifier, LightSource.LightContext.None, 0L);
				return;
			}
		}
		else if (Utility.IsNormalObjectAtParentSheetIndex(this, base.ItemId) || this is Torch)
		{
			if (base.QualifiedItemId == "(O)95" || ItemContextTagManager.HasBaseTag(base.QualifiedItemId, "torch_item"))
			{
				string qualifiedItemId = base.ItemId;
				lightSource = new LightSource(color: (qualifiedItemId == "94") ? Color.Yellow : ((!(qualifiedItemId == "95")) ? (new Color(1, 1, 1) * 0.9f) : (new Color(70, 0, 150) * 0.9f)), textureIndex: 4, position: new Vector2(tileLocation.X * 64f + 16f, tileLocation.Y * 64f + 16f), radius: mineShaft ? 1.5f : 1.25f, identifier: identifier, light_context: LightSource.LightContext.None, playerID: 0L);
				return;
			}
			if (base.QualifiedItemId == "(O)746")
			{
				lightSource = new LightSource(4, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f + 48f), 0.5f, new Color(1, 1, 1) * 0.65f, identifier, LightSource.LightContext.None, 0L);
				return;
			}
			if (IsSprinkler() && base.SpecialVariable == 999999)
			{
				lightSource = new LightSource(4, new Vector2(tileLocation.X * 64f + 16f, tileLocation.Y * 64f + 16f), 1.25f, new Color(1, 1, 1) * 0.9f, (int)(tileLocation.X * 2000f + tileLocation.Y), LightSource.LightContext.None, 0L);
			}
		}
		if (MinutesUntilReady > 0)
		{
			MachineLight light = GetMachineData()?.LightWhileWorking;
			if (light != null)
			{
				lightSource = new LightSource(4, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f), light.Radius, Utility.StringToColor(light.Color) ?? Color.White, identifier, LightSource.LightContext.None, 0L);
			}
		}
	}

	public virtual void performRemoveAction()
	{
		GameLocation environment = Location;
		Vector2 tileLocation = TileLocation;
		if (environment != null)
		{
			if (lightSource != null)
			{
				environment.removeLightSource(lightSource.identifier);
				environment.removeLightSource((int)Game1.player.UniqueMultiplayerID);
			}
			if (IsTapper() && environment.terrainFeatures != null && environment.terrainFeatures.TryGetValue(tileLocation, out var terrainFeature) && terrainFeature is Tree tree)
			{
				tree.tapped.Value = false;
			}
			if (IsSprinkler())
			{
				environment.removeTemporarySpritesWithID((int)tileLocation.X * 4000 + (int)tileLocation.Y);
			}
		}
		if (base.QualifiedItemId == "(BC)126")
		{
			string id = (((int)quality != 0) ? ((int)quality - 1).ToString() : preservedParentSheetIndex.Value);
			if (id != null)
			{
				Game1.createItemDebris(new Hat(id), tileLocation * 64f, (Game1.player.FacingDirection + 2) % 4);
				quality.Value = 0;
				preservedParentSheetIndex.Value = null;
			}
		}
		if (name != null && name.Contains("Seasonal") && bigCraftable.Value)
		{
			ResetParentSheetIndex();
		}
	}

	public virtual void dropItem(GameLocation location, Vector2 origin, Vector2 destination)
	{
		if ((Type == "Crafting" || Type == "interactive") && (int)fragility != 2)
		{
			location.debris.Add(new Debris(base.QualifiedItemId, origin, destination));
		}
	}

	public virtual bool isPassable()
	{
		if (isTemporarilyInvisible)
		{
			return true;
		}
		if ((bool)bigCraftable)
		{
			return false;
		}
		switch (base.QualifiedItemId)
		{
		case "(O)286":
		case "(O)287":
		case "(O)288":
		case "(O)893":
		case "(O)894":
		case "(O)895":
		case "(O)590":
		case "(O)SeedSpot":
		case "(O)297":
		case "(O)BlueGrassStarter":
		case "(O)93":
			return true;
		default:
			if (IsFloorPathItem())
			{
				return true;
			}
			if (base.Category == -74 || base.Category == -19)
			{
				if (isSapling())
				{
					return false;
				}
				switch (base.QualifiedItemId)
				{
				case "(O)301":
				case "(O)302":
				case "(O)473":
					return false;
				default:
					return true;
				}
			}
			return false;
		}
	}

	public virtual void reloadSprite()
	{
		initializeLightSource(tileLocation.Value);
	}

	/// <summary>Get the pixel area containing the object.</summary>
	public Microsoft.Xna.Framework.Rectangle GetBoundingBox()
	{
		Vector2 tile = tileLocation.Value;
		return GetBoundingBoxAt((int)tile.X, (int)tile.Y);
	}

	/// <summary>Get the pixel area containing the object, adjusted for the given tile position.</summary>
	/// <param name="x">The tile X position to use instead of the object's current position.</param>
	/// <param name="y">The tile Y position to use instead of the object's current position.</param>
	public virtual Microsoft.Xna.Framework.Rectangle GetBoundingBoxAt(int x, int y)
	{
		Microsoft.Xna.Framework.Rectangle bounds = boundingBox.Value;
		if ((this is Torch && !bigCraftable) || base.QualifiedItemId == "(O)590")
		{
			bounds.X = (int)tileLocation.X * 64 + 24;
			bounds.Y = (int)tileLocation.Y * 64 + 24;
		}
		else
		{
			bounds.X = (int)tileLocation.X * 64;
			bounds.Y = (int)tileLocation.Y * 64;
		}
		if (boundingBox.Value != bounds)
		{
			boundingBox.Value = bounds;
		}
		return bounds;
	}

	/// <inheritdoc />
	public override bool canBeGivenAsGift()
	{
		if (!bigCraftable.Value && !(this is Furniture) && !(this is Wallpaper))
		{
			if (Game1.objectData.TryGetValue(base.ItemId, out var data))
			{
				return data.CanBeGivenAsGift;
			}
			return true;
		}
		return false;
	}

	/// <summary>Update the object instance before it's placed in the world.</summary>
	/// <param name="who">The player placing the item.</param>
	/// <returns>Returns <c>true</c> if the item should be destroyed, or <c>false</c> if it should be set down.</returns>
	/// <remarks>This is called on the object instance being placed, after it's already been split from the inventory stack if applicable. At this point the <see cref="P:StardewValley.Object.Location" /> and <see cref="P:StardewValley.Object.TileLocation" /> values should already be set.</remarks>
	public virtual bool performDropDownAction(Farmer who)
	{
		if (who == null)
		{
			who = Game1.getFarmer(owner.Value);
		}
		GameLocation location = Location;
		MachineData machineData = GetMachineData();
		if (MachineDataUtility.TryGetMachineOutputRule(this, machineData, MachineOutputTrigger.MachinePutDown, null, who, location, out var outputRule, out var _, out var _, out var _))
		{
			OutputMachine(machineData, outputRule, null, who, location, probe: false);
			return false;
		}
		string qualifiedItemId = base.QualifiedItemId;
		if (!(qualifiedItemId == "(BC)96"))
		{
			if (qualifiedItemId == "(BC)99")
			{
				showNextIndex.Value = (int)location.GetRootLocation().piecesOfHay >= 0;
			}
		}
		else
		{
			minutesUntilReady.Value = Utility.CalculateMinutesUntilMorning(Game1.timeOfDay, 3);
		}
		return false;
	}

	private void totemWarp(Farmer who)
	{
		GameLocation location = who.currentLocation;
		for (int i = 0; i < 12; i++)
		{
			Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(354, Game1.random.Next(25, 75), 6, 1, new Vector2(Game1.random.Next((int)who.Position.X - 256, (int)who.Position.X + 192), Game1.random.Next((int)who.Position.Y - 256, (int)who.Position.Y + 192)), flicker: false, Game1.random.NextBool()));
		}
		who.playNearbySoundAll("wand");
		Game1.displayFarmer = false;
		Game1.player.temporarilyInvincible = true;
		Game1.player.temporaryInvincibilityTimer = -2000;
		Game1.player.freezePause = 1000;
		Game1.flashAlpha = 1f;
		DelayedAction.fadeAfterDelay(totemWarpForReal, 1000);
		Microsoft.Xna.Framework.Rectangle playerBounds = who.GetBoundingBox();
		new Microsoft.Xna.Framework.Rectangle(playerBounds.X, playerBounds.Y, 64, 64).Inflate(192, 192);
		int j = 0;
		Point playerTile = who.TilePoint;
		for (int x = playerTile.X + 8; x >= playerTile.X - 8; x--)
		{
			Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(6, new Vector2(x, playerTile.Y) * 64f, Color.White, 8, flipped: false, 50f)
			{
				layerDepth = 1f,
				delayBeforeAnimationStart = j * 25,
				motion = new Vector2(-0.25f, 0f)
			});
			j++;
		}
	}

	private void totemWarpForReal()
	{
		switch (base.QualifiedItemId)
		{
		case "(O)688":
		{
			if (!Game1.getFarm().TryGetMapPropertyAs("WarpTotemEntry", out Point warp_location, required: false))
			{
				warp_location = Game1.whichFarm switch
				{
					6 => new Point(82, 29), 
					5 => new Point(48, 39), 
					_ => new Point(48, 7), 
				};
			}
			Game1.warpFarmer("Farm", warp_location.X, warp_location.Y, flip: false);
			break;
		}
		case "(O)689":
			Game1.warpFarmer("Mountain", 31, 20, flip: false);
			break;
		case "(O)690":
			Game1.warpFarmer("Beach", 20, 4, flip: false);
			break;
		case "(O)261":
			Game1.warpFarmer("Desert", 35, 43, flip: false);
			break;
		case "(O)886":
			Game1.warpFarmer("IslandSouth", 11, 11, flip: false);
			break;
		}
		Game1.fadeToBlackAlpha = 0.99f;
		Game1.screenGlow = false;
		Game1.player.temporarilyInvincible = false;
		Game1.player.temporaryInvincibilityTimer = 0;
		Game1.displayFarmer = true;
	}

	public void MonsterMusk(Farmer who)
	{
		GameLocation location = who.currentLocation;
		who.FarmerSprite.PauseForSingleAnimation = false;
		who.FarmerSprite.StopAnimation();
		who.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[4]
		{
			new FarmerSprite.AnimationFrame(104, 350, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(105, 350, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(104, 350, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(105, 350, secondaryArm: false, flip: false)
		});
		location.playSound("croak");
		who.applyBuff("24");
	}

	public override void ModifyItemBuffs(BuffEffects effects)
	{
		if (effects != null && base.Category == -7)
		{
			int buff_bonus = 0;
			if (base.Quality != 0)
			{
				buff_bonus = 1;
			}
			if (buff_bonus > 0)
			{
				NetFloat[] array = new NetFloat[9] { effects.FarmingLevel, effects.FishingLevel, effects.MiningLevel, effects.LuckLevel, effects.ForagingLevel, effects.MaxStamina, effects.MagneticRadius, effects.Defense, effects.Attack };
				foreach (NetFloat effect in array)
				{
					if (effect.Value != 0f)
					{
						effect.Value += buff_bonus;
					}
				}
			}
		}
		base.ModifyItemBuffs(effects);
	}

	private void treasureTotem(Farmer who, GameLocation gameLocation)
	{
		Game1.playSound("treasure_totem");
		Game1.netWorldState.Value.TreasureTotemsUsed++;
		Vector2 center = who.Tile;
		int radius = 4;
		for (int x = (int)center.X - radius; (float)x < center.X + (float)radius; x++)
		{
			for (int y = (int)center.Y - radius; (float)y < center.Y + (float)radius; y++)
			{
				if (Math.Round(Utility.distance(x, center.X, y, center.Y)) == (double)(radius - 1))
				{
					Vector2 location = new Vector2(x, y);
					if (gameLocation.CanItemBePlacedHere(location) && !gameLocation.IsTileOccupiedBy(location) && gameLocation.getTileIndexAt(x, y, "AlwaysFront") == -1 && gameLocation.getTileIndexAt(x, y, "Front") == -1 && !gameLocation.isBehindBush(location) && (gameLocation.doesTileHaveProperty(x, y, "Diggable", "Back") != null || (gameLocation.GetSeason() == Season.Winter && gameLocation.doesTileHaveProperty(x, y, "Type", "Back") != null && gameLocation.doesTileHaveProperty(x, y, "Type", "Back").Equals("Grass"))))
					{
						if ((name.Equals("Forest") && x >= 93 && y <= 22) || !gameLocation.IsOutdoors)
						{
							continue;
						}
						gameLocation.objects.Add(location, ItemRegistry.Create<Object>("(O)590"));
					}
					Utility.addRainbowStarExplosion(gameLocation, new Vector2(x, y) * 64f, 1);
					Utility.addStarsAndSpirals(gameLocation, x, y, 1, 1, 100, 100, Color.White);
				}
				if (Math.Round(Utility.distance(x, center.X, y, center.Y)) <= (double)(radius - 1))
				{
					Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(144, 249, 7, 7), Game1.random.Next(100, 200), 6, 1, new Vector2(x, y) * 64f + new Vector2(32 + Game1.random.Next(-16, 16), 32 + Game1.random.Next(-16, 16)), flicker: false, flipped: false, 0.001f, 0f, (Game1.random.NextDouble() < 0.5) ? new Color(255, 255, 100) : Color.White, 4f, 0f, 0f, 0f), gameLocation);
				}
			}
		}
	}

	private void rainTotem(Farmer who)
	{
		GameLocation location = who.currentLocation;
		string contextId = location.GetLocationContextId();
		LocationContextData context = location.GetLocationContext();
		if (!context.AllowRainTotem)
		{
			Game1.showRedMessageUsingLoadString("Strings\\UI:Item_CantBeUsedHere");
			return;
		}
		if (context.RainTotemAffectsContext != null)
		{
			contextId = context.RainTotemAffectsContext;
		}
		bool applied = false;
		if (contextId == "Default")
		{
			if (!Utility.isFestivalDay(Game1.dayOfMonth + 1, Game1.season))
			{
				Game1.netWorldState.Value.WeatherForTomorrow = (Game1.weatherForTomorrow = "Rain");
				applied = true;
			}
		}
		else
		{
			location.GetWeather().WeatherForTomorrow = "Rain";
			applied = true;
		}
		if (applied)
		{
			Game1.pauseThenMessage(2000, Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12822"));
		}
		Game1.screenGlow = false;
		location.playSound("thunder");
		who.canMove = false;
		Game1.screenGlowOnce(Color.SlateBlue, hold: false);
		Game1.player.faceDirection(2);
		Game1.player.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[1]
		{
			new FarmerSprite.AnimationFrame(57, 2000, secondaryArm: false, flip: false, Farmer.canMoveNow, behaviorAtEndOfFrame: true)
		});
		for (int i = 0; i < 6; i++)
		{
			Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(648, 1045, 52, 33), 9999f, 1, 999, who.Position + new Vector2(0f, -128f), flicker: false, flipped: false, 1f, 0.01f, Color.White * 0.8f, 2f, 0.01f, 0f, 0f)
			{
				motion = new Vector2((float)Game1.random.Next(-10, 11) / 10f, -2f),
				delayBeforeAnimationStart = i * 200
			});
			Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(648, 1045, 52, 33), 9999f, 1, 999, who.Position + new Vector2(0f, -128f), flicker: false, flipped: false, 1f, 0.01f, Color.White * 0.8f, 1f, 0.01f, 0f, 0f)
			{
				motion = new Vector2((float)Game1.random.Next(-30, -10) / 10f, -1f),
				delayBeforeAnimationStart = 100 + i * 200
			});
			Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(648, 1045, 52, 33), 9999f, 1, 999, who.Position + new Vector2(0f, -128f), flicker: false, flipped: false, 1f, 0.01f, Color.White * 0.8f, 1f, 0.01f, 0f, 0f)
			{
				motion = new Vector2((float)Game1.random.Next(10, 30) / 10f, -1f),
				delayBeforeAnimationStart = 200 + i * 200
			});
		}
		TemporaryAnimatedSprite sprite = new TemporaryAnimatedSprite(0, 9999f, 1, 999, Game1.player.Position + new Vector2(0f, -96f), flicker: false, flipped: false, verticalFlipped: false, 0f)
		{
			motion = new Vector2(0f, -7f),
			acceleration = new Vector2(0f, 0.1f),
			scaleChange = 0.015f,
			alpha = 1f,
			alphaFade = 0.0075f,
			shakeIntensity = 1f,
			initialPosition = Game1.player.Position + new Vector2(0f, -96f),
			xPeriodic = true,
			xPeriodicLoopTime = 1000f,
			xPeriodicRange = 4f,
			layerDepth = 1f
		};
		sprite.CopyAppearanceFromItemId(base.QualifiedItemId);
		Game1.multiplayer.broadcastSprites(location, sprite);
		DelayedAction.playSoundAfterDelay("rainsound", 2000);
	}

	private void readBook(GameLocation location)
	{
		Game1.player.canMove = false;
		Game1.player.freezePause = 1030;
		Game1.player.faceDirection(2);
		Game1.player.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[1]
		{
			new FarmerSprite.AnimationFrame(57, 1000, secondaryArm: false, flip: false, Farmer.canMoveNow, behaviorAtEndOfFrame: true)
			{
				frameEndBehavior = delegate
				{
					location.removeTemporarySpritesWithID(1987654);
					Utility.addRainbowStarExplosion(location, Game1.player.getStandingPosition() + new Vector2(-40f, -156f), 8);
				}
			}
		});
		Game1.MusicDuckTimer = 4000f;
		Game1.playSound("book_read");
		Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Book_Animation", new Microsoft.Xna.Framework.Rectangle(0, 0, 20, 20), 10f, 45, 1, Game1.player.getStandingPosition() + new Vector2(-48f, -156f), flicker: false, flipped: false, Game1.player.getDrawLayer() + 0.001f, 0f, Color.White, 4f, 0f, 0f, 0f)
		{
			holdLastFrame = true,
			id = 1987654
		});
		Color? c = ItemContextTagManager.GetColorFromTags(this);
		if (c.HasValue)
		{
			Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Book_Animation", new Microsoft.Xna.Framework.Rectangle(0, 20, 20, 20), 10f, 45, 1, Game1.player.getStandingPosition() + new Vector2(-48f, -156f), flicker: false, flipped: false, Game1.player.getDrawLayer() + 0.0012f, 0f, c.Value, 4f, 0f, 0f, 0f)
			{
				holdLastFrame = true,
				id = 1987654
			});
		}
		if (base.ItemId.StartsWith("SkillBook_"))
		{
			int current = Game1.player.newLevels.Count;
			Game1.player.gainExperience(Convert.ToInt32(base.ItemId.Last().ToString() ?? ""), 250);
			if (Game1.player.newLevels.Count == current || (Game1.player.newLevels.Count > 1 && current >= 1))
			{
				DelayedAction.functionAfterDelay(delegate
				{
					Game1.showGlobalMessage(Game1.content.LoadString("Strings\\1_6_Strings:SkillBookMessage", Game1.content.LoadString("Strings\\1_6_Strings:SkillName_" + base.ItemId.Last()).ToLower()));
				}, 1000);
			}
			return;
		}
		if (Game1.player.stats.Get(itemId) != 0 && base.ItemId != "Book_PriceCatalogue" && base.ItemId != "Book_AnimalCatalogue")
		{
			if (!Game1.player.mailReceived.Contains("read_a_book"))
			{
				Game1.player.mailReceived.Add("read_a_book");
			}
			bool foundAny = false;
			foreach (string s in GetContextTags())
			{
				if (s.StartsWith("book_xp"))
				{
					foundAny = true;
					string whichSkill = s.Split('_')[2];
					Game1.player.gainExperience(Farmer.getSkillNumberFromName(whichSkill), 100);
					break;
				}
			}
			if (!foundAny)
			{
				for (int i = 0; i < 5; i++)
				{
					Game1.player.gainExperience(i, 20);
				}
			}
			return;
		}
		string text = base.ItemId;
		if (!(text == "Book_QueenOfSauce"))
		{
			if (text == "PurpleBook")
			{
				Game1.player.gainExperience(0, 250);
				Game1.player.gainExperience(1, 250);
				Game1.player.gainExperience(2, 250);
				Game1.player.gainExperience(3, 250);
				Game1.player.gainExperience(4, 250);
				return;
			}
			Game1.player.stats.Increment(itemId);
			DelayedAction.functionAfterDelay(delegate
			{
				Game1.showGlobalMessage(Game1.content.LoadString("Strings\\1_6_Strings:LearnedANewPower"));
			}, 1000);
			if (!Game1.player.mailReceived.Contains("read_a_book"))
			{
				Game1.player.mailReceived.Add("read_a_book");
			}
			Utility.checkForBooksReadAchievement();
			return;
		}
		Dictionary<string, string> dictionary = DataLoader.Tv_CookingChannel(Game1.content);
		int num = 0;
		foreach (KeyValuePair<string, string> s in dictionary)
		{
			if (!Game1.player.cookingRecipes.ContainsKey(s.Value.Split("/")[0]))
			{
				Game1.player.cookingRecipes.Add(s.Value.Split("/")[0], 0);
				num++;
			}
		}
		Game1.player.stats.Increment(itemId);
		Game1.showGlobalMessage(Game1.content.LoadString("Strings\\1_6_Strings:QoS_Cookbook", num.ToString() ?? ""));
	}

	public virtual bool performUseAction(GameLocation location)
	{
		if (!Game1.player.canMove || isTemporarilyInvisible)
		{
			return false;
		}
		bool normal_gameplay = !Game1.eventUp && !Game1.isFestival() && !Game1.fadeToBlack && !Game1.player.swimming && !Game1.player.bathingClothes && !Game1.player.onBridge.Value;
		if (normal_gameplay && (base.Category == -102 || base.Category == -103))
		{
			readBook(location);
			return true;
		}
		if (name != null && name.Contains("Totem"))
		{
			if (normal_gameplay)
			{
				switch (base.QualifiedItemId)
				{
				case "(O)TreasureTotem":
					if (!location.IsOutdoors)
					{
						Game1.showRedMessageUsingLoadString("Strings\\StringsFromCSFiles:Object.cs.13053");
						return false;
					}
					treasureTotem(Game1.player, location);
					return true;
				case "(O)681":
					rainTotem(Game1.player);
					return true;
				case "(O)688":
				case "(O)689":
				case "(O)690":
				case "(O)886":
				case "(O)261":
				{
					Game1.player.jitterStrength = 1f;
					Color sprinkleColor = ((base.QualifiedItemId == "(O)681") ? Color.SlateBlue : ((base.QualifiedItemId == "(O)688") ? Color.LimeGreen : ((base.QualifiedItemId == "(O)689") ? Color.OrangeRed : ((base.QualifiedItemId == "(O)261") ? new Color(255, 200, 0) : Color.LightBlue))));
					location.playSound("warrior");
					Game1.player.faceDirection(2);
					Game1.player.CanMove = false;
					Game1.player.temporarilyInvincible = true;
					Game1.player.temporaryInvincibilityTimer = -4000;
					Game1.changeMusicTrack("silence");
					if (base.QualifiedItemId == "(O)681")
					{
						Game1.player.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[2]
						{
							new FarmerSprite.AnimationFrame(57, 2000, secondaryArm: false, flip: false),
							new FarmerSprite.AnimationFrame((short)Game1.player.FarmerSprite.CurrentFrame, 0, secondaryArm: false, flip: false, rainTotem, behaviorAtEndOfFrame: true)
						});
					}
					else
					{
						Game1.player.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[2]
						{
							new FarmerSprite.AnimationFrame(57, 2000, secondaryArm: false, flip: false),
							new FarmerSprite.AnimationFrame((short)Game1.player.FarmerSprite.CurrentFrame, 0, secondaryArm: false, flip: false, totemWarp, behaviorAtEndOfFrame: true)
						});
					}
					TemporaryAnimatedSprite sprite = new TemporaryAnimatedSprite(0, 9999f, 1, 999, Game1.player.Position + new Vector2(0f, -96f), flicker: false, flipped: false, verticalFlipped: false, 0f)
					{
						motion = new Vector2(0f, -1f),
						scaleChange = 0.01f,
						alpha = 1f,
						alphaFade = 0.0075f,
						shakeIntensity = 1f,
						initialPosition = Game1.player.Position + new Vector2(0f, -96f),
						xPeriodic = true,
						xPeriodicLoopTime = 1000f,
						xPeriodicRange = 4f,
						layerDepth = 1f
					};
					sprite.CopyAppearanceFromItemId(base.QualifiedItemId);
					Game1.multiplayer.broadcastSprites(location, sprite);
					sprite = new TemporaryAnimatedSprite(0, 9999f, 1, 999, Game1.player.Position + new Vector2(-64f, -96f), flicker: false, flipped: false, verticalFlipped: false, 0f)
					{
						motion = new Vector2(0f, -0.5f),
						scaleChange = 0.005f,
						scale = 0.5f,
						alpha = 1f,
						alphaFade = 0.0075f,
						shakeIntensity = 1f,
						delayBeforeAnimationStart = 10,
						initialPosition = Game1.player.Position + new Vector2(-64f, -96f),
						xPeriodic = true,
						xPeriodicLoopTime = 1000f,
						xPeriodicRange = 4f,
						layerDepth = 0.9999f
					};
					sprite.CopyAppearanceFromItemId(base.QualifiedItemId);
					Game1.multiplayer.broadcastSprites(location, sprite);
					sprite = new TemporaryAnimatedSprite(0, 9999f, 1, 999, Game1.player.Position + new Vector2(64f, -96f), flicker: false, flipped: false, verticalFlipped: false, 0f)
					{
						motion = new Vector2(0f, -0.5f),
						scaleChange = 0.005f,
						scale = 0.5f,
						alpha = 1f,
						alphaFade = 0.0075f,
						delayBeforeAnimationStart = 20,
						shakeIntensity = 1f,
						initialPosition = Game1.player.Position + new Vector2(64f, -96f),
						xPeriodic = true,
						xPeriodicLoopTime = 1000f,
						xPeriodicRange = 4f,
						layerDepth = 0.9988f
					};
					sprite.CopyAppearanceFromItemId(base.QualifiedItemId);
					Game1.multiplayer.broadcastSprites(location, sprite);
					Game1.screenGlowOnce(sprinkleColor, hold: false);
					Utility.addSprinklesToLocation(location, Game1.player.TilePoint.X, Game1.player.TilePoint.Y, 16, 16, 1300, 20, Color.White, null, motionTowardCenter: true);
					return true;
				}
				}
			}
		}
		else if (base.QualifiedItemId == "(O)79" || base.QualifiedItemId == "(O)842")
		{
			bool isJournal = base.QualifiedItemId == "(O)842";
			int totalNotes;
			int[] unseenNotes = Utility.GetUnseenSecretNotes(Game1.player, isJournal, out totalNotes);
			if (unseenNotes.Length == 0)
			{
				return false;
			}
			Random r = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.player.UniqueMultiplayerID, unseenNotes.Length * 777);
			int which = (isJournal ? unseenNotes.Min() : r.ChooseFrom(unseenNotes));
			if (!Game1.player.secretNotesSeen.Add(which))
			{
				return false;
			}
			switch (which)
			{
			case 23:
				if (!Game1.player.eventsSeen.Contains("2120303"))
				{
					Game1.player.addQuest("29");
				}
				break;
			case 10:
				if (!Game1.player.mailReceived.Contains("qiCave"))
				{
					Game1.player.addQuest("30");
				}
				break;
			}
			Game1.activeClickableMenu = new LetterViewerMenu(which);
			return true;
		}
		if (base.QualifiedItemId == "(O)911")
		{
			if (!normal_gameplay)
			{
				return false;
			}
			string warpError = Utility.GetHorseWarpErrorMessage(Utility.GetHorseWarpRestrictionsForFarmer(Game1.player));
			if (warpError == null)
			{
				Horse horse = null;
				foreach (NPC character in location.characters)
				{
					if (character is Horse curHorse && curHorse.getOwner() == Game1.player)
					{
						horse = curHorse;
						break;
					}
				}
				if (horse == null || Math.Abs(Game1.player.TilePoint.X - horse.TilePoint.X) > 1 || Math.Abs(Game1.player.TilePoint.Y - horse.TilePoint.Y) > 1)
				{
					Game1.player.faceDirection(2);
					Game1.MusicDuckTimer = 2000f;
					Game1.playSound("horse_flute");
					Game1.player.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[6]
					{
						new FarmerSprite.AnimationFrame(98, 400, secondaryArm: true, flip: false),
						new FarmerSprite.AnimationFrame(99, 200, secondaryArm: true, flip: false),
						new FarmerSprite.AnimationFrame(100, 200, secondaryArm: true, flip: false),
						new FarmerSprite.AnimationFrame(99, 200, secondaryArm: true, flip: false),
						new FarmerSprite.AnimationFrame(98, 400, secondaryArm: true, flip: false),
						new FarmerSprite.AnimationFrame(99, 200, secondaryArm: true, flip: false)
					});
					Game1.player.freezePause = 1500;
					DelayedAction.functionAfterDelay(delegate
					{
						string horseWarpErrorMessage = Utility.GetHorseWarpErrorMessage(Utility.GetHorseWarpRestrictionsForFarmer(Game1.player));
						if (horseWarpErrorMessage != null)
						{
							Game1.showRedMessage(horseWarpErrorMessage);
						}
						else
						{
							Game1.player.team.requestHorseWarpEvent.Fire(Game1.player.UniqueMultiplayerID);
						}
					}, 1500);
				}
				stack.Value += 1;
				return true;
			}
			Game1.showRedMessage(warpError);
		}
		if (base.QualifiedItemId == "(O)879")
		{
			if (!normal_gameplay)
			{
				return false;
			}
			Game1.player.faceDirection(2);
			Game1.player.freezePause = 1750;
			Game1.player.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[2]
			{
				new FarmerSprite.AnimationFrame(57, 750, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame((short)Game1.player.FarmerSprite.CurrentFrame, 0, secondaryArm: false, flip: false, MonsterMusk, behaviorAtEndOfFrame: true)
			});
			for (int i = 0; i < 3; i++)
			{
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(5, new Vector2(16f, -64 + 32 * i), Color.Purple)
				{
					motion = new Vector2(Utility.RandomFloat(-1f, 1f), -0.5f),
					scaleChange = 0.005f,
					scale = 0.5f,
					alpha = 1f,
					alphaFade = 0.0075f,
					shakeIntensity = 1f,
					delayBeforeAnimationStart = 100 * i,
					layerDepth = 0.9999f,
					positionFollowsAttachedCharacter = true,
					attachedCharacter = Game1.player
				});
			}
			location.playSound("steam");
			return true;
		}
		return false;
	}

	/// <inheritdoc />
	public override Color getCategoryColor()
	{
		if (type == "Arch")
		{
			return new Color(110, 0, 90);
		}
		return base.getCategoryColor();
	}

	/// <inheritdoc />
	public override string getCategoryName()
	{
		if (this is Furniture { placementRestriction: var placementRestriction })
		{
			return placementRestriction switch
			{
				1 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Furniture_Outdoors"), 
				2 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Furniture_Decoration"), 
				_ => Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12847"), 
			};
		}
		if (Type == "Arch")
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12849");
		}
		return base.getCategoryName();
	}

	/// <summary>Get the translated display name for an object category, if any.</summary>
	/// <param name="category">The object category.</param>
	/// <returns>Returns the display name, or an empty string if there is none.</returns>
	public static string GetCategoryDisplayName(int category)
	{
		switch (category)
		{
		case -97:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Boots.cs.12501");
		case -100:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:category_clothes");
		case -96:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Ring.cs.1");
		case -99:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Tool.cs.14307");
		case -12:
		case -2:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12850");
		case -75:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12851");
		case -4:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12852");
		case -25:
		case -7:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12853");
		case -79:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12854");
		case -74:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12855");
		case -19:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12856");
		case -21:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12857");
		case -22:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12858");
		case -24:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12859");
		case -20:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12860");
		case -27:
		case -26:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12862");
		case -8:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12863");
		case -18:
		case -14:
		case -6:
		case -5:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12864");
		case -80:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12866");
		case -28:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12867");
		case -16:
		case -15:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12868");
		case -81:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12869");
		case -102:
			return Game1.content.LoadString("Strings\\1_6_Strings:Book_Category");
		case -103:
			return Game1.content.LoadString("Strings\\1_6_Strings:skillBook_Category");
		default:
			return "";
		}
	}

	/// <summary>Get the display color for an object category.</summary>
	/// <param name="category">The object category.</param>
	public static Color GetCategoryColor(int category)
	{
		switch (category)
		{
		case -12:
		case -2:
			return new Color(110, 0, 90);
		case -75:
			return Color.Green;
		case -4:
			return Color.DarkBlue;
		case -7:
			return new Color(220, 60, 0);
		case -79:
			return Color.DeepPink;
		case -74:
			return Color.Brown;
		case -19:
			return Color.SlateGray;
		case -21:
			return Color.DarkRed;
		case -22:
			return Color.DarkCyan;
		case -24:
			return new Color(150, 80, 190);
		case -20:
			return Color.DimGray;
		case -27:
		case -26:
			return new Color(0, 155, 111);
		case -8:
			return new Color(148, 61, 40);
		case -18:
		case -14:
		case -6:
		case -5:
			return new Color(255, 0, 100);
		case -80:
			return new Color(219, 54, 211);
		case -28:
			return new Color(50, 10, 70);
		case -16:
		case -15:
			return new Color(64, 102, 114);
		case -81:
			return new Color(10, 130, 50);
		case -102:
			return new Color(85, 47, 27);
		case -103:
			return new Color(122, 93, 39);
		default:
			return Color.Black;
		}
	}

	public virtual bool isActionable(Farmer who)
	{
		if (!isTemporarilyInvisible)
		{
			return checkForAction(who, justCheckingForActivity: true);
		}
		return false;
	}

	public int getHealth()
	{
		return health;
	}

	public void setHealth(int health)
	{
		this.health = health;
	}

	protected virtual void grabItemFromAutoGrabber(Item item, Farmer who)
	{
		if (heldObject.Value is Chest chest)
		{
			if (who.couldInventoryAcceptThisItem(item))
			{
				chest.Items.Remove(item);
				chest.clearNulls();
				Game1.activeClickableMenu = new ItemGrabMenu(chest.Items, reverseGrab: false, showReceivingMenu: true, InventoryMenu.highlightAllItems, chest.grabItemFromInventory, null, grabItemFromAutoGrabber, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: true, 1, this, -1, this);
			}
			if (chest.isEmpty())
			{
				showNextIndex.Value = false;
			}
		}
	}

	public static bool HighlightFertilizers(Item i)
	{
		return i.Category == -19;
	}

	public override int healthRecoveredOnConsumption()
	{
		if (Edibility < 0)
		{
			return 0;
		}
		if (base.QualifiedItemId == "(O)874")
		{
			return (int)((float)staminaRecoveredOnConsumption() * 0.68f);
		}
		if (base.QualifiedItemId == "(O)349")
		{
			return 0;
		}
		if (base.QualifiedItemId == "(O)773")
		{
			return 999;
		}
		return (int)((float)staminaRecoveredOnConsumption() * 0.45f);
	}

	public override int staminaRecoveredOnConsumption()
	{
		if (base.QualifiedItemId == "(O)773")
		{
			return 0;
		}
		return (int)Math.Ceiling((double)Edibility * 2.5) + base.Quality * Edibility;
	}

	/// <summary>Perform an action when the user interacts with this object.</summary>
	/// <param name="who">The player interacting with the object.</param>
	/// <param name="justCheckingForActivity">Whether to check if an action would be performed, without actually doing it. Setting this to true may have inconsistent effects depending on the action.</param>
	/// <returns>Returns true if the action was performed, or false if the player should pick up the item instead.</returns>
	public virtual bool checkForAction(Farmer who, bool justCheckingForActivity = false)
	{
		if (isTemporarilyInvisible)
		{
			return true;
		}
		if (!justCheckingForActivity && who != null)
		{
			GameLocation location = Location;
			Point tile = who.TilePoint;
			if (location.isObjectAtTile(tile.X, tile.Y - 1) && location.isObjectAtTile(tile.X, tile.Y + 1) && location.isObjectAtTile(tile.X + 1, tile.Y) && location.isObjectAtTile(tile.X - 1, tile.Y) && !location.getObjectAtTile(tile.X, tile.Y - 1).isPassable() && !location.getObjectAtTile(tile.X, tile.Y + 1).isPassable() && !location.getObjectAtTile(tile.X - 1, tile.Y).isPassable() && !location.getObjectAtTile(tile.X + 1, tile.Y).isPassable())
			{
				performToolAction(null);
			}
		}
		switch (base.QualifiedItemId)
		{
		case "(O)PotOfGold":
			if (!justCheckingForActivity)
			{
				Game1.playSound("hammer");
				Game1.playSound("moneyDial");
				Game1.createMultipleItemDebris(ItemRegistry.Create("(O)GoldCoin", Math.Min(100, 7 + Game1.year)), TileLocation * 64f + new Vector2(32f), 1);
				Game1.createMultipleItemDebris(ItemRegistry.Create("(H)LeprechuanHat"), TileLocation * 64f + new Vector2(32f), 1);
				Location.removeObject(TileLocation, showDestroyedObject: false);
				Utility.addDirtPuffs(Location, (int)TileLocation.X, (int)TileLocation.Y, 1, 1, 3);
				Utility.addStarsAndSpirals(Location, (int)TileLocation.X, (int)TileLocation.Y, 1, 1, 100, 30, Color.White);
			}
			return true;
		case "(BC)MiniForge":
			if (!justCheckingForActivity)
			{
				Game1.activeClickableMenu = new ForgeMenu();
			}
			return true;
		case "(BC)StatueOfTheDwarfKing":
			if (!justCheckingForActivity)
			{
				if (who.stats.Get(StatKeys.Mastery(3)) < 1)
				{
					Game1.showGlobalMessage(Game1.content.LoadString("Strings\\1_6_Strings:MasteryRequirement"));
					Game1.playSound("cancel");
				}
				else if (!who.hasBuffWithNameContainingString("dwarfStatue"))
				{
					Game1.activeClickableMenu = new ChooseFromIconsMenu("dwarfStatue");
					(Game1.activeClickableMenu as ChooseFromIconsMenu).sourceObject = this;
				}
				else
				{
					shakeTimer = 400;
					Game1.playSound("cancel");
				}
			}
			return true;
		case "(BC)StatueOfBlessings":
			return CheckForActionOnBlessedStatue(who, who.currentLocation, justCheckingForActivity);
		case "(BC)0":
		case "(BC)1":
		case "(BC)2":
		case "(BC)3":
		case "(BC)4":
		case "(BC)5":
		case "(BC)6":
		case "(BC)7":
			return CheckForActionOnHousePlant(who, justCheckingForActivity);
		case "(BC)56":
			return CheckForActionOnSlimeBall(who, justCheckingForActivity);
		case "(BC)71":
			return CheckForActionOnStaircase(who, justCheckingForActivity);
		case "(BC)94":
			return CheckForActionOnSingingStone(who, justCheckingForActivity);
		case "(BC)99":
			return CheckForActionOnFeedHopper(who, justCheckingForActivity);
		case "(BC)141":
			return CheckForActionOnPrairieKingArcadeSystem(who, justCheckingForActivity);
		case "(BC)159":
			return CheckForActionOnJunimoKartArcadeSystem(who, justCheckingForActivity);
		case "(BC)165":
			return CheckForActionOnAutoGrabber(who, justCheckingForActivity);
		case "(BC)238":
			return CheckForActionOnMiniObelisk(who, justCheckingForActivity);
		case "(BC)239":
			return CheckForActionOnFarmComputer(who, justCheckingForActivity);
		case "(BC)247":
			return CheckForActionOnSewingMachine(who, justCheckingForActivity);
		case "(BC)TextSign":
			return CheckForActionOnTextSign(who, justCheckingForActivity);
		case "(O)464":
			return CheckForActionOnFluteBlock(who, justCheckingForActivity);
		case "(O)463":
			return CheckForActionOnDrumBlock(who, justCheckingForActivity);
		default:
			if (IsSprinkler() && CheckForActionOnSprinkler(who, justCheckingForActivity))
			{
				return true;
			}
			if (IsScarecrow() && CheckForActionOnScarecrow(who, justCheckingForActivity))
			{
				return true;
			}
			return CheckForActionOnMachine(who, justCheckingForActivity);
		}
	}

	/// <summary>Perform an action when the user interacts with this object, assuming it's a sewing machine.</summary>
	/// <inheritdoc cref="M:StardewValley.Object.checkForAction(StardewValley.Farmer,System.Boolean)" />
	protected virtual bool CheckForActionOnSewingMachine(Farmer who, bool justCheckingForActivity = false)
	{
		if (justCheckingForActivity)
		{
			return true;
		}
		Game1.activeClickableMenu = new TailoringMenu();
		return true;
	}

	/// <summary>Perform an action when the user interacts with this object, assuming it's an auto-grabber.</summary>
	/// <inheritdoc cref="M:StardewValley.Object.checkForAction(StardewValley.Farmer,System.Boolean)" />
	protected virtual bool CheckForActionOnAutoGrabber(Farmer who, bool justCheckingForActivity = false)
	{
		if (justCheckingForActivity)
		{
			return true;
		}
		if (heldObject.Value is Chest chest && !chest.isEmpty())
		{
			Game1.activeClickableMenu = new ItemGrabMenu(chest.Items, reverseGrab: false, showReceivingMenu: true, InventoryMenu.highlightAllItems, chest.grabItemFromInventory, null, grabItemFromAutoGrabber, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: true, 1, null, -1, this);
			return true;
		}
		return false;
	}

	/// <summary>Perform an action when the user interacts with this object, assuming it's a farm computer.</summary>
	/// <inheritdoc cref="M:StardewValley.Object.checkForAction(StardewValley.Farmer,System.Boolean)" />
	protected virtual bool CheckForActionOnFarmComputer(Farmer who, bool justCheckingForActivity = false)
	{
		if (justCheckingForActivity)
		{
			return true;
		}
		shakeTimer = 500;
		Location.localSound("DwarvishSentry");
		who.freezePause = 500;
		DelayedAction.functionAfterDelay(delegate
		{
			ShowFarmComputerReport(who);
		}, 500);
		return true;
	}

	/// <summary>Show a farm computer analysis for a player's current location.</summary>
	/// <param name="who">The player viewing the farm report.</param>
	protected virtual void ShowFarmComputerReport(Farmer who)
	{
		GameLocation location = (Location ?? who.currentLocation).GetRootLocation();
		Farm farm = location as Farm;
		bool num = location.IsBuildableLocation() || location.buildings.Any();
		string locationDisplayName = location.GetDisplayName();
		int totalCrops = location.getTotalCrops();
		int totalOpenHoeDirt = location.getTotalOpenHoeDirt();
		int numCropsToHarvest = location.getTotalCropsReadyForHarvest();
		int numUnwateredCrops = location.getTotalUnwateredCrops();
		int? numGreenhouseCropsToHarvest = (location.HasMinBuildings("Greenhouse", 1) ? location.getTotalGreenhouseCropsReadyForHarvest() : null);
		int numForage = location.getTotalForageItems();
		int anyMachinesReady = location.getNumberOfMachinesReadyForHarvest();
		bool? farmCaveNeedsHarvest = farm?.doesFarmCaveNeedHarvesting();
		StringBuilder report = new StringBuilder();
		if (location is Farm)
		{
			report.Append(Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_Intro_Farm", Game1.player.farmName.Value));
		}
		else if (!string.IsNullOrWhiteSpace(locationDisplayName))
		{
			report.Append(Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_Intro_NamedLocation", locationDisplayName));
		}
		else
		{
			report.Append(Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_Intro_Generic"));
		}
		report.Append("^--------------^");
		if (num)
		{
			report.Append(Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_PiecesHay", location.piecesOfHay, location.GetHayCapacity())).Append(" ^");
		}
		report.Append(Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_TotalCrops", totalCrops)).Append("  ^").Append(Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_CropsReadyForHarvest", numCropsToHarvest))
			.Append("  ^")
			.Append(Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_CropsUnwatered", numUnwateredCrops))
			.Append("  ^");
		if (numGreenhouseCropsToHarvest.HasValue)
		{
			report.Append(Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_CropsReadyForHarvest_Greenhouse", numGreenhouseCropsToHarvest)).Append("  ^");
		}
		report.Append(Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_TotalOpenHoeDirt", totalOpenHoeDirt)).Append("  ^");
		if (farm == null || farm.SpawnsForage())
		{
			report.Append(Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_TotalForage", numForage)).Append("  ^");
		}
		report.Append(Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_MachinesReady", anyMachinesReady)).Append("  ^");
		if (farmCaveNeedsHarvest.HasValue)
		{
			report.Append(Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_FarmCave", farmCaveNeedsHarvest.Value ? Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_Yes") : Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_No")));
		}
		Game1.multipleDialogues(new string[1] { report.ToString() });
	}

	/// <summary>Perform an action when the user interacts with this object, assuming it's a mini-obelisk.</summary>
	/// <inheritdoc cref="M:StardewValley.Object.checkForAction(StardewValley.Farmer,System.Boolean)" />
	protected virtual bool CheckForActionOnMiniObelisk(Farmer who, bool justCheckingForActivity = false)
	{
		if (justCheckingForActivity)
		{
			return true;
		}
		GameLocation location = Location;
		Vector2 obelisk1 = Vector2.Zero;
		Vector2 obelisk2 = Vector2.Zero;
		foreach (KeyValuePair<Vector2, Object> o in location.objects.Pairs)
		{
			if ((bool)o.Value.bigCraftable && o.Value.QualifiedItemId == "(BC)238")
			{
				if (obelisk1 == Vector2.Zero)
				{
					obelisk1 = o.Key;
				}
				else if (obelisk2 == Vector2.Zero)
				{
					obelisk2 = o.Key;
					break;
				}
			}
		}
		if (obelisk2 == Vector2.Zero)
		{
			Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:MiniObelisk_NeedsPair"));
			return false;
		}
		Vector2 target = ((Vector2.Distance(who.Tile, obelisk1) > Vector2.Distance(who.Tile, obelisk2)) ? obelisk1 : obelisk2);
		Vector2[] array = new Vector2[4]
		{
			new Vector2(target.X, target.Y + 1f),
			new Vector2(target.X - 1f, target.Y),
			new Vector2(target.X + 1f, target.Y),
			new Vector2(target.X, target.Y - 1f)
		};
		foreach (Vector2 v in array)
		{
			if (!location.IsTileBlockedBy(v, CollisionMask.All, CollisionMask.All))
			{
				for (int i = 0; i < 12; i++)
				{
					location.temporarySprites.Add(new TemporaryAnimatedSprite(354, Game1.random.Next(25, 75), 6, 1, new Vector2(Game1.random.Next((int)who.Position.X - 256, (int)who.Position.X + 192), Game1.random.Next((int)who.Position.Y - 256, (int)who.Position.Y + 192)), flicker: false, Game1.random.NextBool()));
				}
				location.playSound("wand");
				Game1.displayFarmer = false;
				Game1.player.freezePause = 50;
				Game1.flashAlpha = 1f;
				DelayedAction.fadeAfterDelay(delegate
				{
					who.setTileLocation(v);
					Game1.displayFarmer = true;
					Game1.globalFadeToClear();
				}, 50);
				Microsoft.Xna.Framework.Rectangle playerBounds = who.GetBoundingBox();
				new Microsoft.Xna.Framework.Rectangle(playerBounds.X, playerBounds.Y, 64, 64).Inflate(192, 192);
				int j = 0;
				Point playerTile = who.TilePoint;
				for (int x = playerTile.X + 8; x >= playerTile.X - 8; x--)
				{
					location.temporarySprites.Add(new TemporaryAnimatedSprite(6, new Vector2(x, playerTile.Y) * 64f, Color.White, 8, flipped: false, 50f)
					{
						layerDepth = 1f,
						delayBeforeAnimationStart = j * 25,
						motion = new Vector2(-0.25f, 0f)
					});
					j++;
				}
				return true;
			}
		}
		Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:MiniObelisk_NeedsSpace"));
		return false;
	}

	/// <summary>Perform an action when the user interacts with this object, assuming it's a Prairie King arcade system.</summary>
	/// <inheritdoc cref="M:StardewValley.Object.checkForAction(StardewValley.Farmer,System.Boolean)" />
	protected virtual bool CheckForActionOnPrairieKingArcadeSystem(Farmer who, bool justCheckingForActivity = false)
	{
		if (justCheckingForActivity)
		{
			return true;
		}
		Location.showPrairieKingMenu();
		return true;
	}

	/// <summary>Perform an action when the user interacts with this object, assuming it's a Junimo Kart arcade system.</summary>
	/// <inheritdoc cref="M:StardewValley.Object.checkForAction(StardewValley.Farmer,System.Boolean)" />
	protected virtual bool CheckForActionOnJunimoKartArcadeSystem(Farmer who, bool justCheckingForActivity = false)
	{
		if (justCheckingForActivity)
		{
			return true;
		}
		Response[] responses = new Response[3]
		{
			new Response("Progress", Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12873")),
			new Response("Endless", Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12875")),
			new Response("Exit", Game1.content.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11738"))
		};
		Location.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Minecart_Menu"), responses, "MinecartGame");
		return true;
	}

	/// <summary>Perform an action when the user interacts with this object, assuming it's a staircase.</summary>
	/// <inheritdoc cref="M:StardewValley.Object.checkForAction(StardewValley.Farmer,System.Boolean)" />
	protected virtual bool CheckForActionOnStaircase(Farmer who, bool justCheckingForActivity = false)
	{
		if (Location is MineShaft mine && mine.shouldCreateLadderOnThisLevel())
		{
			if (justCheckingForActivity)
			{
				return true;
			}
			Game1.enterMine(Game1.CurrentMineLevel + 1);
			Game1.playSound("stairsdown");
		}
		else if (Location.Name.Equals("ManorHouse"))
		{
			if (justCheckingForActivity)
			{
				return true;
			}
			Game1.warpFarmer("LewisBasement", 4, 4, 2);
			Game1.playSound("stairsdown");
		}
		return false;
	}

	/// <summary>Perform an action when the user interacts with this object, assuming it's a slime ball.</summary>
	/// <inheritdoc cref="M:StardewValley.Object.checkForAction(StardewValley.Farmer,System.Boolean)" />
	protected virtual bool CheckForActionOnSlimeBall(Farmer who, bool justCheckingForActivity = false)
	{
		if (justCheckingForActivity)
		{
			return true;
		}
		GameLocation location = Location;
		location.objects.Remove(tileLocation.Value);
		DelayedAction.playSoundAfterDelay("slimedead", 40);
		DelayedAction.playSoundAfterDelay("slimeHit", 100);
		location.playSound("slimeHit");
		Random r = Utility.CreateRandom(Game1.stats.DaysPlayed, Game1.uniqueIDForThisGame, (double)tileLocation.X * 77.0, (double)tileLocation.Y * 777.0, 2.0);
		Game1.createMultipleObjectDebris("(O)766", (int)tileLocation.X, (int)tileLocation.Y, r.Next(10, 21), 1f + ((who.FacingDirection == 2) ? 0f : ((float)Game1.random.NextDouble())));
		Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(44, tileLocation.Value * 64f, Color.Lime, 10)
		{
			interval = 70f,
			holdLastFrame = true,
			alphaFade = 0.01f
		}, location);
		Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(44, tileLocation.Value * 64f + new Vector2(-16f, 0f), Color.Lime, 10)
		{
			interval = 70f,
			delayBeforeAnimationStart = 0,
			holdLastFrame = true,
			alphaFade = 0.01f
		});
		Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(44, tileLocation.Value * 64f + new Vector2(0f, 16f), Color.Lime, 10)
		{
			interval = 70f,
			delayBeforeAnimationStart = 100,
			holdLastFrame = true,
			alphaFade = 0.01f
		});
		Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(44, tileLocation.Value * 64f + new Vector2(16f, 0f), Color.Lime, 10)
		{
			interval = 70f,
			delayBeforeAnimationStart = 200,
			holdLastFrame = true,
			alphaFade = 0.01f
		});
		while (r.NextDouble() < 0.33)
		{
			Game1.createObjectDebris("(O)557", (int)tileLocation.X, (int)tileLocation.Y, who.UniqueMultiplayerID);
		}
		return true;
	}

	protected virtual bool CheckForActionOnBlessedStatue(Farmer who, GameLocation location, bool justCheckingForActivitiy = false)
	{
		if (who.stats.Get(StatKeys.Mastery(0)) < 1 && !justCheckingForActivitiy)
		{
			Game1.showGlobalMessage(Game1.content.LoadString("Strings\\1_6_Strings:MasteryRequirement"));
			Game1.playSound("cancel");
			return true;
		}
		if (!who.hasBuffWithNameContainingString("statue_of_blessings_") && !who.hasBeenBlessedByStatueToday)
		{
			if (justCheckingForActivitiy)
			{
				return true;
			}
			who.hasBeenBlessedByStatueToday = true;
			Random r = Utility.CreateDaySaveRandom(Game1.stats.DaysPlayed * 777);
			for (int i = 0; i < 8; i++)
			{
				r.Next();
			}
			who.applyBuff("statue_of_blessings_" + r.Next((Game1.isRaining || Utility.isFestivalDay()) ? 6 : 7));
			Game1.playSound("statue_of_blessings");
			showNextIndex.Value = true;
			if (location.critters == null)
			{
				location.critters = new List<Critter>();
			}
			location.critters.Add(new Butterfly(location, TileLocation + new Vector2(1f, 0f), islandButterfly: false, forceSummerButterfly: false, 163));
			location.critters.Add(new Butterfly(location, TileLocation + new Vector2(0.33f, 0.25f), islandButterfly: false, forceSummerButterfly: false, 163));
			location.critters.Add(new Butterfly(location, TileLocation + new Vector2(1.58f, 0.25f), islandButterfly: false, forceSummerButterfly: false, 163));
			location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(221, 225, 15, 31), 9000f, 1, 1, TileLocation * 64f + new Vector2(1f, -16f) * 4f, flicker: false, flipped: false, Math.Max(0f, ((TileLocation.Y + 1f) * 64f - 20f) / 10000f) + TileLocation.X * 1E-05f, 0.02f, Color.White, 4f, 0f, 0f, 0f));
			for (int i = 0; i < 6; i++)
			{
				Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(144, 249, 7, 7), Game1.random.Next(100, 200), 6, 1, TileLocation * 64f + new Vector2(32 + Game1.random.Next(-64, 64), Game1.random.Next(-64, 64)), flicker: false, flipped: false, Math.Max(0f, ((TileLocation.Y + 1f) * 64f - 24f) / 10000f) + TileLocation.X * 1E-05f, 0f, (Game1.random.NextDouble() < 0.5) ? new Color(255, 180, 210) : Color.White, 4f, 0f, 0f, 0f), location);
			}
			return true;
		}
		return false;
	}

	/// <summary>Perform an action when the user interacts with this object, assuming it's a house plant.</summary>
	/// <inheritdoc cref="M:StardewValley.Object.checkForAction(StardewValley.Farmer,System.Boolean)" />
	protected virtual bool CheckForActionOnHousePlant(Farmer who, bool justCheckingForActivity = false)
	{
		if (justCheckingForActivity)
		{
			return true;
		}
		base.ParentSheetIndex++;
		int total = -1;
		int baseIndex = -1;
		if (name == "House Plant")
		{
			total = 8;
			baseIndex = 0;
		}
		if (base.ParentSheetIndex == baseIndex + total)
		{
			base.ParentSheetIndex -= total;
			return false;
		}
		return true;
	}

	/// <summary>Perform an action when the user interacts with this object, assuming it's a flute block.</summary>
	/// <inheritdoc cref="M:StardewValley.Object.checkForAction(StardewValley.Farmer,System.Boolean)" />
	protected virtual bool CheckForActionOnFluteBlock(Farmer who, bool justCheckingForActivity = false)
	{
		if (justCheckingForActivity)
		{
			return true;
		}
		int.TryParse(preservedParentSheetIndex.Value, out var preservedParentSheetInt);
		preservedParentSheetInt = preservedParentSheetInt switch
		{
			2300 => 2400, 
			2400 => 0, 
			_ => (preservedParentSheetInt + 100) % 2400, 
		};
		preservedParentSheetIndex.Value = preservedParentSheetInt.ToString();
		shakeTimer = 200;
		string sound = "flute";
		if (who.ActiveObject != null)
		{
			sound = getFluteBlockSoundFromHeldObject(who.ActiveObject);
		}
		internalSound?.Stop(AudioStopOptions.Immediate);
		Game1.playSound(sound, preservedParentSheetInt, out internalSound);
		scale.Y = 1.3f;
		shakeTimer = 200;
		return true;
	}

	/// <summary>Perform an action when the user interacts with this object, assuming it's a drum block.</summary>
	/// <inheritdoc cref="M:StardewValley.Object.checkForAction(StardewValley.Farmer,System.Boolean)" />
	protected virtual bool CheckForActionOnDrumBlock(Farmer who, bool justCheckingForActivity = false)
	{
		if (justCheckingForActivity)
		{
			return true;
		}
		int.TryParse(preservedParentSheetIndex.Value, out var preservedParentSheetInt);
		preservedParentSheetInt = (preservedParentSheetInt + 1) % 7;
		preservedParentSheetIndex.Value = preservedParentSheetInt.ToString();
		shakeTimer = 200;
		internalSound?.Stop(AudioStopOptions.Immediate);
		Game1.playSound("drumkit" + preservedParentSheetInt, out internalSound);
		scale.Y = 1.3f;
		shakeTimer = 200;
		return true;
	}

	/// <summary>Perform an action when the user interacts with this object, assuming it's a sprinkler.</summary>
	/// <inheritdoc cref="M:StardewValley.Object.checkForAction(StardewValley.Farmer,System.Boolean)" />
	protected bool CheckForActionOnSprinkler(Farmer who, bool justCheckingForActivity = false)
	{
		if (heldObject.Value != null && heldObject.Value.QualifiedItemId == "(O)913")
		{
			if (justCheckingForActivity)
			{
				return true;
			}
			if (!Game1.didPlayerJustRightClick(ignoreNonMouseHeldInput: true))
			{
				return false;
			}
			if (heldObject.Value.heldObject.Value is Chest chest)
			{
				chest.GetMutex().RequestLock(chest.ShowMenu);
				return true;
			}
		}
		return false;
	}

	/// <summary>Perform an action when the user interacts with this object, assuming it's a scarecrow.</summary>
	/// <inheritdoc cref="M:StardewValley.Object.checkForAction(StardewValley.Farmer,System.Boolean)" />
	protected bool CheckForActionOnScarecrow(Farmer who, bool justCheckingForActivity = false)
	{
		if (justCheckingForActivity)
		{
			return true;
		}
		if (base.QualifiedItemId == "(BC)126" && who.CurrentItem is Hat hat)
		{
			shakeTimer = 100;
			if ((int)quality != 0)
			{
				Game1.createItemDebris(ItemRegistry.Create("(H)" + ((int)quality - 1)), tileLocation.Value * 64f, (who.FacingDirection + 2) % 4);
				quality.Value = 0;
			}
			if (preservedParentSheetIndex.Value != null)
			{
				Game1.createItemDebris(new Hat(preservedParentSheetIndex.Value), tileLocation.Value * 64f, (who.FacingDirection + 2) % 4);
			}
			preservedParentSheetIndex.Value = hat.ItemId;
			who.Items[who.CurrentToolIndex] = null;
			Location.playSound("dirtyHit");
			return true;
		}
		if (!Game1.didPlayerJustRightClick(ignoreNonMouseHeldInput: true))
		{
			return false;
		}
		shakeTimer = 100;
		if (base.SpecialVariable == 0)
		{
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12926"));
		}
		else
		{
			Game1.drawObjectDialogue((base.SpecialVariable == 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12927") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12929", base.SpecialVariable));
		}
		return true;
	}

	/// <summary>Perform an action when the user interacts with this object, assuming it's a singing stone.</summary>
	/// <inheritdoc cref="M:StardewValley.Object.checkForAction(StardewValley.Farmer,System.Boolean)" />
	protected bool CheckForActionOnSingingStone(Farmer who, bool justCheckingForActivity = false)
	{
		if (justCheckingForActivity)
		{
			return true;
		}
		int pitch = Game1.random.Next(2400);
		pitch -= pitch % 100;
		Game1.playSound("crystal", pitch);
		shakeTimer = 100;
		return true;
	}

	/// <summary>Perform an action when the user interacts with this object, assuming it's a text sign.</summary>
	/// <inheritdoc cref="M:StardewValley.Object.checkForAction(StardewValley.Farmer,System.Boolean)" />
	protected virtual bool CheckForActionOnTextSign(Farmer who, bool justCheckingForActivity = false)
	{
		if (justCheckingForActivity)
		{
			return true;
		}
		if (Game1.activeClickableMenu == null)
		{
			TitleTextInputMenu signMenu = new TitleTextInputMenu(Game1.content.LoadString("Strings\\UI:TextSignEntry"), null, signText.Value, null);
			signMenu.pasteButton.visible = false;
			signMenu.doneNaming = delegate(string text)
			{
				signText.Value = text.Trim();
				signMenu.exitThisMenu();
				if (!string.IsNullOrEmpty(signText.Value))
				{
					showNextIndex.Value = false;
				}
				else
				{
					showNextIndex.Value = true;
				}
			};
			signMenu.textBox.textLimit = 60;
			Game1.activeClickableMenu = signMenu;
			return true;
		}
		return false;
	}

	/// <summary>Perform an action when the user interacts with this object, assuming it's a feed hopper.</summary>
	/// <inheritdoc cref="M:StardewValley.Object.checkForAction(StardewValley.Farmer,System.Boolean)" />
	protected bool CheckForActionOnFeedHopper(Farmer who, bool justCheckingForActivity = false)
	{
		if (justCheckingForActivity)
		{
			return true;
		}
		if (who.ActiveObject != null)
		{
			return false;
		}
		if (who.freeSpotsInInventory() > 0)
		{
			GameLocation location = Location;
			GameLocation rootLocation = location.GetRootLocation();
			int piecesHay = rootLocation.piecesOfHay;
			if (piecesHay > 0)
			{
				bool shouldReturn = false;
				if (location is AnimalHouse l)
				{
					int piecesOfHayToRemove = Math.Min(l.animalsThatLiveHere.Count, piecesHay);
					piecesOfHayToRemove = Math.Max(1, piecesOfHayToRemove);
					int alreadyHay = l.numberOfObjectsWithName("Hay");
					piecesOfHayToRemove = Math.Min(piecesOfHayToRemove, (int)l.animalLimit - alreadyHay);
					if (piecesOfHayToRemove != 0 && Game1.player.couldInventoryAcceptThisItem("(O)178", piecesOfHayToRemove))
					{
						rootLocation.piecesOfHay.Value -= Math.Max(1, piecesOfHayToRemove);
						who.addItemToInventoryBool(ItemRegistry.Create("(O)178", piecesOfHayToRemove));
						Game1.playSound("shwip");
						shouldReturn = true;
					}
				}
				else if (Game1.player.couldInventoryAcceptThisItem("(O)178", 1))
				{
					rootLocation.piecesOfHay.Value--;
					who.addItemToInventoryBool(ItemRegistry.Create("(O)178"));
					Game1.playSound("shwip");
				}
				if ((int)rootLocation.piecesOfHay <= 0)
				{
					showNextIndex.Value = false;
				}
				return true;
			}
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12942"));
		}
		else
		{
			Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
		}
		return true;
	}

	/// <summary>Perform an action when the user interacts with this object, assuming it's a machine.</summary>
	/// <inheritdoc cref="M:StardewValley.Object.checkForAction(StardewValley.Farmer,System.Boolean)" />
	protected bool CheckForActionOnMachine(Farmer who, bool justCheckingForActivity = false)
	{
		GameLocation location = Location;
		MachineData machineData;
		if ((bool)readyForHarvest)
		{
			if (justCheckingForActivity)
			{
				return true;
			}
			if (who.isMoving())
			{
				Game1.haltAfterCheck = false;
			}
			machineData = GetMachineData();
			if (lastOutputRuleId.Value != null)
			{
				MachineOutputRule outputRule = machineData.OutputRules?.FirstOrDefault((MachineOutputRule p) => p.Id == lastOutputRuleId.Value);
				if (outputRule != null && outputRule.RecalculateOnCollect)
				{
					Object prevOutput = heldObject.Value;
					heldObject.Value = null;
					OutputMachine(machineData, outputRule, lastInputItem.Value, who, location, probe: false);
					if (heldObject.Value == null)
					{
						heldObject.Value = prevOutput;
					}
				}
			}
			bool check_for_reload = false;
			Object objectThatWasHeld = heldObject.Value;
			if (who.IsLocalPlayer)
			{
				heldObject.Value = null;
				if (!who.addItemToInventoryBool(objectThatWasHeld))
				{
					heldObject.Value = objectThatWasHeld;
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
					return false;
				}
				Game1.playSound("coin");
				check_for_reload = true;
				MachineDataUtility.UpdateStats(machineData?.StatsToIncrementWhenHarvested, objectThatWasHeld, objectThatWasHeld.Stack);
			}
			heldObject.Value = null;
			readyForHarvest.Value = false;
			showNextIndex.Value = false;
			ResetParentSheetIndex();
			if (MachineDataUtility.TryGetMachineOutputRule(this, machineData, MachineOutputTrigger.OutputCollected, objectThatWasHeld.getOne(), who, location, out var outputCollectedRule, out var _, out var _, out var _))
			{
				OutputMachine(machineData, outputCollectedRule, lastInputItem.Value, who, location, probe: false);
			}
			if (IsTapper() && location.terrainFeatures.TryGetValue(tileLocation.Value, out var terrainFeature) && terrainFeature is Tree tree)
			{
				tree.UpdateTapperProduct(this, objectThatWasHeld);
			}
			if (machineData != null && machineData.ExperienceGainOnHarvest != null)
			{
				string[] expSplit = machineData.ExperienceGainOnHarvest.Split(' ');
				for (int i = 0; i < expSplit.Length; i += 2)
				{
					int skill = Farmer.getSkillNumberFromName(expSplit[i]);
					if (skill != -1 && ArgUtility.TryGetInt(expSplit, i + 1, out var amount, out var _))
					{
						who.gainExperience(skill, amount);
					}
				}
			}
			if (check_for_reload)
			{
				AttemptAutoLoad(who);
			}
			return true;
		}
		machineData = GetMachineData();
		if (machineData != null && machineData.InteractMethod != null)
		{
			if (StaticDelegateBuilder.TryCreateDelegate<MachineInteractDelegate>(machineData.InteractMethod, out var method, out var error))
			{
				if (!justCheckingForActivity)
				{
					return method(this, location, who);
				}
				return true;
			}
			Game1.log.Warn($"Machine {base.ItemId} has invalid interaction method '{machineData.InteractMethod}': {error}");
		}
		return false;
	}

	/// <summary>Play a sound for the current player only if they're near this object.</summary>
	/// <param name="audioName">The sound ID to play.</param>
	/// <param name="pitch">The pitch modifier to apply, or <c>null</c> to keep it as-is.</param>
	/// <param name="context">The source which triggered the sound.</param>
	public void playNearbySoundLocal(string audioName, int? pitch = null, SoundContext context = SoundContext.Default)
	{
		Game1.sounds.PlayLocal(audioName, Location, tileLocation.Value, pitch, context, out var _);
	}

	/// <summary>Play a sound for each nearby online player.</summary>
	/// <param name="audioName">The sound ID to play.</param>
	/// <param name="pitch">The pitch modifier to apply, or <c>null</c> to keep it as-is.</param>
	/// <param name="context">The source which triggered the sound.</param>
	public void playNearbySoundAll(string audioName, int? pitch = null, SoundContext context = SoundContext.Default)
	{
		Game1.sounds.PlayAll(audioName, Location, tileLocation.Value, pitch, context);
	}

	public virtual bool IsScarecrow()
	{
		if (HasContextTag("crow_scare"))
		{
			return true;
		}
		return Name.Contains("arecrow");
	}

	public virtual int GetRadiusForScarecrow()
	{
		foreach (string context_tag in GetContextTags())
		{
			if (context_tag != null && context_tag.StartsWith("crow_scare_radius_") && int.TryParse(context_tag.Substring("crow_scare_radius".Length + 1), out var radius))
			{
				return radius;
			}
		}
		if (Name.StartsWith("Deluxe"))
		{
			return 17;
		}
		return 9;
	}

	/// <summary>If there's a chest above this object, try to auto-load the held object from the chest.</summary>
	/// <param name="who">The player interacting with the machine, if applicable.</param>
	/// <returns>If a chest is found, this method will acquire a mutex lock so the auto-load may not happen during the same tick. The returned task will complete once the auto-load happens, and contain true (an item was loaded) or false (no item was loaded).</returns>
	public virtual Task<bool> AttemptAutoLoad(Farmer who)
	{
		GameLocation location = Location;
		if (location != null && location.objects.TryGetValue(new Vector2(TileLocation.X, TileLocation.Y - 1f), out var fromObj))
		{
			Chest chest = fromObj as Chest;
			if (chest != null && chest.specialChestType.Value == Chest.SpecialChestTypes.AutoLoader)
			{
				TaskCompletionSource<bool> taskSource = new TaskCompletionSource<bool>();
				chest.GetMutex().RequestLock(delegate
				{
					try
					{
						chest.GetMutex().ReleaseLock();
						bool result = AttemptAutoLoad(chest.Items, who);
						taskSource.SetResult(result);
					}
					catch (Exception exception)
					{
						taskSource.SetException(exception);
					}
				});
				return taskSource.Task;
			}
		}
		return Task.FromResult(result: false);
	}

	/// <summary>Try to auto-load the held object from the given inventory.</summary>
	/// <param name="inventory">The inventory from which to take items.</param>
	/// <param name="who">The player interacting with the machine, if applicable.</param>
	public virtual bool AttemptAutoLoad(IInventory inventory, Farmer who)
	{
		if (heldObject.Value != null)
		{
			return false;
		}
		autoLoadFrom = inventory;
		foreach (Item item in inventory)
		{
			if (performObjectDropInAction(item, probe: false, who))
			{
				autoLoadFrom = null;
				return true;
			}
		}
		autoLoadFrom = null;
		return false;
	}

	private string getFluteBlockSoundFromHeldObject(Object o)
	{
		switch (o.QualifiedItemId)
		{
		case "(O)797":
		case "(O)372":
			return "clam_tone";
		case "(BC)214":
			return "telephone_buttonPush";
		case "(O)66":
			return "miniharp_note";
		case "(O)430":
			return "pig";
		case "(O)80":
		case "(O)577":
		case "(O)578":
		case "(O)338":
			return "crystal";
		case "(O)444":
			return "Duck";
		case "(O)746":
		case "(O)769":
			return "toyPiano";
		case "(O)382":
			return "dustMeep";
		default:
			return "flute";
		}
	}

	public virtual void farmerAdjacentAction(Farmer who, bool diagonal = false)
	{
		if (name == null || isTemporarilyInvisible)
		{
			return;
		}
		GameLocation location = Location;
		switch (base.QualifiedItemId)
		{
		case "(BC)TextSign":
			hovering = true;
			return;
		case "(O)464":
			if ((internalSound == null || ((int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds - lastNoteBlockSoundTime >= 1000 && !internalSound.IsPlaying)) && !Game1.dialogueUp && !diagonal)
			{
				int.TryParse(preservedParentSheetIndex.Value, out var preservedParentSheetInt);
				string sound = "flute";
				if (who.ActiveObject != null)
				{
					sound = getFluteBlockSoundFromHeldObject(who.ActiveObject);
				}
				Game1.playSound(sound, preservedParentSheetInt, out internalSound);
				scale.Y = 1.3f;
				shakeTimer = 200;
				lastNoteBlockSoundTime = (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds;
				if (location is IslandSouthEast islandSouthEast)
				{
					islandSouthEast.OnFlutePlayed(preservedParentSheetInt);
				}
			}
			return;
		case "(O)463":
			if ((internalSound == null || (Game1.currentGameTime.TotalGameTime.TotalMilliseconds - (double)lastNoteBlockSoundTime >= 1000.0 && !internalSound.IsPlaying)) && !Game1.dialogueUp && !diagonal)
			{
				int.TryParse(preservedParentSheetIndex.Value, out var preservedParentSheetInt);
				Game1.playSound("drumkit" + preservedParentSheetInt, out internalSound);
				scale.Y = 1.3f;
				shakeTimer = 200;
				lastNoteBlockSoundTime = (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds;
			}
			return;
		case "(BC)29":
		{
			if (diagonal)
			{
				return;
			}
			scale.X += 1f;
			if (scale.X > 30f)
			{
				base.ParentSheetIndex = ((base.ParentSheetIndex == 29) ? 30 : 29);
				scale.X = 0f;
				scale.Y += 2f;
			}
			if (!(scale.Y >= 20f) || !(Game1.random.NextDouble() < 0.0001) || location.characters.Count >= 4)
			{
				return;
			}
			Vector2 playerPos = Game1.player.Tile;
			Vector2[] adjacentTilesOffsets = Character.AdjacentTilesOffsets;
			foreach (Vector2 offset in adjacentTilesOffsets)
			{
				Vector2 v = playerPos + offset;
				if (!location.IsTileOccupiedBy(v) && location.isTilePassable(new Location((int)v.X, (int)v.Y), Game1.viewport) && location.isCharacterAtTile(v) == null)
				{
					if (Game1.random.NextDouble() < 0.1)
					{
						location.characters.Add(new GreenSlime(v * new Vector2(64f, 64f)));
					}
					else if (Game1.random.NextBool())
					{
						location.characters.Add(new ShadowGuy(v * new Vector2(64f, 64f)));
					}
					else
					{
						location.characters.Add(new ShadowGirl(v * new Vector2(64f, 64f)));
					}
					((Monster)location.characters[location.characters.Count - 1]).moveTowardPlayerThreshold.Value = 4;
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(352, 400f, 2, 1, v * new Vector2(64f, 64f), flicker: false, flipped: false));
					location.playSound("shadowpeep");
					break;
				}
			}
			return;
		}
		}
		if (!diagonal)
		{
			Vector2 v = new Vector2(TileLocation.X, TileLocation.Y - 1f);
			if (Location.objects.ContainsKey(v) && Location.objects[v].ItemId == "TextSign")
			{
				Location.objects[v].hovering = true;
			}
		}
	}

	public virtual void addWorkingAnimation()
	{
		GameLocation environment = Location;
		if (environment == null || !environment.farmers.Any())
		{
			return;
		}
		MachineData machineData = GetMachineData();
		if (machineData?.WorkingEffects == null)
		{
			return;
		}
		foreach (MachineEffects effect in machineData.WorkingEffects)
		{
			if (PlayMachineEffect(effect))
			{
				break;
			}
		}
	}

	public virtual void onReadyForHarvest()
	{
	}

	/// <summary>Update the object when the time of day changes.</summary>
	/// <param name="minutes">The number of minutes that passed.</param>
	/// <returns>Returns <c>true</c> if the object should be removed, else <c>false</c>.</returns>
	public virtual bool minutesElapsed(int minutes)
	{
		GameLocation environment = Location;
		if (environment == null)
		{
			return false;
		}
		if (heldObject.Value != null && base.QualifiedItemId != "(BC)165")
		{
			if (IsSprinkler())
			{
				return false;
			}
			MachineData machineData = GetMachineData();
			if (Game1.IsMasterGame && (machineData == null || ShouldTimePassForMachine()))
			{
				minutesUntilReady.Value -= minutes;
			}
			if (MinutesUntilReady <= 0 && (machineData == null || !machineData.OnlyCompleteOvernight || Game1.newDaySync.hasInstance()))
			{
				if (!readyForHarvest && (!Game1.newDaySync.hasInstance() || Game1.newDaySync.hasFinished()))
				{
					environment.playSound("dwop");
				}
				readyForHarvest.Value = true;
				minutesUntilReady.Value = 0;
				onReadyForHarvest();
				showNextIndex.Value = machineData?.ShowNextIndexWhenReady ?? false;
				if (lightSource != null)
				{
					environment.removeLightSource(lightSource.identifier);
					lightSource = null;
				}
			}
			if (machineData != null)
			{
				if (!readyForHarvest && machineData.WorkingEffects != null && Game1.random.NextDouble() < (double)machineData.WorkingEffectChance)
				{
					addWorkingAnimation();
				}
			}
			else if (!readyForHarvest && Game1.random.NextDouble() < 0.33)
			{
				addWorkingAnimation();
			}
		}
		else if ((bool)bigCraftable)
		{
			switch (base.QualifiedItemId)
			{
			case "(BC)29":
				scale.Y = Math.Max(0f, scale.Y -= minutes / 2 + 1);
				break;
			case "(BC)96":
				MinutesUntilReady -= minutes;
				showNextIndex.Value = !showNextIndex.Value;
				if (MinutesUntilReady <= 0)
				{
					performRemoveAction();
					environment.objects.Remove(tileLocation.Value);
					environment.objects.Add(tileLocation.Value, ItemRegistry.Create<Object>("(BC)98"));
					Game1.player.team.RequestSetMail(PlayerActionTarget.Host, "Capsule_Broken", MailType.Received, add: true);
				}
				break;
			case "(BC)141":
				showNextIndex.Value = !showNextIndex.Value;
				break;
			case "(BC)83":
				showNextIndex.Value = false;
				environment.removeLightSource((int)(tileLocation.X * 797f + tileLocation.Y * 13f + 666f));
				break;
			}
		}
		return false;
	}

	public virtual bool ShouldTimePassForMachine()
	{
		GameLocation location = Location;
		MachineData data = GetMachineData();
		if (location == null || data == null)
		{
			return false;
		}
		if (data.PreventTimePass != null)
		{
			using List<MachineTimeBlockers>.Enumerator enumerator = data.PreventTimePass.GetEnumerator();
			while (enumerator.MoveNext())
			{
				switch (enumerator.Current)
				{
				case MachineTimeBlockers.Always:
					return false;
				case MachineTimeBlockers.Spring:
					if (location.IsSpringHere())
					{
						return false;
					}
					break;
				case MachineTimeBlockers.Summer:
					if (location.IsSummerHere())
					{
						return false;
					}
					break;
				case MachineTimeBlockers.Fall:
					if (location.IsFallHere())
					{
						return false;
					}
					break;
				case MachineTimeBlockers.Winter:
					if (location.IsWinterHere())
					{
						return false;
					}
					break;
				case MachineTimeBlockers.Sun:
					if (!location.IsRainingHere())
					{
						return false;
					}
					break;
				case MachineTimeBlockers.Rain:
					if (location.IsRainingHere())
					{
						return false;
					}
					break;
				case MachineTimeBlockers.Inside:
					if (!location.IsOutdoors)
					{
						return false;
					}
					break;
				case MachineTimeBlockers.Outside:
					if (location.IsOutdoors)
					{
						return false;
					}
					break;
				}
			}
		}
		return true;
	}

	public override string checkForSpecialItemHoldUpMeessage()
	{
		if (!bigCraftable && Type == "Arch")
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12993");
		}
		return base.QualifiedItemId switch
		{
			"(O)102" => Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12994"), 
			"(O)535" => Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12995"), 
			"(BC)160" => Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12996"), 
			_ => base.checkForSpecialItemHoldUpMeessage(), 
		};
	}

	public virtual bool countsForShippedCollection()
	{
		if (type == null || Type == "Arch" || (bool)bigCraftable)
		{
			return false;
		}
		if (base.QualifiedItemId == "(O)433")
		{
			return true;
		}
		switch (base.Category)
		{
		case -999:
		case -74:
		case -29:
		case -24:
		case -22:
		case -21:
		case -20:
		case -19:
		case -14:
		case -12:
		case -8:
		case -7:
		case -2:
		case 0:
			return false;
		default:
		{
			if (Game1.objectData.TryGetValue(base.ItemId, out var data) && data.ExcludeFromShippingCollection)
			{
				return false;
			}
			return true;
		}
		}
	}

	public static bool isPotentialBasicShipped(string itemId, int category, string objectType)
	{
		if (itemId == "433")
		{
			return true;
		}
		switch (objectType)
		{
		case "Arch":
		case "Fish":
		case "Minerals":
		case "Cooking":
			return false;
		default:
			switch (category)
			{
			case -999:
			case -103:
			case -102:
			case -96:
			case -74:
			case -29:
			case -24:
			case -22:
			case -21:
			case -20:
			case -19:
			case -14:
			case -12:
			case -8:
			case -7:
			case -2:
			case 0:
				return false;
			default:
			{
				if (Game1.objectData.TryGetValue(itemId, out var data) && data.ExcludeFromShippingCollection)
				{
					return false;
				}
				return true;
			}
			}
		}
	}

	public override IEnumerable<Buff> GetFoodOrDrinkBuffs()
	{
		foreach (Buff foodOrDrinkBuff in base.GetFoodOrDrinkBuffs())
		{
			yield return foodOrDrinkBuff;
		}
		if (customBuff != null)
		{
			Buff buff = customBuff();
			if (buff != null)
			{
				yield return buff;
			}
		}
		if ((int)edibility <= -300 || !Game1.objectData.TryGetValue(base.ItemId, out var data))
		{
			yield break;
		}
		List<ObjectBuffData> buffs = data.Buffs;
		if (buffs == null || buffs.Count <= 0)
		{
			yield break;
		}
		float durationMultiplier = ((base.Quality != 0) ? 1.5f : 1f);
		foreach (Buff item in TryCreateBuffsFromData(data, Name, DisplayName, durationMultiplier, ModifyItemBuffs))
		{
			yield return item;
		}
	}

	/// <summary>Create buffs matching data from <c>Data/Objects</c>, if valid.</summary>
	/// <param name="obj">The raw data from <c>Data/Objects</c> to parse.</param>
	/// <param name="name">The buff source name (usually the <see cref="P:StardewValley.Item.Name" />).</param>
	/// <param name="displayName">The translated buff source name (usually the <see cref="P:StardewValley.Item.DisplayName" />).</param>
	/// <param name="durationMultiplier">A multiplier to apply to food or drink buff durations. This only applies to food/drink buffs defined directly in <c>Data/Objects</c>, not to buff IDs which reference <c>Data/Buffs</c>.</param>
	/// <param name="adjustEffects">Adjust the parsed attribute effects before the buff is constructed.</param>
	public static IEnumerable<Buff> TryCreateBuffsFromData(ObjectData obj, string name, string displayName, float durationMultiplier = 1f, Action<BuffEffects> adjustEffects = null)
	{
		List<ObjectBuffData> buffs = obj.Buffs;
		if (buffs == null || buffs.Count <= 0)
		{
			yield break;
		}
		foreach (ObjectBuffData data in obj.Buffs)
		{
			if (data == null)
			{
				continue;
			}
			string id = data.BuffId;
			bool num = !string.IsNullOrWhiteSpace(id);
			if (!num)
			{
				id = (obj.IsDrink ? "drink" : "food");
			}
			BuffEffects effects = new BuffEffects(data.CustomAttributes);
			adjustEffects?.Invoke(effects);
			Texture2D texture = null;
			int spriteIndex = -1;
			if (data.IconTexture != null)
			{
				texture = Game1.content.Load<Texture2D>(data.IconTexture);
				spriteIndex = data.IconSpriteIndex;
			}
			int millisecondsDuration = -1;
			if (data.Duration == -2)
			{
				millisecondsDuration = -2;
			}
			else if (data.Duration != 0)
			{
				millisecondsDuration = (int)((float)data.Duration * durationMultiplier) * Game1.realMilliSecondsPerGameMinute;
			}
			bool isDebuff = data.IsDebuff;
			Color? glowColor = Utility.StringToColor(data.GlowColor);
			if (num || ((millisecondsDuration > 0 || millisecondsDuration == -2) && effects.HasAnyValue()))
			{
				Buff buff = new Buff(id, name, displayName, millisecondsDuration, texture, spriteIndex, effects, isDebuff);
				if (glowColor.HasValue)
				{
					buff.glow = glowColor.Value;
				}
				yield return buff;
			}
		}
	}

	/// <summary>Get whether the object scale should pulse currently.</summary>
	public virtual bool ShouldWobble()
	{
		if (minutesUntilReady.Value > 0 && !readyForHarvest.Value)
		{
			MachineData machineData = GetMachineData();
			if (machineData != null)
			{
				if (machineData.WobbleWhileWorking)
				{
					return heldObject.Value != null;
				}
				return false;
			}
			if (bigCraftable.Value)
			{
				switch (base.QualifiedItemId)
				{
				case "(BC)22":
				case "(BC)23":
				case "(BC)65":
				case "(BC)66":
				case "(BC)165":
					return false;
				default:
					return true;
				}
			}
		}
		return false;
	}

	public virtual Vector2 getScale()
	{
		if (base.Category == -22)
		{
			return Vector2.Zero;
		}
		if (!bigCraftable)
		{
			scale.Y = Math.Max(4f, scale.Y - 0.04f);
			return scale;
		}
		if (ShouldWobble())
		{
			if (base.QualifiedItemId.Equals("(BC)17"))
			{
				scale.X = (float)((double)(scale.X + 0.04f) % (Math.PI * 2.0));
				return Vector2.Zero;
			}
			scale.X -= 0.1f;
			scale.Y += 0.1f;
			if (scale.X <= 0f)
			{
				scale.X = 10f;
			}
			if (scale.Y >= 10f)
			{
				scale.Y = 0f;
			}
			return new Vector2(Math.Abs(scale.X - 5f), Math.Abs(scale.Y - 5f));
		}
		return Vector2.Zero;
	}

	public virtual void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
	{
		ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
		float drawLayer = Math.Max(0f, (float)(f.StandingPixel.Y + 3) / 10000f);
		Texture2D texture = itemData.GetTexture();
		int offset = 0;
		if (this is Mannequin)
		{
			offset = 2;
		}
		spriteBatch.Draw(texture, objectPosition, itemData.GetSourceRect(offset, base.ParentSheetIndex), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, drawLayer);
	}

	/// <summary>Draw a green or red placement tile for the held item, if applicable.</summary>
	/// <param name="spriteBatch">The sprite batch being drawn.</param>
	/// <param name="location">The current location.</param>
	public virtual void drawPlacementBounds(SpriteBatch spriteBatch, GameLocation location)
	{
		if (!isPlaceable() || this is Wallpaper)
		{
			return;
		}
		Game1.isCheckingNonMousePlacement = !Game1.IsPerformingMousePlacement();
		int x = (int)Game1.GetPlacementGrabTile().X * 64;
		int y = (int)Game1.GetPlacementGrabTile().Y * 64;
		if (Game1.isCheckingNonMousePlacement)
		{
			Vector2 nearbyValidPlacementPosition = Utility.GetNearbyValidPlacementPosition(Game1.player, location, this, x, y);
			x = (int)nearbyValidPlacementPosition.X;
			y = (int)nearbyValidPlacementPosition.Y;
		}
		Vector2 tile = new Vector2(x / 64, y / 64);
		if (Equals(Game1.player.ActiveObject))
		{
			TileLocation = tile;
		}
		if (Utility.isThereAnObjectHereWhichAcceptsThisItem(location, this, x, y) && (!location.objects.TryGetValue(tile, out var obj) || !(obj is IndoorPot pot) || !pot.IsPlantableItem(this)))
		{
			return;
		}
		bool canPlaceHere = Utility.playerCanPlaceItemHere(location, this, x, y, Game1.player) || (Utility.isThereAnObjectHereWhichAcceptsThisItem(location, this, x, y) && Utility.withinRadiusOfPlayer(x, y, 1, Game1.player));
		Game1.isCheckingNonMousePlacement = false;
		int width = 1;
		int height = 1;
		if (this is Furniture furniture)
		{
			width = furniture.getTilesWide();
			height = furniture.getTilesHigh();
		}
		for (int x_offset = 0; x_offset < width; x_offset++)
		{
			for (int y_offset = 0; y_offset < height; y_offset++)
			{
				spriteBatch.Draw(Game1.mouseCursors, new Vector2((tile.X + (float)x_offset) * 64f - (float)Game1.viewport.X, (tile.Y + (float)y_offset) * 64f - (float)Game1.viewport.Y), new Microsoft.Xna.Framework.Rectangle(canPlaceHere ? 194 : 210, 388, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01f);
			}
		}
		if ((bool)bigCraftable || this is Furniture || ((int)category != -74 && (int)category != -19))
		{
			draw(spriteBatch, (int)tile.X, (int)tile.Y, 0.5f);
		}
	}

	public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
	{
		AdjustMenuDrawForRecipes(ref transparency, ref scaleSize);
		if (drawShadow && !bigCraftable.Value && base.QualifiedItemId != "(O)590" && base.QualifiedItemId != "(O)SeedSpot")
		{
			spriteBatch.Draw(Game1.shadowTexture, location + new Vector2(32f, 48f), Game1.shadowTexture.Bounds, color * 0.5f, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f, SpriteEffects.None, layerDepth - 0.0001f);
		}
		ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
		float drawnScale = scaleSize;
		if (bigCraftable.Value && drawnScale > 0.2f)
		{
			drawnScale /= 2f;
		}
		int offset = 0;
		if (this is Mannequin)
		{
			offset = 2;
		}
		Microsoft.Xna.Framework.Rectangle sourceRect = itemData.GetSourceRect(offset, base.ParentSheetIndex);
		spriteBatch.Draw(itemData.GetTexture(), location + new Vector2(32f, 32f), sourceRect, color * transparency, 0f, new Vector2(sourceRect.Width / 2, sourceRect.Height / 2), 4f * drawnScale, SpriteEffects.None, layerDepth);
		DrawMenuIcons(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color);
	}

	public override void DrawIconBar(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color)
	{
		if (base.Category == -22 && uses.Value > 0)
		{
			float health = ((float)(FishingRod.maxTackleUses - uses.Value) + 0f) / (float)FishingRod.maxTackleUses;
			spriteBatch.Draw(Game1.staminaRect, new Microsoft.Xna.Framework.Rectangle((int)location.X, (int)(location.Y + 56f * scaleSize), (int)(64f * scaleSize * health), (int)(8f * scaleSize)), Utility.getRedToGreenLerpColor(health));
		}
	}

	public virtual void drawAsProp(SpriteBatch b)
	{
		if (isTemporarilyInvisible)
		{
			return;
		}
		int x = (int)tileLocation.X;
		int y = (int)tileLocation.Y;
		if ((bool)bigCraftable)
		{
			int indexOffset = 0;
			if ((bool)showNextIndex)
			{
				indexOffset = 1;
			}
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
			Texture2D texture = itemData.GetTexture();
			Vector2 scaleFactor = getScale();
			scaleFactor *= 4f;
			Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64));
			Microsoft.Xna.Framework.Rectangle destination = new Microsoft.Xna.Framework.Rectangle((int)(position.X - scaleFactor.X / 2f), (int)(position.Y - scaleFactor.Y / 2f), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
			b.Draw(texture, destination, itemData.GetSourceRect(indexOffset, base.ParentSheetIndex), Color.White, 0f, Vector2.Zero, SpriteEffects.None, Math.Max(0f, (float)((y + 1) * 64 - 1) / 10000f) + (IsTapper() ? 0.0015f : 0f));
			if (base.QualifiedItemId == "(BC)17" && MinutesUntilReady > 0)
			{
				b.Draw(Game1.objectSpriteSheet, getLocalPosition(Game1.viewport) + new Vector2(32f, 0f), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 435), Color.White, scale.X, new Vector2(32f, 32f), 1f, SpriteEffects.None, Math.Max(0f, (float)((y + 1) * 64 - 1) / 10000f + 0.0001f));
			}
		}
		else
		{
			Microsoft.Xna.Framework.Rectangle bounds = GetBoundingBoxAt(x, y);
			if (base.QualifiedItemId != "(O)590" && base.QualifiedItemId != "(O)742" && base.QualifiedItemId != "(O)SeedSpot")
			{
				b.Draw(Game1.shadowTexture, getLocalPosition(Game1.viewport) + new Vector2(32f, 53f), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, (float)bounds.Bottom / 15000f);
			}
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
			b.Draw(itemData.GetTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32, y * 64 + 32)), itemData.GetSourceRect(), Color.White, 0f, new Vector2(8f, 8f), (scale.Y > 1f) ? getScale().Y : 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)bounds.Bottom / 10000f);
		}
	}

	public virtual void drawAboveFrontLayer(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
	{
	}

	public virtual void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
	{
		if (isTemporarilyInvisible)
		{
			return;
		}
		GameLocation location = Location;
		if (hovering)
		{
			if (base.ItemId == "TextSign" && signText.Value != null && signText.Value.Length > 0)
			{
				Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32, y * 64 - 64));
				SpriteText.drawSmallTextBubble(spriteBatch, signText.Value, position, 256, 0.98f + TileLocation.X * 0.0001f + TileLocation.Y * 1E-06f);
			}
			hovering = false;
		}
		ParsedItemData heldItemData;
		Texture2D texture;
		if ((bool)bigCraftable)
		{
			Vector2 scaleFactor = getScale();
			scaleFactor *= 4f;
			Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64));
			Microsoft.Xna.Framework.Rectangle destination = new Microsoft.Xna.Framework.Rectangle((int)(position.X - scaleFactor.X / 2f) + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scaleFactor.Y / 2f) + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
			float draw_layer = Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f;
			int offset = 0;
			if ((bool)showNextIndex)
			{
				offset = 1;
			}
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
			if (heldObject.Value != null)
			{
				MachineData machineData = GetMachineData();
				if (machineData != null && machineData.IsIncubator)
				{
					offset = FarmAnimal.GetAnimalDataFromEgg(heldObject.Value, location)?.IncubatorParentSheetOffset ?? 1;
				}
			}
			if (_machineAnimationFrame >= 0 && _machineAnimation != null)
			{
				offset = _machineAnimationFrame;
			}
			if (this is Mannequin mannequin)
			{
				offset = mannequin.facing.Value;
			}
			if (IsTapper())
			{
				draw_layer = Math.Max(0f, (float)((y + 1) * 64 + 2) / 10000f) + (float)x / 1000000f;
			}
			if (base.QualifiedItemId == "(BC)272")
			{
				texture = itemData.GetTexture();
				spriteBatch.Draw(texture, destination, itemData.GetSourceRect(1, base.ParentSheetIndex), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);
				spriteBatch.Draw(texture, position + new Vector2(8.5f, 12f) * 4f, itemData.GetSourceRect(2, base.ParentSheetIndex), Color.White * alpha, (float)Game1.currentGameTime.TotalGameTime.TotalSeconds * -1.5f, new Vector2(7.5f, 15.5f), 4f, SpriteEffects.None, draw_layer + 1E-05f);
				return;
			}
			spriteBatch.Draw(itemData.GetTexture(), destination, itemData.GetSourceRect(offset, base.ParentSheetIndex), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);
			if (base.QualifiedItemId == "(BC)17" && MinutesUntilReady > 0)
			{
				spriteBatch.Draw(Game1.objectSpriteSheet, getLocalPosition(Game1.viewport) + new Vector2(32f, 0f), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 435, 16, 16), Color.White * alpha, scale.X, new Vector2(8f, 8f), 4f, SpriteEffects.None, Math.Max(0f, (float)((y + 1) * 64) / 10000f + 0.0001f + (float)x * 1E-05f));
			}
			if ((bool)isLamp && Game1.isDarkOut(Location))
			{
				spriteBatch.Draw(Game1.mouseCursors, position + new Vector2(-32f, -32f), new Microsoft.Xna.Framework.Rectangle(88, 1779, 32, 32), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)((y + 1) * 64 - 20) / 10000f) + (float)x / 1000000f);
			}
			if (base.QualifiedItemId == "(BC)126")
			{
				string hatId = (((int)quality != 0) ? ((int)quality - 1).ToString() : preservedParentSheetIndex.Value);
				if (hatId != null)
				{
					ParsedItemData dataOrErrorItem = ItemRegistry.GetDataOrErrorItem("(H)" + hatId);
					texture = dataOrErrorItem.GetTexture();
					int spriteIndex = dataOrErrorItem.SpriteIndex;
					bool isPrismatic = ItemContextTagManager.HasBaseTag("(H)" + hatId, "Prismatic");
					spriteBatch.Draw(texture, position + new Vector2(-3f, -6f) * 4f, new Microsoft.Xna.Framework.Rectangle(spriteIndex * 20 % texture.Width, spriteIndex * 20 / texture.Width * 20 * 4, 20, 20), (isPrismatic ? Utility.GetPrismaticColor() : Color.White) * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)((y + 1) * 64 - 20) / 10000f) + (float)x * 1E-05f);
				}
			}
		}
		else if (!Game1.eventUp || (Game1.CurrentEvent != null && !Game1.CurrentEvent.isTileWalkedOn(x, y)))
		{
			Microsoft.Xna.Framework.Rectangle bounds = GetBoundingBoxAt(x, y);
			if (base.QualifiedItemId == "(O)590")
			{
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), y * 64 + 32 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), new Microsoft.Xna.Framework.Rectangle(368 + ((Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1200.0 <= 400.0) ? ((int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 400.0 / 100.0) * 16) : 0), 32, 16, 16), Color.White * alpha, 0f, new Vector2(8f, 8f), (scale.Y > 1f) ? getScale().Y : 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(isPassable() ? bounds.Top : bounds.Bottom) / 10000f);
				return;
			}
			if (base.QualifiedItemId == "(O)SeedSpot")
			{
				spriteBatch.Draw(Game1.mouseCursors_1_6, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), y * 64 + 32 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), new Microsoft.Xna.Framework.Rectangle(160 + ((Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1600.0 <= 800.0) ? ((int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 400.0 / 100.0) * 16) : 0), 0, 17, 16), Color.White * alpha, 0f, new Vector2(8f, 8f), (scale.Y > 1f) ? getScale().Y : 4f, (Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1600.0 <= 400.0) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(isPassable() ? bounds.Top : bounds.Bottom) / 10000f);
				return;
			}
			if ((int)fragility != 2)
			{
				spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32, y * 64 + 51 + 4)), Game1.shadowTexture.Bounds, Color.White * alpha, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, (float)bounds.Bottom / 15000f);
			}
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
			spriteBatch.Draw(itemData.GetTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), y * 64 + 32 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), itemData.GetSourceRect(), Color.White * alpha, 0f, new Vector2(8f, 8f), (scale.Y > 1f) ? getScale().Y : 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(isPassable() ? bounds.Top : bounds.Center.Y) / 10000f);
			if (IsSprinkler())
			{
				if (heldObject.Value != null)
				{
					Vector2 offset = Vector2.Zero;
					if (heldObject.Value.QualifiedItemId == "(O)913")
					{
						offset = new Vector2(0f, -20f);
					}
					heldItemData = ItemRegistry.GetDataOrErrorItem(heldObject.Value.QualifiedItemId);
					spriteBatch.Draw(heldItemData.GetTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), y * 64 + 32 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)) + offset), heldItemData.GetSourceRect(1), Color.White * alpha, 0f, new Vector2(8f, 8f), (scale.Y > 1f) ? getScale().Y : 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(isPassable() ? bounds.Top : bounds.Bottom) / 10000f + 1E-05f);
				}
				if (base.SpecialVariable == 999999)
				{
					if (heldObject.Value != null && heldObject.Value.QualifiedItemId == "(O)913")
					{
						Torch.drawBasicTorch(spriteBatch, (float)(x * 64) - 2f, y * 64 - 32, (float)bounds.Bottom / 10000f + 1E-06f);
					}
					else
					{
						Torch.drawBasicTorch(spriteBatch, (float)(x * 64) - 2f, y * 64 - 32 + 12, (float)(bounds.Bottom + 2) / 10000f);
					}
				}
			}
		}
		if (!readyForHarvest)
		{
			return;
		}
		float base_sort = (float)((y + 1) * 64) / 10000f + tileLocation.X / 50000f;
		if (IsTapper() || base.QualifiedItemId.Equals("(BC)MushroomLog"))
		{
			base_sort += 0.02f;
		}
		float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
		spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 - 8, (float)(y * 64 - 96 - 16) + yOffset)), new Microsoft.Xna.Framework.Rectangle(141, 465, 20, 24), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort + 1E-06f);
		if (heldObject.Value == null)
		{
			return;
		}
		heldItemData = ItemRegistry.GetDataOrErrorItem(heldObject.Value.QualifiedItemId);
		texture = heldItemData.GetTexture();
		if (heldObject.Value is ColoredObject coloredObj)
		{
			coloredObj.drawInMenu(spriteBatch, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, (float)(y * 64) - 96f - 8f + yOffset)), 1f, 0.75f, base_sort + 1.1E-05f);
			return;
		}
		spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32, (float)(y * 64 - 64 - 8) + yOffset)), heldItemData.GetSourceRect(), Color.White * 0.75f, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, base_sort + 1E-05f);
		if (heldObject.Value.Stack > 1)
		{
			heldObject.Value.DrawMenuIcons(spriteBatch, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, (float)(y * 64 - 64 - 32) + yOffset - 4f)), 1f, 1f, base_sort + 1.2E-05f, StackDrawType.Draw, Color.White);
		}
		else if (heldObject.Value.Quality > 0)
		{
			heldObject.Value.DrawMenuIcons(spriteBatch, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, (float)(y * 64 - 64 - 32) + yOffset - 4f)), 1f, 1f, base_sort + 1.2E-05f, StackDrawType.HideButShowQuality, Color.White);
		}
	}

	public virtual void draw(SpriteBatch spriteBatch, int xNonTile, int yNonTile, float layerDepth, float alpha = 1f)
	{
		if (isTemporarilyInvisible)
		{
			return;
		}
		if ((bool)bigCraftable)
		{
			Vector2 scaleFactor = getScale();
			scaleFactor *= 4f;
			Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(xNonTile, yNonTile));
			Microsoft.Xna.Framework.Rectangle destination = new Microsoft.Xna.Framework.Rectangle((int)(position.X - scaleFactor.X / 2f) + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scaleFactor.Y / 2f) + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
			int indexOffset = 0;
			if ((bool)showNextIndex)
			{
				indexOffset = 1;
			}
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
			spriteBatch.Draw(itemData.GetTexture(), destination, itemData.GetSourceRect(indexOffset, base.ParentSheetIndex), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
			if (base.QualifiedItemId == "(BC)17" && MinutesUntilReady > 0)
			{
				spriteBatch.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(position) + new Vector2(32f, 0f), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 435, 16, 16), Color.White * alpha, scale.X, new Vector2(8f, 8f), 4f, SpriteEffects.None, layerDepth);
			}
			if ((bool)isLamp && Game1.isDarkOut(Location))
			{
				spriteBatch.Draw(Game1.mouseCursors, position + new Vector2(-32f, -32f), new Microsoft.Xna.Framework.Rectangle(88, 1779, 32, 32), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth);
			}
		}
		else if (!Game1.eventUp || !Game1.CurrentEvent.isTileWalkedOn(xNonTile / 64, yNonTile / 64))
		{
			if (base.QualifiedItemId != "(O)590" && base.QualifiedItemId != "(O)SeedSpot" && (int)fragility != 2)
			{
				spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(xNonTile + 32, yNonTile + 51 + 4)), Game1.shadowTexture.Bounds, Color.White * alpha, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, layerDepth - 1E-06f);
			}
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
			spriteBatch.Draw(itemData.GetTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2(xNonTile + 32 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), yNonTile + 32 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), itemData.GetSourceRect(0, base.ParentSheetIndex), Color.White * alpha, 0f, new Vector2(8f, 8f), (scale.Y > 1f) ? getScale().Y : 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
		}
	}

	public override int maximumStackSize()
	{
		switch (base.QualifiedItemId)
		{
		case "(O)79":
		case "(O)842":
		case "(O)911":
			return 1;
		default:
			if (base.Category == -22)
			{
				return 1;
			}
			return 999;
		}
	}

	public virtual void hoverAction()
	{
		hovering = true;
	}

	public virtual bool clicked(Farmer who)
	{
		return false;
	}

	/// <inheritdoc />
	protected override Item GetOneNew()
	{
		if (!bigCraftable)
		{
			return new Object(base.ItemId, 1);
		}
		return new Object(tileLocation.Value, base.ItemId);
	}

	/// <inheritdoc />
	protected override void GetOneCopyFrom(Item source)
	{
		base.GetOneCopyFrom(source);
		if (source is Object fromObj)
		{
			Scale = fromObj.scale;
			IsSpawnedObject = fromObj.isSpawnedObject.Value;
			Price = fromObj.price.Value;
			Edibility = fromObj.edibility.Value;
			name = fromObj.name;
			displayNameFormat = fromObj.displayNameFormat;
			TileLocation = fromObj.TileLocation;
			uses.Value = fromObj.uses.Value;
			questItem.Value = fromObj.questItem;
			questId.Value = fromObj.questId;
			preserve.Value = fromObj.preserve.Value;
			preservedParentSheetIndex.Value = fromObj.preservedParentSheetIndex.Value;
			orderData.Value = fromObj.orderData.Value;
			owner.Value = fromObj.owner.Value;
		}
	}

	public override bool canBePlacedHere(GameLocation l, Vector2 tile, CollisionMask collisionMask = CollisionMask.All, bool showError = false)
	{
		if (base.QualifiedItemId == "(O)710")
		{
			if (CrabPot.IsValidCrabPotLocationTile(l, (int)tile.X, (int)tile.Y))
			{
				return true;
			}
			return false;
		}
		if (IsTapper() && l.terrainFeatures.TryGetValue(tile, out var terrainFeature) && terrainFeature is Tree tree && !l.objects.ContainsKey(tile) && (tree.GetData()?.CanBeTapped() ?? false))
		{
			return true;
		}
		if (base.QualifiedItemId == "(O)805" && l.terrainFeatures.TryGetValue(tile, out terrainFeature) && terrainFeature is Tree)
		{
			return true;
		}
		if (base.QualifiedItemId == "(O)419")
		{
			if (l.terrainFeatures.TryGetValue(tile, out terrainFeature) && terrainFeature is Tree tree && !tree.stopGrowingMoss.Value)
			{
				return true;
			}
			return false;
		}
		if (isWildTreeSeed(base.ItemId))
		{
			if (!l.CanItemBePlacedHere(tile, itemIsPassable: true, collisionMask))
			{
				return false;
			}
			if (!canPlaceWildTreeSeed(l, tile, out var deniedMessage))
			{
				if (showError && deniedMessage != null)
				{
					Game1.showRedMessage(deniedMessage);
				}
				return false;
			}
			return true;
		}
		if ((int)category == -74)
		{
			HoeDirt dirt = l.GetHoeDirtAtTile(tile);
			Object obj = l.getObjectAtTile((int)tile.X, (int)tile.Y);
			IndoorPot pot = obj as IndoorPot;
			if (dirt?.crop != null || (dirt == null && l.terrainFeatures.TryGetValue(tile, out var _)))
			{
				return false;
			}
			if (IsFruitTreeSapling())
			{
				if (obj != null)
				{
					return false;
				}
				if (dirt == null)
				{
					if (FruitTree.IsTooCloseToAnotherTree(tile, l, !IsFruitTreeSapling()))
					{
						if (showError)
						{
							Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13060"));
						}
						return false;
					}
					if (FruitTree.IsGrowthBlocked(tile, l))
					{
						if (showError)
						{
							Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:FruitTree_PlacementWarning", DisplayName));
						}
						return false;
					}
					if (!l.CanItemBePlacedHere(tile, itemIsPassable: true, collisionMask))
					{
						return false;
					}
					if (!l.CanPlantTreesHere(base.ItemId, (int)tile.X, (int)tile.Y, out var deniedMessage))
					{
						if (showError && deniedMessage != null)
						{
							Game1.showRedMessage(deniedMessage);
						}
						return false;
					}
					return true;
				}
				return false;
			}
			if (IsTeaSapling())
			{
				bool isFreeGardenPot = pot != null && pot.bush.Value == null && pot.hoeDirt.Value.crop == null;
				if (isFreeGardenPot)
				{
					if (!l.IsOutdoors)
					{
						return true;
					}
				}
				else
				{
					if (obj != null || dirt != null)
					{
						return false;
					}
					if (!l.CanItemBePlacedHere(tile, itemIsPassable: true, collisionMask))
					{
						return false;
					}
					if (l.IsGreenhouse && l.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Diggable", "Back") == null)
					{
						return false;
					}
				}
				if (!l.CheckItemPlantRules(base.QualifiedItemId, isFreeGardenPot, (bool)l.isOutdoors || l.IsGreenhouse, out var deniedMessage))
				{
					if (showError && deniedMessage != null)
					{
						Game1.showRedMessage(Game1.content.LoadString(deniedMessage));
					}
					return false;
				}
				return true;
			}
			if (IsWildTreeSapling())
			{
				if (obj != null)
				{
					return false;
				}
				if (FruitTree.IsTooCloseToAnotherTree(tile, l, fruitTreesOnly: true))
				{
					if (showError)
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13060_Fruit"));
					}
					return false;
				}
				return l.CanItemBePlacedHere(tile, itemIsPassable: true, collisionMask);
			}
			if (this.HasTypeObject())
			{
				if (pot != null)
				{
					if (pot.IsPlantableItem(this) && pot.bush.Value == null)
					{
						return pot.hoeDirt.Value.canPlantThisSeedHere(base.ItemId);
					}
					return false;
				}
				if (dirt != null && l.CanItemBePlacedHere(tile, itemIsPassable: true, collisionMask) && dirt.canPlantThisSeedHere(base.ItemId))
				{
					return true;
				}
			}
			return false;
		}
		if ((int)category == -19)
		{
			HoeDirt dirt = l.GetHoeDirtAtTile(tile);
			if (dirt != null && dirt.CanApplyFertilizer(base.QualifiedItemId))
			{
				if (l.getObjectAtTile((int)tile.X, (int)tile.Y) is IndoorPot pot)
				{
					return pot.IsPlantableItem(this);
				}
				return true;
			}
			return false;
		}
		if (l != null)
		{
			Vector2 nonTile = tile * 64f * 64f;
			nonTile.X += 32f;
			nonTile.Y += 32f;
			foreach (Furniture f in l.furniture)
			{
				if ((int)f.furniture_type == 11 && f.GetBoundingBox().Contains((int)nonTile.X, (int)nonTile.Y) && f.heldObject.Value == null)
				{
					return true;
				}
			}
		}
		if (IsFloorPathItem())
		{
			collisionMask &= ~CollisionMask.Buildings;
		}
		return l.CanItemBePlacedHere(tile, isPassable(), collisionMask);
	}

	public override bool isPlaceable()
	{
		if (HasContextTag("placeable"))
		{
			return true;
		}
		if (HasContextTag("not_placeable"))
		{
			return false;
		}
		if (type.Value != null && (base.Category == -8 || base.Category == -9 || Type == "Crafting" || isSapling() || base.QualifiedItemId == "(O)710" || base.Category == -74 || base.Category == -19) && ((int)edibility < 0 || IsWildTreeSapling()))
		{
			return true;
		}
		return false;
	}

	public bool IsConsideredReadyMachineForComputer()
	{
		if (bigCraftable.Value && heldObject.Value != null)
		{
			if (!(heldObject.Value is Chest chest))
			{
				return minutesUntilReady.Value <= 0;
			}
			if (!chest.isEmpty())
			{
				return true;
			}
		}
		return false;
	}

	public MachineData GetMachineData()
	{
		if (!Game1.content.Load<Dictionary<string, MachineData>>("Data\\Machines").TryGetValue(base.QualifiedItemId, out var data))
		{
			return null;
		}
		return data;
	}

	public virtual bool isSapling()
	{
		if (!IsTeaSapling() && !IsWildTreeSapling())
		{
			return IsFruitTreeSapling();
		}
		return true;
	}

	public virtual bool IsTeaSapling()
	{
		return base.QualifiedItemId == "(O)251";
	}

	public virtual bool IsFruitTreeSapling()
	{
		if (this.HasTypeObject())
		{
			return Game1.fruitTreeData.ContainsKey(base.ItemId);
		}
		return false;
	}

	public virtual bool IsWildTreeSapling()
	{
		if (this.HasTypeObject())
		{
			return isWildTreeSeed(base.ItemId);
		}
		return false;
	}

	public virtual bool IsFloorPathItem()
	{
		if (this.HasTypeObject())
		{
			return IsFloorPathItem(base.ItemId);
		}
		return false;
	}

	public static bool IsFloorPathItem(string itemId)
	{
		if (itemId != null)
		{
			return Flooring.GetFloorPathItemLookup().ContainsKey(itemId);
		}
		return false;
	}

	public virtual bool IsFenceItem()
	{
		if (this.HasTypeObject())
		{
			return Fence.GetFenceLookup().ContainsKey(base.ItemId);
		}
		return false;
	}

	public static bool isWildTreeSeed(string itemId)
	{
		if (itemId != null)
		{
			return Tree.GetWildTreeSeedLookup().ContainsKey(itemId);
		}
		return false;
	}

	private bool canPlaceWildTreeSeed(GameLocation location, Vector2 tile, out string deniedMessage)
	{
		if (location.IsNoSpawnTile(tile, "Tree", ignoreTileSheetProperties: true))
		{
			deniedMessage = null;
			return false;
		}
		if (location.IsNoSpawnTile(tile, "Tree") && !location.doesEitherTileOrTileIndexPropertyEqual((int)tile.X, (int)tile.Y, "CanPlantTrees", "Back", "T"))
		{
			deniedMessage = null;
			return false;
		}
		if (location.objects.ContainsKey(tile))
		{
			deniedMessage = null;
			return false;
		}
		if (location.terrainFeatures.TryGetValue(tile, out var terrainFeature) && !(terrainFeature is HoeDirt))
		{
			deniedMessage = null;
			return false;
		}
		if (!location.CanPlantTreesHere(base.ItemId, (int)tile.X, (int)tile.Y, out deniedMessage))
		{
			return false;
		}
		return location.CheckItemPlantRules(base.QualifiedItemId, isGardenPot: false, location is Farm || location.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Diggable", "Back") != null || location.doesEitherTileOrTileIndexPropertyEqual((int)tile.X, (int)tile.Y, "CanPlantTrees", "Back", "T"), out deniedMessage);
	}

	public virtual bool IsSprinkler()
	{
		if (GetBaseRadiusForSprinkler() >= 0)
		{
			return true;
		}
		return false;
	}

	/// <summary>Get whether this is a stone litter item which can be broken by a pickaxe.</summary>
	public bool IsBreakableStone()
	{
		if (base.Category == -999)
		{
			return Name == "Stone";
		}
		return false;
	}

	/// <summary>Get whether this is a twig litter item.</summary>
	public bool IsTwig()
	{
		if (base.Category == -999)
		{
			return Name == "Twig";
		}
		return false;
	}

	public bool isDebrisOrForage()
	{
		if (!IsWeeds() && !IsBreakableStone() && !IsTwig())
		{
			return isForage();
		}
		return true;
	}

	/// <summary>Get whether this is a weed litter item.</summary>
	public bool IsWeeds()
	{
		if (base.Category == -999)
		{
			return Name.ToLower().Contains("weeds");
		}
		return false;
	}

	public virtual bool IsTapper()
	{
		return HasContextTag("tapper_item");
	}

	public virtual bool IsBar()
	{
		if (!(base.QualifiedItemId == "(O)334") && !(base.QualifiedItemId == "(O)335") && !(base.QualifiedItemId == "(O)336") && !(base.QualifiedItemId == "(O)337"))
		{
			return base.QualifiedItemId == "(O)910";
		}
		return true;
	}

	public virtual int GetModifiedRadiusForSprinkler()
	{
		int radius = GetBaseRadiusForSprinkler();
		if (radius < 0)
		{
			return -1;
		}
		if (heldObject.Value != null && heldObject.Value.QualifiedItemId == "(O)915")
		{
			radius++;
		}
		return radius;
	}

	public virtual int GetBaseRadiusForSprinkler()
	{
		return base.QualifiedItemId switch
		{
			"(O)599" => 0, 
			"(O)621" => 1, 
			"(O)645" => 2, 
			_ => -1, 
		};
	}

	/// <summary>Check whether the object can be added to a location, and (sometimes) add it to the location.</summary>
	/// <param name="location">The location in which to place it.</param>
	/// <param name="x">The X tile position at which to place it.</param>
	/// <param name="y">The Y tile position at which to place it.</param>
	/// <param name="who">The player placing the object, if applicable.</param>
	/// <returns>Returns whether the object should be (or was) added to the location.</returns>
	/// <remarks>For legacy reasons, the behavior of this method is inconsistent. It'll sometimes add the object to the location itself, and sometimes expect the caller to do it.</remarks>
	public virtual bool placementAction(GameLocation location, int x, int y, Farmer who = null)
	{
		Vector2 placementTile = new Vector2(x / 64, y / 64);
		health = 10;
		Location = location;
		TileLocation = placementTile;
		owner.Value = who?.UniqueMultiplayerID ?? Game1.player.UniqueMultiplayerID;
		TerrainFeature terrainFeature;
		if (!bigCraftable && !(this is Furniture))
		{
			if (IsSprinkler() && location.doesTileHavePropertyNoNull((int)placementTile.X, (int)placementTile.Y, "NoSprinklers", "Back") == "T")
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:NoSprinklers"));
				return false;
			}
			if (IsWildTreeSapling())
			{
				if (!canPlaceWildTreeSeed(location, placementTile, out var deniedMessage))
				{
					if (deniedMessage == null)
					{
						deniedMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13021");
					}
					Game1.showRedMessage(deniedMessage);
					return false;
				}
				string treeType = Tree.ResolveTreeTypeFromSeed(base.QualifiedItemId);
				if (treeType != null)
				{
					Game1.stats.Increment("wildtreesplanted");
					location.terrainFeatures.Remove(placementTile);
					location.terrainFeatures.Add(placementTile, new Tree(treeType, 0));
					location.playSound("dirtyHit");
					return true;
				}
				return false;
			}
			if (IsFloorPathItem())
			{
				if (location.terrainFeatures.ContainsKey(placementTile))
				{
					return false;
				}
				string key = Flooring.GetFloorPathItemLookup()[base.ItemId];
				location.terrainFeatures.Add(placementTile, new Flooring(key));
				if (Game1.floorPathData.TryGetValue(key, out var floorData) && floorData.PlacementSound != null)
				{
					location.playSound(floorData.PlacementSound);
				}
				return true;
			}
			if (ItemContextTagManager.HasBaseTag(base.QualifiedItemId, "torch_item"))
			{
				if (location.objects.ContainsKey(placementTile))
				{
					return false;
				}
				location.removeLightSource((int)(tileLocation.X * 2000f + tileLocation.Y));
				location.removeLightSource((int)Game1.player.UniqueMultiplayerID);
				new Torch(1, base.ItemId).placementAction(location, x, y, who ?? Game1.player);
				return true;
			}
			if (IsFenceItem())
			{
				if (location.objects.ContainsKey(placementTile))
				{
					return false;
				}
				FenceData fenceData = Fence.GetFenceLookup()[base.ItemId];
				location.objects.Add(placementTile, new Fence(placementTile, base.ItemId, base.ItemId == "325"));
				if (fenceData.PlacementSound != null)
				{
					location.playSound(fenceData.PlacementSound);
				}
				return true;
			}
			switch (base.QualifiedItemId)
			{
			case "(O)TentKit":
			{
				if (location == null || !location.IsOutdoors)
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Furniture_Outdoors_Message"));
					return false;
				}
				if (Utility.isFestivalDay((Game1.dayOfMonth + 1) % 28, (Game1.dayOfMonth == 28) ? ((Season)((int)(Game1.season + 1) % 4)) : Game1.season, location.GetLocationContextId()))
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\1_6_Strings:FestivalTentWarning"));
					return false;
				}
				PassiveFestivalData passiveFestival = null;
				string passiveFestivalID = null;
				if (Utility.TryGetPassiveFestivalDataForDay((Game1.dayOfMonth + 1) % 28, (Game1.dayOfMonth == 28) ? ((Season)((int)(Game1.season + 1) % 4)) : Game1.season, null, out passiveFestivalID, out passiveFestival) && passiveFestival != null)
				{
					if (passiveFestival.MapReplacements != null)
					{
						foreach (string key2 in passiveFestival.MapReplacements.Keys)
						{
							if (key2.Equals(location.Name))
							{
								Game1.showRedMessage(Game1.content.LoadString("Strings\\1_6_Strings:FestivalTentWarning"));
								return false;
							}
						}
					}
					if (((passiveFestivalID.Equals("TroutDerby") && location.Name.Equals("Forest")) || (passiveFestivalID.Equals("SquidFest") && location.Name.Equals("Beach"))) && passiveFestival.StartDay > Game1.dayOfMonth)
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\1_6_Strings:FestivalTentWarning"));
						return false;
					}
				}
				if (who != null)
				{
					Microsoft.Xna.Framework.Rectangle area = Microsoft.Xna.Framework.Rectangle.Empty;
					switch (Utility.getDirectionFromChange(placementTile, who.Tile))
					{
					case 0:
						area = new Microsoft.Xna.Framework.Rectangle((int)(placementTile.X - 1f), (int)(placementTile.Y - 1f), 3, 2);
						break;
					case 1:
						area = new Microsoft.Xna.Framework.Rectangle((int)placementTile.X, (int)(placementTile.Y - 1f), 3, 2);
						break;
					case 2:
						area = new Microsoft.Xna.Framework.Rectangle((int)(placementTile.X - 1f), (int)placementTile.Y, 3, 2);
						break;
					case 3:
						area = new Microsoft.Xna.Framework.Rectangle((int)(placementTile.X - 2f), (int)(placementTile.Y - 1f), 3, 2);
						break;
					}
					if (area != Microsoft.Xna.Framework.Rectangle.Empty && location.isAreaClear(area))
					{
						location.largeTerrainFeatures.Add(new Tent(new Vector2(area.X + 1, area.Y + 1)));
						Game1.playSound("moss_cut");
						Game1.playSound("woodyHit");
						new Microsoft.Xna.Framework.Rectangle(area.X * 64, area.Y * 64, 192, 128);
						Utility.addDirtPuffs(location, area.X, area.Y, 3, 2, 9);
						return true;
					}
					Game1.showRedMessage(Game1.content.LoadString("Strings\\1_6_Strings:Tent_Blocked"));
					return false;
				}
				break;
			}
			case "(O)926":
				if (location.objects.ContainsKey(placementTile) || location.terrainFeatures.ContainsKey(placementTile))
				{
					return false;
				}
				location.objects.Add(placementTile, new Torch("278", bigCraftable: true)
				{
					Fragility = 1,
					destroyOvernight = true
				});
				Utility.addSmokePuff(location, new Vector2(x, y));
				Utility.addSmokePuff(location, new Vector2(x + 16, y + 16));
				Utility.addSmokePuff(location, new Vector2(x + 32, y));
				Utility.addSmokePuff(location, new Vector2(x + 48, y + 16));
				Utility.addSmokePuff(location, new Vector2(x + 32, y + 32));
				Game1.playSound("fireball");
				return true;
			case "(O)286":
			{
				foreach (TemporaryAnimatedSprite temporarySprite in location.temporarySprites)
				{
					if (temporarySprite.position.Equals(placementTile * 64f))
					{
						return false;
					}
				}
				int idNum = Game1.random.Next();
				location.playSound("thudStep");
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(base.ParentSheetIndex, 100f, 1, 24, placementTile * 64f, flicker: true, flipped: false, location, who)
				{
					shakeIntensity = 0.5f,
					shakeIntensityChange = 0.002f,
					extraInfoForEndBehavior = idNum,
					endFunction = location.removeTemporarySpritesWithID
				});
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f + new Vector2(5f, 3f) * 4f, flicker: true, flipped: false, (float)(y + 7) / 10000f, 0f, Color.Yellow, 4f, 0f, 0f, 0f)
				{
					id = idNum
				});
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f + new Vector2(5f, 3f) * 4f, flicker: true, flipped: true, (float)(y + 7) / 10000f, 0f, Color.Orange, 4f, 0f, 0f, 0f)
				{
					delayBeforeAnimationStart = 100,
					id = idNum
				});
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f + new Vector2(5f, 3f) * 4f, flicker: true, flipped: false, (float)(y + 7) / 10000f, 0f, Color.White, 3f, 0f, 0f, 0f)
				{
					delayBeforeAnimationStart = 200,
					id = idNum
				});
				location.netAudio.StartPlaying("fuse");
				return true;
			}
			case "(O)287":
			{
				foreach (TemporaryAnimatedSprite temporarySprite2 in location.temporarySprites)
				{
					if (temporarySprite2.position.Equals(placementTile * 64f))
					{
						return false;
					}
				}
				int idNum = Game1.random.Next();
				location.playSound("thudStep");
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(base.ParentSheetIndex, 100f, 1, 24, placementTile * 64f, flicker: true, flipped: false, location, who)
				{
					shakeIntensity = 0.5f,
					shakeIntensityChange = 0.002f,
					extraInfoForEndBehavior = idNum,
					endFunction = location.removeTemporarySpritesWithID
				});
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f, flicker: true, flipped: false, (float)(y + 7) / 10000f, 0f, Color.Yellow, 4f, 0f, 0f, 0f)
				{
					id = idNum
				});
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f, flicker: true, flipped: false, (float)(y + 7) / 10000f, 0f, Color.Orange, 4f, 0f, 0f, 0f)
				{
					delayBeforeAnimationStart = 100,
					id = idNum
				});
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f, flicker: true, flipped: false, (float)(y + 7) / 10000f, 0f, Color.White, 3f, 0f, 0f, 0f)
				{
					delayBeforeAnimationStart = 200,
					id = idNum
				});
				location.netAudio.StartPlaying("fuse");
				return true;
			}
			case "(O)288":
			{
				foreach (TemporaryAnimatedSprite temporarySprite3 in location.temporarySprites)
				{
					if (temporarySprite3.position.Equals(placementTile * 64f))
					{
						return false;
					}
				}
				int idNum = Game1.random.Next();
				location.playSound("thudStep");
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(base.ParentSheetIndex, 100f, 1, 24, placementTile * 64f, flicker: true, flipped: false, location, who)
				{
					shakeIntensity = 0.5f,
					shakeIntensityChange = 0.002f,
					extraInfoForEndBehavior = idNum,
					endFunction = location.removeTemporarySpritesWithID
				});
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f + new Vector2(5f, 0f) * 4f, flicker: true, flipped: false, (float)(y + 7) / 10000f, 0f, Color.Yellow, 4f, 0f, 0f, 0f)
				{
					id = idNum
				});
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f + new Vector2(5f, 0f) * 4f, flicker: true, flipped: true, (float)(y + 7) / 10000f, 0f, Color.Orange, 4f, 0f, 0f, 0f)
				{
					delayBeforeAnimationStart = 100,
					id = idNum
				});
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f + new Vector2(5f, 0f) * 4f, flicker: true, flipped: false, (float)(y + 7) / 10000f, 0f, Color.White, 3f, 0f, 0f, 0f)
				{
					delayBeforeAnimationStart = 200,
					id = idNum
				});
				location.netAudio.StartPlaying("fuse");
				return true;
			}
			case "(O)893":
			case "(O)894":
			case "(O)895":
			{
				int fireworkType = base.ParentSheetIndex - 893;
				int spriteX = 256 + fireworkType * 16;
				foreach (TemporaryAnimatedSprite temporarySprite4 in location.temporarySprites)
				{
					if (temporarySprite4.position.Equals(placementTile * 64f))
					{
						return false;
					}
				}
				int idNum = Game1.random.Next();
				int idNumFirework = Game1.random.Next();
				location.playSound("thudStep");
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(spriteX, 397, 16, 16), 2400f, 1, 1, placementTile * 64f, flicker: false, flipped: false, -1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					shakeIntensity = 0.5f,
					shakeIntensityChange = 0.002f,
					extraInfoForEndBehavior = idNum,
					endFunction = location.removeTemporarySpritesWithID,
					layerDepth = (placementTile.Y * 64f + 64f - 16f) / 10000f
				});
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(spriteX, 397, 16, 16), 800f, 1, 0, placementTile * 64f, flicker: false, flipped: false, -1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					fireworkType = fireworkType,
					delayBeforeAnimationStart = 2400,
					acceleration = new Vector2(0f, -0.36f + (float)Game1.random.Next(10) / 100f),
					drawAboveAlwaysFront = true,
					startSound = "firework",
					shakeIntensity = 0.5f,
					shakeIntensityChange = 0.002f,
					extraInfoForEndBehavior = idNumFirework,
					endFunction = location.removeTemporarySpritesWithID,
					id = Game1.random.Next(20, 31),
					Parent = location,
					owner = who
				});
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 40f, 5, 5, placementTile * 64f + new Vector2(11f, 12f) * 4f, flicker: true, flipped: false, (float)(y + 7) / 10000f, 0f, Color.Yellow, 4f, 0f, 0f, 0f)
				{
					id = idNum
				});
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 40f, 5, 5, placementTile * 64f + new Vector2(11f, 12f) * 4f, flicker: true, flipped: true, (float)(y + 7) / 10000f, 0f, Color.Orange, 4f, 0f, 0f, 0f)
				{
					delayBeforeAnimationStart = 100,
					id = idNum
				});
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 40f, 5, 5, placementTile * 64f + new Vector2(11f, 12f) * 4f, flicker: true, flipped: false, (float)(y + 7) / 10000f, 0f, Color.White, 3f, 0f, 0f, 0f)
				{
					delayBeforeAnimationStart = 200,
					id = idNum
				});
				location.netAudio.StartPlaying("fuse");
				DelayedAction.functionAfterDelay(delegate
				{
					location.netAudio.StopPlaying("fuse");
				}, 2400);
				return true;
			}
			case "(O)297":
				if (location.objects.ContainsKey(placementTile) || location.terrainFeatures.ContainsKey(placementTile))
				{
					return false;
				}
				location.terrainFeatures.Add(placementTile, new Grass(1, 4));
				location.playSound("dirtyHit");
				return true;
			case "(O)BlueGrassStarter":
				if (location.objects.ContainsKey(placementTile) || location.terrainFeatures.ContainsKey(placementTile))
				{
					return false;
				}
				location.terrainFeatures.Add(placementTile, new Grass(7, 4));
				location.playSound("dirtyHit");
				return true;
			case "(O)710":
				if (!CrabPot.IsValidCrabPotLocationTile(location, (int)placementTile.X, (int)placementTile.Y))
				{
					return false;
				}
				new CrabPot().placementAction(location, x, y, who);
				return true;
			case "(O)805":
				if (location.terrainFeatures.TryGetValue(placementTile, out terrainFeature) && terrainFeature is Tree tree)
				{
					return tree.fertilize();
				}
				return false;
			case "(O)419":
				if (location.terrainFeatures.TryGetValue(placementTile, out terrainFeature) && terrainFeature is Tree tree && !tree.stopGrowingMoss.Value)
				{
					tree.hasMoss.Value = false;
					tree.stopGrowingMoss.Value = true;
					Game1.playSound("slosh");
					Game1.playSound("glug");
					Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(21, tree.Tile * 64f + new Vector2(0f, -64f), new Color(165, 100, 255), 8, flipped: false, 80f, 1, -1, (tree.Tile.Y + 1.25f) * 64f / 10000f, 128), location);
					return true;
				}
				return false;
			}
		}
		else
		{
			if (IsTapper())
			{
				if (location.terrainFeatures.TryGetValue(placementTile, out terrainFeature) && terrainFeature is Tree tree && (int)tree.growthStage >= 5 && !tree.stump && !location.objects.ContainsKey(placementTile) && (!tree.isTemporaryGreenRainTree || Game1.season != Season.Summer))
				{
					WildTreeData data = tree.GetData();
					if (data != null && data.CanBeTapped())
					{
						Object tapper = (Object)getOne();
						tapper.heldObject.Value = null;
						tapper.TileLocation = placementTile;
						location.objects.Add(placementTile, tapper);
						tree.tapped.Value = true;
						tree.UpdateTapperProduct(tapper);
						location.playSound("axe");
						return true;
					}
				}
				return false;
			}
			if (HasContextTag("sign_item"))
			{
				if (location.objects.ContainsKey(placementTile))
				{
					return false;
				}
				location.objects.Add(placementTile, new Sign(placementTile, base.ItemId));
				location.playSound("axe");
				return true;
			}
			if (HasContextTag("torch_item"))
			{
				if (location.objects.ContainsKey(placementTile))
				{
					return false;
				}
				Torch torch = new Torch(base.ItemId, bigCraftable: true);
				torch.shakeTimer = 25;
				torch.placementAction(location, x, y, who);
				return true;
			}
			switch (base.QualifiedItemId)
			{
			case "(BC)108":
			case "(BC)109":
			{
				Object tub = (Object)getOne();
				tub.ResetParentSheetIndex();
				Season season = location.GetSeason();
				if (Location.IsOutdoors && (season == Season.Winter || season == Season.Fall))
				{
					tub.ParentSheetIndex = 109;
				}
				location.Objects.Add(placementTile, tub);
				Game1.playSound("axe");
				return true;
			}
			case "(BC)62":
				location.objects.Add(placementTile, new IndoorPot(placementTile));
				break;
			case "(BC)71":
				if (location is MineShaft mine)
				{
					if (mine.shouldCreateLadderOnThisLevel() && mine.recursiveTryToCreateLadderDown(placementTile))
					{
						MineShaft.numberOfCraftedStairsUsedThisRun++;
						return true;
					}
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"));
				}
				else if (location.Name.Equals("ManorHouse") && x >= 1088)
				{
					Game1.warpFarmer("LewisBasement", 4, 4, 2);
					Game1.playSound("stairsdown");
					Game1.screenGlowOnce(Color.Black, hold: true, 1f, 1f);
					return true;
				}
				return false;
			case "(BC)232":
			case "(BC)130":
				if (location.objects.ContainsKey(placementTile) || location is MineShaft || location is VolcanoDungeon)
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"));
					return false;
				}
				location.objects.Add(placementTile, new Chest(playerChest: true, placementTile, base.ItemId)
				{
					name = name,
					shakeTimer = 50
				});
				location.playSound((base.QualifiedItemId == "(BC)130") ? "axe" : "hammer");
				return true;
			case "(BC)BigChest":
			case "(BC)BigStoneChest":
			{
				if (location.objects.ContainsKey(placementTile) || location is MineShaft || location is VolcanoDungeon)
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"));
					return false;
				}
				Chest bigchest = new Chest(playerChest: true, placementTile, base.ItemId)
				{
					shakeTimer = 50
				};
				location.objects.Add(placementTile, bigchest);
				location.playSound((base.QualifiedItemId == "(BC)BigChest") ? "axe" : "hammer");
				return true;
			}
			case "(BC)163":
				location.objects.Add(placementTile, new Cask(placementTile));
				location.playSound("hammer");
				break;
			case "(BC)165":
			{
				Object autoGrabber = ItemRegistry.Create<Object>("(BC)165");
				location.objects.Add(placementTile, autoGrabber);
				autoGrabber.heldObject.Value = new Chest();
				location.playSound("axe");
				return true;
			}
			case "(BC)208":
				location.objects.Add(placementTile, new Workbench(placementTile));
				location.playSound("axe");
				return true;
			case "(BC)209":
			{
				MiniJukebox mini_jukebox = (this as MiniJukebox) ?? new MiniJukebox(placementTile);
				location.objects.Add(placementTile, mini_jukebox);
				mini_jukebox.RegisterToLocation();
				location.playSound("hammer");
				return true;
			}
			case "(BC)211":
			{
				WoodChipper wood_chipper = (this as WoodChipper) ?? new WoodChipper(placementTile);
				wood_chipper.placementAction(location, x, y);
				location.objects.Add(placementTile, wood_chipper);
				location.playSound("hammer");
				return true;
			}
			case "(BC)214":
			{
				Phone phone = (this as Phone) ?? new Phone(placementTile);
				location.objects.Add(placementTile, phone);
				location.playSound("hammer");
				return true;
			}
			case "(BC)216":
			{
				if (location.objects.ContainsKey(placementTile))
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"));
					return false;
				}
				if (!(location is FarmHouse) && !(location is IslandFarmHouse))
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"));
					return false;
				}
				if (location is FarmHouse { upgradeLevel: <1 })
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:MiniFridge_NoKitchen"));
					return false;
				}
				Chest fridge = new Chest("216", placementTile, 217, 2)
				{
					shakeTimer = 50
				};
				fridge.fridge.Value = true;
				location.objects.Add(placementTile, fridge);
				location.playSound("hammer");
				return true;
			}
			case "(BC)248":
				if (location.objects.ContainsKey(placementTile) || location is MineShaft || location is VolcanoDungeon)
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"));
					return false;
				}
				location.objects.Add(placementTile, new Chest(playerChest: true, placementTile, base.ItemId)
				{
					name = name,
					shakeTimer = 50
				});
				location.playSound("axe");
				return true;
			case "(BC)238":
			{
				if (!(location is Farm))
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:OnlyPlaceOnFarm"));
					return false;
				}
				Vector2 obelisk1 = Vector2.Zero;
				Vector2 obelisk2 = Vector2.Zero;
				foreach (KeyValuePair<Vector2, Object> o in location.objects.Pairs)
				{
					if (o.Value.QualifiedItemId == "(BC)238")
					{
						if (obelisk1.Equals(Vector2.Zero))
						{
							obelisk1 = o.Key;
						}
						else if (obelisk2.Equals(Vector2.Zero))
						{
							obelisk2 = o.Key;
							break;
						}
					}
				}
				if (!obelisk1.Equals(Vector2.Zero) && !obelisk2.Equals(Vector2.Zero))
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:OnlyPlaceTwo"));
					return false;
				}
				break;
			}
			case "(BC)254":
				if (!(location is AnimalHouse animalHouse) || !animalHouse.name.Contains("Barn"))
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:MustBePlacedInBarn"));
					return false;
				}
				break;
			case "(BC)256":
				if (location.objects.ContainsKey(placementTile) || location is MineShaft || location is VolcanoDungeon)
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"));
					return false;
				}
				location.objects.Add(placementTile, new Chest(playerChest: true, placementTile, base.ItemId)
				{
					name = name,
					shakeTimer = 50
				});
				location.playSound("axe");
				return true;
			case "(BC)275":
			{
				if (location.objects.ContainsKey(placementTile) || location is MineShaft || location is VolcanoDungeon)
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"));
					return false;
				}
				Chest chest = new Chest(playerChest: true, placementTile, base.ItemId)
				{
					name = name,
					shakeTimer = 50
				};
				chest.lidFrameCount.Value = 2;
				location.objects.Add(placementTile, chest);
				location.playSound("axe");
				return true;
			}
			}
		}
		if (base.Category == -19 && location.terrainFeatures.TryGetValue(placementTile, out terrainFeature) && terrainFeature is HoeDirt { crop: not null } dirt && (base.QualifiedItemId == "(O)369" || base.QualifiedItemId == "(O)368") && (int)dirt.crop.currentPhase != 0)
		{
			Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HoeDirt.cs.13916"));
			return false;
		}
		if (isSapling())
		{
			if (IsWildTreeSapling() || IsFruitTreeSapling())
			{
				if (FruitTree.IsTooCloseToAnotherTree(new Vector2(x / 64, y / 64), location))
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13060"));
					return false;
				}
				if (FruitTree.IsGrowthBlocked(new Vector2(x / 64, y / 64), location))
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:FruitTree_PlacementWarning", DisplayName));
					return false;
				}
			}
			if (location.terrainFeatures.TryGetValue(placementTile, out terrainFeature))
			{
				if (!(terrainFeature is HoeDirt { crop: null }))
				{
					return false;
				}
				location.terrainFeatures.Remove(placementTile);
			}
			string deniedMessage = null;
			bool canDig = location.doesTileHaveProperty((int)placementTile.X, (int)placementTile.Y, "Diggable", "Back") != null;
			string tileType = location.doesTileHaveProperty((int)placementTile.X, (int)placementTile.Y, "Type", "Back");
			bool canPlantTrees = location.doesEitherTileOrTileIndexPropertyEqual((int)placementTile.X, (int)placementTile.Y, "CanPlantTrees", "Back", "T");
			if ((location is Farm && (canDig || tileType == "Grass" || tileType == "Dirt" || canPlantTrees) && (!location.IsNoSpawnTile(placementTile, "Tree") || canPlantTrees)) || ((canDig || tileType == "Stone") && location.CanPlantTreesHere(base.ItemId, (int)placementTile.X, (int)placementTile.Y, out deniedMessage)))
			{
				location.playSound("dirtyHit");
				DelayedAction.playSoundAfterDelay("coin", 100);
				if (IsTeaSapling())
				{
					location.terrainFeatures.Add(placementTile, new Bush(placementTile, 3, location));
					return true;
				}
				FruitTree fruitTree = new FruitTree(base.ItemId)
				{
					GreenHouseTileTree = (location.IsGreenhouse && tileType == "Stone")
				};
				fruitTree.growthRate.Value = Math.Max(1, base.Quality + 1);
				location.terrainFeatures.Add(placementTile, fruitTree);
				return true;
			}
			if (deniedMessage == null)
			{
				deniedMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13068");
			}
			Game1.showRedMessage(deniedMessage);
			return false;
		}
		if (base.Category == -74 || base.Category == -19)
		{
			if (location.terrainFeatures.TryGetValue(placementTile, out terrainFeature) && terrainFeature is HoeDirt dirt)
			{
				string seedId = Crop.ResolveSeedId(who.ActiveObject.ItemId, location);
				if (dirt.canPlantThisSeedHere(seedId, who.ActiveObject.Category == -19))
				{
					if (dirt.plant(seedId, who, who.ActiveObject.Category == -19) && who.IsLocalPlayer)
					{
						if (base.Category == -74)
						{
							foreach (Object o in location.Objects.Values)
							{
								if (!o.IsSprinkler() || o.heldObject.Value == null || !(o.heldObject.Value.QualifiedItemId == "(O)913") || !o.IsInSprinklerRangeBroadphase(placementTile))
								{
									continue;
								}
								if (!o.GetSprinklerTiles().Contains(placementTile))
								{
									continue;
								}
								Object value = o.heldObject.Value.heldObject.Value;
								Chest chest = value as Chest;
								if (chest == null)
								{
									continue;
								}
								IInventory items = chest.Items;
								if (items.Count <= 0 || items[0] == null || chest.GetMutex().IsLocked())
								{
									continue;
								}
								chest.GetMutex().RequestLock(delegate
								{
									if (items.Count > 0 && items[0] != null)
									{
										Item item = items[0];
										if (item.Category == -19 && ((HoeDirt)terrainFeature).plant(item.ItemId, who, isFertilizer: true))
										{
											item.Stack--;
											if (item.Stack <= 0)
											{
												items[0] = null;
											}
										}
									}
									chest.GetMutex().ReleaseLock();
								});
								break;
							}
						}
						Game1.haltAfterCheck = false;
						return true;
					}
					return false;
				}
				return false;
			}
			return false;
		}
		if (!performDropDownAction(who))
		{
			Object toPlace = (Object)getOne();
			bool place_furniture_instance_instead = false;
			if (toPlace.GetType() == typeof(Furniture) && Furniture.GetFurnitureInstance(base.ItemId, new Vector2(x / 64, y / 64)).GetType() != toPlace.GetType())
			{
				StorageFurniture storageFurniture = new StorageFurniture(base.ItemId, new Vector2(x / 64, y / 64));
				storageFurniture.currentRotation.Value = (this as Furniture).currentRotation.Value;
				storageFurniture.updateRotation();
				toPlace = storageFurniture;
				place_furniture_instance_instead = true;
			}
			toPlace.shakeTimer = 50;
			toPlace.Location = location;
			toPlace.TileLocation = placementTile;
			toPlace.performDropDownAction(who);
			if (toPlace.QualifiedItemId == "(BC)TextSign")
			{
				toPlace.signText.Value = null;
				toPlace.showNextIndex.Value = true;
			}
			if (toPlace.name.Contains("Seasonal"))
			{
				int baseIndex = toPlace.ParentSheetIndex - toPlace.ParentSheetIndex % 4;
				toPlace.ParentSheetIndex = baseIndex + location.GetSeasonIndex();
			}
			if (!(toPlace is Furniture) && location.objects.TryGetValue(placementTile, out var tileObj))
			{
				if (tileObj.QualifiedItemId != base.QualifiedItemId)
				{
					Game1.createItemDebris(tileObj, placementTile * 64f, Game1.random.Next(4));
					location.objects[placementTile] = toPlace;
				}
			}
			else if (toPlace is Furniture furniture)
			{
				if (place_furniture_instance_instead)
				{
					location.furniture.Add(furniture);
				}
				else
				{
					location.furniture.Add(this as Furniture);
				}
			}
			else
			{
				location.objects.Add(placementTile, toPlace);
			}
			toPlace.initializeLightSource(placementTile);
		}
		location.playSound("woodyStep");
		return true;
	}

	/// <inheritdoc />
	protected override void MigrateLegacyItemId()
	{
		if (bigCraftable.Value && !Game1.bigCraftableData.ContainsKey(base.ParentSheetIndex.ToString()))
		{
			if (base.ParentSheetIndex >= 56 && base.ParentSheetIndex <= 61)
			{
				base.ItemId = "56";
				return;
			}
			if (base.ParentSheetIndex >= 101 && base.ParentSheetIndex <= 103)
			{
				SetIdAndSprite(101);
				return;
			}
			if (name.Contains("Seasonal"))
			{
				base.ItemId = (base.ParentSheetIndex - base.ParentSheetIndex % 4).ToString();
				return;
			}
			if (Game1.bigCraftableData.ContainsKey((base.ParentSheetIndex - 1).ToString()))
			{
				base.ItemId = (base.ParentSheetIndex - 1).ToString();
				return;
			}
		}
		base.MigrateLegacyItemId();
	}

	/// <inheritdoc />
	public override bool actionWhenPurchased(string shopId)
	{
		if (base.QualifiedItemId == "(O)434")
		{
			if (!Game1.isFestival())
			{
				Game1.player.mailReceived.Add("CF_Sewer");
			}
			else
			{
				Game1.player.mailReceived.Add("CF_Fair");
			}
			Game1.exitActiveMenu();
			Game1.player.eatObject(this, overrideFullness: true);
		}
		if (base.actionWhenPurchased(shopId))
		{
			return true;
		}
		return isRecipe;
	}

	public virtual bool needsToBeDonated()
	{
		return LibraryMuseum.IsItemSuitableForDonation(base.QualifiedItemId);
	}

	public override string getDescription()
	{
		if (base.Category == -102 && Game1.player.stats.Get(itemId) != 0 && base.ItemId != "Book_PriceCatalogue" && base.ItemId != "Book_AnimalCatalogue")
		{
			foreach (string s in GetContextTags())
			{
				if (s.StartsWith("book_xp"))
				{
					string whichSkill = s.Split('_')[2];
					return Game1.parseText(Game1.content.LoadString("Strings\\1_6_Strings:alreadyreadbook", Farmer.getSkillDisplayNameFromIndex(Farmer.getSkillNumberFromName(whichSkill)).ToLower()), Game1.smallFont, getDescriptionWidth());
				}
			}
			return Game1.parseText(Game1.content.LoadString("Strings\\1_6_Strings:alreadyreadbook_random"), Game1.smallFont, getDescriptionWidth());
		}
		if ((bool)isRecipe)
		{
			if (base.Category == -7)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13073", loadDisplayName());
			}
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13074", loadDisplayName());
		}
		if (needsToBeDonated())
		{
			return Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13078"), Game1.smallFont, getDescriptionWidth());
		}
		string text = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId).Description;
		if (preservedParentSheetIndex.Value != null && (base.QualifiedItemId == "(O)SpecificBait" || base.QualifiedItemId == "(O)DriedFruit" || base.QualifiedItemId == "(O)SmokedFish"))
		{
			text = string.Format(text, ItemRegistry.GetDataOrErrorItem(preservedParentSheetIndex.Value).DisplayName);
		}
		return Game1.parseText(text, Game1.smallFont, getDescriptionWidth());
	}

	/// <inheritdoc />
	public override int sellToStorePrice(long specificPlayerID = -1L)
	{
		if (this is Fence)
		{
			return price;
		}
		if (base.Category == -22)
		{
			return (int)((float)(int)price * (1f + (float)(int)quality * 0.25f) * (((float)(FishingRod.maxTackleUses - uses.Value) + 0f) / (float)FishingRod.maxTackleUses));
		}
		float salePrice = (int)((float)(int)price * (1f + (float)base.Quality * 0.25f));
		salePrice = getPriceAfterMultipliers(salePrice, specificPlayerID);
		if (base.QualifiedItemId == "(O)493")
		{
			salePrice /= 2f;
		}
		if (salePrice > 0f)
		{
			salePrice = Math.Max(1f, salePrice * Game1.MasterPlayer.difficultyModifier);
		}
		return (int)salePrice;
	}

	/// <inheritdoc />
	public override int salePrice(bool ignoreProfitMargins = false)
	{
		if (this is Fence)
		{
			return price;
		}
		if ((bool)isRecipe)
		{
			return (int)price * 10;
		}
		switch (base.QualifiedItemId)
		{
		case "(O)388":
			if (Game1.year <= 1)
			{
				return 10;
			}
			return 50;
		case "(O)390":
			if (Game1.year <= 1)
			{
				return 20;
			}
			return 100;
		case "(O)382":
			if (Game1.year <= 1)
			{
				return 120;
			}
			return 250;
		case "(O)378":
			if (Game1.year <= 1)
			{
				return 80;
			}
			return 160;
		case "(O)380":
			if (Game1.year <= 1)
			{
				return 150;
			}
			return 250;
		case "(O)384":
			if (Game1.year <= 1)
			{
				return 350;
			}
			return 750;
		default:
		{
			float salePrice = (int)((float)((int)price * 2) * (1f + (float)(int)quality * 0.25f));
			if (!ignoreProfitMargins && appliesProfitMargins())
			{
				salePrice = (int)Math.Max(1f, salePrice * Game1.MasterPlayer.difficultyModifier);
			}
			return (int)salePrice;
		}
		}
	}

	/// <inheritdoc />
	public override bool appliesProfitMargins()
	{
		if ((int)category != -74 && !isSapling())
		{
			return base.appliesProfitMargins();
		}
		return true;
	}

	protected virtual float getPriceAfterMultipliers(float startPrice, long specificPlayerID = -1L)
	{
		bool animalGood = false;
		if (name != null && (name.ToLower().Contains("mayonnaise") || name.ToLower().Contains("cheese") || name.ToLower().Contains("cloth") || name.ToLower().Contains("wool")))
		{
			animalGood = true;
		}
		float saleMultiplier = 1f;
		foreach (Farmer player in Game1.getAllFarmers())
		{
			if (Game1.player.useSeparateWallets)
			{
				if (specificPlayerID == -1)
				{
					if (player.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID || !player.isActive())
					{
						continue;
					}
				}
				else if (player.UniqueMultiplayerID != specificPlayerID)
				{
					continue;
				}
			}
			else if (!player.isActive())
			{
				continue;
			}
			float multiplier = 1f;
			if (player.professions.Contains(0) && (animalGood || base.Category == -5 || base.Category == -6 || base.Category == -18))
			{
				multiplier *= 1.2f;
			}
			if (player.professions.Contains(1) && (base.Category == -75 || base.Category == -80 || (base.Category == -79 && !isSpawnedObject)))
			{
				multiplier *= 1.1f;
			}
			if (player.professions.Contains(4) && base.Category == -26)
			{
				multiplier *= 1.4f;
			}
			if (player.professions.Contains(6) && (base.Category == -4 || (preserve != null && preserve.Value.HasValue && preserve.Value == PreserveType.SmokedFish)))
			{
				multiplier *= (player.professions.Contains(8) ? 1.5f : 1.25f);
			}
			if (player.professions.Contains(15) && base.Category == -27)
			{
				multiplier *= 1.25f;
			}
			if (player.professions.Contains(20) && IsBar())
			{
				multiplier *= 1.5f;
			}
			if (player.professions.Contains(23) && (base.Category == -2 || base.Category == -12))
			{
				multiplier *= 1.3f;
			}
			if (player.eventsSeen.Contains("2120303") && (base.QualifiedItemId == "(O)296" || base.QualifiedItemId == "(O)410"))
			{
				multiplier *= 3f;
			}
			if (player.eventsSeen.Contains("3910979") && base.QualifiedItemId == "(O)399")
			{
				multiplier *= 5f;
			}
			if (player.stats.Get("Book_Artifact") != 0 && Type != null && Type.Equals("Arch"))
			{
				multiplier *= 3f;
			}
			saleMultiplier = Math.Max(saleMultiplier, multiplier);
		}
		return startPrice * saleMultiplier;
	}

	/// <inheritdoc />
	public override bool ForEachItem(ForEachItemDelegate handler)
	{
		if (base.ForEachItem(handler))
		{
			return ForEachItemHelper.ApplyToField(heldObject, handler);
		}
		return false;
	}
}
