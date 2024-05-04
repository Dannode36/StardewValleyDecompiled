using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Netcode;

public abstract class NetHashSet<TValue> : AbstractNetSerializable, IEquatable<NetHashSet<TValue>>, ISet<TValue>, ICollection<TValue>, IEnumerable<TValue>, IEnumerable
{
	public class IncomingChange
	{
		public uint Tick;

		public bool Removal;

		public TValue Value;

		public IncomingChange(uint tick, bool removal, TValue value)
		{
			Tick = tick;
			Removal = removal;
			Value = value;
		}
	}

	public class OutgoingChange
	{
		public bool Removal;

		public TValue Value;

		public OutgoingChange(bool removal, TValue value)
		{
			Removal = removal;
			Value = value;
		}
	}

	public delegate void ContentsChangeEvent(TValue value);

	public bool InterpolationWait = true;

	private readonly HashSet<TValue> Set = new HashSet<TValue>();

	private readonly List<IncomingChange> IncomingChanges = new List<IncomingChange>();

	private readonly List<OutgoingChange> OutgoingChanges = new List<OutgoingChange>();

	/// <inheritdoc />
	public int Count => Set.Count;

	/// <inheritdoc />
	public bool IsReadOnly => false;

	public event ContentsChangeEvent OnValueAdded;

	public event ContentsChangeEvent OnValueRemoved;

	public NetHashSet()
	{
	}

	public NetHashSet(IEnumerable<TValue> values)
		: this()
	{
		foreach (TValue value in values)
		{
			Add(value);
		}
	}

	public bool Add(TValue item)
	{
		if (!Set.Add(item))
		{
			return false;
		}
		OutgoingChanges.Add(new OutgoingChange(removal: false, item));
		MarkDirty();
		addedEvent(item);
		return true;
	}

	/// <inheritdoc />
	public void Clear()
	{
		TValue[] array = Set.ToArray();
		foreach (TValue entry in array)
		{
			Remove(entry);
		}
		OutgoingChanges.RemoveAll((OutgoingChange ch) => !ch.Removal);
	}

	/// <inheritdoc />
	public bool Contains(TValue item)
	{
		return Set.Contains(item);
	}

	/// <inheritdoc />
	public void CopyTo(TValue[] array, int arrayIndex)
	{
		Set.CopyTo(array, arrayIndex);
	}

	/// <inheritdoc />
	public bool Equals(NetHashSet<TValue> other)
	{
		return Set.Equals(other?.Set);
	}

	/// <inheritdoc />
	public void ExceptWith(IEnumerable<TValue> other)
	{
		Set.ExceptWith(other);
	}

	/// <inheritdoc />
	public IEnumerator<TValue> GetEnumerator()
	{
		return Set.GetEnumerator();
	}

	/// <inheritdoc />
	public void IntersectWith(IEnumerable<TValue> other)
	{
		Set.IntersectWith(other);
	}

	/// <inheritdoc />
	public bool IsProperSubsetOf(IEnumerable<TValue> other)
	{
		return Set.IsProperSubsetOf(other);
	}

	/// <inheritdoc />
	public bool IsProperSupersetOf(IEnumerable<TValue> other)
	{
		return Set.IsProperSupersetOf(other);
	}

	/// <inheritdoc />
	public bool IsSubsetOf(IEnumerable<TValue> other)
	{
		return Set.IsSubsetOf(other);
	}

	/// <inheritdoc />
	public bool IsSupersetOf(IEnumerable<TValue> other)
	{
		return Set.IsSupersetOf(other);
	}

	/// <inheritdoc />
	public bool Overlaps(IEnumerable<TValue> other)
	{
		return Set.Overlaps(other);
	}

	/// <inheritdoc />
	public bool Remove(TValue item)
	{
		if (!Set.Remove(item))
		{
			return false;
		}
		OutgoingChanges.Add(new OutgoingChange(removal: true, item));
		MarkDirty();
		removedEvent(item);
		return true;
	}

	/// <summary>Remove all elements that match a condition.</summary>
	/// <param name="match">The predicate matching values to remove.</param>
	/// <returns>Returns the number of values removed from the set.</returns>
	public int RemoveWhere(Predicate<TValue> match)
	{
		int num = Set.RemoveWhere(delegate(TValue value)
		{
			if (match(value))
			{
				OutgoingChanges.Add(new OutgoingChange(removal: true, value));
				removedEvent(value);
				return true;
			}
			return false;
		});
		if (num > 0)
		{
			MarkDirty();
		}
		return num;
	}

	/// <inheritdoc />
	public bool SetEquals(IEnumerable<TValue> other)
	{
		return Set.SetEquals(other);
	}

	/// <inheritdoc />
	public void SymmetricExceptWith(IEnumerable<TValue> other)
	{
		Set.SymmetricExceptWith(other);
	}

	/// <inheritdoc />
	public void UnionWith(IEnumerable<TValue> other)
	{
		Set.UnionWith(other);
	}

	/// <inheritdoc />
	void ICollection<TValue>.Add(TValue item)
	{
		Add(item);
	}

	/// <inheritdoc />
	IEnumerator IEnumerable.GetEnumerator()
	{
		return Set.GetEnumerator();
	}

	protected override bool tickImpl()
	{
		List<IncomingChange> triggeredChanges = null;
		foreach (IncomingChange ch in IncomingChanges)
		{
			if (base.Root == null || GetLocalTick() >= ch.Tick)
			{
				if (triggeredChanges == null)
				{
					triggeredChanges = new List<IncomingChange>();
				}
				triggeredChanges.Add(ch);
				continue;
			}
			break;
		}
		if (triggeredChanges != null)
		{
			foreach (IncomingChange ch in triggeredChanges)
			{
				IncomingChanges.Remove(ch);
			}
			foreach (IncomingChange ch in triggeredChanges)
			{
				if (ch.Removal)
				{
					if (Set.Remove(ch.Value))
					{
						removedEvent(ch.Value);
					}
				}
				else if (Set.Add(ch.Value))
				{
					addedEvent(ch.Value);
				}
			}
		}
		return IncomingChanges.Count > 0;
	}

	private void removedEvent(TValue value)
	{
		this.OnValueRemoved?.Invoke(value);
	}

	private void addedEvent(TValue value)
	{
		this.OnValueAdded?.Invoke(value);
	}

	/// <inheritdoc />
	public override bool Equals(object obj)
	{
		if (obj is NetHashSet<TValue> other)
		{
			return Equals(other);
		}
		return false;
	}

	/// <inheritdoc />
	public override void Read(BinaryReader reader, NetVersion version)
	{
		uint tick = GetLocalTick() + (uint)((InterpolationWait && base.Root != null) ? base.Root.Clock.InterpolationTicks : 0);
		uint count = reader.Read7BitEncoded();
		for (uint i = 0u; i < count; i++)
		{
			bool removal = reader.ReadBoolean();
			TValue value = ReadValue(reader);
			IncomingChanges.Add(new IncomingChange(tick, removal, value));
			base.NeedsTick = true;
		}
	}

	/// <inheritdoc />
	public override void Write(BinaryWriter writer)
	{
		writer.Write7BitEncoded((uint)OutgoingChanges.Count);
		foreach (OutgoingChange ch in OutgoingChanges)
		{
			writer.Write(ch.Removal);
			WriteValue(writer, ch.Value);
		}
	}

	/// <inheritdoc />
	public override void ReadFull(BinaryReader reader, NetVersion version)
	{
		Set.Clear();
		int count = reader.ReadInt32();
		Set.EnsureCapacity(count);
		for (int i = 0; i < count; i++)
		{
			TValue value = ReadValue(reader);
			Set.Add(value);
			addedEvent(value);
		}
	}

	/// <inheritdoc />
	public override void WriteFull(BinaryWriter writer)
	{
		writer.Write(Set.Count);
		foreach (TValue value in Set)
		{
			WriteValue(writer, value);
		}
	}

	public override int GetHashCode()
	{
		return Set.GetHashCode();
	}

	public abstract TValue ReadValue(BinaryReader reader);

	public abstract void WriteValue(BinaryWriter writer, TValue value);

	protected override void CleanImpl()
	{
		base.CleanImpl();
		OutgoingChanges.Clear();
	}
}
