using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using StardewValley.GameData.Buffs;

namespace StardewValley.GameData.Objects;

/// <summary>As part of <see cref="T:StardewValley.GameData.Objects.ObjectData" />, a buff to set when this item is eaten.</summary>
public class ObjectBuffData
{
	/// <summary>The backing field for <see cref="P:StardewValley.GameData.Objects.ObjectBuffData.Id" />.</summary>
	private string IdImpl;

	/// <summary>A unique identifier for this entry within the list.</summary>
	[ContentSerializer(Optional = true)]
	public string Id
	{
		get
		{
			return IdImpl ?? BuffId;
		}
		set
		{
			IdImpl = value;
		}
	}

	/// <summary>The buff ID to apply, or <c>null</c> to use <c>food</c> or <c>drink</c> depending on the item data.</summary>
	[ContentSerializer(Optional = true)]
	public string BuffId { get; set; }

	/// <summary>The texture to load for the buff icon, or <c>null</c> for the default icon based on the <see cref="P:StardewValley.GameData.Objects.ObjectBuffData.BuffId" /> and <see cref="P:StardewValley.GameData.Objects.ObjectBuffData.CustomAttributes" />.</summary>
	[ContentSerializer(Optional = true)]
	public string IconTexture { get; set; }

	/// <summary>The sprite index for the buff icon within the <see cref="P:StardewValley.GameData.Objects.ObjectBuffData.IconTexture" />.</summary>
	[ContentSerializer(Optional = true)]
	public int IconSpriteIndex { get; set; }

	/// <summary>The buff duration measured in in-game minutes, or <c>-2</c> for a buff that should last all day, or (if <see cref="P:StardewValley.GameData.Objects.ObjectBuffData.BuffId" /> is set) omit it to use the duration in <c>Data/Buffs</c>.</summary>
	[ContentSerializer(Optional = true)]
	public int Duration { get; set; }

	/// <summary>Whether this buff counts as a debuff, so its duration should be halved when wearing a Sturdy Ring.</summary>
	[ContentSerializer(Optional = true)]
	public bool IsDebuff { get; set; }

	/// <summary>The glow color to apply to the player, if any.</summary>
	[ContentSerializer(Optional = true)]
	public string GlowColor { get; set; }

	/// <summary>The custom buff attributes to apply, if any.</summary>
	[ContentSerializer(Optional = true)]
	public BuffAttributesData CustomAttributes { get; set; }

	/// <summary>Custom fields ignored by the base game, for use by mods.</summary>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> CustomFields { get; set; }
}
