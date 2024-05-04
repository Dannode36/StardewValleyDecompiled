using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using StardewValley.Extensions;
using StardewValley.Monsters;
using xTile.Layers;

namespace StardewValley.Locations;

public class BugLand : GameLocation
{
	[XmlElement("hasSpawnedBugsToday")]
	public bool hasSpawnedBugsToday;

	public BugLand()
	{
	}

	public BugLand(string map, string name)
		: base(map, name)
	{
	}

	public override void TransferDataFromSavedLocation(GameLocation l)
	{
		if (l is BugLand bugLand)
		{
			hasSpawnedBugsToday = bugLand.hasSpawnedBugsToday;
		}
		base.TransferDataFromSavedLocation(l);
	}

	public override void hostSetup()
	{
		base.hostSetup();
		if (Game1.IsMasterGame && !hasSpawnedBugsToday)
		{
			InitializeBugLand();
		}
	}

	public override void DayUpdate(int dayOfMonth)
	{
		base.DayUpdate(dayOfMonth);
		for (int i = 0; i < characters.Count; i++)
		{
			if (characters[i] is Grub || characters[i] is Fly)
			{
				characters.RemoveAt(i);
				i--;
			}
		}
		hasSpawnedBugsToday = false;
	}

	public virtual void InitializeBugLand()
	{
		if (hasSpawnedBugsToday)
		{
			return;
		}
		hasSpawnedBugsToday = true;
		Layer pathsLayer = map.RequireLayer("Paths");
		for (int x = 0; x < map.Layers[0].LayerWidth; x++)
		{
			for (int y = 0; y < map.Layers[0].LayerHeight; y++)
			{
				if (!(Game1.random.NextDouble() < 0.33))
				{
					continue;
				}
				int tileIndex = pathsLayer.GetTileIndexAt(x, y);
				if (tileIndex == -1)
				{
					continue;
				}
				Vector2 tile = new Vector2(x, y);
				switch (tileIndex)
				{
				case 13:
				case 14:
				case 15:
					if (!objects.ContainsKey(tile))
					{
						objects.Add(tile, ItemRegistry.Create<Object>(GameLocation.getWeedForSeason(Game1.random, Season.Spring)));
					}
					break;
				case 16:
					if (!objects.ContainsKey(tile))
					{
						objects.Add(tile, ItemRegistry.Create<Object>(Game1.random.Choose("(O)343", "(O)450")));
					}
					break;
				case 17:
					if (!objects.ContainsKey(tile))
					{
						objects.Add(tile, ItemRegistry.Create<Object>(Game1.random.Choose("(O)343", "(O)450")));
					}
					break;
				case 18:
					if (!objects.ContainsKey(tile))
					{
						objects.Add(tile, ItemRegistry.Create<Object>(Game1.random.Choose("(O)294", "(O)295")));
					}
					break;
				case 28:
					if (CanSpawnCharacterHere(tile) && characters.Count < 50)
					{
						characters.Add(new Grub(new Vector2(tile.X * 64f, tile.Y * 64f), hard: true));
					}
					break;
				}
			}
		}
	}
}
