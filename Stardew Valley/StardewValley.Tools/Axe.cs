using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Extensions;
using StardewValley.TerrainFeatures;

namespace StardewValley.Tools;

public class Axe : Tool
{
	public NetInt additionalPower = new NetInt(0);

	public Axe()
		: base("Axe", 0, 189, 215, stackable: false)
	{
	}

	/// <inheritdoc />
	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(additionalPower, "additionalPower");
	}

	/// <inheritdoc />
	protected override void MigrateLegacyItemId()
	{
		switch (base.UpgradeLevel)
		{
		case 0:
			base.ItemId = "Axe";
			break;
		case 1:
			base.ItemId = "CopperAxe";
			break;
		case 2:
			base.ItemId = "SteelAxe";
			break;
		case 3:
			base.ItemId = "GoldAxe";
			break;
		case 4:
			base.ItemId = "IridiumAxe";
			break;
		default:
			base.ItemId = "Axe";
			break;
		}
	}

	/// <inheritdoc />
	protected override Item GetOneNew()
	{
		return new Axe();
	}

	public override bool beginUsing(GameLocation location, int x, int y, Farmer who)
	{
		Update(who.FacingDirection, 0, who);
		who.EndUsingTool();
		return true;
	}

	public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
	{
		base.DoFunction(location, x, y, power, who);
		if (!isEfficient)
		{
			who.Stamina -= (float)(2 * power) - (float)who.ForagingLevel * 0.1f;
		}
		int tileX = x / 64;
		int tileY = y / 64;
		Rectangle tileRect = new Rectangle(tileX * 64, tileY * 64, 64, 64);
		Vector2 tile = new Vector2(tileX, tileY);
		if (location.Map.RequireLayer("Buildings").Tiles[tileX, tileY] != null && location.Map.RequireLayer("Buildings").Tiles[tileX, tileY].TileIndexProperties.ContainsKey("TreeStump"))
		{
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Axe.cs.14023"));
			return;
		}
		upgradeLevel.Value += additionalPower.Value;
		location.performToolAction(this, tileX, tileY);
		if (location.terrainFeatures.TryGetValue(tile, out var terrainFeature) && terrainFeature.performToolAction(this, 0, tile))
		{
			location.terrainFeatures.Remove(tile);
		}
		if (location.largeTerrainFeatures != null)
		{
			for (int i = location.largeTerrainFeatures.Count - 1; i >= 0; i--)
			{
				LargeTerrainFeature largeFeature = location.largeTerrainFeatures[i];
				if (largeFeature.getBoundingBox().Intersects(tileRect) && largeFeature.performToolAction(this, 0, tile))
				{
					location.largeTerrainFeatures.RemoveAt(i);
				}
			}
		}
		Vector2 toolTilePosition = new Vector2(tileX, tileY);
		if (location.Objects.TryGetValue(toolTilePosition, out var obj) && obj.Type != null && obj.performToolAction(this))
		{
			if (obj.Type == "Crafting" && (int)obj.fragility != 2)
			{
				location.debris.Add(new Debris(obj.QualifiedItemId, who.GetToolLocation(), Utility.PointToVector2(who.StandingPixel)));
			}
			obj.performRemoveAction();
			location.Objects.Remove(toolTilePosition);
		}
		upgradeLevel.Value -= additionalPower.Value;
	}
}
