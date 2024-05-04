using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Locations;

namespace StardewValley;

public class Shed : DecoratableLocation
{
	private bool isRobinUpgrading;

	public Shed()
	{
	}

	public Shed(string m, string name)
		: base(m, name)
	{
	}

	protected override void resetLocalState()
	{
		base.resetLocalState();
		if (Game1.isDarkOut(this))
		{
			Game1.ambientLight = new Color(180, 180, 0);
		}
		isRobinUpgrading = Game1.GetBuildingUnderConstruction()?.HasIndoorsName(base.NameOrUniqueName) ?? false;
	}

	public override void draw(SpriteBatch b)
	{
		base.draw(b);
		if (isRobinUpgrading)
		{
			b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(Game1.viewport, new Vector2(64f, 64f)), new Rectangle(90, 0, 33, 6), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01546f);
			b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(Game1.viewport, new Vector2(64f, 84f)), new Rectangle(90, 0, 33, 31), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.015360001f);
		}
	}
}
