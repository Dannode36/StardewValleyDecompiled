using Netcode;

namespace StardewValley.Network;

public abstract class NetPausableField<T, TField, TBaseField> : INetObject<NetFields> where TField : TBaseField, new() where TBaseField : NetFieldBase<T, TBaseField>, new()
{
	private bool paused;

	public readonly TField Field;

	private readonly NetEvent1Field<bool, NetBool> pauseEvent = new NetEvent1Field<bool, NetBool>();

	public T Value
	{
		get
		{
			return Get();
		}
		set
		{
			Set(value);
		}
	}

	public bool Paused
	{
		get
		{
			pauseEvent.Poll();
			return paused;
		}
		set
		{
			if (value != paused)
			{
				pauseEvent.Fire(value);
				pauseEvent.Poll();
			}
		}
	}

	public abstract NetFields NetFields { get; }

	public NetPausableField(TField field)
	{
		Field = field;
		initNetFields();
	}

	protected virtual void initNetFields()
	{
		NetFields.SetOwner(this).AddField(Field, "Field").AddField(pauseEvent, "pauseEvent");
		pauseEvent.onEvent += delegate(bool newPauseValue)
		{
			paused = newPauseValue;
		};
	}

	public NetPausableField()
		: this(new TField())
	{
	}

	public virtual T Get()
	{
		if (Paused)
		{
			Field.CancelInterpolation();
		}
		return Field.Get();
	}

	public void Set(T value)
	{
		Field.Set(value);
	}

	public bool IsPausePending()
	{
		return pauseEvent.HasPendingEvent((bool p) => p);
	}

	public bool IsInterpolating()
	{
		if (Field.IsInterpolating())
		{
			return !Paused;
		}
		return false;
	}
}
