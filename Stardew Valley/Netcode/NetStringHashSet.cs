using System.IO;

namespace Netcode;

public class NetStringHashSet : NetHashSet<string>
{
	public override string ReadValue(BinaryReader reader)
	{
		if (!reader.ReadBoolean())
		{
			return null;
		}
		return reader.ReadString();
	}

	public override void WriteValue(BinaryWriter writer, string value)
	{
		writer.Write(value != null);
		if (value != null)
		{
			writer.Write(value);
		}
	}
}
