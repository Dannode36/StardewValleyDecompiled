using System;

namespace StardewValley.Characters;

/// <summary>Obsolete. This is only kept to preserve data from old save files. All cats now use the <see cref="T:StardewValley.Characters.Pet" /> class instead.</summary>
[Obsolete("All cats now use the Pet class.")]
public class Cat : Pet
{
	public Cat()
	{
		Sprite = new AnimatedSprite(getPetTextureName(), 0, 32, 32);
		base.HideShadow = true;
		base.Breather = false;
		base.willDestroyObjectsUnderfoot = false;
	}
}
