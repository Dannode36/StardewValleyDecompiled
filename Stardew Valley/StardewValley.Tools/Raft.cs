using Microsoft.Xna.Framework;

namespace StardewValley.Tools;

public class Raft : Tool
{
	public Raft()
		: base("Raft", 0, 1, 1, stackable: false)
	{
		base.InstantUse = true;
	}

	/// <inheritdoc />
	protected override Item GetOneNew()
	{
		return new Raft();
	}

	protected override string loadDisplayName()
	{
		return Game1.content.LoadString("Strings\\StringsFromCSFiles:Raft.cs.14204");
	}

	protected override string loadDescription()
	{
		return Game1.content.LoadString("Strings\\StringsFromCSFiles:Raft.cs.14205");
	}

	public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
	{
		base.DoFunction(location, x, y, power, who);
		if (!who.isRafting && location.isWaterTile(x / 64, y / 64))
		{
			who.isRafting = true;
			Rectangle collidingBox = new Rectangle(x - 32, y - 32, 64, 64);
			if (location.isCollidingPosition(collidingBox, Game1.viewport, who))
			{
				who.isRafting = false;
				return;
			}
			who.xVelocity = ((who.FacingDirection == 1) ? 3f : ((who.FacingDirection == 3) ? (-3f) : 0f));
			who.yVelocity = ((who.FacingDirection == 2) ? 3f : ((who.FacingDirection == 0) ? (-3f) : 0f));
			who.Position = new Vector2(x - 32, y - 32 - 32 - ((y < who.StandingPixel.Y) ? 64 : 0));
			who.playNearbySoundAll("dropItemInWater");
		}
		base.CurrentParentTileIndex = base.IndexOfMenuItemView;
	}
}
