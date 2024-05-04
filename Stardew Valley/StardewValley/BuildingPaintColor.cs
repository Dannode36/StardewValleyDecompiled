using System;
using System.Xml.Serialization;
using Netcode;

namespace StardewValley;

public class BuildingPaintColor : INetObject<NetFields>
{
	public NetString ColorName = new NetString();

	public NetBool Color1Default = new NetBool(value: true);

	public NetInt Color1Hue = new NetInt();

	public NetInt Color1Saturation = new NetInt();

	public NetInt Color1Lightness = new NetInt();

	public NetBool Color2Default = new NetBool(value: true);

	public NetInt Color2Hue = new NetInt();

	public NetInt Color2Saturation = new NetInt();

	public NetInt Color2Lightness = new NetInt();

	public NetBool Color3Default = new NetBool(value: true);

	public NetInt Color3Hue = new NetInt();

	public NetInt Color3Saturation = new NetInt();

	public NetInt Color3Lightness = new NetInt();

	protected bool _dirty;

	[XmlIgnore]
	public NetFields NetFields { get; } = new NetFields("BuildingPaintColor");


	public BuildingPaintColor()
	{
		NetFields.SetOwner(this).AddField(ColorName, "ColorName").AddField(Color1Default, "Color1Default")
			.AddField(Color2Default, "Color2Default")
			.AddField(Color3Default, "Color3Default")
			.AddField(Color1Hue, "Color1Hue")
			.AddField(Color1Saturation, "Color1Saturation")
			.AddField(Color1Lightness, "Color1Lightness")
			.AddField(Color2Hue, "Color2Hue")
			.AddField(Color2Saturation, "Color2Saturation")
			.AddField(Color2Lightness, "Color2Lightness")
			.AddField(Color3Hue, "Color3Hue")
			.AddField(Color3Saturation, "Color3Saturation")
			.AddField(Color3Lightness, "Color3Lightness");
		Color1Default.fieldChangeVisibleEvent += OnDefaultFlagChanged;
		Color2Default.fieldChangeVisibleEvent += OnDefaultFlagChanged;
		Color3Default.fieldChangeVisibleEvent += OnDefaultFlagChanged;
		Color1Hue.fieldChangeVisibleEvent += OnColorChanged;
		Color1Saturation.fieldChangeVisibleEvent += OnColorChanged;
		Color1Lightness.fieldChangeVisibleEvent += OnColorChanged;
		Color2Hue.fieldChangeVisibleEvent += OnColorChanged;
		Color2Saturation.fieldChangeVisibleEvent += OnColorChanged;
		Color2Lightness.fieldChangeVisibleEvent += OnColorChanged;
		Color3Hue.fieldChangeVisibleEvent += OnColorChanged;
		Color3Saturation.fieldChangeVisibleEvent += OnColorChanged;
		Color3Lightness.fieldChangeVisibleEvent += OnColorChanged;
	}

	public virtual void CopyFrom(BuildingPaintColor other)
	{
		ColorName.Value = other.ColorName.Value;
		Color1Default.Value = other.Color1Default.Value;
		Color1Hue.Value = other.Color1Hue.Value;
		Color1Saturation.Value = other.Color1Saturation.Value;
		Color1Lightness.Value = other.Color1Lightness.Value;
		Color2Default.Value = other.Color2Default.Value;
		Color2Hue.Value = other.Color2Hue.Value;
		Color2Saturation.Value = other.Color2Saturation.Value;
		Color2Lightness.Value = other.Color2Lightness.Value;
		Color3Default.Value = other.Color3Default.Value;
		Color3Hue.Value = other.Color3Hue.Value;
		Color3Saturation.Value = other.Color3Saturation.Value;
		Color3Lightness.Value = other.Color3Lightness.Value;
	}

	public virtual void OnDefaultFlagChanged(NetBool field, bool old_value, bool new_value)
	{
		_dirty = true;
	}

	public virtual void OnColorChanged(NetInt field, int old_value, int new_value)
	{
		_dirty = true;
	}

	public virtual void Poll(Action apply)
	{
		if (_dirty)
		{
			apply?.Invoke();
			_dirty = false;
		}
	}

	public bool IsDirty()
	{
		return _dirty;
	}

	public bool RequiresRecolor()
	{
		if (Color1Default.Value && Color2Default.Value)
		{
			return !Color3Default.Value;
		}
		return true;
	}
}
