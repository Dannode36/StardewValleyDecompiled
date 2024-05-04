using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewValley.Audio;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.GameData.Characters;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Pathfinding;
using StardewValley.Quests;
using StardewValley.SpecialOrders;
using StardewValley.TerrainFeatures;
using StardewValley.TokenizableStrings;
using xTile.Dimensions;

namespace StardewValley;

[XmlInclude(typeof(Cat))]
[XmlInclude(typeof(Child))]
[XmlInclude(typeof(Dog))]
[XmlInclude(typeof(Horse))]
[XmlInclude(typeof(Junimo))]
[XmlInclude(typeof(JunimoHarvester))]
[XmlInclude(typeof(Pet))]
[XmlInclude(typeof(TrashBear))]
[XmlInclude(typeof(Raccoon))]
[XmlInclude(typeof(Monster))]
public class NPC : Character, IComparable
{
	public const int minimum_square_pause = 6000;

	public const int maximum_square_pause = 12000;

	public const int portrait_width = 64;

	public const int portrait_height = 64;

	public const int portrait_neutral_index = 0;

	public const int portrait_happy_index = 1;

	public const int portrait_sad_index = 2;

	public const int portrait_custom_index = 3;

	public const int portrait_blush_index = 4;

	public const int portrait_angry_index = 5;

	public const int startingFriendship = 0;

	public const int defaultSpeed = 2;

	public const int maxGiftsPerWeek = 2;

	public const int friendshipPointsPerHeartLevel = 250;

	public const int maxFriendshipPoints = 2500;

	public const int gift_taste_love = 0;

	public const int gift_taste_like = 2;

	public const int gift_taste_neutral = 8;

	public const int gift_taste_dislike = 4;

	public const int gift_taste_hate = 6;

	public const int gift_taste_stardroptea = 7;

	public const int textStyle_shake = 0;

	public const int textStyle_none = 2;

	public const int adult = 0;

	public const int teen = 1;

	public const int child = 2;

	public const int neutral = 0;

	public const int polite = 1;

	public const int rude = 2;

	public const int outgoing = 0;

	public const int shy = 1;

	public const int positive = 0;

	public const int negative = 1;

	public const string region_desert = "Desert";

	public const string region_town = "Town";

	public const string region_other = "Other";

	private Dictionary<string, string> dialogue;

	private SchedulePathDescription directionsToNewLocation;

	private int directionIndex;

	private int lengthOfWalkingSquareX;

	private int lengthOfWalkingSquareY;

	private int squarePauseAccumulation;

	private int squarePauseTotal;

	private int squarePauseOffset;

	public Microsoft.Xna.Framework.Rectangle lastCrossroad;

	/// <summary>The loaded portrait asset.</summary>
	/// <remarks>This is normally set via <see cref="M:StardewValley.NPC.ChooseAppearance(StardewValley.LocalizedContentManager)" />.</remarks>
	private Texture2D portrait;

	/// <summary>The last location for which <see cref="M:StardewValley.NPC.ChooseAppearance(StardewValley.LocalizedContentManager)" /> was applied.</summary>
	private string LastLocationNameForAppearance;

	/// <summary>The appearance ID from <c>Data/Characters</c> chosen by the last <see cref="M:StardewValley.NPC.ChooseAppearance(StardewValley.LocalizedContentManager)" /> call, or <c>null</c> if the last call didn't apply an appearance entry. This may not match their current textures if they were manually overridden after calling <see cref="M:StardewValley.NPC.ChooseAppearance(StardewValley.LocalizedContentManager)" />.</summary>
	[XmlIgnore]
	public string LastAppearanceId;

	private Vector2 nextSquarePosition;

	[XmlIgnore]
	public int shakeTimer;

	private bool isWalkingInSquare;

	private readonly NetBool isWalkingTowardPlayer = new NetBool();

	protected string textAboveHead;

	protected int textAboveHeadPreTimer;

	protected int textAboveHeadTimer;

	protected int textAboveHeadStyle;

	protected Color? textAboveHeadColor;

	protected float textAboveHeadAlpha;

	public int daysAfterLastBirth = -1;

	protected Dialogue extraDialogueMessageToAddThisMorning;

	[XmlElement("birthday_Season")]
	public readonly NetString birthday_Season = new NetString();

	[XmlElement("birthday_Day")]
	public readonly NetInt birthday_Day = new NetInt();

	[XmlElement("age")]
	public readonly NetInt age = new NetInt();

	[XmlElement("manners")]
	public readonly NetInt manners = new NetInt();

	[XmlElement("socialAnxiety")]
	public readonly NetInt socialAnxiety = new NetInt();

	[XmlElement("optimism")]
	public readonly NetInt optimism = new NetInt();

	/// <summary>The net-synchronized backing field for <see cref="P:StardewValley.NPC.Gender" />.</summary>
	[XmlElement("gender")]
	public readonly NetEnum<Gender> gender = new NetEnum<Gender>();

	[XmlIgnore]
	public readonly NetBool breather = new NetBool(value: true);

	[XmlIgnore]
	public readonly NetBool isSleeping = new NetBool(value: false);

	[XmlElement("sleptInBed")]
	public readonly NetBool sleptInBed = new NetBool(value: true);

	[XmlIgnore]
	public readonly NetBool hideShadow = new NetBool();

	[XmlElement("isInvisible")]
	public readonly NetBool isInvisible = new NetBool(value: false);

	[XmlElement("lastSeenMovieWeek")]
	public readonly NetInt lastSeenMovieWeek = new NetInt(-1);

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="F:StardewValley.Farmer.friendshipData" /> instead.</summary>
	public bool? datingFarmer;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="F:StardewValley.Farmer.friendshipData" /> instead.</summary>
	public bool? divorcedFromFarmer;

	[XmlElement("datable")]
	public readonly NetBool datable = new NetBool();

	[XmlIgnore]
	public bool updatedDialogueYet;

	[XmlIgnore]
	public bool immediateSpeak;

	[XmlIgnore]
	public bool ignoreScheduleToday;

	protected int defaultFacingDirection;

	private readonly NetVector2 defaultPosition = new NetVector2();

	[XmlElement("defaultMap")]
	public readonly NetString defaultMap = new NetString();

	public string loveInterest;

	public int id = -1;

	public int daysUntilNotInvisible;

	public bool followSchedule = true;

	[XmlIgnore]
	public PathFindController temporaryController;

	[XmlElement("moveTowardPlayerThreshold")]
	public readonly NetInt moveTowardPlayerThreshold = new NetInt();

	[XmlIgnore]
	public float rotation;

	[XmlIgnore]
	public float yOffset;

	[XmlIgnore]
	public float swimTimer;

	[XmlIgnore]
	public float timerSinceLastMovement;

	[XmlIgnore]
	public string mapBeforeEvent;

	[XmlIgnore]
	public Vector2 positionBeforeEvent;

	[XmlIgnore]
	public Vector2 lastPosition;

	[XmlIgnore]
	public float currentScheduleDelay;

	[XmlIgnore]
	public float scheduleDelaySeconds;

	[XmlIgnore]
	public bool layingDown;

	[XmlIgnore]
	public Vector2 appliedRouteAnimationOffset = Vector2.Zero;

	[XmlIgnore]
	public string[] routeAnimationMetadata;

	[XmlElement("hasSaidAfternoonDialogue")]
	private NetBool hasSaidAfternoonDialogue = new NetBool(value: false);

	[XmlIgnore]
	public static bool hasSomeoneWateredCrops;

	[XmlIgnore]
	public static bool hasSomeoneFedThePet;

	[XmlIgnore]
	public static bool hasSomeoneFedTheAnimals;

	[XmlIgnore]
	public static bool hasSomeoneRepairedTheFences = false;

	[XmlIgnore]
	protected bool _skipRouteEndIntro;

	[NonInstancedStatic]
	public static HashSet<string> invalidDialogueFiles = new HashSet<string>();

	[XmlIgnore]
	protected bool _hasLoadedMasterScheduleData;

	[XmlIgnore]
	protected Dictionary<string, string> _masterScheduleData;

	protected static Stack<Dialogue> _EmptyDialogue = new Stack<Dialogue>();

	/// <summary>If set to a non-null value, the dialogue to return for <see cref="P:StardewValley.NPC.CurrentDialogue" /> instead of reading <see cref="F:StardewValley.Game1.npcDialogues" />.</summary>
	[XmlIgnore]
	public Stack<Dialogue> TemporaryDialogue;

	[XmlIgnore]
	public readonly NetList<MarriageDialogueReference, NetRef<MarriageDialogueReference>> currentMarriageDialogue = new NetList<MarriageDialogueReference, NetRef<MarriageDialogueReference>>();

	public readonly NetBool hasBeenKissedToday = new NetBool(value: false);

	[XmlIgnore]
	public readonly NetRef<MarriageDialogueReference> marriageDefaultDialogue = new NetRef<MarriageDialogueReference>(null);

	[XmlIgnore]
	public readonly NetBool shouldSayMarriageDialogue = new NetBool(value: false);

	public readonly NetEvent0 removeHenchmanEvent = new NetEvent0();

	private bool isPlayingSleepingAnimation;

	public readonly NetBool shouldPlayRobinHammerAnimation = new NetBool();

	private bool isPlayingRobinHammerAnimation;

	public readonly NetBool shouldPlaySpousePatioAnimation = new NetBool();

	private bool isPlayingSpousePatioAnimation = new NetBool();

	public readonly NetBool shouldWearIslandAttire = new NetBool();

	private bool isWearingIslandAttire;

	public readonly NetBool isMovingOnPathFindPath = new NetBool();

	/// <summary>Whether the NPC's portrait has been explicitly overridden (e.g. using the <c>changePortrait</c> event command) and shouldn't be changed automatically.</summary>
	[XmlIgnore]
	public bool portraitOverridden;

	/// <summary>Whether the NPC's sprite has been explicitly overridden (e.g. using the <c>changeSprite</c> event command) and shouldn't be changed automatically.</summary>
	[XmlIgnore]
	public bool spriteOverridden;

	[XmlIgnore]
	public List<SchedulePathDescription> queuedSchedulePaths = new List<SchedulePathDescription>();

	[XmlIgnore]
	public int lastAttemptedSchedule = -1;

	[XmlIgnore]
	public readonly NetBool doingEndOfRouteAnimation = new NetBool();

	private bool currentlyDoingEndOfRouteAnimation;

	[XmlIgnore]
	public readonly NetBool goingToDoEndOfRouteAnimation = new NetBool();

	[XmlIgnore]
	public readonly NetString endOfRouteMessage = new NetString();

	/// <summary>The backing field for <see cref="P:StardewValley.NPC.ScheduleKey" />. Most code should use that property instead.</summary>
	[XmlElement("dayScheduleName")]
	public readonly NetString dayScheduleName = new NetString();

	[XmlElement("islandScheduleName")]
	public readonly NetString islandScheduleName = new NetString();

	private int[] routeEndIntro;

	private int[] routeEndAnimation;

	private int[] routeEndOutro;

	[XmlIgnore]
	public string nextEndOfRouteMessage;

	private string loadedEndOfRouteBehavior;

	[XmlIgnore]
	protected string _startedEndOfRouteBehavior;

	[XmlIgnore]
	protected string _finishingEndOfRouteBehavior;

	[XmlIgnore]
	protected int _beforeEndOfRouteAnimationFrame;

	public readonly NetString endOfRouteBehaviorName = new NetString();

	public Point previousEndPoint;

	public int squareMovementFacingPreference;

	protected bool returningToEndPoint;

	private bool wasKissedYesterday;

	[XmlIgnore]
	public SchedulePathDescription DirectionsToNewLocation
	{
		get
		{
			return directionsToNewLocation;
		}
		set
		{
			directionsToNewLocation = value;
		}
	}

	[XmlIgnore]
	public int DirectionIndex
	{
		get
		{
			return directionIndex;
		}
		set
		{
			directionIndex = value;
		}
	}

	public int DefaultFacingDirection
	{
		get
		{
			return defaultFacingDirection;
		}
		set
		{
			defaultFacingDirection = value;
		}
	}

	/// <summary>The main dialogue data for this NPC, if available.</summary>
	[XmlIgnore]
	public Dictionary<string, string> Dialogue
	{
		get
		{
			if (this is Monster || this is Pet || this is Horse || this is Child)
			{
				LoadedDialogueKey = null;
				return null;
			}
			if (dialogue == null)
			{
				string dialogue_file = "Characters\\Dialogue\\" + GetDialogueSheetName();
				if (invalidDialogueFiles.Contains(dialogue_file))
				{
					LoadedDialogueKey = null;
					dialogue = new Dictionary<string, string>();
				}
				try
				{
					dialogue = Game1.content.Load<Dictionary<string, string>>(dialogue_file).Select(delegate(KeyValuePair<string, string> pair)
					{
						string key = pair.Key;
						string value2 = StardewValley.Dialogue.applyGenderSwitch(str: pair.Value, gender: Game1.player.Gender, altTokenOnly: true);
						return new KeyValuePair<string, string>(key, value2);
					}).ToDictionary((KeyValuePair<string, string> p) => p.Key, (KeyValuePair<string, string> p) => p.Value);
					LoadedDialogueKey = dialogue_file;
				}
				catch (ContentLoadException)
				{
					invalidDialogueFiles.Add(dialogue_file);
					dialogue = new Dictionary<string, string>();
					LoadedDialogueKey = null;
				}
			}
			return dialogue;
		}
	}

	/// <summary>The dialogue key that was loaded via <see cref="P:StardewValley.NPC.Dialogue" />, if any.</summary>
	[XmlIgnore]
	public string LoadedDialogueKey { get; private set; }

	[XmlIgnore]
	public string DefaultMap
	{
		get
		{
			return defaultMap.Value;
		}
		set
		{
			defaultMap.Value = value;
		}
	}

	public Vector2 DefaultPosition
	{
		get
		{
			return defaultPosition.Value;
		}
		set
		{
			defaultPosition.Value = value;
		}
	}

	[XmlIgnore]
	public Texture2D Portrait
	{
		get
		{
			if (portrait == null && IsVillager)
			{
				ChooseAppearance();
			}
			return portrait;
		}
		set
		{
			portrait = value;
		}
	}

	/// <summary>Whether this NPC can dynamically change appearance based on their data in <c>Data/Characters</c>. This can be disabled for temporary NPCs and event actors.</summary>
	[XmlIgnore]
	public bool AllowDynamicAppearance { get; set; } = true;


	/// <inheritdoc />
	[XmlIgnore]
	public override bool IsVillager => true;

	/// <summary>The schedule of this NPC's movements and actions today, if loaded. The key is the time of departure, and the value is a list of directions to reach the new position.</summary>
	/// <remarks>You can set the schedule using <see cref="M:StardewValley.NPC.TryLoadSchedule" /> or one of its overloads.</remarks>
	[XmlIgnore]
	public Dictionary<int, SchedulePathDescription> Schedule { get; private set; }

	/// <summary>The <see cref="P:StardewValley.NPC.Schedule" />'s key in the original data asset, if loaded.</summary>
	[XmlIgnore]
	public string ScheduleKey => dayScheduleName.Value;

	public bool IsWalkingInSquare
	{
		get
		{
			return isWalkingInSquare;
		}
		set
		{
			isWalkingInSquare = value;
		}
	}

	public bool IsWalkingTowardPlayer
	{
		get
		{
			return isWalkingTowardPlayer;
		}
		set
		{
			isWalkingTowardPlayer.Value = value;
		}
	}

	[XmlIgnore]
	public virtual Stack<Dialogue> CurrentDialogue
	{
		get
		{
			if (TemporaryDialogue != null)
			{
				return TemporaryDialogue;
			}
			if (Game1.npcDialogues == null)
			{
				Game1.npcDialogues = new Dictionary<string, Stack<Dialogue>>();
			}
			if (!IsVillager)
			{
				return _EmptyDialogue;
			}
			Game1.npcDialogues.TryGetValue(base.Name, out var currentDialogue);
			if (currentDialogue == null)
			{
				return Game1.npcDialogues[base.Name] = loadCurrentDialogue();
			}
			return currentDialogue;
		}
		set
		{
			if (TemporaryDialogue != null)
			{
				TemporaryDialogue = value;
			}
			else if (Game1.npcDialogues != null)
			{
				Game1.npcDialogues[base.Name] = value;
			}
		}
	}

	[XmlIgnore]
	public string Birthday_Season
	{
		get
		{
			return birthday_Season;
		}
		set
		{
			birthday_Season.Value = value;
		}
	}

	[XmlIgnore]
	public int Birthday_Day
	{
		get
		{
			return birthday_Day;
		}
		set
		{
			birthday_Day.Value = value;
		}
	}

	[XmlIgnore]
	public int Age
	{
		get
		{
			return age;
		}
		set
		{
			age.Value = value;
		}
	}

	[XmlIgnore]
	public int Manners
	{
		get
		{
			return manners;
		}
		set
		{
			manners.Value = value;
		}
	}

	[XmlIgnore]
	public int SocialAnxiety
	{
		get
		{
			return socialAnxiety;
		}
		set
		{
			socialAnxiety.Value = value;
		}
	}

	[XmlIgnore]
	public int Optimism
	{
		get
		{
			return optimism;
		}
		set
		{
			optimism.Value = value;
		}
	}

	/// <summary>The character's gender identity.</summary>
	[XmlIgnore]
	public override Gender Gender
	{
		get
		{
			return gender.Value;
		}
		set
		{
			gender.Value = value;
		}
	}

	[XmlIgnore]
	public bool Breather
	{
		get
		{
			return breather;
		}
		set
		{
			breather.Value = value;
		}
	}

	[XmlIgnore]
	public bool HideShadow
	{
		get
		{
			return hideShadow;
		}
		set
		{
			hideShadow.Value = value;
		}
	}

	[XmlIgnore]
	public bool HasPartnerForDance
	{
		get
		{
			foreach (Farmer onlineFarmer in Game1.getOnlineFarmers())
			{
				if (onlineFarmer.dancePartner.TryGetVillager() == this)
				{
					return true;
				}
			}
			return false;
		}
	}

	[XmlIgnore]
	public bool IsInvisible
	{
		get
		{
			return isInvisible;
		}
		set
		{
			isInvisible.Value = value;
		}
	}

	public virtual bool CanSocialize
	{
		get
		{
			if (!IsVillager)
			{
				return false;
			}
			CharacterData data = GetData();
			if (data != null)
			{
				return GameStateQuery.CheckConditions(data.CanSocialize, base.currentLocation);
			}
			return false;
		}
	}

	public NPC()
	{
	}

	public NPC(AnimatedSprite sprite, Vector2 position, int facingDir, string name, LocalizedContentManager content = null)
		: base(sprite, position, 2, name)
	{
		faceDirection(facingDir);
		defaultPosition.Value = position;
		defaultFacingDirection = facingDir;
		lastCrossroad = new Microsoft.Xna.Framework.Rectangle((int)position.X, (int)position.Y + 64, 64, 64);
		if (content != null)
		{
			try
			{
				portrait = content.Load<Texture2D>("Portraits\\" + name);
			}
			catch (Exception)
			{
			}
		}
	}

	public NPC(AnimatedSprite sprite, Vector2 position, string defaultMap, int facingDirection, string name, bool datable, Texture2D portrait)
		: this(sprite, position, defaultMap, facingDirection, name, portrait, eventActor: false)
	{
		this.datable.Value = datable;
	}

	public NPC(AnimatedSprite sprite, Vector2 position, string defaultMap, int facingDir, string name, Texture2D portrait, bool eventActor)
		: base(sprite, position, 2, name)
	{
		this.portrait = portrait;
		faceDirection(facingDir);
		if (!eventActor)
		{
			lastCrossroad = new Microsoft.Xna.Framework.Rectangle((int)position.X, (int)position.Y + 64, 64, 64);
		}
		reloadData();
		defaultPosition.Value = position;
		this.defaultMap.Value = defaultMap;
		base.currentLocation = Game1.getLocationFromName(defaultMap);
		defaultFacingDirection = facingDir;
	}

	public virtual void reloadData()
	{
		if (this is Child)
		{
			return;
		}
		CharacterData data = GetData();
		if (data != null)
		{
			Age = (int)Utility.GetEnumOrDefault(data.Age, NpcAge.Adult);
			Manners = (int)Utility.GetEnumOrDefault(data.Manner, NpcManner.Neutral);
			SocialAnxiety = (int)Utility.GetEnumOrDefault(data.SocialAnxiety, NpcSocialAnxiety.Outgoing);
			Optimism = (int)Utility.GetEnumOrDefault(data.Optimism, NpcOptimism.Positive);
			Gender = Utility.GetEnumOrDefault(data.Gender, Gender.Male);
			datable.Value = data.CanBeRomanced;
			loveInterest = data.LoveInterest;
			Birthday_Season = (data.BirthSeason.HasValue ? Utility.getSeasonKey(data.BirthSeason.Value) : null);
			Birthday_Day = data.BirthDay;
			id = ((data.FestivalVanillaActorIndex > -1) ? data.FestivalVanillaActorIndex : Game1.hash.GetDeterministicHashCode(name.Value));
			breather.Value = data.Breather;
			if (!isMarried())
			{
				reloadDefaultLocation();
			}
			displayName = translateName();
		}
	}

	public virtual void reloadDefaultLocation()
	{
		CharacterData data = GetData();
		if (data != null && ReadNpcHomeData(data, base.currentLocation, out var locationName, out var tile, out var direction))
		{
			DefaultMap = locationName;
			DefaultPosition = new Vector2(tile.X * 64, tile.Y * 64);
			DefaultFacingDirection = direction;
		}
	}

	/// <summary>Get an NPC's home location from its data, or fallback values if it doesn't exist.</summary>
	/// <param name="data">The character data for the NPC.</param>
	/// <param name="currentLocation">The NPC's current location, if applicable.</param>
	/// <param name="locationName">The internal name of the NPC's default map.</param>
	/// <param name="tile">The NPC's default tile position within the <paramref name="locationName" />.</param>
	/// <param name="direction">The default facing direction.</param>
	/// <returns>Returns whether a valid home was found in the given character data.</returns>
	public static bool ReadNpcHomeData(CharacterData data, GameLocation currentLocation, out string locationName, out Point tile, out int direction)
	{
		if (data?.Home != null)
		{
			foreach (CharacterHomeData home in data.Home)
			{
				if (home.Condition == null || GameStateQuery.CheckConditions(home.Condition, currentLocation))
				{
					locationName = home.Location;
					tile = home.Tile;
					direction = (Utility.TryParseDirection(home.Direction, out var parsedDirection) ? parsedDirection : 0);
					return true;
				}
			}
		}
		locationName = "Town";
		tile = new Point(29, 67);
		direction = 2;
		return false;
	}

	public virtual bool canTalk()
	{
		return true;
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(birthday_Season, "birthday_Season").AddField(birthday_Day, "birthday_Day").AddField(datable, "datable")
			.AddField(shouldPlayRobinHammerAnimation, "shouldPlayRobinHammerAnimation")
			.AddField(shouldPlaySpousePatioAnimation, "shouldPlaySpousePatioAnimation")
			.AddField(isWalkingTowardPlayer, "isWalkingTowardPlayer")
			.AddField(moveTowardPlayerThreshold, "moveTowardPlayerThreshold")
			.AddField(age, "age")
			.AddField(manners, "manners")
			.AddField(socialAnxiety, "socialAnxiety")
			.AddField(optimism, "optimism")
			.AddField(gender, "gender")
			.AddField(breather, "breather")
			.AddField(isSleeping, "isSleeping")
			.AddField(hideShadow, "hideShadow")
			.AddField(isInvisible, "isInvisible")
			.AddField(defaultMap, "defaultMap")
			.AddField(defaultPosition, "defaultPosition")
			.AddField(removeHenchmanEvent, "removeHenchmanEvent")
			.AddField(doingEndOfRouteAnimation, "doingEndOfRouteAnimation")
			.AddField(goingToDoEndOfRouteAnimation, "goingToDoEndOfRouteAnimation")
			.AddField(endOfRouteMessage, "endOfRouteMessage")
			.AddField(endOfRouteBehaviorName, "endOfRouteBehaviorName")
			.AddField(lastSeenMovieWeek, "lastSeenMovieWeek")
			.AddField(currentMarriageDialogue, "currentMarriageDialogue")
			.AddField(marriageDefaultDialogue, "marriageDefaultDialogue")
			.AddField(shouldSayMarriageDialogue, "shouldSayMarriageDialogue")
			.AddField(hasBeenKissedToday, "hasBeenKissedToday")
			.AddField(hasSaidAfternoonDialogue, "hasSaidAfternoonDialogue")
			.AddField(dayScheduleName, "dayScheduleName")
			.AddField(islandScheduleName, "islandScheduleName")
			.AddField(sleptInBed, "sleptInBed")
			.AddField(shouldWearIslandAttire, "shouldWearIslandAttire")
			.AddField(isMovingOnPathFindPath, "isMovingOnPathFindPath");
		position.Field.AxisAlignedMovement = true;
		removeHenchmanEvent.onEvent += performRemoveHenchman;
	}

	/// <summary>Reload the NPC's sprite or portrait based on their character data within the current context.</summary>
	/// <param name="content">The content manager from which to load assets, or <c>null</c> for the default content manager.</param>
	public virtual void ChooseAppearance(LocalizedContentManager content = null)
	{
		LastAppearanceId = null;
		if (base.SimpleNonVillagerNPC)
		{
			return;
		}
		content = content ?? Game1.content;
		GameLocation location = base.currentLocation;
		if (location == null)
		{
			return;
		}
		LastLocationNameForAppearance = location.NameOrUniqueName;
		bool appliedLegacyUniquePortraits = false;
		if (location.TryGetMapProperty("UniquePortrait", out var uniquePortraitsProperty) && ArgUtility.SplitBySpace(uniquePortraitsProperty).Contains(base.Name))
		{
			string assetName = "Portraits\\" + getTextureName() + "_" + location.Name;
			appliedLegacyUniquePortraits = TryLoadPortraits(assetName, out var errorPhrase, content);
			if (!appliedLegacyUniquePortraits)
			{
				Game1.log.Warn($"NPC {base.Name} can't load portraits from '{assetName}' (per the {"UniquePortrait"} map property in '{location.NameOrUniqueName}'): {errorPhrase}. Falling back to default portraits.");
			}
		}
		bool appliedLegacyUniqueSprites = false;
		if (location.TryGetMapProperty("UniqueSprite", out var uniqueSpritesProperty) && ArgUtility.SplitBySpace(uniqueSpritesProperty).Contains(base.Name))
		{
			string assetName = "Characters\\" + getTextureName() + "_" + location.Name;
			appliedLegacyUniqueSprites = TryLoadSprites(assetName, out var errorPhrase, content);
			if (!appliedLegacyUniqueSprites)
			{
				Game1.log.Warn($"NPC {base.Name} can't load sprites from '{assetName}' (per the {"UniqueSprite"} map property in '{location.NameOrUniqueName}'): {errorPhrase}. Falling back to default sprites.");
			}
		}
		if (appliedLegacyUniquePortraits && appliedLegacyUniqueSprites)
		{
			return;
		}
		CharacterData data = null;
		CharacterAppearanceData appearance = null;
		if (!IsMonster)
		{
			data = GetData();
			if (data != null && data.Appearance?.Count > 0)
			{
				List<CharacterAppearanceData> possibleOptions = new List<CharacterAppearanceData>();
				int totalWeight = 0;
				Random random = Utility.CreateDaySaveRandom(Game1.hash.GetDeterministicHashCode(base.Name));
				Season season = location.GetSeason();
				bool isOutdoors = location.IsOutdoors;
				int precedence = int.MaxValue;
				foreach (CharacterAppearanceData option in data.Appearance)
				{
					if (option.Precedence > precedence || option.IsIslandAttire != isWearingIslandAttire)
					{
						continue;
					}
					Season? season2 = option.Season;
					if ((!season2.HasValue || option.Season.Value == season) && (isOutdoors ? option.Outdoors : option.Indoors) && GameStateQuery.CheckConditions(option.Condition, location, null, null, null, random))
					{
						if (option.Precedence < precedence)
						{
							precedence = option.Precedence;
							possibleOptions.Clear();
							totalWeight = 0;
						}
						possibleOptions.Add(option);
						totalWeight += option.Weight;
					}
				}
				switch (possibleOptions.Count)
				{
				case 1:
					appearance = possibleOptions[0];
					break;
				default:
				{
					appearance = possibleOptions[possibleOptions.Count - 1];
					int cursor = Utility.CreateDaySaveRandom(Game1.hash.GetDeterministicHashCode(base.Name)).Next(totalWeight + 1);
					foreach (CharacterAppearanceData option in possibleOptions)
					{
						cursor -= option.Weight;
						if (cursor <= 0)
						{
							appearance = option;
							break;
						}
					}
					break;
				}
				case 0:
					break;
				}
			}
		}
		if (!appliedLegacyUniquePortraits)
		{
			string defaultAsset = "Portraits/" + getTextureName();
			bool loaded = false;
			string errorPhrase;
			if (appearance != null && appearance.Portrait != null && appearance.Portrait != defaultAsset)
			{
				loaded = TryLoadPortraits(appearance.Portrait, out errorPhrase, content);
				if (!loaded)
				{
					Game1.log.Warn($"NPC {base.Name} can't load portraits from '{appearance.Portrait}' (per appearance entry '{appearance.Id}' in Data/Characters): {errorPhrase}. Falling back to default portraits.");
				}
			}
			if (!loaded && isWearingIslandAttire)
			{
				string beachAsset = defaultAsset + "_Beach";
				if (content.DoesAssetExist<Texture2D>(beachAsset))
				{
					loaded = TryLoadPortraits(beachAsset, out errorPhrase, content);
					if (!loaded)
					{
						Game1.log.Warn($"NPC {base.Name} can't load portraits from '{beachAsset}' for island attire: {errorPhrase}. Falling back to default portraits.");
					}
				}
			}
			if (!loaded && !TryLoadPortraits(defaultAsset, out errorPhrase, content))
			{
				Game1.log.Warn($"NPC {base.Name} can't load portraits from '{defaultAsset}': {errorPhrase}.");
			}
			if (loaded)
			{
				LastAppearanceId = appearance?.Id;
			}
		}
		if (!appliedLegacyUniqueSprites)
		{
			string defaultAsset = "Characters/" + getTextureName();
			bool loaded = false;
			string errorPhrase;
			if (appearance != null && appearance.Sprite != null && appearance.Sprite != defaultAsset)
			{
				loaded = TryLoadSprites(appearance.Sprite, out errorPhrase, content);
				if (!loaded)
				{
					Game1.log.Warn($"NPC {base.Name} can't load sprites from '{appearance.Sprite}' (per appearance entry '{appearance.Id}' in Data/Characters): {errorPhrase}. Falling back to default sprites.");
				}
			}
			if (!loaded && isWearingIslandAttire)
			{
				string beachAsset = defaultAsset + "_Beach";
				if (content.DoesAssetExist<Texture2D>(beachAsset))
				{
					loaded = TryLoadSprites(beachAsset, out errorPhrase, content);
					if (!loaded)
					{
						Game1.log.Warn($"NPC {base.Name} can't load sprites from '{beachAsset}' for island attire: {errorPhrase}. Falling back to default sprites.");
					}
				}
			}
			if (!loaded && !TryLoadSprites(defaultAsset, out errorPhrase, content))
			{
				Game1.log.Warn($"NPC {base.Name} can't load sprites from '{defaultAsset}': {errorPhrase}.");
			}
			if (loaded)
			{
				LastAppearanceId = appearance?.Id;
			}
		}
		if (data != null && Sprite != null)
		{
			Sprite.SpriteWidth = data.Size.X;
			Sprite.SpriteHeight = data.Size.Y;
			Sprite.ignoreSourceRectUpdates = false;
		}
	}

	protected override string translateName()
	{
		return GetDisplayName(name.Value);
	}

	public string getName()
	{
		if (displayName != null && displayName.Length > 0)
		{
			return displayName;
		}
		return base.Name;
	}

	public virtual string getTextureName()
	{
		return getTextureNameForCharacter(base.Name);
	}

	public static string getTextureNameForCharacter(string character_name)
	{
		TryGetData(character_name, out var data);
		string textureName = data?.TextureName;
		if (string.IsNullOrEmpty(textureName))
		{
			return character_name;
		}
		return textureName;
	}

	public void resetSeasonalDialogue()
	{
		dialogue = null;
	}

	public void performSpecialScheduleChanges()
	{
		if (Schedule == null || !base.Name.Equals("Pam") || !Game1.MasterPlayer.mailReceived.Contains("ccVault"))
		{
			return;
		}
		bool foundBus = false;
		foreach (KeyValuePair<int, SchedulePathDescription> v in Schedule)
		{
			if (v.Value.targetLocationName.Equals("BusStop"))
			{
				foundBus = true;
			}
			if (v.Value.targetLocationName.Equals("DesertFestival") || v.Value.targetLocationName.Equals("Desert") || v.Value.targetLocationName.Equals("IslandSouth"))
			{
				BusStop obj = Game1.getLocationFromName("BusStop") as BusStop;
				Game1.netWorldState.Value.canDriveYourselfToday.Value = true;
				Object sign = (Object)ItemRegistry.Create("(BC)TextSign");
				sign.signText.Value = Game1.content.LoadString(v.Value.targetLocationName.Equals("IslandSouth") ? "Strings\\1_6_Strings:Pam_busSign_resort" : "Strings\\1_6_Strings:Pam_busSign");
				sign.SpecialVariable = 987659;
				obj.tryPlaceObject(new Vector2(25f, 10f), sign);
				foundBus = true;
				break;
			}
		}
		if (!foundBus && !Game1.isGreenRain)
		{
			BusStop obj2 = Game1.getLocationFromName("BusStop") as BusStop;
			Game1.netWorldState.Value.canDriveYourselfToday.Value = true;
			Object sign = (Object)ItemRegistry.Create("(BC)TextSign");
			sign.signText.Value = Game1.content.LoadString("Strings\\1_6_Strings:Pam_busSign_generic");
			sign.SpecialVariable = 987659;
			obj2.tryPlaceObject(new Vector2(25f, 10f), sign);
		}
	}

	/// <summary>Update the NPC state (including sprite, dialogue, facing direction, schedules, etc). Despite the name, this doesn't only affect the sprite.</summary>
	/// <param name="onlyAppearance">Only reload the NPC's appearance (e.g. sprite, portraits, or breather/shadow fields), don't change any other data.</param>
	public virtual void reloadSprite(bool onlyAppearance = false)
	{
		if (base.SimpleNonVillagerNPC)
		{
			return;
		}
		ChooseAppearance();
		if (onlyAppearance || (!Game1.newDay && Game1.gameMode != 6))
		{
			return;
		}
		faceDirection(DefaultFacingDirection);
		previousEndPoint = new Point((int)defaultPosition.X / 64, (int)defaultPosition.Y / 64);
		TryLoadSchedule();
		performSpecialScheduleChanges();
		resetSeasonalDialogue();
		resetCurrentDialogue();
		updateConstructionAnimation();
		try
		{
			displayName = translateName();
		}
		catch (Exception)
		{
		}
	}

	/// <summary>Try to load a portraits texture, or keep the current texture if the load fails.</summary>
	/// <param name="assetName">The asset name to load.</param>
	/// <param name="error">If loading the portrait failed, an error phrase indicating why it failed.</param>
	/// <param name="content">The content manager from which to load the asset, or <c>null</c> for the default content manager.</param>
	/// <returns>Returns whether the texture was successfully loaded.</returns>
	public bool TryLoadPortraits(string assetName, out string error, LocalizedContentManager content = null)
	{
		if (base.Name == "Raccoon" || base.Name == "MrsRaccoon")
		{
			error = null;
			return true;
		}
		if (portraitOverridden)
		{
			error = null;
			return true;
		}
		if (string.IsNullOrWhiteSpace(assetName))
		{
			error = "the asset name is empty";
			return false;
		}
		if (portrait?.Name == assetName && !portrait.IsDisposed)
		{
			error = null;
			return true;
		}
		if (content == null)
		{
			content = Game1.content;
		}
		try
		{
			portrait = content.Load<Texture2D>(assetName);
			portrait.Name = assetName;
			error = null;
			return true;
		}
		catch (Exception ex)
		{
			error = ex.ToString();
			return false;
		}
	}

	/// <summary>Try to load a sprite texture, or keep the current texture if the load fails.</summary>
	/// <param name="assetName">The asset name to load.</param>
	/// <param name="error">If loading the portrait failed, an error phrase indicating why it failed.</param>
	/// <param name="content">The content manager from which to load the asset, or <c>null</c> for the default content manager.</param>
	/// <param name="logOnFail">Whether to log a warning if the texture can't be loaded.</param>
	/// <returns>Returns whether the texture was successfully loaded.</returns>
	public bool TryLoadSprites(string assetName, out string error, LocalizedContentManager content = null)
	{
		if (spriteOverridden)
		{
			error = null;
			return true;
		}
		if (string.IsNullOrWhiteSpace(assetName))
		{
			error = "the asset name is empty";
			return false;
		}
		if (Sprite?.spriteTexture != null && ((Sprite.overrideTextureName ?? Sprite.textureName.Value) == assetName || Sprite.spriteTexture.Name == assetName) && !Sprite.spriteTexture.IsDisposed)
		{
			error = null;
			return true;
		}
		if (content == null)
		{
			content = Game1.content;
		}
		try
		{
			if (Sprite == null)
			{
				Sprite = new AnimatedSprite(content, assetName);
			}
			else
			{
				Sprite.LoadTexture(assetName, Game1.IsMasterGame);
			}
			error = null;
			return true;
		}
		catch (Exception ex)
		{
			error = ex.ToString();
			return false;
		}
	}

	private void updateConstructionAnimation()
	{
		bool isFestivalDay = Utility.isFestivalDay();
		if (Game1.IsMasterGame && base.Name == "Robin" && !isFestivalDay)
		{
			if ((int)Game1.player.daysUntilHouseUpgrade > 0)
			{
				Farm farm = Game1.getFarm();
				Game1.warpCharacter(this, farm.NameOrUniqueName, new Vector2(farm.GetMainFarmHouseEntry().X + 4, farm.GetMainFarmHouseEntry().Y - 1));
				isPlayingRobinHammerAnimation = false;
				shouldPlayRobinHammerAnimation.Value = true;
				return;
			}
			if (Game1.IsThereABuildingUnderConstruction())
			{
				Building b = Game1.GetBuildingUnderConstruction();
				GameLocation indoors = b.GetIndoors();
				if ((int)b.daysUntilUpgrade > 0 && indoors != null)
				{
					base.currentLocation?.characters.Remove(this);
					base.currentLocation = indoors;
					if (base.currentLocation != null && !base.currentLocation.characters.Contains(this))
					{
						base.currentLocation.addCharacter(this);
					}
					string indoorsName = b.GetIndoorsName();
					if (indoorsName != null && indoorsName.StartsWith("Shed"))
					{
						setTilePosition(2, 2);
						position.X -= 28f;
					}
					else
					{
						setTilePosition(1, 5);
					}
				}
				else
				{
					Game1.warpCharacter(this, b.parentLocationName.Value, new Vector2((int)b.tileX + (int)b.tilesWide / 2, (int)b.tileY + (int)b.tilesHigh / 2));
					position.X += 16f;
					position.Y -= 32f;
				}
				isPlayingRobinHammerAnimation = false;
				shouldPlayRobinHammerAnimation.Value = true;
				return;
			}
			if (Game1.RequireLocation<Town>("Town").daysUntilCommunityUpgrade.Value > 0)
			{
				if (Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
				{
					Game1.warpCharacter(this, "Backwoods", new Vector2(41f, 23f));
					isPlayingRobinHammerAnimation = false;
					shouldPlayRobinHammerAnimation.Value = true;
				}
				else if (Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
				{
					Game1.warpCharacter(this, "Town", new Vector2(77f, 68f));
					isPlayingRobinHammerAnimation = false;
					shouldPlayRobinHammerAnimation.Value = true;
				}
				return;
			}
		}
		shouldPlayRobinHammerAnimation.Value = false;
	}

	private void doPlayRobinHammerAnimation()
	{
		Sprite.ClearAnimation();
		Sprite.AddFrame(new FarmerSprite.AnimationFrame(24, 75));
		Sprite.AddFrame(new FarmerSprite.AnimationFrame(25, 75));
		Sprite.AddFrame(new FarmerSprite.AnimationFrame(26, 300, secondaryArm: false, flip: false, robinHammerSound));
		Sprite.AddFrame(new FarmerSprite.AnimationFrame(27, 1000, secondaryArm: false, flip: false, robinVariablePause));
		ignoreScheduleToday = true;
		bool oneDayLeft = (int)Game1.player.daysUntilHouseUpgrade == 1 || (int)Game1.RequireLocation<Town>("Town").daysUntilCommunityUpgrade == 1;
		CurrentDialogue.Clear();
		CurrentDialogue.Push(new Dialogue(this, oneDayLeft ? "Strings\\StringsFromCSFiles:NPC.cs.3927" : "Strings\\StringsFromCSFiles:NPC.cs.3926"));
	}

	public void showTextAboveHead(string text, Color? spriteTextColor = null, int style = 2, int duration = 3000, int preTimer = 0)
	{
		if (!IsInvisible)
		{
			textAboveHeadAlpha = 0f;
			textAboveHead = StardewValley.Dialogue.applyGenderSwitchBlocks(Game1.player.Gender, text);
			textAboveHeadPreTimer = preTimer;
			textAboveHeadTimer = duration;
			textAboveHeadStyle = style;
			textAboveHeadColor = spriteTextColor;
		}
	}

	public virtual bool hitWithTool(Tool t)
	{
		return false;
	}

	/// <summary>Get whether this NPC can receive gifts from the player (regardless of whether they've already received one today).</summary>
	public bool CanReceiveGifts()
	{
		if (CanSocialize && !base.SimpleNonVillagerNPC && Game1.NPCGiftTastes.ContainsKey(base.Name))
		{
			return GetData()?.CanReceiveGifts ?? true;
		}
		return false;
	}

	/// <summary>Get how much the NPC likes receiving an item as a gift.</summary>
	/// <param name="item">The item to check.</param>
	/// <returns>Returns one of <see cref="F:StardewValley.NPC.gift_taste_hate" />, <see cref="F:StardewValley.NPC.gift_taste_dislike" />, <see cref="F:StardewValley.NPC.gift_taste_neutral" />, <see cref="F:StardewValley.NPC.gift_taste_like" />, or <see cref="F:StardewValley.NPC.gift_taste_love" />.</returns>
	public int getGiftTasteForThisItem(Item item)
	{
		if (item.QualifiedItemId == "(O)StardropTea")
		{
			return 7;
		}
		int tasteForItem = 8;
		if (item is Object { Category: var categoryNumber } obj)
		{
			string categoryNumberString = categoryNumber.ToString() ?? "";
			string[] universalLoves = ArgUtility.SplitBySpace(Game1.NPCGiftTastes["Universal_Love"]);
			string[] universalHates = ArgUtility.SplitBySpace(Game1.NPCGiftTastes["Universal_Hate"]);
			string[] universalLikes = ArgUtility.SplitBySpace(Game1.NPCGiftTastes["Universal_Like"]);
			string[] universalDislikes = ArgUtility.SplitBySpace(Game1.NPCGiftTastes["Universal_Dislike"]);
			string[] universalNeutrals = ArgUtility.SplitBySpace(Game1.NPCGiftTastes["Universal_Neutral"]);
			if (universalLoves.Contains(categoryNumberString))
			{
				tasteForItem = 0;
			}
			else if (universalHates.Contains(categoryNumberString))
			{
				tasteForItem = 6;
			}
			else if (universalLikes.Contains(categoryNumberString))
			{
				tasteForItem = 2;
			}
			else if (universalDislikes.Contains(categoryNumberString))
			{
				tasteForItem = 4;
			}
			if (CheckTasteContextTags(obj, universalLoves))
			{
				tasteForItem = 0;
			}
			else if (CheckTasteContextTags(obj, universalHates))
			{
				tasteForItem = 6;
			}
			else if (CheckTasteContextTags(obj, universalLikes))
			{
				tasteForItem = 2;
			}
			else if (CheckTasteContextTags(obj, universalDislikes))
			{
				tasteForItem = 4;
			}
			bool wasIndividualUniversal = false;
			bool skipDefaultValueRules = false;
			if (CheckTaste(universalLoves, obj))
			{
				tasteForItem = 0;
				wasIndividualUniversal = true;
			}
			else if (CheckTaste(universalHates, obj))
			{
				tasteForItem = 6;
				wasIndividualUniversal = true;
			}
			else if (CheckTaste(universalLikes, obj))
			{
				tasteForItem = 2;
				wasIndividualUniversal = true;
			}
			else if (CheckTaste(universalDislikes, obj))
			{
				tasteForItem = 4;
				wasIndividualUniversal = true;
			}
			else if (CheckTaste(universalNeutrals, obj))
			{
				tasteForItem = 8;
				wasIndividualUniversal = true;
				skipDefaultValueRules = true;
			}
			if (obj.Type == "Arch")
			{
				tasteForItem = 4;
				if (base.Name.Equals("Penny") || name.Equals("Dwarf"))
				{
					tasteForItem = 2;
				}
			}
			if (tasteForItem == 8 && !skipDefaultValueRules)
			{
				if ((int)obj.edibility != -300 && (int)obj.edibility < 0)
				{
					tasteForItem = 6;
				}
				else if ((int)obj.price < 20)
				{
					tasteForItem = 4;
				}
			}
			if (Game1.NPCGiftTastes.TryGetValue(base.Name, out var dispositionData))
			{
				string[] split = dispositionData.Split('/');
				List<string[]> items = new List<string[]>();
				for (int i = 0; i < 10; i += 2)
				{
					string[] splitItems = ArgUtility.SplitBySpace(split[i + 1]);
					string[] thisItems = new string[splitItems.Length];
					for (int j = 0; j < splitItems.Length; j++)
					{
						if (splitItems[j].Length > 0)
						{
							thisItems[j] = splitItems[j];
						}
					}
					items.Add(thisItems);
				}
				if (CheckTaste(items[0], obj))
				{
					return 0;
				}
				if (CheckTaste(items[3], obj))
				{
					return 6;
				}
				if (CheckTaste(items[1], obj))
				{
					return 2;
				}
				if (CheckTaste(items[2], obj))
				{
					return 4;
				}
				if (CheckTaste(items[4], obj))
				{
					return 8;
				}
				if (CheckTasteContextTags(obj, items[0]))
				{
					return 0;
				}
				if (CheckTasteContextTags(obj, items[3]))
				{
					return 6;
				}
				if (CheckTasteContextTags(obj, items[1]))
				{
					return 2;
				}
				if (CheckTasteContextTags(obj, items[2]))
				{
					return 4;
				}
				if (CheckTasteContextTags(obj, items[4]))
				{
					return 8;
				}
				if (!wasIndividualUniversal)
				{
					if (categoryNumber != 0 && items[0].Contains(categoryNumberString))
					{
						return 0;
					}
					if (categoryNumber != 0 && items[3].Contains(categoryNumberString))
					{
						return 6;
					}
					if (categoryNumber != 0 && items[1].Contains(categoryNumberString))
					{
						return 2;
					}
					if (categoryNumber != 0 && items[2].Contains(categoryNumberString))
					{
						return 4;
					}
					if (categoryNumber != 0 && items[4].Contains(categoryNumberString))
					{
						return 8;
					}
				}
			}
		}
		return tasteForItem;
	}

	public bool CheckTaste(IEnumerable<string> list, Item item)
	{
		foreach (string item_entry in list)
		{
			if (item_entry != null && !item_entry.StartsWith('-'))
			{
				ParsedItemData data = ItemRegistry.GetData(item_entry);
				if (data?.ItemType != null && item.QualifiedItemId == data.QualifiedItemId)
				{
					return true;
				}
			}
		}
		return false;
	}

	public virtual bool CheckTasteContextTags(Item item, string[] list)
	{
		foreach (string entry in list)
		{
			if (entry != null && entry.Length > 0 && !char.IsNumber(entry[0]) && entry[0] != '-' && item.HasContextTag(entry))
			{
				return true;
			}
		}
		return false;
	}

	private void goblinDoorEndBehavior(Character c, GameLocation l)
	{
		l.characters.Remove(this);
		l.playSound("doorClose");
	}

	private void performRemoveHenchman()
	{
		Sprite.CurrentFrame = 4;
		Game1.netWorldState.Value.IsGoblinRemoved = true;
		Game1.player.removeQuest("27");
		Stack<Point> p = new Stack<Point>();
		p.Push(new Point(20, 21));
		p.Push(new Point(20, 22));
		p.Push(new Point(20, 23));
		p.Push(new Point(20, 24));
		p.Push(new Point(20, 25));
		p.Push(new Point(20, 26));
		p.Push(new Point(20, 27));
		p.Push(new Point(20, 28));
		addedSpeed = 2f;
		controller = new PathFindController(p, this, base.currentLocation);
		controller.endBehaviorFunction = goblinDoorEndBehavior;
		showTextAboveHead(Game1.content.LoadString("Strings\\Characters:Henchman6"));
		Game1.player.mailReceived.Add("henchmanGone");
		base.currentLocation.removeTile(20, 29, "Buildings");
	}

	private void engagementResponse(Farmer who, bool asRoommate = false)
	{
		Game1.changeMusicTrack("silence");
		who.spouse = base.Name;
		if (!asRoommate)
		{
			Game1.multiplayer.globalChatInfoMessage("Engaged", Game1.player.Name, GetTokenizedDisplayName());
		}
		Friendship friendship = who.friendshipData[base.Name];
		friendship.Status = FriendshipStatus.Engaged;
		friendship.RoommateMarriage = asRoommate;
		WorldDate weddingDate = new WorldDate(Game1.Date);
		weddingDate.TotalDays += 3;
		who.removeDatingActiveDialogueEvents(Game1.player.spouse);
		while (!Game1.canHaveWeddingOnDay(weddingDate.DayOfMonth, weddingDate.Season))
		{
			weddingDate.TotalDays++;
		}
		friendship.WeddingDate = weddingDate;
		CurrentDialogue.Clear();
		if (asRoommate && DataLoader.EngagementDialogue(Game1.content).ContainsKey(base.Name + "Roommate0"))
		{
			CurrentDialogue.Push(new Dialogue(this, "Data\\EngagementDialogue:" + base.Name + "Roommate0"));
			Dialogue attemptDialogue = StardewValley.Dialogue.TryGetDialogue(this, "Strings\\StringsFromCSFiles:" + base.Name + "_EngagedRoommate");
			if (attemptDialogue != null)
			{
				CurrentDialogue.Push(attemptDialogue);
			}
			else
			{
				attemptDialogue = StardewValley.Dialogue.TryGetDialogue(this, "Strings\\StringsFromCSFiles:" + base.Name + "_Engaged");
				if (attemptDialogue != null)
				{
					CurrentDialogue.Push(attemptDialogue);
				}
				else
				{
					CurrentDialogue.Push(new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.3980"));
				}
			}
		}
		else
		{
			Dialogue attemptDialogue = StardewValley.Dialogue.TryGetDialogue(this, "Data\\EngagementDialogue:" + base.Name + "0");
			if (attemptDialogue != null)
			{
				CurrentDialogue.Push(attemptDialogue);
			}
			attemptDialogue = StardewValley.Dialogue.TryGetDialogue(this, "Strings\\StringsFromCSFiles:" + base.Name + "_Engaged");
			if (attemptDialogue != null)
			{
				CurrentDialogue.Push(attemptDialogue);
			}
			else
			{
				CurrentDialogue.Push(new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.3980"));
			}
		}
		Dialogue obj = CurrentDialogue.Peek();
		obj.onFinish = (Action)Delegate.Combine(obj.onFinish, (Action)delegate
		{
			Game1.changeMusicTrack("none", track_interruptable: true);
			GameLocation.HandleMusicChange(null, who.currentLocation);
		});
		who.changeFriendship(1, this);
		who.reduceActiveItemByOne();
		who.completelyStopAnimatingOrDoingAction();
		Game1.drawDialogue(this);
	}

	/// <summary>Try to receive an item from the player.</summary>
	/// <param name="who">The player whose active object to receive.</param>
	/// <param name="probe">Whether to return what the method would return if called normally, but without actually accepting the item or making any changes to the NPC. This is used to accurately predict whether the NPC would accept or react to the offer.</param>
	/// <returns>Returns true if the NPC accepted the item or reacted to the offer, else false.</returns>
	public virtual bool tryToReceiveActiveObject(Farmer who, bool probe = false)
	{
		if (base.SimpleNonVillagerNPC)
		{
			return false;
		}
		Object activeObj = who.ActiveObject;
		if (activeObj == null)
		{
			return false;
		}
		if (!probe)
		{
			who.Halt();
			who.faceGeneralDirection(getStandingPosition(), 0, opposite: false, useTileCalculations: false);
		}
		if (base.Name == "Henchman" && Game1.currentLocation.NameOrUniqueName == "WitchSwamp")
		{
			if (activeObj.QualifiedItemId == "(O)308")
			{
				if (controller != null)
				{
					return false;
				}
				if (!probe)
				{
					who.currentLocation.localSound("coin");
					who.reduceActiveItemByOne();
					CurrentDialogue.Push(new Dialogue(this, "Strings\\Characters:Henchman5"));
					Game1.drawDialogue(this);
					who.freezePause = 2000;
					removeHenchmanEvent.Fire();
				}
			}
			else if (!probe)
			{
				CurrentDialogue.Push(new Dialogue(this, (activeObj.QualifiedItemId == "(O)684") ? "Strings\\Characters:Henchman4" : "Strings\\Characters:Henchman3"));
				Game1.drawDialogue(this);
			}
			return true;
		}
		if (Game1.player.team.specialOrders != null)
		{
			foreach (SpecialOrder order in Game1.player.team.specialOrders)
			{
				if (order.onItemDelivered == null)
				{
					continue;
				}
				Delegate[] invocationList = order.onItemDelivered.GetInvocationList();
				for (int i = 0; i < invocationList.Length; i++)
				{
					if (((Func<Farmer, NPC, Item, bool, int>)invocationList[i])(Game1.player, this, activeObj, probe) > 0)
					{
						if (!probe && activeObj.Stack <= 0)
						{
							who.ActiveObject = null;
							who.showNotCarrying();
						}
						return true;
					}
				}
			}
		}
		Quest questOfTheDay = Game1.questOfTheDay;
		if (!(questOfTheDay is ItemDeliveryQuest deliveryQuest))
		{
			if (questOfTheDay is FishingQuest fishingQuest && fishingQuest.checkIfComplete(this, -1, 1, null, activeObj.ItemId, probe))
			{
				if (!probe)
				{
					who.reduceActiveItemByOne();
					who.completelyStopAnimatingOrDoingAction();
					if (Game1.random.NextDouble() < 0.3 && base.Name != "Wizard")
					{
						doEmote(32);
					}
				}
				return true;
			}
		}
		else if ((bool)deliveryQuest.accepted && !deliveryQuest.completed && deliveryQuest.checkIfComplete(this, -1, -1, activeObj, null, probe))
		{
			if (!probe)
			{
				who.reduceActiveItemByOne();
				who.completelyStopAnimatingOrDoingAction();
				if (Game1.random.NextDouble() < 0.3 && base.Name != "Wizard")
				{
					doEmote(32);
				}
			}
			return true;
		}
		switch (who.ActiveObject?.QualifiedItemId)
		{
		case "(O)233":
			if (name == "Jas" && Utility.GetDayOfPassiveFestival("DesertFestival") > 0 && base.currentLocation is Desert && !who.mailReceived.Contains("Jas_IceCream_DF_" + Game1.year))
			{
				if (!probe)
				{
					who.reduceActiveItemByOne();
					jump();
					doEmote(16);
					CurrentDialogue.Clear();
					setNewDialogue("Strings\\1_6_Strings:Jas_IceCream", add: true);
					Game1.drawDialogue(this);
					who.mailReceived.Add("Jas_IceCream_DF_" + Game1.year);
					who.changeFriendship(200, this);
				}
				return true;
			}
			break;
		case "(O)897":
			if (!probe)
			{
				if (base.Name == "Pierre" && !Game1.player.hasOrWillReceiveMail("PierreStocklist"))
				{
					Game1.addMail("PierreStocklist", noLetter: true, sendToEveryone: true);
					who.reduceActiveItemByOne();
					who.completelyStopAnimatingOrDoingAction();
					who.currentLocation.localSound("give_gift");
					Game1.player.team.itemsToRemoveOvernight.Add("897");
					setNewDialogue("Strings\\Characters:PierreStockListDialogue", add: true);
					Game1.drawDialogue(this);
					Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
					{
						Game1.multiplayer.globalChatInfoMessage("StockList");
					});
				}
				else
				{
					Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_NoTheater", displayName)));
				}
			}
			return true;
		case "(O)71":
			if (base.Name == "Lewis" && who.hasQuest("102"))
			{
				if (!probe)
				{
					if (who.currentLocation?.NameOrUniqueName == "IslandSouth")
					{
						Game1.player.activeDialogueEvents["lucky_pants_lewis"] = 28;
					}
					who.completeQuest("102");
					string[] questFields = Quest.GetRawQuestFields("102");
					Dialogue thankYou = new Dialogue(this, null, ArgUtility.Get(questFields, 9, "Data\\ExtraDialogue:LostItemQuest_DefaultThankYou", allowBlank: false));
					setNewDialogue(thankYou);
					Game1.drawDialogue(this);
					Game1.player.changeFriendship(250, this);
					who.ActiveObject = null;
				}
				return true;
			}
			return false;
		}
		Dialogue dialogue;
		if (activeObj.HasTypeObject())
		{
			dialogue = TryGetDialogue("reject_" + activeObj.ItemId);
			if (dialogue != null)
			{
				if (!probe)
				{
					setNewDialogue(dialogue);
					Game1.drawDialogue(this);
				}
				return true;
			}
		}
		if ((bool)activeObj.questItem)
		{
			if (who.hasQuest("130") && activeObj.HasTypeObject())
			{
				dialogue = TryGetDialogue("accept_" + activeObj.ItemId);
				if (dialogue != null)
				{
					if (!probe)
					{
						setNewDialogue(dialogue);
						Game1.drawDialogue(this);
						CurrentDialogue.Peek().onFinish = delegate
						{
							Object o = ItemRegistry.Create<Object>("(O)" + (activeObj.ParentSheetIndex + 1));
							o.specialItem = true;
							o.questItem.Value = true;
							who.reduceActiveItemByOne();
							DelayedAction.playSoundAfterDelay("coin", 200);
							DelayedAction.functionAfterDelay(delegate
							{
								who.addItemByMenuIfNecessary(o);
							}, 200);
							Game1.player.freezePause = 550;
							DelayedAction.functionAfterDelay(delegate
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1919", o.DisplayName, Lexicon.getProperArticleForWord(o.DisplayName)));
							}, 550);
						};
					}
					return true;
				}
			}
			if (!who.checkForQuestComplete(this, -1, -1, activeObj, "", 9, 3, probe) && name != "Birdie")
			{
				if (!probe)
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3954"));
				}
				return true;
			}
			return false;
		}
		if (who.checkForQuestComplete(this, -1, -1, null, "", 10, -1, probe))
		{
			return true;
		}
		dialogue = TryGetDialogue("RejectItem_" + activeObj.QualifiedItemId) ?? (from tag in activeObj.GetContextTags()
			select TryGetDialogue("RejectItem_" + tag)).FirstOrDefault((Dialogue p) => p != null);
		if (dialogue != null)
		{
			if (!probe)
			{
				setNewDialogue(dialogue);
				Game1.drawDialogue(this);
			}
			return true;
		}
		who.friendshipData.TryGetValue(base.Name, out var friendship);
		bool canReceiveGifts = CanReceiveGifts();
		switch (activeObj.QualifiedItemId)
		{
		case "(O)809":
			if (!Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccMovieTheater"))
			{
				if (!probe)
				{
					Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_NoTheater", displayName)));
				}
				return true;
			}
			if (SpeaksDwarvish() && !who.canUnderstandDwarves)
			{
				if (!probe)
				{
					Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_NoTheater", displayName)));
				}
				return true;
			}
			if (base.Name == "Krobus" && Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth) == "Fri")
			{
				if (!probe)
				{
					Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_NoTheater", displayName)));
				}
				return true;
			}
			if (base.Name == "Leo" && !Game1.MasterPlayer.mailReceived.Contains("leoMoved"))
			{
				if (!probe)
				{
					Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_NoTheater", displayName)));
				}
				return true;
			}
			if (!IsVillager || !CanSocialize)
			{
				if (!probe)
				{
					Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_CantInvite", displayName)));
				}
				return true;
			}
			if (friendship == null)
			{
				if (!probe)
				{
					Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_NoTheater", displayName)));
				}
				return true;
			}
			if (friendship.IsDivorced())
			{
				if (!probe)
				{
					if (who == Game1.player)
					{
						Game1.multiplayer.globalChatInfoMessage("MovieInviteReject", Game1.player.displayName, GetTokenizedDisplayName());
					}
					CurrentDialogue.Push(TryGetDialogue("RejectMovieTicket_Divorced") ?? TryGetDialogue("RejectMovieTicket") ?? new Dialogue(this, "Strings\\Characters:Divorced_gift"));
					Game1.drawDialogue(this);
				}
				return true;
			}
			if (who.lastSeenMovieWeek.Value >= Game1.Date.TotalWeeks)
			{
				if (!probe)
				{
					Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_FarmerAlreadySeen")));
				}
				return true;
			}
			if (Utility.isFestivalDay())
			{
				if (!probe)
				{
					Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_Festival")));
				}
				return true;
			}
			if (Game1.timeOfDay > 2100)
			{
				if (!probe)
				{
					Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_Closed")));
				}
				return true;
			}
			foreach (MovieInvitation invitation in who.team.movieInvitations)
			{
				if (invitation.farmer == who)
				{
					if (!probe)
					{
						Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_AlreadyInvitedSomeone", invitation.invitedNPC.displayName)));
					}
					return true;
				}
			}
			if (!probe)
			{
				faceTowardFarmerForPeriod(4000, 3, faceAway: false, who);
			}
			foreach (MovieInvitation invitation in who.team.movieInvitations)
			{
				if (invitation.invitedNPC != this)
				{
					continue;
				}
				if (!probe)
				{
					if (who == Game1.player)
					{
						Game1.multiplayer.globalChatInfoMessage("MovieInviteReject", Game1.player.displayName, GetTokenizedDisplayName());
					}
					CurrentDialogue.Push(TryGetDialogue("RejectMovieTicket_AlreadyInvitedBySomeoneElse", invitation.farmer.displayName) ?? TryGetDialogue("RejectMovieTicket") ?? new Dialogue(this, "Strings\\Characters:MovieInvite_InvitedBySomeoneElse", GetDispositionModifiedString("Strings\\Characters:MovieInvite_InvitedBySomeoneElse", invitation.farmer.displayName)));
					Game1.drawDialogue(this);
				}
				return true;
			}
			if (lastSeenMovieWeek.Value >= Game1.Date.TotalWeeks)
			{
				if (!probe)
				{
					if (who == Game1.player)
					{
						Game1.multiplayer.globalChatInfoMessage("MovieInviteReject", Game1.player.displayName, GetTokenizedDisplayName());
					}
					CurrentDialogue.Push(TryGetDialogue("RejectMovieTicket_AlreadyWatchedThisWeek") ?? TryGetDialogue("RejectMovieTicket") ?? new Dialogue(this, "Strings\\Characters:MovieInvite_AlreadySeen", GetDispositionModifiedString("Strings\\Characters:MovieInvite_AlreadySeen")));
					Game1.drawDialogue(this);
				}
				return true;
			}
			if (MovieTheater.GetResponseForMovie(this) == "reject")
			{
				if (!probe)
				{
					if (who == Game1.player)
					{
						Game1.multiplayer.globalChatInfoMessage("MovieInviteReject", Game1.player.displayName, GetTokenizedDisplayName());
					}
					CurrentDialogue.Push(TryGetDialogue("RejectMovieTicket_DontWantToSeeThatMovie") ?? TryGetDialogue("RejectMovieTicket") ?? new Dialogue(this, "Strings\\Characters:MovieInvite_Reject", GetDispositionModifiedString("Strings\\Characters:MovieInvite_Reject")));
					Game1.drawDialogue(this);
				}
				return true;
			}
			if (!probe)
			{
				CurrentDialogue.Push(((getSpouse() == who) ? StardewValley.Dialogue.TryGetDialogue(this, "Strings\\Characters:MovieInvite_Spouse_" + name) : null) ?? TryGetDialogue("MovieInvitation") ?? new Dialogue(this, "Strings\\Characters:MovieInvite_Invited", GetDispositionModifiedString("Strings\\Characters:MovieInvite_Invited")));
				Game1.drawDialogue(this);
				who.reduceActiveItemByOne();
				who.completelyStopAnimatingOrDoingAction();
				who.currentLocation.localSound("give_gift");
				MovieTheater.Invite(who, this);
				if (who == Game1.player)
				{
					Game1.multiplayer.globalChatInfoMessage("MovieInviteAccept", Game1.player.displayName, GetTokenizedDisplayName());
				}
			}
			return true;
		case "(O)458":
			if (canReceiveGifts)
			{
				if (!probe)
				{
					bool npcMarriedToSomeoneElse = who.spouse != base.Name && isMarriedOrEngaged();
					if (!datable.Value || npcMarriedToSomeoneElse)
					{
						if (Game1.random.NextBool())
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3955", displayName));
						}
						else
						{
							CurrentDialogue.Push(((!datable.Value) ? TryGetDialogue("RejectBouquet_NotDatable") : null) ?? (npcMarriedToSomeoneElse ? TryGetDialogue("RejectBouquet_NpcAlreadyMarried", getSpouse()?.Name) : null) ?? TryGetDialogue("RejectBouquet") ?? (Game1.random.NextBool() ? new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.3956") : new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.3957", isGendered: true)));
							Game1.drawDialogue(this);
						}
					}
					else
					{
						if (friendship == null)
						{
							friendship = (who.friendshipData[base.Name] = new Friendship());
						}
						if (friendship.IsDating())
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:AlreadyDatingBouquet", displayName));
						}
						else if (friendship.IsDivorced())
						{
							CurrentDialogue.Push(TryGetDialogue("RejectBouquet_Divorced") ?? TryGetDialogue("RejectBouquet") ?? new Dialogue(this, "Strings\\Characters:Divorced_bouquet"));
							Game1.drawDialogue(this);
						}
						else if (friendship.Points < 1000)
						{
							CurrentDialogue.Push(TryGetDialogue("RejectBouquet_VeryLowHearts") ?? TryGetDialogue("RejectBouquet") ?? (Game1.random.NextBool() ? new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.3958") : new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.3959", isGendered: true)));
							Game1.drawDialogue(this);
						}
						else if (friendship.Points < 2000)
						{
							CurrentDialogue.Push(TryGetDialogue("RejectBouquet_LowHearts") ?? TryGetDialogue("RejectBouquet") ?? new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs." + Game1.random.Choose("3960", "3961")));
							Game1.drawDialogue(this);
						}
						else
						{
							friendship.Status = FriendshipStatus.Dating;
							Game1.multiplayer.globalChatInfoMessage("Dating", Game1.player.Name, GetTokenizedDisplayName());
							CurrentDialogue.Push(TryGetDialogue("AcceptBouquet") ?? new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs." + Game1.random.Choose("3962", "3963"), isGendered: true));
							who.autoGenerateActiveDialogueEvent("dating_" + base.Name);
							who.autoGenerateActiveDialogueEvent("dating");
							who.changeFriendship(25, this);
							who.reduceActiveItemByOne();
							who.completelyStopAnimatingOrDoingAction();
							doEmote(20);
							Game1.drawDialogue(this);
						}
					}
				}
				return true;
			}
			return false;
		case "(O)277":
			if (canReceiveGifts)
			{
				if (!probe)
				{
					if (!datable || friendship == null || !friendship.IsDating() || (friendship != null && friendship.IsMarried()))
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Wilted_Bouquet_Meaningless", displayName));
					}
					else
					{
						Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Wilted_Bouquet_Effect", displayName));
						Game1.multiplayer.globalChatInfoMessage("BreakUp", Game1.player.Name, GetTokenizedDisplayName());
						who.removeDatingActiveDialogueEvents(base.Name);
						who.reduceActiveItemByOne();
						friendship.Status = FriendshipStatus.Friendly;
						if (who.spouse == base.Name)
						{
							who.spouse = null;
						}
						friendship.WeddingDate = null;
						who.completelyStopAnimatingOrDoingAction();
						friendship.Points = Math.Min(friendship.Points, 1250);
						switch ((string)name)
						{
						case "Maru":
						case "Haley":
							doEmote(12);
							break;
						default:
							doEmote(28);
							break;
						case "Shane":
						case "Alex":
							break;
						}
						CurrentDialogue.Clear();
						CurrentDialogue.Push(new Dialogue(this, "Characters\\Dialogue\\" + GetDialogueSheetName() + ":breakUp"));
						Game1.drawDialogue(this);
					}
				}
				return true;
			}
			return false;
		case "(O)460":
			if (canReceiveGifts)
			{
				if (!probe)
				{
					bool isDivorced = friendship?.IsDivorced() ?? false;
					if (who.isMarriedOrRoommates() || who.isEngaged())
					{
						if (who.hasCurrentOrPendingRoommate())
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:TriedToMarryButKrobus"));
						}
						else if (who.isEngaged())
						{
							CurrentDialogue.Push(TryGetDialogue("RejectMermaidPendant_PlayerWithSomeoneElse", who.getSpouse()?.displayName ?? who.spouse) ?? TryGetDialogue("RejectMermaidPendant") ?? new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs." + Game1.random.Choose("3965", "3966"), isGendered: true));
							Game1.drawDialogue(this);
						}
						else
						{
							CurrentDialogue.Push(TryGetDialogue("RejectMermaidPendant_PlayerWithSomeoneElse") ?? TryGetDialogue("RejectMermaidPendant") ?? (Game1.random.NextBool() ? new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.3967") : new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.3968", isGendered: true)));
							Game1.drawDialogue(this);
						}
					}
					else if (!datable || isMarriedOrEngaged() || isDivorced || (friendship != null && friendship.Points < 1500))
					{
						if (Game1.random.NextBool())
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3969", displayName));
						}
						else
						{
							CurrentDialogue.Push(((!datable.Value) ? TryGetDialogue("RejectMermaidPendant_NotDatable") : null) ?? (isDivorced ? TryGetDialogue("RejectMermaidPendant_Divorced") : null) ?? (isMarriedOrEngaged() ? TryGetDialogue("RejectMermaidPendant_NpcWithSomeoneElse", getSpouse()?.Name) : null) ?? ((datable.Value && friendship != null && friendship.Points < 1500) ? TryGetDialogue("RejectMermaidPendant_Under8Hearts") : null) ?? TryGetDialogue("RejectMermaidPendant") ?? new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs." + ((Gender == Gender.Female) ? "3970" : "3971")));
							Game1.drawDialogue(this);
						}
					}
					else if ((bool)datable && friendship != null && friendship.Points < 2500)
					{
						if (!friendship.ProposalRejected)
						{
							CurrentDialogue.Push(TryGetDialogue("RejectMermaidPendant_Under10Hearts") ?? new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs." + Game1.random.Choose("3972", "3973")));
							Game1.drawDialogue(this);
							who.changeFriendship(-20, this);
							friendship.ProposalRejected = true;
						}
						else
						{
							CurrentDialogue.Push(TryGetDialogue("RejectMermaidPendant_Under10Hearts_AskedAgain") ?? TryGetDialogue("RejectMermaidPendant_Under10Hearts") ?? TryGetDialogue("RejectMermaidPendant") ?? new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs." + Game1.random.Choose("3974", "3975"), isGendered: true));
							Game1.drawDialogue(this);
							who.changeFriendship(-50, this);
						}
					}
					else if ((bool)datable && (int)who.houseUpgradeLevel < 1)
					{
						if (Game1.random.NextBool())
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3969", displayName));
						}
						else
						{
							CurrentDialogue.Push(TryGetDialogue("RejectMermaidPendant_NeedHouseUpgrade") ?? TryGetDialogue("RejectMermaidPendant") ?? new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.3972"));
							Game1.drawDialogue(this);
						}
					}
					else
					{
						engagementResponse(who);
					}
				}
				return true;
			}
			return false;
		default:
		{
			if (canReceiveGifts && activeObj.HasContextTag(ItemContextTagManager.SanitizeContextTag("propose_roommate_" + base.Name)))
			{
				if (!probe)
				{
					if (who.getFriendshipHeartLevelForNPC(base.Name) >= 10 && (int)who.houseUpgradeLevel >= 1 && !who.isMarriedOrRoommates() && !who.isEngaged())
					{
						engagementResponse(who, asRoommate: true);
					}
					else if (base.Name != "Krobus")
					{
						Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_NoTheater", displayName)));
					}
				}
				return true;
			}
			bool obsoleteNotGiftable = ItemContextTagManager.HasBaseTag(activeObj.QualifiedItemId, "not_giftable");
			if (canReceiveGifts && activeObj.canBeGivenAsGift() && !obsoleteNotGiftable)
			{
				foreach (string activeKey in who.activeDialogueEvents.Keys)
				{
					if (activeKey.Contains("dumped") && Dialogue.ContainsKey(activeKey))
					{
						if (!probe)
						{
							doEmote(12);
						}
						return true;
					}
				}
				if (!probe)
				{
					who.completeQuest("25");
				}
				if (Game1.IsGreenRainingHere() && Game1.year == 1 && !isMarried())
				{
					if (!probe)
					{
						Game1.showRedMessage(".........");
					}
					return false;
				}
				if ((friendship != null && friendship.GiftsThisWeek < 2) || who.spouse == base.Name || this is Child || isBirthday() || who.ActiveObject.QualifiedItemId == "(O)StardropTea")
				{
					if (!probe)
					{
						if (friendship == null)
						{
							friendship = (who.friendshipData[base.Name] = new Friendship());
						}
						if (friendship.IsDivorced())
						{
							CurrentDialogue.Push(TryGetDialogue("RejectGift_Divorced") ?? new Dialogue(this, "Strings\\Characters:Divorced_gift"));
							Game1.drawDialogue(this);
							return true;
						}
						if (friendship.GiftsToday == 1 && who.ActiveObject.QualifiedItemId != "(O)StardropTea")
						{
							Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3981", displayName)));
							return true;
						}
						receiveGift(who.ActiveObject, who, who.ActiveObject.QualifiedItemId != "(O)StardropTea");
						who.reduceActiveItemByOne();
						who.completelyStopAnimatingOrDoingAction();
						faceTowardFarmerForPeriod(4000, 3, faceAway: false, who);
						if ((bool)datable && who.spouse != null && who.spouse != base.Name && !who.hasCurrentOrPendingRoommate() && Utility.isMale(who.spouse) == Utility.isMale(base.Name) && Game1.random.NextDouble() < 0.3 - (double)((float)who.LuckLevel / 100f) - who.DailyLuck && !isBirthday() && friendship.IsDating())
						{
							NPC spouse = Game1.getCharacterFromName(who.spouse);
							CharacterData spouseData = spouse?.GetData();
							if (spouse != null && GameStateQuery.CheckConditions(spouseData?.SpouseGiftJealousy, null, who, activeObj))
							{
								who.changeFriendship(spouseData?.SpouseGiftJealousyFriendshipChange ?? (-30), spouse);
								spouse.CurrentDialogue.Clear();
								spouse.CurrentDialogue.Push(spouse.TryGetDialogue("SpouseGiftJealous", displayName, activeObj.DisplayName) ?? StardewValley.Dialogue.FromTranslation(spouse, "Strings\\StringsFromCSFiles:NPC.cs.3985", displayName));
							}
						}
					}
					return true;
				}
				if (!probe)
				{
					Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3987", displayName, 2)));
				}
				return true;
			}
			return false;
		}
		}
	}

	public string GetDispositionModifiedString(string path, params object[] substitutions)
	{
		List<string> disposition_tags = new List<string>();
		disposition_tags.Add(name.Value);
		if (Game1.player.isMarriedOrRoommates() && Game1.player.getSpouse() == this)
		{
			disposition_tags.Add("spouse");
		}
		CharacterData npcData = GetData();
		if (npcData != null)
		{
			disposition_tags.Add(npcData.Manner.ToString().ToLower());
			disposition_tags.Add(npcData.SocialAnxiety.ToString().ToLower());
			disposition_tags.Add(npcData.Optimism.ToString().ToLower());
			disposition_tags.Add(npcData.Age.ToString().ToLower());
		}
		foreach (string tag in disposition_tags)
		{
			string current_path = path + "_" + Utility.capitalizeFirstLetter(tag);
			string found_string = Game1.content.LoadString(current_path, substitutions);
			if (!(found_string == current_path))
			{
				return found_string;
			}
		}
		return Game1.content.LoadString(path, substitutions);
	}

	public void haltMe(Farmer who)
	{
		Halt();
	}

	public virtual bool checkAction(Farmer who, GameLocation l)
	{
		if (IsInvisible)
		{
			return false;
		}
		if (isSleeping.Value)
		{
			if (!isEmoting)
			{
				doEmote(24);
			}
			shake(250);
			return false;
		}
		if (!who.CanMove)
		{
			return false;
		}
		Game1.player.friendshipData.TryGetValue(base.Name, out var friendship);
		if (base.Name.Equals("Henchman") && l.Name.Equals("WitchSwamp"))
		{
			if (Game1.player.mailReceived.Add("Henchman1"))
			{
				CurrentDialogue.Push(new Dialogue(this, "Strings\\Characters:Henchman1"));
				Game1.drawDialogue(this);
				Game1.player.addQuest("27");
				if (!Game1.player.friendshipData.ContainsKey("Henchman"))
				{
					Game1.player.friendshipData.Add("Henchman", friendship = new Friendship());
				}
			}
			else
			{
				if (who.ActiveObject != null && !who.isRidingHorse() && tryToReceiveActiveObject(who))
				{
					return true;
				}
				if (controller == null)
				{
					CurrentDialogue.Push(new Dialogue(this, "Strings\\Characters:Henchman2"));
					Game1.drawDialogue(this);
				}
			}
			return true;
		}
		bool reacting_to_shorts = false;
		if (who.pantsItem.Value != null && who.pantsItem.Value.QualifiedItemId == "(P)15" && (base.Name.Equals("Lewis") || base.Name.Equals("Marnie")))
		{
			reacting_to_shorts = true;
		}
		if (CanReceiveGifts() && friendship == null)
		{
			Game1.player.friendshipData.Add(base.Name, friendship = new Friendship(0));
			if (base.Name.Equals("Krobus"))
			{
				CurrentDialogue.Push(new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.3990"));
				Game1.drawDialogue(this);
				return true;
			}
		}
		if (who.checkForQuestComplete(this, -1, -1, who.ActiveObject, null, -1, 5))
		{
			faceTowardFarmerForPeriod(6000, 3, faceAway: false, who);
			return true;
		}
		if (base.Name.Equals("Krobus") && who.hasQuest("28"))
		{
			CurrentDialogue.Push(new Dialogue(this, (l is Sewer) ? "Strings\\Characters:KrobusDarkTalisman" : "Strings\\Characters:KrobusDarkTalisman_elsewhere"));
			Game1.drawDialogue(this);
			who.removeQuest("28");
			who.mailReceived.Add("krobusUnseal");
			if (l is Sewer)
			{
				DelayedAction.addTemporarySpriteAfterDelay(new TemporaryAnimatedSprite("TileSheets\\Projectiles", new Microsoft.Xna.Framework.Rectangle(0, 0, 16, 16), 3000f, 1, 0, new Vector2(31f, 17f) * 64f, flicker: false, flipped: false)
				{
					scale = 4f,
					delayBeforeAnimationStart = 1,
					startSound = "debuffSpell",
					motion = new Vector2(-9f, 1f),
					rotationChange = (float)Math.PI / 64f,
					light = true,
					lightRadius = 1f,
					lightcolor = new Color(150, 0, 50),
					layerDepth = 1f,
					alphaFade = 0.003f
				}, l, 200, waitUntilMenusGone: true);
				DelayedAction.addTemporarySpriteAfterDelay(new TemporaryAnimatedSprite("TileSheets\\Projectiles", new Microsoft.Xna.Framework.Rectangle(0, 0, 16, 16), 3000f, 1, 0, new Vector2(31f, 17f) * 64f, flicker: false, flipped: false)
				{
					startSound = "debuffSpell",
					delayBeforeAnimationStart = 1,
					scale = 4f,
					motion = new Vector2(-9f, 1f),
					rotationChange = (float)Math.PI / 64f,
					light = true,
					lightRadius = 1f,
					lightcolor = new Color(150, 0, 50),
					layerDepth = 1f,
					alphaFade = 0.003f
				}, l, 700, waitUntilMenusGone: true);
			}
			return true;
		}
		if (name == "Jas" && base.currentLocation is Desert && who.mailReceived.Contains("Jas_IceCream_DF_" + Game1.year))
		{
			doEmote(32);
			return true;
		}
		if (base.Name == who.spouse && who.IsLocalPlayer && Sprite.CurrentAnimation == null)
		{
			faceDirection(-3);
			if (friendship != null && friendship.Points >= 3125 && who.mailReceived.Add("CF_Spouse"))
			{
				CurrentDialogue.Push(TryGetDialogue("SpouseStardrop") ?? new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.4001"));
				Object stardrop = ItemRegistry.Create<Object>("(O)434");
				stardrop.CanBeSetDown = false;
				stardrop.CanBeGrabbed = false;
				Game1.player.addItemByMenuIfNecessary(stardrop);
				shouldSayMarriageDialogue.Value = false;
				currentMarriageDialogue.Clear();
				return true;
			}
			if (!hasTemporaryMessageAvailable() && currentMarriageDialogue.Count == 0 && CurrentDialogue.Count == 0 && Game1.timeOfDay < 2200 && !isMoving() && who.ActiveObject == null)
			{
				faceGeneralDirection(who.getStandingPosition(), 0, opposite: false, useTileCalculations: false);
				who.faceGeneralDirection(getStandingPosition(), 0, opposite: false, useTileCalculations: false);
				if (FacingDirection == 3 || FacingDirection == 1)
				{
					CharacterData data = GetData();
					int spouseFrame = data?.KissSpriteIndex ?? 28;
					bool facingRight = data?.KissSpriteFacingRight ?? true;
					bool flip = facingRight != (FacingDirection == 1);
					if (who.getFriendshipHeartLevelForNPC(base.Name) > 9 && sleptInBed.Value)
					{
						int delay = (movementPause = (Game1.IsMultiplayer ? 1000 : 10));
						Sprite.ClearAnimation();
						Sprite.AddFrame(new FarmerSprite.AnimationFrame(spouseFrame, delay, secondaryArm: false, flip, haltMe, behaviorAtEndOfFrame: true));
						if (!hasBeenKissedToday.Value)
						{
							who.changeFriendship(10, this);
							if (who.hasCurrentOrPendingRoommate())
							{
								Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("LooseSprites\\emojis", new Microsoft.Xna.Framework.Rectangle(0, 0, 9, 9), 2000f, 1, 0, base.Tile * 64f + new Vector2(16f, -64f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
								{
									motion = new Vector2(0f, -0.5f),
									alphaFade = 0.01f
								});
							}
							else
							{
								Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(211, 428, 7, 6), 2000f, 1, 0, base.Tile * 64f + new Vector2(16f, -64f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
								{
									motion = new Vector2(0f, -0.5f),
									alphaFade = 0.01f
								});
							}
							l.playSound("dwop", null, null, SoundContext.NPC);
							who.exhausted.Value = false;
						}
						hasBeenKissedToday.Value = true;
						Sprite.UpdateSourceRect();
					}
					else
					{
						faceDirection(Game1.random.Choose(2, 0));
						doEmote(12);
					}
					int playerFaceDirection = 1;
					if ((facingRight && !flip) || (!facingRight && flip))
					{
						playerFaceDirection = 3;
					}
					who.PerformKiss(playerFaceDirection);
					return true;
				}
			}
		}
		if (base.SimpleNonVillagerNPC)
		{
			if (name == "Fizz")
			{
				int waivers = Game1.netWorldState.Value.PerfectionWaivers;
				if (Utility.percentGameComplete() + (float)waivers * 0.01f >= 1f)
				{
					doEmote(56);
					shakeTimer = 250;
				}
				else
				{
					CurrentDialogue.Clear();
					if (!Game1.player.mailReceived.Contains("FizzFirstDialogue"))
					{
						Game1.player.mailReceived.Add("FizzFirstDialogue");
						CurrentDialogue.Push(new Dialogue(this, "Strings\\1_6_Strings:Fizz_Intro_1"));
						Game1.drawDialogue(this);
					}
					else
					{
						CurrentDialogue.Push(new Dialogue(this, "Strings\\1_6_Strings:Fizz_Intro_2"));
						Game1.drawDialogue(this);
						Game1.afterDialogues = delegate
						{
							Game1.currentLocation.createQuestionDialogue("", new Response[2]
							{
								new Response("Yes", Game1.content.LoadString("Strings\\1_6_Strings:Fizz_Yes")).SetHotKey(Keys.Y),
								new Response("No", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_No")).SetHotKey(Keys.Escape)
							}, "Fizz");
						};
					}
				}
			}
			else
			{
				string path = "Strings\\SimpleNonVillagerDialogues:" + base.Name;
				string s = Game1.content.LoadString(path);
				if (s != path)
				{
					string[] split = s.Split("||");
					if (nonVillagerNPCTimesTalked != -1 && nonVillagerNPCTimesTalked < split.Length)
					{
						Game1.drawObjectDialogue(split[nonVillagerNPCTimesTalked]);
						nonVillagerNPCTimesTalked++;
						if (nonVillagerNPCTimesTalked >= split.Length)
						{
							nonVillagerNPCTimesTalked = -1;
						}
					}
				}
			}
			return true;
		}
		bool newCurrentDialogue = false;
		if (friendship != null)
		{
			if (getSpouse() == Game1.player && shouldSayMarriageDialogue.Value && currentMarriageDialogue.Count > 0 && currentMarriageDialogue.Count > 0)
			{
				while (currentMarriageDialogue.Count > 0)
				{
					MarriageDialogueReference dialogue_reference = currentMarriageDialogue[currentMarriageDialogue.Count - 1];
					if (dialogue_reference == marriageDefaultDialogue.Value)
					{
						marriageDefaultDialogue.Value = null;
					}
					currentMarriageDialogue.RemoveAt(currentMarriageDialogue.Count - 1);
					CurrentDialogue.Push(dialogue_reference.GetDialogue(this));
				}
				newCurrentDialogue = true;
			}
			if (!newCurrentDialogue)
			{
				newCurrentDialogue = checkForNewCurrentDialogue(friendship.Points / 250);
				if (!newCurrentDialogue)
				{
					newCurrentDialogue = checkForNewCurrentDialogue(friendship.Points / 250, noPreface: true);
				}
			}
		}
		if (who.IsLocalPlayer && friendship != null && (endOfRouteMessage.Value != null || newCurrentDialogue || (base.currentLocation != null && base.currentLocation.HasLocationOverrideDialogue(this))))
		{
			if (!newCurrentDialogue && setTemporaryMessages(who))
			{
				Game1.player.checkForQuestComplete(this, -1, -1, null, null, 5);
				return false;
			}
			Texture2D texture = Sprite.Texture;
			if (texture != null && texture.Bounds.Height > 32 && (CurrentDialogue.Count <= 0 || !CurrentDialogue.Peek().dontFaceFarmer))
			{
				faceTowardFarmerForPeriod(5000, 4, faceAway: false, who);
			}
			if (who.ActiveObject != null && !who.isRidingHorse() && tryToReceiveActiveObject(who))
			{
				Game1.player.checkForQuestComplete(this, -1, -1, null, null, 5);
				faceTowardFarmerForPeriod(3000, 4, faceAway: false, who);
				return true;
			}
			grantConversationFriendship(who);
			Game1.drawDialogue(this);
			return true;
		}
		if (canTalk() && who.hasClubCard && base.Name.Equals("Bouncer") && who.IsLocalPlayer)
		{
			Response[] responses = new Response[2]
			{
				new Response("Yes.", Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4018")),
				new Response("That's", Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4020"))
			};
			l.createQuestionDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4021"), responses, "ClubCard");
		}
		else if (canTalk() && CurrentDialogue.Count > 0)
		{
			if (who.ActiveObject != null && !who.isRidingHorse() && tryToReceiveActiveObject(who, probe: true))
			{
				if (who.IsLocalPlayer)
				{
					tryToReceiveActiveObject(who);
				}
				else
				{
					faceTowardFarmerForPeriod(3000, 4, faceAway: false, who);
				}
				return true;
			}
			if (CurrentDialogue.Count >= 1 || endOfRouteMessage.Value != null || (base.currentLocation != null && base.currentLocation.HasLocationOverrideDialogue(this)))
			{
				if (setTemporaryMessages(who))
				{
					Game1.player.checkForQuestComplete(this, -1, -1, null, null, 5);
					return false;
				}
				Texture2D texture2 = Sprite.Texture;
				if (texture2 != null && texture2.Bounds.Height > 32 && !CurrentDialogue.Peek().dontFaceFarmer)
				{
					faceTowardFarmerForPeriod(5000, 4, faceAway: false, who);
				}
				if (who.IsLocalPlayer)
				{
					grantConversationFriendship(who);
					if (!reacting_to_shorts)
					{
						Game1.drawDialogue(this);
						return true;
					}
				}
			}
			else if (!doingEndOfRouteAnimation)
			{
				try
				{
					if (friendship != null)
					{
						faceTowardFarmerForPeriod(friendship.Points / 125 * 1000 + 1000, 4, faceAway: false, who);
					}
				}
				catch (Exception)
				{
				}
				if (Game1.random.NextDouble() < 0.1)
				{
					doEmote(8);
				}
			}
		}
		else if (canTalk() && !Game1.game1.wasAskedLeoMemory && Game1.CurrentEvent == null && name == "Leo" && base.currentLocation != null && (base.currentLocation.NameOrUniqueName == "LeoTreeHouse" || base.currentLocation.NameOrUniqueName == "Mountain") && Game1.MasterPlayer.hasOrWillReceiveMail("leoMoved") && GetUnseenLeoEvent().HasValue && CanRevisitLeoMemory(GetUnseenLeoEvent()))
		{
			Game1.DrawDialogue(this, "Strings\\Characters:Leo_Memory");
			Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, new Game1.afterFadeFunction(AskLeoMemoryPrompt));
		}
		else
		{
			if (who.ActiveObject != null && !who.isRidingHorse() && tryToReceiveActiveObject(who))
			{
				faceTowardFarmerForPeriod(3000, 4, faceAway: false, who);
				return true;
			}
			if (base.Name.Equals("Krobus"))
			{
				if (l is Sewer)
				{
					Utility.TryOpenShopMenu("ShadowShop", "Krobus");
					return true;
				}
			}
			else if (base.Name.Equals("Dwarf") && who.canUnderstandDwarves && l is Mine)
			{
				Utility.TryOpenShopMenu("Dwarf", base.Name);
				return true;
			}
		}
		if (reacting_to_shorts)
		{
			if (yJumpVelocity != 0f || Sprite.CurrentAnimation != null)
			{
				return true;
			}
			string text = base.Name;
			if (!(text == "Lewis"))
			{
				if (text == "Marnie")
				{
					faceTowardFarmerForPeriod(1000, 3, faceAway: false, who);
					Sprite.ClearAnimation();
					Sprite.AddFrame(new FarmerSprite.AnimationFrame(33, 150, secondaryArm: false, flip: false, delegate
					{
						l.playSound("dustMeep");
					}));
					Sprite.AddFrame(new FarmerSprite.AnimationFrame(34, 180));
					Sprite.AddFrame(new FarmerSprite.AnimationFrame(33, 180, secondaryArm: false, flip: false, delegate
					{
						l.playSound("dustMeep");
					}));
					Sprite.AddFrame(new FarmerSprite.AnimationFrame(34, 180));
					Sprite.AddFrame(new FarmerSprite.AnimationFrame(33, 180, secondaryArm: false, flip: false, delegate
					{
						l.playSound("dustMeep");
					}));
					Sprite.AddFrame(new FarmerSprite.AnimationFrame(34, 180));
					Sprite.AddFrame(new FarmerSprite.AnimationFrame(33, 180, secondaryArm: false, flip: false, delegate
					{
						l.playSound("dustMeep");
					}));
					Sprite.AddFrame(new FarmerSprite.AnimationFrame(34, 180));
					Sprite.loop = false;
				}
			}
			else
			{
				faceTowardFarmerForPeriod(1000, 3, faceAway: false, who);
				jump();
				Sprite.ClearAnimation();
				Sprite.AddFrame(new FarmerSprite.AnimationFrame(26, 1000, secondaryArm: false, flip: false, delegate
				{
					doEmote(12);
				}, behaviorAtEndOfFrame: true));
				Sprite.loop = false;
				shakeTimer = 1000;
				l.playSound("batScreech");
			}
			return true;
		}
		if (setTemporaryMessages(who))
		{
			return false;
		}
		if (((bool)doingEndOfRouteAnimation || !goingToDoEndOfRouteAnimation) && endOfRouteMessage.Value != null)
		{
			Game1.drawDialogue(this);
			return true;
		}
		return false;
	}

	public void grantConversationFriendship(Farmer who, int amount = 20)
	{
		if (who.hasPlayerTalkedToNPC(base.Name) || !who.friendshipData.TryGetValue(base.Name, out var friendship))
		{
			return;
		}
		friendship.TalkedToToday = true;
		Game1.player.checkForQuestComplete(this, -1, -1, null, null, 5);
		if (!isDivorcedFrom(who))
		{
			if (who.hasBuff("statue_of_blessings_4"))
			{
				amount = 60;
			}
			who.changeFriendship(amount, this);
		}
	}

	public virtual void AskLeoMemoryPrompt()
	{
		GameLocation l = base.currentLocation;
		Response[] responses = new Response[2]
		{
			new Response("Yes", Game1.content.LoadString("Strings\\Characters:Leo_Memory_Answer_Yes")),
			new Response("No", Game1.content.LoadString("Strings\\Characters:Leo_Memory_Answer_No"))
		};
		string question = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Characters:Leo_Memory_" + GetUnseenLeoEvent().Value.Value);
		if (question == null)
		{
			question = "";
		}
		l.createQuestionDialogue(question, responses, OnLeoMemoryResponse, this);
	}

	public bool CanRevisitLeoMemory(KeyValuePair<string, string>? event_data)
	{
		if (!event_data.HasValue)
		{
			return false;
		}
		string location_name = event_data.Value.Key;
		string event_id = event_data.Value.Value;
		Dictionary<string, string> location_events;
		try
		{
			location_events = Game1.content.Load<Dictionary<string, string>>("Data\\Events\\" + location_name);
		}
		catch
		{
			return false;
		}
		if (location_events == null)
		{
			return false;
		}
		foreach (string key in location_events.Keys)
		{
			if (Event.SplitPreconditions(key)[0] == event_id)
			{
				GameLocation locationFromName = Game1.getLocationFromName(location_name);
				string event_key = key;
				event_key = event_key.Replace("/e 1039573", "");
				event_key = event_key.Replace("/Hl leoMoved", "");
				string condition = locationFromName?.checkEventPrecondition(event_key);
				if (locationFromName != null && string.IsNullOrEmpty(condition) && condition != "-1")
				{
					return true;
				}
			}
		}
		return false;
	}

	public KeyValuePair<string, string>? GetUnseenLeoEvent()
	{
		if (!Game1.player.eventsSeen.Contains("6497423"))
		{
			return new KeyValuePair<string, string>("IslandWest", "6497423");
		}
		if (!Game1.player.eventsSeen.Contains("6497421"))
		{
			return new KeyValuePair<string, string>("IslandNorth", "6497421");
		}
		if (!Game1.player.eventsSeen.Contains("6497428"))
		{
			return new KeyValuePair<string, string>("IslandSouth", "6497428");
		}
		return null;
	}

	public void OnLeoMemoryResponse(Farmer who, string whichAnswer)
	{
		if (whichAnswer.ToLower() == "yes")
		{
			KeyValuePair<string, string>? event_data = GetUnseenLeoEvent();
			if (!event_data.HasValue)
			{
				return;
			}
			string location_name = event_data.Value.Key;
			string event_id = event_data.Value.Value;
			string eventAssetName = "Data\\Events\\" + location_name;
			Dictionary<string, string> location_events;
			try
			{
				location_events = Game1.content.Load<Dictionary<string, string>>(eventAssetName);
			}
			catch
			{
				return;
			}
			if (location_events == null)
			{
				return;
			}
			Point oldTile = Game1.player.TilePoint;
			string oldLocation = Game1.player.currentLocation.NameOrUniqueName;
			int oldDirection = Game1.player.FacingDirection;
			{
				foreach (string key in location_events.Keys)
				{
					if (Event.SplitPreconditions(key)[0] == event_id)
					{
						LocationRequest location_request = Game1.getLocationRequest(location_name);
						Game1.warpingForForcedRemoteEvent = true;
						location_request.OnWarp += delegate
						{
							Event @event = new Event(location_events[key], eventAssetName, "event_id");
							@event.isMemory = true;
							@event.setExitLocation(oldLocation, oldTile.X, oldTile.Y);
							Game1.player.orientationBeforeEvent = oldDirection;
							location_request.Location.currentEvent = @event;
							location_request.Location.startEvent(@event);
							Game1.warpingForForcedRemoteEvent = false;
						};
						int x = 8;
						int y = 8;
						Utility.getDefaultWarpLocation(location_request.Name, ref x, ref y);
						Game1.warpFarmer(location_request, x, y, Game1.player.FacingDirection);
					}
				}
				return;
			}
		}
		Game1.game1.wasAskedLeoMemory = true;
	}

	public bool isDivorcedFrom(Farmer who)
	{
		return IsDivorcedFrom(who, base.Name);
	}

	public static bool IsDivorcedFrom(Farmer player, string npcName)
	{
		if (player != null && player.friendshipData.TryGetValue(npcName, out var friendship))
		{
			return friendship.IsDivorced();
		}
		return false;
	}

	public override void MovePosition(GameTime time, xTile.Dimensions.Rectangle viewport, GameLocation currentLocation)
	{
		if (movementPause <= 0)
		{
			faceTowardFarmerTimer = 0;
			base.MovePosition(time, viewport, currentLocation);
		}
	}

	public GameLocation getHome()
	{
		if (isMarried() && getSpouse() != null)
		{
			return Utility.getHomeOfFarmer(getSpouse());
		}
		return Game1.RequireLocation(defaultMap);
	}

	public override bool canPassThroughActionTiles()
	{
		return true;
	}

	public virtual void behaviorOnFarmerPushing()
	{
	}

	public virtual void behaviorOnFarmerLocationEntry(GameLocation location, Farmer who)
	{
		if (Sprite != null && Sprite.CurrentAnimation == null && Sprite.SourceRect.Height > 32 && !base.SimpleNonVillagerNPC)
		{
			Sprite.SpriteWidth = 16;
			Sprite.SpriteHeight = 16;
			Sprite.currentFrame = 0;
		}
	}

	public virtual void behaviorOnLocalFarmerLocationEntry(GameLocation location)
	{
		shouldPlayRobinHammerAnimation.CancelInterpolation();
		shouldPlaySpousePatioAnimation.CancelInterpolation();
		shouldWearIslandAttire.CancelInterpolation();
		isSleeping.CancelInterpolation();
		doingEndOfRouteAnimation.CancelInterpolation();
		if (doingEndOfRouteAnimation.Value)
		{
			_skipRouteEndIntro = true;
		}
		else
		{
			_skipRouteEndIntro = false;
		}
		endOfRouteBehaviorName.CancelInterpolation();
		if (isSleeping.Value)
		{
			position.Field.CancelInterpolation();
		}
	}

	public override void updateMovement(GameLocation location, GameTime time)
	{
		lastPosition = base.Position;
		if (DirectionsToNewLocation != null && !Game1.newDay)
		{
			Point standingPixel = base.StandingPixel;
			if (standingPixel.X < -64 || standingPixel.X > location.map.DisplayWidth + 64 || standingPixel.Y < -64 || standingPixel.Y > location.map.DisplayHeight + 64)
			{
				IsWalkingInSquare = false;
				Game1.warpCharacter(this, DefaultMap, DefaultPosition);
				location.characters.Remove(this);
			}
			else if (IsWalkingInSquare)
			{
				returnToEndPoint();
				MovePosition(time, Game1.viewport, location);
			}
		}
		else if (IsWalkingInSquare)
		{
			randomSquareMovement(time);
			MovePosition(time, Game1.viewport, location);
		}
	}

	public void facePlayer(Farmer who)
	{
		if ((int)facingDirectionBeforeSpeakingToPlayer == -1)
		{
			facingDirectionBeforeSpeakingToPlayer.Value = getFacingDirection();
		}
		faceDirection((who.FacingDirection + 2) % 4);
	}

	public void doneFacingPlayer(Farmer who)
	{
	}

	public override void update(GameTime time, GameLocation location)
	{
		if (AllowDynamicAppearance && base.currentLocation != null && base.currentLocation.NameOrUniqueName != LastLocationNameForAppearance)
		{
			ChooseAppearance();
		}
		if (Game1.IsMasterGame && currentScheduleDelay > 0f)
		{
			currentScheduleDelay -= (float)time.ElapsedGameTime.TotalSeconds;
			if (currentScheduleDelay <= 0f)
			{
				currentScheduleDelay = -1f;
				checkSchedule(Game1.timeOfDay);
				currentScheduleDelay = 0f;
			}
		}
		removeHenchmanEvent.Poll();
		if (Game1.IsMasterGame && shouldWearIslandAttire.Value && (base.currentLocation == null || base.currentLocation.InValleyContext()))
		{
			shouldWearIslandAttire.Value = false;
		}
		if (_startedEndOfRouteBehavior == null && _finishingEndOfRouteBehavior == null && loadedEndOfRouteBehavior != endOfRouteBehaviorName.Value)
		{
			loadEndOfRouteBehavior(endOfRouteBehaviorName);
		}
		if (doingEndOfRouteAnimation.Value != currentlyDoingEndOfRouteAnimation)
		{
			if (!currentlyDoingEndOfRouteAnimation)
			{
				if (string.Equals(loadedEndOfRouteBehavior, endOfRouteBehaviorName.Value, StringComparison.Ordinal))
				{
					reallyDoAnimationAtEndOfScheduleRoute();
				}
			}
			else
			{
				finishEndOfRouteAnimation();
			}
			currentlyDoingEndOfRouteAnimation = doingEndOfRouteAnimation.Value;
		}
		if (shouldWearIslandAttire.Value != isWearingIslandAttire)
		{
			if (!isWearingIslandAttire)
			{
				wearIslandAttire();
			}
			else
			{
				wearNormalClothes();
			}
		}
		if (isSleeping.Value != isPlayingSleepingAnimation)
		{
			if (!isPlayingSleepingAnimation)
			{
				playSleepingAnimation();
			}
			else
			{
				Sprite.StopAnimation();
				isPlayingSleepingAnimation = false;
			}
		}
		if (shouldPlayRobinHammerAnimation.Value != isPlayingRobinHammerAnimation)
		{
			if (!isPlayingRobinHammerAnimation)
			{
				doPlayRobinHammerAnimation();
				isPlayingRobinHammerAnimation = true;
			}
			else
			{
				Sprite.StopAnimation();
				isPlayingRobinHammerAnimation = false;
			}
		}
		if (shouldPlaySpousePatioAnimation.Value != isPlayingSpousePatioAnimation)
		{
			if (!isPlayingSpousePatioAnimation)
			{
				doPlaySpousePatioAnimation();
				isPlayingSpousePatioAnimation = true;
			}
			else
			{
				Sprite.StopAnimation();
				isPlayingSpousePatioAnimation = false;
			}
		}
		if (returningToEndPoint)
		{
			returnToEndPoint();
			MovePosition(time, Game1.viewport, location);
		}
		else if (temporaryController != null)
		{
			if (temporaryController.update(time))
			{
				bool nPCSchedule = temporaryController.NPCSchedule;
				temporaryController = null;
				if (nPCSchedule)
				{
					currentScheduleDelay = -1f;
					checkSchedule(Game1.timeOfDay);
					currentScheduleDelay = 0f;
				}
			}
			updateEmote(time);
		}
		else
		{
			base.update(time, location);
		}
		if (textAboveHeadTimer > 0)
		{
			if (textAboveHeadPreTimer > 0)
			{
				textAboveHeadPreTimer -= time.ElapsedGameTime.Milliseconds;
			}
			else
			{
				textAboveHeadTimer -= time.ElapsedGameTime.Milliseconds;
				if (textAboveHeadTimer > 500)
				{
					textAboveHeadAlpha = Math.Min(1f, textAboveHeadAlpha + 0.1f);
				}
				else
				{
					textAboveHeadAlpha = Math.Max(0f, textAboveHeadAlpha - 0.04f);
				}
			}
		}
		if (isWalkingInSquare && !returningToEndPoint)
		{
			randomSquareMovement(time);
		}
		if (Sprite?.CurrentAnimation != null && !Game1.eventUp && Game1.IsMasterGame && Sprite.animateOnce(time))
		{
			Sprite.CurrentAnimation = null;
		}
		if (movementPause > 0 && (!Game1.dialogueUp || controller != null))
		{
			freezeMotion = true;
			movementPause -= time.ElapsedGameTime.Milliseconds;
			if (movementPause <= 0)
			{
				freezeMotion = false;
			}
		}
		if (shakeTimer > 0)
		{
			shakeTimer -= time.ElapsedGameTime.Milliseconds;
		}
		if (lastPosition.Equals(base.Position))
		{
			timerSinceLastMovement += time.ElapsedGameTime.Milliseconds;
		}
		else
		{
			timerSinceLastMovement = 0f;
		}
		if ((bool)swimming)
		{
			yOffset = (float)(Math.Cos(time.TotalGameTime.TotalMilliseconds / 2000.0) * 4.0);
			float oldSwimTimer = swimTimer;
			swimTimer -= time.ElapsedGameTime.Milliseconds;
			if (timerSinceLastMovement == 0f)
			{
				if (oldSwimTimer > 400f && swimTimer <= 400f && location.Equals(Game1.currentLocation))
				{
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), 150f - (Math.Abs(xVelocity) + Math.Abs(yVelocity)) * 3f, 8, 0, new Vector2(base.Position.X, base.StandingPixel.Y - 32), flicker: false, Game1.random.NextBool(), 0.01f, 0.01f, Color.White, 1f, 0.003f, 0f, 0f));
					location.playSound("slosh", null, null, SoundContext.NPC);
				}
				if (swimTimer < 0f)
				{
					swimTimer = 800f;
					if (location.Equals(Game1.currentLocation))
					{
						location.playSound("slosh", null, null, SoundContext.NPC);
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), 150f - (Math.Abs(xVelocity) + Math.Abs(yVelocity)) * 3f, 8, 0, new Vector2(base.Position.X, base.StandingPixel.Y - 32), flicker: false, Game1.random.NextBool(), 0.01f, 0.01f, Color.White, 1f, 0.003f, 0f, 0f));
					}
				}
			}
			else if (swimTimer < 0f)
			{
				swimTimer = 100f;
			}
		}
		if (Game1.IsMasterGame)
		{
			isMovingOnPathFindPath.Value = controller != null && temporaryController != null;
		}
	}

	public virtual void wearIslandAttire()
	{
		isWearingIslandAttire = true;
		ChooseAppearance();
	}

	public virtual void wearNormalClothes()
	{
		isWearingIslandAttire = false;
		ChooseAppearance();
	}

	/// <summary>Runs NPC update logic on ten in-game minute intervals (e.g. greeting players or other NPCs)</summary>
	/// <param name="timeOfDay">The new in-game time.</param>
	/// <param name="location">The location where the update is occurring.</param>
	public virtual void performTenMinuteUpdate(int timeOfDay, GameLocation location)
	{
		if (Game1.eventUp || location == null)
		{
			return;
		}
		if (Game1.random.NextDouble() < 0.1 && Dialogue != null && Dialogue.TryGetValue(location.Name + "_Ambient", out var rawText))
		{
			CharacterData data2 = GetData();
			if (data2 == null || data2.CanGreetNearbyCharacters)
			{
				string[] split = rawText.Split('/');
				int extraTime = Game1.random.Next(4) * 1000;
				showTextAboveHead(Game1.random.Choose(split), null, 2, 3000, extraTime);
				return;
			}
		}
		if (!isMoving() || !location.IsOutdoors || timeOfDay >= 1800 || !(Game1.random.NextDouble() < 0.3 + ((SocialAnxiety == 0) ? 0.25 : ((SocialAnxiety != 1) ? 0.0 : ((Manners == 2) ? (-1.0) : (-0.2))))) || (Age == 1 && (Manners != 1 || SocialAnxiety != 0)) || isMarried())
		{
			return;
		}
		CharacterData data = GetData();
		if (data == null || !data.CanGreetNearbyCharacters)
		{
			return;
		}
		Character c = Utility.isThereAFarmerOrCharacterWithinDistance(base.Tile, 4, location);
		if (c == null || c.Name == base.Name || c is Horse)
		{
			return;
		}
		NPC obj = c as NPC;
		if (obj != null && obj.GetData()?.CanGreetNearbyCharacters == false)
		{
			return;
		}
		NPC obj2 = c as NPC;
		if (obj2 == null || !obj2.SimpleNonVillagerNPC)
		{
			Dictionary<string, string> friendsAndFamily = data.FriendsAndFamily;
			if ((friendsAndFamily == null || !friendsAndFamily.ContainsKey(c.Name)) && isFacingToward(c.Tile))
			{
				sayHiTo(c);
			}
		}
	}

	public void sayHiTo(Character c)
	{
		if (getHi(c.displayName) != null)
		{
			showTextAboveHead(getHi(c.displayName));
			if (c is NPC npc && Game1.random.NextDouble() < 0.66 && npc.getHi(displayName) != null)
			{
				npc.showTextAboveHead(npc.getHi(displayName), null, 2, 3000, 1000 + Game1.random.Next(500));
			}
		}
	}

	public string getHi(string nameToGreet)
	{
		if (Age == 2)
		{
			if (SocialAnxiety != 1)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4059");
			}
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4058");
		}
		switch (SocialAnxiety)
		{
		case 1:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs." + Game1.random.Choose("4060", "4061"));
		case 0:
			if (!(Game1.random.NextDouble() < 0.33))
			{
				if (!Game1.random.NextBool())
				{
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4068", nameToGreet);
				}
				return ((Game1.timeOfDay < 1200) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4063") : ((Game1.timeOfDay < 1700) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4064") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4065"))) + ", " + Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4066", nameToGreet);
			}
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4062");
		default:
			if (!(Game1.random.NextDouble() < 0.33))
			{
				if (!Game1.random.NextBool())
				{
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4072");
				}
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4071", nameToGreet);
			}
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4060");
		}
	}

	public bool isFacingToward(Vector2 tileLocation)
	{
		return FacingDirection switch
		{
			0 => (float)base.TilePoint.Y > tileLocation.Y, 
			1 => (float)base.TilePoint.X < tileLocation.X, 
			2 => (float)base.TilePoint.Y < tileLocation.Y, 
			3 => (float)base.TilePoint.X > tileLocation.X, 
			_ => false, 
		};
	}

	public virtual void arriveAt(GameLocation l)
	{
		if (!Game1.eventUp && Game1.random.NextBool() && Dialogue != null && Dialogue.TryGetValue(string.Concat(l.name, "_Entry"), out var rawText))
		{
			showTextAboveHead(Game1.random.Choose(rawText.Split('/')));
		}
	}

	public override void Halt()
	{
		base.Halt();
		shouldPlaySpousePatioAnimation.Value = false;
		isPlayingSleepingAnimation = false;
		isCharging = false;
		base.speed = 2;
		addedSpeed = 0f;
		if (isSleeping.Value)
		{
			playSleepingAnimation();
			Sprite.UpdateSourceRect();
		}
	}

	public void addExtraDialogue(Dialogue dialogue)
	{
		if (updatedDialogueYet)
		{
			if (dialogue != null)
			{
				CurrentDialogue.Push(dialogue);
			}
		}
		else
		{
			extraDialogueMessageToAddThisMorning = dialogue;
		}
	}

	public void PerformDivorce()
	{
		reloadDefaultLocation();
		Game1.warpCharacter(this, defaultMap, DefaultPosition / 64f);
	}

	public Dialogue tryToGetMarriageSpecificDialogue(string dialogueKey)
	{
		Dictionary<string, string> marriageDialogues = null;
		string assetName = null;
		bool skip_married_dialogue = false;
		string rawText;
		if (isRoommate())
		{
			try
			{
				assetName = "Characters\\Dialogue\\MarriageDialogue" + GetDialogueSheetName() + "Roommate";
				Dictionary<string, string> rawData = Game1.content.Load<Dictionary<string, string>>(assetName);
				if (rawData != null)
				{
					skip_married_dialogue = true;
					marriageDialogues = rawData;
					if (marriageDialogues != null && marriageDialogues.TryGetValue(dialogueKey, out rawText))
					{
						return new Dialogue(this, assetName + ":" + dialogueKey, rawText);
					}
				}
			}
			catch (Exception)
			{
				assetName = null;
			}
		}
		if (!skip_married_dialogue)
		{
			try
			{
				assetName = "Characters\\Dialogue\\MarriageDialogue" + GetDialogueSheetName();
				marriageDialogues = Game1.content.Load<Dictionary<string, string>>(assetName);
			}
			catch (Exception)
			{
				assetName = null;
			}
		}
		if (marriageDialogues != null && marriageDialogues.TryGetValue(dialogueKey, out rawText))
		{
			return new Dialogue(this, assetName + ":" + dialogueKey, rawText);
		}
		assetName = "Characters\\Dialogue\\MarriageDialogue";
		marriageDialogues = Game1.content.Load<Dictionary<string, string>>(assetName);
		if (isRoommate())
		{
			string key = dialogueKey + "Roommate";
			if (marriageDialogues != null && marriageDialogues.TryGetValue(key, out rawText))
			{
				return new Dialogue(this, assetName + ":" + dialogueKey, rawText);
			}
		}
		if (marriageDialogues != null && marriageDialogues.TryGetValue(dialogueKey, out rawText))
		{
			return new Dialogue(this, assetName + ":" + dialogueKey, rawText);
		}
		return null;
	}

	public void resetCurrentDialogue()
	{
		CurrentDialogue = null;
		shouldSayMarriageDialogue.Value = false;
		currentMarriageDialogue.Clear();
	}

	private Stack<Dialogue> loadCurrentDialogue()
	{
		updatedDialogueYet = true;
		Stack<Dialogue> currentDialogue = new Stack<Dialogue>();
		try
		{
			Friendship friends;
			int heartLevel = (Game1.player.friendshipData.TryGetValue(base.Name, out friends) ? (friends.Points / 250) : 0);
			Random r = Utility.CreateDaySaveRandom(Game1.stats.DaysPlayed * 77, 2f + defaultPosition.X * 77f, defaultPosition.Y * 777f);
			if (Game1.IsGreenRainingHere())
			{
				Dialogue dialogue = null;
				if (Game1.year >= 2)
				{
					dialogue = TryGetDialogue("GreenRain_2");
				}
				if (dialogue == null)
				{
					dialogue = TryGetDialogue("GreenRain");
				}
				if (dialogue != null)
				{
					currentDialogue.Clear();
					currentDialogue.Push(dialogue);
					return currentDialogue;
				}
			}
			if (r.NextDouble() < 0.025 && heartLevel >= 1)
			{
				CharacterData npcData = GetData();
				if (npcData?.FriendsAndFamily != null && Utility.TryGetRandom(npcData.FriendsAndFamily, out var relativeName, out var relativeTitle))
				{
					NPC relative = Game1.getCharacterFromName(relativeName);
					string relativeDisplayName = relative?.displayName ?? GetDisplayName(relativeName);
					CharacterData relativeData;
					bool relativeIsMale = ((relative != null) ? (relative.gender.Value == Gender.Male) : (TryGetData(relativeName, out relativeData) && relativeData.Gender == Gender.Male));
					relativeTitle = TokenParser.ParseText(relativeTitle);
					if (string.IsNullOrWhiteSpace(relativeTitle))
					{
						relativeTitle = null;
					}
					Dictionary<string, string> npcGiftTastes = DataLoader.NpcGiftTastes(Game1.content);
					if (npcGiftTastes.TryGetValue(relativeName, out var rawGiftTasteData))
					{
						string[] rawGiftTasteFields = rawGiftTasteData.Split('/');
						string item = null;
						string itemName = null;
						string nameAndTitle = ((relativeTitle == null || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ja) ? relativeDisplayName : (relativeIsMale ? Game1.LoadStringByGender(Gender, "Strings\\StringsFromCSFiles:NPC.cs.4079", relativeTitle) : Game1.LoadStringByGender(Gender, "Strings\\StringsFromCSFiles:NPC.cs.4080", relativeTitle)));
						string message = Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4083", nameAndTitle);
						if (r.NextBool())
						{
							int tries = 0;
							string[] lovedItems = ArgUtility.SplitBySpace(ArgUtility.Get(rawGiftTasteFields, 1));
							while ((item == null || item.StartsWith("-")) && tries < 30)
							{
								item = r.Choose(lovedItems);
								tries++;
							}
							if (base.Name == "Penny" && relativeName == "Pam")
							{
								while (true)
								{
									switch (item)
									{
									case "303":
									case "346":
									case "348":
									case "459":
										goto IL_0273;
									}
									break;
									IL_0273:
									item = r.Choose(lovedItems);
								}
							}
							if (item != null)
							{
								ParsedItemData itemData = ItemRegistry.GetData(item);
								if (itemData != null)
								{
									itemName = itemData.DisplayName;
									message += Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4084", itemName);
									if (Age == 2)
									{
										message = Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4086", nameAndTitle, itemName) + (relativeIsMale ? Game1.LoadStringByGender(Gender, "Strings\\StringsFromCSFiles:NPC.cs.4088") : Game1.LoadStringByGender(Gender, "Strings\\StringsFromCSFiles:NPC.cs.4089"));
									}
									else
									{
										switch (r.Next(5))
										{
										case 0:
											message = Game1.LoadStringByGender(Gender, "Strings\\StringsFromCSFiles:NPC.cs.4091", nameAndTitle, itemName);
											break;
										case 1:
											message = (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4094", nameAndTitle, itemName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4097", nameAndTitle, itemName));
											break;
										case 2:
											message = (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4100", nameAndTitle, itemName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4103", nameAndTitle, itemName));
											break;
										case 3:
											message = Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4106", nameAndTitle, itemName);
											break;
										}
										if (r.NextDouble() < 0.65)
										{
											switch (r.Next(5))
											{
											case 0:
												message += (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4109") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4111"));
												break;
											case 1:
												message += ((!relativeIsMale) ? (r.NextBool() ? Game1.LoadStringByGender(Gender, "Strings\\StringsFromCSFiles:NPC.cs.4115") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4116")) : (r.NextBool() ? Game1.LoadStringByGender(Gender, "Strings\\StringsFromCSFiles:NPC.cs.4113") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4114")));
												break;
											case 2:
												message += (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4118") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4120"));
												break;
											case 3:
												message += Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4125");
												break;
											case 4:
												message += (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4126") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4128"));
												break;
											}
											if (relativeName.Equals("Abigail") && r.NextBool())
											{
												message = Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4128", relativeDisplayName, itemName);
											}
										}
									}
								}
							}
						}
						else
						{
							string[] hatedItems = ArgUtility.SplitBySpace(ArgUtility.Get(rawGiftTasteFields, 7));
							if (hatedItems.Count() > 0)
							{
								int tries = 0;
								while ((item == null || item.StartsWith("-")) && tries < 30)
								{
									item = r.Choose(hatedItems);
									tries++;
								}
							}
							if (item == null)
							{
								int tries = 0;
								while ((item == null || item.StartsWith("-")) && tries < 30)
								{
									item = r.Choose(ArgUtility.SplitBySpace(npcGiftTastes["Universal_Hate"]));
									tries++;
								}
							}
							if (item != null)
							{
								ParsedItemData itemData = ItemRegistry.GetData(item);
								if (itemData != null)
								{
									itemName = itemData.DisplayName;
									message += (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4135", itemName, Lexicon.getRandomNegativeFoodAdjective()) : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4138", itemName, Lexicon.getRandomNegativeFoodAdjective()));
									if (Age == 2)
									{
										message = (relativeIsMale ? Game1.LoadStringByGender(Gender, "Strings\\StringsFromCSFiles:NPC.cs.4141", relativeDisplayName, itemName) : Game1.LoadStringByGender(Gender, "Strings\\StringsFromCSFiles:NPC.cs.4144", relativeDisplayName, itemName));
									}
									else
									{
										switch (r.Next(4))
										{
										case 0:
											message = (r.NextBool() ? Game1.LoadStringByGender(Gender, "Strings\\StringsFromCSFiles:NPC.cs.4146") : "") + Game1.LoadStringByGender(Gender, "Strings\\StringsFromCSFiles:NPC.cs.4147", nameAndTitle, itemName);
											break;
										case 1:
											message = ((!relativeIsMale) ? (r.NextBool() ? Game1.LoadStringByGender(Gender, "Strings\\StringsFromCSFiles:NPC.cs.4153", nameAndTitle, itemName) : Game1.LoadStringByGender(Gender, "Strings\\StringsFromCSFiles:NPC.cs.4154", nameAndTitle, itemName)) : (r.NextBool() ? Game1.LoadStringByGender(Gender, "Strings\\StringsFromCSFiles:NPC.cs.4149", nameAndTitle, itemName) : Game1.LoadStringByGender(Gender, "Strings\\StringsFromCSFiles:NPC.cs.4152", nameAndTitle, itemName)));
											break;
										case 2:
											message = (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4161", nameAndTitle, itemName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4164", nameAndTitle, itemName));
											break;
										}
										if (r.NextDouble() < 0.65)
										{
											switch (r.Next(5))
											{
											case 0:
												message += Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4170");
												break;
											case 1:
												message += Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4171");
												break;
											case 2:
												message += (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4172") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4174"));
												break;
											case 3:
												message += (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4176") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4178"));
												break;
											case 4:
												message += Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4180");
												break;
											}
											if (base.Name.Equals("Lewis") && r.NextBool())
											{
												message = Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4182", relativeDisplayName, itemName);
											}
										}
									}
								}
							}
						}
						if (itemName != null)
						{
							if (Game1.getCharacterFromName(relativeName) != null)
							{
								message = message + "%revealtaste:" + relativeName + ":" + item;
							}
							currentDialogue.Clear();
							if (message.Length > 0)
							{
								try
								{
									message = message.Substring(0, 1).ToUpper() + message.Substring(1, message.Length - 1);
								}
								catch (Exception)
								{
								}
							}
							currentDialogue.Push(new Dialogue(this, null, message));
							return currentDialogue;
						}
					}
				}
			}
			if (Dialogue != null && Dialogue.Count != 0)
			{
				currentDialogue.Clear();
				if (Game1.player.spouse != null && Game1.player.spouse == base.Name)
				{
					if (Game1.player.isEngaged())
					{
						Dictionary<string, string> engagementDialogue = Game1.content.Load<Dictionary<string, string>>("Data\\EngagementDialogue");
						if (Game1.player.hasCurrentOrPendingRoommate() && engagementDialogue.ContainsKey(base.Name + "Roommate0"))
						{
							currentDialogue.Push(new Dialogue(this, "Data\\EngagementDialogue:" + base.Name + "Roommate" + r.Next(2)));
						}
						else if (engagementDialogue.ContainsKey(base.Name + "0"))
						{
							currentDialogue.Push(new Dialogue(this, "Data\\EngagementDialogue:" + base.Name + r.Next(2)));
						}
					}
					else if (!Game1.newDay && marriageDefaultDialogue.Value != null && !shouldSayMarriageDialogue.Value)
					{
						currentDialogue.Push(marriageDefaultDialogue.Value.GetDialogue(this));
						marriageDefaultDialogue.Value = null;
					}
				}
				else
				{
					if (Game1.player.friendshipData.TryGetValue(base.Name, out var friendship) && friendship.IsDivorced())
					{
						Dialogue dialogue = StardewValley.Dialogue.TryGetDialogue(this, "Characters\\Dialogue\\" + GetDialogueSheetName() + ":divorced");
						if (dialogue != null)
						{
							currentDialogue.Push(dialogue);
							return currentDialogue;
						}
					}
					if (Game1.isRaining && r.NextBool() && (base.currentLocation == null || base.currentLocation.InValleyContext()) && (!base.Name.Equals("Krobus") || !(Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth) == "Fri")) && (!base.Name.Equals("Penny") || !Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade")) && (!base.Name.Equals("Emily") || !Game1.IsFall || Game1.dayOfMonth != 15))
					{
						Dialogue dialogue = StardewValley.Dialogue.TryGetDialogue(this, "Characters\\Dialogue\\rainy:" + GetDialogueSheetName());
						if (dialogue != null)
						{
							currentDialogue.Push(dialogue);
							return currentDialogue;
						}
					}
					Dialogue d = tryToRetrieveDialogue(Game1.currentSeason + "_", heartLevel);
					if (d == null)
					{
						d = tryToRetrieveDialogue("", heartLevel);
					}
					if (d != null)
					{
						currentDialogue.Push(d);
					}
				}
			}
			else if (base.Name.Equals("Bouncer"))
			{
				currentDialogue.Push(new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.4192"));
			}
			if (extraDialogueMessageToAddThisMorning != null)
			{
				currentDialogue.Push(extraDialogueMessageToAddThisMorning);
			}
		}
		catch (Exception ex)
		{
			Game1.log.Error("NPC '" + base.Name + "' failed loading their current dialogue.", ex);
		}
		return currentDialogue;
	}

	public bool checkForNewCurrentDialogue(int heartLevel, bool noPreface = false)
	{
		if (Game1.IsGreenRainingHere())
		{
			return false;
		}
		Dialogue dialogue;
		foreach (string eventMessageKey in Game1.player.activeDialogueEvents.Keys)
		{
			if (eventMessageKey == "")
			{
				continue;
			}
			dialogue = TryGetDialogue(eventMessageKey);
			if (dialogue == null)
			{
				continue;
			}
			string mailKey = base.Name + "_" + eventMessageKey;
			if (dialogue != null && !Game1.player.mailReceived.Contains(mailKey))
			{
				CurrentDialogue.Clear();
				CurrentDialogue.Push(dialogue);
				if (!eventMessageKey.Contains("dumped"))
				{
					Game1.player.mailReceived.Add(mailKey);
				}
				return true;
			}
		}
		string preface = ((Game1.season != 0 && !noPreface) ? Game1.currentSeason : "");
		dialogue = TryGetDialogue(string.Concat(preface, Game1.currentLocation.name, "_", base.TilePoint.X.ToString(), "_", base.TilePoint.Y.ToString())) ?? TryGetDialogue(string.Concat(preface, Game1.currentLocation.name, "_", Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth)));
		int hearts = 10;
		while (dialogue == null && hearts >= 2 && heartLevel >= hearts)
		{
			dialogue = TryGetDialogue(string.Concat(preface, Game1.currentLocation.name, hearts.ToString()));
			hearts -= 2;
		}
		dialogue = dialogue ?? TryGetDialogue(preface + Game1.currentLocation.Name);
		if (dialogue != null)
		{
			dialogue.removeOnNextMove = true;
			CurrentDialogue.Push(dialogue);
			return true;
		}
		return false;
	}

	/// <summary>Try to get a specific dialogue from the loaded <see cref="P:StardewValley.NPC.Dialogue" />.</summary>
	/// <param name="key">The dialogue key.</param>
	/// <returns>Returns the matched dialogue if found, else <c>null</c>.</returns>
	public Dialogue TryGetDialogue(string key)
	{
		Dictionary<string, string> dialogue = Dialogue;
		if (dialogue != null && dialogue.TryGetValue(key, out var text))
		{
			return new Dialogue(this, LoadedDialogueKey + ":" + key, text);
		}
		return null;
	}

	/// <summary>Try to get a specific dialogue from the loaded <see cref="P:StardewValley.NPC.Dialogue" />.</summary>
	/// <param name="key">The dialogue key.</param>
	/// <param name="substitutions">The values with which to replace placeholders like <c>{0}</c> in the loaded text.</param>
	/// <returns>Returns the matched dialogue if found, else <c>null</c>.</returns>
	public Dialogue TryGetDialogue(string key, params object[] substitutions)
	{
		Dictionary<string, string> dialogue = Dialogue;
		if (dialogue != null && dialogue.TryGetValue(key, out var text))
		{
			return new Dialogue(this, LoadedDialogueKey + ":" + key, string.Format(text, substitutions));
		}
		return null;
	}

	/// <summary>Try to get a dialogue from the loaded <see cref="P:StardewValley.NPC.Dialogue" />, applying variant rules for roommates, marriage, inlaws, dates, etc.</summary>
	/// <param name="preface">A prefix added to the translation keys to look up.</param>
	/// <param name="heartLevel">The NPC's heart level with the player.</param>
	/// <param name="appendToEnd">A suffix added to the translation keys to look up.</param>
	/// <returns>Returns the best matched dialogue if found, else <c>null</c>.</returns>
	public Dialogue tryToRetrieveDialogue(string preface, int heartLevel, string appendToEnd = "")
	{
		int year = Game1.year;
		if (Game1.year > 2)
		{
			year = 2;
		}
		if (!string.IsNullOrEmpty(Game1.player.spouse) && appendToEnd.Equals(""))
		{
			if (Game1.player.hasCurrentOrPendingRoommate())
			{
				Dialogue s = tryToRetrieveDialogue(preface, heartLevel, "_roommate_" + Game1.player.spouse);
				if (s != null)
				{
					return s;
				}
			}
			else
			{
				Dialogue s = tryToRetrieveDialogue(preface, heartLevel, "_inlaw_" + Game1.player.spouse);
				if (s != null)
				{
					return s;
				}
			}
		}
		string day_name = Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth);
		if (base.Name == "Pierre" && (Game1.isLocationAccessible("CommunityCenter") || Game1.player.HasTownKey))
		{
			_ = day_name == "Wed";
		}
		Dialogue dialogue;
		if (year == 1)
		{
			dialogue = TryGetDialogue(preface + Game1.dayOfMonth + appendToEnd);
			if (dialogue != null)
			{
				return dialogue;
			}
		}
		dialogue = TryGetDialogue(preface + Game1.dayOfMonth + "_" + year + appendToEnd);
		if (dialogue != null)
		{
			return dialogue;
		}
		dialogue = TryGetDialogue(preface + Game1.dayOfMonth + "_*" + appendToEnd);
		if (dialogue != null)
		{
			return dialogue;
		}
		for (int hearts = 10; hearts >= 2; hearts -= 2)
		{
			if (heartLevel >= hearts)
			{
				dialogue = TryGetDialogue(preface + day_name + hearts + "_" + year + appendToEnd) ?? TryGetDialogue(preface + day_name + hearts + appendToEnd);
				if (dialogue != null)
				{
					if (hearts == 4 && preface == "fall_" && day_name == "Mon" && base.Name.Equals("Penny") && Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
					{
						return TryGetDialogue(preface + day_name + "_" + year + appendToEnd) ?? TryGetDialogue("fall_Mon");
					}
					return dialogue;
				}
			}
		}
		dialogue = TryGetDialogue(preface + day_name + appendToEnd);
		if (dialogue != null)
		{
			Dialogue specificDialogue = TryGetDialogue(preface + day_name + "_" + year + appendToEnd);
			if (specificDialogue != null)
			{
				dialogue = specificDialogue;
			}
		}
		if (dialogue != null && base.Name.Equals("Caroline") && Game1.isLocationAccessible("CommunityCenter") && preface == "summer_" && day_name == "Mon")
		{
			dialogue = TryGetDialogue("summer_Wed");
		}
		if (dialogue != null)
		{
			return dialogue;
		}
		return null;
	}

	public virtual void checkSchedule(int timeOfDay)
	{
		if (currentScheduleDelay == 0f && scheduleDelaySeconds > 0f)
		{
			currentScheduleDelay = scheduleDelaySeconds;
		}
		else
		{
			if (returningToEndPoint)
			{
				return;
			}
			updatedDialogueYet = false;
			extraDialogueMessageToAddThisMorning = null;
			if (ignoreScheduleToday || Schedule == null)
			{
				return;
			}
			SchedulePathDescription possibleNewDirections = null;
			if (lastAttemptedSchedule < timeOfDay)
			{
				lastAttemptedSchedule = timeOfDay;
				Schedule.TryGetValue(timeOfDay, out possibleNewDirections);
				if (possibleNewDirections != null)
				{
					queuedSchedulePaths.Add(possibleNewDirections);
				}
				possibleNewDirections = null;
			}
			if (controller != null && controller.pathToEndPoint != null && controller.pathToEndPoint.Count > 0)
			{
				return;
			}
			if (queuedSchedulePaths.Count > 0 && timeOfDay >= queuedSchedulePaths[0].time)
			{
				possibleNewDirections = queuedSchedulePaths[0];
			}
			if (possibleNewDirections == null)
			{
				return;
			}
			prepareToDisembarkOnNewSchedulePath();
			if (!returningToEndPoint && temporaryController == null)
			{
				directionsToNewLocation = possibleNewDirections;
				if (queuedSchedulePaths.Count > 0)
				{
					queuedSchedulePaths.RemoveAt(0);
				}
				controller = new PathFindController(directionsToNewLocation.route, this, Utility.getGameLocationOfCharacter(this))
				{
					finalFacingDirection = directionsToNewLocation.facingDirection,
					endBehaviorFunction = getRouteEndBehaviorFunction(directionsToNewLocation.endOfRouteBehavior, directionsToNewLocation.endOfRouteMessage)
				};
				if (controller.pathToEndPoint == null || controller.pathToEndPoint.Count == 0)
				{
					controller.endBehaviorFunction?.Invoke(this, base.currentLocation);
					controller = null;
				}
				if (directionsToNewLocation?.route != null)
				{
					previousEndPoint = directionsToNewLocation.route.LastOrDefault();
				}
			}
		}
	}

	private void finishEndOfRouteAnimation()
	{
		_finishingEndOfRouteBehavior = _startedEndOfRouteBehavior;
		_startedEndOfRouteBehavior = null;
		string finishingEndOfRouteBehavior = _finishingEndOfRouteBehavior;
		if (!(finishingEndOfRouteBehavior == "change_beach"))
		{
			if (finishingEndOfRouteBehavior == "change_normal")
			{
				shouldWearIslandAttire.Value = false;
				currentlyDoingEndOfRouteAnimation = false;
			}
		}
		else
		{
			shouldWearIslandAttire.Value = true;
			currentlyDoingEndOfRouteAnimation = false;
		}
		while (CurrentDialogue.Count > 0 && CurrentDialogue.Peek().removeOnNextMove)
		{
			CurrentDialogue.Pop();
		}
		shouldSayMarriageDialogue.Value = false;
		currentMarriageDialogue.Clear();
		nextEndOfRouteMessage = null;
		endOfRouteMessage.Value = null;
		if (currentlyDoingEndOfRouteAnimation && routeEndOutro != null)
		{
			bool addedFrame = false;
			for (int i = 0; i < routeEndOutro.Length; i++)
			{
				if (!addedFrame)
				{
					Sprite.ClearAnimation();
					addedFrame = true;
				}
				if (i == routeEndOutro.Length - 1)
				{
					Sprite.AddFrame(new FarmerSprite.AnimationFrame(routeEndOutro[i], 100, 0, secondaryArm: false, flip: false, routeEndAnimationFinished, behaviorAtEndOfFrame: true));
				}
				else
				{
					Sprite.AddFrame(new FarmerSprite.AnimationFrame(routeEndOutro[i], 100, 0, secondaryArm: false, flip: false));
				}
			}
			if (!addedFrame)
			{
				routeEndAnimationFinished(null);
			}
			if (_finishingEndOfRouteBehavior != null)
			{
				finishRouteBehavior(_finishingEndOfRouteBehavior);
			}
		}
		else
		{
			routeEndAnimationFinished(null);
		}
	}

	protected virtual void prepareToDisembarkOnNewSchedulePath()
	{
		finishEndOfRouteAnimation();
		doingEndOfRouteAnimation.Value = false;
		currentlyDoingEndOfRouteAnimation = false;
		if (!isMarried())
		{
			return;
		}
		if (temporaryController == null && Utility.getGameLocationOfCharacter(this) is FarmHouse)
		{
			temporaryController = new PathFindController(this, getHome(), new Point(getHome().warps[0].X, getHome().warps[0].Y), 2, clearMarriageDialogues: true)
			{
				NPCSchedule = true
			};
			if (temporaryController.pathToEndPoint == null || temporaryController.pathToEndPoint.Count <= 0)
			{
				temporaryController = null;
				ClearSchedule();
			}
			else
			{
				followSchedule = true;
			}
		}
		else if (Utility.getGameLocationOfCharacter(this) is Farm)
		{
			temporaryController = null;
			ClearSchedule();
		}
	}

	public void checkForMarriageDialogue(int timeOfDay, GameLocation location)
	{
		if (base.Name == "Krobus" && Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth) == "Fri")
		{
			return;
		}
		switch (timeOfDay)
		{
		case 1100:
			setRandomAfternoonMarriageDialogue(1100, location);
			break;
		case 1800:
			if (location is FarmHouse)
			{
				int which = Utility.CreateDaySaveRandom(timeOfDay, getSpouse().UniqueMultiplayerID).Next(Game1.isRaining ? 7 : 6) - 1;
				string suffix = ((which >= 0) ? (which.ToString() ?? "") : base.Name);
				currentMarriageDialogue.Clear();
				addMarriageDialogue("MarriageDialogue", (Game1.isRaining ? "Rainy" : "Indoor") + "_Night_" + suffix, false);
			}
			break;
		}
	}

	private void routeEndAnimationFinished(Farmer who)
	{
		doingEndOfRouteAnimation.Value = false;
		freezeMotion = false;
		CharacterData data = GetData();
		Sprite.SpriteWidth = data?.Size.X ?? 16;
		Sprite.SpriteHeight = data?.Size.Y ?? 32;
		Sprite.UpdateSourceRect();
		Sprite.oldFrame = _beforeEndOfRouteAnimationFrame;
		Sprite.StopAnimation();
		endOfRouteMessage.Value = null;
		isCharging = false;
		base.speed = 2;
		addedSpeed = 0f;
		goingToDoEndOfRouteAnimation.Value = false;
		if (isWalkingInSquare)
		{
			returningToEndPoint = true;
		}
		if (_finishingEndOfRouteBehavior == "penny_dishes")
		{
			drawOffset = Vector2.Zero;
		}
		if (appliedRouteAnimationOffset != Vector2.Zero)
		{
			drawOffset = Vector2.Zero;
			appliedRouteAnimationOffset = Vector2.Zero;
		}
		_finishingEndOfRouteBehavior = null;
	}

	public bool isOnSilentTemporaryMessage()
	{
		if (((bool)doingEndOfRouteAnimation || !goingToDoEndOfRouteAnimation) && endOfRouteMessage.Value != null && endOfRouteMessage.Value.ToLower().Equals("silent"))
		{
			return true;
		}
		return false;
	}

	public bool hasTemporaryMessageAvailable()
	{
		if (isDivorcedFrom(Game1.player))
		{
			return false;
		}
		if (base.currentLocation != null && base.currentLocation.HasLocationOverrideDialogue(this))
		{
			return true;
		}
		if (endOfRouteMessage.Value != null && ((bool)doingEndOfRouteAnimation || !goingToDoEndOfRouteAnimation))
		{
			return true;
		}
		return false;
	}

	public bool setTemporaryMessages(Farmer who)
	{
		if (isOnSilentTemporaryMessage())
		{
			return true;
		}
		if (endOfRouteMessage.Value != null && ((bool)doingEndOfRouteAnimation || !goingToDoEndOfRouteAnimation))
		{
			if (!isDivorcedFrom(Game1.player) && (!endOfRouteMessage.Value.Contains("marriage") || getSpouse() == Game1.player))
			{
				_PushTemporaryDialogue(endOfRouteMessage);
				return false;
			}
		}
		else if (base.currentLocation != null && base.currentLocation.HasLocationOverrideDialogue(this))
		{
			_PushTemporaryDialogue(base.currentLocation.GetLocationOverrideDialogue(this));
			return false;
		}
		return false;
	}

	protected void _PushTemporaryDialogue(string dialogue_key)
	{
		string oldDialogueKey = dialogue_key;
		try
		{
			if (dialogue_key.StartsWith("Resort"))
			{
				string alternate_key = "Resort_Marriage" + dialogue_key.Substring(6);
				if (Game1.content.LoadStringReturnNullIfNotFound(alternate_key) != null)
				{
					dialogue_key = alternate_key;
				}
			}
			if (CurrentDialogue.Count == 0 || CurrentDialogue.Peek().temporaryDialogueKey != dialogue_key)
			{
				Dialogue temporary_dialogue = new Dialogue(this, dialogue_key)
				{
					removeOnNextMove = true,
					temporaryDialogueKey = dialogue_key
				};
				CurrentDialogue.Push(temporary_dialogue);
			}
		}
		catch (Exception ex)
		{
			Game1.log.Error($"NPC '{base.Name}' failed setting temporary dialogue key '{dialogue_key}'{((dialogue_key != oldDialogueKey) ? (" (from dialogue key '" + oldDialogueKey + "')") : "")}", ex);
		}
	}

	private void walkInSquareAtEndOfRoute(Character c, GameLocation l)
	{
		startRouteBehavior(endOfRouteBehaviorName);
	}

	private void doAnimationAtEndOfScheduleRoute(Character c, GameLocation l)
	{
		doingEndOfRouteAnimation.Value = true;
		reallyDoAnimationAtEndOfScheduleRoute();
		currentlyDoingEndOfRouteAnimation = true;
	}

	private void reallyDoAnimationAtEndOfScheduleRoute()
	{
		_startedEndOfRouteBehavior = loadedEndOfRouteBehavior;
		bool is_special_route_behavior = false;
		string startedEndOfRouteBehavior = _startedEndOfRouteBehavior;
		if (startedEndOfRouteBehavior == "change_beach" || startedEndOfRouteBehavior == "change_normal")
		{
			is_special_route_behavior = true;
		}
		if (!is_special_route_behavior)
		{
			if (_startedEndOfRouteBehavior == "penny_dishes")
			{
				drawOffset = new Vector2(0f, 16f);
			}
			if (_startedEndOfRouteBehavior.EndsWith("_sleep"))
			{
				layingDown = true;
				HideShadow = true;
			}
			if (routeAnimationMetadata != null)
			{
				for (int i = 0; i < routeAnimationMetadata.Length; i++)
				{
					string[] metadata = ArgUtility.SplitBySpace(routeAnimationMetadata[i]);
					startedEndOfRouteBehavior = metadata[0];
					if (!(startedEndOfRouteBehavior == "laying_down"))
					{
						if (startedEndOfRouteBehavior == "offset")
						{
							appliedRouteAnimationOffset = new Vector2(int.Parse(metadata[1]), int.Parse(metadata[2]));
						}
					}
					else
					{
						layingDown = true;
						HideShadow = true;
					}
				}
			}
			if (appliedRouteAnimationOffset != Vector2.Zero)
			{
				drawOffset = appliedRouteAnimationOffset;
			}
			if (_skipRouteEndIntro)
			{
				doMiddleAnimation(null);
			}
			else
			{
				Sprite.ClearAnimation();
				for (int i = 0; i < routeEndIntro.Length; i++)
				{
					if (i == routeEndIntro.Length - 1)
					{
						Sprite.AddFrame(new FarmerSprite.AnimationFrame(routeEndIntro[i], 100, 0, secondaryArm: false, flip: false, doMiddleAnimation, behaviorAtEndOfFrame: true));
					}
					else
					{
						Sprite.AddFrame(new FarmerSprite.AnimationFrame(routeEndIntro[i], 100, 0, secondaryArm: false, flip: false));
					}
				}
			}
		}
		_skipRouteEndIntro = false;
		doingEndOfRouteAnimation.Value = true;
		freezeMotion = true;
		_beforeEndOfRouteAnimationFrame = Sprite.oldFrame;
	}

	private void doMiddleAnimation(Farmer who)
	{
		Sprite.ClearAnimation();
		for (int i = 0; i < routeEndAnimation.Length; i++)
		{
			Sprite.AddFrame(new FarmerSprite.AnimationFrame(routeEndAnimation[i], 100, 0, secondaryArm: false, flip: false));
		}
		Sprite.loop = true;
		if (_startedEndOfRouteBehavior != null)
		{
			startRouteBehavior(_startedEndOfRouteBehavior);
		}
	}

	private void startRouteBehavior(string behaviorName)
	{
		if (behaviorName.Length > 0 && behaviorName[0] == '"')
		{
			if (Game1.IsMasterGame)
			{
				endOfRouteMessage.Value = behaviorName.Replace("\"", "");
			}
			return;
		}
		if (behaviorName.Contains("square_") && Game1.IsMasterGame)
		{
			lastCrossroad = new Microsoft.Xna.Framework.Rectangle(base.TilePoint.X * 64, base.TilePoint.Y * 64, 64, 64);
			string[] squareSplit = behaviorName.Split('_');
			walkInSquare(Convert.ToInt32(squareSplit[1]), Convert.ToInt32(squareSplit[2]), 6000);
			if (squareSplit.Length > 3)
			{
				squareMovementFacingPreference = Convert.ToInt32(squareSplit[3]);
			}
			else
			{
				squareMovementFacingPreference = -1;
			}
		}
		if (behaviorName.Contains("sleep"))
		{
			isPlayingSleepingAnimation = true;
			playSleepingAnimation();
		}
		switch (behaviorName)
		{
		case "abigail_videogames":
			if (Game1.IsMasterGame)
			{
				Game1.multiplayer.broadcastSprites(Utility.getGameLocationOfCharacter(this), new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(167, 1714, 19, 14), 100f, 3, 999999, new Vector2(2f, 3f) * 64f + new Vector2(7f, 12f) * 4f, flicker: false, flipped: false, 0.0002f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					id = 688
				});
				doEmote(52);
			}
			break;
		case "dick_fish":
			extendSourceRect(0, 32);
			Sprite.tempSpriteHeight = 64;
			drawOffset = new Vector2(0f, 96f);
			Sprite.ignoreSourceRectUpdates = false;
			if (Utility.isOnScreen(Utility.Vector2ToPoint(base.Position), 64, base.currentLocation))
			{
				base.currentLocation.playSound("slosh", base.Tile);
			}
			break;
		case "clint_hammer":
			extendSourceRect(16, 0);
			Sprite.SpriteWidth = 32;
			Sprite.ignoreSourceRectUpdates = false;
			Sprite.currentFrame = 8;
			Sprite.CurrentAnimation[14] = new FarmerSprite.AnimationFrame(9, 100, 0, secondaryArm: false, flip: false, clintHammerSound);
			break;
		case "birdie_fish":
			extendSourceRect(16, 0);
			Sprite.SpriteWidth = 32;
			Sprite.ignoreSourceRectUpdates = false;
			Sprite.currentFrame = 8;
			break;
		}
	}

	public void playSleepingAnimation()
	{
		isSleeping.Value = true;
		Vector2 draw_offset = new Vector2(0f, name.Equals("Sebastian") ? 12 : (-4));
		if (isMarried())
		{
			draw_offset.X = -12f;
		}
		drawOffset = draw_offset;
		if (!isPlayingSleepingAnimation)
		{
			if (DataLoader.AnimationDescriptions(Game1.content).TryGetValue(name.Value.ToLower() + "_sleep", out var animationData))
			{
				int sleep_frame = Convert.ToInt32(animationData.Split('/')[0]);
				Sprite.ClearAnimation();
				Sprite.AddFrame(new FarmerSprite.AnimationFrame(sleep_frame, 100, secondaryArm: false, flip: false));
				Sprite.loop = true;
			}
			isPlayingSleepingAnimation = true;
		}
	}

	private void finishRouteBehavior(string behaviorName)
	{
		switch (behaviorName)
		{
		case "abigail_videogames":
			Utility.getGameLocationOfCharacter(this).removeTemporarySpritesWithID(688);
			break;
		case "birdie_fish":
		case "clint_hammer":
		case "dick_fish":
		{
			reloadSprite();
			CharacterData data = GetData();
			Sprite.SpriteWidth = data?.Size.X ?? 16;
			Sprite.SpriteHeight = data?.Size.Y ?? 32;
			Sprite.UpdateSourceRect();
			drawOffset = Vector2.Zero;
			Halt();
			movementPause = 1;
			break;
		}
		}
		if (layingDown)
		{
			layingDown = false;
			HideShadow = false;
		}
	}

	public bool IsReturningToEndPoint()
	{
		return returningToEndPoint;
	}

	public void StartActivityWalkInSquare(int square_width, int square_height, int pause_offset)
	{
		Point tile = base.TilePoint;
		lastCrossroad = new Microsoft.Xna.Framework.Rectangle(tile.X * 64, tile.Y * 64, 64, 64);
		walkInSquare(square_height, square_height, pause_offset);
	}

	public void EndActivityRouteEndBehavior()
	{
		finishEndOfRouteAnimation();
	}

	public void StartActivityRouteEndBehavior(string behavior_name, string end_message)
	{
		getRouteEndBehaviorFunction(behavior_name, end_message)?.Invoke(this, base.currentLocation);
	}

	protected PathFindController.endBehavior getRouteEndBehaviorFunction(string behaviorName, string endMessage)
	{
		if (endMessage != null || (behaviorName != null && behaviorName.Length > 0 && behaviorName[0] == '"'))
		{
			nextEndOfRouteMessage = endMessage.Replace("\"", "");
		}
		if (behaviorName != null)
		{
			if (behaviorName.Length > 0 && behaviorName.Contains("square_"))
			{
				endOfRouteBehaviorName.Value = behaviorName;
				return walkInSquareAtEndOfRoute;
			}
			Dictionary<string, string> animationDescriptions = DataLoader.AnimationDescriptions(Game1.content);
			if (behaviorName == "change_beach" || behaviorName == "change_normal")
			{
				endOfRouteBehaviorName.Value = behaviorName;
				goingToDoEndOfRouteAnimation.Value = true;
			}
			else
			{
				if (!animationDescriptions.ContainsKey(behaviorName))
				{
					return null;
				}
				endOfRouteBehaviorName.Value = behaviorName;
				loadEndOfRouteBehavior(endOfRouteBehaviorName);
				goingToDoEndOfRouteAnimation.Value = true;
			}
			return doAnimationAtEndOfScheduleRoute;
		}
		return null;
	}

	private void loadEndOfRouteBehavior(string name)
	{
		loadedEndOfRouteBehavior = name;
		if (name.Length > 0 && name.Contains("square_"))
		{
			return;
		}
		string rawData = null;
		try
		{
			if (DataLoader.AnimationDescriptions(Game1.content).TryGetValue(name, out rawData))
			{
				string[] fields = rawData.Split('/');
				routeEndIntro = Utility.parseStringToIntArray(fields[0]);
				routeEndAnimation = Utility.parseStringToIntArray(fields[1]);
				routeEndOutro = Utility.parseStringToIntArray(fields[2]);
				if (fields.Length > 3 && fields[3] != "")
				{
					nextEndOfRouteMessage = fields[3];
				}
				if (fields.Length > 4)
				{
					routeAnimationMetadata = fields.Skip(4).ToArray();
				}
				else
				{
					routeAnimationMetadata = null;
				}
			}
		}
		catch (Exception ex)
		{
			Game1.log.Error($"NPC {base.Name} failed to apply end-of-route behavior '{name}'{((rawData != null) ? (" with raw data '" + rawData + "'") : "")}.", ex);
		}
	}

	public void shake(int duration)
	{
		shakeTimer = duration;
	}

	public void setNewDialogue(string translationKey, bool add = false, bool clearOnMovement = false)
	{
		setNewDialogue(new Dialogue(this, translationKey), add, clearOnMovement);
	}

	public void setNewDialogue(Dialogue dialogue, bool add = false, bool clearOnMovement = false)
	{
		if (!add)
		{
			CurrentDialogue.Clear();
		}
		dialogue.removeOnNextMove = clearOnMovement;
		CurrentDialogue.Push(dialogue);
	}

	private void setNewDialogue(string dialogueSheetName, string dialogueSheetKey, bool clearOnMovement = false)
	{
		CurrentDialogue.Clear();
		string nameToAppend = base.Name;
		Dialogue dialogue;
		if (dialogueSheetName.Contains("Marriage"))
		{
			if (getSpouse() == Game1.player)
			{
				dialogue = tryToGetMarriageSpecificDialogue(dialogueSheetKey + nameToAppend) ?? new Dialogue(this, null, "");
				dialogue.removeOnNextMove = clearOnMovement;
				CurrentDialogue.Push(dialogue);
			}
			return;
		}
		dialogue = StardewValley.Dialogue.TryGetDialogue(this, "Characters\\Dialogue\\" + dialogueSheetName + ":" + dialogueSheetKey + nameToAppend);
		if (dialogue != null)
		{
			dialogue.removeOnNextMove = clearOnMovement;
			CurrentDialogue.Push(dialogue);
		}
	}

	public string GetDialogueSheetName()
	{
		if (base.Name == "Leo" && DefaultMap != "IslandHut")
		{
			return base.Name + "Mainland";
		}
		return base.Name;
	}

	public void setSpouseRoomMarriageDialogue()
	{
		currentMarriageDialogue.Clear();
		addMarriageDialogue("MarriageDialogue", "spouseRoom_" + base.Name, false);
	}

	public void setRandomAfternoonMarriageDialogue(int time, GameLocation location, bool countAsDailyAfternoon = false)
	{
		if ((base.Name == "Krobus" && Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth) == "Fri") || hasSaidAfternoonDialogue.Value)
		{
			return;
		}
		if (countAsDailyAfternoon)
		{
			hasSaidAfternoonDialogue.Value = true;
		}
		Random r = Utility.CreateDaySaveRandom(time);
		int hearts = getSpouse().getFriendshipHeartLevelForNPC(base.Name);
		if (!(location is FarmHouse))
		{
			if (location is Farm)
			{
				currentMarriageDialogue.Clear();
				if (r.NextDouble() < 0.2)
				{
					addMarriageDialogue("MarriageDialogue", "Outdoor_" + base.Name, false);
				}
				else
				{
					addMarriageDialogue("MarriageDialogue", "Outdoor_" + r.Next(5), false);
				}
			}
		}
		else if (r.NextBool())
		{
			if (hearts < 9)
			{
				currentMarriageDialogue.Clear();
				addMarriageDialogue("MarriageDialogue", (r.NextDouble() < (double)((float)hearts / 11f)) ? "Neutral_" : ("Bad_" + r.Next(10)), false);
			}
			else if (r.NextDouble() < 0.05)
			{
				currentMarriageDialogue.Clear();
				addMarriageDialogue("MarriageDialogue", Game1.currentSeason + "_" + base.Name, false);
			}
			else if ((hearts >= 10 && r.NextBool()) || (hearts >= 11 && r.NextDouble() < 0.75) || (hearts >= 12 && r.NextDouble() < 0.95))
			{
				currentMarriageDialogue.Clear();
				addMarriageDialogue("MarriageDialogue", "Good_" + r.Next(10), false);
			}
			else
			{
				currentMarriageDialogue.Clear();
				addMarriageDialogue("MarriageDialogue", "Neutral_" + r.Next(10), false);
			}
		}
	}

	/// <summary>Get whether it's the NPC's birthday today.</summary>
	public bool isBirthday()
	{
		if (Birthday_Season == Game1.currentSeason)
		{
			return Birthday_Day == Game1.dayOfMonth;
		}
		return false;
	}

	/// <summary>Get the NPC's first loved item for the Statue of Endless Fortune.</summary>
	public Item getFavoriteItem()
	{
		if (Game1.NPCGiftTastes.TryGetValue(base.Name, out var rawData))
		{
			Item item = (from id in ArgUtility.SplitBySpace(rawData.Split('/')[1])
				select ItemRegistry.ResolveMetadata(id)?.CreateItem()).FirstOrDefault((Item p) => p != null);
			if (item != null)
			{
				return item;
			}
		}
		return null;
	}

	/// <summary>Get the NPC's data from <see cref="F:StardewValley.Game1.characterData" />, if found.</summary>
	public CharacterData GetData()
	{
		if (!IsVillager || !TryGetData(name.Value, out var data))
		{
			return null;
		}
		return data;
	}

	/// <summary>Try to get an NPC's data from <see cref="F:StardewValley.Game1.characterData" />.</summary>
	/// <param name="name">The NPC's internal name (i.e. the key in <see cref="F:StardewValley.Game1.characterData" />).</param>
	/// <param name="data">The NPC data, if found.</param>
	/// <returns>Returns whether the NPC data was found.</returns>
	public static bool TryGetData(string name, out CharacterData data)
	{
		if (name == null)
		{
			data = null;
			return false;
		}
		return Game1.characterData.TryGetValue(name, out data);
	}

	/// <summary>Get the translated display name for an NPC from the underlying data, if any.</summary>
	/// <param name="name">The NPC's internal name.</param>
	public static string GetDisplayName(string name)
	{
		TryGetData(name, out var data);
		return TokenParser.ParseText(data?.DisplayName) ?? name;
	}

	/// <summary>Get a tokenized string for the NPC's display name.</summary>
	public string GetTokenizedDisplayName()
	{
		return GetData()?.DisplayName ?? displayName;
	}

	/// <summary>Get whether this NPC speaks Dwarvish, which the player can only understand after finding the Dwarvish Translation Guide.</summary>
	public bool SpeaksDwarvish()
	{
		CharacterData data = GetData();
		if (data == null)
		{
			return false;
		}
		return data.Language == NpcLanguage.Dwarvish;
	}

	public virtual void receiveGift(Object o, Farmer giver, bool updateGiftLimitInfo = true, float friendshipChangeMultiplier = 1f, bool showResponse = true)
	{
		if (CanReceiveGifts())
		{
			float qualityChangeMultipler = 1f;
			switch (o.Quality)
			{
			case 1:
				qualityChangeMultipler = 1.1f;
				break;
			case 2:
				qualityChangeMultipler = 1.25f;
				break;
			case 4:
				qualityChangeMultipler = 1.5f;
				break;
			}
			if (isBirthday())
			{
				friendshipChangeMultiplier = 8f;
			}
			if (getSpouse() != null && getSpouse().Equals(giver))
			{
				friendshipChangeMultiplier /= 2f;
			}
			giver.onGiftGiven(this, o);
			Game1.stats.GiftsGiven++;
			giver.currentLocation.localSound("give_gift");
			if (updateGiftLimitInfo)
			{
				giver.friendshipData[base.Name].GiftsToday++;
				giver.friendshipData[base.Name].GiftsThisWeek++;
				giver.friendshipData[base.Name].LastGiftDate = new WorldDate(Game1.Date);
			}
			switch (giver.FacingDirection)
			{
			case 0:
				((FarmerSprite)giver.Sprite).animateBackwardsOnce(80, 50f);
				break;
			case 1:
				((FarmerSprite)giver.Sprite).animateBackwardsOnce(72, 50f);
				break;
			case 2:
				((FarmerSprite)giver.Sprite).animateBackwardsOnce(64, 50f);
				break;
			case 3:
				((FarmerSprite)giver.Sprite).animateBackwardsOnce(88, 50f);
				break;
			}
			int tasteForItem = getGiftTasteForThisItem(o);
			switch (tasteForItem)
			{
			case 7:
				giver.changeFriendship(Math.Min(750, (int)(250f * friendshipChangeMultiplier)), this);
				doEmote(56);
				faceTowardFarmerForPeriod(15000, 4, faceAway: false, giver);
				break;
			case 0:
				giver.changeFriendship((int)(80f * friendshipChangeMultiplier * qualityChangeMultipler), this);
				doEmote(20);
				faceTowardFarmerForPeriod(15000, 4, faceAway: false, giver);
				break;
			case 6:
				giver.changeFriendship((int)(-40f * friendshipChangeMultiplier), this);
				doEmote(12);
				faceTowardFarmerForPeriod(15000, 4, faceAway: true, giver);
				break;
			case 2:
				giver.changeFriendship((int)(45f * friendshipChangeMultiplier * qualityChangeMultipler), this);
				faceTowardFarmerForPeriod(7000, 3, faceAway: true, giver);
				break;
			case 4:
				giver.changeFriendship((int)(-20f * friendshipChangeMultiplier), this);
				break;
			default:
				giver.changeFriendship((int)(20f * friendshipChangeMultiplier), this);
				break;
			}
			if (showResponse)
			{
				Game1.DrawDialogue(GetGiftReaction(giver, o, tasteForItem));
			}
		}
	}

	/// <summary>Get the NPC's reaction dialogue for receiving an item as a gift.</summary>
	/// <param name="giver">The player giving the gift.</param>
	/// <param name="gift">The item being gifted.</param>
	/// <param name="taste">The NPC's gift taste for this item, as returned by <see cref="M:StardewValley.NPC.getGiftTasteForThisItem(StardewValley.Item)" />.</param>
	/// <returns>Returns the dialogue if the NPC can receive gifts, else <c>null</c>.</returns>
	public virtual Dialogue GetGiftReaction(Farmer giver, Object gift, int taste)
	{
		if (!CanReceiveGifts() || !Game1.NPCGiftTastes.TryGetValue(base.Name, out var rawData))
		{
			return null;
		}
		Dialogue dialogue = null;
		string portrait = null;
		if (base.Name == "Krobus" && Game1.Date.DayOfWeek == DayOfWeek.Friday)
		{
			dialogue = new Dialogue(this, null, "...");
		}
		else if (isBirthday())
		{
			dialogue = TryGetDialogue("AcceptBirthdayGift_" + gift.QualifiedItemId) ?? (from tag in gift.GetContextTags()
				select TryGetDialogue("AcceptBirthdayGift_" + tag)).FirstOrDefault((Dialogue p) => p != null);
			switch (taste)
			{
			case 0:
			case 2:
			case 7:
				portrait = "$h";
				dialogue = dialogue ?? TryGetDialogue((taste == 0) ? "AcceptBirthdayGift_Loved" : "AcceptBirthdayGift_Liked") ?? TryGetDialogue("AcceptBirthdayGift_Positive") ?? TryGetDialogue("AcceptBirthdayGift") ?? ((!Game1.random.NextBool()) ? ((Manners == 2) ? new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.4274", isGendered: true) : new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.4275")) : ((Manners == 2) ? new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.4276", isGendered: true) : new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.4277", isGendered: true)));
				break;
			case 4:
			case 6:
				portrait = "$s";
				dialogue = dialogue ?? TryGetDialogue((taste == 4) ? "AcceptBirthdayGift_Disliked" : "AcceptBirthdayGift_Hated") ?? TryGetDialogue("AcceptBirthdayGift_Negative") ?? TryGetDialogue("AcceptBirthdayGift") ?? ((Manners == 2) ? new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.4278", isGendered: true) : new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.4279", isGendered: true));
				break;
			default:
				dialogue = dialogue ?? TryGetDialogue("AcceptBirthdayGift_Neutral") ?? TryGetDialogue("AcceptBirthdayGift_Positive") ?? TryGetDialogue("AcceptBirthdayGift") ?? ((Manners == 2) ? new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.4280") : new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.4281", isGendered: true));
				break;
			}
		}
		else
		{
			dialogue = TryGetDialogue("AcceptGift_" + gift.QualifiedItemId) ?? (from tag in gift.GetContextTags()
				select TryGetDialogue("AcceptGift_" + tag)).FirstOrDefault((Dialogue p) => p != null);
			string[] rawFields = rawData.Split('/');
			switch (taste)
			{
			case 7:
				portrait = "$h";
				dialogue = dialogue ?? new Dialogue(this, null, ArgUtility.Get(rawFields, taste));
				break;
			case 0:
			case 2:
				if (dialogue == null)
				{
					portrait = "$h";
				}
				dialogue = dialogue ?? new Dialogue(this, null, ArgUtility.Get(rawFields, taste));
				break;
			case 4:
			case 6:
				portrait = "$s";
				dialogue = dialogue ?? new Dialogue(this, null, ArgUtility.Get(rawFields, taste));
				break;
			default:
				dialogue = dialogue ?? new Dialogue(this, null, ArgUtility.Get(rawFields, 8));
				break;
			}
		}
		if (!giver.canUnderstandDwarves && SpeaksDwarvish())
		{
			dialogue.convertToDwarvish();
		}
		else if (portrait != null && !dialogue.CurrentEmotionSetExplicitly)
		{
			dialogue.CurrentEmotion = portrait;
		}
		return dialogue;
	}

	public override void draw(SpriteBatch b, float alpha = 1f)
	{
		int standingY = base.StandingPixel.Y;
		float mainLayerDepth = Math.Max(0f, drawOnTop ? 0.991f : ((float)standingY / 10000f));
		if (Sprite.Texture == null)
		{
			Vector2 position = Game1.GlobalToLocal(Game1.viewport, base.Position);
			Microsoft.Xna.Framework.Rectangle spriteArea = new Microsoft.Xna.Framework.Rectangle((int)position.X, (int)position.Y - Sprite.SpriteWidth * 4, Sprite.SpriteWidth * 4, Sprite.SpriteHeight * 4);
			Utility.DrawErrorTexture(b, spriteArea, mainLayerDepth);
		}
		else if (!IsInvisible && (Utility.isOnScreen(base.Position, 128) || (EventActor && base.currentLocation is Summit)))
		{
			if ((bool)swimming)
			{
				b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(32f, 80 + yJumpOffset * 2) + ((shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero) - new Vector2(0f, yOffset), new Microsoft.Xna.Framework.Rectangle(Sprite.SourceRect.X, Sprite.SourceRect.Y, Sprite.SourceRect.Width, Sprite.SourceRect.Height / 2 - (int)(yOffset / 4f)), Color.White, rotation, new Vector2(32f, 96f) / 4f, Math.Max(0.2f, scale.Value) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, mainLayerDepth);
				Vector2 localPosition = getLocalPosition(Game1.viewport);
				b.Draw(Game1.staminaRect, new Microsoft.Xna.Framework.Rectangle((int)localPosition.X + (int)yOffset + 8, (int)localPosition.Y - 128 + Sprite.SourceRect.Height * 4 + 48 + yJumpOffset * 2 - (int)yOffset, Sprite.SourceRect.Width * 4 - (int)yOffset * 2 - 16, 4), Game1.staminaRect.Bounds, Color.White * 0.75f, 0f, Vector2.Zero, SpriteEffects.None, (float)standingY / 10000f + 0.001f);
			}
			else
			{
				b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(GetSpriteWidthForPositioning() * 4 / 2, GetBoundingBox().Height / 2) + ((shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), Sprite.SourceRect, Color.White * alpha, rotation, new Vector2(Sprite.SpriteWidth / 2, (float)Sprite.SpriteHeight * 3f / 4f), Math.Max(0.2f, scale.Value) * 4f, (flip || (Sprite.CurrentAnimation != null && Sprite.CurrentAnimation[Sprite.currentAnimationIndex].flip)) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, mainLayerDepth);
			}
			DrawBreathing(b, alpha);
			DrawGlow(b);
			DrawEmote(b);
		}
	}

	public virtual void DrawBreathing(SpriteBatch b, float alpha = 1f)
	{
		if (!Breather || shakeTimer > 0 || swimming.Value || farmerPassesThrough)
		{
			return;
		}
		AnimatedSprite animatedSprite = Sprite;
		if (animatedSprite != null && animatedSprite.SpriteHeight > 32)
		{
			return;
		}
		AnimatedSprite animatedSprite2 = Sprite;
		if (animatedSprite2 != null && animatedSprite2.SpriteWidth > 16)
		{
			return;
		}
		AnimatedSprite sprite = Sprite;
		if (sprite.currentFrame >= 16)
		{
			return;
		}
		CharacterData data = GetData();
		Microsoft.Xna.Framework.Rectangle spriteRect = sprite.SourceRect;
		Microsoft.Xna.Framework.Rectangle chestBox;
		if (data != null && data.BreathChestRect.HasValue)
		{
			Microsoft.Xna.Framework.Rectangle dataRect = data.BreathChestRect.Value;
			chestBox = new Microsoft.Xna.Framework.Rectangle(spriteRect.X + dataRect.X, spriteRect.Y + dataRect.Y, dataRect.Width, dataRect.Height);
		}
		else
		{
			chestBox = new Microsoft.Xna.Framework.Rectangle(spriteRect.X + sprite.SpriteWidth / 4, spriteRect.Y + sprite.SpriteHeight / 2 + sprite.SpriteHeight / 32, sprite.SpriteHeight / 4, sprite.SpriteWidth / 2);
			if (Age == 2)
			{
				chestBox.Y += sprite.SpriteHeight / 6 + 1;
				chestBox.Height /= 2;
			}
			else if (Gender == Gender.Female)
			{
				chestBox.Y++;
				chestBox.Height /= 2;
			}
		}
		Vector2 chestPosition;
		if (data != null && data.BreathChestPosition.HasValue)
		{
			chestPosition = Utility.PointToVector2(data.BreathChestPosition.Value);
		}
		else
		{
			chestPosition = new Vector2(sprite.SpriteWidth * 4 / 2, 8f);
			if (Age == 2)
			{
				chestPosition.Y += sprite.SpriteHeight / 8 * 4;
				if (this is Child { Age: var num })
				{
					switch (num)
					{
					case 0:
						chestPosition.X -= 12f;
						break;
					case 1:
						chestPosition.X -= 4f;
						break;
					}
				}
			}
			else if (Gender == Gender.Female)
			{
				chestPosition.Y -= 4f;
			}
		}
		float breathScale = Math.Max(0f, (float)Math.Ceiling(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 600.0 + (double)(defaultPosition.X * 20f))) / 4f);
		int standingY = base.StandingPixel.Y;
		b.Draw(sprite.Texture, getLocalPosition(Game1.viewport) + chestPosition + ((shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), chestBox, Color.White * alpha, rotation, new Vector2(chestBox.Width / 2, chestBox.Height / 2 + 1), Math.Max(0.2f, scale.Value) * 4f + breathScale, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.992f : (((float)standingY + 0.01f) / 10000f)));
	}

	public virtual void DrawGlow(SpriteBatch b)
	{
		int standingY = base.StandingPixel.Y;
		if (isGlowing)
		{
			b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(GetSpriteWidthForPositioning() * 4 / 2, GetBoundingBox().Height / 2) + ((shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), Sprite.SourceRect, glowingColor * glowingTransparency, rotation, new Vector2(Sprite.SpriteWidth / 2, (float)Sprite.SpriteHeight * 3f / 4f), Math.Max(0.2f, scale.Value) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.99f : ((float)standingY / 10000f + 0.001f)));
		}
	}

	public virtual void DrawEmote(SpriteBatch b)
	{
		if (base.IsEmoting && !Game1.eventUp && !(this is Child) && !(this is Pet))
		{
			int standingY = base.StandingPixel.Y;
			Point dataOffset = GetData()?.EmoteOffset ?? Point.Zero;
			Vector2 emotePosition = getLocalPosition(Game1.viewport);
			b.Draw(position: new Vector2(emotePosition.X + (float)dataOffset.X + ((float)(Sprite.SourceRect.Width * 4) / 2f - 32f), emotePosition.Y + (float)dataOffset.Y + (float)emoteYOffset - (float)(32 + Sprite.SpriteHeight * 4)), texture: Game1.emoteSpriteSheet, sourceRectangle: new Microsoft.Xna.Framework.Rectangle(base.CurrentEmoteIndex * 16 % Game1.emoteSpriteSheet.Width, base.CurrentEmoteIndex * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16), color: Color.White, rotation: 0f, origin: Vector2.Zero, scale: 4f, effects: SpriteEffects.None, layerDepth: (float)standingY / 10000f);
		}
	}

	public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
	{
		if (textAboveHeadTimer > 0 && textAboveHead != null)
		{
			Point standingPixel = base.StandingPixel;
			Vector2 local = Game1.GlobalToLocal(new Vector2(standingPixel.X, standingPixel.Y - Sprite.SpriteHeight * 4 - 64 + yJumpOffset));
			if (textAboveHeadStyle == 0)
			{
				local += new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2));
			}
			if (NeedsBirdieEmoteHack())
			{
				local.X += -GetBoundingBox().Width / 4 + 64;
			}
			if (shouldShadowBeOffset)
			{
				local += drawOffset;
			}
			Point tile = base.TilePoint;
			SpriteText.drawStringWithScrollCenteredAt(b, textAboveHead, (int)local.X, (int)local.Y, "", textAboveHeadAlpha, textAboveHeadColor, 1, (float)(tile.Y * 64) / 10000f + 0.001f + (float)tile.X / 10000f);
		}
	}

	public bool NeedsBirdieEmoteHack()
	{
		if (Game1.eventUp && Sprite.SpriteWidth == 32 && base.Name == "Birdie")
		{
			return true;
		}
		return false;
	}

	public void warpToPathControllerDestination()
	{
		if (controller != null)
		{
			while (controller.pathToEndPoint.Count > 2)
			{
				controller.pathToEndPoint.Pop();
				controller.handleWarps(new Microsoft.Xna.Framework.Rectangle(controller.pathToEndPoint.Peek().X * 64, controller.pathToEndPoint.Peek().Y * 64, 64, 64));
				base.Position = new Vector2(controller.pathToEndPoint.Peek().X * 64, controller.pathToEndPoint.Peek().Y * 64 + 16);
				Halt();
			}
		}
	}

	/// <summary>Get the pixel area in the <see cref="P:StardewValley.Character.Sprite" /> texture to show as the NPC's icon in contexts like the calendar and social menu.</summary>
	public virtual Microsoft.Xna.Framework.Rectangle getMugShotSourceRect()
	{
		return GetData()?.MugShotSourceRect ?? new Microsoft.Xna.Framework.Rectangle(0, (Age == 2) ? 4 : 0, 16, 24);
	}

	public void getHitByPlayer(Farmer who, GameLocation location)
	{
		doEmote(12);
		if (who == null)
		{
			if (Game1.IsMultiplayer)
			{
				return;
			}
			who = Game1.player;
		}
		if (who.friendshipData.ContainsKey(base.Name))
		{
			who.changeFriendship(-30, this);
			if (who.IsLocalPlayer)
			{
				CurrentDialogue.Clear();
				CurrentDialogue.Push(TryGetDialogue("HitBySlingshot") ?? (Game1.random.NextBool() ? new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.4293", isGendered: true) : new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.4294")));
			}
			if (Sprite.Texture != null)
			{
				location.debris.Add(new Debris(Sprite.textureName, Game1.random.Next(3, 8), Utility.PointToVector2(base.StandingPixel)));
			}
		}
		if (base.Name.Equals("Bouncer"))
		{
			location.localSound("crafting");
		}
		else
		{
			location.localSound("hitEnemy");
		}
	}

	public void walkInSquare(int squareWidth, int squareHeight, int squarePauseOffset)
	{
		isWalkingInSquare = true;
		lengthOfWalkingSquareX = squareWidth;
		lengthOfWalkingSquareY = squareHeight;
		this.squarePauseOffset = squarePauseOffset;
	}

	public void moveTowardPlayer(int threshold)
	{
		isWalkingTowardPlayer.Value = true;
		moveTowardPlayerThreshold.Value = threshold;
	}

	protected virtual Farmer findPlayer()
	{
		return Game1.MasterPlayer;
	}

	public virtual bool withinPlayerThreshold()
	{
		return withinPlayerThreshold(moveTowardPlayerThreshold);
	}

	public virtual bool withinPlayerThreshold(int threshold)
	{
		if (base.currentLocation != null && !base.currentLocation.farmers.Any())
		{
			return false;
		}
		Vector2 tileLocationOfPlayer = findPlayer().Tile;
		Vector2 tileLocationOfMonster = base.Tile;
		if (Math.Abs(tileLocationOfMonster.X - tileLocationOfPlayer.X) <= (float)threshold && Math.Abs(tileLocationOfMonster.Y - tileLocationOfPlayer.Y) <= (float)threshold)
		{
			return true;
		}
		return false;
	}

	private Stack<Point> addToStackForSchedule(Stack<Point> original, Stack<Point> toAdd)
	{
		if (toAdd == null)
		{
			return original;
		}
		original = new Stack<Point>(original);
		while (original.Count > 0)
		{
			toAdd.Push(original.Pop());
		}
		return toAdd;
	}

	public virtual SchedulePathDescription pathfindToNextScheduleLocation(string scheduleKey, string startingLocation, int startingX, int startingY, string endingLocation, int endingX, int endingY, int finalFacingDirection, string endBehavior, string endMessage)
	{
		Stack<Point> path = new Stack<Point>();
		Point locationStartPoint = new Point(startingX, startingY);
		if (locationStartPoint == Point.Zero)
		{
			throw new Exception($"NPC {base.Name} has an invalid schedule with key '{scheduleKey}': start position in {startingLocation} is at tile (0, 0), which isn't valid.");
		}
		string[] locationsRoute = ((!startingLocation.Equals(endingLocation, StringComparison.Ordinal)) ? getLocationRoute(startingLocation, endingLocation) : null);
		if (locationsRoute != null)
		{
			for (int i = 0; i < locationsRoute.Length; i++)
			{
				string targetLocationName = locationsRoute[i];
				foreach (string activePassiveFestival in Game1.netWorldState.Value.ActivePassiveFestivals)
				{
					if (Utility.TryGetPassiveFestivalData(activePassiveFestival, out var data) && data.MapReplacements != null && data.MapReplacements.TryGetValue(targetLocationName, out var newName))
					{
						targetLocationName = newName;
						break;
					}
				}
				GameLocation currentLocation = Game1.RequireLocation(targetLocationName);
				if (currentLocation.Name.Equals("Trailer") && Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
				{
					currentLocation = Game1.RequireLocation("Trailer_Big");
				}
				if (i < locationsRoute.Length - 1)
				{
					Point target = currentLocation.getWarpPointTo(locationsRoute[i + 1]);
					if (target == Point.Zero)
					{
						throw new Exception($"NPC {base.Name} has an invalid schedule with key '{scheduleKey}': it requires a warp from {currentLocation.NameOrUniqueName} to {locationsRoute[i + 1]}, but none was found.");
					}
					path = addToStackForSchedule(path, PathFindController.findPathForNPCSchedules(locationStartPoint, target, currentLocation, 30000));
					locationStartPoint = currentLocation.getWarpPointTarget(target, this);
				}
				else
				{
					path = addToStackForSchedule(path, PathFindController.findPathForNPCSchedules(locationStartPoint, new Point(endingX, endingY), currentLocation, 30000));
				}
			}
		}
		else if (startingLocation.Equals(endingLocation, StringComparison.Ordinal))
		{
			string targetLocationName = startingLocation;
			foreach (string activePassiveFestival2 in Game1.netWorldState.Value.ActivePassiveFestivals)
			{
				if (Utility.TryGetPassiveFestivalData(activePassiveFestival2, out var data) && data.MapReplacements != null && data.MapReplacements.TryGetValue(targetLocationName, out var newName))
				{
					targetLocationName = newName;
					break;
				}
			}
			GameLocation location = Game1.RequireLocation(targetLocationName);
			if (location.Name.Equals("Trailer") && Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
			{
				location = Game1.RequireLocation("Trailer_Big");
			}
			path = PathFindController.findPathForNPCSchedules(locationStartPoint, new Point(endingX, endingY), location, 30000);
		}
		return new SchedulePathDescription(path, finalFacingDirection, endBehavior, endMessage, endingLocation, new Point(endingX, endingY));
	}

	private string[] getLocationRoute(string startingLocation, string endingLocation)
	{
		return WarpPathfindingCache.GetLocationRoute(startingLocation, endingLocation, Gender);
	}

	/// <summary>
	/// returns true if location is inaccessable and should use "Default" instead.
	///
	///
	/// </summary>
	/// <param name="locationName"></param>
	/// <param name="tileX"></param>
	/// <param name="tileY"></param>
	/// <param name="facingDirection"></param>
	/// <returns></returns>
	private bool changeScheduleForLocationAccessibility(ref string locationName, ref int tileX, ref int tileY, ref int facingDirection)
	{
		switch (locationName)
		{
		case "JojaMart":
		case "Railroad":
			if (!Game1.isLocationAccessible(locationName))
			{
				if (!hasMasterScheduleEntry(locationName + "_Replacement"))
				{
					return true;
				}
				string[] split = ArgUtility.SplitBySpace(getMasterScheduleEntry(locationName + "_Replacement"));
				locationName = split[0];
				tileX = Convert.ToInt32(split[1]);
				tileY = Convert.ToInt32(split[2]);
				facingDirection = Convert.ToInt32(split[3]);
			}
			break;
		case "CommunityCenter":
			return !Game1.isLocationAccessible(locationName);
		}
		return false;
	}

	/// <inheritdoc cref="M:StardewValley.NPC.parseMasterScheduleImpl(System.String,System.String,System.Collections.Generic.List{System.String})" />
	public virtual Dictionary<int, SchedulePathDescription> parseMasterSchedule(string scheduleKey, string rawData)
	{
		return parseMasterScheduleImpl(scheduleKey, rawData, new List<string>());
	}

	/// <summary>Parse a schedule script into its component commands, handling redirection like <c>GOTO</c> automatically.</summary>
	/// <param name="scheduleKey">The schedule key being parsed.</param>
	/// <param name="rawData">The raw schedule script to parse.</param>
	/// <param name="visited">The schedule keys which led to this parse (if any).</param>
	/// <remarks>This is a low-level method. Most code should call <see cref="M:StardewValley.NPC.TryLoadSchedule(System.String)" /> instead.</remarks>
	protected virtual Dictionary<int, SchedulePathDescription> parseMasterScheduleImpl(string scheduleKey, string rawData, List<string> visited)
	{
		if (visited.Contains<string>(scheduleKey, StringComparer.OrdinalIgnoreCase))
		{
			Game1.log.Warn($"NPC {base.Name} can't load schedules because they led to an infinite loop ({string.Join(" -> ", visited)} -> {scheduleKey}).");
			return new Dictionary<int, SchedulePathDescription>();
		}
		visited.Add(scheduleKey);
		try
		{
			string[] split = SplitScheduleCommands(rawData);
			Dictionary<int, SchedulePathDescription> oneDaySchedule = new Dictionary<int, SchedulePathDescription>();
			int routesToSkip = 0;
			if (split[0].Contains("GOTO"))
			{
				string newKey = ArgUtility.SplitBySpaceAndGet(split[0], 1);
				Dictionary<string, string> allSchedules = getMasterScheduleRawData();
				if (string.Equals(newKey, "season", StringComparison.OrdinalIgnoreCase))
				{
					newKey = Game1.currentSeason;
					if (!allSchedules.ContainsKey(newKey))
					{
						newKey = "spring";
					}
				}
				try
				{
					if (allSchedules.TryGetValue(newKey, out var newScript))
					{
						return parseMasterScheduleImpl(newKey, newScript, visited);
					}
					Game1.log.Error($"Failed to load schedule '{scheduleKey}' for NPC '{base.Name}': GOTO references schedule '{newKey}' which doesn't exist. Falling back to 'spring'.");
				}
				catch (Exception e)
				{
					Game1.log.Error($"Failed to load schedule '{scheduleKey}' for NPC '{base.Name}': GOTO references schedule '{newKey}' which couldn't be parsed. Falling back to 'spring'.", e);
				}
				return parseMasterScheduleImpl("spring", getMasterScheduleEntry("spring"), visited);
			}
			if (split[0].Contains("NOT"))
			{
				string[] commandSplit = ArgUtility.SplitBySpace(split[0]);
				if (commandSplit[1].ToLower() == "friendship")
				{
					int index = 2;
					bool conditionMet = false;
					for (; index < commandSplit.Length; index += 2)
					{
						string who = commandSplit[index];
						if (int.TryParse(commandSplit[index + 1], out var level))
						{
							foreach (Farmer allFarmer in Game1.getAllFarmers())
							{
								if (allFarmer.getFriendshipHeartLevelForNPC(who) >= level)
								{
									conditionMet = true;
									break;
								}
							}
						}
						if (conditionMet)
						{
							break;
						}
					}
					if (conditionMet)
					{
						return parseMasterScheduleImpl("spring", getMasterScheduleEntry("spring"), visited);
					}
					routesToSkip++;
				}
			}
			else if (split[0].Contains("MAIL"))
			{
				string mailID = ArgUtility.SplitBySpace(split[0])[1];
				routesToSkip = ((!Game1.MasterPlayer.mailReceived.Contains(mailID) && !NetWorldState.checkAnywhereForWorldStateID(mailID)) ? (routesToSkip + 1) : (routesToSkip + 2));
			}
			if (split[routesToSkip].Contains("GOTO"))
			{
				string newKey = ArgUtility.SplitBySpaceAndGet(split[routesToSkip], 1);
				string text = newKey.ToLower();
				if (!(text == "season"))
				{
					if (text == "no_schedule")
					{
						followSchedule = false;
						return null;
					}
				}
				else
				{
					newKey = Game1.currentSeason;
				}
				return parseMasterScheduleImpl(newKey, getMasterScheduleEntry(newKey), visited);
			}
			Point previousPosition = (isMarried() ? new Point(10, 23) : new Point((int)defaultPosition.X / 64, (int)defaultPosition.Y / 64));
			string previousGameLocation = (isMarried() ? "BusStop" : ((string)defaultMap));
			int previousTime = 610;
			string default_map = DefaultMap;
			int default_x = (int)(defaultPosition.X / 64f);
			int default_y = (int)(defaultPosition.Y / 64f);
			bool default_map_dirty = false;
			for (int i = routesToSkip; i < split.Length; i++)
			{
				int index = 0;
				string[] newDestinationDescription = ArgUtility.SplitBySpace(split[i]);
				bool time_is_arrival_time = false;
				string time_string = newDestinationDescription[index];
				if (time_string.Length > 0 && newDestinationDescription[index][0] == 'a')
				{
					time_is_arrival_time = true;
					time_string = time_string.Substring(1);
				}
				int time = Convert.ToInt32(time_string);
				index++;
				string location = newDestinationDescription[index];
				string endOfRouteAnimation = null;
				string endOfRouteMessage = null;
				int xLocation = 0;
				int yLocation = 0;
				int localFacingDirection = 2;
				if (location == "bed")
				{
					if (isMarried())
					{
						location = "BusStop";
						xLocation = 9;
						yLocation = 23;
						localFacingDirection = 3;
					}
					else
					{
						string default_schedule = null;
						if (hasMasterScheduleEntry("default"))
						{
							default_schedule = getMasterScheduleEntry("default");
						}
						else if (hasMasterScheduleEntry("spring"))
						{
							default_schedule = getMasterScheduleEntry("spring");
						}
						if (default_schedule != null)
						{
							try
							{
								string[] last_schedule_split = ArgUtility.SplitBySpace(SplitScheduleCommands(default_schedule)[^1]);
								location = last_schedule_split[1];
								if (last_schedule_split.Length > 3)
								{
									if (!int.TryParse(last_schedule_split[2], out xLocation) || !int.TryParse(last_schedule_split[3], out yLocation))
									{
										default_schedule = null;
									}
								}
								else
								{
									default_schedule = null;
								}
							}
							catch (Exception)
							{
								default_schedule = null;
							}
						}
						if (default_schedule == null)
						{
							location = default_map;
							xLocation = default_x;
							yLocation = default_y;
						}
					}
					index++;
					Dictionary<string, string> dictionary = DataLoader.AnimationDescriptions(Game1.content);
					string sleep_behavior = name.Value.ToLower() + "_sleep";
					if (dictionary.ContainsKey(sleep_behavior))
					{
						endOfRouteAnimation = sleep_behavior;
					}
				}
				else
				{
					if (int.TryParse(location, out var _))
					{
						location = previousGameLocation;
						index--;
					}
					index++;
					xLocation = Convert.ToInt32(newDestinationDescription[index]);
					index++;
					yLocation = Convert.ToInt32(newDestinationDescription[index]);
					index++;
					try
					{
						if (newDestinationDescription.Length > index)
						{
							if (int.TryParse(newDestinationDescription[index], out localFacingDirection))
							{
								index++;
							}
							else
							{
								localFacingDirection = 2;
							}
						}
					}
					catch (Exception)
					{
						localFacingDirection = 2;
					}
				}
				if (changeScheduleForLocationAccessibility(ref location, ref xLocation, ref yLocation, ref localFacingDirection))
				{
					string newKey = (getMasterScheduleRawData().ContainsKey("default") ? "default" : "spring");
					return parseMasterScheduleImpl(newKey, getMasterScheduleEntry(newKey), visited);
				}
				if (index < newDestinationDescription.Length)
				{
					if (newDestinationDescription[index].Length > 0 && newDestinationDescription[index][0] == '"')
					{
						endOfRouteMessage = split[i].Substring(split[i].IndexOf('"'));
					}
					else
					{
						endOfRouteAnimation = newDestinationDescription[index];
						index++;
						if (index < newDestinationDescription.Length && newDestinationDescription[index].Length > 0 && newDestinationDescription[index][0] == '"')
						{
							endOfRouteMessage = split[i].Substring(split[i].IndexOf('"')).Replace("\"", "");
						}
					}
				}
				if (time == 0)
				{
					default_map_dirty = true;
					default_map = location;
					default_x = xLocation;
					default_y = yLocation;
					previousGameLocation = location;
					previousPosition.X = xLocation;
					previousPosition.Y = yLocation;
					faceDirection(localFacingDirection);
					previousEndPoint = new Point(xLocation, yLocation);
					continue;
				}
				SchedulePathDescription path_description = pathfindToNextScheduleLocation(scheduleKey, previousGameLocation, previousPosition.X, previousPosition.Y, location, xLocation, yLocation, localFacingDirection, endOfRouteAnimation, endOfRouteMessage);
				if (time_is_arrival_time)
				{
					int distance_traveled = 0;
					Point? last_point = null;
					foreach (Point point in path_description.route)
					{
						if (!last_point.HasValue)
						{
							last_point = point;
							continue;
						}
						if (Math.Abs(last_point.Value.X - point.X) + Math.Abs(last_point.Value.Y - point.Y) == 1)
						{
							distance_traveled += 64;
						}
						last_point = point;
					}
					int num = distance_traveled / 2;
					int ticks_per_ten_minutes = Game1.realMilliSecondsPerGameTenMinutes / 1000 * 60;
					int travel_time = (int)Math.Round((float)num / (float)ticks_per_ten_minutes) * 10;
					time = Math.Max(Utility.ConvertMinutesToTime(Utility.ConvertTimeToMinutes(time) - travel_time), previousTime);
				}
				path_description.time = time;
				oneDaySchedule.Add(time, path_description);
				previousPosition.X = xLocation;
				previousPosition.Y = yLocation;
				previousGameLocation = location;
				previousTime = time;
			}
			if (Game1.IsMasterGame && default_map_dirty)
			{
				Game1.warpCharacter(this, default_map, new Point(default_x, default_y));
			}
			return oneDaySchedule;
		}
		catch (Exception ex)
		{
			Game1.log.Error($"NPC '{base.Name}' failed to parse master schedule '{scheduleKey}' with raw data '{rawData}'.", ex);
			return new Dictionary<int, SchedulePathDescription>();
		}
	}

	/// <summary>Split a raw schedule script into its component commands.</summary>
	/// <param name="rawScript">The raw schedule script to split.</param>
	public static string[] SplitScheduleCommands(string rawScript)
	{
		return LegacyShims.SplitAndTrim(rawScript, '/', StringSplitOptions.RemoveEmptyEntries);
	}

	/// <summary>Try to load a schedule that applies today, or disable the schedule if none is found.</summary>
	/// <returns>Returns whether a schedule was successfully loaded.</returns>
	public bool TryLoadSchedule()
	{
		string season = Game1.currentSeason;
		int day = Game1.dayOfMonth;
		string dayName = Game1.shortDayNameFromDayOfSeason(day);
		int heartLevel = Math.Max(0, Utility.GetAllPlayerFriendshipLevel(this)) / 250;
		if (getMasterScheduleRawData() == null)
		{
			ClearSchedule();
			return false;
		}
		if (Game1.IsGreenRainingHere() && Game1.year == 1 && TryLoadSchedule("GreenRain"))
		{
			return true;
		}
		if (!string.IsNullOrWhiteSpace(islandScheduleName.Value))
		{
			TryLoadSchedule(islandScheduleName.Value, Schedule);
			return true;
		}
		foreach (string festivalId in Game1.netWorldState.Value.ActivePassiveFestivals)
		{
			int dayOfPassiveFestival = Utility.GetDayOfPassiveFestival(festivalId);
			if (isMarried())
			{
				if (TryLoadSchedule("marriage_" + festivalId + "_" + dayOfPassiveFestival))
				{
					return true;
				}
				if (TryLoadSchedule("marriage_" + festivalId))
				{
					return true;
				}
			}
			else
			{
				if (TryLoadSchedule(festivalId + "_" + dayOfPassiveFestival))
				{
					return true;
				}
				if (TryLoadSchedule(festivalId))
				{
					return true;
				}
			}
		}
		if (isMarried())
		{
			if (TryLoadSchedule("marriage_" + season + "_" + day))
			{
				return true;
			}
			if (base.Name == "Penny")
			{
				switch (dayName)
				{
				case "Tue":
				case "Wed":
				case "Fri":
					goto IL_0206;
				}
			}
			if ((base.Name == "Maru" && (dayName == "Tue" || dayName == "Thu")) || (base.Name == "Harvey" && (dayName == "Tue" || dayName == "Thu")))
			{
				goto IL_0206;
			}
			goto IL_0215;
		}
		if (TryLoadSchedule(season + "_" + day))
		{
			return true;
		}
		int tryHearts;
		for (tryHearts = heartLevel; tryHearts > 0; tryHearts--)
		{
			if (TryLoadSchedule(day + "_" + tryHearts))
			{
				return true;
			}
		}
		if (TryLoadSchedule(day.ToString()))
		{
			return true;
		}
		if (base.Name == "Pam" && Game1.player.mailReceived.Contains("ccVault") && TryLoadSchedule("bus"))
		{
			return true;
		}
		if (base.currentLocation?.IsRainingHere() ?? false)
		{
			if (Game1.random.NextBool() && TryLoadSchedule("rain2"))
			{
				return true;
			}
			if (TryLoadSchedule("rain"))
			{
				return true;
			}
		}
		for (tryHearts = heartLevel; tryHearts > 0; tryHearts--)
		{
			if (TryLoadSchedule(season + "_" + dayName + "_" + tryHearts))
			{
				return true;
			}
			tryHearts--;
		}
		if (TryLoadSchedule(season + "_" + dayName))
		{
			return true;
		}
		for (tryHearts = heartLevel; tryHearts > 0; tryHearts--)
		{
			if (TryLoadSchedule(dayName + "_" + tryHearts))
			{
				return true;
			}
			tryHearts--;
		}
		if (TryLoadSchedule(dayName))
		{
			return true;
		}
		if (TryLoadSchedule(season))
		{
			return true;
		}
		if (TryLoadSchedule("spring_" + dayName))
		{
			return true;
		}
		if (TryLoadSchedule("spring"))
		{
			return true;
		}
		ClearSchedule();
		return false;
		IL_0206:
		if (TryLoadSchedule("marriageJob"))
		{
			return true;
		}
		goto IL_0215;
		IL_0215:
		if (!Game1.isRaining && TryLoadSchedule("marriage_" + dayName))
		{
			return true;
		}
		ClearSchedule();
		return false;
	}

	/// <summary>Try to load a schedule matching the the given key, or disable the schedule if it's missing or invalid.</summary>
	/// <param name="key">The key for the schedule to load.</param>
	/// <returns>Returns whether the schedule was successfully loaded.</returns>
	public bool TryLoadSchedule(string key)
	{
		try
		{
			if (hasMasterScheduleEntry(key))
			{
				TryLoadSchedule(key, parseMasterSchedule(key, getMasterScheduleEntry(key)));
				return true;
			}
		}
		catch (Exception ex)
		{
			Game1.log.Error($"Failed to load schedule key '{key}' for NPC '{base.Name}'.", ex);
		}
		ClearSchedule();
		return false;
	}

	/// <summary>Try to load a raw schedule script, or disable the schedule if it's invalid.</summary>
	/// <param name="key">The schedule's key in the data asset.</param>
	/// <param name="rawSchedule">The schedule script to load.</param>
	public bool TryLoadSchedule(string key, string rawSchedule)
	{
		Dictionary<int, SchedulePathDescription> schedule;
		try
		{
			schedule = parseMasterSchedule(key, rawSchedule);
		}
		catch (Exception ex)
		{
			Game1.log.Error($"Failed to load schedule key '{key}' from raw string for NPC '{base.Name}'.", ex);
			ClearSchedule();
			return false;
		}
		return TryLoadSchedule(key, schedule);
	}

	/// <summary>Try to load raw schedule data, or disable the schedule if it's invalid.</summary>
	/// <param name="key">The schedule's key in the data asset.</param>
	/// <param name="schedule">The schedule data to load.</param>
	public bool TryLoadSchedule(string key, Dictionary<int, SchedulePathDescription> schedule)
	{
		if (schedule == null)
		{
			ClearSchedule();
			return false;
		}
		Schedule = schedule;
		if (Game1.IsMasterGame)
		{
			dayScheduleName.Value = key;
		}
		followSchedule = true;
		return true;
	}

	/// <summary>Disable the schedule for today.</summary>
	public void ClearSchedule()
	{
		Schedule = null;
		if (Game1.IsMasterGame)
		{
			dayScheduleName.Value = null;
		}
		followSchedule = false;
	}

	public virtual void handleMasterScheduleFileLoadError(Exception e)
	{
		Game1.log.Error("NPC '" + base.Name + "' failed loading schedule file.", e);
	}

	public virtual void InvalidateMasterSchedule()
	{
		_hasLoadedMasterScheduleData = false;
	}

	public Dictionary<string, string> getMasterScheduleRawData()
	{
		if (!_hasLoadedMasterScheduleData)
		{
			_hasLoadedMasterScheduleData = true;
			string assetName = "Characters\\schedules\\" + base.Name;
			if (base.Name == "Leo" && DefaultMap != "IslandHut")
			{
				assetName += "Mainland";
			}
			try
			{
				if (Game1.content.DoesAssetExist<Dictionary<string, string>>(assetName))
				{
					_masterScheduleData = Game1.content.Load<Dictionary<string, string>>("Characters\\schedules\\" + base.Name);
					_masterScheduleData = new Dictionary<string, string>(_masterScheduleData, StringComparer.OrdinalIgnoreCase);
				}
			}
			catch (Exception e)
			{
				handleMasterScheduleFileLoadError(e);
			}
		}
		return _masterScheduleData;
	}

	public string getMasterScheduleEntry(string schedule_key)
	{
		if (getMasterScheduleRawData() == null)
		{
			throw new KeyNotFoundException("The schedule file for NPC '" + base.Name + "' could not be loaded...");
		}
		if (_masterScheduleData.TryGetValue(schedule_key, out var data))
		{
			return data;
		}
		throw new KeyNotFoundException($"The schedule file for NPC '{base.Name}' has no schedule named '{schedule_key}'.");
	}

	public bool hasMasterScheduleEntry(string key)
	{
		if (getMasterScheduleRawData() == null)
		{
			return false;
		}
		return getMasterScheduleRawData().ContainsKey(key);
	}

	public virtual bool isRoommate()
	{
		if (!IsVillager)
		{
			return false;
		}
		foreach (Farmer f in Game1.getAllFarmers())
		{
			if (f.spouse != null && f.spouse == base.Name && !f.isEngaged() && f.isRoommate(base.Name))
			{
				return true;
			}
		}
		return false;
	}

	public bool isMarried()
	{
		if (!IsVillager)
		{
			return false;
		}
		foreach (Farmer f in Game1.getAllFarmers())
		{
			if (f.spouse != null && f.spouse == base.Name && !f.isEngaged())
			{
				return true;
			}
		}
		return false;
	}

	public bool isMarriedOrEngaged()
	{
		if (!IsVillager)
		{
			return false;
		}
		foreach (Farmer f in Game1.getAllFarmers())
		{
			if (f.spouse != null && f.spouse == base.Name)
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Update the NPC state when setting up the new day, before the game saves overnight.</summary>
	/// <param name="dayOfMonth">The current day of month.</param>
	/// <remarks>See also <see cref="M:StardewValley.NPC.OnDayStarted" />, which happens after saving when the day has started.</remarks>
	public virtual void dayUpdate(int dayOfMonth)
	{
		bool villager = IsVillager;
		isMovingOnPathFindPath.Value = false;
		queuedSchedulePaths.Clear();
		lastAttemptedSchedule = -1;
		drawOffset = Vector2.Zero;
		appliedRouteAnimationOffset = Vector2.Zero;
		shouldWearIslandAttire.Value = false;
		if (layingDown)
		{
			layingDown = false;
			HideShadow = false;
		}
		if (isWearingIslandAttire)
		{
			wearNormalClothes();
		}
		if (base.currentLocation != null && defaultMap.Value != null)
		{
			try
			{
				Game1.warpCharacter(this, defaultMap, defaultPosition.Value / 64f);
			}
			catch (Exception ex)
			{
				Game1.log.Error($"NPC '{base.Name}' failed to warp home to '{defaultMap}' overnight.", ex);
			}
		}
		if (villager)
		{
			string text = base.Name;
			if (!(text == "Willy"))
			{
				if (text == "Elliott" && Game1.IsMasterGame && Game1.netWorldState.Value.hasWorldStateID("elliottGone"))
				{
					daysUntilNotInvisible = 7;
					Game1.netWorldState.Value.removeWorldStateID("elliottGone");
					Game1.worldStateIDs.Remove("elliottGone");
				}
			}
			else
			{
				IsInvisible = false;
			}
		}
		UpdateInvisibilityOnNewDay();
		resetForNewDay(dayOfMonth);
		ChooseAppearance();
		if (villager)
		{
			updateConstructionAnimation();
		}
		clearTextAboveHead();
	}

	/// <summary>Handle the new day starting after the player saves, loads, or connects.</summary>
	/// <remarks>See also <see cref="M:StardewValley.NPC.dayUpdate(System.Int32)" />, which happens while setting up the day before saving.</remarks>
	public void OnDayStarted()
	{
		if (Game1.IsMasterGame && isMarried() && !getSpouse().divorceTonight && !IsInvisible)
		{
			marriageDuties();
		}
	}

	protected void UpdateInvisibilityOnNewDay()
	{
		if (Game1.IsMasterGame && (IsInvisible || daysUntilNotInvisible > 0))
		{
			daysUntilNotInvisible--;
			IsInvisible = daysUntilNotInvisible > 0;
			if (!IsInvisible)
			{
				daysUntilNotInvisible = 0;
			}
		}
	}

	public virtual void resetForNewDay(int dayOfMonth)
	{
		sleptInBed.Value = true;
		if (isMarried() && !isRoommate())
		{
			FarmHouse house = Utility.getHomeOfFarmer(getSpouse());
			if (house != null && house.GetSpouseBed() == null)
			{
				sleptInBed.Value = false;
			}
		}
		if (doingEndOfRouteAnimation.Value)
		{
			routeEndAnimationFinished(null);
		}
		Halt();
		wasKissedYesterday = hasBeenKissedToday.Value;
		hasBeenKissedToday.Value = false;
		currentMarriageDialogue.Clear();
		marriageDefaultDialogue.Value = null;
		shouldSayMarriageDialogue.Value = false;
		isSleeping.Value = false;
		drawOffset = Vector2.Zero;
		faceTowardFarmer = false;
		faceTowardFarmerTimer = 0;
		drawOffset = Vector2.Zero;
		hasSaidAfternoonDialogue.Value = false;
		isPlayingSleepingAnimation = false;
		ignoreScheduleToday = false;
		Halt();
		controller = null;
		temporaryController = null;
		directionsToNewLocation = null;
		faceDirection(DefaultFacingDirection);
		Sprite.oldFrame = Sprite.CurrentFrame;
		previousEndPoint = new Point((int)defaultPosition.X / 64, (int)defaultPosition.Y / 64);
		isWalkingInSquare = false;
		returningToEndPoint = false;
		lastCrossroad = Microsoft.Xna.Framework.Rectangle.Empty;
		_startedEndOfRouteBehavior = null;
		_finishingEndOfRouteBehavior = null;
		loadedEndOfRouteBehavior = null;
		_beforeEndOfRouteAnimationFrame = Sprite.CurrentFrame;
		if (IsVillager)
		{
			if (base.Name == "Willy" && Game1.stats.DaysPlayed < 2)
			{
				IsInvisible = true;
				daysUntilNotInvisible = 1;
			}
			TryLoadSchedule();
			performSpecialScheduleChanges();
		}
		endOfRouteMessage.Value = null;
	}

	public void returnHomeFromFarmPosition(Farm farm)
	{
		Farmer farmer = getSpouse();
		if (farmer != null)
		{
			FarmHouse farm_house = Utility.getHomeOfFarmer(farmer);
			Point porchPoint = farm_house.getPorchStandingSpot();
			if (base.TilePoint == porchPoint)
			{
				drawOffset = Vector2.Zero;
				string nameOfHome = getHome().NameOrUniqueName;
				base.willDestroyObjectsUnderfoot = true;
				Point destination = farm.getWarpPointTo(nameOfHome, this);
				controller = new PathFindController(this, farm, destination, 0)
				{
					NPCSchedule = true
				};
			}
			else if (!shouldPlaySpousePatioAnimation.Value || !farm.farmers.Any())
			{
				drawOffset = Vector2.Zero;
				Halt();
				controller = null;
				temporaryController = null;
				ignoreScheduleToday = true;
				Game1.warpCharacter(this, farm_house, Utility.PointToVector2(farm_house.getKitchenStandingSpot()));
			}
		}
	}

	public virtual Vector2 GetSpousePatioPosition()
	{
		return Utility.PointToVector2(Game1.getFarm().spousePatioSpot);
	}

	public void setUpForOutdoorPatioActivity()
	{
		Vector2 patio_location = GetSpousePatioPosition();
		if (!checkTileOccupancyForSpouse(Game1.getFarm(), patio_location))
		{
			Game1.warpCharacter(this, "Farm", patio_location);
			popOffAnyNonEssentialItems();
			currentMarriageDialogue.Clear();
			addMarriageDialogue("MarriageDialogue", "patio_" + base.Name, false);
			setTilePosition((int)patio_location.X, (int)patio_location.Y);
			shouldPlaySpousePatioAnimation.Value = true;
		}
	}

	private void doPlaySpousePatioAnimation()
	{
		CharacterSpousePatioData patioData = GetData()?.SpousePatio;
		if (patioData == null)
		{
			return;
		}
		List<int[]> frames = patioData.SpriteAnimationFrames;
		if (frames == null || frames.Count <= 0)
		{
			return;
		}
		drawOffset = Utility.PointToVector2(patioData.SpriteAnimationPixelOffset);
		Sprite.ClearAnimation();
		for (int i = 0; i < frames.Count; i++)
		{
			int[] frame = frames[i];
			if (frame != null && frame.Length != 0)
			{
				int index = frame[0];
				int duration = (ArgUtility.HasIndex(frame, 1) ? frame[1] : 100);
				Sprite.AddFrame(new FarmerSprite.AnimationFrame(index, duration, 0, secondaryArm: false, flip: false));
			}
		}
	}

	/// <summary>Whether this character has dark skin for the purposes of child genetics.</summary>
	public virtual bool hasDarkSkin()
	{
		if (IsVillager)
		{
			return GetData()?.IsDarkSkinned ?? false;
		}
		return false;
	}

	/// <summary>Whether the player will need to adopt children with this spouse, instead of either the player or NPC giving birth.</summary>
	public bool isAdoptionSpouse()
	{
		Farmer spouse = getSpouse();
		if (spouse == null)
		{
			return false;
		}
		string isAdoptionSpouse = GetData()?.SpouseAdopts;
		if (isAdoptionSpouse != null)
		{
			return GameStateQuery.CheckConditions(isAdoptionSpouse, base.currentLocation, spouse);
		}
		return Gender == spouse.Gender;
	}

	public bool canGetPregnant()
	{
		if (this is Horse || base.Name.Equals("Krobus") || isRoommate() || IsInvisible)
		{
			return false;
		}
		Farmer spouse = getSpouse();
		if (spouse == null || (bool)spouse.divorceTonight)
		{
			return false;
		}
		int heartsWithSpouse = spouse.getFriendshipHeartLevelForNPC(base.Name);
		Friendship friendship = spouse.GetSpouseFriendship();
		List<Child> kids = spouse.getChildren();
		defaultMap.Value = spouse.homeLocation.Value;
		FarmHouse farmHouse = Utility.getHomeOfFarmer(spouse);
		if (farmHouse.cribStyle.Value <= 0)
		{
			return false;
		}
		if (farmHouse.upgradeLevel >= 2 && friendship.DaysUntilBirthing < 0 && heartsWithSpouse >= 10 && spouse.GetDaysMarried() >= 7)
		{
			if (kids.Count != 0)
			{
				if (kids.Count < 2)
				{
					return kids[0].Age > 2;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public void marriageDuties()
	{
		Farmer spouse = getSpouse();
		if (spouse == null)
		{
			return;
		}
		shouldSayMarriageDialogue.Value = true;
		DefaultMap = spouse.homeLocation.Value;
		FarmHouse farmHouse = Game1.RequireLocation<FarmHouse>(spouse.homeLocation.Value);
		Random r = Utility.CreateDaySaveRandom(spouse.UniqueMultiplayerID);
		int heartsWithSpouse = spouse.getFriendshipHeartLevelForNPC(base.Name);
		if (Game1.IsMasterGame && (base.currentLocation == null || !base.currentLocation.Equals(farmHouse)))
		{
			Game1.warpCharacter(this, spouse.homeLocation.Value, farmHouse.getSpouseBedSpot(base.Name));
		}
		if (Game1.isRaining)
		{
			marriageDefaultDialogue.Value = new MarriageDialogueReference("MarriageDialogue", "Rainy_Day_" + r.Next(5), false);
		}
		else
		{
			marriageDefaultDialogue.Value = new MarriageDialogueReference("MarriageDialogue", "Indoor_Day_" + r.Next(5), false);
		}
		currentMarriageDialogue.Add(new MarriageDialogueReference(marriageDefaultDialogue.Value.DialogueFile, marriageDefaultDialogue.Value.DialogueKey, marriageDefaultDialogue.Value.IsGendered, marriageDefaultDialogue.Value.Substitutions));
		if (spouse.GetSpouseFriendship().DaysUntilBirthing == 0)
		{
			setTilePosition(farmHouse.getKitchenStandingSpot());
			currentMarriageDialogue.Clear();
			return;
		}
		if (daysAfterLastBirth >= 0)
		{
			daysAfterLastBirth--;
			switch (getSpouse().getChildrenCount())
			{
			case 1:
				setTilePosition(farmHouse.getKitchenStandingSpot());
				if (!spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4406", false), farmHouse))
				{
					currentMarriageDialogue.Clear();
					addMarriageDialogue("MarriageDialogue", "OneKid_" + r.Next(4), false);
				}
				return;
			case 2:
				setTilePosition(farmHouse.getKitchenStandingSpot());
				if (!spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4406", false), farmHouse))
				{
					currentMarriageDialogue.Clear();
					addMarriageDialogue("MarriageDialogue", "TwoKids_" + r.Next(4), false);
				}
				return;
			}
		}
		setTilePosition(farmHouse.getKitchenStandingSpot());
		if (!sleptInBed.Value)
		{
			currentMarriageDialogue.Clear();
			addMarriageDialogue("MarriageDialogue", "NoBed_" + r.Next(4), false);
			return;
		}
		if (tryToGetMarriageSpecificDialogue(Game1.currentSeason + "_" + Game1.dayOfMonth) != null)
		{
			if (spouse != null)
			{
				currentMarriageDialogue.Clear();
				addMarriageDialogue("MarriageDialogue", Game1.currentSeason + "_" + Game1.dayOfMonth, false);
			}
			return;
		}
		if (Schedule != null)
		{
			if (ScheduleKey == "marriage_" + Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth))
			{
				currentMarriageDialogue.Clear();
				addMarriageDialogue("MarriageDialogue", "funLeave_" + base.Name, false);
			}
			else if (ScheduleKey == "marriageJob")
			{
				currentMarriageDialogue.Clear();
				addMarriageDialogue("MarriageDialogue", "jobLeave_" + base.Name, false);
			}
			return;
		}
		if (!Game1.isRaining && !Game1.IsWinter && Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth).Equals("Sat") && spouse == Game1.MasterPlayer && !base.Name.Equals("Krobus"))
		{
			setUpForOutdoorPatioActivity();
			return;
		}
		int minHeartLevelForNegativeDialogue = 12;
		if (Game1.Date.TotalDays - spouse.GetSpouseFriendship().LastGiftDate?.TotalDays <= 1)
		{
			minHeartLevelForNegativeDialogue--;
		}
		if (wasKissedYesterday)
		{
			minHeartLevelForNegativeDialogue--;
		}
		if (spouse.GetDaysMarried() > 7 && r.NextDouble() < (double)(1f - (float)Math.Max(1, heartsWithSpouse) / (float)minHeartLevelForNegativeDialogue))
		{
			Furniture f = farmHouse.getRandomFurniture(r);
			if (f != null && f.isGroundFurniture() && f.furniture_type.Value != 15 && f.furniture_type.Value != 12)
			{
				Point p = new Point((int)f.tileLocation.X - 1, (int)f.tileLocation.Y);
				if (farmHouse.CanItemBePlacedHere(new Vector2(p.X, p.Y)))
				{
					setTilePosition(p);
					faceDirection(1);
					switch (r.Next(10))
					{
					case 0:
						currentMarriageDialogue.Clear();
						addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4420", false);
						break;
					case 1:
						currentMarriageDialogue.Clear();
						addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4421", false);
						break;
					case 2:
						currentMarriageDialogue.Clear();
						addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4422", true);
						break;
					case 3:
						currentMarriageDialogue.Clear();
						addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4423", false);
						break;
					case 4:
						currentMarriageDialogue.Clear();
						addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4424", false);
						break;
					case 5:
						currentMarriageDialogue.Clear();
						addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4425", false);
						break;
					case 6:
						currentMarriageDialogue.Clear();
						addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4426", false);
						break;
					case 7:
						if (Gender == Gender.Female)
						{
							currentMarriageDialogue.Clear();
							addMarriageDialogue("Strings\\StringsFromCSFiles", r.Choose("NPC.cs.4427", "NPC.cs.4429"), false);
						}
						else
						{
							currentMarriageDialogue.Clear();
							addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4431", false);
						}
						break;
					case 8:
						currentMarriageDialogue.Clear();
						addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4432", false);
						break;
					case 9:
						currentMarriageDialogue.Clear();
						addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4433", false);
						break;
					}
					return;
				}
			}
			spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4406", false), farmHouse, force: true);
			return;
		}
		Friendship friendship = spouse.GetSpouseFriendship();
		if (friendship.DaysUntilBirthing != -1 && friendship.DaysUntilBirthing <= 7 && r.NextBool())
		{
			if (isAdoptionSpouse())
			{
				setTilePosition(farmHouse.getKitchenStandingSpot());
				if (!spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4439", false), farmHouse))
				{
					if (r.NextBool())
					{
						currentMarriageDialogue.Clear();
					}
					if (r.NextBool())
					{
						addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4440", false, getSpouse().displayName);
					}
					else
					{
						addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4441", false, "%endearment");
					}
				}
				return;
			}
			if (Gender == Gender.Female)
			{
				setTilePosition(farmHouse.getKitchenStandingSpot());
				if (!spouseObstacleCheck(r.NextBool() ? new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4442", false) : new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4443", false), farmHouse))
				{
					if (r.NextBool())
					{
						currentMarriageDialogue.Clear();
					}
					currentMarriageDialogue.Add(r.NextBool() ? new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4444", false, getSpouse().displayName) : new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4445", false, "%endearment"));
				}
				return;
			}
			setTilePosition(farmHouse.getKitchenStandingSpot());
			if (!spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4446", true), farmHouse))
			{
				if (r.NextBool())
				{
					currentMarriageDialogue.Clear();
				}
				currentMarriageDialogue.Add(r.NextBool() ? new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4447", true, getSpouse().displayName) : new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4448", false, "%endearment"));
			}
			return;
		}
		if (r.NextDouble() < 0.07)
		{
			switch (getSpouse().getChildrenCount())
			{
			case 1:
				setTilePosition(farmHouse.getKitchenStandingSpot());
				if (!spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4449", true), farmHouse))
				{
					currentMarriageDialogue.Clear();
					addMarriageDialogue("MarriageDialogue", "OneKid_" + r.Next(4), false);
				}
				return;
			case 2:
				setTilePosition(farmHouse.getKitchenStandingSpot());
				if (!spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4452", true), farmHouse))
				{
					currentMarriageDialogue.Clear();
					addMarriageDialogue("MarriageDialogue", "TwoKids_" + r.Next(4), false);
				}
				return;
			}
		}
		Farm farm = Game1.getFarm();
		if (currentMarriageDialogue.Count > 0 && currentMarriageDialogue[0].IsItemGrabDialogue(this))
		{
			setTilePosition(farmHouse.getKitchenStandingSpot());
			spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4455", true), farmHouse);
		}
		else if (!Game1.isRaining && r.NextDouble() < 0.4 && !checkTileOccupancyForSpouse(farm, Utility.PointToVector2(farmHouse.getPorchStandingSpot())) && !base.Name.Equals("Krobus"))
		{
			bool filledBowl = false;
			if (!hasSomeoneFedThePet)
			{
				foreach (Building building in farm.buildings)
				{
					if (building is PetBowl bowl && !bowl.watered.Value)
					{
						filledBowl = true;
						bowl.watered.Value = true;
						hasSomeoneFedThePet = true;
					}
				}
			}
			if (r.NextDouble() < 0.6 && Game1.season != Season.Winter && !hasSomeoneWateredCrops)
			{
				Vector2 origin = Vector2.Zero;
				int tries = 0;
				bool foundWatered = false;
				for (; tries < Math.Min(50, farm.terrainFeatures.Length); tries++)
				{
					if (!origin.Equals(Vector2.Zero))
					{
						break;
					}
					if (Utility.TryGetRandom(farm.terrainFeatures, out var tile, out var feature) && feature is HoeDirt dirt && dirt.needsWatering())
					{
						if (!dirt.isWatered())
						{
							origin = tile;
						}
						else
						{
							foundWatered = true;
						}
					}
				}
				if (!origin.Equals(Vector2.Zero))
				{
					foreach (Vector2 currentPosition in new Microsoft.Xna.Framework.Rectangle((int)origin.X - 30, (int)origin.Y - 30, 60, 60).GetVectors())
					{
						if (farm.isTileOnMap(currentPosition) && farm.terrainFeatures.TryGetValue(currentPosition, out var terrainFeature) && terrainFeature is HoeDirt dirt && Game1.IsMasterGame && dirt.needsWatering())
						{
							dirt.state.Value = 1;
						}
					}
					faceDirection(2);
					currentMarriageDialogue.Clear();
					addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4462", true);
					if (filledBowl)
					{
						if (Utility.getAllPets().Count > 1 && Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.en)
						{
							addMarriageDialogue("Strings\\StringsFromCSFiles", "MultiplePetBowls_watered", false, Game1.player.getPetDisplayName());
						}
						else
						{
							addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4463", false, Game1.player.getPetDisplayName());
						}
					}
					addMarriageDialogue("MarriageDialogue", "Outdoor_" + r.Next(5), false);
					hasSomeoneWateredCrops = true;
				}
				else
				{
					faceDirection(2);
					if (foundWatered)
					{
						currentMarriageDialogue.Clear();
						if (Game1.gameMode == 6)
						{
							if (r.NextBool())
							{
								addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4465", false, "%endearment");
							}
							else
							{
								addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4466", false, "%endearment");
								addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4462", true);
								if (filledBowl)
								{
									if (Utility.getAllPets().Count > 1 && Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.en)
									{
										addMarriageDialogue("Strings\\StringsFromCSFiles", "MultiplePetBowls_watered", false, Game1.player.getPetDisplayName());
									}
									else
									{
										addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4463", false, Game1.player.getPetDisplayName());
									}
								}
							}
						}
						else
						{
							currentMarriageDialogue.Clear();
							addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4470", true);
						}
					}
					else
					{
						currentMarriageDialogue.Clear();
						addMarriageDialogue("MarriageDialogue", "Outdoor_" + r.Next(5), false);
					}
				}
			}
			else if (r.NextDouble() < 0.6 && !hasSomeoneFedTheAnimals)
			{
				bool fedAnything = false;
				foreach (Building b in farm.buildings)
				{
					if (b.GetIndoors() is AnimalHouse animalHouse && (int)b.daysOfConstructionLeft <= 0 && Game1.IsMasterGame)
					{
						animalHouse.feedAllAnimals();
						fedAnything = true;
					}
				}
				faceDirection(2);
				if (fedAnything)
				{
					hasSomeoneFedTheAnimals = true;
					currentMarriageDialogue.Clear();
					addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4474", true);
					if (filledBowl)
					{
						if (Utility.getAllPets().Count > 1 && Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.en)
						{
							addMarriageDialogue("Strings\\StringsFromCSFiles", "MultiplePetBowls_watered", false, Game1.player.getPetDisplayName());
						}
						else
						{
							addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4463", false, Game1.player.getPetDisplayName());
						}
					}
					addMarriageDialogue("MarriageDialogue", "Outdoor_" + r.Next(5), false);
				}
				else
				{
					currentMarriageDialogue.Clear();
					addMarriageDialogue("MarriageDialogue", "Outdoor_" + r.Next(5), false);
				}
				if (Game1.IsMasterGame)
				{
					foreach (Building building2 in farm.buildings)
					{
						if (building2 is PetBowl bowl && !bowl.watered.Value)
						{
							filledBowl = true;
							bowl.watered.Value = true;
							hasSomeoneFedThePet = true;
						}
					}
				}
			}
			else if (!hasSomeoneRepairedTheFences)
			{
				int tries = 0;
				faceDirection(2);
				Vector2 origin = Vector2.Zero;
				for (; tries < Math.Min(50, farm.objects.Length); tries++)
				{
					if (!origin.Equals(Vector2.Zero))
					{
						break;
					}
					if (Utility.TryGetRandom(farm.objects, out var tile, out var obj) && obj is Fence)
					{
						origin = tile;
					}
				}
				if (!origin.Equals(Vector2.Zero))
				{
					foreach (Vector2 currentPosition in new Microsoft.Xna.Framework.Rectangle((int)origin.X - 10, (int)origin.Y - 10, 20, 20).GetVectors())
					{
						if (farm.isTileOnMap(currentPosition) && farm.objects.TryGetValue(currentPosition, out var obj) && obj is Fence fence && Game1.IsMasterGame)
						{
							fence.repair();
						}
					}
					currentMarriageDialogue.Clear();
					addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4481", true);
					if (filledBowl)
					{
						if (Utility.getAllPets().Count > 1 && Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.en)
						{
							addMarriageDialogue("Strings\\StringsFromCSFiles", "MultiplePetBowls_watered", false, Game1.player.getPetDisplayName());
						}
						else
						{
							addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4463", false, Game1.player.getPetDisplayName());
						}
					}
					addMarriageDialogue("MarriageDialogue", "Outdoor_" + r.Next(5), false);
					hasSomeoneRepairedTheFences = true;
				}
				else
				{
					currentMarriageDialogue.Clear();
					addMarriageDialogue("MarriageDialogue", "Outdoor_" + r.Next(5), false);
				}
			}
			Game1.warpCharacter(this, "Farm", farmHouse.getPorchStandingSpot());
			popOffAnyNonEssentialItems();
			faceDirection(2);
		}
		else if (base.Name.Equals("Krobus") && Game1.isRaining && r.NextDouble() < 0.4 && !checkTileOccupancyForSpouse(farm, Utility.PointToVector2(farmHouse.getPorchStandingSpot())))
		{
			addMarriageDialogue("MarriageDialogue", "Outdoor_" + r.Next(5), false);
			Game1.warpCharacter(this, "Farm", farmHouse.getPorchStandingSpot());
			popOffAnyNonEssentialItems();
			faceDirection(2);
		}
		else if (spouse.GetDaysMarried() >= 1 && r.NextDouble() < 0.045)
		{
			if (r.NextDouble() < 0.75)
			{
				Point spot = farmHouse.getRandomOpenPointInHouse(r, 1);
				Furniture new_furniture;
				try
				{
					new_furniture = ItemRegistry.Create<Furniture>(Utility.getRandomSingleTileFurniture(r)).SetPlacement(spot);
				}
				catch
				{
					new_furniture = null;
				}
				if (new_furniture != null && spot.X > 0 && farmHouse.CanItemBePlacedHere(new Vector2(spot.X - 1, spot.Y)))
				{
					farmHouse.furniture.Add(new_furniture);
					setTilePosition(spot.X - 1, spot.Y);
					faceDirection(1);
					addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4486", false, "%endearmentlower");
					if (Game1.random.NextBool())
					{
						addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4488", true);
					}
					else
					{
						addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4489", false);
					}
				}
				else
				{
					setTilePosition(farmHouse.getKitchenStandingSpot());
					spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4490", false), farmHouse);
				}
				return;
			}
			Point p = farmHouse.getRandomOpenPointInHouse(r);
			if (p.X <= 0)
			{
				return;
			}
			setTilePosition(p.X, p.Y);
			faceDirection(0);
			if (r.NextBool())
			{
				string wall = farmHouse.GetWallpaperID(p.X, p.Y);
				if (wall != null)
				{
					string wallpaperId = r.ChooseFrom(GetData()?.SpouseWallpapers) ?? r.Next(112).ToString();
					farmHouse.SetWallpaper(wallpaperId, wall);
					addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4496", false);
				}
			}
			else
			{
				string floor = farmHouse.getFloorRoomIdAt(p);
				if (floor != null)
				{
					string floorId = r.ChooseFrom(GetData()?.SpouseFloors) ?? r.Next(40).ToString();
					farmHouse.SetFloor(floorId, floor);
					addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4497", false);
				}
			}
		}
		else if (Game1.isRaining && r.NextDouble() < 0.08 && heartsWithSpouse < 11 && spouse.GetDaysMarried() > 7 && base.Name != "Krobus")
		{
			foreach (Furniture f in farmHouse.furniture)
			{
				if ((int)f.furniture_type == 13 && farmHouse.CanItemBePlacedHere(new Vector2((int)f.tileLocation.X, (int)f.tileLocation.Y + 1)))
				{
					setTilePosition((int)f.tileLocation.X, (int)f.tileLocation.Y + 1);
					faceDirection(0);
					currentMarriageDialogue.Clear();
					addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4498", true);
					return;
				}
			}
			spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4499", false), farmHouse, force: true);
		}
		else if (r.NextDouble() < 0.45)
		{
			Vector2 spot = Utility.PointToVector2(farmHouse.GetSpouseRoomSpot());
			setTilePosition((int)spot.X, (int)spot.Y);
			faceDirection(0);
			setSpouseRoomMarriageDialogue();
			if (name == "Sebastian" && Game1.netWorldState.Value.hasWorldStateID("sebastianFrog"))
			{
				Point frog_spot = farmHouse.GetSpouseRoomCorner();
				frog_spot.X += 2;
				frog_spot.Y += 5;
				setTilePosition(frog_spot);
				faceDirection(2);
			}
		}
		else
		{
			setTilePosition(farmHouse.getKitchenStandingSpot());
			faceDirection(0);
			if (r.NextDouble() < 0.2)
			{
				setRandomAfternoonMarriageDialogue(Game1.timeOfDay, farmHouse);
			}
		}
	}

	public virtual void popOffAnyNonEssentialItems()
	{
		if (!Game1.IsMasterGame || base.currentLocation == null)
		{
			return;
		}
		Point tile = base.TilePoint;
		Object tile_object = base.currentLocation.getObjectAtTile(tile.X, tile.Y);
		if (tile_object != null)
		{
			bool pop_off = false;
			if (tile_object.QualifiedItemId == "(O)93" || tile_object is Torch)
			{
				pop_off = true;
			}
			if (pop_off)
			{
				Vector2 tile_position = tile_object.TileLocation;
				tile_object.performRemoveAction();
				base.currentLocation.objects.Remove(tile_position);
				tile_object.dropItem(base.currentLocation, tile_position * 64f, tile_position * 64f);
			}
		}
	}

	public static bool checkTileOccupancyForSpouse(GameLocation location, Vector2 point, string characterToIgnore = "")
	{
		return location?.IsTileOccupiedBy(point, ~(CollisionMask.Characters | CollisionMask.Farmers), CollisionMask.All) ?? true;
	}

	public void addMarriageDialogue(string dialogue_file, string dialogue_key, bool gendered = false, params string[] substitutions)
	{
		shouldSayMarriageDialogue.Value = true;
		currentMarriageDialogue.Add(new MarriageDialogueReference(dialogue_file, dialogue_key, gendered, substitutions));
	}

	public void clearTextAboveHead()
	{
		textAboveHead = null;
		textAboveHeadPreTimer = -1;
		textAboveHeadTimer = -1;
	}

	/// <summary>Get whether this is a villager NPC, regardless of whether they're present in <c>Data/Characters</c>.</summary>
	[Obsolete("Use IsVillager instead.")]
	public bool isVillager()
	{
		return IsVillager;
	}

	public override bool shouldCollideWithBuildingLayer(GameLocation location)
	{
		if (isMarried() && (Schedule == null || location is FarmHouse))
		{
			return true;
		}
		return base.shouldCollideWithBuildingLayer(location);
	}

	public virtual void arriveAtFarmHouse(FarmHouse farmHouse)
	{
		if (Game1.newDay || !isMarried() || Game1.timeOfDay <= 630 || !(base.TilePoint != farmHouse.getSpouseBedSpot(name)))
		{
			return;
		}
		setTilePosition(farmHouse.getEntryLocation());
		ignoreScheduleToday = true;
		temporaryController = null;
		controller = null;
		if (Game1.timeOfDay >= 2130)
		{
			Point bed_spot = farmHouse.getSpouseBedSpot(name);
			bool found_bed = farmHouse.GetSpouseBed() != null;
			PathFindController.endBehavior end_behavior = null;
			if (found_bed)
			{
				end_behavior = FarmHouse.spouseSleepEndFunction;
			}
			controller = new PathFindController(this, farmHouse, bed_spot, 0, end_behavior);
			if (controller.pathToEndPoint != null && found_bed)
			{
				foreach (Furniture furniture in farmHouse.furniture)
				{
					if (furniture is BedFurniture bed && furniture.GetBoundingBox().Intersects(new Microsoft.Xna.Framework.Rectangle(bed_spot.X * 64, bed_spot.Y * 64, 64, 64)))
					{
						bed.ReserveForNPC();
						break;
					}
				}
			}
		}
		else
		{
			controller = new PathFindController(this, farmHouse, farmHouse.getKitchenStandingSpot(), 0);
		}
		if (controller.pathToEndPoint == null)
		{
			base.willDestroyObjectsUnderfoot = true;
			controller = new PathFindController(this, farmHouse, farmHouse.getKitchenStandingSpot(), 0);
			setNewDialogue(TryGetDialogue("SpouseFarmhouseClutter") ?? new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.4500", isGendered: true));
		}
		else if (Game1.timeOfDay > 1300)
		{
			if (ScheduleKey == "marriage_" + Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth))
			{
				setNewDialogue("MarriageDialogue", "funReturn_", clearOnMovement: true);
			}
			else if (ScheduleKey == "marriageJob")
			{
				setNewDialogue("MarriageDialogue", "jobReturn_");
			}
			else if (Game1.timeOfDay < 1800)
			{
				setRandomAfternoonMarriageDialogue(Game1.timeOfDay, base.currentLocation, countAsDailyAfternoon: true);
			}
		}
		if (Game1.currentLocation == farmHouse)
		{
			Game1.currentLocation.playSound("doorClose", null, null, SoundContext.NPC);
		}
	}

	public Farmer getSpouse()
	{
		foreach (Farmer f in Game1.getAllFarmers())
		{
			if (f.spouse != null && f.spouse == base.Name)
			{
				return f;
			}
		}
		return null;
	}

	public string getTermOfSpousalEndearment(bool happy = true)
	{
		Farmer spouse = getSpouse();
		if (spouse != null)
		{
			if (isRoommate())
			{
				return spouse.displayName;
			}
			if (spouse.getFriendshipHeartLevelForNPC(base.Name) < 9)
			{
				return spouse.displayName;
			}
			if (!happy)
			{
				return Game1.random.Next(2) switch
				{
					0 => Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4517"), 
					1 => Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4518"), 
					_ => spouse.displayName, 
				};
			}
			if (Game1.random.NextDouble() < 0.08)
			{
				switch (Game1.random.Next(8))
				{
				case 0:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4507");
				case 1:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4508");
				case 2:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4509");
				case 3:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4510");
				case 4:
					if (!spouse.IsMale)
					{
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4512");
					}
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4511");
				case 5:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4513");
				case 6:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4514");
				default:
					if (!spouse.IsMale)
					{
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4516");
					}
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4515");
				}
			}
			return Game1.random.Next(5) switch
			{
				0 => Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4519"), 
				1 => Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4518"), 
				2 => Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4517"), 
				3 => Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4522"), 
				_ => Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4523"), 
			};
		}
		return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4517");
	}

	/// <summary>
	/// return true if spouse encountered obstacle.
	/// if force == true then the obstacle check will be ignored and spouse will absolutely be put into bed.
	/// </summary>
	/// <param name="backToBedMessage"></param>
	/// <param name="currentLocation"></param>
	/// <returns></returns>
	public bool spouseObstacleCheck(MarriageDialogueReference backToBedMessage, GameLocation currentLocation, bool force = false)
	{
		if (force || checkTileOccupancyForSpouse(currentLocation, base.Tile, base.Name))
		{
			Game1.warpCharacter(this, defaultMap, Game1.RequireLocation<FarmHouse>(defaultMap).getSpouseBedSpot(name));
			faceDirection(1);
			currentMarriageDialogue.Clear();
			currentMarriageDialogue.Add(backToBedMessage);
			shouldSayMarriageDialogue.Value = true;
			return true;
		}
		return false;
	}

	public void setTilePosition(Point p)
	{
		setTilePosition(p.X, p.Y);
	}

	public void setTilePosition(int x, int y)
	{
		base.Position = new Vector2(x * 64, y * 64);
	}

	private void clintHammerSound(Farmer who)
	{
		base.currentLocation.playSound("hammer", base.Tile);
	}

	private void robinHammerSound(Farmer who)
	{
		if (Game1.currentLocation.Equals(base.currentLocation) && Utility.isOnScreen(base.Position, 256))
		{
			Game1.playSound((Game1.random.NextDouble() < 0.1) ? "clank" : "axchop");
			shakeTimer = 250;
		}
	}

	private void robinVariablePause(Farmer who)
	{
		if (Game1.random.NextDouble() < 0.4)
		{
			Sprite.CurrentAnimation[Sprite.currentAnimationIndex] = new FarmerSprite.AnimationFrame(27, 300, secondaryArm: false, flip: false, robinVariablePause);
		}
		else if (Game1.random.NextDouble() < 0.25)
		{
			Sprite.CurrentAnimation[Sprite.currentAnimationIndex] = new FarmerSprite.AnimationFrame(23, Game1.random.Next(500, 4000), secondaryArm: false, flip: false, robinVariablePause);
		}
		else
		{
			Sprite.CurrentAnimation[Sprite.currentAnimationIndex] = new FarmerSprite.AnimationFrame(27, Game1.random.Next(1000, 4000), secondaryArm: false, flip: false, robinVariablePause);
		}
	}

	public void randomSquareMovement(GameTime time)
	{
		Microsoft.Xna.Framework.Rectangle boundingBox = GetBoundingBox();
		boundingBox.Inflate(2, 2);
		Microsoft.Xna.Framework.Rectangle endRect = new Microsoft.Xna.Framework.Rectangle((int)nextSquarePosition.X * 64, (int)nextSquarePosition.Y * 64, 64, 64);
		_ = nextSquarePosition;
		if (nextSquarePosition.Equals(Vector2.Zero))
		{
			squarePauseAccumulation = 0;
			squarePauseTotal = Game1.random.Next(6000 + squarePauseOffset, 12000 + squarePauseOffset);
			nextSquarePosition = new Vector2(lastCrossroad.X / 64 - lengthOfWalkingSquareX / 2 + Game1.random.Next(lengthOfWalkingSquareX), lastCrossroad.Y / 64 - lengthOfWalkingSquareY / 2 + Game1.random.Next(lengthOfWalkingSquareY));
		}
		else if (endRect.Contains(boundingBox))
		{
			Halt();
			if (squareMovementFacingPreference != -1)
			{
				faceDirection(squareMovementFacingPreference);
			}
			isCharging = false;
			base.speed = 2;
		}
		else if (boundingBox.Left <= endRect.Left)
		{
			SetMovingOnlyRight();
		}
		else if (boundingBox.Right >= endRect.Right)
		{
			SetMovingOnlyLeft();
		}
		else if (boundingBox.Top <= endRect.Top)
		{
			SetMovingOnlyDown();
		}
		else if (boundingBox.Bottom >= endRect.Bottom)
		{
			SetMovingOnlyUp();
		}
		squarePauseAccumulation += time.ElapsedGameTime.Milliseconds;
		if (squarePauseAccumulation >= squarePauseTotal && endRect.Contains(boundingBox))
		{
			nextSquarePosition = Vector2.Zero;
			isCharging = false;
			base.speed = 2;
		}
	}

	public void returnToEndPoint()
	{
		Microsoft.Xna.Framework.Rectangle boundingBox = GetBoundingBox();
		boundingBox.Inflate(2, 2);
		if (boundingBox.Left <= lastCrossroad.Left)
		{
			SetMovingOnlyRight();
		}
		else if (boundingBox.Right >= lastCrossroad.Right)
		{
			SetMovingOnlyLeft();
		}
		else if (boundingBox.Top <= lastCrossroad.Top)
		{
			SetMovingOnlyDown();
		}
		else if (boundingBox.Bottom >= lastCrossroad.Bottom)
		{
			SetMovingOnlyUp();
		}
		boundingBox.Inflate(-2, -2);
		if (lastCrossroad.Contains(boundingBox))
		{
			isWalkingInSquare = false;
			nextSquarePosition = Vector2.Zero;
			returningToEndPoint = false;
			Halt();
		}
	}

	public void SetMovingOnlyUp()
	{
		moveUp = true;
		moveDown = false;
		moveLeft = false;
		moveRight = false;
	}

	public void SetMovingOnlyRight()
	{
		moveUp = false;
		moveDown = false;
		moveLeft = false;
		moveRight = true;
	}

	public void SetMovingOnlyDown()
	{
		moveUp = false;
		moveDown = true;
		moveLeft = false;
		moveRight = false;
	}

	public void SetMovingOnlyLeft()
	{
		moveUp = false;
		moveDown = false;
		moveLeft = true;
		moveRight = false;
	}

	public virtual int getTimeFarmerMustPushBeforePassingThrough()
	{
		return 1500;
	}

	public virtual int getTimeFarmerMustPushBeforeStartShaking()
	{
		return 400;
	}

	public int CompareTo(object obj)
	{
		if (obj is NPC npc)
		{
			return npc.id - id;
		}
		return 0;
	}

	public virtual void Removed()
	{
	}
}
