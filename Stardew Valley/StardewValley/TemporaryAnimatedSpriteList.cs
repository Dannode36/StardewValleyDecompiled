using System.Collections;
using System.Collections.Generic;

namespace StardewValley;

public class TemporaryAnimatedSpriteList : IList<TemporaryAnimatedSprite>, ICollection<TemporaryAnimatedSprite>, IEnumerable<TemporaryAnimatedSprite>, IEnumerable
{
	public List<TemporaryAnimatedSprite> AnimatedSprites = new List<TemporaryAnimatedSprite>();

	public TemporaryAnimatedSprite this[int index]
	{
		get
		{
			return AnimatedSprites[index];
		}
		set
		{
			AnimatedSprites[index] = value;
		}
	}

	public int Count => AnimatedSprites.Count;

	public bool IsReadOnly => false;

	public void AddRange(IEnumerable<TemporaryAnimatedSprite> values)
	{
		AnimatedSprites.AddRange(values);
	}

	public void Add(TemporaryAnimatedSprite item)
	{
		AnimatedSprites.Add(item);
	}

	public void Clear()
	{
		foreach (TemporaryAnimatedSprite sprite in AnimatedSprites)
		{
			if (sprite.Pooled)
			{
				sprite.Pool();
			}
		}
		AnimatedSprites.Clear();
	}

	public bool Contains(TemporaryAnimatedSprite item)
	{
		return AnimatedSprites.Contains(item);
	}

	public void CopyTo(TemporaryAnimatedSprite[] array, int index)
	{
		AnimatedSprites.CopyTo(array, index);
	}

	public IEnumerator<TemporaryAnimatedSprite> GetEnumerator()
	{
		return AnimatedSprites.GetEnumerator();
	}

	public int IndexOf(TemporaryAnimatedSprite item)
	{
		return AnimatedSprites.IndexOf(item);
	}

	public void Insert(int index, TemporaryAnimatedSprite item)
	{
		AnimatedSprites.Insert(index, item);
	}

	public bool Remove(TemporaryAnimatedSprite item)
	{
		if (AnimatedSprites.Remove(item))
		{
			if (item.Pooled)
			{
				item.Pool();
			}
			return true;
		}
		return false;
	}

	public void RemoveAt(int index)
	{
		TemporaryAnimatedSprite item = AnimatedSprites[index];
		AnimatedSprites.RemoveAt(index);
		if (item.Pooled)
		{
			item.Pool();
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
