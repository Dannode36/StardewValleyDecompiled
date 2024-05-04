using System.IO;

namespace Netcode;

public interface INetSerializable
{
	uint DirtyTick { get; set; }

	bool Dirty { get; }

	bool NeedsTick { get; set; }

	bool ChildNeedsTick { get; set; }

	/// <summary>A name for this net field, used for troubleshooting network sync.</summary>
	string Name { get; set; }

	INetSerializable Parent { get; set; }

	INetRoot Root { get; }

	void MarkDirty();

	void MarkClean();

	bool Tick();

	void Read(BinaryReader reader, NetVersion version);

	void Write(BinaryWriter writer);

	void ReadFull(BinaryReader reader, NetVersion version);

	void WriteFull(BinaryWriter writer);
}
