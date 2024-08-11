using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using Netcode.Validation;
using StardewValley.Audio;
using StardewValley.BellsAndWhistles;
using StardewValley.Buffs;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Companions;
using StardewValley.Constants;
using StardewValley.Enchantments;
using StardewValley.Extensions;
using StardewValley.GameData.LocationContexts;
using StardewValley.GameData.Pants;
using StardewValley.GameData.Shirts;
using StardewValley.Inventories;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Network.NetEvents;
using StardewValley.Objects;
using StardewValley.Quests;
using StardewValley.SpecialOrders;
using StardewValley.Tools;
using StardewValley.Util;
using xTile.Dimensions;
using xTile.Tiles;

namespace StardewValley;

public class Farmer : Character, IComparable
{
	public class EmoteType
	{
		public string emoteString = "";

		public int emoteIconIndex = -1;

		public FarmerSprite.AnimationFrame[] animationFrames;

		public bool hidden;

		public int facingDirection = 2;

		public string displayNameKey;

		public string displayName => Game1.content.LoadString(displayNameKey);

		public EmoteType(string emote_string = "", string display_name_key = "", int icon_index = -1, FarmerSprite.AnimationFrame[] frames = null, int facing_direction = 2, bool is_hidden = false)
		{
			emoteString = emote_string;
			emoteIconIndex = icon_index;
			animationFrames = frames;
			facingDirection = facing_direction;
			hidden = is_hidden;
			displayNameKey = "Strings\\UI:" + display_name_key;
		}
	}

	public const int millisecondsPerSpeedUnit = 64;

	public const byte halt = 64;

	public const byte up = 1;

	public const byte right = 2;

	public const byte down = 4;

	public const byte left = 8;

	public const byte run = 16;

	public const byte release = 32;

	public const int farmingSkill = 0;

	public const int miningSkill = 3;

	public const int fishingSkill = 1;

	public const int foragingSkill = 2;

	public const int combatSkill = 4;

	public const int luckSkill = 5;

	public const float interpolationConstant = 0.5f;

	public const int runningSpeed = 5;

	public const int walkingSpeed = 2;

	public const int caveNothing = 0;

	public const int caveBats = 1;

	public const int caveMushrooms = 2;

	public const int millisecondsInvincibleAfterDamage = 1200;

	public const int millisecondsPerFlickerWhenInvincible = 50;

	public const int startingStamina = 270;

	public const int totalLevels = 35;

	public const int maxInventorySpace = 36;

	public const int hotbarSize = 12;

	public const int eyesOpen = 0;

	public const int eyesHalfShut = 4;

	public const int eyesClosed = 1;

	public const int eyesRight = 2;

	public const int eyesLeft = 3;

	public const int eyesWide = 5;

	public const int rancher = 0;

	public const int tiller = 1;

	public const int butcher = 2;

	public const int shepherd = 3;

	public const int artisan = 4;

	public const int agriculturist = 5;

	public const int fisher = 6;

	public const int trapper = 7;

	public const int angler = 8;

	public const int pirate = 9;

	public const int baitmaster = 10;

	public const int mariner = 11;

	public const int forester = 12;

	public const int gatherer = 13;

	public const int lumberjack = 14;

	public const int tapper = 15;

	public const int botanist = 16;

	public const int tracker = 17;

	public const int miner = 18;

	public const int geologist = 19;

	public const int blacksmith = 20;

	public const int burrower = 21;

	public const int excavator = 22;

	public const int gemologist = 23;

	public const int fighter = 24;

	public const int scout = 25;

	public const int brute = 26;

	public const int defender = 27;

	public const int acrobat = 28;

	public const int desperado = 29;

	public static int MaximumTrinkets = 1;

	public readonly NetObjectList<Quest> questLog = new NetObjectList<Quest>();

	public readonly NetIntHashSet professions = new NetIntHashSet();

	public readonly NetList<Point, NetPoint> newLevels = new NetList<Point, NetPoint>();

	private Queue<int> newLevelSparklingTexts = new Queue<int>();

	private SparklingText sparklingText;

	public readonly NetArray<int, NetInt> experiencePoints = new NetArray<int, NetInt>(6);

	/// <summary>The backing field for <see cref="P:StardewValley.Farmer.Items" />.</summary>
	[XmlElement("items")]
	public readonly NetRef<Inventory> netItems = new NetRef<Inventory>(new Inventory());

	[XmlArrayItem("int")]
	public readonly NetStringHashSet dialogueQuestionsAnswered = new NetStringHashSet();

	[XmlElement("cookingRecipes")]
	public readonly NetStringDictionary<int, NetInt> cookingRecipes = new NetStringDictionary<int, NetInt>();

	[XmlElement("craftingRecipes")]
	public readonly NetStringDictionary<int, NetInt> craftingRecipes = new NetStringDictionary<int, NetInt>();

	[XmlElement("activeDialogueEvents")]
	public readonly NetStringDictionary<int, NetInt> activeDialogueEvents = new NetStringDictionary<int, NetInt>();

	[XmlElement("previousActiveDialogueEvents")]
	public readonly NetStringDictionary<int, NetInt> previousActiveDialogueEvents = new NetStringDictionary<int, NetInt>();

	/// <summary>The trigger actions which have been run for the player.</summary>
	public readonly NetStringHashSet triggerActionsRun = new NetStringHashSet();

	/// <summary>The event IDs which the player has seen.</summary>
	[XmlArrayItem("int")]
	public readonly NetStringHashSet eventsSeen = new NetStringHashSet();

	public readonly NetIntHashSet secretNotesSeen = new NetIntHashSet();

	public HashSet<string> songsHeard = new HashSet<string>();

	public readonly NetIntHashSet achievements = new NetIntHashSet();

	[XmlArrayItem("int")]
	public readonly NetStringList specialItems = new NetStringList();

	[XmlArrayItem("int")]
	public readonly NetStringList specialBigCraftables = new NetStringList();

	/// <summary>The mail flags set on the player. This includes both actual mail letter IDs matching <c>Data/mail</c>, and non-mail flags used to track game state like <c>ccIsComplete</c> (community center complete).</summary>
	/// <remarks>See also <see cref="F:StardewValley.Farmer.mailForTomorrow" /> and <see cref="F:StardewValley.Farmer.mailbox" />.</remarks>
	public readonly NetStringHashSet mailReceived = new NetStringHashSet();

	/// <summary>The mail flags that will be added to the <see cref="F:StardewValley.Farmer.mailbox" /> tomorrow.</summary>
	public readonly NetStringHashSet mailForTomorrow = new NetStringHashSet();

	/// <summary>The mail IDs matching <c>Data/mail</c> in the player's mailbox, if any. Each time the player checks their mailbox, one letter from this set will be displayed and moved into <see cref="F:StardewValley.Farmer.mailReceived" />.</summary>
	public readonly NetStringList mailbox = new NetStringList();

	/// <summary>The internal names of locations which the player has previously visited.</summary>
	/// <remarks>This contains the <see cref="P:StardewValley.GameLocation.Name" /> field, not <see cref="P:StardewValley.GameLocation.NameOrUniqueName" />. They're equivalent for most locations, but building interiors will use their common name (like <c>Barn</c> instead of <c>Barn{unique ID}</c> for barns).</remarks>
	public readonly NetStringHashSet locationsVisited = new NetStringHashSet();

	public readonly NetInt timeWentToBed = new NetInt();

	[XmlIgnore]
	public readonly NetList<Companion, NetRef<Companion>> companions = new NetList<Companion, NetRef<Companion>>();

	[XmlIgnore]
	public bool hasMoved;

	[XmlIgnore]
	public bool hasBeenBlessedByStatueToday;

	public readonly NetBool sleptInTemporaryBed = new NetBool();

	[XmlIgnore]
	public readonly NetBool requestingTimePause = new NetBool
	{
		InterpolationWait = false
	};

	public Stats stats = new Stats();

	[XmlIgnore]
	public readonly NetRef<Inventory> personalShippingBin = new NetRef<Inventory>(new Inventory());

	[XmlIgnore]
	public IList<Item> displayedShippedItems = new List<Item>();

	[XmlElement("biteChime")]
	public NetInt biteChime = new NetInt(-1);

	[XmlIgnore]
	public float usernameDisplayTime;

	[XmlIgnore]
	protected NetRef<Item> _recoveredItem = new NetRef<Item>();

	public NetObjectList<Item> itemsLostLastDeath = new NetObjectList<Item>();

	public List<int> movementDirections = new List<int>();

	[XmlElement("farmName")]
	public readonly NetString farmName = new NetString("");

	[XmlElement("favoriteThing")]
	public readonly NetString favoriteThing = new NetString();

	[XmlElement("horseName")]
	public readonly NetString horseName = new NetString();

	public string slotName;

	public bool slotCanHost;

	[XmlIgnore]
	public readonly NetString tempFoodItemTextureName = new NetString();

	[XmlIgnore]
	public readonly NetRectangle tempFoodItemSourceRect = new NetRectangle();

	[XmlIgnore]
	public bool hasReceivedToolUpgradeMessageYet;

	[XmlIgnore]
	public readonly BuffManager buffs = new BuffManager();

	[XmlIgnore]
	public IList<OutgoingMessage> messageQueue = new List<OutgoingMessage>();

	[XmlIgnore]
	public readonly NetLong uniqueMultiplayerID = new NetLong(Utility.RandomLong());

	[XmlElement("userID")]
	public readonly NetString userID = new NetString("");

	[XmlIgnore]
	public string previousLocationName = "";

	[XmlIgnore]
	public readonly NetString platformType = new NetString("");

	[XmlIgnore]
	public readonly NetString platformID = new NetString("");

	[XmlIgnore]
	public readonly NetBool hasMenuOpen = new NetBool(value: false);

	[XmlIgnore]
	public readonly Color DEFAULT_SHIRT_COLOR = Color.White;

	public string defaultChatColor;

	/// <summary>Obsolete since 1.6. This is only kept to preserve data from old save files. Use <see cref="F:StardewValley.Farmer.whichPetType" /> instead.</summary>
	[XmlElement("catPerson")]
	public bool? obsolete_catPerson;

	/// <summary>Obsolete since 1.6. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Farmer.canUnderstandDwarves" /> instead.</summary>
	[XmlElement("canUnderstandDwarves")]
	public bool? obsolete_canUnderstandDwarves;

	/// <summary>Obsolete since 1.6. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Farmer.hasClubCard" /> instead.</summary>
	[XmlElement("hasClubCard")]
	public bool? obsolete_hasClubCard;

	/// <summary>Obsolete since 1.6. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Farmer.hasDarkTalisman" /> instead.</summary>
	[XmlElement("hasDarkTalisman")]
	public bool? obsolete_hasDarkTalisman;

	/// <summary>Obsolete since 1.6. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Farmer.hasMagicInk" /> instead.</summary>
	[XmlElement("hasMagicInk")]
	public bool? obsolete_hasMagicInk;

	/// <summary>Obsolete since 1.6. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Farmer.hasMagnifyingGlass" /> instead.</summary>
	[XmlElement("hasMagnifyingGlass")]
	public bool? obsolete_hasMagnifyingGlass;

	/// <summary>Obsolete since 1.6. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Farmer.hasRustyKey" /> instead.</summary>
	[XmlElement("hasRustyKey")]
	public bool? obsolete_hasRustyKey;

	/// <summary>Obsolete since 1.6. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Farmer.hasSkullKey" /> instead.</summary>
	[XmlElement("hasSkullKey")]
	public bool? obsolete_hasSkullKey;

	/// <summary>Obsolete since 1.6. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Farmer.hasSpecialCharm" /> instead.</summary>
	[XmlElement("hasSpecialCharm")]
	public bool? obsolete_hasSpecialCharm;

	/// <summary>Obsolete since 1.6. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Farmer.HasTownKey" /> instead.</summary>
	[XmlElement("HasTownKey")]
	public bool? obsolete_hasTownKey;

	/// <summary>Obsolete since 1.6. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Farmer.hasUnlockedSkullDoor" /> instead.</summary>
	[XmlElement("hasUnlockedSkullDoor")]
	public bool? obsolete_hasUnlockedSkullDoor;

	/// <summary>Obsolete since 1.3. This is only kept to preserve data from old save files. Use <see cref="F:StardewValley.Farmer.friendshipData" /> for NPC friendships or <see cref="F:StardewValley.FarmerTeam.friendshipData" /> for farmhands instead.</summary>
	[XmlElement("friendships")]
	public SerializableDictionary<string, int[]> obsolete_friendships;

	/// <summary>Obsolete since 1.3. This is only kept to preserve data from old save files. Use <see cref="M:StardewValley.Farmer.GetDaysMarried" /> instead.</summary>
	[XmlElement("daysMarried")]
	public int? obsolete_daysMarried;

	/// <summary>The preferred pet type, matching an ID in <c>Data/Pets</c>. The vanilla pet types are <see cref="F:StardewValley.Characters.Pet.type_cat" /> and <see cref="F:StardewValley.Characters.Pet.type_dog" />.</summary>
	public string whichPetType = "Cat";

	/// <summary>The selected breed ID in <c>Data/Pets</c> for the <see cref="F:StardewValley.Farmer.whichPetType" />.</summary>
	public string whichPetBreed = "0";

	[XmlIgnore]
	public bool isAnimatingMount;

	[XmlElement("acceptedDailyQuest")]
	public readonly NetBool acceptedDailyQuest = new NetBool(value: false);

	[XmlIgnore]
	public Item mostRecentlyGrabbedItem;

	[XmlIgnore]
	public Item itemToEat;

	[XmlElement("farmerRenderer")]
	private readonly NetRef<FarmerRenderer> farmerRenderer = new NetRef<FarmerRenderer>();

	[XmlIgnore]
	public readonly NetInt toolPower = new NetInt();

	[XmlIgnore]
	public readonly NetInt toolHold = new NetInt();

	public Vector2 mostRecentBed;

	public static Dictionary<int, string> hairStyleMetadataFile = null;

	public static List<int> allHairStyleIndices = null;

	[XmlIgnore]
	public static Dictionary<int, HairStyleMetadata> hairStyleMetadata = new Dictionary<int, HairStyleMetadata>();

	[XmlElement("emoteFavorites")]
	public readonly List<string> emoteFavorites = new List<string>();

	[XmlElement("performedEmotes")]
	public readonly SerializableDictionary<string, bool> performedEmotes = new SerializableDictionary<string, bool>();

	/// <summary>If set, the unqualified item ID of the <see cref="F:StardewValley.ItemRegistry.type_shirt" /> item to show this player wearing instead of the equipped <see cref="F:StardewValley.Farmer.shirtItem" />.</summary>
	[XmlElement("shirt")]
	public readonly NetString shirt = new NetString("1000");

	[XmlElement("hair")]
	public readonly NetInt hair = new NetInt(0);

	[XmlElement("skin")]
	public readonly NetInt skin = new NetInt(0);

	[XmlElement("shoes")]
	public readonly NetString shoes = new NetString("2");

	[XmlElement("accessory")]
	public readonly NetInt accessory = new NetInt(-1);

	[XmlElement("facialHair")]
	public readonly NetInt facialHair = new NetInt(-1);

	/// <summary>If set, the unqualified item ID of the <see cref="F:StardewValley.ItemRegistry.type_pants" /> item to show this player wearing instead of the equipped <see cref="F:StardewValley.Farmer.pantsItem" />.</summary>
	[XmlElement("pants")]
	public readonly NetString pants = new NetString("0");

	[XmlIgnore]
	public int currentEyes;

	[XmlIgnore]
	public int blinkTimer;

	[XmlIgnore]
	public readonly NetInt netFestivalScore = new NetInt();

	/// <summary>The last date that we submitted the Calico Egg Rating to Gil.</summary>
	public readonly NetRef<WorldDate> lastGotPrizeFromGil = new NetRef<WorldDate>();

	[XmlIgnore]
	public float temporarySpeedBuff;

	[XmlElement("hairstyleColor")]
	public readonly NetColor hairstyleColor = new NetColor(new Color(193, 90, 50));

	[XmlIgnore]
	public NetBool prismaticHair = new NetBool();

	/// <summary>The color to apply when rendering <see cref="F:StardewValley.Farmer.pants" />. Most code should use <see cref="M:StardewValley.Farmer.GetPantsColor" /> instead.</summary>
	[XmlElement("pantsColor")]
	public readonly NetColor pantsColor = new NetColor(new Color(46, 85, 183));

	[XmlElement("newEyeColor")]
	public readonly NetColor newEyeColor = new NetColor(new Color(122, 68, 52));

	[XmlElement("hat")]
	public readonly NetRef<Hat> hat = new NetRef<Hat>();

	[XmlElement("boots")]
	public readonly NetRef<Boots> boots = new NetRef<Boots>();

	[XmlElement("leftRing")]
	public readonly NetRef<Ring> leftRing = new NetRef<Ring>();

	[XmlElement("rightRing")]
	public readonly NetRef<Ring> rightRing = new NetRef<Ring>();

	[XmlElement("shirtItem")]
	public readonly NetRef<Clothing> shirtItem = new NetRef<Clothing>();

	[XmlElement("pantsItem")]
	public readonly NetRef<Clothing> pantsItem = new NetRef<Clothing>();

	[XmlIgnore]
	public readonly NetDancePartner dancePartner = new NetDancePartner();

	[XmlIgnore]
	public bool ridingMineElevator;

	[XmlIgnore]
	public readonly NetBool exhausted = new NetBool();

	[XmlElement("divorceTonight")]
	public readonly NetBool divorceTonight = new NetBool();

	[XmlElement("changeWalletTypeTonight")]
	public readonly NetBool changeWalletTypeTonight = new NetBool();

	[XmlIgnore]
	public AnimatedSprite.endOfAnimationBehavior toolOverrideFunction;

	[XmlIgnore]
	public NetBool onBridge = new NetBool();

	[XmlIgnore]
	public SuspensionBridge bridge;

	private readonly NetInt netDeepestMineLevel = new NetInt();

	[XmlElement("currentToolIndex")]
	private readonly NetInt currentToolIndex = new NetInt(0);

	[XmlIgnore]
	private readonly NetRef<Item> temporaryItem = new NetRef<Item>();

	[XmlIgnore]
	private readonly NetRef<Item> cursorSlotItem = new NetRef<Item>();

	[XmlIgnore]
	public readonly NetBool netItemStowed = new NetBool(value: false);

	protected bool _itemStowed;

	public string gameVersion = "-1";

	public string gameVersionLabel;

	[XmlIgnore]
	public bool isFakeEventActor;

	[XmlElement("bibberstyke")]
	public readonly NetInt bobberStyle = new NetInt(0);

	public bool usingRandomizedBobber;

	[XmlElement("caveChoice")]
	public readonly NetInt caveChoice = new NetInt();

	[XmlElement("farmingLevel")]
	public readonly NetInt farmingLevel = new NetInt();

	[XmlElement("miningLevel")]
	public readonly NetInt miningLevel = new NetInt();

	[XmlElement("combatLevel")]
	public readonly NetInt combatLevel = new NetInt();

	[XmlElement("foragingLevel")]
	public readonly NetInt foragingLevel = new NetInt();

	[XmlElement("fishingLevel")]
	public readonly NetInt fishingLevel = new NetInt();

	[XmlElement("luckLevel")]
	public readonly NetInt luckLevel = new NetInt();

	[XmlElement("maxStamina")]
	public readonly NetInt maxStamina = new NetInt(270);

	[XmlElement("maxItems")]
	public readonly NetInt maxItems = new NetInt(12);

	[XmlElement("lastSeenMovieWeek")]
	public readonly NetInt lastSeenMovieWeek = new NetInt(-1);

	[XmlIgnore]
	public readonly NetString viewingLocation = new NetString();

	private readonly NetFloat netStamina = new NetFloat(270f);

	[XmlIgnore]
	public bool ignoreItemConsumptionThisFrame;

	[XmlIgnore]
	[NotNetField]
	public NetRoot<FarmerTeam> teamRoot = new NetRoot<FarmerTeam>(new FarmerTeam());

	public int clubCoins;

	public int trashCanLevel;

	private NetLong netMillisecondsPlayed = new NetLong
	{
		DeltaAggregateTicks = (ushort)(60 * (Game1.realMilliSecondsPerGameTenMinutes / 1000))
	};

	[XmlElement("toolBeingUpgraded")]
	public readonly NetRef<Tool> toolBeingUpgraded = new NetRef<Tool>();

	[XmlElement("daysLeftForToolUpgrade")]
	public readonly NetInt daysLeftForToolUpgrade = new NetInt();

	[XmlElement("houseUpgradeLevel")]
	public readonly NetInt houseUpgradeLevel = new NetInt(0);

	[XmlElement("daysUntilHouseUpgrade")]
	public readonly NetInt daysUntilHouseUpgrade = new NetInt(-1);

	public bool showChestColorPicker = true;

	public bool hasWateringCanEnchantment;

	[XmlIgnore]
	public List<BaseEnchantment> enchantments = new List<BaseEnchantment>();

	public readonly int BaseMagneticRadius = 128;

	public int temporaryInvincibilityTimer;

	public int currentTemporaryInvincibilityDuration = 1200;

	[XmlIgnore]
	public float rotation;

	private int craftingTime = 1000;

	private int raftPuddleCounter = 250;

	private int raftBobCounter = 1000;

	public int health = 100;

	public int maxHealth = 100;

	private readonly NetInt netTimesReachedMineBottom = new NetInt(0);

	public float difficultyModifier = 1f;

	[XmlIgnore]
	public Vector2 jitter = Vector2.Zero;

	[XmlIgnore]
	public Vector2 lastPosition;

	[XmlIgnore]
	public Vector2 lastGrabTile = Vector2.Zero;

	[XmlIgnore]
	public float jitterStrength;

	[XmlIgnore]
	public float xOffset;

	/// <summary>The net-synchronized backing field for <see cref="P:StardewValley.Farmer.Gender" />.</summary>
	[XmlElement("gender")]
	public readonly NetEnum<Gender> netGender = new NetEnum<Gender>();

	[XmlIgnore]
	public bool canMove = true;

	[XmlIgnore]
	public bool running;

	[XmlIgnore]
	public bool ignoreCollisions;

	[XmlIgnore]
	public readonly NetBool usingTool = new NetBool(value: false);

	[XmlIgnore]
	public bool isEating;

	[XmlIgnore]
	public readonly NetBool isInBed = new NetBool(value: false);

	[XmlIgnore]
	public bool forceTimePass;

	[XmlIgnore]
	public bool isRafting;

	[XmlIgnore]
	public bool usingSlingshot;

	[XmlIgnore]
	public readonly NetBool bathingClothes = new NetBool(value: false);

	[XmlIgnore]
	public bool canOnlyWalk;

	[XmlIgnore]
	public bool temporarilyInvincible;

	[XmlIgnore]
	public bool flashDuringThisTemporaryInvincibility = true;

	private readonly NetBool netCanReleaseTool = new NetBool(value: false);

	[XmlIgnore]
	public bool isCrafting;

	[XmlIgnore]
	public bool isEmoteAnimating;

	[XmlIgnore]
	public bool passedOut;

	[XmlIgnore]
	protected int _emoteGracePeriod;

	[XmlIgnore]
	private BoundingBoxGroup temporaryPassableTiles = new BoundingBoxGroup();

	[XmlIgnore]
	public readonly NetBool hidden = new NetBool();

	[XmlElement("basicShipped")]
	public readonly NetStringDictionary<int, NetInt> basicShipped = new NetStringDictionary<int, NetInt>();

	[XmlElement("mineralsFound")]
	public readonly NetStringDictionary<int, NetInt> mineralsFound = new NetStringDictionary<int, NetInt>();

	[XmlElement("recipesCooked")]
	public readonly NetStringDictionary<int, NetInt> recipesCooked = new NetStringDictionary<int, NetInt>();

	[XmlElement("fishCaught")]
	public readonly NetStringIntArrayDictionary fishCaught = new NetStringIntArrayDictionary();

	[XmlElement("archaeologyFound")]
	public readonly NetStringIntArrayDictionary archaeologyFound = new NetStringIntArrayDictionary();

	[XmlElement("callsReceived")]
	public readonly NetStringDictionary<int, NetInt> callsReceived = new NetStringDictionary<int, NetInt>();

	public SerializableDictionary<string, SerializableDictionary<string, int>> giftedItems;

	[XmlElement("tailoredItems")]
	public readonly NetStringDictionary<int, NetInt> tailoredItems = new NetStringDictionary<int, NetInt>();

	[XmlElement("friendshipData")]
	public readonly NetStringDictionary<Friendship, NetRef<Friendship>> friendshipData = new NetStringDictionary<Friendship, NetRef<Friendship>>();

	[XmlIgnore]
	public NetString locationBeforeForcedEvent = new NetString(null);

	[XmlIgnore]
	public Vector2 positionBeforeEvent;

	[XmlIgnore]
	public int orientationBeforeEvent;

	[XmlIgnore]
	public int swimTimer;

	[XmlIgnore]
	public int regenTimer;

	[XmlIgnore]
	public int timerSinceLastMovement;

	[XmlIgnore]
	public int noMovementPause;

	[XmlIgnore]
	public int freezePause;

	[XmlIgnore]
	public float yOffset;

	/// <summary>The backing field for <see cref="P:StardewValley.Farmer.spouse" />.</summary>
	protected readonly NetString netSpouse = new NetString();

	public string dateStringForSaveGame;

	public int? dayOfMonthForSaveGame;

	public int? seasonForSaveGame;

	public int? yearForSaveGame;

	[XmlIgnore]
	public Vector2 armOffset;

	private readonly NetRef<Horse> netMount = new NetRef<Horse>();

	[XmlIgnore]
	public ISittable sittingFurniture;

	[XmlIgnore]
	public NetBool isSitting = new NetBool();

	[XmlIgnore]
	public NetVector2 mapChairSitPosition = new NetVector2(new Vector2(-1f, -1f));

	[XmlIgnore]
	public NetBool hasCompletedAllMonsterSlayerQuests = new NetBool(value: false);

	[XmlIgnore]
	public bool isStopSitting;

	[XmlIgnore]
	protected bool _wasSitting;

	[XmlIgnore]
	public Vector2 lerpStartPosition;

	[XmlIgnore]
	public Vector2 lerpEndPosition;

	[XmlIgnore]
	public float lerpPosition = -1f;

	[XmlIgnore]
	public float lerpDuration = -1f;

	[XmlIgnore]
	protected Item _lastSelectedItem;

	[XmlIgnore]
	protected internal Tool _lastEquippedTool;

	[XmlElement("qiGems")]
	public NetIntDelta netQiGems = new NetIntDelta
	{
		Minimum = 0
	};

	[XmlElement("JOTPKProgress")]
	public NetRef<AbigailGame.JOTPKProgress> jotpkProgress = new NetRef<AbigailGame.JOTPKProgress>();

	[XmlIgnore]
	public NetBool hasUsedDailyRevive = new NetBool(value: false);

	[XmlElement("trinketItem")]
	public readonly NetList<Trinket, NetRef<Trinket>> trinketItems = new NetList<Trinket, NetRef<Trinket>>();

	private readonly NetEvent0 fireToolEvent = new NetEvent0(interpolate: true);

	private readonly NetEvent0 beginUsingToolEvent = new NetEvent0(interpolate: true);

	private readonly NetEvent0 endUsingToolEvent = new NetEvent0(interpolate: true);

	private readonly NetEvent0 sickAnimationEvent = new NetEvent0();

	private readonly NetEvent0 passOutEvent = new NetEvent0();

	private readonly NetEvent0 haltAnimationEvent = new NetEvent0();

	private readonly NetEvent1Field<Object, NetRef<Object>> drinkAnimationEvent = new NetEvent1Field<Object, NetRef<Object>>();

	private readonly NetEvent1Field<Object, NetRef<Object>> eatAnimationEvent = new NetEvent1Field<Object, NetRef<Object>>();

	private readonly NetEvent1Field<string, NetString> doEmoteEvent = new NetEvent1Field<string, NetString>();

	private readonly NetEvent1Field<long, NetLong> kissFarmerEvent = new NetEvent1Field<long, NetLong>();

	private readonly NetEvent1Field<float, NetFloat> synchronizedJumpEvent = new NetEvent1Field<float, NetFloat>();

	public readonly NetEvent1Field<string, NetString> renovateEvent = new NetEvent1Field<string, NetString>();

	[XmlElement("chestConsumedLevels")]
	public readonly NetIntDictionary<bool, NetBool> chestConsumedMineLevels = new NetIntDictionary<bool, NetBool>();

	public int saveTime;

	[XmlIgnore]
	public float drawLayerDisambiguator;

	[XmlElement("isCustomized")]
	public readonly NetBool isCustomized = new NetBool(value: false);

	[XmlElement("homeLocation")]
	public readonly NetString homeLocation = new NetString("FarmHouse");

	[XmlElement("lastSleepLocation")]
	public readonly NetString lastSleepLocation = new NetString();

	[XmlElement("lastSleepPoint")]
	public readonly NetPoint lastSleepPoint = new NetPoint();

	[XmlElement("disconnectDay")]
	public readonly NetInt disconnectDay = new NetInt(-1);

	[XmlElement("disconnectLocation")]
	public readonly NetString disconnectLocation = new NetString();

	[XmlElement("disconnectPosition")]
	public readonly NetVector2 disconnectPosition = new NetVector2();

	public static readonly EmoteType[] EMOTES = new EmoteType[22]
	{
		new EmoteType("happy", "Emote_Happy", 32),
		new EmoteType("sad", "Emote_Sad", 28),
		new EmoteType("heart", "Emote_Heart", 20),
		new EmoteType("exclamation", "Emote_Exclamation", 16),
		new EmoteType("note", "Emote_Note", 56),
		new EmoteType("sleep", "Emote_Sleep", 24),
		new EmoteType("game", "Emote_Game", 52),
		new EmoteType("question", "Emote_Question", 8),
		new EmoteType("x", "Emote_X", 36),
		new EmoteType("pause", "Emote_Pause", 40),
		new EmoteType("blush", "Emote_Blush", 60, null, 2, is_hidden: true),
		new EmoteType("angry", "Emote_Angry", 12),
		new EmoteType("yes", "Emote_Yes", 56, new FarmerSprite.AnimationFrame[7]
		{
			new FarmerSprite.AnimationFrame(0, 250, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
			{
				if (who.ShouldHandleAnimationSound())
				{
					who.playNearbySoundLocal("jingle1");
				}
			}),
			new FarmerSprite.AnimationFrame(16, 150, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(0, 250, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(16, 150, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(0, 250, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(16, 150, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(0, 250, secondaryArm: false, flip: false)
		}),
		new EmoteType("no", "Emote_No", 36, new FarmerSprite.AnimationFrame[5]
		{
			new FarmerSprite.AnimationFrame(25, 250, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
			{
				if (who.ShouldHandleAnimationSound())
				{
					who.playNearbySoundLocal("cancel");
				}
			}),
			new FarmerSprite.AnimationFrame(27, 250, secondaryArm: true, flip: false),
			new FarmerSprite.AnimationFrame(25, 250, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(27, 250, secondaryArm: true, flip: false),
			new FarmerSprite.AnimationFrame(25, 250, secondaryArm: false, flip: false)
		}),
		new EmoteType("sick", "Emote_Sick", 12, new FarmerSprite.AnimationFrame[8]
		{
			new FarmerSprite.AnimationFrame(104, 350, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
			{
				if (who.ShouldHandleAnimationSound())
				{
					who.playNearbySoundLocal("croak");
				}
			}),
			new FarmerSprite.AnimationFrame(105, 350, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(104, 350, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(105, 350, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(104, 350, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(105, 350, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(104, 350, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(105, 350, secondaryArm: false, flip: false)
		}),
		new EmoteType("laugh", "Emote_Laugh", 56, new FarmerSprite.AnimationFrame[8]
		{
			new FarmerSprite.AnimationFrame(102, 150, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
			{
				if (who.ShouldHandleAnimationSound())
				{
					who.playNearbySoundLocal("dustMeep");
				}
			}),
			new FarmerSprite.AnimationFrame(103, 150, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(102, 150, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
			{
				if (who.ShouldHandleAnimationSound())
				{
					who.playNearbySoundLocal("dustMeep");
				}
			}),
			new FarmerSprite.AnimationFrame(103, 150, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(102, 150, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
			{
				if (who.ShouldHandleAnimationSound())
				{
					who.playNearbySoundLocal("dustMeep");
				}
			}),
			new FarmerSprite.AnimationFrame(103, 150, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(102, 150, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
			{
				if (who.ShouldHandleAnimationSound())
				{
					who.playNearbySoundLocal("dustMeep");
				}
			}),
			new FarmerSprite.AnimationFrame(103, 150, secondaryArm: false, flip: false)
		}),
		new EmoteType("surprised", "Emote_Surprised", 16, new FarmerSprite.AnimationFrame[1] { new FarmerSprite.AnimationFrame(94, 1500, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
		{
			if (who.ShouldHandleAnimationSound())
			{
				who.playNearbySoundLocal("batScreech");
			}
			who.jumpWithoutSound(4f);
			who.jitterStrength = 1f;
		}) }),
		new EmoteType("hi", "Emote_Hi", 56, new FarmerSprite.AnimationFrame[4]
		{
			new FarmerSprite.AnimationFrame(3, 250, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
			{
				if (who.ShouldHandleAnimationSound())
				{
					who.playNearbySoundLocal("give_gift");
				}
			}),
			new FarmerSprite.AnimationFrame(85, 250, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(3, 250, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(85, 250, secondaryArm: false, flip: false)
		}),
		new EmoteType("taunt", "Emote_Taunt", 12, new FarmerSprite.AnimationFrame[10]
		{
			new FarmerSprite.AnimationFrame(3, 250, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(102, 50, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(10, 250, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
			{
				if (who.ShouldHandleAnimationSound())
				{
					who.playNearbySoundLocal("hitEnemy");
				}
				who.jitterStrength = 1f;
			}).AddFrameEndAction(delegate(Farmer who)
			{
				who.stopJittering();
			}),
			new FarmerSprite.AnimationFrame(3, 250, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(102, 50, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(10, 250, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
			{
				if (who.ShouldHandleAnimationSound())
				{
					who.playNearbySoundLocal("hitEnemy");
				}
				who.jitterStrength = 1f;
			}).AddFrameEndAction(delegate(Farmer who)
			{
				who.stopJittering();
			}),
			new FarmerSprite.AnimationFrame(3, 250, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(102, 50, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(10, 250, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
			{
				if (who.ShouldHandleAnimationSound())
				{
					who.playNearbySoundLocal("hitEnemy");
				}
				who.jitterStrength = 1f;
			}).AddFrameEndAction(delegate(Farmer who)
			{
				who.stopJittering();
			}),
			new FarmerSprite.AnimationFrame(3, 500, secondaryArm: false, flip: false)
		}, 2, is_hidden: true),
		new EmoteType("uh", "Emote_Uh", 40, new FarmerSprite.AnimationFrame[1] { new FarmerSprite.AnimationFrame(10, 1500, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
		{
			if (who.ShouldHandleAnimationSound())
			{
				who.playNearbySoundLocal("clam_tone");
			}
		}) }),
		new EmoteType("music", "Emote_Music", 56, new FarmerSprite.AnimationFrame[9]
		{
			new FarmerSprite.AnimationFrame(98, 150, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
			{
				who.playHarpEmoteSound();
			}),
			new FarmerSprite.AnimationFrame(99, 150, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(100, 150, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(98, 150, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(99, 150, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(100, 150, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(98, 150, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(99, 150, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(100, 150, secondaryArm: false, flip: false)
		}, 2, is_hidden: true),
		new EmoteType("jar", "Emote_Jar", -1, new FarmerSprite.AnimationFrame[6]
		{
			new FarmerSprite.AnimationFrame(111, 150, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(111, 300, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
			{
				if (who.ShouldHandleAnimationSound())
				{
					who.playNearbySoundLocal("fishingRodBend");
				}
				who.jitterStrength = 1f;
			}).AddFrameEndAction(delegate(Farmer who)
			{
				who.stopJittering();
			}),
			new FarmerSprite.AnimationFrame(111, 500, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(111, 300, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
			{
				if (who.ShouldHandleAnimationSound())
				{
					who.playNearbySoundLocal("fishingRodBend");
				}
				who.jitterStrength = 1f;
			}).AddFrameEndAction(delegate(Farmer who)
			{
				who.stopJittering();
			}),
			new FarmerSprite.AnimationFrame(111, 500, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(112, 1000, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
			{
				if (who.ShouldHandleAnimationSound())
				{
					who.playNearbySoundLocal("coin");
				}
				who.jumpWithoutSound(4f);
			})
		}, 1, is_hidden: true)
	};

	[XmlIgnore]
	public int emoteFacingDirection = 2;

	private int toolPitchAccumulator;

	[XmlIgnore]
	public readonly NetInt toolHoldStartTime = new NetInt();

	private int charactercollisionTimer;

	private NPC collisionNPC;

	public float movementMultiplier = 0.01f;

	public bool hasVisibleQuests
	{
		get
		{
			foreach (SpecialOrder specialOrder in team.specialOrders)
			{
				if (!specialOrder.IsHidden())
				{
					return true;
				}
			}
			foreach (Quest quest in questLog)
			{
				if (quest != null && !quest.IsHidden())
				{
					return true;
				}
			}
			return false;
		}
	}

	public Item recoveredItem
	{
		get
		{
			return _recoveredItem.Value;
		}
		set
		{
			_recoveredItem.Value = value;
		}
	}

	/// <summary>Obsolete since 1.6. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Farmer.Gender" /> or <see cref="P:StardewValley.Farmer.IsMale" /> instead.</summary>
	[XmlElement("isMale")]
	public bool? obsolete_isMale
	{
		get
		{
			return null;
		}
		set
		{
			if (value.HasValue)
			{
				Gender = ((!value.Value) ? Gender.Female : Gender.Male);
			}
		}
	}

	/// <summary>Whether the player's preferred pet type is <see cref="F:StardewValley.Characters.Pet.type_cat" />.</summary>
	/// <remarks>See also <see cref="F:StardewValley.Farmer.whichPetType" />.</remarks>
	[XmlIgnore]
	public bool catPerson => whichPetType == "Cat";

	[XmlIgnore]
	public int festivalScore
	{
		get
		{
			return netFestivalScore;
		}
		set
		{
			if (team?.festivalScoreStatus != null)
			{
				team.festivalScoreStatus.UpdateState(festivalScore.ToString() ?? "");
			}
			netFestivalScore.Value = value;
		}
	}

	public int deepestMineLevel
	{
		get
		{
			return netDeepestMineLevel;
		}
		set
		{
			netDeepestMineLevel.Value = value;
		}
	}

	public float stamina
	{
		get
		{
			return netStamina.Value;
		}
		set
		{
			netStamina.Value = value;
		}
	}

	[XmlIgnore]
	public FarmerTeam team
	{
		get
		{
			if (Game1.player != null && this != Game1.player)
			{
				return Game1.player.team;
			}
			return teamRoot.Value;
		}
	}

	public uint totalMoneyEarned
	{
		get
		{
			return (uint)teamRoot.Value.totalMoneyEarned.Value;
		}
		set
		{
			if (teamRoot.Value.totalMoneyEarned.Value != 0)
			{
				if (value >= 15000 && teamRoot.Value.totalMoneyEarned.Value < 15000)
				{
					Game1.multiplayer.globalChatInfoMessage("Earned15k", farmName);
				}
				if (value >= 50000 && teamRoot.Value.totalMoneyEarned.Value < 50000)
				{
					Game1.multiplayer.globalChatInfoMessage("Earned50k", farmName);
				}
				if (value >= 250000 && teamRoot.Value.totalMoneyEarned.Value < 250000)
				{
					Game1.multiplayer.globalChatInfoMessage("Earned250k", farmName);
				}
				if (value >= 1000000 && teamRoot.Value.totalMoneyEarned.Value < 1000000)
				{
					Game1.multiplayer.globalChatInfoMessage("Earned1m", farmName);
				}
				if (value >= 10000000 && teamRoot.Value.totalMoneyEarned.Value < 10000000)
				{
					Game1.multiplayer.globalChatInfoMessage("Earned10m", farmName);
				}
				if (value >= 100000000 && teamRoot.Value.totalMoneyEarned.Value < 100000000)
				{
					Game1.multiplayer.globalChatInfoMessage("Earned100m", farmName);
				}
			}
			teamRoot.Value.totalMoneyEarned.Value = (int)value;
		}
	}

	public ulong millisecondsPlayed
	{
		get
		{
			return (ulong)netMillisecondsPlayed.Value;
		}
		set
		{
			netMillisecondsPlayed.Value = (long)value;
		}
	}

	/// <summary>Whether <strong>any player</strong> has found the Dwarvish Translation Guide that allows speaking to dwarves.</summary>
	[XmlIgnore]
	public bool canUnderstandDwarves
	{
		get
		{
			return Game1.MasterPlayer.mailReceived.Contains("HasDwarvishTranslationGuide");
		}
		set
		{
			Game1.player.team.RequestSetMail(PlayerActionTarget.Host, "HasDwarvishTranslationGuide", MailType.Received, value);
		}
	}

	/// <summary>Whether this player has unlocked access to the casino club.</summary>
	[XmlIgnore]
	public bool hasClubCard
	{
		get
		{
			return mailReceived.Contains("HasClubCard");
		}
		set
		{
			mailReceived.Toggle("HasClubCard", value);
		}
	}

	/// <summary>Whether this player has found the dark talisman, which unblocks the railroad's northeast path.</summary>
	[XmlIgnore]
	public bool hasDarkTalisman
	{
		get
		{
			return mailReceived.Contains("HasDarkTalisman");
		}
		set
		{
			mailReceived.Toggle("HasDarkTalisman", value);
		}
	}

	/// <summary>Whether this player has found the magic ink which allows magical building construction by the Wizard.</summary>
	[XmlIgnore]
	public bool hasMagicInk
	{
		get
		{
			return mailReceived.Contains("HasMagicInk");
		}
		set
		{
			mailReceived.Toggle("HasMagicInk", value);
		}
	}

	/// <summary>Whether this player has found the magnifying glass which allows finding secret notes.</summary>
	[XmlIgnore]
	public bool hasMagnifyingGlass
	{
		get
		{
			return mailReceived.Contains("HasMagnifyingGlass");
		}
		set
		{
			mailReceived.Toggle("HasMagnifyingGlass", value);
		}
	}

	/// <summary>Whether <strong>any player</strong> has found the Rusty Key which unlocks the sewers.</summary>
	[XmlIgnore]
	public bool hasRustyKey
	{
		get
		{
			return Game1.MasterPlayer.mailReceived.Contains("HasRustyKey");
		}
		set
		{
			Game1.player.team.RequestSetMail(PlayerActionTarget.Host, "HasRustyKey", MailType.Received, value);
		}
	}

	/// <summary>Whether <strong>any player</strong> has found the Skull Key which unlocks the skull caverns.</summary>
	[XmlIgnore]
	public bool hasSkullKey
	{
		get
		{
			return Game1.MasterPlayer.mailReceived.Contains("HasSkullKey");
		}
		set
		{
			Game1.player.team.RequestSetMail(PlayerActionTarget.Host, "HasSkullKey", MailType.Received, value);
		}
	}

	/// <summary>Whether this player has the Special Charm which increases daily luck.</summary>
	[XmlIgnore]
	public bool hasSpecialCharm
	{
		get
		{
			return mailReceived.Contains("HasSpecialCharm");
		}
		set
		{
			mailReceived.Toggle("HasSpecialCharm", value);
		}
	}

	/// <summary>Whether this player has unlocked the 'Key to the Town' item which lets them enter all town buildings.</summary>
	[XmlIgnore]
	public bool HasTownKey
	{
		get
		{
			return mailReceived.Contains("HasTownKey");
		}
		set
		{
			mailReceived.Toggle("HasTownKey", value);
		}
	}

	/// <summary>Whether the player has unlocked the door to the skull caverns using <see cref="P:StardewValley.Farmer.hasSkullKey" />.</summary>
	[XmlIgnore]
	public bool hasUnlockedSkullDoor
	{
		get
		{
			return mailReceived.Contains("HasUnlockedSkullDoor");
		}
		set
		{
			mailReceived.Toggle("HasUnlockedSkullDoor", value);
		}
	}

	[XmlIgnore]
	public bool hasPendingCompletedQuests
	{
		get
		{
			foreach (SpecialOrder quest in team.specialOrders)
			{
				if (quest.participants.ContainsKey(UniqueMultiplayerID) && quest.ShouldDisplayAsComplete())
				{
					return true;
				}
			}
			foreach (Quest quest in questLog)
			{
				if (!quest.IsHidden() && quest.ShouldDisplayAsComplete() && !quest.destroy.Value)
				{
					return true;
				}
			}
			return false;
		}
	}

	[XmlElement("useSeparateWallets")]
	public bool useSeparateWallets
	{
		get
		{
			return teamRoot.Value.useSeparateWallets;
		}
		set
		{
			teamRoot.Value.useSeparateWallets.Value = value;
		}
	}

	[XmlElement("theaterBuildDate")]
	public long theaterBuildDate
	{
		get
		{
			return teamRoot.Value.theaterBuildDate.Value;
		}
		set
		{
			teamRoot.Value.theaterBuildDate.Value = value;
		}
	}

	public int timesReachedMineBottom
	{
		get
		{
			return netTimesReachedMineBottom;
		}
		set
		{
			netTimesReachedMineBottom.Value = value;
		}
	}

	[XmlIgnore]
	public bool canReleaseTool
	{
		get
		{
			return netCanReleaseTool.Value;
		}
		set
		{
			netCanReleaseTool.Value = value;
		}
	}

	/// <summary>The player's NPC spouse or roommate.</summary>
	[XmlElement("spouse")]
	public string spouse
	{
		get
		{
			if (!string.IsNullOrEmpty(netSpouse.Value))
			{
				return netSpouse.Value;
			}
			return null;
		}
		set
		{
			if (value == null)
			{
				netSpouse.Value = "";
			}
			else
			{
				netSpouse.Value = value;
			}
		}
	}

	[XmlIgnore]
	public bool isUnclaimedFarmhand
	{
		get
		{
			if (!IsMainPlayer)
			{
				return !isCustomized;
			}
			return false;
		}
	}

	[XmlIgnore]
	public Horse mount
	{
		get
		{
			return netMount.Value;
		}
		set
		{
			setMount(value);
		}
	}

	[XmlIgnore]
	public int MaxItems
	{
		get
		{
			return maxItems;
		}
		set
		{
			maxItems.Value = value;
		}
	}

	[XmlIgnore]
	public int Level => ((int)farmingLevel + (int)fishingLevel + (int)foragingLevel + (int)combatLevel + (int)miningLevel + (int)luckLevel) / 2;

	[XmlIgnore]
	public int FarmingLevel => Math.Max((int)farmingLevel + buffs.FarmingLevel, 0);

	[XmlIgnore]
	public int MiningLevel => Math.Max((int)miningLevel + buffs.MiningLevel, 0);

	[XmlIgnore]
	public int CombatLevel => Math.Max((int)combatLevel + buffs.CombatLevel, 0);

	[XmlIgnore]
	public int ForagingLevel => Math.Max((int)foragingLevel + buffs.ForagingLevel, 0);

	[XmlIgnore]
	public int FishingLevel => Math.Max((int)fishingLevel + buffs.FishingLevel, 0);

	[XmlIgnore]
	public int LuckLevel => Math.Max((int)luckLevel + buffs.LuckLevel, 0);

	[XmlIgnore]
	public double DailyLuck => team.sharedDailyLuck.Value + (double)(hasSpecialCharm ? 0.025f : 0f);

	[XmlIgnore]
	public int HouseUpgradeLevel
	{
		get
		{
			return houseUpgradeLevel;
		}
		set
		{
			houseUpgradeLevel.Value = value;
		}
	}

	[XmlIgnore]
	public BoundingBoxGroup TemporaryPassableTiles
	{
		get
		{
			return temporaryPassableTiles;
		}
		set
		{
			temporaryPassableTiles = value;
		}
	}

	[XmlIgnore]
	public Inventory Items => netItems.Value;

	[XmlIgnore]
	public int MagneticRadius => Math.Max(BaseMagneticRadius + buffs.MagneticRadius, 0);

	[XmlIgnore]
	public Item ActiveItem
	{
		get
		{
			if (TemporaryItem != null)
			{
				return TemporaryItem;
			}
			if (_itemStowed)
			{
				return null;
			}
			if ((int)currentToolIndex < Items.Count && Items[currentToolIndex] != null)
			{
				return Items[currentToolIndex];
			}
			return null;
		}
	}

	[XmlIgnore]
	public Object ActiveObject
	{
		get
		{
			if (TemporaryItem != null)
			{
				return TemporaryItem as Object;
			}
			if (_itemStowed)
			{
				return null;
			}
			if ((int)currentToolIndex < Items.Count && Items[currentToolIndex] is Object obj)
			{
				return obj;
			}
			return null;
		}
		set
		{
			netItemStowed.Set(newValue: false);
			if (value == null)
			{
				removeItemFromInventory(ActiveObject);
			}
			else
			{
				addItemToInventory(value, CurrentToolIndex);
			}
		}
	}

	/// <summary>The player's gender identity.</summary>
	[XmlIgnore]
	public override Gender Gender
	{
		get
		{
			return netGender.Value;
		}
		set
		{
			netGender.Value = value;
		}
	}

	[XmlIgnore]
	public bool IsMale => netGender.Value == Gender.Male;

	[XmlIgnore]
	public ISet<string> DialogueQuestionsAnswered => dialogueQuestionsAnswered;

	[XmlIgnore]
	public bool CanMove
	{
		get
		{
			return canMove;
		}
		set
		{
			canMove = value;
		}
	}

	[XmlIgnore]
	public bool UsingTool
	{
		get
		{
			return usingTool;
		}
		set
		{
			usingTool.Set(value);
		}
	}

	[XmlIgnore]
	public Tool CurrentTool
	{
		get
		{
			return CurrentItem as Tool;
		}
		set
		{
			while (CurrentToolIndex >= Items.Count)
			{
				Items.Add(null);
			}
			Items[CurrentToolIndex] = value;
		}
	}

	[XmlIgnore]
	public Item TemporaryItem
	{
		get
		{
			return temporaryItem.Value;
		}
		set
		{
			temporaryItem.Value = value;
		}
	}

	public Item CursorSlotItem
	{
		get
		{
			return cursorSlotItem.Value;
		}
		set
		{
			cursorSlotItem.Value = value;
		}
	}

	[XmlIgnore]
	public Item CurrentItem
	{
		get
		{
			if (TemporaryItem != null)
			{
				return TemporaryItem;
			}
			if (_itemStowed)
			{
				return null;
			}
			if ((int)currentToolIndex >= Items.Count)
			{
				return null;
			}
			return Items[currentToolIndex];
		}
	}

	[XmlIgnore]
	public int CurrentToolIndex
	{
		get
		{
			return currentToolIndex;
		}
		set
		{
			netItemStowed.Set(newValue: false);
			if ((int)currentToolIndex >= 0 && CurrentItem != null && value != (int)currentToolIndex)
			{
				CurrentItem.actionWhenStopBeingHeld(this);
			}
			currentToolIndex.Set(value);
		}
	}

	[XmlIgnore]
	public float Stamina
	{
		get
		{
			return stamina;
		}
		set
		{
			if (!hasBuff("statue_of_blessings_2") || !(value < stamina))
			{
				stamina = Math.Min(MaxStamina, Math.Max(value, -16f));
			}
		}
	}

	[XmlIgnore]
	public int MaxStamina => Math.Max((int)maxStamina + buffs.MaxStamina, 0);

	[XmlIgnore]
	public int Attack => buffs.Attack;

	[XmlIgnore]
	public int Immunity => buffs.Immunity;

	[XmlIgnore]
	public override float addedSpeed
	{
		get
		{
			return buffs.Speed + ((stats.Get("Book_Speed") != 0 && !isRidingHorse()) ? 0.25f : 0f) + ((stats.Get("Book_Speed2") != 0 && !isRidingHorse()) ? 0.25f : 0f);
		}
		[Obsolete("Player speed can't be changed directly. You can add a speed buff via applyBuff instead (and optionally mark it invisible).")]
		set
		{
		}
	}

	public long UniqueMultiplayerID
	{
		get
		{
			return uniqueMultiplayerID.Value;
		}
		set
		{
			uniqueMultiplayerID.Value = value;
		}
	}

	/// <summary>Whether this is the farmer controlled by the local player, <strong>or</strong> the main farmer in an event being viewed by the local player (even if that farmer instance is a different player).</summary>
	[XmlIgnore]
	public bool IsLocalPlayer
	{
		get
		{
			if (UniqueMultiplayerID != Game1.player.UniqueMultiplayerID)
			{
				if (Game1.CurrentEvent != null)
				{
					return Game1.CurrentEvent.farmer == this;
				}
				return false;
			}
			return true;
		}
	}

	[XmlIgnore]
	public bool IsMainPlayer
	{
		get
		{
			if (!(Game1.serverHost == null) || !IsLocalPlayer)
			{
				if (Game1.serverHost != null)
				{
					return UniqueMultiplayerID == Game1.serverHost.Value.UniqueMultiplayerID;
				}
				return false;
			}
			return true;
		}
	}

	[XmlIgnore]
	public override AnimatedSprite Sprite
	{
		get
		{
			return base.Sprite;
		}
		set
		{
			base.Sprite = value;
		}
	}

	[XmlIgnore]
	public FarmerSprite FarmerSprite
	{
		get
		{
			return (FarmerSprite)Sprite;
		}
		set
		{
			Sprite = value;
		}
	}

	[XmlIgnore]
	public FarmerRenderer FarmerRenderer
	{
		get
		{
			return farmerRenderer.Value;
		}
		set
		{
			farmerRenderer.Set(value);
		}
	}

	[XmlElement("money")]
	public int _money
	{
		get
		{
			return teamRoot.Value.GetMoney(this).Value;
		}
		set
		{
			teamRoot.Value.GetMoney(this).Value = value;
		}
	}

	[XmlIgnore]
	public int QiGems
	{
		get
		{
			return netQiGems.Value;
		}
		set
		{
			netQiGems.Value = value;
		}
	}

	[XmlIgnore]
	public int Money
	{
		get
		{
			return _money;
		}
		set
		{
			if (Game1.player != this)
			{
				throw new Exception("Cannot change another farmer's money. Use Game1.player.team.SetIndividualMoney");
			}
			int previousMoney = _money;
			_money = value;
			if (value > previousMoney)
			{
				uint earned = (uint)(value - previousMoney);
				totalMoneyEarned += earned;
				if (useSeparateWallets)
				{
					stats.IndividualMoneyEarned += earned;
				}
				Game1.stats.checkForMoneyAchievements();
			}
		}
	}

	public override int FacingDirection
	{
		get
		{
			if (!IsLocalPlayer && !isFakeEventActor && UsingTool && CurrentTool is FishingRod { CastDirection: >=0 } rod)
			{
				return rod.CastDirection;
			}
			if (isEmoteAnimating)
			{
				return emoteFacingDirection;
			}
			return facingDirection.Value;
		}
		set
		{
			facingDirection.Set(value);
		}
	}

	public void addUnearnedMoney(int money)
	{
		_money += money;
	}

	public List<string> GetEmoteFavorites()
	{
		if (emoteFavorites.Count == 0)
		{
			emoteFavorites.Add("question");
			emoteFavorites.Add("heart");
			emoteFavorites.Add("yes");
			emoteFavorites.Add("happy");
			emoteFavorites.Add("pause");
			emoteFavorites.Add("sad");
			emoteFavorites.Add("no");
			emoteFavorites.Add("angry");
		}
		return emoteFavorites;
	}

	public Farmer()
	{
		farmerInit();
		Sprite = new FarmerSprite(null);
	}

	public Farmer(FarmerSprite sprite, Vector2 position, int speed, string name, List<Item> initialTools, bool isMale)
		: base(sprite, position, speed, name)
	{
		farmerInit();
		base.Name = name;
		displayName = name;
		Gender = ((!isMale) ? Gender.Female : Gender.Male);
		stamina = (int)maxStamina;
		Items.OverwriteWith(initialTools);
		for (int i = Items.Count; i < (int)maxItems; i++)
		{
			Items.Add(null);
		}
		activeDialogueEvents.Add("Introduction", 6);
		if (base.currentLocation != null)
		{
			mostRecentBed = Utility.PointToVector2((base.currentLocation as FarmHouse).GetPlayerBedSpot()) * 64f;
		}
		else
		{
			mostRecentBed = new Vector2(9f, 9f) * 64f;
		}
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(uniqueMultiplayerID, "uniqueMultiplayerID").AddField(userID, "userID").AddField(platformType, "platformType")
			.AddField(platformID, "platformID")
			.AddField(hasMenuOpen, "hasMenuOpen")
			.AddField(farmerRenderer, "farmerRenderer")
			.AddField(netGender, "netGender")
			.AddField(bathingClothes, "bathingClothes")
			.AddField(shirt, "shirt")
			.AddField(pants, "pants")
			.AddField(hair, "hair")
			.AddField(skin, "skin")
			.AddField(shoes, "shoes")
			.AddField(accessory, "accessory")
			.AddField(facialHair, "facialHair")
			.AddField(hairstyleColor, "hairstyleColor")
			.AddField(pantsColor, "pantsColor")
			.AddField(newEyeColor, "newEyeColor")
			.AddField(netItems, "netItems")
			.AddField(currentToolIndex, "currentToolIndex")
			.AddField(temporaryItem, "temporaryItem")
			.AddField(cursorSlotItem, "cursorSlotItem")
			.AddField(fireToolEvent, "fireToolEvent")
			.AddField(beginUsingToolEvent, "beginUsingToolEvent")
			.AddField(endUsingToolEvent, "endUsingToolEvent")
			.AddField(hat, "hat")
			.AddField(boots, "boots")
			.AddField(leftRing, "leftRing")
			.AddField(rightRing, "rightRing")
			.AddField(hidden, "hidden")
			.AddField(usingTool, "usingTool")
			.AddField(isInBed, "isInBed")
			.AddField(bobberStyle, "bobberStyle")
			.AddField(caveChoice, "caveChoice")
			.AddField(houseUpgradeLevel, "houseUpgradeLevel")
			.AddField(daysUntilHouseUpgrade, "daysUntilHouseUpgrade")
			.AddField(netSpouse, "netSpouse")
			.AddField(mailReceived, "mailReceived")
			.AddField(mailForTomorrow, "mailForTomorrow")
			.AddField(mailbox, "mailbox")
			.AddField(triggerActionsRun, "triggerActionsRun")
			.AddField(eventsSeen, "eventsSeen")
			.AddField(locationsVisited, "locationsVisited")
			.AddField(secretNotesSeen, "secretNotesSeen")
			.AddField(netMount.NetFields, "netMount.NetFields")
			.AddField(dancePartner.NetFields, "dancePartner.NetFields")
			.AddField(divorceTonight, "divorceTonight")
			.AddField(changeWalletTypeTonight, "changeWalletTypeTonight")
			.AddField(isCustomized, "isCustomized")
			.AddField(homeLocation, "homeLocation")
			.AddField(farmName, "farmName")
			.AddField(favoriteThing, "favoriteThing")
			.AddField(horseName, "horseName")
			.AddField(netMillisecondsPlayed, "netMillisecondsPlayed")
			.AddField(netFestivalScore, "netFestivalScore")
			.AddField(friendshipData, "friendshipData")
			.AddField(drinkAnimationEvent, "drinkAnimationEvent")
			.AddField(eatAnimationEvent, "eatAnimationEvent")
			.AddField(sickAnimationEvent, "sickAnimationEvent")
			.AddField(passOutEvent, "passOutEvent")
			.AddField(doEmoteEvent, "doEmoteEvent")
			.AddField(questLog, "questLog")
			.AddField(professions, "professions")
			.AddField(newLevels, "newLevels")
			.AddField(experiencePoints, "experiencePoints")
			.AddField(dialogueQuestionsAnswered, "dialogueQuestionsAnswered")
			.AddField(cookingRecipes, "cookingRecipes")
			.AddField(craftingRecipes, "craftingRecipes")
			.AddField(activeDialogueEvents, "activeDialogueEvents")
			.AddField(previousActiveDialogueEvents, "previousActiveDialogueEvents")
			.AddField(achievements, "achievements")
			.AddField(specialItems, "specialItems")
			.AddField(specialBigCraftables, "specialBigCraftables")
			.AddField(farmingLevel, "farmingLevel")
			.AddField(miningLevel, "miningLevel")
			.AddField(combatLevel, "combatLevel")
			.AddField(foragingLevel, "foragingLevel")
			.AddField(fishingLevel, "fishingLevel")
			.AddField(luckLevel, "luckLevel")
			.AddField(maxStamina, "maxStamina")
			.AddField(netStamina, "netStamina")
			.AddField(maxItems, "maxItems")
			.AddField(chestConsumedMineLevels, "chestConsumedMineLevels")
			.AddField(toolBeingUpgraded, "toolBeingUpgraded")
			.AddField(daysLeftForToolUpgrade, "daysLeftForToolUpgrade")
			.AddField(exhausted, "exhausted")
			.AddField(netDeepestMineLevel, "netDeepestMineLevel")
			.AddField(netTimesReachedMineBottom, "netTimesReachedMineBottom")
			.AddField(netItemStowed, "netItemStowed")
			.AddField(acceptedDailyQuest, "acceptedDailyQuest")
			.AddField(lastSeenMovieWeek, "lastSeenMovieWeek")
			.AddField(shirtItem, "shirtItem")
			.AddField(pantsItem, "pantsItem")
			.AddField(personalShippingBin, "personalShippingBin")
			.AddField(viewingLocation, "viewingLocation")
			.AddField(kissFarmerEvent, "kissFarmerEvent")
			.AddField(haltAnimationEvent, "haltAnimationEvent")
			.AddField(synchronizedJumpEvent, "synchronizedJumpEvent")
			.AddField(tailoredItems, "tailoredItems")
			.AddField(basicShipped, "basicShipped")
			.AddField(mineralsFound, "mineralsFound")
			.AddField(recipesCooked, "recipesCooked")
			.AddField(archaeologyFound, "archaeologyFound")
			.AddField(fishCaught, "fishCaught")
			.AddField(biteChime, "biteChime")
			.AddField(_recoveredItem, "_recoveredItem")
			.AddField(itemsLostLastDeath, "itemsLostLastDeath")
			.AddField(renovateEvent, "renovateEvent")
			.AddField(callsReceived, "callsReceived")
			.AddField(onBridge, "onBridge")
			.AddField(lastSleepLocation, "lastSleepLocation")
			.AddField(lastSleepPoint, "lastSleepPoint")
			.AddField(sleptInTemporaryBed, "sleptInTemporaryBed")
			.AddField(timeWentToBed, "timeWentToBed")
			.AddField(hasUsedDailyRevive, "hasUsedDailyRevive")
			.AddField(jotpkProgress, "jotpkProgress")
			.AddField(requestingTimePause, "requestingTimePause")
			.AddField(isSitting, "isSitting")
			.AddField(mapChairSitPosition, "mapChairSitPosition")
			.AddField(netQiGems, "netQiGems")
			.AddField(locationBeforeForcedEvent, "locationBeforeForcedEvent")
			.AddField(hasCompletedAllMonsterSlayerQuests, "hasCompletedAllMonsterSlayerQuests")
			.AddField(buffs.NetFields, "buffs.NetFields")
			.AddField(trinketItems, "trinketItems")
			.AddField(companions, "companions")
			.AddField(prismaticHair, "prismaticHair")
			.AddField(disconnectDay, "disconnectDay")
			.AddField(disconnectLocation, "disconnectLocation")
			.AddField(disconnectPosition, "disconnectPosition")
			.AddField(tempFoodItemTextureName, "tempFoodItemTextureName")
			.AddField(tempFoodItemSourceRect, "tempFoodItemSourceRect")
			.AddField(toolHoldStartTime, "toolHoldStartTime")
			.AddField(toolHold, "toolHold")
			.AddField(toolPower, "toolPower")
			.AddField(netCanReleaseTool, "netCanReleaseTool")
			.AddField(lastGotPrizeFromGil, "lastGotPrizeFromGil");
		fireToolEvent.onEvent += performFireTool;
		beginUsingToolEvent.onEvent += performBeginUsingTool;
		endUsingToolEvent.onEvent += performEndUsingTool;
		drinkAnimationEvent.onEvent += performDrinkAnimation;
		eatAnimationEvent.onEvent += performEatAnimation;
		sickAnimationEvent.onEvent += performSickAnimation;
		passOutEvent.onEvent += performPassOut;
		doEmoteEvent.onEvent += performPlayerEmote;
		kissFarmerEvent.onEvent += performKissFarmer;
		haltAnimationEvent.onEvent += performHaltAnimation;
		synchronizedJumpEvent.onEvent += performSynchronizedJump;
		renovateEvent.onEvent += performRenovation;
		netMount.fieldChangeEvent += delegate
		{
			ClearCachedPosition();
		};
		shirtItem.fieldChangeVisibleEvent += delegate
		{
			UpdateClothing();
		};
		pantsItem.fieldChangeVisibleEvent += delegate
		{
			UpdateClothing();
		};
		trinketItems.OnArrayReplaced += OnTrinketArrayReplaced;
		trinketItems.OnElementChanged += OnTrinketChange;
	}

	private void farmerInit()
	{
		buffs.SetOwner(this);
		FarmerRenderer = new FarmerRenderer("Characters\\Farmer\\farmer_" + (IsMale ? "" : "girl_") + "base", this);
		base.currentLocation = Game1.getLocationFromName(homeLocation);
		Items.Clear();
		giftedItems = new SerializableDictionary<string, SerializableDictionary<string, int>>();
		LearnDefaultRecipes();
		songsHeard.Add("title_day");
		songsHeard.Add("title_night");
		changeShirt("1000");
		changeSkinColor(0);
		changeShoeColor("2");
		farmName.FilterStringEvent += Utility.FilterDirtyWords;
		name.FilterStringEvent += Utility.FilterDirtyWords;
	}

	public virtual void OnWarp()
	{
		foreach (Companion companion in companions)
		{
			companion.OnOwnerWarp();
		}
		autoGenerateActiveDialogueEvent("firstVisit_" + base.currentLocation.Name);
	}

	public Trinket getFirstTrinketWithID(string id)
	{
		foreach (Trinket trinket in trinketItems)
		{
			if (trinket != null && trinket.ItemId == id)
			{
				return trinket;
			}
		}
		return null;
	}

	public bool hasTrinketWithID(string id)
	{
		foreach (Trinket trinket in trinketItems)
		{
			if (trinket != null && trinket.ItemId == id)
			{
				return true;
			}
		}
		return false;
	}

	public void resetAllTrinketEffects()
	{
		UnapplyAllTrinketEffects();
		ApplyAllTrinketEffects();
	}

	public virtual void ApplyAllTrinketEffects()
	{
		foreach (Trinket trinket in trinketItems)
		{
			if (trinket != null)
			{
				trinket.reloadSprite();
				trinket.Apply(this);
			}
		}
	}

	public virtual void UnapplyAllTrinketEffects()
	{
		foreach (Trinket trinketItem in trinketItems)
		{
			trinketItem?.Unapply(this);
		}
	}

	public virtual void OnTrinketArrayReplaced(NetList<Trinket, NetRef<Trinket>> list, IList<Trinket> before, IList<Trinket> after)
	{
		if ((Game1.gameMode != 0 && Utility.ShouldIgnoreValueChangeCallback()) || (!IsLocalPlayer && !isFakeEventActor && Game1.gameMode != 0))
		{
			return;
		}
		foreach (Trinket item in before)
		{
			item?.Unapply(this);
		}
		foreach (Trinket item2 in after)
		{
			item2?.Apply(this);
		}
	}

	public virtual void OnTrinketChange(NetList<Trinket, NetRef<Trinket>> list, int index, Trinket old_value, Trinket new_value)
	{
		if ((Game1.gameMode == 0 || !Utility.ShouldIgnoreValueChangeCallback()) && (IsLocalPlayer || isFakeEventActor || Game1.gameMode == 0))
		{
			old_value?.Unapply(this);
			new_value?.Apply(this);
		}
	}

	public bool CanEmote()
	{
		if (Game1.farmEvent != null)
		{
			return false;
		}
		if (Game1.eventUp && Game1.CurrentEvent != null && !Game1.CurrentEvent.playerControlSequence && IsLocalPlayer)
		{
			return false;
		}
		if (usingSlingshot)
		{
			return false;
		}
		if (isEating)
		{
			return false;
		}
		if (UsingTool)
		{
			return false;
		}
		if (!CanMove && IsLocalPlayer)
		{
			return false;
		}
		if (IsSitting())
		{
			return false;
		}
		if (isRidingHorse())
		{
			return false;
		}
		if (bathingClothes.Value)
		{
			return false;
		}
		return true;
	}

	/// <summary>Learn the recipes that have no unlock requirements.</summary>
	public void LearnDefaultRecipes()
	{
		foreach (KeyValuePair<string, string> recipe in CraftingRecipe.craftingRecipes)
		{
			if (!craftingRecipes.ContainsKey(recipe.Key) && ArgUtility.Get(recipe.Value.Split('/'), 4) == "default")
			{
				craftingRecipes.Add(recipe.Key, 0);
			}
		}
		foreach (KeyValuePair<string, string> recipe in CraftingRecipe.cookingRecipes)
		{
			if (!cookingRecipes.ContainsKey(recipe.Key) && ArgUtility.Get(recipe.Value.Split('/'), 3) == "default")
			{
				cookingRecipes.Add(recipe.Key, 0);
			}
		}
	}

	public void performRenovation(string location_name)
	{
		if (Game1.RequireLocation(location_name) is FarmHouse farmhouse)
		{
			farmhouse.UpdateForRenovation();
		}
	}

	public void performPlayerEmote(string emote_string)
	{
		for (int i = 0; i < EMOTES.Length; i++)
		{
			EmoteType emote_type = EMOTES[i];
			if (!(emote_type.emoteString == emote_string))
			{
				continue;
			}
			performedEmotes[emote_string] = true;
			if (emote_type.animationFrames != null)
			{
				if (!CanEmote())
				{
					break;
				}
				if (isEmoteAnimating)
				{
					EndEmoteAnimation();
				}
				else if (FarmerSprite.PauseForSingleAnimation)
				{
					break;
				}
				isEmoteAnimating = true;
				_emoteGracePeriod = 200;
				if (this == Game1.player)
				{
					noMovementPause = Math.Max(noMovementPause, 200);
				}
				emoteFacingDirection = emote_type.facingDirection;
				FarmerSprite.animateOnce(emote_type.animationFrames, OnEmoteAnimationEnd);
			}
			if (emote_type.emoteIconIndex >= 0)
			{
				isEmoting = false;
				doEmote(emote_type.emoteIconIndex, nextEventCommand: false);
			}
		}
	}

	public bool ShouldHandleAnimationSound()
	{
		if (!LocalMultiplayer.IsLocalMultiplayer(is_local_only: true))
		{
			return true;
		}
		if (IsLocalPlayer)
		{
			return true;
		}
		return false;
	}

	public static List<Item> initialTools()
	{
		return new List<Item>
		{
			ItemRegistry.Create("(T)Axe"),
			ItemRegistry.Create("(T)Hoe"),
			ItemRegistry.Create("(T)WateringCan"),
			ItemRegistry.Create("(T)Pickaxe"),
			ItemRegistry.Create("(W)47")
		};
	}

	private void playHarpEmoteSound()
	{
		int[] notes = new int[4] { 1200, 1600, 1900, 2400 };
		switch (Game1.random.Next(5))
		{
		case 0:
			notes = new int[4] { 1200, 1600, 1900, 2400 };
			break;
		case 1:
			notes = new int[4] { 1200, 1700, 2100, 2400 };
			break;
		case 2:
			notes = new int[4] { 1100, 1400, 1900, 2300 };
			break;
		case 3:
			notes = new int[3] { 1600, 1900, 2400 };
			break;
		case 4:
			notes = new int[3] { 700, 1200, 1900 };
			break;
		}
		if (!IsLocalPlayer)
		{
			return;
		}
		if (Game1.IsMultiplayer && UniqueMultiplayerID % 111 == 0L)
		{
			notes = new int[4]
			{
				800 + Game1.random.Next(4) * 100,
				1200 + Game1.random.Next(4) * 100,
				1600 + Game1.random.Next(4) * 100,
				2000 + Game1.random.Next(4) * 100
			};
			for (int i = 0; i < notes.Length; i++)
			{
				DelayedAction.playSoundAfterDelay("miniharp_note", Game1.random.Next(60, 150) * i, base.currentLocation, base.Tile, notes[i]);
				if (i > 1 && Game1.random.NextDouble() < 0.25)
				{
					break;
				}
			}
		}
		else
		{
			for (int i = 0; i < notes.Length; i++)
			{
				DelayedAction.playSoundAfterDelay("miniharp_note", (i > 0) ? (150 + Game1.random.Next(35, 51) * i) : 0, base.currentLocation, base.Tile, notes[i]);
			}
		}
	}

	private static void removeLowestUpgradeLevelTool(List<Item> items, Type toolType)
	{
		Tool lowestItem = null;
		foreach (Item item in items)
		{
			if (item is Tool tool && tool.GetType() == toolType && (lowestItem == null || (int)tool.upgradeLevel < (int)lowestItem.upgradeLevel))
			{
				lowestItem = tool;
			}
		}
		if (lowestItem != null)
		{
			items.Remove(lowestItem);
		}
	}

	public static void removeInitialTools(List<Item> items)
	{
		removeLowestUpgradeLevelTool(items, typeof(Axe));
		removeLowestUpgradeLevelTool(items, typeof(Hoe));
		removeLowestUpgradeLevelTool(items, typeof(WateringCan));
		removeLowestUpgradeLevelTool(items, typeof(Pickaxe));
		Item scythe = items.FirstOrDefault((Item item) => item is MeleeWeapon && item.ItemId == "47");
		if (scythe != null)
		{
			items.Remove(scythe);
		}
	}

	public Point getMailboxPosition()
	{
		foreach (Building b in Game1.getFarm().buildings)
		{
			if (b.isCabin && b.HasIndoorsName(homeLocation))
			{
				return b.getMailboxPosition();
			}
		}
		return Game1.getFarm().GetMainMailboxPosition();
	}

	public void ClearBuffs()
	{
		buffs.Clear();
		stopGlowing();
	}

	public bool isActive()
	{
		if (this != Game1.player)
		{
			return Game1.otherFarmers.ContainsKey(UniqueMultiplayerID);
		}
		return true;
	}

	public string getTexture()
	{
		return "Characters\\Farmer\\farmer_" + (IsMale ? "" : "girl_") + "base" + (isBald() ? "_bald" : "");
	}

	public void unload()
	{
		FarmerRenderer?.unload();
	}

	public void setInventory(List<Item> newInventory)
	{
		Items.OverwriteWith(newInventory);
		for (int i = Items.Count; i < (int)maxItems; i++)
		{
			Items.Add(null);
		}
	}

	public void makeThisTheActiveObject(Object o)
	{
		if (freeSpotsInInventory() > 0)
		{
			Item i = CurrentItem;
			ActiveObject = o;
			addItemToInventory(i);
		}
	}

	public int getNumberOfChildren()
	{
		return getChildrenCount();
	}

	private void setMount(Horse mount)
	{
		if (mount != null)
		{
			netMount.Value = mount;
			xOffset = -11f;
			base.Position = Utility.PointToVector2(mount.GetBoundingBox().Location);
			position.Y -= 16f;
			position.X -= 8f;
			base.speed = 2;
			showNotCarrying();
			return;
		}
		netMount.Value = null;
		collisionNPC = null;
		running = false;
		base.speed = ((Game1.isOneOfTheseKeysDown(Game1.GetKeyboardState(), Game1.options.runButton) && !Game1.options.autoRun) ? 5 : 2);
		bool isRunning = base.speed == 5;
		running = isRunning;
		if (running)
		{
			base.speed = 5;
		}
		else
		{
			base.speed = 2;
		}
		completelyStopAnimatingOrDoingAction();
		xOffset = 0f;
	}

	public bool isRidingHorse()
	{
		if (mount != null)
		{
			return !Game1.eventUp;
		}
		return false;
	}

	public List<Child> getChildren()
	{
		return Utility.getHomeOfFarmer(this).getChildren();
	}

	public int getChildrenCount()
	{
		return Utility.getHomeOfFarmer(this).getChildrenCount();
	}

	public Tool getToolFromName(string name)
	{
		foreach (Item item in Items)
		{
			if (item is Tool tool && tool.Name.Contains(name))
			{
				return tool;
			}
		}
		return null;
	}

	public override void SetMovingDown(bool b)
	{
		setMoving((byte)(4 + ((!b) ? 32 : 0)));
	}

	public override void SetMovingRight(bool b)
	{
		setMoving((byte)(2 + ((!b) ? 32 : 0)));
	}

	public override void SetMovingUp(bool b)
	{
		setMoving((byte)(1 + ((!b) ? 32 : 0)));
	}

	public override void SetMovingLeft(bool b)
	{
		setMoving((byte)(8 + ((!b) ? 32 : 0)));
	}

	public int? tryGetFriendshipLevelForNPC(string name)
	{
		if (friendshipData.TryGetValue(name, out var friendship))
		{
			return friendship.Points;
		}
		return null;
	}

	public int getFriendshipLevelForNPC(string name)
	{
		if (friendshipData.TryGetValue(name, out var friendship))
		{
			return friendship.Points;
		}
		return 0;
	}

	public int getFriendshipHeartLevelForNPC(string name)
	{
		return getFriendshipLevelForNPC(name) / 250;
	}

	/// <summary>Get whether the player is roommates with a given NPC (excluding marriage).</summary>
	/// <param name="npc">The NPC's internal name.</param>
	/// <remarks>See also <see cref="M:StardewValley.Farmer.hasRoommate" />.</remarks>
	public bool isRoommate(string name)
	{
		if (name != null && friendshipData.TryGetValue(name, out var friendship))
		{
			return friendship.IsRoommate();
		}
		return false;
	}

	/// <summary>Get whether the player is or will soon be roommates with an NPC (excluding marriage).</summary>
	public bool hasCurrentOrPendingRoommate()
	{
		if (spouse != null && friendshipData.TryGetValue(spouse, out var friendship))
		{
			return friendship.RoommateMarriage;
		}
		return false;
	}

	/// <summary>Get whether the player is roommates with an NPC (excluding marriage).</summary>
	/// <remarks>See also <see cref="M:StardewValley.Farmer.isRoommate(System.String)" />.</remarks>
	public bool hasRoommate()
	{
		return isRoommate(spouse);
	}

	public bool hasAFriendWithFriendshipPoints(int minPoints, bool datablesOnly, int maxPoints = int.MaxValue)
	{
		bool found = false;
		Utility.ForEachVillager(delegate(NPC n)
		{
			if (!datablesOnly || n.datable.Value)
			{
				int friendshipLevelForNPC = getFriendshipLevelForNPC(n.Name);
				if (friendshipLevelForNPC >= minPoints && friendshipLevelForNPC <= maxPoints)
				{
					found = true;
				}
			}
			return !found;
		});
		return found;
	}

	public bool hasAFriendWithHeartLevel(int minHeartLevel, bool datablesOnly, int maxHeartLevel = int.MaxValue)
	{
		int minPoints = minHeartLevel * 250;
		int maxPoints = maxHeartLevel * 250;
		if (maxPoints < maxHeartLevel)
		{
			maxPoints = int.MaxValue;
		}
		return hasAFriendWithFriendshipPoints(minPoints, datablesOnly, maxPoints);
	}

	public void shippedBasic(string itemId, int number)
	{
		if (!basicShipped.TryGetValue(itemId, out var curValue))
		{
			curValue = 0;
		}
		basicShipped[itemId] = curValue + number;
	}

	public void shiftToolbar(bool right)
	{
		if (Items == null || Items.Count < 12 || UsingTool || Game1.dialogueUp || !CanMove || !Items.HasAny() || Game1.eventUp || Game1.farmEvent != null)
		{
			return;
		}
		Game1.playSound("shwip");
		CurrentItem?.actionWhenStopBeingHeld(this);
		if (right)
		{
			IList<Item> toMove = Items.GetRange(0, 12);
			Items.RemoveRange(0, 12);
			Items.AddRange(toMove);
		}
		else
		{
			IList<Item> toMove = Items.GetRange(Items.Count - 12, 12);
			for (int i = 0; i < Items.Count - 12; i++)
			{
				toMove.Add(Items[i]);
			}
			Items.OverwriteWith(toMove);
		}
		netItemStowed.Set(newValue: false);
		CurrentItem?.actionWhenBeingHeld(this);
		for (int i = 0; i < Game1.onScreenMenus.Count; i++)
		{
			if (Game1.onScreenMenus[i] is Toolbar toolbar)
			{
				toolbar.shifted(right);
				break;
			}
		}
	}

	public void foundWalnut(int stack = 1)
	{
		if (Game1.netWorldState.Value.GoldenWalnutsFound < 130)
		{
			Game1.netWorldState.Value.GoldenWalnuts += stack;
			Game1.netWorldState.Value.GoldenWalnutsFound += stack;
			Game1.PerformActionWhenPlayerFree(showNutPickup);
		}
	}

	public virtual void RemoveMail(string mail_key, bool from_broadcast_list = false)
	{
		mail_key = mail_key.Replace("%&NL&%", "");
		mailReceived.Remove(mail_key);
		mailbox.Remove(mail_key);
		mailForTomorrow.Remove(mail_key);
		mailForTomorrow.Remove(mail_key + "%&NL&%");
		if (from_broadcast_list)
		{
			team.broadcastedMail.Remove("%&SM&%" + mail_key);
			team.broadcastedMail.Remove("%&MFT&%" + mail_key);
			team.broadcastedMail.Remove("%&MB&%" + mail_key);
		}
	}

	public virtual void showNutPickup()
	{
		if (!hasOrWillReceiveMail("lostWalnutFound") && !Game1.eventUp)
		{
			Game1.addMailForTomorrow("lostWalnutFound", noLetter: true);
			completelyStopAnimatingOrDoingAction();
			holdUpItemThenMessage(ItemRegistry.Create("(O)73"));
		}
		else if (hasOrWillReceiveMail("lostWalnutFound") && !Game1.eventUp)
		{
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(0, 240, 16, 16), 100f, 4, 2, new Vector2(0f, -96f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
			{
				motion = new Vector2(0f, -6f),
				acceleration = new Vector2(0f, 0.2f),
				stopAcceleratingWhenVelocityIsZero = true,
				attachedCharacter = this,
				positionFollowsAttachedCharacter = true
			});
		}
	}

	/// <summary>Handle the player finding an artifact object.</summary>
	/// <param name="itemId">The unqualified item ID for an <see cref="F:StardewValley.ItemRegistry.type_object" />-type item.</param>
	/// <param name="number">The number found.</param>
	public void foundArtifact(string itemId, int number)
	{
		bool shouldHoldUpArtifact = false;
		if (itemId == "102")
		{
			if (!hasOrWillReceiveMail("lostBookFound"))
			{
				Game1.addMailForTomorrow("lostBookFound", noLetter: true);
				shouldHoldUpArtifact = true;
			}
			else
			{
				Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14100"));
			}
			Game1.playSound("newRecipe");
			Game1.netWorldState.Value.LostBooksFound++;
			Game1.multiplayer.globalChatInfoMessage("LostBook", displayName);
		}
		if (archaeologyFound.TryGetValue(itemId, out var artifactEntry))
		{
			artifactEntry[0] += number;
			artifactEntry[1] += number;
			archaeologyFound[itemId] = artifactEntry;
		}
		else
		{
			if (archaeologyFound.Length == 0)
			{
				if (!eventsSeen.Contains("0") && itemId != "102")
				{
					addQuest("23");
				}
				mailReceived.Add("artifactFound");
				shouldHoldUpArtifact = true;
			}
			archaeologyFound.Add(itemId, new int[2] { number, number });
		}
		if (shouldHoldUpArtifact)
		{
			holdUpItemThenMessage(ItemRegistry.Create("(O)" + itemId));
		}
	}

	public void cookedRecipe(string itemId)
	{
		if (!recipesCooked.TryGetValue(itemId, out var curValue))
		{
			curValue = 0;
		}
		recipesCooked[itemId] = curValue + 1;
	}

	public bool caughtFish(string itemId, int size, bool from_fish_pond = false, int numberCaught = 1)
	{
		ItemMetadata itemData = ItemRegistry.GetMetadata(itemId);
		itemId = itemData.QualifiedItemId;
		bool num = !from_fish_pond && itemData.Exists() && !ItemContextTagManager.HasBaseTag(itemData.QualifiedItemId, "trash_item") && !(itemId == "(O)167") && (itemData.GetParsedData()?.ObjectType == "Fish" || itemData.QualifiedItemId == "(O)372");
		bool sizeRecord = false;
		if (num)
		{
			if (fishCaught.TryGetValue(itemId, out var fishEntry))
			{
				fishEntry[0] += numberCaught;
				Game1.stats.checkForFishingAchievements();
				if (size > fishCaught[itemId][1])
				{
					fishEntry[1] = size;
					sizeRecord = true;
				}
				fishCaught[itemId] = fishEntry;
			}
			else
			{
				fishCaught.Add(itemId, new int[2] { numberCaught, size });
				Game1.stats.checkForFishingAchievements();
				autoGenerateActiveDialogueEvent("fishCaught_" + itemData.LocalItemId);
			}
			checkForQuestComplete(null, -1, numberCaught, null, itemId, 7);
			if (Utility.GetDayOfPassiveFestival("SquidFest") > 0 && itemId == "(O)151")
			{
				Game1.stats.Increment(StatKeys.SquidFestScore(Game1.dayOfMonth, Game1.year), numberCaught);
			}
		}
		return sizeRecord;
	}

	public virtual void gainExperience(int which, int howMuch)
	{
		if (which == 5 || howMuch <= 0)
		{
			return;
		}
		if (!IsLocalPlayer && Game1.IsServer)
		{
			queueMessage(17, Game1.player, which, howMuch);
			return;
		}
		if (Level >= 25)
		{
			int old = MasteryTrackerMenu.getCurrentMasteryLevel();
			Game1.stats.Increment("MasteryExp", Math.Max(1, (which == 0) ? (howMuch / 2) : howMuch));
			if (MasteryTrackerMenu.getCurrentMasteryLevel() > old)
			{
				Game1.showGlobalMessage(Game1.content.LoadString("Strings\\1_6_Strings:Mastery_newlevel"));
				Game1.playSound("newArtifact");
			}
		}
		int newLevel = checkForLevelGain(experiencePoints[which], experiencePoints[which] + howMuch);
		experiencePoints[which] += howMuch;
		int oldLevel = -1;
		if (newLevel != -1)
		{
			switch (which)
			{
			case 0:
				oldLevel = farmingLevel;
				farmingLevel.Value = newLevel;
				break;
			case 3:
				oldLevel = miningLevel;
				miningLevel.Value = newLevel;
				break;
			case 1:
				oldLevel = fishingLevel;
				fishingLevel.Value = newLevel;
				break;
			case 2:
				oldLevel = foragingLevel;
				foragingLevel.Value = newLevel;
				break;
			case 5:
				oldLevel = luckLevel;
				luckLevel.Value = newLevel;
				break;
			case 4:
				oldLevel = combatLevel;
				combatLevel.Value = newLevel;
				break;
			}
		}
		if (newLevel <= oldLevel)
		{
			return;
		}
		for (int i = oldLevel + 1; i <= newLevel; i++)
		{
			newLevels.Add(new Point(which, i));
			if (newLevels.Count == 1)
			{
				Game1.showGlobalMessage(Game1.content.LoadString("Strings\\1_6_Strings:NewIdeas"));
			}
		}
	}

	public int getEffectiveSkillLevel(int whichSkill)
	{
		if (whichSkill < 0 || whichSkill > 5)
		{
			return -1;
		}
		int[] effectiveSkillLevels = new int[6] { farmingLevel, fishingLevel, foragingLevel, miningLevel, combatLevel, luckLevel };
		for (int i = 0; i < newLevels.Count; i++)
		{
			effectiveSkillLevels[newLevels[i].X]--;
		}
		return effectiveSkillLevels[whichSkill];
	}

	public static int checkForLevelGain(int oldXP, int newXP)
	{
		for (int level = 10; level >= 1; level--)
		{
			if (oldXP < getBaseExperienceForLevel(level) && newXP >= getBaseExperienceForLevel(level))
			{
				return level;
			}
		}
		return -1;
	}

	public static int getBaseExperienceForLevel(int level)
	{
		return level switch
		{
			1 => 100, 
			2 => 380, 
			3 => 770, 
			4 => 1300, 
			5 => 2150, 
			6 => 3300, 
			7 => 4800, 
			8 => 6900, 
			9 => 10000, 
			10 => 15000, 
			_ => -1, 
		};
	}

	/// <summary>Mark a gift as having been revealed to the player, even if it hasn't yet been gifted.</summary>
	/// <param name="npcName">The name of the NPC.</param>
	/// <param name="itemId">The item ID.</param>
	public void revealGiftTaste(string npcName, string itemId)
	{
		if (npcName != null)
		{
			if (!giftedItems.TryGetValue(npcName, out var giftData))
			{
				giftData = (giftedItems[npcName] = new SerializableDictionary<string, int>());
			}
			giftData.TryAdd(itemId, 0);
		}
	}

	public void onGiftGiven(NPC npc, Object item)
	{
		if ((bool)item.bigCraftable)
		{
			return;
		}
		if (!giftedItems.TryGetValue(npc.name, out var giftData))
		{
			giftData = (giftedItems[npc.name] = new SerializableDictionary<string, int>());
		}
		if (!giftData.TryGetValue(item.ItemId, out var curValue))
		{
			curValue = 0;
		}
		giftData[item.ItemId] = curValue + 1;
		if (team.specialOrders == null)
		{
			return;
		}
		foreach (SpecialOrder specialOrder in team.specialOrders)
		{
			specialOrder.onGiftGiven?.Invoke(this, npc, item);
		}
	}

	public bool hasGiftTasteBeenRevealed(NPC npc, string itemId)
	{
		if (hasItemBeenGifted(npc, itemId))
		{
			return true;
		}
		if (!giftedItems.TryGetValue(npc.name, out var giftData))
		{
			return false;
		}
		return giftData.ContainsKey(itemId);
	}

	public bool hasItemBeenGifted(NPC npc, string itemId)
	{
		if (!giftedItems.TryGetValue(npc.name, out var giftData))
		{
			return false;
		}
		if (!giftData.TryGetValue(itemId, out var value))
		{
			return false;
		}
		return value > 0;
	}

	public void MarkItemAsTailored(Item item)
	{
		if (item != null)
		{
			string item_key = Utility.getStandardDescriptionFromItem(item, 1);
			if (!tailoredItems.TryGetValue(item_key, out var curValue))
			{
				curValue = 0;
			}
			tailoredItems[item_key] = curValue + 1;
		}
	}

	public bool HasTailoredThisItem(Item item)
	{
		if (item == null)
		{
			return false;
		}
		string item_key = Utility.getStandardDescriptionFromItem(item, 1);
		return tailoredItems.ContainsKey(item_key);
	}

	/// <summary>Handle the player finding a mineral object.</summary>
	/// <param name="itemId">The unqualified item ID for an <see cref="F:StardewValley.ItemRegistry.type_object" />-type item.</param>
	public void foundMineral(string itemId)
	{
		if (!mineralsFound.TryGetValue(itemId, out var curValue))
		{
			curValue = 0;
		}
		mineralsFound[itemId] = curValue + 1;
		if (!hasOrWillReceiveMail("artifactFound"))
		{
			mailReceived.Add("artifactFound");
		}
	}

	public void increaseBackpackSize(int howMuch)
	{
		MaxItems += howMuch;
		while (Items.Count < MaxItems)
		{
			Items.Add(null);
		}
	}

	[Obsolete("Most code should use Items.CountId instead. However this method works a bit differently in that the item ID can be 858 (Qi Gems), 73 (Golden Walnuts), a category number, or -777 to match seasonal wild seeds.")]
	public int getItemCount(string itemId)
	{
		return getItemCountInList(Items, itemId);
	}

	[Obsolete("Most code should use Items.CountId instead. However this method works a bit differently in that the item ID can be a category number, or -777 to match seasonal wild seeds.")]
	public int getItemCountInList(IList<Item> list, string itemId)
	{
		int number_found = 0;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] != null && CraftingRecipe.ItemMatchesForCrafting(list[i], itemId))
			{
				number_found += list[i].Stack;
			}
		}
		return number_found;
	}

	/// <summary>Cause the player to lose a random number of items based on their luck after dying. These will be added to <see cref="F:StardewValley.Farmer.itemsLostLastDeath" /> so they can recover one of them.</summary>
	/// <param name="random">The RNG to use, or <c>null</c> to create one.</param>
	/// <returns>Returns the number of items lost.</returns>
	public int LoseItemsOnDeath(Random random = null)
	{
		if (random == null)
		{
			random = Utility.CreateDaySaveRandom(Game1.timeOfDay);
		}
		double itemLossRate = 0.22 - (double)LuckLevel * 0.04 - DailyLuck;
		int numberOfItemsLost = 0;
		itemsLostLastDeath.Clear();
		for (int i = Items.Count - 1; i >= 0; i--)
		{
			Item item = Items[i];
			if (item != null && item.CanBeLostOnDeath() && random.NextBool(itemLossRate))
			{
				numberOfItemsLost++;
				Items[i] = null;
				itemsLostLastDeath.Add(item);
				if (numberOfItemsLost == 3)
				{
					break;
				}
			}
		}
		return numberOfItemsLost;
	}

	public void ShowSitting()
	{
		if (!IsSitting())
		{
			return;
		}
		if (sittingFurniture != null)
		{
			FacingDirection = sittingFurniture.GetSittingDirection();
		}
		if (yJumpOffset != 0)
		{
			switch (FacingDirection)
			{
			case 0:
				FarmerSprite.setCurrentSingleFrame(12, 32000);
				break;
			case 1:
				FarmerSprite.setCurrentSingleFrame(6, 32000);
				break;
			case 3:
				FarmerSprite.setCurrentSingleFrame(6, 32000, secondaryArm: false, flip: true);
				break;
			case 2:
				FarmerSprite.setCurrentSingleFrame(0, 32000);
				break;
			}
			return;
		}
		switch (FacingDirection)
		{
		case 0:
			FarmerSprite.setCurrentSingleFrame(113, 32000);
			xOffset = 0f;
			yOffset = -40f;
			break;
		case 1:
			FarmerSprite.setCurrentSingleFrame(117, 32000);
			xOffset = -4f;
			yOffset = -32f;
			break;
		case 3:
			FarmerSprite.setCurrentSingleFrame(117, 32000, secondaryArm: false, flip: true);
			xOffset = 4f;
			yOffset = -32f;
			break;
		case 2:
			FarmerSprite.setCurrentSingleFrame(107, 32000, secondaryArm: true);
			xOffset = 0f;
			yOffset = -48f;
			break;
		}
	}

	public void showRiding()
	{
		if (!isRidingHorse())
		{
			return;
		}
		xOffset = -6f;
		switch (FacingDirection)
		{
		case 0:
			FarmerSprite.setCurrentSingleFrame(113, 32000);
			break;
		case 1:
			FarmerSprite.setCurrentSingleFrame(106, 32000);
			xOffset += 2f;
			break;
		case 3:
			FarmerSprite.setCurrentSingleFrame(106, 32000, secondaryArm: false, flip: true);
			xOffset = -12f;
			break;
		case 2:
			FarmerSprite.setCurrentSingleFrame(107, 32000);
			break;
		}
		if (isMoving())
		{
			switch (mount.Sprite.currentAnimationIndex)
			{
			case 0:
				yOffset = 0f;
				break;
			case 1:
				yOffset = -4f;
				break;
			case 2:
				yOffset = -4f;
				break;
			case 3:
				yOffset = 0f;
				break;
			case 4:
				yOffset = 4f;
				break;
			case 5:
				yOffset = 4f;
				break;
			}
		}
		else
		{
			yOffset = 0f;
		}
	}

	public void showCarrying()
	{
		if (Game1.eventUp || isRidingHorse() || Game1.killScreen || IsSitting())
		{
			return;
		}
		if ((bool)bathingClothes || onBridge.Value)
		{
			showNotCarrying();
			return;
		}
		if (!FarmerSprite.PauseForSingleAnimation && !isMoving())
		{
			switch (FacingDirection)
			{
			case 0:
				FarmerSprite.setCurrentFrame(144);
				break;
			case 1:
				FarmerSprite.setCurrentFrame(136);
				break;
			case 2:
				FarmerSprite.setCurrentFrame(128);
				break;
			case 3:
				FarmerSprite.setCurrentFrame(152);
				break;
			}
		}
		if (ActiveObject != null)
		{
			mostRecentlyGrabbedItem = ActiveObject;
		}
		if (IsLocalPlayer && mostRecentlyGrabbedItem?.QualifiedItemId == "(O)434")
		{
			eatHeldObject();
		}
	}

	public void showNotCarrying()
	{
		if (!FarmerSprite.PauseForSingleAnimation && !isMoving())
		{
			bool canOnlyWalk = this.canOnlyWalk || (bool)bathingClothes || onBridge.Value;
			switch (FacingDirection)
			{
			case 0:
				FarmerSprite.setCurrentFrame(canOnlyWalk ? 16 : 48, canOnlyWalk ? 1 : 0);
				break;
			case 1:
				FarmerSprite.setCurrentFrame(canOnlyWalk ? 8 : 40, canOnlyWalk ? 1 : 0);
				break;
			case 2:
				FarmerSprite.setCurrentFrame((!canOnlyWalk) ? 32 : 0, canOnlyWalk ? 1 : 0);
				break;
			case 3:
				FarmerSprite.setCurrentFrame(canOnlyWalk ? 24 : 56, canOnlyWalk ? 1 : 0);
				break;
			}
		}
	}

	public int GetDaysMarried()
	{
		return GetSpouseFriendship()?.DaysMarried ?? 0;
	}

	public Friendship GetSpouseFriendship()
	{
		long? farmerSpouseId = team.GetSpouse(UniqueMultiplayerID);
		if (farmerSpouseId.HasValue)
		{
			long spouseID = farmerSpouseId.Value;
			return team.GetFriendship(UniqueMultiplayerID, spouseID);
		}
		if (string.IsNullOrEmpty(spouse) || !friendshipData.TryGetValue(spouse, out var friendship))
		{
			return null;
		}
		return friendship;
	}

	public bool hasDailyQuest()
	{
		for (int i = questLog.Count - 1; i >= 0; i--)
		{
			if ((bool)questLog[i].dailyQuest)
			{
				return true;
			}
		}
		return false;
	}

	public void showToolUpgradeAvailability()
	{
		int day = Game1.dayOfMonth;
		if (!(toolBeingUpgraded != null) || (int)daysLeftForToolUpgrade > 0 || toolBeingUpgraded.Value == null || Utility.isFestivalDay() || (!(Game1.shortDayNameFromDayOfSeason(day) != "Fri") && hasCompletedCommunityCenter() && !Game1.isRaining) || hasReceivedToolUpgradeMessageYet)
		{
			return;
		}
		if (Game1.newDay)
		{
			Game1.morningQueue.Enqueue(delegate
			{
				Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:ToolReady", toolBeingUpgraded.Value.DisplayName));
			});
		}
		else
		{
			Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:ToolReady", toolBeingUpgraded.Value.DisplayName));
		}
		hasReceivedToolUpgradeMessageYet = true;
	}

	public void dayupdate(int timeWentToSleep)
	{
		if (IsSitting())
		{
			StopSitting(animate: false);
		}
		resetFriendshipsForNewDay();
		LearnDefaultRecipes();
		hasUsedDailyRevive.Value = false;
		hasBeenBlessedByStatueToday = false;
		acceptedDailyQuest.Set(newValue: false);
		dancePartner.Value = null;
		festivalScore = 0;
		forceTimePass = false;
		if ((int)daysLeftForToolUpgrade > 0)
		{
			daysLeftForToolUpgrade.Value--;
		}
		if ((int)daysUntilHouseUpgrade > 0)
		{
			daysUntilHouseUpgrade.Value--;
			if ((int)daysUntilHouseUpgrade <= 0)
			{
				FarmHouse homeOfFarmer = Utility.getHomeOfFarmer(this);
				homeOfFarmer.moveObjectsForHouseUpgrade((int)houseUpgradeLevel + 1);
				houseUpgradeLevel.Value++;
				daysUntilHouseUpgrade.Value = -1;
				homeOfFarmer.setMapForUpgradeLevel(houseUpgradeLevel);
				Game1.stats.checkForBuildingUpgradeAchievements();
				autoGenerateActiveDialogueEvent("houseUpgrade_" + houseUpgradeLevel);
			}
		}
		for (int i = questLog.Count - 1; i >= 0; i--)
		{
			if (questLog[i].IsTimedQuest())
			{
				questLog[i].daysLeft.Value--;
				if ((int)questLog[i].daysLeft <= 0 && !questLog[i].completed)
				{
					questLog.RemoveAt(i);
				}
			}
		}
		ClearBuffs();
		if (MaxStamina >= 508)
		{
			mailReceived.Add("gotMaxStamina");
		}
		float oldStamina = Stamina;
		Stamina = MaxStamina;
		if ((bool)exhausted)
		{
			exhausted.Value = false;
			Stamina = MaxStamina / 2 + 1;
		}
		int bedTime = (((int)timeWentToBed == 0) ? timeWentToSleep : ((int)timeWentToBed));
		if (bedTime > 2400)
		{
			float staminaRestorationReduction = (1f - (float)(2600 - Math.Min(2600, bedTime)) / 200f) * (float)(MaxStamina / 2);
			Stamina -= staminaRestorationReduction;
			if (timeWentToSleep > 2700)
			{
				Stamina /= 2f;
			}
		}
		if (timeWentToSleep < 2700 && oldStamina > Stamina && !exhausted)
		{
			Stamina = oldStamina;
		}
		health = maxHealth;
		string[] array = activeDialogueEvents.Keys.ToArray();
		foreach (string key in array)
		{
			if (!key.Contains("_memory_"))
			{
				previousActiveDialogueEvents.TryAdd(key, 0);
			}
			activeDialogueEvents[key]--;
			if (activeDialogueEvents[key] < 0)
			{
				if (key == "pennyRedecorating" && Utility.getHomeOfFarmer(this).GetSpouseBed() == null)
				{
					activeDialogueEvents[key] = 0;
				}
				else
				{
					activeDialogueEvents.Remove(key);
				}
			}
		}
		foreach (string previousEvent in previousActiveDialogueEvents.Keys)
		{
			previousActiveDialogueEvents[previousEvent]++;
			if (previousActiveDialogueEvents[previousEvent] == 1)
			{
				activeDialogueEvents.Add(previousEvent + "_memory_oneday", 4);
			}
			if (previousActiveDialogueEvents[previousEvent] == 7)
			{
				activeDialogueEvents.Add(previousEvent + "_memory_oneweek", 4);
			}
			if (previousActiveDialogueEvents[previousEvent] == 14)
			{
				activeDialogueEvents.Add(previousEvent + "_memory_twoweeks", 4);
			}
			if (previousActiveDialogueEvents[previousEvent] == 28)
			{
				activeDialogueEvents.Add(previousEvent + "_memory_fourweeks", 4);
			}
			if (previousActiveDialogueEvents[previousEvent] == 56)
			{
				activeDialogueEvents.Add(previousEvent + "_memory_eightweeks", 4);
			}
			if (previousActiveDialogueEvents[previousEvent] == 104)
			{
				activeDialogueEvents.Add(previousEvent + "_memory_oneyear", 4);
			}
		}
		hasMoved = false;
		if (Game1.random.NextDouble() < 0.905 && !hasOrWillReceiveMail("RarecrowSociety") && Utility.doesItemExistAnywhere("(BC)136") && Utility.doesItemExistAnywhere("(BC)137") && Utility.doesItemExistAnywhere("(BC)138") && Utility.doesItemExistAnywhere("(BC)139") && Utility.doesItemExistAnywhere("(BC)140") && Utility.doesItemExistAnywhere("(BC)126") && Utility.doesItemExistAnywhere("(BC)110") && Utility.doesItemExistAnywhere("(BC)113"))
		{
			mailbox.Add("RarecrowSociety");
		}
		timeWentToBed.Value = 0;
		stats.Set("blessingOfWaters", 0);
		if (shirtItem.Value == null || pantsItem.Value == null || (!(base.currentLocation is FarmHouse) && !(base.currentLocation is IslandFarmHouse) && !(base.currentLocation is Shed)))
		{
			return;
		}
		foreach (Object value in base.currentLocation.netObjects.Values)
		{
			if (value is Mannequin mannequin && mannequin.GetMannequinData().Cursed && Game1.random.NextDouble() < 0.005 && !mannequin.swappedWithFarmerTonight.Value)
			{
				mannequin.hat.Value = Equip(mannequin.hat.Value, hat);
				mannequin.shirt.Value = Equip(mannequin.shirt.Value, shirtItem);
				mannequin.pants.Value = Equip(mannequin.pants.Value, pantsItem);
				mannequin.boots.Value = Equip(mannequin.boots.Value, boots);
				mannequin.swappedWithFarmerTonight.Value = true;
				base.currentLocation.playSound("cursed_mannequin");
				mannequin.eyeTimer = 1000;
			}
		}
	}

	public bool hasSeenActiveDialogueEvent(string eventName)
	{
		if (!activeDialogueEvents.ContainsKey(eventName))
		{
			return previousActiveDialogueEvents.ContainsKey(eventName);
		}
		return true;
	}

	public bool autoGenerateActiveDialogueEvent(string eventName, int duration = 4)
	{
		if (!hasSeenActiveDialogueEvent(eventName))
		{
			activeDialogueEvents.Add(eventName, duration);
			return true;
		}
		return false;
	}

	public void removeDatingActiveDialogueEvents(string npcName)
	{
		activeDialogueEvents.Remove("dating_" + npcName);
		removeActiveDialogMemoryEvents("dating_" + npcName);
		previousActiveDialogueEvents.Remove("dating_" + npcName);
	}

	public void removeMarriageActiveDialogueEvents(string npcName)
	{
		activeDialogueEvents.Remove("married_" + npcName);
		removeActiveDialogMemoryEvents("married_" + npcName);
		previousActiveDialogueEvents.Remove("married_" + npcName);
	}

	public void removeActiveDialogMemoryEvents(string activeDialogKey)
	{
		activeDialogueEvents.Remove(activeDialogKey + "_memory_oneday");
		activeDialogueEvents.Remove(activeDialogKey + "_memory_oneweek");
		activeDialogueEvents.Remove(activeDialogKey + "_memory_twoweeks");
		activeDialogueEvents.Remove(activeDialogKey + "_memory_fourweeks");
		activeDialogueEvents.Remove(activeDialogKey + "_memory_eightweeks");
		activeDialogueEvents.Remove(activeDialogKey + "_memory_oneyear");
	}

	public void doDivorce()
	{
		divorceTonight.Value = false;
		if (!isMarriedOrRoommates())
		{
			return;
		}
		if (spouse != null)
		{
			NPC currentSpouse = getSpouse();
			if (currentSpouse != null)
			{
				removeMarriageActiveDialogueEvents(currentSpouse.Name);
				if (!currentSpouse.isRoommate())
				{
					autoGenerateActiveDialogueEvent("divorced_" + currentSpouse.Name);
				}
				spouse = null;
				for (int i = specialItems.Count - 1; i >= 0; i--)
				{
					if (specialItems[i] == "460")
					{
						specialItems.RemoveAt(i);
					}
				}
				if (friendshipData.TryGetValue(currentSpouse.name, out var friendship))
				{
					friendship.Points = 0;
					friendship.RoommateMarriage = false;
					friendship.Status = FriendshipStatus.Divorced;
				}
				Utility.getHomeOfFarmer(this).showSpouseRoom();
				Game1.getFarm().UpdatePatio();
				removeQuest("126");
			}
		}
		else if (team.GetSpouse(UniqueMultiplayerID).HasValue)
		{
			long spouseID = team.GetSpouse(UniqueMultiplayerID).Value;
			Friendship friendship2 = team.GetFriendship(UniqueMultiplayerID, spouseID);
			friendship2.Points = 0;
			friendship2.RoommateMarriage = false;
			friendship2.Status = FriendshipStatus.Divorced;
		}
		if (!autoGenerateActiveDialogueEvent("divorced_once"))
		{
			autoGenerateActiveDialogueEvent("divorced_twice");
		}
	}

	public static void showReceiveNewItemMessage(Farmer who, Item item)
	{
		string possibleSpecialMessage = item.checkForSpecialItemHoldUpMeessage();
		bool fromGiftbox;
		if (possibleSpecialMessage != null)
		{
			Game1.drawObjectDialogue(possibleSpecialMessage);
		}
		else if (item.TryGetTempData<bool>("FromStarterGiftBox", out fromGiftbox) && fromGiftbox && item.QualifiedItemId == "(O)472" && item.Stack == 15)
		{
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1918"));
		}
		else if (item.HasContextTag("book_item"))
		{
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:FoundABook", item.DisplayName));
		}
		else
		{
			Game1.drawObjectDialogue((item.Stack > 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1922", item.Stack, item.DisplayName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1919", item.DisplayName, Lexicon.getProperArticleForWord(item.DisplayName)));
		}
		who.completelyStopAnimatingOrDoingAction();
	}

	public static void showEatingItem(Farmer who)
	{
		TemporaryAnimatedSprite tempSprite = null;
		if (who.itemToEat == null)
		{
			return;
		}
		TemporaryAnimatedSprite coloredTempSprite = null;
		ParsedItemData dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(who.itemToEat.QualifiedItemId);
		string textureName = dataOrErrorItem.TextureName;
		Microsoft.Xna.Framework.Rectangle sourceRect = dataOrErrorItem.GetSourceRect();
		Color color = Color.White;
		Color coloredObjectColor = Color.White;
		if (who.tempFoodItemTextureName.Value != null)
		{
			textureName = who.tempFoodItemTextureName;
			sourceRect = who.tempFoodItemSourceRect.Value;
		}
		else if (who.itemToEat is Object && (who.itemToEat as Object).preservedParentSheetIndex.Value != null)
		{
			if (who.itemToEat.ItemId.Equals("SmokedFish"))
			{
				ParsedItemData dataOrErrorItem2 = ItemRegistry.GetDataOrErrorItem("(O)" + (who.itemToEat as Object).preservedParentSheetIndex.Value);
				textureName = dataOrErrorItem2.TextureName;
				sourceRect = dataOrErrorItem2.GetSourceRect();
				color = new Color(130, 100, 83);
			}
			else if (who.itemToEat is ColoredObject coloredO)
			{
				coloredObjectColor = coloredO.color.Value;
			}
		}
		switch (who.FarmerSprite.currentAnimationIndex)
		{
		case 1:
			if (who.IsLocalPlayer && who.itemToEat.QualifiedItemId == "(O)434")
			{
				tempSprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(368, 16, 16, 16), 62.75f, 8, 2, who.Position + new Vector2(-21f, -112f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f);
				break;
			}
			tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 254f, 1, 0, who.Position + new Vector2(-21f, -112f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, color, 4f, 0f, 0f, 0f);
			if (!coloredObjectColor.Equals(Color.White))
			{
				sourceRect.X += sourceRect.Width;
				coloredTempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 254f, 1, 0, who.Position + new Vector2(-21f, -112f), flicker: false, flipped: false, (float)(who.StandingPixel.Y + 1) / 10000f + 0.01f, 0f, coloredObjectColor, 4f, 0f, 0f, 0f);
			}
			break;
		case 2:
			if (who.IsLocalPlayer && who.itemToEat.QualifiedItemId == "(O)434")
			{
				tempSprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(368, 16, 16, 16), 81.25f, 8, 0, who.Position + new Vector2(-21f, -108f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, Color.White, 4f, -0.01f, 0f, 0f)
				{
					motion = new Vector2(0.8f, -11f),
					acceleration = new Vector2(0f, 0.5f)
				};
				break;
			}
			if (Game1.currentLocation == who.currentLocation)
			{
				Game1.playSound("dwop");
			}
			tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 650f, 1, 0, who.Position + new Vector2(-21f, -108f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, color, 4f, -0.01f, 0f, 0f)
			{
				motion = new Vector2(0.8f, -11f),
				acceleration = new Vector2(0f, 0.5f)
			};
			if (!coloredObjectColor.Equals(Color.White))
			{
				sourceRect.X += sourceRect.Width;
				coloredTempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 650f, 1, 0, who.Position + new Vector2(-21f, -108f), flicker: false, flipped: false, (float)(who.StandingPixel.Y + 1) / 10000f + 0.01f, 0f, coloredObjectColor, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0.8f, -11f),
					acceleration = new Vector2(0f, 0.5f)
				};
			}
			break;
		case 3:
			who.yJumpVelocity = 6f;
			who.yJumpOffset = 1;
			break;
		case 4:
		{
			if (Game1.currentLocation == who.currentLocation && who.ShouldHandleAnimationSound())
			{
				Game1.playSound("eat");
			}
			for (int i = 0; i < 8; i++)
			{
				int size = Game1.random.Next(2, 4);
				Microsoft.Xna.Framework.Rectangle r = sourceRect.Clone();
				r.X += 8;
				r.Y += 8;
				r.Width = size;
				r.Height = size;
				tempSprite = new TemporaryAnimatedSprite(textureName, r, 400f, 1, 0, who.Position + new Vector2(24f, -48f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, color, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2((float)Game1.random.Next(-30, 31) / 10f, Game1.random.Next(-6, -3)),
					acceleration = new Vector2(0f, 0.5f)
				};
				who.currentLocation.temporarySprites.Add(tempSprite);
			}
			return;
		}
		default:
			who.freezePause = 0;
			break;
		}
		if (tempSprite != null)
		{
			who.currentLocation.temporarySprites.Add(tempSprite);
		}
		if (coloredTempSprite != null)
		{
			who.currentLocation.temporarySprites.Add(coloredTempSprite);
		}
	}

	public static void eatItem(Farmer who)
	{
	}

	/// <summary>Get whether the player has a buff applied.</summary>
	/// <param name="id">The buff ID, like <see cref="F:StardewValley.Buff.tipsy" />.</param>
	public bool hasBuff(string id)
	{
		return buffs.IsApplied(id);
	}

	/// <summary>Add a buff to the player, or refresh it if it's already applied.</summary>
	/// <param name="id">The buff ID, like <see cref="F:StardewValley.Buff.tipsy" />.</param>
	public void applyBuff(string id)
	{
		buffs.Apply(new Buff(id, null, null, -1, null, -1, null, false));
	}

	/// <summary>Add a buff to the player, or refresh it if it's already applied.</summary>
	/// <param name="id">The buff to apply.</param>
	public void applyBuff(Buff buff)
	{
		buffs.Apply(buff);
	}

	/// <summary>Get whether the player has a buff with an ID containing the given string.</summary>
	/// <param name="idSubstring">The substring to match in the buff ID.</param>
	public bool hasBuffWithNameContainingString(string idSubstr)
	{
		return buffs.HasBuffWithNameContaining(idSubstr);
	}

	public bool hasOrWillReceiveMail(string id)
	{
		if (!mailReceived.Contains(id) && !mailForTomorrow.Contains(id) && !Game1.mailbox.Contains(id))
		{
			return mailForTomorrow.Contains(id + "%&NL&%");
		}
		return true;
	}

	public static void showHoldingItem(Farmer who, Item item)
	{
		if (item is SpecialItem specialItem)
		{
			TemporaryAnimatedSprite t = specialItem.getTemporarySpriteForHoldingUp(who.Position + new Vector2(0f, -124f));
			t.motion = new Vector2(0f, -0.1f);
			t.scale = 4f;
			t.interval = 2500f;
			t.totalNumberOfLoops = 0;
			t.animationLength = 1;
			Game1.currentLocation.temporarySprites.Add(t);
		}
		else if (item is Slingshot || item is MeleeWeapon || item is Boots)
		{
			TemporaryAnimatedSprite sprite = new TemporaryAnimatedSprite(null, default(Microsoft.Xna.Framework.Rectangle), 2500f, 1, 0, who.Position + new Vector2(0f, -124f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
			{
				motion = new Vector2(0f, -0.1f)
			};
			sprite.CopyAppearanceFromItemId(item.QualifiedItemId);
			Game1.currentLocation.temporarySprites.Add(sprite);
		}
		else if (item is Hat)
		{
			TemporaryAnimatedSprite sprite = new TemporaryAnimatedSprite(null, default(Microsoft.Xna.Framework.Rectangle), 2500f, 1, 0, who.Position + new Vector2(-8f, -124f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
			{
				motion = new Vector2(0f, -0.1f)
			};
			sprite.CopyAppearanceFromItemId(item.QualifiedItemId);
			Game1.currentLocation.temporarySprites.Add(sprite);
		}
		else if (item is Furniture)
		{
			TemporaryAnimatedSprite sprite = new TemporaryAnimatedSprite(null, default(Microsoft.Xna.Framework.Rectangle), 2500f, 1, 0, Vector2.Zero, flicker: false, flipped: false)
			{
				motion = new Vector2(0f, -0.1f),
				layerDepth = 1f
			};
			sprite.CopyAppearanceFromItemId(item.QualifiedItemId);
			sprite.initialPosition = (sprite.position = who.Position + new Vector2(32 - sprite.sourceRect.Width / 2 * 4, -188f));
			Game1.currentLocation.temporarySprites.Add(sprite);
		}
		else if (item is Tool || (item is Object obj && !obj.bigCraftable))
		{
			TemporaryAnimatedSprite sprite = new TemporaryAnimatedSprite(null, default(Microsoft.Xna.Framework.Rectangle), 2500f, 1, 0, who.Position + new Vector2(0f, -124f), flicker: false, flipped: false)
			{
				motion = new Vector2(0f, -0.1f),
				layerDepth = 1f
			};
			sprite.CopyAppearanceFromItemId(item.QualifiedItemId);
			Game1.currentLocation.temporarySprites.Add(sprite);
			if (who.IsLocalPlayer && item.QualifiedItemId == "(O)434")
			{
				who.eatHeldObject();
			}
		}
		else if (item is Object)
		{
			TemporaryAnimatedSprite sprite = new TemporaryAnimatedSprite(null, default(Microsoft.Xna.Framework.Rectangle), 2500f, 1, 0, who.Position + new Vector2(0f, -188f), flicker: false, flipped: false)
			{
				motion = new Vector2(0f, -0.1f),
				layerDepth = 1f
			};
			sprite.CopyAppearanceFromItemId(item.QualifiedItemId);
			Game1.currentLocation.temporarySprites.Add(sprite);
		}
		else if (item is Ring)
		{
			TemporaryAnimatedSprite sprite = new TemporaryAnimatedSprite(null, default(Microsoft.Xna.Framework.Rectangle), 2500f, 1, 0, who.Position + new Vector2(-4f, -124f), flicker: false, flipped: false)
			{
				motion = new Vector2(0f, -0.1f),
				layerDepth = 1f
			};
			sprite.CopyAppearanceFromItemId(item.QualifiedItemId);
			Game1.currentLocation.temporarySprites.Add(sprite);
		}
		else if (item != null)
		{
			TemporaryAnimatedSprite sprite = new TemporaryAnimatedSprite(null, default(Microsoft.Xna.Framework.Rectangle), 2500f, 1, 0, who.Position + new Vector2(0f, -124f), flicker: false, flipped: false)
			{
				motion = new Vector2(0f, -0.1f),
				layerDepth = 1f
			};
			sprite.CopyAppearanceFromItemId(item.QualifiedItemId);
			Game1.currentLocation.temporarySprites.Add(sprite);
		}
		else if (item == null)
		{
			Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(420, 489, 25, 18), 2500f, 1, 0, who.Position + new Vector2(-20f, -152f), flicker: false, flipped: false)
			{
				motion = new Vector2(0f, -0.1f),
				scale = 4f,
				layerDepth = 1f
			});
		}
		else
		{
			Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(10, who.Position + new Vector2(32f, -96f), Color.White)
			{
				motion = new Vector2(0f, -0.1f)
			});
		}
	}

	public void holdUpItemThenMessage(Item item, bool showMessage = true)
	{
		completelyStopAnimatingOrDoingAction();
		if (showMessage)
		{
			Game1.MusicDuckTimer = 2000f;
			DelayedAction.playSoundAfterDelay("getNewSpecialItem", 750);
		}
		faceDirection(2);
		freezePause = 4000;
		FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[3]
		{
			new FarmerSprite.AnimationFrame(57, 0),
			new FarmerSprite.AnimationFrame(57, 2500, secondaryArm: false, flip: false, delegate(Farmer who)
			{
				showHoldingItem(who, item);
			}),
			showMessage ? new FarmerSprite.AnimationFrame((short)FarmerSprite.CurrentFrame, 500, secondaryArm: false, flip: false, delegate(Farmer who)
			{
				showReceiveNewItemMessage(who, item);
			}, behaviorAtEndOfFrame: true) : new FarmerSprite.AnimationFrame((short)FarmerSprite.CurrentFrame, 500, secondaryArm: false, flip: false)
		});
		mostRecentlyGrabbedItem = item;
		canMove = false;
	}

	public void resetState()
	{
		mount = null;
		ClearBuffs();
		TemporaryItem = null;
		swimming.Value = false;
		bathingClothes.Value = false;
		ignoreCollisions = false;
		resetItemStates();
		fireToolEvent.Clear();
		beginUsingToolEvent.Clear();
		endUsingToolEvent.Clear();
		sickAnimationEvent.Clear();
		passOutEvent.Clear();
		drinkAnimationEvent.Clear();
		eatAnimationEvent.Clear();
	}

	public void resetItemStates()
	{
		for (int i = 0; i < Items.Count; i++)
		{
			Items[i]?.resetState();
		}
	}

	public void clearBackpack()
	{
		for (int i = 0; i < Items.Count; i++)
		{
			Items[i] = null;
		}
	}

	public void resetFriendshipsForNewDay()
	{
		foreach (string name in friendshipData.Keys)
		{
			bool single = false;
			NPC n = Game1.getCharacterFromName(name);
			if (n == null)
			{
				n = Game1.getCharacterFromName<Child>(name, mustBeVillager: false);
			}
			if (n != null)
			{
				if (n != null && (bool)n.datable && !friendshipData[name].IsDating() && !n.isMarried())
				{
					single = true;
				}
				if (spouse != null && name == spouse && !hasPlayerTalkedToNPC(name))
				{
					changeFriendship(-20, n);
				}
				else if (n != null && friendshipData[name].IsDating() && !hasPlayerTalkedToNPC(name) && friendshipData[name].Points < 2500)
				{
					changeFriendship(-8, n);
				}
				if (hasPlayerTalkedToNPC(name))
				{
					friendshipData[name].TalkedToToday = false;
				}
				else if ((!single && friendshipData[name].Points < 2500) || (single && friendshipData[name].Points < 2000))
				{
					changeFriendship(-2, n);
				}
			}
		}
		updateFriendshipGifts(Game1.Date);
	}

	public virtual int GetAppliedMagneticRadius()
	{
		return Math.Max(128, MagneticRadius);
	}

	public void updateFriendshipGifts(WorldDate date)
	{
		foreach (string name in friendshipData.Keys)
		{
			if (friendshipData[name].LastGiftDate == null || date.TotalDays != friendshipData[name].LastGiftDate.TotalDays)
			{
				friendshipData[name].GiftsToday = 0;
			}
			if (friendshipData[name].LastGiftDate == null || date.TotalSundayWeeks != friendshipData[name].LastGiftDate.TotalSundayWeeks)
			{
				if (friendshipData[name].GiftsThisWeek >= 2)
				{
					changeFriendship(10, Game1.getCharacterFromName(name));
				}
				friendshipData[name].GiftsThisWeek = 0;
			}
		}
	}

	public bool hasPlayerTalkedToNPC(string name)
	{
		if (!friendshipData.TryGetValue(name, out var friendship) && Game1.NPCGiftTastes.ContainsKey(name))
		{
			friendship = (friendshipData[name] = new Friendship());
		}
		return friendship?.TalkedToToday ?? false;
	}

	public void fuelLantern(int units)
	{
		Tool lantern = getToolFromName("Lantern");
		if (lantern != null)
		{
			((Lantern)lantern).fuelLeft = Math.Min(100, ((Lantern)lantern).fuelLeft + units);
		}
	}

	public bool IsEquippedItem(Item item)
	{
		if (item != null)
		{
			foreach (Item equippedItem in GetEquippedItems())
			{
				if (equippedItem == item)
				{
					return true;
				}
			}
		}
		return false;
	}

	public IEnumerable<Item> GetEquippedItems()
	{
		return new Item[7] { CurrentTool, hat.Value, shirtItem.Value, pantsItem.Value, boots.Value, leftRing.Value, rightRing.Value }.Where((Item item) => item != null);
	}

	public override bool collideWith(Object o)
	{
		base.collideWith(o);
		if (isRidingHorse() && o is Fence)
		{
			mount.squeezeForGate();
			switch (FacingDirection)
			{
			case 3:
				if (o.tileLocation.X > base.Tile.X)
				{
					return false;
				}
				break;
			case 1:
				if (o.tileLocation.X < base.Tile.X)
				{
					return false;
				}
				break;
			}
		}
		return true;
	}

	public void changeIntoSwimsuit()
	{
		bathingClothes.Value = true;
		Halt();
		setRunning(isRunning: false);
		canOnlyWalk = true;
	}

	public void changeOutOfSwimSuit()
	{
		bathingClothes.Value = false;
		canOnlyWalk = false;
		Halt();
		FarmerSprite.StopAnimation();
		if (Game1.options.autoRun)
		{
			setRunning(isRunning: true);
		}
	}

	public void showFrame(int frame, bool flip = false)
	{
		List<FarmerSprite.AnimationFrame> animationFrames = new List<FarmerSprite.AnimationFrame>();
		animationFrames.Add(new FarmerSprite.AnimationFrame(Convert.ToInt32(frame), 100, secondaryArm: false, flip));
		FarmerSprite.setCurrentAnimation(animationFrames.ToArray());
		FarmerSprite.loop = true;
		FarmerSprite.PauseForSingleAnimation = true;
		Sprite.currentFrame = Convert.ToInt32(frame);
	}

	public void stopShowingFrame()
	{
		FarmerSprite.loop = false;
		FarmerSprite.PauseForSingleAnimation = false;
		completelyStopAnimatingOrDoingAction();
	}

	/// <summary>Add an item to the player's inventory if there's room for it.</summary>
	/// <param name="item">The item to add.</param>
	/// <returns>If the item was fully added to the inventory, returns <c>null</c>. Else returns the input item with its stack reduced to the amount that couldn't be added.</returns>
	public Item addItemToInventory(Item item)
	{
		return addItemToInventory(item, null);
	}

	/// <summary>Add an item to the player's inventory if there's room for it.</summary>
	/// <param name="item">The item to add.</param>
	/// <param name="affected_items_list">A list to update with the inventory item stacks it was merged into, or <c>null</c> to ignore it.</param>
	/// <returns>If the item was fully added to the inventory, returns <c>null</c>. Else returns the input item with its stack reduced to the amount that couldn't be added.</returns>
	public Item addItemToInventory(Item item, List<Item> affected_items_list)
	{
		if (item == null)
		{
			return null;
		}
		GetItemReceiveBehavior(item, out var needsInventorySpace, out var _);
		if (!needsInventorySpace)
		{
			OnItemReceived(item, item.Stack, null);
			return null;
		}
		int originalStack = item.Stack;
		int stackLeft = originalStack;
		foreach (Item slot in Items)
		{
			if (!item.canStackWith(slot))
			{
				continue;
			}
			int stack = item.Stack;
			stackLeft = slot.addToStack(item);
			int added = stack - stackLeft;
			if (added > 0)
			{
				item.Stack = stackLeft;
				OnItemReceived(item, added, slot, hideHudNotification: true);
				affected_items_list?.Add(slot);
				if (stackLeft < 1)
				{
					break;
				}
			}
		}
		if (stackLeft > 0)
		{
			for (int i = 0; i < (int)maxItems && i < Items.Count; i++)
			{
				if (Items[i] == null)
				{
					item.onDetachedFromParent();
					Items[i] = item;
					stackLeft = 0;
					OnItemReceived(item, item.Stack, null, hideHudNotification: true);
					affected_items_list?.Add(Items[i]);
					break;
				}
			}
		}
		if (originalStack > stackLeft)
		{
			ShowItemReceivedHudMessageIfNeeded(item, originalStack - stackLeft);
		}
		if (stackLeft <= 0)
		{
			return null;
		}
		return item;
	}

	/// <summary>Add an item to the player's inventory at a specific index position. If there's already an item at that position, the stacks are merged (if possible) else they're swapped.</summary>
	/// <param name="item">The item to add.</param>
	/// <param name="position">The index position within the list at which to add the item.</param>
	/// <returns>If the item was fully added to the inventory, returns <c>null</c>. If it replaced an item stack previously at that position, returns the replaced item stack. Else returns the input item with its stack reduced to the amount that couldn't be added.</returns>
	public Item addItemToInventory(Item item, int position)
	{
		if (item == null)
		{
			return null;
		}
		GetItemReceiveBehavior(item, out var needsInventorySpace, out var _);
		if (!needsInventorySpace)
		{
			OnItemReceived(item, item.Stack, null);
			return null;
		}
		if (position >= 0 && position < Items.Count)
		{
			if (Items[position] == null)
			{
				Items[position] = item;
				OnItemReceived(item, item.Stack, null);
				return null;
			}
			if (!Items[position].canStackWith(item))
			{
				Item result = Items[position];
				Items[position] = item;
				OnItemReceived(item, item.Stack, null);
				return result;
			}
			int stack = item.Stack;
			int stackLeft = Items[position].addToStack(item);
			int added = stack - stackLeft;
			if (added > 0)
			{
				item.Stack = stackLeft;
				OnItemReceived(item, added, Items[position]);
				if (stackLeft <= 0)
				{
					return null;
				}
				return item;
			}
		}
		return item;
	}

	/// <summary>Add an item to the player's inventory if there's room for it.</summary>
	/// <param name="item">The item to add.</param>
	/// <param name="makeActiveObject">Legacy option which may behave in unexpected ways; shouldn't be used by most code.</param>
	/// <returns>Returns whether the item was at least partially added to the inventory. The number of items added will be deducted from the <paramref name="item" />'s <see cref="P:StardewValley.Item.Stack" />.</returns>
	public bool addItemToInventoryBool(Item item, bool makeActiveObject = false)
	{
		if (item == null)
		{
			return false;
		}
		if (IsLocalPlayer)
		{
			Item remainder = null;
			GetItemReceiveBehavior(item, out var needsInventorySpace, out var _);
			if (needsInventorySpace)
			{
				remainder = addItemToInventory(item);
			}
			else
			{
				OnItemReceived(item, item.Stack, null);
			}
			bool success = remainder == null || remainder.Stack != item.Stack || item is SpecialItem;
			if (makeActiveObject && success && !(item is SpecialItem) && remainder != null && item.Stack <= 1)
			{
				int newItemPosition = getIndexOfInventoryItem(item);
				if (newItemPosition > -1)
				{
					Item i = Items[currentToolIndex];
					Items[currentToolIndex] = Items[newItemPosition];
					Items[newItemPosition] = i;
				}
			}
			return success;
		}
		return false;
	}

	/// <summary>Add an item to the player's inventory if there's room for it, then show an animation of the player holding up the item above their head. If the item can't be fully added to the player's inventory, show (or queue) an item-grab menu to let the player collect the remainder.</summary>
	/// <param name="item">The item to add.</param>
	/// <param name="itemSelectedCallback">The callback to invoke when the item is added to the player's inventory.</param>
	/// <param name="forceQueue">For any remainder that can't be added to the inventory directly, whether to add the item-grab menu to <see cref="F:StardewValley.Game1.nextClickableMenu" /> even if there's no active menu currently open.</param>
	public void addItemByMenuIfNecessaryElseHoldUp(Item item, ItemGrabMenu.behaviorOnItemSelect itemSelectedCallback = null, bool forceQueue = false)
	{
		mostRecentlyGrabbedItem = item;
		addItemsByMenuIfNecessary(new List<Item> { item }, itemSelectedCallback, forceQueue);
		if (Game1.activeClickableMenu == null && item?.QualifiedItemId != "(O)434")
		{
			holdUpItemThenMessage(item);
		}
	}

	/// <summary>Add an item to the player's inventory if there's room for it. If the item can't be fully added to the player's inventory, show (or queue) an item-grab menu to let the player collect the remainder.</summary>
	/// <param name="item">The item to add.</param>
	/// <param name="itemSelectedCallback">The callback to invoke when the item is added to the player's inventory.</param>
	/// <param name="forceQueue">For any remainder that can't be added to the inventory directly, whether to add the item-grab menu to <see cref="F:StardewValley.Game1.nextClickableMenu" /> even if there's no active menu currently open.</param>
	public void addItemByMenuIfNecessary(Item item, ItemGrabMenu.behaviorOnItemSelect itemSelectedCallback = null, bool forceQueue = false)
	{
		addItemsByMenuIfNecessary(new List<Item> { item }, itemSelectedCallback, forceQueue);
	}

	/// <summary>Add items to the player's inventory if there's room for them. If the items can't be fully added to the player's inventory, show (or queue) an item-grab menu to let the player collect the remainder.</summary>
	/// <param name="itemsToAdd">The items to add.</param>
	/// <param name="itemSelectedCallback">The callback to invoke when an item is added to the player's inventory.</param>
	/// <param name="forceQueue">For any items that can't be added to the inventory directly, whether to add the item-grab menu to <see cref="F:StardewValley.Game1.nextClickableMenu" /> even if there's no active menu currently open.</param>
	public void addItemsByMenuIfNecessary(List<Item> itemsToAdd, ItemGrabMenu.behaviorOnItemSelect itemSelectedCallback = null, bool forceQueue = false)
	{
		if (itemsToAdd == null || !IsLocalPlayer)
		{
			return;
		}
		if (itemsToAdd.Count > 0 && itemsToAdd[0]?.QualifiedItemId == "(O)434")
		{
			if (Game1.activeClickableMenu == null && !forceQueue)
			{
				eatObject(itemsToAdd[0] as Object, overrideFullness: true);
			}
			else
			{
				Game1.nextClickableMenu.Add(ItemGrabMenu.CreateOverflowMenu(itemsToAdd));
			}
			return;
		}
		for (int i = itemsToAdd.Count - 1; i >= 0; i--)
		{
			if (addItemToInventoryBool(itemsToAdd[i]))
			{
				itemSelectedCallback?.Invoke(itemsToAdd[i], this);
				itemsToAdd.Remove(itemsToAdd[i]);
			}
		}
		if (itemsToAdd.Count > 0 && (forceQueue || Game1.activeClickableMenu != null))
		{
			for (int menuIndex = 0; menuIndex < Game1.nextClickableMenu.Count; menuIndex++)
			{
				if (Game1.nextClickableMenu[menuIndex] is ItemGrabMenu { source: 4 } menu)
				{
					IList<Item> inventory = menu.ItemsToGrabMenu.actualInventory;
					int capacity = menu.ItemsToGrabMenu.capacity;
					bool anyAdded = false;
					for (int i = 0; i < itemsToAdd.Count; i++)
					{
						Item item = itemsToAdd[i];
						int stack = item.Stack;
						item = (itemsToAdd[i] = Utility.addItemToThisInventoryList(item, inventory, capacity));
						if (stack != item?.Stack)
						{
							anyAdded = true;
							if (item == null)
							{
								itemsToAdd.RemoveAt(i);
								i--;
							}
						}
					}
					if (anyAdded)
					{
						Game1.nextClickableMenu[menuIndex] = ItemGrabMenu.CreateOverflowMenu(inventory);
					}
				}
				if (itemsToAdd.Count == 0)
				{
					break;
				}
			}
		}
		if (itemsToAdd.Count > 0)
		{
			ItemGrabMenu itemGrabMenu = ItemGrabMenu.CreateOverflowMenu(itemsToAdd);
			if (forceQueue || Game1.activeClickableMenu != null)
			{
				Game1.nextClickableMenu.Add(itemGrabMenu);
			}
			else
			{
				Game1.activeClickableMenu = itemGrabMenu;
			}
		}
	}

	public virtual void BeginSitting(ISittable furniture)
	{
		if (furniture == null || bathingClothes.Value || swimming.Value || isRidingHorse() || !CanMove || UsingTool || base.IsEmoting)
		{
			return;
		}
		Vector2? sitting_position = furniture.AddSittingFarmer(this);
		if (!sitting_position.HasValue)
		{
			return;
		}
		playNearbySoundAll("woodyStep");
		Halt();
		synchronizedJump(4f);
		FarmerSprite.StopAnimation();
		sittingFurniture = furniture;
		mapChairSitPosition.Value = new Vector2(-1f, -1f);
		if (sittingFurniture is MapSeat)
		{
			Vector2? seat_position = sittingFurniture.GetSittingPosition(this, ignore_offsets: true);
			if (seat_position.HasValue)
			{
				mapChairSitPosition.Value = seat_position.Value;
			}
		}
		isSitting.Value = true;
		LerpPosition(base.Position, new Vector2(sitting_position.Value.X * 64f, sitting_position.Value.Y * 64f), 0.15f);
		freezePause += 100;
	}

	public virtual void LerpPosition(Vector2 start_position, Vector2 end_position, float duration)
	{
		freezePause = (int)(duration * 1000f);
		lerpStartPosition = start_position;
		lerpEndPosition = end_position;
		lerpPosition = 0f;
		lerpDuration = duration;
	}

	public virtual void StopSitting(bool animate = true)
	{
		if (sittingFurniture == null)
		{
			return;
		}
		ISittable furniture = sittingFurniture;
		if (!animate)
		{
			mapChairSitPosition.Value = new Vector2(-1f, -1f);
			furniture.RemoveSittingFarmer(this);
		}
		bool furniture_is_in_this_location = false;
		bool location_found = false;
		Vector2 old_position = base.Position;
		if (furniture.IsSeatHere(base.currentLocation))
		{
			furniture_is_in_this_location = true;
			List<Vector2> exit_positions = new List<Vector2>();
			Vector2 sit_position = new Vector2(furniture.GetSeatBounds().Left, furniture.GetSeatBounds().Top);
			if (furniture.IsSittingHere(this))
			{
				sit_position = furniture.GetSittingPosition(this, ignore_offsets: true).Value;
			}
			if (furniture.GetSittingDirection() == 2)
			{
				exit_positions.Add(sit_position + new Vector2(0f, 1f));
				SortSeatExitPositions(exit_positions, sit_position + new Vector2(1f, 0f), sit_position + new Vector2(-1f, 0f), sit_position + new Vector2(0f, -1f));
			}
			else if (furniture.GetSittingDirection() == 1)
			{
				exit_positions.Add(sit_position + new Vector2(1f, 0f));
				SortSeatExitPositions(exit_positions, sit_position + new Vector2(0f, -1f), sit_position + new Vector2(0f, 1f), sit_position + new Vector2(-1f, 0f));
			}
			else if (furniture.GetSittingDirection() == 3)
			{
				exit_positions.Add(sit_position + new Vector2(-1f, 0f));
				SortSeatExitPositions(exit_positions, sit_position + new Vector2(0f, 1f), sit_position + new Vector2(0f, -1f), sit_position + new Vector2(1f, 0f));
			}
			else if (furniture.GetSittingDirection() == 0)
			{
				exit_positions.Add(sit_position + new Vector2(0f, -1f));
				SortSeatExitPositions(exit_positions, sit_position + new Vector2(-1f, 0f), sit_position + new Vector2(1f, 0f), sit_position + new Vector2(0f, 1f));
			}
			Microsoft.Xna.Framework.Rectangle bounds = furniture.GetSeatBounds();
			bounds.Inflate(1, 1);
			foreach (Vector2 v in Utility.getBorderOfThisRectangle(bounds))
			{
				exit_positions.Add(v);
			}
			foreach (Vector2 exit_position in exit_positions)
			{
				setTileLocation(exit_position);
				Microsoft.Xna.Framework.Rectangle boundingBox = GetBoundingBox();
				base.Position = old_position;
				Object tile_object = base.currentLocation.getObjectAtTile((int)exit_position.X, (int)exit_position.Y, ignorePassables: true);
				if (!base.currentLocation.isCollidingPosition(boundingBox, Game1.viewport, isFarmer: true, 0, glider: false, this) && (tile_object == null || tile_object.isPassable()))
				{
					if (animate)
					{
						playNearbySoundAll("coin");
						synchronizedJump(4f);
						LerpPosition(sit_position * 64f, exit_position * 64f, 0.15f);
					}
					location_found = true;
					break;
				}
			}
		}
		if (!location_found)
		{
			if (animate)
			{
				playNearbySoundAll("coin");
			}
			base.Position = old_position;
			if (furniture_is_in_this_location)
			{
				Microsoft.Xna.Framework.Rectangle bounds = furniture.GetSeatBounds();
				bounds.X *= 64;
				bounds.Y *= 64;
				bounds.Width *= 64;
				bounds.Height *= 64;
				temporaryPassableTiles.Add(bounds);
			}
		}
		if (!animate)
		{
			sittingFurniture = null;
			isSitting.Value = false;
			Halt();
			showNotCarrying();
		}
		else
		{
			isStopSitting = true;
		}
		Game1.haltAfterCheck = false;
		yOffset = 0f;
		xOffset = 0f;
	}

	public void SortSeatExitPositions(List<Vector2> list, Vector2 a, Vector2 b, Vector2 c)
	{
		Vector2 mouse_pos = Utility.PointToVector2(Game1.getMousePosition(ui_scale: false)) + new Vector2(Game1.viewport.X, Game1.viewport.Y);
		Vector2 move_direction = Vector2.Zero;
		if (Game1.isOneOfTheseKeysDown(Game1.input.GetKeyboardState(), Game1.options.moveUpButton) || (Game1.options.gamepadControls && ((double)Game1.input.GetGamePadState().ThumbSticks.Left.Y > 0.25 || Game1.input.GetGamePadState().IsButtonDown(Buttons.DPadUp))))
		{
			move_direction.Y -= 1f;
		}
		else if (Game1.isOneOfTheseKeysDown(Game1.input.GetKeyboardState(), Game1.options.moveDownButton) || (Game1.options.gamepadControls && ((double)Game1.input.GetGamePadState().ThumbSticks.Left.Y < -0.25 || Game1.input.GetGamePadState().IsButtonDown(Buttons.DPadDown))))
		{
			move_direction.Y += 1f;
		}
		if (Game1.isOneOfTheseKeysDown(Game1.input.GetKeyboardState(), Game1.options.moveLeftButton) || (Game1.options.gamepadControls && ((double)Game1.input.GetGamePadState().ThumbSticks.Left.X < -0.25 || Game1.input.GetGamePadState().IsButtonDown(Buttons.DPadLeft))))
		{
			move_direction.X -= 1f;
		}
		else if (Game1.isOneOfTheseKeysDown(Game1.input.GetKeyboardState(), Game1.options.moveRightButton) || (Game1.options.gamepadControls && ((double)Game1.input.GetGamePadState().ThumbSticks.Left.X > 0.25 || Game1.input.GetGamePadState().IsButtonDown(Buttons.DPadRight))))
		{
			move_direction.X += 1f;
		}
		if (move_direction != Vector2.Zero)
		{
			mouse_pos = getStandingPosition() + move_direction * 64f;
		}
		mouse_pos /= 64f;
		List<Vector2> exit_positions = new List<Vector2>();
		exit_positions.Add(a);
		exit_positions.Add(b);
		exit_positions.Add(c);
		exit_positions.Sort((Vector2 d, Vector2 e) => (d + new Vector2(0.5f, 0.5f) - mouse_pos).Length().CompareTo((e + new Vector2(0.5f, 0.5f) - mouse_pos).Length()));
		list.AddRange(exit_positions);
	}

	public virtual bool IsSitting()
	{
		return isSitting.Value;
	}

	public bool isInventoryFull()
	{
		for (int i = 0; i < (int)maxItems; i++)
		{
			if (Items.Count > i && Items[i] == null)
			{
				return false;
			}
		}
		return true;
	}

	public bool couldInventoryAcceptThisItem(Item item)
	{
		if (item == null)
		{
			return false;
		}
		if (item.IsRecipe)
		{
			return true;
		}
		switch (item.QualifiedItemId)
		{
		case "(O)73":
		case "(O)930":
		case "(O)102":
		case "(O)858":
		case "(O)GoldCoin":
			return true;
		default:
		{
			for (int i = 0; i < (int)maxItems; i++)
			{
				if (Items.Count > i && (Items[i] == null || (item is Object && Items[i] is Object && Items[i].Stack + item.Stack <= Items[i].maximumStackSize() && (Items[i] as Object).canStackWith(item))))
				{
					return true;
				}
			}
			if (IsLocalPlayer && isInventoryFull() && Game1.hudMessages.Count == 0)
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
			}
			return false;
		}
		}
	}

	public bool couldInventoryAcceptThisItem(string id, int stack, int quality = 0)
	{
		ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(id);
		switch (itemData.QualifiedItemId)
		{
		case "(O)73":
		case "(O)930":
		case "(O)102":
		case "(O)858":
		case "(O)GoldCoin":
			return true;
		default:
		{
			for (int i = 0; i < (int)maxItems; i++)
			{
				if (Items.Count > i && (Items[i] == null || (Items[i].Stack + stack <= Items[i].maximumStackSize() && Items[i].QualifiedItemId == itemData.QualifiedItemId && (int)Items[i].quality == quality)))
				{
					return true;
				}
			}
			if (IsLocalPlayer && isInventoryFull() && Game1.hudMessages.Count == 0)
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
			}
			return false;
		}
		}
	}

	public NPC getSpouse()
	{
		if (isMarriedOrRoommates() && spouse != null)
		{
			return Game1.getCharacterFromName(spouse);
		}
		return null;
	}

	public int freeSpotsInInventory()
	{
		int slotsUsed = Items.CountItemStacks();
		if (slotsUsed >= (int)maxItems)
		{
			return 0;
		}
		return (int)maxItems - slotsUsed;
	}

	/// <summary>Get the behavior that applies when this item is received.</summary>
	/// <param name="item">The item being received.</param>
	/// <param name="needsInventorySpace">Whether this item takes space in the player inventory. This is false for special items like Qi Gems.</param>
	/// <param name="showNotification">Whether to show a HUD notification when the item is received.</param>
	public void GetItemReceiveBehavior(Item item, out bool needsInventorySpace, out bool showNotification)
	{
		if (item is SpecialItem)
		{
			needsInventorySpace = false;
			showNotification = false;
			return;
		}
		switch (item.QualifiedItemId)
		{
		case "(O)73":
		case "(O)102":
		case "(O)858":
			needsInventorySpace = false;
			showNotification = true;
			break;
		case "(O)GoldCoin":
		case "(O)930":
			needsInventorySpace = false;
			showNotification = false;
			break;
		default:
			needsInventorySpace = true;
			showNotification = true;
			break;
		}
	}

	/// <summary>Handle an item being added to the current player's inventory.</summary>
	/// <param name="item">The item that was added. If <see cref="!:mergedIntoStack" /> is set, this is the original item rather than the one actually in the player's inventory.</param>
	/// <param name="countAdded">The number of the item that was added. This may differ from <paramref name="item" />'s stack size if it was only partly added or split across multiple stacks.</param>
	/// <param name="mergedIntoStack">The previous item stack it was merged into, if applicable.</param>
	/// <param name="hideHudNotification">Hide the 'item received' HUD notification even if it would normally be shown. This is used when merging the item into multiple stacks, so the HUD notification is shown once.</param>
	public void OnItemReceived(Item item, int countAdded, Item mergedIntoStack, bool hideHudNotification = false)
	{
		if (!IsLocalPlayer)
		{
			return;
		}
		(item as Object)?.reloadSprite();
		if (item.HasBeenInInventory)
		{
			return;
		}
		Item actualItem = mergedIntoStack ?? item;
		if (!hideHudNotification)
		{
			GetItemReceiveBehavior(actualItem, out var _, out var showHudNotification);
			if (showHudNotification)
			{
				ShowItemReceivedHudMessage(actualItem, countAdded);
			}
		}
		if (freezePause <= 0)
		{
			mostRecentlyGrabbedItem = actualItem;
		}
		if (item.SetFlagOnPickup != null)
		{
			if (!hasOrWillReceiveMail(item.SetFlagOnPickup))
			{
				Game1.addMail(item.SetFlagOnPickup, noLetter: true);
			}
			actualItem.SetFlagOnPickup = null;
		}
		(actualItem as SpecialItem)?.actionWhenReceived(this);
		if (actualItem is Object { specialItem: not false } obj)
		{
			string key = (obj.IsRecipe ? ("-" + obj.ItemId) : obj.ItemId);
			if ((bool)obj.bigCraftable || obj is Furniture)
			{
				if (!specialBigCraftables.Contains(key))
				{
					specialBigCraftables.Add(key);
				}
			}
			else if (!specialItems.Contains(key))
			{
				specialItems.Add(key);
			}
		}
		int originalStack = actualItem.Stack;
		try
		{
			actualItem.Stack = countAdded;
			checkForQuestComplete(null, -1, countAdded, actualItem, null, 9);
			checkForQuestComplete(null, -1, countAdded, actualItem, null, 10);
			if (team.specialOrders != null)
			{
				foreach (SpecialOrder specialOrder in team.specialOrders)
				{
					specialOrder.onItemCollected?.Invoke(this, actualItem);
				}
			}
		}
		finally
		{
			actualItem.Stack = originalStack;
		}
		if (actualItem.HasTypeObject() && actualItem is Object basicObj)
		{
			if (basicObj.Category == -2 || basicObj.Type == "Minerals")
			{
				foundMineral(basicObj.ItemId);
			}
			else if (basicObj.Type == "Arch")
			{
				foundArtifact(basicObj.ItemId, 1);
			}
		}
		switch (actualItem.QualifiedItemId)
		{
		case "(O)GoldCoin":
		{
			Game1.playSound("moneyDial");
			int coinAmount = 250;
			if (Game1.IsSpring && Game1.dayOfMonth == 17 && base.currentLocation is Forest && base.Tile.Y > 90f)
			{
				coinAmount = 25;
			}
			Money += coinAmount;
			removeItemFromInventory(item);
			Game1.dayTimeMoneyBox.gotGoldCoin(coinAmount);
			break;
		}
		case "(O)73":
			foundWalnut(countAdded);
			removeItemFromInventory(actualItem);
			break;
		case "(O)858":
			QiGems += countAdded;
			Game1.playSound("qi_shop_purchase");
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 858, 16, 16), 100f, 1, 8, new Vector2(0f, -96f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
			{
				motion = new Vector2(0f, -6f),
				acceleration = new Vector2(0f, 0.2f),
				stopAcceleratingWhenVelocityIsZero = true,
				attachedCharacter = this,
				positionFollowsAttachedCharacter = true
			});
			removeItemFromInventory(actualItem);
			break;
		case "(O)930":
		{
			int amount = 10;
			health = Math.Min(maxHealth, health + amount);
			base.currentLocation.debris.Add(new Debris(amount, getStandingPosition(), Color.Lime, 1f, this));
			Game1.playSound("healSound");
			removeItemFromInventory(actualItem);
			break;
		}
		case "(O)875":
			if (!Game1.MasterPlayer.hasOrWillReceiveMail("ectoplasmDrop") && team.SpecialOrderActive("Wizard"))
			{
				Game1.addMailForTomorrow("ectoplasmDrop", noLetter: true, sendToEveryone: true);
			}
			break;
		case "(O)876":
			if (!Game1.MasterPlayer.hasOrWillReceiveMail("prismaticJellyDrop") && team.SpecialOrderActive("Wizard2"))
			{
				Game1.addMailForTomorrow("prismaticJellyDrop", noLetter: true, sendToEveryone: true);
			}
			break;
		case "(O)897":
			if (!Game1.MasterPlayer.hasOrWillReceiveMail("gotMissingStocklist"))
			{
				Game1.addMailForTomorrow("gotMissingStocklist", noLetter: true, sendToEveryone: true);
			}
			break;
		case "(BC)256":
			if (!Game1.MasterPlayer.hasOrWillReceiveMail("gotFirstJunimoChest"))
			{
				Game1.addMailForTomorrow("gotFirstJunimoChest", noLetter: true, sendToEveryone: true);
			}
			break;
		case "(O)535":
			Game1.PerformActionWhenPlayerFree(delegate
			{
				if (!hasOrWillReceiveMail("geodeFound"))
				{
					mailReceived.Add("geodeFound");
					holdUpItemThenMessage(actualItem);
				}
			});
			break;
		case "(O)428":
			if (!hasOrWillReceiveMail("clothFound"))
			{
				Game1.addMailForTomorrow("clothFound", noLetter: true);
			}
			break;
		case "(O)102":
			Game1.PerformActionWhenPlayerFree(delegate
			{
				foundArtifact(actualItem.ItemId, 1);
			});
			removeItemFromInventory(actualItem);
			stats.NotesFound++;
			break;
		case "(O)390":
			stats.StoneGathered++;
			if (stats.StoneGathered >= 100 && !hasOrWillReceiveMail("robinWell"))
			{
				Game1.addMailForTomorrow("robinWell");
			}
			break;
		case "(O)384":
			stats.GoldFound += (uint)countAdded;
			break;
		case "(O)380":
			stats.IronFound += (uint)countAdded;
			break;
		case "(O)386":
			stats.IridiumFound += (uint)countAdded;
			break;
		case "(O)378":
			stats.CopperFound += (uint)countAdded;
			if (!hasOrWillReceiveMail("copperFound"))
			{
				Game1.addMailForTomorrow("copperFound", noLetter: true);
			}
			break;
		case "(O)74":
			stats.PrismaticShardsFound++;
			break;
		case "(O)72":
			stats.DiamondsFound++;
			break;
		case "(BC)248":
			Game1.netWorldState.Value.MiniShippingBinsObtained++;
			break;
		case "(W)62":
		case "(W)63":
		case "(W)64":
			Game1.getAchievement(42);
			break;
		}
		actualItem.HasBeenInInventory = true;
	}

	/// <summary>Show the item-received HUD message for an item if applicable for the item type.</summary>
	/// <param name="item">The item that was added.</param>
	/// <param name="countAdded">The number of the item that was added. This may differ from <paramref name="item" />'s stack size if it was only partly added or split across multiple stacks.</param>
	public void ShowItemReceivedHudMessageIfNeeded(Item item, int countAdded)
	{
		GetItemReceiveBehavior(item, out var _, out var showHudNotification);
		if (showHudNotification)
		{
			ShowItemReceivedHudMessage(item, countAdded);
		}
	}

	/// <summary>Show the item-received HUD message for an item.</summary>
	/// <param name="item">The item that was added.</param>
	/// <param name="countAdded">The number of the item that was added. This may differ from <paramref name="item" />'s stack size if it was only partly added or split across multiple stacks.</param>
	public void ShowItemReceivedHudMessage(Item item, int countAdded)
	{
		if (Game1.activeClickableMenu == null || !(Game1.activeClickableMenu is ItemGrabMenu))
		{
			Game1.addHUDMessage(HUDMessage.ForItemGained(item, countAdded));
		}
	}

	public int getIndexOfInventoryItem(Item item)
	{
		for (int i = 0; i < Items.Count; i++)
		{
			if (Items[i] == item || (Items[i] != null && item != null && item.canStackWith(Items[i])))
			{
				return i;
			}
		}
		return -1;
	}

	public void reduceActiveItemByOne()
	{
		if (CurrentItem != null && --CurrentItem.Stack <= 0)
		{
			removeItemFromInventory(CurrentItem);
			showNotCarrying();
		}
	}

	public void ReequipEnchantments()
	{
		Tool tool = CurrentTool;
		if (tool == null)
		{
			return;
		}
		foreach (BaseEnchantment enchantment in tool.enchantments)
		{
			enchantment.OnEquip(this);
		}
	}

	public void removeItemFromInventory(Item which)
	{
		int i = Items.IndexOf(which);
		if (i >= 0 && i < Items.Count)
		{
			Items[i].actionWhenStopBeingHeld(this);
			Items[i] = null;
		}
	}

	/// <summary>Get whether the player is married to or roommates with an NPC or player.</summary>
	public bool isMarriedOrRoommates()
	{
		if (team.IsMarried(UniqueMultiplayerID))
		{
			return true;
		}
		if (spouse != null && friendshipData.TryGetValue(spouse, out var friendship))
		{
			return friendship.IsMarried();
		}
		return false;
	}

	public bool isEngaged()
	{
		if (team.IsEngaged(UniqueMultiplayerID))
		{
			return true;
		}
		if (spouse != null && friendshipData.TryGetValue(spouse, out var friendship))
		{
			return friendship.IsEngaged();
		}
		return false;
	}

	public void removeFirstOfThisItemFromInventory(string itemId, int count = 1)
	{
		itemId = ItemRegistry.QualifyItemId(itemId);
		if (itemId == null)
		{
			return;
		}
		int remaining = count;
		if (ActiveObject?.QualifiedItemId == itemId)
		{
			remaining -= ActiveObject.Stack;
			ActiveObject.Stack -= count;
			if (ActiveObject.Stack <= 0)
			{
				ActiveObject = null;
				showNotCarrying();
			}
		}
		if (remaining > 0)
		{
			Items.ReduceId(itemId, remaining);
		}
	}

	public void rotateShirt(int direction, List<string> validIds = null)
	{
		string itemId = shirt.Value;
		if (validIds == null)
		{
			validIds = new List<string>();
			foreach (KeyValuePair<string, ShirtData> shirtDatum in Game1.shirtData)
			{
				validIds.Add(shirtDatum.Key);
			}
		}
		int index = validIds.IndexOf(itemId);
		if (index == -1)
		{
			itemId = validIds.FirstOrDefault();
			if (itemId != null)
			{
				changeShirt(itemId);
			}
		}
		else
		{
			index = Utility.WrapIndex(index + direction, validIds.Count);
			itemId = validIds[index];
			changeShirt(itemId);
		}
	}

	public void changeShirt(string itemId)
	{
		shirt.Set(itemId);
		FarmerRenderer.changeShirt(itemId);
	}

	public void rotatePantStyle(int direction, List<string> validIds = null)
	{
		string itemId = pants.Value;
		if (validIds == null)
		{
			validIds = new List<string>();
			foreach (KeyValuePair<string, PantsData> pantsDatum in Game1.pantsData)
			{
				validIds.Add(pantsDatum.Key);
			}
		}
		int index = validIds.IndexOf(itemId);
		if (index == -1)
		{
			itemId = validIds.FirstOrDefault();
			if (itemId != null)
			{
				changePantStyle(itemId);
			}
		}
		else
		{
			index = Utility.WrapIndex(index + direction, validIds.Count);
			itemId = validIds[index];
			changePantStyle(itemId);
		}
	}

	public void changePantStyle(string itemId)
	{
		pants.Set(itemId);
		FarmerRenderer.changePants(itemId);
	}

	public void ConvertClothingOverrideToClothesItems()
	{
		if (IsOverridingPants(out var pantsId, out var color))
		{
			if (ItemRegistry.Exists("(P)" + pantsId))
			{
				Clothing clothes = new Clothing(pantsId);
				clothes.clothesColor.Value = color ?? Color.White;
				Equip(clothes, pantsItem);
			}
			pants.Value = "-1";
		}
		if (IsOverridingShirt(out var shirtId))
		{
			if (int.TryParse(shirtId, out var index) && index < 1000)
			{
				shirtId = (index + 1000).ToString();
			}
			if (ItemRegistry.Exists("(S)" + shirtId))
			{
				Clothing clothes = new Clothing(shirtId);
				Equip(clothes, shirtItem);
			}
			shirt.Value = "-1";
		}
	}

	public static Dictionary<int, string> GetHairStyleMetadataFile()
	{
		if (hairStyleMetadataFile == null)
		{
			hairStyleMetadataFile = DataLoader.HairData(Game1.content);
		}
		return hairStyleMetadataFile;
	}

	public static HairStyleMetadata GetHairStyleMetadata(int hair_index)
	{
		GetHairStyleMetadataFile();
		if (hairStyleMetadata.TryGetValue(hair_index, out var hair_data))
		{
			return hair_data;
		}
		try
		{
			if (hairStyleMetadataFile.TryGetValue(hair_index, out var data))
			{
				string[] split = data.Split('/');
				HairStyleMetadata new_hair_data = new HairStyleMetadata();
				new_hair_data.texture = Game1.content.Load<Texture2D>("Characters\\Farmer\\" + split[0]);
				new_hair_data.tileX = int.Parse(split[1]);
				new_hair_data.tileY = int.Parse(split[2]);
				if (split.Length > 3 && split[3].ToLower() == "true")
				{
					new_hair_data.usesUniqueLeftSprite = true;
				}
				else
				{
					new_hair_data.usesUniqueLeftSprite = false;
				}
				if (split.Length > 4)
				{
					new_hair_data.coveredIndex = int.Parse(split[4]);
				}
				if (split.Length > 5 && split[5].ToLower() == "true")
				{
					new_hair_data.isBaldStyle = true;
				}
				else
				{
					new_hair_data.isBaldStyle = false;
				}
				hair_data = new_hair_data;
			}
		}
		catch (Exception)
		{
		}
		hairStyleMetadata[hair_index] = hair_data;
		return hair_data;
	}

	public static List<int> GetAllHairstyleIndices()
	{
		if (allHairStyleIndices != null)
		{
			return allHairStyleIndices;
		}
		GetHairStyleMetadataFile();
		allHairStyleIndices = new List<int>();
		int highest_hair = FarmerRenderer.hairStylesTexture.Height / 96 * 8;
		for (int i = 0; i < highest_hair; i++)
		{
			allHairStyleIndices.Add(i);
		}
		foreach (int key in hairStyleMetadataFile.Keys)
		{
			if (key >= 0 && !allHairStyleIndices.Contains(key))
			{
				allHairStyleIndices.Add(key);
			}
		}
		allHairStyleIndices.Sort();
		return allHairStyleIndices;
	}

	public static int GetLastHairStyle()
	{
		return GetAllHairstyleIndices()[GetAllHairstyleIndices().Count - 1];
	}

	public void changeHairStyle(int whichHair)
	{
		bool num = isBald();
		if (GetHairStyleMetadata(whichHair) != null)
		{
			hair.Set(whichHair);
		}
		else
		{
			if (whichHair < 0)
			{
				whichHair = GetLastHairStyle();
			}
			else if (whichHair > GetLastHairStyle())
			{
				whichHair = 0;
			}
			hair.Set(whichHair);
		}
		if (IsBaldHairStyle(whichHair))
		{
			FarmerRenderer.textureName.Set(getTexture());
		}
		if (num && !isBald())
		{
			FarmerRenderer.textureName.Set(getTexture());
		}
	}

	public virtual bool IsBaldHairStyle(int style)
	{
		if (GetHairStyleMetadata(hair.Value) != null)
		{
			return GetHairStyleMetadata(hair.Value).isBaldStyle;
		}
		if ((uint)(style - 49) <= 6u)
		{
			return true;
		}
		return false;
	}

	private bool isBald()
	{
		return IsBaldHairStyle(getHair());
	}

	/// <summary>Change the color of the player's shoes.</summary>
	/// <param name="color">The new color to set.</param>
	public void changeShoeColor(string which)
	{
		FarmerRenderer.recolorShoes(which);
		shoes.Set(which);
	}

	/// <summary>Change the color of the player's hair.</summary>
	/// <param name="color">The new color to set.</param>
	public void changeHairColor(Color c)
	{
		hairstyleColor.Set(c);
	}

	/// <summary>Change the color of the player's equipped pants.</summary>
	/// <param name="color">The new color to set.</param>
	public void changePantsColor(Color color)
	{
		pantsColor.Set(color);
		pantsItem.Value?.clothesColor.Set(color);
	}

	public void changeHat(int newHat)
	{
		if (newHat < 0)
		{
			Equip(null, hat);
		}
		else
		{
			Equip(ItemRegistry.Create<Hat>("(H)" + newHat), hat);
		}
	}

	public void changeAccessory(int which)
	{
		if (which < -1)
		{
			which = 29;
		}
		if (which >= -1)
		{
			if (which >= 30)
			{
				which = -1;
			}
			accessory.Set(which);
		}
	}

	public void changeSkinColor(int which, bool force = false)
	{
		if (which < 0)
		{
			which = 23;
		}
		else if (which >= 24)
		{
			which = 0;
		}
		skin.Set(FarmerRenderer.recolorSkin(which, force));
	}

	/// <summary>Whether this player has dark skin for the purposes of child genetics.</summary>
	public virtual bool hasDarkSkin()
	{
		if ((int)skin < 4 || (int)skin > 8 || (int)skin == 7)
		{
			return (int)skin == 14;
		}
		return true;
	}

	/// <summary>Change the color of the player's eyes.</summary>
	/// <param name="color">The new color to set.</param>
	public void changeEyeColor(Color c)
	{
		newEyeColor.Set(c);
		FarmerRenderer.recolorEyes(c);
	}

	public int getHair(bool ignore_hat = false)
	{
		if (hat.Value != null && !bathingClothes && !ignore_hat)
		{
			switch ((Hat.HairDrawType)hat.Value.hairDrawType.Value)
			{
			case Hat.HairDrawType.HideHair:
				return -1;
			case Hat.HairDrawType.DrawObscuredHair:
				switch ((long)hair.Value)
				{
				case 50L:
				case 51L:
				case 52L:
				case 53L:
				case 54L:
				case 55L:
					return hair;
				case 48L:
					return 6;
				case 49L:
					return 52;
				case 3L:
					return 11;
				case 1L:
				case 5L:
				case 6L:
				case 9L:
				case 11L:
				case 17L:
				case 20L:
				case 23L:
				case 24L:
				case 25L:
				case 27L:
				case 28L:
				case 29L:
				case 30L:
				case 32L:
				case 33L:
				case 34L:
				case 36L:
				case 39L:
				case 41L:
				case 43L:
				case 44L:
				case 45L:
				case 46L:
				case 47L:
					return hair;
				case 18L:
				case 19L:
				case 21L:
				case 31L:
					return 23;
				case 42L:
					return 46;
				default:
					if ((int)hair >= 16)
					{
						if ((int)hair < 100)
						{
							return 30;
						}
						return hair;
					}
					return 7;
				}
			}
		}
		return hair;
	}

	public void changeGender(bool male)
	{
		if (male)
		{
			Gender = Gender.Male;
			FarmerRenderer.textureName.Set(getTexture());
			FarmerRenderer.heightOffset.Set(0);
		}
		else
		{
			Gender = Gender.Female;
			FarmerRenderer.heightOffset.Set(4);
			FarmerRenderer.textureName.Set(getTexture());
		}
		changeShirt(shirt);
	}

	public void changeFriendship(int amount, NPC n)
	{
		if (n == null || (!(n is Child) && !n.IsVillager))
		{
			return;
		}
		if (amount > 0 && stats.Get("Book_Friendship") != 0)
		{
			amount = (int)((float)amount * 1.1f);
		}
		if (amount > 0 && n.SpeaksDwarvish() && !canUnderstandDwarves)
		{
			return;
		}
		if (friendshipData.TryGetValue(n.Name, out var friendship))
		{
			if (n.isDivorcedFrom(this) && amount > 0)
			{
				return;
			}
			if (n.Equals(getSpouse()))
			{
				amount = (int)((float)amount * 0.66f);
			}
			friendship.Points = Math.Max(0, Math.Min(friendship.Points + amount, (Utility.GetMaximumHeartsForCharacter(n) + 1) * 250 - 1));
			if ((bool)n.datable && friendship.Points >= 2000 && !hasOrWillReceiveMail("Bouquet"))
			{
				Game1.addMailForTomorrow("Bouquet");
			}
			if ((bool)n.datable && friendship.Points >= 2500 && !hasOrWillReceiveMail("SeaAmulet"))
			{
				Game1.addMailForTomorrow("SeaAmulet");
			}
			if (friendship.Points < 0)
			{
				friendship.Points = 0;
			}
		}
		else
		{
			Game1.debugOutput = "Tried to change friendship for a friend that wasn't there.";
		}
		Game1.stats.checkForFriendshipAchievements();
	}

	public bool knowsRecipe(string name)
	{
		if (!craftingRecipes.Keys.Contains(name.Replace(" Recipe", "")))
		{
			return cookingRecipes.Keys.Contains(name.Replace(" Recipe", ""));
		}
		return true;
	}

	public Vector2 getUniformPositionAwayFromBox(int direction, int distance)
	{
		Microsoft.Xna.Framework.Rectangle bounds = GetBoundingBox();
		return FacingDirection switch
		{
			0 => new Vector2(bounds.Center.X, bounds.Y - distance), 
			1 => new Vector2(bounds.Right + distance, bounds.Center.Y), 
			2 => new Vector2(bounds.Center.X, bounds.Bottom + distance), 
			3 => new Vector2(bounds.X - distance, bounds.Center.Y), 
			_ => Vector2.Zero, 
		};
	}

	public bool hasTalkedToFriendToday(string npcName)
	{
		if (friendshipData.TryGetValue(npcName, out var friendship))
		{
			return friendship.TalkedToToday;
		}
		return false;
	}

	public void talkToFriend(NPC n, int friendshipPointChange = 20)
	{
		if (friendshipData.TryGetValue(n.Name, out var friendship) && !friendship.TalkedToToday)
		{
			changeFriendship(friendshipPointChange, n);
			friendship.TalkedToToday = true;
		}
	}

	public void moveRaft(GameLocation currentLocation, GameTime time)
	{
		float raftInertia = 0.2f;
		if (CanMove && Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveUpButton))
		{
			yVelocity = Math.Max(yVelocity - raftInertia, -3f + Math.Abs(xVelocity) / 2f);
			faceDirection(0);
		}
		if (CanMove && Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveRightButton))
		{
			xVelocity = Math.Min(xVelocity + raftInertia, 3f - Math.Abs(yVelocity) / 2f);
			faceDirection(1);
		}
		if (CanMove && Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveDownButton))
		{
			yVelocity = Math.Min(yVelocity + raftInertia, 3f - Math.Abs(xVelocity) / 2f);
			faceDirection(2);
		}
		if (CanMove && Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveLeftButton))
		{
			xVelocity = Math.Max(xVelocity - raftInertia, -3f + Math.Abs(yVelocity) / 2f);
			faceDirection(3);
		}
		Microsoft.Xna.Framework.Rectangle collidingBox = new Microsoft.Xna.Framework.Rectangle((int)base.Position.X, (int)(base.Position.Y + 64f + 16f), 64, 64);
		collidingBox.X += (int)Math.Ceiling(xVelocity);
		if (!currentLocation.isCollidingPosition(collidingBox, Game1.viewport, this))
		{
			position.X += xVelocity;
		}
		collidingBox.X -= (int)Math.Ceiling(xVelocity);
		collidingBox.Y += (int)Math.Floor(yVelocity);
		if (!currentLocation.isCollidingPosition(collidingBox, Game1.viewport, this))
		{
			position.Y += yVelocity;
		}
		if (xVelocity != 0f || yVelocity != 0f)
		{
			raftPuddleCounter -= time.ElapsedGameTime.Milliseconds;
			if (raftPuddleCounter <= 0)
			{
				raftPuddleCounter = 250;
				currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), 150f - (Math.Abs(xVelocity) + Math.Abs(yVelocity)) * 3f, 8, 0, new Vector2(collidingBox.X, collidingBox.Y - 64), flicker: false, Game1.random.NextBool(), 0.001f, 0.01f, Color.White, 1f, 0.003f, 0f, 0f));
				if (Game1.random.NextDouble() < 0.6)
				{
					Game1.playSound("wateringCan");
				}
				if (Game1.random.NextDouble() < 0.6)
				{
					raftBobCounter /= 2;
				}
			}
		}
		raftBobCounter -= time.ElapsedGameTime.Milliseconds;
		if (raftBobCounter <= 0)
		{
			raftBobCounter = Game1.random.Next(15, 28) * 100;
			if (yOffset <= 0f)
			{
				yOffset = 4f;
				currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), 150f - (Math.Abs(xVelocity) + Math.Abs(yVelocity)) * 3f, 8, 0, new Vector2(collidingBox.X, collidingBox.Y - 64), flicker: false, Game1.random.NextBool(), 0.001f, 0.01f, Color.White, 1f, 0.003f, 0f, 0f));
			}
			else
			{
				yOffset = 0f;
			}
		}
		if (xVelocity > 0f)
		{
			xVelocity = Math.Max(0f, xVelocity - raftInertia / 2f);
		}
		else if (xVelocity < 0f)
		{
			xVelocity = Math.Min(0f, xVelocity + raftInertia / 2f);
		}
		if (yVelocity > 0f)
		{
			yVelocity = Math.Max(0f, yVelocity - raftInertia / 2f);
		}
		else if (yVelocity < 0f)
		{
			yVelocity = Math.Min(0f, yVelocity + raftInertia / 2f);
		}
	}

	public void warpFarmer(Warp w, int warp_collide_direction)
	{
		if (w == null || Game1.eventUp)
		{
			return;
		}
		Halt();
		int target_x = w.TargetX;
		int target_y = w.TargetY;
		if (isRidingHorse())
		{
			switch (warp_collide_direction)
			{
			case 3:
				Game1.nextFarmerWarpOffsetX = -1;
				break;
			case 0:
				Game1.nextFarmerWarpOffsetY = -1;
				break;
			}
		}
		Game1.warpFarmer(w.TargetName, target_x, target_y, w.flipFarmer);
	}

	public void warpFarmer(Warp w)
	{
		warpFarmer(w, -1);
	}

	public void startToPassOut()
	{
		passOutEvent.Fire();
	}

	private void performPassOut()
	{
		if (isEmoteAnimating)
		{
			EndEmoteAnimation();
		}
		if (!swimming.Value && bathingClothes.Value)
		{
			bathingClothes.Value = false;
		}
		if (!passedOut && !FarmerSprite.isPassingOut())
		{
			faceDirection(2);
			completelyStopAnimatingOrDoingAction();
			animateOnce(293);
		}
	}

	public static void passOutFromTired(Farmer who)
	{
		if (!who.IsLocalPlayer)
		{
			return;
		}
		if (who.IsSitting())
		{
			who.StopSitting(animate: false);
		}
		if (who.isRidingHorse())
		{
			who.mount.dismount();
		}
		if (Game1.activeClickableMenu != null)
		{
			Game1.activeClickableMenu.emergencyShutDown();
			Game1.exitActiveMenu();
		}
		who.completelyStopAnimatingOrDoingAction();
		if ((bool)who.bathingClothes)
		{
			who.changeOutOfSwimSuit();
		}
		who.swimming.Value = false;
		who.CanMove = false;
		who.FarmerSprite.setCurrentSingleFrame(5, 3000);
		who.FarmerSprite.PauseForSingleAnimation = true;
		if (who == Game1.player && who.team.sleepAnnounceMode.Value != FarmerTeam.SleepAnnounceModes.Off)
		{
			string key = "PassedOut";
			string possibleLocationKey = "PassedOut_" + who.currentLocation.Name.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
			if (Game1.content.LoadStringReturnNullIfNotFound("Strings\\UI:Chat_" + possibleLocationKey) != null)
			{
				Game1.multiplayer.globalChatInfoMessage(possibleLocationKey, who.displayName);
			}
			else
			{
				int key_index = 0;
				for (int i = 0; i < 2; i++)
				{
					if (Game1.random.NextDouble() < 0.25)
					{
						key_index++;
					}
				}
				Game1.multiplayer.globalChatInfoMessage(key + key_index, who.displayName);
			}
		}
		if (Game1.currentLocation is FarmHouse farmhouse)
		{
			who.lastSleepLocation.Value = farmhouse.NameOrUniqueName;
			who.lastSleepPoint.Value = farmhouse.GetPlayerBedSpot();
		}
		Game1.multiplayer.sendPassoutRequest();
	}

	public static void performPassoutWarp(Farmer who, string bed_location_name, Point bed_point, bool has_bed)
	{
		GameLocation passOutLocation = who.currentLocationRef.Value;
		Vector2 bed = Utility.PointToVector2(bed_point) * 64f;
		Vector2 bed_tile = new Vector2((int)bed.X / 64, (int)bed.Y / 64);
		Vector2 bed_sleep_position = bed;
		if (!who.isInBed)
		{
			LocationRequest locationRequest = Game1.getLocationRequest(bed_location_name);
			Game1.warpFarmer(locationRequest, (int)bed.X / 64, (int)bed.Y / 64, 2);
			locationRequest.OnWarp += ContinuePassOut;
			who.FarmerSprite.setCurrentSingleFrame(5, 3000);
			who.FarmerSprite.PauseForSingleAnimation = true;
		}
		else
		{
			ContinuePassOut();
		}
		void ContinuePassOut()
		{
			who.Position = bed_sleep_position;
			who.currentLocation.lastTouchActionLocation = bed_tile;
			(who.NetFields.Root as NetRoot<Farmer>)?.CancelInterpolation();
			if (!Game1.IsMultiplayer || Game1.timeOfDay >= 2600)
			{
				Game1.PassOutNewDay();
			}
			Game1.changeMusicTrack("none");
			if (!(passOutLocation is FarmHouse) && !(passOutLocation is IslandFarmHouse) && !(passOutLocation is Cellar) && !passOutLocation.HasMapPropertyWithValue("PassOutSafe"))
			{
				Random r = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed, who.UniqueMultiplayerID);
				int max_passout_cost = passOutLocation.GetLocationContext().MaxPassOutCost;
				if (max_passout_cost == -1)
				{
					max_passout_cost = LocationContexts.Default.MaxPassOutCost;
				}
				int moneyToTake = Math.Min(max_passout_cost, who.Money / 10);
				List<PassOutMailData> obj = passOutLocation.GetLocationContext().PassOutMail ?? LocationContexts.Default.PassOutMail;
				PassOutMailData selected_mail = null;
				List<PassOutMailData> valid_mails = new List<PassOutMailData>();
				foreach (PassOutMailData mail in obj)
				{
					if (GameStateQuery.CheckConditions(mail.Condition, passOutLocation, null, null, null, r))
					{
						if (mail.SkipRandomSelection)
						{
							selected_mail = mail;
							break;
						}
						valid_mails.Add(mail);
					}
				}
				if (selected_mail == null && valid_mails.Count > 0)
				{
					selected_mail = r.ChooseFrom(valid_mails);
				}
				string mail_to_send = null;
				if (selected_mail != null)
				{
					if (selected_mail.MaxPassOutCost >= 0)
					{
						moneyToTake = Math.Min(moneyToTake, selected_mail.MaxPassOutCost);
					}
					string mailName = selected_mail.Mail;
					if (!string.IsNullOrEmpty(mailName))
					{
						Dictionary<string, string> mails = DataLoader.Mail(Game1.content);
						mail_to_send = (mails.ContainsKey(mailName + "_" + ((moneyToTake > 0) ? "Billed" : "NotBilled") + "_" + (who.IsMale ? "Male" : "Female")) ? (mailName + "_" + ((moneyToTake > 0) ? "Billed" : "NotBilled") + "_" + (who.IsMale ? "Male" : "Female")) : (mails.ContainsKey(mailName + "_" + ((moneyToTake > 0) ? "Billed" : "NotBilled")) ? (mailName + "_" + ((moneyToTake > 0) ? "Billed" : "NotBilled")) : ((!mails.ContainsKey(mailName)) ? "passedOut2" : mailName)));
						if (mail_to_send.StartsWith("passedOut"))
						{
							mail_to_send = mail_to_send + " " + moneyToTake;
						}
					}
				}
				if (moneyToTake > 0)
				{
					who.Money -= moneyToTake;
				}
				if (mail_to_send != null)
				{
					who.mailForTomorrow.Add(mail_to_send);
				}
			}
		}
	}

	public static void doSleepEmote(Farmer who)
	{
		who.doEmote(24);
		who.yJumpVelocity = -2f;
	}

	public override Microsoft.Xna.Framework.Rectangle GetBoundingBox()
	{
		if (mount != null && !mount.dismounting)
		{
			return mount.GetBoundingBox();
		}
		Vector2 position = base.Position;
		return new Microsoft.Xna.Framework.Rectangle((int)position.X + 8, (int)position.Y + Sprite.getHeight() - 32, 48, 32);
	}

	public string getPetName()
	{
		foreach (NPC n in Game1.getFarm().characters)
		{
			if (n is Pet)
			{
				return n.Name;
			}
		}
		foreach (Farmer allFarmer in Game1.getAllFarmers())
		{
			foreach (NPC n in Utility.getHomeOfFarmer(allFarmer).characters)
			{
				if (n is Pet)
				{
					return n.Name;
				}
			}
		}
		return "your pet";
	}

	public Pet getPet()
	{
		foreach (NPC character in Game1.getFarm().characters)
		{
			if (character is Pet pet)
			{
				return pet;
			}
		}
		foreach (Farmer allFarmer in Game1.getAllFarmers())
		{
			foreach (NPC character2 in Utility.getHomeOfFarmer(allFarmer).characters)
			{
				if (character2 is Pet pet)
				{
					return pet;
				}
			}
		}
		return null;
	}

	public string getPetDisplayName()
	{
		foreach (NPC n in Game1.getFarm().characters)
		{
			if (n is Pet)
			{
				return n.displayName;
			}
		}
		foreach (Farmer allFarmer in Game1.getAllFarmers())
		{
			foreach (NPC n in Utility.getHomeOfFarmer(allFarmer).characters)
			{
				if (n is Pet)
				{
					return n.displayName;
				}
			}
		}
		return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1972");
	}

	public bool hasPet()
	{
		foreach (NPC character in Game1.getFarm().characters)
		{
			if (character is Pet)
			{
				return true;
			}
		}
		foreach (Farmer allFarmer in Game1.getAllFarmers())
		{
			foreach (NPC character2 in Utility.getHomeOfFarmer(allFarmer).characters)
			{
				if (character2 is Pet)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void UpdateClothing()
	{
		FarmerRenderer.MarkSpriteDirty();
	}

	/// <summary>Get whether custom pants should be drawn instead of the equipped pants item.</summary>
	/// <param name="id">The pants ID to draw, if overridden.</param>
	/// <param name="color">The pants color to draw, if overridden.</param>
	public bool IsOverridingPants(out string id, out Color? color)
	{
		if (pants.Value != null && pants.Value != "-1")
		{
			id = pants.Value;
			color = pantsColor.Value;
			return true;
		}
		id = null;
		color = null;
		return false;
	}

	/// <summary>Get whether the current pants can be dyed.</summary>
	public bool CanDyePants()
	{
		return pantsItem.Value?.dyeable.Value ?? false;
	}

	/// <summary>Get the pants to draw on the farmer.</summary>
	/// <param name="texture">The texture to render.</param>
	/// <param name="spriteIndex">The sprite index in the <paramref name="texture" />.</param>
	public void GetDisplayPants(out Texture2D texture, out int spriteIndex)
	{
		if (IsOverridingPants(out var id, out var _))
		{
			ParsedItemData itemData = ItemRegistry.GetData("(P)" + id);
			if (itemData != null && !itemData.IsErrorItem)
			{
				texture = itemData.GetTexture();
				spriteIndex = itemData.SpriteIndex;
				return;
			}
		}
		if (pantsItem.Value != null)
		{
			ParsedItemData data = ItemRegistry.GetDataOrErrorItem(pantsItem.Value.QualifiedItemId);
			if (data != null && !data.IsErrorItem)
			{
				texture = data.GetTexture();
				spriteIndex = pantsItem.Value.indexInTileSheet;
				return;
			}
		}
		texture = FarmerRenderer.pantsTexture;
		spriteIndex = 14;
	}

	/// <summary>Get the unqualified item ID for the displayed pants (which aren't necessarily the equipped ones).</summary>
	public string GetPantsId()
	{
		if (IsOverridingPants(out var id, out var _))
		{
			return id;
		}
		return pantsItem.Value?.ItemId ?? "14";
	}

	public int GetPantsIndex()
	{
		GetDisplayPants(out var _, out var index);
		return index;
	}

	/// <summary>Get whether a custom shirt should be drawn instead of the equipped shirt item.</summary>
	/// <param name="id">The shirt ID to draw, if overridden.</param>
	public bool IsOverridingShirt(out string id)
	{
		if (shirt.Value != null && shirt.Value != "-1")
		{
			id = shirt.Value;
			return true;
		}
		id = null;
		return false;
	}

	/// <summary>Get whether the current shirt can be dyed.</summary>
	public bool CanDyeShirt()
	{
		return shirtItem.Value?.dyeable.Value ?? false;
	}

	/// <summary>Get the shirt to draw on the farmer.</summary>
	/// <param name="texture">The texture to render.</param>
	/// <param name="spriteIndex">The sprite index in the <paramref name="texture" />.</param>
	public void GetDisplayShirt(out Texture2D texture, out int spriteIndex)
	{
		if (IsOverridingShirt(out var id))
		{
			ParsedItemData itemData = ItemRegistry.GetData("(S)" + id);
			if (itemData != null && !itemData.IsErrorItem)
			{
				texture = itemData.GetTexture();
				spriteIndex = itemData.SpriteIndex;
				return;
			}
		}
		if (shirtItem.Value != null)
		{
			ParsedItemData data = ItemRegistry.GetDataOrErrorItem(shirtItem.Value.QualifiedItemId);
			if (data != null && !data.IsErrorItem)
			{
				texture = data.GetTexture();
				spriteIndex = shirtItem.Value.indexInTileSheet;
				return;
			}
		}
		texture = FarmerRenderer.shirtsTexture;
		spriteIndex = (IsMale ? 209 : 41);
	}

	/// <summary>Get the unqualified item ID for the displayed shirt (which isn't necessarily the equipped one).</summary>
	public string GetShirtId()
	{
		if (IsOverridingShirt(out var id))
		{
			return id;
		}
		if (shirtItem.Value != null)
		{
			return shirtItem.Value.ItemId;
		}
		if (!IsMale)
		{
			return "1041";
		}
		return "1209";
	}

	public int GetShirtIndex()
	{
		GetDisplayShirt(out var _, out var index);
		return index;
	}

	public bool ShirtHasSleeves()
	{
		if (!IsOverridingShirt(out var itemId))
		{
			itemId = shirtItem.Value?.ItemId;
		}
		if (itemId != null && Game1.shirtData.TryGetValue(itemId, out var data))
		{
			return data.HasSleeves;
		}
		return true;
	}

	/// <summary>Get the color of the currently worn shirt.</summary>
	public Color GetShirtColor()
	{
		if (IsOverridingShirt(out var id) && Game1.shirtData.TryGetValue(id, out var shirtData))
		{
			if (!shirtData.IsPrismatic)
			{
				return Utility.StringToColor(shirtData.DefaultColor) ?? Color.White;
			}
			return Utility.GetPrismaticColor();
		}
		if (shirtItem.Value != null)
		{
			if ((bool)shirtItem.Value.isPrismatic)
			{
				return Utility.GetPrismaticColor();
			}
			return shirtItem.Value.clothesColor.Value;
		}
		return DEFAULT_SHIRT_COLOR;
	}

	/// <summary>Get the color of the currently worn pants.</summary>
	public Color GetPantsColor()
	{
		if (IsOverridingPants(out var _, out var color))
		{
			return color ?? Color.White;
		}
		if (pantsItem.Value != null)
		{
			if ((bool)pantsItem.Value.isPrismatic)
			{
				return Utility.GetPrismaticColor();
			}
			return pantsItem.Value.clothesColor.Value;
		}
		return Color.White;
	}

	public bool movedDuringLastTick()
	{
		return !base.Position.Equals(lastPosition);
	}

	public int CompareTo(object obj)
	{
		return ((Farmer)obj).saveTime - saveTime;
	}

	public virtual void SetOnBridge(bool val)
	{
		if (onBridge.Value != val)
		{
			onBridge.Value = val;
			if ((bool)onBridge)
			{
				showNotCarrying();
			}
		}
	}

	public float getDrawLayer()
	{
		if (onBridge.Value)
		{
			return (float)base.StandingPixel.Y / 10000f + drawLayerDisambiguator + 0.0256f;
		}
		if (IsSitting() && mapChairSitPosition.Value.X != -1f && mapChairSitPosition.Value.Y != -1f)
		{
			return (mapChairSitPosition.Value.Y + 1f) * 64f / 10000f;
		}
		return (float)base.StandingPixel.Y / 10000f + drawLayerDisambiguator;
	}

	public override void draw(SpriteBatch b)
	{
		if (base.currentLocation == null || (!base.currentLocation.Equals(Game1.currentLocation) && !IsLocalPlayer && !Game1.currentLocation.IsTemporary && !isFakeEventActor) || ((bool)hidden && (base.currentLocation.currentEvent == null || this != base.currentLocation.currentEvent.farmer) && (!IsLocalPlayer || Game1.locationRequest == null)) || (viewingLocation.Value != null && IsLocalPlayer))
		{
			return;
		}
		float draw_layer = getDrawLayer();
		if (isRidingHorse())
		{
			mount.SyncPositionToRider();
			mount.draw(b);
			if (FacingDirection == 3 || FacingDirection == 1)
			{
				draw_layer += 0.0016f;
			}
		}
		float layerDepth = FarmerRenderer.GetLayerDepth(0f, FarmerRenderer.FarmerSpriteLayers.MAX);
		Vector2 origin = new Vector2(xOffset, (yOffset + 128f - (float)(GetBoundingBox().Height / 2)) / 4f + 4f);
		Point standingPixel = base.StandingPixel;
		Tile shadowTile = Game1.currentLocation.Map.RequireLayer("Buildings").PickTile(new Location(standingPixel.X, standingPixel.Y), Game1.viewport.Size);
		float glow_offset = layerDepth * 1f;
		float shadow_offset = layerDepth * 2f;
		if (isGlowing)
		{
			if (coloredBorder)
			{
				b.Draw(Sprite.Texture, new Vector2(getLocalPosition(Game1.viewport).X - 4f, getLocalPosition(Game1.viewport).Y - 4f), Sprite.SourceRect, glowingColor * glowingTransparency, 0f, Vector2.Zero, 1.1f, SpriteEffects.None, draw_layer + glow_offset);
			}
			else
			{
				FarmerRenderer.draw(b, FarmerSprite, FarmerSprite.SourceRect, getLocalPosition(Game1.viewport) + jitter + new Vector2(0f, yJumpOffset), origin, draw_layer + glow_offset, glowingColor * glowingTransparency, rotation, this);
			}
		}
		if ((!(shadowTile?.TileIndexProperties.ContainsKey("Shadow"))) ?? true)
		{
			if (IsSitting() || !Game1.shouldTimePass() || !temporarilyInvincible || !flashDuringThisTemporaryInvincibility || temporaryInvincibilityTimer % 100 < 50)
			{
				farmerRenderer.Value.draw(b, FarmerSprite, FarmerSprite.SourceRect, getLocalPosition(Game1.viewport) + jitter + new Vector2(0f, yJumpOffset), origin, draw_layer, Color.White, rotation, this);
			}
		}
		else
		{
			farmerRenderer.Value.draw(b, FarmerSprite, FarmerSprite.SourceRect, getLocalPosition(Game1.viewport), origin, draw_layer, Color.White, rotation, this);
			farmerRenderer.Value.draw(b, FarmerSprite, FarmerSprite.SourceRect, getLocalPosition(Game1.viewport), origin, draw_layer + shadow_offset, Color.Black * 0.25f, rotation, this);
		}
		if (isRafting)
		{
			b.Draw(Game1.toolSpriteSheet, getLocalPosition(Game1.viewport) + new Vector2(0f, yOffset), Game1.getSourceRectForStandardTileSheet(Game1.toolSpriteSheet, 1), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, FarmerRenderer.GetLayerDepth(draw_layer, FarmerRenderer.FarmerSpriteLayers.ToolUp));
		}
		if (Game1.activeClickableMenu == null && !Game1.eventUp && IsLocalPlayer && CurrentTool != null && (Game1.oldKBState.IsKeyDown(Keys.LeftShift) || Game1.options.alwaysShowToolHitLocation) && CurrentTool.doesShowTileLocationMarker() && (!Game1.options.hideToolHitLocationWhenInMotion || !isMoving()))
		{
			Vector2 mouse_position = Utility.PointToVector2(Game1.getMousePosition()) + new Vector2(Game1.viewport.X, Game1.viewport.Y);
			Vector2 draw_location = Game1.GlobalToLocal(Game1.viewport, Utility.clampToTile(GetToolLocation(mouse_position)));
			b.Draw(Game1.mouseCursors, draw_location, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 29), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, draw_location.Y / 10000f);
		}
		if (base.IsEmoting)
		{
			Vector2 emotePosition = getLocalPosition(Game1.viewport);
			emotePosition.Y -= 160f;
			b.Draw(Game1.emoteSpriteSheet, emotePosition, new Microsoft.Xna.Framework.Rectangle(base.CurrentEmoteIndex * 16 % Game1.emoteSpriteSheet.Width, base.CurrentEmoteIndex * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, draw_layer);
		}
		if (ActiveObject != null && IsCarrying())
		{
			Game1.drawPlayerHeldObject(this);
		}
		sparklingText?.draw(b, Game1.GlobalToLocal(Game1.viewport, base.Position + new Vector2(32f - sparklingText.textWidth / 2f, -128f)));
		if (UsingTool && CurrentTool != null)
		{
			Game1.drawTool(this);
		}
		foreach (Companion companion in companions)
		{
			companion.Draw(b);
		}
	}

	public virtual void DrawUsername(SpriteBatch b)
	{
		if (!Game1.IsMultiplayer || Game1.multiplayer == null || LocalMultiplayer.IsLocalMultiplayer(is_local_only: true) || usernameDisplayTime <= 0f)
		{
			return;
		}
		string username = Game1.multiplayer.getUserName(UniqueMultiplayerID);
		if (username == null)
		{
			return;
		}
		Vector2 string_size = Game1.smallFont.MeasureString(username);
		Vector2 draw_origin = getLocalPosition(Game1.viewport) + new Vector2(32f, -104f) - string_size / 2f;
		for (int x = -1; x <= 1; x++)
		{
			for (int y = -1; y <= 1; y++)
			{
				if (x != 0 || y != 0)
				{
					b.DrawString(Game1.smallFont, username, draw_origin + new Vector2(x, y) * 2f, Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9999f);
				}
			}
		}
		b.DrawString(Game1.smallFont, username, draw_origin, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
	}

	public static void drinkGlug(Farmer who)
	{
		Color c = Color.LightBlue;
		if (who.itemToEat != null)
		{
			switch (ArgUtility.SplitBySpace(who.itemToEat.Name).Last())
			{
			case "Tonic":
				c = Color.Red;
				break;
			case "Remedy":
				c = Color.LimeGreen;
				break;
			case "Cola":
			case "Espresso":
			case "Coffee":
				c = new Color(46, 20, 0);
				break;
			case "Wine":
				c = Color.Purple;
				break;
			case "Beer":
				c = Color.Orange;
				break;
			case "Milk":
				c = Color.White;
				break;
			case "Tea":
			case "Juice":
				c = Color.LightGreen;
				break;
			case "Mayonnaise":
				c = ((who.itemToEat.Name == "Void Mayonnaise") ? Color.Black : Color.White);
				break;
			case "Soup":
				c = Color.LightGreen;
				break;
			}
		}
		if (Game1.currentLocation == who.currentLocation)
		{
			Game1.playSound((who.itemToEat != null && who.itemToEat is Object o && o.preserve.Value == Object.PreserveType.Pickle) ? "eat" : "gulp");
		}
		who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(653, 858, 1, 1), 9999f, 1, 1, who.Position + new Vector2(32 + Game1.random.Next(-2, 3) * 4, -48f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f + 0.001f, 0.04f, c, 5f, 0f, 0f, 0f)
		{
			acceleration = new Vector2(0f, 0.5f)
		});
	}

	public void handleDisconnect()
	{
		if (base.currentLocation != null)
		{
			rightRing.Value?.onLeaveLocation(this, base.currentLocation);
			leftRing.Value?.onLeaveLocation(this, base.currentLocation);
		}
		UnapplyAllTrinketEffects();
		disconnectDay.Value = (int)Game1.stats.DaysPlayed;
		disconnectLocation.Value = base.currentLocation.NameOrUniqueName;
		disconnectPosition.Value = base.Position;
	}

	public bool isDivorced()
	{
		foreach (Friendship value in friendshipData.Values)
		{
			if (value.IsDivorced())
			{
				return true;
			}
		}
		return false;
	}

	public void wipeExMemories()
	{
		foreach (string npcName in friendshipData.Keys)
		{
			Friendship friendship = friendshipData[npcName];
			if (friendship.IsDivorced())
			{
				friendship.Clear();
				NPC n = Game1.getCharacterFromName(npcName);
				if (n != null)
				{
					n.CurrentDialogue.Clear();
					n.CurrentDialogue.Push(n.TryGetDialogue("WipedMemory") ?? new Dialogue(n, "Strings\\Characters:WipedMemory"));
					Game1.stats.Increment("exMemoriesWiped");
				}
			}
		}
	}

	public void getRidOfChildren()
	{
		FarmHouse farmhouse = Utility.getHomeOfFarmer(this);
		for (int i = farmhouse.characters.Count - 1; i >= 0; i--)
		{
			if (farmhouse.characters[i] is Child child)
			{
				farmhouse.GetChildBed((int)child.Gender)?.mutex.ReleaseLock();
				if (child.hat.Value != null)
				{
					Hat hat = child.hat.Value;
					child.hat.Value = null;
					team.returnedDonations.Add(hat);
					team.newLostAndFoundItems.Value = true;
				}
				farmhouse.characters.RemoveAt(i);
				Game1.stats.Increment("childrenTurnedToDoves");
			}
		}
	}

	public void animateOnce(int whichAnimation)
	{
		FarmerSprite.animateOnce(whichAnimation, 100f, 6);
		CanMove = false;
	}

	public static void showItemIntake(Farmer who)
	{
		TemporaryAnimatedSprite tempSprite = null;
		Object toShow = ((!(who.mostRecentlyGrabbedItem is Object grabbedObj)) ? ((who.ActiveObject == null) ? null : who.ActiveObject) : grabbedObj);
		if (toShow == null)
		{
			return;
		}
		ParsedItemData dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(toShow.QualifiedItemId);
		string textureName = dataOrErrorItem.TextureName;
		Microsoft.Xna.Framework.Rectangle sourceRect = dataOrErrorItem.GetSourceRect();
		switch (who.FacingDirection)
		{
		case 2:
			switch (who.FarmerSprite.currentAnimationIndex)
			{
			case 1:
				tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 100f, 1, 0, who.Position + new Vector2(0f, -32f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f);
				break;
			case 2:
				tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 100f, 1, 0, who.Position + new Vector2(0f, -43f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f);
				break;
			case 3:
				tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 100f, 1, 0, who.Position + new Vector2(0f, -128f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f);
				break;
			case 4:
				tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 200f, 1, 0, who.Position + new Vector2(0f, -120f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f);
				break;
			case 5:
				tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 200f, 1, 0, who.Position + new Vector2(0f, -120f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0.02f, Color.White, 4f, -0.02f, 0f, 0f);
				break;
			}
			break;
		case 1:
			switch (who.FarmerSprite.currentAnimationIndex)
			{
			case 1:
				tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 100f, 1, 0, who.Position + new Vector2(28f, -64f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f);
				break;
			case 2:
				tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 100f, 1, 0, who.Position + new Vector2(24f, -72f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f);
				break;
			case 3:
				tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 100f, 1, 0, who.Position + new Vector2(4f, -128f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f);
				break;
			case 4:
				tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 200f, 1, 0, who.Position + new Vector2(0f, -124f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f);
				break;
			case 5:
				tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 200f, 1, 0, who.Position + new Vector2(0f, -124f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0.02f, Color.White, 4f, -0.02f, 0f, 0f);
				break;
			}
			break;
		case 0:
			switch (who.FarmerSprite.currentAnimationIndex)
			{
			case 1:
				tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 100f, 1, 0, who.Position + new Vector2(0f, -32f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f - 0.001f, 0f, Color.White, 4f, 0f, 0f, 0f);
				break;
			case 2:
				tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 100f, 1, 0, who.Position + new Vector2(0f, -43f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f - 0.001f, 0f, Color.White, 4f, 0f, 0f, 0f);
				break;
			case 3:
				tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 100f, 1, 0, who.Position + new Vector2(0f, -128f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f - 0.001f, 0f, Color.White, 4f, 0f, 0f, 0f);
				break;
			case 4:
				tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 200f, 1, 0, who.Position + new Vector2(0f, -120f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f - 0.001f, 0f, Color.White, 4f, 0f, 0f, 0f);
				break;
			case 5:
				tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 200f, 1, 0, who.Position + new Vector2(0f, -120f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f - 0.001f, 0.02f, Color.White, 4f, -0.02f, 0f, 0f);
				break;
			}
			break;
		case 3:
			switch (who.FarmerSprite.currentAnimationIndex)
			{
			case 1:
				tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 100f, 1, 0, who.Position + new Vector2(-32f, -64f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f);
				break;
			case 2:
				tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 100f, 1, 0, who.Position + new Vector2(-28f, -76f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f);
				break;
			case 3:
				tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 100f, 1, 0, who.Position + new Vector2(-16f, -128f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f);
				break;
			case 4:
				tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 200f, 1, 0, who.Position + new Vector2(0f, -124f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f);
				break;
			case 5:
				tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 200f, 1, 0, who.Position + new Vector2(0f, -124f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0.02f, Color.White, 4f, -0.02f, 0f, 0f);
				break;
			}
			break;
		}
		if (toShow.QualifiedItemId == who.ActiveObject?.QualifiedItemId && who.FarmerSprite.currentAnimationIndex == 5)
		{
			tempSprite = null;
		}
		if (tempSprite != null)
		{
			who.currentLocation.temporarySprites.Add(tempSprite);
		}
		if (who.mostRecentlyGrabbedItem is ColoredObject coloredObj && tempSprite != null)
		{
			Microsoft.Xna.Framework.Rectangle coloredSourceRect = ItemRegistry.GetDataOrErrorItem(coloredObj.QualifiedItemId).GetSourceRect(1);
			who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(textureName, coloredSourceRect, tempSprite.interval, 1, 0, tempSprite.Position, flicker: false, flipped: false, tempSprite.layerDepth + 0.0001f, tempSprite.alphaFade, coloredObj.color.Value, 4f, tempSprite.scaleChange, 0f, 0f));
		}
		if (who.FarmerSprite.currentAnimationIndex == 5)
		{
			who.Halt();
			who.FarmerSprite.CurrentAnimation = null;
		}
	}

	public virtual void showSwordSwipe(Farmer who)
	{
		TemporaryAnimatedSprite tempSprite = null;
		Vector2 actionTile = who.GetToolLocation(ignoreClick: true);
		bool dagger = false;
		if (who.CurrentTool is MeleeWeapon weapon)
		{
			dagger = (int)weapon.type == 1;
			if (!dagger)
			{
				weapon.DoDamage(who.currentLocation, (int)actionTile.X, (int)actionTile.Y, who.FacingDirection, 1, who);
			}
		}
		int min_swipe_interval = 20;
		switch (who.FacingDirection)
		{
		case 2:
			switch (who.FarmerSprite.currentAnimationIndex)
			{
			case 0:
				if (dagger)
				{
					who.yVelocity = -0.6f;
				}
				break;
			case 1:
				who.yVelocity = (dagger ? 0.5f : (-0.5f));
				break;
			case 5:
				who.yVelocity = 0.3f;
				tempSprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(503, 256, 42, 17), who.Position + new Vector2(-16f, -2f) * 4f, flipped: false, 0.07f, Color.White)
				{
					scale = 4f,
					animationLength = 1,
					interval = Math.Max(who.FarmerSprite.CurrentAnimationFrame.milliseconds, min_swipe_interval),
					alpha = 0.5f,
					layerDepth = (who.Position.Y + 64f) / 10000f
				};
				break;
			}
			break;
		case 1:
			switch (who.FarmerSprite.currentAnimationIndex)
			{
			case 0:
				if (dagger)
				{
					who.xVelocity = 0.6f;
				}
				break;
			case 1:
				who.xVelocity = (dagger ? (-0.5f) : 0.5f);
				break;
			case 5:
				who.xVelocity = -0.3f;
				tempSprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(518, 274, 23, 31), who.Position + new Vector2(4f, -12f) * 4f, flipped: false, 0.07f, Color.White)
				{
					scale = 4f,
					animationLength = 1,
					interval = Math.Max(who.FarmerSprite.CurrentAnimationFrame.milliseconds, min_swipe_interval),
					alpha = 0.5f
				};
				break;
			}
			break;
		case 3:
			switch (who.FarmerSprite.currentAnimationIndex)
			{
			case 0:
				if (dagger)
				{
					who.xVelocity = -0.6f;
				}
				break;
			case 1:
				who.xVelocity = (dagger ? 0.5f : (-0.5f));
				break;
			case 5:
				who.xVelocity = 0.3f;
				tempSprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(518, 274, 23, 31), who.Position + new Vector2(-15f, -12f) * 4f, flipped: false, 0.07f, Color.White)
				{
					scale = 4f,
					animationLength = 1,
					interval = Math.Max(who.FarmerSprite.CurrentAnimationFrame.milliseconds, min_swipe_interval),
					flipped = true,
					alpha = 0.5f
				};
				break;
			}
			break;
		case 0:
			switch (who.FarmerSprite.currentAnimationIndex)
			{
			case 0:
				if (dagger)
				{
					who.yVelocity = 0.6f;
				}
				break;
			case 1:
				who.yVelocity = (dagger ? (-0.5f) : 0.5f);
				break;
			case 5:
				who.yVelocity = -0.3f;
				tempSprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(518, 274, 23, 31), who.Position + new Vector2(0f, -32f) * 4f, flipped: false, 0.07f, Color.White)
				{
					scale = 4f,
					animationLength = 1,
					interval = Math.Max(who.FarmerSprite.CurrentAnimationFrame.milliseconds, min_swipe_interval),
					alpha = 0.5f,
					rotation = 3.926991f
				};
				break;
			}
			break;
		}
		if (tempSprite != null)
		{
			if (who.CurrentTool?.QualifiedItemId == "(W)4")
			{
				tempSprite.color = Color.HotPink;
			}
			who.currentLocation.temporarySprites.Add(tempSprite);
		}
	}

	public static void showToolSwipeEffect(Farmer who)
	{
		if (!(who.CurrentTool is WateringCan))
		{
			switch (who.FacingDirection)
			{
			case 1:
				who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(15, who.Position + new Vector2(20f, -132f), Color.White, 4, flipped: false, (who.stamina <= 0f) ? 80f : 40f, 0, 128, 1f, 128)
				{
					layerDepth = (float)(who.GetBoundingBox().Bottom + 1) / 10000f
				});
				break;
			case 3:
				who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(15, who.Position + new Vector2(-92f, -132f), Color.White, 4, flipped: true, (who.stamina <= 0f) ? 80f : 40f, 0, 128, 1f, 128)
				{
					layerDepth = (float)(who.GetBoundingBox().Bottom + 1) / 10000f
				});
				break;
			case 2:
				who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(19, who.Position + new Vector2(-4f, -128f), Color.White, 4, flipped: false, (who.stamina <= 0f) ? 80f : 40f, 0, 128, 1f, 128)
				{
					layerDepth = (float)(who.GetBoundingBox().Bottom + 1) / 10000f
				});
				break;
			case 0:
				who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(18, who.Position + new Vector2(0f, -132f), Color.White, 4, flipped: false, (who.stamina <= 0f) ? 100f : 50f, 0, 64, 1f, 64)
				{
					layerDepth = (float)(who.StandingPixel.Y - 9) / 10000f
				});
				break;
			}
		}
	}

	public static void canMoveNow(Farmer who)
	{
		who.CanMove = true;
		who.UsingTool = false;
		who.usingSlingshot = false;
		who.FarmerSprite.PauseForSingleAnimation = false;
		who.yVelocity = 0f;
		who.xVelocity = 0f;
	}

	public void FireTool()
	{
		fireToolEvent.Fire();
	}

	public void synchronizedJump(float velocity)
	{
		if (IsLocalPlayer)
		{
			synchronizedJumpEvent.Fire(velocity);
			synchronizedJumpEvent.Poll();
		}
	}

	protected void performSynchronizedJump(float velocity)
	{
		yJumpVelocity = velocity;
		yJumpOffset = -1;
	}

	private void performFireTool()
	{
		if (isEmoteAnimating)
		{
			EndEmoteAnimation();
		}
		CurrentTool?.leftClick(this);
	}

	public static void useTool(Farmer who)
	{
		if (who.toolOverrideFunction != null)
		{
			who.toolOverrideFunction(who);
		}
		else if (who.CurrentTool != null)
		{
			float oldStamina = who.stamina;
			if (who.IsLocalPlayer)
			{
				who.CurrentTool.DoFunction(who.currentLocation, (int)who.GetToolLocation().X, (int)who.GetToolLocation().Y, 1, who);
			}
			who.lastClick = Vector2.Zero;
			who.checkForExhaustion(oldStamina);
		}
	}

	public void BeginUsingTool()
	{
		beginUsingToolEvent.Fire();
	}

	private void performBeginUsingTool()
	{
		if (isEmoteAnimating)
		{
			EndEmoteAnimation();
		}
		if (CurrentTool != null)
		{
			CanMove = false;
			UsingTool = true;
			canReleaseTool = true;
			CurrentTool.beginUsing(base.currentLocation, (int)lastClick.X, (int)lastClick.Y, this);
		}
	}

	public void EndUsingTool()
	{
		if (this == Game1.player)
		{
			endUsingToolEvent.Fire();
		}
		else
		{
			performEndUsingTool();
		}
	}

	private void performEndUsingTool()
	{
		if (isEmoteAnimating)
		{
			EndEmoteAnimation();
		}
		CurrentTool?.endUsing(base.currentLocation, this);
	}

	public void checkForExhaustion(float oldStamina)
	{
		if (stamina <= 0f && oldStamina > 0f)
		{
			if (!exhausted && IsLocalPlayer)
			{
				Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1986"));
			}
			setRunning(isRunning: false);
			doEmote(36);
		}
		else if (stamina <= 15f && oldStamina > 15f && IsLocalPlayer)
		{
			Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1987"));
		}
		if (stamina <= 0f)
		{
			exhausted.Value = true;
		}
	}

	public void setMoving(byte command)
	{
		switch (command)
		{
		case 1:
			if (movementDirections.Count < 2 && !movementDirections.Contains(0) && !movementDirections.Contains(2))
			{
				movementDirections.Insert(0, 0);
			}
			break;
		case 2:
			if (movementDirections.Count < 2 && !movementDirections.Contains(1) && !movementDirections.Contains(3))
			{
				movementDirections.Insert(0, 1);
			}
			break;
		case 4:
			if (movementDirections.Count < 2 && !movementDirections.Contains(2) && !movementDirections.Contains(0))
			{
				movementDirections.Insert(0, 2);
			}
			break;
		case 8:
			if (movementDirections.Count < 2 && !movementDirections.Contains(3) && !movementDirections.Contains(1))
			{
				movementDirections.Insert(0, 3);
			}
			break;
		case 33:
			movementDirections.Remove(0);
			break;
		case 34:
			movementDirections.Remove(1);
			break;
		case 36:
			movementDirections.Remove(2);
			break;
		case 40:
			movementDirections.Remove(3);
			break;
		case 16:
			setRunning(isRunning: true);
			break;
		case 48:
			setRunning(isRunning: false);
			break;
		}
		if ((command & 0x40) == 64)
		{
			Halt();
			running = false;
		}
	}

	public void toolPowerIncrease()
	{
		if (CurrentTool is Pan)
		{
			return;
		}
		if ((int)toolPower == 0)
		{
			toolPitchAccumulator = 0;
		}
		toolPower.Value++;
		if (CurrentTool is Pickaxe && (int)toolPower == 1)
		{
			toolPower.Value += 2;
		}
		Color powerUpColor = Color.White;
		int frameOffset = ((FacingDirection == 0) ? 4 : ((FacingDirection == 2) ? 2 : 0));
		switch ((long)toolPower.Value)
		{
		case 1L:
			powerUpColor = Color.Orange;
			if (!(CurrentTool is WateringCan))
			{
				FarmerSprite.CurrentFrame = 72 + frameOffset;
			}
			jitterStrength = 0.25f;
			break;
		case 2L:
			powerUpColor = Color.LightSteelBlue;
			if (!(CurrentTool is WateringCan))
			{
				FarmerSprite.CurrentFrame++;
			}
			jitterStrength = 0.5f;
			break;
		case 3L:
			powerUpColor = Color.Gold;
			jitterStrength = 1f;
			break;
		case 4L:
			powerUpColor = Color.Violet;
			jitterStrength = 2f;
			break;
		case 5L:
			powerUpColor = Color.BlueViolet;
			jitterStrength = 3f;
			break;
		}
		int xAnimation = ((FacingDirection == 1) ? 40 : ((FacingDirection == 3) ? (-40) : ((FacingDirection == 2) ? 32 : 0)));
		int yAnimation = 192;
		if (CurrentTool is WateringCan)
		{
			switch (FacingDirection)
			{
			case 3:
				xAnimation = 48;
				break;
			case 1:
				xAnimation = -48;
				break;
			case 2:
				xAnimation = 0;
				break;
			}
			yAnimation = 128;
		}
		int standingY = base.StandingPixel.Y;
		Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(21, base.Position - new Vector2(xAnimation, yAnimation), powerUpColor, 8, flipped: false, 70f, 0, 64, (float)standingY / 10000f + 0.005f, 128));
		Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(192, 1152, 64, 64), 50f, 4, 0, base.Position - new Vector2((FacingDirection != 1) ? (-64) : 0, 128f), flicker: false, FacingDirection == 1, (float)standingY / 10000f, 0.01f, Color.White, 1f, 0f, 0f, 0f));
		int pitch = Utility.CreateRandom(Game1.dayOfMonth, (double)base.Position.X * 1000.0, base.Position.Y).Next(12, 16) * 100 + (int)toolPower * 100;
		Game1.playSound("toolCharge", pitch);
	}

	public void UpdateIfOtherPlayer(GameTime time)
	{
		if (base.currentLocation == null)
		{
			return;
		}
		position.UpdateExtrapolation(getMovementSpeed());
		position.Field.InterpolationEnabled = !currentLocationRef.IsChanging();
		if (Game1.ShouldShowOnscreenUsernames() && Game1.mouseCursorTransparency > 0f && base.currentLocation == Game1.currentLocation && Game1.currentMinigame == null && Game1.activeClickableMenu == null)
		{
			Vector2 local_position = getLocalPosition(Game1.viewport);
			Microsoft.Xna.Framework.Rectangle bounding_rect = new Microsoft.Xna.Framework.Rectangle(0, 0, 128, 192);
			bounding_rect.X = (int)(local_position.X + 32f - (float)(bounding_rect.Width / 2));
			bounding_rect.Y = (int)(local_position.Y - (float)bounding_rect.Height + 48f);
			if (bounding_rect.Contains(Game1.getMouseX(ui_scale: false), Game1.getMouseY(ui_scale: false)))
			{
				usernameDisplayTime = 1f;
			}
		}
		if (_lastSelectedItem != CurrentItem)
		{
			_lastSelectedItem?.actionWhenStopBeingHeld(this);
			_lastSelectedItem = CurrentItem;
		}
		fireToolEvent.Poll();
		beginUsingToolEvent.Poll();
		endUsingToolEvent.Poll();
		drinkAnimationEvent.Poll();
		eatAnimationEvent.Poll();
		sickAnimationEvent.Poll();
		passOutEvent.Poll();
		doEmoteEvent.Poll();
		kissFarmerEvent.Poll();
		haltAnimationEvent.Poll();
		synchronizedJumpEvent.Poll();
		renovateEvent.Poll();
		FarmerSprite.checkForSingleAnimation(time);
		updateCommon(time, base.currentLocation);
	}

	/// <summary>Put an item into an equipment slot with appropriate updates (e.g. calling <see cref="M:StardewValley.Item.onEquip(StardewValley.Farmer)" /> or <see cref="M:StardewValley.Item.onUnequip(StardewValley.Farmer)" />).</summary>
	/// <typeparam name="TItem">The item type.</typeparam>
	/// <param name="newItem">The item to place in the equipment slot, or <c>null</c> to just unequip the old item.</param>
	/// <param name="slot">The equipment slot to update.</param>
	/// <returns>Returns the item that was previously in the equipment slot, or <c>null</c> if it was empty.</returns>
	public TItem Equip<TItem>(TItem newItem, NetRef<TItem> slot) where TItem : Item
	{
		TItem oldItem = slot.Value;
		oldItem?.onDetachedFromParent();
		newItem?.onDetachedFromParent();
		Equip(oldItem, newItem, delegate(TItem val)
		{
			slot.Value = val;
		});
		return oldItem;
	}

	/// <summary>Place an item into an equipment slot manually with appropriate updates (e.g. calling <see cref="M:StardewValley.Item.onEquip(StardewValley.Farmer)" /> or <see cref="M:StardewValley.Item.onUnequip(StardewValley.Farmer)" />).</summary>
	/// <typeparam name="TItem">The item type.</typeparam>
	/// <param name="oldItem">The item previously in the equipment slot, or <c>null</c> if it was empty.</param>
	/// <param name="newItem">The item to place in the equipment slot, or <c>null</c> to just unequip the old item.</param>
	/// <param name="equip">A callback which equips an item in the slot.</param>
	/// <remarks>Most code should use <see cref="M:StardewValley.Farmer.Equip``1(``0,Netcode.NetRef{``0})" /> instead. When calling this form, you should call <see cref="M:StardewValley.Item.onDetachedFromParent" /> on the old/new items as needed to avoid warnings.</remarks>
	public void Equip<TItem>(TItem oldItem, TItem newItem, Action<TItem> equip) where TItem : Item
	{
		bool raiseEvents = Game1.hasLoadedGame && Game1.dayOfMonth > 0 && IsLocalPlayer;
		if (raiseEvents)
		{
			oldItem?.onUnequip(this);
		}
		equip(newItem);
		if (newItem != null)
		{
			newItem.HasBeenInInventory = true;
			if (raiseEvents)
			{
				newItem.onEquip(this);
			}
		}
		if ((oldItem?.HasEquipmentBuffs() ?? false) || !((!(newItem?.HasEquipmentBuffs())) ?? true))
		{
			buffs.Dirty = true;
		}
	}

	public void forceCanMove()
	{
		forceTimePass = false;
		movementDirections.Clear();
		isEating = false;
		CanMove = true;
		Game1.freezeControls = false;
		freezePause = 0;
		UsingTool = false;
		usingSlingshot = false;
		FarmerSprite.PauseForSingleAnimation = false;
		if (CurrentTool is FishingRod rod)
		{
			rod.isFishing = false;
		}
	}

	public void dropItem(Item i)
	{
		if (i != null && i.canBeDropped())
		{
			Game1.createItemDebris(i.getOne(), getStandingPosition(), FacingDirection);
		}
	}

	public bool addEvent(string eventName, int daysActive)
	{
		return activeDialogueEvents.TryAdd(eventName, daysActive);
	}

	public Vector2 getMostRecentMovementVector()
	{
		return new Vector2(base.Position.X - lastPosition.X, base.Position.Y - lastPosition.Y);
	}

	public int GetSkillLevel(int index)
	{
		return index switch
		{
			0 => FarmingLevel, 
			3 => MiningLevel, 
			1 => FishingLevel, 
			2 => ForagingLevel, 
			5 => LuckLevel, 
			4 => CombatLevel, 
			_ => 0, 
		};
	}

	public int GetUnmodifiedSkillLevel(int index)
	{
		return index switch
		{
			0 => farmingLevel.Value, 
			3 => miningLevel.Value, 
			1 => fishingLevel.Value, 
			2 => foragingLevel.Value, 
			5 => luckLevel.Value, 
			4 => combatLevel.Value, 
			_ => 0, 
		};
	}

	public static string getSkillNameFromIndex(int index)
	{
		return index switch
		{
			0 => "Farming", 
			3 => "Mining", 
			1 => "Fishing", 
			2 => "Foraging", 
			5 => "Luck", 
			4 => "Combat", 
			_ => "", 
		};
	}

	public static int getSkillNumberFromName(string name)
	{
		return name.ToLower() switch
		{
			"farming" => 0, 
			"mining" => 3, 
			"fishing" => 1, 
			"foraging" => 2, 
			"luck" => 5, 
			"combat" => 4, 
			_ => -1, 
		};
	}

	public bool setSkillLevel(string nameOfSkill, int level)
	{
		int skillIndex = getSkillNumberFromName(nameOfSkill);
		switch (nameOfSkill)
		{
		case "Farming":
			if (farmingLevel.Value < level)
			{
				newLevels.Add(new Point(skillIndex, level - farmingLevel.Value));
				farmingLevel.Value = level;
				experiencePoints[skillIndex] = getBaseExperienceForLevel(level);
				return true;
			}
			break;
		case "Fishing":
			if (fishingLevel.Value < level)
			{
				newLevels.Add(new Point(skillIndex, level - fishingLevel.Value));
				fishingLevel.Value = level;
				experiencePoints[skillIndex] = getBaseExperienceForLevel(level);
				return true;
			}
			break;
		case "Foraging":
			if (foragingLevel.Value < level)
			{
				newLevels.Add(new Point(skillIndex, level - foragingLevel.Value));
				foragingLevel.Value = level;
				experiencePoints[skillIndex] = getBaseExperienceForLevel(level);
				return true;
			}
			break;
		case "Mining":
			if (miningLevel.Value < level)
			{
				newLevels.Add(new Point(skillIndex, level - miningLevel.Value));
				miningLevel.Value = level;
				experiencePoints[skillIndex] = getBaseExperienceForLevel(level);
				return true;
			}
			break;
		case "Combat":
			if (combatLevel.Value < level)
			{
				newLevels.Add(new Point(skillIndex, level - combatLevel.Value));
				combatLevel.Value = level;
				experiencePoints[skillIndex] = getBaseExperienceForLevel(level);
				return true;
			}
			break;
		}
		return false;
	}

	public static string getSkillDisplayNameFromIndex(int index)
	{
		return index switch
		{
			0 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1991"), 
			3 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1992"), 
			1 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1993"), 
			2 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1994"), 
			5 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1995"), 
			4 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1996"), 
			_ => "", 
		};
	}

	public bool hasCompletedCommunityCenter()
	{
		if (mailReceived.Contains("ccBoilerRoom") && mailReceived.Contains("ccCraftsRoom") && mailReceived.Contains("ccPantry") && mailReceived.Contains("ccFishTank") && mailReceived.Contains("ccVault"))
		{
			return mailReceived.Contains("ccBulletin");
		}
		return false;
	}

	private bool localBusMoving()
	{
		GameLocation gameLocation = base.currentLocation;
		if (!(gameLocation is Desert desert))
		{
			if (gameLocation is BusStop busStop)
			{
				if (!busStop.drivingOff)
				{
					return busStop.drivingBack;
				}
				return true;
			}
			return false;
		}
		if (!desert.drivingOff)
		{
			return desert.drivingBack;
		}
		return true;
	}

	public virtual bool CanBeDamaged()
	{
		if (!temporarilyInvincible && !isEating && !Game1.fadeToBlack)
		{
			return !hasBuff("21");
		}
		return false;
	}

	public void takeDamage(int damage, bool overrideParry, Monster damager)
	{
		if (Game1.eventUp || FarmerSprite.isPassingOut() || (isInBed.Value && Game1.activeClickableMenu != null && Game1.activeClickableMenu is ReadyCheckDialog))
		{
			return;
		}
		bool num = damager != null && !damager.isInvincible() && !overrideParry;
		bool monsterDamageCapable = (damager == null || !damager.isInvincible()) && (damager == null || (!(damager is GreenSlime) && !(damager is BigSlime)) || !isWearingRing("520"));
		bool playerParryable = CurrentTool is MeleeWeapon && ((MeleeWeapon)CurrentTool).isOnSpecial && (int)((MeleeWeapon)CurrentTool).type == 3;
		bool playerDamageable = CanBeDamaged();
		if (num && playerParryable)
		{
			Rumble.rumble(0.75f, 150f);
			playNearbySoundAll("parry");
			damager.parried(damage, this);
		}
		else
		{
			if (!(monsterDamageCapable && playerDamageable))
			{
				return;
			}
			damager?.onDealContactDamage(this);
			damage += Game1.random.Next(Math.Min(-1, -damage / 8), Math.Max(1, damage / 8));
			int defense = buffs.Defense;
			if (stats.Get("Book_Defense") != 0)
			{
				defense++;
			}
			if ((float)defense >= (float)damage * 0.5f)
			{
				defense -= (int)((float)defense * (float)Game1.random.Next(3) / 10f);
			}
			if (damager != null && isWearingRing("839"))
			{
				Microsoft.Xna.Framework.Rectangle monsterBox = damager.GetBoundingBox();
				Vector2 trajectory = Utility.getAwayFromPlayerTrajectory(monsterBox, this);
				trajectory /= 2f;
				int damageToMonster = damage;
				int farmerDamage = Math.Max(1, damage - defense);
				if (farmerDamage < 10)
				{
					damageToMonster = (int)Math.Ceiling((double)(damageToMonster + farmerDamage) / 2.0);
				}
				damager.takeDamage(damageToMonster, (int)trajectory.X, (int)trajectory.Y, isBomb: false, 1.0, this);
				damager.currentLocation.debris.Add(new Debris(damageToMonster, new Vector2(monsterBox.Center.X + 16, monsterBox.Center.Y), new Color(255, 130, 0), 1f, damager));
			}
			if (isWearingRing("524") && !hasBuff("21") && Game1.random.NextDouble() < (0.9 - (double)((float)health / 100f)) / (double)(3 - LuckLevel / 10) + ((health <= 15) ? 0.2 : 0.0))
			{
				playNearbySoundAll("yoba");
				applyBuff("21");
				return;
			}
			Rumble.rumble(0.75f, 150f);
			damage = Math.Max(1, damage - defense);
			if (Utility.GetDayOfPassiveFestival("DesertFestival") > 0 && base.currentLocation is MineShaft && Game1.mine.getMineArea() == 121)
			{
				float adjustment = 1f;
				if (team.calicoStatueEffects.TryGetValue(8, out var sharpTeethAmount))
				{
					adjustment += (float)sharpTeethAmount * 0.25f;
				}
				if (team.calicoStatueEffects.TryGetValue(14, out var toothFileAmount))
				{
					adjustment -= (float)toothFileAmount * 0.25f;
				}
				damage = Math.Max(1, (int)((float)damage * adjustment));
			}
			health = Math.Max(0, health - damage);
			foreach (Trinket trinketItem in trinketItems)
			{
				trinketItem?.OnReceiveDamage(this, damage);
			}
			if (health <= 0 && GetEffectsOfRingMultiplier("863") > 0 && !hasUsedDailyRevive.Value)
			{
				startGlowing(new Color(255, 255, 0), border: false, 0.25f);
				DelayedAction.functionAfterDelay(base.stopGlowing, 500);
				Game1.playSound("yoba");
				for (int i = 0; i < 13; i++)
				{
					float xPos = Game1.random.Next(-32, 33);
					base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(114, 46, 2, 2), 200f, 5, 1, new Vector2(xPos + 32f, -96f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						attachedCharacter = this,
						positionFollowsAttachedCharacter = true,
						motion = new Vector2(xPos / 32f, -3f),
						delayBeforeAnimationStart = i * 50,
						alphaFade = 0.001f,
						acceleration = new Vector2(0f, 0.1f)
					});
				}
				base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(157, 280, 28, 19), 2000f, 1, 1, new Vector2(-20f, -16f), flicker: false, flipped: false, 1E-06f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					attachedCharacter = this,
					positionFollowsAttachedCharacter = true,
					alpha = 0.1f,
					alphaFade = -0.01f,
					alphaFadeFade = -0.00025f
				});
				health = (int)Math.Min(maxHealth, (float)maxHealth * 0.5f + (float)GetEffectsOfRingMultiplier("863"));
				hasUsedDailyRevive.Value = true;
			}
			temporarilyInvincible = true;
			flashDuringThisTemporaryInvincibility = true;
			temporaryInvincibilityTimer = 0;
			currentTemporaryInvincibilityDuration = 1200 + GetEffectsOfRingMultiplier("861") * 400;
			Point standingPixel = base.StandingPixel;
			base.currentLocation.debris.Add(new Debris(damage, new Vector2(standingPixel.X + 8, standingPixel.Y), Color.Red, 1f, this));
			playNearbySoundAll("ow");
			Game1.hitShakeTimer = 100 * damage;
		}
	}

	public int GetEffectsOfRingMultiplier(string ringId)
	{
		int count = 0;
		if (leftRing.Value != null)
		{
			count += leftRing.Value.GetEffectsOfRingMultiplier(ringId);
		}
		if (rightRing.Value != null)
		{
			count += rightRing.Value.GetEffectsOfRingMultiplier(ringId);
		}
		return count;
	}

	private void checkDamage(GameLocation location)
	{
		if (Game1.eventUp)
		{
			return;
		}
		for (int i = location.characters.Count - 1; i >= 0; i--)
		{
			if (i < location.characters.Count && location.characters[i] is Monster monster && monster.OverlapsFarmerForDamage(this))
			{
				monster.currentLocation = location;
				monster.collisionWithFarmerBehavior();
				if (monster.DamageToFarmer > 0)
				{
					if (CurrentTool is MeleeWeapon && ((MeleeWeapon)CurrentTool).isOnSpecial && (int)((MeleeWeapon)CurrentTool).type == 3)
					{
						takeDamage(monster.DamageToFarmer, overrideParry: false, monster);
					}
					else
					{
						takeDamage(Math.Max(1, monster.DamageToFarmer + Game1.random.Next(-monster.DamageToFarmer / 4, monster.DamageToFarmer / 4)), overrideParry: false, monster);
					}
				}
			}
		}
	}

	public bool checkAction(Farmer who, GameLocation location)
	{
		if (who.isRidingHorse())
		{
			who.Halt();
		}
		if ((bool)hidden)
		{
			return false;
		}
		if (Game1.CurrentEvent != null)
		{
			if (Game1.CurrentEvent.isSpecificFestival("spring24") && who.dancePartner.Value == null)
			{
				who.Halt();
				who.faceGeneralDirection(getStandingPosition(), 0, opposite: false, useTileCalculations: false);
				string question = Game1.content.LoadString("Strings\\UI:AskToDance_" + (IsMale ? "Male" : "Female"), base.Name);
				location.createQuestionDialogue(question, location.createYesNoResponses(), delegate(Farmer _, string answer)
				{
					if (answer == "Yes")
					{
						who.team.SendProposal(this, ProposalType.Dance);
						Game1.activeClickableMenu = new PendingProposalDialog();
					}
				});
				return true;
			}
			return false;
		}
		if (who.CurrentItem != null && who.CurrentItem.QualifiedItemId == "(O)801" && !isMarriedOrRoommates() && !isEngaged() && !who.isMarriedOrRoommates() && !who.isEngaged())
		{
			who.Halt();
			who.faceGeneralDirection(getStandingPosition(), 0, opposite: false, useTileCalculations: false);
			string question = Game1.content.LoadString("Strings\\UI:AskToMarry_" + (IsMale ? "Male" : "Female"), base.Name);
			location.createQuestionDialogue(question, location.createYesNoResponses(), delegate(Farmer _, string answer)
			{
				if (answer == "Yes")
				{
					who.team.SendProposal(this, ProposalType.Marriage, who.CurrentItem.getOne());
					Game1.activeClickableMenu = new PendingProposalDialog();
				}
			});
			return true;
		}
		if (who.CanMove)
		{
			bool? flag = who.ActiveObject?.canBeGivenAsGift();
			if (flag.HasValue && flag.GetValueOrDefault() && !who.ActiveObject.questItem)
			{
				who.Halt();
				who.faceGeneralDirection(getStandingPosition(), 0, opposite: false, useTileCalculations: false);
				string question = Game1.content.LoadString("Strings\\UI:GiftPlayerItem_" + (IsMale ? "Male" : "Female"), who.ActiveObject.DisplayName, base.Name);
				location.createQuestionDialogue(question, location.createYesNoResponses(), delegate(Farmer _, string answer)
				{
					if (answer == "Yes")
					{
						who.team.SendProposal(this, ProposalType.Gift, who.ActiveObject.getOne());
						Game1.activeClickableMenu = new PendingProposalDialog();
					}
				});
				return true;
			}
		}
		long? playerSpouseID = team.GetSpouse(UniqueMultiplayerID);
		if ((playerSpouseID.HasValue & (who.UniqueMultiplayerID == playerSpouseID)) && who.CanMove && !who.isMoving() && !isMoving() && Utility.IsHorizontalDirection(getGeneralDirectionTowards(who.getStandingPosition(), -10, opposite: false, useTileCalculations: false)))
		{
			who.Halt();
			who.faceGeneralDirection(getStandingPosition(), 0, opposite: false, useTileCalculations: false);
			who.kissFarmerEvent.Fire(UniqueMultiplayerID);
			Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(211, 428, 7, 6), 2000f, 1, 0, base.Tile * 64f + new Vector2(16f, -64f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
			{
				motion = new Vector2(0f, -0.5f),
				alphaFade = 0.01f
			});
			playNearbySoundAll("dwop", null, SoundContext.NPC);
			return true;
		}
		return false;
	}

	public void Update(GameTime time, GameLocation location)
	{
		if (_lastEquippedTool != CurrentTool)
		{
			Equip(_lastEquippedTool, CurrentTool, delegate(Tool tool)
			{
				_lastEquippedTool = tool;
			});
		}
		buffs.SetOwner(this);
		buffs.Update(time);
		position.UpdateExtrapolation(getMovementSpeed());
		fireToolEvent.Poll();
		beginUsingToolEvent.Poll();
		endUsingToolEvent.Poll();
		drinkAnimationEvent.Poll();
		eatAnimationEvent.Poll();
		sickAnimationEvent.Poll();
		passOutEvent.Poll();
		doEmoteEvent.Poll();
		kissFarmerEvent.Poll();
		synchronizedJumpEvent.Poll();
		renovateEvent.Poll();
		if (IsLocalPlayer)
		{
			if (base.currentLocation == null)
			{
				return;
			}
			hidden.Value = localBusMoving() || (location.currentEvent != null && !location.currentEvent.isFestival) || (location.currentEvent != null && location.currentEvent.doingSecretSanta) || Game1.locationRequest != null || !Game1.displayFarmer;
			isInBed.Value = base.currentLocation.doesTileHaveProperty(base.TilePoint.X, base.TilePoint.Y, "Bed", "Back") != null || (bool)sleptInTemporaryBed;
			if (!Game1.options.allowStowing)
			{
				netItemStowed.Value = false;
			}
			hasMenuOpen.Value = Game1.activeClickableMenu != null;
		}
		if (IsSitting())
		{
			movementDirections.Clear();
			if (IsSitting() && !isStopSitting)
			{
				if (!sittingFurniture.IsSeatHere(base.currentLocation))
				{
					StopSitting(animate: false);
				}
				else if (sittingFurniture is MapSeat mapSeat)
				{
					if (!base.currentLocation.mapSeats.Contains(sittingFurniture))
					{
						StopSitting(animate: false);
					}
					else if (mapSeat.IsBlocked(base.currentLocation))
					{
						StopSitting();
					}
				}
			}
		}
		if (Game1.CurrentEvent == null && !bathingClothes && !onBridge.Value)
		{
			canOnlyWalk = false;
		}
		if (noMovementPause > 0)
		{
			CanMove = false;
			noMovementPause -= time.ElapsedGameTime.Milliseconds;
			if (noMovementPause <= 0)
			{
				CanMove = true;
			}
		}
		if (freezePause > 0)
		{
			CanMove = false;
			freezePause -= time.ElapsedGameTime.Milliseconds;
			if (freezePause <= 0)
			{
				CanMove = true;
			}
		}
		if (sparklingText != null && sparklingText.update(time))
		{
			sparklingText = null;
		}
		if (newLevelSparklingTexts.Count > 0 && sparklingText == null && !UsingTool && CanMove && Game1.activeClickableMenu == null)
		{
			sparklingText = new SparklingText(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2003", getSkillDisplayNameFromIndex(newLevelSparklingTexts.Peek())), Color.White, Color.White, rainbow: true);
			newLevelSparklingTexts.Dequeue();
		}
		if (lerpPosition >= 0f)
		{
			lerpPosition += (float)time.ElapsedGameTime.TotalSeconds;
			if (lerpPosition >= lerpDuration)
			{
				lerpPosition = lerpDuration;
			}
			base.Position = new Vector2(Utility.Lerp(lerpStartPosition.X, lerpEndPosition.X, lerpPosition / lerpDuration), Utility.Lerp(lerpStartPosition.Y, lerpEndPosition.Y, lerpPosition / lerpDuration));
			if (lerpPosition >= lerpDuration)
			{
				lerpPosition = -1f;
			}
		}
		if (isStopSitting && lerpPosition < 0f)
		{
			isStopSitting = false;
			if (sittingFurniture != null)
			{
				mapChairSitPosition.Value = new Vector2(-1f, -1f);
				sittingFurniture.RemoveSittingFarmer(this);
				sittingFurniture = null;
				isSitting.Value = false;
			}
		}
		if ((bool)isInBed && Game1.IsMultiplayer && Game1.shouldTimePass())
		{
			regenTimer -= time.ElapsedGameTime.Milliseconds;
			if (regenTimer < 0)
			{
				regenTimer = 500;
				if (stamina < (float)MaxStamina)
				{
					stamina++;
				}
				if (health < maxHealth)
				{
					health++;
				}
			}
		}
		FarmerSprite.checkForSingleAnimation(time);
		if (CanMove)
		{
			rotation = 0f;
			if (health <= 0 && !Game1.killScreen && Game1.timeOfDay < 2600)
			{
				if (IsSitting())
				{
					StopSitting(animate: false);
				}
				CanMove = false;
				Game1.screenGlowOnce(Color.Red, hold: true);
				Game1.killScreen = true;
				faceDirection(2);
				FarmerSprite.setCurrentFrame(5);
				jitterStrength = 1f;
				Game1.pauseTime = 3000f;
				Rumble.rumbleAndFade(0.75f, 1500f);
				freezePause = 8000;
				if (Game1.currentSong != null && Game1.currentSong.IsPlaying)
				{
					Game1.currentSong.Stop(AudioStopOptions.Immediate);
				}
				Game1.changeMusicTrack("silence");
				playNearbySoundAll("death");
				Game1.dialogueUp = false;
				Game1.stats.TimesUnconscious++;
				if (Utility.GetDayOfPassiveFestival("DesertFestival") > 0 && Game1.player.currentLocation is MineShaft && Game1.mine.getMineArea() == 121)
				{
					int eggsRemoved = 0;
					float eggPercentToRemove = 0.2f;
					if (Game1.player.team.calicoStatueEffects.ContainsKey(5))
					{
						eggPercentToRemove = 0.5f;
					}
					eggsRemoved = (int)(eggPercentToRemove * (float)Game1.player.getItemCount("CalicoEgg"));
					Game1.player.Items.ReduceId("CalicoEgg", eggsRemoved);
					itemsLostLastDeath.Clear();
					if (eggsRemoved > 0)
					{
						itemsLostLastDeath.Add(new Object("CalicoEgg", eggsRemoved));
					}
				}
				if (Game1.activeClickableMenu is GameMenu)
				{
					Game1.activeClickableMenu.emergencyShutDown();
					Game1.activeClickableMenu = null;
				}
			}
			if (collisionNPC != null)
			{
				collisionNPC.farmerPassesThrough = true;
			}
			NPC collider;
			if (movementDirections.Count > 0 && !isRidingHorse() && (collider = location.isCollidingWithCharacter(nextPosition(FacingDirection))) != null)
			{
				charactercollisionTimer += time.ElapsedGameTime.Milliseconds;
				if (charactercollisionTimer > collider.getTimeFarmerMustPushBeforeStartShaking())
				{
					collider.shake(50);
				}
				if (charactercollisionTimer >= collider.getTimeFarmerMustPushBeforePassingThrough() && collisionNPC == null)
				{
					collisionNPC = collider;
					if (collisionNPC.Name.Equals("Bouncer") && base.currentLocation != null && base.currentLocation.name.Equals("SandyHouse"))
					{
						collisionNPC.showTextAboveHead(Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2010"));
						collisionNPC = null;
						charactercollisionTimer = 0;
					}
					else if (collisionNPC.name.Equals("Henchman") && base.currentLocation != null && base.currentLocation.name.Equals("WitchSwamp"))
					{
						collisionNPC = null;
						charactercollisionTimer = 0;
					}
					else if (collisionNPC is Raccoon)
					{
						collisionNPC = null;
						charactercollisionTimer = 0;
					}
				}
			}
			else
			{
				charactercollisionTimer = 0;
				if (collisionNPC != null && location.isCollidingWithCharacter(nextPosition(FacingDirection)) == null)
				{
					collisionNPC.farmerPassesThrough = false;
					collisionNPC = null;
				}
			}
		}
		if (Game1.shouldTimePass())
		{
			MeleeWeapon.weaponsTypeUpdate(time);
		}
		if (!Game1.eventUp || movementDirections.Count <= 0 || base.currentLocation.currentEvent == null || base.currentLocation.currentEvent.playerControlSequence || (controller != null && controller.allowPlayerPathingInEvent))
		{
			lastPosition = base.Position;
			if (controller != null)
			{
				if (controller.update(time))
				{
					controller = null;
				}
			}
			else if (controller == null)
			{
				MovePosition(time, Game1.viewport, location);
			}
		}
		if (Game1.actionsWhenPlayerFree.Count > 0 && IsLocalPlayer && !IsBusyDoingSomething())
		{
			Action action = Game1.actionsWhenPlayerFree[0];
			Game1.actionsWhenPlayerFree.RemoveAt(0);
			action();
		}
		updateCommon(time, location);
		position.Paused = FarmerSprite.PauseForSingleAnimation || (UsingTool && !canStrafeForToolUse()) || isEating;
		checkDamage(location);
	}

	private void updateCommon(GameTime time, GameLocation location)
	{
		if (usernameDisplayTime > 0f)
		{
			usernameDisplayTime -= (float)time.ElapsedGameTime.TotalSeconds;
			if (usernameDisplayTime < 0f)
			{
				usernameDisplayTime = 0f;
			}
		}
		if (jitterStrength > 0f)
		{
			jitter = new Vector2((float)Game1.random.Next(-(int)(jitterStrength * 100f), (int)((jitterStrength + 1f) * 100f)) / 100f, (float)Game1.random.Next(-(int)(jitterStrength * 100f), (int)((jitterStrength + 1f) * 100f)) / 100f);
		}
		if (_wasSitting != isSitting.Value)
		{
			if (_wasSitting)
			{
				yOffset = 0f;
				xOffset = 0f;
			}
			_wasSitting = isSitting.Value;
		}
		if (yJumpOffset != 0)
		{
			yJumpVelocity -= ((UsingTool && canStrafeForToolUse() && (movementDirections.Count > 0 || (!IsLocalPlayer && IsRemoteMoving()))) ? 0.25f : 0.5f);
			yJumpOffset -= (int)yJumpVelocity;
			if (yJumpOffset >= 0)
			{
				yJumpOffset = 0;
				yJumpVelocity = 0f;
			}
		}
		updateMovementAnimation(time);
		updateEmote(time);
		updateGlow();
		currentLocationRef.Update();
		if ((bool)exhausted && stamina <= 1f)
		{
			currentEyes = 4;
			blinkTimer = -1000;
		}
		blinkTimer += time.ElapsedGameTime.Milliseconds;
		if (blinkTimer > 2200 && Game1.random.NextDouble() < 0.01)
		{
			blinkTimer = -150;
			currentEyes = 4;
		}
		else if (blinkTimer > -100)
		{
			if (blinkTimer < -50)
			{
				currentEyes = 1;
			}
			else if (blinkTimer < 0)
			{
				currentEyes = 4;
			}
			else
			{
				currentEyes = 0;
			}
		}
		if (isCustomized.Value && isInBed.Value && !Game1.eventUp && ((timerSinceLastMovement >= 3000 && Game1.timeOfDay >= 630) || timeWentToBed.Value != 0))
		{
			currentEyes = 1;
			blinkTimer = -10;
		}
		UpdateItemStow();
		if ((bool)swimming)
		{
			yOffset = (float)(Math.Cos(time.TotalGameTime.TotalMilliseconds / 2000.0) * 4.0);
			int oldSwimTimer = swimTimer;
			swimTimer -= time.ElapsedGameTime.Milliseconds;
			if (timerSinceLastMovement == 0)
			{
				if (oldSwimTimer > 400 && swimTimer <= 400 && IsLocalPlayer)
				{
					Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), 150f - (Math.Abs(xVelocity) + Math.Abs(yVelocity)) * 3f, 8, 0, new Vector2(base.Position.X, base.StandingPixel.Y - 32), flicker: false, Game1.random.NextBool(), 0.01f, 0.01f, Color.White, 1f, 0.003f, 0f, 0f));
				}
				if (swimTimer < 0)
				{
					swimTimer = 800;
					if (IsLocalPlayer)
					{
						playNearbySoundAll("slosh");
						Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), 150f - (Math.Abs(xVelocity) + Math.Abs(yVelocity)) * 3f, 8, 0, new Vector2(base.Position.X, base.StandingPixel.Y - 32), flicker: false, Game1.random.NextBool(), 0.01f, 0.01f, Color.White, 1f, 0.003f, 0f, 0f));
					}
				}
			}
			else if (!Game1.eventUp && (Game1.activeClickableMenu == null || Game1.IsMultiplayer) && !Game1.paused)
			{
				if (timerSinceLastMovement > 800)
				{
					currentEyes = 1;
				}
				else if (timerSinceLastMovement > 700)
				{
					currentEyes = 4;
				}
				if (swimTimer < 0)
				{
					swimTimer = 100;
					if (stamina < (float)(int)maxStamina)
					{
						stamina++;
					}
					if (health < maxHealth)
					{
						health++;
					}
				}
			}
		}
		if (!isMoving())
		{
			timerSinceLastMovement += time.ElapsedGameTime.Milliseconds;
		}
		else
		{
			timerSinceLastMovement = 0;
		}
		for (int i = Items.Count - 1; i >= 0; i--)
		{
			if (Items[i] is Tool tool)
			{
				tool.tickUpdate(time, this);
			}
		}
		if (TemporaryItem is Tool tempTool)
		{
			tempTool.tickUpdate(time, this);
		}
		rightRing.Value?.update(time, location, this);
		leftRing.Value?.update(time, location, this);
		if (Game1.shouldTimePass() && IsLocalPlayer)
		{
			foreach (Trinket trinketItem in trinketItems)
			{
				trinketItem?.Update(this, time, location);
			}
		}
		mount?.update(time, location);
		mount?.SyncPositionToRider();
		foreach (Companion companion in companions)
		{
			companion.Update(time, location);
		}
	}

	/// <summary>Get whether the player is engaged in any action and shouldn't be interrupted. This includes viewing a menu or event, fading to black, warping, using a tool, etc. If this returns false, we should be free to interrupt the player.</summary>
	public virtual bool IsBusyDoingSomething()
	{
		if (Game1.eventUp)
		{
			return true;
		}
		if (Game1.fadeToBlack)
		{
			return true;
		}
		if (Game1.currentMinigame != null)
		{
			return true;
		}
		if (Game1.activeClickableMenu != null)
		{
			return true;
		}
		if (Game1.isWarping)
		{
			return true;
		}
		if (UsingTool)
		{
			return true;
		}
		if (Game1.killScreen)
		{
			return true;
		}
		if (freezePause > 0)
		{
			return true;
		}
		if (!CanMove)
		{
			return true;
		}
		if (FarmerSprite.PauseForSingleAnimation)
		{
			return true;
		}
		if (usingSlingshot)
		{
			return true;
		}
		return false;
	}

	public void UpdateItemStow()
	{
		if (_itemStowed != netItemStowed.Value)
		{
			if (netItemStowed.Value && ActiveObject != null)
			{
				ActiveObject.actionWhenStopBeingHeld(this);
			}
			_itemStowed = netItemStowed.Value;
			if (!netItemStowed.Value)
			{
				ActiveObject?.actionWhenBeingHeld(this);
			}
		}
	}

	/// <summary>Add a quest to the player's quest log, or log a warning if it doesn't exist.</summary>
	/// <param name="questId">The quest ID in <c>Data/Quests</c>.</param>
	public void addQuest(string questId)
	{
		if (hasQuest(questId))
		{
			return;
		}
		Quest quest = Quest.getQuestFromId(questId);
		if (quest == null)
		{
			Game1.log.Warn("Can't add quest with ID '" + questId + "' because no such ID was found.");
			return;
		}
		questLog.Add(quest);
		if (!quest.IsHidden())
		{
			Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2011"), 2));
		}
		if (quest.questType.Value == 8 && Game1.player.team.constructedBuildings.Contains(quest.completionString.Value))
		{
			quest.questComplete();
		}
	}

	public void removeQuest(string questID)
	{
		for (int i = questLog.Count - 1; i >= 0; i--)
		{
			if (questLog[i].id.Value == questID)
			{
				questLog.RemoveAt(i);
			}
		}
	}

	public void completeQuest(string questID)
	{
		for (int i = questLog.Count - 1; i >= 0; i--)
		{
			if (questLog[i].id.Value == questID)
			{
				questLog[i].questComplete();
			}
		}
	}

	public bool hasQuest(string id)
	{
		for (int i = questLog.Count - 1; i >= 0; i--)
		{
			if (questLog[i].id.Value == id)
			{
				return true;
			}
		}
		return false;
	}

	public bool hasNewQuestActivity()
	{
		foreach (SpecialOrder o in team.specialOrders)
		{
			if (!o.IsHidden() && (o.ShouldDisplayAsNew() || o.ShouldDisplayAsComplete()))
			{
				return true;
			}
		}
		foreach (Quest q in questLog)
		{
			if (!q.IsHidden() && ((bool)q.showNew || ((bool)q.completed && !q.destroy)))
			{
				return true;
			}
		}
		return false;
	}

	public float getMovementSpeed()
	{
		if (UsingTool && canStrafeForToolUse())
		{
			return 2f;
		}
		float movementSpeed;
		if (Game1.CurrentEvent == null || Game1.CurrentEvent.playerControlSequence)
		{
			movementMultiplier = 0.066f;
			movementSpeed = 1f;
			movementSpeed = ((!isRidingHorse()) ? Math.Max(1f, ((float)base.speed + (Game1.eventUp ? 0f : (addedSpeed + temporarySpeedBuff))) * movementMultiplier * (float)Game1.currentGameTime.ElapsedGameTime.Milliseconds) : Math.Max(1f, ((float)base.speed + (Game1.eventUp ? 0f : (addedSpeed + 4.6f + (mount.ateCarrotToday ? 0.4f : 0f) + ((stats.Get("Book_Horse") != 0) ? 0.5f : 0f)))) * movementMultiplier * (float)Game1.currentGameTime.ElapsedGameTime.Milliseconds));
			if (movementDirections.Count > 1)
			{
				movementSpeed *= 0.707f;
			}
			if (Game1.CurrentEvent == null && hasBuff("19"))
			{
				movementSpeed = 0f;
			}
			return movementSpeed;
		}
		movementSpeed = Math.Max(1f, (float)base.speed + (Game1.eventUp ? ((float)Math.Max(0, Game1.CurrentEvent.farmerAddedSpeed - 2)) : (addedSpeed + (isRidingHorse() ? 5f : temporarySpeedBuff))));
		if (movementDirections.Count > 1)
		{
			movementSpeed = Math.Max(1, (int)Math.Sqrt(2f * (movementSpeed * movementSpeed)) / 2);
		}
		return movementSpeed;
	}

	public bool isWearingRing(string itemId)
	{
		if (rightRing.Value == null || !rightRing.Value.GetsEffectOfRing(itemId))
		{
			if (leftRing.Value != null)
			{
				return leftRing.Value.GetsEffectOfRing(itemId);
			}
			return false;
		}
		return true;
	}

	public override void Halt()
	{
		if (!FarmerSprite.PauseForSingleAnimation && !isRidingHorse() && !UsingTool)
		{
			base.Halt();
		}
		movementDirections.Clear();
		if (!isEmoteAnimating && !UsingTool)
		{
			stopJittering();
		}
		armOffset = Vector2.Zero;
		if (isRidingHorse())
		{
			mount.Halt();
			mount.Sprite.CurrentAnimation = null;
		}
		if (IsSitting())
		{
			ShowSitting();
		}
	}

	public void stopJittering()
	{
		jitterStrength = 0f;
		jitter = Vector2.Zero;
	}

	public override Microsoft.Xna.Framework.Rectangle nextPosition(int direction)
	{
		Microsoft.Xna.Framework.Rectangle nextPosition = GetBoundingBox();
		switch (direction)
		{
		case 0:
			nextPosition.Y -= (int)Math.Ceiling(getMovementSpeed());
			break;
		case 1:
			nextPosition.X += (int)Math.Ceiling(getMovementSpeed());
			break;
		case 2:
			nextPosition.Y += (int)Math.Ceiling(getMovementSpeed());
			break;
		case 3:
			nextPosition.X -= (int)Math.Ceiling(getMovementSpeed());
			break;
		}
		return nextPosition;
	}

	public Microsoft.Xna.Framework.Rectangle nextPositionHalf(int direction)
	{
		Microsoft.Xna.Framework.Rectangle nextPosition = GetBoundingBox();
		switch (direction)
		{
		case 0:
			nextPosition.Y -= (int)Math.Ceiling((double)getMovementSpeed() / 2.0);
			break;
		case 1:
			nextPosition.X += (int)Math.Ceiling((double)getMovementSpeed() / 2.0);
			break;
		case 2:
			nextPosition.Y += (int)Math.Ceiling((double)getMovementSpeed() / 2.0);
			break;
		case 3:
			nextPosition.X -= (int)Math.Ceiling((double)getMovementSpeed() / 2.0);
			break;
		}
		return nextPosition;
	}

	/// <summary>
	///
	/// </summary>
	/// <param name="skillType">e.g. farming, fishing, foraging</param>
	/// <param name="skillLevel">5 or 10</param>
	/// <returns></returns>
	public int getProfessionForSkill(int skillType, int skillLevel)
	{
		switch (skillLevel)
		{
		case 5:
			switch (skillType)
			{
			case 0:
				if (professions.Contains(0))
				{
					return 0;
				}
				if (professions.Contains(1))
				{
					return 1;
				}
				break;
			case 1:
				if (professions.Contains(6))
				{
					return 6;
				}
				if (professions.Contains(7))
				{
					return 7;
				}
				break;
			case 2:
				if (professions.Contains(12))
				{
					return 12;
				}
				if (professions.Contains(13))
				{
					return 13;
				}
				break;
			case 3:
				if (professions.Contains(18))
				{
					return 18;
				}
				if (professions.Contains(19))
				{
					return 19;
				}
				break;
			case 4:
				if (professions.Contains(24))
				{
					return 24;
				}
				if (professions.Contains(25))
				{
					return 25;
				}
				break;
			}
			break;
		case 10:
			switch (skillType)
			{
			case 0:
				if (professions.Contains(1))
				{
					if (professions.Contains(4))
					{
						return 4;
					}
					if (professions.Contains(5))
					{
						return 5;
					}
				}
				else
				{
					if (professions.Contains(2))
					{
						return 2;
					}
					if (professions.Contains(3))
					{
						return 3;
					}
				}
				break;
			case 1:
				if (professions.Contains(6))
				{
					if (professions.Contains(8))
					{
						return 8;
					}
					if (professions.Contains(9))
					{
						return 9;
					}
				}
				else
				{
					if (professions.Contains(10))
					{
						return 10;
					}
					if (professions.Contains(11))
					{
						return 11;
					}
				}
				break;
			case 2:
				if (professions.Contains(12))
				{
					if (professions.Contains(14))
					{
						return 14;
					}
					if (professions.Contains(15))
					{
						return 15;
					}
				}
				else
				{
					if (professions.Contains(16))
					{
						return 16;
					}
					if (professions.Contains(17))
					{
						return 17;
					}
				}
				break;
			case 3:
				if (professions.Contains(18))
				{
					if (professions.Contains(20))
					{
						return 20;
					}
					if (professions.Contains(21))
					{
						return 21;
					}
				}
				else
				{
					if (professions.Contains(23))
					{
						return 23;
					}
					if (professions.Contains(22))
					{
						return 22;
					}
				}
				break;
			case 4:
				if (professions.Contains(24))
				{
					if (professions.Contains(26))
					{
						return 26;
					}
					if (professions.Contains(27))
					{
						return 27;
					}
				}
				else
				{
					if (professions.Contains(28))
					{
						return 28;
					}
					if (professions.Contains(29))
					{
						return 29;
					}
				}
				break;
			}
			break;
		}
		return -1;
	}

	public void behaviorOnMovement(int direction)
	{
		hasMoved = true;
	}

	public void OnEmoteAnimationEnd(Farmer farmer)
	{
		if (farmer == this && isEmoteAnimating)
		{
			EndEmoteAnimation();
		}
	}

	public void EndEmoteAnimation()
	{
		if (isEmoteAnimating)
		{
			if (jitterStrength > 0f)
			{
				stopJittering();
			}
			if (yJumpOffset != 0)
			{
				yJumpOffset = 0;
				yJumpVelocity = 0f;
			}
			FarmerSprite.PauseForSingleAnimation = false;
			FarmerSprite.StopAnimation();
			isEmoteAnimating = false;
		}
	}

	private void broadcastHaltAnimation(Farmer who)
	{
		if (IsLocalPlayer)
		{
			haltAnimationEvent.Fire();
		}
		else
		{
			completelyStopAnimating(who);
		}
	}

	private void performHaltAnimation()
	{
		completelyStopAnimatingOrDoingAction();
	}

	public void performKissFarmer(long otherPlayerID)
	{
		Farmer spouse = Game1.getFarmer(otherPlayerID);
		if (spouse != null)
		{
			bool localPlayerOnLeft = base.StandingPixel.X < spouse.StandingPixel.X;
			PerformKiss(localPlayerOnLeft ? 1 : 3);
			spouse.PerformKiss((!localPlayerOnLeft) ? 1 : 3);
		}
	}

	public void PerformKiss(int facingDirection)
	{
		if (!Game1.eventUp && !UsingTool && (!IsLocalPlayer || Game1.activeClickableMenu == null) && !isRidingHorse() && !IsSitting() && !base.IsEmoting && CanMove)
		{
			CanMove = false;
			FarmerSprite.PauseForSingleAnimation = false;
			faceDirection(facingDirection);
			FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[2]
			{
				new FarmerSprite.AnimationFrame(101, 1000, 0, secondaryArm: false, FacingDirection == 3),
				new FarmerSprite.AnimationFrame(6, 1, secondaryArm: false, FacingDirection == 3, broadcastHaltAnimation)
			});
		}
	}

	public override void MovePosition(GameTime time, xTile.Dimensions.Rectangle viewport, GameLocation currentLocation)
	{
		if (IsSitting())
		{
			return;
		}
		if (Game1.CurrentEvent == null || Game1.CurrentEvent.playerControlSequence)
		{
			if (Game1.shouldTimePass() && temporarilyInvincible)
			{
				if (temporaryInvincibilityTimer < 0)
				{
					currentTemporaryInvincibilityDuration = 1200;
				}
				temporaryInvincibilityTimer += time.ElapsedGameTime.Milliseconds;
				if (temporaryInvincibilityTimer > currentTemporaryInvincibilityDuration)
				{
					temporarilyInvincible = false;
					temporaryInvincibilityTimer = 0;
				}
			}
		}
		else if (temporarilyInvincible)
		{
			temporarilyInvincible = false;
			temporaryInvincibilityTimer = 0;
		}
		if (Game1.activeClickableMenu != null && (Game1.CurrentEvent == null || Game1.CurrentEvent.playerControlSequence))
		{
			return;
		}
		if (isRafting)
		{
			moveRaft(currentLocation, time);
			return;
		}
		if (xVelocity != 0f || yVelocity != 0f)
		{
			if (double.IsNaN(xVelocity) || double.IsNaN(yVelocity))
			{
				xVelocity = 0f;
				yVelocity = 0f;
			}
			Microsoft.Xna.Framework.Rectangle bounds = GetBoundingBox();
			Microsoft.Xna.Framework.Rectangle value = new Microsoft.Xna.Framework.Rectangle(bounds.X + (int)Math.Floor(xVelocity), bounds.Y - (int)Math.Floor(yVelocity), bounds.Width, bounds.Height);
			Microsoft.Xna.Framework.Rectangle nextPositionCeil = new Microsoft.Xna.Framework.Rectangle(bounds.X + (int)Math.Ceiling(xVelocity), bounds.Y - (int)Math.Ceiling(yVelocity), bounds.Width, bounds.Height);
			Microsoft.Xna.Framework.Rectangle nextPosition = Microsoft.Xna.Framework.Rectangle.Union(value, nextPositionCeil);
			if (!currentLocation.isCollidingPosition(nextPosition, viewport, isFarmer: true, -1, glider: false, this))
			{
				position.X += xVelocity;
				position.Y -= yVelocity;
				xVelocity -= xVelocity / 16f;
				yVelocity -= yVelocity / 16f;
				if (Math.Abs(xVelocity) <= 0.05f)
				{
					xVelocity = 0f;
				}
				if (Math.Abs(yVelocity) <= 0.05f)
				{
					yVelocity = 0f;
				}
			}
			else
			{
				xVelocity -= xVelocity / 16f;
				yVelocity -= yVelocity / 16f;
				if (Math.Abs(xVelocity) <= 0.05f)
				{
					xVelocity = 0f;
				}
				if (Math.Abs(yVelocity) <= 0.05f)
				{
					yVelocity = 0f;
				}
			}
		}
		if (CanMove || Game1.eventUp || controller != null || canStrafeForToolUse())
		{
			temporaryPassableTiles.ClearNonIntersecting(GetBoundingBox());
			float movementSpeed = getMovementSpeed();
			temporarySpeedBuff = 0f;
			if ((movementDirections.Contains(0) && MovePositionImpl(0, 0f, 0f - movementSpeed, time, viewport)) || (movementDirections.Contains(2) && MovePositionImpl(2, 0f, movementSpeed, time, viewport)) || (movementDirections.Contains(1) && MovePositionImpl(1, movementSpeed, 0f, time, viewport)) || (movementDirections.Contains(3) && MovePositionImpl(3, 0f - movementSpeed, 0f, time, viewport)))
			{
				return;
			}
		}
		if (movementDirections.Count > 0 && !UsingTool)
		{
			FarmerSprite.intervalModifier = 1f - (running ? 0.0255f : 0.025f) * (Math.Max(1f, ((float)base.speed + (Game1.eventUp ? 0f : ((float)(int)addedSpeed + (isRidingHorse() ? 4.6f : 0f)))) * movementMultiplier * (float)Game1.currentGameTime.ElapsedGameTime.Milliseconds) * 1.25f);
		}
		else
		{
			FarmerSprite.intervalModifier = 1f;
		}
		if (currentLocation != null && currentLocation.isFarmerCollidingWithAnyCharacter())
		{
			temporaryPassableTiles.Add(new Microsoft.Xna.Framework.Rectangle(base.TilePoint.X * 64, base.TilePoint.Y * 64, 64, 64));
		}
	}

	public bool canStrafeForToolUse()
	{
		if ((int)toolHold != 0 && canReleaseTool)
		{
			if ((int)toolPower < 1)
			{
				return (int)toolHoldStartTime - (int)toolHold > 150;
			}
			return true;
		}
		return false;
	}

	/// <summary>Handle a player's movement in a specific direction, after the game has already checked whether movement is allowed.</summary>
	/// <param name="direction">The direction the player is moving in, matching a constant like <see cref="F:StardewValley.Game1.up" />.</param>
	/// <param name="movementSpeedX">The player's movement speed along the X axis for this direction.</param>
	/// <param name="movementSpeedY">The player's movement speed along the Y axis for this direction.</param>
	/// <param name="time">The elapsed game time.</param>
	/// <param name="viewport">The pixel area being viewed relative to the top-left corner of the map.</param>
	/// <returns>Returns whether the movement was fully handled (e.g. a warp was activated), so no further movement logic should be applied.</returns>
	protected virtual bool MovePositionImpl(int direction, float movementSpeedX, float movementSpeedY, GameTime time, xTile.Dimensions.Rectangle viewport)
	{
		Microsoft.Xna.Framework.Rectangle targetPos = nextPosition(direction);
		Warp warp = Game1.currentLocation.isCollidingWithWarp(targetPos, this);
		if (warp != null && IsLocalPlayer)
		{
			if (Game1.eventUp && !((!(Game1.CurrentEvent?.isFestival)) ?? true))
			{
				Game1.CurrentEvent.TryStartEndFestivalDialogue(this);
			}
			else
			{
				warpFarmer(warp, direction);
			}
			return true;
		}
		bool isCutscene = Game1.eventUp && !(Game1.CurrentEvent?.isFestival ?? true) && ((!(Game1.CurrentEvent?.playerControlSequence)) ?? false);
		if (!base.currentLocation.isCollidingPosition(targetPos, viewport, isFarmer: true, 0, glider: false, this) || ignoreCollisions || isCutscene)
		{
			position.X += movementSpeedX;
			position.Y += movementSpeedY;
			behaviorOnMovement(direction);
			return false;
		}
		if (!base.currentLocation.isCollidingPosition(nextPositionHalf(direction), viewport, isFarmer: true, 0, glider: false, this))
		{
			position.X += movementSpeedX / 2f;
			position.Y += movementSpeedY / 2f;
			behaviorOnMovement(direction);
			return false;
		}
		if (movementDirections.Count == 1)
		{
			Microsoft.Xna.Framework.Rectangle tmp = targetPos;
			if (direction == 0 || direction == 2)
			{
				tmp.Width /= 4;
				bool leftCorner = base.currentLocation.isCollidingPosition(tmp, viewport, isFarmer: true, 0, glider: false, this);
				tmp.X += tmp.Width * 3;
				bool rightCorner = base.currentLocation.isCollidingPosition(tmp, viewport, isFarmer: true, 0, glider: false, this);
				if (leftCorner && !rightCorner && !base.currentLocation.isCollidingPosition(nextPosition(1), viewport, isFarmer: true, 0, glider: false, this))
				{
					position.X += (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
				}
				else if (rightCorner && !leftCorner && !base.currentLocation.isCollidingPosition(nextPosition(3), viewport, isFarmer: true, 0, glider: false, this))
				{
					position.X -= (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
				}
			}
			else
			{
				tmp.Height /= 4;
				bool topCorner = base.currentLocation.isCollidingPosition(tmp, viewport, isFarmer: true, 0, glider: false, this);
				tmp.Y += tmp.Height * 3;
				bool bottomCorner = base.currentLocation.isCollidingPosition(tmp, viewport, isFarmer: true, 0, glider: false, this);
				if (topCorner && !bottomCorner && !base.currentLocation.isCollidingPosition(nextPosition(2), viewport, isFarmer: true, 0, glider: false, this))
				{
					position.Y += (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
				}
				else if (bottomCorner && !topCorner && !base.currentLocation.isCollidingPosition(nextPosition(0), viewport, isFarmer: true, 0, glider: false, this))
				{
					position.Y -= (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
				}
			}
		}
		return false;
	}

	public void updateMovementAnimation(GameTime time)
	{
		if (_emoteGracePeriod > 0)
		{
			_emoteGracePeriod -= time.ElapsedGameTime.Milliseconds;
		}
		if (isEmoteAnimating && (((IsLocalPlayer ? (movementDirections.Count > 0) : IsRemoteMoving()) && _emoteGracePeriod <= 0) || !FarmerSprite.PauseForSingleAnimation))
		{
			EndEmoteAnimation();
		}
		bool carrying = IsCarrying();
		if (!isRidingHorse())
		{
			xOffset = 0f;
		}
		if (CurrentTool is FishingRod rod && (rod.isTimingCast || rod.isCasting))
		{
			rod.setTimingCastAnimation(this);
			return;
		}
		if (FarmerSprite.PauseForSingleAnimation || UsingTool)
		{
			if (UsingTool && canStrafeForToolUse() && (movementDirections.Count > 0 || (!IsLocalPlayer && IsRemoteMoving())) && yJumpOffset == 0)
			{
				jumpWithoutSound(2.5f);
			}
			return;
		}
		if (IsSitting())
		{
			ShowSitting();
			return;
		}
		if (IsLocalPlayer && !CanMove && !Game1.eventUp)
		{
			if (isRidingHorse() && mount != null && !isAnimatingMount)
			{
				showRiding();
			}
			else if (carrying)
			{
				showCarrying();
			}
			return;
		}
		if (IsLocalPlayer || isFakeEventActor)
		{
			moveUp = movementDirections.Contains(0);
			moveRight = movementDirections.Contains(1);
			moveDown = movementDirections.Contains(2);
			moveLeft = movementDirections.Contains(3);
			if (moveLeft)
			{
				FacingDirection = 3;
			}
			else if (moveRight)
			{
				FacingDirection = 1;
			}
			else if (moveUp)
			{
				FacingDirection = 0;
			}
			else if (moveDown)
			{
				FacingDirection = 2;
			}
			if (isRidingHorse() && !mount.dismounting)
			{
				base.speed = 2;
			}
		}
		else
		{
			moveLeft = IsRemoteMoving() && FacingDirection == 3;
			moveRight = IsRemoteMoving() && FacingDirection == 1;
			moveUp = IsRemoteMoving() && FacingDirection == 0;
			moveDown = IsRemoteMoving() && FacingDirection == 2;
			bool num = moveUp || moveRight || moveDown || moveLeft;
			float speed = position.CurrentInterpolationSpeed() / ((float)Game1.currentGameTime.ElapsedGameTime.Milliseconds * 0.066f);
			running = Math.Abs(speed - 5f) < Math.Abs(speed - 2f) && !bathingClothes && !onBridge.Value;
			if (!num)
			{
				FarmerSprite.StopAnimation();
			}
		}
		if (hasBuff("19"))
		{
			running = false;
			moveUp = false;
			moveDown = false;
			moveLeft = false;
			moveRight = false;
		}
		if (!FarmerSprite.PauseForSingleAnimation && !UsingTool)
		{
			if (isRidingHorse() && !mount.dismounting)
			{
				showRiding();
			}
			else if (moveLeft && running && !carrying)
			{
				FarmerSprite.animate(56, time);
			}
			else if (moveRight && running && !carrying)
			{
				FarmerSprite.animate(40, time);
			}
			else if (moveUp && running && !carrying)
			{
				FarmerSprite.animate(48, time);
			}
			else if (moveDown && running && !carrying)
			{
				FarmerSprite.animate(32, time);
			}
			else if (moveLeft && running)
			{
				FarmerSprite.animate(152, time);
			}
			else if (moveRight && running)
			{
				FarmerSprite.animate(136, time);
			}
			else if (moveUp && running)
			{
				FarmerSprite.animate(144, time);
			}
			else if (moveDown && running)
			{
				FarmerSprite.animate(128, time);
			}
			else if (moveLeft && !carrying)
			{
				FarmerSprite.animate(24, time);
			}
			else if (moveRight && !carrying)
			{
				FarmerSprite.animate(8, time);
			}
			else if (moveUp && !carrying)
			{
				FarmerSprite.animate(16, time);
			}
			else if (moveDown && !carrying)
			{
				FarmerSprite.animate(0, time);
			}
			else if (moveLeft)
			{
				FarmerSprite.animate(120, time);
			}
			else if (moveRight)
			{
				FarmerSprite.animate(104, time);
			}
			else if (moveUp)
			{
				FarmerSprite.animate(112, time);
			}
			else if (moveDown)
			{
				FarmerSprite.animate(96, time);
			}
			else if (carrying)
			{
				showCarrying();
			}
			else
			{
				showNotCarrying();
			}
		}
	}

	public bool IsCarrying()
	{
		if (mount != null || isAnimatingMount)
		{
			return false;
		}
		if (IsSitting())
		{
			return false;
		}
		if (onBridge.Value)
		{
			return false;
		}
		if (ActiveObject == null || Game1.eventUp || Game1.killScreen)
		{
			return false;
		}
		if (!ActiveObject.IsHeldOverHead())
		{
			return false;
		}
		return true;
	}

	public void doneEating()
	{
		isEating = false;
		tempFoodItemTextureName.Value = null;
		completelyStopAnimatingOrDoingAction();
		forceCanMove();
		if (mostRecentlyGrabbedItem == null || !IsLocalPlayer)
		{
			return;
		}
		Object consumed = itemToEat as Object;
		if (consumed.QualifiedItemId == "(O)434")
		{
			if (Utility.foundAllStardrops())
			{
				Game1.getSteamAchievement("Achievement_Stardrop");
			}
			yOffset = 0f;
			yJumpOffset = 0;
			Game1.changeMusicTrack("none");
			Game1.playSound("stardrop");
			string mid = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs." + Game1.random.Choose("3094", "3095"));
			DelayedAction.showDialogueAfterDelay(string.Concat(str1: favoriteThing.Contains("Stardew") ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3097") : ((!favoriteThing.Equals("ConcernedApe")) ? (mid + favoriteThing) : Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3099")), str0: Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3100"), str2: Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3101")), 6000);
			maxStamina.Value += 34;
			stamina = MaxStamina;
			FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[1]
			{
				new FarmerSprite.AnimationFrame(57, 6000)
			});
			startGlowing(new Color(200, 0, 255), border: false, 0.1f);
			jitterStrength = 1f;
			Game1.staminaShakeTimer = 12000;
			Game1.screenGlowOnce(new Color(200, 0, 255), hold: true);
			CanMove = false;
			freezePause = 8000;
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(368, 16, 16, 16), 60f, 8, 40, base.Position + new Vector2(-8f, -128f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0.0075f, 0f, 0f)
			{
				alpha = 0.75f,
				alphaFade = 0.0025f,
				motion = new Vector2(0f, -0.25f)
			});
			if (Game1.displayHUD && !Game1.eventUp)
			{
				for (int i = 0; i < 40; i++)
				{
					Game1.uiOverlayTempSprites.Add(new TemporaryAnimatedSprite(Game1.random.Next(10, 12), new Vector2((float)Game1.graphics.GraphicsDevice.Viewport.TitleSafeArea.Right / Game1.options.uiScale - 48f - 8f - (float)Game1.random.Next(64), (float)Game1.random.Next(-64, 64) + (float)Game1.graphics.GraphicsDevice.Viewport.TitleSafeArea.Bottom / Game1.options.uiScale - 224f - 16f - (float)(int)((double)(MaxStamina - 270) * 0.715)), Game1.random.Choose(Color.White, Color.Lime), 8, flipped: false, 50f)
					{
						layerDepth = 1f,
						delayBeforeAnimationStart = 200 * i,
						interval = 100f,
						local = true
					});
				}
			}
			Point tile = base.TilePoint;
			Utility.addSprinklesToLocation(base.currentLocation, tile.X, tile.Y, 9, 9, 6000, 100, new Color(200, 0, 255), null, motionTowardCenter: true);
			DelayedAction.stopFarmerGlowing(6000);
			Utility.addSprinklesToLocation(base.currentLocation, tile.X, tile.Y, 9, 9, 6000, 300, Color.Cyan, null, motionTowardCenter: true);
			mostRecentlyGrabbedItem = null;
		}
		else
		{
			if (consumed.HasContextTag("ginger_item"))
			{
				buffs.Remove("25");
			}
			foreach (Buff buff in consumed.GetFoodOrDrinkBuffs())
			{
				applyBuff(buff);
			}
			if (consumed.QualifiedItemId == "(O)773")
			{
				health = maxHealth;
			}
			else if (consumed.QualifiedItemId == "(O)351")
			{
				exhausted.Value = false;
			}
			else if (consumed.QualifiedItemId == "(O)349")
			{
				Stamina = MaxStamina;
			}
			float oldStam = Stamina;
			int oldHealth = health;
			int staminaToHeal = consumed.staminaRecoveredOnConsumption();
			int healthToHeal = consumed.healthRecoveredOnConsumption();
			if (Utility.GetDayOfPassiveFestival("DesertFestival") > 0 && base.currentLocation is MineShaft && Game1.mine.getMineArea() == 121 && team.calicoStatueEffects.ContainsKey(6))
			{
				staminaToHeal = Math.Max(1, staminaToHeal / 2);
				healthToHeal = Math.Max(1, healthToHeal / 2);
			}
			Stamina = Math.Min(MaxStamina, Stamina + (float)staminaToHeal);
			health = Math.Min(maxHealth, health + healthToHeal);
			if (oldStam < Stamina)
			{
				Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3116", (int)(Stamina - oldStam)), 4));
			}
			if (oldHealth < health)
			{
				Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3118", health - oldHealth), 5));
			}
		}
		if (consumed != null && consumed.Edibility < 0)
		{
			CanMove = false;
			sickAnimationEvent.Fire();
		}
	}

	public bool checkForQuestComplete(NPC n, int number1, int number2, Item item, string str, int questType = -1, int questTypeToIgnore = -1, bool probe = false)
	{
		bool worked = false;
		for (int i = questLog.Count - 1; i >= 0; i--)
		{
			if (questLog[i] != null && (questType == -1 || (int)questLog[i].questType == questType) && (questTypeToIgnore == -1 || (int)questLog[i].questType != questTypeToIgnore) && questLog[i].checkIfComplete(n, number1, number2, item, str, probe))
			{
				worked = true;
			}
		}
		return worked;
	}

	public virtual void AddCompanion(Companion companion)
	{
		if (!companions.Contains(companion))
		{
			companion.InitializeCompanion(this);
			companions.Add(companion);
		}
	}

	public virtual void RemoveCompanion(Companion companion)
	{
		if (companions.Contains(companion))
		{
			companions.Remove(companion);
			companion.CleanupCompanion();
		}
	}

	public static void completelyStopAnimating(Farmer who)
	{
		who.completelyStopAnimatingOrDoingAction();
	}

	public void completelyStopAnimatingOrDoingAction()
	{
		CanMove = !Game1.eventUp;
		if (isEmoteAnimating)
		{
			EndEmoteAnimation();
		}
		if (UsingTool)
		{
			EndUsingTool();
			if (CurrentTool is FishingRod rod)
			{
				rod.resetState();
			}
		}
		if (usingSlingshot && CurrentTool is Slingshot slingshot)
		{
			slingshot.finish();
		}
		UsingTool = false;
		isEating = false;
		FarmerSprite.PauseForSingleAnimation = false;
		usingSlingshot = false;
		canReleaseTool = false;
		Halt();
		Sprite.StopAnimation();
		if (CurrentTool is MeleeWeapon weapon)
		{
			weapon.isOnSpecial = false;
		}
		stopJittering();
	}

	public void doEmote(int whichEmote)
	{
		if (!Game1.eventUp && !isEmoting)
		{
			isEmoting = true;
			currentEmote = whichEmote;
			currentEmoteFrame = 0;
			emoteInterval = 0f;
		}
	}

	public void performTenMinuteUpdate()
	{
	}

	public void setRunning(bool isRunning, bool force = false)
	{
		if (canOnlyWalk || ((bool)bathingClothes && !running) || (Game1.CurrentEvent != null && isRunning && !Game1.CurrentEvent.isFestival && !Game1.CurrentEvent.playerControlSequence && (controller == null || !controller.allowPlayerPathingInEvent)))
		{
			return;
		}
		if (isRidingHorse())
		{
			running = true;
		}
		else if (stamina <= 0f)
		{
			base.speed = 2;
			if (running)
			{
				Halt();
			}
			running = false;
		}
		else if (force || (CanMove && !isEating && Game1.currentLocation != null && (Game1.currentLocation.currentEvent == null || Game1.currentLocation.currentEvent.playerControlSequence) && (isRunning || !UsingTool) && (Sprite == null || !((FarmerSprite)Sprite).PauseForSingleAnimation)))
		{
			running = isRunning;
			if (running)
			{
				base.speed = 5;
			}
			else
			{
				base.speed = 2;
			}
		}
		else if (UsingTool)
		{
			running = isRunning;
			if (running)
			{
				base.speed = 5;
			}
			else
			{
				base.speed = 2;
			}
		}
	}

	public void addSeenResponse(string id)
	{
		dialogueQuestionsAnswered.Add(id);
	}

	public void eatObject(Object o, bool overrideFullness = false)
	{
		if (o?.QualifiedItemId == "(O)434")
		{
			Game1.MusicDuckTimer = 10000f;
			Game1.changeMusicTrack("none");
			Game1.multiplayer.globalChatInfoMessage("Stardrop", base.Name);
		}
		if (getFacingDirection() != 2)
		{
			faceDirection(2);
		}
		itemToEat = o;
		mostRecentlyGrabbedItem = o;
		forceCanMove();
		completelyStopAnimatingOrDoingAction();
		if (Game1.objectData.TryGetValue(o.ItemId, out var data) && data.IsDrink)
		{
			if (IsLocalPlayer && hasBuff("7") && !overrideFullness)
			{
				Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2898")));
				return;
			}
			drinkAnimationEvent.Fire(o.getOne() as Object);
		}
		else if (o.Edibility != -300)
		{
			if (hasBuff("6") && !overrideFullness)
			{
				Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2899")));
				return;
			}
			eatAnimationEvent.Fire(o.getOne() as Object);
		}
		freezePause = 20000;
		CanMove = false;
		isEating = true;
	}

	/// <inheritdoc />
	public override void DrawShadow(SpriteBatch b)
	{
		float drawLayer = getDrawLayer() - 1E-06f;
		b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(GetShadowOffset() + base.Position + new Vector2(32f, 24f)), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f - (((running || UsingTool) && FarmerSprite.currentAnimationIndex > 1) ? ((float)Math.Abs(FarmerRenderer.featureYOffsetPerFrame[FarmerSprite.CurrentFrame]) * 0.5f) : 0f), SpriteEffects.None, drawLayer);
	}

	private void performDrinkAnimation(Object item)
	{
		if (isEmoteAnimating)
		{
			EndEmoteAnimation();
		}
		if (!IsLocalPlayer)
		{
			itemToEat = item;
		}
		FarmerSprite.animateOnce(294, 80f, 8);
		isEating = true;
		if (item != null && item.HasContextTag("mayo_item") && Utility.isThereAFarmerOrCharacterWithinDistance(base.Tile, 7, base.currentLocation) is NPC { Age: not 2 } npc)
		{
			int whichMessage = Game1.random.Next(3);
			if (npc.Manners == 2 || npc.SocialAnxiety == 1)
			{
				whichMessage = 3;
			}
			if (npc.Name == "Emily" || npc.Name == "Sandy" || npc.Name == "Linus" || (npc.Name == "Krobus" && item.QualifiedItemId == "(O)308"))
			{
				whichMessage = 4;
			}
			else if (npc.Name == "Krobus" || npc.Name == "Dwarf" || npc is Monster || npc is Horse || npc is Pet || npc is Child)
			{
				return;
			}
			npc.showTextAboveHead(Game1.content.LoadString("Strings\\1_6_Strings:Mayo_reaction" + whichMessage), null, 2, 3000, 500);
			npc.faceTowardFarmerForPeriod(1500, 7, faceAway: false, this);
		}
	}

	public Farmer CreateFakeEventFarmer()
	{
		Farmer fake_farmer = new Farmer(new FarmerSprite(FarmerSprite.textureName.Value), new Vector2(192f, 192f), 1, "", new List<Item>(), IsMale);
		fake_farmer.Name = base.Name;
		fake_farmer.displayName = displayName;
		fake_farmer.isFakeEventActor = true;
		fake_farmer.changeGender(IsMale);
		fake_farmer.changeHairStyle(hair);
		fake_farmer.UniqueMultiplayerID = UniqueMultiplayerID;
		fake_farmer.shirtItem.Set(shirtItem.Value);
		fake_farmer.pantsItem.Set(pantsItem.Value);
		fake_farmer.shirt.Set(shirt.Value);
		fake_farmer.pants.Set(pants.Value);
		foreach (Trinket t in trinketItems)
		{
			fake_farmer.trinketItems.Add((Trinket)t.getOne());
		}
		fake_farmer.changeShoeColor(shoes.Value);
		fake_farmer.boots.Set(boots.Value);
		fake_farmer.leftRing.Set(leftRing.Value);
		fake_farmer.rightRing.Set(rightRing.Value);
		fake_farmer.hat.Set(hat.Value);
		fake_farmer.pantsColor.Set(pantsColor.Value);
		fake_farmer.changeHairColor(hairstyleColor.Value);
		fake_farmer.changeSkinColor(skin.Value);
		fake_farmer.accessory.Set(accessory.Value);
		fake_farmer.changeEyeColor(newEyeColor.Value);
		fake_farmer.UpdateClothing();
		return fake_farmer;
	}

	private void performEatAnimation(Object item)
	{
		if (isEmoteAnimating)
		{
			EndEmoteAnimation();
		}
		if (!IsLocalPlayer)
		{
			itemToEat = item;
		}
		FarmerSprite.animateOnce(216, 80f, 8);
		isEating = true;
	}

	public void netDoEmote(string emote_type)
	{
		doEmoteEvent.Fire(emote_type);
	}

	private void performSickAnimation()
	{
		if (isEmoteAnimating)
		{
			EndEmoteAnimation();
		}
		isEating = false;
		FarmerSprite.animateOnce(224, 350f, 4);
		doEmote(12);
	}

	public void eatHeldObject()
	{
		if (isEmoteAnimating)
		{
			EndEmoteAnimation();
		}
		if (!Game1.fadeToBlack)
		{
			if (ActiveObject == null)
			{
				ActiveObject = (Object)mostRecentlyGrabbedItem;
			}
			eatObject(ActiveObject);
			if (isEating)
			{
				reduceActiveItemByOne();
				CanMove = false;
			}
		}
	}

	public void grabObject(Object obj)
	{
		if (isEmoteAnimating)
		{
			EndEmoteAnimation();
		}
		if (obj != null)
		{
			CanMove = false;
			switch (FacingDirection)
			{
			case 2:
				((FarmerSprite)Sprite).animateOnce(64, 50f, 8);
				break;
			case 1:
				((FarmerSprite)Sprite).animateOnce(72, 50f, 8);
				break;
			case 0:
				((FarmerSprite)Sprite).animateOnce(80, 50f, 8);
				break;
			case 3:
				((FarmerSprite)Sprite).animateOnce(88, 50f, 8);
				break;
			}
			Game1.playSound("pickUpItem");
		}
	}

	public virtual void PlayFishBiteChime()
	{
		int bite_chime = biteChime.Value;
		if (bite_chime < 0)
		{
			bite_chime = Game1.game1.instanceIndex;
		}
		if (bite_chime > 3)
		{
			bite_chime = 3;
		}
		if (bite_chime == 0)
		{
			playNearbySoundLocal("fishBite");
		}
		else
		{
			playNearbySoundLocal("fishBite_alternate_" + (bite_chime - 1));
		}
	}

	public string getTitle()
	{
		int level = Level;
		if (level >= 30)
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2016");
		}
		switch (level)
		{
		case 28:
		case 29:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2017");
		case 26:
		case 27:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2018");
		case 24:
		case 25:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2019");
		case 22:
		case 23:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2020");
		case 20:
		case 21:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2021");
		case 18:
		case 19:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2022");
		case 16:
		case 17:
			if (!IsMale)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2024");
			}
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2023");
		case 14:
		case 15:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2025");
		case 12:
		case 13:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2026");
		case 10:
		case 11:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2027");
		case 8:
		case 9:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2028");
		case 6:
		case 7:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2029");
		case 4:
		case 5:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2030");
		case 2:
		case 3:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2031");
		default:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2032");
		}
	}

	public void queueMessage(byte messageType, Farmer sourceFarmer, params object[] data)
	{
		queueMessage(new OutgoingMessage(messageType, sourceFarmer, data));
	}

	public void queueMessage(OutgoingMessage message)
	{
		messageQueue.Add(message);
	}
}
