using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Buildings;

/// <summary>As part of <see cref="T:StardewValley.GameData.Buildings.BuildingData" />, a placed item in its interior to move when transitioning to an upgraded map.</summary>
public class IndoorItemMove
{
	/// <summary>A key which uniquely identifies this entry within the list. The ID should only contain alphanumeric/underscore/dot characters. For custom entries, this should be prefixed with your mod ID like <c>Example.ModId_Id</c>.</summary>
	public string Id;

	/// <summary>The tile position on which any item will be moved.</summary>
	[ContentSerializer(Optional = true)]
	public Point Source;

	/// <summary>The tile position to which to move the item.</summary>
	[ContentSerializer(Optional = true)]
	public Point Destination;

	/// <summary>The tile size of the area to move. If this is multiple tiles, the <see cref="F:StardewValley.GameData.Buildings.IndoorItemMove.Source" /> and <see cref="F:StardewValley.GameData.Buildings.IndoorItemMove.Destination" /> specify the top-left coordinate of the area.</summary>
	[ContentSerializer(Optional = true)]
	public Point Size = new Point(1, 1);

	/// <summary>If set, an item on this spot won't be moved if its item ID matches this one.</summary>
	[ContentSerializer(Optional = true)]
	public string UnlessItemId;
}
