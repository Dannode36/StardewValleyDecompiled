using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.GameData.Pets;
using StardewValley.Internal;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.Objects;

namespace StardewValley.Characters;

public class Pet : NPC
{
	/// <summary>The cat's pet type ID in <c>Data/Pets</c>.</summary>
	public const string type_cat = "Cat";

	/// <summary>The dog's pet type ID in <c>Data/Pets</c>.</summary>
	public const string type_dog = "Dog";

	/// <summary>A unique ID for this pet.</summary>
	/// <remarks>This matches the <see cref="F:StardewValley.Buildings.PetBowl.petId" /> of the pet's bowl, if any. See also <see cref="M:StardewValley.Characters.Pet.GetPetBowl" />.</remarks>
	[XmlElement("guid")]
	public NetGuid petId = new NetGuid(Guid.NewGuid());

	public const int bedTime = 2000;

	public const int maxFriendship = 1000;

	public const string behavior_Walk = "Walk";

	public const string behavior_Sleep = "Sleep";

	public const string behavior_SitDown = "SitDown";

	public const string behavior_Sprint = "Sprint";

	protected int behaviorTimer = -1;

	protected int animationLoopsLeft;

	[XmlElement("petType")]
	public readonly NetString petType = new NetString("Dog");

	[XmlElement("whichBreed")]
	public readonly NetString whichBreed = new NetString("0");

	private readonly NetString netCurrentBehavior = new NetString();

	/// <summary>The unique name of the location containing the pet's bowl, if any.</summary>
	[XmlElement("homeLocationName")]
	public readonly NetString homeLocationName = new NetString();

	[XmlIgnore]
	public readonly NetEvent1Field<long, NetLong> petPushEvent = new NetEvent1Field<long, NetLong>();

	[XmlIgnore]
	protected string _currentBehavior;

	[XmlElement("lastPetDay")]
	public NetLongDictionary<int, NetInt> lastPetDay = new NetLongDictionary<int, NetInt>();

	[XmlElement("grantedFriendshipForPet")]
	public NetBool grantedFriendshipForPet = new NetBool(value: false);

	[XmlElement("friendshipTowardFarmer")]
	public NetInt friendshipTowardFarmer = new NetInt(0);

	[XmlElement("timesPet")]
	public NetInt timesPet = new NetInt(0);

	[XmlElement("hat")]
	public readonly NetRef<Hat> hat = new NetRef<Hat>();

	protected int _walkFromPushTimer;

	public NetBool isSleepingOnFarmerBed = new NetBool(value: false);

	[XmlIgnore]
	public readonly NetMutex mutex = new NetMutex();

	private int pushingTimer;

	/// <inheritdoc />
	[XmlIgnore]
	public override bool IsVillager => false;

	public string CurrentBehavior
	{
		get
		{
			return netCurrentBehavior.Value;
		}
		set
		{
			if (netCurrentBehavior.Value != value)
			{
				netCurrentBehavior.Value = value;
			}
		}
	}

	public override void reloadData()
	{
	}

	protected override string translateName()
	{
		return name.Value.Trim();
	}

	public Pet(int xTile, int yTile, string petBreed, string petType)
	{
		base.Name = petType;
		displayName = name;
		this.petType.Value = petType;
		whichBreed.Value = petBreed;
		Sprite = new AnimatedSprite(getPetTextureName(), 0, 32, 32);
		base.Position = new Vector2(xTile, yTile) * 64f;
		base.Breather = false;
		base.willDestroyObjectsUnderfoot = false;
		base.currentLocation = Game1.currentLocation;
		base.HideShadow = true;
	}

	public Pet()
		: this(0, 0, "0", "Dog")
	{
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(petId, "petId").AddField(petType, "petType").AddField(whichBreed, "whichBreed")
			.AddField(netCurrentBehavior, "netCurrentBehavior")
			.AddField(homeLocationName, "homeLocationName")
			.AddField(petPushEvent, "petPushEvent")
			.AddField(lastPetDay, "lastPetDay")
			.AddField(grantedFriendshipForPet, "grantedFriendshipForPet")
			.AddField(friendshipTowardFarmer, "friendshipTowardFarmer")
			.AddField(isSleepingOnFarmerBed, "isSleepingOnFarmerBed")
			.AddField(mutex.NetFields, "mutex.NetFields")
			.AddField(hat, "hat")
			.AddField(timesPet, "timesPet");
		name.FilterStringEvent += Utility.FilterDirtyWords;
		name.fieldChangeVisibleEvent += delegate
		{
			resetCachedDisplayName();
		};
		petPushEvent.onEvent += OnPetPush;
		friendshipTowardFarmer.fieldChangeVisibleEvent += delegate
		{
			GrantLoveMailIfNecessary();
		};
		isSleepingOnFarmerBed.fieldChangeVisibleEvent += delegate
		{
			UpdateSleepingOnBed();
		};
		petType.fieldChangeVisibleEvent += delegate
		{
			reloadBreedSprite();
		};
		whichBreed.fieldChangeVisibleEvent += delegate
		{
			reloadBreedSprite();
		};
		netCurrentBehavior.fieldChangeVisibleEvent += delegate
		{
			if (_currentBehavior != CurrentBehavior)
			{
				_OnNewBehavior();
			}
		};
	}

	public virtual void OnPetPush(long farmerId)
	{
		pushingTimer = 0;
		if (Game1.IsMasterGame)
		{
			Farmer farmer = Game1.getFarmer(farmerId);
			Vector2 trajectory = Utility.getAwayFromPlayerTrajectory(GetBoundingBox(), farmer);
			setTrajectory((int)trajectory.X / 2, (int)trajectory.Y / 2);
			_walkFromPushTimer = 250;
			CurrentBehavior = "Walk";
			OnNewBehavior();
			Halt();
			faceDirection(farmer.FacingDirection);
			setMovingInFacingDirection();
		}
	}

	public override int getTimeFarmerMustPushBeforeStartShaking()
	{
		return 300;
	}

	public override int getTimeFarmerMustPushBeforePassingThrough()
	{
		return 750;
	}

	public override void behaviorOnFarmerLocationEntry(GameLocation location, Farmer who)
	{
		base.behaviorOnFarmerLocationEntry(location, who);
		if (location is Farm && Game1.timeOfDay >= 2000 && !location.farmers.Any())
		{
			if (CurrentBehavior != "Sleep" || base.currentLocation is Farm)
			{
				Game1.player.team.requestPetWarpHomeEvent.Fire(Game1.player.UniqueMultiplayerID);
			}
		}
		else if (Game1.timeOfDay < 2000 && Game1.random.NextBool() && _currentBehavior != "Sleep")
		{
			CurrentBehavior = "Sleep";
			_OnNewBehavior();
			Sprite.UpdateSourceRect();
		}
		UpdateSleepingOnBed();
	}

	public override void behaviorOnLocalFarmerLocationEntry(GameLocation location)
	{
		base.behaviorOnLocalFarmerLocationEntry(location);
		netCurrentBehavior.CancelInterpolation();
		if (netCurrentBehavior.Value == "Sleep")
		{
			position.NetFields.CancelInterpolation();
			if (_currentBehavior != "Sleep")
			{
				_OnNewBehavior();
				Sprite.UpdateSourceRect();
			}
		}
		UpdateSleepingOnBed();
	}

	public override bool canTalk()
	{
		return false;
	}

	/// <summary>Get the data from <c>Data/Pets</c> for the pet type, if it's valid.</summary>
	public PetData GetPetData()
	{
		if (!TryGetData(petType.Value, out var petData))
		{
			return null;
		}
		return petData;
	}

	/// <summary>Get the underlying content data for a pet type, if any.</summary>
	/// <param name="petType">The pet type's ID in <c>Data/Pets</c>.</param>
	/// <param name="data">The pet data, if found.</param>
	/// <returns>Returns whether the pet data was found.</returns>
	public static bool TryGetData(string petType, out PetData data)
	{
		if (petType != null && Game1.petData.TryGetValue(petType, out data))
		{
			return true;
		}
		data = null;
		return false;
	}

	/// <summary>Get the icon to show in the game menu for this pet.</summary>
	/// <param name="assetName">The asset name for the texture.</param>
	/// <param name="sourceRect">The 16x16 pixel area within the texture for the icon.</param>
	public void GetPetIcon(out string assetName, out Rectangle sourceRect)
	{
		PetData petData = GetPetData();
		PetData dogData;
		PetBreed breed = petData?.GetBreedById(whichBreed.Value) ?? petData?.Breeds?.FirstOrDefault() ?? ((!TryGetData("Dog", out dogData)) ? null : dogData.Breeds?.FirstOrDefault());
		if (breed != null)
		{
			assetName = breed.IconTexture;
			sourceRect = breed.IconSourceRect;
		}
		else
		{
			assetName = "Animals\\dog";
			sourceRect = new Rectangle(208, 208, 16, 16);
		}
	}

	public virtual string getPetTextureName()
	{
		try
		{
			PetData petType = GetPetData();
			if (petType != null)
			{
				return petType.GetBreedById(whichBreed.Value).Texture;
			}
		}
		catch (Exception)
		{
		}
		return "Animals\\dog";
	}

	public void reloadBreedSprite()
	{
		Sprite?.LoadTexture(getPetTextureName());
	}

	/// <inheritdoc />
	public override void reloadSprite(bool onlyAppearance = false)
	{
		reloadBreedSprite();
		base.HideShadow = true;
		base.Breather = false;
		if (!onlyAppearance)
		{
			base.DefaultPosition = new Vector2(54f, 8f) * 64f;
			setAtFarmPosition();
			if (GetPetBowl() == null)
			{
				warpToFarmHouse(Game1.MasterPlayer);
			}
			GrantLoveMailIfNecessary();
		}
	}

	/// <inheritdoc />
	public override void ChooseAppearance(LocalizedContentManager content = null)
	{
		if (Sprite?.Texture == null)
		{
			reloadSprite(onlyAppearance: true);
		}
	}

	public void warpToFarmHouse(Farmer who)
	{
		PetData petData = GetPetData();
		isSleepingOnFarmerBed.Value = false;
		FarmHouse farmHouse = Utility.getHomeOfFarmer(who);
		int tries = 0;
		Vector2 sleepTile = new Vector2(Game1.random.Next(2, farmHouse.map.Layers[0].LayerWidth - 3), Game1.random.Next(3, farmHouse.map.Layers[0].LayerHeight - 5));
		List<Furniture> rugs = new List<Furniture>();
		foreach (Furniture house_furniture in farmHouse.furniture)
		{
			if ((int)house_furniture.furniture_type == 12)
			{
				rugs.Add(house_furniture);
			}
		}
		BedFurniture player_bed = farmHouse.GetPlayerBed();
		float sleepOnBedChance = 0f;
		float sleepAtBedFootChance = 0.3f;
		float sleepOnRugChance = 0.5f;
		if (petData != null)
		{
			sleepOnBedChance = petData.SleepOnBedChance;
			sleepAtBedFootChance = petData.SleepNearBedChance;
			sleepOnRugChance = petData.SleepOnRugChance;
		}
		if (player_bed != null && !Game1.newDay && Game1.timeOfDay >= 2000 && Game1.random.NextDouble() <= (double)sleepOnBedChance)
		{
			sleepTile = Utility.PointToVector2(player_bed.GetBedSpot()) + new Vector2(-1f, 0f);
			if (farmHouse.isCharacterAtTile(sleepTile) == null)
			{
				Game1.warpCharacter(this, farmHouse, sleepTile);
				base.NetFields.CancelInterpolation();
				CurrentBehavior = "Sleep";
				isSleepingOnFarmerBed.Value = true;
				Rectangle petBounds = GetBoundingBox();
				foreach (Furniture item in farmHouse.furniture)
				{
					if (item is BedFurniture bed && bed.GetBoundingBox().Intersects(petBounds))
					{
						bed.ReserveForNPC();
						break;
					}
				}
				UpdateSleepingOnBed();
				_OnNewBehavior();
				Sprite.UpdateSourceRect();
				return;
			}
		}
		else if (Game1.random.NextDouble() <= (double)sleepAtBedFootChance)
		{
			sleepTile = Utility.PointToVector2(farmHouse.getBedSpot()) + new Vector2(0f, 2f);
		}
		else if (Game1.random.NextDouble() <= (double)sleepOnRugChance)
		{
			Furniture rug = Game1.random.ChooseFrom(rugs);
			if (rug != null)
			{
				sleepTile = Utility.getRandomPositionInThisRectangle(rug.boundingBox.Value, Game1.random) / 64f;
			}
		}
		for (; tries < 50; tries++)
		{
			if (farmHouse.canPetWarpHere(sleepTile) && farmHouse.CanItemBePlacedHere(sleepTile, itemIsPassable: false, ~CollisionMask.Farmers) && farmHouse.CanItemBePlacedHere(sleepTile + new Vector2(1f, 0f), itemIsPassable: false, ~CollisionMask.Farmers) && !farmHouse.isTileOnWall((int)sleepTile.X, (int)sleepTile.Y))
			{
				break;
			}
			sleepTile = new Vector2(Game1.random.Next(2, farmHouse.map.Layers[0].LayerWidth - 3), Game1.random.Next(3, farmHouse.map.Layers[0].LayerHeight - 4));
		}
		if (tries < 50)
		{
			Game1.warpCharacter(this, farmHouse, sleepTile);
			CurrentBehavior = "Sleep";
		}
		else
		{
			WarpToPetBowl();
		}
		UpdateSleepingOnBed();
		_OnNewBehavior();
		Sprite.UpdateSourceRect();
	}

	public virtual void UpdateSleepingOnBed()
	{
		drawOnTop = false;
		collidesWithOtherCharacters.Value = !isSleepingOnFarmerBed.Value;
		farmerPassesThrough = isSleepingOnFarmerBed.Value;
	}

	public override void dayUpdate(int dayOfMonth)
	{
		isSleepingOnFarmerBed.Value = false;
		UpdateSleepingOnBed();
		base.DefaultPosition = new Vector2(54f, 8f) * 64f;
		Sprite.loop = false;
		base.Breather = false;
		if (Game1.IsMasterGame && GetPetBowl() == null)
		{
			foreach (Building building in Game1.getFarm().buildings)
			{
				if (building is PetBowl bowl && !bowl.HasPet())
				{
					bowl.AssignPet(this);
					break;
				}
			}
		}
		PetBowl petBowl = GetPetBowl();
		if (Game1.isRaining)
		{
			CurrentBehavior = "SitDown";
			warpToFarmHouse(Game1.player);
		}
		else if (petBowl != null && base.currentLocation is FarmHouse)
		{
			setAtFarmPosition();
		}
		else if (petBowl == null)
		{
			warpToFarmHouse(Game1.player);
		}
		if (Game1.IsMasterGame)
		{
			if (petBowl != null && petBowl.watered.Value)
			{
				friendshipTowardFarmer.Set(Math.Min(1000, friendshipTowardFarmer.Value + 6));
				petBowl.watered.Set(newValue: false);
			}
			if (petBowl == null)
			{
				friendshipTowardFarmer.Value -= 10;
			}
		}
		if (petBowl == null)
		{
			Game1.addMorningFluffFunction(delegate
			{
				doEmote(28);
			});
		}
		Halt();
		CurrentBehavior = "Sleep";
		grantedFriendshipForPet.Set(newValue: false);
		_OnNewBehavior();
		Sprite.UpdateSourceRect();
	}

	public void GrantLoveMailIfNecessary()
	{
		if (friendshipTowardFarmer.Value < 1000)
		{
			return;
		}
		foreach (Farmer farmer in Game1.getAllFarmers())
		{
			if (farmer != null && farmer.mailReceived.Add("petLoveMessage") && farmer == Game1.player)
			{
				if (Game1.newDay)
				{
					Game1.addMorningFluffFunction(delegate
					{
						Game1.showGlobalMessage(Game1.content.LoadString("Strings\\Characters:PetLovesYou", displayName));
					});
				}
				else
				{
					Game1.showGlobalMessage(Game1.content.LoadString("Strings\\Characters:PetLovesYou", displayName));
				}
			}
			if (!farmer.hasOrWillReceiveMail("MarniePetAdoption"))
			{
				Game1.addMailForTomorrow("MarniePetAdoption");
			}
		}
	}

	/// <summary>Get the pet bowl assigned to this pet, if any.</summary>
	public PetBowl GetPetBowl()
	{
		foreach (Building building in (Game1.getLocationFromName(homeLocationName.Value) ?? Game1.getFarm()).buildings)
		{
			if (building is PetBowl bowl && bowl.petId.Value == petId.Value)
			{
				return bowl;
			}
		}
		return null;
	}

	/// <summary>Warp the pet to its assigned pet bowl, if any.</summary>
	public virtual void WarpToPetBowl()
	{
		PetBowl bowl = GetPetBowl();
		if (bowl != null)
		{
			faceDirection(2);
			Game1.warpCharacter(this, bowl.parentLocationName.Value, bowl.GetPetSpot());
		}
	}

	public void setAtFarmPosition()
	{
		if (Game1.IsMasterGame)
		{
			if (!Game1.isRaining)
			{
				WarpToPetBowl();
			}
			else
			{
				warpToFarmHouse(Game1.MasterPlayer);
			}
		}
	}

	public override bool shouldCollideWithBuildingLayer(GameLocation location)
	{
		return true;
	}

	public override bool canPassThroughActionTiles()
	{
		return false;
	}

	public void unassignPetBowl()
	{
		foreach (Building building in (Game1.getLocationFromName(homeLocationName.Value) ?? Game1.getFarm()).buildings)
		{
			if (building is PetBowl bowl && bowl.petId.Value == petId.Value)
			{
				bowl.petId.Value = Guid.Empty;
			}
		}
	}

	public void applyButterflyPowder(Farmer who, string responseKey)
	{
		if (responseKey.Contains("Yes"))
		{
			GameLocation l = base.currentLocation;
			unassignPetBowl();
			l.characters.Remove(this);
			playContentSound();
			Game1.playSound("fireball");
			Rectangle r = GetBoundingBox();
			r.Inflate(32, 32);
			r.X -= 32;
			r.Y -= 32;
			l.temporarySprites.AddRange(Utility.sparkleWithinArea(r, 6, Color.White, 50));
			l.temporarySprites.Add(new TemporaryAnimatedSprite(5, Utility.PointToVector2(GetBoundingBox().Center) - new Vector2(32f), Color.White, 8, flipped: false, 50f));
			for (int i = 0; i < 8; i++)
			{
				l.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(372, 1956, 10, 10), base.Position + new Vector2(32f) + new Vector2(Game1.random.Next(-16, 16), Game1.random.Next(-32, 16)), flipped: false, 0.002f, Color.White)
				{
					alphaFade = 0.0043333336f,
					alpha = 0.75f,
					motion = new Vector2((float)Game1.random.Next(-10, 11) / 20f, -1f),
					acceleration = new Vector2(0f, 0f),
					interval = 99999f,
					layerDepth = 1f,
					scale = 3f,
					scaleChange = 0.01f,
					rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f
				});
			}
			l.instantiateCrittersList();
			l.addCritter(new Butterfly(l, base.Tile + new Vector2(0f, 1f)));
			who.reduceActiveItemByOne();
			if (hat.Value != null)
			{
				Game1.createItemDebris(hat.Value, base.Position, -1, l);
			}
			Game1.showGlobalMessage(Game1.content.LoadString("Strings\\1_6_Strings:ButterflyPowder_Goodbye", base.Name));
		}
	}

	public override bool checkAction(Farmer who, GameLocation l)
	{
		if (who.Items.Count > who.CurrentToolIndex && who.Items[who.CurrentToolIndex] != null && who.Items[who.CurrentToolIndex] is Hat && (petType == "Cat" || petType == "Dog"))
		{
			if (hat.Value != null)
			{
				Game1.createItemDebris(hat.Value, base.Position, FacingDirection);
				hat.Value = null;
			}
			else
			{
				Hat hatItem = who.Items[who.CurrentToolIndex] as Hat;
				who.Items[who.CurrentToolIndex] = null;
				hat.Value = hatItem;
				Game1.playSound("dirtyHit");
			}
			mutex.ReleaseLock();
		}
		if (who.CurrentItem != null && who.CurrentItem.QualifiedItemId.Equals("(O)ButterflyPowder"))
		{
			l.createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:ButterflyPowder_Question", base.Name), l.createYesNoResponses(), applyButterflyPowder);
		}
		if (!lastPetDay.TryGetValue(who.UniqueMultiplayerID, out var curLastPetDay) || curLastPetDay != Game1.Date.TotalDays)
		{
			lastPetDay[who.UniqueMultiplayerID] = Game1.Date.TotalDays;
			mutex.RequestLock(delegate
			{
				if (!grantedFriendshipForPet.Value)
				{
					grantedFriendshipForPet.Set(newValue: true);
					friendshipTowardFarmer.Set(Math.Min(1000, (int)friendshipTowardFarmer + 12));
					if (Utility.CreateDaySaveRandom(timesPet.Value, 71928.0, petId.Value.GetHashCode()).NextDouble() < (double)GetPetData().GiftChance)
					{
						Item item = TryGetGiftItem(GetPetData().Gifts);
						if (item != null)
						{
							Game1.createMultipleItemDebris(item, base.Position, -1, l, -1, flopFish: true);
						}
					}
					timesPet.Value++;
				}
				mutex.ReleaseLock();
			});
			doEmote(20);
			playContentSound();
			return true;
		}
		return false;
	}

	public virtual void playContentSound()
	{
		if (!Utility.isOnScreen(base.TilePoint, 128, base.currentLocation) || Game1.options.muteAnimalSounds)
		{
			return;
		}
		PetData petData = GetPetData();
		if (petData == null || petData.ContentSound == null)
		{
			return;
		}
		string contentSound = petData.ContentSound;
		PlaySound(contentSound, is_voice: true, -1, -1);
		if (petData.RepeatContentSoundAfter >= 0)
		{
			DelayedAction.functionAfterDelay(delegate
			{
				PlaySound(contentSound, is_voice: true, -1, -1);
			}, petData.RepeatContentSoundAfter);
		}
	}

	public void hold(Farmer who)
	{
		FarmerSprite.AnimationFrame lastFrame = Sprite.CurrentAnimation.Last();
		flip = lastFrame.flip;
		Sprite.CurrentFrame = lastFrame.frame;
		Sprite.CurrentAnimation = null;
		Sprite.loop = false;
	}

	public override void behaviorOnFarmerPushing()
	{
		if (!(CurrentBehavior == "Sprint"))
		{
			pushingTimer += 2;
			if (pushingTimer > 100)
			{
				petPushEvent.Fire(Game1.player.UniqueMultiplayerID);
			}
		}
	}

	public override void update(GameTime time, GameLocation location, long id, bool move)
	{
		base.update(time, location, id, move);
		pushingTimer = Math.Max(0, pushingTimer - 1);
	}

	public override void update(GameTime time, GameLocation location)
	{
		base.update(time, location);
		petPushEvent.Poll();
		if (isSleepingOnFarmerBed.Value && CurrentBehavior != "Sleep" && Game1.IsMasterGame)
		{
			isSleepingOnFarmerBed.Value = false;
			UpdateSleepingOnBed();
		}
		if (base.currentLocation == null)
		{
			base.currentLocation = location;
		}
		mutex.Update(location);
		if (Game1.eventUp)
		{
			return;
		}
		if (_currentBehavior != CurrentBehavior)
		{
			_OnNewBehavior();
		}
		RunState(time);
		if (Game1.IsMasterGame)
		{
			PetBehavior currentBehavior = GetCurrentPetBehavior();
			if (currentBehavior != null && currentBehavior.WalkInDirection)
			{
				if (currentBehavior.Animation == null)
				{
					MovePosition(time, Game1.viewport, location);
				}
				else
				{
					tryToMoveInDirection(FacingDirection, isFarmer: false, -1, glider: false);
				}
			}
		}
		flip = false;
		if (FacingDirection == 3 && Sprite.CurrentFrame >= 16)
		{
			flip = true;
		}
	}

	public Item TryGetGiftItem(List<PetGift> gifts)
	{
		float totalWeight = 0f;
		foreach (PetGift gift in gifts)
		{
			if (gift.MinimumFriendshipThreshold <= friendshipTowardFarmer.Value)
			{
				totalWeight += gift.Weight;
			}
		}
		totalWeight = Utility.RandomFloat(0f, totalWeight);
		foreach (PetGift gift in gifts)
		{
			if (gift.MinimumFriendshipThreshold > friendshipTowardFarmer.Value)
			{
				continue;
			}
			totalWeight -= gift.Weight;
			if (totalWeight <= 0f)
			{
				Item i = ItemQueryResolver.TryResolveRandomItem(gift.QualifiedItemID, null);
				if (i != null && !i.Name.Contains("Error"))
				{
					i.Stack = gift.Stack;
					return i;
				}
				return ItemRegistry.Create(gift.QualifiedItemID, gift.Stack);
			}
		}
		return null;
	}

	public bool TryBehaviorChange(List<PetBehaviorChanges> changes)
	{
		float totalWeight = 0f;
		foreach (PetBehaviorChanges change in changes)
		{
			if (!change.OutsideOnly || base.currentLocation.IsOutdoors)
			{
				totalWeight += change.Weight;
			}
		}
		totalWeight = Utility.RandomFloat(0f, totalWeight);
		foreach (PetBehaviorChanges change in changes)
		{
			if (change.OutsideOnly && !base.currentLocation.IsOutdoors)
			{
				continue;
			}
			totalWeight -= change.Weight;
			if (totalWeight <= 0f)
			{
				string nextBehavior = null;
				switch (FacingDirection)
				{
				case 0:
					nextBehavior = change.UpBehavior;
					break;
				case 2:
					nextBehavior = change.DownBehavior;
					break;
				case 3:
					nextBehavior = change.LeftBehavior;
					break;
				case 1:
					nextBehavior = change.RightBehavior;
					break;
				}
				if (nextBehavior == null)
				{
					nextBehavior = change.Behavior;
				}
				if (nextBehavior != null)
				{
					CurrentBehavior = nextBehavior;
				}
				return true;
			}
		}
		return false;
	}

	public PetBehavior GetCurrentPetBehavior()
	{
		PetData petData = GetPetData();
		if (petData?.Behaviors != null)
		{
			foreach (PetBehavior behavior in petData.Behaviors)
			{
				if (behavior.Id == CurrentBehavior)
				{
					return behavior;
				}
			}
		}
		return null;
	}

	public virtual void RunState(GameTime time)
	{
		if (_currentBehavior == "Walk" && Game1.IsMasterGame && _walkFromPushTimer <= 0 && base.currentLocation.isCollidingPosition(nextPosition(FacingDirection), Game1.viewport, this))
		{
			int new_direction = Game1.random.Next(0, 4);
			if (!base.currentLocation.isCollidingPosition(nextPosition(FacingDirection), Game1.viewport, this))
			{
				faceDirection(new_direction);
			}
		}
		if (Game1.IsMasterGame && Game1.timeOfDay >= 2000 && Sprite.CurrentAnimation == null && xVelocity == 0f && yVelocity == 0f)
		{
			CurrentBehavior = "Sleep";
		}
		if (CurrentBehavior == "Sleep")
		{
			if (Game1.IsMasterGame && Game1.timeOfDay < 2000 && Game1.random.NextDouble() < 0.001)
			{
				CurrentBehavior = "Walk";
			}
			if (Game1.random.NextDouble() < 0.002)
			{
				doEmote(24);
			}
		}
		if (_walkFromPushTimer > 0)
		{
			_walkFromPushTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
			if (_walkFromPushTimer <= 0)
			{
				_walkFromPushTimer = 0;
			}
		}
		PetBehavior behavior = GetCurrentPetBehavior();
		if (behavior == null || !Game1.IsMasterGame)
		{
			return;
		}
		if (behaviorTimer >= 0)
		{
			behaviorTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
			if (behaviorTimer <= 0)
			{
				behaviorTimer = -1;
				TryBehaviorChange(behavior.TimeoutBehaviorChanges);
				return;
			}
		}
		if (_walkFromPushTimer <= 0)
		{
			if (behavior.RandomBehaviorChanges != null && behavior.RandomBehaviorChangeChance > 0f && Game1.random.NextDouble() < (double)behavior.RandomBehaviorChangeChance)
			{
				TryBehaviorChange(behavior.RandomBehaviorChanges);
				return;
			}
			if (behavior.PlayerNearbyBehaviorChanges != null && withinPlayerThreshold(2))
			{
				TryBehaviorChange(behavior.PlayerNearbyBehaviorChanges);
				return;
			}
		}
		if (behavior.JumpLandBehaviorChanges != null && yJumpOffset == 0 && yJumpVelocity == 0f)
		{
			TryBehaviorChange(behavior.JumpLandBehaviorChanges);
		}
	}

	protected override void updateSlaveAnimation(GameTime time)
	{
		if (Sprite.CurrentAnimation != null)
		{
			Sprite.animateOnce(time);
		}
		else
		{
			if (!(CurrentBehavior == "Walk"))
			{
				return;
			}
			Sprite.faceDirection(FacingDirection);
			if (isMoving())
			{
				animateInFacingDirection(time);
				int target = -1;
				switch (FacingDirection)
				{
				case 0:
					target = 12;
					break;
				case 2:
					target = 4;
					break;
				case 3:
					target = 16;
					break;
				case 1:
					target = 8;
					break;
				}
				if (Sprite.CurrentFrame == target)
				{
					Sprite.CurrentFrame -= 4;
				}
			}
			else
			{
				Sprite.StopAnimation();
			}
		}
	}

	protected void _OnNewBehavior()
	{
		_currentBehavior = CurrentBehavior;
		Halt();
		Sprite.CurrentAnimation = null;
		OnNewBehavior();
	}

	public virtual void OnNewBehavior()
	{
		Sprite.loop = false;
		Sprite.CurrentAnimation = null;
		behaviorTimer = -1;
		animationLoopsLeft = -1;
		if (CurrentBehavior == "Sleep")
		{
			Sprite.loop = true;
			bool local_sleep_flip = Game1.random.NextBool();
			Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
			{
				new FarmerSprite.AnimationFrame(28, 1000, secondaryArm: false, local_sleep_flip),
				new FarmerSprite.AnimationFrame(29, 1000, secondaryArm: false, local_sleep_flip)
			});
		}
		PetBehavior behavior = GetCurrentPetBehavior();
		if (behavior == null)
		{
			return;
		}
		if (Game1.IsMasterGame)
		{
			if (_walkFromPushTimer <= 0)
			{
				if (Utility.TryParseDirection(behavior.Direction, out var direction))
				{
					FacingDirection = direction;
				}
				if (behavior.RandomizeDirection)
				{
					FacingDirection = (behavior.IsSideBehavior ? Game1.random.Choose(3, 1) : Game1.random.Next(4));
				}
			}
			if ((FacingDirection == 0 || FacingDirection == 2) && behavior.IsSideBehavior)
			{
				FacingDirection = ((!Game1.random.NextBool()) ? 1 : 3);
			}
			if (behavior.WalkInDirection)
			{
				if (behavior.MoveSpeed >= 0)
				{
					base.speed = behavior.MoveSpeed;
				}
				setMovingInFacingDirection();
			}
			if (behavior.Duration >= 0)
			{
				behaviorTimer = behavior.Duration;
			}
			else if (behavior.MinimumDuration >= 0 && behavior.MaximumDuration >= 0)
			{
				behaviorTimer = Game1.random.Next(behavior.MinimumDuration, behavior.MaximumDuration + 1);
			}
		}
		if (behavior.SoundOnStart != null)
		{
			PlaySound(behavior.SoundOnStart, behavior.SoundIsVoice, behavior.SoundRangeFromBorder, behavior.SoundRange);
		}
		if (behavior.Shake > 0)
		{
			shake(behavior.Shake);
		}
		if (behavior.Animation == null)
		{
			return;
		}
		Sprite.ClearAnimation();
		for (int i = 0; i < behavior.Animation.Count; i++)
		{
			FarmerSprite.AnimationFrame frame = new FarmerSprite.AnimationFrame(behavior.Animation[i].Frame, behavior.Animation[i].Duration, secondaryArm: false, flip: false);
			if (behavior.Animation[i].HitGround)
			{
				frame.AddFrameAction(hitGround);
			}
			if (behavior.Animation[i].Jump)
			{
				jump();
			}
			if (behavior.AnimationMinimumLoops >= 0 && behavior.AnimationMaximumLoops >= 0)
			{
				animationLoopsLeft = Game1.random.Next(behavior.AnimationMinimumLoops, behavior.AnimationMaximumLoops + 1);
			}
			if (behavior.Animation[i].Sound != null)
			{
				frame.AddFrameAction(_PerformAnimationSound);
			}
			if (i == behavior.Animation.Count - 1)
			{
				if (animationLoopsLeft > 0 || behavior.AnimationEndBehaviorChanges != null)
				{
					frame.AddFrameEndAction(_TryAnimationEndBehaviorChange);
				}
				if (behavior.LoopMode == PetAnimationLoopMode.Hold)
				{
					if (behavior.AnimationEndBehaviorChanges != null)
					{
						frame.AddFrameEndAction(hold);
					}
					else
					{
						frame.AddFrameAction(hold);
					}
				}
			}
			Sprite.AddFrame(frame);
			if (behavior.Animation.Count == 1 && behavior.LoopMode == PetAnimationLoopMode.Hold)
			{
				Sprite.AddFrame(frame);
			}
			Sprite.UpdateSourceRect();
		}
		Sprite.loop = behavior.LoopMode == PetAnimationLoopMode.Loop || animationLoopsLeft > 0;
	}

	public void _PerformAnimationSound(Farmer who)
	{
		PetBehavior behavior = GetCurrentPetBehavior();
		if (behavior?.Animation != null && Sprite.currentAnimationIndex >= 0 && Sprite.currentAnimationIndex < behavior.Animation.Count)
		{
			PetAnimationFrame frame = behavior.Animation[Sprite.currentAnimationIndex];
			if (frame.Sound != null)
			{
				PlaySound(frame.Sound, frame.SoundIsVoice, frame.SoundRangeFromBorder, frame.SoundRange);
			}
		}
	}

	public void PlaySound(string sound, bool is_voice, int range_from_border, int range)
	{
		if ((Game1.options.muteAnimalSounds && is_voice) || !IsSoundInRange(range_from_border, range))
		{
			return;
		}
		float pitch = 1f;
		PetBreed breed = GetPetData().GetBreedById(whichBreed.Value);
		if (sound == "BARK")
		{
			sound = GetPetData().BarkSound;
			if (breed.BarkOverride != null)
			{
				sound = breed.BarkOverride;
			}
		}
		if (is_voice)
		{
			pitch = breed.VoicePitch;
		}
		if (pitch != 1f)
		{
			playNearbySoundAll(sound, (int)(1200f * pitch));
		}
		else
		{
			Game1.playSound(sound);
		}
	}

	public bool IsSoundInRange(int range_from_border, int sound_range)
	{
		if (sound_range > 0)
		{
			return withinLocalPlayerThreshold(sound_range);
		}
		if (range_from_border > 0)
		{
			return Utility.isOnScreen(base.TilePoint, range_from_border * 64, base.currentLocation);
		}
		return true;
	}

	public virtual void _TryAnimationEndBehaviorChange(Farmer who)
	{
		if (animationLoopsLeft <= 0)
		{
			if (animationLoopsLeft == 0)
			{
				animationLoopsLeft = -1;
				hold(who);
			}
			PetBehavior behavior = GetCurrentPetBehavior();
			if (behavior != null && Game1.IsMasterGame)
			{
				TryBehaviorChange(behavior.AnimationEndBehaviorChanges);
			}
		}
		else
		{
			animationLoopsLeft--;
		}
	}

	public override Rectangle GetBoundingBox()
	{
		Vector2 position = base.Position;
		return new Rectangle((int)position.X + 16, (int)position.Y + 16, Sprite.SpriteWidth * 4 * 3 / 4, 32);
	}

	public virtual void drawHat(SpriteBatch b, Vector2 shake)
	{
		if (hat.Value == null)
		{
			return;
		}
		Vector2 hatOffset = Vector2.Zero;
		hatOffset *= 4f;
		if (hatOffset.X <= -100f)
		{
			return;
		}
		float horse_draw_layer = Math.Max(0f, isSleepingOnFarmerBed.Value ? (((float)base.StandingPixel.Y + 112f) / 10000f) : ((float)base.StandingPixel.Y / 10000f));
		hatOffset.X = -2f;
		hatOffset.Y = -24f;
		horse_draw_layer += 1E-07f;
		int direction = 2;
		bool flipped = flip || (sprite.Value.CurrentAnimation != null && sprite.Value.CurrentAnimation[sprite.Value.currentAnimationIndex].flip);
		float scale = 1.3333334f;
		if (petType == "Cat")
		{
			switch (Sprite.CurrentFrame)
			{
			case 16:
				hatOffset.Y += 20f;
				direction = 2;
				break;
			case 0:
			case 2:
				hatOffset.Y += 28f;
				direction = 2;
				break;
			case 1:
			case 3:
				hatOffset.Y += 32f;
				direction = 2;
				break;
			case 4:
			case 6:
				direction = 1;
				hatOffset.X += 23f;
				hatOffset.Y += 20f;
				break;
			case 5:
			case 7:
				hatOffset.Y += 4f;
				direction = 1;
				hatOffset.X += 23f;
				hatOffset.Y += 20f;
				break;
			case 30:
			case 31:
				direction = ((!flipped) ? 1 : 3);
				hatOffset.X += ((!flipped) ? 1 : (-1)) * 25;
				hatOffset.Y += 32f;
				break;
			case 8:
			case 10:
				direction = 0;
				hatOffset.Y -= 4f;
				break;
			case 9:
			case 11:
				direction = 0;
				break;
			case 12:
			case 14:
				direction = 3;
				hatOffset.X -= 22f;
				hatOffset.Y += 20f;
				break;
			case 13:
			case 15:
				hatOffset.Y += 20f;
				hatOffset.Y += 4f;
				direction = 3;
				hatOffset.X -= 22f;
				break;
			case 21:
			case 23:
				hatOffset.Y += 16f;
				break;
			case 17:
			case 20:
			case 22:
				hatOffset.Y += 12f;
				break;
			case 18:
			case 19:
				hatOffset.Y += 8f;
				break;
			case 24:
				direction = ((!flipped) ? 1 : 3);
				hatOffset.X += ((!flipped) ? 1 : (-1)) * 29;
				hatOffset.Y += 28f;
				break;
			case 25:
				direction = ((!flipped) ? 1 : 3);
				hatOffset.X += ((!flipped) ? 1 : (-1)) * 29;
				hatOffset.Y += 36f;
				break;
			case 26:
				direction = ((!flipped) ? 1 : 3);
				hatOffset.X += ((!flipped) ? 1 : (-1)) * 29;
				hatOffset.Y += 40f;
				break;
			case 27:
				direction = ((!flipped) ? 1 : 3);
				hatOffset.X += ((!flipped) ? 1 : (-1)) * 29;
				hatOffset.Y += 44f;
				break;
			case 28:
			case 29:
				scale = 1.2f;
				hatOffset.Y += 46f;
				hatOffset.X -= ((!flipped) ? (-1) : 0) * 4;
				hatOffset.X += ((!flipped) ? 1 : (-1)) * 2;
				direction = (flipped ? 1 : 3);
				break;
			}
			if ((whichBreed == "3" || whichBreed == "4") && direction == 3)
			{
				hatOffset.X -= 4f;
			}
		}
		else if (petType == "Dog")
		{
			hatOffset.Y -= 20f;
			switch (Sprite.CurrentFrame)
			{
			case 16:
				hatOffset.Y += 20f;
				direction = 2;
				break;
			case 0:
			case 2:
				hatOffset.Y += 28f;
				direction = 2;
				break;
			case 1:
			case 3:
				hatOffset.Y += 32f;
				direction = 2;
				break;
			case 4:
			case 6:
				direction = 1;
				hatOffset.X += 26f;
				hatOffset.Y += 24f;
				break;
			case 5:
			case 7:
				direction = 1;
				hatOffset.X += 26f;
				hatOffset.Y += 28f;
				break;
			case 30:
			case 31:
				direction = ((!flipped) ? 1 : 3);
				hatOffset.X += 18f;
				hatOffset.Y += 8f;
				break;
			case 8:
			case 10:
				direction = 0;
				hatOffset.Y += 4f;
				break;
			case 9:
			case 11:
				direction = 0;
				hatOffset.Y += 8f;
				break;
			case 12:
			case 14:
				direction = 3;
				hatOffset.X -= 26f;
				hatOffset.Y += 24f;
				break;
			case 13:
			case 15:
				hatOffset.Y += 24f;
				hatOffset.Y += 4f;
				direction = 3;
				hatOffset.X -= 26f;
				break;
			case 23:
				direction = ((!flipped) ? 1 : 3);
				hatOffset.X += 18f;
				hatOffset.Y += 8f;
				break;
			case 20:
				direction = ((!flipped) ? 1 : 3);
				hatOffset.X += 26f;
				hatOffset.Y += ((whichBreed == "2") ? 16 : ((whichBreed == "1") ? 24 : 20));
				break;
			case 21:
				direction = ((!flipped) ? 1 : 3);
				hatOffset.X += 22f;
				hatOffset.Y += ((whichBreed == "2") ? 12 : ((whichBreed == "1") ? 20 : 16));
				break;
			case 22:
				direction = ((!flipped) ? 1 : 3);
				hatOffset.X += 18f;
				hatOffset.Y += ((whichBreed == "2") ? 8 : ((whichBreed == "1") ? 8 : 12));
				break;
			case 17:
				hatOffset.Y += 12f;
				break;
			case 18:
			case 19:
				hatOffset.Y += 8f;
				break;
			case 24:
			case 25:
				direction = ((!flipped) ? 1 : 3);
				hatOffset.X += 21 - (flipped ? 4 : 4) + 1;
				hatOffset.Y += 8f;
				break;
			case 26:
				direction = ((!flipped) ? 1 : 3);
				hatOffset.X += 18f;
				hatOffset.Y -= 8f;
				break;
			case 27:
				direction = 2;
				hatOffset.Y += 12 + ((whichBreed == "2") ? (-4) : 0);
				break;
			case 28:
			case 29:
				scale = 1.3333334f;
				hatOffset.Y += 48f;
				hatOffset.X += (flipped ? 6 : 5) * 4;
				hatOffset.X += 2f;
				direction = 2;
				break;
			case 32:
				direction = ((!flipped) ? 1 : 3);
				hatOffset.X += 26f;
				hatOffset.Y += ((whichBreed == "2") ? 12 : 16);
				break;
			case 33:
				direction = ((!flipped) ? 1 : 3);
				hatOffset.X += 26f;
				hatOffset.Y += ((whichBreed == "2") ? 16 : 20);
				break;
			case 34:
				direction = ((!flipped) ? 1 : 3);
				hatOffset.X += 26f;
				hatOffset.Y += ((whichBreed == "2") ? 20 : 24);
				break;
			}
			if (whichBreed == "2")
			{
				if (direction == 1)
				{
					hatOffset.X -= 4f;
				}
				hatOffset.Y += 8f;
			}
			else if (whichBreed == "3" && direction == 3 && Sprite.CurrentFrame > 16)
			{
				hatOffset.X += 4f;
			}
			if (flipped)
			{
				hatOffset.X *= -1f;
			}
		}
		hatOffset += shake;
		if (flipped)
		{
			hatOffset.X -= 4f;
		}
		hat.Value.draw(b, getLocalPosition(Game1.viewport) + hatOffset + new Vector2(30f, -42f), scale, 1f, horse_draw_layer, direction, useAnimalTexture: true);
	}

	public override void draw(SpriteBatch b)
	{
		int standingY = base.StandingPixel.Y;
		Vector2 shake = ((shakeTimer > 0 && !isSleepingOnFarmerBed) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero);
		b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(Sprite.SpriteWidth * 4 / 2, GetBoundingBox().Height / 2) + shake, Sprite.SourceRect, Color.White, rotation, new Vector2(Sprite.SpriteWidth / 2, (float)Sprite.SpriteHeight * 3f / 4f), Math.Max(0.2f, scale.Value) * 4f, (flip || (Sprite.CurrentAnimation != null && Sprite.CurrentAnimation[Sprite.currentAnimationIndex].flip)) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, isSleepingOnFarmerBed.Value ? (((float)standingY + 112f) / 10000f) : ((float)standingY / 10000f)));
		drawHat(b, shake);
		if (base.IsEmoting)
		{
			Vector2 localPosition = getLocalPosition(Game1.viewport);
			Point emoteOffset = GetPetData()?.EmoteOffset ?? Point.Zero;
			b.Draw(position: new Vector2(localPosition.X + 32f + (float)emoteOffset.X, localPosition.Y - 96f + (float)emoteOffset.Y), texture: Game1.emoteSpriteSheet, sourceRectangle: new Rectangle(base.CurrentEmoteIndex * 16 % Game1.emoteSpriteSheet.Width, base.CurrentEmoteIndex * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16), color: Color.White, rotation: 0f, origin: Vector2.Zero, scale: 4f, effects: SpriteEffects.None, layerDepth: (float)standingY / 10000f + 0.0001f);
		}
	}

	public virtual bool withinLocalPlayerThreshold(int threshold)
	{
		if (base.currentLocation != Game1.currentLocation)
		{
			return false;
		}
		Vector2 tileLocationOfMonster = base.Tile;
		Vector2 tileLocationOfPlayer = Game1.player.Tile;
		if (Math.Abs(tileLocationOfMonster.X - tileLocationOfPlayer.X) <= (float)threshold)
		{
			return Math.Abs(tileLocationOfMonster.Y - tileLocationOfPlayer.Y) <= (float)threshold;
		}
		return false;
	}

	public override bool withinPlayerThreshold(int threshold)
	{
		if (base.currentLocation != null && !base.currentLocation.farmers.Any())
		{
			return false;
		}
		Vector2 tileLocationOfMonster = base.Tile;
		foreach (Farmer farmer in base.currentLocation.farmers)
		{
			Vector2 tileLocationOfPlayer = farmer.Tile;
			if (Math.Abs(tileLocationOfMonster.X - tileLocationOfPlayer.X) <= (float)threshold && Math.Abs(tileLocationOfMonster.Y - tileLocationOfPlayer.Y) <= (float)threshold)
			{
				return true;
			}
		}
		return false;
	}

	public void hitGround(Farmer who)
	{
		if (Utility.isOnScreen(base.TilePoint, 128, base.currentLocation))
		{
			base.currentLocation.playTerrainSound(base.Tile, this, showTerrainDisturbAnimation: false);
		}
	}
}
