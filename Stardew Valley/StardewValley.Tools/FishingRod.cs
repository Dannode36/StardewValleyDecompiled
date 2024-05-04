using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Constants;
using StardewValley.Enchantments;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.SpecialOrders;
using StardewValley.TokenizableStrings;

namespace StardewValley.Tools;

public class FishingRod : Tool
{
	/// <summary>The index in <see cref="F:StardewValley.Tool.attachments" /> for equipped bait.</summary>
	public const int BaitIndex = 0;

	/// <summary>The index in <see cref="F:StardewValley.Tool.attachments" /> for equipped tackle.</summary>
	public const int TackleIndex = 1;

	public const int sizeOfLandCheckRectangle = 11;

	public static int NUM_BOBBER_STYLES = 39;

	[XmlElement("bobber")]
	public readonly NetPosition bobber = new NetPosition();

	/// <summary>The underlying field for <see cref="P:StardewValley.Tools.FishingRod.CastDirection" />.</summary>
	private readonly NetInt castDirection = new NetInt(-1);

	public static int minFishingBiteTime = 600;

	public static int maxFishingBiteTime = 30000;

	public static int maxTimeToNibble = 800;

	public static int maxTackleUses = 20;

	private int whichTackleSlotToReplace = 1;

	protected Vector2 _lastAppliedMotion = Vector2.Zero;

	protected Vector2[] _totalMotionBuffer = new Vector2[4];

	protected int _totalMotionBufferIndex;

	protected NetVector2 _totalMotion = new NetVector2(Vector2.Zero)
	{
		InterpolationEnabled = false,
		InterpolationWait = false
	};

	public static double baseChanceForTreasure = 0.15;

	[XmlIgnore]
	public int bobberBob;

	[XmlIgnore]
	public float bobberTimeAccumulator;

	[XmlIgnore]
	public float timePerBobberBob = 2000f;

	[XmlIgnore]
	public float timeUntilFishingBite = -1f;

	[XmlIgnore]
	public float fishingBiteAccumulator;

	[XmlIgnore]
	public float fishingNibbleAccumulator;

	[XmlIgnore]
	public float timeUntilFishingNibbleDone = -1f;

	[XmlIgnore]
	public float castingPower;

	[XmlIgnore]
	public float castingChosenCountdown;

	[XmlIgnore]
	public float castingTimerSpeed = 0.001f;

	[XmlIgnore]
	public bool isFishing;

	[XmlIgnore]
	public bool hit;

	[XmlIgnore]
	public bool isNibbling;

	[XmlIgnore]
	public bool favBait;

	[XmlIgnore]
	public bool isTimingCast;

	[XmlIgnore]
	public bool isCasting;

	[XmlIgnore]
	public bool castedButBobberStillInAir;

	[XmlIgnore]
	public bool gotTroutDerbyTag;

	/// <summary>The cached value for <see cref="M:StardewValley.Tools.FishingRod.GetWaterColor" />.</summary>
	protected Color? lastWaterColor;

	[XmlIgnore]
	protected bool _hasPlayerAdjustedBobber;

	[XmlIgnore]
	public bool lastCatchWasJunk;

	[XmlIgnore]
	public bool goldenTreasure;

	[XmlIgnore]
	public bool doneWithAnimation;

	[XmlIgnore]
	public bool pullingOutOfWater;

	[XmlIgnore]
	public bool isReeling;

	[XmlIgnore]
	public bool hasDoneFucntionYet;

	[XmlIgnore]
	public bool fishCaught;

	[XmlIgnore]
	public bool recordSize;

	[XmlIgnore]
	public bool treasureCaught;

	[XmlIgnore]
	public bool showingTreasure;

	[XmlIgnore]
	public bool hadBobber;

	[XmlIgnore]
	public bool bossFish;

	[XmlIgnore]
	public bool fromFishPond;

	[XmlIgnore]
	public TemporaryAnimatedSpriteList animations = new TemporaryAnimatedSpriteList();

	[XmlIgnore]
	public SparklingText sparklingText;

	[XmlIgnore]
	public int fishSize;

	[XmlIgnore]
	public int fishQuality;

	[XmlIgnore]
	public int clearWaterDistance;

	[XmlIgnore]
	public int originalFacingDirection;

	[XmlIgnore]
	public int numberOfFishCaught = 1;

	[XmlIgnore]
	public ItemMetadata whichFish;

	/// <summary>The mail flag to set for the current player when the current <see cref="F:StardewValley.Tools.FishingRod.whichFish" /> is successfully caught.</summary>
	[XmlIgnore]
	public string setFlagOnCatch;

	/// <summary>The delay (in milliseconds) before recasting if the left mouse is held down after closing the 'caught fish' display.</summary>
	[XmlIgnore]
	public int recastTimerMs;

	protected const int RECAST_DELAY_MS = 200;

	[XmlIgnore]
	private readonly NetEventBinary pullFishFromWaterEvent = new NetEventBinary();

	[XmlIgnore]
	private readonly NetEvent1Field<bool, NetBool> doneFishingEvent = new NetEvent1Field<bool, NetBool>();

	[XmlIgnore]
	private readonly NetEvent0 startCastingEvent = new NetEvent0();

	[XmlIgnore]
	private readonly NetEvent0 castingEndEnableMovementEvent = new NetEvent0();

	[XmlIgnore]
	private readonly NetEvent0 putAwayEvent = new NetEvent0();

	[XmlIgnore]
	private readonly NetEvent0 beginReelingEvent = new NetEvent0();

	public static ICue chargeSound;

	public static ICue reelSound;

	private int randomBobberStyle = -1;

	private bool usedGamePadToCast;

	/// <summary>The direction in which the fishing rod was cast.</summary>
	public int CastDirection
	{
		get
		{
			if (fishCaught)
			{
				return 2;
			}
			return castDirection.Value;
		}
		set
		{
			castDirection.Value = value;
		}
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(bobber.NetFields, "bobber.NetFields").AddField(castDirection, "castDirection").AddField(pullFishFromWaterEvent, "pullFishFromWaterEvent")
			.AddField(doneFishingEvent, "doneFishingEvent")
			.AddField(startCastingEvent, "startCastingEvent")
			.AddField(castingEndEnableMovementEvent, "castingEndEnableMovementEvent")
			.AddField(putAwayEvent, "putAwayEvent")
			.AddField(_totalMotion, "_totalMotion")
			.AddField(beginReelingEvent, "beginReelingEvent");
		pullFishFromWaterEvent.AddReaderHandler(doPullFishFromWater);
		doneFishingEvent.onEvent += doDoneFishing;
		startCastingEvent.onEvent += doStartCasting;
		castingEndEnableMovementEvent.onEvent += doCastingEndEnableMovement;
		beginReelingEvent.onEvent += beginReeling;
		putAwayEvent.onEvent += resetState;
	}

	/// <inheritdoc />
	protected override void MigrateLegacyItemId()
	{
		switch (base.UpgradeLevel)
		{
		case 0:
			base.ItemId = "BambooPole";
			break;
		case 1:
			base.ItemId = "TrainingRod";
			break;
		case 2:
			base.ItemId = "FiberglassRod";
			break;
		case 3:
			base.ItemId = "IridiumRod";
			break;
		case 4:
			base.ItemId = "AdvancedIridiumRod";
			break;
		default:
			base.ItemId = "BambooPole";
			break;
		}
	}

	public override void actionWhenStopBeingHeld(Farmer who)
	{
		putAwayEvent.Fire();
		base.actionWhenStopBeingHeld(who);
	}

	public FishingRod()
		: base("Fishing Rod", 0, 189, 8, stackable: false, 2)
	{
	}

	public override void resetState()
	{
		isNibbling = false;
		fishCaught = false;
		isFishing = false;
		isReeling = false;
		isCasting = false;
		isTimingCast = false;
		doneWithAnimation = false;
		pullingOutOfWater = false;
		fromFishPond = false;
		numberOfFishCaught = 1;
		fishingBiteAccumulator = 0f;
		showingTreasure = false;
		fishingNibbleAccumulator = 0f;
		timeUntilFishingBite = -1f;
		timeUntilFishingNibbleDone = -1f;
		bobberTimeAccumulator = 0f;
		castingChosenCountdown = 0f;
		lastWaterColor = null;
		gotTroutDerbyTag = false;
		_totalMotionBufferIndex = 0;
		for (int i = 0; i < _totalMotionBuffer.Length; i++)
		{
			_totalMotionBuffer[i] = Vector2.Zero;
		}
		if (lastUser != null && lastUser == Game1.player)
		{
			for (int i = Game1.screenOverlayTempSprites.Count - 1; i >= 0; i--)
			{
				if (Game1.screenOverlayTempSprites[i].id == 987654321)
				{
					Game1.screenOverlayTempSprites.RemoveAt(i);
				}
			}
		}
		_totalMotion.Value = Vector2.Zero;
		_lastAppliedMotion = Vector2.Zero;
		pullFishFromWaterEvent.Clear();
		doneFishingEvent.Clear();
		startCastingEvent.Clear();
		castingEndEnableMovementEvent.Clear();
		beginReelingEvent.Clear();
		bobber.Set(Vector2.Zero);
		CastDirection = -1;
	}

	public FishingRod(int upgradeLevel)
		: base("Fishing Rod", upgradeLevel, 189, 8, stackable: false, (upgradeLevel == 4) ? 3 : 2)
	{
		base.IndexOfMenuItemView = 8 + upgradeLevel;
	}

	public FishingRod(int upgradeLevel, int numAttachmentSlots)
		: base("Fishing Rod", upgradeLevel, 189, 8, stackable: false, numAttachmentSlots)
	{
		base.IndexOfMenuItemView = 8 + upgradeLevel;
	}

	/// <inheritdoc />
	protected override Item GetOneNew()
	{
		return new FishingRod();
	}

	private int getAddedDistance(Farmer who)
	{
		if (who.FishingLevel >= 15)
		{
			return 4;
		}
		if (who.FishingLevel >= 8)
		{
			return 3;
		}
		if (who.FishingLevel >= 4)
		{
			return 2;
		}
		if (who.FishingLevel >= 1)
		{
			return 1;
		}
		return 0;
	}

	private Vector2 calculateBobberTile()
	{
		return new Vector2(bobber.X / 64f, bobber.Y / 64f);
	}

	public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
	{
		who = who ?? lastUser;
		if (fishCaught || (!who.IsLocalPlayer && (isReeling || isFishing || pullingOutOfWater)))
		{
			return;
		}
		hasDoneFucntionYet = true;
		Vector2 bobberTile = calculateBobberTile();
		int tileX = (int)bobberTile.X;
		int tileY = (int)bobberTile.Y;
		base.DoFunction(location, x, y, power, who);
		if (doneWithAnimation)
		{
			who.canReleaseTool = true;
		}
		if (Game1.isAnyGamePadButtonBeingPressed())
		{
			Game1.lastCursorMotionWasMouse = false;
		}
		if (!isFishing && !castedButBobberStillInAir && !pullingOutOfWater && !isNibbling && !hit && !showingTreasure)
		{
			if (!Game1.eventUp && who.IsLocalPlayer && !hasEnchantmentOfType<EfficientToolEnchantment>())
			{
				float oldStamina = who.Stamina;
				who.Stamina -= 8f - (float)who.FishingLevel * 0.1f;
				who.checkForExhaustion(oldStamina);
			}
			if (location.canFishHere() && location.isTileFishable(tileX, tileY))
			{
				clearWaterDistance = distanceToLand((int)(bobber.X / 64f), (int)(bobber.Y / 64f), who.currentLocation);
				isFishing = true;
				location.temporarySprites.Add(new TemporaryAnimatedSprite(28, 100f, 2, 1, new Vector2(bobber.X - 32f, bobber.Y - 32f), flicker: false, flipped: false));
				if (who.IsLocalPlayer)
				{
					location.playSound("dropItemInWater", bobberTile);
					Game1.stats.TimesFished++;
				}
				timeUntilFishingBite = calculateTimeUntilFishingBite(bobberTile, isFirstCast: true, who);
				if (location.fishSplashPoint != null)
				{
					bool frenzy = location.fishFrenzyFish.Value != null && !location.fishFrenzyFish.Equals("");
					Rectangle fishSplashRect = new Rectangle(location.fishSplashPoint.X * 64, location.fishSplashPoint.Y * 64, 64, 64);
					if (frenzy)
					{
						fishSplashRect.Inflate(32, 32);
					}
					if (new Rectangle((int)bobber.X - 32, (int)bobber.Y - 32, 64, 64).Intersects(fishSplashRect))
					{
						timeUntilFishingBite /= (frenzy ? 2 : 4);
						location.temporarySprites.Add(new TemporaryAnimatedSprite(10, bobber.Value - new Vector2(32f, 32f), Color.Cyan));
					}
				}
				who.UsingTool = true;
				who.canMove = false;
			}
			else
			{
				if (doneWithAnimation)
				{
					who.UsingTool = false;
				}
				if (doneWithAnimation)
				{
					who.canMove = true;
				}
			}
		}
		else
		{
			if (isCasting || pullingOutOfWater)
			{
				return;
			}
			bool fromFishPond = location.isTileBuildingFishable((int)bobberTile.X, (int)bobberTile.Y);
			who.FarmerSprite.PauseForSingleAnimation = false;
			int result = who.FacingDirection;
			switch (result)
			{
			case 0:
				who.FarmerSprite.animateBackwardsOnce(299, 35f);
				break;
			case 1:
				who.FarmerSprite.animateBackwardsOnce(300, 35f);
				break;
			case 2:
				who.FarmerSprite.animateBackwardsOnce(301, 35f);
				break;
			case 3:
				who.FarmerSprite.animateBackwardsOnce(302, 35f);
				break;
			}
			if (isNibbling)
			{
				Object bait = GetBait();
				double baitPotency = ((bait != null) ? ((float)bait.Price / 10f) : 0f);
				bool splashPoint = false;
				if (location.fishSplashPoint != null)
				{
					Rectangle fishSplashRect = new Rectangle(location.fishSplashPoint.X * 64, location.fishSplashPoint.Y * 64, 64, 64);
					Rectangle bobberRect = new Rectangle((int)bobber.X - 80, (int)bobber.Y - 80, 64, 64);
					splashPoint = fishSplashRect.Intersects(bobberRect);
				}
				Item o = location.getFish(fishingNibbleAccumulator, bait?.QualifiedItemId, clearWaterDistance + (splashPoint ? 1 : 0), who, baitPotency + (splashPoint ? 0.4 : 0.0), bobberTile);
				if (o == null || ItemRegistry.GetDataOrErrorItem(o.QualifiedItemId).IsErrorItem)
				{
					result = Game1.random.Next(167, 173);
					o = ItemRegistry.Create("(O)" + result);
				}
				Object obj = o as Object;
				if (obj != null && obj.scale.X == 1f)
				{
					favBait = true;
				}
				Dictionary<string, string> data = DataLoader.Fish(Game1.content);
				bool non_fishable_fish = false;
				string rawData;
				if (!o.HasTypeObject())
				{
					non_fishable_fish = true;
				}
				else if (data.TryGetValue(o.ItemId, out rawData))
				{
					if (!int.TryParse(rawData.Split('/')[1], out result))
					{
						non_fishable_fish = true;
					}
				}
				else
				{
					non_fishable_fish = true;
				}
				lastCatchWasJunk = false;
				bool isJunk;
				switch (o.QualifiedItemId)
				{
				case "(O)152":
				case "(O)153":
				case "(O)157":
				case "(O)797":
				case "(O)79":
				case "(O)73":
				case "(O)842":
				case "(O)890":
				case "(O)820":
				case "(O)821":
				case "(O)822":
				case "(O)823":
				case "(O)824":
				case "(O)825":
				case "(O)826":
				case "(O)827":
				case "(O)828":
					isJunk = true;
					break;
				default:
					isJunk = o.Category == -20 || o.QualifiedItemId == GameLocation.CAROLINES_NECKLACE_ITEM_QID;
					break;
				}
				if (isJunk || fromFishPond || non_fishable_fish)
				{
					lastCatchWasJunk = true;
					pullFishFromWater(o.QualifiedItemId, -1, 0, 0, treasureCaught: false, wasPerfect: false, fromFishPond, o.SetFlagOnPickup, isBossFish: false, 1);
				}
				else if (!hit && who.IsLocalPlayer)
				{
					hit = true;
					Game1.screenOverlayTempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(612, 1913, 74, 30), 1500f, 1, 0, Game1.GlobalToLocal(Game1.viewport, bobber.Value + new Vector2(-140f, -160f)), flicker: false, flipped: false, 1f, 0.005f, Color.White, 4f, 0.075f, 0f, 0f, local: true)
					{
						scaleChangeChange = -0.005f,
						motion = new Vector2(0f, -0.1f),
						endFunction = delegate
						{
							startMinigameEndFunction(o);
						},
						id = 987654321
					});
					who.playNearbySoundLocal("FishHit");
				}
				return;
			}
			if (fromFishPond)
			{
				Item fishPondPull = location.getFish(-1f, null, -1, who, -1.0, bobberTile);
				if (fishPondPull != null)
				{
					pullFishFromWater(fishPondPull.QualifiedItemId, -1, 0, 0, treasureCaught: false, wasPerfect: false, fromFishPond: true, null, isBossFish: false, 1);
					return;
				}
			}
			if (who.IsLocalPlayer)
			{
				location.playSound("pullItemFromWater", bobberTile);
			}
			isFishing = false;
			pullingOutOfWater = true;
			Point playerPixel = who.StandingPixel;
			if (who.FacingDirection == 1 || who.FacingDirection == 3)
			{
				float num = Math.Abs(bobber.X - (float)playerPixel.X);
				float gravity = 0.005f;
				float velocity = 0f - (float)Math.Sqrt(num * gravity / 2f);
				float t = 2f * (Math.Abs(velocity - 0.5f) / gravity);
				t *= 1.2f;
				Rectangle sourceRect = Game1.getSourceRectForStandardTileSheet(Game1.bobbersTexture, getBobberStyle(who), 16, 32);
				sourceRect.Height = 16;
				animations.Add(new TemporaryAnimatedSprite("TileSheets\\bobbers", sourceRect, t, 1, 0, bobber.Value + new Vector2(-32f, -48f), flicker: false, flipped: false, (float)playerPixel.Y / 10000f, 0f, Color.White, 4f, 0f, 0f, (float)Game1.random.Next(-20, 20) / 100f)
				{
					motion = new Vector2((float)((who.FacingDirection != 3) ? 1 : (-1)) * (velocity + 0.2f), velocity - 0.8f),
					acceleration = new Vector2(0f, gravity),
					endFunction = donefishingEndFunction,
					timeBasedMotion = true,
					alphaFade = 0.001f,
					flipped = (who.FacingDirection == 1 && flipCurrentBobberWhenFacingRight())
				});
			}
			else
			{
				float distance = bobber.Y - (float)playerPixel.Y;
				float height = Math.Abs(distance + 256f);
				float gravity = 0.005f;
				float velocity = (float)Math.Sqrt(2f * gravity * height);
				float t = (float)(Math.Sqrt(2f * (height - distance) / gravity) + (double)(velocity / gravity));
				Rectangle sourceRect = Game1.getSourceRectForStandardTileSheet(Game1.bobbersTexture, getBobberStyle(who), 16, 32);
				sourceRect.Height = 16;
				animations.Add(new TemporaryAnimatedSprite("TileSheets\\bobbers", sourceRect, t, 1, 0, bobber.Value + new Vector2(-32f, -48f), flicker: false, flipped: false, bobber.Y / 10000f, 0f, Color.White, 4f, 0f, 0f, (float)Game1.random.Next(-20, 20) / 100f)
				{
					motion = new Vector2(((float)who.StandingPixel.X - bobber.Value.X) / 800f, 0f - velocity),
					acceleration = new Vector2(0f, gravity),
					endFunction = donefishingEndFunction,
					timeBasedMotion = true,
					alphaFade = 0.001f
				});
			}
			who.UsingTool = true;
			who.canReleaseTool = false;
		}
	}

	public int getBobberStyle(Farmer who)
	{
		if (GetTackleQualifiedItemIDs().Contains("(O)789"))
		{
			return 39;
		}
		if (who != null)
		{
			if (randomBobberStyle == -1 && who.usingRandomizedBobber && randomBobberStyle == -1)
			{
				who.bobberStyle.Value = Math.Min(NUM_BOBBER_STYLES - 1, Game1.random.Next(Game1.player.fishCaught.Count() / 2));
				randomBobberStyle = who.bobberStyle.Value;
			}
			return who.bobberStyle.Value;
		}
		return 0;
	}

	public bool flipCurrentBobberWhenFacingRight()
	{
		switch (getBobberStyle(getLastFarmerToUse()))
		{
		case 9:
		case 19:
		case 21:
		case 23:
		case 36:
			return true;
		default:
			return false;
		}
	}

	public Color getFishingLineColor()
	{
		switch (getBobberStyle(getLastFarmerToUse()))
		{
		case 6:
		case 20:
			return new Color(255, 200, 255);
		case 7:
			return Color.Yellow;
		case 35:
		case 39:
			return new Color(180, 160, 255);
		case 9:
			return new Color(255, 255, 200);
		case 10:
			return new Color(255, 208, 169);
		case 11:
			return new Color(170, 170, 255);
		case 12:
			return Color.DimGray;
		case 14:
		case 22:
			return new Color(178, 255, 112);
		case 15:
			return new Color(250, 193, 70);
		case 16:
			return new Color(255, 170, 170);
		case 37:
		case 38:
			return new Color(200, 255, 255);
		case 17:
			return new Color(200, 220, 255);
		case 13:
			return new Color(228, 228, 172);
		case 31:
			return Color.Red * 0.5f;
		case 29:
		case 32:
			return Color.Lime * 0.66f;
		case 25:
		case 27:
			return Color.White * 0.5f;
		default:
			return Color.White;
		}
	}

	private float calculateTimeUntilFishingBite(Vector2 bobberTile, bool isFirstCast, Farmer who)
	{
		if (Game1.currentLocation.isTileBuildingFishable((int)bobberTile.X, (int)bobberTile.Y) && Game1.currentLocation.getBuildingAt(bobberTile) is FishPond pond && (int)pond.currentOccupants > 0)
		{
			return FishPond.FISHING_MILLISECONDS;
		}
		List<string> tackleIds = GetTackleQualifiedItemIDs();
		string baitId = GetBait()?.QualifiedItemId;
		int reductionTime = 0;
		reductionTime += Utility.getStringCountInList(tackleIds, "(O)687") * 10000;
		reductionTime += Utility.getStringCountInList(tackleIds, "(O)686") * 5000;
		float time = Game1.random.Next(minFishingBiteTime, Math.Max(minFishingBiteTime, maxFishingBiteTime - 250 * who.FishingLevel - reductionTime));
		if (isFirstCast)
		{
			time *= 0.75f;
		}
		if (baitId != null)
		{
			time *= 0.5f;
			if (baitId == "(O)774" || baitId == "(O)ChallengeBait")
			{
				time *= 0.75f;
			}
			if (baitId == "(O)DeluxeBait")
			{
				time *= 0.66f;
			}
		}
		return Math.Max(500f, time);
	}

	public Color getColor()
	{
		return upgradeLevel switch
		{
			0L => Color.Goldenrod, 
			1L => Color.OliveDrab, 
			2L => Color.White, 
			3L => Color.Violet, 
			4L => new Color(128, 143, 255), 
			_ => Color.White, 
		};
	}

	public static int distanceToLand(int tileX, int tileY, GameLocation location, bool landMustBeAdjacentToWalkableTile = false)
	{
		Rectangle r = new Rectangle(tileX - 1, tileY - 1, 3, 3);
		bool foundLand = false;
		int distance = 1;
		while (!foundLand && r.Width <= 11)
		{
			foreach (Vector2 v in Utility.getBorderOfThisRectangle(r))
			{
				if (!location.isTileOnMap(v) || location.isWaterTile((int)v.X, (int)v.Y))
				{
					continue;
				}
				foundLand = true;
				distance = r.Width / 2;
				if (!landMustBeAdjacentToWalkableTile)
				{
					break;
				}
				foundLand = false;
				Vector2[] surroundingTileLocationsArray = Utility.getSurroundingTileLocationsArray(v);
				foreach (Vector2 surroundings in surroundingTileLocationsArray)
				{
					if (location.isTilePassable(surroundings) && !location.isWaterTile((int)v.X, (int)v.Y))
					{
						foundLand = true;
						break;
					}
				}
				break;
			}
			r.Inflate(1, 1);
		}
		if (r.Width > 11)
		{
			distance = 6;
		}
		return distance - 1;
	}

	public void startMinigameEndFunction(Item fish)
	{
		fish.TryGetTempData<bool>("IsBossFish", out bossFish);
		Farmer who = lastUser;
		beginReelingEvent.Fire();
		isReeling = true;
		hit = false;
		switch (who.FacingDirection)
		{
		case 1:
			who.FarmerSprite.setCurrentSingleFrame(48, 32000);
			break;
		case 3:
			who.FarmerSprite.setCurrentSingleFrame(48, 32000, secondaryArm: false, flip: true);
			break;
		}
		float fishSize = 1f;
		fishSize *= (float)clearWaterDistance / 5f;
		int minimumSizeContribution = 1 + who.FishingLevel / 2;
		fishSize *= (float)Game1.random.Next(minimumSizeContribution, Math.Max(6, minimumSizeContribution)) / 5f;
		if (favBait)
		{
			fishSize *= 1.2f;
		}
		fishSize *= 1f + (float)Game1.random.Next(-10, 11) / 100f;
		fishSize = Math.Max(0f, Math.Min(1f, fishSize));
		string baitId = GetBait()?.QualifiedItemId;
		List<string> tackleIds = GetTackleQualifiedItemIDs();
		double extraTreasureChance = (double)Utility.getStringCountInList(tackleIds, "(O)693") * baseChanceForTreasure / 3.0;
		goldenTreasure = false;
		int num;
		if (!Game1.isFestival())
		{
			NetStringIntArrayDictionary netStringIntArrayDictionary = who.fishCaught;
			if (netStringIntArrayDictionary != null && netStringIntArrayDictionary.Length > 1)
			{
				num = ((Game1.random.NextDouble() < baseChanceForTreasure + (double)who.LuckLevel * 0.005 + ((baitId == "(O)703") ? baseChanceForTreasure : 0.0) + extraTreasureChance + who.DailyLuck / 2.0 + (who.professions.Contains(9) ? baseChanceForTreasure : 0.0)) ? 1 : 0);
				goto IL_01cc;
			}
		}
		num = 0;
		goto IL_01cc;
		IL_01cc:
		bool treasure = (byte)num != 0;
		if (treasure && Game1.player.stats.Get(StatKeys.Mastery(1)) != 0 && Game1.random.NextDouble() < 0.25 + Game1.player.team.AverageDailyLuck())
		{
			goldenTreasure = true;
		}
		Game1.activeClickableMenu = new BobberBar(fish.ItemId, fishSize, treasure, tackleIds, fish.SetFlagOnPickup, bossFish, baitId, goldenTreasure);
	}

	/// <summary>Get the equipped tackle, if any.</summary>
	public List<Object> GetTackle()
	{
		List<Object> tack = new List<Object>();
		if (CanUseTackle())
		{
			for (int i = 1; i < attachments.Count; i++)
			{
				tack.Add(attachments[i]);
			}
		}
		return tack;
	}

	public List<string> GetTackleQualifiedItemIDs()
	{
		List<string> ids = new List<string>();
		foreach (Object o in GetTackle())
		{
			if (o != null)
			{
				ids.Add(o.QualifiedItemId);
			}
		}
		return ids;
	}

	/// <summary>Get the equipped bait, if any.</summary>
	public Object GetBait()
	{
		if (!CanUseBait())
		{
			return null;
		}
		return attachments[0];
	}

	/// <summary>Whether the fishing rod has Magic Bait equipped.</summary>
	public bool HasMagicBait()
	{
		return GetBait()?.QualifiedItemId == "(O)908";
	}

	/// <summary>Whether the fishing rod has a Curiosity Lure equipped.</summary>
	public bool HasCuriosityLure()
	{
		return GetTackleQualifiedItemIDs().Contains("(O)856");
	}

	public bool inUse()
	{
		if (!isFishing && !isCasting && !isTimingCast && !isNibbling && !isReeling)
		{
			return fishCaught;
		}
		return true;
	}

	public void donefishingEndFunction(int extra)
	{
		Farmer who = lastUser;
		isFishing = false;
		isReeling = false;
		who.canReleaseTool = true;
		who.canMove = true;
		who.UsingTool = false;
		who.FarmerSprite.PauseForSingleAnimation = false;
		pullingOutOfWater = false;
		doneFishing(who);
	}

	public static void endOfAnimationBehavior(Farmer f)
	{
	}

	public override void drawAttachments(SpriteBatch b, int x, int y)
	{
		y += ((enchantments.Count > 0) ? 8 : 4);
		if (CanUseBait())
		{
			DrawAttachmentSlot(0, b, x, y);
		}
		y += 68;
		if (CanUseTackle())
		{
			for (int i = 1; i < base.AttachmentSlotsCount; i++)
			{
				DrawAttachmentSlot(i, b, x, y);
				x += 68;
			}
		}
	}

	/// <inheritdoc />
	protected override void GetAttachmentSlotSprite(int slot, out Texture2D texture, out Rectangle sourceRect)
	{
		base.GetAttachmentSlotSprite(slot, out texture, out sourceRect);
		if (slot == 0)
		{
			if (GetBait() == null)
			{
				sourceRect = Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 36);
			}
		}
		else if (attachments[slot] == null)
		{
			sourceRect = Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 37);
		}
	}

	/// <inheritdoc />
	protected override bool canThisBeAttached(Object o, int slot)
	{
		if (o.QualifiedItemId == "(O)789" && slot != 0)
		{
			return true;
		}
		if (slot != 0)
		{
			if (o.Category == -22)
			{
				return CanUseTackle();
			}
			return false;
		}
		if (o.Category == -21)
		{
			return CanUseBait();
		}
		return false;
	}

	/// <summary>Whether the fishing rod has a bait attachment slot.</summary>
	public bool CanUseBait()
	{
		return base.AttachmentSlotsCount > 0;
	}

	/// <summary>Whether the fishing rod has a tackle attachment slot.</summary>
	public bool CanUseTackle()
	{
		return base.AttachmentSlotsCount > 1;
	}

	public void playerCaughtFishEndFunction(bool isBossFish)
	{
		Farmer who = lastUser;
		who.Halt();
		who.armOffset = Vector2.Zero;
		castedButBobberStillInAir = false;
		fishCaught = true;
		isReeling = false;
		isFishing = false;
		pullingOutOfWater = false;
		who.canReleaseTool = false;
		if (!who.IsLocalPlayer)
		{
			return;
		}
		bool firstCatch = whichFish.QualifiedItemId.StartsWith("(O)") && !who.fishCaught.ContainsKey(whichFish.QualifiedItemId) && !whichFish.QualifiedItemId.Equals("(O)388") && !whichFish.QualifiedItemId.Equals("(O)390");
		if (!Game1.isFestival())
		{
			recordSize = who.caughtFish(whichFish.QualifiedItemId, fishSize, fromFishPond, numberOfFishCaught);
			who.faceDirection(2);
		}
		else
		{
			Game1.currentLocation.currentEvent.caughtFish(whichFish.QualifiedItemId, fishSize, who);
			fishCaught = false;
			doneFishing(who);
		}
		if (isBossFish)
		{
			Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14068"));
			Game1.multiplayer.globalChatInfoMessage("CaughtLegendaryFish", who.Name, TokenStringBuilder.ItemName(whichFish.QualifiedItemId));
		}
		else if (recordSize)
		{
			sparklingText = new SparklingText(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14069"), Color.LimeGreen, Color.Azure);
			if (!firstCatch)
			{
				who.playNearbySoundLocal("newRecord");
			}
		}
		else
		{
			who.playNearbySoundLocal("fishSlap");
		}
		if (firstCatch && who.fishCaught.ContainsKey(whichFish.QualifiedItemId))
		{
			sparklingText = new SparklingText(Game1.dialogueFont, Game1.content.LoadString("Strings\\1_6_Strings:FirstCatch"), new Color(200, 255, 220), Color.White);
			who.playNearbySoundLocal("discoverMineral");
		}
	}

	public void pullFishFromWater(string fishId, int fishSize, int fishQuality, int fishDifficulty, bool treasureCaught, bool wasPerfect, bool fromFishPond, string setFlagOnCatch, bool isBossFish, int numCaught)
	{
		pullFishFromWaterEvent.Fire(delegate(BinaryWriter writer)
		{
			writer.Write(fishId);
			writer.Write(fishSize);
			writer.Write(fishQuality);
			writer.Write(fishDifficulty);
			writer.Write(treasureCaught);
			writer.Write(wasPerfect);
			writer.Write(fromFishPond);
			writer.Write(setFlagOnCatch ?? string.Empty);
			writer.Write(isBossFish);
			writer.Write(numCaught);
		});
	}

	private void doPullFishFromWater(BinaryReader argReader)
	{
		Farmer who = lastUser;
		string fishId = argReader.ReadString();
		int fishSize = argReader.ReadInt32();
		int fishQuality = argReader.ReadInt32();
		int fishDifficulty = argReader.ReadInt32();
		bool treasureCaught = argReader.ReadBoolean();
		bool wasPerfect = argReader.ReadBoolean();
		bool fromFishPond = argReader.ReadBoolean();
		string setFlagOnCatch = argReader.ReadString();
		bool isBossFish = argReader.ReadBoolean();
		int numCaught = argReader.ReadInt32();
		this.treasureCaught = treasureCaught;
		this.fishSize = fishSize;
		this.fishQuality = fishQuality;
		whichFish = ItemRegistry.GetMetadata(fishId);
		this.fromFishPond = fromFishPond;
		this.setFlagOnCatch = ((setFlagOnCatch != string.Empty) ? setFlagOnCatch : null);
		numberOfFishCaught = numCaught;
		Vector2 bobberTile = calculateBobberTile();
		bool fishIsObject = whichFish.TypeIdentifier == "(O)";
		if (fishQuality >= 2 && wasPerfect)
		{
			this.fishQuality = 4;
		}
		else if (fishQuality >= 1 && wasPerfect)
		{
			this.fishQuality = 2;
		}
		if (who == null)
		{
			return;
		}
		if (!Game1.isFestival() && who.IsLocalPlayer && !fromFishPond && fishIsObject)
		{
			int experience = Math.Max(1, (fishQuality + 1) * 3 + fishDifficulty / 3);
			if (treasureCaught)
			{
				experience += (int)((float)experience * 1.2f);
			}
			if (wasPerfect)
			{
				experience += (int)((float)experience * 1.4f);
			}
			if (isBossFish)
			{
				experience *= 5;
			}
			who.gainExperience(1, experience);
		}
		if (this.fishQuality < 0)
		{
			this.fishQuality = 0;
		}
		string sprite_sheet_name;
		Rectangle sprite_rect;
		if (fishIsObject)
		{
			ParsedItemData parsedOrErrorData = whichFish.GetParsedOrErrorData();
			sprite_sheet_name = parsedOrErrorData.TextureName;
			sprite_rect = parsedOrErrorData.GetSourceRect();
		}
		else
		{
			sprite_sheet_name = "LooseSprites\\Cursors";
			sprite_rect = new Rectangle(228, 408, 16, 16);
		}
		float t;
		if (who.FacingDirection == 1 || who.FacingDirection == 3)
		{
			float distance = Vector2.Distance(bobber.Value, who.Position);
			float gravity = 0.001f;
			float height = 128f - (who.Position.Y - bobber.Y + 10f);
			double angle = 1.1423973285781066;
			float yVelocity = (float)((double)(distance * gravity) * Math.Tan(angle) / Math.Sqrt((double)(2f * distance * gravity) * Math.Tan(angle) - (double)(2f * gravity * height)));
			if (float.IsNaN(yVelocity))
			{
				yVelocity = 0.6f;
			}
			float xVelocity = (float)((double)yVelocity * (1.0 / Math.Tan(angle)));
			t = distance / xVelocity;
			animations.Add(new TemporaryAnimatedSprite(sprite_sheet_name, sprite_rect, t, 1, 0, bobber.Value, flicker: false, flipped: false, bobber.Y / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f)
			{
				motion = new Vector2((float)((who.FacingDirection != 3) ? 1 : (-1)) * (0f - xVelocity), 0f - yVelocity),
				acceleration = new Vector2(0f, gravity),
				timeBasedMotion = true,
				endFunction = delegate
				{
					playerCaughtFishEndFunction(isBossFish);
				},
				endSound = "tinyWhip"
			});
			if (numberOfFishCaught > 1)
			{
				for (int i = 1; i < numberOfFishCaught; i++)
				{
					distance = Vector2.Distance(bobber.Value, who.Position);
					gravity = 0.0008f - (float)i * 0.0001f;
					height = 128f - (who.Position.Y - bobber.Y + 10f);
					angle = 1.1423973285781066;
					yVelocity = (float)((double)(distance * gravity) * Math.Tan(angle) / Math.Sqrt((double)(2f * distance * gravity) * Math.Tan(angle) - (double)(2f * gravity * height)));
					if (float.IsNaN(yVelocity))
					{
						yVelocity = 0.6f;
					}
					xVelocity = (float)((double)yVelocity * (1.0 / Math.Tan(angle)));
					t = distance / xVelocity;
					animations.Add(new TemporaryAnimatedSprite(sprite_sheet_name, sprite_rect, t, 1, 0, bobber.Value, flicker: false, flipped: false, bobber.Y / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						motion = new Vector2((float)((who.FacingDirection != 3) ? 1 : (-1)) * (0f - xVelocity), 0f - yVelocity),
						acceleration = new Vector2(0f, gravity),
						timeBasedMotion = true,
						endSound = "fishSlap",
						Parent = who.currentLocation,
						delayBeforeAnimationStart = (i - 1) * 100
					});
				}
			}
		}
		else
		{
			int playerStandingY = who.StandingPixel.Y;
			float distance = bobber.Y - (float)(playerStandingY - 64);
			float height = Math.Abs(distance + 256f + 32f);
			if (who.FacingDirection == 0)
			{
				height += 96f;
			}
			float gravity = 0.003f;
			float velocity = (float)Math.Sqrt(2f * gravity * height);
			t = (float)(Math.Sqrt(2f * (height - distance) / gravity) + (double)(velocity / gravity));
			float xVelocity = 0f;
			if (t != 0f)
			{
				xVelocity = (who.Position.X - bobber.X) / t;
			}
			animations.Add(new TemporaryAnimatedSprite(sprite_sheet_name, sprite_rect, t, 1, 0, bobber.Value, flicker: false, flipped: false, bobber.Y / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f)
			{
				motion = new Vector2(xVelocity, 0f - velocity),
				acceleration = new Vector2(0f, gravity),
				timeBasedMotion = true,
				endFunction = delegate
				{
					playerCaughtFishEndFunction(isBossFish);
				},
				endSound = "tinyWhip"
			});
			if (numberOfFishCaught > 1)
			{
				for (int i = 1; i < numberOfFishCaught; i++)
				{
					distance = bobber.Y - (float)(playerStandingY - 64);
					height = Math.Abs(distance + 256f + 32f);
					if (who.FacingDirection == 0)
					{
						height += 96f;
					}
					gravity = 0.004f - (float)i * 0.0005f;
					velocity = (float)Math.Sqrt(2f * gravity * height);
					t = (float)(Math.Sqrt(2f * (height - distance) / gravity) + (double)(velocity / gravity));
					xVelocity = 0f;
					if (t != 0f)
					{
						xVelocity = (who.Position.X - bobber.X) / t;
					}
					animations.Add(new TemporaryAnimatedSprite(sprite_sheet_name, sprite_rect, t, 1, 0, new Vector2(bobber.X, bobber.Y), flicker: false, flipped: false, bobber.Y / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						motion = new Vector2(xVelocity, 0f - velocity),
						acceleration = new Vector2(0f, gravity),
						timeBasedMotion = true,
						endSound = "fishSlap",
						Parent = who.currentLocation,
						delayBeforeAnimationStart = (i - 1) * 100
					});
				}
			}
		}
		if (who.IsLocalPlayer)
		{
			who.currentLocation.playSound("pullItemFromWater", bobberTile);
			who.currentLocation.playSound("dwop", bobberTile);
		}
		castedButBobberStillInAir = false;
		pullingOutOfWater = true;
		isFishing = false;
		isReeling = false;
		who.FarmerSprite.PauseForSingleAnimation = false;
		switch (who.FacingDirection)
		{
		case 0:
			who.FarmerSprite.animateBackwardsOnce(299, t);
			break;
		case 1:
			who.FarmerSprite.animateBackwardsOnce(300, t);
			break;
		case 2:
			who.FarmerSprite.animateBackwardsOnce(301, t);
			break;
		case 3:
			who.FarmerSprite.animateBackwardsOnce(302, t);
			break;
		}
	}

	public override void draw(SpriteBatch b)
	{
		base.draw(b);
		Farmer who = lastUser;
		float scale = 4f;
		if (!bobber.Equals(Vector2.Zero) && isFishing)
		{
			Vector2 bobberPos = bobber.Value;
			if (bobberTimeAccumulator > timePerBobberBob)
			{
				if ((!isNibbling && !isReeling) || Game1.random.NextDouble() < 0.05)
				{
					who.playNearbySoundLocal("waterSlosh");
					who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 0, 64, 64), 150f, 8, 0, new Vector2(bobber.X - 32f, bobber.Y - 16f), flicker: false, Game1.random.NextBool(), 0.001f, 0.01f, Color.White, 0.75f, 0.003f, 0f, 0f));
				}
				timePerBobberBob = ((bobberBob == 0) ? Game1.random.Next(1500, 3500) : Game1.random.Next(350, 750));
				bobberTimeAccumulator = 0f;
				if (isNibbling || isReeling)
				{
					timePerBobberBob = Game1.random.Next(25, 75);
					bobberPos.X += Game1.random.Next(-5, 5);
					bobberPos.Y += Game1.random.Next(-5, 5);
					if (!isReeling)
					{
						scale += (float)Game1.random.Next(-20, 20) / 100f;
					}
				}
				else if (Game1.random.NextDouble() < 0.1)
				{
					who.playNearbySoundLocal("bob");
				}
			}
			float bobberLayerDepth = bobberPos.Y / 10000f;
			Rectangle position = Game1.getSourceRectForStandardTileSheet(Game1.bobbersTexture, getBobberStyle(getLastFarmerToUse()), 16, 32);
			position.Height = 16;
			position.Y += 16;
			b.Draw(Game1.bobbersTexture, Game1.GlobalToLocal(Game1.viewport, bobberPos), position, Color.White, 0f, new Vector2(8f, 8f), scale, (getLastFarmerToUse().FacingDirection == 1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, bobberLayerDepth);
			position = new Rectangle(position.X, position.Y + 8, position.Width, position.Height - 8);
		}
		else if ((isTimingCast || castingChosenCountdown > 0f) && who.IsLocalPlayer)
		{
			int yOffset = (int)((0f - Math.Abs(castingChosenCountdown / 2f - castingChosenCountdown)) / 50f);
			float alpha = ((castingChosenCountdown > 0f && castingChosenCountdown < 100f) ? (castingChosenCountdown / 100f) : 1f);
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, getLastFarmerToUse().Position + new Vector2(-48f, -160 + yOffset)), new Rectangle(193, 1868, 47, 12), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.885f);
			b.Draw(Game1.staminaRect, new Rectangle((int)Game1.GlobalToLocal(Game1.viewport, getLastFarmerToUse().Position).X - 32 - 4, (int)Game1.GlobalToLocal(Game1.viewport, getLastFarmerToUse().Position).Y + yOffset - 128 - 32 + 12, (int)(164f * castingPower), 25), Game1.staminaRect.Bounds, Utility.getRedToGreenLerpColor(castingPower) * alpha, 0f, Vector2.Zero, SpriteEffects.None, 0.887f);
		}
		for (int i = animations.Count - 1; i >= 0; i--)
		{
			animations[i].draw(b);
		}
		if (sparklingText != null && !fishCaught)
		{
			sparklingText.draw(b, Game1.GlobalToLocal(Game1.viewport, getLastFarmerToUse().Position + new Vector2(-24f, -192f)));
		}
		else if (sparklingText != null && fishCaught)
		{
			sparklingText.draw(b, Game1.GlobalToLocal(Game1.viewport, getLastFarmerToUse().Position + new Vector2(-64f, -352f)));
		}
		if (!bobber.Value.Equals(Vector2.Zero) && (isFishing || pullingOutOfWater || castedButBobberStillInAir) && who.FarmerSprite.CurrentFrame != 57 && (who.FacingDirection != 0 || !pullingOutOfWater || whichFish == null))
		{
			Vector2 bobberPos = (isFishing ? bobber.Value : ((animations.Count > 0) ? (animations[0].position + new Vector2(0f, 4f * scale)) : Vector2.Zero));
			if (whichFish != null)
			{
				bobberPos += new Vector2(32f, 32f);
			}
			Vector2 lastPosition = Vector2.Zero;
			if (castedButBobberStillInAir)
			{
				switch (who.FacingDirection)
				{
				case 2:
					lastPosition = who.FarmerSprite.currentAnimationIndex switch
					{
						0 => Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(8f, who.armOffset.Y - 96f + 4f)), 
						1 => Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(22f, who.armOffset.Y - 96f + 4f)), 
						2 => Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(28f, who.armOffset.Y - 64f + 40f)), 
						3 => Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(28f, who.armOffset.Y - 8f)), 
						4 => Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(28f, who.armOffset.Y + 32f)), 
						5 => Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(28f, who.armOffset.Y + 32f)), 
						_ => Vector2.Zero, 
					};
					break;
				case 0:
					lastPosition = who.FarmerSprite.currentAnimationIndex switch
					{
						0 => Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(22f, who.armOffset.Y - 96f + 4f)), 
						1 => Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(32f, who.armOffset.Y - 96f + 4f)), 
						2 => Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(36f, who.armOffset.Y - 64f + 40f)), 
						3 => Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(36f, who.armOffset.Y - 16f)), 
						4 => Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(36f, who.armOffset.Y - 32f)), 
						5 => Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(36f, who.armOffset.Y - 32f)), 
						_ => Vector2.Zero, 
					};
					break;
				case 1:
					lastPosition = who.FarmerSprite.currentAnimationIndex switch
					{
						0 => Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(-48f, who.armOffset.Y - 96f - 8f)), 
						1 => Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(-16f, who.armOffset.Y - 96f - 20f)), 
						2 => Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(84f, who.armOffset.Y - 96f - 20f)), 
						3 => Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(112f, who.armOffset.Y - 32f - 20f)), 
						4 => Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(120f, who.armOffset.Y - 32f + 8f)), 
						5 => Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(120f, who.armOffset.Y - 32f + 8f)), 
						_ => Vector2.Zero, 
					};
					break;
				case 3:
					switch (who.FarmerSprite.currentAnimationIndex)
					{
					case 0:
						lastPosition = Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(112f, who.armOffset.Y - 96f - 8f));
						break;
					case 1:
						lastPosition = Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(80f, who.armOffset.Y - 96f - 20f));
						break;
					case 2:
						lastPosition = Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(-20f, who.armOffset.Y - 96f - 20f));
						break;
					case 3:
						lastPosition = Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(-48f, who.armOffset.Y - 32f - 20f));
						break;
					case 4:
						lastPosition = Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(-56f, who.armOffset.Y - 32f + 8f));
						break;
					case 5:
						lastPosition = Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(-56f, who.armOffset.Y - 32f + 8f));
						break;
					}
					break;
				default:
					lastPosition = Vector2.Zero;
					break;
				}
			}
			else if (!isReeling)
			{
				lastPosition = who.FacingDirection switch
				{
					0 => pullingOutOfWater ? Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(22f, who.armOffset.Y - 96f + 4f)) : Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(28f, who.armOffset.Y - 64f - 12f)), 
					2 => pullingOutOfWater ? Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(8f, who.armOffset.Y - 96f + 4f)) : Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(28f, who.armOffset.Y + 64f - 12f)), 
					1 => pullingOutOfWater ? Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(-48f, who.armOffset.Y - 96f - 8f)) : Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(120f, who.armOffset.Y - 64f + 16f)), 
					3 => pullingOutOfWater ? Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(112f, who.armOffset.Y - 96f - 8f)) : Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(-56f, who.armOffset.Y - 64f + 16f)), 
					_ => Vector2.Zero, 
				};
			}
			else if (who != null && who.IsLocalPlayer && Game1.didPlayerJustClickAtAll())
			{
				switch (who.FacingDirection)
				{
				case 0:
					lastPosition = Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(24f, who.armOffset.Y - 96f + 12f));
					break;
				case 3:
					lastPosition = Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(48f, who.armOffset.Y - 96f - 12f));
					break;
				case 2:
					lastPosition = Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(12f, who.armOffset.Y - 96f + 8f));
					break;
				case 1:
					lastPosition = Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(20f, who.armOffset.Y - 96f - 12f));
					break;
				}
			}
			else
			{
				switch (who.FacingDirection)
				{
				case 2:
					lastPosition = Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(12f, who.armOffset.Y - 96f + 4f));
					break;
				case 0:
					lastPosition = Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(25f, who.armOffset.Y - 96f + 4f));
					break;
				case 3:
					lastPosition = Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(36f, who.armOffset.Y - 96f - 8f));
					break;
				case 1:
					lastPosition = Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(28f, who.armOffset.Y - 96f - 8f));
					break;
				}
			}
			Vector2 localBobber = Game1.GlobalToLocal(Game1.viewport, bobberPos + new Vector2(0f, -2.5f * scale + (float)((bobberBob == 1) ? 4 : 0)));
			if (isTimingCast || (isCasting && !who.IsLocalPlayer))
			{
				return;
			}
			if (isReeling)
			{
				Utility.drawLineWithScreenCoordinates((int)lastPosition.X, (int)lastPosition.Y, (int)localBobber.X, (int)localBobber.Y, b, getFishingLineColor() * 0.5f);
				return;
			}
			if (!isFishing)
			{
				localBobber += new Vector2(20f, 20f);
			}
			if (pullingOutOfWater && whichFish != null)
			{
				localBobber += new Vector2(-20f, -30f);
			}
			Vector2 v1 = lastPosition;
			Vector2 v2 = new Vector2(lastPosition.X + (localBobber.X - lastPosition.X) / 3f, lastPosition.Y + (localBobber.Y - lastPosition.Y) * 2f / 3f);
			Vector2 v3 = new Vector2(lastPosition.X + (localBobber.X - lastPosition.X) * 2f / 3f, lastPosition.Y + (localBobber.Y - lastPosition.Y) * (float)(isFishing ? 6 : 2) / 5f);
			Vector2 v4 = localBobber;
			float drawLayer = ((bobberPos.Y > (float)who.StandingPixel.Y) ? (bobberPos.Y / 10000f) : ((float)who.StandingPixel.Y / 10000f)) + ((who.FacingDirection != 0) ? 0.005f : (-0.001f));
			for (float i = 0f; i < 1f; i += 0.025f)
			{
				Vector2 current = Utility.GetCurvePoint(i, v1, v2, v3, v4);
				Utility.drawLineWithScreenCoordinates((int)lastPosition.X, (int)lastPosition.Y, (int)current.X, (int)current.Y, b, getFishingLineColor() * 0.5f, drawLayer);
				lastPosition = current;
			}
		}
		else
		{
			if (!fishCaught)
			{
				return;
			}
			bool fishIsObject = whichFish.TypeIdentifier == "(O)";
			float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
			int playerStandingY = who.StandingPixel.Y;
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(-120f, -288f + yOffset)), new Rectangle(31, 1870, 73, 49), Color.White * 0.8f, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)playerStandingY / 10000f + 0.06f);
			if (fishIsObject)
			{
				ParsedItemData parsedOrErrorData = whichFish.GetParsedOrErrorData();
				Texture2D texture = parsedOrErrorData.GetTexture();
				Rectangle sourceRect = parsedOrErrorData.GetSourceRect();
				b.Draw(texture, Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(-124f, -284f + yOffset) + new Vector2(44f, 68f)), sourceRect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)playerStandingY / 10000f + 0.0001f + 0.06f);
				if (numberOfFishCaught > 1)
				{
					Utility.drawTinyDigits(numberOfFishCaught, b, Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(-120f, -284f + yOffset) + new Vector2(23f, 29f) * 4f), 3f, (float)playerStandingY / 10000f + 0.0001f + 0.061f, Color.White);
				}
				b.Draw(texture, Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(0f, -56f)), sourceRect, Color.White, (fishSize == -1 || whichFish.QualifiedItemId == "(O)800" || whichFish.QualifiedItemId == "(O)798" || whichFish.QualifiedItemId == "(O)149" || whichFish.QualifiedItemId == "(O)151") ? 0f : ((float)Math.PI * 3f / 4f), new Vector2(8f, 8f), 3f, SpriteEffects.None, (float)playerStandingY / 10000f + 0.002f + 0.06f);
				if (numberOfFishCaught > 1)
				{
					for (int i = 1; i < numberOfFishCaught; i++)
					{
						b.Draw(texture, Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(-(12 * i), -56f)), sourceRect, Color.White, (fishSize == -1 || whichFish.QualifiedItemId == "(O)800" || whichFish.QualifiedItemId == "(O)798" || whichFish.QualifiedItemId == "(O)149" || whichFish.QualifiedItemId == "(O)151") ? 0f : ((i == 2) ? ((float)Math.PI) : ((float)Math.PI * 4f / 5f)), new Vector2(8f, 8f), 3f, SpriteEffects.None, (float)playerStandingY / 10000f + 0.002f + 0.058f);
					}
				}
			}
			else
			{
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(-124f, -284f + yOffset) + new Vector2(44f, 68f)), new Rectangle(228, 408, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)playerStandingY / 10000f + 0.0001f + 0.06f);
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(0f, -56f)), new Rectangle(228, 408, 16, 16), Color.White, 0f, new Vector2(8f, 8f), 3f, SpriteEffects.None, (float)playerStandingY / 10000f + 0.002f + 0.06f);
			}
			string name = (fishIsObject ? whichFish.GetParsedOrErrorData().DisplayName : "???");
			b.DrawString(Game1.smallFont, name, Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(26f - Game1.smallFont.MeasureString(name).X / 2f, -278f + yOffset)), bossFish ? new Color(126, 61, 237) : Game1.textColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, (float)playerStandingY / 10000f + 0.002f + 0.06f);
			if (fishSize != -1)
			{
				b.DrawString(Game1.smallFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14082"), Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(20f, -214f + yOffset)), Game1.textColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, (float)playerStandingY / 10000f + 0.002f + 0.06f);
				b.DrawString(Game1.smallFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14083", (LocalizedContentManager.CurrentLanguageCode != 0) ? Math.Round((double)fishSize * 2.54) : ((double)fishSize)), Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(85f - Game1.smallFont.MeasureString(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14083", (LocalizedContentManager.CurrentLanguageCode != 0) ? Math.Round((double)fishSize * 2.54) : ((double)fishSize))).X / 2f, -179f + yOffset)), recordSize ? (Color.Blue * Math.Min(1f, yOffset / 8f + 1.5f)) : Game1.textColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, (float)playerStandingY / 10000f + 0.002f + 0.06f);
			}
		}
	}

	/// <summary>Get the color of the water which the bobber is submerged in.</summary>
	public Color GetWaterColor()
	{
		if (lastWaterColor.HasValue)
		{
			return lastWaterColor.Value;
		}
		GameLocation location = lastUser?.currentLocation ?? Game1.currentLocation;
		Vector2 tile = calculateBobberTile();
		if (tile != Vector2.Zero)
		{
			foreach (Building building in location.buildings)
			{
				if (building.isTileFishable(tile))
				{
					lastWaterColor = building.GetWaterColor(tile);
					if (lastWaterColor.HasValue)
					{
						return lastWaterColor.Value;
					}
					break;
				}
			}
		}
		lastWaterColor = location.waterColor.Value;
		return lastWaterColor.Value;
	}

	public override bool beginUsing(GameLocation location, int x, int y, Farmer who)
	{
		if (who.Stamina <= 1f && who.IsLocalPlayer)
		{
			if (!who.isEmoting)
			{
				who.doEmote(36);
			}
			who.CanMove = !Game1.eventUp;
			who.UsingTool = false;
			who.canReleaseTool = false;
			doneFishing(null);
			return true;
		}
		usedGamePadToCast = false;
		if (Game1.input.GetGamePadState().IsButtonDown(Buttons.X))
		{
			usedGamePadToCast = true;
		}
		bossFish = false;
		originalFacingDirection = who.FacingDirection;
		if (who.IsLocalPlayer || who.isFakeEventActor)
		{
			CastDirection = originalFacingDirection;
		}
		who.Halt();
		treasureCaught = false;
		showingTreasure = false;
		isFishing = false;
		hit = false;
		favBait = false;
		if (GetTackle().Count > 0)
		{
			bool foundTackle = false;
			foreach (Object item in GetTackle())
			{
				if (item != null)
				{
					foundTackle = true;
					break;
				}
			}
			hadBobber = foundTackle;
		}
		isNibbling = false;
		lastUser = who;
		lastWaterColor = null;
		isTimingCast = true;
		_totalMotionBufferIndex = 0;
		for (int i = 0; i < _totalMotionBuffer.Length; i++)
		{
			_totalMotionBuffer[i] = Vector2.Zero;
		}
		_totalMotion.Value = Vector2.Zero;
		_lastAppliedMotion = Vector2.Zero;
		who.UsingTool = true;
		whichFish = null;
		recastTimerMs = 0;
		who.canMove = false;
		fishCaught = false;
		doneWithAnimation = false;
		who.canReleaseTool = false;
		hasDoneFucntionYet = false;
		isReeling = false;
		pullingOutOfWater = false;
		castingPower = 0f;
		castingChosenCountdown = 0f;
		animations.Clear();
		sparklingText = null;
		setTimingCastAnimation(who);
		return true;
	}

	public void setTimingCastAnimation(Farmer who)
	{
		if (who.CurrentTool != null)
		{
			switch (who.FacingDirection)
			{
			case 0:
				who.FarmerSprite.setCurrentFrame(295);
				who.CurrentTool.Update(0, 0, who);
				break;
			case 1:
				who.FarmerSprite.setCurrentFrame(296);
				who.CurrentTool.Update(1, 0, who);
				break;
			case 2:
				who.FarmerSprite.setCurrentFrame(297);
				who.CurrentTool.Update(2, 0, who);
				break;
			case 3:
				who.FarmerSprite.setCurrentFrame(298);
				who.CurrentTool.Update(3, 0, who);
				break;
			}
		}
	}

	public void doneFishing(Farmer who, bool consumeBaitAndTackle = false)
	{
		doneFishingEvent.Fire(consumeBaitAndTackle);
	}

	private void doDoneFishing(bool consumeBaitAndTackle)
	{
		Farmer who = lastUser;
		if (consumeBaitAndTackle && who != null && who.IsLocalPlayer)
		{
			float consumeChance = 1f;
			if (hasEnchantmentOfType<PreservingEnchantment>())
			{
				consumeChance = 0.5f;
			}
			Object bait = GetBait();
			if (bait != null && Game1.random.NextDouble() < (double)consumeChance)
			{
				bait.Stack--;
				if (bait.Stack <= 0)
				{
					attachments[0] = null;
					Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14085"));
				}
			}
			int i = 1;
			foreach (Object tackle in GetTackle())
			{
				if (tackle != null && !lastCatchWasJunk && Game1.random.NextDouble() < (double)consumeChance)
				{
					if (tackle.QualifiedItemId == "(O)789")
					{
						break;
					}
					tackle.uses.Value++;
					if (tackle.uses.Value >= maxTackleUses)
					{
						attachments[i] = null;
						Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14086"));
					}
				}
				i++;
			}
		}
		if (who != null && who.IsLocalPlayer)
		{
			bobber.Set(Vector2.Zero);
		}
		isNibbling = false;
		fishCaught = false;
		isFishing = false;
		isReeling = false;
		isCasting = false;
		isTimingCast = false;
		treasureCaught = false;
		showingTreasure = false;
		doneWithAnimation = false;
		pullingOutOfWater = false;
		fromFishPond = false;
		numberOfFishCaught = 1;
		fishingBiteAccumulator = 0f;
		fishingNibbleAccumulator = 0f;
		timeUntilFishingBite = -1f;
		timeUntilFishingNibbleDone = -1f;
		bobberTimeAccumulator = 0f;
		if (chargeSound != null && chargeSound.IsPlaying && who.IsLocalPlayer)
		{
			chargeSound.Stop(AudioStopOptions.Immediate);
			chargeSound = null;
		}
		if (reelSound != null && reelSound.IsPlaying)
		{
			reelSound.Stop(AudioStopOptions.Immediate);
			reelSound = null;
		}
		if (who != null)
		{
			who.UsingTool = false;
			who.CanMove = true;
			who.completelyStopAnimatingOrDoingAction();
			if (who == Game1.player)
			{
				who.faceDirection(originalFacingDirection);
			}
		}
	}

	public static void doneWithCastingAnimation(Farmer who)
	{
		if (who.CurrentTool is FishingRod rod)
		{
			rod.doneWithAnimation = true;
			if (rod.hasDoneFucntionYet)
			{
				who.canReleaseTool = true;
				who.UsingTool = false;
				who.canMove = true;
				Farmer.canMoveNow(who);
			}
		}
	}

	public void castingEndFunction(Farmer who)
	{
		lastWaterColor = null;
		castedButBobberStillInAir = false;
		if (who != null)
		{
			float oldStamina = who.Stamina;
			DoFunction(who.currentLocation, (int)bobber.X, (int)bobber.Y, 1, who);
			who.lastClick = Vector2.Zero;
			reelSound?.Stop(AudioStopOptions.Immediate);
			reelSound = null;
			if (who.Stamina <= 0f && oldStamina > 0f)
			{
				who.doEmote(36);
			}
			if (!isFishing && doneWithAnimation)
			{
				castingEndEnableMovement();
			}
		}
	}

	private void castingEndEnableMovement()
	{
		castingEndEnableMovementEvent.Fire();
	}

	private void doCastingEndEnableMovement()
	{
		Farmer.canMoveNow(lastUser);
	}

	public override void tickUpdate(GameTime time, Farmer who)
	{
		lastUser = who;
		beginReelingEvent.Poll();
		putAwayEvent.Poll();
		startCastingEvent.Poll();
		pullFishFromWaterEvent.Poll();
		doneFishingEvent.Poll();
		castingEndEnableMovementEvent.Poll();
		if (recastTimerMs > 0 && who.IsLocalPlayer && who.freezePause <= 0)
		{
			if (Game1.input.GetMouseState().LeftButton == ButtonState.Pressed || Game1.didPlayerJustClickAtAll() || Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.useToolButton))
			{
				recastTimerMs -= time.ElapsedGameTime.Milliseconds;
				if (recastTimerMs <= 0)
				{
					recastTimerMs = 0;
					if (Game1.activeClickableMenu == null)
					{
						who.BeginUsingTool();
					}
				}
			}
			else
			{
				recastTimerMs = 0;
			}
		}
		if (isFishing && !Game1.shouldTimePass() && Game1.activeClickableMenu != null && !(Game1.activeClickableMenu is BobberBar))
		{
			return;
		}
		if (who.CurrentTool != null && who.CurrentTool.Equals(this) && who.UsingTool)
		{
			who.CanMove = false;
		}
		else if (Game1.currentMinigame == null && (!(who.CurrentTool is FishingRod) || !who.UsingTool))
		{
			if (chargeSound != null && chargeSound.IsPlaying && who.IsLocalPlayer)
			{
				chargeSound.Stop(AudioStopOptions.Immediate);
				chargeSound = null;
			}
			return;
		}
		for (int i = animations.Count - 1; i >= 0; i--)
		{
			if (animations[i].update(time))
			{
				animations.RemoveAt(i);
			}
		}
		if (sparklingText != null && sparklingText.update(time))
		{
			sparklingText = null;
		}
		if (castingChosenCountdown > 0f)
		{
			castingChosenCountdown -= time.ElapsedGameTime.Milliseconds;
			if (castingChosenCountdown <= 0f && who.CurrentTool != null)
			{
				switch (who.FacingDirection)
				{
				case 0:
					who.FarmerSprite.animateOnce(295, 1f, 1);
					who.CurrentTool.Update(0, 0, who);
					break;
				case 1:
					who.FarmerSprite.animateOnce(296, 1f, 1);
					who.CurrentTool.Update(1, 0, who);
					break;
				case 2:
					who.FarmerSprite.animateOnce(297, 1f, 1);
					who.CurrentTool.Update(2, 0, who);
					break;
				case 3:
					who.FarmerSprite.animateOnce(298, 1f, 1);
					who.CurrentTool.Update(3, 0, who);
					break;
				}
				if (who.FacingDirection == 1 || who.FacingDirection == 3)
				{
					float distance = Math.Max(128f, castingPower * (float)(getAddedDistance(who) + 4) * 64f);
					distance -= 8f;
					float gravity = 0.005f;
					float velocity = (float)((double)distance * Math.Sqrt(gravity / (2f * (distance + 96f))));
					float t = 2f * (velocity / gravity) + (float)((Math.Sqrt(velocity * velocity + 2f * gravity * 96f) - (double)velocity) / (double)gravity);
					Point playerPixel = who.StandingPixel;
					if (who.IsLocalPlayer)
					{
						bobber.Set(new Vector2((float)playerPixel.X + (float)((who.FacingDirection != 3) ? 1 : (-1)) * distance, playerPixel.Y));
					}
					Rectangle sourceRect = Game1.getSourceRectForStandardTileSheet(Game1.bobbersTexture, getBobberStyle(who), 16, 32);
					sourceRect.Height = 16;
					animations.Add(new TemporaryAnimatedSprite("TileSheets\\bobbers", sourceRect, t, 1, 0, who.Position + new Vector2(0f, -96f), flicker: false, flipped: false, (float)playerPixel.Y / 10000f, 0f, Color.White, 4f, 0f, 0f, (float)Game1.random.Next(-20, 20) / 100f)
					{
						motion = new Vector2((float)((who.FacingDirection != 3) ? 1 : (-1)) * velocity, 0f - velocity),
						acceleration = new Vector2(0f, gravity),
						endFunction = delegate
						{
							castingEndFunction(who);
						},
						timeBasedMotion = true,
						flipped = (who.FacingDirection == 1 && flipCurrentBobberWhenFacingRight())
					});
				}
				else
				{
					float distance = 0f - Math.Max(128f, castingPower * (float)(getAddedDistance(who) + 3) * 64f);
					float height = Math.Abs(distance - 64f);
					if (who.FacingDirection == 0)
					{
						distance = 0f - distance;
						height += 64f;
					}
					float gravity = 0.005f;
					float velocity = (float)Math.Sqrt(2f * gravity * height);
					float t = (float)(Math.Sqrt(2f * (height - distance) / gravity) + (double)(velocity / gravity));
					t *= 1.05f;
					if (who.FacingDirection == 0)
					{
						t *= 1.05f;
					}
					if (who.IsLocalPlayer)
					{
						Point playerPixel = who.StandingPixel;
						bobber.Set(new Vector2(playerPixel.X, (float)playerPixel.Y - distance));
					}
					Rectangle sourceRect = Game1.getSourceRectForStandardTileSheet(Game1.bobbersTexture, getBobberStyle(who), 16, 32);
					sourceRect.Height = 16;
					animations.Add(new TemporaryAnimatedSprite("TileSheets\\bobbers", sourceRect, t, 1, 0, who.Position + new Vector2(0f, -96f), flicker: false, flipped: false, bobber.Y / 10000f, 0f, Color.White, 4f, 0f, 0f, (float)Game1.random.Next(-20, 20) / 100f)
					{
						alphaFade = 0.0001f,
						motion = new Vector2(0f, 0f - velocity),
						acceleration = new Vector2(0f, gravity),
						endFunction = delegate
						{
							castingEndFunction(who);
						},
						timeBasedMotion = true
					});
				}
				_hasPlayerAdjustedBobber = false;
				castedButBobberStillInAir = true;
				isCasting = false;
				if (who.IsLocalPlayer)
				{
					who.playNearbySoundAll("cast");
				}
				if (who.IsLocalPlayer)
				{
					Game1.playSound("slowReel", 1600, out reelSound);
				}
			}
		}
		else if (!isTimingCast && castingChosenCountdown <= 0f)
		{
			who.jitterStrength = 0f;
		}
		if (isTimingCast)
		{
			castingPower = Math.Max(0f, Math.Min(1f, castingPower + castingTimerSpeed * (float)time.ElapsedGameTime.Milliseconds));
			if (who.IsLocalPlayer)
			{
				if (chargeSound == null || !chargeSound.IsPlaying)
				{
					Game1.playSound("SinWave", out chargeSound);
				}
				Game1.sounds.SetPitch(chargeSound, 2400f * castingPower);
			}
			if (castingPower == 1f || castingPower == 0f)
			{
				castingTimerSpeed = 0f - castingTimerSpeed;
			}
			who.armOffset.Y = 2f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
			who.jitterStrength = Math.Max(0f, castingPower - 0.5f);
			if (who.IsLocalPlayer && ((!usedGamePadToCast && Game1.input.GetMouseState().LeftButton == ButtonState.Released) || (usedGamePadToCast && Game1.options.gamepadControls && Game1.input.GetGamePadState().IsButtonUp(Buttons.X))) && Game1.areAllOfTheseKeysUp(Game1.GetKeyboardState(), Game1.options.useToolButton))
			{
				startCasting();
			}
		}
		else if (isReeling)
		{
			if (who.IsLocalPlayer && Game1.didPlayerJustClickAtAll())
			{
				if (Game1.isAnyGamePadButtonBeingPressed())
				{
					Game1.lastCursorMotionWasMouse = false;
				}
				switch (who.FacingDirection)
				{
				case 0:
					who.FarmerSprite.setCurrentSingleFrame(76, 32000);
					break;
				case 1:
					who.FarmerSprite.setCurrentSingleFrame(72, 100);
					break;
				case 2:
					who.FarmerSprite.setCurrentSingleFrame(75, 32000);
					break;
				case 3:
					who.FarmerSprite.setCurrentSingleFrame(72, 100, secondaryArm: false, flip: true);
					break;
				}
				who.armOffset.Y = (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
				who.jitterStrength = 1f;
			}
			else
			{
				switch (who.FacingDirection)
				{
				case 0:
					who.FarmerSprite.setCurrentSingleFrame(36, 32000);
					break;
				case 1:
					who.FarmerSprite.setCurrentSingleFrame(48, 100);
					break;
				case 2:
					who.FarmerSprite.setCurrentSingleFrame(66, 32000);
					break;
				case 3:
					who.FarmerSprite.setCurrentSingleFrame(48, 100, secondaryArm: false, flip: true);
					break;
				}
				who.stopJittering();
			}
			who.armOffset = new Vector2((float)Game1.random.Next(-10, 11) / 10f, (float)Game1.random.Next(-10, 11) / 10f);
			bobberTimeAccumulator += time.ElapsedGameTime.Milliseconds;
		}
		else if (isFishing)
		{
			if (who.IsLocalPlayer)
			{
				bobber.Y += (float)(0.11999999731779099 * Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0));
			}
			who.canReleaseTool = true;
			bobberTimeAccumulator += time.ElapsedGameTime.Milliseconds;
			switch (who.FacingDirection)
			{
			case 0:
				who.FarmerSprite.setCurrentFrame(44);
				break;
			case 1:
				who.FarmerSprite.setCurrentFrame(89);
				break;
			case 2:
				who.FarmerSprite.setCurrentFrame(70);
				break;
			case 3:
				who.FarmerSprite.setCurrentFrame(89, 0, 10, 1, flip: true, secondaryArm: false);
				break;
			}
			who.armOffset.Y = (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2) + (float)((who.FacingDirection == 1 || who.FacingDirection == 3) ? 1 : (-1));
			if (!who.IsLocalPlayer)
			{
				return;
			}
			if (timeUntilFishingBite != -1f)
			{
				fishingBiteAccumulator += time.ElapsedGameTime.Milliseconds;
				if (fishingBiteAccumulator > timeUntilFishingBite)
				{
					fishingBiteAccumulator = 0f;
					timeUntilFishingBite = -1f;
					isNibbling = true;
					if (hasEnchantmentOfType<AutoHookEnchantment>())
					{
						timePerBobberBob = 1f;
						timeUntilFishingNibbleDone = maxTimeToNibble;
						DoFunction(who.currentLocation, (int)bobber.X, (int)bobber.Y, 1, who);
						Rumble.rumble(0.95f, 200f);
						return;
					}
					who.PlayFishBiteChime();
					Rumble.rumble(0.75f, 250f);
					timeUntilFishingNibbleDone = maxTimeToNibble;
					Point playerPixel = who.StandingPixel;
					Game1.screenOverlayTempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(395, 497, 3, 8), new Vector2(playerPixel.X - Game1.viewport.X, playerPixel.Y - 128 - 8 - Game1.viewport.Y), flipped: false, 0.02f, Color.White)
					{
						scale = 5f,
						scaleChange = -0.01f,
						motion = new Vector2(0f, -0.5f),
						shakeIntensityChange = -0.005f,
						shakeIntensity = 1f
					});
					timePerBobberBob = 1f;
				}
			}
			if (timeUntilFishingNibbleDone != -1f && !hit)
			{
				fishingNibbleAccumulator += time.ElapsedGameTime.Milliseconds;
				if (fishingNibbleAccumulator > timeUntilFishingNibbleDone)
				{
					fishingNibbleAccumulator = 0f;
					timeUntilFishingNibbleDone = -1f;
					isNibbling = false;
					timeUntilFishingBite = calculateTimeUntilFishingBite(calculateBobberTile(), isFirstCast: false, who);
				}
			}
		}
		else if (who.UsingTool && castedButBobberStillInAir)
		{
			Vector2 motion = Vector2.Zero;
			if ((Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveDownButton) || (Game1.options.gamepadControls && (Game1.oldPadState.IsButtonDown(Buttons.DPadDown) || Game1.input.GetGamePadState().ThumbSticks.Left.Y < 0f))) && who.FacingDirection != 2 && who.FacingDirection != 0)
			{
				motion.Y += 4f;
				_hasPlayerAdjustedBobber = true;
			}
			if ((Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveRightButton) || (Game1.options.gamepadControls && (Game1.oldPadState.IsButtonDown(Buttons.DPadRight) || Game1.input.GetGamePadState().ThumbSticks.Left.X > 0f))) && who.FacingDirection != 1 && who.FacingDirection != 3)
			{
				motion.X += 2f;
				_hasPlayerAdjustedBobber = true;
			}
			if ((Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveUpButton) || (Game1.options.gamepadControls && (Game1.oldPadState.IsButtonDown(Buttons.DPadUp) || Game1.input.GetGamePadState().ThumbSticks.Left.Y > 0f))) && who.FacingDirection != 0 && who.FacingDirection != 2)
			{
				motion.Y -= 4f;
				_hasPlayerAdjustedBobber = true;
			}
			if ((Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveLeftButton) || (Game1.options.gamepadControls && (Game1.oldPadState.IsButtonDown(Buttons.DPadLeft) || Game1.input.GetGamePadState().ThumbSticks.Left.X < 0f))) && who.FacingDirection != 3 && who.FacingDirection != 1)
			{
				motion.X -= 2f;
				_hasPlayerAdjustedBobber = true;
			}
			if (!_hasPlayerAdjustedBobber)
			{
				Vector2 bobber_tile = calculateBobberTile();
				if (!who.currentLocation.isTileFishable((int)bobber_tile.X, (int)bobber_tile.Y))
				{
					if (who.FacingDirection == 3 || who.FacingDirection == 1)
					{
						int offset = 1;
						if (bobber_tile.Y % 1f < 0.5f)
						{
							offset = -1;
						}
						if (who.currentLocation.isTileFishable((int)bobber_tile.X, (int)bobber_tile.Y + offset))
						{
							motion.Y += (float)offset * 4f;
						}
						else if (who.currentLocation.isTileFishable((int)bobber_tile.X, (int)bobber_tile.Y - offset))
						{
							motion.Y -= (float)offset * 4f;
						}
					}
					if (who.FacingDirection == 0 || who.FacingDirection == 2)
					{
						int offset = 1;
						if (bobber_tile.X % 1f < 0.5f)
						{
							offset = -1;
						}
						if (who.currentLocation.isTileFishable((int)bobber_tile.X + offset, (int)bobber_tile.Y))
						{
							motion.X += (float)offset * 4f;
						}
						else if (who.currentLocation.isTileFishable((int)bobber_tile.X - offset, (int)bobber_tile.Y))
						{
							motion.X -= (float)offset * 4f;
						}
					}
				}
			}
			if (who.IsLocalPlayer)
			{
				bobber.Set(bobber.Value + motion);
				_totalMotion.Set(_totalMotion.Value + motion);
			}
			if (animations.Count <= 0)
			{
				return;
			}
			Vector2 applied_motion = Vector2.Zero;
			if (who.IsLocalPlayer)
			{
				applied_motion = _totalMotion.Value;
			}
			else
			{
				_totalMotionBuffer[_totalMotionBufferIndex] = _totalMotion.Value;
				for (int i = 0; i < _totalMotionBuffer.Length; i++)
				{
					applied_motion += _totalMotionBuffer[i];
				}
				applied_motion /= (float)_totalMotionBuffer.Length;
				_totalMotionBufferIndex = (_totalMotionBufferIndex + 1) % _totalMotionBuffer.Length;
			}
			animations[0].position -= _lastAppliedMotion;
			_lastAppliedMotion = applied_motion;
			animations[0].position += applied_motion;
		}
		else if (showingTreasure)
		{
			who.FarmerSprite.setCurrentSingleFrame(0, 32000);
		}
		else if (fishCaught)
		{
			if (!Game1.isFestival())
			{
				who.faceDirection(2);
				who.FarmerSprite.setCurrentFrame(84);
			}
			if (Game1.random.NextDouble() < 0.025)
			{
				who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(653, 858, 1, 1), 9999f, 1, 1, who.Position + new Vector2(Game1.random.Next(-3, 2) * 4, -32f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f + 0.002f, 0.04f, Color.LightBlue, 5f, 0f, 0f, 0f)
				{
					acceleration = new Vector2(0f, 0.25f)
				});
			}
			if (!who.IsLocalPlayer || (Game1.input.GetMouseState().LeftButton != ButtonState.Pressed && !Game1.didPlayerJustClickAtAll() && !Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.useToolButton)))
			{
				return;
			}
			who.playNearbySoundLocal("coin");
			if (!fromFishPond && Game1.IsSummer && whichFish.QualifiedItemId == "(O)138" && Game1.dayOfMonth >= 20 && Game1.dayOfMonth <= 21 && Game1.random.NextDouble() < 0.33 * (double)numberOfFishCaught)
			{
				gotTroutDerbyTag = true;
			}
			Item item;
			if (!treasureCaught && !gotTroutDerbyTag)
			{
				recastTimerMs = 200;
				item = CreateFish();
				bool fishIsObject = item.HasTypeObject();
				if ((item.Category == -4 || item.HasContextTag("counts_as_fish_catch")) && !fromFishPond)
				{
					Game1.player.stats.Increment("PreciseFishCaught", Math.Max(1, numberOfFishCaught));
				}
				if (item.QualifiedItemId == "(O)79" || item.QualifiedItemId == "(O)842")
				{
					item = who.currentLocation.tryToCreateUnseenSecretNote(who);
					if (item == null)
					{
						return;
					}
				}
				bool caughtFromFishPond = fromFishPond;
				who.completelyStopAnimatingOrDoingAction();
				doneFishing(who, !caughtFromFishPond);
				if (!Game1.isFestival() && !caughtFromFishPond && fishIsObject && who.team.specialOrders != null)
				{
					foreach (SpecialOrder specialOrder in who.team.specialOrders)
					{
						specialOrder.onFishCaught?.Invoke(who, item);
					}
				}
				if (!Game1.isFestival() && !who.addItemToInventoryBool(item))
				{
					Game1.activeClickableMenu = new ItemGrabMenu(new List<Item> { item }, this).setEssential(essential: true);
				}
				return;
			}
			fishCaught = false;
			showingTreasure = true;
			who.UsingTool = true;
			item = CreateFish();
			if ((item.Category == -4 || item.HasContextTag("counts_as_fish_catch")) && !fromFishPond)
			{
				Game1.player.stats.Increment("PreciseFishCaught", Math.Max(1, numberOfFishCaught));
			}
			if (who.team.specialOrders != null)
			{
				foreach (SpecialOrder specialOrder2 in who.team.specialOrders)
				{
					specialOrder2.onFishCaught?.Invoke(who, item);
				}
			}
			bool hadRoomForFish = who.addItemToInventoryBool(item);
			if (treasureCaught)
			{
				animations.Add(new TemporaryAnimatedSprite(goldenTreasure ? "LooseSprites\\Cursors_1_6" : "LooseSprites\\Cursors", goldenTreasure ? new Rectangle(256, 75, 32, 32) : new Rectangle(64, 1920, 32, 32), 500f, 1, 0, who.Position + new Vector2(-32f, -160f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f + 0.001f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -0.128f),
					timeBasedMotion = true,
					endFunction = openChestEndFunction,
					extraInfoForEndBehavior = ((!hadRoomForFish) ? item.Stack : 0),
					alpha = 0f,
					alphaFade = -0.002f
				});
			}
			else if (gotTroutDerbyTag)
			{
				animations.Add(new TemporaryAnimatedSprite("TileSheets\\Objects_2", new Rectangle(80, 16, 16, 16), 500f, 1, 0, who.Position + new Vector2(-8f, -128f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f + 0.001f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -0.128f),
					timeBasedMotion = true,
					endFunction = openChestEndFunction,
					extraInfoForEndBehavior = ((!hadRoomForFish) ? item.Stack : 0),
					alpha = 0f,
					alphaFade = -0.002f,
					id = 1074
				});
			}
		}
		else if (who.UsingTool && castedButBobberStillInAir && doneWithAnimation)
		{
			switch (who.FacingDirection)
			{
			case 0:
				who.FarmerSprite.setCurrentFrame(39);
				break;
			case 1:
				who.FarmerSprite.setCurrentFrame(89);
				break;
			case 2:
				who.FarmerSprite.setCurrentFrame(28);
				break;
			case 3:
				who.FarmerSprite.setCurrentFrame(89, 0, 10, 1, flip: true, secondaryArm: false);
				break;
			}
			who.armOffset.Y = (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
		}
		else if (!castedButBobberStillInAir && whichFish != null && animations.Count > 0 && animations[0].timer > 500f && !Game1.eventUp)
		{
			who.faceDirection(2);
			who.FarmerSprite.setCurrentFrame(57);
		}
	}

	/// <summary>Create a fish instance from the raw fields like <see cref="F:StardewValley.Tools.FishingRod.whichFish" />.</summary>
	private Item CreateFish()
	{
		Item fish = whichFish.CreateItemOrErrorItem(1, fishQuality);
		fish.SetFlagOnPickup = setFlagOnCatch;
		if (fish.HasTypeObject())
		{
			if (fish.QualifiedItemId == GameLocation.CAROLINES_NECKLACE_ITEM_QID)
			{
				if (fish is Object obj)
				{
					obj.questItem.Value = true;
				}
			}
			else if (numberOfFishCaught > 1 && fish.QualifiedItemId != "(O)79" && fish.QualifiedItemId != "(O)842")
			{
				fish.Stack = numberOfFishCaught;
			}
		}
		return fish;
	}

	private void startCasting()
	{
		startCastingEvent.Fire();
	}

	public void beginReeling()
	{
		isReeling = true;
	}

	private void doStartCasting()
	{
		Farmer who = lastUser;
		randomBobberStyle = -1;
		if (chargeSound != null && who.IsLocalPlayer)
		{
			chargeSound.Stop(AudioStopOptions.Immediate);
			chargeSound = null;
		}
		if (who.currentLocation != null)
		{
			if (who.IsLocalPlayer)
			{
				who.playNearbySoundLocal("button1");
				Rumble.rumble(0.5f, 150f);
			}
			who.UsingTool = true;
			isTimingCast = false;
			isCasting = true;
			castingChosenCountdown = 350f;
			who.armOffset.Y = 0f;
			if (castingPower > 0.99f && who.IsLocalPlayer)
			{
				Game1.screenOverlayTempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(545, 1921, 53, 19), 800f, 1, 0, Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(0f, -192f)), flicker: false, flipped: false, 1f, 0.01f, Color.White, 2f, 0f, 0f, 0f, local: true)
				{
					motion = new Vector2(0f, -4f),
					acceleration = new Vector2(0f, 0.2f),
					delayBeforeAnimationStart = 200
				});
				DelayedAction.playSoundAfterDelay("crit", 200);
			}
		}
	}

	public void openChestEndFunction(int remainingFish)
	{
		Farmer who = lastUser;
		if (gotTroutDerbyTag && !treasureCaught)
		{
			who.playNearbySoundLocal("discoverMineral");
			animations.Add(new TemporaryAnimatedSprite("TileSheets\\Objects_2", new Rectangle(80, 16, 16, 16), 800f, 1, 0, who.Position + new Vector2(-8f, -196f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f + 0.001f, 0f, Color.White, 4f, 0f, 0f, 0f)
			{
				endFunction = justGotDerbyTagEndFunction,
				extraInfoForEndBehavior = remainingFish,
				shakeIntensity = 0f
			});
			animations.AddRange(Utility.getTemporarySpritesWithinArea(new int[2] { 10, 11 }, new Rectangle((int)who.Position.X - 16, (int)who.Position.Y - 228 + 16, 32, 32), 4, Color.White));
		}
		else
		{
			who.playNearbySoundLocal("openChest");
			animations.Add(new TemporaryAnimatedSprite(goldenTreasure ? "LooseSprites\\Cursors_1_6" : "LooseSprites\\Cursors", goldenTreasure ? new Rectangle(256, 75, 32, 32) : new Rectangle(64, 1920, 32, 32), 200f, 4, 0, who.Position + new Vector2(-32f, -228f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f + 0.001f, 0f, Color.White, 4f, 0f, 0f, 0f)
			{
				endFunction = openTreasureMenuEndFunction,
				extraInfoForEndBehavior = remainingFish
			});
		}
		sparklingText = null;
	}

	public void justGotDerbyTagEndFunction(int remainingFish)
	{
		Farmer who = lastUser;
		who.UsingTool = false;
		doneFishing(who, consumeBaitAndTackle: true);
		Item tag = ItemRegistry.Create("(O)TroutDerbyTag");
		Item fish = null;
		if (remainingFish == 1)
		{
			fish = CreateFish();
		}
		Game1.playSound("coin");
		gotTroutDerbyTag = false;
		if (!who.addItemToInventoryBool(tag))
		{
			List<Item> items = new List<Item> { tag };
			if (fish != null)
			{
				items.Add(fish);
			}
			ItemGrabMenu itemGrabMenu = new ItemGrabMenu(items, this).setEssential(essential: true);
			itemGrabMenu.source = 3;
			Game1.activeClickableMenu = itemGrabMenu;
			who.completelyStopAnimatingOrDoingAction();
		}
		else if (fish != null && !who.addItemToInventoryBool(fish))
		{
			ItemGrabMenu itemGrabMenu2 = new ItemGrabMenu(new List<Item> { fish }, this).setEssential(essential: true);
			itemGrabMenu2.source = 3;
			Game1.activeClickableMenu = itemGrabMenu2;
			who.completelyStopAnimatingOrDoingAction();
		}
	}

	public override bool doesShowTileLocationMarker()
	{
		return false;
	}

	public void openTreasureMenuEndFunction(int remainingFish)
	{
		Farmer who = lastUser;
		who.gainExperience(5, 10 * (clearWaterDistance + 1));
		who.UsingTool = false;
		who.completelyStopAnimatingOrDoingAction();
		bool num = treasureCaught;
		doneFishing(who, consumeBaitAndTackle: true);
		List<Item> treasures = new List<Item>();
		if (remainingFish == 1)
		{
			treasures.Add(CreateFish());
		}
		float chance = 1f;
		if (num)
		{
			Game1.player.stats.Increment("FishingTreasures", 1);
			while (Game1.random.NextDouble() <= (double)chance)
			{
				chance *= (goldenTreasure ? 0.6f : 0.4f);
				if (Game1.IsSpring && !(who.currentLocation is Beach) && Game1.random.NextDouble() < 0.1)
				{
					treasures.Add(ItemRegistry.Create("(O)273", Game1.random.Next(2, 6) + ((Game1.random.NextDouble() < 0.25) ? 5 : 0)));
				}
				if (numberOfFishCaught > 1 && who.craftingRecipes.ContainsKey("Wild Bait") && Game1.random.NextBool())
				{
					treasures.Add(ItemRegistry.Create("(O)774", 2 + ((Game1.random.NextDouble() < 0.25) ? 2 : 0)));
				}
				if (Game1.random.NextDouble() <= 0.33 && who.team.SpecialOrderRuleActive("DROP_QI_BEANS"))
				{
					treasures.Add(ItemRegistry.Create("(O)890", Game1.random.Next(1, 3) + ((Game1.random.NextDouble() < 0.25) ? 2 : 0)));
				}
				while (Utility.tryRollMysteryBox(0.08 + Game1.player.team.AverageDailyLuck() / 5.0))
				{
					treasures.Add(ItemRegistry.Create((Game1.player.stats.Get(StatKeys.Mastery(2)) != 0) ? "(O)GoldenMysteryBox" : "(O)MysteryBox"));
				}
				if (Game1.player.stats.Get(StatKeys.Mastery(0)) != 0 && Game1.random.NextDouble() < 0.05)
				{
					treasures.Add(ItemRegistry.Create("(O)GoldenAnimalCracker"));
				}
				if (goldenTreasure && Game1.random.NextDouble() < 0.5)
				{
					switch (Game1.random.Next(13))
					{
					case 0:
						treasures.Add(ItemRegistry.Create("(O)337", Game1.random.Next(1, 6)));
						break;
					case 1:
						treasures.Add(ItemRegistry.Create("(O)SkillBook_" + Game1.random.Next(5)));
						break;
					case 2:
						treasures.Add(Utility.getRaccoonSeedForCurrentTimeOfYear(Game1.player, Game1.random, 8));
						break;
					case 3:
						treasures.Add(ItemRegistry.Create("(O)213"));
						break;
					case 4:
						treasures.Add(ItemRegistry.Create("(O)872", Game1.random.Next(3, 6)));
						break;
					case 5:
						treasures.Add(ItemRegistry.Create("(O)687"));
						break;
					case 6:
						treasures.Add(ItemRegistry.Create("(O)ChallengeBait", Game1.random.Next(3, 6)));
						break;
					case 7:
						treasures.Add(ItemRegistry.Create("(O)703", Game1.random.Next(3, 6)));
						break;
					case 8:
						treasures.Add(ItemRegistry.Create("(O)StardropTea"));
						break;
					case 9:
						treasures.Add(ItemRegistry.Create("(O)797"));
						break;
					case 10:
						treasures.Add(ItemRegistry.Create("(O)733"));
						break;
					case 11:
						treasures.Add(ItemRegistry.Create("(O)728"));
						break;
					case 12:
						treasures.Add(ItemRegistry.Create("(O)SonarBobber"));
						break;
					}
					continue;
				}
				switch (Game1.random.Next(4))
				{
				case 0:
				{
					if (clearWaterDistance >= 5 && Game1.random.NextDouble() < 0.03)
					{
						treasures.Add(new Object("386", Game1.random.Next(1, 3)));
						break;
					}
					List<int> possibles = new List<int>();
					if (clearWaterDistance >= 4)
					{
						possibles.Add(384);
					}
					if (clearWaterDistance >= 3 && (possibles.Count == 0 || Game1.random.NextDouble() < 0.6))
					{
						possibles.Add(380);
					}
					if (possibles.Count == 0 || Game1.random.NextDouble() < 0.6)
					{
						possibles.Add(378);
					}
					if (possibles.Count == 0 || Game1.random.NextDouble() < 0.6)
					{
						possibles.Add(388);
					}
					if (possibles.Count == 0 || Game1.random.NextDouble() < 0.6)
					{
						possibles.Add(390);
					}
					possibles.Add(382);
					Item treasure = ItemRegistry.Create(Game1.random.ChooseFrom(possibles).ToString(), Game1.random.Next(2, 7) * ((!(Game1.random.NextDouble() < 0.05 + (double)(int)who.luckLevel * 0.015)) ? 1 : 2));
					if (Game1.random.NextDouble() < 0.05 + (double)who.LuckLevel * 0.03)
					{
						treasure.Stack *= 2;
					}
					treasures.Add(treasure);
					break;
				}
				case 1:
					if (clearWaterDistance >= 4 && Game1.random.NextDouble() < 0.1 && who.FishingLevel >= 6)
					{
						treasures.Add(ItemRegistry.Create("(O)687"));
					}
					else if (Game1.random.NextDouble() < 0.25 && who.craftingRecipes.ContainsKey("Wild Bait"))
					{
						treasures.Add(ItemRegistry.Create("(O)774", 5 + ((Game1.random.NextDouble() < 0.25) ? 5 : 0)));
					}
					else if (Game1.random.NextDouble() < 0.11 && who.FishingLevel >= 6)
					{
						treasures.Add(ItemRegistry.Create("(O)SonarBobber"));
					}
					else if (who.FishingLevel >= 6)
					{
						treasures.Add(ItemRegistry.Create("(O)DeluxeBait", 5));
					}
					else
					{
						treasures.Add(ItemRegistry.Create("(O)685", 10));
					}
					break;
				case 2:
					if (Game1.random.NextDouble() < 0.1 && Game1.netWorldState.Value.LostBooksFound < 21 && who != null && who.hasOrWillReceiveMail("lostBookFound"))
					{
						treasures.Add(ItemRegistry.Create("(O)102"));
					}
					else if (who.archaeologyFound.Length > 0)
					{
						if (Game1.random.NextDouble() < 0.25 && who.FishingLevel > 1)
						{
							treasures.Add(ItemRegistry.Create("(O)" + Game1.random.Next(585, 588)));
						}
						else if (Game1.random.NextBool() && who.FishingLevel > 1)
						{
							treasures.Add(ItemRegistry.Create("(O)" + Game1.random.Next(103, 120)));
						}
						else
						{
							treasures.Add(ItemRegistry.Create("(O)535"));
						}
					}
					else
					{
						treasures.Add(ItemRegistry.Create("(O)382", Game1.random.Next(1, 3)));
					}
					break;
				case 3:
					switch (Game1.random.Next(3))
					{
					case 0:
					{
						Item treasure = ((clearWaterDistance >= 4) ? ItemRegistry.Create("(O)" + (537 + ((Game1.random.NextDouble() < 0.4) ? Game1.random.Next(-2, 0) : 0)), Game1.random.Next(1, 4)) : ((clearWaterDistance < 3) ? ItemRegistry.Create("(O)535", Game1.random.Next(1, 4)) : ItemRegistry.Create("(O)" + (536 + ((Game1.random.NextDouble() < 0.4) ? (-1) : 0)), Game1.random.Next(1, 4))));
						if (Game1.random.NextDouble() < 0.05 + (double)who.LuckLevel * 0.03)
						{
							treasure.Stack *= 2;
						}
						treasures.Add(treasure);
						break;
					}
					case 1:
					{
						if (who.FishingLevel < 2)
						{
							treasures.Add(ItemRegistry.Create("(O)382", Game1.random.Next(1, 4)));
							break;
						}
						Item treasure;
						if (clearWaterDistance >= 4)
						{
							treasures.Add(treasure = ItemRegistry.Create("(O)" + ((Game1.random.NextDouble() < 0.3) ? 82 : Game1.random.Choose(64, 60)), Game1.random.Next(1, 3)));
						}
						else if (clearWaterDistance >= 3)
						{
							treasures.Add(treasure = ItemRegistry.Create("(O)" + ((Game1.random.NextDouble() < 0.3) ? 84 : Game1.random.Choose(70, 62)), Game1.random.Next(1, 3)));
						}
						else
						{
							treasures.Add(treasure = ItemRegistry.Create("(O)" + ((Game1.random.NextDouble() < 0.3) ? 86 : Game1.random.Choose(66, 68)), Game1.random.Next(1, 3)));
						}
						if (Game1.random.NextDouble() < 0.028 * (double)((float)clearWaterDistance / 5f))
						{
							treasures.Add(treasure = ItemRegistry.Create("(O)72"));
						}
						if (Game1.random.NextDouble() < 0.05)
						{
							treasure.Stack *= 2;
						}
						break;
					}
					case 2:
					{
						if (who.FishingLevel < 2)
						{
							treasures.Add(new Object("770", Game1.random.Next(1, 4)));
							break;
						}
						float luckModifier = (1f + (float)who.DailyLuck) * ((float)clearWaterDistance / 5f);
						if (Game1.random.NextDouble() < 0.05 * (double)luckModifier && !who.specialItems.Contains("14"))
						{
							Item weapon = MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)14"), Game1.random);
							weapon.specialItem = true;
							treasures.Add(weapon);
						}
						if (Game1.random.NextDouble() < 0.05 * (double)luckModifier && !who.specialItems.Contains("51"))
						{
							Item weapon = MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)51"), Game1.random);
							weapon.specialItem = true;
							treasures.Add(weapon);
						}
						if (Game1.random.NextDouble() < 0.07 * (double)luckModifier)
						{
							switch (Game1.random.Next(3))
							{
							case 0:
								treasures.Add(new Ring((516 + ((Game1.random.NextDouble() < (double)((float)who.LuckLevel / 11f)) ? 1 : 0)).ToString()));
								break;
							case 1:
								treasures.Add(new Ring((518 + ((Game1.random.NextDouble() < (double)((float)who.LuckLevel / 11f)) ? 1 : 0)).ToString()));
								break;
							case 2:
								treasures.Add(new Ring(Game1.random.Next(529, 535).ToString()));
								break;
							}
						}
						if (Game1.random.NextDouble() < 0.02 * (double)luckModifier)
						{
							treasures.Add(ItemRegistry.Create("(O)166"));
						}
						if (who.FishingLevel > 5 && Game1.random.NextDouble() < 0.001 * (double)luckModifier)
						{
							treasures.Add(ItemRegistry.Create("(O)74"));
						}
						if (Game1.random.NextDouble() < 0.01 * (double)luckModifier)
						{
							treasures.Add(ItemRegistry.Create("(O)127"));
						}
						if (Game1.random.NextDouble() < 0.01 * (double)luckModifier)
						{
							treasures.Add(ItemRegistry.Create("(O)126"));
						}
						if (Game1.random.NextDouble() < 0.01 * (double)luckModifier)
						{
							treasures.Add(new Ring("527"));
						}
						if (Game1.random.NextDouble() < 0.01 * (double)luckModifier)
						{
							treasures.Add(ItemRegistry.Create("(B)" + Game1.random.Next(504, 514)));
						}
						if (Game1.MasterPlayer.mailReceived.Contains("Farm_Eternal") && Game1.random.NextDouble() < 0.01 * (double)luckModifier)
						{
							treasures.Add(ItemRegistry.Create("(O)928"));
						}
						if (treasures.Count == 1)
						{
							treasures.Add(ItemRegistry.Create("(O)72"));
						}
						if (Game1.player.stats.Get("FishingTreasures") > 3)
						{
							Random r = Utility.CreateRandom(Game1.player.stats.Get("FishingTreasures") * 27973, Game1.uniqueIDForThisGame);
							if (r.NextDouble() < 0.05 * (double)luckModifier)
							{
								treasures.Add(ItemRegistry.Create("(O)SkillBook_" + r.Next(5)));
								chance = 0f;
							}
						}
						break;
					}
					}
					break;
				}
			}
			if (treasures.Count == 0)
			{
				treasures.Add(ItemRegistry.Create("(O)685", Game1.random.Next(1, 4) * 5));
			}
			if (lastUser.hasQuest("98765") && Utility.GetDayOfPassiveFestival("DesertFestival") == 3 && !lastUser.Items.ContainsId("GoldenBobber", 1))
			{
				treasures.Clear();
				treasures.Add(ItemRegistry.Create("(O)GoldenBobber"));
			}
			if (Game1.random.NextDouble() < 0.25 && lastUser.stats.Get("Book_Roe") != 0)
			{
				Item fish = CreateFish();
				if (fish is Object)
				{
					ColoredObject roe = ItemRegistry.GetObjectTypeDefinition().CreateFlavoredRoe(fish as Object);
					roe.Stack = Game1.random.Next(1, 3);
					if (Game1.random.NextDouble() < 0.1 + lastUser.team.AverageDailyLuck())
					{
						roe.Stack++;
					}
					if (Game1.random.NextDouble() < 0.1 + lastUser.team.AverageDailyLuck())
					{
						roe.Stack *= 2;
					}
					treasures.Add(roe);
				}
			}
			if ((int)Game1.player.fishingLevel > 4 && Game1.player.stats.Get("FishingTreasures") > 2 && Game1.random.NextDouble() < 0.02 + ((!Game1.player.mailReceived.Contains("roeBookDropped")) ? ((double)Game1.player.stats.Get("FishingTreasures") * 0.001) : 0.001))
			{
				treasures.Add(ItemRegistry.Create("(O)Book_Roe"));
				Game1.player.mailReceived.Add("roeBookDropped");
			}
		}
		if (gotTroutDerbyTag)
		{
			treasures.Add(ItemRegistry.Create("(O)TroutDerbyTag"));
			gotTroutDerbyTag = false;
		}
		ItemGrabMenu itemGrabMenu = new ItemGrabMenu(treasures, this).setEssential(essential: true);
		itemGrabMenu.source = 3;
		Game1.activeClickableMenu = itemGrabMenu;
		who.completelyStopAnimatingOrDoingAction();
	}
}
