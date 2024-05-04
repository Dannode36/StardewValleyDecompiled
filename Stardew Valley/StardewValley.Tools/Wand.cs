using Microsoft.Xna.Framework;
using StardewValley.Extensions;
using StardewValley.Locations;

namespace StardewValley.Tools;

public class Wand : Tool
{
	public Wand()
		: base("Return Scepter", 0, 2, 2, stackable: false)
	{
		base.InstantUse = true;
	}

	/// <inheritdoc />
	protected override void MigrateLegacyItemId()
	{
		base.ItemId = "ReturnScepter";
	}

	/// <inheritdoc />
	protected override Item GetOneNew()
	{
		return new Wand();
	}

	public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
	{
		if (!who.bathingClothes && who.IsLocalPlayer && !who.onBridge.Value)
		{
			indexOfMenuItemView.Value = 2;
			base.CurrentParentTileIndex = 2;
			for (int i = 0; i < 12; i++)
			{
				Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite(354, Game1.random.Next(25, 75), 6, 1, new Vector2(Game1.random.Next((int)who.position.X - 256, (int)who.position.X + 192), Game1.random.Next((int)who.position.Y - 256, (int)who.position.Y + 192)), flicker: false, Game1.random.NextBool()));
			}
			who.playNearbySoundAll("wand");
			Game1.displayFarmer = false;
			who.temporarilyInvincible = true;
			who.temporaryInvincibilityTimer = -2000;
			who.Halt();
			who.faceDirection(2);
			who.CanMove = false;
			who.freezePause = 2000;
			Game1.flashAlpha = 1f;
			DelayedAction.fadeAfterDelay(wandWarpForReal, 1000);
			Rectangle playerBounds = who.GetBoundingBox();
			new Rectangle(playerBounds.X, playerBounds.Y, 64, 64).Inflate(192, 192);
			int j = 0;
			Point playerTile = who.TilePoint;
			for (int xTile = playerTile.X + 8; xTile >= playerTile.X - 8; xTile--)
			{
				Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite(6, new Vector2(xTile, playerTile.Y) * 64f, Color.White, 8, flipped: false, 50f)
				{
					layerDepth = 1f,
					delayBeforeAnimationStart = j * 25,
					motion = new Vector2(-0.25f, 0f)
				});
				j++;
			}
			base.CurrentParentTileIndex = base.IndexOfMenuItemView;
		}
	}

	/// <inheritdoc />
	public override bool actionWhenPurchased(string shopId)
	{
		Game1.player.mailReceived.Add("ReturnScepter");
		return base.actionWhenPurchased(shopId);
	}

	private void wandWarpForReal()
	{
		FarmHouse home = Utility.getHomeOfFarmer(Game1.player);
		if (home != null)
		{
			Point position = home.getFrontDoorSpot();
			Game1.warpFarmer("Farm", position.X, position.Y, flip: false);
			Game1.fadeToBlackAlpha = 0.99f;
			Game1.screenGlow = false;
			lastUser.temporarilyInvincible = false;
			lastUser.temporaryInvincibilityTimer = 0;
			Game1.displayFarmer = true;
			lastUser.CanMove = true;
		}
	}
}
