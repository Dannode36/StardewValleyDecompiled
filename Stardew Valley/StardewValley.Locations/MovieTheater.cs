using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Events;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.Movies;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.Network;
using StardewValley.Pathfinding;
using StardewValley.TokenizableStrings;
using xTile.Dimensions;
using xTile.Layers;

namespace StardewValley.Locations;

/// <summary>The movie theater location.</summary>
/// <remarks>See also <see cref="T:StardewValley.Events.MovieTheaterScreeningEvent" />.</remarks>
public class MovieTheater : GameLocation
{
	public enum MovieStates
	{
		Preshow,
		Show,
		PostShow
	}

	protected bool _startedMovie;

	protected static bool _isJojaTheater = false;

	protected static List<MovieData> _movieData;

	protected static Dictionary<string, MovieData> _movieDataById;

	protected static List<MovieCharacterReaction> _genericReactions;

	protected static List<ConcessionTaste> _concessionTastes;

	protected readonly NetStringDictionary<int, NetInt> _spawnedMoviePatrons = new NetStringDictionary<int, NetInt>();

	protected readonly NetStringDictionary<string, NetString> _purchasedConcessions = new NetStringDictionary<string, NetString>();

	protected readonly NetStringDictionary<int, NetInt> _playerInvitedPatrons = new NetStringDictionary<int, NetInt>();

	protected readonly NetStringDictionary<bool, NetBool> _characterGroupLookup = new NetStringDictionary<bool, NetBool>();

	protected Dictionary<int, List<Point>> _hangoutPoints;

	protected Dictionary<int, List<Point>> _availableHangoutPoints;

	protected int _maxHangoutGroups;

	protected int _movieStartTime = -1;

	[XmlElement("dayFirstEntered")]
	public readonly NetInt dayFirstEntered = new NetInt(-1);

	protected static Dictionary<string, MovieConcession> _concessions;

	public const int LOVE_MOVIE_FRIENDSHIP = 200;

	public const int LIKE_MOVIE_FRIENDSHIP = 100;

	public const int DISLIKE_MOVIE_FRIENDSHIP = 0;

	public const int LOVE_CONCESSION_FRIENDSHIP = 50;

	public const int LIKE_CONCESSION_FRIENDSHIP = 25;

	public const int DISLIKE_CONCESSION_FRIENDSHIP = 0;

	public const int OPEN_TIME = 900;

	public const int CLOSE_TIME = 2100;

	[XmlIgnore]
	protected Dictionary<string, KeyValuePair<Point, int>> _destinationPositions = new Dictionary<string, KeyValuePair<Point, int>>();

	[XmlIgnore]
	public PerchingBirds birds;

	/// <summary>If set, the movie ID to watch when a movie is requested, instead of the movie for the current date.</summary>
	[XmlIgnore]
	public static string forceMovieId;

	protected int _exitX;

	protected int _exitY;

	private NetEvent1<MovieViewerLockEvent> movieViewerLockEvent = new NetEvent1<MovieViewerLockEvent>();

	private NetEvent1<StartMovieEvent> startMovieEvent = new NetEvent1<StartMovieEvent>();

	private NetEvent1Field<long, NetLong> requestStartMovieEvent = new NetEvent1Field<long, NetLong>();

	private NetEvent1Field<long, NetLong> endMovieEvent = new NetEvent1Field<long, NetLong>();

	protected List<Farmer> _viewingFarmers = new List<Farmer>();

	protected List<List<Character>> _viewingGroups = new List<List<Character>>();

	protected List<List<Character>> _playerGroups = new List<List<Character>>();

	protected List<List<Character>> _npcGroups = new List<List<Character>>();

	protected static bool _hasRequestedMovieStart = false;

	protected static int _playerHangoutGroup = -1;

	protected int _farmerCount;

	protected readonly NetInt currentState = new NetInt();

	protected readonly NetInt showingId = new NetInt();

	public static string[][][][] possibleNPCGroups = new string[7][][][]
	{
		new string[3][][]
		{
			new string[1][] { new string[1] { "Lewis" } },
			new string[3][]
			{
				new string[3] { "Jas", "Vincent", "Marnie" },
				new string[3] { "Abigail", "Sebastian", "Sam" },
				new string[2] { "Penny", "Maru" }
			},
			new string[1][] { new string[2] { "Lewis", "Marnie" } }
		},
		new string[3][][]
		{
			new string[3][]
			{
				new string[1] { "Clint" },
				new string[2] { "Demetrius", "Robin" },
				new string[1] { "Lewis" }
			},
			new string[2][]
			{
				new string[2] { "Caroline", "Jodi" },
				new string[3] { "Abigail", "Sebastian", "Sam" }
			},
			new string[2][]
			{
				new string[1] { "Lewis" },
				new string[3] { "Abigail", "Sebastian", "Sam" }
			}
		},
		new string[3][][]
		{
			new string[2][]
			{
				new string[2] { "Evelyn", "George" },
				new string[1] { "Lewis" }
			},
			new string[2][]
			{
				new string[2] { "Penny", "Pam" },
				new string[3] { "Abigail", "Sebastian", "Sam" }
			},
			new string[2][]
			{
				new string[2] { "Sandy", "Emily" },
				new string[1] { "Elliot" }
			}
		},
		new string[3][][]
		{
			new string[3][]
			{
				new string[2] { "Penny", "Pam" },
				new string[3] { "Abigail", "Sebastian", "Sam" },
				new string[1] { "Lewis" }
			},
			new string[2][]
			{
				new string[3] { "Alex", "Haley", "Emily" },
				new string[3] { "Abigail", "Sebastian", "Sam" }
			},
			new string[2][]
			{
				new string[2] { "Pierre", "Caroline" },
				new string[3] { "Shane", "Jas", "Marnie" }
			}
		},
		new string[3][][]
		{
			null,
			new string[3][]
			{
				new string[2] { "Haley", "Emily" },
				new string[3] { "Abigail", "Sebastian", "Sam" },
				new string[1] { "Lewis" }
			},
			new string[2][]
			{
				new string[2] { "Penny", "Pam" },
				new string[3] { "Abigail", "Sebastian", "Sam" }
			}
		},
		new string[3][][]
		{
			new string[1][] { new string[1] { "Lewis" } },
			new string[2][]
			{
				new string[2] { "Penny", "Pam" },
				new string[3] { "Abigail", "Sebastian", "Sam" }
			},
			new string[2][]
			{
				new string[3] { "Harvey", "Maru", "Penny" },
				new string[1] { "Leah" }
			}
		},
		new string[3][][]
		{
			new string[3][]
			{
				new string[2] { "Penny", "Pam" },
				new string[3] { "George", "Evelyn", "Alex" },
				new string[1] { "Lewis" }
			},
			new string[2][]
			{
				new string[2] { "Gus", "Willy" },
				new string[2] { "Maru", "Sebastian" }
			},
			new string[2][]
			{
				new string[2] { "Penny", "Pam" },
				new string[2] { "Sandy", "Emily" }
			}
		}
	};

	protected int CurrentState
	{
		get
		{
			return currentState.Value;
		}
		set
		{
			if (Game1.IsMasterGame)
			{
				currentState.Value = value;
			}
			else
			{
				Game1.log.Warn("Tried to set MovieTheater::CurrentState as a farmhand.");
			}
		}
	}

	protected int ShowingId
	{
		get
		{
			return showingId.Value;
		}
		set
		{
			if (Game1.IsMasterGame)
			{
				showingId.Value = value;
			}
			else
			{
				Game1.log.Warn("Tried to set MovieTheater::ShowingId as a farmhand.");
			}
		}
	}

	public MovieTheater()
	{
	}

	public static void AddMoviePoster(GameLocation location, float x, float y, bool isUpcoming = false)
	{
		MovieData data = (isUpcoming ? GetUpcomingMovie() : GetMovieToday());
		if (data != null)
		{
			Microsoft.Xna.Framework.Rectangle sourceRect = GetSourceRectForPoster(data.SheetIndex);
			location.temporarySprites.Add(new TemporaryAnimatedSprite
			{
				texture = Game1.temporaryContent.Load<Texture2D>(data.Texture ?? "LooseSprites\\Movies"),
				sourceRect = sourceRect,
				sourceRectStartingPos = new Vector2(sourceRect.X, sourceRect.Y),
				animationLength = 1,
				totalNumberOfLoops = 9999,
				interval = 9999f,
				scale = 4f,
				position = new Vector2(x, y),
				layerDepth = 0.01f
			});
		}
	}

	public MovieTheater(string map, string name)
		: base(map, name)
	{
		CurrentState = 0;
		GetMovieData();
		_InitializeMap();
		GetMovieReactions();
	}

	public static List<MovieCharacterReaction> GetMovieReactions()
	{
		if (_genericReactions == null)
		{
			_genericReactions = DataLoader.MoviesReactions(Game1.content);
		}
		return _genericReactions;
	}

	public static string GetConcessionTasteForCharacter(Character character, MovieConcession concession)
	{
		if (_concessionTastes == null)
		{
			_concessionTastes = DataLoader.ConcessionTastes(Game1.content);
		}
		ConcessionTaste universal_taste = null;
		foreach (ConcessionTaste taste in _concessionTastes)
		{
			if (taste.Name == "*")
			{
				universal_taste = taste;
				break;
			}
		}
		foreach (ConcessionTaste taste in _concessionTastes)
		{
			if (!(taste.Name == character.Name))
			{
				continue;
			}
			if (taste.LovedTags.Contains(concession.Name))
			{
				return "love";
			}
			if (taste.LikedTags.Contains(concession.Name))
			{
				return "like";
			}
			if (taste.DislikedTags.Contains(concession.Name))
			{
				return "dislike";
			}
			if (universal_taste != null)
			{
				if (universal_taste.LovedTags.Contains(concession.Name))
				{
					return "love";
				}
				if (universal_taste.LikedTags.Contains(concession.Name))
				{
					return "like";
				}
				if (universal_taste.DislikedTags.Contains(concession.Name))
				{
					return "dislike";
				}
			}
			if (concession.Tags == null)
			{
				break;
			}
			foreach (string tag in concession.Tags)
			{
				if (taste.LovedTags.Contains(tag))
				{
					return "love";
				}
				if (taste.LikedTags.Contains(tag))
				{
					return "like";
				}
				if (taste.DislikedTags.Contains(tag))
				{
					return "dislike";
				}
				if (universal_taste != null)
				{
					if (universal_taste.LovedTags.Contains(tag))
					{
						return "love";
					}
					if (universal_taste.LikedTags.Contains(tag))
					{
						return "like";
					}
					if (universal_taste.DislikedTags.Contains(tag))
					{
						return "dislike";
					}
				}
			}
			break;
		}
		return "like";
	}

	public static IEnumerable<string> GetPatronNames()
	{
		return (Game1.getLocationFromName("MovieTheater") as MovieTheater)?._spawnedMoviePatrons?.Keys;
	}

	protected void _InitializeMap()
	{
		_hangoutPoints = new Dictionary<int, List<Point>>();
		_maxHangoutGroups = 0;
		Layer paths_layer = map.GetLayer("Paths");
		if (paths_layer != null)
		{
			for (int x = 0; x < paths_layer.LayerWidth; x++)
			{
				for (int y = 0; y < paths_layer.LayerHeight; y++)
				{
					if (paths_layer.Tiles[x, y] != null && paths_layer.GetTileIndexAt(x, y) == 7 && paths_layer.Tiles[x, y].Properties.TryGetValue("group", out var property) && int.TryParse(property, out var hangout_group))
					{
						if (!_hangoutPoints.TryGetValue(hangout_group, out var points))
						{
							points = (_hangoutPoints[hangout_group] = new List<Point>());
						}
						points.Add(new Point(x, y));
						_maxHangoutGroups = Math.Max(_maxHangoutGroups, hangout_group);
					}
				}
			}
		}
		ResetTheater();
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(_spawnedMoviePatrons, "_spawnedMoviePatrons").AddField(_purchasedConcessions, "_purchasedConcessions").AddField(currentState, "currentState")
			.AddField(showingId, "showingId")
			.AddField(movieViewerLockEvent, "movieViewerLockEvent")
			.AddField(requestStartMovieEvent, "requestStartMovieEvent")
			.AddField(startMovieEvent, "startMovieEvent")
			.AddField(endMovieEvent, "endMovieEvent")
			.AddField(_playerInvitedPatrons, "_playerInvitedPatrons")
			.AddField(_characterGroupLookup, "_characterGroupLookup")
			.AddField(dayFirstEntered, "dayFirstEntered");
		movieViewerLockEvent.onEvent += OnMovieViewerLockEvent;
		requestStartMovieEvent.onEvent += OnRequestStartMovieEvent;
		startMovieEvent.onEvent += OnStartMovieEvent;
	}

	public void OnStartMovieEvent(StartMovieEvent e)
	{
		if (e.uid == Game1.player.UniqueMultiplayerID)
		{
			if (Game1.activeClickableMenu is ReadyCheckDialog readyCheckDialog)
			{
				readyCheckDialog.closeDialog(Game1.player);
			}
			MovieTheaterScreeningEvent event_generator = new MovieTheaterScreeningEvent();
			Event viewing_event = event_generator.getMovieEvent(GetMovieToday().Id, e.playerGroups, e.npcGroups, GetConcessionsDictionary());
			Rumble.rumble(0.15f, 200f);
			Game1.player.completelyStopAnimatingOrDoingAction();
			playSound("doorClose", Game1.player.Tile);
			Game1.globalFadeToBlack(delegate
			{
				Game1.changeMusicTrack("none");
				startEvent(viewing_event);
			});
		}
	}

	public void OnRequestStartMovieEvent(long uid)
	{
		if (!Game1.IsMasterGame)
		{
			return;
		}
		if (CurrentState == 0)
		{
			if (Game1.player.team.movieMutex.IsLocked())
			{
				Game1.player.team.movieMutex.ReleaseLock();
			}
			Game1.player.team.movieMutex.RequestLock();
			_playerGroups = new List<List<Character>>();
			_npcGroups = new List<List<Character>>();
			List<Character> patrons = new List<Character>();
			foreach (string patronName in GetPatronNames())
			{
				Character character = Game1.getCharacterFromName(patronName);
				patrons.Add(character);
			}
			foreach (Farmer farmer in _viewingFarmers)
			{
				List<Character> farmer_group = new List<Character>();
				farmer_group.Add(farmer);
				for (int i = 0; i < Game1.player.team.movieInvitations.Count; i++)
				{
					MovieInvitation invite = Game1.player.team.movieInvitations[i];
					if (invite.farmer == farmer && GetFirstInvitedPlayer(invite.invitedNPC) == farmer && patrons.Contains(invite.invitedNPC))
					{
						patrons.Remove(invite.invitedNPC);
						farmer_group.Add(invite.invitedNPC);
					}
				}
				_playerGroups.Add(farmer_group);
			}
			foreach (List<Character> playerGroup in _playerGroups)
			{
				foreach (Character item in playerGroup)
				{
					if (item is NPC npc)
					{
						npc.lastSeenMovieWeek.Set(Game1.Date.TotalWeeks);
					}
				}
			}
			_npcGroups.Add(new List<Character>(patrons));
			_PopulateNPCOnlyGroups(_playerGroups, _npcGroups);
			_viewingGroups = new List<List<Character>>();
			List<Character> player_invited_npcs = new List<Character>();
			foreach (List<Character> playerGroup2 in _playerGroups)
			{
				foreach (Character character in playerGroup2)
				{
					player_invited_npcs.Add(character);
				}
			}
			_viewingGroups.Add(player_invited_npcs);
			foreach (List<Character> characters in _npcGroups)
			{
				_viewingGroups.Add(new List<Character>(characters));
			}
			CurrentState = 1;
		}
		startMovieEvent.Fire(new StartMovieEvent(uid, _playerGroups, _npcGroups));
	}

	public void OnMovieViewerLockEvent(MovieViewerLockEvent e)
	{
		_viewingFarmers = new List<Farmer>();
		_movieStartTime = e.movieStartTime;
		foreach (long uid in e.uids)
		{
			Farmer farmer = Game1.getFarmer(uid);
			if (farmer != null)
			{
				_viewingFarmers.Add(farmer);
			}
		}
		if (_viewingFarmers.Count > 0 && Game1.IsMultiplayer)
		{
			Game1.showGlobalMessage(Game1.content.LoadString("Strings\\UI:MovieStartRequest"));
		}
		if (Game1.player.team.movieMutex.IsLockHeld())
		{
			_ShowMovieStartReady();
		}
	}

	public void _ShowMovieStartReady()
	{
		if (!Game1.IsMultiplayer)
		{
			requestStartMovieEvent.Fire(Game1.player.UniqueMultiplayerID);
			return;
		}
		string readyCheckName = $"start_movie_{ShowingId}";
		Game1.netReady.SetLocalRequiredFarmers(readyCheckName, _viewingFarmers);
		Game1.netReady.SetLocalReady(readyCheckName, ready: true);
		Game1.dialogueUp = false;
		_hasRequestedMovieStart = true;
		Game1.activeClickableMenu = new ReadyCheckDialog(readyCheckName, allowCancel: true, delegate(Farmer farmer)
		{
			if (_hasRequestedMovieStart)
			{
				_hasRequestedMovieStart = false;
				requestStartMovieEvent.Fire(farmer.UniqueMultiplayerID);
			}
		}, delegate(Farmer farmer)
		{
			if (Game1.activeClickableMenu is ReadyCheckDialog)
			{
				(Game1.activeClickableMenu as ReadyCheckDialog).closeDialog(farmer);
			}
			if (Game1.player.team.movieMutex.IsLockHeld())
			{
				Game1.player.team.movieMutex.ReleaseLock();
			}
		});
	}

	/// <summary>Get the data for all movies.</summary>
	public static List<MovieData> GetMovieData()
	{
		if (_movieData == null)
		{
			_movieData = new List<MovieData>();
			_movieDataById = new Dictionary<string, MovieData>();
			foreach (MovieData movie in DataLoader.Movies(Game1.content))
			{
				if (string.IsNullOrWhiteSpace(movie.Id))
				{
					Game1.log.Warn("Ignored movie with no ID.");
				}
				else if (!_movieDataById.TryAdd(movie.Id, movie))
				{
					Game1.log.Warn("Ignored duplicate movie with ID '" + movie.Id + "'.");
				}
				else
				{
					_movieData.Add(movie);
				}
			}
		}
		return _movieData;
	}

	/// <summary>Get the data for all movies by ID.</summary>
	public static Dictionary<string, MovieData> GetMovieDataById()
	{
		if (_movieDataById == null)
		{
			GetMovieData();
		}
		return _movieDataById;
	}

	/// <summary>Get the data for a specific movie, if it exists.</summary>
	/// <param name="id">The movie ID in <c>Data/Movies</c>.</param>
	/// <param name="data">The movie data, if found.</param>
	/// <returns>Returns whether the movie data was found.</returns>
	public static bool TryGetMovieData(string id, out MovieData data)
	{
		if (id == null)
		{
			data = null;
			return false;
		}
		return GetMovieDataById().TryGetValue(id, out data);
	}

	/// <summary>Get the movie ID corresponding to a pre-1.6 movie index.</summary>
	/// <param name="id">The movie index.</param>
	public static string GetMovieIdFromLegacyIndex(string id)
	{
		if (int.TryParse(id, out var index))
		{
			foreach (MovieData movie in GetMovieData())
			{
				if (movie.SheetIndex == index && (string.IsNullOrWhiteSpace(movie.Texture) || movie.Texture == "LooseSprites\\Movies"))
				{
					return movie.Id;
				}
			}
		}
		return id;
	}

	/// <summary>Get the pixel area in a movie's spritesheet which contains a screen frame.</summary>
	/// <param name="movieIndex">The movie's sprite index in its spritesheet.</param>
	/// <param name="frame">The screen index within the movie's area.</param>
	public static Microsoft.Xna.Framework.Rectangle GetSourceRectForScreen(int movieIndex, int frame)
	{
		int yOffset = movieIndex * 128 + frame / 5 * 64;
		int xOffset = frame % 5 * 96;
		return new Microsoft.Xna.Framework.Rectangle(16 + xOffset, yOffset, 90, 61);
	}

	/// <summary>Get the pixel area in a movie's spritesheet which contains a screen frame.</summary>
	/// <param name="movieIndex">The movie's sprite index in its spritesheet.</param>
	public static Microsoft.Xna.Framework.Rectangle GetSourceRectForPoster(int movieIndex)
	{
		return new Microsoft.Xna.Framework.Rectangle(0, movieIndex * 128, 13, 19);
	}

	public NPC GetMoviePatron(string name)
	{
		for (int i = 0; i < characters.Count; i++)
		{
			if (characters[i].name == name)
			{
				return characters[i];
			}
		}
		return null;
	}

	protected NPC AddMoviePatronNPC(string name, int x, int y, int facingDirection)
	{
		if (_spawnedMoviePatrons.ContainsKey(name))
		{
			return GetMoviePatron(name);
		}
		string textureName = NPC.getTextureNameForCharacter(name);
		NPC.TryGetData(name, out var data);
		int width = data?.Size.X ?? 16;
		int height = data?.Size.Y ?? 32;
		NPC n = new NPC(new AnimatedSprite("Characters\\" + textureName, 0, width, height), new Vector2(x * 64, y * 64), base.Name, facingDirection, name, null, eventActor: true);
		n.EventActor = true;
		n.collidesWithOtherCharacters.Set(newValue: false);
		addCharacter(n);
		_spawnedMoviePatrons.Add(name, 1);
		GetDialogueForCharacter(n);
		return n;
	}

	public void RemoveAllPatrons()
	{
		if (_spawnedMoviePatrons == null)
		{
			return;
		}
		for (int i = 0; i < characters.Count; i++)
		{
			if (_spawnedMoviePatrons.ContainsKey(characters[i].Name))
			{
				characters.RemoveAt(i);
				i--;
			}
		}
		_spawnedMoviePatrons.Clear();
	}

	protected override void resetSharedState()
	{
		base.resetSharedState();
		if (CurrentState == 0)
		{
			MovieData movie = GetMovieToday();
			Game1.multiplayer.globalChatInfoMessage("MovieStart", TokenStringBuilder.MovieName(movie.Id));
		}
	}

	protected override void resetLocalState()
	{
		base.resetLocalState();
		Game1.getAchievement(36);
		birds = new PerchingBirds(Game1.birdsSpriteSheet, 2, 16, 16, new Vector2(8f, 14f), new Point[14]
		{
			new Point(19, 5),
			new Point(21, 4),
			new Point(16, 3),
			new Point(10, 13),
			new Point(2, 13),
			new Point(2, 6),
			new Point(9, 2),
			new Point(18, 12),
			new Point(21, 11),
			new Point(3, 11),
			new Point(4, 2),
			new Point(12, 12),
			new Point(11, 5),
			new Point(13, 13)
		}, new Point[6]
		{
			new Point(19, 5),
			new Point(21, 4),
			new Point(16, 3),
			new Point(9, 2),
			new Point(21, 11),
			new Point(4, 2)
		});
		if (!_isJojaTheater && Game1.MasterPlayer.mailReceived.Contains("ccMovieTheaterJoja"))
		{
			_isJojaTheater = true;
		}
		if (dayFirstEntered.Value == -1)
		{
			dayFirstEntered.Value = Game1.Date.TotalDays;
		}
		if (!_isJojaTheater)
		{
			birds.roosting = CurrentState == 2;
			for (int i = 0; i < Game1.random.Next(2, 5); i++)
			{
				int bird_type = Game1.random.Next(0, 4);
				if (IsFallHere())
				{
					bird_type = 10;
				}
				birds.AddBird(bird_type);
			}
			if (Game1.timeOfDay > 2100 && Game1.random.NextBool())
			{
				birds.AddBird(11);
			}
		}
		AddMoviePoster(this, 1104f, 292f);
		loadMap(mapPath, force_reload: true);
		if (_isJojaTheater)
		{
			string addOn = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en) ? "" : "_international");
			base.Map.TileSheets[0].ImageSource = "Maps\\MovieTheaterJoja_TileSheet" + addOn;
			base.Map.LoadTileSheets(Game1.mapDisplayDevice);
		}
		switch (CurrentState)
		{
		case 0:
			addRandomNPCs();
			break;
		case 2:
			Game1.changeMusicTrack("movieTheaterAfter");
			Game1.ambientLight = new Color(150, 170, 80);
			addSpecificRandomNPC(0);
			break;
		}
	}

	private void addRandomNPCs()
	{
		Season season = GetSeason();
		Random r = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.Date.TotalDays);
		critters = new List<Critter>();
		if (dayFirstEntered.Value == Game1.Date.TotalDays || r.NextDouble() < 0.25)
		{
			addSpecificRandomNPC(0);
		}
		if (!_isJojaTheater && r.NextDouble() < 0.28)
		{
			addSpecificRandomNPC(4);
			addSpecificRandomNPC(11);
		}
		else if (_isJojaTheater && r.NextDouble() < 0.33)
		{
			addSpecificRandomNPC(13);
		}
		if (r.NextDouble() < 0.1)
		{
			addSpecificRandomNPC(9);
			addSpecificRandomNPC(7);
		}
		switch (season)
		{
		case Season.Fall:
			if (r.NextBool())
			{
				addSpecificRandomNPC(1);
			}
			break;
		case Season.Spring:
			if (r.NextBool())
			{
				addSpecificRandomNPC(3);
			}
			break;
		}
		if (r.NextDouble() < 0.25)
		{
			addSpecificRandomNPC(2);
		}
		if (r.NextDouble() < 0.25)
		{
			addSpecificRandomNPC(6);
		}
		if (r.NextDouble() < 0.25)
		{
			addSpecificRandomNPC(8);
		}
		if (r.NextDouble() < 0.2)
		{
			addSpecificRandomNPC(10);
		}
		if (r.NextDouble() < 0.2)
		{
			addSpecificRandomNPC(12);
		}
		if (r.NextDouble() < 0.2)
		{
			addSpecificRandomNPC(5);
		}
		if (!_isJojaTheater)
		{
			if (r.NextDouble() < 0.75)
			{
				addCritter(new Butterfly(this, new Vector2(13f, 7f)).setStayInbounds(stayInbounds: true));
			}
			if (r.NextDouble() < 0.75)
			{
				addCritter(new Butterfly(this, new Vector2(4f, 8f)).setStayInbounds(stayInbounds: true));
			}
			if (r.NextDouble() < 0.75)
			{
				addCritter(new Butterfly(this, new Vector2(17f, 10f)).setStayInbounds(stayInbounds: true));
			}
		}
	}

	private void addSpecificRandomNPC(int whichRandomNPC)
	{
		Random r = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.Date.TotalDays, whichRandomNPC);
		switch (whichRandomNPC)
		{
		case 0:
			setMapTile(2, 9, 215, "Buildings", "MessageSpeech MovieTheater_CraneMan" + r.Choose("2", ""));
			setMapTile(2, 8, 199, "Front", null);
			break;
		case 1:
			setMapTile(19, 7, 216, "Buildings", "MessageSpeech MovieTheater_Welwick" + r.Choose("2", ""));
			setMapTile(19, 6, 200, "Front", null);
			break;
		case 2:
			setAnimatedMapTile(21, 7, new int[4] { 217, 217, 217, 218 }, 700L, "Buildings", "MessageSpeech MovieTheater_ShortsMan" + r.Choose("2", ""));
			setAnimatedMapTile(21, 6, new int[4] { 201, 201, 201, 202 }, 700L, "Front", null);
			break;
		case 3:
			setMapTile(5, 9, 219, "Buildings", "MessageSpeech MovieTheater_Mother" + r.Choose("2", ""));
			setMapTile(6, 9, 220, "Buildings", "MessageSpeech MovieTheater_Child" + r.Choose("2", ""));
			setAnimatedMapTile(5, 8, new int[6] { 203, 203, 203, 204, 204, 204 }, 1000L, "Front", null);
			break;
		case 4:
			setMapTileIndex(20, 9, 222, "Front");
			setMapTileIndex(21, 9, 223, "Front");
			setMapTile(20, 10, 238, "Buildings", null);
			setMapTile(21, 10, 239, "Buildings", null);
			setMapTileIndex(20, 11, 254, "Buildings");
			setMapTileIndex(21, 11, 255, "Buildings");
			break;
		case 5:
			setAnimatedMapTile(10, 7, new int[4] { 251, 251, 251, 252 }, 900L, "Buildings", "MessageSpeech MovieTheater_Lupini" + r.Choose("2", ""));
			setAnimatedMapTile(10, 6, new int[4] { 235, 235, 235, 236 }, 900L, "Front", null);
			break;
		case 6:
			setAnimatedMapTile(5, 7, new int[4] { 249, 249, 249, 250 }, 600L, "Buildings", "MessageSpeech MovieTheater_ConcessionMan" + r.Choose("2", ""));
			setAnimatedMapTile(5, 6, new int[4] { 233, 233, 233, 234 }, 600L, "Front", null);
			break;
		case 7:
			setMapTile(1, 12, 248, "Buildings", "MessageSpeech MovieTheater_PurpleHairLady");
			setMapTile(1, 11, 232, "Front", null);
			break;
		case 8:
			setMapTile(3, 8, 247, "Buildings", "MessageSpeech MovieTheater_RedCapGuy" + r.Choose("2", ""));
			setMapTile(3, 7, 231, "Front", null);
			break;
		case 9:
			setMapTile(2, 11, 253, "Buildings", "MessageSpeech MovieTheater_Governor" + r.Choose("2", ""));
			setMapTile(2, 10, 237, "Front", null);
			break;
		case 10:
			setMapTile(9, 7, 221, "Buildings", "NPCSpeechMessageNoRadius Gunther MovieTheater_Gunther" + r.Choose("2", ""));
			setMapTile(9, 6, 205, "Front", null);
			break;
		case 11:
			setMapTile(19, 10, 208, "Buildings", "NPCSpeechMessageNoRadius Marlon MovieTheater_Marlon" + r.Choose("2", ""));
			setMapTile(19, 9, 192, "Front", null);
			break;
		case 12:
			setMapTile(12, 4, 209, "Buildings", "MessageSpeech MovieTheater_Marcello" + r.Choose("2", ""));
			setMapTile(12, 3, 193, "Front", null);
			break;
		case 13:
			setMapTile(17, 12, 241, "Buildings", "NPCSpeechMessageNoRadius Morris MovieTheater_Morris" + r.Choose("2", ""));
			setMapTile(17, 11, 225, "Front", null);
			break;
		}
	}

	/// <summary>Get the movie that plays today.</summary>
	public static MovieData GetMovieToday()
	{
		if (forceMovieId != null)
		{
			if (TryGetMovieData(forceMovieId, out var data))
			{
				return data;
			}
			Game1.log.Warn($"Ignored invalid {"MovieTheater"}.{"forceMovieId"} override '{forceMovieId}'.");
			forceMovieId = null;
		}
		return GetMovieForDate(Game1.Date);
	}

	/// <summary>Get the movies that play in a given season.</summary>
	/// <param name="date">The date whose season and year to check.</param>
	public static List<MovieData> GetMoviesForSeason(WorldDate date)
	{
		WorldDate dayTheaterBuilt = WorldDate.ForDaysPlayed((int)Game1.player.team.theaterBuildDate.Value);
		int relativeBuildYear = date.Year - dayTheaterBuilt.Year;
		List<MovieData> allMovies = GetMovieData();
		List<MovieData> movies = new List<MovieData>();
		foreach (MovieData movie in allMovies)
		{
			if (MovieSeasonMatches(movie, date.Season) && MovieYearMatches(movie, relativeBuildYear))
			{
				movies.Add(movie);
			}
		}
		if (movies.Count == 0)
		{
			foreach (MovieData movie in allMovies)
			{
				if (MovieSeasonMatches(movie, date.Season))
				{
					movies.Add(movie);
				}
			}
		}
		if (movies.Count == 0)
		{
			movies.AddRange(allMovies);
		}
		if (movies.Count > 28)
		{
			Utility.Shuffle(Utility.CreateRandom(Game1.uniqueIDForThisGame, (double)Game1.season, Game1.year), movies);
			movies.RemoveRange(28, movies.Count - 28);
		}
		return movies;
	}

	/// <summary>Get the movie that plays on the given date.</summary>
	/// <param name="date">The date to check.</param>
	public static MovieData GetMovieForDate(WorldDate date)
	{
		List<MovieData> movies = GetMoviesForSeason(date);
		float daysPerMovie = 28f / (float)movies.Count;
		int index = ((int)Math.Ceiling((float)date.DayOfMonth / daysPerMovie) - 1) % movies.Count;
		return movies[index];
	}

	/// <summary>Get the next different movie that will play after today.</summary>
	public static MovieData GetUpcomingMovie()
	{
		return GetUpcomingMovieForDate(Game1.Date);
	}

	/// <summary>Get the next different movie that will play after the given date.</summary>
	/// <param name="afterDate">The date of the current movie for which to get the upcoming movie.</param>
	public static MovieData GetUpcomingMovieForDate(WorldDate afterDate)
	{
		List<MovieData> movies = GetMoviesForSeason(afterDate);
		MovieData currentMovie = GetMovieForDate(afterDate);
		bool foundMovie = false;
		foreach (MovieData movie in movies)
		{
			if (movie.Id == currentMovie.Id)
			{
				foundMovie = true;
			}
			else if (foundMovie)
			{
				return movie;
			}
		}
		movies = GetMoviesForSeason(WorldDate.ForDaysPlayed(afterDate.TotalDays + 28));
		foreach (MovieData movie in movies)
		{
			if (movie.Id != currentMovie.Id)
			{
				return movie;
			}
		}
		return movies[0];
	}

	/// <summary>Get whether a movie should play in a given year.</summary>
	/// <param name="movie">The movie data to check.</param>
	/// <param name="year">The relative year when the movie theater was built (e.g. 0 if built this year).</param>
	public static bool MovieYearMatches(MovieData movie, int year)
	{
		int? yearModulus = movie.YearModulus;
		if (!yearModulus.HasValue)
		{
			return true;
		}
		int modulus = movie.YearModulus.Value;
		int remainder = movie.YearRemainder.GetValueOrDefault();
		if (modulus < 1)
		{
			Game1.log.Warn($"Movie '{movie.Id}' has invalid year modulus {movie.YearModulus}, must be a number greater than zero.");
			return false;
		}
		return year % modulus == remainder;
	}

	/// <summary>Get whether a movie should play in a given season.</summary>
	/// <param name="movie">The movie data to check.</param>
	/// <param name="season">The calendar season.</param>
	public static bool MovieSeasonMatches(MovieData movie, Season season)
	{
		List<Season> seasons = movie.Seasons;
		if (seasons != null && seasons.Count > 0)
		{
			return movie.Seasons.Contains(season);
		}
		return true;
	}

	public override void DayUpdate(int dayOfMonth)
	{
		ShowingId = 0;
		ResetTheater();
		_ResetHangoutPoints();
		base.DayUpdate(dayOfMonth);
	}

	public override void UpdateWhenCurrentLocation(GameTime time)
	{
		if (_farmerCount != farmers.Count)
		{
			_farmerCount = farmers.Count;
			if (Game1.activeClickableMenu is ReadyCheckDialog readyCheckDialog)
			{
				readyCheckDialog.closeDialog(Game1.player);
				if (Game1.player.team.movieMutex.IsLockHeld())
				{
					Game1.player.team.movieMutex.ReleaseLock();
				}
			}
		}
		birds?.Update(time);
		base.UpdateWhenCurrentLocation(time);
	}

	public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
	{
		birds?.Draw(b);
		base.drawAboveAlwaysFrontLayer(b);
	}

	public static bool Invite(Farmer farmer, NPC invited_npc)
	{
		if (farmer == null || invited_npc == null)
		{
			return false;
		}
		MovieInvitation invitation = new MovieInvitation();
		invitation.farmer = farmer;
		invitation.invitedNPC = invited_npc;
		farmer.team.movieInvitations.Add(invitation);
		return true;
	}

	public void ResetTheater()
	{
		_playerHangoutGroup = -1;
		RemoveAllPatrons();
		_playerGroups.Clear();
		_npcGroups.Clear();
		_viewingGroups.Clear();
		_viewingFarmers.Clear();
		_purchasedConcessions.Clear();
		_playerInvitedPatrons.Clear();
		_characterGroupLookup.Clear();
		_ResetHangoutPoints();
		Game1.player.team.movieMutex.ReleaseLock();
		CurrentState = 0;
	}

	public override void updateEvenIfFarmerIsntHere(GameTime time, bool ignoreWasUpdatedFlush = false)
	{
		base.updateEvenIfFarmerIsntHere(time, ignoreWasUpdatedFlush);
		movieViewerLockEvent.Poll();
		requestStartMovieEvent.Poll();
		startMovieEvent.Poll();
		endMovieEvent.Poll();
		if (!Game1.IsMasterGame)
		{
			return;
		}
		for (int i = 0; i < _viewingFarmers.Count; i++)
		{
			Farmer viewing_farmer = _viewingFarmers[i];
			if (!Game1.getOnlineFarmers().Contains(viewing_farmer))
			{
				_viewingFarmers.RemoveAt(i);
				i--;
			}
			else if (CurrentState == 2 && !farmers.Contains(viewing_farmer) && !HasFarmerWatchingBroadcastEventReturningHere() && viewing_farmer.currentLocation != null && !viewing_farmer.currentLocation.IsTemporary)
			{
				_viewingFarmers.RemoveAt(i);
				i--;
			}
		}
		if (CurrentState != 0 && _viewingFarmers.Count == 0)
		{
			MovieData movie = GetMovieToday();
			Game1.multiplayer.globalChatInfoMessage("MovieEnd", TokenStringBuilder.MovieName(movie.Id));
			ResetTheater();
			ShowingId++;
		}
		if (Game1.player.team.movieInvitations == null || _playerInvitedPatrons.Count() >= 8)
		{
			return;
		}
		foreach (Farmer farmer in farmers)
		{
			for (int i = 0; i < Game1.player.team.movieInvitations.Count; i++)
			{
				MovieInvitation invite = Game1.player.team.movieInvitations[i];
				if (invite.fulfilled || _spawnedMoviePatrons.ContainsKey(invite.invitedNPC.displayName))
				{
					continue;
				}
				if (_playerHangoutGroup < 0)
				{
					_playerHangoutGroup = Game1.random.Next(_maxHangoutGroups);
				}
				int group = _playerHangoutGroup;
				if (invite.farmer == farmer && GetFirstInvitedPlayer(invite.invitedNPC) == farmer)
				{
					while (_availableHangoutPoints[group].Count == 0)
					{
						group = Game1.random.Next(_maxHangoutGroups);
					}
					Point point = Game1.random.ChooseFrom(_availableHangoutPoints[group]);
					NPC character = AddMoviePatronNPC(invite.invitedNPC.name, 14, 15, 0);
					_playerInvitedPatrons.Add(character.name, 1);
					_availableHangoutPoints[group].Remove(point);
					int direction = 2;
					if (map.GetLayer("Paths").Tiles[point.X, point.Y].Properties != null && map.GetLayer("Paths").Tiles[point.X, point.Y].Properties.ContainsKey("direction"))
					{
						int.TryParse(map.GetLayer("Paths").Tiles[point.X, point.Y].Properties["direction"], out direction);
					}
					_destinationPositions[character.Name] = new KeyValuePair<Point, int>(point, direction);
					PathCharacterToLocation(character, point, direction);
					invite.fulfilled = true;
				}
			}
		}
	}

	public static MovieCharacterReaction GetReactionsForCharacter(NPC character)
	{
		if (character == null)
		{
			return null;
		}
		foreach (MovieCharacterReaction reactions in GetMovieReactions())
		{
			if (!(reactions.NPCName != character.Name))
			{
				return reactions;
			}
		}
		return null;
	}

	/// <inheritdoc />
	public override void checkForMusic(GameTime time)
	{
	}

	public static string GetResponseForMovie(NPC character)
	{
		string response = "like";
		MovieData movie = GetMovieToday();
		if (movie == null)
		{
			return null;
		}
		if (movie != null)
		{
			foreach (MovieCharacterReaction reactions in GetMovieReactions())
			{
				if (reactions.NPCName != character.Name)
				{
					continue;
				}
				foreach (MovieReaction tagged_reactions in reactions.Reactions)
				{
					if (tagged_reactions.ShouldApplyToMovie(movie, GetPatronNames()))
					{
						string response2 = tagged_reactions.Response;
						if (response2 != null && response2.Length > 0)
						{
							response = tagged_reactions.Response;
							break;
						}
					}
				}
			}
		}
		return response;
	}

	public Dialogue GetDialogueForCharacter(NPC character)
	{
		MovieData movie = GetMovieToday();
		if (movie != null)
		{
			foreach (MovieCharacterReaction reactions in _genericReactions)
			{
				if (reactions.NPCName != character.Name)
				{
					continue;
				}
				foreach (MovieReaction tagged_reactions in reactions.Reactions)
				{
					if (!tagged_reactions.ShouldApplyToMovie(movie, GetPatronNames(), GetResponseForMovie(character)))
					{
						continue;
					}
					string response = tagged_reactions.Response;
					if (response == null || response.Length <= 0 || tagged_reactions.SpecialResponses == null)
					{
						continue;
					}
					switch (CurrentState)
					{
					case 0:
						if (tagged_reactions.SpecialResponses.BeforeMovie != null)
						{
							return new Dialogue(character, null, FormatString(tagged_reactions.SpecialResponses.BeforeMovie.Text));
						}
						break;
					case 1:
						if (tagged_reactions.SpecialResponses.DuringMovie != null)
						{
							return new Dialogue(character, null, FormatString(tagged_reactions.SpecialResponses.DuringMovie.Text));
						}
						break;
					case 2:
						if (tagged_reactions.SpecialResponses.AfterMovie != null)
						{
							return new Dialogue(character, null, FormatString(tagged_reactions.SpecialResponses.AfterMovie.Text));
						}
						break;
					}
					break;
				}
				break;
			}
		}
		return null;
	}

	public string FormatString(string text, params string[] args)
	{
		text = TokenParser.ParseText(text);
		string title = TokenParser.ParseText(GetMovieToday().Title);
		return string.Format(text, title, Game1.player.displayName, args);
	}

	public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
	{
		Microsoft.Xna.Framework.Rectangle tileRect = new Microsoft.Xna.Framework.Rectangle(tileLocation.X * 64, tileLocation.Y * 64, 64, 64);
		string[] action = GetTilePropertySplitBySpaces("Action", "Buildings", tileLocation.X, tileLocation.Y);
		if (action.Length != 0)
		{
			return performAction(action, who, tileLocation);
		}
		foreach (NPC npc in characters)
		{
			if (npc == null || npc.IsMonster || (who.isRidingHorse() && npc is Horse) || !npc.GetBoundingBox().Intersects(tileRect))
			{
				continue;
			}
			if (!npc.isMoving())
			{
				bool is_in_group;
				if (_playerInvitedPatrons.ContainsKey(npc.Name))
				{
					npc.faceTowardFarmerForPeriod(5000, 4, faceAway: false, who);
					Dialogue dialogue = GetDialogueForCharacter(npc);
					if (dialogue != null)
					{
						npc.CurrentDialogue.Push(dialogue);
						Game1.drawDialogue(npc);
						npc.grantConversationFriendship(Game1.player);
					}
				}
				else if (_characterGroupLookup.TryGetValue(npc.Name, out is_in_group))
				{
					if (!is_in_group)
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Characters:MovieTheater_AfterMovieAlone", npc.displayName));
					}
					else
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Characters:MovieTheater_AfterMovie", npc.displayName));
					}
				}
			}
			return true;
		}
		return base.checkAction(tileLocation, viewport, who);
	}

	protected void _PopulateNPCOnlyGroups(List<List<Character>> player_groups, List<List<Character>> groups)
	{
		HashSet<string> used_characters = new HashSet<string>();
		foreach (List<Character> player_group in player_groups)
		{
			foreach (Character character in player_group)
			{
				if (character is NPC)
				{
					used_characters.Add(character.name);
				}
			}
		}
		foreach (List<Character> group in groups)
		{
			foreach (Character character in group)
			{
				if (character is NPC)
				{
					used_characters.Add(character.name);
				}
			}
		}
		Random r = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.Date.TotalDays);
		int group_count = 0;
		for (int i = 0; i < 2; i++)
		{
			if (r.NextDouble() < 0.75)
			{
				group_count++;
			}
		}
		int time_of_day = 0;
		if (_movieStartTime >= 1200)
		{
			time_of_day = 1;
		}
		if (_movieStartTime >= 1800)
		{
			time_of_day = 2;
		}
		string[][] possible_npcs_for_this_day = possibleNPCGroups[(int)Game1.Date.DayOfWeek][time_of_day];
		if (possible_npcs_for_this_day == null)
		{
			return;
		}
		if (groups.Count > 0 && groups[0].Count == 0)
		{
			groups.RemoveAt(0);
		}
		for (int i = 0; i < group_count; i++)
		{
			if (groups.Count >= 2)
			{
				break;
			}
			string[] characters = r.Choose(possible_npcs_for_this_day);
			bool valid = true;
			string[] array = characters;
			foreach (string character in array)
			{
				bool found_friendship = false;
				foreach (Farmer allFarmer in Game1.getAllFarmers())
				{
					if (allFarmer.friendshipData.ContainsKey(character))
					{
						found_friendship = true;
						break;
					}
				}
				if (!found_friendship)
				{
					valid = false;
					break;
				}
				if (used_characters.Contains(character))
				{
					valid = false;
					break;
				}
				if (GetResponseForMovie(Game1.getCharacterFromName(character)) == "dislike" || GetResponseForMovie(Game1.getCharacterFromName(character)) == "reject")
				{
					valid = false;
					break;
				}
			}
			if (valid)
			{
				List<Character> new_group = new List<Character>();
				array = characters;
				foreach (string character in array)
				{
					NPC patron = AddMoviePatronNPC(character, 1000, 1000, 2);
					new_group.Add(patron);
					used_characters.Add(character);
					_characterGroupLookup[character] = characters.Length > 1;
				}
				groups.Add(new_group);
			}
		}
	}

	public Dictionary<Character, MovieConcession> GetConcessionsDictionary()
	{
		Dictionary<Character, MovieConcession> dictionary = new Dictionary<Character, MovieConcession>();
		foreach (string npc_name in _purchasedConcessions.Keys)
		{
			Character character = Game1.getCharacterFromName(npc_name);
			if (character != null && GetConcessions().TryGetValue(_purchasedConcessions[npc_name], out var purchasedConcession))
			{
				dictionary[character] = purchasedConcession;
			}
		}
		return dictionary;
	}

	protected void _ResetHangoutPoints()
	{
		_destinationPositions.Clear();
		_availableHangoutPoints = new Dictionary<int, List<Point>>();
		foreach (int key in _hangoutPoints.Keys)
		{
			_availableHangoutPoints[key] = new List<Point>(_hangoutPoints[key]);
		}
	}

	public override void cleanupBeforePlayerExit()
	{
		if (!Game1.eventUp)
		{
			Game1.changeMusicTrack("none");
		}
		birds = null;
		base.cleanupBeforePlayerExit();
	}

	public void RequestEndMovie(long uid)
	{
		if (!Game1.IsMasterGame)
		{
			return;
		}
		if (CurrentState == 1)
		{
			CurrentState = 2;
			for (int i = 0; i < _viewingGroups.Count; i++)
			{
				int index = Game1.random.Next(_viewingGroups.Count);
				List<Character> characters = _viewingGroups[i];
				_viewingGroups[i] = _viewingGroups[index];
				_viewingGroups[index] = characters;
			}
			_ResetHangoutPoints();
			int character_index = 0;
			for (int group = 0; group < _viewingGroups.Count; group++)
			{
				for (int i = 0; i < _viewingGroups[group].Count; i++)
				{
					if (!(_viewingGroups[group][i] is NPC))
					{
						continue;
					}
					NPC patron_character = GetMoviePatron(_viewingGroups[group][i].Name);
					if (patron_character != null)
					{
						patron_character.setTileLocation(new Vector2(14f, 4f + (float)character_index * 1f));
						Point point = Game1.random.ChooseFrom(_availableHangoutPoints[group]);
						if (!int.TryParse(doesTileHaveProperty(point.X, point.Y, "direction", "Paths"), out var direction))
						{
							direction = 2;
						}
						_destinationPositions[patron_character.Name] = new KeyValuePair<Point, int>(point, direction);
						PathCharacterToLocation(patron_character, point, direction);
						_availableHangoutPoints[group].Remove(point);
						character_index++;
					}
				}
			}
		}
		Game1.getFarmer(uid).team.endMovieEvent.Fire(uid);
	}

	public void PathCharacterToLocation(NPC character, Point point, int direction)
	{
		if (character.currentLocation == this)
		{
			PathFindController controller = new PathFindController(character, this, character.TilePoint, direction);
			controller.pathToEndPoint = PathFindController.findPathForNPCSchedules(character.TilePoint, point, this, 30000);
			character.temporaryController = controller;
			character.followSchedule = true;
			character.ignoreScheduleToday = true;
		}
	}

	public static Dictionary<string, MovieConcession> GetConcessions()
	{
		if (_concessions == null)
		{
			_concessions = new Dictionary<string, MovieConcession>();
			foreach (ConcessionItemData data in DataLoader.Concessions(Game1.content))
			{
				_concessions[data.Id] = new MovieConcession(data);
			}
		}
		return _concessions;
	}

	/// <summary>Get a movie concession.</summary>
	/// <param name="id">The concession ID.</param>
	public static MovieConcession GetConcessionItem(string id)
	{
		if (id == null || !GetConcessions().TryGetValue(id, out var concession))
		{
			return null;
		}
		return concession;
	}

	public bool OnPurchaseConcession(ISalable salable, Farmer who, int amount)
	{
		foreach (MovieInvitation invitation in who.team.movieInvitations)
		{
			if (invitation.farmer == who && GetFirstInvitedPlayer(invitation.invitedNPC) == Game1.player && _spawnedMoviePatrons.ContainsKey(invitation.invitedNPC.Name))
			{
				_purchasedConcessions[invitation.invitedNPC.Name] = (salable as MovieConcession).Id;
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Characters:MovieTheater_ConcessionPurchased", (salable as MovieConcession).DisplayName, invitation.invitedNPC.displayName));
				return true;
			}
		}
		return false;
	}

	public bool HasInvitedSomeone(Farmer who)
	{
		foreach (MovieInvitation invitation in who.team.movieInvitations)
		{
			if (invitation.farmer == who && GetFirstInvitedPlayer(invitation.invitedNPC) == Game1.player && _spawnedMoviePatrons.ContainsKey(invitation.invitedNPC.Name))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasPurchasedConcession(Farmer who)
	{
		if (!HasInvitedSomeone(who))
		{
			return false;
		}
		foreach (MovieInvitation invitation in who.team.movieInvitations)
		{
			if (invitation.farmer != who || GetFirstInvitedPlayer(invitation.invitedNPC) != Game1.player)
			{
				continue;
			}
			foreach (string key in _purchasedConcessions.Keys)
			{
				if (key == invitation.invitedNPC.Name && _spawnedMoviePatrons.ContainsKey(invitation.invitedNPC.Name))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static Farmer GetFirstInvitedPlayer(NPC npc)
	{
		foreach (MovieInvitation invitation in Game1.player.team.movieInvitations)
		{
			if (invitation.invitedNPC.Name == npc.Name)
			{
				return invitation.farmer;
			}
		}
		return null;
	}

	/// <inheritdoc />
	public override void performTouchAction(string[] action, Vector2 playerStandingPosition)
	{
		if (IgnoreTouchActions())
		{
			return;
		}
		if (ArgUtility.Get(action, 0) == "Theater_Exit")
		{
			if (!ArgUtility.TryGetPoint(action, 1, out var exitTile, out var error))
			{
				LogTileTouchActionError(action, playerStandingPosition, error);
				return;
			}
			Point offset = Town.GetTheaterTileOffset();
			_exitX = exitTile.X + offset.X;
			_exitY = exitTile.Y + offset.Y;
			if ((int)Game1.player.lastSeenMovieWeek >= Game1.Date.TotalWeeks)
			{
				_Leave();
				return;
			}
			Game1.player.position.Y -= ((float)Game1.player.Speed + Game1.player.addedSpeed) * 2f;
			Game1.player.Halt();
			Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Characters:MovieTheater_LeavePrompt"), Game1.currentLocation.createYesNoResponses(), "LeaveMovie");
		}
		else
		{
			base.performTouchAction(action, playerStandingPosition);
		}
	}

	public static List<MovieConcession> GetConcessionsForGuest()
	{
		string npcName = null;
		foreach (MovieInvitation invitation in Game1.player.team.movieInvitations)
		{
			if (invitation.farmer == Game1.player && GetFirstInvitedPlayer(invitation.invitedNPC) == Game1.player)
			{
				npcName = invitation.invitedNPC.Name;
				break;
			}
		}
		return GetConcessionsForGuest(npcName);
	}

	public static List<MovieConcession> GetConcessionsForGuest(string npc_name)
	{
		if (npc_name == null)
		{
			npc_name = "Abigail";
		}
		List<MovieConcession> concessions = new List<MovieConcession>();
		List<MovieConcession> all_concessions = GetConcessions().Values.ToList();
		Random r = Utility.CreateDaySaveRandom();
		Utility.Shuffle(r, all_concessions);
		NPC npc = Game1.getCharacterFromName(npc_name);
		if (npc == null)
		{
			return concessions;
		}
		int num_loved = 1;
		int num_liked = 2;
		int num_disliked = 1;
		int min_concessions = 5;
		for (int j = 0; j < num_loved; j++)
		{
			for (int i = 0; i < all_concessions.Count; i++)
			{
				MovieConcession concession = all_concessions[i];
				if (GetConcessionTasteForCharacter(npc, concession) == "love" && (!concession.Name.Equals("Stardrop Sorbet") || r.NextDouble() < 0.33))
				{
					concessions.Add(concession);
					all_concessions.RemoveAt(i);
					i--;
					break;
				}
			}
		}
		for (int j = 0; j < num_liked; j++)
		{
			for (int i = 0; i < all_concessions.Count; i++)
			{
				MovieConcession concession = all_concessions[i];
				if (GetConcessionTasteForCharacter(npc, concession) == "like")
				{
					concessions.Add(concession);
					all_concessions.RemoveAt(i);
					i--;
					break;
				}
			}
		}
		for (int j = 0; j < num_disliked; j++)
		{
			for (int i = 0; i < all_concessions.Count; i++)
			{
				MovieConcession concession = all_concessions[i];
				if (GetConcessionTasteForCharacter(npc, concession) == "dislike")
				{
					concessions.Add(concession);
					all_concessions.RemoveAt(i);
					i--;
					break;
				}
			}
		}
		for (int j = concessions.Count; j < min_concessions; j++)
		{
			int i = 0;
			if (i < all_concessions.Count)
			{
				MovieConcession concession = all_concessions[i];
				concessions.Add(concession);
				all_concessions.RemoveAt(i);
				i--;
			}
		}
		if (_isJojaTheater && !concessions.Exists((MovieConcession x) => x.Name.Equals("JojaCorn")))
		{
			MovieConcession jojaCorn = all_concessions.Find((MovieConcession x) => x.Name.Equals("JojaCorn"));
			if (jojaCorn != null)
			{
				concessions.Add(jojaCorn);
			}
		}
		Utility.Shuffle(r, concessions);
		return concessions;
	}

	public override bool answerDialogueAction(string questionAndAnswer, string[] questionParams)
	{
		if (questionAndAnswer == null)
		{
			return false;
		}
		if (!(questionAndAnswer == "LeaveMovie_Yes"))
		{
			if (questionAndAnswer == "Concession_Yes")
			{
				Utility.TryOpenShopMenu("Concessions", this, null, null, forceOpen: true);
				if (Game1.activeClickableMenu is ShopMenu menu)
				{
					menu.onPurchase = OnPurchaseConcession;
				}
				return true;
			}
			return base.answerDialogueAction(questionAndAnswer, questionParams);
		}
		_Leave();
		return true;
	}

	protected void _Leave()
	{
		forceMovieId = null;
		Game1.player.completelyStopAnimatingOrDoingAction();
		Game1.warpFarmer("Town", _exitX, _exitY, 2);
	}

	/// <inheritdoc />
	public override bool performAction(string[] action, Farmer who, Location tileLocation)
	{
		switch (ArgUtility.Get(action, 0))
		{
		case "Concessions":
			if (CurrentState > 0)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Characters:MovieTheater_ConcessionAfterMovie"));
				return true;
			}
			if (!HasInvitedSomeone(who))
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Characters:MovieTheater_ConcessionAlone"));
				return true;
			}
			if (HasPurchasedConcession(who))
			{
				foreach (MovieInvitation invitation in who.team.movieInvitations)
				{
					if (invitation.farmer != who || GetFirstInvitedPlayer(invitation.invitedNPC) != Game1.player)
					{
						continue;
					}
					foreach (string name in _purchasedConcessions.Keys)
					{
						if (name == invitation.invitedNPC.Name)
						{
							MovieConcession concession = GetConcessionsDictionary()[Game1.getCharacterFromName(name)];
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Characters:MovieTheater_ConcessionPurchased", concession.DisplayName, Game1.RequireCharacter(name).displayName));
							return true;
						}
					}
				}
				return true;
			}
			Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Characters:MovieTheater_Concession"), Game1.currentLocation.createYesNoResponses(), "Concession");
			return true;
		case "Theater_Doors":
			if (CurrentState > 0)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Theater_MovieEndReEntry"));
				return true;
			}
			if (Game1.player.team.movieMutex.IsLocked())
			{
				_ShowMovieStartReady();
				return true;
			}
			Game1.player.team.movieMutex.RequestLock(delegate
			{
				List<Farmer> list = new List<Farmer>();
				foreach (Farmer current in farmers)
				{
					if (current.isActive() && current.currentLocation == this)
					{
						list.Add(current);
					}
				}
				movieViewerLockEvent.Fire(new MovieViewerLockEvent(list, Game1.timeOfDay));
			});
			return true;
		case "CraneGame":
			if (getTileIndexAt(2, 9, "Buildings") == -1)
			{
				createQuestionDialogue(Game1.content.LoadString("Strings\\StringsFromMaps:MovieTheater_CranePlay", 500), createYesNoResponses(), tryToStartCraneGame);
			}
			else
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromMaps:MovieTheater_CraneOccupied"));
			}
			return true;
		default:
			return base.performAction(action, who, tileLocation);
		}
	}

	private void tryToStartCraneGame(Farmer who, string whichAnswer)
	{
		if (!(whichAnswer.ToLower() == "yes"))
		{
			return;
		}
		if (Game1.player.Money >= 500)
		{
			Game1.player.Money -= 500;
			Game1.changeMusicTrack("none", track_interruptable: false, MusicContext.MiniGame);
			Game1.globalFadeToBlack(delegate
			{
				Game1.currentMinigame = new CraneGame();
			}, 0.008f);
		}
		else
		{
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11325"));
		}
	}

	public static void ClearCachedLocalizedData()
	{
		_concessions = null;
		_genericReactions = null;
		_movieData = null;
	}

	/// <summary>Reset the cached concession tastes, so they're reloaded from <c>Data/ConcessionTastes</c> next time they're accessed.</summary>
	public static void ClearCachedConcessionTastes()
	{
		_concessionTastes = null;
	}
}
