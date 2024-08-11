using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.FarmAnimals;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Pathfinding;
using StardewValley.TerrainFeatures;
using StardewValley.TokenizableStrings;
using StardewValley.Tools;
using xTile.Dimensions;

namespace StardewValley;

public class FarmAnimal : Character
{
	public const byte eatGrassBehavior = 0;

	public const short newHome = 0;

	public const short happy = 1;

	public const short neutral = 2;

	public const short unhappy = 3;

	public const short hungry = 4;

	public const short disturbedByDog = 5;

	public const short leftOutAtNight = 6;

	public const double chancePerUpdateToChangeDirection = 0.007;

	public const byte fullnessValueOfGrass = 60;

	public const int noWarpTimerTime = 3000;

	public new const double chanceForSound = 0.002;

	public const double chanceToGoOutside = 0.002;

	public const int uniqueDownFrame = 16;

	public const int uniqueRightFrame = 18;

	public const int uniqueUpFrame = 20;

	public const int uniqueLeftFrame = 22;

	public const int pushAccumulatorTimeTillPush = 60;

	public const int timePerUniqueFrame = 500;

	/// <summary>The texture name to load if the animal's actual sprite can't be loaded.</summary>
	public const string ErrorTextureName = "Animals\\Error";

	/// <summary>The pixel size of sprites in the <see cref="F:StardewValley.FarmAnimal.ErrorTextureName" />.</summary>
	public const int ErrorSpriteSize = 16;

	public NetBool isSwimming = new NetBool();

	[XmlIgnore]
	public Vector2 hopOffset = new Vector2(0f, 0f);

	[XmlElement("currentProduce")]
	public readonly NetString currentProduce = new NetString();

	[XmlElement("friendshipTowardFarmer")]
	public readonly NetInt friendshipTowardFarmer = new NetInt();

	[XmlElement("skinID")]
	public readonly NetString skinID = new NetString();

	[XmlIgnore]
	public int pushAccumulator;

	[XmlIgnore]
	public int uniqueFrameAccumulator = -1;

	[XmlElement("age")]
	public readonly NetInt age = new NetInt();

	[XmlElement("daysOwned")]
	public readonly NetInt daysOwned = new NetInt(-1);

	[XmlElement("health")]
	public readonly NetInt health = new NetInt();

	[XmlElement("produceQuality")]
	public readonly NetInt produceQuality = new NetInt();

	[XmlElement("daysSinceLastLay")]
	public readonly NetInt daysSinceLastLay = new NetInt();

	[XmlElement("happiness")]
	public readonly NetInt happiness = new NetInt();

	[XmlElement("fullness")]
	public readonly NetInt fullness = new NetInt();

	[XmlElement("wasAutoPet")]
	public readonly NetBool wasAutoPet = new NetBool();

	[XmlElement("wasPet")]
	public readonly NetBool wasPet = new NetBool();

	[XmlElement("allowReproduction")]
	public readonly NetBool allowReproduction = new NetBool(value: true);

	[XmlElement("type")]
	public readonly NetString type = new NetString();

	[XmlElement("buildingTypeILiveIn")]
	public readonly NetString buildingTypeILiveIn = new NetString();

	[XmlElement("myID")]
	public readonly NetLong myID = new NetLong();

	[XmlElement("ownerID")]
	public readonly NetLong ownerID = new NetLong();

	[XmlElement("parentId")]
	public readonly NetLong parentId = new NetLong(-1L);

	[XmlIgnore]
	private readonly NetBuildingRef netHome = new NetBuildingRef();

	[XmlElement("hasEatenAnimalCracker")]
	public readonly NetBool hasEatenAnimalCracker = new NetBool();

	[XmlIgnore]
	public int noWarpTimer;

	[XmlIgnore]
	public int hitGlowTimer;

	[XmlIgnore]
	public int pauseTimer;

	[XmlElement("moodMessage")]
	public readonly NetInt moodMessage = new NetInt();

	[XmlElement("isEating")]
	public readonly NetBool isEating = new NetBool();

	[XmlIgnore]
	private readonly NetEvent1Field<int, NetInt> doFarmerPushEvent = new NetEvent1Field<int, NetInt>();

	[XmlIgnore]
	private readonly NetEvent0 doBuildingPokeEvent = new NetEvent0();

	[XmlIgnore]
	private readonly NetEvent0 doDiveEvent = new NetEvent0();

	private string _displayHouse;

	private string _displayType;

	public static int NumPathfindingThisTick = 0;

	public static int MaxPathfindingPerTick = 1;

	[XmlIgnore]
	public int nextRipple;

	[XmlIgnore]
	public int nextFollowDirectionChange;

	protected FarmAnimal _followTarget;

	protected Point? _followTargetPosition;

	protected float _nextFollowTargetScan = 1f;

	[XmlIgnore]
	public int bobOffset;

	[XmlIgnore]
	protected Vector2 _swimmingVelocity = Vector2.Zero;

	[XmlIgnore]
	public static HashSet<Grass> reservedGrass = new HashSet<Grass>();

	[XmlIgnore]
	public Grass foundGrass;

	/// <summary>The building within which the animal is normally housed, if any.</summary>
	[XmlIgnore]
	public Building home
	{
		get
		{
			return netHome.Value;
		}
		set
		{
			netHome.Value = value;
		}
	}

	[XmlIgnore]
	public string displayHouse
	{
		get
		{
			if (_displayHouse == null)
			{
				FarmAnimalData data = GetAnimalData();
				if (data != null)
				{
					_displayHouse = (Game1.buildingData.TryGetValue(data.House, out var buildingData) ? TokenParser.ParseText(buildingData.Name) : data.House);
				}
				else
				{
					_displayHouse = buildingTypeILiveIn.Value;
				}
			}
			return _displayHouse;
		}
		set
		{
			_displayHouse = value;
		}
	}

	[XmlIgnore]
	public string displayType
	{
		get
		{
			if (_displayType == null)
			{
				_displayType = TokenParser.ParseText(GetAnimalData()?.DisplayName);
			}
			return _displayType;
		}
		set
		{
			_displayType = value;
		}
	}

	public override string displayName
	{
		get
		{
			return base.Name;
		}
		set
		{
		}
	}

	/// <summary>Get whether the farm animal is currently inside their home building.</summary>
	[MemberNotNullWhen(true, "home")]
	public bool IsHome
	{
		[MemberNotNullWhen(true, "home")]
		get
		{
			Building building = home;
			if (building == null)
			{
				return false;
			}
			return building.GetIndoors()?.animals.ContainsKey(myID.Value) == true;
		}
	}

	public FarmAnimal()
	{
	}

	protected override void initNetFields()
	{
		bobOffset = Game1.random.Next(0, 1000);
		base.initNetFields();
		base.NetFields.AddField(currentProduce, "currentProduce").AddField(friendshipTowardFarmer, "friendshipTowardFarmer").AddField(age, "age")
			.AddField(health, "health")
			.AddField(produceQuality, "produceQuality")
			.AddField(daysSinceLastLay, "daysSinceLastLay")
			.AddField(happiness, "happiness")
			.AddField(fullness, "fullness")
			.AddField(wasPet, "wasPet")
			.AddField(wasAutoPet, "wasAutoPet")
			.AddField(allowReproduction, "allowReproduction")
			.AddField(type, "type")
			.AddField(buildingTypeILiveIn, "buildingTypeILiveIn")
			.AddField(myID, "myID")
			.AddField(ownerID, "ownerID")
			.AddField(parentId, "parentId")
			.AddField(netHome.NetFields, "netHome.NetFields")
			.AddField(moodMessage, "moodMessage")
			.AddField(isEating, "isEating")
			.AddField(doFarmerPushEvent, "doFarmerPushEvent")
			.AddField(doBuildingPokeEvent, "doBuildingPokeEvent")
			.AddField(isSwimming, "isSwimming")
			.AddField(doDiveEvent.NetFields, "doDiveEvent.NetFields")
			.AddField(daysOwned, "daysOwned")
			.AddField(skinID, "skinID")
			.AddField(hasEatenAnimalCracker, "hasEatenAnimalCracker");
		position.Field.AxisAlignedMovement = true;
		doFarmerPushEvent.onEvent += doFarmerPush;
		doBuildingPokeEvent.onEvent += doBuildingPoke;
		doDiveEvent.onEvent += doDive;
		skinID.fieldChangeVisibleEvent += delegate
		{
			if (Game1.gameMode != 6)
			{
				ReloadTextureIfNeeded();
			}
		};
		isSwimming.fieldChangeVisibleEvent += delegate
		{
			if (isSwimming.Value)
			{
				position.Field.AxisAlignedMovement = false;
			}
			else
			{
				position.Field.AxisAlignedMovement = true;
			}
		};
		name.FilterStringEvent += Utility.FilterDirtyWords;
	}

	public FarmAnimal(string type, long id, long ownerID)
		: base(null, new Vector2(64 * Game1.random.Next(2, 9), 64 * Game1.random.Next(4, 8)), 2, type)
	{
		this.ownerID.Value = ownerID;
		health.Value = 3;
		myID.Value = id;
		if (type == "Dairy Cow")
		{
			type = "Brown Cow";
		}
		this.type.Value = type;
		base.Name = Dialogue.randomName();
		displayName = name;
		happiness.Value = 255;
		fullness.Value = 255;
		_nextFollowTargetScan = Utility.RandomFloat(1f, 3f);
		ReloadTextureIfNeeded(forceReload: true);
		FarmAnimalData data = GetAnimalData();
		if (data == null)
		{
			Game1.log.Warn("Constructed farm animal type '" + type + "' which has no entry in Data/FarmAnimals.");
		}
		buildingTypeILiveIn.Value = data?.House;
		if (data?.Skins == null)
		{
			return;
		}
		Random random = Utility.CreateRandom(id);
		float totalWeight = 1f;
		foreach (FarmAnimalSkin skin in data.Skins)
		{
			totalWeight += skin.Weight;
		}
		totalWeight = Utility.RandomFloat(0f, totalWeight, random);
		foreach (FarmAnimalSkin skin in data.Skins)
		{
			totalWeight -= skin.Weight;
			if (totalWeight <= 0f)
			{
				skinID.Value = skin.Id;
				break;
			}
		}
	}

	/// <summary>Reload the texture if the asset name should change based on the current animal state and data.</summary>
	/// <param name="forceReload">Whether to reload the texture even if the texture path hasn't changed.</param>
	public void ReloadTextureIfNeeded(bool forceReload = false)
	{
		if (Sprite == null || forceReload)
		{
			FarmAnimalData data = GetAnimalData();
			string texturePath;
			int spriteWidth;
			int spriteHeight;
			if (data != null)
			{
				texturePath = GetTexturePath(data);
				spriteWidth = data.SpriteWidth;
				spriteHeight = data.SpriteHeight;
			}
			else
			{
				texturePath = "Animals\\Error";
				spriteWidth = 16;
				spriteHeight = 16;
			}
			if (!Game1.content.DoesAssetExist<Texture2D>(texturePath))
			{
				Game1.log.Warn($"Farm animal '{type.Value}' failed to load texture path '{texturePath}': asset doesn't exist. Defaulting to error texture.");
				texturePath = "Animals\\Error";
				spriteWidth = 16;
				spriteHeight = 16;
			}
			Sprite = new AnimatedSprite(texturePath, 0, spriteWidth, spriteHeight)
			{
				textureUsesFlippedRightForLeft = (data?.UseFlippedRightForLeft ?? false)
			};
			ValidateSpritesheetSize();
		}
		else
		{
			string texturePath = GetTexturePath();
			if (Sprite.textureName != texturePath)
			{
				Sprite.LoadTexture(texturePath);
			}
		}
	}

	public string GetTexturePath()
	{
		return GetTexturePath(GetAnimalData());
	}

	public virtual string GetTexturePath(FarmAnimalData data)
	{
		string texturePath = "Animals\\" + type;
		if (data != null)
		{
			FarmAnimalSkin skin = null;
			if (skinID.Value != null && data.Skins != null)
			{
				foreach (FarmAnimalSkin animalSkin in data.Skins)
				{
					if (skinID.Value == animalSkin.Id)
					{
						skin = animalSkin;
						break;
					}
				}
			}
			if (skin != null && skin.Texture != null)
			{
				texturePath = skin.Texture;
			}
			else if (data.Texture != null)
			{
				texturePath = data.Texture;
			}
			if (currentProduce.Value == null)
			{
				if (skin != null && skin.HarvestedTexture != null)
				{
					texturePath = skin.HarvestedTexture;
				}
				else if (data.HarvestedTexture != null)
				{
					texturePath = data.HarvestedTexture;
				}
			}
			if (isBaby())
			{
				if (skin != null && skin.BabyTexture != null)
				{
					texturePath = skin.BabyTexture;
				}
				else if (data.BabyTexture != null)
				{
					texturePath = data.BabyTexture;
				}
			}
		}
		return texturePath;
	}

	public static FarmAnimalData GetAnimalDataFromEgg(Item eggItem, GameLocation location)
	{
		if (!TryGetAnimalDataFromEgg(eggItem, location, out var _, out var data))
		{
			return null;
		}
		return data;
	}

	public static bool TryGetAnimalDataFromEgg(Item eggItem, GameLocation location, out string id, out FarmAnimalData data)
	{
		if (!eggItem.HasTypeObject())
		{
			id = null;
			data = null;
			return false;
		}
		List<string> validOccupantTypes = location?.GetContainingBuilding()?.GetData()?.ValidOccupantTypes;
		foreach (KeyValuePair<string, FarmAnimalData> pair in Game1.farmAnimalData)
		{
			FarmAnimalData animalData = pair.Value;
			if (animalData.EggItemIds != null && animalData.EggItemIds.Count != 0 && (validOccupantTypes == null || validOccupantTypes.Contains(animalData.House)) && animalData.EggItemIds.Contains(eggItem.ItemId))
			{
				id = pair.Key;
				data = animalData;
				return true;
			}
		}
		id = null;
		data = null;
		return false;
	}

	public virtual FarmAnimalData GetAnimalData()
	{
		if (!Game1.farmAnimalData.TryGetValue(type.Value, out var animalData))
		{
			return null;
		}
		return animalData;
	}

	/// <summary>Get the translated display name for a farm animal from its data, if any.</summary>
	/// <param name="id">The animal type ID in <c>Data/FarmAnimals</c>.</param>
	/// <param name="forShop">Whether to get the shop name, if applicable.</param>
	public static string GetDisplayName(string id, bool forShop = false)
	{
		if (!Game1.farmAnimalData.TryGetValue(id, out var data))
		{
			return null;
		}
		return TokenParser.ParseText(forShop ? (data.ShopDisplayName ?? data.DisplayName) : data.DisplayName);
	}

	/// <summary>Get the translated shop description for a farm animal from its data, if any.</summary>
	/// <param name="id">The animal type ID in <c>Data/FarmAnimals</c>.</param>
	public static string GetShopDescription(string id)
	{
		if (!Game1.farmAnimalData.TryGetValue(id, out var data))
		{
			return null;
		}
		return TokenParser.ParseText(data.ShopDescription);
	}

	public string shortDisplayType()
	{
		switch (LocalizedContentManager.CurrentLanguageCode)
		{
		case LocalizedContentManager.LanguageCode.en:
			return ArgUtility.SplitBySpace(displayType).Last();
		case LocalizedContentManager.LanguageCode.ja:
			if (!displayType.Contains("トリ"))
			{
				if (!displayType.Contains("ウシ"))
				{
					if (!displayType.Contains("ブタ"))
					{
						return displayType;
					}
					return "ブタ";
				}
				return "ウシ";
			}
			return "トリ";
		case LocalizedContentManager.LanguageCode.ru:
			if (!displayType.ToLower().Contains("курица"))
			{
				if (!displayType.ToLower().Contains("корова"))
				{
					return displayType;
				}
				return "Корова";
			}
			return "Курица";
		case LocalizedContentManager.LanguageCode.zh:
			if (!displayType.Contains('鸡'))
			{
				if (!displayType.Contains('牛'))
				{
					if (!displayType.Contains('猪'))
					{
						return displayType;
					}
					return "猪";
				}
				return "牛";
			}
			return "鸡";
		case LocalizedContentManager.LanguageCode.pt:
		case LocalizedContentManager.LanguageCode.es:
			return ArgUtility.SplitBySpaceAndGet(displayType, 0);
		case LocalizedContentManager.LanguageCode.de:
			return ArgUtility.SplitBySpace(displayType).Last().Split('-')
				.Last();
		default:
			return displayType;
		}
	}

	public Microsoft.Xna.Framework.Rectangle GetHarvestBoundingBox()
	{
		Vector2 position = base.Position;
		return new Microsoft.Xna.Framework.Rectangle((int)(position.X + (float)(Sprite.getWidth() * 4 / 2) - 32f + 4f), (int)(position.Y + (float)(Sprite.getHeight() * 4) - 64f - 24f), 56, 72);
	}

	public Microsoft.Xna.Framework.Rectangle GetCursorPetBoundingBox()
	{
		Vector2 position = base.Position;
		FarmAnimalData animalData = GetAnimalData();
		if (animalData != null)
		{
			int width;
			int height;
			if (isBaby())
			{
				if (FacingDirection == 0 || FacingDirection == 2 || Sprite.currentFrame >= 12)
				{
					width = (int)(animalData.BabyUpDownPetHitboxTileSize.X * 64f);
					height = (int)(animalData.BabyUpDownPetHitboxTileSize.Y * 64f);
				}
				else
				{
					width = (int)(animalData.BabyLeftRightPetHitboxTileSize.X * 64f);
					height = (int)(animalData.BabyLeftRightPetHitboxTileSize.Y * 64f);
				}
			}
			else if (FacingDirection == 0 || FacingDirection == 2 || Sprite.currentFrame >= 12)
			{
				width = (int)(animalData.UpDownPetHitboxTileSize.X * 64f);
				height = (int)(animalData.UpDownPetHitboxTileSize.Y * 64f);
			}
			else
			{
				width = (int)(animalData.LeftRightPetHitboxTileSize.X * 64f);
				height = (int)(animalData.LeftRightPetHitboxTileSize.Y * 64f);
			}
			return new Microsoft.Xna.Framework.Rectangle((int)(base.Position.X + (float)(Sprite.getWidth() * 4 / 2) - (float)(width / 2)), (int)(base.Position.Y - 24f + (float)(Sprite.getHeight() * 4) - (float)height), width, height);
		}
		return new Microsoft.Xna.Framework.Rectangle((int)(position.X + (float)(Sprite.getWidth() * 4 / 2) - 32f + 4f), (int)(position.Y + (float)(Sprite.getHeight() * 4) - 64f - 24f), 56, 72);
	}

	public override Microsoft.Xna.Framework.Rectangle GetBoundingBox()
	{
		Vector2 position = base.Position;
		return new Microsoft.Xna.Framework.Rectangle((int)(position.X + (float)(Sprite.getWidth() * 4 / 2) - 32f + 8f), (int)(position.Y + (float)(Sprite.getHeight() * 4) - 64f + 8f), 48, 48);
	}

	public void reload(Building home)
	{
		this.home = home;
		ReloadTextureIfNeeded();
	}

	public int GetDaysOwned()
	{
		if (daysOwned.Value < 0)
		{
			daysOwned.Value = age.Value;
		}
		return daysOwned.Value;
	}

	public void pet(Farmer who, bool is_auto_pet = false)
	{
		if (!is_auto_pet)
		{
			if (who.FarmerSprite.PauseForSingleAnimation)
			{
				return;
			}
			who.Halt();
			who.faceGeneralDirection(base.Position, 0, opposite: false, useTileCalculations: false);
			if (Game1.timeOfDay >= 1900 && !isMoving())
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\FarmAnimals:TryingToSleep", displayName));
				return;
			}
			Halt();
			Sprite.StopAnimation();
			uniqueFrameAccumulator = -1;
			switch (Game1.player.FacingDirection)
			{
			case 0:
				Sprite.currentFrame = 0;
				break;
			case 1:
				Sprite.currentFrame = 12;
				break;
			case 2:
				Sprite.currentFrame = 8;
				break;
			case 3:
				Sprite.currentFrame = 4;
				break;
			}
			if (!hasEatenAnimalCracker.Value && who.ActiveObject?.QualifiedItemId == "(O)GoldenAnimalCracker")
			{
				if ((!(GetAnimalData()?.CanEatGoldenCrackers)) ?? false)
				{
					Game1.playSound("cancel");
					doEmote(8);
					return;
				}
				hasEatenAnimalCracker.Value = true;
				Game1.playSound("give_gift");
				doEmote(56);
				Game1.player.reduceActiveItemByOne();
				return;
			}
		}
		else if (wasAutoPet.Value)
		{
			return;
		}
		if (!wasPet)
		{
			if (!is_auto_pet)
			{
				wasPet.Value = true;
			}
			int auto_pet_reduction = 7;
			if (wasAutoPet.Value)
			{
				friendshipTowardFarmer.Value = Math.Min(1000, (int)friendshipTowardFarmer + auto_pet_reduction);
			}
			else if (is_auto_pet)
			{
				friendshipTowardFarmer.Value = Math.Min(1000, (int)friendshipTowardFarmer + (15 - auto_pet_reduction));
			}
			else
			{
				friendshipTowardFarmer.Value = Math.Min(1000, (int)friendshipTowardFarmer + 15);
			}
			if (is_auto_pet)
			{
				wasAutoPet.Value = true;
			}
			FarmAnimalData data = GetAnimalData();
			int happinessDrain = data?.HappinessDrain ?? 0;
			if (!is_auto_pet)
			{
				if (data != null && data.ProfessionForHappinessBoost >= 0 && who.professions.Contains(data.ProfessionForHappinessBoost))
				{
					friendshipTowardFarmer.Value = Math.Min(1000, (int)friendshipTowardFarmer + 15);
					happiness.Value = (byte)Math.Min(255, (int)happiness + Math.Max(5, 30 + happinessDrain));
				}
				int emote_index = 20;
				if (wasAutoPet.Value)
				{
					emote_index = 32;
				}
				doEmote(((int)moodMessage == 4) ? 12 : emote_index);
			}
			happiness.Value = (byte)Math.Min(255, (int)happiness + Math.Max(5, 30 + happinessDrain));
			if (!is_auto_pet)
			{
				makeSound();
				who.gainExperience(0, 5);
			}
		}
		else if (!is_auto_pet && who.ActiveObject?.QualifiedItemId != "(O)178")
		{
			Game1.activeClickableMenu = new AnimalQueryMenu(this);
		}
	}

	public void farmerPushing()
	{
		pushAccumulator++;
		if (pushAccumulator > 60)
		{
			doFarmerPushEvent.Fire(Game1.player.FacingDirection);
			Microsoft.Xna.Framework.Rectangle bounds = GetBoundingBox();
			bounds = Utility.ExpandRectangle(bounds, Utility.GetOppositeFacingDirection(Game1.player.FacingDirection), 6);
			Game1.player.TemporaryPassableTiles.Add(bounds);
			pushAccumulator = 0;
		}
	}

	public virtual void doDive()
	{
		yJumpVelocity = 8f;
		yJumpOffset = 1;
	}

	public void doFarmerPush(int direction)
	{
		if (Game1.IsMasterGame)
		{
			switch (direction)
			{
			case 0:
				Halt();
				break;
			case 1:
				Halt();
				break;
			case 2:
				Halt();
				break;
			case 3:
				Halt();
				break;
			}
		}
	}

	public void Poke()
	{
		doBuildingPokeEvent.Fire();
	}

	public void doBuildingPoke()
	{
		if (Game1.IsMasterGame)
		{
			FacingDirection = Game1.random.Next(4);
			setMovingInFacingDirection();
		}
	}

	public void setRandomPosition(GameLocation location)
	{
		StopAllActions();
		if (!location.TryGetMapPropertyAs("ProduceArea", out Microsoft.Xna.Framework.Rectangle produceArea, required: true))
		{
			return;
		}
		base.Position = new Vector2(Game1.random.Next(produceArea.X, produceArea.Right) * 64, Game1.random.Next(produceArea.Y, produceArea.Bottom) * 64);
		int tries = 0;
		while (base.Position.Equals(Vector2.Zero) || location.Objects.ContainsKey(base.Position) || location.isCollidingPosition(GetBoundingBox(), Game1.viewport, isFarmer: false, 0, glider: false, this))
		{
			base.Position = new Vector2(Game1.random.Next(produceArea.X, produceArea.Right), Game1.random.Next(produceArea.Y, produceArea.Bottom)) * 64f;
			tries++;
			if (tries > 64)
			{
				break;
			}
		}
		SleepIfNecessary();
	}

	public virtual void StopAllActions()
	{
		foundGrass = null;
		controller = null;
		isSwimming.Value = false;
		hopOffset = Vector2.Zero;
		_followTarget = null;
		_followTargetPosition = null;
		Halt();
		Sprite.StopAnimation();
		Sprite.UpdateSourceRect();
	}

	public virtual void HandleStats(List<StatIncrement> stats, Item item, uint amount = 1u)
	{
		if (stats == null)
		{
			return;
		}
		foreach (StatIncrement stat in stats)
		{
			if (stat.RequiredItemId == null || ItemRegistry.HasItemId(item, stat.RequiredItemId))
			{
				List<string> requiredTags = stat.RequiredTags;
				if (requiredTags == null || requiredTags.Count <= 0 || ItemContextTagManager.DoAllTagsMatch(stat.RequiredTags, item.GetContextTags()))
				{
					Game1.stats.Increment(stat.StatName, amount);
				}
			}
		}
	}

	public string GetProduceID(Random r, bool deluxe = false)
	{
		FarmAnimalData data = GetAnimalData();
		if (data == null)
		{
			return null;
		}
		List<FarmAnimalProduce> produceList = new List<FarmAnimalProduce>();
		if (deluxe)
		{
			if (data.DeluxeProduceItemIds != null)
			{
				produceList.AddRange(data.DeluxeProduceItemIds);
			}
		}
		else if (data.ProduceItemIds != null)
		{
			produceList.AddRange(data.ProduceItemIds);
		}
		if (produceList.Count == 0)
		{
			return null;
		}
		for (int i = 0; i < produceList.Count; i++)
		{
			if (produceList[i].MinimumFriendship > 0 && friendshipTowardFarmer.Value < produceList[i].MinimumFriendship)
			{
				produceList.RemoveAt(i);
				i--;
			}
			else if (produceList[i].Condition != null && !GameStateQuery.CheckConditions(produceList[i].Condition, base.currentLocation, null, null, null, r))
			{
				produceList.RemoveAt(i);
				i--;
			}
		}
		if (produceList.Count == 0)
		{
			return null;
		}
		return r.ChooseFrom(produceList).ItemId;
	}

	/// <summary>Update the animal state when setting up the new day, before the game saves overnight.</summary>
	/// <param name="environment">The location containing the animal.</param>
	/// <remarks>See also <see cref="M:StardewValley.FarmAnimal.OnDayStarted" />, which happens after saving when the day has started.</remarks>
	public void dayUpdate(GameLocation environment)
	{
		if (daysOwned.Value < 0)
		{
			daysOwned.Value = age.Value;
		}
		FarmAnimalData data = GetAnimalData();
		int happinessDrain = GetAnimalData()?.HappinessDrain ?? 0;
		int produceSpeedBonus = ((data != null && data.FriendshipForFasterProduce >= 0 && friendshipTowardFarmer.Value >= data.FriendshipForFasterProduce) ? 1 : 0);
		StopAllActions();
		health.Value = 3;
		bool wasLeftOutLastNight = false;
		GameLocation insideHome = home?.GetIndoors();
		if (insideHome != null && !IsHome)
		{
			if ((bool)home.animalDoorOpen)
			{
				environment.animals.Remove(myID.Value);
				insideHome.animals.Add(myID.Value, this);
				if (Game1.timeOfDay > 1800 && controller == null)
				{
					happiness.Value /= 2;
				}
				setRandomPosition(insideHome);
				return;
			}
			moodMessage.Value = 6;
			wasLeftOutLastNight = true;
			happiness.Value /= 2;
		}
		else if (insideHome != null && IsHome && !home.animalDoorOpen)
		{
			happiness.Value = (byte)Math.Min(255, (int)happiness + happinessDrain * 2);
		}
		daysSinceLastLay.Value++;
		if (!wasPet.Value && !wasAutoPet.Value)
		{
			friendshipTowardFarmer.Value = Math.Max(0, (int)friendshipTowardFarmer - (10 - (int)friendshipTowardFarmer / 200));
			happiness.Value = (byte)Math.Max(0, (int)happiness - 50);
		}
		wasPet.Value = false;
		wasAutoPet.Value = false;
		daysOwned.Value++;
		if ((int)fullness < 200 && environment is AnimalHouse)
		{
			KeyValuePair<Vector2, Object>[] array = environment.objects.Pairs.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				KeyValuePair<Vector2, Object> pair = array[i];
				if (pair.Value.QualifiedItemId == "(O)178")
				{
					environment.objects.Remove(pair.Key);
					fullness.Value = 255;
					break;
				}
			}
		}
		Random r = Utility.CreateRandom((double)myID.Value / 2.0, Game1.stats.DaysPlayed);
		if ((int)fullness > 200 || r.NextDouble() < (double)((int)fullness - 30) / 170.0)
		{
			if (age.Value == ((data != null) ? new int?(data.DaysToMature - 1) : null))
			{
				growFully(r);
			}
			else
			{
				age.Value++;
			}
			happiness.Value = (byte)Math.Min(255, (int)happiness + happinessDrain * 2);
		}
		if (fullness.Value < 200)
		{
			happiness.Value = (byte)Math.Max(0, (int)happiness - 100);
			friendshipTowardFarmer.Value = Math.Max(0, (int)friendshipTowardFarmer - 20);
		}
		if (data != null && data.ProfessionForFasterProduce >= 0 && Game1.getFarmer(ownerID.Value).professions.Contains(data.ProfessionForFasterProduce))
		{
			produceSpeedBonus++;
		}
		bool produceToday = (int)daysSinceLastLay >= ((data != null) ? new int?(data.DaysToProduce - produceSpeedBonus) : null) && r.NextDouble() < (double)(int)fullness / 200.0 && r.NextDouble() < (double)(int)happiness / 70.0;
		string whichProduce;
		if (!produceToday || isBaby())
		{
			whichProduce = null;
		}
		else
		{
			whichProduce = GetProduceID(r);
			if (r.NextDouble() < (double)(int)happiness / 150.0)
			{
				float happinessModifier = (((int)happiness > 200) ? ((float)(int)happiness * 1.5f) : ((float)(((int)happiness <= 100) ? ((int)happiness - 100) : 0)));
				string deluxeProduce = GetProduceID(r, deluxe: true);
				if (data != null && data.DeluxeProduceCareDivisor >= 0f && deluxeProduce != null && friendshipTowardFarmer.Value >= data.DeluxeProduceMinimumFriendship && r.NextDouble() < (double)(((float)(int)friendshipTowardFarmer + happinessModifier) / data.DeluxeProduceCareDivisor) + Game1.player.team.AverageDailyLuck() * (double)data.DeluxeProduceLuckMultiplier)
				{
					whichProduce = deluxeProduce;
				}
				daysSinceLastLay.Value = 0;
				double chanceForQuality = (float)(int)friendshipTowardFarmer / 1000f - (1f - (float)(int)happiness / 225f);
				if (data != null && data.ProfessionForQualityBoost >= 0 && Game1.getFarmer(ownerID.Value).professions.Contains(data.ProfessionForQualityBoost))
				{
					chanceForQuality += 0.33;
				}
				if (chanceForQuality >= 0.95 && r.NextDouble() < chanceForQuality / 2.0)
				{
					produceQuality.Value = 4;
				}
				else if (r.NextDouble() < chanceForQuality / 2.0)
				{
					produceQuality.Value = 2;
				}
				else if (r.NextDouble() < chanceForQuality)
				{
					produceQuality.Value = 1;
				}
				else
				{
					produceQuality.Value = 0;
				}
			}
		}
		if ((data == null || data.HarvestType != FarmAnimalHarvestType.DropOvernight) && produceToday)
		{
			currentProduce.Value = whichProduce;
			whichProduce = null;
		}
		if (whichProduce != null && home != null)
		{
			bool spawn_object = true;
			Object producedObject = ItemRegistry.Create<Object>("(O)" + whichProduce);
			producedObject.CanBeSetDown = false;
			producedObject.Quality = produceQuality;
			if ((bool)hasEatenAnimalCracker)
			{
				producedObject.Stack = 2;
			}
			if (data?.StatToIncrementOnProduce != null)
			{
				HandleStats(data.StatToIncrementOnProduce, producedObject, (uint)producedObject.Stack);
			}
			foreach (Object location_object in insideHome.objects.Values)
			{
				if (location_object.QualifiedItemId == "(BC)165" && location_object.heldObject.Value is Chest chest && chest.addItem(producedObject) == null)
				{
					location_object.showNextIndex.Value = true;
					spawn_object = false;
					break;
				}
			}
			if (spawn_object)
			{
				producedObject.Stack = 1;
				Utility.spawnObjectAround(base.Tile, producedObject, environment);
				if ((bool)hasEatenAnimalCracker)
				{
					Object o = (Object)producedObject.getOne();
					Utility.spawnObjectAround(base.Tile, o, environment);
				}
			}
		}
		if (!wasLeftOutLastNight)
		{
			if ((int)fullness < 30)
			{
				moodMessage.Value = 4;
			}
			else if ((int)happiness < 30)
			{
				moodMessage.Value = 3;
			}
			else if ((int)happiness < 200)
			{
				moodMessage.Value = 2;
			}
			else
			{
				moodMessage.Value = 1;
			}
		}
		fullness.Value = 0;
		if (Utility.isFestivalDay())
		{
			fullness.Value = 250;
		}
		reload(home);
	}

	/// <summary>Handle the new day starting after the player saves, loads, or connects.</summary>
	/// <remarks>See also <see cref="M:StardewValley.FarmAnimal.dayUpdate(StardewValley.GameLocation)" />, which happens while setting up the day before saving.</remarks>
	public void OnDayStarted()
	{
		FarmAnimalData animalData = GetAnimalData();
		if (animalData != null && animalData.GrassEatAmount < 1)
		{
			fullness.Value = 255;
		}
	}

	public int getSellPrice()
	{
		int num = GetAnimalData()?.SellPrice ?? 0;
		double adjustedFriendship = (double)(int)friendshipTowardFarmer / 1000.0 + 0.3;
		return (int)((double)num * adjustedFriendship);
	}

	public bool isMale()
	{
		return GetAnimalData()?.Gender switch
		{
			FarmAnimalGender.Female => false, 
			FarmAnimalGender.Male => true, 
			_ => myID.Value % 2 == 0, 
		};
	}

	public string getMoodMessage()
	{
		string gender = (isMale() ? "Male" : "Female");
		switch (moodMessage.Value)
		{
		case 0:
			if (parentId.Value != -1)
			{
				return Game1.content.LoadString("Strings\\FarmAnimals:MoodMessage_NewHome_Baby_" + gender, displayName);
			}
			return Game1.content.LoadString("Strings\\FarmAnimals:MoodMessage_NewHome_Adult_" + gender + "_" + (Game1.dayOfMonth % 2 + 1), displayName);
		case 6:
			return Game1.content.LoadString("Strings\\FarmAnimals:MoodMessage_LeftOutsideAtNight_" + gender, displayName);
		case 5:
			return Game1.content.LoadString("Strings\\FarmAnimals:MoodMessage_DisturbedByDog_" + gender, displayName);
		case 4:
			return Game1.content.LoadString("Strings\\FarmAnimals:MoodMessage_" + (((Game1.dayOfMonth + myID.Value) % 2 == 0L) ? "Hungry1" : "Hungry2"), displayName);
		default:
			if ((int)happiness < 30)
			{
				moodMessage.Value = 3;
			}
			else if ((int)happiness < 200)
			{
				moodMessage.Value = 2;
			}
			else
			{
				moodMessage.Value = 1;
			}
			return moodMessage.Value switch
			{
				3 => Game1.content.LoadString("Strings\\FarmAnimals:MoodMessage_Sad", displayName), 
				2 => Game1.content.LoadString("Strings\\FarmAnimals:MoodMessage_Fine", displayName), 
				1 => Game1.content.LoadString("Strings\\FarmAnimals:MoodMessage_Happy", displayName), 
				_ => "", 
			};
		}
	}

	/// <summary>Get whether this farm animal is fully grown.</summary>
	/// <remarks>See also <see cref="M:StardewValley.FarmAnimal.isBaby" />.</remarks>
	public bool isAdult()
	{
		int? adultAge = GetAnimalData()?.DaysToMature;
		if (adultAge.HasValue)
		{
			return (int)age >= adultAge;
		}
		return true;
	}

	/// <summary>Get whether this farm animal is a baby.</summary>
	/// <remarks>See also <see cref="M:StardewValley.FarmAnimal.isAdult" />.</remarks>
	public bool isBaby()
	{
		return (int)age < GetAnimalData()?.DaysToMature;
	}

	/// <summary>Get whether this farm animal's produce can be collected using a given tool.</summary>
	/// <param name="tool">The tool to check.</param>
	public bool CanGetProduceWithTool(Tool tool)
	{
		if (tool != null && tool.BaseName != null)
		{
			return GetAnimalData().HarvestTool == tool.BaseName;
		}
		return false;
	}

	/// <summary>Get the way in which the animal's produce is output.</summary>
	public FarmAnimalHarvestType? GetHarvestType()
	{
		return GetAnimalData()?.HarvestType;
	}

	/// <summary>Get whether this farm animal can live in a building.</summary>
	/// <param name="building">The building to check.</param>
	/// <remarks>This doesn't check whether there's room for it in the building; see <see cref="M:StardewValley.AnimalHouse.isFull" /> on <see cref="M:StardewValley.Buildings.Building.GetIndoors" /> for that.</remarks>
	public bool CanLiveIn(Building building)
	{
		BuildingData buildingData = building?.GetData();
		if (buildingData?.ValidOccupantTypes != null && buildingData.ValidOccupantTypes.Contains(buildingTypeILiveIn.Value) && !building.isUnderConstruction())
		{
			return building.GetIndoors() is AnimalHouse;
		}
		return false;
	}

	public void warpHome()
	{
		GameLocation insideHome = home?.GetIndoors();
		if (insideHome != null && insideHome != base.currentLocation)
		{
			if (insideHome.animals.TryAdd(myID.Value, this))
			{
				setRandomPosition(insideHome);
				home.currentOccupants.Value++;
			}
			base.currentLocation?.animals.Remove(myID.Value);
			controller = null;
			isSwimming.Value = false;
			hopOffset = Vector2.Zero;
			_followTarget = null;
			_followTargetPosition = null;
		}
	}

	/// <summary>If the animal is a baby, instantly age it to adult.</summary>
	/// <param name="random">The RNG with which to select its produce, if applicable.</param>
	public void growFully(Random random = null)
	{
		FarmAnimalData data = GetAnimalData();
		if ((int)age <= data?.DaysToMature)
		{
			age.Value = data.DaysToMature;
			if (data.ProduceOnMature)
			{
				currentProduce.Value = GetProduceID(random ?? Game1.random);
			}
			daysSinceLastLay.Value = 99;
			ReloadTextureIfNeeded();
		}
	}

	public override void draw(SpriteBatch b)
	{
		Vector2 offset = new Vector2(0f, yJumpOffset);
		Microsoft.Xna.Framework.Rectangle boundingBox = GetBoundingBox();
		FarmAnimalData data = GetAnimalData();
		bool isActuallySwimming = IsActuallySwimming();
		bool baby = isBaby();
		FarmAnimalShadowData shadow = data?.GetShadow(baby, isActuallySwimming);
		if (shadow == null || shadow.Visible)
		{
			int shadowOffsetX = (shadow?.Offset?.X).GetValueOrDefault();
			int shadowOffsetY = (shadow?.Offset?.Y).GetValueOrDefault();
			if (isActuallySwimming)
			{
				float shadowScale = shadow?.Scale ?? (baby ? 2.5f : 3.5f);
				Vector2 shadowPos = new Vector2(base.Position.X + (float)shadowOffsetX, base.Position.Y - 24f + (float)shadowOffsetY);
				Sprite.drawShadow(b, Game1.GlobalToLocal(Game1.viewport, shadowPos), shadowScale, 0.5f);
				int bobAmount = (int)((Math.Sin(Game1.currentGameTime.TotalGameTime.TotalSeconds * 4.0 + (double)bobOffset) + 0.5) * 3.0);
				offset.Y += bobAmount;
			}
			else
			{
				float shadowScale = shadow?.Scale ?? (baby ? 3f : 4f);
				Vector2 shadowPos = new Vector2(base.Position.X + (float)shadowOffsetX, base.Position.Y - 24f + (float)shadowOffsetY);
				Sprite.drawShadow(b, Game1.GlobalToLocal(Game1.viewport, shadowPos), shadowScale);
			}
		}
		offset.Y += yJumpOffset;
		float layer_depth = ((float)(boundingBox.Center.Y + 4) + base.Position.X / 20000f) / 10000f;
		Sprite.draw(b, Utility.snapDrawPosition(Game1.GlobalToLocal(Game1.viewport, base.Position - new Vector2(0f, 24f) + offset)), layer_depth, 0, 0, (hitGlowTimer > 0) ? Color.Red : Color.White, FacingDirection == 3, 4f);
		if (isEmoting)
		{
			int emoteOffsetX = Sprite.SpriteWidth / 2 * 4 - 32 + (data?.EmoteOffset.X ?? 0);
			int emoteOffsetY = -64 + (data?.EmoteOffset.Y ?? 0);
			Vector2 emotePosition = Game1.GlobalToLocal(Game1.viewport, new Vector2(base.Position.X + offset.X + (float)emoteOffsetX, base.Position.Y + offset.Y + (float)emoteOffsetY));
			b.Draw(Game1.emoteSpriteSheet, emotePosition, new Microsoft.Xna.Framework.Rectangle(base.CurrentEmoteIndex * 16 % Game1.emoteSpriteSheet.Width, base.CurrentEmoteIndex * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)boundingBox.Bottom / 10000f);
		}
	}

	public virtual void updateWhenNotCurrentLocation(Building currentBuilding, GameTime time, GameLocation environment)
	{
		doFarmerPushEvent.Poll();
		doBuildingPokeEvent.Poll();
		doDiveEvent.Poll();
		if (!Game1.shouldTimePass())
		{
			return;
		}
		update(time, environment, myID.Value, move: false);
		if (!Game1.IsMasterGame)
		{
			return;
		}
		if (hopOffset != Vector2.Zero)
		{
			HandleHop();
			return;
		}
		if (currentBuilding != null && Game1.random.NextBool(0.002) && (bool)currentBuilding.animalDoorOpen && Game1.timeOfDay < 1630 && !environment.IsRainingHere() && !environment.IsWinterHere() && !environment.farmers.Any())
		{
			GameLocation buildingLocation = currentBuilding.GetParentLocation();
			Microsoft.Xna.Framework.Rectangle doorArea = currentBuilding.getRectForAnimalDoor();
			doorArea.Inflate(-2, -2);
			if (buildingLocation.isCollidingPosition(doorArea, Game1.viewport, isFarmer: false, 0, glider: false, this, pathfinding: false) || buildingLocation.isCollidingPosition(new Microsoft.Xna.Framework.Rectangle(doorArea.X, doorArea.Y + 64, doorArea.Width, doorArea.Height), Game1.viewport, isFarmer: false, 0, glider: false, this, pathfinding: false))
			{
				return;
			}
			if (buildingLocation.animals.ContainsKey(myID.Value))
			{
				buildingLocation.animals.Remove(myID.Value);
			}
			currentBuilding.GetIndoors().animals.Remove(myID.Value);
			buildingLocation.animals.Add(myID.Value, this);
			faceDirection(2);
			SetMovingDown(b: true);
			base.Position = new Vector2(doorArea.X, doorArea.Y - (Sprite.getHeight() * 4 - GetBoundingBox().Height) + 32);
			if (NumPathfindingThisTick < MaxPathfindingPerTick)
			{
				NumPathfindingThisTick++;
				controller = new PathFindController(this, buildingLocation, grassEndPointFunction, Game1.random.Next(4), behaviorAfterFindingGrassPatch, 200, Point.Zero);
			}
			if (controller?.pathToEndPoint == null || controller.pathToEndPoint.Count < 3)
			{
				SetMovingDown(b: true);
				controller = null;
			}
			else
			{
				faceDirection(2);
				base.Position = new Vector2(controller.pathToEndPoint.Peek().X * 64, controller.pathToEndPoint.Peek().Y * 64 - (Sprite.getHeight() * 4 - GetBoundingBox().Height) + 16);
				if (Sprite.SpriteWidth * 4 > 64)
				{
					position.X -= 32f;
				}
			}
			noWarpTimer = 3000;
			currentBuilding.currentOccupants.Value--;
			if (Utility.isOnScreen(base.TilePoint, 192, buildingLocation))
			{
				buildingLocation.localSound("sandyStep");
			}
			environment.isTileOccupiedByFarmer(base.Tile)?.TemporaryPassableTiles.Add(GetBoundingBox());
		}
		UpdateRandomMovements();
		behaviors(time, environment);
	}

	public static void behaviorAfterFindingGrassPatch(Character c, GameLocation environment)
	{
		if (environment.terrainFeatures.TryGetValue(c.Tile, out var feature) && feature is Grass grass)
		{
			reservedGrass.Remove(grass);
		}
		if ((int)((FarmAnimal)c).fullness < 255)
		{
			((FarmAnimal)c).eatGrass(environment);
		}
	}

	public static bool grassEndPointFunction(PathNode currentPoint, Point endPoint, GameLocation location, Character c)
	{
		Vector2 tileLocation = new Vector2(currentPoint.x, currentPoint.y);
		if (location.terrainFeatures.TryGetValue(tileLocation, out var t) && t is Grass grass)
		{
			if (reservedGrass.Contains(t))
			{
				return false;
			}
			reservedGrass.Add(grass);
			if (c is FarmAnimal animal)
			{
				animal.foundGrass = grass;
			}
			return true;
		}
		return false;
	}

	public virtual void updatePerTenMinutes(int timeOfDay, GameLocation environment)
	{
		if (timeOfDay >= 1800)
		{
			int happinessDrain = GetAnimalData()?.HappinessDrain ?? 0;
			int change = 0;
			if (environment.IsOutdoors)
			{
				change = ((timeOfDay > 1900 || environment.IsRainingHere() || environment.IsWinterHere()) ? (-happinessDrain) : happinessDrain);
			}
			else if ((int)happiness > 150 && environment.IsWinterHere())
			{
				change = ((environment.numberOfObjectsWithName("Heater") > 0) ? happinessDrain : (-happinessDrain));
			}
			if (change != 0)
			{
				happiness.Value = (byte)MathHelper.Clamp(happiness.Value + change, 0, 255);
			}
		}
		environment.isTileOccupiedByFarmer(base.Tile)?.TemporaryPassableTiles.Add(GetBoundingBox());
	}

	public void eatGrass(GameLocation environment)
	{
		if (environment.terrainFeatures.TryGetValue(base.Tile, out var feature) && feature is Grass grass)
		{
			reservedGrass.Remove(grass);
			if (foundGrass != null)
			{
				reservedGrass.Remove(foundGrass);
			}
			foundGrass = null;
			Eat(environment);
		}
	}

	public virtual void Eat(GameLocation location)
	{
		Vector2 tile = base.Tile;
		isEating.Value = true;
		int grassType = 1;
		if (location.terrainFeatures.TryGetValue(tile, out var terrainFeature) && terrainFeature is Grass grass)
		{
			grassType = grass.grassType.Value;
			int grassEatAmount = GetAnimalData()?.GrassEatAmount ?? 2;
			if (grass.reduceBy(grassEatAmount, location.Equals(Game1.currentLocation)))
			{
				location.terrainFeatures.Remove(tile);
			}
		}
		Sprite.loop = false;
		fullness.Value = 255;
		if ((int)moodMessage != 5 && (int)moodMessage != 6 && !location.IsRainingHere())
		{
			happiness.Value = 255;
			friendshipTowardFarmer.Value = Math.Min(1000, friendshipTowardFarmer.Value + ((grassType == 7) ? 16 : 8));
		}
	}

	public virtual bool behaviors(GameTime time, GameLocation location)
	{
		if (!Game1.IsMasterGame)
		{
			return false;
		}
		Building home = this.home;
		if (home == null)
		{
			return false;
		}
		if (isBaby() && CanFollowAdult())
		{
			_nextFollowTargetScan -= (float)time.ElapsedGameTime.TotalSeconds;
			if (_nextFollowTargetScan < 0f)
			{
				_nextFollowTargetScan = Utility.RandomFloat(1f, 3f);
				if (controller != null || !location.IsOutdoors)
				{
					_followTarget = null;
					_followTargetPosition = null;
				}
				else
				{
					if (_followTarget != null)
					{
						if (!GetFollowRange(_followTarget).Contains(_followTargetPosition.Value))
						{
							GetNewFollowPosition();
						}
						return false;
					}
					if (location.IsOutdoors)
					{
						foreach (FarmAnimal animal in location.animals.Values)
						{
							if (!animal.isBaby() && animal.type.Value == type.Value && GetFollowRange(animal, 4).Contains(base.StandingPixel))
							{
								_followTarget = animal;
								GetNewFollowPosition();
								return false;
							}
						}
					}
				}
			}
		}
		if ((bool)isEating)
		{
			if (home != null && home.getRectForAnimalDoor().Intersects(GetBoundingBox()))
			{
				behaviorAfterFindingGrassPatch(this, location);
				isEating.Value = false;
				Halt();
				return false;
			}
			FarmAnimalData animalData = GetAnimalData();
			int eatFrame = 16;
			if (!Sprite.textureUsesFlippedRightForLeft)
			{
				eatFrame += 4;
			}
			if (animalData?.UseDoubleUniqueAnimationFrames ?? false)
			{
				eatFrame += 4;
			}
			if (Sprite.Animate(time, eatFrame, 4, 100f))
			{
				isEating.Value = false;
				Sprite.loop = true;
				Sprite.currentFrame = 0;
				faceDirection(2);
			}
			return true;
		}
		if (controller != null)
		{
			return true;
		}
		if (!isSwimming.Value && location.IsOutdoors && (int)fullness < 195 && Game1.random.NextDouble() < 0.002 && NumPathfindingThisTick < MaxPathfindingPerTick)
		{
			NumPathfindingThisTick++;
			controller = new PathFindController(this, location, grassEndPointFunction, -1, behaviorAfterFindingGrassPatch, 200, Point.Zero);
			_followTarget = null;
			_followTargetPosition = null;
		}
		if (Game1.timeOfDay >= 1700 && location.IsOutdoors && controller == null && Game1.random.NextDouble() < 0.002 && (bool)home.animalDoorOpen)
		{
			if (!location.farmers.Any())
			{
				GameLocation insideHome = home.GetIndoors();
				location.animals.Remove(myID.Value);
				insideHome.animals.Add(myID.Value, this);
				setRandomPosition(insideHome);
				faceDirection(Game1.random.Next(4));
				controller = null;
				return true;
			}
			if (NumPathfindingThisTick < MaxPathfindingPerTick)
			{
				NumPathfindingThisTick++;
				controller = new PathFindController(this, location, PathFindController.isAtEndPoint, 0, null, 200, new Point((int)home.tileX + home.animalDoor.X, (int)home.tileY + home.animalDoor.Y));
				_followTarget = null;
				_followTargetPosition = null;
			}
		}
		if (location.IsOutdoors && !location.IsRainingHere() && !location.IsWinterHere() && currentProduce.Value != null && isAdult() && GetHarvestType() == FarmAnimalHarvestType.DigUp && Game1.random.NextDouble() < 0.0002)
		{
			Object produce = ItemRegistry.Create<Object>(currentProduce.Value);
			Microsoft.Xna.Framework.Rectangle rect = GetBoundingBox();
			for (int i = 0; i < 4; i++)
			{
				Vector2 v = Utility.getCornersOfThisRectangle(ref rect, i);
				Vector2 vec = new Vector2((int)(v.X / 64f), (int)(v.Y / 64f));
				if (location.terrainFeatures.ContainsKey(vec) || location.objects.ContainsKey(vec))
				{
					return false;
				}
			}
			if (Game1.player.currentLocation.Equals(location))
			{
				DelayedAction.playSoundAfterDelay("dirtyHit", 450);
				DelayedAction.playSoundAfterDelay("dirtyHit", 900);
				DelayedAction.playSoundAfterDelay("dirtyHit", 1350);
			}
			if (location.Equals(Game1.currentLocation))
			{
				switch (FacingDirection)
				{
				case 2:
					Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(1, 250),
						new FarmerSprite.AnimationFrame(3, 250),
						new FarmerSprite.AnimationFrame(1, 250),
						new FarmerSprite.AnimationFrame(3, 250),
						new FarmerSprite.AnimationFrame(1, 250),
						new FarmerSprite.AnimationFrame(3, 250, secondaryArm: false, flip: false, delegate
						{
							DigUpProduce(location, produce);
						})
					});
					break;
				case 1:
					Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(5, 250),
						new FarmerSprite.AnimationFrame(7, 250),
						new FarmerSprite.AnimationFrame(5, 250),
						new FarmerSprite.AnimationFrame(7, 250),
						new FarmerSprite.AnimationFrame(5, 250),
						new FarmerSprite.AnimationFrame(7, 250, secondaryArm: false, flip: false, delegate
						{
							DigUpProduce(location, produce);
						})
					});
					break;
				case 0:
					Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(9, 250),
						new FarmerSprite.AnimationFrame(11, 250),
						new FarmerSprite.AnimationFrame(9, 250),
						new FarmerSprite.AnimationFrame(11, 250),
						new FarmerSprite.AnimationFrame(9, 250),
						new FarmerSprite.AnimationFrame(11, 250, secondaryArm: false, flip: false, delegate
						{
							DigUpProduce(location, produce);
						})
					});
					break;
				case 3:
					Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(5, 250, secondaryArm: false, flip: true),
						new FarmerSprite.AnimationFrame(7, 250, secondaryArm: false, flip: true),
						new FarmerSprite.AnimationFrame(5, 250, secondaryArm: false, flip: true),
						new FarmerSprite.AnimationFrame(7, 250, secondaryArm: false, flip: true),
						new FarmerSprite.AnimationFrame(5, 250, secondaryArm: false, flip: true),
						new FarmerSprite.AnimationFrame(7, 250, secondaryArm: false, flip: true, delegate
						{
							DigUpProduce(location, produce);
						})
					});
					break;
				}
				Sprite.loop = false;
			}
			else
			{
				DigUpProduce(location, produce);
			}
		}
		return false;
	}

	public virtual void DigUpProduce(GameLocation location, Object produce)
	{
		Random r = Utility.CreateRandom((double)myID.Value / 2.0, Game1.stats.DaysPlayed, Game1.timeOfDay);
		bool success = false;
		if (produce.QualifiedItemId == "(O)430" && r.NextDouble() < 0.002)
		{
			RockCrab crab = new RockCrab(base.Tile, "Truffle Crab");
			Vector2 v = Utility.recursiveFindOpenTileForCharacter(crab, location, base.Tile, 50, allowOffMap: false);
			if (v != Vector2.Zero)
			{
				crab.setTileLocation(v);
				location.addCharacter(crab);
				success = true;
			}
		}
		if (!success && Utility.spawnObjectAround(Utility.getTranslatedVector2(base.Tile, FacingDirection, 1f), produce, base.currentLocation) && produce.QualifiedItemId == "(O)430")
		{
			Game1.stats.TrufflesFound++;
		}
		if (!r.NextBool((double)friendshipTowardFarmer.Value / 1500.0))
		{
			currentProduce.Value = null;
		}
	}

	public static Microsoft.Xna.Framework.Rectangle GetFollowRange(FarmAnimal animal, int distance = 2)
	{
		Point standingPixel = animal.StandingPixel;
		return new Microsoft.Xna.Framework.Rectangle(standingPixel.X - distance * 64, standingPixel.Y - distance * 64, distance * 64 * 2, 64 * distance * 2);
	}

	public virtual void GetNewFollowPosition()
	{
		if (_followTarget == null)
		{
			_followTargetPosition = null;
		}
		else if (_followTarget.isMoving() && _followTarget.IsActuallySwimming())
		{
			_followTargetPosition = Utility.Vector2ToPoint(Utility.getRandomPositionInThisRectangle(GetFollowRange(_followTarget, 1), Game1.random));
		}
		else
		{
			_followTargetPosition = Utility.Vector2ToPoint(Utility.getRandomPositionInThisRectangle(GetFollowRange(_followTarget), Game1.random));
		}
	}

	public void hitWithWeapon(MeleeWeapon t)
	{
	}

	public void makeSound()
	{
		if (base.currentLocation == Game1.currentLocation && !Game1.options.muteAnimalSounds)
		{
			string soundToPlay = GetSoundId();
			if (soundToPlay != null)
			{
				Game1.playSound(soundToPlay, 1200 + Game1.random.Next(-200, 201));
			}
		}
	}

	/// <summary>Get the sound ID produced by the animal (e.g. when pet).</summary>
	public string GetSoundId()
	{
		FarmAnimalData data = GetAnimalData();
		if (!isBaby() || data == null || data.BabySound == null)
		{
			return data?.Sound;
		}
		return data.BabySound;
	}

	public virtual bool CanHavePregnancy()
	{
		return GetAnimalData()?.CanGetPregnant ?? false;
	}

	public virtual bool SleepIfNecessary()
	{
		if (Game1.timeOfDay >= 2000)
		{
			isSwimming.Value = false;
			hopOffset = Vector2.Zero;
			_followTarget = null;
			_followTargetPosition = null;
			if (isMoving())
			{
				Halt();
			}
			FarmAnimalData data = GetAnimalData();
			Sprite.currentFrame = data?.SleepFrame ?? 12;
			FacingDirection = 2;
			Sprite.UpdateSourceRect();
			return true;
		}
		return false;
	}

	public override bool isMoving()
	{
		if (_swimmingVelocity != Vector2.Zero)
		{
			return true;
		}
		if (!IsActuallySwimming() && uniqueFrameAccumulator != -1)
		{
			return false;
		}
		return base.isMoving();
	}

	public virtual bool updateWhenCurrentLocation(GameTime time, GameLocation location)
	{
		if (!Game1.shouldTimePass())
		{
			return false;
		}
		if (health.Value <= 0)
		{
			return true;
		}
		doBuildingPokeEvent.Poll();
		doDiveEvent.Poll();
		if (IsActuallySwimming())
		{
			int time_multiplier = 1;
			if (isMoving())
			{
				time_multiplier = 4;
			}
			nextRipple -= (int)time.ElapsedGameTime.TotalMilliseconds * time_multiplier;
			if (nextRipple <= 0)
			{
				nextRipple = 2000;
				float scale = 1f;
				if (isBaby())
				{
					scale = 0.65f;
				}
				Point standingPixel = base.StandingPixel;
				float x_offset = base.Position.X - (float)standingPixel.X;
				TemporaryAnimatedSprite ripple = new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), isMoving() ? 75f : 150f, 8, 0, new Vector2((float)standingPixel.X + x_offset * scale, (float)standingPixel.Y - 32f * scale), flicker: false, Game1.random.NextBool(), 0.01f, 0.01f, Color.White * 0.75f, scale, 0f, 0f, 0f);
				Vector2 offset = Utility.PointToVector2(Utility.getTranslatedPoint(default(Point), FacingDirection, -1));
				ripple.motion = offset * 0.25f;
				location.TemporarySprites.Add(ripple);
			}
		}
		if (hitGlowTimer > 0)
		{
			hitGlowTimer -= time.ElapsedGameTime.Milliseconds;
		}
		if (Sprite.CurrentAnimation != null)
		{
			if (Sprite.animateOnce(time))
			{
				Sprite.CurrentAnimation = null;
			}
			return false;
		}
		update(time, location, myID.Value, move: false);
		if (hopOffset != Vector2.Zero)
		{
			Sprite.UpdateSourceRect();
			HandleHop();
			return false;
		}
		if (Game1.IsMasterGame && behaviors(time, location))
		{
			return false;
		}
		if (Sprite.CurrentAnimation != null)
		{
			return false;
		}
		PathFindController pathFindController = controller;
		if (pathFindController != null && pathFindController.timerSinceLastCheckPoint > 10000)
		{
			controller = null;
			Halt();
		}
		if (Game1.IsMasterGame)
		{
			if (!IsHome && noWarpTimer <= 0)
			{
				GameLocation insideHome = home?.GetIndoors();
				if (insideHome != null)
				{
					Microsoft.Xna.Framework.Rectangle bounds = GetBoundingBox();
					if (home.getRectForAnimalDoor().Contains(bounds.Center.X, bounds.Top))
					{
						if (Utility.isOnScreen(base.TilePoint, 192, location))
						{
							location.localSound("dwoop");
						}
						location.animals.Remove(myID.Value);
						insideHome.animals[myID.Value] = this;
						setRandomPosition(insideHome);
						faceDirection(Game1.random.Next(4));
						controller = null;
						return true;
					}
				}
			}
			noWarpTimer = Math.Max(0, noWarpTimer - time.ElapsedGameTime.Milliseconds);
		}
		if (pauseTimer > 0)
		{
			pauseTimer -= time.ElapsedGameTime.Milliseconds;
		}
		if (SleepIfNecessary())
		{
			if (!isEmoting && Game1.random.NextDouble() < 0.002)
			{
				doEmote(24);
			}
		}
		else if (pauseTimer <= 0 && Game1.random.NextDouble() < 0.001 && isAdult() && Game1.gameMode == 3 && Utility.isOnScreen(base.Position, 192))
		{
			makeSound();
		}
		if (Game1.IsMasterGame)
		{
			UpdateRandomMovements();
			if (uniqueFrameAccumulator != -1 && _followTarget != null && !GetFollowRange(_followTarget, 1).Contains(base.StandingPixel))
			{
				uniqueFrameAccumulator = -1;
			}
			if (uniqueFrameAccumulator != -1)
			{
				uniqueFrameAccumulator += time.ElapsedGameTime.Milliseconds;
				if (uniqueFrameAccumulator > 500)
				{
					if (GetAnimalData()?.UseDoubleUniqueAnimationFrames ?? false)
					{
						Sprite.currentFrame = Sprite.currentFrame + 1 - Sprite.currentFrame % 2 * 2;
					}
					else if (Sprite.currentFrame > 12)
					{
						Sprite.currentFrame = (Sprite.currentFrame - 13) * 4;
					}
					else
					{
						switch (FacingDirection)
						{
						case 0:
							Sprite.currentFrame = 15;
							break;
						case 1:
							Sprite.currentFrame = 14;
							break;
						case 2:
							Sprite.currentFrame = 13;
							break;
						case 3:
							Sprite.currentFrame = 14;
							break;
						}
					}
					uniqueFrameAccumulator = 0;
					if (Game1.random.NextDouble() < 0.4)
					{
						uniqueFrameAccumulator = -1;
					}
				}
				if (IsActuallySwimming())
				{
					MovePosition(time, Game1.viewport, location);
				}
			}
			else
			{
				MovePosition(time, Game1.viewport, location);
			}
		}
		if (IsActuallySwimming())
		{
			FarmAnimalData data = GetAnimalData();
			Sprite.UpdateSourceRect();
			Microsoft.Xna.Framework.Rectangle source_rect = Sprite.SourceRect;
			source_rect.Offset(data?.SwimOffset ?? new Point(0, 112));
			Sprite.SourceRect = source_rect;
		}
		return false;
	}

	public virtual void UpdateRandomMovements()
	{
		if (!Game1.IsMasterGame || Game1.timeOfDay >= 2000 || pauseTimer > 0)
		{
			return;
		}
		if (fullness.Value < 255 && IsActuallySwimming() && Game1.random.NextDouble() < 0.002 && !isEating.Value)
		{
			Eat(base.currentLocation);
		}
		if (Game1.random.NextDouble() < 0.007 && uniqueFrameAccumulator == -1)
		{
			int newDirection = Game1.random.Next(5);
			if (newDirection != (FacingDirection + 2) % 4 || IsActuallySwimming())
			{
				if (newDirection < 4)
				{
					int oldDirection = FacingDirection;
					faceDirection(newDirection);
					if (!base.currentLocation.isOutdoors && base.currentLocation.isCollidingPosition(nextPosition(newDirection), Game1.viewport, this))
					{
						faceDirection(oldDirection);
						return;
					}
				}
				switch (newDirection)
				{
				case 0:
					SetMovingUp(b: true);
					break;
				case 1:
					SetMovingRight(b: true);
					break;
				case 2:
					SetMovingDown(b: true);
					break;
				case 3:
					SetMovingLeft(b: true);
					break;
				default:
					Halt();
					Sprite.StopAnimation();
					break;
				}
			}
			else if (noWarpTimer <= 0)
			{
				Halt();
				Sprite.StopAnimation();
			}
		}
		if (!isMoving() || !(Game1.random.NextDouble() < 0.014) || uniqueFrameAccumulator != -1)
		{
			return;
		}
		Halt();
		Sprite.StopAnimation();
		if (Game1.random.NextDouble() < 0.75)
		{
			FarmAnimalData data = GetAnimalData();
			uniqueFrameAccumulator = 0;
			if (data?.UseDoubleUniqueAnimationFrames ?? false)
			{
				switch (FacingDirection)
				{
				case 0:
					Sprite.currentFrame = 20;
					break;
				case 1:
					Sprite.currentFrame = 18;
					break;
				case 2:
					Sprite.currentFrame = 16;
					break;
				case 3:
					Sprite.currentFrame = 22;
					break;
				}
			}
			else
			{
				switch (FacingDirection)
				{
				case 0:
					Sprite.currentFrame = 15;
					break;
				case 1:
					Sprite.currentFrame = 14;
					break;
				case 2:
					Sprite.currentFrame = 13;
					break;
				case 3:
					Sprite.currentFrame = ((data?.UseFlippedRightForLeft ?? false) ? 14 : 12);
					break;
				}
			}
			uniqueFrameAccumulator = 0;
		}
		Sprite.UpdateSourceRect();
	}

	public virtual bool CanSwim()
	{
		return GetAnimalData()?.CanSwim ?? false;
	}

	public virtual bool CanFollowAdult()
	{
		if (isBaby())
		{
			return GetAnimalData()?.BabiesFollowAdults ?? false;
		}
		return false;
	}

	public override bool shouldCollideWithBuildingLayer(GameLocation location)
	{
		return true;
	}

	public virtual void HandleHop()
	{
		int hop_speed = 4;
		if (hopOffset != Vector2.Zero)
		{
			if (hopOffset.X != 0f)
			{
				int move_amount = (int)Math.Min(hop_speed, Math.Abs(hopOffset.X));
				base.Position += new Vector2(move_amount * Math.Sign(hopOffset.X), 0f);
				hopOffset.X = Utility.MoveTowards(hopOffset.X, 0f, move_amount);
			}
			if (hopOffset.Y != 0f)
			{
				int move_amount = (int)Math.Min(hop_speed, Math.Abs(hopOffset.Y));
				base.Position += new Vector2(0f, move_amount * Math.Sign(hopOffset.Y));
				hopOffset.Y = Utility.MoveTowards(hopOffset.Y, 0f, move_amount);
			}
			if (hopOffset == Vector2.Zero && isSwimming.Value)
			{
				Splash();
				_swimmingVelocity = Utility.getTranslatedVector2(Vector2.Zero, FacingDirection, base.speed);
				base.Position = new Vector2((int)Math.Round(base.Position.X), (int)Math.Round(base.Position.Y));
			}
		}
	}

	public override void MovePosition(GameTime time, xTile.Dimensions.Rectangle viewport, GameLocation currentLocation)
	{
		if (pauseTimer > 0 || Game1.IsClient)
		{
			return;
		}
		Location next_tile = nextPositionTile();
		if (!currentLocation.isTileOnMap(new Vector2(next_tile.X, next_tile.Y)))
		{
			FacingDirection = Utility.GetOppositeFacingDirection(FacingDirection);
			moveUp = facingDirection.Value == 0;
			moveLeft = facingDirection.Value == 3;
			moveDown = facingDirection.Value == 2;
			moveRight = facingDirection.Value == 1;
			_followTarget = null;
			_followTargetPosition = null;
			_swimmingVelocity = Vector2.Zero;
			return;
		}
		if (_followTarget != null && (_followTarget.currentLocation != currentLocation || (int)_followTarget.health <= 0))
		{
			_followTarget = null;
			_followTargetPosition = null;
		}
		if (_followTargetPosition.HasValue)
		{
			Point standingPixel = base.StandingPixel;
			Point targetPosition = _followTargetPosition.Value;
			Point offset = new Point(standingPixel.X - targetPosition.X, standingPixel.Y - targetPosition.Y);
			if (Math.Abs(offset.X) <= 64 || Math.Abs(offset.Y) <= 64)
			{
				moveDown = false;
				moveUp = false;
				moveLeft = false;
				moveRight = false;
				GetNewFollowPosition();
			}
			else if (nextFollowDirectionChange >= 0)
			{
				nextFollowDirectionChange -= (int)time.ElapsedGameTime.TotalMilliseconds;
			}
			else
			{
				if (IsActuallySwimming())
				{
					nextFollowDirectionChange = 100;
				}
				else
				{
					nextFollowDirectionChange = 500;
				}
				moveDown = false;
				moveUp = false;
				moveLeft = false;
				moveRight = false;
				if (Math.Abs(standingPixel.X - _followTargetPosition.Value.X) < Math.Abs(standingPixel.Y - _followTargetPosition.Value.Y))
				{
					if (standingPixel.Y > _followTargetPosition.Value.Y)
					{
						moveUp = true;
					}
					else if (standingPixel.Y < _followTargetPosition.Value.Y)
					{
						moveDown = true;
					}
				}
				else if (standingPixel.X < _followTargetPosition.Value.X)
				{
					moveRight = true;
				}
				else if (standingPixel.X > _followTargetPosition.Value.X)
				{
					moveLeft = true;
				}
			}
		}
		if (IsActuallySwimming())
		{
			Vector2 desired_movement = default(Vector2);
			if (!isEating.Value)
			{
				if (moveUp)
				{
					desired_movement.Y = -base.speed;
				}
				else if (moveDown)
				{
					desired_movement.Y = base.speed;
				}
				if (moveLeft)
				{
					desired_movement.X = -base.speed;
				}
				else if (moveRight)
				{
					desired_movement.X = base.speed;
				}
			}
			_swimmingVelocity = new Vector2(Utility.MoveTowards(_swimmingVelocity.X, desired_movement.X, 0.025f), Utility.MoveTowards(_swimmingVelocity.Y, desired_movement.Y, 0.025f));
			Vector2 old_position = base.Position;
			base.Position += _swimmingVelocity;
			Microsoft.Xna.Framework.Rectangle next_bounds = GetBoundingBox();
			base.Position = old_position;
			int moving_direction = -1;
			if (!currentLocation.isCollidingPosition(next_bounds, Game1.viewport, isFarmer: false, 0, glider: false, this, pathfinding: false))
			{
				base.Position += _swimmingVelocity;
				if (Math.Abs(_swimmingVelocity.X) > Math.Abs(_swimmingVelocity.Y))
				{
					if (_swimmingVelocity.X < 0f)
					{
						moving_direction = 3;
					}
					else if (_swimmingVelocity.X > 0f)
					{
						moving_direction = 1;
					}
				}
				else if (_swimmingVelocity.Y < 0f)
				{
					moving_direction = 0;
				}
				else if (_swimmingVelocity.Y > 0f)
				{
					moving_direction = 2;
				}
				switch (moving_direction)
				{
				case 0:
					Sprite.AnimateUp(time);
					faceDirection(0);
					break;
				case 3:
					Sprite.AnimateRight(time);
					FacingDirection = 3;
					break;
				case 1:
					Sprite.AnimateRight(time);
					faceDirection(1);
					break;
				case 2:
					Sprite.AnimateDown(time);
					faceDirection(2);
					break;
				}
			}
			else if (!HandleCollision(next_bounds))
			{
				Halt();
				Sprite.StopAnimation();
				_swimmingVelocity *= -1f;
			}
		}
		else if (moveUp)
		{
			if (!currentLocation.isCollidingPosition(nextPosition(0), Game1.viewport, isFarmer: false, 0, glider: false, this, pathfinding: false))
			{
				position.Y -= base.speed;
				Sprite.AnimateUp(time);
			}
			else if (!HandleCollision(nextPosition(0)))
			{
				Halt();
				Sprite.StopAnimation();
				if (Game1.random.NextDouble() < 0.6 || IsActuallySwimming())
				{
					SetMovingDown(b: true);
				}
			}
			faceDirection(0);
		}
		else if (moveRight)
		{
			if (!currentLocation.isCollidingPosition(nextPosition(1), Game1.viewport, isFarmer: false, 0, glider: false, this))
			{
				position.X += base.speed;
				Sprite.AnimateRight(time);
			}
			else if (!HandleCollision(nextPosition(1)))
			{
				Halt();
				Sprite.StopAnimation();
				if (Game1.random.NextDouble() < 0.6 || IsActuallySwimming())
				{
					SetMovingLeft(b: true);
				}
			}
			faceDirection(1);
		}
		else if (moveDown)
		{
			if (!currentLocation.isCollidingPosition(nextPosition(2), Game1.viewport, isFarmer: false, 0, glider: false, this))
			{
				position.Y += base.speed;
				Sprite.AnimateDown(time);
			}
			else if (!HandleCollision(nextPosition(2)))
			{
				Halt();
				Sprite.StopAnimation();
				if (Game1.random.NextDouble() < 0.6 || IsActuallySwimming())
				{
					SetMovingUp(b: true);
				}
			}
			faceDirection(2);
		}
		else
		{
			if (!moveLeft)
			{
				return;
			}
			if (!currentLocation.isCollidingPosition(nextPosition(3), Game1.viewport, isFarmer: false, 0, glider: false, this))
			{
				position.X -= base.speed;
				Sprite.AnimateRight(time);
			}
			else if (!HandleCollision(nextPosition(3)))
			{
				Halt();
				Sprite.StopAnimation();
				if (Game1.random.NextDouble() < 0.6 || IsActuallySwimming())
				{
					SetMovingRight(b: true);
				}
			}
			FacingDirection = 3;
		}
	}

	public virtual bool HandleCollision(Microsoft.Xna.Framework.Rectangle next_position)
	{
		if (_followTarget != null)
		{
			_followTarget = null;
			_followTargetPosition = null;
		}
		if (base.currentLocation.IsOutdoors && CanSwim() && (isSwimming.Value || controller == null) && wasPet.Value && hopOffset == Vector2.Zero)
		{
			base.Position = new Vector2((int)Math.Round(base.Position.X), (int)Math.Round(base.Position.Y));
			Microsoft.Xna.Framework.Rectangle current_position = GetBoundingBox();
			Vector2 offset = Utility.getTranslatedVector2(Vector2.Zero, FacingDirection, 1f);
			if (offset != Vector2.Zero)
			{
				Point hop_over_tile = base.TilePoint;
				hop_over_tile.X += (int)offset.X;
				hop_over_tile.Y += (int)offset.Y;
				offset *= 128f;
				Microsoft.Xna.Framework.Rectangle hop_destination = current_position;
				hop_destination.Offset(Utility.Vector2ToPoint(offset));
				Point hop_tile = new Point(hop_destination.X / 64, hop_destination.Y / 64);
				if (base.currentLocation.isWaterTile(hop_over_tile.X, hop_over_tile.Y) && base.currentLocation.doesTileHaveProperty(hop_over_tile.X, hop_over_tile.Y, "Passable", "Buildings") == null && !base.currentLocation.isCollidingPosition(hop_destination, Game1.viewport, isFarmer: false, 0, glider: false, this) && base.currentLocation.isOpenWater(hop_tile.X, hop_tile.Y) != isSwimming.Value)
				{
					isSwimming.Value = !isSwimming.Value;
					if (!isSwimming.Value)
					{
						Splash();
					}
					hopOffset = offset;
					pauseTimer = 0;
					doDiveEvent.Fire();
				}
				return true;
			}
		}
		return false;
	}

	public virtual bool IsActuallySwimming()
	{
		if (isSwimming.Value)
		{
			return hopOffset == Vector2.Zero;
		}
		return false;
	}

	public virtual void Splash()
	{
		if (Utility.isOnScreen(base.TilePoint, 192, base.currentLocation))
		{
			base.currentLocation.playSound("dropItemInWater");
		}
		Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite(28, 100f, 2, 1, getStandingPosition() + new Vector2(-0.5f, -0.5f) * 64f, flicker: false, flipped: false)
		{
			delayBeforeAnimationStart = 0,
			layerDepth = (float)base.StandingPixel.Y / 10000f
		});
	}

	public override void animateInFacingDirection(GameTime time)
	{
		if (FacingDirection == 3)
		{
			Sprite.AnimateRight(time);
		}
		else
		{
			base.animateInFacingDirection(time);
		}
	}

	/// <summary>Log warnings if the farm animal's sprite is incorrectly sized, which would otherwise lead to hard-to-diagnose issues like animals freezing.</summary>
	private void ValidateSpritesheetSize()
	{
		int expectedRows = 5 + ((!Sprite.textureUsesFlippedRightForLeft) ? 1 : 0) + ((GetAnimalData()?.UseDoubleUniqueAnimationFrames ?? false) ? 1 : 0);
		if (Sprite.Texture.Height < expectedRows * Sprite.SpriteHeight)
		{
			Game1.log.Warn($"Farm animal '{type.Value}' has sprite height {Sprite.Texture.Height}px, but expected at least {expectedRows * Sprite.SpriteHeight}px based on its data. This may cause issues like frozen animations.");
		}
		if (Sprite.Texture.Width != 4 * Sprite.SpriteWidth)
		{
			Game1.log.Warn($"Farm animal '{type.Value}' has sprite width {Sprite.Texture.Width}px, but it should be exactly {4 * Sprite.SpriteWidth}px.");
		}
	}
}
