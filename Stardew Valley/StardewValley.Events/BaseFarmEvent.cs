using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;

namespace StardewValley.Events;

/// <inheritdoc />
public abstract class BaseFarmEvent : FarmEvent, INetObject<NetFields>
{
	/// <summary>The multiplayer-synchronized fields for this event.</summary>
	public NetFields NetFields { get; private set; }

	/// <summary>Construct an instance.</summary>
	protected BaseFarmEvent()
	{
		initNetFields();
	}

	/// <summary>Initialize the multiplayer-synchronized fields for this instance.</summary>
	public virtual void initNetFields()
	{
		NetFields = new NetFields(GetType().Name).SetOwner(this);
	}

	/// <inheritdoc />
	public virtual bool setUp()
	{
		return false;
	}

	/// <inheritdoc />
	public virtual bool tickUpdate(GameTime time)
	{
		return true;
	}

	/// <inheritdoc />
	public virtual void draw(SpriteBatch b)
	{
	}

	/// <inheritdoc />
	public virtual void drawAboveEverything(SpriteBatch b)
	{
	}

	/// <inheritdoc />
	public virtual void makeChangesToLocation()
	{
	}
}
