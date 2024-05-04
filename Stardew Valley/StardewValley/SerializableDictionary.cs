using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace StardewValley;

/// <summary>A dictionary that can be read and written in the save XML.</summary>
/// <typeparam name="TKey">The key type.</typeparam>
/// <typeparam name="TValue">The value type.</typeparam>
[XmlRoot("dictionary")]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
{
	public struct ChangeArgs
	{
		public readonly ChangeType Type;

		public readonly TKey Key;

		public readonly TValue Value;

		public ChangeArgs(ChangeType type, TKey k, TValue v)
		{
			Type = type;
			Key = k;
			Value = v;
		}
	}

	public delegate void ChangeCallback(object sender, ChangeArgs args);

	private static XmlSerializer _keySerializer;

	private static XmlSerializer _valueSerializer;

	public event ChangeCallback CollectionChanged;

	static SerializableDictionary()
	{
		_keySerializer = SaveGame.GetSerializer(typeof(TKey));
		_valueSerializer = SaveGame.GetSerializer(typeof(TValue));
	}

	/// <summary>Construct an empty instance.</summary>
	public SerializableDictionary()
	{
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="data">The data to copy.</param>
	public SerializableDictionary(IDictionary<TKey, TValue> data)
		: base(data)
	{
	}

	/// <summary>Create an instance from a dictionary with a different value type.</summary>
	/// <typeparam name="TSourceValue">The value type in the source data to copy.</typeparam>
	/// <param name="data">The data to copy.</param>
	/// <param name="getValue">Get the value to use for an entry in the original data.</param>
	public static SerializableDictionary<TKey, TValue> BuildFrom<TSourceValue>(IDictionary<TKey, TSourceValue> data, Func<TSourceValue, TValue> getValue)
	{
		SerializableDictionary<TKey, TValue> result = new SerializableDictionary<TKey, TValue>();
		foreach (KeyValuePair<TKey, TSourceValue> entry in data)
		{
			result[entry.Key] = getValue(entry.Value);
		}
		return result;
	}

	/// <summary>Create an instance from a dictionary with different key and value types.</summary>
	/// <typeparam name="TSourceKey">The key type in the source data to copy.</typeparam>
	/// <typeparam name="TSourceValue">The value type in the source data to copy.</typeparam>
	/// <param name="data">The data to copy.</param>
	/// <param name="getKey">Get the key to use for an entry in the original data.</param>
	/// <param name="getValue">Get the value to use for an entry in the original data.</param>
	public static SerializableDictionary<TKey, TValue> BuildFrom<TSourceKey, TSourceValue>(IDictionary<TSourceKey, TSourceValue> data, Func<TSourceKey, TKey> getKey, Func<TSourceValue, TValue> getValue)
	{
		SerializableDictionary<TKey, TValue> result = new SerializableDictionary<TKey, TValue>();
		foreach (KeyValuePair<TSourceKey, TSourceValue> entry in data)
		{
			result[getKey(entry.Key)] = getValue(entry.Value);
		}
		return result;
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="comparer">The equality comparer to use when comparing keys, or null to use the default comparer for the key type.</param>
	protected SerializableDictionary(IEqualityComparer<TKey> comparer = null)
		: base(comparer)
	{
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="data">The data to copy.</param>
	/// <param name="comparer">The equality comparer to use when comparing keys, or null to use the default comparer for the key type.</param>
	protected SerializableDictionary(IDictionary<TKey, TValue> data, IEqualityComparer<TKey> comparer = null)
		: base(data, comparer)
	{
	}

	public new void Add(TKey key, TValue value)
	{
		base.Add(key, value);
		OnCollectionChanged(this, new ChangeArgs(ChangeType.Add, key, value));
	}

	public new bool Remove(TKey key)
	{
		if (TryGetValue(key, out var val))
		{
			base.Remove(key);
			OnCollectionChanged(this, new ChangeArgs(ChangeType.Remove, key, val));
			return true;
		}
		return false;
	}

	public new void Clear()
	{
		base.Clear();
		OnCollectionChanged(this, new ChangeArgs(ChangeType.Clear, default(TKey), default(TValue)));
	}

	private void OnCollectionChanged(object sender, ChangeArgs args)
	{
		this.CollectionChanged?.Invoke(sender ?? this, args);
	}

	public XmlSchema GetSchema()
	{
		return null;
	}

	public void ReadXml(XmlReader reader)
	{
		bool isEmptyElement = reader.IsEmptyElement;
		reader.Read();
		if (isEmptyElement)
		{
			return;
		}
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			reader.ReadStartElement("item");
			reader.ReadStartElement("key");
			bool read = false;
			TKey key = default(TKey);
			if (typeof(TKey) == typeof(string))
			{
				string name = reader.Name;
				if (!(name == "int"))
				{
					if (name == "LocationContext")
					{
						reader.ReadStartElement();
						key = (TKey)Convert.ChangeType(reader.ReadContentAsString(), typeof(TKey));
						reader.ReadEndElement();
						read = true;
					}
				}
				else
				{
					key = (TKey)Convert.ChangeType((int)SaveGame.GetSerializer(typeof(int)).Deserialize(reader), typeof(TKey));
					read = true;
				}
			}
			if (!read)
			{
				key = (TKey)_keySerializer.Deserialize(reader);
			}
			reader.ReadEndElement();
			reader.ReadStartElement("value");
			TValue value = default(TValue);
			read = false;
			if (typeof(TValue) == typeof(string) && reader.Name == "int")
			{
				value = (TValue)Convert.ChangeType((int)SaveGame.GetSerializer(typeof(int)).Deserialize(reader), typeof(TValue));
				read = true;
			}
			if (!read)
			{
				value = (TValue)_valueSerializer.Deserialize(reader);
			}
			reader.ReadEndElement();
			base.Add(key, value);
			reader.ReadEndElement();
			reader.MoveToContent();
		}
		reader.ReadEndElement();
	}

	public void WriteXml(XmlWriter writer)
	{
		foreach (TKey key in base.Keys)
		{
			writer.WriteStartElement("item");
			writer.WriteStartElement("key");
			_keySerializer.Serialize(writer, key);
			writer.WriteEndElement();
			writer.WriteStartElement("value");
			TValue value = base[key];
			_valueSerializer.Serialize(writer, value);
			writer.WriteEndElement();
			writer.WriteEndElement();
		}
	}
}
