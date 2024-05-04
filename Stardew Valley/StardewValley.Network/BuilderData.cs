using Microsoft.Xna.Framework;
using Netcode;

namespace StardewValley.Network;

public class BuilderData : INetObject<NetFields>
{
	/// <summary>The current building type (i.e. the one being constructed, or the one being upgraded from).</summary>
	public NetString buildingType = new NetString();

	/// <summary>The number of days until it's completed.</summary>
	public NetInt daysUntilBuilt = new NetInt();

	/// <summary>The unique name for the location containing the building.</summary>
	public NetString buildingLocation = new NetString();

	/// <summary>The building's tile position within the <see cref="F:StardewValley.Network.BuilderData.buildingLocation" /> location.</summary>
	public NetPoint buildingTile = new NetPoint();

	/// <summary>Whether this is an upgrade (instead of a new building being constructed).</summary>
	public NetBool isUpgrade = new NetBool();

	public NetFields NetFields { get; } = new NetFields("BuilderData");


	/// <summary>Construct an empty instance.</summary>
	public BuilderData()
	{
		NetFields.SetOwner(this).AddField(buildingType, "buildingType").AddField(daysUntilBuilt, "daysUntilBuilt")
			.AddField(buildingLocation, "buildingLocation")
			.AddField(buildingTile, "buildingTile")
			.AddField(isUpgrade, "isUpgrade");
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="buildingType">The current building type (i.e. the one being constructed, or the one being upgraded from).</param>
	/// <param name="daysUntilBuilt">The number of days until it's completed.</param>
	/// <param name="location">The unique name for the location containing the building.</param>
	/// <param name="tile">The building's tile position within the <see cref="F:StardewValley.Network.BuilderData.buildingLocation" /> location.</param>
	/// <param name="isUpgrade">Whether this is an upgrade (instead of a new building being constructed).</param>
	public BuilderData(string buildingType, int daysUntilBuilt, string location, Point tile, bool isUpgrade)
		: this()
	{
		this.buildingType.Value = buildingType;
		this.daysUntilBuilt.Value = daysUntilBuilt;
		buildingLocation.Value = location;
		buildingTile.Value = tile;
		this.isUpgrade.Value = isUpgrade;
	}
}
