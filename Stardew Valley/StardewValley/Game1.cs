using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using SkiaSharp;
using StardewValley.Audio;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Delegates;
using StardewValley.Enchantments;
using StardewValley.Events;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.BigCraftables;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.Characters;
using StardewValley.GameData.Crops;
using StardewValley.GameData.FarmAnimals;
using StardewValley.GameData.FloorsAndPaths;
using StardewValley.GameData.FruitTrees;
using StardewValley.GameData.LocationContexts;
using StardewValley.GameData.Locations;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Pants;
using StardewValley.GameData.Pets;
using StardewValley.GameData.Shirts;
using StardewValley.GameData.Tools;
using StardewValley.GameData.Weapons;
using StardewValley.Hashing;
using StardewValley.Internal;
using StardewValley.Inventories;
using StardewValley.Locations;
using StardewValley.Logging;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.Mods;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Network.NetReady;
using StardewValley.Objects;
using StardewValley.Pathfinding;
using StardewValley.Projectiles;
using StardewValley.Quests;
using StardewValley.SaveMigrations;
using StardewValley.SDKs.Steam;
using StardewValley.SpecialOrders;
using StardewValley.TerrainFeatures;
using StardewValley.TokenizableStrings;
using StardewValley.Tools;
using StardewValley.Triggers;
using StardewValley.Util;
using xTile.Dimensions;
using xTile.Display;
using xTile.Layers;
using xTile.Tiles;

namespace StardewValley;

/// <summary>
/// This is the main type for your game
/// </summary>
[InstanceStatics]
public class Game1 : InstanceGame
{
	public enum BundleType
	{
		Default,
		Remixed
	}

	public enum MineChestType
	{
		Default,
		Remixed
	}

	public delegate void afterFadeFunction();

	public const int defaultResolutionX = 1280;

	public const int defaultResolutionY = 720;

	public const int pixelZoom = 4;

	public const int tileSize = 64;

	public const int smallestTileSize = 16;

	public const int up = 0;

	public const int right = 1;

	public const int down = 2;

	public const int left = 3;

	public const int dialogueBoxTileHeight = 5;

	public static int realMilliSecondsPerGameMinute;

	public static int realMilliSecondsPerGameTenMinutes;

	public const int rainDensity = 70;

	public const int rainLoopLength = 70;

	/// <summary>For <see cref="F:StardewValley.Game1.mouseCursor" />, a value indicating the cursor should be hidden.</summary>
	public static readonly int cursor_none;

	/// <summary>For <see cref="F:StardewValley.Game1.mouseCursor" />, a default pointer icon.</summary>
	public static readonly int cursor_default;

	/// <summary>For <see cref="F:StardewValley.Game1.mouseCursor" />, a wait icon.</summary>
	public static readonly int cursor_wait;

	/// <summary>For <see cref="F:StardewValley.Game1.mouseCursor" />, a hand icon indicating that an item can be picked up.</summary>
	public static readonly int cursor_grab;

	/// <summary>For <see cref="F:StardewValley.Game1.mouseCursor" />, a gift box icon indicating that an NPC on this tile can accept a gift.</summary>
	public static readonly int cursor_gift;

	/// <summary>For <see cref="F:StardewValley.Game1.mouseCursor" />, a speech bubble icon indicating that an NPC can be talked to.</summary>
	public static readonly int cursor_talk;

	/// <summary>For <see cref="F:StardewValley.Game1.mouseCursor" />, a magnifying glass icon indicating that something can be examined.</summary>
	public static readonly int cursor_look;

	/// <summary>For <see cref="F:StardewValley.Game1.mouseCursor" />, an icon indicating that something can be harvested.</summary>
	public static readonly int cursor_harvest;

	/// <summary>For <see cref="F:StardewValley.Game1.mouseCursor" />, a pointer icon used when hovering elements with gamepad controls.</summary>
	public static readonly int cursor_gamepad_pointer;

	public static readonly string asianSpacingRegexString;

	public const int legacy_weather_sunny = 0;

	public const int legacy_weather_rain = 1;

	public const int legacy_weather_debris = 2;

	public const int legacy_weather_lightning = 3;

	public const int legacy_weather_festival = 4;

	public const int legacy_weather_snow = 5;

	public const int legacy_weather_wedding = 6;

	public const string weather_sunny = "Sun";

	public const string weather_rain = "Rain";

	public const string weather_green_rain = "GreenRain";

	public const string weather_debris = "Wind";

	public const string weather_lightning = "Storm";

	public const string weather_festival = "Festival";

	public const string weather_snow = "Snow";

	public const string weather_wedding = "Wedding";

	/// <summary>The builder name for Robin's carpenter shop.</summary>
	public const string builder_robin = "Robin";

	/// <summary>The builder name for Wizard's magical construction shop.</summary>
	public const string builder_wizard = "Wizard";

	/// <summary>The shop ID for the Adventurer's Guild shop.</summary>
	public const string shop_adventurersGuild = "AdventureShop";

	/// <summary>The shop ID for the Adventurer's Guild item recovery shop.</summary>
	public const string shop_adventurersGuildItemRecovery = "AdventureGuildRecovery";

	/// <summary>The shop ID for Marnie's animal supply shop.</summary>
	public const string shop_animalSupplies = "AnimalShop";

	/// <summary>The shop ID for Clint's blacksmithery.</summary>
	public const string shop_blacksmith = "Blacksmith";

	/// <summary>The shop ID for Clint's tool upgrade shop.</summary>
	public const string shop_blacksmithUpgrades = "ClintUpgrade";

	/// <summary>The shop ID for the movie theater box office.</summary>
	public const string shop_boxOffice = "BoxOffice";

	/// <summary>The 'shop' ID for the floorpaper/wallpaper catalogue.</summary>
	public const string shop_catalogue = "Catalogue";

	/// <summary>The shop ID for Robin's carpenter supplies.</summary>
	public const string shop_carpenter = "Carpenter";

	/// <summary>The shop ID for the casino club shop.</summary>
	public const string shop_casino = "Casino";

	/// <summary>The shop ID for the desert trader.</summary>
	public const string shop_desertTrader = "DesertTrade";

	/// <summary>The shop ID for Dwarf's shop.</summary>
	public const string shop_dwarf = "Dwarf";

	/// <summary>The shop ID for Willy's fish shop.</summary>
	public const string shop_fish = "FishShop";

	/// <summary>The 'shop' ID for the furniture catalogue.</summary>
	public const string shop_furnitureCatalogue = "Furniture Catalogue";

	/// <summary>The shop ID for Pierre's General Store.</summary>
	public const string shop_generalStore = "SeedShop";

	/// <summary>The shop ID for the Hat Mouse shop.</summary>
	public const string shop_hatMouse = "HatMouse";

	/// <summary>The shop ID for Harvey's clinic.</summary>
	public const string shop_hospital = "Hospital";

	/// <summary>The shop ID for the ice-cream stand.</summary>
	public const string shop_iceCreamStand = "IceCreamStand";

	/// <summary>The shop ID for the island trader.</summary>
	public const string shop_islandTrader = "IslandTrade";

	/// <summary>The shop ID for Joja Mart.</summary>
	public const string shop_jojaMart = "Joja";

	/// <summary>The shop ID for Krobus' shop.</summary>
	public const string shop_krobus = "ShadowShop";

	/// <summary>The shop ID for Qi's gem shop.</summary>
	public const string shop_qiGemShop = "QiGemShop";

	/// <summary>The shop ID for the Ginger Island resort bar.</summary>
	public const string shop_resortBar = "ResortBar";

	/// <summary>The shop ID for Sandy's Oasis.</summary>
	public const string shop_sandy = "Sandy";

	/// <summary>The shop ID for the Stardrop Saloon.</summary>
	public const string shop_saloon = "Saloon";

	/// <summary>The shop ID for the traveling cart shop.</summary>
	public const string shop_travelingCart = "Traveler";

	/// <summary>The shop ID for the Volcano Dungeon shop.</summary>
	public const string shop_volcanoShop = "VolcanoShop";

	/// <summary>The shop ID for the bookseller.</summary>
	public const string shop_bookseller = "Bookseller";

	/// <summary>The shop ID for the bookseller trade-ins.</summary>
	public const string shop_bookseller_trade = "BooksellerTrade";

	/// <summary>The 'shop' ID for the joja furniture catalogue.</summary>
	public const string shop_jojaCatalogue = "JojaFurnitureCatalogue";

	/// <summary>The 'shop' ID for the wizard furniture catalogue.</summary>
	public const string shop_wizardCatalogue = "WizardFurnitureCatalogue";

	/// <summary>The 'shop' ID for the wizard furniture catalogue.</summary>
	public const string shop_junimoCatalogue = "JunimoFurnitureCatalogue";

	/// <summary>The 'shop' ID for the wizard furniture catalogue.</summary>
	public const string shop_retroCatalogue = "RetroFurnitureCatalogue";

	/// <summary>The 'shop' ID for the wizard furniture catalogue.</summary>
	public const string shop_trashCatalogue = "TrashFurnitureCatalogue";

	/// <summary>The shop ID for Marnie's pet adoption shop.</summary>
	public const string shop_petAdoption = "PetAdoption";

	public const byte singlePlayer = 0;

	public const byte multiplayerClient = 1;

	public const byte multiplayerServer = 2;

	public const byte logoScreenGameMode = 4;

	public const byte titleScreenGameMode = 0;

	public const byte loadScreenGameMode = 1;

	public const byte newGameMode = 2;

	public const byte playingGameMode = 3;

	public const byte loadingMode = 6;

	public const byte saveMode = 7;

	public const byte saveCompleteMode = 8;

	public const byte selectGameScreen = 9;

	public const byte creditsMode = 10;

	public const byte errorLogMode = 11;

	/// <summary>The name of the game's main assembly.</summary>
	public static readonly string GameAssemblyName;

	/// <summary>The semantic game version, like <c>1.6.0</c>.</summary>
	/// <remarks>
	///   <para>
	///     This mostly follows semantic versioning format with three or four numbers (without leading zeros), so
	///     1.6.7 comes before 1.6.10. The first three numbers are consistent across all platforms, while some
	///     platforms may add a fourth number for the port version. This doesn't include tags like <c>-alpha</c>
	///     or <c>-beta</c>; see <see cref="F:StardewValley.Game1.versionLabel" /> or <see cref="M:StardewValley.Game1.GetVersionString" /> for that.
	///   </para>
	///
	///   <para>Game versions can be compared using <see cref="M:StardewValley.Utility.CompareGameVersions(System.String,System.String,System.Boolean)" />.</para>
	/// </remarks>
	public static readonly string version;

	/// <summary>A human-readable label for the update, like 'modding update' or 'hotfix #3', if any.</summary>
	public static readonly string versionLabel;

	/// <summary>The game build number used to distinguish different builds with the same version number, like <c>26055</c>.</summary>
	/// <remarks>This value is platform-dependent.</remarks>
	public static readonly int versionBuildNumber;

	public const float keyPollingThreshold = 650f;

	public const float toolHoldPerPowerupLevel = 600f;

	public const float startingMusicVolume = 1f;

	/// <summary>
	/// ContentManager specifically for loading xTile.Map(s).
	/// Will be unloaded when returning to title.
	/// </summary>
	public LocalizedContentManager xTileContent;

	public static DelayedAction morningSongPlayAction;

	private static LocalizedContentManager _temporaryContent;

	[NonInstancedStatic]
	public static GraphicsDeviceManager graphics;

	[NonInstancedStatic]
	public static LocalizedContentManager content;

	public static SpriteBatch spriteBatch;

	public static float MusicDuckTimer;

	public static GamePadState oldPadState;

	public static float thumbStickSensitivity;

	public static float runThreshold;

	public static int rightStickHoldTime;

	public static int emoteMenuShowTime;

	public static int nextFarmerWarpOffsetX;

	public static int nextFarmerWarpOffsetY;

	public static KeyboardState oldKBState;

	public static MouseState oldMouseState;

	[NonInstancedStatic]
	public static Game1 keyboardFocusInstance;

	private static Farmer _player;

	public static NetFarmerRoot serverHost;

	protected static bool _isWarping;

	[NonInstancedStatic]
	public static bool hasLocalClientsOnly;

	protected bool _instanceIsPlayingBackgroundMusic;

	protected bool _instanceIsPlayingOutdoorsAmbience;

	protected bool _instanceIsPlayingNightAmbience;

	protected bool _instanceIsPlayingTownMusic;

	protected bool _instanceIsPlayingMorningSong;

	public static bool isUsingBackToFrontSorting;

	protected static StringBuilder _debugStringBuilder;

	public static Dictionary<string, GameLocation> _locationLookup;

	public IList<GameLocation> _locations = new List<GameLocation>();

	public static Regex asianSpacingRegex;

	public static Viewport defaultDeviceViewport;

	public static LocationRequest locationRequest;

	public static bool warpingForForcedRemoteEvent;

	protected static GameLocation _PreviousNonNullLocation;

	public GameLocation instanceGameLocation;

	public static IDisplayDevice mapDisplayDevice;

	[NonInstancedStatic]
	public static Microsoft.Xna.Framework.Rectangle safeAreaBounds;

	public static xTile.Dimensions.Rectangle viewport;

	public static xTile.Dimensions.Rectangle uiViewport;

	public static Texture2D objectSpriteSheet;

	public static Texture2D cropSpriteSheet;

	public static Texture2D emoteSpriteSheet;

	public static Texture2D debrisSpriteSheet;

	public static Texture2D rainTexture;

	public static Texture2D bigCraftableSpriteSheet;

	public static Texture2D buffsIcons;

	public static Texture2D daybg;

	public static Texture2D nightbg;

	public static Texture2D menuTexture;

	public static Texture2D uncoloredMenuTexture;

	public static Texture2D lantern;

	public static Texture2D windowLight;

	public static Texture2D sconceLight;

	public static Texture2D cauldronLight;

	public static Texture2D shadowTexture;

	public static Texture2D mouseCursors;

	public static Texture2D mouseCursors2;

	public static Texture2D mouseCursors_1_6;

	public static Texture2D giftboxTexture;

	public static Texture2D controllerMaps;

	public static Texture2D indoorWindowLight;

	public static Texture2D animations;

	public static Texture2D concessionsSpriteSheet;

	public static Texture2D birdsSpriteSheet;

	public static Texture2D objectSpriteSheet_2;

	public static Texture2D bobbersTexture;

	public static Dictionary<string, Stack<Dialogue>> npcDialogues;

	protected readonly List<Farmer> _farmerShadows = new List<Farmer>();

	/// <summary>Actions that are called after waking up in the morning. These aren't saved, so they're only use for "fluff".</summary>
	public static Queue<Action> morningQueue;

	[NonInstancedStatic]
	protected internal static ModHooks hooks;

	public static InputState input;

	protected internal static IInputSimulator inputSimulator;

	public const string concessionsSpriteSheetName = "LooseSprites\\Concessions";

	public const string cropSpriteSheetName = "TileSheets\\crops";

	public const string objectSpriteSheetName = "Maps\\springobjects";

	public const string animationsName = "TileSheets\\animations";

	public const string mouseCursorsName = "LooseSprites\\Cursors";

	public const string mouseCursors2Name = "LooseSprites\\Cursors2";

	public const string mouseCursors1_6Name = "LooseSprites\\Cursors_1_6";

	public const string giftboxName = "LooseSprites\\Giftbox";

	public const string toolSpriteSheetName = "TileSheets\\tools";

	public const string bigCraftableSpriteSheetName = "TileSheets\\Craftables";

	public const string debrisSpriteSheetName = "TileSheets\\debris";

	public const string parrotSheetName = "LooseSprites\\parrots";

	public const string hatsSheetName = "Characters\\Farmer\\hats";

	public const string bobbersTextureName = "TileSheets\\bobbers";

	private static Texture2D _toolSpriteSheet;

	public static Dictionary<Vector2, int> crabPotOverlayTiles;

	protected static bool _setSaveName;

	protected static string _currentSaveName;

	public static string savePathOverride;

	public static List<string> mailDeliveredFromMailForTomorrow;

	private static RenderTarget2D _lightmap;

	public static Texture2D fadeToBlackRect;

	public static Texture2D staminaRect;

	public static SpriteFont dialogueFont;

	public static SpriteFont smallFont;

	public static SpriteFont tinyFont;

	public static float screenGlowAlpha;

	public static float flashAlpha;

	public static float noteBlockTimer;

	public static int currentGemBirdIndex;

	public Dictionary<string, object> newGameSetupOptions = new Dictionary<string, object>();

	public static bool dialogueUp;

	public static bool dialogueTyping;

	public static bool isQuestion;

	public static bool newDay;

	public static bool eventUp;

	public static bool viewportFreeze;

	public static bool eventOver;

	public static bool screenGlow;

	public static bool screenGlowHold;

	public static bool screenGlowUp;

	public static bool killScreen;

	public static bool messagePause;

	public static bool weddingToday;

	public static bool exitToTitle;

	public static bool debugMode;

	public static bool displayHUD;

	public static bool displayFarmer;

	public static bool dialogueButtonShrinking;

	public static bool drawLighting;

	public static bool quit;

	public static bool drawGrid;

	public static bool freezeControls;

	public static bool saveOnNewDay;

	public static bool panMode;

	public static bool showingEndOfNightStuff;

	public static bool wasRainingYesterday;

	public static bool hasLoadedGame;

	public static bool isActionAtCurrentCursorTile;

	public static bool isInspectionAtCurrentCursorTile;

	public static bool isSpeechAtCurrentCursorTile;

	public static bool paused;

	public static bool isTimePaused;

	public static bool frameByFrame;

	public static bool lastCursorMotionWasMouse;

	public static bool showingHealth;

	public static bool cabinsSeparate;

	public static bool showingHealthBar;

	/// <summary>Whether <see cref="M:StardewValley.Game1.OnDayStarted" /> has been called at least once since this save was loaded or joined.</summary>
	public static bool hasStartedDay;

	/// <summary>The event IDs which the current player has seen since entering the location.</summary>
	public static HashSet<string> eventsSeenSinceLastLocationChange;

	internal static bool hasApplied1_3_UpdateChanges;

	internal static bool hasApplied1_4_UpdateChanges;

	private static Action postExitToTitleCallback;

	protected int _lastUsedDisplay = -1;

	public bool wasAskedLeoMemory;

	public float controllerSlingshotSafeTime;

	public static BundleType bundleType;

	public static bool isRaining;

	public static bool isSnowing;

	public static bool isLightning;

	public static bool isDebrisWeather;

	/// <summary>Internal state that tracks whether today's weather state is a green rain day.</summary>
	private static bool _isGreenRain;

	/// <summary>Whether today's weather state was green rain at any point.</summary>
	internal static bool wasGreenRain;

	/// <summary>Whether the locations affected by green rain still need cleanup. This should only be set by <see cref="M:StardewValley.Game1._newDayAfterFade" />.</summary>
	internal static bool greenRainNeedsCleanup;

	/// <summary>The season for which the debris weather fields like <see cref="F:StardewValley.Game1.debrisWeather" /> were last generated.</summary>
	public static Season? debrisWeatherSeason;

	public static string weatherForTomorrow;

	public float zoomModifier = 1f;

	private static ScreenFade screenFade;

	/// <summary>The current season of the year.</summary>
	public static Season season;

	public static SerializableDictionary<string, string> bannedUsers;

	private static object _debugOutputLock;

	private static string _debugOutput;

	public static string requestedMusicTrack;

	public static string messageAfterPause;

	public static string samBandName;

	public static string loadingMessage;

	public static string errorMessage;

	protected Dictionary<MusicContext, KeyValuePair<string, bool>> _instanceRequestedMusicTracks = new Dictionary<MusicContext, KeyValuePair<string, bool>>();

	protected MusicContext _instanceActiveMusicContext;

	public static bool requestedMusicTrackOverrideable;

	public static bool currentTrackOverrideable;

	public static bool requestedMusicDirty;

	protected bool _useUnscaledLighting;

	protected bool _didInitiateItemStow;

	public bool instanceIsOverridingTrack;

	private static string[] _shortDayDisplayName;

	public static Queue<string> currentObjectDialogue;

	public static HashSet<string> worldStateIDs;

	public static List<Response> questionChoices;

	public static int xLocationAfterWarp;

	public static int yLocationAfterWarp;

	public static int gameTimeInterval;

	public static int currentQuestionChoice;

	public static int currentDialogueCharacterIndex;

	public static int dialogueTypingInterval;

	public static int dayOfMonth;

	public static int year;

	public static int timeOfDay;

	public static int timeOfDayAfterFade;

	public static int dialogueWidth;

	public static int facingDirectionAfterWarp;

	public static int mouseClickPolling;

	public static int gamePadXButtonPolling;

	public static int gamePadAButtonPolling;

	public static int weatherIcon;

	public static int hitShakeTimer;

	public static int staminaShakeTimer;

	public static int pauseThenDoFunctionTimer;

	public static int cursorTileHintCheckTimer;

	public static int timerUntilMouseFade;

	public static int whichFarm;

	public static int startingCabins;

	public static ModFarmType whichModFarm;

	public static ulong? startingGameSeed;

	public static int elliottPiano;

	public static Microsoft.Xna.Framework.Rectangle viewportClampArea;

	public static SaveFixes lastAppliedSaveFix;

	public static Color eveningColor;

	public static Color unselectedOptionColor;

	public static Color screenGlowColor;

	public static NPC currentSpeaker;

	/// <summary>A default random number generator used for a wide variety of randomization in the game. This provides non-repeatable randomization (e.g. reloading the save will produce different results).</summary>
	public static Random random;

	public static Random recentMultiplayerRandom;

	/// <summary>The cached data for achievements from <c>Data/Achievements</c>.</summary>
	public static Dictionary<int, string> achievements;

	/// <summary>The cached data for <see cref="F:StardewValley.ItemRegistry.type_bigCraftable" />-type items from <c>Data/BigCraftables</c>.</summary>
	public static IDictionary<string, BigCraftableData> bigCraftableData;

	/// <summary>The cached data for buildings from <c>Data/Buildings</c>.</summary>
	public static IDictionary<string, BuildingData> buildingData;

	/// <summary>The cached data for NPCs from <c>Data/Characters</c>.</summary>
	public static IDictionary<string, CharacterData> characterData;

	/// <summary>The cached data for crops from <c>Data/Crops</c>.</summary>
	public static IDictionary<string, CropData> cropData;

	/// <summary>The cached data for farm animals from <c>Data/FarmAnimals</c>.</summary>
	public static IDictionary<string, FarmAnimalData> farmAnimalData;

	/// <summary>The cached data for flooring and path items from <c>Data/FloorsAndPaths</c>.</summary>
	public static IDictionary<string, FloorPathData> floorPathData;

	/// <summary>The cached data for fruit trees from <c>Data/FruitTrees</c>.</summary>
	public static IDictionary<string, FruitTreeData> fruitTreeData;

	/// <summary>The cached data for jukebox tracks from <c>Data/JukeboxTracks</c>.</summary>
	public static IDictionary<string, JukeboxTrackData> jukeboxTrackData;

	/// <summary>The cached data for location contexts from <c>Data/LocationContexts</c>.</summary>
	public static IDictionary<string, LocationContextData> locationContextData;

	/// <summary>The cached data for <see cref="F:StardewValley.ItemRegistry.type_object" />-type items from <c>Data/Objects</c>.</summary>
	public static IDictionary<string, ObjectData> objectData;

	/// <summary>The cached data for <see cref="F:StardewValley.ItemRegistry.type_pants" />-type items from <c>Data/Pants</c>.</summary>
	public static IDictionary<string, PantsData> pantsData;

	/// <summary>The cached data for pets from <c>Data/Pets</c>.</summary>
	public static IDictionary<string, PetData> petData;

	/// <summary>The cached data for <see cref="F:StardewValley.ItemRegistry.type_shirt" />-type items from <c>Data/Shirts</c>.</summary>
	public static IDictionary<string, ShirtData> shirtData;

	/// <summary>The cached data for <see cref="F:StardewValley.ItemRegistry.type_tool" />-type items from <c>Data/Tools</c>.</summary>
	public static IDictionary<string, ToolData> toolData;

	/// <summary>The cached data for <see cref="F:StardewValley.ItemRegistry.type_weapon" />-type items from <c>Data/Weapons</c>.</summary>
	public static IDictionary<string, WeaponData> weaponData;

	public static List<HUDMessage> hudMessages;

	public static IDictionary<string, string> NPCGiftTastes;

	public static float musicPlayerVolume;

	public static float ambientPlayerVolume;

	public static float pauseAccumulator;

	public static float pauseTime;

	public static float upPolling;

	public static float downPolling;

	public static float rightPolling;

	public static float leftPolling;

	public static float debrisSoundInterval;

	public static float windGust;

	public static float dialogueButtonScale;

	public ICue instanceCurrentSong;

	public static IAudioCategory musicCategory;

	public static IAudioCategory soundCategory;

	public static IAudioCategory ambientCategory;

	public static IAudioCategory footstepCategory;

	public PlayerIndex instancePlayerOneIndex;

	[NonInstancedStatic]
	public static IAudioEngine audioEngine;

	[NonInstancedStatic]
	public static WaveBank waveBank;

	[NonInstancedStatic]
	public static WaveBank waveBank1_4;

	[NonInstancedStatic]
	public static ISoundBank soundBank;

	public static Vector2 previousViewportPosition;

	public static Vector2 currentCursorTile;

	public static Vector2 lastCursorTile;

	public static Vector2 snowPos;

	public Microsoft.Xna.Framework.Rectangle localMultiplayerWindow;

	public static RainDrop[] rainDrops;

	public static ICue chargeUpSound;

	public static ICue wind;

	/// <summary>The audio cues for the current location which are continuously looping until they're stopped.</summary>
	public static LoopingCueManager loopingLocationCues;

	/// <summary>Encapsulates the game logic for playing sound effects (excluding music and background ambience).</summary>
	public static ISoundsHelper sounds;

	[NonInstancedStatic]
	public static AudioCueModificationManager CueModification;

	public static List<WeatherDebris> debrisWeather;

	public static TemporaryAnimatedSpriteList screenOverlayTempSprites;

	public static TemporaryAnimatedSpriteList uiOverlayTempSprites;

	private static byte _gameMode;

	private bool _isSaving;

	/// <summary>Handles writing game messages to the log output.</summary>
	[NonInstancedStatic]
	protected internal static IGameLogger log;

	/// <summary>Combines hash codes in a deterministic way that's consistent between both sessions and players.</summary>
	[NonInstancedStatic]
	public static IHashUtility hash;

	protected internal static Multiplayer multiplayer;

	public static byte multiplayerMode;

	public static IEnumerator<int> currentLoader;

	public static ulong uniqueIDForThisGame;

	public static int[] directionKeyPolling;

	public static HashSet<LightSource> currentLightSources;

	public static Color ambientLight;

	public static Color outdoorLight;

	public static Color textColor;

	/// <summary>The default color for shadows drawn under text.</summary>
	public static Color textShadowColor;

	/// <summary>A darker version of <see cref="F:StardewValley.Game1.textShadowColor" /> used in some cases.</summary>
	public static Color textShadowDarkerColor;

	public static IClickableMenu overlayMenu;

	private static IClickableMenu _activeClickableMenu;

	/// <summary>The queue of menus to open when the <see cref="P:StardewValley.Game1.activeClickableMenu" /> is closed.</summary>
	/// <remarks>See also <see cref="P:StardewValley.Game1.activeClickableMenu" />, <see cref="F:StardewValley.Game1.onScreenMenus" />, and <see cref="F:StardewValley.Game1.overlayMenu" />.</remarks>
	public static List<IClickableMenu> nextClickableMenu;

	/// <summary>A queue of actions to perform when <see cref="M:StardewValley.Farmer.IsBusyDoingSomething" /> is false.</summary>
	/// <remarks>Most code should call <see cref="M:StardewValley.Game1.PerformActionWhenPlayerFree(System.Action)" /> instead of using this field directly.</remarks>
	public static List<Action> actionsWhenPlayerFree;

	public static bool isCheckingNonMousePlacement;

	private static IMinigame _currentMinigame;

	public static IList<IClickableMenu> onScreenMenus;

	public static BuffsDisplay buffsDisplay;

	public static DayTimeMoneyBox dayTimeMoneyBox;

	public static NetRootDictionary<long, Farmer> otherFarmers;

	private static readonly FarmerCollection _onlineFarmers;

	public static IGameServer server;

	public static Client client;

	public KeyboardDispatcher instanceKeyboardDispatcher;

	public static Background background;

	public static FarmEvent farmEvent;

	/// <summary>The farm event to play next, if a regular farm event doesn't play via <see cref="F:StardewValley.Game1.farmEvent" /> instead.</summary>
	/// <remarks>This is set via the <see cref="M:StardewValley.DebugCommands.DefaultHandlers.SetFarmEvent(System.String[],StardewValley.Logging.IGameLogger)" /> debug command.</remarks>
	public static FarmEvent farmEventOverride;

	public static afterFadeFunction afterFade;

	public static afterFadeFunction afterDialogues;

	public static afterFadeFunction afterViewport;

	public static afterFadeFunction viewportReachedTarget;

	public static afterFadeFunction afterPause;

	public static GameTime currentGameTime;

	public static IList<DelayedAction> delayedActions;

	public static Stack<IClickableMenu> endOfNightMenus;

	public Options instanceOptions;

	[NonInstancedStatic]
	public static SerializableDictionary<long, Options> splitscreenOptions;

	public static Game1 game1;

	public static Point lastMousePositionBeforeFade;

	public static int ticks;

	public static EmoteMenu emoteMenu;

	[NonInstancedStatic]
	public static SerializableDictionary<string, string> CustomData;

	/// <summary>Manages and synchronizes ready checks, which ensure all players are ready before proceeding (e.g. before sleeping).</summary>
	public static ReadySynchronizer netReady;

	public static NetRoot<NetWorldState> netWorldState;

	public static ChatBox chatBox;

	public TextEntryMenu instanceTextEntry;

	public static SpecialCurrencyDisplay specialCurrencyDisplay;

	private static string debugPresenceString;

	public static List<Action> remoteEventQueue;

	public static List<long> weddingsToday;

	public int instanceIndex;

	public int instanceId;

	public static bool overrideGameMenuReset;

	protected bool _windowResizing;

	protected Point _oldMousePosition;

	protected bool _oldGamepadConnectedState;

	protected int _oldScrollWheelValue;

	public static Point viewportCenter;

	public static Vector2 viewportTarget;

	public static float viewportSpeed;

	public static int viewportHold;

	private static bool _cursorDragEnabled;

	private static bool _cursorDragPrevEnabled;

	private static bool _cursorSpeedDirty;

	private const float CursorBaseSpeed = 16f;

	private static float _cursorSpeed;

	private static float _cursorSpeedScale;

	private static float _cursorUpdateElapsedSec;

	private static int thumbstickPollingTimer;

	public static bool toggleFullScreen;

	public static string whereIsTodaysFest;

	public const string NO_LETTER_MAIL = "%&NL&%";

	public const string BROADCAST_MAIL_FOR_TOMORROW_PREFIX = "%&MFT&%";

	public const string BROADCAST_SEEN_MAIL_PREFIX = "%&SM&%";

	public const string BROADCAST_MAILBOX_PREFIX = "%&MB&%";

	public bool isLocalMultiplayerNewDayActive;

	protected static Task _newDayTask;

	private static Action _afterNewDayAction;

	public static NewDaySynchronizer newDaySync;

	public static bool forceSnapOnNextViewportUpdate;

	public static Vector2 currentViewportTarget;

	public static Vector2 viewportPositionLerp;

	public static float screenGlowRate;

	public static float screenGlowMax;

	public static bool haltAfterCheck;

	public static bool uiMode;

	public static RenderTarget2D nonUIRenderTarget;

	public static int uiModeCount;

	protected static int _oldUIModeCount;

	internal string panModeString;

	public static bool conventionMode;

	internal static EventTest eventTest;

	internal bool panFacingDirectionWait;

	public static bool isRunningMacro;

	public static int thumbstickMotionMargin;

	public static float thumbstickMotionAccell;

	public static int triggerPolling;

	public static int rightClickPolling;

	private RenderTarget2D _screen;

	private RenderTarget2D _uiScreen;

	public static Color bgColor;

	protected readonly BlendState lightingBlend = new BlendState
	{
		ColorBlendFunction = BlendFunction.ReverseSubtract,
		ColorDestinationBlend = Blend.One,
		ColorSourceBlend = Blend.SourceColor
	};

	public bool isDrawing;

	[NonInstancedStatic]
	public static bool isRenderingScreenBuffer;

	protected bool _lastDrewMouseCursor;

	protected static int _activatedTick;

	/// <summary>The cursor icon to show, usually matching a constant like <see cref="F:StardewValley.Game1.cursor_default" />.</summary>
	public static int mouseCursor;

	private static float _mouseCursorTransparency;

	public static bool wasMouseVisibleThisFrame;

	public static NPC objectDialoguePortraitPerson;

	protected static StringBuilder _ParseTextStringBuilder;

	protected static StringBuilder _ParseTextStringBuilderLine;

	protected static StringBuilder _ParseTextStringBuilderWord;

	public bool ScreenshotBusy;

	public bool takingMapScreenshot;

	public bool IsActiveNoOverlay
	{
		get
		{
			if (!base.IsActive)
			{
				return false;
			}
			if (Program.sdk.HasOverlay)
			{
				return false;
			}
			return true;
		}
	}

	public static LocalizedContentManager temporaryContent
	{
		get
		{
			if (_temporaryContent == null)
			{
				_temporaryContent = content.CreateTemporary();
			}
			return _temporaryContent;
		}
	}

	public static Farmer player
	{
		get
		{
			return _player;
		}
		set
		{
			if (_player != null)
			{
				_player.unload();
				_player = null;
			}
			_player = value;
		}
	}

	public static bool IsPlayingBackgroundMusic
	{
		get
		{
			return game1._instanceIsPlayingBackgroundMusic;
		}
		set
		{
			game1._instanceIsPlayingBackgroundMusic = value;
		}
	}

	public static bool IsPlayingOutdoorsAmbience
	{
		get
		{
			return game1._instanceIsPlayingOutdoorsAmbience;
		}
		set
		{
			game1._instanceIsPlayingOutdoorsAmbience = value;
		}
	}

	public static bool IsPlayingNightAmbience
	{
		get
		{
			return game1._instanceIsPlayingNightAmbience;
		}
		set
		{
			game1._instanceIsPlayingNightAmbience = value;
		}
	}

	public static bool IsPlayingTownMusic
	{
		get
		{
			return game1._instanceIsPlayingTownMusic;
		}
		set
		{
			game1._instanceIsPlayingTownMusic = value;
		}
	}

	public static bool IsPlayingMorningSong
	{
		get
		{
			return game1._instanceIsPlayingMorningSong;
		}
		set
		{
			game1._instanceIsPlayingMorningSong = value;
		}
	}

	public static bool isWarping => _isWarping;

	public static IList<GameLocation> locations => game1._locations;

	public static GameLocation currentLocation
	{
		get
		{
			return game1.instanceGameLocation;
		}
		set
		{
			if (game1.instanceGameLocation != value)
			{
				if (_PreviousNonNullLocation == null)
				{
					_PreviousNonNullLocation = game1.instanceGameLocation;
				}
				game1.instanceGameLocation = value;
				if (game1.instanceGameLocation != null)
				{
					GameLocation previousNonNullLocation = _PreviousNonNullLocation;
					_PreviousNonNullLocation = null;
					OnLocationChanged(previousNonNullLocation, game1.instanceGameLocation);
				}
			}
		}
	}

	public static Texture2D toolSpriteSheet
	{
		get
		{
			if (_toolSpriteSheet == null)
			{
				ResetToolSpriteSheet();
			}
			return _toolSpriteSheet;
		}
	}

	public static RenderTarget2D lightmap => _lightmap;

	/// <summary>Whether today's weather state is a green rain day.</summary>
	public static bool isGreenRain
	{
		get
		{
			return _isGreenRain;
		}
		set
		{
			_isGreenRain = value;
			wasGreenRain |= value;
		}
	}

	public static bool spawnMonstersAtNight
	{
		get
		{
			return player.team.spawnMonstersAtNight;
		}
		set
		{
			player.team.spawnMonstersAtNight.Value = value;
		}
	}

	/// <summary>When the game makes a random choice, whether to use a simpler method that's prone to repeating patterns.</summary>
	/// <remarks>This is mainly intended for speedrunning, where full randomization might be undesirable.</remarks>
	public static bool UseLegacyRandom
	{
		get
		{
			return player.team.useLegacyRandom;
		}
		set
		{
			player.team.useLegacyRandom.Value = value;
		}
	}

	public static bool fadeToBlack
	{
		get
		{
			return screenFade.fadeToBlack;
		}
		set
		{
			screenFade.fadeToBlack = value;
		}
	}

	public static bool fadeIn
	{
		get
		{
			return screenFade.fadeIn;
		}
		set
		{
			screenFade.fadeIn = value;
		}
	}

	public static bool globalFade
	{
		get
		{
			return screenFade.globalFade;
		}
		set
		{
			screenFade.globalFade = value;
		}
	}

	public static bool nonWarpFade
	{
		get
		{
			return screenFade.nonWarpFade;
		}
		set
		{
			screenFade.nonWarpFade = value;
		}
	}

	public static float fadeToBlackAlpha
	{
		get
		{
			return screenFade.fadeToBlackAlpha;
		}
		set
		{
			screenFade.fadeToBlackAlpha = value;
		}
	}

	public static float globalFadeSpeed
	{
		get
		{
			return screenFade.globalFadeSpeed;
		}
		set
		{
			screenFade.globalFadeSpeed = value;
		}
	}

	public static string CurrentSeasonDisplayName => content.LoadString("Strings\\StringsFromCSFiles:" + currentSeason);

	/// <summary>The current season of the year as a string (one of <c>spring</c>, <c>summer</c>, <c>fall</c>, or <c>winter</c>).</summary>
	/// <remarks>Most code should use <see cref="F:StardewValley.Game1.season" /> instead.</remarks>
	public static string currentSeason
	{
		get
		{
			return Utility.getSeasonKey(season);
		}
		set
		{
			if (Utility.TryParseEnum<Season>(value, out var seasonValue))
			{
				season = seasonValue;
				return;
			}
			throw new ArgumentException("Can't parse value '" + value + "' as a season name.");
		}
	}

	/// <summary>The current season of the year as a numeric index.</summary>
	/// <remarks>Most code should use <see cref="F:StardewValley.Game1.season" /> instead.</remarks>
	public static int seasonIndex => (int)season;

	public static string debugOutput
	{
		get
		{
			return _debugOutput;
		}
		set
		{
			lock (_debugOutputLock)
			{
				if (_debugOutput != value)
				{
					_debugOutput = value;
					if (!string.IsNullOrEmpty(_debugOutput))
					{
						log.Debug("DebugOutput: " + _debugOutput);
					}
				}
			}
		}
	}

	public static string elliottBookName
	{
		get
		{
			if (player != null && player.DialogueQuestionsAnswered.Contains("958699"))
			{
				return content.LoadString("Strings\\Events:ElliottBook_mystery");
			}
			if (player != null && player.DialogueQuestionsAnswered.Contains("958700"))
			{
				return content.LoadString("Strings\\Events:ElliottBook_romance");
			}
			return content.LoadString("Strings\\Events:ElliottBook_default");
		}
		set
		{
		}
	}

	protected static Dictionary<MusicContext, KeyValuePair<string, bool>> _requestedMusicTracks
	{
		get
		{
			return game1._instanceRequestedMusicTracks;
		}
		set
		{
			game1._instanceRequestedMusicTracks = value;
		}
	}

	protected static MusicContext _activeMusicContext
	{
		get
		{
			return game1._instanceActiveMusicContext;
		}
		set
		{
			game1._instanceActiveMusicContext = value;
		}
	}

	public static bool isOverridingTrack
	{
		get
		{
			return game1.instanceIsOverridingTrack;
		}
		set
		{
			game1.instanceIsOverridingTrack = value;
		}
	}

	public bool useUnscaledLighting
	{
		get
		{
			return _useUnscaledLighting;
		}
		set
		{
			if (_useUnscaledLighting != value)
			{
				_useUnscaledLighting = value;
				allocateLightmap(localMultiplayerWindow.Width, localMultiplayerWindow.Height);
			}
		}
	}

	/// <inheritdoc cref="F:StardewValley.Farmer.mailbox" />
	public static IList<string> mailbox => player.mailbox;

	public static ICue currentSong
	{
		get
		{
			return game1.instanceCurrentSong;
		}
		set
		{
			game1.instanceCurrentSong = value;
		}
	}

	public static PlayerIndex playerOneIndex
	{
		get
		{
			return game1.instancePlayerOneIndex;
		}
		set
		{
			game1.instancePlayerOneIndex = value;
		}
	}

	/// <summary>The number of ticks since <see cref="P:StardewValley.Game1.gameMode" /> changed.</summary>
	public static int gameModeTicks { get; private set; }

	public static byte gameMode
	{
		get
		{
			return _gameMode;
		}
		set
		{
			if (_gameMode != value)
			{
				log.Verbose("gameMode was '" + GameModeToString(_gameMode) + "', set to '" + GameModeToString(value) + "'.");
				_gameMode = value;
				gameModeTicks = 0;
			}
		}
	}

	public bool IsSaving
	{
		get
		{
			return _isSaving;
		}
		set
		{
			_isSaving = value;
		}
	}

	public static Multiplayer Multiplayer => multiplayer;

	public static Stats stats => player.stats;

	/// <summary>The daily quest that's shown on the billboard, if any.</summary>
	public static Quest questOfTheDay => netWorldState.Value.QuestOfTheDay;

	/// <summary>The menu which is currently handling player interactions (e.g. a letter viewer, dialogue box, inventory, etc).</summary>
	/// <remarks>See also <see cref="F:StardewValley.Game1.nextClickableMenu" />, <see cref="F:StardewValley.Game1.onScreenMenus" />, and <see cref="F:StardewValley.Game1.overlayMenu" />.</remarks>
	public static IClickableMenu activeClickableMenu
	{
		get
		{
			return _activeClickableMenu;
		}
		set
		{
			bool num = (activeClickableMenu is SaveGameMenu || activeClickableMenu is ShippingMenu) && !(value is SaveGameMenu) && !(value is ShippingMenu);
			if (_activeClickableMenu is IDisposable disposable && !_activeClickableMenu.HasDependencies())
			{
				disposable.Dispose();
			}
			if (textEntry != null && _activeClickableMenu != value)
			{
				closeTextEntry();
			}
			if (_activeClickableMenu != null && value == null)
			{
				timerUntilMouseFade = 0;
			}
			_activeClickableMenu = value;
			if (num)
			{
				OnDayStarted();
			}
			if (_activeClickableMenu != null)
			{
				if (!eventUp || (CurrentEvent != null && CurrentEvent.playerControlSequence && !player.UsingTool))
				{
					player.Halt();
				}
			}
			else if (nextClickableMenu.Count > 0)
			{
				activeClickableMenu = nextClickableMenu[0];
				nextClickableMenu.RemoveAt(0);
			}
		}
	}

	public static IMinigame currentMinigame
	{
		get
		{
			return _currentMinigame;
		}
		set
		{
			_currentMinigame = value;
			if (value == null)
			{
				if (currentLocation != null)
				{
					setRichPresence("location", currentLocation.Name);
				}
				randomizeDebrisWeatherPositions(debrisWeather);
				randomizeRainPositions();
			}
			else if (value.minigameId() != null)
			{
				setRichPresence("minigame", value.minigameId());
			}
		}
	}

	public static Object dishOfTheDay
	{
		get
		{
			return netWorldState.Value.DishOfTheDay;
		}
		set
		{
			netWorldState.Value.DishOfTheDay = value;
		}
	}

	public static KeyboardDispatcher keyboardDispatcher
	{
		get
		{
			return game1.instanceKeyboardDispatcher;
		}
		set
		{
			game1.instanceKeyboardDispatcher = value;
		}
	}

	public static Options options
	{
		get
		{
			return game1.instanceOptions;
		}
		set
		{
			game1.instanceOptions = value;
		}
	}

	public static TextEntryMenu textEntry
	{
		get
		{
			return game1.instanceTextEntry;
		}
		set
		{
			game1.instanceTextEntry = value;
		}
	}

	public static WorldDate Date => netWorldState.Value.Date;

	public static bool NetTimePaused => netWorldState.Get().IsTimePaused;

	public static bool HostPaused => netWorldState.Get().IsPaused;

	/// <summary>Whether the game is currently in multiplayer mode with at least one other player connected.</summary>
	public static bool IsMultiplayer => otherFarmers.Count > 0;

	/// <summary>Whether this game instance is a farmhand connected to a remote host in multiplayer.</summary>
	public static bool IsClient => multiplayerMode == 1;

	/// <summary>Whether this game instance is the host in multiplayer.</summary>
	public static bool IsServer => multiplayerMode == 2;

	/// <summary>Whether this game instance is the main or host player.</summary>
	public static bool IsMasterGame
	{
		get
		{
			if (multiplayerMode != 0)
			{
				return multiplayerMode == 2;
			}
			return true;
		}
	}

	/// <summary>The main or host player instance.</summary>
	public static Farmer MasterPlayer
	{
		get
		{
			if (!IsMasterGame)
			{
				return serverHost.Value;
			}
			return player;
		}
	}

	public static bool IsChatting
	{
		get
		{
			if (chatBox != null)
			{
				return chatBox.isActive();
			}
			return false;
		}
		set
		{
			if (value != chatBox.isActive())
			{
				if (value)
				{
					chatBox.activate();
				}
				else
				{
					chatBox.clickAway();
				}
			}
		}
	}

	public static Event CurrentEvent
	{
		get
		{
			if (currentLocation == null)
			{
				return null;
			}
			return currentLocation.currentEvent;
		}
	}

	public static MineShaft mine => (locationRequest?.Location as MineShaft) ?? (currentLocation as MineShaft);

	public static int CurrentMineLevel => (currentLocation as MineShaft)?.mineLevel ?? 0;

	public static int CurrentPlayerLimit
	{
		get
		{
			if (netWorldState?.Value != null)
			{
				_ = netWorldState.Value.CurrentPlayerLimit;
				return netWorldState.Value.CurrentPlayerLimit;
			}
			return multiplayer.playerLimit;
		}
	}

	private static float thumbstickToMouseModifier
	{
		get
		{
			if (_cursorSpeedDirty)
			{
				ComputeCursorSpeed();
			}
			return _cursorSpeed / 720f * (float)viewport.Height * (float)currentGameTime.ElapsedGameTime.TotalSeconds;
		}
	}

	public static bool isFullscreen => graphics.IsFullScreen;

	/// <summary>Get whether it's summer in the valley.</summary>
	/// <remarks>See <see cref="M:StardewValley.GameLocation.IsSummerHere" /> to handle local seasons.</remarks>
	public static bool IsSummer => season == Season.Summer;

	/// <summary>Get whether it's spring in the valley.</summary>
	/// <remarks>See <see cref="M:StardewValley.GameLocation.IsSpringHere" /> to handle local seasons.</remarks>
	public static bool IsSpring => season == Season.Spring;

	/// <summary>Get whether it's fall in the valley.</summary>
	/// <remarks>See <see cref="M:StardewValley.GameLocation.IsFallHere" /> to handle local seasons.</remarks>
	public static bool IsFall => season == Season.Fall;

	/// <summary>Get whether it's winter in the valley.</summary>
	/// <remarks>See <see cref="M:StardewValley.GameLocation.IsWinterHere" /> to handle local seasons.</remarks>
	public static bool IsWinter => season == Season.Winter;

	public RenderTarget2D screen
	{
		get
		{
			return _screen;
		}
		set
		{
			if (_screen != null)
			{
				_screen.Dispose();
				_screen = null;
			}
			_screen = value;
		}
	}

	public RenderTarget2D uiScreen
	{
		get
		{
			return _uiScreen;
		}
		set
		{
			if (_uiScreen != null)
			{
				_uiScreen.Dispose();
				_uiScreen = null;
			}
			_uiScreen = value;
		}
	}

	public static float mouseCursorTransparency
	{
		get
		{
			return _mouseCursorTransparency;
		}
		set
		{
			_mouseCursorTransparency = value;
		}
	}

	public static void GetHasRoomAnotherFarmAsync(ReportHasRoomAnotherFarmDelegate callback)
	{
		if (LocalMultiplayer.IsLocalMultiplayer())
		{
			bool yes = GetHasRoomAnotherFarm();
			callback(yes);
			return;
		}
		Task task = new Task(delegate
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			bool hasRoomAnotherFarm = GetHasRoomAnotherFarm();
			callback(hasRoomAnotherFarm);
		});
		hooks.StartTask(task, "Farm_SpaceCheck");
	}

	private static string GameModeToString(byte mode)
	{
		return mode switch
		{
			4 => $"logoScreenGameMode ({mode})", 
			0 => $"titleScreenGameMode ({mode})", 
			1 => $"loadScreenGameMode ({mode})", 
			2 => $"newGameMode ({mode})", 
			3 => $"playingGameMode ({mode})", 
			6 => $"loadingMode ({mode})", 
			7 => $"saveMode ({mode})", 
			8 => $"saveCompleteMode ({mode})", 
			9 => $"selectGameScreen ({mode})", 
			10 => $"creditsMode ({mode})", 
			11 => $"errorLogMode ({mode})", 
			_ => $"unknown ({mode})", 
		};
	}

	/// <summary>Get a human-readable game version which includes the <see cref="F:StardewValley.Game1.version" />, <see cref="F:StardewValley.Game1.versionLabel" />, and <see cref="F:StardewValley.Game1.versionBuildNumber" />.</summary>
	public static string GetVersionString()
	{
		string label = version;
		if (!string.IsNullOrEmpty(versionLabel))
		{
			label = label + " '" + versionLabel + "'";
		}
		if (versionBuildNumber > 0)
		{
			label = label + " build " + versionBuildNumber;
		}
		return label;
	}

	public static void ResetToolSpriteSheet()
	{
		if (_toolSpriteSheet != null)
		{
			_toolSpriteSheet.Dispose();
			_toolSpriteSheet = null;
		}
		Texture2D texture = content.Load<Texture2D>("TileSheets\\tools");
		int w = texture.Width;
		int h = texture.Height;
		Texture2D texture2D = new Texture2D(game1.GraphicsDevice, w, h, mipmap: false, SurfaceFormat.Color);
		Color[] data = new Color[w * h];
		texture.GetData(data);
		texture2D.SetData(data);
		_toolSpriteSheet = texture2D;
	}

	public static void SetSaveName(string new_save_name)
	{
		if (new_save_name == null)
		{
			new_save_name = "";
		}
		_currentSaveName = new_save_name;
		_setSaveName = true;
	}

	public static string GetSaveGameName(bool set_value = true)
	{
		if (!_setSaveName && set_value)
		{
			string base_name = MasterPlayer.farmName.Value;
			string save_name = base_name;
			int collision_index = 2;
			while (SaveGame.IsNewGameSaveNameCollision(save_name))
			{
				save_name = base_name + collision_index;
				collision_index++;
			}
			SetSaveName(save_name);
		}
		return _currentSaveName;
	}

	private static void allocateLightmap(int width, int height)
	{
		int quality = 8;
		float zoom = 1f;
		if (options != null)
		{
			quality = options.lightingQuality;
			zoom = ((!game1.useUnscaledLighting) ? options.zoomLevel : 1f);
		}
		int w = (int)((float)width * (1f / zoom) + 64f) / (quality / 2);
		int h = (int)((float)height * (1f / zoom) + 64f) / (quality / 2);
		if (lightmap == null || lightmap.Width != w || lightmap.Height != h)
		{
			_lightmap?.Dispose();
			_lightmap = new RenderTarget2D(graphics.GraphicsDevice, w, h, mipMap: false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
		}
	}

	public static bool canHaveWeddingOnDay(int day, Season season)
	{
		if (!Utility.isFestivalDay(day, season))
		{
			return !Utility.isGreenRainDay(day, season);
		}
		return false;
	}

	/// <summary>Reset the <see cref="P:StardewValley.Game1.questOfTheDay" /> for today and synchronize it to other player. In multiplayer, this can only be called on the host instance.</summary>
	public static void RefreshQuestOfTheDay()
	{
		Quest quest = ((!Utility.isFestivalDay() && !Utility.isFestivalDay(dayOfMonth + 1, season)) ? Utility.getQuestOfTheDay() : null);
		quest?.dailyQuest.Set(newValue: true);
		quest?.reloadObjective();
		quest?.reloadDescription();
		netWorldState.Value.SetQuestOfTheDay(quest);
	}

	public static void ExitToTitle(Action postExitCallback = null)
	{
		currentMinigame?.unload();
		_requestedMusicTracks.Clear();
		UpdateRequestedMusicTrack();
		changeMusicTrack("none");
		setGameMode(0);
		exitToTitle = true;
		postExitToTitleCallback = postExitCallback;
	}

	static Game1()
	{
		realMilliSecondsPerGameMinute = 700;
		realMilliSecondsPerGameTenMinutes = realMilliSecondsPerGameMinute * 10;
		cursor_none = -1;
		cursor_default = 0;
		cursor_wait = 1;
		cursor_grab = 2;
		cursor_gift = 3;
		cursor_talk = 4;
		cursor_look = 5;
		cursor_harvest = 6;
		cursor_gamepad_pointer = 44;
		asianSpacingRegexString = "\\s|[（《“‘「『(](?:[\\w,%％]+|[^…—])[々ぁぃぅぇぉっゃゅょゎゕゖァィゥェォッャュョヶー]*[）》”’」』)，。、？！：；·～,.?!:;~…]*|.[々ぁぃぅぇぉっゃゅょゎゕゖァィゥェォッャュョヶー]*[·・].[々ぁぃぅぇぉっゃゅょゎゕゖァィゥェォッャュョヶー]*|(?:[\\w,%％]+|[^…—])[々ぁぃぅぇぉっゃゅょゎゕゖァィゥェォッャュョヶー]*[）》”’」』)]?(?:[，。、？！：；·～,.?!:;~…]{1,2}[）》”’」』)]?)?|[\\w,%％]+|.[々ぁぃぅぇぉっゃゅょゎゕゖァィゥェォッャュョヶー]+|……|——|.";
		MusicDuckTimer = 0f;
		thumbStickSensitivity = 0.1f;
		runThreshold = 0.5f;
		rightStickHoldTime = 0;
		emoteMenuShowTime = 250;
		nextFarmerWarpOffsetX = 0;
		nextFarmerWarpOffsetY = 0;
		keyboardFocusInstance = null;
		_isWarping = false;
		hasLocalClientsOnly = false;
		isUsingBackToFrontSorting = false;
		_debugStringBuilder = new StringBuilder();
		_locationLookup = new Dictionary<string, GameLocation>(StringComparer.OrdinalIgnoreCase);
		asianSpacingRegex = new Regex(asianSpacingRegexString, RegexOptions.ECMAScript);
		warpingForForcedRemoteEvent = false;
		_PreviousNonNullLocation = null;
		safeAreaBounds = default(Microsoft.Xna.Framework.Rectangle);
		npcDialogues = new Dictionary<string, Stack<Dialogue>>();
		morningQueue = new Queue<Action>();
		hooks = new ModHooks();
		input = new InputState();
		inputSimulator = null;
		_toolSpriteSheet = null;
		crabPotOverlayTiles = new Dictionary<Vector2, int>();
		_setSaveName = false;
		_currentSaveName = "";
		savePathOverride = "";
		mailDeliveredFromMailForTomorrow = new List<string>();
		screenGlowAlpha = 0f;
		flashAlpha = 0f;
		currentGemBirdIndex = 0;
		dialogueUp = false;
		dialogueTyping = false;
		isQuestion = false;
		newDay = false;
		eventUp = false;
		viewportFreeze = false;
		eventOver = false;
		screenGlow = false;
		screenGlowHold = false;
		killScreen = false;
		displayHUD = true;
		displayFarmer = true;
		showingHealth = false;
		cabinsSeparate = false;
		showingHealthBar = false;
		hasStartedDay = false;
		eventsSeenSinceLastLocationChange = new HashSet<string>();
		hasApplied1_3_UpdateChanges = false;
		hasApplied1_4_UpdateChanges = false;
		postExitToTitleCallback = null;
		bundleType = BundleType.Default;
		isRaining = false;
		isSnowing = false;
		isLightning = false;
		isDebrisWeather = false;
		_isGreenRain = false;
		wasGreenRain = false;
		greenRainNeedsCleanup = false;
		season = Season.Spring;
		bannedUsers = new SerializableDictionary<string, string>();
		_debugOutputLock = new object();
		requestedMusicTrack = "";
		messageAfterPause = "";
		samBandName = "The Alfalfas";
		loadingMessage = "";
		errorMessage = "";
		requestedMusicDirty = false;
		_shortDayDisplayName = new string[7];
		currentObjectDialogue = new Queue<string>();
		worldStateIDs = new HashSet<string>();
		questionChoices = new List<Response>();
		dayOfMonth = 0;
		year = 1;
		timeOfDay = 600;
		timeOfDayAfterFade = -1;
		whichModFarm = null;
		startingGameSeed = null;
		elliottPiano = 0;
		viewportClampArea = Microsoft.Xna.Framework.Rectangle.Empty;
		eveningColor = new Color(255, 255, 0);
		unselectedOptionColor = new Color(100, 100, 100);
		random = new Random();
		recentMultiplayerRandom = new Random();
		hudMessages = new List<HUDMessage>();
		dialogueButtonScale = 1f;
		lastCursorTile = Vector2.Zero;
		rainDrops = new RainDrop[70];
		loopingLocationCues = new LoopingCueManager();
		sounds = new SoundsHelper();
		CueModification = new AudioCueModificationManager();
		debrisWeather = new List<WeatherDebris>();
		screenOverlayTempSprites = new TemporaryAnimatedSpriteList();
		uiOverlayTempSprites = new TemporaryAnimatedSpriteList();
		log = new DefaultLogger(!Program.releaseBuild, shouldWriteToLogFile: false);
		hash = new HashUtility();
		multiplayer = new Multiplayer();
		uniqueIDForThisGame = Utility.NewUniqueIdForThisGame();
		directionKeyPolling = new int[4];
		currentLightSources = new HashSet<LightSource>();
		outdoorLight = new Color(255, 255, 0);
		textColor = new Color(34, 17, 34);
		textShadowColor = new Color(206, 156, 95);
		textShadowDarkerColor = new Color(221, 148, 84);
		nextClickableMenu = new List<IClickableMenu>();
		actionsWhenPlayerFree = new List<Action>();
		isCheckingNonMousePlacement = false;
		_currentMinigame = null;
		onScreenMenus = new List<IClickableMenu>();
		_onlineFarmers = new FarmerCollection();
		delayedActions = new List<DelayedAction>();
		endOfNightMenus = new Stack<IClickableMenu>();
		splitscreenOptions = new SerializableDictionary<long, Options>();
		CustomData = new SerializableDictionary<string, string>();
		netReady = new ReadySynchronizer();
		specialCurrencyDisplay = null;
		remoteEventQueue = new List<Action>();
		weddingsToday = new List<long>();
		viewportTarget = new Vector2(-2.1474836E+09f, -2.1474836E+09f);
		viewportSpeed = 2f;
		_cursorDragEnabled = false;
		_cursorDragPrevEnabled = false;
		_cursorSpeedDirty = true;
		_cursorSpeed = 16f;
		_cursorSpeedScale = 1f;
		_cursorUpdateElapsedSec = 0f;
		newDaySync = new NewDaySynchronizer();
		forceSnapOnNextViewportUpdate = false;
		screenGlowRate = 0.005f;
		haltAfterCheck = false;
		uiMode = false;
		nonUIRenderTarget = null;
		uiModeCount = 0;
		_oldUIModeCount = 0;
		conventionMode = false;
		isRunningMacro = false;
		thumbstickMotionAccell = 1f;
		bgColor = new Color(5, 3, 4);
		isRenderingScreenBuffer = false;
		_activatedTick = 0;
		mouseCursor = cursor_default;
		_mouseCursorTransparency = 1f;
		wasMouseVisibleThisFrame = true;
		_ParseTextStringBuilder = new StringBuilder(2408);
		_ParseTextStringBuilderLine = new StringBuilder(1024);
		_ParseTextStringBuilderWord = new StringBuilder(256);
		GameAssemblyName = typeof(Game1).Assembly.GetName().Name;
		AssemblyInformationalVersionAttribute attribute = typeof(Game1).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
		if (!string.IsNullOrWhiteSpace(attribute?.InformationalVersion))
		{
			string[] parts = attribute.InformationalVersion.Split(',');
			if (parts.Length == 4)
			{
				version = parts[0].Trim();
				if (!string.IsNullOrWhiteSpace(parts[1]))
				{
					versionLabel = parts[1].Trim();
				}
				if (!string.IsNullOrWhiteSpace(parts[2]))
				{
					if (!int.TryParse(parts[2], out var buildNumber))
					{
						throw new InvalidOperationException("Can't parse game build number value '" + parts[2] + "' as a number.");
					}
					versionBuildNumber = buildNumber;
				}
				if (!string.IsNullOrWhiteSpace(parts[3]))
				{
					Multiplayer.protocolVersionOverride = parts[3].Trim();
				}
			}
		}
		if (string.IsNullOrWhiteSpace(version))
		{
			throw new InvalidOperationException("No game version found in assembly info.");
		}
	}

	public Game1(PlayerIndex player_index, int index)
		: this()
	{
		instancePlayerOneIndex = player_index;
		instanceIndex = index;
	}

	public Game1()
	{
		instanceId = GameRunner.instance.GetNewInstanceID();
		if (Program.gamePtr == null)
		{
			Program.gamePtr = this;
		}
		_temporaryContent = CreateContentManager(base.Content.ServiceProvider, base.Content.RootDirectory);
	}

	public void TranslateFields()
	{
		LocalizedContentManager.localizedAssetNames.Clear();
		BaseEnchantment.ResetEnchantments();
		samBandName = content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2156");
		elliottBookName = content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2157");
		objectSpriteSheet = content.Load<Texture2D>("Maps\\springobjects");
		objectSpriteSheet_2 = content.Load<Texture2D>("TileSheets\\Objects_2");
		bobbersTexture = content.Load<Texture2D>("TileSheets\\bobbers");
		dialogueFont = content.Load<SpriteFont>("Fonts\\SpriteFont1");
		smallFont = content.Load<SpriteFont>("Fonts\\SmallFont");
		smallFont.LineSpacing = 28;
		switch (LocalizedContentManager.CurrentLanguageCode)
		{
		case LocalizedContentManager.LanguageCode.ko:
			smallFont.LineSpacing += 16;
			break;
		case LocalizedContentManager.LanguageCode.tr:
			smallFont.LineSpacing += 4;
			break;
		case LocalizedContentManager.LanguageCode.mod:
			smallFont.LineSpacing = LocalizedContentManager.CurrentModLanguage.SmallFontLineSpacing;
			break;
		}
		tinyFont = content.Load<SpriteFont>("Fonts\\tinyFont");
		objectData = DataLoader.Objects(content);
		bigCraftableData = DataLoader.BigCraftables(content);
		achievements = DataLoader.Achievements(content);
		CraftingRecipe.craftingRecipes = DataLoader.CraftingRecipes(content);
		CraftingRecipe.cookingRecipes = DataLoader.CookingRecipes(content);
		ItemRegistry.ResetCache();
		MovieTheater.ClearCachedLocalizedData();
		mouseCursors = content.Load<Texture2D>("LooseSprites\\Cursors");
		mouseCursors2 = content.Load<Texture2D>("LooseSprites\\Cursors2");
		mouseCursors_1_6 = content.Load<Texture2D>("LooseSprites\\Cursors_1_6");
		giftboxTexture = content.Load<Texture2D>("LooseSprites\\Giftbox");
		controllerMaps = content.Load<Texture2D>("LooseSprites\\ControllerMaps");
		NPCGiftTastes = DataLoader.NpcGiftTastes(content);
		_shortDayDisplayName[0] = content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3042");
		_shortDayDisplayName[1] = content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3043");
		_shortDayDisplayName[2] = content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3044");
		_shortDayDisplayName[3] = content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3045");
		_shortDayDisplayName[4] = content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3046");
		_shortDayDisplayName[5] = content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3047");
		_shortDayDisplayName[6] = content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3048");
	}

	public void exitEvent(object sender, EventArgs e)
	{
		multiplayer.Disconnect(Multiplayer.DisconnectType.ClosedGame);
		keyboardDispatcher.Cleanup();
	}

	public void refreshWindowSettings()
	{
		GameRunner.instance.OnWindowSizeChange(null, null);
	}

	public void Window_ClientSizeChanged(object sender, EventArgs e)
	{
		if (_windowResizing)
		{
			return;
		}
		log.Verbose("Window_ClientSizeChanged(); Window.ClientBounds=" + base.Window.ClientBounds.ToString());
		if (options == null)
		{
			log.Verbose("Window_ClientSizeChanged(); options is null, returning.");
			return;
		}
		_windowResizing = true;
		int w = (graphics.IsFullScreen ? graphics.PreferredBackBufferWidth : base.Window.ClientBounds.Width);
		int h = (graphics.IsFullScreen ? graphics.PreferredBackBufferHeight : base.Window.ClientBounds.Height);
		GameRunner.instance.ExecuteForInstances(delegate(Game1 instance)
		{
			instance.SetWindowSize(w, h);
		});
		_windowResizing = false;
	}

	public virtual void SetWindowSize(int w, int h)
	{
		Microsoft.Xna.Framework.Rectangle oldWindow = new Microsoft.Xna.Framework.Rectangle(viewport.X, viewport.Y, viewport.Width, viewport.Height);
		if (Environment.OSVersion.Platform == PlatformID.Win32NT)
		{
			if (w < 1280 && !graphics.IsFullScreen)
			{
				w = 1280;
			}
			if (h < 720 && !graphics.IsFullScreen)
			{
				h = 720;
			}
		}
		if (!graphics.IsFullScreen && base.Window.AllowUserResizing)
		{
			graphics.PreferredBackBufferWidth = w;
			graphics.PreferredBackBufferHeight = h;
		}
		if (base.IsMainInstance && graphics.SynchronizeWithVerticalRetrace != options.vsyncEnabled)
		{
			graphics.SynchronizeWithVerticalRetrace = options.vsyncEnabled;
			log.Verbose("Vsync toggled: " + graphics.SynchronizeWithVerticalRetrace);
		}
		graphics.ApplyChanges();
		try
		{
			if (graphics.IsFullScreen)
			{
				localMultiplayerWindow = new Microsoft.Xna.Framework.Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
			}
			else
			{
				localMultiplayerWindow = new Microsoft.Xna.Framework.Rectangle(0, 0, w, h);
			}
		}
		catch (Exception)
		{
		}
		defaultDeviceViewport = new Viewport(localMultiplayerWindow);
		List<Vector4> screen_splits = new List<Vector4>();
		if (GameRunner.instance.gameInstances.Count <= 1)
		{
			screen_splits.Add(new Vector4(0f, 0f, 1f, 1f));
		}
		else
		{
			switch (GameRunner.instance.gameInstances.Count)
			{
			case 2:
				screen_splits.Add(new Vector4(0f, 0f, 0.5f, 1f));
				screen_splits.Add(new Vector4(0.5f, 0f, 0.5f, 1f));
				break;
			case 3:
				screen_splits.Add(new Vector4(0f, 0f, 1f, 0.5f));
				screen_splits.Add(new Vector4(0f, 0.5f, 0.5f, 0.5f));
				screen_splits.Add(new Vector4(0.5f, 0.5f, 0.5f, 0.5f));
				break;
			case 4:
				screen_splits.Add(new Vector4(0f, 0f, 0.5f, 0.5f));
				screen_splits.Add(new Vector4(0.5f, 0f, 0.5f, 0.5f));
				screen_splits.Add(new Vector4(0f, 0.5f, 0.5f, 0.5f));
				screen_splits.Add(new Vector4(0.5f, 0.5f, 0.5f, 0.5f));
				break;
			}
		}
		if (GameRunner.instance.gameInstances.Count <= 1)
		{
			zoomModifier = 1f;
		}
		else
		{
			zoomModifier = 0.5f;
		}
		Vector4 current_screen_split = screen_splits[game1.instanceIndex];
		Vector2? old_ui_dimensions = null;
		if (uiScreen != null)
		{
			old_ui_dimensions = new Vector2(uiScreen.Width, uiScreen.Height);
		}
		localMultiplayerWindow.X = (int)((float)w * current_screen_split.X);
		localMultiplayerWindow.Y = (int)((float)h * current_screen_split.Y);
		localMultiplayerWindow.Width = (int)Math.Ceiling((float)w * current_screen_split.Z);
		localMultiplayerWindow.Height = (int)Math.Ceiling((float)h * current_screen_split.W);
		try
		{
			int sw = (int)Math.Ceiling((float)localMultiplayerWindow.Width * (1f / options.zoomLevel));
			int sh = (int)Math.Ceiling((float)localMultiplayerWindow.Height * (1f / options.zoomLevel));
			screen = new RenderTarget2D(graphics.GraphicsDevice, sw, sh, mipMap: false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
			screen.Name = "Screen";
			int uw = (int)Math.Ceiling((float)localMultiplayerWindow.Width / options.uiScale);
			int uh = (int)Math.Ceiling((float)localMultiplayerWindow.Height / options.uiScale);
			uiScreen = new RenderTarget2D(graphics.GraphicsDevice, uw, uh, mipMap: false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
			uiScreen.Name = "UI Screen";
		}
		catch (Exception)
		{
		}
		updateViewportForScreenSizeChange(fullscreenChange: false, localMultiplayerWindow.Width, localMultiplayerWindow.Height);
		if (old_ui_dimensions.HasValue && old_ui_dimensions.Value.X == (float)uiScreen.Width && old_ui_dimensions.Value.Y == (float)uiScreen.Height)
		{
			return;
		}
		PushUIMode();
		textEntry?.gameWindowSizeChanged(oldWindow, new Microsoft.Xna.Framework.Rectangle(viewport.X, viewport.Y, viewport.Width, viewport.Height));
		foreach (IClickableMenu onScreenMenu in onScreenMenus)
		{
			onScreenMenu.gameWindowSizeChanged(oldWindow, new Microsoft.Xna.Framework.Rectangle(viewport.X, viewport.Y, viewport.Width, viewport.Height));
		}
		currentMinigame?.changeScreenSize();
		activeClickableMenu?.gameWindowSizeChanged(oldWindow, new Microsoft.Xna.Framework.Rectangle(viewport.X, viewport.Y, viewport.Width, viewport.Height));
		if (activeClickableMenu is GameMenu gameMenu)
		{
			if (gameMenu.GetCurrentPage() is OptionsPage optionsPage)
			{
				optionsPage.preWindowSizeChange();
			}
			GameMenu gameMenuWhat = (GameMenu)(activeClickableMenu = new GameMenu(gameMenu.currentTab));
			if (gameMenuWhat.GetCurrentPage() is OptionsPage newOptionsPage)
			{
				newOptionsPage.postWindowSizeChange();
			}
		}
		PopUIMode();
	}

	private void Game1_Exiting(object sender, EventArgs e)
	{
		Program.sdk.Shutdown();
	}

	public static void setGameMode(byte mode)
	{
		log.Verbose("setGameMode( '" + GameModeToString(mode) + "' )");
		_gameMode = mode;
		temporaryContent?.Unload();
		switch (mode)
		{
		case 0:
		{
			bool skip = false;
			if (activeClickableMenu != null)
			{
				GameTime gameTime = currentGameTime;
				if (gameTime != null && gameTime.TotalGameTime.TotalSeconds > 10.0)
				{
					skip = true;
				}
			}
			if (game1.instanceIndex <= 0)
			{
				TitleMenu titleMenu = (TitleMenu)(activeClickableMenu = new TitleMenu());
				if (skip)
				{
					titleMenu.skipToTitleButtons();
				}
			}
			break;
		}
		case 3:
			hasApplied1_3_UpdateChanges = true;
			hasApplied1_4_UpdateChanges = false;
			break;
		}
	}

	public static void updateViewportForScreenSizeChange(bool fullscreenChange, int width, int height)
	{
		forceSnapOnNextViewportUpdate = true;
		if (graphics.GraphicsDevice != null)
		{
			allocateLightmap(width, height);
		}
		width = (int)Math.Ceiling((float)width / options.zoomLevel);
		height = (int)Math.Ceiling((float)height / options.zoomLevel);
		Point center = new Point(viewport.X + viewport.Width / 2, viewport.Y + viewport.Height / 2);
		bool size_dirty = false;
		if (viewport.Width != width || viewport.Height != height)
		{
			size_dirty = true;
		}
		viewport = new xTile.Dimensions.Rectangle(center.X - width / 2, center.Y - height / 2, width, height);
		if (currentLocation == null)
		{
			return;
		}
		if (eventUp)
		{
			if (!IsFakedBlackScreen() && currentLocation.IsOutdoors)
			{
				clampViewportToGameMap();
			}
			return;
		}
		if (viewport.X >= 0 || !currentLocation.IsOutdoors || fullscreenChange)
		{
			center = new Point(viewport.X + viewport.Width / 2, viewport.Y + viewport.Height / 2);
			viewport = new xTile.Dimensions.Rectangle(center.X - width / 2, center.Y - height / 2, width, height);
			UpdateViewPort(overrideFreeze: true, center);
		}
		if (size_dirty)
		{
			forceSnapOnNextViewportUpdate = true;
			randomizeRainPositions();
			randomizeDebrisWeatherPositions(debrisWeather);
		}
	}

	public void Instance_Initialize()
	{
		Initialize();
	}

	public static bool IsFading()
	{
		if (!globalFade && (!fadeIn || !(fadeToBlackAlpha > 0f)))
		{
			if (fadeToBlack)
			{
				return fadeToBlackAlpha < 1f;
			}
			return false;
		}
		return true;
	}

	public static bool IsFakedBlackScreen()
	{
		if (currentMinigame != null)
		{
			return false;
		}
		if (CurrentEvent != null && CurrentEvent.currentCustomEventScript != null)
		{
			return false;
		}
		if (!eventUp)
		{
			return false;
		}
		return (float)(int)Math.Floor((float)new Point(viewport.X + viewport.Width / 2, viewport.Y + viewport.Height / 2).X / 64f) <= -200f;
	}

	/// <summary>
	/// Allows the game to perform any initialization it needs to before starting to run.
	/// This is where it can query for any required services and load any non-graphic
	/// related content.  Calling base.Initialize will enumerate through any components
	/// and initialize them as well.
	/// </summary>
	protected override void Initialize()
	{
		keyboardDispatcher = new KeyboardDispatcher(base.Window);
		screenFade = new ScreenFade(onFadeToBlackComplete, onFadedBackInComplete);
		options = new Options();
		options.musicVolumeLevel = 1f;
		options.soundVolumeLevel = 1f;
		otherFarmers = new NetRootDictionary<long, Farmer>();
		otherFarmers.Serializer = SaveGame.farmerSerializer;
		viewport = new xTile.Dimensions.Rectangle(new Size(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight));
		string rootpath = base.Content.RootDirectory;
		if (base.IsMainInstance)
		{
			try
			{
				AudioEngine obj = new AudioEngine(Path.Combine(rootpath, "XACT", "FarmerSounds.xgs"));
				obj.GetReverbSettings()[18] = 4f;
				obj.GetReverbSettings()[17] = -12f;
				audioEngine = new AudioEngineWrapper(obj);
				waveBank = new WaveBank(audioEngine.Engine, Path.Combine(rootpath, "XACT", "Wave Bank.xwb"));
				waveBank1_4 = new WaveBank(audioEngine.Engine, Path.Combine(rootpath, "XACT", "Wave Bank(1.4).xwb"));
				soundBank = new SoundBankWrapper(new SoundBank(audioEngine.Engine, Path.Combine(rootpath, "XACT", "Sound Bank.xsb")));
			}
			catch (Exception e)
			{
				log.Error("Game.Initialize() caught exception initializing XACT.", e);
				audioEngine = new DummyAudioEngine();
				soundBank = new DummySoundBank();
			}
		}
		audioEngine.Update();
		musicCategory = audioEngine.GetCategory("Music");
		soundCategory = audioEngine.GetCategory("Sound");
		ambientCategory = audioEngine.GetCategory("Ambient");
		footstepCategory = audioEngine.GetCategory("Footsteps");
		currentSong = null;
		wind = soundBank.GetCue("wind");
		chargeUpSound = soundBank.GetCue("toolCharge");
		int width = graphics.GraphicsDevice.Viewport.Width;
		int height = graphics.GraphicsDevice.Viewport.Height;
		screen = new RenderTarget2D(graphics.GraphicsDevice, width, height, mipMap: false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
		allocateLightmap(width, height);
		AmbientLocationSounds.InitShared();
		previousViewportPosition = Vector2.Zero;
		PushUIMode();
		PopUIMode();
		setRichPresence("menus");
	}

	public static void pauseThenDoFunction(int pauseTime, afterFadeFunction function)
	{
		afterPause = function;
		pauseThenDoFunctionTimer = pauseTime;
	}

	protected internal virtual LocalizedContentManager CreateContentManager(IServiceProvider serviceProvider, string rootDirectory)
	{
		return new LocalizedContentManager(serviceProvider, rootDirectory);
	}

	public void Instance_LoadContent()
	{
		LoadContent();
	}

	/// <summary>LoadContent will be called once per game and is the place to load all of your content.</summary>
	protected override void LoadContent()
	{
		content = CreateContentManager(base.Content.ServiceProvider, base.Content.RootDirectory);
		xTileContent = CreateContentManager(content.ServiceProvider, content.RootDirectory);
		mapDisplayDevice = new XnaDisplayDevice(content, base.GraphicsDevice);
		spriteBatch = new SpriteBatch(base.GraphicsDevice);
		bigCraftableData = DataLoader.BigCraftables(content);
		objectData = DataLoader.Objects(content);
		cropData = DataLoader.Crops(content);
		characterData = DataLoader.Characters(content);
		achievements = DataLoader.Achievements(content);
		buildingData = DataLoader.Buildings(content);
		farmAnimalData = DataLoader.FarmAnimals(content);
		floorPathData = DataLoader.FloorsAndPaths(content);
		fruitTreeData = DataLoader.FruitTrees(content);
		locationContextData = DataLoader.LocationContexts(content);
		pantsData = DataLoader.Pants(content);
		petData = DataLoader.Pets(content);
		shirtData = DataLoader.Shirts(content);
		toolData = DataLoader.Tools(content);
		weaponData = DataLoader.Weapons(content);
		NPCGiftTastes = DataLoader.NpcGiftTastes(content);
		CraftingRecipe.InitShared();
		ItemRegistry.ResetCache();
		jukeboxTrackData = new Dictionary<string, JukeboxTrackData>(StringComparer.OrdinalIgnoreCase);
		foreach (KeyValuePair<string, JukeboxTrackData> pair in DataLoader.JukeboxTracks(content))
		{
			if (!jukeboxTrackData.TryAdd(pair.Key, pair.Value))
			{
				log.Warn("Ignored duplicate ID '" + pair.Key + "' in Data/JukeboxTracks.");
			}
		}
		concessionsSpriteSheet = content.Load<Texture2D>("LooseSprites\\Concessions");
		birdsSpriteSheet = content.Load<Texture2D>("LooseSprites\\birds");
		daybg = content.Load<Texture2D>("LooseSprites\\daybg");
		nightbg = content.Load<Texture2D>("LooseSprites\\nightbg");
		menuTexture = content.Load<Texture2D>("Maps\\MenuTiles");
		uncoloredMenuTexture = content.Load<Texture2D>("Maps\\MenuTilesUncolored");
		lantern = content.Load<Texture2D>("LooseSprites\\Lighting\\lantern");
		windowLight = content.Load<Texture2D>("LooseSprites\\Lighting\\windowLight");
		sconceLight = content.Load<Texture2D>("LooseSprites\\Lighting\\sconceLight");
		cauldronLight = content.Load<Texture2D>("LooseSprites\\Lighting\\greenLight");
		indoorWindowLight = content.Load<Texture2D>("LooseSprites\\Lighting\\indoorWindowLight");
		shadowTexture = content.Load<Texture2D>("LooseSprites\\shadow");
		mouseCursors = content.Load<Texture2D>("LooseSprites\\Cursors");
		mouseCursors2 = content.Load<Texture2D>("LooseSprites\\Cursors2");
		mouseCursors_1_6 = content.Load<Texture2D>("LooseSprites\\Cursors_1_6");
		giftboxTexture = content.Load<Texture2D>("LooseSprites\\Giftbox");
		controllerMaps = content.Load<Texture2D>("LooseSprites\\ControllerMaps");
		animations = content.Load<Texture2D>("TileSheets\\animations");
		objectSpriteSheet = content.Load<Texture2D>("Maps\\springobjects");
		objectSpriteSheet_2 = content.Load<Texture2D>("TileSheets\\Objects_2");
		bobbersTexture = content.Load<Texture2D>("TileSheets\\bobbers");
		cropSpriteSheet = content.Load<Texture2D>("TileSheets\\crops");
		emoteSpriteSheet = content.Load<Texture2D>("TileSheets\\emotes");
		debrisSpriteSheet = content.Load<Texture2D>("TileSheets\\debris");
		bigCraftableSpriteSheet = content.Load<Texture2D>("TileSheets\\Craftables");
		rainTexture = content.Load<Texture2D>("TileSheets\\rain");
		buffsIcons = content.Load<Texture2D>("TileSheets\\BuffsIcons");
		Tool.weaponsTexture = content.Load<Texture2D>("TileSheets\\weapons");
		FarmerRenderer.hairStylesTexture = content.Load<Texture2D>("Characters\\Farmer\\hairstyles");
		FarmerRenderer.shirtsTexture = content.Load<Texture2D>("Characters\\Farmer\\shirts");
		FarmerRenderer.pantsTexture = content.Load<Texture2D>("Characters\\Farmer\\pants");
		FarmerRenderer.hatsTexture = content.Load<Texture2D>("Characters\\Farmer\\hats");
		FarmerRenderer.accessoriesTexture = content.Load<Texture2D>("Characters\\Farmer\\accessories");
		MapSeat.mapChairTexture = content.Load<Texture2D>("TileSheets\\ChairTiles");
		SpriteText.spriteTexture = content.Load<Texture2D>("LooseSprites\\font_bold");
		SpriteText.coloredTexture = content.Load<Texture2D>("LooseSprites\\font_colored");
		Projectile.projectileSheet = content.Load<Texture2D>("TileSheets\\Projectiles");
		Color[] white = new Color[1] { Color.White };
		fadeToBlackRect = new Texture2D(base.GraphicsDevice, 1, 1, mipmap: false, SurfaceFormat.Color);
		fadeToBlackRect.SetData(white);
		white = new Color[1];
		for (int i = 0; i < white.Length; i++)
		{
			white[i] = new Color(255, 255, 255, 255);
		}
		staminaRect = new Texture2D(base.GraphicsDevice, 1, 1, mipmap: false, SurfaceFormat.Color);
		staminaRect.SetData(white);
		onScreenMenus.Clear();
		onScreenMenus.Add(dayTimeMoneyBox = new DayTimeMoneyBox());
		onScreenMenus.Add(new Toolbar());
		onScreenMenus.Add(buffsDisplay = new BuffsDisplay());
		for (int i = 0; i < 70; i++)
		{
			rainDrops[i] = new RainDrop(random.Next(viewport.Width), random.Next(viewport.Height), random.Next(4), random.Next(70));
		}
		dialogueWidth = Math.Min(1024, graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Width - 256);
		dialogueFont = content.Load<SpriteFont>("Fonts\\SpriteFont1");
		dialogueFont.LineSpacing = 42;
		smallFont = content.Load<SpriteFont>("Fonts\\SmallFont");
		smallFont.LineSpacing = 28;
		tinyFont = content.Load<SpriteFont>("Fonts\\tinyFont");
		_shortDayDisplayName[0] = content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3042");
		_shortDayDisplayName[1] = content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3043");
		_shortDayDisplayName[2] = content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3044");
		_shortDayDisplayName[3] = content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3045");
		_shortDayDisplayName[4] = content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3046");
		_shortDayDisplayName[5] = content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3047");
		_shortDayDisplayName[6] = content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3048");
		saveOnNewDay = true;
		if (gameMode == 4)
		{
			fadeToBlackAlpha = -0.5f;
			fadeIn = true;
		}
		if (random.NextDouble() < 0.7)
		{
			isDebrisWeather = true;
			populateDebrisWeatherArray();
		}
		netWorldState = new NetRoot<NetWorldState>(new NetWorldState());
		resetPlayer();
		CueModification.OnStartup();
		setGameMode(0);
	}

	public static void resetPlayer()
	{
		List<Item> farmersInitialTools = Farmer.initialTools();
		player = new Farmer(new FarmerSprite(null), new Vector2(192f, 192f), 1, "", farmersInitialTools, isMale: true);
	}

	public static void resetVariables()
	{
		xLocationAfterWarp = 0;
		yLocationAfterWarp = 0;
		gameTimeInterval = 0;
		currentQuestionChoice = 0;
		currentDialogueCharacterIndex = 0;
		dialogueTypingInterval = 0;
		dayOfMonth = 0;
		year = 1;
		timeOfDay = 600;
		timeOfDayAfterFade = -1;
		facingDirectionAfterWarp = 0;
		dialogueWidth = 0;
		facingDirectionAfterWarp = 0;
		mouseClickPolling = 0;
		weatherIcon = 0;
		hitShakeTimer = 0;
		staminaShakeTimer = 0;
		pauseThenDoFunctionTimer = 0;
		weatherForTomorrow = "Sun";
	}

	/// <summary>Play a game sound for the local player.</summary>
	/// <param name="cueName">The sound ID to play.</param>
	/// <param name="pitch">The pitch modifier to apply, or <c>null</c> for the default pitch.</param>
	/// <returns>Returns whether the cue exists and was started successfully.</returns>
	/// <remarks>To play audio in a specific location, see <see cref="M:StardewValley.GameLocation.playSound(System.String,System.Nullable{Microsoft.Xna.Framework.Vector2},System.Nullable{System.Int32},StardewValley.Audio.SoundContext)" /> or <see cref="M:StardewValley.GameLocation.localSound(System.String,System.Nullable{Microsoft.Xna.Framework.Vector2},System.Nullable{System.Int32},StardewValley.Audio.SoundContext)" /> instead.</remarks>
	public static bool playSound(string cueName, int? pitch = null)
	{
		ICue cue;
		return sounds.PlayLocal(cueName, null, null, pitch, SoundContext.Default, out cue);
	}

	/// <summary>Play a game sound for the local player.</summary>
	/// <param name="cueName">The sound ID to play.</param>
	/// <param name="cue">The cue instance that was started, or a no-op cue if it failed.</param>
	/// <returns>Returns whether the cue exists and was started successfully.</returns>
	/// <remarks>To play audio in a specific location, see <see cref="M:StardewValley.GameLocation.playSound(System.String,System.Nullable{Microsoft.Xna.Framework.Vector2},System.Nullable{System.Int32},StardewValley.Audio.SoundContext)" /> or <see cref="M:StardewValley.GameLocation.localSound(System.String,System.Nullable{Microsoft.Xna.Framework.Vector2},System.Nullable{System.Int32},StardewValley.Audio.SoundContext)" /> instead.</remarks>
	public static bool playSound(string cueName, out ICue cue)
	{
		return sounds.PlayLocal(cueName, null, null, null, SoundContext.Default, out cue);
	}

	/// <summary>Play a game sound for the local player.</summary>
	/// <param name="cueName">The sound ID to play.</param>
	/// <param name="pitch">The pitch modifier to apply.</param>
	/// <param name="cue">The cue instance that was started, or a no-op cue if it failed.</param>
	/// <returns>Returns whether the cue exists and was started successfully.</returns>
	/// <remarks>To play audio in a specific location, see <see cref="M:StardewValley.GameLocation.playSound(System.String,System.Nullable{Microsoft.Xna.Framework.Vector2},System.Nullable{System.Int32},StardewValley.Audio.SoundContext)" /> or <see cref="M:StardewValley.GameLocation.localSound(System.String,System.Nullable{Microsoft.Xna.Framework.Vector2},System.Nullable{System.Int32},StardewValley.Audio.SoundContext)" /> instead.</remarks>
	public static bool playSound(string cueName, int pitch, out ICue cue)
	{
		return sounds.PlayLocal(cueName, null, null, pitch, SoundContext.Default, out cue);
	}

	public static void setRichPresence(string friendlyName, object argument = null)
	{
		switch (friendlyName)
		{
		case "menus":
			debugPresenceString = "In menus";
			break;
		case "location":
			debugPresenceString = $"At {argument}";
			break;
		case "festival":
			debugPresenceString = $"At {argument}";
			break;
		case "fishing":
			debugPresenceString = $"Fishing at {argument}";
			break;
		case "minigame":
			debugPresenceString = $"Playing {argument}";
			break;
		case "wedding":
			debugPresenceString = $"Getting married to {argument}";
			break;
		case "earnings":
			debugPresenceString = $"Made {argument}g last night";
			break;
		case "giantcrop":
			debugPresenceString = $"Just harvested a Giant {argument}";
			break;
		}
	}

	public static void GenerateBundles(BundleType bundle_type, bool use_seed = true)
	{
		if (bundle_type == BundleType.Remixed)
		{
			Random r = (use_seed ? Utility.CreateRandom((double)uniqueIDForThisGame * 9.0) : new Random());
			Dictionary<string, string> bundle_data = new BundleGenerator().Generate(DataLoader.RandomBundles(content), r);
			netWorldState.Value.SetBundleData(bundle_data);
		}
		else
		{
			netWorldState.Value.SetBundleData(DataLoader.Bundles(content));
		}
	}

	public void SetNewGameOption<T>(string key, T val)
	{
		newGameSetupOptions[key] = val;
	}

	public T GetNewGameOption<T>(string key)
	{
		if (!newGameSetupOptions.TryGetValue(key, out var value))
		{
			return default(T);
		}
		return (T)value;
	}

	public virtual void loadForNewGame(bool loadedGame = false)
	{
		if (startingGameSeed.HasValue)
		{
			uniqueIDForThisGame = startingGameSeed.Value;
		}
		specialCurrencyDisplay = new SpecialCurrencyDisplay();
		flushLocationLookup();
		locations.Clear();
		mailbox.Clear();
		currentLightSources.Clear();
		questionChoices.Clear();
		hudMessages.Clear();
		weddingToday = false;
		timeOfDay = 600;
		season = Season.Spring;
		if (!loadedGame)
		{
			year = 1;
		}
		dayOfMonth = 0;
		isQuestion = false;
		nonWarpFade = false;
		newDay = false;
		eventUp = false;
		viewportFreeze = false;
		eventOver = false;
		screenGlow = false;
		screenGlowHold = false;
		screenGlowUp = false;
		isRaining = false;
		wasGreenRain = false;
		killScreen = false;
		messagePause = false;
		isDebrisWeather = false;
		weddingToday = false;
		exitToTitle = false;
		dialogueUp = false;
		postExitToTitleCallback = null;
		displayHUD = true;
		messageAfterPause = "";
		samBandName = content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2156");
		background = null;
		currentCursorTile = Vector2.Zero;
		if (!loadedGame)
		{
			lastAppliedSaveFix = SaveMigrator.LatestSaveFix;
		}
		resetVariables();
		player.team.sharedDailyLuck.Value = 0.001;
		if (!loadedGame)
		{
			options = new Options();
			options.LoadDefaultOptions();
			initializeVolumeLevels();
		}
		game1.CheckGamepadMode();
		onScreenMenus.Add(chatBox = new ChatBox());
		outdoorLight = Color.White;
		ambientLight = Color.White;
		UpdateDishOfTheDay();
		locations.Clear();
		Farm farm = new Farm("Maps\\" + Farm.getMapNameFromTypeInt(whichFarm), "Farm");
		locations.Add(farm);
		AddLocations();
		foreach (GameLocation location in locations)
		{
			location.AddDefaultBuildings();
		}
		forceSnapOnNextViewportUpdate = true;
		farm.onNewGame();
		if (!loadedGame)
		{
			foreach (GameLocation location2 in locations)
			{
				if (location2 is IslandLocation islandLocation)
				{
					islandLocation.AddAdditionalWalnutBushes();
				}
			}
		}
		if (!loadedGame)
		{
			hooks.CreatedInitialLocations();
		}
		else
		{
			hooks.SaveAddedLocations();
		}
		if (!loadedGame)
		{
			AddNPCs();
		}
		WarpPathfindingCache.PopulateCache();
		if (!loadedGame)
		{
			GenerateBundles(bundleType);
			foreach (string value in netWorldState.Value.BundleData.Values)
			{
				string[] item_split = ArgUtility.SplitBySpace(value.Split('/')[2]);
				if (!game1.GetNewGameOption<bool>("YearOneCompletable"))
				{
					continue;
				}
				for (int i = 0; i < item_split.Length; i += 3)
				{
					if (item_split[i] == "266")
					{
						int visits = (16 - 2) * 2;
						visits += 3;
						Random r = Utility.CreateRandom((double)uniqueIDForThisGame * 12.0);
						netWorldState.Value.VisitsUntilY1Guarantee = r.Next(2, visits);
					}
				}
			}
			netWorldState.Value.ShuffleMineChests = game1.GetNewGameOption<MineChestType>("MineChests");
			if (game1.newGameSetupOptions.ContainsKey("SpawnMonstersAtNight"))
			{
				spawnMonstersAtNight = game1.GetNewGameOption<bool>("SpawnMonstersAtNight");
			}
		}
		player.ConvertClothingOverrideToClothesItems();
		player.addQuest("9");
		RefreshQuestOfTheDay();
		player.currentLocation = RequireLocation("FarmHouse");
		player.gameVersion = version;
		hudMessages.Clear();
		hasLoadedGame = true;
		setGraphicsForSeason(onLoad: true);
		if (!loadedGame)
		{
			_setSaveName = false;
		}
		game1.newGameSetupOptions.Clear();
		updateCellarAssignments();
		if (!loadedGame && netWorldState != null && netWorldState.Value != null)
		{
			netWorldState.Value.RegisterSpecialCurrencies();
		}
	}

	public bool IsLocalCoopJoinable()
	{
		if (GameRunner.instance.gameInstances.Count >= GameRunner.instance.GetMaxSimultaneousPlayers())
		{
			return false;
		}
		if (IsClient)
		{
			return false;
		}
		return true;
	}

	public static void StartLocalMultiplayerIfNecessary()
	{
		if (multiplayerMode == 0)
		{
			log.Verbose("Starting multiplayer server for local multiplayer...");
			multiplayerMode = 2;
			if (server == null)
			{
				multiplayer.StartLocalMultiplayerServer();
			}
		}
	}

	public static void EndLocalMultiplayer()
	{
	}

	public static void UpdatePassiveFestivalStates()
	{
		netWorldState.Value.ActivePassiveFestivals.Clear();
		foreach (KeyValuePair<string, PassiveFestivalData> pair in DataLoader.PassiveFestivals(content))
		{
			string id = pair.Key;
			PassiveFestivalData festival = pair.Value;
			if (dayOfMonth >= festival.StartDay && dayOfMonth <= festival.EndDay && season == festival.Season && GameStateQuery.CheckConditions(festival.Condition))
			{
				netWorldState.Value.ActivePassiveFestivals.Add(id);
			}
		}
	}

	public void Instance_UnloadContent()
	{
		UnloadContent();
	}

	/// <summary>
	/// UnloadContent will be called once per game and is the place to unload
	/// all content.
	/// </summary>
	protected override void UnloadContent()
	{
		base.UnloadContent();
		spriteBatch.Dispose();
		content.Unload();
		xTileContent.Unload();
		server?.stopServer();
	}

	public static void showRedMessage(string message, bool playSound = true)
	{
		addHUDMessage(new HUDMessage(message, 3));
		if (!message.Contains("Inventory") && playSound)
		{
			Game1.playSound("cancel");
		}
		else if (player.mailReceived.Add("BackpackTip"))
		{
			addMailForTomorrow("pierreBackpack");
		}
	}

	public static void showRedMessageUsingLoadString(string loadString, bool playSound = true)
	{
		showRedMessage(content.LoadString(loadString), playSound);
	}

	public static bool didPlayerJustLeftClick(bool ignoreNonMouseHeldInput = false)
	{
		if (input.GetMouseState().LeftButton == ButtonState.Pressed && oldMouseState.LeftButton != ButtonState.Pressed)
		{
			return true;
		}
		if (input.GetGamePadState().Buttons.X == ButtonState.Pressed && (!ignoreNonMouseHeldInput || !oldPadState.IsButtonDown(Buttons.X)))
		{
			return true;
		}
		if (isOneOfTheseKeysDown(input.GetKeyboardState(), options.useToolButton) && (!ignoreNonMouseHeldInput || areAllOfTheseKeysUp(oldKBState, options.useToolButton)))
		{
			return true;
		}
		return false;
	}

	public static bool didPlayerJustRightClick(bool ignoreNonMouseHeldInput = false)
	{
		if (input.GetMouseState().RightButton == ButtonState.Pressed && oldMouseState.RightButton != ButtonState.Pressed)
		{
			return true;
		}
		if (input.GetGamePadState().Buttons.A == ButtonState.Pressed && (!ignoreNonMouseHeldInput || !oldPadState.IsButtonDown(Buttons.A)))
		{
			return true;
		}
		if (isOneOfTheseKeysDown(input.GetKeyboardState(), options.actionButton) && (!ignoreNonMouseHeldInput || !isOneOfTheseKeysDown(oldKBState, options.actionButton)))
		{
			return true;
		}
		return false;
	}

	public static bool didPlayerJustClickAtAll(bool ignoreNonMouseHeldInput = false)
	{
		if (!didPlayerJustLeftClick(ignoreNonMouseHeldInput))
		{
			return didPlayerJustRightClick(ignoreNonMouseHeldInput);
		}
		return true;
	}

	public static void showGlobalMessage(string message)
	{
		addHUDMessage(HUDMessage.ForCornerTextbox(message));
	}

	public static void globalFadeToBlack(afterFadeFunction afterFade = null, float fadeSpeed = 0.02f)
	{
		screenFade.GlobalFadeToBlack(afterFade, fadeSpeed);
	}

	public static void globalFadeToClear(afterFadeFunction afterFade = null, float fadeSpeed = 0.02f)
	{
		screenFade.GlobalFadeToClear(afterFade, fadeSpeed);
	}

	public void CheckGamepadMode()
	{
		bool old_gamepad_active_state = options.gamepadControls;
		switch (options.gamepadMode)
		{
		case Options.GamepadModes.ForceOn:
			options.gamepadControls = true;
			return;
		case Options.GamepadModes.ForceOff:
			options.gamepadControls = false;
			return;
		}
		MouseState mouseState = input.GetMouseState();
		KeyboardState keyState = GetKeyboardState();
		GamePadState padState = input.GetGamePadState();
		bool non_gamepad_control_was_used = false;
		if ((mouseState.LeftButton == ButtonState.Pressed || mouseState.MiddleButton == ButtonState.Pressed || mouseState.RightButton == ButtonState.Pressed || mouseState.ScrollWheelValue != _oldScrollWheelValue || ((mouseState.X != _oldMousePosition.X || mouseState.Y != _oldMousePosition.Y) && lastCursorMotionWasMouse) || keyState.GetPressedKeys().Length != 0) && (keyState.GetPressedKeys().Length != 1 || keyState.GetPressedKeys()[0] != Keys.Pause))
		{
			non_gamepad_control_was_used = true;
			if (Program.sdk is SteamHelper steamHelper && steamHelper.IsRunningOnSteamDeck())
			{
				non_gamepad_control_was_used = false;
			}
		}
		_oldScrollWheelValue = mouseState.ScrollWheelValue;
		_oldMousePosition.X = mouseState.X;
		_oldMousePosition.Y = mouseState.Y;
		bool gamepad_control_was_used = isAnyGamePadButtonBeingPressed() || isDPadPressed() || isGamePadThumbstickInMotion() || padState.Triggers.Left != 0f || padState.Triggers.Right != 0f;
		if (_oldGamepadConnectedState != padState.IsConnected)
		{
			_oldGamepadConnectedState = padState.IsConnected;
			if (_oldGamepadConnectedState)
			{
				options.gamepadControls = true;
				showGlobalMessage(content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2574"));
			}
			else
			{
				options.gamepadControls = false;
				if (instancePlayerOneIndex != (PlayerIndex)(-1))
				{
					showGlobalMessage(content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2575"));
					if (CanShowPauseMenu() && activeClickableMenu == null)
					{
						activeClickableMenu = new GameMenu();
					}
				}
			}
		}
		if (non_gamepad_control_was_used && options.gamepadControls)
		{
			options.gamepadControls = false;
		}
		if (!options.gamepadControls && gamepad_control_was_used)
		{
			options.gamepadControls = true;
		}
		if (old_gamepad_active_state == options.gamepadControls || !options.gamepadControls)
		{
			return;
		}
		lastMousePositionBeforeFade = new Point(localMultiplayerWindow.Width / 2, localMultiplayerWindow.Height / 2);
		if (activeClickableMenu != null)
		{
			activeClickableMenu.setUpForGamePadMode();
			if (options.SnappyMenus)
			{
				activeClickableMenu.populateClickableComponentList();
				activeClickableMenu.snapToDefaultClickableComponent();
			}
		}
		timerUntilMouseFade = 0;
	}

	public void Instance_Update(GameTime gameTime)
	{
		Update(gameTime);
	}

	protected override void Update(GameTime gameTime)
	{
		GameTime time = gameTime;
		DebugTools.BeforeGameUpdate(this, ref time);
		input.UpdateStates();
		if (input.GetGamePadState().IsButtonDown(Buttons.RightStick))
		{
			rightStickHoldTime += gameTime.ElapsedGameTime.Milliseconds;
		}
		GameMenu.bundleItemHovered = false;
		_update(time);
		if (IsMultiplayer && player != null)
		{
			player.requestingTimePause.Value = !shouldTimePass(LocalMultiplayer.IsLocalMultiplayer(is_local_only: true));
			if (IsMasterGame)
			{
				bool should_time_pause = false;
				if (LocalMultiplayer.IsLocalMultiplayer(is_local_only: true))
				{
					should_time_pause = true;
					foreach (Farmer onlineFarmer in getOnlineFarmers())
					{
						if (!onlineFarmer.requestingTimePause.Value)
						{
							should_time_pause = false;
							break;
						}
					}
				}
				netWorldState.Value.IsTimePaused = should_time_pause;
			}
		}
		Rumble.update(gameTime.ElapsedGameTime.Milliseconds);
		if (options.gamepadControls && thumbstickMotionMargin > 0)
		{
			thumbstickMotionMargin -= gameTime.ElapsedGameTime.Milliseconds;
		}
		if (!input.GetGamePadState().IsButtonDown(Buttons.RightStick))
		{
			rightStickHoldTime = 0;
		}
		base.Update(gameTime);
	}

	public void Instance_OnActivated(object sender, EventArgs args)
	{
		OnActivated(sender, args);
	}

	protected override void OnActivated(object sender, EventArgs args)
	{
		base.OnActivated(sender, args);
		_activatedTick = ticks + 1;
		input.IgnoreKeys(GetKeyboardState().GetPressedKeys());
	}

	public bool HasKeyboardFocus()
	{
		if (keyboardFocusInstance == null)
		{
			return base.IsMainInstance;
		}
		return keyboardFocusInstance == this;
	}

	/// <summary>
	/// Allows the game to run logic such as updating the world,
	/// checking for collisions, gathering input, and playing audio.
	/// </summary>
	/// <param name="gameTime">Provides a snapshot of timing values.</param>
	private void _update(GameTime gameTime)
	{
		if (graphics.GraphicsDevice == null)
		{
			return;
		}
		bool zoom_dirty = false;
		gameModeTicks++;
		if (options != null && !takingMapScreenshot)
		{
			if (options.baseUIScale != options.desiredUIScale)
			{
				if (options.desiredUIScale < 0f)
				{
					options.desiredUIScale = options.desiredBaseZoomLevel;
				}
				options.baseUIScale = options.desiredUIScale;
				zoom_dirty = true;
			}
			if (options.desiredBaseZoomLevel != options.baseZoomLevel)
			{
				options.baseZoomLevel = options.desiredBaseZoomLevel;
				forceSnapOnNextViewportUpdate = true;
				zoom_dirty = true;
			}
		}
		if (zoom_dirty)
		{
			refreshWindowSettings();
		}
		CheckGamepadMode();
		FarmAnimal.NumPathfindingThisTick = 0;
		options.reApplySetOptions();
		if (toggleFullScreen)
		{
			toggleFullscreen();
			toggleFullScreen = false;
		}
		input.Update();
		if (frameByFrame)
		{
			if (GetKeyboardState().IsKeyDown(Keys.Escape) && oldKBState.IsKeyUp(Keys.Escape))
			{
				frameByFrame = false;
			}
			bool advanceFrame = false;
			if (GetKeyboardState().IsKeyDown(Keys.G) && oldKBState.IsKeyUp(Keys.G))
			{
				advanceFrame = true;
			}
			if (!advanceFrame)
			{
				oldKBState = GetKeyboardState();
				return;
			}
		}
		if (client != null && client.timedOut)
		{
			multiplayer.clientRemotelyDisconnected(client.pendingDisconnect);
		}
		if (_newDayTask != null)
		{
			if (_newDayTask.Status == TaskStatus.Created)
			{
				hooks.StartTask(_newDayTask, "NewDay");
			}
			if (_newDayTask.Status >= TaskStatus.RanToCompletion)
			{
				if (_newDayTask.IsFaulted)
				{
					Exception e = _newDayTask.Exception.GetBaseException();
					if (!IsMasterGame)
					{
						if (e is AbortNetSynchronizerException)
						{
							log.Verbose("_newDayTask failed: client lost connection to the server");
						}
						else
						{
							log.Error("Client _newDayTask failed with an exception:", e);
						}
						multiplayer.clientRemotelyDisconnected(Multiplayer.DisconnectType.ClientTimeout);
						_newDayTask = null;
						Utility.CollectGarbage();
						return;
					}
					log.Error("_newDayTask failed with an exception:", e);
					throw new Exception($"Error on new day: \n---------------\n{e}\n---------------\n");
				}
				_newDayTask = null;
				Utility.CollectGarbage();
			}
			UpdateChatBox();
			return;
		}
		if (isLocalMultiplayerNewDayActive)
		{
			UpdateChatBox();
			return;
		}
		if (IsSaving)
		{
			PushUIMode();
			activeClickableMenu?.update(gameTime);
			if (overlayMenu != null)
			{
				overlayMenu.update(gameTime);
				if (overlayMenu == null)
				{
					PopUIMode();
					return;
				}
			}
			PopUIMode();
			UpdateChatBox();
			return;
		}
		if (exitToTitle)
		{
			exitToTitle = false;
			CleanupReturningToTitle();
			Utility.CollectGarbage();
			postExitToTitleCallback?.Invoke();
		}
		SetFreeCursorElapsed((float)gameTime.ElapsedGameTime.TotalSeconds);
		Program.sdk.Update();
		if (game1.IsMainInstance)
		{
			keyboardFocusInstance = game1;
			foreach (Game1 instance in GameRunner.instance.gameInstances)
			{
				if (instance.instanceKeyboardDispatcher.Subscriber != null && instance.instanceTextEntry != null)
				{
					keyboardFocusInstance = instance;
					break;
				}
			}
		}
		if (base.IsMainInstance)
		{
			int current_display_index = base.Window.GetDisplayIndex();
			if (_lastUsedDisplay != -1 && _lastUsedDisplay != current_display_index)
			{
				StartupPreferences startupPreferences = new StartupPreferences();
				startupPreferences.loadPreferences(async: false, applyLanguage: false);
				startupPreferences.displayIndex = current_display_index;
				startupPreferences.savePreferences(async: false);
			}
			_lastUsedDisplay = current_display_index;
		}
		if (HasKeyboardFocus())
		{
			keyboardDispatcher.Poll();
		}
		else
		{
			keyboardDispatcher.Discard();
		}
		if (gameMode == 6)
		{
			multiplayer.UpdateLoading();
		}
		if (gameMode == 3)
		{
			multiplayer.UpdateEarly();
			if (player?.team != null)
			{
				player.team.Update();
			}
		}
		if ((paused || (!IsActiveNoOverlay && Program.releaseBuild)) && (options == null || options.pauseWhenOutOfFocus || paused) && multiplayerMode == 0)
		{
			UpdateChatBox();
			return;
		}
		if (quit)
		{
			Exit();
		}
		currentGameTime = gameTime;
		if (gameMode != 11)
		{
			ticks++;
			if (IsActiveNoOverlay)
			{
				checkForEscapeKeys();
			}
			updateMusic();
			updateRaindropPosition();
			if (globalFade)
			{
				screenFade.UpdateGlobalFade();
			}
			else if (pauseThenDoFunctionTimer > 0)
			{
				freezeControls = true;
				pauseThenDoFunctionTimer -= gameTime.ElapsedGameTime.Milliseconds;
				if (pauseThenDoFunctionTimer <= 0)
				{
					freezeControls = false;
					afterPause?.Invoke();
				}
			}
			bool should_clamp_cursor = false;
			if (options.gamepadControls && activeClickableMenu != null && activeClickableMenu.shouldClampGamePadCursor())
			{
				should_clamp_cursor = true;
			}
			if (should_clamp_cursor)
			{
				Point pos = getMousePositionRaw();
				Microsoft.Xna.Framework.Rectangle rect = new Microsoft.Xna.Framework.Rectangle(0, 0, localMultiplayerWindow.Width, localMultiplayerWindow.Height);
				if (pos.X < rect.X)
				{
					pos.X = rect.X;
				}
				else if (pos.X > rect.Right)
				{
					pos.X = rect.Right;
				}
				if (pos.Y < rect.Y)
				{
					pos.Y = rect.Y;
				}
				else if (pos.Y > rect.Bottom)
				{
					pos.Y = rect.Bottom;
				}
				setMousePositionRaw(pos.X, pos.Y);
			}
			if (gameMode == 3 || gameMode == 2)
			{
				if (!warpingForForcedRemoteEvent && !eventUp && !dialogueUp && remoteEventQueue.Count > 0 && player != null && player.isCustomized.Value && (!fadeIn || !(fadeToBlackAlpha > 0f)))
				{
					if (activeClickableMenu != null)
					{
						activeClickableMenu.emergencyShutDown();
						exitActiveMenu();
					}
					else if (currentMinigame != null && currentMinigame.forceQuit())
					{
						currentMinigame = null;
					}
					if (activeClickableMenu == null && currentMinigame == null && player.freezePause <= 0)
					{
						Action action = remoteEventQueue[0];
						remoteEventQueue.RemoveAt(0);
						action();
					}
				}
				player.millisecondsPlayed += (uint)gameTime.ElapsedGameTime.Milliseconds;
				bool doMainGameUpdates = true;
				if (currentMinigame != null && !HostPaused)
				{
					if (pauseTime > 0f)
					{
						updatePause(gameTime);
					}
					if (fadeToBlack)
					{
						screenFade.UpdateFadeAlpha(gameTime);
						if (fadeToBlackAlpha >= 1f)
						{
							fadeToBlack = false;
						}
					}
					else
					{
						if (thumbstickMotionMargin > 0)
						{
							thumbstickMotionMargin -= gameTime.ElapsedGameTime.Milliseconds;
						}
						KeyboardState currentKBState = default(KeyboardState);
						MouseState currentMouseState = default(MouseState);
						GamePadState currentPadState = default(GamePadState);
						if (base.IsActive)
						{
							currentKBState = GetKeyboardState();
							currentMouseState = input.GetMouseState();
							currentPadState = input.GetGamePadState();
							bool ignore_controls = false;
							if (chatBox != null && chatBox.isActive())
							{
								ignore_controls = true;
							}
							else if (textEntry != null)
							{
								ignore_controls = true;
							}
							if (ignore_controls)
							{
								currentKBState = default(KeyboardState);
								currentPadState = default(GamePadState);
							}
							else
							{
								Keys[] pressedKeys = currentKBState.GetPressedKeys();
								foreach (Keys k in pressedKeys)
								{
									if (!oldKBState.IsKeyDown(k) && currentMinigame != null)
									{
										currentMinigame.receiveKeyPress(k);
									}
								}
								if (options.gamepadControls)
								{
									if (currentMinigame == null)
									{
										oldMouseState = currentMouseState;
										oldKBState = currentKBState;
										oldPadState = currentPadState;
										UpdateChatBox();
										return;
									}
									ButtonCollection.ButtonEnumerator enumerator2 = Utility.getPressedButtons(currentPadState, oldPadState).GetEnumerator();
									while (enumerator2.MoveNext())
									{
										Buttons b = enumerator2.Current;
										currentMinigame?.receiveKeyPress(Utility.mapGamePadButtonToKey(b));
									}
									if (currentMinigame == null)
									{
										oldMouseState = currentMouseState;
										oldKBState = currentKBState;
										oldPadState = currentPadState;
										UpdateChatBox();
										return;
									}
									if (currentPadState.ThumbSticks.Right.Y < -0.2f && oldPadState.ThumbSticks.Right.Y >= -0.2f)
									{
										currentMinigame.receiveKeyPress(Keys.Down);
									}
									if (currentPadState.ThumbSticks.Right.Y > 0.2f && oldPadState.ThumbSticks.Right.Y <= 0.2f)
									{
										currentMinigame.receiveKeyPress(Keys.Up);
									}
									if (currentPadState.ThumbSticks.Right.X < -0.2f && oldPadState.ThumbSticks.Right.X >= -0.2f)
									{
										currentMinigame.receiveKeyPress(Keys.Left);
									}
									if (currentPadState.ThumbSticks.Right.X > 0.2f && oldPadState.ThumbSticks.Right.X <= 0.2f)
									{
										currentMinigame.receiveKeyPress(Keys.Right);
									}
									if (oldPadState.ThumbSticks.Right.Y < -0.2f && currentPadState.ThumbSticks.Right.Y >= -0.2f)
									{
										currentMinigame.receiveKeyRelease(Keys.Down);
									}
									if (oldPadState.ThumbSticks.Right.Y > 0.2f && currentPadState.ThumbSticks.Right.Y <= 0.2f)
									{
										currentMinigame.receiveKeyRelease(Keys.Up);
									}
									if (oldPadState.ThumbSticks.Right.X < -0.2f && currentPadState.ThumbSticks.Right.X >= -0.2f)
									{
										currentMinigame.receiveKeyRelease(Keys.Left);
									}
									if (oldPadState.ThumbSticks.Right.X > 0.2f && currentPadState.ThumbSticks.Right.X <= 0.2f)
									{
										currentMinigame.receiveKeyRelease(Keys.Right);
									}
									if (isGamePadThumbstickInMotion() && currentMinigame != null && !currentMinigame.overrideFreeMouseMovement())
									{
										setMousePosition(getMouseX() + (int)(currentPadState.ThumbSticks.Left.X * thumbstickToMouseModifier), getMouseY() - (int)(currentPadState.ThumbSticks.Left.Y * thumbstickToMouseModifier));
									}
									else if (getMouseX() != getOldMouseX() || getMouseY() != getOldMouseY())
									{
										lastCursorMotionWasMouse = true;
									}
								}
								pressedKeys = oldKBState.GetPressedKeys();
								foreach (Keys k in pressedKeys)
								{
									if (!currentKBState.IsKeyDown(k) && currentMinigame != null)
									{
										currentMinigame.receiveKeyRelease(k);
									}
								}
								if (options.gamepadControls)
								{
									if (currentMinigame == null)
									{
										oldMouseState = currentMouseState;
										oldKBState = currentKBState;
										oldPadState = currentPadState;
										UpdateChatBox();
										return;
									}
									if (currentPadState.IsConnected)
									{
										if (currentPadState.IsButtonDown(Buttons.X) && !oldPadState.IsButtonDown(Buttons.X))
										{
											currentMinigame.receiveRightClick(getMouseX(), getMouseY());
										}
										else if (currentPadState.IsButtonDown(Buttons.A) && !oldPadState.IsButtonDown(Buttons.A))
										{
											currentMinigame.receiveLeftClick(getMouseX(), getMouseY());
										}
										else if (!currentPadState.IsButtonDown(Buttons.X) && oldPadState.IsButtonDown(Buttons.X))
										{
											currentMinigame.releaseRightClick(getMouseX(), getMouseY());
										}
										else if (!currentPadState.IsButtonDown(Buttons.A) && oldPadState.IsButtonDown(Buttons.A))
										{
											currentMinigame.releaseLeftClick(getMouseX(), getMouseY());
										}
									}
									ButtonCollection.ButtonEnumerator enumerator2 = Utility.getPressedButtons(oldPadState, currentPadState).GetEnumerator();
									while (enumerator2.MoveNext())
									{
										Buttons b = enumerator2.Current;
										currentMinigame?.receiveKeyRelease(Utility.mapGamePadButtonToKey(b));
									}
									if (currentPadState.IsConnected && currentPadState.IsButtonDown(Buttons.A) && currentMinigame != null)
									{
										currentMinigame.leftClickHeld(0, 0);
									}
								}
								if (currentMinigame == null)
								{
									oldMouseState = currentMouseState;
									oldKBState = currentKBState;
									oldPadState = currentPadState;
									UpdateChatBox();
									return;
								}
								if (currentMinigame != null && currentMouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton != ButtonState.Pressed)
								{
									currentMinigame.receiveLeftClick(getMouseX(), getMouseY());
								}
								if (currentMinigame != null && currentMouseState.RightButton == ButtonState.Pressed && oldMouseState.RightButton != ButtonState.Pressed)
								{
									currentMinigame.receiveRightClick(getMouseX(), getMouseY());
								}
								if (currentMinigame != null && currentMouseState.LeftButton == ButtonState.Released && oldMouseState.LeftButton == ButtonState.Pressed)
								{
									currentMinigame.releaseLeftClick(getMouseX(), getMouseY());
								}
								if (currentMinigame != null && currentMouseState.RightButton == ButtonState.Released && oldMouseState.RightButton == ButtonState.Pressed)
								{
									currentMinigame.releaseLeftClick(getMouseX(), getMouseY());
								}
								if (currentMinigame != null && currentMouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Pressed)
								{
									currentMinigame.leftClickHeld(getMouseX(), getMouseY());
								}
							}
						}
						if (currentMinigame != null && currentMinigame.tick(gameTime))
						{
							oldMouseState = currentMouseState;
							oldKBState = currentKBState;
							oldPadState = currentPadState;
							currentMinigame?.unload();
							currentMinigame = null;
							fadeIn = true;
							fadeToBlackAlpha = 1f;
							UpdateChatBox();
							return;
						}
						if (currentMinigame == null && IsMusicContextActive(MusicContext.MiniGame))
						{
							stopMusicTrack(MusicContext.MiniGame);
						}
						oldMouseState = currentMouseState;
						oldKBState = currentKBState;
						oldPadState = currentPadState;
					}
					doMainGameUpdates = IsMultiplayer || currentMinigame == null || currentMinigame.doMainGameUpdates();
				}
				else if (farmEvent != null && !HostPaused && farmEvent.tickUpdate(gameTime))
				{
					farmEvent.makeChangesToLocation();
					timeOfDay = 600;
					outdoorLight = Color.White;
					displayHUD = true;
					farmEvent = null;
					netWorldState.Value.WriteToGame1();
					currentLocation = player.currentLocation;
					LocationRequest obj = getLocationRequest(currentLocation.Name);
					obj.OnWarp += delegate
					{
						if (currentLocation is FarmHouse farmHouse)
						{
							player.Position = Utility.PointToVector2(farmHouse.GetPlayerBedSpot()) * 64f;
							BedFurniture.ShiftPositionForBed(player);
						}
						else
						{
							BedFurniture.ApplyWakeUpPosition(player);
						}
						if (player.IsSitting())
						{
							player.StopSitting(animate: false);
						}
						changeMusicTrack("none", track_interruptable: true);
						player.forceCanMove();
						freezeControls = false;
						displayFarmer = true;
						viewportFreeze = false;
						fadeToBlackAlpha = 0f;
						fadeToBlack = false;
						globalFadeToClear();
						RemoveDeliveredMailForTomorrow();
						handlePostFarmEventActions();
						showEndOfNightStuff();
					};
					warpFarmer(obj, 5, 9, player.FacingDirection);
					fadeToBlackAlpha = 1.1f;
					fadeToBlack = true;
					nonWarpFade = false;
					UpdateOther(gameTime);
				}
				if (doMainGameUpdates)
				{
					if (endOfNightMenus.Count > 0 && activeClickableMenu == null)
					{
						activeClickableMenu = endOfNightMenus.Pop();
						if (activeClickableMenu != null && options.SnappyMenus)
						{
							activeClickableMenu.snapToDefaultClickableComponent();
						}
					}
					specialCurrencyDisplay?.Update(gameTime);
					if (currentLocation != null && currentMinigame == null)
					{
						if (emoteMenu != null)
						{
							emoteMenu.update(gameTime);
							if (emoteMenu != null)
							{
								PushUIMode();
								emoteMenu.performHoverAction(getMouseX(), getMouseY());
								KeyboardState currentState = GetKeyboardState();
								if (input.GetMouseState().LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released)
								{
									emoteMenu.receiveLeftClick(getMouseX(), getMouseY());
								}
								else if (input.GetMouseState().RightButton == ButtonState.Pressed && oldMouseState.RightButton == ButtonState.Released)
								{
									emoteMenu.receiveRightClick(getMouseX(), getMouseY());
								}
								else if (isOneOfTheseKeysDown(currentState, options.menuButton) || (isOneOfTheseKeysDown(currentState, options.emoteButton) && areAllOfTheseKeysUp(oldKBState, options.emoteButton)))
								{
									emoteMenu.exitThisMenu(playSound: false);
								}
								PopUIMode();
								oldKBState = currentState;
								oldMouseState = input.GetMouseState();
							}
						}
						else if (textEntry != null)
						{
							PushUIMode();
							updateTextEntry(gameTime);
							PopUIMode();
						}
						else if (activeClickableMenu != null)
						{
							PushUIMode();
							updateActiveMenu(gameTime);
							PopUIMode();
						}
						else
						{
							if (pauseTime > 0f)
							{
								updatePause(gameTime);
							}
							if (!globalFade && !freezeControls && activeClickableMenu == null && (IsActiveNoOverlay || inputSimulator != null))
							{
								UpdateControlInput(gameTime);
							}
						}
					}
					if (showingEndOfNightStuff && endOfNightMenus.Count == 0 && activeClickableMenu == null)
					{
						newDaySync.destroy();
						player.team.endOfNightStatus.WithdrawState();
						showingEndOfNightStuff = false;
						Action afterAction = _afterNewDayAction;
						if (afterAction != null)
						{
							_afterNewDayAction = null;
							afterAction();
						}
						player.ReequipEnchantments();
						globalFadeToClear(doMorningStuff);
					}
					if (currentLocation != null)
					{
						if (!HostPaused && !showingEndOfNightStuff)
						{
							if (IsMultiplayer || (activeClickableMenu == null && currentMinigame == null) || player.viewingLocation.Value != null)
							{
								UpdateGameClock(gameTime);
							}
							UpdateCharacters(gameTime);
							UpdateLocations(gameTime);
							if (currentMinigame == null)
							{
								UpdateViewPort(overrideFreeze: false, getViewportCenter());
							}
							else
							{
								previousViewportPosition.X = viewport.X;
								previousViewportPosition.Y = viewport.Y;
							}
							UpdateOther(gameTime);
						}
						if (messagePause)
						{
							KeyboardState tmp = GetKeyboardState();
							MouseState tmp2 = input.GetMouseState();
							GamePadState tmp3 = input.GetGamePadState();
							if (isOneOfTheseKeysDown(tmp, options.actionButton) && !isOneOfTheseKeysDown(oldKBState, options.actionButton))
							{
								pressActionButton(tmp, tmp2, tmp3);
							}
							oldKBState = tmp;
							oldPadState = tmp3;
						}
					}
				}
				else if (textEntry != null)
				{
					PushUIMode();
					updateTextEntry(gameTime);
					PopUIMode();
				}
			}
			else
			{
				UpdateTitleScreen(gameTime);
				if (textEntry != null)
				{
					PushUIMode();
					updateTextEntry(gameTime);
					PopUIMode();
				}
				else if (activeClickableMenu != null)
				{
					PushUIMode();
					updateActiveMenu(gameTime);
					PopUIMode();
				}
				if (gameMode == 10)
				{
					UpdateOther(gameTime);
				}
			}
			audioEngine?.Update();
			UpdateChatBox();
			if (gameMode != 6)
			{
				multiplayer.UpdateLate();
			}
		}
		if (gameMode == 3 && gameModeTicks == 1)
		{
			OnDayStarted();
		}
	}

	/// <summary>Handle the new day starting after the player saves, loads, or connects.</summary>
	public static void OnDayStarted()
	{
		TriggerActionManager.Raise("DayStarted");
		Utility.ForEachLocation(delegate(GameLocation location)
		{
			location.OnDayStarted();
			return true;
		});
		Utility.fixAllAnimals();
		foreach (NPC allCharacter in Utility.getAllCharacters())
		{
			allCharacter.OnDayStarted();
		}
		Utility.ForEachLocation(delegate(GameLocation location)
		{
			foreach (FarmAnimal value in location.animals.Values)
			{
				value.OnDayStarted();
			}
			return true;
		});
		player.currentLocation.resetForPlayerEntry();
		if (hasStartedDay)
		{
			return;
		}
		foreach (string buildingType in player.team.constructedBuildings)
		{
			player.checkForQuestComplete(null, -1, -1, null, buildingType, 8);
		}
		foreach (int achievement in player.achievements)
		{
			getPlatformAchievement(achievement.ToString());
		}
		hasStartedDay = true;
	}

	public static void PerformPassiveFestivalSetup()
	{
		foreach (string festival_id in netWorldState.Value.ActivePassiveFestivals)
		{
			if (Utility.TryGetPassiveFestivalData(festival_id, out var data) && data.DailySetupMethod != null)
			{
				if (StaticDelegateBuilder.TryCreateDelegate<FestivalDailySetupDelegate>(data.DailySetupMethod, out var method, out var error))
				{
					method();
					continue;
				}
				log.Warn($"Passive festival '{festival_id}' has invalid daily setup method '{data.DailySetupMethod}': {error}");
			}
		}
	}

	public static void showTextEntry(TextBox text_box)
	{
		timerUntilMouseFade = 0;
		PushUIMode();
		textEntry = new TextEntryMenu(text_box);
		PopUIMode();
	}

	public static void closeTextEntry()
	{
		if (textEntry != null)
		{
			textEntry = null;
		}
		if (activeClickableMenu != null && options.SnappyMenus)
		{
			if (activeClickableMenu is TitleMenu && TitleMenu.subMenu != null)
			{
				TitleMenu.subMenu.snapCursorToCurrentSnappedComponent();
			}
			else
			{
				activeClickableMenu.snapCursorToCurrentSnappedComponent();
			}
		}
	}

	public static bool isDarkOut(GameLocation location)
	{
		return timeOfDay >= getTrulyDarkTime(location);
	}

	public static bool isTimeToTurnOffLighting(GameLocation location)
	{
		return timeOfDay >= getTrulyDarkTime(location) - 100;
	}

	public static bool isStartingToGetDarkOut(GameLocation location)
	{
		return timeOfDay >= getStartingToGetDarkTime(location);
	}

	public static int getStartingToGetDarkTime(GameLocation location)
	{
		if (location != null && location.InIslandContext())
		{
			return 1800;
		}
		return season switch
		{
			Season.Fall => 1700, 
			Season.Winter => 1500, 
			_ => 1800, 
		};
	}

	public static void updateCellarAssignments()
	{
		if (!IsMasterGame)
		{
			return;
		}
		player.team.cellarAssignments[1] = MasterPlayer.UniqueMultiplayerID;
		for (int i = 2; i <= netWorldState.Value.HighestPlayerLimit; i++)
		{
			string cellar_name = "Cellar" + i;
			if (i == 1 || getLocationFromName(cellar_name) == null)
			{
				continue;
			}
			if (player.team.cellarAssignments.TryGetValue(i, out var assignedFarmerId))
			{
				if (getFarmerMaybeOffline(assignedFarmerId) != null)
				{
					continue;
				}
				player.team.cellarAssignments.Remove(i);
			}
			foreach (Farmer farmer in getAllFarmers())
			{
				if (!player.team.cellarAssignments.Values.Contains(farmer.UniqueMultiplayerID))
				{
					player.team.cellarAssignments[i] = farmer.UniqueMultiplayerID;
					break;
				}
			}
		}
	}

	public static int getModeratelyDarkTime(GameLocation location)
	{
		return (getTrulyDarkTime(location) + getStartingToGetDarkTime(location)) / 2;
	}

	public static int getTrulyDarkTime(GameLocation location)
	{
		return getStartingToGetDarkTime(location) + 200;
	}

	public static void playMorningSong(bool ignoreDelay = false)
	{
		LocationContextData context;
		if (!eventUp && dayOfMonth > 0)
		{
			LocationData data = currentLocation.GetData();
			if (currentLocation.GetLocationSpecificMusic() != null && (data == null || !data.MusicIsTownTheme))
			{
				changeMusicTrack("none", track_interruptable: true);
				GameLocation.HandleMusicChange(null, currentLocation);
				return;
			}
			if (IsRainingHere())
			{
				if (ignoreDelay)
				{
					PlayAction();
				}
				else
				{
					morningSongPlayAction = DelayedAction.functionAfterDelay(PlayAction, 500);
				}
				return;
			}
			context = currentLocation?.GetLocationContext();
			if (context?.DefaultMusic != null)
			{
				if (context.DefaultMusicCondition == null || GameStateQuery.CheckConditions(context.DefaultMusicCondition))
				{
					if (ignoreDelay)
					{
						PlayAction();
					}
					else
					{
						morningSongPlayAction = DelayedAction.functionAfterDelay(PlayAction, 500);
					}
				}
			}
			else if (ignoreDelay)
			{
				PlayAction();
			}
			else
			{
				morningSongPlayAction = DelayedAction.functionAfterDelay(PlayAction, 500);
			}
		}
		else if (getMusicTrackName() == "silence")
		{
			changeMusicTrack("none", track_interruptable: true);
		}
		static void PlayAction()
		{
			changeMusicTrack("rain", track_interruptable: true);
		}
		void PlayAction()
		{
			if (currentLocation == null)
			{
				changeMusicTrack("none", track_interruptable: true);
			}
			else
			{
				changeMusicTrack(context.DefaultMusic, track_interruptable: true);
				IsPlayingBackgroundMusic = true;
			}
		}
		static void PlayAction()
		{
			changeMusicTrack(currentLocation.GetMorningSong(), track_interruptable: true);
			IsPlayingBackgroundMusic = true;
			IsPlayingMorningSong = true;
		}
	}

	public static void doMorningStuff()
	{
		playMorningSong();
		DelayedAction.functionAfterDelay(delegate
		{
			while (morningQueue.Count > 0)
			{
				morningQueue.Dequeue()();
			}
		}, 1000);
		if (player.hasPendingCompletedQuests)
		{
			dayTimeMoneyBox.PingQuestLog();
		}
	}

	/// <summary>Add an action that will be called one second after fully waking up in the morning. This won't be saved, so it should only be used for "fluff" functions like sending multiplayer chat messages, etc.</summary>
	/// <param name="action">The action to perform.</param>
	public static void addMorningFluffFunction(Action action)
	{
		morningQueue.Enqueue(action);
	}

	private Point getViewportCenter()
	{
		if (viewportTarget.X != -2.1474836E+09f)
		{
			if (!(Math.Abs((float)viewportCenter.X - viewportTarget.X) <= viewportSpeed) || !(Math.Abs((float)viewportCenter.Y - viewportTarget.Y) <= viewportSpeed))
			{
				Vector2 velocity = Utility.getVelocityTowardPoint(viewportCenter, viewportTarget, viewportSpeed);
				viewportCenter.X += (int)Math.Round(velocity.X);
				viewportCenter.Y += (int)Math.Round(velocity.Y);
			}
			else
			{
				if (viewportReachedTarget != null)
				{
					viewportReachedTarget();
					viewportReachedTarget = null;
				}
				viewportHold -= currentGameTime.ElapsedGameTime.Milliseconds;
				if (viewportHold <= 0)
				{
					viewportTarget = new Vector2(-2.1474836E+09f, -2.1474836E+09f);
					afterViewport?.Invoke();
				}
			}
		}
		else
		{
			viewportCenter = getPlayerOrEventFarmer().StandingPixel;
		}
		return viewportCenter;
	}

	public static void afterFadeReturnViewportToPlayer()
	{
		viewportTarget = new Vector2(-2.1474836E+09f, -2.1474836E+09f);
		viewportHold = 0;
		viewportFreeze = false;
		viewportCenter = player.StandingPixel;
		globalFadeToClear();
	}

	public static bool isViewportOnCustomPath()
	{
		return viewportTarget.X != -2.1474836E+09f;
	}

	public static void moveViewportTo(Vector2 target, float speed, int holdTimer = 0, afterFadeFunction reachedTarget = null, afterFadeFunction endFunction = null)
	{
		viewportTarget = target;
		viewportSpeed = speed;
		viewportHold = holdTimer;
		afterViewport = endFunction;
		viewportReachedTarget = reachedTarget;
	}

	public static Farm getFarm()
	{
		return RequireLocation<Farm>("Farm");
	}

	public static void setMousePosition(int x, int y, bool ui_scale)
	{
		if (ui_scale)
		{
			setMousePositionRaw((int)((float)x * options.uiScale), (int)((float)y * options.uiScale));
		}
		else
		{
			setMousePositionRaw((int)((float)x * options.zoomLevel), (int)((float)y * options.zoomLevel));
		}
	}

	public static void setMousePosition(int x, int y)
	{
		setMousePosition(x, y, uiMode);
	}

	public static void setMousePosition(Point position, bool ui_scale)
	{
		setMousePosition(position.X, position.Y, ui_scale);
	}

	public static void setMousePosition(Point position)
	{
		setMousePosition(position, uiMode);
	}

	public static void setMousePositionRaw(int x, int y)
	{
		input.SetMousePosition(x, y);
		InvalidateOldMouseMovement();
		lastCursorMotionWasMouse = false;
	}

	public static Point getMousePositionRaw()
	{
		return new Point(getMouseXRaw(), getMouseYRaw());
	}

	public static Point getMousePosition(bool ui_scale)
	{
		return new Point(getMouseX(ui_scale), getMouseY(ui_scale));
	}

	public static Point getMousePosition()
	{
		return getMousePosition(uiMode);
	}

	private static void ComputeCursorSpeed()
	{
		_cursorSpeedDirty = false;
		GamePadState p = input.GetGamePadState();
		float accellTol = 0.9f;
		bool isAccell = false;
		float num = p.ThumbSticks.Left.Length();
		float rlen = p.ThumbSticks.Right.Length();
		if (num > accellTol || rlen > accellTol)
		{
			isAccell = true;
		}
		float min = 0.7f;
		float max = 2f;
		float rate = 1f;
		if (_cursorDragEnabled)
		{
			min = 0.5f;
			max = 2f;
			rate = 1f;
		}
		if (!isAccell)
		{
			rate = -5f;
		}
		if (_cursorDragPrevEnabled != _cursorDragEnabled)
		{
			_cursorSpeedScale *= 0.5f;
		}
		_cursorDragPrevEnabled = _cursorDragEnabled;
		_cursorSpeedScale += _cursorUpdateElapsedSec * rate;
		_cursorSpeedScale = MathHelper.Clamp(_cursorSpeedScale, min, max);
		float num2 = 16f / (float)game1.TargetElapsedTime.TotalSeconds * _cursorSpeedScale;
		float deltaSpeed = num2 - _cursorSpeed;
		_cursorSpeed = num2;
		_cursorUpdateElapsedSec = 0f;
		if (debugMode)
		{
			log.Verbose("_cursorSpeed=" + _cursorSpeed.ToString("0.0") + ", _cursorSpeedScale=" + _cursorSpeedScale.ToString("0.0") + ", deltaSpeed=" + deltaSpeed.ToString("0.0"));
		}
	}

	private static void SetFreeCursorElapsed(float elapsedSec)
	{
		if (elapsedSec != _cursorUpdateElapsedSec)
		{
			_cursorUpdateElapsedSec = elapsedSec;
			_cursorSpeedDirty = true;
		}
	}

	public static void ResetFreeCursorDrag()
	{
		if (_cursorDragEnabled)
		{
			_cursorSpeedDirty = true;
		}
		_cursorDragEnabled = false;
	}

	public static void SetFreeCursorDrag()
	{
		if (!_cursorDragEnabled)
		{
			_cursorSpeedDirty = true;
		}
		_cursorDragEnabled = true;
	}

	public static void updateActiveMenu(GameTime gameTime)
	{
		IClickableMenu active_menu = activeClickableMenu;
		while (active_menu.GetChildMenu() != null)
		{
			active_menu = active_menu.GetChildMenu();
		}
		if (!Program.gamePtr.IsActiveNoOverlay && Program.releaseBuild)
		{
			if (active_menu != null && active_menu.IsActive())
			{
				active_menu.update(gameTime);
			}
			return;
		}
		MouseState mouseState = input.GetMouseState();
		KeyboardState keyState = GetKeyboardState();
		GamePadState padState = input.GetGamePadState();
		if (CurrentEvent != null)
		{
			if ((mouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released) || (options.gamepadControls && padState.IsButtonDown(Buttons.A) && oldPadState.IsButtonUp(Buttons.A)))
			{
				CurrentEvent.receiveMouseClick(getMouseX(), getMouseY());
			}
			else if (options.gamepadControls && padState.IsButtonDown(Buttons.Back) && oldPadState.IsButtonUp(Buttons.Back) && !CurrentEvent.skipped && CurrentEvent.skippable)
			{
				CurrentEvent.skipped = true;
				CurrentEvent.skipEvent();
				freezeControls = false;
			}
			if (CurrentEvent != null && CurrentEvent.skipped)
			{
				oldMouseState = input.GetMouseState();
				oldKBState = keyState;
				oldPadState = padState;
				return;
			}
		}
		if (options.gamepadControls && active_menu != null && active_menu.IsActive())
		{
			if (isGamePadThumbstickInMotion() && (!options.snappyMenus || active_menu.overrideSnappyMenuCursorMovementBan()))
			{
				setMousePositionRaw((int)((float)mouseState.X + padState.ThumbSticks.Left.X * thumbstickToMouseModifier), (int)((float)mouseState.Y - padState.ThumbSticks.Left.Y * thumbstickToMouseModifier));
			}
			if (active_menu != null && active_menu.IsActive() && (chatBox == null || !chatBox.isActive()))
			{
				ButtonCollection.ButtonEnumerator enumerator = Utility.getPressedButtons(padState, oldPadState).GetEnumerator();
				while (enumerator.MoveNext())
				{
					Buttons b = enumerator.Current;
					active_menu.receiveGamePadButton(b);
					if (active_menu == null || !active_menu.IsActive())
					{
						break;
					}
				}
				enumerator = Utility.getHeldButtons(padState).GetEnumerator();
				while (enumerator.MoveNext())
				{
					Buttons b = enumerator.Current;
					if (active_menu != null && active_menu.IsActive())
					{
						active_menu.gamePadButtonHeld(b);
					}
					if (active_menu == null || !active_menu.IsActive())
					{
						break;
					}
				}
			}
		}
		if ((getMouseX() != getOldMouseX() || getMouseY() != getOldMouseY()) && !isGamePadThumbstickInMotion() && !isDPadPressed())
		{
			lastCursorMotionWasMouse = true;
		}
		ResetFreeCursorDrag();
		if (active_menu != null && active_menu.IsActive())
		{
			active_menu.performHoverAction(getMouseX(), getMouseY());
		}
		if (active_menu != null && active_menu.IsActive())
		{
			active_menu.update(gameTime);
		}
		if (active_menu != null && active_menu.IsActive() && mouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released)
		{
			if (chatBox != null && chatBox.isActive() && chatBox.isWithinBounds(getMouseX(), getMouseY()))
			{
				chatBox.receiveLeftClick(getMouseX(), getMouseY());
			}
			else
			{
				active_menu.receiveLeftClick(getMouseX(), getMouseY());
			}
		}
		else if (active_menu != null && active_menu.IsActive() && mouseState.RightButton == ButtonState.Pressed && (oldMouseState.RightButton == ButtonState.Released || ((float)mouseClickPolling > 650f && !(active_menu is DialogueBox))))
		{
			active_menu.receiveRightClick(getMouseX(), getMouseY());
			if ((float)mouseClickPolling > 650f)
			{
				mouseClickPolling = 600;
			}
			if ((active_menu == null || !active_menu.IsActive()) && activeClickableMenu == null)
			{
				rightClickPolling = 500;
				mouseClickPolling = 0;
			}
		}
		if (mouseState.ScrollWheelValue != oldMouseState.ScrollWheelValue && active_menu != null && active_menu.IsActive())
		{
			if (chatBox != null && chatBox.choosingEmoji && chatBox.emojiMenu.isWithinBounds(getOldMouseX(), getOldMouseY()))
			{
				chatBox.receiveScrollWheelAction(mouseState.ScrollWheelValue - oldMouseState.ScrollWheelValue);
			}
			else
			{
				active_menu.receiveScrollWheelAction(mouseState.ScrollWheelValue - oldMouseState.ScrollWheelValue);
			}
		}
		if (options.gamepadControls && active_menu != null && active_menu.IsActive())
		{
			thumbstickPollingTimer -= currentGameTime.ElapsedGameTime.Milliseconds;
			if (thumbstickPollingTimer <= 0)
			{
				if (padState.ThumbSticks.Right.Y > 0.2f)
				{
					active_menu.receiveScrollWheelAction(1);
				}
				else if (padState.ThumbSticks.Right.Y < -0.2f)
				{
					active_menu.receiveScrollWheelAction(-1);
				}
			}
			if (thumbstickPollingTimer <= 0)
			{
				thumbstickPollingTimer = 220 - (int)(Math.Abs(padState.ThumbSticks.Right.Y) * 170f);
			}
			if (Math.Abs(padState.ThumbSticks.Right.Y) < 0.2f)
			{
				thumbstickPollingTimer = 0;
			}
		}
		if (active_menu != null && active_menu.IsActive() && mouseState.LeftButton == ButtonState.Released && oldMouseState.LeftButton == ButtonState.Pressed)
		{
			active_menu.releaseLeftClick(getMouseX(), getMouseY());
		}
		else if (active_menu != null && active_menu.IsActive() && mouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Pressed)
		{
			active_menu.leftClickHeld(getMouseX(), getMouseY());
		}
		Keys[] pressedKeys = keyState.GetPressedKeys();
		foreach (Keys k in pressedKeys)
		{
			if (active_menu != null && active_menu.IsActive() && !oldKBState.GetPressedKeys().Contains(k))
			{
				active_menu.receiveKeyPress(k);
			}
		}
		if (chatBox == null || !chatBox.isActive())
		{
			if (isOneOfTheseKeysDown(oldKBState, options.moveUpButton) || (options.snappyMenus && options.gamepadControls && (Math.Abs(padState.ThumbSticks.Left.X) < padState.ThumbSticks.Left.Y || padState.IsButtonDown(Buttons.DPadUp))))
			{
				directionKeyPolling[0] -= currentGameTime.ElapsedGameTime.Milliseconds;
			}
			else if (isOneOfTheseKeysDown(oldKBState, options.moveRightButton) || (options.snappyMenus && options.gamepadControls && (padState.ThumbSticks.Left.X > Math.Abs(padState.ThumbSticks.Left.Y) || padState.IsButtonDown(Buttons.DPadRight))))
			{
				directionKeyPolling[1] -= currentGameTime.ElapsedGameTime.Milliseconds;
			}
			else if (isOneOfTheseKeysDown(oldKBState, options.moveDownButton) || (options.snappyMenus && options.gamepadControls && (Math.Abs(padState.ThumbSticks.Left.X) < Math.Abs(padState.ThumbSticks.Left.Y) || padState.IsButtonDown(Buttons.DPadDown))))
			{
				directionKeyPolling[2] -= currentGameTime.ElapsedGameTime.Milliseconds;
			}
			else if (isOneOfTheseKeysDown(oldKBState, options.moveLeftButton) || (options.snappyMenus && options.gamepadControls && (Math.Abs(padState.ThumbSticks.Left.X) > Math.Abs(padState.ThumbSticks.Left.Y) || padState.IsButtonDown(Buttons.DPadLeft))))
			{
				directionKeyPolling[3] -= currentGameTime.ElapsedGameTime.Milliseconds;
			}
			if (areAllOfTheseKeysUp(oldKBState, options.moveUpButton) && (!options.snappyMenus || !options.gamepadControls || ((double)padState.ThumbSticks.Left.Y < 0.1 && padState.IsButtonUp(Buttons.DPadUp))))
			{
				directionKeyPolling[0] = 250;
			}
			if (areAllOfTheseKeysUp(oldKBState, options.moveRightButton) && (!options.snappyMenus || !options.gamepadControls || ((double)padState.ThumbSticks.Left.X < 0.1 && padState.IsButtonUp(Buttons.DPadRight))))
			{
				directionKeyPolling[1] = 250;
			}
			if (areAllOfTheseKeysUp(oldKBState, options.moveDownButton) && (!options.snappyMenus || !options.gamepadControls || ((double)padState.ThumbSticks.Left.Y > -0.1 && padState.IsButtonUp(Buttons.DPadDown))))
			{
				directionKeyPolling[2] = 250;
			}
			if (areAllOfTheseKeysUp(oldKBState, options.moveLeftButton) && (!options.snappyMenus || !options.gamepadControls || ((double)padState.ThumbSticks.Left.X > -0.1 && padState.IsButtonUp(Buttons.DPadLeft))))
			{
				directionKeyPolling[3] = 250;
			}
			if (directionKeyPolling[0] <= 0 && active_menu != null && active_menu.IsActive())
			{
				active_menu.receiveKeyPress(options.getFirstKeyboardKeyFromInputButtonList(options.moveUpButton));
				directionKeyPolling[0] = 70;
			}
			if (directionKeyPolling[1] <= 0 && active_menu != null && active_menu.IsActive())
			{
				active_menu.receiveKeyPress(options.getFirstKeyboardKeyFromInputButtonList(options.moveRightButton));
				directionKeyPolling[1] = 70;
			}
			if (directionKeyPolling[2] <= 0 && active_menu != null && active_menu.IsActive())
			{
				active_menu.receiveKeyPress(options.getFirstKeyboardKeyFromInputButtonList(options.moveDownButton));
				directionKeyPolling[2] = 70;
			}
			if (directionKeyPolling[3] <= 0 && active_menu != null && active_menu.IsActive())
			{
				active_menu.receiveKeyPress(options.getFirstKeyboardKeyFromInputButtonList(options.moveLeftButton));
				directionKeyPolling[3] = 70;
			}
			if (options.gamepadControls && active_menu != null && active_menu.IsActive())
			{
				if (!active_menu.areGamePadControlsImplemented() && padState.IsButtonDown(Buttons.A) && (!oldPadState.IsButtonDown(Buttons.A) || ((float)gamePadAButtonPolling > 650f && !(active_menu is DialogueBox))))
				{
					active_menu.receiveLeftClick(getMousePosition().X, getMousePosition().Y);
					if ((float)gamePadAButtonPolling > 650f)
					{
						gamePadAButtonPolling = 600;
					}
				}
				else if (!active_menu.areGamePadControlsImplemented() && !padState.IsButtonDown(Buttons.A) && oldPadState.IsButtonDown(Buttons.A))
				{
					active_menu.releaseLeftClick(getMousePosition().X, getMousePosition().Y);
				}
				else if (!active_menu.areGamePadControlsImplemented() && padState.IsButtonDown(Buttons.X) && (!oldPadState.IsButtonDown(Buttons.X) || ((float)gamePadXButtonPolling > 650f && !(active_menu is DialogueBox))))
				{
					active_menu.receiveRightClick(getMousePosition().X, getMousePosition().Y);
					if ((float)gamePadXButtonPolling > 650f)
					{
						gamePadXButtonPolling = 600;
					}
				}
				ButtonCollection.ButtonEnumerator enumerator = Utility.getPressedButtons(padState, oldPadState).GetEnumerator();
				while (enumerator.MoveNext())
				{
					Buttons b = enumerator.Current;
					if (active_menu == null || !active_menu.IsActive())
					{
						break;
					}
					Keys key = Utility.mapGamePadButtonToKey(b);
					if (!(active_menu is FarmhandMenu) || game1.IsMainInstance || !options.doesInputListContain(options.menuButton, key))
					{
						active_menu.receiveKeyPress(key);
					}
				}
				if (active_menu != null && active_menu.IsActive() && !active_menu.areGamePadControlsImplemented() && padState.IsButtonDown(Buttons.A) && oldPadState.IsButtonDown(Buttons.A))
				{
					active_menu.leftClickHeld(getMousePosition().X, getMousePosition().Y);
				}
				if (padState.IsButtonDown(Buttons.X))
				{
					gamePadXButtonPolling += gameTime.ElapsedGameTime.Milliseconds;
				}
				else
				{
					gamePadXButtonPolling = 0;
				}
				if (padState.IsButtonDown(Buttons.A))
				{
					gamePadAButtonPolling += gameTime.ElapsedGameTime.Milliseconds;
				}
				else
				{
					gamePadAButtonPolling = 0;
				}
				if (!active_menu.IsActive() && activeClickableMenu == null)
				{
					rightClickPolling = 500;
					gamePadXButtonPolling = 0;
					gamePadAButtonPolling = 0;
				}
			}
		}
		if (mouseState.RightButton == ButtonState.Pressed)
		{
			mouseClickPolling += gameTime.ElapsedGameTime.Milliseconds;
		}
		else
		{
			mouseClickPolling = 0;
		}
		oldMouseState = input.GetMouseState();
		oldKBState = keyState;
		oldPadState = padState;
	}

	public bool ShowLocalCoopJoinMenu()
	{
		if (!base.IsMainInstance)
		{
			return false;
		}
		if (gameMode != 3)
		{
			return false;
		}
		int free_farmhands = 0;
		Utility.ForEachLocation(delegate(GameLocation location)
		{
			if (location is Cabin cabin && (!cabin.HasOwner || !cabin.IsOwnerActivated))
			{
				free_farmhands++;
			}
			return true;
		});
		if (free_farmhands == 0)
		{
			showRedMessage(content.LoadString("Strings\\UI:CoopMenu_NoSlots"));
			return false;
		}
		if (currentMinigame != null)
		{
			return false;
		}
		if (activeClickableMenu != null)
		{
			return false;
		}
		if (!IsLocalCoopJoinable())
		{
			return false;
		}
		playSound("bigSelect");
		activeClickableMenu = new LocalCoopJoinMenu();
		return true;
	}

	public static void updateTextEntry(GameTime gameTime)
	{
		MouseState mouseState = input.GetMouseState();
		KeyboardState keyState = GetKeyboardState();
		GamePadState padState = input.GetGamePadState();
		if (options.gamepadControls && textEntry != null && textEntry != null)
		{
			ButtonCollection.ButtonEnumerator enumerator = Utility.getPressedButtons(padState, oldPadState).GetEnumerator();
			while (enumerator.MoveNext())
			{
				Buttons b = enumerator.Current;
				textEntry.receiveGamePadButton(b);
				if (textEntry == null)
				{
					break;
				}
			}
			enumerator = Utility.getHeldButtons(padState).GetEnumerator();
			while (enumerator.MoveNext())
			{
				Buttons b = enumerator.Current;
				textEntry?.gamePadButtonHeld(b);
				if (textEntry == null)
				{
					break;
				}
			}
		}
		textEntry?.performHoverAction(getMouseX(), getMouseY());
		textEntry?.update(gameTime);
		if (textEntry != null && mouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released)
		{
			textEntry.receiveLeftClick(getMouseX(), getMouseY());
		}
		else if (textEntry != null && mouseState.RightButton == ButtonState.Pressed && (oldMouseState.RightButton == ButtonState.Released || (float)mouseClickPolling > 650f))
		{
			textEntry.receiveRightClick(getMouseX(), getMouseY());
			if ((float)mouseClickPolling > 650f)
			{
				mouseClickPolling = 600;
			}
			if (textEntry == null)
			{
				rightClickPolling = 500;
				mouseClickPolling = 0;
			}
		}
		if (mouseState.ScrollWheelValue != oldMouseState.ScrollWheelValue && textEntry != null)
		{
			if (chatBox != null && chatBox.choosingEmoji && chatBox.emojiMenu.isWithinBounds(getOldMouseX(), getOldMouseY()))
			{
				chatBox.receiveScrollWheelAction(mouseState.ScrollWheelValue - oldMouseState.ScrollWheelValue);
			}
			else
			{
				textEntry.receiveScrollWheelAction(mouseState.ScrollWheelValue - oldMouseState.ScrollWheelValue);
			}
		}
		if (options.gamepadControls && textEntry != null)
		{
			thumbstickPollingTimer -= currentGameTime.ElapsedGameTime.Milliseconds;
			if (thumbstickPollingTimer <= 0)
			{
				if (padState.ThumbSticks.Right.Y > 0.2f)
				{
					textEntry.receiveScrollWheelAction(1);
				}
				else if (padState.ThumbSticks.Right.Y < -0.2f)
				{
					textEntry.receiveScrollWheelAction(-1);
				}
			}
			if (thumbstickPollingTimer <= 0)
			{
				thumbstickPollingTimer = 220 - (int)(Math.Abs(padState.ThumbSticks.Right.Y) * 170f);
			}
			if (Math.Abs(padState.ThumbSticks.Right.Y) < 0.2f)
			{
				thumbstickPollingTimer = 0;
			}
		}
		if (textEntry != null && mouseState.LeftButton == ButtonState.Released && oldMouseState.LeftButton == ButtonState.Pressed)
		{
			textEntry.releaseLeftClick(getMouseX(), getMouseY());
		}
		else if (textEntry != null && mouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Pressed)
		{
			textEntry.leftClickHeld(getMouseX(), getMouseY());
		}
		Keys[] pressedKeys = keyState.GetPressedKeys();
		foreach (Keys k in pressedKeys)
		{
			if (textEntry != null && !oldKBState.GetPressedKeys().Contains(k))
			{
				textEntry.receiveKeyPress(k);
			}
		}
		if (isOneOfTheseKeysDown(oldKBState, options.moveUpButton) || (options.snappyMenus && options.gamepadControls && (Math.Abs(padState.ThumbSticks.Left.X) < padState.ThumbSticks.Left.Y || padState.IsButtonDown(Buttons.DPadUp))))
		{
			directionKeyPolling[0] -= currentGameTime.ElapsedGameTime.Milliseconds;
		}
		else if (isOneOfTheseKeysDown(oldKBState, options.moveRightButton) || (options.snappyMenus && options.gamepadControls && (padState.ThumbSticks.Left.X > Math.Abs(padState.ThumbSticks.Left.Y) || padState.IsButtonDown(Buttons.DPadRight))))
		{
			directionKeyPolling[1] -= currentGameTime.ElapsedGameTime.Milliseconds;
		}
		else if (isOneOfTheseKeysDown(oldKBState, options.moveDownButton) || (options.snappyMenus && options.gamepadControls && (Math.Abs(padState.ThumbSticks.Left.X) < Math.Abs(padState.ThumbSticks.Left.Y) || padState.IsButtonDown(Buttons.DPadDown))))
		{
			directionKeyPolling[2] -= currentGameTime.ElapsedGameTime.Milliseconds;
		}
		else if (isOneOfTheseKeysDown(oldKBState, options.moveLeftButton) || (options.snappyMenus && options.gamepadControls && (Math.Abs(padState.ThumbSticks.Left.X) > Math.Abs(padState.ThumbSticks.Left.Y) || padState.IsButtonDown(Buttons.DPadLeft))))
		{
			directionKeyPolling[3] -= currentGameTime.ElapsedGameTime.Milliseconds;
		}
		if (areAllOfTheseKeysUp(oldKBState, options.moveUpButton) && (!options.snappyMenus || !options.gamepadControls || ((double)padState.ThumbSticks.Left.Y < 0.1 && padState.IsButtonUp(Buttons.DPadUp))))
		{
			directionKeyPolling[0] = 250;
		}
		if (areAllOfTheseKeysUp(oldKBState, options.moveRightButton) && (!options.snappyMenus || !options.gamepadControls || ((double)padState.ThumbSticks.Left.X < 0.1 && padState.IsButtonUp(Buttons.DPadRight))))
		{
			directionKeyPolling[1] = 250;
		}
		if (areAllOfTheseKeysUp(oldKBState, options.moveDownButton) && (!options.snappyMenus || !options.gamepadControls || ((double)padState.ThumbSticks.Left.Y > -0.1 && padState.IsButtonUp(Buttons.DPadDown))))
		{
			directionKeyPolling[2] = 250;
		}
		if (areAllOfTheseKeysUp(oldKBState, options.moveLeftButton) && (!options.snappyMenus || !options.gamepadControls || ((double)padState.ThumbSticks.Left.X > -0.1 && padState.IsButtonUp(Buttons.DPadLeft))))
		{
			directionKeyPolling[3] = 250;
		}
		if (directionKeyPolling[0] <= 0 && textEntry != null)
		{
			textEntry.receiveKeyPress(options.getFirstKeyboardKeyFromInputButtonList(options.moveUpButton));
			directionKeyPolling[0] = 70;
		}
		if (directionKeyPolling[1] <= 0 && textEntry != null)
		{
			textEntry.receiveKeyPress(options.getFirstKeyboardKeyFromInputButtonList(options.moveRightButton));
			directionKeyPolling[1] = 70;
		}
		if (directionKeyPolling[2] <= 0 && textEntry != null)
		{
			textEntry.receiveKeyPress(options.getFirstKeyboardKeyFromInputButtonList(options.moveDownButton));
			directionKeyPolling[2] = 70;
		}
		if (directionKeyPolling[3] <= 0 && textEntry != null)
		{
			textEntry.receiveKeyPress(options.getFirstKeyboardKeyFromInputButtonList(options.moveLeftButton));
			directionKeyPolling[3] = 70;
		}
		if (options.gamepadControls && textEntry != null)
		{
			if (!textEntry.areGamePadControlsImplemented() && padState.IsButtonDown(Buttons.A) && (!oldPadState.IsButtonDown(Buttons.A) || (float)gamePadAButtonPolling > 650f))
			{
				textEntry.receiveLeftClick(getMousePosition().X, getMousePosition().Y);
				if ((float)gamePadAButtonPolling > 650f)
				{
					gamePadAButtonPolling = 600;
				}
			}
			else if (!textEntry.areGamePadControlsImplemented() && !padState.IsButtonDown(Buttons.A) && oldPadState.IsButtonDown(Buttons.A))
			{
				textEntry.releaseLeftClick(getMousePosition().X, getMousePosition().Y);
			}
			else if (!textEntry.areGamePadControlsImplemented() && padState.IsButtonDown(Buttons.X) && (!oldPadState.IsButtonDown(Buttons.X) || (float)gamePadXButtonPolling > 650f))
			{
				textEntry.receiveRightClick(getMousePosition().X, getMousePosition().Y);
				if ((float)gamePadXButtonPolling > 650f)
				{
					gamePadXButtonPolling = 600;
				}
			}
			ButtonCollection.ButtonEnumerator enumerator = Utility.getPressedButtons(padState, oldPadState).GetEnumerator();
			while (enumerator.MoveNext())
			{
				Buttons b = enumerator.Current;
				if (textEntry == null)
				{
					break;
				}
				textEntry.receiveKeyPress(Utility.mapGamePadButtonToKey(b));
			}
			if (textEntry != null && !textEntry.areGamePadControlsImplemented() && padState.IsButtonDown(Buttons.A) && oldPadState.IsButtonDown(Buttons.A))
			{
				textEntry.leftClickHeld(getMousePosition().X, getMousePosition().Y);
			}
			if (padState.IsButtonDown(Buttons.X))
			{
				gamePadXButtonPolling += gameTime.ElapsedGameTime.Milliseconds;
			}
			else
			{
				gamePadXButtonPolling = 0;
			}
			if (padState.IsButtonDown(Buttons.A))
			{
				gamePadAButtonPolling += gameTime.ElapsedGameTime.Milliseconds;
			}
			else
			{
				gamePadAButtonPolling = 0;
			}
			if (textEntry == null)
			{
				rightClickPolling = 500;
				gamePadAButtonPolling = 0;
				gamePadXButtonPolling = 0;
			}
		}
		if (mouseState.RightButton == ButtonState.Pressed)
		{
			mouseClickPolling += gameTime.ElapsedGameTime.Milliseconds;
		}
		else
		{
			mouseClickPolling = 0;
		}
		oldMouseState = input.GetMouseState();
		oldKBState = keyState;
		oldPadState = padState;
	}

	public static string DateCompiled()
	{
		Version version = Assembly.GetExecutingAssembly().GetName().Version;
		return version.Major + "." + version.Minor + "." + version.Build + "." + version.Revision;
	}

	public static void updatePause(GameTime gameTime)
	{
		pauseTime -= gameTime.ElapsedGameTime.Milliseconds;
		if (player.isCrafting && random.NextDouble() < 0.007)
		{
			playSound("crafting");
		}
		if (!(pauseTime <= 0f))
		{
			return;
		}
		if (currentObjectDialogue.Count == 0)
		{
			messagePause = false;
		}
		pauseTime = 0f;
		if (!string.IsNullOrEmpty(messageAfterPause))
		{
			player.isCrafting = false;
			drawObjectDialogue(messageAfterPause);
			messageAfterPause = "";
			if (killScreen)
			{
				killScreen = false;
				player.health = 10;
			}
		}
		else if (killScreen)
		{
			multiplayer.globalChatInfoMessage("PlayerDeath", player.Name);
			screenGlow = false;
			bool handledRevive = false;
			if (currentLocation.GetLocationContext().ReviveLocations != null)
			{
				foreach (ReviveLocation revive_location in currentLocation.GetLocationContext().ReviveLocations)
				{
					if (GameStateQuery.CheckConditions(revive_location.Condition, null, player))
					{
						warpFarmer(revive_location.Location, revive_location.Position.X, revive_location.Position.Y, flip: false);
						handledRevive = true;
						break;
					}
				}
			}
			else
			{
				foreach (ReviveLocation revive_location in LocationContexts.Default.ReviveLocations)
				{
					if (GameStateQuery.CheckConditions(revive_location.Condition, null, player))
					{
						warpFarmer(revive_location.Location, revive_location.Position.X, revive_location.Position.Y, flip: false);
						handledRevive = true;
						break;
					}
				}
			}
			if (!handledRevive)
			{
				warpFarmer("Hospital", 20, 12, flip: false);
			}
		}
		if (currentLocation.currentEvent != null)
		{
			currentLocation.currentEvent.CurrentCommand++;
		}
	}

	public static void CheckValidFullscreenResolution(ref int width, ref int height)
	{
		int preferredW = width;
		int preferredH = height;
		foreach (DisplayMode v in graphics.GraphicsDevice.Adapter.SupportedDisplayModes)
		{
			if (v.Width >= 1280 && v.Width == preferredW && v.Height == preferredH)
			{
				width = preferredW;
				height = preferredH;
				return;
			}
		}
		foreach (DisplayMode v in graphics.GraphicsDevice.Adapter.SupportedDisplayModes)
		{
			if (v.Width >= 1280 && v.Width == graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Width && v.Height == graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Height)
			{
				width = graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Width;
				height = graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Height;
				return;
			}
		}
		bool found_resolution = false;
		foreach (DisplayMode v in graphics.GraphicsDevice.Adapter.SupportedDisplayModes)
		{
			if (v.Width >= 1280 && preferredW > v.Width)
			{
				width = v.Width;
				height = v.Height;
				found_resolution = true;
			}
		}
		if (!found_resolution)
		{
			log.Warn("Requested fullscreen resolution not valid, switching to windowed.");
			width = 1280;
			height = 720;
			options.fullscreen = false;
		}
	}

	public static void toggleNonBorderlessWindowedFullscreen()
	{
		int width = options.preferredResolutionX;
		int height = options.preferredResolutionY;
		graphics.HardwareModeSwitch = options.fullscreen && !options.windowedBorderlessFullscreen;
		if (options.fullscreen && !options.windowedBorderlessFullscreen)
		{
			CheckValidFullscreenResolution(ref width, ref height);
		}
		if (!options.fullscreen && !options.windowedBorderlessFullscreen)
		{
			width = 1280;
			height = 720;
		}
		graphics.PreferredBackBufferWidth = width;
		graphics.PreferredBackBufferHeight = height;
		if (options.fullscreen != graphics.IsFullScreen)
		{
			graphics.ToggleFullScreen();
		}
		graphics.ApplyChanges();
		updateViewportForScreenSizeChange(fullscreenChange: true, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
		GameRunner.instance.OnWindowSizeChange(null, null);
	}

	public static void toggleFullscreen()
	{
		if (options.windowedBorderlessFullscreen)
		{
			graphics.HardwareModeSwitch = false;
			graphics.IsFullScreen = true;
			graphics.ApplyChanges();
			graphics.PreferredBackBufferWidth = Program.gamePtr.Window.ClientBounds.Width;
			graphics.PreferredBackBufferHeight = Program.gamePtr.Window.ClientBounds.Height;
		}
		else
		{
			toggleNonBorderlessWindowedFullscreen();
		}
		GameRunner.instance.OnWindowSizeChange(null, null);
	}

	private void checkForEscapeKeys()
	{
		KeyboardState kbState = input.GetKeyboardState();
		if (!base.IsMainInstance)
		{
			return;
		}
		if (kbState.IsKeyDown(Keys.LeftAlt) && kbState.IsKeyDown(Keys.Enter) && (oldKBState.IsKeyUp(Keys.LeftAlt) || oldKBState.IsKeyUp(Keys.Enter)))
		{
			if (options.isCurrentlyFullscreen() || options.isCurrentlyWindowedBorderless())
			{
				options.setWindowedOption(1);
			}
			else
			{
				options.setWindowedOption(0);
			}
		}
		if ((player.UsingTool || freezeControls) && kbState.IsKeyDown(Keys.RightShift) && kbState.IsKeyDown(Keys.R) && kbState.IsKeyDown(Keys.Delete))
		{
			freezeControls = false;
			player.forceCanMove();
			player.completelyStopAnimatingOrDoingAction();
			player.UsingTool = false;
		}
	}

	public static bool IsPressEvent(ref KeyboardState state, Keys key)
	{
		if (state.IsKeyDown(key) && !oldKBState.IsKeyDown(key))
		{
			oldKBState = state;
			return true;
		}
		return false;
	}

	public static bool IsPressEvent(ref GamePadState state, Buttons btn)
	{
		if (state.IsConnected && state.IsButtonDown(btn) && !oldPadState.IsButtonDown(btn))
		{
			oldPadState = state;
			return true;
		}
		return false;
	}

	public static bool isOneOfTheseKeysDown(KeyboardState state, InputButton[] keys)
	{
		for (int i = 0; i < keys.Length; i++)
		{
			InputButton k = keys[i];
			if (k.key != 0 && state.IsKeyDown(k.key))
			{
				return true;
			}
		}
		return false;
	}

	public static bool areAllOfTheseKeysUp(KeyboardState state, InputButton[] keys)
	{
		for (int i = 0; i < keys.Length; i++)
		{
			InputButton k = keys[i];
			if (k.key != 0 && !state.IsKeyUp(k.key))
			{
				return false;
			}
		}
		return true;
	}

	internal void UpdateTitleScreen(GameTime time)
	{
		if (quit)
		{
			Exit();
			changeMusicTrack("none");
		}
		switch (gameMode)
		{
		case 6:
			_requestedMusicTracks = new Dictionary<MusicContext, KeyValuePair<string, bool>>();
			requestedMusicTrack = "none";
			requestedMusicTrackOverrideable = false;
			requestedMusicDirty = true;
			if (currentLoader != null && !currentLoader.MoveNext())
			{
				if (gameMode == 3)
				{
					setGameMode(3);
					fadeIn = true;
					fadeToBlackAlpha = 0.99f;
				}
				else
				{
					ExitToTitle();
				}
			}
			return;
		case 7:
			currentLoader.MoveNext();
			return;
		case 8:
			pauseAccumulator -= time.ElapsedGameTime.Milliseconds;
			if (pauseAccumulator <= 0f)
			{
				pauseAccumulator = 0f;
				setGameMode(3);
				if (currentObjectDialogue.Count > 0)
				{
					messagePause = true;
					pauseTime = 1E+10f;
					fadeToBlackAlpha = 1f;
					player.CanMove = false;
				}
			}
			return;
		}
		if (game1.instanceIndex > 0)
		{
			if (activeClickableMenu == null && ticks > 1)
			{
				activeClickableMenu = new FarmhandMenu(multiplayer.InitClient(new LidgrenClient("localhost")));
				activeClickableMenu.populateClickableComponentList();
				if (options.SnappyMenus)
				{
					activeClickableMenu.snapToDefaultClickableComponent();
				}
			}
			return;
		}
		if (fadeToBlackAlpha < 1f && fadeIn)
		{
			fadeToBlackAlpha += 0.02f;
		}
		else if (fadeToBlackAlpha > 0f && fadeToBlack)
		{
			fadeToBlackAlpha -= 0.02f;
		}
		if (pauseTime > 0f)
		{
			pauseTime = Math.Max(0f, pauseTime - (float)time.ElapsedGameTime.Milliseconds);
		}
		if (fadeToBlackAlpha >= 1f)
		{
			switch (gameMode)
			{
			case 4:
				if (!fadeToBlack)
				{
					fadeIn = false;
					fadeToBlack = true;
					fadeToBlackAlpha = 2.5f;
				}
				break;
			case 0:
				if (currentSong == null && pauseTime <= 0f && base.IsMainInstance)
				{
					playSound("spring_day_ambient", out var cue);
					currentSong = cue;
				}
				if (activeClickableMenu == null && !quit)
				{
					activeClickableMenu = new TitleMenu();
				}
				break;
			}
			return;
		}
		if (!(fadeToBlackAlpha <= 0f))
		{
			return;
		}
		switch (gameMode)
		{
		case 4:
			if (fadeToBlack)
			{
				fadeIn = true;
				fadeToBlack = false;
				setGameMode(0);
				pauseTime = 2000f;
			}
			break;
		case 0:
			if (fadeToBlack)
			{
				currentLoader = Utility.generateNewFarm(IsClient);
				setGameMode(6);
				loadingMessage = (IsClient ? content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2574", client.serverName) : content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2575"));
				exitActiveMenu();
			}
			break;
		}
	}

	/// <summary>Get whether the given NPC is currently constructing a building anywhere in the world.</summary>
	/// <param name="builder">The NPC constructing the building, usually <see cref="F:StardewValley.Game1.builder_robin" /> or <see cref="F:StardewValley.Game1.builder_wizard" />.</param>
	public static bool IsThereABuildingUnderConstruction(string builder = "Robin")
	{
		if (netWorldState.Value.GetBuilderData(builder) != null)
		{
			return true;
		}
		return false;
	}

	/// <summary>Get the building currently being constructed by a given builder.</summary>
	/// <param name="builder">The NPC constructing the building, usually <see cref="F:StardewValley.Game1.builder_robin" /> or <see cref="F:StardewValley.Game1.builder_wizard" />.</param>
	public static Building GetBuildingUnderConstruction(string builder = "Robin")
	{
		BuilderData builder_data = netWorldState.Value.GetBuilderData(builder);
		if (builder_data == null)
		{
			return null;
		}
		GameLocation location = getLocationFromName(builder_data.buildingLocation.Value);
		if (location == null)
		{
			return null;
		}
		if (client != null && !multiplayer.isActiveLocation(location))
		{
			return null;
		}
		return location.getBuildingAt(Utility.PointToVector2(builder_data.buildingTile.Value));
	}

	/// <summary>Get whether a building type was constructed anywhere in the world.</summary>
	/// <param name="name">The building type's ID in <c>Data/Buildings</c>.</param>
	public static bool IsBuildingConstructed(string name)
	{
		return GetNumberBuildingsConstructed(name) > 0;
	}

	/// <summary>Get the number of buildings of all types constructed anywhere in the world.</summary>
	/// <param name="includeUnderConstruction">Whether to count buildings that haven't finished construction yet.</param>
	public static int GetNumberBuildingsConstructed(bool includeUnderConstruction = false)
	{
		int count = 0;
		foreach (string locationName in netWorldState.Value.LocationsWithBuildings)
		{
			count += getLocationFromName(locationName)?.getNumberBuildingsConstructed(includeUnderConstruction) ?? 0;
		}
		return count;
	}

	/// <summary>Get the number of buildings of a given type constructed anywhere in the world.</summary>
	/// <param name="name">The building type's ID in <c>Data/Buildings</c>.</param>
	/// <param name="includeUnderConstruction">Whether to count buildings that haven't finished construction yet.</param>
	public static int GetNumberBuildingsConstructed(string name, bool includeUnderConstruction = false)
	{
		int count = 0;
		foreach (string locationName in netWorldState.Value.LocationsWithBuildings)
		{
			count += getLocationFromName(locationName)?.getNumberBuildingsConstructed(name, includeUnderConstruction) ?? 0;
		}
		return count;
	}

	private void UpdateLocations(GameTime time)
	{
		loopingLocationCues.Update(currentLocation);
		if (IsClient)
		{
			currentLocation.UpdateWhenCurrentLocation(time);
			{
				foreach (GameLocation item in multiplayer.activeLocations())
				{
					item.updateEvenIfFarmerIsntHere(time);
				}
				return;
			}
		}
		Utility.ForEachLocation(delegate(GameLocation location)
		{
			_UpdateLocation(location, time);
			return true;
		});
		if (currentLocation.IsTemporary)
		{
			_UpdateLocation(currentLocation, time);
		}
		MineShaft.UpdateMines(time);
		VolcanoDungeon.UpdateLevels(time);
	}

	protected void _UpdateLocation(GameLocation location, GameTime time)
	{
		bool shouldUpdate = location.farmers.Any();
		if (!shouldUpdate && location.CanBeRemotedlyViewed())
		{
			if (player.currentLocation == location)
			{
				shouldUpdate = true;
			}
			else
			{
				foreach (Farmer who in otherFarmers.Values)
				{
					if (who.viewingLocation.Value != null && who.viewingLocation.Value.Equals(location.NameOrUniqueName))
					{
						shouldUpdate = true;
						break;
					}
				}
			}
		}
		if (shouldUpdate)
		{
			location.UpdateWhenCurrentLocation(time);
		}
		location.updateEvenIfFarmerIsntHere(time);
		if (location.wasInhabited != shouldUpdate)
		{
			location.wasInhabited = shouldUpdate;
			if (IsMasterGame)
			{
				location.cleanupForVacancy();
			}
		}
	}

	public static void performTenMinuteClockUpdate()
	{
		hooks.OnGame1_PerformTenMinuteClockUpdate(delegate
		{
			int num = getTrulyDarkTime(currentLocation) - 100;
			gameTimeInterval = 0;
			if (IsMasterGame)
			{
				timeOfDay += 10;
			}
			if (timeOfDay % 100 >= 60)
			{
				timeOfDay = timeOfDay - timeOfDay % 100 + 100;
			}
			timeOfDay = Math.Min(timeOfDay, 2600);
			if (isLightning && timeOfDay < 2400 && IsMasterGame)
			{
				Utility.performLightningUpdate(timeOfDay);
			}
			if (timeOfDay == num)
			{
				currentLocation.switchOutNightTiles();
			}
			else if (timeOfDay == getModeratelyDarkTime(currentLocation) && currentLocation.IsOutdoors && !currentLocation.IsRainingHere())
			{
				ambientLight = Color.White;
			}
			if (!eventUp && isDarkOut(currentLocation) && IsPlayingBackgroundMusic)
			{
				changeMusicTrack("none", track_interruptable: true);
			}
			if (weatherIcon == 1)
			{
				Dictionary<string, string> dictionary = temporaryContent.Load<Dictionary<string, string>>("Data\\Festivals\\" + currentSeason + dayOfMonth);
				string[] array = dictionary["conditions"].Split('/');
				int num2 = Convert.ToInt32(ArgUtility.SplitBySpaceAndGet(array[1], 0));
				if (whereIsTodaysFest == null)
				{
					whereIsTodaysFest = array[0];
				}
				if (timeOfDay == num2)
				{
					if (dictionary.TryGetValue("startedMessage", out var value))
					{
						showGlobalMessage(TokenParser.ParseText(value));
					}
					else
					{
						if (!dictionary.TryGetValue("locationDisplayName", out var value2))
						{
							value2 = array[0];
							value2 = value2 switch
							{
								"Forest" => IsWinter ? content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2634") : content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2635"), 
								"Town" => content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2637"), 
								"Beach" => content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2639"), 
								_ => TokenParser.ParseText(GameLocation.GetData(value2)?.DisplayName) ?? value2, 
							};
						}
						showGlobalMessage(content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2640", dictionary["name"]) + value2);
					}
				}
			}
			player.performTenMinuteUpdate();
			switch (timeOfDay)
			{
			case 1200:
				if ((bool)currentLocation.isOutdoors && !currentLocation.IsRainingHere() && (IsPlayingOutdoorsAmbience || currentSong == null || isMusicContextActiveButNotPlaying()))
				{
					playMorningSong();
				}
				break;
			case 2000:
				if (IsPlayingTownMusic)
				{
					changeMusicTrack("none", track_interruptable: true);
				}
				break;
			case 2400:
				dayTimeMoneyBox.timeShakeTimer = 2000;
				player.doEmote(24);
				showGlobalMessage(content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2652"));
				break;
			case 2500:
				dayTimeMoneyBox.timeShakeTimer = 2000;
				player.doEmote(24);
				break;
			case 2600:
				dayTimeMoneyBox.timeShakeTimer = 2000;
				player.mount?.dismount();
				if (player.IsSitting())
				{
					player.StopSitting(animate: false);
				}
				if (player.UsingTool && (!(player.CurrentTool is FishingRod fishingRod) || (!fishingRod.isReeling && !fishingRod.pullingOutOfWater)))
				{
					player.completelyStopAnimatingOrDoingAction();
				}
				break;
			case 2800:
				if (activeClickableMenu != null)
				{
					activeClickableMenu.emergencyShutDown();
					exitActiveMenu();
				}
				player.startToPassOut();
				player.mount?.dismount();
				break;
			}
			foreach (string current in netWorldState.Value.ActivePassiveFestivals)
			{
				if (Utility.TryGetPassiveFestivalData(current, out var data) && timeOfDay == data.StartTime && (!data.OnlyShowMessageOnFirstDay || Utility.GetDayOfPassiveFestival(current) == 1))
				{
					showGlobalMessage(TokenParser.ParseText(data.StartMessage));
				}
			}
			foreach (GameLocation location in locations)
			{
				GameLocation current2 = location;
				if (current2.NameOrUniqueName == currentLocation.NameOrUniqueName)
				{
					current2 = currentLocation;
				}
				current2.performTenMinuteUpdate(timeOfDay);
				current2.timeUpdate(10);
			}
			MineShaft.UpdateMines10Minutes(timeOfDay);
			VolcanoDungeon.UpdateLevels10Minutes(timeOfDay);
			if (IsMasterGame && farmEvent == null)
			{
				netWorldState.Value.UpdateFromGame1();
			}
			for (int num3 = currentLightSources.Count - 1; num3 >= 0; num3--)
			{
				if (currentLightSources.ElementAt(num3).color.A <= 0)
				{
					currentLightSources.Remove(currentLightSources.ElementAt(num3));
				}
			}
		});
	}

	public static bool shouldPlayMorningSong(bool loading_game = false)
	{
		if (eventUp)
		{
			return false;
		}
		if ((double)options.musicVolumeLevel <= 0.025)
		{
			return false;
		}
		if (timeOfDay >= 1200)
		{
			return false;
		}
		if (!loading_game)
		{
			if (currentSong != null)
			{
				return IsPlayingOutdoorsAmbience;
			}
			return false;
		}
		return true;
	}

	public static void UpdateGameClock(GameTime time)
	{
		if (shouldTimePass() && !IsClient)
		{
			gameTimeInterval += time.ElapsedGameTime.Milliseconds;
		}
		if (timeOfDay >= getTrulyDarkTime(currentLocation))
		{
			int adjustedTime = (int)((float)(timeOfDay - timeOfDay % 100) + (float)(timeOfDay % 100 / 10) * 16.66f);
			float transparency = Math.Min(0.93f, 0.75f + ((float)(adjustedTime - getTrulyDarkTime(currentLocation)) + (float)gameTimeInterval / (float)realMilliSecondsPerGameTenMinutes * 16.6f) * 0.000625f);
			outdoorLight = (IsRainingHere() ? ambientLight : eveningColor) * transparency;
		}
		else if (timeOfDay >= getStartingToGetDarkTime(currentLocation))
		{
			int adjustedTime = (int)((float)(timeOfDay - timeOfDay % 100) + (float)(timeOfDay % 100 / 10) * 16.66f);
			float transparency = Math.Min(0.93f, 0.3f + ((float)(adjustedTime - getStartingToGetDarkTime(currentLocation)) + (float)gameTimeInterval / (float)realMilliSecondsPerGameTenMinutes * 16.6f) * 0.00225f);
			outdoorLight = (IsRainingHere() ? ambientLight : eveningColor) * transparency;
		}
		else if (IsRainingHere())
		{
			outdoorLight = ambientLight * 0.3f;
		}
		else
		{
			outdoorLight = ambientLight;
		}
		int num = gameTimeInterval;
		int num2 = realMilliSecondsPerGameTenMinutes;
		GameLocation gameLocation = currentLocation;
		if (num > num2 + ((gameLocation != null) ? new int?(gameLocation.ExtraMillisecondsPerInGameMinute * 10) : null))
		{
			if (panMode)
			{
				gameTimeInterval = 0;
			}
			else
			{
				performTenMinuteClockUpdate();
			}
		}
	}

	public static Event getAvailableWeddingEvent()
	{
		if (weddingsToday.Count > 0)
		{
			long id = weddingsToday[0];
			weddingsToday.RemoveAt(0);
			Farmer farmer = getFarmerMaybeOffline(id);
			if (farmer == null)
			{
				return null;
			}
			if (farmer.hasRoommate())
			{
				return null;
			}
			if (farmer.spouse != null)
			{
				return Utility.getWeddingEvent(farmer);
			}
			long? spouseID = farmer.team.GetSpouse(farmer.UniqueMultiplayerID);
			Farmer spouse = getFarmerMaybeOffline(spouseID.Value);
			if (spouse == null)
			{
				return null;
			}
			if (!getOnlineFarmers().Contains(farmer) || !getOnlineFarmers().Contains(spouse))
			{
				return null;
			}
			player.team.GetFriendship(farmer.UniqueMultiplayerID, spouseID.Value).Status = FriendshipStatus.Married;
			player.team.GetFriendship(farmer.UniqueMultiplayerID, spouseID.Value).WeddingDate = new WorldDate(Date);
			return Utility.getWeddingEvent(farmer);
		}
		return null;
	}

	public static void exitActiveMenu()
	{
		activeClickableMenu = null;
	}

	/// <summary>Perform an action when <see cref="M:StardewValley.Farmer.IsBusyDoingSomething" /> becomes false for the current player (or do it immediately if it's already false).</summary>
	/// <param name="action">The action to perform.</param>
	public static void PerformActionWhenPlayerFree(Action action)
	{
		if (player.IsBusyDoingSomething())
		{
			actionsWhenPlayerFree.Add(action);
		}
		else
		{
			action();
		}
	}

	public static void fadeScreenToBlack()
	{
		screenFade.FadeScreenToBlack();
	}

	public static void fadeClear()
	{
		screenFade.FadeClear();
	}

	private bool onFadeToBlackComplete()
	{
		bool should_halt = false;
		if (killScreen)
		{
			viewportFreeze = true;
			viewport.X = -10000;
		}
		if (exitToTitle)
		{
			setGameMode(4);
			fadeIn = false;
			fadeToBlack = true;
			fadeToBlackAlpha = 0.01f;
			exitToTitle = false;
			changeMusicTrack("none");
			debrisWeather.Clear();
			return true;
		}
		if (timeOfDayAfterFade != -1)
		{
			timeOfDay = timeOfDayAfterFade;
			timeOfDayAfterFade = -1;
		}
		int level;
		if (!nonWarpFade && locationRequest != null)
		{
			if (IsMasterGame && locationRequest.Location == null)
			{
				log.Error("Warp to " + locationRequest.Name + " failed: location wasn't found or couldn't be loaded.");
				locationRequest = null;
			}
			if (locationRequest != null)
			{
				GameLocation previousLocation = currentLocation;
				emoteMenu?.exitThisMenuNoSound();
				if (client != null)
				{
					currentLocation?.StoreCachedMultiplayerMap(multiplayer.cachedMultiplayerMaps);
				}
				currentLocation.cleanupBeforePlayerExit();
				multiplayer.broadcastLocationDelta(currentLocation);
				bool hasResetLocation = false;
				displayFarmer = true;
				if (eventOver)
				{
					eventFinished();
					if (dayOfMonth == 0)
					{
						newDayAfterFade(delegate
						{
							player.Position = new Vector2(320f, 320f);
						});
					}
					return true;
				}
				if (locationRequest.IsRequestFor(currentLocation) && player.previousLocationName != "" && !eventUp && !MineShaft.IsGeneratedLevel(currentLocation, out level))
				{
					player.Position = new Vector2(xLocationAfterWarp * 64, yLocationAfterWarp * 64 - (player.Sprite.getHeight() - 32) + 16);
					viewportFreeze = false;
					currentLocation.resetForPlayerEntry();
					hasResetLocation = true;
				}
				else
				{
					if (MineShaft.IsGeneratedLevel(locationRequest.Name, out level))
					{
						MineShaft mine = locationRequest.Location as MineShaft;
						if (player.IsSitting())
						{
							player.StopSitting(animate: false);
						}
						player.Halt();
						player.forceCanMove();
						if (!IsClient || locationRequest.Location?.Root != null)
						{
							currentLocation = mine;
							mine.resetForPlayerEntry();
							hasResetLocation = true;
						}
						currentLocation.Map.LoadTileSheets(mapDisplayDevice);
						checkForRunButton(GetKeyboardState());
					}
					if (!eventUp)
					{
						player.Position = new Vector2(xLocationAfterWarp * 64, yLocationAfterWarp * 64 - (player.Sprite.getHeight() - 32) + 16);
					}
					if (!MineShaft.IsGeneratedLevel(locationRequest.Name, out level) && locationRequest.Location != null)
					{
						currentLocation = locationRequest.Location;
						if (!IsClient)
						{
							locationRequest.Loaded(locationRequest.Location);
							currentLocation.resetForPlayerEntry();
							hasResetLocation = true;
						}
						currentLocation.Map.LoadTileSheets(mapDisplayDevice);
						if (!viewportFreeze && currentLocation.Map.DisplayWidth <= viewport.Width)
						{
							viewport.X = (currentLocation.Map.DisplayWidth - viewport.Width) / 2;
						}
						if (!viewportFreeze && currentLocation.Map.DisplayHeight <= viewport.Height)
						{
							viewport.Y = (currentLocation.Map.DisplayHeight - viewport.Height) / 2;
						}
						checkForRunButton(GetKeyboardState(), ignoreKeyPressQualifier: true);
					}
					if (!eventUp)
					{
						viewportFreeze = false;
					}
				}
				forceSnapOnNextViewportUpdate = true;
				player.FarmerSprite.PauseForSingleAnimation = false;
				player.faceDirection(facingDirectionAfterWarp);
				_isWarping = false;
				if (player.ActiveObject != null)
				{
					player.showCarrying();
				}
				else
				{
					player.showNotCarrying();
				}
				if (IsClient)
				{
					if (locationRequest.Location != null && locationRequest.Location.Root != null && multiplayer.isActiveLocation(locationRequest.Location))
					{
						currentLocation = locationRequest.Location;
						locationRequest.Loaded(locationRequest.Location);
						if (!hasResetLocation)
						{
							currentLocation.resetForPlayerEntry();
						}
						player.currentLocation = currentLocation;
						locationRequest.Warped(currentLocation);
						currentLocation.updateSeasonalTileSheets();
						if (IsDebrisWeatherHere())
						{
							populateDebrisWeatherArray();
						}
						warpingForForcedRemoteEvent = false;
						locationRequest = null;
					}
					else
					{
						requestLocationInfoFromServer();
						if (currentLocation == null)
						{
							return true;
						}
					}
				}
				else
				{
					player.currentLocation = locationRequest.Location;
					locationRequest.Warped(locationRequest.Location);
					locationRequest = null;
				}
				if (locationRequest == null && currentLocation.Name == "Farm" && !eventUp)
				{
					if (player.position.X / 64f >= (float)(currentLocation.map.Layers[0].LayerWidth - 1))
					{
						player.position.X -= 64f;
					}
					else if (player.position.Y / 64f >= (float)(currentLocation.map.Layers[0].LayerHeight - 1))
					{
						player.position.Y -= 32f;
					}
					if (player.position.Y / 64f >= (float)(currentLocation.map.Layers[0].LayerHeight - 2))
					{
						player.position.X -= 48f;
					}
				}
				if (MineShaft.IsGeneratedLevel(previousLocation, out level) && currentLocation != null && !MineShaft.IsGeneratedLevel(currentLocation, out level))
				{
					MineShaft.OnLeftMines();
				}
				player.OnWarp();
				should_halt = true;
			}
		}
		if (newDay)
		{
			newDayAfterFade(After);
			return true;
		}
		if (eventOver)
		{
			eventFinished();
			if (dayOfMonth == 0)
			{
				newDayAfterFade(After);
			}
			return true;
		}
		if (currentSong?.Name == "rain" && currentLocation.IsRainingHere())
		{
			if (currentLocation.IsOutdoors)
			{
				currentSong.SetVariable("Frequency", 100f);
			}
			else if (!MineShaft.IsGeneratedLevel(currentLocation.Name, out level))
			{
				currentSong.SetVariable("Frequency", 15f);
			}
		}
		return should_halt;
		static void After()
		{
			if (eventOver)
			{
				eventFinished();
				if (dayOfMonth == 0)
				{
					newDayAfterFade(delegate
					{
						player.Position = new Vector2(320f, 320f);
					});
				}
			}
			nonWarpFade = false;
			fadeIn = false;
		}
		static void After()
		{
			currentLocation.resetForPlayerEntry();
			nonWarpFade = false;
			fadeIn = false;
		}
	}

	/// <summary>Update game state when the current player finishes warping to a new location.</summary>
	/// <param name="oldLocation">The location which the player just left (or <c>null</c> for the first location after loading the save).</param>
	/// <param name="newLocation">The location which the player just arrived in.</param>
	public static void OnLocationChanged(GameLocation oldLocation, GameLocation newLocation)
	{
		if (!hasLoadedGame)
		{
			return;
		}
		eventsSeenSinceLastLocationChange.Clear();
		if (newLocation.Name != null && !MineShaft.IsGeneratedLevel(newLocation, out var level) && !VolcanoDungeon.IsGeneratedLevel(newLocation.Name, out level))
		{
			player.locationsVisited.Add(newLocation.Name);
		}
		if (newLocation.IsOutdoors && !newLocation.ignoreDebrisWeather && newLocation.IsDebrisWeatherHere() && GetSeasonForLocation(newLocation) != debrisWeatherSeason)
		{
			windGust = 0f;
			WeatherDebris.globalWind = 0f;
			populateDebrisWeatherArray();
			if (wind != null)
			{
				wind.Stop(AudioStopOptions.AsAuthored);
				wind = null;
			}
		}
		GameLocation.HandleMusicChange(oldLocation, newLocation);
		TriggerActionManager.Raise("LocationChanged");
	}

	private static void onFadedBackInComplete()
	{
		if (killScreen)
		{
			pauseThenMessage(1500, "..." + player.Name + "?");
		}
		else if (!eventUp)
		{
			player.CanMove = true;
		}
		checkForRunButton(oldKBState, ignoreKeyPressQualifier: true);
	}

	public static void UpdateOther(GameTime time)
	{
		if (currentLocation == null || (!player.passedOut && screenFade.UpdateFade(time)))
		{
			return;
		}
		if (dialogueUp)
		{
			player.CanMove = false;
		}
		for (int i = delayedActions.Count - 1; i >= 0; i--)
		{
			DelayedAction action = delayedActions[i];
			if (action.update(time) && delayedActions.Contains(action))
			{
				delayedActions.Remove(action);
			}
		}
		if (timeOfDay >= 2600 || player.stamina <= -15f)
		{
			if (currentMinigame != null && currentMinigame.forceQuit())
			{
				currentMinigame = null;
			}
			if (currentMinigame == null && player.canMove && player.freezePause <= 0 && !player.UsingTool && !eventUp && (IsMasterGame || (bool)player.isCustomized) && locationRequest == null && activeClickableMenu == null)
			{
				player.startToPassOut();
				player.freezePause = 7000;
			}
		}
		for (int i = screenOverlayTempSprites.Count - 1; i >= 0; i--)
		{
			if (screenOverlayTempSprites[i].update(time))
			{
				screenOverlayTempSprites.RemoveAt(i);
			}
		}
		for (int i = uiOverlayTempSprites.Count - 1; i >= 0; i--)
		{
			if (uiOverlayTempSprites[i].update(time))
			{
				uiOverlayTempSprites.RemoveAt(i);
			}
		}
		if ((player.CanMove || player.UsingTool) && shouldTimePass())
		{
			buffsDisplay.update(time);
		}
		player.CurrentItem?.actionWhenBeingHeld(player);
		float tmp = dialogueButtonScale;
		dialogueButtonScale = (float)(16.0 * Math.Sin(time.TotalGameTime.TotalMilliseconds % 1570.0 / 500.0));
		if (tmp > dialogueButtonScale && !dialogueButtonShrinking)
		{
			dialogueButtonShrinking = true;
		}
		else if (tmp < dialogueButtonScale && dialogueButtonShrinking)
		{
			dialogueButtonShrinking = false;
		}
		if (screenGlow)
		{
			if (screenGlowUp || screenGlowHold)
			{
				if (screenGlowHold)
				{
					screenGlowAlpha = Math.Min(screenGlowAlpha + screenGlowRate, screenGlowMax);
				}
				else
				{
					screenGlowAlpha = Math.Min(screenGlowAlpha + 0.03f, 0.6f);
					if (screenGlowAlpha >= 0.6f)
					{
						screenGlowUp = false;
					}
				}
			}
			else
			{
				screenGlowAlpha -= 0.01f;
				if (screenGlowAlpha <= 0f)
				{
					screenGlow = false;
				}
			}
		}
		for (int i = hudMessages.Count - 1; i >= 0; i--)
		{
			if (hudMessages[i].update(time))
			{
				hudMessages.RemoveAt(i);
			}
		}
		updateWeather(time);
		if (!fadeToBlack)
		{
			currentLocation.checkForMusic(time);
		}
		if (debrisSoundInterval > 0f)
		{
			debrisSoundInterval -= time.ElapsedGameTime.Milliseconds;
		}
		noteBlockTimer += time.ElapsedGameTime.Milliseconds;
		if (noteBlockTimer > 1000f)
		{
			noteBlockTimer = 0f;
			if (player.health < 20 && CurrentEvent == null)
			{
				hitShakeTimer = 250;
				if (player.health <= 10)
				{
					hitShakeTimer = 500;
					if (showingHealthBar && fadeToBlackAlpha <= 0f)
					{
						for (int i = 0; i < 3; i++)
						{
							uiOverlayTempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(366, 412, 5, 6), new Vector2(random.Next(32) + uiViewport.Width - 112, uiViewport.Height - 224 - (player.maxHealth - 100) - 16 + 4), flipped: false, 0.017f, Color.Red)
							{
								motion = new Vector2(-1.5f, -8 + random.Next(-1, 2)),
								acceleration = new Vector2(0f, 0.5f),
								local = true,
								scale = 4f,
								delayBeforeAnimationStart = i * 150
							});
						}
					}
				}
			}
		}
		drawLighting = (currentLocation.IsOutdoors && !outdoorLight.Equals(Color.White)) || !ambientLight.Equals(Color.White) || (currentLocation is MineShaft && !((MineShaft)currentLocation).getLightingColor(time).Equals(Color.White));
		if (player.hasBuff("26"))
		{
			drawLighting = true;
		}
		if (hitShakeTimer > 0)
		{
			hitShakeTimer -= time.ElapsedGameTime.Milliseconds;
		}
		if (staminaShakeTimer > 0)
		{
			staminaShakeTimer -= time.ElapsedGameTime.Milliseconds;
		}
		background?.update(viewport);
		cursorTileHintCheckTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
		currentCursorTile.X = (viewport.X + getOldMouseX()) / 64;
		currentCursorTile.Y = (viewport.Y + getOldMouseY()) / 64;
		if (cursorTileHintCheckTimer <= 0 || !currentCursorTile.Equals(lastCursorTile))
		{
			cursorTileHintCheckTimer = 250;
			updateCursorTileHint();
			if (player.CanMove)
			{
				checkForRunButton(oldKBState, ignoreKeyPressQualifier: true);
			}
		}
		if (!MineShaft.IsGeneratedLevel(currentLocation.Name, out var _))
		{
			MineShaft.timeSinceLastMusic = 200000;
		}
		if (activeClickableMenu == null && farmEvent == null && keyboardDispatcher != null && !IsChatting)
		{
			keyboardDispatcher.Subscriber = null;
		}
	}

	public static void updateWeather(GameTime time)
	{
		if (currentLocation.IsOutdoors && currentLocation.IsSnowingHere())
		{
			snowPos = updateFloatingObjectPositionForMovement(current: new Vector2(viewport.X, viewport.Y), w: snowPos, previous: previousViewportPosition, speed: -1f);
			return;
		}
		if (currentLocation.IsOutdoors && currentLocation.IsRainingHere())
		{
			for (int i = 0; i < rainDrops.Length; i++)
			{
				if (rainDrops[i].frame == 0)
				{
					rainDrops[i].accumulator += time.ElapsedGameTime.Milliseconds;
					if (rainDrops[i].accumulator < 70)
					{
						continue;
					}
					rainDrops[i].position += new Vector2(-16 + i * 8 / rainDrops.Length, 32 - i * 8 / rainDrops.Length);
					rainDrops[i].accumulator = 0;
					if (random.NextDouble() < 0.1)
					{
						rainDrops[i].frame++;
					}
					if (currentLocation is IslandNorth || currentLocation is Caldera)
					{
						Point p = new Point((int)(rainDrops[i].position.X + (float)viewport.X) / 64, (int)(rainDrops[i].position.Y + (float)viewport.Y) / 64);
						p.Y--;
						if (currentLocation.isTileOnMap(p.X, p.Y) && currentLocation.getTileIndexAt(p, "Back") == -1 && currentLocation.getTileIndexAt(p, "Buildings") == -1)
						{
							rainDrops[i].frame = 0;
						}
					}
					if (rainDrops[i].position.Y > (float)(viewport.Height + 64))
					{
						rainDrops[i].position.Y = -64f;
					}
					continue;
				}
				rainDrops[i].accumulator += time.ElapsedGameTime.Milliseconds;
				if (rainDrops[i].accumulator > 70)
				{
					rainDrops[i].frame = (rainDrops[i].frame + 1) % 4;
					rainDrops[i].accumulator = 0;
					if (rainDrops[i].frame == 0)
					{
						rainDrops[i].position = new Vector2(random.Next(viewport.Width), random.Next(viewport.Height));
					}
				}
			}
			return;
		}
		if (currentLocation.IsOutdoors && !currentLocation.ignoreDebrisWeather && currentLocation.IsDebrisWeatherHere())
		{
			if (currentLocation.GetSeason() == Season.Fall)
			{
				if (WeatherDebris.globalWind == 0f)
				{
					WeatherDebris.globalWind = -0.5f;
				}
				if (random.NextDouble() < 0.001 && windGust == 0f && WeatherDebris.globalWind >= -0.5f)
				{
					windGust += (float)random.Next(-10, -1) / 100f;
					playSound("wind", out wind);
				}
				else if (windGust != 0f)
				{
					windGust = Math.Max(-5f, windGust * 1.02f);
					WeatherDebris.globalWind = -0.5f + windGust;
					if (windGust < -0.2f && random.NextDouble() < 0.007)
					{
						windGust = 0f;
					}
				}
				if (WeatherDebris.globalWind < -0.5f)
				{
					WeatherDebris.globalWind = Math.Min(-0.5f, WeatherDebris.globalWind + 0.015f);
					if (wind != null)
					{
						wind.SetVariable("Volume", (0f - WeatherDebris.globalWind) * 20f);
						wind.SetVariable("Frequency", (0f - WeatherDebris.globalWind) * 20f);
						if (WeatherDebris.globalWind == -0.5f)
						{
							wind.Stop(AudioStopOptions.AsAuthored);
						}
					}
				}
			}
			else
			{
				if (WeatherDebris.globalWind == 0f)
				{
					WeatherDebris.globalWind = -0.25f;
				}
				if (wind != null)
				{
					wind.Stop(AudioStopOptions.AsAuthored);
					wind = null;
				}
			}
			{
				foreach (WeatherDebris item in debrisWeather)
				{
					item.update();
				}
				return;
			}
		}
		if (wind != null)
		{
			wind.Stop(AudioStopOptions.AsAuthored);
			wind = null;
		}
	}

	public static void updateCursorTileHint()
	{
		if (activeClickableMenu != null)
		{
			return;
		}
		mouseCursorTransparency = 1f;
		isActionAtCurrentCursorTile = false;
		isInspectionAtCurrentCursorTile = false;
		isSpeechAtCurrentCursorTile = false;
		int xTile = (viewport.X + getOldMouseX()) / 64;
		int yTile = (viewport.Y + getOldMouseY()) / 64;
		if (currentLocation != null)
		{
			isActionAtCurrentCursorTile = currentLocation.isActionableTile(xTile, yTile, player);
			if (!isActionAtCurrentCursorTile)
			{
				isActionAtCurrentCursorTile = currentLocation.isActionableTile(xTile, yTile + 1, player);
			}
		}
		lastCursorTile = currentCursorTile;
	}

	public static void updateMusic()
	{
		if (game1.IsMainInstance)
		{
			Game1 important_music_instance = null;
			string important_instance_music = null;
			int sub_location_priority = 1;
			int non_ambient_world_priority = 2;
			int minigame_priority = 5;
			int event_priority = 6;
			int mermaid_show = 7;
			int priority = 0;
			float default_context_priority = GetDefaultSongPriority(getMusicTrackName(), game1.instanceIsOverridingTrack, game1);
			MusicContext primary_music_context = MusicContext.Default;
			foreach (Game1 instance in GameRunner.instance.gameInstances)
			{
				MusicContext active_context = instance._instanceActiveMusicContext;
				if (instance.IsMainInstance)
				{
					primary_music_context = active_context;
				}
				string track_name = null;
				string actual_track_name = null;
				if (instance._instanceRequestedMusicTracks.TryGetValue(active_context, out var trackData))
				{
					track_name = trackData.Key;
				}
				if (instance.instanceIsOverridingTrack && instance.instanceCurrentSong != null)
				{
					actual_track_name = instance.instanceCurrentSong.Name;
				}
				switch (active_context)
				{
				case MusicContext.Event:
					if (priority < event_priority && track_name != null)
					{
						priority = event_priority;
						important_music_instance = instance;
						important_instance_music = track_name;
					}
					break;
				case MusicContext.MiniGame:
					if (priority < minigame_priority && track_name != null)
					{
						priority = minigame_priority;
						important_music_instance = instance;
						important_instance_music = track_name;
					}
					break;
				case MusicContext.SubLocation:
					if (priority < sub_location_priority && track_name != null)
					{
						priority = sub_location_priority;
						important_music_instance = instance;
						important_instance_music = ((actual_track_name == null) ? track_name : actual_track_name);
					}
					break;
				case MusicContext.Default:
					if (track_name == "mermaidSong")
					{
						priority = mermaid_show;
						important_music_instance = instance;
						important_instance_music = track_name;
					}
					if (primary_music_context <= active_context && track_name != null)
					{
						float instance_default_context_priority = GetDefaultSongPriority(track_name, instance.instanceIsOverridingTrack, instance);
						if (default_context_priority < instance_default_context_priority)
						{
							default_context_priority = instance_default_context_priority;
							priority = non_ambient_world_priority;
							important_music_instance = instance;
							important_instance_music = ((actual_track_name == null) ? track_name : actual_track_name);
						}
					}
					break;
				}
			}
			if (important_music_instance == null || important_music_instance == game1)
			{
				if (doesMusicContextHaveTrack(MusicContext.ImportantSplitScreenMusic))
				{
					stopMusicTrack(MusicContext.ImportantSplitScreenMusic);
				}
			}
			else if (important_instance_music == null && doesMusicContextHaveTrack(MusicContext.ImportantSplitScreenMusic))
			{
				stopMusicTrack(MusicContext.ImportantSplitScreenMusic);
			}
			else if (important_instance_music != null && getMusicTrackName(MusicContext.ImportantSplitScreenMusic) != important_instance_music)
			{
				changeMusicTrack(important_instance_music, track_interruptable: false, MusicContext.ImportantSplitScreenMusic);
			}
		}
		string song_to_play = null;
		bool track_overrideable = false;
		bool song_overridden = false;
		if (currentLocation != null && currentLocation.IsMiniJukeboxPlaying() && (!requestedMusicDirty || requestedMusicTrackOverrideable) && currentTrackOverrideable)
		{
			song_to_play = null;
			song_overridden = true;
			string mini_jukebox_track = currentLocation.miniJukeboxTrack.Value;
			if (mini_jukebox_track == "random")
			{
				mini_jukebox_track = ((currentLocation.randomMiniJukeboxTrack.Value != null) ? currentLocation.randomMiniJukeboxTrack.Value : "");
			}
			if (currentSong == null || !currentSong.IsPlaying || currentSong.Name != mini_jukebox_track)
			{
				if (!soundBank.Exists(mini_jukebox_track))
				{
					log.Error($"Location {currentLocation.NameOrUniqueName} has invalid jukebox track '{mini_jukebox_track}' selected, turning off jukebox.");
					player.currentLocation.miniJukeboxTrack.Value = "";
				}
				else
				{
					song_to_play = mini_jukebox_track;
					requestedMusicDirty = false;
					track_overrideable = true;
				}
			}
		}
		if (isOverridingTrack != song_overridden)
		{
			isOverridingTrack = song_overridden;
			if (!isOverridingTrack)
			{
				requestedMusicDirty = true;
			}
		}
		if (requestedMusicDirty)
		{
			song_to_play = requestedMusicTrack;
			track_overrideable = requestedMusicTrackOverrideable;
		}
		if (!string.IsNullOrEmpty(song_to_play))
		{
			musicPlayerVolume = Math.Max(0f, Math.Min(options.musicVolumeLevel, musicPlayerVolume - 0.01f));
			ambientPlayerVolume = Math.Max(0f, Math.Min(options.musicVolumeLevel, ambientPlayerVolume - 0.01f));
			if (game1.IsMainInstance)
			{
				musicCategory.SetVolume(musicPlayerVolume);
				ambientCategory.SetVolume(ambientPlayerVolume);
			}
			if (musicPlayerVolume != 0f || ambientPlayerVolume != 0f)
			{
				return;
			}
			if (song_to_play == "none" || song_to_play == "silence")
			{
				if (game1.IsMainInstance && currentSong != null)
				{
					currentSong.Stop(AudioStopOptions.Immediate);
					currentSong.Dispose();
					currentSong = null;
				}
			}
			else if ((options.musicVolumeLevel != 0f || options.ambientVolumeLevel != 0f) && (song_to_play != "rain" || endOfNightMenus.Count == 0))
			{
				if (game1.IsMainInstance && currentSong != null)
				{
					currentSong.Stop(AudioStopOptions.Immediate);
					currentSong.Dispose();
					currentSong = null;
				}
				currentSong = soundBank.GetCue(song_to_play);
				if (game1.IsMainInstance)
				{
					currentSong.Play();
				}
				if (game1.IsMainInstance && currentSong != null && currentSong.Name == "rain" && currentLocation != null)
				{
					if (IsRainingHere())
					{
						int level;
						if (currentLocation.IsOutdoors)
						{
							currentSong.SetVariable("Frequency", 100f);
						}
						else if (!MineShaft.IsGeneratedLevel(currentLocation, out level))
						{
							currentSong.SetVariable("Frequency", 15f);
						}
					}
					else if (eventUp)
					{
						currentSong.SetVariable("Frequency", 100f);
					}
				}
			}
			else
			{
				currentSong?.Stop(AudioStopOptions.Immediate);
			}
			currentTrackOverrideable = track_overrideable;
			requestedMusicDirty = false;
		}
		else if (MusicDuckTimer > 0f)
		{
			MusicDuckTimer -= (float)currentGameTime.ElapsedGameTime.TotalMilliseconds;
			musicPlayerVolume = Math.Max(musicPlayerVolume - options.musicVolumeLevel / 33f, options.musicVolumeLevel / 12f);
			if (game1.IsMainInstance)
			{
				musicCategory.SetVolume(musicPlayerVolume);
			}
		}
		else if (musicPlayerVolume < options.musicVolumeLevel || ambientPlayerVolume < options.ambientVolumeLevel)
		{
			if (musicPlayerVolume < options.musicVolumeLevel)
			{
				musicPlayerVolume = Math.Min(1f, musicPlayerVolume += 0.01f);
				if (game1.IsMainInstance)
				{
					musicCategory.SetVolume(musicPlayerVolume);
				}
			}
			if (ambientPlayerVolume < options.ambientVolumeLevel)
			{
				ambientPlayerVolume = Math.Min(1f, ambientPlayerVolume += 0.015f);
				if (game1.IsMainInstance)
				{
					ambientCategory.SetVolume(ambientPlayerVolume);
				}
			}
		}
		else if (currentSong != null && !currentSong.IsPlaying && !currentSong.IsStopped)
		{
			currentSong = soundBank.GetCue(currentSong.Name);
			if (game1.IsMainInstance)
			{
				currentSong.Play();
			}
		}
	}

	public static int GetDefaultSongPriority(string song_name, bool is_playing_override, Game1 instance)
	{
		if (is_playing_override)
		{
			return 9;
		}
		if (song_name == "none")
		{
			return 0;
		}
		if (instance._instanceIsPlayingOutdoorsAmbience || instance._instanceIsPlayingNightAmbience || song_name == "rain")
		{
			return 1;
		}
		if (instance._instanceIsPlayingMorningSong)
		{
			return 2;
		}
		if (instance._instanceIsPlayingTownMusic)
		{
			return 3;
		}
		if (song_name == "jungle_ambience")
		{
			return 7;
		}
		if (instance._instanceIsPlayingBackgroundMusic)
		{
			return 8;
		}
		if (instance.instanceGameLocation is MineShaft)
		{
			if (song_name.Contains("Ambient"))
			{
				return 7;
			}
			if (song_name.EndsWith("Mine"))
			{
				return 20;
			}
		}
		return 10;
	}

	public static void updateRainDropPositionForPlayerMovement(int direction, float speed)
	{
		if (currentLocation.IsRainingHere())
		{
			for (int i = 0; i < rainDrops.Length; i++)
			{
				switch (direction)
				{
				case 0:
					rainDrops[i].position.Y += speed;
					if (rainDrops[i].position.Y > (float)(viewport.Height + 64))
					{
						rainDrops[i].position.Y = -64f;
					}
					break;
				case 1:
					rainDrops[i].position.X -= speed;
					if (rainDrops[i].position.X < -64f)
					{
						rainDrops[i].position.X = viewport.Width;
					}
					break;
				case 2:
					rainDrops[i].position.Y -= speed;
					if (rainDrops[i].position.Y < -64f)
					{
						rainDrops[i].position.Y = viewport.Height;
					}
					break;
				case 3:
					rainDrops[i].position.X += speed;
					if (rainDrops[i].position.X > (float)(viewport.Width + 64))
					{
						rainDrops[i].position.X = -64f;
					}
					break;
				}
			}
		}
		else
		{
			updateDebrisWeatherForMovement(debrisWeather, direction, speed);
		}
	}

	public static void initializeVolumeLevels()
	{
		if (!LocalMultiplayer.IsLocalMultiplayer() || game1.IsMainInstance)
		{
			soundCategory.SetVolume(options.soundVolumeLevel);
			musicCategory.SetVolume(options.musicVolumeLevel);
			ambientCategory.SetVolume(options.ambientVolumeLevel);
			footstepCategory.SetVolume(options.footstepVolumeLevel);
		}
	}

	public static void updateDebrisWeatherForMovement(List<WeatherDebris> debris, int direction, float speed)
	{
		if (!(fadeToBlackAlpha <= 0f) || debris == null)
		{
			return;
		}
		foreach (WeatherDebris w in debris)
		{
			switch (direction)
			{
			case 0:
				w.position.Y += speed;
				if (w.position.Y > (float)(viewport.Height + 64))
				{
					w.position.Y = -64f;
				}
				break;
			case 1:
				w.position.X -= speed;
				if (w.position.X < -64f)
				{
					w.position.X = viewport.Width;
				}
				break;
			case 2:
				w.position.Y -= speed;
				if (w.position.Y < -64f)
				{
					w.position.Y = viewport.Height;
				}
				break;
			case 3:
				w.position.X += speed;
				if (w.position.X > (float)(viewport.Width + 64))
				{
					w.position.X = -64f;
				}
				break;
			}
		}
	}

	public static Vector2 updateFloatingObjectPositionForMovement(Vector2 w, Vector2 current, Vector2 previous, float speed)
	{
		if (current.Y < previous.Y)
		{
			w.Y -= Math.Abs(current.Y - previous.Y) * speed;
		}
		else if (current.Y > previous.Y)
		{
			w.Y += Math.Abs(current.Y - previous.Y) * speed;
		}
		if (current.X > previous.X)
		{
			w.X += Math.Abs(current.X - previous.X) * speed;
		}
		else if (current.X < previous.X)
		{
			w.X -= Math.Abs(current.X - previous.X) * speed;
		}
		return w;
	}

	public static void updateRaindropPosition()
	{
		if (HostPaused)
		{
			return;
		}
		if (IsRainingHere())
		{
			int xOffset = viewport.X - (int)previousViewportPosition.X;
			int yOffset = viewport.Y - (int)previousViewportPosition.Y;
			for (int i = 0; i < rainDrops.Length; i++)
			{
				rainDrops[i].position.X -= (float)xOffset * 1f;
				rainDrops[i].position.Y -= (float)yOffset * 1f;
				if (rainDrops[i].position.Y > (float)(viewport.Height + 64))
				{
					rainDrops[i].position.Y = -64f;
				}
				else if (rainDrops[i].position.X < -64f)
				{
					rainDrops[i].position.X = viewport.Width;
				}
				else if (rainDrops[i].position.Y < -64f)
				{
					rainDrops[i].position.Y = viewport.Height;
				}
				else if (rainDrops[i].position.X > (float)(viewport.Width + 64))
				{
					rainDrops[i].position.X = -64f;
				}
			}
		}
		else
		{
			updateDebrisWeatherForMovement(debrisWeather);
		}
	}

	public static void updateDebrisWeatherForMovement(List<WeatherDebris> debris)
	{
		if (HostPaused || debris == null || !(fadeToBlackAlpha < 1f))
		{
			return;
		}
		int xOffset = viewport.X - (int)previousViewportPosition.X;
		int yOffset = viewport.Y - (int)previousViewportPosition.Y;
		if (Math.Abs(xOffset) > 100 || Math.Abs(yOffset) > 80)
		{
			return;
		}
		int wrapBuffer = 16;
		foreach (WeatherDebris w in debris)
		{
			w.position.X -= (float)xOffset * 1f;
			w.position.Y -= (float)yOffset * 1f;
			if (w.position.Y > (float)(viewport.Height + 64 + wrapBuffer))
			{
				w.position.Y = -64f;
			}
			else if (w.position.X < (float)(-64 - wrapBuffer))
			{
				w.position.X = viewport.Width;
			}
			else if (w.position.Y < (float)(-64 - wrapBuffer))
			{
				w.position.Y = viewport.Height;
			}
			else if (w.position.X > (float)(viewport.Width + 64 + wrapBuffer))
			{
				w.position.X = -64f;
			}
		}
	}

	public static void randomizeRainPositions()
	{
		for (int i = 0; i < 70; i++)
		{
			rainDrops[i] = new RainDrop(random.Next(viewport.Width), random.Next(viewport.Height), random.Next(4), random.Next(70));
		}
	}

	public static void randomizeDebrisWeatherPositions(List<WeatherDebris> debris)
	{
		if (debris == null)
		{
			return;
		}
		foreach (WeatherDebris debri in debris)
		{
			debri.position = Utility.getRandomPositionOnScreen();
		}
	}

	public static void eventFinished()
	{
		player.canOnlyWalk = false;
		if (player.bathingClothes.Value)
		{
			player.canOnlyWalk = true;
		}
		eventOver = false;
		eventUp = false;
		player.CanMove = true;
		displayHUD = true;
		player.faceDirection(player.orientationBeforeEvent);
		player.completelyStopAnimatingOrDoingAction();
		viewportFreeze = false;
		Action callback = null;
		if (currentLocation.currentEvent != null && currentLocation.currentEvent.onEventFinished != null)
		{
			callback = currentLocation.currentEvent.onEventFinished;
			currentLocation.currentEvent.onEventFinished = null;
		}
		LocationRequest exitLocation = null;
		if (currentLocation.currentEvent != null)
		{
			exitLocation = currentLocation.currentEvent.exitLocation;
			currentLocation.currentEvent.cleanup();
			currentLocation.currentEvent = null;
		}
		if (player.ActiveObject != null)
		{
			player.showCarrying();
		}
		if (dayOfMonth != 0)
		{
			currentLightSources.Clear();
		}
		if (exitLocation == null && currentLocation != null && locationRequest == null)
		{
			exitLocation = new LocationRequest(currentLocation.NameOrUniqueName, currentLocation.isStructure, currentLocation);
		}
		if (exitLocation != null)
		{
			if (exitLocation.Location is Farm && player.positionBeforeEvent.Y == 64f)
			{
				player.positionBeforeEvent.X += 1f;
			}
			exitLocation.OnWarp += delegate
			{
				player.locationBeforeForcedEvent.Value = null;
			};
			if (exitLocation.Location == currentLocation)
			{
				GameLocation.HandleMusicChange(currentLocation, currentLocation);
			}
			warpFarmer(exitLocation, (int)player.positionBeforeEvent.X, (int)player.positionBeforeEvent.Y, player.orientationBeforeEvent);
		}
		else
		{
			GameLocation.HandleMusicChange(currentLocation, currentLocation);
			player.setTileLocation(player.positionBeforeEvent);
			player.locationBeforeForcedEvent.Value = null;
		}
		nonWarpFade = false;
		fadeToBlackAlpha = 1f;
		callback?.Invoke();
	}

	public static void populateDebrisWeatherArray()
	{
		Season season = GetSeasonForLocation(currentLocation);
		int debrisToMake = random.Next(16, 64);
		int baseIndex = season switch
		{
			Season.Fall => 2, 
			Season.Winter => 3, 
			Season.Summer => 1, 
			_ => 0, 
		};
		isDebrisWeather = true;
		debrisWeatherSeason = season;
		debrisWeather.Clear();
		for (int i = 0; i < debrisToMake; i++)
		{
			debrisWeather.Add(new WeatherDebris(new Vector2(random.Next(0, viewport.Width), random.Next(0, viewport.Height)), baseIndex, (float)random.Next(15) / 500f, (float)random.Next(-10, 0) / 50f, (float)random.Next(10) / 50f));
		}
	}

	private static void OnNewSeason()
	{
		setGraphicsForSeason();
		Utility.ForEachLocation(delegate(GameLocation location)
		{
			location.seasonUpdate();
			return true;
		});
	}

	public static void prepareSpouseForWedding(Farmer farmer)
	{
		NPC nPC = RequireCharacter(farmer.spouse);
		nPC.ClearSchedule();
		nPC.DefaultMap = farmer.homeLocation.Value;
		nPC.DefaultPosition = Utility.PointToVector2(RequireLocation<FarmHouse>(farmer.homeLocation.Value).getSpouseBedSpot(farmer.spouse)) * 64f;
		nPC.DefaultFacingDirection = 2;
	}

	public static bool AddCharacterIfNecessary(string characterId, bool bypassConditions = false)
	{
		if (!NPC.TryGetData(characterId, out var data))
		{
			return false;
		}
		bool characterAdded = false;
		if (getCharacterFromName(characterId) == null)
		{
			if (!bypassConditions && !GameStateQuery.CheckConditions(data.UnlockConditions))
			{
				return false;
			}
			NPC.ReadNpcHomeData(data, null, out var homeName, out var homeTile, out var direction);
			bool datable = data.CanBeRomanced;
			Point size = data.Size;
			GameLocation homeLocation = getLocationFromNameInLocationsList(homeName);
			if (homeLocation == null)
			{
				return false;
			}
			string characterTextureName = NPC.getTextureNameForCharacter(characterId);
			NPC character;
			try
			{
				character = new NPC(new AnimatedSprite("Characters\\" + characterTextureName, 0, size.X, size.Y), new Vector2(homeTile.X * 64, homeTile.Y * 64), homeName, direction, characterId, datable, content.Load<Texture2D>("Portraits\\" + characterTextureName));
			}
			catch (Exception ex)
			{
				log.Error("Failed to spawn NPC '" + characterId + "'.", ex);
				return false;
			}
			character.Breather = data.Breather;
			homeLocation.addCharacter(character);
			characterAdded = true;
		}
		if (data.SocialTab == SocialTabBehavior.AlwaysShown && !player.friendshipData.ContainsKey(characterId))
		{
			player.friendshipData.Add(characterId, new Friendship());
		}
		return characterAdded;
	}

	public static GameLocation CreateGameLocation(string id)
	{
		if (string.IsNullOrWhiteSpace(id))
		{
			return null;
		}
		LocationData locationData;
		CreateLocationData createData = (DataLoader.Locations(content).TryGetValue(id, out locationData) ? locationData.CreateOnLoad : null);
		return CreateGameLocation(id, createData);
	}

	public static GameLocation CreateGameLocation(string id, CreateLocationData createData)
	{
		if (createData == null)
		{
			return null;
		}
		GameLocation location = ((createData.Type == null) ? new GameLocation(createData.MapPath, id) : ((GameLocation)Activator.CreateInstance(Type.GetType(createData.Type) ?? throw new Exception("Invalid type for location " + id + ": " + createData.Type), createData.MapPath, id)));
		location.isAlwaysActive.Value = createData.AlwaysActive;
		return location;
	}

	public static void AddLocations()
	{
		bool currentLocationSet = false;
		foreach (KeyValuePair<string, LocationData> pair in DataLoader.Locations(content))
		{
			if (pair.Value.CreateOnLoad == null)
			{
				continue;
			}
			GameLocation location;
			try
			{
				location = CreateGameLocation(pair.Key, pair.Value.CreateOnLoad);
			}
			catch (Exception ex)
			{
				log.Error("Couldn't create the '" + pair.Key + "' location. Is its data in Data/Locations invalid?", ex);
				continue;
			}
			if (location == null)
			{
				log.Error("Couldn't create the '" + pair.Key + "' location. Is its data in Data/Locations invalid?");
				continue;
			}
			if (!currentLocationSet)
			{
				try
				{
					location.map.LoadTileSheets(mapDisplayDevice);
					currentLocation = location;
					currentLocationSet = true;
				}
				catch (Exception ex)
				{
					log.Error("Couldn't load tilesheets for the '" + pair.Key + "' location.", ex);
				}
			}
			locations.Add(location);
		}
		for (int i = 1; i < netWorldState.Value.HighestPlayerLimit; i++)
		{
			GameLocation cellar = CreateGameLocation("Cellar");
			cellar.name.Value += i + 1;
			locations.Add(cellar);
		}
	}

	public static void AddNPCs()
	{
		foreach (KeyValuePair<string, CharacterData> entry in characterData)
		{
			if (entry.Value.SpawnIfMissing)
			{
				AddCharacterIfNecessary(entry.Key);
			}
		}
		GameLocation location = getLocationFromNameInLocationsList("QiNutRoom");
		if (location.getCharacterFromName("Mister Qi") == null)
		{
			AnimatedSprite sprite = new AnimatedSprite("Characters\\MrQi", 0, 16, 32);
			location.addCharacter(new NPC(sprite, new Vector2(448f, 256f), "QiNutRoom", 0, "Mister Qi", datable: false, content.Load<Texture2D>("Portraits\\MrQi")));
		}
	}

	public static void AddModNPCs()
	{
	}

	public static void fixProblems()
	{
		if (!IsMasterGame)
		{
			return;
		}
		Utility.ForEachLocation(delegate(GameLocation location)
		{
			location.characters.RemoveWhere(delegate(NPC npc)
			{
				if (npc == null)
				{
					log.Warn("Removed broken NPC in " + location.NameOrUniqueName + ": null instance.");
					return true;
				}
				if (npc.IsVillager && npc.GetData() == null)
				{
					try
					{
						if (npc.Sprite.Texture == null)
						{
							log.Warn($"Removed broken NPC '{npc.Name}' in {location.NameOrUniqueName}: villager with no data or sprites.");
							return true;
						}
					}
					catch
					{
						log.Warn($"Removed broken NPC '{npc.Name}' in {location.NameOrUniqueName}: villager with no data or sprites.");
						return true;
					}
				}
				return false;
			});
			return true;
		});
		AddNPCs();
		List<NPC> divorced = null;
		Utility.ForEachVillager(delegate(NPC n)
		{
			if (!n.datable.Value || n.getSpouse() != null)
			{
				return true;
			}
			if (n.DefaultMap == null || !n.DefaultMap.ToLower().Contains("cabin") || n.DefaultMap != "FarmHouse")
			{
				return true;
			}
			CharacterData data = n.GetData();
			if (data == null)
			{
				return true;
			}
			NPC.ReadNpcHomeData(data, n.currentLocation, out var locationName, out var _, out var _);
			if (n.DefaultMap != locationName)
			{
				if (divorced == null)
				{
					divorced = new List<NPC>();
				}
				divorced.Add(n);
			}
			return true;
		});
		if (divorced != null)
		{
			foreach (NPC n in divorced)
			{
				log.Warn("Fixing " + n.Name + " who was improperly divorced and left stranded");
				n.PerformDivorce();
			}
		}
		int playerCount = getAllFarmers().Count();
		Dictionary<Type, int> missingTools = new Dictionary<Type, int>();
		missingTools.Add(typeof(Axe), playerCount);
		missingTools.Add(typeof(Pickaxe), playerCount);
		missingTools.Add(typeof(Hoe), playerCount);
		missingTools.Add(typeof(WateringCan), playerCount);
		missingTools.Add(typeof(Wand), 0);
		foreach (Farmer allFarmer in getAllFarmers())
		{
			if (allFarmer.hasOrWillReceiveMail("ReturnScepter"))
			{
				missingTools[typeof(Wand)]++;
			}
		}
		int missingScythes = playerCount;
		foreach (Farmer who in getAllFarmers())
		{
			if (who.toolBeingUpgraded.Value != null)
			{
				if (who.toolBeingUpgraded.Value.Stack <= 0)
				{
					who.toolBeingUpgraded.Value.Stack = 1;
				}
				Type key = who.toolBeingUpgraded.Value.GetType();
				if (missingTools.TryGetValue(key, out var count))
				{
					missingTools[key] = count - 1;
				}
			}
			for (int i = 0; i < who.Items.Count; i++)
			{
				if (who.Items[i] != null)
				{
					checkIsMissingTool(missingTools, ref missingScythes, who.Items[i]);
				}
			}
		}
		bool allFound = true;
		foreach (int value in missingTools.Values)
		{
			if (value > 0)
			{
				allFound = false;
				break;
			}
		}
		if (missingScythes > 0)
		{
			allFound = false;
		}
		if (allFound)
		{
			return;
		}
		Utility.ForEachLocation(delegate(GameLocation l)
		{
			List<Debris> list = new List<Debris>();
			foreach (Debris current in l.debris)
			{
				Item item2 = current.item;
				if (item2 != null)
				{
					foreach (Type current2 in missingTools.Keys)
					{
						if (item2.GetType() == current2)
						{
							list.Add(current);
						}
					}
					if (item2.QualifiedItemId == "(W)47")
					{
						list.Add(current);
					}
				}
			}
			foreach (Debris current3 in list)
			{
				l.debris.Remove(current3);
			}
			return true;
		});
		Utility.iterateChestsAndStorage(delegate(Item item)
		{
			checkIsMissingTool(missingTools, ref missingScythes, item);
		});
		List<string> toAdd = new List<string>();
		foreach (KeyValuePair<Type, int> pair in missingTools)
		{
			if (pair.Value > 0)
			{
				for (int i = 0; i < pair.Value; i++)
				{
					toAdd.Add(pair.Key.ToString());
				}
			}
		}
		for (int i = 0; i < missingScythes; i++)
		{
			toAdd.Add("Scythe");
		}
		if (toAdd.Count > 0)
		{
			addMailForTomorrow("foundLostTools");
		}
		for (int i = 0; i < toAdd.Count; i++)
		{
			Item tool = null;
			switch (toAdd[i])
			{
			case "StardewValley.Tools.Axe":
				tool = ItemRegistry.Create("(T)Axe");
				break;
			case "StardewValley.Tools.Hoe":
				tool = ItemRegistry.Create("(T)Hoe");
				break;
			case "StardewValley.Tools.WateringCan":
				tool = ItemRegistry.Create("(T)WateringCan");
				break;
			case "Scythe":
				tool = ItemRegistry.Create("(W)47");
				break;
			case "StardewValley.Tools.Pickaxe":
				tool = ItemRegistry.Create("(T)Pickaxe");
				break;
			case "StardewValley.Tools.Wand":
				tool = ItemRegistry.Create("(T)ReturnScepter");
				break;
			}
			if (tool != null)
			{
				if (newDaySync.hasInstance())
				{
					player.team.newLostAndFoundItems.Value = true;
				}
				player.team.returnedDonations.Add(tool);
			}
		}
	}

	private static void checkIsMissingTool(Dictionary<Type, int> missingTools, ref int missingScythes, Item item)
	{
		foreach (Type key in missingTools.Keys)
		{
			if (item.GetType() == key)
			{
				missingTools[key]--;
			}
		}
		if (item.QualifiedItemId == "(W)47")
		{
			missingScythes--;
		}
	}

	public static void newDayAfterFade(Action after)
	{
		if (player.currentLocation != null)
		{
			if (player.rightRing.Value != null)
			{
				player.rightRing.Value.onLeaveLocation(player, player.currentLocation);
			}
			if (player.leftRing.Value != null)
			{
				player.leftRing.Value.onLeaveLocation(player, player.currentLocation);
			}
		}
		if (LocalMultiplayer.IsLocalMultiplayer())
		{
			hooks.OnGame1_NewDayAfterFade(delegate
			{
				game1.isLocalMultiplayerNewDayActive = true;
				_afterNewDayAction = after;
				GameRunner.instance.activeNewDayProcesses.Add(new KeyValuePair<Game1, IEnumerator<int>>(game1, _newDayAfterFade()));
			});
			return;
		}
		hooks.OnGame1_NewDayAfterFade(delegate
		{
			_afterNewDayAction = after;
			if (_newDayTask != null)
			{
				log.Warn("Warning: There is already a _newDayTask; unusual code path.\n" + Environment.StackTrace);
			}
			else
			{
				_newDayTask = new Task(delegate
				{
					Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
					IEnumerator<int> enumerator = _newDayAfterFade();
					while (enumerator.MoveNext())
					{
					}
				});
			}
		});
	}

	public static bool CanAcceptDailyQuest()
	{
		if (questOfTheDay == null)
		{
			return false;
		}
		if (player.acceptedDailyQuest.Value)
		{
			return false;
		}
		if (questOfTheDay.questDescription == null || questOfTheDay.questDescription.Length == 0)
		{
			return false;
		}
		return true;
	}

	private static IEnumerator<int> _newDayAfterFade()
	{
		TriggerActionManager.Raise("DayEnding");
		newDaySync.start();
		while (!newDaySync.hasStarted())
		{
			yield return 0;
		}
		int timeWentToSleep = timeOfDay;
		newDaySync.barrier("start");
		while (!newDaySync.isBarrierReady("start"))
		{
			yield return 0;
		}
		int overnightMinutesElapsed = Utility.CalculateMinutesUntilMorning(timeWentToSleep);
		stats.AverageBedtime = (uint)timeWentToSleep;
		if (IsMasterGame)
		{
			dayOfMonth++;
			stats.DaysPlayed++;
			if (dayOfMonth > 28)
			{
				dayOfMonth = 1;
				switch (season)
				{
				case Season.Spring:
					season = Season.Summer;
					break;
				case Season.Summer:
					season = Season.Fall;
					break;
				case Season.Fall:
					season = Season.Winter;
					break;
				case Season.Winter:
					season = Season.Spring;
					year++;
					MineShaft.yearUpdate();
					break;
				}
			}
			timeOfDay = 600;
			netWorldState.Value.UpdateFromGame1();
		}
		newDaySync.barrier("date");
		while (!newDaySync.isBarrierReady("date"))
		{
			yield return 0;
		}
		player.dayOfMonthForSaveGame = dayOfMonth;
		player.seasonForSaveGame = seasonIndex;
		player.yearForSaveGame = year;
		flushLocationLookup();
		Event.OnNewDay();
		try
		{
			fixProblems();
		}
		catch (Exception)
		{
		}
		foreach (Farmer allFarmer in getAllFarmers())
		{
			allFarmer.FarmerSprite.PauseForSingleAnimation = false;
		}
		whereIsTodaysFest = null;
		if (wind != null)
		{
			wind.Stop(AudioStopOptions.Immediate);
			wind = null;
		}
		foreach (int key in new List<int>(player.chestConsumedMineLevels.Keys))
		{
			if (key > 120)
			{
				player.chestConsumedMineLevels.Remove(key);
			}
		}
		player.currentEyes = 0;
		int seed;
		if (IsMasterGame)
		{
			player.team.announcedSleepingFarmers.Clear();
			seed = (int)uniqueIDForThisGame / 100 + (int)(stats.DaysPlayed * 10) + 1 + (int)stats.StepsTaken;
			newDaySync.sendVar<NetInt, int>("seed", seed);
		}
		else
		{
			while (!newDaySync.isVarReady("seed"))
			{
				yield return 0;
			}
			seed = newDaySync.waitForVar<NetInt, int>("seed");
		}
		random = Utility.CreateRandom(seed);
		for (int i = 0; i < dayOfMonth; i++)
		{
			random.Next();
		}
		player.team.endOfNightStatus.UpdateState("sleep");
		newDaySync.barrier("sleep");
		while (!newDaySync.isBarrierReady("sleep"))
		{
			yield return 0;
		}
		gameTimeInterval = 0;
		game1.wasAskedLeoMemory = false;
		player.team.Update();
		player.team.NewDay();
		player.passedOut = false;
		player.CanMove = true;
		player.FarmerSprite.PauseForSingleAnimation = false;
		player.FarmerSprite.StopAnimation();
		player.completelyStopAnimatingOrDoingAction();
		changeMusicTrack("silence");
		if (IsMasterGame)
		{
			UpdateDishOfTheDay();
		}
		newDaySync.barrier("dishOfTheDay");
		while (!newDaySync.isBarrierReady("dishOfTheDay"))
		{
			yield return 0;
		}
		npcDialogues = null;
		Utility.ForEachCharacter(delegate(NPC n)
		{
			n.updatedDialogueYet = false;
			return true;
		});
		Utility.ForEachLocation(delegate(GameLocation location)
		{
			location.currentEvent = null;
			if (IsMasterGame)
			{
				location.passTimeForObjects(overnightMinutesElapsed);
			}
			return true;
		});
		outdoorLight = Color.White;
		ambientLight = Color.White;
		if (isLightning && IsMasterGame)
		{
			Utility.overnightLightning(timeWentToSleep);
		}
		if (MasterPlayer.hasOrWillReceiveMail("ccBulletinThankYou") && !player.hasOrWillReceiveMail("ccBulletinThankYou"))
		{
			addMailForTomorrow("ccBulletinThankYou");
		}
		ReceiveMailForTomorrow();
		if (Utility.TryGetRandom(player.friendshipData, out var whichFriend, out var friendship) && random.NextBool((double)(friendship.Points / 250) * 0.1) && player.spouse != whichFriend && DataLoader.Mail(content).ContainsKey(whichFriend))
		{
			mailbox.Add(whichFriend);
		}
		MineShaft.clearActiveMines();
		VolcanoDungeon.ClearAllLevels();
		netWorldState.Value.CheckedGarbage.Clear();
		for (int i = player.enchantments.Count - 1; i >= 0; i--)
		{
			player.enchantments[i].OnUnequip(player);
		}
		player.dayupdate(timeWentToSleep);
		if (IsMasterGame)
		{
			player.team.sharedDailyLuck.Value = Math.Min(0.10000000149011612, (double)random.Next(-100, 101) / 1000.0);
		}
		player.showToolUpgradeAvailability();
		if (IsMasterGame)
		{
			queueWeddingsForToday();
			newDaySync.sendVar<NetRef<NetLongList>, NetLongList>("weddingsToday", new NetLongList(weddingsToday));
		}
		else
		{
			while (!newDaySync.isVarReady("weddingsToday"))
			{
				yield return 0;
			}
			weddingsToday = new List<long>(newDaySync.waitForVar<NetRef<NetLongList>, NetLongList>("weddingsToday"));
		}
		weddingToday = false;
		foreach (long item2 in weddingsToday)
		{
			Farmer spouse_farmer = getFarmer(item2);
			if (spouse_farmer != null && !spouse_farmer.hasCurrentOrPendingRoommate())
			{
				weddingToday = true;
				break;
			}
		}
		if (player.spouse != null && player.isEngaged() && weddingsToday.Contains(player.UniqueMultiplayerID))
		{
			Friendship friendship2 = player.friendshipData[player.spouse];
			friendship2.Status = FriendshipStatus.Married;
			friendship2.WeddingDate = new WorldDate(Date);
			prepareSpouseForWedding(player);
			if (!player.getSpouse().isRoommate())
			{
				player.autoGenerateActiveDialogueEvent("married_" + player.spouse);
				if (!player.autoGenerateActiveDialogueEvent("married"))
				{
					player.autoGenerateActiveDialogueEvent("married_twice");
				}
			}
			else
			{
				player.autoGenerateActiveDialogueEvent("roommates_" + player.spouse);
			}
		}
		NetLongDictionary<NetList<Item, NetRef<Item>>, NetRef<NetList<Item, NetRef<Item>>>> additional_shipped_items = new NetLongDictionary<NetList<Item, NetRef<Item>>, NetRef<NetList<Item, NetRef<Item>>>>();
		if (IsMasterGame)
		{
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				foreach (Object value in location.objects.Values)
				{
					if (value is Chest { SpecialChestType: Chest.SpecialChestTypes.MiniShippingBin } chest)
					{
						chest.clearNulls();
						if ((bool)player.team.useSeparateWallets)
						{
							foreach (long current2 in chest.separateWalletItems.Keys)
							{
								if (!additional_shipped_items.ContainsKey(current2))
								{
									additional_shipped_items[current2] = new NetList<Item, NetRef<Item>>();
								}
								List<Item> list = new List<Item>(chest.separateWalletItems[current2]);
								chest.separateWalletItems[current2].Clear();
								foreach (Item current3 in list)
								{
									current3.onDetachedFromParent();
									additional_shipped_items[current2].Add(current3);
								}
							}
						}
						else
						{
							IInventory shippingBin2 = getFarm().getShippingBin(player);
							shippingBin2.RemoveEmptySlots();
							foreach (Item current4 in chest.Items)
							{
								current4.onDetachedFromParent();
								shippingBin2.Add(current4);
							}
						}
						chest.Items.Clear();
						chest.separateWalletItems.Clear();
					}
				}
				return true;
			});
		}
		if (IsMasterGame)
		{
			newDaySync.sendVar<NetRef<NetLongDictionary<NetList<Item, NetRef<Item>>, NetRef<NetList<Item, NetRef<Item>>>>>, NetLongDictionary<NetList<Item, NetRef<Item>>, NetRef<NetList<Item, NetRef<Item>>>>>("additional_shipped_items", additional_shipped_items);
		}
		else
		{
			while (!newDaySync.isVarReady("additional_shipped_items"))
			{
				yield return 0;
			}
			additional_shipped_items = newDaySync.waitForVar<NetRef<NetLongDictionary<NetList<Item, NetRef<Item>>, NetRef<NetList<Item, NetRef<Item>>>>>, NetLongDictionary<NetList<Item, NetRef<Item>>, NetRef<NetList<Item, NetRef<Item>>>>>("additional_shipped_items");
		}
		if (player.team.useSeparateWallets.Value)
		{
			IInventory shipping_bin = getFarm().getShippingBin(player);
			if (additional_shipped_items.TryGetValue(player.UniqueMultiplayerID, out var item_list))
			{
				foreach (Item item in item_list)
				{
					shipping_bin.Add(item);
				}
			}
		}
		newDaySync.barrier("handleMiniShippingBins");
		while (!newDaySync.isBarrierReady("handleMiniShippingBins"))
		{
			yield return 0;
		}
		IInventory shippingBin = getFarm().getShippingBin(player);
		shippingBin.RemoveEmptySlots();
		foreach (Item i in shippingBin)
		{
			player.displayedShippedItems.Add(i);
		}
		if (player.useSeparateWallets || player.IsMainPlayer)
		{
			int total = 0;
			foreach (Item item in shippingBin)
			{
				int item_value = 0;
				if (item is Object obj)
				{
					item_value = obj.sellToStorePrice(-1L) * obj.Stack;
					total += item_value;
				}
				if (player.team.specialOrders == null)
				{
					continue;
				}
				foreach (SpecialOrder specialOrder in player.team.specialOrders)
				{
					specialOrder.onItemShipped?.Invoke(player, item, item_value);
				}
			}
			player.Money += total;
		}
		if (IsMasterGame)
		{
			if (IsWinter && dayOfMonth == 18)
			{
				GameLocation source = RequireLocation("Submarine");
				if (source.objects.Length >= 0)
				{
					Utility.transferPlacedObjectsFromOneLocationToAnother(source, null, new Vector2(20f, 20f), getLocationFromName("Beach"));
				}
				source = RequireLocation("MermaidHouse");
				if (source.objects.Length >= 0)
				{
					Utility.transferPlacedObjectsFromOneLocationToAnother(source, null, new Vector2(21f, 20f), getLocationFromName("Beach"));
				}
			}
			if (player.hasOrWillReceiveMail("pamHouseUpgrade") && !player.hasOrWillReceiveMail("transferredObjectsPamHouse"))
			{
				addMailForTomorrow("transferredObjectsPamHouse", noLetter: true);
				GameLocation source = RequireLocation("Trailer");
				GameLocation destination = getLocationFromName("Trailer_Big");
				if (source.objects.Length >= 0)
				{
					Utility.transferPlacedObjectsFromOneLocationToAnother(source, destination, new Vector2(14f, 23f));
				}
			}
			if (Utility.HasAnyPlayerSeenEvent("191393") && !player.hasOrWillReceiveMail("transferredObjectsJojaMart"))
			{
				addMailForTomorrow("transferredObjectsJojaMart", noLetter: true);
				GameLocation source = RequireLocation("JojaMart");
				if (source.objects.Length >= 0)
				{
					Utility.transferPlacedObjectsFromOneLocationToAnother(source, null, new Vector2(89f, 51f), getLocationFromName("Town"));
				}
			}
		}
		if (player.useSeparateWallets && player.IsMainPlayer)
		{
			foreach (Farmer who in getOfflineFarmhands())
			{
				if (who.isUnclaimedFarmhand)
				{
					continue;
				}
				int total = 0;
				IInventory farmhandShippingBin = getFarm().getShippingBin(who);
				farmhandShippingBin.RemoveEmptySlots();
				foreach (Item item in farmhandShippingBin)
				{
					int item_value = 0;
					if (item is Object obj)
					{
						item_value = obj.sellToStorePrice(who.UniqueMultiplayerID) * obj.Stack;
						total += item_value;
					}
					if (player.team.specialOrders == null)
					{
						continue;
					}
					foreach (SpecialOrder specialOrder2 in player.team.specialOrders)
					{
						specialOrder2.onItemShipped?.Invoke(player, item, item_value);
					}
				}
				player.team.AddIndividualMoney(who, total);
				farmhandShippingBin.Clear();
			}
		}
		List<NPC> divorceNPCs = new List<NPC>();
		if (IsMasterGame)
		{
			foreach (Farmer who in getAllFarmers())
			{
				if (who.isActive() && (bool)who.divorceTonight && who.getSpouse() != null)
				{
					divorceNPCs.Add(who.getSpouse());
				}
			}
		}
		newDaySync.barrier("player.dayupdate");
		while (!newDaySync.isBarrierReady("player.dayupdate"))
		{
			yield return 0;
		}
		if ((bool)player.divorceTonight)
		{
			player.doDivorce();
		}
		newDaySync.barrier("player.divorce");
		while (!newDaySync.isBarrierReady("player.divorce"))
		{
			yield return 0;
		}
		if (IsMasterGame)
		{
			foreach (NPC npc in divorceNPCs)
			{
				if (npc.getSpouse() == null)
				{
					npc.PerformDivorce();
				}
			}
		}
		newDaySync.barrier("player.finishDivorce");
		while (!newDaySync.isBarrierReady("player.finishDivorce"))
		{
			yield return 0;
		}
		if (IsMasterGame && (bool)player.changeWalletTypeTonight)
		{
			if (player.useSeparateWallets)
			{
				ManorHouse.MergeWallets();
			}
			else
			{
				ManorHouse.SeparateWallets();
			}
		}
		newDaySync.barrier("player.wallets");
		while (!newDaySync.isBarrierReady("player.wallets"))
		{
			yield return 0;
		}
		getFarm().lastItemShipped = null;
		getFarm().getShippingBin(player).Clear();
		newDaySync.barrier("clearShipping");
		while (!newDaySync.isBarrierReady("clearShipping"))
		{
			yield return 0;
		}
		if (IsClient)
		{
			multiplayer.sendFarmhand();
			newDaySync.processMessages();
		}
		newDaySync.barrier("sendFarmhands");
		while (!newDaySync.isBarrierReady("sendFarmhands"))
		{
			yield return 0;
		}
		if (IsMasterGame)
		{
			multiplayer.saveFarmhands();
		}
		newDaySync.barrier("saveFarmhands");
		while (!newDaySync.isBarrierReady("saveFarmhands"))
		{
			yield return 0;
		}
		if (IsMasterGame)
		{
			UpdatePassiveFestivalStates();
			if (Utility.IsPassiveFestivalDay("NightMarket") && IsMasterGame && netWorldState.Value.VisitsUntilY1Guarantee >= 0)
			{
				netWorldState.Value.VisitsUntilY1Guarantee--;
			}
		}
		if (dayOfMonth == 1)
		{
			OnNewSeason();
		}
		if (IsMasterGame && (dayOfMonth == 1 || dayOfMonth == 8 || dayOfMonth == 15 || dayOfMonth == 22))
		{
			SpecialOrder.UpdateAvailableSpecialOrders("", forceRefresh: true);
			SpecialOrder.UpdateAvailableSpecialOrders("Qi", forceRefresh: true);
		}
		if (IsMasterGame)
		{
			netWorldState.Value.UpdateFromGame1();
		}
		newDaySync.barrier("specialOrders");
		while (!newDaySync.isBarrierReady("specialOrders"))
		{
			yield return 0;
		}
		if (IsMasterGame)
		{
			for (int i = 0; i < player.team.specialOrders.Count; i++)
			{
				SpecialOrder order = player.team.specialOrders[i];
				if (order.questState.Value != SpecialOrderStatus.Complete && order.GetDaysLeft() <= 0)
				{
					order.OnFail();
					player.team.specialOrders.RemoveAt(i);
					i--;
				}
			}
		}
		newDaySync.barrier("processOrders");
		while (!newDaySync.isBarrierReady("processOrders"))
		{
			yield return 0;
		}
		if (IsMasterGame)
		{
			foreach (string item3 in player.team.specialRulesRemovedToday)
			{
				SpecialOrder.RemoveSpecialRuleAtEndOfDay(item3);
			}
		}
		player.team.specialRulesRemovedToday.Clear();
		if (DataLoader.Mail(content).ContainsKey(currentSeason + "_" + dayOfMonth + "_" + year))
		{
			mailbox.Add(currentSeason + "_" + dayOfMonth + "_" + year);
		}
		else if (DataLoader.Mail(content).ContainsKey(currentSeason + "_" + dayOfMonth))
		{
			mailbox.Add(currentSeason + "_" + dayOfMonth);
		}
		if (MasterPlayer.mailReceived.Contains("ccVault") && IsSpring && dayOfMonth == 14)
		{
			mailbox.Add("DesertFestival");
		}
		if (IsMasterGame)
		{
			if (player.team.toggleMineShrineOvernight.Value)
			{
				player.team.toggleMineShrineOvernight.Value = false;
				player.team.mineShrineActivated.Value = !player.team.mineShrineActivated.Value;
				if (player.team.mineShrineActivated.Value)
				{
					netWorldState.Value.MinesDifficulty++;
				}
				else
				{
					netWorldState.Value.MinesDifficulty--;
				}
			}
			if (player.team.toggleSkullShrineOvernight.Value)
			{
				player.team.toggleSkullShrineOvernight.Value = false;
				player.team.skullShrineActivated.Value = !player.team.skullShrineActivated.Value;
				if (player.team.skullShrineActivated.Value)
				{
					netWorldState.Value.SkullCavesDifficulty++;
				}
				else
				{
					netWorldState.Value.SkullCavesDifficulty--;
				}
			}
		}
		if (IsMasterGame)
		{
			if (!player.team.SpecialOrderRuleActive("MINE_HARD") && netWorldState.Value.MinesDifficulty > 1)
			{
				netWorldState.Value.MinesDifficulty = 1;
			}
			if (!player.team.SpecialOrderRuleActive("SC_HARD") && netWorldState.Value.SkullCavesDifficulty > 1)
			{
				netWorldState.Value.SkullCavesDifficulty = 1;
			}
		}
		if (IsMasterGame)
		{
			RefreshQuestOfTheDay();
		}
		newDaySync.barrier("questOfTheDay");
		while (!newDaySync.isBarrierReady("questOfTheDay"))
		{
			yield return 0;
		}
		bool yesterdayWasGreenRain = wasGreenRain;
		wasGreenRain = false;
		UpdateWeatherForNewDay();
		newDaySync.barrier("updateWeather");
		while (!newDaySync.isBarrierReady("updateWeather"))
		{
			yield return 0;
		}
		ApplyWeatherForNewDay();
		if (isGreenRain)
		{
			morningQueue.Enqueue(delegate
			{
				showGlobalMessage(content.LoadString("Strings\\1_6_Strings:greenrainmessage"));
			});
			if (year == 1 && !player.hasOrWillReceiveMail("GreenRainGus"))
			{
				mailbox.Add("GreenRainGus");
			}
			if (IsMasterGame)
			{
				Utility.ForEachLocation(delegate(GameLocation location)
				{
					location.performGreenRainUpdate();
					return true;
				});
			}
		}
		else if (yesterdayWasGreenRain)
		{
			if (IsMasterGame)
			{
				Utility.ForEachLocation(delegate(GameLocation location)
				{
					location.performDayAfterGreenRainUpdate();
					return true;
				});
			}
			if (year == 1)
			{
				player.activeDialogueEvents.TryAdd("GreenRainFinished", 1);
			}
		}
		if (Utility.getDaysOfBooksellerThisSeason().Contains(dayOfMonth))
		{
			addMorningFluffFunction(delegate
			{
				showGlobalMessage(content.LoadString("Strings\\1_6_Strings:BooksellerInTown"));
			});
		}
		WeatherDebris.globalWind = 0f;
		windGust = 0f;
		AddNPCs();
		Utility.ForEachVillager(delegate(NPC n)
		{
			player.mailReceived.Remove(n.Name);
			player.mailReceived.Remove(n.Name + "Cooking");
			n.drawOffset = Vector2.Zero;
			if (!IsMasterGame)
			{
				n.ChooseAppearance();
			}
			return true;
		});
		FarmAnimal.reservedGrass.Clear();
		if (IsMasterGame)
		{
			NPC.hasSomeoneRepairedTheFences = false;
			NPC.hasSomeoneFedTheAnimals = false;
			NPC.hasSomeoneFedThePet = false;
			NPC.hasSomeoneWateredCrops = false;
			foreach (GameLocation location in locations)
			{
				location.ResetCharacterDialogues();
				location.DayUpdate(dayOfMonth);
			}
			netWorldState.Value.UpdateUnderConstruction();
			UpdateHorseOwnership();
			foreach (NPC n in Utility.getAllCharacters())
			{
				if (n.IsVillager)
				{
					n.islandScheduleName.Value = null;
					n.currentScheduleDelay = 0f;
				}
				n.dayUpdate(dayOfMonth);
			}
			IslandSouth.SetupIslandSchedules();
			HashSet<NPC> purchased_item_npcs = new HashSet<NPC>();
			UpdateShopPlayerItemInventory("SeedShop", purchased_item_npcs);
			UpdateShopPlayerItemInventory("FishShop", purchased_item_npcs);
		}
		if (IsMasterGame && netWorldState.Value.GetWeatherForLocation("Island").IsRaining)
		{
			Vector2 tile_location = new Vector2(0f, 0f);
			IslandLocation island_location = null;
			List<int> order = new List<int>();
			for (int i = 0; i < 4; i++)
			{
				order.Add(i);
			}
			Utility.Shuffle(Utility.CreateRandom(uniqueIDForThisGame), order);
			switch (order[currentGemBirdIndex])
			{
			case 0:
				island_location = getLocationFromName("IslandSouth") as IslandLocation;
				tile_location = new Vector2(10f, 30f);
				break;
			case 1:
				island_location = getLocationFromName("IslandNorth") as IslandLocation;
				tile_location = new Vector2(56f, 56f);
				break;
			case 2:
				island_location = getLocationFromName("Islandwest") as IslandLocation;
				tile_location = new Vector2(53f, 51f);
				break;
			case 3:
				island_location = getLocationFromName("IslandEast") as IslandLocation;
				tile_location = new Vector2(21f, 35f);
				break;
			}
			currentGemBirdIndex = (currentGemBirdIndex + 1) % 4;
			if (island_location != null)
			{
				island_location.locationGemBird.Value = new IslandGemBird(tile_location, IslandGemBird.GetBirdTypeForLocation(island_location.Name));
			}
		}
		if (IsMasterGame)
		{
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				if (location.IsOutdoors && location.IsRainingHere())
				{
					foreach (Building building in location.buildings)
					{
						if (building is PetBowl petBowl)
						{
							petBowl.watered.Value = true;
						}
					}
					foreach (KeyValuePair<Vector2, TerrainFeature> pair2 in location.terrainFeatures.Pairs)
					{
						if (pair2.Value is HoeDirt hoeDirt && (int)hoeDirt.state != 2)
						{
							hoeDirt.state.Value = 1;
						}
					}
				}
				return true;
			});
		}
		WorldDate yesterday = new WorldDate(Date);
		yesterday.TotalDays--;
		foreach (KeyValuePair<string, PassiveFestivalData> pair in DataLoader.PassiveFestivals(content))
		{
			string id = pair.Key;
			PassiveFestivalData festival = pair.Value;
			if (yesterday.DayOfMonth == festival.EndDay && yesterday.Season == festival.Season && GameStateQuery.CheckConditions(festival.Condition) && festival != null && festival.CleanupMethod != null)
			{
				if (StaticDelegateBuilder.TryCreateDelegate<FestivalCleanupDelegate>(festival.CleanupMethod, out var method, out var error))
				{
					method();
					continue;
				}
				log.Warn($"Passive festival '{id}' has invalid cleanup method '{festival.CleanupMethod}': {error}");
			}
		}
		PerformPassiveFestivalSetup();
		newDaySync.barrier("buildingUpgrades");
		while (!newDaySync.isBarrierReady("buildingUpgrades"))
		{
			yield return 0;
		}
		List<string> mailToRemoveOvernight = new List<string>(player.team.mailToRemoveOvernight);
		foreach (string index in new List<string>(player.team.itemsToRemoveOvernight))
		{
			if (IsMasterGame)
			{
				game1._PerformRemoveNormalItemFromWorldOvernight(index);
				foreach (Farmer farmer in getOfflineFarmhands())
				{
					game1._PerformRemoveNormalItemFromFarmerOvernight(farmer, index);
				}
			}
			game1._PerformRemoveNormalItemFromFarmerOvernight(player, index);
		}
		foreach (string mail_key in mailToRemoveOvernight)
		{
			if (IsMasterGame)
			{
				foreach (Farmer farmer in getAllFarmers())
				{
					farmer.RemoveMail(mail_key, farmer == MasterPlayer);
				}
			}
			else
			{
				player.RemoveMail(mail_key);
			}
		}
		newDaySync.barrier("removeItemsFromWorld");
		while (!newDaySync.isBarrierReady("removeItemsFromWorld"))
		{
			yield return 0;
		}
		if (IsMasterGame)
		{
			player.team.itemsToRemoveOvernight.Clear();
			player.team.mailToRemoveOvernight.Clear();
		}
		newDay = false;
		if (IsMasterGame)
		{
			netWorldState.Value.UpdateFromGame1();
		}
		if (player.currentLocation != null)
		{
			player.currentLocation.resetForPlayerEntry();
			BedFurniture.ApplyWakeUpPosition(player);
			forceSnapOnNextViewportUpdate = true;
			UpdateViewPort(overrideFreeze: false, player.StandingPixel);
			previousViewportPosition = new Vector2(viewport.X, viewport.Y);
		}
		displayFarmer = true;
		updateWeatherIcon();
		freezeControls = false;
		if (stats.DaysPlayed > 1 || !IsMasterGame)
		{
			farmEvent = null;
			if (IsMasterGame)
			{
				farmEvent = Utility.pickFarmEvent() ?? farmEventOverride;
				farmEventOverride = null;
				newDaySync.sendVar<NetRef<FarmEvent>, FarmEvent>("farmEvent", farmEvent);
			}
			else
			{
				while (!newDaySync.isVarReady("farmEvent"))
				{
					yield return 0;
				}
				farmEvent = newDaySync.waitForVar<NetRef<FarmEvent>, FarmEvent>("farmEvent");
			}
			if (farmEvent == null)
			{
				farmEvent = Utility.pickPersonalFarmEvent();
			}
			if (farmEvent != null && farmEvent.setUp())
			{
				farmEvent = null;
			}
		}
		if (farmEvent == null)
		{
			RemoveDeliveredMailForTomorrow();
		}
		if (player.team.newLostAndFoundItems.Value)
		{
			morningQueue.Enqueue(delegate
			{
				showGlobalMessage(content.LoadString("Strings\\StringsFromCSFiles:NewLostAndFoundItems"));
			});
		}
		newDaySync.barrier("mail");
		while (!newDaySync.isBarrierReady("mail"))
		{
			yield return 0;
		}
		if (IsMasterGame)
		{
			player.team.newLostAndFoundItems.Value = false;
		}
		Utility.ForEachBuilding(delegate(Building building)
		{
			if (building.GetIndoors() is Cabin)
			{
				player.slotCanHost = true;
				return false;
			}
			return true;
		});
		if (Utility.percentGameComplete() + (float)netWorldState.Value.PerfectionWaivers * 0.01f >= 1f)
		{
			player.team.farmPerfect.Value = true;
		}
		newDaySync.barrier("checkcompletion");
		while (!newDaySync.isBarrierReady("checkcompletion"))
		{
			yield return 0;
		}
		UpdateFarmPerfection();
		if (farmEvent == null)
		{
			handlePostFarmEventActions();
			showEndOfNightStuff();
		}
		if (server != null)
		{
			server.updateLobbyData();
		}
	}

	/// <summary>Reset the Saloon's dish of the day.</summary>
	public static void UpdateDishOfTheDay()
	{
		string itemId;
		do
		{
			itemId = random.Next(194, 240).ToString();
		}
		while (Utility.IsForbiddenDishOfTheDay(itemId));
		int count = random.Next(1, 4 + ((random.NextDouble() < 0.08) ? 10 : 0));
		dishOfTheDay = ItemRegistry.Create<Object>("(O)" + itemId, count);
	}

	/// <summary>Apply updates overnight if this save has completed perfection.</summary>
	/// <remarks>See also <see cref="M:StardewValley.Utility.percentGameComplete" /> to check if the save has reached perfection.</remarks>
	public static void UpdateFarmPerfection()
	{
		if (MasterPlayer.mailReceived.Contains("Farm_Eternal") || (!MasterPlayer.hasCompletedCommunityCenter() && !Utility.hasFinishedJojaRoute()) || !player.team.farmPerfect.Value)
		{
			return;
		}
		addMorningFluffFunction(delegate
		{
			changeMusicTrack("none", track_interruptable: true);
			if (IsMasterGame)
			{
				multiplayer.globalChatInfoMessageEvenInSinglePlayer("Eternal1");
			}
			playSound("discoverMineral");
			if (IsMasterGame)
			{
				DelayedAction.functionAfterDelay(delegate
				{
					multiplayer.globalChatInfoMessageEvenInSinglePlayer("Eternal2", MasterPlayer.farmName);
				}, 4000);
			}
			player.mailReceived.Add("Farm_Eternal");
			DelayedAction.functionAfterDelay(delegate
			{
				playSound("thunder_small");
				if (IsMultiplayer)
				{
					if (IsMasterGame)
					{
						multiplayer.globalChatInfoMessage("Eternal3");
					}
				}
				else
				{
					showGlobalMessage(content.LoadString("Strings\\UI:Chat_Eternal3"));
				}
			}, 12000);
		});
	}

	/// <summary>Get whether it's green raining in the given location's context (regardless of whether the player is currently indoors and sheltered from the green rain).</summary>
	/// <param name="location">The location to check, or <c>null</c> to use <see cref="P:StardewValley.Game1.currentLocation" />.</param>
	public static bool IsGreenRainingHere(GameLocation location = null)
	{
		if (location == null)
		{
			location = currentLocation;
		}
		if (location != null && netWorldState != null)
		{
			return location.IsGreenRainingHere();
		}
		return false;
	}

	/// <summary>Get whether it's raining in the given location's context (regardless of whether the player is currently indoors and sheltered from the rain).</summary>
	/// <param name="location">The location to check, or <c>null</c> to use <see cref="P:StardewValley.Game1.currentLocation" />.</param>
	public static bool IsRainingHere(GameLocation location = null)
	{
		if (location == null)
		{
			location = currentLocation;
		}
		if (location != null && netWorldState != null)
		{
			return location.IsRainingHere();
		}
		return false;
	}

	/// <summary>Get whether it's storming in the given location's context (regardless of whether the player is currently indoors and sheltered from the storm).</summary>
	/// <param name="location">The location to check, or <c>null</c> to use <see cref="P:StardewValley.Game1.currentLocation" />.</param>
	public static bool IsLightningHere(GameLocation location = null)
	{
		if (location == null)
		{
			location = currentLocation;
		}
		if (location != null && netWorldState != null)
		{
			return location.IsLightningHere();
		}
		return false;
	}

	/// <summary>Get whether it's snowing in the given location's context (regardless of whether the player is currently indoors and sheltered from the snow).</summary>
	/// <param name="location">The location to check, or <c>null</c> to use <see cref="P:StardewValley.Game1.currentLocation" />.</param>
	public static bool IsSnowingHere(GameLocation location = null)
	{
		if (location == null)
		{
			location = currentLocation;
		}
		if (location != null && netWorldState != null)
		{
			return location.IsSnowingHere();
		}
		return false;
	}

	/// <summary>Get whether it's blowing debris like leaves in the given location's context (regardless of whether the player is currently indoors and sheltered from the wind).</summary>
	/// <param name="location">The location to check, or <c>null</c> to use <see cref="P:StardewValley.Game1.currentLocation" />.</param>
	public static bool IsDebrisWeatherHere(GameLocation location = null)
	{
		if (location == null)
		{
			location = currentLocation;
		}
		if (location != null && netWorldState != null)
		{
			return location.IsDebrisWeatherHere();
		}
		return false;
	}

	public static string getWeatherModificationsForDate(WorldDate date, string default_weather)
	{
		string weather = default_weather;
		int day_offset = date.TotalDays - Date.TotalDays;
		if (date.DayOfMonth == 1 || stats.DaysPlayed + day_offset <= 4)
		{
			weather = "Sun";
		}
		if (stats.DaysPlayed + day_offset == 3)
		{
			weather = "Rain";
		}
		if (Utility.isGreenRainDay(date.DayOfMonth, date.Season))
		{
			weather = "GreenRain";
		}
		if (date.Season == Season.Summer && date.DayOfMonth % 13 == 0)
		{
			weather = "Storm";
		}
		if (Utility.isFestivalDay(date.DayOfMonth, date.Season))
		{
			weather = "Festival";
		}
		foreach (PassiveFestivalData festival in DataLoader.PassiveFestivals(content).Values)
		{
			if (date.DayOfMonth < festival.StartDay || date.DayOfMonth > festival.EndDay || date.Season != festival.Season || !GameStateQuery.CheckConditions(festival.Condition) || festival.MapReplacements == null)
			{
				continue;
			}
			foreach (string key in festival.MapReplacements.Keys)
			{
				GameLocation replacedLocation = getLocationFromName(key);
				if (replacedLocation != null && replacedLocation.InValleyContext())
				{
					weather = "Sun";
					break;
				}
			}
		}
		return weather;
	}

	public static void UpdateWeatherForNewDay()
	{
		weatherForTomorrow = getWeatherModificationsForDate(Date, weatherForTomorrow);
		if (weddingToday)
		{
			weatherForTomorrow = "Wedding";
		}
		if (IsMasterGame)
		{
			netWorldState.Value.GetWeatherForLocation("Default").WeatherForTomorrow = weatherForTomorrow;
		}
		wasRainingYesterday = isRaining || isLightning;
		debrisWeather.Clear();
		if (!IsMasterGame)
		{
			return;
		}
		foreach (KeyValuePair<string, LocationContextData> pair in locationContextData)
		{
			netWorldState.Value.GetWeatherForLocation(pair.Key).UpdateDailyWeather(pair.Key, pair.Value, random);
		}
		foreach (KeyValuePair<string, LocationContextData> pair in locationContextData)
		{
			string contextToCopy = pair.Value.CopyWeatherFromLocation;
			if (contextToCopy != null)
			{
				try
				{
					LocationWeather weatherForLocation = netWorldState.Value.GetWeatherForLocation(pair.Key);
					LocationWeather otherLocationWeather = netWorldState.Value.GetWeatherForLocation(contextToCopy);
					weatherForLocation.CopyFrom(otherLocationWeather);
				}
				catch
				{
				}
			}
		}
	}

	public static void ApplyWeatherForNewDay()
	{
		LocationWeather weatherForLocation = netWorldState.Value.GetWeatherForLocation("Default");
		weatherForTomorrow = weatherForLocation.WeatherForTomorrow;
		isRaining = weatherForLocation.IsRaining;
		isSnowing = weatherForLocation.IsSnowing;
		isLightning = weatherForLocation.IsLightning;
		isDebrisWeather = weatherForLocation.IsDebrisWeather;
		isGreenRain = weatherForLocation.IsGreenRain;
		if (isDebrisWeather)
		{
			populateDebrisWeatherArray();
		}
		if (!IsMasterGame)
		{
			return;
		}
		foreach (string key in netWorldState.Value.LocationWeather.Keys)
		{
			LocationWeather locationWeather = netWorldState.Value.LocationWeather[key];
			if (dayOfMonth == 1)
			{
				locationWeather.monthlyNonRainyDayCount.Value = 0;
			}
			if (!locationWeather.IsRaining)
			{
				locationWeather.monthlyNonRainyDayCount.Value++;
			}
		}
	}

	public static void UpdateShopPlayerItemInventory(string location_name, HashSet<NPC> purchased_item_npcs)
	{
		if (!(getLocationFromName(location_name) is ShopLocation shopLocation))
		{
			return;
		}
		for (int i = shopLocation.itemsFromPlayerToSell.Count - 1; i >= 0; i--)
		{
			if (!(shopLocation.itemsFromPlayerToSell[i] is Object item))
			{
				shopLocation.itemsFromPlayerToSell.RemoveAt(i);
			}
			else
			{
				for (int j = 0; j < item.Stack; j++)
				{
					bool soldItem = false;
					if ((int)item.edibility != -300 && random.NextDouble() < 0.04)
					{
						NPC n = Utility.GetRandomNpc((string name, CharacterData data) => data.CanCommentOnPurchasedShopItems ?? (data.HomeRegion == "Town"));
						if (n.Age != 2 && n.getSpouse() == null)
						{
							if (!purchased_item_npcs.Contains(n))
							{
								n.addExtraDialogue(shopLocation.getPurchasedItemDialogueForNPC(item, n));
								purchased_item_npcs.Add(n);
							}
							item.Stack--;
							soldItem = true;
						}
					}
					if (!soldItem && random.NextDouble() < 0.15)
					{
						item.Stack--;
					}
					if (item.Stack <= 0)
					{
						break;
					}
				}
				if (item.Stack <= 0)
				{
					shopLocation.itemsFromPlayerToSell.RemoveAt(i);
				}
			}
		}
	}

	private static void handlePostFarmEventActions()
	{
		Utility.ForEachLocation(delegate(GameLocation location)
		{
			foreach (Action postFarmEventOvernightAction in location.postFarmEventOvernightActions)
			{
				postFarmEventOvernightAction();
			}
			location.postFarmEventOvernightActions.Clear();
			return true;
		});
		if (IsMasterGame)
		{
			Mountain mountain = RequireLocation<Mountain>("Mountain");
			mountain.ApplyTreehouseIfNecessary();
			if (mountain.treehouseDoorDirty)
			{
				mountain.treehouseDoorDirty = false;
				WarpPathfindingCache.PopulateCache();
			}
		}
	}

	public static void ReceiveMailForTomorrow(string mail_to_transfer = null)
	{
		foreach (string s in player.mailForTomorrow)
		{
			if (s == null)
			{
				continue;
			}
			string stripped = s.Replace("%&NL&%", "");
			if (mail_to_transfer == null || !(mail_to_transfer != s) || !(mail_to_transfer != stripped))
			{
				mailDeliveredFromMailForTomorrow.Add(s);
				if (s.Contains("%&NL&%"))
				{
					player.mailReceived.Add(stripped);
				}
				else
				{
					mailbox.Add(s);
				}
			}
		}
	}

	public static void RemoveDeliveredMailForTomorrow()
	{
		ReceiveMailForTomorrow("abandonedJojaMartAccessible");
		foreach (string s in mailDeliveredFromMailForTomorrow)
		{
			player.mailForTomorrow.Remove(s);
		}
		mailDeliveredFromMailForTomorrow.Clear();
	}

	public static void queueWeddingsForToday()
	{
		weddingsToday.Clear();
		weddingToday = false;
		if (!canHaveWeddingOnDay(dayOfMonth, season))
		{
			return;
		}
		foreach (Farmer farmer in from farmer in getOnlineFarmers()
			orderby farmer.UniqueMultiplayerID
			select farmer)
		{
			if (farmer.spouse != null && farmer.isEngaged() && farmer.friendshipData[farmer.spouse].CountdownToWedding < 1)
			{
				weddingsToday.Add(farmer.UniqueMultiplayerID);
			}
			if (!farmer.team.IsEngaged(farmer.UniqueMultiplayerID))
			{
				continue;
			}
			long? spouse = farmer.team.GetSpouse(farmer.UniqueMultiplayerID);
			if (spouse.HasValue && !weddingsToday.Contains(spouse.Value))
			{
				Farmer spouse_farmer = getFarmerMaybeOffline(spouse.Value);
				if (spouse_farmer != null && getOnlineFarmers().Contains(spouse_farmer) && getOnlineFarmers().Contains(farmer) && player.team.GetFriendship(farmer.UniqueMultiplayerID, spouse.Value).CountdownToWedding < 1)
				{
					weddingsToday.Add(farmer.UniqueMultiplayerID);
				}
			}
		}
	}

	public static bool PollForEndOfNewDaySync()
	{
		if (!IsMultiplayer)
		{
			newDaySync.destroy();
			currentLocation.resetForPlayerEntry();
			return true;
		}
		if (newDaySync.readyForFinish())
		{
			if (IsMasterGame && newDaySync.hasInstance() && !newDaySync.hasFinished())
			{
				newDaySync.finish();
			}
			if (IsClient)
			{
				player.sleptInTemporaryBed.Value = false;
			}
			if (newDaySync.hasInstance() && newDaySync.hasFinished())
			{
				newDaySync.destroy();
				currentLocation.resetForPlayerEntry();
				return true;
			}
		}
		return false;
	}

	public static void updateWeatherIcon()
	{
		if (IsSnowingHere())
		{
			weatherIcon = 7;
		}
		else if (IsRainingHere())
		{
			weatherIcon = 4;
		}
		else if (IsDebrisWeatherHere() && IsSpring)
		{
			weatherIcon = 3;
		}
		else if (IsDebrisWeatherHere() && IsFall)
		{
			weatherIcon = 6;
		}
		else if (IsDebrisWeatherHere() && IsWinter)
		{
			weatherIcon = 7;
		}
		else if (weddingToday)
		{
			weatherIcon = 0;
		}
		else
		{
			weatherIcon = 2;
		}
		if (IsLightningHere())
		{
			weatherIcon = 5;
		}
		if (Utility.isFestivalDay())
		{
			weatherIcon = 1;
		}
		if (IsGreenRainingHere())
		{
			weatherIcon = 999;
		}
	}

	public static void showEndOfNightStuff()
	{
		hooks.OnGame1_ShowEndOfNightStuff(delegate
		{
			bool flag = false;
			if (player.displayedShippedItems.Count > 0)
			{
				endOfNightMenus.Push(new ShippingMenu(player.displayedShippedItems));
				player.displayedShippedItems.Clear();
				flag = true;
			}
			bool flag2 = false;
			if (player.newLevels.Count > 0 && !flag)
			{
				endOfNightMenus.Push(new SaveGameMenu());
			}
			for (int num = player.newLevels.Count - 1; num >= 0; num--)
			{
				endOfNightMenus.Push(new LevelUpMenu(player.newLevels[num].X, player.newLevels[num].Y));
				flag2 = true;
			}
			if ((int)player.farmingLevel == 10 && (int)player.miningLevel == 10 && (int)player.fishingLevel == 10 && (int)player.foragingLevel == 10 && (int)player.combatLevel == 10 && player.mailReceived.Add("gotMasteryHint") && !player.locationsVisited.Contains("MasteryCave"))
			{
				morningQueue.Enqueue(delegate
				{
					showGlobalMessage(content.LoadString("Strings\\1_6_Strings:MasteryHint"));
				});
			}
			if (flag2)
			{
				playSound("newRecord");
			}
			if (client == null || !client.timedOut)
			{
				if (endOfNightMenus.Count > 0)
				{
					showingEndOfNightStuff = true;
					activeClickableMenu = endOfNightMenus.Pop();
				}
				else
				{
					showingEndOfNightStuff = true;
					activeClickableMenu = new SaveGameMenu();
				}
			}
		});
	}

	/// <summary>Update the game state when the season changes. Despite the name, this may update more than graphics (e.g. it'll remove grass in winter).</summary>
	/// <param name="onLoad">Whether the season is being initialized as part of loading the save, instead of an actual in-game season change.</param>
	public static void setGraphicsForSeason(bool onLoad = false)
	{
		foreach (GameLocation l in locations)
		{
			Season season = l.GetSeason();
			l.seasonUpdate(onLoad);
			l.updateSeasonalTileSheets();
			if (!l.IsOutdoors)
			{
				continue;
			}
			switch (season)
			{
			case Season.Spring:
				eveningColor = new Color(255, 255, 0);
				break;
			case Season.Summer:
				foreach (Object o in l.Objects.Values)
				{
					if (!o.IsWeeds())
					{
						continue;
					}
					switch (o.QualifiedItemId)
					{
					case "(O)792":
						o.SetIdAndSprite(o.ParentSheetIndex + 1);
						continue;
					case "(O)882":
					case "(O)883":
					case "(O)884":
						continue;
					}
					if (random.NextDouble() < 0.3)
					{
						o.SetIdAndSprite(676);
					}
					else if (random.NextDouble() < 0.3)
					{
						o.SetIdAndSprite(677);
					}
				}
				eveningColor = new Color(255, 255, 0);
				break;
			case Season.Fall:
				foreach (Object o in l.Objects.Values)
				{
					if (o.IsWeeds())
					{
						switch (o.QualifiedItemId)
						{
						case "(O)793":
							o.SetIdAndSprite(o.ParentSheetIndex + 1);
							break;
						default:
							o.SetIdAndSprite(random.Choose(678, 679));
							break;
						case "(O)882":
						case "(O)883":
						case "(O)884":
							break;
						}
					}
				}
				eveningColor = new Color(255, 255, 0);
				foreach (WeatherDebris item in debrisWeather)
				{
					item.which = 2;
				}
				break;
			case Season.Winter:
			{
				KeyValuePair<Vector2, Object>[] array = l.Objects.Pairs.ToArray();
				for (int i = 0; i < array.Length; i++)
				{
					KeyValuePair<Vector2, Object> pair = array[i];
					Object o = pair.Value;
					if (o.IsWeeds())
					{
						switch (o.QualifiedItemId)
						{
						case "(O)882":
						case "(O)883":
						case "(O)884":
							continue;
						}
						l.Objects.Remove(pair.Key);
					}
				}
				foreach (WeatherDebris item2 in debrisWeather)
				{
					item2.which = 3;
				}
				eveningColor = new Color(245, 225, 170);
				break;
			}
			}
		}
	}

	public static void pauseThenMessage(int millisecondsPause, string message)
	{
		messageAfterPause = message;
		pauseTime = millisecondsPause;
	}

	public static bool IsVisitingIslandToday(string npc_name)
	{
		return netWorldState.Value.IslandVisitors.Contains(npc_name);
	}

	public static bool shouldTimePass(bool ignore_multiplayer = false)
	{
		if (isFestival())
		{
			return false;
		}
		if (CurrentEvent != null && CurrentEvent.isWedding)
		{
			return false;
		}
		if (farmEvent != null)
		{
			return false;
		}
		if (IsMultiplayer && !ignore_multiplayer)
		{
			return !netWorldState.Value.IsTimePaused;
		}
		if (paused || freezeControls || overlayMenu != null || isTimePaused)
		{
			return false;
		}
		if (eventUp)
		{
			return false;
		}
		if (activeClickableMenu != null && !(activeClickableMenu is BobberBar))
		{
			return false;
		}
		if (!player.CanMove && !player.UsingTool)
		{
			return player.forceTimePass;
		}
		return true;
	}

	public static Farmer getPlayerOrEventFarmer()
	{
		if (eventUp && CurrentEvent != null && !CurrentEvent.isFestival && CurrentEvent.farmer != null)
		{
			return CurrentEvent.farmer;
		}
		return player;
	}

	public static void UpdateViewPort(bool overrideFreeze, Point centerPoint)
	{
		previousViewportPosition.X = viewport.X;
		previousViewportPosition.Y = viewport.Y;
		Farmer farmer = getPlayerOrEventFarmer();
		if (currentLocation == null)
		{
			return;
		}
		if (!viewportFreeze || overrideFreeze)
		{
			Microsoft.Xna.Framework.Rectangle viewportBounds = ((viewportClampArea == Microsoft.Xna.Framework.Rectangle.Empty) ? new Microsoft.Xna.Framework.Rectangle(0, 0, currentLocation.Map.DisplayWidth, currentLocation.Map.DisplayHeight) : viewportClampArea);
			Point playerPixel = farmer.StandingPixel;
			bool snapBack = Math.Abs(currentViewportTarget.X + (float)(viewport.Width / 2) + (float)viewportBounds.X - (float)playerPixel.X) > 64f || Math.Abs(currentViewportTarget.Y + (float)(viewport.Height / 2) + (float)viewportBounds.Y - (float)playerPixel.Y) > 64f;
			if (forceSnapOnNextViewportUpdate)
			{
				snapBack = true;
			}
			if (centerPoint.X >= viewportBounds.X + viewport.Width / 2 && centerPoint.X <= viewportBounds.X + viewportBounds.Width - viewport.Width / 2)
			{
				if (farmer.isRafting || snapBack)
				{
					currentViewportTarget.X = centerPoint.X - viewport.Width / 2;
				}
				else if (Math.Abs(currentViewportTarget.X - (currentViewportTarget.X = centerPoint.X - viewport.Width / 2 + viewportBounds.X)) > farmer.getMovementSpeed())
				{
					currentViewportTarget.X += (float)Math.Sign(currentViewportTarget.X - (currentViewportTarget.X = centerPoint.X - viewport.Width / 2 + viewportBounds.X)) * farmer.getMovementSpeed();
				}
			}
			else if (centerPoint.X < viewport.Width / 2 + viewportBounds.X && viewport.Width <= viewportBounds.Width)
			{
				if (farmer.isRafting || snapBack)
				{
					currentViewportTarget.X = viewportBounds.X;
				}
				else if (Math.Abs(currentViewportTarget.X - (float)viewportBounds.X) > farmer.getMovementSpeed())
				{
					currentViewportTarget.X -= (float)Math.Sign(currentViewportTarget.X - (float)viewportBounds.X) * farmer.getMovementSpeed();
				}
			}
			else if (viewport.Width <= viewportBounds.Width)
			{
				if (farmer.isRafting || snapBack)
				{
					currentViewportTarget.X = viewportBounds.X + viewportBounds.Width - viewport.Width;
				}
				else if (!(Math.Abs(currentViewportTarget.X - (float)(viewportBounds.Width - viewport.Width)) > farmer.getMovementSpeed()))
				{
				}
			}
			else if (viewportBounds.Width < viewport.Width)
			{
				if (farmer.isRafting || snapBack)
				{
					currentViewportTarget.X = (viewportBounds.Width - viewport.Width) / 2 + viewportBounds.X;
				}
				else
				{
					Math.Abs(currentViewportTarget.X - (float)((viewportBounds.Width + viewportBounds.X - viewport.Width) / 2));
					farmer.getMovementSpeed();
				}
			}
			if (centerPoint.Y >= viewport.Height / 2 && centerPoint.Y <= currentLocation.Map.DisplayHeight - viewport.Height / 2)
			{
				if (farmer.isRafting || snapBack)
				{
					currentViewportTarget.Y = centerPoint.Y - viewport.Height / 2;
				}
				else if (Math.Abs(currentViewportTarget.Y - (float)(centerPoint.Y - viewport.Height / 2)) >= farmer.getMovementSpeed())
				{
					currentViewportTarget.Y -= (float)Math.Sign(currentViewportTarget.Y - (float)(centerPoint.Y - viewport.Height / 2)) * farmer.getMovementSpeed();
				}
			}
			else if (centerPoint.Y < viewport.Height / 2 && viewport.Height <= currentLocation.Map.DisplayHeight)
			{
				if (farmer.isRafting || snapBack)
				{
					currentViewportTarget.Y = 0f;
				}
				else if (Math.Abs(currentViewportTarget.Y - 0f) > farmer.getMovementSpeed())
				{
					currentViewportTarget.Y -= (float)Math.Sign(currentViewportTarget.Y - 0f) * farmer.getMovementSpeed();
				}
				currentViewportTarget.Y = 0f;
			}
			else if (viewport.Height <= currentLocation.Map.DisplayHeight)
			{
				if (farmer.isRafting || snapBack)
				{
					currentViewportTarget.Y = currentLocation.Map.DisplayHeight - viewport.Height;
				}
				else if (Math.Abs(currentViewportTarget.Y - (float)(currentLocation.Map.DisplayHeight - viewport.Height)) > farmer.getMovementSpeed())
				{
					currentViewportTarget.Y -= (float)Math.Sign(currentViewportTarget.Y - (float)(currentLocation.Map.DisplayHeight - viewport.Height)) * farmer.getMovementSpeed();
				}
			}
			else if (currentLocation.Map.DisplayHeight < viewport.Height)
			{
				if (farmer.isRafting || snapBack)
				{
					currentViewportTarget.Y = (currentLocation.Map.DisplayHeight - viewport.Height) / 2;
				}
				else if (Math.Abs(currentViewportTarget.Y - (float)((currentLocation.Map.DisplayHeight - viewport.Height) / 2)) > farmer.getMovementSpeed())
				{
					currentViewportTarget.Y -= (float)Math.Sign(currentViewportTarget.Y - (float)((currentLocation.Map.DisplayHeight - viewport.Height) / 2)) * farmer.getMovementSpeed();
				}
			}
		}
		if (currentLocation.forceViewportPlayerFollow)
		{
			currentViewportTarget.X = farmer.Position.X - (float)(viewport.Width / 2);
			currentViewportTarget.Y = farmer.Position.Y - (float)(viewport.Height / 2);
		}
		bool force_snap = false;
		if (forceSnapOnNextViewportUpdate)
		{
			force_snap = true;
			forceSnapOnNextViewportUpdate = false;
		}
		if (currentViewportTarget.X != -2.1474836E+09f && (!viewportFreeze || overrideFreeze))
		{
			int difference = (int)(currentViewportTarget.X - (float)viewport.X);
			if (Math.Abs(difference) > 128)
			{
				viewportPositionLerp.X = currentViewportTarget.X;
			}
			else
			{
				viewportPositionLerp.X += (float)difference * farmer.getMovementSpeed() * 0.03f;
			}
			difference = (int)(currentViewportTarget.Y - (float)viewport.Y);
			if (Math.Abs(difference) > 128)
			{
				viewportPositionLerp.Y = (int)currentViewportTarget.Y;
			}
			else
			{
				viewportPositionLerp.Y += (float)difference * farmer.getMovementSpeed() * 0.03f;
			}
			if (force_snap)
			{
				viewportPositionLerp.X = (int)currentViewportTarget.X;
				viewportPositionLerp.Y = (int)currentViewportTarget.Y;
			}
			viewport.X = (int)viewportPositionLerp.X;
			viewport.Y = (int)viewportPositionLerp.Y;
		}
	}

	private void UpdateCharacters(GameTime time)
	{
		if (CurrentEvent?.farmer != null && CurrentEvent.farmer != player)
		{
			CurrentEvent.farmer.Update(time, currentLocation);
		}
		player.Update(time, currentLocation);
		foreach (KeyValuePair<long, Farmer> v in otherFarmers)
		{
			if (v.Key != player.UniqueMultiplayerID)
			{
				v.Value.UpdateIfOtherPlayer(time);
			}
		}
	}

	public static void addMail(string mailName, bool noLetter = false, bool sendToEveryone = false)
	{
		if (sendToEveryone)
		{
			multiplayer.broadcastPartyWideMail(mailName, Multiplayer.PartyWideMessageQueue.SeenMail, noLetter);
			return;
		}
		mailName = mailName.Trim();
		mailName = mailName.Replace(Environment.NewLine, "");
		if (!player.hasOrWillReceiveMail(mailName))
		{
			if (noLetter)
			{
				player.mailReceived.Add(mailName);
			}
			else
			{
				player.mailbox.Add(mailName);
			}
		}
	}

	public static void addMailForTomorrow(string mailName, bool noLetter = false, bool sendToEveryone = false)
	{
		if (sendToEveryone)
		{
			multiplayer.broadcastPartyWideMail(mailName, Multiplayer.PartyWideMessageQueue.MailForTomorrow, noLetter);
			return;
		}
		mailName = mailName.Trim();
		mailName = mailName.Replace(Environment.NewLine, "");
		if (player.hasOrWillReceiveMail(mailName))
		{
			return;
		}
		if (noLetter)
		{
			mailName += "%&NL&%";
		}
		player.mailForTomorrow.Add(mailName);
		if (!sendToEveryone || !IsMultiplayer)
		{
			return;
		}
		foreach (Farmer farmer in otherFarmers.Values)
		{
			if (farmer != player && !player.hasOrWillReceiveMail(mailName))
			{
				farmer.mailForTomorrow.Add(mailName);
			}
		}
	}

	public static void drawDialogue(NPC speaker)
	{
		if (speaker.CurrentDialogue.Count == 0)
		{
			return;
		}
		activeClickableMenu = new DialogueBox(speaker.CurrentDialogue.Peek());
		if (activeClickableMenu is DialogueBox { dialogueFinished: not false })
		{
			activeClickableMenu = null;
			return;
		}
		dialogueUp = true;
		if (!eventUp)
		{
			player.Halt();
			player.CanMove = false;
		}
		if (speaker != null)
		{
			currentSpeaker = speaker;
		}
	}

	public static void multipleDialogues(string[] messages)
	{
		activeClickableMenu = new DialogueBox(messages.ToList());
		dialogueUp = true;
		player.CanMove = false;
	}

	public static void drawDialogueNoTyping(string dialogue)
	{
		drawObjectDialogue(dialogue);
		if (activeClickableMenu is DialogueBox dialogueBox)
		{
			dialogueBox.showTyping = false;
		}
	}

	public static void drawDialogueNoTyping(List<string> dialogues)
	{
		drawObjectDialogue(dialogues);
		if (activeClickableMenu is DialogueBox dialogueBox)
		{
			dialogueBox.showTyping = false;
		}
	}

	/// <summary>Show a dialogue box with text from an NPC's answering machine.</summary>
	/// <param name="npc">The NPC whose answering machine to display.</param>
	/// <param name="translationKey">The translation key for the message text.</param>
	/// <param name="substitutions">The token substitutions for placeholders in the translation text, if any.</param>
	public static void DrawAnsweringMachineDialogue(NPC npc, string translationKey, params object[] substitutions)
	{
		Dialogue dialogue = Dialogue.FromTranslation(npc, translationKey, substitutions);
		dialogue.overridePortrait = temporaryContent.Load<Texture2D>("Portraits\\AnsweringMachine");
		DrawDialogue(dialogue);
	}

	/// <summary>Show a dialogue box with text from an NPC.</summary>
	/// <param name="npc">The NPC whose dialogue to display.</param>
	/// <param name="translationKey">The translation from which to take the dialogue text, in the form <c>assetName:fieldKey</c> like <c>Strings/UI:Confirm</c>.</param>
	public static void DrawDialogue(NPC npc, string translationKey)
	{
		DrawDialogue(new Dialogue(npc, translationKey));
	}

	/// <summary>Show a dialogue box with text from an NPC.</summary>
	/// <param name="npc">The NPC whose dialogue to display.</param>
	/// <param name="translationKey">The translation from which to take the dialogue text, in the form <c>assetName:fieldKey</c> like <c>Strings/UI:Confirm</c>.</param>
	/// <param name="substitutions">The values with which to replace placeholders like <c>{0}</c> in the loaded text.</param>
	public static void DrawDialogue(NPC npc, string translationKey, params object[] substitutions)
	{
		DrawDialogue(Dialogue.FromTranslation(npc, translationKey, substitutions));
	}

	/// <summary>Show a dialogue box with text from an NPC.</summary>
	/// <param name="dialogue">The dialogue to display.</param>
	public static void DrawDialogue(Dialogue dialogue)
	{
		if (dialogue.speaker != null)
		{
			dialogue.speaker.CurrentDialogue.Push(dialogue);
			drawDialogue(dialogue.speaker);
			return;
		}
		activeClickableMenu = new DialogueBox(dialogue);
		dialogueUp = true;
		if (!eventUp)
		{
			player.Halt();
			player.CanMove = false;
		}
	}

	private static void checkIfDialogueIsQuestion()
	{
		if (currentSpeaker != null && currentSpeaker.CurrentDialogue.Count > 0 && currentSpeaker.CurrentDialogue.Peek().isCurrentDialogueAQuestion())
		{
			questionChoices.Clear();
			isQuestion = true;
			List<NPCDialogueResponse> questions = currentSpeaker.CurrentDialogue.Peek().getNPCResponseOptions();
			for (int i = 0; i < questions.Count; i++)
			{
				questionChoices.Add(questions[i]);
			}
		}
	}

	public static void drawLetterMessage(string message)
	{
		activeClickableMenu = new LetterViewerMenu(message);
	}

	public static void drawObjectDialogue(string dialogue)
	{
		activeClickableMenu?.emergencyShutDown();
		activeClickableMenu = new DialogueBox(dialogue);
		player.CanMove = false;
		dialogueUp = true;
	}

	public static void drawObjectDialogue(List<string> dialogue)
	{
		activeClickableMenu?.emergencyShutDown();
		activeClickableMenu = new DialogueBox(dialogue);
		player.CanMove = false;
		dialogueUp = true;
	}

	public static void drawObjectQuestionDialogue(string dialogue, Response[] choices, int width)
	{
		activeClickableMenu = new DialogueBox(dialogue, choices, width);
		dialogueUp = true;
		player.CanMove = false;
	}

	public static void drawObjectQuestionDialogue(string dialogue, Response[] choices)
	{
		activeClickableMenu = new DialogueBox(dialogue, choices);
		dialogueUp = true;
		player.CanMove = false;
	}

	public static void warpCharacter(NPC character, string targetLocationName, Point position)
	{
		warpCharacter(character, targetLocationName, new Vector2(position.X, position.Y));
	}

	public static void warpCharacter(NPC character, string targetLocationName, Vector2 position)
	{
		warpCharacter(character, RequireLocation(targetLocationName), position);
	}

	public static void warpCharacter(NPC character, GameLocation targetLocation, Vector2 position)
	{
		foreach (string activePassiveFestival in netWorldState.Value.ActivePassiveFestivals)
		{
			if (Utility.TryGetPassiveFestivalData(activePassiveFestival, out var festival) && dayOfMonth >= festival.StartDay && dayOfMonth <= festival.EndDay && festival.Season == season && festival.MapReplacements != null && festival.MapReplacements.TryGetValue(targetLocation.name, out var newName))
			{
				targetLocation = RequireLocation(newName);
			}
		}
		if (targetLocation.name.Equals("Trailer") && MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
		{
			targetLocation = RequireLocation("Trailer_Big");
			if (position.X == 12f && position.Y == 9f)
			{
				position.X = 13f;
				position.Y = 24f;
			}
		}
		if (IsClient)
		{
			multiplayer.requestCharacterWarp(character, targetLocation, position);
			return;
		}
		if (!targetLocation.characters.Contains(character))
		{
			character.currentLocation?.characters.Remove(character);
			targetLocation.addCharacter(character);
		}
		character.isCharging = false;
		character.speed = 2;
		character.blockedInterval = 0;
		NPC.getTextureNameForCharacter(character.Name);
		character.position.X = position.X * 64f;
		character.position.Y = position.Y * 64f;
		if (character.CurrentDialogue.Count > 0 && character.CurrentDialogue.Peek().removeOnNextMove && character.Tile != character.DefaultPosition / 64f)
		{
			character.CurrentDialogue.Pop();
		}
		if (targetLocation is FarmHouse farmHouse)
		{
			character.arriveAtFarmHouse(farmHouse);
		}
		else
		{
			character.arriveAt(targetLocation);
		}
		if (character.currentLocation != null && !character.currentLocation.Equals(targetLocation))
		{
			character.currentLocation.characters.Remove(character);
		}
		character.currentLocation = targetLocation;
	}

	public static LocationRequest getLocationRequest(string locationName, bool isStructure = false)
	{
		if (locationName == null)
		{
			throw new ArgumentException();
		}
		return new LocationRequest(locationName, isStructure, getLocationFromName(locationName, isStructure));
	}

	public static void warpHome()
	{
		LocationRequest obj = getLocationRequest(player.homeLocation.Value);
		obj.OnWarp += delegate
		{
			player.position.Set(Utility.PointToVector2((currentLocation as FarmHouse).GetPlayerBedSpot()) * 64f);
		};
		warpFarmer(obj, 5, 9, player.FacingDirection);
	}

	public static void warpFarmer(string locationName, int tileX, int tileY, bool flip)
	{
		warpFarmer(getLocationRequest(locationName), tileX, tileY, flip ? ((player.FacingDirection + 2) % 4) : player.FacingDirection);
	}

	public static void warpFarmer(string locationName, int tileX, int tileY, int facingDirectionAfterWarp)
	{
		warpFarmer(getLocationRequest(locationName), tileX, tileY, facingDirectionAfterWarp);
	}

	public static void warpFarmer(string locationName, int tileX, int tileY, int facingDirectionAfterWarp, bool isStructure)
	{
		warpFarmer(getLocationRequest(locationName, isStructure), tileX, tileY, facingDirectionAfterWarp);
	}

	public virtual bool ShouldDismountOnWarp(Horse mount, GameLocation old_location, GameLocation new_location)
	{
		if (mount == null)
		{
			return false;
		}
		if (currentLocation != null && currentLocation.IsOutdoors && new_location != null)
		{
			return !new_location.IsOutdoors;
		}
		return false;
	}

	public static void warpFarmer(LocationRequest locationRequest, int tileX, int tileY, int facingDirectionAfterWarp)
	{
		int warp_offset_x = nextFarmerWarpOffsetX;
		int warp_offset_y = nextFarmerWarpOffsetY;
		nextFarmerWarpOffsetX = 0;
		nextFarmerWarpOffsetY = 0;
		foreach (string activePassiveFestival in netWorldState.Value.ActivePassiveFestivals)
		{
			if (Utility.TryGetPassiveFestivalData(activePassiveFestival, out var festival) && dayOfMonth >= festival.StartDay && dayOfMonth <= festival.EndDay && festival.Season == season && festival.MapReplacements != null && festival.MapReplacements.TryGetValue(locationRequest.Name, out var newName))
			{
				locationRequest = getLocationRequest(newName);
			}
		}
		int level;
		switch (locationRequest.Name)
		{
		case "BusStop":
			if (tileX < 10)
			{
				tileX = 10;
			}
			break;
		case "Farm":
			switch (currentLocation?.NameOrUniqueName)
			{
			case "FarmCave":
			{
				if (tileX != 34 || tileY != 6)
				{
					break;
				}
				if (getFarm().TryGetMapPropertyAs("FarmCaveEntry", out Point tile, required: false))
				{
					tileX = tile.X;
					tileY = tile.Y;
					break;
				}
				level = whichFarm;
				switch (level)
				{
				case 6:
					tileX = 34;
					tileY = 16;
					break;
				case 5:
					tileX = 30;
					tileY = 36;
					break;
				}
				break;
			}
			case "Forest":
			{
				if (tileX != 41 || tileY != 64)
				{
					break;
				}
				if (getFarm().TryGetMapPropertyAs("ForestEntry", out Point tile, required: false))
				{
					tileX = tile.X;
					tileY = tile.Y;
					break;
				}
				level = whichFarm;
				switch (level)
				{
				case 6:
					tileX = 82;
					tileY = 103;
					break;
				case 5:
					tileX = 40;
					tileY = 64;
					break;
				}
				break;
			}
			case "BusStop":
			{
				if (tileX == 79 && tileY == 17 && getFarm().TryGetMapPropertyAs("BusStopEntry", out Point tile, required: false))
				{
					tileX = tile.X;
					tileY = tile.Y;
				}
				break;
			}
			case "Backwoods":
			{
				if (tileX == 40 && tileY == 0 && getFarm().TryGetMapPropertyAs("BackwoodsEntry", out Point tile, required: false))
				{
					tileX = tile.X;
					tileY = tile.Y;
				}
				break;
			}
			}
			break;
		case "IslandSouth":
			if (tileX <= 15 && tileY <= 6)
			{
				tileX = 21;
				tileY = 43;
			}
			break;
		case "Trailer":
			if (MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
			{
				locationRequest = getLocationRequest("Trailer_Big");
				tileX = 13;
				tileY = 24;
			}
			break;
		case "Club":
			if (player.hasClubCard)
			{
				break;
			}
			locationRequest = getLocationRequest("SandyHouse");
			locationRequest.OnWarp += delegate
			{
				NPC characterFromName = currentLocation.getCharacterFromName("Bouncer");
				if (characterFromName != null)
				{
					Vector2 vector = new Vector2(17f, 4f);
					characterFromName.showTextAboveHead(content.LoadString("Strings\\Locations:Club_Bouncer_TextAboveHead" + (random.Next(2) + 1)));
					int num = random.Next();
					currentLocation.playSound("thudStep");
					multiplayer.broadcastSprites(currentLocation, new TemporaryAnimatedSprite(288, 100f, 1, 24, vector * 64f, flicker: true, flipped: false, currentLocation, player)
					{
						shakeIntensity = 0.5f,
						shakeIntensityChange = 0.002f,
						extraInfoForEndBehavior = num,
						endFunction = currentLocation.removeTemporarySpritesWithID
					}, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, vector * 64f + new Vector2(5f, 0f) * 4f, flicker: true, flipped: false, 0.0263f, 0f, Color.Yellow, 4f, 0f, 0f, 0f)
					{
						id = num
					}, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, vector * 64f + new Vector2(5f, 0f) * 4f, flicker: true, flipped: true, 0.0263f, 0f, Color.Orange, 4f, 0f, 0f, 0f)
					{
						delayBeforeAnimationStart = 100,
						id = num
					}, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, vector * 64f + new Vector2(5f, 0f) * 4f, flicker: true, flipped: false, 0.0263f, 0f, Color.White, 3f, 0f, 0f, 0f)
					{
						delayBeforeAnimationStart = 200,
						id = num
					});
					currentLocation.netAudio.StartPlaying("fuse");
				}
			};
			tileX = 17;
			tileY = 4;
			break;
		}
		if (VolcanoDungeon.IsGeneratedLevel(locationRequest.Name, out level))
		{
			warp_offset_x = 0;
			warp_offset_y = 0;
		}
		if (player.isRidingHorse() && currentLocation != null)
		{
			GameLocation next_location = locationRequest.Location;
			if (next_location == null)
			{
				next_location = getLocationFromName(locationRequest.Name);
			}
			if (game1.ShouldDismountOnWarp(player.mount, currentLocation, next_location))
			{
				player.mount.dismount();
				warp_offset_x = 0;
				warp_offset_y = 0;
			}
		}
		if (weatherIcon == 1 && whereIsTodaysFest != null && locationRequest.Name.Equals(whereIsTodaysFest) && !warpingForForcedRemoteEvent)
		{
			string[] timeParts = ArgUtility.SplitBySpace(temporaryContent.Load<Dictionary<string, string>>("Data\\Festivals\\" + currentSeason + dayOfMonth)["conditions"].Split('/')[1]);
			if (timeOfDay <= Convert.ToInt32(timeParts[1]))
			{
				if (timeOfDay < Convert.ToInt32(timeParts[0]))
				{
					if (!(currentLocation?.Name == "Hospital"))
					{
						player.Position = player.lastPosition;
						drawObjectDialogue(content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2973"));
						return;
					}
					locationRequest = getLocationRequest("BusStop");
					tileX = 34;
					tileY = 23;
				}
				else
				{
					if (IsMultiplayer)
					{
						netReady.SetLocalReady("festivalStart", ready: true);
						activeClickableMenu = new ReadyCheckDialog("festivalStart", allowCancel: true, delegate
						{
							exitActiveMenu();
							if (player.mount != null)
							{
								player.mount.dismount();
								warp_offset_x = 0;
								warp_offset_y = 0;
							}
							performWarpFarmer(locationRequest, tileX, tileY, facingDirectionAfterWarp);
						});
						return;
					}
					if (player.mount != null)
					{
						player.mount.dismount();
						warp_offset_x = 0;
						warp_offset_y = 0;
					}
				}
			}
		}
		tileX += warp_offset_x;
		tileY += warp_offset_y;
		performWarpFarmer(locationRequest, tileX, tileY, facingDirectionAfterWarp);
	}

	private static void performWarpFarmer(LocationRequest locationRequest, int tileX, int tileY, int facingDirectionAfterWarp)
	{
		if (locationRequest.Location != null)
		{
			if (tileX >= locationRequest.Location.Map.Layers[0].LayerWidth - 1)
			{
				tileX--;
			}
			if (IsMasterGame)
			{
				locationRequest.Location.hostSetup();
			}
		}
		log.Verbose("Warping to " + locationRequest.Name);
		if (player.IsSitting())
		{
			player.StopSitting(animate: false);
		}
		if (player.UsingTool)
		{
			player.completelyStopAnimatingOrDoingAction();
		}
		player.previousLocationName = ((player.currentLocation != null) ? ((string)player.currentLocation.name) : "");
		Game1.locationRequest = locationRequest;
		xLocationAfterWarp = tileX;
		yLocationAfterWarp = tileY;
		_isWarping = true;
		Game1.facingDirectionAfterWarp = facingDirectionAfterWarp;
		fadeScreenToBlack();
		setRichPresence("location", locationRequest.Name);
	}

	public static void requestLocationInfoFromServer()
	{
		if (locationRequest != null)
		{
			client.sendMessage(5, (short)xLocationAfterWarp, (short)yLocationAfterWarp, locationRequest.Name, (byte)(locationRequest.IsStructure ? 1 : 0));
		}
		currentLocation = null;
		player.Position = new Vector2(xLocationAfterWarp * 64, yLocationAfterWarp * 64 - (player.Sprite.getHeight() - 32) + 16);
		player.faceDirection(facingDirectionAfterWarp);
	}

	/// <summary>Get the first NPC which matches a condition.</summary>
	/// <typeparam name="T">The expected NPC type.</typeparam>
	/// <param name="check">The condition to check on each NPC.</param>
	/// <param name="includeEventActors">Whether to match temporary event actors.</param>
	/// <returns>Returns the matching NPC if found, else <c>null</c>.</returns>
	public static T GetCharacterWhere<T>(Func<T, bool> check, bool includeEventActors = false) where T : NPC
	{
		T match = null;
		T fallback = null;
		Utility.ForEachCharacter(delegate(NPC rawNpc)
		{
			if (rawNpc is T val && check(val))
			{
				if (val.currentLocation?.IsActiveLocation() ?? false)
				{
					match = val;
					return false;
				}
				fallback = val;
			}
			return true;
		}, includeEventActors);
		return match ?? fallback;
	}

	/// <summary>Get the first NPC of the given type.</summary>
	/// <typeparam name="T">The expected NPC type.</typeparam>
	/// <param name="includeEventActors">Whether to match temporary event actors.</param>
	/// <returns>Returns the matching NPC if found, else <c>null</c>.</returns>
	public static T GetCharacterOfType<T>(bool includeEventActors = false) where T : NPC
	{
		T match = null;
		T fallback = null;
		Utility.ForEachCharacter(delegate(NPC rawNpc)
		{
			if (rawNpc is T val)
			{
				if (rawNpc.currentLocation?.IsActiveLocation() ?? false)
				{
					match = val;
					return false;
				}
				fallback = val;
			}
			return true;
		}, includeEventActors);
		return match ?? fallback;
	}

	/// <summary>Get an NPC by its name.</summary>
	/// <typeparam name="T">The expected NPC type.</typeparam>
	/// <param name="name">The NPC name.</param>
	/// <param name="mustBeVillager">Whether to only match NPCs which return true for <see cref="P:StardewValley.NPC.IsVillager" />.</param>
	/// <param name="includeEventActors">Whether to match temporary event actors.</param>
	/// <returns>Returns the matching NPC if found, else <c>null</c>.</returns>
	public static T getCharacterFromName<T>(string name, bool mustBeVillager = true, bool includeEventActors = false) where T : NPC
	{
		T match = null;
		T fallback = null;
		Utility.ForEachCharacter(delegate(NPC rawNpc)
		{
			if (rawNpc is T val && val.Name == name && (!mustBeVillager || val.IsVillager))
			{
				if (val.currentLocation?.IsActiveLocation() ?? false)
				{
					match = val;
					return false;
				}
				fallback = val;
			}
			return true;
		}, includeEventActors);
		return match ?? fallback;
	}

	/// <summary>Get an NPC by its name.</summary>
	/// <param name="name">The NPC name.</param>
	/// <param name="mustBeVillager">Whether to only match NPCs which return true for <see cref="P:StardewValley.NPC.IsVillager" />.</param>
	/// <param name="includeEventActors">Whether to match temporary event actors.</param>
	/// <returns>Returns the matching NPC if found, else <c>null</c>.</returns>
	public static NPC getCharacterFromName(string name, bool mustBeVillager = true, bool includeEventActors = false)
	{
		NPC match = null;
		NPC fallback = null;
		Utility.ForEachCharacter(delegate(NPC npc)
		{
			if (npc.Name == name && (!mustBeVillager || npc.IsVillager))
			{
				if (npc.currentLocation?.IsActiveLocation() ?? false)
				{
					match = npc;
					return false;
				}
				fallback = npc;
			}
			return true;
		}, includeEventActors);
		return match ?? fallback;
	}

	/// <summary>Get an NPC by its name, or throw an exception if it's not found.</summary>
	/// <param name="name">The NPC name.</param>
	/// <param name="mustBeVillager">Whether to only match NPCs which return true for <see cref="P:StardewValley.NPC.IsVillager" />.</param>
	public static NPC RequireCharacter(string name, bool mustBeVillager = true)
	{
		return getCharacterFromName(name, mustBeVillager) ?? throw new KeyNotFoundException($"Required {(mustBeVillager ? "villager" : "NPC")} '{name}' not found.");
	}

	/// <summary>Get an NPC by its name, or throw an exception if it's not found.</summary>
	/// <typeparam name="T">The expected NPC type.</typeparam>
	/// <param name="name">The NPC name.</param>
	/// <param name="mustBeVillager">Whether to only match NPCs which return true for <see cref="P:StardewValley.NPC.IsVillager" />.</param>
	/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">There's no NPC matching the given arguments.</exception>
	/// <exception cref="T:System.InvalidCastException">The NPC found can't be converted to <typeparamref name="T" />.</exception>
	public static T RequireCharacter<T>(string name, bool mustBeVillager = true) where T : NPC
	{
		NPC npc = getCharacterFromName(name, mustBeVillager);
		if (!(npc is T cast))
		{
			if (npc == null)
			{
				throw new KeyNotFoundException($"Required {(mustBeVillager ? "villager" : "NPC")} '{name}' not found.");
			}
			throw new InvalidCastException($"Can't convert NPC '{name}' from '{npc?.GetType().FullName}' to the required '{typeof(T).FullName}'.");
		}
		return cast;
	}

	/// <summary>Get a location by its name, or throw an exception if it's not found.</summary>
	/// <param name="name">The location name.</param>
	/// <param name="isStructure">Whether the location is an interior structure.</param>
	/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">There's no location matching the given arguments.</exception>
	public static GameLocation RequireLocation(string name, bool isStructure = false)
	{
		return getLocationFromName(name, isStructure) ?? throw new KeyNotFoundException($"Required {(isStructure ? "structure " : "")}location '{name}' not found.");
	}

	/// <summary>Get a location by its name, or throw an exception if it's not found.</summary>
	/// <typeparam name="TLocation">The expected location type.</typeparam>
	/// <param name="name">The location name.</param>
	/// <param name="isStructure">Whether the location is an interior structure.</param>
	/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">There's no location matching the given arguments.</exception>
	/// <exception cref="T:System.InvalidCastException">The location found can't be converted to <typeparamref name="TLocation" />.</exception>
	public static TLocation RequireLocation<TLocation>(string name, bool isStructure = false) where TLocation : GameLocation
	{
		GameLocation location = getLocationFromName(name, isStructure);
		if (!(location is TLocation cast))
		{
			if (location == null)
			{
				throw new KeyNotFoundException($"Required {(isStructure ? "structure " : "")}location '{name}' not found.");
			}
			throw new InvalidCastException($"Can't convert location {name} from '{location?.GetType().FullName}' to the required '{typeof(TLocation).FullName}'.");
		}
		return cast;
	}

	/// <summary>Get a location by its name, or <c>null</c> if it's not found.</summary>
	/// <param name="name">The location name.</param>
	public static GameLocation getLocationFromName(string name)
	{
		return getLocationFromName(name, isStructure: false);
	}

	/// <summary>Get a location by its name, or <c>null</c> if it's not found.</summary>
	/// <param name="name">The location name.</param>
	/// <param name="isStructure">Whether the location is an interior structure.</param>
	public static GameLocation getLocationFromName(string name, bool isStructure)
	{
		if (string.IsNullOrEmpty(name))
		{
			return null;
		}
		if (currentLocation != null)
		{
			if (!isStructure)
			{
				if (string.Equals(currentLocation.name, name, StringComparison.OrdinalIgnoreCase))
				{
					return currentLocation;
				}
				if ((bool)currentLocation.isStructure && currentLocation.Root != null && string.Equals(currentLocation.Root.Value.NameOrUniqueName, name, StringComparison.OrdinalIgnoreCase))
				{
					return currentLocation.Root.Value;
				}
			}
			else if (currentLocation.NameOrUniqueName == name)
			{
				return currentLocation;
			}
		}
		if (_locationLookup.TryGetValue(name, out var cached_location))
		{
			return cached_location;
		}
		return getLocationFromNameInLocationsList(name, isStructure);
	}

	/// <summary>Get a location by its name (ignoring the cache and current location), or <c>null</c> if it's not found.</summary>
	/// <param name="name">The location name.</param>
	/// <param name="isStructure">Whether the location is an interior structure.</param>
	public static GameLocation getLocationFromNameInLocationsList(string name, bool isStructure = false)
	{
		for (int i = 0; i < locations.Count; i++)
		{
			GameLocation location = locations[i];
			if (!isStructure)
			{
				if (string.Equals(location.Name, name, StringComparison.OrdinalIgnoreCase))
				{
					_locationLookup[location.Name] = location;
					return location;
				}
				continue;
			}
			GameLocation buildingIndoors = findStructure(location, name);
			if (buildingIndoors != null)
			{
				_locationLookup[name] = buildingIndoors;
				return buildingIndoors;
			}
		}
		if (MineShaft.IsGeneratedLevel(name, out var level))
		{
			return MineShaft.GetMine(name);
		}
		if (VolcanoDungeon.IsGeneratedLevel(name, out level))
		{
			return VolcanoDungeon.GetLevel(name);
		}
		if (!isStructure)
		{
			return getLocationFromName(name, isStructure: true);
		}
		return null;
	}

	public static void flushLocationLookup()
	{
		_locationLookup.Clear();
	}

	public static void removeLocationFromLocationLookup(string name_or_unique_name)
	{
		List<string> keys_to_remove = new List<string>();
		foreach (string key in _locationLookup.Keys)
		{
			if (_locationLookup[key].NameOrUniqueName == name_or_unique_name)
			{
				keys_to_remove.Add(key);
			}
		}
		foreach (string key in keys_to_remove)
		{
			_locationLookup.Remove(key);
		}
	}

	public static void removeLocationFromLocationLookup(GameLocation location)
	{
		List<string> keys_to_remove = new List<string>();
		foreach (string key in _locationLookup.Keys)
		{
			if (_locationLookup[key] == location)
			{
				keys_to_remove.Add(key);
			}
		}
		foreach (string key in keys_to_remove)
		{
			_locationLookup.Remove(key);
		}
	}

	public static GameLocation findStructure(GameLocation parentLocation, string name)
	{
		foreach (Building building in parentLocation.buildings)
		{
			if (building.HasIndoorsName(name))
			{
				return building.GetIndoors();
			}
		}
		return null;
	}

	public static void addNewFarmBuildingMaps()
	{
		FarmHouse home = Utility.getHomeOfFarmer(player);
		if (player.HouseUpgradeLevel >= 1 && home.Map.Id.Equals("FarmHouse"))
		{
			home.updateMap();
		}
	}

	public static void PassOutNewDay()
	{
		player.lastSleepLocation.Value = currentLocation.NameOrUniqueName;
		player.lastSleepPoint.Value = player.TilePoint;
		if (!IsMultiplayer)
		{
			NewDay(0f);
			return;
		}
		player.FarmerSprite.setCurrentSingleFrame(5, 3000);
		player.FarmerSprite.PauseForSingleAnimation = true;
		player.passedOut = true;
		if (activeClickableMenu != null)
		{
			activeClickableMenu.emergencyShutDown();
			exitActiveMenu();
		}
		activeClickableMenu = new ReadyCheckDialog("sleep", allowCancel: false, delegate
		{
			NewDay(0f);
		});
	}

	public static void NewDay(float timeToPause)
	{
		if (activeClickableMenu is ReadyCheckDialog { checkName: "sleep" } readyCheckDialog && !readyCheckDialog.isCancelable())
		{
			readyCheckDialog.confirm();
		}
		currentMinigame = null;
		newDay = true;
		newDaySync.create();
		if ((bool)player.isInBed || player.passedOut)
		{
			nonWarpFade = true;
			screenFade.FadeScreenToBlack(player.passedOut ? 1.1f : 0f);
			player.Halt();
			player.currentEyes = 1;
			player.blinkTimer = -4000;
			player.CanMove = false;
			player.passedOut = false;
			pauseTime = timeToPause;
		}
		if (activeClickableMenu != null && !dialogueUp)
		{
			activeClickableMenu.emergencyShutDown();
			exitActiveMenu();
		}
	}

	public static void screenGlowOnce(Color glowColor, bool hold, float rate = 0.005f, float maxAlpha = 0.3f)
	{
		screenGlowMax = maxAlpha;
		screenGlowRate = rate;
		screenGlowAlpha = 0f;
		screenGlowUp = true;
		screenGlowColor = glowColor;
		screenGlow = true;
		screenGlowHold = hold;
	}

	public static string shortDayNameFromDayOfSeason(int dayOfSeason)
	{
		return (dayOfSeason % 7) switch
		{
			0 => "Sun", 
			1 => "Mon", 
			2 => "Tue", 
			3 => "Wed", 
			4 => "Thu", 
			5 => "Fri", 
			6 => "Sat", 
			_ => "", 
		};
	}

	public static string shortDayDisplayNameFromDayOfSeason(int dayOfSeason)
	{
		if (dayOfSeason < 0)
		{
			return string.Empty;
		}
		return _shortDayDisplayName[dayOfSeason % 7];
	}

	public static void runTestEvent()
	{
		StreamReader file = new StreamReader("test_event.txt");
		string? locationName = file.ReadLine();
		string event_string = file.ReadToEnd();
		event_string = event_string.Replace("\r\n", "/").Replace("\n", "/");
		log.Verbose("Running test event: " + event_string);
		LocationRequest location_request = getLocationRequest(locationName);
		location_request.OnWarp += delegate
		{
			currentLocation.currentEvent = new Event(event_string);
			currentLocation.checkForEvents();
		};
		int x = 8;
		int y = 8;
		Utility.getDefaultWarpLocation(locationName, ref x, ref y);
		warpFarmer(location_request, x, y, player.FacingDirection);
	}

	public static bool isMusicContextActiveButNotPlaying(MusicContext music_context = MusicContext.Default)
	{
		if (_activeMusicContext != music_context)
		{
			return false;
		}
		if (morningSongPlayAction != null)
		{
			return false;
		}
		string currentTrack = getMusicTrackName(music_context);
		if (currentTrack == "none")
		{
			return true;
		}
		if (currentSong != null && currentSong.Name == currentTrack && !currentSong.IsPlaying)
		{
			return true;
		}
		return false;
	}

	public static bool IsMusicContextActive(MusicContext music_context = MusicContext.Default)
	{
		if (_activeMusicContext != music_context)
		{
			return true;
		}
		return false;
	}

	public static bool doesMusicContextHaveTrack(MusicContext music_context = MusicContext.Default)
	{
		return _requestedMusicTracks.ContainsKey(music_context);
	}

	public static string getMusicTrackName(MusicContext music_context = MusicContext.Default)
	{
		if (_requestedMusicTracks.TryGetValue(music_context, out var trackData))
		{
			return trackData.Key;
		}
		if (music_context == MusicContext.Default)
		{
			return getMusicTrackName(MusicContext.SubLocation);
		}
		return "none";
	}

	public static void stopMusicTrack(MusicContext music_context)
	{
		if (_requestedMusicTracks.Remove(music_context))
		{
			if (music_context == MusicContext.Default)
			{
				stopMusicTrack(MusicContext.SubLocation);
			}
			UpdateRequestedMusicTrack();
		}
	}

	public static void changeMusicTrack(string newTrackName, bool track_interruptable = false, MusicContext music_context = MusicContext.Default)
	{
		if (newTrackName == null)
		{
			return;
		}
		if (music_context == MusicContext.Default)
		{
			if (morningSongPlayAction != null)
			{
				if (delayedActions.Contains(morningSongPlayAction))
				{
					delayedActions.Remove(morningSongPlayAction);
				}
				morningSongPlayAction = null;
			}
			if (IsGreenRainingHere() && !currentLocation.InIslandContext() && IsRainingHere(currentLocation) && !newTrackName.Equals("rain"))
			{
				return;
			}
		}
		if (music_context == MusicContext.Default || music_context == MusicContext.SubLocation)
		{
			IsPlayingBackgroundMusic = false;
			IsPlayingOutdoorsAmbience = false;
			IsPlayingNightAmbience = false;
			IsPlayingTownMusic = false;
			IsPlayingMorningSong = false;
		}
		if (music_context != MusicContext.ImportantSplitScreenMusic && !player.songsHeard.Contains(newTrackName))
		{
			Utility.farmerHeardSong(newTrackName);
		}
		_requestedMusicTracks[music_context] = new KeyValuePair<string, bool>(newTrackName, track_interruptable);
		UpdateRequestedMusicTrack();
	}

	public static void UpdateRequestedMusicTrack()
	{
		_activeMusicContext = MusicContext.Default;
		KeyValuePair<string, bool> requested_track_data = new KeyValuePair<string, bool>("none", value: true);
		for (int i = 0; i < 6; i++)
		{
			MusicContext context = (MusicContext)i;
			if (_requestedMusicTracks.TryGetValue(context, out var trackData))
			{
				if (context != MusicContext.ImportantSplitScreenMusic)
				{
					_activeMusicContext = context;
				}
				requested_track_data = trackData;
			}
		}
		if (requested_track_data.Key != requestedMusicTrack || requested_track_data.Value != requestedMusicTrackOverrideable)
		{
			requestedMusicDirty = true;
			requestedMusicTrack = requested_track_data.Key;
			requestedMusicTrackOverrideable = requested_track_data.Value;
		}
	}

	public static void enterMine(int whatLevel)
	{
		warpFarmer(MineShaft.GetLevelName(whatLevel), 6, 6, 2);
		player.temporarilyInvincible = true;
		player.temporaryInvincibilityTimer = 0;
		player.flashDuringThisTemporaryInvincibility = false;
		player.currentTemporaryInvincibilityDuration = 1000;
	}

	/// <summary>Get the season which currently applies to a location.</summary>
	/// <param name="location">The location to check, or <c>null</c> for the global season.</param>
	public static Season GetSeasonForLocation(GameLocation location)
	{
		return location?.GetSeason() ?? season;
	}

	/// <summary>Get the season which currently applies to a location as a numeric index.</summary>
	/// <param name="location">The location to check, or <c>null</c> for the global season.</param>
	/// <remarks>Most code should use <see cref="M:StardewValley.Game1.GetSeasonForLocation(StardewValley.GameLocation)" /> instead.</remarks>
	public static int GetSeasonIndexForLocation(GameLocation location)
	{
		return location?.GetSeasonIndex() ?? seasonIndex;
	}

	/// <summary>Get the season which currently applies to a location as a string.</summary>
	/// <param name="location">The location to check, or <c>null</c> for the global season.</param>
	/// <remarks>Most code should use <see cref="M:StardewValley.Game1.GetSeasonForLocation(StardewValley.GameLocation)" /> instead.</remarks>
	public static string GetSeasonKeyForLocation(GameLocation location)
	{
		return location?.GetSeasonKey() ?? currentSeason;
	}

	/// <summary>Unlock an achievement for the current platform.</summary>
	/// <param name="which">The achievement to unlock.</param>
	public static void getPlatformAchievement(string which)
	{
		Program.sdk.GetAchievement(which);
	}

	public static void getSteamAchievement(string which)
	{
		if (which.Equals("0"))
		{
			which = "a0";
		}
		getPlatformAchievement(which);
	}

	public static void getAchievement(int which, bool allowBroadcasting = true)
	{
		if (player.achievements.Contains(which) || gameMode != 3 || !DataLoader.Achievements(content).TryGetValue(which, out var rawData))
		{
			return;
		}
		string achievementName = rawData.Split('^')[0];
		player.achievements.Add(which);
		if (which < 32 && allowBroadcasting)
		{
			if (stats.isSharedAchievement(which))
			{
				multiplayer.sendSharedAchievementMessage(which);
			}
			else
			{
				string farmerName = player.Name;
				if (farmerName == "")
				{
					farmerName = TokenStringBuilder.LocalizedText("Strings\\UI:Chat_PlayerJoinedNewName");
				}
				multiplayer.globalChatInfoMessage("Achievement", farmerName, TokenStringBuilder.AchievementName(which));
			}
		}
		playSound("achievement");
		addHUDMessage(HUDMessage.ForAchievement(achievementName));
		player.autoGenerateActiveDialogueEvent("achievement_" + which);
		getPlatformAchievement(which.ToString());
		if (!player.hasOrWillReceiveMail("hatter"))
		{
			addMailForTomorrow("hatter");
		}
	}

	public static void createMultipleObjectDebris(string id, int xTile, int yTile, int number)
	{
		for (int i = 0; i < number; i++)
		{
			createObjectDebris(id, xTile, yTile);
		}
	}

	public static void createMultipleObjectDebris(string id, int xTile, int yTile, int number, GameLocation location)
	{
		for (int i = 0; i < number; i++)
		{
			createObjectDebris(id, xTile, yTile, -1, 0, 1f, location);
		}
	}

	public static void createMultipleObjectDebris(string id, int xTile, int yTile, int number, float velocityMultiplier)
	{
		for (int i = 0; i < number; i++)
		{
			createObjectDebris(id, xTile, yTile, -1, 0, velocityMultiplier);
		}
	}

	public static void createMultipleObjectDebris(string id, int xTile, int yTile, int number, long who)
	{
		for (int i = 0; i < number; i++)
		{
			createObjectDebris(id, xTile, yTile, who);
		}
	}

	public static void createMultipleObjectDebris(string id, int xTile, int yTile, int number, long who, GameLocation location)
	{
		for (int i = 0; i < number; i++)
		{
			createObjectDebris(id, xTile, yTile, who, location);
		}
	}

	public static void createDebris(int debrisType, int xTile, int yTile, int numberOfChunks)
	{
		createDebris(debrisType, xTile, yTile, numberOfChunks, currentLocation);
	}

	public static void createDebris(int debrisType, int xTile, int yTile, int numberOfChunks, GameLocation location)
	{
		if (location == null)
		{
			location = currentLocation;
		}
		location.debris.Add(new Debris(debrisType, numberOfChunks, new Vector2(xTile * 64 + 32, yTile * 64 + 32), player.getStandingPosition()));
	}

	public static Debris createItemDebris(Item item, Vector2 pixelOrigin, int direction, GameLocation location = null, int groundLevel = -1, bool flopFish = false)
	{
		if (location == null)
		{
			location = currentLocation;
		}
		Vector2 targetLocation = new Vector2(pixelOrigin.X, pixelOrigin.Y);
		switch (direction)
		{
		case 0:
			pixelOrigin.Y -= 16f + (float)recentMultiplayerRandom.Next(32);
			targetLocation.Y -= 35.2f;
			break;
		case 1:
			pixelOrigin.X += 16f;
			pixelOrigin.Y -= 32 - recentMultiplayerRandom.Next(8);
			targetLocation.X += 128f;
			break;
		case 2:
			pixelOrigin.Y += recentMultiplayerRandom.Next(16);
			targetLocation.Y += 64f;
			break;
		case 3:
			pixelOrigin.X -= 16f;
			pixelOrigin.Y -= 32 - recentMultiplayerRandom.Next(8);
			targetLocation.X -= 128f;
			break;
		case -1:
			targetLocation = player.getStandingPosition();
			break;
		}
		Debris d = new Debris(item, pixelOrigin, targetLocation);
		if (flopFish && item.Category == -4)
		{
			d.floppingFish.Value = true;
		}
		if (groundLevel != -1)
		{
			d.chunkFinalYLevel = groundLevel;
		}
		location.debris.Add(d);
		return d;
	}

	public static void createMultipleItemDebris(Item item, Vector2 pixelOrigin, int direction, GameLocation location = null, int groundLevel = -1, bool flopFish = false)
	{
		int stack = item.Stack;
		item.Stack = 1;
		createItemDebris(item, pixelOrigin, (direction == -1) ? random.Next(4) : direction, location, groundLevel, flopFish);
		for (int i = 1; i < stack; i++)
		{
			createItemDebris(item.getOne(), pixelOrigin, (direction == -1) ? random.Next(4) : direction, location, groundLevel, flopFish);
		}
	}

	public static void createRadialDebris(GameLocation location, int debrisType, int xTile, int yTile, int numberOfChunks, bool resource, int groundLevel = -1, bool item = false, Color? color = null)
	{
		if (groundLevel == -1)
		{
			groundLevel = yTile * 64 + 32;
		}
		Vector2 debrisOrigin = new Vector2(xTile * 64 + 64, yTile * 64 + 64);
		if (item)
		{
			while (numberOfChunks > 0)
			{
				Vector2 offset = random.Next(4) switch
				{
					0 => new Vector2(-64f, 0f), 
					1 => new Vector2(64f, 0f), 
					2 => new Vector2(0f, 64f), 
					_ => new Vector2(0f, -64f), 
				};
				Item debris = ItemRegistry.Create("(O)" + debrisType);
				location.debris.Add(new Debris(debris, debrisOrigin, debrisOrigin + offset));
				numberOfChunks--;
			}
		}
		if (resource)
		{
			location.debris.Add(new Debris(debrisType, numberOfChunks / 4, debrisOrigin, debrisOrigin + new Vector2(-64f, 0f)));
			numberOfChunks++;
			location.debris.Add(new Debris(debrisType, numberOfChunks / 4, debrisOrigin, debrisOrigin + new Vector2(64f, 0f)));
			numberOfChunks++;
			location.debris.Add(new Debris(debrisType, numberOfChunks / 4, debrisOrigin, debrisOrigin + new Vector2(0f, -64f)));
			numberOfChunks++;
			location.debris.Add(new Debris(debrisType, numberOfChunks / 4, debrisOrigin, debrisOrigin + new Vector2(0f, 64f)));
		}
		else
		{
			location.debris.Add(new Debris(debrisType, numberOfChunks / 4, debrisOrigin, debrisOrigin + new Vector2(-64f, 0f), groundLevel, color));
			numberOfChunks++;
			location.debris.Add(new Debris(debrisType, numberOfChunks / 4, debrisOrigin, debrisOrigin + new Vector2(64f, 0f), groundLevel, color));
			numberOfChunks++;
			location.debris.Add(new Debris(debrisType, numberOfChunks / 4, debrisOrigin, debrisOrigin + new Vector2(0f, -64f), groundLevel, color));
			numberOfChunks++;
			location.debris.Add(new Debris(debrisType, numberOfChunks / 4, debrisOrigin, debrisOrigin + new Vector2(0f, 64f), groundLevel, color));
		}
	}

	public static void createRadialDebris(GameLocation location, string texture, Microsoft.Xna.Framework.Rectangle sourcerectangle, int xTile, int yTile, int numberOfChunks)
	{
		createRadialDebris(location, texture, sourcerectangle, xTile, yTile, numberOfChunks, yTile);
	}

	public static void createRadialDebris(GameLocation location, string texture, Microsoft.Xna.Framework.Rectangle sourcerectangle, int xTile, int yTile, int numberOfChunks, int groundLevelTile)
	{
		createRadialDebris(location, texture, sourcerectangle, 8, xTile * 64 + 32 + random.Next(32), yTile * 64 + 32 + random.Next(32), numberOfChunks, groundLevelTile);
	}

	public static void createRadialDebris(GameLocation location, string texture, Microsoft.Xna.Framework.Rectangle sourcerectangle, int sizeOfSourceRectSquares, int xPosition, int yPosition, int numberOfChunks, int groundLevelTile)
	{
		Vector2 debrisOrigin = new Vector2(xPosition, yPosition);
		location.debris.Add(new Debris(texture, sourcerectangle, numberOfChunks / 4, debrisOrigin, debrisOrigin + new Vector2(-64f, 0f), groundLevelTile * 64, sizeOfSourceRectSquares));
		location.debris.Add(new Debris(texture, sourcerectangle, numberOfChunks / 4, debrisOrigin, debrisOrigin + new Vector2(64f, 0f), groundLevelTile * 64, sizeOfSourceRectSquares));
		location.debris.Add(new Debris(texture, sourcerectangle, numberOfChunks / 4, debrisOrigin, debrisOrigin + new Vector2(0f, -64f), groundLevelTile * 64, sizeOfSourceRectSquares));
		location.debris.Add(new Debris(texture, sourcerectangle, numberOfChunks / 4, debrisOrigin, debrisOrigin + new Vector2(0f, 64f), groundLevelTile * 64, sizeOfSourceRectSquares));
	}

	public static void createRadialDebris_MoreNatural(GameLocation location, string texture, Microsoft.Xna.Framework.Rectangle sourcerectangle, int sizeOfSourceRectSquares, int xPosition, int yPosition, int numberOfChunks, int groundLevel)
	{
		Vector2 debrisOrigin = new Vector2(xPosition, yPosition);
		for (int i = 0; i < numberOfChunks; i++)
		{
			location.debris.Add(new Debris(texture, sourcerectangle, numberOfChunks / 4, debrisOrigin, debrisOrigin + new Vector2(random.Next(-64, 64), random.Next(-64, 64)), groundLevel + random.Next(-32, 32), sizeOfSourceRectSquares));
		}
	}

	public static void createRadialDebris(GameLocation location, string texture, Microsoft.Xna.Framework.Rectangle sourcerectangle, int sizeOfSourceRectSquares, int xPosition, int yPosition, int numberOfChunks, int groundLevelTile, Color color)
	{
		createRadialDebris(location, texture, sourcerectangle, sizeOfSourceRectSquares, xPosition, yPosition, numberOfChunks, groundLevelTile, color, 1f);
	}

	public static void createRadialDebris(GameLocation location, string texture, Microsoft.Xna.Framework.Rectangle sourcerectangle, int sizeOfSourceRectSquares, int xPosition, int yPosition, int numberOfChunks, int groundLevelTile, Color color, float scale)
	{
		Vector2 debrisOrigin = new Vector2(xPosition, yPosition);
		while (numberOfChunks > 0)
		{
			switch (random.Next(4))
			{
			case 0:
			{
				Debris d = new Debris(texture, sourcerectangle, 1, debrisOrigin, debrisOrigin + new Vector2(-64f, 0f), groundLevelTile * 64, sizeOfSourceRectSquares);
				d.nonSpriteChunkColor.Value = color;
				location?.debris.Add(d);
				d.Chunks[0].scale = scale;
				break;
			}
			case 1:
			{
				Debris d = new Debris(texture, sourcerectangle, 1, debrisOrigin, debrisOrigin + new Vector2(64f, 0f), groundLevelTile * 64, sizeOfSourceRectSquares);
				d.nonSpriteChunkColor.Value = color;
				location?.debris.Add(d);
				d.Chunks[0].scale = scale;
				break;
			}
			case 2:
			{
				Debris d = new Debris(texture, sourcerectangle, 1, debrisOrigin, debrisOrigin + new Vector2(random.Next(-64, 64), -64f), groundLevelTile * 64, sizeOfSourceRectSquares);
				d.nonSpriteChunkColor.Value = color;
				location?.debris.Add(d);
				d.Chunks[0].scale = scale;
				break;
			}
			case 3:
			{
				Debris d = new Debris(texture, sourcerectangle, 1, debrisOrigin, debrisOrigin + new Vector2(random.Next(-64, 64), 64f), groundLevelTile * 64, sizeOfSourceRectSquares);
				d.nonSpriteChunkColor.Value = color;
				location?.debris.Add(d);
				d.Chunks[0].scale = scale;
				break;
			}
			}
			numberOfChunks--;
		}
	}

	public static void createObjectDebris(string id, int xTile, int yTile, long whichPlayer)
	{
		currentLocation.debris.Add(new Debris(id, new Vector2(xTile * 64 + 32, yTile * 64 + 32), getFarmer(whichPlayer).getStandingPosition()));
	}

	public static void createObjectDebris(string id, int xTile, int yTile, long whichPlayer, GameLocation location)
	{
		location.debris.Add(new Debris(id, new Vector2(xTile * 64 + 32, yTile * 64 + 32), getFarmer(whichPlayer).getStandingPosition()));
	}

	public static void createObjectDebris(string id, int xTile, int yTile, GameLocation location)
	{
		createObjectDebris(id, xTile, yTile, -1, 0, 1f, location);
	}

	public static void createObjectDebris(string id, int xTile, int yTile, int groundLevel = -1, int itemQuality = 0, float velocityMultiplyer = 1f, GameLocation location = null)
	{
		if (location == null)
		{
			location = currentLocation;
		}
		Debris d = new Debris(id, new Vector2(xTile * 64 + 32, yTile * 64 + 32), player.getStandingPosition())
		{
			itemQuality = itemQuality
		};
		foreach (Chunk chunk in d.Chunks)
		{
			chunk.xVelocity.Value *= velocityMultiplyer;
			chunk.yVelocity.Value *= velocityMultiplyer;
		}
		if (groundLevel != -1)
		{
			d.chunkFinalYLevel = groundLevel;
		}
		location.debris.Add(d);
	}

	public static Farmer getFarmer(long id)
	{
		if (player.UniqueMultiplayerID == id)
		{
			return player;
		}
		if (otherFarmers.TryGetValue(id, out var otherFarmer))
		{
			return otherFarmer;
		}
		if (!IsMultiplayer)
		{
			return player;
		}
		return MasterPlayer;
	}

	public static Farmer getFarmerMaybeOffline(long id)
	{
		if (MasterPlayer.UniqueMultiplayerID == id)
		{
			return MasterPlayer;
		}
		if (otherFarmers.TryGetValue(id, out var otherFarmer))
		{
			return otherFarmer;
		}
		if (netWorldState.Value.farmhandData.TryGetValue(id, out var farmhand))
		{
			return farmhand;
		}
		return null;
	}

	/// <summary>Get all players including the host, online farmhands, and offline farmhands.</summary>
	public static IEnumerable<Farmer> getAllFarmers()
	{
		return Enumerable.Repeat(MasterPlayer, 1).Concat(getAllFarmhands());
	}

	/// <summary>Get all players who are currently connected, including the host player.</summary>
	public static FarmerCollection getOnlineFarmers()
	{
		return _onlineFarmers;
	}

	/// <summary>Get online and offline farmhands.</summary>
	public static IEnumerable<Farmer> getAllFarmhands()
	{
		foreach (Farmer farmer in netWorldState.Value.farmhandData.Values)
		{
			if (farmer.isActive())
			{
				yield return otherFarmers[farmer.UniqueMultiplayerID];
			}
			else
			{
				yield return farmer;
			}
		}
	}

	/// <summary>Get farmhands which aren't currently connected.</summary>
	public static IEnumerable<Farmer> getOfflineFarmhands()
	{
		foreach (Farmer farmer in netWorldState.Value.farmhandData.Values)
		{
			if (!farmer.isActive())
			{
				yield return farmer;
			}
		}
	}

	public static void farmerFindsArtifact(string itemId)
	{
		Item item = ItemRegistry.Create(itemId);
		player.addItemToInventoryBool(item);
	}

	public static bool doesHUDMessageExist(string s)
	{
		for (int i = 0; i < hudMessages.Count; i++)
		{
			if (s.Equals(hudMessages[i].message))
			{
				return true;
			}
		}
		return false;
	}

	public static void addHUDMessage(HUDMessage message)
	{
		if (message.type != null || message.whatType != 0)
		{
			for (int i = 0; i < hudMessages.Count; i++)
			{
				if (message.type != null && message.type == hudMessages[i].type)
				{
					hudMessages[i].number = hudMessages[i].number + message.number;
					hudMessages[i].timeLeft = 3500f;
					hudMessages[i].transparency = 1f;
					if (hudMessages[i].number > 50000)
					{
						HUDMessage.numbersEasterEgg(hudMessages[i].number);
					}
					return;
				}
				if (message.whatType == hudMessages[i].whatType && message.whatType != 1 && message.message != null && message.message.Equals(hudMessages[i].message))
				{
					hudMessages[i].timeLeft = message.timeLeft;
					hudMessages[i].transparency = 1f;
					return;
				}
			}
		}
		hudMessages.Add(message);
		for (int i = hudMessages.Count - 1; i >= 0; i--)
		{
			if (hudMessages[i].noIcon)
			{
				HUDMessage tmp = hudMessages[i];
				hudMessages.RemoveAt(i);
				hudMessages.Add(tmp);
			}
		}
	}

	public static void showSwordswipeAnimation(int direction, Vector2 source, float animationSpeed, bool flip)
	{
		switch (direction)
		{
		case 0:
			currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite(-1, animationSpeed, 5, 1, new Vector2(source.X + 32f, source.Y), flicker: false, flipped: false, !flip, -(float)Math.PI / 2f));
			break;
		case 1:
			currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite(-1, animationSpeed, 5, 1, new Vector2(source.X + 96f + 16f, source.Y + 48f), flicker: false, flip, verticalFlipped: false, flip ? (-(float)Math.PI) : 0f));
			break;
		case 2:
			currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite(-1, animationSpeed, 5, 1, new Vector2(source.X + 32f, source.Y + 128f), flicker: false, flipped: false, !flip, (float)Math.PI / 2f));
			break;
		case 3:
			currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite(-1, animationSpeed, 5, 1, new Vector2(source.X - 32f - 16f, source.Y + 48f), flicker: false, !flip, verticalFlipped: false, flip ? (-(float)Math.PI) : 0f));
			break;
		}
	}

	public static void removeDebris(Debris.DebrisType type)
	{
		currentLocation.debris.RemoveWhere((Debris debris) => debris.debrisType.Value == type);
	}

	public static void toolAnimationDone(Farmer who)
	{
		float oldStamina = player.Stamina;
		if (who.CurrentTool == null)
		{
			return;
		}
		if (who.Stamina > 0f)
		{
			int powerupLevel = 1;
			Vector2 actionTile = who.GetToolLocation();
			if (who.CurrentTool is FishingRod { isFishing: not false })
			{
				who.canReleaseTool = false;
			}
			else if (!(who.CurrentTool is FishingRod))
			{
				who.UsingTool = false;
				if (who.CurrentTool.QualifiedItemId == "(T)WateringCan")
				{
					switch (who.FacingDirection)
					{
					case 0:
					case 2:
						who.CurrentTool.DoFunction(currentLocation, (int)actionTile.X, (int)actionTile.Y, powerupLevel, who);
						break;
					case 1:
					case 3:
						who.CurrentTool.DoFunction(currentLocation, (int)actionTile.X, (int)actionTile.Y, powerupLevel, who);
						break;
					}
				}
				else if (who.CurrentTool is MeleeWeapon)
				{
					who.CurrentTool.CurrentParentTileIndex = who.CurrentTool.IndexOfMenuItemView;
				}
				else
				{
					if (who.CurrentTool.QualifiedItemId == "(T)ReturnScepter")
					{
						who.CurrentTool.CurrentParentTileIndex = who.CurrentTool.IndexOfMenuItemView;
					}
					who.CurrentTool.DoFunction(currentLocation, (int)actionTile.X, (int)actionTile.Y, powerupLevel, who);
				}
			}
			else
			{
				who.UsingTool = false;
			}
		}
		else if ((bool)who.CurrentTool.instantUse)
		{
			who.CurrentTool.DoFunction(currentLocation, 0, 0, 0, who);
		}
		else
		{
			who.UsingTool = false;
		}
		who.lastClick = Vector2.Zero;
		if (who.IsLocalPlayer && !GetKeyboardState().IsKeyDown(Keys.LeftShift))
		{
			who.setRunning(options.autoRun);
		}
		if (!who.UsingTool && who.FarmerSprite.PauseForSingleAnimation)
		{
			who.FarmerSprite.StopAnimation();
		}
		if (player.Stamina <= 0f && oldStamina > 0f)
		{
			player.doEmote(36);
		}
	}

	public static bool pressActionButton(KeyboardState currentKBState, MouseState currentMouseState, GamePadState currentPadState)
	{
		if (IsChatting)
		{
			currentKBState = default(KeyboardState);
		}
		if (dialogueTyping)
		{
			bool consume = true;
			dialogueTyping = false;
			if (currentSpeaker != null)
			{
				currentDialogueCharacterIndex = currentSpeaker.CurrentDialogue.Peek().getCurrentDialogue().Length;
			}
			else if (currentObjectDialogue.Count > 0)
			{
				currentDialogueCharacterIndex = currentObjectDialogue.Peek().Length;
			}
			else
			{
				consume = false;
			}
			dialogueTypingInterval = 0;
			oldKBState = currentKBState;
			oldMouseState = input.GetMouseState();
			oldPadState = currentPadState;
			if (consume)
			{
				playSound("dialogueCharacterClose");
				return false;
			}
		}
		if (dialogueUp)
		{
			if (isQuestion)
			{
				isQuestion = false;
				if (currentSpeaker != null)
				{
					if (currentSpeaker.CurrentDialogue.Peek().chooseResponse(questionChoices[currentQuestionChoice]))
					{
						currentDialogueCharacterIndex = 1;
						dialogueTyping = true;
						oldKBState = currentKBState;
						oldMouseState = input.GetMouseState();
						oldPadState = currentPadState;
						return false;
					}
				}
				else
				{
					dialogueUp = false;
					if (eventUp && currentLocation.afterQuestion == null)
					{
						currentLocation.currentEvent.answerDialogue(currentLocation.lastQuestionKey, currentQuestionChoice);
						currentQuestionChoice = 0;
						oldKBState = currentKBState;
						oldMouseState = input.GetMouseState();
						oldPadState = currentPadState;
					}
					else if (currentLocation.answerDialogue(questionChoices[currentQuestionChoice]))
					{
						currentQuestionChoice = 0;
						oldKBState = currentKBState;
						oldMouseState = input.GetMouseState();
						oldPadState = currentPadState;
						return false;
					}
					if (dialogueUp)
					{
						currentDialogueCharacterIndex = 1;
						dialogueTyping = true;
						oldKBState = currentKBState;
						oldMouseState = input.GetMouseState();
						oldPadState = currentPadState;
						return false;
					}
				}
				currentQuestionChoice = 0;
			}
			string exitDialogue = null;
			if (currentSpeaker != null)
			{
				if (currentSpeaker.immediateSpeak)
				{
					currentSpeaker.immediateSpeak = false;
					return false;
				}
				exitDialogue = ((currentSpeaker.CurrentDialogue.Count > 0) ? currentSpeaker.CurrentDialogue.Peek().exitCurrentDialogue() : null);
			}
			if (exitDialogue == null)
			{
				if (currentSpeaker != null && currentSpeaker.CurrentDialogue.Count > 0 && currentSpeaker.CurrentDialogue.Peek().isOnFinalDialogue() && currentSpeaker.CurrentDialogue.Count > 0)
				{
					currentSpeaker.CurrentDialogue.Pop();
				}
				dialogueUp = false;
				if (messagePause)
				{
					pauseTime = 500f;
				}
				if (currentObjectDialogue.Count > 0)
				{
					currentObjectDialogue.Dequeue();
				}
				currentDialogueCharacterIndex = 0;
				if (currentObjectDialogue.Count > 0)
				{
					dialogueUp = true;
					questionChoices.Clear();
					oldKBState = currentKBState;
					oldMouseState = input.GetMouseState();
					oldPadState = currentPadState;
					dialogueTyping = true;
					return false;
				}
				if (currentSpeaker != null && !currentSpeaker.Name.Equals("Gunther") && !eventUp && !currentSpeaker.doingEndOfRouteAnimation)
				{
					currentSpeaker.doneFacingPlayer(player);
				}
				currentSpeaker = null;
				if (!eventUp)
				{
					player.CanMove = true;
				}
				else if (currentLocation.currentEvent.CurrentCommand > 0 || currentLocation.currentEvent.specialEventVariable1)
				{
					if (!isFestival() || !currentLocation.currentEvent.canMoveAfterDialogue())
					{
						currentLocation.currentEvent.CurrentCommand++;
					}
					else
					{
						player.CanMove = true;
					}
				}
				questionChoices.Clear();
				playSound("smallSelect");
			}
			else
			{
				playSound("smallSelect");
				currentDialogueCharacterIndex = 0;
				dialogueTyping = true;
				checkIfDialogueIsQuestion();
			}
			oldKBState = currentKBState;
			oldMouseState = input.GetMouseState();
			oldPadState = currentPadState;
			if (questOfTheDay != null && (bool)questOfTheDay.accepted && questOfTheDay is SocializeQuest)
			{
				((SocializeQuest)questOfTheDay).checkIfComplete(null, -1, -1);
			}
			return false;
		}
		if (!player.UsingTool && (!eventUp || (currentLocation.currentEvent != null && currentLocation.currentEvent.playerControlSequence)) && !fadeToBlack)
		{
			if (wasMouseVisibleThisFrame && currentLocation.animals.Length > 0)
			{
				Vector2 mousePosition = new Vector2(getOldMouseX() + viewport.X, getOldMouseY() + viewport.Y);
				if (Utility.withinRadiusOfPlayer((int)mousePosition.X, (int)mousePosition.Y, 1, player))
				{
					if (currentLocation.CheckPetAnimal(mousePosition, player))
					{
						return true;
					}
					if (didPlayerJustRightClick(ignoreNonMouseHeldInput: true) && currentLocation.CheckInspectAnimal(mousePosition, player))
					{
						return true;
					}
				}
			}
			Vector2 grabTile = new Vector2(getOldMouseX() + viewport.X, getOldMouseY() + viewport.Y) / 64f;
			Vector2 cursorTile = grabTile;
			bool non_directed_tile = false;
			if (!wasMouseVisibleThisFrame || mouseCursorTransparency == 0f || !Utility.tileWithinRadiusOfPlayer((int)grabTile.X, (int)grabTile.Y, 1, player))
			{
				grabTile = player.GetGrabTile();
				non_directed_tile = true;
			}
			bool was_character_at_grab_tile = false;
			if (eventUp && !isFestival())
			{
				CurrentEvent?.receiveActionPress((int)grabTile.X, (int)grabTile.Y);
				oldKBState = currentKBState;
				oldMouseState = input.GetMouseState();
				oldPadState = currentPadState;
				return false;
			}
			if (tryToCheckAt(grabTile, player))
			{
				return false;
			}
			if (player.isRidingHorse())
			{
				player.mount.checkAction(player, player.currentLocation);
				return false;
			}
			if (!player.canMove)
			{
				return false;
			}
			if (!was_character_at_grab_tile && player.currentLocation.isCharacterAtTile(grabTile) != null)
			{
				was_character_at_grab_tile = true;
			}
			bool isPlacingObject = false;
			if (player.ActiveObject != null && !(player.ActiveObject is Furniture))
			{
				if (player.ActiveObject.performUseAction(currentLocation))
				{
					player.reduceActiveItemByOne();
					oldKBState = currentKBState;
					oldMouseState = input.GetMouseState();
					oldPadState = currentPadState;
					return false;
				}
				int stack = player.ActiveObject.Stack;
				isCheckingNonMousePlacement = !IsPerformingMousePlacement();
				if (non_directed_tile)
				{
					isCheckingNonMousePlacement = true;
				}
				if (isOneOfTheseKeysDown(currentKBState, options.actionButton))
				{
					isCheckingNonMousePlacement = true;
				}
				Vector2 valid_position = Utility.GetNearbyValidPlacementPosition(player, currentLocation, player.ActiveObject, (int)grabTile.X * 64 + 32, (int)grabTile.Y * 64 + 32);
				if (!isCheckingNonMousePlacement && player.ActiveObject is Wallpaper && Utility.tryToPlaceItem(currentLocation, player.ActiveObject, (int)cursorTile.X * 64, (int)cursorTile.Y * 64))
				{
					isCheckingNonMousePlacement = false;
					return true;
				}
				if (Utility.tryToPlaceItem(currentLocation, player.ActiveObject, (int)valid_position.X, (int)valid_position.Y))
				{
					isCheckingNonMousePlacement = false;
					return true;
				}
				if (!eventUp && (player.ActiveObject == null || player.ActiveObject.Stack < stack || player.ActiveObject.isPlaceable()))
				{
					isPlacingObject = true;
				}
				isCheckingNonMousePlacement = false;
			}
			if (!isPlacingObject && !was_character_at_grab_tile)
			{
				grabTile.Y += 1f;
				if (player.FacingDirection >= 0 && player.FacingDirection <= 3)
				{
					Vector2 normalized_offset = grabTile - player.Tile;
					if (normalized_offset.X > 0f || normalized_offset.Y > 0f)
					{
						normalized_offset.Normalize();
					}
					if (Vector2.Dot(Utility.DirectionsTileVectors[player.FacingDirection], normalized_offset) >= 0f && tryToCheckAt(grabTile, player))
					{
						return false;
					}
				}
				if (!eventUp && player.ActiveObject is Furniture furniture)
				{
					furniture.rotate();
					playSound("dwoop");
					oldKBState = currentKBState;
					oldMouseState = input.GetMouseState();
					oldPadState = currentPadState;
					return false;
				}
				grabTile.Y -= 2f;
				if (player.FacingDirection >= 0 && player.FacingDirection <= 3 && !was_character_at_grab_tile)
				{
					Vector2 normalized_offset = grabTile - player.Tile;
					if (normalized_offset.X > 0f || normalized_offset.Y > 0f)
					{
						normalized_offset.Normalize();
					}
					if (Vector2.Dot(Utility.DirectionsTileVectors[player.FacingDirection], normalized_offset) >= 0f && tryToCheckAt(grabTile, player))
					{
						return false;
					}
				}
				if (!eventUp && player.ActiveObject is Furniture furniture)
				{
					furniture.rotate();
					playSound("dwoop");
					oldKBState = currentKBState;
					oldMouseState = input.GetMouseState();
					oldPadState = currentPadState;
					return false;
				}
				grabTile = player.Tile;
				if (tryToCheckAt(grabTile, player))
				{
					return false;
				}
				if (!eventUp && player.ActiveObject is Furniture furniture)
				{
					furniture.rotate();
					playSound("dwoop");
					oldKBState = currentKBState;
					oldMouseState = input.GetMouseState();
					oldPadState = currentPadState;
					return false;
				}
			}
			if (!player.isEating && player.ActiveObject != null && !dialogueUp && !eventUp && !player.canOnlyWalk && !player.FarmerSprite.PauseForSingleAnimation && !fadeToBlack && player.ActiveObject.Edibility != -300 && didPlayerJustRightClick(ignoreNonMouseHeldInput: true))
			{
				if (player.team.SpecialOrderRuleActive("SC_NO_FOOD"))
				{
					MineShaft obj = player.currentLocation as MineShaft;
					if (obj != null && obj.getMineArea() == 121)
					{
						addHUDMessage(new HUDMessage(content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"), 3));
						return false;
					}
				}
				if (player.hasBuff("25") && player.ActiveObject != null && !player.ActiveObject.HasContextTag("ginger_item"))
				{
					addHUDMessage(new HUDMessage(content.LoadString("Strings\\StringsFromCSFiles:Nauseous_CantEat"), 3));
					return false;
				}
				player.faceDirection(2);
				player.itemToEat = player.ActiveObject;
				player.FarmerSprite.setCurrentSingleAnimation(304);
				if (Game1.objectData.TryGetValue(player.ActiveObject.ItemId, out var objectData))
				{
					currentLocation.createQuestionDialogue((objectData.IsDrink && player.ActiveObject.preserve.Value != Object.PreserveType.Pickle) ? content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3159", player.ActiveObject.DisplayName) : content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3160", player.ActiveObject.DisplayName), currentLocation.createYesNoResponses(), "Eat");
				}
				oldKBState = currentKBState;
				oldMouseState = input.GetMouseState();
				oldPadState = currentPadState;
				return false;
			}
		}
		if (player.CurrentTool is MeleeWeapon && player.CanMove && !player.canOnlyWalk && !eventUp && !player.onBridge && didPlayerJustRightClick(ignoreNonMouseHeldInput: true))
		{
			((MeleeWeapon)player.CurrentTool).animateSpecialMove(player);
			return false;
		}
		return true;
	}

	public static bool IsPerformingMousePlacement()
	{
		if (mouseCursorTransparency == 0f || !wasMouseVisibleThisFrame || (!lastCursorMotionWasMouse && (player.ActiveObject == null || (!player.ActiveObject.isPlaceable() && player.ActiveObject.Category != -74 && !player.ActiveObject.isSapling()))))
		{
			return false;
		}
		return true;
	}

	public static Vector2 GetPlacementGrabTile()
	{
		if (!IsPerformingMousePlacement())
		{
			return player.GetGrabTile();
		}
		return new Vector2(getOldMouseX() + viewport.X, getOldMouseY() + viewport.Y) / 64f;
	}

	public static bool tryToCheckAt(Vector2 grabTile, Farmer who)
	{
		if (player.onBridge.Value)
		{
			return false;
		}
		haltAfterCheck = true;
		if (Utility.tileWithinRadiusOfPlayer((int)grabTile.X, (int)grabTile.Y, 1, player) && hooks.OnGameLocation_CheckAction(currentLocation, new Location((int)grabTile.X, (int)grabTile.Y), viewport, who, () => currentLocation.checkAction(new Location((int)grabTile.X, (int)grabTile.Y), viewport, who)))
		{
			updateCursorTileHint();
			who.lastGrabTile = grabTile;
			if (who.CanMove && haltAfterCheck)
			{
				who.faceGeneralDirection(grabTile * 64f);
				who.Halt();
			}
			oldKBState = GetKeyboardState();
			oldMouseState = input.GetMouseState();
			oldPadState = input.GetGamePadState();
			return true;
		}
		return false;
	}

	public static void pressSwitchToolButton()
	{
		if (player.netItemStowed.Value)
		{
			player.netItemStowed.Set(newValue: false);
			player.UpdateItemStow();
		}
		int whichWay = ((input.GetMouseState().ScrollWheelValue > oldMouseState.ScrollWheelValue) ? (-1) : ((input.GetMouseState().ScrollWheelValue < oldMouseState.ScrollWheelValue) ? 1 : 0));
		if (options.gamepadControls && whichWay == 0)
		{
			if (input.GetGamePadState().IsButtonDown(Buttons.LeftTrigger))
			{
				whichWay = -1;
			}
			else if (input.GetGamePadState().IsButtonDown(Buttons.RightTrigger))
			{
				whichWay = 1;
			}
		}
		if (options.invertScrollDirection)
		{
			whichWay *= -1;
		}
		if (whichWay == 0)
		{
			return;
		}
		player.CurrentToolIndex = (player.CurrentToolIndex + whichWay) % 12;
		if (player.CurrentToolIndex < 0)
		{
			player.CurrentToolIndex = 11;
		}
		for (int i = 0; i < 12; i++)
		{
			if (player.CurrentItem != null)
			{
				break;
			}
			player.CurrentToolIndex = (whichWay + player.CurrentToolIndex) % 12;
			if (player.CurrentToolIndex < 0)
			{
				player.CurrentToolIndex = 11;
			}
		}
		playSound("toolSwap");
		if (player.ActiveObject != null)
		{
			player.showCarrying();
		}
		else
		{
			player.showNotCarrying();
		}
	}

	public static bool pressUseToolButton()
	{
		bool stow_was_initialized = game1._didInitiateItemStow;
		game1._didInitiateItemStow = false;
		if (fadeToBlack)
		{
			return false;
		}
		player.toolPower.Value = 0;
		player.toolHold.Value = 0;
		bool did_attempt_object_removal = false;
		if (player.CurrentTool == null && player.ActiveObject == null)
		{
			Vector2 c = player.GetToolLocation() / 64f;
			c.X = (int)c.X;
			c.Y = (int)c.Y;
			if (currentLocation.Objects.TryGetValue(c, out var o) && !o.readyForHarvest && o.heldObject.Value == null && !(o is Fence) && !(o is CrabPot) && (o.Type == "Crafting" || o.Type == "interactive") && !o.IsTwig())
			{
				did_attempt_object_removal = true;
				o.setHealth(o.getHealth() - 1);
				o.shakeTimer = 300;
				o.playNearbySoundAll("hammer");
				if (o.getHealth() < 2)
				{
					o.playNearbySoundAll("hammer");
					if (o.getHealth() < 1)
					{
						Tool t = ItemRegistry.Create<Tool>("(T)Pickaxe");
						t.DoFunction(currentLocation, -1, -1, 0, player);
						if (o.performToolAction(t))
						{
							o.performRemoveAction();
							if (o.Type == "Crafting" && (int)o.fragility != 2)
							{
								currentLocation.debris.Add(new Debris(o.QualifiedItemId, player.GetToolLocation(), Utility.PointToVector2(player.StandingPixel)));
							}
							currentLocation.Objects.Remove(c);
							return true;
						}
					}
				}
			}
		}
		if (currentMinigame == null && !player.UsingTool && (player.IsSitting() || player.isRidingHorse() || player.onBridge.Value || dialogueUp || (eventUp && !CurrentEvent.canPlayerUseTool() && (!currentLocation.currentEvent.playerControlSequence || (activeClickableMenu == null && currentMinigame == null))) || (player.CurrentTool != null && (currentLocation.doesPositionCollideWithCharacter(Utility.getRectangleCenteredAt(player.GetToolLocation(), 64), ignoreMonsters: true)?.IsVillager ?? false))))
		{
			pressActionButton(GetKeyboardState(), input.GetMouseState(), input.GetGamePadState());
			return false;
		}
		if (player.canOnlyWalk)
		{
			return true;
		}
		Vector2 position = ((!wasMouseVisibleThisFrame) ? player.GetToolLocation() : new Vector2(getOldMouseX() + viewport.X, getOldMouseY() + viewport.Y));
		if (Utility.canGrabSomethingFromHere((int)position.X, (int)position.Y, player))
		{
			Vector2 tile = new Vector2(position.X / 64f, position.Y / 64f);
			if (hooks.OnGameLocation_CheckAction(currentLocation, new Location((int)tile.X, (int)tile.Y), viewport, player, () => currentLocation.checkAction(new Location((int)tile.X, (int)tile.Y), viewport, player)))
			{
				updateCursorTileHint();
				return true;
			}
			if (currentLocation.terrainFeatures.TryGetValue(tile, out var terrainFeature))
			{
				terrainFeature.performUseAction(tile);
				return true;
			}
			return false;
		}
		if (currentLocation.leftClick((int)position.X, (int)position.Y, player))
		{
			return true;
		}
		isCheckingNonMousePlacement = !IsPerformingMousePlacement();
		if (player.ActiveObject != null)
		{
			if (options.allowStowing && CanPlayerStowItem(GetPlacementGrabTile()))
			{
				if (didPlayerJustLeftClick() || stow_was_initialized)
				{
					game1._didInitiateItemStow = true;
					playSound("stoneStep");
					player.netItemStowed.Set(newValue: true);
					return true;
				}
				return true;
			}
			if (Utility.withinRadiusOfPlayer((int)position.X, (int)position.Y, 1, player) && hooks.OnGameLocation_CheckAction(currentLocation, new Location((int)position.X / 64, (int)position.Y / 64), viewport, player, () => currentLocation.checkAction(new Location((int)position.X / 64, (int)position.Y / 64), viewport, player)))
			{
				return true;
			}
			Vector2 grabTile = GetPlacementGrabTile();
			Vector2 valid_position = Utility.GetNearbyValidPlacementPosition(player, currentLocation, player.ActiveObject, (int)grabTile.X * 64, (int)grabTile.Y * 64);
			if (Utility.tryToPlaceItem(currentLocation, player.ActiveObject, (int)valid_position.X, (int)valid_position.Y))
			{
				isCheckingNonMousePlacement = false;
				return true;
			}
			isCheckingNonMousePlacement = false;
		}
		if (currentLocation.LowPriorityLeftClick((int)position.X, (int)position.Y, player))
		{
			return true;
		}
		if (options.allowStowing && player.netItemStowed.Value && !did_attempt_object_removal && (stow_was_initialized || didPlayerJustLeftClick(ignoreNonMouseHeldInput: true)))
		{
			game1._didInitiateItemStow = true;
			playSound("toolSwap");
			player.netItemStowed.Set(newValue: false);
			return true;
		}
		if (player.UsingTool)
		{
			player.lastClick = new Vector2((int)position.X, (int)position.Y);
			player.CurrentTool.DoFunction(player.currentLocation, (int)player.lastClick.X, (int)player.lastClick.Y, 1, player);
			return true;
		}
		if (player.ActiveObject == null && !player.isEating && player.CurrentTool != null)
		{
			if (player.Stamina <= 20f && player.CurrentTool != null && !(player.CurrentTool is MeleeWeapon) && !eventUp)
			{
				staminaShakeTimer = 1000;
				for (int i = 0; i < 4; i++)
				{
					uiOverlayTempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(366, 412, 5, 6), new Vector2(random.Next(32) + uiViewport.Width - 56, uiViewport.Height - 224 - 16 - (int)((double)(player.MaxStamina - 270) * 0.715)), flipped: false, 0.012f, Color.SkyBlue)
					{
						motion = new Vector2(-2f, -10f),
						acceleration = new Vector2(0f, 0.5f),
						local = true,
						scale = 4 + random.Next(-1, 0),
						delayBeforeAnimationStart = i * 30
					});
				}
			}
			if (!(player.CurrentTool is MeleeWeapon) || didPlayerJustLeftClick(ignoreNonMouseHeldInput: true))
			{
				int old_direction = player.FacingDirection;
				Vector2 tool_location = player.GetToolLocation(position);
				player.FacingDirection = player.getGeneralDirectionTowards(new Vector2((int)tool_location.X, (int)tool_location.Y));
				player.lastClick = new Vector2((int)position.X, (int)position.Y);
				player.BeginUsingTool();
				if (!player.usingTool)
				{
					player.FacingDirection = old_direction;
				}
				else if (player.FarmerSprite.IsPlayingBasicAnimation(old_direction, carrying: true) || player.FarmerSprite.IsPlayingBasicAnimation(old_direction, carrying: false))
				{
					player.FarmerSprite.StopAnimation();
				}
			}
		}
		return false;
	}

	public static bool CanPlayerStowItem(Vector2 position)
	{
		if (player.ActiveObject == null)
		{
			return false;
		}
		if ((bool)player.ActiveObject.bigCraftable)
		{
			return false;
		}
		Object activeObject = player.ActiveObject;
		if (!(activeObject is Furniture))
		{
			if (activeObject != null && (player.ActiveObject.Category == -74 || player.ActiveObject.Category == -19))
			{
				Vector2 valid_position = Utility.GetNearbyValidPlacementPosition(player, currentLocation, player.ActiveObject, (int)position.X * 64, (int)position.Y * 64);
				if (Utility.playerCanPlaceItemHere(player.currentLocation, player.ActiveObject, (int)valid_position.X, (int)valid_position.Y, player) && (!player.ActiveObject.isSapling() || IsPerformingMousePlacement()))
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	public static int getMouseXRaw()
	{
		return input.GetMouseState().X;
	}

	public static int getMouseYRaw()
	{
		return input.GetMouseState().Y;
	}

	public static bool IsOnMainThread()
	{
		if (Thread.CurrentThread != null)
		{
			return !Thread.CurrentThread.IsBackground;
		}
		return false;
	}

	public static void PushUIMode()
	{
		if (!IsOnMainThread())
		{
			return;
		}
		uiModeCount++;
		if (uiModeCount <= 0 || uiMode)
		{
			return;
		}
		uiMode = true;
		if (game1.isDrawing && IsOnMainThread())
		{
			if (game1.uiScreen != null && !game1.uiScreen.IsDisposed)
			{
				RenderTargetBinding[] render_targets = graphics.GraphicsDevice.GetRenderTargets();
				if (render_targets.Length != 0)
				{
					nonUIRenderTarget = render_targets[0].RenderTarget as RenderTarget2D;
				}
				else
				{
					nonUIRenderTarget = null;
				}
				SetRenderTarget(game1.uiScreen);
			}
			if (isRenderingScreenBuffer)
			{
				SetRenderTarget(null);
			}
		}
		xTile.Dimensions.Rectangle ui_viewport_rect = new xTile.Dimensions.Rectangle(0, 0, (int)Math.Ceiling((float)viewport.Width * options.zoomLevel / options.uiScale), (int)Math.Ceiling((float)viewport.Height * options.zoomLevel / options.uiScale));
		ui_viewport_rect.X = viewport.X;
		ui_viewport_rect.Y = viewport.Y;
		uiViewport = ui_viewport_rect;
	}

	public static void PopUIMode()
	{
		if (!IsOnMainThread())
		{
			return;
		}
		uiModeCount--;
		if (uiModeCount > 0 || !uiMode)
		{
			return;
		}
		if (game1.isDrawing)
		{
			if (graphics.GraphicsDevice.GetRenderTargets().Length != 0 && graphics.GraphicsDevice.GetRenderTargets()[0].RenderTarget == game1.uiScreen)
			{
				if (nonUIRenderTarget != null && !nonUIRenderTarget.IsDisposed)
				{
					SetRenderTarget(nonUIRenderTarget);
				}
				else
				{
					SetRenderTarget(null);
				}
			}
			if (isRenderingScreenBuffer)
			{
				SetRenderTarget(null);
			}
		}
		nonUIRenderTarget = null;
		uiMode = false;
	}

	public static void SetRenderTarget(RenderTarget2D target)
	{
		if (!isRenderingScreenBuffer && IsOnMainThread())
		{
			graphics.GraphicsDevice.SetRenderTarget(target);
		}
	}

	public static void InUIMode(Action action)
	{
		PushUIMode();
		try
		{
			action();
		}
		finally
		{
			PopUIMode();
		}
	}

	public static void StartWorldDrawInUI(SpriteBatch b)
	{
		_oldUIModeCount = 0;
		if (uiMode)
		{
			_oldUIModeCount = uiModeCount;
			b?.End();
			while (uiModeCount > 0)
			{
				PopUIMode();
			}
			b?.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
		}
	}

	public static void EndWorldDrawInUI(SpriteBatch b)
	{
		if (_oldUIModeCount > 0)
		{
			b?.End();
			for (int i = 0; i < _oldUIModeCount; i++)
			{
				PushUIMode();
			}
			b?.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
		}
		_oldUIModeCount = 0;
	}

	public static int getMouseX()
	{
		return getMouseX(uiMode);
	}

	public static int getMouseX(bool ui_scale)
	{
		if (ui_scale)
		{
			return (int)((float)input.GetMouseState().X / options.uiScale);
		}
		return (int)((float)input.GetMouseState().X * (1f / options.zoomLevel));
	}

	public static int getOldMouseX()
	{
		return getOldMouseX(uiMode);
	}

	public static int getOldMouseX(bool ui_scale)
	{
		if (ui_scale)
		{
			return (int)((float)oldMouseState.X / options.uiScale);
		}
		return (int)((float)oldMouseState.X * (1f / options.zoomLevel));
	}

	public static int getMouseY()
	{
		return getMouseY(uiMode);
	}

	public static int getMouseY(bool ui_scale)
	{
		if (ui_scale)
		{
			return (int)((float)input.GetMouseState().Y / options.uiScale);
		}
		return (int)((float)input.GetMouseState().Y * (1f / options.zoomLevel));
	}

	public static int getOldMouseY()
	{
		return getOldMouseY(uiMode);
	}

	public static int getOldMouseY(bool ui_scale)
	{
		if (ui_scale)
		{
			return (int)((float)oldMouseState.Y / options.uiScale);
		}
		return (int)((float)oldMouseState.Y * (1f / options.zoomLevel));
	}

	public static bool PlayEvent(string eventId, GameLocation location, out bool validEvent, bool checkPreconditions = true, bool checkSeen = true)
	{
		string eventAssetName;
		Dictionary<string, string> locationEvents;
		try
		{
			if (!location.TryGetLocationEvents(out eventAssetName, out locationEvents))
			{
				validEvent = false;
				return false;
			}
		}
		catch
		{
			validEvent = false;
			return false;
		}
		if (locationEvents == null)
		{
			validEvent = false;
			return false;
		}
		foreach (string key in locationEvents.Keys)
		{
			if (!(key.Split('/')[0] == eventId))
			{
				continue;
			}
			validEvent = true;
			if (checkSeen && (player.eventsSeen.Contains(eventId) || eventsSeenSinceLastLocationChange.Contains(eventId)))
			{
				return false;
			}
			string id = eventId;
			if (checkPreconditions)
			{
				id = location.checkEventPrecondition(key, check_seen: false);
			}
			if (!string.IsNullOrEmpty(id) && id != "-1")
			{
				if (location.Name != currentLocation.Name)
				{
					LocationRequest obj2 = getLocationRequest(location.Name);
					obj2.OnLoad += delegate
					{
						currentLocation.currentEvent = new Event(locationEvents[key], eventAssetName, id);
					};
					int x = 8;
					int y = 8;
					Utility.getDefaultWarpLocation(obj2.Name, ref x, ref y);
					warpFarmer(obj2, x, y, player.FacingDirection);
				}
				else
				{
					globalFadeToBlack(delegate
					{
						forceSnapOnNextViewportUpdate = true;
						currentLocation.startEvent(new Event(locationEvents[key], eventAssetName, id));
						globalFadeToClear();
					});
				}
				return true;
			}
			return false;
		}
		validEvent = false;
		return false;
	}

	public static bool PlayEvent(string eventId, bool checkPreconditions = true, bool checkSeen = true)
	{
		if (checkSeen && (player.eventsSeen.Contains(eventId) || eventsSeenSinceLastLocationChange.Contains(eventId)))
		{
			return false;
		}
		if (PlayEvent(eventId, currentLocation, out var validEvent, checkPreconditions, checkSeen))
		{
			return true;
		}
		if (validEvent)
		{
			return false;
		}
		foreach (GameLocation location in locations)
		{
			if (location != currentLocation)
			{
				if (PlayEvent(eventId, location, out validEvent, checkPreconditions, checkSeen))
				{
					return true;
				}
				if (validEvent)
				{
					return false;
				}
			}
		}
		return false;
	}

	public static int numberOfPlayers()
	{
		return _onlineFarmers.Count;
	}

	public static bool isFestival()
	{
		if (currentLocation != null && currentLocation.currentEvent != null)
		{
			return currentLocation.currentEvent.isFestival;
		}
		return false;
	}

	/// <summary>Parse a raw debug command and run it if it's valid.</summary>
	/// <param name="debugInput">The full debug command, including the command name and arguments.</param>
	/// <param name="log">The log to which to write command output, or <c>null</c> to use <see cref="F:StardewValley.Game1.log" />.</param>
	/// <returns>Returns whether the command was found and executed, regardless of whether the command logic succeeded.</returns>
	public bool parseDebugInput(string debugInput, IGameLogger log = null)
	{
		debugInput = debugInput.Trim();
		string[] command = ArgUtility.SplitBySpaceQuoteAware(debugInput);
		try
		{
			return DebugCommands.TryHandle(command, log);
		}
		catch (Exception e)
		{
			Game1.log.Error("Debug command error.", e);
			debugOutput = e.Message;
			return false;
		}
	}

	public void RecountWalnuts()
	{
		if (!IsMasterGame || netWorldState.Value.ActivatedGoldenParrot || !(getLocationFromName("IslandHut") is IslandHut hut))
		{
			return;
		}
		int missing_nuts = hut.ShowNutHint();
		int current_nut_count = 130 - missing_nuts;
		netWorldState.Value.GoldenWalnutsFound = current_nut_count;
		foreach (GameLocation location in locations)
		{
			if (!(location is IslandLocation island_location))
			{
				continue;
			}
			foreach (ParrotUpgradePerch perch in island_location.parrotUpgradePerches)
			{
				if (perch.currentState.Value == ParrotUpgradePerch.UpgradeState.Complete)
				{
					current_nut_count -= (int)perch.requiredNuts;
				}
			}
		}
		if (MasterPlayer.hasOrWillReceiveMail("Island_VolcanoShortcutOut"))
		{
			current_nut_count -= 5;
		}
		if (MasterPlayer.hasOrWillReceiveMail("Island_VolcanoBridge"))
		{
			current_nut_count -= 5;
		}
		netWorldState.Value.GoldenWalnuts = current_nut_count;
	}

	public void ResetIslandLocations()
	{
		netWorldState.Value.GoldenWalnutsFound = 0;
		player.team.collectedNutTracker.Clear();
		NetStringHashSet[] array = new NetStringHashSet[3]
		{
			player.mailReceived,
			player.mailForTomorrow,
			player.team.broadcastedMail
		};
		foreach (NetStringHashSet obj in array)
		{
			obj.Remove("birdieQuestBegun");
			obj.Remove("birdieQuestFinished");
			obj.Remove("tigerSlimeNut");
			obj.Remove("Island_W_BuriedTreasureNut");
			obj.Remove("Island_W_BuriedTreasure");
			obj.Remove("islandNorthCaveOpened");
			obj.Remove("Saw_Flame_Sprite_North_North");
			obj.Remove("Saw_Flame_Sprite_North_South");
			obj.Remove("Island_N_BuriedTreasureNut");
			obj.Remove("Island_W_BuriedTreasure");
			obj.Remove("Saw_Flame_Sprite_South");
			obj.Remove("Visited_Island");
			obj.Remove("Island_FirstParrot");
			obj.Remove("gotBirdieReward");
			obj.RemoveWhere((string key) => key.StartsWith("Island_Upgrade"));
		}
		player.secretNotesSeen.RemoveWhere((int id) => id >= GameLocation.JOURNAL_INDEX);
		player.team.limitedNutDrops.Clear();
		netWorldState.Value.GoldenCoconutCracked = false;
		netWorldState.Value.GoldenWalnuts = 0;
		netWorldState.Value.ParrotPlatformsUnlocked = false;
		netWorldState.Value.FoundBuriedNuts.Clear();
		for (int i = 0; i < locations.Count; i++)
		{
			GameLocation location = locations[i];
			if (location.InIslandContext())
			{
				_locationLookup.Clear();
				string map_path = location.mapPath.Value;
				string location_name = location.name.Value;
				object[] args = new object[2] { map_path, location_name };
				try
				{
					locations[i] = Activator.CreateInstance(location.GetType(), args) as GameLocation;
				}
				catch
				{
					locations[i] = Activator.CreateInstance(location.GetType()) as GameLocation;
				}
				_locationLookup.Clear();
			}
		}
		AddCharacterIfNecessary("Birdie");
	}

	public void ShowTelephoneMenu()
	{
		playSound("openBox");
		if (IsGreenRainingHere())
		{
			drawObjectDialogue("...................");
			return;
		}
		List<KeyValuePair<string, string>> responses = new List<KeyValuePair<string, string>>();
		foreach (IPhoneHandler handler in Phone.PhoneHandlers)
		{
			responses.AddRange(handler.GetOutgoingNumbers());
		}
		responses.Add(new KeyValuePair<string, string>("HangUp", content.LoadString("Strings\\Locations:MineCart_Destination_Cancel")));
		currentLocation.ShowPagedResponses(content.LoadString("Strings\\Characters:Phone_SelectNumber"), responses, delegate(string callId)
		{
			if (callId == "HangUp")
			{
				Phone.HangUp();
			}
			else
			{
				foreach (IPhoneHandler phoneHandler in Phone.PhoneHandlers)
				{
					if (phoneHandler.TryHandleOutgoingCall(callId))
					{
						return;
					}
				}
				Phone.HangUp();
			}
		}, auto_select_single_choice: false, addCancel: false, 6);
	}

	public void requestDebugInput()
	{
		chatBox.activate();
		chatBox.setText("/");
	}

	private void panModeSuccess(KeyboardState currentKBState)
	{
		panFacingDirectionWait = false;
		playSound("smallSelect");
		if (currentKBState.IsKeyDown(Keys.LeftShift))
		{
			panModeString += " (animation_name_here)";
		}
		debugOutput = panModeString;
	}

	private void updatePanModeControls(MouseState currentMouseState, KeyboardState currentKBState)
	{
		if (currentKBState.IsKeyDown(Keys.F8) && !oldKBState.IsKeyDown(Keys.F8))
		{
			requestDebugInput();
			return;
		}
		if (!panFacingDirectionWait)
		{
			if (currentKBState.IsKeyDown(Keys.W))
			{
				viewport.Y -= 16;
			}
			if (currentKBState.IsKeyDown(Keys.A))
			{
				viewport.X -= 16;
			}
			if (currentKBState.IsKeyDown(Keys.S))
			{
				viewport.Y += 16;
			}
			if (currentKBState.IsKeyDown(Keys.D))
			{
				viewport.X += 16;
			}
		}
		else
		{
			if (currentKBState.IsKeyDown(Keys.W))
			{
				panModeString += "0";
				panModeSuccess(currentKBState);
			}
			if (currentKBState.IsKeyDown(Keys.A))
			{
				panModeString += "3";
				panModeSuccess(currentKBState);
			}
			if (currentKBState.IsKeyDown(Keys.S))
			{
				panModeString += "2";
				panModeSuccess(currentKBState);
			}
			if (currentKBState.IsKeyDown(Keys.D))
			{
				panModeString += "1";
				panModeSuccess(currentKBState);
			}
		}
		if (getMouseX(ui_scale: false) < 192)
		{
			viewport.X -= 8;
			viewport.X -= (192 - getMouseX()) / 8;
		}
		if (getMouseX(ui_scale: false) > viewport.Width - 192)
		{
			viewport.X += 8;
			viewport.X += (getMouseX() - viewport.Width + 192) / 8;
		}
		if (getMouseY(ui_scale: false) < 192)
		{
			viewport.Y -= 8;
			viewport.Y -= (192 - getMouseY()) / 8;
		}
		if (getMouseY(ui_scale: false) > viewport.Height - 192)
		{
			viewport.Y += 8;
			viewport.Y += (getMouseY() - viewport.Height + 192) / 8;
		}
		if (currentMouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released)
		{
			string text = panModeString;
			if (text != null && text.Length > 0)
			{
				int x = (getMouseX() + viewport.X) / 64;
				int y = (getMouseY() + viewport.Y) / 64;
				panModeString = panModeString + currentLocation.Name + " " + x + " " + y + " ";
				panFacingDirectionWait = true;
				currentLocation.playTerrainSound(new Vector2(x, y));
				debugOutput = panModeString;
			}
		}
		if (currentMouseState.RightButton == ButtonState.Pressed && oldMouseState.RightButton == ButtonState.Released)
		{
			int x = getMouseX() + viewport.X;
			int y = getMouseY() + viewport.Y;
			Warp w = currentLocation.isCollidingWithWarpOrDoor(new Microsoft.Xna.Framework.Rectangle(x, y, 1, 1));
			if (w != null)
			{
				currentLocation = RequireLocation(w.TargetName);
				currentLocation.map.LoadTileSheets(mapDisplayDevice);
				viewport.X = w.TargetX * 64 - viewport.Width / 2;
				viewport.Y = w.TargetY * 64 - viewport.Height / 2;
				playSound("dwop");
			}
		}
		if (currentKBState.IsKeyDown(Keys.Escape) && !oldKBState.IsKeyDown(Keys.Escape))
		{
			Warp w = currentLocation.warps[0];
			currentLocation = RequireLocation(w.TargetName);
			currentLocation.map.LoadTileSheets(mapDisplayDevice);
			viewport.X = w.TargetX * 64 - viewport.Width / 2;
			viewport.Y = w.TargetY * 64 - viewport.Height / 2;
			playSound("dwop");
		}
		if (viewport.X < -64)
		{
			viewport.X = -64;
		}
		if (viewport.X + viewport.Width > currentLocation.Map.Layers[0].LayerWidth * 64 + 128)
		{
			viewport.X = currentLocation.Map.Layers[0].LayerWidth * 64 + 128 - viewport.Width;
		}
		if (viewport.Y < -64)
		{
			viewport.Y = -64;
		}
		if (viewport.Y + viewport.Height > currentLocation.Map.Layers[0].LayerHeight * 64 + 128)
		{
			viewport.Y = currentLocation.Map.Layers[0].LayerHeight * 64 + 128 - viewport.Height;
		}
		oldMouseState = input.GetMouseState();
		oldKBState = currentKBState;
	}

	public static bool isLocationAccessible(string locationName)
	{
		switch (locationName)
		{
		case "Desert":
			if (MasterPlayer.mailReceived.Contains("ccVault"))
			{
				return true;
			}
			break;
		case "CommunityCenter":
			if (player.eventsSeen.Contains("191393"))
			{
				return true;
			}
			break;
		case "JojaMart":
			if (!Utility.HasAnyPlayerSeenEvent("191393"))
			{
				return true;
			}
			break;
		case "Railroad":
			if (stats.DaysPlayed > 31)
			{
				return true;
			}
			break;
		default:
			return true;
		}
		return false;
	}

	public static bool isDPadPressed()
	{
		return isDPadPressed(input.GetGamePadState());
	}

	public static bool isDPadPressed(GamePadState pad_state)
	{
		if (pad_state.DPad.Up == ButtonState.Pressed || pad_state.DPad.Down == ButtonState.Pressed || pad_state.DPad.Left == ButtonState.Pressed || pad_state.DPad.Right == ButtonState.Pressed)
		{
			return true;
		}
		return false;
	}

	public static bool isGamePadThumbstickInMotion(double threshold = 0.2)
	{
		bool inMotion = false;
		GamePadState p = input.GetGamePadState();
		if ((double)p.ThumbSticks.Left.X < 0.0 - threshold || p.IsButtonDown(Buttons.LeftThumbstickLeft))
		{
			inMotion = true;
		}
		if ((double)p.ThumbSticks.Left.X > threshold || p.IsButtonDown(Buttons.LeftThumbstickRight))
		{
			inMotion = true;
		}
		if ((double)p.ThumbSticks.Left.Y < 0.0 - threshold || p.IsButtonDown(Buttons.LeftThumbstickUp))
		{
			inMotion = true;
		}
		if ((double)p.ThumbSticks.Left.Y > threshold || p.IsButtonDown(Buttons.LeftThumbstickDown))
		{
			inMotion = true;
		}
		if ((double)p.ThumbSticks.Right.X < 0.0 - threshold)
		{
			inMotion = true;
		}
		if ((double)p.ThumbSticks.Right.X > threshold)
		{
			inMotion = true;
		}
		if ((double)p.ThumbSticks.Right.Y < 0.0 - threshold)
		{
			inMotion = true;
		}
		if ((double)p.ThumbSticks.Right.Y > threshold)
		{
			inMotion = true;
		}
		if (inMotion)
		{
			thumbstickMotionMargin = 50;
		}
		return thumbstickMotionMargin > 0;
	}

	public static bool isAnyGamePadButtonBeingPressed()
	{
		return Utility.getPressedButtons(input.GetGamePadState(), oldPadState).Count > 0;
	}

	public static bool isAnyGamePadButtonBeingHeld()
	{
		return Utility.getHeldButtons(input.GetGamePadState()).Count > 0;
	}

	private static void UpdateChatBox()
	{
		if (chatBox == null)
		{
			return;
		}
		KeyboardState keyState = input.GetKeyboardState();
		GamePadState padState = input.GetGamePadState();
		if (IsChatting)
		{
			if (textEntry != null)
			{
				return;
			}
			if (padState.IsButtonDown(Buttons.A))
			{
				MouseState mouse = input.GetMouseState();
				if (chatBox != null && chatBox.isActive() && !chatBox.isHoveringOverClickable(mouse.X, mouse.Y))
				{
					oldPadState = padState;
					oldKBState = keyState;
					showTextEntry(chatBox.chatBox);
				}
			}
			if (keyState.IsKeyDown(Keys.Escape) || padState.IsButtonDown(Buttons.B) || padState.IsButtonDown(Buttons.Back))
			{
				chatBox.clickAway();
				oldKBState = keyState;
			}
		}
		else if (keyboardDispatcher.Subscriber == null && ((isOneOfTheseKeysDown(keyState, options.chatButton) && game1.HasKeyboardFocus()) || (!padState.IsButtonDown(Buttons.RightStick) && rightStickHoldTime > 0 && rightStickHoldTime < emoteMenuShowTime)))
		{
			chatBox.activate();
			if (keyState.IsKeyDown(Keys.OemQuestion))
			{
				chatBox.setText("/");
			}
		}
	}

	public static KeyboardState GetKeyboardState()
	{
		KeyboardState keyState = input.GetKeyboardState();
		if (chatBox != null)
		{
			if (IsChatting)
			{
				return default(KeyboardState);
			}
			if (keyboardDispatcher.Subscriber == null && isOneOfTheseKeysDown(keyState, options.chatButton) && game1.HasKeyboardFocus())
			{
				return default(KeyboardState);
			}
		}
		return keyState;
	}

	private void UpdateControlInput(GameTime time)
	{
		KeyboardState currentKBState = GetKeyboardState();
		MouseState currentMouseState = input.GetMouseState();
		GamePadState currentPadState = input.GetGamePadState();
		if (ticks < _activatedTick + 2 && oldKBState.IsKeyDown(Keys.Tab) != currentKBState.IsKeyDown(Keys.Tab))
		{
			List<Keys> keys = oldKBState.GetPressedKeys().ToList();
			if (currentKBState.IsKeyDown(Keys.Tab))
			{
				keys.Add(Keys.Tab);
			}
			else
			{
				keys.Remove(Keys.Tab);
			}
			oldKBState = new KeyboardState(keys.ToArray());
		}
		hooks.OnGame1_UpdateControlInput(ref currentKBState, ref currentMouseState, ref currentPadState, delegate
		{
			if (options.gamepadControls)
			{
				bool flag = false;
				if (Math.Abs(currentPadState.ThumbSticks.Right.X) > 0f || Math.Abs(currentPadState.ThumbSticks.Right.Y) > 0f)
				{
					setMousePositionRaw((int)((float)currentMouseState.X + currentPadState.ThumbSticks.Right.X * thumbstickToMouseModifier), (int)((float)currentMouseState.Y - currentPadState.ThumbSticks.Right.Y * thumbstickToMouseModifier));
					flag = true;
				}
				if (IsChatting)
				{
					flag = true;
				}
				if (((getMouseX() != getOldMouseX() || getMouseY() != getOldMouseY()) && getMouseX() != 0 && getMouseY() != 0) || flag)
				{
					if (flag)
					{
						if (timerUntilMouseFade <= 0)
						{
							lastMousePositionBeforeFade = new Point(localMultiplayerWindow.Width / 2, localMultiplayerWindow.Height / 2);
						}
					}
					else
					{
						lastCursorMotionWasMouse = true;
					}
					if (timerUntilMouseFade <= 0 && !lastCursorMotionWasMouse)
					{
						setMousePositionRaw(lastMousePositionBeforeFade.X, lastMousePositionBeforeFade.Y);
					}
					timerUntilMouseFade = 4000;
				}
			}
			else if (getMouseX() != getOldMouseX() || getMouseY() != getOldMouseY())
			{
				lastCursorMotionWasMouse = true;
			}
			bool actionButtonPressed = false;
			bool switchToolButtonPressed = false;
			bool useToolButtonPressed = false;
			bool useToolButtonReleased = false;
			bool addItemToInventoryButtonPressed = false;
			bool cancelButtonPressed = false;
			bool moveUpPressed = false;
			bool moveRightPressed = false;
			bool moveLeftPressed = false;
			bool moveDownPressed = false;
			bool moveUpReleased = false;
			bool moveRightReleased = false;
			bool moveDownReleased = false;
			bool moveLeftReleased = false;
			bool moveUpHeld = false;
			bool moveRightHeld = false;
			bool moveDownHeld = false;
			bool moveLeftHeld = false;
			bool flag2 = false;
			if ((isOneOfTheseKeysDown(currentKBState, options.actionButton) && areAllOfTheseKeysUp(oldKBState, options.actionButton)) || (currentMouseState.RightButton == ButtonState.Pressed && oldMouseState.RightButton == ButtonState.Released))
			{
				actionButtonPressed = true;
				rightClickPolling = 250;
			}
			if ((isOneOfTheseKeysDown(currentKBState, options.useToolButton) && areAllOfTheseKeysUp(oldKBState, options.useToolButton)) || (currentMouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released))
			{
				useToolButtonPressed = true;
			}
			if ((areAllOfTheseKeysUp(currentKBState, options.useToolButton) && isOneOfTheseKeysDown(oldKBState, options.useToolButton)) || (currentMouseState.LeftButton == ButtonState.Released && oldMouseState.LeftButton == ButtonState.Pressed))
			{
				useToolButtonReleased = true;
			}
			if (currentMouseState.ScrollWheelValue != oldMouseState.ScrollWheelValue)
			{
				switchToolButtonPressed = true;
			}
			if ((isOneOfTheseKeysDown(currentKBState, options.cancelButton) && areAllOfTheseKeysUp(oldKBState, options.cancelButton)) || (currentMouseState.RightButton == ButtonState.Pressed && oldMouseState.RightButton == ButtonState.Released))
			{
				cancelButtonPressed = true;
			}
			if (isOneOfTheseKeysDown(currentKBState, options.moveUpButton) && areAllOfTheseKeysUp(oldKBState, options.moveUpButton))
			{
				moveUpPressed = true;
			}
			if (isOneOfTheseKeysDown(currentKBState, options.moveRightButton) && areAllOfTheseKeysUp(oldKBState, options.moveRightButton))
			{
				moveRightPressed = true;
			}
			if (isOneOfTheseKeysDown(currentKBState, options.moveDownButton) && areAllOfTheseKeysUp(oldKBState, options.moveDownButton))
			{
				moveDownPressed = true;
			}
			if (isOneOfTheseKeysDown(currentKBState, options.moveLeftButton) && areAllOfTheseKeysUp(oldKBState, options.moveLeftButton))
			{
				moveLeftPressed = true;
			}
			if (areAllOfTheseKeysUp(currentKBState, options.moveUpButton) && isOneOfTheseKeysDown(oldKBState, options.moveUpButton))
			{
				moveUpReleased = true;
			}
			if (areAllOfTheseKeysUp(currentKBState, options.moveRightButton) && isOneOfTheseKeysDown(oldKBState, options.moveRightButton))
			{
				moveRightReleased = true;
			}
			if (areAllOfTheseKeysUp(currentKBState, options.moveDownButton) && isOneOfTheseKeysDown(oldKBState, options.moveDownButton))
			{
				moveDownReleased = true;
			}
			if (areAllOfTheseKeysUp(currentKBState, options.moveLeftButton) && isOneOfTheseKeysDown(oldKBState, options.moveLeftButton))
			{
				moveLeftReleased = true;
			}
			if (isOneOfTheseKeysDown(currentKBState, options.moveUpButton))
			{
				moveUpHeld = true;
			}
			if (isOneOfTheseKeysDown(currentKBState, options.moveRightButton))
			{
				moveRightHeld = true;
			}
			if (isOneOfTheseKeysDown(currentKBState, options.moveDownButton))
			{
				moveDownHeld = true;
			}
			if (isOneOfTheseKeysDown(currentKBState, options.moveLeftButton))
			{
				moveLeftHeld = true;
			}
			if (isOneOfTheseKeysDown(currentKBState, options.useToolButton) || currentMouseState.LeftButton == ButtonState.Pressed)
			{
				flag2 = true;
			}
			if (isOneOfTheseKeysDown(currentKBState, options.actionButton) || currentMouseState.RightButton == ButtonState.Pressed)
			{
				rightClickPolling -= time.ElapsedGameTime.Milliseconds;
				if (rightClickPolling <= 0)
				{
					rightClickPolling = 100;
					actionButtonPressed = true;
				}
			}
			if (options.gamepadControls)
			{
				if (currentKBState.GetPressedKeys().Length != 0 || currentMouseState.LeftButton == ButtonState.Pressed || currentMouseState.RightButton == ButtonState.Pressed)
				{
					timerUntilMouseFade = 4000;
				}
				if (currentPadState.IsButtonDown(Buttons.A) && !oldPadState.IsButtonDown(Buttons.A))
				{
					actionButtonPressed = true;
					lastCursorMotionWasMouse = false;
					rightClickPolling = 250;
				}
				if (currentPadState.IsButtonDown(Buttons.X) && !oldPadState.IsButtonDown(Buttons.X))
				{
					useToolButtonPressed = true;
					lastCursorMotionWasMouse = false;
				}
				if (!currentPadState.IsButtonDown(Buttons.X) && oldPadState.IsButtonDown(Buttons.X))
				{
					useToolButtonReleased = true;
				}
				if (currentPadState.IsButtonDown(Buttons.RightTrigger) && !oldPadState.IsButtonDown(Buttons.RightTrigger))
				{
					switchToolButtonPressed = true;
					triggerPolling = 300;
				}
				else if (currentPadState.IsButtonDown(Buttons.LeftTrigger) && !oldPadState.IsButtonDown(Buttons.LeftTrigger))
				{
					switchToolButtonPressed = true;
					triggerPolling = 300;
				}
				if (currentPadState.IsButtonDown(Buttons.X))
				{
					flag2 = true;
				}
				if (currentPadState.IsButtonDown(Buttons.A))
				{
					rightClickPolling -= time.ElapsedGameTime.Milliseconds;
					if (rightClickPolling <= 0)
					{
						rightClickPolling = 100;
						actionButtonPressed = true;
					}
				}
				if (currentPadState.IsButtonDown(Buttons.RightTrigger) || currentPadState.IsButtonDown(Buttons.LeftTrigger))
				{
					triggerPolling -= time.ElapsedGameTime.Milliseconds;
					if (triggerPolling <= 0)
					{
						triggerPolling = 100;
						switchToolButtonPressed = true;
					}
				}
				if (currentPadState.IsButtonDown(Buttons.RightShoulder) && !oldPadState.IsButtonDown(Buttons.RightShoulder))
				{
					player.shiftToolbar(right: true);
				}
				if (currentPadState.IsButtonDown(Buttons.LeftShoulder) && !oldPadState.IsButtonDown(Buttons.LeftShoulder))
				{
					player.shiftToolbar(right: false);
				}
				if (currentPadState.IsButtonDown(Buttons.DPadUp) && !oldPadState.IsButtonDown(Buttons.DPadUp))
				{
					moveUpPressed = true;
				}
				else if (!currentPadState.IsButtonDown(Buttons.DPadUp) && oldPadState.IsButtonDown(Buttons.DPadUp))
				{
					moveUpReleased = true;
				}
				if (currentPadState.IsButtonDown(Buttons.DPadRight) && !oldPadState.IsButtonDown(Buttons.DPadRight))
				{
					moveRightPressed = true;
				}
				else if (!currentPadState.IsButtonDown(Buttons.DPadRight) && oldPadState.IsButtonDown(Buttons.DPadRight))
				{
					moveRightReleased = true;
				}
				if (currentPadState.IsButtonDown(Buttons.DPadDown) && !oldPadState.IsButtonDown(Buttons.DPadDown))
				{
					moveDownPressed = true;
				}
				else if (!currentPadState.IsButtonDown(Buttons.DPadDown) && oldPadState.IsButtonDown(Buttons.DPadDown))
				{
					moveDownReleased = true;
				}
				if (currentPadState.IsButtonDown(Buttons.DPadLeft) && !oldPadState.IsButtonDown(Buttons.DPadLeft))
				{
					moveLeftPressed = true;
				}
				else if (!currentPadState.IsButtonDown(Buttons.DPadLeft) && oldPadState.IsButtonDown(Buttons.DPadLeft))
				{
					moveLeftReleased = true;
				}
				if (currentPadState.IsButtonDown(Buttons.DPadUp))
				{
					moveUpHeld = true;
				}
				if (currentPadState.IsButtonDown(Buttons.DPadRight))
				{
					moveRightHeld = true;
				}
				if (currentPadState.IsButtonDown(Buttons.DPadDown))
				{
					moveDownHeld = true;
				}
				if (currentPadState.IsButtonDown(Buttons.DPadLeft))
				{
					moveLeftHeld = true;
				}
				if ((double)currentPadState.ThumbSticks.Left.X < -0.2)
				{
					moveLeftPressed = true;
					moveLeftHeld = true;
				}
				else if ((double)currentPadState.ThumbSticks.Left.X > 0.2)
				{
					moveRightPressed = true;
					moveRightHeld = true;
				}
				if ((double)currentPadState.ThumbSticks.Left.Y < -0.2)
				{
					moveDownPressed = true;
					moveDownHeld = true;
				}
				else if ((double)currentPadState.ThumbSticks.Left.Y > 0.2)
				{
					moveUpPressed = true;
					moveUpHeld = true;
				}
				if ((double)oldPadState.ThumbSticks.Left.X < -0.2 && !moveLeftHeld)
				{
					moveLeftReleased = true;
				}
				if ((double)oldPadState.ThumbSticks.Left.X > 0.2 && !moveRightHeld)
				{
					moveRightReleased = true;
				}
				if ((double)oldPadState.ThumbSticks.Left.Y < -0.2 && !moveDownHeld)
				{
					moveDownReleased = true;
				}
				if ((double)oldPadState.ThumbSticks.Left.Y > 0.2 && !moveUpHeld)
				{
					moveUpReleased = true;
				}
				if (controllerSlingshotSafeTime > 0f)
				{
					if (!currentPadState.IsButtonDown(Buttons.DPadUp) && !currentPadState.IsButtonDown(Buttons.DPadDown) && !currentPadState.IsButtonDown(Buttons.DPadLeft) && !currentPadState.IsButtonDown(Buttons.DPadRight) && (double)Math.Abs(currentPadState.ThumbSticks.Left.X) < 0.04 && (double)Math.Abs(currentPadState.ThumbSticks.Left.Y) < 0.04)
					{
						controllerSlingshotSafeTime = 0f;
					}
					if (controllerSlingshotSafeTime <= 0f)
					{
						controllerSlingshotSafeTime = 0f;
					}
					else
					{
						controllerSlingshotSafeTime -= (float)time.ElapsedGameTime.TotalSeconds;
						moveUpPressed = false;
						moveDownPressed = false;
						moveLeftPressed = false;
						moveRightPressed = false;
						moveUpHeld = false;
						moveDownHeld = false;
						moveLeftHeld = false;
						moveRightHeld = false;
					}
				}
			}
			else
			{
				controllerSlingshotSafeTime = 0f;
			}
			ResetFreeCursorDrag();
			if (flag2)
			{
				mouseClickPolling += time.ElapsedGameTime.Milliseconds;
			}
			else
			{
				mouseClickPolling = 0;
			}
			if (isOneOfTheseKeysDown(currentKBState, options.toolbarSwap) && areAllOfTheseKeysUp(oldKBState, options.toolbarSwap))
			{
				player.shiftToolbar(!currentKBState.IsKeyDown(Keys.LeftControl));
			}
			if (mouseClickPolling > 250 && (!(player.CurrentTool is FishingRod) || (int)player.CurrentTool.upgradeLevel <= 0))
			{
				useToolButtonPressed = true;
				mouseClickPolling = 100;
			}
			PushUIMode();
			foreach (IClickableMenu current in onScreenMenus)
			{
				if ((displayHUD || current == chatBox) && wasMouseVisibleThisFrame && current.isWithinBounds(getMouseX(), getMouseY()))
				{
					current.performHoverAction(getMouseX(), getMouseY());
				}
			}
			PopUIMode();
			if (chatBox != null && chatBox.chatBox.Selected && oldMouseState.ScrollWheelValue != currentMouseState.ScrollWheelValue)
			{
				chatBox.receiveScrollWheelAction(currentMouseState.ScrollWheelValue - oldMouseState.ScrollWheelValue);
			}
			if (panMode)
			{
				updatePanModeControls(currentMouseState, currentKBState);
			}
			else
			{
				if (inputSimulator != null)
				{
					if (currentKBState.IsKeyDown(Keys.Escape))
					{
						inputSimulator = null;
					}
					else
					{
						inputSimulator.SimulateInput(ref actionButtonPressed, ref switchToolButtonPressed, ref useToolButtonPressed, ref useToolButtonReleased, ref addItemToInventoryButtonPressed, ref cancelButtonPressed, ref moveUpPressed, ref moveRightPressed, ref moveLeftPressed, ref moveDownPressed, ref moveUpReleased, ref moveRightReleased, ref moveLeftReleased, ref moveDownReleased, ref moveUpHeld, ref moveRightHeld, ref moveLeftHeld, ref moveDownHeld);
					}
				}
				if (useToolButtonReleased && player.CurrentTool != null && CurrentEvent == null && pauseTime <= 0f && player.CurrentTool.onRelease(currentLocation, getMouseX(), getMouseY(), player))
				{
					oldMouseState = input.GetMouseState();
					oldKBState = currentKBState;
					oldPadState = currentPadState;
					player.usingSlingshot = false;
					player.canReleaseTool = true;
					player.UsingTool = false;
					player.CanMove = true;
				}
				else
				{
					if (((useToolButtonPressed && !isAnyGamePadButtonBeingPressed()) || (actionButtonPressed && isAnyGamePadButtonBeingPressed())) && pauseTime <= 0f && wasMouseVisibleThisFrame)
					{
						if (debugMode)
						{
							Console.WriteLine(getMouseX() + viewport.X + ", " + (getMouseY() + viewport.Y));
						}
						PushUIMode();
						foreach (IClickableMenu current2 in onScreenMenus)
						{
							if (displayHUD || current2 == chatBox)
							{
								if ((!IsChatting || current2 == chatBox) && !(current2 is LevelUpMenu { informationUp: false }) && current2.isWithinBounds(getMouseX(), getMouseY()))
								{
									current2.receiveLeftClick(getMouseX(), getMouseY());
									PopUIMode();
									oldMouseState = input.GetMouseState();
									oldKBState = currentKBState;
									oldPadState = currentPadState;
									return;
								}
								if (current2 == chatBox && options.gamepadControls && IsChatting)
								{
									oldMouseState = input.GetMouseState();
									oldKBState = currentKBState;
									oldPadState = currentPadState;
									PopUIMode();
									return;
								}
								current2.clickAway();
							}
						}
						PopUIMode();
					}
					if (IsChatting || player.freezePause > 0)
					{
						if (IsChatting)
						{
							ButtonCollection.ButtonEnumerator enumerator2 = Utility.getPressedButtons(currentPadState, oldPadState).GetEnumerator();
							while (enumerator2.MoveNext())
							{
								Buttons current3 = enumerator2.Current;
								chatBox.receiveGamePadButton(current3);
							}
						}
						oldMouseState = input.GetMouseState();
						oldKBState = currentKBState;
						oldPadState = currentPadState;
					}
					else
					{
						if (paused || HostPaused)
						{
							if (!HostPaused || !IsMasterGame || (!isOneOfTheseKeysDown(currentKBState, options.menuButton) && !currentPadState.IsButtonDown(Buttons.B) && !currentPadState.IsButtonDown(Buttons.Back)))
							{
								oldMouseState = input.GetMouseState();
								return;
							}
							netWorldState.Value.IsPaused = false;
							chatBox?.globalInfoMessage("Resumed");
						}
						if (eventUp)
						{
							if (currentLocation.currentEvent == null && locationRequest == null)
							{
								eventUp = false;
							}
							else if (actionButtonPressed || useToolButtonPressed)
							{
								CurrentEvent?.receiveMouseClick(getMouseX(), getMouseY());
							}
						}
						bool flag3 = eventUp || farmEvent != null;
						if (actionButtonPressed || (dialogueUp && useToolButtonPressed))
						{
							PushUIMode();
							foreach (IClickableMenu current4 in onScreenMenus)
							{
								if (wasMouseVisibleThisFrame && (displayHUD || current4 == chatBox) && current4.isWithinBounds(getMouseX(), getMouseY()) && !(current4 is LevelUpMenu { informationUp: false }))
								{
									current4.receiveRightClick(getMouseX(), getMouseY());
									oldMouseState = input.GetMouseState();
									if (!isAnyGamePadButtonBeingPressed())
									{
										PopUIMode();
										oldKBState = currentKBState;
										oldPadState = currentPadState;
										return;
									}
								}
							}
							PopUIMode();
							if (!pressActionButton(currentKBState, currentMouseState, currentPadState))
							{
								oldKBState = currentKBState;
								oldMouseState = input.GetMouseState();
								oldPadState = currentPadState;
								return;
							}
						}
						if (useToolButtonPressed && (!player.UsingTool || player.CurrentTool is MeleeWeapon) && !player.isEating && !dialogueUp && farmEvent == null && (player.CanMove || player.CurrentTool is MeleeWeapon))
						{
							if (player.CurrentTool != null && (!(player.CurrentTool is MeleeWeapon) || didPlayerJustLeftClick(ignoreNonMouseHeldInput: true)))
							{
								player.FireTool();
							}
							if (!pressUseToolButton() && player.canReleaseTool && player.UsingTool)
							{
								_ = player.CurrentTool;
							}
							if (player.UsingTool)
							{
								oldMouseState = input.GetMouseState();
								oldKBState = currentKBState;
								oldPadState = currentPadState;
								return;
							}
						}
						if (useToolButtonReleased && _didInitiateItemStow)
						{
							_didInitiateItemStow = false;
						}
						if (useToolButtonReleased && player.canReleaseTool && player.UsingTool && player.CurrentTool != null)
						{
							player.EndUsingTool();
						}
						if (switchToolButtonPressed && !player.UsingTool && !dialogueUp && player.CanMove && player.Items.HasAny() && !flag3)
						{
							pressSwitchToolButton();
						}
						if (player.CurrentTool != null && flag2 && player.canReleaseTool && !flag3 && !dialogueUp && player.Stamina >= 1f && !(player.CurrentTool is FishingRod))
						{
							int num = (player.CurrentTool.hasEnchantmentOfType<ReachingToolEnchantment>() ? 1 : 0);
							if ((int)player.toolHold <= 0 && (int)player.CurrentTool.upgradeLevel + num > (int)player.toolPower)
							{
								float num2 = 1f;
								if (player.CurrentTool != null)
								{
									num2 = player.CurrentTool.AnimationSpeedModifier;
								}
								player.toolHold.Value = (int)(600f * num2);
								player.toolHoldStartTime.Value = player.toolHold;
							}
							else if ((int)player.CurrentTool.upgradeLevel + num > (int)player.toolPower)
							{
								player.toolHold.Value -= time.ElapsedGameTime.Milliseconds;
								if ((int)player.toolHold <= 0)
								{
									player.toolPowerIncrease();
								}
							}
						}
						if (upPolling >= 650f)
						{
							moveUpPressed = true;
							upPolling -= 100f;
						}
						else if (downPolling >= 650f)
						{
							moveDownPressed = true;
							downPolling -= 100f;
						}
						else if (rightPolling >= 650f)
						{
							moveRightPressed = true;
							rightPolling -= 100f;
						}
						else if (leftPolling >= 650f)
						{
							moveLeftPressed = true;
							leftPolling -= 100f;
						}
						else if (pauseTime <= 0f && locationRequest == null && (!player.UsingTool || player.canStrafeForToolUse()) && (!flag3 || (CurrentEvent != null && CurrentEvent.playerControlSequence)))
						{
							if (player.movementDirections.Count < 2)
							{
								if (moveUpHeld)
								{
									player.setMoving(1);
								}
								if (moveRightHeld)
								{
									player.setMoving(2);
								}
								if (moveDownHeld)
								{
									player.setMoving(4);
								}
								if (moveLeftHeld)
								{
									player.setMoving(8);
								}
							}
							if (moveUpReleased || (player.movementDirections.Contains(0) && !moveUpHeld))
							{
								player.setMoving(33);
								if (player.movementDirections.Count == 0)
								{
									player.setMoving(64);
								}
							}
							if (moveRightReleased || (player.movementDirections.Contains(1) && !moveRightHeld))
							{
								player.setMoving(34);
								if (player.movementDirections.Count == 0)
								{
									player.setMoving(64);
								}
							}
							if (moveDownReleased || (player.movementDirections.Contains(2) && !moveDownHeld))
							{
								player.setMoving(36);
								if (player.movementDirections.Count == 0)
								{
									player.setMoving(64);
								}
							}
							if (moveLeftReleased || (player.movementDirections.Contains(3) && !moveLeftHeld))
							{
								player.setMoving(40);
								if (player.movementDirections.Count == 0)
								{
									player.setMoving(64);
								}
							}
							if ((!moveUpHeld && !moveRightHeld && !moveDownHeld && !moveLeftHeld && !player.UsingTool) || activeClickableMenu != null)
							{
								player.Halt();
							}
						}
						else if (isQuestion)
						{
							if (moveUpPressed)
							{
								currentQuestionChoice = Math.Max(currentQuestionChoice - 1, 0);
								playSound("toolSwap");
							}
							else if (moveDownPressed)
							{
								currentQuestionChoice = Math.Min(currentQuestionChoice + 1, questionChoices.Count - 1);
								playSound("toolSwap");
							}
						}
						if (moveUpHeld && !player.CanMove)
						{
							upPolling += time.ElapsedGameTime.Milliseconds;
						}
						else if (moveDownHeld && !player.CanMove)
						{
							downPolling += time.ElapsedGameTime.Milliseconds;
						}
						else if (moveRightHeld && !player.CanMove)
						{
							rightPolling += time.ElapsedGameTime.Milliseconds;
						}
						else if (moveLeftHeld && !player.CanMove)
						{
							leftPolling += time.ElapsedGameTime.Milliseconds;
						}
						else if (moveUpReleased)
						{
							upPolling = 0f;
						}
						else if (moveDownReleased)
						{
							downPolling = 0f;
						}
						else if (moveRightReleased)
						{
							rightPolling = 0f;
						}
						else if (moveLeftReleased)
						{
							leftPolling = 0f;
						}
						if (debugMode)
						{
							if (currentKBState.IsKeyDown(Keys.Q))
							{
								oldKBState.IsKeyDown(Keys.Q);
							}
							if (currentKBState.IsKeyDown(Keys.P) && !oldKBState.IsKeyDown(Keys.P))
							{
								NewDay(0f);
							}
							if (currentKBState.IsKeyDown(Keys.M) && !oldKBState.IsKeyDown(Keys.M))
							{
								dayOfMonth = 28;
								NewDay(0f);
							}
							if (currentKBState.IsKeyDown(Keys.T) && !oldKBState.IsKeyDown(Keys.T))
							{
								addHour();
							}
							if (currentKBState.IsKeyDown(Keys.Y) && !oldKBState.IsKeyDown(Keys.Y))
							{
								addMinute();
							}
							if (currentKBState.IsKeyDown(Keys.D1) && !oldKBState.IsKeyDown(Keys.D1))
							{
								warpFarmer("Mountain", 15, 35, flip: false);
							}
							if (currentKBState.IsKeyDown(Keys.D2) && !oldKBState.IsKeyDown(Keys.D2))
							{
								warpFarmer("Town", 35, 35, flip: false);
							}
							if (currentKBState.IsKeyDown(Keys.D3) && !oldKBState.IsKeyDown(Keys.D3))
							{
								warpFarmer("Farm", 64, 15, flip: false);
							}
							if (currentKBState.IsKeyDown(Keys.D4) && !oldKBState.IsKeyDown(Keys.D4))
							{
								warpFarmer("Forest", 34, 13, flip: false);
							}
							if (currentKBState.IsKeyDown(Keys.D5) && !oldKBState.IsKeyDown(Keys.D4))
							{
								warpFarmer("Beach", 34, 10, flip: false);
							}
							if (currentKBState.IsKeyDown(Keys.D6) && !oldKBState.IsKeyDown(Keys.D6))
							{
								warpFarmer("Mine", 18, 12, flip: false);
							}
							if (currentKBState.IsKeyDown(Keys.D7) && !oldKBState.IsKeyDown(Keys.D7))
							{
								warpFarmer("SandyHouse", 16, 3, flip: false);
							}
							if (currentKBState.IsKeyDown(Keys.K) && !oldKBState.IsKeyDown(Keys.K))
							{
								enterMine(mine.mineLevel + 1);
							}
							if (currentKBState.IsKeyDown(Keys.H) && !oldKBState.IsKeyDown(Keys.H))
							{
								player.changeHat(random.Next(FarmerRenderer.hatsTexture.Height / 80 * 12));
							}
							if (currentKBState.IsKeyDown(Keys.I) && !oldKBState.IsKeyDown(Keys.I))
							{
								player.changeHairStyle(random.Next(FarmerRenderer.hairStylesTexture.Height / 96 * 8));
							}
							if (currentKBState.IsKeyDown(Keys.J) && !oldKBState.IsKeyDown(Keys.J))
							{
								player.changeShirt(random.Next(1000, 1040).ToString());
								player.changePantsColor(new Color(random.Next(255), random.Next(255), random.Next(255)));
							}
							if (currentKBState.IsKeyDown(Keys.L) && !oldKBState.IsKeyDown(Keys.L))
							{
								player.changeShirt(random.Next(1000, 1040).ToString());
								player.changePantsColor(new Color(random.Next(255), random.Next(255), random.Next(255)));
								player.changeHairStyle(random.Next(FarmerRenderer.hairStylesTexture.Height / 96 * 8));
								if (random.NextBool())
								{
									player.changeHat(random.Next(-1, FarmerRenderer.hatsTexture.Height / 80 * 12));
								}
								else
								{
									player.changeHat(-1);
								}
								player.changeHairColor(new Color(random.Next(255), random.Next(255), random.Next(255)));
								player.changeSkinColor(random.Next(16));
							}
							if (currentKBState.IsKeyDown(Keys.U) && !oldKBState.IsKeyDown(Keys.U))
							{
								FarmHouse farmHouse = RequireLocation<FarmHouse>("FarmHouse");
								farmHouse.SetWallpaper(random.Next(112).ToString(), null);
								farmHouse.SetFloor(random.Next(40).ToString(), null);
							}
							if (currentKBState.IsKeyDown(Keys.F2))
							{
								oldKBState.IsKeyDown(Keys.F2);
							}
							if (currentKBState.IsKeyDown(Keys.F5) && !oldKBState.IsKeyDown(Keys.F5))
							{
								displayFarmer = !displayFarmer;
							}
							if (currentKBState.IsKeyDown(Keys.F6))
							{
								oldKBState.IsKeyDown(Keys.F6);
							}
							if (currentKBState.IsKeyDown(Keys.F7) && !oldKBState.IsKeyDown(Keys.F7))
							{
								drawGrid = !drawGrid;
							}
							if (currentKBState.IsKeyDown(Keys.B) && !oldKBState.IsKeyDown(Keys.B))
							{
								player.shiftToolbar(right: false);
							}
							if (currentKBState.IsKeyDown(Keys.N) && !oldKBState.IsKeyDown(Keys.N))
							{
								player.shiftToolbar(right: true);
							}
							if (currentKBState.IsKeyDown(Keys.F10) && !oldKBState.IsKeyDown(Keys.F10) && server == null)
							{
								multiplayer.StartServer();
							}
						}
						else if (!player.UsingTool)
						{
							if (isOneOfTheseKeysDown(currentKBState, options.inventorySlot1) && areAllOfTheseKeysUp(oldKBState, options.inventorySlot1))
							{
								player.CurrentToolIndex = 0;
							}
							else if (isOneOfTheseKeysDown(currentKBState, options.inventorySlot2) && areAllOfTheseKeysUp(oldKBState, options.inventorySlot2))
							{
								player.CurrentToolIndex = 1;
							}
							else if (isOneOfTheseKeysDown(currentKBState, options.inventorySlot3) && areAllOfTheseKeysUp(oldKBState, options.inventorySlot3))
							{
								player.CurrentToolIndex = 2;
							}
							else if (isOneOfTheseKeysDown(currentKBState, options.inventorySlot4) && areAllOfTheseKeysUp(oldKBState, options.inventorySlot4))
							{
								player.CurrentToolIndex = 3;
							}
							else if (isOneOfTheseKeysDown(currentKBState, options.inventorySlot5) && areAllOfTheseKeysUp(oldKBState, options.inventorySlot5))
							{
								player.CurrentToolIndex = 4;
							}
							else if (isOneOfTheseKeysDown(currentKBState, options.inventorySlot6) && areAllOfTheseKeysUp(oldKBState, options.inventorySlot6))
							{
								player.CurrentToolIndex = 5;
							}
							else if (isOneOfTheseKeysDown(currentKBState, options.inventorySlot7) && areAllOfTheseKeysUp(oldKBState, options.inventorySlot7))
							{
								player.CurrentToolIndex = 6;
							}
							else if (isOneOfTheseKeysDown(currentKBState, options.inventorySlot8) && areAllOfTheseKeysUp(oldKBState, options.inventorySlot8))
							{
								player.CurrentToolIndex = 7;
							}
							else if (isOneOfTheseKeysDown(currentKBState, options.inventorySlot9) && areAllOfTheseKeysUp(oldKBState, options.inventorySlot9))
							{
								player.CurrentToolIndex = 8;
							}
							else if (isOneOfTheseKeysDown(currentKBState, options.inventorySlot10) && areAllOfTheseKeysUp(oldKBState, options.inventorySlot10))
							{
								player.CurrentToolIndex = 9;
							}
							else if (isOneOfTheseKeysDown(currentKBState, options.inventorySlot11) && areAllOfTheseKeysUp(oldKBState, options.inventorySlot11))
							{
								player.CurrentToolIndex = 10;
							}
							else if (isOneOfTheseKeysDown(currentKBState, options.inventorySlot12) && areAllOfTheseKeysUp(oldKBState, options.inventorySlot12))
							{
								player.CurrentToolIndex = 11;
							}
						}
						if (((options.gamepadControls && rightStickHoldTime >= emoteMenuShowTime && activeClickableMenu == null) || (isOneOfTheseKeysDown(input.GetKeyboardState(), options.emoteButton) && areAllOfTheseKeysUp(oldKBState, options.emoteButton))) && !debugMode && player.CanEmote())
						{
							if (player.CanMove)
							{
								player.Halt();
							}
							emoteMenu = new EmoteMenu();
							emoteMenu.gamepadMode = options.gamepadControls && rightStickHoldTime >= emoteMenuShowTime;
							timerUntilMouseFade = 0;
						}
						if (!Program.releaseBuild)
						{
							if (IsPressEvent(ref currentKBState, Keys.F3) || IsPressEvent(ref currentPadState, Buttons.LeftStick))
							{
								debugMode = !debugMode;
								if (gameMode == 11)
								{
									gameMode = 3;
								}
							}
							if (IsPressEvent(ref currentKBState, Keys.F8))
							{
								requestDebugInput();
							}
						}
						if (currentKBState.IsKeyDown(Keys.F4) && !oldKBState.IsKeyDown(Keys.F4))
						{
							displayHUD = !displayHUD;
							playSound("smallSelect");
							if (!displayHUD)
							{
								showGlobalMessage(content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3666"));
							}
						}
						bool flag4 = isOneOfTheseKeysDown(currentKBState, options.menuButton) && areAllOfTheseKeysUp(oldKBState, options.menuButton);
						bool flag5 = isOneOfTheseKeysDown(currentKBState, options.journalButton) && areAllOfTheseKeysUp(oldKBState, options.journalButton);
						bool flag6 = isOneOfTheseKeysDown(currentKBState, options.mapButton) && areAllOfTheseKeysUp(oldKBState, options.mapButton);
						if (options.gamepadControls && !flag4)
						{
							flag4 = (currentPadState.IsButtonDown(Buttons.Start) && !oldPadState.IsButtonDown(Buttons.Start)) || (currentPadState.IsButtonDown(Buttons.B) && !oldPadState.IsButtonDown(Buttons.B));
						}
						if (options.gamepadControls && !flag5)
						{
							flag5 = currentPadState.IsButtonDown(Buttons.Back) && !oldPadState.IsButtonDown(Buttons.Back);
						}
						if (options.gamepadControls && !flag6)
						{
							flag6 = currentPadState.IsButtonDown(Buttons.Y) && !oldPadState.IsButtonDown(Buttons.Y);
						}
						if (flag4 && CanShowPauseMenu())
						{
							if (activeClickableMenu == null)
							{
								PushUIMode();
								activeClickableMenu = new GameMenu();
								PopUIMode();
							}
							else if (activeClickableMenu.readyToClose())
							{
								exitActiveMenu();
							}
						}
						if (dayOfMonth > 0 && player.CanMove && flag5 && !dialogueUp && !flag3)
						{
							if (activeClickableMenu == null)
							{
								activeClickableMenu = new QuestLog();
							}
						}
						else if (flag3 && CurrentEvent != null && flag5 && !CurrentEvent.skipped && CurrentEvent.skippable)
						{
							CurrentEvent.skipped = true;
							CurrentEvent.skipEvent();
							freezeControls = false;
						}
						if (options.gamepadControls && dayOfMonth > 0 && player.CanMove && isAnyGamePadButtonBeingPressed() && flag6 && !dialogueUp && !flag3)
						{
							if (activeClickableMenu == null)
							{
								PushUIMode();
								activeClickableMenu = new GameMenu(GameMenu.craftingTab);
								PopUIMode();
							}
						}
						else if (dayOfMonth > 0 && player.CanMove && flag6 && !dialogueUp && !flag3 && activeClickableMenu == null)
						{
							PushUIMode();
							activeClickableMenu = new GameMenu(GameMenu.mapTab);
							PopUIMode();
						}
						checkForRunButton(currentKBState);
						oldKBState = currentKBState;
						oldMouseState = input.GetMouseState();
						oldPadState = currentPadState;
					}
				}
			}
		});
	}

	public static bool CanShowPauseMenu()
	{
		if (dayOfMonth > 0 && player.CanMove && !dialogueUp && (!eventUp || (isFestival() && CurrentEvent.festivalTimer <= 0)) && currentMinigame == null)
		{
			return farmEvent == null;
		}
		return false;
	}

	internal static void addHour()
	{
		timeOfDay += 100;
		foreach (GameLocation g in locations)
		{
			for (int i = 0; i < g.characters.Count; i++)
			{
				NPC nPC = g.characters[i];
				nPC.checkSchedule(timeOfDay);
				nPC.checkSchedule(timeOfDay - 50);
				nPC.checkSchedule(timeOfDay - 60);
				nPC.checkSchedule(timeOfDay - 70);
				nPC.checkSchedule(timeOfDay - 80);
				nPC.checkSchedule(timeOfDay - 90);
			}
		}
		switch (timeOfDay)
		{
		case 1900:
			currentLocation.switchOutNightTiles();
			break;
		case 2000:
			if (!currentLocation.IsRainingHere())
			{
				changeMusicTrack("none");
			}
			break;
		}
	}

	internal static void addMinute()
	{
		if (GetKeyboardState().IsKeyDown(Keys.LeftShift))
		{
			timeOfDay -= 10;
		}
		else
		{
			timeOfDay += 10;
		}
		if (timeOfDay % 100 == 60)
		{
			timeOfDay += 40;
		}
		if (timeOfDay % 100 == 90)
		{
			timeOfDay -= 40;
		}
		currentLocation.performTenMinuteUpdate(timeOfDay);
		foreach (GameLocation g in locations)
		{
			for (int i = 0; i < g.characters.Count; i++)
			{
				g.characters[i].checkSchedule(timeOfDay);
			}
		}
		if (isLightning && IsMasterGame)
		{
			Utility.performLightningUpdate(timeOfDay);
		}
		switch (timeOfDay)
		{
		case 1750:
			outdoorLight = Color.White;
			break;
		case 1900:
			currentLocation.switchOutNightTiles();
			break;
		case 2000:
			if (!currentLocation.IsRainingHere())
			{
				changeMusicTrack("none");
			}
			break;
		}
	}

	public static void checkForRunButton(KeyboardState kbState, bool ignoreKeyPressQualifier = false)
	{
		bool wasRunning = player.running;
		bool runPressed = isOneOfTheseKeysDown(kbState, options.runButton) && (!isOneOfTheseKeysDown(oldKBState, options.runButton) || ignoreKeyPressQualifier);
		bool runReleased = !isOneOfTheseKeysDown(kbState, options.runButton) && (isOneOfTheseKeysDown(oldKBState, options.runButton) || ignoreKeyPressQualifier);
		if (options.gamepadControls)
		{
			if (!options.autoRun && Math.Abs(Vector2.Distance(input.GetGamePadState().ThumbSticks.Left, Vector2.Zero)) > 0.9f)
			{
				runPressed = true;
			}
			else if (Math.Abs(Vector2.Distance(oldPadState.ThumbSticks.Left, Vector2.Zero)) > 0.9f && Math.Abs(Vector2.Distance(input.GetGamePadState().ThumbSticks.Left, Vector2.Zero)) <= 0.9f)
			{
				runReleased = true;
			}
		}
		if (runPressed && !player.canOnlyWalk)
		{
			player.setRunning(!options.autoRun);
			player.setMoving((byte)(player.running ? 16u : 48u));
		}
		else if (runReleased && !player.canOnlyWalk)
		{
			player.setRunning(options.autoRun);
			player.setMoving((byte)(player.running ? 16u : 48u));
		}
		if (player.running != wasRunning && !player.UsingTool)
		{
			player.Halt();
		}
	}

	public static Vector2 getMostRecentViewportMotion()
	{
		return new Vector2((float)viewport.X - previousViewportPosition.X, (float)viewport.Y - previousViewportPosition.Y);
	}

	protected virtual void DrawOverlays(GameTime time, RenderTarget2D target_screen)
	{
		if (takingMapScreenshot)
		{
			return;
		}
		PushUIMode();
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
		if (hooks.OnRendering(RenderSteps.Overlays, spriteBatch, time, target_screen))
		{
			specialCurrencyDisplay?.Draw(spriteBatch);
			emoteMenu?.draw(spriteBatch);
			currentLocation?.drawOverlays(spriteBatch);
			if (HostPaused && !takingMapScreenshot)
			{
				string msg = content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10378");
				SpriteText.drawStringWithScrollBackground(spriteBatch, msg, 96, 32);
			}
			if (overlayMenu != null)
			{
				if (hooks.OnRendering(RenderSteps.Overlays_OverlayMenu, spriteBatch, time, target_screen))
				{
					overlayMenu.draw(spriteBatch);
				}
				hooks.OnRendered(RenderSteps.Overlays_OverlayMenu, spriteBatch, time, target_screen);
			}
			if (chatBox != null)
			{
				if (hooks.OnRendering(RenderSteps.Overlays_Chatbox, spriteBatch, time, target_screen))
				{
					chatBox.update(currentGameTime);
					chatBox.draw(spriteBatch);
				}
				hooks.OnRendered(RenderSteps.Overlays_Chatbox, spriteBatch, time, target_screen);
			}
			if (textEntry != null)
			{
				if (hooks.OnRendering(RenderSteps.Overlays_OnscreenKeyboard, spriteBatch, time, target_screen))
				{
					textEntry.draw(spriteBatch);
				}
				hooks.OnRendered(RenderSteps.Overlays_OnscreenKeyboard, spriteBatch, time, target_screen);
			}
			if ((displayHUD || eventUp || currentLocation is Summit) && gameMode == 3 && !freezeControls && !panMode)
			{
				drawMouseCursor();
			}
		}
		hooks.OnRendered(RenderSteps.Overlays, spriteBatch, time, target_screen);
		spriteBatch.End();
		PopUIMode();
	}

	public static void setBGColor(byte r, byte g, byte b)
	{
		bgColor.R = r;
		bgColor.G = g;
		bgColor.B = b;
	}

	public void Instance_Draw(GameTime gameTime)
	{
		Draw(gameTime);
	}

	/// <summary>
	/// This is called when the game should draw itself.
	/// </summary>
	/// <param name="gameTime">Provides a snapshot of timing values.</param>
	protected override void Draw(GameTime gameTime)
	{
		isDrawing = true;
		RenderTarget2D target_screen = null;
		if (ShouldDrawOnBuffer())
		{
			target_screen = screen;
		}
		if (uiScreen != null)
		{
			SetRenderTarget(uiScreen);
			base.GraphicsDevice.Clear(Color.Transparent);
			SetRenderTarget(target_screen);
		}
		GameTime time = gameTime;
		DebugTools.BeforeGameDraw(this, ref time);
		_draw(time, target_screen);
		isRenderingScreenBuffer = true;
		renderScreenBuffer(target_screen);
		isRenderingScreenBuffer = false;
		if (uiModeCount != 0)
		{
			log.Warn("WARNING: Mismatched UI Mode Push/Pop counts. Correcting.");
			while (uiModeCount < 0)
			{
				PushUIMode();
			}
			while (uiModeCount > 0)
			{
				PopUIMode();
			}
		}
		base.Draw(gameTime);
		isDrawing = false;
	}

	public virtual bool ShouldDrawOnBuffer()
	{
		if (LocalMultiplayer.IsLocalMultiplayer())
		{
			return true;
		}
		if (options.zoomLevel != 1f)
		{
			return true;
		}
		return false;
	}

	public static bool ShouldShowOnscreenUsernames()
	{
		return false;
	}

	public virtual bool checkCharacterTilesForShadowDrawFlag(Character character)
	{
		if (character is Farmer farmer && farmer.onBridge.Value)
		{
			return true;
		}
		Microsoft.Xna.Framework.Rectangle bounding_box = character.GetBoundingBox();
		bounding_box.Height += 8;
		int right = bounding_box.Right / 64;
		int bottom = bounding_box.Bottom / 64;
		int num = bounding_box.Left / 64;
		int top = bounding_box.Top / 64;
		for (int x = num; x <= right; x++)
		{
			for (int y = top; y <= bottom; y++)
			{
				if (currentLocation.shouldShadowBeDrawnAboveBuildingsLayer(new Vector2(x, y)))
				{
					return true;
				}
			}
		}
		return false;
	}

	protected virtual void _draw(GameTime gameTime, RenderTarget2D target_screen)
	{
		showingHealthBar = false;
		if (_newDayTask != null || isLocalMultiplayerNewDayActive)
		{
			base.GraphicsDevice.Clear(bgColor);
			return;
		}
		if (target_screen != null)
		{
			SetRenderTarget(target_screen);
		}
		if (IsSaving)
		{
			base.GraphicsDevice.Clear(bgColor);
			DrawMenu(gameTime, target_screen);
			PushUIMode();
			if (overlayMenu != null)
			{
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
				overlayMenu.draw(spriteBatch);
				spriteBatch.End();
			}
			PopUIMode();
			return;
		}
		base.GraphicsDevice.Clear(bgColor);
		if (hooks.OnRendering(RenderSteps.FullScene, spriteBatch, gameTime, target_screen))
		{
			if (gameMode == 11)
			{
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
				spriteBatch.DrawString(dialogueFont, content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3685"), new Vector2(16f, 16f), Color.HotPink);
				spriteBatch.DrawString(dialogueFont, content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3686"), new Vector2(16f, 32f), new Color(0, 255, 0));
				spriteBatch.DrawString(dialogueFont, parseText(errorMessage, dialogueFont, graphics.GraphicsDevice.Viewport.Width), new Vector2(16f, 48f), Color.White);
				spriteBatch.End();
				return;
			}
			bool draw_world = true;
			if (activeClickableMenu != null && options.showMenuBackground && activeClickableMenu.showWithoutTransparencyIfOptionIsSet() && !takingMapScreenshot)
			{
				PushUIMode();
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
				if (hooks.OnRendering(RenderSteps.MenuBackground, spriteBatch, gameTime, target_screen))
				{
					activeClickableMenu.drawBackground(spriteBatch);
					draw_world = false;
				}
				hooks.OnRendered(RenderSteps.MenuBackground, spriteBatch, gameTime, target_screen);
				spriteBatch.End();
				PopUIMode();
			}
			if (currentMinigame != null)
			{
				if (hooks.OnRendering(RenderSteps.Minigame, spriteBatch, gameTime, target_screen))
				{
					currentMinigame.draw(spriteBatch);
					draw_world = false;
				}
				hooks.OnRendered(RenderSteps.Minigame, spriteBatch, gameTime, target_screen);
			}
			if (gameMode == 6 || (gameMode == 3 && currentLocation == null))
			{
				if (hooks.OnRendering(RenderSteps.LoadingScreen, spriteBatch, gameTime, target_screen))
				{
					DrawLoadScreen(gameTime, target_screen);
				}
				hooks.OnRendered(RenderSteps.LoadingScreen, spriteBatch, gameTime, target_screen);
				draw_world = false;
			}
			if (showingEndOfNightStuff)
			{
				draw_world = false;
			}
			else if (gameMode == 0)
			{
				draw_world = false;
			}
			if (gameMode == 3 && dayOfMonth == 0 && newDay)
			{
				base.Draw(gameTime);
				return;
			}
			if (draw_world)
			{
				DrawWorld(gameTime, target_screen);
				PushUIMode();
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
				if (hooks.OnRendering(RenderSteps.HUD, spriteBatch, gameTime, target_screen))
				{
					if ((displayHUD || eventUp) && gameMode == 3 && !freezeControls && !panMode && !HostPaused && !takingMapScreenshot)
					{
						drawHUD();
					}
					if (hudMessages.Count > 0 && !takingMapScreenshot)
					{
						int heightUsed = 0;
						for (int i = hudMessages.Count - 1; i >= 0; i--)
						{
							hudMessages[i].draw(spriteBatch, i, ref heightUsed);
						}
					}
				}
				hooks.OnRendered(RenderSteps.HUD, spriteBatch, gameTime, target_screen);
				spriteBatch.End();
				PopUIMode();
			}
			bool draw_dialogue_box_after_fade = false;
			if (!takingMapScreenshot)
			{
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
				PushUIMode();
				if ((messagePause || globalFade) && dialogueUp)
				{
					draw_dialogue_box_after_fade = true;
				}
				else if (dialogueUp && !messagePause && (activeClickableMenu == null || !(activeClickableMenu is DialogueBox)))
				{
					if (hooks.OnRendering(RenderSteps.DialogueBox, spriteBatch, gameTime, target_screen))
					{
						drawDialogueBox();
					}
					hooks.OnRendered(RenderSteps.DialogueBox, spriteBatch, gameTime, target_screen);
				}
				spriteBatch.End();
				PopUIMode();
				DrawGlobalFade(gameTime, target_screen);
				if (draw_dialogue_box_after_fade)
				{
					spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
					PushUIMode();
					if (hooks.OnRendering(RenderSteps.DialogueBox, spriteBatch, gameTime, target_screen))
					{
						drawDialogueBox();
					}
					hooks.OnRendered(RenderSteps.DialogueBox, spriteBatch, gameTime, target_screen);
					spriteBatch.End();
					PopUIMode();
				}
				DrawScreenOverlaySprites(gameTime, target_screen);
				if (debugMode)
				{
					DrawDebugUIs(gameTime, target_screen);
				}
				DrawMenu(gameTime, target_screen);
			}
			farmEvent?.drawAboveEverything(spriteBatch);
			DrawOverlays(gameTime, target_screen);
		}
		hooks.OnRendered(RenderSteps.FullScene, spriteBatch, gameTime, target_screen);
	}

	public virtual void DrawLoadScreen(GameTime time, RenderTarget2D target_screen)
	{
		PushUIMode();
		base.GraphicsDevice.Clear(bgColor);
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
		string addOn = "".PadRight((int)Math.Ceiling(time.TotalGameTime.TotalMilliseconds % 999.0 / 333.0), '.');
		string text = content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3688");
		string msg = text + addOn;
		string largestMessage = text + "... ";
		int msgw = SpriteText.getWidthOfString(largestMessage);
		int msgh = 64;
		int msgx = 64;
		int msgy = graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Bottom - msgh;
		SpriteText.drawString(spriteBatch, msg, msgx, msgy, 999999, msgw, msgh, 1f, 0.88f, junimoText: false, 0, largestMessage);
		spriteBatch.End();
		PopUIMode();
	}

	public virtual void DrawMenu(GameTime time, RenderTarget2D target_screen)
	{
		PushUIMode();
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
		if (hooks.OnRendering(RenderSteps.Menu, spriteBatch, time, target_screen))
		{
			IClickableMenu menu = activeClickableMenu;
			while (menu != null && hooks.TryDrawMenu(menu, delegate
			{
				menu.draw(spriteBatch);
			}))
			{
				menu = menu.GetChildMenu();
			}
		}
		hooks.OnRendered(RenderSteps.Menu, spriteBatch, time, target_screen);
		spriteBatch.End();
		PopUIMode();
	}

	public virtual void DrawScreenOverlaySprites(GameTime time, RenderTarget2D target_screen)
	{
		if (hooks.OnRendering(RenderSteps.OverlayTemporarySprites, spriteBatch, time, target_screen))
		{
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			foreach (TemporaryAnimatedSprite screenOverlayTempSprite in screenOverlayTempSprites)
			{
				screenOverlayTempSprite.draw(spriteBatch, localPosition: true);
			}
			spriteBatch.End();
			PushUIMode();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			foreach (TemporaryAnimatedSprite uiOverlayTempSprite in uiOverlayTempSprites)
			{
				uiOverlayTempSprite.draw(spriteBatch, localPosition: true);
			}
			spriteBatch.End();
			PopUIMode();
		}
		hooks.OnRendered(RenderSteps.OverlayTemporarySprites, spriteBatch, time, target_screen);
	}

	public virtual void DrawWorld(GameTime time, RenderTarget2D target_screen)
	{
		if (hooks.OnRendering(RenderSteps.World, spriteBatch, time, target_screen))
		{
			mapDisplayDevice.BeginScene(spriteBatch);
			if (drawLighting)
			{
				DrawLighting(time, target_screen);
			}
			base.GraphicsDevice.Clear(bgColor);
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			if (hooks.OnRendering(RenderSteps.World_Background, spriteBatch, time, target_screen))
			{
				background?.draw(spriteBatch);
				currentLocation.drawBackground(spriteBatch);
				spriteBatch.End();
				for (int i = 0; i < currentLocation.backgroundLayers.Count; i++)
				{
					spriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, SamplerState.PointClamp);
					currentLocation.backgroundLayers[i].Key.Draw(mapDisplayDevice, viewport, Location.Origin, wrapAround: false, 4, -1f);
					spriteBatch.End();
				}
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
				currentLocation.drawWater(spriteBatch);
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
				currentLocation.drawFloorDecorations(spriteBatch);
				spriteBatch.End();
			}
			hooks.OnRendered(RenderSteps.World_Background, spriteBatch, time, target_screen);
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			_farmerShadows.Clear();
			if (currentLocation.currentEvent != null && !currentLocation.currentEvent.isFestival && currentLocation.currentEvent.farmerActors.Count > 0)
			{
				foreach (Farmer f in currentLocation.currentEvent.farmerActors)
				{
					if ((f.IsLocalPlayer && displayFarmer) || !f.hidden)
					{
						_farmerShadows.Add(f);
					}
				}
			}
			else
			{
				foreach (Farmer f in currentLocation.farmers)
				{
					if ((f.IsLocalPlayer && displayFarmer) || !f.hidden)
					{
						_farmerShadows.Add(f);
					}
				}
			}
			if (!currentLocation.shouldHideCharacters())
			{
				if (CurrentEvent == null)
				{
					foreach (NPC n in currentLocation.characters)
					{
						if (!n.swimming && !n.HideShadow && !n.IsInvisible && !checkCharacterTilesForShadowDrawFlag(n))
						{
							n.DrawShadow(spriteBatch);
						}
					}
				}
				else
				{
					foreach (NPC n in CurrentEvent.actors)
					{
						if ((CurrentEvent == null || !CurrentEvent.ShouldHideCharacter(n)) && !n.swimming && !n.HideShadow && !checkCharacterTilesForShadowDrawFlag(n))
						{
							n.DrawShadow(spriteBatch);
						}
					}
				}
				foreach (Farmer f in _farmerShadows)
				{
					if (!multiplayer.isDisconnecting(f.UniqueMultiplayerID) && !f.swimming && !f.isRidingHorse() && !f.IsSitting() && (currentLocation == null || !checkCharacterTilesForShadowDrawFlag(f)))
					{
						f.DrawShadow(spriteBatch);
					}
				}
			}
			float layer_sub_sort = 0.1f;
			for (int i = 0; i < currentLocation.buildingLayers.Count; i++)
			{
				float layer = 0f;
				if (currentLocation.buildingLayers.Count > 1)
				{
					layer = (float)i / (float)(currentLocation.buildingLayers.Count - 1);
				}
				currentLocation.buildingLayers[i].Key.Draw(mapDisplayDevice, viewport, Location.Origin, wrapAround: false, 4, layer_sub_sort * layer);
			}
			Layer building_layer = currentLocation.Map.RequireLayer("Buildings");
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
			if (hooks.OnRendering(RenderSteps.World_Sorted, spriteBatch, time, target_screen))
			{
				if (!currentLocation.shouldHideCharacters())
				{
					if (CurrentEvent == null)
					{
						foreach (NPC n in currentLocation.characters)
						{
							if (!n.swimming && !n.HideShadow && !n.isInvisible && checkCharacterTilesForShadowDrawFlag(n))
							{
								n.DrawShadow(spriteBatch);
							}
						}
					}
					else
					{
						foreach (NPC n in CurrentEvent.actors)
						{
							if ((CurrentEvent == null || !CurrentEvent.ShouldHideCharacter(n)) && !n.swimming && !n.HideShadow && checkCharacterTilesForShadowDrawFlag(n))
							{
								n.DrawShadow(spriteBatch);
							}
						}
					}
					foreach (Farmer f in _farmerShadows)
					{
						if (!f.swimming && !f.isRidingHorse() && !f.IsSitting() && currentLocation != null && checkCharacterTilesForShadowDrawFlag(f))
						{
							f.DrawShadow(spriteBatch);
						}
					}
				}
				if ((eventUp || killScreen) && !killScreen && currentLocation.currentEvent != null)
				{
					currentLocation.currentEvent.draw(spriteBatch);
				}
				currentLocation.draw(spriteBatch);
				foreach (Vector2 tile_position in crabPotOverlayTiles.Keys)
				{
					Tile tile = building_layer.Tiles[(int)tile_position.X, (int)tile_position.Y];
					if (tile != null)
					{
						Vector2 vector_draw_position = GlobalToLocal(viewport, tile_position * 64f);
						Location draw_location = new Location((int)vector_draw_position.X, (int)vector_draw_position.Y);
						mapDisplayDevice.DrawTile(tile, draw_location, (tile_position.Y * 64f - 1f) / 10000f);
					}
				}
				if (player.ActiveObject == null && player.UsingTool && player.CurrentTool != null)
				{
					drawTool(player);
				}
				if (panMode)
				{
					spriteBatch.Draw(fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle((int)Math.Floor((double)(getOldMouseX() + viewport.X) / 64.0) * 64 - viewport.X, (int)Math.Floor((double)(getOldMouseY() + viewport.Y) / 64.0) * 64 - viewport.Y, 64, 64), Color.Lime * 0.75f);
					foreach (Warp w in currentLocation.warps)
					{
						spriteBatch.Draw(fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(w.X * 64 - viewport.X, w.Y * 64 - viewport.Y, 64, 64), Color.Red * 0.75f);
					}
				}
				for (int i = 0; i < currentLocation.frontLayers.Count; i++)
				{
					float layer = 0f;
					if (currentLocation.frontLayers.Count > 1)
					{
						layer = (float)i / (float)(currentLocation.frontLayers.Count - 1);
					}
					currentLocation.frontLayers[i].Key.Draw(mapDisplayDevice, viewport, Location.Origin, wrapAround: false, 4, 64f + layer_sub_sort * layer);
				}
				currentLocation.drawAboveFrontLayer(spriteBatch);
			}
			hooks.OnRendered(RenderSteps.World_Sorted, spriteBatch, time, target_screen);
			spriteBatch.End();
			if (hooks.OnRendering(RenderSteps.World_AlwaysFront, spriteBatch, time, target_screen))
			{
				for (int i = 0; i < currentLocation.alwaysFrontLayers.Count; i++)
				{
					spriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, SamplerState.PointClamp);
					currentLocation.alwaysFrontLayers[i].Key.Draw(mapDisplayDevice, viewport, Location.Origin, wrapAround: false, 4, -1f);
					spriteBatch.End();
				}
			}
			hooks.OnRendered(RenderSteps.World_AlwaysFront, spriteBatch, time, target_screen);
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			if (currentLocation.LightLevel > 0f && timeOfDay < 2000)
			{
				spriteBatch.Draw(fadeToBlackRect, graphics.GraphicsDevice.Viewport.Bounds, Color.Black * currentLocation.LightLevel);
			}
			if (screenGlow)
			{
				spriteBatch.Draw(fadeToBlackRect, graphics.GraphicsDevice.Viewport.Bounds, screenGlowColor * screenGlowAlpha);
			}
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			currentLocation.drawAboveAlwaysFrontLayer(spriteBatch);
			if (!IsFakedBlackScreen())
			{
				spriteBatch.End();
				drawWeather(time, target_screen);
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			}
			if (player.CurrentTool is FishingRod rod && (rod.isTimingCast || rod.castingChosenCountdown > 0f || rod.fishCaught || rod.showingTreasure))
			{
				player.CurrentTool.draw(spriteBatch);
			}
			spriteBatch.End();
			DrawCharacterEmotes(time, target_screen);
			mapDisplayDevice.EndScene();
			if (drawLighting && !IsFakedBlackScreen())
			{
				DrawLightmapOnScreen(time, target_screen);
			}
			if (!eventUp && farmEvent == null && gameMode == 3 && !takingMapScreenshot && isOutdoorMapSmallerThanViewport())
			{
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
				spriteBatch.Draw(fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(0, 0, -viewport.X, graphics.GraphicsDevice.Viewport.Height), Color.Black);
				spriteBatch.Draw(fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(-viewport.X + currentLocation.map.Layers[0].LayerWidth * 64, 0, graphics.GraphicsDevice.Viewport.Width - (-viewport.X + currentLocation.map.Layers[0].LayerWidth * 64), graphics.GraphicsDevice.Viewport.Height), Color.Black);
				spriteBatch.Draw(fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(0, 0, graphics.GraphicsDevice.Viewport.Width, -viewport.Y), Color.Black);
				spriteBatch.Draw(fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(0, -viewport.Y + currentLocation.map.Layers[0].LayerHeight * 64, graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height - (-viewport.Y + currentLocation.map.Layers[0].LayerHeight * 64)), Color.Black);
				spriteBatch.End();
			}
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			if (currentLocation != null && (bool)currentLocation.isOutdoors && !IsFakedBlackScreen() && currentLocation.IsRainingHere())
			{
				bool isGreenRain = IsGreenRainingHere();
				spriteBatch.Draw(staminaRect, graphics.GraphicsDevice.Viewport.Bounds, isGreenRain ? (new Color(0, 120, 150) * 0.22f) : (Color.Blue * 0.2f));
			}
			spriteBatch.End();
			if (farmEvent != null)
			{
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
				farmEvent.draw(spriteBatch);
				spriteBatch.End();
			}
			if (eventUp && currentLocation?.currentEvent != null)
			{
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
				currentLocation.currentEvent.drawAfterMap(spriteBatch);
				spriteBatch.End();
			}
			if (!takingMapScreenshot)
			{
				if (drawGrid)
				{
					spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
					int startingX = -viewport.X % 64;
					float startingY = -viewport.Y % 64;
					for (int x = startingX; x < graphics.GraphicsDevice.Viewport.Width; x += 64)
					{
						spriteBatch.Draw(staminaRect, new Microsoft.Xna.Framework.Rectangle(x, (int)startingY, 1, graphics.GraphicsDevice.Viewport.Height), Color.Red * 0.5f);
					}
					for (float y = startingY; y < (float)graphics.GraphicsDevice.Viewport.Height; y += 64f)
					{
						spriteBatch.Draw(staminaRect, new Microsoft.Xna.Framework.Rectangle(startingX, (int)y, graphics.GraphicsDevice.Viewport.Width, 1), Color.Red * 0.5f);
					}
					spriteBatch.End();
				}
				if (ShouldShowOnscreenUsernames() && currentLocation != null)
				{
					spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
					currentLocation.DrawFarmerUsernames(spriteBatch);
					spriteBatch.End();
				}
				if (flashAlpha > 0f)
				{
					if (options.screenFlash)
					{
						spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
						spriteBatch.Draw(fadeToBlackRect, graphics.GraphicsDevice.Viewport.Bounds, Color.White * Math.Min(1f, flashAlpha));
						spriteBatch.End();
					}
					flashAlpha -= 0.1f;
				}
			}
		}
		hooks.OnRendered(RenderSteps.World, spriteBatch, time, target_screen);
	}

	public virtual void DrawCharacterEmotes(GameTime time, RenderTarget2D target_screen)
	{
		spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
		if (eventUp && currentLocation.currentEvent != null)
		{
			foreach (NPC n in currentLocation.currentEvent.actors)
			{
				if (n.isEmoting)
				{
					Vector2 emotePosition = n.getLocalPosition(viewport);
					if (n.NeedsBirdieEmoteHack())
					{
						emotePosition.X += 64f;
					}
					emotePosition.Y -= 140f;
					if (n.Age == 2)
					{
						emotePosition.Y += 32f;
					}
					else if (n.Gender == Gender.Female)
					{
						emotePosition.Y += 10f;
					}
					CharacterData data = n.GetData();
					if (data != null)
					{
						emotePosition.X += data.EmoteOffset.X;
						emotePosition.Y += data.EmoteOffset.Y;
					}
					spriteBatch.Draw(emoteSpriteSheet, emotePosition, new Microsoft.Xna.Framework.Rectangle(n.CurrentEmoteIndex * 16 % emoteSpriteSheet.Width, n.CurrentEmoteIndex * 16 / emoteSpriteSheet.Width * 16, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)n.StandingPixel.Y / 10000f);
				}
			}
		}
		spriteBatch.End();
	}

	public virtual void DrawLightmapOnScreen(GameTime time, RenderTarget2D target_screen)
	{
		if (hooks.OnRendering(RenderSteps.World_DrawLightmapOnScreen, spriteBatch, time, target_screen))
		{
			spriteBatch.Begin(SpriteSortMode.Deferred, lightingBlend, SamplerState.LinearClamp);
			Viewport vp = base.GraphicsDevice.Viewport;
			vp.Bounds = target_screen?.Bounds ?? base.GraphicsDevice.PresentationParameters.Bounds;
			base.GraphicsDevice.Viewport = vp;
			float render_zoom = options.lightingQuality / 2;
			if (useUnscaledLighting)
			{
				render_zoom /= options.zoomLevel;
			}
			spriteBatch.Draw(lightmap, Vector2.Zero, lightmap.Bounds, Color.White, 0f, Vector2.Zero, render_zoom, SpriteEffects.None, 1f);
			if ((bool)currentLocation.isOutdoors && currentLocation.IsRainingHere())
			{
				spriteBatch.Draw(staminaRect, vp.Bounds, Color.OrangeRed * 0.45f);
			}
		}
		hooks.OnRendered(RenderSteps.World_DrawLightmapOnScreen, spriteBatch, time, target_screen);
		spriteBatch.End();
	}

	public virtual void DrawDebugUIs(GameTime time, RenderTarget2D target_screen)
	{
		StringBuilder sb = _debugStringBuilder;
		sb.Clear();
		if (panMode)
		{
			sb.Append((getOldMouseX() + viewport.X) / 64);
			sb.Append(",");
			sb.Append((getOldMouseY() + viewport.Y) / 64);
		}
		else
		{
			Point playerPixel = player.StandingPixel;
			sb.Append("player: ");
			sb.Append(playerPixel.X / 64);
			sb.Append(", ");
			sb.Append(playerPixel.Y / 64);
		}
		sb.Append(" mouseTransparency: ");
		sb.Append(mouseCursorTransparency);
		sb.Append(" mousePosition: ");
		sb.Append(getMouseX());
		sb.Append(",");
		sb.Append(getMouseY());
		sb.Append(Environment.NewLine);
		sb.Append(" mouseWorldPosition: ");
		sb.Append(getMouseX() + viewport.X);
		sb.Append(",");
		sb.Append(getMouseY() + viewport.Y);
		sb.Append("  debugOutput: ");
		sb.Append(debugOutput);
		PushUIMode();
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
		spriteBatch.DrawString(smallFont, sb, new Vector2(base.GraphicsDevice.Viewport.GetTitleSafeArea().X, base.GraphicsDevice.Viewport.GetTitleSafeArea().Y + smallFont.LineSpacing * 8), Color.Red, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9999999f);
		spriteBatch.End();
		PopUIMode();
	}

	public virtual void DrawGlobalFade(GameTime time, RenderTarget2D target_screen)
	{
		if ((fadeToBlack || globalFade) && !takingMapScreenshot)
		{
			PushUIMode();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			if (hooks.OnRendering(RenderSteps.GlobalFade, spriteBatch, time, target_screen))
			{
				spriteBatch.Draw(fadeToBlackRect, graphics.GraphicsDevice.Viewport.Bounds, Color.Black * ((gameMode == 0) ? (1f - fadeToBlackAlpha) : fadeToBlackAlpha));
			}
			hooks.OnRendered(RenderSteps.GlobalFade, spriteBatch, time, target_screen);
			spriteBatch.End();
			PopUIMode();
		}
	}

	public virtual void DrawLighting(GameTime time, RenderTarget2D target_screen)
	{
		SetRenderTarget(lightmap);
		base.GraphicsDevice.Clear(Color.White * 0f);
		Matrix lighting_matrix = Matrix.Identity;
		if (useUnscaledLighting)
		{
			lighting_matrix = Matrix.CreateScale(options.zoomLevel);
		}
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null, lighting_matrix);
		if (hooks.OnRendering(RenderSteps.World_RenderLightmap, spriteBatch, time, target_screen))
		{
			Color lighting = ((!(currentLocation is MineShaft mine)) ? ((ambientLight.Equals(Color.White) || ((bool)currentLocation.isOutdoors && currentLocation.IsRainingHere())) ? outdoorLight : ambientLight) : mine.getLightingColor(time));
			float light_multiplier = 1f;
			if (player.hasBuff("26"))
			{
				if (lighting == Color.White)
				{
					lighting = new Color(0.75f, 0.75f, 0.75f);
				}
				else
				{
					lighting.R = (byte)Utility.Lerp((int)lighting.R, 255f, 0.5f);
					lighting.G = (byte)Utility.Lerp((int)lighting.G, 255f, 0.5f);
					lighting.B = (byte)Utility.Lerp((int)lighting.B, 255f, 0.5f);
				}
				light_multiplier = 0.33f;
			}
			if (IsGreenRainingHere())
			{
				lighting.R = (byte)Utility.Lerp((int)lighting.R, 255f, 0.25f);
				lighting.G = (byte)Utility.Lerp((int)lighting.R, 0f, 0.25f);
			}
			spriteBatch.Draw(staminaRect, lightmap.Bounds, lighting);
			foreach (LightSource currentLightSource in currentLightSources)
			{
				currentLightSource.Draw(spriteBatch, currentLocation, light_multiplier);
			}
		}
		hooks.OnRendered(RenderSteps.World_RenderLightmap, spriteBatch, time, target_screen);
		spriteBatch.End();
		SetRenderTarget(target_screen);
	}

	public virtual void drawWeather(GameTime time, RenderTarget2D target_screen)
	{
		spriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, SamplerState.PointClamp);
		if (hooks.OnRendering(RenderSteps.World_Weather, spriteBatch, time, target_screen) && currentLocation.IsOutdoors)
		{
			if (currentLocation.IsSnowingHere())
			{
				snowPos.X %= 64f;
				Vector2 v = default(Vector2);
				for (float x = -64f + snowPos.X % 64f; x < (float)viewport.Width; x += 64f)
				{
					for (float y = -64f + snowPos.Y % 64f; y < (float)viewport.Height; y += 64f)
					{
						v.X = (int)x;
						v.Y = (int)y;
						spriteBatch.Draw(mouseCursors, v, new Microsoft.Xna.Framework.Rectangle(368 + (int)(currentGameTime.TotalGameTime.TotalMilliseconds % 1200.0) / 75 * 16, 192, 16, 16), Color.White * 0.8f * options.snowTransparency, 0f, Vector2.Zero, 4.001f, SpriteEffects.None, 1f);
					}
				}
			}
			if (!currentLocation.ignoreDebrisWeather && currentLocation.IsDebrisWeatherHere())
			{
				if (takingMapScreenshot)
				{
					if (debrisWeather != null)
					{
						foreach (WeatherDebris w in debrisWeather)
						{
							Vector2 position = w.position;
							w.position = new Vector2(random.Next(viewport.Width - w.sourceRect.Width * 3), random.Next(viewport.Height - w.sourceRect.Height * 3));
							w.draw(spriteBatch);
							w.position = position;
						}
					}
				}
				else if (viewport.X > -viewport.Width)
				{
					foreach (WeatherDebris item in debrisWeather)
					{
						item.draw(spriteBatch);
					}
				}
			}
			if (currentLocation.IsRainingHere() && !(currentLocation is Summit) && (!eventUp || currentLocation.isTileOnMap(new Vector2(viewport.X / 64, viewport.Y / 64))))
			{
				bool isGreenRain = IsGreenRainingHere();
				Color rainColor = (isGreenRain ? Color.LimeGreen : Color.White);
				int vibrancy = ((!isGreenRain) ? 1 : 2);
				for (int i = 0; i < rainDrops.Length; i++)
				{
					for (int v = 0; v < vibrancy; v++)
					{
						spriteBatch.Draw(rainTexture, rainDrops[i].position, getSourceRectForStandardTileSheet(rainTexture, rainDrops[i].frame + (isGreenRain ? 4 : 0), 16, 16), rainColor, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					}
				}
			}
		}
		hooks.OnRendered(RenderSteps.World_Weather, spriteBatch, time, target_screen);
		spriteBatch.End();
	}

	protected virtual void renderScreenBuffer(RenderTarget2D target_screen)
	{
		graphics.GraphicsDevice.SetRenderTarget(null);
		if (!takingMapScreenshot && !LocalMultiplayer.IsLocalMultiplayer() && (target_screen == null || !target_screen.IsContentLost))
		{
			if (ShouldDrawOnBuffer() && target_screen != null)
			{
				base.GraphicsDevice.Clear(bgColor);
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
				spriteBatch.Draw(target_screen, new Vector2(0f, 0f), target_screen.Bounds, Color.White, 0f, Vector2.Zero, options.zoomLevel, SpriteEffects.None, 1f);
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
				spriteBatch.Draw(uiScreen, new Vector2(0f, 0f), uiScreen.Bounds, Color.White, 0f, Vector2.Zero, options.uiScale, SpriteEffects.None, 1f);
				spriteBatch.End();
			}
			else
			{
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
				spriteBatch.Draw(uiScreen, new Vector2(0f, 0f), uiScreen.Bounds, Color.White, 0f, Vector2.Zero, options.uiScale, SpriteEffects.None, 1f);
				spriteBatch.End();
			}
		}
	}

	public virtual void DrawSplitScreenWindow()
	{
		if (!LocalMultiplayer.IsLocalMultiplayer())
		{
			return;
		}
		graphics.GraphicsDevice.SetRenderTarget(null);
		if (screen == null || !screen.IsContentLost)
		{
			Viewport old_viewport = base.GraphicsDevice.Viewport;
			GraphicsDevice graphicsDevice = base.GraphicsDevice;
			Viewport viewport2 = (base.GraphicsDevice.Viewport = defaultDeviceViewport);
			graphicsDevice.Viewport = viewport2;
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
			spriteBatch.Draw(screen, new Vector2(localMultiplayerWindow.X, localMultiplayerWindow.Y), screen.Bounds, Color.White, 0f, Vector2.Zero, instanceOptions.zoomLevel, SpriteEffects.None, 1f);
			if (uiScreen != null)
			{
				spriteBatch.Draw(uiScreen, new Vector2(localMultiplayerWindow.X, localMultiplayerWindow.Y), uiScreen.Bounds, Color.White, 0f, Vector2.Zero, instanceOptions.uiScale, SpriteEffects.None, 1f);
			}
			spriteBatch.End();
			base.GraphicsDevice.Viewport = old_viewport;
		}
	}

	/// ###########################
	/// METHODS FOR DRAWING THINGS.
	/// ############################
	public static void drawWithBorder(string message, Color borderColor, Color insideColor, Vector2 position)
	{
		drawWithBorder(message, borderColor, insideColor, position, 0f, 1f, 1f, tiny: false);
	}

	public static void drawWithBorder(string message, Color borderColor, Color insideColor, Vector2 position, float rotate, float scale, float layerDepth)
	{
		drawWithBorder(message, borderColor, insideColor, position, rotate, scale, layerDepth, tiny: false);
	}

	public static void drawWithBorder(string message, Color borderColor, Color insideColor, Vector2 position, float rotate, float scale, float layerDepth, bool tiny)
	{
		string[] words = ArgUtility.SplitBySpace(message);
		int offset = 0;
		for (int i = 0; i < words.Length; i++)
		{
			if (words[i].Contains('='))
			{
				spriteBatch.DrawString(tiny ? tinyFont : dialogueFont, words[i], new Vector2(position.X + (float)offset, position.Y), Color.Purple, rotate, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
				offset += (int)((tiny ? tinyFont : dialogueFont).MeasureString(words[i]).X + 8f);
			}
			else
			{
				spriteBatch.DrawString(tiny ? tinyFont : dialogueFont, words[i], new Vector2(position.X + (float)offset, position.Y), insideColor, rotate, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
				offset += (int)((tiny ? tinyFont : dialogueFont).MeasureString(words[i]).X + 8f);
			}
		}
	}

	public static bool isOutdoorMapSmallerThanViewport()
	{
		if (uiMode)
		{
			return false;
		}
		if (currentLocation != null && currentLocation.IsOutdoors && !(currentLocation is Summit))
		{
			if (currentLocation.map.Layers[0].LayerWidth * 64 >= viewport.Width)
			{
				return currentLocation.map.Layers[0].LayerHeight * 64 < viewport.Height;
			}
			return true;
		}
		return false;
	}

	protected virtual void drawHUD()
	{
		if (eventUp || farmEvent != null)
		{
			return;
		}
		float modifier = 0.625f;
		Vector2 topOfBar = new Vector2(graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Right - 48 - 8, graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Bottom - 224 - 16 - (int)((float)(player.MaxStamina - 270) * modifier));
		if (isOutdoorMapSmallerThanViewport())
		{
			topOfBar.X = Math.Min(topOfBar.X, -viewport.X + currentLocation.map.Layers[0].LayerWidth * 64 - 48);
		}
		if (staminaShakeTimer > 0)
		{
			topOfBar.X += random.Next(-3, 4);
			topOfBar.Y += random.Next(-3, 4);
		}
		spriteBatch.Draw(mouseCursors, topOfBar, new Microsoft.Xna.Framework.Rectangle(256, 408, 12, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
		spriteBatch.Draw(mouseCursors, new Microsoft.Xna.Framework.Rectangle((int)topOfBar.X, (int)(topOfBar.Y + 64f), 48, graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Bottom - 64 - 16 - (int)(topOfBar.Y + 64f - 8f)), new Microsoft.Xna.Framework.Rectangle(256, 424, 12, 16), Color.White);
		spriteBatch.Draw(mouseCursors, new Vector2(topOfBar.X, topOfBar.Y + 224f + (float)(int)((float)(player.MaxStamina - 270) * modifier) - 64f), new Microsoft.Xna.Framework.Rectangle(256, 448, 12, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
		Microsoft.Xna.Framework.Rectangle r = new Microsoft.Xna.Framework.Rectangle((int)topOfBar.X + 12, (int)topOfBar.Y + 16 + 32 + (int)((float)player.MaxStamina * modifier) - (int)(Math.Max(0f, player.Stamina) * modifier), 24, (int)(player.Stamina * modifier) - 1);
		if ((float)getOldMouseX() >= topOfBar.X && (float)getOldMouseY() >= topOfBar.Y)
		{
			drawWithBorder((int)Math.Max(0f, player.Stamina) + "/" + player.MaxStamina, Color.Black * 0f, Color.White, topOfBar + new Vector2(0f - dialogueFont.MeasureString("999/999").X - 16f - (float)(showingHealth ? 64 : 0), 64f));
		}
		Color c = Utility.getRedToGreenLerpColor(player.stamina / (float)(int)player.maxStamina);
		spriteBatch.Draw(staminaRect, r, c);
		r.Height = 4;
		c.R = (byte)Math.Max(0, c.R - 50);
		c.G = (byte)Math.Max(0, c.G - 50);
		spriteBatch.Draw(staminaRect, r, c);
		if ((bool)player.exhausted)
		{
			spriteBatch.Draw(mouseCursors, topOfBar - new Vector2(0f, 11f) * 4f, new Microsoft.Xna.Framework.Rectangle(191, 406, 12, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			if ((float)getOldMouseX() >= topOfBar.X && (float)getOldMouseY() >= topOfBar.Y - 44f)
			{
				drawWithBorder(content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3747"), Color.Black * 0f, Color.White, topOfBar + new Vector2(0f - dialogueFont.MeasureString(content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3747")).X - 16f - (float)(showingHealth ? 64 : 0), 96f));
			}
		}
		if (currentLocation is MineShaft || currentLocation is Woods || currentLocation is SlimeHutch || currentLocation is VolcanoDungeon || player.health < player.maxHealth)
		{
			showingHealthBar = true;
			showingHealth = true;
			int bar_full_height = 168 + (player.maxHealth - 100);
			int height = (int)((float)player.health / (float)player.maxHealth * (float)bar_full_height);
			topOfBar.X -= 56 + ((hitShakeTimer > 0) ? random.Next(-3, 4) : 0);
			topOfBar.Y = graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Bottom - 224 - 16 - (player.maxHealth - 100);
			spriteBatch.Draw(mouseCursors, topOfBar, new Microsoft.Xna.Framework.Rectangle(268, 408, 12, 16), (player.health < 20) ? (Color.Pink * ((float)Math.Sin(currentGameTime.TotalGameTime.TotalMilliseconds / (double)((float)player.health * 50f)) / 4f + 0.9f)) : Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			spriteBatch.Draw(mouseCursors, new Microsoft.Xna.Framework.Rectangle((int)topOfBar.X, (int)(topOfBar.Y + 64f), 48, graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Bottom - 64 - 16 - (int)(topOfBar.Y + 64f)), new Microsoft.Xna.Framework.Rectangle(268, 424, 12, 16), (player.health < 20) ? (Color.Pink * ((float)Math.Sin(currentGameTime.TotalGameTime.TotalMilliseconds / (double)((float)player.health * 50f)) / 4f + 0.9f)) : Color.White);
			spriteBatch.Draw(mouseCursors, new Vector2(topOfBar.X, topOfBar.Y + 224f + (float)(player.maxHealth - 100) - 64f), new Microsoft.Xna.Framework.Rectangle(268, 448, 12, 16), (player.health < 20) ? (Color.Pink * ((float)Math.Sin(currentGameTime.TotalGameTime.TotalMilliseconds / (double)((float)player.health * 50f)) / 4f + 0.9f)) : Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			Microsoft.Xna.Framework.Rectangle health_bar_rect = new Microsoft.Xna.Framework.Rectangle((int)topOfBar.X + 12, (int)topOfBar.Y + 16 + 32 + bar_full_height - height, 24, height);
			c = Utility.getRedToGreenLerpColor((float)player.health / (float)player.maxHealth);
			spriteBatch.Draw(staminaRect, health_bar_rect, staminaRect.Bounds, c, 0f, Vector2.Zero, SpriteEffects.None, 1f);
			c.R = (byte)Math.Max(0, c.R - 50);
			c.G = (byte)Math.Max(0, c.G - 50);
			if ((float)getOldMouseX() >= topOfBar.X && (float)getOldMouseY() >= topOfBar.Y && (float)getOldMouseX() < topOfBar.X + 32f)
			{
				drawWithBorder(Math.Max(0, player.health) + "/" + player.maxHealth, Color.Black * 0f, Color.Red, topOfBar + new Vector2(0f - dialogueFont.MeasureString("999/999").X - 32f, 64f));
			}
			health_bar_rect.Height = 4;
			spriteBatch.Draw(staminaRect, health_bar_rect, staminaRect.Bounds, c, 0f, Vector2.Zero, SpriteEffects.None, 1f);
		}
		else
		{
			showingHealth = false;
		}
		foreach (IClickableMenu menu in onScreenMenus)
		{
			if (menu != chatBox)
			{
				menu.update(currentGameTime);
				menu.draw(spriteBatch);
			}
		}
		if (!player.professions.Contains(17) || !currentLocation.IsOutdoors)
		{
			return;
		}
		foreach (KeyValuePair<Vector2, Object> v in currentLocation.objects.Pairs)
		{
			if (((bool)v.Value.isSpawnedObject || v.Value.QualifiedItemId == "(O)590") && !Utility.isOnScreen(v.Key * 64f + new Vector2(32f, 32f), 64))
			{
				Microsoft.Xna.Framework.Rectangle vpbounds = graphics.GraphicsDevice.Viewport.Bounds;
				Vector2 onScreenPosition = default(Vector2);
				float rotation = 0f;
				if (v.Key.X * 64f > (float)(viewport.MaxCorner.X - 64))
				{
					onScreenPosition.X = vpbounds.Right - 8;
					rotation = (float)Math.PI / 2f;
				}
				else if (v.Key.X * 64f < (float)viewport.X)
				{
					onScreenPosition.X = 8f;
					rotation = -(float)Math.PI / 2f;
				}
				else
				{
					onScreenPosition.X = v.Key.X * 64f - (float)viewport.X;
				}
				if (v.Key.Y * 64f > (float)(viewport.MaxCorner.Y - 64))
				{
					onScreenPosition.Y = vpbounds.Bottom - 8;
					rotation = (float)Math.PI;
				}
				else if (v.Key.Y * 64f < (float)viewport.Y)
				{
					onScreenPosition.Y = 8f;
				}
				else
				{
					onScreenPosition.Y = v.Key.Y * 64f - (float)viewport.Y;
				}
				if (onScreenPosition.X == 8f && onScreenPosition.Y == 8f)
				{
					rotation += (float)Math.PI / 4f;
				}
				if (onScreenPosition.X == 8f && onScreenPosition.Y == (float)(vpbounds.Bottom - 8))
				{
					rotation += (float)Math.PI / 4f;
				}
				if (onScreenPosition.X == (float)(vpbounds.Right - 8) && onScreenPosition.Y == 8f)
				{
					rotation -= (float)Math.PI / 4f;
				}
				if (onScreenPosition.X == (float)(vpbounds.Right - 8) && onScreenPosition.Y == (float)(vpbounds.Bottom - 8))
				{
					rotation -= (float)Math.PI / 4f;
				}
				Microsoft.Xna.Framework.Rectangle srcRect = new Microsoft.Xna.Framework.Rectangle(412, 495, 5, 4);
				float renderScale = 4f;
				Vector2 safePos = Utility.makeSafe(renderSize: new Vector2((float)srcRect.Width * renderScale, (float)srcRect.Height * renderScale), renderPos: onScreenPosition);
				spriteBatch.Draw(mouseCursors, safePos, srcRect, Color.White, rotation, new Vector2(2f, 2f), renderScale, SpriteEffects.None, 1f);
			}
		}
		if (!currentLocation.orePanPoint.Equals(Point.Zero) && !Utility.isOnScreen(Utility.PointToVector2(currentLocation.orePanPoint.Value) * 64f + new Vector2(32f, 32f), 64))
		{
			Vector2 onScreenPosition = default(Vector2);
			float rotation = 0f;
			if (currentLocation.orePanPoint.X * 64 > viewport.MaxCorner.X - 64)
			{
				onScreenPosition.X = graphics.GraphicsDevice.Viewport.Bounds.Right - 8;
				rotation = (float)Math.PI / 2f;
			}
			else if (currentLocation.orePanPoint.X * 64 < viewport.X)
			{
				onScreenPosition.X = 8f;
				rotation = -(float)Math.PI / 2f;
			}
			else
			{
				onScreenPosition.X = currentLocation.orePanPoint.X * 64 - viewport.X;
			}
			if (currentLocation.orePanPoint.Y * 64 > viewport.MaxCorner.Y - 64)
			{
				onScreenPosition.Y = graphics.GraphicsDevice.Viewport.Bounds.Bottom - 8;
				rotation = (float)Math.PI;
			}
			else if (currentLocation.orePanPoint.Y * 64 < viewport.Y)
			{
				onScreenPosition.Y = 8f;
			}
			else
			{
				onScreenPosition.Y = currentLocation.orePanPoint.Y * 64 - viewport.Y;
			}
			if (onScreenPosition.X == 8f && onScreenPosition.Y == 8f)
			{
				rotation += (float)Math.PI / 4f;
			}
			if (onScreenPosition.X == 8f && onScreenPosition.Y == (float)(graphics.GraphicsDevice.Viewport.Bounds.Bottom - 8))
			{
				rotation += (float)Math.PI / 4f;
			}
			if (onScreenPosition.X == (float)(graphics.GraphicsDevice.Viewport.Bounds.Right - 8) && onScreenPosition.Y == 8f)
			{
				rotation -= (float)Math.PI / 4f;
			}
			if (onScreenPosition.X == (float)(graphics.GraphicsDevice.Viewport.Bounds.Right - 8) && onScreenPosition.Y == (float)(graphics.GraphicsDevice.Viewport.Bounds.Bottom - 8))
			{
				rotation -= (float)Math.PI / 4f;
			}
			spriteBatch.Draw(mouseCursors, onScreenPosition, new Microsoft.Xna.Framework.Rectangle(412, 495, 5, 4), Color.Cyan, rotation, new Vector2(2f, 2f), 4f, SpriteEffects.None, 1f);
		}
	}

	public static void InvalidateOldMouseMovement()
	{
		MouseState input = Game1.input.GetMouseState();
		oldMouseState = new MouseState(input.X, input.Y, oldMouseState.ScrollWheelValue, oldMouseState.LeftButton, oldMouseState.MiddleButton, oldMouseState.RightButton, oldMouseState.XButton1, oldMouseState.XButton2);
	}

	public static bool IsRenderingNonNativeUIScale()
	{
		return options.uiScale != options.zoomLevel;
	}

	public virtual void drawMouseCursor()
	{
		if (activeClickableMenu == null && timerUntilMouseFade > 0)
		{
			timerUntilMouseFade -= currentGameTime.ElapsedGameTime.Milliseconds;
			lastMousePositionBeforeFade = getMousePosition();
		}
		if (options.gamepadControls && timerUntilMouseFade <= 0 && activeClickableMenu == null && (emoteMenu == null || emoteMenu.gamepadMode))
		{
			mouseCursorTransparency = 0f;
		}
		if (activeClickableMenu == null && mouseCursor > cursor_none && currentLocation != null)
		{
			if (IsRenderingNonNativeUIScale())
			{
				spriteBatch.End();
				PopUIMode();
				if (ShouldDrawOnBuffer())
				{
					SetRenderTarget(screen);
				}
				else
				{
					SetRenderTarget(null);
				}
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			}
			if (!(mouseCursorTransparency > 0f) || !Utility.canGrabSomethingFromHere(getOldMouseX() + viewport.X, getOldMouseY() + viewport.Y, player) || mouseCursor == cursor_gift)
			{
				if (player.ActiveObject != null && mouseCursor != cursor_gift && !eventUp && currentMinigame == null && !player.isRidingHorse() && player.CanMove && displayFarmer)
				{
					if (mouseCursorTransparency > 0f || options.showPlacementTileForGamepad)
					{
						player.ActiveObject.drawPlacementBounds(spriteBatch, currentLocation);
						if (mouseCursorTransparency > 0f)
						{
							spriteBatch.End();
							PushUIMode();
							spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
							bool canPlace = Utility.playerCanPlaceItemHere(currentLocation, player.CurrentItem, getMouseX() + viewport.X, getMouseY() + viewport.Y, player) || (Utility.isThereAnObjectHereWhichAcceptsThisItem(currentLocation, player.CurrentItem, getMouseX() + viewport.X, getMouseY() + viewport.Y) && Utility.withinRadiusOfPlayer(getMouseX() + viewport.X, getMouseY() + viewport.Y, 1, player));
							player.CurrentItem?.drawInMenu(spriteBatch, new Vector2(getMouseX() + 16, getMouseY() + 16), canPlace ? (dialogueButtonScale / 75f + 1f) : 1f, canPlace ? 1f : 0.5f, 0.999f);
							spriteBatch.End();
							PopUIMode();
							spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
						}
					}
				}
				else if (mouseCursor == cursor_default && isActionAtCurrentCursorTile && currentMinigame == null)
				{
					mouseCursor = (isSpeechAtCurrentCursorTile ? cursor_talk : (isInspectionAtCurrentCursorTile ? cursor_look : cursor_grab));
				}
				else if (mouseCursorTransparency > 0f)
				{
					NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>> animals = currentLocation.animals;
					if (animals != null)
					{
						Vector2 mousePos = new Vector2(getOldMouseX() + uiViewport.X, getOldMouseY() + uiViewport.Y);
						bool mouseWithinRadiusOfPlayer = Utility.withinRadiusOfPlayer((int)mousePos.X, (int)mousePos.Y, 1, player);
						foreach (KeyValuePair<long, FarmAnimal> kvp in animals.Pairs)
						{
							Microsoft.Xna.Framework.Rectangle animalBounds = kvp.Value.GetCursorPetBoundingBox();
							if (!kvp.Value.wasPet && animalBounds.Contains((int)mousePos.X, (int)mousePos.Y))
							{
								mouseCursor = cursor_grab;
								if (!mouseWithinRadiusOfPlayer)
								{
									mouseCursorTransparency = 0.5f;
								}
								break;
							}
						}
					}
				}
			}
			if (IsRenderingNonNativeUIScale())
			{
				spriteBatch.End();
				PushUIMode();
				SetRenderTarget(uiScreen);
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			}
			if (currentMinigame != null)
			{
				mouseCursor = cursor_default;
			}
			if (!freezeControls && !options.hardwareCursor)
			{
				spriteBatch.Draw(mouseCursors, new Vector2(getMouseX(), getMouseY()), getSourceRectForStandardTileSheet(mouseCursors, mouseCursor, 16, 16), Color.White * mouseCursorTransparency, 0f, Vector2.Zero, 4f + dialogueButtonScale / 150f, SpriteEffects.None, 1f);
			}
			wasMouseVisibleThisFrame = mouseCursorTransparency > 0f;
			_lastDrewMouseCursor = wasMouseVisibleThisFrame;
		}
		mouseCursor = cursor_default;
		if (!isActionAtCurrentCursorTile && activeClickableMenu == null)
		{
			mouseCursorTransparency = 1f;
		}
	}

	public static void panScreen(int x, int y)
	{
		int old_ui_mode_count = uiModeCount;
		while (uiModeCount > 0)
		{
			PopUIMode();
		}
		previousViewportPosition.X = viewport.Location.X;
		previousViewportPosition.Y = viewport.Location.Y;
		viewport.X += x;
		viewport.Y += y;
		clampViewportToGameMap();
		updateRaindropPosition();
		for (int i = 0; i < old_ui_mode_count; i++)
		{
			PushUIMode();
		}
	}

	public static void clampViewportToGameMap()
	{
		if (viewport.X < 0)
		{
			viewport.X = 0;
		}
		if (viewport.X > currentLocation.map.DisplayWidth - viewport.Width)
		{
			viewport.X = currentLocation.map.DisplayWidth - viewport.Width;
		}
		if (viewport.Y < 0)
		{
			viewport.Y = 0;
		}
		if (viewport.Y > currentLocation.map.DisplayHeight - viewport.Height)
		{
			viewport.Y = currentLocation.map.DisplayHeight - viewport.Height;
		}
	}

	protected void drawDialogueBox()
	{
		if (currentSpeaker != null)
		{
			int messageHeight = (int)dialogueFont.MeasureString(currentSpeaker.CurrentDialogue.Peek().getCurrentDialogue()).Y;
			messageHeight = Math.Max(messageHeight, 320);
			drawDialogueBox((base.GraphicsDevice.Viewport.GetTitleSafeArea().Width - Math.Min(1280, base.GraphicsDevice.Viewport.GetTitleSafeArea().Width - 128)) / 2, base.GraphicsDevice.Viewport.GetTitleSafeArea().Height - messageHeight, Math.Min(1280, base.GraphicsDevice.Viewport.GetTitleSafeArea().Width - 128), messageHeight, speaker: true, drawOnlyBox: false, null, objectDialoguePortraitPerson != null && currentSpeaker == null);
		}
	}

	public static void drawDialogueBox(string message)
	{
		drawDialogueBox(viewport.Width / 2, viewport.Height / 2, speaker: false, drawOnlyBox: false, message);
	}

	public static void drawDialogueBox(int centerX, int centerY, bool speaker, bool drawOnlyBox, string message)
	{
		string text = null;
		if (speaker && currentSpeaker != null)
		{
			text = currentSpeaker.CurrentDialogue.Peek().getCurrentDialogue();
		}
		else if (message != null)
		{
			text = message;
		}
		else if (currentObjectDialogue.Count > 0)
		{
			text = currentObjectDialogue.Peek();
		}
		if (text != null)
		{
			Vector2 vector = dialogueFont.MeasureString(text);
			int width = (int)vector.X + 128;
			int height = (int)vector.Y + 128;
			int x = centerX - width / 2;
			int y = centerY - height / 2;
			drawDialogueBox(x, y, width, height, speaker, drawOnlyBox, message, objectDialoguePortraitPerson != null && !speaker);
		}
	}

	public static void DrawBox(int x, int y, int width, int height, Color? color = null)
	{
		Microsoft.Xna.Framework.Rectangle sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64);
		sourceRect.X = 64;
		sourceRect.Y = 128;
		Texture2D menu_texture = menuTexture;
		Color draw_color = Color.White;
		Color inner_color = Color.White;
		if (color.HasValue)
		{
			draw_color = color.Value;
			menu_texture = uncoloredMenuTexture;
			inner_color = new Color((int)Utility.Lerp((int)draw_color.R, Math.Min(255, draw_color.R + 150), 0.65f), (int)Utility.Lerp((int)draw_color.G, Math.Min(255, draw_color.G + 150), 0.65f), (int)Utility.Lerp((int)draw_color.B, Math.Min(255, draw_color.B + 150), 0.65f));
		}
		spriteBatch.Draw(menu_texture, new Microsoft.Xna.Framework.Rectangle(x, y, width, height), sourceRect, inner_color);
		sourceRect.Y = 0;
		Vector2 offset = new Vector2((float)(-sourceRect.Width) * 0.5f, (float)(-sourceRect.Height) * 0.5f);
		sourceRect.X = 0;
		spriteBatch.Draw(menu_texture, new Vector2((float)x + offset.X, (float)y + offset.Y), sourceRect, draw_color);
		sourceRect.X = 192;
		spriteBatch.Draw(menu_texture, new Vector2((float)x + offset.X + (float)width, (float)y + offset.Y), sourceRect, draw_color);
		sourceRect.Y = 192;
		spriteBatch.Draw(menu_texture, new Vector2((float)(x + width) + offset.X, (float)(y + height) + offset.Y), sourceRect, draw_color);
		sourceRect.X = 0;
		spriteBatch.Draw(menu_texture, new Vector2((float)x + offset.X, (float)(y + height) + offset.Y), sourceRect, draw_color);
		sourceRect.X = 128;
		sourceRect.Y = 0;
		spriteBatch.Draw(menu_texture, new Microsoft.Xna.Framework.Rectangle(64 + x + (int)offset.X, y + (int)offset.Y, width - 64, 64), sourceRect, draw_color);
		sourceRect.Y = 192;
		spriteBatch.Draw(menu_texture, new Microsoft.Xna.Framework.Rectangle(64 + x + (int)offset.X, y + (int)offset.Y + height, width - 64, 64), sourceRect, draw_color);
		sourceRect.Y = 128;
		sourceRect.X = 0;
		spriteBatch.Draw(menu_texture, new Microsoft.Xna.Framework.Rectangle(x + (int)offset.X, y + (int)offset.Y + 64, 64, height - 64), sourceRect, draw_color);
		sourceRect.X = 192;
		spriteBatch.Draw(menu_texture, new Microsoft.Xna.Framework.Rectangle(x + width + (int)offset.X, y + (int)offset.Y + 64, 64, height - 64), sourceRect, draw_color);
	}

	public static void drawDialogueBox(int x, int y, int width, int height, bool speaker, bool drawOnlyBox, string message = null, bool objectDialogueWithPortrait = false, bool ignoreTitleSafe = true, int r = -1, int g = -1, int b = -1)
	{
		if (!drawOnlyBox)
		{
			return;
		}
		Microsoft.Xna.Framework.Rectangle titleSafeArea = graphics.GraphicsDevice.Viewport.GetTitleSafeArea();
		int screenHeight = titleSafeArea.Height;
		int screenWidth = titleSafeArea.Width;
		int dialogueX = 0;
		int dialogueY = 0;
		if (!ignoreTitleSafe)
		{
			dialogueY = ((y <= titleSafeArea.Y) ? (titleSafeArea.Y - y) : 0);
		}
		int everythingYOffset = 0;
		width = Math.Min(titleSafeArea.Width, width);
		if (!isQuestion && currentSpeaker == null && currentObjectDialogue.Count > 0 && !drawOnlyBox)
		{
			width = (int)dialogueFont.MeasureString(currentObjectDialogue.Peek()).X + 128;
			height = (int)dialogueFont.MeasureString(currentObjectDialogue.Peek()).Y + 64;
			x = screenWidth / 2 - width / 2;
			everythingYOffset = ((height > 256) ? (-(height - 256)) : 0);
		}
		Microsoft.Xna.Framework.Rectangle sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64);
		int addedTileHeightForQuestions = -1;
		if (questionChoices.Count >= 3)
		{
			addedTileHeightForQuestions = questionChoices.Count - 3;
		}
		if (!drawOnlyBox && currentObjectDialogue.Count > 0)
		{
			if (dialogueFont.MeasureString(currentObjectDialogue.Peek()).Y >= (float)(height - 128))
			{
				addedTileHeightForQuestions -= (int)(((float)(height - 128) - dialogueFont.MeasureString(currentObjectDialogue.Peek()).Y) / 64f) - 1;
			}
			else
			{
				height += (int)dialogueFont.MeasureString(currentObjectDialogue.Peek()).Y / 2;
				everythingYOffset -= (int)dialogueFont.MeasureString(currentObjectDialogue.Peek()).Y / 2;
				if ((int)dialogueFont.MeasureString(currentObjectDialogue.Peek()).Y / 2 > 64)
				{
					addedTileHeightForQuestions = 0;
				}
			}
		}
		if (currentSpeaker != null && isQuestion && currentSpeaker.CurrentDialogue.Peek().getCurrentDialogue().Substring(0, currentDialogueCharacterIndex)
			.Contains(Environment.NewLine))
		{
			addedTileHeightForQuestions++;
		}
		sourceRect.Width = 64;
		sourceRect.Height = 64;
		sourceRect.X = 64;
		sourceRect.Y = 128;
		Color tint = ((r == -1) ? Color.White : new Color(r, g, b));
		Texture2D texture = ((r == -1) ? menuTexture : uncoloredMenuTexture);
		spriteBatch.Draw(texture, new Microsoft.Xna.Framework.Rectangle(28 + x + dialogueX, 28 + y - 64 * addedTileHeightForQuestions + dialogueY + everythingYOffset, width - 64, height - 64 + addedTileHeightForQuestions * 64), sourceRect, (r == -1) ? tint : new Color((int)Utility.Lerp(r, Math.Min(255, r + 150), 0.65f), (int)Utility.Lerp(g, Math.Min(255, g + 150), 0.65f), (int)Utility.Lerp(b, Math.Min(255, b + 150), 0.65f)));
		sourceRect.Y = 0;
		sourceRect.X = 0;
		spriteBatch.Draw(texture, new Vector2(x + dialogueX, y - 64 * addedTileHeightForQuestions + dialogueY + everythingYOffset), sourceRect, tint);
		sourceRect.X = 192;
		spriteBatch.Draw(texture, new Vector2(x + width + dialogueX - 64, y - 64 * addedTileHeightForQuestions + dialogueY + everythingYOffset), sourceRect, tint);
		sourceRect.Y = 192;
		spriteBatch.Draw(texture, new Vector2(x + width + dialogueX - 64, y + height + dialogueY - 64 + everythingYOffset), sourceRect, tint);
		sourceRect.X = 0;
		spriteBatch.Draw(texture, new Vector2(x + dialogueX, y + height + dialogueY - 64 + everythingYOffset), sourceRect, tint);
		sourceRect.X = 128;
		sourceRect.Y = 0;
		spriteBatch.Draw(texture, new Microsoft.Xna.Framework.Rectangle(64 + x + dialogueX, y - 64 * addedTileHeightForQuestions + dialogueY + everythingYOffset, width - 128, 64), sourceRect, tint);
		sourceRect.Y = 192;
		spriteBatch.Draw(texture, new Microsoft.Xna.Framework.Rectangle(64 + x + dialogueX, y + height + dialogueY - 64 + everythingYOffset, width - 128, 64), sourceRect, tint);
		sourceRect.Y = 128;
		sourceRect.X = 0;
		spriteBatch.Draw(texture, new Microsoft.Xna.Framework.Rectangle(x + dialogueX, y - 64 * addedTileHeightForQuestions + dialogueY + 64 + everythingYOffset, 64, height - 128 + addedTileHeightForQuestions * 64), sourceRect, tint);
		sourceRect.X = 192;
		spriteBatch.Draw(texture, new Microsoft.Xna.Framework.Rectangle(x + width + dialogueX - 64, y - 64 * addedTileHeightForQuestions + dialogueY + 64 + everythingYOffset, 64, height - 128 + addedTileHeightForQuestions * 64), sourceRect, tint);
		if ((objectDialogueWithPortrait && objectDialoguePortraitPerson != null) || (speaker && currentSpeaker != null && currentSpeaker.CurrentDialogue.Count > 0 && currentSpeaker.CurrentDialogue.Peek().showPortrait))
		{
			NPC theSpeaker = (objectDialogueWithPortrait ? objectDialoguePortraitPerson : currentSpeaker);
			Microsoft.Xna.Framework.Rectangle portraitRect;
			switch ((!objectDialogueWithPortrait) ? theSpeaker.CurrentDialogue.Peek().CurrentEmotion : ((objectDialoguePortraitPerson.Name == player.spouse) ? "$l" : "$neutral"))
			{
			case "$h":
				portraitRect = new Microsoft.Xna.Framework.Rectangle(64, 0, 64, 64);
				break;
			case "$s":
				portraitRect = new Microsoft.Xna.Framework.Rectangle(0, 64, 64, 64);
				break;
			case "$u":
				portraitRect = new Microsoft.Xna.Framework.Rectangle(64, 64, 64, 64);
				break;
			case "$l":
				portraitRect = new Microsoft.Xna.Framework.Rectangle(0, 128, 64, 64);
				break;
			case "$a":
				portraitRect = new Microsoft.Xna.Framework.Rectangle(64, 128, 64, 64);
				break;
			case "$k":
			case "$neutral":
				portraitRect = new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64);
				break;
			default:
				portraitRect = getSourceRectForStandardTileSheet(theSpeaker.Portrait, Convert.ToInt32(theSpeaker.CurrentDialogue.Peek().CurrentEmotion.Substring(1)));
				break;
			}
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp);
			if (theSpeaker.Portrait != null)
			{
				spriteBatch.Draw(mouseCursors, new Vector2(dialogueX + x + 768, screenHeight - 320 - 64 * addedTileHeightForQuestions - 256 + dialogueY + 16 - 60 + everythingYOffset), new Microsoft.Xna.Framework.Rectangle(333, 305, 80, 87), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.98f);
				spriteBatch.Draw(theSpeaker.Portrait, new Vector2(dialogueX + x + 768 + 32, screenHeight - 320 - 64 * addedTileHeightForQuestions - 256 + dialogueY + 16 - 60 + everythingYOffset), portraitRect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
			}
			spriteBatch.End();
			spriteBatch.Begin();
			if (isQuestion)
			{
				spriteBatch.DrawString(dialogueFont, theSpeaker.displayName, new Vector2(928f - dialogueFont.MeasureString(theSpeaker.displayName).X / 2f + (float)dialogueX + (float)x, (float)(screenHeight - 320 - 64 * addedTileHeightForQuestions) - dialogueFont.MeasureString(theSpeaker.displayName).Y + (float)dialogueY + 21f + (float)everythingYOffset) + new Vector2(2f, 2f), new Color(150, 150, 150));
			}
			spriteBatch.DrawString(dialogueFont, theSpeaker.Name.Equals("Lewis") ? content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3756") : theSpeaker.displayName, new Vector2((float)(dialogueX + x + 896 + 32) - dialogueFont.MeasureString(theSpeaker.Name.Equals("Lewis") ? content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3756") : theSpeaker.displayName).X / 2f, (float)(screenHeight - 320 - 64 * addedTileHeightForQuestions) - dialogueFont.MeasureString(theSpeaker.Name.Equals("Lewis") ? content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3756") : theSpeaker.displayName).Y + (float)dialogueY + 21f + 8f + (float)everythingYOffset), textColor);
		}
		if (drawOnlyBox)
		{
			return;
		}
		string text = "";
		if (currentSpeaker != null && currentSpeaker.CurrentDialogue.Count > 0)
		{
			if (currentSpeaker.CurrentDialogue.Peek() == null || currentSpeaker.CurrentDialogue.Peek().getCurrentDialogue().Length < currentDialogueCharacterIndex - 1)
			{
				dialogueUp = false;
				currentDialogueCharacterIndex = 0;
				playSound("dialogueCharacterClose");
				player.forceCanMove();
				return;
			}
			text = currentSpeaker.CurrentDialogue.Peek().getCurrentDialogue().Substring(0, currentDialogueCharacterIndex);
		}
		else if (message != null)
		{
			text = message;
		}
		else if (currentObjectDialogue.Count > 0)
		{
			text = ((currentObjectDialogue.Peek().Length <= 1) ? "" : currentObjectDialogue.Peek().Substring(0, currentDialogueCharacterIndex));
		}
		Vector2 textPosition = ((dialogueFont.MeasureString(text).X > (float)(screenWidth - 256 - dialogueX)) ? new Vector2(128 + dialogueX, screenHeight - 64 * addedTileHeightForQuestions - 256 - 16 + dialogueY + everythingYOffset) : ((currentSpeaker != null && currentSpeaker.CurrentDialogue.Count > 0) ? new Vector2((float)(screenWidth / 2) - dialogueFont.MeasureString(currentSpeaker.CurrentDialogue.Peek().getCurrentDialogue()).X / 2f + (float)dialogueX, screenHeight - 64 * addedTileHeightForQuestions - 256 - 16 + dialogueY + everythingYOffset) : ((message != null) ? new Vector2((float)(screenWidth / 2) - dialogueFont.MeasureString(text).X / 2f + (float)dialogueX, y + 96 + 4) : ((!isQuestion) ? new Vector2((float)(screenWidth / 2) - dialogueFont.MeasureString((currentObjectDialogue.Count == 0) ? "" : currentObjectDialogue.Peek()).X / 2f + (float)dialogueX, y + 4 + everythingYOffset) : new Vector2((float)(screenWidth / 2) - dialogueFont.MeasureString((currentObjectDialogue.Count == 0) ? "" : currentObjectDialogue.Peek()).X / 2f + (float)dialogueX, screenHeight - 64 * addedTileHeightForQuestions - 256 - (16 + (questionChoices.Count - 2) * 64) + dialogueY + everythingYOffset)))));
		if (!drawOnlyBox)
		{
			spriteBatch.DrawString(dialogueFont, text, textPosition + new Vector2(3f, 0f), textShadowColor);
			spriteBatch.DrawString(dialogueFont, text, textPosition + new Vector2(3f, 3f), textShadowColor);
			spriteBatch.DrawString(dialogueFont, text, textPosition + new Vector2(0f, 3f), textShadowColor);
			spriteBatch.DrawString(dialogueFont, text, textPosition, textColor);
		}
		if (dialogueFont.MeasureString(text).Y <= 64f)
		{
			dialogueY += 64;
		}
		if (isQuestion && !dialogueTyping)
		{
			for (int i = 0; i < questionChoices.Count; i++)
			{
				if (currentQuestionChoice == i)
				{
					textPosition.X = 80 + dialogueX + x;
					textPosition.Y = (float)(screenHeight - (5 + addedTileHeightForQuestions + 1) * 64) + ((text.Trim().Length > 0) ? dialogueFont.MeasureString(text).Y : 0f) + 128f + (float)(48 * i) - (float)(16 + (questionChoices.Count - 2) * 64) + (float)dialogueY + (float)everythingYOffset;
					spriteBatch.End();
					spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp);
					spriteBatch.Draw(objectSpriteSheet, textPosition + new Vector2((float)Math.Cos((double)currentGameTime.TotalGameTime.Milliseconds * Math.PI / 512.0) * 3f, 0f), GameLocation.getSourceRectForObject(26), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					spriteBatch.End();
					spriteBatch.Begin();
					textPosition.X = 160 + dialogueX + x;
					textPosition.Y = (float)(screenHeight - (5 + addedTileHeightForQuestions + 1) * 64) + ((text.Trim().Length > 1) ? dialogueFont.MeasureString(text).Y : 0f) + 128f - (float)((questionChoices.Count - 2) * 64) + (float)(48 * i) + (float)dialogueY + (float)everythingYOffset;
					spriteBatch.DrawString(dialogueFont, questionChoices[i].responseText, textPosition, textColor);
				}
				else
				{
					textPosition.X = 128 + dialogueX + x;
					textPosition.Y = (float)(screenHeight - (5 + addedTileHeightForQuestions + 1) * 64) + ((text.Trim().Length > 1) ? dialogueFont.MeasureString(text).Y : 0f) + 128f - (float)((questionChoices.Count - 2) * 64) + (float)(48 * i) + (float)dialogueY + (float)everythingYOffset;
					spriteBatch.DrawString(dialogueFont, questionChoices[i].responseText, textPosition, unselectedOptionColor);
				}
			}
		}
		if (!drawOnlyBox && !dialogueTyping && message == null)
		{
			spriteBatch.Draw(mouseCursors, new Vector2(x + dialogueX + width - 96, (float)(y + height + dialogueY + everythingYOffset - 96) - dialogueButtonScale), getSourceRectForStandardTileSheet(mouseCursors, (!dialogueButtonShrinking && dialogueButtonScale < 8f) ? 3 : 2), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9999999f);
		}
	}

	public static void drawPlayerHeldObject(Farmer f)
	{
		if ((!eventUp || (currentLocation.currentEvent != null && currentLocation.currentEvent.showActiveObject)) && !f.FarmerSprite.PauseForSingleAnimation && !f.isRidingHorse() && !f.bathingClothes && !f.onBridge.Value)
		{
			float xPosition = f.getLocalPosition(viewport).X + (float)((f.rotation < 0f) ? (-8) : ((f.rotation > 0f) ? 8 : 0)) + (float)(f.FarmerSprite.CurrentAnimationFrame.xOffset * 4);
			float objectYLoc = f.getLocalPosition(viewport).Y - 128f + (float)(f.FarmerSprite.CurrentAnimationFrame.positionOffset * 4) + (float)(FarmerRenderer.featureYOffsetPerFrame[f.FarmerSprite.CurrentFrame] * 4);
			if ((bool)f.ActiveObject.bigCraftable)
			{
				objectYLoc -= 64f;
			}
			if (f.isEating)
			{
				xPosition = f.getLocalPosition(viewport).X - 21f;
				objectYLoc = f.getLocalPosition(viewport).Y - 128f + 12f;
			}
			if (!f.isEating || (f.isEating && f.Sprite.currentFrame <= 218))
			{
				f.ActiveObject.drawWhenHeld(spriteBatch, new Vector2((int)xPosition, (int)objectYLoc), f);
			}
		}
	}

	public static void drawTool(Farmer f)
	{
		drawTool(f, f.CurrentTool.CurrentParentTileIndex);
	}

	public static void drawTool(Farmer f, int currentToolIndex)
	{
		Vector2 fPosition = f.getLocalPosition(viewport) + f.jitter + f.armOffset;
		FarmerSprite farmerSprite = (FarmerSprite)f.Sprite;
		if (f.CurrentTool is MeleeWeapon weapon)
		{
			weapon.drawDuringUse(farmerSprite.currentAnimationIndex, f.FacingDirection, spriteBatch, fPosition, f);
			return;
		}
		if (f.FarmerSprite.isUsingWeapon())
		{
			MeleeWeapon.drawDuringUse(farmerSprite.currentAnimationIndex, f.FacingDirection, spriteBatch, fPosition, f, f.FarmerSprite.CurrentToolIndex.ToString(), f.FarmerSprite.getWeaponTypeFromAnimation(), isOnSpecial: false);
			return;
		}
		Tool currentTool = f.CurrentTool;
		if (!(currentTool is Slingshot) && !(currentTool is Shears) && !(currentTool is MilkPail) && !(currentTool is Pan))
		{
			if (!(currentTool is FishingRod) && !(currentTool is WateringCan) && f != player)
			{
				if (farmerSprite.currentSingleAnimation < 160 || farmerSprite.currentSingleAnimation >= 192)
				{
					return;
				}
				if (f.CurrentTool != null)
				{
					f.CurrentTool.Update(f.FacingDirection, 0, f);
					currentToolIndex = f.CurrentTool.CurrentParentTileIndex;
				}
			}
			Texture2D spritesheet = ItemRegistry.GetData(f.CurrentTool?.QualifiedItemId)?.GetTexture() ?? toolSpriteSheet;
			Microsoft.Xna.Framework.Rectangle sourceRectangleForTool = new Microsoft.Xna.Framework.Rectangle(currentToolIndex * 16 % spritesheet.Width, currentToolIndex * 16 / spritesheet.Width * 16, 16, 32);
			float base_layer_depth = f.getDrawLayer();
			if (f.CurrentTool is FishingRod rod)
			{
				if (rod.fishCaught || rod.showingTreasure)
				{
					f.CurrentTool.draw(spriteBatch);
					return;
				}
				sourceRectangleForTool = new Microsoft.Xna.Framework.Rectangle(farmerSprite.currentAnimationIndex * 48, 288, 48, 48);
				if (f.FacingDirection == 2 || f.FacingDirection == 0)
				{
					sourceRectangleForTool.Y += 48;
				}
				else if (rod.isFishing && (!rod.isReeling || rod.hit))
				{
					fPosition.Y += 8f;
				}
				if (rod.isFishing)
				{
					sourceRectangleForTool.X += (5 - farmerSprite.currentAnimationIndex) * 48;
				}
				if (rod.isReeling)
				{
					if (f.FacingDirection == 2 || f.FacingDirection == 0)
					{
						sourceRectangleForTool.X = 288;
						if (f.IsLocalPlayer && didPlayerJustClickAtAll())
						{
							sourceRectangleForTool.X = 0;
						}
					}
					else
					{
						sourceRectangleForTool.X = 288;
						sourceRectangleForTool.Y = 240;
						if (f.IsLocalPlayer && didPlayerJustClickAtAll())
						{
							sourceRectangleForTool.Y += 48;
						}
					}
				}
				if (f.FarmerSprite.CurrentFrame == 57)
				{
					sourceRectangleForTool.Height = 0;
				}
				if (f.FacingDirection == 0)
				{
					fPosition.X += 16f;
				}
			}
			f.CurrentTool?.draw(spriteBatch);
			int toolYOffset = 0;
			int toolXOffset = 0;
			if (f.CurrentTool is WateringCan)
			{
				toolYOffset += 80;
				toolXOffset = ((f.FacingDirection == 1) ? 32 : ((f.FacingDirection == 3) ? (-32) : 0));
				if (farmerSprite.currentAnimationIndex == 0 || farmerSprite.currentAnimationIndex == 1)
				{
					toolXOffset = toolXOffset * 3 / 2;
				}
			}
			toolYOffset += f.yJumpOffset;
			float layerDepth = FarmerRenderer.GetLayerDepth(base_layer_depth, f.FacingDirection switch
			{
				0 => FarmerRenderer.FarmerSpriteLayers.ToolUp, 
				2 => FarmerRenderer.FarmerSpriteLayers.ToolDown, 
				_ => FarmerRenderer.FarmerSpriteLayers.TOOL_IN_USE_SIDE, 
			});
			Color color;
			switch (f.FacingDirection)
			{
			case 1:
				if (farmerSprite.currentAnimationIndex > 2)
				{
					Point tileLocation = f.TilePoint;
					tileLocation.X++;
					tileLocation.Y--;
					if (!(f.CurrentTool is WateringCan) && f.currentLocation.getTileIndexAt(tileLocation, "Front") != -1)
					{
						return;
					}
					tileLocation.Y++;
				}
				currentTool = f.CurrentTool;
				if (!(currentTool is FishingRod rod))
				{
					if (currentTool is WateringCan)
					{
						if (farmerSprite.currentAnimationIndex == 1)
						{
							Point tileLocation = f.TilePoint;
							tileLocation.X--;
							tileLocation.Y--;
							if (f.currentLocation.getTileIndexAt(tileLocation, "Front") != -1 && f.Position.Y % 64f < 32f)
							{
								return;
							}
						}
						switch (farmerSprite.currentAnimationIndex)
						{
						case 0:
						case 1:
							spriteBatch.Draw(spritesheet, new Vector2((int)(fPosition.X + (float)toolXOffset - 4f), (int)(fPosition.Y - 128f + 8f + (float)toolYOffset)), sourceRectangleForTool, Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, layerDepth);
							break;
						case 2:
							spriteBatch.Draw(spritesheet, new Vector2((int)fPosition.X + toolXOffset + 24, (int)(fPosition.Y - 128f - 8f + (float)toolYOffset)), sourceRectangleForTool, Color.White, (float)Math.PI / 12f, new Vector2(0f, 16f), 4f, SpriteEffects.None, layerDepth);
							break;
						case 3:
							sourceRectangleForTool.X += 16;
							spriteBatch.Draw(spritesheet, new Vector2((int)(fPosition.X + (float)toolXOffset + 8f), (int)(fPosition.Y - 128f - 24f + (float)toolYOffset)), sourceRectangleForTool, Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, layerDepth);
							break;
						}
						return;
					}
					switch (farmerSprite.currentAnimationIndex)
					{
					case 0:
						spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 32f - 4f + (float)toolXOffset - (float)Math.Min(8, (int)f.toolPower * 4), fPosition.Y - 128f + 24f + (float)toolYOffset + (float)Math.Min(8, (int)f.toolPower * 4))), sourceRectangleForTool, Color.White, -(float)Math.PI / 12f - (float)Math.Min(f.toolPower, 2) * ((float)Math.PI / 64f), new Vector2(0f, 16f), 4f, SpriteEffects.None, layerDepth);
						break;
					case 1:
						spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + 32f - 24f + (float)toolXOffset, fPosition.Y - 124f + (float)toolYOffset + 64f)), sourceRectangleForTool, Color.White, (float)Math.PI / 12f, new Vector2(0f, 32f), 4f, SpriteEffects.None, layerDepth);
						break;
					case 2:
						spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + 32f + (float)toolXOffset - 4f, fPosition.Y - 132f + (float)toolYOffset + 64f)), sourceRectangleForTool, Color.White, (float)Math.PI / 4f, new Vector2(0f, 32f), 4f, SpriteEffects.None, layerDepth);
						break;
					case 3:
						spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + 32f + 28f + (float)toolXOffset, fPosition.Y - 64f + (float)toolYOffset)), sourceRectangleForTool, Color.White, (float)Math.PI * 7f / 12f, new Vector2(0f, 32f), 4f, SpriteEffects.None, layerDepth);
						break;
					case 4:
						spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + 32f + 28f + (float)toolXOffset, fPosition.Y - 64f + 4f + (float)toolYOffset)), sourceRectangleForTool, Color.White, (float)Math.PI * 7f / 12f, new Vector2(0f, 32f), 4f, SpriteEffects.None, layerDepth);
						break;
					case 5:
						spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + 64f + 12f + (float)toolXOffset, fPosition.Y - 128f + 32f + (float)toolYOffset + 128f)), sourceRectangleForTool, Color.White, (float)Math.PI / 4f, new Vector2(0f, 32f), 4f, SpriteEffects.None, layerDepth);
						break;
					case 6:
						spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + 42f + 8f + (float)toolXOffset, fPosition.Y - 64f + 24f + (float)toolYOffset + 128f)), sourceRectangleForTool, Color.White, 0f, new Vector2(0f, 128f), 4f, SpriteEffects.None, layerDepth);
						break;
					}
					return;
				}
				color = rod.getColor();
				switch (farmerSprite.currentAnimationIndex)
				{
				case 0:
					if (rod.isReeling || rod.isFishing || rod.doneWithAnimation || !rod.hasDoneFucntionYet || rod.pullingOutOfWater)
					{
						spriteBatch.Draw(spritesheet, new Vector2(fPosition.X - 64f + (float)toolXOffset, fPosition.Y - 160f + (float)toolYOffset), sourceRectangleForTool, color, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth);
					}
					break;
				case 1:
					spriteBatch.Draw(spritesheet, new Vector2(fPosition.X - 64f + (float)toolXOffset, fPosition.Y - 160f + 8f + (float)toolYOffset), sourceRectangleForTool, color, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth);
					break;
				case 2:
					spriteBatch.Draw(spritesheet, new Vector2(fPosition.X - 96f + 32f + (float)toolXOffset, fPosition.Y - 128f - 24f + (float)toolYOffset), sourceRectangleForTool, color, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth);
					break;
				case 3:
					spriteBatch.Draw(spritesheet, new Vector2(fPosition.X - 96f + 24f + (float)toolXOffset, fPosition.Y - 128f - 32f + (float)toolYOffset), sourceRectangleForTool, color, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth);
					break;
				case 4:
					if (rod.isFishing || rod.doneWithAnimation)
					{
						spriteBatch.Draw(spritesheet, new Vector2(fPosition.X - 64f + (float)toolXOffset, fPosition.Y - 160f + (float)toolYOffset), sourceRectangleForTool, color, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth);
					}
					else
					{
						spriteBatch.Draw(spritesheet, new Vector2(fPosition.X - 64f + (float)toolXOffset, fPosition.Y - 160f + 4f + (float)toolYOffset), sourceRectangleForTool, color, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth);
					}
					break;
				case 5:
					spriteBatch.Draw(spritesheet, new Vector2(fPosition.X - 64f + (float)toolXOffset, fPosition.Y - 160f + (float)toolYOffset), sourceRectangleForTool, color, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth);
					break;
				}
				return;
			case 3:
				if (farmerSprite.currentAnimationIndex > 2)
				{
					Point tileLocation = f.TilePoint;
					tileLocation.X--;
					tileLocation.Y--;
					if (!(f.CurrentTool is WateringCan) && f.currentLocation.getTileIndexAt(tileLocation, "Front") != -1 && f.Position.Y % 64f < 32f)
					{
						return;
					}
					tileLocation.Y++;
				}
				currentTool = f.CurrentTool;
				if (!(currentTool is FishingRod rod))
				{
					if (currentTool is WateringCan)
					{
						if (farmerSprite.currentAnimationIndex == 1)
						{
							Point tileLocation = f.TilePoint;
							tileLocation.X--;
							tileLocation.Y--;
							if (f.currentLocation.getTileIndexAt(tileLocation, "Front") != -1 && f.Position.Y % 64f < 32f)
							{
								return;
							}
						}
						switch (farmerSprite.currentAnimationIndex)
						{
						case 0:
						case 1:
							spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + (float)toolXOffset - 4f, fPosition.Y - 128f + 8f + (float)toolYOffset)), sourceRectangleForTool, Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.FlipHorizontally, layerDepth);
							break;
						case 2:
							spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + (float)toolXOffset - 16f, fPosition.Y - 128f + (float)toolYOffset)), sourceRectangleForTool, Color.White, -(float)Math.PI / 12f, new Vector2(0f, 16f), 4f, SpriteEffects.FlipHorizontally, layerDepth);
							break;
						case 3:
							sourceRectangleForTool.X += 16;
							spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + (float)toolXOffset - 16f, fPosition.Y - 128f - 24f + (float)toolYOffset)), sourceRectangleForTool, Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.FlipHorizontally, layerDepth);
							break;
						}
					}
					else
					{
						switch (farmerSprite.currentAnimationIndex)
						{
						case 0:
							spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + 32f + 8f + (float)toolXOffset + (float)Math.Min(8, (int)f.toolPower * 4), fPosition.Y - 128f + 8f + (float)toolYOffset + (float)Math.Min(8, (int)f.toolPower * 4))), sourceRectangleForTool, Color.White, (float)Math.PI / 12f + (float)Math.Min(f.toolPower, 2) * ((float)Math.PI / 64f), new Vector2(0f, 16f), 4f, SpriteEffects.FlipHorizontally, layerDepth);
							break;
						case 1:
							spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 16f + (float)toolXOffset, fPosition.Y - 128f + 16f + (float)toolYOffset)), sourceRectangleForTool, Color.White, -(float)Math.PI / 12f, new Vector2(0f, 16f), 4f, SpriteEffects.FlipHorizontally, layerDepth);
							break;
						case 2:
							spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 64f + 4f + (float)toolXOffset, fPosition.Y - 128f + 60f + (float)toolYOffset)), sourceRectangleForTool, Color.White, -(float)Math.PI / 4f, new Vector2(0f, 16f), 4f, SpriteEffects.FlipHorizontally, layerDepth);
							break;
						case 3:
							spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 64f + 20f + (float)toolXOffset, fPosition.Y - 64f + 76f + (float)toolYOffset)), sourceRectangleForTool, Color.White, (float)Math.PI * -7f / 12f, new Vector2(0f, 16f), 4f, SpriteEffects.FlipHorizontally, layerDepth);
							break;
						case 4:
							spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 64f + 24f + (float)toolXOffset, fPosition.Y + 24f + (float)toolYOffset)), sourceRectangleForTool, Color.White, (float)Math.PI * -7f / 12f, new Vector2(0f, 16f), 4f, SpriteEffects.FlipHorizontally, layerDepth);
							break;
						}
					}
					return;
				}
				color = rod.getColor();
				switch (farmerSprite.currentAnimationIndex)
				{
				case 0:
					if (rod.isReeling || rod.isFishing || rod.doneWithAnimation || !rod.hasDoneFucntionYet || rod.pullingOutOfWater)
					{
						spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 64f + (float)toolXOffset, fPosition.Y - 160f + (float)toolYOffset)), sourceRectangleForTool, color, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, layerDepth);
					}
					break;
				case 1:
					spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 64f + (float)toolXOffset, fPosition.Y - 160f + 8f + (float)toolYOffset)), sourceRectangleForTool, color, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, layerDepth);
					break;
				case 2:
					spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 96f + 32f + (float)toolXOffset, fPosition.Y - 128f - 24f + (float)toolYOffset)), sourceRectangleForTool, color, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, layerDepth);
					break;
				case 3:
					spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 96f + 24f + (float)toolXOffset, fPosition.Y - 128f - 32f + (float)toolYOffset)), sourceRectangleForTool, color, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, layerDepth);
					break;
				case 4:
					if (rod.isFishing || rod.doneWithAnimation)
					{
						spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 64f + (float)toolXOffset, fPosition.Y - 160f + (float)toolYOffset)), sourceRectangleForTool, color, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, layerDepth);
					}
					else
					{
						spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 64f + (float)toolXOffset, fPosition.Y - 160f + 4f + (float)toolYOffset)), sourceRectangleForTool, color, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, layerDepth);
					}
					break;
				case 5:
					spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 64f + (float)toolXOffset, fPosition.Y - 160f + (float)toolYOffset)), sourceRectangleForTool, color, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, layerDepth);
					break;
				}
				return;
			}
			if (farmerSprite.currentAnimationIndex > 2 && !(f.CurrentTool is FishingRod { isCasting: false, castedButBobberStillInAir: false, isTimingCast: false }))
			{
				Point tileLocation = f.TilePoint;
				if (f.currentLocation.getTileIndexAt(tileLocation, "Front") != -1 && f.Position.Y % 64f < 32f && f.Position.Y % 64f > 16f)
				{
					return;
				}
			}
			currentTool = f.CurrentTool;
			if (!(currentTool is FishingRod fishingRod))
			{
				if (currentTool is WateringCan)
				{
					switch (farmerSprite.currentAnimationIndex)
					{
					case 0:
					case 1:
						spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + (float)toolXOffset, fPosition.Y - 128f + 16f + (float)toolYOffset)), sourceRectangleForTool, Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, layerDepth);
						break;
					case 2:
						spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + (float)toolXOffset, fPosition.Y - 128f - (float)((f.FacingDirection == 2) ? (-4) : 32) + (float)toolYOffset)), sourceRectangleForTool, Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, layerDepth);
						break;
					case 3:
						if (f.FacingDirection == 2)
						{
							sourceRectangleForTool.X += 16;
						}
						spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + (float)toolXOffset - (float)((f.FacingDirection == 2) ? 4 : 0), fPosition.Y - 128f - (float)((f.FacingDirection == 2) ? (-24) : 64) + (float)toolYOffset)), sourceRectangleForTool, Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, layerDepth);
						break;
					}
					return;
				}
				switch (farmerSprite.currentAnimationIndex)
				{
				case 0:
					if (f.FacingDirection == 0)
					{
						spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + (float)toolXOffset, fPosition.Y - 128f - 8f + (float)toolYOffset + (float)Math.Min(8, (int)f.toolPower * 4))), sourceRectangleForTool, Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, layerDepth);
					}
					else
					{
						spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + (float)toolXOffset - 20f, fPosition.Y - 128f + 12f + (float)toolYOffset + (float)Math.Min(8, (int)f.toolPower * 4))), sourceRectangleForTool, Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, layerDepth);
					}
					break;
				case 1:
					if (f.FacingDirection == 0)
					{
						spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + (float)toolXOffset + 4f, fPosition.Y - 128f + 40f + (float)toolYOffset)), sourceRectangleForTool, Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, layerDepth);
					}
					else
					{
						spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + (float)toolXOffset - 12f, fPosition.Y - 128f + 32f + (float)toolYOffset)), sourceRectangleForTool, Color.White, -(float)Math.PI / 24f, new Vector2(0f, 16f), 4f, SpriteEffects.None, layerDepth);
					}
					break;
				case 2:
					spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + (float)toolXOffset, fPosition.Y - 128f + 64f + (float)toolYOffset)), sourceRectangleForTool, Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, layerDepth);
					break;
				case 3:
					if (f.FacingDirection != 0)
					{
						spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + (float)toolXOffset, fPosition.Y - 64f + 44f + (float)toolYOffset)), sourceRectangleForTool, Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, layerDepth);
					}
					break;
				case 4:
					if (f.FacingDirection != 0)
					{
						spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + (float)toolXOffset, fPosition.Y - 64f + 48f + (float)toolYOffset)), sourceRectangleForTool, Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, layerDepth);
					}
					break;
				case 5:
					spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + (float)toolXOffset, fPosition.Y - 64f + 32f + (float)toolYOffset)), sourceRectangleForTool, Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, layerDepth);
					break;
				}
				return;
			}
			if (farmerSprite.currentAnimationIndex <= 2)
			{
				Point tileLocation = f.TilePoint;
				tileLocation.Y--;
				if (f.currentLocation.getTileIndexAt(tileLocation, "Front") != -1)
				{
					return;
				}
			}
			if (f.FacingDirection == 2)
			{
				layerDepth += 0.01f;
			}
			color = fishingRod.getColor();
			switch (farmerSprite.currentAnimationIndex)
			{
			case 0:
				if (!fishingRod.showingTreasure && !fishingRod.fishCaught && (f.FacingDirection != 0 || !fishingRod.isFishing || fishingRod.isReeling))
				{
					spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 64f, fPosition.Y - 128f + 4f)), sourceRectangleForTool, color, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth);
				}
				break;
			case 1:
				spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 64f, fPosition.Y - 128f + 4f)), sourceRectangleForTool, color, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth);
				break;
			case 2:
				spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 64f, fPosition.Y - 128f + 4f)), sourceRectangleForTool, color, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth);
				break;
			case 3:
				if (f.FacingDirection == 2)
				{
					spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 64f, fPosition.Y - 128f + 4f)), sourceRectangleForTool, color, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth);
				}
				break;
			case 4:
				if (f.FacingDirection == 0 && fishingRod.isFishing)
				{
					spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 80f, fPosition.Y - 96f)), sourceRectangleForTool, color, 0f, Vector2.Zero, 4f, SpriteEffects.FlipVertically, layerDepth);
				}
				else if (f.FacingDirection == 2)
				{
					spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 64f, fPosition.Y - 128f + 4f)), sourceRectangleForTool, color, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth);
				}
				break;
			case 5:
				if (f.FacingDirection == 2 && !fishingRod.showingTreasure && !fishingRod.fishCaught)
				{
					spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 64f, fPosition.Y - 128f + 4f)), sourceRectangleForTool, color, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth);
				}
				break;
			}
		}
		else
		{
			f.CurrentTool.draw(spriteBatch);
		}
	}

	/// ####################
	/// OTHER HELPER METHODS
	/// ####################
	public static Vector2 GlobalToLocal(xTile.Dimensions.Rectangle viewport, Vector2 globalPosition)
	{
		return new Vector2(globalPosition.X - (float)viewport.X, globalPosition.Y - (float)viewport.Y);
	}

	public static bool IsEnglish()
	{
		return content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.en;
	}

	public static Vector2 GlobalToLocal(Vector2 globalPosition)
	{
		return new Vector2(globalPosition.X - (float)viewport.X, globalPosition.Y - (float)viewport.Y);
	}

	public static Microsoft.Xna.Framework.Rectangle GlobalToLocal(xTile.Dimensions.Rectangle viewport, Microsoft.Xna.Framework.Rectangle globalPosition)
	{
		return new Microsoft.Xna.Framework.Rectangle(globalPosition.X - viewport.X, globalPosition.Y - viewport.Y, globalPosition.Width, globalPosition.Height);
	}

	public static string parseText(string text, SpriteFont whichFont, int width)
	{
		if (text == null)
		{
			return "";
		}
		text = Dialogue.applyGenderSwitchBlocks(player.Gender, text);
		_ParseTextStringBuilder.Clear();
		_ParseTextStringBuilderLine.Clear();
		_ParseTextStringBuilderWord.Clear();
		float current_width = 0f;
		LocalizedContentManager.LanguageCode currentLanguageCode = LocalizedContentManager.CurrentLanguageCode;
		if (currentLanguageCode == LocalizedContentManager.LanguageCode.ja || currentLanguageCode == LocalizedContentManager.LanguageCode.zh || currentLanguageCode == LocalizedContentManager.LanguageCode.th)
		{
			foreach (object item in asianSpacingRegex.Matches(text))
			{
				string s = item.ToString();
				float character_width = whichFont.MeasureString(s).X + whichFont.Spacing;
				if (current_width + character_width > (float)width || s.Equals(Environment.NewLine) || s.Equals("\n"))
				{
					_ParseTextStringBuilder.Append(_ParseTextStringBuilderLine);
					_ParseTextStringBuilder.Append(Environment.NewLine);
					_ParseTextStringBuilderLine.Clear();
					current_width = 0f;
				}
				if (!s.Equals(Environment.NewLine) && !s.Equals("\n"))
				{
					_ParseTextStringBuilderLine.Append(s);
					current_width += character_width;
				}
			}
			_ParseTextStringBuilder.Append(_ParseTextStringBuilderLine);
			return _ParseTextStringBuilder.ToString();
		}
		current_width = 0f;
		for (int i = 0; i < text.Length; i++)
		{
			char c = text[i];
			bool check_width;
			if (c != '\n')
			{
				if (c == '\r')
				{
					continue;
				}
				if (c == ' ')
				{
					check_width = true;
				}
				else
				{
					_ParseTextStringBuilderWord.Append(c);
					check_width = i == text.Length - 1;
				}
			}
			else
			{
				check_width = true;
			}
			if (!check_width)
			{
				continue;
			}
			try
			{
				float word_width = whichFont.MeasureString(_ParseTextStringBuilderWord).X + whichFont.Spacing;
				if (current_width + word_width > (float)width)
				{
					_ParseTextStringBuilder.Append(_ParseTextStringBuilderLine);
					_ParseTextStringBuilder.Append(Environment.NewLine);
					_ParseTextStringBuilderLine.Clear();
					current_width = 0f;
				}
				if (c == '\n')
				{
					_ParseTextStringBuilderLine.Append(_ParseTextStringBuilderWord);
					_ParseTextStringBuilder.Append(_ParseTextStringBuilderLine);
					_ParseTextStringBuilder.Append(Environment.NewLine);
					_ParseTextStringBuilderLine.Clear();
					_ParseTextStringBuilderWord.Clear();
					current_width = 0f;
					continue;
				}
				_ParseTextStringBuilderLine.Append(_ParseTextStringBuilderWord);
				_ParseTextStringBuilderLine.Append(" ");
				float space_width = whichFont.MeasureString(" ").X + whichFont.Spacing;
				current_width += word_width + space_width;
			}
			catch (Exception e)
			{
				log.Error("Exception measuring string: ", e);
			}
			_ParseTextStringBuilderWord.Clear();
		}
		_ParseTextStringBuilderLine.Append(_ParseTextStringBuilderWord);
		_ParseTextStringBuilder.Append(_ParseTextStringBuilderLine);
		return _ParseTextStringBuilder.ToString();
	}

	public static void UpdateHorseOwnership()
	{
		bool verbose = false;
		Dictionary<long, Horse> horse_lookup = new Dictionary<long, Horse>();
		HashSet<Horse> claimed_horses = new HashSet<Horse>();
		List<Stable> stables = new List<Stable>();
		Utility.ForEachBuilding(delegate(Stable stable)
		{
			stables.Add(stable);
			return true;
		});
		foreach (Stable stable in stables)
		{
			if (stable.owner.Value == -6666666 && getFarmerMaybeOffline(-6666666L) == null)
			{
				stable.owner.Value = player.UniqueMultiplayerID;
			}
			stable.grabHorse();
		}
		foreach (Stable item in stables)
		{
			Horse horse = item.getStableHorse();
			if (horse != null && !claimed_horses.Contains(horse) && horse.getOwner() != null && !horse_lookup.ContainsKey(horse.getOwner().UniqueMultiplayerID) && horse.getOwner().horseName.Value != null && horse.getOwner().horseName.Value.Length > 0 && horse.Name == horse.getOwner().horseName.Value)
			{
				horse_lookup[horse.getOwner().UniqueMultiplayerID] = horse;
				claimed_horses.Add(horse);
				if (verbose)
				{
					log.Verbose("Assigned horse " + horse.Name + " to " + horse.getOwner().Name + " (Exact match)");
				}
			}
		}
		Dictionary<string, Farmer> horse_name_lookup = new Dictionary<string, Farmer>();
		foreach (Farmer farmer in getAllFarmers())
		{
			if (string.IsNullOrEmpty(farmer?.horseName.Value))
			{
				continue;
			}
			bool fail = false;
			foreach (Horse item2 in claimed_horses)
			{
				if (item2.getOwner() == farmer)
				{
					fail = true;
					break;
				}
			}
			if (!fail)
			{
				horse_name_lookup[farmer.horseName] = farmer;
			}
		}
		foreach (Stable stable in stables)
		{
			Horse horse = stable.getStableHorse();
			if (horse != null && !claimed_horses.Contains(horse) && horse.getOwner() != null && horse.Name != null && horse.Name.Length > 0 && horse_name_lookup.TryGetValue(horse.Name, out var owner) && !horse_lookup.ContainsKey(owner.UniqueMultiplayerID))
			{
				stable.owner.Value = owner.UniqueMultiplayerID;
				stable.updateHorseOwnership();
				horse_lookup[horse.getOwner().UniqueMultiplayerID] = horse;
				claimed_horses.Add(horse);
				if (verbose)
				{
					log.Verbose("Assigned horse " + horse.Name + " to " + horse.getOwner().Name + " (Name match from different owner.)");
				}
			}
		}
		foreach (Stable stable in stables)
		{
			Horse horse = stable.getStableHorse();
			if (horse != null && !claimed_horses.Contains(horse) && horse.getOwner() != null && !horse_lookup.ContainsKey(horse.getOwner().UniqueMultiplayerID))
			{
				horse_lookup[horse.getOwner().UniqueMultiplayerID] = horse;
				claimed_horses.Add(horse);
				stable.updateHorseOwnership();
				if (verbose)
				{
					log.Verbose("Assigned horse " + horse.Name + " to " + horse.getOwner().Name + " (Owner's only stable)");
				}
			}
		}
		foreach (Stable stable in stables)
		{
			Horse horse = stable.getStableHorse();
			if (horse == null || claimed_horses.Contains(horse))
			{
				continue;
			}
			foreach (Horse claimed_horse in claimed_horses)
			{
				if (horse.ownerId == claimed_horse.ownerId)
				{
					stable.owner.Value = 0L;
					stable.updateHorseOwnership();
					if (verbose)
					{
						log.Verbose("Unassigned horse (stable owner already has a horse).");
					}
					break;
				}
			}
		}
	}

	public static string LoadStringByGender(Gender npcGender, string key)
	{
		if (npcGender == Gender.Male)
		{
			return content.LoadString(key).Split('/')[0];
		}
		return content.LoadString(key).Split('/').Last();
	}

	public static string LoadStringByGender(Gender npcGender, string key, params object[] substitutions)
	{
		string sentence;
		if (npcGender == Gender.Male)
		{
			sentence = content.LoadString(key).Split('/')[0];
			if (substitutions.Length != 0)
			{
				try
				{
					return string.Format(sentence, substitutions);
				}
				catch
				{
					return sentence;
				}
			}
		}
		sentence = content.LoadString(key).Split('/').Last();
		if (substitutions.Length != 0)
		{
			try
			{
				return string.Format(sentence, substitutions);
			}
			catch
			{
				return sentence;
			}
		}
		return sentence;
	}

	public static string parseText(string text)
	{
		return parseText(text, dialogueFont, dialogueWidth);
	}

	public static Microsoft.Xna.Framework.Rectangle getSourceRectForStandardTileSheet(Texture2D tileSheet, int tilePosition, int width = -1, int height = -1)
	{
		if (width == -1)
		{
			width = 64;
		}
		if (height == -1)
		{
			height = 64;
		}
		return new Microsoft.Xna.Framework.Rectangle(tilePosition * width % tileSheet.Width, tilePosition * width / tileSheet.Width * height, width, height);
	}

	public static Microsoft.Xna.Framework.Rectangle getSquareSourceRectForNonStandardTileSheet(Texture2D tileSheet, int tileWidth, int tileHeight, int tilePosition)
	{
		return new Microsoft.Xna.Framework.Rectangle(tilePosition * tileWidth % tileSheet.Width, tilePosition * tileWidth / tileSheet.Width * tileHeight, tileWidth, tileHeight);
	}

	public static Microsoft.Xna.Framework.Rectangle getArbitrarySourceRect(Texture2D tileSheet, int tileWidth, int tileHeight, int tilePosition)
	{
		if (tileSheet != null)
		{
			return new Microsoft.Xna.Framework.Rectangle(tilePosition * tileWidth % tileSheet.Width, tilePosition * tileWidth / tileSheet.Width * tileHeight, tileWidth, tileHeight);
		}
		return Microsoft.Xna.Framework.Rectangle.Empty;
	}

	public static string getTimeOfDayString(int time)
	{
		string zeroPad = ((time % 100 == 0) ? "0" : string.Empty);
		string hours;
		switch (LocalizedContentManager.CurrentLanguageCode)
		{
		default:
			hours = ((time / 100 % 12 == 0) ? "12" : (time / 100 % 12).ToString());
			break;
		case LocalizedContentManager.LanguageCode.ja:
			hours = ((time / 100 % 12 == 0) ? "0" : (time / 100 % 12).ToString());
			break;
		case LocalizedContentManager.LanguageCode.zh:
			hours = (time / 100 % 24).ToString();
			break;
		case LocalizedContentManager.LanguageCode.ru:
		case LocalizedContentManager.LanguageCode.pt:
		case LocalizedContentManager.LanguageCode.es:
		case LocalizedContentManager.LanguageCode.de:
		case LocalizedContentManager.LanguageCode.th:
		case LocalizedContentManager.LanguageCode.fr:
		case LocalizedContentManager.LanguageCode.tr:
		case LocalizedContentManager.LanguageCode.hu:
			hours = (time / 100 % 24).ToString();
			hours = ((time / 100 % 24 <= 9) ? ("0" + hours) : hours);
			break;
		}
		string timeText = hours + ":" + time % 100 + zeroPad;
		switch (LocalizedContentManager.CurrentLanguageCode)
		{
		case LocalizedContentManager.LanguageCode.en:
			return timeText + " " + ((time < 1200 || time >= 2400) ? content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10370") : content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10371"));
		case LocalizedContentManager.LanguageCode.ja:
			if (time >= 1200 && time < 2400)
			{
				return content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10371") + " " + timeText;
			}
			return content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10370") + " " + timeText;
		case LocalizedContentManager.LanguageCode.fr:
			if (time % 100 != 0)
			{
				return hours + "h" + time % 100;
			}
			return hours + "h";
		case LocalizedContentManager.LanguageCode.mod:
			return LocalizedContentManager.FormatTimeString(time, LocalizedContentManager.CurrentModLanguage.TimeFormat).ToString();
		default:
			return timeText;
		}
	}

	public static bool[,] getCircleOutlineGrid(int radius)
	{
		bool[,] circleGrid = new bool[radius * 2 + 1, radius * 2 + 1];
		int f = 1 - radius;
		int ddF_x = 1;
		int ddF_y = -2 * radius;
		int x = 0;
		int y = radius;
		circleGrid[radius, radius + radius] = true;
		circleGrid[radius, radius - radius] = true;
		circleGrid[radius + radius, radius] = true;
		circleGrid[radius - radius, radius] = true;
		while (x < y)
		{
			if (f >= 0)
			{
				y--;
				ddF_y += 2;
				f += ddF_y;
			}
			x++;
			ddF_x += 2;
			f += ddF_x;
			circleGrid[radius + x, radius + y] = true;
			circleGrid[radius - x, radius + y] = true;
			circleGrid[radius + x, radius - y] = true;
			circleGrid[radius - x, radius - y] = true;
			circleGrid[radius + y, radius + x] = true;
			circleGrid[radius - y, radius + x] = true;
			circleGrid[radius + y, radius - x] = true;
			circleGrid[radius - y, radius - x] = true;
		}
		return circleGrid;
	}

	/// <summary>Get the internal identifier for the current farm type. This is either the numeric index for a vanilla farm, or the <see cref="F:StardewValley.GameData.ModFarmType.Id" /> field for a custom type.</summary>
	public static string GetFarmTypeID()
	{
		if (whichFarm != 7 || whichModFarm == null)
		{
			return whichFarm.ToString();
		}
		return whichModFarm.Id;
	}

	/// <summary>Get the human-readable identifier for the current farm type. For a custom farm type, this is equivalent to <see cref="M:StardewValley.Game1.GetFarmTypeID" />.</summary>
	public static string GetFarmTypeKey()
	{
		return whichFarm switch
		{
			0 => "Standard", 
			1 => "Riverland", 
			2 => "Forest", 
			3 => "Hilltop", 
			4 => "Wilderness", 
			5 => "FourCorners", 
			6 => "Beach", 
			_ => GetFarmTypeID(), 
		};
	}

	public void _PerformRemoveNormalItemFromWorldOvernight(string itemId)
	{
		Utility.ForEachLocation(delegate(GameLocation location)
		{
			_RecursiveRemoveThisNormalItemLocation(location, itemId);
			return true;
		}, includeInteriors: true, includeGenerated: true);
		for (int i = 0; i < player.team.returnedDonations.Count; i++)
		{
			if (_RecursiveRemoveThisNormalItemItem(player.team.returnedDonations[i], itemId))
			{
				player.team.returnedDonations.RemoveAt(i);
				i--;
			}
		}
		foreach (Inventory inventory in player.team.globalInventories.Values)
		{
			for (int i = 0; i < ((ICollection<Item>)inventory).Count; i++)
			{
				if (_RecursiveRemoveThisNormalItemItem(((IList<Item>)inventory)[i], itemId))
				{
					((IList<Item>)inventory).RemoveAt(i);
					i--;
				}
			}
		}
		foreach (SpecialOrder order in player.team.specialOrders)
		{
			for (int i = 0; i < order.donatedItems.Count; i++)
			{
				Item item = order.donatedItems[i];
				if (_RecursiveRemoveThisNormalItemItem(item, itemId))
				{
					order.donatedItems[i] = null;
				}
			}
		}
	}

	protected virtual void _PerformRemoveNormalItemFromFarmerOvernight(Farmer farmer, string itemId)
	{
		for (int i = 0; i < farmer.Items.Count; i++)
		{
			if (_RecursiveRemoveThisNormalItemItem(farmer.Items[i], itemId))
			{
				farmer.Items[i] = null;
			}
		}
		for (int i = 0; i < farmer.itemsLostLastDeath.Count; i++)
		{
			if (_RecursiveRemoveThisNormalItemItem(farmer.itemsLostLastDeath[i], itemId))
			{
				farmer.itemsLostLastDeath.RemoveAt(i);
				i--;
			}
		}
		if (farmer.recoveredItem != null && _RecursiveRemoveThisNormalItemItem(farmer.recoveredItem, itemId))
		{
			farmer.recoveredItem = null;
			farmer.mailbox.Remove("MarlonRecovery");
			farmer.mailForTomorrow.Remove("MarlonRecovery");
		}
		if (farmer.toolBeingUpgraded.Value != null && _RecursiveRemoveThisNormalItemItem(farmer.toolBeingUpgraded.Value, itemId))
		{
			farmer.toolBeingUpgraded.Value = null;
		}
	}

	protected virtual bool _RecursiveRemoveThisNormalItemItem(Item this_item, string itemId)
	{
		if (this_item != null)
		{
			if (this_item is Object o)
			{
				if (o.heldObject.Value != null && _RecursiveRemoveThisNormalItemItem(o.heldObject.Value, itemId))
				{
					o.ResetParentSheetIndex();
					o.heldObject.Value = null;
					o.readyForHarvest.Value = false;
					o.showNextIndex.Value = false;
				}
				if (!(o is StorageFurniture furniture))
				{
					if (!(o is IndoorPot pot))
					{
						if (o is Chest chest)
						{
							bool removed_item = false;
							IInventory items = chest.Items;
							for (int i = 0; i < items.Count; i++)
							{
								Item item = items[i];
								if (item != null && _RecursiveRemoveThisNormalItemItem(item, itemId))
								{
									items[i] = null;
									removed_item = true;
								}
							}
							if (removed_item)
							{
								chest.clearNulls();
							}
						}
					}
					else if (pot.hoeDirt.Value != null)
					{
						_RecursiveRemoveThisNormalItemDirt(pot.hoeDirt.Value, null, Vector2.Zero, itemId);
					}
				}
				else
				{
					bool removed_item = false;
					for (int i = 0; i < furniture.heldItems.Count; i++)
					{
						Item item = furniture.heldItems[i];
						if (item != null && _RecursiveRemoveThisNormalItemItem(item, itemId))
						{
							furniture.heldItems[i] = null;
							removed_item = true;
						}
					}
					if (removed_item)
					{
						furniture.ClearNulls();
					}
				}
				if (o.heldObject.Value != null && _RecursiveRemoveThisNormalItemItem(o.heldObject.Value, itemId))
				{
					o.heldObject.Value = null;
				}
			}
			return Utility.IsNormalObjectAtParentSheetIndex(this_item, itemId);
		}
		return false;
	}

	protected virtual void _RecursiveRemoveThisNormalItemDirt(HoeDirt dirt, GameLocation location, Vector2 coord, string itemId)
	{
		if (dirt.crop != null && dirt.crop.indexOfHarvest.Value == itemId)
		{
			dirt.destroyCrop(showAnimation: false);
		}
	}

	protected virtual void _RecursiveRemoveThisNormalItemLocation(GameLocation l, string itemId)
	{
		if (l == null)
		{
			return;
		}
		List<Guid> removed_items = new List<Guid>();
		foreach (Furniture furniture in l.furniture)
		{
			if (_RecursiveRemoveThisNormalItemItem(furniture, itemId))
			{
				removed_items.Add(l.furniture.GuidOf(furniture));
			}
		}
		foreach (Guid guid in removed_items)
		{
			l.furniture.Remove(guid);
		}
		foreach (NPC character in l.characters)
		{
			if (!(character is Monster monster))
			{
				continue;
			}
			NetStringList objectsToDrop = monster.objectsToDrop;
			if (objectsToDrop == null || objectsToDrop.Count <= 0)
			{
				continue;
			}
			for (int i = monster.objectsToDrop.Count - 1; i >= 0; i--)
			{
				if (monster.objectsToDrop[i] == itemId)
				{
					monster.objectsToDrop.RemoveAt(i);
				}
			}
		}
		Chest fridge = l.GetFridge(onlyUnlocked: false);
		if (fridge != null)
		{
			IInventory fridgeItems = fridge.Items;
			for (int i = 0; i < fridgeItems.Count; i++)
			{
				Item item = fridgeItems[i];
				if (item != null && _RecursiveRemoveThisNormalItemItem(item, itemId))
				{
					fridgeItems[i] = null;
				}
			}
		}
		foreach (Vector2 coord in l.terrainFeatures.Keys)
		{
			if (l.terrainFeatures[coord] is HoeDirt dirt)
			{
				_RecursiveRemoveThisNormalItemDirt(dirt, l, coord, itemId);
			}
		}
		foreach (Building building in l.buildings)
		{
			foreach (Chest chest in building.buildingChests)
			{
				bool anyRemoved = false;
				for (int i = 0; i < chest.Items.Count; i++)
				{
					Item item = chest.Items[i];
					if (item != null && _RecursiveRemoveThisNormalItemItem(item, itemId))
					{
						chest.Items[i] = null;
						anyRemoved = true;
					}
				}
				if (anyRemoved)
				{
					chest.clearNulls();
				}
			}
		}
		Vector2[] array = l.objects.Keys.ToArray();
		foreach (Vector2 key in array)
		{
			Object obj = l.objects[key];
			if (obj != fridge && _RecursiveRemoveThisNormalItemItem(obj, itemId))
			{
				l.objects.Remove(key);
			}
		}
		for (int i = 0; i < l.debris.Count; i++)
		{
			Debris d = l.debris[i];
			if (d.item != null && _RecursiveRemoveThisNormalItemItem(d.item, itemId))
			{
				l.debris.RemoveAt(i);
				i--;
			}
		}
		if (l is ShopLocation shopLocation)
		{
			shopLocation.itemsFromPlayerToSell.RemoveWhere((Item item) => _RecursiveRemoveThisNormalItemItem(item, itemId));
			shopLocation.itemsToStartSellingTomorrow.RemoveWhere((Item item) => _RecursiveRemoveThisNormalItemItem(item, itemId));
		}
	}

	public static bool GetHasRoomAnotherFarm()
	{
		return true;
	}

	public virtual void CleanupReturningToTitle()
	{
		if (!game1.IsMainInstance)
		{
			GameRunner.instance.RemoveGameInstance(this);
		}
		else
		{
			foreach (Game1 instance in GameRunner.instance.gameInstances)
			{
				if (instance != this)
				{
					GameRunner.instance.RemoveGameInstance(instance);
				}
			}
		}
		LocalizedContentManager.localizedAssetNames.Clear();
		Event.invalidFestivals.Clear();
		NPC.invalidDialogueFiles.Clear();
		SaveGame.CancelToTitle = false;
		overlayMenu = null;
		multiplayer.cachedMultiplayerMaps.Clear();
		keyboardFocusInstance = null;
		multiplayer.Disconnect(Multiplayer.DisconnectType.ExitedToMainMenu);
		BuildingPaintMenu.savedColors = null;
		startingGameSeed = null;
		UseLegacyRandom = false;
		_afterNewDayAction = null;
		_currentMinigame = null;
		gameMode = 0;
		_isSaving = false;
		_mouseCursorTransparency = 1f;
		_newDayTask = null;
		newDaySync.destroy();
		netReady.Reset();
		resetPlayer();
		serverHost = null;
		afterDialogues = null;
		afterFade = null;
		afterPause = null;
		afterViewport = null;
		ambientLight = new Color(0, 0, 0, 0);
		background = null;
		chatBox = null;
		specialCurrencyDisplay?.Cleanup();
		GameLocation.PlayedNewLocationContextMusic = false;
		IsPlayingBackgroundMusic = false;
		IsPlayingNightAmbience = false;
		IsPlayingOutdoorsAmbience = false;
		IsPlayingMorningSong = false;
		IsPlayingTownMusic = false;
		specialCurrencyDisplay = null;
		client = null;
		conventionMode = false;
		currentCursorTile = Vector2.Zero;
		currentDialogueCharacterIndex = 0;
		currentLightSources.Clear();
		currentLoader = null;
		currentLocation = null;
		_PreviousNonNullLocation = null;
		currentObjectDialogue.Clear();
		currentQuestionChoice = 0;
		season = Season.Spring;
		currentSpeaker = null;
		currentViewportTarget = Vector2.Zero;
		cursorTileHintCheckTimer = 0;
		CustomData = new SerializableDictionary<string, string>();
		player.team.sharedDailyLuck.Value = 0.001;
		dayOfMonth = 0;
		debrisSoundInterval = 0f;
		debrisWeather.Clear();
		debugMode = false;
		debugOutput = null;
		debugPresenceString = "In menus";
		delayedActions.Clear();
		morningSongPlayAction = null;
		dialogueButtonScale = 1f;
		dialogueButtonShrinking = false;
		dialogueTyping = false;
		dialogueTypingInterval = 0;
		dialogueUp = false;
		dialogueWidth = 1024;
		displayFarmer = true;
		displayHUD = true;
		downPolling = 0f;
		drawGrid = false;
		drawLighting = false;
		elliottBookName = "Blue Tower";
		endOfNightMenus.Clear();
		errorMessage = "";
		eveningColor = new Color(255, 255, 0, 255);
		eventOver = false;
		eventUp = false;
		exitToTitle = false;
		facingDirectionAfterWarp = 0;
		fadeIn = true;
		fadeToBlack = false;
		fadeToBlackAlpha = 1.02f;
		farmEvent = null;
		flashAlpha = 0f;
		freezeControls = false;
		gamePadAButtonPolling = 0;
		gameTimeInterval = 0;
		globalFade = false;
		globalFadeSpeed = 0f;
		haltAfterCheck = false;
		hasLoadedGame = false;
		hasStartedDay = false;
		hitShakeTimer = 0;
		hudMessages.Clear();
		isActionAtCurrentCursorTile = false;
		isDebrisWeather = false;
		isInspectionAtCurrentCursorTile = false;
		isLightning = false;
		isQuestion = false;
		isRaining = false;
		wasGreenRain = false;
		isSnowing = false;
		killScreen = false;
		lastCursorMotionWasMouse = true;
		lastCursorTile = Vector2.Zero;
		lastMousePositionBeforeFade = Point.Zero;
		leftPolling = 0f;
		loadingMessage = "";
		locationRequest = null;
		warpingForForcedRemoteEvent = false;
		locations.Clear();
		mailbox.Clear();
		mapDisplayDevice = new XnaDisplayDevice(content, base.GraphicsDevice);
		messageAfterPause = "";
		messagePause = false;
		mouseClickPolling = 0;
		mouseCursor = cursor_default;
		multiplayerMode = 0;
		netWorldState = new NetRoot<NetWorldState>(new NetWorldState());
		newDay = false;
		nonWarpFade = false;
		noteBlockTimer = 0f;
		npcDialogues = null;
		objectDialoguePortraitPerson = null;
		hasApplied1_3_UpdateChanges = false;
		hasApplied1_4_UpdateChanges = false;
		remoteEventQueue.Clear();
		bannedUsers?.Clear();
		nextClickableMenu.Clear();
		actionsWhenPlayerFree.Clear();
		onScreenMenus.Clear();
		onScreenMenus.Add(new Toolbar());
		dayTimeMoneyBox = new DayTimeMoneyBox();
		onScreenMenus.Add(dayTimeMoneyBox);
		buffsDisplay = new BuffsDisplay();
		onScreenMenus.Add(buffsDisplay);
		bool gamepad_controls = options.gamepadControls;
		bool snappy_menus = options.snappyMenus;
		options = new Options();
		options.gamepadControls = gamepad_controls;
		options.snappyMenus = snappy_menus;
		foreach (KeyValuePair<long, Farmer> otherFarmer in otherFarmers)
		{
			otherFarmer.Value.unload();
		}
		otherFarmers.Clear();
		outdoorLight = new Color(255, 255, 0, 255);
		overlayMenu = null;
		panFacingDirectionWait = false;
		panMode = false;
		panModeString = null;
		pauseAccumulator = 0f;
		paused = false;
		pauseThenDoFunctionTimer = 0;
		pauseTime = 0f;
		previousViewportPosition = Vector2.Zero;
		questionChoices.Clear();
		quit = false;
		rightClickPolling = 0;
		rightPolling = 0f;
		runThreshold = 0.5f;
		samBandName = "The Alfalfas";
		saveOnNewDay = true;
		startingCabins = 0;
		cabinsSeparate = false;
		screenGlow = false;
		screenGlowAlpha = 0f;
		screenGlowColor = new Color(0, 0, 0, 0);
		screenGlowHold = false;
		screenGlowMax = 0f;
		screenGlowRate = 0.005f;
		screenGlowUp = false;
		screenOverlayTempSprites.Clear();
		uiOverlayTempSprites.Clear();
		server = null;
		newGameSetupOptions.Clear();
		showingEndOfNightStuff = false;
		spawnMonstersAtNight = false;
		staminaShakeTimer = 0;
		textColor = new Color(34, 17, 34, 255);
		textShadowColor = new Color(206, 156, 95, 255);
		thumbstickMotionAccell = 1f;
		thumbstickMotionMargin = 0;
		thumbstickPollingTimer = 0;
		thumbStickSensitivity = 0.1f;
		timeOfDay = 600;
		timeOfDayAfterFade = -1;
		timerUntilMouseFade = 0;
		toggleFullScreen = false;
		ResetToolSpriteSheet();
		triggerPolling = 0;
		uniqueIDForThisGame = (ulong)(DateTime.UtcNow - new DateTime(2012, 6, 22)).TotalSeconds;
		upPolling = 0f;
		viewportFreeze = false;
		viewportHold = 0;
		viewportPositionLerp = Vector2.Zero;
		viewportReachedTarget = null;
		viewportSpeed = 2f;
		viewportTarget = new Vector2(-2.1474836E+09f, -2.1474836E+09f);
		wasMouseVisibleThisFrame = true;
		wasRainingYesterday = false;
		weatherForTomorrow = "Sun";
		elliottPiano = 0;
		weatherIcon = 0;
		weddingToday = false;
		whereIsTodaysFest = null;
		worldStateIDs.Clear();
		whichFarm = 0;
		whichModFarm = null;
		windGust = 0f;
		xLocationAfterWarp = 0;
		game1.xTileContent.Dispose();
		game1.xTileContent = CreateContentManager(content.ServiceProvider, content.RootDirectory);
		year = 1;
		yLocationAfterWarp = 0;
		mailDeliveredFromMailForTomorrow.Clear();
		bundleType = BundleType.Default;
		JojaMart.Morris = null;
		AmbientLocationSounds.onLocationLeave();
		WeatherDebris.globalWind = -0.25f;
		Utility.killAllStaticLoopingSoundCues();
		TitleMenu.subMenu = null;
		OptionsDropDown.selected = null;
		JunimoNoteMenu.tempSprites.Clear();
		JunimoNoteMenu.screenSwipe = null;
		JunimoNoteMenu.canClick = true;
		GameMenu.forcePreventClose = false;
		Club.timesPlayedCalicoJack = 0;
		MineShaft.activeMines.Clear();
		MineShaft.permanentMineChanges.Clear();
		MineShaft.numberOfCraftedStairsUsedThisRun = 0;
		MineShaft.mushroomLevelsGeneratedToday.Clear();
		VolcanoDungeon.activeLevels.Clear();
		ItemRegistry.ResetCache();
		Rumble.stopRumbling();
		game1.refreshWindowSettings();
		if (activeClickableMenu is TitleMenu titleMenu)
		{
			titleMenu.applyPreferences();
			activeClickableMenu.gameWindowSizeChanged(graphics.GraphicsDevice.Viewport.Bounds, graphics.GraphicsDevice.Viewport.Bounds);
		}
	}

	public bool CanTakeScreenshots()
	{
		return true;
	}

	/// <summary>Get the absolute path to the folder containing screenshots.</summary>
	/// <param name="createIfMissing">Whether to create the folder if it doesn't exist already.</param>
	public string GetScreenshotFolder(bool createIfMissing = true)
	{
		return Program.GetLocalAppDataFolder("Screenshots", createIfMissing);
	}

	public bool CanBrowseScreenshots()
	{
		return Directory.Exists(GetScreenshotFolder(createIfMissing: false));
	}

	public bool CanZoomScreenshots()
	{
		return true;
	}

	public void BrowseScreenshots()
	{
		string folderPath = GetScreenshotFolder(createIfMissing: false);
		if (Directory.Exists(folderPath))
		{
			try
			{
				Process.Start(new ProcessStartInfo
				{
					FileName = folderPath,
					UseShellExecute = true,
					Verb = "open"
				});
			}
			catch (Exception e)
			{
				log.Error("Failed to open screenshot folder.", e);
			}
		}
	}

	public unsafe string takeMapScreenshot(float? in_scale, string screenshot_name, Action onDone)
	{
		float scale = in_scale.Value;
		if (screenshot_name == null || screenshot_name.Trim() == "")
		{
			DateTime now = DateTime.UtcNow;
			screenshot_name = SaveGame.FilterFileName(player.name) + "_" + now.Month + "-" + now.Day + "-" + now.Year + "_" + (int)now.TimeOfDay.TotalMilliseconds;
		}
		if (currentLocation == null)
		{
			return null;
		}
		string filename = screenshot_name + ".png";
		int start_x = 0;
		int start_y = 0;
		int width = currentLocation.map.DisplayWidth;
		int height = currentLocation.map.DisplayHeight;
		string[] fields = currentLocation.GetMapPropertySplitBySpaces("ScreenshotRegion");
		if (fields.Length != 0)
		{
			if (!ArgUtility.TryGetInt(fields, 0, out var topLeftX, out var error) || !ArgUtility.TryGetInt(fields, 1, out var topLeftY, out error) || !ArgUtility.TryGetInt(fields, 2, out var bottomRightX, out error) || !ArgUtility.TryGetInt(fields, 3, out var bottomRightY, out error))
			{
				currentLocation.LogMapPropertyError("ScreenshotRegion", fields, error);
			}
			else
			{
				start_x = topLeftX * 64;
				start_y = topLeftY * 64;
				width = (bottomRightX + 1) * 64 - start_x;
				height = (bottomRightY + 1) * 64 - start_y;
			}
		}
		SKSurface map_bitmap = null;
		bool failed;
		int scaled_width;
		int scaled_height;
		do
		{
			failed = false;
			scaled_width = (int)((float)width * scale);
			scaled_height = (int)((float)height * scale);
			try
			{
				map_bitmap = SKSurface.Create(scaled_width, scaled_height, SKColorType.Rgb888x, SKAlphaType.Opaque);
			}
			catch (Exception e)
			{
				log.Error("Map Screenshot: Error trying to create Bitmap.", e);
				failed = true;
			}
			if (failed)
			{
				scale -= 0.25f;
			}
			if (scale <= 0f)
			{
				return null;
			}
		}
		while (failed);
		int chunk_size = 2048;
		int scaled_chunk_size = (int)((float)chunk_size * scale);
		xTile.Dimensions.Rectangle old_viewport = viewport;
		bool old_display_hud = displayHUD;
		takingMapScreenshot = true;
		float old_zoom_level = options.baseZoomLevel;
		options.baseZoomLevel = 1f;
		RenderTarget2D cached_lightmap = _lightmap;
		_lightmap = null;
		bool fail = false;
		try
		{
			allocateLightmap(chunk_size, chunk_size);
			int chunks_wide = (int)Math.Ceiling((float)scaled_width / (float)scaled_chunk_size);
			int chunks_high = (int)Math.Ceiling((float)scaled_height / (float)scaled_chunk_size);
			for (int y_offset = 0; y_offset < chunks_high; y_offset++)
			{
				for (int x_offset = 0; x_offset < chunks_wide; x_offset++)
				{
					int current_width = scaled_chunk_size;
					int current_height = scaled_chunk_size;
					int current_x = x_offset * scaled_chunk_size;
					int current_y = y_offset * scaled_chunk_size;
					if (current_x + scaled_chunk_size > scaled_width)
					{
						current_width += scaled_width - (current_x + scaled_chunk_size);
					}
					if (current_y + scaled_chunk_size > scaled_height)
					{
						current_height += scaled_height - (current_y + scaled_chunk_size);
					}
					if (current_height <= 0 || current_width <= 0)
					{
						continue;
					}
					Microsoft.Xna.Framework.Rectangle rect = new Microsoft.Xna.Framework.Rectangle(current_x, current_y, current_width, current_height);
					RenderTarget2D render_target = new RenderTarget2D(graphics.GraphicsDevice, chunk_size, chunk_size, mipMap: false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
					viewport = new xTile.Dimensions.Rectangle(x_offset * chunk_size + start_x, y_offset * chunk_size + start_y, chunk_size, chunk_size);
					_draw(currentGameTime, render_target);
					RenderTarget2D scaled_render_target = new RenderTarget2D(graphics.GraphicsDevice, current_width, current_height, mipMap: false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
					base.GraphicsDevice.SetRenderTarget(scaled_render_target);
					spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
					Color color = Color.White;
					spriteBatch.Draw(render_target, Vector2.Zero, render_target.Bounds, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 1f);
					spriteBatch.End();
					render_target.Dispose();
					base.GraphicsDevice.SetRenderTarget(null);
					Color[] colors = new Color[current_width * current_height];
					scaled_render_target.GetData(colors);
					SKBitmap portion_bitmap = new SKBitmap(rect.Width, rect.Height, SKColorType.Rgb888x, SKAlphaType.Opaque);
					byte* ptr = (byte*)portion_bitmap.GetPixels().ToPointer();
					for (int row = 0; row < current_height; row++)
					{
						for (int col = 0; col < current_width; col++)
						{
							*(ptr++) = colors[col + row * current_width].R;
							*(ptr++) = colors[col + row * current_width].G;
							*(ptr++) = colors[col + row * current_width].B;
							*(ptr++) = byte.MaxValue;
						}
					}
					SKPaint paint = new SKPaint();
					map_bitmap.Canvas.DrawBitmap(portion_bitmap, SKRect.Create(rect.X, rect.Y, current_width, current_height), paint);
					portion_bitmap.Dispose();
					scaled_render_target.Dispose();
				}
			}
			string fullFilePath = Path.Combine(GetScreenshotFolder(), filename);
			map_bitmap.Snapshot().Encode(SKEncodedImageFormat.Png, 100).SaveTo(new FileStream(fullFilePath, FileMode.OpenOrCreate));
			map_bitmap.Dispose();
		}
		catch (Exception e)
		{
			log.Error("Map Screenshot: Error taking screenshot.", e);
			base.GraphicsDevice.SetRenderTarget(null);
			fail = true;
		}
		if (_lightmap != null)
		{
			_lightmap.Dispose();
			_lightmap = null;
		}
		_lightmap = cached_lightmap;
		options.baseZoomLevel = old_zoom_level;
		takingMapScreenshot = false;
		displayHUD = old_display_hud;
		viewport = old_viewport;
		if (fail)
		{
			return null;
		}
		return filename;
	}
}
