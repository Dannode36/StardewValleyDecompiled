using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;
using StardewValley.Network;
using xTile.Dimensions;

namespace StardewValley.Locations;

public class BeachNightMarket : GameLocation
{
	private Texture2D shopClosedTexture;

	private float smokeTimer;

	private string paintingMailKey;

	private bool hasReceivedFreeGift;

	private bool hasShownCCUpgrade;

	public BeachNightMarket()
	{
		forceLoadPathLayerLights = true;
	}

	public BeachNightMarket(string mapPath, string name)
		: base(mapPath, name)
	{
		forceLoadPathLayerLights = true;
	}

	public override void DayUpdate(int dayOfMonth)
	{
		base.DayUpdate(dayOfMonth);
		objects.Clear();
		hasReceivedFreeGift = false;
		paintingMailKey = "NightMarketYear" + Game1.year + "Day" + getDayOfNightMarket() + "_paintingSold";
	}

	public override void draw(SpriteBatch b)
	{
		base.draw(b);
		if (Game1.timeOfDay < 1700)
		{
			b.Draw(shopClosedTexture, Game1.GlobalToLocal(new Vector2(39f, 29f) * 64f + new Vector2(-1f, -3f) * 4f), new Microsoft.Xna.Framework.Rectangle(72, 167, 16, 17), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
			b.Draw(shopClosedTexture, Game1.GlobalToLocal(new Vector2(47f, 34f) * 64f + new Vector2(7f, -3f) * 4f), new Microsoft.Xna.Framework.Rectangle(45, 170, 26, 14), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
			b.Draw(shopClosedTexture, Game1.GlobalToLocal(new Vector2(19f, 31f) * 64f + new Vector2(6f, 10f) * 4f), new Microsoft.Xna.Framework.Rectangle(89, 164, 18, 23), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
		}
		if (!Game1.player.mailReceived.Contains(paintingMailKey))
		{
			b.Draw(shopClosedTexture, Game1.GlobalToLocal(new Vector2(41f, 33f) * 64f + new Vector2(2f, 2f) * 4f), new Microsoft.Xna.Framework.Rectangle(144 + (getDayOfNightMarket() - 1 + (Game1.year - 1) % 3 * 3) * 28, 201, 28, 13), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.22500001f);
		}
	}

	public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
	{
		switch (getTileIndexAt(tileLocation, "Buildings"))
		{
		case 595:
			Utility.TryOpenShopMenu("Festival_NightMarket_DecorationBoat", this);
			break;
		case 69:
		case 877:
			if (Game1.timeOfDay < 1700)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BeachNightMarket_GiftGiverClosed"));
			}
			else if (!hasReceivedFreeGift)
			{
				createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:BeachNightMarket_GiftGiverQuestion"), createYesNoResponses(), "GiftGiverQuestion");
			}
			else
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BeachNightMarket_GiftGiverEnjoy"));
			}
			break;
		case 653:
			if (Game1.RequireLocation<Submarine>("Submarine").submerged.Value || Game1.netWorldState.Value.IsSubmarineLocked)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BeachNightMarket_SubmarineInUse"));
				return true;
			}
			break;
		case 399:
			Utility.TryOpenShopMenu("Traveler", this);
			break;
		case 70:
			Utility.TryOpenShopMenu("Festival_NightMarket_MagicBoat_Day" + getDayOfNightMarket(), this);
			break;
		case 68:
			if (Game1.timeOfDay < 1700)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BeachNightMarket_PainterClosed"));
			}
			else if (Game1.player.mailReceived.Contains(paintingMailKey))
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BeachNightMarket_PainterSold"));
			}
			else
			{
				createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:BeachNightMarket_PainterQuestion"), createYesNoResponses(), "PainterQuestion");
			}
			break;
		case 1285:
			createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:BeachNightMarket_WarperQuestion"), createYesNoResponses(), "WarperQuestion");
			break;
		}
		return base.checkAction(tileLocation, viewport, who);
	}

	public int getDayOfNightMarket()
	{
		return Utility.GetDayOfPassiveFestival("NightMarket");
	}

	public override bool answerDialogueAction(string questionAndAnswer, string[] questionParams)
	{
		if (questionAndAnswer == null)
		{
			return false;
		}
		switch (questionAndAnswer)
		{
		case "WarperQuestion_Yes":
			if (Game1.player.Money < 250)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BusStop_NotEnoughMoneyForTicket"));
			}
			else
			{
				Game1.player.Money -= 250;
				Game1.player.CanMove = true;
				ItemRegistry.Create<Object>("(O)688").performUseAction(this);
				Game1.player.freezePause = 5000;
			}
			return true;
		case "PainterQuestion_Yes":
			if (Game1.player.mailReceived.Contains(paintingMailKey))
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BeachNightMarket_PainterSold"));
				break;
			}
			if (Game1.player.Money < 1200)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BusStop_NotEnoughMoneyForTicket"));
				break;
			}
			Game1.player.Money -= 1200;
			Game1.activeClickableMenu = null;
			Game1.player.addItemByMenuIfNecessaryElseHoldUp(ItemRegistry.Create("(F)" + (1838 + ((getDayOfNightMarket() - 1) * 2 + (Game1.year - 1) % 3 * 6))));
			Game1.multiplayer.globalChatInfoMessage("Lupini", Game1.player.Name);
			Game1.multiplayer.broadcastPartyWideMail(paintingMailKey, Multiplayer.PartyWideMessageQueue.SeenMail, no_letter: true);
			break;
		case "GiftGiverQuestion_Yes":
			if (hasReceivedFreeGift)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BeachNightMarket_GiftGiverEnjoy"));
				break;
			}
			Game1.player.freezePause = 5000;
			temporarySprites.Add(new TemporaryAnimatedSprite
			{
				texture = shopClosedTexture,
				layerDepth = 0.2442f,
				scale = 4f,
				sourceRectStartingPos = new Vector2(354f, 168f),
				sourceRect = new Microsoft.Xna.Framework.Rectangle(354, 168, 32, 32),
				animationLength = 1,
				id = 777,
				holdLastFrame = true,
				interval = 250f,
				position = new Vector2(13f, 36f) * 64f,
				delayBeforeAnimationStart = 500,
				endFunction = getFreeGiftPartOne
			});
			hasReceivedFreeGift = true;
			break;
		}
		return base.answerDialogueAction(questionAndAnswer, questionParams);
	}

	public void getFreeGiftPartOne(int extra)
	{
		removeTemporarySpritesWithIDLocal(777);
		Game1.playSound("Milking");
		temporarySprites.Add(new TemporaryAnimatedSprite
		{
			texture = shopClosedTexture,
			layerDepth = 0.2442f,
			scale = 4f,
			sourceRect = new Microsoft.Xna.Framework.Rectangle(386, 168, 32, 32),
			animationLength = 1,
			id = 778,
			holdLastFrame = true,
			interval = 9500f,
			position = new Vector2(13f, 36f) * 64f
		});
		for (int i = 0; i <= 2000; i += 100)
		{
			temporarySprites.Add(new TemporaryAnimatedSprite
			{
				texture = shopClosedTexture,
				delayBeforeAnimationStart = i,
				id = 778,
				layerDepth = 0.24430001f,
				scale = 4f,
				sourceRect = new Microsoft.Xna.Framework.Rectangle(362, 170, 2, 2),
				animationLength = 1,
				interval = 100f,
				position = new Vector2(13f, 36f) * 64f + new Vector2(8f, 12f) * 4f,
				motion = new Vector2(0f, 2f),
				endFunction = ((i == 2000) ? new TemporaryAnimatedSprite.endBehavior(getFreeGift) : null)
			});
		}
	}

	public void getFreeGift(int extra)
	{
		Game1.player.addItemByMenuIfNecessaryElseHoldUp(ItemRegistry.Create("(O)395"));
		removeTemporarySpritesWithIDLocal(778);
	}

	public override void MakeMapModifications(bool force = false)
	{
		base.MakeMapModifications(force);
		if (force)
		{
			hasShownCCUpgrade = false;
		}
		if ((bool)Game1.RequireLocation<Beach>("Beach").bridgeFixed || NetWorldState.checkAnywhereForWorldStateID("beachBridgeFixed"))
		{
			Beach.fixBridge(this);
		}
		if (Game1.MasterPlayer.mailReceived.Contains("communityUpgradeShortcuts"))
		{
			Beach.showCommunityUpgradeShortcuts(this, ref hasShownCCUpgrade);
		}
	}

	protected override void resetLocalState()
	{
		base.resetLocalState();
		if (Game1.timeOfDay >= 1700)
		{
			Game1.changeMusicTrack("night_market");
		}
		else
		{
			Game1.changeMusicTrack("ocean");
		}
		shopClosedTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
		temporarySprites.Add(new EmilysParrot(new Vector2(2968f, 2056f)));
		paintingMailKey = "NightMarketYear" + Game1.year + "Day" + getDayOfNightMarket() + "_paintingSold";
	}

	public override void performTenMinuteUpdate(int timeOfDay)
	{
		base.performTenMinuteUpdate(timeOfDay);
		if (timeOfDay == 1700 && Game1.currentLocation.Equals(this))
		{
			Game1.changeMusicTrack("night_market");
			temporarySprites.Add(new TemporaryAnimatedSprite
			{
				texture = shopClosedTexture,
				sourceRect = new Microsoft.Xna.Framework.Rectangle(89, 164, 18, 23),
				layerDepth = 0.001f,
				interval = 100f,
				position = new Vector2(19f, 31f) * 64f + new Vector2(6f, 10f) * 4f,
				scale = 4f,
				animationLength = 3
			});
		}
	}

	public override void UpdateWhenCurrentLocation(GameTime time)
	{
		base.UpdateWhenCurrentLocation(time);
		smokeTimer -= time.ElapsedGameTime.Milliseconds;
		if (smokeTimer <= 0f)
		{
			temporarySprites.Add(new TemporaryAnimatedSprite
			{
				texture = shopClosedTexture,
				sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 180, 9, 11),
				sourceRectStartingPos = new Vector2(0f, 180f),
				layerDepth = 1f,
				interval = 250f,
				position = new Vector2(35f, 38f) * 64f + new Vector2(9f, 6f) * 4f,
				scale = 4f,
				scaleChange = 0.005f,
				alpha = 0.75f,
				alphaFade = 0.005f,
				motion = new Vector2(0f, -0.5f),
				acceleration = new Vector2((float)(Game1.random.NextDouble() - 0.5) / 100f, 0f),
				animationLength = 3,
				holdLastFrame = true
			});
			smokeTimer = 1250f;
		}
	}
}
