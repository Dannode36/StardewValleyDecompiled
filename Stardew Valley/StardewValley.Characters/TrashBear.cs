using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Network;

namespace StardewValley.Characters;

public class TrashBear : NPC
{
	private int showWantBubbleTimer;

	[XmlIgnore]
	public string itemWantedIndex;

	[XmlIgnore]
	private readonly NetEvent0 cutsceneEvent = new NetEvent0();

	[XmlIgnore]
	private readonly NetEvent1Field<string, NetString> eatEvent = new NetEvent1Field<string, NetString>();

	[XmlIgnore]
	private string itemBeingEaten;

	/// <inheritdoc />
	[XmlIgnore]
	public override bool IsVillager => false;

	public TrashBear()
		: base(new AnimatedSprite("Characters\\TrashBear", 0, 32, 32), new Vector2(102f, 95f) * 64f, 0, "TrashBear")
	{
		CurrentDialogue.Clear();
		base.HideShadow = true;
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(cutsceneEvent, "cutsceneEvent").AddField(eatEvent, "eatEvent");
		cutsceneEvent.onEvent += doCutscene;
		eatEvent.onEvent += doEatEvent;
	}

	/// <inheritdoc />
	public override void ChooseAppearance(LocalizedContentManager content = null)
	{
	}

	public override bool checkAction(Farmer who, GameLocation l)
	{
		if (sprite.Value.CurrentAnimation != null)
		{
			return false;
		}
		if (tryToReceiveActiveObject(who))
		{
			return true;
		}
		faceTowardFarmerForPeriod(4000, 3, faceAway: false, who);
		shake(500);
		Game1.playSound("trashbear");
		showWantBubbleTimer = 3000;
		updateItemWanted();
		return false;
	}

	public void updateItemWanted()
	{
		int which = 0;
		if (NetWorldState.checkAnywhereForWorldStateID("trashBear1"))
		{
			which = 1;
		}
		if (NetWorldState.checkAnywhereForWorldStateID("trashBear2"))
		{
			which = 2;
		}
		if (NetWorldState.checkAnywhereForWorldStateID("trashBear3"))
		{
			which = 3;
		}
		int randomSeed = 777111 + which;
		itemWantedIndex = Utility.getRandomPureSeasonalItem(Game1.season, randomSeed);
		if (which <= 1)
		{
			return;
		}
		int position = Utility.CreateRandom(Game1.uniqueIDForThisGame, randomSeed).Next(CraftingRecipe.cookingRecipes.Count);
		int counter = 0;
		foreach (string v in CraftingRecipe.cookingRecipes.Values)
		{
			if (counter == position)
			{
				string craft_result = ArgUtility.Get(v.Split('/'), 2);
				craft_result = ArgUtility.SplitBySpaceAndGet(craft_result, 0);
				itemWantedIndex = craft_result;
				break;
			}
			counter++;
		}
	}

	public override void update(GameTime time, GameLocation location)
	{
		base.update(time, location);
		cutsceneEvent.Poll();
		eatEvent.Poll();
		if (showWantBubbleTimer > 0)
		{
			showWantBubbleTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
		}
	}

	/// <inheritdoc />
	public override bool tryToReceiveActiveObject(Farmer who, bool probe = false)
	{
		updateItemWanted();
		if (who.ActiveObject?.QualifiedItemId == "(O)" + itemWantedIndex)
		{
			if (!probe)
			{
				Game1.currentLocation.playSound("coin");
				if (NetWorldState.checkAnywhereForWorldStateID("trashBear3"))
				{
					NetWorldState.addWorldStateIDEverywhere("trashBearDone");
				}
				else if (NetWorldState.checkAnywhereForWorldStateID("trashBear2"))
				{
					NetWorldState.addWorldStateIDEverywhere("trashBear3");
				}
				else if (NetWorldState.checkAnywhereForWorldStateID("trashBear1"))
				{
					NetWorldState.addWorldStateIDEverywhere("trashBear2");
				}
				else
				{
					NetWorldState.addWorldStateIDEverywhere("trashBear1");
				}
				eatEvent.Fire(itemWantedIndex);
				who.reduceActiveItemByOne();
			}
			return true;
		}
		return false;
	}

	public void doEatEvent(string item_index)
	{
		if (Game1.currentLocation is Forest)
		{
			showWantBubbleTimer = 0;
			itemBeingEaten = item_index;
			sprite.Value.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
			{
				new FarmerSprite.AnimationFrame(9, 1500, secondaryArm: false, flip: false, throwUpItem, behaviorAtEndOfFrame: true),
				new FarmerSprite.AnimationFrame(5, 1000, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(6, 250, secondaryArm: false, flip: false, chew),
				new FarmerSprite.AnimationFrame(7, 250, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(6, 250, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(7, 250, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(6, 250, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(7, 500, secondaryArm: false, flip: false, doneAnimating, behaviorAtEndOfFrame: true)
			});
			sprite.Value.loop = false;
			ParsedItemData dataOrErrorItem = ItemRegistry.GetDataOrErrorItem("(O)" + itemBeingEaten);
			string textureName = dataOrErrorItem.GetTextureName();
			Rectangle sourceRect = dataOrErrorItem.GetSourceRect();
			Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(textureName, sourceRect, 1500f, 1, 0, base.Position + new Vector2(96f, -92f), flicker: false, flipped: false, (float)(base.StandingPixel.Y + 1) / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f));
		}
	}

	private void throwUpItem(Farmer who)
	{
		ParsedItemData dataOrErrorItem = ItemRegistry.GetDataOrErrorItem("(O)" + itemBeingEaten);
		string textureName = dataOrErrorItem.GetTextureName();
		Rectangle sourceRect = dataOrErrorItem.GetSourceRect();
		Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(textureName, sourceRect, 1000f, 1, 0, base.Position + new Vector2(96f, -108f), flicker: false, flipped: false, (float)(base.StandingPixel.Y + 1) / 10000f, 0f, Color.White, 4f, -0.01f, 0f, 0f)
		{
			motion = new Vector2(-0.8f, -15f),
			acceleration = new Vector2(0f, 0.5f)
		});
		Game1.playSound("dwop");
	}

	private void chew(Farmer who)
	{
		Game1.playSound("eat");
		DelayedAction.playSoundAfterDelay("dirtyHit", 500);
		DelayedAction.playSoundAfterDelay("dirtyHit", 1000);
		DelayedAction.playSoundAfterDelay("gulp", 1400);
		ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem("(O)" + itemBeingEaten);
		string textureName = itemData.GetTextureName();
		for (int i = 0; i < 8; i++)
		{
			Rectangle sourceRect = itemData.GetSourceRect().Clone();
			sourceRect.X += 8;
			sourceRect.Y += 8;
			sourceRect.Width = 4;
			sourceRect.Height = 4;
			Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(textureName, sourceRect, 400f, 1, 0, base.Position + new Vector2(64f, -48f), flicker: false, flipped: false, (float)base.StandingPixel.Y / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f)
			{
				motion = new Vector2((float)Game1.random.Next(-30, 31) / 10f, Game1.random.Next(-6, -3)),
				acceleration = new Vector2(0f, 0.5f)
			});
		}
	}

	private void doneAnimating(Farmer who)
	{
		sprite.Value.CurrentFrame = 8;
		if (NetWorldState.checkAnywhereForWorldStateID("trashBearDone") && Game1.currentLocation is Forest)
		{
			doCutsceneEvent();
		}
	}

	private void doCutsceneEvent()
	{
		cutsceneEvent.Fire();
	}

	private void doCutscene()
	{
		if (!(Game1.currentLocation is Forest))
		{
			return;
		}
		if (Game1.activeClickableMenu != null && Game1.activeClickableMenu.readyToClose())
		{
			Game1.activeClickableMenu.exitThisMenuNoSound();
		}
		if (Game1.activeClickableMenu == null)
		{
			Game1.player.freezePause = 2000;
			Game1.globalFadeToBlack(delegate
			{
				Game1.currentLocation.startEvent(new Event("spring_day_ambient/-1000 -1000/farmer 104 95 3/skippable/addTemporaryActor TrashBear 32 32 102 95 0 false/animate TrashBear false true 250 0 1/viewport 102 97 clamp true/pause 3000/stopAnimation TrashBear/move TrashBear 0 2 2/faceDirection farmer 2/pause 1000/animate TrashBear false true 275 13 14 15 14/playSound trashbear_flute/specificTemporarySprite trashBearPrelude/viewport move -1 1 4000/pause 9000/stopAnimation TrashBear/playSound yoba/specificTemporarySprite trashBearMagic/pause 500/animate farmer false true 100 94/jump farmer/pause 2000/viewport move 1 -1 4000/stopAnimation farmer/move farmer 0 2 2/pause 4000/playSound trashbear/specificTemporarySprite trashBearUmbrella1/warp TrashBear -100 -100/pause 2000/faceDirection farmer 1/pause 2000/fade/viewport -5000 -5000/changeLocation Town/viewport 54 68 true/specificTemporarySprite trashBearTown/pause 10000/end", null, "777111"));
			});
		}
	}

	public override void draw(SpriteBatch b)
	{
		base.draw(b);
		if (showWantBubbleTimer > 0)
		{
			float yOffset = 2f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
			Point tile = base.TilePoint;
			float drawLayer = (float)((tile.Y + 1) * 64) / 10000f;
			yOffset -= 40f;
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(tile.X * 64 + 32, (float)(tile.Y * 64 - 96 - 48) + yOffset)), new Rectangle(141, 465, 20, 24), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, drawLayer + 1E-06f);
			ParsedItemData dataOrErrorItem = ItemRegistry.GetDataOrErrorItem("(O)" + itemWantedIndex);
			Texture2D texture = dataOrErrorItem.GetTexture();
			Rectangle sourceRect = dataOrErrorItem.GetSourceRect();
			b.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(tile.X * 64 + 64 + 8, (float)(tile.Y * 64 - 64 - 32 - 8) + yOffset)), sourceRect, Color.White, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, drawLayer + 1E-05f);
		}
	}
}
