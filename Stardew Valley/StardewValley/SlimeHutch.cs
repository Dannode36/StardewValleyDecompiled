using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Tools;

namespace StardewValley;

public class SlimeHutch : DecoratableLocation
{
	[XmlElement("slimeMatingsLeft")]
	public readonly NetInt slimeMatingsLeft = new NetInt();

	public readonly NetArray<bool, NetBool> waterSpots = new NetArray<bool, NetBool>(4);

	protected int _slimeCapacity = -1;

	public SlimeHutch()
	{
	}

	public SlimeHutch(string m, string name)
		: base(m, name)
	{
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(slimeMatingsLeft, "slimeMatingsLeft").AddField(waterSpots, "waterSpots");
	}

	/// <inheritdoc />
	public override void OnParentBuildingUpgraded(Building building)
	{
		base.OnParentBuildingUpgraded(building);
		_slimeCapacity = -1;
	}

	public bool isFull()
	{
		if (_slimeCapacity < 0)
		{
			_slimeCapacity = GetContainingBuilding()?.GetData()?.MaxOccupants ?? 20;
		}
		return characters.Count >= _slimeCapacity;
	}

	public override bool canSlimeMateHere()
	{
		int matesLeft = slimeMatingsLeft;
		slimeMatingsLeft.Value--;
		if (!isFull())
		{
			return matesLeft > 0;
		}
		return false;
	}

	public override bool canSlimeHatchHere()
	{
		return !isFull();
	}

	public override void DayUpdate(int dayOfMonth)
	{
		int waters = 0;
		int startIndex = Game1.random.Next(waterSpots.Length);
		for (int i = 0; i < waterSpots.Length; i++)
		{
			if (waterSpots[(i + startIndex) % waterSpots.Length] && waters * 5 < characters.Count)
			{
				waters++;
				waterSpots[(i + startIndex) % waterSpots.Length] = false;
			}
		}
		foreach (Object sprinkler in objects.Values)
		{
			if (!sprinkler.IsSprinkler())
			{
				continue;
			}
			foreach (Vector2 v in sprinkler.GetSprinklerTiles())
			{
				if (v.X == 16f && v.Y >= 6f && v.Y <= 9f)
				{
					waterSpots[(int)v.Y - 6] = true;
				}
			}
		}
		for (int numSlimeBalls = Math.Min(characters.Count / 5, waters); numSlimeBalls > 0; numSlimeBalls--)
		{
			int tries = 50;
			Vector2 tile = getRandomTile();
			while ((!CanItemBePlacedHere(tile, itemIsPassable: false, CollisionMask.All, CollisionMask.None) || doesTileHaveProperty((int)tile.X, (int)tile.Y, "NPCBarrier", "Back") != null || tile.Y >= 12f) && tries > 0)
			{
				tile = getRandomTile();
				tries--;
			}
			if (tries > 0)
			{
				Object slimeBall = ItemRegistry.Create<Object>("(BC)56");
				slimeBall.fragility.Value = 2;
				objects.Add(tile, slimeBall);
			}
		}
		while ((int)slimeMatingsLeft > 0)
		{
			if (characters.Count > 1 && !isFull() && characters[Game1.random.Next(characters.Count)] is GreenSlime mate && (int)mate.ageUntilFullGrown <= 0)
			{
				for (int distance = 1; distance < 10; distance++)
				{
					GreenSlime otherMate = (GreenSlime)Utility.checkForCharacterWithinArea(mate.GetType(), mate.Position, this, new Rectangle((int)mate.Position.X - 64 * distance, (int)mate.Position.Y - 64 * distance, 64 * (distance * 2 + 1), 64 * (distance * 2 + 1)));
					if (otherMate != null && otherMate.cute != mate.cute && (int)otherMate.ageUntilFullGrown <= 0)
					{
						mate.mateWith(otherMate, this);
						break;
					}
				}
			}
			slimeMatingsLeft.Value--;
		}
		slimeMatingsLeft.Value = characters.Count / 5 + 1;
		base.DayUpdate(dayOfMonth);
	}

	public override void TransferDataFromSavedLocation(GameLocation l)
	{
		if (l is SlimeHutch slimeHutch)
		{
			for (int i = 0; i < waterSpots.Length; i++)
			{
				if (i < slimeHutch.waterSpots.Count)
				{
					waterSpots[i] = slimeHutch.waterSpots[i];
				}
			}
		}
		base.TransferDataFromSavedLocation(l);
	}

	protected override void resetLocalState()
	{
		base.resetLocalState();
		if (objects.ContainsKey(new Vector2(1f, 4f)))
		{
			objects[new Vector2(1f, 4f)].Fragility = 0;
		}
	}

	public override bool performToolAction(Tool t, int tileX, int tileY)
	{
		if (t is WateringCan && tileX == 16 && tileY >= 6 && tileY <= 9)
		{
			waterSpots[tileY - 6] = true;
		}
		return false;
	}

	public override void UpdateWhenCurrentLocation(GameTime time)
	{
		base.UpdateWhenCurrentLocation(time);
		for (int i = 0; i < waterSpots.Length; i++)
		{
			if (waterSpots[i])
			{
				setMapTileIndex(16, 6 + i, 2135, "Buildings");
			}
			else
			{
				setMapTileIndex(16, 6 + i, 2134, "Buildings");
			}
		}
	}
}
