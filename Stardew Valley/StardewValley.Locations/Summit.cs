using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.Characters;
using StardewValley.GameData.FarmAnimals;
using StardewValley.GameData.Pets;
using StardewValley.Monsters;
using StardewValley.TokenizableStrings;

namespace StardewValley.Locations;

public class Summit : GameLocation
{
	private ICue wind;

	private float windGust;

	private float globalWind = -0.25f;

	[XmlIgnore]
	public bool isShowingEndSlideshow;

	public Summit()
	{
	}

	public Summit(string map, string name)
		: base(map, name)
	{
	}

	/// <inheritdoc />
	public override void checkForMusic(GameTime time)
	{
	}

	public override void UpdateWhenCurrentLocation(GameTime time)
	{
		if (Game1.random.NextDouble() < 0.005 || globalWind >= 1f || globalWind <= 0.35f)
		{
			if (globalWind < 0.35f)
			{
				windGust = (float)Game1.random.Next(3, 6) / 2000f;
			}
			else if (globalWind > 0.75f)
			{
				windGust = (float)(-Game1.random.Next(2, 6)) / 2000f;
			}
			else
			{
				windGust = (float)(Game1.random.Choose(-1, 1) * Game1.random.Next(4, 6)) / 2000f;
			}
		}
		if (wind != null)
		{
			globalWind += windGust;
			globalWind = Utility.Clamp(globalWind, -0.5f, 1f);
			wind.SetVariable("Volume", Math.Abs(globalWind) * 60f);
			wind.SetVariable("Frequency", globalWind * 100f);
			Game1.sounds.SetPitch(wind, 1200f + Math.Abs(globalWind) * 1200f);
		}
		if (Game1.background != null && Game1.background.cursed)
		{
			if (Game1.random.NextDouble() < 0.01)
			{
				Game1.playSound(Game1.random.Choose<string>("coin", "slimeHit", "squid_hit", "skeletonStep", "rabbit", "pig", "gulp"));
				if (Game1.options.screenFlash)
				{
					Game1.background.c = Utility.getBlendedColor(Utility.getRandomRainbowColor(), Color.Black);
				}
			}
			if (Game1.background.c.R > 0)
			{
				Game1.background.c.R--;
			}
			if (Game1.background.c.G > 0)
			{
				Game1.background.c.G--;
			}
			if (Game1.background.c.B > 0)
			{
				Game1.background.c.B--;
			}
		}
		base.UpdateWhenCurrentLocation(time);
		Season localSeason = GetSeason();
		if (currentEvent == null && Game1.background != null && !Game1.background.cursed && temporarySprites.Count == 0 && Game1.random.NextDouble() < ((Game1.timeOfDay < 1800) ? 0.0006 : ((Game1.season == Season.Summer && Game1.dayOfMonth == 20) ? 1.0 : 0.001)))
		{
			Rectangle sourceRect = Rectangle.Empty;
			Vector2 startingPosition = new Vector2(Game1.viewport.Width, Game1.random.Next(10, Game1.viewport.Height / 2));
			float speed = -4f;
			int loops = 200;
			float animationSpeed = 100f;
			if (Game1.timeOfDay < 1800)
			{
				switch (localSeason)
				{
				case Season.Spring:
				case Season.Fall:
				{
					sourceRect = new Rectangle(640, 736, 16, 16);
					int rows = Game1.random.Next(1, 4);
					speed = -1f;
					for (int i = 0; i < rows; i++)
					{
						TemporaryAnimatedSprite bird = new TemporaryAnimatedSprite("LooseSprites\\Cursors", sourceRect, Game1.random.Next(80, 121), 4, 200, startingPosition + new Vector2((i + 1) * Game1.random.Next(15, 18), (i + 1) * -20), flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
						{
							layerDepth = 0f
						};
						bird.motion = new Vector2(-1f, 0f);
						temporarySprites.Add(bird);
						bird = new TemporaryAnimatedSprite("LooseSprites\\Cursors", sourceRect, Game1.random.Next(80, 121), 4, 200, startingPosition + new Vector2((i + 1) * Game1.random.Next(15, 18), (i + 1) * 20), flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
						{
							layerDepth = 0f
						};
						bird.motion = new Vector2(-1f, 0f);
						temporarySprites.Add(bird);
					}
					break;
				}
				case Season.Summer:
					sourceRect = new Rectangle(640, 752 + Game1.random.Choose(16, 0), 16, 16);
					speed = -0.5f;
					animationSpeed = 150f;
					break;
				}
				if (Game1.random.NextDouble() < 0.25)
				{
					TemporaryAnimatedSprite bird = localSeason switch
					{
						Season.Spring => new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(0, 302, 26, 18), Game1.random.Next(80, 121), 4, 200, startingPosition, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
						{
							layerDepth = 0f,
							pingPong = true
						}, 
						Season.Summer => new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(1, 165, 24, 21), Game1.random.Next(60, 80), 6, 200, startingPosition, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
						{
							layerDepth = 0f
						}, 
						Season.Fall => new TemporaryAnimatedSprite("TileSheets\\critters", new Rectangle(0, 64, 32, 32), Game1.random.Next(60, 80), 5, 200, startingPosition, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
						{
							layerDepth = 0f,
							pingPong = true
						}, 
						Season.Winter => new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(104, 302, 26, 18), Game1.random.Next(80, 121), 4, 200, startingPosition, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
						{
							layerDepth = 0f,
							pingPong = true
						}, 
						_ => new TemporaryAnimatedSprite(), 
					};
					bird.motion = new Vector2(-3f, 0f);
					temporarySprites.Add(bird);
				}
				else if (Game1.random.NextDouble() < 0.15 && Game1.stats.Get("childrenTurnedToDoves") > 1)
				{
					for (int i = 0; i < Game1.stats.Get("childrenTurnedToDoves"); i++)
					{
						sourceRect = Rectangle.Empty;
						TemporaryAnimatedSprite bird = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(388, 1894, 24, 21), Game1.random.Next(80, 121), 6, 200, startingPosition + new Vector2((i + 1) * (Game1.random.Next(25, 27) * 4), Game1.random.Next(-32, 33) * 4), flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
						{
							layerDepth = 0f
						};
						bird.motion = new Vector2(-3f, 0f);
						temporarySprites.Add(bird);
					}
				}
				if (Game1.MasterPlayer.eventsSeen.Contains("571102") && Game1.random.NextDouble() < 0.1)
				{
					sourceRect = Rectangle.Empty;
					TemporaryAnimatedSprite bird = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(222, 1890, 20, 9), 30f, 2, 99900, startingPosition, flicker: false, flipped: false, 0.01f, 0f, Color.White, 2f, 0f, 0f, 0f, local: true)
					{
						yPeriodic = true,
						yPeriodicLoopTime = 4000f,
						yPeriodicRange = 8f,
						layerDepth = 0f
					};
					bird.motion = new Vector2(-3f, 0f);
					temporarySprites.Add(bird);
				}
				if (Game1.MasterPlayer.eventsSeen.Contains("10") && Game1.random.NextDouble() < 0.05)
				{
					sourceRect = Rectangle.Empty;
					TemporaryAnimatedSprite bird = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(206, 1827, 15, 25), 30f, 4, 99900, startingPosition, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
					{
						rotation = -(float)Math.PI / 3f,
						layerDepth = 0f
					};
					bird.motion = new Vector2(-4f, -0.5f);
					temporarySprites.Add(bird);
				}
			}
			else if (Game1.timeOfDay >= 1900)
			{
				sourceRect = new Rectangle(640, 816, 16, 16);
				speed = -2f;
				loops = 0;
				startingPosition.X -= Game1.random.Next(64, Game1.viewport.Width);
				if (localSeason == Season.Summer && Game1.dayOfMonth == 20)
				{
					int numExtra = Game1.random.Next(3);
					for (int i = 0; i < numExtra; i++)
					{
						TemporaryAnimatedSprite t = new TemporaryAnimatedSprite("LooseSprites\\Cursors", sourceRect, Game1.random.Next(80, 121), 4, loops, startingPosition, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
						{
							layerDepth = 0f
						};
						t.motion = new Vector2(speed, 0f);
						temporarySprites.Add(t);
						startingPosition.X -= Game1.random.Next(64, Game1.viewport.Width);
						startingPosition.Y = Game1.random.Next(0, 200);
					}
				}
				else if (Game1.season == Season.Winter)
				{
					if (Game1.timeOfDay >= 1700 && Game1.random.NextDouble() < 0.1)
					{
						sourceRect = new Rectangle(640, 800, 32, 16);
						loops = 1000;
						startingPosition.X = Game1.viewport.Width;
					}
					else
					{
						sourceRect = Rectangle.Empty;
					}
				}
			}
			if (Game1.timeOfDay >= 2200 && Game1.season == Season.Summer && Game1.dayOfMonth == 20 && Game1.random.NextDouble() < 0.05)
			{
				sourceRect = new Rectangle(640, 784, 16, 16);
				loops = 200;
				startingPosition.X = Game1.viewport.Width;
				speed = -3f;
			}
			if (sourceRect != Rectangle.Empty && Game1.viewport.X > -10000)
			{
				TemporaryAnimatedSprite t = new TemporaryAnimatedSprite("LooseSprites\\Cursors", sourceRect, animationSpeed, (localSeason == Season.Winter) ? 2 : 4, loops, startingPosition, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
				{
					layerDepth = 0f
				};
				t.motion = new Vector2(speed, 0f);
				temporarySprites.Add(t);
			}
		}
		if (Game1.viewport.X > -10000)
		{
			foreach (TemporaryAnimatedSprite temporarySprite in temporarySprites)
			{
				temporarySprite.position.Y -= ((float)Game1.viewport.Y - Game1.previousViewportPosition.Y) / 8f;
				temporarySprite.drawAboveAlwaysFront = true;
			}
		}
		if (Game1.eventUp)
		{
			foreach (TemporaryAnimatedSprite temporarySprite2 in temporarySprites)
			{
				temporarySprite2.attachedCharacter?.animateInFacingDirection(time);
			}
			return;
		}
		isShowingEndSlideshow = false;
	}

	public override void cleanupBeforePlayerExit()
	{
		isShowingEndSlideshow = false;
		base.cleanupBeforePlayerExit();
		Game1.background = null;
		Game1.displayHUD = true;
		wind?.Stop(AudioStopOptions.Immediate);
	}

	protected override void resetLocalState()
	{
		if (!Game1.player.team.farmPerfect.Value)
		{
			Game1.background = new Background(this);
			Game1.background.cursed = true;
			Game1.background.c = Color.Red;
			showQiCheatingEvent();
			return;
		}
		Game1.getAchievement(44);
		isShowingEndSlideshow = false;
		isOutdoors.Value = false;
		base.resetLocalState();
		Game1.background = new Background(this);
		temporarySprites.Clear();
		Game1.displayHUD = false;
		Game1.changeMusicTrack("winter_day_ambient", track_interruptable: true, MusicContext.SubLocation);
		Game1.playSound("wind", out wind);
		globalWind = 0f;
		windGust = 0.001f;
		if (!Game1.player.mailReceived.Contains("Summit_event") && Game1.MasterPlayer.mailReceived.Contains("Farm_Eternal"))
		{
			string eventScript = getSummitEvent();
			if (eventScript != "")
			{
				Game1.player.songsHeard.Add("end_credits");
				Game1.player.mailReceived.Add("Summit_event");
				startEvent(new Event(eventScript));
			}
		}
	}

	public string GetSummitDialogue(string file, string key)
	{
		NPC spouse = Game1.player.getSpouse();
		string asset_path = "Data\\" + file + ":" + key;
		if (!(spouse?.Name == "Penny"))
		{
			return Game1.content.LoadString(asset_path, "");
		}
		return Game1.content.LoadString(asset_path, "요");
	}

	private void showQiCheatingEvent()
	{
		StringBuilder sb = new StringBuilder();
		if (Game1.player.mailReceived.Contains("summit_cheat_event"))
		{
			Game1.player.health = -1;
			return;
		}
		sb.Append("winter_day_ambient/-1000 -1000/farmer 9 23 0 MrQi 11 13 0/viewport 11 13 clamp true/move farmer 0 -10 0/faceDirection MrQi 3/speak MrQi \"");
		sb.Append(Game1.content.LoadString("Strings\\1_6_Strings:QiSummitCheat") + "\"/faceDirection MrQi 0/pause 1000/playMusic none/pause 1000/speed MrQi 8/move MrQi -1 0 3/faceDirection farmer 2 true/animate farmer false true 100 94/startJittering/viewport -1000 -1000 true/end qiSummitCheat");
		startEvent(new Event(sb.ToString()));
		Game1.player.mailReceived.Add("summit_cheat_event");
	}

	private string getSummitEvent()
	{
		StringBuilder sb = new StringBuilder();
		try
		{
			sb.Append("winter_day_ambient/-1000 -1000/farmer 9 23 0 ");
			NPC spouse = Game1.player.getSpouse();
			if (spouse != null && spouse.Name != "Krobus")
			{
				string spouseName = spouse.Name;
				sb.Append(spouseName).Append(" 11 13 0/skippable/viewport 10 17 clamp true/pause 2000/viewport move 0 -1 4000/move farmer 0 -10 0/move farmer 1 0 0/pause 2000/speak ").Append(spouseName)
					.Append(" \"")
					.Append(GetSummitDialogue("ExtraDialogue", "SummitEvent_Intro_Spouse"))
					.Append("\"/viewport move 0 -1 4000/pause 5000/speak ")
					.Append(spouseName)
					.Append(" \"")
					.Append(GetSummitDialogue("ExtraDialogue", "SummitEvent_Intro2_Spouse" + (sayGrufferSummitIntro(spouse) ? "_Gruff" : "")))
					.Append("\"/pause 400/emote farmer 56/pause 2000/speak ")
					.Append(spouseName)
					.Append(" \"")
					.Append(GetSummitDialogue("ExtraDialogue", "SummitEvent_Dialogue1_Spouse"))
					.Append("\"/pause 1000/speak ")
					.Append(spouseName)
					.Append(" \"")
					.Append(GetSummitDialogue("ExtraDialogue", "SummitEvent_Dialogue1B_Spouse"))
					.Append("\"/pause 2000/faceDirection ")
					.Append(spouseName)
					.Append(" 3/faceDirection farmer 1/pause 1000/speak ")
					.Append(spouseName)
					.Append(" \"")
					.Append(GetSummitDialogue("ExtraDialogue", "SummitEvent_Dialogue2_Spouse"))
					.Append("\"/pause 2000/faceDirection ")
					.Append(spouseName)
					.Append(" 0/faceDirection farmer 0/pause 2000/speak ")
					.Append(spouseName)
					.Append(" \"")
					.Append(GetSummitDialogue("ExtraDialogue", "SummitEvent_Dialogue3_" + spouseName));
				if (!sb.ToString()[sb.Length - 1].Equals('"'))
				{
					sb.Append("\"");
				}
				sb.Append("/emote farmer 20/pause 500/faceDirection farmer 1/faceDirection ").Append(spouseName).Append(" 3/pause 1500/animate farmer false true 100 101/showKissFrame ")
					.Append(spouseName)
					.Append("/playSound dwop/positionOffset farmer 8 0/positionOffset ")
					.Append(spouseName)
					.Append(" -4 0/specificTemporarySprite heart 11 12/pause 10");
			}
			else if (Game1.MasterPlayer.mailReceived.Contains("JojaMember"))
			{
				sb.Append("Morris 11 13 0/skippable/viewport 10 17 clamp true/pause 2000/viewport move 0 -1 4000/move farmer 0 -10 0/pause 2000/speak Morris \"").Append(GetSummitDialogue("ExtraDialogue", "SummitEvent_Intro_Morris")).Append("\"/viewport move 0 -1 4000/pause 5000/speak Morris \"")
					.Append(GetSummitDialogue("ExtraDialogue", "SummitEvent_Dialogue1_Morris"))
					.Append("\"/pause 2000/faceDirection Morris 3/speak Morris \"")
					.Append(GetSummitDialogue("ExtraDialogue", "SummitEvent_Dialogue2_Morris"))
					.Append("\"/pause 2000/faceDirection Morris 0/speak Morris \"")
					.Append(GetSummitDialogue("ExtraDialogue", "SummitEvent_Outro_Morris"))
					.Append("\"/emote farmer 20/pause 10");
			}
			else
			{
				sb.Append("Lewis 11 13 0/skippable/viewport 10 17 clamp true/pause 2000/viewport move 0 -1 4000/move farmer 0 -10 0/pause 2000/speak Lewis \"").Append(GetSummitDialogue("ExtraDialogue", "SummitEvent_Intro_Lewis")).Append("\"/viewport move 0 -1 4000/pause 5000/speak Lewis \"")
					.Append(GetSummitDialogue("ExtraDialogue", "SummitEvent_Dialogue1_Lewis"))
					.Append("\"/pause 2000/faceDirection Lewis 3/speak Lewis \"")
					.Append(GetSummitDialogue("ExtraDialogue", "SummitEvent_Dialogue2_Lewis"))
					.Append("\"/pause 2000/faceDirection Lewis 0/speak Lewis \"")
					.Append(GetSummitDialogue("ExtraDialogue", "SummitEvent_Outro_Lewis"))
					.Append("\"/pause 10");
			}
			int pauseTime = 35000;
			if (Game1.player.mailReceived.Contains("Broken_Capsule"))
			{
				pauseTime += 8000;
			}
			if (Game1.player.totalMoneyEarned >= 100000000)
			{
				pauseTime += 8000;
			}
			if (Game1.year <= 2)
			{
				pauseTime += 8000;
			}
			sb.Append("/playMusic moonlightJellies/pause 2000/specificTemporarySprite krobusraven/viewport move 0 -1 12000/pause 10/pause ").Append(pauseTime).Append("/pause 2000/playMusic none/viewport move 0 -1 5000/fade/playMusic end_credits/viewport -8000 -8000 true/removeTemporarySprites/specificTemporarySprite getEndSlideshow/pause 1000/playMusic none/pause 500")
				.Append("/playMusic grandpas_theme/pause 2000/fade/viewport -3000 -2000/specificTemporarySprite doneWithSlideShow/removeTemporarySprites/pause 3000/addTemporaryActor MrQi 16 32 -998 -1000 2 true/addTemporaryActor Grandpa 1 1 -100 -100 2 true/specificTemporarySprite grandpaSpirit/viewport -1000 -1000 true/pause 6000/spriteText 3 \"")
				.Append(GetSummitDialogue("ExtraDialogue", "SummitEvent_closingmessage"))
				.Append(" \"/spriteText 3 \"")
				.Append(GetSummitDialogue("ExtraDialogue", "SummitEvent_closingmessage2"))
				.Append(" \"/spriteText 3 \"")
				.Append(GetSummitDialogue("ExtraDialogue", "SummitEvent_closingmessage3"))
				.Append(" \"/spriteText 3 \"")
				.Append(GetSummitDialogue("ExtraDialogue", "SummitEvent_closingmessage4"))
				.Append(" \"/spriteText 7 \"")
				.Append(GetSummitDialogue("ExtraDialogue", "SummitEvent_closingmessage5"))
				.Append(" \"/pause 400/playSound dwop/showFrame MrQi 1/pause 100/showFrame MrQi 2/pause 100/showFrame MrQi 3/pause 400/specificTemporarySprite grandpaThumbsUp/pause 10000/end");
		}
		catch (Exception)
		{
			return "";
		}
		return sb.ToString();
	}

	public string getEndSlideshow()
	{
		StringBuilder sb = new StringBuilder();
		int i = 0;
		foreach (KeyValuePair<string, CharacterData> entry in Game1.characterData)
		{
			string name = entry.Key;
			CharacterData data = entry.Value;
			if (data.EndSlideShow == EndSlideShowBehavior.MainGroup && TryDrawNpc(name, data, 90, i))
			{
				i += 500;
			}
		}
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\night_market_tilesheet_objects", new Rectangle(586, 119, 122, 28), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 392f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-3f, 0f),
			delayBeforeAnimationStart = 2000
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\night_market_tilesheet_objects", new Rectangle(586, 119, 122, 28), 900f, 1, 999999, new Vector2(Game1.viewport.Width + 488, (float)Game1.viewport.Height * 0.5f - 392f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-3f, 0f),
			delayBeforeAnimationStart = 2000
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\night_market_tilesheet_objects", new Rectangle(586, 119, 122, 28), 900f, 1, 999999, new Vector2(Game1.viewport.Width + 976, (float)Game1.viewport.Height * 0.5f - 392f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-3f, 0f),
			delayBeforeAnimationStart = 2000
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\night_market_tilesheet_objects", new Rectangle(586, 119, 122, 28), 900f, 1, 999999, new Vector2(Game1.viewport.Width + 1464, (float)Game1.viewport.Height * 0.5f - 392f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-3f, 0f),
			delayBeforeAnimationStart = 2000
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(324, 1936, 12, 20), 90f, 4, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.4f + 192f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-4f, 0f),
			delayBeforeAnimationStart = 14000,
			startSound = "dogWhining"
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(43, 80, 51, 56), 90f, 1, 999999, new Vector2(Game1.viewport.Width / 2, Game1.viewport.Height), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-1f, -4f),
			delayBeforeAnimationStart = 27000,
			startSound = "trashbear",
			drawAboveAlwaysFront = true
		});
		sb.Append("pause 10/spriteText 5 \"").Append(Utility.loadStringShort("UI", "EndCredit_Neighbors")).Append(" \"/pause 30000/");
		i += 4000;
		int oldTime = i;
		foreach (KeyValuePair<string, CharacterData> entry in Game1.characterData)
		{
			string name = entry.Key;
			CharacterData data = entry.Value;
			if (data.EndSlideShow == EndSlideShowBehavior.TrailingGroup && TryDrawNpc(name, data, 120, i))
			{
				i += 500;
			}
		}
		i += 5000;
		sb.Append("spriteText 4 \"").Append(Utility.loadStringShort("UI", "EndCredit_Animals")).Append(" \"/pause ")
			.Append(i - oldTime + 22000);
		oldTime = i;
		foreach (KeyValuePair<string, FarmAnimalData> pair in Game1.farmAnimalData)
		{
			string id = pair.Key;
			FarmAnimalData animal = pair.Value;
			if (!animal.ShowInSummitCredits)
			{
				continue;
			}
			int width = animal.SpriteWidth;
			int height = animal.SpriteHeight;
			int animalWidth = 0;
			base.TemporarySprites.Add(new TemporaryAnimatedSprite(animal.Texture, new Rectangle(0, height, width, height), 120f, 4, 999999, new Vector2(Game1.viewport.Width, (int)((float)Game1.viewport.Height * 0.5f - (float)(height * 4))), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = i
			});
			animalWidth += width * 4;
			int extra = ((width > 16) ? 4 : 0);
			if (animal.BabyTexture != null && animal.BabyTexture != animal.Texture)
			{
				for (int babyCount = 1; babyCount <= 2; babyCount++)
				{
					base.TemporarySprites.Add(new TemporaryAnimatedSprite(animal.BabyTexture, new Rectangle(0, height, width, height), 90f, 4, 999999, new Vector2(Game1.viewport.Width + (width + 2 + extra) * babyCount * 4, (int)((float)Game1.viewport.Height * 0.5f - (float)(height * 4))), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
					{
						motion = new Vector2(-3f, 0f),
						delayBeforeAnimationStart = i
					});
				}
				animalWidth += (width + 2 + extra) * 4 * 2;
			}
			string displayName = TokenParser.ParseText(animal.DisplayName) ?? id;
			float displayNameWidth = Game1.dialogueFont.MeasureString(displayName).X;
			base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(0, height, width, height), 120f, 1, 999999, new Vector2((float)(Game1.viewport.Width + animalWidth / 2) - displayNameWidth / 2f, (int)((float)Game1.viewport.Height * 0.5f + 12f)), flicker: false, flipped: true, 0.9f, 0f, Color.White, 1f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = i,
				text = displayName
			});
			i += 2000 + extra * 300;
		}
		int pet_i = 0;
		foreach (Pet p in Utility.getAllPets())
		{
			if (Pet.TryGetData(p.petType.Value, out var petData) && petData.SummitPerfectionEvent != null)
			{
				PetBreed breed = petData.GetBreedById(p.whichBreed.Value);
				PetSummitPerfectionEventData summitData = petData.SummitPerfectionEvent;
				base.TemporarySprites.Add(new TemporaryAnimatedSprite(breed.Texture, summitData.SourceRect, 90f, summitData.AnimationLength, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 320f + (float)(p.petType.Value.Equals("Dog") ? 96 : 0)), flicker: false, summitData.Flipped, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
				{
					motion = summitData.Motion,
					delayBeforeAnimationStart = 38000 + pet_i * 400,
					startSound = petData.BarkSound,
					pingPong = summitData.PingPong
				});
				pet_i++;
			}
			if (pet_i >= 20)
			{
				break;
			}
		}
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(64, 192, 32, 32), 90f, 6, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 128f), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-4f, 0f),
			delayBeforeAnimationStart = 45000
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(128, 160, 32, 32), 90f, 6, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 128f), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-4f, 0f),
			delayBeforeAnimationStart = 47000
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(128, 224, 32, 32), 90f, 6, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 128f), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-4f, 0f),
			delayBeforeAnimationStart = 48000
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(32, 160, 32, 32), 90f, 3, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 320f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-4f, 0f),
			delayBeforeAnimationStart = 49000
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(32, 160, 32, 32), 90f, 3, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 288f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-4f, 0f),
			delayBeforeAnimationStart = 49500,
			pingPong = true
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(34, 98, 32, 32), 90f, 3, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 352f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-4f, 0f),
			delayBeforeAnimationStart = 50000,
			pingPong = true
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(0, 32, 32, 32), 90f, 4, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 352f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-4f, 0f),
			delayBeforeAnimationStart = 50500,
			pingPong = true
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(128, 96, 16, 16), 90f, 4, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 352f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-4f, 0f),
			delayBeforeAnimationStart = 55000,
			pingPong = true,
			yPeriodic = true,
			yPeriodicRange = 8f,
			yPeriodicLoopTime = 3000f
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(192, 96, 16, 16), 90f, 4, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 358.4f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-4f, 0f),
			delayBeforeAnimationStart = 55300,
			pingPong = true,
			yPeriodic = true,
			yPeriodicRange = 8f,
			yPeriodicLoopTime = 3000f
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(256, 96, 16, 16), 90f, 4, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 345.6f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-4f, 0f),
			delayBeforeAnimationStart = 55600,
			pingPong = true,
			yPeriodic = true,
			yPeriodicRange = 8f,
			yPeriodicLoopTime = 3000f
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(0, 128, 16, 16), 90f, 3, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 352f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-4f, 0f),
			delayBeforeAnimationStart = 57000,
			pingPong = true,
			yPeriodic = true,
			yPeriodicRange = 8f,
			yPeriodicLoopTime = 3000f
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(48, 144, 16, 16), 90f, 3, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 358.4f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-4f, 0f),
			delayBeforeAnimationStart = 57300,
			pingPong = true,
			yPeriodic = true,
			yPeriodicRange = 8f,
			yPeriodicLoopTime = 3000f
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(96, 144, 16, 16), 90f, 3, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 345.6f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-4f, 0f),
			delayBeforeAnimationStart = 57600,
			pingPong = true,
			yPeriodic = true,
			yPeriodicRange = 8f,
			yPeriodicLoopTime = 3000f
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(192, 288, 16, 16), 90f, 4, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 345.6f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-4f, 0f),
			delayBeforeAnimationStart = 58000,
			pingPong = true,
			yPeriodic = true,
			yPeriodicRange = 8f,
			yPeriodicLoopTime = 3000f
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(128, 288, 16, 16), 90f, 4, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 358.4f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-4f, 0f),
			delayBeforeAnimationStart = 58300,
			pingPong = true,
			yPeriodic = true,
			yPeriodicRange = 8f,
			yPeriodicLoopTime = 3000f
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(0, 224, 16, 16), 90f, 5, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 64f), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-3f, 0f),
			delayBeforeAnimationStart = 54000
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(0, 240, 16, 16), 90f, 5, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 64f), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-3f, 0f),
			delayBeforeAnimationStart = 55000
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\temporary_sprites_1", new Rectangle(67, 190, 24, 51), 90f, 3, 999999, new Vector2(Game1.viewport.Width / 2, Game1.viewport.Height), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-3f, -4f),
			delayBeforeAnimationStart = 68000,
			rotation = -(float)Math.PI / 16f,
			pingPong = true,
			drawAboveAlwaysFront = true
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\temporary_sprites_1", new Rectangle(0, 0, 57, 70), 150f, 2, 999999, new Vector2(Game1.viewport.Width / 2, Game1.viewport.Height), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-3f, -4f),
			delayBeforeAnimationStart = 69000,
			rotation = -(float)Math.PI / 16f,
			drawAboveAlwaysFront = true
		});
		sb.Append("/spriteText 1 \"").Append(Utility.loadStringShort("UI", "EndCredit_Fish")).Append(" \"/pause ")
			.Append(i - oldTime + 18000);
		i += 6000;
		oldTime = i;
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\temporary_sprites_1", new Rectangle(257, 98, 182, 18), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 72f), flicker: false, flipped: true, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-3f, 0f),
			delayBeforeAnimationStart = 70000
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\temporary_sprites_1", new Rectangle(257, 98, 182, 18), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 72f), flicker: false, flipped: true, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-3f, 0f),
			delayBeforeAnimationStart = 86000
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\temporary_sprites_1", new Rectangle(257, 98, 182, 18), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 72f), flicker: false, flipped: true, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-3f, 0f),
			delayBeforeAnimationStart = 91000
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\temporary_sprites_1", new Rectangle(140, 78, 28, 38), 250f, 2, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 152f), flicker: false, flipped: true, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-3f, 0f),
			delayBeforeAnimationStart = 102000
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\temporary_sprites_1", new Rectangle(257, 98, 182, 18), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 72f), flicker: false, flipped: true, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-3f, 0f),
			delayBeforeAnimationStart = 75000
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\AquariumFish", new Rectangle(0, 287, 47, 14), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 56f), flicker: false, flipped: true, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-3f, 0f),
			delayBeforeAnimationStart = 82000
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\AquariumFish", new Rectangle(0, 287, 47, 14), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 56f), flicker: false, flipped: true, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-3f, 0f),
			delayBeforeAnimationStart = 80000
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\AquariumFish", new Rectangle(0, 287, 47, 14), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 56f), flicker: false, flipped: true, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-3f, 0f),
			delayBeforeAnimationStart = 84000
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\temporary_sprites_1", new Rectangle(132, 20, 8, 8), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 48f), flicker: false, flipped: true, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-3f, 0f),
			delayBeforeAnimationStart = 81500,
			yPeriodic = true,
			yPeriodicRange = 21f,
			yPeriodicLoopTime = 5000f
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\temporary_sprites_1", new Rectangle(140, 20, 8, 8), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 48f), flicker: false, flipped: true, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-3f, 0f),
			delayBeforeAnimationStart = 83500,
			yPeriodic = true,
			yPeriodicRange = 21f,
			yPeriodicLoopTime = 5000f
		});
		Dictionary<string, string> dictionary = DataLoader.Fish(Game1.content);
		Dictionary<string, string> aquariumData = DataLoader.AquariumFish(Game1.content);
		int stack = 0;
		foreach (KeyValuePair<string, string> v in dictionary)
		{
			try
			{
				string fishId = v.Key;
				if (aquariumData.TryGetValue(fishId, out var rawAquariumEntry))
				{
					string fishName = ItemRegistry.GetData("(O)" + fishId)?.DisplayName ?? ArgUtility.SplitBySpaceAndGet(v.Value, 0);
					string[] array = rawAquariumEntry.Split('/');
					string aquariumTextureName = ArgUtility.Get(array, 6, "LooseSprites\\AquariumFish", allowBlank: false);
					int aquariumIndex = ArgUtility.GetInt(array, 0);
					Rectangle source = new Rectangle(24 * aquariumIndex % 480, 24 * aquariumIndex / 480 * 48, 24, 24);
					float textWidth = Game1.dialogueFont.MeasureString(fishName).X;
					base.TemporarySprites.Add(new TemporaryAnimatedSprite(aquariumTextureName, source, 9999f, 1, 999999, new Vector2(Game1.viewport.Width + 192, (int)((float)Game1.viewport.Height * 0.53f - (float)(stack * 64) * 2f)), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
					{
						motion = new Vector2(-3f, 0f),
						delayBeforeAnimationStart = i,
						yPeriodic = true,
						yPeriodicLoopTime = Game1.random.Next(1500, 2100),
						yPeriodicRange = 4f
					});
					base.TemporarySprites.Add(new TemporaryAnimatedSprite(aquariumTextureName, source, 9999f, 1, 999999, new Vector2((float)(Game1.viewport.Width + 192 + 48) - textWidth / 2f, (int)((float)Game1.viewport.Height * 0.53f - (float)(stack * 64) * 2f + 64f + 16f)), flicker: false, flipped: true, 0.9f, 0f, Color.White, 1f, 0f, 0f, 0f, local: true)
					{
						motion = new Vector2(-3f, 0f),
						delayBeforeAnimationStart = i,
						text = fishName
					});
					stack++;
					if (stack == 4)
					{
						i += 2000;
						stack = 0;
					}
				}
			}
			catch (Exception ex)
			{
				Game1.log.Error("Couldn't add fish '" + v.Key + "' to summit event credits.", ex);
			}
		}
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\projectiles", new Rectangle(64, 0, 16, 16), 909f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 352f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-6f, 0f),
			delayBeforeAnimationStart = 123000,
			rotationChange = -0.1f
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\projectiles", new Rectangle(64, 0, 16, 16), 909f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 339.2f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-6f, 0f),
			delayBeforeAnimationStart = 123300,
			rotationChange = -0.1f
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(0, 1452, 640, 69), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 392f), flicker: false, flipped: false, 0.2f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-3f, 0f),
			delayBeforeAnimationStart = 108000
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(0, 1452, 640, 69), 900f, 1, 999999, new Vector2(Game1.viewport.Width + 2564, (float)Game1.viewport.Height * 0.5f - 392f), flicker: false, flipped: false, 0.2f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-3f, 0f),
			delayBeforeAnimationStart = 108000
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(0, 1452, 640, 69), 900f, 1, 999999, new Vector2(Game1.viewport.Width + 5128, (float)Game1.viewport.Height * 0.5f - 392f), flicker: false, flipped: false, 0.2f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-3f, 0f),
			delayBeforeAnimationStart = 108000
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(0, 1452, 300, 69), 900f, 1, 999999, new Vector2(Game1.viewport.Width + 7692, (float)Game1.viewport.Height * 0.5f - 392f), flicker: false, flipped: false, 0.2f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-3f, 0f),
			delayBeforeAnimationStart = 108000
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\bushes", new Rectangle(0, 0, 31, 29), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 116f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-3f, 0f),
			delayBeforeAnimationStart = 110000
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\bushes", new Rectangle(65, 0, 31, 29), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 116f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-3f, 0f),
			delayBeforeAnimationStart = 115000
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\bushes", new Rectangle(96, 90, 31, 29), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 116f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-3f, 0f),
			delayBeforeAnimationStart = 118000
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\bushes", new Rectangle(0, 176, 104, 29), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 116f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-3f, 0f),
			delayBeforeAnimationStart = 121000
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\bushes", new Rectangle(32, 320, 32, 23), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 92f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-3f, 0f),
			delayBeforeAnimationStart = 124000
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\bushes", new Rectangle(31, 58, 67, 23), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 92f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-3f, 0f),
			delayBeforeAnimationStart = 127000
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\bushes", new Rectangle(0, 98, 32, 23), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 92f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-3f, 0f),
			delayBeforeAnimationStart = 132000
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\bushes", new Rectangle(49, 131, 47, 29), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 116f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-3f, 0f),
			delayBeforeAnimationStart = 137000
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("TerrainFeatures\\grass", new Rectangle(0, 0, 44, 13), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 52f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-3f, 0f),
			delayBeforeAnimationStart = 113000
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("TerrainFeatures\\grass", new Rectangle(0, 20, 44, 13), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 52f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-3f, 0f),
			delayBeforeAnimationStart = 116000
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("TerrainFeatures\\grass", new Rectangle(0, 40, 44, 13), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 52f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-3f, 0f),
			delayBeforeAnimationStart = 119000
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("TerrainFeatures\\grass", new Rectangle(0, 60, 44, 13), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 52f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-3f, 0f),
			delayBeforeAnimationStart = 126000
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("TerrainFeatures\\grass", new Rectangle(0, 120, 44, 13), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 52f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-3f, 0f),
			delayBeforeAnimationStart = 129000
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("TerrainFeatures\\grass", new Rectangle(0, 100, 44, 13), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 52f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-3f, 0f),
			delayBeforeAnimationStart = 134000
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("TerrainFeatures\\grass", new Rectangle(0, 120, 44, 13), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 52f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-3f, 0f),
			delayBeforeAnimationStart = 139000
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("TerrainFeatures\\upperCavePlants", new Rectangle(0, 0, 48, 21), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 84f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-3f, 0f),
			delayBeforeAnimationStart = 142000
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("TerrainFeatures\\upperCavePlants", new Rectangle(96, 0, 48, 21), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 84f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-3f, 0f),
			delayBeforeAnimationStart = 146000
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\temporary_sprites_1", new Rectangle(2, 123, 19, 24), 90f, 4, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 352f), flicker: false, flipped: true, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-4f, 0f),
			delayBeforeAnimationStart = 145000,
			yPeriodic = true,
			yPeriodicRange = 8f,
			yPeriodicLoopTime = 2500f
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\temporary_sprites_1", new Rectangle(2, 123, 19, 24), 100f, 4, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 358.4f), flicker: false, flipped: true, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-4f, 0f),
			delayBeforeAnimationStart = 142500,
			yPeriodic = true,
			yPeriodicRange = 8f,
			yPeriodicLoopTime = 2000f
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\bushes", new Rectangle(0, 0, 31, 29), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 116f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-3f, 0f),
			delayBeforeAnimationStart = 149000
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\bushes", new Rectangle(65, 0, 31, 29), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 116f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-3f, 0f),
			delayBeforeAnimationStart = 151000
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\bushes", new Rectangle(96, 90, 31, 29), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 116f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-3f, 0f),
			delayBeforeAnimationStart = 154000
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\bushes", new Rectangle(0, 176, 104, 29), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 116f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-3f, 0f),
			delayBeforeAnimationStart = 156000
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("TerrainFeatures\\grass", new Rectangle(0, 0, 44, 13), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 52f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-3f, 0f),
			delayBeforeAnimationStart = 155000
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("TerrainFeatures\\grass", new Rectangle(0, 20, 44, 13), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 52f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-3f, 0f),
			delayBeforeAnimationStart = 152500
		});
		base.TemporarySprites.Add(new TemporaryAnimatedSprite("TerrainFeatures\\grass", new Rectangle(0, 40, 44, 13), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 52f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			motion = new Vector2(-3f, 0f),
			delayBeforeAnimationStart = 158000
		});
		if (Game1.player.favoriteThing.Value != null && Game1.player.favoriteThing.Value.ToLower().Equals("concernedape"))
		{
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("Minigames\\Clouds", new Rectangle(210, 842, 138, 130), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 240f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 160000,
				startSound = "discoverMineral"
			});
		}
		if (!Utility.hasFinishedJojaRoute() && Game1.netWorldState.Value.PerfectionWaivers == 0 && !Game1.netWorldState.Value.ActivatedGoldenParrot)
		{
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("Characters\\Morris", new Rectangle(48, 128, 16, 32), 9900f, 1, 999999, new Vector2(Game1.viewport.Width, Game1.viewport.Height), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-7f, -4f),
				delayBeforeAnimationStart = 168500,
				startSound = "slimeHit",
				rotationChange = 0.05f
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite
			{
				text = Game1.content.LoadString("Strings\\1_6_Strings:JojaFreeRun"),
				color = Color.Lime,
				position = new Vector2(Game1.viewport.Width, Game1.viewport.Height) / 2f,
				interval = 3000f,
				totalNumberOfLoops = 1,
				animationLength = 1,
				delayBeforeAnimationStart = 169500,
				layerDepth = 0.71f,
				local = true
			});
		}
		sb.Append("/spriteText 2 \"").Append(Utility.loadStringShort("UI", "EndCredit_Monsters")).Append(" \"/pause ")
			.Append(i - oldTime + 19000);
		i += 6000;
		oldTime = i;
		foreach (KeyValuePair<string, string> v in DataLoader.Monsters(Game1.content))
		{
			if (v.Key == "Fireball" || v.Key == "Skeleton Warrior")
			{
				continue;
			}
			int spriteHeight = 16;
			int spriteWidth = 16;
			int spriteStartIndex = 0;
			int animationLength = 4;
			bool pingpong = false;
			int yOffset = 0;
			Character attachedCharacter = null;
			if (v.Key.Contains("Bat") || v.Key.Contains("Ghost"))
			{
				spriteHeight = 24;
			}
			switch (v.Key)
			{
			case "Grub":
			case "Rock Crab":
			case "Lava Crab":
			case "Iridium Crab":
			case "Stone Golem":
			case "Fly":
			case "Duggy":
			case "Magma Duggy":
			case "Wilderness Golem":
				spriteHeight = 24;
				spriteStartIndex = 4;
				break;
			case "False Magma Cap":
			case "Dust Spirit":
				spriteHeight = 24;
				spriteStartIndex = 0;
				break;
			case "Pepper Rex":
				spriteWidth = 32;
				spriteHeight = 32;
				break;
			case "Lava Lurk":
				spriteStartIndex = 4;
				pingpong = true;
				break;
			case "Magma Sparker":
			case "Magma Sprite":
				animationLength = 7;
				spriteStartIndex = 7;
				break;
			case "Big Slime":
				spriteHeight = 32;
				spriteWidth = 32;
				yOffset = 64;
				attachedCharacter = new BigSlime(Vector2.Zero, 0);
				break;
			case "Blue Squid":
				spriteWidth = 24;
				spriteHeight = 24;
				animationLength = 5;
				break;
			case "Spider":
				spriteWidth = 32;
				spriteHeight = 32;
				animationLength = 2;
				break;
			case "Serpent":
				spriteWidth = 32;
				spriteHeight = 32;
				animationLength = 5;
				break;
			case "Spiker":
			case "Carbon Ghost":
			case "Ghost":
			case "Dwarvish Sentry":
			case "Putrid Ghost":
				animationLength = 1;
				break;
			case "Skeleton":
			case "Mummy":
			case "Skeleton Mage":
				spriteHeight = 32;
				spriteStartIndex = 4;
				break;
			case "Shadow Guy":
			{
				spriteHeight = 32;
				spriteStartIndex = 4;
				Texture2D t = Game1.content.Load<Texture2D>("Characters\\Monsters\\Shadow Brute");
				base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(spriteWidth * spriteStartIndex % t.Width, spriteWidth * spriteStartIndex / t.Width * spriteHeight, spriteWidth, spriteHeight), 100f, animationLength, 999999, new Vector2(Game1.viewport.Width + 192, (float)Game1.viewport.Height * 0.5f - (float)(spriteHeight * 4) - 16f), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
				{
					motion = new Vector2(-3f, 0f),
					delayBeforeAnimationStart = i,
					yPeriodic = (animationLength == 1),
					yPeriodicRange = 16f,
					yPeriodicLoopTime = 3000f,
					attachedCharacter = attachedCharacter,
					texture = t
				});
				t = Game1.content.Load<Texture2D>("Characters\\Monsters\\Shadow Shaman");
				spriteHeight = 24;
				base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(spriteWidth * spriteStartIndex % t.Width, spriteWidth * spriteStartIndex / t.Width * spriteHeight, spriteWidth, spriteHeight), 100f, animationLength, 999999, new Vector2((float)Game1.viewport.Width + 96f, (float)Game1.viewport.Height * 0.5f - (float)(spriteHeight * 4) - 16f), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
				{
					motion = new Vector2(-3f, 0f),
					delayBeforeAnimationStart = i,
					yPeriodic = (animationLength == 1),
					yPeriodicRange = 16f,
					yPeriodicLoopTime = 3000f,
					attachedCharacter = attachedCharacter,
					texture = t
				});
				t = Game1.content.Load<Texture2D>("Characters\\Monsters\\Shadow Sniper");
				spriteHeight = 32;
				spriteWidth = 32;
				base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(spriteWidth * spriteStartIndex % t.Width, spriteWidth * spriteStartIndex / t.Width * spriteHeight, spriteWidth, spriteHeight), 100f, animationLength, 999999, new Vector2((float)Game1.viewport.Width + 288f, (float)Game1.viewport.Height * 0.5f - (float)(spriteHeight * 4) - 16f), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
				{
					motion = new Vector2(-3f, 0f),
					delayBeforeAnimationStart = i,
					yPeriodic = (animationLength == 1),
					yPeriodicRange = 16f,
					yPeriodicLoopTime = 3000f,
					attachedCharacter = attachedCharacter,
					texture = t
				});
				base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(spriteWidth * spriteStartIndex % t.Width, spriteWidth * spriteStartIndex / t.Width * spriteHeight, spriteWidth, spriteHeight), 100f, animationLength, 999999, new Vector2((float)(Game1.viewport.Width + 128 + spriteWidth * 4 / 2) - Game1.dialogueFont.MeasureString(v.Value.Split('/')[14]).X / 2f, (float)Game1.viewport.Height * 0.5f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 1f, 0f, 0f, 0f, local: true)
				{
					motion = new Vector2(-3f, 0f),
					delayBeforeAnimationStart = i,
					text = Utility.loadStringShort("UI", "EndCredit_ShadowPeople")
				});
				i += 1500;
				continue;
			}
			case "Bat":
			{
				Texture2D t = Game1.content.Load<Texture2D>("Characters\\Monsters\\Frost Bat");
				base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(spriteWidth * spriteStartIndex % t.Width, spriteWidth * spriteStartIndex / t.Width * spriteHeight, spriteWidth, spriteHeight), 100f, animationLength, 999999, new Vector2(Game1.viewport.Width + 192, (float)Game1.viewport.Height * 0.5f - (float)(spriteHeight * 4) - 16f), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
				{
					motion = new Vector2(-3f, 0f),
					delayBeforeAnimationStart = i,
					yPeriodic = (animationLength == 1),
					yPeriodicRange = 16f,
					yPeriodicLoopTime = 3000f,
					attachedCharacter = attachedCharacter,
					texture = t
				});
				t = Game1.content.Load<Texture2D>("Characters\\Monsters\\Lava Bat");
				spriteHeight = 24;
				base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(spriteWidth * spriteStartIndex % t.Width, spriteWidth * spriteStartIndex / t.Width * spriteHeight, spriteWidth, spriteHeight), 100f, animationLength, 999999, new Vector2((float)Game1.viewport.Width + 96f, (float)Game1.viewport.Height * 0.5f - (float)(spriteHeight * 4) - 16f), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
				{
					motion = new Vector2(-3f, 0f),
					delayBeforeAnimationStart = i,
					yPeriodic = (animationLength == 1),
					yPeriodicRange = 16f,
					yPeriodicLoopTime = 3000f,
					attachedCharacter = attachedCharacter,
					texture = t
				});
				t = Game1.content.Load<Texture2D>("Characters\\Monsters\\Iridium Bat");
				base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(spriteWidth * spriteStartIndex % t.Width, spriteWidth * spriteStartIndex / t.Width * spriteHeight, spriteWidth, spriteHeight), 100f, animationLength, 999999, new Vector2((float)Game1.viewport.Width + 288f, (float)Game1.viewport.Height * 0.5f - (float)(spriteHeight * 4) - 16f), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
				{
					motion = new Vector2(-3f, 0f),
					delayBeforeAnimationStart = i,
					yPeriodic = (animationLength == 1),
					yPeriodicRange = 16f,
					yPeriodicLoopTime = 3000f,
					attachedCharacter = attachedCharacter,
					texture = t
				});
				base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(spriteWidth * spriteStartIndex % t.Width, spriteWidth * spriteStartIndex / t.Width * spriteHeight, spriteWidth, spriteHeight), 100f, animationLength, 999999, new Vector2((float)(Game1.viewport.Width + 128 + spriteWidth * 4 / 2) - Game1.dialogueFont.MeasureString(v.Value.Split('/')[14]).X / 2f, (float)Game1.viewport.Height * 0.5f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 1f, 0f, 0f, 0f, local: true)
				{
					motion = new Vector2(-3f, 0f),
					delayBeforeAnimationStart = i,
					text = Utility.loadStringShort("UI", "EndCredit_Bats")
				});
				i += 1500;
				continue;
			}
			case "Green Slime":
			{
				Texture2D t = null;
				if (attachedCharacter == null)
				{
					t = Game1.content.Load<Texture2D>("Characters\\Monsters\\Green Slime");
				}
				spriteHeight = 32;
				spriteStartIndex = 4;
				GreenSlime slime = new GreenSlime(Vector2.Zero, 0);
				base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(spriteWidth * spriteStartIndex % t.Width, spriteWidth * spriteStartIndex / t.Width * spriteHeight, spriteWidth, spriteHeight), 100f, animationLength, 999999, new Vector2(Game1.viewport.Width + 192 - 64, (float)Game1.viewport.Height * 0.5f - (float)(spriteHeight * 4) + 32f), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
				{
					motion = new Vector2(-3f, 0f),
					delayBeforeAnimationStart = i,
					yPeriodic = (animationLength == 1),
					yPeriodicRange = 16f,
					yPeriodicLoopTime = 3000f,
					attachedCharacter = slime,
					texture = null
				});
				slime = new GreenSlime(Vector2.Zero, 41);
				base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(spriteWidth * spriteStartIndex % t.Width, spriteWidth * spriteStartIndex / t.Width * spriteHeight, spriteWidth, spriteHeight), 100f, animationLength, 999999, new Vector2((float)Game1.viewport.Width + 96f - 64f, (float)Game1.viewport.Height * 0.5f - (float)(spriteHeight * 4) + 32f), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
				{
					motion = new Vector2(-3f, 0f),
					delayBeforeAnimationStart = i,
					yPeriodic = (animationLength == 1),
					yPeriodicRange = 16f,
					yPeriodicLoopTime = 3000f,
					attachedCharacter = slime,
					texture = null
				});
				slime = new GreenSlime(Vector2.Zero, 81);
				base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(spriteWidth * spriteStartIndex % t.Width, spriteWidth * spriteStartIndex / t.Width * spriteHeight, spriteWidth, spriteHeight), 100f, animationLength, 999999, new Vector2((float)Game1.viewport.Width + 288f - 64f, (float)Game1.viewport.Height * 0.5f - (float)(spriteHeight * 4) + 32f), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
				{
					motion = new Vector2(-3f, 0f),
					delayBeforeAnimationStart = i,
					yPeriodic = (animationLength == 1),
					yPeriodicRange = 16f,
					yPeriodicLoopTime = 3000f,
					attachedCharacter = slime,
					texture = null
				});
				slime = new GreenSlime(Vector2.Zero, 121);
				base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(spriteWidth * spriteStartIndex % t.Width, spriteWidth * spriteStartIndex / t.Width * spriteHeight, spriteWidth, spriteHeight), 100f, animationLength, 999999, new Vector2((float)Game1.viewport.Width + 240f - 64f, (float)Game1.viewport.Height * 0.5f - (float)(spriteHeight * 4 * 2) + 32f), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
				{
					motion = new Vector2(-3f, 0f),
					delayBeforeAnimationStart = i,
					yPeriodic = (animationLength == 1),
					yPeriodicRange = 16f,
					yPeriodicLoopTime = 3000f,
					attachedCharacter = slime,
					texture = null
				});
				slime = new GreenSlime(Vector2.Zero, 0);
				slime.makeTigerSlime();
				base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(spriteWidth * spriteStartIndex % t.Width, spriteWidth * spriteStartIndex / t.Width * spriteHeight, spriteWidth, spriteHeight), 100f, animationLength, 999999, new Vector2((float)Game1.viewport.Width + 144f - 64f, (float)Game1.viewport.Height * 0.5f - (float)(spriteHeight * 4 * 2) + 32f), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
				{
					motion = new Vector2(-3f, 0f),
					delayBeforeAnimationStart = i,
					yPeriodic = (animationLength == 1),
					yPeriodicRange = 16f,
					yPeriodicLoopTime = 3000f,
					attachedCharacter = slime,
					texture = null
				});
				base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(spriteWidth * spriteStartIndex % t.Width, spriteWidth * spriteStartIndex / t.Width * spriteHeight, spriteWidth, spriteHeight), 100f, animationLength, 999999, new Vector2((float)(Game1.viewport.Width + 192 + spriteWidth * 4 / 2) - Game1.dialogueFont.MeasureString(v.Value.Split('/')[14]).X / 2f, (float)Game1.viewport.Height * 0.5f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 1f, 0f, 0f, 0f, local: true)
				{
					motion = new Vector2(-3f, 0f),
					delayBeforeAnimationStart = i,
					text = Utility.loadStringShort("UI", "EndCredit_Slimes")
				});
				attachedCharacter = slime;
				i += 1500;
				continue;
			}
			case "Shadow Shaman":
			case "Shadow Sniper":
			case "Shadow Brute":
			case "Cat":
			case "Frog":
			case "Crow":
			case "Frost Jelly":
			case "Sludge":
			case "Iridium Slime":
			case "Tiger Slime":
			case "Lava Bat":
			case "Iridium Bat":
			case "Frost Bat":
			case "Royal Serpent":
				continue;
			}
			try
			{
				Texture2D t = ((attachedCharacter == null) ? Game1.content.Load<Texture2D>("Characters\\Monsters\\" + v.Key) : attachedCharacter.Sprite.Texture);
				base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(spriteWidth * spriteStartIndex % t.Width, spriteWidth * spriteStartIndex / t.Width * spriteHeight + 1, spriteWidth, spriteHeight - 1), 100f, animationLength, 999999, new Vector2(Game1.viewport.Width + 192, (float)Game1.viewport.Height * 0.5f - (float)(spriteHeight * 4) - 16f + (float)yOffset), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
				{
					motion = new Vector2(-3f, 0f),
					delayBeforeAnimationStart = i,
					yPeriodic = (animationLength == 1),
					yPeriodicRange = 16f,
					yPeriodicLoopTime = 3000f,
					attachedCharacter = attachedCharacter,
					texture = ((attachedCharacter == null) ? t : null),
					pingPong = pingpong
				});
				base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(spriteWidth * spriteStartIndex % t.Width, spriteWidth * spriteStartIndex / t.Width * spriteHeight, spriteWidth, spriteHeight), 100f, animationLength, 999999, new Vector2((float)(Game1.viewport.Width + 192 + spriteWidth * 4 / 2) - Game1.dialogueFont.MeasureString(Game1.parseText(v.Value.Split('/')[14], Game1.dialogueFont, 256)).X / 2f, (float)Game1.viewport.Height * 0.5f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 1f, 0f, 0f, 0f, local: true)
				{
					motion = new Vector2(-3f, 0f),
					delayBeforeAnimationStart = i,
					text = Game1.parseText(v.Value.Split('/')[14], Game1.dialogueFont, 256)
				});
				i += 1500;
			}
			catch
			{
			}
		}
		return sb.ToString();
	}

	/// <summary>Try to draw an NPC in the ending slide show.</summary>
	/// <param name="name">The NPC's internal name.</param>
	/// <param name="data">The NPC's content data.</param>
	/// <param name="animationInterval">The interval for their walking animation.</param>
	/// <param name="delayBeforeAnimationStart">The millisecond delay until they begin walking across the screen.</param>
	public bool TryDrawNpc(string name, CharacterData data, int animationInterval, int delayBeforeAnimationStart)
	{
		try
		{
			string texName = NPC.getTextureNameForCharacter(name);
			Rectangle sourceRect = new Rectangle(0, data.Size.Y * 3, data.Size.X, data.Size.Y);
			Vector2 position = new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.4f + (float)((32 - sourceRect.Height) * 4));
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("Characters\\" + texName, sourceRect, 90f, 4, 999999, position, flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = delayBeforeAnimationStart
			});
			return true;
		}
		catch
		{
			return false;
		}
	}

	private bool sayGrufferSummitIntro(NPC spouse)
	{
		switch ((string)spouse.name)
		{
		case "Harvey":
		case "Elliott":
			return false;
		case "Abigail":
		case "Maru":
			return true;
		default:
			return spouse.Gender == Gender.Male;
		}
	}

	public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
	{
		base.drawAboveAlwaysFrontLayer(b);
		if (Game1.eventUp && isShowingEndSlideshow)
		{
			b.Draw(Game1.staminaRect, new Rectangle(0, (int)((float)Game1.viewport.Height * 0.5f - 400f), Game1.viewport.Width, 8), Utility.GetPrismaticColor());
			b.Draw(Game1.staminaRect, new Rectangle(0, (int)((float)Game1.viewport.Height * 0.5f - 412f), Game1.viewport.Width, 4), Utility.GetPrismaticColor() * 0.8f);
			b.Draw(Game1.staminaRect, new Rectangle(0, (int)((float)Game1.viewport.Height * 0.5f - 432f), Game1.viewport.Width, 4), Utility.GetPrismaticColor() * 0.6f);
			b.Draw(Game1.staminaRect, new Rectangle(0, (int)((float)Game1.viewport.Height * 0.5f - 468f), Game1.viewport.Width, 4), Utility.GetPrismaticColor() * 0.4f);
			b.Draw(Game1.staminaRect, new Rectangle(0, (int)((float)Game1.viewport.Height * 0.5f - 536f), Game1.viewport.Width, 4), Utility.GetPrismaticColor() * 0.2f);
			b.Draw(Game1.staminaRect, new Rectangle(0, (int)((float)Game1.viewport.Height * 0.5f + 240f), Game1.viewport.Width, 8), Utility.GetPrismaticColor());
			b.Draw(Game1.staminaRect, new Rectangle(0, (int)((float)Game1.viewport.Height * 0.5f + 256f), Game1.viewport.Width, 4), Utility.GetPrismaticColor() * 0.8f);
			b.Draw(Game1.staminaRect, new Rectangle(0, (int)((float)Game1.viewport.Height * 0.5f + 276f), Game1.viewport.Width, 4), Utility.GetPrismaticColor() * 0.6f);
			b.Draw(Game1.staminaRect, new Rectangle(0, (int)((float)Game1.viewport.Height * 0.5f + 312f), Game1.viewport.Width, 4), Utility.GetPrismaticColor() * 0.4f);
			b.Draw(Game1.staminaRect, new Rectangle(0, (int)((float)Game1.viewport.Height * 0.5f + 380f), Game1.viewport.Width, 4), Utility.GetPrismaticColor() * 0.2f);
		}
	}
}
