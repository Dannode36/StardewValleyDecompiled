using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Network;

namespace StardewValley;

public class Chunk : INetObject<NetFields>
{
	[XmlElement("position")]
	public NetPosition position = new NetPosition();

	[XmlIgnore]
	public readonly NetFloat xVelocity = new NetFloat().Interpolated(interpolate: true, wait: true);

	[XmlIgnore]
	public readonly NetFloat yVelocity = new NetFloat().Interpolated(interpolate: true, wait: true);

	[XmlIgnore]
	public readonly NetBool hasPassedRestingLineOnce = new NetBool(value: false);

	[XmlIgnore]
	public int bounces;

	private readonly NetInt netDebrisType = new NetInt();

	[XmlIgnore]
	public bool hitWall;

	[XmlElement("xSpriteSheet")]
	public readonly NetInt xSpriteSheet = new NetInt();

	[XmlElement("ySpriteSheet")]
	public readonly NetInt ySpriteSheet = new NetInt();

	[XmlIgnore]
	public float rotation;

	[XmlIgnore]
	public float rotationVelocity;

	private readonly NetFloat netScale = new NetFloat().Interpolated(interpolate: true, wait: true);

	private readonly NetFloat netAlpha = new NetFloat();

	public int randomOffset
	{
		get
		{
			return netDebrisType;
		}
		set
		{
			netDebrisType.Value = value;
		}
	}

	public float scale
	{
		get
		{
			return netScale.Value;
		}
		set
		{
			netScale.Value = value;
		}
	}

	public float alpha
	{
		get
		{
			return netAlpha.Value;
		}
		set
		{
			netAlpha.Value = value;
		}
	}

	[XmlIgnore]
	public NetFields NetFields { get; } = new NetFields("Chunk");


	public Chunk()
	{
		NetFields.SetOwner(this).AddField(position.NetFields, "position.NetFields").AddField(xVelocity, "xVelocity")
			.AddField(yVelocity, "yVelocity")
			.AddField(netDebrisType, "netDebrisType")
			.AddField(xSpriteSheet, "xSpriteSheet")
			.AddField(ySpriteSheet, "ySpriteSheet")
			.AddField(netScale, "netScale")
			.AddField(netAlpha, "netAlpha")
			.AddField(hasPassedRestingLineOnce, "hasPassedRestingLineOnce");
		if (LocalMultiplayer.IsLocalMultiplayer(is_local_only: true))
		{
			NetFields.DeltaAggregateTicks = 10;
		}
		else
		{
			NetFields.DeltaAggregateTicks = 30;
		}
	}

	public Chunk(Vector2 position, float xVelocity, float yVelocity, int random_offset)
		: this()
	{
		this.position.Value = position;
		this.xVelocity.Value = xVelocity;
		this.yVelocity.Value = yVelocity;
		randomOffset = random_offset;
		alpha = 1f;
	}

	public float getSpeed()
	{
		return (float)Math.Sqrt(xVelocity.Value * xVelocity.Value + yVelocity.Value * yVelocity.Value);
	}
}
