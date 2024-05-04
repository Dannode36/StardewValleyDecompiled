using System.IO;

namespace Netcode;

public class NetIntHashSet : NetHashSet<int>
{
	public override int ReadValue(BinaryReader reader)
	{
		return reader.ReadInt32();
	}

	public override void WriteValue(BinaryWriter writer, int value)
	{
		writer.Write(value);
	}
}
