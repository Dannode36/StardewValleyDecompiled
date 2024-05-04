using System;

namespace Netcode.Validation;

/// <summary>Indicates that a field of this type isn't automatically a synchronized net field, even if it implements <see cref="T:Netcode.INetSerializable" /> or <see cref="T:Netcode.INetObject`1" />.</summary>
[AttributeUsage(AttributeTargets.Class)]
public class NotImplicitNetFieldAttribute : Attribute
{
}
