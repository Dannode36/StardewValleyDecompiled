using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Companions;

public class HoppingCompanion : Companion
{
	public HoppingCompanion()
	{
	}

	public HoppingCompanion(int which = 0)
	{
		whichVariant.Value = which;
	}

	public override void Draw(SpriteBatch b)
	{
		if (base.Owner != null && base.Owner.currentLocation != null && (!(base.Owner.currentLocation.DisplayName == "Temp") || Game1.isFestival()))
		{
			Texture2D texture = Game1.content.Load<Texture2D>("TileSheets\\companions");
			_draw(b, texture, new Rectangle(0, 16, 16, 16));
		}
	}

	protected void _draw(SpriteBatch b, Texture2D texture, Rectangle startingSourceRect)
	{
		SpriteEffects effect = SpriteEffects.None;
		if (direction.Value == 3)
		{
			effect = SpriteEffects.FlipHorizontally;
		}
		if (height > 0f)
		{
			if (gravity > 0f)
			{
				b.Draw(texture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset + new Vector2(0f, (0f - height) * 4f)), Utility.translateRect(startingSourceRect, 16), Color.White, 0f, new Vector2(8f, 16f), 4f, effect, _position.Y / 10000f);
			}
			else if (gravity > -0.15f)
			{
				b.Draw(texture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset + new Vector2(0f, (0f - height) * 4f)), Utility.translateRect(startingSourceRect, 32), Color.White, 0f, new Vector2(8f, 16f), 4f, effect, _position.Y / 10000f);
			}
			else
			{
				b.Draw(texture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset + new Vector2(0f, (0f - height) * 4f)), Utility.translateRect(startingSourceRect, 48), Color.White, 0f, new Vector2(8f, 16f), 4f, effect, _position.Y / 10000f);
			}
		}
		else
		{
			b.Draw(texture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset + new Vector2(0f, (0f - height) * 4f)), startingSourceRect, Color.White, 0f, new Vector2(8f, 16f), 4f, effect, _position.Y / 10000f);
		}
		b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f * Utility.Lerp(1f, 0.8f, Math.Min(height, 1f)), SpriteEffects.None, 0f);
	}
}
