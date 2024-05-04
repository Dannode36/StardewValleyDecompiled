using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Extensions;
using StardewValley.GameData;

namespace StardewValley.Locations;

public class IslandSouthEast : IslandLocation
{
	[XmlIgnore]
	public Texture2D mermaidSprites;

	[XmlIgnore]
	public int lastPlayedNote = -1;

	[XmlIgnore]
	public int songIndex = -1;

	[XmlIgnore]
	public int[] mermaidIdle = new int[1];

	[XmlIgnore]
	public int[] mermaidWave = new int[4] { 1, 1, 2, 2 };

	[XmlIgnore]
	public int[] mermaidReward = new int[7] { 3, 3, 3, 3, 3, 4, 4 };

	[XmlIgnore]
	public int[] mermaidDance = new int[6] { 5, 5, 5, 6, 6, 6 };

	[XmlIgnore]
	public int mermaidFrameIndex;

	[XmlIgnore]
	public int[] currentMermaidAnimation;

	[XmlIgnore]
	public float mermaidFrameTimer;

	[XmlIgnore]
	public float mermaidDanceTime;

	[XmlIgnore]
	public NetEvent0 mermaidPuzzleSuccess = new NetEvent0();

	[XmlElement("mermaidPuzzleFinished")]
	public NetBool mermaidPuzzleFinished = new NetBool();

	[XmlIgnore]
	public NetEvent0 fishWalnutEvent = new NetEvent0();

	[XmlElement("fishedWalnut")]
	public NetBool fishedWalnut = new NetBool();

	public IslandSouthEast()
	{
	}

	public IslandSouthEast(string map, string name)
		: base(map, name)
	{
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(mermaidPuzzleSuccess, "mermaidPuzzleSuccess").AddField(mermaidPuzzleFinished, "mermaidPuzzleFinished").AddField(fishWalnutEvent, "fishWalnutEvent")
			.AddField(fishedWalnut, "fishedWalnut");
		mermaidPuzzleSuccess.onEvent += OnMermaidPuzzleSuccess;
		fishWalnutEvent.onEvent += OnFishWalnut;
	}

	public virtual void OnMermaidPuzzleSuccess()
	{
		currentMermaidAnimation = mermaidReward;
		mermaidFrameTimer = 0f;
		if (Game1.currentLocation == this)
		{
			Game1.playSound("yoba");
		}
		if (Game1.IsMasterGame && !mermaidPuzzleFinished.Value)
		{
			Game1.player.team.MarkCollectedNut("Mermaid");
			mermaidPuzzleFinished.Value = true;
			for (int i = 0; i < 5; i++)
			{
				Game1.createItemDebris(ItemRegistry.Create("(O)73"), new Vector2(32f, 33f) * 64f, 0, this, 0);
			}
		}
	}

	public override void MakeMapModifications(bool force = false)
	{
		base.MakeMapModifications(force);
		if (IsRainingHere())
		{
			setMapTile(16, 27, 3, "Back", "", 2);
			setMapTile(18, 27, 4, "Back", "", 2);
			setMapTile(20, 27, 5, "Back", "", 2);
			setMapTile(22, 27, 6, "Back", "", 2);
			setMapTile(24, 27, 7, "Back", "", 2);
			setMapTile(26, 27, 8, "Back", "", 2);
		}
		else
		{
			setMapTile(16, 27, 39, "Back", "");
			setMapTile(18, 27, 39, "Back", "");
			setMapTile(20, 27, 39, "Back", "");
			setMapTile(22, 27, 39, "Back", "");
			setMapTile(24, 27, 39, "Back", "");
			setMapTile(26, 27, 39, "Back", "");
		}
		if (IslandSouthEastCave.isPirateNight())
		{
			setMapTileIndex(29, 18, 36, "Buildings", 2);
			setTileProperty(29, 18, "Buildings", "Passable", "T");
			setMapTileIndex(29, 19, 68, "Buildings", 2);
			setTileProperty(29, 19, "Buildings", "Passable", "T");
			setMapTileIndex(30, 18, 99, "Buildings", 2);
			setTileProperty(30, 18, "Buildings", "Passable", "T");
			setMapTileIndex(30, 19, 131, "Buildings", 2);
			setTileProperty(30, 19, "Buildings", "Passable", "T");
		}
		else
		{
			setMapTileIndex(29, 18, 35, "Buildings", 2);
			setTileProperty(29, 18, "Buildings", "Passable", "T");
			setMapTileIndex(29, 19, 67, "Buildings", 2);
			setTileProperty(29, 19, "Buildings", "Passable", "T");
			setMapTileIndex(30, 18, 35, "Buildings", 2);
			setTileProperty(30, 18, "Buildings", "Passable", "T");
			setMapTileIndex(30, 19, 67, "Buildings", 2);
			setTileProperty(30, 19, "Buildings", "Passable", "T");
		}
	}

	protected override void resetLocalState()
	{
		base.resetLocalState();
		mermaidSprites = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
		if (IslandSouthEastCave.isPirateNight())
		{
			Game1.changeMusicTrack("PIRATE_THEME(muffled)", track_interruptable: true, MusicContext.SubLocation);
			if (!hasLightSource(797))
			{
				sharedLights.Add(797, new LightSource(1, new Vector2(30.5f, 18.5f) * 64f, 4f, LightSource.LightContext.None, 0L));
			}
		}
		if (AreMoonlightJelliesOut())
		{
			addMoonlightJellies(50, Utility.CreateRandom(Game1.stats.DaysPlayed, Game1.uniqueIDForThisGame, -24917.0), new Rectangle(0, 0, 0, 0));
		}
	}

	public override void cleanupBeforePlayerExit()
	{
		removeLightSource(797);
		base.cleanupBeforePlayerExit();
	}

	public override void SetBuriedNutLocations()
	{
		base.SetBuriedNutLocations();
		buriedNutPoints.Add(new Point(25, 17));
	}

	public override void UpdateWhenCurrentLocation(GameTime time)
	{
		base.UpdateWhenCurrentLocation(time);
		mermaidPuzzleSuccess.Poll();
		fishWalnutEvent.Poll();
		if (!fishedWalnut && Game1.random.NextDouble() < 0.005)
		{
			playSound("waterSlosh");
			temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 0, 64, 64), 150f, 8, 0, new Vector2(1216f, 1344f), flicker: false, Game1.random.NextBool(), 0.001f, 0.01f, Color.White, 1f, 0.003f, 0f, 0f));
		}
		if (!MermaidIsHere())
		{
			return;
		}
		bool should_wave = false;
		if (mermaidPuzzleFinished.Value)
		{
			foreach (Farmer farmer in farmers)
			{
				Point point = farmer.TilePoint;
				if (point.X > 24 && point.Y > 25)
				{
					should_wave = true;
					break;
				}
			}
		}
		if (should_wave && (currentMermaidAnimation == null || currentMermaidAnimation == mermaidIdle))
		{
			currentMermaidAnimation = mermaidWave;
			mermaidFrameIndex = 0;
			mermaidFrameTimer = 0f;
		}
		if (mermaidDanceTime > 0f)
		{
			if (currentMermaidAnimation == null || currentMermaidAnimation == mermaidIdle)
			{
				currentMermaidAnimation = mermaidDance;
				mermaidFrameTimer = 0f;
			}
			mermaidDanceTime -= (float)time.ElapsedGameTime.TotalSeconds;
			if (mermaidDanceTime < 0f && currentMermaidAnimation == mermaidDance)
			{
				currentMermaidAnimation = mermaidIdle;
				mermaidFrameTimer = 0f;
			}
		}
		mermaidFrameTimer += (float)time.ElapsedGameTime.TotalSeconds;
		if (!(mermaidFrameTimer > 0.25f))
		{
			return;
		}
		mermaidFrameTimer = 0f;
		mermaidFrameIndex++;
		if (currentMermaidAnimation == null)
		{
			mermaidFrameIndex = 0;
		}
		else
		{
			if (mermaidFrameIndex < currentMermaidAnimation.Length)
			{
				return;
			}
			mermaidFrameIndex = 0;
			if (currentMermaidAnimation == mermaidReward)
			{
				if (should_wave)
				{
					currentMermaidAnimation = mermaidWave;
				}
				else
				{
					currentMermaidAnimation = mermaidIdle;
				}
			}
			else if (!should_wave && currentMermaidAnimation == mermaidWave)
			{
				currentMermaidAnimation = mermaidIdle;
			}
		}
	}

	public bool MermaidIsHere()
	{
		return IsRainingHere();
	}

	public override void draw(SpriteBatch b)
	{
		base.draw(b);
		if (MermaidIsHere())
		{
			int frame = 0;
			if (mermaidFrameIndex < currentMermaidAnimation?.Length)
			{
				frame = currentMermaidAnimation[mermaidFrameIndex];
			}
			b.Draw(mermaidSprites, Game1.GlobalToLocal(new Vector2(32f, 32f) * 64f + new Vector2(0f, -8f) * 4f), new Rectangle(304 + 28 * frame, 592, 28, 36), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0009f);
		}
	}

	public override Item getFish(float millisecondsAfterNibble, string bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string locationName = null)
	{
		if ((int)bobberTile.X >= 18 && (int)bobberTile.X <= 20 && (int)bobberTile.Y >= 20 && (int)bobberTile.Y <= 22)
		{
			if (!fishedWalnut.Value)
			{
				Game1.player.team.MarkCollectedNut("StardropPool");
				if (!Game1.IsMultiplayer)
				{
					fishedWalnut.Value = true;
					return ItemRegistry.Create("(O)73");
				}
				fishWalnutEvent.Fire();
			}
			return null;
		}
		return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, (string)null);
	}

	public void OnFishWalnut()
	{
		if (!fishedWalnut.Value && Game1.IsMasterGame)
		{
			Vector2 tile = new Vector2(19f, 21f);
			Game1.createItemDebris(ItemRegistry.Create("(O)73"), tile * 64f + new Vector2(0.5f, 1.5f) * 64f, 0, this, 0);
			Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(28, 100f, 2, 1, tile * 64f, flicker: false, flipped: false)
			{
				layerDepth = ((tile.Y + 0.5f) * 64f + 2f) / 10000f
			});
			playSound("dropItemInWater");
			fishedWalnut.Value = true;
		}
	}

	public override void TransferDataFromSavedLocation(GameLocation l)
	{
		base.TransferDataFromSavedLocation(l);
		if (l is IslandSouthEast islandSouthEast)
		{
			mermaidPuzzleFinished.Value = islandSouthEast.mermaidPuzzleFinished.Value;
			fishedWalnut.Value = islandSouthEast.fishedWalnut.Value;
		}
	}

	public virtual void OnFlutePlayed(int pitch)
	{
		if (!MermaidIsHere())
		{
			return;
		}
		if (songIndex == -1)
		{
			lastPlayedNote = pitch;
			songIndex = 0;
		}
		int relative_pitch = pitch - lastPlayedNote;
		if (relative_pitch == 900)
		{
			songIndex = 1;
			mermaidDanceTime = 5f;
		}
		else
		{
			switch (songIndex)
			{
			case 1:
				if (relative_pitch == -200)
				{
					songIndex++;
					mermaidDanceTime = 5f;
				}
				else
				{
					songIndex = -1;
					mermaidDanceTime = 0f;
					currentMermaidAnimation = mermaidIdle;
				}
				break;
			case 2:
				if (relative_pitch == -400)
				{
					songIndex++;
					mermaidDanceTime = 5f;
				}
				else
				{
					songIndex = -1;
					mermaidDanceTime = 0f;
					currentMermaidAnimation = mermaidIdle;
				}
				break;
			case 3:
				if (relative_pitch == 200)
				{
					songIndex = 0;
					mermaidPuzzleSuccess.Fire();
					mermaidDanceTime = 0f;
				}
				else
				{
					songIndex = -1;
					mermaidDanceTime = 0f;
					currentMermaidAnimation = mermaidIdle;
				}
				break;
			}
		}
		lastPlayedNote = pitch;
	}
}
