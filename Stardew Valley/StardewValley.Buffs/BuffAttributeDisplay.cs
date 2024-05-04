using System;
using Microsoft.Xna.Framework.Graphics;
using Netcode;

namespace StardewValley.Buffs;

/// <summary>Display info for a buff attribute shown when a buff doesn't have its own dedicated icon.</summary>
public class BuffAttributeDisplay
{
	/// <summary>The icon texture to draw.</summary>
	public readonly Func<Texture2D> Texture;

	/// <summary>The icon's sprite index within the <see cref="F:StardewValley.Buffs.BuffAttributeDisplay.Texture" />.</summary>
	public readonly int SpriteIndex;

	/// <summary>The attribute's current value.</summary>
	public readonly Func<Buff, float> Value;

	/// <summary>The attribute's translated display name.</summary>
	public readonly Func<float, string> Description;

	/// <summary>Construct an instance for a custom buff attribute.</summary>
	/// <param name="texture">The icon texture to draw.</param>
	/// <param name="spriteIndex">The icon's sprite index within the <paramref name="texture" />.</param>
	/// <param name="value">The attribute's current value.</param>
	/// <param name="description">The attribute's translated display name.</param>
	public BuffAttributeDisplay(Func<Texture2D> texture, int spriteIndex, Func<Buff, float> value, Func<float, string> description)
	{
		Texture = texture;
		SpriteIndex = spriteIndex;
		Value = value;
		Description = description;
	}

	/// <summary>Construct an instance for a standard buff attribute.</summary>
	/// <param name="spriteIndex">The icon's sprite index within <see cref="F:StardewValley.Game1.buffsIcons" />.</param>
	/// <param name="value">The attribute's current value.</param>
	/// <param name="descriptionKey">The translation key for the attribute's display name.</param>
	public BuffAttributeDisplay(int spriteIndex, Func<BuffEffects, NetFloat> value, string descriptionKey)
	{
		Texture = () => Game1.buffsIcons;
		SpriteIndex = spriteIndex;
		Value = (Buff buff) => value(buff.effects).Value;
		Description = delegate(float buffValue)
		{
			string text = ((buffValue > 0f) ? ("+" + buffValue) : (buffValue.ToString() ?? ""));
			string text2 = Game1.content.LoadString(descriptionKey);
			LocalizedContentManager.LanguageCode currentLanguageCode = LocalizedContentManager.CurrentLanguageCode;
			return (currentLanguageCode == LocalizedContentManager.LanguageCode.ja || currentLanguageCode == LocalizedContentManager.LanguageCode.es || currentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? (text2 + text) : (text + text2);
		};
	}
}
