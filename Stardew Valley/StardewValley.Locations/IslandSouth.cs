using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.TerrainFeatures;
using StardewValley.WorldMaps;
using xTile.Dimensions;

namespace StardewValley.Locations;

public class IslandSouth : IslandLocation
{
	public class IslandActivityAssigments
	{
		public int activityTime;

		public List<NPC> visitors;

		public Dictionary<Character, string> currentAssignments;

		public Dictionary<Character, string> currentAnimationAssignments;

		public Random random;

		public Dictionary<string, string> animationDescriptions;

		public List<Point> shoreLoungePoints = new List<Point>(new Point[6]
		{
			new Point(9, 33),
			new Point(13, 33),
			new Point(17, 33),
			new Point(24, 33),
			new Point(28, 32),
			new Point(32, 31)
		});

		public List<Point> chairPoints = new List<Point>(new Point[2]
		{
			new Point(20, 24),
			new Point(30, 29)
		});

		public List<Point> umbrellaPoints = new List<Point>(new Point[3]
		{
			new Point(26, 26),
			new Point(28, 29),
			new Point(10, 27)
		});

		public List<Point> towelLoungePoints = new List<Point>(new Point[4]
		{
			new Point(14, 27),
			new Point(17, 28),
			new Point(20, 27),
			new Point(23, 28)
		});

		public List<Point> drinkPoints = new List<Point>(new Point[2]
		{
			new Point(12, 23),
			new Point(15, 23)
		});

		public List<Point> wanderPoints = new List<Point>(new Point[3]
		{
			new Point(7, 16),
			new Point(31, 24),
			new Point(18, 13)
		});

		public IslandActivityAssigments(int time, List<NPC> visitors, Random seeded_random, Dictionary<Character, string> last_activity_assignments)
		{
			activityTime = time;
			this.visitors = new List<NPC>(visitors);
			random = seeded_random;
			Utility.Shuffle(random, this.visitors);
			animationDescriptions = DataLoader.AnimationDescriptions(Game1.content);
			FindActivityForCharacters(last_activity_assignments);
		}

		public virtual void FindActivityForCharacters(Dictionary<Character, string> last_activity_assignments)
		{
			currentAssignments = new Dictionary<Character, string>();
			currentAnimationAssignments = new Dictionary<Character, string>();
			foreach (NPC character in visitors)
			{
				if (currentAssignments.ContainsKey(character))
				{
					continue;
				}
				string name = character.Name;
				if (!(name == "Gus"))
				{
					if (!(name == "Sam") || !TryAssignment(character, towelLoungePoints, "Resort_Towel", character.name.Value.ToLower() + "_beach_towel", animation_required: true, 0.5, last_activity_assignments))
					{
						continue;
					}
					foreach (NPC other_character in visitors)
					{
						if (!currentAssignments.ContainsKey(other_character) && animationDescriptions.ContainsKey(other_character.Name.ToLower() + "_beach_dance"))
						{
							string[] array = ArgUtility.SplitBySpace(currentAssignments[character]);
							int x = int.Parse(array[0]);
							int y = int.Parse(array[1]);
							currentAssignments.Remove(other_character);
							TryAssignment(other_character, new List<Point>(new Point[1]
							{
								new Point(x + 1, y + 1)
							}), "Resort_Dance", other_character.Name.ToLower() + "_beach_dance", animation_required: true, 1.0, last_activity_assignments);
							other_character.currentScheduleDelay = 0f;
							character.currentScheduleDelay = 0f;
							break;
						}
					}
					continue;
				}
				currentAssignments[character] = "14 21 2";
				foreach (NPC other_character in visitors)
				{
					if (!currentAssignments.ContainsKey(other_character) && other_character.Age != 2)
					{
						TryAssignment(other_character, drinkPoints, "Resort_Bar", other_character.name.Value.ToLower() + "_beach_drink", animation_required: false, 0.5, last_activity_assignments);
					}
				}
			}
			foreach (NPC character in visitors)
			{
				if (!currentAssignments.ContainsKey(character) && !TryAssignment(character, towelLoungePoints, "Resort_Towel", character.name.Value.ToLower() + "_beach_towel", animation_required: true, 0.5, last_activity_assignments) && !TryAssignment(character, wanderPoints, "Resort_Wander", "square_3_3", animation_required: false, 0.4, last_activity_assignments) && !TryAssignment(character, umbrellaPoints, "Resort_Umbrella", character.name.Value.ToLower() + "_beach_umbrella", animation_required: true, (character.Name == "Abigail") ? 0.5 : 0.1) && (character.Age != 0 || !TryAssignment(character, chairPoints, "Resort_Chair", "_beach_chair", animation_required: false, 0.4, last_activity_assignments)))
				{
					TryAssignment(character, shoreLoungePoints, "Resort_Shore", null, animation_required: false, 1.0, last_activity_assignments);
				}
			}
			last_activity_assignments.Clear();
			foreach (Character key in currentAnimationAssignments.Keys)
			{
				last_activity_assignments[key] = currentAnimationAssignments[key];
			}
		}

		public bool TryAssignment(Character character, List<Point> points, string dialogue_key, string animation_name = null, bool animation_required = false, double chance = 1.0, Dictionary<Character, string> last_activity_assignments = null)
		{
			if (last_activity_assignments != null && !string.IsNullOrEmpty(animation_name) && !animation_name.StartsWith("square_") && last_activity_assignments.TryGetValue(character, out var assignment) && assignment == animation_name)
			{
				return false;
			}
			if (points.Count > 0 && (random.NextDouble() < chance || chance >= 1.0))
			{
				Point current_point = random.ChooseFrom(points);
				if (!string.IsNullOrEmpty(animation_name) && !animation_name.StartsWith("square_") && !animationDescriptions.ContainsKey(animation_name))
				{
					if (animation_required)
					{
						return false;
					}
					animation_name = null;
				}
				string assignment_string = (string.IsNullOrEmpty(animation_name) ? (current_point.X + " " + current_point.Y + " 2") : (current_point.X + " " + current_point.Y + " " + animation_name));
				if (dialogue_key != null)
				{
					dialogue_key = GetRandomDialogueKey("Characters\\Dialogue\\" + character.Name + ":" + dialogue_key, random);
					if (dialogue_key == null)
					{
						dialogue_key = GetRandomDialogueKey("Characters\\Dialogue\\" + character.Name + ":Resort", random);
					}
					if (dialogue_key != null)
					{
						assignment_string = assignment_string + " \"" + dialogue_key + "\"";
					}
				}
				currentAssignments[character] = assignment_string;
				points.Remove(current_point);
				currentAnimationAssignments[character] = animation_name;
				return true;
			}
			return false;
		}

		public string GetRandomDialogueKey(string dialogue_key, Random random)
		{
			if (Game1.content.LoadStringReturnNullIfNotFound(dialogue_key) != null)
			{
				bool fail = false;
				int count = 0;
				while (!fail)
				{
					count++;
					if (Game1.content.LoadStringReturnNullIfNotFound(dialogue_key + "_" + (count + 1)) == null)
					{
						fail = true;
					}
				}
				int index = random.Next(count) + 1;
				if (index == 1)
				{
					return dialogue_key;
				}
				return dialogue_key + "_" + index;
			}
			return null;
		}

		public string GetScheduleStringForCharacter(NPC character)
		{
			if (currentAssignments.TryGetValue(character, out var assignment))
			{
				return "/" + activityTime + " IslandSouth " + assignment;
			}
			return "";
		}
	}

	[XmlIgnore]
	protected int _boatDirection;

	[XmlIgnore]
	public Texture2D boatTexture;

	[XmlIgnore]
	public Vector2 boatPosition;

	[XmlIgnore]
	protected int _boatOffset;

	[XmlIgnore]
	protected float _nextBubble;

	[XmlIgnore]
	protected float _nextSlosh;

	[XmlIgnore]
	protected float _nextSmoke;

	[XmlIgnore]
	public LightSource boatLight;

	[XmlIgnore]
	public LightSource boatStringLight;

	[XmlElement("shouldToggleResort")]
	public readonly NetBool shouldToggleResort = new NetBool(value: false);

	[XmlElement("resortOpenToday")]
	public readonly NetBool resortOpenToday = new NetBool(value: true);

	[XmlElement("resortRestored")]
	public readonly NetBool resortRestored = new NetBool
	{
		InterpolationWait = false
	};

	[XmlElement("westernTurtleMoved")]
	public readonly NetBool westernTurtleMoved = new NetBool();

	[XmlIgnore]
	protected bool _parrotBoyHiding;

	[XmlIgnore]
	protected bool _isFirstVisit;

	[XmlIgnore]
	protected bool _exitsBlocked;

	[XmlIgnore]
	protected bool _sawFlameSprite;

	[XmlIgnore]
	public NetEvent0 moveTurtleEvent = new NetEvent0();

	private Microsoft.Xna.Framework.Rectangle turtle1Spot = new Microsoft.Xna.Framework.Rectangle(1088, 0, 192, 192);

	private Microsoft.Xna.Framework.Rectangle turtle2Spot = new Microsoft.Xna.Framework.Rectangle(0, 640, 256, 256);

	public IslandSouth()
	{
	}

	public IslandSouth(string map, string name)
		: base(map, name)
	{
		largeTerrainFeatures.Add(new Bush(new Vector2(31f, 5f), 4, this));
		parrotUpgradePerches.Add(new ParrotUpgradePerch(this, new Point(17, 22), new Microsoft.Xna.Framework.Rectangle(12, 18, 14, 7), 20, delegate
		{
			Game1.addMailForTomorrow("Island_Resort", noLetter: true, sendToEveryone: true);
			resortRestored.Value = true;
		}, () => resortRestored.Value, "Resort", "Island_UpgradeHouse"));
		parrotUpgradePerches.Add(new ParrotUpgradePerch(this, new Point(5, 9), new Microsoft.Xna.Framework.Rectangle(1, 10, 3, 4), 10, delegate
		{
			Game1.addMailForTomorrow("Island_Turtle", noLetter: true, sendToEveryone: true);
			westernTurtleMoved.Value = true;
			moveTurtleEvent.Fire();
		}, () => westernTurtleMoved.Value, "Turtle", "Island_FirstParrot"));
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(resortRestored, "resortRestored").AddField(westernTurtleMoved, "westernTurtleMoved").AddField(shouldToggleResort, "shouldToggleResort")
			.AddField(resortOpenToday, "resortOpenToday")
			.AddField(moveTurtleEvent, "moveTurtleEvent");
		resortRestored.fieldChangeEvent += delegate(NetBool f, bool oldValue, bool newValue)
		{
			if (newValue && mapPath.Value != null)
			{
				ApplyResortRestore();
			}
		};
		moveTurtleEvent.onEvent += ApplyWesternTurtleMove;
	}

	public override void TransferDataFromSavedLocation(GameLocation l)
	{
		if (l is IslandSouth location)
		{
			resortRestored.Value = location.resortRestored.Value;
			westernTurtleMoved.Value = location.westernTurtleMoved.Value;
			shouldToggleResort.Value = location.shouldToggleResort.Value;
			resortOpenToday.Value = location.resortOpenToday.Value;
		}
		base.TransferDataFromSavedLocation(l);
	}

	public override void DayUpdate(int dayOfMonth)
	{
		if (shouldToggleResort.Value)
		{
			resortOpenToday.Value = !resortOpenToday.Value;
			shouldToggleResort.Value = false;
			ApplyResortRestore();
		}
		base.DayUpdate(dayOfMonth);
	}

	public void ApplyResortRestore()
	{
		if (map != null)
		{
			ApplyUnsafeMapOverride("Island_Resort", null, new Microsoft.Xna.Framework.Rectangle(9, 15, 26, 16));
		}
		removeTile(new Location(41, 28), "Buildings");
		removeTile(new Location(42, 28), "Buildings");
		removeTile(new Location(42, 29), "Buildings");
		removeTile(new Location(42, 30), "Front");
		removeTileProperty(42, 30, "Back", "Passable");
		if (resortRestored.Value)
		{
			if (resortOpenToday.Value)
			{
				removeTile(new Location(22, 21), "Buildings");
				removeTile(new Location(22, 22), "Buildings");
				removeTile(new Location(24, 21), "Buildings");
				removeTile(new Location(24, 22), "Buildings");
			}
			else
			{
				setMapTile(22, 21, 1405, "Buildings", null);
				setMapTile(22, 22, 1437, "Buildings", null);
				setMapTile(24, 21, 1405, "Buildings", null);
				setMapTile(24, 22, 1437, "Buildings", null);
			}
		}
	}

	public void ApplyWesternTurtleMove()
	{
		TemporaryAnimatedSprite t = getTemporarySpriteByID(789);
		if (t != null)
		{
			t.motion = new Vector2(-2f, 0f);
			t.yPeriodic = true;
			t.yPeriodicRange = 8f;
			t.yPeriodicLoopTime = 300f;
			t.shakeIntensity = 1f;
		}
		localSound("shadowDie");
	}

	private void parrotBoyLands(int extra)
	{
		TemporaryAnimatedSprite v = getTemporarySpriteByID(888);
		if (v != null)
		{
			v.sourceRect.X = 0;
			v.sourceRect.Y = 32;
			v.sourceRectStartingPos.X = 0f;
			v.sourceRectStartingPos.Y = 32f;
			v.motion = new Vector2(4f, 0f);
			v.acceleration = Vector2.Zero;
			v.id = 888;
			v.animationLength = 4;
			v.interval = 100f;
			v.totalNumberOfLoops = 10;
			v.drawAboveAlwaysFront = false;
			v.layerDepth = 0.1f;
			temporarySprites.Add(v);
		}
	}

	public override void UpdateWhenCurrentLocation(GameTime time)
	{
		base.UpdateWhenCurrentLocation(time);
		moveTurtleEvent.Poll();
		if (boatLight != null)
		{
			boatLight.position.Value = new Vector2(3f, 1f) * 64f + GetBoatPosition();
		}
		if (boatStringLight != null)
		{
			boatStringLight.position.Value = new Vector2(3f, 4f) * 64f + GetBoatPosition();
		}
		if (_parrotBoyHiding && Utility.isThereAFarmerWithinDistance(new Vector2(29f, 16f), 4, this) == Game1.player)
		{
			TemporaryAnimatedSprite v = getTemporarySpriteByID(777);
			if (v != null)
			{
				v.sourceRect.X = 0;
				v.sourceRectStartingPos.X = 0f;
				v.motion = new Vector2(3f, -10f);
				v.acceleration = new Vector2(0f, 0.4f);
				v.yStopCoordinate = 992;
				v.shakeIntensity = 2f;
				v.id = 888;
				v.reachedStopCoordinate = parrotBoyLands;
				localSound("parrot_squawk");
			}
		}
		if (!_exitsBlocked && !_sawFlameSprite && Utility.isThereAFarmerWithinDistance(new Vector2(18f, 11f), 5, this) == Game1.player)
		{
			Game1.addMailForTomorrow("Saw_Flame_Sprite_South", noLetter: true);
			TemporaryAnimatedSprite v = getTemporarySpriteByID(999);
			if (v != null)
			{
				v.yPeriodic = false;
				v.xPeriodic = false;
				v.sourceRect.Y = 0;
				v.sourceRectStartingPos.Y = 0f;
				v.motion = new Vector2(0f, -4f);
				v.acceleration = new Vector2(0f, -0.04f);
			}
			localSound("magma_sprite_spot");
			v = getTemporarySpriteByID(998);
			if (v != null)
			{
				v.yPeriodic = false;
				v.xPeriodic = false;
				v.motion = new Vector2(0f, -4f);
				v.acceleration = new Vector2(0f, -0.04f);
			}
			_sawFlameSprite = true;
		}
		if (!(currentEvent?.id == "-157039427"))
		{
			return;
		}
		if (_boatDirection != 0)
		{
			_boatOffset += _boatDirection;
			foreach (NPC actor in currentEvent.actors)
			{
				actor.shouldShadowBeOffset = true;
				actor.drawOffset.Y = _boatOffset;
			}
			foreach (Farmer farmerActor in currentEvent.farmerActors)
			{
				farmerActor.shouldShadowBeOffset = true;
				farmerActor.drawOffset.Y = _boatOffset;
			}
		}
		Vector2 position;
		TemporaryAnimatedSprite sprite;
		if ((float)_boatDirection != 0f)
		{
			if (_nextBubble > 0f)
			{
				_nextBubble -= (float)time.ElapsedGameTime.TotalSeconds;
			}
			else
			{
				Microsoft.Xna.Framework.Rectangle back_rectangle = new Microsoft.Xna.Framework.Rectangle(64, 256, 192, 64);
				back_rectangle.X += (int)GetBoatPosition().X;
				back_rectangle.Y += (int)GetBoatPosition().Y;
				position = Utility.getRandomPositionInThisRectangle(back_rectangle, Game1.random);
				sprite = new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), 50f, 9, 1, position, flicker: false, flipped: false, 0f, 0.025f, Color.White, 1f, 0f, 0f, 0f);
				sprite.acceleration = new Vector2(0f, -0.25f * (float)Math.Sign(_boatDirection));
				temporarySprites.Add(sprite);
				_nextBubble = 0.01f;
			}
			if (_nextSlosh > 0f)
			{
				_nextSlosh -= (float)time.ElapsedGameTime.TotalSeconds;
			}
			else
			{
				Game1.playSound("waterSlosh");
				_nextSlosh = 0.5f;
			}
		}
		if (_nextSmoke > 0f)
		{
			_nextSmoke -= (float)time.ElapsedGameTime.TotalSeconds;
			return;
		}
		position = new Vector2(2f, 2.5f) * 64f + GetBoatPosition();
		sprite = new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 1600, 64, 128), 200f, 9, 1, position, flicker: false, flipped: false, 1f, 0.025f, Color.White, 1f, 0.025f, 0f, 0f);
		sprite.acceleration = new Vector2(-0.25f, -0.15f);
		temporarySprites.Add(sprite);
		_nextSmoke = 0.2f;
	}

	public override void cleanupBeforePlayerExit()
	{
		boatLight = null;
		boatStringLight = null;
		base.cleanupBeforePlayerExit();
	}

	public override bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character)
	{
		if (_exitsBlocked && position.Intersects(turtle1Spot))
		{
			return true;
		}
		if (!westernTurtleMoved && position.Intersects(turtle2Spot))
		{
			return true;
		}
		return base.isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character);
	}

	public override bool isTilePlaceable(Vector2 tileLocation, bool itemIsPassable = false)
	{
		Point non_tile_position = Utility.Vector2ToPoint((tileLocation + new Vector2(0.5f, 0.5f)) * 64f);
		if (_exitsBlocked && turtle1Spot.Contains(non_tile_position))
		{
			return false;
		}
		if (!westernTurtleMoved && turtle2Spot.Contains(non_tile_position))
		{
			return false;
		}
		return base.isTilePlaceable(tileLocation, itemIsPassable);
	}

	public override void MakeMapModifications(bool force = false)
	{
		base.MakeMapModifications(force);
		if (resortRestored.Value)
		{
			ApplyResortRestore();
		}
	}

	protected override void resetLocalState()
	{
		_isFirstVisit = false;
		if (!Game1.player.hasOrWillReceiveMail("Visited_Island"))
		{
			WorldMapManager.ReloadData();
			Game1.addMailForTomorrow("Visited_Island", noLetter: true);
			_isFirstVisit = true;
		}
		Game1.getAchievement(40);
		if (Game1.player.hasOrWillReceiveMail("Saw_Flame_Sprite_South"))
		{
			_sawFlameSprite = true;
		}
		_exitsBlocked = !Game1.MasterPlayer.hasOrWillReceiveMail("Island_FirstParrot");
		boatLight = new LightSource(4, new Vector2(0f, 0f), 1f, LightSource.LightContext.None, 0L);
		boatStringLight = new LightSource(4, new Vector2(0f, 0f), 1f, LightSource.LightContext.None, 0L);
		Game1.currentLightSources.Add(boatLight);
		Game1.currentLightSources.Add(boatStringLight);
		base.resetLocalState();
		boatTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\WillysBoat");
		if (Game1.random.NextDouble() < 0.25 || _isFirstVisit)
		{
			addCritter(new CrabCritter(new Vector2(37f, 30f) * 64f));
		}
		if (_isFirstVisit)
		{
			addCritter(new CrabCritter(new Vector2(21f, 35f) * 64f));
			addCritter(new CrabCritter(new Vector2(21f, 36f) * 64f));
			addCritter(new CrabCritter(new Vector2(35f, 31f) * 64f));
			if (!Game1.MasterPlayer.hasOrWillReceiveMail("addedParrotBoy"))
			{
				_parrotBoyHiding = true;
				temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\ParrotBoy", new Microsoft.Xna.Framework.Rectangle(32, 128, 16, 32), new Vector2(29f, 15.5f) * 64f, flipped: false, 0f, Color.White)
				{
					id = 777,
					scale = 4f,
					totalNumberOfLoops = 99999,
					interval = 9999f,
					animationLength = 1,
					layerDepth = 1f,
					drawAboveAlwaysFront = true
				});
			}
		}
		if (_exitsBlocked)
		{
			temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(208, 94, 48, 53), new Vector2(17f, 0f) * 64f, flipped: false, 0f, Color.White)
			{
				id = 555,
				scale = 4f,
				totalNumberOfLoops = 99999,
				interval = 9999f,
				animationLength = 1,
				layerDepth = 0.001f
			});
		}
		else if (!_sawFlameSprite)
		{
			temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\Monsters\\Magma Sprite", new Microsoft.Xna.Framework.Rectangle(0, 16, 16, 16), new Vector2(18f, 11f) * 64f, flipped: false, 0f, Color.White)
			{
				id = 999,
				scale = 4f,
				totalNumberOfLoops = 99999,
				interval = 70f,
				light = true,
				lightRadius = 1f,
				animationLength = 7,
				layerDepth = 1f,
				yPeriodic = true,
				yPeriodicRange = 12f,
				yPeriodicLoopTime = 1000f,
				xPeriodic = true,
				xPeriodicRange = 16f,
				xPeriodicLoopTime = 1800f
			});
			temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\shadow", new Microsoft.Xna.Framework.Rectangle(0, 0, 12, 7), new Vector2(18.2f, 12.4f) * 64f, flipped: false, 0f, Color.White)
			{
				id = 998,
				scale = 4f,
				totalNumberOfLoops = 99999,
				interval = 1000f,
				animationLength = 1,
				layerDepth = 0.001f,
				yPeriodic = true,
				yPeriodicRange = 1f,
				yPeriodicLoopTime = 1000f,
				xPeriodic = true,
				xPeriodicRange = 16f,
				xPeriodicLoopTime = 1800f
			});
		}
		if (!westernTurtleMoved)
		{
			temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(152, 101, 56, 40), new Vector2(0.5f, 10f) * 64f, flipped: false, 0f, Color.White)
			{
				id = 789,
				scale = 4f,
				totalNumberOfLoops = 99999,
				interval = 9999f,
				animationLength = 1,
				layerDepth = 0.001f
			});
		}
		if (AreMoonlightJelliesOut())
		{
			addMoonlightJellies(50, Utility.CreateRandom(Game1.stats.DaysPlayed, Game1.uniqueIDForThisGame, -24917.0), new Microsoft.Xna.Framework.Rectangle(0, 0, 0, 0));
		}
		ResetBoat();
	}

	public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
	{
		if (tileLocation.X == 14 && tileLocation.Y == 22)
		{
			Microsoft.Xna.Framework.Rectangle shopArea = new Microsoft.Xna.Framework.Rectangle(14, 21, 1, 1);
			if (Utility.TryOpenShopMenu("ResortBar", this, shopArea))
			{
				return true;
			}
		}
		return base.checkAction(tileLocation, viewport, who);
	}

	/// <summary>Get whether an NPC can visit the island resort today.</summary>
	/// <param name="npc">The NPC to check.</param>
	public static bool CanVisitIslandToday(NPC npc)
	{
		if (!npc.IsVillager || !npc.CanSocialize || npc.daysUntilNotInvisible > 0 || npc.IsInvisible)
		{
			return false;
		}
		if (!GameStateQuery.CheckConditions(npc.GetData()?.CanVisitIsland, npc.currentLocation))
		{
			return false;
		}
		if (npc.currentLocation?.NameOrUniqueName == "Farm")
		{
			return false;
		}
		if (Utility.IsHospitalVisitDay(npc.Name))
		{
			return false;
		}
		return true;
	}

	public override bool answerDialogueAction(string questionAndAnswer, string[] questionParams)
	{
		if (questionAndAnswer == null)
		{
			return false;
		}
		if (!(questionAndAnswer == "LeaveIsland_Yes"))
		{
			if (questionAndAnswer == "ToggleResort_Yes")
			{
				shouldToggleResort.Value = !shouldToggleResort.Value;
				bool open = resortOpenToday.Value;
				if (shouldToggleResort.Value)
				{
					open = !open;
				}
				if (open)
				{
					Game1.drawDialogueNoTyping(Game1.content.LoadString("Strings\\Locations:IslandSouth_ResortWillOpenSign"));
				}
				else
				{
					Game1.drawDialogueNoTyping(Game1.content.LoadString("Strings\\Locations:IslandSouth_ResortWillCloseSign"));
				}
				return true;
			}
			return base.answerDialogueAction(questionAndAnswer, questionParams);
		}
		Depart();
		return true;
	}

	/// <inheritdoc />
	public override bool performAction(string[] action, Farmer who, Location tileLocation)
	{
		if (ArgUtility.Get(action, 0) == "ResortSign")
		{
			string key = ((!resortOpenToday.Value) ? (shouldToggleResort.Value ? "Strings\\Locations:IslandSouth_ResortClosedWillOpenSign" : "Strings\\Locations:IslandSouth_ResortClosedSign") : (shouldToggleResort.Value ? "Strings\\Locations:IslandSouth_ResortOpenWillCloseSign" : "Strings\\Locations:IslandSouth_ResortOpenSign"));
			createQuestionDialogue(Game1.content.LoadString(key), createYesNoResponses(), "ToggleResort");
			return true;
		}
		return base.performAction(action, who, tileLocation);
	}

	/// <inheritdoc />
	public override void performTouchAction(string[] action, Vector2 playerStandingPosition)
	{
		if (!IgnoreTouchActions())
		{
			if (ArgUtility.Get(action, 0) == "LeaveIsland")
			{
				Response[] returnOptions = new Response[2]
				{
					new Response("Yes", Game1.content.LoadString("Strings\\Locations:Desert_Return_Yes")),
					new Response("Not", Game1.content.LoadString("Strings\\Locations:Desert_Return_No"))
				};
				createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Desert_Return_Question"), returnOptions, "LeaveIsland");
			}
			else
			{
				base.performTouchAction(action, playerStandingPosition);
			}
		}
	}

	public void Depart()
	{
		Game1.globalFadeToBlack(delegate
		{
			currentEvent = new Event(Game1.content.LoadString("Data\\Events\\IslandSouth:IslandDepart"), "Data\\Events\\IslandSouth", "-157039427", Game1.player);
			Game1.eventUp = true;
		});
	}

	public static Point GetDressingRoomPoint(NPC character)
	{
		if (character.Gender == Gender.Female)
		{
			return new Point(22, 19);
		}
		return new Point(24, 19);
	}

	public override bool HasLocationOverrideDialogue(NPC character)
	{
		if (Game1.player.friendshipData.TryGetValue(character.Name, out var friendship) && friendship.IsDivorced())
		{
			return false;
		}
		return character.islandScheduleName.Value != null;
	}

	public override string GetLocationOverrideDialogue(NPC character)
	{
		if (Game1.timeOfDay < 1200 || (!character.shouldWearIslandAttire.Value && Game1.timeOfDay < 1730 && HasIslandAttire(character)))
		{
			string dialogue_key = "Characters\\Dialogue\\" + character.Name + ":Resort_Entering";
			if (Game1.content.LoadStringReturnNullIfNotFound(dialogue_key) != null)
			{
				return dialogue_key;
			}
		}
		if (Game1.timeOfDay >= 1800)
		{
			string dialogue_key = "Characters\\Dialogue\\" + character.Name + ":Resort_Leaving";
			if (Game1.content.LoadStringReturnNullIfNotFound(dialogue_key) != null)
			{
				return dialogue_key;
			}
		}
		return "Characters\\Dialogue\\" + character.Name + ":Resort";
	}

	public static bool HasIslandAttire(NPC character)
	{
		try
		{
			Game1.temporaryContent.Load<Texture2D>("Characters\\" + NPC.getTextureNameForCharacter(character.name.Value) + "_Beach");
			if (character?.Name == "Lewis")
			{
				foreach (Farmer farmer in Game1.getAllFarmers())
				{
					if (farmer?.activeDialogueEvents != null && farmer.activeDialogueEvents.ContainsKey("lucky_pants_lewis"))
					{
						return true;
					}
				}
				return false;
			}
			return true;
		}
		catch (Exception)
		{
		}
		return false;
	}

	public static void SetupIslandSchedules()
	{
		Game1.netWorldState.Value.IslandVisitors.Clear();
		if (Utility.isFestivalDay() || Utility.IsPassiveFestivalDay() || !(Game1.getLocationFromName("IslandSouth") is IslandSouth island) || !island.resortRestored.Value || island.IsRainingHere() || !island.resortOpenToday.Value)
		{
			return;
		}
		Random seeded_random = Utility.CreateRandom((double)Game1.uniqueIDForThisGame * 1.21, (double)Game1.stats.DaysPlayed * 2.5);
		List<NPC> valid_visitors = new List<NPC>();
		Utility.ForEachVillager(delegate(NPC npc)
		{
			if (CanVisitIslandToday(npc))
			{
				valid_visitors.Add(npc);
			}
			return true;
		});
		List<NPC> visitors = new List<NPC>();
		if (seeded_random.NextDouble() < 0.4)
		{
			for (int i = 0; i < 5; i++)
			{
				NPC visitor = seeded_random.ChooseFrom(valid_visitors);
				if (visitor != null && (int)visitor.age != 2)
				{
					valid_visitors.Remove(visitor);
					visitors.Add(visitor);
					visitor.scheduleDelaySeconds = Math.Min((float)i * 0.6f, (float)Game1.realMilliSecondsPerGameTenMinutes / 1000f);
				}
			}
		}
		else
		{
			List<List<string>> potentialGroups = new List<List<string>>();
			potentialGroups.Add(new List<string> { "Sebastian", "Sam", "Abigail" });
			potentialGroups.Add(new List<string> { "Jodi", "Kent", "Vincent", "Sam" });
			potentialGroups.Add(new List<string> { "Jodi", "Vincent", "Sam" });
			potentialGroups.Add(new List<string> { "Pierre", "Caroline", "Abigail" });
			potentialGroups.Add(new List<string> { "Robin", "Demetrius", "Maru", "Sebastian" });
			potentialGroups.Add(new List<string> { "Lewis", "Marnie" });
			potentialGroups.Add(new List<string> { "Marnie", "Shane", "Jas" });
			potentialGroups.Add(new List<string> { "Penny", "Jas", "Vincent" });
			potentialGroups.Add(new List<string> { "Pam", "Penny" });
			potentialGroups.Add(new List<string> { "Caroline", "Marnie", "Robin", "Jodi" });
			potentialGroups.Add(new List<string> { "Haley", "Penny", "Leah", "Emily", "Maru", "Abigail" });
			potentialGroups.Add(new List<string> { "Alex", "Sam", "Sebastian", "Elliott", "Shane", "Harvey" });
			List<string> group = potentialGroups[seeded_random.Next(potentialGroups.Count)];
			bool failed = false;
			foreach (string s in group)
			{
				if (!valid_visitors.Contains(Game1.getCharacterFromName(s)))
				{
					failed = true;
					break;
				}
			}
			if (!failed)
			{
				int i = 0;
				foreach (string item in group)
				{
					NPC visitor = Game1.getCharacterFromName(item);
					valid_visitors.Remove(visitor);
					visitors.Add(visitor);
					visitor.scheduleDelaySeconds = Math.Min((float)i * 0.6f, (float)Game1.realMilliSecondsPerGameTenMinutes / 1000f);
					i++;
				}
			}
			for (int i = 0; i < 5 - visitors.Count; i++)
			{
				NPC visitor = seeded_random.ChooseFrom(valid_visitors);
				if (visitor != null && (int)visitor.age != 2)
				{
					valid_visitors.Remove(visitor);
					visitors.Add(visitor);
					visitor.scheduleDelaySeconds = Math.Min((float)i * 0.6f, (float)Game1.realMilliSecondsPerGameTenMinutes / 1000f);
				}
			}
		}
		List<IslandActivityAssigments> activities = new List<IslandActivityAssigments>();
		Dictionary<Character, string> last_activity_assignments = new Dictionary<Character, string>();
		activities.Add(new IslandActivityAssigments(1200, visitors, seeded_random, last_activity_assignments));
		activities.Add(new IslandActivityAssigments(1400, visitors, seeded_random, last_activity_assignments));
		activities.Add(new IslandActivityAssigments(1600, visitors, seeded_random, last_activity_assignments));
		foreach (NPC visitor in visitors)
		{
			StringBuilder schedule = new StringBuilder("");
			bool should_dress = HasIslandAttire(visitor);
			bool had_first_activity = false;
			if (should_dress)
			{
				Point dressing_room = GetDressingRoomPoint(visitor);
				schedule.Append("/a1150 IslandSouth " + dressing_room.X + " " + dressing_room.Y + " change_beach");
				had_first_activity = true;
			}
			foreach (IslandActivityAssigments item2 in activities)
			{
				string current_string = item2.GetScheduleStringForCharacter(visitor);
				if (current_string != "")
				{
					if (!had_first_activity)
					{
						current_string = "/a" + current_string.Substring(1);
						had_first_activity = true;
					}
					schedule.Append(current_string);
				}
			}
			if (should_dress)
			{
				Point dressing_room = GetDressingRoomPoint(visitor);
				schedule.Append("/a1730 IslandSouth " + dressing_room.X + " " + dressing_room.Y + " change_normal");
			}
			if (visitor.Name == "Gus")
			{
				schedule.Append("/1800 Saloon 10 18 2/2430 bed");
			}
			else
			{
				schedule.Append("/1800 bed");
			}
			schedule.Remove(0, 1);
			if (visitor.TryLoadSchedule("island", schedule.ToString()))
			{
				visitor.islandScheduleName.Value = "island";
				Game1.netWorldState.Value.IslandVisitors.Add(visitor.Name);
			}
			visitor.performSpecialScheduleChanges();
		}
	}

	public virtual void ResetBoat()
	{
		boatPosition = new Vector2(14f, 37f) * 64f;
		_boatOffset = 0;
		_boatDirection = 0;
		_nextBubble = 0f;
		_nextSmoke = 0f;
		_nextSlosh = 0f;
	}

	public Vector2 GetBoatPosition()
	{
		return boatPosition + new Vector2(0f, _boatOffset);
	}

	public override void draw(SpriteBatch b)
	{
		base.draw(b);
		Vector2 boat_position = GetBoatPosition();
		b.Draw(boatTexture, Game1.GlobalToLocal(boat_position), new Microsoft.Xna.Framework.Rectangle(192, 0, 96, 208), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (boatPosition.Y + 320f) / 10000f);
		b.Draw(boatTexture, Game1.GlobalToLocal(boat_position), new Microsoft.Xna.Framework.Rectangle(288, 0, 96, 208), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (boatPosition.Y + 616f) / 10000f);
		if (currentEvent == null || currentEvent.id != "-157039427")
		{
			b.Draw(boatTexture, Game1.GlobalToLocal(new Vector2(1184f, 2752f)), new Microsoft.Xna.Framework.Rectangle(192, 208, 32, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.272f);
		}
	}

	public override bool RunLocationSpecificEventCommand(Event current_event, string command_string, bool first_run, params string[] args)
	{
		if (!(command_string == "boat_reset"))
		{
			if (command_string == "boat_depart")
			{
				_boatDirection = 1;
				if (_boatOffset >= 100)
				{
					return true;
				}
				return false;
			}
			return false;
		}
		ResetBoat();
		return true;
	}
}
