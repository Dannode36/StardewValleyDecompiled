using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.Menus;
using StardewValley.Network;

namespace StardewValley.Characters;

public class Raccoon : NPC
{
	[XmlElement("mrs_raccoon")]
	public readonly NetBool mrs_raccoon = new NetBool();

	[XmlIgnore]
	public readonly NetMutex mutex = new NetMutex();

	private bool wasTalkedTo;

	private float updateFacingDirectionTimer;

	public Raccoon()
	{
		reloadSprite();
	}

	public Raccoon(bool mrs_racooon = false)
		: base(new AnimatedSprite("Characters\\raccoon", mrs_racooon ? 40 : 0, 32, 32), new Vector2(54.5f, 8.25f) * 64f, 2, "Raccoon")
	{
		base.HideShadow = true;
		mrs_raccoon.Value = mrs_racooon;
		base.Breather = false;
		if (mrs_racooon)
		{
			base.Position = new Vector2(56.5f, 8.25f) * 64f;
			base.Name = "MrsRaccoon";
		}
	}

	public override void reloadSprite(bool onlyAppearance = false)
	{
		base.HideShadow = true;
		base.Breather = false;
		if (Sprite == null)
		{
			Sprite = new AnimatedSprite("Characters\\raccoon", mrs_raccoon.Value ? 40 : 0, 32, 32);
		}
		if ((bool)mrs_raccoon)
		{
			base.Position = new Vector2(56.5f, 8.25f) * 64f;
			base.Name = "MrsRaccoon";
		}
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(mrs_raccoon, "mrs_raccoon");
		base.NetFields.AddField(mutex.NetFields, "mutex.NetFields");
	}

	public void activate()
	{
		if (mrs_raccoon.Value)
		{
			Utility.TryOpenShopMenu("Raccoon", base.Name);
			return;
		}
		bool interim = Game1.netWorldState.Value.Date.TotalDays - Game1.netWorldState.Value.DaysPlayedWhenLastRaccoonBundleWasFinished < 7;
		if (!wasTalkedTo)
		{
			int whichDialogue = Game1.netWorldState.Value.TimesFedRaccoons;
			if (whichDialogue == 0)
			{
				interim = false;
			}
			if (whichDialogue >= 5 && !interim)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Raccoon_intro"));
			}
			else if (whichDialogue > 5 && interim)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Raccoon_interim"));
			}
			else
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Raccoon_" + (interim ? "interim_" : "intro_") + whichDialogue));
			}
			if (interim)
			{
				return;
			}
			Game1.afterDialogues = delegate
			{
				mutex.RequestLock(delegate
				{
					_activateMrRaccoon();
				}, delegate
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Raccoon_busy"));
				});
			};
		}
		else if (!interim)
		{
			mutex.RequestLock(delegate
			{
				_activateMrRaccoon();
			}, delegate
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Raccoon_busy"));
			});
		}
	}

	public override void dayUpdate(int dayOfMonth)
	{
		base.dayUpdate(dayOfMonth);
		wasTalkedTo = false;
		mutex?.ReleaseLock();
	}

	private void _activateMrRaccoon()
	{
		wasTalkedTo = true;
		if (Game1.netWorldState.Value.SeasonOfCurrentRacconBundle == -1)
		{
			Game1.netWorldState.Value.SeasonOfCurrentRacconBundle = (Game1.seasonIndex + ((Game1.dayOfMonth > 21) ? 1 : 0)) % 4;
		}
		int timesFed = Game1.netWorldState.Value.TimesFedRaccoons;
		List<BundleIngredientDescription> ingredients = new List<BundleIngredientDescription>();
		Random raccoonRandom = Utility.CreateRandom(Game1.uniqueIDForThisGame, timesFed * 377);
		for (int i = 0; i < 10; i++)
		{
			raccoonRandom.Next();
		}
		int which = ((timesFed < 5) ? (timesFed % 5) : raccoonRandom.Next(5));
		addNextIngredient(ingredients, which, raccoonRandom);
		addNextIngredient(ingredients, which, raccoonRandom);
		addNextIngredient(ingredients, which, raccoonRandom);
		Game1.activeClickableMenu = new JunimoNoteMenu(new Bundle("Seafood", null, ingredients, new bool[1])
		{
			bundleTextureOverride = Game1.content.Load<Texture2D>("LooseSprites\\BundleSprites"),
			bundleTextureIndexOverride = 14 + which
		}, "LooseSprites\\raccoon_bundle_menu")
		{
			onIngredientDeposit = delegate(int x)
			{
				Game1.netWorldState.Value.raccoonBundles[x] = true;
			},
			onBundleComplete = bundleComplete,
			onScreenSwipeFinished = bundleCompleteAfterSwipe,
			behaviorBeforeCleanup = delegate
			{
				mutex?.ReleaseLock();
			}
		};
	}

	public Item getBundleReward()
	{
		switch (Game1.netWorldState.Value.TimesFedRaccoons)
		{
		case 1:
			return Utility.getRaccoonSeedForCurrentTimeOfYear(Game1.player, Game1.random, 25);
		case 2:
			Game1.Multiplayer.broadcastGlobalMessage("Strings\\1_6_Strings:Raccoon_expanded", false, null);
			return ItemRegistry.Create("(O)Book_WildSeeds");
		case 3:
			Game1.Multiplayer.broadcastGlobalMessage("Strings\\1_6_Strings:Raccoon_expanded", false, null);
			return ItemRegistry.Create("(H)RaccoonHat");
		case 4:
			Game1.Multiplayer.broadcastGlobalMessage("Strings\\1_6_Strings:Raccoon_expanded", false, null);
			return ItemRegistry.Create("(O)872", 5);
		case 5:
			Game1.Multiplayer.broadcastGlobalMessage("Strings\\1_6_Strings:Raccoon_expanded", false, null);
			return ItemRegistry.Create("(F)JungleTank");
		case 6:
			Game1.Multiplayer.broadcastGlobalMessage("Strings\\1_6_Strings:Raccoon_expanded", false, null);
			break;
		}
		Random raccoonRandom = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.netWorldState.Value.TimesFedRaccoons * 377);
		for (int i = 0; i < 10; i++)
		{
			raccoonRandom.Next();
		}
		switch (raccoonRandom.Next(5))
		{
		case 0:
			return ItemRegistry.Create("(O)872", 7);
		case 1:
			return ItemRegistry.Create("(O)PurpleBook");
		case 2:
			if (Game1.netWorldState.Value.GoldenWalnutsFound >= 100 && Utility.getFarmerItemsShippedPercent() < 1f)
			{
				Item missed = Utility.recentlyDiscoveredMissingBasicShippedItem;
				if (missed != null && missed.Category != -26 && missed.ItemId != "812")
				{
					return missed;
				}
			}
			return ItemRegistry.Create("(O)MysteryBox", 5);
		case 3:
			return ItemRegistry.Create("(O)StardropTea");
		case 4:
			return Utility.getRaccoonSeedForCurrentTimeOfYear(Game1.player, Game1.random, 25);
		default:
			return ItemRegistry.Create("(O)MysteryBox", 3);
		}
	}

	private void bundleCompleteAfterSwipe(JunimoNoteMenu menu)
	{
		Game1.activeClickableMenu = null;
		mutex?.ReleaseLock();
		Game1.netWorldState.Value.DaysPlayedWhenLastRaccoonBundleWasFinished = Game1.netWorldState.Value.Date.TotalDays;
		Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Raccoon_receive"));
		Game1.afterDialogues = delegate
		{
			Game1.player.addItemByMenuIfNecessaryElseHoldUp(getBundleReward());
		};
	}

	private void bundleComplete(JunimoNoteMenu menu)
	{
		JunimoNoteMenu.screenSwipe = new ScreenSwipe(1);
		Game1.netWorldState.Value.TimesFedRaccoons++;
		Game1.netWorldState.Value.raccoonBundles[0] = false;
		Game1.netWorldState.Value.raccoonBundles[1] = false;
		Game1.netWorldState.Value.SeasonOfCurrentRacconBundle = -1;
		wasTalkedTo = false;
	}

	private void addNextIngredient(List<BundleIngredientDescription> ingredients, int whichBundle, Random r)
	{
		int whichIngredient = ingredients.Count;
		int whichSeasonToChoose = Game1.netWorldState.Value.SeasonOfCurrentRacconBundle;
		switch (whichBundle)
		{
		case 0:
			switch (whichIngredient)
			{
			case 0:
				ingredients.Add(new BundleIngredientDescription(r.ChooseFrom(new string[7] { "722", "721", "716", "719", "723", "718", "372" }), 5, 0, Game1.netWorldState.Value.raccoonBundles[0]));
				break;
			case 1:
			{
				string[][] fish = new string[4][]
				{
					new string[7] { "136", "132", "700", "702", "156", "267", "706" },
					new string[11]
					{
						"136", "132", "700", "702", "156", "267", "706", "138", "701", "146",
						"130"
					},
					new string[9] { "136", "132", "700", "702", "156", "701", "269", "139", "139" },
					new string[9] { "136", "132", "700", "702", "156", "146", "130", "141", "269" }
				};
				ingredients.Add(new BundleIngredientDescription("SmokedFish", 1, 0, Game1.netWorldState.Value.raccoonBundles[1], r.ChooseFrom(fish[whichSeasonToChoose])));
				break;
			}
			}
			break;
		case 1:
		{
			string[][] fruits = new string[4][]
			{
				new string[5] { "90", "634", "638", "400", "88" },
				new string[7] { "90", "258", "260", "635", "636", "88", "396" },
				new string[7] { "90", "613", "282", "637", "410", "88", "406" },
				new string[6] { "90", "414", "414", "88", "Powdermelon", "Powdermelon" }
			};
			switch (whichIngredient)
			{
			case 0:
				ingredients.Add(new BundleIngredientDescription("DriedFruit", 1, 0, Game1.netWorldState.Value.raccoonBundles[0], r.ChooseFrom(fruits[whichSeasonToChoose])));
				break;
			case 1:
			{
				string fruit = "";
				while (fruit == "" || fruit == ingredients[0].preservesId)
				{
					fruit = r.ChooseFrom(fruits[whichSeasonToChoose]);
				}
				ingredients.Add(new BundleIngredientDescription("Jelly", 1, 0, Game1.netWorldState.Value.raccoonBundles[1], fruit));
				break;
			}
			}
			break;
		}
		case 2:
		{
			string[][] mushrooms = new string[4][]
			{
				new string[3] { "422", "404", "257" },
				new string[2] { "422", "404" },
				new string[3] { "422", "404", "281" },
				new string[2] { "422", "404" }
			};
			switch (whichIngredient)
			{
			case 0:
				ingredients.Add(new BundleIngredientDescription("DriedMushroom", 1, 0, Game1.netWorldState.Value.raccoonBundles[0], r.ChooseFrom(mushrooms[whichSeasonToChoose])));
				break;
			case 1:
				ingredients.Add(new BundleIngredientDescription(r.ChooseFrom(new string[3] { "-5", "78", "157" }), 5, 0, Game1.netWorldState.Value.raccoonBundles[1]));
				break;
			}
			break;
		}
		case 3:
		{
			string[][] veggies = new string[4][]
			{
				new string[8] { "190", "188", "250", "192", "16", "22", "Carrot", "Carrot" },
				new string[6] { "270", "264", "256", "78", "SummerSquash", "SummerSquash" },
				new string[5] { "Broccoli", "Broccoli", "278", "272", "276" },
				new string[3] { "416", "412", "78" }
			};
			switch (whichIngredient)
			{
			case 0:
				ingredients.Add(new BundleIngredientDescription("Juice", 1, 0, Game1.netWorldState.Value.raccoonBundles[0], r.ChooseFrom(veggies[whichSeasonToChoose])));
				break;
			case 1:
			{
				string fruit = "";
				while (fruit == "" || fruit == ingredients[0].preservesId)
				{
					fruit = r.ChooseFrom(veggies[whichSeasonToChoose]);
				}
				ingredients.Add(new BundleIngredientDescription("Pickle", 1, 0, Game1.netWorldState.Value.raccoonBundles[1], fruit));
				break;
			}
			}
			break;
		}
		case 4:
		{
			string[] items = new string[14]
			{
				"Moss_10", "110_1", "168_5", "766_99", "767_20", "535_8", "536_5", "537_3", "393_4", "397_2",
				"684_20", "72_1", "68_3", "156_3"
			};
			switch (whichIngredient)
			{
			case 0:
			{
				string s = r.ChooseFrom(items);
				ingredients.Add(new BundleIngredientDescription(s.Split('_')[0], Convert.ToInt32(s.Split('_')[1]), 0, Game1.netWorldState.Value.raccoonBundles[0]));
				break;
			}
			case 1:
			{
				string fruit = "";
				while (fruit == "" || fruit.Split("_")[0] == ingredients[0].id)
				{
					fruit = r.ChooseFrom(items);
				}
				ingredients.Add(new BundleIngredientDescription(fruit.Split('_')[0], Convert.ToInt32(fruit.Split('_')[1]), 0, Game1.netWorldState.Value.raccoonBundles[1]));
				break;
			}
			}
			break;
		}
		}
	}

	public override void update(GameTime time, GameLocation location)
	{
		_ = shakeTimer;
		base.update(time, location);
		mutex?.Update(location);
		if (mrs_raccoon.Value)
		{
			Sprite.CurrentFrame = ((time.TotalGameTime.TotalMilliseconds % 13200.0 > 10000.0) ? (40 + (int)(time.TotalGameTime.TotalMilliseconds % 800.0 / 100.0)) : (32 + (int)(time.TotalGameTime.TotalMilliseconds % 1200.0 / 150.0)));
		}
		else if (Vector2.Distance(base.Position, Game1.player.getStandingPosition()) < 256f)
		{
			switch (getGeneralDirectionTowards(Game1.player.getStandingPosition(), 32, opposite: false, useTileCalculations: false))
			{
			case 0:
				Sprite.CurrentFrame = 16 + (int)(time.TotalGameTime.TotalMilliseconds % 800.0 / 100.0);
				break;
			case 1:
			case 2:
			case 3:
				Sprite.CurrentFrame = (int)(time.TotalGameTime.TotalMilliseconds % 800.0 / 100.0);
				break;
			}
		}
		else
		{
			Sprite.CurrentFrame = ((time.TotalGameTime.TotalMilliseconds % 8000.0 < 3200.0) ? ((int)(time.TotalGameTime.TotalMilliseconds % 800.0 / 100.0)) : (48 + (int)(time.TotalGameTime.TotalMilliseconds % 400.0 / 100.0)));
		}
	}

	public override bool checkAction(Farmer who, GameLocation l)
	{
		if (shakeTimer <= 0)
		{
			if ((bool)mrs_raccoon)
			{
				playNearbySoundLocal("Raccoon", 2400);
			}
			else
			{
				playNearbySoundLocal("Raccoon");
			}
			shakeTimer = 200;
			who.freezePause = 300;
			DelayedAction.functionAfterDelay(activate, 300);
		}
		return true;
	}

	public override void performTenMinuteUpdate(int timeOfDay, GameLocation l)
	{
		base.performTenMinuteUpdate(timeOfDay, l);
	}

	public override void draw(SpriteBatch b)
	{
		base.draw(b);
	}
}
