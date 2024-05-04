using Microsoft.Xna.Framework;
using Netcode;

namespace StardewValley.Network;

public sealed class NetPosition : NetPausableField<Vector2, NetVector2, NetVector2>
{
	private const float SmoothingFudge = 0.8f;

	private const ushort DefaultDeltaAggregateTicks = 0;

	public bool ExtrapolationEnabled;

	public readonly NetBool moving = new NetBool().Interpolated(interpolate: false, wait: false);

	public override NetFields NetFields { get; } = new NetFields("NetPosition");


	public float X
	{
		get
		{
			return Get().X;
		}
		set
		{
			Set(new Vector2(value, Y));
		}
	}

	public float Y
	{
		get
		{
			return Get().Y;
		}
		set
		{
			Set(new Vector2(X, value));
		}
	}

	/// <summary>An event raised when this field's value is set (either locally or remotely). Not triggered by changes due to interpolation. May be triggered before the change is visible on the field, if InterpolationTicks &gt; 0.</summary>
	public event FieldChange<NetPosition, Vector2> fieldChangeEvent;

	/// <summary>An event raised after this field's value is set and interpolated.</summary>
	public event FieldChange<NetPosition, Vector2> fieldChangeVisibleEvent;

	public NetPosition()
		: base(new NetVector2().Interpolated(interpolate: true, wait: true))
	{
	}

	public NetPosition(NetVector2 field)
		: base(field)
	{
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		NetFields.AddField(moving, "moving");
		NetFields.DeltaAggregateTicks = 0;
		Field.fieldChangeEvent += delegate(NetVector2 f, Vector2 oldValue, Vector2 newValue)
		{
			if (IsMaster())
			{
				moving.Value = true;
			}
			this.fieldChangeEvent?.Invoke(this, oldValue, newValue);
		};
		Field.fieldChangeVisibleEvent += delegate(NetVector2 field, Vector2 oldValue, Vector2 newValue)
		{
			this.fieldChangeVisibleEvent?.Invoke(this, oldValue, newValue);
		};
		moving.fieldChangeEvent += delegate(NetBool f, bool oldValue, bool newValue)
		{
			if (!IsMaster())
			{
				Field.ExtrapolationEnabled = newValue && ExtrapolationEnabled;
			}
		};
	}

	protected bool IsMaster()
	{
		INetRoot root = NetFields.Root;
		if (root == null)
		{
			return false;
		}
		return root.Clock.LocalId == 0;
	}

	public override Vector2 Get()
	{
		if (Game1.HostPaused)
		{
			Field.CancelInterpolation();
		}
		return base.Get();
	}

	public Vector2 CurrentInterpolationDirection()
	{
		if (base.Paused)
		{
			return Vector2.Zero;
		}
		return Field.CurrentInterpolationDirection();
	}

	public float CurrentInterpolationSpeed()
	{
		if (base.Paused)
		{
			return 0f;
		}
		return Field.CurrentInterpolationSpeed();
	}

	public void UpdateExtrapolation(float extrapolationSpeed)
	{
		NetFields.DeltaAggregateTicks = (ushort)((NetFields.Root != null) ? ((ushort)((float)NetFields.Root.Clock.InterpolationTicks * 0.8f)) : 0);
		ExtrapolationEnabled = true;
		Field.ExtrapolationSpeed = extrapolationSpeed;
		if (IsMaster())
		{
			moving.Value = false;
		}
	}
}
