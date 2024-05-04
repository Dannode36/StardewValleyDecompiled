using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Buildings;

/// <summary>As part of <see cref="T:StardewValley.GameData.Buildings.BuildingData" />, a texture to draw over or behind the building.</summary>
public class BuildingDrawLayer
{
	/// <summary>A key which uniquely identifies this entry within the list. The ID should only contain alphanumeric/underscore/dot characters. For custom entries, this should be prefixed with your mod ID like <c>Example.ModId_DrawLayerId</c>.</summary>
	public string Id;

	/// <summary>The asset name of the texture to draw. Defaults to the building's <see cref="F:StardewValley.GameData.Buildings.BuildingData.Texture" /> field.</summary>
	[ContentSerializer(Optional = true)]
	public string Texture;

	/// <summary>The pixel area within the texture to draw. If the overlay is animated via <see cref="F:StardewValley.GameData.Buildings.BuildingDrawLayer.FrameCount" />, this is the area of the first frame.</summary>
	public Rectangle SourceRect = Rectangle.Empty;

	/// <summary>The tile position at which to draw the top-left corner of the texture, relative to the building's top-left corner tile.</summary>
	public Vector2 DrawPosition;

	/// <summary>Whether to draw the texture behind the building sprite (i.e. underlay) instead of over it.</summary>
	[ContentSerializer(Optional = true)]
	public bool DrawInBackground;

	/// <summary>A Y tile offset applied when figuring out render layering. For example, a value of 2.5 will treat the texture as if it was 2.5 tiles further up the screen for the purposes of layering.</summary>
	[ContentSerializer(Optional = true)]
	public float SortTileOffset;

	/// <summary>The name of a chest defined in the <see cref="F:StardewValley.GameData.Buildings.BuildingData.Chests" /> field which must contain items. If it's empty, this overlay won't be rendered. Default none.</summary>
	[ContentSerializer(Optional = true)]
	public string OnlyDrawIfChestHasContents;

	/// <summary>The number of milliseconds each animation frame is displayed on-screen before switching to the next, if <see cref="F:StardewValley.GameData.Buildings.BuildingDrawLayer.FrameCount" /> is more than one.</summary>
	[ContentSerializer(Optional = true)]
	public int FrameDuration = 90;

	/// <summary>The number of animation frames to render. If this is more than one, the building will be animated automatically based on <see cref="F:StardewValley.GameData.Buildings.BuildingDrawLayer.FramesPerRow" /> and <see cref="F:StardewValley.GameData.Buildings.BuildingDrawLayer.FrameDuration" />.</summary>
	[ContentSerializer(Optional = true)]
	public int FrameCount = 1;

	/// <summary>The number of animation frames per row in the spritesheet.</summary>
	/// <remarks>
	///   For each frame, the <see cref="F:StardewValley.GameData.Buildings.BuildingDrawLayer.SourceRect" /> will be offset by its width to the right up to <see cref="F:StardewValley.GameData.Buildings.BuildingDrawLayer.FramesPerRow" /> - 1 times, and then down by its height.
	///   For example, if you set <see cref="F:StardewValley.GameData.Buildings.BuildingDrawLayer.FrameCount" /> to 6 and <see cref="F:StardewValley.GameData.Buildings.BuildingDrawLayer.FramesPerRow" /> to 3, the building will expect the frames to be laid out like this in the spritesheet (where frame 1 matches <see cref="F:StardewValley.GameData.Buildings.BuildingDrawLayer.SourceRect" />):
	///   <code>
	///     1 2 3
	///     4 5 6
	///   </code>
	/// </remarks>
	[ContentSerializer(Optional = true)]
	public int FramesPerRow = -1;

	/// <summary>A pixel offset applied to the draw layer when the animal door is open. While the door is opening, the percentage open is applied to the offset (e.g. 50% open = 50% offset).</summary>
	[ContentSerializer(Optional = true)]
	public Point AnimalDoorOffset = Point.Zero;

	/// <summary>Get the parsed <see cref="F:StardewValley.GameData.Buildings.BuildingDrawLayer.SourceRect" /> adjusted for the current game time, accounting for <see cref="F:StardewValley.GameData.Buildings.BuildingDrawLayer.FrameCount" />.</summary>
	/// <param name="time">The total milliseconds elapsed since the game started.</param>
	public Rectangle GetSourceRect(int time)
	{
		Rectangle sourceRect = SourceRect;
		time /= FrameDuration;
		time %= FrameCount;
		if (FramesPerRow < 0)
		{
			sourceRect.X += sourceRect.Width * time;
		}
		else
		{
			sourceRect.X += sourceRect.Width * (time % FramesPerRow);
			sourceRect.Y += sourceRect.Height * (time / FramesPerRow);
		}
		return sourceRect;
	}
}
