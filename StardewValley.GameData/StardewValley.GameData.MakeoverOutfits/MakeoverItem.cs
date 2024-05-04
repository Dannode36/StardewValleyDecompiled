using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.MakeoverOutfits;

public class MakeoverItem
{
	/// <summary>The backing field for <see cref="P:StardewValley.GameData.MakeoverOutfits.MakeoverItem.Id" />.</summary>
	private string IdImpl;

	/// <summary>An ID for this entry within the list. Defaults to <see cref="P:StardewValley.GameData.MakeoverOutfits.MakeoverItem.ItemId" />.</summary>
	[ContentSerializer(Optional = true)]
	public string Id
	{
		get
		{
			return IdImpl ?? ItemId;
		}
		set
		{
			IdImpl = value;
		}
	}

	public string ItemId { get; set; }

	/// <summary>A tint color to apply to the item. This can be a MonoGame color field name (like <c>ForestGreen</c>), RGB hex code (like <c>#AABBCC</c>), RGBA hex code (like <c>#AABBCCDD</c>), or null (no tint).</summary>
	[ContentSerializer(Optional = true)]
	public string Color { get; set; }
}
