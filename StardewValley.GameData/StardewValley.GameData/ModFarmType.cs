using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData;

/// <summary>The metadata for a custom farm layout which can be selected by players.</summary>
public class ModFarmType
{
	/// <summary>A key which uniquely identifies this farm type. The ID should only contain alphanumeric/underscore/dot characters. This should be prefixed with your mod ID like <c>Example.ModId_FarmType.</c></summary>
	public string Id;

	/// <summary>Where to get the translatable farm name and description. This must be a key in the form <c>{asset name}:{key}</c>; for example, <c>Strings/UI:Farm_Description</c> will get it from the <c>Farm_Description</c> entry in the <c>Strings/UI</c> file. The translated text must be in the form <c>{name}_{description}</c>.</summary>
	public string TooltipStringPath;

	/// <summary>The map asset name relative to the game's <c>Content/Maps</c> folder.</summary>
	public string MapName;

	/// <summary>The asset name for a 22x20 pixel icon texture, shown on the 'New Game' and co-op join screens. Defaults to the standard farm type's icon.</summary>
	[ContentSerializer(Optional = true)]
	public string IconTexture;

	/// <summary>The asset name for a 131x61 pixel texture that's drawn over the farm area in the in-game world map. Defaults to the standard farm type's texture.</summary>
	[ContentSerializer(Optional = true)]
	public string WorldMapTexture;

	/// <summary>Whether monsters should spawn by default on this farm map. This affects the initial value of the advanced option during save creation, which the player can change.</summary>
	[ContentSerializer(Optional = true)]
	public bool SpawnMonstersByDefault;

	/// <summary>Mod-specific metadata for the farm type.</summary>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> ModData;

	/// <summary>Custom fields ignored by the base game, for use by mods.</summary>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> CustomFields;
}
