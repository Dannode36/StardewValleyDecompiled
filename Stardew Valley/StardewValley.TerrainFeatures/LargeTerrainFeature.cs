using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;

namespace StardewValley.TerrainFeatures;

[XmlInclude(typeof(Bush))]
public abstract class LargeTerrainFeature : TerrainFeature
{
	/// <summary>The backing field for <see cref="P:StardewValley.TerrainFeatures.LargeTerrainFeature.Tile" />.</summary>
	[XmlElement("tilePosition")]
	public readonly NetVector2 netTilePosition = new NetVector2();

	public bool isDestroyedByNPCTrample;

	/// <inheritdoc />
	[XmlIgnore]
	public override Vector2 Tile
	{
		get
		{
			return netTilePosition.Value;
		}
		set
		{
			netTilePosition.Value = value;
		}
	}

	protected LargeTerrainFeature(bool needsTick)
		: base(needsTick)
	{
	}

	public override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(netTilePosition, "netTilePosition");
	}

	public virtual void onDestroy()
	{
	}
}
