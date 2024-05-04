using System.IO;
using Microsoft.Xna.Framework;

namespace Netcode;

public class NetVector2HashSet : NetHashSet<Vector2>
{
	public override Vector2 ReadValue(BinaryReader reader)
	{
		float x = reader.ReadSingle();
		float y = reader.ReadSingle();
		return new Vector2(x, y);
	}

	public override void WriteValue(BinaryWriter writer, Vector2 value)
	{
		writer.Write(value.X);
		writer.Write(value.Y);
	}
}
