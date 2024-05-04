using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Shops;

/// <summary>A visual theme to apply to the UI, or <c>null</c> for the default theme.</summary>
public class ShopThemeData
{
	/// <summary>A game state query which indicates whether this theme should be applied. Defaults to always applied.</summary>
	[ContentSerializer(Optional = true)]
	public string Condition;

	/// <summary>The name of the texture to load for the shop window border, or <c>null</c> for the default shop texture.</summary>
	[ContentSerializer(Optional = true)]
	public string WindowBorderTexture;

	/// <summary>The pixel area within the <see cref="F:StardewValley.GameData.Shops.ShopThemeData.WindowBorderTexture" /> for the shop window border, or <c>null</c> for the default shop texture. This should be an 18x18 pixel area.</summary>
	[ContentSerializer(Optional = true)]
	public Rectangle? WindowBorderSourceRect;

	/// <summary>The name of the texture to load for the NPC portrait background, or <c>null</c> for the default shop texture.</summary>
	[ContentSerializer(Optional = true)]
	public string PortraitBackgroundTexture;

	/// <summary>The pixel area within the <see cref="F:StardewValley.GameData.Shops.ShopThemeData.PortraitBackgroundTexture" /> for the NPC portrait background, or <c>null</c> for the default shop texture. This should be a 74x47 pixel area.</summary>
	[ContentSerializer(Optional = true)]
	public Rectangle? PortraitBackgroundSourceRect;

	/// <summary>The name of the texture to load for the NPC dialogue background, or <c>null</c> for the default shop texture.</summary>
	[ContentSerializer(Optional = true)]
	public string DialogueBackgroundTexture;

	/// <summary>The pixel area within the <see cref="F:StardewValley.GameData.Shops.ShopThemeData.DialogueBackgroundTexture" /> for the NPC dialogue background, or <c>null</c> for the default shop texture. This should be a 60x60 pixel area.</summary>
	[ContentSerializer(Optional = true)]
	public Rectangle? DialogueBackgroundSourceRect;

	/// <summary>The sprite text color for the dialogue text, or <c>null</c> for the default color. This can be a MonoGame color field name (like <c>ForestGreen</c>), RGB hex code (like <c>#AABBCC</c>), or RGBA hex code (like <c>#AABBCCDD</c>).</summary>
	[ContentSerializer(Optional = true)]
	public string DialogueColor;

	/// <summary>The sprite text shadow color for the dialogue text shadow, or <c>null</c> for the default color.</summary>
	[ContentSerializer(Optional = true)]
	public string DialogueShadowColor;

	/// <summary>The name of the texture to load for the item row background, or <c>null</c> for the default shop texture.</summary>
	[ContentSerializer(Optional = true)]
	public string ItemRowBackgroundTexture;

	/// <summary>The pixel area within the <see cref="F:StardewValley.GameData.Shops.ShopThemeData.ItemRowBackgroundTexture" /> for the item row background, or <c>null</c> for the default shop texture. This should be a 15x15 pixel area.</summary>
	[ContentSerializer(Optional = true)]
	public Rectangle? ItemRowBackgroundSourceRect;

	/// <summary>The color tint to apply to the item row background when the cursor is hovering over it, or <c>White</c> for no tint, or <c>null</c> for the default color. This can be a MonoGame color field name (like <c>ForestGreen</c>), RGB hex code (like <c>#AABBCC</c>), or RGBA hex code (like <c>#AABBCCDD</c>).</summary>
	[ContentSerializer(Optional = true)]
	public string ItemRowBackgroundHoverColor;

	/// <summary>The sprite text color for the item text, or <c>null</c> for the default color. This can be a MonoGame color field name (like <c>ForestGreen</c>), RGB hex code (like <c>#AABBCC</c>), or RGBA hex code (like <c>#AABBCCDD</c>).</summary>
	[ContentSerializer(Optional = true)]
	public string ItemRowTextColor;

	/// <summary>The name of the texture to load for the box behind the item icons, or <c>null</c> for the default shop texture.</summary>
	[ContentSerializer(Optional = true)]
	public string ItemIconBackgroundTexture;

	/// <summary>The pixel area within the <see cref="F:StardewValley.GameData.Shops.ShopThemeData.ItemIconBackgroundTexture" /> for the item icon background, or <c>null</c> for the default shop texture. This should be an 18x18 pixel area.</summary>
	[ContentSerializer(Optional = true)]
	public Rectangle? ItemIconBackgroundSourceRect;

	/// <summary>The name of the texture to load for the scroll up icon, or <c>null</c> for the default shop texture.</summary>
	[ContentSerializer(Optional = true)]
	public string ScrollUpTexture;

	/// <summary>The pixel area within the <see cref="F:StardewValley.GameData.Shops.ShopThemeData.ScrollUpTexture" /> for the scroll up icon, or <c>null</c> for the default shop texture. This should be an 11x12 pixel area.</summary>
	[ContentSerializer(Optional = true)]
	public Rectangle? ScrollUpSourceRect;

	/// <summary>The name of the texture to load for the scroll down icon, or <c>null</c> for the default shop texture.</summary>
	[ContentSerializer(Optional = true)]
	public string ScrollDownTexture;

	/// <summary>The pixel area within the <see cref="F:StardewValley.GameData.Shops.ShopThemeData.ScrollDownTexture" /> for the scroll down icon, or <c>null</c> for the default shop texture. This should be an 11x12 pixel area.</summary>
	[ContentSerializer(Optional = true)]
	public Rectangle? ScrollDownSourceRect;

	/// <summary>The name of the texture to load for the scrollbar foreground texture, or <c>null</c> for the default shop texture.</summary>
	[ContentSerializer(Optional = true)]
	public string ScrollBarFrontTexture;

	/// <summary>The pixel area within the <see cref="F:StardewValley.GameData.Shops.ShopThemeData.ScrollBarFrontTexture" /> for the scroll foreground, or <c>null</c> for the default shop texture. This should be a 6x10 pixel area.</summary>
	[ContentSerializer(Optional = true)]
	public Rectangle? ScrollBarFrontSourceRect;

	/// <summary>The name of the texture to load for the scrollbar background texture, or <c>null</c> for the default shop texture.</summary>
	[ContentSerializer(Optional = true)]
	public string ScrollBarBackTexture;

	/// <summary>The pixel area within the <see cref="F:StardewValley.GameData.Shops.ShopThemeData.ScrollBarBackTexture" /> for the scroll background, or <c>null</c> for the default shop texture. This should be a 6x6 pixel area.</summary>
	[ContentSerializer(Optional = true)]
	public Rectangle? ScrollBarBackSourceRect;
}
