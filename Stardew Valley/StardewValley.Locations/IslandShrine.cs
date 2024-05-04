using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Network;
using StardewValley.Objects;

namespace StardewValley.Locations;

public class IslandShrine : IslandForestLocation
{
	[XmlIgnore]
	public ItemPedestal northPedestal;

	[XmlIgnore]
	public ItemPedestal southPedestal;

	[XmlIgnore]
	public ItemPedestal eastPedestal;

	[XmlIgnore]
	public ItemPedestal westPedestal;

	[XmlIgnore]
	public NetEvent0 puzzleFinishedEvent = new NetEvent0();

	[XmlElement("puzzleFinished")]
	public NetBool puzzleFinished = new NetBool();

	public IslandShrine()
	{
	}

	public IslandShrine(string map, string name)
		: base(map, name)
	{
		AddMissingPedestals();
	}

	public override List<Vector2> GetAdditionalWalnutBushes()
	{
		return new List<Vector2>
		{
			new Vector2(23f, 34f)
		};
	}

	public ItemPedestal AddOrUpdatePedestal(Vector2 position, string birdLocation)
	{
		ItemPedestal pedestal = getObjectAtTile((int)position.X, (int)position.Y) as ItemPedestal;
		string itemId = IslandGemBird.GetItemIndex(IslandGemBird.GetBirdTypeForLocation(birdLocation));
		if (pedestal == null || !pedestal.isIslandShrinePedestal.Value)
		{
			OverlaidDictionary overlaidDictionary = objects;
			Vector2 key = position;
			ItemPedestal itemPedestal = new ItemPedestal(position, null, lock_on_success: false, Color.White);
			itemPedestal.Fragility = 2;
			itemPedestal.isIslandShrinePedestal.Value = true;
			pedestal = itemPedestal;
			overlaidDictionary[key] = itemPedestal;
		}
		pedestal.successColor.Value = Color.Transparent;
		if (pedestal.requiredItem.Value?.ItemId != itemId)
		{
			pedestal.requiredItem.Value = new Object(itemId, 1);
			if (pedestal.heldObject.Value?.ItemId != itemId)
			{
				pedestal.heldObject.Value = null;
			}
		}
		return pedestal;
	}

	public virtual void AddMissingPedestals()
	{
		westPedestal = AddOrUpdatePedestal(new Vector2(21f, 27f), "IslandWest");
		eastPedestal = AddOrUpdatePedestal(new Vector2(27f, 27f), "IslandEast");
		southPedestal = AddOrUpdatePedestal(new Vector2(24f, 28f), "IslandSouth");
		northPedestal = AddOrUpdatePedestal(new Vector2(24f, 25f), "IslandNorth");
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(puzzleFinished, "puzzleFinished").AddField(puzzleFinishedEvent, "puzzleFinishedEvent");
		puzzleFinishedEvent.onEvent += OnPuzzleFinish;
	}

	protected override void resetLocalState()
	{
		base.resetLocalState();
		if (Game1.IsMasterGame)
		{
			AddMissingPedestals();
		}
	}

	public override void MakeMapModifications(bool force = false)
	{
		base.MakeMapModifications(force);
		if (puzzleFinished.Value)
		{
			ApplyFinishedTiles();
		}
	}

	public override void TransferDataFromSavedLocation(GameLocation l)
	{
		base.TransferDataFromSavedLocation(l);
		if (l is IslandShrine shrine)
		{
			northPedestal = shrine.getObjectAtTile((int)northPedestal.TileLocation.X, (int)northPedestal.TileLocation.Y) as ItemPedestal;
			southPedestal = shrine.getObjectAtTile((int)southPedestal.TileLocation.X, (int)southPedestal.TileLocation.Y) as ItemPedestal;
			eastPedestal = shrine.getObjectAtTile((int)eastPedestal.TileLocation.X, (int)eastPedestal.TileLocation.Y) as ItemPedestal;
			westPedestal = shrine.getObjectAtTile((int)westPedestal.TileLocation.X, (int)westPedestal.TileLocation.Y) as ItemPedestal;
			puzzleFinished.Value = shrine.puzzleFinished.Value;
		}
	}

	public void OnPuzzleFinish()
	{
		if (Game1.IsMasterGame)
		{
			for (int i = 0; i < 5; i++)
			{
				Game1.createItemDebris(ItemRegistry.Create("(O)73"), new Vector2(24f, 19f) * 64f, -1, this);
			}
		}
		if (Game1.currentLocation == this)
		{
			Game1.playSound("boulderBreak");
			Game1.playSound("secret1");
			Game1.flashAlpha = 1f;
			ApplyFinishedTiles();
		}
	}

	public virtual void ApplyFinishedTiles()
	{
		setMapTileIndex(23, 19, 142, "AlwaysFront", 2);
		setMapTileIndex(24, 19, 143, "AlwaysFront", 2);
		setMapTileIndex(25, 19, 144, "AlwaysFront", 2);
	}

	public override void UpdateWhenCurrentLocation(GameTime time)
	{
		base.UpdateWhenCurrentLocation(time);
		if (Game1.IsMasterGame && !puzzleFinished.Value && northPedestal.match.Value && southPedestal.match.Value && eastPedestal.match.Value && westPedestal.match.Value)
		{
			Game1.player.team.MarkCollectedNut("IslandShrinePuzzle");
			puzzleFinishedEvent.Fire();
			puzzleFinished.Value = true;
			northPedestal.locked.Value = true;
			northPedestal.heldObject.Value = null;
			southPedestal.locked.Value = true;
			southPedestal.heldObject.Value = null;
			eastPedestal.locked.Value = true;
			eastPedestal.heldObject.Value = null;
			westPedestal.locked.Value = true;
			westPedestal.heldObject.Value = null;
		}
	}
}
