using System.IO;
using System.Linq;
using Netcode;

namespace StardewValley;

public class NetStringIntArrayDictionary : NetDictionary<string, int[], NetArray<int, NetInt>, SerializableDictionary<string, int[]>, NetStringIntArrayDictionary>
{
	protected override string ReadKey(BinaryReader reader)
	{
		return reader.ReadString();
	}

	protected override void WriteKey(BinaryWriter writer, string key)
	{
		writer.Write(key);
	}

	protected override void setFieldValue(NetArray<int, NetInt> field, string key, int[] value)
	{
		field.Set(value);
	}

	protected override int[] getFieldValue(NetArray<int, NetInt> field)
	{
		return field.ToArray();
	}

	protected override int[] getFieldTargetValue(NetArray<int, NetInt> field)
	{
		return field.ToArray();
	}
}
