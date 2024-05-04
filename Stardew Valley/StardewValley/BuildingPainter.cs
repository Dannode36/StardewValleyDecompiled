using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley;

[XmlInclude(typeof(BuildingPaintColor))]
public class BuildingPainter
{
	[XmlIgnore]
	public static Dictionary<string, List<List<int>>> paintMaskLookup = new Dictionary<string, List<List<int>>>(StringComparer.OrdinalIgnoreCase);

	public static Texture2D Apply(Texture2D base_texture, string mask_path, BuildingPaintColor color)
	{
		if (!paintMaskLookup.TryGetValue(mask_path, out var paint_indices))
		{
			try
			{
				Texture2D paint_mask_texture = Game1.content.Load<Texture2D>(mask_path);
				Color[] mask_pixels = new Color[paint_mask_texture.Width * paint_mask_texture.Height];
				paint_mask_texture.GetData(mask_pixels);
				paint_indices = new List<List<int>>();
				for (int i = 0; i < 3; i++)
				{
					paint_indices.Add(new List<int>());
				}
				for (int i = 0; i < mask_pixels.Length; i++)
				{
					if (mask_pixels[i] == Color.Red)
					{
						paint_indices[0].Add(i);
					}
					else if (mask_pixels[i] == Color.Lime)
					{
						paint_indices[1].Add(i);
					}
					else if (mask_pixels[i] == Color.Blue)
					{
						paint_indices[2].Add(i);
					}
				}
				paintMaskLookup[mask_path] = paint_indices;
			}
			catch (Exception)
			{
				paintMaskLookup[mask_path] = null;
			}
		}
		if (paint_indices == null)
		{
			return null;
		}
		if (!color.RequiresRecolor())
		{
			return null;
		}
		Color[] painted_pixels = new Color[base_texture.Width * base_texture.Height];
		base_texture.GetData(painted_pixels);
		Texture2D texture2D = new Texture2D(Game1.graphics.GraphicsDevice, base_texture.Width, base_texture.Height);
		if (!color.Color1Default.Value)
		{
			_ApplyPaint(0, -100, 0, painted_pixels, paint_indices[0]);
			_ApplyPaint(color.Color1Hue.Value, color.Color1Saturation.Value, color.Color1Lightness.Value, painted_pixels, paint_indices[0]);
		}
		if (!color.Color2Default.Value)
		{
			_ApplyPaint(0, -100, 0, painted_pixels, paint_indices[1]);
			_ApplyPaint(color.Color2Hue.Value, color.Color2Saturation.Value, color.Color2Lightness.Value, painted_pixels, paint_indices[1]);
		}
		if (!color.Color3Default.Value)
		{
			_ApplyPaint(0, -100, 0, painted_pixels, paint_indices[2]);
			_ApplyPaint(color.Color3Hue.Value, color.Color3Saturation.Value, color.Color3Lightness.Value, painted_pixels, paint_indices[2]);
		}
		texture2D.SetData(painted_pixels);
		return texture2D;
	}

	protected static void _ApplyPaint(int h_shift, int s_shift, int l_shift, Color[] pixels, List<int> indices)
	{
		foreach (int index in indices)
		{
			if (index < pixels.Length)
			{
				Color color = pixels[index];
				Utility.RGBtoHSL(color.R, color.G, color.B, out var h, out var s, out var l);
				h += (double)h_shift;
				s += (double)s_shift / 100.0;
				l += (double)l_shift / 100.0;
				while (h > 360.0)
				{
					h -= 360.0;
				}
				for (; h < 0.0; h += 360.0)
				{
				}
				if (s < 0.0)
				{
					s = 0.0;
				}
				if (s > 1.0)
				{
					s = 1.0;
				}
				if (l < 0.0)
				{
					l = 0.0;
				}
				if (l > 1.0)
				{
					l = 1.0;
				}
				Utility.HSLtoRGB(h, s, l, out var r, out var g, out var b);
				color.R = (byte)r;
				color.G = (byte)g;
				color.B = (byte)b;
				pixels[index] = color;
			}
		}
	}
}
