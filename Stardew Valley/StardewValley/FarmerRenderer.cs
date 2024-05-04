using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Tools;

namespace StardewValley;

[InstanceStatics]
public class FarmerRenderer : INetObject<NetFields>
{
	public enum FarmerSpriteLayers
	{
		SlingshotUp,
		ToolUp,
		Base,
		Pants,
		FaceSkin,
		Eyes,
		Shirt,
		AccessoryUnderHair,
		ArmsUp,
		HatMaskUp,
		Hair,
		Accessory,
		Hat,
		Tool,
		Arms,
		ToolDown,
		Slingshot,
		PantsPassedOut,
		SwimWaterRing,
		MAX,
		TOOL_IN_USE_SIDE
	}

	public const int sleeveDarkestColorIndex = 256;

	public const int skinDarkestColorIndex = 260;

	public const int shoeDarkestColorIndex = 268;

	public const int eyeLightestColorIndex = 276;

	public const int accessoryDrawBelowHairThreshold = 8;

	public const int accessoryFacialHairThreshold = 6;

	protected bool _sickFrame;

	public static bool isDrawingForUI = false;

	public const int TransparentSkin = -12345;

	public const int pantsOffset = 288;

	public const int armOffset = 96;

	public const int shirtXOffset = 16;

	public const int shirtYOffset = 56;

	public static int[] featureYOffsetPerFrame = new int[126]
	{
		1, 2, 2, 0, 5, 6, 1, 2, 2, 1,
		0, 2, 0, 1, 1, 0, 2, 2, 3, 3,
		2, 2, 1, 1, 0, 0, 2, 2, 4, 4,
		0, 0, 1, 2, 1, 1, 1, 1, 0, 0,
		1, 1, 1, 0, 0, -2, -1, 1, 1, 0,
		-1, -2, -1, -1, 5, 4, 0, 0, 3, 2,
		-1, 0, 4, 2, 0, 0, 2, 1, 0, -1,
		1, -2, 0, 0, 1, 1, 1, 1, 1, 1,
		0, 0, 0, 0, 1, -1, -1, -1, -1, 1,
		1, 0, 0, 0, 0, 4, 1, 0, 1, 2,
		1, 0, 1, 0, 1, 2, -3, -4, -1, 0,
		0, 2, 1, -4, -1, 0, 0, -3, 0, 0,
		-1, 0, 0, 2, 1, 1
	};

	public static int[] featureXOffsetPerFrame = new int[126]
	{
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, -1, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, -1,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, -1, -1, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 4, 0, 0, 0, 0, -1, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, -1, 0, 0,
		0, 0, 0, 0, 0, 0
	};

	public static int[] hairstyleHatOffset = new int[16]
	{
		0, 0, 0, 4, 0, 0, 3, 0, 4, 0,
		0, 0, 0, 0, 0, 0
	};

	public static Texture2D hairStylesTexture;

	public static Texture2D shirtsTexture;

	public static Texture2D hatsTexture;

	public static Texture2D accessoriesTexture;

	public static Texture2D pantsTexture;

	public static Dictionary<string, Dictionary<int, List<int>>> recolorOffsets;

	[XmlElement("textureName")]
	public readonly NetString textureName = new NetString();

	[XmlIgnore]
	private LocalizedContentManager farmerTextureManager;

	[XmlIgnore]
	internal Texture2D baseTexture;

	[XmlElement("heightOffset")]
	public readonly NetInt heightOffset = new NetInt(0);

	[XmlIgnore]
	private readonly NetColor eyes = new NetColor();

	[XmlIgnore]
	private readonly NetInt skin = new NetInt();

	[XmlIgnore]
	private readonly NetString shoes = new NetString();

	[XmlIgnore]
	private readonly NetString shirt = new NetString();

	[XmlIgnore]
	private readonly NetString pants = new NetString();

	protected bool _spriteDirty;

	protected bool _baseTextureDirty;

	protected bool _eyesDirty;

	protected bool _skinDirty;

	protected bool _shoesDirty;

	protected bool _shirtDirty;

	protected bool _pantsDirty;

	private Rectangle shirtSourceRect;

	private Rectangle hairstyleSourceRect;

	private Rectangle hatSourceRect;

	private Rectangle accessorySourceRect;

	internal Vector2 rotationAdjustment;

	internal Vector2 positionOffset;

	[XmlIgnore]
	public NetFields NetFields { get; } = new NetFields("FarmerRenderer");


	public FarmerRenderer()
	{
		NetFields.SetOwner(this).AddField(textureName, "textureName").AddField(heightOffset, "heightOffset")
			.AddField(eyes, "eyes")
			.AddField(skin, "skin")
			.AddField(shoes, "shoes")
			.AddField(shirt, "shirt")
			.AddField(pants, "pants");
		farmerTextureManager = Game1.content.CreateTemporary();
		textureName.fieldChangeVisibleEvent += delegate
		{
			_spriteDirty = true;
			_baseTextureDirty = true;
		};
		eyes.fieldChangeVisibleEvent += delegate
		{
			_spriteDirty = true;
			_eyesDirty = true;
		};
		skin.fieldChangeVisibleEvent += delegate
		{
			_spriteDirty = true;
			_skinDirty = true;
			_shirtDirty = true;
		};
		shoes.fieldChangeVisibleEvent += delegate
		{
			_spriteDirty = true;
			_shoesDirty = true;
		};
		shirt.fieldChangeVisibleEvent += delegate
		{
			_spriteDirty = true;
			_shirtDirty = true;
		};
		pants.fieldChangeVisibleEvent += delegate
		{
			_spriteDirty = true;
			_pantsDirty = true;
		};
		_spriteDirty = true;
		_baseTextureDirty = true;
	}

	public FarmerRenderer(string textureName, Farmer farmer)
		: this()
	{
		eyes.Set(farmer.newEyeColor.Value);
		this.textureName.Set(textureName);
		_spriteDirty = true;
		_baseTextureDirty = true;
	}

	public bool isAccessoryFacialHair(int which)
	{
		if (which >= 6)
		{
			if (which >= 19)
			{
				return which <= 22;
			}
			return false;
		}
		return true;
	}

	public bool drawAccessoryBelowHair(int which)
	{
		if (which >= 8)
		{
			return isAccessoryFacialHair(which);
		}
		return true;
	}

	private void executeRecolorActions(Farmer farmer)
	{
		if (_spriteDirty)
		{
			_spriteDirty = false;
			if (_baseTextureDirty)
			{
				_baseTextureDirty = false;
				textureChanged();
				_eyesDirty = true;
				_shoesDirty = true;
				_pantsDirty = true;
				_skinDirty = true;
				_shirtDirty = true;
			}
			if (recolorOffsets == null)
			{
				recolorOffsets = new Dictionary<string, Dictionary<int, List<int>>>();
			}
			if (!recolorOffsets.ContainsKey(textureName))
			{
				recolorOffsets[textureName.Value] = new Dictionary<int, List<int>>();
				Texture2D source_texture = farmerTextureManager.Load<Texture2D>(textureName.Value);
				Color[] source_pixel_data = new Color[source_texture.Width * source_texture.Height];
				source_texture.GetData(source_pixel_data);
				_GeneratePixelIndices(256, textureName, source_pixel_data);
				_GeneratePixelIndices(257, textureName, source_pixel_data);
				_GeneratePixelIndices(258, textureName, source_pixel_data);
				_GeneratePixelIndices(268, textureName, source_pixel_data);
				_GeneratePixelIndices(269, textureName, source_pixel_data);
				_GeneratePixelIndices(270, textureName, source_pixel_data);
				_GeneratePixelIndices(271, textureName, source_pixel_data);
				_GeneratePixelIndices(260, textureName, source_pixel_data);
				_GeneratePixelIndices(261, textureName, source_pixel_data);
				_GeneratePixelIndices(262, textureName, source_pixel_data);
				_GeneratePixelIndices(276, textureName, source_pixel_data);
				_GeneratePixelIndices(277, textureName, source_pixel_data);
			}
			Color[] pixel_data = new Color[baseTexture.Width * baseTexture.Height];
			baseTexture.GetData(pixel_data);
			if (_eyesDirty)
			{
				_eyesDirty = false;
				ApplyEyeColor(textureName, pixel_data);
			}
			if (_skinDirty)
			{
				_skinDirty = false;
				ApplySkinColor(textureName, pixel_data);
			}
			if (_shoesDirty)
			{
				_shoesDirty = false;
				ApplyShoeColor(textureName, pixel_data);
			}
			if (_shirtDirty)
			{
				_shirtDirty = false;
				ApplySleeveColor(textureName, pixel_data, farmer);
			}
			if (_pantsDirty)
			{
				_pantsDirty = false;
			}
			baseTexture.SetData(pixel_data);
		}
	}

	protected void _GeneratePixelIndices(int source_color_index, string texture_name, Color[] pixels)
	{
		Color source_color = pixels[source_color_index];
		List<int> pixel_indices = new List<int>();
		for (int i = 0; i < pixels.Length; i++)
		{
			if (pixels[i].PackedValue == source_color.PackedValue)
			{
				pixel_indices.Add(i);
			}
		}
		recolorOffsets[texture_name][source_color_index] = pixel_indices;
	}

	public void unload()
	{
		farmerTextureManager.Unload();
		farmerTextureManager.Dispose();
	}

	private void textureChanged()
	{
		if (baseTexture != null)
		{
			baseTexture.Dispose();
			baseTexture = null;
		}
		Texture2D source_texture = farmerTextureManager.Load<Texture2D>(textureName.Value);
		baseTexture = new Texture2D(Game1.graphics.GraphicsDevice, source_texture.Width, source_texture.Height);
		Color[] data = new Color[source_texture.Width * source_texture.Height];
		source_texture.GetData(data, 0, data.Length);
		baseTexture.SetData(data);
	}

	public void recolorEyes(Color lightestColor)
	{
		eyes.Set(lightestColor);
	}

	private void ApplyEyeColor(string texture_name, Color[] pixels)
	{
		Color lightest_color = eyes.Value;
		Color darker_color = changeBrightness(lightest_color, -75);
		if (lightest_color.Equals(darker_color))
		{
			lightest_color.B += 10;
		}
		_SwapColor(texture_name, pixels, 276, lightest_color);
		_SwapColor(texture_name, pixels, 277, darker_color);
	}

	private void _SwapColor(string texture_name, Color[] pixels, int color_index, Color color)
	{
		List<int> pixels_offsets = recolorOffsets[texture_name][color_index];
		for (int i = 0; i < pixels_offsets.Count; i++)
		{
			pixels[pixels_offsets[i]] = color;
		}
	}

	public void recolorShoes(string which)
	{
		shoes.Set(which);
	}

	private void ApplyShoeColor(string texture_name, Color[] pixels)
	{
		int which = 12;
		Texture2D texture = null;
		if (shoes.Value.Contains(':'))
		{
			string texture_path = shoes.Value.Substring(0, shoes.Value.LastIndexOf(":"));
			string index = shoes.Value.Substring(shoes.Value.LastIndexOf(":") + 1);
			try
			{
				texture = farmerTextureManager.Load<Texture2D>(texture_path);
				if (!int.TryParse(index, out which))
				{
					which = 12;
				}
			}
			catch (Exception)
			{
				texture = farmerTextureManager.Load<Texture2D>("Characters\\Farmer\\shoeColors");
			}
		}
		else if (!int.TryParse(shoes.Value, out which))
		{
			which = 12;
		}
		if (texture == null)
		{
			texture = farmerTextureManager.Load<Texture2D>("Characters\\Farmer\\shoeColors");
		}
		Texture2D shoeColors = texture;
		if (which >= shoeColors.Height)
		{
			which = shoeColors.Height - 1;
		}
		if (shoeColors.Width >= 4)
		{
			Color[] shoeColorsData = new Color[shoeColors.Width * shoeColors.Height];
			shoeColors.GetData(shoeColorsData);
			Color darkest = shoeColorsData[which * 4 % (shoeColors.Height * 4)];
			Color medium = shoeColorsData[which * 4 % (shoeColors.Height * 4) + 1];
			Color lightest = shoeColorsData[which * 4 % (shoeColors.Height * 4) + 2];
			Color lightest2 = shoeColorsData[which * 4 % (shoeColors.Height * 4) + 3];
			_SwapColor(texture_name, pixels, 268, darkest);
			_SwapColor(texture_name, pixels, 269, medium);
			_SwapColor(texture_name, pixels, 270, lightest);
			_SwapColor(texture_name, pixels, 271, lightest2);
		}
	}

	public int recolorSkin(int which, bool force = false)
	{
		if (force)
		{
			skin.Value = -1;
		}
		skin.Set(which);
		return which;
	}

	private void ApplySkinColor(string texture_name, Color[] pixels)
	{
		int which = skin.Value;
		Texture2D skinColors = farmerTextureManager.Load<Texture2D>("Characters\\Farmer\\skinColors");
		Color[] skinColorsData = new Color[skinColors.Width * skinColors.Height];
		if (which < 0)
		{
			which = skinColors.Height - 1;
		}
		if (which > skinColors.Height - 1)
		{
			which = 0;
		}
		skinColors.GetData(skinColorsData);
		Color darkest = skinColorsData[which * 3 % (skinColors.Height * 3)];
		Color medium = skinColorsData[which * 3 % (skinColors.Height * 3) + 1];
		Color lightest = skinColorsData[which * 3 % (skinColors.Height * 3) + 2];
		if (skin.Value == -12345)
		{
			darkest = (medium = (lightest = Color.Transparent));
		}
		_SwapColor(texture_name, pixels, 260, darkest);
		_SwapColor(texture_name, pixels, 261, medium);
		_SwapColor(texture_name, pixels, 262, lightest);
	}

	public void changeShirt(string whichShirt)
	{
		shirt.Set(whichShirt);
	}

	public void changePants(string whichPants)
	{
		pants.Set(whichPants);
	}

	public void MarkSpriteDirty()
	{
		_spriteDirty = true;
		_shirtDirty = true;
		_pantsDirty = true;
		_eyesDirty = true;
		_shoesDirty = true;
		_baseTextureDirty = true;
	}

	public void ApplySleeveColor(string texture_name, Color[] pixels, Farmer who)
	{
		who.GetDisplayShirt(out var shirtTexture, out var shirtIndex);
		Color[] shirtData = new Color[shirtTexture.Bounds.Width * shirtTexture.Bounds.Height];
		shirtTexture.GetData(shirtData);
		int index = shirtIndex * 8 / 128 * 32 * shirtTexture.Bounds.Width + shirtIndex * 8 % 128 + shirtTexture.Width * 4;
		int dye_index = index + 128;
		if (!who.ShirtHasSleeves() || index >= shirtData.Length || (skin.Value == -12345 && who.shirtItem.Value == null))
		{
			Texture2D skinColors = farmerTextureManager.Load<Texture2D>("Characters\\Farmer\\skinColors");
			Color[] skinColorsData = new Color[skinColors.Width * skinColors.Height];
			int skin_index = skin.Value;
			if (skin_index < 0)
			{
				skin_index = skinColors.Height - 1;
			}
			if (skin_index > skinColors.Height - 1)
			{
				skin_index = 0;
			}
			skinColors.GetData(skinColorsData);
			Color darkest = skinColorsData[skin_index * 3 % (skinColors.Height * 3)];
			Color medium = skinColorsData[skin_index * 3 % (skinColors.Height * 3) + 1];
			Color lightest = skinColorsData[skin_index * 3 % (skinColors.Height * 3) + 2];
			if (skin.Value == -12345)
			{
				darkest = pixels[260 + baseTexture.Width * 2];
				medium = pixels[261 + baseTexture.Width * 2];
				lightest = pixels[262 + baseTexture.Width * 2];
			}
			if (_sickFrame)
			{
				darkest = pixels[260 + baseTexture.Width];
				medium = pixels[261 + baseTexture.Width];
				lightest = pixels[262 + baseTexture.Width];
			}
			_SwapColor(texture_name, pixels, 256, darkest);
			_SwapColor(texture_name, pixels, 257, medium);
			_SwapColor(texture_name, pixels, 258, lightest);
		}
		else
		{
			Color color = Utility.MakeCompletelyOpaque(who.GetShirtColor());
			Color shirtSleeveColor = shirtData[dye_index];
			Color clothes_color = color;
			if (shirtSleeveColor.A < byte.MaxValue)
			{
				shirtSleeveColor = shirtData[index];
				clothes_color = Color.White;
			}
			shirtSleeveColor = Utility.MultiplyColor(shirtSleeveColor, clothes_color);
			_SwapColor(texture_name, pixels, 256, shirtSleeveColor);
			shirtSleeveColor = shirtData[dye_index - shirtTexture.Width];
			if (shirtSleeveColor.A < byte.MaxValue)
			{
				shirtSleeveColor = shirtData[index - shirtTexture.Width];
				clothes_color = Color.White;
			}
			shirtSleeveColor = Utility.MultiplyColor(shirtSleeveColor, clothes_color);
			_SwapColor(texture_name, pixels, 257, shirtSleeveColor);
			shirtSleeveColor = shirtData[dye_index - shirtTexture.Width * 2];
			if (shirtSleeveColor.A < byte.MaxValue)
			{
				shirtSleeveColor = shirtData[index - shirtTexture.Width * 2];
				clothes_color = Color.White;
			}
			shirtSleeveColor = Utility.MultiplyColor(shirtSleeveColor, clothes_color);
			_SwapColor(texture_name, pixels, 258, shirtSleeveColor);
		}
	}

	private static Color changeBrightness(Color c, int brightness)
	{
		c.R = (byte)Math.Min(255, Math.Max(0, c.R + brightness));
		c.G = (byte)Math.Min(255, Math.Max(0, c.G + brightness));
		c.B = (byte)Math.Min(255, Math.Max(0, c.B + ((brightness > 0) ? (brightness * 5 / 6) : (brightness * 8 / 7))));
		return c;
	}

	public void draw(SpriteBatch b, Farmer who, int whichFrame, Vector2 position, float layerDepth = 1f, bool flip = false)
	{
		who.FarmerSprite.setCurrentSingleFrame(whichFrame, 32000, secondaryArm: false, flip);
		draw(b, who.FarmerSprite, who.FarmerSprite.SourceRect, position, Vector2.Zero, layerDepth, Color.White, 0f, who);
	}

	public void draw(SpriteBatch b, FarmerSprite farmerSprite, Rectangle sourceRect, Vector2 position, Vector2 origin, float layerDepth, Color overrideColor, float rotation, Farmer who)
	{
		draw(b, farmerSprite.CurrentAnimationFrame, farmerSprite.CurrentFrame, sourceRect, position, origin, layerDepth, overrideColor, rotation, 1f, who);
	}

	public void drawMiniPortrat(SpriteBatch b, Vector2 position, float layerDepth, float scale, int facingDirection, Farmer who)
	{
		int hair_style = who.getHair(ignore_hat: true);
		HairStyleMetadata hair_metadata = Farmer.GetHairStyleMetadata(who.hair.Value);
		executeRecolorActions(who);
		facingDirection = 2;
		bool flip = false;
		int yOffset = 0;
		int feature_y_offset = 0;
		Texture2D hair_texture = hairStylesTexture;
		hairstyleSourceRect = new Rectangle(hair_style * 16 % hairStylesTexture.Width, hair_style * 16 / hairStylesTexture.Width * 96, 16, 15);
		if (hair_metadata != null)
		{
			hair_texture = hair_metadata.texture;
			hairstyleSourceRect = new Rectangle(hair_metadata.tileX * 16, hair_metadata.tileY * 16, 16, 15);
		}
		switch (facingDirection)
		{
		case 0:
			yOffset = 64;
			hairstyleSourceRect.Offset(0, 64);
			feature_y_offset = featureYOffsetPerFrame[12];
			break;
		case 3:
			if (hair_metadata != null && hair_metadata.usesUniqueLeftSprite)
			{
				flip = false;
				yOffset = 96;
			}
			else
			{
				flip = true;
				yOffset = 32;
			}
			hairstyleSourceRect.Offset(0, 32);
			feature_y_offset = featureYOffsetPerFrame[6];
			break;
		case 1:
			yOffset = 32;
			hairstyleSourceRect.Offset(0, 32);
			feature_y_offset = featureYOffsetPerFrame[6];
			break;
		case 2:
			yOffset = 0;
			hairstyleSourceRect.Offset(0, 0);
			feature_y_offset = featureYOffsetPerFrame[0];
			break;
		}
		b.Draw(baseTexture, position, new Rectangle(0, yOffset, 16, who.IsMale ? 15 : 16), Color.White, 0f, Vector2.Zero, scale, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, GetLayerDepth(layerDepth, FarmerSpriteLayers.Base));
		Color hair_color = who.hairstyleColor.Value;
		if ((bool)who.prismaticHair)
		{
			hair_color = Utility.GetPrismaticColor();
		}
		b.Draw(hair_texture, position + new Vector2(0f, feature_y_offset * 4 + ((who.IsMale && (int)who.hair >= 16) ? (-4) : ((!who.IsMale && (int)who.hair < 16) ? 4 : 0))) * scale / 4f, hairstyleSourceRect, hair_color, 0f, Vector2.Zero, scale, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, GetLayerDepth(layerDepth, FarmerSpriteLayers.Hair));
	}

	public void draw(SpriteBatch b, FarmerSprite.AnimationFrame animationFrame, int currentFrame, Rectangle sourceRect, Vector2 position, Vector2 origin, float layerDepth, Color overrideColor, float rotation, float scale, Farmer who)
	{
		draw(b, animationFrame, currentFrame, sourceRect, position, origin, layerDepth, who.FacingDirection, overrideColor, rotation, scale, who);
	}

	public void drawHairAndAccesories(SpriteBatch b, int facingDirection, Farmer who, Vector2 position, Vector2 origin, float scale, int currentFrame, float rotation, Color overrideColor, float layerDepth)
	{
		int hair_style = who.getHair();
		HairStyleMetadata hair_metadata = Farmer.GetHairStyleMetadata(hair_style);
		if (who != null && who.hat.Value?.hairDrawType.Value == 1 && hair_metadata != null && hair_metadata.coveredIndex != -1)
		{
			hair_style = hair_metadata.coveredIndex;
			hair_metadata = Farmer.GetHairStyleMetadata(hair_style);
		}
		executeRecolorActions(who);
		Color hair_color = who.hairstyleColor.Value;
		if ((bool)who.prismaticHair)
		{
			hair_color = Utility.GetPrismaticColor();
		}
		who.GetDisplayShirt(out var shirtTexture, out var shirtIndex);
		shirtSourceRect = new Rectangle(shirtIndex * 8 % 128, shirtIndex * 8 / 128 * 32, 8, 8);
		Texture2D hair_texture = hairStylesTexture;
		hairstyleSourceRect = new Rectangle(hair_style * 16 % hairStylesTexture.Width, hair_style * 16 / hairStylesTexture.Width * 96, 16, 32);
		if (hair_metadata != null)
		{
			hair_texture = hair_metadata.texture;
			hairstyleSourceRect = new Rectangle(hair_metadata.tileX * 16, hair_metadata.tileY * 16, 16, 32);
		}
		if ((int)who.accessory >= 0)
		{
			accessorySourceRect = new Rectangle((int)who.accessory * 16 % accessoriesTexture.Width, (int)who.accessory * 16 / accessoriesTexture.Width * 32, 16, 16);
		}
		Texture2D hatTexture = hatsTexture;
		bool isErrorHat = false;
		if (who.hat.Value != null)
		{
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(who.hat.Value.QualifiedItemId);
			int spriteIndex = itemData.SpriteIndex;
			hatTexture = itemData.GetTexture();
			hatSourceRect = new Rectangle(20 * spriteIndex % hatTexture.Width, 20 * spriteIndex / hatTexture.Width * 20 * 4, 20, 20);
			if (itemData.IsErrorItem)
			{
				hatSourceRect = itemData.GetSourceRect();
				isErrorHat = true;
			}
		}
		FarmerSpriteLayers accessoryLayer = FarmerSpriteLayers.Accessory;
		if ((int)who.accessory >= 0 && drawAccessoryBelowHair(who.accessory))
		{
			accessoryLayer = FarmerSpriteLayers.AccessoryUnderHair;
		}
		switch (facingDirection)
		{
		case 0:
		{
			shirtSourceRect.Offset(0, 24);
			hairstyleSourceRect.Offset(0, 64);
			Rectangle dyed_shirt_source_rect = shirtSourceRect;
			dyed_shirt_source_rect.Offset(128, 0);
			if (!isErrorHat && who.hat.Value != null)
			{
				hatSourceRect.Offset(0, 60);
			}
			if (!who.bathingClothes && (skin.Value != -12345 || who.shirtItem.Value != null))
			{
				b.Draw(shirtTexture, position + origin + positionOffset + new Vector2(16f * scale + (float)(featureXOffsetPerFrame[currentFrame] * 4), (float)(56 + featureYOffsetPerFrame[currentFrame] * 4) + (float)(int)heightOffset * scale), shirtSourceRect, overrideColor.Equals(Color.White) ? Color.White : overrideColor, rotation, origin, 4f * scale, SpriteEffects.None, GetLayerDepth(layerDepth, FarmerSpriteLayers.Shirt));
				b.Draw(shirtTexture, position + origin + positionOffset + new Vector2(16f * scale + (float)(featureXOffsetPerFrame[currentFrame] * 4), (float)(56 + featureYOffsetPerFrame[currentFrame] * 4) + (float)(int)heightOffset * scale), dyed_shirt_source_rect, overrideColor.Equals(Color.White) ? Utility.MakeCompletelyOpaque(who.GetShirtColor()) : overrideColor, rotation, origin, 4f * scale, SpriteEffects.None, GetLayerDepth(layerDepth, FarmerSpriteLayers.Shirt, dyeLayer: true));
			}
			b.Draw(hair_texture, position + origin + positionOffset + new Vector2(featureXOffsetPerFrame[currentFrame] * 4, featureYOffsetPerFrame[currentFrame] * 4 + 4 + ((who.IsMale && hair_style >= 16) ? (-4) : ((!who.IsMale && hair_style < 16) ? 4 : 0))), hairstyleSourceRect, overrideColor.Equals(Color.White) ? hair_color : overrideColor, rotation, origin, 4f * scale, SpriteEffects.None, GetLayerDepth(layerDepth, FarmerSpriteLayers.Hair));
			break;
		}
		case 1:
		{
			shirtSourceRect.Offset(0, 8);
			hairstyleSourceRect.Offset(0, 32);
			Rectangle dyed_shirt_source_rect = shirtSourceRect;
			dyed_shirt_source_rect.Offset(128, 0);
			if (!isErrorHat && who.hat.Value != null)
			{
				hatSourceRect.Offset(0, 20);
			}
			if (rotation == -(float)Math.PI / 32f)
			{
				rotationAdjustment.X = 6f;
				rotationAdjustment.Y = -2f;
			}
			else if (rotation == (float)Math.PI / 32f)
			{
				rotationAdjustment.X = -6f;
				rotationAdjustment.Y = 1f;
			}
			if (!who.bathingClothes && (skin.Value != -12345 || who.shirtItem.Value != null))
			{
				b.Draw(shirtTexture, position + origin + positionOffset + rotationAdjustment + new Vector2(16f * scale + (float)(featureXOffsetPerFrame[currentFrame] * 4), 56f * scale + (float)(featureYOffsetPerFrame[currentFrame] * 4) + (float)(int)heightOffset * scale), shirtSourceRect, overrideColor.Equals(Color.White) ? Color.White : overrideColor, rotation, origin, 4f * scale, SpriteEffects.None, GetLayerDepth(layerDepth, FarmerSpriteLayers.Shirt));
				b.Draw(shirtTexture, position + origin + positionOffset + rotationAdjustment + new Vector2(16f * scale + (float)(featureXOffsetPerFrame[currentFrame] * 4), 56f * scale + (float)(featureYOffsetPerFrame[currentFrame] * 4) + (float)(int)heightOffset * scale), dyed_shirt_source_rect, overrideColor.Equals(Color.White) ? Utility.MakeCompletelyOpaque(who.GetShirtColor()) : overrideColor, rotation, origin, 4f * scale, SpriteEffects.None, GetLayerDepth(layerDepth, FarmerSpriteLayers.Shirt, dyeLayer: true));
			}
			if ((int)who.accessory >= 0)
			{
				accessorySourceRect.Offset(0, 16);
				b.Draw(accessoriesTexture, position + origin + positionOffset + rotationAdjustment + new Vector2(featureXOffsetPerFrame[currentFrame] * 4, 4 + featureYOffsetPerFrame[currentFrame] * 4 + (int)heightOffset), accessorySourceRect, (overrideColor.Equals(Color.White) && isAccessoryFacialHair(who.accessory)) ? hair_color : overrideColor, rotation, origin, 4f * scale, SpriteEffects.None, GetLayerDepth(layerDepth, accessoryLayer));
			}
			b.Draw(hair_texture, position + origin + positionOffset + new Vector2(featureXOffsetPerFrame[currentFrame] * 4, featureYOffsetPerFrame[currentFrame] * 4 + ((who.IsMale && (int)who.hair >= 16) ? (-4) : ((!who.IsMale && (int)who.hair < 16) ? 4 : 0))), hairstyleSourceRect, overrideColor.Equals(Color.White) ? hair_color : overrideColor, rotation, origin, 4f * scale, SpriteEffects.None, GetLayerDepth(layerDepth, FarmerSpriteLayers.Hair));
			break;
		}
		case 2:
		{
			Rectangle dyed_shirt_source_rect = shirtSourceRect;
			dyed_shirt_source_rect.Offset(128, 0);
			if (!who.bathingClothes && (skin.Value != -12345 || who.shirtItem.Value != null))
			{
				b.Draw(shirtTexture, position + origin + positionOffset + new Vector2(16 + featureXOffsetPerFrame[currentFrame] * 4, (float)(56 + featureYOffsetPerFrame[currentFrame] * 4) + (float)(int)heightOffset * scale), shirtSourceRect, overrideColor.Equals(Color.White) ? Color.White : overrideColor, rotation, origin, 4f * scale, SpriteEffects.None, GetLayerDepth(layerDepth, FarmerSpriteLayers.Shirt));
				b.Draw(shirtTexture, position + origin + positionOffset + new Vector2(16 + featureXOffsetPerFrame[currentFrame] * 4, (float)(56 + featureYOffsetPerFrame[currentFrame] * 4) + (float)(int)heightOffset * scale), dyed_shirt_source_rect, overrideColor.Equals(Color.White) ? Utility.MakeCompletelyOpaque(who.GetShirtColor()) : overrideColor, rotation, origin, 4f * scale, SpriteEffects.None, GetLayerDepth(layerDepth, FarmerSpriteLayers.Shirt, dyeLayer: true));
			}
			if ((int)who.accessory >= 0)
			{
				if ((int)who.accessory == 26)
				{
					switch (currentFrame)
					{
					case 24:
					case 25:
					case 26:
					case 70:
						positionOffset.Y += 4f;
						break;
					}
				}
				b.Draw(accessoriesTexture, position + origin + positionOffset + rotationAdjustment + new Vector2(featureXOffsetPerFrame[currentFrame] * 4, 8 + featureYOffsetPerFrame[currentFrame] * 4 + (int)heightOffset - 4), accessorySourceRect, (overrideColor.Equals(Color.White) && isAccessoryFacialHair(who.accessory)) ? hair_color : overrideColor, rotation, origin, 4f * scale, SpriteEffects.None, GetLayerDepth(layerDepth, accessoryLayer));
			}
			b.Draw(hair_texture, position + origin + positionOffset + new Vector2(featureXOffsetPerFrame[currentFrame] * 4, featureYOffsetPerFrame[currentFrame] * 4 + ((who.IsMale && (int)who.hair >= 16) ? (-4) : ((!who.IsMale && (int)who.hair < 16) ? 4 : 0))), hairstyleSourceRect, overrideColor.Equals(Color.White) ? hair_color : overrideColor, rotation, origin, 4f * scale, SpriteEffects.None, GetLayerDepth(layerDepth, FarmerSpriteLayers.Hair));
			break;
		}
		case 3:
		{
			bool flip = true;
			shirtSourceRect.Offset(0, 16);
			Rectangle dyed_shirt_source_rect = shirtSourceRect;
			dyed_shirt_source_rect.Offset(128, 0);
			if (hair_metadata != null && hair_metadata.usesUniqueLeftSprite)
			{
				flip = false;
				hairstyleSourceRect.Offset(0, 96);
			}
			else
			{
				hairstyleSourceRect.Offset(0, 32);
			}
			if (!isErrorHat && who.hat.Value != null)
			{
				hatSourceRect.Offset(0, 40);
			}
			if (rotation == -(float)Math.PI / 32f)
			{
				rotationAdjustment.X = 6f;
				rotationAdjustment.Y = -2f;
			}
			else if (rotation == (float)Math.PI / 32f)
			{
				rotationAdjustment.X = -5f;
				rotationAdjustment.Y = 1f;
			}
			if (!who.bathingClothes && (skin.Value != -12345 || who.shirtItem.Value != null))
			{
				b.Draw(shirtTexture, position + origin + positionOffset + rotationAdjustment + new Vector2(16 - featureXOffsetPerFrame[currentFrame] * 4, 56 + featureYOffsetPerFrame[currentFrame] * 4 + (int)heightOffset), shirtSourceRect, overrideColor.Equals(Color.White) ? Color.White : overrideColor, rotation, origin, 4f * scale, SpriteEffects.None, GetLayerDepth(layerDepth, FarmerSpriteLayers.Shirt));
				b.Draw(shirtTexture, position + origin + positionOffset + rotationAdjustment + new Vector2(16 - featureXOffsetPerFrame[currentFrame] * 4, 56 + featureYOffsetPerFrame[currentFrame] * 4 + (int)heightOffset), dyed_shirt_source_rect, overrideColor.Equals(Color.White) ? Utility.MakeCompletelyOpaque(who.GetShirtColor()) : overrideColor, rotation, origin, 4f * scale, SpriteEffects.None, GetLayerDepth(layerDepth, FarmerSpriteLayers.Shirt, dyeLayer: true));
			}
			if ((int)who.accessory >= 0)
			{
				accessorySourceRect.Offset(0, 16);
				b.Draw(accessoriesTexture, position + origin + positionOffset + rotationAdjustment + new Vector2(-featureXOffsetPerFrame[currentFrame] * 4, 4 + featureYOffsetPerFrame[currentFrame] * 4 + (int)heightOffset), accessorySourceRect, (overrideColor.Equals(Color.White) && isAccessoryFacialHair(who.accessory)) ? hair_color : overrideColor, rotation, origin, 4f * scale, SpriteEffects.FlipHorizontally, GetLayerDepth(layerDepth, accessoryLayer));
			}
			b.Draw(hair_texture, position + origin + positionOffset + new Vector2(-featureXOffsetPerFrame[currentFrame] * 4, featureYOffsetPerFrame[currentFrame] * 4 + ((who.IsMale && (int)who.hair >= 16) ? (-4) : ((!who.IsMale && (int)who.hair < 16) ? 4 : 0))), hairstyleSourceRect, overrideColor.Equals(Color.White) ? hair_color : overrideColor, rotation, origin, 4f * scale, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, GetLayerDepth(layerDepth, FarmerSpriteLayers.Hair));
			break;
		}
		}
		if (who.hat.Value != null && !who.bathingClothes)
		{
			bool flip = who.FarmerSprite.CurrentAnimationFrame.flip;
			if (!isErrorHat && who.hat.Value.isMask && facingDirection == 0)
			{
				Rectangle mask_draw_rect = hatSourceRect;
				mask_draw_rect.Height -= 11;
				mask_draw_rect.Y += 11;
				b.Draw(hatTexture, position + origin + positionOffset + new Vector2(0f, 44f) + new Vector2(-8 + ((!flip) ? 1 : (-1)) * featureXOffsetPerFrame[currentFrame] * 4, -16 + featureYOffsetPerFrame[currentFrame] * 4 + ((!who.hat.Value.ignoreHairstyleOffset) ? hairstyleHatOffset[(int)who.hair % 16] : 0) + 4 + (int)heightOffset), mask_draw_rect, overrideColor, rotation, origin, 4f * scale, SpriteEffects.None, GetLayerDepth(layerDepth, FarmerSpriteLayers.Hat));
				mask_draw_rect = hatSourceRect;
				mask_draw_rect.Height = 11;
				b.Draw(hatTexture, position + origin + positionOffset + new Vector2(-8 + ((!flip) ? 1 : (-1)) * featureXOffsetPerFrame[currentFrame] * 4, -16 + featureYOffsetPerFrame[currentFrame] * 4 + ((!who.hat.Value.ignoreHairstyleOffset) ? hairstyleHatOffset[(int)who.hair % 16] : 0) + 4 + (int)heightOffset), mask_draw_rect, who.hat.Value.isPrismatic ? Utility.GetPrismaticColor() : overrideColor, rotation, origin, 4f * scale, SpriteEffects.None, GetLayerDepth(layerDepth, FarmerSpriteLayers.HatMaskUp));
			}
			else
			{
				b.Draw(hatTexture, position + origin + positionOffset + new Vector2(-8 + ((!flip) ? 1 : (-1)) * featureXOffsetPerFrame[currentFrame] * 4, -16 + featureYOffsetPerFrame[currentFrame] * 4 + ((!who.hat.Value.ignoreHairstyleOffset) ? hairstyleHatOffset[(int)who.hair % 16] : 0) + 4 + (int)heightOffset), hatSourceRect, who.hat.Value.isPrismatic ? Utility.GetPrismaticColor() : overrideColor, rotation, origin, 4f * scale, SpriteEffects.None, GetLayerDepth(layerDepth, FarmerSpriteLayers.Hat));
			}
		}
	}

	public static float GetLayerDepth(float baseLayerDepth, FarmerSpriteLayers layer, bool dyeLayer = false)
	{
		if (layer == FarmerSpriteLayers.TOOL_IN_USE_SIDE)
		{
			return baseLayerDepth + 0.0032f;
		}
		int sortDirection = ((!Game1.isUsingBackToFrontSorting) ? 1 : (-1));
		if (dyeLayer)
		{
			baseLayerDepth += 1E-07f * (float)sortDirection;
		}
		return baseLayerDepth + (float)layer * 1E-06f * (float)sortDirection;
	}

	public void draw(SpriteBatch b, FarmerSprite.AnimationFrame animationFrame, int currentFrame, Rectangle sourceRect, Vector2 position, Vector2 origin, float layerDepth, int facingDirection, Color overrideColor, float rotation, float scale, Farmer who)
	{
		bool sick_frame = currentFrame == 104 || currentFrame == 105;
		if (_sickFrame != sick_frame)
		{
			_sickFrame = sick_frame;
			_shirtDirty = true;
			_spriteDirty = true;
		}
		executeRecolorActions(who);
		position = new Vector2((float)Math.Floor(position.X), (float)Math.Floor(position.Y));
		rotationAdjustment = Vector2.Zero;
		positionOffset.Y = animationFrame.positionOffset * 4;
		positionOffset.X = animationFrame.xOffset * 4;
		if (!isDrawingForUI && (bool)who.swimming)
		{
			sourceRect.Height /= 2;
			sourceRect.Height -= (int)who.yOffset / 4;
			position.Y += 64f;
		}
		if (facingDirection == 3 || facingDirection == 1)
		{
			facingDirection = ((!animationFrame.flip) ? 1 : 3);
		}
		b.Draw(baseTexture, position + origin + positionOffset, sourceRect, overrideColor, rotation, origin, 4f * scale, animationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, GetLayerDepth(layerDepth, FarmerSpriteLayers.Base));
		if (!isDrawingForUI && (bool)who.swimming)
		{
			if (who.currentEyes != 0 && who.FacingDirection != 0 && (Game1.timeOfDay < 2600 || (who.isInBed.Value && who.timeWentToBed.Value != 0)) && ((!who.FarmerSprite.PauseForSingleAnimation && !who.UsingTool) || (who.UsingTool && who.CurrentTool is FishingRod)))
			{
				b.Draw(baseTexture, position + origin + positionOffset + new Vector2(featureXOffsetPerFrame[currentFrame] * 4 + 20 + ((who.FacingDirection == 1) ? 12 : ((who.FacingDirection == 3) ? 4 : 0)), featureYOffsetPerFrame[currentFrame] * 4 + 40), new Rectangle(5, 16, (who.FacingDirection == 2) ? 6 : 2, 2), overrideColor, 0f, origin, 4f * scale, SpriteEffects.None, GetLayerDepth(layerDepth, FarmerSpriteLayers.FaceSkin));
				b.Draw(baseTexture, position + origin + positionOffset + new Vector2(featureXOffsetPerFrame[currentFrame] * 4 + 20 + ((who.FacingDirection == 1) ? 12 : ((who.FacingDirection == 3) ? 4 : 0)), featureYOffsetPerFrame[currentFrame] * 4 + 40), new Rectangle(264 + ((who.FacingDirection == 3) ? 4 : 0), 2 + (who.currentEyes - 1) * 2, (who.FacingDirection == 2) ? 6 : 2, 2), overrideColor, 0f, origin, 4f * scale, SpriteEffects.None, GetLayerDepth(layerDepth, FarmerSpriteLayers.Eyes));
			}
			drawHairAndAccesories(b, facingDirection, who, position, origin, scale, currentFrame, rotation, overrideColor, layerDepth);
			b.Draw(Game1.staminaRect, new Rectangle((int)position.X + (int)who.yOffset + 8, (int)position.Y - 128 + sourceRect.Height * 4 + (int)origin.Y - (int)who.yOffset, sourceRect.Width * 4 - (int)who.yOffset * 2 - 16, 4), Game1.staminaRect.Bounds, Color.White * 0.75f, 0f, Vector2.Zero, SpriteEffects.None, GetLayerDepth(layerDepth, FarmerSpriteLayers.SwimWaterRing));
			return;
		}
		who.GetDisplayPants(out var texture, out var pantsIndex);
		Rectangle pants_rect = new Rectangle(sourceRect.X, sourceRect.Y, sourceRect.Width, sourceRect.Height);
		pants_rect.X += pantsIndex % 10 * 192;
		pants_rect.Y += pantsIndex / 10 * 688;
		if (!who.IsMale)
		{
			pants_rect.X += 96;
		}
		if (skin.Value != -12345 || who.pantsItem.Value != null)
		{
			b.Draw(texture, position + origin + positionOffset, pants_rect, overrideColor.Equals(Color.White) ? Utility.MakeCompletelyOpaque(who.GetPantsColor()) : overrideColor, rotation, origin, 4f * scale, animationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, GetLayerDepth(layerDepth, (who.FarmerSprite.CurrentAnimationFrame.frame == 5) ? FarmerSpriteLayers.PantsPassedOut : FarmerSpriteLayers.Pants));
		}
		sourceRect.Offset(288, 0);
		if (who.currentEyes != 0 && facingDirection != 0 && (Game1.timeOfDay < 2600 || (who.isInBed.Value && who.timeWentToBed.Value != 0)) && ((!who.FarmerSprite.PauseForSingleAnimation && !who.UsingTool) || (who.UsingTool && who.CurrentTool is FishingRod)) && (!who.UsingTool || !(who.CurrentTool is FishingRod { isFishing: false })))
		{
			int x_adjustment = 5;
			x_adjustment = (animationFrame.flip ? (x_adjustment - featureXOffsetPerFrame[currentFrame]) : (x_adjustment + featureXOffsetPerFrame[currentFrame]));
			switch (facingDirection)
			{
			case 1:
				x_adjustment += 3;
				break;
			case 3:
				x_adjustment++;
				break;
			}
			x_adjustment *= 4;
			b.Draw(baseTexture, position + origin + positionOffset + new Vector2(x_adjustment, featureYOffsetPerFrame[currentFrame] * 4 + ((who.IsMale && who.FacingDirection != 2) ? 36 : 40)), new Rectangle(5, 16, (facingDirection == 2) ? 6 : 2, 2), overrideColor, 0f, origin, 4f * scale, SpriteEffects.None, GetLayerDepth(layerDepth, FarmerSpriteLayers.FaceSkin));
			b.Draw(baseTexture, position + origin + positionOffset + new Vector2(x_adjustment, featureYOffsetPerFrame[currentFrame] * 4 + ((who.FacingDirection == 1 || who.FacingDirection == 3) ? 40 : 44)), new Rectangle(264 + ((facingDirection == 3) ? 4 : 0), 2 + (who.currentEyes - 1) * 2, (facingDirection == 2) ? 6 : 2, 2), overrideColor, 0f, origin, 4f * scale, SpriteEffects.None, GetLayerDepth(layerDepth, FarmerSpriteLayers.Eyes));
		}
		drawHairAndAccesories(b, facingDirection, who, position, origin, scale, currentFrame, rotation, overrideColor, layerDepth);
		FarmerSpriteLayers armLayer = FarmerSpriteLayers.Arms;
		if (facingDirection == 0)
		{
			armLayer = FarmerSpriteLayers.ArmsUp;
		}
		if (animationFrame.armOffset > 0)
		{
			sourceRect.Offset(-288 + animationFrame.armOffset * 16, 0);
			b.Draw(baseTexture, position + origin + positionOffset + who.armOffset, sourceRect, overrideColor, rotation, origin, 4f * scale, animationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, GetLayerDepth(layerDepth, armLayer));
		}
		if (!who.usingSlingshot || !(who.CurrentTool is Slingshot slingshot))
		{
			return;
		}
		Point point = Utility.Vector2ToPoint(slingshot.AdjustForHeight(Utility.PointToVector2(slingshot.aimPos.Value)));
		int mouseX = point.X;
		int y = point.Y;
		int backArmDistance = slingshot.GetBackArmDistance(who);
		Vector2 shoot_origin = slingshot.GetShootOrigin(who);
		float frontArmRotation = (float)Math.Atan2((float)y - shoot_origin.Y, (float)mouseX - shoot_origin.X) + (float)Math.PI;
		if (!Game1.options.useLegacySlingshotFiring)
		{
			frontArmRotation -= (float)Math.PI;
			if (frontArmRotation < 0f)
			{
				frontArmRotation += (float)Math.PI * 2f;
			}
		}
		switch (facingDirection)
		{
		case 0:
			b.Draw(baseTexture, position + new Vector2(4f + frontArmRotation * 8f, -44f), new Rectangle(173, 238, 9, 14), Color.White, 0f, new Vector2(4f, 11f), 4f * scale, SpriteEffects.None, GetLayerDepth(layerDepth, FarmerSpriteLayers.SlingshotUp));
			break;
		case 1:
		{
			b.Draw(baseTexture, position + new Vector2(52 - backArmDistance, -32f), new Rectangle(147, 237, 10, 4), Color.White, 0f, new Vector2(8f, 3f), 4f * scale, SpriteEffects.None, GetLayerDepth(layerDepth, FarmerSpriteLayers.Slingshot));
			b.Draw(baseTexture, position + new Vector2(36f, -44f), new Rectangle(156, 244, 9, 10), Color.White, frontArmRotation, new Vector2(0f, 3f), 4f * scale, SpriteEffects.None, GetLayerDepth(layerDepth, FarmerSpriteLayers.SlingshotUp));
			int slingshotAttachX = (int)(Math.Cos(frontArmRotation + (float)Math.PI / 2f) * (double)(20 - backArmDistance - 8) - Math.Sin(frontArmRotation + (float)Math.PI / 2f) * -68.0);
			int slingshotAttachY = (int)(Math.Sin(frontArmRotation + (float)Math.PI / 2f) * (double)(20 - backArmDistance - 8) + Math.Cos(frontArmRotation + (float)Math.PI / 2f) * -68.0);
			Utility.drawLineWithScreenCoordinates((int)(position.X + 52f - (float)backArmDistance), (int)(position.Y - 32f - 4f), (int)(position.X + 32f + (float)(slingshotAttachX / 2)), (int)(position.Y - 32f - 12f + (float)(slingshotAttachY / 2)), b, Color.White);
			break;
		}
		case 3:
		{
			b.Draw(baseTexture, position + new Vector2(40 + backArmDistance, -32f), new Rectangle(147, 237, 10, 4), Color.White, 0f, new Vector2(9f, 4f), 4f * scale, SpriteEffects.FlipHorizontally, GetLayerDepth(layerDepth, FarmerSpriteLayers.Slingshot));
			b.Draw(baseTexture, position + new Vector2(24f, -40f), new Rectangle(156, 244, 9, 10), Color.White, frontArmRotation + (float)Math.PI, new Vector2(8f, 3f), 4f * scale, SpriteEffects.FlipHorizontally, GetLayerDepth(layerDepth, FarmerSpriteLayers.SlingshotUp));
			int slingshotAttachX = (int)(Math.Cos(frontArmRotation + (float)Math.PI * 2f / 5f) * (double)(20 + backArmDistance - 8) - Math.Sin(frontArmRotation + (float)Math.PI * 2f / 5f) * -68.0);
			int slingshotAttachY = (int)(Math.Sin(frontArmRotation + (float)Math.PI * 2f / 5f) * (double)(20 + backArmDistance - 8) + Math.Cos(frontArmRotation + (float)Math.PI * 2f / 5f) * -68.0);
			Utility.drawLineWithScreenCoordinates((int)(position.X + 4f + (float)backArmDistance), (int)(position.Y - 32f - 8f), (int)(position.X + 26f + (float)slingshotAttachX * 4f / 10f), (int)(position.Y - 32f - 8f + (float)slingshotAttachY * 4f / 10f), b, Color.White);
			break;
		}
		case 2:
			b.Draw(baseTexture, position + new Vector2(4f, -32 - backArmDistance / 2), new Rectangle(148, 244, 4, 4), Color.White, 0f, Vector2.Zero, 4f * scale, SpriteEffects.None, GetLayerDepth(layerDepth, FarmerSpriteLayers.Arms));
			Utility.drawLineWithScreenCoordinates((int)(position.X + 16f), (int)(position.Y - 28f - (float)(backArmDistance / 2)), (int)(position.X + 44f - frontArmRotation * 10f), (int)(position.Y - 16f - 8f), b, Color.White);
			Utility.drawLineWithScreenCoordinates((int)(position.X + 16f), (int)(position.Y - 28f - (float)(backArmDistance / 2)), (int)(position.X + 56f - frontArmRotation * 10f), (int)(position.Y - 16f - 8f), b, Color.White);
			b.Draw(baseTexture, position + new Vector2(44f - frontArmRotation * 10f, -16f), new Rectangle(167, 235, 7, 9), Color.White, 0f, new Vector2(3f, 5f), 4f * scale, SpriteEffects.None, GetLayerDepth(layerDepth, FarmerSpriteLayers.Slingshot, dyeLayer: true));
			break;
		}
	}
}
