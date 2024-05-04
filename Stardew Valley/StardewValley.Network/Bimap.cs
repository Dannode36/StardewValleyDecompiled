using System;
using System.Collections;
using System.Collections.Generic;

namespace StardewValley.Network;

public class Bimap<L, R> : IEnumerable<KeyValuePair<L, R>>, IEnumerable
{
	private Dictionary<L, R> leftToRight = new Dictionary<L, R>();

	private Dictionary<R, L> rightToLeft = new Dictionary<R, L>();

	public R this[L l]
	{
		get
		{
			return leftToRight[l];
		}
		set
		{
			if (leftToRight.TryGetValue(l, out var rightKey))
			{
				rightToLeft.Remove(rightKey);
			}
			if (rightToLeft.TryGetValue(value, out var leftKey))
			{
				leftToRight.Remove(leftKey);
			}
			leftToRight[l] = value;
			rightToLeft[value] = l;
		}
	}

	public L this[R r]
	{
		get
		{
			return rightToLeft[r];
		}
		set
		{
			if (rightToLeft.TryGetValue(r, out var leftKey))
			{
				leftToRight.Remove(leftKey);
			}
			if (leftToRight.TryGetValue(value, out var rightKey))
			{
				rightToLeft.Remove(rightKey);
			}
			rightToLeft[r] = value;
			leftToRight[value] = r;
		}
	}

	public ICollection<L> LeftValues => leftToRight.Keys;

	public ICollection<R> RightValues => rightToLeft.Keys;

	public int Count => rightToLeft.Count;

	public void Clear()
	{
		leftToRight.Clear();
		rightToLeft.Clear();
	}

	public void Add(L l, R r)
	{
		if (leftToRight.ContainsKey(l) || rightToLeft.ContainsKey(r))
		{
			throw new ArgumentException();
		}
		leftToRight.Add(l, r);
		rightToLeft.Add(r, l);
	}

	public bool ContainsLeft(L l)
	{
		return leftToRight.ContainsKey(l);
	}

	public bool ContainsRight(R r)
	{
		return rightToLeft.ContainsKey(r);
	}

	public void RemoveLeft(L l)
	{
		if (leftToRight.TryGetValue(l, out var rightKey))
		{
			rightToLeft.Remove(rightKey);
		}
		leftToRight.Remove(l);
	}

	public void RemoveRight(R r)
	{
		if (rightToLeft.TryGetValue(r, out var leftKey))
		{
			leftToRight.Remove(leftKey);
		}
		rightToLeft.Remove(r);
	}

	public L GetLeft(R r)
	{
		return rightToLeft[r];
	}

	public R GetRight(L l)
	{
		return leftToRight[l];
	}

	public IEnumerator<KeyValuePair<L, R>> GetEnumerator()
	{
		return leftToRight.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
