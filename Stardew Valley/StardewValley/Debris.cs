using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Network;

namespace StardewValley;

public class Debris : INetObject<NetFields>
{
	public enum DebrisType
	{
		/// <summary>The small 'chunks' that appear when hitting a tree with wood.</summary>
		CHUNKS = 0,
		LETTERS = 1,
		ARCHAEOLOGY = 3,
		OBJECT = 4,
		/// <summary>Sprites broken up into square chunks (i.e. the crumbs when you eat).</summary>
		SPRITECHUNKS = 5,
		RESOURCE = 6,
		NUMBERS = 7
	}

	public const int copperDebris = 0;

	public const int ironDebris = 2;

	public const int coalDebris = 4;

	public const int goldDebris = 6;

	public const int coinsDebris = 8;

	public const int iridiumDebris = 10;

	public const int woodDebris = 12;

	public const int stoneDebris = 14;

	public const int bigStoneDebris = 32;

	public const int bigWoodDebris = 34;

	public const int timesToBounce = 2;

	public const float gravity = 0.4f;

	public const float timeToWaitBeforeRemoval = 600f;

	public const int marginForChunkPickup = 64;

	public const int white = 10000;

	public const int green = 100001;

	public const int blue = 100002;

	public const int red = 100003;

	public const int yellow = 100004;

	public const int black = 100005;

	public const int charcoal = 100007;

	public const int gray = 100006;

	private float relativeXPosition;

	private readonly NetObjectShrinkList<Chunk> chunks = new NetObjectShrinkList<Chunk>();

	public readonly NetInt chunkType = new NetInt();

	public readonly NetInt sizeOfSourceRectSquares = new NetInt(8);

	private readonly NetInt netItemQuality = new NetInt();

	private readonly NetInt netChunkFinalYLevel = new NetInt();

	private readonly NetInt netChunkFinalYTarget = new NetInt();

	public float timeSinceDoneBouncing;

	public readonly NetFloat scale = new NetFloat(1f).Interpolated(interpolate: true, wait: true);

	protected NetBool _chunksMoveTowardsPlayer = new NetBool(value: false).Interpolated(interpolate: false, wait: false);

	public readonly NetLong DroppedByPlayerID = new NetLong().Interpolated(interpolate: false, wait: false);

	private bool movingUp;

	public readonly NetBool floppingFish = new NetBool();

	public bool isFishable;

	public bool movingFinalYLevel;

	public readonly NetEnum<DebrisType> debrisType = new NetEnum<DebrisType>(DebrisType.CHUNKS);

	public readonly NetString debrisMessage = new NetString("");

	public readonly NetColor nonSpriteChunkColor = new NetColor(Color.White);

	public readonly NetColor chunksColor = new NetColor();

	private float animationTimer;

	private int timeBeforeReturnToDroppingPlayer = 1200;

	public readonly NetString spriteChunkSheetName = new NetString();

	private Texture2D _spriteChunkSheet;

	public readonly NetString itemId = new NetString();

	private readonly NetRef<Item> netItem = new NetRef<Item>();

	public Character toHover;

	public readonly NetFarmerRef player = new NetFarmerRef();

	public int itemQuality
	{
		get
		{
			return netItemQuality;
		}
		set
		{
			netItemQuality.Value = value;
		}
	}

	public int chunkFinalYLevel
	{
		get
		{
			return netChunkFinalYLevel;
		}
		set
		{
			netChunkFinalYLevel.Value = value;
		}
	}

	public int chunkFinalYTarget
	{
		get
		{
			return netChunkFinalYTarget;
		}
		set
		{
			netChunkFinalYTarget.Value = value;
		}
	}

	public bool chunksMoveTowardPlayer
	{
		get
		{
			return _chunksMoveTowardsPlayer.Value;
		}
		set
		{
			_chunksMoveTowardsPlayer.Value = value;
		}
	}

	public Texture2D spriteChunkSheet
	{
		get
		{
			if (_spriteChunkSheet == null && spriteChunkSheetName != null)
			{
				_spriteChunkSheet = Game1.content.Load<Texture2D>(spriteChunkSheetName);
			}
			return _spriteChunkSheet;
		}
	}

	public Item item
	{
		get
		{
			return netItem.Value;
		}
		set
		{
			netItem.Value = value;
		}
	}

	public NetFields NetFields { get; } = new NetFields("Debris");


	public NetObjectShrinkList<Chunk> Chunks => chunks;

	public Debris()
	{
		InitNetFields();
	}

	public virtual void InitNetFields()
	{
		NetFields.SetOwner(this).AddField(chunks, "chunks").AddField(chunkType, "chunkType")
			.AddField(sizeOfSourceRectSquares, "sizeOfSourceRectSquares")
			.AddField(netItemQuality, "netItemQuality")
			.AddField(netChunkFinalYLevel, "netChunkFinalYLevel")
			.AddField(netChunkFinalYTarget, "netChunkFinalYTarget")
			.AddField(scale, "scale")
			.AddField(floppingFish, "floppingFish")
			.AddField(debrisType, "debrisType")
			.AddField(debrisMessage, "debrisMessage")
			.AddField(nonSpriteChunkColor, "nonSpriteChunkColor")
			.AddField(chunksColor, "chunksColor")
			.AddField(spriteChunkSheetName, "spriteChunkSheetName")
			.AddField(netItem, "netItem")
			.AddField(player.NetFields, "player.NetFields")
			.AddField(DroppedByPlayerID, "DroppedByPlayerID")
			.AddField(_chunksMoveTowardsPlayer, "_chunksMoveTowardsPlayer")
			.AddField(itemId, "itemId");
		player.Delayed(interpolationWait: false);
	}

	/// <summary>Construct an instance for resource/item debris.</summary>
	public Debris(int debris_type, Vector2 debrisOrigin, Vector2 playerPosition)
		: this(debris_type, 1, debrisOrigin, playerPosition)
	{
	}

	/// <summary>Construct an instance for resource/item type debris.</summary>
	public Debris(int resource_type, int numberOfChunks, Vector2 debrisOrigin, Vector2 playerPosition, float velocityMultiplyer = 1f)
		: this()
	{
		InitializeResource(resource_type);
		InitializeChunks(numberOfChunks, debrisOrigin, playerPosition, velocityMultiplyer);
	}

	/// <summary>Construct an instance for cosmetic "chunks".</summary>
	public Debris(int debrisType, int numberOfChunks, Vector2 debrisOrigin, Vector2 playerPosition, int groundLevel, Color? color = null)
		: this()
	{
		this.debrisType.Value = DebrisType.CHUNKS;
		chunkType.Value = debrisType;
		chunksColor.Value = color ?? getColorForDebris(debrisType);
		InitializeChunks(numberOfChunks, debrisOrigin, playerPosition);
	}

	/// <summary>Construct an instance for floating items.</summary>
	public Debris(string item_id, Vector2 debrisOrigin, Vector2 playerPosition)
		: this(item_id, 1, debrisOrigin, playerPosition)
	{
	}

	/// <summary>Construct an instance for floating items.</summary>
	public Debris(string item_id, int numberOfChunks, Vector2 debrisOrigin, Vector2 playerPosition, float velocityMultiplyer = 1f)
		: this()
	{
		InitializeItem(item_id);
		InitializeChunks(numberOfChunks, debrisOrigin, playerPosition, velocityMultiplyer);
	}

	public virtual void InitializeItem(string item_id)
	{
		if (debrisType.Value == DebrisType.CHUNKS)
		{
			debrisType.Value = DebrisType.OBJECT;
		}
		itemId.Value = item_id;
		ParsedItemData data = ItemRegistry.GetData(itemId.Value);
		if (item != null)
		{
			return;
		}
		if (data.HasTypeObject())
		{
			floppingFish.Value = data.Category == -4 && data.InternalName != "Mussel";
			isFishable = data.ObjectType == "Fish";
			if (data.ObjectType == "Arch")
			{
				debrisType.Value = DebrisType.ARCHAEOLOGY;
			}
		}
		else
		{
			item = ItemRegistry.Create(itemId);
		}
	}

	public virtual void InitializeResource(int item_id)
	{
		debrisType.Value = DebrisType.OBJECT;
		switch (item_id)
		{
		case 0:
		case 378:
			itemId.Value = "(O)378";
			debrisType.Value = DebrisType.RESOURCE;
			break;
		case 2:
		case 380:
			itemId.Value = "(O)380";
			debrisType.Value = DebrisType.RESOURCE;
			break;
		case 6:
		case 384:
			itemId.Value = "(O)384";
			debrisType.Value = DebrisType.RESOURCE;
			break;
		case 10:
		case 386:
			itemId.Value = "(O)386";
			debrisType.Value = DebrisType.RESOURCE;
			break;
		case 12:
		case 388:
			itemId.Value = "(O)388";
			debrisType.Value = DebrisType.RESOURCE;
			break;
		case 14:
		case 390:
			itemId.Value = "(O)390";
			debrisType.Value = DebrisType.RESOURCE;
			break;
		case 4:
		case 382:
			itemId.Value = "(O)382";
			debrisType.Value = DebrisType.RESOURCE;
			break;
		default:
			itemId.Value = "(O)" + item_id;
			break;
		}
		if (itemId.Value != null)
		{
			InitializeItem(itemId.Value);
		}
	}

	/// <summary>Construct an instance for floating items.</summary>
	public Debris(Item item, Vector2 debrisOrigin)
		: this()
	{
		this.item = item;
		item.resetState();
		InitializeItem(item.QualifiedItemId);
		InitializeChunks(1, debrisOrigin, Utility.PointToVector2(Game1.player.StandingPixel));
	}

	/// <summary>Construct an instance for floating items.</summary>
	public Debris(Item item, Vector2 debrisOrigin, Vector2 targetLocation)
		: this()
	{
		this.item = item;
		item.resetState();
		InitializeItem(item.QualifiedItemId);
		InitializeChunks(1, debrisOrigin, targetLocation);
	}

	/// <summary>Construct an instance for numbers.</summary>
	public Debris(int number, Vector2 debrisOrigin, Color messageColor, float scale, Character toHover)
		: this()
	{
		chunkType.Value = number;
		debrisType.Value = DebrisType.NUMBERS;
		nonSpriteChunkColor.Value = messageColor;
		InitializeChunks(1, debrisOrigin, Game1.player.Position);
		chunks[0].scale = scale;
		this.toHover = toHover;
		chunks[0].xVelocity.Value = Game1.random.Next(-1, 2);
		updateHoverPosition(chunks[0]);
	}

	/// <summary>Construct an instance for letters.</summary>
	public Debris(string message, int numberOfChunks, Vector2 debrisOrigin, Color messageColor, float scale, float rotation)
		: this()
	{
		debrisType.Value = DebrisType.LETTERS;
		debrisMessage.Value = message;
		nonSpriteChunkColor.Value = messageColor;
		InitializeChunks(numberOfChunks, debrisOrigin, Game1.player.Position);
		chunks[0].rotation = rotation;
		chunks[0].scale = scale;
	}

	/// <summary>Construct an instance for sprite chunks.</summary>
	public Debris(string spriteSheet, int numberOfChunks, Vector2 debrisOrigin)
		: this()
	{
		InitializeChunks(numberOfChunks, debrisOrigin, Game1.player.Position);
		debrisType.Value = DebrisType.SPRITECHUNKS;
		spriteChunkSheetName.Value = spriteSheet;
		for (int i = 0; i < chunks.Count; i++)
		{
			Chunk chunk = chunks[i];
			chunk.xSpriteSheet.Value = Game1.random.Next(0, 56);
			chunk.ySpriteSheet.Value = Game1.random.Next(0, 88);
			chunk.scale = 1f;
		}
	}

	/// <summary>Construct an instance for sprite chunks.</summary>
	public Debris(string spriteSheet, Rectangle sourceRect, int numberOfChunks, Vector2 debrisOrigin)
		: this()
	{
		InitializeChunks(numberOfChunks, debrisOrigin, Game1.player.Position);
		debrisType.Value = DebrisType.SPRITECHUNKS;
		spriteChunkSheetName.Value = spriteSheet;
		for (int i = 0; i < chunks.Count; i++)
		{
			Chunk chunk = chunks[i];
			chunk.xSpriteSheet.Value = Game1.random.Next(sourceRect.X, sourceRect.X + sourceRect.Width - 4);
			chunk.ySpriteSheet.Value = Game1.random.Next(sourceRect.Y, sourceRect.Y + sourceRect.Width - 4);
			chunk.scale = 1f;
		}
	}

	/// <summary>Construct an instance for sprite chunks.</summary>
	public Debris(string spriteSheet, Rectangle sourceRect, int numberOfChunks, Vector2 debrisOrigin, Vector2 playerPosition, int groundLevel, int sizeOfSourceRectSquares)
		: this()
	{
		InitializeChunks(numberOfChunks, debrisOrigin, Game1.player.Position, 0.6f);
		this.sizeOfSourceRectSquares.Value = sizeOfSourceRectSquares;
		debrisType.Value = DebrisType.SPRITECHUNKS;
		spriteChunkSheetName.Value = spriteSheet;
		for (int i = 0; i < chunks.Count; i++)
		{
			Chunk chunk = chunks[i];
			chunk.xSpriteSheet.Value = Game1.random.Next(2) * sizeOfSourceRectSquares + sourceRect.X;
			chunk.ySpriteSheet.Value = Game1.random.Next(2) * sizeOfSourceRectSquares + sourceRect.Y;
			chunk.rotationVelocity = (Game1.random.NextBool() ? ((float)(Math.PI / (double)Game1.random.Next(-32, -16))) : ((float)(Math.PI / (double)Game1.random.Next(16, 32))));
			chunk.xVelocity.Value *= 1.2f;
			chunk.yVelocity.Value *= 1.2f;
			chunk.scale = 4f;
		}
	}

	/// <summary>Construct an instance for sprite chunks.</summary>
	public Debris(string spriteSheet, Rectangle sourceRect, int numberOfChunks, Vector2 debrisOrigin, Vector2 playerPosition, int groundLevel)
		: this()
	{
		InitializeChunks(numberOfChunks, debrisOrigin, playerPosition);
		debrisType.Value = DebrisType.SPRITECHUNKS;
		spriteChunkSheetName.Value = spriteSheet;
		for (int i = 0; i < chunks.Count; i++)
		{
			Chunk chunk = chunks[i];
			chunk.xSpriteSheet.Value = Game1.random.Next(sourceRect.X, sourceRect.X + sourceRect.Width - 4);
			chunk.ySpriteSheet.Value = Game1.random.Next(sourceRect.Y, sourceRect.Y + sourceRect.Width - 4);
			chunk.scale = 1f;
		}
		chunkFinalYLevel = groundLevel;
	}

	public virtual bool isEssentialItem()
	{
		if (itemId.Value == "(O)73" || item?.QualifiedItemId == "(O)73")
		{
			return true;
		}
		if (item != null && !item.canBeTrashed())
		{
			return true;
		}
		return false;
	}

	public virtual bool collect(Farmer farmer, Chunk chunk = null)
	{
		if (debrisType.Value == DebrisType.ARCHAEOLOGY)
		{
			Game1.farmerFindsArtifact(itemId.Value);
		}
		else if (item != null)
		{
			Item tmpItem = item;
			item = null;
			if (!farmer.addItemToInventoryBool(tmpItem))
			{
				item = tmpItem;
				return false;
			}
		}
		else if ((debrisType.Value != 0 || chunkType.Value != 8) && !farmer.addItemToInventoryBool(ItemRegistry.Create(itemId.Value, 1, itemQuality)))
		{
			return false;
		}
		return true;
	}

	public static Color getColorForDebris(int type)
	{
		return type switch
		{
			12 => new Color(170, 106, 46), 
			100006 => Color.Gray, 
			100001 => Color.LightGreen, 
			100003 => Color.Red, 
			100004 => Color.Yellow, 
			100005 => Color.Black, 
			100007 => Color.DimGray, 
			100002 => Color.LightBlue, 
			_ => Color.White, 
		};
	}

	/// <summary>Initialize the chunks, called from all constructors.</summary>
	public void InitializeChunks(int numberOfChunks, Vector2 debrisOrigin, Vector2 playerPosition, float velocityMultiplyer = 1f)
	{
		if (itemId.Value != null || chunkType.Value != -1)
		{
			playerPosition -= (playerPosition - debrisOrigin) * 2f;
		}
		int minXVelocity;
		int maxXVelocity;
		int minYVelocity;
		int maxYVelocity;
		if (playerPosition.Y >= debrisOrigin.Y - 32f && playerPosition.Y <= debrisOrigin.Y + 32f)
		{
			chunkFinalYLevel = (int)debrisOrigin.Y - 32;
			minYVelocity = 230;
			maxYVelocity = 280;
			if (playerPosition.X < debrisOrigin.X)
			{
				minXVelocity = 20;
				maxXVelocity = 110;
			}
			else
			{
				minXVelocity = -110;
				maxXVelocity = -20;
			}
		}
		else if (playerPosition.Y < debrisOrigin.Y - 32f)
		{
			chunkFinalYLevel = (int)debrisOrigin.Y + (int)(32f * velocityMultiplyer);
			minYVelocity = 180;
			maxYVelocity = 230;
			minXVelocity = -50;
			maxXVelocity = 50;
		}
		else
		{
			movingFinalYLevel = true;
			chunkFinalYLevel = (int)debrisOrigin.Y - 1;
			chunkFinalYTarget = (int)debrisOrigin.Y - (int)(96f * velocityMultiplyer);
			movingUp = true;
			minYVelocity = 350;
			maxYVelocity = 400;
			minXVelocity = -50;
			maxXVelocity = 50;
		}
		debrisOrigin.X -= 32f;
		debrisOrigin.Y -= 32f;
		minXVelocity = (int)((float)minXVelocity * velocityMultiplyer);
		maxXVelocity = (int)((float)maxXVelocity * velocityMultiplyer);
		minYVelocity = (int)((float)minYVelocity * velocityMultiplyer);
		maxYVelocity = (int)((float)maxYVelocity * velocityMultiplyer);
		for (int i = 0; i < numberOfChunks; i++)
		{
			chunks.Add(new Chunk(debrisOrigin, (float)Game1.recentMultiplayerRandom.Next(minXVelocity, maxXVelocity) / 40f, (float)Game1.recentMultiplayerRandom.Next(minYVelocity, maxYVelocity) / 40f, Game1.recentMultiplayerRandom.Next(0, 2)));
		}
	}

	private Vector2 approximatePosition()
	{
		Vector2 total = default(Vector2);
		foreach (Chunk chunk in Chunks)
		{
			total += chunk.position.Value;
		}
		return total / Chunks.Count;
	}

	private bool playerInRange(Vector2 position, Farmer farmer)
	{
		if (isEssentialItem())
		{
			return true;
		}
		int applied_magnetic_radius = farmer.GetAppliedMagneticRadius();
		Point playerPixel = farmer.StandingPixel;
		if (Math.Abs(position.X + 32f - (float)playerPixel.X) <= (float)applied_magnetic_radius)
		{
			return Math.Abs(position.Y + 32f - (float)playerPixel.Y) <= (float)applied_magnetic_radius;
		}
		return false;
	}

	private Farmer findBestPlayer(GameLocation location)
	{
		if (location?.IsTemporary ?? false)
		{
			return Game1.player;
		}
		Vector2 position = approximatePosition();
		float bestDistance = float.MaxValue;
		Farmer bestFarmer = null;
		foreach (Farmer farmer in location.farmers)
		{
			if ((farmer.UniqueMultiplayerID != DroppedByPlayerID.Value || bestFarmer == null) && playerInRange(position, farmer))
			{
				float distance = (farmer.Position - position).LengthSquared();
				if (distance < bestDistance || (bestFarmer != null && bestFarmer.UniqueMultiplayerID == DroppedByPlayerID.Value))
				{
					bestFarmer = farmer;
					bestDistance = distance;
				}
			}
		}
		return bestFarmer;
	}

	public bool shouldControlThis(GameLocation location)
	{
		if (!Game1.IsMasterGame)
		{
			return location?.IsTemporary ?? false;
		}
		return true;
	}

	public bool updateChunks(GameTime time, GameLocation location)
	{
		if (chunks.Count == 0)
		{
			return true;
		}
		timeSinceDoneBouncing += time.ElapsedGameTime.Milliseconds;
		if (timeSinceDoneBouncing >= (floppingFish ? 2500f : ((debrisType.Value == DebrisType.SPRITECHUNKS || debrisType.Value == DebrisType.NUMBERS) ? 1800f : 600f)))
		{
			switch (debrisType.Value)
			{
			case DebrisType.LETTERS:
			case DebrisType.SPRITECHUNKS:
			case DebrisType.NUMBERS:
				return true;
			case DebrisType.CHUNKS:
				if ((int)chunkType != 8)
				{
					return true;
				}
				chunksMoveTowardPlayer = true;
				break;
			case DebrisType.ARCHAEOLOGY:
			case DebrisType.OBJECT:
			case DebrisType.RESOURCE:
				chunksMoveTowardPlayer = true;
				break;
			}
			timeSinceDoneBouncing = 0f;
		}
		if (!location.farmers.Any() && !location.IsTemporary)
		{
			return false;
		}
		Vector2 position = approximatePosition();
		Farmer farmer = player.Value;
		if (isEssentialItem() && shouldControlThis(location) && farmer == null)
		{
			farmer = findBestPlayer(location);
		}
		if (chunksMoveTowardPlayer)
		{
			if (timeBeforeReturnToDroppingPlayer > 0)
			{
				timeBeforeReturnToDroppingPlayer -= (int)time.ElapsedGameTime.TotalMilliseconds;
			}
			if (!isEssentialItem())
			{
				if (player.Value != null && player.Value == Game1.player && !playerInRange(position, player.Value))
				{
					player.Value = null;
					farmer = null;
				}
				if (shouldControlThis(location))
				{
					if (player.Value != null && player.Value.currentLocation != location)
					{
						player.Value = null;
						farmer = null;
					}
					if (farmer == null)
					{
						farmer = findBestPlayer(location);
					}
				}
				if (farmer != null && timeBeforeReturnToDroppingPlayer > 0 && farmer.UniqueMultiplayerID == DroppedByPlayerID.Value)
				{
					farmer = null;
				}
			}
		}
		bool anyCouldMove = false;
		for (int i = chunks.Count - 1; i >= 0; i--)
		{
			Chunk chunk = chunks[i];
			chunk.position.UpdateExtrapolation(chunk.getSpeed());
			if (chunk.alpha > 0.1f && (debrisType.Value == DebrisType.SPRITECHUNKS || debrisType.Value == DebrisType.NUMBERS) && timeSinceDoneBouncing > 600f)
			{
				chunk.alpha = (1800f - timeSinceDoneBouncing) / 1000f;
			}
			if (chunk.position.X < -128f || chunk.position.Y < -64f || chunk.position.X >= (float)(location.map.DisplayWidth + 64) || chunk.position.Y >= (float)(location.map.DisplayHeight + 64))
			{
				chunks.RemoveAt(i);
			}
			else
			{
				if (item?.QualifiedItemId == "(O)GoldCoin")
				{
					animationTimer += (int)time.ElapsedGameTime.TotalMilliseconds;
					if (animationTimer > 700f)
					{
						animationTimer = 0f;
						location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Rectangle(144, 249, 7, 7), 100f, 6, 1, Utility.getRandomPositionInThisRectangle(new Rectangle((int)chunk.position.X + 32 - 4, (int)chunk.position.Y + 32 - 4, 32, 28), Game1.random), flicker: false, flipped: false, ((float)(chunkFinalYLevel + 64 + 8) + (chunk.position.X + 1f) / 10000f) / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f));
					}
				}
				bool canMoveTowardPlayer = farmer != null;
				if (canMoveTowardPlayer)
				{
					switch (debrisType.Value)
					{
					case DebrisType.ARCHAEOLOGY:
					case DebrisType.OBJECT:
						if (item != null)
						{
							canMoveTowardPlayer = farmer.couldInventoryAcceptThisItem(item);
							break;
						}
						canMoveTowardPlayer = farmer.couldInventoryAcceptThisItem(itemId, 1, itemQuality);
						if (itemId == "(O)102" && (bool)farmer.hasMenuOpen)
						{
							canMoveTowardPlayer = false;
						}
						break;
					case DebrisType.RESOURCE:
						canMoveTowardPlayer = farmer.couldInventoryAcceptThisItem(itemId, 1);
						break;
					default:
						canMoveTowardPlayer = true;
						break;
					}
					anyCouldMove = anyCouldMove || canMoveTowardPlayer;
					if (canMoveTowardPlayer && shouldControlThis(location))
					{
						player.Value = farmer;
					}
				}
				if ((chunksMoveTowardPlayer || isFishable) && canMoveTowardPlayer && player.Value != null)
				{
					if (player.Value.IsLocalPlayer)
					{
						if (chunk.position.X < player.Value.Position.X - 12f)
						{
							chunk.xVelocity.Value = Math.Min(chunk.xVelocity.Value + 0.8f, 8f);
						}
						else if (chunk.position.X > player.Value.Position.X + 12f)
						{
							chunk.xVelocity.Value = Math.Max(chunk.xVelocity.Value - 0.8f, -8f);
						}
						int playerStandingY = player.Value.StandingPixel.Y;
						if (chunk.position.Y + 32f < (float)(playerStandingY - 12))
						{
							chunk.yVelocity.Value = Math.Max(chunk.yVelocity.Value - 0.8f, -8f);
						}
						else if (chunk.position.Y + 32f > (float)(playerStandingY + 12))
						{
							chunk.yVelocity.Value = Math.Min(chunk.yVelocity.Value + 0.8f, 8f);
						}
						chunk.position.X += chunk.xVelocity.Value;
						chunk.position.Y -= chunk.yVelocity.Value;
						Point playerPixel = player.Value.StandingPixel;
						if (Math.Abs(chunk.position.X + 32f - (float)playerPixel.X) <= 64f && Math.Abs(chunk.position.Y + 32f - (float)playerPixel.Y) <= 64f)
						{
							Item old = item;
							if (collect(player.Value, chunk))
							{
								if (Game1.debrisSoundInterval <= 0f)
								{
									Game1.debrisSoundInterval = 10f;
									if ((old == null || old.QualifiedItemId != "(O)73") && itemId != "(O)73")
									{
										location.localSound("coin");
									}
								}
								chunks.RemoveAt(i);
							}
						}
					}
				}
				else
				{
					if (debrisType.Value == DebrisType.NUMBERS)
					{
						updateHoverPosition(chunk);
					}
					chunk.position.X += chunk.xVelocity.Value;
					chunk.position.Y -= chunk.yVelocity.Value;
					if (movingFinalYLevel)
					{
						chunkFinalYLevel -= (int)Math.Ceiling(chunk.yVelocity.Value / 2f);
						if (chunkFinalYLevel <= chunkFinalYTarget)
						{
							chunkFinalYLevel = chunkFinalYTarget;
							movingFinalYLevel = false;
						}
					}
					if (chunk.bounces <= (floppingFish ? 65 : 2))
					{
						if (debrisType.Value == DebrisType.SPRITECHUNKS)
						{
							chunk.yVelocity.Value -= 0.25f;
						}
						else
						{
							chunk.yVelocity.Value -= 0.4f;
						}
					}
					bool destroyThisChunk = false;
					if (chunk.position.Y >= (float)chunkFinalYLevel && (bool)chunk.hasPassedRestingLineOnce && chunk.bounces <= (floppingFish ? 65 : 2))
					{
						Point tile_point = new Point((int)chunk.position.X / 64, chunkFinalYLevel / 64);
						if (Game1.currentLocation is IslandNorth && (debrisType.Value == DebrisType.ARCHAEOLOGY || debrisType.Value == DebrisType.OBJECT || debrisType.Value == DebrisType.RESOURCE || debrisType.Value == DebrisType.CHUNKS) && Game1.currentLocation.isTileOnMap(tile_point.X, tile_point.Y) && Game1.currentLocation.getTileIndexAt(tile_point, "Back") == -1)
						{
							chunkFinalYLevel += 48;
						}
						if (debrisType.Value != DebrisType.LETTERS && debrisType.Value != DebrisType.NUMBERS && debrisType.Value != DebrisType.SPRITECHUNKS && (debrisType.Value != 0 || (int)chunkType == 8) && shouldControlThis(location))
						{
							location.playSound("shiny4");
						}
						chunk.bounces++;
						if ((bool)floppingFish)
						{
							chunk.yVelocity.Value = Math.Abs(chunk.yVelocity.Value) * ((movingUp && chunk.bounces < 2) ? 0.6f : 0.9f);
							chunk.xVelocity.Value = (float)Game1.random.Next(-250, 250) / 100f;
						}
						else
						{
							chunk.yVelocity.Value = Math.Abs(chunk.yVelocity.Value * 2f / 3f);
							chunk.rotationVelocity = (Game1.random.NextBool() ? (chunk.rotationVelocity / 2f) : ((0f - chunk.rotationVelocity) * 2f / 3f));
							chunk.xVelocity.Value -= chunk.xVelocity.Value / 2f;
						}
						Vector2 chunkTile = new Vector2((int)((chunk.position.X + 32f) / 64f), (int)((chunk.position.Y + 32f) / 64f));
						if (debrisType.Value != DebrisType.LETTERS && debrisType.Value != DebrisType.SPRITECHUNKS && debrisType.Value != DebrisType.NUMBERS && location.doesTileSinkDebris((int)chunkTile.X, (int)chunkTile.Y, debrisType.Value))
						{
							destroyThisChunk = location.sinkDebris(this, chunkTile, chunk.position.Value);
						}
					}
					int tile_x = (int)((chunk.position.X + 32f) / 64f);
					int tile_y = (int)((chunk.position.Y + 32f) / 64f);
					if ((!chunk.hitWall && location.Map.RequireLayer("Buildings").Tiles[tile_x, tile_y] != null && location.doesTileHaveProperty(tile_x, tile_y, "Passable", "Buildings") == null) || location.Map.RequireLayer("Back").Tiles[tile_x, tile_y] == null)
					{
						chunk.xVelocity.Value = 0f - chunk.xVelocity.Value;
						chunk.hitWall = true;
					}
					if (chunk.position.Y < (float)chunkFinalYLevel)
					{
						chunk.hasPassedRestingLineOnce.Value = true;
					}
					if (chunk.bounces > (floppingFish ? 65 : 2))
					{
						chunk.yVelocity.Value = 0f;
						chunk.xVelocity.Value = 0f;
						chunk.rotationVelocity = 0f;
					}
					chunk.rotation += chunk.rotationVelocity;
					if (destroyThisChunk)
					{
						chunks.RemoveAt(i);
					}
				}
			}
		}
		if (!anyCouldMove && shouldControlThis(location))
		{
			player.Value = null;
		}
		if (chunks.Count == 0)
		{
			return true;
		}
		return false;
	}

	public void updateHoverPosition(Chunk chunk)
	{
		if (toHover != null)
		{
			relativeXPosition += chunk.xVelocity.Value;
			chunk.position.X = toHover.Position.X + 32f + relativeXPosition;
			chunk.scale = Math.Min(2f, Math.Max(1f, 0.9f + Math.Abs(chunk.position.Y - (float)chunkFinalYLevel) / 128f));
			chunkFinalYLevel = toHover.StandingPixel.Y + 8;
			if (timeSinceDoneBouncing > 250f)
			{
				chunk.alpha = Math.Max(0f, chunk.alpha - 0.033f);
			}
			if (!(toHover is Farmer) && !nonSpriteChunkColor.Equals(Color.Yellow) && !nonSpriteChunkColor.Equals(Color.Green))
			{
				nonSpriteChunkColor.R = (byte)Math.Max(Math.Min(255, 200 + (int)chunkType), Math.Min(Math.Min(255, 220 + (int)chunkType), 400.0 * Math.Sin((double)timeSinceDoneBouncing / (Math.PI * 256.0) + Math.PI / 12.0)));
				nonSpriteChunkColor.G = (byte)Math.Max(150 - (int)chunkType, Math.Min(255 - (int)chunkType, (nonSpriteChunkColor.R > 220) ? (300.0 * Math.Sin((double)timeSinceDoneBouncing / (Math.PI * 256.0) + Math.PI / 12.0)) : 0.0));
				nonSpriteChunkColor.B = (byte)Math.Max(0, Math.Min(255, (nonSpriteChunkColor.G > 200) ? (nonSpriteChunkColor.G - 20) : 0));
			}
		}
	}

	public static string getNameOfDebrisTypeFromIntId(int id)
	{
		switch (id)
		{
		case 0:
		case 1:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Debris.cs.621");
		case 2:
		case 3:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Debris.cs.622");
		case 4:
		case 5:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Debris.cs.623");
		case 6:
		case 7:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Debris.cs.624");
		case 8:
		case 9:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Debris.cs.625");
		case 10:
		case 11:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Debris.cs.626");
		case 12:
		case 13:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Debris.cs.627");
		case 14:
		case 15:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Debris.cs.628");
		case 28:
		case 29:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Debris.cs.629");
		case 30:
		case 31:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Debris.cs.630");
		default:
			return "???";
		}
	}
}
