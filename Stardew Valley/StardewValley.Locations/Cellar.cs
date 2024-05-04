using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Objects;

namespace StardewValley.Locations;

public class Cellar : GameLocation
{
	public Cellar()
	{
	}

	public Cellar(string mapPath, string name)
		: base(mapPath, name)
	{
		setUpAgingBoards();
	}

	public void setUpAgingBoards()
	{
		for (int i = 6; i < 17; i++)
		{
			Vector2 v = new Vector2(i, 8f);
			if (!objects.ContainsKey(v))
			{
				objects.Add(v, new Cask(v));
			}
			v = new Vector2(i, 10f);
			if (!objects.ContainsKey(v))
			{
				objects.Add(v, new Cask(v));
			}
			v = new Vector2(i, 12f);
			if (!objects.ContainsKey(v))
			{
				objects.Add(v, new Cask(v));
			}
		}
	}

	protected override void resetLocalState()
	{
		base.resetLocalState();
		string target = "Farmhouse";
		bool targetFound = false;
		Utility.ForEachLocation(delegate(GameLocation location)
		{
			if (location is Cabin cabin && cabin.GetCellarName() == base.Name)
			{
				target = cabin.NameOrUniqueName;
				targetFound = true;
				return false;
			}
			return true;
		});
		foreach (Warp warp in warps)
		{
			warp.TargetName = target;
		}
	}

	public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
	{
		b.Draw(Game1.staminaRect, new Rectangle(-Game1.viewport.X, -Game1.viewport.Y - 256, 512, 256), Color.Black);
		base.drawAboveAlwaysFrontLayer(b);
	}
}
