using System;
using Microsoft.Xna.Framework;
using StardewValley.Characters;
using StardewValley.Util;

namespace StardewValley.Buildings;

public class Stable : Building
{
	public Guid HorseId
	{
		get
		{
			return id.Value;
		}
		set
		{
			id.Value = value;
		}
	}

	public Stable()
		: this(Vector2.Zero)
	{
	}

	public Stable(Vector2 tileLocation)
		: this(tileLocation, GuidHelper.NewGuid())
	{
	}

	public Stable(Vector2 tileLocation, Guid horseId)
		: base("Stable", tileLocation)
	{
		HorseId = horseId;
	}

	public override Rectangle? getSourceRectForMenu()
	{
		return new Rectangle(0, 0, texture.Value.Bounds.Width, texture.Value.Bounds.Height);
	}

	public Horse getStableHorse()
	{
		return Utility.findHorse(HorseId);
	}

	public virtual void grabHorse()
	{
		if ((int)daysOfConstructionLeft <= 0)
		{
			Horse horse = Utility.findHorse(HorseId);
			if (horse == null)
			{
				horse = new Horse(HorseId, (int)tileX + 1, (int)tileY + 1);
				GetParentLocation().characters.Add(horse);
			}
			else
			{
				Game1.warpCharacter(horse, parentLocationName.Value, new Point((int)tileX + 1, (int)tileY + 1));
			}
			horse.ownerId.Value = owner.Value;
		}
	}

	public virtual void updateHorseOwnership()
	{
		if ((int)daysOfConstructionLeft > 0)
		{
			return;
		}
		Horse horse = Utility.findHorse(HorseId);
		if (horse == null)
		{
			return;
		}
		horse.ownerId.Value = owner.Value;
		if (horse.getOwner() != null)
		{
			if (horse.getOwner().horseName.Value != null)
			{
				horse.name.Value = horse.getOwner().horseName.Value;
				horse.displayName = horse.getOwner().horseName.Value;
			}
			else
			{
				horse.name.Value = "";
				horse.displayName = "";
			}
		}
	}

	public override void dayUpdate(int dayOfMonth)
	{
		base.dayUpdate(dayOfMonth);
		grabHorse();
	}

	/// <inheritdoc />
	public override void performActionOnDemolition(GameLocation location)
	{
		base.performActionOnDemolition(location);
		Horse horse = getStableHorse();
		horse?.currentLocation?.characters.Remove(horse);
	}
}
