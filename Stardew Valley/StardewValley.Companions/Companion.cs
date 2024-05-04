using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Network;

namespace StardewValley.Companions;

public class Companion : INetObject<NetFields>
{
	public readonly NetInt direction = new NetInt();

	protected readonly NetPosition _position = new NetPosition();

	protected readonly NetFarmerRef _owner = new NetFarmerRef();

	public readonly NetInt whichVariant = new NetInt();

	public float lerp = -1f;

	public Vector2 startPosition;

	public Vector2 endPosition;

	public float height;

	public float gravity;

	public NetEvent1Field<float, NetFloat> hopEvent = new NetEvent1Field<float, NetFloat>();

	public NetFields NetFields { get; } = new NetFields("Companion");


	public Farmer Owner
	{
		get
		{
			return _owner.Value;
		}
		set
		{
			_owner.Value = value;
		}
	}

	public Vector2 Position
	{
		get
		{
			return _position.Value;
		}
		set
		{
			_position.Value = value;
		}
	}

	public Vector2 OwnerPosition => Utility.PointToVector2(Owner.GetBoundingBox().Center);

	public bool IsLocal => Owner.IsLocalPlayer;

	public Companion()
	{
		InitNetFields();
		direction.Value = 1;
	}

	public virtual void InitializeCompanion(Farmer farmer)
	{
		_owner.Value = farmer;
		_position.Value = farmer.Position;
	}

	public virtual void CleanupCompanion()
	{
		_owner.Value = null;
	}

	public virtual void InitNetFields()
	{
		NetFields.SetOwner(this).AddField(_owner.NetFields, "_owner.NetFields").AddField(_position.NetFields, "_position.NetFields")
			.AddField(hopEvent, "hopEvent")
			.AddField(direction, "direction")
			.AddField(whichVariant, "whichVariant");
		hopEvent.onEvent += Hop;
	}

	public virtual void Hop(float amount)
	{
		height = 0f;
		gravity = amount;
	}

	public virtual void Update(GameTime time, GameLocation location)
	{
		if (IsLocal)
		{
			if (lerp < 0f)
			{
				if ((OwnerPosition - Position).Length() > 768f)
				{
					Utility.addRainbowStarExplosion(location, Position + new Vector2(0f, 0f - height), 1);
					Position = Owner.Position;
					lerp = -1f;
				}
				if ((OwnerPosition - Position).Length() > 80f)
				{
					startPosition = Position;
					float radius = 0.33f;
					endPosition = OwnerPosition + new Vector2(Utility.RandomFloat(-64f, 64f) * radius, Utility.RandomFloat(-64f, 64f) * radius);
					if (location.isCollidingPosition(new Rectangle((int)endPosition.X - 8, (int)endPosition.Y - 8, 16, 16), Game1.viewport, isFarmer: false, 0, glider: false, null, pathfinding: true, projectile: false, ignoreCharacterRequirement: true))
					{
						endPosition = OwnerPosition;
					}
					lerp = 0f;
					hopEvent.Fire(1f);
					if (Math.Abs(OwnerPosition.X - Position.X) > 8f)
					{
						if (OwnerPosition.X > Position.X)
						{
							direction.Value = 1;
						}
						else
						{
							direction.Value = 3;
						}
					}
				}
			}
			if (lerp >= 0f)
			{
				lerp += (float)time.ElapsedGameTime.TotalSeconds / 0.4f;
				if (lerp > 1f)
				{
					lerp = 1f;
				}
				float x = Utility.Lerp(startPosition.X, endPosition.X, lerp);
				float y = Utility.Lerp(startPosition.Y, endPosition.Y, lerp);
				Position = new Vector2(x, y);
				if (lerp == 1f)
				{
					lerp = -1f;
				}
			}
		}
		hopEvent.Poll();
		if (gravity != 0f || height != 0f)
		{
			height += gravity;
			gravity -= (float)time.ElapsedGameTime.TotalSeconds * 6f;
			if (height <= 0f)
			{
				height = 0f;
				gravity = 0f;
			}
		}
	}

	public virtual void Draw(SpriteBatch b)
	{
	}

	public virtual void OnOwnerWarp()
	{
		_position.Value = _owner.Value.Position;
	}
}
