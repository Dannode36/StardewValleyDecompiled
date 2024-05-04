namespace StardewValley.Tools;

/// <summary>A generic tool instance with no logic of its own, used for cases where the logic is applied elsewhere.</summary>
public class GenericTool : Tool
{
	/// <inheritdoc />
	protected override Item GetOneNew()
	{
		return new GenericTool();
	}
}
