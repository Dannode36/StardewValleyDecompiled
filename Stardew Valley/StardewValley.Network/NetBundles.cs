using System.IO;
using System.Linq;
using Netcode;
using StardewValley.Menus;

namespace StardewValley.Network;

public class NetBundles : NetDictionary<int, bool[], NetArray<bool, NetBool>, SerializableDictionary<int, bool[]>, NetBundles>
{
	protected override int ReadKey(BinaryReader reader)
	{
		int result = reader.ReadInt32();
		if (Game1.activeClickableMenu is JunimoNoteMenu menu)
		{
			menu.bundlesChanged = true;
		}
		return result;
	}

	protected override void WriteKey(BinaryWriter writer, int key)
	{
		writer.Write(key);
	}

	protected override void setFieldValue(NetArray<bool, NetBool> field, int key, bool[] value)
	{
		field.Set(value);
	}

	protected override bool[] getFieldValue(NetArray<bool, NetBool> field)
	{
		return field.ToArray();
	}

	protected override bool[] getFieldTargetValue(NetArray<bool, NetBool> field)
	{
		return field.ToArray();
	}
}
