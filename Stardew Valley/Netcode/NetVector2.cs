using System;
using System.IO;
using Microsoft.Xna.Framework;

namespace Netcode;

public sealed class NetVector2 : NetField<Vector2, NetVector2>
{
	public bool AxisAlignedMovement;

	public float ExtrapolationSpeed;

	public float MinDeltaForDirectionChange = 8f;

	public float MaxInterpolationDistance = 320f;

	private bool interpolateXFirst;

	private bool isExtrapolating;

	private bool isFixingExtrapolation;

	public float X
	{
		get
		{
			return base.Value.X;
		}
		set
		{
			Vector2 vector = base.value;
			if (vector.X != value)
			{
				Vector2 newValue = new Vector2(value, vector.Y);
				if (canShortcutSet())
				{
					base.value = newValue;
					return;
				}
				cleanSet(newValue);
				MarkDirty();
			}
		}
	}

	public float Y
	{
		get
		{
			return base.Value.Y;
		}
		set
		{
			Vector2 vector = base.value;
			if (vector.Y != value)
			{
				Vector2 newValue = new Vector2(vector.X, value);
				if (canShortcutSet())
				{
					base.value = newValue;
					return;
				}
				cleanSet(newValue);
				MarkDirty();
			}
		}
	}

	public NetVector2()
	{
	}

	public NetVector2(Vector2 value)
		: base(value)
	{
	}

	public void Set(float x, float y)
	{
		Set(new Vector2(x, y));
	}

	public override void Set(Vector2 newValue)
	{
		if (canShortcutSet())
		{
			value = newValue;
		}
		else if (newValue != value)
		{
			cleanSet(newValue);
			MarkDirty();
		}
	}

	public Vector2 InterpolationDelta()
	{
		if (base.NeedsTick)
		{
			return targetValue - previousValue;
		}
		return Vector2.Zero;
	}

	protected override bool setUpInterpolation(Vector2 oldValue, Vector2 newValue)
	{
		if ((newValue - oldValue).LengthSquared() >= MaxInterpolationDistance * MaxInterpolationDistance)
		{
			return false;
		}
		if (AxisAlignedMovement)
		{
			if (base.NeedsTick)
			{
				Vector2 delta = targetValue - previousValue;
				Vector2 absDelta = new Vector2(Math.Abs(delta.X), Math.Abs(delta.Y));
				if (interpolateXFirst)
				{
					interpolateXFirst = InterpolationFactor() * (absDelta.X + absDelta.Y) < absDelta.X;
				}
				else
				{
					interpolateXFirst = InterpolationFactor() * (absDelta.X + absDelta.Y) > absDelta.Y;
				}
			}
			else
			{
				Vector2 delta = newValue - oldValue;
				Vector2 absDelta = new Vector2(Math.Abs(delta.X), Math.Abs(delta.Y));
				interpolateXFirst = absDelta.X < absDelta.Y;
			}
		}
		return true;
	}

	public Vector2 CurrentInterpolationDirection()
	{
		Vector2 delta;
		if (AxisAlignedMovement)
		{
			float factor = InterpolationFactor();
			delta = InterpolationDelta();
			float traveledLength = (Math.Abs(delta.X) + Math.Abs(delta.Y)) * factor;
			if (Math.Abs(delta.X) < MinDeltaForDirectionChange && Math.Abs(delta.Y) < MinDeltaForDirectionChange)
			{
				return Vector2.Zero;
			}
			if (Math.Abs(delta.X) < MinDeltaForDirectionChange)
			{
				return new Vector2(0f, Math.Sign(delta.Y));
			}
			if (Math.Abs(delta.Y) < MinDeltaForDirectionChange)
			{
				return new Vector2(Math.Sign(delta.X), 0f);
			}
			if (interpolateXFirst)
			{
				if (traveledLength > Math.Abs(delta.X))
				{
					return new Vector2(0f, Math.Sign(delta.Y));
				}
				return new Vector2(Math.Sign(delta.X), 0f);
			}
			if (traveledLength > Math.Abs(delta.Y))
			{
				return new Vector2(Math.Sign(delta.X), 0f);
			}
			return new Vector2(0f, Math.Sign(delta.Y));
		}
		delta = InterpolationDelta();
		delta.Normalize();
		return delta;
	}

	public float CurrentInterpolationSpeed()
	{
		float distance = InterpolationDelta().Length();
		if (InterpolationTicks() == 0)
		{
			return distance;
		}
		if (InterpolationFactor() > 1f)
		{
			return ExtrapolationSpeed;
		}
		return distance / (float)InterpolationTicks();
	}

	protected override Vector2 interpolate(Vector2 startValue, Vector2 endValue, float factor)
	{
		if (AxisAlignedMovement && factor <= 1f && !isFixingExtrapolation)
		{
			isExtrapolating = false;
			Vector2 delta = InterpolationDelta();
			Vector2 absDelta = new Vector2(Math.Abs(delta.X), Math.Abs(delta.Y));
			float traveledLength = (absDelta.X + absDelta.Y) * factor;
			float x;
			float y;
			if (interpolateXFirst)
			{
				if (traveledLength > absDelta.X)
				{
					x = endValue.X;
					y = startValue.Y + (traveledLength - absDelta.X) * (float)Math.Sign(delta.Y);
				}
				else
				{
					x = startValue.X + traveledLength * (float)Math.Sign(delta.X);
					y = startValue.Y;
				}
			}
			else if (traveledLength > absDelta.Y)
			{
				y = endValue.Y;
				x = startValue.X + (traveledLength - absDelta.Y) * (float)Math.Sign(delta.X);
			}
			else
			{
				y = startValue.Y + traveledLength * (float)Math.Sign(delta.Y);
				x = startValue.X;
			}
			return new Vector2(x, y);
		}
		if (factor > 1f)
		{
			isExtrapolating = true;
			uint extrapolationTicks = base.Root.Clock.GetLocalTick() - interpolationStartTick - (uint)InterpolationTicks();
			Vector2 direction = endValue - startValue;
			if (direction.LengthSquared() > ExtrapolationSpeed * ExtrapolationSpeed)
			{
				direction.Normalize();
				return endValue + direction * extrapolationTicks * ExtrapolationSpeed;
			}
		}
		isExtrapolating = false;
		return startValue + (endValue - startValue) * factor;
	}

	protected override void ReadDelta(BinaryReader reader, NetVersion version)
	{
		float newX = reader.ReadSingle();
		float newY = reader.ReadSingle();
		if (version.IsPriorityOver(ChangeVersion))
		{
			isFixingExtrapolation = isExtrapolating;
			setInterpolationTarget(new Vector2(newX, newY));
			isExtrapolating = false;
		}
	}

	protected override void WriteDelta(BinaryWriter writer)
	{
		writer.Write(base.Value.X);
		writer.Write(base.Value.Y);
	}
}
